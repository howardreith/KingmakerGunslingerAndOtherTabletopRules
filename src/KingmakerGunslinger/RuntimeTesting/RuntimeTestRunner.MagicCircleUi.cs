using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Assets.UI.LevelUp;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.GameModes;
using Kingmaker.Items;
using Kingmaker.UI;
using Kingmaker.UI.ActionBar;
using Kingmaker.UI.Common;
using Kingmaker.UI.Group;
using Kingmaker.UI.LevelUp;
using Kingmaker.UI.LevelUp.Phase;
using Kingmaker.UI.ServiceWindow;
using Kingmaker.UI.ServiceWindow.CharacterScreen;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UI.Tooltip;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Spells.Teleportation;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerator<int> _circleUiSteps;
        private Stopwatch _circleUiWatch;
        private NativeIconScreenEvidence _circleUiScreens;
        private readonly List<RuntimeTestAssertion> _circleUiAssertions = new List<RuntimeTestAssertion>();
        private readonly List<object> _circleUiCaptures = new List<object>();
        private readonly List<object> _circleUiExceptions = new List<object>();
        private readonly List<object> _circleUiNativePopupExceptions = new List<object>();
        private bool _circleUiNativePopupBaseline;
        private readonly List<string> _circleUiDiagnostics = new List<string>();
        private string CircleUiPath { get { return Path.Combine(_request.EvidenceDirectory, "magic-circle-native-ui.json"); } }
        private void PollMagicCircleUi()
        {
            if (_request.Scenario != RuntimeTestScenarioCatalog.DisposableMagicCircleUi || !_request.ExitAfterCompletion ||
                _workingSaveSmoke == null || !_workingSaveSmoke.Complete || _workingSaveSmoke.WriteObserved)
                throw new InvalidOperationException("Circle UI requires the exact guarded read-only working save and mandatory exit.");
            if (_circleUiWatch == null) _circleUiWatch = Stopwatch.StartNew();
            if (_circleUiWatch.Elapsed.TotalSeconds > _request.CompletionTimeoutSeconds)
                throw new InvalidOperationException("Circle UI qualification timed out.");
            if (LoadingProcess.Instance.IsLoadingInProcess || LoadingProcess.Instance.IsLoadingScreenActive) return;
            if (_circleUiSteps == null) _circleUiSteps = RunCircleUi().GetEnumerator();
            Exception failure = null;
            try { if (_circleUiSteps.MoveNext()) return; }
            catch (Exception error) { failure = error; }
            try { _circleUiSteps.Dispose(); }
            catch (Exception error) { failure = failure == null ? error : new AggregateException(failure, error); }
            _circleUiSteps = null;
            WriteCircleUi(failure?.ToString());
            var result = CreateResult(failure != null ? RuntimeTestStatuses.Error : _circleUiAssertions.All(value => value.Status == "PASS") ?
                RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, _circleUiAssertions, failure?.ToString());
            result.Diagnostics.AddRange(_circleUiDiagnostics);
            _circleUiScreens?.AppendEvidence(result.EvidenceFiles);
            result.EvidenceFiles.Add(CircleUiPath);
            Complete(result);
        }
        private void StopMagicCircleUi(RuntimeTestResult result)
        {
            if (_circleUiSteps == null) return;
            var steps = _circleUiSteps; _circleUiSteps = null;
            string error = "Runner completed before Circle native UI qualification finished.";
            try { steps.Dispose(); }
            catch (Exception failure) { error += " " + failure; result.Status = RuntimeTestStatuses.Error; }
            WriteCircleUi(error); result.Diagnostics.Add(error); _circleUiScreens?.AppendEvidence(result.EvidenceFiles);
        }
        private void WriteCircleUi(string error)
        {
            WriteTeleportationForensicJson(CircleUiPath, new { schemaVersion = 1, runId = _request.RunId, nativeGameVersion = GameVersion.Cached,
                claims = "Native Sorcerer level-up cancellation and committed single known-spell selection; native spellbook, held-touch, active buff and scroll UI. Actual framebuffer screenshots require separate visual inspection. No save writes or uninstall-safety claim.",
                captures = _circleUiCaptures, exceptions = _circleUiExceptions,
                nativeProtectionPopupBaselineExceptions = _circleUiNativePopupExceptions, assertions = _circleUiAssertions,
                diagnostics = _circleUiDiagnostics, saveWriteObserved = _workingSaveSmoke.WriteObserved, error });
        }
        private static JObject CircleUiState(object value)
        {
            // The game's global opt-in resolver drops anonymous inspector
            // properties. Use the existing forensic JSON contract locally.
            return JObject.FromObject(value, Newtonsoft.Json.JsonSerializer.Create(new Newtonsoft.Json.JsonSerializerSettings {
                ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver(),
                PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.None,
                TypeNameHandling = Newtonsoft.Json.TypeNameHandling.None }));
        }
        private void CircleUiAssert(string id, string expected, string observed, bool pass)
        { _circleUiAssertions.Add(Assertion("circle-ui-" + id, expected, observed, pass, CircleUiPath)); }
        private void CircleUiCapture(string stage, object state)
        { _circleUiCaptures.Add(new { stage, frame = Time.frameCount, state }); WriteCircleUi(null); }
        private void ObserveCircleUiException(string message, string stack, LogType type)
        {
            if (type != LogType.Exception && !(type == LogType.Error && message.Contains("Exception"))) return;
            var observed = new { message, stack, type = type.ToString(), frame = Time.frameCount };
            // A native Protection popup is the control for the first-use widget
            // pool. Preserve this exact native/foreign initialization exception
            // as separate evidence; Circle popups never receive this exception
            // allowance and no patch, widget, or exception is silently repaired.
            if (_circleUiNativePopupBaseline && type == LogType.Exception &&
                message.StartsWith("NullReferenceException:", StringComparison.Ordinal) &&
                stack.StartsWith("Kingmaker.UI.ActionBar.ActionBarSlot.Hover ", StringComparison.Ordinal) &&
                stack.Contains("UnityEngine.Transform:SetParent") &&
                stack.Contains("RacesUnleashed.ActionBarSpellsGroup_FillSlots_Patch:Prefix"))
                _circleUiNativePopupExceptions.Add(observed);
            else _circleUiExceptions.Add(observed);
        }
        private IEnumerable<int> RunCircleUi()
        {
            var game = Game.Instance; var ui = game.UI;
            var backend = ui.LevelUpController;
            if (!_context.FeatureModules.Active.MagicCircleSpells || game.IsControllerGamepad || game.CurrentMode != GameModeType.Default ||
                game.SelectedAbilityHandler.Ability != null || ui.CharacterBuildController == null || ui.CharacterBuildController.IsShow || ui.CharacterBuildController.LevelUpController != null ||
                backend != null && (!backend.AutoCommit || !ReferenceEquals(backend.Unit, backend.Preview)) ||
                ui.ServiceWindow == null || ui.ServiceWindow.WindowTabs.IsShow || ui.DescriptionController.DescWindow.gameObject.activeInHierarchy ||
                DialogMessageBox.Instance == null || DialogMessageBox.Instance.IsShown || SettingsRoot.Instance.AutoLevelup.CurrentValue != AutolevelupState.Off)
                throw new InvalidOperationException("Circle UI requires idle native desktop local-area UI with no active level-up preview, modal or service window.");
            bool enhanced = _context.FeatureModules.Active.ProtectionFromAlignmentControlImmunity;
            CircleUiAssert("startup-descriptions", "all twenty descriptions retain ordinary defenses and include added control protection only when enabled",
                "sharedControlEnhancement=" + enhanced, BlueprintBootstrap.MagicCircles.All(circle =>
                    new[] { circle.Spell.Description, circle.Delivery.Description, circle.Carrier.Description, circle.Recipient.Description, circle.Scroll.Description }
                    .All(description => description.Contains("prevents new charm") == enhanced && description.Contains("+2 deflection") && description.Contains("+2 resistance"))));
            var units = game.State.Units.All.ToArray(); var areas = game.State.AreaEffects.All.ToArray();
            var buffs = units.Select(unit => unit.Buffs.Enumerable.ToArray()).ToArray();
            var positions = units.Select(unit => unit.Position).ToArray();
            var party = game.Player.Party.ToArray(); var selection = ui.SelectionManagerPC.SelectedUnits.ToArray();
            var settings = party.Select(unit => new TeleportationUiSettingsFixture(unit.UISettings)).ToArray();
            var selectedGroup = GroupController.Instance.GetCurrentCharacter();
            var actionGroups = ActionBarManager.Instance.Group.GroupElements.ToArray();
            var groupToggles = actionGroups.Select(value => value.ToggleState).ToArray();
            var cameraRig = TeleportationCastingCamera();
            var cameraPosition = cameraRig.transform.position; var cameraTarget = cameraRig.GetPosition();
            var localMapField = typeof(Kingmaker.View.CameraRig).GetField("m_LocalMapArea", BindingFlags.Instance | BindingFlags.NonPublic);
            var originalLocalMap = localMapField.GetValue(cameraRig);
            var time = game.Player.GameTime; bool paused = game.IsPaused;
            _circleUiScreens = new NativeIconScreenEvidence(_request);
            Application.logMessageReceived += ObserveCircleUiException;
            try {
                game.IsPaused = true;
                foreach (int frame in CircleLearnAndPresent()) yield return frame;
                GroupController.Instance.SelectUnit(selectedGroup);
                ui.SelectionManagerPC.MultiSelect(selection.Select(value => value.View).ToArray(), false);
                for (int frame = 0; frame < 12; frame++) yield return 0;
                foreach (int frame in CircleScrollUi()) yield return frame;
            }
            finally {
                ui.DescriptionController.HandleCloseDescriptionWindow(ui.DescriptionController.DescWindow);
                GroupController.Instance.SelectUnit(selectedGroup);
                ui.SelectionManagerPC.MultiSelect(selection.Select(unit => unit.View).ToArray(), false);
                foreach (var snapshot in settings) snapshot.Restore();
                for (int index = 0; index < actionGroups.Length; index++) actionGroups[index].Toggle(groupToggles[index], true);
                cameraRig.ScrollToImmediately(cameraTarget); cameraRig.transform.position = cameraPosition;
                localMapField.SetValue(cameraRig, originalLocalMap);
                CircleUiAssert("camera-cleanup", "native camera framing and target restored exactly", "position=" + cameraRig.transform.position,
                    cameraRig.transform.position == cameraPosition && cameraRig.GetPosition() == cameraTarget && ReferenceEquals(localMapField.GetValue(cameraRig), originalLocalMap));
                game.IsPaused = paused; _circleUiScreens.Dispose();
                Application.logMessageReceived -= ObserveCircleUiException;
                CircleUiAssert("exceptions", "no native or mod exception through UI cleanup", "count=" + _circleUiExceptions.Count, _circleUiExceptions.Count == 0);
                CircleUiCapture("world-cleanup-detail", new {
                    units = game.State.Units.All.SequenceEqual(units), party = game.Player.Party.SequenceEqual(party),
                    areas = game.State.AreaEffects.All.SequenceEqual(areas), clock = game.Player.GameTime == time,
                    settings = settings.All(value => value.IsRestored()), selection = ui.SelectionManagerPC.SelectedUnits.SequenceEqual(selection),
                    serviceWindow = ui.ServiceWindow.WindowTabs.IsShow, description = ui.DescriptionController.DescWindow.gameObject.activeInHierarchy,
                    extraUnits = game.State.Units.All.Except(units).Select(value => new { value.UniqueId, value.Descriptor.CustomName }).ToArray(),
                    changedUnits = units.Select((unit, index) => new { unit.UniqueId, position = unit.Position == positions[index],
                        buffs = unit.Buffs.Enumerable.SequenceEqual(buffs[index]),
                        added = unit.Buffs.Enumerable.Except(buffs[index]).Select(value => value.Blueprint.AssetGuid).ToArray(),
                        removed = buffs[index].Except(unit.Buffs.Enumerable).Select(value => value.Blueprint.AssetGuid).ToArray() })
                        .Where(value => !value.position || !value.buffs).ToArray() });
                CircleUiAssert("world-cleanup", "original units, party, areas, buff instances, positions, UI, time and zero save writes", "units=" + game.State.Units.All.Count(),
                    game.State.Units.All.SequenceEqual(units) && game.State.AreaEffects.All.SequenceEqual(areas) && game.Player.Party.SequenceEqual(party) &&
                    units.Select((unit, index) => unit.Buffs.Enumerable.SequenceEqual(buffs[index]) && unit.Position == positions[index]).All(value => value) &&
                    settings.All(value => value.IsRestored()) && ui.SelectionManagerPC.SelectedUnits.SequenceEqual(selection) && game.Player.GameTime == time &&
                    !ui.ServiceWindow.WindowTabs.IsShow && !ui.DescriptionController.DescWindow.gameObject.activeInHierarchy && !_workingSaveSmoke.WriteObserved);
            }
        }
        private IEnumerable<int> CirclePresentLearnedActor(UnitEntityData unit, Spellbook learnedBook, Action<string> captureOwnedItems)
        {
            var game = Game.Instance; var ui = game.UI; var player = game.Player;
            var actors = new List<UnitEntityData> { unit }; var prototypes = new List<BlueprintUnit>();
            var fixture = new TeleportResourceFixtureOwner(unit);
            var originalAreas = game.State.AreaEffects.All.ToArray();
            var originalGroup = GroupController.Instance.GetCurrentCharacter();
            var sheet = ui.ServiceWindow.WindowTabs.SubWindowsList.Select(pair => pair.SubWindow).OfType<CharacterScreenController>().Single();
            var characterField = typeof(CharacterScreenController).GetField("m_CurrentCharacter", BindingFlags.Instance | BindingFlags.NonPublic);
            var sectionField = typeof(CharacterScreenController).GetField("m_CurrentSection", BindingFlags.Instance | BindingFlags.NonPublic);
            var originalCharacter = characterField.GetValue(sheet); var originalSection = sectionField.GetValue(sheet);
            var restoreCharacter = originalCharacter as UnitDescriptor ?? originalGroup?.Descriptor;
            if (restoreCharacter == null) throw new InvalidOperationException("Native sheet has no unambiguous original owner.");
            UnitEntityData bearer = null;
            try {
                bearer = CircleSpawn("UiBearer", unit.Position + new Vector3(.5f, 0, 0), unit, actors, prototypes);
                player.PartyCharacters.Add(bearer); player.InvalidateCharacterLists(); player.UpdateCharacterLists();
                captureOwnedItems("circle-ui-bearer-registration-items");
                CircleSynchronize(actors);
                var evil = BlueprintBootstrap.MagicCircles.Single(value => value.Alignment == "Evil");
                learnedBook.Rest(); int slots = learnedBook.GetSpontaneousSlots(3);
                ui.SelectionManagerPC.SelectUnit(unit.View, true, true, false);
                for (int frame = 0; frame < 12; frame++) yield return 0;
                var nativeProtection = BlueprintLibraryLookup.RequireExact<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>(
                    BlueprintBootstrap.Library, "433b1faf4d02cc34abb0ade5ceda47c4", "native Protection popup control");
                var nativeEvil = BlueprintLibraryLookup.RequireExact<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>(
                    BlueprintBootstrap.Library, "eee384c813b6d74498d1b9cc720d61f4", "native Protection variant control");
                if (!nativeProtection.HasVariant(nativeEvil) || nativeProtection.Variants.Length != 4)
                    throw new InvalidOperationException("Native Protection popup control changed.");
                if (!learnedBook.IsKnown(nativeProtection)) learnedBook.AddKnown(1, nativeProtection, true);
                // AddKnown is mechanical; this menu caches rows for its current
                // selected unit. Use ordinary character selection to refresh it.
                ui.SelectionManagerPC.SelectUnit(bearer.View, true, true, false);
                for (int frame = 0; frame < 12; frame++) yield return 0;
                ui.SelectionManagerPC.SelectUnit(unit.View, true, true, false);
                for (int frame = 0; frame < 12; frame++) yield return 0;
                _circleUiNativePopupBaseline = true;
                try {
                    foreach (int frame in CircleChooseGroupedUi(new AbilityData(nativeProtection, learnedBook), nativeEvil, selected => { })) yield return frame;
                } finally { _circleUiNativePopupBaseline = false; }
                game.ClickEventsController.ClearPointerMode(); game.SelectedAbilityHandler.DropAbility();
                CircleUiAssert("native-protection-popup-control", "native four-variant Protection popup; first-use hover exceptions separately attributed and retained",
                    "nativeInitializationExceptions=" + _circleUiNativePopupExceptions.Count,
                    _circleUiNativePopupExceptions.Count <= nativeProtection.Variants.Length);
                AbilityData learnedChoice = null;
                foreach (int frame in CircleChooseGroupedUi(new AbilityData(MagicCircleBlueprints.Family, learnedBook),
                    evil.Spell, selected => learnedChoice = selected)) yield return frame;
                game.ClickEventsController.ClearPointerMode(); game.SelectedAbilityHandler.DropAbility();
                CircleCast(unit, bearer, learnedChoice, _circleUiDiagnostics);
                var learnedCarrier = CircleBuffs(bearer, evil.Carrier).Single(); var learnedArea = CircleArea(learnedCarrier);
                CircleRefresh(learnedArea, actors);
                CircleUiAssert("newly-learned-cast", "committed native known spell casts on another bearer for one normal slot", "slots=" + slots + "->" + learnedBook.GetSpontaneousSlots(3),
                    learnedBook.GetSpontaneousSlots(3) == slots - 1 && ReferenceEquals(learnedCarrier.Context.MaybeCaster, unit) &&
                    learnedCarrier.Context.Params.CasterLevel == learnedBook.CasterLevel && learnedBook.IsKnown(MagicCircleBlueprints.Family) && BlueprintBootstrap.MagicCircles.All(value => !learnedBook.IsKnown(value.Spell)));
                learnedCarrier.Remove(); CircleRefresh(learnedArea, actors);
                var wizard = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library, "ba34257984f4c41408ce1dc2004e342e", "native Wizard UI book");
                var book = fixture.AddBook(wizard.Spellbook, 8);
                book.AddKnown(3, MagicCircleBlueprints.Family, true);
                book.Rest();
                ui.SelectionManagerPC.SelectUnit(unit.View, true, true, false);
                for (int frame = 0; frame < 12; frame++) yield return 0;
                ui.ServiceWindow.HandleOpenSpellbook();
                for (int frame = 0; frame < 60; frame++) yield return 0;
                GroupController.Instance.SelectUnit(unit);
                for (int frame = 0; frame < 12; frame++) yield return 0;
                foreach (int frame in CircleBookUiEntry(ui.SpellBookController, new TeleportUiSpellEntry(book, MagicCircleBlueprints.Family, 3))) yield return frame;
                ui.ServiceWindow.HandleOpenSpellbook();
                for (int frame = 0; frame < 30; frame++) yield return 0;
                foreach (var circle in BlueprintBootstrap.MagicCircles) {
                    // ActionBarManager.Set reads UIUtility.GetCurrentCharacter,
                    // whose native group owner still names the prior bearer.
                    GroupController.Instance.SelectUnit(unit);
                    ui.SelectionManagerPC.SelectUnit(unit.View, true, true, false);
                    for (int frame = 0; frame < 12; frame++) yield return 0;
                    book.Rest();
                    var prepared = book.GetMemorizedSpells(3).First(value => value.Available && value.Spell.Blueprint == MagicCircleBlueprints.Family);
                    AbilityData data = null;
                    foreach (int frame in CircleChooseGroupedUi(prepared.Spell, circle.Spell, selected => data = selected)) yield return frame;
                    // The popup hide event dismisses the targeting overlay. Start
                    // a fresh native selection event for the independent preview
                    // check instead of reassigning an already selected instance.
                    game.ClickEventsController.ClearPointerMode(); game.SelectedAbilityHandler.DropAbility();
                    // Native player ability selection publishes the same event used
                    // by the installed AoE preview. Never draw our own circle.
                    TeleportationCastingCamera().ScrollToImmediately(unit.Position);
                    for (int frame = 0; frame < 12; frame++) yield return 0;
                    game.SelectedAbilityHandler.SetAbility(data);
                    try {
                        for (int frame = 0; frame < 12; frame++) yield return 0;
                        var preview = UnityEngine.Object.FindObjectsOfType<Kingmaker.UI.AbilityTarget.AbilityAoERange>()
                            .Single(value => value.isActiveAndEnabled && value.Range.activeInHierarchy);
                        var scale = preview.Range.transform.localScale;
                        var viewport = TeleportationCastingCamera().Camera.WorldToViewportPoint(preview.Range.transform.position);
                        float diameter = circle.Area.Size.Meters * 2;
                        CircleUiCapture("native-radius-" + circle.Alignment, new { x = scale.x, z = scale.z, diameter, viewport, world = preview.Range.transform.position,
                            spell = data.Blueprint.AssetGuid, selected = game.SelectedAbilityHandler.Ability.Blueprint.AssetGuid });
                        CircleUiAssert("native-radius-" + circle.Alignment, "native selection displays the same ten-foot mechanical radius", "scale=" + scale,
                            ReferenceEquals(game.SelectedAbilityHandler.Ability, data) && Math.Abs(scale.x - diameter) < .001f && Math.Abs(scale.z - diameter) < .001f);
                        foreach (int frame in _circleUiScreens.Capture("native-radius-" + circle.Alignment,
                            CircleUiState(new { diameter, scaleX = scale.x, scaleZ = scale.z, viewport, world = preview.Range.transform.position, spell = circle.Spell.AssetGuid }),
                            () => preview.Range.activeInHierarchy && ReferenceEquals(game.SelectedAbilityHandler.Ability, data) && !_workingSaveSmoke.WriteObserved)) yield return frame;
                    }
                    finally { game.ClickEventsController.ClearPointerMode(); game.SelectedAbilityHandler.DropAbility(); }
                    for (int frame = 0; frame < 6; frame++) yield return 0;
                    var root = new UnitUseAbility(data, new TargetWrapper(bearer));
                    var queueBefore = unit.Commands.Queue.ToArray();
                    if (!data.IsAvailable || !root.CanStart) throw new InvalidOperationException("Prepared Circle UI cast unavailable.");
                    root.IgnoreCooldown(TimeSpan.Zero); unit.Commands.Run(root); root.Start(); CircleCompleteCommand(root, _circleUiDiagnostics);
                    var touch = unit.Get<UnitPartTouch>();
                    var held = unit.Commands.Queue.Except(queueBefore).OfType<UnitUseAbility>().Single(value => value.Spell.Blueprint == circle.Delivery);
                    for (int frame = 0; frame < 12; frame++) yield return 0;
                    // Held charges appear in the native abilities popup. A fresh
                    // multiclass fixture need not pin every prepared root to its
                    // first visible shortcut bar; do not manufacture shortcuts.
                    var heldGroup = ActionBarManager.Instance.Group.GroupElements.Single(value =>
                        value.GetComponentsInChildren<ActionBarSlot>(true).Any(row =>
                            (row.MechanicSlot?.GetContentData() as AbilityData)?.Blueprint == circle.Delivery));
                    if (!heldGroup.ToggleState) heldGroup.OnClick();
                    for (int frame = 0; frame < 30; frame++) yield return 0;
                    var rows = heldGroup.GetComponentsInChildren<ActionBarSlot>(true).Where(row => row.gameObject.activeInHierarchy &&
                        (row.MechanicSlot?.GetContentData() as AbilityData)?.Blueprint == circle.Delivery).ToArray();
                    CircleUiCapture("native-held-touch-" + circle.Alignment, new { selectedGroup = GroupController.Instance.GetCurrentCharacter()?.UniqueId,
                        selectedUnits = ui.SelectionManagerPC.SelectedUnits.Select(value => value.UniqueId).ToArray(),
                        actionBarOwner = (typeof(ActionBarManager).GetField("m_Selected", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(ActionBarManager.Instance) as UnitEntityData)?.UniqueId,
                        nativeRows = ActionBarManager.Instance.GetComponentsInChildren<ActionBarSlot>(true).Select(value => new { visible = value.gameObject.activeInHierarchy,
                            guid = (value.MechanicSlot?.GetContentData() as AbilityData)?.Blueprint.AssetGuid, sprite = value.Icon?.sprite?.name }).ToArray(), delivery = touch?.Ability.Data.Blueprint.AssetGuid, root = data.Blueprint.AssetGuid,
                        rows = rows.Select(row => new { sprite = row.Icon.sprite?.name, exact = ReferenceEquals(row.Icon.sprite, circle.Delivery.Icon) }).ToArray(),
                        originalSource = touch?.Ability.Data.StickyTouch?.Blueprint.AssetGuid, context = "actual native abilities popup renders the held-delivery identity while preserving its originating prepared spell" });
                    CircleUiAssert("held-touch-" + circle.Alignment, "actual held delivery retains original spell and canonical icon in the native abilities popup", "rows=" + rows.Length,
                        touch != null && touch.Ability.Data.Blueprint == circle.Delivery && ReferenceEquals(touch.Ability.Data.StickyTouch, data) && rows.Length > 0 &&
                        rows.All(row => ReferenceEquals(row.Icon.sprite, circle.Delivery.Icon)));
                    if (rows.Length == 0) throw new InvalidOperationException("Native abilities popup has no visible held Circle charge.");
                    foreach (int frame in _circleUiScreens.Capture("held-touch-" + circle.Alignment, CircleUiState(new { spell = circle.Spell.AssetGuid,
                        delivery = circle.Delivery.AssetGuid, sprite = rows[0].Icon.sprite.name }), () => !_workingSaveSmoke.WriteObserved &&
                            ReferenceEquals(unit.Get<UnitPartTouch>(), touch) && rows[0].gameObject.activeInHierarchy)) yield return frame;
                    if (heldGroup.ToggleState) heldGroup.OnClick();
                    unit.Commands.RemoveFinishedAndUpdateQueue();
                    if (!unit.Commands.Raw.Contains(held) || !held.CanStart) throw new InvalidOperationException("Held UI delivery did not enter its native command slot.");
                    held.Start(); CircleCompleteCommand(held, _circleUiDiagnostics); unit.Commands.RemoveFinishedAndUpdateQueue();
                    var carrier = CircleBuffs(bearer, circle.Carrier).Single(); var area = CircleArea(carrier);
                    CircleRefresh(area, actors); CircleRefresh(area, actors);
                    CircleUiAssert("moving-boundary-" + circle.Alignment, "active spell has one correctly colored renderer at the actual radius",
                        "area=" + area.UniqueId + ";" + CircleBoundaryState(area), CircleBoundaryMatches(area));
                    TeleportationCastingCamera().ScrollToImmediately(bearer.Position);
                    for (int frame = 0; frame < 12; frame++) yield return 0;
                    foreach (int frame in _circleUiScreens.Capture("active-moving-boundary-" + circle.Alignment,
                        CircleUiState(new { area = area.UniqueId, bearer = bearer.UniqueId, radius = circle.Area.Size.Meters,
                            alignment = circle.Alignment, role = "supporting native battlefield visual; renderer state is asserted separately" }),
                        () => carrier.Active && !area.IsEnded && area.View != null && !_workingSaveSmoke.WriteObserved)) yield return frame;
                    ui.ServiceWindow.HandleOpenCharScreen();
                    for (int frame = 0; frame < 60; frame++) yield return 0;
                    GroupController.Instance.SelectUnit(bearer); sheet.SetCharacter(bearer.Descriptor);
                    if (sheet.BuffsAndConditions.AlwaysHidden || sheet.BuffsAndConditions.SectionGroupIndex.Count == 0)
                        throw new InvalidOperationException("Native buffs section has no visible menu membership.");
                    sheet.ShowSection(sheet.BuffsAndConditions.SectionGroupIndex[0]);
                    sheet.BuffsAndConditions.SetDirty(); sheet.Refresh();
                    for (int frame = 0; frame < 30; frame++) yield return 0;
                    CircleUiCapture("native-buff-section-" + circle.Alignment, new {
                        shown = sheet.IsShow, sectionShown = sheet.BuffsAndConditions.IsShowed, section = sectionField.GetValue(sheet),
                        owner = (characterField.GetValue(sheet) as UnitDescriptor)?.Unit.UniqueId,
                        rows = sheet.BuffsAndConditions.GetComponentsInChildren<CharSComponentBuffSlot>(true).Select(value => new {
                            shown = value.gameObject.activeInHierarchy, guid = value.Buff?.Blueprint.AssetGuid, owner = value.Buff?.Owner.Unit.UniqueId }).ToArray() });
                    foreach (var buff in new[] { carrier, CircleBuffs(bearer, circle.Recipient).Single() }) {
                        var row = sheet.BuffsAndConditions.GetComponentsInChildren<CharSComponentBuffSlot>(true).Single(value => value.gameObject.activeInHierarchy && ReferenceEquals(value.Buff, buff));
                        Func<bool> owned = () => game.IsPaused && sheet.IsShow && ReferenceEquals(row.Buff, buff) && buff.Active && !_workingSaveSmoke.WriteObserved;
                        Func<JObject> describe = () => CircleUiState(new { surface = "native-active-circle-buff", targetRow = new { guid = buff.Blueprint.AssetGuid,
                            name = row.Name.GetParsedText(), icon = row.Icon.sprite.name, expectedIcon = buff.Blueprint.Icon.name,
                            timer = row.EventSting.text, remainingSeconds = buff.TimeLeft.TotalSeconds, description = row.Description,
                            nativeAlpha = row.CanvasGroup.alpha, truncated = row.Name.isTextTruncated, owner = bearer.UniqueId, area = area.UniqueId } });
                        foreach (int frame in _circleUiScreens.CaptureRow("buff-" + buff.Blueprint.AssetGuid, (RectTransform)row.transform, describe, owned)) yield return frame;
                        CircleUiAssert("buff-" + buff.Blueprint.AssetGuid, "native active row shows exact painting, name, description and carrier timer or proximity text", describe().ToString(),
                            ReferenceEquals(row.Icon.sprite, buff.Blueprint.Icon) && row.Name.GetParsedText() == buff.Name && !row.Name.isTextTruncated &&
                            row.Description == buff.Description && row.CanvasGroup.alpha > 0 && (ReferenceEquals(buff, carrier) ?
                                row.EventSting.text == UIUtility.GetBuffDurationString(buff.TimeLeft) : buff.Description.Contains("within")));
                        var tooltip = row.GetComponent<TooltipTrigger>() ?? row.GetComponentInChildren<TooltipTrigger>(true);
                        if (tooltip == null) throw new InvalidOperationException("Native buff row has no description trigger.");
                        tooltip.OpenDescriptionWindow();
                        for (int frame = 0; frame < 8; frame++) yield return 0;
                        var description = ui.DescriptionController.DescWindow;
                        foreach (int frame in _circleUiScreens.Capture("buff-description-" + buff.Blueprint.AssetGuid, describe(),
                            () => owned() && description.gameObject.activeInHierarchy)) yield return frame;
                        CircleUiAssert("buff-description-" + buff.Blueprint.AssetGuid, "native tooltip renders configuration-accurate full description", "shown=" + description.gameObject.activeInHierarchy,
                            description.GetComponentsInChildren<TextMeshProUGUI>(true).Any(value => value.isActiveAndEnabled && !value.isTextTruncated && value.GetParsedText() == buff.Description));
                        tooltip.CloseDescriptionWindow();
                    }
                    ui.ServiceWindow.HandleOpenCharScreen();
                    for (int frame = 0; frame < 30; frame++) yield return 0;
                    carrier.Remove(); CircleRefresh(area, actors);
                }
            }
            finally {
                ui.DescriptionController.HandleCloseDescriptionWindow(ui.DescriptionController.DescWindow);
                if (ui.SpellBookController.IsShow && ui.ServiceWindow.WindowTabs.IsShow) ui.ServiceWindow.HandleOpenSpellbook();
                sheet.SetCharacter(restoreCharacter); sheet.ShowSection((int)originalSection);
                if (sheet.IsShow && ui.ServiceWindow.WindowTabs.IsShow) ui.ServiceWindow.HandleOpenCharScreen();
                foreach (var actor in actors)
                    foreach (var circle in BlueprintBootstrap.MagicCircles)
                        foreach (var buff in CircleBuffs(actor, circle.Carrier)) buff.Remove();
                foreach (var area in game.State.AreaEffects.All.Except(originalAreas).ToArray()) {
                    if (!BlueprintBootstrap.MagicCircles.Any(value => ReferenceEquals(value.Area, area.Blueprint))) throw new InvalidOperationException("Unexpected foreign area appeared during Circle UI.");
                    area.ForceEnd(); area.Tick();
                }
                fixture.Restore();
                if (bearer != null) { player.PartyCharacters.RemoveAll(value => value.UniqueId == bearer.UniqueId); bearer.Destroy(); }
                game.EntityDestroyer.Tick(); game.EntityDestroyer.Tick();
                foreach (var prototype in prototypes) UnityEngine.Object.Destroy(prototype);
                player.InvalidateCharacterLists(); player.UpdateCharacterLists();
                GroupController.Instance.SelectUnit(originalGroup);
                characterField.SetValue(sheet, originalCharacter); sectionField.SetValue(sheet, originalSection);
            }
        }
        private IEnumerable<int> CircleLearnAndPresent()
        {
            var sorcerer = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library, "b3a505fb61437dc4097f43c3f8f9a4cf", "native Sorcerer");
            if (_request.Scenario != RuntimeTestScenarioCatalog.DisposableMagicCircleUi || !_request.ExitAfterCompletion || _workingSaveSmoke.WriteObserved)
                throw new InvalidOperationException("Sorcerer learning requires the guarded disposable level-up request.");
            var game = Game.Instance; var player = game.Player; var ui = game.UI;
            var presenter = ui.CharacterBuildController;
            var priorBackend = ui.LevelUpController; var priorPresenter = presenter.Unit;
            var originalParty = player.Party.ToArray(); var originalPartyRefs = player.PartyCharacters.ToArray();
            var originalCross = player.CrossSceneState.AllEntityData.ToArray();
            var originalItems = player.Inventory.Items.ToArray();
            var originalCounts = originalItems.Select(value => value.Count).ToArray();
            var starterDeltas = new Dictionary<Kingmaker.Items.ItemEntity, int>();
            Action<string> captureStarterItems = stage => {
                foreach (var item in player.Inventory.Items)
                {
                    int index = Array.IndexOf(originalItems, item);
                    int delta = item.Count - (index < 0 ? 0 : originalCounts[index]);
                    if (delta > 0) starterDeltas[item] = delta;
                }
                CircleUiCapture(stage, starterDeltas.Select(value => new {
                    item = value.Key.Blueprint.AssetGuid, instance = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value.Key), count = value.Value }).ToArray());
            };
            long originalMoney = player.Money;
            bool originalPause = game.IsPaused;
            UnitEntityData unit = null; LevelUpController backend = null;
            bool committed = false;
            try
            {
                game.IsPaused = true;
                var anchor = originalParty.First(value => value.View != null && value.Descriptor.Progression.Race != null);
                var dollState = new DollState();
                dollState.SetGender(anchor.Descriptor.Gender); dollState.SetRace(anchor.Descriptor.Progression.Race); dollState.SetClass(sorcerer);
                var doll = dollState.CreateData();
                var view = doll.CreateUnitView(false);
                if (view == null) throw new InvalidOperationException("Native Sorcerer fixture has no real character view.");
                view.Blueprint = game.BlueprintRoot.DefaultPlayerCharacter; view.UniqueId = Guid.NewGuid().ToString();
                view.transform.position = anchor.Position;
                var pending = (System.Collections.IList)game.EntityCreator.GetType().GetField("m_ToCreate", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(game.EntityCreator);
                if (pending.Count != 0) throw new InvalidOperationException("Unrelated native entity creation is pending.");
                unit = game.EntityCreator.SpawnEntityWithView(view, player.CrossSceneState) as UnitEntityData;
                if (unit == null) throw new InvalidOperationException("Native Sorcerer entity ownership transfer failed.");
                game.EntityCreator.Tick();
                if (!ReferenceEquals(unit.HoldingState, player.CrossSceneState) || pending.Count != 0)
                    throw new InvalidOperationException("Native Sorcerer entity registration differs.");
                unit.Descriptor.Doll = doll; unit.Descriptor.CustomGender = anchor.Descriptor.Gender;
                unit.Descriptor.CustomName = "KMG Sorcerer Learning";
                unit.Stats.Charisma.BaseValue = 18;
                unit.Stats.Intelligence.BaseValue = 10;
                unit.Stats.Wisdom.BaseValue = 12;
                unit.Descriptor.TurnOn();
                for (int level = 0; level < 5; level++)
                {
                    backend = LevelUpController.StartWithoutAssigningStaticInstance(unit.Descriptor, false, null, null,
                        level == 0 ? LevelUpState.CharBuildMode.CharGen : LevelUpState.CharBuildMode.LevelUp);
                    if (level == 0)
                    {
                        backend.SelectRace(anchor.Descriptor.Progression.Race);
                        backend.SelectGender(anchor.Descriptor.Gender);
                        backend.SelectAlignment(anchor.Descriptor.Alignment.Value);
                    }
                    if (!backend.SelectClass(sorcerer)) throw new InvalidOperationException("Native Sorcerer seed class selection failed.");
                    FillOracleNativeChoices(backend);
                    typeof(LevelUpController).GetMethod("ApplyLevelup", BindingFlags.Instance | BindingFlags.NonPublic)
                        .Invoke(backend, new object[] { unit.Descriptor });
                    backend.Cancel(); backend = null;
                    if (unit.Descriptor.Progression.GetClassLevel(sorcerer) != level + 1)
                        throw new InvalidOperationException("Native Sorcerer seed progression did not advance exactly once.");
                }
                captureStarterItems("sorcerer-seed-starter-items");
                var learned = MagicCircleBlueprints.Family;
                var book = unit.Descriptor.GetSpellbook(sorcerer.Spellbook);
                var table = sorcerer.Spellbook.SpellsKnown;
                CircleUiCapture("sorcerer-native-progression", new {
                    unitId = unit.UniqueId, classId = sorcerer.AssetGuid,
                    race = new { id = unit.Descriptor.Progression.Race.AssetGuid, name = unit.Descriptor.Progression.Race.name },
                    gender = unit.Descriptor.Gender.ToString(), alignment = unit.Descriptor.Alignment.Value.ToString(),
                    charisma = new { baseValue = unit.Stats.Charisma.BaseValue, effective = unit.Stats.Charisma.ModifiedValue },
                    nativeFeatures = unit.Descriptor.Progression.Features.Enumerable.Select(value => new { id = value.Blueprint.AssetGuid, name = value.Blueprint.name }).ToArray(),
                    classLevel = unit.Descriptor.Progression.GetClassLevel(sorcerer),
                    characterLevel = unit.Descriptor.Progression.CharacterLevel, book = book.Blueprint.AssetGuid,
                    book.CasterLevel, archetypes = unit.Descriptor.Progression.GetClassData(sorcerer).Archetypes.Select(value => value.AssetGuid).ToArray(),
                    thirdKnownByLevel = Enumerable.Range(5, 4).Select(level => new { level, count = table.GetCount(level, 3) ?? 0 }).ToArray(),
                    choices5to6 = (table.GetCount(6, 3) ?? 0) - (table.GetCount(5, 3) ?? 0),
                    choices7to8 = (table.GetCount(8, 3) ?? 0) - (table.GetCount(7, 3) ?? 0),
                    knownBefore = book.IsKnown(learned), known = book.GetAllKnownSpells().Select(value => new { id = value.Blueprint.AssetGuid, value.SpellLevel }).ToArray()
                });
                if (book.CasterLevel != 5 || book.IsKnown(learned) || unit.Descriptor.Progression.CharacterLevel != 5)
                    throw new InvalidOperationException("The native Sorcerer 5 learning precondition differs.");
                typeof(UnitProgressionData).GetProperty("Experience").SetValue(unit.Descriptor.Progression,
                    game.BlueprintRoot.Progression.XPTable.GetBonus(6), null);
                for (int attempt = 0; attempt < 2; attempt++)
                {
                    committed = false;
                    string before = TeleportResourceFingerprint(book);
                    int successes = 0;
                    presenter.HandleLevelUpStart(unit.Descriptor, null, () => successes++);
                    backend = presenter.LevelUpController;
                    if (backend == null || backend.AutoCommit || ReferenceEquals(backend.Preview, unit.Descriptor))
                        throw new InvalidOperationException("Native Sorcerer level-up did not create an independent preview.");
                    for (int frame = 0; frame < 15; frame++) yield return 0;
                    presenter.SetClass(sorcerer); CompleteTeleportLevelUpPrerequisites(presenter);
                    var selection = backend.State.SpellSelections.Single(value => value.Spellbook == sorcerer.Spellbook && value.SpellList == sorcerer.Spellbook.SpellList);
                    if (selection.LevelCount[3].SpellSelections.Length != 1 || selection.ExtraSelected != null && selection.ExtraSelected.Length != 0)
                        throw new InvalidOperationException("Sorcerer learning must use exactly one normal third-level choice.");
                    presenter.SetPhase((int)CharBPhase.Type.Spells);
                    foreach (int tick in WaitTeleportLevelUpUi(() => presenter.GetComponentsInChildren<CharBSelectionSwitchItem>(true).Any(value =>
                        value.gameObject.activeInHierarchy && value.SpellLevel == 3 && value.SpellSelectionData != null && value.SpellSelectionData.Spellbook == sorcerer.Spellbook), "Sorcerer third-level slot")) yield return tick;
                    presenter.GetComponentsInChildren<CharBSelectionSwitchItem>(true).Single(value => value.gameObject.activeInHierarchy &&
                        value.SpellLevel == 3 && value.SpellIndex == 0 && value.SpellSelectionData != null && value.SpellSelectionData.Spellbook == sorcerer.Spellbook).Toggle.isOn = true;
                    Func<CharBuildSelectorItem[]> rows = () => presenter.GetComponentsInChildren<CharBuildSelectorItem>(true).Where(value =>
                        value.gameObject.activeInHierarchy && value.BlueprintAbility == learned && value.Toggle.interactable).ToArray();
                    foreach (int tick in WaitTeleportLevelUpUi(() => rows().Length == 1, "one enabled native Sorcerer Circle candidate", 8)) yield return tick;
                    var row = rows().Single();
                    bool label = row.GetComponentsInChildren<TextMeshProUGUI>(true).Any(value => value.isActiveAndEnabled && !value.isTextTruncated &&
                        string.Equals(value.GetParsedText(), learned.Name, StringComparison.OrdinalIgnoreCase));
                    CircleUiAssert("sorcerer-real-candidate-" + attempt,
                        "Sorcerer 5 to 6 displays one enabled canonical Circle candidate at level 3 in the native selector",
                        "rows=" + rows().Length + ";level=" + row.SpellLevel + ";label=" + label,
                        label && row.SpellLevel == 3 && !book.IsKnown(learned) &&
                        !presenter.GetComponentsInChildren<CharBuildSelectorItem>(true).Any(value => value.gameObject.activeInHierarchy &&
                            BlueprintBootstrap.MagicCircles.Any(circle => value.BlueprintAbility == circle.Spell)));
                    CircleUiAssert("sorcerer-candidate-icon-" + attempt, "native learning row uses the canonical candidate painting", "icon=" + learned.Icon.name,
                        row.GetComponentsInChildren<Image>(true).Any(value => value.isActiveAndEnabled && ReferenceEquals(value.sprite, learned.Icon)));
                    foreach (int frame in _circleUiScreens.CaptureRow("learning-candidate-" + attempt, (RectTransform)row.transform,
                        () => CircleUiState(new { spell = learned.AssetGuid, icon = learned.Icon.name, level = row.SpellLevel, role = "real Sorcerer level-up candidate" }),
                        () => presenter.IsShow && ReferenceEquals(presenter.LevelUpController, backend) && row.gameObject.activeInHierarchy && !_workingSaveSmoke.WriteObserved)) yield return frame;
                    row.Toggle.isOn = true;
                    foreach (int tick in WaitTeleportLevelUpUi(() => backend.Preview.GetSpellbook(sorcerer.Spellbook).IsKnown(learned), "native Sorcerer preview learns Circle")) yield return tick;
                    FillOracleNativeChoices(backend);
                    foreach (int tick in WaitTeleportLevelUpUi(() => backend.State.IsComplete(), "all normal Sorcerer level-up choices complete")) yield return tick;
                    if (attempt == 0)
                    {
                        presenter.OnHotKeyEscPressed();
                        foreach (int tick in WaitTeleportLevelUpUi(() => DialogMessageBox.Instance.IsShown, "Sorcerer cancel confirmation")) yield return tick;
                        var cancelCallback = (Action<DialogMessageBoxBase.BoxButton>)WorldMapPointSpellActionPatches.ConfirmationCallbackField.GetValue(DialogMessageBox.Instance);
                        if (cancelCallback == null || !ReferenceEquals(cancelCallback.Target, presenter))
                            throw new InvalidOperationException("The Sorcerer cancel dialog has an unrelated owner.");
                        TeleportationFixtureDialogButton("m_ButtonYes").onClick.Invoke();
                        foreach (int tick in WaitTeleportLevelUpUi(() => !presenter.IsShow && !DialogMessageBox.Instance.IsShown, "Sorcerer cancelled preview")) yield return tick;
                        backend.Cancel(); backend = null; ui.LevelUpController = priorBackend;
                        CircleUiAssert("sorcerer-real-cancel", "cancel learns nothing, spends nothing and leaves Sorcerer 5",
                            "known=" + book.IsKnown(learned), !book.IsKnown(learned) && before == TeleportResourceFingerprint(book) &&
                            unit.Descriptor.Progression.GetClassLevel(sorcerer) == 5 && successes == 0);
                    }
                    else
                    {
                        presenter.Next();
                        foreach (int tick in WaitTeleportLevelUpUi(() => presenter.CurrentPhase == CharBPhase.Type.Total, "Sorcerer native summary")) yield return tick;
                        var finish = (UnityEngine.UI.Button)typeof(CharacterBuildController).GetField("m_CompleteButton", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(presenter);
                        if (finish == null || !finish.interactable || !backend.State.IsComplete())
                            throw new InvalidOperationException("Native Sorcerer completion button is unavailable.");
                        var finalSelection = backend.State.SpellSelections.Single(value => value.Spellbook == sorcerer.Spellbook);
                        bool oneNormalChoice = finalSelection.LevelCount[3].SpellSelections.Length == 1 &&
                            ReferenceEquals(finalSelection.LevelCount[3].SpellSelections[0], learned) && !finalSelection.CanSpendSlot(3, 0) &&
                            (finalSelection.ExtraSelected == null || finalSelection.ExtraSelected.Length == 0);
                        finish.onClick.Invoke(); committed = true; backend = null;
                        captureStarterItems("sorcerer-commit-starter-items");
                        foreach (int tick in WaitTeleportLevelUpUi(() => !presenter.IsShow, "Sorcerer native committed level-up")) yield return tick;
                        ui.LevelUpController = priorBackend;
                        book = unit.Descriptor.GetSpellbook(sorcerer.Spellbook);
                        CircleUiCapture("sorcerer-real-commit", new { oneNormalChoice, normalAllowance = book.Blueprint.SpellsKnown.GetCount(book.CasterLevel, 3),
                            learned = book.GetKnownSpells(3).Select(value => new { id = value.Blueprint.AssetGuid, value.Blueprint.name }).ToArray(), successes });
                        CircleUiAssert("sorcerer-real-commit", "native completion learns the canonical spell in Sorcerer 6 using one normal third-level choice",
                            "level=" + unit.Descriptor.Progression.GetClassLevel(sorcerer) + ";known=" + book.GetKnownSpells(3).Count() + ";callbacks=" + successes,
                            unit.Descriptor.Progression.GetClassLevel(sorcerer) == 6 && book.CasterLevel == 6 && successes == 1 &&
                            oneNormalChoice && book.GetKnownSpells(3).Count(value => ReferenceEquals(value.Blueprint, learned)) == 1 &&
                            book.Blueprint.SpellsKnown.GetCount(book.CasterLevel, 3) == 1 &&
                            BlueprintBootstrap.MagicCircles.All(value => !book.IsKnown(value.Spell)));
                    }
                }
                player.PartyCharacters.Add(unit);
                player.InvalidateCharacterLists(); player.UpdateCharacterLists();
                if (!player.Party.Contains(unit)) throw new InvalidOperationException("The owned learning actor did not enter the UI party.");
                captureStarterItems("circle-ui-party-items");
                foreach (int tick in CirclePresentLearnedActor(unit, book, captureStarterItems)) yield return tick;
            }
            finally
            {
                if (backend != null)
                {
                    if (presenter.IsShow && ReferenceEquals(presenter.LevelUpController, backend)) presenter.Show(false);
                    if (!committed) backend.Cancel();
                }
                ui.LevelUpController = priorBackend; presenter.Unit = priorPresenter;
                if (unit != null)
                {
                    player.PartyCharacters.RemoveAll(value => value.UniqueId == unit.UniqueId);
                    unit.Destroy(); game.EntityDestroyer.Tick(); game.EntityDestroyer.Tick();
                    if (unit.HoldingState != null && unit.HoldingState.AllEntityData.Contains(unit)) unit.HoldingState.RemoveEntityData(unit);
                }
                player.InvalidateCharacterLists(); player.UpdateCharacterLists();
                // Only positive item deltas captured synchronously after this
                // owned character's native seed/commit/registration are removed.
                foreach (var entry in starterDeltas)
                    if (ReferenceEquals(entry.Key.Collection, player.Inventory) && entry.Key.Count >= entry.Value)
                        player.Inventory.Remove(entry.Key, entry.Value);
                game.IsPaused = originalPause;
                bool restored = originalParty.SequenceEqual(player.Party) && originalPartyRefs.SequenceEqual(player.PartyCharacters) &&
                    originalCross.SequenceEqual(player.CrossSceneState.AllEntityData) && originalItems.SequenceEqual(player.Inventory.Items) &&
                    originalCounts.SequenceEqual(originalItems.Select(value => value.Count)) && player.Money == originalMoney && !_workingSaveSmoke.WriteObserved;
                CircleUiCapture("sorcerer-real-cleanup", new { restored, originalMoney, money = player.Money,
                    partyRestored = originalParty.SequenceEqual(player.Party), crossRestored = originalCross.SequenceEqual(player.CrossSceneState.AllEntityData),
                    inventoryRestored = originalItems.SequenceEqual(player.Inventory.Items),
                    originalInventory = originalItems.Select((value, index) => new { instance = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value), id = value.Blueprint.AssetGuid, before = originalCounts[index], after = value.Count }).ToArray(),
                    currentInventory = player.Inventory.Items.Select(value => new { instance = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value), id = value.Blueprint.AssetGuid, value.Count }).ToArray(), exceptions = _circleUiExceptions.ToArray() });
                CircleUiAssert("sorcerer-real-cleanup", "owned Sorcerer removed; exact original party, inventory, money and settings; no save write", "restored=" + restored, restored);
                CircleUiAssert("sorcerer-real-exceptions", "no native or mod exception from Sorcerer seed through learned casting and cleanup", "count=" + _circleUiExceptions.Count, _circleUiExceptions.Count == 0);
            }
        }

        private IEnumerable<int> CircleBookUiEntry(SpellBookController controller, TeleportUiSpellEntry entry)
        {
            string caseId = entry.Spell.AssetGuid;
            var tabs = controller.SpellBookView.ClassToggle;
            int bookIndex = tabs.Spellbooks.IndexOf(entry.Book);
            var tab = tabs.GetComponentsInChildren<SpellbookClassTab>(true).Single(value => value.Index == bookIndex && value.gameObject.activeInHierarchy);
            tab.Toggle.isOn = true;
            if (!ReferenceEquals(controller.CurrentSpellbook, entry.Book)) throw new InvalidOperationException("Native class toggle did not select its physical book.");
            var levelTab = controller.SpellBookView.LevelBookSwitcher.m_Tabs[entry.Level];
            if (!levelTab.gameObject.activeInHierarchy || !levelTab.Toggle.interactable) throw new InvalidOperationException("Native required spell-level tab is unavailable.");
            levelTab.Toggle.isOn = true;
            if (controller.CurrentBookLevel != entry.Level) throw new InvalidOperationException("Native level tab selected a different spell level.");
            while (controller.CurrentPageIndex > 0) controller.SpellBookView.GoPrevPage();
            SpellItem row = null;
            for (int page = 0; page < 30; page++)
            {
                row = controller.SpellBookView.GetComponentsInChildren<SpellItem>(true).SingleOrDefault(value =>
                    value.gameObject.activeInHierarchy && value.SpellData != null && value.SpellData.Blueprint == entry.Spell);
                if (row != null) break;
                controller.SpellBookView.GoNextPage();
                if (controller.CurrentBookLevel != entry.Level) break;
            }
            if (row == null) throw new InvalidOperationException("Native spellbook pagination has no project spell row: " + caseId);
            controller.SpellBookView.ResetSelection(); row.Toggle.isOn = true;
            for (int frame = 0; frame < 8; frame++) yield return 0;
            CircleUiAssert("row-" + caseId, "native row selects the exact spell, book, level and icon",
                "selected=" + (controller.SelectedSpell == null ? "none" : controller.SelectedSpell.Blueprint.AssetGuid),
                controller.SelectedSpell != null && controller.SelectedSpell.Blueprint == entry.Spell &&
                ReferenceEquals(controller.SelectedSpell.Spellbook, entry.Book) && row.SpellImage.sprite == entry.Spell.Icon &&
                row.GetComponentsInChildren<TextMeshProUGUI>(true).Any(value => value.isActiveAndEnabled && string.Equals(value.GetParsedText(), entry.Spell.Name, StringComparison.OrdinalIgnoreCase)));
            var tooltip = row.GetComponent<TooltipTrigger>();
            if (tooltip == null) throw new InvalidOperationException("Native spell row has no description trigger.");
            tooltip.OpenDescriptionWindow();
            for (int frame = 0; frame < 8; frame++) yield return 0;
            var description = Game.Instance.UI.DescriptionController.DescWindow;
            var labels = description.GetComponentsInChildren<TextMeshProUGUI>(true).Where(value => value.isActiveAndEnabled).ToArray();
            CircleUiCapture("native-spell-description-" + caseId, new { spell = entry.Spell.AssetGuid, book = entry.Book.Blueprint.AssetGuid,
                entry.Level, controller.CurrentPageIndex, descriptionShown = description.gameObject.activeInHierarchy,
                labels = labels.Select(value => new { value.name, value.text, parsed = value.GetParsedText(), value.isTextTruncated, value.isTextOverflowing }).ToArray() });
            CircleUiAssert("description-" + caseId, "native description renders the complete active-configuration Circle instructions",
                "labels=" + labels.Length, description.gameObject.activeInHierarchy && labels.Any(value => !value.isTextTruncated &&
                    string.Equals(value.GetParsedText(), entry.Spell.Description, StringComparison.OrdinalIgnoreCase)));
            foreach (int frame in _circleUiScreens.Capture("spell-description-" + caseId,
                CircleUiState(new { spell = entry.Spell.AssetGuid, name = entry.Spell.Name, book = entry.Book.Blueprint.AssetGuid,
                    entry.Level, controller.CurrentPageIndex, icon = row.SpellImage.sprite.name,
                    texts = labels.Select(value => new { value.name, parsed = value.GetParsedText(), value.isTextTruncated, value.isTextOverflowing }).ToArray() }),
                () => !_workingSaveSmoke.WriteObserved && controller.IsShow && ReferenceEquals(controller.CurrentSpellbook, entry.Book) &&
                    controller.SelectedSpell != null && controller.SelectedSpell.Blueprint == entry.Spell &&
                    row.gameObject.activeInHierarchy && description.gameObject.activeInHierarchy)) yield return frame;
            tooltip.CloseDescriptionWindow();
            if (!entry.Book.Blueprint.Spontaneous)
            {
                int before = entry.Book.GetMemorizedSpells(entry.Level).Count(value => value.Spell != null && value.Spell.Blueprint == entry.Spell);
                row.Memorize();
                for (int frame = 0; frame < 4; frame++) yield return 0;
                var prepared = entry.Book.GetMemorizedSpells(entry.Level).Where(value => value.Spell != null && value.Spell.Blueprint == entry.Spell).ToArray();
                var shownSlots = controller.GetComponentsInChildren<SpellSlotItem>(true).Where(value => value.gameObject.activeInHierarchy &&
                    value.MechanicSlot != null && prepared.Contains(value.MechanicSlot)).ToArray();
                CircleUiAssert("prepare-" + caseId, "native row preparation allocates one real preparation and displays its unready slot",
                    "before=" + before + ";after=" + prepared.Length + ";shown=" + shownSlots.Length,
                    prepared.Length == before + 1 && shownSlots.Length > 0 && !prepared.Last().Available);
                entry.Book.Rest();
                // Rest() rebuilds the native spell page asynchronously; a fixed
                // frame count races that rebuild (observed losing it after the
                // scroll icon composition build). Wait, bounded, until the
                // exact capture guard holds instead of guessing a frame count.
                Func<bool> preparationStable = () => controller.IsShow &&
                    ReferenceEquals(controller.CurrentSpellbook, entry.Book) &&
                    row.gameObject.activeInHierarchy && !description.gameObject.activeInHierarchy;
                for (int frame = 0; frame < 120 && !preparationStable(); frame++) yield return 0;
                // Record every preparation-capture guard term so a failed
                // capture identifies its exact cause in structured evidence.
                CircleUiCapture("native-spell-preparation-guard-" + caseId, new {
                    controllerIsShow = controller.IsShow,
                    currentSpellbookExact = ReferenceEquals(controller.CurrentSpellbook, entry.Book),
                    rowActiveInHierarchy = row.gameObject.activeInHierarchy,
                    rowActiveSelf = row.gameObject.activeSelf,
                    descriptionActiveInHierarchy = description.gameObject.activeInHierarchy,
                    currentBookLevel = controller.CurrentBookLevel,
                    currentPageIndex = controller.CurrentPageIndex,
                    frame = Time.frameCount });
                foreach (int frame in _circleUiScreens.Capture("spell-preparation-" + caseId,
                    CircleUiState(new { spell = entry.Spell.AssetGuid, book = entry.Book.Blueprint.AssetGuid, entry.Level,
                        preparationCount = prepared.Length, visibleSlots = shownSlots.Length }),
                    () => !_workingSaveSmoke.WriteObserved && controller.IsShow && ReferenceEquals(controller.CurrentSpellbook, entry.Book) &&
                        row.gameObject.activeInHierarchy && !description.gameObject.activeInHierarchy)) yield return frame;
            }
            CircleUiAssert("available-" + caseId, "rested Circle is available and accepts native metamagic", "available=" + row.SpellData.IsAvailable,
                row.SpellData.IsAvailable && controller.SpellBookView.GetMetamagicSpells().Contains(entry.Spell));
        }

        private static JObject DescribeCircleScrollSlot(ItemSlot slot, ItemEntity item,
            Kingmaker.Blueprints.Items.Equipment.BlueprintItemEquipmentUsable blueprint, ItemEntity[] controls, bool retained)
        {
            var state = DescribeNativeScrollSlot(slot, item, blueprint, controls, retained);
            var circle = BlueprintBootstrap.MagicCircles.Single(value => ReferenceEquals(value.Scroll, blueprint));
            string key = "scroll-of-magic-circle-against-" + circle.Alignment.ToLowerInvariant();
            var target = (JObject)state["targetRow"];
            target["expectedIconKey"] = key;
            target["scrollIconExact"] = ReferenceEquals(blueprint.Icon, ProjectAssetIcons.RequireIcon(key)) &&
                ReferenceEquals(blueprint.Ability, circle.Spell);
            target["familySharing"] = "matching alignment painting inside the unchanged strategic scroll shell; distinct item sprite";
            return state;
        }

        private IEnumerable<int> CircleScrollUi()
        {
            var game = Game.Instance; var player = game.Player; var ui = game.UI;
            if (_request.Scenario != RuntimeTestScenarioCatalog.DisposableMagicCircleUi ||
                !_request.ExitAfterCompletion || _workingSaveSmoke == null || !_workingSaveSmoke.Complete ||
                _workingSaveSmoke.WriteObserved || !game.IsPaused || ui.ServiceWindow.WindowTabs.IsShow ||
                ui.DescriptionController.DescWindow.gameObject.activeInHierarchy || BlueprintBootstrap.MagicCircles == null)
                throw new InvalidOperationException("Scroll icon capture requires the owned paused working-save spellbook fixture and idle native UI.");
            var inventory = player.Inventory;
            var before = inventory.Items.ToArray();
            var counts = before.Select(item => item.Count).ToArray();
            var indices = before.Select(item => item.InventorySlotIndex).ToArray();
            var identified = before.Select(item => item.IsIdentified).ToArray();
            var charges = before.Select(item => item.Charges).ToArray();
            var icons = before.Select(item => item.Icon).ToArray();
            long money = player.Money;
            var window = ui.ServiceWindow.WindowTabs.SubWindowsList.Select(pair => pair.SubWindow).OfType<Inventory>().Single();
            if (window.IsShow || window.Filter == null || window.Stash == null || window.Sheet == null)
                throw new InvalidOperationException("Native inventory is unavailable or already owned.");
            var filter = window.Filter;
            var originalFilter = filter.CurrentFilter; var originalSorter = filter.CurrentSorter;
            bool originalSave = filter.Save;
            var savedFilter = player.UISettings.InventoryFilter; var savedSorter = player.UISettings.InventorySorter;
            var originalGroup = GroupController.Instance.GetCurrentCharacter();
            var sheetCharacter = typeof(CharacterScreenController).GetField("m_CurrentCharacter", BindingFlags.Instance | BindingFlags.NonPublic);
            var sheetSection = typeof(CharacterScreenController).GetField("m_CurrentSection", BindingFlags.Instance | BindingFlags.NonPublic);
            var oldCharacter = sheetCharacter.GetValue(window.Sheet); var oldSection = sheetSection.GetValue(window.Sheet);
            var blueprints = BlueprintBootstrap.MagicCircles.Select(value => value.Scroll).ToArray();
            if (before.Any(item => blueprints.Contains(item.Blueprint)))
                throw new InvalidOperationException("Owned scroll capture requires initial absence to avoid merging with existing stacks.");
            var owned = new List<ItemEntity>();
            ScrollRectExtended scroll = null;
            Vector2 originalScroll = Vector2.zero, originalVelocity = Vector2.zero;
            bool opened = false, filterOwned = false;
            var evidence = new JObject { ["surface"] = "native-scroll-inventory", ["status"] = "pending" };
            Func<bool> inventoryExact = () => ReferenceEquals(player.Inventory, inventory) && player.Money == money &&
                before.All(item => inventory.Items.Any(value => ReferenceEquals(value, item))) &&
                before.Select((item, index) => item.Count == counts[index] && item.Charges == charges[index] &&
                    item.IsIdentified == identified[index] && ReferenceEquals(item.Icon, icons[index])).All(value => value) &&
                inventory.Items.Count == before.Length + owned.Count && owned.All(item =>
                    ReferenceEquals(item.Collection, inventory) && inventory.Items.Contains(item) && item.Count == 1 && item.Charges == 1);
            Func<bool> ownsUi = () => opened && window.IsShow && window.gameObject.activeInHierarchy &&
                ui.ServiceWindow.WindowTabs.IsShow && ReferenceEquals(window.Stash.Collection, inventory) &&
                game.IsPaused && !_workingSaveSmoke.WriteObserved && inventoryExact();
            try
            {
                foreach (var blueprint in blueprints)
                {
                    var item = inventory.Add(blueprint);
                    if (item == null || before.Contains(item)) throw new InvalidOperationException("Native scroll insertion did not produce an owned entity.");
                    owned.Add(item);
                    item.Identify();
                }
                if (!inventoryExact()) throw new InvalidOperationException("Native scroll setup changed unrelated inventory state.");
                // Use normal filter APIs while retaining the saved preference.
                // NotSorted prevents this fixture from rearranging the stash.
                filterOwned = true; filter.Save = false;
                filter.SetSorter(ItemsFilter.SorterType.NotSorted);
                filter.ChangeFilter(ItemsFilter.FilterType.NoFilter, false, true);
                opened = true;
                ui.ServiceWindow.HandleOpenInventory();
                for (int frame = 0; frame < 60; frame++) yield return 0;
                if (!ownsUi()) throw new InvalidOperationException("The real native inventory did not open with the exact owned items.");
                scroll = window.Stash.GetComponentsInChildren<ScrollRectExtended>(true).Single(value =>
                    value.isActiveAndEnabled && value.vertical && value.content != null && value.viewport != null);
                originalScroll = scroll.normalizedPosition; originalVelocity = scroll.velocity;
                for (int index = 0; index < owned.Count; index++)
                {
                    var item = owned[index]; var blueprint = blueprints[index];
                    var virtualSlot = window.Stash.VirtualSlots.Single(value => ReferenceEquals(value.Item, item));
                    Canvas.ForceUpdateCanvases();
                    // Scroll using the native virtual row's measured position;
                    // never relocate a pooled slot or construct a substitute row.
                    float range = scroll.content.rect.height - scroll.viewport.rect.height;
                    scroll.StopMovement();
                    if (range > 0) scroll.verticalNormalizedPosition = Mathf.Clamp01(1 -
                        (virtualSlot.Position.y + virtualSlot.Size.y / 2 - scroll.viewport.rect.height / 2) / range);
                    for (int frame = 0; frame < 12; frame++) yield return 0;
                    var slot = window.Stash.GetComponentsInChildren<ItemSlot>(true).Single(value =>
                        value.gameObject.activeInHierarchy && ReferenceEquals(value.Item, item));
                    Func<bool> ownsSlot = () => ownsUi() && slot != null && slot.gameObject.activeInHierarchy &&
                        ReferenceEquals(slot.Item, item) && ReferenceEquals(virtualSlot.Item, item);
                    Func<JObject> describe = () => DescribeCircleScrollSlot(slot, item, blueprint, before, ownsSlot());
                    foreach (int frame in _circleUiScreens.CaptureRow("native-scroll-row:" + blueprint.AssetGuid,
                        (RectTransform)slot.transform, describe, ownsSlot)) yield return frame;
                    var state = describe();
                    var target = (JObject)state["targetRow"];
                    bool exact = (bool)target["renderedIconExact"] && (bool)target["scrollIconExact"] &&
                        (bool)target["spellIconDistinctFromItem"] &&
                        (bool)target["itemReferenceRetained"] && (int)target["otherItemRows"] > 0 && (bool)target["otherItemIconsExact"];
                    CircleUiAssert("inventory-icon-" + blueprint.AssetGuid,
                        "real native item slot displays the matching parchment composite, distinct from its spell painting, and preserves existing controls", state.ToString(), exact);
                    if (!exact) throw new InvalidOperationException("Native scroll inventory icon identity differs.");
                    var tooltip = slot.Tooltip;
                    var tipObject = typeof(TooltipTrigger).GetField("m_Obj", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (tooltip == null || tooltip.gameObject != slot.gameObject)
                        throw new InvalidOperationException("The native scroll slot has no owned tooltip trigger.");
                    // OpenDescriptionWindow collects its object from the native
                    // slot on demand. An unopened trigger may have no m_Obj yet.
                    tooltip.OpenDescriptionWindow();
                    for (int frame = 0; frame < 12; frame++) yield return 0;
                    bool tooltipItemExact = ReferenceEquals(tipObject.GetValue(tooltip), item);
                    bool tooltipDataItemExact = tooltip.Data != null && ReferenceEquals(tooltip.Data.Item, item);
                    if (!tooltipItemExact || !tooltipDataItemExact)
                        throw new InvalidOperationException("The native description did not collect the exact scroll item from its slot.");
                    var description = ui.DescriptionController.DescWindow;
                    var labels = description.GetComponentsInChildren<TextMeshProUGUI>(true).Where(value => value.isActiveAndEnabled).ToArray();
                    int descriptionIcons = description.GetComponentsInChildren<Image>(true).Count(value =>
                        value.isActiveAndEnabled && ReferenceEquals(value.sprite, blueprint.Icon));
                    bool named = labels.Any(value => !value.isTextTruncated &&
                        string.Equals(value.GetParsedText(), item.Name, StringComparison.OrdinalIgnoreCase));
                    var descriptionState = describe();
                    descriptionState["surface"] = "native-scroll-description";
                    descriptionState["description"] = new JObject { ["nameExact"] = named,
                        ["matchingIcons"] = descriptionIcons, ["tooltipItemExact"] = tooltipItemExact,
                        ["tooltipDataItemExact"] = tooltipDataItemExact,
                        ["shown"] = description.gameObject.activeInHierarchy };
                    foreach (int frame in _circleUiScreens.Capture("native-scroll-description:" + blueprint.AssetGuid,
                        descriptionState, () => ownsSlot() && description.gameObject.activeInHierarchy &&
                            ReferenceEquals(tipObject.GetValue(tooltip), item) && ReferenceEquals(tooltip.Data?.Item, item))) yield return frame;
                    bool descriptionExact = named && descriptionIcons > 0 && description.gameObject.activeInHierarchy;
                    CircleUiAssert("inventory-description-" + blueprint.AssetGuid,
                        "actual native item description displays the exact scroll name and matching icon", descriptionState.ToString(), descriptionExact);
                    if (!descriptionExact) throw new InvalidOperationException("Native scroll description identity differs.");
                    tooltip.CloseDescriptionWindow();
                    for (int frame = 0; frame < 8; frame++) yield return 0;
                }
                evidence["status"] = "four-native-scroll-slots-and-descriptions-captured";
                ui.ServiceWindow.HandleOpenInventory();
                for (int frame = 0; frame < 60; frame++) yield return 0;
                if (!ui.ServiceWindow.WindowTabs.IsShow && window.IsShow) window.Show(false);
                if (window.IsShow || ui.ServiceWindow.WindowTabs.IsShow)
                    throw new InvalidOperationException("Native inventory did not finish its normal close.");
                opened = false;

                foreach (int frame in CaptureNativeStrategicScrollMerchant(blueprints, before, inventoryExact)) yield return frame;
            }
            finally
            {
                ui.DescriptionController.HandleCloseDescriptionWindow(ui.DescriptionController.DescWindow);
                if (scroll != null && scroll.isActiveAndEnabled)
                {
                    scroll.verticalNormalizedPosition = originalScroll.y;
                    if (scroll.horizontal) scroll.horizontalNormalizedPosition = originalScroll.x;
                    scroll.velocity = originalVelocity;
                }
                if (opened && window.IsShow && ui.ServiceWindow.WindowTabs.IsShow) ui.ServiceWindow.HandleOpenInventory();
                // Native tabs can retain their selected subwindow's IsShow
                // flag while the parent closes. Restore this initially closed,
                // request-owned window through its ordinary native close API.
                if (opened && !ui.ServiceWindow.WindowTabs.IsShow && window.IsShow) window.Show(false);
                if (filterOwned)
                {
                    filter.SetSorter(originalSorter);
                    filter.ChangeFilter(originalFilter, false, true);
                    filter.Save = originalSave;
                }
                foreach (var item in owned)
                {
                    if (!ReferenceEquals(item.Collection, inventory) || !inventory.Items.Contains(item) || item.Count != 1)
                        throw new InvalidOperationException("Owned scroll changed before exact removal.");
                    inventory.Remove(item, 1).Dispose();
                }
                // The native UI assigns display indices. Restore only those
                // original scalar indices after closing its rows and removing
                // the exact owned additions, without sorting original items.
                var slotIndex = typeof(ItemEntity).GetField("m_InventorySlotIndex", BindingFlags.Instance | BindingFlags.NonPublic);
                for (int index = 0; index < before.Length; index++) slotIndex.SetValue(before[index], indices[index]);
                sheetCharacter.SetValue(window.Sheet, oldCharacter); sheetSection.SetValue(window.Sheet, oldSection);
                var cleanup = new JObject { ["itemReferencesAndOrder"] = inventory.Items.SequenceEqual(before),
                    ["itemScalarsAndIcons"] = before.Select((item, index) =>
                    item.Count == counts[index] && item.InventorySlotIndex == indices[index] && item.IsIdentified == identified[index] &&
                    item.Charges == charges[index] && ReferenceEquals(item.Icon, icons[index])).All(value => value),
                    ["money"] = player.Money == money, ["tabsClosed"] = !ui.ServiceWindow.WindowTabs.IsShow,
                    ["inventoryClosed"] = !window.IsShow, ["descriptionClosed"] = !ui.DescriptionController.DescWindow.gameObject.activeInHierarchy,
                    ["filter"] = filter.CurrentFilter == originalFilter, ["sorter"] = filter.CurrentSorter == originalSorter,
                    ["filterSave"] = filter.Save == originalSave, ["savedFilter"] = player.UISettings.InventoryFilter == savedFilter,
                    ["savedSorter"] = player.UISettings.InventorySorter == savedSorter,
                    ["sheetCharacter"] = ReferenceEquals(sheetCharacter.GetValue(window.Sheet), oldCharacter),
                    ["sheetSection"] = Equals(sheetSection.GetValue(window.Sheet), oldSection),
                    ["group"] = ReferenceEquals(GroupController.Instance.GetCurrentCharacter(), originalGroup),
                    ["zeroSaveWrites"] = !_workingSaveSmoke.WriteObserved };
                bool restored = cleanup.Properties().All(value => (bool)value.Value);
                evidence["checks"] = cleanup;
                evidence["restored"] = restored; evidence["originalItemCount"] = before.Length; evidence["temporaryItemCount"] = owned.Count;
                CircleUiCapture("native-scroll-inventory-cleanup", evidence);
                CircleUiAssert("inventory-icon-cleanup", "exact original item references/order/counts/indices/identification/charges/icons, gold, filters, group and closed windows; zero writes",
                    evidence.ToString(), restored);
                // The assertion makes the run fail; do not mask a prior capture
                // exception while this iterator is unwinding.
            }
        }

    }
}
