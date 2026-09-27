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
        internal const int AppendedLedgerIdentities = 28;

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
            Assertions.Equal(12,
                SummonVisibilityCatalog.SuppressedLogicalPlacementCount,
                "Only Wasp's twelve legal placements may be suppressed.");
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
                .Build().Where(value => value.Symbol.StartsWith(
                    "KMG.Summoning.Natural.GiantWasp.",
                    StringComparison.Ordinal)).ToArray();
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
            Assertions.Equal(12,
                SummonVisibilityCatalog.SuppressedLogicalPlacementCount,
                "Prepublication review cannot make Wasp a player-visible choice.");
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
