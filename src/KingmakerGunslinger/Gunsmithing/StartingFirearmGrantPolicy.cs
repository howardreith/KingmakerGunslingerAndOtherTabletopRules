using System;

namespace KingmakerGunslinger.Gunsmithing
{
    internal enum StartingFirearmGrantDisposition
    {
        None,
        ReconcileReceipt,
        Grant
    }

    internal sealed class StartingFirearmGrantDecision
    {
        internal StartingFirearmGrantDecision(
            StartingFirearmGrantDisposition disposition, string status)
        {
            Disposition = disposition;
            Status = status;
        }

        internal StartingFirearmGrantDisposition Disposition
        { get; private set; }
        internal string Status { get; private set; }
    }

    internal static class StartingFirearmGrantPolicy
    {
        internal static bool IsCommittedCharacterCreation(
            bool isFirstLevel, bool isCharacterCreationMode)
        {
            return isFirstLevel && isCharacterCreationMode;
        }

        internal static StartingFirearmGrantDecision Decide(
            bool moduleEnabled, bool playerControlled, int priorGunslingerLevel,
            int currentGunslingerLevel, bool hasDurableReceipt,
            int ownerBoundStarterCount)
        {
            if (priorGunslingerLevel < 0)
                throw new ArgumentOutOfRangeException("priorGunslingerLevel");
            if (currentGunslingerLevel < 0)
                throw new ArgumentOutOfRangeException("currentGunslingerLevel");
            if (ownerBoundStarterCount < 0)
                throw new ArgumentOutOfRangeException("ownerBoundStarterCount");
            if (!moduleEnabled)
                return None("module-disabled");
            if (!playerControlled)
                return None("receiver-not-player-controlled");
            if (priorGunslingerLevel != 0 || currentGunslingerLevel != 1)
                return None("not-first-gunslinger-level-transition");
            if (hasDurableReceipt)
                return None("durable-receipt-present");
            if (ownerBoundStarterCount > 1)
                throw new InvalidOperationException(
                    "A unit without a durable receipt has multiple owner-bound starter firearms.");
            if (ownerBoundStarterCount == 1)
                return new StartingFirearmGrantDecision(
                    StartingFirearmGrantDisposition.ReconcileReceipt,
                    "owner-bound-starter-receipt-missing");
            return new StartingFirearmGrantDecision(
                StartingFirearmGrantDisposition.Grant,
                "first-gunslinger-level-committed");
        }

        private static StartingFirearmGrantDecision None(string status)
        {
            return new StartingFirearmGrantDecision(
                StartingFirearmGrantDisposition.None, status);
        }
    }
}
