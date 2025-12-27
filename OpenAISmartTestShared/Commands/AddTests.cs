using Community.VisualStudio.Toolkit;
using Eduardo.OpenAISmartTest.Commands;
using Eduardo.OpenAISmartTest.Options;
using System;

namespace Eduardo.OpenAISmartTest
{
    [Command(PackageIds.AddTests)]
    internal sealed class AddTests : BaseChatGPTCommand<AddTests>
    {
        public AddTests()
        {
            SingleResponse = true;
        }

        protected override CommandType GetCommandType(string selectedText)
        {
            return CommandType.InsertAfter;
        }

        protected override string GetCommand(string selectedText)
        {
            string cleanText = selectedText?.Trim() ?? string.Empty;

            // Instrução clara e direta
            string instruction = "Create comprehensive unit tests for this C# code. ";

            // Adiciona informações do framework
            instruction += GetFrameworkInstruction();

            // Adiciona informações do idioma
            instruction += GetLanguageInstruction();

            // Instruções específicas
            instruction += "\n\nRequirements:\n" +
                          "1. Create a complete test class with proper namespace\n" +
                          "2. Use appropriate test attributes for the selected framework\n" +
                          "3. Test both normal scenarios and edge cases\n" +
                          "4. Include meaningful test method names\n" +
                          "5. Add comments explaining what each test does\n" +
                          "6. Mock external dependencies if needed\n" +
                          "7. Return ONLY the test code, no explanations\n" +
                          "8. Ensure tests compile and run successfully\n\n" +
                          "Code to test:\n";

            return $"{instruction}{cleanText}";
        }

        private string GetFrameworkInstruction()
        {
            return OptionsGeneral?.framework switch
            {
                SelectFrameworkTestEnum.MSTest =>
                    "Use MSTest framework. Use [TestClass] for test classes and [TestMethod] for test methods. " +
                    "Use [TestInitialize] for setup and [TestCleanup] for teardown if needed. " +
                    "Use Assert.AreEqual, Assert.IsTrue, Assert.ThrowsException for assertions.",

                SelectFrameworkTestEnum.xNunit =>
                    "Use xUnit framework. Use [Fact] for normal tests and [Theory] with [InlineData] for parameterized tests. " +
                    "No setup/teardown attributes - use constructor for setup and implement IDisposable for cleanup. " +
                    "Use Assert.Equal, Assert.True, Assert.Throws for assertions.",

                SelectFrameworkTestEnum.NUnit =>
                    "Use NUnit framework. Use [TestFixture] for test classes and [Test] for test methods. " +
                    "Use [SetUp] for setup and [TearDown] for teardown. Use [TestCase] for parameterized tests. " +
                    "Use Assert.AreEqual, Assert.IsTrue, Assert.Throws for assertions.",

                _ =>
                    "Use MSTest framework. Use [TestClass] for test classes and [TestMethod] for test methods. " +
                    "Use [TestInitialize] for setup and [TestCleanup] for teardown if needed."
            };
        }

        private string GetLanguageInstruction()
        {
            return OptionsGeneral?.language switch
            {
                SelectLanguageEnum.en =>
                    "Write tests in English. Use English names and comments.",

                SelectLanguageEnum.es =>
                    "Escribe los tests en español. Usa nombres y comentarios en español.",

                SelectLanguageEnum.pt =>
                    "Escreva os testes em português. Use nomes e comentários em português.",

                _ =>
                    "Write tests in English. Use English names and comments."
            };
        }
    }
}