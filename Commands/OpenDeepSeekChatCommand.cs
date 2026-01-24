using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.Threading;
using System.Threading.Tasks;
using DeepSeekAssistantVSPackage.ToolWindows;
using ThreadTask = System.Threading.Tasks.Task;

namespace DeepSeekAssistantVSPackage.Commands
{
    internal sealed class OpenDeepSeekChatCommand
    {
        public static async ThreadTask InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var cmdId = new CommandID(PackageGuids.DeepSeekAssistantVSPackagePackageCmdSet, PackageIds.OpenDeepSeekChatCommandId);
                var cmd = new MenuCommand(Execute, cmdId);
                commandService.AddCommand(cmd);
            }

            // Явно возвращаем управление, устраняя CS0161
            await ThreadTask.CompletedTask;
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