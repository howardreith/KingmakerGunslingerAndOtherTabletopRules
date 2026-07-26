using System;
using KingmakerGunslinger.Actions;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.DomainTests
{
    internal static partial class Program
    {
        private static void GenericReloadNormal()
        {
            AssertAvailable(FirearmActionKind.Reload, FirearmState.CreateEmpty(), true);
        }

        private static void GenericReloadBroken()
        {
            AssertAvailable(FirearmActionKind.Reload, Empty(FirearmCondition.Broken), true);
        }

        private static void GenericReloadWreckedRejected()
        {
            AssertRejected(FirearmActionKind.Reload, Empty(FirearmCondition.Wrecked), true);
        }

        private static void GenericReloadLoadedRejected()
        {
            AssertRejected(FirearmActionKind.Reload, Loaded(FirearmCondition.Normal), true);
        }

        private static void GenericReloadMissingResourcesRejected()
        {
            AssertRejected(FirearmActionKind.Reload, FirearmState.CreateEmpty(), false);
        }

        private static void GenericOverhaulWrecked()
        {
            AssertAvailable(FirearmActionKind.Overhaul, Empty(FirearmCondition.Wrecked), true);
        }

        private static void GenericOverhaulBrokenRejected()
        {
            AssertRejected(FirearmActionKind.Overhaul, Empty(FirearmCondition.Broken), true);
        }

        private static void GenericRepairBroken()
        {
            AssertAvailable(FirearmActionKind.Repair, Empty(FirearmCondition.Broken), true);
        }

        private static void GenericRepairLoadedRejected()
        {
            AssertRejected(FirearmActionKind.Repair, Loaded(FirearmCondition.Broken), true);
        }

        private static void GenericRepairMissingKitRejected()
        {
            AssertRejected(FirearmActionKind.Repair, Empty(FirearmCondition.Broken), false);
        }

        private static void GenericUnknownActionRejected()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => FirearmActionPolicy.Evaluate(
                    FirearmActionKind.Unknown,
                    EarlyMusket(),
                    FirearmState.CreateEmpty(),
                    true),
                "Unknown generic action must fail closed.");
        }

        private static void ReloadProfileAmmunitionIdentity()
        {
            var ammunition = new AmmunitionId("kmg.test.paper-cartridge");
            var profile = new ReloadProfile(
                ReloadActionType.FullRound,
                true,
                1,
                ammunition);
            Assertions.Equal(ammunition, profile.Ammunition, "Reload ammunition identity was not retained.");
        }

        private static void AssertAvailable(
            FirearmActionKind action,
            FirearmState state,
            bool hasResources)
        {
            FirearmActionDecision decision =
                FirearmActionPolicy.Evaluate(action, EarlyMusket(), state, hasResources);
            Assertions.True(decision.IsAvailable, decision.Reason);
            Assertions.Equal(action, decision.Action, "Generic action identity mismatch.");
        }

        private static void AssertRejected(
            FirearmActionKind action,
            FirearmState state,
            bool hasResources)
        {
            FirearmActionDecision decision =
                FirearmActionPolicy.Evaluate(action, EarlyMusket(), state, hasResources);
            Assertions.False(decision.IsAvailable, "Generic action unexpectedly available.");
        }

        private static FirearmState Empty(FirearmCondition condition)
        {
            return new FirearmState(FirearmState.CurrentSchemaVersion, 0, null, condition);
        }

        private static FirearmState Loaded(FirearmCondition condition)
        {
            return new FirearmState(
                FirearmState.CurrentSchemaVersion,
                1,
                FirearmStateTokenCatalog.DiagnosticLeadBall,
                condition);
        }
    }
}
