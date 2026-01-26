using DeepSeekAssistantVSPackage.ToolWindows;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;

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
        public static async System.Threading.Tasks.Task InitializeAsync(AsyncPackage package)
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

            await System.Threading.Tasks.Task.CompletedTask;
        }

        private static void Execute(object sender, EventArgs e)
        {
            _ = ShowWindowAsync();
        }

        private static async System.Threading.Tasks.Task ShowWindowAsync()
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