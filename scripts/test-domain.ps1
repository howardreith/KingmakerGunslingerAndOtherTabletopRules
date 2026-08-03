[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',

    [string]$MSBuildPath,

    [switch]$Clean
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common.ps1')
$repositoryRoot = Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot
$projectPath = Join-Path $repositoryRoot 'tests\KingmakerGunslinger.DomainTests\KingmakerGunslinger.DomainTests.csproj'

& (Join-Path $PSScriptRoot 'validate-repository.ps1')

$msbuild = Resolve-KmgMsBuild -ExplicitPath $MSBuildPath
$processPath = $env:Path
[Environment]::SetEnvironmentVariable('PATH', $null, 'Process')
[Environment]::SetEnvironmentVariable('Path', $processPath, 'Process')
$target = if ($Clean) { 'Rebuild' } else { 'Build' }
$arguments = @(
    $projectPath,
    '/nologo',
    "/t:$target",
    "/p:Configuration=$Configuration",
    '/p:Platform=AnyCPU',
    '/verbosity:minimal'
)

Write-Host "Building dependency-free domain/reflection tests ($Configuration) with $msbuild"
& $msbuild @arguments
if ($LASTEXITCODE -ne 0) {
    throw "Domain test build failed with exit code $LASTEXITCODE."
}

$testExecutable = Join-Path $repositoryRoot "artifacts\tests\$Configuration\KingmakerGunslinger.DomainTests\KingmakerGunslinger.DomainTests.exe"
if (-not (Test-Path -LiteralPath $testExecutable -PathType Leaf)) {
    throw "Domain test executable was not produced: $testExecutable"
}

Write-Host "Running dependency-free domain/reflection tests: $testExecutable"
& $testExecutable
if ($LASTEXITCODE -ne 0) {
    throw "Domain tests failed with exit code $LASTEXITCODE."
}

Write-Host 'Dependency-free domain/reflection tests passed.'
