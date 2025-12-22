using Community.VisualStudio.Toolkit;
using Eduardo.OpenAISmartTest.Commands;
using Eduardo.OpenAISmartTest.Options;
using Eduardo.OpenAISmartTest.Utils;
using GTranslate.Translators;
using Nito.AsyncEx;
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
            return TextFormat.FormatCommandForSummary($"{OptionsCommands.AddSummary}\r\n\r\n{{0}}\r\n\r\n", GetTranslatorLanguage(selectedText));
        }

        private string GetTranslatorLanguage(string phrase)
        {
            string response = null;
            switch (OptionsGeneral.language)
            {
                case SelectLanguageEnum.en:
                    return AsyncContext.Run(async () =>
                    {
                        var translator = new BingTranslator();
                        var result = await translator.TranslateAsync(phrase, "en");
                        return response = Convert.ToString(result);
                    });
                case SelectLanguageEnum.es:
                    return AsyncContext.Run(async () =>
                    {
                        var translator = new BingTranslator();
                        var result = await translator.TranslateAsync(phrase, "es");
                        return response = Convert.ToString(result);
                    });
                case SelectLanguageEnum.pt:
                    return AsyncContext.Run(async () =>
                    {
                        var translator = new BingTranslator();
                        var result = await translator.TranslateAsync(phrase, "pt");
                        return response = Convert.ToString(result);
                    });
            }
            return response;
        }
    }
}
