using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Interop;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace VSExtensions.MinikubeGenerator
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", Vsix.Version, IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.guidMinikubeGeneratorPkgString)]
    public sealed class MinikubeGeneratorPackage : AsyncPackage
    {
        private DTE2 _dte;

        protected override async System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync();

            _dte = await GetServiceAsync(typeof(DTE)) as DTE2;

            if (await GetServiceAsync(typeof(IMenuCommandService)) is OleMenuCommandService mcs)
            {
                var menuCommandID = new CommandID(PackageGuids.guidMinikubeGeneratorCmdSet, PackageIds.cmdidMyCommand);
                var menuItem = new OleMenuCommand(ExecuteAsync, menuCommandID);
                mcs.AddCommand(menuItem);
            }
        }

        private async void ExecuteAsync(object sender, EventArgs e)
        {
            var dialog = new MinikubeGeneratorDialog(_dte);
            var hwnd = new IntPtr(_dte.MainWindow.HWnd);
            var window = (System.Windows.Window)HwndSource.FromHwnd(hwnd).RootVisual;
            dialog.Owner = window;
            dialog.ShowDialog();
        }
    }
}