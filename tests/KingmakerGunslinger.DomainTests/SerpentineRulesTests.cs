using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using KingmakerGunslinger.Summoning;
using KingmakerGunslinger.RuntimeTesting;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class SerpentineRulesTests
    {
        internal const int AppendedLedgerIdentities = 73;
        internal static void ProfileRequestIsClosedWorkingSaveSlice()
        {
            string scenario = RuntimeTestScenarioCatalog.DisposableExpandedSummoningSnakeProfiles;
            Assertions.Equal("disposable-expanded-summoning-snake-profiles", scenario, "One bounded profile/body request.");
            Assertions.True(RuntimeTestScenarioCatalog.IsAllowed(scenario) &&
                RuntimeTestScenarioCatalog.IsExpandedSummoningRulesScenario(scenario),
                "Native request traverses the exact working-save guard.");
            foreach (string other in new[] { null, "", scenario.ToUpperInvariant(), scenario + "-arbitrary",
                "working-save-expanded-summoning-snake-profiles" })
                Assertions.False(RuntimeTestScenarioCatalog.IsAllowed(other) ||
                    RuntimeTestScenarioCatalog.IsExpandedSummoningRulesScenario(other),
                    "No alternate spelling, write authority or arbitrary creature scope.");
        }

        internal static void ProductionBodyHookRequiresExactHiddenSnakeIdentity()
        {
            JArray entries = (JArray)JObject.Parse(File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "blueprints", "blueprints.json")))["entries"];
            foreach (string key in new[] { "viper", "constrictor-snake" })
            {
                string token = key == "viper" ? "Viper" : "ConstrictorSnake";
                string guid = (string)entries.Single(e =>
                    (string)e["symbol"] == "KMG.Summoning.Unit." + token)["guid"];
                string name = "KMG_Summoning_Unit_" + token, found;
                Assertions.True(SerpentineVisualPolicy.TryProductionSnake(true, guid, name,
                    SerpentineVisualPolicy.WormPrefab, out found) && found == key,
                    "The append-only identity resolves its own original body.");
                Assertions.False(SerpentineVisualPolicy.TryProductionSnake(false, guid, name,
                    SerpentineVisualPolicy.WormPrefab, out found), "Module-disabled has no attachment.");
                foreach (string bad in new[] { null, "", "foreign", guid.ToUpperInvariant(),
                    "bf2216f48b3f4d24c9c502007649340d", "f8fb103168d74b4c93182437e5d2b4e4" })
                    Assertions.False(SerpentineVisualPolicy.TryProductionSnake(true, bad, name,
                        SerpentineVisualPolicy.WormPrefab, out found), "Name alone cannot capture a donor/hybrid.");
                foreach (string bad in new[] { null, "", name.ToLowerInvariant(),
                    "KMG_Summoning_Unit_PurpleWorm", "KMG_Summoning_Unit_Salamander" })
                    Assertions.False(SerpentineVisualPolicy.TryProductionSnake(true, guid, bad,
                        SerpentineVisualPolicy.WormPrefab, out found), "Exact KMG identity/name pair required.");
                foreach (string bad in new[] { null, "", SerpentineVisualPolicy.ClubShieldPrefab,
                    SerpentineVisualPolicy.TwoHandPrefab, SerpentineVisualPolicy.WormPrefab.ToUpperInvariant() })
                    Assertions.False(SerpentineVisualPolicy.TryProductionSnake(true, guid, name, bad, out found),
                        "Reject an unreviewed replacement prefab, even on our identity.");
                Assertions.True(found == null, "Rejected dispatch must not leak the prior key.");
                float multiplier;
                Assertions.False(SummonViewScaleCatalog.TryGetMultiplier(name, out multiplier),
                    "The owned hook applies scale once; the shared hook must not multiply it again.");
                Assertions.Equal("Medium", ExpandedSummoningNaturalProfiles.For(key).Size,
                    "Visual binding does not change mechanical size.");
            }
            Assertions.Equal(.2f, SerpentineVisualPolicy.SnakeViewMultiplier,
                "Same view-only scale as the measured native snake bite research, pending production proof.");
            Assertions.Equal(17, SummonViewScaleCatalog.All.Count,
                "All prior production view steps remain unchanged.");
        }

        internal static void LedgerAppendPreservesEveryHistoricalEntry()
        {
            JArray entries = (JArray)JObject.Parse(File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "blueprints", "blueprints.json")))["entries"];
            Assertions.Equal(2836 + AppendedLedgerIdentities, entries.Count,
                "Exactly 73 Sprint 17 identities append to the frozen prefix.");
            string digest;
            using (var sha = SHA256.Create())
                digest = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(
                    new JArray(entries.Take(2836)).ToString(Newtonsoft.Json.Formatting.None))))
                    .Replace("-", "").ToLowerInvariant();
            Assertions.Equal("bf46e4e3d2d709935f2a27fa32f2bd8ad640098801174680c755c89287ea0570",
                digest, "Every prior symbol, GUID, type, status and historical note stays intact.");
            foreach (string[] row in new[] {
                new[] { "KMG.Summoning.Unit.Viper", "d8be82543ab64dc988c33e9f13608bad" },
                new[] { "KMG.Summoning.Unit.ConstrictorSnake", "f1a2eadf588e4c3b9fb670724d706364" } })
                Assertions.Equal(row[1], (string)entries.Single(e => (string)e["symbol"] == row[0])["guid"],
                    "Allocated snake identity is permanent even while withheld.");
        }
        internal static void PrintedProfilesAreMediumAndCreatureOwned()
        {
            var viper = ExpandedSummoningNaturalProfiles.For("viper");
            var snake = ExpandedSummoningNaturalProfiles.For("constrictor-snake");
            foreach (var profile in new[] { viper, snake })
            {
                Assertions.Equal("Medium", profile.Size, "Frozen contract uses Medium snakes.");
                Assertions.Equal("Animal", profile.HitDieClass, "No Purple Worm magical-beast progression.");
                Assertions.Equal(20, profile.SpeedFeet, "Land speed is printed 20 feet.");
                Assertions.Equal("Bite1d4", profile.PrimaryWeapon, "One printed bite.");
                Assertions.Equal(0, profile.AdditionalWeapons.Count, "No donor sting.");
                Assertions.Equal(0, profile.AdditionalSecondaryWeapons.Count, "No donor tail or sting.");
                Assertions.True(profile.Facts.Contains("TripImmune"), "Legless trip defense.");
                Assertions.False(profile.Facts.Contains("PurpleWormPoison"), "No worm state leakage.");
            }
            Assertions.Equal("2|8|13|14|1|13|2|3", Row(viper), "Printed Viper chassis.");
            Assertions.Equal("3|17|17|12|1|12|2|2", Row(snake), "Printed Constrictor chassis.");
            Assertions.True(viper.Facts.Contains("ImprovedInitiative") &&
                viper.Facts.Contains("WeaponFinesse"), "Printed Viper feats, including bonus finesse.");
            Assertions.True(snake.Facts.Contains("SkillFocusPerception") &&
                snake.Facts.Contains("Toughness"), "Printed Constrictor feats.");
        }

        private static string Row(NaturalSummonProfile p)
        { return string.Join("|", new[] { p.HitDice, p.Strength, p.Dexterity,
            p.Constitution, p.Intelligence, p.Wisdom, p.Charisma, p.NaturalArmor }); }

        internal static void ExactRanksPreserveNativeContributions()
        {
            foreach (string key in new[] { "viper", "constrictor-snake" })
            {
                int m = 0, p = 0, s = 0;
                SerpentineRulesPolicy.AllocateLandRanks(key, ref m, ref p, ref s);
                bool viper = key == "viper";
                int dex = viper ? 1 : 3, wis = 1, focus = viper ? 0 : 3;
                Assertions.Equal(viper ? 9 : 15,
                    m + (m > 0 ? 3 : 0) + dex + SerpentineRulesPolicy.MobilityRacialBonus,
                    "Mobility is ranks + native class + Dexterity + racial.");
                Assertions.Equal(viper ? 9 : 12,
                    p + 3 + wis + focus + SerpentineRulesPolicy.PerceptionRacialBonus,
                    "Perception includes only the printed feat.");
                Assertions.Equal(viper ? 9 : 11,
                    s + 3 + dex + SerpentineRulesPolicy.StealthRacialBonus,
                    "Land Stealth; no aquatic bonus.");
                Assertions.Equal(viper ? 13 : 19,
                    SerpentineRulesPolicy.For(key).BaseHitPoints +
                    (viper ? 2 * 2 : 3 * 1 + 3), "Native Constitution/Toughness complete printed HP.");
            }
        }

        internal static void RankAllocationRejectsDonorOrRepeatRanks()
        {
            foreach (string key in new[] { "viper", "constrictor-snake" })
            foreach (int dirtyIndex in new[] { 0, 1, 2 })
            foreach (int donorRanks in new[] { 1, 5 })
            {
                int m = dirtyIndex == 0 ? donorRanks : 0, p = dirtyIndex == 1 ? donorRanks : 0,
                    s = dirtyIndex == 2 ? donorRanks : 0;
                Assertions.Throws<InvalidOperationException>(() =>
                    SerpentineRulesPolicy.AllocateLandRanks(key, ref m, ref p, ref s),
                    "Unexpected ranks must fail, not be overwritten silently.");
                Assertions.Equal(donorRanks, m + p + s,
                    "The observed donor five-rank seed is rejected atomically, not canceled with a hidden bonus.");
            }
            foreach (string key in new[] { null, "", "salamander", "purple-worm" })
                Assertions.Throws<ArgumentException>(() => SerpentineRulesPolicy.For(key),
                    "Policy must not bleed into a donor or hybrid.");
        }

        internal static void PoisonAndDamageUseLiveModifiers()
        {
            Assertions.Equal(13, SerpentineRulesPolicy.ViperPoisonDifficultyClass(2), "Printed DC.");
            Assertions.Equal(15, SerpentineRulesPolicy.ViperPoisonDifficultyClass(4), "Live Con increase.");
            Assertions.Equal(10, SerpentineRulesPolicy.ViperPoisonDifficultyClass(-1), "Live Con penalty.");
            Assertions.Equal(6, SerpentineRulesPolicy.ViperPoisonExposures, "Six total exposures.");
            Assertions.Equal(1, SerpentineRulesPolicy.ViperPoisonSavesToCure, "One native cure save.");
            foreach (int[] row in new[] { new[] { -3, -3 }, new[] { -1, -1 },
                new[] { 0, 0 }, new[] { 1, 1 }, new[] { 3, 4 }, new[] { 5, 7 } })
                Assertions.Equal(row[1], SerpentineRulesPolicy.SingleNaturalDamageBonus(row[0]),
                    "Only a positive Strength bonus gains an extra half.");
        }

        internal static void WeaponSizeDropsDonorSizeButKeepsLiveShift()
        {
            foreach (int donorSize in new[] { 2, 4, 7, 8 })
            foreach (int bodySize in new[] { 3, 4, 5 })
            {
                Assertions.Equal(bodySize, SerpentineRulesPolicy.LiveWeaponSize(bodySize, donorSize, donorSize),
                    "Donor size is not a second body-size multiplier.");
                Assertions.Equal(bodySize + 1, SerpentineRulesPolicy.LiveWeaponSize(bodySize, donorSize, donorSize + 1),
                    "Legitimate native weapon-size shifts survive.");
            }
            Assertions.Equal(2, SerpentineRulesPolicy.LiveWeaponSize(2, 8, 2), "Native lower bound.");
            Assertions.Equal(8, SerpentineRulesPolicy.LiveWeaponSize(8, 2, 8), "Native upper bound.");
        }

        internal static void NewIdentitiesAreHiddenWithoutMovingPublishedChoices()
        {
            var all = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                .Concat(ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly)).ToArray();
            foreach (string key in new[] { "viper", "constrictor-snake" })
            {
                var rows = all.Where(v => v.Creature.Key == key).ToArray();
                Assertions.Equal(key == "viper" ? 18 : 14, rows.Length, "Exact registered placement count.");
                Assertions.True(rows.All(v => !SummonVisibilityCatalog.IsPublished(v)), "Every new root stays hidden.");
                Assertions.True(rows.Select(v => v.Multiplicity).Distinct().Count() == 3,
                    "Direct, 1d3 and 1d4+1 private routes are registered.");
            }
            Assertions.Equal(976, all.Count(SummonVisibilityCatalog.IsPublished), "No historical root suppressed.");
            Assertions.Equal(1005, all.Count(SummonVisibilityCatalog.IsPublished) +
                SummonNativeExpansionCatalog.All.Count, "Visible surface unchanged.");
            Assertions.True(all.Where(v => !SerpentineRulesPolicy.IsSnake(v.Creature.Key))
                .All(SummonVisibilityCatalog.IsPublished), "Only the two new snakes are withheld.");
        }

        internal static void SnakeArtworkHasExactConsumersAndProvenance()
        {
            JObject manifest = JObject.Parse(File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "assets-source", "original-icons", "expanded-summoning", "icon-manifest.json")));
            var icons = (JArray)manifest["icons"];
            foreach (string key in new[] { "viper", "constrictor-snake" })
            {
                JToken row = icons.Single(v => (string)v["key"] == key);
                string token = key == "viper" ? "Viper" : "ConstrictorSnake";
                string[] consumers = ((JArray)row["blueprintSymbols"]).Values<string>().ToArray();
                Assertions.True(consumers.Contains("KMG.Summoning.Unit." + token), "Owned unit painting.");
                Assertions.True(consumers.Contains("KMG.Summoning.Natural." + token + ".UnitType"), "Owned inspection painting.");
                Assertions.Equal(key == "viper" ? 38 : 30, consumers.Length,
                    "Unit/type plus exact logical and SM template consumers.");
                Assertions.Equal(128, (int)row["width"], "Native-sized export.");
                Assertions.Equal(128, (int)row["height"], "Native-sized export.");
                Assertions.True(icons.All(other => ReferenceEquals(other, row) ||
                    (string)other["outputSha256"] != (string)row["outputSha256"]), "No unrelated art reuse.");
            }
        }
    }
}
