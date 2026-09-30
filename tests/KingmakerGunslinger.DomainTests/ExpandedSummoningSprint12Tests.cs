using System;
using System.IO;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningSprint12Tests
    {
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
