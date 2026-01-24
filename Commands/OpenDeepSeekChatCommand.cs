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
            // Используем перегрузку без параметра - будет использоваться параметр по умолчанию (null)
            // Пакет будет найден автоматически внутри ShowAsync через ServiceProvider.GlobalProvider
            _ = ShowWindowAsync();
        }

        private static async Task ShowWindowAsync()
        {
            try
            {
                await DeepSeekChatWindow.ShowAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DeepSeek] Ошибка открытия окна: {ex.Message}");
                // Можно показать сообщение пользователю через MessageBox
            }
        }
    }
}