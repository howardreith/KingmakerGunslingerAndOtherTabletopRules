[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')

$failures = [Collections.Generic.List[string]]::new()
function Assert-Throws([scriptblock]$Action, [string]$Name) {
    try { & $Action; $failures.Add($Name) } catch { }
}

$synthetic = Join-Path $script:KmgRuntimeEvidenceRoot 'source-only-request-test'
$request = New-KmgRuntimeRequest -Scenario 'mod-load-smoke' -ExpectedVersion '0.0.54' `
    -TimeoutSeconds 30 -ExitAfterCompletion $true -EvidenceDirectory $synthetic
if ($request.schemaVersion -ne 1 -or -not $request.enabled -or
    $request.scenario -ne 'mod-load-smoke' -or $request.parameters.Count -ne 0 -or
    $request.startupTimeoutSeconds -ne 180) {
    $failures.Add('valid-request-schema')
}
Assert-Throws { New-KmgRuntimeRequest -Scenario 'unknown' -ExpectedVersion '0.0.54' `
    -TimeoutSeconds 30 -ExitAfterCompletion $false -EvidenceDirectory $synthetic } 'unknown-scenario'
Assert-Throws { New-KmgRuntimeRequest -Scenario 'mod-load-smoke' -ExpectedVersion '' `
    -TimeoutSeconds 30 -ExitAfterCompletion $false -EvidenceDirectory $synthetic } 'empty-version'
Assert-Throws { New-KmgRuntimeRequest -Scenario 'mod-load-smoke' -ExpectedVersion '0.0.54' `
    -TimeoutSeconds 4 -ExitAfterCompletion $false -EvidenceDirectory $synthetic } 'short-timeout'
Assert-Throws { New-KmgRuntimeRequest -Scenario 'mod-load-smoke' -ExpectedVersion '0.0.54' `
    -TimeoutSeconds 30 -StartupTimeoutSeconds 4 -ExitAfterCompletion $false `
    -EvidenceDirectory $synthetic } 'short-startup-timeout'
Assert-Throws { New-KmgRuntimeRequest -Scenario 'mod-load-smoke' -ExpectedVersion '0.0.54' `
    -TimeoutSeconds 30 -ExitAfterCompletion $false -EvidenceDirectory 'C:\Windows\Temp' } 'outside-root'
Assert-Throws { New-KmgRuntimeRequest -Scenario 'mod-load-smoke' -ExpectedVersion '0.0.54' `
    -TimeoutSeconds 30 -ExitAfterCompletion $false -EvidenceDirectory $synthetic `
    -Parameters @{ unexpected = $true } } 'unknown-parameter'
Assert-Throws { New-KmgRuntimeRequest -Scenario 'working-save-smoke' -ExpectedVersion '0.0.54' `
    -TimeoutSeconds 30 -ExitAfterCompletion $false -EvidenceDirectory $synthetic `
    -Parameters @{} } 'working-save-name-missing'
Assert-Throws { New-KmgRuntimeRequest -Scenario 'working-save-smoke' -ExpectedVersion '0.0.54' `
    -TimeoutSeconds 30 -ExitAfterCompletion $false -EvidenceDirectory $synthetic `
    -Parameters @{ saveName = 'KMG_AUTOMATION_BASELINE' } } 'baseline-forbidden'
$featureRequest = New-KmgRuntimeRequest -Scenario 'generic-firearm-actions' `
    -ExpectedVersion '0.0.54' -TimeoutSeconds 30 -ExitAfterCompletion $true `
    -EvidenceDirectory $synthetic -Parameters @{ saveName = 'KMG_AUTOMATION_WORKING' } `
    -CatalogTimeoutSeconds 30 -SelectionTimeoutSeconds 30 `
    -CompletionTimeoutSeconds 30 -MainMenuTimeoutSeconds 30 `
    -ActionResolutionTimeoutSeconds 30 -ActionInvocationTimeoutSeconds 30 `
    -DescriptorResolutionTimeoutSeconds 30 -LoadEntryTimeoutSeconds 30 `
    -FingerprintTimeoutSeconds 30
if ($featureRequest.scenario -cne 'generic-firearm-actions' -or
    $featureRequest.parameters.saveName -cne 'KMG_AUTOMATION_WORKING') {
    $failures.Add('sprint30-feature-request-valid')
}
Assert-Throws { New-KmgRuntimeRequest -Scenario 'generic-firearm-actions' `
    -ExpectedVersion '0.0.54' -TimeoutSeconds 30 -ExitAfterCompletion $false `
    -EvidenceDirectory $synthetic -Parameters @{} } 'sprint30-save-name-missing'
$catalogRequest = New-KmgRuntimeRequest -Scenario 'production-firearm-catalog' `
    -ExpectedVersion '0.0.54' -TimeoutSeconds 30 -ExitAfterCompletion $true `
    -EvidenceDirectory $synthetic -Parameters @{ saveName = 'KMG_AUTOMATION_WORKING' } `
    -CatalogTimeoutSeconds 30 -SelectionTimeoutSeconds 30 `
    -CompletionTimeoutSeconds 30 -MainMenuTimeoutSeconds 30 `
    -ActionResolutionTimeoutSeconds 30 -ActionInvocationTimeoutSeconds 30 `
    -DescriptorResolutionTimeoutSeconds 30 -LoadEntryTimeoutSeconds 30 `
    -FingerprintTimeoutSeconds 30
if ($catalogRequest.scenario -cne 'production-firearm-catalog' -or
    $catalogRequest.parameters.saveName -cne 'KMG_AUTOMATION_WORKING') {
    $failures.Add('sprint31-catalog-request-valid')
}
Assert-Throws { New-KmgRuntimeRequest -Scenario 'production-firearm-catalog' `
    -ExpectedVersion '0.0.54' -TimeoutSeconds 30 -ExitAfterCompletion $false `
    -EvidenceDirectory $synthetic -Parameters @{} } 'sprint31-save-name-missing'
$capacityRequest = New-KmgRuntimeRequest -Scenario 'advanced-capacity' `
    -ExpectedVersion '0.0.54' -TimeoutSeconds 30 -ExitAfterCompletion $true `
    -EvidenceDirectory $synthetic -Parameters @{ saveName = 'KMG_AUTOMATION_WORKING' } `
    -CatalogTimeoutSeconds 30 -SelectionTimeoutSeconds 30 `
    -CompletionTimeoutSeconds 30 -MainMenuTimeoutSeconds 30 `
    -ActionResolutionTimeoutSeconds 30 -ActionInvocationTimeoutSeconds 30 `
    -DescriptorResolutionTimeoutSeconds 30 -LoadEntryTimeoutSeconds 30 `
    -FingerprintTimeoutSeconds 30
if ($capacityRequest.scenario -cne 'advanced-capacity' -or
    $capacityRequest.parameters.saveName -cne 'KMG_AUTOMATION_WORKING') {
    $failures.Add('sprint33-capacity-request-valid')
}
Assert-Throws { New-KmgRuntimeRequest -Scenario 'advanced-capacity' `
    -ExpectedVersion '0.0.54' -TimeoutSeconds 30 -ExitAfterCompletion $false `
    -EvidenceDirectory $synthetic -Parameters @{} } 'sprint33-save-name-missing'
$startingItemsRequest = New-KmgRuntimeRequest -Scenario 'gunslinger-starting-items' `
    -ExpectedVersion '0.0.54' -TimeoutSeconds 30 -ExitAfterCompletion $true `
    -EvidenceDirectory $synthetic -Parameters @{ saveName = 'KMG_AUTOMATION_WORKING' } `
    -CatalogTimeoutSeconds 30 -SelectionTimeoutSeconds 30 `
    -CompletionTimeoutSeconds 30 -MainMenuTimeoutSeconds 30 `
    -ActionResolutionTimeoutSeconds 30 -ActionInvocationTimeoutSeconds 30 `
    -DescriptorResolutionTimeoutSeconds 30 -LoadEntryTimeoutSeconds 30 `
    -FingerprintTimeoutSeconds 30
if ($startingItemsRequest.scenario -cne 'gunslinger-starting-items' -or
    $startingItemsRequest.parameters.saveName -cne 'KMG_AUTOMATION_WORKING') {
    $failures.Add('starting-items-request-valid')
}
Assert-Throws { New-KmgRuntimeRequest -Scenario 'gunslinger-starting-items' `
    -ExpectedVersion '0.0.54' -TimeoutSeconds 30 -ExitAfterCompletion $false `
    -EvidenceDirectory $synthetic -Parameters @{} } 'starting-items-save-name-missing'
$entryRequest = New-KmgRuntimeRequest -Scenario 'observe-working-save-entry-action' `
    -ExpectedVersion '0.0.54' -TimeoutSeconds 30 -ExitAfterCompletion $true `
    -EvidenceDirectory $synthetic -Parameters @{ saveName = 'KMG_AUTOMATION_WORKING' } `
    -CatalogTimeoutSeconds 30 -SelectionTimeoutSeconds 30 `
    -CompletionTimeoutSeconds 30 -MainMenuTimeoutSeconds 30 `
    -ActionResolutionTimeoutSeconds 30 -ActionInvocationTimeoutSeconds 30 `
    -DescriptorResolutionTimeoutSeconds 30 -LoadEntryTimeoutSeconds 30 `
    -FingerprintTimeoutSeconds 30
if ($entryRequest.scenario -cne 'observe-working-save-entry-action' -or
    $entryRequest.parameters.saveName -cne 'KMG_AUTOMATION_WORKING') {
    $failures.Add('entry-request-valid')
}
Assert-Throws { New-KmgRuntimeRequest -Scenario 'observe-working-save-entry-action' `
    -ExpectedVersion '0.0.54' -TimeoutSeconds 30 -ExitAfterCompletion $false `
    -EvidenceDirectory $synthetic -Parameters @{} } 'entry-save-name-missing'
Assert-Throws { New-KmgRuntimeRequest -Scenario 'observe-working-save-entry-action' `
    -ExpectedVersion '0.0.54' -TimeoutSeconds 30 -ExitAfterCompletion $false `
    -EvidenceDirectory $synthetic `
    -Parameters @{ saveName = 'KMG_AUTOMATION_BASELINE' } } 'entry-baseline-forbidden'
$startlingRequest = New-KmgRuntimeRequest `
    -Scenario 'disposable-gunslinger-startling-shot' `
    -ExpectedVersion '0.0.54' -TimeoutSeconds 30 -ExitAfterCompletion $true `
    -EvidenceDirectory $synthetic
if ($startlingRequest.scenario -cne 'disposable-gunslinger-startling-shot' -or
    $startlingRequest.parameters.Count -ne 0) {
    $failures.Add('startling-shot-request-valid')
}
$targetingRequest = New-KmgRuntimeRequest `
    -Scenario 'disposable-gunslinger-targeting-head' `
    -ExpectedVersion '0.0.54' -TimeoutSeconds 30 -ExitAfterCompletion $true `
    -EvidenceDirectory $synthetic
if ($targetingRequest.scenario -cne 'disposable-gunslinger-targeting-head' -or
    $targetingRequest.parameters.Count -ne 0) {
    $failures.Add('targeting-head-request-valid')
}
$torsoRequest = New-KmgRuntimeRequest `
    -Scenario 'disposable-gunslinger-targeting-torso' `
    -ExpectedVersion '0.0.54' -TimeoutSeconds 30 -ExitAfterCompletion $true `
    -EvidenceDirectory $synthetic
if ($torsoRequest.scenario -cne 'disposable-gunslinger-targeting-torso' -or
    $torsoRequest.parameters.Count -ne 0) {
    $failures.Add('targeting-torso-request-valid')
}
$legsRequest = New-KmgRuntimeRequest `
    -Scenario 'disposable-gunslinger-targeting-legs' `
    -ExpectedVersion '0.0.54' -TimeoutSeconds 30 -ExitAfterCompletion $true `
    -EvidenceDirectory $synthetic
if ($legsRequest.scenario -cne 'disposable-gunslinger-targeting-legs' -or
    $legsRequest.parameters.Count -ne 0) {
    $failures.Add('targeting-legs-request-valid')
}
$bleedingRequest = New-KmgRuntimeRequest `
    -Scenario 'disposable-gunslinger-bleeding-wound' `
    -ExpectedVersion '0.0.54' -TimeoutSeconds 30 -ExitAfterCompletion $true `
    -EvidenceDirectory $synthetic
if ($bleedingRequest.scenario -cne 'disposable-gunslinger-bleeding-wound' -or
    $bleedingRequest.parameters.Count -ne 0) {
    $failures.Add('bleeding-wound-request-valid')
}
$expertLoadingRequest = New-KmgRuntimeRequest `
    -Scenario 'disposable-gunslinger-expert-loading' `
    -ExpectedVersion '0.0.54' -TimeoutSeconds 30 -ExitAfterCompletion $true `
    -EvidenceDirectory $synthetic
if ($expertLoadingRequest.scenario -cne 'disposable-gunslinger-expert-loading' -or
    $expertLoadingRequest.parameters.Count -ne 0) {
    $failures.Add('expert-loading-request-valid')
}
$lightningReloadRequest = New-KmgRuntimeRequest `
    -Scenario 'disposable-gunslinger-lightning-reload' `
    -ExpectedVersion '0.0.54' -TimeoutSeconds 30 -ExitAfterCompletion $true `
    -EvidenceDirectory $synthetic
if ($lightningReloadRequest.scenario -cne 'disposable-gunslinger-lightning-reload' -or
    $lightningReloadRequest.parameters.Count -ne 0) {
    $failures.Add('lightning-reload-request-valid')
}
$evasiveRequest = New-KmgRuntimeRequest `
    -Scenario 'disposable-gunslinger-evasive' `
    -ExpectedVersion '0.0.54' -TimeoutSeconds 30 -ExitAfterCompletion $true `
    -EvidenceDirectory $synthetic
if ($evasiveRequest.scenario -cne 'disposable-gunslinger-evasive' -or
    $evasiveRequest.parameters.Count -ne 0) {
    $failures.Add('evasive-request-valid')
}
$evasiveObserverRequest = New-KmgRuntimeRequest `
    -Scenario 'observe-evasive-native-features' `
    -ExpectedVersion '0.0.54' -TimeoutSeconds 30 -ExitAfterCompletion $true `
    -EvidenceDirectory $synthetic
if ($evasiveObserverRequest.scenario -cne 'observe-evasive-native-features' -or
    $evasiveObserverRequest.parameters.Count -ne 0) {
    $failures.Add('evasive-observer-request-valid')
}
$menacingObserverRequest = New-KmgRuntimeRequest `
    -Scenario 'observe-menacing-shot-native-fear' `
    -ExpectedVersion '0.0.54' -TimeoutSeconds 30 -ExitAfterCompletion $true `
    -EvidenceDirectory $synthetic
if ($menacingObserverRequest.scenario -cne 'observe-menacing-shot-native-fear' -or
    $menacingObserverRequest.parameters.Count -ne 0) {
    $failures.Add('menacing-observer-request-valid')
}
$menacingRequest = New-KmgRuntimeRequest `
    -Scenario 'disposable-gunslinger-menacing-shot' `
    -ExpectedVersion '0.0.54' -TimeoutSeconds 30 -ExitAfterCompletion $true `
    -EvidenceDirectory $synthetic
if ($menacingRequest.scenario -cne 'disposable-gunslinger-menacing-shot' -or
    $menacingRequest.parameters.Count -ne 0) {
    $failures.Add('menacing-shot-request-valid')
}
$slingersLuckObserverRequest = New-KmgRuntimeRequest `
    -Scenario 'observe-slingers-luck-native-rerolls' `
    -ExpectedVersion '0.0.54' -TimeoutSeconds 30 -ExitAfterCompletion $true `
    -EvidenceDirectory $synthetic
if ($slingersLuckObserverRequest.scenario -cne
    'observe-slingers-luck-native-rerolls' -or
    $slingersLuckObserverRequest.parameters.Count -ne 0) {
    $failures.Add('slingers-luck-observer-request-valid')
}

if ($failures.Count -ne 0) { throw "Runtime request tests failed: $($failures -join ', ')" }
Write-Host 'Runtime request source tests passed: 30'
