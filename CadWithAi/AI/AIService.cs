using CadWithAi.Services;
using Microsoft.Extensions.AI;

namespace CadWithAi.AI
{
    public class AIService
    {
        private readonly IChatClient _chatClient;
        private readonly ChatOptions _chatOptions;

        public AIService(IBaseCadOperationService baseCadOperationService)
        {
            // Connect to local Ollama instance running qwen3:14b
            IChatClient baseClient = new OllamaChatClient(new Uri("http://localhost:11434"), "qwen3:14b");

            // Build automatic tool invocation pipeline
            _chatClient = baseClient
                .AsBuilder()
                .UseFunctionInvocation()
                .Build();

            // Register your interface methods as tools
            var tools = new List<AITool>
            {
                AIFunctionFactory.Create(baseCadOperationService.LoadObjAsync),
                AIFunctionFactory.Create(baseCadOperationService.GetLoadedModelAsync),
                AIFunctionFactory.Create(baseCadOperationService.RotateObjAsync),
                AIFunctionFactory.Create(baseCadOperationService.SetCameraAngleAsync),
            };

            _chatOptions = new ChatOptions
            {
                Tools = tools,
                Instructions = "You are an order processing assistant. " +
                               "Select and execute the appropriate tool function based on user intent."
            };
        }

        public async Task<string> ProcessUserRequestAsync(string userMessage)
        {
            var messages = new List<ChatMessage>
            {
                new(ChatRole.User, userMessage)
            };

            ChatResponse response = await _chatClient.GetResponseAsync(messages, _chatOptions);
            return response.Text;
        }
    }
}