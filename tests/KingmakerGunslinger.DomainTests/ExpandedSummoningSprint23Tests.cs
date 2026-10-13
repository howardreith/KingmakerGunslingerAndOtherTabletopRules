using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using KingmakerGunslinger.RuntimeTesting;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningSprint23Tests
    {
        internal const int AppendedLedgerIdentities = 33;
        internal const int ActiveLedgerIdentities = 32;

        private static readonly string[] PrivateTail = {
            ArmoredDinosaurRulesPolicy.TriceratopsGoreSymbol,
            ArmoredDinosaurRulesPolicy.StegosaurusTailSymbol,
            ArmoredDinosaurRulesPolicy.TriceratopsUnitTypeSymbol,
            ArmoredDinosaurRulesPolicy.StegosaurusUnitTypeSymbol,
            ArmoredDinosaurRulesPolicy.TriceratopsChassisSymbol,
            ArmoredDinosaurRulesPolicy.StegosaurusChassisSymbol,
            ArmoredDinosaurRulesPolicy.TriceratopsPowerfulChargeSymbol,
            ArmoredDinosaurRulesPolicy.TriceratopsTrampleSymbol,
            ArmoredDinosaurRulesPolicy.StegosaurusTripSymbol,
            ArmoredDinosaurRulesPolicy.StegosaurusTailTypeSymbol
        };

        internal static void RoutesAreExactlyTheCharteredTwelve()
        {
            ExpandedSummoningCatalog.Validate();
            SummonVisibilityCatalog.Validate();
            SummonVariantSpec[] monster = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster).Where(value =>
                    value.Creature.Key ==
                    ArmoredDinosaurRulesPolicy.TriceratopsKey).ToArray();
            SummonVariantSpec[] ally = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.NaturesAlly).Where(value =>
                    ArmoredDinosaurRulesPolicy.UsesExactRanks(
                        value.Creature.Key)).ToArray();
            Assertions.Equal(4, monster.Length,
                "Triceratops has four Monster roots.");
            Assertions.Equal(8, ally.Length,
                "Both animals have four Nature's Ally roots.");
            Assertions.Equal(0, ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster).Count(value =>
                    value.Creature.Key ==
                    ArmoredDinosaurRulesPolicy.StegosaurusKey),
                "Stegosaurus has no Monster route.");
            foreach (IGrouping<string, SummonVariantSpec> creature in
                monster.Concat(ally).GroupBy(value =>
                    value.Family + ":" + value.Creature.Key))
            {
                SummonVariantSpec[] routes = creature.OrderBy(value =>
                    value.ParentTier).ToArray();
                Assertions.True(routes.Select(value => value.ParentTier)
                    .SequenceEqual(new[] { 6, 7, 8, 9 }),
                    creature.Key + " parent tiers.");
                Assertions.True(routes.Select(value => value.Multiplicity)
                    .SequenceEqual(new[] { SummonMultiplicity.One,
                        SummonMultiplicity.OneD3,
                        SummonMultiplicity.OneD4PlusOne,
                        SummonMultiplicity.OneD4PlusOne }),
                    creature.Key + " quantities.");
            }
            Assertions.True(monster.All(value => value.Creature
                .MonsterTemplated), "Every Triceratops Monster root owns template children.");
            Assertions.Equal(1075,
                SummonVisibilityCatalog.RegisteredLogicalPlacementCount,
                "Twelve placements append.");
            Assertions.Equal(12,
                SummonVisibilityCatalog.SuppressedLogicalPlacementCount,
                "Both creatures remain hidden for candidate qualification.");
            Assertions.Equal(1063,
                SummonVisibilityCatalog.PublishedLogicalPlacementCount,
                "No released menu choice moves while hidden.");
        }

        internal static void ProfilesDeriveThePrintedLines()
        {
            ExpandedSummoningNaturalProfiles.Validate();
            NaturalSummonProfile tri = ExpandedSummoningNaturalProfiles.For(
                ArmoredDinosaurRulesPolicy.TriceratopsKey);
            NaturalSummonProfile stego = ExpandedSummoningNaturalProfiles.For(
                ArmoredDinosaurRulesPolicy.StegosaurusKey);
            Assertions.True(new[] { tri.HitDice, tri.Strength, tri.Dexterity,
                tri.Constitution, tri.NaturalArmor }.SequenceEqual(
                    new[] { 14, 26, 9, 19, 14 }),
                "Triceratops adult profile.");
            Assertions.True(new[] { stego.HitDice, stego.Strength,
                stego.Dexterity, stego.Constitution, stego.NaturalArmor }
                .SequenceEqual(new[] { 12, 27, 14, 17, 12 }),
                "Stegosaurus adult profile.");
            Assertions.Equal("TriceratopsGore2d10", tri.PrimaryWeapon,
                "One private 2d10 gore.");
            Assertions.Equal("StegosaurusTail4d6", stego.PrimaryWeapon,
                "One private 4d6 tail.");
            Assertions.True(tri.Facts.Contains("ImprovedCriticalGore") &&
                tri.Facts.Contains("WeaponFocusGore") &&
                tri.Facts.Contains("TriceratopsChassis"),
                "Triceratops feat and chassis carriers.");
            Assertions.Equal(15,
                ArmoredDinosaurRulesPolicy.CorrectTriceratopsGoreRange(
                    true, true, true, 12),
                "The exact enabled Triceratops gore closes the measured three-foot gap.");
            Assertions.True(new[] {
                ArmoredDinosaurRulesPolicy.CorrectTriceratopsGoreRange(
                    false, true, true, 12),
                ArmoredDinosaurRulesPolicy.CorrectTriceratopsGoreRange(
                    true, false, true, 12),
                ArmoredDinosaurRulesPolicy.CorrectTriceratopsGoreRange(
                    true, true, false, 12)
            }.All(value => value == 12),
                "Module-off, other-owner and other-weapon ranges remain native.");
            Assertions.Equal(24,
                ArmoredDinosaurRulesPolicy.TriceratopsPerceptionRanks + 3 +
                    1 + 3 + ArmoredDinosaurRulesPolicy
                        .TriceratopsSkillFocusCompletionBonus,
                "Triceratops completes Skill Focus's +6 at ten ranks.");
            Assertions.True(stego.Facts.Contains("Cleave") &&
                stego.Facts.Contains("StegosaurusTrip") &&
                stego.Facts.Contains("StegosaurusChassis"),
                "Stegosaurus feat, trip and chassis carriers.");
            Assertions.False(stego.Facts.Any(value =>
                value.IndexOf("Ankylosaurus", StringComparison.Ordinal) >= 0 ||
                value.IndexOf("Stun", StringComparison.Ordinal) >= 0 ||
                value.IndexOf("Trample", StringComparison.Ordinal) >= 0),
                "No Ankylosaurus or trample rider leaks to Stegosaurus.");
            Assertions.Equal(0, tri.AdditionalWeapons.Count +
                tri.AdditionalSecondaryWeapons.Count,
                "Triceratops has no bite or separate horn attacks.");
            Assertions.Equal(0, stego.AdditionalWeapons.Count +
                stego.AdditionalSecondaryWeapons.Count,
                "Stegosaurus has no follow-up attack.");
            string triDisposition = string.Join(" ", tri.Deviations);
            string stegoDisposition = string.Join(" ", stego.Deviations);
            Assertions.True(triDisposition.Contains("Run remains omitted") &&
                stegoDisposition.Contains("Improved Overrun retains") &&
                stegoDisposition.Contains("Cleave is the native feat/action"),
                "Unsupported Run/overrun consumers and native Cleave have bounded dispositions.");
        }

        internal static void TrampleUsesD8AndLiveArithmetic()
        {
            UngulateRulesProfile rules = UngulateRulesPolicy.For(
                ArmoredDinosaurRulesPolicy.TriceratopsKey);
            Assertions.True(rules.HasTrample && !rules.Stampede &&
                rules.LiveTrampleMath, "Exact Triceratops trample carrier.");
            Assertions.Equal(1, rules.TrampleDiceCount, "One trample die.");
            Assertions.Equal(8, rules.TrampleDieSides, "The die is d8, not d6.");
            Assertions.Equal(12, rules.TrampleBonusFor(8),
                "Baseline 1.5x Strength damage.");
            Assertions.Equal(15, rules.TrampleBonusFor(10),
                "Strength buff changes damage live.");
            Assertions.Equal(9, rules.TrampleBonusFor(6),
                "Strength penalty changes damage live.");
            Assertions.Equal(25, rules.TrampleSaveDc(0, 14, 8),
                "Printed baseline DC.");
            Assertions.Equal(27, rules.TrampleSaveDc(0, 14, 10),
                "Strength buff changes DC live.");
            Assertions.Equal(26, rules.TrampleSaveDc(0, 16, 8),
                "Racial HD changes DC live.");
            Assertions.True(rules.CanTrample(5, 4, 0),
                "Smaller target is legal.");
            Assertions.False(rules.CanTrample(5, 5, 0),
                "Same-size target is illegal without invented Stampede.");
        }

        internal static void PowerfulChargeIsOneEligibleStrike()
        {
            UngulateRulesProfile rules = UngulateRulesPolicy.For(
                ArmoredDinosaurRulesPolicy.TriceratopsKey);
            Assertions.True(rules.HasPowerfulCharge &&
                rules.LivePowerfulChargeMath,
                "Triceratops uses the live charge adapter.");
            Assertions.Equal(4, rules.ChargeDiceCount, "Printed 4d10.");
            Assertions.Equal(10, rules.ChargeDieSides, "Printed d10.");
            Assertions.Equal(4,
                ArmoredDinosaurRulesPolicy.PowerfulChargeLiveBonus(8),
                "Printed +16 is ordinary +12 plus live +4.");
            Assertions.Equal(5,
                ArmoredDinosaurRulesPolicy.PowerfulChargeLiveBonus(10),
                "Buffed Strength contribution remains live.");
            Assertions.True(rules.AppliesPowerfulCharge(true, true, true,
                false), "First genuine charge gore qualifies.");
            Assertions.False(rules.AppliesPowerfulCharge(false, true, true,
                false), "Ordinary gore cannot charge.");
            Assertions.False(rules.AppliesPowerfulCharge(true, false, true,
                false), "Later strike cannot replay charge.");
            Assertions.False(rules.AppliesPowerfulCharge(true, true, false,
                false), "Another weapon cannot charge.");
            Assertions.False(rules.AppliesPowerfulCharge(true, true, true,
                true), "Opportunity attack cannot charge.");
        }

        internal static void TailTripRejectsEveryFalsePositive()
        {
            Func<bool, bool, bool, bool, bool, bool, bool, int, int, bool>
                decide = ArmoredDinosaurRulesPolicy.CanAttemptTailTrip;
            Assertions.True(decide(true, true, true, true, true, false,
                false, 5, 6), "A new hit can trip one-size-larger target.");
            Assertions.False(decide(false, true, true, true, true, false,
                false, 5, 5), "Foreign owner.");
            Assertions.False(decide(true, false, true, true, true, false,
                false, 5, 5), "Foreign weapon.");
            Assertions.False(decide(true, true, false, true, true, false,
                false, 5, 5), "Miss.");
            Assertions.False(decide(true, true, true, false, true, false,
                false, 5, 5), "Destroyed target.");
            Assertions.False(decide(true, true, true, true, false, false,
                false, 5, 5), "Duplicate callback.");
            Assertions.False(decide(true, true, true, true, true, true,
                false, 5, 5), "Already prone target.");
            Assertions.False(decide(true, true, true, true, true, false,
                true, 5, 5), "Native maneuver immunity.");
            Assertions.False(decide(true, true, true, true, true, false,
                false, 5, 7), "Target more than one size larger.");
            Assertions.True(decide(true, true, true, true, true, false,
                false, 5, 5), "A separate genuine hit remains eligible.");
            string carrier = File.ReadAllText(
                "src/KingmakerGunslinger/Summoning/StegosaurusTailTrip.cs");
            Assertions.True(carrier.Contains("!target.Destroyed") &&
                carrier.Contains("!owner.Destroyed"),
                "Destroyed source and target identities fail closed.");
            Assertions.False(carrier.Contains("RuleDealDamage") ||
                carrier.Contains("Damage > 0") ||
                carrier.Contains("DamageDealt"),
                "Trip eligibility is not gated on positive HP damage.");
            Assertions.True(carrier.Contains("ownerWasProne") &&
                carrier.Contains("SummonManeuverChecks.Succeeded") &&
                carrier.Contains("AddCondition(UnitCondition.Prone") &&
                carrier.Contains("RemoveConditionAll(UnitCondition.Prone)"),
                "Native success applies prone; failure removes only its own backlash.");
        }

        internal static void LedgerAppendIsExactAndAllocatedOnce()
        {
            IReadOnlyList<SummoningIdentitySpec> planned =
                ExpandedSummoningIdentityCatalog.Build();
            Assertions.Equal(2101, planned.Count,
                "Foundation gains exactly thirty-two active identities.");
            Assertions.True(planned.Skip(planned.Count - PrivateTail.Length)
                .Select(value => value.Symbol).SequenceEqual(PrivateTail),
                "Ten active private carriers append in frozen order.");
            JObject manifest = JObject.Parse(File.ReadAllText(
                "blueprints/blueprints.json"));
            JToken[] entries = manifest["entries"].ToArray();
            var bySymbol = entries.ToDictionary(value =>
                (string)value["symbol"], value => (string)value["guid"],
                StringComparer.Ordinal);
            Assertions.Equal(3101, entries.Length,
                "Manifest total includes the rejected gore type as a third reserved identity.");
            JToken retired = entries.Single(value => (string)value["symbol"] ==
                "KMG.Summoning.Natural.Triceratops.GoreType");
            Assertions.Equal("reserved", (string)retired["status"],
                "The rejected private gore type remains append-only and unavailable.");
            foreach (string symbol in planned.Select(value => value.Symbol))
                Assertions.True(bySymbol.ContainsKey(symbol),
                    "Allocated identity exists: " + symbol);
            Assertions.Equal(entries.Length, entries.Select(value =>
                (string)value["guid"]).Distinct(StringComparer.Ordinal).Count(),
                "No GUID is reused.");
        }

        internal static void OriginalBodiesAndIconsOwnExactSilhouettes()
        {
            var expected = new Dictionary<string, string[]>(
                StringComparer.Ordinal)
            {
                { ArmoredDinosaurRulesPolicy.TriceratopsKey,
                    ArmoredDinosaurVisualPolicy.TriceratopsBones },
                { ArmoredDinosaurRulesPolicy.StegosaurusKey,
                    ArmoredDinosaurVisualPolicy.StegosaurusBones }
            };
            foreach (KeyValuePair<string, string[]> entry in expected)
            {
                string key = entry.Key;
                string path = "assets/sprint23-armored-dinosaurs/" +
                    key + "-mesh.json";
                JObject payload = JObject.Parse(File.ReadAllText(path));
                Assertions.Equal(key, (string)payload["creature"],
                    key + " asset identity.");
                Assertions.True(ArmoredDinosaurVisualPolicy.IsPermitted(
                    key, payload["bones"].Values<string>()),
                    key + " exact driver set.");
                Assertions.Equal(4, (int)payload["visibleLegs"],
                    key + " is a quadruped.");
                Assertions.False((bool)payload["terminalClub"],
                    key + " has no Ankylosaurus club.");
                Assertions.Equal(ArmoredDinosaurVisualPolicy.DonorGuidFor(key),
                    (string)payload["nativeDonor"],
                    key + " measured rig owner.");
            }
            JObject tri = JObject.Parse(File.ReadAllText(
                "assets/sprint23-armored-dinosaurs/triceratops-mesh.json"));
            JObject stego = JObject.Parse(File.ReadAllText(
                "assets/sprint23-armored-dinosaurs/stegosaurus-mesh.json"));
            Assertions.True((int)tri["horns"] == 3 &&
                (bool)tri["frill"] && (int)tri["dorsalPlates"] == 0,
                "Triceratops owns three horns and a frill, not plates.");
            Assertions.True((int)stego["horns"] == 0 &&
                !(bool)stego["frill"] && (int)stego["dorsalPlates"] == 9 &&
                (int)stego["tailSpikes"] == 4 &&
                (int)stego["tailJoints"] == 6,
                "Stegosaurus owns plates and four spikes, not horns or club.");
            Assertions.Equal(
                "ec2e38eae2cb3279a003ec2cf507f4eb30e4e3153905a60a697940833cd8c914",
                Sha256("assets/sprint23-armored-dinosaurs/triceratops-mesh.json"),
                "Deterministic Triceratops mesh bytes.");
            Assertions.Equal(
                "02a37218c41c09ed9962ac39a8e01ad0e9d99d8f07d718e1e27d9b9b7573582f",
                Sha256("assets/sprint23-armored-dinosaurs/stegosaurus-mesh.json"),
                "Deterministic Stegosaurus mesh bytes.");

            JObject icons = JObject.Parse(File.ReadAllText(
                "assets-source/original-icons/expanded-summoning/icon-manifest.json"));
            Assertions.Equal(117, (int)icons["count"],
                "Two distinct icon concepts append.");
            JToken[] armored = icons["icons"].Where(value =>
                ArmoredDinosaurVisualPolicy.Handles((string)value["key"]))
                .ToArray();
            Assertions.Equal(2, armored.Length,
                "Each armored dinosaur has one original painting.");
            Assertions.True(armored.Select(value =>
                (string)value["outputSha256"]).Distinct(
                    StringComparer.Ordinal).Count() == 2 &&
                armored.All(value => value["blueprintSymbols"].Any()),
                "Paintings are distinct and own concrete consumers.");
        }

        private static string Sha256(string path)
        {
            using (SHA256 hash = SHA256.Create())
                return string.Concat(hash.ComputeHash(File.ReadAllBytes(path))
                    .Select(value => value.ToString("x2")));
        }

        internal static void ReleasedControlsStayDistinct()
        {
            NaturalSummonProfile anky = ExpandedSummoningNaturalProfiles.For(
                AnkylosaurusRulesPolicy.Key);
            Assertions.Equal("Tail3d6", anky.PrimaryWeapon,
                "Ankylosaurus weapon remains unchanged.");
            Assertions.True(anky.Facts.Contains("AnkylosaurusStun") &&
                !anky.Facts.Contains("StegosaurusTrip"),
                "Ankylosaurus keeps only its own control rider.");
            Assertions.Equal(2,
                AnkylosaurusRulesPolicy.PowerAttackAdditionalDamage,
                "BAB +7 released correction remains +2.");
            Assertions.Equal(3,
                ArmoredDinosaurRulesPolicy.PowerAttackAdditionalDamage,
                "BAB +9/+10 Sprint 23 correction is independently +3.");
            UngulateRulesProfile rhino = UngulateRulesPolicy.For(
                "rhinoceros");
            Assertions.False(rhino.LivePowerfulChargeMath ||
                rhino.LiveTrampleMath,
                "Released Rhino stays on its frozen adapter path.");
            UngulateRulesProfile woolly = UngulateRulesPolicy.For(
                "woolly-rhinoceros");
            Assertions.Equal(6, woolly.TrampleDieSides,
                "Released d6 trampler stays d6.");
            Assertions.Equal(13, woolly.TrampleBonusFor(99),
                "Released trample ignores the new live input.");
            string build = File.ReadAllText("scripts/Build-Local.ps1");
            string packageValidator = File.ReadAllText(
                "scripts/validate-package.ps1");
            const string releasedAnkylosaurusMesh =
                "f8fa71cadd3fdef3a59bc461a6ca0f51ca151fc59e7740596ab50fcb6d247626";
            Assertions.True(build.Contains("ankylosaurusMeshText.Replace") &&
                build.Contains(releasedAnkylosaurusMesh) &&
                packageValidator.Contains(releasedAnkylosaurusMesh),
                "A fresh checkout must reproduce the exact released Ankylosaurus mesh bytes.");
        }

        internal static void NativeReviewScenarioIsExactAndBounded()
        {
            string catalog = File.ReadAllText(
                "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestScenarioCatalog.cs");
            string request = File.ReadAllText(
                "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRequest.cs");
            string policy = File.ReadAllText(
                "src/KingmakerGunslinger/RuntimeTesting/ExpandedSummoningNativeProfilePolicy.cs");
            const string scenario =
                "disposable-expanded-summoning-sprint23-native-review";
            Assertions.True(catalog.Contains(scenario) &&
                catalog.Contains("IsSummoningNativeProfile"),
                "The Sprint 23 review is an allowlisted native-profile scenario.");
            Assertions.True(request.Contains("Sprint23ReviewPhase") &&
                request.Contains("rtwp-foundation") &&
                request.Contains("turn-based-control") &&
                request.Contains("ExitAfterCompletion"),
                "Only the two exact automatically exiting review phases are accepted.");
            Assertions.True(policy.Contains("SaveFreeParameterScope") &&
                policy.Contains("Sprint23NativeReview"),
                "Sprint 23 inherits the qualified save-free parameter boundary.");
            Assertions.True(ExpandedSummoningRosterPersistencePolicy
                    .ArmoredDinosaurKeys.SequenceEqual(new[] {
                        ArmoredDinosaurRulesPolicy.TriceratopsKey,
                        ArmoredDinosaurRulesPolicy.StegosaurusKey }) &&
                ExpandedSummoningRosterPersistencePolicy.ObserveLoad(
                    RuntimeTestScenarioCatalog
                        .WorkingSaveExpandedSummoningPrepare,
                    ExpandedSummoningRosterPersistencePolicy
                        .ArmoredDinosaursScope),
                "The existing persistence trilogy has one exact two-creature Sprint 23 scope.");
        }

        internal static void HiddenPersistenceSelectsRegisteredCreatureRoutes()
        {
            foreach (string key in ExpandedSummoningRosterPersistencePolicy
                .ArmoredDinosaurKeys)
            {
                Assertions.True(ExpandedSummoningCatalog.GenerateVariants(
                        SummonFamily.Monster).Concat(ExpandedSummoningCatalog
                        .GenerateVariants(SummonFamily.NaturesAlly)).Where(value =>
                            value.Creature.Key == key && value.Multiplicity ==
                            SummonMultiplicity.One).All(value =>
                                !SummonVisibilityCatalog.IsPublished(value)),
                    "Sprint 23 persistence starts while every new route is still hidden: " + key);
                SummonVariantSpec selected = ExpandedSummoningRosterPersistencePolicy
                    .SelectCreatureVariant(ExpandedSummoningRosterPersistencePolicy
                        .ArmoredDinosaursScope, key);
                Assertions.Equal(key, selected.Creature.Key,
                    "The named hidden scope selects its registered creature identity.");
                Assertions.Equal(SummonMultiplicity.One, selected.Multiplicity,
                    "Persistence never creates a quantity-band group.");
            }
            bool broadRejected = false;
            try
            {
                ExpandedSummoningRosterPersistencePolicy.SelectCreatureVariant(
                    ExpandedSummoningRosterPersistencePolicy.Scope,
                    ArmoredDinosaurRulesPolicy.TriceratopsKey);
            }
            catch (InvalidOperationException) { broadRejected = true; }
            Assertions.True(broadRejected,
                "The broad released-roster fixture cannot bypass suppression.");
            bool crossScopeRejected = false;
            try
            {
                ExpandedSummoningRosterPersistencePolicy.SelectCreatureVariant(
                    ExpandedSummoningRosterPersistencePolicy.AnkylosaurusScope,
                    ArmoredDinosaurRulesPolicy.StegosaurusKey);
            }
            catch (InvalidOperationException) { crossScopeRejected = true; }
            Assertions.True(crossScopeRejected,
                "A targeted scope cannot select another sprint's identity.");
        }

        internal static void NativeReviewObservesRealCommandsAndControls()
        {
            string review = File.ReadAllText(
                "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint23Review.cs");
            foreach (string required in new[] {
                "Sprint18RunFullAttack",
                "Sprint23NativeChargeCommand",
                "Sprint23NativeTrampleCommand",
                "new UnitUseAbility",
                "UnitActionController",
                "nativeEconomy",
                "AwakeUnits",
                "Math.Max(0, owner.Stats.Dexterity.Bonus)",
                "PlaceExpandedSummoningUnit(owner, floor)",
                "damageEvents",
                "Sprint23LeaveTurnBasedBeforeDisposal",
                "KMG_Summoning_Special_Triceratops_Trample",
                "queued-native-charge",
                "queued-native-trample-path-contact",
                "released-rhino-charge-control",
                "Sprint23AnkylosaurusControl",
                "CleanupSprint23NativeReview"
            })
                Assertions.True(review.Contains(required),
                    "Native review owns required discriminator: " + required);
            Assertions.False(review.Contains(
                "new ExpandedSummoningTrampleContactResult {\r\n                                Label = \"native-path\""),
                "Native-path evidence cannot serialize an unpopulated synthetic contact.");
        }

        internal static void NativeReviewScriptsRejectLooseAdmission()
        {
            string common = File.ReadAllText(
                "scripts/RuntimeAutomation.Common.ps1");
            string profile = File.ReadAllText(
                "scripts/compatibility/Invoke-KingmakerCompatibilityProfile.ps1");
            string preflight = File.ReadAllText(
                "scripts/Test-RuntimeScenarioPreflight.ps1");
            const string scenario =
                "disposable-expanded-summoning-sprint23-native-review";
            Assertions.True(common.Contains(scenario) &&
                common.Contains("Test-KmgSprint23ReviewPhaseScope"),
                "The common runtime boundary validates the exact Sprint 23 phase.");
            Assertions.True(profile.Contains(scenario) &&
                profile.Contains("ReuseInstalledArtifact"),
                "The profile wrapper recognizes the scenario without conflating artifact reuse interfaces.");
            Assertions.True(preflight.Contains(scenario) &&
                preflight.Contains("turn-based-control") &&
                preflight.Contains("rtwp-foundation"),
                "Offline preflight exercises both exact review phases.");
        }
    }
}
