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

    internal sealed class UngulateRulesProfile
    {
        internal UngulateRulesProfile(string key, int hitDice, int strength,
            int goreDiceCount, int goreDieSides, int trampleDiceCount,
            int trampleDieSides, int chargeDiceCount, int chargeDieSides,
            int chargeBonus, bool stampede)
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
                2, 6, 4, 8, 18, false)
        };

        internal static IReadOnlyList<UngulateRulesProfile> All
        { get { return Array.AsReadOnly(Values); } }

        internal static UngulateRulesProfile For(string key)
        { return Values.Single(value => value.Key == key); }

        internal static void Validate()
        {
            if (Values.Length != 4 || Values.Select(value => value.Key)
                    .Distinct(StringComparer.Ordinal).Count() != 4 ||
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
