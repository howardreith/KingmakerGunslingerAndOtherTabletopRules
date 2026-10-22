using System;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Brain;
using Kingmaker.Controllers.Brain.Blueprints.Considerations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>Native moving-area exposure. The victim's separate native buff
    /// instances retain the exact emitter and native 24-hour expiration across
    /// departure/re-entry and saves. This is neither fear nor mind-affecting.</summary>
    [Serializable]
    public sealed class ContextActionArchonMenace : ContextAction
    {
        public BlueprintUnit[] Owners;
        public BlueprintBuff Penalty;
        public BlueprintBuff Immunity;
        public override string GetCaption() { return "Source-specific archon menace exposure"; }
        public override void RunAction()
        {
            var source = Context?.MaybeCaster;
            var victim = Target?.Unit;
            if (source == null || victim == null || source.Destroyed ||
                source.Descriptor.State.IsDead || victim.Destroyed || victim.Descriptor.State.IsDead ||
                Owners == null || !Owners.Any(owner => ReferenceEquals(owner, source.Blueprint)) ||
                !source.IsEnemy(victim) || Penalty == null || Immunity == null ||
                HasSource(victim, Penalty, source) || HasSource(victim, Immunity, source)) return;
            int hd = source.Descriptor.Progression.CharacterLevel;
            var context = new MechanicsContext(source, victim.Descriptor, Penalty,
                Context, new TargetWrapper(victim));
            context.Params.CasterLevel = hd;
            context.Params.DC = 10 + hd / 2 + source.Descriptor.Stats.Charisma.Bonus + 2;
            var save = new RuleSavingThrow(victim, SavingThrowType.Will, context.Params.DC) { Reason = context };
            context.TriggerRule(save);
            var buff = victim.Buffs.AddBuff(save.IsPassed ? Immunity : Penalty,
                context, TimeSpan.FromHours(24));
            if (buff != null) buff.IsNotDispelable = true;
        }
        internal static bool HasSource(UnitEntityData victim, BlueprintBuff blueprint, UnitEntityData source)
        {
            return victim.Buffs.Enumerable.Any(buff => ReferenceEquals(buff.Blueprint, blueprint) &&
                ReferenceEquals(buff.Context?.MaybeCaster, source));
        }
    }

    /// <summary>One native penalty for this named magical effect, with
    /// independent source buff instances. This follows the established Nereid
    /// overlap/hand-off pattern, without suppressing unrelated penalties.</summary>
    [Serializable]
    public sealed class ArchonMenacePenalty : BuffLogic
    {
        [NonSerialized] private bool _present;
        [NonSerialized] private ModifiableValue.Modifier[] _modifiers;
        public override void OnTurnOn()
        {
            _present = true;
            ApplyIfAbsent();
        }
        private void ApplyIfAbsent()
        {
            var stats = Owner.Stats;
            var values = new ModifiableValue[] { stats.AdditionalAttackBonus, stats.AC,
                stats.SaveFortitude, stats.SaveReflex, stats.SaveWill };
            _modifiers = values.Where(stat => !stat.Modifiers.Any(modifier =>
                modifier.Source != null && ReferenceEquals(modifier.Source.Blueprint, Buff.Blueprint)))
                .Select(stat => stat.AddModifier(-2, this, Kingmaker.Enums.ModifierDescriptor.Penalty)).ToArray();
        }
        public override void OnTurnOff()
        {
            _present = false;
            if (_modifiers == null || _modifiers.Length == 0) return;
            foreach (var modifier in _modifiers) modifier.Remove();
            _modifiers = null;
            var next = Owner.Buffs.Enumerable.Where(buff => ReferenceEquals(buff.Blueprint, Buff.Blueprint))
                .SelectMany(buff => buff.SelectComponents<ArchonMenacePenalty>())
                .FirstOrDefault(component => !ReferenceEquals(component, this) && component._present);
            if (next != null) next.ApplyIfAbsent();
        }
    }

    [Serializable]
    public sealed class ArchonMenaceHitBreak : BuffLogic,
        IInitiatorRulebookHandler<RuleAttackRoll>
    {
        public BlueprintBuff Immunity;
        public void OnEventAboutToTrigger(RuleAttackRoll evt) { }
        public void OnEventDidTrigger(RuleAttackRoll evt)
        {
            var source = Buff?.Context?.MaybeCaster;
            if (source == null || evt == null || !evt.IsHit ||
                !ReferenceEquals(evt.Initiator, Owner?.Unit) || !ReferenceEquals(evt.Target, source) ||
                Immunity == null) return;
            // Capture the original context before removing this one contribution.
            var context = Buff.Context;
            Buff.Remove();
            if (!ContextActionArchonMenace.HasSource(evt.Initiator, Immunity, source))
            {
                var marker = evt.Initiator.Buffs.AddBuff(Immunity, context, TimeSpan.FromHours(24));
                if (marker != null) marker.IsNotDispelable = true;
            }
        }
    }

    /// <summary>Only the disappearing emitter's grants end. No victim areas,
    /// spell effects or another archon's state are removed.</summary>
    [Serializable]
    public sealed class ArchonMenaceEmitterCleanup : BuffLogic
    {
        public BlueprintBuff Penalty;
        public BlueprintBuff Immunity;
        public override void OnFactDeactivate()
        {
            var source = Owner?.Unit;
            if (source == null || Game.Instance?.State?.Units == null) return;
            foreach (var victim in Game.Instance.State.Units.All.ToArray())
            {
                if (victim == null) continue;
                foreach (var buff in victim.Buffs.Enumerable.Where(buff =>
                    (ReferenceEquals(buff.Blueprint, Penalty) || ReferenceEquals(buff.Blueprint, Immunity)) &&
                    ReferenceEquals(buff.Context?.MaybeCaster, source)).ToArray()) buff.Remove();
            }
        }
    }

    /// <summary>Bounded Aid selection, with the native effect and costs intact.
    /// Nonenemy includes the player faction that native summon IsAlly misses.</summary>
    public sealed class ArchonAidEligibility : Consideration
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintAbility Ability;
        public BlueprintBuff AidBuff;
        public override float Score(DecisionContext context)
        {
            var actor = context?.Unit;
            var target = context?.Target?.Unit;
            if (actor == null || target == null || !ReferenceEquals(actor.Blueprint, OwningBlueprint) ||
                Ability == null || AidBuff == null || actor.IsEnemy(target) || target.Destroyed ||
                target.Descriptor.State.IsDead || target.Buffs.GetBuff(AidBuff) != null) return 0f;
            var granted = actor.Descriptor.Abilities.GetAbility(Ability);
            if (granted == null) return 0f;
            var data = new Kingmaker.UnitLogic.Abilities.AbilityData(granted);
            return data.IsAvailable && data.CanTarget(new TargetWrapper(target)) ? 1f : 0f;
        }
    }
}
