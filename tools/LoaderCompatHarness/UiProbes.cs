using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Kingmaker;
using Kingmaker.Assets.UI.LevelUp;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Group;
using Kingmaker.UI.LevelUp;
using Kingmaker.UI.LevelUp.Phase;
using Kingmaker.UI.ServiceWindow.CharacterScreen;
using Kingmaker.UI.Tooltip;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
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

        private double screenshotsAwaitedFrom = -1;

        /// <summary>True while a requested picture is still being written, for at most ten seconds of real time.</summary>
        protected bool AwaitingScreenshots()
        {
            if (MissingScreenshots() == null) return false;
            if (screenshotsAwaitedFrom < 0) screenshotsAwaitedFrom = Time.realtimeSinceStartup;
            return Time.realtimeSinceStartup - screenshotsAwaitedFrom < 10.0;
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
                    // Unity writes a picture at the end of a later frame; a fixed frame count can end first.
                    if (AwaitingScreenshots()) return null;
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

    /// <summary>
    /// Arms the loaded save's main character with one weapon, has the game queue a real attack of opportunity against a
    /// disposable hostile target and runs that native command, then reads the buff the hit granted where a player sees
    /// it: the portrait's buff slot and the tooltip that slot opens. It then ends the fight, lets time run and requires
    /// the buff to end by itself. Nothing is saved.
    /// </summary>
    internal sealed class OpportunityBuffUiProbe : ScreenProbe
    {
        private enum Stage { Start, PartyBar, Tooltip, TooltipShown, Resume, WaitEnd, Finish }

        private sealed class AttackObserver : IGlobalRulebookHandler<RuleAttackWithWeapon>
        {
            public readonly JArray Attacks = new JArray();

            public void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }

            public void OnEventDidTrigger(RuleAttackWithWeapon evt)
            {
                Attacks.Add(new JObject
                {
                    ["attackOfOpportunity"] = evt.IsAttackOfOpportunity,
                    ["hit"] = evt.AttackRoll != null && evt.AttackRoll.IsHit,
                    ["result"] = evt.AttackRoll == null ? null : evt.AttackRoll.Result.ToString(),
                    ["weapon"] = evt.Weapon == null ? null : evt.Weapon.Blueprint.AssetGuid,
                    ["initiator"] = evt.Initiator == null ? null : evt.Initiator.CharacterName
                });
            }
        }

        private const int CertainHitAttackBonus = 100;
        private readonly string weaponGuid;
        private readonly string buffGuid;
        private readonly string expectedIcon;
        private readonly string[] expectedTexts;
        private readonly AttackObserver observer = new AttackObserver();
        private readonly List<string> findings = new List<string>();
        private Stage stage = Stage.Start;
        private UnitEntityData unit;
        private UnitEntityData target;
        private BlueprintItemWeapon weapon;
        private BlueprintBuff blueprint;
        private BuffController widget;
        private TooltipTrigger trigger;
        private int attackBonusBefore;
        private bool attackBonusRaised;
        private bool refreshed;
        private int armorClassBefore;
        private double resumedAt;
        private TimeSpan resumedAtGameTime;
        private int pausesLifted;

        public OpportunityBuffUiProbe(string directory, string label, string weaponGuid, string buffGuid, string expectedIcon, IEnumerable<string> expectedTexts) : base(directory, label)
        {
            this.weaponGuid = weaponGuid;
            this.buffGuid = buffGuid;
            this.expectedIcon = expectedIcon;
            this.expectedTexts = expectedTexts.ToArray();
        }

        protected override string Step()
        {
            switch (stage)
            {
                case Stage.Start:
                {
                    unit = Game.Instance.Player.MainCharacter.Value;
                    if (unit == null) return "no main character; this probe needs a loaded save";
                    weapon = ResourcesLibrary.TryGetBlueprint(weaponGuid) as BlueprintItemWeapon;
                    if (weapon == null) return "weapon " + weaponGuid + " does not resolve";
                    blueprint = ResourcesLibrary.TryGetBlueprint(buffGuid) as BlueprintBuff;
                    if (blueprint == null) return "buff " + buffGuid + " does not resolve";
                    Record["unit"] = unit.CharacterName;
                    Record["gameMode"] = Game.Instance.CurrentMode.ToString();
                    Record["screen"] = Screen.width + "x" + Screen.height;
                    Record["weapon"] = weapon.name;
                    Record["buffBlueprint"] = new JObject
                    {
                        ["blueprint"] = blueprint.name, ["name"] = blueprint.Name, ["description"] = blueprint.Description,
                        ["hiddenInUi"] = blueprint.IsHiddenInUI, ["icon"] = blueprint.Icon == null ? null : blueprint.Icon.name,
                        ["iconTexture"] = blueprint.Icon == null || blueprint.Icon.texture == null ? null : blueprint.Icon.texture.width + "x" + blueprint.Icon.texture.height,
                        ["fxOnStartAssigned"] = blueprint.FxOnStart != null, ["fxOnRemoveAssigned"] = blueprint.FxOnRemove != null
                    };
                    if (OnUnit() != null) return "the main character already has " + blueprint.name + " before any attack";

                    // Paused throughout: the attack is the game's own command, run once, with nothing else moving.
                    Game.Instance.IsPaused = true;
                    ItemSlot primary = unit.Body.PrimaryHand;
                    ItemSlot secondary = unit.Body.SecondaryHand;
                    if (secondary != null && secondary.HasItem) secondary.RemoveItem();
                    if (primary.HasItem) primary.RemoveItem();
                    ItemEntity item = Game.Instance.Player.Inventory.Add(weapon);
                    primary.InsertItem(item);
                    if (!ReferenceEquals(primary.MaybeItem, item)) return weapon.name + " did not stay in the primary hand";

                    target = SpawnHostileTarget(unit, unit.Position + new Vector3(1.5f, 0f, 0f));
                    target.Descriptor.State.Immortality.Retain();
                    unit.CombatState.JoinCombat();
                    target.CombatState.JoinCombat();
                    unit.CombatState.OnNewRound();
                    unit.CombatState.Cooldown.Initiative = 0f;
                    unit.PreviousPosition = unit.Position;
                    Record["target"] = new JObject { ["faction"] = target.Blueprint.Faction.name, ["enemy"] = target.IsEnemy(unit) };

                    attackBonusBefore = unit.Descriptor.Stats.BaseAttackBonus.BaseValue;
                    unit.Descriptor.Stats.BaseAttackBonus.BaseValue = CertainHitAttackBonus;
                    attackBonusRaised = true;
                    Record["attackBonusRaisedForCertainHit"] = CertainHitAttackBonus;
                    armorClassBefore = unit.Descriptor.Stats.AC.ModifiedValue;
                    EventBus.Subscribe(observer);
                    string refused = null;
                    for (int attempt = 0; attempt < 5 && refused == null && OnUnit() == null; attempt++) refused = Opportunity();
                    Record["attacks"] = observer.Attacks;
                    if (refused != null) return Abandon(refused);
                    Buff granted = OnUnit();
                    Record["afterOpportunityHit"] = new JObject
                    {
                        ["buffOnUnit"] = granted != null, ["buffActive"] = granted != null && granted.Active,
                        ["armorClassBefore"] = armorClassBefore, ["armorClassAfter"] = unit.Descriptor.Stats.AC.ModifiedValue,
                        ["hiddenBuffsOnUnit"] = new JArray(unit.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(buff => buff.Blueprint.IsHiddenInUI).Select(buff => (object)buff.Blueprint.name).ToArray())
                    };
                    if (granted == null) return Abandon("no attack of opportunity granted " + blueprint.name);

                    GroupController.Instance.SelectUnit(unit);
                    stage = Stage.PartyBar;
                    Wait(45);
                    return null;
                }

                case Stage.PartyBar:
                {
                    GroupCharacter portrait = UnityEngine.Object.FindObjectsOfType<GroupCharacter>()
                        .FirstOrDefault(candidate => ReferenceEquals(candidate.GetType().GetProperty("Unit", Any).GetValue(candidate, null), unit));
                    if (portrait == null) return Abandon("the party bar has no portrait for the main character");
                    GroupCharacterBuffs bar = portrait.GetComponentInChildren<GroupCharacterBuffs>(true);
                    if (bar == null) return Abandon("the portrait has no buff bar");
                    List<BuffController> widgets = Field<List<BuffController>>(bar, "m_Buffs");
                    widget = widgets.FirstOrDefault(candidate => candidate != null && candidate.Buff != null && ReferenceEquals(candidate.Buff.Blueprint, blueprint));
                    if (widget == null && !refreshed)
                    {
                        // The portrait redraws its buffs on the game's buff added/removed events. A second hit replaces
                        // the buff, as it does in play, and that removal is one of those events.
                        refreshed = true;
                        Record["inPartyBarAfterFirstHit"] = false;
                        string refused = null;
                        int before = observer.Attacks.Count;
                        for (int attempt = 0; attempt < 5 && refused == null && !HitSince(before); attempt++) refused = Opportunity();
                        if (refused != null) return Abandon(refused);
                        Wait(45);
                        return null;
                    }

                    if (!refreshed) Record["inPartyBarAfterFirstHit"] = true;
                    else if (widget != null) findings.Add(blueprint.name + " was not in the party bar after the first hit; it appeared only after a second one");
                    Sprite shown = widget == null || widget.Icon == null ? null : widget.Icon.sprite;
                    Record["partyBar"] = new JObject
                    {
                        ["inPartyBar"] = widget != null, ["visibleBuffWidgets"] = widgets.Count(candidate => candidate != null && candidate.gameObject.activeInHierarchy),
                        ["hiddenBuffWidgets"] = widgets.Count(candidate => candidate != null && candidate.Buff != null && candidate.Buff.Blueprint.IsHiddenInUI),
                        ["shownIcon"] = shown == null ? null : shown.name, ["blueprintIcon"] = blueprint.Icon == null ? null : blueprint.Icon.name,
                        ["sameSpriteAsBlueprint"] = shown != null && ReferenceEquals(shown, blueprint.Icon),
                        ["iconColor"] = widget == null || widget.Icon == null ? null : widget.Icon.color.ToString(),
                        ["iconScreenRect"] = widget == null || widget.Icon == null ? null : ScreenRect(widget.Icon.rectTransform)
                    };
                    if (blueprint.Icon == null) findings.Add(blueprint.name + " has no icon, so its slot draws a blank square");
                    else if (!string.IsNullOrEmpty(expectedIcon) && blueprint.Icon.name != expectedIcon) findings.Add(blueprint.name + " icon is " + blueprint.Icon.name + ", expected " + expectedIcon);
                    if (widget == null) findings.Add(blueprint.name + " is not in the party bar");
                    else if (shown == null || !ReferenceEquals(shown, blueprint.Icon)) findings.Add(blueprint.name + " party-bar icon is " + (shown == null ? "missing" : shown.name));
                    if (!observer.Attacks.Any(attack => (bool)attack["attackOfOpportunity"] && (bool)attack["hit"] && (string)attack["weapon"] == weaponGuid))
                        findings.Add("no attack of opportunity with " + weapon.name + " hit");
                    if (widget != null && widget.transform.parent != bar.transform) bar.OnTriggerPointerEnter();
                    stage = Stage.Tooltip;
                    Wait(15);
                    return null;
                }

                case Stage.Tooltip:
                    Capture("party-bar");
                    stage = Stage.Resume;
                    if (widget != null)
                    {
                        trigger = widget.GetComponent<TooltipTrigger>() ?? widget.GetComponentInChildren<TooltipTrigger>(true);
                        if (trigger == null) findings.Add("the buff slot has no tooltip trigger");
                        else
                        {
                            // The handler a pointer entering the slot reaches.
                            trigger.OnPointerEnter(null);
                            stage = Stage.TooltipShown;
                        }
                    }

                    Wait(240);
                    return null;

                case Stage.TooltipShown:
                {
                    TooltipData data = trigger.Data;
                    // Only what the tooltip itself draws, with the markup of its decorated first letter removed; the
                    // combat log also names the weapon.
                    JArray texts = new JArray();
                    foreach (TextMeshProUGUI text in Game.Instance.UI.TooltipsController.GetComponentsInChildren<TextMeshProUGUI>(false))
                    {
                        if (!text.isActiveAndEnabled || string.IsNullOrEmpty(text.text)) continue;
                        texts.Add(new JObject { ["path"] = PathOf(text.transform), ["text"] = Regex.Replace(text.text, "<[^>]+>", string.Empty) });
                    }

                    JArray images = new JArray();
                    if (blueprint.Icon != null)
                        foreach (Image image in UnityEngine.Object.FindObjectsOfType<Image>())
                            if (image.isActiveAndEnabled && ReferenceEquals(image.sprite, blueprint.Icon))
                                images.Add(new JObject { ["path"] = PathOf(image.transform), ["screenRect"] = ScreenRect(image.rectTransform) });
                    Record["tooltip"] = new JObject
                    {
                        ["type"] = data == null ? null : data.Type.ToString(),
                        ["buff"] = data == null || data.Buff == null ? null : data.Buff.Blueprint.name,
                        ["drawnTexts"] = texts, ["imagesShowingIcon"] = images
                    };
                    if (data == null || data.Buff == null || !ReferenceEquals(data.Buff.Blueprint, blueprint)) findings.Add("the slot's tooltip is not for " + blueprint.name);
                    foreach (string expected in expectedTexts)
                        if (!texts.Any(entry => ((string)entry["text"]).Contains(expected))) findings.Add("no drawn tooltip text contains '" + expected + "'");
                    Capture("tooltip");
                    stage = Stage.Resume;
                    Wait(30);
                    return null;
                }

                case Stage.Resume:
                    // The fight is over before time runs, so nothing can hit again and renew the buff.
                    if (trigger != null) trigger.OnPointerExit(null);
                    Restore();
                    unit.Commands.InterruptAll(true);
                    target.CombatState.LeaveCombat();
                    unit.CombatState.LeaveCombat();
                    target.Dispose();
                    Game.Instance.IsPaused = false;
                    resumedAt = Time.realtimeSinceStartup;
                    resumedAtGameTime = Game.Instance.TimeController.GameTime;
                    stage = Stage.WaitEnd;
                    return null;

                case Stage.WaitEnd:
                {
                    // A timed buff must end once time runs, without help, and take its bonus with it. The game's own
                    // automatic pauses are lifted, and the wait is measured in game time: three rounds.
                    if (Game.Instance.IsPaused)
                    {
                        pausesLifted++;
                        Game.Instance.IsPaused = false;
                    }

                    bool present = OnUnit() != null;
                    double elapsed = Time.realtimeSinceStartup - resumedAt;
                    double gameSeconds = (Game.Instance.TimeController.GameTime - resumedAtGameTime).TotalSeconds;
                    if (present && gameSeconds < 18.0 && elapsed < 60.0) return null;
                    int armorClassAfterEnd = unit.Descriptor.Stats.AC.ModifiedValue;
                    Record["afterTimeRuns"] = new JObject
                    {
                        ["ended"] = !present, ["gameSecondsAfterResume"] = Math.Round(gameSeconds, 1), ["realSecondsAfterResume"] = Math.Round(elapsed, 1),
                        ["pausesLifted"] = pausesLifted, ["armorClass"] = armorClassAfterEnd, ["gameMode"] = Game.Instance.CurrentMode.ToString()
                    };
                    if (present && gameSeconds < 18.0) findings.Add("game time did not run: " + Math.Round(gameSeconds, 1) + " s in " + Math.Round(elapsed, 1) + " s of play");
                    else if (present) findings.Add(blueprint.name + " did not end by itself within three rounds of game time");
                    else if (armorClassAfterEnd != armorClassBefore) findings.Add("armor class is " + armorClassAfterEnd + " after the buff ended, " + armorClassBefore + " before it");
                    Game.Instance.IsPaused = true;
                    stage = Stage.Finish;
                    return null;
                }

                default:
                    string missing = MissingScreenshots();
                    if (missing != null) findings.Add(missing);
                    return findings.Count == 0 ? string.Empty : string.Join("; ", findings.ToArray());
            }
        }

        private Buff OnUnit()
        {
            return unit.Descriptor.Buffs.RawFacts.OfType<Buff>().FirstOrDefault(candidate => ReferenceEquals(candidate.Blueprint, blueprint));
        }

        private bool HitSince(int index)
        {
            return observer.Attacks.Skip(index).Any(attack => (bool)attack["attackOfOpportunity"] && (bool)attack["hit"]);
        }

        /// <summary>Has the game queue its own attack of opportunity and runs that command's action. Null when it ran.</summary>
        private string Opportunity()
        {
            unit.Commands.InterruptAll(true);
            if (!target.Memory.Contains(unit)) target.Memory.Add(unit);
            unit.CombatState.AttackOfOpportunityCount = Math.Max(1, unit.CombatState.AttackOfOpportunityPerRound);
            if (!unit.CombatState.AttackOfOpportunity(target))
                return "the game did not queue an attack of opportunity (inCombat=" + unit.CombatState.IsInCombat + ", canAct=" + unit.CombatState.CanActInCombat +
                    ", canAttackOfOpportunity=" + unit.CombatState.CanAttackOfOpportunity + ")";
            UnitAttackOfOpportunity[] commands = unit.Commands.Raw.OfType<UnitAttackOfOpportunity>().Where(candidate => ReferenceEquals(candidate.Target, target)).ToArray();
            if (commands.Length != 1) return "expected one queued attack of opportunity, found " + commands.Length;
            MethodInfo action = typeof(UnitAttackOfOpportunity).GetMethod("OnAction", Any, null, Type.EmptyTypes, null);
            if (action == null) return "UnitAttackOfOpportunity.OnAction is missing";
            action.Invoke(commands[0], null);
            unit.Commands.InterruptAll(true);
            return null;
        }

        private string Abandon(string failure)
        {
            Restore();
            return failure;
        }

        private void Restore()
        {
            EventBus.Unsubscribe(observer);
            if (attackBonusRaised) unit.Descriptor.Stats.BaseAttackBonus.BaseValue = attackBonusBefore;
            attackBonusRaised = false;
        }

        /// <summary>A copy of the default player character under the first faction the game treats as the unit's enemy.</summary>
        private static UnitEntityData SpawnHostileTarget(UnitEntityData attacker, Vector3 position)
        {
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            var seen = new HashSet<BlueprintFaction>();
            foreach (BlueprintUnit candidate in ResourcesLibrary.GetBlueprints<BlueprintUnit>().OrderBy(value => value.AssetGuid, StringComparer.Ordinal))
            {
                BlueprintFaction faction = candidate.Faction;
                if (faction == null || ReferenceEquals(faction, source.Faction) || !seen.Add(faction)) continue;
                BlueprintUnit clone = UnityEngine.Object.Instantiate(source);
                clone.name = "KmgLoaderCompat_OpportunityTarget";
                clone.IsCheater = true;
                clone.Faction = faction;
                UnitEntityData spawned = null;
                try
                {
                    spawned = Game.Instance.EntityCreator.SpawnUnit(clone, position, Quaternion.identity, Game.Instance.State.LoadedAreaState.MainState);
                    if (spawned != null && spawned.IsEnemy(attacker)) return spawned;
                }
                catch (Exception)
                {
                    // The next faction is tried.
                }

                if (spawned != null) spawned.Dispose();
                UnityEngine.Object.DestroyImmediate(clone);
            }

            throw new InvalidOperationException("no loaded faction produced a hostile disposable target");
        }

        private static string PathOf(Transform transform)
        {
            var names = new List<string>();
            for (Transform current = transform; current != null; current = current.parent) names.Insert(0, current.name);
            return string.Join("/", names.ToArray());
        }

        /// <summary>Where a UI rectangle is drawn, in screenshot pixels from the top-left corner.</summary>
        private static JObject ScreenRect(RectTransform rect)
        {
            Canvas canvas = rect.GetComponentInParent<Canvas>();
            Camera camera = canvas == null || canvas.rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.rootCanvas.worldCamera;
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            Vector2 min = RectTransformUtility.WorldToScreenPoint(camera, corners[0]);
            Vector2 max = RectTransformUtility.WorldToScreenPoint(camera, corners[2]);
            return new JObject
            {
                ["x"] = Math.Round(min.x, 1), ["y"] = Math.Round(Screen.height - max.y, 1),
                ["width"] = Math.Round(max.x - min.x, 1), ["height"] = Math.Round(max.y - min.y, 1)
            };
        }
    }

    /// <summary>
    /// Puts timed buffs on the loaded save's main character with the game's own AddBuff, then requires what a timed buff
    /// owes the player: activation that completes, a portrait slot for each visible buff and none for a hidden one, and
    /// an end that comes by itself once time runs, taking its stat changes with it. Nothing is saved.
    /// </summary>
    internal sealed class TimedBuffProbe : ScreenProbe
    {
        private enum Stage { Start, PartyBar, Resume, WaitEnd, Finish }

        private readonly string[] guids;
        private readonly double seconds;
        private readonly string expectedIcon;
        private readonly List<BlueprintBuff> blueprints = new List<BlueprintBuff>();
        private readonly Dictionary<BlueprintBuff, double> endedAt = new Dictionary<BlueprintBuff, double>();
        private readonly List<string> findings = new List<string>();
        private Stage stage = Stage.Start;
        private UnitEntityData unit;
        private JObject statsBefore;
        private double resumedAt;
        private TimeSpan resumedAtGameTime;
        private int pausesLifted;

        public TimedBuffProbe(string directory, string label, IEnumerable<string> guids, double seconds, string expectedIcon) : base(directory, label)
        {
            this.guids = guids.ToArray();
            this.seconds = seconds;
            this.expectedIcon = expectedIcon;
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
                    if (seconds <= 0) return "the duration must be positive";
                    Record["unit"] = unit.CharacterName;
                    Record["durationSeconds"] = seconds;
                    Game.Instance.IsPaused = true;
                    statsBefore = Stats();
                    Record["statsBefore"] = statsBefore;
                    JArray applied = new JArray();
                    foreach (string guid in guids)
                    {
                        BlueprintBuff blueprint = ResourcesLibrary.TryGetBlueprint(guid) as BlueprintBuff;
                        if (blueprint == null) return "buff " + guid + " does not resolve";
                        if (OnUnit(blueprint) != null) return "the main character already has " + blueprint.name;
                        // AddBuff returns null when Kingmaker's rule swallowed a failure while activating the buff.
                        Buff buff = unit.Descriptor.Buffs.AddBuff(blueprint, unit, TimeSpan.FromSeconds(seconds));
                        Buff present = OnUnit(blueprint);
                        applied.Add(new JObject
                        {
                            ["guid"] = guid, ["blueprint"] = blueprint.name, ["name"] = blueprint.Name, ["hiddenInUi"] = blueprint.IsHiddenInUI,
                            ["fxOnStartAssigned"] = blueprint.FxOnStart != null, ["fxOnRemoveAssigned"] = blueprint.FxOnRemove != null,
                            ["addBuffReturnedBuff"] = buff != null, ["onUnit"] = present != null, ["active"] = present != null && present.Active
                        });
                        if (present == null) return "buff " + blueprint.name + " is not on the unit after AddBuff";
                        if (buff == null) findings.Add("Kingmaker did not finish activating " + blueprint.name + " (AddBuff returned nothing)");
                        blueprints.Add(blueprint);
                    }

                    Record["applied"] = applied;
                    Record["statsWithBuffs"] = Stats();
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
                    JArray slots = new JArray();
                    foreach (BlueprintBuff blueprint in blueprints)
                    {
                        BuffController widget = widgets.FirstOrDefault(candidate => candidate != null && candidate.Buff != null && ReferenceEquals(candidate.Buff.Blueprint, blueprint));
                        bool shown = widget != null;
                        Sprite sprite = !shown || widget.Icon == null ? null : widget.Icon.sprite;
                        slots.Add(new JObject
                        {
                            ["blueprint"] = blueprint.name, ["hiddenInUi"] = blueprint.IsHiddenInUI, ["inPartyBar"] = shown,
                            ["shownIcon"] = sprite == null ? null : sprite.name, ["blueprintIcon"] = blueprint.Icon == null ? null : blueprint.Icon.name
                        });
                        if (shown && blueprint.IsHiddenInUI) findings.Add("hidden buff " + blueprint.name + " is in the party bar");
                        if (blueprint.IsHiddenInUI) continue;
                        if (!shown) findings.Add(blueprint.name + " is not in the party bar");
                        else if (sprite == null) findings.Add(blueprint.name + " has no icon, so its slot draws a blank square");
                        else if (!ReferenceEquals(sprite, blueprint.Icon)) findings.Add(blueprint.name + " party-bar icon is " + sprite.name + ", not its blueprint's");
                        else if (!string.IsNullOrEmpty(expectedIcon) && sprite.name != expectedIcon) findings.Add(blueprint.name + " icon is " + sprite.name + ", expected " + expectedIcon);
                    }

                    Record["partyBar"] = slots;
                    Record["partyBarWidgets"] = widgets.Count;
                    if (widgets.Count >= 6) bar.OnTriggerPointerEnter();
                    stage = Stage.Resume;
                    Wait(15);
                    return null;
                }

                case Stage.Resume:
                    Capture("party-bar");
                    Game.Instance.IsPaused = false;
                    resumedAt = Time.realtimeSinceStartup;
                    resumedAtGameTime = Game.Instance.TimeController.GameTime;
                    stage = Stage.WaitEnd;
                    return null;

                case Stage.WaitEnd:
                {
                    // The game's own automatic pauses are lifted, and the wait is measured in game time: three durations.
                    if (Game.Instance.IsPaused)
                    {
                        pausesLifted++;
                        Game.Instance.IsPaused = false;
                    }

                    double elapsed = Time.realtimeSinceStartup - resumedAt;
                    double gameSeconds = (Game.Instance.TimeController.GameTime - resumedAtGameTime).TotalSeconds;
                    foreach (BlueprintBuff blueprint in blueprints)
                        if (!endedAt.ContainsKey(blueprint) && OnUnit(blueprint) == null) endedAt[blueprint] = Math.Round(gameSeconds, 1);
                    if (endedAt.Count < blueprints.Count && gameSeconds < seconds * 3 && elapsed < 60.0) return null;
                    JArray ends = new JArray();
                    foreach (BlueprintBuff blueprint in blueprints)
                    {
                        bool ended = endedAt.ContainsKey(blueprint);
                        ends.Add(new JObject { ["blueprint"] = blueprint.name, ["ended"] = ended, ["gameSecondsAfterResume"] = ended ? (JToken)endedAt[blueprint] : null });
                        if (!ended && gameSeconds >= seconds * 3) findings.Add(blueprint.name + " did not end by itself within " + seconds * 3 + " s of game time");
                    }

                    if (endedAt.Count < blueprints.Count && gameSeconds < seconds * 3)
                        findings.Add("game time did not run: " + Math.Round(gameSeconds, 1) + " s in " + Math.Round(elapsed, 1) + " s of play");
                    JObject statsAfter = Stats();
                    Record["afterTimeRuns"] = new JObject
                    {
                        ["ends"] = ends, ["gameSeconds"] = Math.Round(gameSeconds, 1), ["realSeconds"] = Math.Round(elapsed, 1), ["pausesLifted"] = pausesLifted,
                        ["stats"] = statsAfter, ["gameMode"] = Game.Instance.CurrentMode.ToString()
                    };
                    if (endedAt.Count == blueprints.Count && !JToken.DeepEquals(statsAfter, statsBefore))
                        findings.Add("stats after the buffs ended are " + statsAfter.ToString(Newtonsoft.Json.Formatting.None) + ", before them " + statsBefore.ToString(Newtonsoft.Json.Formatting.None));
                    Game.Instance.IsPaused = true;
                    stage = Stage.Finish;
                    return null;
                }

                default:
                    string missing = MissingScreenshots();
                    if (missing != null) findings.Add(missing);
                    return findings.Count == 0 ? string.Empty : string.Join("; ", findings.ToArray());
            }
        }

        private Buff OnUnit(BlueprintBuff blueprint)
        {
            return unit.Descriptor.Buffs.RawFacts.OfType<Buff>().FirstOrDefault(candidate => ReferenceEquals(candidate.Blueprint, blueprint));
        }

        private JObject Stats()
        {
            return new JObject
            {
                ["armorClass"] = unit.Descriptor.Stats.AC.ModifiedValue, ["speed"] = unit.Descriptor.Stats.Speed.ModifiedValue,
                ["reflex"] = unit.Descriptor.Stats.SaveReflex.ModifiedValue
            };
        }
    }
}
