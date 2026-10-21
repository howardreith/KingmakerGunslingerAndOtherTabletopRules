using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>A private standard action for the two printed owners. Native
    /// CMB/CMD, damage resolution and forced movement remain the consumers.
    /// The feature does not change any campaign actor's Bull Rush or slam.</summary>
    [Serializable]
    public sealed class Sprint29AwesomeBlow : ContextAction
    {
        public BlueprintUnit Cloud;
        public BlueprintUnit Storm;
        public BlueprintFeature Incorporeal;
        public Kingmaker.ElementsSystem.ActionList Knockdown;

        public override string GetCaption()
        { return "Awesome Blow (owned Cloud/Storm giant)"; }

        public override void RunAction()
        {
            UnitEntityData caster = Context == null ? null :
                Context.MaybeCaster;
            UnitEntityData victim = Target == null ? null : Target.Unit;
            if (caster == null || victim == null ||
                (!ReferenceEquals(caster.Blueprint, Cloud) &&
                 !ReferenceEquals(caster.Blueprint, Storm)) ||
                caster.Descriptor == null || victim.Descriptor == null ||
                caster.Destroyed || victim.Destroyed ||
                victim.Descriptor.State.IsDead ||
                !caster.IsEnemy(victim) ||
                (int)victim.Descriptor.State.Size >=
                    (int)caster.Descriptor.State.Size ||
                (Incorporeal != null &&
                    victim.Descriptor.HasFact(Incorporeal)) ||
                victim.Descriptor.State.HasCondition(
                    UnitCondition.ImmuneToCombatManeuvers)) return;

            var maneuver = new RuleCombatManeuver(caster, victim,
                CombatManeuver.BullRush) {
                // This is an Awesome Blow check, not a Bull Rush use.
                // Both owners have Improved Bull Rush, whose native +2
                // would otherwise attach to this substitute rule type.
                AdditionalBonus = -2
            };
            Context.TriggerRule(maneuver);
            if (maneuver.AutoFailure || !maneuver.Success) return;

            var impact = new PhysicalDamage(
                new DiceFormula(2, DiceType.D6),
                PhysicalDamageForm.Bludgeoning);
            impact.AddBonus(caster.Stats.Strength.Bonus);
            Context.TriggerRule(new RuleDealDamage(caster, victim, impact));
            if (!victim.Descriptor.State.IsDead)
            {
                victim.Ensure<Kingmaker.UnitLogic.Parts.UnitPartForceMove>()
                    .Push(victim.Position - caster.Position,
                        new Feet(10).Meters, false);
                if (Knockdown != null) Knockdown.Run();
            }
        }
    }
}
