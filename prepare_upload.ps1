# prepare_upload.ps1
# This script copies modified, staged, untracked files, and files from recent commits to a "ToBeUpload" folder.

param (
    [int]$CommitCount = 0
)

$sourceRoot = Get-Location
$destinationRoot = Join-Path $sourceRoot "ToBeUpload"

Write-Host "Collecting files for upload..." -ForegroundColor Cyan

# 1. Get Uncommitted changes (Modified + Staged)
$files = @(git diff --name-only HEAD)

# 2. Get Untracked files
$files += @(git ls-files --others --exclude-standard)

# 3. Get files from previous commits if requested
if ($CommitCount -gt 0) {
    Write-Host "Including files from the last $CommitCount commit(s)..." -ForegroundColor Gray
    $files += @(git diff --name-only "HEAD~$CommitCount" HEAD)
}

# Remove duplicates and empty entries
$files = $files | Where-Object { $_ -ne "" } | Select-Object -Unique

if ($files.Count -eq 0) {
    Write-Host "No changed files found in Git." -ForegroundColor Yellow
    exit
}

# Ensure destination exists
if (Test-Path $destinationRoot) {
    Write-Host "Clearing existing content in $destinationRoot..." -ForegroundColor Gray
    Get-ChildItem -Path $destinationRoot | Remove-Item -Recurse -Force
} else {
    New-Item -ItemType Directory -Path $destinationRoot | Out-Null
}

$count = 0
foreach ($relativePath in $files) {
    # Normalize path for Windows
    $normalizedPath = $relativePath.Replace("/", "\")
    $sourceFile = Join-Path $sourceRoot $normalizedPath
    
    if (Test-Path $sourceFile -PathType Leaf) {
        $targetFile = Join-Path $destinationRoot $normalizedPath
        $targetDir = Split-Path $targetFile -Parent
        
        # Create directory structure
        if (!(Test-Path $targetDir)) {
            New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
        }
        
        # Copy file
        Copy-Item -Path $sourceFile -Destination $targetFile -Force
        Write-Host "Copied: $relativePath" -ForegroundColor Green
        $count++
    } else {
        Write-Host "Skipped (not found or directory): $relativePath" -ForegroundColor Gray
    }
}

Write-Host "`nTotal files collected: $count" -ForegroundColor Cyan
Write-Host "Files are ready in: $destinationRoot" -ForegroundColor Cyan
