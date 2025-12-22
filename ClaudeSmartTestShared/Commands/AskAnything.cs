using Community.VisualStudio.Toolkit;
using Eduardo.OpenAISmartTest.Commands;
using System;

namespace Eduardo.OpenAISmartTest
{
    [Command(PackageIds.AskAnything)]
    internal sealed class AskAnything : BaseChatGPTCommand<AskAnything>
    {
        protected override CommandType GetCommandType(string selectedText)
        {
            return CommandType.InsertAfter;
        }

        protected override string GetCommand(string selectedText)
        {
            return $"{OptionsCommands.AskAnything}{Environment.NewLine}{Environment.NewLine}{selectedText}";
        }
    }
}
