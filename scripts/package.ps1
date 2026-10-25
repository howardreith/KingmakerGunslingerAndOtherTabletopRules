[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',

    [switch]$IncludeSymbols,
    [switch]$AllowMissingFirearmSoundBank,
    [string]$PackagePath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common.ps1')
$repositoryRoot = Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot
$info = Get-KmgModInfo -RepositoryRoot $repositoryRoot

if ($IncludeSymbols) {
    throw 'The standalone UMM package contract permits exactly one binary. PDB symbols must not be included.'
}

$outputDirectory = Join-Path $repositoryRoot "artifacts\bin\$Configuration\KingmakerGunslinger"
$stagingDirectory = Join-Path $repositoryRoot 'artifacts\staging\install'
$modDirectory = Join-Path $stagingDirectory $info.Id
$packagesDirectory = Join-Path $repositoryRoot 'artifacts\packages'
if (-not $PackagePath) {
    [xml]$versionProperties = Get-Content -LiteralPath (Join-Path $repositoryRoot 'Directory.Build.props') -Raw
    $informational = [string]$versionProperties.Project.PropertyGroup.KmgInformationalVersion
    if (-not $informational.StartsWith($info.Version + '-', [StringComparison]::Ordinal) -or
        $informational -cnotmatch '^[0-9]+\.[0-9]+\.[0-9]+-[a-z0-9-]+$') {
        throw 'Package name requires the source version and its declared informational suffix.'
    }
    $PackagePath = Join-Path $packagesDirectory "$($info.Id)-$informational.zip"
}
$packagePath = [IO.Path]::GetFullPath($PackagePath)
$artifactRoot = [IO.Path]::GetFullPath((Join-Path $repositoryRoot 'artifacts')).TrimEnd('\') + '\'
if (-not $packagePath.StartsWith($artifactRoot, [StringComparison]::OrdinalIgnoreCase) -or
    [IO.Path]::GetExtension($packagePath) -ine '.zip') {
    $PSCmdlet.ThrowTerminatingError([Management.Automation.ErrorRecord]::new(
        [ArgumentException]::new('PackagePath must name a ZIP inside this repository artifacts directory.'),
        'KMG_PACKAGE_OUTPUT_PATH', [Management.Automation.ErrorCategory]::InvalidArgument, $PackagePath))
}
& (Join-Path $PSScriptRoot 'validate-build-output.ps1') -Configuration $Configuration
& (Join-Path $PSScriptRoot 'Validate-FirearmSoundBank.ps1') -AllowMissing:$AllowMissingFirearmSoundBank
New-Item -ItemType Directory -Path (Split-Path -Parent $packagePath) -Force | Out-Null
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
foreach ($relative in @('docs\WEAPON-FINDABILITY-RECOVERY.md',
    'docs\WEAPON-FINDABILITY-QUALIFICATION.md',
    'planning\PROJECT-MAGIC-ITEM-ACQUISITION-INVENTORY.md',
    'validation\weapon-findability-native-reference.json',
    'validation\weapon-findability-scene-corrections.json',
    'validation\weapon-findability-runtime-qualification.json',
    'docs\RELEASE-NOTES-0.0.145.md',
    'docs\RELEASE-NOTES-0.0.146.md',
    'docs\RELEASE-NOTES-0.0.151.md',
    'docs\RELEASE-NOTES-0.0.152.md',
    'docs\RELEASE-NOTES-0.0.153.md',
    'docs\RELEASE-NOTES-0.0.155.md',
    'docs\RELEASE-NOTES-0.0.156.md',
    'docs\RELEASE-NOTES-0.0.157.md',
    'docs\RELEASE-NOTES-0.0.158.md',
    'docs\RELEASE-NOTES-0.0.159.md',
    'docs\RELEASE-NOTES-0.0.160.md',
    'docs\RELEASE-NOTES-0.0.161.md',
    'docs\RELEASE-NOTES-0.0.162.md',
    'docs\RELEASE-NOTES-0.0.163.md',
    'docs\RELEASE-NOTES-0.0.164.md',
    'docs\RELEASE-NOTES-0.0.165.md',
    'docs\RELEASE-NOTES-0.0.166.md',
    'docs\RELEASE-NOTES-0.0.167.md')) {
    $source = Join-Path $repositoryRoot $relative
    if (-not (Test-Path -LiteralPath $source -PathType Leaf)) { throw "Required acquisition documentation missing: $relative" }
    $destination = Join-Path $modDirectory $relative
    New-Item -ItemType Directory -Path (Split-Path $destination) -Force | Out-Null
    Copy-Item -LiteralPath $source -Destination $destination
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

# The Pteranodon replacement visual ships as mesh data rather than an
# AssetBundle: it carries no bind poses, no material and no import settings,
# and it does not depend on a Unity editor licence or editor version.
$pteranodonSource = Join-Path $outputDirectory 'assets\pteranodon'
if (Test-Path -LiteralPath $pteranodonSource -PathType Container) {
    $pteranodonDestination = Join-Path $modDirectory 'assets\pteranodon'
    New-Item -ItemType Directory -Path $pteranodonDestination -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path $pteranodonSource 'pteranodon-mesh.json') `
        -Destination $pteranodonDestination
    Copy-Item -LiteralPath (Join-Path $pteranodonSource 'pteranodon-albedo.png') `
        -Destination $pteranodonDestination
}
$batSource = Join-Path $outputDirectory 'assets\flying-animals'
if (Test-Path -LiteralPath $batSource -PathType Container) {
    $batDestination = Join-Path $modDirectory 'assets\flying-animals'
    New-Item -ItemType Directory -Path $batDestination -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path $batSource 'dire-bat-mesh.json') -Destination $batDestination
    Copy-Item -LiteralPath (Join-Path $batSource 'dire-bat-albedo.png') -Destination $batDestination
    Copy-Item -LiteralPath (Join-Path $batSource 'eagle-mesh.json') -Destination $batDestination
    Copy-Item -LiteralPath (Join-Path $batSource 'eagle-albedo.png') -Destination $batDestination
    Copy-Item -LiteralPath (Join-Path $batSource 'giant-wasp-mesh.json') -Destination $batDestination
    Copy-Item -LiteralPath (Join-Path $batSource 'giant-wasp-albedo.png') -Destination $batDestination
    Copy-Item -LiteralPath (Join-Path $batSource 'stirge-mesh.json') -Destination $batDestination
    Copy-Item -LiteralPath (Join-Path $batSource 'stirge-albedo.png') -Destination $batDestination
}
$ungulateSource = Join-Path $outputDirectory 'assets\ungulates'
if (Test-Path -LiteralPath $ungulateSource -PathType Container) {
    $ungulateDestination = Join-Path $modDirectory 'assets\ungulates'
    New-Item -ItemType Directory -Path $ungulateDestination -Force | Out-Null
    foreach ($kind in @('aurochs','bison','rhinoceros','woolly-rhinoceros','gorgon')) {
        Copy-Item -LiteralPath (Join-Path $ungulateSource "$kind-mesh.json") -Destination $ungulateDestination
        Copy-Item -LiteralPath (Join-Path $ungulateSource "$kind-albedo.png") -Destination $ungulateDestination
    }
    Copy-Item -LiteralPath (Join-Path $ungulateSource 'gorgon-parameters.png') -Destination $ungulateDestination
}
$sprint12QuadrupedSource = Join-Path $outputDirectory 'assets\sprint12-quadrupeds'
if (Test-Path -LiteralPath $sprint12QuadrupedSource -PathType Container) {
    $sprint12QuadrupedDestination = Join-Path $modDirectory 'assets\sprint12-quadrupeds'
    New-Item -ItemType Directory -Path $sprint12QuadrupedDestination -Force | Out-Null
    foreach ($kind in @('dire-rat','hyena','goblin-dog')) {
        Copy-Item -LiteralPath (Join-Path $sprint12QuadrupedSource "$kind-mesh.json") -Destination $sprint12QuadrupedDestination
        Copy-Item -LiteralPath (Join-Path $sprint12QuadrupedSource "$kind-albedo.png") -Destination $sprint12QuadrupedDestination
    }
}
$sprint13CreatureSource = Join-Path $outputDirectory 'assets\sprint13-creatures'
if (Test-Path -LiteralPath $sprint13CreatureSource -PathType Container) {
    $sprint13CreatureDestination = Join-Path $modDirectory 'assets\sprint13-creatures'
    New-Item -ItemType Directory -Path $sprint13CreatureDestination -Force | Out-Null
    foreach ($kind in @('wolverine','shadow-mastiff','poisonous-frog')) {
        Copy-Item -LiteralPath (Join-Path $sprint13CreatureSource "$kind-mesh.json") -Destination $sprint13CreatureDestination
        Copy-Item -LiteralPath (Join-Path $sprint13CreatureSource "$kind-albedo.png") -Destination $sprint13CreatureDestination
    }
}
$sprint14InsectSource = Join-Path $outputDirectory 'assets\sprint14-insects'
if (Test-Path -LiteralPath $sprint14InsectSource -PathType Container) {
    $sprint14InsectDestination = Join-Path $modDirectory 'assets\sprint14-insects'
    New-Item -ItemType Directory -Path $sprint14InsectDestination -Force | Out-Null
    foreach ($kind in @('fire-beetle','giant-ant-worker','giant-ant-soldier',
                 'giant-ant-drone','giant-stag-beetle')) {
        Copy-Item -LiteralPath (Join-Path $sprint14InsectSource "$kind-mesh.json") -Destination $sprint14InsectDestination
        Copy-Item -LiteralPath (Join-Path $sprint14InsectSource "$kind-albedo.png") -Destination $sprint14InsectDestination
    }
}
$crocodilianSource = Join-Path $outputDirectory 'assets\sprint16-crocodilians'
if (Test-Path -LiteralPath $crocodilianSource -PathType Container) {
    $crocodilianDestination = Join-Path $modDirectory 'assets\sprint16-crocodilians'
    New-Item -ItemType Directory -Path $crocodilianDestination -Force | Out-Null
    foreach ($kind in @('crocodile','dire-crocodile')) {
        Copy-Item -LiteralPath (Join-Path $crocodilianSource "$kind-mesh.json") -Destination $crocodilianDestination
        Copy-Item -LiteralPath (Join-Path $crocodilianSource "$kind-albedo.png") -Destination $crocodilianDestination
    }
}
# Closed seven-file original Sprint 17 body inventory. Fail closed on a
# missing export; no private authoring outputs or broad folder copy.
$serpentineSource = Join-Path $outputDirectory 'assets\sprint17-serpents'
$serpentineDestination = Join-Path $modDirectory 'assets\sprint17-serpents'
New-Item -ItemType Directory -Path $serpentineDestination -Force | Out-Null
foreach ($kind in @('viper','constrictor-snake','salamander')) {
    Copy-Item -LiteralPath (Join-Path $serpentineSource "$kind-mesh.json") -Destination $serpentineDestination
    Copy-Item -LiteralPath (Join-Path $serpentineSource "$kind-albedo.png") -Destination $serpentineDestination
}
Copy-Item -LiteralPath (Join-Path $serpentineSource 'salamander-human-mesh.json') -Destination $serpentineDestination
# Closed four-file original Sprint 18 body inventory: one mesh and one
# painting for each ape. Fail closed on a missing export; no private
# authoring outputs and no broad folder copy.
$primateSource = Join-Path $outputDirectory 'assets\sprint18-primates'
$primateDestination = Join-Path $modDirectory 'assets\sprint18-primates'
New-Item -ItemType Directory -Path $primateDestination -Force | Out-Null
foreach ($kind in @('ape','dire-ape')) {
    Copy-Item -LiteralPath (Join-Path $primateSource "$kind-mesh.json") -Destination $primateDestination
    Copy-Item -LiteralPath (Join-Path $primateSource "$kind-albedo.png") -Destination $primateDestination
}
# Closed four-file original Sprint 19 body inventory, on the same terms:
# one mesh and one painting for each four-armed creature.
$fourArmedSource = Join-Path $outputDirectory 'assets\sprint19-fourarmed'
$fourArmedDestination = Join-Path $modDirectory 'assets\sprint19-fourarmed'
New-Item -ItemType Directory -Path $fourArmedDestination -Force | Out-Null
foreach ($kind in @('girallon','xill')) {
    Copy-Item -LiteralPath (Join-Path $fourArmedSource "$kind-mesh.json") -Destination $fourArmedDestination
    Copy-Item -LiteralPath (Join-Path $fourArmedSource "$kind-albedo.png") -Destination $fourArmedDestination
}
# Sprint 20's two-file original body, on the same terms: one mesh and
# one painting for the one arachnid.
$arachnidSource = Join-Path $outputDirectory 'assets\sprint20-arachnids'
$arachnidDestination = Join-Path $modDirectory 'assets\sprint20-arachnids'
New-Item -ItemType Directory -Path $arachnidDestination -Force | Out-Null
foreach ($kind in @('giant-scorpion')) {
    Copy-Item -LiteralPath (Join-Path $arachnidSource "$kind-mesh.json") -Destination $arachnidDestination
    Copy-Item -LiteralPath (Join-Path $arachnidSource "$kind-albedo.png") -Destination $arachnidDestination
}
# Sprint 21's two original bodies, staged by name on the same terms: a
# missing export fails the package rather than shipping a crab that never
# arrives and a bebelith that still looks like the Doomspider it replaces.
$arthropodSource = Join-Path $outputDirectory 'assets\sprint21-arthropods'
$arthropodDestination = Join-Path $modDirectory 'assets\sprint21-arthropods'
New-Item -ItemType Directory -Path $arthropodDestination -Force | Out-Null
foreach ($kind in @('giant-crab','bebelith')) {
    Copy-Item -LiteralPath (Join-Path $arthropodSource "$kind-mesh.json") -Destination $arthropodDestination
    Copy-Item -LiteralPath (Join-Path $arthropodSource "$kind-albedo.png") -Destination $arthropodDestination
}
$ankylosaurusDestination = Join-Path $modDirectory 'assets\ankylosaurus'
New-Item -ItemType Directory -Path $ankylosaurusDestination -Force | Out-Null
foreach ($member in @('ankylosaurus-mesh.json','ankylosaurus-albedo.png')) {
    Copy-Item -LiteralPath (Join-Path $outputDirectory "assets\ankylosaurus\$member") -Destination $ankylosaurusDestination
}
$armoredDinosaurSource = Join-Path $outputDirectory 'assets\sprint23-armored-dinosaurs'
$armoredDinosaurDestination = Join-Path $modDirectory 'assets\sprint23-armored-dinosaurs'
New-Item -ItemType Directory -Path $armoredDinosaurDestination -Force | Out-Null
foreach ($kind in @('triceratops','stegosaurus')) {
    Copy-Item -LiteralPath (Join-Path $armoredDinosaurSource "$kind-mesh.json") -Destination $armoredDinosaurDestination
    Copy-Item -LiteralPath (Join-Path $armoredDinosaurSource "$kind-albedo.png") -Destination $armoredDinosaurDestination
}
$outsiderDestination = Join-Path $modDirectory 'assets\outsider-celestial'
New-Item -ItemType Directory -Path $outsiderDestination -Force | Out-Null
$s3031OriginalBodyParts = @('lemure-mesh.json','lemure-albedo.png','dretch-mesh.json','dretch-albedo.png','hound-archon-mesh.json','hound-archon-albedo.png')
foreach ($part in $s3031OriginalBodyParts) {
    Copy-Item -LiteralPath (Join-Path $outputDirectory "assets\outsider-celestial\$part") -Destination $outsiderDestination
}
$theropodSource = Join-Path $outputDirectory 'assets\sprint24-theropods'
$theropodDestination = Join-Path $modDirectory 'assets\sprint24-theropods'
New-Item -ItemType Directory -Path $theropodDestination -Force | Out-Null
foreach ($kind in @('deinonychus','tyrannosaurus')) {
    Copy-Item -LiteralPath (Join-Path $theropodSource "$kind-mesh.json") -Destination $theropodDestination
    Copy-Item -LiteralPath (Join-Path $theropodSource "$kind-albedo.png") -Destination $theropodDestination
}
$giantBodySource = Join-Path $outputDirectory 'assets\sprint25-giant-bodies'
$giantBodyDestination = Join-Path $modDirectory 'assets\sprint25-giant-bodies'
New-Item -ItemType Directory -Path $giantBodyDestination -Force | Out-Null
foreach ($kind in @('brachiosaurus','elephant')) {
    Copy-Item -LiteralPath (Join-Path $giantBodySource "$kind-mesh.json") -Destination $giantBodyDestination
    Copy-Item -LiteralPath (Join-Path $giantBodySource "$kind-albedo.png") -Destination $giantBodyDestination
}
$griffonDestination=Join-Path $modDirectory 'assets\sprint27-griffon'
New-Item -ItemType Directory -Path $griffonDestination -Force | Out-Null
foreach ($part in @('griffon-mesh.json','griffon-albedo.png')) {
    Copy-Item -LiteralPath (Join-Path $outputDirectory "assets\sprint27-griffon\$part") -Destination $griffonDestination
}
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
# Count every declared summon icon, including authorized new creatures. The
# other 272 paths and original-body members retain the existing inventory;
# strict package validation below still requires its exact complete path/hash
# set. The optional soundbank and manifest remain a two-file delta.
. (Join-Path $PSScriptRoot 'IconCatalog.Common.ps1')
$summonManifest = Get-KmgSummonIconManifest -RepositoryRoot $repositoryRoot
$basePackageFileCount = 272 + @($summonManifest.icons).Count + $s3031OriginalBodyParts.Count
$expectedPackageFileCount = if ($hasFirearmSoundBank) {
    $basePackageFileCount + 2
} else {
    $basePackageFileCount
}
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
