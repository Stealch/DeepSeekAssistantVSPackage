// ViewModels\ChatViewModel.cs
using DeepseekAPILib;
using DeepSeekAssistantVSPackage.Options;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace DeepSeekAssistantVSPackage.ViewModels
{
    public class ChatViewModel : INotifyPropertyChanged, IDisposable
    {
        private DeepSeekAPI _apiClient;
        private DeepSeekOptionsPage _optionsPage;
        private bool _isInitialized;

        public ObservableCollection<ChatMessageItem> Messages { get; }
        public event EventHandler<string> NewSystemMessage;
        public event EventHandler ScrollToBottomRequested;

        public ChatViewModel()
        {
            Messages = new ObservableCollection<ChatMessageItem>();
        }

        public async System.Threading.Tasks.Task InitializeAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var package = DeepSeekAssistantVSPackagePackage.Instance;
            if (package == null)
            {
                OnNewSystemMessage("Package not initialized");
                return;
            }

            _optionsPage = (DeepSeekOptionsPage)package.GetDialogPage(typeof(DeepSeekOptionsPage));
            if (_optionsPage == null)
            {
                OnNewSystemMessage("Options page not found");
                return;
            }

            UpdateApiClient();
            SubscribeToOptionsChanges();

            _isInitialized = true;
            OnNewSystemMessage("Chat initialized");
        }

        private void UpdateApiClient()
        {
            try
            {
                _apiClient?.Dispose();

                var apiKey = _optionsPage?.ApiKey?.Trim();
                string keyForApi = string.IsNullOrEmpty(apiKey) ? " " : apiKey;

                _apiClient = new DeepSeekAPI(keyForApi);

                string status = string.IsNullOrEmpty(apiKey)
                    ? "Using anonymous access"
                    : $"API key set ({apiKey.Length} chars)";

                OnNewSystemMessage(status);
            }
            catch (Exception ex)
            {
                _apiClient = null;
                OnNewSystemMessage($"Failed to initialize API: {ex.Message}");
            }
        }

        public async System.Threading.Tasks.Task SendMessageAsync(string message)
        {
            if (string.IsNullOrEmpty(message) || _apiClient == null)
                return;

            AddUserMessage(message);

            try
            {
                var chatMessage = new ChatMessage("user", message);
                var messages = new System.Collections.Generic.List<ChatMessage> { chatMessage };

                var response = await _apiClient.SendChatSimpleAsync(messages);

                if (!string.IsNullOrEmpty(response))
                {
                    AddAssistantMessage(response);
                }
                else
                {
                    OnNewSystemMessage("Empty response received");
                }
            }
            catch (Exception ex)
            {
                OnNewSystemMessage($"Error: {ex.Message}");
            }

            OnScrollToBottomRequested();
        }

        public async System.Threading.Tasks.Task SendTestMessageAsync()
        {
            await SendMessageAsync("Hello! This is a test message.");
        }

        public void ClearChat()
        {
            Messages.Clear();
            OnNewSystemMessage("Chat cleared");
        }

        private void AddUserMessage(string message)
        {
            AddMessage(new ChatMessageItem
            {
                Message = message,
                BackgroundColor = new SolidColorBrush(Color.FromRgb(0, 120, 215))
            });
        }

        private void AddAssistantMessage(string message)
        {
            AddMessage(new ChatMessageItem
            {
                Message = message,
                BackgroundColor = new SolidColorBrush(Color.FromRgb(30, 30, 30))
            });
        }

        private void AddMessage(ChatMessageItem item)
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                Messages.Add(item);
            });
        }

        private void OnNewSystemMessage(string message)
        {
            NewSystemMessage?.Invoke(this, message);
        }

        private void OnScrollToBottomRequested()
        {
            ScrollToBottomRequested?.Invoke(this, EventArgs.Empty);
        }

        private void SubscribeToOptionsChanges()
        {
            if (_optionsPage != null)
            {
                _optionsPage.PropertyChanged += OnOptionsPropertyChanged;
            }
        }

        private void OnOptionsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DeepSeekOptionsPage.ApiKey))
            {
                ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    UpdateApiClient();
                    OnNewSystemMessage("API key updated");
                });
            }
        }

        public void Dispose()
        {
            _apiClient?.Dispose();
            _optionsPage.PropertyChanged -= OnOptionsPropertyChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ChatMessageItem
    {
        public string Message { get; set; }
        public Brush BackgroundColor { get; set; }
    }
}