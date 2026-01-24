using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using DeepSeekAssistantVSPackage.ToolWindows;
using DeepSeekAssistantVSPackage.Commands;

namespace DeepSeekAssistantVSPackage
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(DeepSeekAssistantVSPackagePackage.PackageGuidString)]
    [ProvideToolWindow(typeof(DeepSeekChatWindow))]
    public sealed class DeepSeekAssistantVSPackagePackage : AsyncPackage
    {
        public const string PackageGuidString = "b6216745-f7c2-486a-848d-5d89b93666f9";

        #region Package Members

        protected override async System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            // Инициализируем команду открытия окна
            await OpenDeepSeekChatCommand.InitializeAsync(this);
        }

        #endregion
    }
}