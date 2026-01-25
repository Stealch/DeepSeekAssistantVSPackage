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
        private static void OnBeforeQueryStatus(object sender, EventArgs e)
        {
            var menuCommand = sender as OleMenuCommand;
            if (menuCommand != null)
            {
                // Всегда активна
                menuCommand.Enabled = true;
                menuCommand.Visible = true;
                menuCommand.Supported = true;
            }
        }
        public static async ThreadTask InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                CommandID cmdId = new CommandID(PackageGuids.DeepSeekAssistantVSPackagePackageCmdSet, PackageIds.OpenDeepSeekChatCommandId);

                OleMenuCommand cmd = new OleMenuCommand(Execute, cmdId)
                {
                    Enabled = true,
                    Visible = true
                };
                
                cmd.BeforeQueryStatus += OnBeforeQueryStatus;
                commandService.AddCommand(cmd);
            }

            await ThreadTask.CompletedTask;
        }

        private static void Execute(object sender, EventArgs e)
        {
            _ = ShowWindowAsync();
        }

        private static async ThreadTask ShowWindowAsync()
        {
            try
            {
                await DeepSeekChatWindow.ShowAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DeepSeek] Ошибка открытия окна: {ex.Message}");
            }
        }
    }
}