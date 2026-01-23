using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace DeepSeekAssistantVSPackage.Providers
{
    [Guid("8a4b1d3c-7e9f-4a5c-8b5a-6c5b550d2a4b")]
    public class DeepSeekChatWindowProvider
    {
        [Microsoft.VisualStudio.Shell.ProvideToolWindow(typeof(ToolWindows.DeepSeekChatWindow))]
        public void RegisterToolWindow()
        {
            // Регистрация через атрибут
        }
    }
}