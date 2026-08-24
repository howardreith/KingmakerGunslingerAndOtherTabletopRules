[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'High')]
param(
    [Parameter(Mandatory = $true)][string]$PackagePath,
    [string]$LiveModDirectory = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Mods\KingmakerGunslinger',
    [string]$BackupRoot = 'C:\Dev\KingmakerGunslingerLab\runtime-backups\live-mod',
    [string]$EvidenceRoot = 'C:\Dev\KingmakerGunslingerLab\runtime-evidence',
    [switch]$AllowEmptyFirstInstall,
    [switch]$PassThru
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')

$root = Get-KmgRepositoryRoot -ScriptDirectory $PSScriptRoot
$requiredEvidenceRoot = [IO.Path]::GetFullPath('C:\Dev\KingmakerGunslingerLab\runtime-evidence').TrimEnd('\')
if (-not [IO.Path]::GetFullPath($EvidenceRoot).TrimEnd('\').Equals($requiredEvidenceRoot, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Evidence root must be exactly: $requiredEvidenceRoot"
}
$manifest = Read-KmgBuildLocalManifest -PackagePath $PackagePath -RepositoryRoot $root
Assert-KmgNotRunning
if (-not (Test-Path -LiteralPath $LiveModDirectory -PathType Container)) {
    throw "Expected live mod directory is missing: $LiveModDirectory"
}
$live = (Resolve-Path -LiteralPath $LiveModDirectory).Path
$expectedLive = [IO.Path]::GetFullPath('C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Mods\KingmakerGunslinger')
if (-not $live.Equals($expectedLive, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Refusing deployment outside the exact KingmakerGunslinger directory: $live"
}
$liveFiles = @(Get-ChildItem -LiteralPath $live -Recurse -File)
if ($AllowEmptyFirstInstall -and $liveFiles.Count -ne 0) {
    throw '-AllowEmptyFirstInstall is valid only when the exact live mod tree contains no files.'
}

Write-Host "Validated Build-Local package: $($manifest.packagePath)"
Write-Host "Target directory: $live"
if (-not $PSCmdlet.ShouldProcess($live, "Back up and deploy version $($manifest.version)")) {
    Write-Host 'Dry run only; package and target were validated and no deployment manifest was written.'
    return
}

$backup = & (Join-Path $PSScriptRoot 'Backup-Live-Mod.ps1') `
    -LiveModDirectory $live -BackupRoot $BackupRoot `
    -AllowEmptySource:$AllowEmptyFirstInstall -Confirm:$false
$featureSettingsPath = Join-Path $live 'FeatureModules.json'
$featureSettingsExisted = Test-Path -LiteralPath $featureSettingsPath -PathType Leaf
$featureSettingsBytes = if ($featureSettingsExisted) {
    [IO.File]::ReadAllBytes($featureSettingsPath)
} else { $null }
$stagingRoot = Join-Path $root 'artifacts\deploy-staging'
if (Test-Path -LiteralPath $stagingRoot) {
    $resolved = Assert-KmgPathWithin -Path $stagingRoot -Root (Join-Path $root 'artifacts')
    Remove-Item -LiteralPath $resolved -Recurse -Force
}
New-Item -ItemType Directory -Path $stagingRoot | Out-Null
Expand-Archive -LiteralPath $manifest.packagePath -DestinationPath $stagingRoot
$source = Join-Path $stagingRoot 'KingmakerGunslinger'
if (-not (Test-Path -LiteralPath (Join-Path $source 'Info.json') -PathType Leaf)) {
    throw 'Validated package did not extract to the expected single mod root.'
}
$packagedFirearmManifest = Join-Path $source `
    'assets\soundbanks\firearm-soundbank-manifest.json'
$packagedFirearmSoundBank = Join-Path $source `
    'assets\soundbanks\KMG_Firearms.bnk'
$packagedFirearmManifestSha256 = Get-KmgSha256 -Path $packagedFirearmManifest
$packagedFirearmSoundBankSha256 = Get-KmgSha256 -Path $packagedFirearmSoundBank
if ($packagedFirearmManifestSha256 -ne $manifest.firearmManifestSha256 -or
    $packagedFirearmSoundBankSha256 -ne $manifest.firearmSoundBankSha256) {
    throw 'Extracted package firearm audio differs from its immutable build manifest.'
}

$deployedDll = Join-Path $live 'KingmakerGunslinger.dll'
$deployedFirearmBundle = Join-Path $live 'assets\bundles\kingmakergunslinger.firearms'
$deployedFirearmManifest = Join-Path $live `
    'assets\soundbanks\firearm-soundbank-manifest.json'
$deployedFirearmSoundBank = Join-Path $live `
    'assets\soundbanks\KMG_Firearms.bnk'
try {
    foreach ($child in Get-ChildItem -LiteralPath $live -Force) {
        $target = Assert-KmgPathWithin -Path $child.FullName -Root $live
        Remove-Item -LiteralPath $target -Recurse -Force
    }
    Copy-Item -Path (Join-Path $source '*') -Destination $live -Recurse -Force

    $deployedInfo = Get-Content -LiteralPath (Join-Path $live 'Info.json') -Raw | ConvertFrom-Json
    if ($deployedInfo.Version -ne $manifest.version -or
        (Get-KmgSha256 -Path $deployedDll) -ne $manifest.dllSha256) {
        throw 'Deployed metadata or DLL hash verification failed. Restore the explicit backup.'
    }
    $expectedFiles = @(Get-ChildItem -LiteralPath $source -Recurse -File |
        ForEach-Object { $_.FullName.Substring($source.Length).TrimStart('\') } | Sort-Object)
    $actualFiles = @(Get-ChildItem -LiteralPath $live -Recurse -File |
        ForEach-Object { $_.FullName.Substring($live.Length).TrimStart('\') } | Sort-Object)
    if (($expectedFiles -join "`n") -ne ($actualFiles -join "`n")) {
        throw 'Deployed filename verification failed.'
    }
    $deployedFirearmManifestSha256 = Get-KmgSha256 -Path $deployedFirearmManifest
    $deployedFirearmSoundBankSha256 = Get-KmgSha256 -Path $deployedFirearmSoundBank
    if ($packagedFirearmManifestSha256 -ne $deployedFirearmManifestSha256 -or
        $packagedFirearmSoundBankSha256 -ne $deployedFirearmSoundBankSha256) {
        throw 'Packaged and deployed firearm audio files differ.'
    }
}
finally {
    if ($featureSettingsExisted) {
        $featureSettingsTemporary = $featureSettingsPath + '.kmg-deploy-restore.tmp'
        [IO.File]::WriteAllBytes($featureSettingsTemporary, $featureSettingsBytes)
        Move-Item -LiteralPath $featureSettingsTemporary -Destination $featureSettingsPath -Force
    }
}

$deploymentDirectory = Join-Path $EvidenceRoot ('deployments\' + [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffffffZ'))
New-Item -ItemType Directory -Path $deploymentDirectory | Out-Null
[ordered]@{
    schemaVersion = 2
    deployedAtUtc = [DateTime]::UtcNow.ToString('o')
    packagePath = $manifest.packagePath
    packageSha256 = $manifest.packageSha256
    commit = $manifest.commit
    branch = $manifest.branch
    version = $manifest.version
    dllSha256 = $manifest.dllSha256
    dllMvid = $manifest.dllMvid
    deployedDllSha256 = Get-KmgSha256 -Path $deployedDll
    firearmBundleSha256 = Get-KmgSha256 -Path $deployedFirearmBundle
    firearmManifestSha256 = $packagedFirearmManifestSha256
    firearmSoundBankSha256 = $packagedFirearmSoundBankSha256
    deployedFirearmManifestSha256 = $deployedFirearmManifestSha256
    deployedFirearmSoundBankSha256 = $deployedFirearmSoundBankSha256
    featureModuleSettingsPreserved = $featureSettingsExisted
    featureModuleSettingsSha256 = if ($featureSettingsExisted) {
        Get-KmgSha256 -Path $featureSettingsPath
    } else { '<absent>' }
    liveModDirectory = $live
    backupDirectory = $backup.Destination
    backupWasEmpty = [bool]$backup.EmptySource
    files = $actualFiles
} | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $deploymentDirectory 'deployment.json') -Encoding UTF8
$deploymentManifestPath = Join-Path $deploymentDirectory 'deployment.json'
Write-Host "Deployment verified; manifest: $deploymentManifestPath"
if ($PassThru) {
    Write-Output $deploymentManifestPath
}
