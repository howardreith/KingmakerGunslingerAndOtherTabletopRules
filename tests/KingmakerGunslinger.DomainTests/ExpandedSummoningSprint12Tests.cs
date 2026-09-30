using System;
using System.IO;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningSprint12Tests
    {
        internal const int AppendedLedgerIdentities = 37;

        internal static void Sprint12FoundationStaysHiddenUntilQualified()
        {
            string catalog = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Summoning", "ExpandedSummoningCatalog.cs"));
            string visibility = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Summoning", "SummonVisibilityCatalog.cs"));
            string donors = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Summoning", "ExpandedSummoningDonorCatalog.cs"));
            string profiles = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Summoning", "ExpandedSummoningNaturalProfiles.cs"));

            Assertions.True(catalog.Contains(
                    "C(\"dire-rat\",\"Dire Rat\",1,true,1,\"Dog\")") &&
                catalog.Contains("ValidateFamily(SummonFamily.Monster, 80, 453)") &&
                catalog.Contains("ValidateFamily(SummonFamily.NaturesAlly, 78, 447)"),
                "Sprint 12 must register Dire Rat at tier 1 in both families.");
            foreach (string key in new[] {
                "\"dire-rat\"", "\"dog\"", "\"hyena\"", "\"goblin-dog\""
            })
                Assertions.True(visibility.Contains(key),
                    "Unqualified Sprint 12 creature must be suppressed: " + key);
            Assertions.True(visibility.Contains(
                    "RegisteredLogicalPlacementCount = 900") &&
                visibility.Contains("SuppressedLogicalPlacementCount = 68"),
                "Sprint 12 publication boundary must freeze 900/68/832.");
            Assertions.True(donors.Contains(
                    "dire-rat|77f3f2ddf1ec2da45ab956c433e3b557|1") &&
                donors.Contains("dog|77f3f2ddf1ec2da45ab956c433e3b557|1"),
                "Dire Rat and Dog must use the audited native Dog summon rig.");
            foreach (string token in new[] {
                "P(\"dire-rat\", \"Dire Rat\", \"Animal\", 1, \"Small\"",
                "10, 17, 13, 2, 13, 4, 40, 1, \"Bite1d4\"",
                "\"TripDefenseFourLegs\", \"WeaponFinesse\"",
                "\"SkillFocusPerception\"", "exact DC 11 contract"
            })
                Assertions.True(profiles.Contains(token),
                    "Dire Rat hidden foundation is missing " + token + ".");
        }

        internal static void NativeCanidAndRatSurveyStaysMetadataOnly()
        {
            string source = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "RuntimeTesting",
                "RuntimeTestRunner.ExpandedSummoningNativeDonors.cs"));
            foreach (string token in new[] {
                "\"rat\"", "\"dog\"", "\"hyena\"", "\"worg\"",
                "\"wolf\"", "\"goblin\"", "\"goblinoid\"",
                "\"disease\"", "\"immunity\"",
                "\"allerg\"", "\"filth\"", "\"fever\"",
                "\"DiseaseImmunity\"", "\"AllergicReaction\"",
                "\"GoblinDogAllergicReaction\"", "\"DireRatDisease\"",
                "\"9545a5550d89feb47a84edaeb4e63d0b\"",
                "\"SubtypeGoblinoid\"",
                "units.Add(DescribeNativeUnit(unit))",
                "native-donor-audit.json"
            })
                Assertions.True(source.Contains(token),
                    "Sprint 12 native metadata survey is missing " + token + ".");
            Assertions.False(source.Contains("AssetBundle.LoadFromFile") ||
                source.Contains("Texture2D.EncodeToPNG"),
                "The donor survey must not extract or export native game art.");

            string record = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "planning",
                "EXPANDED-SUMMONING-SPRINT12-RULES-AND-DONORS.md"));
            foreach (string token in new[] {
                "77f3f2ddf1ec2da45ab956c433e3b557",
                "9545a5550d89feb47a84edaeb4e63d0b",
                "d524df24b2f38cf4590525b2e7c4f34e",
                "No broader native Goblinoid",
                "subtype fact exists",
                "No individual Dire Rat exists",
                "all 1,954 domain tests",
                "20260930T0823186905486Z"
            })
                Assertions.True(record.Contains(token),
                    "Sprint 12 audit record is missing " + token + ".");
        }

        internal static void StirgeRosterTextRecordsPrimaryCadenceException()
        {
            string tool = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "tools",
                "expanded_summoning_manifest.py"));
            string roster = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "planning",
                "EXPANDED-SUMMONING-ROSTER.md"));
            foreach (string text in new[] { tool, roster })
            {
                Assertions.True(text.Contains(
                        "one disclosed 10% Filth Fever exposure check per victim") &&
                    text.Contains("primary Paizo stat block requires"),
                    "Generated and checked-in roster text must record the " +
                    "primary-source once-per-Stirge/victim exception.");
                Assertions.False(text.Contains(
                    "each successful blood drain rolls"),
                    "Roster text must not claim a per-drain Stirge disease roll.");
            }
        }
    }
}
