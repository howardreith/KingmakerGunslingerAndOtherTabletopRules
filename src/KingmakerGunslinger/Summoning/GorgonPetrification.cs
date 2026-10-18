using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Controllers.Units;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.Utility;
using Newtonsoft.Json;

namespace KingmakerGunslinger.Summoning
{
    // Primitive fields match native Fact.PostLoad's direct field assignment.
    // No source object, delegate, dictionary or custom record is serialized.
    [Serializable]
    public sealed class GorgonPetrificationState : BuffLogic, ITickEachRound
    {
        [JsonProperty] private bool _initialized;
        [JsonProperty] private bool _permanent;
        [JsonProperty] private int _recoveryDc;
        [JsonProperty] private string _firstSourceId;
        [JsonProperty] private string _lastSourceId;
        [JsonProperty] private string _firstActivation;
        [JsonProperty] private string _lastActivation;

        internal bool Initialized { get { return _initialized; } }
        internal bool Permanent { get { return _permanent; } }
        internal int RecoveryDc { get { return _recoveryDc; } }
        internal string FirstSourceId { get { return _firstSourceId; } }
        internal string LastSourceId { get { return _lastSourceId; } }
        internal string LastActivation { get { return _lastActivation; } }

        internal void Initialize(int dc, string sourceId, string activation)
        {
            if (_initialized || string.IsNullOrEmpty(sourceId) || string.IsNullOrEmpty(activation))
                throw new InvalidOperationException("Gorgon victim state cannot be refreshed or lose attribution.");
            _recoveryDc = dc;
            _firstSourceId = _lastSourceId = sourceId;
            _firstActivation = _lastActivation = activation;
            _permanent = false;
            _initialized = true;
        }

        internal bool Escalate(string sourceId, string activation)
        {
            if (string.IsNullOrEmpty(sourceId) || !Sprint26MagicalBeastRules.CanEscalate(
                _initialized, _permanent, _firstActivation, _lastActivation, activation)) return false;
            _lastSourceId = sourceId;
            _lastActivation = activation;
            _permanent = true;
            return true;
        }

        public void OnNewRound()
        {
            if (!_initialized || _permanent) return;
            if (Buff == null || Buff.Context == null || Buff.Context.Params == null)
                throw new InvalidOperationException("Gorgon victim lost its native recovery context.");
            // Delegate the actual round save and removal to the inspected native
            // condition carrier. Its ordinary automatic tick remains disabled.
            var condition = Buff.SelectComponents<BuffStatusCondition>().Single();
            Buff.Context.Params.DC = _recoveryDc;
            condition.SaveType = SavingThrowType.Fortitude;
            condition.SaveEachRound = true;
            try { condition.OnNewRound(); }
            finally { condition.SaveEachRound = false; }
        }
    }

    internal sealed class GorgonBreathActivation
    {
        internal readonly string Id = Guid.NewGuid().ToString("N");
        internal readonly HashSet<string> Targets = new HashSet<string>(StringComparer.Ordinal);
        internal string SourceId;
        internal int Dc;
        internal int CooldownRounds;
        internal WeakReference<AbilityExecutionContext> Context;
    }

    internal static class GorgonBreathRuntime
    {
        private static readonly ConditionalWeakTable<AbilityExecutionContext,
            GorgonBreathActivation> Activations = new ConditionalWeakTable<
                AbilityExecutionContext, GorgonBreathActivation>();

        private static readonly ConditionalWeakTable<UnitEntityData, GorgonBreathActivation> Last =
            new ConditionalWeakTable<UnitEntityData, GorgonBreathActivation>();
        internal static GorgonBreathActivation Latest(UnitEntityData owner)
        { GorgonBreathActivation value; return owner != null && Last.TryGetValue(owner, out value) ? value : null; }

        internal static GorgonBreathActivation Begin(AbilityExecutionContext context,
            BlueprintUnit ownerBlueprint, BlueprintCharacterClass hitDieClass, BlueprintBuff cooldown)
        {
            if (context == null || context.Caster == null || ownerBlueprint == null ||
                !ReferenceEquals(context.Caster.Blueprint, ownerBlueprint) || hitDieClass == null || cooldown == null)
                throw new InvalidOperationException("Gorgon breath requires its exact owning creature and carriers.");
            GorgonBreathActivation existing;
            if (Activations.TryGetValue(context, out existing)) return existing;
            var owner = context.Caster;
            var activation = new GorgonBreathActivation {
                SourceId = owner.UniqueId,
                Dc = Sprint26MagicalBeastRules.BreathDc(
                    owner.Descriptor.Progression.GetClassLevel(hitDieClass), owner.Stats.Constitution.Bonus)
            };
            activation.Context = new WeakReference<AbilityExecutionContext>(context);
            Activations.Add(context, activation);
            Last.Remove(owner);
            Last.Add(owner, activation);
            context.Params.DC = activation.Dc;
            var roll = Rulebook.Trigger(new RuleRollDice(owner,
                new DiceFormula(1, DiceType.D4)) { Reason = context });
            activation.CooldownRounds = Sprint26MagicalBeastRules.CooldownRounds(roll.Result);
            // An internal clock must not inherit the offensive Petrified descriptor
            // from the cone's context and be defeated by its owner's immunity.
            if (owner.Descriptor.AddBuff(cooldown, owner,
                TimeSpan.FromSeconds(activation.CooldownRounds * 6d)) == null)
                throw new InvalidOperationException("Gorgon cooldown could not attach.");
            return activation;
        }

        internal static GorgonBreathActivation Require(AbilityExecutionContext context)
        {
            GorgonBreathActivation activation;
            if (context == null || !Activations.TryGetValue(context, out activation))
                throw new InvalidOperationException("Gorgon delivery has no distinct breath activation.");
            return activation;
        }

        internal static string Apply(AbilityExecutionContext context,
            UnitEntityData victim, BlueprintBuff stone)
        {
            var activation = Require(context);
            if (victim == null || victim.Destroyed || stone == null || string.IsNullOrEmpty(victim.UniqueId)) return "ineligible";
            if (!activation.Targets.Add(victim.UniqueId)) return "duplicate";
            bool petrified = victim.Descriptor.State.HasCondition(UnitCondition.Petrified);
            if (victim.Descriptor.State.IsDead && !petrified) return "dead";
            if (victim.Descriptor.State.HasConditionImmunity(UnitCondition.Petrified)) return "immune";
            Buff own = victim.Buffs.GetBuff(stone);
            if (own != null && petrified)
            {
                var state = own.SelectComponents<GorgonPetrificationState>().Single();
                if (state.Permanent) return "already-permanent";
                bool escalated = false;
                // This is the actual second application attempt. Native descriptor
                // and story immunity run before the callback; the existing victim
                // buff retains its context, tick time and initial source/DC.
                var exposure = new RuleApplyBuff(victim, stone, context, null,
                    (blueprint, source, duration) => {
                        escalated = state.Escalate(activation.SourceId, activation.Id);
                        return own;
                    }) { Reason = context };
                context.TriggerRule(exposure);
                return escalated ? "permanent" : "immune-or-duplicate";
            }
            // A marker with its condition removed externally is not a petrified
            // victim. An unrelated native stone effect never supplies this marker.
            if (own != null) own.Remove();
            var saving = new RuleSavingThrow(victim, SavingThrowType.Fortitude, activation.Dc) { Reason = context };
            context.TriggerRule(saving);
            if (saving.IsPassed) return "saved";
            // BuffCollection.AddBuff owns the single native RuleApplyBuff event.
            // Do not wrap it in another application event.
            Buff applied = victim.Buffs.AddBuff(stone, context, null);
            if (applied == null) return "immune";
            if (!victim.Descriptor.State.HasCondition(UnitCondition.Petrified))
            {
                applied.Remove();
                return "immune";
            }
            applied.SelectComponents<GorgonPetrificationState>().Single().Initialize(
                activation.Dc, activation.SourceId, activation.Id);
            return "temporary";
        }
    }

    [Serializable]
    public sealed class GorgonBreathOnCast : BlueprintComponent, IAbilityOnCastLogic
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintCharacterClass HitDieClass;
        public BlueprintBuff Cooldown;
        public void OnCast(AbilityExecutionContext context)
        { GorgonBreathRuntime.Begin(context, OwningBlueprint, HitDieClass, Cooldown); }
    }

    [Serializable]
    public sealed class GorgonBreathEffect : AbilityApplyEffect
    {
        public BlueprintBuff Stone;
        public override void Apply(AbilityExecutionContext context, TargetWrapper target)
        { GorgonBreathRuntime.Apply(context, target == null ? null : target.Unit, Stone); }
    }

    [Serializable]
    public sealed class GorgonConeSafety : BlueprintComponent, IAbilityTargetChecker
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintAbility Breath;
        public bool CanTarget(UnitEntityData caster, TargetWrapper target)
        {
            if (caster == null || !ReferenceEquals(caster.Blueprint, OwningBlueprint) ||
                Breath == null || target == null || Game.Instance == null) return false;
            var delivery = Breath.GetComponent<AbilityDeliverProjectile>();
            if (delivery == null) return false;
            var data = new AbilityData(Breath, caster.Descriptor);
            foreach (var unit in Game.Instance.State.Units.All)
            {
                if (unit == null || unit.Destroyed || ReferenceEquals(unit, caster) || unit.IsEnemy(caster)) continue;
                if (unit.Descriptor.State.IsDead && !unit.Descriptor.State.HasCondition(UnitCondition.Petrified)) continue;
                if (delivery.WouldTargetUnit(data, target.Point, unit)) return false;
            }
            return true;
        }
    }
}
