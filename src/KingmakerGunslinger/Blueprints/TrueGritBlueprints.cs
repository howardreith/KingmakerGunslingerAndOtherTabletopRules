using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Prerequisites;
using KingmakerGunslinger.Deeds;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class TrueGritBlueprintSet
    {
        internal TrueGritBlueprintSet(BlueprintFeatureSelection selection,
            BlueprintFeature[] choices, TrueGritDeed[] deeds)
        {
            Selection = selection ?? throw new ArgumentNullException("selection");
            Choices = choices ?? throw new ArgumentNullException("choices");
            Deeds = deeds ?? throw new ArgumentNullException("deeds");
            if (choices.Length != deeds.Length)
                throw new ArgumentException("True Grit choice metadata changed.");
        }

        internal BlueprintFeatureSelection Selection { get; private set; }
        internal BlueprintFeature[] Choices { get; private set; }
        internal TrueGritDeed[] Deeds { get; private set; }
        internal int Count { get { return 1 + Choices.Length; } }

        internal BlueprintFeature ChoiceFor(TrueGritDeed deed)
        {
            for (int index = 0; index < Deeds.Length; index++)
                if (Deeds[index] == deed) return Choices[index];
            throw new ArgumentOutOfRangeException("deed");
        }
    }

    internal static class TrueGritBlueprints
    {
        internal const string SelectionSymbol = "KMG.Classes.TrueGritSelection";
        internal static readonly string[] ChoiceSymbols =
        {
            "KMG.Classes.TrueGritDeadeye",
            "KMG.Classes.TrueGritGunslingersDodge",
            "KMG.Classes.TrueGritQuickClear",
            "KMG.Classes.TrueGritGunslingerInitiative",
            "KMG.Classes.TrueGritPistolWhip",
            "KMG.Classes.TrueGritStopBleeding",
            "KMG.Classes.TrueGritDeadShot",
            "KMG.Classes.TrueGritStartlingShot",
            "KMG.Classes.TrueGritTargetingArms",
            "KMG.Classes.TrueGritTargetingHead",
            "KMG.Classes.TrueGritTargetingTorso",
            "KMG.Classes.TrueGritTargetingLegs",
            "KMG.Classes.TrueGritBleedingWound",
            "KMG.Classes.TrueGritExpertLoading",
            "KMG.Classes.TrueGritLightningReload",
            "KMG.Classes.TrueGritEvasive",
            "KMG.Classes.TrueGritMenacingShot",
            "KMG.Classes.TrueGritCheatDeath",
            "KMG.Classes.TrueGritDeathsShot",
            "KMG.Classes.TrueGritStunningShot",
            "KMG.Classes.TrueGritFocusedAim",
            "KMG.Classes.TrueGritTwinShotKnockdown",
            "KMG.Classes.TrueGritSteadyAim",
            "KMG.Classes.TrueGritFastMusket"
        };

        internal static TrueGritBlueprintSet Register(BlueprintRegistry registry)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            TrueGritChoice[] catalog = TrueGritCatalog.Choices;
            if (catalog.Length != ChoiceSymbols.Length)
                throw new InvalidOperationException("True Grit catalog changed.");
            var choices = new BlueprintFeature[catalog.Length];
            var deeds = new TrueGritDeed[catalog.Length];
            for (int index = 0; index < catalog.Length; index++)
            {
                TrueGritChoice choice = catalog[index];
                deeds[index] = choice.Deed;
                choices[index] = registry.Register<BlueprintFeature>(
                    ChoiceSymbols[index], () => CreateChoice(choice));
            }
            BlueprintFeatureSelection selection = registry.Register<
                BlueprintFeatureSelection>(SelectionSymbol,
                    () => CreateSelection(choices));
            return new TrueGritBlueprintSet(selection, choices, deeds);
        }

        private static BlueprintFeature CreateChoice(TrueGritChoice choice)
        {
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = "KMG_TrueGrit_" + choice.Deed;
            feature.Ranks = 1;
            feature.IsClassFeature = true;
            feature.HideInUI = false;
            feature.ComponentsArray = Array.Empty<BlueprintComponent>();
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create("KMG.TrueGrit." + choice.Deed + ".Name",
                    "True Grit: " + choice.DisplayName),
                LocalizationService.Create("KMG.TrueGrit." + choice.Deed +
                    ".Description", "Reduce the grit cost of " +
                    choice.DisplayName + " by 1, following the True Grit rules."),
                null);
            return feature;
        }

        internal static void ConfigureOwnership(TrueGritBlueprintSet set,
            params BlueprintFeature[] ownedDeeds)
        {
            if (set == null || ownedDeeds == null ||
                ownedDeeds.Length != set.Choices.Length ||
                ownedDeeds.Any(value => value == null))
                throw new InvalidOperationException(
                    "Every True Grit choice requires one exact owned deed fact.");
            for (int index = 0; index < set.Choices.Length; index++)
            {
                BlueprintFeature choice = set.Choices[index];
                PrerequisiteFeature[] existing = choice.ComponentsArray
                    .OfType<PrerequisiteFeature>().ToArray();
                if (existing.Length == 1 &&
                    ReferenceEquals(existing[0].Feature, ownedDeeds[index]) &&
                    existing[0].Group == Prerequisite.GroupType.All)
                    continue;
                if (existing.Length != 0)
                    throw new InvalidOperationException(
                        "True Grit choice has an unexpected ownership prerequisite.");
                var prerequisite = ScriptableObject.CreateInstance<
                    PrerequisiteFeature>();
                prerequisite.name = "$KMG_TrueGrit_Owns_" + set.Deeds[index];
                prerequisite.Feature = ownedDeeds[index];
                prerequisite.Group = Prerequisite.GroupType.All;
                choice.ComponentsArray = choice.ComponentsArray
                    .Concat(new BlueprintComponent[] { prerequisite }).ToArray();
            }
        }

        private static BlueprintFeatureSelection CreateSelection(
            BlueprintFeature[] choices)
        {
            var selection = ScriptableObject.CreateInstance<BlueprintFeatureSelection>();
            selection.name = "KMG_TrueGrit_Selection";
            selection.Ranks = 1;
            selection.IsClassFeature = true;
            selection.HideInUI = false;
            selection.IgnorePrerequisites = false;
            selection.Obligatory = true;
            selection.Group = FeatureGroup.None;
            selection.Group2 = FeatureGroup.None;
            selection.Features = (BlueprintFeature[])choices.Clone();
            selection.AllFeatures = (BlueprintFeature[])choices.Clone();
            BlueprintUnitFactAccess.Resolve().Configure(selection,
                LocalizationService.Create("KMG.TrueGrit.Selection.Name",
                    "True Grit"),
                LocalizationService.Create("KMG.TrueGrit.Selection.Description",
                    "Select two different deeds. Each selected deed costs 1 fewer grit, minimum 0."),
                null);
            return selection;
        }
    }
}
