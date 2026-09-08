using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Assets.UI.LevelUp;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.GameModes;
using Kingmaker.UI;
using Kingmaker.UI.LevelUp;
using Kingmaker.UI.LevelUp.Phase;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Spells.Teleportation;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private bool IsTeleportationLevelUpFixture { get { return _request.Scenario == RuntimeTestScenarioCatalog.DisposableTeleportationLevelUp; } }
        private IEnumerable<int> RunTeleportationLevelUp()
        {
            var game = Game.Instance; var player = game.Player; var ui = game.UI;
            var controller = ui.CharacterBuildController;
            var originalBackend = ui.LevelUpController;
            var originalPresenterUnit = controller == null ? null : controller.Unit;
            // MainMenu.Warmup leaves its AutoCommit controller in UIAccess.
            // It has no isolated preview and native Start normally replaces it.
            // Preserve that exact reference; never cancel or mutate it.
            bool inactiveWarmup = originalBackend == null || originalBackend.AutoCommit &&
                ReferenceEquals(originalBackend.Unit, originalBackend.Preview);
            CaptureTeleportSpellbookUi("level-up-entry-state", new {
                enabled = _context.FeatureModules.Active.TeleportationSpells,
                published = BlueprintBootstrap.TeleportationPublication != null,
                gamepad = game.IsControllerGamepad, mode = game.CurrentMode.ToString(),
                controllerPresent = controller != null, controllerShown = controller != null && controller.IsShow,
                previewPresent = ui.LevelUpController != null, inactiveWarmup,
                baselineAutoCommit = originalBackend != null && originalBackend.AutoCommit, servicePresent = ui.ServiceWindow != null,
                serviceShown = ui.ServiceWindow != null && ui.ServiceWindow.WindowTabs.IsShow,
                modalPresent = DialogMessageBox.Instance != null,
                modalShown = DialogMessageBox.Instance != null && DialogMessageBox.Instance.IsShown,
                autoLevelUp = SettingsRoot.Instance.AutoLevelup.CurrentValue.ToString(),
                campaignAutoLevelUp = player.Difficulty.AutoLevelup.ToString()
            });
            if (!_context.FeatureModules.Active.TeleportationSpells || BlueprintBootstrap.TeleportationPublication == null || game.IsControllerGamepad ||
                game.CurrentMode != GameModeType.Default || controller == null || controller.IsShow || controller.LevelUpController != null || !inactiveWarmup ||
                ui.ServiceWindow == null || ui.ServiceWindow.WindowTabs.IsShow || DialogMessageBox.Instance == null || DialogMessageBox.Instance.IsShown ||
                SettingsRoot.Instance.AutoLevelup.CurrentValue != AutolevelupState.Off)
                throw new InvalidOperationException("Level-up qualification requires idle native desktop local-area UI, no active preview/modal and native auto-level-up OFF.");
            var wizard = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library, "ba34257984f4c41408ce1dc2004e342e", "native Wizard level-up fixture");
            var sorcerer = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library, "b3a505fb61437dc4097f43c3f8f9a4cf", "native Sorcerer level-up fixture");
            var owner = player.Party.FirstOrDefault(value => TeleportationSpellbookAdapter.CasterAvailable(value) && value.View != null && value.IsDirectlyControllable &&
                value.Descriptor.Progression.CharacterLevel < 20 && value.Descriptor.GetSpellbook(wizard.Spellbook) == null && value.Descriptor.GetSpellbook(sorcerer.Spellbook) == null);
            if (owner == null || !ReferenceEquals(owner, player.MainCharacter.Value) || !owner.Descriptor.IsTurnedOn)
                throw new InvalidOperationException("The canonical active save owner must have unused level-up fixture books.");
            var experience = typeof(UnitProgressionData).GetProperty("Experience");
            if (experience == null || experience.PropertyType != typeof(int) || experience.GetSetMethod(true) == null)
                throw new InvalidOperationException("Native request-local experience restoration seam differs.");
            int originalExperience = owner.Descriptor.Progression.Experience;
            int originalLevel = owner.Descriptor.Progression.CharacterLevel;
            var originalClasses = owner.Descriptor.Progression.Classes.ToArray();
            var originalClassLevels = originalClasses.Select(value => value.Level).ToArray();
            var originalFeatures = owner.Descriptor.Progression.Features.Enumerable.ToArray();
            var originalSelection = ui.SelectionManagerPC.SelectedUnits.ToArray();
            var originalParty = player.Party.ToArray(); var originalArea = game.CurrentlyLoadedArea;
            var originalPositions = originalParty.Select(value => value.Position).ToArray();
            var originalTime = player.GameTime; bool originalPaused = game.IsPaused;
            var uiSnapshots = originalParty.Select(value => new TeleportationUiSettingsFixture(value.UISettings)).ToArray();
            var entries = new[] {
                new TeleportLevelUpEntry(wizard, 8, TeleportSpellKind.Teleport, 5),
                new TeleportLevelUpEntry(wizard, 12, TeleportSpellKind.GreaterTeleport, 7),
                new TeleportLevelUpEntry(sorcerer, 9, TeleportSpellKind.Teleport, 5),
                new TeleportLevelUpEntry(sorcerer, 13, TeleportSpellKind.GreaterTeleport, 7)
            };
            var savedState = new TeleportLevelUpSavedStateFixture(owner.Descriptor);
            Application.logMessageReceived += ObserveTeleportSpellbookUiException;
            try
            {
                game.IsPaused = true;
                savedState.Prepare();
                // The native XP setter is a fixture-only precondition; it grants
                // no level and emits no experience/automatic level-up event.
                experience.SetValue(owner.Descriptor.Progression,
                    Math.Max(originalExperience, game.BlueprintRoot.Progression.XPTable.GetBonus(originalLevel + 1)), null);
                foreach (var entry in entries)
                    foreach (int frame in QualifyTeleportLevelUpEntry(controller, owner, entry, savedState)) yield return frame;
                foreach (var snapshot in uiSnapshots) snapshot.Restore();
                for (int frame = 0; frame < 30; frame++) yield return 0;
            }
            finally
            {
                savedState.Restore();
                experience.SetValue(owner.Descriptor.Progression, originalExperience, null);
                controller.Unit = originalPresenterUnit;
                ui.SelectionManagerPC.MultiSelect(originalSelection.Select(value => value.View).ToArray(), false);
                foreach (var snapshot in uiSnapshots) snapshot.Restore();
                game.IsPaused = originalPaused;
                Application.logMessageReceived -= ObserveTeleportSpellbookUiException;
                bool unchanged = owner.Descriptor.Progression.Experience == originalExperience && owner.Descriptor.Progression.CharacterLevel == originalLevel &&
                    owner.Descriptor.Progression.Classes.SequenceEqual(originalClasses) && originalClassLevels.SequenceEqual(originalClasses.Select(value => value.Level)) &&
                    owner.Descriptor.Progression.Features.Enumerable.SequenceEqual(originalFeatures) && uiSnapshots.All(value => value.IsRestored()) &&
                    ui.SelectionManagerPC.SelectedUnits.SequenceEqual(originalSelection) && player.Party.SequenceEqual(originalParty) &&
                    originalPositions.SequenceEqual(originalParty.Select(value => value.Position)) &&
                    ReferenceEquals(game.CurrentlyLoadedArea, originalArea) && player.GameTime == originalTime && game.IsPaused == originalPaused &&
                    savedState.IsRestored() && !controller.IsShow && ReferenceEquals(ui.LevelUpController, originalBackend) &&
                    ReferenceEquals(controller.Unit, originalPresenterUnit) && !DialogMessageBox.Instance.IsShown && !_workingSaveSmoke.WriteObserved;
                CaptureTeleportSpellbookUi("level-up-cleanup", new { unchanged, experience = owner.Descriptor.Progression.Experience,
                    characterLevel = owner.Descriptor.Progression.CharacterLevel, sameClassReferences = owner.Descriptor.Progression.Classes.SequenceEqual(originalClasses),
                    sameFeatureReferences = owner.Descriptor.Progression.Features.Enumerable.SequenceEqual(originalFeatures), uiSettingsRestored = uiSnapshots.All(value => value.IsRestored()),
                    timeUnchanged = player.GameTime == originalTime, baselineBackendRestored = ReferenceEquals(ui.LevelUpController, originalBackend), uiShown = controller.IsShow });
                TeleportSpellbookUiAssert("saved-owner-cleanup", "original familiarity part presence, fields, owner and native serialization source restored", "restored=" + savedState.IsRestored(), savedState.IsRestored());
                TeleportSpellbookUiAssert("cleanup", "no character level, feature, spell, XP, resource, UI, party, time or save-write effect survives cancellation", "unchanged=" + unchanged, unchanged);
                TeleportSpellbookUiAssert("exceptions", "zero native or mod exceptions from level-up fixture setup through cleanup", "count=" + _teleportationSpellbookUiExceptions.Count, _teleportationSpellbookUiExceptions.Count == 0);
            }
        }
        private sealed class TeleportLevelUpEntry
        {
            internal readonly BlueprintCharacterClass Class;
            internal readonly int CasterLevelBefore, SpellLevel;
            internal readonly TeleportSpellKind Kind;
            internal TeleportLevelUpEntry(BlueprintCharacterClass type, int casterLevel, TeleportSpellKind kind, int spellLevel)
            { Class = type; CasterLevelBefore = casterLevel; Kind = kind; SpellLevel = spellLevel; }
        }
        private IEnumerable<int> QualifyTeleportLevelUpEntry(CharacterBuildController controller, UnitEntityData owner, TeleportLevelUpEntry entry, TeleportLevelUpSavedStateFixture savedState)
        {
            string caseId = entry.Class.AssetGuid + "-" + entry.SpellLevel;
            var spell = BlueprintBootstrap.Teleportation.Get(entry.Kind);
            var fixture = new TeleportResourceFixtureOwner(owner);
            LevelUpController backend = null; bool cancelled = false;
            var previousBackend = Game.Instance.UI.LevelUpController;
            int originalLevel = owner.Descriptor.Progression.CharacterLevel;
            try
            {
                var book = fixture.AddBook(entry.Class.Spellbook, entry.CasterLevelBefore);
                string originalResource = TeleportResourceFingerprint(book);
                if (book.IsKnown(spell) || !LevelUpController.CanLevelUp(owner.Descriptor))
                    throw new InvalidOperationException("The guarded level-up requires an unknown project spell and native XP eligibility.");
                controller.HandleLevelUpStart(owner.Descriptor);
                backend = controller.LevelUpController;
                if (backend == null || backend.AutoCommit || ReferenceEquals(backend.Preview, owner.Descriptor))
                    throw new InvalidOperationException("The native level-up failed to allocate its isolated preview.");
                for (int frame = 0; frame < 30; frame++) yield return 0;
                object savedEvidence;
                bool savedRoundTrip = savedState.RoundTripMatches(backend.Preview, out savedEvidence);
                CaptureTeleportSpellbookUi("native-owner-round-trip-initial-" + caseId, savedEvidence);
                TeleportSpellbookUiAssert("saved-owner-initial-" + caseId, "native serialized owner graph contains both fields and the loaded preview has an independent correctly owned part", "matches=" + savedRoundTrip, savedRoundTrip);
                if (!savedRoundTrip) throw new InvalidOperationException("Native unit serialization did not preserve the owned teleportation state.");
                controller.SetClass(entry.Class);
                CompleteTeleportLevelUpPrerequisites(controller);
                CaptureTeleportSpellbookUi("native-level-up-prerequisites-" + caseId, new { classId = entry.Class.AssetGuid, ownerId = owner.UniqueId,
                    originalLevel, previewLevel = backend.Preview.Progression.CharacterLevel, autoCommit = backend.AutoCommit,
                    nativeCasterLevel = backend.Preview.GetSpellbook(entry.Class.Spellbook).CasterLevel,
                    phaseUnlocked = controller.Spells.IsUnlocked, phaseAvailable = controller.Spells.IsAvailible,
                    remainingSkills = backend.State.SkillPointsRemaining, remainingFeatures = backend.State.RemainingSelections(),
                    selections = backend.State.Selections.Select(value => new { source = value.Source == null ? null : value.Source.AssetGuid,
                        selected = value.SelectedItem == null ? null : value.SelectedItem.Feature.AssetGuid }).ToArray(),
                    spellSelections = backend.State.SpellSelections.Select(value => new { book = value.Spellbook.AssetGuid, list = value.SpellList.AssetGuid,
                        value.ExtraMaxLevel, extraCount = value.ExtraSelected == null ? 0 : value.ExtraSelected.Length,
                        counts = value.LevelCount.Select(part => part == null ? 0 : part.SpellSelections.Length).ToArray() }).ToArray() });
                if (!controller.Spells.IsUnlocked || !controller.Spells.IsAvailible)
                    throw new InvalidOperationException("Native prerequisites did not unlock the spell-selection phase.");
                controller.SetPhase((int)CharBPhase.Type.Spells);
                foreach (int tick in WaitTeleportLevelUpUi(() => controller.GetComponentsInChildren<CharBSelectionSwitchItem>(true).Any(value =>
                    value.gameObject.activeInHierarchy && value.SpellSelectionData != null && value.SpellSelectionData.Spellbook == entry.Class.Spellbook), "native spell collections")) yield return tick;
                var slot = controller.GetComponentsInChildren<CharBSelectionSwitchItem>(true).Where(value => value.gameObject.activeInHierarchy &&
                    value.SpellSelectionData != null && value.SpellSelectionData.Spellbook == entry.Class.Spellbook &&
                    (entry.Class.Spellbook.Spontaneous ? value.SpellLevel == entry.SpellLevel : value.SpellSelectionData.ExtraMaxLevel == entry.SpellLevel))
                    .OrderBy(value => value.SpellIndex).First();
                slot.Toggle.isOn = true;
                // Native deferred preview refresh can recycle selector widgets.
                // Resolve the current binding on every frame; never retain a row
                // across yields and then invoke its now unrelated callback.
                Func<CharBuildSelectorItem> currentRow = () => controller.GetComponentsInChildren<CharBuildSelectorItem>(true)
                    .SingleOrDefault(value => value.gameObject.activeInHierarchy && value.BlueprintAbility == spell && value.Toggle.interactable);
                foreach (int tick in WaitTeleportLevelUpUi(() => {
                    var candidate = currentRow();
                    return candidate != null && candidate.GetComponentsInChildren<TextMeshProUGUI>(true).Any(value =>
                        value.isActiveAndEnabled && !value.isTextTruncated && string.Equals(value.GetParsedText(), spell.Name, StringComparison.OrdinalIgnoreCase));
                }, "native available project spell row", 8)) yield return tick;
                var row = currentRow();
                CaptureTeleportSpellbookUi("native-level-up-row-" + caseId, new { spell = spell.AssetGuid, entry.SpellLevel,
                    actualLevel = row.SpellLevel, currentSlot = controller.Spells.CurrentSpellsCollectionIndex,
                    currentList = controller.Spells.CurrentSpellSelectionData.SpellList.AssetGuid,
                    labels = row.GetComponentsInChildren<TextMeshProUGUI>(true).Where(value => value.isActiveAndEnabled)
                        .Select(value => new { value.name, parsed = value.GetParsedText(), value.isTextTruncated }).ToArray() });
                TeleportSpellbookUiAssert("available-row-" + caseId, "published strategic spell appears as an enabled native level-up choice at the exact spell level",
                    "spell=" + row.BlueprintAbility.AssetGuid + ";level=" + row.SpellLevel,
                    row.SpellLevel == entry.SpellLevel && row.Toggle.interactable && controller.CurrentPhase == CharBPhase.Type.Spells &&
                    controller.Spells.CurrentSpellSelectionData.SpellList.AssetGuid == TeleportationSpellListPublication.WizardListId &&
                    row.GetComponentsInChildren<TextMeshProUGUI>(true).Any(value => value.isActiveAndEnabled && !value.isTextTruncated &&
                        string.Equals(value.GetParsedText(), spell.Name, StringComparison.OrdinalIgnoreCase)));
                row.Toggle.isOn = true;
                foreach (int tick in WaitTeleportLevelUpUi(() => backend.Preview.GetSpellbook(entry.Class.Spellbook).IsKnown(spell), "native preview spell selection")) yield return tick;
                TeleportSpellbookUiAssert("preview-only-" + caseId, "native selector learns the exact spell only in the isolated preview",
                    "previewKnown=" + backend.Preview.GetSpellbook(entry.Class.Spellbook).IsKnown(spell) + ";originalKnown=" + book.IsKnown(spell),
                    backend.Preview.GetSpellbook(entry.Class.Spellbook).IsKnown(spell) && !book.IsKnown(spell) &&
                    originalResource == TeleportResourceFingerprint(book) && owner.Descriptor.Progression.CharacterLevel == originalLevel);
                savedRoundTrip = savedState.RoundTripMatches(backend.Preview, out savedEvidence);
                CaptureTeleportSpellbookUi("native-owner-round-trip-selected-" + caseId, savedEvidence);
                TeleportSpellbookUiAssert("saved-owner-selected-" + caseId, "native preview rebuilds and spell selection preserve both saved fields and owner references", "matches=" + savedRoundTrip, savedRoundTrip);
                if (!savedRoundTrip) throw new InvalidOperationException("Native preview refresh changed teleportation saved state.");
                backend.Preview.Get<UnitPartTeleportFamiliarity>().ClearExplorationBoundary();
                bool independent = backend.Preview.Get<UnitPartTeleportFamiliarity>().ReadExplorationBoundary() == null && savedState.SourceUnchanged() && savedState.NativePayloadUnchanged();
                TeleportSpellbookUiAssert("saved-owner-independent-" + caseId, "changing the disposable preview cannot change the original part or native serialized source", "independent=" + independent, independent);
                controller.OnHotKeyEscPressed();
                foreach (int tick in WaitTeleportLevelUpUi(() => DialogMessageBox.Instance.IsShown, "native cancel confirmation")) yield return tick;
                var callback = (Action<DialogMessageBoxBase.BoxButton>)WorldMapPointSpellActionPatches.ConfirmationCallbackField.GetValue(DialogMessageBox.Instance);
                if (callback == null || !ReferenceEquals(callback.Target, controller)) throw new InvalidOperationException("The level-up cancel dialog has an unrelated owner.");
                TeleportationFixtureDialogButton("m_ButtonYes").onClick.Invoke();
                foreach (int tick in WaitTeleportLevelUpUi(() => !controller.IsShow && !DialogMessageBox.Instance.IsShown, "native level-up cancellation")) yield return tick;
                // Native UI closing drops its presenter reference. Explicitly close
                // this owned native preview/thread too, before removing fixture books.
                backend.Cancel(); cancelled = true;
                if (ReferenceEquals(Game.Instance.UI.LevelUpController, backend)) Game.Instance.UI.LevelUpController = previousBackend;
                for (int frame = 0; frame < 30; frame++) yield return 0;
                TeleportSpellbookUiAssert("cancel-" + caseId, "native cancellation commits no known spell, level or resource change and closes the owned preview",
                    "originalKnown=" + book.IsKnown(spell) + ";level=" + owner.Descriptor.Progression.CharacterLevel,
                    !book.IsKnown(spell) && originalResource == TeleportResourceFingerprint(book) && owner.Descriptor.Progression.CharacterLevel == originalLevel &&
                    savedState.SourceUnchanged() && savedState.NativeSourceCleared &&
                    !controller.IsShow && ReferenceEquals(Game.Instance.UI.LevelUpController, previousBackend));
            }
            finally
            {
                if (backend != null)
                {
                    var dialog = DialogMessageBox.Instance;
                    var callback = dialog == null ? null : (Action<DialogMessageBoxBase.BoxButton>)WorldMapPointSpellActionPatches.ConfirmationCallbackField.GetValue(dialog);
                    if (dialog != null && dialog.IsShown && callback != null && ReferenceEquals(callback.Target, controller)) dialog.HandleForceClose();
                    if (controller.IsShow && ReferenceEquals(controller.LevelUpController, backend)) controller.Show(false);
                    if (!cancelled) backend.Cancel();
                    if (ReferenceEquals(Game.Instance.UI.LevelUpController, backend)) Game.Instance.UI.LevelUpController = previousBackend;
                }
                fixture.Restore();
                TeleportSpellbookUiAssert("book-cleanup-" + caseId, "exact native original books, resources and casting stats restored", "restored=" + fixture.IsRestored(), fixture.IsRestored());
            }
        }
        // Payload setup is request-local, not an ordinary arrival. The actual
        // native level-up serializes this part with its UnitDescriptor owner,
        // deserializes the native preview and invokes its normal PostLoad path.
        private sealed class TeleportLevelUpSavedStateFixture
        {
            private const string Counts = "1|1|00afbe29e976df24db85cda2c4c67864:2|5a69648758f7d0d49afbe2a4ef61225e:4|b78220bfcf6d25145a808aa1f1f7eee8:7";
            private const string Boundary = "1|76f2987ea128d624490e3dfa56ec8a47|5a69648758f7d0d49afbe2a4ef61225e|12.5";
            private readonly UnitDescriptor _owner;
            private readonly UnitPartTeleportFamiliarity _originalPart;
            private readonly TeleportNativeFieldSnapshot _originalFields;
            private readonly FieldInfo _payloadField, _nativeSourceField;
            private UnitPartTeleportFamiliarity _part;
            internal TeleportLevelUpSavedStateFixture(UnitDescriptor owner)
            {
                _owner = owner; _originalPart = owner.Get<UnitPartTeleportFamiliarity>();
                _originalFields = _originalPart == null ? null : new TeleportNativeFieldSnapshot(_originalPart);
                _payloadField = typeof(UnitPartTeleportFamiliarity).GetField("_state", BindingFlags.Instance | BindingFlags.NonPublic);
                _nativeSourceField = typeof(LevelUpPreviewThread).GetField("s_Source", BindingFlags.Static | BindingFlags.NonPublic);
                if (_payloadField == null || _nativeSourceField == null || _nativeSourceField.FieldType != typeof(JToken) ||
                    !owner.IsTurnedOn || !NativeSourceCleared || _originalPart != null && !ReferenceEquals(_originalPart.Owner, owner))
                    throw new InvalidOperationException("Native saved-owner fixture prerequisites or exact serialization source seam differ.");
            }
            internal bool NativeSourceCleared { get { return _nativeSourceField.GetValue(null) == null; } }
            internal void Prepare()
            {
                TeleportFamiliarityState.Parse(Counts);
                _part = _originalPart ?? _owner.Ensure<UnitPartTeleportFamiliarity>();
                _payloadField.SetValue(_part, Counts);
                _part.MarkMagicalArrival(TeleportExplorationBoundary.Parse(Boundary));
            }
            internal bool SourceUnchanged()
            {
                return _part != null && ReferenceEquals(_owner.Get<UnitPartTeleportFamiliarity>(), _part) && ReferenceEquals(_part.Owner, _owner) &&
                    _owner.IsTurnedOn && _part.Read().Serialize() == Counts && _part.ReadExplorationBoundary().Serialize() == Boundary;
            }
            internal bool NativePayloadUnchanged()
            {
                var source = _nativeSourceField.GetValue(null) as JContainer;
                if (source == null) return false;
                var fields = source.Descendants().OfType<JProperty>().Where(value => value.Name == "_state" &&
                    value.Value.Type == JTokenType.String && (string)value.Value == Counts).ToArray();
                return fields.Length == 1 && fields[0].Parent is JObject && (string)fields[0].Parent["_explorationBoundary"] == Boundary;
            }
            internal bool RoundTripMatches(UnitDescriptor preview, out object evidence)
            {
                var source = _nativeSourceField.GetValue(null) as JContainer;
                var fields = source == null ? new JProperty[0] : source.Descendants().OfType<JProperty>()
                    .Where(value => value.Name == "_state" && value.Value.Type == JTokenType.String && (string)value.Value == Counts).ToArray();
                var serializedPart = fields.Length == 1 ? fields[0].Parent as JObject : null;
                var part = preview.Get<UnitPartTeleportFamiliarity>();
                bool jsonMatches = serializedPart != null && (string)serializedPart["_explorationBoundary"] == Boundary;
                bool independentOwner = part != null && !ReferenceEquals(part, _part) && ReferenceEquals(part.Owner, preview) && !ReferenceEquals(preview, _owner);
                bool fieldsMatch = part != null && part.Read().Serialize() == Counts && part.ReadExplorationBoundary() != null && part.ReadExplorationBoundary().Serialize() == Boundary;
                bool matches = jsonMatches && independentOwner && fieldsMatch && SourceUnchanged();
                evidence = new { sourceOwnerId = _owner.Unit.UniqueId, previewOwnerId = preview.Unit.UniqueId,
                    nativeSourceType = source == null ? null : source.GetType().FullName, matchingSerializedFields = fields.Length,
                    serializedPartPath = serializedPart == null ? null : serializedPart.Path,
                    serializedPartType = serializedPart == null ? null : (string)serializedPart["$type"],
                    serializedProperties = serializedPart == null ? null : serializedPart.Properties().Select(value => value.Name).ToArray(),
                    jsonMatches, independentOwner, fieldsMatch, sourceUnchanged = SourceUnchanged(), counts = Counts, boundary = Boundary };
                return matches;
            }
            internal void Restore()
            {
                if (_originalPart == null) _owner.Remove<UnitPartTeleportFamiliarity>(); else _originalFields.Restore();
                // Native UnitSerialization lacks an inner finally around TurnOn.
                // Restore this proven originally active owner if serialization throws.
                if (!_owner.IsTurnedOn) _owner.TurnOn();
            }
            internal bool IsRestored()
            {
                return ReferenceEquals(_owner.Get<UnitPartTeleportFamiliarity>(), _originalPart) &&
                    (_originalFields == null || _originalFields.Matches() && ReferenceEquals(_originalPart.Owner, _owner)) &&
                    _owner.IsTurnedOn && NativeSourceCleared;
            }
        }
        private void CompleteTeleportLevelUpPrerequisites(CharacterBuildController controller)
        {
            // Choices affect only the disposable native preview. Every candidate
            // passes native selection rules; no phase flag or prerequisite is set.
            for (int iteration = 0; iteration < 128 && !controller.Spells.IsUnlocked; iteration++)
            {
                var backend = controller.LevelUpController;
                if (backend.State.AttributePoints > 0) { controller.SpendAttributePoint(StatType.Intelligence, true); continue; }
                var selection = backend.State.Selections.FirstOrDefault(value => !value.Selected && value.CanSelectAnything(backend.State, backend.Preview));
                if (selection != null)
                {
                    var item = selection.Selection.Items.Where(value => selection.Selection.CanSelect(backend.Preview, backend.State, selection, value))
                        .OrderBy(value => value.Feature.AssetGuid, StringComparer.Ordinal).FirstOrDefault();
                    if (item == null) throw new InvalidOperationException("Native level-up has no eligible feature choice for its remaining prerequisite.");
                    controller.SetFeature(selection, item); continue;
                }
                int before = backend.State.SkillPointsRemaining;
                foreach (StatType skill in Enum.GetValues(typeof(StatType)))
                {
                    if (!skill.ToString().StartsWith("Skill", StringComparison.Ordinal)) continue;
                    controller.SpendSkillPoint(skill, true);
                    if (backend.State.SkillPointsRemaining < before) break;
                }
                if (backend.State.SkillPointsRemaining == before) break;
            }
        }
        private static IEnumerable<int> WaitTeleportLevelUpUi(Func<bool> ready, string boundary, int stableFrames = 1)
        {
            var watch = Stopwatch.StartNew(); int observedFrames = 0;
            while (true)
            {
                observedFrames = ready() ? observedFrames + 1 : 0;
                if (observedFrames >= stableFrames) yield break;
                if (watch.Elapsed.TotalSeconds > 8) throw new InvalidOperationException("Native level-up UI did not settle: " + boundary);
                yield return 0;
            }
        }
    }
}
