﻿using Community.VisualStudio.Toolkit;
using Eduardo.OpenAISmartTest.Utils;
using Eduardo.OpenAISmartTest;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using OpenAI_API.Completions;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Eduardo.OpenAISmartTest.Options;
using Constants = Eduardo.OpenAISmartTest.Utils.Constants;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using GTranslate.Translators;
using Nito.AsyncEx;

namespace OpenAISmartTestShared.Commands
{
    /// <summary>
    /// Command to add summary for the entire class.
    /// </summary>
    [Command(PackageIds.AddSummaryForAll)]
    internal sealed class AddSummaryForAll : BaseCommand<AddSummaryForAll>
    {
        /// <summary>
        /// Gets the OptionsGeneral property of the OpenAISmartTestPackage.
        /// </summary>
        protected OptionPageGridGeneral OptionsGeneral
        {
            get
            {
                return ((OpenAISmartTestPackage)this.Package).OptionsGeneral;
            }
        }

        /// <summary>
        /// Gets the OptionsCommands property of the OpenAISmartTestPackage.
        /// </summary>
        protected OptionPageGridCommands OptionsCommands
        {
            get
            {
                return ((OpenAISmartTestPackage)this.Package).OptionsCommands;
            }
        }

        /// <summary>
        /// Executes the command to add summaries to class members.
        /// </summary>
        /// <param name="e">The <see cref="OleMenuCmdEventArgs"/> instance containing the event data.</param>
        protected override async System.Threading.Tasks.Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            const string PROGRESS_MESSAGE = "Creating Summaries...";

            DocumentView docView = await VS.Documents.GetActiveDocumentViewAsync();

            string originalCode = string.Empty;

            int totalDeclarations = 0;

            try
            {
                if (string.IsNullOrWhiteSpace(OptionsGeneral.ApiKey))
                {
                    await VS.MessageBox.ShowAsync(Constants.EXTENSION_NAME, Constants.MESSAGE_SET_API_KEY, buttons: Microsoft.VisualStudio.Shell.Interop.OLEMSGBUTTON.OLEMSGBUTTON_OK);

                    Package.ShowOptionPage(typeof(OptionPageGridGeneral));

                    return;
                }

                if (!System.IO.Path.GetExtension(docView.FilePath).TrimStart('.').Equals("cs", StringComparison.InvariantCultureIgnoreCase))
                {
                    await VS.MessageBox.ShowAsync(Constants.EXTENSION_NAME, "This command is for C# code only.", buttons: Microsoft.VisualStudio.Shell.Interop.OLEMSGBUTTON.OLEMSGBUTTON_OK);

                    return;
                }

                await Package.JoinableTaskFactory.SwitchToMainThreadAsync();

                ITextBuffer textBuffer = docView.TextView.TextBuffer;

                originalCode = textBuffer.CurrentSnapshot.GetText();

                string code = originalCode;

                string editedCode = RemoveCurrentSummaries(code);

                docView.TextView.TextBuffer.Replace(new Span(0, code.Length), editedCode);

                code = textBuffer.CurrentSnapshot.GetText();

                editedCode = code;

                SyntaxTree tree = CSharpSyntaxTree.ParseText(code);

                SyntaxNode root = tree.GetRoot();

                totalDeclarations = root.DescendantNodes().Count(d => CheckIfMemberTypeIsValid(d)) + 1;

                if (totalDeclarations == 0)
                {
                    await VS.MessageBox.ShowAsync(Constants.EXTENSION_NAME, "Without declaration(s) to add summary.", buttons: Microsoft.VisualStudio.Shell.Interop.OLEMSGBUTTON.OLEMSGBUTTON_OK);

                    return;
                }

                int memberIndex = 1;

                await VS.StatusBar.ShowProgressAsync(PROGRESS_MESSAGE, memberIndex, totalDeclarations);

                foreach (SyntaxNode member in root.DescendantNodes().Where(d => CheckIfMemberTypeIsValid(d)))
                {
                    editedCode = await AddSummaryToClassMemberAsync(tree, member, textBuffer, editedCode);

                    memberIndex++;

                    await VS.StatusBar.ShowProgressAsync(PROGRESS_MESSAGE, memberIndex, totalDeclarations);
                }

                docView.TextView.TextBuffer.Replace(new Span(0, code.Length), editedCode);

                try
                {
                    (await VS.GetServiceAsync<DTE, DTE>()).ExecuteCommand(Constants.EDIT_DOCUMENT_COMMAND, string.Empty);
                }
                catch (Exception)
                {

                }

                await VS.StatusBar.ShowProgressAsync("Finished", totalDeclarations, totalDeclarations);
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrWhiteSpace(originalCode))
                {
                    docView.TextView.TextBuffer.Replace(new Span(0, originalCode.Length), originalCode);
                }

                await VS.StatusBar.ShowProgressAsync(ex.Message, totalDeclarations, totalDeclarations);

                await VS.MessageBox.ShowAsync(Constants.EXTENSION_NAME, ex.Message, Microsoft.VisualStudio.Shell.Interop.OLEMSGICON.OLEMSGICON_WARNING, Microsoft.VisualStudio.Shell.Interop.OLEMSGBUTTON.OLEMSGBUTTON_OK);
            }
        }

        /// <summary>
        /// Removes all summary comments from the given class code.
        /// </summary>
        /// <param name="classCode">The class code to remove summaries from.</param>
        /// <returns>The class code with all summaries removed.</returns>
        public static string RemoveCurrentSummaries(string classCode)
        {
            StringBuilder outputBuilder = new();

            bool insideSummary = false;

            string startPattern = @"^\s*///\s*<summary\b[^>]*>";

            string[] lines = classCode.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                if (insideSummary)
                {
                    if (!lines[i + 1].Contains("///"))
                    {
                        insideSummary = false;
                        continue;
                    }
                }
                else if (Regex.IsMatch(lines[i], startPattern))
                {
                    insideSummary = true;
                    continue;
                }

                if (!insideSummary)
                {
                    outputBuilder.Append(lines[i]);
                }
            }

            return outputBuilder.ToString().Trim();
        }

        /// <summary>
        /// Adds the summary to the class member.
        /// </summary>
        /// <param name="tree">The tree.</param>
        /// <param name="classMember">The class member.</param>
        /// <param name="textBuffer">The text buffer.</param>
        /// <param name="editedCode">The edited code.</param>
        /// <returns>The code edited with the summary added.</returns>
        private async System.Threading.Tasks.Task<string> AddSummaryToClassMemberAsync(SyntaxTree tree, SyntaxNode classMember, ITextBuffer textBuffer, string editedCode)
        {            
            string declarationCode;
            string summary;

            if (classMember is ClassDeclarationSyntax || classMember is InterfaceDeclarationSyntax)
            {
                int lineNumber = tree.GetLineSpan(classMember.Span).StartLinePosition.Line;
                declarationCode = textBuffer.CurrentSnapshot.GetLineFromLineNumber(lineNumber).GetText();
            }
            else
            {
                declarationCode = classMember.ToFullString().Trim();
            }
            summary = await RequestAsync(declarationCode);
            summary = TextFormat.RemoveLinesStartingWithCodeTags(summary);

            if (string.IsNullOrWhiteSpace(summary))
            {
                return editedCode;
            }

            return editedCode.Replace(declarationCode, summary + Environment.NewLine + declarationCode);
        }

        /// <summary>
        /// Requests a summary from ChatGPT for the given code.
        /// </summary>
        /// <param name="code">The code to summarize.</param>
        /// <returns>The summary of the code.</returns>
        private async Task<string> RequestAsync(string code)
        {
            string command = null;
            switch (OptionsGeneral.language)
            {
                case SelectLanguageEnum.en:
                    command = TextFormat.FormatCommandForSummary($"{OptionsCommands.AddSummary}\r\n\r\n{{0}}\r\n\r\n", code);
                    break;
                case SelectLanguageEnum.es:
                    command = TextFormat.FormatCommandForSummary($"{OptionsCommands.AddSummarySpanish}\r\n\r\n{{0}}\r\n\r\n", code);
                    break;
                case SelectLanguageEnum.pt:
                    command = TextFormat.FormatCommandForSummary($"{OptionsCommands.AddSummaryPortuguese}\r\n\r\n{{0}}\r\n\r\n", code);
                    break;
            }

            CompletionResult result = await ChatGPT.RequestAsync(OptionsGeneral, command, new[] { "public", "private", "internal" });

            string resultText = result.ToString();

            //This code checks if the string "resultText" starts with "\r\n" and if it does, it removes from the string. 
            //It will continue to do this until the string no longer starts with "\r\n". 
            while (resultText.StartsWith("\r\n"))
            {
                resultText = resultText.Substring(4);
            }

            if (OptionsGeneral.language == SelectLanguageEnum.pt || OptionsGeneral.language == SelectLanguageEnum.es)
            {
                resultText = resultText.Replace("/ <summary>", "/// <summary>");
                resultText = resultText.Replace("///// <summary>", "/// <summary>");
            }

            if (resultText.Contains("{") || resultText.Contains("}"))
            {
                return string.Empty;
            }

            return resultText;
        }

        /// <summary>
        /// Checks if the given SyntaxNode is a valid member type.
        /// </summary>
        /// <param name="member">The SyntaxNode to check.</param>
        /// <returns>True if the SyntaxNode is a valid member type, false otherwise.</returns>
        private bool CheckIfMemberTypeIsValid(SyntaxNode member)
        {
            return member is ClassDeclarationSyntax ||
                   member is InterfaceDeclarationSyntax ||
                   member is StructDeclarationSyntax ||
                   member is MethodDeclarationSyntax ||
                   member is PropertyDeclarationSyntax ||
                   member is EnumDeclarationSyntax ||
                   member is DelegateDeclarationSyntax ||
                   member is EventDeclarationSyntax;
        }
    }
}
