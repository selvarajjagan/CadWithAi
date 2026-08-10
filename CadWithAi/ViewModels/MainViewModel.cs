using CadWithAi.AI;
using CadWithAi.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Reflection;

namespace CadWithAi.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ToolRegistry _toolRegistry;
    private readonly AIService _aiService;

    [ObservableProperty]
    private string _userPrompt = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    public ObservableCollection<string> Logs { get; } = new();

    public MainViewModel()
    {
        _toolRegistry = new ToolRegistry(Assembly.GetExecutingAssembly());
        _aiService = new AIService(_toolRegistry);

        Log($"App initialized. Discovered {_toolRegistry.Services.Count} AI service(s).");

        foreach (ToolServiceDescriptor service in _toolRegistry.Services)
            Log($"AI service: {service.Name} ({service.Tools.Count} tools)");

        Log("Connected to local Ollama using qwen3:4b.");
    }

    public void SetHelixViewport3D(HelixToolkit.Wpf.HelixViewport3D? hvp)
    {
        ToolServiceDescriptor? descriptor = _toolRegistry.FindService(nameof(IBaseCadOperationService));

        if (descriptor?.Instance is IBaseCadOperationService cadService)
        {
            cadService.Hvp = hvp;
            Log("HelixViewport3D connected to BaseCadOperationService.");
        }
        else
        {
            Log("Error: BaseCadOperationService was not discovered by the AI tool registry.");
        }
    }

    [RelayCommand]
    private async Task SendAiMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(UserPrompt) || IsBusy)
            return;

        string query = UserPrompt.Trim();
        UserPrompt = string.Empty;
        IsBusy = true;

        Log($"User (AI Chat): \"{query}\"");

        try
        {
            string aiResponse = await _aiService.ProcessUserRequestAsync(query);
            Log($"AI Result: {aiResponse}");
        }
        catch (OperationCanceledException)
        {
            Log("AI request cancelled.");
        }
        catch (Exception ex)
        {
            Log($"AI Error: {ex.Message}");

            if (ex.Message.Contains("11434", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("connection refused", StringComparison.OrdinalIgnoreCase))
            {
                Log("Hint: Start Ollama and make sure qwen3:4b is installed.");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void Log(string message)
        => Logs.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}");
}
