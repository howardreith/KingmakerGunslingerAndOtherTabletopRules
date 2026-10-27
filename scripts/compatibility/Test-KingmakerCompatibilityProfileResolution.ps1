[CmdletBinding()]
param(
    [string[]]$ProfileId,
    [string]$ReferenceRoot,
    [string]$PackagePath
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..')).Path
$profiles = Get-Content -LiteralPath (Join-Path $root 'compatibility\profiles.json') -Raw | ConvertFrom-Json
$results = @()
$selected = @($profiles.profiles | Where-Object {
    if ($ProfileId) { $_.id -cin $ProfileId }
    else { $_.requiredGunslingerPackage -cne 'explicit-validated-candidate' }
})
if (-not $ProfileId) {
    foreach ($profile in @($profiles.profiles | Where-Object requiredGunslingerPackage -ceq 'explicit-validated-candidate')) {
        Write-Host ("NOT_RUN: " + $profile.id + ' requires explicit -ProfileId, -ReferenceRoot and -PackagePath.')
    }
}
if ($selected.Count -eq 0 -or ($ProfileId -and @($ProfileId | Where-Object { $_ -cnotin @($selected.id) }).Count)) {
    throw 'Profile selection is empty or includes an unknown exact ID.'
}
foreach ($profile in $selected) {
    $arguments = @{ ProfileId = $profile.id }
    if ($ReferenceRoot) { $arguments.ReferenceRoot = $ReferenceRoot }
    if ($PackagePath) { $arguments.PackagePath = $PackagePath }
    $path = & (Join-Path $PSScriptRoot 'Resolve-KingmakerCompatibilityProfile.ps1') @arguments | Select-Object -Last 1
    $result = Get-Content -LiteralPath $path -Raw | ConvertFrom-Json
    if ($result.profileId -cne $profile.id) { throw "Dry-run profile mismatch: $($profile.id)" }
    $results += $result
}
if ($ProfileId) {
    Write-Host ('Selected compatibility resolution PASS: ' + (($results | ForEach-Object profileId) -join ', ') + '; no staging or game launch.')
    return
}
$runtime = @($results | Where-Object runtimeCapable)
$expectedRuntime = @($selected | Where-Object runtimeLoadableRequired)
if ($runtime.Count -ne $expectedRuntime.Count) { throw 'Declared runtime-capable profiles did not all resolve.' }
$racesUnleashed = @($results | Where-Object {
    $_.profileId -ceq 'gunslinger-races-unleashed'
})[0]
$cotwRacesUnleashed = @($results | Where-Object {
    $_.profileId -ceq 'gunslinger-call-of-the-wild-races-unleashed'
})[0]
if (-not $racesUnleashed.runtimeCapable -or
    @($racesUnleashed.runtimeMods | Where-Object {
        $_.key -ceq 'races-unleashed'
    }).Count -ne 1 -or
    @($racesUnleashed.runtimeMods | Where-Object {
        $_.key -ceq 'call-of-the-wild'
    }).Count -ne 0 -or
    -not $cotwRacesUnleashed.runtimeCapable -or
    @($cotwRacesUnleashed.runtimeMods | Where-Object {
        $_.key -ceq 'races-unleashed'
    }).Count -ne 1 -or
    @($cotwRacesUnleashed.runtimeMods | Where-Object {
        $_.key -ceq 'call-of-the-wild'
    }).Count -ne 1) {
    throw 'The two exact Races Unleashed profiles did not preserve their required mod graphs.'
}
$tweak = @($results | Where-Object {
    $_.profileId -ceq 'gunslinger-tweak-or-treat'
})[0]
if (-not $tweak.runtimeCapable -or
    @($tweak.runtimeMods | Where-Object key -ceq 'call-of-the-wild').Count -ne 1 -or
    @($tweak.runtimeMods | Where-Object key -ceq 'races-unleashed').Count -ne 1 -or
    @($tweak.runtimeMods | Where-Object key -ceq 'tweak-or-treat').Count -ne 1 -or
    @($tweak.runtimeMods | Where-Object key -ceq 'favored-class').Count -ne 0 -or
    @($tweak.expectedUmmIds).Count -ne 4) {
    throw 'The minimum Tweak or Treat profile did not preserve its exact dependency graph or Favored Class isolation.'
}
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
Write-Host 'All selected compatibility profile dry-runs passed; required dependency graphs resolve, dedicated CMI authority remains separately staged, and KAZ references remain non-runtime.'
