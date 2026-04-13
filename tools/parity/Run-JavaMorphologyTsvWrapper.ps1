param(
    [Parameter(Mandatory = $true)]
    [string]$JarPath,

    [Parameter(Mandatory = $true)]
    [string]$InputPath,

    [Parameter(Mandatory = $true)]
    [string]$OutputPath
)

$ErrorActionPreference = 'Stop'

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$javaFile = Join-Path $scriptDir 'JavaMorphologyTsvWrapper.java'
$classDir = Join-Path $scriptDir 'classes'

if (!(Test-Path $JarPath)) {
    throw "JarPath not found: $JarPath"
}

if (!(Test-Path $InputPath)) {
    throw "InputPath not found: $InputPath"
}

New-Item -ItemType Directory -Path $classDir -Force | Out-Null

javac -encoding UTF-8 -cp $JarPath -d $classDir $javaFile
if ($LASTEXITCODE -ne 0) {
    throw 'javac failed.'
}

$cp = "$JarPath;$classDir"
java -cp $cp JavaMorphologyTsvWrapper $InputPath $OutputPath
if ($LASTEXITCODE -ne 0) {
    throw 'java wrapper execution failed.'
}

Write-Host "Wrapper run completed: $OutputPath"
