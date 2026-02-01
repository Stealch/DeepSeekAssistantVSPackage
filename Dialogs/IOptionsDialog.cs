using Microsoft.VisualStudio.Shell;
using System.ComponentModel;
using System.Drawing.Design;
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
        [Description("Your DeepSeek API key (optional). Leave empty for anonymous access.")]
        [Editor(typeof(ApiKeyEditor), typeof(UITypeEditor))] // ← ДОБАВЛЯЕМ
        [PasswordPropertyText(true)] // ← Звездочки
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

        // УДАЛЯЕМ старые методы TestConnection() и ClearKey()
        // Они теперь в ApiKeyDialog

        [Browsable(false)] // Скрываем от PropertyGrid
        public string RawApiKey => _apiKey;

        protected override void OnApply(PageApplyEventArgs e)
        {
            if (!string.IsNullOrEmpty(_apiKey))
            {
                if (_apiKey.Length < 32 || _apiKey.Contains(" "))
                {
                    var result = MessageBox.Show(
                        "API key may be invalid. Save anyway?\n\n" +
                        $"Length: {_apiKey.Length} characters\n" +
                        "Valid keys are usually 32+ chars, no spaces.",
                        "Validation",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.No)
                    {
                        e.ApplyBehavior = ApplyKind.Cancel;
                    }
                }
            }
            base.OnApply(e);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}