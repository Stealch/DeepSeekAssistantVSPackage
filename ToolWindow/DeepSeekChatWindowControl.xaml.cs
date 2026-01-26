using DeepseekAPILib;
using DeepseekAPILib.OAuth;
using DeepseekAPILib.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;

// DeepSeekChatWindowControl.xaml.cs
namespace DeepSeekAssistantVSPackage.ToolWindows
{
    public partial class DeepSeekChatWindowControl : UserControl
    {
        private DeepSeekAPI _apiClient;
        private DeepseekAuthService _authService;
        private readonly SystemBrowserOAuthService _oauthService;

        public ObservableCollection<ChatMessageItem> ChatMessages { get; set; }

        public DeepSeekChatWindowControl()
        {
            InitializeComponent();
            ChatMessages = new ObservableCollection<ChatMessageItem>();
            ChatHistory.ItemsSource = ChatMessages;

            try
            {
                InitializeServices();
            }
            catch (Exception ex)
            {
                // Показываем сообщение об ошибке
                AddSystemMessage($"Initialization error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[DeepSeek] Control init error: {ex}");
            }
        }

        private void InitializeServices()
        {
            try
            {
                // TODO: Загрузить API ключ из настроек
                var apiKey = LoadApiKeyFromSettings();

                if (string.IsNullOrEmpty(apiKey))
                {
                    AddSystemMessage("Please configure your API key in Tools → Options → DeepSeek Assistant");
                    return;
                }

                _apiClient = new DeepSeekAPI(apiKey);
                _authService = new DeepseekAuthService();
                AddSystemMessage("DeepSeek Assistant ready. Type your message...");
            }
            catch (Exception ex)
            {
                AddSystemMessage($"Error initializing: {ex.Message}");
            }
        }

        private string LoadApiKeyFromSettings()
        {
            // TODO: Реализовать загрузку из настроек VS
            return string.Empty;
        }

        private async void SendButton_Click(object sender, System.Windows.RoutedEventArgs e)
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
                await SendMessageAsync();
            }
            catch (System.Exception ex)
            {
                AddSystemMessage($"Error: {ex.Message}");
                // Логирование для отладки
                System.Diagnostics.Debug.WriteLine($"[DeepSeek] Error in HandleSendMessageSafeAsync: {ex}");
            }
        }

        private async System.Threading.Tasks.Task SendMessageAsync()
        {
            var message = InputTextBox.Text.Trim();
            if (string.IsNullOrEmpty(message) || _apiClient == null)
                return;

            // Добавляем сообщение пользователя
            AddUserMessage(message);
            InputTextBox.Clear();

            try
            {
                /* // Отправляем запрос
                 var chatMessage = new ChatMessage("user", message);
                 var messages = new System.Collections.Generic.List<ChatMessage> { chatMessage };

                 // TODO: Добавить streaming
                 var response = await _apiClient.SendChatSimpleAsync(messages);

                 // Добавляем ответ ассистента
                 AddAssistantMessage(response);*/
                AddAssistantMessage("API service is not available in this version.");
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
                BackgroundColor = new SolidColorBrush(Color.FromRgb(0, 120, 215)) // Синий
            });
            ScrollToBottom();
        }

        private void AddAssistantMessage(string message)
        {
            ChatMessages.Add(new ChatMessageItem
            {
                Message = message,
                BackgroundColor = new SolidColorBrush(Color.FromRgb(30, 30, 30)) // Темно-серый
            });
            ScrollToBottom();
        }

        private void AddSystemMessage(string message)
        {
            ChatMessages.Add(new ChatMessageItem
            {
                Message = message,
                BackgroundColor = new SolidColorBrush(Color.FromRgb(100, 50, 0)) // Оранжевый
            });
            ScrollToBottom();
        }

        private void ScrollToBottom()
        {
            // Получаем ScrollViewer который оборачивает ChatHistory
            if (VisualTreeHelper.GetParent(ChatHistory) is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollToEnd();
            }
        }
    }

    public class ChatMessageItem
    {
        public string Message { get; set; }
        public Brush BackgroundColor { get; set; }
    }
}