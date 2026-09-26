using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.SriptZones;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.FavoredClass;
using KingmakerGunslinger.FavoredClass.Hooks;
using KingmakerGunslinger.FavoredClass.Mechanics;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private const string FcbArchaeologistGuid = "38384e0c1e99c2e42ac6ed70a04aca46";
        private const string FcbInspireCourageBuffGuid = "b4027a834204042409248889cc8abf67";

        private PerformanceRangeProbe _performanceRange;
        private JArray _performanceMenus;
        private List<string> _performanceMenuFailures;
        private object[] _performanceParty;
        private object[] _performanceUnits;

        // O01: native Bard menus, then every published performance in the
        // save-free fixture scene across frames: two owners with different
        // investments, the per-instance radius, the ring geometry, the owner's
        // own texts, excluded targets, and a released and reclaimed pooled
        // ring. Returns null until complete.
        private RuntimeTestResult PollFavoredClassPerformanceRange()
        {
            var assertions = new List<RuntimeTestAssertion>();
            if (_performanceRange == null)
            {
                FavoredClassIntegrationStatus status = FavoredClassIntegrationStatusRegistry.Current;
                FavoredClassHostHandles host = FavoredClassIntegrationCoordinator.Host;
                FavoredClassBlueprintSet leaves = BlueprintBootstrap.FavoredClassLeaves;
                BlueprintFeatureSelection bonus = host == null ? null :
                    host.BonusSelectionFor(FavoredClassPerformanceManifest.BardClassGuid);
                bool ready = status.Availability == FavoredClassIntegrationAvailability.Published &&
                    host != null && leaves != null && FavoredClassRuntime.MechanicsEnabled && bonus != null &&
                    FavoredClassPerformanceRing.Available;
                if (!ready)
                {
                    assertions.Add(Assertion("fcb-performance-ready",
                        "the exact host is published with a Bard bonus selection, mechanics are enabled and Unity particles are reachable",
                        status + ";bardSelection=" + (bonus == null ? "none" : bonus.AssetGuid) + ";ring=" +
                            FavoredClassPerformanceRing.Available, false,
                        "FavoredClassIntegrationStatusRegistry and host.BonusSelectionFor"));
                    return CreateResult(RuntimeTestStatuses.Fail, assertions, null);
                }
                _performanceParty = SnapshotReferences(ReadExactMember(ReadExactMember(Game.Instance, "Player"),
                    "Party"));
                _performanceUnits = SnapshotReferences(ReadExactMember(ReadExactMember(Game.Instance, "State"),
                    "AllUnits"));
                _performanceMenuFailures = new List<string>();
                try
                {
                    _performanceMenus = RunBardMenus(bonus, leaves, _performanceMenuFailures);
                }
                catch (Exception exception)
                {
                    _performanceMenuFailures.Add("menus: " + exception.GetType().Name + ": " + exception.Message);
                }
                _performanceRange = new PerformanceRangeProbe(leaves);
            }
            _performanceRange.Poll();
            if (!_performanceRange.Done)
                return null;
            bool cleaned = SameReferences(_performanceParty, SnapshotReferences(ReadExactMember(
                    ReadExactMember(Game.Instance, "Player"), "Party"))) &&
                SameReferences(_performanceUnits, SnapshotReferences(ReadExactMember(
                    ReadExactMember(Game.Instance, "State"), "AllUnits")));
            var evidence = new JObject
            {
                ["menus"] = _performanceMenus,
                ["targets"] = _performanceRange.Targets,
                ["excluded"] = _performanceRange.Excluded,
                ["release"] = _performanceRange.Release,
                ["injected"] = _performanceRange.Injected,
                ["outcomeText"] = _performanceRange.OutcomeText,
                ["unresolved"] = _performanceRange.Unresolved,
                ["diagnostics"] = new JArray(_performanceRange.Diagnostics),
            };
            string evidencePath = WriteFavoredClassEvidence("favored-class-performance-range.json", evidence);
            PerformanceRangeProbe probe = _performanceRange;
            assertions.Add(Assertion("fcb-performance-menus",
                "an Oread Bard is offered exactly the counters of the published performances it has; a Human Bard and an Oread Archaeologist none",
                Describe(_performanceMenus, _performanceMenuFailures), _performanceMenuFailures.Count == 0,
                "level-1 native Bard visits; BlueprintFeatureSelection.CanSelect"));
            assertions.Add(Assertion("fcb-performance-radius",
                "for every published performance present: the uninvested bard keeps the native radius, a two-step bard gets +10 feet and a six-step bard +30 feet on its own instance only, the shared blueprint never changes, and a point just beyond the native edge is inside only the widened instance",
                Describe(probe.Targets, probe.RadiusFailures), probe.RadiusFailures.Count == 0,
                "AreaEffectsController.SpawnAttachedToTarget; AreaEffectView.Shape (ScriptZoneCylinder)"));
            assertions.Add(Assertion("fcb-performance-ring",
                "each widened instance's ring is scaled horizontally by exactly its owner's radius ratio (no compounding, vertical axes unchanged); its emitter circles sit at the owner's radius as the native circles sit at the native radius; other instances keep the native ring",
                Describe(probe.Targets, probe.RingFailures), probe.RingFailures.Count == 0,
                "spawned AreaEffectView ring: ParticleSystem transforms, scaling modes and shape radii"));
            assertions.Add(Assertion("fcb-performance-text",
                "the invested bard's own performance feature, toggles and action-bar slot state its range; uninvested and other bards keep the exact native text",
                Describe(probe.Targets, probe.TextFailures), probe.TextFailures.Count == 0,
                "Fact.Description (SelectUIData) and MechanicActionBarSlotActivableAbility.GetDescription"));
            assertions.Add(Assertion("fcb-performance-excluded",
                "Storm Call and Mockery counters, even when held, never widen an area, scale a ring or change a text",
                Describe(probe.Excluded, probe.ExcludedFailures), probe.ExcludedFailures.Count == 0,
                "FavoredClassPerformanceManifest publication exclusions"));
            assertions.Add(Assertion("fcb-performance-injected-failure",
                "an injected ring failure on a real invested bard's area (a scaler that scales then throws, and one that scales nothing) leaves that instance at its native radius with its ring restored exactly; the recovered next cast widens its cylinder and ring together; a failing second area narrows the widened first one and a success while both are live is held, so all three are native; a success beside a live deferred area is held and the late ring then keeps both native; a deferred ring alone leaves the instance native until the late ring widens both; a widened area whose rollback cannot restore its ring is ended",
                Describe(probe.Injected, probe.InjectedFailures), probe.InjectedFailures.Count == 0,
                "FavoredClassPerformanceRangePatch.RingScalerOverride and DeferRingsForQualification (guarded qualification seams); spawned instance radius and ring systems"));
            assertions.Add(Assertion("fcb-performance-outcome-text",
                "the invested bard's feature, toggle and action-bar descriptions follow the bard's live areas of the performance: the configured range before any cast and once every live area ended (nothing is remembered); the native range while any live area is native (failed, deferred, held or narrowed); the widened range together with a recovered or late-widened area",
                Describe(probe.OutcomeText, probe.OutcomeTextFailures), probe.OutcomeTextFailures.Count == 0,
                "Fact.Description (SelectUIData), ActivatableAbility.Description and MechanicActionBarSlotActivableAbility.GetDescription on the invested bard; FavoredClassPerformanceInstances"));
            assertions.Add(Assertion("fcb-performance-unresolved-ending",
                "on a real invested bard's areas, a widened area whose narrowing cannot be verified and whose ending throws, does nothing or finds no area data, and a widened area whose liveness read throws, all stay tracked and unresolved: every live area stays tracked, no widened live area sits beside native or configured text unless it is unresolved, a success is held while any area is unresolved, the descriptions are native, a diagnostic is logged for each; once the fault clears, the next widening attempt narrows or ends it verifiably (an ending that never took effect is retried and verified), and an area that ended while its liveness could not be read is forgotten only once the read works (fourth review finding 2)",
                Describe(probe.Unresolved, probe.UnresolvedFailures), probe.UnresolvedFailures.Count == 0,
                "FavoredClassPerformanceInstances EndFault, DataUnavailable, LivenessFault and NarrowFault qualification seams; AreaEffectEntityData.IsEnded; FavoredClassPerformanceInstances.RecentDiagnostics"));
            assertions.Add(Assertion("fcb-performance-ring-release",
                "every scaled ring is restored exactly when the pool releases it, and a ring reclaimed from the pool for an uninvested bard is native",
                Describe(probe.Release, probe.ReleaseFailures), probe.ReleaseFailures.Count == 0,
                "GameObjectsPool.Release prefix; reclaimed pooled effect transforms"));
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
                    string[] expected = entry.Item3 ? owned.Where(key =>
                        FavoredClassPerformanceManifest.For(key).Published).ToArray() : new string[0];
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

        /// <summary>The multi-frame O01 probe in the save-free fixture scene.</summary>
        private sealed class PerformanceRangeProbe
        {
            private static readonly Type ParticleSystemType = FindRuntimeType("UnityEngine.ParticleSystem");
            private static readonly FieldInfo SpawnedFx = typeof(AreaEffectView).GetField("m_SpawnedFx",
                BindingFlags.Instance | BindingFlags.NonPublic);
            private const int Low = 2;
            private const int High = 6;
            private const float ReleaseTimeoutSeconds = 20f;

            private readonly FavoredClassBlueprintSet _leaves;
            private readonly LibraryScriptableObject _library = BlueprintBootstrap.Library;
            private readonly List<AreaEffectEntityData> _spawned = new List<AreaEffectEntityData>();
            private readonly Dictionary<string, Dictionary<string, JObject>> _nativeSystems =
                new Dictionary<string, Dictionary<string, JObject>>(StringComparer.Ordinal);
            private readonly List<GameObject> _scaledRings = new List<GameObject>();
            private ElementalUndineFeatScenario.PortalHarness _scene;
            private UnitEntityData _control;
            private UnitEntityData _low;
            private UnitEntityData _high;
            private UnitEntityData _outcome;
            private int _stage;
            private float _releaseStarted;

            internal PerformanceRangeProbe(FavoredClassBlueprintSet leaves)
            {
                _leaves = leaves;
                Targets = new JArray();
                Excluded = new JArray();
                Release = new JObject();
                Diagnostics = new List<string>();
                RadiusFailures = new List<string>();
                RingFailures = new List<string>();
                TextFailures = new List<string>();
                ExcludedFailures = new List<string>();
                ReleaseFailures = new List<string>();
                Injected = new JObject();
                OutcomeText = new JObject();
                OutcomeTextFailures = new List<string>();
                InjectedFailures = new List<string>();
                Unresolved = new JObject();
                UnresolvedFailures = new List<string>();
            }

            internal bool Done { get; private set; }
            internal JArray Targets { get; private set; }
            internal JArray Excluded { get; private set; }
            internal JObject Release { get; private set; }
            internal List<string> Diagnostics { get; private set; }
            internal List<string> RadiusFailures { get; private set; }
            internal List<string> RingFailures { get; private set; }
            internal List<string> TextFailures { get; private set; }
            internal List<string> ExcludedFailures { get; private set; }
            internal List<string> ReleaseFailures { get; private set; }
            internal JObject Injected { get; private set; }
            internal JObject OutcomeText { get; private set; }
            internal List<string> OutcomeTextFailures { get; private set; }
            internal List<string> InjectedFailures { get; private set; }
            internal JObject Unresolved { get; private set; }
            internal List<string> UnresolvedFailures { get; private set; }

            internal void Poll()
            {
                if (Done) return;
                try
                {
                    if (_stage == 0)
                    {
                        Start();
                        foreach (FavoredClassPerformanceTarget target in FavoredClassPerformanceManifest.All)
                        {
                            if (target.Published) ObserveTarget(target);
                            else ObserveExcluded(target);
                        }
                        ObserveInjectedOutcomes();
                        Release["scaledBeforeRelease"] = FavoredClassPerformanceRing.ScaledCount;
                        DestroySpawned();
                        _releaseStarted = Time.realtimeSinceStartup;
                        _stage = 1;
                        return;
                    }
                    if (_stage == 1)
                    {
                        bool released = _scaledRings.All(ring => FavoredClassPerformanceRing.FactorOf(ring) == 1f);
                        if (!released && Time.realtimeSinceStartup - _releaseStarted < ReleaseTimeoutSeconds)
                            return;
                        Release["releasedSeconds"] = Time.realtimeSinceStartup - _releaseStarted;
                        Release["ringsPooled"] = _scaledRings.Count(ring => ring != null);
                        Release["ringsDestroyed"] = _scaledRings.Count(ring => ring == null);
                        Release["allRestored"] = released;
                        Release["scaledAfterRelease"] = FavoredClassPerformanceRing.ScaledCount;
                        if (!released)
                            ReleaseFailures.Add("a scaled ring was not restored by its pool release");
                        ObserveReclaimed();
                        DestroySpawned();
                        _stage = 2;
                        return;
                    }
                }
                catch (Exception exception)
                {
                    RadiusFailures.Add("probe: " + exception.GetType().Name + ": " + exception.Message);
                }
                Finish();
            }

            private void Start()
            {
                BlueprintRace oread = BlueprintLibraryLookup.RequireExact<BlueprintRace>(_library,
                    FavoredClassRaceIdentities.ForAncestry(FavoredClassAncestry.Oread).RaceGuid, "Oread");
                _scene = new ElementalUndineFeatScenario.PortalHarness(Diagnostics);
                _control = _scene.Initialize(oread);
                _low = _scene.SpawnFixtureUnit(oread, _control.Blueprint.Faction, new Vector3(0, 0, 40), "BardLow");
                _high = _scene.SpawnFixtureUnit(oread, _control.Blueprint.Faction, new Vector3(40, 0, 0), "BardHigh");
                _outcome = _scene.SpawnFixtureUnit(oread, _control.Blueprint.Faction, new Vector3(-40, 0, 0),
                    "BardOutcome");
            }

            private void ObserveTarget(FavoredClassPerformanceTarget target)
            {
                var row = new JObject { ["key"] = target.Key, ["baseFeet"] = target.BaseFeet };
                Targets.Add(row);
                BlueprintFeature feature = Resolve<BlueprintFeature>(target.FeatureGuid);
                if (feature == null)
                {
                    row["absent"] = target.Provider ? "provider performance not present" : "native feature missing";
                    if (!target.Provider)
                        RadiusFailures.Add(target.Key + ": the native performance feature is missing");
                    return;
                }
                BlueprintFeature leaf = _leaves.Pair(FavoredClassCatalog.EffectPerformanceRange, target.Key).Full;
                GrantFavoredClassRanks(_low, leaf, Low);
                GrantFavoredClassRanks(_high, leaf, High);
                var areas = new JArray();
                foreach (string areaGuid in target.AreaGuids)
                {
                    var area = Resolve<BlueprintAbilityAreaEffect>(areaGuid);
                    if (area == null)
                    {
                        RadiusFailures.Add(target.Key + ": area " + areaGuid + " missing");
                        continue;
                    }
                    areas.Add(ObserveArea(target, area));
                }
                row["areas"] = areas;
                row["text"] = ObserveText(target, feature);
                RemoveRanks(_low, leaf);
                RemoveRanks(_high, leaf);
            }

            // PR #24 reviews: the widening transaction of a real invested bard's
            // own areas under injected ring failures, a deferred ring, a
            // success beside native siblings and an unverifiable rollback, with
            // every live area, its ring and the bard's feature, toggle and
            // action-bar descriptions after every step. A fresh bard keeps
            // earlier targets' live areas out of the observation.
            private void ObserveInjectedOutcomes()
            {
                FavoredClassPerformanceTarget target = FavoredClassPerformanceManifest.For("InspireCourage");
                BlueprintFeature leaf = _leaves.Pair(FavoredClassCatalog.EffectPerformanceRange, target.Key).Full;
                var area = Resolve<BlueprintAbilityAreaEffect>(target.AreaGuids[0]);
                BlueprintFeature feature = Resolve<BlueprintFeature>(target.FeatureGuid);
                Dictionary<string, JObject> native;
                if (area == null || feature == null || !_nativeSystems.TryGetValue(target.RingAssetId, out native))
                {
                    InjectedFailures.Add("Inspire Courage's area, feature or native ring systems were not observed");
                    return;
                }
                UnitEntityData bard = _outcome;
                float nativeRadius = area.Size.Meters;
                float widened = FavoredClassMechanicsPolicy.PerformanceRadiusMeters(nativeRadius, Low);
                int widenedFeet = FavoredClassPerformanceManifest.OwnerFeet(target, Low);
                // Exactly the native local scales the uninvested bard's ring showed.
                Func<GameObject, bool> nativeRing = ring =>
                {
                    if (ring == null || FavoredClassPerformanceRing.IsScaled(ring))
                        return false;
                    Dictionary<string, JObject> systems = Systems(ring);
                    return native.All(entry => systems.ContainsKey(entry.Key) &&
                        Floats(systems[entry.Key]["localScale"]).SequenceEqual(Floats(entry.Value["localScale"])));
                };
                Func<AreaEffectEntityData, bool> nativeInstance = instance =>
                    Same(Radius(instance), nativeRadius) && nativeRing(Ring(instance));
                Func<AreaEffectEntityData, bool> widenedInstance = instance =>
                    Same(Radius(instance), widened) && Ring(instance) != null &&
                    FavoredClassPerformanceRing.IsScaled(Ring(instance));
                GrantFavoredClassRanks(bard, leaf, Low);
                Feature fact = bard.Descriptor.AddFact(feature) as Feature ??
                    bard.Descriptor.Progression.Features.GetFact(feature) as Feature;
                ActivatableAbility toggle = bard.Descriptor.ActivatableAbilities.Enumerable.FirstOrDefault(value =>
                    value.Blueprint != null && target.ToggleGuids.Contains(value.Blueprint.AssetGuid));
                try
                {
                    if (fact == null || toggle == null)
                    {
                        OutcomeTextFailures.Add("the invested bard's performance feature or toggle is missing");
                        return;
                    }
                    string nativeFeature = feature.Description;
                    string nativeToggle = toggle.Blueprint.Description;
                    string widenedFeature = FavoredClassPerformanceText.OwnerDescription(nativeFeature,
                        target.BaseFeet, widenedFeet);
                    string widenedToggle = FavoredClassPerformanceText.OwnerDescription(nativeToggle,
                        target.BaseFeet, widenedFeet);
                    if (widenedFeature == nativeFeature || widenedToggle == nativeToggle)
                        OutcomeTextFailures.Add("the widened descriptions do not differ from the native ones");
                    // Each surface: native, widened or other.
                    Func<JObject> texts = () =>
                    {
                        Func<string, string, string, string> label = (actual, nativeText, widenedText) =>
                            actual == nativeText ? "native" : actual == widenedText ? "widened" : "other";
                        var slot = new MechanicActionBarSlotActivableAbility { ActivatableAbility = toggle, Unit = bard };
                        return new JObject
                        {
                            ["feature"] = label(fact.Description, nativeFeature, widenedFeature),
                            ["toggle"] = label(toggle.Description, nativeToggle, widenedToggle),
                            ["actionBar"] = label(slot.GetDescription(), nativeToggle, widenedToggle),
                            ["liveAreas"] = FavoredClassPerformanceInstances.LiveCount(bard.Descriptor, target.Key)
                        };
                    };
                    Func<JObject, string, bool> all = (row, expected) => (string)row["feature"] == expected &&
                        (string)row["toggle"] == expected && (string)row["actionBar"] == expected;
                    Func<AreaEffectEntityData, JObject> mechanics = instance =>
                    {
                        FavoredClassWideningOutcome? recorded = instance == null || instance.View == null ? null :
                            FavoredClassPerformanceInstances.OutcomeOf(bard.Descriptor, target.Key, instance.View);
                        return new JObject
                        {
                            ["radius"] = Radius(instance),
                            ["ringPresent"] = Ring(instance) != null,
                            ["ringFactor"] = Ring(instance) == null ? 1f :
                                FavoredClassPerformanceRing.FactorOf(Ring(instance)),
                            ["ringNative"] = nativeRing(Ring(instance)),
                            ["ended"] = instance != null && instance.IsEnded,
                            ["recorded"] = recorded == null ? null : recorded.Value.ToString()
                        };
                    };
                    Action<string, string, string> expectTexts = (step, expected, problem) =>
                    {
                        JObject row = texts();
                        OutcomeText[step] = row;
                        if (!all(row, expected))
                            OutcomeTextFailures.Add(step + ": " + problem);
                    };
                    Func<string, AreaEffectEntityData> failing = fault =>
                    {
                        FavoredClassPerformanceRangePatch.RingScalerOverride = FaultyScaler(fault);
                        try
                        {
                            return Spawn(bard, area);
                        }
                        finally
                        {
                            FavoredClassPerformanceRangePatch.RingScalerOverride = null;
                        }
                    };
                    Func<AreaEffectEntityData> deferredSpawn = () =>
                    {
                        FavoredClassPerformanceRangePatch.DeferRingsForQualification = true;
                        try
                        {
                            return Spawn(bard, area);
                        }
                        finally
                        {
                            FavoredClassPerformanceRangePatch.DeferRingsForQualification = false;
                        }
                    };

                    // 0. Before any cast: the owner's configured range.
                    expectTexts("before-cast", "widened", "the descriptions do not show the configured range");

                    // 1-2. Injected failures on a real instance, then the area
                    // ends: nothing is remembered, so the configured range returns.
                    foreach (string fault in new[] { "scaled-then-threw", "scaled-nothing" })
                    {
                        AreaEffectEntityData failed = failing(fault);
                        Injected[fault] = mechanics(failed);
                        if (!Same(Radius(failed), nativeRadius))
                            InjectedFailures.Add(fault + ": the cylinder stayed widened after the ring failed");
                        if (!nativeRing(Ring(failed)))
                            InjectedFailures.Add(fault + ": the ring was not restored to its native transforms");
                        expectTexts(fault, "native", "a description advertised the widened range for a native area");
                        End(failed);
                        expectTexts(fault + "-ended", "widened",
                            "after the failed area ended the descriptions did not return to the configured range");
                    }

                    // 3. The recovered next cast: widened mechanics and text together.
                    AreaEffectEntityData first = Spawn(bard, area);
                    Injected["recovered"] = mechanics(first);
                    if (Ring(first) != null)
                        _scaledRings.Add(Ring(first));
                    if (!widenedInstance(first))
                        InjectedFailures.Add("recovered: the next cast did not widen its cylinder and ring together");
                    expectTexts("recovered", "widened", "the recovered cast's descriptions do not show its range");

                    // 4. A failing second area narrows the first; a success
                    // while both are live is held, so all three converge.
                    AreaEffectEntityData second = failing("scaled-then-threw");
                    AreaEffectEntityData third = Spawn(bard, area);
                    Injected["success-beside-native"] = new JObject
                    {
                        ["first"] = mechanics(first),
                        ["second"] = mechanics(second),
                        ["third"] = mechanics(third)
                    };
                    if (!nativeInstance(first) || !nativeInstance(second) || !nativeInstance(third))
                        InjectedFailures.Add("success-beside-native: the live areas did not converge on native");
                    expectTexts("success-beside-native", "native", "a description disagreed with the native areas");
                    End(first);
                    End(second);
                    End(third);
                    expectTexts("success-beside-native-ended", "widened",
                        "after the areas ended the descriptions did not return to the configured range");

                    // 5. A success while another live area waits for its ring
                    // is held; the late ring then keeps both native.
                    AreaEffectEntityData waiting = deferredSpawn();
                    AreaEffectEntityData beside = Spawn(bard, area);
                    Injected["success-beside-deferred"] = new JObject
                    {
                        ["deferred"] = mechanics(waiting),
                        ["success"] = mechanics(beside)
                    };
                    if (!nativeInstance(waiting) || !nativeInstance(beside))
                        InjectedFailures.Add("success-beside-deferred: a success widened beside a deferred area");
                    expectTexts("success-beside-deferred", "native", "a description disagreed with the native areas");
                    FavoredClassPerformanceRangePatch.ScaleLateRing(waiting.View);
                    Injected["late-ring-beside-held"] = new JObject
                    {
                        ["deferred"] = mechanics(waiting),
                        ["success"] = mechanics(beside)
                    };
                    if (!nativeInstance(waiting) || !nativeInstance(beside))
                        InjectedFailures.Add("late-ring-beside-held: the late ring widened beside a native area");
                    expectTexts("late-ring-beside-held", "native", "a description disagreed with the native areas");
                    End(waiting);
                    End(beside);

                    // 6. A deferred ring alone: native until the late ring, then widened together.
                    AreaEffectEntityData deferred = deferredSpawn();
                    Injected["deferred"] = mechanics(deferred);
                    if (!nativeInstance(deferred))
                        InjectedFailures.Add("deferred: the instance widened before its ring");
                    expectTexts("deferred", "native", "a description advertised the widened range before the ring");
                    FavoredClassPerformanceRangePatch.ScaleLateRing(deferred.View);
                    Injected["late-ring"] = mechanics(deferred);
                    if (Ring(deferred) != null)
                        _scaledRings.Add(Ring(deferred));
                    if (!widenedInstance(deferred))
                        InjectedFailures.Add("late-ring: the late ring did not widen the cylinder and ring together");
                    expectTexts("late-ring", "widened", "the late-widened area's descriptions do not show its range");
                    End(deferred);

                    // 7. A widened area whose rollback cannot restore its ring
                    // is ended rather than kept live with native text.
                    AreaEffectEntityData widenedArea = Spawn(bard, area);
                    GameObject widenedRing = Ring(widenedArea);
                    if (widenedRing != null)
                        _scaledRings.Add(widenedRing);
                    FavoredClassPerformanceInstances.NarrowFaultForQualification = view =>
                    {
                        if (ReferenceEquals(view, widenedArea.View))
                            throw new InvalidOperationException("KMG injected ring restore failure while narrowing");
                    };
                    AreaEffectEntityData trigger;
                    try
                    {
                        trigger = failing("scaled-then-threw");
                    }
                    finally
                    {
                        FavoredClassPerformanceInstances.NarrowFaultForQualification = null;
                    }
                    Injected["unverifiable-rollback"] = new JObject
                    {
                        ["widened"] = mechanics(widenedArea),
                        ["trigger"] = mechanics(trigger)
                    };
                    if (!widenedArea.IsEnded)
                        InjectedFailures.Add("unverifiable-rollback: the area whose ring could not be restored stayed live");
                    if (!nativeInstance(trigger))
                        InjectedFailures.Add("unverifiable-rollback: the failed area was not native");
                    expectTexts("unverifiable-rollback", "native", "a description disagreed with the live failed area");
                    if ((int)OutcomeText["unverifiable-rollback"]["liveAreas"] != 1)
                        InjectedFailures.Add("unverifiable-rollback: the ended area was still counted live");
                    End(trigger);
                    End(widenedArea);
                    expectTexts("unverifiable-rollback-ended", "widened",
                        "after the areas ended the descriptions did not return to the configured range");

                    // 8. Fourth review, finding 2: an area that cannot be
                    // narrowed and whose ending throws, does nothing or finds
                    // no area data, and an area whose liveness read throws,
                    // stay tracked and unresolved; the next widening attempt
                    // retries once the fault clears.
                    ObserveUnresolvedEndings(bard, area, target, nativeRadius, texts, all, mechanics,
                        nativeInstance, widenedInstance, failing);
                }
                finally
                {
                    FavoredClassPerformanceRangePatch.RingScalerOverride = null;
                    FavoredClassPerformanceRangePatch.DeferRingsForQualification = false;
                    FavoredClassPerformanceInstances.NarrowFaultForQualification = null;
                    FavoredClassPerformanceInstances.EndFaultForQualification = null;
                    FavoredClassPerformanceInstances.DataUnavailableForQualification = null;
                    FavoredClassPerformanceInstances.LivenessFaultForQualification = null;
                    if (fact != null)
                        bard.Descriptor.RemoveFact(fact);
                    RemoveRanks(bard, leaf);
                }
            }

            // Fourth review, finding 2, on the invested bard's real areas. Each
            // case widens an area, makes its narrowing fail (the ring restore
            // throws) and its ending fail in one way, then records a failing
            // sibling: the area must stay live, tracked and unresolved, block
            // a success (held), keep the descriptions native and log a
            // diagnostic; once the fault clears the next widening attempt must
            // retry it verifiably. A last case makes the liveness read throw.
            private void ObserveUnresolvedEndings(UnitEntityData bard, BlueprintAbilityAreaEffect area,
                FavoredClassPerformanceTarget target, float nativeRadius, Func<JObject> texts,
                Func<JObject, string, bool> all, Func<AreaEffectEntityData, JObject> mechanics,
                Func<AreaEffectEntityData, bool> nativeInstance, Func<AreaEffectEntityData, bool> widenedInstance,
                Func<string, AreaEffectEntityData> failing)
            {
                int diagnosticsBefore = FavoredClassPerformanceInstances.RecentDiagnostics().Length;
                Func<AreaEffectEntityData, string> problem = instance => instance == null || instance.View == null ? null :
                    FavoredClassPerformanceInstances.ProblemOf(bard.Descriptor, target.Key, instance.View);
                Func<AreaEffectEntityData, FavoredClassWideningOutcome?> recorded = instance =>
                    instance == null || instance.View == null ? null :
                        FavoredClassPerformanceInstances.OutcomeOf(bard.Descriptor, target.Key, instance.View);
                Func<AreaEffectEntityData, JObject> tracked = instance =>
                {
                    JObject row = mechanics(instance);
                    row["problem"] = problem(instance);
                    return row;
                };
                // Every live area stays tracked; a widened live area beside
                // native or configured text is unresolved; the texts agree.
                Func<string, string, AreaEffectEntityData[], JObject> check = (step, expectedText, live) =>
                {
                    JObject row = texts();
                    row["unresolvedCount"] = FavoredClassPerformanceInstances.UnresolvedCount(bard.Descriptor, target.Key);
                    if (!all(row, expectedText))
                        UnresolvedFailures.Add(step + ": the descriptions are not " + expectedText);
                    bool widenedText = all(row, "widened");
                    foreach (AreaEffectEntityData instance in live)
                    {
                        if (instance == null || instance.IsEnded)
                            continue;
                        if (recorded(instance) == null)
                            UnresolvedFailures.Add(step + ": a live area is untracked");
                        GameObject ring = Ring(instance);
                        bool widenedNow = !Same(Radius(instance), nativeRadius) ||
                            (ring != null && FavoredClassPerformanceRing.IsScaled(ring));
                        if (widenedNow && !widenedText && problem(instance) == null)
                            UnresolvedFailures.Add(step + ": a widened live area sits beside " + expectedText +
                                " text without being unresolved");
                    }
                    return row;
                };
                Func<AreaEffectEntityData> widenedSpawn = () =>
                {
                    AreaEffectEntityData spawned = Spawn(bard, area);
                    if (Ring(spawned) != null)
                        _scaledRings.Add(Ring(spawned));
                    return spawned;
                };
                Func<AreaEffectEntityData, Action<AreaEffectView>> narrowFails = instance => view =>
                {
                    if (ReferenceEquals(view, instance.View))
                        throw new InvalidOperationException("KMG injected ring restore failure while narrowing");
                };
                try
                {
                    // 8a. The ending throws; the retry narrows.
                    AreaEffectEntityData throwing = widenedSpawn();
                    if (!widenedInstance(throwing))
                        UnresolvedFailures.Add("end-throws: the area did not widen first");
                    FavoredClassPerformanceInstances.NarrowFaultForQualification = narrowFails(throwing);
                    FavoredClassPerformanceInstances.EndFaultForQualification = view =>
                    {
                        if (ReferenceEquals(view, throwing.View))
                            throw new InvalidOperationException("KMG injected ending failure");
                        return true;
                    };
                    AreaEffectEntityData throwTrigger = failing("scaled-then-threw");
                    AreaEffectEntityData throwHeld = Spawn(bard, area);
                    Unresolved["end-throws"] = new JObject
                    {
                        ["unresolved"] = tracked(throwing), ["trigger"] = tracked(throwTrigger), ["held"] = tracked(throwHeld),
                        ["texts"] = check("end-throws", "native", new[] { throwing, throwTrigger, throwHeld })
                    };
                    // The failed narrowing restored the radius but not the ring.
                    if (throwing.IsEnded || nativeInstance(throwing) || problem(throwing) == null ||
                        !problem(throwing).Contains("ending it threw"))
                        UnresolvedFailures.Add("end-throws: the area whose ending threw is not live, unrestored and unresolved");
                    if (recorded(throwHeld) != FavoredClassWideningOutcome.Held || !nativeInstance(throwHeld))
                        UnresolvedFailures.Add("end-throws: a success beside the unresolved area was not held native");
                    FavoredClassPerformanceInstances.NarrowFaultForQualification = null;
                    FavoredClassPerformanceInstances.EndFaultForQualification = null;
                    AreaEffectEntityData throwRetry = Spawn(bard, area);
                    Unresolved["end-throws-retried"] = new JObject
                    {
                        ["retried"] = tracked(throwing), ["held"] = tracked(throwRetry),
                        ["texts"] = check("end-throws-retried", "native",
                            new[] { throwing, throwTrigger, throwHeld, throwRetry })
                    };
                    if (!nativeInstance(throwing) || throwing.IsEnded ||
                        recorded(throwing) != FavoredClassWideningOutcome.Narrowed || problem(throwing) != null)
                        UnresolvedFailures.Add("end-throws: the next widening attempt did not narrow the area verifiably");
                    End(throwTrigger);
                    End(throwHeld);
                    End(throwRetry);
                    End(throwing);

                    // 8b. The ending does nothing; the retry ends it.
                    AreaEffectEntityData noOp = widenedSpawn();
                    FavoredClassPerformanceInstances.NarrowFaultForQualification = narrowFails(noOp);
                    FavoredClassPerformanceInstances.EndFaultForQualification = view => !ReferenceEquals(view, noOp.View);
                    AreaEffectEntityData noOpTrigger = failing("scaled-then-threw");
                    Unresolved["end-no-op"] = new JObject
                    {
                        ["unresolved"] = tracked(noOp), ["trigger"] = tracked(noOpTrigger),
                        ["texts"] = check("end-no-op", "native", new[] { noOp, noOpTrigger })
                    };
                    if (noOp.IsEnded || problem(noOp) == null || !problem(noOp).Contains("still live after ending"))
                        UnresolvedFailures.Add("end-no-op: the area whose ending did nothing is not live and unresolved");
                    FavoredClassPerformanceInstances.EndFaultForQualification = null;
                    AreaEffectEntityData noOpRetry = Spawn(bard, area);
                    Unresolved["end-no-op-retried"] = new JObject
                    {
                        ["retried"] = tracked(noOp), ["held"] = tracked(noOpRetry),
                        ["texts"] = check("end-no-op-retried", "native", new[] { noOp, noOpTrigger, noOpRetry })
                    };
                    if (!noOp.IsEnded || recorded(noOp) != null)
                        UnresolvedFailures.Add("end-no-op: the retried ending was not verified and forgotten");
                    if (recorded(noOpRetry) != FavoredClassWideningOutcome.Held || !nativeInstance(noOpRetry))
                        UnresolvedFailures.Add("end-no-op: the retry's own area was not held native");
                    FavoredClassPerformanceInstances.NarrowFaultForQualification = null;
                    End(noOpTrigger);
                    End(noOpRetry);
                    End(noOp);

                    // 8c. The area data is unavailable (it cannot be ended and
                    // counts as live); the retry narrows once it returns.
                    AreaEffectEntityData missing = widenedSpawn();
                    FavoredClassPerformanceInstances.NarrowFaultForQualification = narrowFails(missing);
                    FavoredClassPerformanceInstances.DataUnavailableForQualification = view =>
                        ReferenceEquals(view, missing.View);
                    AreaEffectEntityData missingTrigger = failing("scaled-then-threw");
                    Unresolved["data-unavailable"] = new JObject
                    {
                        ["unresolved"] = tracked(missing), ["trigger"] = tracked(missingTrigger),
                        ["texts"] = check("data-unavailable", "native", new[] { missing, missingTrigger })
                    };
                    if (missing.IsEnded || problem(missing) == null || !problem(missing).Contains("data is unavailable"))
                        UnresolvedFailures.Add("data-unavailable: the area without data is not live and unresolved");
                    FavoredClassPerformanceInstances.NarrowFaultForQualification = null;
                    FavoredClassPerformanceInstances.DataUnavailableForQualification = null;
                    AreaEffectEntityData missingRetry = Spawn(bard, area);
                    Unresolved["data-unavailable-retried"] = new JObject
                    {
                        ["retried"] = tracked(missing), ["held"] = tracked(missingRetry),
                        ["texts"] = check("data-unavailable-retried", "native",
                            new[] { missing, missingTrigger, missingRetry })
                    };
                    if (!nativeInstance(missing) || recorded(missing) != FavoredClassWideningOutcome.Narrowed ||
                        problem(missing) != null)
                        UnresolvedFailures.Add("data-unavailable: the retry did not narrow the area verifiably");
                    End(missingTrigger);
                    End(missingRetry);
                    End(missing);

                    // 8d. The liveness read throws: nothing is forgotten; a
                    // success is held (and narrows it); an area that ended
                    // while unreadable is forgotten only once the read works.
                    AreaEffectEntityData unreadable = widenedSpawn();
                    FavoredClassPerformanceInstances.LivenessFaultForQualification = view =>
                        ReferenceEquals(view, unreadable.View);
                    JObject alone = check("liveness-throws", "widened", new[] { unreadable });
                    Unresolved["liveness-throws"] = new JObject { ["unreadable"] = tracked(unreadable), ["texts"] = alone };
                    if (recorded(unreadable) == null || problem(unreadable) == null ||
                        !problem(unreadable).Contains("liveness"))
                        UnresolvedFailures.Add("liveness-throws: the unreadable area was forgotten or not unresolved");
                    AreaEffectEntityData besideUnreadable = Spawn(bard, area);
                    Unresolved["liveness-throws-beside"] = new JObject
                    {
                        ["unreadable"] = tracked(unreadable), ["held"] = tracked(besideUnreadable),
                        ["texts"] = check("liveness-throws-beside", "native", new[] { unreadable, besideUnreadable })
                    };
                    if (recorded(besideUnreadable) != FavoredClassWideningOutcome.Held || !nativeInstance(besideUnreadable) ||
                        !nativeInstance(unreadable) || recorded(unreadable) != FavoredClassWideningOutcome.Narrowed)
                        UnresolvedFailures.Add("liveness-throws: the success beside the unreadable area was not held, or did not narrow it");
                    unreadable.ForceEnd();
                    texts();
                    bool keptWhileUnreadable = recorded(unreadable) != null;
                    FavoredClassPerformanceInstances.LivenessFaultForQualification = null;
                    texts();
                    bool forgottenOnceReadable = recorded(unreadable) == null;
                    Unresolved["liveness-throws-ended"] = new JObject
                    {
                        ["keptWhileUnreadable"] = keptWhileUnreadable, ["forgottenOnceReadable"] = forgottenOnceReadable,
                        ["liveAreas"] = FavoredClassPerformanceInstances.LiveCount(bard.Descriptor, target.Key)
                    };
                    if (!keptWhileUnreadable || !forgottenOnceReadable)
                        UnresolvedFailures.Add("liveness-throws: an area that ended while unreadable was forgotten early or kept after the read worked");
                    End(besideUnreadable);
                    End(unreadable);
                    Unresolved["ended"] = check("unresolved-ended", "widened", new AreaEffectEntityData[0]);
                    if (FavoredClassPerformanceInstances.LiveCount(bard.Descriptor, target.Key) != 0)
                        UnresolvedFailures.Add("unresolved-ended: an area is still tracked after every area ended");

                    string[] diagnostics = FavoredClassPerformanceInstances.RecentDiagnostics()
                        .Skip(diagnosticsBefore).ToArray();
                    Unresolved["diagnostics"] = new JArray(diagnostics);
                    foreach (string expected in new[]
                        { "ending it threw", "still live after ending", "data is unavailable", "liveness probe threw" })
                        if (!diagnostics.Any(value => value.Contains(";unresolved") && value.Contains(expected)))
                            UnresolvedFailures.Add("diagnostics: no unresolved diagnostic for " + expected);
                    if (diagnostics.Count(value => value.Contains(";resolved")) < 4)
                        UnresolvedFailures.Add("diagnostics: fewer than four resolutions were logged");
                }
                finally
                {
                    FavoredClassPerformanceInstances.NarrowFaultForQualification = null;
                    FavoredClassPerformanceInstances.EndFaultForQualification = null;
                    FavoredClassPerformanceInstances.DataUnavailableForQualification = null;
                    FavoredClassPerformanceInstances.LivenessFaultForQualification = null;
                }
            }

            /// <summary>A ring scaler that scales then throws, or one that scales nothing.</summary>
            private static Func<GameObject, float, int> FaultyScaler(string fault)
            {
                return (effect, factor) =>
                {
                    if (fault == "scaled-nothing")
                        return 0;
                    FavoredClassPerformanceRing.Scale(effect, factor);
                    throw new InvalidOperationException("KMG injected ring failure after scaling");
                };
            }

            /// <summary>Ends one spawned instance exactly as the performance ending would.</summary>
            private void End(AreaEffectEntityData data)
            {
                if (data == null)
                    return;
                try
                {
                    AreaEffectView view = data.View;
                    data.ForceEnd();
                    data.Destroy();
                    if (view != null && view.gameObject != null)
                        UnityEngine.Object.Destroy(view.gameObject);
                }
                catch (Exception exception)
                {
                    Diagnostics.Add("end: " + exception.GetType().Name + ": " + exception.Message);
                }
                _spawned.Remove(data);
            }

            private JObject ObserveArea(FavoredClassPerformanceTarget target, BlueprintAbilityAreaEffect area)
            {
                float native = area.Size.Meters;
                float nativeSize = area.Size.Value;
                var row = new JObject { ["area"] = area.name + ":" + area.AssetGuid, ["sizeFeet"] = nativeSize };
                if ((int)nativeSize != target.BaseFeet)
                    RadiusFailures.Add(target.Key + ": the live area is " + nativeSize + " feet, not " + target.BaseFeet);
                AreaEffectEntityData control = Spawn(_control, area);
                AreaEffectEntityData low = Spawn(_low, area);
                AreaEffectEntityData high = Spawn(_high, area);
                float lowRadius = FavoredClassMechanicsPolicy.PerformanceRadiusMeters(native, Low);
                float highRadius = FavoredClassMechanicsPolicy.PerformanceRadiusMeters(native, High);
                row["controlMeters"] = Radius(control);
                row["lowMeters"] = Radius(low);
                row["highMeters"] = Radius(high);
                row["blueprintMetersAfter"] = area.Size.Meters;
                string label = target.Key + "/" + area.name;
                if (!Same(Radius(control), native)) RadiusFailures.Add(label + ": the uninvested bard's radius changed");
                if (!Same(Radius(low), lowRadius)) RadiusFailures.Add(label + ": two steps did not add exactly 10 feet");
                if (!Same(Radius(high), highRadius)) RadiusFailures.Add(label + ": six steps did not add exactly 30 feet");
                if (!Same(area.Size.Meters, native)) RadiusFailures.Add(label + ": the shared blueprint changed");
                float probe = native + 0.5f * FavoredClassMechanicsPolicy.FeetToMeters * FavoredClassPerformanceManifest.FeetPerStep;
                bool controlBeyond = Contains(control, probe), lowBeyond = Contains(low, probe);
                row["pointBeyondNativeEdge"] = new JObject { ["control"] = controlBeyond, ["low"] = lowBeyond };
                if (controlBeyond || !lowBeyond)
                    RadiusFailures.Add(label + ": containment did not follow the widened radius");
                row["ring"] = ObserveRing(target, area, label, control, low, high, native, lowRadius, highRadius);
                return row;
            }

            private JObject ObserveRing(FavoredClassPerformanceTarget target, BlueprintAbilityAreaEffect area,
                string label, AreaEffectEntityData control, AreaEffectEntityData low, AreaEffectEntityData high,
                float native, float lowRadius, float highRadius)
            {
                var row = new JObject { ["fxAssetId"] = area.Fx == null ? null : area.Fx.AssetId };
                GameObject controlRing = Ring(control), lowRing = Ring(low), highRing = Ring(high);
                bool expectRing = target.RingSpawns;
                row["present"] = new JObject { ["control"] = controlRing != null, ["low"] = lowRing != null,
                    ["high"] = highRing != null };
                if ((area.Fx == null ? null : area.Fx.AssetId) != target.RingAssetId)
                    RingFailures.Add(label + ": the ring effect link is not the manifest's " + target.RingAssetId);
                if (!expectRing)
                {
                    if (controlRing != null || lowRing != null)
                        RingFailures.Add(label + ": a ring exists although the provider link resolves to none");
                    row["noRing"] = "the provider's effect link names an area blueprint and spawns nothing for any bard";
                    return row;
                }
                if (controlRing == null || lowRing == null || highRing == null)
                {
                    RingFailures.Add(label + ": a ring effect was not spawned");
                    return row;
                }
                row["factor"] = new JObject { ["control"] = FavoredClassPerformanceRing.FactorOf(controlRing),
                    ["low"] = FavoredClassPerformanceRing.FactorOf(lowRing),
                    ["high"] = FavoredClassPerformanceRing.FactorOf(highRing) };
                _scaledRings.Add(lowRing);
                _scaledRings.Add(highRing);
                Dictionary<string, JObject> nativeSystems = Systems(controlRing);
                if (!_nativeSystems.ContainsKey(target.RingAssetId))
                    _nativeSystems[target.RingAssetId] = nativeSystems;
                row["low"] = CompareRing(label + "/two-step", nativeSystems, Systems(lowRing), lowRadius / native, native,
                    lowRadius);
                row["high"] = CompareRing(label + "/six-step", nativeSystems, Systems(highRing), highRadius / native,
                    native, highRadius);
                if (FavoredClassPerformanceRing.FactorOf(controlRing) != 1f)
                    RingFailures.Add(label + ": the uninvested bard's ring was scaled");
                return row;
            }

            private JObject CompareRing(string label, Dictionary<string, JObject> native,
                Dictionary<string, JObject> scaled, float factor, float nativeRadius, float ownerRadius)
            {
                var row = new JObject { ["factor"] = factor };
                var circles = new JArray();
                foreach (KeyValuePair<string, JObject> entry in native)
                {
                    JObject owner;
                    if (!scaled.TryGetValue(entry.Key, out owner))
                    {
                        RingFailures.Add(label + ": system " + entry.Key + " is missing");
                        continue;
                    }
                    float[] nativeScale = Floats(entry.Value["effectiveScale"]);
                    float[] ownerScale = Floats(owner["effectiveScale"]);
                    bool[] horizontal = ((JArray)entry.Value["horizontalAxes"]).Select(value => (bool)value).ToArray();
                    for (int axis = 0; axis < 3; axis++)
                    {
                        float expected = nativeScale[axis] * (horizontal[axis] ? factor : 1f);
                        if (Math.Abs(ownerScale[axis] - expected) > 0.001f * Math.Max(1f, Math.Abs(expected)))
                            RingFailures.Add(label + ": " + entry.Key + " axis " + axis + " is " +
                                ownerScale[axis].ToString("0.####", CultureInfo.InvariantCulture) + ", expected " +
                                expected.ToString("0.####", CultureInfo.InvariantCulture));
                    }
                    if ((bool)entry.Value["circle"])
                    {
                        float nativeCircle = (float)entry.Value["circleMeters"];
                        float ownerCircle = (float)owner["circleMeters"];
                        circles.Add(new JObject { ["system"] = entry.Key, ["nativeMeters"] = nativeCircle,
                            ["ownerMeters"] = ownerCircle });
                        if (entry.Key.IndexOf("Border", StringComparison.Ordinal) >= 0 ||
                            entry.Key.IndexOf("Notes", StringComparison.Ordinal) >= 0 ||
                            entry.Key.IndexOf("sparks", StringComparison.Ordinal) >= 0)
                        {
                            if (Math.Abs(nativeCircle - nativeRadius) > 0.1f * nativeRadius)
                                RingFailures.Add(label + ": native circle " + entry.Key + " is not at the native radius");
                            if (Math.Abs(ownerCircle - ownerRadius) > 0.1f * ownerRadius)
                                RingFailures.Add(label + ": owner circle " + entry.Key + " is not at the owner's radius");
                        }
                    }
                }
                row["circles"] = circles;
                row["systems"] = scaled.Count;
                return row;
            }

            private JObject ObserveText(FavoredClassPerformanceTarget target, BlueprintFeature feature)
            {
                var row = new JObject();
                BlueprintFeature leaf = _leaves.Pair(FavoredClassCatalog.EffectPerformanceRange, target.Key).Full;
                // The ranks were granted by ObserveTarget; granting again would add a rank.
                int lowRank = _low.Descriptor.Progression.Features.GetRank(leaf);
                row["lowRank"] = lowRank;
                if (lowRank != Low)
                    TextFailures.Add(target.Key + ": the two-step bard holds rank " + lowRank);
                var entries = new JArray();
                foreach (UnitEntityData unit in new[] { _control, _low })
                {
                    Feature fact = unit.Descriptor.AddFact(feature) as Feature ??
                        unit.Descriptor.Progression.Features.GetFact(feature) as Feature;
                    int steps = ReferenceEquals(unit, _low) ? Low : 0;
                    int feet = FavoredClassPerformanceManifest.OwnerFeet(target, steps);
                    string nativeFeature = feature.Description;
                    string expectedFeature = FavoredClassPerformanceText.OwnerDescription(nativeFeature, target.BaseFeet,
                        feet);
                    string actualFeature = fact == null ? null : fact.Description;
                    var entry = new JObject { ["owner"] = steps, ["feet"] = feet,
                        ["featureDescription"] = actualFeature };
                    if (actualFeature != expectedFeature)
                        TextFailures.Add(target.Key + "/" + steps + ": feature description differs");
                    var toggles = new JArray();
                    foreach (string toggleGuid in target.ToggleGuids)
                    {
                        ActivatableAbility ability = unit.Descriptor.ActivatableAbilities.Enumerable.FirstOrDefault(value =>
                            value.Blueprint != null && value.Blueprint.AssetGuid == toggleGuid);
                        if (ability == null)
                        {
                            TextFailures.Add(target.Key + ": toggle " + toggleGuid + " was not granted by its feature");
                            continue;
                        }
                        string nativeToggle = ability.Blueprint.Description;
                        string expectedToggle = FavoredClassPerformanceText.OwnerDescription(nativeToggle,
                            target.BaseFeet, feet);
                        var slot = new MechanicActionBarSlotActivableAbility { ActivatableAbility = ability, Unit = unit };
                        string bar = slot.GetDescription();
                        toggles.Add(new JObject { ["toggle"] = ability.Blueprint.name, ["fact"] = ability.Description,
                            ["actionBar"] = bar });
                        if (ability.Description != expectedToggle || bar != expectedToggle)
                            TextFailures.Add(target.Key + "/" + steps + ": toggle " + ability.Blueprint.name +
                                " description differs");
                        if (steps > 0 && expectedToggle == nativeToggle)
                            TextFailures.Add(target.Key + ": the invested text did not change");
                    }
                    entry["toggles"] = toggles;
                    entries.Add(entry);
                    if (fact != null)
                        unit.Descriptor.RemoveFact(fact);
                }
                row["owners"] = entries;
                return row;
            }

            private void ObserveExcluded(FavoredClassPerformanceTarget target)
            {
                var row = new JObject { ["key"] = target.Key, ["exclusion"] = target.Exclusion };
                Excluded.Add(row);
                BlueprintFeature feature = Resolve<BlueprintFeature>(target.FeatureGuid);
                var area = Resolve<BlueprintAbilityAreaEffect>(target.AreaGuids[0]);
                if (feature == null || area == null)
                {
                    row["absent"] = true;
                    if (!target.Provider)
                        ExcludedFailures.Add(target.Key + ": the native performance is missing");
                    return;
                }
                BlueprintFeature leaf = _leaves.Pair(FavoredClassCatalog.EffectPerformanceRange, target.Key).Full;
                GrantFavoredClassRanks(_low, leaf, High);
                AreaEffectEntityData held = Spawn(_low, area);
                GameObject ring = Ring(held);
                Feature fact = _low.Descriptor.AddFact(feature) as Feature ??
                    _low.Descriptor.Progression.Features.GetFact(feature) as Feature;
                row["meters"] = Radius(held);
                row["ringFactor"] = ring == null ? 1f : FavoredClassPerformanceRing.FactorOf(ring);
                row["textNative"] = fact != null && fact.Description == feature.Description;
                if (!Same(Radius(held), area.Size.Meters) || (ring != null && FavoredClassPerformanceRing.FactorOf(ring) != 1f))
                    ExcludedFailures.Add(target.Key + ": an excluded counter widened its area or ring");
                if (!(bool)row["textNative"])
                    ExcludedFailures.Add(target.Key + ": an excluded counter changed the description");
                if (fact != null) _low.Descriptor.RemoveFact(fact);
                RemoveRanks(_low, leaf);
            }

            private void ObserveReclaimed()
            {
                var rows = new JArray();
                foreach (FavoredClassPerformanceTarget target in FavoredClassPerformanceManifest.All.Where(value =>
                    value.Published && value.RingAssetId != null))
                {
                    var area = Resolve<BlueprintAbilityAreaEffect>(target.AreaGuids[0]);
                    Dictionary<string, JObject> native;
                    if (area == null || !_nativeSystems.TryGetValue(target.RingAssetId, out native))
                        continue;
                    AreaEffectEntityData reclaimed = Spawn(_control, area);
                    GameObject ring = Ring(reclaimed);
                    if (ring == null)
                    {
                        ReleaseFailures.Add(target.Key + ": no ring was reclaimed");
                        continue;
                    }
                    Dictionary<string, JObject> systems = Systems(ring);
                    bool exact = FavoredClassPerformanceRing.FactorOf(ring) == 1f && native.All(entry =>
                        systems.ContainsKey(entry.Key) && Floats(systems[entry.Key]["localScale"]).SequenceEqual(
                            Floats(entry.Value["localScale"])));
                    rows.Add(new JObject { ["key"] = target.Key, ["native"] = exact });
                    if (!exact)
                        ReleaseFailures.Add(target.Key + ": a reclaimed ring was not native");
                }
                Release["reclaimed"] = rows;
            }

            private AreaEffectEntityData Spawn(UnitEntityData caster, BlueprintAbilityAreaEffect area)
            {
                AreaEffectEntityData data = AreaEffectsController.SpawnAttachedToTarget(
                    new MechanicsContext(caster, caster.Descriptor, area), area, caster, null);
                _spawned.Add(data);
                return data;
            }

            private void DestroySpawned()
            {
                foreach (AreaEffectEntityData data in _spawned.Where(value => value != null))
                    try
                    {
                        AreaEffectView view = data.View;
                        data.ForceEnd();
                        data.Destroy();
                        // The native destroyer only sweeps the loaded area and the
                        // cross-scene state; fixture units hold their own scene
                        // state, so the view object is destroyed exactly as the
                        // destroyer would (its OnDestroy releases the ring).
                        if (view != null && view.gameObject != null)
                            UnityEngine.Object.Destroy(view.gameObject);
                    }
                    catch (Exception exception)
                    {
                        Diagnostics.Add("destroy: " + exception.GetType().Name + ": " + exception.Message);
                    }
                _spawned.Clear();
                try
                {
                    Game.Instance.EntityDestroyer.Tick();
                    Game.Instance.EntityDestroyer.Tick();
                }
                catch (Exception exception)
                {
                    Diagnostics.Add("destroyer: " + exception.GetType().Name + ": " + exception.Message);
                }
            }

            private void Finish()
            {
                Done = true;
                try { DestroySpawned(); } catch (Exception) { }
                if (_scene != null) _scene.Dispose();
            }

            private T Resolve<T>(string guid) where T : BlueprintScriptableObject
            {
                BlueprintScriptableObject value;
                _library.BlueprintsByAssetId.TryGetValue(guid, out value);
                return value as T;
            }

            private static GameObject Ring(AreaEffectEntityData data)
            {
                AreaEffectView view = data == null ? null : data.View;
                return view == null || SpawnedFx == null ? null : SpawnedFx.GetValue(view) as GameObject;
            }

            private static float Radius(AreaEffectEntityData data)
            {
                var cylinder = data == null || data.View == null ? null : data.View.Shape as ScriptZoneCylinder;
                return cylinder == null ? -1f : cylinder.Radius;
            }

            private static bool Contains(AreaEffectEntityData data, float meters)
            {
                return data.View.Shape.Contains(data.View.transform.position + new Vector3(meters, 0f, 0f), 0f);
            }

            private static bool Same(float left, float right)
            {
                return Math.Abs(left - right) < 0.001f;
            }

            /// <summary>Every particle system of a ring by path, with the scale Unity applies to it.</summary>
            private static Dictionary<string, JObject> Systems(GameObject ring)
            {
                var result = new Dictionary<string, JObject>(StringComparer.Ordinal);
                foreach (Component system in ring.GetComponentsInChildren(ParticleSystemType, true))
                {
                    Transform transform = system.transform;
                    object main = system.GetType().GetProperty("main").GetValue(system, null);
                    int mode = Convert.ToInt32(main.GetType().GetProperty("scalingMode").GetValue(main, null),
                        CultureInfo.InvariantCulture);
                    Vector3 effective = mode == 1 ? transform.localScale : transform.lossyScale;
                    bool[] horizontal = new[] { Vector3.right, Vector3.up, Vector3.forward }
                        .Select(axis => Math.Abs((transform.rotation * axis).y) < 0.7071f).ToArray();
                    object shape = system.GetType().GetProperty("shape").GetValue(system, null);
                    bool enabled = (bool)shape.GetType().GetProperty("enabled").GetValue(shape, null);
                    string type = shape.GetType().GetProperty("shapeType").GetValue(shape, null).ToString();
                    float radius = (float)shape.GetType().GetProperty("radius").GetValue(shape, null);
                    int axisIndex = horizontal[0] ? 0 : 2;
                    string path = PathOf(transform, ring.transform);
                    string key = path;
                    for (int suffix = 1; result.ContainsKey(key); suffix++)
                        key = path + "#" + suffix;
                    result[key] = new JObject
                    {
                        ["mode"] = mode,
                        ["localScale"] = new JArray(transform.localScale.x, transform.localScale.y, transform.localScale.z),
                        ["effectiveScale"] = new JArray(effective.x, effective.y, effective.z),
                        ["horizontalAxes"] = new JArray(horizontal),
                        ["circle"] = enabled && type == "Circle",
                        ["circleMeters"] = radius * Math.Abs(axisIndex == 0 ? effective.x : effective.z),
                    };
                }
                return result;
            }

            private static float[] Floats(JToken token)
            {
                return ((JArray)token).Select(value => (float)value).ToArray();
            }

            private static string PathOf(Transform value, Transform root)
            {
                var parts = new List<string>();
                for (Transform cursor = value; cursor != null; cursor = cursor.parent)
                {
                    parts.Add(cursor.name);
                    if (cursor == root) break;
                }
                parts.Reverse();
                return string.Join("/", parts.ToArray());
            }

            private static void RemoveRanks(UnitEntityData unit, BlueprintFeature leaf)
            {
                var fact = unit.Descriptor.Progression.Features.GetFact(leaf);
                if (fact != null)
                    unit.Descriptor.RemoveFact(fact);
            }

            private static Type FindRuntimeType(string fullName)
            {
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type type = assembly.GetType(fullName, false);
                    if (type != null) return type;
                }
                return null;
            }
        }
    }
}
