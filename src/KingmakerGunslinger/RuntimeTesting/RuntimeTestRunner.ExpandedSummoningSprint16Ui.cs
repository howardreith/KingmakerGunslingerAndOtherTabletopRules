using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.UI.ActionBar;
using Kingmaker.UI.Group;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UI.ServiceWindow.CharacterScreen;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private bool _sprint16CombatComplete;
        private IEnumerator<int> _sprint16FinalReview;
        private readonly JArray _sprint16FinalRows = new JArray();

        private void PollSprint16FinalReview()
        {
            try
            {
                if (_sprint16FinalReview == null) _sprint16FinalReview = ReviewSprint16FinalCases().GetEnumerator();
                if (_sprint16FinalReview.MoveNext()) return;
            }
            catch (Exception exception)
            {
                Sprint16Check(_crocodilianAssertions, _sprint16FinalRows, "final-review-exception", false,
                    new JObject { ["exception"] = exception.ToString() }, "bounded UI, lifecycle and route checks complete");
            }
            finally
            {
                File.WriteAllText(Path.Combine(_request.EvidenceDirectory, "sprint16-final-review.json"),
                    _sprint16FinalRows.ToString(Formatting.Indented));
            }
            if (_sprint16FinalReview != null) _sprint16FinalReview.Dispose();
            Complete(CreateResult(_crocodilianAssertions.All(value => value.Status == RuntimeTestStatuses.Pass)
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, _crocodilianAssertions, null));
        }

        private IEnumerable<int> ReviewSprint16FinalCases()
        {
            ExpandedSummoningCorrectionFixture fixture = null;
            bool cleaned = false;
            var game = Game.Instance;
            bool mode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool pause = game.IsPaused;
            TimeSpan clock = game.Player.GameTime;
            UnitEntityData[] awake = game.State.AwakeUnits.ToArray();
            UnitEntityData[] selection = game.UI.SelectionManagerPC.SelectedUnits.ToArray();
            UnitEntityData groupCharacter = GroupController.Instance.GetCurrentCharacter();
            try
            {
                // The preceding matrix already exercises real commands in
                // both modes. This synchronous lifecycle drill must use the
                // RTWP buff clock, not inherit an unrelated final TB turn.
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                game.TurnBasedCombatController.Activate();
                if (TurnBased.Controllers.CombatController.IsInTurnBasedCombat())
                    throw new InvalidOperationException("Final lifecycle fixture did not enter its explicit RTWP scope.");
                fixture = BeginExpandedSummoningCorrectionFixture("KMG_Runtime_Sprint16_FinalCaster");
                CreateExpandedSummoningCorrectionHostile(fixture);
                foreach (int frame in ReviewSprint16NativeUi(fixture)) yield return frame;
                foreach (int frame in ReviewSprint16Lifecycle(fixture)) yield return frame;
                ExerciseSprint16Routes(fixture);
                foreach (int frame in ReviewSprint16Fallback(fixture)) yield return frame;
            }
            finally
            {
                try { EndExpandedSummoningCorrectionFixture(fixture, out cleaned); }
                finally
                {
                    try
                    {
                        SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = mode;
                        game.TurnBasedCombatController.Activate();
                        GroupController.Instance.SelectUnit(groupCharacter);
                        game.UI.SelectionManagerPC.MultiSelect(selection.Select(value => value.View).ToArray(), false);
                    }
                    finally
                    {
                        game.State.AwakeUnits.Clear();
                        game.State.AwakeUnits.AddRange(awake);
                        game.Player.GameTime = clock;
                        game.IsPaused = pause;
                    }
                }
                bool scopeRestored = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue == mode &&
                    game.Player.GameTime == clock && game.IsPaused == pause &&
                    game.State.AwakeUnits.SequenceEqual(awake) &&
                    game.UI.SelectionManagerPC.SelectedUnits.SequenceEqual(selection) &&
                    ReferenceEquals(GroupController.Instance.GetCurrentCharacter(), groupCharacter);
                Sprint16Check(_crocodilianAssertions, _sprint16FinalRows, "final-fixture-cleanup", cleaned,
                    new JObject { ["cleaned"] = cleaned }, "exact pre-fixture unit and party references restored");
                Sprint16Check(_crocodilianAssertions, _sprint16FinalRows, "final-lifecycle-scope-restoration", scopeRestored,
                    new JObject { ["restored"] = scopeRestored, ["configuredModeRestored"] =
                        SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue == mode,
                        ["selectionRestored"] = game.UI.SelectionManagerPC.SelectedUnits.SequenceEqual(selection),
                        ["groupRestored"] = ReferenceEquals(GroupController.Instance.GetCurrentCharacter(), groupCharacter),
                        ["clockRestored"] = game.Player.GameTime == clock, ["pauseRestored"] = game.IsPaused == pause,
                        ["awakeRestored"] = game.State.AwakeUnits.SequenceEqual(awake) },
                    "request-local RTWP lifecycle scope restores the exact preceding mode, selection, group, clock, pause and awake census");
            }
        }

        private IEnumerable<int> ReviewSprint16NativeUi(ExpandedSummoningCorrectionFixture fixture)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var game = Game.Instance;
            var ui = game.UI;
            var group = GroupController.Instance;
            if (ui.ServiceWindow == null || ui.ServiceWindow.WindowTabs.IsShow ||
                ui.SelectionManagerPC == null || group == null || game.IsControllerGamepad)
                throw new InvalidOperationException("Crocodilian native UI requires idle desktop service UI.");
            var sheet = ui.ServiceWindow.WindowTabs.SubWindowsList.Select(value => value.SubWindow)
                .OfType<CharacterScreenController>().Single();
            var characterField = typeof(CharacterScreenController).GetField("m_CurrentCharacter", flags);
            var sectionField = typeof(CharacterScreenController).GetField("m_CurrentSection", flags);
            var originalCharacter = (UnitDescriptor)characterField.GetValue(sheet);
            int section = (int)sectionField.GetValue(sheet);
            UnitEntityData groupCharacter = group.GetCurrentCharacter();
            UnitDescriptor restore = originalCharacter ?? groupCharacter?.Descriptor;
            var selected = ui.SelectionManagerPC.SelectedUnits.ToArray();
            var clock = game.Player.GameTime;
            bool pause = game.IsPaused;
            bool opened = false;
            var owners = new List<UnitEntityData>();
            var widgets = new List<ActionBarSpontaneousConvertedSlot>();
            UnityEngine.Random.State random = UnityEngine.Random.state;
            if (restore == null || sheet.IsShow) throw new InvalidOperationException("No exact idle sheet restoration owner.");
            try
            {
                game.IsPaused = true;
                // Use the real native converted-row prefab and binding path;
                // no icon, label, availability overlay or mechanic is painted
                // by the fixture. This is widget rendering, not menu navigation.
                var subGroup = Resources.FindObjectsOfTypeAll<ActionBarSpellsGroup>().First();
                var prefab = (ActionBarSpontaneousConvertedSlot)typeof(ActionBarSpellsGroup)
                    .GetField("m_ActionBarSpontaneousConvertedSlotPrefab", flags).GetValue(subGroup);
                foreach (string key in CrocodilianVisualPolicy.Keys)
                {
                    UnitEntityData owner = CastExpandedSummoningOwnTier(fixture, key);
                    owners.Add(owner);
                    SetExpandedSummoningBrainActive(owner, false);
                    var sprint = Sprint16Sprint(fixture.Blueprints, key);
                    var ability = owner.Descriptor.Abilities.GetAbility(sprint);
                    var widget = Kingmaker.UI.WidgetFactory.GetWidget(prefab);
                    widgets.Add(widget);
                    widget.transform.SetParent(group.transform, false);
                    widget.Initialize();
                    widget.Set(owner, new AbilityData(ability));
                    for (int frame = 0; frame < 8; frame++) yield return 0;
                    bool ready = new AbilityData(ability).IsAvailable;
                    bool icon = widget.gameObject.activeInHierarchy && widget.Icon.isActiveAndEnabled &&
                        ReferenceEquals(widget.Icon.sprite, sprint.Icon);
                    ExecuteExpandedSummoningRuntimeAbility(owner, sprint, 0, new TargetWrapper(owner), false);
                    for (int frame = 0; frame < 8; frame++) yield return 0;
                    Sprint16Check(_crocodilianAssertions, _sprint16FinalRows, key + "-native-sprint-row",
                        ready && icon && !new AbilityData(ability).IsAvailable &&
                        ReferenceEquals(widget.Icon.sprite, sprint.Icon),
                        new JObject { ["owner"] = owner.UniqueId, ["ability"] = sprint.AssetGuid,
                            ["sprite"] = widget.Icon.sprite == null ? null : widget.Icon.sprite.name,
                            ["active"] = widget.gameObject.activeInHierarchy,
                            ["availableBefore"] = ready, ["availableAfter"] = new AbilityData(ability).IsAvailable,
                            ["surface"] = "native converted-slot prefab bound on the live portrait strip; not ordinary menu navigation" },
                        "actual native row retains the exact Sprint emblem across a real cooldown application");
                }
                opened = true;
                ui.ServiceWindow.HandleOpenCharScreen();
                for (int frame = 0; frame < 10; frame++) yield return 0;
                int buffSection = sheet.BuffsAndConditions.SectionGroupIndex.First();
                foreach (UnitEntityData owner in owners)
                {
                    sheet.SetCharacter(owner.Descriptor);
                    sheet.ShowSection(buffSection);
                    sheet.BuffsAndConditions.SetDirty();
                    sheet.Refresh();
                    for (int frame = 0; frame < 12; frame++) yield return 0;
                    Buff[] expected = owner.Buffs.Enumerable.Where(value => value.Blueprint.name.StartsWith(
                        owner.Blueprint.name.Replace("_Unit_", "_Special_") + "_", StringComparison.Ordinal) &&
                        (value.Blueprint.name.EndsWith("_CombatTraits", StringComparison.Ordinal) ||
                         value.Blueprint.name.EndsWith("_SprintState", StringComparison.Ordinal) ||
                         value.Blueprint.name.EndsWith("_SprintCooldown", StringComparison.Ordinal))).ToArray();
                    Sprint16Check(_crocodilianAssertions, _sprint16FinalRows, owner.Blueprint.name + "-visible-buff-count",
                        expected.Length == 3, new JObject { ["buffs"] = new JArray(expected.Select(value => value.Blueprint.name)) },
                        "exact combat trait, active Sprint and cooldown buffs");
                    foreach (Buff buff in expected) ObserveSprint16BuffRow(sheet, owner, buff);
                }
                UnitEntityData dire = owners.Single(value => value.Blueprint.name == "KMG_Summoning_Unit_DireCrocodile");
                SummonGrabComponent grab = SummonGrabComponent.Find(dire);
                Buff held = Sprint16EstablishHold(fixture, dire, grab, Size.Large, true);
                int claimed = -1;
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                SummonHoldComponent.MaintainLink(dire, fixture.Hostile, grab, null,
                    dire.Descriptor.Buffs.GetBuff(grab.HoldBuff), held, ref claimed);
                Buff swallowed = fixture.Hostile.Descriptor.Buffs.GetBuff(grab.SwallowedBuff);
                sheet.SetCharacter(fixture.Hostile.Descriptor);
                sheet.ShowSection(buffSection);
                sheet.BuffsAndConditions.SetDirty();
                sheet.Refresh();
                for (int frame = 0; frame < 12; frame++) yield return 0;
                if (swallowed == null) throw new InvalidOperationException("Native UI case did not establish the creature-owned swallowed state.");
                ObserveSprint16BuffRow(sheet, fixture.Hostile, swallowed);
                Sprint16Release(fixture, dire, grab);
            }
            finally
            {
                sheet.SetCharacter(restore);
                sheet.ShowSection(section);
                if (opened && ui.ServiceWindow.WindowTabs.IsShow && sheet.IsShow) ui.ServiceWindow.HandleOpenCharScreen();
                // Tabs unbind their panel before closing. The detached sheet
                // can retain IsShow; close the exact pane we opened through
                // its native API before restoring its cached character.
                if (opened && sheet.IsShow) sheet.Show(false);
                if (originalCharacter == null && ReferenceEquals(characterField.GetValue(sheet), restore))
                    characterField.SetValue(sheet, null);
                foreach (var widget in widgets) Kingmaker.UI.WidgetFactory.DisposeWidget(widget);
                foreach (var row in sheet.BuffsAndConditions.GetComponentsInChildren<CharSComponentBuffSlot>(true))
                    if (row.Buff != null && (owners.Contains(row.Buff.Owner.Unit) || ReferenceEquals(row.Buff.Owner.Unit, fixture.Hostile)))
                    { row.Clear(); row.Buff = null; row.Hide(true); }
                ResetExpandedSummoningHostile(fixture);
                DisposeExpandedSummoningUnits(fixture.Created, owners.ToArray());
                UnityEngine.Random.state = random;
                group.SelectUnit(groupCharacter);
                ui.SelectionManagerPC.MultiSelect(selected.Select(value => value.View).ToArray(), false);
                game.Player.GameTime = clock;
                game.IsPaused = pause;
                bool restored = !ui.ServiceWindow.WindowTabs.IsShow && !sheet.IsShow &&
                    ReferenceEquals(characterField.GetValue(sheet), originalCharacter) &&
                    (int)sectionField.GetValue(sheet) == section && ReferenceEquals(group.GetCurrentCharacter(), groupCharacter) &&
                    ui.SelectionManagerPC.SelectedUnits.SequenceEqual(selected) && game.Player.GameTime == clock;
                Sprint16Check(_crocodilianAssertions, _sprint16FinalRows, "native-ui-restoration", restored,
                    new JObject { ["restored"] = restored,
                        ["tabsClosed"] = !ui.ServiceWindow.WindowTabs.IsShow, ["sheetClosed"] = !sheet.IsShow,
                        ["characterRestored"] = ReferenceEquals(characterField.GetValue(sheet), originalCharacter),
                        ["sectionRestored"] = (int)sectionField.GetValue(sheet) == section,
                        ["groupRestored"] = ReferenceEquals(group.GetCurrentCharacter(), groupCharacter),
                        ["selectionRestored"] = ui.SelectionManagerPC.SelectedUnits.SequenceEqual(selected),
                        ["clockBefore"] = clock.ToString(), ["clockAfter"] = game.Player.GameTime.ToString(),
                        ["pauseRestored"] = game.IsPaused == pause },
                    "exact sheet, selection, group, pause and game time restored");
            }
            for (int frame = 0; frame < 5; frame++) yield return 0;
        }

        private void ObserveSprint16BuffRow(CharacterScreenController sheet, UnitEntityData owner, Buff buff)
        {
            var rows = sheet.BuffsAndConditions.GetComponentsInChildren<CharSComponentBuffSlot>(true)
                .Where(value => value.gameObject.activeInHierarchy && ReferenceEquals(value.Buff, buff)).ToArray();
            var row = rows.SingleOrDefault();
            bool exact = row != null && sheet.IsShow && sheet.BuffsAndConditions.IsShowed &&
                ReferenceEquals(buff.Owner.Unit, owner) && row.Icon.isActiveAndEnabled &&
                ReferenceEquals(row.Icon.sprite, buff.Blueprint.Icon) && row.Name.text == buff.Name &&
                !row.Name.isTextTruncated;
            Sprint16Check(_crocodilianAssertions, _sprint16FinalRows, buff.Blueprint.name + "-native-buff-row", exact,
                new JObject { ["owner"] = owner.UniqueId, ["buff"] = buff.Blueprint.AssetGuid,
                    ["rows"] = rows.Length, ["sprite"] = row == null || row.Icon.sprite == null ? null : row.Icon.sprite.name,
                    ["label"] = row == null ? null : row.Name.text, ["expectedLabel"] = buff.Name },
                "one actual character-sheet buff row with the exact source-owned emblem and full label");
        }
    }
}
