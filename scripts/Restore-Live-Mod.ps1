[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'High')]
param(
    [Parameter(Mandatory = $true)][string]$BackupDirectory,
    [string]$LiveModDirectory = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Mods\KingmakerGunslinger'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')

Assert-KmgNotRunning
if (-not (Test-Path -LiteralPath $BackupDirectory -PathType Container)) {
    throw "Explicit backup directory does not exist: $BackupDirectory"
}
if (-not (Test-Path -LiteralPath $LiveModDirectory -PathType Container)) {
    throw "Expected live mod directory does not exist: $LiveModDirectory"
}
$backup = (Resolve-Path -LiteralPath $BackupDirectory).Path
$live = (Resolve-Path -LiteralPath $LiveModDirectory).Path
$expectedLive = [IO.Path]::GetFullPath('C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Mods\KingmakerGunslinger')
if (-not $live.Equals($expectedLive, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Refusing restore outside the exact KingmakerGunslinger directory: $live"
}
if ($backup.Equals($live, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Backup directory cannot be the live mod directory.'
}
foreach ($required in @('Info.json', 'KingmakerGunslinger.dll')) {
    if (-not (Test-Path -LiteralPath (Join-Path $backup $required) -PathType Leaf)) {
        throw "Backup structure is invalid; missing $required."
    }
}
$info = Get-Content -LiteralPath (Join-Path $backup 'Info.json') -Raw | ConvertFrom-Json
if ($info.Id -ne 'KingmakerGunslinger' -or $info.AssemblyName -ne 'KingmakerGunslinger.dll') {
    throw 'Backup metadata does not identify the KingmakerGunslinger mod.'
}

Write-Host "Explicit restore source: $backup"
Write-Host "Only restore target: $live"
if (-not $PSCmdlet.ShouldProcess($live, "Restore explicit backup $backup")) {
    Write-Host 'Dry run only; backup and target were validated.'
    return
}
foreach ($child in Get-ChildItem -LiteralPath $live -Force) {
    $target = Assert-KmgPathWithin -Path $child.FullName -Root $live
    Remove-Item -LiteralPath $target -Recurse -Force
}
Copy-Item -Path (Join-Path $backup '*') -Destination $live -Recurse -Force
if ((Get-KmgSha256 -Path (Join-Path $backup 'KingmakerGunslinger.dll')) -ne
    (Get-KmgSha256 -Path (Join-Path $live 'KingmakerGunslinger.dll'))) {
    throw 'Restored DLL hash does not match the explicit backup.'
}
Write-Host "Restore verified for only: $live"
