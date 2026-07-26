namespace KingmakerGunslinger.Misfires
{
    /// <summary>
    /// Pure Sprint 23 natural-roll rule. A roll in the configured misfire range
    /// always fails even when Kingmaker's ordinary attack calculation succeeded.
    /// Rolls outside the range preserve Kingmaker's result unchanged.
    /// </summary>
    internal sealed class FirearmMisfireService
    {
        internal FirearmMisfireDecision Evaluate(
            int naturalRoll,
            int misfireValue,
            bool nativeSuccess)
        {
            return new FirearmMisfireDecision(
                naturalRoll,
                misfireValue,
                nativeSuccess);
        }
    }
}
