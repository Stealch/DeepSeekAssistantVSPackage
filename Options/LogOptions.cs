// Options\LogOptions.cs
using DeepseekAPILib.Utilities;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Runtime.InteropServices;

namespace DeepSeekAssistantVSPackage.Options
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid(PackageGuids.DeepSeekLogOptionsPageString)]
    [ComVisible(true)]
    public class LogOptions : DialogPage
    {
        private bool _enableApiLogging = true;
        private string _logFilePath;

        [Category("Debugging")]
        [DisplayName("Enable API Logging")]
        [Description("Enable detailed logging of API requests and responses")]
        public bool EnableApiLogging
        {
            get => _enableApiLogging;
            set
            {
                if (_enableApiLogging != value)
                {
                    _enableApiLogging = value;
                    OnPropertyChanged(nameof(EnableApiLogging));

                    // Применить настройку
                    if (Logger.IsEnabled != value)
                    {
                        Logger.SetEnabled(value);
                    }
                }
            }
        }

        [Category("Debugging")]
        [DisplayName("Log File Path")]
        [Description("Path to the API log file")]
        [Editor(typeof(LogFileEditor), typeof(UITypeEditor))]
        [ReadOnly(true)]
        public string LogFilePath
        {
            get
            {
                if (string.IsNullOrEmpty(_logFilePath))
                {
                    _logFilePath = Logger.GetLogFilePath() ?? "Not initialized";
                }
                return _logFilePath;
            }
        }

        [Category("Debugging")]
        [DisplayName("Open Log File")]
        [Description("Open the current log file in default text editor")]
        [Browsable(true)] // ← ВАЖНО: чтобы отображалась как кнопка
        public void OpenLogFile()
        {
            ThreadHelper.ThrowIfNotOnUIThread(); // ← ВАЖНО: UI поток

            try
            {
                var path = Logger.GetLogFilePath();
                if (File.Exists(path))
                {
                    System.Diagnostics.Process.Start("notepad.exe", $"\"{path}\"");
                }
                else
                {
                    System.Windows.MessageBox.Show(
                        $"Log file not found:\n{path}",
                        "Log File",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Error opening log: {ex.Message}",
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        [Category("Debugging")]
        [DisplayName("Clear Log")]
        [Description("Clear the current log file")]
        [Browsable(true)] // ← ВАЖНО: чтобы отображалась как кнопка
        public void ClearLog()
        {
            ThreadHelper.ThrowIfNotOnUIThread(); // ← ВАЖНО: UI поток

            var result = System.Windows.MessageBox.Show(
                "Clear all log contents?",
                "Clear Log",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                Logger.ClearLog();
                System.Windows.MessageBox.Show(
                    "Log cleared",
                    "Clear Log",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            }
        }

        [Category("Debugging")]
        [DisplayName("Show Log Folder")]
        [Description("Open the folder containing the log file")]
        [Browsable(true)] // ← ВАЖНО: чтобы отображалась как кнопка
        public void ShowLogFolder()
        {
            ThreadHelper.ThrowIfNotOnUIThread(); // ← ВАЖНО: UI поток

            try
            {
                var path = Logger.GetLogFilePath();
                if (!string.IsNullOrEmpty(path))
                {
                    var folder = Path.GetDirectoryName(path);
                    if (Directory.Exists(folder))
                    {
                        System.Diagnostics.Process.Start("explorer.exe", $"\"{folder}\"");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Error opening folder: {ex.Message}",
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}