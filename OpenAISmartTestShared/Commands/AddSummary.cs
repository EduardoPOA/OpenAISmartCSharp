using Community.VisualStudio.Toolkit;
using Eduardo.OpenAISmartTest.Commands;
using Eduardo.OpenAISmartTest.Options;
using System;

namespace Eduardo.OpenAISmartTest
{
    [Command(PackageIds.AddSummary)]
    internal sealed class AddSummary : BaseChatGPTCommand<AddSummary>
    {
        public AddSummary()
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

            // Comando baseado na linguagem configurada
            string instruction = OptionsGeneral?.language switch
            {
                SelectLanguageEnum.en => "Add English XML documentation summary for this code. Use triple slash /// format. Be concise and descriptive. Return only the summary documentation:",
                SelectLanguageEnum.es => "Agrega documentación XML en español para este código. Usa formato triple barra ///. Sé conciso y descriptivo. Retorna solo la documentación:",
                SelectLanguageEnum.pt => "Adicione documentação XML em português para este código. Use formato de três barras ///. Seja conciso e descritivo. Retorne apenas a documentação:",
                _ => "Add Portuguese XML documentation summary for this code. Use triple slash /// format. Be concise and descriptive. Return only the summary documentation:"
            };

            return $"{instruction}{Environment.NewLine}{Environment.NewLine}{cleanText}";
        }
    }
}