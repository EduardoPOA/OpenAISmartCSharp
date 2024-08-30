using Community.VisualStudio.Toolkit;
using Eduardo.OpenAISmartTest.Utils;

namespace Eduardo.OpenAISmartTest.Commands
{
    [Command(PackageIds.Complete)]
    internal sealed class Complete : BaseChatGPTCommand<Complete>
    {
        protected override CommandType GetCommandType(string selectedText)
        {
            return CommandType.InsertAfter;
        }

        protected override string GetCommand(string selectedText)
        {
            return TextFormat.FormatForCompleteCommand(OptionsCommands.Complete, selectedText, docView.FilePath);
        }
    }
}
