using System;

namespace KingmakerGunslinger.Rules
{
    /// <summary>
    /// Immutable result of selecting ordinary or touch AC for one firearm attack.
    /// A decision can select touch AC without requiring a write when ordinary and
    /// touch AC happen to be numerically identical.
    /// </summary>
    internal sealed class FirearmArmorClassDecision
    {
        internal FirearmArmorClassDecision(
            bool usesTouchArmorClass,
            bool shouldWriteTargetArmorClass,
            int selectedTargetArmorClass,
            int adjustment,
            int rangeIncrement,
            string reason)
        {
            if (rangeIncrement < 0)
            {
                throw new ArgumentOutOfRangeException("rangeIncrement");
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new ArgumentException("A decision reason is required.", "reason");
            }

            UsesTouchArmorClass = usesTouchArmorClass;
            ShouldWriteTargetArmorClass = shouldWriteTargetArmorClass;
            SelectedTargetArmorClass = selectedTargetArmorClass;
            Adjustment = adjustment;
            RangeIncrement = rangeIncrement;
            Reason = reason.Trim();
        }

        internal bool UsesTouchArmorClass { get; private set; }

        internal bool ShouldWriteTargetArmorClass { get; private set; }

        internal int SelectedTargetArmorClass { get; private set; }

        internal int Adjustment { get; private set; }

        internal int RangeIncrement { get; private set; }

        internal string Reason { get; private set; }
    }
}
