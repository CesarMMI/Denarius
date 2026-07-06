#Requires -Version 5.1
# Runs the Denarius API in the Development environment.
# Usage: .\run-api.ps1 [-NoBuild]

param(
    [switch]$NoBuild
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$ApiProject = Join-Path $RepoRoot "api\src\Denarius.Api"

$env:ASPNETCORE_ENVIRONMENT = "Development"

Write-Host "Starting Denarius API (Development)..." -ForegroundColor Cyan
Write-Host "Project: $ApiProject"

$dotnetArgs = @("run", "--project", $ApiProject)
if ($NoBuild) { $dotnetArgs += "--no-build" }

& dotnet @dotnetArgs
exit $LASTEXITCODE
