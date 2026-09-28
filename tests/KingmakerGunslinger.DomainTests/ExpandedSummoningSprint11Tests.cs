using System;
using System.IO;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningSprint11Tests
    {
        internal static void UngulateDonorSurveyRecordsNativeMechanicGraphs()
        {
            string source = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.ExpandedSummoningNativeDonors.cs"));
            foreach (string name in new[] { "aurochs", "bison", "rhinoceros",
                "woolly", "mastodon", "elephant", "trample",
                "powerfulcharge", "TrampleFeature", "PowerfulChargeFeature",
                "FlyTrampleTest", "OverrunAbility", "MammothTrample",
                "PowerfulChargeSharedStrengthBuff" })
                Assertions.True(source.Contains("\"" + name + "\""),
                    "Native survey must cover " + name + " by exact search term.");
            Assertions.True(source.Contains("DescribeGraph(matches[0], 0,") &&
                source.Contains("native-donor-audit.json") &&
                source.Contains("DescribeNativeUnit(unit)"),
                "Survey records component graphs and physical donor profiles.");
        }
    }
}
