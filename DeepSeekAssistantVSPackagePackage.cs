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
    [ProvideOptionPage(
        typeof(Options.DeepSeekOptionsPage),
        "DeepSeek Assistant",
        "General",
        0,
        0,
        true)]
    // Используем константу из PackageGuids
    [Guid(PackageGuids.DeepSeekAssistantVSPackagePackageString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(DeepSeekChatWindow))]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideOptionPage(typeof(Options.DeepSeekOptionsPage), "DeepSeek Assistant", "General", 0, 0, true)]
    [ProvideOptionPage(typeof(Options.LogOptions), "DeepSeek Assistant", "Debug", 1, 0, true)]
    #endregion
    public sealed class DeepSeekAssistantVSPackagePackage : AsyncPackage
    {
        // ВОССТАНАВЛИВАЕМ константы - они НУЖНЫ!
        public const string PackageGuidString = PackageGuids.DeepSeekAssistantVSPackagePackageString;
        public const string PackageCommandSetGuidString = PackageGuids.DeepSeekAssistantVSPackagePackageCmdSetString;
        public const string DeepSeekChatToolWindowString = PackageGuids.DeepSeekChatToolWindowString;

        // Добавляем константу для страницы настроек
        public const string DeepSeekOptionsPageGuidString = PackageGuids.DeepSeekOptionsPageString;
        public const string DeepSeekLogOptionsPage = PackageGuids.DeepSeekLogOptionsPageString;

        #region Package Members

        private static DeepSeekAssistantVSPackagePackage _instance;
        public static DeepSeekAssistantVSPackagePackage Instance => _instance;

        protected override async System.Threading.Tasks.Task InitializeAsync(
            CancellationToken cancellationToken,
            IProgress<ServiceProgressData> progress)
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