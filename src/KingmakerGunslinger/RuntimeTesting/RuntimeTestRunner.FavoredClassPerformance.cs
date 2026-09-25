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
                "an injected ring failure on a real invested bard's area (a scaler that scales then throws, and one that scales nothing) leaves that instance at its native radius with its ring restored exactly; with the scaler restored the same bard's next instance widens its cylinder and ring together",
                Describe(probe.Injected, probe.InjectedFailures), probe.InjectedFailures.Count == 0,
                "FavoredClassPerformanceRangePatch.RingScalerOverride (guarded qualification seam); spawned instance radius and ring systems"));
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
                InjectedFailures = new List<string>();
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
            internal List<string> InjectedFailures { get; private set; }

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
                        ObserveInjectedFailure();
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

            // Review finding 4: an injected ring failure on a real instance
            // restores its radius and ring; the healthy path still widens both.
            private void ObserveInjectedFailure()
            {
                FavoredClassPerformanceTarget target = FavoredClassPerformanceManifest.For("InspireCourage");
                BlueprintFeature leaf = _leaves.Pair(FavoredClassCatalog.EffectPerformanceRange, target.Key).Full;
                var area = Resolve<BlueprintAbilityAreaEffect>(target.AreaGuids[0]);
                Dictionary<string, JObject> native;
                if (area == null || !_nativeSystems.TryGetValue(target.RingAssetId, out native))
                {
                    InjectedFailures.Add("Inspire Courage's area or native ring systems were not observed");
                    return;
                }
                float nativeRadius = area.Size.Meters;
                float widened = FavoredClassMechanicsPolicy.PerformanceRadiusMeters(nativeRadius, Low);
                // Exactly the native local scales the uninvested bard's ring showed.
                Func<GameObject, bool> nativeRing = ring =>
                {
                    if (ring == null || FavoredClassPerformanceRing.IsScaled(ring))
                        return false;
                    Dictionary<string, JObject> systems = Systems(ring);
                    return native.All(entry => systems.ContainsKey(entry.Key) &&
                        Floats(systems[entry.Key]["localScale"]).SequenceEqual(Floats(entry.Value["localScale"])));
                };
                GrantFavoredClassRanks(_low, leaf, Low);
                try
                {
                    foreach (string fault in new[] { "scaled-then-threw", "scaled-nothing" })
                    {
                        FavoredClassPerformanceRangePatch.RingScalerOverride = (effect, factor) =>
                        {
                            if (fault == "scaled-nothing")
                                return 0;
                            FavoredClassPerformanceRing.Scale(effect, factor);
                            throw new InvalidOperationException("injected ring failure after scaling");
                        };
                        AreaEffectEntityData instance;
                        try
                        {
                            instance = Spawn(_low, area);
                        }
                        finally
                        {
                            FavoredClassPerformanceRangePatch.RingScalerOverride = null;
                        }
                        GameObject ring = Ring(instance);
                        bool restored = nativeRing(ring);
                        Injected[fault] = new JObject
                        {
                            ["radius"] = Radius(instance),
                            ["nativeRadius"] = nativeRadius,
                            ["ringPresent"] = ring != null,
                            ["ringFactor"] = ring == null ? 1f : FavoredClassPerformanceRing.FactorOf(ring),
                            ["ringNative"] = restored
                        };
                        if (!Same(Radius(instance), nativeRadius))
                            InjectedFailures.Add(fault + ": the cylinder stayed widened after the ring failed");
                        if (!restored)
                            InjectedFailures.Add(fault + ": the ring was not restored to its native transforms");
                    }
                    AreaEffectEntityData healthy = Spawn(_low, area);
                    GameObject healthyRing = Ring(healthy);
                    Injected["healthy"] = new JObject
                    {
                        ["radius"] = Radius(healthy),
                        ["factor"] = healthyRing == null ? 1f : FavoredClassPerformanceRing.FactorOf(healthyRing)
                    };
                    if (healthyRing != null)
                        _scaledRings.Add(healthyRing);
                    if (!Same(Radius(healthy), widened) || healthyRing == null ||
                        !FavoredClassPerformanceRing.IsScaled(healthyRing))
                        InjectedFailures.Add("with the scaler restored the instance did not widen with its ring");
                }
                finally
                {
                    FavoredClassPerformanceRangePatch.RingScalerOverride = null;
                    RemoveRanks(_low, leaf);
                }
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
