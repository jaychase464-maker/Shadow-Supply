param(
    [string]$RemoteUrl = ""
)

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "Shadow Supply Repository Initializer" -ForegroundColor Cyan
Write-Host "Run this script from the Unity project root." -ForegroundColor Yellow
Write-Host ""

if (-not (Test-Path "Assets") -or -not (Test-Path "Packages") -or -not (Test-Path "ProjectSettings")) {
    Write-Host "ERROR: Assets, Packages, or ProjectSettings was not found." -ForegroundColor Red
    Write-Host "Extract the import pack into the root of the Unity project, then run again."
    exit 1
}

if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
    Write-Host "ERROR: Git is not installed or not available in PATH." -ForegroundColor Red
    exit 1
}

if (-not (Get-Command git-lfs -ErrorAction SilentlyContinue)) {
    Write-Host "ERROR: Git LFS is not installed or not available in PATH." -ForegroundColor Red
    exit 1
}

git lfs install

if (-not (Test-Path ".git")) {
    git init
    git branch -M main
}

if ($RemoteUrl -ne "") {
    $existingRemote = git remote get-url origin 2>$null
    if ($LASTEXITCODE -eq 0) {
        git remote set-url origin $RemoteUrl
    } else {
        git remote add origin $RemoteUrl
    }
}

Write-Host ""
Write-Host "Checking ignored Unity folders..." -ForegroundColor Cyan

$generatedFolders = @("Library", "Temp", "Logs", "obj", "Build", "Builds", "UserSettings", ".vs")
foreach ($folder in $generatedFolders) {
    if (Test-Path $folder) {
        Write-Host "Ignored: $folder"
    }
}

Write-Host ""
Write-Host "Adding repository files..." -ForegroundColor Cyan
git add .gitignore .gitattributes README.md PROJECT_CONTEXT.md CURRENT_STATUS.md SYSTEM_MAP.md BUGS.md CHANGELOG.md CHATGPT_START_HERE.md Documentation Tools .github

Write-Host ""
Write-Host "Repository framework staged." -ForegroundColor Green
Write-Host "Review the files, then run:" -ForegroundColor Yellow
Write-Host '  git add Assets Packages ProjectSettings'
Write-Host '  git commit -m "chore: import Shadow Supply Unity project"'

if ($RemoteUrl -ne "") {
    Write-Host '  git push -u origin main'
}

Write-Host ""
Write-Host "IMPORTANT: Confirm that large binary files show as Git LFS pointers before pushing." -ForegroundColor Yellow
