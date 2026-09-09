using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static partial class GunslingerOutfitRenderScenario
    {
        // One native ReloadArea, AutoSaveMode.None, with the working-save write
        // sentinels retained. This does not run the larger persistence mutation
        // matrix and never writes the loaded disposable fixture back to disk.
        internal sealed class ElementalCompletionSceneSession : IAreaLoadingStagesHandler,
            IAreaActivationHandler, IGlobalRulebookHandler<RuleSavingThrow>
        {
            private readonly ModContext _context;
            private readonly RuntimeTestRequest _request;
            private readonly WorkingSaveSmokeScenario _smoke;
            private readonly DateTime _started = DateTime.UtcNow;
            private readonly Stopwatch _elapsed = Stopwatch.StartNew();
            private readonly List<RuntimeTestAssertion> _assertions = new List<RuntimeTestAssertion>();
            private readonly JArray _events = new JArray();
            private JObject _before, _after;
            private string _readinessKey;
            private bool _subscribed, _requested;
            private int _scenesLoaded, _loadingComplete, _activated, _settle;
            private readonly List<int> _unloadedScenes = new List<int>();
            private readonly HashSet<AreaEffectEntityData> _observedAreas = new HashSet<AreaEffectEntityData>();
            private readonly List<string> _savingThrows = new List<string>();
            private BlueprintAbilityAreaEffect _earthArea, _nereidArea;
            private BlueprintBuff _earthBuff, _aura, _fascinated, _assistance;
            private BlueprintAbilityResource _resource;
            private static readonly FieldInfo Creation = typeof(AreaEffectEntityData).GetField("m_CreationTime", BindingFlags.Instance | BindingFlags.NonPublic);
            private static readonly FieldInfo Duration = typeof(AreaEffectEntityData).GetField("m_Duration", BindingFlags.Instance | BindingFlags.NonPublic);
            internal bool Complete => Result != null;
            internal RuntimeTestResult Result { get; private set; }
            internal ElementalCompletionSceneSession(ModContext context, RuntimeTestRequest request, WorkingSaveSmokeScenario smoke)
            { _context = context; _request = request; _smoke = smoke; }

            internal void Poll()
            {
                if (Complete) return;
                try {
                    Require(_smoke != null && _smoke.Complete && !_smoke.WriteObserved &&
                        !_smoke.BaselineLoadObserved && !_smoke.OtherLoadObserved && _smoke.ScenarioException == null,
                        "The original correlated native working-save guard must remain active and write-free.");
                    var loading = LoadingProcess.Instance;
                    Require(loading != null && !loading.IsAwaitingUserInput, "Native loading requires unexpected user input.");
                    Require(_elapsed.Elapsed.TotalSeconds < 180, "The native scene round trip timed out.");
                    Game.Instance.IsPaused = true;
                    if (!_requested) {
                        bool scope = RuntimeTestRequestParser.IsCompletionSceneScope(_request) && _request.ExitAfterCompletion &&
                            ElementalAlternateTraitPolicy.NereidQualificationActive;
                        var readiness = new JObject { ["event"] = "initial-native-readiness", ["scope"] = scope,
                            ["qualificationActive"] = ElementalAlternateTraitPolicy.NereidQualificationActive,
                            ["paused"] = Game.Instance.IsPaused, ["loading"] = loading.IsLoadingInProcess,
                            ["screen"] = loading.IsLoadingScreenActive, ["manualScreen"] = loading.IsManualLoadingScreenActive };
                        string key = readiness.ToString(Formatting.None);
                        if (key != _readinessKey) { _events.Add(readiness); _readinessKey = key; }
                        Require(scope, "The exact guarded completion scene scope is required.");
                        // The save callback/fingerprint can finish before the native
                        // loading screen closes. Await its ordinary idle boundary;
                        // do not dismiss the screen or inject input.
                        if (loading.IsLoadingInProcess || loading.IsLoadingScreenActive || loading.IsManualLoadingScreenActive) return;
                        Game.Instance.IsPaused = true;
                        Require(Game.Instance.IsPaused, "The native scene must accept the owned qualification pause.");
                        var set = BlueprintBootstrap.ElementalRaces;
                        var earth = set.Oread.AlternateTraits.Require(ElementalAlternateTraitId.TreacherousEarth);
                        var nereid = set.Undine.AlternateTraits.Require(ElementalAlternateTraitId.NereidFascination);
                        _earthArea = earth.Mechanics().OfType<BlueprintAbilityAreaEffect>().Single();
                        _earthBuff = earth.Mechanics().OfType<BlueprintBuff>().Single();
                        _nereidArea = nereid.Mechanics().OfType<BlueprintAbilityAreaEffect>().Single();
                        _aura = nereid.Mechanics().OfType<BlueprintBuff>().Single(value => value.GetComponent<ElementalNereidAuraState>() != null);
                        _fascinated = nereid.Mechanics().OfType<BlueprintBuff>().Single(value => value.GetComponent<ElementalNereidFascinated>() != null);
                        _assistance = nereid.Mechanics().OfType<BlueprintBuff>().Single(value => value.GetComponent<ElementalNereidAssistance>() != null);
                        _resource = nereid.Mechanics().OfType<BlueprintAbilityResource>().Single();
                        EventBus.Subscribe(this); SceneManager.sceneUnloaded += OnSceneUnloaded; _subscribed = true;
                        _before = Capture("before-reload");
                        Require(_savingThrows.Count == 0, "The qualified saved activation unexpectedly requested another initial save.");
                        _requested = true;
                        _events.Add(new JObject { ["event"] = "native-reload-request", ["autoSaveMode"] = "None" });
                        Game.Instance.ReloadArea();
                        return;
                    }
                    if (loading.IsLoadingInProcess || loading.IsLoadingScreenActive || loading.IsManualLoadingScreenActive ||
                        _loadingComplete == 0 || _scenesLoaded == 0 || _activated == 0) return;
                    if (++_settle < 3) return;
                    _after = Capture("after-reload");
                    var beforeScenes = _before["scenes"].OfType<JObject>().ToArray();
                    var afterScenes = _after["scenes"].OfType<JObject>().ToArray();
                    bool actualReload = beforeScenes.Any(value => _unloadedScenes.Contains((int)value["handle"]) &&
                        afterScenes.Any(after => (string)after["name"] == (string)value["name"] && (int)after["handle"] != (int)value["handle"]));
                    Check("native-scene-unload-and-reload", actualReload && _scenesLoaded == 1 && _loadingComplete == 1 && _activated == 1,
                        "native scene handle replacement and one complete native area-loading event sequence");
                    Check("native-area-and-player-identity", JToken.DeepEquals(_before["area"], _after["area"]) &&
                        JToken.DeepEquals(_before["party"], _after["party"]) && JToken.DeepEquals(_before["positions"], _after["positions"]),
                        "same area, all 24 named native fixture actors and exact positions after scene reconstruction");
                    Check("fixed-area-deadline-owner-position", JToken.DeepEquals(_before["terrain"], _after["terrain"]),
                        "original fixed area ID, caster, position, creation time, duration and exactly two owned terrain conditions");
                    Check("nereid-deadline-responses-and-ownership", JToken.DeepEquals(_before["nereid"], _after["nereid"]),
                        "original aura deadline, caster, terminal responses, source-owned condition and spent daily resources");
                    Check("no-repeated-saving-throws", _savingThrows.Count == 0, "no new saving throws on the native scene round trip");
                    long timeChange = (long)_after["clockTicks"] - (long)_before["clockTicks"];
                    Check("paused-original-lifetimes", timeChange >= 0 && timeChange <= TimeSpan.TicksPerSecond / 4,
                        "paused scene reconstruction does not restart or consume the original lifetime");
                    Finish(string.Empty);
                } catch (Exception exception) { Finish(exception.ToString()); }
            }

            private JObject Capture(string phase)
            {
                var game = Game.Instance;
                var units = game.State.Units.All.ToArray();
                var fixtures = ElementalPersistenceFixtureIds.Select(id => units.Single(value => value.UniqueId == id)).ToArray();
                Require(game.IsPaused && fixtures.All(unit => unit.Descriptor.CustomName != null &&
                    unit.Descriptor.CustomName.StartsWith(ElementalPersistenceFixtureNamePrefix, StringComparison.Ordinal) &&
                    unit.IsInGame && !unit.IsInCombat && !unit.Descriptor.State.IsDead && unit.View != null),
                    "All exact named saved fixture actors must be alive and standing in the actual loaded scene.");
                Require(game.Player.Party.Count == 27 && fixtures.All(unit => game.Player.Party.Contains(unit)),
                    "The qualified native Player party identity changed.");
                SettleOwnedEndedAreas(fixtures, phase);
                var earth = game.State.AreaEffects.All.Single(value => ReferenceEquals(value.Blueprint, _earthArea));
                var nereid = game.State.AreaEffects.All.Single(value => ReferenceEquals(value.Blueprint, _nereidArea));
                _observedAreas.Add(earth); _observedAreas.Add(nereid);
                Require(fixtures.Contains(earth.Context.MaybeCaster) && fixtures.Contains(nereid.Context.MaybeCaster) &&
                    !earth.IsEnded && !nereid.IsEnded && Creation != null && Duration != null,
                    "Exactly one live source-owned area of each kind is required.");
                Synchronize(new[] { fixtures[0], fixtures[1], fixtures[2], fixtures[3], fixtures[23] });
                var update = typeof(AreaEffectEntityData).GetMethod("UpdateUnits", BindingFlags.Instance | BindingFlags.NonPublic);
                Require(update != null, "The audited native area membership method is absent.");
                for (int tick = 0; tick < 2; tick++) foreach (var area in new[] { earth, nereid }) {
                    update.Invoke(area, null); area.Tick(); game.EntityCreator.Tick();
                }
                var caster = nereid.Context.MaybeCaster;
                var aura = caster.Buffs.Enumerable.Single(value => ReferenceEquals(value.Blueprint, _aura));
                var state = aura.SelectComponents<ElementalNereidAuraState>().Single();
                Require(ReferenceEquals(ElementalNereidAuraState.Find(nereid.Context, _aura), state) && aura.Active && aura.TimeLeft > TimeSpan.Zero,
                    "The reconstructed area must resolve the original live aura context.");
                var terrainFacts = units.SelectMany(unit => unit.Buffs.Enumerable.Where(value => ReferenceEquals(value.Blueprint, _earthBuff))
                    .Select(buff => new { unit, buff })).ToArray();
                var conditions = units.SelectMany(unit => unit.Buffs.Enumerable.Where(value => ReferenceEquals(value.Blueprint, _fascinated))
                    .Select(buff => new { unit, buff })).ToArray();
                var helpers = units.SelectMany(unit => unit.Buffs.Enumerable.Where(value => ReferenceEquals(value.Blueprint, _assistance))
                    .Select(buff => new { unit, buff })).ToArray();
                Require(terrainFacts.Length == 2 && terrainFacts.All(value => fixtures.Contains(value.unit) &&
                    value.buff.SourceAreaEffectId == earth.UniqueId && ReferenceEquals(value.buff.Context.MaybeCaster, earth.Context.MaybeCaster) &&
                    value.unit.Descriptor.State.HasCondition(UnitCondition.DifficultTerrain)), "Two exact native terrain conditions must belong to the fixed area.");
                Require(conditions.Length == 1 && conditions[0].unit.UniqueId == ElementalPersistenceFixtureIds[0] &&
                    conditions[0].buff.SourceAreaEffectId == nereid.UniqueId && conditions[0].buff.EndTime == aura.EndTime &&
                    ReferenceEquals(ElementalNereidAuraState.Find(conditions[0].buff.Context, _aura), state) &&
                    conditions[0].unit.Descriptor.State.HasCondition(UnitCondition.Dazed) &&
                    state.Response(fixtures[0]) == ElementalNereidResponse.Affected && state.Response(fixtures[1]) == ElementalNereidResponse.Resisted,
                    "The saved affected/resisted targets and source-owned fascination must remain exact.");
                Require(helpers.Length > 0 && helpers.All(value => value.buff.EndTime == aura.EndTime &&
                    ReferenceEquals(ElementalNereidAuraState.Find(value.buff.Context, _aura), state)), "Every temporary helper must retain its live source and original deadline.");
                var terrain = new JObject { ["areaId"] = earth.UniqueId, ["ownerId"] = earth.Context.MaybeCaster.UniqueId,
                    ["position"] = Point(earth.Position), ["creationTicks"] = ((TimeSpan)Creation.GetValue(earth)).Ticks,
                    ["durationTicks"] = ((TimeSpan)Duration.GetValue(earth)).Ticks,
                    ["affected"] = new JArray(terrainFacts.OrderBy(value => value.unit.UniqueId).Select(value => new JObject {
                        ["id"] = value.unit.UniqueId, ["speed"] = value.unit.CurrentSpeedMps })) };
                var nereidState = new JObject { ["ownerId"] = caster.UniqueId, ["position"] = Point(nereid.Position),
                    ["endTimeTicks"] = aura.EndTime.Ticks, ["dc"] = aura.Context.Params.DC, ["level"] = aura.Context.Params.CasterLevel,
                    ["responses"] = new JArray(fixtures.Select(unit => new JObject { ["id"] = unit.UniqueId, ["response"] = state.Response(unit).ToString() })),
                    ["conditionTarget"] = conditions[0].unit.UniqueId,
                    ["helpers"] = new JArray(helpers.Select(value => value.unit.UniqueId).OrderBy(value => value, StringComparer.Ordinal)),
                    ["resources"] = new JArray(fixtures.Where(unit => unit.Descriptor.Resources.ContainsResource(_resource))
                        .Select(unit => new JObject { ["id"] = unit.UniqueId, ["amount"] = unit.Descriptor.Resources.GetResourceAmount(_resource) })) };
                Require(nereidState["resources"].Count() == 6 && nereidState["resources"].All(value => (int)value["amount"] == 0),
                    "All six saved Undines must retain their spent daily use.");
                _events.Add(new JObject { ["event"] = phase, ["nereidAreaId"] = nereid.UniqueId, ["secondsLeft"] = aura.TimeLeft.TotalSeconds });
                return new JObject { ["clockTicks"] = game.TimeController.GameTime.Ticks, ["area"] = game.CurrentlyLoadedArea.AssetGuid,
                    ["party"] = new JArray(game.Player.Party.Select(unit => unit.UniqueId).OrderBy(value => value, StringComparer.Ordinal)),
                    ["positions"] = new JArray(fixtures.Select(unit => new JObject { ["id"] = unit.UniqueId, ["point"] = Point(unit.Position) })),
                    ["scenes"] = new JArray(Enumerable.Range(0, SceneManager.sceneCount).Select(index => SceneManager.GetSceneAt(index))
                        .Where(scene => scene.isLoaded).Select(scene => new JObject { ["handle"] = scene.handle, ["name"] = scene.name })),
                    ["terrain"] = terrain, ["nereid"] = nereidState };
            }

            private void SettleOwnedEndedAreas(UnitEntityData[] fixtures, string phase)
            {
                var game = Game.Instance;
                var candidates = game.State.AreaEffects.All.Where(area => ReferenceEquals(area.Blueprint, _earthArea) ||
                    ReferenceEquals(area.Blueprint, _nereidArea)).ToArray();
                _events.Add(new JObject { ["event"] = phase + ":native-area-pool", ["areas"] = new JArray(candidates.Select(area =>
                    new JObject { ["id"] = area.UniqueId, ["blueprint"] = area.Blueprint.AssetGuid,
                        ["reference"] = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(area),
                        ["owner"] = area.Context?.MaybeCaster?.UniqueId, ["ended"] = area.IsEnded,
                        ["destroyRequested"] = area.ShouldBeDestroyed, ["destroyed"] = area.Destroyed,
                        ["previouslyObserved"] = _observedAreas.Contains(area),
                        ["sourceAuraResolves"] = ReferenceEquals(area.Blueprint, _nereidArea) &&
                            ElementalNereidAuraState.Find(area.Context, _aura) != null })) });
                var ended = candidates.Where(area => area.IsEnded).ToArray();
                if (ended.Length == 0) return;
                Require(ended.All(area => _observedAreas.Contains(area) && ReferenceEquals(area.Blueprint, _nereidArea) &&
                    fixtures.Contains(area.Context?.MaybeCaster)), "Only the previously observed source-owned ended aura may enter scene cleanup.");
                // Native AddAreaEffect ends its old attached area on unload.
                // Paused qualification must advance the ordinary area/destruction
                // stages after reload, without advancing time or ending live areas.
                foreach (var area in ended) area.Tick();
                var pending = new List<Kingmaker.EntitySystem.EntityDataBase>();
                var controller = typeof(Kingmaker.Controllers.EntityDestructionController);
                var flags = BindingFlags.Static | BindingFlags.NonPublic;
                var listType = typeof(List<Kingmaker.EntitySystem.EntityDataBase>);
                var areaCollector = controller.GetMethod("AddForDestruction", flags, null,
                    new[] { typeof(Kingmaker.EntitySystem.AreaPersistentState), listType }, null);
                var sceneCollector = controller.GetMethod("AddForDestruction", flags, null,
                    new[] { typeof(Kingmaker.EntitySystem.SceneEntitiesState), listType }, null);
                Require(areaCollector != null && sceneCollector != null && game.EntityDestroyer != null,
                    "The audited native destruction boundary is unavailable.");
                areaCollector.Invoke(null, new object[] { game.State.LoadedAreaState, pending });
                sceneCollector.Invoke(null, new object[] { game.Player.CrossSceneState, pending });
                Require(ended.All(area => pending.Contains(area)) && pending.All(value => value is AreaEffectEntityData &&
                    ended.Contains((AreaEffectEntityData)value) && value.ShouldBeDestroyed),
                    "Native scene destruction contains work outside the exact observed ended aura.");
                var units = game.State.Units.All.ToArray();
                var survivors = game.State.AreaEffects.All.Where(area => !ended.Contains(area)).ToArray();
                var clock = game.TimeController.GameTime;
                game.EntityDestroyer.Tick();
                bool exact = ended.All(area => area.Destroyed && !game.State.AreaEffects.All.Contains(area)) &&
                    units.SequenceEqual(game.State.Units.All) && survivors.SequenceEqual(game.State.AreaEffects.All) &&
                    game.TimeController.GameTime == clock;
                _events.Add(new JObject { ["event"] = phase + ":native-ended-area-destruction", ["pendingCount"] = pending.Count,
                    ["endedIds"] = new JArray(ended.Select(area => area.UniqueId)), ["exactForeignPoolsAndClock"] = exact });
                Require(exact, "Native ended-area destruction changed the foreign pools or game clock.");
            }

            private static void Synchronize(UnitEntityData[] actors)
            {
                var game = Game.Instance;
                Require(actors.All(unit => unit.View?.MovementAgent != null && !unit.View.MovementAgent.IsReallyMoving &&
                    unit.Position.Equals(unit.View.transform.position)), "Native zero-time standing reconciliation requires unchanged actor/view positions.");
                var awake = game.State.AwakeUnits.ToArray(); var world = game.State.Units.All.ToArray();
                var positions = world.Select(unit => unit.Position).ToArray();
                var views = world.Select(unit => unit.View?.transform.position).ToArray();
                var previous = world.Select(unit => unit.PreviousPosition).ToArray();
                var foreign = world.Except(actors).ToArray(); var buffs = foreign.Select(unit => unit.Buffs.Enumerable.ToArray()).ToArray();
                float delta = game.TimeController.DeltaTime, gameDelta = game.TimeController.GameDeltaTime;
                var clock = game.TimeController.GameTime;
                try { game.State.AwakeUnits.Clear(); game.State.AwakeUnits.AddRange(actors);
                    game.TimeController.SetDeltaTime(0); game.TimeController.SetGameDeltaTime(0); new UnitMoveController().Tick(); }
                finally { game.State.AwakeUnits.Clear(); game.State.AwakeUnits.AddRange(awake);
                    game.TimeController.SetDeltaTime(delta); game.TimeController.SetGameDeltaTime(gameDelta); }
                Require(game.IsPaused && game.TimeController.GameTime == clock && game.State.Units.All.SequenceEqual(world) &&
                    game.State.AwakeUnits.SequenceEqual(awake) && world.Select((unit, index) => unit.Position.Equals(positions[index]) &&
                        Nullable.Equals(unit.View?.transform.position, views[index]) &&
                        (actors.Contains(unit) || unit.PreviousPosition.Equals(previous[index]))).All(value => value) &&
                    foreign.Select((unit, index) => unit.Buffs.Enumerable.SequenceEqual(buffs[index])).All(value => value),
                    "Native standing reconciliation changed its exact owned boundary.");
            }
            private void Observe(string name) { Game.Instance.IsPaused = true; _events.Add(new JObject { ["event"] = name, ["elapsedMs"] = _elapsed.ElapsedMilliseconds }); }
            public void OnAreaScenesLoaded() { _scenesLoaded++; Observe("area-scenes-loaded"); }
            public void OnAreaLoadingComplete() { _loadingComplete++; Observe("area-loading-complete"); }
            public void OnAreaActivated() { _activated++; Observe("area-activated"); }
            private void OnSceneUnloaded(Scene scene) { _unloadedScenes.Add(scene.handle); Observe("scene-unloaded:" + scene.name); }
            public void OnEventAboutToTrigger(RuleSavingThrow evt) { }
            public void OnEventDidTrigger(RuleSavingThrow evt) { _savingThrows.Add(evt.Initiator.UniqueId); }
            private static JArray Point(Vector3 value) { return new JArray(value.x, value.y, value.z); }
            private static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
            private void Check(string name, bool exact, string expected) { Add(_assertions, "completion-scene-" + name, expected, "exact=" + exact, exact,
                "actual native ReloadArea with original saved effects and live working-save write sentinels"); }
            private void Finish(string failure)
            {
                if (_subscribed) { EventBus.Unsubscribe(this); SceneManager.sceneUnloaded -= OnSceneUnloaded; _subscribed = false; }
                var loaded = _smoke.Stop();
                bool guarded = loaded.CompletionCallbackObserved && loaded.DescriptorReferenceCorrelated &&
                    !string.IsNullOrEmpty(loaded.StableFingerprint) && !loaded.SaveWritingApiObserved && loaded.HooksRemoved;
                Check("write-free-correlated-guard", guarded, "original native working-save correlation and no writes through scene completion");
                string path = Path.Combine(_request.EvidenceDirectory, "elemental-completion-scene-roundtrip.json");
                File.WriteAllText(path, new JObject { ["scope"] = "same-area native scene reload; no material eligibility, area travel or save-write claim",
                    ["before"] = _before, ["after"] = _after, ["events"] = _events,
                    ["savingThrows"] = new JArray(_savingThrows), ["exception"] = failure }.ToString(Formatting.Indented));
                var assembly = _context.Assembly;
                Result = new RuntimeTestResult { SchemaVersion = 1, RunId = _request.RunId, Scenario = _request.Scenario,
                    Status = failure.Length == 0 && _assertions.Count == 7 && _assertions.All(value => value.Status == RuntimeTestStatuses.Pass)
                        ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                    LoadedModVersion = _context.ModEntry.Info.Version,
                    RuntimeIdentity = assembly.FullName + ";mvid=" + assembly.ManifestModule.ModuleVersionId + ";pid=" + Process.GetCurrentProcess().Id,
                    GitCommit = assembly.GetCustomAttributes(typeof(AssemblyMetadataAttribute), false).OfType<AssemblyMetadataAttribute>()
                        .Single(value => value.Key == "GitCommit").Value,
                    GameVersion = Application.version, StartUtc = _started.ToString("o"), EndUtc = string.Empty,
                    DurationMilliseconds = _elapsed.ElapsedMilliseconds, Assertions = _assertions, Diagnostics = new List<string>(), Warnings = new List<string>(),
                    ExceptionSummary = failure, WorkingSaveSmoke = loaded, EvidenceFiles = new List<string> { path },
                    EvidenceDirectory = _request.EvidenceDirectory, AutomaticExitRequested = _request.ExitAfterCompletion };
            }
        }
    }
}
