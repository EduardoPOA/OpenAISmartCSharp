﻿using Community.VisualStudio.Toolkit;
using EnvDTE;
using Eduardo.OpenAISmartTest.Options;
using Eduardo.OpenAISmartTest.Utils;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using OpenAI_API.Completions;
using System;
using System.Windows.Input;
using Constants = Eduardo.OpenAISmartTest.Utils.Constants;
using Span = Microsoft.VisualStudio.Text.Span;
using Nito.AsyncEx;
using GTranslate.Translators;
using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;

namespace Eduardo.OpenAISmartTest.Commands
{
    /// <summary>
    /// Base abstract class for commands
    /// </summary>
    /// <typeparam name="TCommand">The type of the command.</typeparam>
    /// <seealso cref="BaseCommand&lt;&gt;" />
    internal abstract class BaseChatGPTCommand<TCommand> : BaseCommand<TCommand> where TCommand : class, new()
    {
        protected DocumentView docView;
        private string selectedText;
        private int position;
        private int positionStart;
        private int positionEnd;
        private int lineLength;
        private bool firstIteration;
        private bool responseStarted;
        private bool SingleResponse { get; set; } = false;
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
        /// Gets the type of command.
        /// </summary>
        /// <param name="selectedText">The selected text.</param>
        /// <returns>The type of command.</returns>
        protected abstract CommandType GetCommandType(string selectedText);

        /// <summary>
        /// Gets the command for the given selected text.
        /// </summary>
        /// <param name="selectedText">The selected text.</param>
        /// <returns>The command.</returns>
        protected abstract string GetCommand(string selectedText);

        /// <summary>
        /// Executes asynchronously when the command is invoked and <see cref="M:Community.VisualStudio.Toolkit.BaseCommand.Execute(System.Object,System.EventArgs)" /> isn't overridden.
        /// </summary>
        /// <param name="e"></param>
        /// <remarks>
        /// Use this method instead of <see cref="M:Community.VisualStudio.Toolkit.BaseCommand.Execute(System.Object,System.EventArgs)" /> if you're invoking any async tasks by using async/await patterns.
        /// </remarks>
        protected override async System.Threading.Tasks.Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(OptionsGeneral.ApiKey))
                {
                    await VS.MessageBox.ShowAsync(Constants.EXTENSION_NAME, Constants.MESSAGE_SET_API_KEY, buttons: Microsoft.VisualStudio.Shell.Interop.OLEMSGBUTTON.OLEMSGBUTTON_OK);

                    Package.ShowOptionPage(typeof(OptionPageGridGeneral));
                    return;
                }

                firstIteration = true;
                responseStarted = false;
                lineLength = 0;

                docView = await VS.Documents.GetActiveDocumentViewAsync();

                if (docView?.TextView == null) return;

                position = docView.TextView.Caret.Position.BufferPosition.Position;
                positionStart = docView.TextView.Selection.Start.Position.Position;
                positionEnd = docView.TextView.Selection.End.Position.Position;

                selectedText = docView.TextView.Selection.StreamSelectionSpan.GetText();

                if (string.IsNullOrWhiteSpace(selectedText))
                {
                    await VS.MessageBox.ShowAsync(Constants.EXTENSION_NAME, "Porfavor selecione o código.", buttons: Microsoft.VisualStudio.Shell.Interop.OLEMSGBUTTON.OLEMSGBUTTON_OK);

                    return;
                }

                await RequestAsync(selectedText);
            }
            catch (Exception ex)
            {
                await VS.StatusBar.ShowProgressAsync(ex.Message, 2, 2);

                await VS.MessageBox.ShowAsync(Constants.EXTENSION_NAME, ex.Message, Microsoft.VisualStudio.Shell.Interop.OLEMSGICON.OLEMSGICON_WARNING, Microsoft.VisualStudio.Shell.Interop.OLEMSGBUTTON.OLEMSGBUTTON_OK);
            }
        }

        /// <summary>
        /// Requests the specified selected text from ChatGPT
        /// </summary>
        /// <param name="selectedText">The selected text.</param>
        private async System.Threading.Tasks.Task RequestAsync(string selectedText)
        {
            string command = GetCommand(selectedText);

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                await TerminalWindowCommand.Instance.RequestToWindowAsync(command);

                return;
            }

            await VS.StatusBar.ShowProgressAsync(Constants.MESSAGE_WAITING_CHATGPT, 1, 2);

            string[] stopSequences = null;

            if (typeof(TCommand) == typeof(AddSummary))
            {
                stopSequences = new[] { "public", "private", "internal" };
            }

            if (SingleResponse)
            {
                CompletionResult result = await ChatGPT.RequestAsync(OptionsGeneral, command, stopSequences);

                ResultHandler(0, result);
            }
            else
            {
                await ChatGPT.RequestAsync(OptionsGeneral, command, ResultHandler, stopSequences);
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            try
            {
                //Some documents does not has format
                (await VS.GetServiceAsync<DTE, DTE>()).ExecuteCommand("Edit.FormatDocument", string.Empty);
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Results handler.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="result">The result.</param>
        private void ResultHandler(int index, CompletionResult result)
        {
            const int LINE_LIMIT = 160;

            try
            {
                if (firstIteration)
                {
                    _ = VS.StatusBar.ShowProgressAsync(Constants.MESSAGE_RECEIVING_CHATGPT, 2, 2);

                    CommandType commandType = GetCommandType(selectedText);

                    if (commandType == CommandType.Replace)
                    {
                        position = positionStart;

                        //Erase current code
                        _ = docView.TextBuffer?.Replace(new Span(position, docView.TextView.Selection.StreamSelectionSpan.GetText().Length), String.Empty);
                    }
                    else if (commandType == CommandType.InsertBefore)
                    {
                        position = positionStart;

                        InsertANewLine(false);
                    }
                    else
                    {
                        position = positionEnd;

                        InsertANewLine(true);
                    }

                    if (typeof(TCommand) == typeof(Explain) || typeof(TCommand) == typeof(FindBugs))
                    {
                        AddCommentChars();
                    }

                    firstIteration = false;
                }

                string resultText = result.ToString();

                if (SingleResponse)
                {
                    //This code checks if the string "resultText" starts with "\r\n" and if it does, it removes from the string. 
                    //It will continue to do this until the string no longer starts with "\r\n". 
                    while (resultText.StartsWith("\r\n"))
                    {
                        resultText = resultText.Substring(4);
                    }
                }
                else if (!responseStarted && (resultText.Equals("\n") || resultText.Equals("\r") || resultText.Equals(Environment.NewLine)))
                {
                    //Do nothing when API send only break lines on response begin
                    return;
                }

                responseStarted = true;

                if (typeof(TCommand) == typeof(AddSummary) && (resultText.Contains("{") || resultText.Contains("}")))
                {
                    return;
                }

                docView.TextBuffer?.Insert(position, resultText);

                position += resultText.Length;

                lineLength += resultText.Length;

                if (lineLength > LINE_LIMIT && (typeof(TCommand) == typeof(Explain) || typeof(TCommand) == typeof(FindBugs)))
                {
                    MoveToNextLineAndAddCommentPrefix();
                }
            }
            catch (Exception)
            {

            }
        }
        /// <summary>
        /// Inserts a new line into the document and optionally moves the position to the start of the next line.
        /// </summary>
        /// <param name="moveToNextLine">Indicates whether the position should be moved to the start of the next line.</param> 
        private void InsertANewLine(bool moveToNextLine)
        {
            ITextSnapshot textSnapshot = docView.TextBuffer?.Insert(position, Environment.NewLine);

            // Get the next line
            ITextSnapshotLine nextLine = textSnapshot.GetLineFromLineNumber(textSnapshot.GetLineNumberFromPosition(position) + 1);

            if (moveToNextLine)
            {
                // Get the position of the first character on the next line
                position = nextLine.Start.Position;
            }
        }

        /// <summary>
        /// Inserts a new line and adds a comment prefix to the next line.
        /// </summary>
        private void MoveToNextLineAndAddCommentPrefix()
        {
            lineLength = 0;

            InsertANewLine(true);
            AddCommentChars();
        }

        /// <summary>
        /// Inserts the comment characters for the current document into the text buffer at the given position.
        /// </summary>
        private void AddCommentChars()
        {
            string commentChars = TextFormat.GetCommentChars(docView.FilePath);

            docView.TextBuffer?.Insert(position, commentChars);
            position += commentChars.Length;
        }
    }

    /// <summary>
    /// Enum to represent the different types of commands that can be used. 
    /// </summary>
    enum CommandType
    {
        Replace,
        InsertBefore,
        InsertAfter
    }
}
