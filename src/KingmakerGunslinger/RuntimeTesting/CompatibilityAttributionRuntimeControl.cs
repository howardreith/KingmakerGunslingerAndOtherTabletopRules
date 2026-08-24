using System;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Compatibility;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Activates compatibility-audit controls only after the complete guarded
    /// runtime request has passed the ordinary request parser. State is
    /// process-local and cannot be persisted into a save or player setting.
    /// </summary>
    internal static class CompatibilityAttributionRuntimeControl
    {
        private static readonly object Sync = new object();
        private static CompatibilityAssetAttributionPlan _assetPlan;
        private static string _runId = string.Empty;

        internal static bool AssetAttributionActive
        {
            get { lock (Sync) return _assetPlan != null; }
        }

        internal static string AssetConfiguration
        {
            get
            {
                lock (Sync) return _assetPlan == null
                    ? "ordinary-gameplay" : _assetPlan.Configuration;
            }
        }

        internal static string RunId
        {
            get { lock (Sync) return _runId; }
        }

        internal static void TryActivateEarly(ModContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            RuntimeTestRequestDecision decision = RuntimeTestRequestParser
                .TryActivate(Environment.GetCommandLineArgs(),
                    context.ModEntry.Info.Version);
            if (!decision.Accepted || decision.Request == null ||
                !string.Equals(decision.Request.Scenario,
                    RuntimeTestScenarioCatalog
                        .ObserveKmgCompatibilityAssetAttribution,
                    StringComparison.Ordinal))
                return;

            string configuration = decision.Request.Parameters == null
                ? null
                : (string)decision.Request.Parameters["assetConfiguration"];
            CompatibilityAssetAttributionPlan plan;
            if (!CompatibilityAssetAttributionPlan.TryResolve(
                configuration, out plan))
                return;

            lock (Sync)
            {
                _assetPlan = plan;
                _runId = decision.Request.RunId;
            }
            context.Logger.Info("compatibility-attribution",
                "asset-control.activated",
                "runId=" + decision.Request.RunId +
                ";configuration=" + plan.Configuration +
                ";processLocal=true;saveState=false");
        }

        internal static bool IsAssetFamilyEnabled(
            CompatibilityAssetFamily family)
        {
            lock (Sync)
            {
                return _assetPlan == null || _assetPlan.IsEnabled(family);
            }
        }
    }
}
