@ECHO OFF

cd %solutionDir
ECHO "Start Docker-env"
minikube docker-env
minikube -p minikube docker-env
ECHO "Finish Docker-env"
ECHO "Start Docker Build"
docker rmi %nameImageDocker --force
docker build -f %dockerFile -t %nameImageDocker .
ECHO "Finish Docker Build"
ECHO "Start Minikube Deploy"
kubectl delete -n %namespaceKubernetes deployment %nameImageDocker
kubectl delete -n %namespaceKubernetes service %nameImageDocker
kubectl apply -n %namespaceKubernetes -f %deploymentsPath
kubectl expose deployment %nameImageDocker -n %namespaceKubernetes --type=NodePort
minikube service %nameImageDocker -n %namespaceKubernetes --url
ECHO "Finish Minikube Deploy"

:SUCESSO
EXIT /B 0

:END
