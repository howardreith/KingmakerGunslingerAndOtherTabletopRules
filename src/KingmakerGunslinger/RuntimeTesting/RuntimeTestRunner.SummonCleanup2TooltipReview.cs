using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.ActionBar;
using Kingmaker.UI.Group;
using Kingmaker.UI.Tooltip;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private sealed class Cleanup2TooltipTarget
        {
            internal SummonFamily Family;
            internal int Tier;
            internal string Key;
            internal BlueprintAbility Parent, Child;
            internal string Label { get { return Family + "-" + Key; } }
            internal string ExpectedTitle { get { return SummonChoicePresentationPolicy.For(Key).Label; } }
        }

        private sealed class Cleanup2TooltipInputs
        {
            private readonly UnitEntityData _unit;
            private readonly BlueprintScriptableObject[] _resources;
            private readonly int[] _amounts;
            private readonly Cleanup2SlotSnapshot[] _slots;
            internal Cleanup2TooltipInputs(UnitEntityData unit)
            {
                _unit = unit;
                var resources = new List<BlueprintScriptableObject>();
                foreach (BlueprintScriptableObject resource in unit.Descriptor.Resources) resources.Add(resource);
                _resources = resources.ToArray();
                _amounts = _resources.Select(unit.Descriptor.Resources.GetResourceAmount).ToArray();
                _slots = unit.Descriptor.Spellbooks.SelectMany(book => Enumerable.Range(0, 10)
                    .Select(level => new Cleanup2SlotSnapshot(book, level))).ToArray();
                if (!unit.Commands.Empty) throw new InvalidOperationException("Tooltip fixture caster must have no native commands.");
            }
            internal bool Unchanged()
            {
                var current = new List<BlueprintScriptableObject>();
                foreach (BlueprintScriptableObject resource in _unit.Descriptor.Resources) current.Add(resource);
                return _unit.Commands.Empty && current.Count == _resources.Length &&
                    current.Select((value, i) => ReferenceEquals(value, _resources[i]) &&
                        _unit.Descriptor.Resources.GetResourceAmount(value) == _amounts[i]).All(value => value) &&
                    _slots.All(value => value.Unchanged());
            }
            internal JObject Describe()
            { return new JObject { ["unit"] = _unit.UniqueId, ["commandsEmpty"] = _unit.Commands.Empty,
                ["slotObjectCount"] = _slots.Sum(value => value.Slots.Length), ["unchanged"] = Unchanged(),
                ["resources"] = new JArray(_resources.Select((value, i) => new JObject {
                    ["blueprint"] = value.AssetGuid, ["before"] = _amounts[i],
                    ["after"] = _unit.Descriptor.Resources.GetResourceAmount(value) })) }; }
        }

        // Root integrator supplies exact phase admission/dispatch and Oleg
        // fixture selection. This entry never opens nine-tier campaign menus.
        private IEnumerable<int> ReviewSummonCleanup2Tooltips(ExpandedSummoningCorrectionFixture fixture, Spellbook book)
        {
            if (!IsSummonCleanup2Review || !_request.ExitAfterCompletion || _request.Parameters == null ||
                _request.Parameters.Count != 1 || (string)_request.Parameters["reviewPhase"] != "tooltips")
                throw new InvalidOperationException("Exact automatic-exit Cleanup 2 tooltips phase required.");
            var game = Game.Instance; var player = game.Player; var ui = game.UI;
            var bar = ActionBarManager.Instance; var group = GroupController.Instance;
            var tips = ui?.TooltipsController;
            if (!Cleanup2NoNarrativeOverlay() || bar?.Group == null || group == null || ui?.SelectionManagerPC == null ||
                tips?.TooltipConstructor == null || ui.DescriptionController?.DescWindow == null ||
                tips.ContextTooltipData != null || ui.DescriptionController.DescWindow.gameObject.activeInHierarchy ||
                game.IsControllerGamepad || game.SelectedAbilityHandler?.Ability != null ||
                (game.ClickEventsController != null && game.ClickEventsController.Mode != Kingmaker.Controllers.Clicks.PointerMode.Default))
                throw new InvalidOperationException("Idle native desktop menus with no foreign tooltip/description/narrative/input owner required.");
            var shown = typeof(TooltipsController).GetField("m_IsShowed", Cleanup2Fields);
            var needed = typeof(TooltipBase).GetField("m_NeedTooltip", Cleanup2Fields);
            if (shown == null || needed == null || (bool)shown.GetValue(tips) || (bool)needed.GetValue(tips.TooltipConstructor))
                throw new InvalidOperationException("Native tooltip closed-state fields are missing or already shown.");

            UnitEntityData[] party = player.Party.ToArray();
            UnitEntityData[] selection = ui.SelectionManagerPC.SelectedUnits.ToArray();
            UnitEntityData selectedGroup = group.GetCurrentCharacter();
            var selectedHandler = game.SelectedAbilityHandler; var pointer = game.ClickEventsController;
            ActionBarGroupElement[] groups = bar.Group.GroupElements.ToArray();
            bool[] toggles = groups.Select(value => value.ToggleState).ToArray();
            if (toggles.Any(value => value)) throw new InvalidOperationException("Tooltip phase requires closed native groups.");
            var originals = party.Select(value => new Cleanup2TooltipInputs(value)).ToArray();
            var hotbars = party.Select(value => new TeleportationUiSettingsFixture(value.UISettings)).ToArray();
            bool added = false;
            var targets = new[] {
                new Cleanup2TooltipTarget { Family = SummonFamily.Monster, Tier = 2, Key = "horse" },
                new Cleanup2TooltipTarget { Family = SummonFamily.NaturesAlly, Tier = 2, Key = "horse" },
                new Cleanup2TooltipTarget { Family = SummonFamily.NaturesAlly, Tier = 2, Key = "small-fire-elemental" },
                new Cleanup2TooltipTarget { Family = SummonFamily.NaturesAlly, Tier = 4, Key = "air-mephit" },
                new Cleanup2TooltipTarget { Family = SummonFamily.NaturesAlly, Tier = 9, Key = "storm-giant" }
            };
            foreach (var target in targets)
            {
                int offset = target.Family == SummonFamily.Monster ? 0 : 9;
                target.Parent = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                    ExpandedSummoningInventoryObserver.CanonicalParentGuids[offset + target.Tier - 1], "exact canonical tooltip parent");
                SummonVariantSpec spec = ExpandedSummoningCatalog.GenerateVariants(target.Family).Single(value =>
                    value.Creature.Key == target.Key && value.ParentTier == target.Tier && value.Multiplicity == SummonMultiplicity.One);
                string symbol = ExpandedSummoningIdentityCatalog.AbilitySymbol(spec);
                target.Child = fixture.Blueprints.OfType<BlueprintAbility>().Single(value => value.name == ExpandedSummoningInternalName(symbol));
                if (target.Parent.Variants == null || target.Parent.Variants.Count(value => ReferenceEquals(value, target.Child)) != 1)
                    throw new InvalidOperationException("Exact stable tooltip child is not published once: " + spec.StableKey);
                if (target.Child.Icon == null) throw new InvalidOperationException("Exact tooltip child has no live icon: " + spec.StableKey);
                if (target.Child.Name != target.ExpectedTitle) throw new InvalidOperationException("Current child label differs from explicit presentation metadata: " + spec.StableKey);
            }

            using (var screens = new NativeIconScreenEvidence(_request))
            try
            {
                if (!player.Party.Contains(fixture.Caster))
                {
                    player.PartyCharacters.Add(fixture.Caster); added = true;
                    player.InvalidateCharacterLists(); player.UpdateCharacterLists();
                }
                foreach (var parent in targets.GroupBy(value => value.Parent))
                    PrepareExpandedSummoningPlayerPathSpell(book, parent.Key, parent.Key, parent.First().Tier);
                ui.SelectionManagerPC.SelectUnit(fixture.Caster.View, true, true, false);
                group.SelectUnit(fixture.Caster);
                for (int frame = 0; frame < 20; frame++) yield return 0;
                var input = new Cleanup2TooltipInputs(fixture.Caster);
                var fixtureHotbar = new TeleportationUiSettingsFixture(fixture.Caster.UISettings);
                var spellGroup = groups.Single(value => (ActionBarSlotType)typeof(ActionBarGroupElement)
                    .GetField("SlotType", Cleanup2Fields).GetValue(value) == ActionBarSlotType.Spell);
                foreach (var parent in targets.GroupBy(value => value.Parent))
                {
                    if (!Cleanup2NoNarrativeOverlay() || !input.Unchanged()) throw new InvalidOperationException("Tooltip setup ownership changed.");
                    if (!spellGroup.ToggleState) spellGroup.OnClick();
                    for (int frame = 0; frame < 20; frame++) yield return 0;
                    var levels = (ActionBarSubGroupLevels)typeof(ActionBarGroupElement).GetField("m_Levels", Cleanup2Fields).GetValue(spellGroup);
                    if (levels == null) throw new InvalidOperationException("Native spell-level tabs missing.");
                    levels.OnClick(parent.First().Tier);
                    for (int frame = 0; frame < 20; frame++) yield return 0;
                    var sources = spellGroup.GetComponentsInChildren<ActionBarGroupSlot>(true).Where(value => {
                        var data = value.MechanicSlot?.GetContentData() as AbilityData;
                        return value.gameObject.activeInHierarchy && data?.Blueprint == parent.Key && ReferenceEquals(data.Spellbook, book);
                    }).ToArray();
                    if (sources.Length != 1) throw new InvalidOperationException("Exact native parent row unavailable: " + parent.Key.AssetGuid);
                    ActionBarGroupSlot source = sources[0];
                    try
                    {
                        source.OnToggleGroupClick();
                        for (int frame = 0; frame < 20; frame++) yield return 0;
                        var popup = (ActionBarSpellsGroup)typeof(ActionBarGroupSlot).GetField("SubGroup", Cleanup2Fields).GetValue(source);
                        if (popup == null || !popup.gameObject.activeInHierarchy) throw new InvalidOperationException("Exact native variant popup missing.");
                        foreach (var target in parent)
                            foreach (int frame in Cleanup2CaptureTooltipTarget(fixture, book, source, popup, target, input, screens)) yield return frame;
                    }
                    finally
                    {
                        // Hiding the source also disables its TooltipTriggers.
                        // Their native OnDisable clears shared tooltip data, so
                        // do not use popup teardown to erase a foreign hover.
                        if (tips.ContextTooltipData != null)
                            throw new InvalidOperationException("Tooltip context is not closed; native popup teardown is refused to preserve a replacement hover.");
                        source.HideSubGroup();
                    }
                }
                bool noCast = input.Unchanged() && fixtureHotbar.IsRestored() && originals.All(value => value.Unchanged()) && hotbars.All(value => value.IsRestored());
                Cleanup2Check("tooltip-no-cast-or-pinning", noCast, new JObject { ["caster"] = input.Describe(),
                    ["originalParty"] = new JArray(originals.Select(value => value.Describe())),
                    ["fixtureAndOriginalHotbarReferencesExact"] = fixtureHotbar.IsRestored() && hotbars.All(value => value.IsRestored()),
                    ["sampleCount"] = targets.Length, ["coverage"] = "SM II Horse; SNA II Horse/Elemental Fire Small; SNA IV Mephit Air; SNA IX Giant Storm. No cast, click, auto-use toggle, synthetic tooltip data or renderer substitution." },
                    "All five actual native hover captures preserve slot/spell objects, availability, resources, commands and hotbar settings.");
                if (!noCast) throw new InvalidOperationException("Tooltip observation changed native invocation state.");
            }
            finally
            {
                var errors = new List<string>();
                Action<string, Action> restore = (name, action) => { try { action(); } catch (Exception error) { errors.Add(name + ":" + error.Message); } };
                bool mayRestoreUi = tips.ContextTooltipData == null;
                if (!mayRestoreUi)
                    errors.Add("Native tooltip context has another owner or cleanup is unverified; selection/group/party UI changes are refused. Shared exact-owned fixture teardown remains mandatory.");
                else
                {
                    restore("party", () => { if (added) player.PartyCharacters.Remove(fixture.Caster); player.InvalidateCharacterLists(); player.UpdateCharacterLists(); });
                    restore("group", () => {
                        if (!ReferenceEquals(GroupController.Instance, group)) throw new InvalidOperationException("Foreign group controller; not overwritten.");
                        group.SelectUnit(selectedGroup);
                    });
                    restore("selection", () => ui.SelectionManagerPC.MultiSelect(selection.Where(value => value?.View != null).Select(value => value.View).ToArray(), false));
                    for (int i = 0; i < groups.Length; i++) { int index = i; restore("toggle-" + i, () => groups[index].Toggle(toggles[index], true)); }
                }
                bool exact = errors.Count == 0 && player.Party.SequenceEqual(party) &&
                    ui.SelectionManagerPC.SelectedUnits.SequenceEqual(selection) && ReferenceEquals(group.GetCurrentCharacter(), selectedGroup) &&
                    ReferenceEquals(game.SelectedAbilityHandler, selectedHandler) && ReferenceEquals(game.ClickEventsController, pointer) &&
                    game.SelectedAbilityHandler?.Ability == null && (pointer == null || pointer.Mode == Kingmaker.Controllers.Clicks.PointerMode.Default) &&
                    tips.ContextTooltipData == null && !(bool)shown.GetValue(tips) && !(bool)needed.GetValue(tips.TooltipConstructor) &&
                    !ui.DescriptionController.DescWindow.gameObject.activeInHierarchy && originals.All(value => value.Unchanged()) && hotbars.All(value => value.IsRestored());
                Cleanup2Check("tooltip-owned-state-restored", exact, new JObject { ["partyExact"] = player.Party.SequenceEqual(party),
                    ["selectionExact"] = ui.SelectionManagerPC.SelectedUnits.SequenceEqual(selection), ["errors"] = new JArray(errors),
                    ["hoverClosed"] = tips.ContextTooltipData == null && !(bool)shown.GetValue(tips),
                    ["descriptionClosed"] = !ui.DescriptionController.DescWindow.gameObject.activeInHierarchy },
                    "The owned native hover, popup, scrolling, selection, group and input state restore; foreign tooltip data is never hidden by this observer.");
                if (!exact) throw new InvalidOperationException("Tooltip phase restoration unverified: " + string.Join("|", errors.ToArray()));
            }
        }

        private IEnumerable<int> Cleanup2CaptureTooltipTarget(ExpandedSummoningCorrectionFixture fixture, Spellbook book,
            ActionBarGroupSlot source, ActionBarSpellsGroup popup, Cleanup2TooltipTarget target,
            Cleanup2TooltipInputs input, NativeIconScreenEvidence screens)
        {
            var allRows = popup.GetComponentsInChildren<ActionBarSlot>(true).Where(value => {
                var rowAbility = value.MechanicSlot?.GetContentData() as AbilityData;
                return value.gameObject.activeInHierarchy && rowAbility?.ConvertedFrom?.Blueprint == target.Parent;
            }).ToArray();
            var rows = allRows.Where(value => {
                var rowAbility = value.MechanicSlot.GetContentData() as AbilityData;
                return rowAbility?.Blueprint == target.Child && ReferenceEquals(rowAbility.Spellbook, book);
            }).ToArray();
            if (rows.Length != 1) throw new InvalidOperationException("Exact target row unavailable: " + target.Label);
            ActionBarSlot row = rows[0]; var ability = (AbilityData)row.MechanicSlot.GetContentData();
            var scroll = Cleanup2AcquirePopupSurface(source, popup, target.Parent, allRows, "tooltip-" + target.Label);
            Vector2 position = scroll.IsStationary ? Vector2.zero : scroll.Position;
            Vector2 velocity = scroll.IsStationary ? Vector2.zero : scroll.Velocity;
            Vector2 anchored = scroll.Content.anchoredPosition;
            var content = scroll.Content; var viewport = Cleanup2PopupViewport(scroll);
            var trigger = row.GetComponentsInChildren<TooltipTrigger>(true).SingleOrDefault(value =>
                value.gameObject.activeInHierarchy && ReferenceEquals(value.GetComponentInParent<ActionBarSlot>(), row));
            if (trigger == null || trigger.Disable || !trigger.isActiveAndEnabled) throw new InvalidOperationException("Exact native row tooltip trigger unavailable.");
            TooltipData data = null;
            Func<bool> owns = () => Cleanup2NoNarrativeOverlay() && popup != null && popup.gameObject.activeInHierarchy && row != null &&
                row.gameObject.activeInHierarchy && ReferenceEquals(row.MechanicSlot.GetContentData(), ability) &&
                ability.Blueprint == target.Child && ability.ConvertedFrom?.Blueprint == target.Parent && ReferenceEquals(ability.Spellbook, book) &&
                scroll.Active && scroll.Content == content && Cleanup2PopupViewport(scroll) == viewport &&
                ReferenceEquals(GroupController.Instance.GetCurrentCharacter(), fixture.Caster) && input.Unchanged() && Game.Instance.SelectedAbilityHandler?.Ability == null;
            try
            {
                foreach (int frame in Cleanup2RevealPopupRow(scroll, row, owns)) yield return frame;
                if (!Cleanup2FullyVisible(scroll, row))
                {
                    ExpandedSummoningVariantMenuSnapshot snapshot;
                    ExpandedSummoningVariantMenuRuntime.TryGetSnapshot(popup, out snapshot);
                    Cleanup2Check("tooltip-row-visible-" + target.Label, false,
                        Cleanup2PopupSurfaceDiagnostic(source, popup, target.Parent, new[] { row }, snapshot, "target-not-fully-visible"),
                        "The exact target row fits its proven popup/viewport, all applicable native masks and the physical screen before native hover.");
                    throw new InvalidOperationException("Target row not fully visible.");
                }
                var menu = new JObject { ["key"] = target.Key, ["family"] = target.Family.ToString(), ["tier"] = target.Tier,
                    ["parent"] = target.Parent.AssetGuid, ["child"] = target.Child.AssetGuid, ["name"] = target.Child.Name,
                    ["surfaceMode"] = scroll.IsStationary ? "stationary-native-popup" : "owned-overflow-scroll",
                    ["surfaceApi"] = scroll.Api, ["scrollMutationRequested"] = !scroll.IsStationary,
                    ["rowIconMatches"] = ReferenceEquals(row.Icon.sprite, target.Child.Icon), ["rowIcon"] = row.Icon.sprite?.name };
                foreach (int frame in screens.Capture("tooltip-menu-" + target.Label, menu, owns)) yield return frame;
                if (Game.Instance.UI.TooltipsController.ContextTooltipData != null) throw new InvalidOperationException("Foreign native hover appeared before taking the trigger.");
                trigger.ShowTooltipManual(true);
                data = trigger.Data;
                if (data == null || !ReferenceEquals(data.AbilityData, ability)) throw new InvalidOperationException("Native trigger did not collect the exact row AbilityData.");
                var watch = Stopwatch.StartNew();
                for (int frame = 0; frame < 600 && watch.Elapsed.TotalSeconds < 8 && !NativeIconScreenEvidence.OwnsHover(trigger, data); frame++)
                {
                    if (!owns() || !ReferenceEquals(Game.Instance.UI.TooltipsController.ContextTooltipData, data))
                        throw new InvalidOperationException("Foreign hover or source row changed while waiting for native tooltip delay.");
                    yield return 0;
                }
                if (!NativeIconScreenEvidence.OwnsHover(trigger, data)) throw new TimeoutException("Native owned hover did not render within the bound.");
                JObject card = null;
                var layoutWatch = Stopwatch.StartNew();
                for (int frame = 0; frame < 600 && layoutWatch.Elapsed.TotalSeconds < 8; frame++)
                {
                    if (!owns() || !NativeIconScreenEvidence.OwnsHover(trigger, data)) throw new InvalidOperationException("Tooltip ownership changed during native layout/fade.");
                    card = Cleanup2TooltipCard(target, row, trigger, data);
                    if ((bool)card["passed"]) break;
                    yield return 0;
                }
                if (card == null) throw new InvalidOperationException("Native tooltip produced no readable header observation.");
                bool presentation = (bool)card["passed"];
                Cleanup2Check("tooltip-rendered-" + target.Label, presentation, card,
                    "The actual source-owned native tooltip header renders the exact family-first title and child icon, fully visible without overflow/truncation.");
                if (!presentation) throw new InvalidOperationException("Native tooltip title/icon visibility failed: " + target.Label);
                foreach (int frame in screens.CaptureOwnedHover("tooltip-hover-" + target.Label, card,
                    () => owns() && Cleanup2FullyVisible(scroll, row) && (bool)Cleanup2TooltipCard(target, row, trigger, data)["passed"], trigger, data)) yield return frame;
            }
            finally
            {
                var tips = Game.Instance.UI.TooltipsController;
                if (data != null && trigger != null)
                {
                    if (tips.ContextTooltipData != null && !ReferenceEquals(tips.ContextTooltipData, data))
                        throw new InvalidOperationException("Foreign tooltip replaced the owned hover; observer does not hide it or restore its scroll.");
                    trigger.ShowTooltipManual(false);
                }
                if (scroll.Component == null || !scroll.Active || scroll.Content != content || Cleanup2PopupViewport(scroll) != viewport)
                    throw new InvalidOperationException("Foreign native scrolling surface; observer does not overwrite it.");
                if (!scroll.IsStationary) { scroll.RestorePosition(position); scroll.Velocity = velocity; }
                if ((content.anchoredPosition - anchored).sqrMagnitude > .0001f || !scroll.Active)
                    throw new InvalidOperationException("Tooltip target scroll did not restore its native content position.");
            }
            // Let native hover removal/fade finish without writing cached data.
            for (int frame = 0; frame < 30; frame++) yield return 0;
            if (Game.Instance.UI.TooltipsController.ContextTooltipData != null)
                throw new InvalidOperationException("A native hover remains after exact owned-trigger cleanup.");
        }

        private static JObject Cleanup2TooltipCard(Cleanup2TooltipTarget target, ActionBarSlot row, TooltipTrigger trigger, TooltipData data)
        {
            var constructor = Game.Instance.UI.TooltipsController.TooltipConstructor;
            var headers = constructor.Bodies.Where(value => value != null && value.gameObject.activeInHierarchy && value.HeaderBox != null)
                .Select(value => value.HeaderBox).ToArray();
            var texts = headers.SelectMany(value => value.GetComponentsInChildren<TextMeshProUGUI>(true)).Where(value => value.isActiveAndEnabled).ToArray();
            var images = headers.SelectMany(value => value.GetComponentsInChildren<Image>(true)).Where(value => value.isActiveAndEnabled).ToArray();
            bool name = texts.Any(value => value.GetParsedText() == target.ExpectedTitle && !value.isTextTruncated && !value.isTextOverflowing && Cleanup2TooltipGraphicVisible(value));
            bool icon = images.Any(value => ReferenceEquals(value.sprite, target.Child.Icon) && Cleanup2TooltipGraphicVisible(value));
            return new JObject { ["key"] = target.Key, ["family"] = target.Family.ToString(), ["tier"] = target.Tier,
                ["parent"] = target.Parent.AssetGuid, ["child"] = target.Child.AssetGuid, ["expectedTitle"] = target.ExpectedTitle,
                ["passed"] = NativeIconScreenEvidence.OwnsHover(trigger, data) && name && icon && ReferenceEquals(row.Icon.sprite, target.Child.Icon),
                ["nativeDataIsExactRowAbility"] = ReferenceEquals(data.AbilityData, row.MechanicSlot.GetContentData()),
                ["nativeHoverOwned"] = NativeIconScreenEvidence.OwnsHover(trigger, data), ["titleVisible"] = name, ["headerIconMatchesAndVisible"] = icon,
                ["rowIconMatches"] = ReferenceEquals(row.Icon.sprite, target.Child.Icon),
                ["headerTexts"] = new JArray(texts.Select(value => new JObject { ["object"] = value.name, ["parsed"] = value.GetParsedText(),
                    ["truncated"] = value.isTextTruncated, ["overflow"] = value.isTextOverflowing, ["fullyVisible"] = Cleanup2TooltipGraphicVisible(value) })),
                ["headerImages"] = new JArray(images.Select(value => new JObject { ["object"] = value.name, ["sprite"] = value.sprite?.name,
                    ["matchesChild"] = ReferenceEquals(value.sprite, target.Child.Icon), ["fullyVisible"] = Cleanup2TooltipGraphicVisible(value) })),
                ["source"] = "Existing native row TooltipTrigger.ShowTooltipManual uses the same ShowTooltip path as pointer enter/exit; native builder reads child Blueprint.Name/Icon. No tooltip object/text/icon is fabricated.",
                ["visualApproval"] = "Actual framebuffer inspection and owner art approval remain separate." };
        }

        private static bool Cleanup2TooltipGraphicVisible(Graphic graphic)
        {
            if (graphic == null || !graphic.isActiveAndEnabled || graphic.color.a < .99f || graphic.canvasRenderer.cull ||
                graphic.canvasRenderer.GetAlpha() < .99f || graphic.rectTransform.rect.width <= 1f || graphic.rectTransform.rect.height <= 1f ||
                graphic.GetComponentsInParent<CanvasGroup>(true).Any(value => value.alpha < .99f)) return false;
            var clips = graphic.GetComponentsInParent<RectMask2D>(true).Where(value => value.isActiveAndEnabled).Select(value => value.transform as RectTransform)
                .Concat(graphic.GetComponentsInParent<Mask>(true).Where(value => value.isActiveAndEnabled).Select(value => value.transform as RectTransform));
            foreach (RectTransform clip in clips.Where(value => value != null).Distinct())
            {
                Bounds bounds = Cleanup2PopupBounds(clip, graphic.rectTransform); Rect area = clip.rect;
                if (bounds.min.x < area.xMin - .5f || bounds.max.x > area.xMax + .5f ||
                    bounds.min.y < area.yMin - .5f || bounds.max.y > area.yMax + .5f) return false;
            }
            Canvas canvas = graphic.canvas;
            if (canvas == null) return false;
            Camera camera = canvas.rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.rootCanvas.worldCamera;
            if (canvas.rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay && camera == null) return false;
            var corners = new Vector3[4]; graphic.rectTransform.GetWorldCorners(corners);
            return corners.All(value => { Vector2 screen = RectTransformUtility.WorldToScreenPoint(camera, value);
                return (camera == null || camera.WorldToScreenPoint(value).z > 0f) &&
                    screen.x >= 0f && screen.y >= 0f && screen.x <= Screen.width && screen.y <= Screen.height; });
        }
    }
}
