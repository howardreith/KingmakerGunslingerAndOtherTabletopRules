using System;
using System.Linq;
using Harmony12;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace KingmakerGunslinger.EasternWeapons
{
    internal static class EasternWeaponProficiencyRuntime
    {
        private static readonly object Sync = new object();
        private static BlueprintFeature[] _broadMartialFacts =
            Array.Empty<BlueprintFeature>();

        internal static void Configure(BlueprintFeature[] broadMartialFacts)
        {
            if (broadMartialFacts == null ||
                broadMartialFacts.Any(value => value == null) ||
                broadMartialFacts.Distinct().Count() != broadMartialFacts.Length)
                throw new ArgumentException(
                    "Broad martial proficiency facts are incomplete.");
            lock (Sync)
                _broadMartialFacts =
                    (BlueprintFeature[])broadMartialFacts.Clone();
        }

        internal static void Rollback()
        {
            lock (Sync) _broadMartialFacts = Array.Empty<BlueprintFeature>();
        }

        internal static bool HasBroadMartial(UnitDescriptor unit)
        {
            if (unit == null) return false;
            BlueprintFeature[] facts;
            lock (Sync) facts = (BlueprintFeature[])_broadMartialFacts.Clone();
            return facts.Any(unit.HasFact);
        }

        internal static bool IsProficient(UnitDescriptor unit,
            ItemEntityWeapon weapon)
        {
            if (unit == null || weapon == null || weapon.Blueprint == null ||
                weapon.Blueprint.Type == null) return false;
            WeaponCategory category = weapon.Blueprint.Type.Category;
            if (unit.Proficiencies.Contains(category)) return true;
            if (EasternWeaponCategoryRuntime.IsKatana(category))
                return weapon.HoldInTwoHands && HasBroadMartial(unit);
            return false;
        }
    }

    /// <summary>
    /// Runtime-added categories are outside Kingmaker's closed native enum
    /// switch. Apply the ordinary -4 only when the exact category/grip policy
    /// says the wielder is not proficient.
    /// </summary>
    internal sealed class EasternWeaponProficiencyPenaltyComponent :
        WeaponEnchantmentLogic,
        IInitiatorRulebookHandler<RuleAttackRoll>
    {
        internal static EasternWeaponProficiencyPenaltyComponent Create()
        {
            return ScriptableObject.CreateInstance<
                EasternWeaponProficiencyPenaltyComponent>();
        }

        public void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            if (evt == null || evt.Initiator == null || evt.Weapon == null ||
                evt.Weapon.Blueprint == null || evt.Weapon.Blueprint.Type == null)
                return;
            CustomWeapons.CustomWeaponCategoryDefinition definition;
            if (!EasternWeaponCategoryRuntime.TryGet(
                    evt.Weapon.Blueprint.Type.Category, out definition) ||
                EasternWeaponProficiencyRuntime.IsProficient(
                    evt.Initiator.Descriptor, evt.Weapon))
                return;
            evt.SetAttackBonusPenalty(evt.AttackBonusPenalty + 4);
        }

        public void OnEventDidTrigger(RuleAttackRoll evt) { }
    }

    /// <summary>
    /// Nodachi has one primary engine group (Heavy Blades), plus Polearms.
    /// Resolve the greater native training rank across the two groups so two
    /// qualifying facts never manufacture a doubled bonus.
    /// </summary>
    [HarmonyPatch(typeof(UnitPartWeaponTraining), "GetWeaponRank",
        new[] { typeof(ItemEntityWeapon) })]
    internal static class EasternWeaponMultiGroupTrainingPatch
    {
        private static void Postfix(UnitPartWeaponTraining __instance,
            ItemEntityWeapon weapon, ref int __result)
        {
            if (__instance == null || weapon == null || weapon.Blueprint == null ||
                weapon.Blueprint.Type == null ||
                !weapon.Blueprint.Type.Category.Equals(
                    EasternWeaponCategoryRuntime.Category(
                        EasternWeaponFamily.Nodachi)))
                return;
            foreach (Fact fact in __instance.WeaponTrainings ??
                new System.Collections.Generic.List<Fact>())
            {
                if (fact == null) continue;
                WeaponGroupAttackBonus training = fact.Get<WeaponGroupAttackBonus>();
                if (training == null ||
                    (training.WeaponGroup != WeaponFighterGroup.BladesHeavy &&
                     training.WeaponGroup != WeaponFighterGroup.Polearms))
                    continue;
                __result = Math.Max(__result, fact.GetRank());
            }
        }
    }
}
