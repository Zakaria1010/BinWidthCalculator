# Simple Azure Infrastructure Setup
param(
    [string]$ResourceGroupName = "binwidthcalculator-rg",
    [string]$Location = "eastus",
    [string]$AksClusterName = "binwidthcalculator-aks",
    [string]$AcrName = "binwidthcalculatoracr"
)

# Login to Azure
az login

# Create Resource Group
Write-Host "Creating Resource Group..." -ForegroundColor Green
az group create --name $ResourceGroupName --location $Location

# Create Azure Container Registry
Write-Host "Creating Azure Container Registry..." -ForegroundColor Green
az acr create --resource-group $ResourceGroupName --name $AcrName --sku Basic --admin-enabled true

# Create AKS Cluster
Write-Host "Creating AKS Cluster..." -ForegroundColor Green
az aks create `
    --resource-group $ResourceGroupName `
    --name $AksClusterName `
    --node-count 2 `
    --node-vm-size Standard_B2s `
    --enable-cluster-autoscaler `
    --min-count 1 `
    --max-count 3 `
    --generate-ssh-keys `
    --attach-acr $AcrName

# Get AKS credentials
Write-Host "Getting AKS credentials..." -ForegroundColor Green
az aks get-credentials --resource-group $ResourceGroupName --name $AksClusterName --overwrite-existing

# Output information
Write-Host "`n=== SETUP COMPLETE ===" -ForegroundColor Yellow
Write-Host "Resource Group: $ResourceGroupName" -ForegroundColor Cyan
Write-Host "AKS Cluster: $AksClusterName" -ForegroundColor Cyan
Write-Host "ACR: $AcrName" -ForegroundColor Cyan

# Get ACR credentials for GitHub
$acrUsername = az acr credential show --name $AcrName --query "username" -o tsv
$acrPassword = az acr credential show --name $AcrName --query "passwords[0].value" -o tsv

Write-Host "`n=== GITHUB SECRETS ===" -ForegroundColor Yellow
Write-Host "ACR_USERNAME: $acrUsername" -ForegroundColor Cyan
Write-Host "ACR_PASSWORD: [copy the password shown above]" -ForegroundColor Cyan
Write-Host "AKS_RESOURCE_GROUP: $ResourceGroupName" -ForegroundColor Cyan
Write-Host "AKS_CLUSTER_NAME: $AksClusterName" -ForegroundColor Cyan