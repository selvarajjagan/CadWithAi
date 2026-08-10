using Microsoft.Extensions.AI;
using System.Text.Json;

namespace CadWithAi.AI;

public sealed class AIService
{
    private const string OllamaUrl = "http://localhost:11434";
    private const string ModelName = "qwen3:4b";

    private readonly IChatClient _chatClient;
    private readonly ToolRegistry _toolRegistry;

    public AIService(ToolRegistry toolRegistry)
    {
        _toolRegistry = toolRegistry;

        IChatClient baseClient = new OllamaChatClient(
            new Uri(OllamaUrl),
            ModelName);

        _chatClient = baseClient
            .AsBuilder()
            .UseFunctionInvocation()
            .Build();
    }

    public IReadOnlyList<ToolServiceDescriptor> Services => _toolRegistry.Services;

    public async Task<string> ProcessUserRequestAsync(
        string userMessage,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
            return "Please enter a request.";

        ToolServiceDescriptor? service = await SelectServiceAsync(userMessage, cancellationToken);

        if (service is null)
            return "I could not determine which CAD operation service should handle that request.";

        ChatOptions options = new()
        {
            Tools = service.Tools.Select(x => (AITool)x.Function).ToList(),
            Temperature = 0,
            MaxOutputTokens = 128,
            AllowMultipleToolCalls = false,
            Instructions =
                "You are the operation executor for the selected CAD service. " +
                "Choose the single best tool for the user's request and invoke it immediately. " +
                "Do not explain your reasoning. If the request cannot be completed by an available tool, say so briefly."
        };

        ChatResponse response = await _chatClient.GetResponseAsync(
            [new ChatMessage(ChatRole.User, userMessage)],
            options,
            cancellationToken);

        return response.Text;
    }

    private async Task<ToolServiceDescriptor?> SelectServiceAsync(
        string userMessage,
        CancellationToken cancellationToken)
    {
        string catalog = string.Join(
            "\n",
            _toolRegistry.Services.Select(x => $"- {x.Name}: {x.Description}"));

        string prompt = $"""
            Select the single service that best matches the user's request.

            Available services:
            {catalog}

            User request:
            {userMessage}

            Return ONLY valid JSON in this exact format:
            {{"service":"ServiceName"}}
            """;

        ChatOptions options = new()
        {
            Temperature = 0,
            MaxOutputTokens = 32,
            Instructions = "You are a fast service router. Return only the requested JSON. Do not explain your reasoning."
        };

        ChatResponse response = await _chatClient.GetResponseAsync(
            [new ChatMessage(ChatRole.User, prompt)],
            options,
            cancellationToken);

        string json = ExtractJsonObject(response.Text);

        try
        {
            ServiceSelection? selection = JsonSerializer.Deserialize<ServiceSelection>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return selection?.Service is null
                ? null
                : _toolRegistry.FindService(selection.Service);
        }
        catch (JsonException)
        {
            return _toolRegistry.FindService(response.Text.Trim());
        }
    }

    private static string ExtractJsonObject(string text)
    {
        int start = text.IndexOf('{');
        int end = text.LastIndexOf('}');

        return start >= 0 && end > start
            ? text[start..(end + 1)]
            : text.Trim();
    }

    private sealed class ServiceSelection
    {
        public string? Service { get; set; }
    }
}
