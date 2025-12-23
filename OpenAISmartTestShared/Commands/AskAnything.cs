using Community.VisualStudio.Toolkit;
using Eduardo.OpenAISmartTest.Commands;
using System;

namespace Eduardo.OpenAISmartTest
{
    [Command(PackageIds.AskAnything)]
    internal sealed class AskAnything : BaseChatGPTCommand<AskAnything>
    {
        public AskAnything()
        {
            // Para AskAnything, usamos SingleResponse = true para receber a resposta completa de uma vez
            SingleResponse = true;
        }

        protected override CommandType GetCommandType(string selectedText)
        {
            return CommandType.InsertAfter;
        }

        protected override string GetCommand(string selectedText)
        {
            if (string.IsNullOrWhiteSpace(selectedText))
            {
                return OptionsCommands.AskAnything;
            }

            // Verifica se o texto é composto apenas de comentários
            if (IsOnlyComments(selectedText))
            {
                // Se for apenas comentários, extrai a descrição dos comentários
                string extractedDescription = ExtractDescriptionFromComments(selectedText);

                if (!string.IsNullOrWhiteSpace(extractedDescription))
                {
                    return $"{OptionsCommands.AskAnything}{Environment.NewLine}{Environment.NewLine}{extractedDescription}";
                }

                // Se não conseguiu extrair descrição, mantém o texto original
                return $"{OptionsCommands.AskAnything}{Environment.NewLine}{Environment.NewLine}{selectedText}";
            }

            // Pré-processamento do texto selecionado (para textos mistos)
            string processedText = PreprocessSelectedText(selectedText);

            return $"{OptionsCommands.AskAnything}{Environment.NewLine}{Environment.NewLine}{processedText}";
        }

        private bool IsOnlyComments(string text)
        {
            // Remove espaços em branco
            string trimmed = text.Trim();

            if (string.IsNullOrEmpty(trimmed))
                return true;

            // Verifica se todas as linhas são comentários
            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            int commentLines = 0;
            int totalLines = lines.Length;

            foreach (var line in lines)
            {
                string trimmedLine = line.Trim();

                // Verifica se é comentário XML (///)
                if (trimmedLine.StartsWith("///"))
                    commentLines++;
                // Verifica se é comentário de linha (//)
                else if (trimmedLine.StartsWith("//"))
                    commentLines++;
                // Verifica se é início de comentário de bloco (/*)
                else if (trimmedLine.StartsWith("/*"))
                    commentLines++;
                // Verifica se linha está vazia ou só com espaços
                else if (string.IsNullOrWhiteSpace(trimmedLine))
                    commentLines++; // Linhas vazias contam como "não código"
            }

            // Se todas as linhas são comentários ou vazias
            return commentLines == totalLines;
        }

        private string ExtractDescriptionFromComments(string text)
        {
            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var descriptionLines = new System.Collections.Generic.List<string>();

            foreach (var line in lines)
            {
                string trimmedLine = line.Trim();

                // Remove marcadores de comentário
                if (trimmedLine.StartsWith("///"))
                {
                    string content = trimmedLine.Substring(3).Trim();
                    if (!string.IsNullOrWhiteSpace(content))
                        descriptionLines.Add(content);
                }
                else if (trimmedLine.StartsWith("//"))
                {
                    string content = trimmedLine.Substring(2).Trim();
                    if (!string.IsNullOrWhiteSpace(content))
                        descriptionLines.Add(content);
                }
                else if (trimmedLine.StartsWith("/*"))
                {
                    // Remove /* e */
                    string content = trimmedLine.TrimStart('/').TrimStart('*').Trim();
                    if (content.EndsWith("*/"))
                        content = content.Substring(0, content.Length - 2).Trim();
                    if (!string.IsNullOrWhiteSpace(content))
                        descriptionLines.Add(content);
                }
            }

            return string.Join(" ", descriptionLines);
        }

        private string PreprocessSelectedText(string text)
        {
            // Se o texto já contém a instrução "Code it by use cases", pode ser um loop
            if (text.Contains("Code it by use cases"))
            {
                // Tenta extrair apenas a parte da solicitação real
                int index = text.IndexOf("Code it by use cases", StringComparison.OrdinalIgnoreCase);
                if (index > 0)
                {
                    string beforeInstruction = text.Substring(0, index).Trim();
                    if (!string.IsNullOrWhiteSpace(beforeInstruction))
                    {
                        return beforeInstruction;
                    }
                }
            }

            // Apenas remove espaços extras no início/fim
            text = text.Trim();

            // Se após o trim o texto ficou vazio, retorna um placeholder
            if (string.IsNullOrWhiteSpace(text))
            {
                return "Implement the requested functionality";
            }

            return text;
        }
    }
}