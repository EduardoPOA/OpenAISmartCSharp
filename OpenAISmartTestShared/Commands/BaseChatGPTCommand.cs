using Community.VisualStudio.Toolkit;
using EnvDTE;
using Eduardo.OpenAISmartTest.Options;
using Eduardo.OpenAISmartTest.Utils;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Anthropic.SDK.Messaging;
using System;
using System.Linq;
using System.Windows.Input;
using Constants = Eduardo.OpenAISmartTest.Utils.Constants;
using Span = Microsoft.VisualStudio.Text.Span;

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
        public bool SingleResponse { get; set; } = false;

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
                    await VS.MessageBox.ShowAsync(Constants.EXTENSION_NAME, "Por favor selecione o código.", buttons: Microsoft.VisualStudio.Shell.Interop.OLEMSGBUTTON.OLEMSGBUTTON_OK);

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
        /// Requests the specified selected text from Claude
        /// </summary>
        /// <param name="selectedText">The selected text.</param>
        private async System.Threading.Tasks.Task RequestAsync(string selectedText)
        {
            string command = GetCommand(selectedText);

            System.Diagnostics.Debug.WriteLine("=== DEBUG RequestAsync ===");
            System.Diagnostics.Debug.WriteLine($"Command length: {command.Length}");
            System.Diagnostics.Debug.WriteLine($"Command: '{command}'");
            System.Diagnostics.Debug.WriteLine($"SingleResponse: {SingleResponse}");

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                System.Diagnostics.Debug.WriteLine("Shift key pressed, sending to terminal");
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
                System.Diagnostics.Debug.WriteLine("Using SingleResponse mode");
                MessageResponse result = await Claude.RequestAsync(OptionsGeneral, command, stopSequences);

                ResultHandler(0, result);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Using streaming mode");
                await Claude.RequestAsync(OptionsGeneral, command, ResultHandler, stopSequences);
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            try
            {
                //Some documents does not have format
                (await VS.GetServiceAsync<DTE, DTE>()).ExecuteCommand("Edit.FormatDocument", string.Empty);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error formatting document: {ex.Message}");
            }

            System.Diagnostics.Debug.WriteLine("=== END DEBUG RequestAsync ===");
        }
        /// <summary>
        /// Results handler for Claude API responses.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="result">The result from Claude API.</param>
        private void ResultHandler(int index, MessageResponse result)
        {
            const int LINE_LIMIT = 160;

            try
            {
                System.Diagnostics.Debug.WriteLine("=== DEBUG ResultHandler ===");
                System.Diagnostics.Debug.WriteLine($"ResultHandler called - index: {index}");
                System.Diagnostics.Debug.WriteLine($"Result is null: {result == null}");

                if (firstIteration)
                {
                    _ = VS.StatusBar.ShowProgressAsync(Constants.MESSAGE_RECEIVING_CHATGPT, 2, 2);

                    CommandType commandType = GetCommandType(selectedText);

                    System.Diagnostics.Debug.WriteLine($"CommandType: {commandType}");
                    System.Diagnostics.Debug.WriteLine($"Position: {position}, PositionStart: {positionStart}, PositionEnd: {positionEnd}");

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

                // Extrai o texto da resposta do Claude
                string resultText = ExtractTextFromResponse(result);

                System.Diagnostics.Debug.WriteLine($"Result text length: {resultText?.Length}");
                System.Diagnostics.Debug.WriteLine($"Result text: '{resultText}'");

                if (string.IsNullOrEmpty(resultText))
                {
                    System.Diagnostics.Debug.WriteLine("Result text is null or empty");
                    System.Diagnostics.Debug.WriteLine("=== END DEBUG ResultHandler ===");
                    return;
                }

                if (SingleResponse)
                {
                    //This code checks if the string "resultText" starts with "\r\n" and if it does, it removes from the string. 
                    //It will continue to do this until the string no longer starts with "\r\n". 
                    while (resultText.StartsWith("\r\n") || resultText.StartsWith("\n"))
                    {
                        resultText = resultText.TrimStart('\r', '\n');
                    }
                }
                else if (!responseStarted && (resultText.Equals("\n") || resultText.Equals("\r") || resultText.Equals(Environment.NewLine)))
                {
                    //Do nothing when API sends only break lines on response begin
                    System.Diagnostics.Debug.WriteLine("Skipping initial break lines");
                    System.Diagnostics.Debug.WriteLine("=== END DEBUG ResultHandler ===");
                    return;
                }

                responseStarted = true;

                if (typeof(TCommand) == typeof(AddSummary) && (resultText.Contains("{") || resultText.Contains("}")))
                {
                    System.Diagnostics.Debug.WriteLine("Skipping AddSummary response with braces");
                    System.Diagnostics.Debug.WriteLine("=== END DEBUG ResultHandler ===");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"Inserting text at position: {position}");
                docView.TextBuffer?.Insert(position, resultText);

                position += resultText.Length;

                lineLength += resultText.Length;

                if (lineLength > LINE_LIMIT && (typeof(TCommand) == typeof(Explain) || typeof(TCommand) == typeof(FindBugs)))
                {
                    MoveToNextLineAndAddCommentPrefix();
                }

                System.Diagnostics.Debug.WriteLine("=== END DEBUG ResultHandler ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EXCEPTION in ResultHandler: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
        /// <summary>
        /// Extracts text content from Claude MessageResponse.
        /// </summary>
        /// <param name="response">The Claude API response.</param>
        /// <returns>The extracted text content.</returns>
        private string ExtractTextFromResponse(MessageResponse response)
        {
            System.Diagnostics.Debug.WriteLine("=== DEBUG ExtractTextFromResponse ===");
            System.Diagnostics.Debug.WriteLine($"Response is null: {response == null}");
            System.Diagnostics.Debug.WriteLine($"Response.Content is null: {response?.Content == null}");

            if (response?.Content == null)
            {
                System.Diagnostics.Debug.WriteLine("=== END DEBUG ExtractTextFromResponse (empty) ===");
                return string.Empty;
            }

            // Claude retorna Content como lista de ContentBase
            // Filtra apenas TextContent e concatena
            var textContent = new System.Text.StringBuilder();

            System.Diagnostics.Debug.WriteLine($"Content count: {response.Content.Count}");

            foreach (var content in response.Content)
            {
                System.Diagnostics.Debug.WriteLine($"Content type: {content.GetType().Name}");

                if (content is TextContent textBlock)
                {
                    System.Diagnostics.Debug.WriteLine($"TextContent text length: {textBlock.Text?.Length}");
                    System.Diagnostics.Debug.WriteLine($"TextContent preview: '{textBlock.Text?.Substring(0, Math.Min(100, textBlock.Text?.Length ?? 0))}'");
                    textContent.Append(textBlock.Text);
                }
            }

            string result = textContent.ToString();
            System.Diagnostics.Debug.WriteLine($"Final extracted text length: {result.Length}");
            System.Diagnostics.Debug.WriteLine($"Final extracted text: '{result}'");
            System.Diagnostics.Debug.WriteLine("=== END DEBUG ExtractTextFromResponse ===");

            return result;
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