using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.Threading;
using System.Threading.Tasks;
using DeepSeekAssistantVSPackage.ToolWindows;

namespace DeepSeekAssistantVSPackage.Commands
{
    internal sealed class OpenDeepSeekChatCommand
    {
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Переключаемся на главный поток
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            // Получаем сервис команд меню
            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;

            if (commandService != null)
            {
                // Используем GUID и ID из централизованного класса PackageIds
                var cmdId = new CommandID(
                    PackageGuids.DeepSeekAssistantVSPackagePackageCmdSet,
                    PackageIds.OpenDeepSeekChatCommandId
                );

                // Создаем команду, привязанную к методу Execute
                var cmd = new MenuCommand(Execute, cmdId);
                commandService.AddCommand(cmd);
            }
        }

        private static void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            // Получаем пакет через глобальный сервис
            var package = ServiceProvider.GlobalProvider.GetService(typeof(DeepSeekAssistantVSPackagePackage)) as AsyncPackage;
            if (package != null)
            {
                _ = DeepSeekChatWindow.ShowAsync(package);
            }
        }
    }
}