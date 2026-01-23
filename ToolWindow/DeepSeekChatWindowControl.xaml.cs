using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DeepseekAPILib;
using DeepseekAPILib.OAuth;
using DeepseekAPILib.Services;

namespace DeepSeekAssistantVSPackage.ToolWindows
{
    public partial class DeepSeekChatWindowControl : UserControl
    {
        private DeepSeekAPI _apiClient;
        private DeepseekAuthService _authService;
        private SystemBrowserOAuthService _oauthService;

        public ObservableCollection<ChatMessageItem> ChatMessages { get; set; }

        public DeepSeekChatWindowControl()
        {
            InitializeComponent();
            ChatMessages = new ObservableCollection<ChatMessageItem>();
            ChatHistory.ItemsSource = ChatMessages;

            InitializeServices();
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

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await SendMessageAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                AddSystemMessage($"Error: {ex.Message}");
            }
        }

        private async void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
            {
                try
                {
                    await SendMessageAsync().ConfigureAwait(false);
                    e.Handled = true;
                }
                catch (Exception ex)
                {
                    AddSystemMessage($"Error: {ex.Message}");
                }
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
                // Отправляем запрос
                var chatMessage = new ChatMessage("user", message);
                var messages = new System.Collections.Generic.List<ChatMessage> { chatMessage };

                // TODO: Добавить streaming
                var response = await _apiClient.SendChatSimpleAsync(messages);

                // Добавляем ответ ассистента
                AddAssistantMessage(response);
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
            if (ChatHistory.Items.Count > 0)
            {
                var border = (Border)VisualTreeHelper.GetChild(ChatHistory, 0);
                var scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                scrollViewer.ScrollToBottom();
            }
        }
    }

    public class ChatMessageItem
    {
        public string Message { get; set; }
        public Brush BackgroundColor { get; set; }
    }
}