[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$commonPath = Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1'
$orchestratorPath = Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1'
$catalogPath = Join-Path $root `
    'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestScenarioCatalog.cs'
. $commonPath

$failures = [Collections.Generic.List[string]]::new()
$checks = 0
function Assert-True([bool]$Condition, [string]$Name) {
    $script:checks++
    if (-not $Condition) { $script:failures.Add($Name) }
}
function Assert-Throws([scriptblock]$Action, [string]$Name) {
    $script:checks++
    try { & $Action; $script:failures.Add($Name) } catch { }
}

$expected = @(
    'mod-load-smoke',
    'observe-class-blueprint-contracts',
    'observe-character-creation-contracts',
    'disposable-descriptor-construction',
    'disposable-gunslinger-selection',
    'disposable-gunslinger-preview-application',
    'disposable-gunslinger-levelup-preview',
    'disposable-gunslinger-multiclass-preview',
    'disposable-gunslinger-respec-preview',
    'disposable-gunslinger-grit-resource',
    'disposable-gunslinger-grit-rest',
    'disposable-gunslinger-grit-persistence',
    'disposable-gunslinger-grit-recovery',
    'disposable-gunslinger-deadeye',
    'disposable-gunslinger-dodge',
    'disposable-gunslinger-quick-clear',
    'disposable-gunslinger-nimble',
    'disposable-gunslinger-initiative',
    'disposable-gunslinger-pistol-whip',
    'disposable-gunslinger-stop-bleeding',
    'disposable-gunslinger-bonus-feats',
    'disposable-gunslinger-gun-training',
    'disposable-gunslinger-dead-shot',
    'disposable-gunslinger-startling-shot',
    'disposable-gunslinger-targeting-head',
    'disposable-gunslinger-targeting-torso',
    'disposable-gunslinger-targeting-legs',
    'disposable-gunslinger-bleeding-wound',
    'disposable-gunslinger-expert-loading',
    'disposable-gunslinger-lightning-reload',
    'disposable-gunslinger-evasive',
    'observe-evasive-native-features',
    'observe-menacing-shot-native-fear',
    'disposable-gunslinger-menacing-shot',
    'observe-manual-save-load',
    'observe-save-catalog-and-selection',
    'observe-save-catalog-provider',
    'observe-load-game-button-action',
    'working-save-smoke',
    'generic-firearm-actions',
    'production-firearm-catalog',
    'advanced-capacity',
    'gunslinger-starting-items',
    'observe-working-save-entry-action',
    'observe-working-save-selection-load-action',
    'observe-working-save-receiver-bound-action'
)
$catalog = Get-Content -LiteralPath $catalogPath -Raw
$csharpNames = @([regex]::Matches($catalog, '"([a-z][a-z-]+)"') |
    ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique)
$powershellNames = @($script:KmgRuntimeScenarios | Sort-Object)
Assert-True (($csharpNames -join "`n") -ceq ($powershellNames -join "`n")) `
    'csharp-powershell-catalog-sync'
Assert-True (($expected | Sort-Object) -join "`n" -ceq
    ($powershellNames -join "`n")) 'documented-scenarios-retained'

$entry = Get-KmgRuntimeScenarioMetadata 'observe-working-save-entry-action'
Assert-True $entry.RequiresManualInteraction 'entry-requires-manual-interaction'
Assert-True $entry.RequiresSaveName 'entry-requires-save-name'
Assert-True ($entry.PermittedSaveName -ceq 'KMG_AUTOMATION_WORKING') `
    'entry-only-permits-working-save'
$feature = Get-KmgRuntimeScenarioMetadata 'generic-firearm-actions'
Assert-True (-not $feature.RequiresManualInteraction) `
    'sprint30-feature-is-autonomous'
Assert-True $feature.RequiresSaveName 'sprint30-feature-requires-save-name'
Assert-True ($feature.PermittedSaveName -ceq 'KMG_AUTOMATION_WORKING') `
    'sprint30-feature-only-permits-working-save'
$productionCatalog = Get-KmgRuntimeScenarioMetadata 'production-firearm-catalog'
Assert-True (-not $productionCatalog.RequiresManualInteraction) `
    'sprint31-catalog-is-autonomous'
Assert-True $productionCatalog.RequiresSaveName `
    'sprint31-catalog-requires-save-name'
Assert-True ($productionCatalog.PermittedSaveName -ceq 'KMG_AUTOMATION_WORKING') `
    'sprint31-catalog-only-permits-working-save'
$advancedCapacity = Get-KmgRuntimeScenarioMetadata 'advanced-capacity'
Assert-True (-not $advancedCapacity.RequiresManualInteraction) `
    'sprint33-capacity-is-autonomous'
Assert-True $advancedCapacity.RequiresSaveName `
    'sprint33-capacity-requires-save-name'
Assert-True ($advancedCapacity.PermittedSaveName -ceq 'KMG_AUTOMATION_WORKING') `
    'sprint33-capacity-only-permits-working-save'
$startingItems = Get-KmgRuntimeScenarioMetadata 'gunslinger-starting-items'
Assert-True (-not $startingItems.RequiresManualInteraction) `
    'starting-items-is-autonomous'
Assert-True $startingItems.RequiresSaveName `
    'starting-items-requires-save-name'
Assert-True ($startingItems.PermittedSaveName -ceq 'KMG_AUTOMATION_WORKING') `
    'starting-items-only-permits-working-save'
$startlingShot = Get-KmgRuntimeScenarioMetadata `
    'disposable-gunslinger-startling-shot'
Assert-True (-not $startlingShot.RequiresManualInteraction) `
    'startling-shot-is-autonomous'
Assert-True (-not $startlingShot.RequiresSaveName) `
    'startling-shot-is-save-free'
$targetingHead = Get-KmgRuntimeScenarioMetadata `
    'disposable-gunslinger-targeting-head'
Assert-True (-not $targetingHead.RequiresManualInteraction) `
    'targeting-head-is-autonomous'
Assert-True (-not $targetingHead.RequiresSaveName) `
    'targeting-head-is-save-free'
$targetingTorso = Get-KmgRuntimeScenarioMetadata `
    'disposable-gunslinger-targeting-torso'
Assert-True (-not $targetingTorso.RequiresManualInteraction) `
    'targeting-torso-is-autonomous'
Assert-True (-not $targetingTorso.RequiresSaveName) `
    'targeting-torso-is-save-free'
$targetingLegs = Get-KmgRuntimeScenarioMetadata `
    'disposable-gunslinger-targeting-legs'
Assert-True (-not $targetingLegs.RequiresManualInteraction) `
    'targeting-legs-is-autonomous'
Assert-True (-not $targetingLegs.RequiresSaveName) `
    'targeting-legs-is-save-free'
$bleedingWound = Get-KmgRuntimeScenarioMetadata `
    'disposable-gunslinger-bleeding-wound'
Assert-True (-not $bleedingWound.RequiresManualInteraction) `
    'bleeding-wound-is-autonomous'
Assert-True (-not $bleedingWound.RequiresSaveName) `
    'bleeding-wound-is-save-free'
$expertLoading = Get-KmgRuntimeScenarioMetadata `
    'disposable-gunslinger-expert-loading'
Assert-True (-not $expertLoading.RequiresManualInteraction) `
    'expert-loading-is-autonomous'
Assert-True (-not $expertLoading.RequiresSaveName) `
    'expert-loading-is-save-free'
$lightningReload = Get-KmgRuntimeScenarioMetadata `
    'disposable-gunslinger-lightning-reload'
Assert-True (-not $lightningReload.RequiresManualInteraction) `
    'lightning-reload-is-autonomous'
Assert-True (-not $lightningReload.RequiresSaveName) `
    'lightning-reload-is-save-free'
$evasive = Get-KmgRuntimeScenarioMetadata 'disposable-gunslinger-evasive'
Assert-True (-not $evasive.RequiresManualInteraction) `
    'evasive-is-autonomous'
Assert-True (-not $evasive.RequiresSaveName) 'evasive-is-save-free'
$evasiveObserver = Get-KmgRuntimeScenarioMetadata `
    'observe-evasive-native-features'
Assert-True (-not $evasiveObserver.RequiresManualInteraction) `
    'evasive-observer-is-autonomous'
Assert-True (-not $evasiveObserver.RequiresSaveName) `
    'evasive-observer-is-save-free'
$menacingObserver = Get-KmgRuntimeScenarioMetadata `
    'observe-menacing-shot-native-fear'
Assert-True (-not $menacingObserver.RequiresManualInteraction) `
    'menacing-observer-is-autonomous'
Assert-True (-not $menacingObserver.RequiresSaveName) `
    'menacing-observer-is-save-free'
$menacingShot = Get-KmgRuntimeScenarioMetadata `
    'disposable-gunslinger-menacing-shot'
Assert-True (-not $menacingShot.RequiresManualInteraction) `
    'menacing-shot-is-autonomous'
Assert-True (-not $menacingShot.RequiresSaveName) `
    'menacing-shot-is-save-free'

$valid = @{
    Scenario = 'observe-working-save-entry-action'
    ExpectedVersion = '0.0.54'
    TimeoutSeconds = 120
    StartupTimeoutSeconds = 180
    CatalogTimeoutSeconds = 180
    SelectionTimeoutSeconds = 300
    CompletionTimeoutSeconds = 180
    MainMenuTimeoutSeconds = 180
    ActionResolutionTimeoutSeconds = 180
    ActionInvocationTimeoutSeconds = 30
    DescriptorResolutionTimeoutSeconds = 30
    LoadEntryTimeoutSeconds = 30
    FingerprintTimeoutSeconds = 180
    Parameters = @{ saveName = 'KMG_AUTOMATION_WORKING' }
    EnforceManualInteraction = $true
    ManualInteractionRequired = $true
}
Assert-True ($null -ne (Assert-KmgRuntimeScenarioPreflight @valid)) `
    'valid-entry-reaches-request-validation'

$missingSave = $valid.Clone()
$missingSave.Parameters = @{}
Assert-Throws { Assert-KmgRuntimeScenarioPreflight @missingSave } `
    'missing-save-fails-pure-preflight'
$baseline = $valid.Clone()
$baseline.Parameters = @{ saveName = 'KMG_AUTOMATION_BASELINE' }
Assert-Throws { Assert-KmgRuntimeScenarioPreflight @baseline } `
    'baseline-rejected-pure-preflight'
$missingManual = $valid.Clone()
$missingManual.ManualInteractionRequired = $false
Assert-Throws { Assert-KmgRuntimeScenarioPreflight @missingManual } `
    'missing-manual-fails-pure-preflight'
Assert-Throws {
    Assert-KmgRuntimeScenarioPreflight -Scenario 'unsupported-regression-fixture' `
        -ExpectedVersion '0.0.54' -TimeoutSeconds 120
} 'unsupported-fails-pure-preflight'
Assert-Throws {
    Assert-KmgRuntimeScenarioPreflight -Scenario 'mod-load-smoke' `
        -ExpectedVersion '30' -TimeoutSeconds 120
} 'malformed-version-fails-pure-preflight'

$orchestrator = Get-Content -LiteralPath $orchestratorPath -Raw
$preflightIndex = $orchestrator.IndexOf('Assert-KmgRuntimeScenarioPreflight')
foreach ($boundary in @("'Get-KmgRepositoryRoot", "'Build-Local.ps1'",
    "'Deploy-Local.ps1'", 'New-Item -ItemType Directory',
    'Initialize-KmgRuntimeTestEvidence', 'Start-KmgSteamKingmaker')) {
    $index = $orchestrator.IndexOf($boundary.TrimStart("'"))
    Assert-True ($preflightIndex -ge 0 -and $index -gt $preflightIndex) `
        "preflight-before-$boundary"
}
Assert-True (-not $orchestrator.Contains('Wait-KmgSteamProcess -SteamPath $SteamPath')) `
    'no-predeployment-steam-start'
Assert-True (-not $orchestrator.Contains('Kingmaker.exe')) `
    'direct-kingmaker-launch-rejected'

$common = Get-Content -LiteralPath $commonPath -Raw
Assert-True ($common.Contains('$script:KmgRuntimeScenarioMetadata = [ordered]@{')) `
    'one-authoritative-powershell-metadata-table'
Assert-True ($common.Contains("'observe-working-save-entry-action' = [pscustomobject]@{")) `
    'entry-present-in-authoritative-metadata'
Assert-True ($orchestrator.Contains('$scenarioMetadata = Get-KmgRuntimeScenarioMetadata')) `
    'orchestrator-consumes-authoritative-metadata'

$artifactRoot = Join-Path $root 'artifacts'
$backupRoot = 'C:\Dev\KingmakerGunslingerLab\runtime-backups\live-mod'
$evidenceRoot = 'C:\Dev\KingmakerGunslingerLab\runtime-evidence'
function Get-TreeFingerprint([string]$Path) {
    if (-not (Test-Path -LiteralPath $Path)) { return '<missing>' }
    return (@(Get-ChildItem -LiteralPath $Path -Recurse -Force |
        Sort-Object FullName | ForEach-Object {
            $length = if ($_.PSIsContainer) { 0 } else { $_.Length }
            '{0}|{1}|{2}' -f $_.FullName, $length, $_.LastWriteTimeUtc.Ticks
        }) -join "`n")
}
function Get-DirectoryIdentity([string]$Path) {
    if (-not (Test-Path -LiteralPath $Path)) { return '<missing>' }
    return (@(Get-ChildItem -LiteralPath $Path -Directory -Recurse -Force |
        ForEach-Object FullName | Sort-Object) -join "`n")
}
$artifactBefore = Get-TreeFingerprint $artifactRoot
$backupBefore = Get-DirectoryIdentity $backupRoot
$evidenceBefore = Get-DirectoryIdentity $evidenceRoot
$script:cimCalls = 0
$script:startProcessCalls = 0
function global:Get-CimInstance { $script:cimCalls++; throw 'Unexpected CIM call.' }
function global:Start-Process { $script:startProcessCalls++; throw 'Unexpected process launch.' }
try {
    Assert-Throws {
        & $orchestratorPath -Scenario 'unsupported-regression-fixture' `
            -ExpectedVersion '0.0.54' -WhatIf -Confirm:$false
    } 'original-defect-fixture-rejected'
}
finally {
    Remove-Item Function:\global:Get-CimInstance
    Remove-Item Function:\global:Start-Process
}
Assert-True ((Get-TreeFingerprint $artifactRoot) -ceq $artifactBefore) `
    'unsupported-does-not-build-or-stage-package'
Assert-True ((Get-DirectoryIdentity $backupRoot) -ceq $backupBefore) `
    'unsupported-creates-no-backup'
Assert-True ((Get-DirectoryIdentity $evidenceRoot) -ceq $evidenceBefore) `
    'unsupported-creates-no-deployment-or-evidence'
Assert-True ($script:cimCalls -eq 0) 'unsupported-performs-no-cim'
Assert-True ($script:startProcessCalls -eq 0) `
    'unsupported-launches-neither-steam-nor-kingmaker'

if ($failures.Count -ne 0) {
    throw "Runtime scenario preflight tests failed: $($failures -join ', ')"
}
Write-Host "Runtime scenario preflight tests passed: $checks"
