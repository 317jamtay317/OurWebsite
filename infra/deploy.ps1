<#
.SYNOPSIS
    Builds, pushes, and rolls out a new image for the ProManager Online site Container App.

.DESCRIPTION
    Routine deploy, matching how the other apps in the ProjectManagement resource group are managed
    (billingagent, mcpserver, …). Run from a console signed in to BOTH:
      • Azure       — `az login`
      • Docker Hub  — `docker login`  (to push the private image)

    The Container App, its secrets, and its config are created once with `az containerapp create`
    (see infra/README.md). This script only builds + pushes a new image and rolls it out, so no
    secrets are involved here. If the app does not exist yet, it tells you to run the one-time create.

.EXAMPLE
    ./infra/deploy.ps1
    ./infra/deploy.ps1 -Tag v2
    ./infra/deploy.ps1 -SkipBuild
#>
[CmdletBinding()]
param(
    [string]$ResourceGroup = 'ProjectManagement',
    [string]$AppName       = 'promanageronlinewebsite',
    [string]$DockerRepo    = 'jamtay317/promanageronline_website',
    [string]$Tag           = (Get-Date -Format 'yyyyMMddHHmmss'),
    [switch]$SkipBuild
)

$ErrorActionPreference = 'Stop'
$pushImage     = "${DockerRepo}:${Tag}"
$latestImage   = "${DockerRepo}:latest"
$registryImage = "registry.hub.docker.com/${DockerRepo}:${Tag}"

# Fail early with a helpful message if the app hasn't been created yet.
$existing = az containerapp show --name $AppName --resource-group $ResourceGroup --query name -o tsv 2>$null
if (-not $existing) {
    throw "Container App '$AppName' not found in '$ResourceGroup'. Run the one-time 'az containerapp create' from infra/README.md first."
}

if (-not $SkipBuild) {
    Write-Host "Building $pushImage ..." -ForegroundColor Cyan
    # --no-cache guarantees the current source is compiled in. A stale build-cache layer once shipped
    # an image missing the startup migration/seed code, so always build clean here.
    docker build --no-cache -t $pushImage -t $latestImage (Join-Path $PSScriptRoot '..')
    if ($LASTEXITCODE -ne 0) { throw 'docker build failed' }

    Write-Host "Pushing to Docker Hub ..." -ForegroundColor Cyan
    docker push $pushImage;   if ($LASTEXITCODE -ne 0) { throw 'docker push failed' }
    docker push $latestImage; if ($LASTEXITCODE -ne 0) { throw 'docker push (latest) failed' }
}

Write-Host "Rolling out $registryImage ..." -ForegroundColor Cyan
$fqdn = az containerapp update `
    --name $AppName `
    --resource-group $ResourceGroup `
    --image $registryImage `
    --query properties.configuration.ingress.fqdn -o tsv
if ($LASTEXITCODE -ne 0) { throw 'az containerapp update failed' }

Write-Host ""
Write-Host "Deployed: https://$fqdn" -ForegroundColor Green
