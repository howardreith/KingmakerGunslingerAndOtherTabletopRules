using System;
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
using KingmakerGunslinger.Diagnostics;

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
            NamedSpearEffectDiagnostics.RecordEvaluation(Kind, hit,
                opportunity, movement, generated, sneakDamage);

            switch (Kind)
            {
                case NamedSpearKind.Boughkeeper:
                    if (NamedSpearEffectPolicy.Boughkeeper(hit, opportunity))
                    {
                        NamedSpearEffectDiagnostics.RecordApplication(Kind);
                        Refresh(evt.Initiator, EffectBuff);
                    }
                    break;
                case NamedSpearKind.Thornstep:
                    if (NamedSpearEffectPolicy.Thornstep(hit, opportunity,
                            movement, used))
                    {
                        NamedSpearEffectDiagnostics.RecordApplication(Kind);
                        Mark(evt.Initiator);
                        Refresh(evt.Target, EffectBuff);
                    }
                    break;
                case NamedSpearKind.VipersReach:
                    if (NamedSpearEffectPolicy.VipersReach(
                            evt.AttackRoll.IsSneakAttackUsed, sneakDamage, used))
                    {
                        NamedSpearEffectDiagnostics.RecordApplication(Kind);
                        Mark(evt.Initiator);
                        Refresh(evt.Target, EffectBuff);
                    }
                    break;
                case NamedSpearKind.BriarCrownedSpear:
                    int remaining = RemainingOpportunities(evt.Initiator);
                    if (NamedSpearEffectPolicy.BriarCrowned(hit, opportunity,
                            generated, used, remaining))
                    {
                        NamedSpearEffectDiagnostics.RecordApplication(Kind);
                        Mark(evt.Initiator);
                        GenerateBriarAttack(evt.Initiator, evt.Target);
                    }
                    break;
                case NamedSpearKind.SpearOfTheFirstBranch:
                    if (NamedSpearEffectPolicy.FirstBranch(hit, opportunity,
                            evt.AttackRoll.IsSneakAttackUsed, sneakDamage, used,
                            generated))
                    {
                        NamedSpearEffectDiagnostics.RecordApplication(Kind);
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
            {
                evt.AddBonus(-5, Fact);
                NamedSpearEffectDiagnostics.RecordBriarPenalty();
            }
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
            BriarGeneratedOpportunityAttackTracker.EnterGeneration();
            try
            {
                attacker.CombatState.AttackOfOpportunity(target, false);
            }
            finally
            {
                BriarGeneratedOpportunityAttackTracker.ExitGeneration();
            }
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
            NamedSpearEffectDiagnostics.RecordFirstBranchSave(dc,
                saving.RollResult, saving.IsPassed);
        }
    }

    internal static class NamedSpearEffectDiagnostics
    {
        private static readonly object Gate = new object();
        private static readonly int[] Evaluated = new int[6];
        private static readonly int[] Applied = new int[6];

        internal static int MovementEvaluations { get; private set; }
        internal static int GeneratedEvaluations { get; private set; }
        internal static int PositiveSneakDamage { get; private set; }
        internal static int BriarPenaltyApplications { get; private set; }
        internal static int FirstBranchSaves { get; private set; }
        internal static int LastFirstBranchDc { get; private set; }
        internal static int LastFirstBranchRoll { get; private set; }
        internal static bool LastFirstBranchPassed { get; private set; }

        internal static void Reset()
        {
            lock (Gate)
            {
                Array.Clear(Evaluated, 0, Evaluated.Length);
                Array.Clear(Applied, 0, Applied.Length);
                MovementEvaluations = 0;
                GeneratedEvaluations = 0;
                PositiveSneakDamage = 0;
                BriarPenaltyApplications = 0;
                FirstBranchSaves = 0;
                LastFirstBranchDc = 0;
                LastFirstBranchRoll = 0;
                LastFirstBranchPassed = false;
            }
        }

        internal static int EvaluationCount(NamedSpearKind kind)
        {
            lock (Gate) return Evaluated[(int)kind];
        }

        internal static int ApplicationCount(NamedSpearKind kind)
        {
            lock (Gate) return Applied[(int)kind];
        }

        internal static void RecordEvaluation(NamedSpearKind kind, bool hit,
            bool opportunity, bool movement, bool generated, int sneakDamage)
        {
            lock (Gate)
            {
                Evaluated[(int)kind]++;
                if (movement) MovementEvaluations++;
                if (generated) GeneratedEvaluations++;
                if (sneakDamage > 0) PositiveSneakDamage += sneakDamage;
            }
        }

        internal static void RecordApplication(NamedSpearKind kind)
        {
            lock (Gate) Applied[(int)kind]++;
        }

        internal static void RecordBriarPenalty()
        {
            lock (Gate) BriarPenaltyApplications++;
        }

        internal static void RecordFirstBranchSave(int dc, int roll,
            bool passed)
        {
            lock (Gate)
            {
                FirstBranchSaves++;
                LastFirstBranchDc = dc;
                LastFirstBranchRoll = roll;
                LastFirstBranchPassed = passed;
            }
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

        [ThreadStatic]
        private static UnitAttackOfOpportunity ActiveGeneratedAttack;
        [ThreadStatic]
        private static int GenerationDepth;

        internal static bool IsRunning(UnitEntityData attacker)
        {
            return attacker != null && ActiveGeneratedAttack != null &&
                ReferenceEquals(ActiveGeneratedAttack.Executor, attacker);
        }

        internal static bool EnterOpportunityAction(
            UnitAttackOfOpportunity opportunity)
        {
            Marker marker;
            bool generated = opportunity != null && Commands.TryGetValue(
                opportunity, out marker);
            if (generated) ActiveGeneratedAttack = opportunity;
            return generated;
        }

        internal static void ExitOpportunityAction(bool entered)
        {
            if (entered) ActiveGeneratedAttack = null;
        }

        internal static void EnterGeneration()
        {
            GenerationDepth++;
        }

        internal static void ExitGeneration()
        {
            if (GenerationDepth > 0) GenerationDepth--;
        }

        internal static void Mark(UnitAttackOfOpportunity command)
        {
            if (command == null || GenerationDepth <= 0) return;
            Commands.Remove(command);
            Commands.Add(command, new Marker());
        }
    }

    [HarmonyPatch(typeof(UnitAttackOfOpportunity), MethodType.Constructor,
        new[] { typeof(UnitEntityData) })]
    internal static class BriarOpportunityCommandConstructionPatch
    {
        private static void Postfix(UnitAttackOfOpportunity __instance)
        { BriarGeneratedOpportunityAttackTracker.Mark(__instance); }
    }

    [HarmonyPatch(typeof(UnitAttackOfOpportunity), "OnAction")]
    internal static class BriarOpportunityActionBoundaryPatch
    {
        private static void Prefix(UnitAttackOfOpportunity __instance,
            out bool __state)
        {
            __state = BriarGeneratedOpportunityAttackTracker
                .EnterOpportunityAction(__instance);
        }

        private static void Postfix(bool __state)
        {
            BriarGeneratedOpportunityAttackTracker.ExitOpportunityAction(
                __state);
        }
    }

    internal static class NamedSpearCombatLog
    {
        internal static bool Publish(string target, int dc, int roll, bool passed)
        {
            string message = string.Format(CultureInfo.InvariantCulture,
                "First Branch's Reprisal: {0} Fortitude {1} vs DC {2} - {3}; {4}.",
                string.IsNullOrWhiteSpace(target) ? "target" : target.Trim(),
                roll, dc, passed ? "success" : "failed",
                passed ? "speed reduced" : "Entangled");
            return NativeCombatLog.Publish("elven-branched-spear",
                "first-branch-log.failed", message,
                "First Branch's Reprisal resolved, but its native combat-log entry failed.");
        }
    }
}
