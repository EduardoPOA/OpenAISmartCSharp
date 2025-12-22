using Anthropic.SDK.Messaging;
using Community.VisualStudio.Toolkit;
using Eduardo.OpenAISmartTest;
using Eduardo.OpenAISmartTest.Options;
using Eduardo.OpenAISmartTest.Utils;
using EnvDTE;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Constants = Eduardo.OpenAISmartTest.Utils.Constants;
using Span = Microsoft.VisualStudio.Text.Span;

namespace OpenAISmartTestShared.Commands
{
    /// <summary>
    /// Command to add summary for members without existing summaries.
    /// </summary>
    [Command(PackageIds.AddSummaryForAll)]
    internal sealed class AddSummaryForAll : BaseCommand<AddSummaryForAll>
    {
        protected OptionPageGridGeneral OptionsGeneral
        {
            get { return ((OpenAISmartTestPackage)this.Package).OptionsGeneral; }
        }

        protected OptionPageGridCommands OptionsCommands
        {
            get { return ((OpenAISmartTestPackage)this.Package).OptionsCommands; }
        }

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            const string PROGRESS_MESSAGE = "Creating Summaries...";
            DocumentView docView = await VS.Documents.GetActiveDocumentViewAsync();
            string originalCode = string.Empty;
            int totalToProcess = 0;

            try
            {
                // Validações iniciais
                if (string.IsNullOrWhiteSpace(OptionsGeneral.ApiKey))
                {
                    await VS.MessageBox.ShowAsync(Constants.EXTENSION_NAME, Constants.MESSAGE_SET_API_KEY,
                        buttons: Microsoft.VisualStudio.Shell.Interop.OLEMSGBUTTON.OLEMSGBUTTON_OK);
                    Package.ShowOptionPage(typeof(OptionPageGridGeneral));
                    return;
                }

                if (!System.IO.Path.GetExtension(docView.FilePath).TrimStart('.').Equals("cs", StringComparison.InvariantCultureIgnoreCase))
                {
                    await VS.MessageBox.ShowAsync(Constants.EXTENSION_NAME, "This command is for C# code only.",
                        buttons: Microsoft.VisualStudio.Shell.Interop.OLEMSGBUTTON.OLEMSGBUTTON_OK);
                    return;
                }

                await Package.JoinableTaskFactory.SwitchToMainThreadAsync();

                ITextBuffer textBuffer = docView.TextView.TextBuffer;
                originalCode = textBuffer.CurrentSnapshot.GetText();

                // Analisa o código e encontra membros sem summary
                SyntaxTree tree = CSharpSyntaxTree.ParseText(originalCode);
                SyntaxNode root = tree.GetRoot();

                var membersNeedingSummary = GetMembersWithoutSummary(root);

                System.Diagnostics.Debug.WriteLine("=== ADD SUMMARY FOR ALL ===");
                System.Diagnostics.Debug.WriteLine($"Members needing summary: {membersNeedingSummary.Count}");

                totalToProcess = membersNeedingSummary.Count;

                if (totalToProcess == 0)
                {
                    await VS.MessageBox.ShowAsync(Constants.EXTENSION_NAME,
                        "All members already have summaries.",
                        buttons: Microsoft.VisualStudio.Shell.Interop.OLEMSGBUTTON.OLEMSGBUTTON_OK);
                    return;
                }

                // Processa cada membro que precisa de summary
                string editedCode = originalCode;
                int processed = 0;

                foreach (var member in membersNeedingSummary)
                {
                    processed++;
                    await VS.StatusBar.ShowProgressAsync(PROGRESS_MESSAGE, processed, totalToProcess);

                    System.Diagnostics.Debug.WriteLine($"Processing ({processed}/{totalToProcess}): {member.GetType().Name}");

                    editedCode = await AddSummaryToMemberAsync(member, editedCode);

                    // Atualiza a árvore com o código editado
                    tree = CSharpSyntaxTree.ParseText(editedCode);
                }

                // Aplica todas as mudanças de uma vez
                textBuffer.Replace(new Span(0, originalCode.Length), editedCode);

                // Tenta formatar o documento
                try
                {
                    (await VS.GetServiceAsync<DTE, DTE>()).ExecuteCommand(Constants.EDIT_DOCUMENT_COMMAND, string.Empty);
                }
                catch { }

                await VS.StatusBar.ShowProgressAsync("Summaries added successfully!", totalToProcess, totalToProcess);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"STACK: {ex.StackTrace}");

                // Restaura código original em caso de erro
                if (!string.IsNullOrWhiteSpace(originalCode))
                {
                    docView.TextView.TextBuffer.Replace(new Span(0, docView.TextView.TextBuffer.CurrentSnapshot.Length), originalCode);
                }

                await VS.StatusBar.ShowProgressAsync($"Error: {ex.Message}", totalToProcess, totalToProcess);
                await VS.MessageBox.ShowAsync(Constants.EXTENSION_NAME, ex.Message,
                    Microsoft.VisualStudio.Shell.Interop.OLEMSGICON.OLEMSGICON_WARNING,
                    Microsoft.VisualStudio.Shell.Interop.OLEMSGBUTTON.OLEMSGBUTTON_OK);
            }
        }

        /// <summary>
        /// Obtém lista de membros que não possuem summary.
        /// </summary>
        private List<SyntaxNode> GetMembersWithoutSummary(SyntaxNode root)
        {
            var result = new List<SyntaxNode>();

            // Pega todos os membros válidos do documento
            var allMembers = root.DescendantNodes()
                .Where(node => IsValidMemberType(node))
                .ToList();

            System.Diagnostics.Debug.WriteLine($"Total valid members found: {allMembers.Count}");

            foreach (var member in allMembers)
            {
                // Ignora membros que não têm parent (código solto no topo do arquivo)
                if (member.Parent == null || member.Parent is CompilationUnitSyntax)
                {
                    // Só processa se for uma declaração de tipo (class, interface, etc)
                    if (!(member is ClassDeclarationSyntax ||
                          member is InterfaceDeclarationSyntax ||
                          member is StructDeclarationSyntax ||
                          member is EnumDeclarationSyntax))
                    {
                        System.Diagnostics.Debug.WriteLine($"  - {member.GetType().Name} SKIPPED (top-level statement)");
                        continue;
                    }
                }

                if (!HasSummaryComment(member))
                {
                    result.Add(member);
                    System.Diagnostics.Debug.WriteLine($"  - {member.GetType().Name} needs summary");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"  - {member.GetType().Name} already has summary");
                }
            }

            return result;
        }

        /// <summary>
        /// Verifica se um membro já possui comentário de summary.
        /// </summary>
        private bool HasSummaryComment(SyntaxNode member)
        {
            var leadingTrivia = member.GetLeadingTrivia();

            System.Diagnostics.Debug.WriteLine($"    Checking trivia for: {member.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"    Total trivia count: {leadingTrivia.Count}");

            foreach (var trivia in leadingTrivia)
            {
                System.Diagnostics.Debug.WriteLine($"      Trivia kind: {trivia.Kind()}");

                if (trivia.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SingleLineDocumentationCommentTrivia) ||
                    trivia.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.MultiLineDocumentationCommentTrivia))
                {
                    string commentText = trivia.ToFullString();
                    System.Diagnostics.Debug.WriteLine($"      Found documentation comment: {commentText.Substring(0, Math.Min(50, commentText.Length))}...");

                    if (commentText.Contains("<summary>"))
                    {
                        System.Diagnostics.Debug.WriteLine("      ✓ HAS SUMMARY!");
                        return true;
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine("      ✗ NO SUMMARY");
            return false;
        }

        /// <summary>
        /// Verifica se o nó é um tipo de membro válido para receber summary.
        /// </summary>
        private bool IsValidMemberType(SyntaxNode node)
        {
            return node is ClassDeclarationSyntax ||
                   node is InterfaceDeclarationSyntax ||
                   node is StructDeclarationSyntax ||
                   node is MethodDeclarationSyntax ||
                   node is PropertyDeclarationSyntax ||
                   node is EnumDeclarationSyntax ||
                   node is DelegateDeclarationSyntax ||
                   node is EventDeclarationSyntax;
        }

        /// <summary>
        /// Adiciona summary a um membro específico.
        /// </summary>
        private async Task<string> AddSummaryToMemberAsync(SyntaxNode member, string currentCode)
        {
            try
            {
                // Extrai o código do membro
                string memberCode = ExtractMemberCode(member);

                System.Diagnostics.Debug.WriteLine($"Requesting summary for: {memberCode.Substring(0, Math.Min(100, memberCode.Length))}...");

                // Solicita o summary do Claude
                string summary = await RequestSummaryFromClaudeAsync(memberCode);

                if (string.IsNullOrWhiteSpace(summary))
                {
                    System.Diagnostics.Debug.WriteLine("No summary received, skipping member");
                    return currentCode;
                }

                // Processa e formata o summary
                summary = ProcessSummaryResponse(summary);

                if (string.IsNullOrWhiteSpace(summary))
                {
                    System.Diagnostics.Debug.WriteLine("Summary processing failed, skipping member");
                    return currentCode;
                }

                // Calcula indentação e insere o summary
                string indentation = GetMemberIndentation(member);
                string indentedSummary = ApplyIndentation(summary, indentation);

                // Encontra a posição do membro no código atual
                int memberPosition = FindMemberPosition(currentCode, member);

                if (memberPosition == -1)
                {
                    System.Diagnostics.Debug.WriteLine("Could not find member position, skipping");
                    return currentCode;
                }

                // Insere o summary ANTES do membro
                string result = currentCode.Insert(memberPosition, indentedSummary + Environment.NewLine);

                System.Diagnostics.Debug.WriteLine("Summary added successfully");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in AddSummaryToMemberAsync: {ex.Message}");
                return currentCode;
            }
        }

        /// <summary>
        /// Extrai o código relevante do membro para gerar o summary.
        /// </summary>
        private string ExtractMemberCode(SyntaxNode member)
        {
            // Para classes e interfaces, pega só a declaração
            if (member is ClassDeclarationSyntax classDecl)
            {
                return $"{string.Join(" ", classDecl.Modifiers)} class {classDecl.Identifier}";
            }

            if (member is InterfaceDeclarationSyntax interfaceDecl)
            {
                return $"{string.Join(" ", interfaceDecl.Modifiers)} interface {interfaceDecl.Identifier}";
            }

            // Para outros membros, pega a assinatura completa
            return member.ToFullString().Trim();
        }

        /// <summary>
        /// Solicita summary do Claude.
        /// </summary>
        private async Task<string> RequestSummaryFromClaudeAsync(string code)
        {
            string prompt = OptionsGeneral.language switch
            {
                SelectLanguageEnum.pt => $@"Escreva APENAS UM ÚNICO comentário de documentação XML para ESTE código específico.
Regras ESTRITAS:
1. Escreva SOMENTE para o código mostrado abaixo
2. NÃO invente métodos ou funcionalidades que não existem
3. Comece TODAS as linhas com ///
4. Inclua apenas <summary>, <param> e <returns> conforme necessário
5. NÃO escreva múltiplos blocos de summary
6. NÃO inclua o código, APENAS o comentário

Código:
{code}",
                SelectLanguageEnum.es => $@"Escribe SOLO UN comentario de documentación XML para ESTE código específico.
Reglas ESTRICTAS:
1. Escribe SOLAMENTE para el código mostrado abajo
2. NO inventes métodos o funcionalidades que no existen
3. Comienza TODAS las líneas con ///
4. Incluye solo <summary>, <param> y <returns> según sea necesario
5. NO escribas múltiples bloques de summary
6. NO incluyas el código, SOLO el comentario

Código:
{code}",
                _ => $@"Write ONLY ONE XML documentation comment for THIS specific code.
STRICT rules:
1. Write ONLY for the code shown below
2. Do NOT invent methods or features that don't exist
3. Start ALL lines with ///
4. Include only <summary>, <param>, and <returns> as needed
5. Do NOT write multiple summary blocks
6. Do NOT include the code, ONLY the comment

Code:
{code}"
            };

            MessageResponse result = await Claude.RequestAsync(
                OptionsGeneral,
                prompt,
                new[] { "public", "private", "internal", "protected", "class ", "namespace " }
            );

            return ExtractTextFromResponse(result);
        }

        /// <summary>
        /// Processa a resposta do Claude.
        /// </summary>
        private string ProcessSummaryResponse(string response)
        {
            if (string.IsNullOrWhiteSpace(response))
                return string.Empty;

            // Remove espaços/quebras de linha do início
            response = response.TrimStart('\r', '\n', ' ', '\t');

            // Remove markdown code fences
            response = RemoveMarkdownCodeFences(response);

            // Verifica se contém código (chaves)
            if (response.Contains("{") && response.Contains("}"))
            {
                System.Diagnostics.Debug.WriteLine("Response contains code braces, rejecting");
                return string.Empty;
            }

            // CRÍTICO: Se houver MÚLTIPLOS blocos <summary>, pega apenas o PRIMEIRO
            var lines = response.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var processedLines = new List<string>();
            int summaryCount = 0;
            bool insideSummary = false;

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                // Detecta início de um novo <summary>
                if (trimmedLine.Contains("<summary>"))
                {
                    summaryCount++;
                    insideSummary = true;

                    // Se já encontrou um summary completo, PARA
                    if (summaryCount > 1)
                    {
                        System.Diagnostics.Debug.WriteLine("Multiple summaries detected, keeping only first one");
                        break;
                    }
                }

                // Detecta fim do </summary>
                if (trimmedLine.Contains("</summary>"))
                {
                    insideSummary = false;
                }

                // Garante que todas as linhas começam com ///
                if (string.IsNullOrWhiteSpace(line))
                {
                    processedLines.Add(line);
                }
                else if (!trimmedLine.StartsWith("///"))
                {
                    processedLines.Add("/// " + trimmedLine);
                }
                else
                {
                    processedLines.Add(trimmedLine);
                }

                // Se terminou o summary e houver params/returns, continua até o fim deles
                if (!insideSummary && !trimmedLine.Contains("<param") && !trimmedLine.Contains("<returns")
                    && !trimmedLine.Contains("</param>") && !trimmedLine.Contains("</returns>")
                    && summaryCount > 0)
                {
                    // Se a próxima linha não é tag XML, terminou
                    break;
                }
            }

            return string.Join(Environment.NewLine, processedLines);
        }

        /// <summary>
        /// Remove markdown code fences.
        /// </summary>
        private string RemoveMarkdownCodeFences(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var filteredLines = new List<string>();

            foreach (var line in lines)
            {
                if (!line.Trim().StartsWith("```"))
                {
                    filteredLines.Add(line);
                }
            }

            return string.Join(Environment.NewLine, filteredLines);
        }

        /// <summary>
        /// Obtém a indentação de um membro.
        /// </summary>
        private string GetMemberIndentation(SyntaxNode member)
        {
            var fullText = member.ToFullString();
            var firstLine = fullText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)[0];

            int spaces = 0;
            foreach (char c in firstLine)
            {
                if (c == ' ')
                    spaces++;
                else if (c == '\t')
                    spaces += 4;
                else
                    break;
            }

            return new string(' ', spaces);
        }

        /// <summary>
        /// Aplica indentação a todas as linhas de um texto.
        /// </summary>
        private string ApplyIndentation(string text, string indentation)
        {
            var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            return string.Join(Environment.NewLine,
                lines.Select(line => string.IsNullOrWhiteSpace(line) ? line : indentation + line));
        }

        /// <summary>
        /// Encontra a posição de um membro no código.
        /// </summary>
        private int FindMemberPosition(string code, SyntaxNode member)
        {
            // Reconstrói a árvore do código atual
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();

            // Encontra o membro equivalente na árvore atual
            var currentMember = root.DescendantNodes()
                .FirstOrDefault(n => AreSameMember(n, member));

            if (currentMember != null)
            {
                // Pega a posição REAL do início do membro (após comentários normais)
                var span = currentMember.Span; // Usa Span ao invés de FullSpan
                return span.Start;
            }

            return -1;
        }

        /// <summary>
        /// Verifica se dois nós representam o mesmo membro.
        /// </summary>
        private bool AreSameMember(SyntaxNode node1, SyntaxNode node2)
        {
            if (node1.GetType() != node2.GetType())
                return false;

            // Compara com base no tipo e identificador
            if (node1 is ClassDeclarationSyntax class1 && node2 is ClassDeclarationSyntax class2)
                return class1.Identifier.Text == class2.Identifier.Text;

            if (node1 is MethodDeclarationSyntax method1 && node2 is MethodDeclarationSyntax method2)
                return method1.Identifier.Text == method2.Identifier.Text;

            if (node1 is PropertyDeclarationSyntax prop1 && node2 is PropertyDeclarationSyntax prop2)
                return prop1.Identifier.Text == prop2.Identifier.Text;

            // Para outros tipos, compara o texto completo
            return node1.ToString() == node2.ToString();
        }

        /// <summary>
        /// Extrai texto da resposta do Claude.
        /// </summary>
        private string ExtractTextFromResponse(MessageResponse response)
        {
            if (response?.Content == null)
                return string.Empty;

            var sb = new StringBuilder();
            foreach (var content in response.Content)
            {
                if (content is TextContent textBlock)
                {
                    sb.Append(textBlock.Text);
                }
            }

            return sb.ToString();
        }
    }
}