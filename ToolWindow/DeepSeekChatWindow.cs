using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace DeepSeekAssistantVSPackage.ToolWindows
{
    [Guid("3a13c9e9-2072-4ae1-8d25-0f2b1c14fc1d")]
    public class DeepSeekChatWindow : ToolWindowPane
    {
        public DeepSeekChatWindow() : base(null)
        {
            this.Caption = "DeepSeek Chat";
            this.Content = new DeepSeekChatWindowControl();
        }

        public const string WindowGuid = "3a13c9e9-2072-4ae1-8d25-0f2b1c14fc1d";

        /// <summary>
        /// Показывает окно DeepSeek Chat.
        /// </summary>
        public static async Task<DeepSeekChatWindow> ShowAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            // Получаем экземпляр окна через стандартный механизм Visual Studio
            var window = await AsyncPackage.FindToolWindowAsync(
                typeof(DeepSeekChatWindow),
                id: 0, // ID окна (0 для единственного экземпляра)
                create: true, // Создать окно, если оно не существует
                cancellationToken: CancellationToken.None
            ) as DeepSeekChatWindow;

            if (window?.Frame == null)
            {
                throw new NotSupportedException("Не удалось создать окно инструментов.");
            }

            // Показываем окно
            var windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());

            return window;
        }

        /// <summary>
        /// Перегрузка метода для вызова из команд с передачей пакета.
        /// </summary>
        public static async Task<DeepSeekChatWindow> ShowAsync(AsyncPackage package)
        {
            // Для совместимости вызываем основной метод
            return await ShowAsync();
        }
    }
}