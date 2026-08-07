[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..')).Path
$profiles = Get-Content -LiteralPath (Join-Path $root 'compatibility\profiles.json') -Raw | ConvertFrom-Json
$results = @()
foreach ($profile in $profiles.profiles) {
    $path = & (Join-Path $PSScriptRoot 'Resolve-KingmakerCompatibilityProfile.ps1') -ProfileId $profile.id | Select-Object -Last 1
    $result = Get-Content -LiteralPath $path -Raw | ConvertFrom-Json
    if ($result.profileId -cne $profile.id) { throw "Dry-run profile mismatch: $($profile.id)" }
    $results += $result
}
$runtime = @($results | Where-Object runtimeCapable)
if ($runtime.Count -ne 6) { throw "Expected six runtime-capable profiles, observed $($runtime.Count)." }
$craft = $results | Where-Object profileId -ceq 'gunslinger-craft-magic-items'
if ($craft.runtimeCapable -or $craft.staticOnlyReferences.Count -ne 1) { throw 'Craft Magic Items dry-run must remain static-only.' }
$all = $results | Where-Object profileId -ceq 'gunslinger-all-loadable-local'
if (@($all.runtimeMods | Where-Object key -eq 'kaz-asset-references').Count -ne 0) { throw 'KAZ references entered runtime staging.' }
if (@($all.expectedUmmIds | Where-Object { $_ -like 'KAZ_*' }).Count -ne 0) { throw 'KAZ UMM IDs entered all-loadable profile.' }
Write-Host 'All eight compatibility profile dry-runs passed; KAZ references remain non-runtime.'
