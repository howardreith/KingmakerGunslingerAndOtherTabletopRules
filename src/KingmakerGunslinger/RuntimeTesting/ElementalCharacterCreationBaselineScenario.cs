using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Harmony12;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Assets.UI.LevelUp;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UI.LevelUp;
using Kingmaker.UI.LevelUp.Phase;
using Kingmaker.UI.ServiceWindow;
using Kingmaker.Visual.CharacterSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.AidAnotherCompatibility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Operates only request-created ChargenUnits through the real native creator.
    // A diagnostic observation PASS never overrides the per-character acceptance result.
    internal sealed partial class ElementalCharacterCreationBaselineScenario
    {
        internal const string EvidenceFileName = "elemental-character-creation-baseline.json";
        private const string FirstTrait = "34e2812e0f8241bb9e1bee5240c9eb2e";
        private const string SecondTrait = "5253dcee502a49249bdd8bfdfe525e9f";
        private static readonly BindingFlags Members = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        private static ElementalCharacterCreationBaselineScenario _saveGuardOwner;
        private HarmonyInstance _saveGuard;
        private string _saveGuardId;
        private readonly List<MethodBase> _saveMethods = new List<MethodBase>();
        private readonly List<MethodBase> _observedAssetMethods = new List<MethodBase>();
        private readonly JArray _assetUnloads = new JArray();
        private readonly JArray _compatibilityRechecks = new JArray();
        private readonly List<KeyValuePair<UnityEngine.Object, JObject>> _initialInnerAssets =
            new List<KeyValuePair<UnityEngine.Object, JObject>>();
        private readonly ModContext _context;
        private readonly RuntimeTestRequest _request;
        private readonly WorkingSaveSmokeEvidence _loaded;
        private readonly bool _disabledControl;
        private readonly bool _canCommit;
        private readonly bool _useRoll;
        private readonly string _classGuid;
        private bool _rollRequested;
        private bool _rollApplied;
        private UnitEntityData _mainBefore;
        private Kingmaker.Blueprints.Area.BlueprintArea _areaBefore;
        private readonly Stopwatch _elapsed = Stopwatch.StartNew();
        private readonly JArray _characters = new JArray();
        private readonly List<string> _failures = new List<string>();
        private readonly BlueprintRace[] _races;
        private CharacterBuildController _build;
        private LevelUpController _controller;
        private LevelUpController _globalControllerBefore;
        private UnitDescriptor _buildUnitBefore;
        private JObject _initialCreatorOwnership;
        private int _readinessWait;
        private readonly List<StatType> _spentSkills = new List<StatType>();
        private UnitEntityData _unit;
        private UnitEntityData[] _worldBefore;
        private JObject _character;
        private int _raceIndex;
        private int _settle;
        private int _operations;
        private bool _classChosen;
        private bool _advancePending;
        private int _viewWait;
        private bool _committed;
        private bool _successCallback;
        private bool _traitsCaptured;
        private bool _started;
        private string _stage = "ready";
        private string _lastCaptureKey;

        internal ElementalCharacterCreationBaselineScenario(ModContext context, RuntimeTestRequest request,
            WorkingSaveSmokeEvidence loaded = null)
        {
            if (request == null || (request.Scenario != RuntimeTestScenarioCatalog.DisposableElementalCharacterCreationBaseline &&
                request.Scenario != RuntimeTestScenarioCatalog.DisposableElementalCharacterCreationCase &&
                request.Scenario != RuntimeTestScenarioCatalog.DisposableGlobalTraitsKmgDisabledControl &&
                request.Scenario != RuntimeTestScenarioCatalog.WorkingSaveElementalCharacterCreation &&
                request.Scenario != RuntimeTestScenarioCatalog.WorkingSaveElementalCharacterCreationRegression))
                throw new InvalidOperationException("Exact guarded creator-baseline request required.");
            _disabledControl = request.Scenario == RuntimeTestScenarioCatalog.DisposableGlobalTraitsKmgDisabledControl;
            _regression = request.Scenario == RuntimeTestScenarioCatalog.WorkingSaveElementalCharacterCreationRegression;
            _canCommit = _regression || request.Scenario == RuntimeTestScenarioCatalog.WorkingSaveElementalCharacterCreation;
            if (_canCommit && (request.Parameters == null || (string)request.Parameters["saveName"] != WorkingSaveSmokeScenario.ExpectedName ||
                loaded == null || !loaded.CompletionCallbackObserved || !loaded.DescriptorReferenceCorrelated ||
                string.IsNullOrEmpty(loaded.StableFingerprint) || loaded.SaveWritingApiObserved || !loaded.HooksRemoved))
                throw new InvalidOperationException("Qualified exact working-save load must precede character creation.");
            _context = context;
            _request = request;
            _loaded = loaded;
            if (!_disabledControl && (!context.FeatureModules.Active.ElementalRaces || BlueprintBootstrap.ElementalRaces == null))
                throw new InvalidOperationException("Elemental race prerequisites unavailable.");
            _races = _disabledControl ? new[] { BlueprintRoot.Instance.Progression.CharacterRaces.Single(race =>
                race.AssetGuid == "0a5d473ead98b0646b94495af250fdc4" && race.name == "HumanRace") }
                : BlueprintBootstrap.ElementalRaces.OrderedRaces().OrderBy(race =>
                    ReferenceEquals(race, BlueprintBootstrap.ElementalRaces.Sylph.Race) ? 1 : 0).ToArray();
            _classGuid = "48ac8db94d5de7645906c7d0ad3bcfbd";
            if (_regression || request.Scenario == RuntimeTestScenarioCatalog.DisposableElementalCharacterCreationCase)
            {
                _races = new[] { BlueprintBootstrap.ElementalRaces.OrderedBlueprints().Single(value =>
                    value.Definition.Kind.ToString() == (string)request.Parameters["race"]).Race };
                _useRoll = (string)request.Parameters["allocation"] == "roll";
                if ((string)request.Parameters["class"] == "Gunslinger")
                    _classGuid = "abca4797366d4df0831a418eee39069a";
                if (_regression) _races = Enumerable.Repeat(_races[0], 3).ToArray();
            }
        }

        internal void Abort(string reason) { if (!Complete) { _failures.Add(reason); Finish(); } }

        internal bool Complete { get; private set; }
        internal RuntimeTestResult Result { get; private set; }

        internal void Poll()
        {
            if (Complete) return;
            try
            {
                if (_elapsed.Elapsed.TotalSeconds > _request.TimeoutSeconds - 10)
                    throw new TimeoutException("Creator baseline timed out at " + _stage);
                if (_settle-- > 0) return;
                if (_started) VerifyVisualIntegrity();
                if (!_started) { Start(); return; }
                if (_commitCleanupPending) { PollCommittedCreatorCleanup(); return; }
                if (_controller == null)
                {
                    if (_raceIndex == _races.Length) { Finish(); return; }
                    BeginCharacter(); return;
                }
                if (!ReferenceEquals(_controller, _build.LevelUpController) ||
                    !ReferenceEquals(_controller, Game.Instance.UI.LevelUpController) ||
                    !ReferenceEquals(_controller.Unit, _unit.Descriptor))
                    throw new InvalidOperationException("Actual creator lost request-local controller ownership.");
                if (_racialCheckPending)
                { _racialCheckPending = false; VerifySelectedRacialGraph("after-native-choice"); }
                if (_regression && _revising) { DriveRacialRevision(); return; }
                if (_advancePending)
                {
                    _advancePending = false;
                    typeof(CharacterBuildController).GetMethod("SetupButton", Members).Invoke(_build, null);
                    if (!NextEnabled()) { RejectCharacter("native Next/Complete button is disabled at " + _stage); return; }
                    _build.ToNextPhase(); _settle = 12; return;
                }
                if (++_operations > (_regression ? 700 : 240)) { RejectCharacter("native creator did not converge within its operation budget"); return; }
                CharBPhase.Type? phase = _build.CurrentPhase;
                if (!phase.HasValue) throw new InvalidOperationException("Native creator has no active phase.");
                _stage = _races[_raceIndex].name + ":" + phase.Value;
                string captureKey = _stage + "|" + string.Join(",", _controller.State.Selections
                    .Select(value => value.SelectedItem?.Feature?.AssetGuid ?? "-"));
                if (captureKey != _lastCaptureKey) { Capture(_stage); _lastCaptureKey = captureKey; }
                switch (phase.Value)
                {
                    case CharBPhase.Type.Portrait:
                        _build.SetPortrait(BlueprintRoot.Instance.CharGen.Portraits.First(value => value != null));
                        Advance(); break;
                    case CharBPhase.Type.Race:
                        _build.SetRace(_races[_raceIndex]);
                        _build.SetGender(Kingmaker.Blueprints.Gender.Male);
                        RecordDollRamps("race-selected");
                        Advance(); break;
                    case CharBPhase.Type.Class:
                    case CharBPhase.Type.ClassInChargen:
                        if (!_classChosen)
                        {
                            _build.SetClass(BlueprintRoot.Instance.Progression.CharacterClasses.Single(value =>
                                value.AssetGuid == _classGuid));
                            _classChosen = true;
                            _settle = 10;
                        }
                        else { ObserveRouting(); Advance(); }
                        break;
                    case CharBPhase.Type.Determinator:
                    case CharBPhase.Type.Abilities:
                        SelectCurrentFeature(phase.Value == CharBPhase.Type.Determinator ? _build.Determinators : _build.Abilities);
                        break;
                    case CharBPhase.Type.Skills:
                        Allocate(); break;
                    case CharBPhase.Type.Character:
                        _build.SetName("KMG_CHARGEN_DISPOSABLE_" + _races[_raceIndex].name);
                        if (_controller.State.CanSelectAlignment)
                        {
                            var legalAlignments = new[] { Alignment.TrueNeutral, Alignment.LawfulGood, Alignment.NeutralGood,
                                Alignment.ChaoticGood, Alignment.LawfulNeutral, Alignment.ChaoticNeutral,
                                Alignment.LawfulEvil, Alignment.NeutralEvil, Alignment.ChaoticEvil }.Where(value =>
                                new Kingmaker.UnitLogic.Class.LevelUp.Actions.SelectAlignment(value)
                                    .Check(_controller.State, _controller.Preview)).ToArray();
                            if (legalAlignments.Length == 0) { RejectCharacter("no native legal alignment for chosen deity/class"); break; }
                            _build.SelectAlignment(legalAlignments[0]);
                        }
                        _build.SetVoice(BlueprintRoot.Instance.CharGen.MaleVoices.First(value => value != null));
                        _controller.SetBirthDay(1, 1);
                        _character["characterDetails"] = new JObject {
                            ["nameSelected"] = _build.Character.NameInput.IsSelected(),
                            ["alignmentSelected"] = _build.Character.AlignmentSelector.IsSelected(),
                            ["voiceSelected"] = _build.Character.VoiceSelector.IsSelected(),
                            ["phaseComplete"] = _build.Character.IsSelected(),
                            ["alignment"] = _controller.Preview.Alignment.Value.ToString() };
                        if (!_build.Character.IsSelected()) { RejectCharacter("native character details remain incomplete"); break; }
                        Advance(); break;
                    case CharBPhase.Type.Total:
                    case CharBPhase.Type.TotalInChargen:
                        if (!_controller.State.IsComplete() || !NextEnabled())
                        { RejectCharacter("native final state is incomplete"); break; }
                        if (_regression && RevisitOrQualifyFinalReview()) break;
                        _character["unresolvedSelectionsBeforeCommit"] = _controller.State.RemainingSelections();
                        if (!_canCommit)
                        {
                            // Save-free profiles prove their complete native selection contract,
                            // then cancel. Only the exact working-save scenario may commit.
                            bool traits = !_disabledControl || ((bool?)_character["firstGlobalTraitObserved"] == true &&
                                (bool?)_character["secondGlobalTraitObserved"] == true);
                            _character["selectionContractComplete"] = traits;
                            _character["nativeCommitPerformed"] = false;
                            if (!traits) AcceptanceFailure("Both ordinary Trait selections were not observed.");
                            EndCharacter(); break;
                        }
                        CommitOwnedCreator();
                        _committed = true;
                        _character["nativeCommitPerformed"] = true;
                        _character["nativeCommitCallback"] = _successCallback;
                        _character["finalLevel"] = _unit.Descriptor.Progression.CharacterLevel;
                        _character["finalRaceGuid"] = _unit.Descriptor.Progression.Race.AssetGuid;
                        _character["completed"] = _successCallback && _unit.Descriptor.Progression.CharacterLevel == 1;
                        if (_regression) VerifySelectedRacialGraph("committed-unit", _unit.Descriptor);
                        _commitCleanupPending = true; _settle = 12;
                        break;
                    default: RejectCharacter("unhandled native phase " + phase.Value); break;
                }
            }
            catch (Exception error)
            {
                if (_controller != null && _stage.EndsWith(":Race", StringComparison.Ordinal) &&
                    error is NullReferenceException && (error.StackTrace ?? string.Empty)
                        .Contains("Kingmaker.UI.LevelUp.CharBColorSelector.SetData"))
                {
                    // Preserve this reproduced native UI failure as failed race acceptance.
                    // The observer can still inspect the other request-local race fixtures.
                    _character["nativeOperationException"] = error.ToString();
                    RecordDollRamps("native-color-selector-failed");
                    AcceptanceFailure("native race color selector threw; see nativeOperationException and dollRamps");
                    try { EndCharacter(); }
                    catch (Exception cleanup) { _failures.Add("native failure cleanup: " + cleanup); Finish(); }
                    return;
                }
                _failures.Add(_stage + ": " + error);
                Finish();
            }
        }

        private void Start()
        {
            if (Game.Instance == null || Game.Instance.UI == null || Game.Instance.Player == null ||
                Game.Instance.UI.CharacterBuildController == null) return;
            _mainBefore = Game.Instance.Player.MainCharacter.Value;
            _areaBefore = Game.Instance.CurrentlyLoadedArea;
            if (_canCommit && (_mainBefore == null || _areaBefore == null))
                throw new InvalidOperationException("Creator fixture requires a loaded campaign, never the main-menu scene.");
            _build = Game.Instance.UI.CharacterBuildController;
            LevelUpController global = Game.Instance.UI.LevelUpController;
            _initialCreatorOwnership = new JObject {
                ["windowShown"] = _build.IsShow, ["warmUp"] = _build.WarmUp,
                ["visibleControllerPresent"] = _build.LevelUpController != null,
                ["globalControllerPresent"] = global != null, ["globalAutoCommit"] = global?.AutoCommit,
                ["globalPreviewIsUnit"] = global != null && ReferenceEquals(global.Preview, global.Unit),
                ["globalDollAbsent"] = global != null && global.Doll == null,
                ["globalMode"] = global?.State?.Mode.ToString(), ["actionCount"] = global?.LevelUpActions.Count,
                ["globalUnitId"] = global?.Unit?.Unit?.UniqueId,
                ["globalUnitBlueprint"] = global?.Unit?.Blueprint?.AssetGuid,
                ["buildUnitMatchesGlobal"] = global != null && ReferenceEquals(_build.Unit, global.Unit) };
            // An automatic Start can remain in the global slot after its UI
            // closes or the area changes. The captured loaded-game contract has
            // no actions, preview copy, doll or active window. Preserve the exact
            // alias; never cancel, commit, or reuse its unit.
            bool idleAutomatic = _canCommit && global != null && global.AutoCommit &&
                ReferenceEquals(global.Preview, global.Unit) && global.Doll == null &&
                global.LevelUpActions.Count == 0 && global.State != null &&
                (global.State.Mode == LevelUpState.CharBuildMode.LevelUp ||
                    global.State.Mode == LevelUpState.CharBuildMode.CharGen) && !_build.WarmUp &&
                _build.LevelUpController == null && !_build.IsShow;
            _initialCreatorOwnership["provenIdleAutomaticController"] = idleAutomatic;
            if (_canCommit && global == null && _build.LevelUpController == null && _build.IsShow && ++_readinessWait <= 60)
            { _settle = 4; return; }
            if ((global != null && !idleAutomatic) || _build.LevelUpController != null || _build.IsShow)
                throw new InvalidOperationException("An existing creator cannot be used by this diagnostic: " + _initialCreatorOwnership);
            _globalControllerBefore = global;
            _buildUnitBefore = _build.Unit;
            CaptureInitialInnerAssets();
            ArmSaveGuard();
            _worldBefore = Game.Instance.State.Units.All.ToArray();
            CaptureCreatorMembership();
            VerifyRepeatedHelpfulReconciliation();
            _started = true;
            _settle = 15;
        }

        private void CaptureInitialInnerAssets()
        {
            if (BlueprintBootstrap.ElementalRaces == null) return;
            foreach (var resource in BlueprintBootstrap.ElementalRaces.Visuals.Ordered().SelectMany(value => value.Resources))
                foreach (var asset in resource.Resource.GetInnerAssets().Where(value => !ReferenceEquals(value, null)))
                {
                    if (asset == null) throw new InvalidOperationException("Visual inner asset was already dead before creator start: " + resource.AssetId);
                    if (_initialInnerAssets.Any(value => ReferenceEquals(value.Key, asset))) continue;
                    _initialInnerAssets.Add(new KeyValuePair<UnityEngine.Object, JObject>(asset, new JObject {
                        ["firstOwnerGuid"] = resource.AssetId, ["name"] = asset.name,
                        ["type"] = asset.GetType().FullName, ["instanceId"] = asset.GetInstanceID() }));
                }
        }

        private void VerifyVisualIntegrity()
        {
            var dead = _initialInnerAssets.Where(value => value.Key == null).Select(value => value.Value).ToArray();
            if (dead.Length != 0)
                throw new InvalidOperationException("Registered visual inner assets destroyed during creator: " + new JArray(dead));
        }

        private void VerifyRepeatedHelpfulReconciliation()
        {
            if (_disabledControl) return;
            var favored = AidAnotherOptionalExtensionCoordinator.FavoredClassContract;
            if (favored == null) return;
            var selections = new[] { favored.CombatTraits, favored.RaceTraits, favored.EquipmentTraits,
                favored.FirstTrait, favored.SecondTrait, favored.Adopted };
            var features = selections.Select(value => value.Features).ToArray();
            var all = selections.Select(value => value.AllFeatures).ToArray();
            var entries = all.Select(value => value.ToArray()).ToArray();
            MethodInfo reconcile = typeof(AidAnotherOptionalExtensionCoordinator).GetMethod("TryReconcile",
                BindingFlags.Static | BindingFlags.NonPublic);
            for (int pass = 0; pass < 3; pass++)
            {
                int before = AidAnotherOptionalExtensionCoordinator.SuccessfulReconciliations;
                reconcile.Invoke(null, new object[] { "guarded-creator-repeat-" + pass });
                bool exact = AidAnotherOptionalExtensionCoordinator.SuccessfulReconciliations == before + 1 &&
                    selections.Select((value, index) => ReferenceEquals(value.Features, features[index]) &&
                        ReferenceEquals(value.AllFeatures, all[index]) &&
                        CharacterCreationObservationIdentity.SameOrderedReferences(entries[index], value.AllFeatures)).All(value => value);
                int helpful = favored.CombatTraits.AllFeatures.Count(value =>
                    ReferenceEquals(value, BlueprintBootstrap.BodyguardFeats.HelpfulCombat));
                bool expected = favored.CombatTraits.Features.Length == 0 &&
                    helpful == (favored.TraitsEnabled && _context.FeatureModules.Active.BodyguardFeats ? 1 : 0);
                _compatibilityRechecks.Add(new JObject { ["pass"] = pass, ["exactReferencesAndOrder"] = exact,
                    ["featuresCount"] = favored.CombatTraits.Features.Length,
                    ["allFeaturesCount"] = favored.CombatTraits.AllFeatures.Length, ["helpfulCount"] = helpful,
                    ["publicationExpected"] = expected });
                if (!exact || !expected) throw new InvalidOperationException("Repeated Helpful callback damaged the exact foreign trait contract.");
            }
            ElementalCharacterCreationRoutingObserver.CaptureActiveCheckpoint("after-three-guarded-compatibility-rechecks");
        }

        private void BeginCharacter()
        {
            _character = new JObject { ["raceGuid"] = _races[_raceIndex].AssetGuid,
                ["race"] = _races[_raceIndex].name, ["completed"] = false,
                ["acceptance"] = "NOT-RUN", ["acceptanceFailures"] = new JArray(), ["steps"] = new JArray() };
            _characters.Add(_character);
            _unit = new ChargenUnit(_canCommit && _useRoll ? BlueprintRoot.Instance.CustomCompanion :
                BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
            _character["nativeMercenaryFixture"] = _unit.Descriptor.IsCustomCompanion();
            if (ReferenceEquals(_unit, _mainBefore)) throw new InvalidOperationException("Fixture cannot own the campaign character.");
            _character["fixtureId"] = _unit.UniqueId;
            _spentSkills.Clear();
            BeginRegressionCharacter();
            _operations = 0; _lastCaptureKey = null; _viewWait = 0; _advancePending = false; _classChosen = false; _rollRequested = false; _rollApplied = false; _committed = false; _successCallback = false;
            _build.HandleLevelUpStart(_unit.Descriptor, null, () => _successCallback = true, LevelUpState.CharBuildMode.CharGen);
            _controller = _build.LevelUpController;
            if (_controller == null || _controller.State.NextLevel != 1 || !_build.IsShow)
                throw new InvalidOperationException("Real first-level full-screen creator did not open.");
            _character["nativeCreatorOpened"] = true;
            _settle = 15;
        }

        private void ObserveRouting()
        {
            JObject active = ElementalCharacterCreationRoutingObserver.DescribeActiveBuild();
            _character["afterClass"] = active;
            JObject[] selections = ((JArray)active["selections"]).OfType<JObject>().ToArray();
            JObject[] racial = selections.Where(row => ((string)row["selectionGuid"] ?? string.Empty)
                .StartsWith("e115e1e0a17a4aceb001", StringComparison.Ordinal) ||
                ((string)row["selectionGuid"] ?? string.Empty).StartsWith("e117e1e0a17a4acec001", StringComparison.Ordinal)).ToArray();
            _character["racialSelectionCount"] = racial.Length;
            if (_disabledControl)
            {
                if (racial.Length != 0) throw new InvalidOperationException("KMG racial selections leaked into disabled control.");
                return;
            }
            foreach (JObject selection in racial)
            {
                if (!((JArray)selection["consumedByActualPhases"]).Any(value => ((string)value).StartsWith("Determinator:")))
                    AcceptanceFailure("racial selection consumed outside Heritage: " + (string)selection["selectionGuid"]);
            }
            if (racial.Length != (ReferenceEquals(_races[_raceIndex], BlueprintBootstrap.ElementalRaces.Undine.Race) ? 2 : 4)) AcceptanceFailure("missing elemental racial selections");
        }

        private void SelectCurrentFeature(CharBPhaseFeatures phase)
        {
            if (phase.IsSelected())
            {
                if (_regression && ReferenceEquals(phase, _build.Determinators)) VerifySelectedRacialGraph("heritage-phase-complete");
                Advance(); return;
            }
            FeatureSelectionState selection = (FeatureSelectionState)typeof(CharBPhaseFeatures)
                .GetProperty("CurrentFeatureCollection", Members).GetValue(phase, null);
            if (selection == null) { RejectCharacter("active feature phase has no selection"); return; }
            int depth = 0;
            while (selection.Selected && selection.Next != null)
            {
                if (++depth > 8) throw new InvalidOperationException("Ambiguous nested selection chain.");
                selection = selection.Next;
            }
            if (selection.Selected)
            {
                object switcher = typeof(CharBPhaseFeatures).GetField("m_CollectionSwitcher", Members).GetValue(phase);
                bool moved = (bool)typeof(CharBSelectionSwitch).GetMethod("ActivateNextEmptyItem", Members).Invoke(switcher, null);
                if (!moved) { RejectCharacter("native sub-selection switch could not reach an incomplete selection"); return; }
                _settle = 5; return;
            }
            var blueprint = selection.Selection as BlueprintFeatureSelection;
            string guid = blueprint == null ? string.Empty : blueprint.AssetGuid;
            if (guid == FirstTrait || guid == SecondTrait)
            {
                _character[guid == FirstTrait ? "firstGlobalTraitObserved" : "secondGlobalTraitObserved"] = true;
                if (!_traitsCaptured)
                {
                    ElementalCharacterCreationRoutingObserver.CaptureActiveCheckpoint("actual-first-level-global-trait-phase");
                    _traitsCaptured = true;
                }
            }
            var items = selection.Selection.ExtractSelectionItems(_controller.Unit, _controller.Preview).ToArray();
            RejectDeferredChoices(items);
            var legal = items.Where(item => item.Feature != null && selection.Selection.CanSelect(_controller.Preview,
                _controller.State, selection, item)).ToArray();
            if (legal.Length == 0) { RejectCharacter("zero legal choices: " + guid); return; }
            var rendered = _build.GetComponentsInChildren<CharBuildSelectorItem>(true)
                .Where(item => item.gameObject.activeInHierarchy && item.Toggle != null && item.Toggle.interactable &&
                    ReferenceEquals(item.FeatureSelection, selection) && item.Feature != null).ToArray();
            legal = legal.Where(item => rendered.Any(view => ReferenceEquals(view.Feature.Feature, item.Feature))).ToArray();
            if (legal.Length == 0)
            {
                if (++_viewWait < 90) { _settle = 2; return; }
                RejectCharacter("native visible selector contains no legal rendered choices: " + guid); return;
            }
            _viewWait = 0;
            Capture("rendered-selection-ready:" + guid);
            BlueprintFeature preferred = PreferredRegressionChoice(blueprint);
            IFeatureSelectionItem chosen = preferred == null ? legal.OrderBy(item => ChoicePriority(item.Feature)).ThenBy(item => item.Feature.AssetGuid,
                StringComparer.Ordinal).First() : legal.SingleOrDefault(item => ReferenceEquals(item.Feature, preferred));
            if (chosen == null) throw new InvalidOperationException("Planned racial choice is not legal and visibly rendered: " + preferred.AssetGuid);
            ((JArray)_character["steps"]).Add(new JObject { ["action"] = "select-feature", ["selectionGuid"] = guid,
                ["choiceGuid"] = chosen.Feature.AssetGuid, ["extractedCount"] = items.Length, ["legalCount"] = legal.Length,
                ["phase"] = _build.CurrentPhase.ToString() });
            _build.SetFeature(selection, chosen);
            _racialCheckPending = _regression && preferred != null;
            _settle = 8;
        }

        private static int ChoicePriority(BlueprintFeature feature)
        {
            if (feature.name.Contains("Retain") || feature.name.Contains("KeepNormalRacialTrait")) return 0;
            if (feature.name.Contains("_General_")) return 1;
            if (feature.name == "CombatTrait") return 2;
            if (feature.name == "FaithTrait") return 3;
            return 10;
        }

        private void Allocate()
        {
            LevelUpState state = _controller.State;
            if (state.CanSelectRaceStat && !state.SelectedRaceStat.HasValue)
            {
                _build.SetRacialBonus(StatType.Strength);
                _settle = 5; return;
            }
            if (_useRoll && !_rollApplied) { ApplyNativeRoll(); return; }
            if (!state.StatsDistribution.IsComplete())
            {
                foreach (StatType stat in new[] { StatType.Strength, StatType.Dexterity, StatType.Constitution,
                    StatType.Intelligence, StatType.Wisdom, StatType.Charisma })
                    if (state.StatsDistribution.CanAdd(stat))
                    {
                        _build.BuyAttribute(stat, true);
                        _settle = 5; return;
                    }
                RejectCharacter("point-buy has remaining points but no native add operation"); return;
            }
            if (state.SkillPointsRemaining < 0)
            {
                if (_spentSkills.Count == 0) { RejectCharacter("excess skills are not owned by this request"); return; }
                StatType stat = _spentSkills[_spentSkills.Count - 1];
                int before = _controller.LevelUpActions.OfType<Kingmaker.UnitLogic.Class.LevelUp.Actions.SpendSkillPoint>()
                    .Count(value => value.Skill == stat);
                if (before != 1) { RejectCharacter("owned skill refund action is absent or ambiguous"); return; }
                int pointsBefore = state.SkillPointsRemaining;
                _build.SpendSkillPoint(stat, false);
                int after = _controller.LevelUpActions.OfType<Kingmaker.UnitLogic.Class.LevelUp.Actions.SpendSkillPoint>()
                    .Count(value => value.Skill == stat);
                if (after != before - 1) throw new InvalidOperationException("Native skill refund did not remove exactly one owned action.");
                _spentSkills.RemoveAt(_spentSkills.Count - 1);
                ((JArray)_character["steps"]).Add(new JObject { ["action"] = "refund-owned-skill",
                    ["skill"] = stat.ToString(), ["pointsBefore"] = pointsBefore,
                    ["pointsAfter"] = _controller.State.SkillPointsRemaining,
                    ["ownedActionsBefore"] = before, ["ownedActionsAfter"] = after });
                _settle = 8; return;
            }
            if (state.SkillPointsRemaining > 0)
            {
                foreach (StatType stat in Enum.GetValues(typeof(StatType)).Cast<StatType>().Where(value => value.ToString().StartsWith("Skill")))
                    if (new Kingmaker.UnitLogic.Class.LevelUp.Actions.SpendSkillPoint(stat).Check(state, _controller.Preview))
                    {
                        _build.SpendSkillPoint(stat, true);
                        _spentSkills.Add(stat);
                        _settle = 5; return;
                    }
                RejectCharacter("skill allocation has remaining points but no native spend operation"); return;
            }
            Advance();
        }

        private void ApplyNativeRoll()
        {
            var entry = UnityModManagerNet.UnityModManager.modEntries.SingleOrDefault(value =>
                value.Info.Id == "KingmakerDiceRoller" && value.Loaded && value.Active && !value.ErrorOnLoading);
            if (entry == null || entry.Info.Version != "0.1.2")
                throw new InvalidOperationException("The exact active Dice Roller 0.1.2 fixture prerequisite is missing.");
            Type bridge = entry.Assembly.GetType("KingmakerDiceRoller.Patches.KingmakerPatchBridge", true);
            object panel = bridge.GetField("panel", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            object commands = panel.GetType().GetField("commands", Members).GetValue(panel);
            object session = commands.GetType().GetProperty("ActiveSession", Members).GetValue(commands, null);
            if (session == null) throw new InvalidOperationException("Dice Roller has no accepted session for this exact native creator; inspect its eligibility log.");
            Func<string, object> read = name => session.GetType().GetProperty(name, Members).GetValue(session, null);
            if (!ReferenceEquals(read("Controller"), _controller) || !ReferenceEquals(read("State"), _controller.State) ||
                !ReferenceEquals(read("Unit"), _controller.Preview))
                throw new InvalidOperationException("Dice Roller does not own the exact disposable native preview.");
            if (!_rollRequested)
            {
                Type command = entry.Assembly.GetType("KingmakerDiceRoller.CharacterCreation.RollUiCommand", true);
                Type score = entry.Assembly.GetType("KingmakerDiceRoller.Domain.AbilityScore", true);
                panel.GetType().GetMethod("Execute", Members).Invoke(panel,
                    new[] { Enum.Parse(command, "Roll"), Enum.Parse(score, "Strength") });
                _rollRequested = true; _settle = 20; return;
            }
            bool applied = (bool)read("IsRollMode") && (bool)read("IsApplied") && !(bool)read("CandidateBaselineContaminated");
            object assignment = read("Assignment");
            int[] assigned = assignment == null ? new int[0] : (int[])assignment.GetType()
                .GetMethod("ToAssignedArray", Members).Invoke(assignment, null);
            StatType[] abilities = { StatType.Strength, StatType.Dexterity, StatType.Constitution,
                StatType.Intelligence, StatType.Wisdom, StatType.Charisma };
            int[] actual = abilities.Select(stat => _controller.Preview.Stats.GetStat(stat).BaseValue).ToArray();
            _character["diceRoller"] = new JObject { ["mode"] = read("Mode").ToString(),
                ["generation"] = (int)read("Generation"), ["verifiedGeneration"] = (int)read("VerifiedGeneration"),
                ["assigned"] = new JArray(assigned), ["actualBaseValues"] = new JArray(actual),
                ["exactPreviewOwner"] = true, ["applied"] = applied };
            if (!applied || !assigned.SequenceEqual(actual))
                throw new InvalidOperationException("The native rolled assignment did not verify on the current preview.");
            _rollApplied = true; _settle = 8;
        }

        private void RecordDollRamps(string stage)
        {
            var evidence = new JObject { ["stage"] = stage };
            try
            {
                foreach (var entry in new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<UnityEngine.Texture2D>> {
                    ["skin"] = _controller.Doll.GetSkinRamps(), ["hair"] = _controller.Doll.GetHairRamps(),
                    ["horns"] = _controller.Doll.GetHornsRamps() })
                    evidence[entry.Key] = new JArray(entry.Value.Select((texture, index) => new JObject {
                        ["index"] = index, ["managedNull"] = ReferenceEquals(texture, null),
                        ["nativeAlive"] = texture != null, ["name"] = texture == null ? null : texture.name,
                        ["instanceId"] = ReferenceEquals(texture, null) ? 0 : texture.GetInstanceID() }));
            }
            catch (Exception error) { evidence["inspectionError"] = error.ToString(); }
            evidence["dollRooms"] = new JArray(UnityEngine.Resources.FindObjectsOfTypeAll<CharGenDollRoom>()
                .Select(room => DescribeDollRoom(room)));
            _character["dollRamps"] = evidence;
        }

        private static JObject DescribeDollRoom(CharGenDollRoom room)
        {
            var ids = (HashSet<string>)typeof(CharGenDollRoom).GetField(
                "m_InitiallyLoadedEquipmentEntityIds", Members).GetValue(room);
            var assets = (List<UnityEngine.Object>)typeof(CharGenDollRoom).GetField(
                "m_InitiallyLoadedEquipmentEntityInnerAssets", Members).GetValue(room);
            var resources = BlueprintBootstrap.ElementalRaces?.Visuals.Ordered().SelectMany(value => value.Resources).ToArray();
            return new JObject { ["instanceId"] = room.GetInstanceID(),
                ["initialIdsCount"] = ids?.Count, ["initialAssetsCount"] = assets?.Count,
                ["nativeDependencies"] = new JArray((BlueprintBootstrap.ElementalRaces?.Visuals.NativeDependencyIds ?? new string[0])
                    .Select(id => new JObject { ["guid"] = id, ["initialIdProtected"] = ids != null && ids.Contains(id) })),
                ["registeredProxyIds"] = resources == null ? new JArray() : new JArray(resources.Select(value => new JObject {
                    ["guid"] = value.AssetId, ["nativeAlive"] = value.Resource != null,
                    ["initialIdProtected"] = ids != null && ids.Contains(value.AssetId),
                    ["unprotectedSkinRamps"] = new JArray((value.Resource.PrimaryRamps ?? new List<UnityEngine.Texture2D>())
                        .Where(texture => texture != null && (assets == null || !assets.Any(asset => ReferenceEquals(asset, texture))))
                        .Select(texture => texture.name)) })) };
        }

        private static void ObserveInnerAssetUnload(EquipmentEntity __instance,
            HashSet<UnityEngine.Object> exceptedAssets, out JObject __state)
        {
            __state = null;
            if (_saveGuardOwner == null || BlueprintBootstrap.ElementalRaces == null) return;
            try
            {
                var owned = BlueprintBootstrap.ElementalRaces.Visuals.Ordered().SelectMany(value => value.Resources)
                    .SingleOrDefault(value => ReferenceEquals(value.Resource, __instance));
                var shared = _saveGuardOwner._initialInnerAssets.Where(value => __instance.GetInnerAssets()
                    .Any(asset => ReferenceEquals(value.Key, asset))).ToArray();
                if (owned == null && shared.Length == 0) return;
                __state = new JObject { ["stage"] = _saveGuardOwner._stage, ["guid"] = owned?.AssetId,
                    ["sharedAssets"] = new JArray(shared.Select(value => {
                        var item = (JObject)value.Value.DeepClone();
                        item["excepted"] = exceptedAssets.Contains(value.Key);
                        item["aliveBefore"] = value.Key != null;
                        return item;
                    })),
                    ["proxy"] = __instance.name, ["caller"] = Environment.StackTrace,
                    ["skinBefore"] = new JArray(__instance.PrimaryRamps.Select(texture => new JObject {
                        ["name"] = texture == null ? null : texture.name, ["nativeAlive"] = texture != null,
                        ["excepted"] = exceptedAssets.Contains(texture) })) };
                _saveGuardOwner._assetUnloads.Add(__state);
            }
            catch (Exception error) { _saveGuardOwner._failures.Add("asset observation: " + error); }
        }

        private static void AfterInnerAssetUnload(EquipmentEntity __instance, JObject __state)
        {
            if (__state == null) return;
            __state["sharedAssetsAliveAfter"] = new JArray(_saveGuardOwner._initialInnerAssets.Where(value =>
                ((JArray)__state["sharedAssets"]).Any(item => (int)item["instanceId"] == value.Key.GetInstanceID()))
                .Select(value => value.Key != null));
            __state["skinAliveAfter"] = new JArray(__instance.PrimaryRamps.Select(texture => texture != null));
        }

        private bool NextEnabled()
        {
            var button = (UnityEngine.UI.Button)typeof(CharacterBuildController).GetField("m_CompleteButton", Members).GetValue(_build);
            return button != null && button.interactable;
        }
        private void Advance()
        {
            _advancePending = true; _settle = 12;
        }
        private void Capture(string stage)
        {
            ((JArray)_character["steps"]).Add(new JObject { ["checkpoint"] = stage,
                ["nativeBuild"] = ElementalCharacterCreationRoutingObserver.DescribeActiveBuild() });
            Write();
        }
        private void AcceptanceFailure(string failure)
        { ((JArray)_character["acceptanceFailures"]).Add(failure); }
        private void RejectCharacter(string failure)
        { Capture("rejected:" + failure); AcceptanceFailure(failure); EndCharacter(); }
        private void EndCharacter()
        {
            CleanupCharacter();
            _character["acceptance"] = (!_canCommit ? (bool?)_character["selectionContractComplete"] == true :
                (bool)_character["completed"]) &&
                ((JArray)_character["acceptanceFailures"]).Count == 0 ? "PASS" : "FAIL";
            _raceIndex++; _settle = 12; Write();
        }
        private void CleanupCharacter()
        {
            if (_controller != null)
            {
                LevelUpController global = Game.Instance.UI.LevelUpController;
                LevelUpController visible = _build.LevelUpController;
                // Native CharacterBuildController.OnHide clears its controller on Commit.
                // Null after our successful commit is cleanup, never a foreign owner.
                if ((global != null && !ReferenceEquals(global, _controller)) ||
                    (visible != null && !ReferenceEquals(visible, _controller)) ||
                    (!_committed && (global == null || visible == null)))
                    throw new InvalidOperationException("Cannot clean an unrelated native controller.");
                _character["cleanupControllerState"] = new JObject {
                    ["committed"] = _committed, ["globalAlreadyCleared"] = global == null,
                    ["visibleAlreadyCleared"] = visible == null };
                if (!_committed) _controller.Cancel();
                if (_build.IsShow) _build.Show(false);
                Game.Instance.UI.LevelUpController = _globalControllerBefore;
                typeof(CharacterBuildController).GetProperty("LevelUpController", Members).SetValue(_build, null, null);
                _build.Unit = _buildUnitBefore;
                _controller = null;
            }
            if (_unit != null) { CleanupCreatorMembership(); _unit = null; }
        }
        private void ArmSaveGuard()
        {
            if (_saveGuardOwner != null) throw new InvalidOperationException("A creator save guard is already active.");
            _saveGuardId = "KMG.CharacterCreationBaseline." + _request.RunId;
            _saveGuard = HarmonyInstance.Create(_saveGuardId);
            _saveGuardOwner = this;
            foreach (MethodInfo method in typeof(SaveManager).GetMethods(Members)
                .Where(method => method.Name == "SaveRoutine" || method.Name == "SaveStashedArea" ||
                    method.Name == "DeleteSave" || method.Name == "RemoveSaveFromList"))
            {
                MethodInfo prefix = typeof(ElementalCharacterCreationBaselineScenario).GetMethod(
                    method.Name == "SaveRoutine" ? "BlockSaveRoutine" : "BlockSaveMutation",
                    BindingFlags.Static | BindingFlags.NonPublic);
                _saveGuard.Patch(method, new HarmonyMethod(prefix));
                _saveMethods.Add(method);
            }
            if (_saveMethods.Count != 5) throw new InvalidOperationException("Native save mutation boundary changed.");
            if (!_disabledControl)
            {
                MethodInfo unload = typeof(EquipmentEntity).GetMethod("UnloadInnerAssetsExceptGiven", Members);
                _saveGuard.Patch(unload, new HarmonyMethod(typeof(ElementalCharacterCreationBaselineScenario).GetMethod(
                    "ObserveInnerAssetUnload", BindingFlags.Static | BindingFlags.NonPublic)),
                    new HarmonyMethod(typeof(ElementalCharacterCreationBaselineScenario).GetMethod(
                        "AfterInnerAssetUnload", BindingFlags.Static | BindingFlags.NonPublic)));
                _observedAssetMethods.Add(unload);
            }
        }
        private static bool BlockSaveRoutine(ref IEnumerator<object> __result)
        {
            if (_saveGuardOwner == null) return true;
            _saveGuardOwner._failures.Add("Unexpected native SaveRoutine blocked during disposable creation.");
            __result = Enumerable.Empty<object>().GetEnumerator();
            return false;
        }
        private static bool BlockSaveMutation()
        {
            if (_saveGuardOwner == null) return true;
            _saveGuardOwner._failures.Add("Unexpected native save mutation blocked during disposable creation.");
            return false;
        }
        private void DisarmSaveGuard()
        {
            foreach (MethodBase method in _saveMethods.Concat(_observedAssetMethods))
                _saveGuard.Unpatch(method, HarmonyPatchType.All, _saveGuardId);
            _saveMethods.Clear(); _observedAssetMethods.Clear();
            if (ReferenceEquals(_saveGuardOwner, this)) _saveGuardOwner = null;
        }
        private void Write()
        {
            RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory, EvidenceFileName),
                new JObject { ["schemaVersion"] = 1, ["runId"] = _request.RunId,
                    ["purpose"] = "diagnostic native creator baseline", ["humanAcceptance"] = "NOT-RUN",
                    ["initialCreatorOwnership"] = _initialCreatorOwnership,
                    ["initialInnerAssets"] = new JArray(_initialInnerAssets.Select(value => value.Value)),
                    ["compatibilityRechecks"] = _compatibilityRechecks.DeepClone(),
                    ["characters"] = _characters.DeepClone(), ["assetUnloads"] = _assetUnloads.DeepClone(), ["creatorCleanup"] = _creatorCleanupEvidence?.DeepClone(), ["instrumentationFailures"] = new JArray(_failures) }.ToString(Formatting.Indented));
        }
        private void Finish()
        {
            try { CleanupCharacter(); }
            catch (Exception error) { _failures.Add("cleanup: " + error); }
            bool membershipRestored = CreatorMembershipRestored();
            bool restored = !_started && _unit == null && _controller == null ||
                (_worldBefore != null && CharacterCreationObservationIdentity.SameOrderedReferences(
                _worldBefore, Game.Instance.State.Units.All.ToArray()) &&
                ReferenceEquals(Game.Instance.UI.LevelUpController, _globalControllerBefore) &&
                ReferenceEquals(_build.Unit, _buildUnitBefore) && _build.LevelUpController == null &&
                ReferenceEquals(Game.Instance.Player.MainCharacter.Value, _mainBefore) &&
                ReferenceEquals(Game.Instance.CurrentlyLoadedArea, _areaBefore) && membershipRestored);
            if (!restored) _failures.Add("Original world unit membership or controller ownership was not restored.");
            try { DisarmSaveGuard(); }
            catch (Exception error) { _failures.Add("save guard cleanup: " + error); }
            Result = ElementalCharacterCreationRoutingObserver.Run(_context, _request);
            Result.Assertions.Add(new RuntimeTestAssertion { Name = "actual-first-level-creators-observed",
                Expected = _races.Length.ToString(), Observed = _characters.Count.ToString(), Status = _characters.Count == _races.Length &&
                    _characters.OfType<JObject>().All(row => (bool?)row["nativeCreatorOpened"] == true)
                    ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, Evidence = EvidenceFileName });
            Result.Assertions.Add(new RuntimeTestAssertion { Name = "request-local-creator-cleanup",
                Expected = "no instrumentation failures; exact restoration", Observed = string.Join("|", _failures),
                Status = _failures.Count == 0 ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, Evidence = EvidenceFileName });
            if (_regression) Result.Assertions.Add(new RuntimeTestAssertion { Name = "native-elemental-roundtrip-commits",
                Expected = "three complete native commits after exact racial round trips",
                Observed = string.Join("|", _characters.OfType<JObject>().Select(row => (string)row["acceptance"])),
                Status = _characters.Count == 3 && _characters.OfType<JObject>().All(row =>
                    (string)row["acceptance"] == "PASS" && (bool?)row["completed"] == true)
                    ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, Evidence = EvidenceFileName });
            Result.Status = Result.Assertions.All(value => value.Status == RuntimeTestStatuses.Pass)
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail;
            Result.ExceptionSummary = string.Join("|", _failures);
            Result.Diagnostics.Add("Per-character acceptance is recorded independently; baseline observation PASS does not qualify the broken candidate.");
            Result.EvidenceFiles.Add(Path.Combine(_request.EvidenceDirectory, EvidenceFileName));
            Result.WorkingSaveSmoke = _loaded;
            Result.GameVersion = Kingmaker.GameVersion.GetVersion();
            Write(); Complete = true;
        }
    }
}
