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
        internal static void AppearanceSuspensionPreservesNativeAiActions()
        {
            foreach (bool manual in new[] { false, true })
            {
                object first = new object(), second = new object();
                var actions = new System.Collections.Generic.List<object> { first, second };
                int stops = 0, deferrals = 0;
                SerpentineCommandReviewPolicy.SuspendAppearanceDriver(manual,
                    () => { stops++; actions.Clear(); }, () => { deferrals++; });
                Assertions.Equal(manual ? 1 : 0, stops, "Only manual setup empties actions.");
                Assertions.Equal(manual ? 0 : 1, deferrals, "AI setup defers its native scheduler.");
                Assertions.True(manual ? actions.Count == 0 :
                    actions.SequenceEqual(new[] { first, second }), "AI source actions remain the same references.");
            }
        }

        internal static void SnakeFootprintUsesBodyScaleWithoutChangingNativeFloor()
        {
            foreach (float radius in new[] { .25f, .5f, 1f, 3f, 5f, 10f })
            {
                float adjusted = SerpentineVisualPolicy.ScaleSnakeBaseCorpulence(radius);
                Assertions.Equal(radius * .2f, adjusted, "Only the serialized base radius is scaled.");
                foreach (float nativeSizeMultiplier in new[] { .66f, 1f, 1f / .66f })
                    Assertions.Equal(Math.Max(.5f, radius * .2f * nativeSizeMultiplier),
                        Math.Max(.5f, adjusted * nativeSizeMultiplier),
                        "Native getter retains its minimum and later rules-size multiplier.");
            }
            foreach (float invalid in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity,
                -1f, 0f, float.Epsilon })
                Assertions.Throws<ArgumentOutOfRangeException>(
                    () => SerpentineVisualPolicy.ScaleSnakeBaseCorpulence(invalid),
                    "Malformed/underflowed footprints fail closed.");
        }

        internal static void CommandSetupRequiresIntactOriginalAndNativeControl()
        {
            for (int mask = 0; mask < 32; mask++)
            {
                bool original = (mask & 1) != 0, intact = (mask & 2) != 0, canAct = (mask & 4) != 0;
                bool manual = (mask & 8) != 0, controllable = (mask & 16) != 0;
                bool expected = original && intact && canAct && (!manual || controllable);
                foreach (int frame in new[] { 60, 64, 600 })
                    Assertions.Equal(expected, SerpentineCommandReviewPolicy.ReadyToStart(original,
                        intact, canAct, manual, controllable, frame), "Every body/control prerequisite is mandatory.");
            }
            foreach (int frame in new[] { -1, 0, 30, 59, 601, int.MaxValue })
                Assertions.False(SerpentineCommandReviewPolicy.ReadyToStart(true, true, true, true, true, frame),
                    "No early success or extended settlement bound.");
        }

        internal static void CommandRequestAndMatrixAreClosed()
        {
            string scenario = RuntimeTestScenarioCatalog.DisposableExpandedSummoningSnakeCommands;
            Assertions.Equal("disposable-expanded-summoning-snake-commands", scenario, "One closed command request.");
            Assertions.True(RuntimeTestScenarioCatalog.IsAllowed(scenario) &&
                RuntimeTestScenarioCatalog.IsExpandedSummoningRulesScenario(scenario), "Working-save guards inherited.");
            foreach (string invalid in new[] { scenario.ToUpperInvariant(), scenario + "-arbitrary",
                "working-save-expanded-summoning-snake-commands" })
                Assertions.False(RuntimeTestScenarioCatalog.IsAllowed(invalid), "No scope/write alias.");
            var cells = SerpentineCommandReviewPolicy.Cells();
            Assertions.Equal(8, cells.Length, "Both creatures, modes and drivers.");
            Assertions.Equal("viper-rtwp-manual,viper-rtwp-ai,viper-turn-based-manual,viper-turn-based-ai," +
                "constrictor-snake-rtwp-manual,constrictor-snake-rtwp-ai,constrictor-snake-turn-based-manual,constrictor-snake-turn-based-ai",
                string.Join(",", cells.Select(value => string.Join("-", value))), "Closed ordered matrix.");
            cells[0][0] = "foreign";
            Assertions.Equal("viper", SerpentineCommandReviewPolicy.Cells()[0][0], "No mutable global matrix.");
        }

        internal static void CommandRetryNeverDrivesAiOrReplaysHeldAttack()
        {
            for (int attempt = 0; attempt < 4; attempt++)
                Assertions.True(SerpentineCommandReviewPolicy.CanIssueManual(true, true, attempt,
                    false, false, false, true), "Bounded completed real-command retries.");
            for (int mask = 0; mask < 64; mask++)
            {
                bool manual = (mask & 1) != 0, ready = (mask & 2) != 0, pending = (mask & 4) != 0;
                bool signature = (mask & 8) != 0, held = (mask & 16) != 0, alive = (mask & 32) != 0;
                Assertions.Equal(manual && ready && !pending && !signature && !held && alive,
                    SerpentineCommandReviewPolicy.CanIssueManual(manual, ready, 1, pending, signature, held, alive),
                    "Every readiness/control/relationship gate is mandatory.");
            }
            foreach (int invalid in new[] { -1, 4, 99 })
                Assertions.False(SerpentineCommandReviewPolicy.CanIssueManual(true, true, invalid,
                    false, false, false, true), "Attempt bound is closed.");
        }

        internal static void CommandContactRequiresPlayedClipAndActualGap()
        {
            Assertions.True(SerpentineCommandReviewPolicy.Contact(true, true, false, true, 50, .25f), "Exact threshold.");
            foreach (float gap in new[] { -.01f, .251f, float.NaN, float.PositiveInfinity })
                Assertions.False(SerpentineCommandReviewPolicy.Contact(true, true, false, true, 50, gap),
                    "No distant/nonfinite contact waiver.");
            Assertions.False(SerpentineCommandReviewPolicy.Contact(false, true, false, true, 50, 0), "Exact actors.");
            Assertions.False(SerpentineCommandReviewPolicy.Contact(true, false, false, true, 50, 0), "Actual command.");
            Assertions.False(SerpentineCommandReviewPolicy.Contact(true, true, true, true, 50, 0), "No incidental AoO.");
            Assertions.False(SerpentineCommandReviewPolicy.Contact(true, true, false, false, 50, 0), "No fallback IsActed.");
            Assertions.False(SerpentineCommandReviewPolicy.Contact(true, true, false, true, 0, 0), "Actual original points.");
        }

        internal static void CommandMaintainRequiresLaterRoundWithoutSecondAttack()
        {
            Assertions.True(SerpentineCommandReviewPolicy.LaterMaintain(1, 1, 2, 1, 1), "One initial, two later bundles.");
            foreach (int initial in new[] { 0, 2 })
                Assertions.False(SerpentineCommandReviewPolicy.LaterMaintain(initial, 1, 2, 1, 1), "Exact initial constrict.");
            foreach (int later in new[] { 0, 1, 3, 4 })
                Assertions.False(SerpentineCommandReviewPolicy.LaterMaintain(1, 1, later, 1, 1), "Exact later bite+constrict.");
            Assertions.False(SerpentineCommandReviewPolicy.LaterMaintain(1, 0, 2, 1, 1), "Not application frame.");
            Assertions.False(SerpentineCommandReviewPolicy.LaterMaintain(1, 1, 2, 0, 0), "Actual establishing attack.");
            Assertions.False(SerpentineCommandReviewPolicy.LaterMaintain(1, 1, 2, 1, 2), "No second attack credited as maintain.");
        }

        internal static void NativePoisonSavePhasesRemainDistinct()
        {
            for (int exposure = 1; exposure <= 6; exposure++)
            {
                Assertions.True(SerpentinePoisonReviewPolicy.ExactExposureCounts(exposure,
                    exposure, 1, exposure - 1), "One injury gate, then native buff saves.");
                Assertions.False(SerpentinePoisonReviewPolicy.ExactExposureCounts(exposure,
                    exposure, 0, exposure), "Buff saves cannot substitute for the injury gate.");
                Assertions.False(SerpentinePoisonReviewPolicy.ExactExposureCounts(exposure,
                    exposure, 1, exposure), "No invented second save on initial application.");
                Assertions.False(SerpentinePoisonReviewPolicy.ExactExposureCounts(exposure,
                    exposure + 1, 1, exposure - 1), "Duplicate damage remains a failure.");
                Assertions.False(SerpentinePoisonReviewPolicy.ExactExposureCounts(exposure,
                    exposure, 2, exposure - 1), "Duplicate injury gate remains a failure.");
            }
            foreach (int invalid in new[] { -1, 0, 7, int.MaxValue })
                Assertions.False(SerpentinePoisonReviewPolicy.ExactExposureCounts(invalid,
                    invalid, 1, invalid - 1), "Only six native exposures are in scope.");
        }

        internal static void NativePoisonExhaustionRequiresRemovalWithoutReplay()
        {
            Assertions.True(SerpentinePoisonReviewPolicy.ExactExhaustion(6, 1, 5, false, 0, 0),
                "Six total saves/damage events, split into their actual native phases.");
            foreach (int damage in new[] { -1, 1, 2 })
            {
                Assertions.False(SerpentinePoisonReviewPolicy.ExactExhaustion(6, 1, 5, false, damage, 0),
                    "Exhaustion cannot mutate Constitution.");
                Assertions.False(SerpentinePoisonReviewPolicy.ExactExhaustion(6, 1, 5, false, 0, damage),
                    "Duplicate exhausted callback cannot mutate Constitution.");
            }
            Assertions.False(SerpentinePoisonReviewPolicy.ExactExhaustion(6, 1, 5, true, 0, 0),
                "A retained exhausted buff is not removal.");
            Assertions.False(SerpentinePoisonReviewPolicy.ExactExhaustion(7, 1, 6, false, 0, 0),
                "A seventh event fails even if its damage was masked.");
            Assertions.False(SerpentinePoisonReviewPolicy.ExactExhaustion(6, 0, 5, false, 0, 0),
                "The initial injury save must be positively observed.");
        }

        internal static void SignatureRequestIsClosedWorkingSaveSlice()
        {
            string scenario = RuntimeTestScenarioCatalog.DisposableExpandedSummoningSnakeSignatures;
            Assertions.Equal("disposable-expanded-summoning-snake-signatures", scenario, "Closed native rules slice.");
            Assertions.True(RuntimeTestScenarioCatalog.IsAllowed(scenario) &&
                RuntimeTestScenarioCatalog.IsExpandedSummoningRulesScenario(scenario),
                "The new request retains native working-save guards.");
            foreach (string invalid in new[] { scenario.ToUpperInvariant(), scenario + "-arbitrary",
                "working-save-expanded-summoning-snake-signatures" })
                Assertions.False(RuntimeTestScenarioCatalog.IsAllowed(invalid) ||
                    RuntimeTestScenarioCatalog.IsExpandedSummoningRulesScenario(invalid),
                    "No write-enabled alias or alternate asset selector.");
            Assertions.True(RuntimeTestScenarioCatalog.IsAllowed(
                RuntimeTestScenarioCatalog.DisposableExpandedSummoningSnakeProfiles),
                "The qualified closed38-check request remains independently available.");
        }

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
