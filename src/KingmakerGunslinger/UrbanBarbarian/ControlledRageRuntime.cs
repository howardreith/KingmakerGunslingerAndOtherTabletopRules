using System;
using System.Collections.Generic;
using System.Linq;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;

namespace KingmakerGunslinger.UrbanBarbarian
{
    internal static class ControlledRageRuntime
    {
        private static BlueprintBuff _nativeRage;
        private static BlueprintBuff _urbanRage;
        private static BlueprintFeature _ownerFeature;
        private static BlueprintFeature _greaterRage;
        private static BlueprintFeature _mightyRage;
        private static BlueprintAbility _selector;
        private static IDictionary<BlueprintAbility, ControlledRageAllocation>
            _allocationsByAbility;
        private static IDictionary<ControlledRageAllocation, BlueprintFeature>
            _factsByAllocation;

        internal static void Configure(BlueprintBuff nativeRage,
            BlueprintBuff urbanRage, BlueprintFeature ownerFeature,
            BlueprintFeature greaterRage, BlueprintFeature mightyRage,
            BlueprintAbility selector,
            IDictionary<BlueprintAbility, ControlledRageAllocation> abilities,
            IDictionary<ControlledRageAllocation, BlueprintFeature> facts)
        {
            if (nativeRage == null || urbanRage == null || ownerFeature == null ||
                greaterRage == null || mightyRage == null || selector == null ||
                abilities == null || facts == null || abilities.Count != 31 ||
                facts.Count != 31)
                throw new ArgumentException(
                    "The complete Controlled Rage runtime graph is required.");
            _nativeRage = nativeRage; _urbanRage = urbanRage;
            _ownerFeature = ownerFeature; _greaterRage = greaterRage;
            _mightyRage = mightyRage; _selector = selector;
            _allocationsByAbility = new Dictionary<BlueprintAbility,
                ControlledRageAllocation>(abilities);
            _factsByAllocation = new Dictionary<ControlledRageAllocation,
                BlueprintFeature>(facts);
        }

        internal static BlueprintBuff Substitute(BuffCollection collection,
            BlueprintBuff attempted)
        {
            return collection != null && collection.Owner != null &&
                ReferenceEquals(attempted, _nativeRage) && _urbanRage != null &&
                _ownerFeature != null && collection.Owner.HasFact(_ownerFeature) ?
                _urbanRage : attempted;
        }

        internal static bool TryCurrentTier(UnitDescriptor owner,
            out ControlledRageTier tier)
        {
            tier = ControlledRageTier.Ordinary;
            if (owner == null || _ownerFeature == null ||
                !owner.HasFact(_ownerFeature)) return false;
            tier = ControlledRageAllocationPolicy.ResolveTier(
                owner.HasFact(_greaterRage), owner.HasFact(_mightyRage));
            return true;
        }

        internal static bool IsUrbanRageActive(UnitDescriptor owner)
        {
            return owner != null && ((_urbanRage != null &&
                owner.HasFact(_urbanRage)) || (_nativeRage != null &&
                owner.HasFact(_nativeRage)));
        }

        internal static ControlledRageAllocation ResolveSelection(
            UnitDescriptor owner, bool reconcileDefault)
        {
            ControlledRageTier tier;
            if (!TryCurrentTier(owner, out tier)) return null;
            ControlledRageAllocation[] selected =
                ControlledRageAllocationPolicy.Generate(tier).Where(value =>
                    owner.HasFact(_factsByAllocation[value])).ToArray();
            if (selected.Length == 1) return selected[0];
            if (!reconcileDefault) return null;
            foreach (ControlledRageAllocation value in
                ControlledRageAllocationPolicy.Generate(tier))
                if (owner.HasFact(_factsByAllocation[value]))
                    owner.RemoveFact(_factsByAllocation[value]);
            ControlledRageAllocation fallback =
                ControlledRageAllocationPolicy.Default(tier);
            return owner.AddFact(_factsByAllocation[fallback]) == null ? null :
                fallback;
        }

        internal static IList<AbilityData> FilterVariants(AbilityData parent,
            IList<AbilityData> variants)
        {
            if (parent == null || !ReferenceEquals(parent.Blueprint, _selector) ||
                variants == null || parent.Caster == null) return variants;
            ControlledRageTier tier;
            if (!TryCurrentTier(parent.Caster, out tier))
                return new AbilityData[0];
            return variants.Where(value => value != null &&
                value.Blueprint != null && _allocationsByAbility.ContainsKey(
                    value.Blueprint) &&
                _allocationsByAbility[value.Blueprint].Total == (int)tier)
                .ToArray();
        }

        internal static bool IsSelected(AbilityData ability)
        {
            if (ability == null || ability.Caster == null ||
                ability.Blueprint == null || _allocationsByAbility == null)
                return false;
            ControlledRageAllocation allocation;
            return _allocationsByAbility.TryGetValue(ability.Blueprint,
                out allocation) && ability.Caster.HasFact(
                    _factsByAllocation[allocation]);
        }
    }

    public sealed class ControlledRageAbilityScoreBonus :
        OwnedGameLogicComponent<UnitDescriptor>
    {
        private ModifiableValue.Modifier _strength;
        private ModifiableValue.Modifier _dexterity;
        private ModifiableValue.Modifier _constitution;

        public override void OnTurnOn()
        {
            Remove();
            if (Owner == null || Owner.Stats == null) return;
            ControlledRageAllocation allocation =
                ControlledRageRuntime.ResolveSelection(Owner, true);
            if (allocation == null) return;
            if (allocation.Strength > 0)
                _strength = Owner.Stats.Strength.AddModifier(allocation.Strength,
                    Fact, GetType().FullName, ModifierDescriptor.Morale);
            if (allocation.Dexterity > 0)
                _dexterity = Owner.Stats.Dexterity.AddModifier(allocation.Dexterity,
                    Fact, GetType().FullName, ModifierDescriptor.Morale);
            if (allocation.Constitution > 0)
                _constitution = Owner.Stats.Constitution.AddModifier(
                    allocation.Constitution, Fact, GetType().FullName,
                    ModifierDescriptor.Morale);
        }

        public override void OnTurnOff() { Remove(); }

        private void Remove()
        {
            if (Owner != null && Owner.Stats != null)
            {
                if (_strength != null) Owner.Stats.Strength.RemoveModifier(_strength);
                if (_dexterity != null) Owner.Stats.Dexterity.RemoveModifier(_dexterity);
                if (_constitution != null)
                    Owner.Stats.Constitution.RemoveModifier(_constitution);
            }
            _strength = _dexterity = _constitution = null;
        }
    }

    [HarmonyPatch(typeof(BuffCollection), "AddBuff", new[] {
        typeof(BlueprintBuff), typeof(MechanicsContext), typeof(TimeSpan?) })]
    [HarmonyAfter("CallOfTheWild")]
    internal static class ControlledRageBuffSubstitutionPatch
    {
        private static void Prefix(BuffCollection __instance,
            ref BlueprintBuff __0)
        { __0 = ControlledRageRuntime.Substitute(__instance, __0); }
    }

    [HarmonyPatch(typeof(AbilityData), "get_Variants")]
    [HarmonyAfter("CallOfTheWild")]
    internal static class ControlledRageVariantsPatch
    {
        private static void Postfix(AbilityData __instance,
            ref IList<AbilityData> __result)
        { __result = ControlledRageRuntime.FilterVariants(__instance, __result); }
    }

    [HarmonyPatch(typeof(AbilityData), "get_Name")]
    internal static class ControlledRageSelectedNamePatch
    {
        private static void Postfix(AbilityData __instance, ref string __result)
        {
            if (ControlledRageRuntime.IsSelected(__instance))
                __result = "Selected -- " + __result;
        }
    }
}
