namespace VSExtensions.MinikubeGenerator.Configuration
{
    public class MinikubeConfiguration
    {
        public MinikubeConfiguration(string pathDeployMinikubeBATScript, string pathDeploymentsYAMLScript, string solutionDir, string nameImageDocker, string nameDockerFile, string namespaceKubernetes, string pathBaseProject)
        {
            PathDeployMinikubeBATScript = pathDeployMinikubeBATScript;
            PathDeploymentsYAMLScript = pathDeploymentsYAMLScript;
            SolutionDir = solutionDir;
            NameImageDocker = nameImageDocker;
            NameDockerFile = nameDockerFile;
            NamespaceKubernetes = namespaceKubernetes;
            PathBaseProject = pathBaseProject;
        }

        public string PathDeployMinikubeBATScript { get; set; }
        public string PathDeploymentsYAMLScript { get; set; }
        public string SolutionDir { get; set; }
        public string NameImageDocker { get; set; }
        public string NameDockerFile { get; set; }
        public string NamespaceKubernetes { get; set; }
        public string PathBaseProject { get; set; }
    }
}
