using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    internal enum TrampleTargetResponseDecision
    {
        ReflexSave,
        OpportunityAttackContinues,
        OpportunityAttackStops
    }

    /// <summary>
    /// Pure owner-authorized automatic response policy. A contacted target
    /// receives exactly one branch, selected before any roll is observed.
    /// </summary>
    internal static class TrampleTargetResponsePolicy
    {
        internal static bool HasLegalOpportunityAttack(bool hasResource,
            bool canAct, bool hasMeleeAttack, bool threatens,
            bool nativeRulesPermit)
        {
            return hasResource && canAct && hasMeleeAttack && threatens &&
                nativeRulesPermit;
        }

        internal static TrampleTargetResponseDecision Resolve(
            bool legalOpportunityAttack, bool opportunityAttackExecuted,
            bool tramplerCanContinue)
        {
            if (!legalOpportunityAttack || !opportunityAttackExecuted)
                return TrampleTargetResponseDecision.ReflexSave;
            return tramplerCanContinue ?
                TrampleTargetResponseDecision.OpportunityAttackContinues :
                TrampleTargetResponseDecision.OpportunityAttackStops;
        }
    }

    /// <summary>
    /// Pure formation policy for Stampede. Every member of the qualifying
    /// trio must independently pass the caller's exact owner/action checks,
    /// and all three must remain mutually adjacent at the current contact.
    /// </summary>
    internal static class StampedeFormationPolicy
    {
        internal static int QualifiedGroupSize<T>(T actor,
            IEnumerable<T> candidates, Func<T, bool> isEligible,
            Func<T, T, bool> isAdjacent)
        {
            if (ReferenceEquals(actor, null) || candidates == null ||
                isEligible == null || isAdjacent == null ||
                !isEligible(actor)) return 0;
            T[] eligible = candidates.Where(value =>
                    !ReferenceEquals(value, null) && isEligible(value))
                .Distinct().ToArray();
            if (!eligible.Contains(actor) || eligible.Length < 3) return 0;
            for (int first = 0; first < eligible.Length; first++)
            {
                T left = eligible[first];
                if (object.Equals(left, actor) || !isAdjacent(actor, left))
                    continue;
                for (int second = first + 1;
                    second < eligible.Length; second++)
                {
                    T right = eligible[second];
                    if (object.Equals(right, actor) ||
                        !isAdjacent(actor, right) ||
                        !isAdjacent(left, right)) continue;
                    return 3;
                }
            }
            return 0;
        }
    }

    internal sealed class UngulateRulesProfile
    {
        internal UngulateRulesProfile(string key, int hitDice, int strength,
            int goreDiceCount, int goreDieSides, int trampleDiceCount,
            int trampleDieSides, int chargeDiceCount, int chargeDieSides,
            int chargeBonus, bool stampede, bool liveTrampleMath = false,
            bool livePowerfulChargeMath = false)
        {
            Key = key;
            HitDice = hitDice;
            Strength = strength;
            GoreDiceCount = goreDiceCount;
            GoreDieSides = goreDieSides;
            TrampleDiceCount = trampleDiceCount;
            TrampleDieSides = trampleDieSides;
            ChargeDiceCount = chargeDiceCount;
            ChargeDieSides = chargeDieSides;
            ChargeBonus = chargeBonus;
            Stampede = stampede;
            LiveTrampleMath = liveTrampleMath;
            LivePowerfulChargeMath = livePowerfulChargeMath;
        }

        internal string Key { get; private set; }
        internal int HitDice { get; private set; }
        internal int Strength { get; private set; }
        internal int StrengthModifier { get { return (Strength - 10) / 2; } }
        internal int GoreDiceCount { get; private set; }
        internal int GoreDieSides { get; private set; }
        internal int GoreBonus { get { return StrengthModifier * 3 / 2; } }
        internal bool HasTrample { get { return TrampleDiceCount > 0; } }
        internal int TrampleDiceCount { get; private set; }
        internal int TrampleDieSides { get; private set; }
        internal int TrampleBonus { get { return GoreBonus; } }
        internal int TrampleDc { get { return 10 + HitDice / 2 + StrengthModifier; } }
        internal bool HasPowerfulCharge { get { return ChargeDiceCount > 0; } }
        internal int ChargeDiceCount { get; private set; }
        internal int ChargeDieSides { get; private set; }
        internal int ChargeBonus { get; private set; }
        internal int ChargeBonusIncrement
        { get { return HasPowerfulCharge ? ChargeBonus - GoreBonus : 0; } }
        internal int ChargeDiceIncrement
        { get { return HasPowerfulCharge ? ChargeDiceCount - GoreDiceCount : 0; } }
        internal bool Stampede { get; private set; }
        internal bool LiveTrampleMath { get; private set; }
        internal bool LivePowerfulChargeMath { get; private set; }

        internal bool StampedeActive(int adjacentStampedingCreatures)
        { return Stampede && adjacentStampedingCreatures >= 3; }

        internal bool CanTrample(int tramplerSizeOrder, int targetSizeOrder,
            int adjacentStampedingCreatures)
        {
            return HasTrample && tramplerSizeOrder > 0 && targetSizeOrder >= 0 &&
                targetSizeOrder < tramplerSizeOrder +
                    (StampedeActive(adjacentStampedingCreatures) ? 1 : 0);
        }

        internal int TrampleSaveDc(int adjacentStampedingCreatures)
        { return TrampleDc + (StampedeActive(adjacentStampedingCreatures) ? 2 : 0); }

        internal int TrampleBonusFor(int liveStrengthModifier)
        {
            return LiveTrampleMath ? liveStrengthModifier * 3 / 2 :
                TrampleBonus;
        }

        internal int TrampleSaveDc(int adjacentStampedingCreatures,
            int liveHitDice, int liveStrengthModifier)
        {
            int baseline = LiveTrampleMath ?
                10 + liveHitDice / 2 + liveStrengthModifier : TrampleDc;
            return baseline +
                (StampedeActive(adjacentStampedingCreatures) ? 2 : 0);
        }

        internal bool AppliesPowerfulCharge(bool isCharge, bool isFirstAttack,
            bool isGore, bool isAttackOfOpportunity)
        { return HasPowerfulCharge && isCharge && isFirstAttack && isGore &&
            !isAttackOfOpportunity; }
    }

    internal static class UngulateRulesPolicy
    {
        private static readonly UngulateRulesProfile[] Values = {
            new UngulateRulesProfile("aurochs", 3, 23, 1, 8, 2, 6,
                0, 0, 0, true),
            new UngulateRulesProfile("bison", 5, 27, 2, 6, 2, 6,
                0, 0, 0, true),
            new UngulateRulesProfile("rhinoceros", 5, 22, 2, 6, 0, 0,
                4, 6, 12, false),
            new UngulateRulesProfile("woolly-rhinoceros", 8, 28, 2, 8,
                2, 6, 4, 8, 18, false),
            // Sprint 15's Giant Stag Beetle. This type is named for the
            // ungulates because they were the only creatures that had a
            // trample; it is the project's trample-and-charge rules carrier
            // rather than a taxonomy, and renaming it across every file that
            // uses it would be more churn than the clarity is worth.
            //
            // Nothing here is a literal from the creature's trample line, and
            // the derivation still lands on it exactly: damage is one and a
            // half times the Strength modifier on 1d6, which is 1d6+6 at
            // Strength 19, and the save is 10 plus half the hit dice plus the
            // Strength modifier, which is DC 17 at 7 hit dice. Its primary
            // attack is a bite rather than a gore, and the gore dice are
            // carried only so the charge arithmetic has honest inputs; the
            // beetle has no powerful charge and no Stampede, which belongs to
            // the herd ungulates alone.
            new UngulateRulesProfile("giant-stag-beetle", 7, 19, 2, 8,
                1, 6, 0, 0, 0, false),
            // Sprint 23. This reuses the qualified path/contact/response
            // carrier, not ungulate taxonomy. Triceratops has no Stampede.
            // Its Strength and racial-HD contributions are read live; the
            // older five rows retain their frozen released arithmetic.
            new UngulateRulesProfile("triceratops", 14, 26, 2, 10,
                1, 8, 4, 10, 16, false, true, true),
            // Sprint 25 Brachiosaurus, on the same qualified
            // path/contact/response carrier. Nothing here is a literal
            // from the printed trample line and the derivation lands on it
            // exactly: one and a half times the Strength modifier on 2d6
            // is 2d6+19 at Strength 37, and ten plus half the hit dice
            // plus the Strength modifier is DC 32 at eighteen hit dice.
            //
            // The gore slots carry the tail dice. This creature has no
            // gore at all - its single natural attack is a 4d6 tail - and
            // the slots exist so the charge arithmetic has honest inputs,
            // exactly as the Giant Stag Beetle carries its bite there.
            // Trample damage does not read them: TrampleBonus derives from
            // the Strength modifier alone. No powerful charge and no
            // Stampede, which belongs to the herd ungulates.
            new UngulateRulesProfile("brachiosaurus", 18, 37, 4, 6,
                2, 6, 0, 0, 0, false, true)
        };

        internal static IReadOnlyList<UngulateRulesProfile> All
        { get { return Array.AsReadOnly(Values); } }

        internal static UngulateRulesProfile For(string key)
        { return Values.Single(value => value.Key == key); }

        internal static void Validate()
        {
            if (Values.Length != 7 || Values.Select(value => value.Key)
                    .Distinct(StringComparer.Ordinal).Count() != 7 ||
                Values.Any(value => value.HitDice < 1 || value.Strength < 1 ||
                    value.GoreDiceCount < 1 || value.GoreDieSides < 1 ||
                    !value.HasTrample && !value.HasPowerfulCharge))
                throw new InvalidOperationException(
                    "Sprint 11 ungulate rules profile is incomplete.");
        }
    }

    /// <summary>
    /// Request-local/instance-local damage ledger. It tracks only target identity
    /// and round; callers must clear it when a trample ends or a unit is removed.
    /// </summary>
    internal sealed class TrampleRoundLedger
    {
        private long _round = -1;
        private bool _halted;
        private readonly HashSet<string> _targets =
            new HashSet<string>(StringComparer.Ordinal);

        internal bool TryClaim(long round, string targetId)
        {
            if (round < 0 || string.IsNullOrEmpty(targetId) || round < _round)
                return false;
            if (round != _round)
            {
                _round = round;
                _halted = false;
                _targets.Clear();
            }
            if (_halted) return false;
            return _targets.Add(targetId);
        }

        internal bool HasClaim(long round, string targetId)
        {
            return round == _round && !string.IsNullOrEmpty(targetId) &&
                _targets.Contains(targetId);
        }

        internal bool IsHalted(long round)
        { return round == _round && _halted; }

        internal void Halt(long round)
        {
            if (round < 0 || round < _round) return;
            if (round != _round)
            {
                _round = round;
                _targets.Clear();
            }
            _halted = true;
        }

        internal void Clear()
        { _round = -1; _halted = false; _targets.Clear(); }
    }
}
