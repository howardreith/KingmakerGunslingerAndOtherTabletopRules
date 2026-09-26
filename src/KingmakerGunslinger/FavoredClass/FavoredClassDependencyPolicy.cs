using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace KingmakerGunslinger.FavoredClass
{
    /// <summary>What a failed load's missing blueprint references are, as far as KMG can prove.</summary>
    internal sealed class FavoredClassDependencyReport
    {
        internal FavoredClassDependencyReport(int total, int host, int callOfTheWild, int owned,
            FavoredClassIntegrationAvailability availability, IList<string> hostExamples)
        {
            Total = total;
            Host = host;
            CallOfTheWild = callOfTheWild;
            Owned = owned;
            Availability = availability;
            HostExamples = hostExamples ?? new string[0];
        }

        internal int Total { get; private set; }

        /// <summary>The host's favored class choice, per-class favored progressions and bonus selections.</summary>
        internal int Host { get; private set; }

        /// <summary>Call of the Wild content named by KMG's manifests (the Oracle and its revelations).</summary>
        internal int CallOfTheWild { get; private set; }

        /// <summary>KMG's own favored-class identities (always registered, so never expected here).</summary>
        internal int Owned { get; private set; }

        internal int Unknown { get { return Total - Host - CallOfTheWild - Owned; } }

        internal FavoredClassIntegrationAvailability Availability { get; private set; }

        internal IList<string> HostExamples { get; private set; }

        /// <summary>Whether the failure involves the favored-class dependencies at all.</summary>
        internal bool Relevant { get { return Host > 0 || CallOfTheWild > 0; } }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "missing={0};host={1};callOfTheWild={2};owned={3};unknown={4};hostState={5};examples={6}",
                Total, Host, CallOfTheWild, Owned, Unknown, Availability, string.Join(",", HostExamples.ToArray()));
        }
    }

    /// <summary>
    /// L06 policy: which missing references KMG can attribute, and the
    /// recovery warning built from them (game-independent).
    /// </summary>
    internal static class FavoredClassDependencyPolicy
    {
        /// <summary>The host's level-1 favored class choice.</summary>
        internal const string FavoredClassChoiceGuid = "27947ef789544982a437200c3189c59a";

        /// <summary>Classifies missing references against what KMG can prove about their providers.</summary>
        internal static FavoredClassDependencyReport Classify(IList<string> missing,
            FavoredClassIntegrationAvailability availability, IEnumerable<string> classGuids)
        {
            HashSet<string> host = HostIdentities(classGuids);
            HashSet<string> callOfTheWild = CallOfTheWildIdentities();
            var owned = new HashSet<string>(FavoredClassIdentityCatalog.All.Select(value => value.Guid),
                StringComparer.Ordinal);
            int hostCount = 0, cotwCount = 0, ownedCount = 0;
            var examples = new List<string>();
            foreach (string guid in missing.Distinct(StringComparer.Ordinal))
            {
                if (host.Contains(guid))
                {
                    hostCount++;
                    if (examples.Count < 3)
                        examples.Add(guid == FavoredClassChoiceGuid ? "favored-class-choice" : guid);
                }
                else if (callOfTheWild.Contains(guid))
                    cotwCount++;
                else if (owned.Contains(guid))
                    ownedCount++;
            }
            return new FavoredClassDependencyReport(missing.Distinct(StringComparer.Ordinal).Count(), hostCount,
                cotwCount, ownedCount, availability, examples);
        }

        /// <summary>The player-facing recovery warning.</summary>
        internal static string Compose(FavoredClassDependencyReport report)
        {
            var parts = new List<string>();
            if (report.Host > 0)
                parts.Add(report.Host.ToString(CultureInfo.InvariantCulture) + " from Favored Class (ZFavoredClass " +
                    FavoredClassHostContract.VerifiedHostModVersion + ", which is " + HostState(report.Availability) + ")");
            if (report.CallOfTheWild > 0)
                parts.Add(report.CallOfTheWild.ToString(CultureInfo.InvariantCulture) +
                    " from Call of the Wild 1.14.4c");
            if (report.Unknown > 0)
                parts.Add(report.Unknown.ToString(CultureInfo.InvariantCulture) +
                    " that Kingmaker Gunslinger cannot attribute to a mod");
            return "Kingmaker Gunslinger: this save uses favored-class content that is not loaded. " +
                report.Total.ToString(CultureInfo.InvariantCulture) + " of its blueprint references are missing: " +
                string.Join(", ", parts.ToArray()) + ". Nothing was loaded and the save file was not changed. " +
                "To recover, install and enable ZFavoredClass " + FavoredClassHostContract.VerifiedHostModVersion +
                " with Call of the Wild 1.14.4c in Unity Mod Manager, restart the game and load this save again. " +
                "Until then, do not save over it: its favored class choices exist only in that content. Your " +
                "Kingmaker Gunslinger favored-class investments stay recorded in the save.";
        }

        internal static string HostState(FavoredClassIntegrationAvailability availability)
        {
            switch (availability)
            {
                case FavoredClassIntegrationAvailability.HostAbsent:
                    return "not installed or not loaded";
                case FavoredClassIntegrationAvailability.HostDisabled:
                    return "disabled in Unity Mod Manager";
                case FavoredClassIntegrationAvailability.UnsupportedBinary:
                    return "installed but not the supported build";
                case FavoredClassIntegrationAvailability.HostIncomplete:
                    return "installed but did not finish initializing";
                default:
                    return "loaded without these blueprints";
            }
        }

        /// <summary>
        /// The host's identities KMG can prove: its favored class choice and,
        /// for every class KMG knows, the per-class favored progression and
        /// bonus selection (the host's MergeIds rule, checked by the readiness
        /// gate whenever the host is loaded).
        /// </summary>
        private static HashSet<string> HostIdentities(IEnumerable<string> classGuids)
        {
            var result = new HashSet<string>(StringComparer.Ordinal) { FavoredClassChoiceGuid };
            foreach (string classGuid in classGuids ?? new string[0])
            {
                try
                {
                    result.Add(FavoredClassHostContract.ExpectedProgressionGuid(classGuid));
                    result.Add(FavoredClassHostContract.ExpectedBonusSelectionGuid(classGuid));
                }
                catch (FormatException)
                {
                }
            }
            return result;
        }

        /// <summary>The Call of the Wild identities KMG's manifests name (the Oracle and its revelations).</summary>
        private static HashSet<string> CallOfTheWildIdentities()
        {
            var result = new HashSet<string>(StringComparer.Ordinal)
            {
                FavoredClassRevelationManifest.OracleClassGuid,
                FavoredClassRevelationManifest.RavenerHunterArchetypeGuid
            };
            foreach (FavoredClassRevelationTarget target in FavoredClassRevelationManifest.All)
                foreach (string guid in target.FeatureGuids)
                    result.Add(guid);
            return result;
        }
    }
}
