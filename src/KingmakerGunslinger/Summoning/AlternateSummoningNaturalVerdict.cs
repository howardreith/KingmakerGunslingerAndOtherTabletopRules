using System;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// What one natural cast in the working-save scenario observed. The
    /// scenario records the engine state when its wait ends; nothing here is
    /// derived from the adapter builder under test.
    /// </summary>
    internal sealed class NaturalCastObservation
    {
        internal bool TargetClickAccepted;
        internal bool CommandIssued;
        /// <summary>The wait ended because its condition held (command
        /// finished and the standard-action cooldown cleared), not because
        /// the deadline passed.</summary>
        internal bool WaitConditionReached;
        internal bool CommandFinished;
        internal bool CooldownCleared;
        internal string Result = "NotStarted";
        internal bool Acted;
        internal bool CooldownOverride;
        internal bool AssistedAct;
        internal bool DirectCommandTick;
        internal int ResourceBefore;
        internal int ResourceAfter;
        internal int ExpectedCost = 1;
        internal SummonMultiplicity Multiplicity;
        internal bool Maximize;
        internal bool Empower;
        internal string ExpectedUnit;
        internal string[] SummonedUnits = new string[0];
        internal bool AllOwnedByCaster;
    }

    /// <summary>
    /// The verdict for one natural cast. It requires the actual terminal
    /// state (command finished, cooldown cleared, wait condition reached),
    /// no assistance, the exact spend, and a creature count inside the
    /// tabletop bounds of the chosen option.
    /// </summary>
    internal static class AlternateSummoningNaturalVerdict
    {
        /// <summary>
        /// The legal creature count of a summon option: one, 1d3 or 1d4+1,
        /// taken from the tabletop rule, not from the built ability. The only
        /// native count modifiers are Maximize (every die at its maximum) and
        /// Empower (half again, rounded down); both apply only to dice.
        /// </summary>
        internal static void Bounds(SummonMultiplicity multiplicity, bool maximize, bool empower,
            out int minimum, out int maximum)
        {
            switch (multiplicity)
            {
                case SummonMultiplicity.One: minimum = maximum = 1; return;
                case SummonMultiplicity.OneD3: minimum = 1; maximum = 3; break;
                case SummonMultiplicity.OneD4PlusOne: minimum = 2; maximum = 5; break;
                default: throw new ArgumentOutOfRangeException("multiplicity");
            }
            if (maximize) minimum = maximum;
            if (empower) { minimum += minimum / 2; maximum += maximum / 2; }
        }

        /// <summary>Why the wait ended: completed, or which state stalled.</summary>
        internal static string WaitOutcome(NaturalCastObservation value)
        {
            if (!value.CommandIssued) return "not-issued";
            if (value.WaitConditionReached && value.CommandFinished && value.CooldownCleared)
                return "completed";
            if (!value.CommandFinished) return "command-stalled";
            if (!value.CooldownCleared) return "cooldown-stalled";
            return "deadline";
        }

        /// <summary>Null when the cast passes, otherwise the first reason.</summary>
        internal static string Verify(NaturalCastObservation value)
        {
            if (value == null) return "no-observation";
            if (!value.TargetClickAccepted) return "target-click-refused";
            if (!value.CommandIssued) return "command-not-issued";
            if (value.CooldownOverride) return "assisted:cooldown-override";
            if (value.AssistedAct) return "assisted:forced-act";
            if (value.DirectCommandTick) return "assisted:direct-command-tick";
            string wait = WaitOutcome(value);
            if (wait != "completed") return "wait:" + wait;
            if (!string.Equals(value.Result, "Success", StringComparison.Ordinal))
                return "result:" + value.Result;
            if (!value.Acted) return "not-acted";
            int spent = value.ResourceBefore - value.ResourceAfter;
            if (spent != value.ExpectedCost)
                return "resource:expected=" + value.ExpectedCost + ";actual=" + spent;
            int minimum, maximum;
            Bounds(value.Multiplicity, value.Maximize, value.Empower, out minimum, out maximum);
            int count = value.SummonedUnits == null ? 0 : value.SummonedUnits.Length;
            if (count < minimum || count > maximum)
                return "quantity:" + value.Multiplicity + ":expected=[" + minimum + "," + maximum +
                    "];actual=" + count;
            if (value.SummonedUnits.Any(unit => !string.Equals(unit, value.ExpectedUnit,
                StringComparison.Ordinal)))
                return "unit:expected=" + value.ExpectedUnit + ";actual=[" +
                    string.Join(",", value.SummonedUnits.Distinct().ToArray()) + "]";
            if (!value.AllOwnedByCaster) return "ownership";
            return null;
        }
    }
}
