[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$commonPath = Join-Path $PSScriptRoot 'RuntimeAutomation.Common.ps1'
$harnessPath = Join-Path $PSScriptRoot 'RuntimeHarness.Common.ps1'
$orchestratorPath = Join-Path $PSScriptRoot 'Invoke-KingmakerRuntimeTest.ps1'
$catalogPath = Join-Path $root `
    'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestScenarioCatalog.cs'
$runnerPath = Join-Path $root `
    'src\KingmakerGunslinger\RuntimeTesting\RuntimeTestRunner.cs'
. $harnessPath
. $commonPath

$sourceStateFirst = Get-KmgSourceStateFingerprint -RepositoryRoot $root
$sourceStateSecond = Get-KmgSourceStateFingerprint -RepositoryRoot $root

$runnerSource = Get-Content -Raw -LiteralPath $runnerPath
if (-not $runnerSource.Contains('State.Units.All.Add(first)') -or
    -not $runnerSource.Contains('State.Units.All.Remove(first)') -or
    -not $runnerSource.Contains('first.Descriptor.State.Immortality.Retain();')) {
    throw 'Scatter live-area fixture registration/cleanup contract is incomplete.'
}

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
Assert-True ($sourceStateFirst -cmatch '^[0-9a-f]{64}$' -and
    $sourceStateFirst -ceq $sourceStateSecond) `
    'source-state-attestation-is-stable-and-sha256'

$expected = @(
    'mod-load-smoke',
    'observe-kmg-compatibility-asset-attribution',
    'gunslinger-outfit-audit',
    'observe-elemental-race-blueprints',
    'disposable-elemental-race-mechanics',
    'disposable-elemental-race-slas',
    'disposable-hydraulic-push',
    'observe-feature-module-settings',
    'observe-urban-barbarian-rage-inventory',
    'disposable-urban-barbarian-focused',
    'working-save-urban-barbarian-prepare',
    'working-save-urban-barbarian-off-verify-cleanup',
    'observe-brown-fur-cotw-contract',
    'observe-brown-fur-cotw-absent-isolation',
    'observe-brown-fur-transmutation-inventory',
    'observe-brown-fur-cast-engine-contract',
    'disposable-brown-fur-bonus-carriers',
    'disposable-brown-fur-share-targeting',
    'disposable-brown-fur-transmutation-supremacy',
    'disposable-brown-fur-reservoir-accounting',
    'disposable-brown-fur-player-intent',
    'disposable-brown-fur-cast-execution',
    'disposable-brown-fur-arcanist-slot',
    'disposable-brown-fur-native-cast',
    'working-save-brown-fur-prepare',
    'working-save-brown-fur-verify-cleanup',
    'working-save-brown-fur-off-verify-cleanup',
    'observe-shield-other-inventory',
    'observe-expanded-summoning-inventory',
    'observe-expanded-summoning-variant-menu',
    'disposable-expanded-summoning',
    'disposable-expanded-summoning-player-path',
    'summon-same-turn-activation',
    'summon-same-turn-acadamae',
    'summon-same-turn-multiple',
    'summon-same-turn-native-control',
    'summon-same-turn-rtwp-control',
    'summon-same-turn-compatibility-quickened',
    'summon-same-turn-compatibility-acadamae',
    'disposable-expanded-summoning-visual-contracts',
    'disposable-shield-other',
    'observe-capital-cord-vendor',
    'disposable-cord-of-stubborn-resolve',
    'disposable-acadamae-graduate',
    'disposable-native-fatigue-refresh',
    'disposable-acadamae-fatigue-escalation',
    'working-save-fatigue-prepare',
    'working-save-fatigue-verify-cleanup',
    'working-save-fatigue-verify-absent',
    'disposable-focused-aim',
    'disposable-firearm-penetration',
    'disposable-firearm-wwise-audio',
    'disposable-empty-firearm-command',
    'disposable-firearm-dependent-feats',
    'disposable-overhaul-maintenance',
    'disposable-reload-autocast',
    'disposable-paper-cartridge-reload',
    'disposable-paper-cartridge-mode-view-lifecycle',
    'disposable-paper-cartridge-full-attack',
    'disposable-paper-cartridge-misfire',
    'disposable-paper-cartridge-scatter',
    'disposable-paper-cartridge-crafting-vendors',
    'disposable-paper-cartridge-lightning-reload',
    'disposable-paper-cartridge-comprehensive',
    'observe-native-weapon-feat-contracts',
    'observe-elven-branched-spear-contracts',
    'observe-eastern-weapon-contracts',
    'disposable-elven-branched-spear-combat',
    'disposable-eastern-weapons-combat',
    'icon-overhaul-visual-evidence',
    'weapon-presentation-evidence',
    'weapon-presentation-motion-evidence',
    'weapon-presentation-handgun-motion-evidence',
    'weapon-presentation-spear-motion-evidence',
    'weapon-presentation-eastern-motion-evidence',
    'weapon-presentation-transition-motion-evidence',
    'weapon-presentation-reload-evidence',
    'weapon-presentation-body-matrix-evidence',
    'gunslinger-outfit-candidate-render',
    'gunslinger-outfit-finalist-race-matrix',
    'gunslinger-outfit-production-compatibility',
    'gunslinger-outfit-production-motion',
    'gunslinger-outfit-production-persistence-prepare',
    'gunslinger-outfit-production-persistence',
    'gunslinger-outfit-production-persistence-verify-absent',
    'working-save-elven-branched-spear-prepare',
    'working-save-elven-branched-spear-verify-cleanup',
    'working-save-elven-branched-spear-verify-absent',
    'working-save-eastern-weapons-prepare',
    'working-save-eastern-weapons-verify-cleanup',
    'working-save-eastern-weapons-verify-absent',
    'observe-class-blueprint-contracts',
    'observe-gunslinger-presentation',
    'observe-vendor-table-contracts',
    'observe-rare-firearm-acquisition',
    'observe-rare-firearm-blueprint-contracts',
    'magic-firearm-native-properties',
    'reliable-firearm-misfire-matrix',
    'blunderbuss-thundering-scatter',
    'observe-production-firearm-fallbacks',
    'observe-native-firearm-rig-contracts',
    'disposable-firearm-visual-rigs',
    'observe-firearm-item-lifecycle-contracts',
    'disposable-production-firearm-switching',
    'disposable-gunslinger-comprehensive-acceptance',
    'observe-character-creation-contracts',
    'disposable-descriptor-construction',
    'disposable-gunslinger-selection',
    'disposable-gunslinger-preview-application',
    'disposable-gunslinger-levelup-preview',
    'disposable-gunslinger-levelup-commit',
    'disposable-gunslinger-creation-commit',
    'disposable-gunslinger-level-twenty-progression',
    'disposable-gunslinger-evaluated-chassis',
    'disposable-gunslinger-multiclass-preview',
    'disposable-gunslinger-multiclass-commit',
    'disposable-gunslinger-respec-preview',
    'disposable-gunslinger-respec-commit',
    'disposable-gunslinger-broad-respec',
    'disposable-archetype-reconciliation',
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
    'disposable-gunslinger-scatter-shot',
    'disposable-gunslinger-startling-shot',
    'disposable-gunslinger-targeting-head',
    'disposable-gunslinger-targeting-torso',
    'disposable-gunslinger-targeting-legs',
    'disposable-gunslinger-targeting-arms',
    'disposable-gunslinger-deaths-shot',
    'disposable-gunslinger-bleeding-wound',
    'observe-bodyguard-native-contracts',
    'observe-aid-another-compatibility-contracts',
    'disposable-bodyguard-feats',
    'disposable-helpful-bodyguard',
    'disposable-bodyguard-feats-disabled',
    'disposable-in-harms-way-off-turn-economy',
    'disposable-gunslinger-expert-loading',
    'disposable-gunslinger-lightning-reload',
    'disposable-gunslinger-evasive',
    'observe-evasive-native-features',
    'observe-menacing-shot-native-fear',
    'disposable-gunslinger-menacing-shot',
    'observe-slingers-luck-native-rerolls',
    'disposable-gunslinger-slingers-luck',
    'disposable-gunslinger-cheat-death',
    'observe-deaths-shot-native-death',
    'observe-stunning-shot-native-stunned',
    'disposable-gunslinger-stunning-shot',
    'disposable-gunslinger-true-grit',
    'disposable-pistolero-deeds',
    'musket-master-mechanics-and-starter',
    'observe-optional-mod-compatibility',
    'observe-craft-magic-items-compatibility',
    'observe-craft-magic-items-ammunition-ui',
    'working-save-craft-magic-items-prepare',
    'working-save-craft-magic-items-verify-cleanup',
    'observe-manual-save-load',
    'observe-save-catalog-and-selection',
    'observe-save-catalog-provider',
    'observe-load-game-button-action',
    'working-save-smoke',
    'p0-affected-focused-aim-save-load',
    'disposable-in-harms-way-human-repro',
    'working-save-shield-other-prepare',
    'working-save-shield-other-verify-cleanup',
    'working-save-expanded-summoning-prepare',
    'working-save-expanded-summoning-verify-cleanup',
    'working-save-expanded-summoning-verify-absent',
    'generic-firearm-actions',
    'production-firearm-catalog',
    'advanced-capacity',
    'gunslinger-starting-items',
    'observe-working-save-entry-action',
    'observe-working-save-selection-load-action',
    'observe-working-save-receiver-bound-action'
)
$catalog = Get-Content -LiteralPath $catalogPath -Raw
$csharpNames = @([regex]::Matches($catalog, '"([a-z][a-z0-9-]+)"') |
    ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique)
$powershellNames = @($script:KmgRuntimeScenarios | Sort-Object)
Assert-True (($csharpNames -join "`n") -ceq ($powershellNames -join "`n")) `
    'csharp-powershell-catalog-sync'
Assert-True (($expected | Sort-Object) -join "`n" -ceq
    ($powershellNames -join "`n")) 'documented-scenarios-retained'
$assetAttribution = Get-KmgRuntimeScenarioMetadata `
    'observe-kmg-compatibility-asset-attribution'
Assert-True (-not $assetAttribution.RequiresManualInteraction -and
    -not $assetAttribution.RequiresSaveName) `
    'compatibility-asset-attribution-is-autonomous-save-free'
$cmiUi = Get-KmgRuntimeScenarioMetadata `
    'observe-craft-magic-items-ammunition-ui'
Assert-True (-not $cmiUi.RequiresManualInteraction -and
    -not $cmiUi.RequiresSaveName) `
    'craft-magic-items-ammunition-ui-is-autonomous-save-free'
$cmiPersistence = Get-KmgRuntimeScenarioMetadata `
    'working-save-craft-magic-items-prepare'
Assert-True (-not $cmiPersistence.RequiresManualInteraction -and
    $cmiPersistence.RequiresSaveName -and
    $cmiPersistence.PermittedSaveName -ceq 'KMG_AUTOMATION_WORKING') `
    'craft-magic-items-persistence-is-guarded-working-save-only'
$assetRequest = New-KmgRuntimeRequest `
    -Scenario 'observe-kmg-compatibility-asset-attribution' `
    -ExpectedVersion '0.0.113' -TimeoutSeconds 120 -ExitAfterCompletion $true `
    -EvidenceDirectory (Join-Path $script:KmgRuntimeEvidenceRoot `
        'kmg-attribution-request-test') `
    -Parameters @{ assetConfiguration = 'firearms-only' }
Assert-True ($assetRequest.parameters.assetConfiguration -ceq 'firearms-only') `
    'compatibility-asset-attribution-parameter-round-trips'
Assert-Throws {
    New-KmgRuntimeRequest `
        -Scenario 'observe-kmg-compatibility-asset-attribution' `
        -ExpectedVersion '0.0.113' -TimeoutSeconds 120 `
        -ExitAfterCompletion $true `
        -EvidenceDirectory (Join-Path $script:KmgRuntimeEvidenceRoot `
            'kmg-attribution-request-test') `
        -Parameters @{ assetConfiguration = 'unsupported' }
} 'compatibility-asset-attribution-configuration-fails-closed'
$presentation = Get-KmgRuntimeScenarioMetadata 'observe-gunslinger-presentation'
Assert-True (-not $presentation.RequiresManualInteraction) `
    'presentation-is-autonomous'
Assert-True (-not $presentation.RequiresSaveName) `
    'presentation-is-save-free'
$spearContracts = Get-KmgRuntimeScenarioMetadata `
    'observe-elven-branched-spear-contracts'
Assert-True (-not $spearContracts.RequiresManualInteraction) `
    'spear-contracts-is-autonomous'
Assert-True (-not $spearContracts.RequiresSaveName) `
    'spear-contracts-is-save-free'
$easternContracts = Get-KmgRuntimeScenarioMetadata `
    'observe-eastern-weapon-contracts'
Assert-True (-not $easternContracts.RequiresManualInteraction) `
    'eastern-contracts-is-autonomous'
Assert-True (-not $easternContracts.RequiresSaveName) `
    'eastern-contracts-is-save-free'
$easternCombat = Get-KmgRuntimeScenarioMetadata `
    'disposable-eastern-weapons-combat'
Assert-True (-not $easternCombat.RequiresManualInteraction) `
    'eastern-combat-is-autonomous'
Assert-True (-not $easternCombat.RequiresSaveName) `
    'eastern-combat-is-save-free'
$weaponPresentation = Get-KmgRuntimeScenarioMetadata `
    'weapon-presentation-evidence'
Assert-True (-not $weaponPresentation.RequiresManualInteraction) `
    'weapon-presentation-evidence-is-autonomous'
Assert-True $weaponPresentation.RequiresSaveName `
    'weapon-presentation-evidence-requires-save-name'
Assert-True ($weaponPresentation.PermittedSaveName -eq `
    'KMG_AUTOMATION_WORKING') `
    'weapon-presentation-evidence-only-permits-working-save'
$weaponPresentationMotion = Get-KmgRuntimeScenarioMetadata `
    'weapon-presentation-motion-evidence'
Assert-True (-not $weaponPresentationMotion.RequiresManualInteraction) `
    'weapon-presentation-motion-evidence-is-autonomous'
Assert-True $weaponPresentationMotion.RequiresSaveName `
    'weapon-presentation-motion-evidence-requires-save-name'
Assert-True ($weaponPresentationMotion.PermittedSaveName -eq `
    'KMG_AUTOMATION_WORKING') `
    'weapon-presentation-motion-evidence-only-permits-working-save'
$weaponPresentationHandgunMotion = Get-KmgRuntimeScenarioMetadata `
    'weapon-presentation-handgun-motion-evidence'
Assert-True (-not $weaponPresentationHandgunMotion.RequiresManualInteraction) `
    'weapon-presentation-handgun-motion-evidence-is-autonomous'
Assert-True $weaponPresentationHandgunMotion.RequiresSaveName `
    'weapon-presentation-handgun-motion-evidence-requires-save-name'
Assert-True ($weaponPresentationHandgunMotion.PermittedSaveName -eq `
    'KMG_AUTOMATION_WORKING') `
    'weapon-presentation-handgun-motion-evidence-only-permits-working-save'
$weaponPresentationSpearMotion = Get-KmgRuntimeScenarioMetadata `
    'weapon-presentation-spear-motion-evidence'
Assert-True (-not $weaponPresentationSpearMotion.RequiresManualInteraction) `
    'weapon-presentation-spear-motion-evidence-is-autonomous'
Assert-True $weaponPresentationSpearMotion.RequiresSaveName `
    'weapon-presentation-spear-motion-evidence-requires-save-name'
Assert-True ($weaponPresentationSpearMotion.PermittedSaveName -eq `
    'KMG_AUTOMATION_WORKING') `
    'weapon-presentation-spear-motion-evidence-only-permits-working-save'
$weaponPresentationEasternMotion = Get-KmgRuntimeScenarioMetadata `
    'weapon-presentation-eastern-motion-evidence'
Assert-True (-not $weaponPresentationEasternMotion.RequiresManualInteraction) `
    'weapon-presentation-eastern-motion-evidence-is-autonomous'
Assert-True $weaponPresentationEasternMotion.RequiresSaveName `
    'weapon-presentation-eastern-motion-evidence-requires-save-name'
Assert-True ($weaponPresentationEasternMotion.PermittedSaveName -eq `
    'KMG_AUTOMATION_WORKING') `
    'weapon-presentation-eastern-motion-evidence-only-permits-working-save'
$weaponPresentationTransitionMotion = Get-KmgRuntimeScenarioMetadata `
    'weapon-presentation-transition-motion-evidence'
Assert-True (-not $weaponPresentationTransitionMotion.RequiresManualInteraction) `
    'weapon-presentation-transition-motion-evidence-is-autonomous'
Assert-True $weaponPresentationTransitionMotion.RequiresSaveName `
    'weapon-presentation-transition-motion-evidence-requires-save-name'
Assert-True ($weaponPresentationTransitionMotion.PermittedSaveName -eq `
    'KMG_AUTOMATION_WORKING') `
    'weapon-presentation-transition-motion-evidence-only-permits-working-save'
$weaponPresentationReload = Get-KmgRuntimeScenarioMetadata `
    'weapon-presentation-reload-evidence'
Assert-True (-not $weaponPresentationReload.RequiresManualInteraction) `
    'weapon-presentation-reload-evidence-is-autonomous'
Assert-True $weaponPresentationReload.RequiresSaveName `
    'weapon-presentation-reload-evidence-requires-save-name'
Assert-True ($weaponPresentationReload.PermittedSaveName -eq `
    'KMG_AUTOMATION_WORKING') `
    'weapon-presentation-reload-evidence-only-permits-working-save'
$weaponPresentationBodyMatrix = Get-KmgRuntimeScenarioMetadata `
    'weapon-presentation-body-matrix-evidence'
Assert-True (-not $weaponPresentationBodyMatrix.RequiresManualInteraction) `
    'weapon-presentation-body-matrix-evidence-is-autonomous'
Assert-True $weaponPresentationBodyMatrix.RequiresSaveName `
    'weapon-presentation-body-matrix-evidence-requires-save-name'
Assert-True ($weaponPresentationBodyMatrix.PermittedSaveName -eq `
    'KMG_AUTOMATION_WORKING') `
    'weapon-presentation-body-matrix-evidence-only-permits-working-save'
$gunslingerOutfitCandidateRender = Get-KmgRuntimeScenarioMetadata `
    'gunslinger-outfit-candidate-render'
Assert-True (-not $gunslingerOutfitCandidateRender.RequiresManualInteraction) `
    'gunslinger-outfit-candidate-render-is-autonomous'
Assert-True $gunslingerOutfitCandidateRender.RequiresSaveName `
    'gunslinger-outfit-candidate-render-requires-save-name'
Assert-True ($gunslingerOutfitCandidateRender.PermittedSaveName -eq `
    'KMG_AUTOMATION_WORKING') `
    'gunslinger-outfit-candidate-render-only-permits-working-save'
$gunslingerOutfitFinalistRaceMatrix = Get-KmgRuntimeScenarioMetadata `
    'gunslinger-outfit-finalist-race-matrix'
Assert-True (-not $gunslingerOutfitFinalistRaceMatrix.RequiresManualInteraction) `
    'gunslinger-outfit-finalist-race-matrix-is-autonomous'
Assert-True $gunslingerOutfitFinalistRaceMatrix.RequiresSaveName `
    'gunslinger-outfit-finalist-race-matrix-requires-save-name'
Assert-True ($gunslingerOutfitFinalistRaceMatrix.PermittedSaveName -eq `
    'KMG_AUTOMATION_WORKING') `
    'gunslinger-outfit-finalist-race-matrix-only-permits-working-save'
$gunslingerOutfitProductionCompatibility = Get-KmgRuntimeScenarioMetadata `
    'gunslinger-outfit-production-compatibility'
Assert-True (-not $gunslingerOutfitProductionCompatibility.RequiresManualInteraction) `
    'gunslinger-outfit-production-compatibility-is-autonomous'
Assert-True $gunslingerOutfitProductionCompatibility.RequiresSaveName `
    'gunslinger-outfit-production-compatibility-requires-save-name'
Assert-True ($gunslingerOutfitProductionCompatibility.PermittedSaveName -eq `
    'KMG_AUTOMATION_WORKING') `
    'gunslinger-outfit-production-compatibility-only-permits-working-save'
$gunslingerOutfitProductionMotion = Get-KmgRuntimeScenarioMetadata `
    'gunslinger-outfit-production-motion'
Assert-True (-not $gunslingerOutfitProductionMotion.RequiresManualInteraction) `
    'gunslinger-outfit-production-motion-is-autonomous'
Assert-True $gunslingerOutfitProductionMotion.RequiresSaveName `
    'gunslinger-outfit-production-motion-requires-save-name'
Assert-True ($gunslingerOutfitProductionMotion.PermittedSaveName -eq `
    'KMG_AUTOMATION_WORKING') `
    'gunslinger-outfit-production-motion-only-permits-working-save'
foreach ($outfitPersistenceScenario in @(
    'gunslinger-outfit-production-persistence-prepare',
    'gunslinger-outfit-production-persistence',
    'gunslinger-outfit-production-persistence-verify-absent')) {
    $gunslingerOutfitProductionPersistence = Get-KmgRuntimeScenarioMetadata `
        $outfitPersistenceScenario
    Assert-True (-not $gunslingerOutfitProductionPersistence.RequiresManualInteraction) `
        ($outfitPersistenceScenario + '-is-autonomous')
    Assert-True $gunslingerOutfitProductionPersistence.RequiresSaveName `
        ($outfitPersistenceScenario + '-requires-save-name')
    Assert-True ($gunslingerOutfitProductionPersistence.PermittedSaveName -eq `
        'KMG_AUTOMATION_WORKING') `
        ($outfitPersistenceScenario + '-only-permits-working-save')
}
$vendorContracts = Get-KmgRuntimeScenarioMetadata 'observe-vendor-table-contracts'
Assert-True (-not $vendorContracts.RequiresManualInteraction) `
    'vendor-contracts-is-autonomous'
Assert-True (-not $vendorContracts.RequiresSaveName) `
    'vendor-contracts-is-save-free'
$fallbacks = Get-KmgRuntimeScenarioMetadata 'observe-production-firearm-fallbacks'
Assert-True (-not $fallbacks.RequiresManualInteraction) `
    'fallbacks-is-autonomous'
Assert-True (-not $fallbacks.RequiresSaveName) 'fallbacks-is-save-free'
$lifecycle = Get-KmgRuntimeScenarioMetadata 'observe-firearm-item-lifecycle-contracts'
Assert-True (-not $lifecycle.RequiresManualInteraction) `
    'lifecycle-is-autonomous'
Assert-True (-not $lifecycle.RequiresSaveName) 'lifecycle-is-save-free'

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
$expandedSummoning = Get-KmgRuntimeScenarioMetadata `
    'disposable-expanded-summoning'
Assert-True (-not $expandedSummoning.RequiresManualInteraction) `
    'expanded-summoning-is-autonomous'
Assert-True $expandedSummoning.RequiresSaveName `
    'expanded-summoning-requires-save-name'
Assert-True ($expandedSummoning.PermittedSaveName -ceq `
    'KMG_AUTOMATION_WORKING') `
    'expanded-summoning-only-permits-working-save'
$expandedSummoningMenu = Get-KmgRuntimeScenarioMetadata `
    'observe-expanded-summoning-variant-menu'
Assert-True ($expandedSummoningMenu.RequiresManualInteraction -and
    $expandedSummoningMenu.RequiresSaveName -and
    $expandedSummoningMenu.PermittedSaveName -ceq `
        'KMG_AUTOMATION_WORKING') `
    'expanded-summoning-menu-is-supervised-working-save-only'
$fatigueEscalation = Get-KmgRuntimeScenarioMetadata `
    'disposable-native-fatigue-refresh'
Assert-True (-not $fatigueEscalation.RequiresManualInteraction -and
    -not $fatigueEscalation.RequiresSaveName) `
    'native-fatigue-refresh-is-autonomous-save-free'
$acadamaeFatigueEscalation = Get-KmgRuntimeScenarioMetadata 'disposable-acadamae-fatigue-escalation'
Assert-True ((-not $acadamaeFatigueEscalation.RequiresManualInteraction) -and (-not $acadamaeFatigueEscalation.RequiresSaveName)) 'acadamae-fatigue-escalation-is-autonomous-save-free'
$fatiguePersistence = Get-KmgRuntimeScenarioMetadata `
    'working-save-fatigue-prepare'
Assert-True (-not $fatiguePersistence.RequiresManualInteraction -and
    $fatiguePersistence.RequiresSaveName -and
    $fatiguePersistence.PermittedSaveName -ceq `
        'KMG_AUTOMATION_WORKING') `
    'fatigue-persistence-is-guarded-working-save-only'
$expandedSummoningPlayerPath = Get-KmgRuntimeScenarioMetadata `
    'disposable-expanded-summoning-player-path'
Assert-True (-not $expandedSummoningPlayerPath.RequiresManualInteraction) `
    'expanded-summoning-player-path-is-autonomous'
Assert-True $expandedSummoningPlayerPath.RequiresSaveName `
    'expanded-summoning-player-path-requires-save-name'
Assert-True ($expandedSummoningPlayerPath.PermittedSaveName -ceq `
    'KMG_AUTOMATION_WORKING') `
    'expanded-summoning-player-path-only-permits-working-save'
$summonSameTurnActivation = Get-KmgRuntimeScenarioMetadata `
    'summon-same-turn-activation'
Assert-True (-not $summonSameTurnActivation.RequiresManualInteraction) `
    'summon-same-turn-activation-is-autonomous'
Assert-True $summonSameTurnActivation.RequiresSaveName `
    'summon-same-turn-activation-requires-save-name'
Assert-True ($summonSameTurnActivation.PermittedSaveName -ceq `
    'KMG_AUTOMATION_WORKING') `
    'summon-same-turn-activation-only-permits-working-save'
foreach ($summonMatrixScenario in @(
    'summon-same-turn-acadamae',
    'summon-same-turn-multiple',
    'summon-same-turn-native-control',
    'summon-same-turn-rtwp-control')) {
    $summonMatrixMetadata = Get-KmgRuntimeScenarioMetadata `
        $summonMatrixScenario
    Assert-True (-not $summonMatrixMetadata.RequiresManualInteraction -and
        $summonMatrixMetadata.RequiresSaveName -and
        $summonMatrixMetadata.PermittedSaveName -ceq `
            'KMG_AUTOMATION_WORKING') `
        ($summonMatrixScenario + '-is-autonomous-working-save-only')
}
foreach ($summonCompatibilityScenario in @(
    'summon-same-turn-compatibility-quickened',
    'summon-same-turn-compatibility-acadamae')) {
    $summonCompatibilityMetadata = Get-KmgRuntimeScenarioMetadata `
        $summonCompatibilityScenario
    Assert-True (-not $summonCompatibilityMetadata.RequiresManualInteraction -and
        -not $summonCompatibilityMetadata.RequiresSaveName -and
        $summonCompatibilityMetadata.ReadinessBehavior -ceq 'mod-load' -and
        -not $summonCompatibilityMetadata.UsesCatalogTimeout -and
        -not $summonCompatibilityMetadata.UsesSelectionTimeouts -and
        -not $summonCompatibilityMetadata.UsesWorkingStageTimeouts) `
        ($summonCompatibilityScenario + '-is-autonomous-request-local')
}
$brownFurNativeCast = Get-KmgRuntimeScenarioMetadata `
    'disposable-brown-fur-native-cast'
Assert-True (-not $brownFurNativeCast.RequiresManualInteraction) `
    'brown-fur-native-cast-is-autonomous'
Assert-True $brownFurNativeCast.RequiresSaveName `
    'brown-fur-native-cast-requires-save-name'
Assert-True ($brownFurNativeCast.PermittedSaveName -ceq `
    'KMG_AUTOMATION_WORKING') `
    'brown-fur-native-cast-only-permits-working-save'
$brownFurPersistence = Get-KmgRuntimeScenarioMetadata `
    'working-save-brown-fur-prepare'
Assert-True (-not $brownFurPersistence.RequiresManualInteraction) `
    'brown-fur-persistence-is-autonomous'
Assert-True $brownFurPersistence.RequiresSaveName `
    'brown-fur-persistence-requires-save-name'
Assert-True ($brownFurPersistence.PermittedSaveName -ceq `
    'KMG_AUTOMATION_WORKING') `
    'brown-fur-persistence-only-permits-working-save'
$expandedSummoningVisual = Get-KmgRuntimeScenarioMetadata `
    'disposable-expanded-summoning-visual-contracts'
Assert-True (-not $expandedSummoningVisual.RequiresManualInteraction) `
    'expanded-summoning-visual-is-autonomous'
Assert-True $expandedSummoningVisual.RequiresSaveName `
    'expanded-summoning-visual-requires-save-name'
Assert-True ($expandedSummoningVisual.PermittedSaveName -ceq `
    'KMG_AUTOMATION_WORKING') `
    'expanded-summoning-visual-only-permits-working-save'
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
$targetingArms = Get-KmgRuntimeScenarioMetadata `
    'disposable-gunslinger-targeting-arms'
Assert-True (-not $targetingArms.RequiresManualInteraction) `
    'targeting-arms-is-autonomous'
Assert-True (-not $targetingArms.RequiresSaveName) `
    'targeting-arms-is-save-free'
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
$slingersLuckObserver = Get-KmgRuntimeScenarioMetadata `
    'observe-slingers-luck-native-rerolls'
Assert-True (-not $slingersLuckObserver.RequiresManualInteraction) `
    'slingers-luck-observer-is-autonomous'
Assert-True (-not $slingersLuckObserver.RequiresSaveName) `
    'slingers-luck-observer-is-save-free'
$slingersLuck = Get-KmgRuntimeScenarioMetadata `
    'disposable-gunslinger-slingers-luck'
Assert-True (-not $slingersLuck.RequiresManualInteraction) `
    'slingers-luck-is-autonomous'
Assert-True (-not $slingersLuck.RequiresSaveName) `
    'slingers-luck-is-save-free'
$cheatDeath = Get-KmgRuntimeScenarioMetadata `
    'disposable-gunslinger-cheat-death'
Assert-True (-not $cheatDeath.RequiresManualInteraction) `
    'cheat-death-is-autonomous'
Assert-True (-not $cheatDeath.RequiresSaveName) `
    'cheat-death-is-save-free'
$deathsShotObserver = Get-KmgRuntimeScenarioMetadata `
    'observe-deaths-shot-native-death'
Assert-True (-not $deathsShotObserver.RequiresManualInteraction) `
    'deaths-shot-observer-is-autonomous'
Assert-True (-not $deathsShotObserver.RequiresSaveName) `
    'deaths-shot-observer-is-save-free'
$stunningShotObserver = Get-KmgRuntimeScenarioMetadata `
    'observe-stunning-shot-native-stunned'
Assert-True (-not $stunningShotObserver.RequiresManualInteraction) `
    'stunning-shot-observer-is-autonomous'
Assert-True (-not $stunningShotObserver.RequiresSaveName) `
    'stunning-shot-observer-is-save-free'
$stunningShot = Get-KmgRuntimeScenarioMetadata `
    'disposable-gunslinger-stunning-shot'
Assert-True (-not $stunningShot.RequiresManualInteraction) `
    'stunning-shot-is-autonomous'
Assert-True (-not $stunningShot.RequiresSaveName) `
    'stunning-shot-is-save-free'
$trueGrit = Get-KmgRuntimeScenarioMetadata `
    'disposable-gunslinger-true-grit'
Assert-True (-not $trueGrit.RequiresManualInteraction) `
    'true-grit-is-autonomous'
Assert-True (-not $trueGrit.RequiresSaveName) `
    'true-grit-is-save-free'
$bodyguardContracts = Get-KmgRuntimeScenarioMetadata `
    'observe-bodyguard-native-contracts'
Assert-True (-not $bodyguardContracts.RequiresManualInteraction -and
    -not $bodyguardContracts.RequiresSaveName) `
    'bodyguard-contracts-is-autonomous-save-free'
$bodyguard = Get-KmgRuntimeScenarioMetadata 'disposable-bodyguard-feats'
Assert-True (-not $bodyguard.RequiresManualInteraction -and
    -not $bodyguard.RequiresSaveName) `
    'bodyguard-combat-is-autonomous-save-free'
$bodyguardDisabled = Get-KmgRuntimeScenarioMetadata `
    'disposable-bodyguard-feats-disabled'
Assert-True (-not $bodyguardDisabled.RequiresManualInteraction -and
    -not $bodyguardDisabled.RequiresSaveName) `
    'bodyguard-disabled-is-autonomous-save-free'
$humanRepro = Get-KmgRuntimeScenarioMetadata `
    'disposable-in-harms-way-human-repro'
Assert-True (-not $humanRepro.RequiresManualInteraction -and
    $humanRepro.RequiresSaveName -and
    $humanRepro.PermittedSaveName -ceq 'KMG_IHW_HUMAN_REPRO_COPY') `
    'in-harms-way-human-repro-is-autonomous-exact-save'

$valid = @{
    Scenario = 'observe-working-save-entry-action'
    ExpectedVersion = '0.0.113'
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
        -ExpectedVersion '0.0.113' -TimeoutSeconds 120
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
Assert-True ($orchestrator.Contains("'disposable-brown-fur-native-cast',")) `
    'brown-fur-native-cast-uses-working-save-result-deadline'
Assert-True ($orchestrator.Contains("'working-save-brown-fur-prepare',") -and
    $orchestrator.Contains("'working-save-brown-fur-verify-cleanup',")) `
    'brown-fur-persistence-uses-working-save-result-deadline'
Assert-True ($orchestrator.Contains("'working-save-fatigue-prepare',") -and
    $orchestrator.Contains("'working-save-fatigue-verify-cleanup',") -and
    $orchestrator.Contains("'working-save-fatigue-verify-absent',")) `
    'fatigue-persistence-uses-working-save-result-deadline'

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
            -ExpectedVersion '0.0.113' -WhatIf -Confirm:$false
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
