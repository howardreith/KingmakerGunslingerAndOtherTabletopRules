[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common.ps1')
$repositoryRoot = Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot
$validator = Join-Path $repositoryRoot 'tools\validate_sprint29.py'
if (-not (Test-Path -LiteralPath $validator -PathType Leaf)) {
    throw "Sprint 29 validator is missing: $validator"
}

$python = Get-Command python -ErrorAction SilentlyContinue
if ($null -eq $python) {
    $python = Get-Command python3 -ErrorAction SilentlyContinue
}
if ($null -eq $python) {
    throw 'Python 3 is required to run tools\validate_sprint29.py.'
}

& $python.Source $validator
if ($LASTEXITCODE -ne 0) {
    throw "Sprint 29 portable validation failed with exit code $LASTEXITCODE."
}

Write-Host 'Sprint 29 repository validation passed.'
