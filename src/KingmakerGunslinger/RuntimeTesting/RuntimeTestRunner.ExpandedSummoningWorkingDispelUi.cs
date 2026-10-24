using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private DateTime _sprints30DispelNextLoadObservation;
        private readonly List<JObject> _sprints30DispelLoadObservations = new List<JObject>();

        // Read-only diagnosis of the actual failed load boundary. No input,
        // dialog dismissal, load retry, save write or actor mutation.
        private bool ObserveSprints30DispelLoadBoundary()
        {
            if (!RuntimeTestRequestParser.Sprints30And31DispelUi(_request) ||
                _workingSaveSmoke.Stage != "load-completion" || _workingSaveSmoke.Complete)
                return true;
            var dialog = Kingmaker.UI.DialogMessageBox.Instance;
            bool shown = dialog != null && dialog.IsShown;
            if (!shown && DateTime.UtcNow < _sprints30DispelNextLoadObservation) return true;
            _sprints30DispelNextLoadObservation = DateTime.UtcNow.AddSeconds(10);
            var game = Game.Instance;
            var loading = Kingmaker.EntitySystem.Persistence.LoadingProcess.Instance;
            string message = null;
            if (shown)
            {
                var field = typeof(Kingmaker.UI.DialogMessageBox).GetField("m_Messagelabel",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (field == null) throw new MissingFieldException(typeof(Kingmaker.UI.DialogMessageBox).FullName, "m_Messagelabel");
                var label = field.GetValue(dialog);
                message = label?.GetType().GetProperty("text")?.GetValue(label, null) as string;
            }
            var row = new JObject { ["utc"] = DateTime.UtcNow.ToString("o"),
                ["runId"] = _request.RunId, ["stage"] = _workingSaveSmoke.Stage,
                ["stageElapsedMs"] = _workingSaveSmoke.StageElapsedMilliseconds,
                ["mode"] = game.CurrentMode.ToString(), ["area"] = game.CurrentlyLoadedArea?.AssetGuid,
                ["partyCount"] = game.Player?.Party?.Count(),
                ["loading"] = loading != null && loading.IsLoadingInProcess,
                ["screen"] = loading != null && loading.IsLoadingScreenActive,
                ["manualScreen"] = loading != null && loading.IsManualLoadingScreenActive,
                ["processTag"] = loading?.CurrentProcessTag.ToString(),
                ["progress"] = loading?.CurrentProcessProgressNorm,
                ["dialogShown"] = shown, ["dialogMessage"] = message,
                ["writeObserved"] = _workingSaveSmoke.WriteObserved };
            if (_sprints30DispelLoadObservations.Count >= 64)
                throw new InvalidOperationException("Fixed load observer exceeded its bounded observation count.");
            _sprints30DispelLoadObservations.Add(row);
            RuntimeTestResultWriter.WriteAtomic(System.IO.Path.Combine(_request.EvidenceDirectory,
                "sprints30-31-working-load-boundary.json"), new JArray(_sprints30DispelLoadObservations)
                    .ToString(Newtonsoft.Json.Formatting.Indented));
            if (!shown) return true;
            CompleteWorkingSaveSmoke(RuntimeTestStatuses.Fail, "fixed-dispel-ui-unexpected-load-dialog",
                "Unexpected native load dialog; no input was supplied: " + message);
            return false;
        }

        private readonly List<JObject> _sprints30DispelUiErrors = new List<JObject>();
        private int _sprints30DispelUiErrorCount;
        private bool _sprints30DispelUiListenerAttached, _sprints30DispelUiListenerRemoved;

        private void ObserveDispelUiError(string message, string trace, UnityEngine.LogType type)
        {
            if (type != UnityEngine.LogType.Error && type != UnityEngine.LogType.Exception &&
                type != UnityEngine.LogType.Assert) return;
            _sprints30DispelUiErrorCount++;
            if (_sprints30DispelUiErrors.Count >= 64) return;
            _sprints30DispelUiErrors.Add(new JObject { ["sequence"] = _sprints30DispelUiErrorCount,
                ["utc"] = DateTime.UtcNow.ToString("o"), ["type"] = type.ToString(),
                ["message"] = message == null || message.Length <= 4096 ? message : message.Substring(0, 4096),
                ["trace"] = trace == null || trace.Length <= 16384 ? trace : trace.Substring(0, 16384) });
        }

        private IEnumerator<int> _sprints30WorkingUiSteps;
        private DateTime _sprints30WorkingUiDeadline;
        private readonly List<RuntimeTestAssertion> _sprints30WorkingUiAssertions =
            new List<RuntimeTestAssertion>();

        private void Sprints30DispelUiCheck(string id, bool pass, JObject facts,
            string expected)
        {
            facts["runId"] = _request.RunId;
            facts["caseId"] = id;
            _sprints30WorkingUiAssertions.Add(Assertion("sprints30-31-" + id,
                expected, facts.ToString(Newtonsoft.Json.Formatting.None), pass,
                "Fixed read-only Working save UI; controlled disposable summons; no action or AI claim."));
            if (!pass) throw new InvalidOperationException("Working Dispel UI failed: " + id);
        }

        private void StepSprints30DispelWorkingUi()
        {
            if (_sprints30WorkingUiSteps == null)
            {
                _sprints30WorkingUiDeadline = DateTime.UtcNow.AddSeconds(150);
                _sprints30WorkingUiSteps = RunSprints30DispelWorkingUi().GetEnumerator();
            }
            Exception error = null;
            try
            {
                if (DateTime.UtcNow > _sprints30WorkingUiDeadline)
                    throw new TimeoutException("Fixed Working Dispel UI exceeded its bounded observation window.");
                if (_sprints30WorkingUiSteps.MoveNext()) return;
            }
            catch (Exception failure) { error = failure; }
            try { _sprints30WorkingUiSteps.Dispose(); }
            catch (Exception failure) { error = error == null ? failure : new AggregateException(error, failure); }
            _sprints30WorkingUiSteps = null;
            string uiErrorsPath = System.IO.Path.Combine(_request.EvidenceDirectory,
                "sprints30-31-dispel-ui-errors.json");
            RuntimeTestResultWriter.WriteAtomic(uiErrorsPath, new JObject {
                ["runId"] = _request.RunId, ["scope"] = "UI interaction after sealed native Working load; all errors/exceptions/asserts block",
                ["count"] = _sprints30DispelUiErrorCount, ["overflow"] = _sprints30DispelUiErrorCount > 64,
                ["listenerAttached"] = _sprints30DispelUiListenerAttached,
                ["listenerRemoved"] = _sprints30DispelUiListenerRemoved,
                ["errors"] = new JArray(_sprints30DispelUiErrors) }.ToString(Newtonsoft.Json.Formatting.Indented));
            bool uiErrorsClean = _sprints30DispelUiErrorCount == 0 &&
                _sprints30DispelUiListenerAttached && _sprints30DispelUiListenerRemoved;
            _sprints30WorkingUiAssertions.Add(Assertion("sprints30-31-dispel-ui-native-errors",
                "Both native owned menus open/click/reset without Unity errors, exceptions or asserts; UI listener removed.",
                "errors=" + _sprints30DispelUiErrorCount + ";listenerRemoved=" + _sprints30DispelUiListenerRemoved,
                uiErrorsClean, uiErrorsPath));
            WorkingSaveSmokeEvidence save = _workingSaveSmoke.Stop();
            bool readOnly = save.WorkingMatchCount == 1 && save.BaselineMatchCount == 1 &&
                save.DescriptorReferenceCorrelated && save.ExpectedWorkingSaveRoutineCount == 0 &&
                !save.SaveWritingApiObserved;
            _sprints30WorkingUiAssertions.Add(Assertion("sprints30-31-dispel-ui-exact-readonly-working",
                "One correlated exact Working load, distinct baseline, no save-writing API.",
                "working=" + save.WorkingMatchCount + ";baseline=" + save.BaselineMatchCount +
                ";correlated=" + save.DescriptorReferenceCorrelated + ";saveCalls=" +
                save.ExpectedWorkingSaveRoutineCount + ";write=" + save.SaveWritingApiObserved,
                readOnly, "Existing guarded Working-load observer and immutable outer save envelope."));
            string[] required = { "trumpet-dispel-fixture", "trumpet-dispel-native-ui",
                "trumpet-dispel-native-ui-restored", "trumpet-dispel-ui-owned-cleanup",
                "astral-dispel-fixture", "astral-dispel-native-ui",
                "astral-dispel-native-ui-restored", "astral-dispel-ui-owned-cleanup",
                "dispel-ui-world-restored" };
            bool complete = required.All(id => _sprints30WorkingUiAssertions.Count(row =>
                row.Name == "sprints30-31-" + id && row.Status == RuntimeTestStatuses.Pass) == 1);
            var result = CreateResult(error == null && readOnly && complete &&
                _sprints30WorkingUiAssertions.All(row => row.Status == RuntimeTestStatuses.Pass)
                    ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                _sprints30WorkingUiAssertions, error?.ToString());
            result.WorkingSaveSmoke = save;
            result.EvidenceFiles.Add(uiErrorsPath);
            _sprints30DispelScreens?.AppendEvidence(result.EvidenceFiles);
            Complete(result);
        }

        private IEnumerable<int> RunSprints30DispelWorkingUi()
        {
            var game = Game.Instance;
            if (!RuntimeTestRequestParser.Sprints30And31DispelUi(_request) ||
                !_context.FeatureModules.Active.ExpandedSummoning ||
                game.CurrentMode != GameModeType.Default || game.SelectedAbilityHandler == null ||
                game.SelectedAbilityHandler.Ability != null || _workingSaveSmoke.WriteObserved)
                throw new InvalidOperationException("Working Dispel UI requires the exact read-only case and idle native Default-mode input.");
            SealDispelUiLoadDiagnostics();
            var unitsBefore = game.State.Units.All.ToArray();
            var partyBefore = game.Player.Party.ToArray();
            var areasBefore = game.State.AreaEffects.All.ToArray();
            var buffsBefore = unitsBefore.ToDictionary(unit => unit, unit => unit.Buffs.Enumerable.ToArray());
            var positionsBefore = unitsBefore.ToDictionary(unit => unit, unit => unit.Position);
            var uiSettings = partyBefore.Select(unit => new TeleportationUiSettingsFixture(unit.UISettings)).ToArray();
            bool paused = game.IsPaused;
            TimeSpan timeBefore = game.TimeController.GameTime;
            var randomBefore = UnityEngine.Random.state;
            ExpandedSummoningCorrectionFixture fixture = null;
            UnitEntityData owner = null;
            UnitReference originalMaster = default(UnitReference);
            bool masterCaptured = false;
            BlueprintFaction originalFaction = null;
            bool factionCaptured = false, originalControl = false;
            _sprints30DispelScreens = new NativeIconScreenEvidence(_request);
            try
            {
                UnityEngine.Application.logMessageReceived += ObserveDispelUiError;
                _sprints30DispelUiListenerAttached = true;
                game.IsPaused = true;
                // Two fixed actual users, each followed by a verified reset.
                foreach (string key in new[] { "trumpet-archon", "astral-deva" })
                {
                    string phase = key == "trumpet-archon" ? "trumpet-dispel" : "astral-dispel";
                    fixture = BeginExpandedSummoningCorrectionFixture("KMG_S30_31_DispelUiCaster");
                    string unitSymbol = key == "trumpet-archon" ?
                        "KMG.Summoning.Unit.TrumpetArchon" : "KMG.Summoning.Unit.AstralDeva";
                    var control = new Sprint16ManualSummonControl { Caster = fixture.Caster,
                        Blueprint = (BlueprintUnit)ExpandedSummoningBlueprints.Registered.BySymbol[unitSymbol] };
                    EventBus.Subscribe(control);
                    try { owner = CastExpandedSummoningOwnTier(fixture, key); }
                    finally { EventBus.Unsubscribe(control); }
                    Sprint28SetBrainActivePreservingActions(owner, false);
                    owner.Commands.InterruptAll(true); owner.Commands.RemoveFinishedAndUpdateQueue();
                    originalMaster = owner.Descriptor.Master; masterCaptured = true;
                    originalFaction = owner.Faction; factionCaptured = true;
                    var summonPart = owner.Get<UnitPartSummonedMonster>();
                    originalControl = summonPart != null && summonPart.IsDirectlyControllable;
                    bool directlyBefore = owner.IsDirectlyControllable;
                    // Established Working presentation fixture: only this
                    // disposable actor joins the native player faction. This
                    // is UI preparation, not product summon-control evidence.
                    owner.Descriptor.SwitchFactions(game.Player.MainCharacter.Value.Faction, false);
                    if (summonPart == null) throw new InvalidOperationException("Owned UI summon part missing.");
                    summonPart.IsDirectlyControllable = true;
                    bool capitalMaster = false;
                    if (game.CurrentlyLoadedArea.IsCapital && !owner.IsDirectlyControllable)
                    {
                        owner.Descriptor.Master = game.Player.MainCharacter;
                        capitalMaster = true;
                    }
                    string prefix = key == "trumpet-archon" ? ExpandedSummoningSpecialBuilder.Trumpet31Prefix :
                        ExpandedSummoningSpecialBuilder.Astral31Prefix;
                    var family = (BlueprintAbility)ExpandedSummoningBlueprints.Registered.BySymbol[prefix + "DispelEvil"];
                    var delivery = (BlueprintAbility)ExpandedSummoningBlueprints.Registered.BySymbol[prefix + "DispelEvilDispelTouch"];
                    var menu = (BlueprintAbility)ExpandedSummoningBlueprints.Registered.BySymbol[prefix + "DispelEvilHeld"];
                    AbilityData data;
                    SpellSlot slot = null;
                    if (key == "trumpet-archon")
                    {
                        var creation = owner.Blueprint.GetComponent<TrumpetPreparedBook>();
                        var book = owner.Descriptor.GetSpellbook(creation.Book);
                        slot = book.GetMemorizedSpellSlots(5).Single(value => value.Available && ReferenceEquals(value.Spell?.Blueprint, family));
                        data = slot.Spell; data.ParamSpellSlot = slot;
                    }
                    else data = new AbilityData(owner.Descriptor.Abilities.GetAbility(family));
                    var parameters = data.CalculateParams();
                    bool rootAvailable = data.IsAvailable;
                    // UI-only fixture preparation: consume the native owner
                    // preparation and invoke its real arm component. This is
                    // not a command/action/AI or fresh mechanics verdict.
                    data.Spend();
                    var context = new AbilityExecutionContext(data, parameters, new TargetWrapper(owner), null);
                    family.GetComponent<AbilityEffectArmAstralDispelEvilTouch>().Apply(context, new TargetWrapper(owner));
                    var held = owner.Get<UnitPartTouch>();
                    int expectedCl = key == "trumpet-archon" ? 14 : 13;
                    int expectedDc = key == "trumpet-archon" ? 20 : 21;
                    bool ready = rootAvailable && control.Matched == 1 && owner.IsDirectlyControllable &&
                        owner.Get<UnitPartSummonedMonster>()?.IsDirectlyControllable == true &&
                        held != null && ReferenceEquals(held.Ability.Data.Blueprint, menu) && !menu.Hidden &&
                        parameters.CasterLevel == expectedCl && parameters.SpellLevel == 5 && parameters.DC == expectedDc &&
                        (slot == null || !slot.Available);
                    Sprints30DispelUiCheck(phase + "-fixture", ready,
                        new JObject { ["creature"] = key, ["owner"] = owner.UniqueId,
                            ["controlledFixture"] = true, ["nativeControlMatches"] = control.Matched,
                            ["directlyControllable"] = owner.IsDirectlyControllable, ["capitalMasterBound"] = capitalMaster,
                            ["directlyControllableBeforeFixture"] = directlyBefore,
                            ["nativeControlFlagBeforeFixture"] = originalControl,
                            ["nativeControlFlag"] = summonPart.IsDirectlyControllable,
                            ["originalFaction"] = originalFaction.AssetGuid, ["fixtureFaction"] = owner.Faction.AssetGuid,
                            ["factionControl"] = owner.Faction.IsDirectlyControllable,
                            ["playerFaction"] = owner.IsPlayerFaction,
                            ["detached"] = owner.IsDetached, ["preventDirectControl"] = (bool)owner.PreventDirectControl,
                            ["dead"] = owner.Descriptor.State.IsFinallyDead, ["panicked"] = owner.Descriptor.State.IsPanicked,
                            ["ownedFactionFixtureWrites"] = 1, ["ownedControlFixtureWrites"] = 1,
                            ["cl"] = parameters.CasterLevel, ["spellLevel"] = parameters.SpellLevel, ["dc"] = parameters.DC,
                            ["book"] = data.Spellbook?.Blueprint.AssetGuid, ["slotAvailable"] = slot?.Available,
                            ["fixtureNativeSpendCalls"] = 1, ["fixtureArmCalls"] = 1, ["commandClaims"] = 0 },
                        "A controlled owned summon has its actual native held state and parameters; UI preparation is explicit and makes no action/AI claim.");
                    AbilityData selected = null;
                    foreach (int frame in Dispel31ChooseHeldUi(owner, held.Ability.Data, delivery,
                        phase, value => selected = value)) yield return frame;
                    var items = owner.Descriptor.Inventory.Items.Concat(new[] { owner.Body.PrimaryHand.MaybeWeapon })
                        .Where(value => value != null).Distinct().ToArray();
                    owner.Descriptor.Master = originalMaster; masterCaptured = false;
                    summonPart.IsDirectlyControllable = originalControl;
                    owner.Descriptor.SwitchFactions(originalFaction, false); factionCaptured = false;
                    bool cleaned;
                    EndExpandedSummoningCorrectionFixture(fixture, out cleaned); fixture = null;
                    int frames = 0; var deadline = DateTime.UtcNow.AddSeconds(8);
                    while (DateTime.UtcNow < deadline && frames++ < 1200 &&
                        (!game.State.Units.All.SequenceEqual(unitsBefore) ||
                         !game.State.AreaEffects.All.SequenceEqual(areasBefore))) yield return 0;
                    bool reset = cleaned && selected != null && owner.Destroyed && !owner.IsInState &&
                        game.State.Units.All.SequenceEqual(unitsBefore) && game.Player.Party.SequenceEqual(partyBefore) &&
                        game.State.AreaEffects.All.SequenceEqual(areasBefore) &&
                        buffsBefore.All(pair => pair.Key.Buffs.Enumerable.SequenceEqual(pair.Value)) &&
                        positionsBefore.All(pair => pair.Key.Position == pair.Value) &&
                        items.All(item => !item.IsLootable && item.Collection == null) &&
                        game.SelectedAbilityHandler.Ability == null && !_workingSaveSmoke.WriteObserved;
                    Sprints30DispelUiCheck(phase + "-ui-owned-cleanup", reset,
                        new JObject { ["ownerAbsent"] = owner.Destroyed && !owner.IsInState,
                            ["unitsExact"] = game.State.Units.All.SequenceEqual(unitsBefore),
                            ["partyExact"] = game.Player.Party.SequenceEqual(partyBefore),
                            ["areasExact"] = game.State.AreaEffects.All.SequenceEqual(areasBefore),
                            ["originalBuffsExact"] = buffsBefore.All(pair => pair.Key.Buffs.Enumerable.SequenceEqual(pair.Value)),
                            ["ownedItemsAbsentNonlootable"] = items.All(item => !item.IsLootable && item.Collection == null),
                            ["settlingFrames"] = frames, ["writeObserved"] = _workingSaveSmoke.WriteObserved },
                        "The owned actor/caster/items/areas/grants and UI target retire; original world and party remain exact before the next fixed case.");
                    owner = null;
                }
            }
            finally
            {
                try
                {
                    if (masterCaptured && owner != null && !owner.Destroyed) owner.Descriptor.Master = originalMaster;
                    if (factionCaptured && owner != null && !owner.Destroyed)
                    {
                        var part = owner.Get<UnitPartSummonedMonster>();
                        if (part != null) part.IsDirectlyControllable = originalControl;
                        owner.Descriptor.SwitchFactions(originalFaction, false);
                    }
                    if (fixture != null) { bool ignored; EndExpandedSummoningCorrectionFixture(fixture, out ignored); }
                    foreach (var snapshot in uiSettings) snapshot.Restore();
                    _sprints30DispelScreens.Dispose();
                    UnityEngine.Random.state = randomBefore;
                    game.IsPaused = paused;
                }
                finally
                {
                    UnityEngine.Application.logMessageReceived -= ObserveDispelUiError;
                    _sprints30DispelUiListenerRemoved = true;
                }
            }
            bool world = game.State.Units.All.SequenceEqual(unitsBefore) && game.Player.Party.SequenceEqual(partyBefore) &&
                game.State.AreaEffects.All.SequenceEqual(areasBefore) && game.TimeController.GameTime == timeBefore &&
                buffsBefore.All(pair => pair.Key.Buffs.Enumerable.SequenceEqual(pair.Value)) &&
                positionsBefore.All(pair => pair.Key.Position == pair.Value) && !_workingSaveSmoke.WriteObserved;
            Sprints30DispelUiCheck("dispel-ui-world-restored", world,
                new JObject { ["worldExact"] = world, ["gameTimeExact"] = game.TimeController.GameTime == timeBefore,
                    ["writeObserved"] = _workingSaveSmoke.WriteObserved, ["fixedCases"] = 2 },
                "Both UI cases finish with the original world/party/time and no save writes; this remains read-only UI evidence.");
        }
    }
}
