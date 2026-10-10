using System;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Which of the Dire Ape's two claw limbs an attack came from. The rend
    /// decision needs to tell them apart, because one claw striking twice is
    /// not two claws striking once.
    /// </summary>
    /// <summary>
    /// Which claw an attack came from, as a one-based index into the
    /// creature's own claw limbs. Zero means the attack was not one of them.
    ///
    /// <para>Sprint 18 held this in a two-valued enum, which was exactly
    /// right for a creature that prints two claws and cannot express one that
    /// prints four. The index is the same decision with the count taken out
    /// of the type.</para>
    /// </summary>
    internal static class ClawIndex
    {
        internal const int None = 0;
    }

    /// <summary>
    /// One printed ape's numbers, derived from its stat block rather than
    /// copied out of it.
    ///
    /// <para>These are the static baseline contract: what an unmodified
    /// creature must reproduce. They are not the runtime source of anything
    /// that can legitimately change. The Dire Ape's rend is the clearest
    /// case - the printed line reads 1d4+6, and the implementation is the
    /// derivation that produces it, so a buffed, enlarged or weakened Dire Ape
    /// rends for what its live Strength supports.</para>
    /// </summary>
    internal sealed class PrimateRulesProfile
    {
        internal PrimateRulesProfile(string key, int hitDice, int strength,
            int mobilityRanks, int perceptionRanks, int stealthRanks,
            int baseHitPoints, bool hasRend, int rendDiceCount,
            int rendDieSides, int printedClimbSkill)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("key");
            Key = key;
            HitDice = hitDice;
            Strength = strength;
            MobilityRanks = mobilityRanks;
            PerceptionRanks = perceptionRanks;
            StealthRanks = stealthRanks;
            BaseHitPoints = baseHitPoints;
            HasRend = hasRend;
            RendDiceCount = rendDiceCount;
            RendDieSides = rendDieSides;
            PrintedClimbSkill = printedClimbSkill;
        }

        internal string Key { get; private set; }
        internal int HitDice { get; private set; }
        internal int Strength { get; private set; }
        internal int StrengthModifier { get { return (Strength - 10) / 2; } }

        // Land-use allocations, not flat total bonuses. Attribute, class-skill,
        // size and Skill Focus contributions remain native and dynamic.
        internal int MobilityRanks { get; private set; }
        internal int PerceptionRanks { get; private set; }
        internal int StealthRanks { get; private set; }

        /// <summary>
        /// Printed racial dice only. Native Constitution and level
        /// dependencies remain live; the native class's generated hit points
        /// are not the stat block.
        /// </summary>
        internal int BaseHitPoints { get; private set; }

        internal bool HasRend { get; private set; }
        internal int RendDiceCount { get; private set; }
        internal int RendDieSides { get; private set; }

        /// <summary>
        /// The printed Climb total this project does not represent. Kept so the
        /// omission is a recorded number rather than a silence, and so no test
        /// can mistake an absent Climb for an unprinted one.
        /// </summary>
        internal int PrintedClimbSkill { get; private set; }

        /// <summary>
        /// One and a half times the Strength modifier, the tabletop bonus for a
        /// rending attack. Shared with
        /// <see cref="ExpandedSummoningSpecialProfiles.ConstrictBonus"/> so the
        /// two cannot drift apart, and equal to the engine's own
        /// <c>(int)(Strength.Bonus * 1.5f)</c> on every reachable modifier.
        /// </summary>
        internal int RendBonus
        {
            get
            {
                return ExpandedSummoningSpecialProfiles.ConstrictBonus(
                    StrengthModifier);
            }
        }

        internal string RendDamage
        {
            get
            {
                return RendDiceCount + "d" + RendDieSides +
                    (RendBonus >= 0 ? "+" : "-") + Math.Abs(RendBonus);
            }
        }

        /// <summary>
        /// A rend needs both claws on one target, which is a size delta of
        /// zero and no size gate at all: nothing printed restricts the target.
        /// </summary>
        internal bool RendHasTargetSizeGate { get { return false; } }
    }

    /// <summary>
    /// The two Sprint 18 apes. Nothing here knows about donors, views or
    /// Girallon, and nothing here is a framework: the rend decision below is
    /// the Dire Ape's own and is reachable by exactly one creature.
    /// </summary>
    internal static class PrimateRulesPolicy
    {
        internal const string ApeKey = "ape";
        internal const string DireApeKey = "dire-ape";

        /// <summary>
        /// The printed racial Climb bonus both apes carry. Recorded because the
        /// omission has to be honest about what is missing; it is never added
        /// to Athletics, Mobility or anything else.
        /// </summary>
        internal const int PrintedRacialClimbBonus = 8;

        /// <summary>Large, in the engine's own size ordering.</summary>
        internal const int LargeSize = 5;

        private static readonly PrimateRulesProfile Ape =
            new PrimateRulesProfile(ApeKey, 3, 15, 1, 1, 0, 13, false, 0, 0, 14);
        private static readonly PrimateRulesProfile DireApe =
            new PrimateRulesProfile(DireApeKey, 4, 19, 1, 1, 1, 18, true, 1, 4, 16);

        internal static bool IsPrimate(string key)
        { return key == ApeKey || key == DireApeKey; }

        internal static PrimateRulesProfile For(string key)
        {
            if (key == ApeKey) return Ape;
            if (key == DireApeKey) return DireApe;
            throw new ArgumentException("Not a Sprint 18 ape.", "key");
        }

        /// <summary>
        /// Creation-only exact rank allocation. The generic natural builder
        /// spends points by cycling a priority list and cannot express one rank
        /// each at these Intelligence scores, which is how an earlier creature
        /// read a skill total nobody printed.
        /// </summary>
        internal static void AllocateLandRanks(string key, ref int mobility,
            ref int perception, ref int stealth)
        {
            PrimateRulesProfile rules = For(key);
            if (mobility != 0 || perception != 0 || stealth != 0)
                throw new InvalidOperationException(
                    "Ape class ranks must start unallocated.");
            mobility = rules.MobilityRanks;
            perception = rules.PerceptionRanks;
            stealth = rules.StealthRanks;
        }

        /// <summary>
        /// Both apes have more than one natural attack, so every limb adds the
        /// ordinary Strength modifier. The engine's one-and-a-half rule is for
        /// a creature with a single natural attack, which neither ape is.
        /// </summary>
        internal static int PrimaryLimbDamageBonus(int liveStrengthBonus)
        { return liveStrengthBonus; }

        /// <summary>
        /// The rend bonus from a live Strength modifier. Identical to the
        /// engine's <c>(int)(Strength.Bonus * 1.5f)</c> across every modifier
        /// the game can produce, which is what lets the native rend carrier own
        /// the damage while this policy owns the contract.
        /// </summary>
        internal static int RendDamageBonus(int liveStrengthBonus)
        {
            return ExpandedSummoningSpecialProfiles.ConstrictBonus(
                liveStrengthBonus);
        }

        /// <summary>
        /// Whether this claw attack is the rend. Every operand is supplied by
        /// the caller, so the decision is inspectable without a game.
        ///
        /// <para><paramref name="hasLiveSequence"/> is false when the attack
        /// did not come from a live attack command - a replayed rule event, a
        /// scripted strike or a rake-style direct rulebook trigger. Those never
        /// rend: a rend is a thing the creature's attack sequence does.</para>
        /// </summary>
        internal static bool ShouldRend(bool isOwnedClaw, bool hasLiveSequence,
            bool everyOtherClawHitThisTargetInSequence, bool rendAlreadyArmed,
            bool rendAlreadyEmittedInSequence)
        {
            return isOwnedClaw && hasLiveSequence &&
                everyOtherClawHitThisTargetInSequence && !rendAlreadyArmed &&
                !rendAlreadyEmittedInSequence;
        }

        /// <summary>
        /// How many claws a printed rend needs. Two for the Dire Ape, four for
        /// the Girallon, and nothing for a creature that prints no rend. This
        /// is a lookup over the creatures that print one, not a framework for
        /// creatures that do not.
        /// </summary>
        internal static int RendClawCount(string key)
        {
            return key == DireApeKey ? 2 :
                key == GirallonRulesPolicy.GirallonKey ? 4 : 0;
        }

        internal static void Validate()
        {
            PrimateRulesProfile ape = For(ApeKey);
            PrimateRulesProfile dire = For(DireApeKey);
            // Printed hit points are the racial dice average plus the live
            // Constitution contribution: 3d8 is 13 and 4d8 is 18.
            if (ape.HitDice != 3 || ape.Strength != 15 ||
                ape.BaseHitPoints != 13 || ape.HasRend ||
                ape.MobilityRanks != 1 || ape.PerceptionRanks != 1 ||
                ape.StealthRanks != 0 || ape.PrintedClimbSkill != 14)
                throw new InvalidOperationException(
                    "Ape printed profile changed.");
            if (dire.HitDice != 4 || dire.Strength != 19 ||
                dire.BaseHitPoints != 18 || !dire.HasRend ||
                dire.RendDiceCount != 1 || dire.RendDieSides != 4 ||
                dire.MobilityRanks != 1 || dire.PerceptionRanks != 1 ||
                dire.StealthRanks != 1 || dire.PrintedClimbSkill != 16)
                throw new InvalidOperationException(
                    "Dire Ape printed profile changed.");
            // The printed rend line, reached by derivation rather than by a
            // constant: Strength 19 is a +4 modifier and one and a half of it
            // is +6.
            if (dire.StrengthModifier != 4 || dire.RendBonus != 6 ||
                dire.RendDamage != "1d4+6")
                throw new InvalidOperationException(
                    "Dire Ape printed rend derivation changed.");
            if (RendDamageBonus(0) != 0 || RendDamageBonus(3) != 4 ||
                RendDamageBonus(4) != 6 || RendDamageBonus(5) != 7 ||
                RendDamageBonus(-1) != -1)
                throw new InvalidOperationException(
                    "Rend bonus no longer matches the engine's one-and-a-half rule.");
            if (PrimaryLimbDamageBonus(4) != 4 || PrimaryLimbDamageBonus(-2) != -2)
                throw new InvalidOperationException(
                    "Ape limbs must add the ordinary Strength modifier.");
            // The printed rend lines, by claw count. The Girallon needs
            // all four and the Dire Ape both of two; an Ape prints no rend
            // and must stay at nothing.
            if (RendClawCount(DireApeKey) != 2 ||
                RendClawCount(GirallonRulesPolicy.GirallonKey) != 4 ||
                RendClawCount(ApeKey) != 0 || RendClawCount("girallon") != 4)
                throw new InvalidOperationException("Printed rend claw count changed.");
            if (ape.RendHasTargetSizeGate || dire.RendHasTargetSizeGate)
                throw new InvalidOperationException(
                    "Nothing printed gates a rend on target size.");
        }
    }

    /// <summary>
    /// The Dire Ape's rend sequencing, as a pure state machine over opaque
    /// sequence and target references. It replaces exactly one decision the
    /// engine's command-level rend gate cannot express for this creature -
    /// whether this attack is a rend - and owns no damage, no dice and no
    /// rule.
    ///
    /// <para>The engine decides a rend by hand slot: its gate fires when the
    /// secondary-hand attack follows a primary-hand hit. That identifies a
    /// rend by where a limb is kept rather than by which limb it is, so it
    /// cannot express a bite plus two equal primary claws, it would rend from
    /// a bite-then-claw pair, and it never checks that both hits landed on the
    /// same creature. This tracker checks the two things the printed line
    /// actually requires: both claws, one target.</para>
    ///
    /// <para>It is deliberately not serializable and holds no game state. A
    /// tracker lives only as long as the live unit it is attached to, so a
    /// reload, a death, a dismissal and an expiry all start from nothing, and
    /// a new round or a new attack command resets it.</para>
    /// </summary>
    internal sealed class ClawRendTracker
    {
        private readonly object[] _clawTargets;
        private object _sequence;
        private bool _armed;
        private bool _emitted;

        /// <summary>
        /// A tracker for a creature that rends on exactly this many claws.
        /// </summary>
        internal ClawRendTracker(int clawCount)
        {
            if (clawCount < 2)
                throw new ArgumentOutOfRangeException("clawCount",
                    "A printed rend needs at least two claws.");
            _clawTargets = new object[clawCount];
        }

        internal int ClawCount { get { return _clawTargets.Length; } }

        /// <summary>The attack command this tracker is following, if any.</summary>
        internal object Sequence { get { return _sequence; } }
        internal bool IsArmed { get { return _armed; } }
        internal bool HasEmitted { get { return _emitted; } }

        internal void Reset()
        {
            _sequence = null;
            for (int index = 0; index < _clawTargets.Length; index++)
                _clawTargets[index] = null;
            _armed = false;
            _emitted = false;
        }

        /// <summary>
        /// Whether a rend should resolve on this claw attack, and arm it when
        /// it should. A null sequence never rends.
        /// </summary>
        internal bool TryArm(object sequence, int claw, object target)
        {
            if (sequence == null || target == null || !Holds(claw)) return false;
            Follow(sequence);
            bool qualifies = PrimateRulesPolicy.ShouldRend(true, true,
                EveryOtherClawHit(claw, target), _armed, _emitted);
            if (qualifies) _armed = true;
            return qualifies;
        }

        /// <summary>
        /// Whether every claw but this one has already hit this exact target
        /// in this sequence. For the Dire Ape that is the one other claw; for
        /// the Girallon it is the other three, which is what its printed line
        /// requires and what three of four hits must not satisfy.
        /// </summary>
        private bool EveryOtherClawHit(int claw, object target)
        {
            for (int index = 0; index < _clawTargets.Length; index++)
            {
                if (index == claw - 1) continue;
                if (_clawTargets[index] == null ||
                    !ReferenceEquals(_clawTargets[index], target)) return false;
            }
            return true;
        }

        private bool Holds(int claw)
        { return claw >= 1 && claw <= _clawTargets.Length; }

        /// <summary>
        /// What the claw attack actually did. A miss disarms without emitting,
        /// so nothing is left pending; a hit that was armed is the one rend
        /// this sequence gets.
        /// </summary>
        internal void RecordOutcome(object sequence, int claw,
            object target, bool hit)
        {
            if (sequence == null) { Reset(); return; }
            if (!Holds(claw)) return;
            Follow(sequence);
            if (!hit) { _armed = false; return; }
            if (_armed) { _armed = false; _emitted = true; return; }
            if (target == null) return;
            _clawTargets[claw - 1] = target;
        }

        private void Follow(object sequence)
        {
            if (ReferenceEquals(_sequence, sequence)) return;
            Reset();
            _sequence = sequence;
        }
    }
}
