#!/bin/bash

set -e  # Exit on any error

# Simple Azure Infrastructure Setup
RESOURCE_GROUP_NAME="binwidthcalculator-rg"
LOCATION="eastus"
AKS_CLUSTER_NAME="binwidthcalculator-aks"
ACR_NAME="binwidthcalculatoracr"

# Function to check Azure login
check_azure_login() {
    echo "Checking Azure login status..."
    if ! az account show > /dev/null 2>&1; then
        echo "Please log in to Azure CLI..."
        az login
    fi
    echo "Logged in successfully."
}

# Function to register resource providers
register_providers() {
    echo "Registering required resource providers..."
    
    local providers=(
        "Microsoft.ContainerService"
        "Microsoft.ContainerRegistry"
        "Microsoft.Compute" 
        "Microsoft.Storage"
        "Microsoft.Network"
    )
    
    for provider in "${providers[@]}"; do
        echo "Checking $provider..."
        local status=$(az provider show --namespace "$provider" --query "registrationState" -o tsv 2>/dev/null || echo "NotRegistered")
        
        if [ "$status" != "Registered" ]; then
            echo "Registering $provider..."
            az provider register --namespace "$provider" --wait
            echo "✅ $provider registration completed"
        else
            echo "✅ $provider is already registered"
        fi
    done
    
    # Extra wait for propagation
    echo "Waiting for registration to propagate..."
    sleep 45
}

# Function to create resources
create_resources() {
    echo "Creating Resource Group..."
    az group create --name $RESOURCE_GROUP_NAME --location $LOCATION
    
    echo "Creating Azure Container Registry..."
    az acr create --resource-group $RESOURCE_GROUP_NAME --name $ACR_NAME --sku Basic --admin-enabled true
    
    echo "Creating AKS Cluster..."
    az aks create \
        --resource-group $RESOURCE_GROUP_NAME \
        --name $AKS_CLUSTER_NAME \
        --node-count 2 \
        --node-vm-size Standard_B2s \
        --enable-cluster-autoscaler \
        --min-count 1 \
        --max-count 3 \
        --generate-ssh-keys \
        --attach-acr $ACR_NAME
    
    echo "Getting AKS credentials..."
    az aks get-credentials --resource-group $RESOURCE_GROUP_NAME --name $AKS_CLUSTER_NAME --overwrite-existing
}

# Function to display outputs
display_outputs() {
    echo ""
    echo "=== SETUP COMPLETE ==="
    echo "Resource Group: $RESOURCE_GROUP_NAME"
    echo "AKS Cluster: $AKS_CLUSTER_NAME"
    echo "ACR: $ACR_NAME"
    
    # Get ACR credentials
    ACR_USERNAME=$(az acr credential show --name $ACR_NAME --query "username" -o tsv)
    ACR_PASSWORD=$(az acr credential show --name $ACR_NAME --query "passwords[0].value" -o tsv)
    
    echo ""
    echo "=== GITHUB SECRETS ==="
    echo "ACR_USERNAME: $ACR_USERNAME"
    echo "ACR_PASSWORD: $ACR_PASSWORD"
    echo "AKS_RESOURCE_GROUP: $RESOURCE_GROUP_NAME"
    echo "AKS_CLUSTER_NAME: $AKS_CLUSTER_NAME"
}

# Main execution
check_azure_login
register_providers
create_resources
display_outputs