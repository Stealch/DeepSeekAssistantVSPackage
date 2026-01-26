using DeepSeekAssistantVSPackage.Commands;
using DeepSeekAssistantVSPackage.ToolWindows;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

// DeepSeekAssistantVSPackagePackage.cs

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

        private static DeepSeekAssistantVSPackagePackage _instance;
        public static DeepSeekAssistantVSPackagePackage Instance => _instance;
        protected override async System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // КРИТИЧЕСКИ ВАЖНО: вызов базовой инициализации
            await base.InitializeAsync(cancellationToken, progress);

            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            _instance = this;
            AppDomain.CurrentDomain.AssemblyResolve += ResolveDeepSeekLib;

            // Инициализируем команду открытия окна
            await OpenDeepSeekChatCommand.InitializeAsync(this);
        }
        private static Assembly ResolveDeepSeekLib(object sender, ResolveEventArgs e)
        {
            string assemblyName = e.Name;
            if (assemblyName.StartsWith("DeepSeekAPILib", StringComparison.Ordinal))
                return null;

            var baseDir = Path.GetDirectoryName(typeof(DeepSeekAssistantVSPackagePackage).Assembly.Location);
            var dllPath = Path.Combine(baseDir, "Libs", "DeepSeekAPILib.dll");

            return File.Exists(dllPath)
                ? Assembly.LoadFrom(dllPath)
                : null;
        }

        #endregion
    }
}