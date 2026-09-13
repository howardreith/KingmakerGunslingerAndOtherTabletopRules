using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.UI.Group;
using Kingmaker.UI.ServiceWindow.CharacterScreen;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.Feats;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class ElementalCharacterCreationBaselineScenario
    {
        private bool NativeSheetCase => _request.Scenario == RuntimeTestScenarioCatalog.WorkingSaveElementalCharacterCreationRegression &&
            (string)_request.Parameters["class"] == "Gunslinger" && (string)_request.Parameters["allocation"] == "roll";

        private bool PauseForNativeIconSheet()
        {
            if (!NativeSheetCase || !_nativeIconStages.Add(_raceIndex + "|committed-native-sheet")) return false;
            if (_nativeIconScreens == null) _nativeIconScreens = new NativeIconScreenEvidence(_request);
            _nativeIconCapture = CaptureNativeCharacterSheet().GetEnumerator();
            return true;
        }

        // Hold the existing request-owned mercenary after native registration,
        // before the regression's exact item/money/membership rollback. This
        // uses the real sheet and never enrolls the actor in the active party.
        private IEnumerable<int> CaptureNativeCharacterSheet()
        {
            var game = Game.Instance;
            var ui = game.UI;
            var owner = _unit;
            var group = GroupController.Instance;
            if (!_canCommit || !_committed || owner == null || !owner.Descriptor.IsCustomCompanion() ||
                !ReferenceEquals(owner.HoldingState, game.Player.CrossSceneState) || ui.ServiceWindow == null ||
                ui.ServiceWindow.WindowTabs.IsShow || ui.SelectionManagerPC == null || group == null || game.IsControllerGamepad)
                throw new InvalidOperationException("Native sheet capture requires the exact registered disposable mercenary and idle desktop service UI.");
            var sheet = ui.ServiceWindow.WindowTabs.SubWindowsList.Select(pair => pair.SubWindow)
                .OfType<CharacterScreenController>().Single();
            var characterField = typeof(CharacterScreenController).GetField("m_CurrentCharacter", Members);
            var sectionField = typeof(CharacterScreenController).GetField("m_CurrentSection", Members);
            var originalCharacter = (UnitDescriptor)characterField.GetValue(sheet);
            int originalSection = (int)sectionField.GetValue(sheet);
            var originalGroupCharacter = group.GetCurrentCharacter();
            var restoreCharacter = originalCharacter ?? originalGroupCharacter?.Descriptor;
            if (restoreCharacter == null || sheet.IsShow)
                throw new InvalidOperationException("Native sheet has no unambiguous original character or is already open.");
            var originalSelection = ui.SelectionManagerPC.SelectedUnits.ToArray();
            var originalParty = game.Player.Party.ToArray();
            var originalArea = game.CurrentlyLoadedArea;
            var originalTime = game.Player.GameTime;
            bool originalPause = game.IsPaused;
            bool opened = false;
            var evidence = new JObject { ["ownerId"] = owner.UniqueId, ["status"] = "pending",
                ["originalCharacter"] = originalCharacter?.Unit.UniqueId, ["originalSection"] = originalSection };
            _character["nativeCharacterSheet"] = evidence;
            Func<bool> ownsSheet = () => ReferenceEquals(_unit, owner) && _committed &&
                ReferenceEquals(owner.HoldingState, game.Player.CrossSceneState) &&
                ui.ServiceWindow.WindowTabs.IsShow && sheet.IsShow && sheet.gameObject.activeInHierarchy &&
                ReferenceEquals(characterField.GetValue(sheet), owner.Descriptor) &&
                (int)sectionField.GetValue(sheet) == 1 && game.IsPaused &&
                ReferenceEquals(ui.LevelUpController, _globalControllerBefore) && _build.LevelUpController == null;
            try
            {
                game.IsPaused = true;
                for (int frame = 0; frame < 60; frame++) yield return 0;
                if (_build.IsShow || _build.LevelUpController != null)
                    throw new InvalidOperationException("The owned native creator did not release its UI after commit.");
                // Native OnHide clears the visible backend but leaves the global
                // reference. Use the regression's existing owned cleanup before
                // opening another native window; the committed actor stays alive.
                CloseOwnedCreatorController();
                evidence["creatorBackendRestored"] = ReferenceEquals(ui.LevelUpController, _globalControllerBefore);
                if (!(bool)evidence["creatorBackendRestored"])
                    throw new InvalidOperationException("Owned creator backend was not restored before the sheet.");
                var totalFits = _build.GetComponentsInChildren<FirearmNativeTotalFit>(true);
                evidence["totalLayoutRestored"] = totalFits.All(fit => fit.Restored);
                if (!(bool)evidence["totalLayoutRestored"])
                    throw new InvalidOperationException("Native Total layout did not restore after creator commit.");
                opened = true;
                ui.ServiceWindow.HandleOpenCharScreen();
                for (int frame = 0; frame < 60; frame++) yield return 0;
                if (!ui.ServiceWindow.WindowTabs.IsShow || !sheet.IsShow)
                    throw new InvalidOperationException("Native character sheet did not open.");
                sheet.SetCharacter(owner.Descriptor);
                sheet.ShowSection(1);
                for (int frame = 0; frame < 60; frame++) yield return 0;
                if (!ownsSheet()) throw new InvalidOperationException("Native character sheet lost the exact disposable owner.");
                foreach (int frame in CaptureNativeFactSlots(sheet.Abilities.transform, owner.Descriptor,
                    "character-sheet", ownsSheet)) yield return frame;
                evidence["status"] = "native-selected-fact-captured";
                sheet.SetCharacter(restoreCharacter);
                sheet.ShowSection(originalSection);
                ui.ServiceWindow.HandleOpenCharScreen();
                opened = false;
                for (int frame = 0; frame < 60; frame++) yield return 0;
            }
            finally
            {
                // Clear every displayed fixture fact through the normal setter
                // before its actor is disposed, including an initially idle sheet.
                if (ReferenceEquals(characterField.GetValue(sheet), owner.Descriptor)) sheet.SetCharacter(restoreCharacter);
                if ((int)sectionField.GetValue(sheet) != originalSection) sheet.ShowSection(originalSection);
                if (opened && ui.ServiceWindow.WindowTabs.IsShow && sheet.IsShow) ui.ServiceWindow.HandleOpenCharScreen();
                if (originalCharacter == null && ReferenceEquals(characterField.GetValue(sheet), restoreCharacter))
                    characterField.SetValue(sheet, null);
                game.IsPaused = originalPause;
                evidence["characterRestored"] = ReferenceEquals(characterField.GetValue(sheet), originalCharacter);
                evidence["sectionRestored"] = (int)sectionField.GetValue(sheet) == originalSection;
                evidence["serviceClosed"] = !ui.ServiceWindow.WindowTabs.IsShow && !sheet.IsShow;
                evidence["groupUnchanged"] = ReferenceEquals(group.GetCurrentCharacter(), originalGroupCharacter);
                evidence["selectionUnchanged"] = CharacterCreationObservationIdentity.SameOrderedReferences(
                    originalSelection, ui.SelectionManagerPC.SelectedUnits.ToArray());
                evidence["partyUnchanged"] = CharacterCreationObservationIdentity.SameOrderedReferences(originalParty, game.Player.Party.ToArray());
                evidence["areaAndTimeUnchanged"] = ReferenceEquals(game.CurrentlyLoadedArea, originalArea) && game.Player.GameTime == originalTime;
                evidence["pauseRestored"] = game.IsPaused == originalPause;
                Write();
                if (evidence.Properties().Where(value => value.Value.Type == JTokenType.Boolean).Any(value => !(bool)value.Value))
                    throw new InvalidOperationException("Native character sheet did not restore its exact UI/world context: " + evidence);
            }
        }
    }
}
