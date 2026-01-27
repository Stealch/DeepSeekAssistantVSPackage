using Microsoft.VisualStudio.Shell;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

// Dialogs\IOptionsDialog.cs
namespace DeepSeekAssistantVSPackage.Options
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid(PackageGuids.DeepSeekOptionsPageString)]
    [ComVisible(true)]
    public class DeepSeekOptionsPage : DialogPage
    {
        private string _apiKey = string.Empty;

        [Category("DeepSeek API")]
        [DisplayName("API Key")]
        [Description("Your DeepSeek API key (optional). Leave empty for anonymous access. Get it from https://platform.deepseek.com/api_keys")]
        public string ApiKey
        {
            get => _apiKey;
            set
            {
                if (_apiKey != value)
                {
                    _apiKey = value;
                    OnPropertyChanged(nameof(ApiKey));
                }
            }
        }

        [Category("DeepSeek API")]
        [DisplayName("Test Connection")]
        [Description("Test the API connection with current key - sends a test message")]
        public void TestConnection()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                string testKey = string.IsNullOrEmpty(_apiKey) ? " " : _apiKey;
                string keyStatus;

                if (string.IsNullOrEmpty(_apiKey))
                {
                    keyStatus = "Anonymous access (no API key)";
                }
                else if (_apiKey.Length < 32 || _apiKey.Contains(" "))
                {
                    keyStatus = $"Warning: Key may be invalid ({_apiKey.Length} chars)";
                }
                else
                {
                    keyStatus = $"API key looks valid ({_apiKey.Length} chars)";
                }

                // Создаем API клиент для теста
                using (var client = new DeepseekAPILib.DeepSeekAPI(testKey))
                {
                    // Показываем окно с прогрессом
                    var progressForm = new TestConnectionForm(keyStatus);
                    progressForm.ShowDialog();

                    // Отправляем тестовое сообщение
                    var testMessage = new DeepseekAPILib.ChatMessage("user", "Hello, DeepSeek! Reply with 'OK' if you can hear me.");
                    var messages = new System.Collections.Generic.List<DeepseekAPILib.ChatMessage> { testMessage };

                    // Асинхронный вызов в синхронном методе
                    var task = System.Threading.Tasks.Task.Run(async () => await client.SendChatSimpleAsync(messages, maxTokens: 50));
                    task.Wait();

                    var response = task.Result;

                    if (!string.IsNullOrEmpty(response))
                    {
                        MessageBox.Show(
                            $"✅ Connection successful!\n\n" +
                            $"Status: {keyStatus}\n" +
                            $"Response: {response}\n\n" +
                            $"API is working correctly.",
                            "Test Connection - Success",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show(
                            $"⚠️ Connection established but empty response.\n\n" +
                            $"Status: {keyStatus}\n" +
                            $"API responded but returned no content.",
                            "Test Connection - Warning",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                }
            }
            catch (DeepseekAPILib.Models.DeepseekApiException apiEx)
            {
                MessageBox.Show(
                    $"❌ API Error ({apiEx.StatusCode}):\n{apiEx.Message}\n\n" +
                    $"The API key may be invalid, expired, or there's a network issue.",
                    "Test Connection Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (System.Net.Http.HttpRequestException httpEx)
            {
                MessageBox.Show(
                    $"🌐 Network Error:\n{httpEx.Message}\n\n" +
                    $"Check your internet connection and firewall settings.",
                    "Network Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(
                    $"⚠️ Error:\n{ex.Message}\n\n" +
                    $"Stack trace: {ex.StackTrace}",
                    "Test Connection Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        [Category("DeepSeek API")]
        [DisplayName("Clear Key")]
        [Description("Clear the API key")]
        public void ClearKey()
        {
            if (MessageBox.Show("Are you sure you want to clear the API key?",
                "Clear API Key", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _apiKey = string.Empty;
                OnPropertyChanged(nameof(ApiKey));
                MessageBox.Show("API key cleared.", "Clear Key",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        [Browsable(false)]
        public string RawApiKey => _apiKey;

        protected override void OnApply(PageApplyEventArgs e)
        {
            base.OnApply(e);

            if (!string.IsNullOrEmpty(_apiKey))
            {
                // Дополнительная проверка только если ключ не пустой
                if (_apiKey.Length < 32 || _apiKey.Contains(" "))
                {
                    var result = MessageBox.Show(
                        "The API key appears to be invalid. Continue anyway?\n\n" +
                        $"Key length: {_apiKey.Length} characters\n" +
                        "Valid API keys are usually at least 32 characters long and contain no spaces.\n\n" +
                        "Click Yes to save anyway, No to cancel.",
                        "Validation Warning",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.No)
                    {
                        e.ApplyBehavior = ApplyKind.Cancel;
                    }
                }
            }
            else
            {
                // Пустой ключ - это валидно (анонимный доступ)
                var result = MessageBox.Show(
                    "Using anonymous access (no API key).\n\n" +
                    "Note: Some features may be limited without an API key.\n" +
                    "Continue?",
                    "API Key Status",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Information);

                if (result == DialogResult.Cancel)
                {
                    e.ApplyBehavior = ApplyKind.Cancel;
                }
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}