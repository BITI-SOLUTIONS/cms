# deploy.ps1
param(
    [string]$Service = "ui"
)

$Registry = "registry.biti-solutions.com"
$Namespace = "cms"

Write-Host " Building $Service..." -ForegroundColor Yellow
if ($Service -eq "ui") {
    docker build -t "${Registry}/cms-ui:latest" -f Dockerfile.UI .
} else {
    docker build -t "${Registry}/cms-api:latest" -f Dockerfile .
}

if ($LASTEXITCODE -ne 0) {
    Write-Host " Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host " Pushing to Harbor..." -ForegroundColor Yellow
if ($Service -eq "ui") {
    docker push "${Registry}/cms-ui:latest"
} else {
    docker push "${Registry}/cms-api:latest"
}

Write-Host " Deploying to Kubernetes..." -ForegroundColor Green
$DeploymentName = "cms-$Service-deployment"
ssh root@147.182.204.86 "kubectl set image deployment/$DeploymentName cms-$Service=${Registry}/cms-${Service}:latest --namespace=$Namespace && kubectl rollout status deployment/$DeploymentName -n $Namespace"

Write-Host " Deploy completado!" -ForegroundColor Green
