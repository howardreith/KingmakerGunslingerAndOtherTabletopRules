using System;
using System.Runtime.CompilerServices;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using TurnBased.Controllers;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// A contact action for the native overrun delivery. The delivery moves
    /// the unit and invokes its Actions for every contacted creature; this
    /// action replaces the native CMB result with the printed trample rule.
    /// It is installed only on hidden Sprint 11 summons until the movement,
    /// AoO choice and round cadence have been qualified in both combat modes.
    /// </summary>
    [Serializable]
    public sealed class ContextActionUngulateTrample : ContextAction
    {
        private static readonly ConditionalWeakTable<UnitEntityData,
            TrampleRoundLedger> Ledgers = new ConditionalWeakTable<
                UnitEntityData, TrampleRoundLedger>();

        public BlueprintUnit SourceUnit;
        public string CreatureKey;

        public override string GetCaption()
        { return "Apply printed ungulate trample at one native path contact"; }

        public override void RunAction()
        {
            UnitEntityData caster = Context == null ? null :
                Context.MaybeCaster;
            UnitEntityData target = Target == null ? null : Target.Unit;
            if (caster == null || target == null || SourceUnit == null ||
                !ReferenceEquals(caster.Blueprint, SourceUnit) ||
                caster.Descriptor == null || target.Descriptor == null ||
                caster.Destroyed || target.Destroyed ||
                target.Descriptor.State.IsDead ||
                ReferenceEquals(caster, target) || !target.IsEnemy(caster) ||
                string.IsNullOrEmpty(target.UniqueId)) return;

            UngulateRulesProfile rules = UngulateRulesPolicy.For(CreatureKey);
            // Stampede needs a coordinated, adjacent three-creature path.
            // Until that native group route is proved, never award its larger
            // target allowance or +2 DC from mere proximity.
            const int activeStampedeGroup = 0;
            if (!rules.CanTrample((int)caster.Descriptor.State.Size,
                    (int)target.Descriptor.State.Size,
                    activeStampedeGroup)) return;

            long round = CombatController.IsInTurnBasedCombat() &&
                Game.Instance.TurnBasedCombatController != null ?
                Game.Instance.TurnBasedCombatController.RoundNumber :
                Game.Instance.Player.GameTime.Ticks /
                    TimeSpan.FromSeconds(6d).Ticks;
            TrampleRoundLedger ledger = Ledgers.GetOrCreateValue(caster);
            if (!ledger.TryClaim(round, target.UniqueId)) return;

            var save = new RuleSavingThrow(target,
                SavingThrowType.Reflex, rules.TrampleSaveDc(
                    activeStampedeGroup));
            Context.TriggerRule(save);
            DiceType die = rules.TrampleDieSides == 6 ? DiceType.D6 :
                throw new InvalidOperationException(
                    "Unexpected ungulate trample die: " + CreatureKey);
            var damage = new PhysicalDamage(new DiceFormula(
                rules.TrampleDiceCount, die),
                PhysicalDamageForm.Bludgeoning);
            damage.AddBonus(rules.TrampleBonus);
            damage.Half = save.IsPassed;
            var dealt = new RuleDealDamage(caster, target, damage) {
                HalfBecauseSavingThrow = save.IsPassed
            };
            Context.TriggerRule(dealt);
        }
    }

    /// <summary>
    /// AbilityCustomOverrun with AutoSuccess accepts targets beyond its own
    /// speed range. The printed trample is a full-round path of at most twice
    /// the creature's current speed, checked before the native forced path.
    /// </summary>
    [Serializable]
    public sealed class UngulateTramplePathChecker : BlueprintComponent,
        IAbilityTargetChecker
    {
        public bool CanTarget(UnitEntityData caster, TargetWrapper target)
        {
            if (caster == null || target == null || caster.View == null ||
                caster.CombatSpeedMps <= 0f) return false;
            float distance = Vector3.Distance(caster.Position, target.Point);
            return distance > caster.View.Corpulence &&
                distance <= caster.CombatSpeedMps * 12f;
        }
    }
}
