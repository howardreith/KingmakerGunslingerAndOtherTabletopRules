using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;

namespace KingmakerGunslinger.ElvenBranchedSpear
{
    [Serializable]
    internal sealed class NamedSpearEffectComponent : WeaponEnchantmentLogic,
        IInitiatorRulebookHandler<RuleAttackWithWeapon>,
        IInitiatorRulebookHandler<RuleCalculateAttackBonusWithoutTarget>
    {
        public NamedSpearKind Kind;
        public BlueprintBuff EffectBuff;
        public BlueprintBuff RoundMarker;
        public BlueprintBuff SecondaryBuff;
        public BlueprintBuff EntangledBuff;

        public void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }

        public void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
            if (!IsExactAttack(evt) || evt.AttackRoll == null ||
                evt.Target == null) return;
            bool hit = evt.AttackRoll.IsHit;
            bool opportunity = evt.IsAttackOfOpportunity;
            bool movement = opportunity &&
                MovementOpportunityAttackTracker.IsRunning(evt.Initiator);
            bool generated = opportunity &&
                BriarGeneratedOpportunityAttackTracker.IsRunning(evt.Initiator);
            bool used = HasBuff(evt.Initiator, RoundMarker);
            int sneakDamage = AppliedSneakDamage(evt);

            switch (Kind)
            {
                case NamedSpearKind.Boughkeeper:
                    if (NamedSpearEffectPolicy.Boughkeeper(hit, opportunity))
                        Refresh(evt.Initiator, EffectBuff);
                    break;
                case NamedSpearKind.Thornstep:
                    if (NamedSpearEffectPolicy.Thornstep(hit, opportunity,
                            movement, used))
                    {
                        Mark(evt.Initiator);
                        Refresh(evt.Target, EffectBuff);
                    }
                    break;
                case NamedSpearKind.VipersReach:
                    if (NamedSpearEffectPolicy.VipersReach(
                            evt.AttackRoll.IsSneakAttackUsed, sneakDamage, used))
                    {
                        Mark(evt.Initiator);
                        Refresh(evt.Target, EffectBuff);
                    }
                    break;
                case NamedSpearKind.BriarCrownedSpear:
                    int remaining = RemainingOpportunities(evt.Initiator);
                    if (NamedSpearEffectPolicy.BriarCrowned(hit, opportunity,
                            generated, used, remaining))
                    {
                        Mark(evt.Initiator);
                        GenerateBriarAttack(evt.Initiator, evt.Target);
                    }
                    break;
                case NamedSpearKind.SpearOfTheFirstBranch:
                    if (NamedSpearEffectPolicy.FirstBranch(hit, opportunity,
                            evt.AttackRoll.IsSneakAttackUsed, sneakDamage, used,
                            generated))
                    {
                        Mark(evt.Initiator);
                        ResolveFirstBranch(evt.Initiator, evt.Target);
                    }
                    break;
            }
        }

        public void OnEventAboutToTrigger(
            RuleCalculateAttackBonusWithoutTarget evt)
        {
            if (Kind == NamedSpearKind.BriarCrownedSpear && evt != null &&
                ReferenceEquals(evt.Weapon, Owner) && evt.Initiator != null &&
                BriarGeneratedOpportunityAttackTracker.IsRunning(evt.Initiator))
                evt.AddBonus(-5, Fact);
        }

        public void OnEventDidTrigger(
            RuleCalculateAttackBonusWithoutTarget evt) { }

        private bool IsExactAttack(RuleAttackWithWeapon evt)
        {
            return evt != null && evt.Initiator != null && evt.Weapon != null &&
                Owner != null && ReferenceEquals(evt.Weapon, Owner);
        }

        private static int AppliedSneakDamage(RuleAttackWithWeapon evt)
        {
            if (evt == null || evt.AttackRoll == null ||
                !evt.AttackRoll.IsSneakAttackUsed ||
                evt.AttackRoll.FortificationNegatesSneakAttack ||
                evt.AttackRoll.ImmuneToSneakAttack || evt.MeleeDamage == null ||
                evt.MeleeDamage.ResultDamage == null) return 0;
            return evt.MeleeDamage.ResultDamage.Where(value =>
                value.Source != null && value.Source.Sneak &&
                !value.Source.Immune && value.FinalValue > 0)
                .Sum(value => value.FinalValue);
        }

        private static bool HasBuff(UnitEntityData unit, BlueprintBuff buff)
        {
            return unit != null && unit.Descriptor != null && buff != null &&
                unit.Descriptor.Buffs.GetBuff(buff) != null;
        }

        private void Mark(UnitEntityData unit)
        { Refresh(unit, RoundMarker); }

        private void Refresh(UnitEntityData unit, BlueprintBuff buff)
        {
            if (unit == null || unit.Descriptor == null || buff == null) return;
            Buff existing = unit.Descriptor.Buffs.GetBuff(buff);
            if (existing != null) unit.Descriptor.Buffs.RemoveFact(existing);
            unit.Descriptor.Buffs.AddBuff(buff, Context, TimeSpan.FromSeconds(6d));
        }

        private static int RemainingOpportunities(UnitEntityData unit)
        {
            if (unit == null || unit.CombatState == null ||
                !unit.CombatState.CanAttackOfOpportunity) return 0;
            return Math.Max(0, unit.CombatState.AttackOfOpportunityCount);
        }

        private static void GenerateBriarAttack(UnitEntityData attacker,
            UnitEntityData target)
        {
            if (attacker == null || attacker.CombatState == null || target == null)
                return;
            attacker.CombatState.AttackOfOpportunity(target, false);
        }

        private void ResolveFirstBranch(UnitEntityData attacker,
            UnitEntityData target)
        {
            if (attacker == null || attacker.Descriptor == null ||
                target == null || target.Descriptor == null) return;
            int level = Math.Max(1, attacker.Descriptor.Progression.CharacterLevel);
            int dexterity = attacker.Descriptor.Stats.Dexterity.Bonus;
            int dc = NamedSpearEffectPolicy.FirstBranchDifficultyClass(level,
                dexterity);
            var saving = new RuleSavingThrow(target, SavingThrowType.Fortitude, dc);
            Rulebook.Trigger(saving);
            if (saving.IsPassed)
                Refresh(target, SecondaryBuff);
            else
                target.Descriptor.Buffs.AddBuff(EntangledBuff, Context,
                    TimeSpan.FromSeconds(6d));
            NamedSpearCombatLog.Publish(target.CharacterName, dc,
                saving.RollResult, saving.IsPassed);
        }
    }

    [Serializable]
    internal sealed class BoughkeeperArmorClassBonus :
        OwnedGameLogicComponent<UnitDescriptor>, IUnitEquipmentHandler,
        IUnitActiveEquipmentSetHandler, IUnitSubscriber
    {
        public BlueprintItemWeapon Boughkeeper;
        private ModifiableValue.Modifier _modifier;

        public override void OnTurnOn() { Refresh(); }
        public override void OnTurnOff() { Remove(); }

        public void HandleEquipmentSlotUpdated(ItemSlot slot,
            ItemEntity previousItem)
        { Refresh(); }

        public void HandleUnitChangeActiveEquipmentSet(UnitDescriptor unit)
        { if (ReferenceEquals(unit, Owner)) Refresh(); }

        private void Refresh()
        {
            Remove();
            if (Owner == null || Owner.Body == null || Owner.Stats == null ||
                Boughkeeper == null) return;
            ItemEntityWeapon primary = Owner.Body.PrimaryHand == null ? null :
                Owner.Body.PrimaryHand.MaybeWeapon;
            ItemEntityWeapon secondary = Owner.Body.SecondaryHand == null ? null :
                Owner.Body.SecondaryHand.MaybeWeapon;
            if ((primary != null && ReferenceEquals(primary.Blueprint, Boughkeeper)) ||
                (secondary != null && ReferenceEquals(secondary.Blueprint,
                    Boughkeeper)))
                _modifier = Owner.Stats.AC.AddModifier(1, Fact,
                    GetType().FullName, ModifierDescriptor.Dodge);
        }

        private void Remove()
        {
            if (_modifier != null && Owner != null && Owner.Stats != null)
                Owner.Stats.AC.RemoveModifier(_modifier);
            _modifier = null;
        }
    }

    internal static class BriarGeneratedOpportunityAttackTracker
    {
        private sealed class Marker { }
        private static readonly ConditionalWeakTable<UnitAttackOfOpportunity, Marker>
            Commands = new ConditionalWeakTable<UnitAttackOfOpportunity, Marker>();

        internal static bool IsRunning(UnitEntityData attacker)
        {
            if (attacker == null || attacker.Commands == null) return false;
            foreach (Kingmaker.UnitLogic.Commands.Base.UnitCommand command in
                attacker.GetAllCommands())
            {
                UnitAttackOfOpportunity opportunity =
                    command as UnitAttackOfOpportunity;
                Marker marker;
                if (opportunity != null && opportunity.IsRunning &&
                    Commands.TryGetValue(opportunity, out marker)) return true;
            }
            return false;
        }

        internal static void Mark(UnitAttackOfOpportunity command)
        {
            if (command == null || !IsGeneratedBoundary(new StackTrace(1, false)))
                return;
            Commands.Remove(command);
            Commands.Add(command, new Marker());
        }

        internal static bool IsGeneratedBoundary(StackTrace trace)
        {
            if (trace == null) return false;
            bool sawFactory = false;
            foreach (StackFrame frame in trace.GetFrames() ??
                Array.Empty<StackFrame>())
            {
                MethodBase method = frame == null ? null : frame.GetMethod();
                if (method == null) continue;
                if (method.DeclaringType == typeof(Kingmaker.Controllers.Combat
                        .UnitCombatState) && string.Equals(method.Name,
                        "AttackOfOpportunity", StringComparison.Ordinal))
                    sawFactory = true;
                else if (sawFactory && method.DeclaringType ==
                        typeof(NamedSpearEffectComponent) && string.Equals(
                        method.Name, "GenerateBriarAttack",
                        StringComparison.Ordinal))
                    return true;
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(UnitAttackOfOpportunity), MethodType.Constructor,
        new[] { typeof(UnitEntityData) })]
    internal static class BriarOpportunityCommandConstructionPatch
    {
        private static void Postfix(UnitAttackOfOpportunity __instance)
        { BriarGeneratedOpportunityAttackTracker.Mark(__instance); }
    }

    internal static class NamedSpearCombatLog
    {
        internal static bool Publish(string target, int dc, int roll, bool passed)
        {
            string message = string.Format(CultureInfo.InvariantCulture,
                "First Branch's Reprisal: {0} rolls Fortitude {1} against DC {2} and {3} ({4}).",
                string.IsNullOrWhiteSpace(target) ? "target" : target, roll, dc,
                passed ? "succeeds" : "fails",
                passed ? "speed reduced for 1 round" :
                    "Entangled for 1 round");
            try
            {
                EventBus.RaiseEvent<IWarningNotificationUIHandler>(
                    handler => handler.HandleWarning(message, false));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
