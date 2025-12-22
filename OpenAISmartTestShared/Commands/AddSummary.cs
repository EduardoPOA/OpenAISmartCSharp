using Community.VisualStudio.Toolkit;
using Eduardo.OpenAISmartTest.Commands;
using Eduardo.OpenAISmartTest.Options;
using Eduardo.OpenAISmartTest.Utils;
using System;

namespace Eduardo.OpenAISmartTest
{
    [Command(PackageIds.AddSummary)]
    internal sealed class AddSummary : BaseChatGPTCommand<AddSummary>
    {
        protected override CommandType GetCommandType(string selectedText)
        {
            return CommandType.InsertBefore;
        }

        protected override string GetCommand(string selectedText)
        {
            // Pega o comando correto baseado no idioma
            string summaryCommand = GetSummaryCommandByLanguage();

            // Formata o comando completo
            return TextFormat.FormatCommandForSummary(summaryCommand, selectedText);
        }

        /// <summary>
        /// Retorna o comando de summary apropriado baseado no idioma selecionado.
        /// </summary>
        private string GetSummaryCommandByLanguage()
        {
            switch (OptionsGeneral.language)
            {
                case SelectLanguageEnum.es:
                    return OptionsCommands.AddSummarySpanish;

                case SelectLanguageEnum.pt:
                    return OptionsCommands.AddSummaryPortuguese;

                case SelectLanguageEnum.en:
                default:
                    return OptionsCommands.AddSummary;
            }
        }
    }
}