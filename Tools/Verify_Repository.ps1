$ErrorActionPreference = "Stop"

Write-Host "Shadow Supply Repository Verification" -ForegroundColor Cyan
Write-Host ""

$required = @(
    "Assets",
    "Packages",
    "ProjectSettings",
    ".gitignore",
    ".gitattributes",
    "PROJECT_CONTEXT.md",
    "CURRENT_STATUS.md",
    "SYSTEM_MAP.md",
    "BUGS.md",
    "CHANGELOG.md",
    "CHATGPT_START_HERE.md"
)

$failed = $false

foreach ($item in $required) {
    if (Test-Path $item) {
        Write-Host "[OK] $item" -ForegroundColor Green
    } else {
        Write-Host "[MISSING] $item" -ForegroundColor Red
        $failed = $true
    }
}

Write-Host ""
if (Get-Command git -ErrorAction SilentlyContinue) {
    Write-Host "[OK] Git installed" -ForegroundColor Green
} else {
    Write-Host "[MISSING] Git" -ForegroundColor Red
    $failed = $true
}

if (Get-Command git-lfs -ErrorAction SilentlyContinue) {
    Write-Host "[OK] Git LFS installed" -ForegroundColor Green
} else {
    Write-Host "[MISSING] Git LFS" -ForegroundColor Red
    $failed = $true
}

if ($failed) {
    Write-Host ""
    Write-Host "Repository verification failed." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Repository structure is ready." -ForegroundColor Green
