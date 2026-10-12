using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.PubSubSystem;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private int _summoningProfileStage;
        private Stopwatch _summoningProfileClock;
        private CircleProfileSaveGuard _summoningProfileSaveGuard;
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
                    _summoningProfileMenu = game.UI.MainMenu;
                    // GameStarter's initial scene/input coroutine is independent
                    // of blueprint bootstrap and LoadingProcess readiness.
                    if (_summoningProfileMenu == null || !_summoningProfileMenu.isActiveAndEnabled ||
                        UnityEngine.EventSystems.EventSystem.current == null ||
                        UnityEngine.EventSystems.EventSystem.current.currentInputModule == null) return;
                    var entered = typeof(MainMenu).GetField("m_EnterGameStarted",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    bool idleMenu = _summoningProfileMenu != null && entered != null &&
                        !(bool)entered.GetValue(_summoningProfileMenu) &&
                        EventBus.IsGloballySubscribed(_summoningProfileMenu);
                    if (!ExpandedSummoningNativeProfilePolicy.CanStart(
                        _request.ExitAfterCompletion, _request.Parameters?.Count ?? 0,
                        game.State.Units.All.Count(), game.Player.PartyCharacters.Count,
                        game.Player.CrossSceneState.AllEntityData.Count,
                        game.CurrentlyLoadedArea != null || game.State.LoadedAreaState != null,
                        game.Player.MainCharacter.Value != null, Game.ImportSave != null,
                        Game.NewGameUnit != null, idleMenu))
                        throw new InvalidOperationException("Native summoning profile requires a fresh empty main-menu process and no save/import parameters.");
                    _summoningProfileOriginal = BlueprintRoot.Instance.NewGamePreset;
                    if (_summoningProfileOriginal == null || _summoningProfileOriginal.Area == null ||
                        _summoningProfileOriginal.EnterPoint == null)
                        throw new InvalidOperationException("Native first-level preset/area/entry is unavailable.");
                    _summoningProfileSaveGuard = new CircleProfileSaveGuard(_request.RunId);
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
                    // EnterGame queues native scene loading. Its callback is not
                    // proof that the additive base scene is registered yet.
                    _summoningProfileMenu.EnterGame(() => _summoningProfileEntryCallback = true);
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
                    _summoningProfileStage = 1;
                    _summoningProfileClock.Restart();
                    game.LoadNewGame(_summoningProfilePreset, null);
                    WriteLifecycleStage("summoning-profile-native-area-load-start");
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
                    _summoningProfileResult = RunDisposableExpandedSummoning(false);
                    BeginSummoningProfileReset();
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
                if (_summoningProfileStage == 1 || _summoningProfileStage == 3)
                {
                    try { BeginSummoningProfileReset(); return; }
                    catch (Exception resetFailure) { _summoningProfileFailure += "\nreset: " + resetFailure; }
                }
                FinishSummoningNativeProfile(false);
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
            finally { _summoningProfileSaveGuard?.Dispose(); }
            bool guarded = _summoningProfileSaveGuard != null && _summoningProfileSaveGuard.Removed &&
                _summoningProfileSaveGuard.Blocked == 0;
            _summoningProfileResult.Assertions.Add(Assertion("expanded-summoning-native-profile-restored",
                "native empty world restored; no save load/write/delete attempted; guards released",
                "emptyWorld=" + emptyWorldRestored + ";blocked=" + _summoningProfileSaveGuard?.Blocked +
                    ";guardRemoved=" + _summoningProfileSaveGuard?.Removed,
                emptyWorldRestored && guarded, "native LoadNewGame/ResetToMainMenu with request-owned preset and independent save/load guard"));
            if (_summoningProfileFailure != null)
                _summoningProfileResult.Assertions.Add(Assertion("expanded-summoning-native-profile-execution",
                    "no fixture exception", "failure", false, _summoningProfileFailure));
            _summoningProfileEvidence["emptyWorldRestored"] = emptyWorldRestored;
            _summoningProfileEvidence["blockedSaveOrLoadAttempts"] = _summoningProfileSaveGuard?.Blocked;
            _summoningProfileEvidence["guardRemoved"] = _summoningProfileSaveGuard?.Removed;
            _summoningProfileEvidence["exception"] = _summoningProfileFailure;
            string report = Path.Combine(_request.EvidenceDirectory, "expanded-summoning-native-profile.json");
            File.WriteAllText(report, BoundaryObservationPolicy.Serialize(_summoningProfileEvidence));
            _summoningProfileResult.EvidenceFiles.Add(report);
            _summoningProfileResult.Status = _summoningProfileResult.Assertions.All(row => row.Status == RuntimeTestStatuses.Pass)
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail;
            Complete(_summoningProfileResult);
        }
    }
}
