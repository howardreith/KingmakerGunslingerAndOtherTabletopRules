using System;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Same complete request-parser boundary as the other early controls. Only
    /// the guarded blocked-family observation, with automatic exit, can ask
    /// the alternate summoning coordinator to treat Master Summoner as
    /// unadaptable; no setting, save flag or UI input can.
    /// </summary>
    internal static class AlternateSummoningFaultDrillControl
    {
        internal const string DrilledFamily = AlternateSummoningSourceCatalog.MasterSummonerKey;

        internal static void TryActivateEarly(ModContext context)
        {
            var decision = RuntimeTestRequestParser.TryActivate(
                Environment.GetCommandLineArgs(), context.ModEntry.Info.Version);
            if (!decision.Accepted || decision.Request == null ||
                decision.Request.Scenario != RuntimeTestScenarioCatalog
                    .ObserveAlternateSummoningBlockedFamily) return;
            if (!decision.Request.ExitAfterCompletion)
                throw new InvalidOperationException(
                    "The blocked-family drill requires guarded process exit.");
            AlternateSummoningCoordinator.ArmFaultDrill(DrilledFamily,
                "guarded-runtime-request:" + decision.Request.RunId);
            context.Logger.Info("summoning", "alternate-sources.fault-drill-armed",
                "runId=" + decision.Request.RunId + ";family=" + DrilledFamily);
        }
    }
}
