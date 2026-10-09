[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1')
. (Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1')

$base = @{
    Scenario = 'working-save-expanded-summoning-creature-review'
    ExpectedVersion = (Get-KmgModInfo -RepositoryRoot (Split-Path -Parent $PSScriptRoot)).Version
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
    'crocodile', 'dire-crocodile', 'viper,constrictor-snake', 'viper', 'constrictor-snake',
    'salamander', 'viper,constrictor-snake,salamander')
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
    creatures = 'salamander,foreign'; quantity = 'OneD4PlusOne' } 'a foreign actor appended to the closed hybrid crowd'
Assert-Rejected @{ saveName = 'KMG_AUTOMATION_WORKING'
    creatures = 'Viper'; quantity = 'OneD4PlusOne' } 'a differently cased snake'
Assert-Rejected @{ saveName = 'KMG_AUTOMATION_WORKING'
    creatures = 'constrictor-snake'; quantity = 'OneD3' } 'an unauthorized snake quantity'
Assert-Rejected @{ saveName = 'KMG_AUTOMATION_WORKING'
    creatures = 'viper,foreign'; quantity = 'OneD4PlusOne' } 'an arbitrary added key'

Write-Host 'PASS ground crowd request: ten exact round trips and eleven fail-closed cases.'

# Same existing player-path scenario, explicitly bounded for release integration.
$pathBase = $base.Clone()
$pathBase.Scenario = 'disposable-expanded-summoning-player-path'
$defaultPath = New-KmgRuntimeRequest @pathBase -Parameters @{saveName='KMG_AUTOMATION_WORKING'}
if ($defaultPath.parameters.Count -ne 1) { throw 'Default exhaustive path census changed.' }
$representative = New-KmgRuntimeRequest @pathBase -Parameters @{
    saveName='KMG_AUTOMATION_WORKING'; playerPathScope='representative'}
$roundTrip = $representative | ConvertTo-Json -Depth 20 | ConvertFrom-Json
if ($roundTrip.parameters.playerPathScope -cne 'representative' -or
    $representative.parameters.Count -ne 2) { throw 'Representative player-path scope did not round-trip.' }
foreach ($badScope in @($null, '', 'Representative', 'all', @('representative'), 1)) {
    $rejected = $false
    try { $null = New-KmgRuntimeRequest @pathBase -Parameters @{
        saveName='KMG_AUTOMATION_WORKING';playerPathScope=$badScope} } catch { $rejected=$true }
    if (-not $rejected) { throw 'Invalid player-path scope accepted.' }
}
foreach ($badParameters in @(
    @{saveName='KMG_AUTOMATION_BASELINE';playerPathScope='representative'},
    @{saveName='KMG_AUTOMATION_WORKING';playerPathScope='representative';extra='forbidden'})) {
    $rejected=$false
    try { $null=New-KmgRuntimeRequest @pathBase -Parameters $badParameters } catch { $rejected=$true }
    if (-not $rejected) { throw 'Unsafe player-path request accepted.' }
}
$pathBase.ExitAfterCompletion=$false
$rejected=$false
try { $null=New-KmgRuntimeRequest @pathBase -Parameters @{
    saveName='KMG_AUTOMATION_WORKING';playerPathScope='representative'} } catch { $rejected=$true }
if (-not $rejected) { throw 'Non-exiting representative path request accepted.' }
$pathBase.ExitAfterCompletion=$true
$pathBase.Scenario='working-save-smoke'
$rejected=$false
try { $null=New-KmgRuntimeRequest @pathBase -Parameters @{
    saveName='KMG_AUTOMATION_WORKING';playerPathScope='representative'} } catch { $rejected=$true }
if (-not $rejected) { throw 'Player-path scope leaked into another scenario.' }
Write-Host 'PASS representative player-path request: default exhaustive, exact JSON round trip and ten fail-closed cases.'
