[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',

    [string]$MSBuildPath,
    # The production DLL a caller has just compiled (build.ps1, Invoke-KmgGate). When the loader-reference case is
    # selected and no DLL is given, this script compiles production of this checkout first.
    [string]$ProductionDll,
    # The SHA-256 the caller measured when its compile finished. The case inspects a private copy whose digest must equal
    # it, so bytes replacing the shared output while the test runner is prepared are refused, never inspected.
    [string]$ProductionDllSha256,
    # Build-Local: the loader-reference case is run later on the exact compiler output; this run neither compiles
    # production nor binds a DLL (the case reports deferred).
    [switch]$LoaderCaseAfterExactBuild,

    [switch]$Clean,

    # For a caller that has already validated the repository itself, and for
    # the inner loop. Build-Local.ps1 passes it because it runs the wrapper
    # directly one line earlier, which is how a candidate pipeline validates
    # the repository exactly once; Invoke-KmgGate.ps1 passes it at the Focused
    # level, where the wrapper is deferred to the sprint candidate boundary.
    # No qualification level reaches the complete suite without the wrapper.
    [switch]$SkipRepositoryValidation,

    # Records the runner's own standard output and error in these files as
    # well as the console, for a caller that must keep the selected-test
    # evidence. The runner's exit code still decides the outcome.
    [string]$RunnerLogPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common.ps1')
$repositoryRoot = Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot
$projectPath = Join-Path $repositoryRoot 'tests\KingmakerGunslinger.DomainTests\KingmakerGunslinger.DomainTests.csproj'

if ($SkipRepositoryValidation) {
    Write-Host 'Repository validation skipped: the caller runs it, or this is a focused run.'
} else {
    & (Join-Path $PSScriptRoot 'validate-repository.ps1')
}

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

# loader.production-dll-references-pinned-loader inspects a verified compiler output: the given production DLL, or one
# compiled here, bound by path and SHA-256, with the expected identities read by the existing pin reader.
$loaderCase = 'loader.production-dll-references-pinned-loader'
$filterTerms = @(([string]$env:KMG_TEST_FILTER).Split(@(';', ','), [StringSplitOptions]::RemoveEmptyEntries) | ForEach-Object { $_.Trim() } | Where-Object { $_ })
$selectsLoaderCase = $filterTerms.Count -eq 0 -or @($filterTerms | Where-Object { $loaderCase.IndexOf($_, [StringComparison]::OrdinalIgnoreCase) -ge 0 }).Count -gt 0
$bindingNames = @('KMG_PRODUCTION_DLL', 'KMG_PRODUCTION_DLL_SHA256', 'KMG_EXPECTED_LOADER_REFERENCES', 'KMG_LOADER_CASE_DEFERRED')
$previousBinding = @{}; foreach ($name in $bindingNames) { $previousBinding[$name] = [Environment]::GetEnvironmentVariable($name, 'Process') }
$privateProduction = $null
# Every binding change, the private copy and its digest check, and the runner share this cleanup scope: the caller's
# variables are restored and the private copy is removed whatever happens.
try {
    foreach ($name in $bindingNames) { [Environment]::SetEnvironmentVariable($name, $null, 'Process') }
    if ($LoaderCaseAfterExactBuild) { [Environment]::SetEnvironmentVariable('KMG_LOADER_CASE_DEFERRED', '1', 'Process') }
    if ($selectsLoaderCase -and -not $LoaderCaseAfterExactBuild) {
        if (-not $PSBoundParameters.ContainsKey('ProductionDll')) {
            $compileArguments = @{ Configuration = $Configuration; CompileOnly = $true }
            if ($MSBuildPath) { $compileArguments.MSBuildPath = $MSBuildPath }
            & (Join-Path $PSScriptRoot 'build.ps1') @compileArguments
            $ProductionDll = Join-Path $repositoryRoot "artifacts\bin\$Configuration\KingmakerGunslinger\KingmakerGunslinger.dll"
            $ProductionDllSha256 = if (Test-Path -LiteralPath $ProductionDll -PathType Leaf) { (Get-FileHash -LiteralPath $ProductionDll -Algorithm SHA256).Hash.ToLowerInvariant() } else { '' }
        }
        # A selected, non-deferred loader case always inspects a production DLL: a missing one is refused here.
        if (-not $ProductionDll -or -not (Test-Path -LiteralPath $ProductionDll -PathType Leaf)) {
            throw "The production DLL for $loaderCase is missing ($ProductionDll); the case was not run."
        }
        if ([string]::IsNullOrEmpty($ProductionDllSha256) -or $ProductionDllSha256 -cnotmatch '^[0-9a-f]{64}$') { throw 'The production DLL needs the SHA-256 its compile measured (-ProductionDllSha256).' }
        . (Join-Path $PSScriptRoot 'ReferenceProvenance.Common.ps1')
        $expectedLoaderReferences = @((Get-KmgLoaderReferencePins).References | ForEach-Object {
            '{0}, Version={1}, Culture=neutral, PublicKeyToken=null' -f [IO.Path]::GetFileNameWithoutExtension([string]$_.relativePath), [string]$_.assemblyVersion })
        $privateProduction = Join-Path ([IO.Path]::GetTempPath()) ('kmg-production-dll-' + [Guid]::NewGuid().ToString('N'))
        [void][IO.Directory]::CreateDirectory($privateProduction)
        $privateDll = Join-Path $privateProduction 'KingmakerGunslinger.dll'
        [IO.File]::Copy($ProductionDll, $privateDll)
        if ((Get-FileHash -LiteralPath $privateDll -Algorithm SHA256).Hash.ToLowerInvariant() -cne $ProductionDllSha256) {
            throw "The production DLL changed after its compile ($ProductionDllSha256); the loader-reference case was not run."
        }
        [Environment]::SetEnvironmentVariable('KMG_PRODUCTION_DLL', $privateDll, 'Process')
        [Environment]::SetEnvironmentVariable('KMG_PRODUCTION_DLL_SHA256', $ProductionDllSha256, 'Process')
        [Environment]::SetEnvironmentVariable('KMG_EXPECTED_LOADER_REFERENCES', ($expectedLoaderReferences -join ';'), 'Process')
    }

    Write-Host "Running dependency-free domain/reflection tests: $testExecutable"
    if ($RunnerLogPath) {
        $runnerErrorPath = "$RunnerLogPath.stderr"
        $runner = Start-Process -FilePath $testExecutable -NoNewWindow -Wait -PassThru `
            -RedirectStandardOutput $RunnerLogPath -RedirectStandardError $runnerErrorPath
        Get-Content -LiteralPath $RunnerLogPath | Out-Host
        Get-Content -LiteralPath $runnerErrorPath | Out-Host
        $runnerExitCode = $runner.ExitCode
    } else {
        & $testExecutable
        $runnerExitCode = $LASTEXITCODE
    }
    if ($runnerExitCode -ne 0) {
        throw "Domain tests failed with exit code $runnerExitCode."
    }
}
finally {
    foreach ($name in $bindingNames) { [Environment]::SetEnvironmentVariable($name, $previousBinding[$name], 'Process') }
    if ($privateProduction -and (Test-Path -LiteralPath $privateProduction)) { Remove-Item -LiteralPath $privateProduction -Recurse -Force }
}

Write-Host 'Dependency-free domain/reflection tests passed.'
