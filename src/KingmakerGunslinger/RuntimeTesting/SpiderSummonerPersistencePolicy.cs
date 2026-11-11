namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// The Spider Summoner (Races Unleashed) Working-save sequence. Four fresh
    /// processes share one prepared checkpoint:
    /// <list type="number">
    /// <item>prepare, Expanded Summoning ON: the fixture caster, its two pinned
    /// shortcuts, the active marked Black Widow and an active marked Giant
    /// Spider are saved;</item>
    /// <item>verify-cleanup with <c>spiderCheckpoint=preserve</c>, Expanded
    /// Summoning OFF: the disabled-module reload of that checkpoint. It writes
    /// nothing, so the checkpoint stays exactly as prepared;</item>
    /// <item>verify-cleanup, ON: the enabled reload of the same checkpoint,
    /// then the fixture's retirement and the cleanup save;</item>
    /// <item>verify-absent, ON: a fresh load finds nothing of the fixture.</item>
    /// </list>
    /// The disabled reload therefore runs before the final cleanup, and the
    /// enabled verification is the checkpoint's last consumer.
    /// </summary>
    internal static class SpiderSummonerPersistencePolicy
    {
        internal const string Scope = "spider-summoner";
        internal const string CheckpointParameter = "spiderCheckpoint";
        internal const string Preserve = "preserve";
        internal const string ReceiptScope = "KMG_SpiderSummoner_Persistence_v1";
        internal const string CasterRole = "caster";
        internal const string SummonRole = "summon";
        /// <summary>The saved marked Giant Spider whose web the disabled
        /// reload exercises; the Black Widow has no web.</summary>
        internal const string WebSpiderRole = "web-spider";

        /// <summary>Whether the request's stage and parameters are a stage of
        /// this sequence. The module state is checked in the game.</summary>
        internal static bool Admits(string scenario, string scope, bool checkpointPresent, string checkpoint)
        {
            if (scope != Scope) return !checkpointPresent;
            if (!checkpointPresent) return IsStage(scenario);
            return scenario == RuntimeTestScenarioCatalog.WorkingSaveExpandedSummoningVerifyCleanup && checkpoint == Preserve;
        }

        internal static bool IsStage(string scenario)
        {
            return scenario == RuntimeTestScenarioCatalog.WorkingSaveExpandedSummoningPrepare ||
                scenario == RuntimeTestScenarioCatalog.WorkingSaveExpandedSummoningVerifyCleanup ||
                scenario == RuntimeTestScenarioCatalog.WorkingSaveExpandedSummoningVerifyAbsent;
        }

        /// <summary>Whether the stage writes the Working save: prepare and the
        /// enabled cleanup do; the preserving disabled reload and the absence
        /// check do not.</summary>
        internal static bool Writes(string scenario, bool preserve)
        {
            return scenario == RuntimeTestScenarioCatalog.WorkingSaveExpandedSummoningPrepare ||
                (scenario == RuntimeTestScenarioCatalog.WorkingSaveExpandedSummoningVerifyCleanup && !preserve);
        }

        /// <summary>Null when the loaded module state is the one this stage
        /// requires, otherwise why not.</summary>
        internal static string StageProblem(string scenario, bool preserve, bool moduleOn)
        {
            if (!IsStage(scenario)) return "not-a-stage:" + scenario;
            if (scenario == RuntimeTestScenarioCatalog.WorkingSaveExpandedSummoningVerifyCleanup && preserve)
                return moduleOn ? "preserving-reload-requires-module-off" : null;
            if (preserve) return "preserve-only-on-verify-cleanup";
            return moduleOn ? null : "stage-requires-module-on";
        }
    }
}
