using Microsoft.VisualStudio.Shell;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Eduardo.OpenAISmartTest.Options
{
    /// <summary>
    /// Represents a class that provides a dialog page for displaying commands options.
    /// </summary>
    [ComVisible(true)]
    public class OptionPageGridCommands : DialogPage
    {
      
        [Category("OpenAI Smart Test")]
        [DisplayName("Complete")]
        [Description("Set the \"Complete\" command")]
        [DefaultValue("Please complete")]
        public string Complete { get; set; } = "Please complete";

        [Category("OpenAI Smart Test")]
        [DisplayName("Add Tests")]
        [Description("Set the \"Add Tests\" command")]
        [DefaultValue("Create unit tests with MSTest")]
        public string AddTestsMSTest { get; set; } = "Create MSTest tests";

        [Category("OpenAI Smart Test")]
        [DisplayName("Add Tests")]
        [Description("Set the \"Add Tests\" command")]
        [DefaultValue("Create unit tests with xUnit")]
        public string AddTestsxUnit { get; set; } = "Create xUnit tests";

        [Category("OpenAI Smart Test")]
        [DisplayName("Add Tests")]
        [Description("Set the \"Add Tests\" command")]
        [DefaultValue("Create unit tests with NUnit")]
        public string AddTestsNUnit { get; set; } = "Create NUnit tests";


        [Category("OpenAI Smart Test")]
        [DisplayName("Find Bugs")]
        [Description("Set the \"Find Bugs\" command")]
        [DefaultValue("Find Bugs")]
        public string FindBugs { get; set; } = "Find Bugs";

        [Category("OpenAI Smart Test")]
        [DisplayName("Find Bugs")]
        [Description("Set the \"Find Bugs\" command")]
        [DefaultValue("Find Bugs")]
        public string FindBugsSpanish { get; set; } = "FInd Bugs and written in spanish";

        [Category("OpenAI Smart Test")]
        [DisplayName("Find Bugs")]
        [Description("Set the \"Find Bugs\" command")]
        [DefaultValue("Find Bugs written in portuguese")]
        public string FindBugsPortuguese { get; set; } = "Find Bugs and written in portuguese";

        [Category("OpenAI Smart Test")]
        [DisplayName("Optimize")]
        [Description("Set the \"Optimize\" command")]
        [DefaultValue("Optimize")]
        public string Optimize { get; set; } = "Optimize";

        [Category("OpenAI Smart Test")]
        [DisplayName("Explain")]
        [Description("Set the \"Explain\" command")]
        [DefaultValue("Explain")]
        public string Explain { get; set; } = "Explain";

        [Category("OpenAI Smart Test")]
        [DisplayName("Explain portuguese")]
        [Description("Set the \"Explain\" command")]
        [DefaultValue("Explain in portuguese")]
        public string ExplainPortuguese { get; set; } = "Explain in portuguese";

        [Category("OpenAI Smart Test")]
        [DisplayName("Explain spanish")]
        [Description("Set the \"Explain\" command")]
        [DefaultValue("Explain in spanish")]
        public string ExplainSpanish { get; set; } = "Explain in spanish";

        [Category("OpenAI Smart Test")]
        [DisplayName("Add Summary")]
        [Description("Set the \"Add Summary\" command")]
        [DefaultValue("Only write a comment as C# summary format like")]
        public string AddSummary { get; set; } = "Only write a comment as C# summary format like";

        [Category("OpenAI Smart Test")]
        [DisplayName("Add Summary")]
        [Description("Set the \"Add Summary\" command")]
        [DefaultValue("Only write a comment as C# summary format translated in Spanish")]
        public string AddSummarySpanish { get; set; } = "Only write a comment as C# summary format translated in Spanish";

        [Category("OpenAI Smart Test")]
        [DisplayName("Add Summary")]
        [Description("Set the \"Add Summary\" command")]
        [DefaultValue("Only write a comment as C# summary format translated in Portuguese")]
        public string AddSummaryPortuguese { get; set; } = "Only write a comment as C# summary format translated in Portuguese";

        [Category("OpenAI Smart Test")]
        [DisplayName("Add Comments for one line")]
        [Description("Set the \"Add Comments\" command when one line was selected")]
        [DefaultValue("Comment")]
        public string AddCommentsForLine { get; set; } = "Comment. Add comment char for each comment line";

        [Category("OpenAI Smart Test")]
        [DisplayName("Add Comments for multiple lines")]
        [Description("Set the \"Add Comments\" command when multiple lines was selected")]
        [DefaultValue("Rewrite the code with comments")]
        public string AddCommentsForLines { get; set; } = "Rewrite the code with comments. Add comment char for each comment line";

        [Category("OpenAI Smart Test")]
        [DisplayName("Code it the comment")]
        [Description("Write only the code, not the explanation.")]
        [DefaultValue("Code it by use cases. Write only the code, not the explanation.")]
        public string AskAnything { get; set; } = "Code it by use cases. Write only the code, not the explanation.";

        [Category("OpenAI Smart Test")]
        [DisplayName("Custom command Before")]
        [Description("Define a custom command that will insert the response before the selected text")]
        [DefaultValue("")]
        public string CustomBefore { get; set; }

        [Category("OpenAI Smart Test")]
        [DisplayName("Custom command After")]
        [Description("Define a custom command that will insert the response after the selected text")]
        [DefaultValue("")]
        public string CustomAfter { get; set; }

        [Category("OpenAI Smart Test")]
        [DisplayName("Custom command Replace")]
        [Description("Define a custom command that will replace the selected text with the response")]
        [DefaultValue("")]
        public string CustomReplace { get; set; }
    }
}
