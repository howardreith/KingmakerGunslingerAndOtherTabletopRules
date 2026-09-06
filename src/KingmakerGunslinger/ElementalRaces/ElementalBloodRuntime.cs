using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums.Damage;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Newtonsoft.Json;

namespace KingmakerGunslinger.ElementalRaces
{
    /// <summary>Expenditure survives provider removal, reapplication and load.
    /// Native UnitPartsManager owns subscription and serialization lifetime.</summary>
    public sealed class UnitPartElementalBloodCapacity : UnitPart, IUnitRestHandler
    {
        [JsonProperty] private int _schemaVersion = ElementalBloodPolicy.SchemaVersion;
        [JsonProperty] private int _fireHealingReceived;
        [JsonProperty] private int _stoneHealingReceived;
        [JsonProperty] private int _stormHealingReceived;
        private bool _healing;

        internal int Spent(ElementalAlternateTraitId trait)
        {
            if (_schemaVersion != ElementalBloodPolicy.SchemaVersion) return -1;
            switch (trait)
            {
                case ElementalAlternateTraitId.FireInTheBlood: return _fireHealingReceived;
                case ElementalAlternateTraitId.StoneInTheBlood: return _stoneHealingReceived;
                case ElementalAlternateTraitId.StormInTheBlood: return _stormHealingReceived;
                default: return -1;
            }
        }

        internal int Remaining(ElementalAlternateTraitId trait)
        {
            return Owner == null ? 0 : ElementalBloodPolicy.Remaining(
                Owner.Progression.CharacterLevel, Spent(trait));
        }

        internal void Record(ElementalAlternateTraitId trait, int received)
        {
            int total = ElementalBloodPolicy.Spend(Owner.Progression.CharacterLevel,
                Spent(trait), received);
            switch (trait)
            {
                case ElementalAlternateTraitId.FireInTheBlood: _fireHealingReceived = total; break;
                case ElementalAlternateTraitId.StoneInTheBlood: _stoneHealingReceived = total; break;
                case ElementalAlternateTraitId.StormInTheBlood: _stormHealingReceived = total; break;
                default: throw new ArgumentOutOfRangeException("trait");
            }
        }

        internal bool BeginHealing(ElementalAlternateTraitId trait)
        {
            if (_healing || Remaining(trait) <= 0) return false;
            _healing = true;
            return true;
        }

        internal void EndHealing() { _healing = false; }

        public void HandleUnitRest(UnitEntityData unit)
        {
            if (Owner == null || !ReferenceEquals(Owner.Unit, unit)) return;
            _schemaVersion = ElementalBloodPolicy.SchemaVersion;
            _fireHealingReceived = _stoneHealingReceived = _stormHealingReceived = 0;
        }
    }

    [Serializable]
    public sealed class ElementalBloodDamageTrigger : RuleTargetLogicComponent<RuleDealDamage>
    {
        private static readonly ConditionalWeakTable<RuleDealDamage, object> Applied =
            new ConditionalWeakTable<RuleDealDamage, object>();
        public int Trait;
        public DamageEnergyType Energy;
        public BlueprintBuff HealingBuff;

        public override void OnTurnOn()
        {
            if (Owner != null) Owner.Ensure<UnitPartElementalBloodCapacity>();
        }

        // Native save/load and level-up reconstruction also turn providers off.
        // This callback is not evidence of permanent trait loss. The exact
        // buff listens for real marker removal and owns expiry/death cleanup.
        public override void OnTurnOff() { }

        public override void OnEventAboutToTrigger(RuleDealDamage evt) { }

        public override void OnEventDidTrigger(RuleDealDamage evt)
        {
            if (evt == null || Owner == null || HealingBuff == null ||
                !ReferenceEquals(evt.Target, Owner.Unit) || Owner.State.IsDead ||
                evt.ResultDamage == null) return;
            ElementalAlternateTraitId trait = (ElementalAlternateTraitId)Trait;
            if (!evt.ResultDamage.Any(value => ElementalBloodPolicy.Triggers(trait,
                value.Source is EnergyDamage &&
                    ((EnergyDamage)value.Source).EnergyType == Energy,
                value.ValueWithoutReduction, evt.IsFake))) return;
            UnitPartElementalBloodCapacity capacity = Owner.Ensure<UnitPartElementalBloodCapacity>();
            if (capacity.Remaining(trait) <= 0) return;
            object prior;
            if (Applied.TryGetValue(evt, out prior)) return;
            Applied.Add(evt, new object());
            Buff active = Owner.Buffs.Enumerable.FirstOrDefault(value =>
                ReferenceEquals(value.Blueprint, HealingBuff));
            TimeSpan end = Game.Instance.TimeController.GameTime + 1.Rounds().Seconds;
            if (active != null)
            {
                // Do not restart NextTickTime on each hit: frequent hits must
                // neither stack healing nor postpone the existing native tick.
                if (active.EndTime < end) active.EndTime = end;
                return;
            }
            var context = new MechanicsContext(Owner.Unit, Owner, Fact.Blueprint,
                null, new TargetWrapper(Owner.Unit));
            active = Owner.Buffs.AddBuff(HealingBuff, context, 1.Rounds().Seconds);
            if (active != null) active.IsNotDispelable = true;
        }
    }

    [Serializable]
    public sealed class ElementalBloodFastHealing : OwnedGameLogicComponent<UnitDescriptor>,
        ITickEachRound, IUnitLostFactHandler
    {
        public int Trait;
        public BlueprintFeature Provider;
        public BlueprintFeature Marker;

        public void HandleUnitLostFact(Fact fact)
        {
            Feature feature = fact as Feature;
            if (Owner != null && feature != null && Marker != null &&
                ReferenceEquals(feature.Owner, Owner) && ReferenceEquals(feature.Blueprint, Marker))
                Owner.Buffs.RemoveFact(Fact);
        }

        public void OnNewRound()
        {
            if (Owner == null || Owner.State.IsDead) return;
            if (Provider == null || !Owner.HasFact(Provider))
            {
                Owner.Buffs.RemoveFact(Fact);
                return;
            }
            if (Owner.Damage <= 0) return;
            ElementalAlternateTraitId trait = (ElementalAlternateTraitId)Trait;
            UnitPartElementalBloodCapacity capacity = Owner.Ensure<UnitPartElementalBloodCapacity>();
            if (!capacity.BeginHealing(trait)) return;
            try
            {
                Rulebook.Trigger<RuleHealDamage>(new ElementalBloodHeal(Owner.Unit, capacity, trait));
            }
            finally { capacity.EndHealing(); }
        }
    }

    // A narrow native rule subclass, not a global healing patch. All native
    // AboutToTrigger/DidTrigger subscribers still receive RuleHealDamage.
    internal sealed class ElementalBloodHeal : RuleHealDamage
    {
        private readonly UnitPartElementalBloodCapacity _capacity;
        private readonly ElementalAlternateTraitId _trait;
        private bool _resolved;

        internal ElementalBloodHeal(UnitEntityData owner,
            UnitPartElementalBloodCapacity capacity, ElementalAlternateTraitId trait)
            : base(owner, owner, new DiceFormula(0, DiceType.D6), ElementalBloodPolicy.HealingPerRound)
        {
            _capacity = capacity;
            _trait = trait;
        }

        public override void OnTrigger(RulebookEventContext context)
        {
            if (_resolved) return;
            _resolved = true;
            Modifier = ElementalBloodPolicy.CappedHealingModifier(Modifier, _capacity.Remaining(_trait));
            base.OnTrigger(context);
            _capacity.Record(_trait, Value);
        }
    }
}
