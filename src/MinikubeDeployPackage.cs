using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Interop;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace VSExtensions.MinikubeDeploy
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", Vsix.Version, IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.guidMinikubeDeployPkgString)]
    public sealed class MinikubeDeployPackage : AsyncPackage
    {
        private DTE2 _dte;

        protected override async System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync();

            _dte = await GetServiceAsync(typeof(DTE)) as DTE2;

            if (await GetServiceAsync(typeof(IMenuCommandService)) is OleMenuCommandService mcs)
            {
                var menuCommandID = new CommandID(PackageGuids.guidMinikubeDeployCmdSet, PackageIds.cmdidMyCommand);
                var menuItem = new OleMenuCommand(Execute, menuCommandID);
                mcs.AddCommand(menuItem);
            }
        }

        private void Execute(object sender, EventArgs e)
        {
            var dialog = new MinikubeDeployDialog(_dte);
            var hwnd = new IntPtr(_dte.MainWindow.HWnd);
            var window = (System.Windows.Window)HwndSource.FromHwnd(hwnd).RootVisual;
            dialog.Owner = window;
            dialog.ShowDialog();
        }
    }
}