using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic;

namespace KingmakerGunslinger.FavoredClass
{
    /// <summary>
    /// The effective profile of the favored-class integration for this
    /// process. Publication changes are restart-required, so the profile is
    /// fixed once the first update resolves it.
    /// </summary>
    internal sealed class FavoredClassProfileState
    {
        internal FavoredClassProfileState(bool integrationEnabled, bool firstParty,
            bool adaptations, bool thirdParty, bool mostlyHuman)
        {
            IntegrationEnabled = integrationEnabled;
            FirstParty = firstParty;
            Adaptations = adaptations;
            ThirdParty = thirdParty;
            MostlyHuman = mostlyHuman;
        }

        /// <summary>
        /// Charter defaults: integration, faithful catalog, adaptations and
        /// Mostly Human on; third-party profile off.
        /// </summary>
        internal static FavoredClassProfileState Defaults
        {
            get { return new FavoredClassProfileState(true, true, true, false, true); }
        }

        internal bool IntegrationEnabled { get; private set; }
        internal bool FirstParty { get; private set; }
        internal bool Adaptations { get; private set; }
        internal bool ThirdParty { get; private set; }
        internal bool MostlyHuman { get; private set; }

        /// <summary>Whether new choices may be offered through a route of this profile.</summary>
        internal bool Offers(FavoredClassProfile profile)
        {
            if (!IntegrationEnabled)
                return false;
            switch (profile)
            {
                case FavoredClassProfile.FirstParty:
                    return FirstParty;
                case FavoredClassProfile.Adaptation:
                    return Adaptations;
                case FavoredClassProfile.ThirdParty:
                    return ThirdParty;
                default:
                    return false;
            }
        }
    }

    /// <summary>
    /// Process-local runtime state shared by the owned prerequisites and
    /// mechanics. It only reads unit facts; it never mutates a unit.
    /// </summary>
    internal static class FavoredClassRuntime
    {
        /// <summary>Stable permission-graph identity of the verified Mostly Human fact.</summary>
        internal const string MostlyHumanFactId = "KMG.ElementalRaces.MostlyHuman.Identity";

        private static readonly object Gate = new object();
        private static FavoredClassProfileState _profile = FavoredClassProfileState.Defaults;
        private static BlueprintFeature _mostlyHumanIdentity;
        private static readonly FavoredClassPermissionGraph Graph =
            FavoredClassPermissionGraph.CreateVerified(MostlyHumanFactId);

        internal static FavoredClassProfileState Profile
        {
            get { lock (Gate) return _profile; }
        }

        /// <summary>
        /// Owned numerical effects are suppressed only when the whole
        /// integration is disabled. Disabling a profile stops new choices but
        /// keeps already-earned choices working.
        /// </summary>
        internal static bool MechanicsEnabled
        {
            get { lock (Gate) return _profile.IntegrationEnabled; }
        }

        internal static void ConfigureProfile(FavoredClassProfileState profile)
        {
            if (profile == null)
                throw new ArgumentNullException("profile");
            lock (Gate)
                _profile = profile;
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
            return Graph.PermittedAncestries(Evidence(unit));
        }

        /// <summary>Whether this unit may take new choices of the effect.</summary>
        internal static bool IsEffectEligible(string effectId, UnitDescriptor unit)
        {
            FavoredClassEffectSpec effect = FavoredClassCatalog.Effect(effectId);
            FavoredClassProfileState profile = Profile;
            return FavoredClassEligibility.IsEligible(effect, PermittedAncestries(unit),
                profile.Offers, ancestry => true);
        }
    }
}
