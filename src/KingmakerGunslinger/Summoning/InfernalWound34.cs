using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers.Units;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;

namespace KingmakerGunslinger.Summoning
{
    [Serializable]
    public sealed class BarbazuWoundingGlaive34 : RuleInitiatorLogicComponent<RuleAttackWithWeapon>
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintItemWeapon Glaive;
        public BlueprintBuff Wound;
        private static readonly ConditionalWeakTable<RuleAttackWithWeapon, object> Seen =
            new ConditionalWeakTable<RuleAttackWithWeapon, object>();
        public override void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }
        public override void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
            var source = Owner?.Unit; var target = evt?.Target;
            if (source == null || InfernalMartialProfile34.For(source) == null || OwningBlueprint == null || Glaive == null || Wound == null ||
                !ReferenceEquals(source.Blueprint, OwningBlueprint) ||
                !ReferenceEquals(evt.Initiator, source) ||
                !ReferenceEquals(evt.Weapon?.Blueprint, Glaive) ||
                !SummonInjuryDiseasePolicy.ShouldResolve(true, evt.AttackRoll?.IsHit == true,
                    Math.Max(0, evt.MeleeDamage?.Damage ?? 0), SummonDiseaseExposure.IsAvailable(target), false)) return;
            object previous;
            if (Seen.TryGetValue(evt, out previous)) return;
            Seen.Add(evt, new object());
            // One marker for each source, regardless of the number of hits.
            if (target.Buffs.Enumerable.Any(buff => ReferenceEquals(buff.Blueprint, Wound) &&
                ReferenceEquals(buff.Context?.MaybeCaster, source))) return;
            var context = AstralVictimEffectContext.Create(source, target, Wound,
                new AbilityParams { DC = 17, CasterLevel = 6, SpellLevel = 0 });
            var apply = new RuleApplyBuff(target, Wound, context, null,
                (blueprint, parent, duration) => target.Buffs.AddBuff(blueprint, parent, duration));
            Rulebook.Trigger(apply);
        }
    }

    // Multiple source markers retain independent lifetime ownership, but only
    // the first live marker runs the native bleed actions on each victim round.
    [Serializable]
    public sealed class InfernalWound34 : BuffLogic, ITickEachRound,
        ITargetRulebookHandler<RuleHealDamage>
    {
        public ActionList BleedActions;
        internal const string CotwBleedType = "CallOfTheWild.BleedMechanics.BleedBuff";
        public override void OnFactDeactivate()
        {
            base.OnFactDeactivate();
            if (Owner?.Unit == null) return;
            // The installed selector clears only the selected fact. Restore
            // its own strongest-bleed choice from the remaining live facts.
            foreach (var other in Owner.Unit.Buffs.Enumerable.ToArray()) {
                if (ReferenceEquals(other, Buff) || !other.IsTurnedOn) continue;
                foreach (var component in other.SelectComponents<BuffLogic>()) {
                    if (component.GetType().FullName != CotwBleedType) continue;
                    component.OnFactActivate();
                }
            }
        }
        internal sealed class HealingDecision
        {
            internal bool Spell;
            internal bool Passed;
            internal int CasterLevel;
            internal int Roll;
        }
        private static readonly ConditionalWeakTable<RuleHealDamage, HealingDecision> Healing =
            new ConditionalWeakTable<RuleHealDamage, HealingDecision>();

        private static Buff[] Wounds(UnitEntityData target)
        {
            return target.Buffs.Enumerable.Where(buff => buff.IsTurnedOn &&
                buff.SelectComponents<InfernalWound34>().Any() &&
                InfernalEffectLifetime34.SourceIsLive(buff.Context?.MaybeCaster)).ToArray();
        }

        public void OnNewRound()
        {
            if (Owner?.Unit == null || BleedActions == null) return;
            if (!ReferenceEquals(Wounds(Owner.Unit).FirstOrDefault(), Buff)) return;
            Buff.RunActionInContext(BleedActions, null);
        }

        public void OnEventAboutToTrigger(RuleHealDamage evt)
        {
            if (evt == null || !ReferenceEquals(evt.Target, Owner?.Unit) ||
                !InfernalEffectLifetime34.SourceIsLive(Buff?.Context?.MaybeCaster)) return;
            HealingDecision decision;
            if (!Healing.TryGetValue(evt, out decision)) {
                var context = evt.Reason?.Context;
                var ability = evt.Reason?.Ability;
                var blueprint = ability?.Blueprint ?? context?.SourceAbility;
                decision = new HealingDecision { Spell = blueprint?.Type == AbilityType.Spell ||
                    blueprint?.Type == AbilityType.SpellLike };
                // Claim before triggering a nested native d20 rule. Every owned
                // source on this patient observes the same spell check.
                Healing.Add(evt, decision);
                if (decision.Spell) {
                    decision.CasterLevel = context?.Params?.CasterLevel ??
                        (ability == null ? 0 : ability.CalculateParams().CasterLevel);
                    var roll = new RuleRollD20(evt.Initiator);
                    roll.Reason = evt.Reason;
                    Rulebook.Trigger(roll);
                    decision.Roll = roll.Result;
                    decision.Passed = decision.Roll + decision.CasterLevel >= InfernalRules34.WoundSpellCureDc;
                }
            }
            if (decision.Spell && !decision.Passed) evt.Modifier = 0f;
        }

        public void OnEventDidTrigger(RuleHealDamage evt)
        {
            HealingDecision decision;
            if (evt == null || !ReferenceEquals(evt.Target, Owner?.Unit) ||
                !Healing.TryGetValue(evt, out decision) || !decision.Spell || !decision.Passed) return;
            // This is a successful printed cure, not source cleanup. It closes
            // every active Infernal Wound on this patient, including siblings.
            foreach (var wound in Wounds(evt.Target)) wound.Remove();
        }

        internal static bool Rejected(RuleHealDamage evt)
        {
            HealingDecision decision;
            return evt != null && Healing.TryGetValue(evt, out decision) &&
                decision.Spell && !decision.Passed;
        }
        internal static HealingDecision Observe(RuleHealDamage evt)
        {
            HealingDecision decision;
            return evt != null && Healing.TryGetValue(evt, out decision) ? decision : null;
        }
    }

    // The installed consumer removes its buff even when Value is zero. Only a
    // spell event explicitly rejected by a live S34 wound skips that removal.
    // Ordinary healing, other patients and module-off sessions stay native.
    [HarmonyPatch(typeof(RemovedByHeal), "OnEventDidTrigger")]
    internal static class InfernalRejectedHealingClosure34
    {
        private static bool Prefix(RuleHealDamage __0)
        { return !InfernalWound34.Rejected(__0); }
    }
}
