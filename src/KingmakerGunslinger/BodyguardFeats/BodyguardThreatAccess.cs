using System;
using System.Collections.Generic;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;

namespace KingmakerGunslinger.BodyguardFeats
{
    internal sealed class BodyguardSelectedAttack
    {
        internal BodyguardSelectedAttack(WeaponSlot slot, ItemEntityWeapon weapon,
            string identity, int attackBonus, int stableOrder)
        {
            Slot = slot ?? throw new ArgumentNullException("slot");
            Weapon = weapon ?? throw new ArgumentNullException("weapon");
            Identity = identity ?? throw new ArgumentNullException("identity");
            AttackBonus = attackBonus;
            StableOrder = stableOrder;
        }

        internal WeaponSlot Slot { get; private set; }
        internal ItemEntityWeapon Weapon { get; private set; }
        internal string Identity { get; private set; }
        internal int AttackBonus { get; private set; }
        internal int StableOrder { get; private set; }
    }

    /// <summary>
    /// Enumerates the same held/natural slot families used by native engagement,
    /// applies native melee/unarmed and IsReach semantics, then lets the pure
    /// selection policy choose the highest target-aware attack bonus.
    /// </summary>
    internal static class BodyguardThreatAccess
    {
        internal static BodyguardSelectedAttack SelectBestThreateningAttack(
            UnitEntityData protector, UnitEntityData attacker)
        {
            if (protector == null || attacker == null || protector.Body == null ||
                protector.Descriptor == null || protector.Descriptor.State == null)
                return null;

            var candidates = new List<BodyguardAttackCandidate<
                BodyguardSelectedAttack>>();
            int order = 0;
            AddCandidate(candidates, protector, attacker,
                protector.Body.PrimaryHand, "primary", order++);

            ItemEntityWeapon primary = protector.Body.PrimaryHand == null ? null :
                protector.Body.PrimaryHand.MaybeWeapon;
            bool primaryIsTwoHanded = primary != null && primary.Blueprint != null &&
                primary.Blueprint.IsTwoHanded;
            if (!primaryIsTwoHanded)
                AddCandidate(candidates, protector, attacker,
                    protector.Body.SecondaryHand, "secondary", order++);
            else order++;

            if (protector.Body.AdditionalLimbs != null)
            {
                for (int index = 0;
                    index < protector.Body.AdditionalLimbs.Count; index++)
                    AddCandidate(candidates, protector, attacker,
                        protector.Body.AdditionalLimbs[index],
                        "additional-" + index, order++);
            }

            BodyguardAttackCandidate<BodyguardSelectedAttack> selected =
                BodyguardAttackSelectionPolicy.Select(candidates);
            return selected == null ? null : selected.Attack;
        }

        private static void AddCandidate(List<BodyguardAttackCandidate<
            BodyguardSelectedAttack>> candidates, UnitEntityData protector,
            UnitEntityData attacker, WeaponSlot slot, string role, int order)
        {
            if (!IsQualifyingMelee(slot, protector)) return;
            ItemEntityWeapon weapon = slot.Weapon;
            bool reaches;
            try
            { reaches = UnitEngagementExtension.IsReach(protector, attacker, slot); }
            catch { reaches = false; }
            if (!reaches) return;

            int bonus;
            BodyguardSyntheticAidContext.EnterCalculation();
            try
            {
                var rule = new RuleCalculateAttackBonus(protector, attacker,
                    weapon, 0);
                Rulebook.Trigger(rule);
                bonus = rule.Result;
            }
            finally
            { BodyguardSyntheticAidContext.ExitCalculation(); }

            string guid = weapon.Blueprint == null ? "<no-blueprint>" :
                weapon.Blueprint.AssetGuid;
            string identity = role + ":" + guid;
            var selected = new BodyguardSelectedAttack(slot, weapon, identity,
                bonus, order);
            candidates.Add(new BodyguardAttackCandidate<BodyguardSelectedAttack>(
                selected, identity, bonus, order, true));
        }

        private static bool IsQualifyingMelee(WeaponSlot slot,
            UnitEntityData protector)
        {
            if (slot == null || !slot.HasWeapon || slot.Weapon == null ||
                slot.Weapon.Blueprint == null || !slot.Weapon.Blueprint.IsMelee)
                return false;
            if (!slot.Weapon.Blueprint.IsUnarmed) return true;
            return protector.Descriptor.State.Features != null &&
                protector.Descriptor.State.Features.ImprovedUnarmedStrike;
        }
    }

    internal static class BodyguardSyntheticAidContext
    {
        [ThreadStatic]
        private static int _depth;

        internal static bool IsActive { get { return _depth > 0; } }

        internal static void EnterCalculation() { _depth++; }

        internal static void ExitCalculation()
        { if (_depth > 0) _depth--; }
    }
}
