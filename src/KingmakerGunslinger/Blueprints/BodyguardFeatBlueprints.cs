using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.FactLogic;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class BodyguardFeatBlueprintSet
    {
        internal BodyguardFeatBlueprintSet(BlueprintFeature combatReflexes,
            BlueprintFeature bodyguard, BlueprintFeature inHarmsWay,
            BlueprintFeature helpfulCombat, BodyguardModeBlueprintSet modes)
        {
            CombatReflexes = combatReflexes ??
                throw new ArgumentNullException("combatReflexes");
            Bodyguard = bodyguard ?? throw new ArgumentNullException("bodyguard");
            InHarmsWay = inHarmsWay ?? throw new ArgumentNullException("inHarmsWay");
            HelpfulCombat = helpfulCombat ??
                throw new ArgumentNullException("helpfulCombat");
            Modes = modes ?? throw new ArgumentNullException("modes");
        }

        internal BlueprintFeature CombatReflexes { get; private set; }
        internal BlueprintFeature Bodyguard { get; private set; }
        internal BlueprintFeature InHarmsWay { get; private set; }
        internal BlueprintFeature HelpfulCombat { get; private set; }
        internal BodyguardModeBlueprintSet Modes { get; private set; }
        internal int Count { get { return 3 + Modes.Count; } }
    }

    internal static class BodyguardFeatBlueprints
    {
        internal const string BodyguardSymbol = "KMG.Feats.Bodyguard";
        internal const string InHarmsWaySymbol = "KMG.Feats.InHarmsWay";
        internal const string CombatReflexesGuid =
            "0f8939ae6f220984e8fb568abbdfba95";
        internal const string CombatReflexesInternalName = "CombatReflexes";

        internal static BodyguardFeatBlueprintSet Register(
            LibraryScriptableObject library, BlueprintRegistry registry)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (registry == null) throw new ArgumentNullException("registry");
            BlueprintFeature combatReflexes = BlueprintLibraryLookup.RequireExact<
                BlueprintFeature>(library, CombatReflexesGuid,
                    "native Combat Reflexes feat");
            ValidateCombatReflexes(combatReflexes);
            if (combatReflexes.Icon == null)
                throw new InvalidOperationException(
                    "Native Combat Reflexes has no icon for Bodyguard feat publication.");
            BodyguardModeBlueprintSet modes = BodyguardModeBlueprints.Register(
                registry, combatReflexes.Icon);
            BlueprintFeature bodyguard = registry.Register<BlueprintFeature>(
                BodyguardSymbol, () => CreateBodyguard(combatReflexes,
                    modes.BodyguardAbility));
            BlueprintFeature inHarmsWay = registry.Register<BlueprintFeature>(
                InHarmsWaySymbol, () => CreateInHarmsWay(bodyguard,
                    modes.InHarmsWayAbility, combatReflexes));
            BlueprintFeature helpfulCombat = HelpfulCombatBlueprints.Register(
                registry, combatReflexes.Icon);
            ValidateFeat(bodyguard, combatReflexes, modes.BodyguardAbility,
                "Bodyguard");
            ValidateFeat(inHarmsWay, bodyguard, modes.InHarmsWayAbility,
                "In Harm's Way");
            HelpfulCombatBlueprints.Validate(helpfulCombat);
            return new BodyguardFeatBlueprintSet(combatReflexes, bodyguard,
                inHarmsWay, helpfulCombat, modes);
        }

        internal static void ValidateCombatReflexes(BlueprintFeature feature)
        {
            if (feature == null || !string.Equals(feature.AssetGuid,
                    CombatReflexesGuid, StringComparison.Ordinal) ||
                !string.Equals(feature.name, CombatReflexesInternalName,
                    StringComparison.Ordinal) || feature.Groups == null ||
                !feature.Groups.Contains(FeatureGroup.Feat) ||
                !feature.Groups.Contains(FeatureGroup.CombatFeat) ||
                (feature.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                    .OfType<AddCondition>().Count(value => value.Condition ==
                        UnitCondition.AttackOfOpportunityBeforeInitiative) != 1)
                throw new InvalidOperationException(
                    "The exact native Combat Reflexes contract changed.");
        }

        private static BlueprintFeature CreateBodyguard(
            BlueprintFeature combatReflexes, BlueprintUnitFact mode)
        {
            return CreateFeat("KMG_Bodyguard_Feature", "Bodyguard",
                "KMG.Feat.Bodyguard", combatReflexes, mode,
                "When an adjacent ally is attacked, you may expend one available attack of opportunity to attempt an Aid Another melee attack roll against AC 10 with a melee attack that threatens the attacker. Success grants the ally your normal Aid Another AC bonus against that attack (normally +2; effects such as Helpful can increase it), and multiple Bodyguard bonuses stack. The attack of opportunity is spent even on failure and never executes a weapon attack. Activate Use Bodyguard to authorize this automatic reaction; owning the feat alone does not make it mandatory.",
                combatReflexes);
        }

        private static BlueprintFeature CreateInHarmsWay(BlueprintFeature bodyguard,
            BlueprintUnitFact mode, BlueprintFeature iconDonor)
        {
            return CreateFeat("KMG_InHarmsWay_Feature", "In Harm's Way",
                "KMG.Feat.InHarmsWay", bodyguard, mode,
                "After your Bodyguard attempt succeeds, if the protected ally is still hit, you may expend an available immediate action to become the recipient of that attack's complete damage and associated effects without rerolling the attack. Only one protector can intercept each attack. Activate Use In Harm's Way to authorize this automatic reaction; it has no effect unless Use Bodyguard is also active and succeeds for that attack.",
                iconDonor);
        }

        private static BlueprintFeature CreateFeat(string internalName,
            string displayName, string localizationStem, BlueprintFeature prerequisite,
            BlueprintUnitFact mode, string description, BlueprintFeature iconDonor)
        {
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = internalName;
            feature.Ranks = 1;
            feature.HideInUI = false;
            feature.IsClassFeature = false;
            feature.Groups = new[] { FeatureGroup.Feat, FeatureGroup.CombatFeat };
            var prerequisiteComponent =
                ScriptableObject.CreateInstance<PrerequisiteFeature>();
            prerequisiteComponent.name = "$KMG_" + internalName + "_Prerequisite";
            prerequisiteComponent.Feature = prerequisite;
            prerequisiteComponent.Group = Prerequisite.GroupType.All;
            var grant = ScriptableObject.CreateInstance<AddFacts>();
            grant.name = "$KMG_" + internalName + "_GrantMode";
            grant.Facts = new[] { mode };
            grant.DoNotRestoreMissingFacts = false;
            feature.ComponentsArray = new BlueprintComponent[] {
                prerequisiteComponent, grant };
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create(localizationStem + ".Name", displayName),
                LocalizationService.Create(localizationStem + ".Description",
                    description), iconDonor.Icon);
            return feature;
        }

        private static void ValidateFeat(BlueprintFeature feature,
            BlueprintFeature prerequisite, BlueprintUnitFact mode, string role)
        {
            if (feature == null || feature.Ranks != 1 || feature.HideInUI ||
                feature.IsClassFeature || feature.Icon == null || feature.Groups == null ||
                !feature.Groups.Contains(FeatureGroup.Feat) ||
                !feature.Groups.Contains(FeatureGroup.CombatFeat))
                throw new InvalidOperationException(role +
                    " must remain a visible rank-one general combat feat.");
            PrerequisiteFeature[] prerequisites = (feature.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).OfType<PrerequisiteFeature>()
                .ToArray();
            AddFacts[] grants = (feature.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).OfType<AddFacts>().ToArray();
            if (prerequisites.Length != 1 ||
                !ReferenceEquals(prerequisites[0].Feature, prerequisite) ||
                prerequisites[0].Group != Prerequisite.GroupType.All ||
                grants.Length != 1 || grants[0].Facts == null ||
                grants[0].Facts.Length != 1 ||
                !ReferenceEquals(grants[0].Facts[0], mode))
                throw new InvalidOperationException(role +
                    " prerequisite or automation-mode grant changed.");
        }
    }
}
