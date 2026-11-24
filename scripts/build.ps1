[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',

    [string]$KingmakerInstallDir,

    [string]$MSBuildPath,

    [switch]$Clean,

    [switch]$Package,

    [switch]$IncludeSymbols,

    [switch]$SkipDomainTests,
    # Development compilation only: no aggregate tests, package or release gate.
    [switch]$CompileOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common.ps1')

$repositoryRoot = Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot
$projectPath = Join-Path $repositoryRoot 'src\KingmakerGunslinger\KingmakerGunslinger.csproj'
$localPropsPath = Join-Path $repositoryRoot 'GamePath.props'

if ($CompileOnly -and ($Package -or $IncludeSymbols)) {
    throw '-CompileOnly does not accept packaging options.'
}
if (-not $CompileOnly) {
    & (Join-Path $PSScriptRoot 'validate-repository.ps1')
}

if (-not $KingmakerInstallDir -and -not (Test-Path -LiteralPath $localPropsPath -PathType Leaf)) {
    throw 'No Kingmaker path is configured. Copy GamePath.props.example to GamePath.props, or pass -KingmakerInstallDir.'
}

if ($KingmakerInstallDir) {
    if (-not (Test-Path -LiteralPath $KingmakerInstallDir -PathType Container)) {
        throw "KingmakerInstallDir does not exist: $KingmakerInstallDir"
    }

    $KingmakerInstallDir = (Resolve-Path -LiteralPath $KingmakerInstallDir).Path
}

$msbuild = Resolve-KmgMsBuild -ExplicitPath $MSBuildPath
# The product compiles against the pinned loader references (the project's default UnityModManagerDir); fetch and verify
# them first, exactly as Build-Local.ps1 does.
. (Join-Path $PSScriptRoot 'ReferenceProvenance.Common.ps1')
[void](Initialize-KmgPinnedLoaderReferences -LoaderReferenceDir (Join-Path $repositoryRoot 'artifacts\references\loader'))
$target = if ($Clean) { 'Rebuild' } else { 'Build' }
# test-domain already compiles the runner. Building the solution here would
# compile it again along with production.
$platform = 'AnyCPU'
$arguments = @(
    $projectPath,
    '/nologo',
    '/m',
    "/t:$target",
    "/p:Configuration=$Configuration",
    "/p:Platform=$platform",
    '/verbosity:minimal'
)

if ($KingmakerInstallDir) {
    $arguments += "/p:KingmakerInstallDir=$KingmakerInstallDir"
}

Write-Host "Building Kingmaker Gunslinger ($Configuration) with $msbuild"
& $msbuild @arguments
if ($LASTEXITCODE -ne 0) {
    throw "MSBuild failed with exit code $LASTEXITCODE."
}

if ($CompileOnly) {
    Write-Host 'Development production compilation passed; package/release checks deferred.'
    return
}

# The domain tests run after the production compilation, so the loader-reference case inspects this compiler output.
if (-not $SkipDomainTests) {
    $productionDll = Join-Path $repositoryRoot "artifacts\bin\$Configuration\KingmakerGunslinger\KingmakerGunslinger.dll"
    $productionDllSha256 = if (Test-Path -LiteralPath $productionDll -PathType Leaf) { (Get-FileHash -LiteralPath $productionDll -Algorithm SHA256).Hash.ToLowerInvariant() } else { '' }
    & (Join-Path $PSScriptRoot 'test-domain.ps1') -Configuration $Configuration -MSBuildPath $MSBuildPath -SkipRepositoryValidation -ProductionDll $productionDll -ProductionDllSha256 $productionDllSha256
}

if ($Package) {
    $packageArguments = @{
        Configuration = $Configuration
    }
    if ($IncludeSymbols) {
        $packageArguments.IncludeSymbols = $true
    }

    & (Join-Path $PSScriptRoot 'package.ps1') @packageArguments
} else {
    & (Join-Path $PSScriptRoot 'validate-build-output.ps1') -Configuration $Configuration
}
