using System;
using KingmakerGunslinger.Ammunition;
using KingmakerGunslinger.Presentation;

namespace KingmakerGunslinger.Reloading
{
    /// <summary>Maps technical reload decisions to short player-facing text.</summary>
    internal static class ReloadPlayerFacingReasonPolicy
    {
        [ThreadStatic] private static string _lastReason;

        internal static string ForPlan(FirearmReloadPlan plan)
        {
            if (plan == null) return "Cannot reload now.";
            switch (plan.Status)
            {
                case FirearmReloadPlanStatus.Wrecked:
                    return "That firearm is wrecked.";
                case FirearmReloadPlanStatus.AlreadyLoaded:
                    return "That firearm is already loaded.";
                case FirearmReloadPlanStatus.MissingAmmunition:
                    return plan.Profile != null && plan.Profile.SourceKind ==
                        ReloadAmmunitionSourceKind.PaperCartridge ?
                        "No paper cartridges." : "No loose ammunition.";
                case FirearmReloadPlanStatus.MixedAmmunition:
                    return "That firearm has different ammunition loaded.";
                case FirearmReloadPlanStatus.IncompatibleAmmunition:
                    return "That ammunition cannot reload this firearm.";
                default:
                    return "Cannot reload now.";
            }
        }

        internal static string ForExactFirearmFailure()
        { return "Equip one firearm."; }

        internal static string ForQueuedPlanChange()
        { return "Reload setup changed. Try again."; }

        internal static void Remember(string reason)
        {
            _lastReason = PlayerFacingTextPolicy.IsScreenSafe(reason) ?
                reason : "Cannot reload now.";
        }

        internal static string CurrentOrFallback()
        {
            return PlayerFacingTextPolicy.IsScreenSafe(_lastReason) ?
                _lastReason : "Cannot reload now.";
        }
    }
}
