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
using Kingmaker.UI.UnitSettings;
using UnityEngine;

namespace KingmakerGunslinger.UrbanBarbarian
{
    internal static class ControlledRageRuntime
    {
        private static BlueprintBuff _nativeRage;
        private static BlueprintBuff _urbanRage;
        private static BlueprintFeature _ownerFeature;
        private static BlueprintFeature _greaterRage;
        private static BlueprintFeature _mightyRage;
        private static BlueprintAbility _legacySelector;
        private static IDictionary<ControlledRageTier, BlueprintAbility>
            _selectorsByTier;
        private static IDictionary<BlueprintAbility, ControlledRageTier>
            _tiersBySelector;
        private static IDictionary<BlueprintAbility, ControlledRageAllocation>
            _allocationsByAbility;
        private static IDictionary<ControlledRageAllocation, BlueprintAbility>
            _abilitiesByAllocation;
        private static IDictionary<ControlledRageAllocation, BlueprintFeature>
            _factsByAllocation;
        private static IDictionary<BlueprintFeature, ControlledRageAllocation>
            _allocationsByFact;
        private static IDictionary<ControlledRageAllocation, Sprite>
            _selectedIcons;

        internal static void Configure(BlueprintBuff nativeRage,
            BlueprintBuff urbanRage, BlueprintFeature ownerFeature,
            BlueprintFeature greaterRage, BlueprintFeature mightyRage,
            BlueprintAbility legacySelector,
            IDictionary<ControlledRageTier, BlueprintAbility> selectors,
            IDictionary<BlueprintAbility, ControlledRageAllocation> abilities,
            IDictionary<ControlledRageAllocation, BlueprintFeature> facts,
            IDictionary<ControlledRageAllocation, Sprite> selectedIcons)
        {
            if (nativeRage == null || urbanRage == null || ownerFeature == null ||
                greaterRage == null || mightyRage == null ||
                legacySelector == null || selectors == null ||
                selectors.Count != 3 || selectors.Any(value =>
                    value.Value == null) ||
                abilities == null || facts == null || abilities.Count != 31 ||
                facts.Count != 31 || selectedIcons == null ||
                selectedIcons.Count != 31)
                throw new ArgumentException(
                    "The complete Controlled Rage runtime graph is required.");
            _nativeRage = nativeRage; _urbanRage = urbanRage;
            _ownerFeature = ownerFeature; _greaterRage = greaterRage;
            _mightyRage = mightyRage; _legacySelector = legacySelector;
            _selectorsByTier = new Dictionary<ControlledRageTier,
                BlueprintAbility>(selectors);
            _tiersBySelector = selectors.ToDictionary(value => value.Value,
                value => value.Key);
            _allocationsByAbility = new Dictionary<BlueprintAbility,
                ControlledRageAllocation>(abilities);
            _abilitiesByAllocation = abilities.ToDictionary(value => value.Value,
                value => value.Key);
            _factsByAllocation = new Dictionary<ControlledRageAllocation,
                BlueprintFeature>(facts);
            _allocationsByFact = facts.ToDictionary(value => value.Value,
                value => value.Key);
            _selectedIcons = new Dictionary<ControlledRageAllocation, Sprite>(
                selectedIcons);
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
            UnitPartControlledRageSelection part = owner.Get<
                UnitPartControlledRageSelection>();
            if (part == null)
            {
                if (selected.Length == 1)
                {
                    part = owner.Ensure<UnitPartControlledRageSelection>();
                    part.Unlock(tier);
                    if (!part.TrySelect(tier, selected[0], false)) return null;
                }
                else if (!reconcileDefault) return null;
                else
                {
                    part = owner.Ensure<UnitPartControlledRageSelection>();
                    part.Unlock(tier);
                }
            }
            else part.Unlock(tier);
            ControlledRageAllocation result = part.SelectionFor(tier);
            if (result == null) return null;
            return !reconcileDefault || (SynchronizeSelectionFacts(owner, part) &&
                SynchronizeSelector(owner, tier)) ? result : null;
        }

        internal static void UnlockTier(UnitDescriptor owner,
            ControlledRageTier tier)
        {
            if (owner == null || !ControlledRageAllocationPolicy.Generate(tier)
                    .Any()) return;
            UnitPartControlledRageSelection part = owner.Ensure<
                UnitPartControlledRageSelection>();
            part.Unlock(tier);
            SynchronizeSelectionFacts(owner, part);
            SynchronizeSelector(owner, tier);
        }

        internal static bool TrySelect(UnitDescriptor owner,
            ControlledRageAllocation allocation)
        {
            ControlledRageTier tier;
            if (owner == null || allocation == null ||
                !TryCurrentTier(owner, out tier) || allocation.Total != (int)tier ||
                IsUrbanRageActive(owner)) return false;
            UnitPartControlledRageSelection part = owner.Ensure<
                UnitPartControlledRageSelection>();
            part.Unlock(tier);
            ControlledRageAllocation previous = part.SelectionFor(tier);
            if (!part.TrySelect(tier, allocation, false)) return false;
            if (SynchronizeSelectionFacts(owner, part)) return true;
            part.TrySelect(tier, previous, false);
            SynchronizeSelectionFacts(owner, part);
            return false;
        }

        internal static bool TryResolveAllocation(BlueprintFeature feature,
            out ControlledRageAllocation allocation)
        {
            allocation = null;
            return feature != null && _allocationsByFact != null &&
                _allocationsByFact.TryGetValue(feature, out allocation);
        }

        internal static void ClearTierFacts(UnitDescriptor owner,
            ControlledRageTier tier)
        {
            if (owner == null || _factsByAllocation == null) return;
            foreach (ControlledRageAllocation allocation in
                ControlledRageAllocationPolicy.Generate(tier))
                if (owner.HasFact(_factsByAllocation[allocation]))
                    owner.RemoveFact(_factsByAllocation[allocation]);
        }

        internal static void RemoveTier(UnitDescriptor owner,
            ControlledRageTier tier)
        {
            ClearTierFacts(owner, tier);
            if (owner == null || _selectorsByTier == null) return;
            RemoveSelector(owner, _selectorsByTier[tier]);
            if (tier == ControlledRageTier.Ordinary)
            {
                RemoveSelector(owner, _legacySelector);
                foreach (BlueprintAbility selector in _selectorsByTier.Values)
                    RemoveSelector(owner, selector);
                return;
            }
            if (_ownerFeature == null || !owner.HasFact(_ownerFeature)) return;
            ControlledRageTier fallback = tier == ControlledRageTier.Mighty &&
                _greaterRage != null && owner.HasFact(_greaterRage) ?
                ControlledRageTier.Greater : ControlledRageTier.Ordinary;
            SynchronizeSelector(owner, fallback);
        }

        private static bool SynchronizeSelectionFacts(UnitDescriptor owner,
            UnitPartControlledRageSelection part)
        {
            foreach (ControlledRageTier tier in new[] {
                ControlledRageTier.Ordinary, ControlledRageTier.Greater,
                ControlledRageTier.Mighty })
            {
                ControlledRageAllocation selected = part.SelectionFor(tier);
                foreach (ControlledRageAllocation allocation in
                    ControlledRageAllocationPolicy.Generate(tier))
                    if (!Equals(allocation, selected) &&
                        owner.HasFact(_factsByAllocation[allocation]))
                        owner.RemoveFact(_factsByAllocation[allocation]);
                if (selected != null &&
                    !owner.HasFact(_factsByAllocation[selected]) &&
                    owner.AddFact(_factsByAllocation[selected]) == null)
                    return false;
            }
            return true;
        }

        private static bool SynchronizeSelector(UnitDescriptor owner,
            ControlledRageTier tier)
        {
            if (owner == null || _legacySelector == null ||
                _selectorsByTier == null || !_selectorsByTier.ContainsKey(tier))
                return false;
            BlueprintAbility selected = _selectorsByTier[tier];
            RemoveSelector(owner, _legacySelector);
            foreach (BlueprintAbility selector in _selectorsByTier.Values)
                if (!ReferenceEquals(selector, selected))
                    RemoveSelector(owner, selector);
            return owner.HasFact(selected) || owner.AddFact(selected) != null;
        }

        private static void RemoveSelector(UnitDescriptor owner,
            BlueprintAbility selector)
        {
            if (owner != null && selector != null && owner.HasFact(selector))
                owner.RemoveFact(selector);
        }

        internal static bool IsSelected(AbilityData ability)
        {
            if (ability == null || ability.Caster == null ||
                ability.Blueprint == null || _allocationsByAbility == null)
                return false;
            ControlledRageAllocation allocation;
            return _allocationsByAbility.TryGetValue(ability.Blueprint,
                out allocation) && Equals(ResolveSelection(
                    ability.Caster, false), allocation);
        }

        internal static Sprite PresentationIcon(AbilityData ability,
            Sprite fallback)
        {
            if (ability == null || ability.Caster == null ||
                ability.Blueprint == null || _selectedIcons == null)
                return fallback;
            ControlledRageAllocation allocation;
            if (_allocationsByAbility.TryGetValue(ability.Blueprint,
                    out allocation))
                return IsSelected(ability) ? _selectedIcons[allocation] : fallback;
            ControlledRageTier tier;
            if (_tiersBySelector == null || !_tiersBySelector.TryGetValue(
                    ability.Blueprint, out tier)) return fallback;
            allocation = ResolveSelection(ability.Caster, false);
            return allocation != null && allocation.Total == (int)tier ?
                _selectedIcons[allocation] : fallback;
        }

        internal static string PresentationTitle(AbilityData ability,
            string fallback)
        {
            if (ability == null || ability.Caster == null ||
                ability.Blueprint == null) return fallback;
            ControlledRageAllocation allocation;
            if (_allocationsByAbility != null && _allocationsByAbility.TryGetValue(
                    ability.Blueprint, out allocation))
                return IsSelected(ability) ? "Selected \u2713 " + fallback : fallback;
            ControlledRageTier tier;
            if (_tiersBySelector == null || !_tiersBySelector.TryGetValue(
                    ability.Blueprint, out tier)) return fallback;
            allocation = ResolveSelection(ability.Caster, false);
            return allocation == null || allocation.Total != (int)tier ? fallback :
                "Controlled Rage Allocation (+" + (int)tier + "): " +
                    allocation.Name;
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
            Owner.Stats.Strength.UpdateValue();
            Owner.Stats.Dexterity.UpdateValue();
            Owner.Stats.Constitution.UpdateValue();
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
                Owner.Stats.Strength.UpdateValue();
                Owner.Stats.Dexterity.UpdateValue();
                Owner.Stats.Constitution.UpdateValue();
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

    [HarmonyPatch(typeof(AbilityData), "get_Name")]
    internal static class ControlledRageSelectedNamePatch
    {
        private static void Postfix(AbilityData __instance, ref string __result)
        {
            if (ControlledRageRuntime.IsSelected(__instance))
                __result = "Selected -- " + __result;
        }
    }

    [HarmonyPatch(typeof(MechanicActionBarSlotAbility), "GetIcon")]
    internal static class ControlledRageLiveSlotIconPatch
    {
        private static void Postfix(MechanicActionBarSlotAbility __instance,
            ref Sprite __result)
        {
            __result = ControlledRageRuntime.PresentationIcon(
                __instance == null ? null : __instance.Ability, __result);
        }
    }

    [HarmonyPatch(typeof(MechanicActionBarSlotAbility), "GetTitle")]
    internal static class ControlledRageLiveSlotTitlePatch
    {
        private static void Postfix(MechanicActionBarSlotAbility __instance,
            ref string __result)
        {
            __result = ControlledRageRuntime.PresentationTitle(
                __instance == null ? null : __instance.Ability, __result);
        }
    }
}
