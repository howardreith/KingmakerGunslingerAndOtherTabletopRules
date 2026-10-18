using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Persistence.Scenes;
using Kingmaker.PubSubSystem;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private int _summoningProfileStage;
        private Stopwatch _summoningProfileClock;
        private CircleProfileSaveGuard _summoningProfileSaveGuard;
        private SummoningProfileSceneObservation _summoningProfileScenes;
        private BlueprintAreaPreset _summoningProfilePreset, _summoningProfileOriginal;
        private BlueprintUnit _summoningProfileActor;
        private MainMenu _summoningProfileMenu;
        private bool _summoningProfileMenuDetached;
        private bool _summoningProfileEntryCallback;
        private RuntimeTestResult _summoningProfileResult;
        private string _summoningProfileFailure;
        private readonly JObject _summoningProfileEvidence = new JObject();

        // Reduced stacks cannot hydrate the established eleven-mod working
        // save. This fixture loads the real first native area, including its
        // navigation/placement graph, then calls the unchanged mechanics case.
        // It is never a save-backed persistence or terrain-travel claim.
        private void PollExpandedSummoningNativeProfile()
        {
            try
            {
                var game = Game.Instance;
                if (_summoningProfileStage == 0)
                {
                    var startup = LoadingProcess.Instance;
                    if (startup.IsLoadingInProcess || startup.IsLoadingScreenActive || startup.IsManualLoadingScreenActive) return;
                    var starters = Resources.FindObjectsOfTypeAll<GameStarter>().Where(value => value != null &&
                        value.gameObject.scene.IsValid() && value.gameObject.scene.isLoaded).ToArray();
                    var progress = starters.Length == 1 ? starters[0].ProgressBar : null;
                    _summoningProfileEvidence["nativeStartupInstances"] = starters.Length;
                    _summoningProfileEvidence["nativeStartupProgressPresent"] = progress != null;
                    _summoningProfileEvidence["nativeStartupProgressActive"] = progress != null && progress.activeSelf;
                    if (!ExpandedSummoningNativeProfilePolicy.StartupComplete(starters.Length,
                        progress != null, progress != null && progress.activeSelf)) return;
                    _summoningProfileMenu = game.UI.MainMenu;
                    // GameStarter's initial scene/input coroutine is independent
                    // of blueprint bootstrap and LoadingProcess readiness.
                    if (_summoningProfileMenu == null || !_summoningProfileMenu.isActiveAndEnabled ||
                        UnityEngine.EventSystems.EventSystem.current == null ||
                        UnityEngine.EventSystems.EventSystem.current.currentInputModule == null) return;
                    if (Runner.LastError != null) throw new InvalidOperationException("Native runner already failed: " + Runner.LastError);
                    var entered = typeof(MainMenu).GetField("m_EnterGameStarted",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    bool idleMenu = _summoningProfileMenu != null && entered != null &&
                        !(bool)entered.GetValue(_summoningProfileMenu) &&
                        EventBus.IsGloballySubscribed(_summoningProfileMenu);
                    string nativeReviewPhase = _request.Scenario == RuntimeTestScenarioCatalog.DisposableExpandedSummoningSprint26NativeReview
                        ? RuntimeTestRequestParser.Sprint26ReviewPhase(_request)
                        : _request.Scenario ==
                            RuntimeTestScenarioCatalog.DisposableExpandedSummoningSprint25NativeReview
                        ? RuntimeTestRequestParser.Sprint25ReviewPhase(_request)
                        : _request.Scenario ==
                            RuntimeTestScenarioCatalog
                                .DisposableExpandedSummoningSprint24NativeReview
                        ? RuntimeTestRequestParser.Sprint24ReviewPhase(_request)
                        : _request.Scenario == RuntimeTestScenarioCatalog
                                .DisposableExpandedSummoningSprint23NativeReview
                            ? RuntimeTestRequestParser.Sprint23ReviewPhase(_request)
                            : RuntimeTestRequestParser.Sprint22ReviewPhase(_request);
                    if (!ExpandedSummoningNativeProfilePolicy.CanStart(
                        _request.ExitAfterCompletion, _request.Scenario,
                        _request.Parameters?.Count ?? 0, nativeReviewPhase,
                        game.State.Units.All.Count(), game.Player.PartyCharacters.Count,
                        game.Player.CrossSceneState.AllEntityData.Count,
                        game.CurrentlyLoadedArea != null || game.State.LoadedAreaState != null,
                        game.Player.MainCharacter.Value != null, Game.ImportSave != null,
                        Game.NewGameUnit != null, idleMenu))
                        throw new InvalidOperationException("Native summoning profile requires a fresh empty main-menu process and only its exact save-free fixture parameters.");
                    _summoningProfileOriginal = BlueprintRoot.Instance.NewGamePreset;
                    if (_summoningProfileOriginal == null || _summoningProfileOriginal.Area == null ||
                        _summoningProfileOriginal.EnterPoint == null)
                        throw new InvalidOperationException("Native first-level preset/area/entry is unavailable.");
                    _summoningProfileSaveGuard = new CircleProfileSaveGuard(_request.RunId);
                    _summoningProfileScenes = new SummoningProfileSceneObservation(_request.RunId);
                    _summoningProfilePreset = UnityEngine.Object.Instantiate(_summoningProfileOriginal);
                    _summoningProfilePreset.name = "KMG_Runtime_Summoning_Native_Profile";
                    _summoningProfilePreset.MakeAutosave = false;
                    // Request-owned fixture setup omits narrative start actions;
                    // production content and all summon commands remain intact.
                    _summoningProfilePreset.StartGameActions = new ActionList();
                    _summoningProfileActor = UnityEngine.Object.Instantiate(
                        _summoningProfileOriginal.PlayerCharacter ?? BlueprintRoot.Instance.DefaultPlayerCharacter);
                    _summoningProfileActor.name = "KMG_Runtime_Summoning_Profile_Anchor";
                    if (_summoningProfileActor.Race == null)
                        _summoningProfileActor.Race = BlueprintRoot.Instance.Progression.CharacterRaces.Single(
                            race => race.AssetGuid == "0a5d473ead98b0646b94495af250fdc4" && race.name == "HumanRace");
                    _summoningProfilePreset.PlayerCharacter = _summoningProfileActor;
                    EventBus.Unsubscribe(_summoningProfileMenu); _summoningProfileMenuDetached = true;
                    _summoningProfileEvidence["areaGuid"] = _summoningProfileOriginal.Area.AssetGuid;
                    _summoningProfileEvidence["entryGuid"] = _summoningProfileOriginal.EnterPoint.AssetGuid;
                    _summoningProfileEvidence["scope"] = "real native first-level scene/navigation; unchanged summon mechanics; no saved-world load/write";
                    _summoningProfileClock = Stopwatch.StartNew();
                    _summoningProfileStage = 3;
                    // Runner invokes this callback immediately before Game.Tick.
                    // Keep native LoadingProcess ownership while waiting for the
                    // additive scene; an idle callback lets Tick see no game mode
                    // after EnterGame has disposed the menu and resets the world.
                    _summoningProfileMenu.EnterGame(() => {
                        _summoningProfileEntryCallback = true;
                        _summoningProfileEvidence["baseEntryCallback"] = true;
                        LoadingProcess.Instance.StartLoadingProcess(AwaitSummoningProfileBaseScene(), () => {
                            _summoningProfileStage = 1;
                            _summoningProfileClock.Restart();
                            game.LoadNewGame(_summoningProfilePreset, null);
                            WriteLifecycleStage("summoning-profile-native-area-load-start");
                        }, LoadingProcessTag.None, 0f);
                    });
                    WriteLifecycleStage("summoning-profile-native-base-entry-start");
                    return;
                }
                if (_summoningProfileStage == 3)
                {
                    var baseScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(SceneName.BaseMechanics);
                    bool registered = baseScene.IsValid() && baseScene.isLoaded;
                    _summoningProfileEvidence["baseEntryCallback"] = _summoningProfileEntryCallback;
                    _summoningProfileEvidence["baseRegisteredBeforeWorldLoad"] = registered;
                    _summoningProfileEvidence["baseSceneName"] = SceneName.BaseMechanics;
                    _summoningProfileEvidence["loadedScenes"] = new JArray(Enumerable.Range(0,
                        UnityEngine.SceneManagement.SceneManager.sceneCount).Select(index => {
                            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(index);
                            return scene.name + ":" + scene.isLoaded;
                        }));
                    if (!_summoningProfileEntryCallback || !registered)
                    {
                        if (_summoningProfileClock.Elapsed.TotalSeconds >= _request.TimeoutSeconds)
                            throw new TimeoutException("Native base scene did not register after menu entry.");
                        return;
                    }
                    return;
                }
                if (_summoningProfileStage == 1)
                {
                    var loading = LoadingProcess.Instance;
                    if (loading.IsLoadingInProcess || loading.IsLoadingScreenActive || loading.IsManualLoadingScreenActive)
                    {
                        if (_summoningProfileClock.Elapsed.TotalSeconds >= _request.TimeoutSeconds)
                            throw new TimeoutException("Native first-level load did not complete.");
                        return;
                    }
                    var baseScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(SceneName.BaseMechanics);
                    bool baseLoaded = baseScene.IsValid() && baseScene.isLoaded;
                    bool exactArea = game.CurrentlyLoadedArea == _summoningProfileOriginal.Area;
                    bool loadedState = game.State.LoadedAreaState != null;
                    bool mainPresent = game.Player.MainCharacter.Value != null;
                    bool partyState = game.Player.Party.Any(unit => unit != null && unit.HoldingState != null);
                    _summoningProfileEvidence["baseMechanicsLoaded"] = baseLoaded;
                    _summoningProfileEvidence["expectedArea"] = _summoningProfileOriginal.Area.AssetGuid;
                    _summoningProfileEvidence["actualArea"] = game.CurrentlyLoadedArea?.AssetGuid;
                    _summoningProfileEvidence["loadedAreaState"] = loadedState;
                    _summoningProfileEvidence["mainCharacterUid"] = game.Player.MainCharacter.Value?.UniqueId;
                    _summoningProfileEvidence["partyWithHoldingState"] = partyState;
                    if (!ExpandedSummoningNativeProfilePolicy.ReadyForMechanics(baseLoaded, exactArea, loadedState, mainPresent, partyState))
                        throw new InvalidOperationException("Native first-level world/party was not registered exactly.");
                    _summoningProfileEvidence["nativeAreaLoaded"] = true;
                    _summoningProfileEvidence["nativeMode"] = game.CurrentMode.ToString();
                    _summoningProfileEvidence["partyCount"] = game.Player.Party.Count;
                    game.IsPaused = true;
                    WriteLifecycleStage("summoning-profile-native-mechanics-start");
                    if (_request.Scenario == RuntimeTestScenarioCatalog
                            .DisposableExpandedSummoningSprint22NativeReview ||
                        _request.Scenario == RuntimeTestScenarioCatalog
                            .DisposableExpandedSummoningSprint23NativeReview ||
                        _request.Scenario == RuntimeTestScenarioCatalog
                            .DisposableExpandedSummoningSprint24NativeReview ||
                        _request.Scenario == RuntimeTestScenarioCatalog
                            .DisposableExpandedSummoningSprint25NativeReview ||
                        _request.Scenario == RuntimeTestScenarioCatalog.DisposableExpandedSummoningSprint26NativeReview)
                    { _summoningProfileStage=4; return; }
                    _summoningProfileResult = _request.Scenario == RuntimeTestScenarioCatalog
                            .DisposableAlternateSummoningPlayerPaths
                        ? RunAlternateSummoningPlayerPaths()
                        : RunDisposableExpandedSummoning(false);
                    BeginSummoningProfileReset();
                    return;
                }
                if (_summoningProfileStage == 4)
                {
                    if (_summoningProfileClock.Elapsed.TotalSeconds >= _request.TimeoutSeconds)
                        throw new TimeoutException("Expanded Summoning native review exceeded its declared scenario deadline.");
                    bool complete = _request.Scenario == RuntimeTestScenarioCatalog.DisposableExpandedSummoningSprint26NativeReview
                        ? PollSprint25NativeReview()
                        : _request.Scenario == RuntimeTestScenarioCatalog
                            .DisposableExpandedSummoningSprint25NativeReview
                        ? PollSprint25NativeReview()
                        : _request.Scenario == RuntimeTestScenarioCatalog
                            .DisposableExpandedSummoningSprint24NativeReview
                        ? PollSprint24NativeReview()
                        : _request.Scenario == RuntimeTestScenarioCatalog
                                .DisposableExpandedSummoningSprint23NativeReview
                            ? PollSprint23NativeReview()
                            : PollSprint22NativeReview();
                    if (complete) BeginSummoningProfileReset();
                    return;
                }
                if (_summoningProfileStage == 2)
                {
                    bool idle = !LoadingProcess.Instance.IsLoadingInProcess &&
                        game.CurrentlyLoadedArea == null && game.State.LoadedAreaState == null &&
                        !game.State.Units.All.Any() && game.Player.PartyCharacters.Count == 0 &&
                        !game.Player.CrossSceneState.AllEntityData.Any() && game.Player.MainCharacter.Value == null &&
                        game.UI.MainMenu != null && EventBus.IsGloballySubscribed(game.UI.MainMenu);
                    if (!idle && _summoningProfileClock.Elapsed.TotalSeconds < _request.TimeoutSeconds) return;
                    FinishSummoningNativeProfile(idle);
                }
            }
            catch (Exception exception)
            {
                _summoningProfileFailure = (_summoningProfileFailure ?? "") + "\n" + exception;
                if (_summoningProfileStage == 1 || _summoningProfileStage == 3 || _summoningProfileStage == 4)
                {
                    try
                    {
                        if (_request.Scenario == RuntimeTestScenarioCatalog
                                .DisposableExpandedSummoningSprint25NativeReview ||
                            _request.Scenario == RuntimeTestScenarioCatalog.DisposableExpandedSummoningSprint26NativeReview)
                            CleanupSprint25NativeReview();
                        else if (_request.Scenario == RuntimeTestScenarioCatalog
                                .DisposableExpandedSummoningSprint24NativeReview)
                            CleanupSprint24NativeReview();
                        else if (_request.Scenario == RuntimeTestScenarioCatalog
                                .DisposableExpandedSummoningSprint23NativeReview)
                            CleanupSprint23NativeReview();
                        else CleanupSprint22NativeReview();
                        BeginSummoningProfileReset();
                        return;
                    }
                    catch (Exception resetFailure) { _summoningProfileFailure += "\nreset: " + resetFailure; }
                }
                FinishSummoningNativeProfile(false);
            }
        }

        private IEnumerator<object> AwaitSummoningProfileBaseScene()
        {
            while (true)
            {
                var scene = SceneManager.GetSceneByName(SceneName.BaseMechanics);
                bool ready = ExpandedSummoningNativeProfilePolicy.ReadyForWorldLoad(
                    _summoningProfileEntryCallback, scene.IsValid() && scene.isLoaded,
                    LoadingProcess.Instance.IsLoadingInProcess);
                _summoningProfileEvidence["baseRegisteredBeforeWorldLoad"] = scene.IsValid() && scene.isLoaded;
                _summoningProfileEvidence["baseSceneHandleBeforeWorldLoad"] = scene.handle;
                _summoningProfileEvidence["nativeLoadingOwnsEntry"] = LoadingProcess.Instance.IsLoadingInProcess;
                if (ready) yield break;
                if (_summoningProfileClock.Elapsed.TotalSeconds >= _request.TimeoutSeconds)
                    throw new TimeoutException("Native base scene/entry loader ownership was not established.");
                yield return null;
            }
        }

        private void BeginSummoningProfileReset()
        {
            _summoningProfileStage = 2;
            _summoningProfileClock.Restart();
            // Native teardown owns the entire fresh, unsaved first-level world.
            // The mechanics case already retires its exact units/resources.
            Game.Instance.ResetToMainMenu();
            WriteLifecycleStage("summoning-profile-native-reset-start");
        }

        private void FinishSummoningNativeProfile(bool emptyWorldRestored)
        {
            if (_summoningProfileResult == null)
                _summoningProfileResult = CreateResult(RuntimeTestStatuses.Fail, null, _summoningProfileFailure);
            try
            {
                if (_summoningProfileMenuDetached && ReferenceEquals(Game.Instance.UI.MainMenu, _summoningProfileMenu) &&
                    !EventBus.IsGloballySubscribed(_summoningProfileMenu)) EventBus.Subscribe(_summoningProfileMenu);
                _summoningProfileMenuDetached = false;
                if (_summoningProfileActor != null) UnityEngine.Object.Destroy(_summoningProfileActor);
                if (_summoningProfilePreset != null) UnityEngine.Object.Destroy(_summoningProfilePreset);
            }
            finally
            {
                _summoningProfileScenes?.Dispose();
                _summoningProfileSaveGuard?.Dispose();
            }
            bool guarded = _summoningProfileSaveGuard != null && _summoningProfileSaveGuard.Removed &&
                _summoningProfileSaveGuard.Blocked == 0;
            bool sceneObserverRemoved = _summoningProfileScenes != null && _summoningProfileScenes.Removed;
            _summoningProfileResult.Assertions.Add(Assertion("expanded-summoning-native-profile-restored",
                "native empty world restored; no save load/write/delete attempted; guards released",
                "emptyWorld=" + emptyWorldRestored + ";blocked=" + _summoningProfileSaveGuard?.Blocked +
                    ";guardRemoved=" + _summoningProfileSaveGuard?.Removed,
                emptyWorldRestored && guarded && sceneObserverRemoved, "native LoadNewGame/ResetToMainMenu with request-owned preset and independent save/load guard"));
            if (_summoningProfileFailure != null)
                _summoningProfileResult.Assertions.Add(Assertion("expanded-summoning-native-profile-execution",
                    "no fixture exception", "failure", false, _summoningProfileFailure));
            _summoningProfileEvidence["emptyWorldRestored"] = emptyWorldRestored;
            _summoningProfileEvidence["blockedSaveOrLoadAttempts"] = _summoningProfileSaveGuard?.Blocked;
            _summoningProfileEvidence["guardRemoved"] = _summoningProfileSaveGuard?.Removed;
            _summoningProfileEvidence["sceneObserverRemoved"] = sceneObserverRemoved;
            _summoningProfileEvidence["sceneHistory"] = _summoningProfileScenes?.Events;
            _summoningProfileEvidence["exception"] = _summoningProfileFailure;
            string report = Path.Combine(_request.EvidenceDirectory, "expanded-summoning-native-profile.json");
            File.WriteAllText(report, BoundaryObservationPolicy.Serialize(_summoningProfileEvidence));
            _summoningProfileResult.EvidenceFiles.Add(report);
            _summoningProfileResult.Status = _summoningProfileResult.Assertions.All(row => row.Status == RuntimeTestStatuses.Pass)
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail;
            Complete(_summoningProfileResult);
        }

        // Read-only, request-local diagnostics. Record the native caller and
        // actual scene handles without changing any load/unload decision.
        private sealed class SummoningProfileSceneObservation : IDisposable
        {
            private static SummoningProfileSceneObservation _active;
            private readonly HarmonyInstance _harmony;
            internal readonly JArray Events = new JArray();
            internal bool Removed;
            internal SummoningProfileSceneObservation(string runId)
            {
                if (_active != null) throw new InvalidOperationException("Native scene observer already owned.");
                _harmony = HarmonyInstance.Create("kmg.summoning.profile.scene." + runId);
                _active = this;
                try
                {
                    foreach (string method in new[] { "LoadScene", "LoadSceneAsync", "UnloadSceneAsync" })
                        _harmony.Patch(typeof(BundledSceneLoader).GetMethod(method, BindingFlags.Public | BindingFlags.Static),
                            new HarmonyMethod(typeof(SummoningProfileSceneObservation).GetMethod(nameof(LoaderCall), BindingFlags.NonPublic | BindingFlags.Static)), null, null);
                    _harmony.Patch(typeof(Game).GetMethod("ResetToMainMenu", BindingFlags.Public | BindingFlags.Instance),
                        new HarmonyMethod(typeof(SummoningProfileSceneObservation).GetMethod(nameof(ResetCall), BindingFlags.NonPublic | BindingFlags.Static)), null, null);
                    SceneManager.sceneLoaded += Loaded;
                    SceneManager.sceneUnloaded += Unloaded;
                    Record("observer-start", null, null);
                }
                catch { Dispose(); throw; }
            }
            private static void LoaderCall(string sceneName, MethodBase __originalMethod)
            {
                _active?.Record(__originalMethod.Name, sceneName, Environment.StackTrace);
            }
            private static void ResetCall(string message)
            {
                _active?.Record("ResetToMainMenu", message, Environment.StackTrace);
            }
            private void Loaded(Scene scene, LoadSceneMode mode) { Record("scene-loaded:" + mode, scene.name, null, scene.handle); }
            private void Unloaded(Scene scene) { Record("scene-unloaded", scene.name, null, scene.handle); }
            private void Record(string name, string target, string caller, int? handle = null)
            {
                Events.Add(new JObject {
                    ["event"] = name, ["target"] = target, ["handle"] = handle,
                    ["frame"] = Time.frameCount, ["caller"] = caller, ["nativeError"] = Runner.LastError?.ToString(),
                    ["scenes"] = new JArray(Enumerable.Range(0, SceneManager.sceneCount).Select(index => {
                        var scene = SceneManager.GetSceneAt(index);
                        return new JObject { ["name"] = scene.name, ["handle"] = scene.handle,
                            ["valid"] = scene.IsValid(), ["loaded"] = scene.isLoaded };
                    }))
                });
            }
            public void Dispose()
            {
                if (Removed) return;
                SceneManager.sceneLoaded -= Loaded;
                SceneManager.sceneUnloaded -= Unloaded;
                _harmony.UnpatchAll(_harmony.Id);
                if (ReferenceEquals(_active, this)) _active = null;
                Removed = true;
            }
        }
    }
}
