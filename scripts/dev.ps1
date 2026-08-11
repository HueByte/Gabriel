# =============================================================================
# dev.ps1 — the one-command dev environment.
#
# Usage:
#   scripts/dev.ps1 up                  # API (dotnet watch) + webapp (vite), each in its own window
#   scripts/dev.ps1 up -SkipWeb         # API only
#   scripts/dev.ps1 up -SkipApi        # webapp only
#   scripts/dev.ps1 up -Docker          # containerized stack via docker/docker-compose.yml
#   scripts/dev.ps1 up -Docker -Build   # ...forcing an image rebuild
#   scripts/dev.ps1 down                # stop whatever `up` started (process tree kill via pid file)
#   scripts/dev.ps1 down -Docker        # docker compose down
#   scripts/dev.ps1 down -Docker -RemoveVolumes   # ...and wipe named volumes (clean reset: DB, logs, uploads)
#
# The happy path is zero-flag; every deviation is explicit.
#
# Gotchas encoded here so nobody re-learns them:
#   - The compose file lives in docker/, but .env lives at the repo root, so
#     compose MUST be invoked with --env-file for ${VAR} substitution to work.
#   - `docker compose build webapp` COPYs the gitignored-but-generated OpenAPI
#     client; run a host `dotnet build` first so it exists (the API's MSBuild
#     target emits it).
#   - `dotnet watch` holds DLL locks: a plain `dotnet build` while `up` is
#     running fails at the copy step. `down` first.
# =============================================================================
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [ValidateSet("up", "down")]
    [string]$Action,

    [switch]$SkipApi,
    [switch]$SkipWeb,
    [switch]$Docker,
    [switch]$Build,
    [switch]$RemoveVolumes
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDir = Split-Path -Parent $ScriptDir
$ComposeFile = Join-Path $RootDir "docker/docker-compose.yml"
$EnvFile = Join-Path $RootDir ".env"
$PidFile = Join-Path $ScriptDir ".gabriel-dev.pids"

# ============================= UP ============================================
if ($Action -eq "up") {
    if ($Docker) {
        if (-not (Test-Path $EnvFile)) {
            throw "No .env at repo root. Copy .env.example to .env and fill in the REPLACE_ME values first."
        }
        $composeArgs = @("compose", "-f", $ComposeFile, "--env-file", $EnvFile, "up", "-d")
        if ($Build) { $composeArgs += "--build" }
        & docker @composeArgs
        Write-Host "Stack up. API on http://localhost:6040 (or API_PORT), webapp on http://localhost:6080 (or WEB_PORT)."
        exit 0
    }

    $pids = @()

    if (-not $SkipApi) {
        $api = Start-Process -PassThru -WorkingDirectory (Join-Path $RootDir "src/api/Gabriel.API") `
            -FilePath "dotnet" -ArgumentList "watch", "run"
        $pids += $api.Id
        Write-Host "API starting (dotnet watch, pid $($api.Id))."
    }

    if (-not $SkipWeb) {
        # npm is a .cmd shim on Windows — launch through cmd for a real process.
        $web = Start-Process -PassThru -WorkingDirectory (Join-Path $RootDir "src/webapp") `
            -FilePath "cmd" -ArgumentList "/c", "npm run dev"
        $pids += $web.Id
        Write-Host "Webapp starting (vite, pid $($web.Id))."
    }

    if ($pids.Count -gt 0) {
        $pids -join "," | Set-Content $PidFile
        Write-Host "Pids recorded in $PidFile — stop everything with: scripts/dev.ps1 down"
    }
    exit 0
}

# ============================= DOWN ==========================================
if ($Docker) {
    $composeArgs = @("compose", "-f", $ComposeFile, "down")
    if (Test-Path $EnvFile) { $composeArgs = @("compose", "-f", $ComposeFile, "--env-file", $EnvFile, "down") }
    if ($RemoveVolumes) { $composeArgs += "-v" }
    & docker @composeArgs
    exit 0
}

if (-not (Test-Path $PidFile)) {
    Write-Host "No pid file ($PidFile) — nothing recorded as running."
    exit 0
}

# taskkill /T kills the whole tree — `dotnet watch` and npm both spawn
# children that Stop-Process alone would orphan.
foreach ($procId in (Get-Content $PidFile) -split ",") {
    try {
        & taskkill /PID $procId /T /F 2>$null | Out-Null
        Write-Host "Stopped process tree $procId."
    } catch {
        Write-Host "Process $procId already gone."
    }
}
Remove-Item $PidFile -Confirm:$false
