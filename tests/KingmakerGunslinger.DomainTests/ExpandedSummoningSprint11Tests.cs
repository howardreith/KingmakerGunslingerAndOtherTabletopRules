using System;
using System.IO;
using KingmakerGunslinger.Summoning;

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

        internal static void UngulateProfilesMatchPrintedRoles()
        {
            UngulateRulesPolicy.Validate();
            UngulateRulesProfile aurochs = UngulateRulesPolicy.For("aurochs");
            UngulateRulesProfile bison = UngulateRulesPolicy.For("bison");
            UngulateRulesProfile rhino = UngulateRulesPolicy.For("rhinoceros");
            UngulateRulesProfile woolly = UngulateRulesPolicy.For("woolly-rhinoceros");
            Assertions.True(aurochs.HitDice == 3 && aurochs.Strength == 23 &&
                aurochs.GoreDiceCount == 1 && aurochs.GoreDieSides == 8 &&
                aurochs.GoreBonus == 9 && aurochs.TrampleDiceCount == 2 &&
                aurochs.TrampleDieSides == 6 && aurochs.TrampleBonus == 9 &&
                aurochs.TrampleDc == 17 && aurochs.Stampede,
                "Aurochs has the printed gore, trample and stampede profile.");
            Assertions.True(bison.HitDice == 5 && bison.Strength == 27 &&
                bison.GoreDiceCount == 2 && bison.GoreDieSides == 6 &&
                bison.GoreBonus == 12 && bison.TrampleBonus == 12 &&
                bison.TrampleDc == 20 && bison.Stampede,
                "Bison is the heavier printed herd profile.");
            Assertions.True(rhino.HitDice == 5 && rhino.Strength == 22 &&
                rhino.GoreBonus == 9 && !rhino.HasTrample &&
                rhino.ChargeDiceCount == 4 && rhino.ChargeDieSides == 6 &&
                rhino.ChargeBonus == 12 && rhino.ChargeDiceIncrement == 2 &&
                rhino.ChargeBonusIncrement == 3,
                "Ordinary Rhinoceros has charge but no invented trample.");
            Assertions.True(woolly.HitDice == 8 && woolly.Strength == 28 &&
                woolly.GoreDiceCount == 2 && woolly.GoreDieSides == 8 &&
                woolly.GoreBonus == 13 && woolly.ChargeDiceCount == 4 &&
                woolly.ChargeDieSides == 8 && woolly.ChargeBonus == 18 &&
                woolly.ChargeDiceIncrement == 2 &&
                woolly.ChargeBonusIncrement == 5 &&
                woolly.TrampleDiceCount == 2 && woolly.TrampleBonus == 13 &&
                woolly.TrampleDc == 23 && !woolly.Stampede,
                "Woolly Rhinoceros has its distinct charge and trample profile.");
            Assertions.True(rhino.AppliesPowerfulCharge(true, true, true, false),
                "A first gore charge uses the stronger stat-block damage.");
            Assertions.False(rhino.AppliesPowerfulCharge(true, false, true, false) ||
                rhino.AppliesPowerfulCharge(false, true, true, false) ||
                rhino.AppliesPowerfulCharge(true, true, false, false) ||
                rhino.AppliesPowerfulCharge(true, true, true, true) ||
                bison.AppliesPowerfulCharge(true, true, true, false),
                "Powerful charge never leaks to ordinary or unrelated attacks.");
        }

        internal static void TrampleTargetsAndRoundsAreBounded()
        {
            UngulateRulesProfile aurochs = UngulateRulesPolicy.For("aurochs");
            UngulateRulesProfile rhino = UngulateRulesPolicy.For("rhinoceros");
            Assertions.True(aurochs.CanTrample(4, 3, 1),
                "A Large aurochs can trample a Medium creature.");
            Assertions.False(aurochs.CanTrample(4, 4, 2),
                "A pair does not qualify for same-size stampede.");
            Assertions.True(aurochs.CanTrample(4, 4, 3) &&
                aurochs.TrampleSaveDc(3) == 19,
                "Three adjacent stampeding herd animals may trample Large foes at DC 19.");
            Assertions.False(aurochs.CanTrample(4, 5, 3) ||
                rhino.CanTrample(4, 3, 0),
                "Stampede does not reach larger creatures or grant Rhino trample.");
            TrampleRoundLedger ledger = new TrampleRoundLedger();
            Assertions.True(ledger.TryClaim(10, "target-a"),
                "First contact is eligible.");
            Assertions.False(ledger.TryClaim(10, "target-a"),
                "A path replay cannot damage the same target twice in a round.");
            Assertions.True(ledger.TryClaim(10, "target-b") &&
                ledger.TryClaim(11, "target-a"),
                "Another foe and a later round are eligible.");
            Assertions.False(ledger.TryClaim(10, "target-c") ||
                ledger.TryClaim(11, ""),
                "Time regression or missing target identity fails closed.");
            ledger.Clear();
            Assertions.True(ledger.TryClaim(10, "target-a"),
                "Cleanup releases the request-local contact ledger.");
        }
    }
}
