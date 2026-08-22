[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'Medium')]
param(
    [string]$LiveModDirectory = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Mods\KingmakerGunslinger',
    [string]$BackupRoot = 'C:\Dev\KingmakerGunslingerLab\runtime-backups\live-mod',
    [switch]$AllowEmptySource
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
$sourceFiles = @(Get-ChildItem -LiteralPath $live -Recurse -File |
    Sort-Object FullName)
$sourceDirectories = @(Get-ChildItem -LiteralPath $live -Recurse -Directory |
    ForEach-Object { $_.FullName.Substring($live.Length).TrimStart('\') } |
    Sort-Object)
if ($sourceFiles.Count -eq 0 -and -not $AllowEmptySource) {
    throw 'Live mod contains no files; use the explicit first-install path only after verifying that state.'
}
if ($sourceFiles.Count -ne 0 -and $AllowEmptySource) {
    throw 'The explicit first-install path is valid only for a live mod tree containing no files.'
}

Write-Host "Backup source: $live"
Write-Host "Backup destination: $destination"
$sourceFiles | ForEach-Object {
        $relative = $_.FullName.Substring($live.Length).TrimStart('\')
        Write-Host "  $($_.FullName) -> $(Join-Path $destination $relative)"
    }

if ($PSCmdlet.ShouldProcess($destination, "Copy only $live")) {
    New-Item -ItemType Directory -Path $destination | Out-Null
    foreach ($sourceFile in $sourceFiles) {
        $relative = $sourceFile.FullName.Substring($live.Length).TrimStart('\')
        $target = Join-Path $destination $relative
        New-Item -ItemType Directory -Path (Split-Path -Parent $target) -Force | Out-Null
        Copy-Item -LiteralPath $sourceFile.FullName -Destination $target
    }
    $files = @(Get-ChildItem -LiteralPath $destination -Recurse -File)
    if ($sourceFiles.Count -eq 0) {
        [ordered]@{
            schemaVersion = 1
            emptyLiveMod = $true
            source = $live
            directories = $sourceDirectories
        } | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (
            Join-Path $destination '.kmg-empty-live-mod.json') -Encoding UTF8
        $files = @(Get-ChildItem -LiteralPath $destination -Recurse -File)
        if ($files.Count -ne 1) {
            throw 'Empty first-install backup marker was not singular.'
        }
    }
    elseif ($files.Count -ne $sourceFiles.Count) {
        throw 'Backup file count differs from the live source.'
    }
    Write-Host "Backup completed without deleting or overwriting: $destination"
} else {
    Write-Host 'Dry run only; no backup directory was created.'
}

[pscustomobject]@{ Source = $live; Destination = $destination;
    DryRun = [bool]$WhatIfPreference; EmptySource = $sourceFiles.Count -eq 0 }
