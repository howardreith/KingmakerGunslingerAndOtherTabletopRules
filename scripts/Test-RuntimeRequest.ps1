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
$request = New-KmgRuntimeRequest -Scenario 'mod-load-smoke' -ExpectedVersion '0.0.40' `
    -TimeoutSeconds 30 -ExitAfterCompletion $true -EvidenceDirectory $synthetic
if ($request.schemaVersion -ne 1 -or -not $request.enabled -or
    $request.scenario -ne 'mod-load-smoke' -or $request.parameters.Count -ne 0 -or
    $request.startupTimeoutSeconds -ne 180) {
    $failures.Add('valid-request-schema')
}
Assert-Throws { New-KmgRuntimeRequest -Scenario 'unknown' -ExpectedVersion '0.0.40' `
    -TimeoutSeconds 30 -ExitAfterCompletion $false -EvidenceDirectory $synthetic } 'unknown-scenario'
Assert-Throws { New-KmgRuntimeRequest -Scenario 'mod-load-smoke' -ExpectedVersion '' `
    -TimeoutSeconds 30 -ExitAfterCompletion $false -EvidenceDirectory $synthetic } 'empty-version'
Assert-Throws { New-KmgRuntimeRequest -Scenario 'mod-load-smoke' -ExpectedVersion '0.0.40' `
    -TimeoutSeconds 4 -ExitAfterCompletion $false -EvidenceDirectory $synthetic } 'short-timeout'
Assert-Throws { New-KmgRuntimeRequest -Scenario 'mod-load-smoke' -ExpectedVersion '0.0.40' `
    -TimeoutSeconds 30 -StartupTimeoutSeconds 4 -ExitAfterCompletion $false `
    -EvidenceDirectory $synthetic } 'short-startup-timeout'
Assert-Throws { New-KmgRuntimeRequest -Scenario 'mod-load-smoke' -ExpectedVersion '0.0.40' `
    -TimeoutSeconds 30 -ExitAfterCompletion $false -EvidenceDirectory 'C:\Windows\Temp' } 'outside-root'
Assert-Throws { New-KmgRuntimeRequest -Scenario 'mod-load-smoke' -ExpectedVersion '0.0.40' `
    -TimeoutSeconds 30 -ExitAfterCompletion $false -EvidenceDirectory $synthetic `
    -Parameters @{ unexpected = $true } } 'unknown-parameter'
Assert-Throws { New-KmgRuntimeRequest -Scenario 'working-save-smoke' -ExpectedVersion '0.0.40' `
    -TimeoutSeconds 30 -ExitAfterCompletion $false -EvidenceDirectory $synthetic `
    -Parameters @{} } 'working-save-name-missing'
Assert-Throws { New-KmgRuntimeRequest -Scenario 'working-save-smoke' -ExpectedVersion '0.0.40' `
    -TimeoutSeconds 30 -ExitAfterCompletion $false -EvidenceDirectory $synthetic `
    -Parameters @{ saveName = 'KMG_AUTOMATION_BASELINE' } } 'baseline-forbidden'
$featureRequest = New-KmgRuntimeRequest -Scenario 'generic-firearm-actions' `
    -ExpectedVersion '0.0.40' -TimeoutSeconds 30 -ExitAfterCompletion $true `
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
    -ExpectedVersion '0.0.40' -TimeoutSeconds 30 -ExitAfterCompletion $false `
    -EvidenceDirectory $synthetic -Parameters @{} } 'sprint30-save-name-missing'
$catalogRequest = New-KmgRuntimeRequest -Scenario 'production-firearm-catalog' `
    -ExpectedVersion '0.0.40' -TimeoutSeconds 30 -ExitAfterCompletion $true `
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
    -ExpectedVersion '0.0.40' -TimeoutSeconds 30 -ExitAfterCompletion $false `
    -EvidenceDirectory $synthetic -Parameters @{} } 'sprint31-save-name-missing'
$capacityRequest = New-KmgRuntimeRequest -Scenario 'advanced-capacity' `
    -ExpectedVersion '0.0.40' -TimeoutSeconds 30 -ExitAfterCompletion $true `
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
    -ExpectedVersion '0.0.40' -TimeoutSeconds 30 -ExitAfterCompletion $false `
    -EvidenceDirectory $synthetic -Parameters @{} } 'sprint33-save-name-missing'
$startingItemsRequest = New-KmgRuntimeRequest -Scenario 'gunslinger-starting-items' `
    -ExpectedVersion '0.0.40' -TimeoutSeconds 30 -ExitAfterCompletion $true `
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
    -ExpectedVersion '0.0.40' -TimeoutSeconds 30 -ExitAfterCompletion $false `
    -EvidenceDirectory $synthetic -Parameters @{} } 'starting-items-save-name-missing'
$entryRequest = New-KmgRuntimeRequest -Scenario 'observe-working-save-entry-action' `
    -ExpectedVersion '0.0.40' -TimeoutSeconds 30 -ExitAfterCompletion $true `
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
    -ExpectedVersion '0.0.40' -TimeoutSeconds 30 -ExitAfterCompletion $false `
    -EvidenceDirectory $synthetic -Parameters @{} } 'entry-save-name-missing'
Assert-Throws { New-KmgRuntimeRequest -Scenario 'observe-working-save-entry-action' `
    -ExpectedVersion '0.0.40' -TimeoutSeconds 30 -ExitAfterCompletion $false `
    -EvidenceDirectory $synthetic `
    -Parameters @{ saveName = 'KMG_AUTOMATION_BASELINE' } } 'entry-baseline-forbidden'

if ($failures.Count -ne 0) { throw "Runtime request tests failed: $($failures -join ', ')" }
Write-Host 'Runtime request source tests passed: 18'
