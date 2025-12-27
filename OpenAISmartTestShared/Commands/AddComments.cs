using Community.VisualStudio.Toolkit;
using Eduardo.OpenAISmartTest.Commands;
using Eduardo.OpenAISmartTest.Options;
using System;

namespace Eduardo.OpenAISmartTest
{
    [Command(PackageIds.AddComments)]
    internal sealed class AddComments : BaseChatGPTCommand<AddComments>
    {
        public AddComments()
        {
            SingleResponse = true;
        }

        protected override CommandType GetCommandType(string selectedText)
        {
            return CommandType.Replace;
        }

        protected override string GetCommand(string selectedText)
        {
            string cleanText = selectedText?.Trim() ?? string.Empty;

            // Instrução baseada na linguagem configurada
            string instruction = OptionsGeneral?.language switch
            {
                SelectLanguageEnum.en => "Add English comments above code lines. Use // on separate lines. Keep original code. Return only commented code:",
                SelectLanguageEnum.es => "Agrega comentarios en español arriba del código. Usa // en líneas separadas. Mantén código original. Retorna solo código comentado:",
                SelectLanguageEnum.pt => "Adicione comentários em português acima do código. Use // em linhas separadas. Mantenha código original. Retorne apenas código comentado:",
                _ => "Add Portuguese comments above code lines. Use // on separate lines. Keep original code. Return only commented code:"
            };

            return $"{instruction}{Environment.NewLine}{Environment.NewLine}{cleanText}";
        }
    }
}