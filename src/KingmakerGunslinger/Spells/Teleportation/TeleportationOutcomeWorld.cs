using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.State;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;

namespace KingmakerGunslinger.Spells.Teleportation
{
    internal interface ITeleportationRolls
    {
        int D100();
        int D10();
        int Index(int count);
    }
    internal sealed class TeleportationCanonicalRolls : ITeleportationRolls
    {
        public int D100() { return RulebookEvent.Dice.D100; }
        public int D10() { return RulebookEvent.Dice.D10; }
        public int Index(int count)
        {
            if (count < 1) throw new ArgumentOutOfRangeException("count");
            // Native DiceTypeExtension.Sides returns its numeric die size. This
            // follows RulebookEvent.Dice.D, including the native RNG, for any bucket.
            return RulebookEvent.Dice.D(new DiceFormula(1, (DiceType)count)) - 1;
        }
    }

    internal sealed class TeleportationOutcomeWorld : ITeleportOutcomeWorld
    {
        private readonly TeleportationWorldSnapshot _before;
        private readonly TeleportationNativeCastSource _source;
        private readonly ITeleportationRolls _rolls;
        private readonly string _origin;
        internal readonly List<object> Events = new List<object>();
        internal TeleportationWorldSnapshot After { get; private set; }
        internal TeleportationOutcomeWorld(TeleportationWorldSnapshot before, TeleportationNativeCastSource source,
            string origin, ITeleportationRolls rolls)
        { _before = before; _source = source; _origin = origin; _rolls = rolls; }

        public int RollD100()
        { int roll = _rolls.D100(); Events.Add(new { kind = "d100", value = roll, provider = _rolls.GetType().FullName }); return roll; }
        public int RollD10()
        { int roll = _rolls.D10(); Events.Add(new { kind = "d10", value = roll, provider = _rolls.GetType().FullName }); return roll; }
        public IReadOnlyList<TeleportTravelerSnapshot> CaptureTravelers()
        {
            var current = TeleportationTravelers.Read(Kingmaker.Game.Instance.Player);
            if (!_before.Travelers.Matches(current)) throw new InvalidOperationException("Canonical traveling roster changed during resolution.");
            return current.OutcomeSnapshots();
        }
        public void DamageTraveler(string id, int amount)
        {
            if (amount < 1 || amount > 10) throw new ArgumentOutOfRangeException("amount");
            var current = TeleportationTravelers.Read(Kingmaker.Game.Instance.Player);
            if (!_before.Travelers.Matches(current)) throw new InvalidOperationException("Canonical traveling roster changed before mishap damage.");
            var target = current.Units.Single(value => value.UniqueId == id);
            if (target.Descriptor.State.IsDead) throw new InvalidOperationException("Mishap target is no longer living.");
            int before = target.Damage;
            var rule = new RuleDealDamage(_source.Book.Owner.Unit, target,
                new DamageBundle(new DirectDamage(new DiceFormula(0, DiceType.D10), amount))) {
                SourceAbility = _source.Ability.Blueprint
            };
            // No HP floor, direct HP assignment, or special death prevention.
            Rulebook.Trigger(rule);
            Events.Add(new { kind = "mishap-damage", unitId = id, rolledDamage = amount,
                nativeDamage = rule.Damage, damageBefore = before, damageAfter = target.Damage,
                livingAfter = !target.Descriptor.State.IsDead, unconsciousAfter = target.Descriptor.State.IsUnconscious,
                nativeRule = typeof(RuleDealDamage).FullName, minimumHitPoints = rule.MinHPAfterDamage });
        }
        public TeleportAlternateDecision SelectAlternate(string intendedId, string originId, double severity)
        {
            _before.Verify(_origin);
            var context = TeleportationWorldMapAdapter.Capture(false);
            BlueprintLocation intended = ResourcesLibrary.TryGetBlueprint<BlueprintLocation>(intendedId);
            string diagnostic;
            var candidates = TeleportationDistanceAdapter.Read(context, context.Rules.GetLocationObject(intended), out diagnostic);
            var decision = TeleportAlternateDestinationPolicy.Choose(candidates, intendedId, originId,
                TeleportationWorldMapAdapter.Forbidden, severity, _rolls.Index);
            Events.Add(new { kind = "alternate", intendedId = intendedId, originId = originId,
                severity = severity, basis = decision.Basis.ToString(), coordinateFallback = decision.CoordinateFallback,
                candidateCount = decision.CandidateCount, bucket = decision.Bucket, firstRank = decision.FirstRank,
                bucketSize = decision.BucketSize, alternateId = decision.Id, distance = decision.Distance, diagnostic = diagnostic });
            return decision;
        }
        public void Relocate(string destinationId, Action materialEffectStarting)
        {
            _before.Verify(_origin);
            var context = TeleportationWorldMapAdapter.Capture(false);
            BlueprintLocation destination = ResourcesLibrary.TryGetBlueprint<BlueprintLocation>(destinationId);
            var decision = TeleportDestinationPolicy.Evaluate(TeleportationWorldMapAdapter.ReadDestination(context, destination),
                _origin, TeleportationWorldMapAdapter.Forbidden);
            if (!decision.Eligible) throw new InvalidOperationException("Resolved destination changed before placement: " + decision.Diagnostic);
            materialEffectStarting();
            // The stationary-point portion of native settlement-circle relocation.
            // TeleportParty also reveals edges; this narrow boundary does not.
            context.Rules.SetCurrentPosition(new MapPosition(destination));
            context.Rules.UpdatePawnPosition();
            _before.Verify(destinationId);
            After = new TeleportationWorldSnapshot(TeleportationWorldMapAdapter.Capture(false));
            Events.Add(new { kind = "relocation", originId = _origin, destinationId = destinationId,
                nativeSetCurrentPosition = true, nativeUpdatePawnPosition = true, protectedStateUnchanged = true });
        }
        internal void VerifyRulesFailure()
        {
            _before.Verify(_origin);
            After = new TeleportationWorldSnapshot(TeleportationWorldMapAdapter.Capture(false));
        }
    }
}
