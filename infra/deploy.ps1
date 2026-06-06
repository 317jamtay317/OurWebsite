<#
.SYNOPSIS
    Builds, pushes, and deploys ProManager Online to Azure Container Apps.

.DESCRIPTION
    Run this from a console where you are already signed in to BOTH:
      • Azure        — `az login`      (your account needs Owner on the resource group)
      • Docker Hub   — `docker login`  (to push the private image)

    All application secrets are read from Key Vault at runtime, so none are passed here. See
    infra/README.md for the one-time setup (resource group, Key Vault, and the secrets to add).

.EXAMPLE
    ./infra/deploy.ps1
    ./infra/deploy.ps1 -Tag v1 -EnableSmtp -EnableRecaptcha
#>
[CmdletBinding()]
param(
    [string]$ResourceGroup = 'rg-promanageronline-prod',
    [string]$Location      = 'eastus2',
    [string]$KeyVaultName  = 'kv-promanageronline',
    [string]$ImageRepo     = 'docker.io/jamtay317/promanageronline_website',
    [string]$Tag           = (Get-Date -Format 'yyyyMMddHHmmss'),
    [switch]$EnableSmtp,
    [switch]$EnableRecaptcha,
    [switch]$SkipBuild
)

$ErrorActionPreference = 'Stop'
$bicep = Join-Path $PSScriptRoot 'main.bicep'
$image = "${ImageRepo}:${Tag}"
$latest = "${ImageRepo}:latest"

if (-not $SkipBuild) {
    Write-Host "Building $image ..." -ForegroundColor Cyan
    # Build context is the repository root (one level up from infra/).
    docker build -t $image -t $latest (Join-Path $PSScriptRoot '..')
    if ($LASTEXITCODE -ne 0) { throw 'docker build failed' }

    Write-Host "Pushing to Docker Hub ..." -ForegroundColor Cyan
    docker push $image;  if ($LASTEXITCODE -ne 0) { throw 'docker push failed' }
    docker push $latest; if ($LASTEXITCODE -ne 0) { throw 'docker push (latest) failed' }
}

Write-Host "Ensuring resource group $ResourceGroup ..." -ForegroundColor Cyan
az group create --name $ResourceGroup --location $Location --output none
if ($LASTEXITCODE -ne 0) { throw 'az group create failed' }

Write-Host "Deploying infrastructure ..." -ForegroundColor Cyan
$url = az deployment group create `
    --resource-group $ResourceGroup `
    --template-file $bicep `
    --query properties.outputs.containerAppUrl.value -o tsv `
    --parameters `
        location=$Location `
        containerImage=$image `
        keyVaultName=$KeyVaultName `
        enableSmtp=$($EnableSmtp.IsPresent.ToString().ToLower()) `
        enableRecaptcha=$($EnableRecaptcha.IsPresent.ToString().ToLower())
if ($LASTEXITCODE -ne 0) { throw 'az deployment group create failed' }

Write-Host ""
Write-Host "Deployed: $url" -ForegroundColor Green
