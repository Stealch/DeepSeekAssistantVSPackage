using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using DeepSeekAssistantVSPackage.ToolWindows;
using DeepSeekAssistantVSPackage.Commands;
using ThreadTask = System.Threading.Tasks.Task;

namespace DeepSeekAssistantVSPackage
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(DeepSeekAssistantVSPackagePackage.PackageGuidString)]
    [ProvideToolWindow(typeof(DeepSeekChatWindow))]
    public sealed class DeepSeekAssistantVSPackagePackage : AsyncPackage
    {
        public const string PackageGuidString = "b6216745-f7c2-486a-848d-5d89b93666f9";
		public const string PackageCommandSetGuidString = "2c93c2f5-3df3-44be-b847-cfea5db0fd6d";

        #region Package Members

        protected override async ThreadTask InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
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