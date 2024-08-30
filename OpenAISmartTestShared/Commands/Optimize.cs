using Community.VisualStudio.Toolkit;
using Eduardo.OpenAISmartTest.Commands;
using System;

namespace Eduardo.OpenAISmartTest
{
    [Command(PackageIds.Optimize)]
    internal sealed class Optimize : BaseChatGPTCommand<Optimize>
    {
        protected override CommandType GetCommandType(string selectedText)
        {
            return CommandType.Replace;
        }

        protected override string GetCommand(string selectedText)
        {
            return $"{OptionsCommands.Optimize}{Environment.NewLine}{Environment.NewLine}{selectedText}";
        }
    }
}
