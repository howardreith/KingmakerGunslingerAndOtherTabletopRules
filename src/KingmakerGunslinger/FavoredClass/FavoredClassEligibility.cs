using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.FavoredClass
{
    /// <summary>Stable ancestry identifiers used by source rows and permissions.</summary>
    internal static class FavoredClassAncestry
    {
        internal const string Human = "human";
        internal const string HalfElf = "half-elf";
        internal const string HalfOrc = "half-orc";
        internal const string Elf = "elf";
        internal const string Dwarf = "dwarf";
        internal const string Gnome = "gnome";
        internal const string Halfling = "halfling";
        internal const string Aasimar = "aasimar";
        internal const string Tiefling = "tiefling";
        internal const string Goblin = "goblin";
        internal const string Hobgoblin = "hobgoblin";
        internal const string Fetchling = "fetchling";
        internal const string Dhampir = "dhampir";
        internal const string Drow = "drow";
        internal const string Duergar = "duergar";
        internal const string Ganzi = "ganzi";
        internal const string Suli = "suli";
        internal const string Ifrit = "ifrit";
        internal const string Oread = "oread";
        internal const string Sylph = "sylph";
        internal const string Undine = "undine";

        /// <summary>Named in source rows but without a qualified provider.</summary>
        internal const string Orc = "orc";
        internal const string Grippli = "grippli";
        internal const string Kobold = "kobold";
        internal const string Ratfolk = "ratfolk";
        internal const string Kitsune = "kitsune";
        internal const string Wayang = "wayang";

        /// <summary>
        /// The 21 source-addressable race identities of charter section 2.4:
        /// nine native, eight optional Races Unleashed and KMG's four
        /// elemental parent races.
        /// </summary>
        internal static readonly string[] SourceAddressable =
        {
            Human, HalfElf, HalfOrc, Elf, Dwarf, Gnome, Halfling, Aasimar, Tiefling,
            Goblin, Hobgoblin, Fetchling, Dhampir, Drow, Duergar, Ganzi, Suli,
            Ifrit, Oread, Sylph, Undine
        };

        internal static readonly string[] ElementalParents = { Ifrit, Oread, Sylph, Undine };
    }

    /// <summary>Why a unit may take another ancestry's favored-class options.</summary>
    internal enum FavoredClassPermissionBasis
    {
        /// <summary>Paizo FAQ: half-elves take human and elf racial options.</summary>
        HalfElfFaq,
        /// <summary>Paizo FAQ: half-orcs take human and orc racial options.</summary>
        HalfOrcFaq,
        /// <summary>
        /// Preserved Favored Class host policy granting human options to
        /// Aasimar and Tieflings (a compatibility decision, not a tabletop rule).
        /// </summary>
        HostHumanPolicy,
        /// <summary>KMG's Mostly Human alternate racial trait on a geniekin parent.</summary>
        MostlyHuman
    }

    /// <summary>One conditional permission edge of the ancestry graph.</summary>
    internal sealed class FavoredClassPermissionEdge
    {
        internal FavoredClassPermissionEdge(string from, string to,
            FavoredClassPermissionBasis basis, string requiredFact)
        {
            From = from;
            To = to;
            Basis = basis;
            RequiredFact = requiredFact;
        }

        internal string From { get; private set; }
        internal string To { get; private set; }
        internal FavoredClassPermissionBasis Basis { get; private set; }

        /// <summary>
        /// A verified fact identity the unit must own for the edge to apply;
        /// null for an unconditional racial permission.
        /// </summary>
        internal string RequiredFact { get; private set; }
    }

    /// <summary>A unit's ancestry evidence, independent of Kingmaker types.</summary>
    internal sealed class FavoredClassAncestryEvidence
    {
        internal FavoredClassAncestryEvidence(string nativeAncestry,
            IEnumerable<string> verifiedFacts)
        {
            NativeAncestry = nativeAncestry;
            VerifiedFacts = new HashSet<string>(verifiedFacts ?? Enumerable.Empty<string>(),
                StringComparer.Ordinal);
        }

        /// <summary>
        /// The ancestry of the unit's actual race blueprint, or null when the
        /// race is not a recognized identity. Appearance never supplies it.
        /// </summary>
        internal string NativeAncestry { get; private set; }

        internal HashSet<string> VerifiedFacts { get; private set; }
    }

    /// <summary>
    /// Permission union for favored-class eligibility (charter section 5).
    /// It widens which options a unit may choose among; it never changes how
    /// many rewards the unit receives. Unknown ancestries and unverified
    /// providers fail closed, and traversal is bounded and cycle-safe.
    /// </summary>
    internal sealed class FavoredClassPermissionGraph
    {
        internal const int MaximumDepth = 4;

        private readonly FavoredClassPermissionEdge[] _edges;

        internal FavoredClassPermissionGraph(IEnumerable<FavoredClassPermissionEdge> edges)
        {
            if (edges == null)
                throw new ArgumentNullException("edges");
            _edges = edges.ToArray();
        }

        internal IList<FavoredClassPermissionEdge> Edges
        {
            get { return Array.AsReadOnly(_edges); }
        }

        /// <summary>The verified graph used by the published integration.</summary>
        internal static FavoredClassPermissionGraph CreateVerified(string mostlyHumanFactId)
        {
            if (string.IsNullOrEmpty(mostlyHumanFactId))
                throw new ArgumentException("A verified Mostly Human fact identity is required.",
                    "mostlyHumanFactId");
            List<FavoredClassPermissionEdge> edges = new List<FavoredClassPermissionEdge>
            {
                new FavoredClassPermissionEdge(FavoredClassAncestry.HalfElf,
                    FavoredClassAncestry.Human, FavoredClassPermissionBasis.HalfElfFaq, null),
                new FavoredClassPermissionEdge(FavoredClassAncestry.HalfElf,
                    FavoredClassAncestry.Elf, FavoredClassPermissionBasis.HalfElfFaq, null),
                new FavoredClassPermissionEdge(FavoredClassAncestry.HalfOrc,
                    FavoredClassAncestry.Human, FavoredClassPermissionBasis.HalfOrcFaq, null),
                new FavoredClassPermissionEdge(FavoredClassAncestry.HalfOrc,
                    FavoredClassAncestry.Orc, FavoredClassPermissionBasis.HalfOrcFaq, null),
                new FavoredClassPermissionEdge(FavoredClassAncestry.Aasimar,
                    FavoredClassAncestry.Human, FavoredClassPermissionBasis.HostHumanPolicy, null),
                new FavoredClassPermissionEdge(FavoredClassAncestry.Tiefling,
                    FavoredClassAncestry.Human, FavoredClassPermissionBasis.HostHumanPolicy, null),
            };
            foreach (string parent in FavoredClassAncestry.ElementalParents)
                edges.Add(new FavoredClassPermissionEdge(parent, FavoredClassAncestry.Human,
                    FavoredClassPermissionBasis.MostlyHuman, mostlyHumanFactId));
            return new FavoredClassPermissionGraph(edges);
        }

        /// <summary>
        /// Every ancestry whose options the unit may choose: its native
        /// ancestry plus ancestries reachable through edges whose conditions
        /// the unit satisfies. Deterministic, cycle-safe and bounded.
        /// </summary>
        internal ISet<string> PermittedAncestries(FavoredClassAncestryEvidence evidence)
        {
            HashSet<string> visited = new HashSet<string>(StringComparer.Ordinal);
            if (evidence == null || string.IsNullOrEmpty(evidence.NativeAncestry))
                return visited;
            Queue<KeyValuePair<string, int>> pending = new Queue<KeyValuePair<string, int>>();
            visited.Add(evidence.NativeAncestry);
            pending.Enqueue(new KeyValuePair<string, int>(evidence.NativeAncestry, 0));
            while (pending.Count > 0)
            {
                KeyValuePair<string, int> current = pending.Dequeue();
                if (current.Value >= MaximumDepth)
                    continue;
                foreach (FavoredClassPermissionEdge edge in _edges)
                {
                    if (!string.Equals(edge.From, current.Key, StringComparison.Ordinal))
                        continue;
                    if (edge.RequiredFact != null &&
                        !evidence.VerifiedFacts.Contains(edge.RequiredFact))
                        continue;
                    if (visited.Add(edge.To))
                        pending.Enqueue(new KeyValuePair<string, int>(edge.To, current.Value + 1));
                }
            }
            return visited;
        }
    }

    /// <summary>Which enabled source routes make an effect available to a unit.</summary>
    internal static class FavoredClassEligibility
    {
        /// <summary>
        /// The only permission the scoped host ancestry bridge adds to the
        /// host's exact-race human prerequisite: a geniekin parent that owns
        /// the verified Mostly Human fact. Half-elf, Half-orc, Aasimar,
        /// Tiefling and every other race keep exactly the host's own policy,
        /// and an unrecognized race or unverified fact grants nothing.
        /// </summary>
        internal static bool GrantsHostHumanAccess(FavoredClassAncestryEvidence evidence,
            string mostlyHumanFactId)
        {
            return evidence != null && !string.IsNullOrEmpty(mostlyHumanFactId) &&
                evidence.NativeAncestry != null &&
                FavoredClassAncestry.ElementalParents.Contains(evidence.NativeAncestry) &&
                evidence.VerifiedFacts.Contains(mostlyHumanFactId);
        }

        /// <summary>
        /// The eligible routes of one canonical effect for this unit. The
        /// effect is offered once whenever the list is non-empty, regardless
        /// of how many routes qualify, so duplicate ancestry routes can never
        /// multiply a menu leaf or a reward.
        /// </summary>
        internal static IList<FavoredClassSourceRow> EligibleRoutes(
            FavoredClassEffectSpec effect, ISet<string> permittedAncestries,
            Func<FavoredClassProfile, bool> profileEnabled,
            Func<string, bool> ancestryProviderPresent)
        {
            if (effect == null)
                throw new ArgumentNullException("effect");
            if (permittedAncestries == null)
                throw new ArgumentNullException("permittedAncestries");
            if (profileEnabled == null)
                throw new ArgumentNullException("profileEnabled");
            if (ancestryProviderPresent == null)
                throw new ArgumentNullException("ancestryProviderPresent");
            List<FavoredClassSourceRow> routes = new List<FavoredClassSourceRow>();
            foreach (string id in effect.Rows)
            {
                FavoredClassSourceRow row = FavoredClassCatalog.Row(id);
                if (!row.IsScheduled)
                    continue;
                if (!profileEnabled(row.Profile))
                    continue;
                if (!ancestryProviderPresent(row.Ancestry))
                    continue;
                if (!permittedAncestries.Contains(row.Ancestry))
                    continue;
                routes.Add(row);
            }
            return routes.AsReadOnly();
        }

        internal static bool IsEligible(FavoredClassEffectSpec effect,
            ISet<string> permittedAncestries, Func<FavoredClassProfile, bool> profileEnabled,
            Func<string, bool> ancestryProviderPresent)
        {
            return EligibleRoutes(effect, permittedAncestries, profileEnabled,
                ancestryProviderPresent).Count > 0;
        }
    }
}
