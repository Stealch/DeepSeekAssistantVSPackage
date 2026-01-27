using DeepseekAPILib;
using DeepseekAPILib.OAuth;
using DeepseekAPILib.Services;
using DeepSeekAssistantVSPackage.Options;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

// ToolWindow\DeepSeekChatWindowControl.xaml.cs
namespace DeepSeekAssistantVSPackage.ToolWindows
{
    public partial class DeepSeekChatWindowControl : UserControl, IDisposable
    {
        private DeepSeekAPI _apiClient;
        private DeepseekAuthService _authService;
        private DeepSeekOptionsPage _optionsPage;
        private bool _isInitialized;

        public ObservableCollection<ChatMessageItem> ChatMessages { get; set; }

        public DeepSeekChatWindowControl()
        {
            InitializeComponent();
            ChatMessages = new ObservableCollection<ChatMessageItem>();
            ChatHistory.ItemsSource = ChatMessages;

            // Добавляем тестовые команды
            AddSystemMessage("Available commands:");
            AddSystemMessage("- Type any message to chat with DeepSeek");
            AddSystemMessage("- Type '/test' to send a test message");
            AddSystemMessage("- Type '/clear' to clear chat history");

            try
            {
                InitializeServices();
                SubscribeToOptionsChanges();
            }
            catch (Exception ex)
            {
                AddSystemMessage($"Initialization error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[DeepSeek] Control init error: {ex}");
            }
        }

        private void InitializeServices()
        {
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                
                // Получаем пакет
                var package = DeepSeekAssistantVSPackagePackage.Instance;
                if (package == null)
                {
                    AddSystemMessage("Package not initialized. Please restart Visual Studio.");
                    return;
                }

                // Получаем страницу настроек
                _optionsPage = (DeepSeekOptionsPage)package.GetDialogPage(typeof(DeepSeekOptionsPage));
                if (_optionsPage == null)
                {
                    AddSystemMessage("Options page not found.");
                    return;
                }

                // Инициализируем API клиент с текущим ключом
                UpdateApiClient();

                // Проверяем API ключ
                if (!string.IsNullOrEmpty(_optionsPage.ApiKey))
                {
                    AddSystemMessage("DeepSeek Assistant ready with API key. Type your message...");
                }
                else
                {
                    AddSystemMessage("DeepSeek Assistant ready (anonymous access). Type your message...\n" +
                                   "Note: For full features, set API key in Tools → Options → DeepSeek Assistant");
                }

                _isInitialized = true;
            }
            catch (Exception ex)
            {
                _isInitialized = false;
                AddSystemMessage($"Error initializing: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[DeepSeek] InitializeServices error: {ex}");
            }
        }

        private void SubscribeToOptionsChanges()
        {
            try
            {
                if (_optionsPage != null)
                {
                    _optionsPage.PropertyChanged += OnOptionsPropertyChanged;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DeepSeek] Failed to subscribe to options changes: {ex}");
            }
        }

        private void OnOptionsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DeepSeekOptionsPage.ApiKey))
            {
                // Используем Dispatcher.BeginInvoke вместо Dispatcher.InvokeAsync (C# 8.0)
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    try
                    {
                        UpdateApiClient();
                        
                        if (string.IsNullOrEmpty(_optionsPage.ApiKey))
                        {
                            AddSystemMessage("Switched to anonymous access.");
                        }
                        else
                        {
                            AddSystemMessage("API key updated.");
                        }
                    }
                    catch (Exception ex)
                    {
                        AddSystemMessage($"Failed to update API key: {ex.Message}");
                    }
                }));
            }
        }

        private void UpdateApiClient()
        {
            try
            {
                // Dispose старого клиента если есть
                if (_apiClient != null)
                {
                    _apiClient.Dispose();
                    _apiClient = null;
                }

                var apiKey = _optionsPage?.ApiKey?.Trim();

                if (string.IsNullOrEmpty(apiKey))
                {
                    // Анонимный доступ
                    try
                    {
                        _apiClient = new DeepSeekAPI(" ");
                        AddSystemMessage("Using anonymous access.");
                    }
                    catch (Exception ex)
                    {
                        _apiClient = null;
                        AddSystemMessage($"Failed anonymous setup: {ex.Message}");
                    }
                }
                else
                {
                    // Пробуем создать клиент с ключом
                    try
                    {
                        _apiClient = new DeepSeekAPI(apiKey);
                        AddSystemMessage($"API key updated ({apiKey.Length} chars).");
                    }
                    catch (ArgumentException argEx)
                    {
                        // Конструктор бросил исключение при создании
                        _apiClient = null;
                        AddSystemMessage($"Invalid API key format: {argEx.Message}");
                    }
                    catch (Exception ex)
                    {
                        _apiClient = null;
                        AddSystemMessage($"Failed to set API key: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _apiClient = null;
                System.Diagnostics.Debug.WriteLine($"[DeepSeek] UpdateApiClient error: {ex}");
            }
        }

        private bool IsValidApiKeyFormat(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
                return false;

            // DeepSeek API keys are typically:
            // - At least 32 characters
            // - Alphanumeric with some special chars
            // - No spaces

            if (apiKey.Length < 32)
                return false;

            if (apiKey.Contains(" "))
                return false;

            // Можно добавить дополнительные проверки
            // Например, что ключ содержит буквы и цифры
            bool hasLetters = false;
            bool hasDigits = false;

            foreach (char c in apiKey)
            {
                if (char.IsLetter(c)) hasLetters = true;
                if (char.IsDigit(c)) hasDigits = true;
                if (hasLetters && hasDigits) break;
            }

            return hasLetters && hasDigits;
        }

        private string LoadApiKeyFromSettings()
        {
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                
                if (_optionsPage == null)
                {
                    var package = DeepSeekAssistantVSPackagePackage.Instance;
                    if (package != null)
                    {
                        _optionsPage = (DeepSeekOptionsPage)package.GetDialogPage(typeof(DeepSeekOptionsPage));
                    }
                }

                return _optionsPage?.ApiKey ?? string.Empty;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DeepSeek] Error loading API key: {ex.Message}");
                return string.Empty;
            }
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            await HandleSendMessageSafeAsync();
        }

        private async void InputTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter &&
                !System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftShift) &&
                !System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.RightShift))
            {
                e.Handled = true;
                await HandleSendMessageSafeAsync();
            }
        }

        private async System.Threading.Tasks.Task HandleSendMessageSafeAsync()
        {
            try
            {
                string message = InputTextBox.Text.Trim();

                if (message == "/test")
                {
                    await SendTestMessageAsync();
                }
                else if (message == "/clear")
                {
                    ChatMessages.Clear();
                    AddSystemMessage("Chat cleared.");
                }
                else
                {
                    await SendMessageAsync();
                }
            }
            catch (System.Exception ex)
            {
                AddSystemMessage($"Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[DeepSeek] Error in HandleSendMessageSafeAsync: {ex}");
            }
        }

        private async System.Threading.Tasks.Task SendTestMessageAsync()
        {
            InputTextBox.Clear();
            AddUserMessage("/test");

            try
            {
                var testMessage = new ChatMessage("user", "Hello! Please respond with a short greeting.");
                var messages = new System.Collections.Generic.List<ChatMessage> { testMessage };

                var response = await _apiClient.SendChatSimpleAsync(messages, maxTokens: 100);

                if (!string.IsNullOrEmpty(response))
                {
                    AddAssistantMessage(response);
                    AddSystemMessage("✅ Test successful! API is responding correctly.");
                }
                else
                {
                    AddSystemMessage("⚠️ Test completed but received empty response.");
                }
            }
            catch (Exception ex)
            {
                AddSystemMessage($"❌ Test failed: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task SendMessageAsync()
        {
            string message = InputTextBox.Text.Trim();
            if (string.IsNullOrEmpty(message))
                return;

            // Проверяем инициализацию API клиента
            if (_apiClient == null)
            {
                AddSystemMessage("API client not initialized. Please restart the chat window.");
                return;
            }

            // Добавляем сообщение пользователя
            AddUserMessage(message);
            InputTextBox.Clear();

            try
            {
                // Показываем индикатор загрузки
                var loadingItem = new ChatMessageItem
                {
                    Message = "Thinking...",
                    BackgroundColor = new SolidColorBrush(Color.FromRgb(60, 60, 60))
                };
                ChatMessages.Add(loadingItem);
                ScrollToBottom();

                // Отправляем запрос
                var chatMessage = new ChatMessage("user", message);
                var messages = new System.Collections.Generic.List<ChatMessage> { chatMessage };

                var response = await _apiClient.SendChatSimpleAsync(messages);

                // Убираем индикатор загрузки
                ChatMessages.Remove(loadingItem);

                // Добавляем ответ ассистента
                if (!string.IsNullOrEmpty(response))
                {
                    AddAssistantMessage(response);
                }
                else
                {
                    AddSystemMessage("Received empty response from API.");
                }
            }
            catch (DeepseekAPILib.Models.DeepseekApiException apiEx)
            {
                AddSystemMessage($"API Error ({apiEx.StatusCode}): {apiEx.Message}");

                // Полезные советы в зависимости от ошибки
                if (apiEx.StatusCode == 401)
                {
                    AddSystemMessage("Tip: Check your API key in Tools → Options → DeepSeek Assistant");
                }
                else if (apiEx.StatusCode == 429)
                {
                    AddSystemMessage("Tip: Rate limit exceeded. Try again later.");
                }
            }
            catch (System.Net.Http.HttpRequestException httpEx)
            {
                AddSystemMessage($"Network error: {httpEx.Message}");

                if (httpEx.InnerException is System.Net.Sockets.SocketException)
                {
                    AddSystemMessage("Tip: Check your internet connection and firewall.");
                }
            }
            catch (TaskCanceledException)
            {
                AddSystemMessage("Request timeout. The API took too long to respond.");
            }
            catch (Exception ex)
            {
                AddSystemMessage($"Error: {ex.Message}");
            }
        }

        private void AddUserMessage(string message)
        {
            ChatMessages.Add(new ChatMessageItem
            {
                Message = message,
                BackgroundColor = new SolidColorBrush(Color.FromRgb(0, 120, 215))
            });
            ScrollToBottom();
        }

        private void AddAssistantMessage(string message)
        {
            ChatMessages.Add(new ChatMessageItem
            {
                Message = message,
                BackgroundColor = new SolidColorBrush(Color.FromRgb(30, 30, 30))
            });
            ScrollToBottom();
        }

        private void AddSystemMessage(string message)
        {
            ChatMessages.Add(new ChatMessageItem
            {
                Message = $"[System] {message}",
                BackgroundColor = new SolidColorBrush(Color.FromRgb(100, 50, 0))
            });

            ScrollToBottom();
        }

        private void ScrollToBottom()
        {
            if (ChatHistory.Items.Count == 0)
                return;

            // Используем классический BeginInvoke
            Dispatcher.BeginInvoke((Action)(() =>
            {
                ChatHistory.UpdateLayout();
                ChatHistory.ScrollIntoView(ChatHistory.Items[ChatHistory.Items.Count - 1]);
            }));
        }

        public void Dispose()
        {
            try
            {
                if (_optionsPage != null)
                {
                    _optionsPage.PropertyChanged -= OnOptionsPropertyChanged;
                    _optionsPage = null;
                }

                if (_apiClient != null)
                {
                    _apiClient.Dispose();
                    _apiClient = null;
                }

                if (_authService != null)
                {
                    _authService.Dispose();
                    _authService = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DeepSeek] Dispose error: {ex}");
            }
        }
    }

    public class ChatMessageItem
    {
        public string Message { get; set; }
        public Brush BackgroundColor { get; set; }
    }
}