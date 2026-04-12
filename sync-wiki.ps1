param(
    [string]$SolutionRoot = $PSScriptRoot
)

# Skip on any CI environment
if ($env:CI -eq 'true' -or $env:TF_BUILD -eq 'True' -or $env:GITHUB_ACTIONS -eq 'true') {
    Write-Host "[sync-wiki] Skipping: running on CI."
    exit 0
}

$docsDir  = Join-Path $SolutionRoot "docs"
$wikiDir  = Join-Path (Split-Path $SolutionRoot -Parent) "ZemberekDotNet.wiki"

# Ensure git is available
if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
    Write-Warning "[sync-wiki] git not found on PATH. Skipping wiki sync."
    exit 0
}

# Clone wiki if not present, otherwise pull latest
if (-not (Test-Path $wikiDir)) {
    Write-Host "[sync-wiki] Cloning wiki repo to $wikiDir ..."
    git clone "https://github.com/JnRMnT/ZemberekDotNet.wiki.git" $wikiDir
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "[sync-wiki] Clone failed. Skipping wiki sync."
        exit 0
    }
} else {
    Push-Location $wikiDir
    git pull --quiet
    Pop-Location
}

# Copy all .md files from docs/ into the wiki (flat — wiki has no subdirectories)
Copy-Item "$docsDir\*.md" $wikiDir -Force

# Wiki landing page must be named Home.md
$readme = Join-Path $wikiDir "README.md"
$home   = Join-Path $wikiDir "Home.md"
if (Test-Path $readme) {
    if (Test-Path $home) { Remove-Item $home -Force }
    Rename-Item $readme "Home.md"
}

# Commit and push only if something changed
Push-Location $wikiDir
git add -A
$changed = git status --porcelain
if ($changed) {
    git commit -m "Auto-sync docs from main repo"
    git push
    Write-Host "[sync-wiki] Wiki updated."
} else {
    Write-Host "[sync-wiki] Wiki already up to date."
}
Pop-Location
