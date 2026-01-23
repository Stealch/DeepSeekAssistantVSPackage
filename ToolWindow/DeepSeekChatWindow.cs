using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace DeepSeekAssistantVSPackage.ToolWindows
{
    [Guid("6c5b550d-8e4a-4a5c-8b5a-7e9f3c1d2a4b")] // Должен совпадать с ID в манифесте
    public class DeepSeekChatWindow : ToolWindowPane
    {
        public DeepSeekChatWindow() : base(null)
        {
            this.Caption = "DeepSeek Chat";
            this.BitmapResourceID = 4001; // Должен совпадать с Icon в манифесте
            this.BitmapIndex = 1;

            // Создаем и устанавливаем WPF контрол
            var control = new DeepSeekChatWindowControl();
            this.Content = control;
        }
    }
}