using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.View.MapObjects.SriptZones;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.FavoredClass;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private const string FcbArchaeologistGuid = "38384e0c1e99c2e42ac6ed70a04aca46";
        private const string FcbInspireCourageBuffGuid = "b4027a834204042409248889cc8abf67";

        // O01: native Bard menus and live performance areas in the save-free
        // fixture scene (owner-local radius, neighbor, other performer, cap).
        private RuntimeTestResult RunFavoredClassPerformanceRange()
        {
            var assertions = new List<RuntimeTestAssertion>();
            FavoredClassIntegrationStatus status = FavoredClassIntegrationStatusRegistry.Current;
            FavoredClassHostHandles host = FavoredClassIntegrationCoordinator.Host;
            FavoredClassBlueprintSet leaves = BlueprintBootstrap.FavoredClassLeaves;
            BlueprintFeatureSelection bonus = host == null ? null :
                host.BonusSelectionFor(FavoredClassPerformanceManifest.BardClassGuid);
            bool ready = status.Availability == FavoredClassIntegrationAvailability.Published &&
                host != null && leaves != null && FavoredClassRuntime.MechanicsEnabled && bonus != null;
            assertions.Add(Assertion("fcb-performance-ready",
                "the exact host is published with a Bard bonus selection and mechanics are enabled",
                status + ";bardSelection=" + (bonus == null ? "none" : bonus.AssetGuid), ready,
                "FavoredClassIntegrationStatusRegistry and host.BonusSelectionFor"));
            if (!ready)
                return CreateResult(RuntimeTestStatuses.Fail, assertions, null);
            object player = ReadExactMember(Game.Instance, "Player");
            object state = ReadExactMember(Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);

            var evidence = new JObject();
            var menuFailures = new List<string>();
            var radiusFailures = new List<string>();
            bool cleaned = false;
            try
            {
                evidence["menus"] = RunBardMenus(bonus, leaves, menuFailures);
                evidence["radius"] = ObservePerformanceRadius(leaves, radiusFailures);
            }
            catch (Exception exception)
            {
                radiusFailures.Add("exception=" + exception);
            }
            finally
            {
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits));
            }
            string evidencePath = WriteFavoredClassEvidence("favored-class-performance-range.json", evidence);
            assertions.Add(Assertion("fcb-performance-menus",
                "an Oread Bard is offered exactly the counters of the manifest performances it has; a Human Bard and an Oread Archaeologist none",
                Describe(evidence["menus"], menuFailures), menuFailures.Count == 0,
                "level-1 native Bard visits; BlueprintFeatureSelection.CanSelect"));
            assertions.Add(Assertion("fcb-performance-radius",
                "the invested bard's own Inspire Courage area is 60 feet after two steps and 80 feet at the six-step cap, and includes a point at 55 feet that the native 50-foot area does not; another performance, another bard's same area and the shared blueprint keep their native size",
                Describe(evidence["radius"], radiusFailures), radiusFailures.Count == 0,
                "AreaEffectsController.SpawnAttachedToTarget in the save-free fixture scene; AreaEffectView.Shape"));
            assertions.Add(Assertion("external-isolation", "unchanged party and global-unit snapshots",
                "cleaned=" + cleaned, cleaned, "detached entity disposal and exact reference snapshots"));
            assertions.Add(Assertion("loaded-mod-version", _request.ExpectedModVersion,
                _context.ModEntry.Info.Version,
                _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                "Unity Mod Manager ModEntry.Info.Version"));
            RuntimeTestResult result = CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions, null);
            result.EvidenceFiles.Add(evidencePath);
            return result;
        }

        private JArray RunBardMenus(BlueprintFeatureSelection bonus, FavoredClassBlueprintSet leaves,
            IList<string> failures)
        {
            var library = BlueprintBootstrap.Library;
            BlueprintCharacterClass bard = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(library,
                FavoredClassPerformanceManifest.BardClassGuid, "Bard");
            FavoredClassLeafPair[] pairs = leaves.Pairs.Where(pair =>
                pair.Effect.Id == FavoredClassCatalog.EffectPerformanceRange).ToArray();
            var cases = new[]
            {
                Tuple.Create(FavoredClassAncestry.Oread, (string)null, true),
                Tuple.Create(FavoredClassAncestry.Human, (string)null, false),
                Tuple.Create(FavoredClassAncestry.Oread, FcbArchaeologistGuid, false),
            };
            var rows = new JArray();
            foreach (var entry in cases)
            {
                var row = new JObject { ["ancestry"] = entry.Item1, ["archetype"] = entry.Item2 };
                UnitEntityData unit = FavoredClassLevelUpHarness.CreateUnit(14);
                LevelUpController controller = null;
                try
                {
                    BlueprintRace race = BlueprintLibraryLookup.RequireExact<BlueprintRace>(library,
                        FavoredClassRaceIdentities.ForAncestry(entry.Item1).RaceGuid, entry.Item1);
                    controller = FavoredClassLevelUpHarness.Open(unit.Descriptor, race, bard, "KMG FCB Bard Menu",
                        entry.Item2 == null ? null : BlueprintLibraryLookup.RequireExact<BlueprintArchetype>(library,
                            entry.Item2, "Archaeologist"));
                    FavoredClassLevelUpHarness.ChooseFavoredClass(controller, bard, row);
                    row["filled"] = FavoredClassLevelUpHarness.FillOthers(controller,
                        new HashSet<string>(StringComparer.Ordinal) { bonus.AssetGuid });
                    UnitDescriptor preview = controller.Preview;
                    string[] owned = FavoredClassPerformanceManifest.All.Where(target =>
                        preview.Progression.Features.Enumerable.Any(value => value.Blueprint != null &&
                            value.Blueprint.AssetGuid == target.FeatureGuid)).Select(target => target.Key).ToArray();
                    FeatureSelectionState fcb = FavoredClassLevelUpHarness.FindOpenState(controller, bonus.AssetGuid);
                    string[] offered = fcb == null ? new string[0] : pairs.Where(pair =>
                        FavoredClassLevelUpHarness.CanSelect(controller, fcb, pair.Full))
                        .Select(pair => pair.TargetKey).ToArray();
                    row["owned"] = new JArray(owned);
                    row["offered"] = new JArray(offered);
                    if (entry.Item2 == null && entry.Item1 == FavoredClassAncestry.Oread && !owned.Contains("InspireCourage"))
                        failures.Add("the level-1 Oread Bard does not have Inspire Courage");
                    if (entry.Item2 != null && owned.Length != 0)
                        failures.Add("the Archaeologist kept a manifest performance: " + string.Join(",", owned));
                    string[] expected = entry.Item3 ? owned : new string[0];
                    if (!offered.OrderBy(value => value, StringComparer.Ordinal).SequenceEqual(
                            expected.OrderBy(value => value, StringComparer.Ordinal)))
                        failures.Add(entry.Item1 + (entry.Item2 == null ? "" : "/archaeologist") + ": offered " +
                            string.Join(",", offered) + " expected " + string.Join(",", expected));
                }
                catch (Exception exception)
                {
                    failures.Add(entry.Item1 + ": " + exception.GetType().Name + ": " + exception.Message);
                }
                finally
                {
                    FavoredClassLevelUpHarness.Close(controller);
                    unit.Dispose();
                }
                rows.Add(row);
            }
            return rows;
        }

        private static JObject ObservePerformanceRadius(FavoredClassBlueprintSet leaves, IList<string> failures)
        {
            var result = new JObject();
            var diagnostics = new List<string>();
            var library = BlueprintBootstrap.Library;
            var scene = new ElementalUndineFeatScenario.PortalHarness(diagnostics);
            var spawned = new List<AreaEffectEntityData>();
            try
            {
                BlueprintRace oread = BlueprintLibraryLookup.RequireExact<BlueprintRace>(library,
                    FavoredClassRaceIdentities.ForAncestry(FavoredClassAncestry.Oread).RaceGuid, "Oread");
                UnitEntityData bard = scene.Initialize(oread);
                UnitEntityData other = scene.SpawnFixtureUnit(oread, bard.Blueprint.Faction, new Vector3(0, 0, 40),
                    "PerformerB");
                FavoredClassPerformanceTarget courageTarget = FavoredClassPerformanceManifest.For("InspireCourage");
                FavoredClassPerformanceTarget competenceTarget = FavoredClassPerformanceManifest.For("InspireCompetence");
                var courage = BlueprintLibraryLookup.RequireExact<BlueprintAbilityAreaEffect>(library,
                    courageTarget.AreaGuids[0], "Inspire Courage area");
                var competence = BlueprintLibraryLookup.RequireExact<BlueprintAbilityAreaEffect>(library,
                    competenceTarget.AreaGuids[0], "Inspire Competence area");
                var owner = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library, FcbInspireCourageBuffGuid,
                    "Inspire Courage performer buff");
                BlueprintFeature leaf = leaves.Pair(FavoredClassCatalog.EffectPerformanceRange, "InspireCourage").Full;
                float nativeCourage = courage.Size.Meters;
                float nativeCompetence = competence.Size.Meters;
                Func<UnitEntityData, BlueprintAbilityAreaEffect, AreaEffectEntityData> spawn = (caster, area) =>
                {
                    AreaEffectEntityData data = AreaEffectsController.SpawnAttachedToTarget(
                        new MechanicsContext(caster, caster.Descriptor, owner), area, caster, null);
                    spawned.Add(data);
                    return data;
                };
                Func<AreaEffectEntityData, float> radius = data =>
                {
                    var cylinder = data == null || data.View == null ? null : data.View.Shape as ScriptZoneCylinder;
                    return cylinder == null ? -1f : cylinder.Radius;
                };
                Func<AreaEffectEntityData, float, bool> contains = (data, feet) =>
                    data.View.Shape.Contains(data.View.transform.position +
                        new Vector3(feet * FavoredClassMechanicsPolicy.FeetToMeters, 0f, 0f), 0f);
                Func<float, float, bool> same = (a, b) => Math.Abs(a - b) < 0.001f;

                AreaEffectEntityData control = spawn(bard, courage);
                GrantFavoredClassRanks(bard, leaf, 2);
                AreaEffectEntityData invested = spawn(bard, courage);
                AreaEffectEntityData neighbor = spawn(bard, competence);
                AreaEffectEntityData otherPerformer = spawn(other, courage);
                result["nativeCourageMeters"] = nativeCourage;
                result["controlMeters"] = radius(control);
                result["investedMeters"] = radius(invested);
                result["neighborMeters"] = radius(neighbor);
                result["otherPerformerMeters"] = radius(otherPerformer);
                result["controlContains55Feet"] = contains(control, 55f);
                result["investedContains55Feet"] = contains(invested, 55f);
                result["investedContains65Feet"] = contains(invested, 65f);
                GrantFavoredClassRanks(bard, leaf, 6);
                AreaEffectEntityData capped = spawn(bard, courage);
                result["cappedRank"] = bard.Descriptor.Progression.Features.GetRank(leaf);
                result["cappedMeters"] = radius(capped);
                result["blueprintMetersAfter"] = courage.Size.Meters;
                if (!same(radius(control), nativeCourage))
                    failures.Add("the uninvested bard's area is not the native 50 feet");
                if (!same(radius(invested), FavoredClassMechanicsPolicy.PerformanceRadiusMeters(nativeCourage, 2)))
                    failures.Add("two steps did not widen the bard's own area to 60 feet");
                if (!same(radius(neighbor), nativeCompetence))
                    failures.Add("another performance's area changed");
                if (!same(radius(otherPerformer), nativeCourage))
                    failures.Add("another bard's same area changed");
                if ((bool)result["controlContains55Feet"] || !(bool)result["investedContains55Feet"] ||
                    (bool)result["investedContains65Feet"])
                    failures.Add("native containment did not follow the widened radius");
                if ((int)result["cappedRank"] != 6 ||
                    !same(radius(capped), FavoredClassMechanicsPolicy.PerformanceRadiusMeters(nativeCourage, 6)))
                    failures.Add("the six-step cap did not give exactly +30 feet");
                if (!same(courage.Size.Meters, nativeCourage))
                    failures.Add("the shared area blueprint changed");
                result["diagnostics"] = new JArray(diagnostics);
            }
            catch (Exception exception)
            {
                failures.Add("probe: " + exception.GetType().Name + ": " + exception.Message);
            }
            finally
            {
                foreach (AreaEffectEntityData data in spawned.Where(value => value != null))
                    try
                    {
                        data.ForceEnd();
                        data.Destroy();
                    }
                    catch (Exception) { }
                try
                {
                    Game.Instance.EntityDestroyer.Tick();
                    Game.Instance.EntityDestroyer.Tick();
                }
                catch (Exception) { }
                scene.Dispose();
            }
            return result;
        }
    }
}
