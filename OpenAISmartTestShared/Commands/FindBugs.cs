using Community.VisualStudio.Toolkit;
using Eduardo.OpenAISmartTest.Commands;
using Eduardo.OpenAISmartTest.Options;
using System;

namespace Eduardo.OpenAISmartTest
{
    [Command(PackageIds.FindBugs)]
    internal sealed class FindBugs : BaseChatGPTCommand<FindBugs>
    {
        public FindBugs()
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

            // Adiciona instrução explícita de quebra de linha
            string instruction = GetBugFindingPrompt();

            return $"{instruction}\n\n{cleanText}";
        }

        private string GetBugFindingPrompt()
        {
            return OptionsGeneral?.language switch
            {
                SelectLanguageEnum.en =>
                    @"Return ONLY analysis comments for this C# code. 

                    FORMAT RULES:
                    1. Every line MUST start with exactly '// '
                    2. Your response MUST end with TWO newlines (press Enter twice)
                    3. Do NOT include the original code
                    4. Separate comments with proper line breaks
                    
                    Analysis focus:
                    - Null reference exceptions
                    - Memory leaks
                    - Thread safety issues
                    - Security vulnerabilities
                    - Resource leaks
                    - Performance problems
                    
                    If no issues found:
                    // ✅ Code analysis complete
                    // No critical issues detected
                    [TWO NEWLINES HERE - PRESS ENTER TWICE]
                    
                    Code to analyze:",

                SelectLanguageEnum.es =>
                    @"Retorna SOLO comentarios de análisis para este código C#.

                    REGLAS DE FORMATO:
                    1. Cada línea DEBE comenzar con exactamente '// '
                    2. Tu respuesta DEBE terminar con DOS nuevas líneas (presiona Enter dos veces)
                    3. NO incluyas el código original
                    4. Separa los comentarios con saltos de línea apropiados
                    
                    Enfoque del análisis:
                    - Excepciones de referencia nula
                    - Fugas de memoria
                    - Problemas de seguridad de hilos
                    - Vulnerabilidades de seguridad
                    - Fugas de recursos
                    - Problemas de rendimiento
                    
                    Si no hay problemas:
                    // ✅ Análisis completado
                    // No se detectaron problemas críticos
                    [DOS NUEVAS LÍNEAS AQUÍ - PRESIONA ENTER DOS VECES]
                    
                    Código a analizar:",

                SelectLanguageEnum.pt =>
                    @"Retorne APENAS comentários de análise para este código C#.

                    REGRAS DE FORMATAÇÃO:
                    1. Cada linha DEVE começar com exatamente '// '
                    2. Sua resposta DEVE terminar com DUAS novas linhas (pressione Enter duas vezes)
                    3. NÃO inclua o código original
                    4. Separe os comentários com quebras de linha apropriadas
                    
                    Foco da análise:
                    - Exceções de referência nula
                    - Vazamentos de memória
                    - Problemas de segurança de threads
                    - Vulnerabilidades de segurança
                    - Vazamentos de recursos
                    - Problemas de performance
                    
                    Se não houver problemas:
                    // ✅ Análise completada
                    // Nenhum problema crítico detectado
                    [DUAS NOVAS LINHAS AQUÍ - PRESSIONE ENTER DUAS VEZES]
                    
                    Código para analisar:",

                _ =>
                    @"Return ONLY analysis comments for this C# code. 

                    FORMAT RULES:
                    1. Every line MUST start with exactly '// '
                    2. Your response MUST end with TWO newlines (press Enter twice)
                    3. Do NOT include the original code
                    4. Separate comments with proper line breaks
                    
                    Analysis focus:
                    - Null reference exceptions
                    - Memory leaks
                    - Thread safety issues
                    - Security vulnerabilities
                    - Resource leaks
                    - Performance problems
                    
                    If no issues found:
                    // ✅ Code analysis complete
                    // No critical issues detected
                    [TWO NEWLINES HERE - PRESS ENTER TWICE]
                    
                    Code to analyze:"
            };
        }
    }
}