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
if ($runtime.Count -ne 7) { throw "Expected seven runtime-capable profiles, observed $($runtime.Count)." }
$favoredBlocked = @($results | Where-Object {
    $_.profileId -in @(
        'gunslinger-call-of-the-wild-favored-class',
        'gunslinger-call-of-the-wild-favored-class-traits-disabled',
        'gunslinger-high-risk-combined-favored-class')
})
if ($favoredBlocked.Count -ne 3 -or @($favoredBlocked | Where-Object runtimeCapable).Count -ne 0) {
    throw 'All three Favored Class profiles must remain unavailable until an exact compiled artifact is supplied.'
}
$craft = $results | Where-Object profileId -ceq 'gunslinger-craft-magic-items'
if ($craft.runtimeCapable -or $craft.staticOnlyReferences.Count -ne 1) { throw 'Craft Magic Items dry-run must remain static-only.' }
$all = $results | Where-Object profileId -ceq 'gunslinger-all-loadable-local'
if (@($all.runtimeMods | Where-Object key -eq 'kaz-asset-references').Count -ne 0) { throw 'KAZ references entered runtime staging.' }
if (@($all.expectedUmmIds | Where-Object { $_ -like 'KAZ_*' }).Count -ne 0) { throw 'KAZ UMM IDs entered all-loadable profile.' }
Write-Host 'All twelve compatibility profile dry-runs passed; Favored Class remains explicitly unavailable and KAZ references remain non-runtime.'
