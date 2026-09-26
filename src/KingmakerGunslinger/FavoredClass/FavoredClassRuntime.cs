using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic;

namespace KingmakerGunslinger.FavoredClass
{
    /// <summary>
    /// Process-local runtime state shared by the owned prerequisites and
    /// mechanics. It only reads unit facts; it never mutates a unit.
    /// </summary>
    internal static class FavoredClassRuntime
    {
        /// <summary>Stable permission-graph identity of the verified Mostly Human fact.</summary>
        internal const string MostlyHumanFactId =
            KingmakerGunslinger.ElementalRaces.ElementalMostlyHumanPolicy.IdentitySymbol;

        private static readonly object Gate = new object();
        private static FavoredClassProfileState _profile = FavoredClassProfileState.Defaults;
        private static BlueprintFeature _mostlyHumanIdentity;
        private static readonly FavoredClassPermissionGraph VerifiedGraph =
            FavoredClassPermissionGraph.CreateVerified(MostlyHumanFactId);
        private static FavoredClassPermissionGraph _graph = VerifiedGraph;
        private static readonly FavoredClassHostActivation Activation = new FavoredClassHostActivation();

        /// <summary>
        /// Runtime-test seam (E10): the verified graph plus extra edges (a
        /// cycle, a chain past the depth bound, an edge on an unverified fact)
        /// until the returned scope is disposed. No production path calls it.
        /// </summary>
        internal static IDisposable ExtendPermissionGraphForRuntimeTest(IEnumerable<FavoredClassPermissionEdge> extra)
        {
            if (extra == null)
                throw new ArgumentNullException("extra");
            var graph = new FavoredClassPermissionGraph(VerifiedGraph.Edges.Concat(extra));
            lock (Gate)
            {
                if (!ReferenceEquals(_graph, VerifiedGraph))
                    throw new InvalidOperationException("A permission graph extension is already active.");
                _graph = graph;
            }
            return new PermissionGraphScope();
        }

        private sealed class PermissionGraphScope : IDisposable
        {
            private bool _disposed;

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                lock (Gate)
                    _graph = VerifiedGraph;
            }
        }

        internal static FavoredClassProfileState Profile
        {
            get { lock (Gate) return _profile; }
        }

        /// <summary>
        /// Whether the exact qualified host's owned publication is committed
        /// and live (false until then, and after any failure or rollback).
        /// </summary>
        internal static bool HostActive
        {
            get { return Activation.IsActive; }
        }

        /// <summary>Why the host is (in)active, for diagnostics.</summary>
        internal static string HostActivationReason
        {
            get { return Activation.Reason; }
        }

        /// <summary>
        /// Owned numerical effects need both the enabled integration and a
        /// committed publication of the exact host. Saved ranks always
        /// resolve; without the published host their effects are zero until
        /// it is published again. Disabling a profile stops new choices but
        /// keeps already-earned choices working while the host is published.
        /// </summary>
        internal static bool MechanicsEnabled
        {
            get
            {
                bool integration;
                lock (Gate) integration = _profile.IntegrationEnabled;
                return FavoredClassHostActivation.MechanicsEnabled(integration, Activation.IsActive);
            }
        }

        /// <summary>Only the coordinator, after the exact host's publication committed.</summary>
        internal static void ActivateHost(string reason)
        {
            Activation.Activate(reason);
        }

        /// <summary>Absent, disabled, unsupported, incomplete, failed or rolled-back host states.</summary>
        internal static void DeactivateHost(string reason)
        {
            Activation.Deactivate(reason);
        }

        internal static void ConfigureProfile(FavoredClassProfileState profile)
        {
            if (profile == null)
                throw new ArgumentNullException("profile");
            lock (Gate)
                _profile = profile;
        }

        private static HashSet<string> _unavailableEffects = new HashSet<string>(StringComparer.Ordinal);

        /// <summary>
        /// Effects (or "effect|target" counters) withheld from publication
        /// because their exact native read point failed validation or was not
        /// found in this process (only that counter).
        /// </summary>
        internal static void SetUnavailableEffects(IEnumerable<string> effectIds)
        {
            lock (Gate)
                _unavailableEffects = new HashSet<string>(effectIds ?? new string[0], StringComparer.Ordinal);
        }

        internal static bool IsEffectUnavailable(string effectId)
        {
            lock (Gate)
                return effectId != null && _unavailableEffects.Contains(effectId);
        }

        /// <summary>The key that withholds one target counter of an effect.</summary>
        internal static string TargetKey(string effectId, string targetKey)
        {
            return effectId + "|" + targetKey;
        }

        /// <summary>Whether one target's native read points were not found in this process.</summary>
        internal static bool IsTargetUnavailable(string effectId, string targetKey)
        {
            if (effectId == null || targetKey == null)
                return false;
            lock (Gate)
                return _unavailableEffects.Contains(TargetKey(effectId, targetKey));
        }

        internal static void ConfigureMostlyHumanIdentity(BlueprintFeature identity)
        {
            lock (Gate)
                _mostlyHumanIdentity = identity;
        }

        /// <summary>
        /// Ancestry evidence from the unit's actual race blueprint and
        /// verified facts only. Appearance, portraits, names, creature-type
        /// facts and elemental affinity are never consulted.
        /// </summary>
        internal static FavoredClassAncestryEvidence Evidence(UnitDescriptor unit)
        {
            if (unit == null || unit.Progression == null || unit.Progression.Race == null)
                return new FavoredClassAncestryEvidence(null, null);
            string ancestry = FavoredClassRaceIdentities.AncestryForRaceGuid(
                unit.Progression.Race.AssetGuid);
            List<string> facts = new List<string>();
            BlueprintFeature mostlyHuman;
            lock (Gate)
                mostlyHuman = _mostlyHumanIdentity;
            // The profile toggle only controls whether the trait is offered to
            // new characters; a character who owns the trait keeps its identity.
            if (mostlyHuman != null && unit.Progression.Features.HasFact(mostlyHuman))
                facts.Add(MostlyHumanFactId);
            return new FavoredClassAncestryEvidence(ancestry, facts);
        }

        internal static ISet<string> PermittedAncestries(UnitDescriptor unit)
        {
            FavoredClassPermissionGraph graph;
            lock (Gate)
                graph = _graph;
            return graph.PermittedAncestries(Evidence(unit));
        }

        /// <summary>
        /// The scoped host bridge's only addition to the host's exact-race
        /// human prerequisite (see FavoredClassEligibility.GrantsHostHumanAccess).
        /// </summary>
        internal static bool GrantsHostHumanAccess(UnitDescriptor unit)
        {
            return FavoredClassEligibility.GrantsHostHumanAccess(Evidence(unit), MostlyHumanFactId);
        }

        /// <summary>Whether this unit may take new choices of the effect.</summary>
        internal static bool IsEffectEligible(string effectId, UnitDescriptor unit)
        {
            return IsEffectEligible(effectId, unit, null);
        }

        /// <summary>As above, limited to the source rows that open one target.</summary>
        internal static bool IsEffectEligible(string effectId, UnitDescriptor unit,
            ICollection<string> targetRows)
        {
            FavoredClassEffectSpec effect = FavoredClassCatalog.Effect(effectId);
            FavoredClassProfileState profile = Profile;
            return FavoredClassEligibility.IsEligible(effect, PermittedAncestries(unit),
                profile.Offers, ancestry => true, targetRows);
        }
    }
}
