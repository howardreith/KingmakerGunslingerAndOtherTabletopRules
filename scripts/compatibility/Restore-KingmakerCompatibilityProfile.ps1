[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'High')]
param(
    [Parameter(Mandatory = $true)][ValidatePattern('^[A-Za-z0-9._-]{1,100}$')][string]$RunId,
    [string]$StateRoot = 'C:\Dev\KingmakerGunslingerLab\compatibility-state'
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'CompatibilityProfile.Common.ps1')
$expectedStateRoot = [IO.Path]::GetFullPath('C:\Dev\KingmakerGunslingerLab\compatibility-state').TrimEnd('\')
if (-not [IO.Path]::GetFullPath($StateRoot).TrimEnd('\').Equals($expectedStateRoot, [StringComparison]::OrdinalIgnoreCase)) { throw "Public transaction state root must be exact: $expectedStateRoot" }
if (-not $PSCmdlet.ShouldProcess($RunId, 'restore exact pre-profile Mods and managed SoundBank state')) { return }
$state = Restore-KmgCompatibilityTransaction -RunId $RunId -StateRoot $StateRoot
Write-Host "Compatibility restoration verified: $($state.restorationVerified), run $RunId"
