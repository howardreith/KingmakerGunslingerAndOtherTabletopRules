using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UI.Group;
using Kingmaker.UI.LevelUp;
using Kingmaker.UI.ServiceWindow.CharacterScreen;
using Kingmaker.UnitLogic;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Reads the rendered ability block of the native character sheet
    /// (CharacterScreenController.AbilityScores, filled by CharSScores through
    /// CharSScoreEntry.SetData with showPermanentValue false). Each of the six
    /// visible entries must be bound to the owner's live stat and display the live
    /// ModifiedValue. Kingmaker 2.1.7b's sheet entries have no RaceBonus field
    /// (native SetData skips the label when it is null; observed 2026-11-26), so
    /// the racial label is compared only where the sheet actually renders one.
    /// </summary>
    internal static class NativeSheetRacialLabelProbe
    {
        private const BindingFlags Members = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        internal static int AbilitySection(CharacterScreenController sheet, int preferred)
        {
            var groups = sheet.AbilityScores.SectionGroupIndex;
            if (groups == null || groups.Count == 0 || groups.Contains(preferred)) return preferred;
            return groups[0];
        }

        internal static JObject ObserveSheet(CharacterScreenController sheet, UnitDescriptor owner,
            int[] expectedRacial, out bool exact)
        {
            var entries = sheet.AbilityScores.GetComponentsInChildren<CharSScoreEntry>(true)
                .Where(value => !(value is CharBScoresEntry)).ToArray();
            var rendered = new JArray();
            bool all = sheet.AbilityScores.gameObject.activeInHierarchy;
            for (int index = 0; index < ElementalRacialLabelObservation.AbilityStats.Length; index++)
            {
                StatType type = ElementalRacialLabelObservation.AbilityStats[index];
                ModifiableValue stat = owner.Stats.GetStat(type);
                var bound = entries.Where(value => value.Type == type && ReferenceEquals(value.Stat, stat) &&
                    value.gameObject.activeInHierarchy).ToArray();
                var entry = bound.Length == 1 ? bound[0] : null;
                string label = entry?.RaceBonus?.text;
                string shown = entry?.Value?.text;
                string wantedLabel = ElementalRacialLabelObservation.ExpectedLabel(expectedRacial[index]);
                string wantedValue = stat.ModifiedValue.ToString();
                bool raceBonusField = entry != null && entry.RaceBonus != null;
                bool same = entry != null && (!raceBonusField || label == wantedLabel) && shown == wantedValue;
                all &= same;
                rendered.Add(new JObject { ["stat"] = type.ToString(), ["boundVisibleEntries"] = bound.Length,
                    ["raceBonusFieldPresent"] = raceBonusField,
                    ["renderedRaceBonus"] = label, ["expectedRaceBonus"] = wantedLabel,
                    ["renderedValue"] = shown, ["liveModifiedValue"] = stat.ModifiedValue,
                    ["livePermanentValue"] = stat.PermanentValue, ["baseValue"] = stat.BaseValue,
                    ["exact"] = same });
            }
            bool probeExact;
            JObject probe = ElementalRacialLabelObservation.Observe(owner, expectedRacial, null, out probeExact);
            exact = all && probeExact;
            return new JObject { ["abilityBlockActive"] = sheet.AbilityScores.gameObject.activeInHierarchy,
                ["rendered"] = rendered, ["stats"] = probe, ["exact"] = exact };
        }

        /// <summary>
        /// Runs a capture so that its failure is recorded on the evidence (exact
        /// false) instead of aborting the hosting scenario; the capture's own
        /// finally block restores the sheet when it is disposed.
        /// </summary>
        internal static IEnumerable<int> Isolated(IEnumerable<int> capture, JObject evidence)
        {
            IEnumerator<int> inner = capture.GetEnumerator();
            try
            {
                while (true)
                {
                    bool more = false;
                    bool failed = false;
                    try { more = inner.MoveNext(); }
                    catch (Exception exception)
                    {
                        evidence["exception"] = exception.ToString();
                        evidence["exact"] = false;
                        failed = true;
                    }
                    if (failed || !more) yield break;
                    yield return 0;
                }
            }
            finally
            {
                inner.Dispose();
            }
        }

        /// <summary>
        /// Opens the native character sheet for an existing player-faction actor, observes
        /// its rendered ability block, then restores the sheet character, section,
        /// window and pause state. Throws when the desktop service UI is not idle.
        /// </summary>
        internal static IEnumerable<int> CaptureForActor(UnitDescriptor owner, int[] expectedRacial,
            JObject evidence)
        {
            var game = Game.Instance;
            var ui = game.UI;
            var group = GroupController.Instance;
            if (owner == null || owner.Unit == null || !owner.IsPlayerFaction ||
                ui.ServiceWindow == null || ui.ServiceWindow.WindowTabs.IsShow || group == null || game.IsControllerGamepad)
                throw new InvalidOperationException("Native sheet label capture requires a player-faction actor and idle desktop service UI.");
            evidence["partyMember"] = game.Player.Party.Any(value => ReferenceEquals(value.Descriptor, owner));
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
            var originalSelection = ui.SelectionManagerPC == null ? null :
                ui.SelectionManagerPC.SelectedUnits.ToArray();
            bool originalPause = game.IsPaused;
            int section = AbilitySection(sheet, originalSection);
            bool opened = false;
            evidence["status"] = "pending";
            evidence["ownerId"] = owner.Unit.UniqueId;
            evidence["abilitySection"] = section;
            try
            {
                game.IsPaused = true;
                for (int frame = 0; frame < 10; frame++) yield return 0;
                opened = true;
                ui.ServiceWindow.HandleOpenCharScreen();
                for (int frame = 0; frame < 60; frame++) yield return 0;
                if (!ui.ServiceWindow.WindowTabs.IsShow || !sheet.IsShow)
                    throw new InvalidOperationException("Native character sheet did not open.");
                sheet.SetCharacter(owner);
                sheet.ShowSection(section);
                for (int frame = 0; frame < 60; frame++) yield return 0;
                if (!sheet.IsShow || !ReferenceEquals(characterField.GetValue(sheet), owner) ||
                    (int)sectionField.GetValue(sheet) != section)
                    throw new InvalidOperationException("Native character sheet lost the exact owner or section.");
                bool exact;
                evidence["observation"] = ObserveSheet(sheet, owner, expectedRacial, out exact);
                evidence["exact"] = exact;
                evidence["status"] = "observed";
                sheet.SetCharacter(restoreCharacter);
                sheet.ShowSection(originalSection);
                ui.ServiceWindow.HandleOpenCharScreen();
                opened = false;
                for (int frame = 0; frame < 30; frame++) yield return 0;
            }
            finally
            {
                if (ReferenceEquals(characterField.GetValue(sheet), owner)) sheet.SetCharacter(restoreCharacter);
                if ((int)sectionField.GetValue(sheet) != originalSection) sheet.ShowSection(originalSection);
                if (opened && ui.ServiceWindow.WindowTabs.IsShow && sheet.IsShow) ui.ServiceWindow.HandleOpenCharScreen();
                if (originalCharacter == null && ReferenceEquals(characterField.GetValue(sheet), restoreCharacter))
                    characterField.SetValue(sheet, null);
                game.IsPaused = originalPause;
                var restored = new JObject {
                    ["characterRestored"] = ReferenceEquals(characterField.GetValue(sheet), originalCharacter),
                    ["sectionRestored"] = (int)sectionField.GetValue(sheet) == originalSection,
                    ["serviceClosed"] = !ui.ServiceWindow.WindowTabs.IsShow && !sheet.IsShow,
                    ["groupUnchanged"] = ReferenceEquals(group.GetCurrentCharacter(), originalGroupCharacter),
                    ["selectionUnchanged"] = ui.SelectionManagerPC == null ||
                        CharacterCreationObservationIdentity.SameOrderedReferences(
                            originalSelection, ui.SelectionManagerPC.SelectedUnits.ToArray()),
                    ["pauseRestored"] = game.IsPaused == originalPause };
                evidence["restoration"] = restored;
                if (restored.Properties().Any(value => !(bool)value.Value)) evidence["exact"] = false;
            }
        }
    }
}
