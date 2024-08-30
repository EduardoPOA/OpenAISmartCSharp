using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Eduardo.OpenAISmartTest.Options
{
    /// <summary>
    /// Represents a class that provides a dialog page for displaying general options.
    /// </summary>
    [ComVisible(true)]
    public class OptionPageGridGeneral : DialogPage
    {
        internal static readonly object Instance;

        //[Category("OpenAI Smart Test")]
        //[DisplayName("OpenAI Service")]
        //[Description("Select how to connect: OpenAI API or Azure OpenAI.")]
        //[DefaultValue(OpenAIService.OpenAI)]
        //[TypeConverter(typeof(EnumConverter))]
        //public OpenAIService Service { get; set; } = OpenAIService.OpenAI;

        //[Category("OpenAI Smart Test")]
        //[DisplayName("OpenAI Organization")]
        //[Description("Set the OpenAI Organization.")]
        //public string OpenAIOrganization { get; set; }

        //[Category("OpenAI Smart Test")]
        //[DisplayName("Linguagem do OpenAI")]
        //[Description("Veja sobre os modelos de linguagem \"https://platform.openai.com/docs/models/overview\" para mais detalhes.")]
        //[DefaultValue(ModelLanguageEnum.gpt35turboinstruct)]
        //[TypeConverter(typeof(EnumConverter))]
        //public ModelLanguageEnum Model { get; set; } = ModelLanguageEnum.gpt35turboinstruct;

        [Category("OpenAI Smart Test")]
        [DisplayName("API Key")]
        [Description("Insira seu API Key. Para mais informação OpenAI API, veja \"https://beta.openai.com/account/api-keys\" para mais detalhes.")]
        public string ApiKey { get; set; }

        [Category("OpenAI Smart Test")]
        [DisplayName("Selecionar framework de testes")]
        [Description("Selecione o framework para os testes unitários MStest, xUnit, NUnit")]
        [DefaultValue(SelectFrameworkTestEnum.NUnit)]
        [TypeConverter(typeof(EnumConverter))]
        public SelectFrameworkTestEnum framework { get; set; } = SelectFrameworkTestEnum.NUnit;

        [Category("OpenAI Smart Test")]
        [DisplayName("Selecionar linguagem")]
        [Description("As respostas vindo do OpenAi será traduzida em PT, EN, ES")]
        [DefaultValue(SelectLanguageEnum.en)]
        [TypeConverter(typeof(EnumConverter))]
        public SelectLanguageEnum language { get; set; } = SelectLanguageEnum.en;

        //[Category("OpenAI Smart Test")]
        //[DisplayName("Max Tokens")]
        //[Description("See \"https://help.openai.com/en/articles/4936856-what-are-tokens-and-how-to-count-them\" for more details.")]
        //[DefaultValue(2048)]
        //public int MaxTokens { get; set; } = 2048;

        //[Category("OpenAI Smart Test")]
        //[DisplayName("Temperature")]
        //[Description("What sampling temperature to use. Higher values means the model will take more risks. Try 0.9 for more creative applications, and 0 for ones with a well-defined answer.")]
        //[DefaultValue(0)]
        //[TypeConverter(typeof(DoubleConverter))]
        //public double Temperature { get; set; } = 0;

        //[Category("OpenAI Smart Test")]
        //[DisplayName("Presence Penalty")]
        //[Description("The scale of the penalty applied if a token is already present at all. Should generally be between 0 and 1, although negative numbers are allowed to encourage token reuse.")]
        //[DefaultValue(0)]
        //[TypeConverter(typeof(DoubleConverter))]
        //public double PresencePenalty { get; set; } = 0;

        //[Category("OpenAI Smart Test")]
        //[DisplayName("Frequency Penalty")]
        //[Description("The scale of the penalty for how often a token is used. Should generally be between 0 and 1, although negative numbers are allowed to encourage token reuse.")]
        //[DefaultValue(0)]
        //[TypeConverter(typeof(DoubleConverter))]
        //public double FrequencyPenalty { get; set; } = 0;

        //[Category("OpenAI Smart Test")]
        //[DisplayName("top p")]
        //[Description("An alternative to sampling with temperature, called nucleus sampling, where the model considers the results of the tokens with top_p probability mass. So 0.1 means only the tokens comprising the top 10% probability mass are considered.")]
        //[DefaultValue(0)]
        //[TypeConverter(typeof(DoubleConverter))]
        //public double TopP { get; set; } = 0;

        //[Category("OpenAI Smart Test")]
        //[DisplayName("Stop Sequences")]
        //[Description("Up to 4 sequences where the API will stop generating further tokens. The returned text will not contain the stop sequence. Separate different stop strings by a comma e.g. '},;,stop'")]
        //[DefaultValue("")]
        //public string StopSequences { get; set; } = string.Empty;

        //[Category("OpenAI Smart Test")]
        //[DisplayName("Turbo Chat Behavior")]
        //[Description("Set the behavior of the assistant.")]
        //[DefaultValue("You are a programmer assistant called OpenAI Smart Test, and your role is help developers and resolve programmer problems.")]
        //public string TurboChatBehavior { get; set; } = "You are a programmer assistant called OpenAI Smart Test, and your role is help developers and resolve programmer problems.";

        //[Category("OpenAI Smart Test")]
        //[DisplayName("Turbo Chat Model Language")]
        //[Description("Set the Turbo Chat Model Language. See \"https://platform.openai.com/docs/guides/chat\" for more details.")]
        //[DefaultValue(TurboChatModelLanguageEnum.GPT_3_5_Turbo)]
        //[TypeConverter(typeof(EnumConverter))]
        //public TurboChatModelLanguageEnum TurboChatModelLanguage { get; set; } = TurboChatModelLanguageEnum.GPT_3_5_Turbo;

        //[Category("OpenAI Smart Test")]
        //[DisplayName("Single Response")]
        //[Description("If true, the entire response will be displayed at once (less undo history but longer waiting time).")]
        //[DefaultValue(false)]
        //public bool SingleResponse { get; set; } = false;

        //[Category("OpenAI Smart Test")]
        //[DisplayName("Proxy")]
        //[Description("Connect to OpenAI through a proxy.")]
        //[DefaultValue("")]
        //public string Proxy { get; set; } = string.Empty;

        //[Category("OpenAI Smart Test")]
        //[DisplayName("Azure OpenAI Resource Name")]
        //[Description("Set Azure OpenAI resource name.")]
        //[DefaultValue("")]
        //public string AzureResourceName { get; set; } = string.Empty;

        //[Category("OpenAI Smart Test")]
        //[DisplayName("Azure Deployment ID")]
        //[Description("Set Azure OpenAI deployment id.")]
        //[DefaultValue("")]
        //public string AzureDeploymentId { get; set; } = string.Empty;

        //[Category("OpenAI Smart Test")]
        //[DisplayName("Azure Turbo Chat Deployment ID")]
        //[Description("Set Azure OpenAI deployment id specific for the Turbo Chat window. This deployment has to be parameterized with the GPT-3.5-Turbo or GPT-4 model.")]
        //[DefaultValue("")]
        //public string AzureTurboChatDeploymentId { get; set; } = string.Empty;

        //[Category("OpenAI Smart Test")]
        //[DisplayName("Azure Turbo Chat API Version")]
        //[Description("Set the Azure OpenAI API version for the Turbo Chat window used on the deployment with the GPT-3.5-Turbo or GPT-4 model.")]
        //[DefaultValue("")]
        //public string AzureTurboChatApiVersion { get; set; } = string.Empty;
    }

    /// <summary>
    /// Enum containing the different types of model languages.
    /// </summary>
    public enum ModelLanguageEnum
    {
        GPT_3_5_Turbo,
        TextDavinci003,
        TextCurie001,
        TextBabbage001,
        TextAda001
    }

    /// <summary>
    /// Enum to represent the language model used by TurboChat. 
    /// </summary>
    public enum TurboChatModelLanguageEnum
    {
        GPT_3_5_Turbo,
        GPT_3_5_Turbo_1106,
        GPT_4,
        GPT_4_32K,
        GPT_4_Turbo
    }

    /// <summary>
    /// Enum to represent the different OpenAI services available.
    /// </summary>
    public enum OpenAIService
    {
        OpenAI,
        AzureOpenAI
    }

    /// <summary>
    /// Enum to select language
    /// </summary>
    public enum SelectLanguageEnum
    {
        en,
        es,
        pt
    }

    /// <summary>
    /// Enum to select framwork to the unit tests
    /// </summary>
    public enum SelectFrameworkTestEnum
    {
        MSTest,
        xNunit,
        NUnit
    }
}