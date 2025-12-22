using Anthropic.SDK;
using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;
using Eduardo.OpenAISmartTest.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Eduardo.OpenAISmartTest.Utils
{
    /// <summary>
    /// Static class containing methods for interacting with the Claude API.
    /// </summary>
    static class Claude
    {
        private static AnthropicClient client;
        private static string currentApiKey;
        private static ChatGPTHttpClientFactory chatGPTHttpClient;

        public static string Proxy { get; set; } = string.Empty;
        public static bool SingleResponse { get; set; } = false;
        [TypeConverter(typeof(EnumConverter))]
        public static TurboChatModelLanguageEnum TurboChatModelLanguage { get; set; } = TurboChatModelLanguageEnum.Claude_3_Sonnet;
        public static string TurboChatBehavior { get; set; } = "I am Dudu the developer";
        public static string StopSequences { get; set; } = string.Empty;
        [TypeConverter(typeof(DoubleConverter))]
        public static double TopP { get; set; } = 0;
        [TypeConverter(typeof(DoubleConverter))]
        public static double FrequencyPenalty { get; set; } = 0;
        [TypeConverter(typeof(DoubleConverter))]
        public static double PresencePenalty { get; set; } = 0;
        [TypeConverter(typeof(DoubleConverter))]
        public static double Temperature { get; set; } = 1.0;
        public static int MaxTokens { get; set; } = 2048;
        [DefaultValue(ModelLanguageEnum.Claude_3_Opus)]
        [TypeConverter(typeof(EnumConverter))]
        public static ModelLanguageEnum ModelLanguage { get; set; } = ModelLanguageEnum.Claude_3_Opus;

        /// <summary>
        /// Requests a completion from the Claude API using the given options.
        /// </summary>
        public static async Task<MessageResponse> RequestAsync(OptionPageGridGeneral options, string request)
        {
            CreateClient(options);

            var messages = new List<Message>
            {
                new Message(RoleType.User, request)
            };

            var parameters = GetRequestParameters(messages);
            return await client.Messages.GetClaudeMessageAsync(parameters);
        }

        /// <summary>
        /// Requests a completion from the Claude API using the given options.
        /// </summary>
        public static async Task<MessageResponse> RequestAsync(OptionPageGridGeneral options, string request, string[] stopSequences)
        {
            CreateClient(options);

            var messages = new List<Message>
            {
                new Message(RoleType.User, request)
            };

            var parameters = GetRequestParameters(messages, stopSequences);
            return await client.Messages.GetClaudeMessageAsync(parameters);
        }

        /// <summary>
        /// Requests a completion from the Claude API using the given options (streaming).
        /// </summary>
        public static async Task RequestAsync(OptionPageGridGeneral options, string request, Action<int, MessageResponse> resultHandler)
        {
            CreateClient(options);

            var messages = new List<Message>
            {
                new Message(RoleType.User, request)
            };

            var parameters = GetRequestParameters(messages);

            var responses = new List<MessageResponse>();
            await foreach (var res in client.Messages.StreamClaudeMessageAsync(parameters))
            {
                responses.Add(res);
                resultHandler(responses.Count - 1, res);
            }
        }

        /// <summary>
        /// Requests a completion from the Claude API using the given options (streaming).
        /// </summary>
        public static async Task RequestAsync(OptionPageGridGeneral options, string request, Action<int, MessageResponse> resultHandler, string[] stopSequences)
        {
            CreateClient(options);

            var messages = new List<Message>
            {
                new Message(RoleType.User, request)
            };

            var parameters = GetRequestParameters(messages, stopSequences);

            var responses = new List<MessageResponse>();
            await foreach (var res in client.Messages.StreamClaudeMessageAsync(parameters))
            {
                responses.Add(res);
                resultHandler(responses.Count - 1, res);
            }
        }

        /// <summary>
        /// Creates a new conversation.
        /// </summary>
        public static List<Message> CreateConversation(OptionPageGridGeneral options)
        {
            return new List<Message>();
        }

        /// <summary>
        /// Requests a chat response from the Claude API using the given options and message history.
        /// </summary>
        public static async Task<MessageResponse> RequestChatAsync(OptionPageGridGeneral options, List<Message> messages)
        {
            CreateClient(options);
            var parameters = GetRequestParameters(messages);
            return await client.Messages.GetClaudeMessageAsync(parameters);
        }

        /// <summary>
        /// Creates an AnthropicClient with the given API key.
        /// </summary>
        private static void CreateClient(OptionPageGridGeneral options)
        {
            if (client == null || currentApiKey != options.ApiKey)
            {
                currentApiKey = options.ApiKey;
                client = new AnthropicClient(options.ApiKey);
            }
        }

        /// <summary>
        /// Gets a MessageParameters object based on the given options and messages.
        /// </summary>
        private static MessageParameters GetRequestParameters(List<Message> messages, string[] stopSequences = null)
        {
            // Modelos válidos da API Anthropic
            // IMPORTANTE: Use a versão 20240620 se 20241022 não funcionar na sua conta
            string model = "claude-3-5-sonnet-20240620"; // Claude 3.5 Sonnet (versão estável de junho 2024)

            switch (ModelLanguage)
            {
                case ModelLanguageEnum.Claude_3_Sonnet:
                    // Claude 3.5 Sonnet - tente primeiro 20240620, depois 20241022 se disponível
                    model = "claude-3-5-sonnet-20240620";
                    break;
                case ModelLanguageEnum.Claude_3_Opus:
                    // Claude 3 Opus - modelo mais poderoso da família Claude 3
                    model = "claude-3-opus-20240229";
                    break;
                case ModelLanguageEnum.Claude_3_Haiku:
                    // Claude 3 Haiku - modelo mais rápido e econômico
                    model = "claude-3-haiku-20240307";
                    break;
            }

            if (stopSequences == null || stopSequences.Length == 0)
            {
                var sequences = StopSequences.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToArray();

                stopSequences = sequences.Length > 0 ? sequences : null;
            }

            var parameters = new MessageParameters
            {
                Model = model,
                MaxTokens = MaxTokens,
                Temperature = Temperature > 0 ? (decimal)Temperature : 1.0m,
                TopP = TopP > 0 ? (decimal)TopP : (decimal?)null,
                Messages = messages,
                Stream = false
            };

            // Adiciona system prompt se existir
            if (!string.IsNullOrWhiteSpace(TurboChatBehavior))
            {
                parameters.System = new List<SystemMessage>
                {
                    new SystemMessage(TurboChatBehavior)
                };
            }

            // Adiciona stop sequences se existirem
            if (stopSequences != null && stopSequences.Length > 0)
            {
                parameters.StopSequences = stopSequences.Take(4).ToArray();
            }

            return parameters;
        }
    }
}