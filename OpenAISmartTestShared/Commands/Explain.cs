using Community.VisualStudio.Toolkit;
using Eduardo.OpenAISmartTest.Commands;
using Eduardo.OpenAISmartTest.Options;
using System;

namespace Eduardo.OpenAISmartTest
{
    [Command(PackageIds.Explain)]
    internal sealed class Explain : BaseChatGPTCommand<Explain>
    {
        public Explain()
        {
            SingleResponse = true;
        }

        protected override CommandType GetCommandType(string selectedText)
        {
            return CommandType.InsertBefore;
        }

        protected override string GetCommand(string selectedText)
        {
            string cleanText = selectedText?.Trim() ?? string.Empty;

            // Comando MUITO específico e restritivo
            string instruction = OptionsGeneral?.language switch
            {
                SelectLanguageEnum.en => "// Explain in 3 short lines max:\n// Line1\n// Line2\n// Line3\nCode:",
                SelectLanguageEnum.es => "// Explica en 3 líneas cortas max:\n// Línea1\n// Línea2\n// Línea3\nCódigo:",
                SelectLanguageEnum.pt => "// Explique em 3 linhas curtas max:\n// Linha1\n// Linha2\n// Linha3\nCódigo:",
                _ => "// Explain in 3 short lines max:\n// Line1\n// Line2\n// Line3\nCode:"
            };

            return $"{instruction}\n{cleanText}";
        }
    }
}