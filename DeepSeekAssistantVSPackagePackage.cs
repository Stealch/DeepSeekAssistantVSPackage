using DeepSeekAssistantVSPackage.Commands;
using DeepSeekAssistantVSPackage.ToolWindows;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;
using System.Threading;


namespace DeepSeekAssistantVSPackage
{
    #region Product Registration
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(
    "DeepSeek Assistant",
    "DeepSeek Assistant Visual Studio Package",
    "1.0")]
    //[Guid(PackageGuidString)]
    [Guid(PackageGuids.DeepSeekAssistantVSPackagePackageString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(DeepSeekChatWindow))]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    #endregion
    public sealed class DeepSeekAssistantVSPackagePackage : AsyncPackage
    {
        public const string PackageGuidString = PackageGuids.DeepSeekAssistantVSPackagePackageString;
        public const string PackageCommandSetGuidString = PackageGuids.DeepSeekAssistantVSPackagePackageCmdSetString;
        public const string DeepSeekChatToolWindowString = PackageGuids.DeepSeekChatToolWindowString;

        #region Package Members

        protected override async System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // КРИТИЧЕСКИ ВАЖНО: вызов базовой инициализации
            await base.InitializeAsync(cancellationToken, progress);

            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            // Инициализируем команду открытия окна
            await OpenDeepSeekChatCommand.InitializeAsync(this);
        }

        #endregion
    }
}