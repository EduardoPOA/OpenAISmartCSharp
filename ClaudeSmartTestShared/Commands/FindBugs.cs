using Community.VisualStudio.Toolkit;
using Eduardo.OpenAISmartTest.Commands;
using Eduardo.OpenAISmartTest.Options;
using GTranslate.Translators;
using Nito.AsyncEx;
using System;

namespace Eduardo.OpenAISmartTest
{
    [Command(PackageIds.FindBugs)]
    internal sealed class FindBugs : BaseChatGPTCommand<FindBugs>
    {
        protected override CommandType GetCommandType(string selectedText)
        {
            return CommandType.InsertBefore;
        }

        protected override string GetCommand(string selectedText)
        {
            var language = OptionsCommands.FindBugs;

            switch (OptionsGeneral.language)
            {
                case SelectLanguageEnum.en:
                    language = OptionsCommands.FindBugs;
                    break;
                case SelectLanguageEnum.es:
                    language = OptionsCommands.FindBugsSpanish;
                    break;
                case SelectLanguageEnum.pt:
                    language = OptionsCommands.FindBugsPortuguese;
                    break;
            }
            return $"{language}{Environment.NewLine}{Environment.NewLine}{selectedText}";
        }
    }
}
