[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')

$base = @{
    Scenario = 'working-save-expanded-summoning-creature-review'
    ExpectedVersion = '0.0.140'
    TimeoutSeconds = 1200
    CatalogTimeoutSeconds = 180
    SelectionTimeoutSeconds = 300
    CompletionTimeoutSeconds = 180
    MainMenuTimeoutSeconds = 180
    ActionResolutionTimeoutSeconds = 180
    ActionInvocationTimeoutSeconds = 30
    DescriptorResolutionTimeoutSeconds = 30
    LoadEntryTimeoutSeconds = 30
    FingerprintTimeoutSeconds = 180
    ExitAfterCompletion = $true
    EvidenceDirectory = (Join-Path $script:KmgRuntimeEvidenceRoot 'ungulate-crowd-request-test')
}
$allowed = @{ saveName = 'KMG_AUTOMATION_WORKING'
    creatures = 'aurochs,bison,rhinoceros,woolly-rhinoceros'
    quantity = 'OneD4PlusOne' }
$request = New-KmgRuntimeRequest @base -Parameters $allowed
if ($request.parameters.quantity -cne 'OneD4PlusOne' -or
    $request.parameters.creatures -cne $allowed.creatures -or
    $request.parameters.saveName -cne 'KMG_AUTOMATION_WORKING') {
    throw 'The bounded crowd request did not round-trip exactly.'
}

function Assert-Rejected([hashtable]$parameters, [string]$label) {
    try {
        $null = New-KmgRuntimeRequest @base -Parameters $parameters
    } catch { return }
    throw "The crowd request accepted $label."
}
Assert-Rejected @{ saveName = 'KMG_AUTOMATION_WORKING'
    creatures = 'aurochs,wolf'; quantity = 'OneD4PlusOne' } 'an unrelated creature'
Assert-Rejected @{ saveName = 'KMG_AUTOMATION_WORKING'
    creatures = 'aurochs'; quantity = 'OneD3' } 'an unauthorized quantity'
Assert-Rejected @{ saveName = 'KMG_AUTOMATION_BASELINE'
    creatures = 'aurochs'; quantity = 'OneD4PlusOne' } 'the protected save'
Assert-Rejected @{ saveName = 'KMG_AUTOMATION_WORKING'
    creatures = 'aurochs'; quantity = 'OneD4PlusOne'; extra = 'unexpected' } `
    'an unexpected parameter'

Write-Host 'PASS Sprint 11 crowd request: exact round trip and four fail-closed cases.'
