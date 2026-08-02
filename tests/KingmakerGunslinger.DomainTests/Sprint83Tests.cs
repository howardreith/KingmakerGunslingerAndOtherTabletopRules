using System;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Gunsmithing;

namespace KingmakerGunslinger.DomainTests
{
    internal static class Sprint83Tests
    {
        internal static void OwnerNormal()
        {
            BatteredFirearmUseDecision value = BatteredFirearmUsePolicy.Evaluate(
                true, true, FirearmCondition.Normal, 1000);
            Assertions.Equal(FirearmCondition.Normal, value.EffectiveCondition,
                "The originating owner did not use the battered firearm normally.");
            Assertions.True(value.CanFire, "The owner's Normal battered firearm was rejected.");
            Assertions.Equal(22, value.SaleValueGold, "Battered scrap value mismatch.");
        }

        internal static void OwnerBroken()
        {
            BatteredFirearmUseDecision value = BatteredFirearmUsePolicy.Evaluate(
                true, true, FirearmCondition.Broken, 1000);
            Assertions.Equal(FirearmCondition.Broken, value.EffectiveCondition,
                "The originating owner's actual Broken state changed.");
            Assertions.True(value.CanFire, "The owner's Broken firearm should remain usable.");
        }

        internal static void NonOwnerNormal()
        {
            BatteredFirearmUseDecision value = BatteredFirearmUsePolicy.Evaluate(
                true, false, FirearmCondition.Normal, 1000);
            Assertions.Equal(FirearmCondition.Broken, value.EffectiveCondition,
                "A nonowner did not treat the battered firearm as Broken.");
            Assertions.True(value.CanFire, "A nonowner should be able to fire an actually Normal battered firearm as Broken.");
        }

        internal static void NonOwnerBroken()
        {
            BatteredFirearmUseDecision value = BatteredFirearmUsePolicy.Evaluate(
                true, false, FirearmCondition.Broken, 1000);
            Assertions.Equal(FirearmCondition.Wrecked, value.EffectiveCondition,
                "A nonowner did not treat an actually Broken battered firearm as unusable.");
            Assertions.False(value.CanFire, "A nonowner fired an actually Broken battered firearm.");
        }

        internal static void NonOwnerWrecked()
        {
            BatteredFirearmUseDecision value = BatteredFirearmUsePolicy.Evaluate(
                true, false, FirearmCondition.Wrecked, 1000);
            Assertions.Equal(FirearmCondition.Wrecked, value.EffectiveCondition,
                "Wrecked battered state changed for a nonowner.");
            Assertions.False(value.CanFire, "A nonowner fired a Wrecked battered firearm.");
        }

        internal static void OrdinaryFirearm()
        {
            BatteredFirearmUseDecision value = BatteredFirearmUsePolicy.Evaluate(
                false, true, FirearmCondition.Normal, 1500);
            Assertions.False(value.IsOriginatingOwner,
                "Ordinary firearms must not acquire battered ownership semantics.");
            Assertions.Equal(1500, value.SaleValueGold,
                "Ordinary firearm sale value changed.");
        }

        internal static void InvalidInputs()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                BatteredFirearmUsePolicy.Evaluate(true, true,
                    FirearmCondition.Unknown, 1), "Unknown condition was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                BatteredFirearmUsePolicy.Evaluate(true, true,
                    FirearmCondition.Normal, -1), "Negative sale value was accepted.");
        }
    }
}
