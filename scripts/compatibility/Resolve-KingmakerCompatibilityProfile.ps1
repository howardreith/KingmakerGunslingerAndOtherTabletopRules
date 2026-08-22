[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][ValidatePattern('^gunslinger(?:-[a-z0-9]+)*$')][string]$ProfileId,
    [string]$ReferenceRoot = 'C:\Dev\KingmakerGunslingerLab\examples',
    [string]$KingmakerInstallDir = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker',
    [string]$PackagePath,
    [string]$OutputPath
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'CompatibilityProfile.Common.ps1')
$root = Get-KmgCompatibilityRepositoryRoot
if (-not $PackagePath) { $PackagePath = Join-Path $root 'artifacts\local-runtime\0.0.90\KingmakerGunslinger-0.0.90-local-runtime.zip' }
if (-not $OutputPath) { $OutputPath = Join-Path $root "artifacts\compatibility\profile-resolution\$ProfileId.json" }
$output = [IO.Path]::GetFullPath($OutputPath)
$allowedOutput = [IO.Path]::GetFullPath((Join-Path $root 'artifacts\compatibility')).TrimEnd('\')
if (-not $output.StartsWith($allowedOutput + '\', [StringComparison]::OrdinalIgnoreCase)) { throw 'Resolution output must remain beneath artifacts/compatibility.' }
$resolution = Resolve-KmgCompatibilityProfile -ProfileId $ProfileId -ReferenceRoot $ReferenceRoot -PackagePath $PackagePath -KingmakerInstallDir $KingmakerInstallDir -RepositoryRoot $root
New-Item -ItemType Directory -Path (Split-Path -Parent $output) -Force | Out-Null
$resolution | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $output -Encoding UTF8
Write-Host "Profile: $($resolution.profileId)"
Write-Host "Runtime capable: $($resolution.runtimeCapable)"
Write-Host "Intended Mods directory: $($resolution.intendedModsDirectory)"
Write-Host "Expected load order: $($resolution.expectedLoadOrder -join ', ')"
foreach ($mod in $resolution.runtimeMods) { Write-Host "Runtime mod: $($mod.ummId) $($mod.version) $($mod.assembly.assemblyName) SHA-256=$($mod.assembly.sha256) source=$($mod.sourceDirectory) destination=$($mod.destinationName)" }
foreach ($item in $resolution.staticOnlyReferences) { Write-Host "Static only: $($item.key) [$($item.paths -join ', ')]" }
foreach ($item in $resolution.unavailableReferences) { Write-Host "Unavailable: $item" }
Write-Host "Conflicts: $($resolution.conflicts.Count); warnings: $($resolution.warnings.Count)"
Write-Output $output
