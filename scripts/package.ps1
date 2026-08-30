[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',

    [switch]$IncludeSymbols,
    [switch]$AllowMissingFirearmSoundBank
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common.ps1')
$repositoryRoot = Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot
$info = Get-KmgModInfo -RepositoryRoot $repositoryRoot

if ($IncludeSymbols) {
    throw 'The standalone UMM package contract permits exactly one binary. PDB symbols must not be included.'
}

& (Join-Path $PSScriptRoot 'validate-build-output.ps1') -Configuration $Configuration
& (Join-Path $PSScriptRoot 'Validate-FirearmSoundBank.ps1') -AllowMissing:$AllowMissingFirearmSoundBank

$outputDirectory = Join-Path $repositoryRoot "artifacts\bin\$Configuration\KingmakerGunslinger"
$stagingDirectory = Join-Path $repositoryRoot 'artifacts\staging\install'
$modDirectory = Join-Path $stagingDirectory $info.Id
$packagesDirectory = Join-Path $repositoryRoot 'artifacts\packages'
$packagePath = Join-Path $packagesDirectory "$($info.Id)-$($info.Version)-martial-performance-repair-notifications.zip"
$checksumPath = "$packagePath.sha256"

if (Test-Path -LiteralPath $stagingDirectory) {
    Remove-Item -LiteralPath $stagingDirectory -Recurse -Force
}
New-Item -ItemType Directory -Path (Join-Path $modDirectory 'blueprints') -Force | Out-Null
New-Item -ItemType Directory -Path $packagesDirectory -Force | Out-Null

$copies = @(
    [ordered]@{ Source = (Join-Path $outputDirectory 'KingmakerGunslinger.dll'); Destination = $modDirectory },
    [ordered]@{ Source = (Join-Path $repositoryRoot 'Info.json'); Destination = $modDirectory },
    [ordered]@{ Source = (Join-Path $repositoryRoot 'CHANGELOG.md'); Destination = $modDirectory },
    [ordered]@{ Source = (Join-Path $repositoryRoot 'LICENSE'); Destination = $modDirectory },
    [ordered]@{ Source = (Join-Path $repositoryRoot 'README.md'); Destination = $modDirectory },
    [ordered]@{ Source = (Join-Path $repositoryRoot 'INSTALLATION-COMPATIBILITY.md'); Destination = $modDirectory },
    [ordered]@{ Source = (Join-Path $repositoryRoot 'SMOKE-TEST-GUIDE.md'); Destination = $modDirectory },
    [ordered]@{ Source = (Join-Path $repositoryRoot 'THIRD-PARTY-ASSETS.md'); Destination = $modDirectory },
    [ordered]@{ Source = (Join-Path $repositoryRoot 'blueprints\blueprints.json'); Destination = (Join-Path $modDirectory 'blueprints') },
    [ordered]@{ Source = (Join-Path $repositoryRoot 'blueprints\blueprints.schema.json'); Destination = (Join-Path $modDirectory 'blueprints') }
)
foreach ($copy in $copies) {
    if (-not (Test-Path -LiteralPath $copy.Source -PathType Leaf)) {
        throw "Required package input is missing: $($copy.Source)"
    }
    Copy-Item -LiteralPath $copy.Source -Destination $copy.Destination
}
$assetSource = Join-Path $outputDirectory 'assets\icons'
$assetDestination = Join-Path $modDirectory 'assets\icons'
if (-not (Test-Path -LiteralPath $assetSource -PathType Container)) {
    throw "Required packaged icon directory is missing: $assetSource"
}
New-Item -ItemType Directory -Path $assetDestination -Force | Out-Null
Copy-Item -Path (Join-Path $assetSource '*.png') -Destination $assetDestination
$summonIconDestination = Join-Path $assetDestination 'expanded-summoning'
New-Item -ItemType Directory -Path $summonIconDestination -Force | Out-Null
Copy-Item -Path (Join-Path $assetSource 'expanded-summoning\*') -Destination $summonIconDestination
$bundleDestination = Join-Path $modDirectory 'assets\bundles'
New-Item -ItemType Directory -Path $bundleDestination -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $outputDirectory 'assets\bundles\kingmakergunslinger.firearms') -Destination $bundleDestination
Copy-Item -LiteralPath (Join-Path $outputDirectory 'assets\bundles\kingmakergunslinger.elvenbranchedspear') -Destination $bundleDestination
Copy-Item -LiteralPath (Join-Path $outputDirectory 'assets\bundles\kingmakergunslinger.easternweapons') -Destination $bundleDestination
Copy-Item -LiteralPath (Join-Path $outputDirectory 'assets\bundles\asset-bundle-manifest.json') -Destination $bundleDestination
$soundBankSource=Join-Path $repositoryRoot 'assets\soundbanks'
if(Test-Path -LiteralPath (Join-Path $soundBankSource 'KMG_Firearms.bnk') -PathType Leaf){
    $soundBankDestination=Join-Path $modDirectory 'assets\soundbanks'
    New-Item -ItemType Directory -Path $soundBankDestination -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path $soundBankSource 'KMG_Firearms.bnk') -Destination $soundBankDestination
    Copy-Item -LiteralPath (Join-Path $soundBankSource 'firearm-soundbank-manifest.json') -Destination $soundBankDestination
}

if (Test-Path -LiteralPath $packagePath) {
    Remove-Item -LiteralPath $packagePath -Force
}
if (Test-Path -LiteralPath $checksumPath) {
    Remove-Item -LiteralPath $checksumPath -Force
}

$python = (Get-Command python -ErrorAction Stop).Source
$hasFirearmSoundBank = Test-Path -LiteralPath (Join-Path $modDirectory `
    'assets\soundbanks\KMG_Firearms.bnk') -PathType Leaf
$expectedPackageFileCount = if ($hasFirearmSoundBank) { 135 } else { 133 }
& $python (Join-Path $repositoryRoot 'tools\create_deterministic_package.py') `
    --source $modDirectory --output $packagePath `
    --expected-file-count $expectedPackageFileCount
if ($LASTEXITCODE -ne 0) {
    throw 'Deterministic standalone package creation failed.'
}
& (Join-Path $PSScriptRoot 'validate-package.ps1') `
    -PackagePath $packagePath -Configuration $Configuration `
    -AllowMissingFirearmSoundBank:$AllowMissingFirearmSoundBank

$checksum = Get-KmgSha256 -Path $packagePath
Set-Content -LiteralPath $checksumPath -Value "$checksum  $([IO.Path]::GetFileName($packagePath))" -Encoding ASCII

Write-Host "Created standalone UMM package: $packagePath"
Write-Host "Created checksum: $checksumPath"
Write-Output $packagePath
