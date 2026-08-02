using System;
using System.Collections.Generic;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static class RuntimeTestScenarioCatalog
    {
        internal const string ModLoadSmoke = "mod-load-smoke";
        internal const string ObserveClassBlueprintContracts =
            "observe-class-blueprint-contracts";
        internal const string ObserveGunslingerPresentation =
            "observe-gunslinger-presentation";
        internal const string ObserveVendorTableContracts =
            "observe-vendor-table-contracts";
        internal const string ObserveProductionFirearmFallbacks =
            "observe-production-firearm-fallbacks";
        internal const string ObserveFirearmItemLifecycleContracts =
            "observe-firearm-item-lifecycle-contracts";
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
        internal const string DisposableGunslingerLevelTwentyProgression =
            "disposable-gunslinger-level-twenty-progression";
        internal const string DisposableGunslingerMulticlassPreview =
            "disposable-gunslinger-multiclass-preview";
        internal const string DisposableGunslingerMulticlassCommit =
            "disposable-gunslinger-multiclass-commit";
        internal const string DisposableGunslingerRespecPreview =
            "disposable-gunslinger-respec-preview";
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
        internal const string DisposableGunslingerStartlingShot =
            "disposable-gunslinger-startling-shot";
        internal const string DisposableGunslingerTargetingHead =
            "disposable-gunslinger-targeting-head";
        internal const string DisposableGunslingerTargetingTorso =
            "disposable-gunslinger-targeting-torso";
        internal const string DisposableGunslingerTargetingLegs =
            "disposable-gunslinger-targeting-legs";
        internal const string DisposableGunslingerBleedingWound =
            "disposable-gunslinger-bleeding-wound";
        internal const string DisposableGunslingerExpertLoading =
            "disposable-gunslinger-expert-loading";
        internal const string DisposableGunslingerLightningReload =
            "disposable-gunslinger-lightning-reload";
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
        internal const string DisposableGunslingerTrueGrit =
            "disposable-gunslinger-true-grit";
        internal const string ObserveManualSaveLoad = "observe-manual-save-load";
        internal const string ObserveSaveCatalogAndSelection =
            "observe-save-catalog-and-selection";
        internal const string ObserveSaveCatalogProvider =
            "observe-save-catalog-provider";
        internal const string ObserveLoadGameButtonAction =
            "observe-load-game-button-action";
        internal const string WorkingSaveSmoke = "working-save-smoke";
        internal const string GenericFirearmActions =
            "generic-firearm-actions";
        internal const string ProductionFirearmCatalog =
            "production-firearm-catalog";
        internal const string AdvancedCapacity = "advanced-capacity";
        internal const string GunslingerStartingItems =
            "gunslinger-starting-items";
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
                ObserveClassBlueprintContracts,
                ObserveGunslingerPresentation,
                ObserveVendorTableContracts,
                ObserveProductionFirearmFallbacks,
                ObserveFirearmItemLifecycleContracts,
                ObserveCharacterCreationContracts,
                DisposableDescriptorConstruction,
                DisposableGunslingerSelection,
                DisposableGunslingerPreviewApplication,
                DisposableGunslingerLevelUpPreview,
                DisposableGunslingerLevelUpCommit,
                DisposableGunslingerLevelTwentyProgression,
                DisposableGunslingerMulticlassPreview,
                DisposableGunslingerMulticlassCommit,
                DisposableGunslingerRespecPreview,
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
                DisposableGunslingerStartlingShot,
                DisposableGunslingerTargetingHead,
                DisposableGunslingerTargetingTorso,
                DisposableGunslingerTargetingLegs,
                DisposableGunslingerBleedingWound,
                DisposableGunslingerExpertLoading,
                DisposableGunslingerLightningReload,
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
                DisposableGunslingerTrueGrit,
                ObserveManualSaveLoad,
                ObserveSaveCatalogAndSelection,
                ObserveSaveCatalogProvider,
                ObserveLoadGameButtonAction,
                WorkingSaveSmoke,
                GenericFirearmActions,
                ProductionFirearmCatalog,
                AdvancedCapacity,
                GunslingerStartingItems,
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
