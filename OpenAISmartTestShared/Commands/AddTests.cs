using Community.VisualStudio.Toolkit;
using Eduardo.OpenAISmartTest.Commands;
using Eduardo.OpenAISmartTest.Options;
using System;

namespace Eduardo.OpenAISmartTest
{
    [Command(PackageIds.AddTests)]
    internal sealed class AddTests : BaseChatGPTCommand<AddTests>
    {
        protected override CommandType GetCommandType(string selectedText)
        {
            return CommandType.InsertAfter;
        }

        protected override string GetCommand(string selectedText)
        {
            var language = OptionsCommands.Explain;

            switch (OptionsGeneral.framework)
            {
                case SelectFrameworkTestEnum.MSTest:
                    language = OptionsCommands.AddTestsMSTest;
                    break;
                case SelectFrameworkTestEnum.xNunit:
                    language = OptionsCommands.AddTestsxUnit;
                    break;
                case SelectFrameworkTestEnum.NUnit:
                    language = OptionsCommands.AddTestsNUnit;
                    break;
            }
            return $"{language}{Environment.NewLine}{Environment.NewLine}{selectedText}";
        }
    }
}
