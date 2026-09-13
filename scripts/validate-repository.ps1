[CmdletBinding()]
param(
    [string]$RepositoryRoot
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common.ps1')
$repositoryRoot = if ($RepositoryRoot) {
    (Resolve-Path -LiteralPath $RepositoryRoot).Path
} else {
    Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot
}
$validator = Join-Path $repositoryRoot 'tools\validate_repository.py'
if (-not (Test-Path -LiteralPath $validator -PathType Leaf)) {
    throw "Version-aware repository validator is missing: $validator"
}

$python = Get-Command python -ErrorAction SilentlyContinue
if ($null -eq $python) {
    $python = Get-Command python3 -ErrorAction SilentlyContinue
}
if ($null -eq $python) {
    throw 'Python 3 is required to run tools\validate_repository.py.'
}

& $python.Source $validator --root $repositoryRoot
if ($LASTEXITCODE -ne 0) {
    throw "Repository validation failed with exit code $LASTEXITCODE."
}

& (Join-Path $PSScriptRoot 'Test-IconOverhaulAssets.ps1') `
    -RepositoryRoot $repositoryRoot
& (Join-Path $PSScriptRoot 'Test-IconPolishRound2Assets.ps1') `
    -RepositoryRoot $repositoryRoot

& $python.Source (Join-Path $repositoryRoot 'tools\validate_icon_catalog.py') --root $repositoryRoot
if ($LASTEXITCODE -ne 0) { throw 'Icon authoring catalog validation failed.' }
& $python.Source (Join-Path $repositoryRoot 'tools\test_icon_catalog.py')
if ($LASTEXITCODE -ne 0) { throw 'Icon catalog corruption fixtures failed.' }
& $python.Source (Join-Path $repositoryRoot 'tools\test_icon_runtime_evidence.py')
if ($LASTEXITCODE -ne 0) { throw 'Icon runtime evidence corruption fixtures failed.' }
& (Join-Path $PSScriptRoot 'Test-IconCensusControlRequest.ps1')

Write-Host 'Version-aware repository validation passed.'
