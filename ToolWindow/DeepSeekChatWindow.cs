using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
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
        public static async Task<DeepSeekChatWindow> ShowAsync(AsyncPackage package = null)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            // Получаем экземпляр пакета для вызова FindToolWindowAsync
            AsyncPackage targetPackage = package;
            if (targetPackage == null)
            {
                // Пытаемся получить пакет через глобальный провайдер услуг
                var serviceProvider = ServiceProvider.GlobalProvider;
                targetPackage = serviceProvider?.GetService(typeof(DeepSeekAssistantVSPackagePackage)) as AsyncPackage ?? serviceProvider?.GetService(typeof(AsyncPackage)) as AsyncPackage;

                    // Последняя попытка: получаем любой AsyncPackage

            }

            if (targetPackage == null)
            {
                throw new InvalidOperationException("Не удалось получить экземпляр пакета для открытия окна.");
            }

            // Получаем экземпляр окна
            var window = await targetPackage.FindToolWindowAsync(
                typeof(DeepSeekChatWindow),
                id: 0,
                create: true,
                cancellationToken: CancellationToken.None
            ) as DeepSeekChatWindow;

            if (window?.Frame == null)
            {
                throw new NotSupportedException("Не удалось создать окно инструментов.");
            }

            // Показываем окно
            var windowFrame = (Microsoft.VisualStudio.Shell.Interop.IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());

            return window;
        }

        /// <summary>
        /// Перегрузка метода для вызова из команд с передачей пакета.
        /// </summary>
//       public static async Task<DeepSeekChatWindow> ShowAsync(AsyncPackage package)
//       {
            // Для совместимости вызываем основной метод
//           return await ShowAsync();
//       }
    }
}