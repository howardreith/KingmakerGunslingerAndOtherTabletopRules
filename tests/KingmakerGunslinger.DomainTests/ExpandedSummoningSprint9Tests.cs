using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningSprint9Tests
    {
        internal static void FlyingReviewRejectsStaticAnimationCaptures()
        {
            string root = Path.Combine(Environment.CurrentDirectory, "src",
                "KingmakerGunslinger", "RuntimeTesting");
            string review = File.ReadAllText(Path.Combine(root,
                "RuntimeTestRunner.PteranodonReview.cs"));
            string scenario = File.ReadAllText(Path.Combine(root,
                "RuntimeTestRunner.ExpandedSummoningCreatureReview.cs"));
            Assertions.True(review.Contains("MovementAgent.TickMovement(delta)") &&
                review.Contains("_motionReviewMaxPlanarTravel >= 0.75f") &&
                review.Contains("_motionReviewMaxDestinationApproach >= 0.75f") &&
                review.Contains("_motionReviewMinDestinationGap <= 2f") &&
                review.Contains("_motionReviewMaxVelocity > 0.01f") &&
                !review.Contains(".ForcePath(") &&
                review.Contains("_motionReviewAppearanceCleared &&") &&
                review.Contains("_motionReviewAwakeRestored &&") &&
                review.Contains("PrepareSprint9FlightMovement(unit)") &&
                review.Contains("Sprint9FlightLineClear(graph, start, destination)") &&
                review.Contains("value.nearest.clampedPosition) >= 2.5f") &&
                review.Contains("value.name == \"Palace_SmallWall_01_Door_05\"") &&
                review.Contains("_motionReviewDoorwayCrossed") &&
                review.Contains("!_motionReviewDoorwayDirectClear") &&
                review.Contains("position.x > _motionReviewDoorwayCrossingX") &&
                review.Contains("position.z < _motionReviewDoorwayCrossingZ") &&
                review.Contains("MotionReviewDoorwayValid") &&
                review.Contains("DescribeSprint9NearbyDoors(") &&
                review.Contains("value.name.IndexOf(\"_door_\",") &&
                review.Contains("value.name.IndexOf(\"_arch_\",") &&
                review.Contains("a.node.GraphIndex == anchor.node.GraphIndex") &&
                review.Contains(";nearbyDoors=") &&
                review.Contains("DescribeSprint9FloorGrid(anchor)") &&
                review.Contains(";floorGrid="),
                "The flight review must move a native agent and measure travel, not accept static animation frames.");
            Assertions.True(scenario.Contains(
                "expanded-summoning-flight-travel-") &&
                scenario.Contains("MotionReviewTravelValid") &&
                scenario.Contains("MotionReviewDoorwayValid"),
                "Eagle and Bat must fail the runtime review when measured travel is absent.");
        }

        internal static void ModuleBoundaryObservationIsNarrowAndTyped()
        {
            string root = Environment.CurrentDirectory;
            string runtime = Path.Combine(root, "src", "KingmakerGunslinger",
                "RuntimeTesting");
            string catalog = File.ReadAllText(Path.Combine(runtime,
                "RuntimeTestScenarioCatalog.cs"));
            string request = File.ReadAllText(Path.Combine(runtime,
                "RuntimeTestRequest.cs"));
            string boundary = File.ReadAllText(Path.Combine(runtime,
                "RuntimeTestRunner.ExpandedSummoningBoundary.cs"));
            string automation = File.ReadAllText(Path.Combine(root, "scripts",
                "RuntimeAutomation.Common.ps1"));
            string profile = File.ReadAllText(Path.Combine(root, "scripts",
                "compatibility", "Invoke-KingmakerCompatibilityProfile.ps1"));
            Assertions.True(catalog.Contains("ObserveExpandedSummoningModuleBoundary,") &&
                request.Contains("expanded-summoning-module-boundary-parameters-invalid") &&
                request.Contains("request.Parameters.Count != 1") &&
                request.Contains("request.Parameters[\"expandedSummoning\"].Type != JTokenType.Boolean") &&
                boundary.Contains("ObserveExpandedSummoningBoundary(expected)") &&
                boundary.Contains("expectedReading == actualReading") &&
                boundary.Contains("expanded-summoning-module-boundary") &&
                !boundary.Contains("RunFeatureModuleSettingsObservation()") &&
                automation.Contains("exactly one Boolean expandedSummoning parameter") &&
                automation.Contains("[ordered]@{ expandedSummoning = [bool]$Parameters.expandedSummoning }") &&
                profile.Contains("expandedSummoning = [bool]$Parameters.expandedSummoning"),
                "The guarded read-only boundary must preserve an exact Boolean request and judge only the live summon publication surface.");
        }

        internal static void ModuleOffPersistenceRequiresUsableDonorViews()
        {
            string root = Path.Combine(Environment.CurrentDirectory, "src",
                "KingmakerGunslinger", "RuntimeTesting");
            string runner = File.ReadAllText(Path.Combine(root,
                "RuntimeTestRunner.cs"));
            string views = File.ReadAllText(Path.Combine(root,
                "RuntimeTestRunner.PteranodonAttachedView.cs"));
            string assets = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Assets",
                "PteranodonAssetRuntime.cs"));
            Assertions.True(assets.Contains("donor-visual:module-disabled") &&
                runner.Contains("IsUsableDisabledSummonDonor(") &&
                runner.Contains("_expandedSummoningPersistencePteranodonVisualValid = prepare || verifyCleanup") &&
                runner.Contains("_expandedSummoningPersistenceFlyingVisualValid = prepare ||") &&
                runner.Contains("DescribeView(value.View) ==") &&
                views.Contains("IsDonorUntouched(renderers)") &&
                views.Contains(";enabled=true;") &&
                views.Contains(";material=<none>;") &&
                views.Contains("mesh=<none>;"),
                "A disabled fresh load must retain usable native donor bodies for all three saved flyers while their custom assets stay off.");
        }

        internal static void FlyingCombatFixtureUsesOwnTierAndExactTarget()
        {
            foreach (var entry in new[] { new { Key = "eagle", Tier = 1 },
                new { Key = "dire-bat", Tier = 3 } })
            {
                SummonVariantSpec variant = ExpandedSummoningCatalog
                    .GenerateVariants(SummonFamily.Monster).Single(value =>
                        value.Creature.Key == entry.Key &&
                        value.ParentTier == entry.Tier &&
                        value.Multiplicity == SummonMultiplicity.One);
                Assertions.True(SummonVisibilityCatalog.IsPublished(variant),
                    entry.Key + " must remain a published own-tier single cast.");
            }
            string source = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "RuntimeTesting", "SummonSameTurnActivationScenario.cs"));
            string request = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRequest.cs"));
            string launcher = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "scripts",
                "Invoke-KingmakerRuntimeTest.ps1"));
            string automation = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "scripts",
                "RuntimeAutomation.Common.ps1"));
            // The fixture used to carry its own eagle/dire-bat/else tier table.
            // It now reads each creature's own Summon Monster tier out of the
            // frozen catalog, which is the same fact without a second copy of
            // it, and refuses a creature that has no such tier.
            Assertions.True(source.Contains(
                    "SummonCreatureSpec creature = ExpandedSummoningCatalog.All") &&
                source.Contains("int tier = creature.MonsterTier.Value;") &&
                source.Contains(
                    "The activation case needs a Summon Monster tier: ") &&
                source.Contains("ExpandedSummoningIdentityCatalog") &&
                source.Contains("PrepareQuickenedSummon(_spellbook,") &&
                source.Contains("attack.Target, _enemy") &&
                source.Contains("AllUnitsAtLeast(_flightTargetAttacksByUnit,") &&
                source.Contains("ObserveFlightImpactGeometry(attack)") &&
                source.Contains("targetBounds.ClosestPoint(bone.position)") &&
                source.Contains("\"Jaw\", \"Head\", \"L_Foot0\", \"R_Foot0\"") &&
                source.Contains("renderer.BakeMesh(baked)") &&
                source.Contains("renderer.sharedMesh.boneWeights") &&
                source.Contains("targetBounds.ClosestPoint(point)") &&
                source.Contains("target.GetComponentsInChildren<SkinnedMeshRenderer>(true)") &&
                source.Contains("value.sharedMesh.vertexCount >= 100") &&
                source.Contains("sourceBounds=") &&
                source.Contains("bakedBounds=") &&
                source.Contains("BakedFlightVertexWorld(renderer,") &&
                source.Contains("renderer.transform.rotation * vertex") &&
                request.Contains("flight-activation-creature-invalid") &&
                request.Contains("creatureReview || flightActivation ? 2 : 1") &&
                launcher.Contains("$Parameters.ContainsKey('flightCreature')") &&
                launcher.Contains("flightCreature = [string]$Parameters.flightCreature") &&
                // The allowlist stays a closed, named set; Sprint 12's Dire Rat
                // and Sprint 13's Wolverine and Shadow Mastiff joined it so
                // ground creatures can prove both combat modes.
                automation.Contains("$Parameters.flightCreature -cnotin @('eagle', 'dire-bat', 'giant-wasp', 'stirge', 'dire-rat', 'wolverine', 'shadow-mastiff')") &&
                automation.Contains("flightCreature = [string]$Parameters.flightCreature"),
                "The guarded combat fixture must select only named published creatures and correlate a native attack to its exact hostile.");
        }

        internal static void EagleVisualLungeIsBoundedAndRestored()
        {
            Assertions.True(EagleAttackLungePolicy.MaximumMeters <= 1f,
                "The Eagle attack pose must remain a short visual motion.");
            Assertions.Equal(0f, EagleAttackLungePolicy.Weight(-1f, -1f),
                "Invalid attack time cannot move the skeleton.");
            Assertions.Equal(0f, EagleAttackLungePolicy.Weight(0f, -1f),
                "The visual starts at its native pose.");
            Assertions.True(Math.Abs(EagleAttackLungePolicy.Weight(0.08f,
                -1f) - 0.5f) < 0.001f,
                "The visual approaches the target during the swing.");
            Assertions.Equal(1f, EagleAttackLungePolicy.Weight(0.3f, -1f),
                "The swing reaches its bounded impact position.");
            Assertions.Equal(0f, EagleAttackLungePolicy.Weight(2f, -1f),
                "An interrupted swing returns to native pose.");
            Assertions.Equal(1f, EagleAttackLungePolicy.Weight(0.4f, 0f),
                "The impact pose is held for the native weapon event.");
            Assertions.True(Math.Abs(EagleAttackLungePolicy.Weight(0.5f,
                0.21f) - 0.5f) < 0.001f,
                "The impact pose returns smoothly.");
            Assertions.Equal(0f, EagleAttackLungePolicy.Weight(0.7f, 0.5f),
                "The impact pose cannot persist after the attack.");

            string root = Environment.CurrentDirectory;
            string visual = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Summoning",
                "EagleAttackVisualLunge.cs"));
            string attachment = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Summoning",
                "ExpandedSummoningPteranodonViewPatch.cs"));
            Assertions.True(visual.Contains("TryStartNextAttack") &&
                visual.Contains("TriggerAttackRule") &&
                visual.Contains("_root.position = _lastAppliedWorld") &&
                visual.Contains("private void OnDisable()") &&
                visual.Contains("private void OnDestroy()") &&
                !visual.Contains("_view.transform.position =") &&
                attachment.Contains("attachment.EagleLunge.Configure(view, donor)") &&
                attachment.Contains("UnityEngine.Object.Destroy(attachment.EagleLunge)"),
                "The Eagle lunge must move only its attached skeleton and restore on failure or teardown.");
        }

        internal static void DireBatPublishesOnlyItsPreservedPlacements()
        {
            SummonVariantSpec[] all = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster).Concat(
                    ExpandedSummoningCatalog.GenerateVariants(
                        SummonFamily.NaturesAlly)).ToArray();
            SummonVariantSpec[] bat = all.Where(value =>
                value.Creature.Key == "dire-bat").ToArray();
            Assertions.Equal(904, all.Count(SummonVisibilityCatalog.IsPublished),
                "The published surface must exclude only unqualified candidates.");
            Assertions.Equal(0, all.Count(value =>
                    !SummonVisibilityCatalog.IsPublished(value)),
                "The authorized hidden set changed.");
            Assertions.Equal(14, bat.Length,
                "Dire Bat retains seven placements in each summon family.");
            Assertions.True(bat.All(SummonVisibilityCatalog.IsPublished),
                "Every qualified Dire Bat placement remains published.");
            foreach (SummonFamily family in new[] {
                SummonFamily.Monster, SummonFamily.NaturesAlly })
            {
                SummonVariantSpec[] familyBat = bat.Where(value =>
                    value.Family == family).OrderBy(value => value.ParentTier)
                    .ToArray();
                Assertions.Equal("3|4|5|6|7|8|9",
                    string.Join("|", familyBat.Select(value => value.ParentTier)),
                    "Every published Bat tier must retain its old placement.");
                Assertions.Equal(SummonMultiplicity.One,
                    familyBat[0].Multiplicity, "Bat starts as one summon.");
                Assertions.Equal(SummonMultiplicity.OneD3,
                    familyBat[1].Multiplicity, "The next Bat tier uses 1d3.");
                Assertions.True(familyBat.Skip(2).All(value =>
                    value.Multiplicity == SummonMultiplicity.OneD4PlusOne),
                    "Later Bat tiers use 1d4+1.");
            }
        }

        internal static void EagleAndBatPersistWithTheirOwnViews()
        {
            string runner = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs"));
            Assertions.True(runner.Contains(
                    "new[] { \"Monster\", \"eagle\", \"1\" }") &&
                runner.Contains(
                    "new[] { \"NaturesAlly\", \"dire-bat\", \"3\" }") &&
                runner.Contains("expanded-summoning-persistent-eagle-bat-visuals") &&
                runner.Contains("IsEagleAttached(") &&
                runner.Contains("IsDireBatAttached("),
                "The save/reload fixture must verify both original flying visuals.");
        }

        internal static void DireBatSenseHasASeparateBoundedIdentity()
        {
            var bat = ExpandedSummoningNaturalProfiles.For("dire-bat");
            Assertions.True(bat.Facts.Contains("DireBatBlindsense"),
                "Dire Bat needs its own imprecise blindsense fact.");
            Assertions.Equal(1, ExpandedSummoningIdentityCatalog.Build().Count(value =>
                value.Symbol == "KMG.Summoning.Natural.DireBat.Blindsense" &&
                value.PlannedType == "BlueprintFeature"),
                "The sense needs one append-only feature identity.");
            string builder = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Blueprints", "ExpandedSummoningNaturalBuilder.cs"));
            Assertions.True(builder.Contains("nativeSense.Blindsight = false") &&
                builder.Contains("nativeSense.Range = new Feet(40)"),
                "Dire Bat senses creatures within 40 feet without precise blindsight.");
            string runner = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs"));
            Assertions.True(runner.Contains("expanded-summoning-dire-bat-blindsense") &&
                runner.Contains("part.Reach(caster)") &&
                runner.Contains("{ \"DireBatBlindsense\", \"5dcc039bc9674208a51e4babcd8a30ee\" }") &&
                runner.Contains("\"KMG_Summoning_Natural_DireBat_Blindsense\""),
                "Guarded cast and inventory audits must recognize only the Bat-owned sense fact.");
        }

        internal static void DireBatOriginalMeshAndPaintingAreBound()
        {
            string root = Environment.CurrentDirectory;
            string directory = Path.Combine(root, "assets", "flying-animals");
            string meshPath = Path.Combine(directory, "dire-bat-mesh.json");
            Assertions.True(File.Exists(meshPath),
                "The bat's original skinned mesh must ship beside its albedo.");
            JObject document = JObject.Parse(File.ReadAllText(meshPath));
            Assertions.Equal(2, (int)document["schemaVersion"],
                "The shared skinned-mesh runtime accepts schema 2.");
            Assertions.True(((string)document["space"]).Contains("donor renderer local"),
                "The bat is authored in the measured donor bind frame.");
            JArray bones = (JArray)document["bones"];
            Assertions.Equal(46, bones.Count, "Bat weights use the audited flying rig.");
            Assertions.Equal(46, bones.Select(value => (string)value)
                .Distinct(StringComparer.Ordinal).Count(), "No ambiguous bat bones.");
            int vertices = (int)document["vertexCount"];
            int triangles = (int)document["triangleCount"];
            Assertions.True(vertices > 1000 && triangles > 1000,
                "The bat has a skinned body, fingers and two-sided membranes.");
            byte[] bytes = Convert.FromBase64String((string)document["data"]);
            int expected = vertices * (12 + 12 + 8 + 4 * 8) + triangles * 3 * 4;
            Assertions.Equal(expected, bytes.Length,
                "The mesh payload exactly matches its declared vertex/triangle counts.");
            int weightOffset = vertices * (12 + 12 + 8) + triangles * 3 * 4;
            for (int vertex = 0; vertex < vertices; vertex++)
            {
                double total = 0;
                for (int slot = 0; slot < 4; slot++)
                {
                    int offset = weightOffset + vertex * 32 + slot * 8;
                    int bone = BitConverter.ToInt32(bytes, offset);
                    float weight = BitConverter.ToSingle(bytes, offset + 4);
                    Assertions.True(!float.IsNaN(weight) && weight >= 0f &&
                        (weight == 0f || bone >= 0 && bone < bones.Count),
                        "Bat vertex " + vertex + " has a valid bone weight.");
                    total += weight;
                }
                Assertions.True(Math.Abs(total - 1.0) < 0.001,
                    "Bat vertex " + vertex + " weights sum to one.");
            }
            JObject albedo = (JObject)document["albedo"];
            Assertions.Equal("dire-bat-albedo.png", (string)albedo["file"],
                "The mesh names only the adjacent original painting.");
            string pngPath = Path.Combine(directory, (string)albedo["file"]);
            Assertions.True(File.Exists(pngPath), "The bat painting must ship.");
            using (var sha = SHA256.Create())
            {
                string actual = string.Concat(sha.ComputeHash(File.ReadAllBytes(pngPath))
                    .Select(value => value.ToString("x2")));
                Assertions.Equal((string)albedo["sha256"], actual,
                    "The bat mesh is bound to the exact painted bytes.");
            }
            Assertions.True(File.Exists(Path.Combine(root, "assets-source",
                "original-models", "flying-animals", "generate_dire_bat.py")) &&
                File.Exists(Path.Combine(root, "assets-source", "original-models",
                    "flying-animals", "paint_dire_bat_albedo.py")),
                "Editable original mesh and painting generators are retained.");
        }

        internal static void EagleOriginalMeshAndPaintingAreBound()
        {
            string root = Environment.CurrentDirectory;
            string directory = Path.Combine(root, "assets", "flying-animals");
            string meshPath = Path.Combine(directory, "eagle-mesh.json");
            Assertions.True(File.Exists(meshPath),
                "The Small Eagle must have its own feathered mesh.");
            JObject document = JObject.Parse(File.ReadAllText(meshPath));
            Assertions.Equal(2, (int)document["schemaVersion"],
                "Eagle mesh must use the audited skinned-mesh schema.");
            Assertions.Equal(46, ((JArray)document["bones"]).Count,
                "Eagle uses the same measured flying-animal binding.");
            int vertices = (int)document["vertexCount"];
            int triangles = (int)document["triangleCount"];
            Assertions.True(vertices > 1500 && triangles > 1500,
                "Eagle must contain feather vanes, fan and body, not a donor skin.");
            byte[] bytes = Convert.FromBase64String((string)document["data"]);
            Assertions.Equal(vertices * 64 + triangles * 12, bytes.Length,
                "Eagle payload must contain exact vertices, faces and weights.");
            JObject albedo = (JObject)document["albedo"];
            Assertions.Equal("eagle-albedo.png", (string)albedo["file"],
                "Eagle mesh names its own painting.");
            string pngPath = Path.Combine(directory, (string)albedo["file"]);
            Assertions.True(File.Exists(pngPath), "Eagle painting must ship.");
            using (var sha = SHA256.Create())
            {
                string actual = string.Concat(sha.ComputeHash(File.ReadAllBytes(pngPath))
                    .Select(value => value.ToString("x2")));
                Assertions.Equal((string)albedo["sha256"], actual,
                    "The Eagle mesh is bound to its exact painting.");
            }
            Assertions.True(File.Exists(Path.Combine(root, "assets-source",
                "original-models", "flying-animals", "generate_eagle.py")) &&
                File.Exists(Path.Combine(root, "assets-source", "original-models",
                    "flying-animals", "paint_eagle_albedo.py")),
                "Retain editable original Eagle geometry and paint generation.");
        }
    }
}
