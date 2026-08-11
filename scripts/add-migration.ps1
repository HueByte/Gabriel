# =============================================================================
# add-migration.ps1 — wraps `dotnet ef migrations add` with the two project
# flags this repo always needs (migrations live in Gabriel.Infrastructure,
# the startup/DI root is Gabriel.API) so nobody gets them wrong.
#
# Usage:
#   scripts/add-migration.ps1 AddWidgetTable
#
# Uses the pinned dotnet-ef from src/api/dotnet-tools.json — the repo's ONE
# tool manifest (the OpenAPI build target restores from src/api too) — so no
# global tool install is required.
# =============================================================================
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$Name
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDir = Split-Path -Parent $ScriptDir

# Run from src/api so `dotnet tool restore` resolves the manifest there.
Push-Location (Join-Path $RootDir "src/api")
try {
    dotnet tool restore | Out-Null
    dotnet dotnet-ef migrations add $Name `
        --project Gabriel.Infrastructure `
        --startup-project Gabriel.API
} finally {
    Pop-Location
}
