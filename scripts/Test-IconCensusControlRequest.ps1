[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
. (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')
$iconControlArgs = @{Scenario='icon-overhaul-visual-evidence'; ExpectedVersion='0.0.127';
    TimeoutSeconds=300; ExitAfterCompletion=$true; Parameters=@{iconCensusControl=$true}}
function Assert-IconControlRejected([string]$Name) {
    $rejected = $false
    try { Assert-KmgRuntimeScenarioPreflight @iconControlArgs | Out-Null }
    catch { $rejected = $true }
    if (-not $rejected) { throw "Icon control accepted an invalid request: $Name" }
}
Assert-KmgRuntimeScenarioPreflight @iconControlArgs | Out-Null
foreach ($invalid in @($false, 'true', 1, $null)) {
    $iconControlArgs.Parameters=@{iconCensusControl=$invalid}
    Assert-IconControlRejected 'parameter type/value'
}
$iconControlArgs.Parameters=@{iconCensusControl=$true; saveName='KMG_AUTOMATION_WORKING'}
Assert-IconControlRejected 'extra/save parameter'
$iconControlArgs.Parameters=@{iconCensusControl=$true}; $iconControlArgs.ExitAfterCompletion=$false
Assert-IconControlRejected 'automatic exit required'
$iconControlArgs.ExitAfterCompletion=$true; $iconControlArgs.Scenario='observe-native-weapon-feat-contracts'
Assert-IconControlRejected 'unrelated scenario'
$iconControlArgs.Scenario='icon-overhaul-visual-evidence'; $iconControlArgs.Parameters=@{}
Assert-KmgRuntimeScenarioPreflight @iconControlArgs | Out-Null
$testEvidence = Join-Path $script:KmgRuntimeEvidenceRoot 'icon-census-request-roundtrip-test'
$request = New-KmgRuntimeRequest @iconControlArgs -EvidenceDirectory $testEvidence
$roundtrip = $request | ConvertTo-Json -Depth 8 | ConvertFrom-Json
if (@($roundtrip.parameters.PSObject.Properties).Count -ne 0) { throw 'Normal census gained parameters.' }
$iconControlArgs.Parameters=@{iconCensusControl=$true}
$request = New-KmgRuntimeRequest @iconControlArgs -EvidenceDirectory $testEvidence
$roundtrip = $request | ConvertTo-Json -Depth 8 | ConvertFrom-Json
if (@($roundtrip.parameters.PSObject.Properties).Count -ne 1 -or
    $roundtrip.parameters.iconCensusControl -isnot [bool] -or -not $roundtrip.parameters.iconCensusControl -or
    -not $roundtrip.exitAfterCompletion -or $roundtrip.scenario -cne 'icon-overhaul-visual-evidence') {
    throw 'Serialized icon census control lost its exact mode, type or exit guard.'
}
Write-Host 'PASS: eleven icon request preflight/JSON round-trip cases; no deployment, evidence directory or launch created.'
