// ToolWindow\DeepSeekChatWindowControl.xaml.cs
using DeepSeekAssistantVSPackage.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DeepSeekAssistantVSPackage.ToolWindows
{
    public partial class DeepSeekChatWindowControl : UserControl
    {
        private readonly ChatViewModel _viewModel;

        public DeepSeekChatWindowControl()
        {
            InitializeComponent();

            _viewModel = new ChatViewModel();
            _viewModel.NewSystemMessage += OnNewSystemMessage;
            _viewModel.ScrollToBottomRequested += OnScrollToBottomRequested;

            DataContext = _viewModel;
            ChatHistory.ItemsSource = _viewModel.Messages;

            Loaded += (s, e) =>
            {
                // НЕ используем await с Task.Run - запускаем без ожидания
                Task.Run(() => _viewModel.InitializeAsync());
            };
        }

        private void OnNewSystemMessage(object sender, string message)
        {
            Dispatcher.BeginInvoke((System.Action)(() =>
            {
                _viewModel.Messages.Add(new ChatMessageItem
                {
                    Message = $"[System] {message}",
                    BackgroundColor = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(100, 50, 0))
                });
                ScrollToBottom();
            }));
        }

        private void OnScrollToBottomRequested(object sender, System.EventArgs e)
        {
            ScrollToBottom();
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string message = InputTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(message))
            {
                InputTextBox.Clear();
                await _viewModel.SendMessageAsync(message);
            }
        }

        private async void InputTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter &&
                !Keyboard.IsKeyDown(System.Windows.Input.Key.LeftShift) &&
                !Keyboard.IsKeyDown(System.Windows.Input.Key.RightShift))
            {
                e.Handled = true;
                await ProcessSendMessageAsync();
            }
        }
        private async Task ProcessSendMessageAsync()
        {
            string message = InputTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(message))
            {
                InputTextBox.Clear();
                await _viewModel.SendMessageAsync(message);
            }
        }

        private void ScrollToBottom()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke((System.Action)ScrollToBottom);
                return;
            }

            if (ChatHistory.Items.Count > 0)
            {
                ChatHistory.UpdateLayout();
                ChatHistory.ScrollIntoView(ChatHistory.Items[ChatHistory.Items.Count - 1]);
            }
        }

        // Контекстное меню методы
        private void ChatMessage_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is ChatMessageItem item)
            {
                ChatHistory.SelectedItem = item;
            }
        }

        private void CopyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (ChatHistory.SelectedItem is ChatMessageItem selectedItem)
            {
                try
                {
                    Clipboard.SetText(selectedItem.Message);
                    _viewModel.Messages.Add(new ChatMessageItem
                    {
                        Message = "[System] Message copied to clipboard",
                        BackgroundColor = new System.Windows.Media.SolidColorBrush(
                            System.Windows.Media.Color.FromRgb(100, 50, 0))
                    });
                }
                catch (System.Exception ex)
                {
                    _viewModel.Messages.Add(new ChatMessageItem
                    {
                        Message = $"[System] Failed to copy: {ex.Message}",
                        BackgroundColor = new System.Windows.Media.SolidColorBrush(
                            System.Windows.Media.Color.FromRgb(100, 50, 0))
                    });
                }
            }
        }

        private void CopyAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Теперь Select будет работать с using System.Linq
                var allMessages = string.Join("\n\n",
                    _viewModel.Messages.Select(m => $"{GetMessageType(m)}: {m.Message}"));

                Clipboard.SetText(allMessages);
                _viewModel.Messages.Add(new ChatMessageItem
                {
                    Message = "[System] All messages copied to clipboard",
                    BackgroundColor = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(100, 50, 0))
                });
            }
            catch (System.Exception ex)
            {
                _viewModel.Messages.Add(new ChatMessageItem
                {
                    Message = $"[System] Failed to copy all: {ex.Message}",
                    BackgroundColor = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(100, 50, 0))
                });
            }
        }

        private void ClearChatMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ClearChat();
        }

        private string GetMessageType(ChatMessageItem item)
        {
            var color = item.BackgroundColor as System.Windows.Media.SolidColorBrush;
            if (color == null) return "[Unknown]";

            if (color.Color.R == 0 && color.Color.G == 120 && color.Color.B == 215)
                return "[User]";
            else if (color.Color.R == 30 && color.Color.G == 30 && color.Color.B == 30)
                return "[Assistant]";
            else
                return "[System]";
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _viewModel?.Dispose();
        }
    }
}