using System;
using System.Globalization;

namespace KingmakerGunslinger.Misfires
{
    /// <summary>
    /// Immutable result of comparing one final natural d20 with a firearm's
    /// blueprint-level misfire threshold. Sprint 24 consumes this pure result when
    /// deciding the exact item's bounded post-discharge condition transition.
    /// </summary>
    internal sealed class FirearmMisfireDecision
    {
        internal FirearmMisfireDecision(
            int naturalRoll,
            int misfireValue,
            bool nativeSuccess)
        {
            if (naturalRoll < 1 || naturalRoll > 20)
            {
                throw new ArgumentOutOfRangeException(
                    "naturalRoll",
                    naturalRoll,
                    "A natural d20 must be in the range 1..20.");
            }

            if (misfireValue < 0 || misfireValue > 20)
            {
                throw new ArgumentOutOfRangeException(
                    "misfireValue",
                    misfireValue,
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "A firearm misfire value must be in the range {0}..{1}.",
                        0,
                        20));
            }

            NaturalRoll = naturalRoll;
            MisfireValue = misfireValue;
            NativeSuccess = nativeSuccess;
            IsMisfire = misfireValue != 0 && naturalRoll <= misfireValue;
            FinalSuccess = nativeSuccess && !IsMisfire;
        }

        internal int NaturalRoll { get; private set; }

        internal int MisfireValue { get; private set; }

        internal bool NativeSuccess { get; private set; }

        internal bool IsMisfire { get; private set; }

        internal bool FinalSuccess { get; private set; }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "naturalD20={0}; misfireRange={1}; nativeSuccess={2}; misfired={3}; finalSuccess={4}",
                NaturalRoll,
                MisfireValue == 0 ? "none" : "1-" + MisfireValue.ToString(CultureInfo.InvariantCulture),
                NativeSuccess,
                IsMisfire,
                FinalSuccess);
        }
    }
}
