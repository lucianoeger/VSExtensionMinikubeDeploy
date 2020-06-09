using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace VSExtensions.MinikubeGenerator
{
    public partial class MinikubeGeneratorDialog : Window
    {
        private DTE2 _dte;
        public MinikubeGeneratorDialog(DTE2 dte)
        {
            _dte = dte;
            InitializeComponent();

            Loaded += (s, e) =>
            {
                Title = Vsix.Name;
                windowMinikubeGenerator.SetResourceReference(BackgroundProperty, SystemColors.WindowBrush);
                windowMinikubeGenerator.SetResourceReference(ForegroundProperty, SystemColors.WindowTextBrush);
            };
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string nameDockerFile = txtNameDockerFile.Text;
            string nameImageDocker = txtNameImageDocker.Text;
            string namespaceKubernetes = txtNamespaceKubernetes.Text;

            new Thread(new ThreadStart(() =>
            {
                string solutionDir = Path.GetDirectoryName(_dte.Solution.FullName);
                string pathBase = Assembly.GetExecutingAssembly().Location.Replace("MinikubeGenerator.dll", "");
                string deployMinikubeScript = Path.Combine(pathBase, "Resources\\DeployMinikube.bat");
                string deploymentsScript = Path.Combine(pathBase, "Resources\\Deployments.yaml");

                File.WriteAllText(deployMinikubeScript, File.ReadAllText(deployMinikubeScript)
                    .Replace("%solutionDir", solutionDir)
                    .Replace("%dockerFile", $"\"{solutionDir}\\{nameDockerFile}\"")
                    .Replace("%nameImageDocker", nameImageDocker)
                    .Replace("%namespaceKubernetes", namespaceKubernetes)
                    .Replace("%deploymentsPath", $"\"{deploymentsScript}\""));

                File.WriteAllText(deploymentsScript, File.ReadAllText(deploymentsScript).Replace("%nameImageDocker", nameImageDocker));

                Process process = new Process();
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.CreateNoWindow = true;
                process.EnableRaisingEvents = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.Verb = "runas";
                process.StartInfo.FileName = deployMinikubeScript;
                process.OutputDataReceived += new DataReceivedEventHandler((s, a) =>
                {
                    WriteOutputWindow(a.Data);
                });
                process.ErrorDataReceived += new DataReceivedEventHandler((s, a) =>
                {
                    WriteOutputWindow(a.Data);
                });

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();
            })).Start();

            DialogResult = true;
            Close();
        }

        private void WriteOutputWindow(string message)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IVsOutputWindow outWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;

            Guid debugPaneGuid = VSConstants.GUID_OutWindowDebugPane;
            IVsOutputWindowPane generalPane;
            outWindow.GetPane(ref debugPaneGuid, out generalPane);

            generalPane.Activate();
            generalPane.OutputString($"Deploy Minikube: {message} \n");
        }
    }
}
