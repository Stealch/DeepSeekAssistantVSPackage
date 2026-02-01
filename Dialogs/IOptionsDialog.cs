using Microsoft.VisualStudio.Shell;
using System;
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
        [Editor(typeof(ApiKeyOptionsEditor), typeof(UITypeEditor))]
        // [PasswordPropertyText(true)] // ← Звездочки
        [ReadOnly(true)] // ← Это сделает поле серым в PropertyGrid
        public string MaskedApiKey
        {
            get
            {
                if (string.IsNullOrEmpty(_apiKey))
                    return "[No key - click to set]";

                return $"{_apiKey.Substring(0, 8)}{new string('*', Math.Max(0, _apiKey.Length - 12))}{_apiKey.Substring(_apiKey.Length - 4)}";
            }
        }

        [Browsable(false)]
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