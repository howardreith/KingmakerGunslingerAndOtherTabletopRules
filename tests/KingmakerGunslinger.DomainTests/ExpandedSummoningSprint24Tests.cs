using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using KingmakerGunslinger.RuntimeTesting;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningSprint24Tests
    {
        internal const int AppendedLedgerIdentities =
            TheropodRulesPolicy.AppendedIdentityCount;

        private static readonly string[] PrivateIdentityTail = {
            TheropodRulesPolicy.DeinonychusTalonSymbol,
            TheropodRulesPolicy.DeinonychusUnitTypeSymbol,
            TheropodRulesPolicy.DeinonychusChassisSymbol,
            TheropodRulesPolicy.TyrannosaurusBiteSymbol,
            TheropodRulesPolicy.TyrannosaurusUnitTypeSymbol,
            TheropodRulesPolicy.TyrannosaurusChassisSymbol,
            TheropodRulesPolicy.TyrannosaurusPowerfulBiteSymbol,
            TheropodRulesPolicy.TyrannosaurusCombatTraitsSymbol,
            TheropodRulesPolicy.TyrannosaurusSwallowedSymbol,
            TheropodRulesPolicy.TyrannosaurusBleedingCriticalSymbol
        };

        internal static void RoutesAreTheExactEighteenHiddenRoots()
        {
            ExpandedSummoningCatalog.Validate();
            SummonVisibilityCatalog.Validate();

            SummonVariantSpec[] monster = NewRoutes(SummonFamily.Monster);
            SummonVariantSpec[] ally = NewRoutes(SummonFamily.NaturesAlly);
            Assertions.Equal(9, monster.Length,
                "The two theropods add nine Monster roots.");
            Assertions.Equal(9, ally.Length,
                "The two theropods add nine Nature's Ally roots.");

            AssertRouteShape(monster, TheropodRulesPolicy.DeinonychusKey,
                new[] { 4, 5, 6, 7, 8, 9 });
            AssertRouteShape(ally, TheropodRulesPolicy.DeinonychusKey,
                new[] { 4, 5, 6, 7, 8, 9 });
            AssertRouteShape(monster, TheropodRulesPolicy.TyrannosaurusKey,
                new[] { 7, 8, 9 });
            AssertRouteShape(ally, TheropodRulesPolicy.TyrannosaurusKey,
                new[] { 7, 8, 9 });

            Assertions.True(monster.All(value => value.Creature
                .TemplatePolicy(SummonFamily.Monster) ==
                    SummonTemplatePolicy.CelestialOrFiendish),
                "Every new Monster root uses only the released templates.");
            Assertions.True(ally.All(value => value.Creature
                .TemplatePolicy(SummonFamily.NaturesAlly) ==
                    SummonTemplatePolicy.CasterAlignment),
                "Every new Ally root stays untemplated under caster alignment.");
            Assertions.True(monster.Concat(ally).All(value =>
                    !SummonVisibilityCatalog.IsPublished(value)),
                "All eighteen new roots remain hidden in the candidate.");
            Assertions.Equal(107, ExpandedSummoningCatalog.All.Count,
                "Exactly two creatures append to the released catalog.");
            Assertions.Equal(TheropodRulesPolicy.RegisteredLogicalPlacements,
                SummonVisibilityCatalog.RegisteredLogicalPlacementCount,
                "The current global route authority is centralized.");
            Assertions.Equal(TheropodRulesPolicy.HiddenLogicalPlacements,
                SummonVisibilityCatalog.SuppressedLogicalPlacementCount,
                "Exactly the two Sprint 24 route sets are withheld.");
            Assertions.Equal(
                TheropodRulesPolicy.HiddenPublishedLogicalPlacements,
                SummonVisibilityCatalog.PublishedLogicalPlacementCount,
                "No released route moves while the candidate is hidden.");
        }

        internal static void DeinonychusHasOneMixedRoleFourAttackRoutine()
        {
            ExpandedSummoningNaturalProfiles.Validate();
            NaturalSummonProfile profile = ExpandedSummoningNaturalProfiles
                .For(TheropodRulesPolicy.DeinonychusKey);

            Assertions.True(new[] { profile.HitDice, profile.Strength,
                profile.Dexterity, profile.Constitution, profile.Intelligence,
                profile.Wisdom, profile.Charisma, profile.SpeedFeet,
                profile.NaturalArmor }.SequenceEqual(new[] {
                    4, 15, 15, 19, 2, 12, 14, 60, 3 }),
                "Deinonychus derives the adult profile.");
            Assertions.Equal("Medium", profile.Size,
                "Deinonychus stays Medium.");
            Assertions.Equal("DeinonychusTalon1d8", profile.PrimaryWeapon,
                "The first talon is primary.");
            Assertions.True(profile.AdditionalWeapons.SequenceEqual(new[] {
                    "DeinonychusTalon1d8", "Bite1d6" }),
                "The second talon and bite are primary attacks.");
            Assertions.True(profile.AdditionalSecondaryWeapons.SequenceEqual(
                    new[] { "Claw1d4" }),
                "Plural foreclaws are one secondary rolled attack.");
            Assertions.Equal(4, 1 + profile.AdditionalWeapons.Count +
                profile.AdditionalSecondaryWeapons.Count,
                "The full routine has exactly four attacks.");
            Assertions.True(profile.Facts.Contains("Pounce") &&
                profile.Facts.Contains("ImprovedInitiative") &&
                profile.Facts.Contains("DeinonychusChassis"),
                "Only the exact printed feat, native Pounce and chassis are carried.");
            string[] forbidden = { "Rake", "Rend", "Grab", "Swallow",
                "Fear", "Poison", "PowerfulCharge", "TripDefenseFourLegs" };
            Assertions.False(profile.Facts.Any(fact => forbidden.Any(value =>
                    fact.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0)),
                "No cat, ape, grappler, poison or quadruped carrier leaks in.");
        }

        internal static void TyrannosaurusHasOnePowerfulBiteAndPrintedFeats()
        {
            NaturalSummonProfile profile = ExpandedSummoningNaturalProfiles
                .For(TheropodRulesPolicy.TyrannosaurusKey);

            Assertions.True(new[] { profile.HitDice, profile.Strength,
                profile.Dexterity, profile.Constitution, profile.Intelligence,
                profile.Wisdom, profile.Charisma, profile.SpeedFeet,
                profile.NaturalArmor }.SequenceEqual(new[] {
                    18, 32, 13, 19, 2, 15, 10, 40, 14 }),
                "Tyrannosaurus derives the adult profile.");
            Assertions.Equal("Gargantuan", profile.Size,
                "Tyrannosaurus stays Gargantuan.");
            Assertions.Equal("TyrannosaurusBite4d6", profile.PrimaryWeapon,
                "Its only attack is the private 4d6 bite.");
            Assertions.Equal(0, profile.AdditionalWeapons.Count +
                profile.AdditionalSecondaryWeapons.Count,
                "No claws, tail or follow-up attack are invented.");
            foreach (string fact in new[] { "CriticalFocus", "Diehard",
                "Endurance", "ImprovedCriticalBite", "ImprovedInitiative",
                "IronWill", "SkillFocusPerception",
                "TyrannosaurusChassis", "TyrannosaurusPowerfulBite",
                "TyrannosaurusBleedingCritical" })
                Assertions.True(profile.Facts.Contains(fact),
                    "Printed or exact carrier is present: " + fact);
            Assertions.False(profile.Facts.Contains("PowerAttack") ||
                profile.Facts.Contains("Pounce"),
                "Power Attack and Pounce are not inferred from dinosaur flavor.");
        }

        internal static void HitPointsRanksAndRacialSkillsAreDerived()
        {
            TheropodRulesPolicy.Validate();
            Assertions.Equal(34,
                TheropodRulesPolicy.DeinonychusBaseRacialHitPoints + 4 * 4,
                "Four d8 animal hit dice derive 34 hp from the frozen base.");
            Assertions.Equal(153,
                TheropodRulesPolicy.TyrannosaurusBaseRacialHitPoints + 18 * 4,
                "Eighteen d8 animal hit dice derive 153 hp from the frozen base.");
            Assertions.Equal(14, TheropodRulesPolicy.DeinonychusPerceptionRanks +
                3 + 1 + TheropodRulesPolicy.DeinonychusRacialSkillBonus,
                "Deinonychus Perception is ranks, class, Wisdom and racial bonus.");
            Assertions.Equal(15, TheropodRulesPolicy.DeinonychusStealthRanks +
                3 + 2 + TheropodRulesPolicy.DeinonychusRacialSkillBonus,
                "Deinonychus Stealth is ranks, class, Dexterity and racial bonus.");
            Assertions.Equal(37, TheropodRulesPolicy.TyrannosaurusPerceptionRanks +
                3 + 2 + TheropodRulesPolicy.TyrannosaurusRacialPerceptionBonus +
                3 + TheropodRulesPolicy.TyrannosaurusSkillFocusCompletionBonus,
                "Tyrannosaurus Perception derives every printed contribution.");
        }

        internal static void PowerfulBiteCorrectsOnlyTheSignedStrengthTerm()
        {
            var expectedCorrection = new Dictionary<int, int> {
                { 9, 5 }, { 10, 5 }, { 11, 6 }, { 12, 6 },
                { 0, 0 }, { -1, -1 }, { -5, -5 }
            };
            foreach (KeyValuePair<int, int> sample in expectedCorrection)
            {
                int native = TheropodRulesPolicy
                    .NativeOrdinaryBiteStrengthContribution(sample.Key);
                int correction = TheropodRulesPolicy.PowerfulBiteCorrection(
                    sample.Key);
                Assertions.Equal(sample.Value, correction,
                    "Only the missing signed Strength difference is added for " +
                    sample.Key + ".");
                Assertions.Equal(sample.Key * 2, checked(native + correction),
                    "The final bite Strength contribution is exactly twice " +
                    sample.Key + ".");
            }
            Assertions.Equal(22,
                TheropodRulesPolicy.NativeOrdinaryBiteStrengthContribution(11) +
                TheropodRulesPolicy.PowerfulBiteCorrection(11),
                "Strength 32 yields the printed +22 bite term.");
            foreach (KeyValuePair<int, int> sample in
                new Dictionary<int, int> {
                    { 2, 1 }, { 3, 1 }, { 0, 0 }, { -1, 0 } })
                Assertions.Equal(sample.Value, TheropodRulesPolicy
                    .MultipleNaturalPrimaryExcessStrength(sample.Key),
                    "The exact lead talon removes only the excess native " +
                    "single-natural-attack Strength term for " + sample.Key + ".");

            string builder = File.ReadAllText(
                "src/KingmakerGunslinger/Blueprints/ExpandedSummoningNaturalBuilder.cs");
            string chassis = File.ReadAllText(
                "src/KingmakerGunslinger/Blueprints/ExpandedSummoningNaturalBuilder.Theropods.cs");
            Assertions.True(builder.Contains(
                    "if (TheropodRulesPolicy.IsTheropod(profile.Key)) unit.Race = null") &&
                chassis.Contains("DeinonychusPrimaryTalonStrength") &&
                chassis.Contains("strength.OwningBlueprint = owner") &&
                chassis.Contains("strength.Talon = talon"),
                "Theropods discard the playable donor race and the lead talon correction is exact-owner/exact-weapon scoped.");
        }

        internal static void GrabAndLaterSwallowHaveDifferentSizeGates()
        {
            Assertions.True(TheropodRulesPolicy.CanGrab(3, 3),
                "A Gargantuan Tyrannosaurus may Grab a same-size target.");
            Assertions.False(TheropodRulesPolicy.CanGrab(3, 4),
                "Grab does not accept a larger target.");
            Assertions.False(TheropodRulesPolicy.CanSwallow(3, 3),
                "A same-size mouth hold never makes Swallow Whole legal.");
            Assertions.True(TheropodRulesPolicy.CanSwallow(3, 2),
                "Swallow Whole accepts a target one size smaller.");
            Assertions.Equal(4, TheropodRulesPolicy.TyrannosaurusGrappleBonus,
                "The exact bite grapple owns the printed +4 contribution.");
            Assertions.Equal(2,
                TheropodRulesPolicy.TyrannosaurusSwallowDiceCount,
                "Periodic interior damage rolls two dice.");
            Assertions.Equal(11,
                TheropodRulesPolicy.TyrannosaurusSwallowFlatDamage,
                "Periodic interior damage uses +11, not Powerful Bite +22.");
        }

        internal static void BleedingCriticalRejectsEveryFalsePositive()
        {
            Func<bool, bool, bool, bool, bool, bool, bool, bool> decide =
                TheropodRulesPolicy.ShouldApplyBleedingCritical;
            Assertions.True(decide(true, true, true, true, false, true, true),
                "The first confirmed exact bite critical may bleed.");
            Assertions.False(decide(false, true, true, true, false, true, true),
                "Another owner cannot bleed.");
            Assertions.False(decide(true, false, true, true, false, true, true),
                "Another weapon cannot bleed.");
            Assertions.False(decide(true, true, false, true, false, true, true),
                "A miss cannot bleed.");
            Assertions.False(decide(true, true, true, false, false, true, true),
                "A threat without native confirmation cannot bleed.");
            Assertions.False(decide(true, true, true, true, true, true, true),
                "Critical immunity is respected.");
            Assertions.False(decide(true, true, true, true, false, false, true),
                "An unavailable target cannot receive the buff.");
            Assertions.False(decide(true, true, true, true, false, true, false),
                "A duplicate callback cannot apply bleed twice.");
        }

        internal static void IdentityAppendOwnsExactlyTenPrivateCarriers()
        {
            IReadOnlyList<SummoningIdentitySpec> identities =
                ExpandedSummoningIdentityCatalog.Build();
            Assertions.Equal(2149, identities.Count,
                "The foundation appends exactly forty-eight Sprint 24 identities.");
            SummoningIdentitySpec[] tail = identities
                .Skip(identities.Count - PrivateIdentityTail.Length).ToArray();
            Assertions.True(tail.Select(value => value.Symbol)
                    .SequenceEqual(PrivateIdentityTail),
                "The ten private theropod carriers append once in frozen order.");
            var types = tail.ToDictionary(value => value.Symbol,
                value => value.PlannedType, StringComparer.Ordinal);
            Assertions.Equal("BlueprintItemWeapon",
                types[TheropodRulesPolicy.DeinonychusTalonSymbol],
                "The talon is a private weapon.");
            Assertions.Equal("BlueprintItemWeapon",
                types[TheropodRulesPolicy.TyrannosaurusBiteSymbol],
                "The powerful bite is a private weapon.");
            Assertions.Equal("BlueprintBuff",
                types[TheropodRulesPolicy.TyrannosaurusSwallowedSymbol],
                "The Tyrannosaurus owns its distinct periodic payload.");
        }

        internal static void OriginalBodiesIconsAndRendererOwnershipAreExact()
        {
            foreach (string key in TheropodVisualPolicy.Keys)
            {
                JObject payload = JObject.Parse(File.ReadAllText(
                    "assets/sprint24-theropods/" + key + "-mesh.json"));
                Assertions.Equal(key, (string)payload["creature"],
                    key + " asset identity.");
                Assertions.True(TheropodVisualPolicy.IsPermitted(key,
                        payload["bones"].Values<string>()),
                    key + " owns the exact reviewed driver set.");
                Assertions.True((bool)payload["biped"] &&
                    (int)payload["visibleLegs"] == 2 &&
                    (int)payload["visibleForelimbs"] == 2 &&
                    (int)payload["jawJoints"] == 2 &&
                    (int)payload["tailJoints"] == 4 &&
                    (bool)payload["balancedCountertail"],
                    key + " is the reviewed two-legged jaw/countertail body.");
                Assertions.Equal(TheropodVisualPolicy.DonorGuid,
                    (string)payload["nativeDonor"],
                    key + " binds only the measured donor identity.");
                Assertions.Equal(TheropodVisualPolicy.DonorPrefabGuid,
                    (string)payload["nativePrefab"],
                    key + " binds only the measured donor prefab.");
                Assertions.Equal(TheropodVisualPolicy.BodyRendererName,
                    (string)payload["bodyRenderer"],
                    key + " swaps only the measured body renderer.");
                Assertions.Equal(TheropodVisualPolicy.AccessoryRendererName,
                    (string)payload["accessoryRenderer"],
                    key + " names the exact reversible accessory.");
            }
            JObject dein = JObject.Parse(File.ReadAllText(
                "assets/sprint24-theropods/deinonychus-mesh.json"));
            JObject tyr = JObject.Parse(File.ReadAllText(
                "assets/sprint24-theropods/tyrannosaurus-mesh.json"));
            Assertions.True((int)dein["sickleFootClaws"] == 2 &&
                    !(bool)dein["massiveSkull"] &&
                    !(bool)dein["tinyForearms"],
                "Deinonychus owns two visible sickle claws and agile anatomy.");
            Assertions.True((int)tyr["sickleFootClaws"] == 0 &&
                    (bool)tyr["massiveSkull"] &&
                    (bool)tyr["tinyForearms"],
                "Tyrannosaurus owns the large skull and tiny forearms.");
            Assertions.Equal(
                "eb1a06fb8933d603bfb89655df82d6e240bbf861bbe98d4134c9e25df4ed16ab",
                Sha256("assets/sprint24-theropods/deinonychus-mesh.json"),
                "Deterministic Deinonychus mesh bytes.");
            Assertions.Equal(
                "308a9a9950227b538ce9e7c068e090bf9e81a95fb6a573e8e8a0a4ea62a9ae87",
                Sha256("assets/sprint24-theropods/tyrannosaurus-mesh.json"),
                "Deterministic Tyrannosaurus mesh bytes.");
            Assertions.Equal(
                "4e0414763bc70122a4645402724c27c6dadd72fd9341ff1b25fad4bd57c236a7",
                Sha256("assets/sprint24-theropods/deinonychus-albedo.png"),
                "Deterministic Deinonychus original paint bytes.");
            Assertions.Equal(
                "3a4f09eabf7095c6f79fc4a7f0d230c458bdc2b7838fec6faaac86884b5ef7e8",
                Sha256("assets/sprint24-theropods/tyrannosaurus-albedo.png"),
                "Deterministic Tyrannosaurus original paint bytes.");
            Assertions.True(TheropodVisualPolicy.IsExactRendererSet(new[] {
                    TheropodVisualPolicy.AccessoryRendererName,
                    TheropodVisualPolicy.BodyRendererName }) &&
                !TheropodVisualPolicy.IsExactRendererSet(new[] {
                    TheropodVisualPolicy.BodyRendererName }) &&
                !TheropodVisualPolicy.IsExactRendererSet(new[] {
                    TheropodVisualPolicy.BodyRendererName,
                    TheropodVisualPolicy.AccessoryRendererName, "foreign" }),
                "Only the exact two-renderer donor view is eligible.");
            float scale;
            Assertions.True(SummonViewScaleCatalog.TryGetMultiplier(
                    TheropodVisualPolicy.TyrannosaurusBlueprintName,
                    out scale) && scale == 2.0f,
                "Only Tyrannosaurus receives the reviewed Gargantuan view step.");
            Assertions.False(SummonViewScaleCatalog.TryGetMultiplier(
                    TheropodVisualPolicy.DeinonychusBlueprintName, out scale),
                "Medium Deinonychus retains the measured donor scale.");

            string attachment = File.ReadAllText(
                "src/KingmakerGunslinger/Summoning/ExpandedSummoningPteranodonViewPatch.cs");
            Assertions.True(attachment.Contains(
                    "class TheropodAccessorySuppression") &&
                attachment.Contains("private void LateUpdate()") &&
                attachment.Contains("attachment.TheropodSuppression.Release()") &&
                attachment.Contains("attachment.TheropodSuppression })"),
                "Per-view suppression survives native fade updates, appears in the owned-resource receipt and releases before exact donor restoration.");

            JObject icons = JObject.Parse(File.ReadAllText(
                "assets-source/original-icons/expanded-summoning/icon-manifest.json"));
            JToken[] rows = icons["icons"].Where(value =>
                TheropodVisualPolicy.Handles((string)value["key"])).ToArray();
            Assertions.Equal(2, rows.Length,
                "Each theropod owns one original icon painting.");
            Assertions.True(rows.Select(value => (string)value["outputSha256"])
                    .Distinct(StringComparer.Ordinal).Count() == 2 &&
                rows.All(value => value["blueprintSymbols"].Any()),
                "Theropod paintings are distinct and own concrete consumers.");

            string package = File.ReadAllText("scripts/package.ps1");
            string packager = File.ReadAllText(
                "tools/create_deterministic_package.py");
            Assertions.True(package.Contains("$basePackageFileCount = 368") &&
                    package.Contains("$basePackageFileCount + 2") &&
                    package.Contains(
                        "--expected-file-count $expectedPackageFileCount") &&
                    packager.Contains("364, 368, 370))"),
                "The strict package inventory adds exactly four bodies and two icons.");
        }

        internal static void NativeReviewIsClosedAndObservesRequiredSeams()
        {
            const string scenario =
                "disposable-expanded-summoning-sprint24-native-review";
            string catalog = File.ReadAllText(
                "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestScenarioCatalog.cs");
            string request = File.ReadAllText(
                "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRequest.cs");
            string runner = File.ReadAllText(
                "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint24Review.cs");
            string common = File.ReadAllText(
                "scripts/RuntimeAutomation.Common.ps1");
            string preflight = File.ReadAllText(
                "scripts/Test-RuntimeScenarioPreflight.ps1");
            string compatibility = File.ReadAllText(
                "scripts/compatibility/Invoke-KingmakerCompatibilityProfile.ps1");
            string[] phases = { "rtwp-foundation", "rtwp-deinonychus",
                "turn-based-deinonychus", "rtwp-tyrannosaurus",
                "turn-based-tyrannosaurus", "visual-lifecycle",
                "shared-controls" };

            Assertions.True(catalog.Contains(scenario) &&
                request.Contains("Sprint24ReviewPhase") &&
                common.Contains("Test-KmgSprint24ReviewPhaseScope") &&
                preflight.Contains(scenario) && compatibility.Contains(scenario),
                "The Sprint 24 review is wired through every guarded admission seam.");
            foreach (string phase in phases)
                Assertions.True(request.Contains("\"" + phase + "\"") &&
                    common.Contains("'" + phase + "'") &&
                    preflight.Contains("'" + phase + "'") &&
                    compatibility.Contains("'" + phase + "'"),
                    "Every layer owns the exact phase: " + phase);
            foreach (string token in new[] {
                "Sprint24RegisteredRoutes", "Sprint24ReviewDeinonychus",
                "Sprint24ReviewTyrannosaurus", "Sprint24VisualLifecycle",
                "Sprint24MovementAndCrowding", "Sprint24SharedControls",
                "Sprint24ContactPresentation", "Sprint24ReleaseHold",
                "RuleAttackWithWeapon", "ChargeAbility",
                "UnitPartSwallowWhole", "CriticalConfirmationRoll",
                "PowerfulBite", "BleedingCritical",
                "CaptureTheropodOwnedResources", "ExerciseExpandedSummoningRakeCadence",
                "ExerciseExpandedSummoningGrappleLifecycle" })
                Assertions.True(runner.Contains(token),
                    "The native observer must retain carrier: " + token);
            Assertions.False(runner.Contains("KMG_AUTOMATION_BASELINE"),
                "The save-free observer never names the protected baseline.");
        }

        internal static void PersistenceScopeOwnsOnlyTheropodsAndNativeStates()
        {
            Assertions.True(ExpandedSummoningRosterPersistencePolicy
                    .TheropodKeys.SequenceEqual(new[] {
                        TheropodRulesPolicy.DeinonychusKey,
                        TheropodRulesPolicy.TyrannosaurusKey }),
                "The targeted save scope owns exactly the two Sprint 24 identities.");
            foreach (string key in ExpandedSummoningRosterPersistencePolicy
                .TheropodKeys)
            {
                SummonVariantSpec selected =
                    ExpandedSummoningRosterPersistencePolicy
                        .SelectCreatureVariant(
                            ExpandedSummoningRosterPersistencePolicy
                                .TheropodsScope, key);
                Assertions.Equal(key, selected.Creature.Key,
                    "The hidden qualification scope selects its registered identity.");
                Assertions.Equal(SummonMultiplicity.One,
                    selected.Multiplicity,
                    "Persistence creates one creature, never a quantity group.");
                Assertions.False(SummonVisibilityCatalog.IsPublished(
                        selected),
                    "Targeted qualification does not publish a hidden root.");
            }
            Assertions.True(ExpandedSummoningRosterPersistencePolicy
                    .ExactSelectedKeys(
                        ExpandedSummoningRosterPersistencePolicy
                            .TheropodsScope,
                        new[] { TheropodRulesPolicy.TyrannosaurusKey,
                            TheropodRulesPolicy.DeinonychusKey }),
                "The two saved identities match independently of load order.");
            foreach (string[] corrupt in new[] {
                new[] { TheropodRulesPolicy.DeinonychusKey },
                new[] { TheropodRulesPolicy.DeinonychusKey,
                    TheropodRulesPolicy.DeinonychusKey },
                new[] { TheropodRulesPolicy.DeinonychusKey, "foreign" } })
                Assertions.False(ExpandedSummoningRosterPersistencePolicy
                        .ExactSelectedKeys(
                            ExpandedSummoningRosterPersistencePolicy
                                .TheropodsScope, corrupt),
                    "Missing, duplicate and foreign save identities fail closed.");
            bool rejected = false;
            try
            {
                ExpandedSummoningRosterPersistencePolicy
                    .SelectCreatureVariant(
                        ExpandedSummoningRosterPersistencePolicy
                            .TheropodsScope,
                        ArmoredDinosaurRulesPolicy.TriceratopsKey);
            }
            catch (InvalidOperationException) { rejected = true; }
            Assertions.True(rejected,
                "The Sprint 24 scope cannot select another sprint's creature.");

            string persistence = File.ReadAllText(
                "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint24Persistence.cs");
            foreach (string token in new[] {
                "UnitPartSprint24StateReceipt", "active-mouth-hold",
                "already-swallowed", "bleeding-critical",
                "ACTIVE_SUMMON_GRAPPLES_RESET_SAFELY_ON_RELOAD",
                "SWALLOW_WHOLE_INTERIOR_AC_HP_UNMODELED",
                "UnitPartSwallowed", "UnitPartSwallowWhole",
                "2d8+11", "2d6", "RuleHealDamage" })
                Assertions.True(persistence.Contains(token),
                    "The targeted persistence observer retains: " + token);
        }

        private static string Sha256(string path)
        {
            using (SHA256 hash = SHA256.Create())
                return string.Concat(hash.ComputeHash(File.ReadAllBytes(path))
                    .Select(value => value.ToString("x2")));
        }

        private static SummonVariantSpec[] NewRoutes(SummonFamily family)
        {
            return ExpandedSummoningCatalog.GenerateVariants(family)
                .Where(value => TheropodRulesPolicy.UsesExactRanks(
                    value.Creature.Key)).ToArray();
        }

        private static void AssertRouteShape(IEnumerable<SummonVariantSpec> source,
            string key, int[] tiers)
        {
            SummonVariantSpec[] routes = source.Where(value =>
                value.Creature.Key == key).OrderBy(value =>
                    value.ParentTier).ToArray();
            Assertions.True(routes.Select(value => value.ParentTier)
                    .SequenceEqual(tiers),
                key + " has the exact parent tiers.");
            SummonMultiplicity[] expected = tiers.Select((value, index) =>
                index == 0 ? SummonMultiplicity.One :
                index == 1 ? SummonMultiplicity.OneD3 :
                    SummonMultiplicity.OneD4PlusOne).ToArray();
            Assertions.True(routes.Select(value => value.Multiplicity)
                    .SequenceEqual(expected),
                key + " has one, 1d3 and then 1d4+1 quantities.");
        }
    }
}
