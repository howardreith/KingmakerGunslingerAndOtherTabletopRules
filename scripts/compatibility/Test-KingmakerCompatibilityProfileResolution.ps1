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
if ($runtime.Count -ne 10) { throw "Expected ten runtime-capable profiles, observed $($runtime.Count)." }
$favoredProfiles = @($results | Where-Object {
    $_.profileId -in @(
        'gunslinger-call-of-the-wild-favored-class',
        'gunslinger-call-of-the-wild-favored-class-traits-disabled',
        'gunslinger-high-risk-combined-favored-class')
})
if ($favoredProfiles.Count -ne 3 -or
    @($favoredProfiles | Where-Object runtimeCapable).Count -ne 3) {
    throw 'All three exact Favored Class profiles must be runtime capable.'
}
$favored = $favoredProfiles | Where-Object {
    $_.profileId -ceq 'gunslinger-call-of-the-wild-favored-class'
}
$favoredDisabled = $favoredProfiles | Where-Object {
    $_.profileId -ceq 'gunslinger-call-of-the-wild-favored-class-traits-disabled'
}
$favoredCombined = $favoredProfiles | Where-Object {
    $_.profileId -ceq 'gunslinger-high-risk-combined-favored-class'
}
if (@($favored.runtimeMods | Where-Object key -ceq 'favored-class').Count -ne 1 -or
    @($favoredDisabled.runtimeMods | Where-Object key -ceq 'favored-class').Count -ne 1 -or
    @($favoredCombined.runtimeMods | Where-Object key -ceq 'favored-class').Count -ne 1 -or
    @($favoredCombined.runtimeMods | Where-Object key -ceq 'tweak-or-treat').Count -ne 1 -or
    @($favoredCombined.runtimeMods | Where-Object key -ceq 'races-unleashed').Count -ne 1) {
    throw 'Favored Class profile resolution did not preserve the exact required mod graph.'
}
$craft = $results | Where-Object profileId -ceq 'gunslinger-craft-magic-items'
if ($craft.runtimeCapable -or $craft.staticOnlyReferences.Count -ne 1) { throw 'The generic Craft Magic Items transaction must remain nonstageable.' }
$all = $results | Where-Object profileId -ceq 'gunslinger-all-loadable-local'
if (@($all.runtimeMods | Where-Object key -eq 'kaz-asset-references').Count -ne 0) { throw 'KAZ references entered runtime staging.' }
if (@($all.expectedUmmIds | Where-Object { $_ -like 'KAZ_*' }).Count -ne 0) { throw 'KAZ UMM IDs entered all-loadable profile.' }
Write-Host 'All twelve compatibility profile dry-runs passed; the dedicated CMI authority remains separate from generic staging, three exact Favored Class profiles are runtime capable, and KAZ references remain non-runtime.'
