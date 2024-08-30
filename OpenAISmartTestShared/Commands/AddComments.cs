using Community.VisualStudio.Toolkit;
using Eduardo.OpenAISmartTest.Commands;
using Eduardo.OpenAISmartTest.Options;
using GTranslate.Translators;
using Nito.AsyncEx;
using System;

namespace Eduardo.OpenAISmartTest
{
    [Command(PackageIds.AddComments)]
    internal sealed class AddComments : BaseChatGPTCommand<AddComments>
    {
        protected override CommandType GetCommandType(string selectedText)
        {
            if (CodeContainsMultipleLines(selectedText))
            {
                return CommandType.Replace;
            }
            return CommandType.InsertBefore;
        }

        protected override string GetCommand(string selectedText)
        {
            if (selectedText.Contains(Environment.NewLine))
            {
                return $"{GetTranslatorLanguage(OptionsCommands.AddCommentsForLines)}{Environment.NewLine}{Environment.NewLine}{selectedText}";
            }
            return $"{GetTranslatorLanguage(OptionsCommands.AddCommentsForLine)}{Environment.NewLine}{Environment.NewLine}{selectedText}";
        }

        private bool CodeContainsMultipleLines(string code)
        {
            return code.Contains("\r\n") || code.Contains("\n") || code.Contains("\r");
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
                        return response = Convert.ToString(result).Replace("</code>","").Trim();
                    });
                case SelectLanguageEnum.es:
                    return AsyncContext.Run(async () =>
                    {
                        var translator = new BingTranslator();
                        var result = await translator.TranslateAsync(phrase, "es");
                        return response = Convert.ToString(result).Replace("</code>", "").Trim();
                    });
                case SelectLanguageEnum.pt:
                    return AsyncContext.Run(async () =>
                    {
                        var translator = new BingTranslator();
                        var result = await translator.TranslateAsync(phrase, "pt");
                        return response = Convert.ToString(result).Replace("</code>", "").Trim();
                    });

            }
            return response;
        }
    }
}
