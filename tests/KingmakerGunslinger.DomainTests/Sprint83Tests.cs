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

        internal static void OwnershipBind()
        {
            var ledger = new BatteredFirearmOwnershipLedger();
            FirearmItemId item = Item(1); OriginatingUnitId owner = new OriginatingUnitId("unit-a");
            Assertions.True(ledger.Bind(item, owner), "First exact binding was not created.");
            OriginatingUnitId observed;
            Assertions.True(ledger.TryGetOwner(item, out observed), "Binding was not readable.");
            Assertions.Equal("unit-a", observed.Value, "Originating owner changed.");
        }

        internal static void OwnershipIdempotent()
        {
            var ledger = new BatteredFirearmOwnershipLedger();
            Assertions.True(ledger.Bind(Item(1), new OriginatingUnitId("unit-a")), "First bind failed.");
            Assertions.False(ledger.Bind(Item(1), new OriginatingUnitId("unit-a")), "Same binding was duplicated.");
            Assertions.Equal(1, ledger.Count, "Idempotent bind changed count.");
        }

        internal static void OwnershipConflict()
        {
            var ledger = new BatteredFirearmOwnershipLedger();
            ledger.Bind(Item(1), new OriginatingUnitId("unit-a"));
            Assertions.Throws<InvalidOperationException>(() =>
                ledger.Bind(Item(1), new OriginatingUnitId("unit-b")),
                "Originating owner rebinding was accepted.");
            Assertions.Equal(1, ledger.Count, "Conflict mutated the ledger.");
        }

        internal static void OwnershipIsolation()
        {
            var ledger = new BatteredFirearmOwnershipLedger();
            ledger.Bind(Item(1), new OriginatingUnitId("unit-a"));
            ledger.Bind(Item(2), new OriginatingUnitId("unit-b"));
            OriginatingUnitId first, second;
            ledger.TryGetOwner(Item(1), out first); ledger.TryGetOwner(Item(2), out second);
            Assertions.Equal("unit-a", first.Value, "First item owner drifted.");
            Assertions.Equal("unit-b", second.Value, "Second item owner drifted.");
        }

        internal static void OwnershipSnapshot()
        {
            var ledger = new BatteredFirearmOwnershipLedger();
            ledger.Bind(Item(1), new OriginatingUnitId("unit-a"));
            BatteredFirearmOwnershipRecord[] snapshot = ledger.Snapshot();
            Assertions.Equal(1, snapshot.Length, "Snapshot count mismatch.");
            Assertions.Equal("unit-a", snapshot[0].OwnerId, "Snapshot owner mismatch.");
        }

        internal static void OwnershipInvalid()
        {
            Assertions.Throws<ArgumentException>(() => new OriginatingUnitId(" unit-a"),
                "Padded unit identity was accepted.");
            Assertions.Throws<ArgumentException>(() => new OriginatingUnitId(""),
                "Empty unit identity was accepted.");
            var ledger = new BatteredFirearmOwnershipLedger();
            Assertions.Throws<ArgumentNullException>(() => ledger.Bind(null,
                new OriginatingUnitId("unit-a")), "Null item identity was accepted.");
        }

        internal static void OwnershipRemove()
        {
            var ledger = new BatteredFirearmOwnershipLedger();
            FirearmItemId item = Item(1);
            ledger.Bind(item, new OriginatingUnitId("unit-a"));
            Assertions.Throws<InvalidOperationException>(() => ledger.Remove(item,
                new OriginatingUnitId("unit-b")), "Wrong-owner removal was accepted.");
            Assertions.Equal(1, ledger.Count, "Wrong-owner removal mutated the ledger.");
            Assertions.True(ledger.Remove(item, new OriginatingUnitId("unit-a")),
                "Exact-owner removal failed.");
            Assertions.False(ledger.Remove(item, new OriginatingUnitId("unit-a")),
                "Missing record removal was not idempotent.");
        }

        private static FirearmItemId Item(int suffix)
        {
            return new FirearmItemId(string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "00000000-0000-0000-0000-{0:D12}", suffix));
        }
    }
}
