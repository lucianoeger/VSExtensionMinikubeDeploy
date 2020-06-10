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
using VSExtensions.MinikubeGenerator.Configuration;

namespace VSExtensions.MinikubeDeploy
{
    public partial class MinikubeDeployDialog : Window
    {
        private DTE2 _dte;
        public MinikubeDeployDialog(DTE2 dte)
        {
            _dte = dte;
            InitializeComponent();

            Loaded += (s, e) =>
            {
                Title = Vsix.Name;
                windowMinikubeDeploy.SetResourceReference(BackgroundProperty, SystemColors.WindowBrush);
                windowMinikubeDeploy.SetResourceReference(ForegroundProperty, SystemColors.WindowTextBrush);
            };
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string nameDockerFile = txtNameDockerFile.Text;
                string nameImageDocker = txtNameImageDocker.Text;
                string namespaceKubernetes = txtNamespaceKubernetes.Text;
                string solutionDir = Path.GetDirectoryName(_dte.Solution.FullName);
                string pathBase = Assembly.GetExecutingAssembly().Location.Replace("MinikubeDeploy.dll", "");
                string pathDeployMinikubeScript = Path.Combine(pathBase, "Resources\\DeployMinikube.bat");
                string pathDeploymentsYAMLScript = Path.Combine(pathBase, "Resources\\Deployments.yaml");

                var configuration = new MinikubeConfiguration(pathDeployMinikubeScript, pathDeploymentsYAMLScript, solutionDir, nameImageDocker, nameDockerFile, namespaceKubernetes, pathBase);

                new Thread(new ThreadStart(() => { DeployMinikube(configuration); })).Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                DialogResult = true;
                Close();
            }
        }

        private void DeployMinikube(MinikubeConfiguration configuration)
        {
            string temporaryFileDeploymentsYAML = "", temporaryFileMinukubeDeployBAT = "";
            try
            {
                (temporaryFileDeploymentsYAML, temporaryFileMinukubeDeployBAT) = CreateAndReplaceFilesDeploy(configuration);

                Process process = new Process();
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.CreateNoWindow = true;
                process.EnableRaisingEvents = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.Verb = "runas";
                process.StartInfo.FileName = temporaryFileMinukubeDeployBAT;
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                RemoveTemporaryFilesDeploy(temporaryFileDeploymentsYAML, temporaryFileMinukubeDeployBAT);
            }
        }

        private (string temporaryFileDeploymentsYAML, string temporaryFileMinukubeDeployBAT) CreateAndReplaceFilesDeploy(MinikubeConfiguration configuration)
        {
            string temporaryFileDeploymentsYAML = $"{configuration.PathBaseProject}{Guid.NewGuid().ToString()}.yaml";
            File.Copy(configuration.PathDeploymentsYAMLScript, temporaryFileDeploymentsYAML);
            File.WriteAllText(temporaryFileDeploymentsYAML, File.ReadAllText(temporaryFileDeploymentsYAML).Replace("%nameImageDocker", configuration.NameImageDocker));

            string temporaryFileMinukubeDeployBAT = $"{configuration.PathBaseProject}{Guid.NewGuid().ToString()}.bat";
            File.Copy(configuration.PathDeployMinikubeBATScript, temporaryFileMinukubeDeployBAT);

            File.WriteAllText(temporaryFileMinukubeDeployBAT, File.ReadAllText(temporaryFileMinukubeDeployBAT)
                .Replace("%solutionDir", configuration.SolutionDir)
                .Replace("%dockerFile", $"\"{configuration.SolutionDir}\\{configuration.NameDockerFile}\"")
                .Replace("%nameImageDocker", configuration.NameImageDocker)
                .Replace("%namespaceKubernetes", configuration.NamespaceKubernetes)
                .Replace("%deploymentsPath", $"\"{temporaryFileDeploymentsYAML}\""));

            return (temporaryFileDeploymentsYAML, temporaryFileMinukubeDeployBAT);
        }

        private void RemoveTemporaryFilesDeploy(string temporaryFileDeploymentsYAML, string temporaryFileMinukubeDeployBAT)
        {
            File.Delete(temporaryFileDeploymentsYAML);
            File.Delete(temporaryFileMinukubeDeployBAT);
        }

        private void WriteOutputWindow(string message)
        {
            IVsOutputWindow outWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;

            Guid debugPaneGuid = VSConstants.GUID_OutWindowDebugPane;
            IVsOutputWindowPane generalPane;
            outWindow.GetPane(ref debugPaneGuid, out generalPane);

            generalPane.Activate();
            generalPane.OutputString($"Deploy Minikube: {message} \n");
        }
    }
}
