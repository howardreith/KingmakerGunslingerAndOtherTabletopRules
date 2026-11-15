using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Assets.UI.LevelUp;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.Group;
using Kingmaker.UI.LevelUp;
using Kingmaker.UI.LevelUp.Phase;
using Kingmaker.UI.ServiceWindow.CharacterScreen;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KmgLoaderCompatHarness
{
    /// <summary>Shared pieces of the screen probes: frame pacing, private-member reads and screenshots.</summary>
    internal abstract class ScreenProbe
    {
        protected const BindingFlags Any = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private readonly string directory;
        private readonly string label;
        private int settle;
        protected readonly JObject Record = new JObject();
        protected readonly JArray Screenshots = new JArray();

        protected ScreenProbe(string directory, string label)
        {
            this.directory = directory;
            this.label = label;
            Record["screenshots"] = Screenshots;
        }

        public JObject Result => Record;

        /// <summary>Null while running, an empty string when done, otherwise the failure.</summary>
        public string Poll()
        {
            if (settle > 0)
            {
                settle--;
                return null;
            }

            return Step();
        }

        protected abstract string Step();

        protected void Wait(int frames) => settle = frames;

        protected static T Field<T>(object instance, string name) where T : class
        {
            for (Type type = instance.GetType(); type != null; type = type.BaseType)
            {
                FieldInfo field = type.GetField(name, Any | BindingFlags.DeclaredOnly);
                if (field != null) return field.GetValue(instance) as T;
            }

            throw new MissingFieldException(instance.GetType().FullName, name);
        }

        /// <summary>Asks Unity for a picture of the game view as drawn; it is written at the end of the frame.</summary>
        protected void Capture(string name)
        {
            string path = Path.Combine(directory, label + "-" + name + ".png");
            if (File.Exists(path)) File.Delete(path);
            ScreenCapture.CaptureScreenshot(path);
            Screenshots.Add(path);
        }

        protected string MissingScreenshots()
        {
            string[] missing = Screenshots.Select(token => (string)token).Where(path => !File.Exists(path) || new FileInfo(path).Length == 0).ToArray();
            return missing.Length == 0 ? null : "screenshots were not written: " + string.Join(", ", missing);
        }
    }

    /// <summary>
    /// Opens the game's own new-character creation the way the New Game window does for a custom character, walks to
    /// the class screen, reads the list the screen actually built, clicks the wanted class through its own toggle and
    /// presses the screen's Next button. Nothing is committed; the launch ends with the creator still open.
    /// </summary>
    internal sealed class ChargenClassScreenProbe : ScreenProbe
    {
        private enum Stage { Start, WaitShown, ReachClass, ReadList, Select, Selected, Advance, Advanced, Finish }

        private readonly string classGuid;
        private readonly string[] labelContains;
        private Stage stage = Stage.Start;
        private CharacterBuildController build;
        private CharBuildSelectorItem wanted;
        private int waited;
        private int presses;

        public ChargenClassScreenProbe(string directory, string label, string classGuid, IEnumerable<string> labelContains) : base(directory, label)
        {
            this.classGuid = classGuid;
            this.labelContains = labelContains.ToArray();
            Record["phases"] = new JArray();
        }

        protected override string Step()
        {
            switch (stage)
            {
                case Stage.Start:
                    build = Game.Instance.UI.CharacterBuildController;
                    if (build == null || build.IsShow || build.LevelUpController != null) return "the character creator is not idle at the main menu";
                    Game.NewGamePreset = BlueprintRoot.Instance.NewGamePreset;
                    // NewGameWindowController.OpenCharGen raises exactly this; a null pregen is the custom character.
                    EventBus.RaiseEvent<IUIStartChargen>(handler => handler.HandleStartChargen(null));
                    stage = Stage.WaitShown;
                    Wait(30);
                    return null;

                case Stage.WaitShown:
                    if (!build.IsShow || build.LevelUpController == null || !build.CurrentPhase.HasValue)
                    {
                        if (++waited > 600) return "the character creator did not open";
                        return null;
                    }

                    Record["mode"] = build.LevelUpController.State.Mode.ToString();
                    stage = Stage.ReachClass;
                    Wait(30);
                    return null;

                case Stage.ReachClass:
                    ((JArray)Record["phases"]).Add(build.CurrentPhase.ToString());
                    if (build.CurrentPhase == CharBPhase.Type.ClassInChargen)
                    {
                        stage = Stage.ReadList;
                        Wait(45);
                        return null;
                    }

                    if (++presses > 6) return "the class screen was not reached: " + Record["phases"];
                    PressNext();
                    Wait(45);
                    return null;

                case Stage.ReadList:
                {
                    CharBPhaseClassInChargen phase = build.ClassInChargen;
                    if (phase == null) return "the creator has no class screen";
                    CharBSelectorLayer layer = Field<CharBFeatureSelector>(phase, "m_Selector").SelectorLayerBody;
                    CharBuildSelectorItem[] items = (layer.SelectorItems ?? new List<CharBuildSelectorItem>())
                        .Where(item => item != null && item.gameObject.activeSelf && item.CharacterClass != null).ToArray();
                    JArray list = new JArray();
                    foreach (CharBuildSelectorItem item in items)
                    {
                        list.Add(new JObject
                        {
                            ["guid"] = item.CharacterClass.AssetGuid, ["blueprint"] = item.CharacterClass.name,
                            ["label"] = Field<TextMeshProUGUI>(item, "m_ItemName").text, ["interactable"] = item.Toggle.interactable
                        });
                    }

                    Record["classList"] = list;
                    Record["classListCount"] = items.Length;
                    Record["duplicateGuids"] = new JArray(items.GroupBy(item => item.CharacterClass.AssetGuid).Where(group => group.Count() > 1).Select(group => (object)group.Key).ToArray());
                    CharBuildSelectorItem[] matches = items.Where(item => item.CharacterClass.AssetGuid == classGuid).ToArray();
                    Record["wantedCount"] = matches.Length;
                    if (matches.Length != 1) return "the class screen lists the class " + matches.Length + " times, expected exactly 1";
                    wanted = matches[0];
                    string text = Field<TextMeshProUGUI>(wanted, "m_ItemName").text;
                    Record["wantedLabel"] = text;
                    if (string.IsNullOrWhiteSpace(text) || text.IndexOf("<null>", StringComparison.Ordinal) >= 0 || text.StartsWith("[unknown key", StringComparison.Ordinal))
                        return "the class is listed without a real label: '" + text + "'";
                    foreach (string part in labelContains)
                        if (text.IndexOf(part, StringComparison.Ordinal) < 0) return "the class label '" + text + "' does not contain '" + part + "'";
                    if (!wanted.Toggle.interactable) return "the class is listed but cannot be selected";
                    ScrollRect scroll = wanted.GetComponentInParent<ScrollRect>();
                    int index = Array.IndexOf(items, wanted);
                    if (scroll != null && items.Length > 1) scroll.verticalNormalizedPosition = 1f - (float)index / (items.Length - 1);
                    stage = Stage.Select;
                    Wait(20);
                    return null;
                }

                case Stage.Select:
                    Capture("class-list");
                    // The item's own toggle: its listener plays the click and calls the creator's SetClass.
                    wanted.Toggle.isOn = true;
                    stage = Stage.Selected;
                    Wait(60);
                    return null;

                case Stage.Selected:
                {
                    BlueprintCharacterClass selected = build.LevelUpController.State.SelectedClass;
                    Record["selectedAfterClick"] = selected == null ? null : selected.AssetGuid;
                    Record["selectedName"] = selected == null ? null : selected.Name;
                    Record["toggleOn"] = wanted.Toggle.isOn;
                    if (selected == null || selected.AssetGuid != classGuid) return "clicking the class did not select it: " + (selected == null ? "nothing selected" : selected.name);
                    Capture("class-selected");
                    stage = Stage.Advance;
                    Wait(30);
                    return null;
                }

                case Stage.Advance:
                    PressNext();
                    stage = Stage.Advanced;
                    Wait(60);
                    return null;

                case Stage.Advanced:
                {
                    Record["phaseAfterNext"] = build.CurrentPhase.ToString();
                    BlueprintCharacterClass selected = build.LevelUpController.State.SelectedClass;
                    if (build.CurrentPhase == CharBPhase.Type.ClassInChargen) return "Next did not leave the class screen";
                    if (selected == null || selected.AssetGuid != classGuid) return "the selection was lost after Next";
                    Record["previewClassLevel"] = build.LevelUpController.Preview.Progression.GetClassLevel(selected);
                    Capture("after-class");
                    stage = Stage.Finish;
                    Wait(30);
                    return null;
                }

                default:
                    return MissingScreenshots() ?? string.Empty;
            }
        }

        private void PressNext()
        {
            Button next = Field<Button>(build, "m_CompleteButton");
            Record["nextPresses"] = ++presses;
            next.onClick.Invoke();
        }
    }

    /// <summary>
    /// Applies buffs to the main character of a loaded disposable save and reads what the game's own buff views show:
    /// the party-bar icons and the character sheet's "Buffs and Conditions" rows (icon, drawn name, tooltip text).
    /// </summary>
    internal sealed class BuffUiProbe : ScreenProbe
    {
        private enum Stage { Start, PartyBar, OpenSheet, Sheet, SheetTop, SheetEnd, CloseSheet, Resume, WaitRound, Finish }

        private readonly string[] guids;
        private readonly string[] expectedNames;
        private readonly string endsNextRoundGuid;
        private double resumedAt;
        private Stage stage = Stage.Start;
        private UnitEntityData unit;
        private readonly List<BlueprintBuff> blueprints = new List<BlueprintBuff>();
        private ScrollRect sheetScroll;
        private int waited;
        private readonly List<string> incomplete = new List<string>();
        private readonly List<string> findings = new List<string>();

        public BuffUiProbe(string directory, string label, IEnumerable<string> guids, IEnumerable<string> expectedNames, string endsNextRoundGuid) : base(directory, label)
        {
            this.guids = guids.ToArray();
            this.expectedNames = expectedNames.ToArray();
            this.endsNextRoundGuid = endsNextRoundGuid;
        }

        protected override string Step()
        {
            switch (stage)
            {
                case Stage.Start:
                {
                    unit = Game.Instance.Player.MainCharacter.Value;
                    if (unit == null) return "no main character; this probe needs a loaded save";
                    if (guids.Length == 0) return "no buffs were named";
                    Record["unit"] = unit.CharacterName;
                    Record["gameMode"] = Game.Instance.CurrentMode.ToString();
                    JArray applied = new JArray();
                    incomplete.Clear();
                    foreach (string guid in guids)
                    {
                        BlueprintBuff blueprint = ResourcesLibrary.TryGetBlueprint(guid) as BlueprintBuff;
                        if (blueprint == null) return "buff " + guid + " does not resolve";
                        Buff buff = unit.Descriptor.Buffs.AddBuff(blueprint, unit, null);
                        // AddBuff returns null when Kingmaker's rule swallowed a failure while activating the buff;
                        // the buff can still be on the unit, half attached, with no "buff added" event raised.
                        Buff present = unit.Descriptor.Buffs.RawFacts.OfType<Buff>().FirstOrDefault(candidate => ReferenceEquals(candidate.Blueprint, blueprint));
                        applied.Add(new JObject
                        {
                            ["guid"] = guid, ["blueprint"] = blueprint.name, ["addBuffReturnedBuff"] = buff != null, ["onUnit"] = present != null,
                            ["active"] = present != null && present.Active, ["fxOnStartAssigned"] = blueprint.FxOnStart != null,
                            ["fxOnRemoveAssigned"] = blueprint.FxOnRemove != null
                        });
                        if (present == null) return "buff " + blueprint.name + " is not on the unit after AddBuff";
                        if (buff == null) incomplete.Add(blueprint.name);
                        blueprints.Add(blueprint);
                    }

                    Record["applied"] = applied;
                    Record["activationIncomplete"] = new JArray(incomplete.Cast<object>().ToArray());

                    // Paused once they are on, so a buff that ends at the next round is still there to look at.
                    Game.Instance.IsPaused = true;

                    GroupController.Instance.SelectUnit(unit);
                    stage = Stage.PartyBar;
                    Wait(45);
                    return null;
                }

                case Stage.PartyBar:
                {
                    GroupCharacter portrait = UnityEngine.Object.FindObjectsOfType<GroupCharacter>()
                        .FirstOrDefault(candidate => ReferenceEquals(candidate.GetType().GetProperty("Unit", Any).GetValue(candidate, null), unit));
                    if (portrait == null) return "the party bar has no portrait for the main character";
                    GroupCharacterBuffs bar = portrait.GetComponentInChildren<GroupCharacterBuffs>(true);
                    if (bar == null) return "the portrait has no buff bar";
                    List<BuffController> widgets = Field<List<BuffController>>(bar, "m_Buffs");
                    JArray icons = new JArray();
                    var problems = new List<string>();
                    foreach (BlueprintBuff blueprint in blueprints)
                    {
                        BuffController widget = widgets.FirstOrDefault(candidate => candidate != null && candidate.Buff != null && ReferenceEquals(candidate.Buff.Blueprint, blueprint));
                        string shown = widget == null || widget.Icon == null || widget.Icon.sprite == null ? null : widget.Icon.sprite.name;
                        string expected = blueprint.Icon == null ? null : blueprint.Icon.name;
                        icons.Add(new JObject { ["blueprint"] = blueprint.name, ["inPartyBar"] = widget != null, ["shownIcon"] = shown, ["blueprintIcon"] = expected });
                        if (widget == null) problems.Add(blueprint.name + " is not in the party bar");
                        else if (shown == null || shown != expected) problems.Add(blueprint.name + " party-bar icon is " + (shown ?? "missing") + ", expected " + (expected ?? "an icon"));
                    }

                    Record["partyBar"] = icons;
                    Record["partyBarWidgets"] = widgets.Count;
                    findings.AddRange(problems);
                    bar.OnTriggerPointerEnter();
                    stage = Stage.OpenSheet;
                    Wait(15);
                    return null;
                }

                case Stage.OpenSheet:
                    Capture("party-bar");
                    EventBus.RaiseEvent<IServiceWindowUIHandler>(handler => handler.HandleOpenCharScreen());
                    stage = Stage.Sheet;
                    Wait(90);
                    return null;

                case Stage.Sheet:
                {
                    CharSBuffsAndConditions section = UnityEngine.Object.FindObjectsOfType<CharSBuffsAndConditions>().FirstOrDefault(candidate => candidate.isActiveAndEnabled);
                    List<CharSComponentBuffSlot> slots = section == null ? null : Field<List<CharSComponentBuffSlot>>(section, "ViewList");
                    if (slots == null || !slots.Any(slot => slot != null && slot.gameObject.activeSelf && slot.Buff != null))
                    {
                        if (++waited > 300) return "the character sheet's Buffs and Conditions list did not appear";
                        return null;
                    }

                    JArray rows = new JArray();
                    var problems = new List<string>();
                    foreach (BlueprintBuff blueprint in blueprints)
                    {
                        CharSComponentBuffSlot slot = slots.FirstOrDefault(candidate => candidate != null && candidate.gameObject.activeSelf && candidate.Buff != null &&
                            ReferenceEquals(candidate.Buff.Blueprint, blueprint));
                        string drawnName = slot == null || slot.Name == null ? null : slot.Name.text;
                        string tooltip = slot == null ? null : slot.Description;
                        string icon = slot == null || slot.Icon == null || slot.Icon.sprite == null ? null : slot.Icon.sprite.name;
                        rows.Add(new JObject
                        {
                            ["blueprint"] = blueprint.name, ["listed"] = slot != null, ["drawnName"] = drawnName, ["tooltipText"] = tooltip, ["icon"] = icon,
                            ["blueprintIcon"] = blueprint.Icon == null ? null : blueprint.Icon.name
                        });
                        if (slot == null)
                        {
                            problems.Add(blueprint.name + " is not listed on the character sheet");
                            continue;
                        }

                        if (!IsRealText(drawnName) || drawnName != blueprint.Name) problems.Add(blueprint.name + " is drawn as '" + drawnName + "'");
                        if (!IsRealText(tooltip) || tooltip != blueprint.Description) problems.Add(blueprint.name + " tooltip text is '" + tooltip + "'");
                        if (icon == null || blueprint.Icon == null || icon != blueprint.Icon.name) problems.Add(blueprint.name + " sheet icon is " + (icon ?? "missing"));
                    }

                    Record["characterSheet"] = rows;
                    string[] drawn = rows.Select(row => (string)row["drawnName"]).ToArray();
                    foreach (string expected in expectedNames)
                        if (!drawn.Contains(expected)) problems.Add("no listed buff is drawn as '" + expected + "'");
                    findings.AddRange(problems);
                    sheetScroll = section.GetComponentInChildren<ScrollRect>(true) ?? slots.First(slot => slot != null).GetComponentInParent<ScrollRect>();
                    if (sheetScroll != null) sheetScroll.verticalNormalizedPosition = 1f;
                    stage = Stage.SheetTop;
                    Wait(20);
                    return null;
                }

                case Stage.SheetTop:
                    Capture("character-sheet");
                    Record["sheetScrolls"] = sheetScroll != null;
                    if (sheetScroll != null) sheetScroll.verticalNormalizedPosition = 0f;
                    stage = sheetScroll != null ? Stage.SheetEnd : Stage.CloseSheet;
                    Wait(30);
                    return null;

                case Stage.SheetEnd:
                    Capture("character-sheet-end");
                    stage = Stage.CloseSheet;
                    Wait(30);
                    return null;

                case Stage.CloseSheet:
                    if (string.IsNullOrEmpty(endsNextRoundGuid))
                    {
                        stage = Stage.Finish;
                        return null;
                    }

                    // The same hotkey handler closes the sheet it opened.
                    EventBus.RaiseEvent<IServiceWindowUIHandler>(handler => handler.HandleOpenCharScreen());
                    stage = Stage.Resume;
                    Wait(60);
                    return null;

                case Stage.Resume:
                    Record["modeAfterSheet"] = Game.Instance.CurrentMode.ToString();
                    Game.Instance.IsPaused = false;
                    resumedAt = Time.realtimeSinceStartup;
                    stage = Stage.WaitRound;
                    return null;

                case Stage.WaitRound:
                {
                    // A buff that ends itself at the next round must do so once time runs, without help.
                    bool present = unit.Descriptor.Buffs.RawFacts.OfType<Buff>().Any(candidate => candidate.Blueprint.AssetGuid == endsNextRoundGuid);
                    double elapsed = Time.realtimeSinceStartup - resumedAt;
                    if (present && elapsed < 25.0) return null;
                    Record["endsNextRound"] = new JObject { ["guid"] = endsNextRoundGuid, ["ended"] = !present, ["secondsAfterResume"] = Math.Round(elapsed, 1) };
                    if (present) findings.Add("the buff " + endsNextRoundGuid + " did not end by itself within 25 s of unpaused play");
                    stage = Stage.Finish;
                    return null;
                }

                default:
                    if (incomplete.Count > 0)
                        findings.Insert(0, "Kingmaker did not finish activating " + incomplete.Count + " buff(s) (AddBuff returned nothing): " + string.Join(", ", incomplete.ToArray()));
                    string missing = MissingScreenshots();
                    if (missing != null) findings.Add(missing);
                    return findings.Count == 0 ? string.Empty : string.Join("; ", findings.ToArray());
            }
        }

        private static bool IsRealText(string text)
        {
            return !string.IsNullOrWhiteSpace(text) && text != "<null>" && !text.StartsWith("[unknown key", StringComparison.Ordinal);
        }
    }
}
