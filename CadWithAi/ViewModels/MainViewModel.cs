using CadWithAi.AI;
using CadWithAi.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Controls;

namespace CadWithAi.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IBaseCadOperationService _baseCadOperationService;
        private readonly AIService _aiService;

        [ObservableProperty]
        private string _userPrompt = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        public ObservableCollection<string> Logs { get; } = new();

        public MainViewModel()
        {
            // Initialize business service
            _baseCadOperationService = new BaseCadOperationService();

            // Initialize AI Service (Ollama runs locally, so no API key is needed)
            _aiService = new AIService(_baseCadOperationService);

            Log("App Initialized. Connected to local Ollama (Llama 3.2).");
        }

        public void SetHelixViewport3D(HelixToolkit.Wpf.HelixViewport3D hvp)
        {
            _baseCadOperationService.Hvp = hvp;
            Log("HelixViewport3D set in BaseCadOperationService.");
        }

        // ==========================================
        // AI MODE: Natural Language Processing
        // ==========================================

        [RelayCommand]
        private async Task SendAiMessageAsync()
        {
            if (string.IsNullOrWhiteSpace(UserPrompt)) return;

            string query = UserPrompt;
            UserPrompt = string.Empty; // Clear text box immediately
            IsBusy = true;

            Log($"User (AI Chat): \"{query}\"");

            try
            {
                // Route query through AI engine to select tool and execute
                string aiResponse = await _aiService.ProcessUserRequestAsync(query);
                Log($"AI Result: {aiResponse}");
            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}");
                if (ex.Message.Contains("refused") || ex.Message.Contains("11434"))
                {
                    Log("Hint: Make sure Ollama is running (`ollama run llama3.2` in terminal).");
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
}