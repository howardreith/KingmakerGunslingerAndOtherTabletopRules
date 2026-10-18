using System;
using Harmony12;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Items;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Utility;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Summoning
{
    internal static class ElephantNativeCombat
    {
        internal const string GoreGuid = "de42c58801037b84c9d992634ddd7220";
        internal const string SlamGuid = "c2ce7bc3559b2024ea91ddf5bb321f0a";

        internal static bool Owns(UnitEntityData owner)
        {
            ModContext context;
            return owner != null && owner.Blueprint != null &&
                owner.Blueprint.AssetGuid == GiantBodyRulesPolicy.ElephantUnitGuid &&
                ModContext.TryGet(out context) && context.FeatureModules?.Active != null &&
                context.FeatureModules.Active.ExpandedSummoning;
        }
    }

    // Like the qualified Triceratops exact-owner range patch: neither shared
    // native weapon type nor Mastodon/donor consumers are changed. Native body
    // radii are still added by UnitAttack, not baked into the printed reach.
    [HarmonyPatch(typeof(ItemEntityWeapon), "get_AttackRange")]
    internal static class ElephantNaturalReachPatch
    {
        private static void Postfix(ItemEntityWeapon __instance, ref Feet __result)
        {
            var owner = __instance?.Wielder?.Unit;
            string weapon = __instance?.Blueprint?.AssetGuid;
            if (ElephantNativeCombat.Owns(owner) &&
                (weapon == ElephantNativeCombat.GoreGuid || weapon == ElephantNativeCombat.SlamGuid))
                __result = new Feet(GiantBodyRulesPolicy.ElephantReachFeet);
        }
    }

    // The released two-primary Elephant must not take the native primary-hand
    // lone-weapon Strength term. Reuse the qualified multi-primary excess
    // calculation; leave Power Attack, secondary roles and all other owners alone.
    [HarmonyPatch(typeof(RuleCalculateWeaponStats), "OnTrigger")]
    internal static class ElephantPrimaryGoreStrengthPatch
    {
        private static void Postfix(RuleCalculateWeaponStats __instance)
        {
            var owner = __instance?.Initiator;
            var weapon = __instance?.Weapon;
            if (!ElephantNativeCombat.Owns(owner) || weapon == null ||
                weapon.Blueprint.AssetGuid != ElephantNativeCombat.GoreGuid ||
                !weapon.Blueprint.IsNatural || weapon.IsSecondary || __instance.SecondaryWeapon ||
                __instance.DamageBonusStat != StatType.Strength ||
                __instance.DamageBonusStatMultiplier <= 1f || owner.Body?.PrimaryHand == null ||
                !ReferenceEquals(owner.Body.PrimaryHand.MaybeWeapon, weapon) ||
                __instance.DamageDescription == null || __instance.DamageDescription.Count == 0) return;
            int excess = TheropodRulesPolicy.MultipleNaturalPrimaryExcessStrength(owner.Stats.Strength.Bonus);
            if (excess == 0) return;
            var row = __instance.DamageDescription[0];
            row.Bonus -= excess;
            __instance.DamageDescription[0] = row;
        }
    }
}
