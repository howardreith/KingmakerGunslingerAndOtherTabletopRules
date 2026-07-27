[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'High')]
param(
    [Parameter(Mandatory = $true)][string]$PackagePath,
    [string]$LiveModDirectory = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Mods\KingmakerGunslinger',
    [string]$BackupRoot = 'C:\Dev\KingmakerGunslingerLab\runtime-backups\live-mod',
    [string]$EvidenceRoot = 'C:\Dev\KingmakerGunslingerLab\runtime-evidence'
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

Write-Host "Validated Build-Local package: $($manifest.packagePath)"
Write-Host "Target directory: $live"
if (-not $PSCmdlet.ShouldProcess($live, "Back up and deploy version $($manifest.version)")) {
    & (Join-Path $PSScriptRoot 'Backup-Live-Mod.ps1') -LiveModDirectory $live -BackupRoot $BackupRoot -WhatIf
    Write-Host 'Dry run only; package and target were validated and no deployment manifest was written.'
    return
}

$backup = & (Join-Path $PSScriptRoot 'Backup-Live-Mod.ps1') -LiveModDirectory $live -BackupRoot $BackupRoot -Confirm:$false
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

foreach ($child in Get-ChildItem -LiteralPath $live -Force) {
    $target = Assert-KmgPathWithin -Path $child.FullName -Root $live
    Remove-Item -LiteralPath $target -Recurse -Force
}
Copy-Item -Path (Join-Path $source '*') -Destination $live -Recurse -Force

$deployedInfo = Get-Content -LiteralPath (Join-Path $live 'Info.json') -Raw | ConvertFrom-Json
$deployedDll = Join-Path $live 'KingmakerGunslinger.dll'
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

$deploymentDirectory = Join-Path $EvidenceRoot ('deployments\' + [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffffffZ'))
New-Item -ItemType Directory -Path $deploymentDirectory | Out-Null
[ordered]@{
    schemaVersion = 1
    deployedAtUtc = [DateTime]::UtcNow.ToString('o')
    packagePath = $manifest.packagePath
    packageSha256 = $manifest.packageSha256
    version = $manifest.version
    deployedDllSha256 = Get-KmgSha256 -Path $deployedDll
    liveModDirectory = $live
    backupDirectory = $backup.Destination
    files = $actualFiles
} | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $deploymentDirectory 'deployment.json') -Encoding UTF8
Write-Host "Deployment verified; manifest: $(Join-Path $deploymentDirectory 'deployment.json')"
