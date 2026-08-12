# =============================================================================
# download-embedding-model.ps1 — fetches the local embedding model used by
# Embeddings:Provider=Local (semantic memory).
#
# Model: sentence-transformers/all-MiniLM-L6-v2 (Apache-2.0), ONNX export +
# BERT WordPiece vocab, ~90 MB total. Files land in
# models/embeddings/all-MiniLM-L6-v2/ at the repo root — the path
# LocalOnnxEmbeddingProvider probes by default (models/ is gitignored; model
# binaries never enter git history).
#
# Usage:
#   scripts/download-embedding-model.ps1              # default location
#   scripts/download-embedding-model.ps1 -Force       # re-download existing
# =============================================================================
param(
    [string]$TargetDir,
    [switch]$Force
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDir = Split-Path -Parent $ScriptDir

if (-not $TargetDir) {
    $TargetDir = Join-Path $RootDir "models/embeddings/all-MiniLM-L6-v2"
}

$Files = @(
    @{ Name = "model.onnx"; Url = "https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2/resolve/main/onnx/model.onnx"; MinBytes = 10MB },
    @{ Name = "vocab.txt";  Url = "https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2/resolve/main/vocab.txt";       MinBytes = 100KB }
)

New-Item -ItemType Directory -Force $TargetDir | Out-Null

foreach ($file in $Files) {
    $dest = Join-Path $TargetDir $file.Name
    if ((Test-Path $dest) -and -not $Force) {
        Write-Host "SKIP  $($file.Name) already present ($([math]::Round((Get-Item $dest).Length / 1MB, 1)) MB) — use -Force to re-download"
        continue
    }

    Write-Host "GET   $($file.Url)"
    # HF resolve/ URLs redirect to the CDN; Invoke-WebRequest follows them.
    Invoke-WebRequest -Uri $file.Url -OutFile $dest -UseBasicParsing

    $size = (Get-Item $dest).Length
    if ($size -lt $file.MinBytes) {
        Remove-Item $dest -Force
        throw "$($file.Name) came back suspiciously small ($size bytes) — likely an error page, not the file. Check the URL / your network."
    }
    Write-Host "OK    $($file.Name) ($([math]::Round($size / 1MB, 1)) MB)"
}

Write-Host ""
Write-Host "Model ready at: $TargetDir"
Write-Host "Enable with:    Embeddings:Provider = 'Local' (plus SemanticMemory:Enabled = true)"
