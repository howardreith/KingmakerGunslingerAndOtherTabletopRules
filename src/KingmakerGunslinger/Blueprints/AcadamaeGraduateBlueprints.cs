using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static class AcadamaeGraduateBlueprints
    {
        internal const string Symbol = "KMG.Feats.AcadamaeGraduate";
        private const string ConjurationSpecializationGuid =
            "567801abe990faf4080df566fadcd038";

        internal static BlueprintFeature Register(LibraryScriptableObject library,
            BlueprintRegistry registry)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (registry == null) throw new ArgumentNullException("registry");
            BlueprintFeature iconDonor = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(
                library, ConjurationSpecializationGuid,
                "native Conjuration specialization icon donor");
            return registry.Register<BlueprintFeature>(Symbol, () => Create(iconDonor));
        }

        private static BlueprintFeature Create(BlueprintFeature iconDonor)
        {
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = "KMG_AcadamaeGraduate_Feature";
            feature.Ranks = 1;
            feature.HideInUI = false;
            feature.IsClassFeature = false;
            feature.Groups = new[] { FeatureGroup.Feat };
            feature.ComponentsArray = Array.Empty<BlueprintComponent>();
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create("KMG.Feat.AcadamaeGraduate.Name",
                    "Acadamae Graduate"),
                LocalizationService.Create("KMG.Feat.AcadamaeGraduate.Description",
                    "Prerequisite: specialist wizard 1st level; Conjuration cannot be a forbidden school. When you cast a prepared arcane Conjuration (Summoning) spell that takes longer than a standard action, reduce its casting time by one round, to a minimum of one standard action. After the accelerated spell is successfully cast, attempt a Fortitude save (DC 15 + spell level); failure causes fatigue."),
                iconDonor.Icon);
            return feature;
        }
    }
}
