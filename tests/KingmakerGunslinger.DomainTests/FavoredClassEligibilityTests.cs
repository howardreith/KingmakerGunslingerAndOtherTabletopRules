using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.FavoredClass;

namespace KingmakerGunslinger.DomainTests
{
    internal static class FavoredClassEligibilityTests
    {
        private const string MostlyHumanFact = "kmg.test.mostly-human";

        // E01: the scheduled Gunslinger menu for every source-addressable race,
        // first-party profile only (third-party OFF by default).
        internal static void GunslingerMenusForEverySourceAddressableRace()
        {
            Dictionary<string, string[]> expected = new Dictionary<string, string[]>(
                StringComparer.Ordinal)
            {
                { FavoredClassAncestry.Human, new[] { FavoredClassCatalog.EffectGrit } },
                { FavoredClassAncestry.HalfElf, new[] { FavoredClassCatalog.EffectGrit,
                    FavoredClassCatalog.EffectFirearmConfirmation } },
                { FavoredClassAncestry.HalfOrc, new[] { FavoredClassCatalog.EffectGrit,
                    FavoredClassCatalog.EffectPistolWhip } },
                { FavoredClassAncestry.Elf, new[] { FavoredClassCatalog.EffectFirearmConfirmation } },
                { FavoredClassAncestry.Dwarf, new[] { FavoredClassCatalog.EffectMisfire } },
                { FavoredClassAncestry.Gnome, new string[0] },
                { FavoredClassAncestry.Halfling, new[] { FavoredClassCatalog.EffectHalflingNimble,
                    FavoredClassCatalog.EffectHalflingDodge } },
                { FavoredClassAncestry.Aasimar, new[] { FavoredClassCatalog.EffectGrit } },
                { FavoredClassAncestry.Tiefling, new[] { FavoredClassCatalog.EffectGrit } },
                { FavoredClassAncestry.Goblin, new[] { FavoredClassCatalog.EffectFirearmConfirmation } },
                { FavoredClassAncestry.Hobgoblin, new[] { FavoredClassCatalog.EffectGrit } },
                { FavoredClassAncestry.Fetchling, new[] { FavoredClassCatalog.EffectGrit } },
                { FavoredClassAncestry.Dhampir, new string[0] },
                { FavoredClassAncestry.Drow, new string[0] },
                { FavoredClassAncestry.Duergar, new string[0] },
                { FavoredClassAncestry.Ganzi, new string[0] },
                { FavoredClassAncestry.Suli, new string[0] },
                { FavoredClassAncestry.Ifrit, new[] { FavoredClassCatalog.EffectInitiative } },
                { FavoredClassAncestry.Oread, new string[0] },
                { FavoredClassAncestry.Sylph, new string[0] },
                { FavoredClassAncestry.Undine, new string[0] },
            };
            Assertions.Equal(21, FavoredClassAncestry.SourceAddressable.Length,
                "Source-addressable race identities.");
            foreach (string race in FavoredClassAncestry.SourceAddressable)
            {
                string[] menu = GunslingerMenu(Evidence(race), FirstPartyOnly);
                Assertions.True(menu.SequenceEqual(expected[race]),
                    race + " Gunslinger menu was: " + string.Join(", ", menu));
            }
        }

        // E02/E03/E04 and third-party routes (profile ON).
        internal static void AliasRoutesNeverDuplicateOrMultiply()
        {
            string[] halfElf = GunslingerMenu(Evidence(FavoredClassAncestry.HalfElf), AllProfiles);
            Assertions.Equal(1, halfElf.Count(id => id == FavoredClassCatalog.EffectGrit),
                "A half-elf sees exactly one grit leaf family (G04/G07 share one counter).");
            FavoredClassEffectSpec grit = FavoredClassCatalog.Effect(FavoredClassCatalog.EffectGrit);
            string[] routes = FavoredClassEligibility.EligibleRoutes(grit,
                Permitted(Evidence(FavoredClassAncestry.HalfElf)), AllProfiles, AnyProvider)
                .Select(row => row.Id).ToArray();
            Assertions.True(routes.SequenceEqual(new[] { "G04", "G07" }),
                "Both half-elf grit routes qualify, still one effect.");

            string[] halfOrc = GunslingerMenu(Evidence(FavoredClassAncestry.HalfOrc), AllProfiles);
            Assertions.True(halfOrc.SequenceEqual(new[] { FavoredClassCatalog.EffectGrit,
                FavoredClassCatalog.EffectPistolWhip }),
                "Optional Orc access adds no duplicate Pistol-Whip family for a half-orc.");
            FavoredClassEffectSpec whip = FavoredClassCatalog.Effect(FavoredClassCatalog.EffectPistolWhip);
            Assertions.True(FavoredClassEligibility.EligibleRoutes(whip,
                Permitted(Evidence(FavoredClassAncestry.HalfOrc)), AllProfiles, AnyProvider)
                .Select(row => row.Id).SequenceEqual(new[] { "G05", "G20" }),
                "Half-orc reaches Pistol-Whip through G05 and the optional G20 alias.");

            Assertions.True(GunslingerMenu(Evidence(FavoredClassAncestry.Tiefling), AllProfiles)
                .SequenceEqual(new[] { FavoredClassCatalog.EffectGrit,
                    FavoredClassCatalog.EffectDirtyTrickTrip }),
                "Tiefling keeps host human grit and gains the labeled third-party maneuver option.");
            Assertions.True(GunslingerMenu(Evidence(FavoredClassAncestry.Dhampir), AllProfiles)
                .SequenceEqual(new[] { FavoredClassCatalog.EffectFirearmConfirmation }),
                "Dhampir confirmation is third-party only.");
            Assertions.True(GunslingerMenu(Evidence(FavoredClassAncestry.Drow), AllProfiles)
                .SequenceEqual(new[] { FavoredClassCatalog.EffectDrowNimble }),
                "Drow Nimble keeps its own 1/6 ledger.");
            Assertions.True(GunslingerMenu(Evidence(FavoredClassAncestry.Duergar), AllProfiles)
                .SequenceEqual(new[] { FavoredClassCatalog.EffectMisfire }),
                "Duergar misfire reuses the per-type counter family.");
        }

        // E05: human access for geniekin only through a real verified permission.
        internal static void MostlyHumanGrantsHumanAccessOnlyToGeniekin()
        {
            foreach (string parent in FavoredClassAncestry.ElementalParents)
            {
                ISet<string> without = Permitted(Evidence(parent));
                ISet<string> with = Permitted(Evidence(parent, MostlyHumanFact));
                Assertions.False(without.Contains(FavoredClassAncestry.Human),
                    parent + " without Mostly Human must not count as human.");
                Assertions.True(with.Contains(FavoredClassAncestry.Human),
                    parent + " with Mostly Human counts as human for favored-class options.");
                Assertions.True(with.Contains(parent),
                    parent + " retains its native ancestry.");
                Assertions.Equal(2, with.Count, parent + " gains exactly one permission.");
            }
            Assertions.True(GunslingerMenu(Evidence(FavoredClassAncestry.Ifrit, MostlyHumanFact),
                FirstPartyOnly).SequenceEqual(new[] { FavoredClassCatalog.EffectGrit,
                    FavoredClassCatalog.EffectInitiative }),
                "A Mostly Human Ifrit keeps G11 and gains G07.");
            Assertions.False(Permitted(Evidence(FavoredClassAncestry.Dwarf, MostlyHumanFact))
                .Contains(FavoredClassAncestry.Human),
                "The fact alone never makes a non-geniekin human.");
            Assertions.False(Permitted(Evidence(FavoredClassAncestry.Suli, MostlyHumanFact))
                .Contains(FavoredClassAncestry.Human),
                "No Suli Mostly Human trait is verified.");
        }

        // E10: unknown ancestry, cycles, depth and duplicate facts.
        internal static void UnknownCyclicAndDuplicateEvidenceIsBounded()
        {
            FavoredClassPermissionGraph graph = FavoredClassPermissionGraph.CreateVerified(MostlyHumanFact);
            Assertions.Equal(0, graph.PermittedAncestries(Evidence(null)).Count,
                "An unrecognized race grants nothing.");
            Assertions.Equal(0, graph.PermittedAncestries(null).Count,
                "Missing evidence grants nothing.");
            FavoredClassPermissionGraph cyclic = new FavoredClassPermissionGraph(new[]
            {
                new FavoredClassPermissionEdge("a", "b", FavoredClassPermissionBasis.HostHumanPolicy, null),
                new FavoredClassPermissionEdge("b", "a", FavoredClassPermissionBasis.HostHumanPolicy, null),
                new FavoredClassPermissionEdge("b", "c", FavoredClassPermissionBasis.HostHumanPolicy, null),
            });
            ISet<string> reached = cyclic.PermittedAncestries(Evidence("a"));
            Assertions.True(reached.OrderBy(x => x, StringComparer.Ordinal)
                .SequenceEqual(new[] { "a", "b", "c" }), "Cycles terminate deterministically.");
            List<FavoredClassPermissionEdge> chain = new List<FavoredClassPermissionEdge>();
            for (int i = 0; i < 10; i++)
                chain.Add(new FavoredClassPermissionEdge("n" + i, "n" + (i + 1),
                    FavoredClassPermissionBasis.HostHumanPolicy, null));
            Assertions.Equal(FavoredClassPermissionGraph.MaximumDepth + 1,
                new FavoredClassPermissionGraph(chain).PermittedAncestries(Evidence("n0")).Count,
                "Traversal depth is bounded.");
            ISet<string> duplicate = graph.PermittedAncestries(new FavoredClassAncestryEvidence(
                FavoredClassAncestry.Ifrit, new[] { MostlyHumanFact, MostlyHumanFact }));
            Assertions.Equal(2, duplicate.Count, "Duplicate facts add no further permission.");
        }

        // H07 at the policy level: an absent optional race removes only its route.
        internal static void AbsentProvidersRemoveOnlyTheirRoute()
        {
            Func<string, bool> noHobgoblin = race => race != FavoredClassAncestry.Hobgoblin;
            FavoredClassEffectSpec grit = FavoredClassCatalog.Effect(FavoredClassCatalog.EffectGrit);
            Assertions.True(FavoredClassEligibility.IsEligible(grit,
                Permitted(Evidence(FavoredClassAncestry.Human)), FirstPartyOnly, noHobgoblin),
                "Humans keep grit when the hobgoblin provider is absent.");
            Assertions.False(FavoredClassEligibility.IsEligible(grit,
                Permitted(Evidence(FavoredClassAncestry.Hobgoblin)), FirstPartyOnly, noHobgoblin),
                "An absent provider route grants nothing (never an unrestricted fallback).");
            Assertions.False(FavoredClassEligibility.IsEligible(grit, new HashSet<string>(),
                AllProfiles, AnyProvider), "No ancestry evidence means no eligibility.");
            Assertions.False(FavoredClassEligibility.IsEligible(grit,
                Permitted(Evidence(FavoredClassAncestry.Human)), profile => false, AnyProvider),
                "Disabled profiles publish nothing.");
        }

        // Race identity manifest: exact GUIDs, no name or appearance inference.
        internal static void RaceIdentitiesAreExactAndComplete()
        {
            IList<FavoredClassRaceIdentity> all = FavoredClassRaceIdentities.All;
            Assertions.Equal(21, all.Count, "Race identity count.");
            Assertions.True(all.Select(value => value.Ancestry).OrderBy(x => x, StringComparer.Ordinal)
                .SequenceEqual(FavoredClassAncestry.SourceAddressable.OrderBy(x => x,
                    StringComparer.Ordinal)), "Identities must cover exactly the 21 ancestries.");
            Assertions.Equal(21, all.Select(value => value.RaceGuid.ToLowerInvariant())
                .Distinct(StringComparer.Ordinal).Count(), "Race GUIDs must be unique.");
            Assertions.Equal(9, all.Count(value => value.Provider == FavoredClassRaceProvider.Native),
                "Native race identities.");
            Assertions.Equal(8, all.Count(value => value.Provider == FavoredClassRaceProvider.Optional),
                "Optional race identities.");
            Assertions.Equal(4, all.Count(value => value.Provider == FavoredClassRaceProvider.Kmg),
                "KMG elemental parent identities.");

            Newtonsoft.Json.Linq.JObject ledger = Newtonsoft.Json.Linq.JObject.Parse(
                System.IO.File.ReadAllText(System.IO.Path.Combine(Environment.CurrentDirectory,
                    "blueprints", "blueprints.json")));
            Dictionary<string, string> symbols = ledger["entries"].ToDictionary(
                entry => (string)entry["symbol"], entry => (string)entry["guid"], StringComparer.Ordinal);
            foreach (string parent in FavoredClassAncestry.ElementalParents)
            {
                string symbol = "KMG.ElementalRaces." + char.ToUpperInvariant(parent[0]) +
                    parent.Substring(1) + ".Race";
                Assertions.Equal(symbols[symbol], FavoredClassRaceIdentities.ForAncestry(parent).RaceGuid,
                    parent + " must match the committed blueprint ledger.");
            }
            Assertions.Equal(FavoredClassAncestry.Human, FavoredClassRaceIdentities.AncestryForRaceGuid(
                "0A5D473EAD98B0646B94495AF250FDC4"), "Lookup is case-insensitive on hex.");
            Assertions.True(FavoredClassRaceIdentities.AncestryForRaceGuid(
                "00000000000000000000000000000000") == null, "Unknown races are unrecognized.");
            Assertions.True(FavoredClassRaceIdentities.AncestryForRaceGuid(null) == null,
                "A missing race is unrecognized.");
        }

        private static readonly Func<FavoredClassProfile, bool> FirstPartyOnly =
            profile => profile == FavoredClassProfile.FirstParty ||
                profile == FavoredClassProfile.Adaptation;

        private static readonly Func<FavoredClassProfile, bool> AllProfiles =
            profile => profile != FavoredClassProfile.None;

        private static readonly Func<string, bool> AnyProvider = race => true;

        private static FavoredClassAncestryEvidence Evidence(string race, params string[] facts)
        {
            return new FavoredClassAncestryEvidence(race, facts);
        }

        private static ISet<string> Permitted(FavoredClassAncestryEvidence evidence)
        {
            return FavoredClassPermissionGraph.CreateVerified(MostlyHumanFact)
                .PermittedAncestries(evidence);
        }

        private static string[] GunslingerMenu(FavoredClassAncestryEvidence evidence,
            Func<FavoredClassProfile, bool> profiles)
        {
            ISet<string> permitted = Permitted(evidence);
            return FavoredClassCatalog.Effects
                .Where(effect => effect.ClassFamily == FavoredClassCatalog.Gunslinger)
                .Where(effect => FavoredClassEligibility.IsEligible(effect, permitted,
                    profiles, AnyProvider))
                .Select(effect => effect.Id)
                .ToArray();
        }
    }
}
