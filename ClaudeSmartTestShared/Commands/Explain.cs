using Community.VisualStudio.Toolkit;
using Eduardo.OpenAISmartTest.Commands;
using Eduardo.OpenAISmartTest.Options;
using EnvDTE;
using GTranslate.Translators;
using Nito.AsyncEx;
using System;
using System.Web.Caching;

namespace Eduardo.OpenAISmartTest
{
    [Command(PackageIds.Explain)]
    internal sealed class Explain : BaseChatGPTCommand<Explain>
    {
        protected override CommandType GetCommandType(string selectedText)
        {
            return CommandType.InsertBefore;
        }

        protected override string GetCommand(string selectedText)
        {
            var language = OptionsCommands.Explain;

            switch (OptionsGeneral.language)
            {
                case SelectLanguageEnum.en:
                    language = OptionsCommands.Explain;
                    break;
                case SelectLanguageEnum.es:
                    language = OptionsCommands.ExplainSpanish;
                    break;
                case SelectLanguageEnum.pt:
                    language = OptionsCommands.ExplainPortuguese;
                    break;
            }
            return $"{language}{Environment.NewLine}{Environment.NewLine}{selectedText}";
        }     
    }
}
