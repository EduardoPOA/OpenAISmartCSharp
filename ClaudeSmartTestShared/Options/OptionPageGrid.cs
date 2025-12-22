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

        

        

        [Category("Claude Smart Test")]
        [DisplayName("Modelo Claude")]
        [Description("Selecione o modelo Claude a ser usado.")]
        [DefaultValue(ModelLanguageEnum.Claude_3_Sonnet)]
        [TypeConverter(typeof(EnumConverter))]
        public ModelLanguageEnum Model { get; set; } = ModelLanguageEnum.Claude_3_Sonnet;

        [Category("Claude Smart Test")]
        [DisplayName("API Key")]
        [Description("Insira seu API Key. Para mais informação OpenAI API, veja \"https://beta.openai.com/account/api-keys\" para mais detalhes.")]
        public string ApiKey { get; set; }

        [Category("Claude Smart Test")]
        [DisplayName("Selecionar framework de testes")]
        [Description("Selecione o framework para os testes unitários MStest, xUnit, NUnit")]
        [DefaultValue(SelectFrameworkTestEnum.NUnit)]
        [TypeConverter(typeof(EnumConverter))]
        public SelectFrameworkTestEnum framework { get; set; } = SelectFrameworkTestEnum.NUnit;

        [Category("Claude Smart Test")]
        [DisplayName("Selecionar linguagem")]
        [Description("As respostas vindo do OpenAi será traduzida em PT, EN, ES")]
        [DefaultValue(SelectLanguageEnum.en)]
        [TypeConverter(typeof(EnumConverter))]
        public SelectLanguageEnum language { get; set; } = SelectLanguageEnum.en;

        [Category("Claude Smart Test")]
        [DisplayName("Max Tokens")]
        [Description("O número máximo de tokens a serem gerados.")]
        [DefaultValue(2048)]
        public int MaxTokens { get; set; } = 2048;

        [Category("Claude Smart Test")]
        [DisplayName("Temperature")]
        [Description("Qual temperatura de amostragem usar. Valores mais altos significam que o modelo assumirá mais riscos.")]
        [DefaultValue(0)]
        [TypeConverter(typeof(DoubleConverter))]
        public double Temperature { get; set; } = 0;

        

        

        [Category("Claude Smart Test")]
        [DisplayName("Top P")]
        [Description("A alternativa à amostragem com temperatura, onde o modelo considera os resultados dos tokens com massa de probabilidade top_p.")]
        [DefaultValue(0)]
        [TypeConverter(typeof(DoubleConverter))]
        public double TopP { get; set; } = 0;

        [Category("Claude Smart Test")]
        [DisplayName("Stop Sequences")]
        [Description("Sequências onde a API irá parar de gerar tokens. O texto retornado não conterá a sequência de parada. Separe diferentes strings de parada por uma vírgula e.g. '},;,stop'")]
        [DefaultValue("")]
        public string StopSequences { get; set; } = string.Empty;

        [Category("Claude Smart Test")]
        [DisplayName("Comportamento do Chat")]
        [Description("Defina o prompt do sistema (comportamento) do assistente Claude.")]
        [DefaultValue("You are a programmer assistant called Claude Smart Test, and your role is help developers and resolve programmer problems.")]
        public string TurboChatBehavior { get; set; } = "You are a programmer assistant called Claude Smart Test, and your role is help developers and resolve programmer problems.";

        [Category("Claude Smart Test")]
        [DisplayName("Modelo de Chat Claude")]
        [Description("Defina o modelo Claude a ser usado para o chat.")]
        [DefaultValue(TurboChatModelLanguageEnum.Claude_3_Sonnet)]
        [TypeConverter(typeof(EnumConverter))]
        public TurboChatModelLanguageEnum TurboChatModelLanguage { get; set; } = TurboChatModelLanguageEnum.Claude_3_Sonnet;

        [Category("Claude Smart Test")]
        [DisplayName("Resposta Única")]
        [Description("Se verdadeiro, a resposta inteira será exibida de uma vez (menos histórico de desfazer, mas maior tempo de espera).")]
        [DefaultValue(false)]
        public bool SingleResponse { get; set; } = false;

        [Category("Claude Smart Test")]
        [DisplayName("Proxy")]
        [Description("Conecte-se ao Claude através de um proxy.")]
        [DefaultValue("")]
        public string Proxy { get; set; } = string.Empty;

        

        

        

        
    }

    /// <summary>
    /// Enum containing the different types of model languages.
    /// </summary>
    public enum ModelLanguageEnum
    {
        Claude_3_Opus,
        Claude_3_Sonnet,
        Claude_3_Haiku
    }

    /// <summary>
    /// Enum to represent the language model used by TurboChat. 
    /// </summary>
    public enum TurboChatModelLanguageEnum
    {
        Claude_3_Opus,
        Claude_3_Sonnet,
        Claude_3_Haiku
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