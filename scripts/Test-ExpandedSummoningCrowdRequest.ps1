[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')

$base = @{
    Scenario = 'working-save-expanded-summoning-creature-review'
    ExpectedVersion = '0.0.141'
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
    EvidenceDirectory = (Join-Path $script:KmgRuntimeEvidenceRoot 'ground-crowd-request-test')
}
$rosters = @('aurochs,bison,rhinoceros,woolly-rhinoceros',
    'dire-rat,dog,hyena,goblin-dog', 'crocodile,dire-crocodile',
    'crocodile', 'dire-crocodile', 'viper,constrictor-snake', 'viper', 'constrictor-snake')
foreach ($roster in $rosters) {
    $allowed = @{ saveName = 'KMG_AUTOMATION_WORKING'
        creatures = $roster; quantity = 'OneD4PlusOne' }
    $request = New-KmgRuntimeRequest @base -Parameters $allowed
    if ($request.parameters.quantity -cne 'OneD4PlusOne' -or
        $request.parameters.creatures -cne $roster -or
        $request.parameters.saveName -cne 'KMG_AUTOMATION_WORKING') {
        throw "The bounded crowd request did not round-trip exactly: $roster."
    }
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

Assert-Rejected @{ saveName = 'KMG_AUTOMATION_WORKING'
    creatures = 'crocodile,purple-worm'; quantity = 'OneD4PlusOne' } 'an unrelated swallower'
Assert-Rejected @{ saveName = 'KMG_AUTOMATION_WORKING'
    creatures = 'Dire-Crocodile'; quantity = 'OneD4PlusOne' } 'a differently cased creature key'
Assert-Rejected @{ saveName = 'KMG_AUTOMATION_WORKING'
    creatures = 'dire-crocodile'; quantity = 'OneD3' } 'an unauthorized crocodilian quantity'

Assert-Rejected @{ saveName = 'KMG_AUTOMATION_WORKING'
    creatures = 'viper,salamander'; quantity = 'OneD4PlusOne' } 'an unqualified hybrid crowd'
Assert-Rejected @{ saveName = 'KMG_AUTOMATION_WORKING'
    creatures = 'Viper'; quantity = 'OneD4PlusOne' } 'a differently cased snake'
Assert-Rejected @{ saveName = 'KMG_AUTOMATION_WORKING'
    creatures = 'constrictor-snake'; quantity = 'OneD3' } 'an unauthorized snake quantity'
Assert-Rejected @{ saveName = 'KMG_AUTOMATION_WORKING'
    creatures = 'viper,foreign'; quantity = 'OneD4PlusOne' } 'an arbitrary added key'

Write-Host 'PASS ground crowd request: eight exact round trips and eleven fail-closed cases.'
