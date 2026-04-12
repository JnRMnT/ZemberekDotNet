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

# Copy docs/*.md into the wiki (flat), but skip docs/README.md —
# it is a docs index, not meant to be a standalone wiki page.
Get-ChildItem "$docsDir\*.md" | Where-Object { $_.Name -ne "README.md" } | ForEach-Object {
    Copy-Item $_.FullName $wikiDir -Force
}

# Remove any stale README.md that may have been copied in a previous run
$staleReadme = Join-Path $wikiDir "README.md"
if (Test-Path $staleReadme) { Remove-Item $staleReadme -Force }

# docs/wiki-home-port.md becomes the wiki Home page
$wikiHome = Join-Path $docsDir "wiki-home-port.md"
Copy-Item $wikiHome (Join-Path $wikiDir "Home.md") -Force

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
