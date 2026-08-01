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
