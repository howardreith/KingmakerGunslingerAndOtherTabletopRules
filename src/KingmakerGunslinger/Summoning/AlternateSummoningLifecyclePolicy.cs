using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>The audit disposition of one adapted route.</summary>
    internal enum AlternateSummoningDisposition
    {
        /// <summary>Not yet reconciled in this process.</summary>
        Pending,
        /// <summary>Identities registered and the host menus published.</summary>
        Published,
        /// <summary>Identities registered for saved references; the module is
        /// OFF, so no host menu was changed.</summary>
        RegisteredModuleOff,
        /// <summary>The providing mod is not installed in this profile.</summary>
        AbsentInProfile,
        /// <summary>The provider is present but its graph or state did not
        /// match; the host keeps its original behavior.</summary>
        Blocked
    }

    internal enum AlternateSummoningDependencyState
    {
        Absent,
        Active,
        /// <summary>Installed but not loaded, inactive, assembly-less or
        /// errored.</summary>
        Inactive,
        /// <summary>More than one entry claims the provider's mod ID.</summary>
        Ambiguous
    }

    internal enum AlternateSummoningLifecycleAction
    {
        Wait,
        Block,
        Absent,
        Reconcile
    }

    internal sealed class AlternateSummoningLifecycleDecision
    {
        internal AlternateSummoningLifecycleDecision(
            AlternateSummoningLifecycleAction action, string reason, bool publish)
        { Action = action; Reason = reason; Publish = publish; }

        internal AlternateSummoningLifecycleAction Action { get; private set; }
        internal string Reason { get; private set; }
        /// <summary>Whether reconciliation may change host menus, rather
        /// than only registering identities.</summary>
        internal bool Publish { get; private set; }
    }

    /// <summary>
    /// When an adapter family may reconcile. It waits for the package's own
    /// blueprint lifecycle and roster, never loads a provider's types, keeps
    /// an absent provider absent, and blocks - leaving the host untouched -
    /// on any provider state it cannot vouch for. The module setting decides
    /// only whether menus change; identities register either way.
    /// </summary>
    internal static class AlternateSummoningLifecyclePolicy
    {
        /// <summary>How long the first-update reconciliation keeps waiting for
        /// the package blueprint lifecycle before it reports a block. Waiting
        /// costs a few reads per frame; the bound only ends an attachment
        /// that could otherwise never settle.</summary>
        internal const int MaximumPendingSeconds = 600;

        internal static AlternateSummoningLifecycleDecision Decide(
            AlternateSummoningProvider provider, bool bootstrapInitialized,
            bool bootstrapFailed, bool canonicalRosterReady,
            AlternateSummoningDependencyState dependency, bool moduleActive,
            bool canonicalPublished)
        {
            if (bootstrapFailed)
                return Block("package-blueprint-lifecycle-failed");
            if (!bootstrapInitialized || !canonicalRosterReady)
                return new AlternateSummoningLifecycleDecision(
                    AlternateSummoningLifecycleAction.Wait,
                    "awaiting-package-blueprint-lifecycle", false);
            if (provider == AlternateSummoningProvider.CallOfTheWild)
            {
                if (dependency == AlternateSummoningDependencyState.Absent)
                    return new AlternateSummoningLifecycleDecision(
                        AlternateSummoningLifecycleAction.Absent,
                        "call-of-the-wild-not-installed", false);
                if (dependency == AlternateSummoningDependencyState.Ambiguous)
                    return Block("call-of-the-wild-entry-ambiguous");
                if (dependency != AlternateSummoningDependencyState.Active)
                    return Block("call-of-the-wild-not-active");
            }
            if (moduleActive && !canonicalPublished)
                return Block("canonical-publication-missing-while-module-on");
            return new AlternateSummoningLifecycleDecision(
                AlternateSummoningLifecycleAction.Reconcile,
                moduleActive ? "module-on" : "module-off", moduleActive);
        }

        private static AlternateSummoningLifecycleDecision Block(string reason)
        {
            return new AlternateSummoningLifecycleDecision(
                AlternateSummoningLifecycleAction.Block, reason, false);
        }
    }

    internal sealed class AlternateSummoningFamilyStatus
    {
        internal AlternateSummoningFamilyStatus(string familyKey,
            AlternateSummoningDisposition disposition, string reason,
            int roots, int registered, int exposed, int keptHost,
            int removedHost, long milliseconds, string evidence)
        {
            FamilyKey = familyKey; Disposition = disposition; Reason = reason;
            Roots = roots; Registered = registered; Exposed = exposed;
            KeptHost = keptHost; RemovedHost = removedHost;
            Milliseconds = milliseconds; Evidence = evidence;
        }

        internal string FamilyKey { get; private set; }
        internal AlternateSummoningDisposition Disposition { get; private set; }
        internal string Reason { get; private set; }
        internal int Roots { get; private set; }
        internal int Registered { get; private set; }
        internal int Exposed { get; private set; }
        internal int KeptHost { get; private set; }
        internal int RemovedHost { get; private set; }
        internal long Milliseconds { get; private set; }
        internal string Evidence { get; private set; }

        public override string ToString()
        {
            return FamilyKey + ":" + Disposition + ":" + (Reason ?? "") +
                ";roots=" + Roots + ";registered=" + Registered + ";exposed=" +
                Exposed + ";keptHost=" + KeptHost + ";removedHost=" +
                RemovedHost + ";ms=" + Milliseconds +
                (string.IsNullOrEmpty(Evidence) ? "" : ";" + Evidence);
        }
    }

    internal static class AlternateSummoningStatusRegistry
    {
        private static readonly object Gate = new object();
        private static readonly Dictionary<string, AlternateSummoningFamilyStatus>
            Statuses = new Dictionary<string, AlternateSummoningFamilyStatus>(
                StringComparer.Ordinal);

        internal static void Update(AlternateSummoningFamilyStatus status)
        {
            if (status == null) throw new ArgumentNullException("status");
            lock (Gate) Statuses[status.FamilyKey] = status;
        }

        internal static IReadOnlyList<AlternateSummoningFamilyStatus> Snapshot()
        {
            lock (Gate) return Statuses.Values.OrderBy(value => value.FamilyKey,
                StringComparer.Ordinal).ToArray();
        }

        internal static AlternateSummoningFamilyStatus Find(string familyKey)
        {
            AlternateSummoningFamilyStatus status;
            lock (Gate) return Statuses.TryGetValue(familyKey, out status) ?
                status : null;
        }

        internal static void ResetForTests()
        {
            lock (Gate) Statuses.Clear();
        }
    }
}
