using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Summoning;
using KingmakerGunslinger.RuntimeTesting;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningSprint25Tests
    {
        /// <summary>
        /// The ledger rows this sprint adds that no other term already counts.
        /// Sprint 25 appends eighteen: ten of its own plus eight caller
        /// adapters. The adapters are NOT included here, because every chain
        /// that uses this constant sits beside
        /// AlternateSummoningTests.AppendedLedgerIdentities, which counts the
        /// adapter projection live from the production roster and therefore
        /// already counts Brachiosaurus's eight. Adding them here too would
        /// double-count them and slide every earlier sprint's window.
        /// </summary>
        internal const int AppendedLedgerIdentities =
            GiantBodyRulesPolicy.AppendedCatalogIdentityCount;

        private static readonly string[] PrivateIdentityTail = {
            GiantBodyRulesPolicy.BrachiosaurusTailSymbol,
            GiantBodyRulesPolicy.BrachiosaurusTailTypeSymbol,
            GiantBodyRulesPolicy.BrachiosaurusUnitTypeSymbol,
            GiantBodyRulesPolicy.BrachiosaurusChassisSymbol,
            GiantBodyRulesPolicy.BrachiosaurusTrampleSymbol,
            GiantBodyRulesPolicy.ElephantTrampleSymbol,
            GiantBodyRulesPolicy.BrachiosaurusOverrunSymbol,
            GiantBodyRulesPolicy.BrachiosaurusGreaterOverrunSymbol
        };

        internal static void RoutesAreExactlyThreeNatureAllyRootsAndNoMonsterRow()
        {
            ExpandedSummoningCatalog.Validate();
            SummonVisibilityCatalog.Validate();
            GiantBodyRulesPolicy.Validate();

            SummonVariantSpec[] monster = NewRoutes(SummonFamily.Monster);
            SummonVariantSpec[] ally = NewRoutes(SummonFamily.NaturesAlly);
            Assertions.Equal(0, monster.Length,
                "Brachiosaurus has no Summon Monster row, so this sprint adds no Monster root.");
            Assertions.Equal(3, ally.Length,
                "Brachiosaurus adds exactly three Nature's Ally roots.");

            int[] tiers = ally.Select(value => value.ParentTier)
                .OrderBy(value => value).ToArray();
            Assertions.True(tiers.SequenceEqual(new[] { 7, 8, 9 }),
                "The three roots are parents VII, VIII and IX.");
            Assertions.True(ally.All(value =>
                value.Creature.Key == GiantBodyRulesPolicy.BrachiosaurusKey),
                "Only the new creature owns a new root.");
            Assertions.True(ally.All(value => value.SourceTier == 7),
                "Every root derives from the printed Nature's Ally VII entry.");
            Assertions.Equal(SummonMultiplicity.One,
                ally.Single(value => value.ParentTier == 7).Multiplicity,
                "Parent VII summons one.");
            Assertions.Equal(SummonMultiplicity.OneD3,
                ally.Single(value => value.ParentTier == 8).Multiplicity,
                "Parent VIII summons 1d3.");
            Assertions.Equal(SummonMultiplicity.OneD4PlusOne,
                ally.Single(value => value.ParentTier == 9).Multiplicity,
                "Parent IX summons 1d4+1.");
            Assertions.False(ally.Any(value => value.Creature.MonsterTemplated),
                "An untemplated creature owns no celestial or fiendish execution child.");
        }

        internal static void ElephantIsCompletedAndNeverReRegisteredOrSuppressed()
        {
            SummonCreatureSpec elephant = ExpandedSummoningCatalog.All.Single(
                value => value.Key == GiantBodyRulesPolicy.ElephantKey);
            Assertions.Equal(6, elephant.MonsterTier ?? 0,
                "Elephant keeps its released Summon Monster VI tier.");
            Assertions.Equal(6, elephant.NaturesAllyTier ?? 0,
                "Elephant keeps its released Summon Nature's Ally VI tier.");
            Assertions.True(elephant.MonsterTemplated,
                "Elephant keeps its released celestial and fiendish children.");

            SummonVariantSpec[] roots = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster)
                .Concat(ExpandedSummoningCatalog.GenerateVariants(
                    SummonFamily.NaturesAlly))
                .Where(value => value.Creature.Key ==
                    GiantBodyRulesPolicy.ElephantKey)
                .ToArray();
            Assertions.Equal(GiantBodyRulesPolicy.ElephantExistingRoots,
                roots.Length,
                "Elephant's eight existing roots are completed, not re-registered.");
            Assertions.Equal(GiantBodyRulesPolicy.ElephantExistingMonsterRoots,
                roots.Count(value => value.Family == SummonFamily.Monster),
                "Four existing Monster roots.");
            Assertions.Equal(GiantBodyRulesPolicy.ElephantExistingNatureAllyRoots,
                roots.Count(value => value.Family == SummonFamily.NaturesAlly),
                "Four existing Nature's Ally roots.");
            Assertions.True(roots.All(SummonVisibilityCatalog.IsPublished),
                "A released Elephant route is never suppressed while a development package exists.");

            // The actual Mastodon stays a separate published creature.
            SummonCreatureSpec mastodon = ExpandedSummoningCatalog.All.Single(
                value => value.Key == "mastodon");
            Assertions.Equal(7, mastodon.MonsterTier ?? 0,
                "The real Mastodon is untouched at Summon Monster VII.");
            Assertions.True(ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster)
                .Where(value => value.Creature.Key == "mastodon")
                .All(SummonVisibilityCatalog.IsPublished),
                "No Mastodon route is suppressed by this sprint.");
        }

        internal static void OnlyTheNewCreatureIsWithheld()
        {
            SummonVariantSpec[] all = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster)
                .Concat(ExpandedSummoningCatalog.GenerateVariants(
                    SummonFamily.NaturesAlly)).ToArray();
            SummonVariantSpec[] withheld = all.Where(value =>
                !SummonVisibilityCatalog.IsPublished(value)).ToArray();

            Assertions.Equal(GiantBodyRulesPolicy.HiddenLogicalPlacements,
                withheld.Length,
                "Exactly the three Brachiosaurus roots are withheld.");
            Assertions.True(withheld.All(value =>
                GiantBodyRulesPolicy.IsWithheldThisSprint(value.Creature.Key)),
                "Nothing but the new creature is withheld.");
            Assertions.Equal(
                GiantBodyRulesPolicy.RegisteredLogicalPlacements,
                SummonVisibilityCatalog.RegisteredLogicalPlacementCount,
                "Registration allocated every placement once.");
            Assertions.Equal(
                GiantBodyRulesPolicy.HiddenPublishedLogicalPlacements,
                SummonVisibilityCatalog.PublishedLogicalPlacementCount,
                "The published surface retains every pre-Sprint-25 placement, which is "
                + "this test's real subject: a later sprint must not reach back and "
                + "suppress a published creature.");
            Assertions.Equal(
                TheropodRulesPolicy.FinalPublishedLogicalPlacements,
                SummonVisibilityCatalog.PublishedLogicalPlacementCount,
                "The withheld candidate leaves the released v0.0.155 surface exactly where it was.");

            foreach (SummonVariantSpec variant in all.Where(value =>
                !GiantBodyRulesPolicy.IsWithheldThisSprint(value.Creature.Key)))
                Assertions.True(SummonVisibilityCatalog.IsPublished(variant),
                    "Every pre-Sprint-25 placement is published, this creature's included: "
                    + variant.StableKey);
        }

        internal static void FrozenAdultArithmeticIsDerivedNotStated()
        {
            GiantBodyRulesPolicy.Validate();

            // The recorded source discrepancy and its resolution.
            Assertions.Equal(23,
                GiantBodyRulesPolicy.BrachiosaurusSelectedTailAttack,
                "The selected tail bonus is the legacy +23.");
            Assertions.Equal(22,
                GiantBodyRulesPolicy.BrachiosaurusPrintedTailAttackCurrentAon,
                "The rejected current-AoN reading is recorded, not discarded.");
            Assertions.Equal(
                GiantBodyRulesPolicy.BrachiosaurusSelectedTailAttack,
                GiantBodyRulesPolicy.AttackBonus(
                    GiantBodyRulesPolicy.BrachiosaurusBaseAttackBonus,
                    GiantBodyRulesPolicy.AbilityModifier(
                        GiantBodyRulesPolicy.BrachiosaurusStrength),
                    GiantBodyRulesPolicy.BrachiosaurusSizeAttackModifier,
                    GiantBodyRulesPolicy.BrachiosaurusWeaponFocusTailBonus),
                "The selected bonus is what the shared printed inputs derive.");

            // The lone tail takes one and a half Strength; Elephant's two
            // primaries take it once each.
            Assertions.Equal(19,
                GiantBodyRulesPolicy.LoneNaturalWeaponStrengthContribution(13),
                "Strength 37 gives the printed 4d6+19 on a single natural weapon.");
            Assertions.Equal(10,
                GiantBodyRulesPolicy
                    .MultipleNaturalPrimaryStrengthContribution(10),
                "Strength 30 gives the printed 2d8+10 and 2d6+10 on two primaries.");
            Assertions.True(GiantBodyRulesPolicy.UsesLoneNaturalWeaponStrength(
                    GiantBodyRulesPolicy.BrachiosaurusKey),
                "Only the one-weapon creature uses the one-and-a-half routine.");
            Assertions.False(GiantBodyRulesPolicy.UsesLoneNaturalWeaponStrength(
                    GiantBodyRulesPolicy.ElephantKey),
                "The two-weapon creature does not.");

            // Zero and penalties are never multiplied.
            foreach (int modifier in new[] { 0, -1, -3, -5 })
                Assertions.Equal(modifier,
                    GiantBodyRulesPolicy
                        .LoneNaturalWeaponStrengthContribution(modifier),
                    "A zero or penalty Strength modifier stays exact: " + modifier);

            // Odd and even positive Strength both round the native way.
            Assertions.Equal(13,
                GiantBodyRulesPolicy.LoneNaturalWeaponStrengthContribution(9),
                "An odd positive modifier rounds down.");
            Assertions.Equal(15,
                GiantBodyRulesPolicy.LoneNaturalWeaponStrengthContribution(10),
                "An even positive modifier is exact.");

            // Trample DCs derive from live hit dice and Strength.
            Assertions.Equal(32,
                GiantBodyRulesPolicy.TrampleSaveDifficulty(
                    GiantBodyRulesPolicy.BrachiosaurusHitDice, 13),
                "Brachiosaurus trample is DC 32 derived, not stated.");
            Assertions.Equal(25,
                GiantBodyRulesPolicy.TrampleSaveDifficulty(
                    GiantBodyRulesPolicy.ElephantHitDice, 10),
                "Elephant trample is DC 25 derived, not stated.");

            // Space and reach, including the one that must not generalise.
            Assertions.Equal(15, GiantBodyRulesPolicy.ElephantSpaceFeet,
                "Elephant occupies fifteen feet.");
            Assertions.Equal(10, GiantBodyRulesPolicy.ElephantReachFeet,
                "Elephant reaches ten feet, which a Huge body does not imply.");
            Assertions.Equal(GiantBodyRulesPolicy.BrachiosaurusSpaceFeet,
                GiantBodyRulesPolicy.BrachiosaurusReachFeet,
                "Brachiosaurus is the creature whose reach equals its space.");
        }

        /// <summary>
        /// The signed-Strength trample contract, read through the same
        /// UngulateRulesProfile the damage path uses at
        /// ContextActionUngulateTrample line 122. Two formulas disagreed
        /// here: the record helper applied one and a half times a Strength
        /// BONUS, while the live branch multiplied unconditionally, so at
        /// a live modifier of -4 the record said -4 and the damage path
        /// said -6. The positive printed baselines agreed, which is why it
        /// survived. This table covers the cases the baselines do not.
        /// </summary>
        internal static void TrampleStrengthArithmeticIsOneRule()
        {
            foreach (string key in new[] {
                GiantBodyRulesPolicy.BrachiosaurusKey,
                GiantBodyRulesPolicy.ElephantKey })
            {
                UngulateRulesProfile rules = UngulateRulesPolicy.For(key);
                Assertions.True(rules.LiveTrampleMath,
                    key + " derives its trample live, which is the branch "
                        + "the disagreement was in.");
                // modifier -> expected contribution. A penalty is not a
                // bonus, so it is never multiplied.
                foreach (int[] row in new[] {
                    new[] { 13, 19 }, new[] { 10, 15 }, new[] { 8, 12 },
                    new[] { 7, 10 }, new[] { 5, 7 }, new[] { 1, 1 },
                    new[] { 0, 0 }, new[] { -1, -1 }, new[] { -2, -2 },
                    new[] { -3, -3 }, new[] { -4, -4 }, new[] { -5, -5 } })
                {
                    Assertions.Equal(row[1],
                        rules.TrampleBonusFor(row[0]),
                        key + " trample Strength contribution at modifier "
                            + row[0]);
                    // The record helper and the damage path are now one
                    // implementation; this proves they cannot drift.
                    Assertions.Equal(rules.TrampleBonusFor(row[0]),
                        GiantBodyRulesPolicy.TrampleStrengthContribution(
                            row[0]),
                        "record helper and damage path agree at " + row[0]);
                }
            }

            // The live save DC for each creature, from live hit dice and
            // live Strength, with no stampede adjacency.
            UngulateRulesProfile brach = UngulateRulesPolicy.For(
                GiantBodyRulesPolicy.BrachiosaurusKey);
            Assertions.Equal(32, brach.TrampleSaveDc(0,
                    GiantBodyRulesPolicy.BrachiosaurusHitDice, 13),
                "Brachiosaurus printed DC 32 derives from 18 HD and +13.");
            Assertions.Equal(28, brach.TrampleSaveDc(0,
                    GiantBodyRulesPolicy.BrachiosaurusHitDice, 9),
                "A debuffed Brachiosaurus DC falls with live Strength.");
            Assertions.Equal(15, brach.TrampleSaveDc(0,
                    GiantBodyRulesPolicy.BrachiosaurusHitDice, -4),
                "A negative live modifier lowers the DC by its face value.");

            UngulateRulesProfile eleph = UngulateRulesPolicy.For(
                GiantBodyRulesPolicy.ElephantKey);
            Assertions.Equal(25, eleph.TrampleSaveDc(0,
                    GiantBodyRulesPolicy.ElephantHitDice, 10),
                "Elephant printed DC 25 derives from 11 HD and +10.");
            Assertions.Equal(11, eleph.TrampleSaveDc(0,
                    GiantBodyRulesPolicy.ElephantHitDice, -4),
                "Elephant DC at a negative live modifier.");

            // The two creatures stay distinct, and neither gains the
            // other's dice.
            Assertions.Equal(2, brach.TrampleDiceCount, "Brachiosaurus 2d6.");
            Assertions.Equal(6, brach.TrampleDieSides, "Brachiosaurus d6.");
            Assertions.Equal(2, eleph.TrampleDiceCount, "Elephant 2d8.");
            Assertions.Equal(8, eleph.TrampleDieSides, "Elephant d8.");

            // The released frozen-math tramplers are the changed
            // dependency's control: they never take the live branch, so
            // their behaviour is unchanged at any live modifier.
            foreach (string released in new[] { "aurochs", "bison",
                "woolly-rhinoceros", "giant-stag-beetle" })
            {
                UngulateRulesProfile frozen = UngulateRulesPolicy.For(
                    released);
                Assertions.False(frozen.LiveTrampleMath,
                    released + " keeps its frozen released arithmetic.");
                Assertions.Equal(frozen.TrampleBonus,
                    frozen.TrampleBonusFor(-4),
                    released + " ignores a live modifier entirely.");
            }

            // Triceratops IS on the live branch, so it is the released
            // creature this repair actually touches. Its printed positive
            // values are unchanged; only the penalty case moves, and it
            // moves to the rule the record always stated.
            UngulateRulesProfile tri = UngulateRulesPolicy.For(
                "triceratops");
            Assertions.True(tri.LiveTrampleMath,
                "Triceratops derives live, so it shares this repair.");
            Assertions.Equal(12, tri.TrampleBonusFor(8),
                "Triceratops positive contribution is unchanged.");
            Assertions.Equal(15, tri.TrampleBonusFor(10),
                "Triceratops positive contribution is unchanged.");
            Assertions.Equal(-4, tri.TrampleBonusFor(-4),
                "Triceratops no longer doubles a Strength penalty.");
        }
        internal static void ShippedBodiesAreFoundAndAccepted()
        {
            // What this can prove without the game: the asset exists, the
            // loader's contract matches the payload, policy and asset cannot
            // drift, the albedo named is the albedo on disk, and the files are
            // in the build and package lists. Whether the body renders is
            // native qualification and is not claimed here.
            string[] keys = {
                GiantBodyRulesPolicy.BrachiosaurusKey,
                GiantBodyRulesPolicy.ElephantKey };
            foreach (string key in keys)
            {
                string path = Path.Combine(Environment.CurrentDirectory,
                    "assets", "sprint25-giant-bodies", key + "-mesh.json");
                Assertions.True(File.Exists(path),
                    key + " ships a mesh payload.");
                JObject payload = JObject.Parse(File.ReadAllText(path));

                Assertions.Equal(key, (string)payload["creature"],
                    "The payload names its own creature.");
                Assertions.Equal(GiantBodyVisualPolicy.DonorGuid,
                    (string)payload["nativeDonor"],
                    "The payload names the measured donor.");
                Assertions.Equal(GiantBodyVisualPolicy.DonorPrefabGuid,
                    (string)payload["nativePrefab"],
                    "The payload names the measured prefab.");
                Assertions.Equal(GiantBodyVisualPolicy.BodyRendererName,
                    (string)payload["bodyRenderer"],
                    "The payload names the donor's one renderer.");
                Assertions.Equal(1, (int)payload["nativeRendererCount"],
                    "That donor exposes exactly one skinned renderer.");
                Assertions.Equal(4, (int)payload["visibleLegs"],
                    "Both creatures are quadrupeds.");
                Assertions.Equal(7, (int)payload["forwardChainJoints"],
                    "The donor's forward chain is seven joints.");
                Assertions.Equal(5, (int)payload["rearTailJoints"],
                    "The donor's rear tail is five joints.");

                // Policy and asset are one contract. If a re-export changed
                // the weighting, the bone list would move and this would fail
                // rather than the game silently refusing the body.
                string[] shipped = payload["bones"].Values<string>().ToArray();
                Assertions.True(GiantBodyVisualPolicy.IsPermitted(key,
                    shipped), key + " bones match its ownership policy.");

                // The albedo the payload names is the albedo on disk.
                string albedoName = (string)payload["albedo"]["file"];
                string albedoPath = Path.Combine(Environment.CurrentDirectory,
                    "assets", "sprint25-giant-bodies", albedoName);
                Assertions.True(File.Exists(albedoPath),
                    key + " ships the albedo its payload names.");
                string actual;
                using (var sha = System.Security.Cryptography.SHA256.Create())
                    actual = BitConverter.ToString(sha.ComputeHash(
                        File.ReadAllBytes(albedoPath)))
                        .Replace("-", string.Empty).ToLowerInvariant();
                Assertions.Equal((string)payload["albedo"]["sha256"], actual,
                    key + " albedo bytes are the ones the payload pins.");

                Assertions.True((int)payload["vertexCount"] > 0 &&
                    (int)payload["triangleCount"] > 0,
                    key + " carries real geometry.");
            }

            // The interim design, declared rather than implied: the owner
            // authorized a stiff Head-weighted Brachiosaurus neck for this
            // release, and the loader refuses a payload claiming otherwise.
            JObject brachiosaurus = JObject.Parse(File.ReadAllText(
                Path.Combine(Environment.CurrentDirectory, "assets",
                    "sprint25-giant-bodies", "brachiosaurus-mesh.json")));
            Assertions.Equal("Head", (string)brachiosaurus["neckDriver"],
                "The interim neck is driven by Head alone.");
            Assertions.True((bool)brachiosaurus["interimStiffNeck"],
                "The payload declares the interim stiff neck.");
            JObject elephant = JObject.Parse(File.ReadAllText(
                Path.Combine(Environment.CurrentDirectory, "assets",
                    "sprint25-giant-bodies", "elephant-mesh.json")));
            Assertions.Equal("chain", (string)elephant["neckDriver"],
                "The Elephant trunk keeps the measured native chain.");
            Assertions.Equal(2, (int)elephant["tusks"],
                "The Elephant ships tusks.");

            // A body nobody can reach is not a body: the view patch must map
            // both blueprints, and the build and package must carry the files.
            string view = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Summoning", "ExpandedSummoningPteranodonViewPatch.cs"));
            Assertions.True(view.Contains(
                "GiantBodyVisualPolicy.BrachiosaurusBlueprintName") &&
                view.Contains("GiantBodyVisualPolicy.ElephantBlueprintName"),
                "Both blueprints map to their visual key.");
            Assertions.True(view.Contains(
                "PteranodonAssetRuntime.TryGetGiantBodyVisual"),
                "The attach path selects the Sprint 25 source.");
            foreach (string script in new[] { "Build-Local.ps1", "package.ps1",
                "validate-package.ps1", "validate-build-output.ps1" })
            {
                string text = File.ReadAllText(Path.Combine(
                    Environment.CurrentDirectory, "scripts", script));
                Assertions.True(text.Contains("sprint25-giant-bodies"),
                    script + " carries the Sprint 25 body assets.");
            }
        }

        internal static void OverrunKnocksDownOnlyThroughItsOwnCommand()
        {
            // The printed rule knocks a target prone only when the
            // overrun result beats its CMD by 5 or more, and Greater
            // Overrun's rider follows THAT knockdown. The engine already
            // draws the line: RuleCombatManeuver.SuccesValue is
            // InitiatorCMValue - TargetCMD and IsPartialSuccess is
            // Success && Type == Overrun && SuccesValue < 5, against its
            // own OverrunPartialSuccessThreshold of 5. So the correction
            // reads that answer; it does not compute a second one.
            string component = ComponentSource();
            Assertions.True(
                component.Contains("evt.Type != CombatManeuver.Overrun"),
                "Only an overrun maneuver can knock a target prone here.");
            Assertions.True(
                component.Contains("!evt.Success || evt.IsPartialSuccess"),
                "Ordinary success below the margin is a pass-through: " +
                "the engine's own partial-success test gates this.");
            // Comments stripped: the code's own commentary names the very
            // native members it must not recompute.
            string code = StripComments(component);
            Assertions.False(code.Contains("TargetCMD") ||
                code.Contains("SuccesValue") ||
                code.Contains("RulebookEvent.Dice"),
                "No second margin calculation and no extra roll.");
            Assertions.True(component.Contains("CanBeKnockedOff"),
                "Immunity stays the engine's answer.");
            Assertions.True(
                component.Contains("Prone.Active") &&
                component.Contains("Prone.ShouldBeActive"),
                "An already-prone target is not knocked down again.");
            // One CALL SITE, which is what source can show. That it
            // raises exactly one provocation event per knockdown, and that
            // each eligible responder answers at most once, is behaviour and
            // is owed to the native campaign.
            Assertions.Equal(1, CountOccurrences(component,
                "ProvokeAttackOfOpportunity("),
                "One provocation call site in the corrected component.");

            // The ability's action list stays empty, because that list
            // runs on ANY success and cannot tell the two apart.
            string builder = BuilderSource();
            string command = Section(builder,
                "private static void ConfigureBrachiosaurusOverrun(");
            Assertions.True(command.Contains("overrun.AutoSuccess = false"),
                "The command rolls a real maneuver.");

            // The printed ordinary maneuver is "you can attempt to overrun
            // your target, moving through its square" - ONE target. The
            // delivery's IL keeps enumerating units and overrunning all of
            // them while FirstTargetOnly is false, and stops after the first
            // when it is true, continuing the movement either way.
            Assertions.True(command.Contains("overrun.FirstTargetOnly = true"),
                "The ordinary Overrun takes one target, not everything in " +
                "the path.");
            Assertions.False(StripComments(command).Contains(
                "ContextActionKnockdownTarget"),
                "The knockdown is not in a list that runs on any success.");

            // The native feat is cloned, not granted: its own provocation
            // fires on success alone. Exactly one provocation is in play.
            string carrier = Section(builder,
                "private static void ConfigureBrachiosaurusGreaterOverrun(");
            Assertions.True(carrier.Contains(
                "InstalledGreaterOverrunFeatureGuid"),
                "The carrier clones the installed-stack donor when present.");
            Assertions.True(carrier.Contains("TryGetValue(") &&
                carrier.Contains("if (native == null)") &&
                carrier.Contains("CreateInstance<ManeuverBonus>()") &&
                carrier.Contains("bonus.Type = CombatManeuver.Overrun") &&
                carrier.Contains("bonus.Bonus = 2"),
                "Standalone uses the native +2 Overrun component, not a required optional donor.");
            Assertions.True(carrier.Contains(
                "if (component is ManeuverProvokeAttack) continue"),
                "The success-only native provocation is dropped.");
            Assertions.True(carrier.Contains(
                "BrachiosaurusOverrunKnockdown"),
                "The corrected behaviour replaces it.");
            Assertions.True(carrier.Contains("DeepCloneComponent"),
                "The carrier owns its components; the native feat every " +
                "other actor has keeps its own instances.");
            Assertions.False(StripComments(carrier).Contains("native." +
                "ComponentsArray ="),
                "The native blueprint is read, never written.");

            // Trample is a different action and keeps its own shape.
            string trample = Section(builder,
                "private static void ConfigureUngulateTrample(");
            Assertions.True(trample.Contains("overrun.AutoSuccess = true"),
                "Trample stays an auto-success and never rolls.");
            // Trample IS the many-target one, and keeps that form.
            Assertions.True(trample.Contains("overrun.FirstTargetOnly = false"),
                "Trample still overruns everything in its path.");
            Assertions.False(StripComments(trample).Contains(
                "ContextActionKnockdownTarget"),
                "Trample gains no knockdown.");
            Assertions.False(StripComments(trample).Contains(
                "GreaterOverrun"),
                "Trample gains no Greater Overrun behaviour.");

            // Only this creature resolves the fact to the clone.
            string natural = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src",
                "KingmakerGunslinger", "Blueprints",
                "ExpandedSummoningNaturalBuilder.cs"));
            Assertions.True(natural.Contains(
                "profile.Key == GiantBodyRulesPolicy.BrachiosaurusKey"),
                "The substitution is keyed on the creature as well as " +
                "the fact, so no other profile can pick it up.");
        }

        internal static void NativeEvidenceRetainsArrayAndNestedRuleRows()
        {
            string source = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.ExpandedSummoningSprint25Review.cs"));
            Assertions.Equal(2, CountOccurrences(source,
                "_sprint25Rows.ToString(Formatting.Indented)"),
                "Progress and final evidence write the actual JArray, bypassing game serializer defaults.");
            var rows = new JArray(new JObject { ["predicate"] = "native-rule",
                ["passed"] = false, ["damage"] = new JArray(new JObject {
                    ["unit"] = "exact-owner", ["bonus"] = -3 }) });
            JArray parsed = JArray.Parse(rows.ToString(Newtonsoft.Json.Formatting.Indented));
            Assertions.Equal(-3, (int)parsed[0]["damage"][0]["bonus"],
                "Nested signed native observations remain machine-readable.");
            Assertions.False((bool)parsed[0]["passed"], "A failed predicate is not erased.");
        }

        private static string Section(string source, string signature)
        {
            int start = source.IndexOf(signature, StringComparison.Ordinal);
            Assertions.True(start > 0, signature + " exists once.");
            int end = source.IndexOf("private static ", start +
                signature.Length, StringComparison.Ordinal);
            return end > start ? source.Substring(start, end - start)
                : source.Substring(start);
        }

        private static string ComponentSource()
        {
            return File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src",
                "KingmakerGunslinger", "Summoning",
                "BrachiosaurusOverrunKnockdown.cs"));
        }
        // A code point, not an escape: this file is edited by scripts,
        // and an escape that survives one layer of quoting and not the
        // next lands a real newline inside a character constant.
        private const char Newline = (char)10;

        private static string StripComments(string text)
        {
            var kept = new List<string>();
            foreach (string line in text.Split(Newline))
            {
                string trimmed = line.TrimStart();
                if (trimmed.StartsWith("//", StringComparison.Ordinal) ||
                    trimmed.StartsWith("///", StringComparison.Ordinal))
                    continue;
                kept.Add(line);
            }
            return string.Join(Newline.ToString(), kept.ToArray());
        }

        private static int CountOccurrences(string text, string token)
        {
            int count = 0;
            int index = text.IndexOf(token, StringComparison.Ordinal);
            while (index >= 0)
            {
                count++;
                index = text.IndexOf(token, index + token.Length,
                    StringComparison.Ordinal);
            }
            return count;
        }

        private static string BuilderSource()
        {
            return File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Blueprints",
                "ExpandedSummoningSpecialBuilder.cs"));
        }

        internal static void IdentityAppendOwnsExactlyFivePrivateCarriers()
        {
            // Eight since 2026-10-16: the five Brachiosaurus carriers, the
            // released Elephant trample that the bounded observation of that
            // creature justified, the Brachiosaurus ordinary Overrun command
            // from the owner-selected bounded native-command route, and the
            // cloned Greater Overrun carrier that corrects the printed rider
            // for that one creature.
            Assertions.Equal(8, PrivateIdentityTail.Length,
                "Sprint 25 appends eight private carriers.");
            Assertions.Equal(GiantBodyRulesPolicy.AppendedPrivateIdentityCount,
                PrivateIdentityTail.Length,
                "The policy's private count is the list's length.");
            Assertions.Equal(PrivateIdentityTail.Length,
                PrivateIdentityTail.Distinct(StringComparer.Ordinal).Count(),
                "No private carrier is allocated twice.");

            IReadOnlyList<SummoningIdentitySpec> identities =
                ExpandedSummoningIdentityCatalog.Build();
            string[] tail = identities.Skip(identities.Count -
                PrivateIdentityTail.Length).Select(value => value.Symbol)
                .ToArray();
            Assertions.True(tail.SequenceEqual(PrivateIdentityTail,
                    StringComparer.Ordinal),
                "The eight private carriers append last, in frozen order.");

            Assertions.Equal(GiantBodyRulesPolicy.RegisteredCreatureCount,
                ExpandedSummoningIdentityCatalog.UnitCount,
                "One unit identity joins for the one new creature.");
            Assertions.Equal(
                GiantBodyRulesPolicy.RegisteredLogicalPlacements,
                ExpandedSummoningIdentityCatalog.LogicalAbilityCount,
                "Three logical ability identities join.");
            Assertions.Equal(
                GiantBodyRulesPolicy.RegisteredTemplatedPlacements,
                ExpandedSummoningIdentityCatalog.TemplatedPlacementCount,
                "No templated placement joins, because the new creature has no Monster row.");

            // Every appended symbol must resolve to exactly one manifest row
            // of the planned type, so an identity cannot be named here and
            // missing, or point at something else.
            var entries = (JArray)JObject.Parse(File.ReadAllText(
                Path.Combine(Environment.CurrentDirectory, "blueprints",
                    "blueprints.json")))["entries"];
            foreach (string symbol in PrivateIdentityTail.Concat(new[] {
                    "KMG.Summoning.Unit.Brachiosaurus",
                    "KMG.Summoning.Ability.SNA.Tier7.Brachiosaurus.One",
                    "KMG.Summoning.Ability.SNA.Tier8.Brachiosaurus.OneD3",
                    "KMG.Summoning.Ability.SNA.Tier9.Brachiosaurus.OneD4PlusOne" }))
                Assertions.Equal(1, entries.Count(entry =>
                        (string)entry["symbol"] == symbol &&
                        (string)entry["status"] == "active"),
                    "Exactly one active manifest row: " + symbol);

            Assertions.Equal(0, entries.Count(entry =>
                    ((string)entry["symbol"]).Contains("Brachiosaurus") &&
                    (((string)entry["symbol"]).EndsWith(".Celestial") ||
                     ((string)entry["symbol"]).EndsWith(".Fiendish"))),
                "No Brachiosaurus execution child exists in the ledger.");
            Assertions.Equal(0, entries.Count(entry =>
                    ((string)entry["symbol"]).Contains("Brachiosaurus") &&
                    ((string)entry["symbol"]).Contains(".Ability.SM.")),
                "No Brachiosaurus Summon Monster ability exists in the ledger.");
        }

        internal static void ElephantProfileIsCorrectedToItsPrintedRoutine()
        {
            NaturalSummonProfile elephant = ExpandedSummoningNaturalProfiles.For(
                GiantBodyRulesPolicy.ElephantKey);

            // The profile row has landed, together with the builder that
            // resolves its weapon key. This replaces the tripwire that held
            // its place: the row was absent on purpose, because a profile
            // whose weapon cannot resolve makes blueprint initialization
            // throw and roll back every registration, so the row and its
            // carriers had to be one change. The contract asserted now is
            // the positive one.
            NaturalSummonProfile brachiosaurus =
                ExpandedSummoningNaturalProfiles.For(
                    GiantBodyRulesPolicy.BrachiosaurusKey);
            Assertions.Equal("BrachiosaurusTail4d6",
                brachiosaurus.PrimaryWeapon,
                "The frozen primary weapon key is the one the builder resolves.");
            Assertions.Equal(GiantBodyRulesPolicy.BrachiosaurusHitDice,
                brachiosaurus.HitDice, "Eighteen racial hit dice.");
            Assertions.Equal(GiantBodyRulesPolicy.BrachiosaurusStrength,
                brachiosaurus.Strength, "Printed Strength.");
            Assertions.Equal("Gargantuan", brachiosaurus.Size,
                "The frozen adult size.");
            Assertions.Equal(0, brachiosaurus.AdditionalWeapons.Count,
                "A single natural attack, which is what the lone-weapon "
                    + "Strength routine depends on.");
            Assertions.Equal(0,
                brachiosaurus.AdditionalSecondaryWeapons.Count,
                "Nothing on this creature is a secondary natural attack.");
            Assertions.True(brachiosaurus.Facts.Contains(
                    "BrachiosaurusChassis"),
                "The chassis carrying the final five feet of reach is carried.");
            Assertions.True(brachiosaurus.Facts.Contains("WeaponFocusTail"),
                "Weapon Focus (tail) is carried; the selected +23 depends on it.");
            // Measured present in the installed library by the 2026-10-16
            // census - GreatCleaveFeature cc9c862e... and
            // GreaterOverrunFeature ee24c02c... - so the omission is a
            // recorded owner decision rather than a missing engine
            // capability. Neither may be carried without that decision.
            // Greater Overrun is carried through a private CLONE of the
            // real native feat - the native component provokes on success
            // alone - and the creature owns an ordinary Overrun command so
            // the feat is reachable: a trample sets AutoSuccess true and
            // raises no maneuver, that command sets it false. The fact key
            // is unchanged; what changed is what it resolves to.
            Assertions.True(brachiosaurus.Facts.Contains("GreaterOverrun"),
                "The printed Greater Overrun is carried natively.");
            // Great Cleave stays uncarried on purpose: the shipped
            // CleaveAction points its GreaterFeature field at CleaveFeature,
            // so the feat itself is read by nothing on this build.
            Assertions.False(brachiosaurus.Facts.Contains("GreatCleave"),
                "An inert display-only fact is not granted.");
            Assertions.True(brachiosaurus.Deviations.Any(value =>
                    value.Contains("Registered is not qualified")),
                "The trample is registered and still refuses to claim "
                    + "native qualification, which is a separate thing.");
            Assertions.True(UngulateRulesPolicy.All.Any(value =>
                    value.Key == GiantBodyRulesPolicy.BrachiosaurusKey &&
                    value.TrampleDiceCount == GiantBodyRulesPolicy
                        .BrachiosaurusTrampleDiceCount &&
                    value.TrampleDieSides == GiantBodyRulesPolicy
                        .BrachiosaurusTrampleDieSize &&
                    value.TrampleBonus == 19 && value.TrampleDc == 32 &&
                    !value.HasPowerfulCharge && !value.Stampede),
                "The trample derives 2d6+19 and DC 32 from the frozen "
                    + "Strength and hit dice, and grants nothing else.");

            // Elephant: the Sprint 25 correction. Both printed attacks are
            // primary, so neither takes the secondary -5 and half Strength.
            Assertions.Equal("Gore2d8", elephant.PrimaryWeapon,
                "The gore stays the primary weapon.");
            Assertions.True(elephant.AdditionalWeapons.Contains("Slam2d6"),
                "The slam is a primary attack, as printed.");
            Assertions.Equal(0, elephant.AdditionalSecondaryWeapons.Count,
                "Nothing on this creature is a secondary natural attack.");
            Assertions.Equal(GiantBodyRulesPolicy.ElephantHitDice,
                elephant.HitDice, "Eleven racial hit dice.");
            Assertions.Equal(GiantBodyRulesPolicy.ElephantStrength,
                elephant.Strength, "Printed Strength.");
            Assertions.Equal(GiantBodyRulesPolicy.ElephantSpeedFeet,
                elephant.SpeedFeet, "Forty-foot land speed.");
            Assertions.True(elephant.Facts.Contains("TripDefenseFourLegs"),
                "The printed quadruped defence against trip is carried.");
            Assertions.True(elephant.Facts.Contains("ImprovedBullRush"),
                "Improved Bull Rush is carried now that its carrier is proven.");
            foreach (string absent in new[] { "PowerfulCharge", "Grab",
                "Pounce" })
                Assertions.False(elephant.Facts.Contains(absent),
                    "No invented or unproven Elephant carrier: " + absent);
            // All six printed Elephant feats are carried. Endurance was
            // wrongly recorded as unproven: the builder has mapped it to a
            // native feat since Sprint 4, the Sprint 24 bounded native census
            // recorded it as resolved, and the released Tyrannosaurus ships
            // it. Every printed feat now resolves through a real carrier.
            foreach (string printed in new[] { "Endurance", "GreatFortitude",
                "ImprovedBullRush", "IronWill", "PowerAttack",
                "SkillFocusPerception" })
                Assertions.True(elephant.Facts.Contains(printed),
                    "Printed Elephant feat carried: " + printed);
            // The trample is registered now, and the record still refuses
            // to call registration qualification.
            Assertions.True(elephant.Deviations.Any(value =>
                    value.Contains("Registered is not qualified")),
                "Trample registration is recorded without claiming native "
                    + "qualification, which is a separate thing.");
            Assertions.True(UngulateRulesPolicy.All.Any(value =>
                    value.Key == GiantBodyRulesPolicy.ElephantKey &&
                    value.TrampleDiceCount == GiantBodyRulesPolicy
                        .ElephantTrampleDiceCount &&
                    value.TrampleDieSides == GiantBodyRulesPolicy
                        .ElephantTrampleDieSize &&
                    value.TrampleBonus == 15 && value.TrampleDc == 25 &&
                    !value.HasPowerfulCharge && !value.Stampede),
                "The Elephant trample derives 2d8+15 and DC 25 from its "
                    + "frozen Strength and hit dice, distinct from the "
                    + "Brachiosaurus 2d6+19 and DC 32, and grants nothing else.");

            // The real Mastodon keeps its own distinct profile.
            NaturalSummonProfile mastodon =
                ExpandedSummoningNaturalProfiles.For("mastodon");
            Assertions.Equal(14, mastodon.HitDice,
                "The Mastodon is untouched by this sprint.");
            Assertions.Equal(34, mastodon.Strength,
                "The Mastodon keeps its own Strength.");
        }

        internal static void TheDonorIsTheMeasuredQuadrupedRigAndNotAnIdentity()
        {
            SummonDonorSpec donor = ExpandedSummoningDonorCatalog.For(
                GiantBodyRulesPolicy.BrachiosaurusKey);
            SummonDonorSpec triceratops =
                ExpandedSummoningDonorCatalog.For("triceratops");
            Assertions.Equal(triceratops.Guid, donor.Guid,
                "Sprint 25 reuses the quadruped rig Sprint 23 already measured.");
            Assertions.False(donor.DedicatedSummon,
                "The rig is borrowed for animation only; the body is project-owned.");
            Assertions.Equal(
                ExpandedSummoningDonorCatalog.For(
                    GiantBodyRulesPolicy.ElephantKey).Guid,
                donor.Guid,
                "It is the same rig the released Elephant already rides.");
            ExpandedSummoningDonorCatalog.Validate();
        }

        internal static void TargetedPersistenceCannotExpandItsRoster()
        {
            const string scope = ExpandedSummoningRosterPersistencePolicy.GiantBodiesScope;
            foreach (string key in new[] { "brachiosaurus", "elephant" })
            {
                SummonVariantSpec variant = ExpandedSummoningRosterPersistencePolicy.SelectCreatureVariant(scope, key);
                Assertions.Equal(key, variant.Creature.Key, "Targeted fixture selects the exact registered identity.");
                Assertions.Equal(SummonMultiplicity.One, variant.Multiplicity, "Targeted fixture creates exactly one creature.");
            }
            Assertions.True(ExpandedSummoningRosterPersistencePolicy.ExactSelectedKeys(scope, new[] { "elephant", "brachiosaurus" }), "Load order does not affect exact two-creature membership.");
            Assertions.False(ExpandedSummoningRosterPersistencePolicy.ExactSelectedKeys(scope, new[] { "elephant" }), "Missing creature cannot pass persistence.");
            Assertions.False(ExpandedSummoningRosterPersistencePolicy.ExactSelectedKeys(scope, new[] { "elephant", "brachiosaurus", "wolf" }), "Unrelated creature cannot broaden the targeted scope.");
            Assertions.False(ExpandedSummoningRosterPersistencePolicy.ExactSelectedKeys(scope, new[] { "elephant", "elephant" }), "Duplicate receipt cannot substitute for Brachiosaurus.");
            Assertions.True(ExpandedSummoningRosterPersistencePolicy.ObserveLoad(RuntimeTestScenarioCatalog.WorkingSaveExpandedSummoningPrepare, scope), "Exact prepare route uses the existing guarded load observer.");
            Assertions.False(ExpandedSummoningRosterPersistencePolicy.ObserveLoad(RuntimeTestScenarioCatalog.DisposableExpandedSummoningNativeProfile, scope), "The targeted scope cannot authorize a different scenario.");
        }

        private static SummonVariantSpec[] NewRoutes(SummonFamily family)
        {
            return ExpandedSummoningCatalog.GenerateVariants(family)
                .Where(value => GiantBodyRulesPolicy.IsWithheldThisSprint(
                    value.Creature.Key))
                .ToArray();
        }
    }
}
