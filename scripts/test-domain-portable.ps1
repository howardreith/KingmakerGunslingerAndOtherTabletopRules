[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    [switch]$KeepGeneratedProject
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$python = Get-Command python -ErrorAction SilentlyContinue
if (-not $python) {
    $python = Get-Command python3 -ErrorAction SilentlyContinue
}
if (-not $python) {
    throw 'Python 3 was not found on PATH.'
}
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw 'dotnet was not found on PATH. Install a .NET 8 SDK before running portable domain tests.'
}

$arguments = @(
    (Join-Path $root 'tools/run_portable_domain_tests.py'),
    '--configuration',
    $Configuration
)
if ($KeepGeneratedProject) {
    $arguments += '--keep-generated-project'
}

& $python.Source @arguments
if ($LASTEXITCODE -ne 0) {
    throw "Portable domain tests failed with exit code $LASTEXITCODE."
}
