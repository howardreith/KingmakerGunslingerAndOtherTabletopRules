using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.FactLogic;
using KingmakerGunslinger.EasternWeapons;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class HeirloomNodachiBlueprintSet
    {
        internal HeirloomNodachiBlueprintSet(
            BlueprintFeatureSelection selection, BlueprintFeature proficiency,
            BlueprintFeature opportunity, BlueprintFeature combatManeuver,
            BlueprintFeature combatManeuverBonus)
        {
            Selection = selection;
            Proficiency = proficiency;
            Opportunity = opportunity;
            CombatManeuver = combatManeuver;
            CombatManeuverBonus = combatManeuverBonus;
        }

        internal BlueprintFeatureSelection Selection { get; private set; }
        internal BlueprintFeature Proficiency { get; private set; }
        internal BlueprintFeature Opportunity { get; private set; }
        internal BlueprintFeature CombatManeuver { get; private set; }
        internal BlueprintFeature CombatManeuverBonus { get; private set; }
    }

    /// <summary>
    /// Save-stable KMG identities reproducing Favored Class 1.3.1's three-way
    /// Heirloom Weapon choice for the KMG-owned martial Nodachi category.
    /// Publication into the foreign Equipment Trait selection is late-bound.
    /// </summary>
    internal static class HeirloomNodachiBlueprints
    {
        internal const string SelectionSymbol =
            "KMG.Traits.HeirloomWeapon.Nodachi.Selection";
        internal const string ProficiencySymbol =
            "KMG.Traits.HeirloomWeapon.Nodachi.Proficiency";
        internal const string OpportunitySymbol =
            "KMG.Traits.HeirloomWeapon.Nodachi.AttackOfOpportunity";
        internal const string CombatManeuverSymbol =
            "KMG.Traits.HeirloomWeapon.Nodachi.CombatManeuver";
        internal const string CombatManeuverBonusSymbol =
            "KMG.Traits.HeirloomWeapon.Nodachi.CombatManeuverBonus";
        internal const string Description =
            "You carry a masterwork simple or martial weapon that has been passed down from generation to generation in your family (pay the standard gp cost for the weapon).\nWhen you select this trait, choose one of the following benefits: proficiency with that specific weapon, a +1 trait bonus on attacks of opportunity with that specific weapon, or a +2 trait bonus on all combat maneuvers when using that specific weapon.";

        internal static HeirloomNodachiBlueprintSet Register(
            BlueprintRegistry registry, BlueprintItemWeapon masterworkNodachi)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            if (masterworkNodachi == null || masterworkNodachi.Icon == null ||
                masterworkNodachi.Type == null ||
                (int)masterworkNodachi.Type.Category !=
                    EasternWeaponMartialPublicationPolicy.NodachiCategoryValue)
                throw new ArgumentException(
                    "The masterwork Nodachi registration donor is invalid.");
            WeaponCategory nodachi = masterworkNodachi.Type.Category;
            Sprite icon = masterworkNodachi.Icon;

            BlueprintFeature hidden = registry.Register<BlueprintFeature>(
                CombatManeuverBonusSymbol, () => CreateHiddenCombatManeuver());
            BlueprintFeature proficiency = registry.Register<BlueprintFeature>(
                ProficiencySymbol, () => CreateProficiency(nodachi, icon));
            BlueprintFeature opportunity = registry.Register<BlueprintFeature>(
                OpportunitySymbol, () => CreateOpportunity(nodachi, icon));
            BlueprintFeature combatManeuver = registry.Register<BlueprintFeature>(
                CombatManeuverSymbol, () => CreateCombatManeuver(nodachi,
                    hidden, icon));
            BlueprintFeatureSelection selection = registry.Register<
                BlueprintFeatureSelection>(SelectionSymbol, () =>
                    CreateSelection(masterworkNodachi, icon));
            selection.AllFeatures = new[]
                { proficiency, opportunity, combatManeuver };
            var noFeature = ScriptableObject.CreateInstance<
                PrerequisiteNoFeature>();
            noFeature.name = "$KMG_HeirloomNodachi_NoDuplicate";
            noFeature.Feature = selection;
            noFeature.Group = Prerequisite.GroupType.All;
            selection.ComponentsArray = selection.ComponentsArray.Concat(
                new BlueprintComponent[] { noFeature }).ToArray();

            var set = new HeirloomNodachiBlueprintSet(selection, proficiency,
                opportunity, combatManeuver, hidden);
            Validate(set, masterworkNodachi);
            return set;
        }

        internal static void Validate(HeirloomNodachiBlueprintSet set,
            BlueprintItemWeapon masterworkNodachi)
        {
            if (set == null || set.Selection == null ||
                set.Selection.Ranks != 1 || set.Selection.HideInUI ||
                set.Selection.IsClassFeature ||
                !set.Selection.HideInCharacterSheetAndLevelUp ||
                set.Selection.Groups == null ||
                !set.Selection.Groups.Contains(FeatureGroup.Trait) ||
                set.Selection.AllFeatures == null ||
                set.Selection.AllFeatures.Length != 3 ||
                !set.Selection.AllFeatures.SequenceEqual(new[]
                    { set.Proficiency, set.Opportunity, set.CombatManeuver }))
                throw new InvalidOperationException(
                    "Heirloom Weapon (Nodachi) selection structure changed.");
            WeaponCategory nodachi = masterworkNodachi.Type.Category;
            AddStartingEquipment starting = set.Selection.ComponentsArray
                .OfType<AddStartingEquipment>().Single();
            if (starting.BasicItems == null || starting.BasicItems.Length != 1 ||
                !ReferenceEquals(starting.BasicItems[0], masterworkNodachi) ||
                set.Selection.ComponentsArray.OfType<PrerequisiteNoFeature>()
                    .Count(value => ReferenceEquals(value.Feature,
                        set.Selection)) != 1 ||
                set.Proficiency.ComponentsArray.OfType<
                    PrerequisiteNotProficient>().Single()
                    .WeaponProficiencies.Single() != nodachi ||
                set.Proficiency.ComponentsArray.OfType<AddProficiencies>()
                    .Single().WeaponProficiencies.Single() != nodachi ||
                set.Opportunity.ComponentsArray.OfType<
                    PrerequisiteProficiency>().Single()
                    .WeaponProficiencies.Single() != nodachi ||
                set.Opportunity.ComponentsArray.OfType<
                    HeirloomNodachiOpportunityBonus>().Count() != 1 ||
                set.CombatManeuver.ComponentsArray.OfType<
                    PrerequisiteProficiency>().Single()
                    .WeaponProficiencies.Single() != nodachi ||
                set.CombatManeuver.ComponentsArray.OfType<
                    HeirloomNodachiCombatManeuverCarrier>().Single().Feature !=
                        set.CombatManeuverBonus)
                throw new InvalidOperationException(
                    "Heirloom Weapon (Nodachi) choice mechanics changed.");
            AddStatBonus bonus = set.CombatManeuverBonus.ComponentsArray
                .OfType<AddStatBonus>().Single();
            if (bonus.Stat != StatType.AdditionalCMB || bonus.Value != 2 ||
                bonus.Descriptor != ModifierDescriptor.Trait ||
                !set.CombatManeuverBonus.HideInUI ||
                !set.CombatManeuverBonus.HideInCharacterSheetAndLevelUp)
                throw new InvalidOperationException(
                    "Heirloom Weapon (Nodachi) hidden CMB benefit changed.");
        }

        private static BlueprintFeatureSelection CreateSelection(
            BlueprintItemWeapon masterworkNodachi, Sprite icon)
        {
            var feature = ScriptableObject.CreateInstance<
                BlueprintFeatureSelection>();
            Configure(feature, "KMG_HeirloomWeapon_Nodachi_Selection",
                "Heirloom Weapon: Nodachi", Description, icon,
                FeatureGroup.Trait, false);
            feature.HideInCharacterSheetAndLevelUp = true;
            feature.Features = Array.Empty<BlueprintFeature>();
            feature.AllFeatures = Array.Empty<BlueprintFeature>();
            var starting = ScriptableObject.CreateInstance<AddStartingEquipment>();
            starting.name = "$KMG_HeirloomNodachi_StartingEquipment";
            starting.BasicItems = new BlueprintItem[] { masterworkNodachi };
            starting.CategoryItems = Array.Empty<WeaponCategory>();
            starting.RestrictedByClass = Array.Empty<BlueprintCharacterClass>();
            feature.ComponentsArray = new BlueprintComponent[] { starting };
            return feature;
        }

        private static BlueprintFeature CreateProficiency(
            WeaponCategory nodachi, Sprite icon)
        {
            var absent = ScriptableObject.CreateInstance<
                PrerequisiteNotProficient>();
            absent.name = "$KMG_HeirloomNodachi_NotProficient";
            absent.WeaponProficiencies = new[] { nodachi };
            absent.ArmorProficiencies = Array.Empty<ArmorProficiencyGroup>();
            var grant = ScriptableObject.CreateInstance<AddProficiencies>();
            grant.name = "$KMG_HeirloomNodachi_ProficiencyGrant";
            grant.RaceRestriction = null;
            grant.WeaponProficiencies = new[] { nodachi };
            grant.ArmorProficiencies = Array.Empty<ArmorProficiencyGroup>();
            return CreateFeature("KMG_HeirloomWeapon_Nodachi_Proficiency",
                "Heirloom Weapon: Nodachi (Proficiency)", icon, absent, grant);
        }

        private static BlueprintFeature CreateOpportunity(
            WeaponCategory nodachi, Sprite icon)
        {
            var proficient = CreateProficient(nodachi,
                "$KMG_HeirloomNodachi_Aoo_Proficient");
            var bonus = ScriptableObject.CreateInstance<
                HeirloomNodachiOpportunityBonus>();
            bonus.name = "$KMG_HeirloomNodachi_AooBonus";
            return CreateFeature("KMG_HeirloomWeapon_Nodachi_AooBonus",
                "Heirloom Weapon: Nodachi (AOO Bonus)", icon, proficient,
                bonus);
        }

        private static BlueprintFeature CreateCombatManeuver(
            WeaponCategory nodachi, BlueprintFeature hidden, Sprite icon)
        {
            var proficient = CreateProficient(nodachi,
                "$KMG_HeirloomNodachi_Cmb_Proficient");
            var carrier = ScriptableObject.CreateInstance<
                HeirloomNodachiCombatManeuverCarrier>();
            carrier.name = "$KMG_HeirloomNodachi_CmbCarrier";
            carrier.Feature = hidden;
            return CreateFeature("KMG_HeirloomWeapon_Nodachi_CmbBonus",
                "Heirloom Weapon: Nodachi (CMB Bonus)", icon, proficient,
                carrier);
        }

        private static BlueprintFeature CreateHiddenCombatManeuver()
        {
            var bonus = ScriptableObject.CreateInstance<AddStatBonus>();
            bonus.name = "$KMG_HeirloomNodachi_AdditionalCmb";
            bonus.Stat = StatType.AdditionalCMB;
            bonus.Value = 2;
            bonus.Descriptor = ModifierDescriptor.Trait;
            BlueprintFeature feature = CreateFeature(
                "KMG_HeirloomWeapon_Nodachi_CmbBonusFeature",
                "Heirloom Weapon: Nodachi (CMB Bonus)", null, bonus);
            feature.Groups = Array.Empty<FeatureGroup>();
            feature.HideInUI = true;
            feature.HideInCharacterSheetAndLevelUp = true;
            return feature;
        }

        private static PrerequisiteProficiency CreateProficient(
            WeaponCategory nodachi, string name)
        {
            var value = ScriptableObject.CreateInstance<
                PrerequisiteProficiency>();
            value.name = name;
            value.WeaponProficiencies = new[] { nodachi };
            value.ArmorProficiencies = Array.Empty<ArmorProficiencyGroup>();
            return value;
        }

        private static BlueprintFeature CreateFeature(string internalName,
            string displayName, Sprite icon, params BlueprintComponent[] components)
        {
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            Configure(feature, internalName, displayName, Description, icon,
                FeatureGroup.Trait, false);
            feature.ComponentsArray = components ??
                Array.Empty<BlueprintComponent>();
            return feature;
        }

        private static void Configure(BlueprintFeature feature,
            string internalName, string displayName, string description,
            Sprite icon, FeatureGroup group, bool hidden)
        {
            feature.name = internalName;
            feature.Ranks = 1;
            feature.HideInUI = hidden;
            feature.IsClassFeature = false;
            feature.Groups = new[] { group };
            feature.ComponentsArray = Array.Empty<BlueprintComponent>();
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create(internalName + ".Name", displayName),
                LocalizationService.Create(internalName + ".Description",
                    description), icon);
        }
    }
}
