using System;
using KingmakerGunslinger.Deeds;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.DomainTests
{
    internal static partial class Program
    {
        private static void LightningReloadAvailableWithoutGritSpend()
        {
            LightningReloadDecision value = Lightning(false, FirearmCondition.Normal,
                0, 1, 2, true);
            Assertions.True(value.IsAvailable, "Eligible swift reload was rejected.");
            Assertions.Equal(1, value.RoundsToLoad, "Reload count mismatch.");
            Assertions.Equal(0, value.GritCost, "Lightning Reload spent grit.");
            Assertions.True(value.MarkUsedOnSuccess, "Successful reload was not marked once-per-round.");
        }

        private static void LightningReloadRequiresPositiveGritAndRoundAvailability()
        {
            Assertions.Equal(LightningReloadStatus.NoGrit,
                Lightning(false, FirearmCondition.Normal, 0, 1, 0, true).Status,
                "Zero grit was accepted.");
            Assertions.Equal(LightningReloadStatus.UsedThisRound,
                Lightning(true, FirearmCondition.Normal, 0, 1, 1, true).Status,
                "Second same-round reload was accepted.");
        }

        private static void LightningReloadPreservesEligibleBrokenState()
        {
            Assertions.True(Lightning(false, FirearmCondition.Broken,
                0, 1, 1, true).IsAvailable,
                "Empty Broken firearm was rejected instead of remaining repair-independent.");
        }

        private static void LightningReloadUnitUseIsIndependent()
        {
            Assertions.Equal(LightningReloadStatus.UsedThisRound,
                Lightning(true, FirearmCondition.Normal, 0, 1, 1, true).Status,
                "Used unit was not gated.");
            Assertions.True(Lightning(false, FirearmCondition.Normal,
                0, 1, 1, true).IsAvailable,
                "One unit's round use leaked into another unit's request.");
        }

        private static void LightningReloadRejectsStateAndResourceGates()
        {
            Assertions.Equal(LightningReloadStatus.NotFirearm,
                new LightningReloadService().Evaluate(new LightningReloadRequest(false,
                    FirearmCondition.Normal, 0, 1, 1, true, false)).Status,
                "Non-firearm was accepted.");
            Assertions.Equal(LightningReloadStatus.Wrecked,
                Lightning(false, FirearmCondition.Wrecked, 0, 1, 1, true).Status,
                "Wrecked firearm was accepted.");
            Assertions.Equal(LightningReloadStatus.Loaded,
                Lightning(false, FirearmCondition.Normal, 1, 1, 1, true).Status,
                "Loaded firearm was accepted.");
            Assertions.Equal(LightningReloadStatus.MissingAmmunition,
                Lightning(false, FirearmCondition.Normal, 0, 1, 1, false).Status,
                "Missing ammunition was accepted.");
        }

        private static void LightningReloadInvalidInputRejected()
        {
            var service = new LightningReloadService();
            Assertions.Throws<ArgumentNullException>(() => service.Evaluate(null),
                "Null request was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new LightningReloadRequest(true, FirearmCondition.Normal,
                    2, 1, 1, true, false), "Over-capacity state was accepted.");
        }

        private static LightningReloadDecision Lightning(bool used,
            FirearmCondition condition, int loaded, int capacity, int grit,
            bool ammunition)
        {
            return new LightningReloadService().Evaluate(new LightningReloadRequest(
                true, condition, loaded, capacity, grit, ammunition, used));
        }
    }
}
