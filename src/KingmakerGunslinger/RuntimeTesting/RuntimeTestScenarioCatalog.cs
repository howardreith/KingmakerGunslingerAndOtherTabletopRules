using System;
using System.Collections.Generic;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static class RuntimeTestScenarioCatalog
    {
        internal const string ModLoadSmoke = "mod-load-smoke";
        internal const string ObserveClassBlueprintContracts =
            "observe-class-blueprint-contracts";
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
        internal const string DisposableGunslingerMulticlassPreview =
            "disposable-gunslinger-multiclass-preview";
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
                ObserveCharacterCreationContracts,
                DisposableDescriptorConstruction,
                DisposableGunslingerSelection,
                DisposableGunslingerPreviewApplication,
                DisposableGunslingerLevelUpPreview,
                DisposableGunslingerMulticlassPreview,
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
