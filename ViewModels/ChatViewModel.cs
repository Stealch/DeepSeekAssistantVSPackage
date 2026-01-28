// ViewModels\ChatViewModel.cs (обновленная версия)
using DeepseekAPILib;
using DeepSeekAssistantVSPackage.Options;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace DeepSeekAssistantVSPackage.ViewModels
{
    public class ChatViewModel : INotifyPropertyChanged, IDisposable
    {
        // Меняем тип с DeepSeekAPI на IDeepSeekClient
        private IDeepSeekClient _apiClient;
        private DeepSeekOptionsPage _optionsPage;

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
                OnNewSystemMessage("Пакет не инициализирован");
                return;
            }

            _optionsPage = (DeepSeekOptionsPage)package.GetDialogPage(typeof(DeepSeekOptionsPage));
            if (_optionsPage == null)
            {
                OnNewSystemMessage("Страница настроек не найдена");
                return;
            }

            UpdateApiClient();
            SubscribeToOptionsChanges();

            OnNewSystemMessage("Чат инициализирован");
        }

        private void UpdateApiClient()
        {
            try
            {
                _apiClient?.Dispose();

                var apiKey = _optionsPage?.ApiKey?.Trim();
                string keyForApi = string.IsNullOrEmpty(apiKey) ? " " : apiKey;

                // ОТЛАДКА АРХИТЕКТУРЫ
                bool is64BitProcess = IntPtr.Size == 8;
                bool is64BitOS = Environment.Is64BitOperatingSystem;

                OnNewSystemMessage($"Process: {(is64BitProcess ? "x64" : "x86")}");
                OnNewSystemMessage($"OS: {(is64BitOS ? "x64" : "x86")}");
                OnNewSystemMessage($"IntPtr.Size: {IntPtr.Size}");

                var version = ProtocolDetector.GetWindowsVersion();
                OnNewSystemMessage($"Windows: {version.Major}.{version.Minor}.{version.Build}");

                // Принудительно Curl для Windows 7 с отладкой
                if (version.Major == 6 && version.Minor == 1)
                {
                    OnNewSystemMessage("Windows 7 detected, testing Curl...");

                    try
                    {
                        // Получаем информацию о libcurl через рефлексию
                        string curlDebugInfo = GetLibCurlDebugInfo();
                        OnNewSystemMessage($"LibCurl: {curlDebugInfo}");

                        _apiClient = new DeepSeekCurlClient(keyForApi);
                        OnNewSystemMessage("✓ Curl client loaded");
                    }
                    catch (Exception curlEx)
                    {
                        OnNewSystemMessage($"✗ Curl failed: {curlEx.Message}");

                        // Fallback на HttpClient
                        OnNewSystemMessage("Trying HttpClient fallback...");
                        _apiClient = new DeepSeekAPI(keyForApi);
                        OnNewSystemMessage("✓ HttpClient fallback loaded");
                    }
                }
                else
                {
                    _apiClient = DeepSeekClientFactory.CreateClient(keyForApi);
                }

                // Проверяем финальный тип клиента
                OnNewSystemMessage($"Final client: {_apiClient.GetType().Name}");
            }
            catch (Exception ex)
            {
                OnNewSystemMessage($"❌ Critical: {ex.Message}");
                throw;
            }
        }

        private string GetLibCurlDebugInfo()
        {
            try
            {
                // Загружаем библиотеку через рефлексию
                var assembly = Assembly.Load("DeepseekAPILib");

                // Проверяем наличие libcurl-x86.dll в ресурсах
                var resources = assembly.GetManifestResourceNames();
                bool hasX86 = resources.Any(r => r.Contains("libcurl-x86.dll"));
                bool hasX64 = resources.Any(r => r.Contains("libcurl-x64.dll"));

                return $"Resources: x86={hasX86}, x64={hasX64}, Count={resources.Length}";
            }
            catch (Exception ex)
            {
                return $"Error checking libcurl: {ex.Message}";
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
                    OnNewSystemMessage("Получен пустой ответ");
                }
            }
            catch (Exception ex)
            {
                OnNewSystemMessage($"Ошибка: {ex.Message}");
            }

            OnScrollToBottomRequested();
        }

        public async System.Threading.Tasks.Task SendTestMessageAsync()
        {
            await SendMessageAsync("Привет! Это тестовое сообщение.");
        }

        public void ClearChat()
        {
            Messages.Clear();
            OnNewSystemMessage("Чат очищен");
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
                    OnNewSystemMessage("API ключ обновлен");
                });
            }
        }

        public void Dispose()
        {
            _apiClient?.Dispose();
            if (_optionsPage != null)
            {
                _optionsPage.PropertyChanged -= OnOptionsPropertyChanged;
            }
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