$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'ExpandedSummoningOrchestration.Common.ps1')

if (-not (Test-KmgSprint29DevelopmentPersistence `
    -Scope giant-humanoids-ii -Purpose sprint29-hidden-bootstrap)) {
    throw 'Exact Sprint 29 hidden development scope refused.'
}
if (Test-KmgSprint29DevelopmentPersistence `
    -Scope giant-humanoids-ii -Purpose '') {
    throw 'Release candidate was treated as hidden development.'
}
$count = 2
foreach ($case in @(
    @('Giant-humanoids-ii', 'sprint29-hidden-bootstrap'),
    @('giant-humanoids-ii', 'Sprint29-hidden-bootstrap'),
    @('giant-humanoids-ii', 'sprint28-native-prototype'),
    @('giant-humanoids', 'sprint29-hidden-bootstrap'),
    @('whole-roster', 'sprint29-hidden-bootstrap'),
    @('giant-humanoids-ii', 'sprint29-hidden-bootstrap ')
)) {
    $refused = $false
    try {
        [void](Test-KmgSprint29DevelopmentPersistence `
            -Scope $case[0] -Purpose $case[1])
    } catch { $refused = $true }
    if (-not $refused) {
        throw 'Invalid Sprint 29 hidden development persistence authority accepted.'
    }
    $count++
}
Write-Host "Sprint 29 hidden development persistence authority PASS=$count; pure scope/refusal checks only."
