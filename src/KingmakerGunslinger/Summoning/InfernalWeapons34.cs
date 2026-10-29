using System;
using System.Collections.Generic;
using System.Linq;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Summoning
{
    // Immutable blueprint configuration. No shared mutable item or unit state.
    public sealed class InfernalMartialProfile34 : BlueprintComponent
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintItemWeapon Glaive;
        public BlueprintItemWeapon Claw;
        public BlueprintItemWeapon Chain;
        public BlueprintBuff DancingChains;

        internal static InfernalMartialProfile34 For(UnitEntityData owner)
        {
            ModContext context;
            if (owner?.Blueprint == null || !ModContext.TryGet(out context) ||
                context.FeatureModules?.Active?.ExpandedSummoning != true) return null;
            var profile = owner.Blueprint.GetComponent<InfernalMartialProfile34>();
            return profile != null && ReferenceEquals(profile.OwningBlueprint, owner.Blueprint)
                ? profile : null;
        }

        internal bool ControlsOwnChains(UnitEntityData owner)
        {
            return DancingChains != null && InfernalEffectLifetime34.SourceIsLive(owner) &&
                owner.Buffs.Enumerable.Any(buff => buff.IsTurnedOn &&
                    ReferenceEquals(buff.Blueprint, DancingChains) &&
                    ReferenceEquals(buff.Context?.MaybeCaster, owner));
        }
    }

    // The existing giant/Griffon hook is needed because Kingmaker's type getter
    // subtracts a four-foot allowance while ItemEntityWeapon adds owner reach.
    // Correct only the four-foot allowance; retain native owner reach and live bonuses.
    [HarmonyPatch(typeof(ItemEntityWeapon), "get_AttackRange")]
    internal static class InfernalWeaponReach34
    {
        private static void Postfix(ItemEntityWeapon __instance, ref Feet __result)
        {
            var owner = __instance?.Wielder?.Unit;
            var profile = InfernalMartialProfile34.For(owner);
            if (profile == null || __instance.Blueprint == null) return;
            if (profile.Glaive != null && ReferenceEquals(__instance.Blueprint, profile.Glaive))
                __result = new Feet(__result.Value + 4);
            else if (profile.Claw != null && ReferenceEquals(__instance.Blueprint, profile.Claw))
                return; // Private raw range 9 already yields five feet.
            else if (profile.Chain != null && ReferenceEquals(__instance.Blueprint, profile.Chain))
                __result = new Feet(__result.Value + 4 + (profile.ControlsOwnChains(owner) ?
                    InfernalRules34.ControlledChainReachFeet - InfernalRules34.ChainReachFeet : 0));
        }
    }

    // Manufactured polearm and natural claws are alternatives. The native
    // routine still owns iterative bonuses, haste, single attacks and charges.
    [HarmonyPatch(typeof(UnitAttack), "CreateFullAttack")]
    internal static class BarbazuAlternativeRoutine34
    {
        private static void Postfix(UnitAttack __instance, List<AttackHandInfo> __result)
        {
            var owner = __instance?.Executor;
            var profile = InfernalMartialProfile34.For(owner);
            if (profile?.Glaive == null || profile.Claw == null || __result == null ||
                !ReferenceEquals(owner.Body?.PrimaryHand?.MaybeWeapon?.Blueprint, profile.Glaive)) return;
            __result.RemoveAll(attack => attack?.Hand?.MaybeWeapon != null &&
                ReferenceEquals(attack.Hand.MaybeWeapon.Blueprint, profile.Claw));
        }
    }
}
