using Eduardo.OpenAISmartTest.Options;
using GTranslate.Translators;
using Nito.AsyncEx;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Completions;
using OpenAI_API.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Eduardo.OpenAISmartTest.Utils
{
    /// <summary>
    /// Static class containing methods for interacting with the ChatGPT API.
    /// </summary>
    static class ChatGPT
    {
        private static OpenAIAPI api;
        private static OpenAIAPI apiForAzureTurboChat;
        private static ChatGPTHttpClientFactory chatGPTHttpClient;
        public static string AzureDeploymentId { get; set; } = string.Empty;
        public static string AzureTurboChatDeploymentId { get; set; } = string.Empty;
        public static string AzureTurboChatApiVersion { get; set; } = string.Empty;
        public static string AzureResourceName { get; set; } = string.Empty;
        public static string Proxy { get; set; } = string.Empty;
        public static bool SingleResponse { get; set; } = false;
        [TypeConverter(typeof(EnumConverter))]
        public static TurboChatModelLanguageEnum TurboChatModelLanguage { get; set; } = TurboChatModelLanguageEnum.GPT_3_5_Turbo;
        public static string TurboChatBehavior { get; set; } = "I am Dudu the developer";
        public static string StopSequences { get; set; } = string.Empty;
        [TypeConverter(typeof(DoubleConverter))]
        public static double TopP { get; set; } = 0;
        [TypeConverter(typeof(DoubleConverter))]
        public static double FrequencyPenalty { get; set; } = 0;
        [TypeConverter(typeof(DoubleConverter))]
        public static double PresencePenalty { get; set; } = 0;
        [TypeConverter(typeof(DoubleConverter))]
        public static double Temperature { get; set; } = 0;
        public static int MaxTokens { get; set; } = 2048;
        public static string OpenAIOrganization { get; set; }
        [TypeConverter(typeof(EnumConverter))]
        public static OpenAIService Service { get; set; } = OpenAIService.OpenAI;
        [DefaultValue(ModelLanguageEnum.GPT_3_5_Turbo)]
        [TypeConverter(typeof(EnumConverter))]
        public static ModelLanguageEnum ModelLanguage { get; set; } = ModelLanguageEnum.GPT_3_5_Turbo;

        /// <summary>
        /// Requests a completion from the OpenAI API using the given options.
        /// </summary>
        /// <param name="options">The options to use for the request.</param>
        /// <param name="request">The request to send to the API.</param>
        /// <returns>The completion result.</returns>
        public static async Task<CompletionResult> RequestAsync(OptionPageGridGeneral options, string request)
        {
            CreateApiHandler(options);

            return await api.Completions.CreateCompletionAsync(GetRequest(options, request));
        }

        /// <summary>
        /// Requests a completion from the OpenAI API using the given options.
        /// </summary>
        /// <param name="options">The options to use for the request.</param>
        /// <param name="request">The request to send to the API.</param>
        /// <param name="stopSequences">Up to 4 sequences where the API will stop generating further tokens.</param>
        /// <returns>The completion result.</returns>
        public static async Task<CompletionResult> RequestAsync(OptionPageGridGeneral options, string request, string[] stopSequences)
        {
            CreateApiHandler(options);

            return await api.Completions.CreateCompletionAsync(GetRequest(options, request, stopSequences));
        }

        /// <summary>
        /// Requests a completion from the OpenAI API using the given options.
        /// </summary>
        /// <param name="options">The options to use for the request.</param>
        /// <param name="request">The request to send to the API.</param>
        /// <param name="resultHandler">The action to take when the result is received.</param>
        public static async Task RequestAsync(OptionPageGridGeneral options, string request, Action<int, CompletionResult> resultHandler)
        {
            CreateApiHandler(options);

            await api.Completions.StreamCompletionAsync(GetRequest(options, request), resultHandler);
        }

        /// <summary>
        /// Requests a completion from the OpenAI API using the given options.
        /// </summary>
        /// <param name="options">The options to use for the request.</param>
        /// <param name="request">The request to send to the API.</param>
        /// <param name="resultHandler">The action to take when the result is received.</param>
        /// <param name="stopSequences">Up to 4 sequences where the API will stop generating further tokens.</param>
        public static async Task RequestAsync(OptionPageGridGeneral options, string request, Action<int, CompletionResult> resultHandler, string[] stopSequences)
        {
            CreateApiHandler(options);

            await api.Completions.StreamCompletionAsync(GetRequest(options, request, stopSequences), resultHandler);
        }

        /// <summary>
        /// Creates a new conversation and appends a system message with the specified TurboChatBehavior.
        /// </summary>
        /// <param name="options">The options to use for the conversation.</param>
        /// <returns>The newly created conversation.</returns>
        public static Conversation CreateConversation(OptionPageGridGeneral options)
        {
            Conversation chat;

            if (Service == OpenAIService.OpenAI || string.IsNullOrWhiteSpace(AzureTurboChatDeploymentId))
            {
                CreateApiHandler(options);

                chat = api.Chat.CreateConversation();
            }
            else
            {
                CreateApiHandlerForAzureTurboChat(options);

                chat = apiForAzureTurboChat.Chat.CreateConversation();
            }

            chat.AppendSystemMessage(TurboChatBehavior);

            if (TurboChatModelLanguage == TurboChatModelLanguageEnum.GPT_4)
            {
                chat.Model = Model.GPT4;
            }

            return chat;
        }

        /// <summary>
        /// Creates an API handler with the given API key and proxy.
        /// </summary>
        /// <param name="options">All configurations to create the connection</param>
        private static void CreateApiHandler(OptionPageGridGeneral options)
        {
            if (api == null)
            {
                chatGPTHttpClient = new();

                if (!string.IsNullOrWhiteSpace(Proxy))
                {
                    chatGPTHttpClient.SetProxy(Proxy);
                }

                if (Service == OpenAIService.AzureOpenAI)
                {
                    api = OpenAIAPI.ForAzure(AzureResourceName, AzureDeploymentId, options.ApiKey);
                }
                else
                {
                    APIAuthentication auth;

                    if (!string.IsNullOrWhiteSpace(OpenAIOrganization))
                    {
                        auth = new(options.ApiKey, OpenAIOrganization);
                    }
                    else
                    {
                        auth = new(options.ApiKey);
                    }

                    api = new(auth);
                }

                api.HttpClientFactory = chatGPTHttpClient;
            }
            else if ((Service == OpenAIService.AzureOpenAI && !api.ApiUrlFormat.ToUpper().Contains("AZURE")) || (Service == OpenAIService.OpenAI && api.ApiUrlFormat.ToUpper().Contains("AZURE")))
            {
                api = null;
                CreateApiHandler(options);
            }
            else if (api.Auth.ApiKey != options.ApiKey)
            {
                api.Auth.ApiKey = options.ApiKey;
            }
        }

        /// <summary>
        /// Creates an API handler for Azure TurboChat using the provided options.
        /// </summary>
        /// <param name="options">The options to use for creating the API handler.</param>
        private static void CreateApiHandlerForAzureTurboChat(OptionPageGridGeneral options)
        {
            if (apiForAzureTurboChat == null)
            {
                chatGPTHttpClient = new();

                if (!string.IsNullOrWhiteSpace(Proxy))
                {
                    chatGPTHttpClient.SetProxy(Proxy);
                }

                apiForAzureTurboChat = OpenAIAPI.ForAzure(AzureResourceName, AzureTurboChatDeploymentId, options.ApiKey);

                apiForAzureTurboChat.HttpClientFactory = chatGPTHttpClient;

                if (!string.IsNullOrWhiteSpace(AzureTurboChatApiVersion))
                {
                    apiForAzureTurboChat.ApiVersion = AzureTurboChatApiVersion;
                }
            }
            else if (apiForAzureTurboChat.Auth.ApiKey != options.ApiKey)
            {
                apiForAzureTurboChat.Auth.ApiKey = options.ApiKey;
            }
        }

        /// <summary>
        /// Gets a CompletionRequest object based on the given options and request.
        /// </summary>
        /// <param name="options">The options to use for the request.</param>
        /// <param name="request">The request string.</param>
        /// <returns>A CompletionRequest object.</returns>
        private static CompletionRequest GetRequest(OptionPageGridGeneral options, string request)
        {
            return GetRequest(options, request, null);
        }

        /// <summary>
        /// Gets a CompletionRequest object based on the given options and request.
        /// </summary>
        /// <param name="options">The options to use for the request.</param>
        /// <param name="request">The request string.</param>
        /// <param name="stopSequences">Up to 4 sequences where the API will stop generating further tokens.</param>
        /// <returns>A CompletionRequest object.</returns>
        private static CompletionRequest GetRequest(OptionPageGridGeneral options, string request, string[] stopSequences)
        {
            Model model = Model.ChatGPTTurboInstruct;

            switch (ModelLanguage)
            {
                case ModelLanguageEnum.GPT_3_5_Turbo:
                    model = Model.ChatGPTTurboInstruct;
                    break;
                    //case ModelLanguageEnum.TextBabbage001:
                    //    model = Model.BabbageText;
                    //    break;
                    //case ModelLanguageEnum.TextAda001:
                    //    model = Model.AdaText;
                    //   break;
            }

            if (stopSequences == null || stopSequences.Length == 0)
            {
                stopSequences = StopSequences.Split(',');
            }

            return new(request, model, MaxTokens, Temperature, presencePenalty: PresencePenalty, frequencyPenalty: FrequencyPenalty, top_p: TopP, stopSequences: stopSequences);
        }
    }
}
