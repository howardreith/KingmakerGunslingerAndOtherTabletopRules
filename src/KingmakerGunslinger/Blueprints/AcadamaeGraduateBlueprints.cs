using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using KingmakerGunslinger.Acadamae;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static class AcadamaeGraduateBlueprints
    {
        internal const string Symbol = "KMG.Feats.AcadamaeGraduate";
        private const string ConjurationSpecializationGuid =
            "567801abe990faf4080df566fadcd038";
        private const string WizardClassGuid = "ba34257984f4c41408ce1dc2004e342e";
        private const string SchoolSelectionGuid = "5f838049069f1ac4d804ce0862ab5110";
        private const string OppositionSelectionGuid = "6c29030e9fea36949877c43a6f94ff31";
        private const string UniversalistGuid = "0933849149cfc9244ac05d6a5b57fd80";

        internal static BlueprintFeature Register(LibraryScriptableObject library,
            BlueprintRegistry registry)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (registry == null) throw new ArgumentNullException("registry");
            BlueprintProgression iconDonor = BlueprintLibraryLookup.RequireExact<BlueprintProgression>(
                library, ConjurationSpecializationGuid,
                "native Conjuration specialization icon donor");
            BlueprintCharacterClass wizard = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(
                library, WizardClassGuid, "native Wizard class");
            BlueprintFeatureSelection schools = BlueprintLibraryLookup.RequireExact<BlueprintFeatureSelection>(
                library, SchoolSelectionGuid, "native Wizard school selection");
            BlueprintFeatureSelection opposition = BlueprintLibraryLookup.RequireExact<BlueprintFeatureSelection>(
                library, OppositionSelectionGuid, "native opposition-school selection");
            BlueprintFeature universalist = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(
                library, UniversalistGuid, "native Universalist feature");
            return registry.Register<BlueprintFeature>(Symbol,
                () => Create(iconDonor, wizard, schools, opposition, universalist));
        }

        private static BlueprintFeature Create(BlueprintProgression iconDonor,
            BlueprintCharacterClass wizard, BlueprintFeatureSelection schools,
            BlueprintFeatureSelection opposition, BlueprintFeature universalist)
        {
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = "KMG_AcadamaeGraduate_Feature";
            feature.Ranks = 1;
            feature.HideInUI = false;
            feature.IsClassFeature = false;
            feature.Groups = new[] { FeatureGroup.Feat };
            var prerequisite = ScriptableObject.CreateInstance<PrerequisiteAcadamaeGraduate>();
            prerequisite.WizardClass = wizard;
            prerequisite.SchoolSelection = schools;
            prerequisite.OppositionSelection = opposition;
            prerequisite.Universalist = universalist;
            feature.ComponentsArray = new BlueprintComponent[] { prerequisite };
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
