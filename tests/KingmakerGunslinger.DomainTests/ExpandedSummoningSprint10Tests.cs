using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningSprint10Tests
    {
        internal const int AppendedLedgerIdentities = 29;
        internal const int StirgeAppendedLedgerIdentities = 13;

        internal static void StirgeAttachRulesBoundDrainAndDetachment()
        {
            Assertions.Equal(8, StirgeAttachPolicy.MaintainGrappleRacialBonus,
                "An attached Stirge has the printed grapple bonus.");
            Assertions.Equal(10, StirgeAttachPolicy.DiseaseChancePercent,
                "One Stirge's blood drain has the printed disease chance.");
            Assertions.True(StirgeAttachPolicy.MayAttach(true, false, true),
                "A touch hit against a live prey establishes attachment.");
            Assertions.False(StirgeAttachPolicy.MayAttach(false, false, true),
                "A missed touch attack cannot attach.");
            Assertions.False(StirgeAttachPolicy.MayAttach(true, true, true),
                "One Stirge cannot establish a second simultaneous link.");
            Assertions.False(StirgeAttachPolicy.MayAttach(true, false, false),
                "A dead target cannot become a new attachment.");

            int cumulative = 0;
            for (int turn = 1; turn <= 4; turn++)
            {
                Assertions.Equal(1, StirgeAttachPolicy.RequestedDamage(true,
                    true, cumulative),
                    "A live attached prey receives one drain attempt.");
                StirgeDrainStep step = StirgeAttachPolicy.EndTurn(true,
                    true, cumulative, 1);
                Assertions.Equal(1, step.Damage,
                    "Each attached end turn drains exactly one Constitution.");
                Assertions.Equal(turn, step.CumulativeDamage,
                    "The same Stirge's drain tracks its four-point meal.");
                Assertions.Equal(turn == 4, step.Detach,
                    "The Stirge detaches only on reaching four points.");
                cumulative = step.CumulativeDamage;
            }
            StirgeDrainStep immune = StirgeAttachPolicy.EndTurn(true,
                true, 2, 0);
            Assertions.True(immune.Damage == 0 &&
                immune.CumulativeDamage == 2 && !immune.Detach,
                "An immune target cannot advance the four-point meal.");
            StirgeDrainStep dead = StirgeAttachPolicy.EndTurn(true, false,
                2, 0);
            Assertions.True(dead.Detach && dead.Damage == 0 &&
                dead.CumulativeDamage == 2,
                "Prey death releases the Stirge without an extra drain.");
            StirgeDrainStep escaped = StirgeAttachPolicy.EndTurn(false,
                true, 2, 0);
            Assertions.True(!escaped.Detach && escaped.Damage == 0,
                "Escape ends future blood drain.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                StirgeAttachPolicy.EndTurn(true, true, 5, 1),
                "Out-of-range cumulative damage must fail closed.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                StirgeAttachPolicy.EndTurn(false, true, 2, 1),
                "Detached Stirges cannot claim blood drain.");
        }

        internal static void GiantWaspRegisteredUnderSuppressionAtExactTiers()
        {
            SummonCreatureSpec wasp = ExpandedSummoningCatalog.All.Single(value =>
                value.Key == "giant-wasp");
            Assertions.Equal(4, wasp.MonsterTier.Value,
                "Wasp belongs at Summon Monster IV.");
            Assertions.Equal(4, wasp.NaturesAllyTier.Value,
                "Wasp belongs at Nature's Ally IV.");
            Assertions.True(wasp.MonsterTemplated,
                "Summon Monster Wasp keeps the native template policy.");
            foreach (SummonFamily family in new[] { SummonFamily.Monster,
                SummonFamily.NaturesAlly })
            {
                SummonVariantSpec[] variants = ExpandedSummoningCatalog
                    .GenerateVariants(family).Where(value =>
                        value.Creature.Key == wasp.Key).OrderBy(value =>
                            value.ParentTier).ToArray();
                Assertions.Equal(6, variants.Length,
                    "Wasp has one identity at each legal tier in each family.");
                Assertions.Equal(SummonMultiplicity.One, variants[0].Multiplicity,
                    "Wasp's own tier is a single creature.");
                Assertions.Equal(SummonMultiplicity.OneD3, variants[1].Multiplicity,
                    "Wasp's next tier is 1d3.");
                Assertions.True(variants.Skip(2).All(value =>
                    value.Multiplicity == SummonMultiplicity.OneD4PlusOne),
                    "Later Wasp tiers use 1d4+1.");
                Assertions.True(variants.All(value =>
                    !SummonVisibilityCatalog.IsPublished(value)),
                    "Wasp must stay hidden until poison and combat pass live gates.");
            }
            NaturalSummonProfile profile = ExpandedSummoningNaturalProfiles
                .For("giant-wasp");
            Assertions.Equal("Vermin", profile.HitDieClass,
                "Vermin racial hit dice carry Wasp immunities.");
            Assertions.Equal(4, profile.HitDice, "Wasp has four racial hit dice.");
            Assertions.Equal("Large", profile.Size, "Wasp is Large.");
            Assertions.Equal("WaspSting1d8", profile.PrimaryWeapon,
                "Wasp requires its exact sting weapon.");
            Assertions.Equal(60, profile.SpeedFeet,
                "Wasp uses its flying speed on Kingmaker maps.");
            Assertions.True(profile.Facts.Contains("Airborne"),
                "Wasp must navigate as an airborne creature.");
            Assertions.Equal("406c1e1af5400ac4881e330502ccbd9e",
                ExpandedSummoningDonorCatalog.For("giant-wasp").Guid,
                "Wasp must bind against the audited Giant Eagle skeleton.");
            Assertions.True(ExpandedSummoningIdentityCatalog.Build().Any(value =>
                value.Symbol == "KMG.Summoning.Natural.WaspSting1d8" &&
                value.PlannedType == "BlueprintItemWeapon"),
                "Wasp sting has its own append-only blueprint identity.");
            Assertions.Equal(21,
                SummonVisibilityCatalog.SuppressedLogicalPlacementCount,
                "Wasp and Stirge's 21 legal placements remain suppressed.");
        }

        internal static void StirgeRegisteredAtAllNineHiddenNatureTiers()
        {
            SummonCreatureSpec stirge = ExpandedSummoningCatalog.All.Single(value =>
                value.Key == "stirge");
            Assertions.True(!stirge.MonsterTier.HasValue &&
                stirge.NaturesAllyTier == 1 && !stirge.MonsterTemplated,
                "Stirge belongs only to the untemplated Nature's Ally I roster.");
            SummonVariantSpec[] variants = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.NaturesAlly).Where(value =>
                    value.Creature.Key == "stirge").OrderBy(value =>
                        value.ParentTier).ToArray();
            Assertions.Equal(9, variants.Length,
                "Stirge owns one legal placement at each Nature's Ally tier.");
            for (int tier = 1; tier <= 9; tier++)
            {
                SummonVariantSpec variant = variants[tier - 1];
                Assertions.Equal(tier, variant.ParentTier,
                    "Stirge's tier order is append-only and complete.");
                Assertions.Equal(tier == 1 ? SummonMultiplicity.One :
                    tier == 2 ? SummonMultiplicity.OneD3 :
                    SummonMultiplicity.OneD4PlusOne, variant.Multiplicity,
                    "Stirge follows the preserved summon quantity ladder.");
                Assertions.False(SummonVisibilityCatalog.IsPublished(variant),
                    "Incomplete Stirge mechanics and visual cannot enter a menu.");
            }
            NaturalSummonProfile profile = ExpandedSummoningNaturalProfiles
                .For("stirge");
            Assertions.True(profile.HitDieClass == "MagicalBeast" &&
                profile.HitDice == 1 && profile.Size == "Tiny" &&
                profile.SpeedFeet == 40 && profile.PrimaryWeapon ==
                "StirgeTouch" && profile.Facts.Contains("Airborne") &&
                profile.Facts.Contains("WeaponFinesse"),
                "The hidden profile must retain Stirge's physical and touch-carrier role.");
            Assertions.Equal("406c1e1af5400ac4881e330502ccbd9e",
                ExpandedSummoningDonorCatalog.For("stirge").Guid,
                "The hidden candidate uses the audited flying donor rig.");
            Assertions.True(ExpandedSummoningIdentityCatalog.Build().Any(value =>
                value.Symbol == "KMG.Summoning.Natural.StirgeTouch" &&
                value.PlannedType == "BlueprintItemWeapon"),
                "The zero-damage touch carrier needs its own stable identity.");
            Assertions.False(SummonIconCatalog.All.Any(value =>
                value.Key == "stirge"),
                "A hidden candidate has no player-visible icon consumer yet.");
            string builder = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Blueprints", "ExpandedSummoningNaturalBuilder.cs"));
            Assertions.True(builder.Contains("StirgeTouchSymbol), StirgeTouchSymbol, 0, DiceType.Zero"),
                "The carrier cannot inflict ordinary weapon damage during attachment.");
            Assertions.True(builder.Contains("17451c1327c571641a1345bd31155209") &&
                builder.Contains("nativeTouch.AttackType != AttackType.Touch") &&
                builder.Contains("ConfigureWeapon(nativeTouch"),
                "Stirge must clone the game's verified held-touch weapon, not a bite AC type.");
            string runtime = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs"));
            Assertions.True(runtime.Contains("expanded-summoning-stirge-native-touch-attack") &&
                runtime.Contains("roll.AttackType == AttackType.Touch") &&
                runtime.Contains("ordinaryAc > touchAc && roll.TargetAC == touchAc") &&
                runtime.Contains("roll.IsHit && damageAfter == damageBefore"),
                "The guarded combat fixture must demand a real zero-HP touch hit against armored AC controls.");
            Assertions.True(runtime.Contains("expanded-summoning-stirge-native-attachment") &&
                runtime.Contains("SummonHoldComponent.HeldTarget(stirge)") &&
                runtime.Contains("ReleaseExpandedSummoningHold(stirge, hostile, hold)"),
                "The guarded fixture must inspect and release both ends of the Stirge's native link.");
            Assertions.True(runtime.Contains("expanded-summoning-stirge-first-blood-drain") &&
                runtime.Contains("liveHold.OnNewRound()") &&
                runtime.Contains("liveHold.CumulativeDamage == 1 && stillAttached"),
                "The first live round must measure actual Constitution loss and persistent attachment.");
            Assertions.True(runtime.Contains("expanded-summoning-stirge-four-point-detach") &&
                runtime.Contains("for (int round = 2; round <= 4") &&
                runtime.Contains("fourPointDetach = mealExact && automaticCleanup"),
                "The guarded fixture must require four actual drains and automatic native release.");
            Assertions.True(runtime.Contains("expanded-summoning-stirge-escape-and-transition") &&
                runtime.Contains("Kingmaker.UnitLogic.UnitHelper.TryBreakFree(hostile,") &&
                runtime.Contains("SummonGrappleAreaSafeguard.Sweep(true,") &&
                runtime.Contains("escapeAndTransition = reattachedForEscape && nativeEscape"),
                "Victim escape and area leave must re-establish and release real native Stirge links.");
            string special = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Summoning", "ExpandedSummoningSpecialCombatComponents.cs"));
            string specialBuilder = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Blueprints", "ExpandedSummoningSpecialBuilder.cs"));
            Assertions.True(special.Contains("StirgeAttachPolicy.MayAttach") &&
                special.Contains("StirgeAttachPolicy.EndTurn") &&
                special.Contains("new RuleDealStatDamage(owner, target,") &&
                special.Contains("target.Ensure<UnitPartGrappleTarget>().Init") &&
                specialBuilder.Contains("ConfigureStirgeAttachment(bySymbol)") &&
                specialBuilder.Contains("UnitCondition.LoseDexterityToAC"),
                "The hidden unit must own a direct-hit native link and bounded actual-Constitution drain.");
            Assertions.Equal(21,
                SummonVisibilityCatalog.SuppressedLogicalPlacementCount,
                "All 12 Wasp and nine Stirge placements remain private.");
        }

        internal static void GiantWaspPoisonTracksConstitutionAndTabletopExposure()
        {
            Assertions.Equal(18, GiantWaspPoisonPolicy.DifficultyClass(4),
                "The baseline 4-HD/Con 18 Wasp poison DC is 18.");
            Assertions.Equal(16, GiantWaspPoisonPolicy.DifficultyClass(2),
                "Constitution loss must lower the poison DC by the same amount.");
            Assertions.Equal(20, GiantWaspPoisonPolicy.DifficultyClass(6),
                "Constitution gains must raise the poison DC by the same amount.");
            Assertions.Equal(6, GiantWaspPoisonPolicy.Exposures,
                "Wasp poison has six total exposures, including the initial hit.");
            Assertions.Equal(1, GiantWaspPoisonPolicy.SavesToCure,
                "One successful later save cures Wasp venom.");
            Assertions.True(ExpandedSummoningNaturalProfiles.For("giant-wasp")
                .Facts.Contains("WaspPoison"),
                "The Wasp unit must carry its dedicated poison feature.");
            SummoningIdentitySpec[] poison = ExpandedSummoningIdentityCatalog
                .Build().Where(value => value.Symbol ==
                    "KMG.Summoning.Natural.GiantWasp.Poison" ||
                    value.Symbol ==
                    "KMG.Summoning.Natural.GiantWasp.Venom").ToArray();
            Assertions.Equal(2, poison.Length,
                "The poison feature and saved venom buff have separate identities.");
            Assertions.True(poison.Any(value => value.PlannedType ==
                    "BlueprintFeature") && poison.Any(value =>
                    value.PlannedType == "BlueprintBuff"),
                "Poison identities must preserve their blueprint types.");
        }

        internal static void SuppressedWaspHasNoVisibleIconConsumer()
        {
            Assertions.True(!SummonIconCatalog.All.Any(value =>
                value.Key == "giant-wasp"),
                "A suppressed Wasp is not yet a player-visible icon consumer.");
            string runtime = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs"));
            Assertions.True(runtime.Contains(
                    ".Where(SummonIconCatalog.IsPublishedSomewhere)") &&
                runtime.Contains("creatureIcons.Count &&"),
                "The live menu icon audit must check only published creatures.");
        }

        internal static void WaspPrepublicationReviewKeepsTheMenuHidden()
        {
            string root = Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "RuntimeTesting");
            string review = File.ReadAllText(Path.Combine(root,
                "RuntimeTestRunner.ExpandedSummoningCreatureReview.cs"));
            string movement = File.ReadAllText(Path.Combine(root,
                "RuntimeTestRunner.PteranodonReview.cs"));
            Assertions.True(review.Contains("suppressedWaspCandidate = key == \"giant-wasp\"") &&
                review.Contains("!SummonVisibilityCatalog.IsPublished(variant)") &&
                review.Contains("!suppressedWaspCandidate") &&
                review.Contains("key == \"giant-wasp\"") &&
                review.Contains("MotionReviewTravelValid") &&
                review.Contains("MotionReviewDoorwayValid") &&
                movement.Contains("GiantWaspBlueprintName") &&
                movement.Contains("PrepareSprint9FlightMovement(unit)"),
                "The hidden Wasp may enter only its guarded review and must use native flight travel checks.");
            Assertions.True(review.Contains("CaptureWaspWithoutAuxiliaryRenderer") &&
                review.Contains("giant-wasp-review-summoned-attack-no-auxiliary.png") &&
                review.Contains("finally") &&
                review.Contains("renderer.enabled = true"),
                "The isolated Wasp frame must restore every temporarily hidden auxiliary renderer.");
            Assertions.Equal(21,
                SummonVisibilityCatalog.SuppressedLogicalPlacementCount,
                "Prepublication review cannot make Wasp or Stirge visible.");
        }

        internal static void WaspQuantityCoverageRemainsPrivate()
        {
            SummonVariantSpec[] crowd = new[] { SummonFamily.Monster,
                    SummonFamily.NaturesAlly }
                .SelectMany(ExpandedSummoningCatalog.GenerateVariants)
                .Where(value => value.Creature.Key == "giant-wasp" &&
                    value.Multiplicity != SummonMultiplicity.One)
                .GroupBy(value => new { value.Family, value.Multiplicity })
                .Select(group => group.OrderBy(value => value.ParentTier).First())
                .ToArray();
            Assertions.Equal(4, crowd.Length,
                "Both Wasp families must offer 1d3 and 1d4+1 for the guarded quantity fixture.");
            Assertions.True(crowd.All(value =>
                    !SummonVisibilityCatalog.IsPublished(value)),
                "Mechanical quantity casts cannot publish a Wasp menu choice.");
            string runtime = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs"));
            Assertions.True(runtime.Contains("expanded-summoning-giant-wasp-quantity") &&
                runtime.Contains(".Concat(waspCrowd).ToArray()") &&
                runtime.Contains("waspCrowdLegal == 4"),
                "The guarded cast loop must exercise all four private Wasp quantity variants.");
        }

        internal static void WaspVerminProbeUsesTheNativeTypeFeature()
        {
            string runtime = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs"));
            Assertions.True(runtime.Contains(
                    "09478937695300944a179530664e42ec") &&
                runtime.Replace("\r\n", "\n").Contains(
                    "blueprint.name ==\n                    \"KMG_Summoning_Natural_GiantWasp_Poison\"") &&
                runtime.Contains("wasp.Descriptor.HasFact(verminType)") &&
                runtime.Contains("expanded-summoning-giant-wasp-vermin-immunity") &&
                runtime.Contains("SpellImmunityToSpellDescriptor") &&
                runtime.Contains("Rulebook.Trigger(waspRule)") &&
                runtime.Contains("Rulebook.Trigger(humanRule)") &&
                runtime.Contains("waspRule.Immunity && !waspRule.CanApply") &&
                runtime.Contains("!humanRule.Immunity && humanRule.CanApply") &&
                runtime.Contains("RemoveFact(onWasp)") &&
                runtime.Contains("RemoveFact(onHuman)"),
                "The guarded Wasp immunity check must compare native mind-affecting buff outcomes and clean both units.");
        }

        internal static void WaspHasAnOwnedSpeciesMarker()
        {
            SummoningIdentitySpec marker = ExpandedSummoningIdentityCatalog
                .Build().Single(value => value.Symbol ==
                    "KMG.Summoning.Natural.GiantWasp.UnitType");
            Assertions.Equal("BlueprintUnitType", marker.PlannedType,
                "Wasp must have an append-only inspectable species identity.");
            string builder = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Blueprints", "ExpandedSummoningNaturalBuilder.cs"));
            Assertions.True(builder.Contains("unit.Type = Require<BlueprintUnitType>") &&
                builder.Contains("type.KnowledgeStat = StatType.SkillLoreNature") &&
                builder.Contains("type.Image = null"),
                "Wasp must not inherit the Eagle donor's type or image.");
        }

        internal static void HiddenWaspUsesBoundedNativeFlightCombatReview()
        {
            string root = Environment.CurrentDirectory;
            string scenario = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "SummonSameTurnActivationScenario.cs"));
            string request = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRequest.cs"));
            string launcher = File.ReadAllText(Path.Combine(root, "scripts",
                "Invoke-KingmakerRuntimeTest.ps1"));
            string automation = File.ReadAllText(Path.Combine(root, "scripts",
                "RuntimeAutomation.Common.ps1"));
            Assertions.True(scenario.Contains("SummonMonsterFourGuid") &&
                scenario.Contains("_flightCreature == \"giant-wasp\" ? 2 : 1") &&
                scenario.Contains("new[] { \"Tail\" }") &&
                scenario.Contains("sprint10-flight-") &&
                scenario.Contains("_waspImpactCaptures < 2") &&
                scenario.Contains("WriteExpandedSummoningPartyCameraCapture(") &&
                scenario.Contains("WriteExpandedSummoningOverheadStrikeCapture(") &&
                scenario.Contains("ProbeWaspTailAim(attack, mesh, targetBounds") &&
                scenario.Contains("tail.rotation = native;") &&
                scenario.Contains("class WaspTailAimFrameProbe : MonoBehaviour") &&
                scenario.Contains("yield return new WaitForEndOfFrame()") &&
                scenario.Contains("private void OnDisable()") &&
                request.Contains("\"eagle\", \"dire-bat\", \"giant-wasp\"") &&
                launcher.Contains("@('eagle', 'dire-bat', 'giant-wasp')") &&
                automation.Contains("@('eagle', 'dire-bat', 'giant-wasp')"),
                "Only the named hidden Wasp may enter the guarded native flight-combat fixture, with two exact hostile strikes and stinger geometry.");
        }

        internal static void GiantWaspOriginalVisualUsesAuditedInstanceBinding()
        {
            string root = Environment.CurrentDirectory;
            string directory = Path.Combine(root, "assets", "flying-animals");
            string path = Path.Combine(directory, "giant-wasp-mesh.json");
            JObject mesh = JObject.Parse(File.ReadAllText(path));
            Assertions.Equal(2, (int)mesh["schemaVersion"],
                "Wasp must use the shared skinned-mesh schema.");
            Assertions.True(((string)mesh["space"]).Contains("donor renderer local"),
                "Wasp geometry must bind in the donor's measured local frame.");
            int vertices = (int)mesh["vertexCount"];
            int triangles = (int)mesh["triangleCount"];
            Assertions.True(vertices >= 500 && triangles >= 500,
                "Wasp must carry a body, stinger, six legs and four wings.");
            byte[] payload = Convert.FromBase64String((string)mesh["data"]);
            Assertions.Equal(vertices * 64 + triangles * 12, payload.Length,
                "Wasp vertex, triangle and bone-weight payload must be complete.");
            string[] bones = ((JArray)mesh["bones"])
                .Select(value => (string)value).ToArray();
            Assertions.Equal(bones.Length,
                bones.Distinct(StringComparer.Ordinal).Count(),
                "Every Wasp weight must have one unambiguous bone name.");
            Assertions.True(bones.Contains("L_Arm_Upper") &&
                bones.Contains("R_Arm_Upper") && bones.Contains("Tail"),
                "Wing and stinger geometry must follow animated donor bones.");
            JObject albedo = (JObject)mesh["albedo"];
            Assertions.Equal("giant-wasp-albedo.png", (string)albedo["file"],
                "Wasp mesh must name its own adjacent painting.");
            using (var sha = SHA256.Create())
            {
                string actual = string.Concat(sha.ComputeHash(File.ReadAllBytes(
                    Path.Combine(directory, (string)albedo["file"])))
                    .Select(value => value.ToString("x2")));
                Assertions.Equal((string)albedo["sha256"], actual,
                    "Wasp painting must match the exact audited mesh manifest.");
            }
            Assertions.True(File.Exists(Path.Combine(root, "assets-source",
                "original-models", "flying-animals", "generate_giant_wasp.py")) &&
                File.Exists(Path.Combine(root, "assets-source",
                    "original-models", "flying-animals",
                    "paint_giant_wasp_albedo.py")),
                "Wasp's original editable geometry and painting must remain available.");
            string view = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Summoning",
                "ExpandedSummoningPteranodonViewPatch.cs"));
            Assertions.True(view.Contains("TryGetGiantWaspVisual") &&
                view.Contains("GiantWaspVisualName") &&
                view.Contains("TryResolveDonorBinding(donor, boneNames") &&
                view.Contains("donor.sharedMesh = mesh") &&
                view.Contains("donor.sharedMaterials = new[] { material }") &&
                view.Contains("Revert(attachment)"),
                "Wasp must share the validated per-instance swap and rollback.");
            string loader = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Assets", "PteranodonAssetRuntime.cs"));
            Assertions.True(loader.Contains(
                "assets/flying-animals/giant-wasp-mesh.json") &&
                loader.Contains("ConfigureGiantWasp(context)") &&
                loader.Contains("BuildMesh(File.ReadAllText(path)") &&
                loader.Contains("LoadAlbedo(Path.GetDirectoryName(path)"),
                "Wasp must load through the package's audited mesh and painting parser.");
            string audit = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.ExpandedSummoningNativeDonors.cs"));
            Assertions.True(audit.Contains("giant-wasp-original-asset-loader") &&
                audit.Contains("PteranodonAssetRuntime.GiantWaspStatus == \"visual:published\""),
                "The guarded audit must fail if the packaged Wasp mesh or painting is rejected in game.");
        }

        internal static void NativeFlyingVerminSurveyStaysMetadataOnly()
        {
            string source = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "RuntimeTesting",
                "RuntimeTestRunner.ExpandedSummoningNativeDonors.cs"));
            Assertions.True(source.Contains("\"wasp\", \"stirge\"") &&
                source.Contains("\"mosquito\"") &&
                source.Contains("\"beetle\"") &&
                source.Contains("\"mantis\"") &&
                source.Contains("56ec8788092b6314e8f3c1c502e8433f") &&
                source.Contains("\"blood\"") &&
                source.Contains("\"attach\"") &&
                source.Contains("\"drain\"") &&
                source.Contains("GetAllBlueprints().Where(value => value != null)") &&
                source.Contains("units.Add(DescribeNativeUnit(unit))") &&
                source.Contains("native-donor-audit.json") &&
                !source.Contains("AssetBundle.LoadFromFile") &&
                !source.Contains("Texture2D.EncodeToPNG"),
                "Sprint 10 intake must inventory installed native metadata for both flying vermin and their signature seams without exporting game art.");
        }
    }
}
