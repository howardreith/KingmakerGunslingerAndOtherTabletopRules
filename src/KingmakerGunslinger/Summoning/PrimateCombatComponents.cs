using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Creation-only printed land ranks and racial hit points for the two apes.
    /// Native class, attribute, size and feat modifiers still calculate the
    /// totals. No additive hidden bonus, donor mutation or reload-time
    /// reallocation.
    /// </summary>
    [Serializable]
    public sealed class SummonPrimateSkillRanks :
        OwnedGameLogicComponent<UnitDescriptor>,
        IHandleEntityComponent<UnitEntityData>
    {
        public string CreatureKey;
        public BlueprintUnit OwningBlueprint;

        public void OnEntityCreated(UnitEntityData unit)
        {
            // IHandleEntityComponent is invoked on the shared blueprint
            // component, not a per-unit clone. Never keep an applied flag here.
            // Native Initialize calls this only on creation, not deserialization.
            if (unit == null || !ReferenceEquals(unit.Blueprint, OwningBlueprint))
                throw new InvalidOperationException(
                    "Ape rank allocation requires its exact owning unit.");
            ModifiableValue mobility = unit.Descriptor.Stats.GetStat(
                StatType.SkillMobility);
            ModifiableValue perception = unit.Descriptor.Stats.GetStat(
                StatType.SkillPerception);
            ModifiableValue stealth = unit.Descriptor.Stats.GetStat(
                StatType.SkillStealth);
            int mobilityRanks = mobility.BaseValue;
            int perceptionRanks = perception.BaseValue;
            int stealthRanks = stealth.BaseValue;
            PrimateRulesPolicy.AllocateLandRanks(CreatureKey,
                ref mobilityRanks, ref perceptionRanks, ref stealthRanks);
            mobility.BaseValue = mobilityRanks;
            perception.BaseValue = perceptionRanks;
            stealth.BaseValue = stealthRanks;
            unit.Descriptor.Stats.HitPoints.BaseValue =
                PrimateRulesPolicy.For(CreatureKey).BaseHitPoints;
        }

        public void OnEntityRemoved(UnitEntityData unit) { }
    }

    /// <summary>
    /// The Dire Ape's rend gate. It decides one thing - whether this claw
    /// attack is the rend - and then lets the engine's own
    /// <c>RendFeature</c>, which sits on the same feature, resolve the damage
    /// from its dice and one and a half times the live Strength modifier.
    ///
    /// <para>The engine's command-level gate is not usable here. It fires when
    /// the secondary-hand attack follows a primary-hand hit, which identifies a
    /// rend by where a limb is kept rather than by which limb it is: it cannot
    /// express a bite plus two equal primary claws, it would rend from a
    /// bite-then-claw pair, and it never checks that both hits landed on one
    /// creature. This gate checks the two things the printed line requires.</para>
    ///
    /// <para>Everything it owns is session-scoped and per unit. The tracker is
    /// held in a weak table keyed on the live unit, is never serialized, and is
    /// reset when the attack command changes, when a new round begins and when
    /// the carrying fact turns off - so no rend state survives a command, a
    /// target change, a turn, a save and load, a death, a dismissal or an
    /// expiry. An attack that reaches the rulebook without a live attack
    /// command - a replay, a script, a direct rulebook trigger - has no
    /// sequence and never rends.</para>
    /// </summary>
    [Serializable]
    public sealed class DireApeRendGate :
        RuleInitiatorLogicComponent<RuleAttackWithWeapon>, ITickEachRound
    {
        private static readonly ConditionalWeakTable<UnitEntityData,
            ClawRendTracker> Trackers =
            new ConditionalWeakTable<UnitEntityData, ClawRendTracker>();

        public BlueprintUnit OwningBlueprint;
        public BlueprintItemWeapon Claw;

        /// <summary>
        /// How many claws this creature's printed rend needs. Two for the
        /// Dire Ape, four for the Girallon. Set by the builder from the
        /// creature's own printed line, so the gate never guesses.
        /// </summary>
        public int RendClawCount = 2;

        public override void OnEventAboutToTrigger(RuleAttackWithWeapon evt)
        {
            UnitEntityData owner = Mine(evt);
            if (owner == null) return;
            int claw = ClawLimb(owner, evt.Weapon);
            if (claw == ClawIndex.None) return;
            object sequence = Sequence(owner);
            lock (Trackers)
            {
                ClawRendTracker tracker = TrackerFor(owner);
                if (tracker.TryArm(sequence, claw, evt.Target)) evt.IsRend = true;
            }
        }

        public override void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
            UnitEntityData owner = Mine(evt);
            if (owner == null) return;
            int claw = ClawLimb(owner, evt.Weapon);
            if (claw == ClawIndex.None) return;
            bool hit = evt.AttackRoll != null && evt.AttackRoll.IsHit;
            lock (Trackers)
            {
                TrackerFor(owner).RecordOutcome(Sequence(owner),
                    claw, evt.Target, hit);
            }
        }

        /// <summary>A rend never carries across the round boundary.</summary>
        public void OnNewRound()
        {
            if (Owner == null || Owner.Unit == null) return;
            lock (Trackers) { Trackers.Remove(Owner.Unit); }
        }

        public override void OnTurnOff()
        {
            if (Owner != null && Owner.Unit != null)
                lock (Trackers) { Trackers.Remove(Owner.Unit); }
            base.OnTurnOff();
        }

        /// <summary>
        /// The attack command this attack belongs to, which is the sequence a
        /// rend is scoped to. Null when nothing is commanding the attack.
        /// </summary>
        private static object Sequence(UnitEntityData owner)
        {
            return owner == null || owner.Commands == null ? null :
                owner.Commands.Attack;
        }

        private UnitEntityData Mine(RuleAttackWithWeapon evt)
        {
            if (evt == null || Owner == null || Owner.Unit == null ||
                Claw == null || OwningBlueprint == null ||
                !ReferenceEquals(evt.Initiator, Owner.Unit) ||
                !ReferenceEquals(Owner.Unit.Blueprint, OwningBlueprint))
                return null;
            return Owner.Unit;
        }

        /// <summary>
        /// Which claw limb an attack came from. The two claws share one weapon
        /// blueprint, so they are told apart by the limb slot they occupy, not
        /// by their blueprint; the bite sits in the primary hand and is never a
        /// claw limb.
        /// </summary>
        private int ClawLimb(UnitEntityData owner, ItemEntityWeapon weapon)
        {
            if (weapon == null || weapon.Blueprint == null ||
                !ReferenceEquals(weapon.Blueprint, Claw))
                return ClawIndex.None;
            int index;
            if (SummonLimbs.Classify(owner, weapon, out index) !=
                SummonLimbKind.Additional) return ClawIndex.None;
            return index >= 0 && index < RendClawCount ?
                index + 1 : ClawIndex.None;
        }

        /// <summary>
        /// This unit's tracker, sized by this creature's printed claw count.
        /// A tracker built for a different count is discarded rather than
        /// reused, so a Girallon can never rend on a Dire Ape's two.
        /// </summary>
        private ClawRendTracker TrackerFor(UnitEntityData owner)
        {
            ClawRendTracker tracker;
            if (Trackers.TryGetValue(owner, out tracker))
            {
                if (tracker.ClawCount == RendClawCount) return tracker;
                Trackers.Remove(owner);
            }
            tracker = new ClawRendTracker(RendClawCount);
            Trackers.Add(owner, tracker);
            return tracker;
        }

        /// <summary>For the guarded runtime fixture: this unit's live tracker.</summary>
        internal static ClawRendTracker ObservedTracker(UnitEntityData unit)
        {
            if (unit == null) return null;
            ClawRendTracker tracker;
            lock (Trackers)
            { return Trackers.TryGetValue(unit, out tracker) ? tracker : null; }
        }

        /// <summary>For the guarded runtime fixture: forget this unit's state.</summary>
        internal static void ForgetObserved(UnitEntityData unit)
        {
            if (unit == null) return;
            lock (Trackers) { Trackers.Remove(unit); }
        }
    }

    /// <summary>
    /// Keeps every limb of a multi-attack creature on the plain Strength
    /// modifier, which is what its stat block prints.
    ///
    /// <para>The guarded Sprint 18 review measured the Ape slamming for
    /// 1d6+3 with one hand and 1d6+2 with the other, and the Dire Ape biting
    /// for one and a half times Strength while both claws carried one times
    /// it. The cause is native and precise: RuleCalculateWeaponStats gives a
    /// natural weapon in the primary hand one and a half times the damage
    /// stat whenever the SECONDARY HAND is empty, and it does not look at the
    /// additional limbs at all. A creature whose other attacks are additional
    /// primary limbs therefore reads as a single-handed natural attacker.</para>
    ///
    /// <para>Pathfinder gives one and a half times Strength only to a
    /// creature with a single natural attack. Both apes have more than one,
    /// so both print the plain modifier on every limb. This component takes
    /// the difference back off the one limb the engine inflated, on exactly
    /// the unit it is attached to, and only when the engine actually applied
    /// the multiplier to a Strength-driven primary-hand natural weapon on a
    /// body that really does carry additional limbs.</para>
    ///
    /// <para>Deliberately not done here: no global rule is patched, no other
    /// creature is touched, no bonus is added, and a creature that genuinely
    /// has one natural attack keeps the engine's one and a half times.</para>
    /// </summary>
    /// <para>It rides a feature rather than the unit's own component array,
    /// because a rulebook component is only subscribed when it is carried by
    /// a unit fact. The first correction put it on the blueprint and the
    /// re-run measured the inflated limb unchanged, which is how that was
    /// found. The feature is granted to exactly the two apes.</para>
    [Serializable]
    public sealed class SummonPrimaryLimbFullStrength :
        RuleInitiatorLogicComponent<RuleCalculateWeaponStats>
    {
        public override void OnEventAboutToTrigger(RuleCalculateWeaponStats evt) { }

        public override void OnEventDidTrigger(RuleCalculateWeaponStats evt)
        {
            UnitEntityData owner = Owner == null ? null : Owner.Unit;
            if (evt == null || owner == null ||
                !ReferenceEquals(evt.Initiator, owner)) return;
            int correction = Correction(owner, evt);
            if (correction == 0 || evt.DamageDescription == null ||
                evt.DamageDescription.Count == 0) return;
            Kingmaker.RuleSystem.Rules.Damage.DamageDescription row =
                evt.DamageDescription[0];
            row.Bonus -= correction;
            evt.DamageDescription[0] = row;
        }

        /// <summary>
        /// How much the engine added beyond the plain modifier, or zero when
        /// this attack is not the inflated primary limb.
        /// </summary>
        private static int Correction(UnitEntityData owner,
            RuleCalculateWeaponStats evt)
        {
            ItemEntityWeapon weapon = evt.Weapon;
            if (weapon == null || weapon.Blueprint == null ||
                !weapon.Blueprint.IsNatural || weapon.IsSecondary ||
                evt.SecondaryWeapon || !evt.DamageBonusStat.HasValue ||
                evt.DamageBonusStat.Value != StatType.Strength ||
                evt.DamageBonusStatMultiplier <= 1f) return 0;
            UnitBody body = owner.Body;
            if (body == null || body.PrimaryHand == null ||
                !ReferenceEquals(body.PrimaryHand.MaybeWeapon, weapon)) return 0;
            // Only a creature that really does attack with more than one limb
            // is owed the plain modifier.
            if (body.AdditionalLimbs == null || !body.AdditionalLimbs.Any(
                    slot => slot != null && slot.MaybeWeapon != null)) return 0;
            int modifier = owner.Descriptor.Stats.Strength.Bonus;
            if (modifier <= 0) return 0;
            int applied = (int)(modifier * evt.DamageBonusStatMultiplier);
            return applied > modifier ? applied - modifier : 0;
        }
    }
}
