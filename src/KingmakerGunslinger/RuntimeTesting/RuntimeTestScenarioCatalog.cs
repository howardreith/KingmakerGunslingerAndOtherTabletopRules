using System;
using System.Collections.Generic;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static class RuntimeTestScenarioCatalog
    {
        internal const string ModLoadSmoke = "mod-load-smoke";
        internal const string ObserveFeatureModuleSettings =
            "observe-feature-module-settings";
        internal const string ObserveBrownFurCotwContract =
            "observe-brown-fur-cotw-contract";
        internal const string ObserveBrownFurTransmutationInventory =
            "observe-brown-fur-transmutation-inventory";
        internal const string ObserveShieldOtherInventory =
            "observe-shield-other-inventory";
        internal const string ObserveExpandedSummoningInventory =
            "observe-expanded-summoning-inventory";
        internal const string DisposableExpandedSummoning =
            "disposable-expanded-summoning";
        internal const string DisposableExpandedSummoningPlayerPath =
            "disposable-expanded-summoning-player-path";
        internal const string DisposableExpandedSummoningVisualContracts =
            "disposable-expanded-summoning-visual-contracts";
        internal const string WorkingSaveExpandedSummoningPrepare =
            "working-save-expanded-summoning-prepare";
        internal const string WorkingSaveExpandedSummoningVerifyCleanup =
            "working-save-expanded-summoning-verify-cleanup";
        internal const string WorkingSaveExpandedSummoningVerifyAbsent =
            "working-save-expanded-summoning-verify-absent";
        internal const string DisposableShieldOther = "disposable-shield-other";
        internal const string ObserveOptionalModCompatibility =
            "observe-optional-mod-compatibility";
        internal const string DisposableFirearmWwiseAudio = "disposable-firearm-wwise-audio";
        internal const string ObserveClassBlueprintContracts =
            "observe-class-blueprint-contracts";
        internal const string ObserveGunslingerPresentation =
            "observe-gunslinger-presentation";
        internal const string ObserveNativeWeaponFeatContracts =
            "observe-native-weapon-feat-contracts";
        internal const string ObserveElvenBranchedSpearContracts =
            "observe-elven-branched-spear-contracts";
        internal const string ObserveEasternWeaponContracts =
            "observe-eastern-weapon-contracts";
        internal const string DisposableElvenBranchedSpearCombat =
            "disposable-elven-branched-spear-combat";
        internal const string DisposableEasternWeaponsCombat =
            "disposable-eastern-weapons-combat";
        internal const string WorkingSaveElvenBranchedSpearPrepare =
            "working-save-elven-branched-spear-prepare";
        internal const string WorkingSaveElvenBranchedSpearVerifyCleanup =
            "working-save-elven-branched-spear-verify-cleanup";
        internal const string WorkingSaveElvenBranchedSpearVerifyAbsent =
            "working-save-elven-branched-spear-verify-absent";
        internal const string WorkingSaveEasternWeaponsPrepare =
            "working-save-eastern-weapons-prepare";
        internal const string WorkingSaveEasternWeaponsVerifyCleanup =
            "working-save-eastern-weapons-verify-cleanup";
        internal const string WorkingSaveEasternWeaponsVerifyAbsent =
            "working-save-eastern-weapons-verify-absent";
        internal const string DisposableFirearmDependentFeats =
            "disposable-firearm-dependent-feats";
        internal const string DisposableEmptyFirearmCommand =
            "disposable-empty-firearm-command";
        internal const string ObserveVendorTableContracts =
            "observe-vendor-table-contracts";
        internal const string ObserveCapitalCordVendor =
            "observe-capital-cord-vendor";
        internal const string DisposableCordOfStubbornResolve =
            "disposable-cord-of-stubborn-resolve";
        internal const string DisposableAcadamaeGraduate =
            "disposable-acadamae-graduate";
        internal const string ObserveRareFirearmAcquisition =
            "observe-rare-firearm-acquisition";
        internal const string ObserveRareFirearmBlueprintContracts =
            "observe-rare-firearm-blueprint-contracts";
        internal const string MagicFirearmNativeProperties =
            "magic-firearm-native-properties";
        internal const string ReliableFirearmMisfireMatrix =
            "reliable-firearm-misfire-matrix";
        internal const string BlunderbussThunderingScatter =
            "blunderbuss-thundering-scatter";
        internal const string ObserveProductionFirearmFallbacks =
            "observe-production-firearm-fallbacks";
        internal const string ObserveNativeFirearmRigContracts =
            "observe-native-firearm-rig-contracts";
        internal const string DisposableFirearmVisualRigs =
            "disposable-firearm-visual-rigs";
        internal const string ObserveFirearmItemLifecycleContracts =
            "observe-firearm-item-lifecycle-contracts";
        internal const string DisposableReloadAutocast =
            "disposable-reload-autocast";
        internal const string DisposablePaperCartridgeReload =
            "disposable-paper-cartridge-reload";
        internal const string DisposablePaperCartridgeModeViewLifecycle =
            "disposable-paper-cartridge-mode-view-lifecycle";
        internal const string DisposablePaperCartridgeFullAttack =
            "disposable-paper-cartridge-full-attack";
        internal const string DisposablePaperCartridgeMisfire =
            "disposable-paper-cartridge-misfire";
        internal const string DisposablePaperCartridgeScatter =
            "disposable-paper-cartridge-scatter";
        internal const string DisposablePaperCartridgeCraftingVendors =
            "disposable-paper-cartridge-crafting-vendors";
        internal const string DisposablePaperCartridgeComprehensive =
            "disposable-paper-cartridge-comprehensive";
        internal const string DisposableOverhaulMaintenance =
            "disposable-overhaul-maintenance";
        internal const string DisposableProductionFirearmSwitching =
            "disposable-production-firearm-switching";
        internal const string DisposableGunslingerComprehensiveAcceptance =
            "disposable-gunslinger-comprehensive-acceptance";
        internal const string ObserveCharacterCreationContracts =
            "observe-character-creation-contracts";
        internal const string DisposableDescriptorConstruction =
            "disposable-descriptor-construction";
        internal const string DisposableGunslingerSelection =
            "disposable-gunslinger-selection";
        internal const string DisposableGunslingerPreviewApplication =
            "disposable-gunslinger-preview-application";
        internal const string DisposableGunslingerLevelUpPreview =
            "disposable-gunslinger-levelup-preview";
        internal const string DisposableGunslingerLevelUpCommit =
            "disposable-gunslinger-levelup-commit";
        internal const string DisposableGunslingerCreationCommit =
            "disposable-gunslinger-creation-commit";
        internal const string DisposableGunslingerLevelTwentyProgression =
            "disposable-gunslinger-level-twenty-progression";
        internal const string DisposableGunslingerEvaluatedChassis =
            "disposable-gunslinger-evaluated-chassis";
        internal const string DisposableGunslingerMulticlassPreview =
            "disposable-gunslinger-multiclass-preview";
        internal const string DisposableGunslingerMulticlassCommit =
            "disposable-gunslinger-multiclass-commit";
        internal const string DisposableGunslingerRespecPreview =
            "disposable-gunslinger-respec-preview";
        internal const string DisposableGunslingerRespecCommit =
            "disposable-gunslinger-respec-commit";
        internal const string DisposableGunslingerBroadRespec =
            "disposable-gunslinger-broad-respec";
        internal const string DisposableArchetypeReconciliation =
            "disposable-archetype-reconciliation";
        internal const string DisposableGunslingerGritResource =
            "disposable-gunslinger-grit-resource";
        internal const string DisposableGunslingerGritRest =
            "disposable-gunslinger-grit-rest";
        internal const string DisposableGunslingerGritPersistence =
            "disposable-gunslinger-grit-persistence";
        internal const string DisposableGunslingerGritRecovery =
            "disposable-gunslinger-grit-recovery";
        internal const string DisposableGunslingerDeadeye =
            "disposable-gunslinger-deadeye";
        internal const string DisposableGunslingerDodge =
            "disposable-gunslinger-dodge";
        internal const string DisposableGunslingerQuickClear =
            "disposable-gunslinger-quick-clear";
        internal const string DisposableGunslingerNimble =
            "disposable-gunslinger-nimble";
        internal const string DisposableGunslingerInitiative =
            "disposable-gunslinger-initiative";
        internal const string DisposableGunslingerPistolWhip =
            "disposable-gunslinger-pistol-whip";
        internal const string DisposableGunslingerStopBleeding =
            "disposable-gunslinger-stop-bleeding";
        internal const string DisposableGunslingerBonusFeats =
            "disposable-gunslinger-bonus-feats";
        internal const string DisposableGunslingerGunTraining =
            "disposable-gunslinger-gun-training";
        internal const string DisposableGunslingerDeadShot =
            "disposable-gunslinger-dead-shot";
        internal const string DisposableGunslingerScatterShot =
            "disposable-gunslinger-scatter-shot";
        internal const string DisposableGunslingerStartlingShot =
            "disposable-gunslinger-startling-shot";
        internal const string DisposableGunslingerTargetingHead =
            "disposable-gunslinger-targeting-head";
        internal const string DisposableGunslingerTargetingTorso =
            "disposable-gunslinger-targeting-torso";
        internal const string DisposableGunslingerTargetingLegs =
            "disposable-gunslinger-targeting-legs";
        internal const string DisposableGunslingerTargetingArms =
            "disposable-gunslinger-targeting-arms";
        internal const string DisposableGunslingerBleedingWound =
            "disposable-gunslinger-bleeding-wound";
        internal const string DisposableGunslingerExpertLoading =
            "disposable-gunslinger-expert-loading";
        internal const string DisposableGunslingerLightningReload =
            "disposable-gunslinger-lightning-reload";
        internal const string DisposablePaperCartridgeLightningReload =
            "disposable-paper-cartridge-lightning-reload";
        internal const string DisposableGunslingerEvasive =
            "disposable-gunslinger-evasive";
        internal const string ObserveEvasiveNativeFeatures =
            "observe-evasive-native-features";
        internal const string ObserveMenacingShotNativeFear =
            "observe-menacing-shot-native-fear";
        internal const string DisposableGunslingerMenacingShot =
            "disposable-gunslinger-menacing-shot";
        internal const string ObserveSlingersLuckNativeRerolls =
            "observe-slingers-luck-native-rerolls";
        internal const string DisposableGunslingerSlingersLuck =
            "disposable-gunslinger-slingers-luck";
        internal const string DisposableGunslingerCheatDeath =
            "disposable-gunslinger-cheat-death";
        internal const string ObserveDeathsShotNativeDeath =
            "observe-deaths-shot-native-death";
        internal const string ObserveStunningShotNativeStunned =
            "observe-stunning-shot-native-stunned";
        internal const string DisposableGunslingerStunningShot =
            "disposable-gunslinger-stunning-shot";
        internal const string DisposableGunslingerDeathsShot =
            "disposable-gunslinger-deaths-shot";
        internal const string DisposableGunslingerTrueGrit =
            "disposable-gunslinger-true-grit";
        internal const string DisposablePistoleroDeeds =
            "disposable-pistolero-deeds";
        internal const string ObserveManualSaveLoad = "observe-manual-save-load";
        internal const string ObserveSaveCatalogAndSelection =
            "observe-save-catalog-and-selection";
        internal const string ObserveSaveCatalogProvider =
            "observe-save-catalog-provider";
        internal const string ObserveLoadGameButtonAction =
            "observe-load-game-button-action";
        internal const string WorkingSaveSmoke = "working-save-smoke";
        internal const string WorkingSaveShieldOtherPrepare =
            "working-save-shield-other-prepare";
        internal const string WorkingSaveShieldOtherVerifyCleanup =
            "working-save-shield-other-verify-cleanup";
        internal const string GenericFirearmActions =
            "generic-firearm-actions";
        internal const string ProductionFirearmCatalog =
            "production-firearm-catalog";
        internal const string AdvancedCapacity = "advanced-capacity";
        internal const string GunslingerStartingItems =
            "gunslinger-starting-items";
        internal const string MusketMasterMechanicsAndStarter =
            "musket-master-mechanics-and-starter";
        internal const string ObserveWorkingSaveEntryAction =
            "observe-working-save-entry-action";
        internal const string ObserveWorkingSaveSelectionLoadAction =
            "observe-working-save-selection-load-action";
        internal const string ObserveWorkingSaveReceiverBoundAction =
            "observe-working-save-receiver-bound-action";

        private static readonly HashSet<string> Allowed =
            new HashSet<string>(StringComparer.Ordinal)
            {
                ModLoadSmoke,
                ObserveFeatureModuleSettings,
                ObserveBrownFurCotwContract,
                ObserveBrownFurTransmutationInventory,
                ObserveShieldOtherInventory,
                ObserveExpandedSummoningInventory,
                DisposableExpandedSummoning,
                DisposableExpandedSummoningPlayerPath,
                DisposableExpandedSummoningVisualContracts,
                WorkingSaveExpandedSummoningPrepare,
                WorkingSaveExpandedSummoningVerifyCleanup,
                WorkingSaveExpandedSummoningVerifyAbsent,
                DisposableShieldOther,
                ObserveOptionalModCompatibility,
                ObserveClassBlueprintContracts,
                ObserveGunslingerPresentation,
                ObserveNativeWeaponFeatContracts,
                ObserveElvenBranchedSpearContracts,
                ObserveEasternWeaponContracts,
                DisposableElvenBranchedSpearCombat,
                DisposableEasternWeaponsCombat,
                WorkingSaveElvenBranchedSpearPrepare,
                WorkingSaveElvenBranchedSpearVerifyCleanup,
                WorkingSaveElvenBranchedSpearVerifyAbsent,
                WorkingSaveEasternWeaponsPrepare,
                WorkingSaveEasternWeaponsVerifyCleanup,
                WorkingSaveEasternWeaponsVerifyAbsent,
                DisposableFirearmDependentFeats,
                DisposableEmptyFirearmCommand,
                ObserveVendorTableContracts,
                ObserveCapitalCordVendor,
                DisposableCordOfStubbornResolve,
                DisposableAcadamaeGraduate,
                ObserveRareFirearmAcquisition,
                ObserveRareFirearmBlueprintContracts,
                MagicFirearmNativeProperties,
                ReliableFirearmMisfireMatrix,
                BlunderbussThunderingScatter,
                ObserveProductionFirearmFallbacks,
                ObserveNativeFirearmRigContracts,
                DisposableFirearmVisualRigs,
                ObserveFirearmItemLifecycleContracts,
                DisposableReloadAutocast,
                DisposablePaperCartridgeReload,
                DisposablePaperCartridgeModeViewLifecycle,
                DisposablePaperCartridgeFullAttack,
                DisposablePaperCartridgeMisfire,
                DisposablePaperCartridgeScatter,
                DisposablePaperCartridgeCraftingVendors,
                DisposablePaperCartridgeComprehensive,
                DisposableOverhaulMaintenance,
                DisposableProductionFirearmSwitching,
                DisposableGunslingerComprehensiveAcceptance,
                ObserveCharacterCreationContracts,
                DisposableDescriptorConstruction,
                DisposableGunslingerSelection,
                DisposableGunslingerPreviewApplication,
                DisposableGunslingerLevelUpPreview,
                DisposableGunslingerLevelUpCommit,
                DisposableGunslingerCreationCommit,
                DisposableFirearmWwiseAudio,
                DisposableGunslingerLevelTwentyProgression,
                DisposableGunslingerEvaluatedChassis,
                DisposableGunslingerMulticlassPreview,
                DisposableGunslingerMulticlassCommit,
                DisposableGunslingerRespecPreview,
                DisposableGunslingerRespecCommit,
                DisposableGunslingerBroadRespec,
                DisposableArchetypeReconciliation,
                DisposableGunslingerGritResource,
                DisposableGunslingerGritRest,
                DisposableGunslingerGritPersistence,
                DisposableGunslingerGritRecovery,
                DisposableGunslingerDeadeye,
                DisposableGunslingerDodge,
                DisposableGunslingerQuickClear,
                DisposableGunslingerNimble,
                DisposableGunslingerInitiative,
                DisposableGunslingerPistolWhip,
                DisposableGunslingerStopBleeding,
                DisposableGunslingerBonusFeats,
                DisposableGunslingerGunTraining,
                DisposableGunslingerDeadShot,
                DisposableGunslingerScatterShot,
                DisposableGunslingerStartlingShot,
                DisposableGunslingerTargetingHead,
                DisposableGunslingerTargetingTorso,
                DisposableGunslingerTargetingLegs,
                DisposableGunslingerTargetingArms,
                DisposableGunslingerBleedingWound,
                DisposableGunslingerExpertLoading,
                DisposableGunslingerLightningReload,
                DisposablePaperCartridgeLightningReload,
                DisposableGunslingerEvasive,
                ObserveEvasiveNativeFeatures,
                ObserveMenacingShotNativeFear,
                DisposableGunslingerMenacingShot,
                ObserveSlingersLuckNativeRerolls,
                DisposableGunslingerSlingersLuck,
                DisposableGunslingerCheatDeath,
                ObserveDeathsShotNativeDeath,
                ObserveStunningShotNativeStunned,
                DisposableGunslingerStunningShot,
                DisposableGunslingerDeathsShot,
                DisposableGunslingerTrueGrit,
                DisposablePistoleroDeeds,
                ObserveManualSaveLoad,
                ObserveSaveCatalogAndSelection,
                ObserveSaveCatalogProvider,
                ObserveLoadGameButtonAction,
                WorkingSaveSmoke,
                WorkingSaveShieldOtherPrepare,
                WorkingSaveShieldOtherVerifyCleanup,
                GenericFirearmActions,
                ProductionFirearmCatalog,
                AdvancedCapacity,
                GunslingerStartingItems,
                MusketMasterMechanicsAndStarter,
                ObserveWorkingSaveEntryAction,
                ObserveWorkingSaveSelectionLoadAction,
                ObserveWorkingSaveReceiverBoundAction
            };

        internal static bool IsAllowed(string scenario)
        {
            return scenario != null && Allowed.Contains(scenario);
        }

        internal static string[] Names()
        {
            var names = new string[Allowed.Count];
            Allowed.CopyTo(names);
            Array.Sort(names, StringComparer.Ordinal);
            return names;
        }
    }
}
