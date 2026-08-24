[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'High')]
param(
    [Parameter(Mandatory = $true)][ValidatePattern('^gunslinger(?:-[a-z0-9]+)*$')][string]$ProfileId,
    [Parameter(Mandatory = $true)][ValidatePattern('^[A-Za-z0-9._-]{1,100}$')][string]$RunId,
    [string]$KingmakerInstallDir = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker',
    [string]$StateRoot = 'C:\Dev\KingmakerGunslingerLab\compatibility-state'
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'CompatibilityProfile.Common.ps1')
$root = Get-KmgCompatibilityRepositoryRoot
$expectedStateRoot = [IO.Path]::GetFullPath('C:\Dev\KingmakerGunslingerLab\compatibility-state').TrimEnd('\')
if (-not [IO.Path]::GetFullPath($StateRoot).TrimEnd('\').Equals($expectedStateRoot, [StringComparison]::OrdinalIgnoreCase)) { throw "Public transaction state root must be exact: $expectedStateRoot" }
$package = Join-Path $root 'artifacts\local-runtime\0.0.97\KingmakerGunslinger-0.0.97-local-runtime.zip'
$resolution = Resolve-KmgCompatibilityProfile -ProfileId $ProfileId -ReferenceRoot 'C:\Dev\KingmakerGunslingerLab\examples' -PackagePath $package -KingmakerInstallDir $KingmakerInstallDir -RepositoryRoot $root
if (-not $PSCmdlet.ShouldProcess((Join-Path $KingmakerInstallDir 'Mods'), "enter isolated profile $($resolution.profileId), run $RunId")) { return }
$state = Enter-KmgCompatibilityTransaction -Resolution $resolution -KingmakerInstallDir $KingmakerInstallDir -StateRoot $StateRoot -RunId $RunId
Write-Host "Compatibility profile active: $($state.profileId), run $RunId"
Write-Output (Join-Path $StateRoot "$RunId\transaction.json")
