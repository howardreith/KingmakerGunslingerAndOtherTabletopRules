[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'Medium')]
param(
    [string]$LiveModDirectory = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Mods\KingmakerGunslinger',
    [string]$BackupRoot = 'C:\Dev\KingmakerGunslingerLab\runtime-backups\live-mod'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')

if (-not (Test-Path -LiteralPath $LiveModDirectory -PathType Container)) {
    throw "Live KingmakerGunslinger mod directory is missing: $LiveModDirectory"
}
$live = (Resolve-Path -LiteralPath $LiveModDirectory).Path
$expectedLive = [IO.Path]::GetFullPath('C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Mods\KingmakerGunslinger')
if (-not $live.Equals($expectedLive, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Refusing to back up an unexpected mod directory: $live"
}
$expectedBackupRoot = [IO.Path]::GetFullPath('C:\Dev\KingmakerGunslingerLab\runtime-backups\live-mod').TrimEnd('\')
if (-not [IO.Path]::GetFullPath($BackupRoot).TrimEnd('\').Equals($expectedBackupRoot, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Backup root must be exactly: $expectedBackupRoot"
}
$stamp = [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffffffZ')
$destination = Join-Path $BackupRoot $stamp
if (Test-Path -LiteralPath $destination) { throw "Backup collision: $destination" }

Write-Host "Backup source: $live"
Write-Host "Backup destination: $destination"
Get-ChildItem -LiteralPath $live -Recurse -File | Sort-Object FullName |
    ForEach-Object {
        $relative = $_.FullName.Substring($live.Length).TrimStart('\')
        Write-Host "  $($_.FullName) -> $(Join-Path $destination $relative)"
    }

if ($PSCmdlet.ShouldProcess($destination, "Copy only $live")) {
    New-Item -ItemType Directory -Path $destination | Out-Null
    foreach ($sourceFile in Get-ChildItem -LiteralPath $live -Recurse -File) {
        $relative = $sourceFile.FullName.Substring($live.Length).TrimStart('\')
        $target = Join-Path $destination $relative
        New-Item -ItemType Directory -Path (Split-Path -Parent $target) -Force | Out-Null
        Copy-Item -LiteralPath $sourceFile.FullName -Destination $target
    }
    $files = @(Get-ChildItem -LiteralPath $destination -Recurse -File)
    if ($files.Count -eq 0) { throw 'Backup unexpectedly contains no files.' }
    Write-Host "Backup completed without deleting or overwriting: $destination"
} else {
    Write-Host 'Dry run only; no backup directory was created.'
}

[pscustomobject]@{ Source = $live; Destination = $destination; DryRun = [bool]$WhatIfPreference }
