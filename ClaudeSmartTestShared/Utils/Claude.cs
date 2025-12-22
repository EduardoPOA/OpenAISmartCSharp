using Anthropic.SDK;
using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;
using Eduardo.OpenAISmartTest.Options;
using GTranslate.Translators;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Eduardo.OpenAISmartTest.Utils
{
    /// <summary>
    /// Static class containing methods for interacting with the Claude API.
    /// </summary>
    static class Claude
    {
        private static AnthropicClient client;
        private static ChatGPTHttpClientFactory chatGPTHttpClient; // Mantido para o proxy, se necessário.

        public static string Proxy { get; set; } = string.Empty;
        public static bool SingleResponse { get; set; } = false;
        [TypeConverter(typeof(EnumConverter))]
        public static TurboChatModelLanguageEnum TurboChatModelLanguage { get; set; } = TurboChatModelLanguageEnum.Claude_3_Sonnet;
        public static string TurboChatBehavior { get; set; } = "I am Dudu the developer";
        public static string StopSequences { get; set; } = string.Empty;
        [TypeConverter(typeof(DoubleConverter))]
        public static double TopP { get; set; } = 0;
        [TypeConverter(typeof(DoubleConverter))]
        public static double FrequencyPenalty { get; set; } = 0; // Claude API não suporta, será ignorado.
        [TypeConverter(typeof(DoubleConverter))]
        public static double PresencePenalty { get; set; } = 0; // Claude API não suporta, será ignorado.
        [TypeConverter(typeof(DoubleConverter))]
        public static double Temperature { get; set; } = 0;
        public static int MaxTokens { get; set; } = 2048;
        [DefaultValue(ModelLanguageEnum.Claude_3_Sonnet)]
        [TypeConverter(typeof(EnumConverter))]
        public static ModelLanguageEnum ModelLanguage { get; set; } = ModelLanguageEnum.Claude_3_Sonnet;

        /// <summary>
        /// Requests a completion from the Claude API using the given options.
        /// </summary>
        /// <param name="options">The options to use for the request.</param>
        /// <param name="request">The request to send to the API.</param>
        /// <returns>The completion result (simulated by MessageResponse).</returns>
        public static async Task<MessageResponse> RequestAsync(OptionPageGridGeneral options, string request)
        {
            CreateClient(options);

            var message = new Message
            {
                Role = "user",
                Content = request
            };

            var parameters = GetRequestParameters(new List<Message> { message });

            return await client.Messages.GetMessageAsync(parameters);
        }

        /// <summary>
        /// Requests a completion from the Claude API using the given options.
        /// </summary>
        /// <param name="options">The options to use for the request.</param>
        /// <param name="request">The request to send to the API.</param>
        /// <param name="stopSequences">Up to 4 sequences where the API will stop generating further tokens.</param>
        /// <returns>The completion result (simulated by MessageResponse).</returns>
        public static async Task<MessageResponse> RequestAsync(OptionPageGridGeneral options, string request, string[] stopSequences)
        {
            CreateClient(options);

            var message = new Message
            {
                Role = "user",
                Content = request
            };

            var parameters = GetRequestParameters(new List<Message> { message }, stopSequences);

            return await client.Messages.GetMessageAsync(parameters);
        }

        /// <summary>
        /// Requests a completion from the Claude API using the given options (streaming).
        /// </summary>
        /// <param name="options">The options to use for the request.</param>
        /// <param name="request">The request to send to the API.</param>
        /// <param name="resultHandler">The action to take when a chunk of the result is received.</param>
        public static async Task RequestAsync(OptionPageGridGeneral options, string request, Action<int, MessageResponse> resultHandler)
        {
            CreateClient(options);

            var message = new Message
            {
                Role = "user",
                Content = request
            };

            var parameters = GetRequestParameters(new List<Message> { message });

            await client.Messages.StreamMessageAsync(parameters, (response) =>
            {
                // O Anthropic.SDK retorna MessageResponse para streaming, mas o original usava CompletionResult.
                // A conversão de índice (int) é arbitrária, mantendo a assinatura.
                resultHandler(0, response);
            });
        }

        /// <summary>
        /// Requests a completion from the Claude API using the given options (streaming).
        /// </summary>
        /// <param name="options">The options to use for the request.</param>
        /// <param name="request">The request to send to the API.</param>
        /// <param name="resultHandler">The action to take when a chunk of the result is received.</param>
        /// <param name="stopSequences">Up to 4 sequences where the API will stop generating further tokens.</param>
        public static async Task RequestAsync(OptionPageGridGeneral options, string request, Action<int, MessageResponse> resultHandler, string[] stopSequences)
        {
            CreateClient(options);

            var message = new Message
            {
                Role = "user",
                Content = request
            };

            var parameters = GetRequestParameters(new List<Message> { message }, stopSequences);

            await client.Messages.StreamMessageAsync(parameters, (response) =>
            {
                resultHandler(0, response);
            });
        }

        /// <summary>
        /// Creates a new conversation and appends a system message with the specified TurboChatBehavior.
        /// </summary>
        /// <summary>
        /// Creates a new conversation and appends a system message with the specified TurboChatBehavior.
        /// </summary>
        /// <param name="options">The options to use for the conversation.</param>
        /// <returns>The initial list of messages with the system message.</returns>
        public static List<Message> CreateConversation(OptionPageGridGeneral options)
        {
            // A API do Claude usa uma lista de mensagens. O prompt do sistema é passado
            // no parâmetro 'system' do request, não na lista de mensagens.
            return new List<Message>();
        }

        /// <summary>
        /// Requests a chat response from the Claude API using the given options and message history.
        /// </summary>
        /// <param name="options">The options to use for the request.</param>
        /// <param name="messages">The message history.</param>
        /// <returns>The chat response (MessageResponse).</returns>
        public static async Task<MessageResponse> RequestChatAsync(OptionPageGridGeneral options, List<Message> messages)
        {
            CreateClient(options);

            var parameters = GetRequestParameters(messages);

            return await client.Messages.GetMessageAsync(parameters);
        }

        /// <summary>
        /// Creates an AnthropicClient with the given API key and proxy.
        /// </summary>
        /// <param name="options">All configurations to create the connection</param>
        private static void CreateClient(OptionPageGridGeneral options)
        {
            if (client == null || client.ApiKey != options.ApiKey)
            {
                chatGPTHttpClient = new();

                if (!string.IsNullOrWhiteSpace(Proxy))
                {
                    // O Anthropic.SDK não tem um HttpClientFactory direto como o OpenAI_API.
                    // A configuração de proxy deve ser feita no HttpClient subjacente,
                    // mas para simplificar a migração, vamos assumir que o AnthropicClient
                    // pode ser inicializado com um HttpClient que usa o proxy, se necessário.
                    // Como o Anthropic.SDK usa HttpClient internamente, a configuração
                    // de proxy via `chatGPTHttpClient` não é trivial de migrar.
                    // Para o escopo desta migração, vamos ignorar a configuração de proxy
                    // no cliente Anthropic, a menos que o usuário forneça um método de injeção.
                    // Se o proxy for essencial, o usuário precisará adaptar o AnthropicClient.
                }

                client = new AnthropicClient(options.ApiKey);
            }
        }

        /// <summary>
        /// Gets a MessageParameters object based on the given options and messages.
        /// </summary>
        /// <param name="messages">The list of messages for the conversation.</param>
        /// <returns>A MessageParameters object.</returns>
        private static MessageParameters GetRequestParameters(List<Message> messages, string[] stopSequences = null)
        {
            string model = ModelNames.Claude_3_Sonnet;

            switch (ModelLanguage)
            {
                case ModelLanguageEnum.Claude_3_Sonnet:
                    model = ModelNames.Claude_3_Sonnet;
                    break;
                case ModelLanguageEnum.Claude_3_Opus:
                    model = ModelNames.Claude_3_Opus;
                    break;
                case ModelLanguageEnum.Claude_3_Haiku:
                    model = ModelNames.Claude_3_Haiku;
                    break;
            }

            if (stopSequences == null || stopSequences.Length == 0)
            {
                stopSequences = StopSequences.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            }

            var parameters = new MessageParameters
            {
                Model = model,
                MaxTokens = MaxTokens,
                Temperature = Temperature,
                TopP = TopP,
                Messages = messages,
                System = TurboChatBehavior, // O comportamento do sistema é passado como um parâmetro 'system'
                StopSequences = stopSequences.Length > 0 ? stopSequences : null
            };

            return parameters;
        }
    }
}
