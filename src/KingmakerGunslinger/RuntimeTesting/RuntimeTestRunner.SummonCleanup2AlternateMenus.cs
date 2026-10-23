using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Assets.Console.GamepadInput;
using Kingmaker.UI._ConsoleUI.ActionBar;
using Kingmaker.UI._ConsoleUI.Context.InGame;
using Kingmaker.UI._ConsoleUI.Models;
using Kingmaker.UI._ConsoleUI.Utils.MultiNavigationTool;
using Kingmaker.UI._ConsoleUI.Utils.NavigationTool;
using Kingmaker.UI.ActionBar;
using Kingmaker.UI.Group;
using Kingmaker.UI.Tooltip;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
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
        private sealed class Cleanup2AlternateMenuRoute
        {
            internal AlternateSummoningFamilySpec Family;
            internal AlternateSummoningRootSpec RootSpec;
            internal SummonVariantSpec Placement;
            internal BlueprintAbility Root, Child;
            internal Sprite CanonicalIcon;
            internal Ability Granted;
            internal Spellbook Book;
            internal BlueprintComponent[] Components;
            internal BlueprintAbility[] Variants;
            internal string[] RemovedGeneric, Foreign;
            internal string Label { get { return Family.Key + "-horse-" + Placement.Multiplicity; } }
            internal bool GraphExact { get { return Root.ComponentsArray.SequenceEqual(Components) &&
                (Root.Variants ?? new BlueprintAbility[0]).SequenceEqual(Variants); } }
        }

        // This is an additional bounded slice of menu-adapters, not a replay
        // of the nine-tier roster or the assisted mechanical player-path run.
        private IEnumerable<int> ReviewSummonCleanup2AlternateMenus(ExpandedSummoningCorrectionFixture fixture)
        {
            Cleanup2AlternateAdmission("menu-adapters", false);
            var game = Game.Instance; var ui = game.UI; var player = game.Player;
            var group = GroupController.Instance; var bar = ActionBarManager.Instance;
            if (game.IsControllerGamepad || ui?.SelectionManagerPC == null || group == null || bar?.Group == null ||
                ui.TooltipsController?.TooltipConstructor == null || ui.TooltipsController.ContextTooltipData != null ||
                game.SelectedAbilityHandler?.Ability != null || !Cleanup2NoNarrativeOverlay())
                throw new InvalidOperationException("Idle actual desktop alternate-source UI required.");
            var party = player.Party.ToArray(); var selected = ui.SelectionManagerPC.SelectedUnits.ToArray();
            var current = group.GetCurrentCharacter(); var groups = bar.Group.GroupElements.ToArray();
            bool[] toggles = groups.Select(value => value.ToggleState).ToArray();
            if (toggles.Any(value => value)) throw new InvalidOperationException("Alternate-source groups must be closed at intake.");
            var originals = party.Select(value => new Cleanup2TooltipInputs(value)).ToArray();
            var pins = party.Select(value => new TeleportationUiSettingsFixture(value.UISettings)).ToArray();
            var casters = new List<UnitEntityData>(); var ownedBlueprints = new List<BlueprintUnit>();
            var watch = Stopwatch.StartNew(); var results = new JArray();
            Cleanup2NativeDefaultScope mode = null; NativeIconScreenEvidence screens = null;
            UnitEntityData selectedOwned = null; object levelController = null;
            try
            {
                mode = Cleanup2EnterNativeDefault(fixture, "alternate-menus");
                // Browsing spends no source resource; a native Pause request
                // here would replace this strict Default mode on the next frame.
                screens = new NativeIconScreenEvidence(_request);
                var manifest = BlueprintManifest.Load(_context.ModEntry.Path);
                foreach (var family in AlternateSummoningSourceCatalog.All)
                {
                    Cleanup2AlternateBoundary(mode, watch, 180);
                    var caster = Cleanup2AlternateCaster(fixture, family, ref levelController, casters, ownedBlueprints);
                    var routes = Cleanup2AlternateRoutes(caster, family, manifest, false);
                    player.PartyCharacters.Add(caster); player.InvalidateCharacterLists(); player.UpdateCharacterLists();
                    ui.SelectionManagerPC.SelectUnit(caster.View, true, true, false); group.SelectUnit(caster); selectedOwned = caster;
                    for (int frame = 0; frame < 8; frame++) yield return 0;
                    var input = new Cleanup2TooltipInputs(caster); var ownPins = new TeleportationUiSettingsFixture(caster.UISettings);
                    var known = caster.Descriptor.Spellbooks.SelectMany(value => Enumerable.Range(0, 10).SelectMany(value.GetKnownSpells)).ToArray();
                    foreach (var route in routes)
                    {
                        Cleanup2AlternateBoundary(mode, watch, 180);
                        var type = route.Book == null ? ActionBarSlotType.ActivatableAbility : ActionBarSlotType.Spell;
                        var nativeGroup = groups.Single(value => (ActionBarSlotType)typeof(ActionBarGroupElement)
                            .GetField("SlotType", Cleanup2Fields).GetValue(value) == type);
                        if (!nativeGroup.ToggleState) nativeGroup.OnClick();
                        if (route.Book != null)
                        {
                            var levels = (ActionBarSubGroupLevels)typeof(ActionBarGroupElement).GetField("m_Levels", Cleanup2Fields).GetValue(nativeGroup);
                            if (levels == null) throw new InvalidOperationException("Actual extract spell levels missing.");
                            levels.OnClick(route.RootSpec.Ordinal);
                        }
                        for (int frame = 0; frame < 6; frame++) yield return 0;
                        var sources = nativeGroup.GetComponentsInChildren<ActionBarGroupSlot>(true).Where(value => {
                            var data = value.MechanicSlot?.GetContentData() as AbilityData;
                            return value.gameObject.activeInHierarchy && ReferenceEquals(data?.Blueprint, route.Root) &&
                                ReferenceEquals(data?.Spellbook, route.Book) && (route.Granted == null || ReferenceEquals(data, route.Granted.Data));
                        }).ToArray();
                        if (sources.Length != 1) throw new InvalidOperationException("Actual alternate source row count=" + sources.Length + ":" + route.Label);
                        var source = sources[0]; var original = (AbilityData)source.MechanicSlot.GetContentData();
                        if ((route.Book == null && !(source.MechanicSlot is MechanicActionBarSlotAbility)) ||
                            (route.Book != null && !(source.MechanicSlot is MechanicActionBarSlotMemorizedSpell)))
                            throw new InvalidOperationException("Unexpected alternate native source mechanism: " + source.MechanicSlot.GetType().FullName);
                        try
                        {
                            foreach (int frame in Cleanup2AwaitStationaryNativeHud(source, "alternate-" + route.Label)) yield return frame;
                            source.OnToggleGroupClick(); for (int frame = 0; frame < 6; frame++) yield return 0;
                            var popup = (ActionBarSpellsGroup)typeof(ActionBarGroupSlot).GetField("SubGroup", Cleanup2Fields).GetValue(source);
                            foreach (int frame in Cleanup2AlternatePopup(caster, route, source, original, popup, input,
                                mode, watch, screens, results)) yield return frame;
                        }
                        finally { Cleanup2MenuAdapterHide(source, original); }
                        nativeGroup.Toggle(false, true);
                    }
                    bool exact = input.Unchanged() && ownPins.IsRestored() && routes.All(value => value.GraphExact) &&
                        caster.Descriptor.Spellbooks.SelectMany(value => Enumerable.Range(0, 10).SelectMany(value.GetKnownSpells)).SequenceEqual(known);
                    Cleanup2Check("alternate-native-menu-budget-" + family.Key, exact, new JObject {
                        ["family"] = family.Key, ["nativeLevel"] = caster.Descriptor.Progression.CharacterLevel,
                        ["routes"] = routes.Length, ["sourceResourcesAndSlots"] = input.Describe(), ["pinsExact"] = ownPins.IsRestored(),
                        ["sourceGraphReferencesExact"] = routes.All(value => value.GraphExact), ["knownReferencesExact"] = exact,
                        ["unsupportedTierIII"] = family.Key == AlternateSummoningSourceCatalog.PreservationistKey ?
                            "Bottled Ally native tier mapping is I, II, IV, V, VII, IX; no invented III/1d3 Horse source." : null },
                        "Browsing and native hover/cancel retain every real source, grant, known/prepared object, resource and hotbar entry.");
                    if (!exact) throw new InvalidOperationException("Alternate source changed during browsing: " + family.Key);
                }
                Cleanup2Check("alternate-native-menu-seven-families", results.Count == 20, new JObject {
                    ["rows"] = results, ["actualDesktopSourceQuantityRoutes"] = results.Count,
                    ["consoleQualification"] = false, ["executionQualification"] = false,
                    ["scope"] = "Seven natively levelled families; Horse single/1d3/1d4+1 except Preservationist's absent tier III. Seven actual native single-Horse hover cards. No casting, saved loading or assisted mechanical evidence relabelling." },
                    "Twenty exact native source/quantity rows and seven owned hover cards are distinct from console and casting qualification.");
                if (results.Count != 20) throw new InvalidOperationException("Alternate menu route count differs from explicit native mappings.");
            }
            finally
            {
                var errors = new List<string>();
                Action<string, Action> restore = (name, action) => { try { action(); } catch (Exception error) { errors.Add(name + ":" + error.Message); } };
                try
                {
                    restore("level-up", () => { if (levelController != null) levelController.GetType().GetMethod("Cancel").Invoke(levelController, null); });
                    bool untouched = ReferenceEquals(group.GetCurrentCharacter(), current) && ui.SelectionManagerPC.SelectedUnits.SequenceEqual(selected);
                    bool owned = selectedOwned != null && ReferenceEquals(group.GetCurrentCharacter(), selectedOwned) &&
                        ui.SelectionManagerPC.SelectedUnits.Count == 1 && ReferenceEquals(ui.SelectionManagerPC.SelectedUnits.First(), selectedOwned);
                    bool authority = ReferenceEquals(ActionBarManager.Instance, bar) && ReferenceEquals(GroupController.Instance, group) &&
                        bar.Group.GroupElements.SequenceEqual(groups) && ui.TooltipsController.ContextTooltipData == null &&
                        game.SelectedAbilityHandler?.Ability == null && (untouched || owned);
                    if (!authority) errors.Add("Foreign native UI; selection/toggle restoration refused.");
                    else
                    {
                        for (int i = 0; i < groups.Length; i++) { int index = i; restore("group-toggle", () => groups[index].Toggle(toggles[index], true)); }
                        restore("party", () => { foreach (var caster in casters) player.PartyCharacters.Remove(caster); player.InvalidateCharacterLists(); player.UpdateCharacterLists(); });
                        restore("group-selection", () => group.SelectUnit(current));
                        restore("selection", () => ui.SelectionManagerPC.MultiSelect(selected.Where(value => value?.View != null).Select(value => value.View).ToArray(), false));
                    }
                    bool exact = errors.Count == 0 && player.Party.SequenceEqual(party) && ui.SelectionManagerPC.SelectedUnits.SequenceEqual(selected) &&
                        ReferenceEquals(group.GetCurrentCharacter(), current) && originals.All(value => value.Unchanged()) && pins.All(value => value.IsRestored());
                    Cleanup2Check("alternate-native-menu-ui-restored", exact, new JObject { ["partyExact"] = player.Party.SequenceEqual(party),
                        ["selectionExact"] = ui.SelectionManagerPC.SelectedUnits.SequenceEqual(selected), ["originalBudgetsAndPinsExact"] =
                            originals.All(value => value.Unchanged()) && pins.All(value => value.IsRestored()), ["errors"] = new JArray(errors) },
                        "Only disposable native actors and their UI were used; original party/selection/budgets/hotbars restore exactly.");
                    if (!exact) errors.Add("Final native UI/input comparison failed.");
                }
                finally
                {
                    try { screens?.Dispose(); }
                    finally
                    {
                        try { mode?.Dispose(); }
                        finally { Cleanup2AlternateDisposeCasters(fixture, casters, ownedBlueprints); }
                    }
                }
                if (errors.Count != 0) throw new InvalidOperationException("Alternate menu cleanup failed: " + string.Join("|", errors.ToArray()));
            }
        }

        private void Cleanup2AlternateAdmission(string phase, bool moduleOff)
        {
            if (!IsSummonCleanup2Review || !_request.ExitAfterCompletion || _request.Parameters == null ||
                _request.Parameters.Count != 1 || (string)_request.Parameters["reviewPhase"] != phase)
                throw new InvalidOperationException("Exact automatic-exit alternate menu phase required: " + phase);
            foreach (var family in AlternateSummoningSourceCatalog.All)
            {
                var status = AlternateSummoningStatusRegistry.Find(family.Key);
                var expected = moduleOff ? AlternateSummoningDisposition.RegisteredModuleOff : AlternateSummoningDisposition.Published;
                if (status == null || status.Disposition != expected)
                    throw new InvalidOperationException("Actual alternate dependency/publication missing: " + family.Key + ":" + status);
            }
        }

        private void Cleanup2AlternateBoundary(Cleanup2NativeDefaultScope mode, Stopwatch watch, int seconds)
        {
            if (watch.Elapsed.TotalSeconds > seconds || !mode.Current || !Cleanup2NoNarrativeOverlay() ||
                Game.Instance.SelectedAbilityHandler?.Ability != null)
                throw new InvalidOperationException("Alternate menu wall/native mode/narrative/target ownership changed.");
        }

        private UnitEntityData Cleanup2AlternateCaster(ExpandedSummoningCorrectionFixture fixture,
            AlternateSummoningFamilySpec family, ref object controller, List<UnitEntityData> ownedActors, List<BlueprintUnit> ownedBlueprints)
        {
            string classGuid, archetypeGuid = null; StatType stat;
            switch (family.Key)
            {
                case AlternateSummoningSourceCatalog.MonsterTacticianKey: classGuid = PlayerPathInquisitorClass; stat = StatType.Wisdom; break;
                case AlternateSummoningSourceCatalog.OccultistKey: classGuid = AlternateSummoningContracts.ArcanistClass; archetypeGuid = PlayerPathOccultistArchetype; stat = StatType.Intelligence; break;
                case AlternateSummoningSourceCatalog.FeralHunterKey: classGuid = AlternateSummoningContracts.HunterClass; archetypeGuid = PlayerPathFeralHunterArchetype; stat = StatType.Wisdom; break;
                case AlternateSummoningSourceCatalog.PreservationistKey: classGuid = PlayerPathAlchemistClass; archetypeGuid = PlayerPathPreservationistArchetype; stat = StatType.Intelligence; break;
                default:
                    classGuid = AlternateSummoningContracts.SummonerClass; stat = StatType.Charisma;
                    if (family.Key == AlternateSummoningSourceCatalog.MasterSummonerKey) archetypeGuid = PlayerPathMasterSummonerArchetype;
                    if (family.Key == AlternateSummoningSourceCatalog.NaturalistKey) archetypeGuid = PlayerPathNaturalistArchetype;
                    break;
            }
            var caster = PlayerPathSecondCaster(fixture, "KMG_Cleanup2_AlternateMenu_" + family.Key);
            // Register the exact positive actor before any synchronous class,
            // archetype or resource operation can throw. Blueprint teardown
            // must see this live owner even when native level-up fails.
            ownedActors.Add(caster);
            ownedBlueprints.Add(caster.Blueprint); caster.Descriptor.Stats.GetStat(stat).BaseValue = 20;
            var characterClass = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library, classGuid, family.Key + " native class");
            var archetype = family.Key == AlternateSummoningSourceCatalog.MonsterTacticianKey ? PlayerPathTacticianArchetype(new JObject()) :
                archetypeGuid == null ? null : BlueprintLibraryLookup.RequireExact<BlueprintArchetype>(BlueprintBootstrap.Library, archetypeGuid, family.Key + " native archetype");
            AdvanceDisposableArchetypeCaster(caster.Descriptor, characterClass, archetype, 8, ref controller);
            foreach (BlueprintScriptableObject resource in caster.Descriptor.Resources) caster.Descriptor.Resources.Restore(resource);
            return caster;
        }

        private Cleanup2AlternateMenuRoute[] Cleanup2AlternateRoutes(UnitEntityData caster, AlternateSummoningFamilySpec family,
            BlueprintManifest manifest, bool moduleOff)
        {
            var result = new List<Cleanup2AlternateMenuRoute>();
            foreach (var quantity in new[] { SummonMultiplicity.One, SummonMultiplicity.OneD3, SummonMultiplicity.OneD4PlusOne })
            {
                int tier = quantity == SummonMultiplicity.One ? 2 : quantity == SummonMultiplicity.OneD3 ? 3 : 4;
                var rootSpec = family.Roots.SingleOrDefault(value => value.Tier == tier);
                if (rootSpec == null)
                {
                    if (family.Key == AlternateSummoningSourceCatalog.PreservationistKey && tier == 3) continue;
                    throw new InvalidOperationException("Missing declared alternate tier: " + family.Key + ":" + tier);
                }
                var placement = ExpandedSummoningCatalog.GenerateVariants(family.Family).Single(value =>
                    value.Creature.Key == "horse" && value.ParentTier == tier && value.Multiplicity == quantity);
                string symbol = AlternateSummoningProjection.CreatureSymbol(family.Key, placement);
                string guid = manifest.ResolveActive(symbol, typeof(BlueprintAbility)).Id.Value;
                var child = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library, guid, symbol);
                string canonicalSymbol = ExpandedSummoningIdentityCatalog.AbilitySymbol(placement);
                var canonical = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                    manifest.ResolveActive(canonicalSymbol, typeof(BlueprintAbility)).Id.Value, canonicalSymbol);
                var feature = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(BlueprintBootstrap.Library, rootSpec.FeatureGuid, family.Key + " actual native granting feature");
                if (!caster.Descriptor.HasFact(feature)) throw new InvalidOperationException("Native level-up did not grant exact feature: " + family.Key + ":" + tier);
                BlueprintAbility root;
                if (rootSpec.RootGuid != null) root = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library, rootSpec.RootGuid, rootSpec.RootName);
                else if (!moduleOff) root = child.Parent;
                else
                {
                    var sources = AlternateSummoningHostResolver.ResolveTactician(AlternateSummoningHostResolver.LibraryLookup(BlueprintBootstrap.Library));
                    if (sources.Failure != null) throw new InvalidOperationException(sources.Failure);
                    root = sources.Find(tier, quantity)?.Ability;
                }
                if (root == null || !AlternateSummoningGraph.References<BlueprintAbility>(feature.ComponentsArray).Any(value => ReferenceEquals(value, root)))
                    throw new InvalidOperationException("Stable source is not the actual feature-granted root: " + symbol);
                Ability granted = null; Spellbook book = null;
                if (family.Key == AlternateSummoningSourceCatalog.PreservationistKey)
                {
                    book = caster.Descriptor.Spellbooks.Single(value => value.IsKnown(root));
                    var known = book.GetKnownSpells(rootSpec.Ordinal).Single(value => ReferenceEquals(value.Blueprint, root));
                    if (!book.Memorize(known, null)) throw new InvalidOperationException("Actual known Bottled Ally extract cannot be memorized: " + rootSpec.Ordinal);
                    book.Rest();
                    if (!book.GetMemorizedSpells(rootSpec.Ordinal).Any(value => value.Available && ReferenceEquals(value.Spell.Blueprint, root)))
                        throw new InvalidOperationException("Native extract preparation did not produce an available original source slot.");
                }
                else granted = caster.Descriptor.Abilities.RawFacts.OfType<Ability>().Single(value => ReferenceEquals(value.Blueprint, root));
                var variants = root.Variants ?? new BlueprintAbility[0];
                if (!moduleOff && (!ReferenceEquals(child.Parent, root) || variants.Count(value => ReferenceEquals(value, child)) != 1 ||
                    !AlternateSummoningExposure.IsExposed(child.AssetGuid) || child.name != AlternateSummoningAdapterBuilder.InternalName(symbol) ||
                    canonical.Icon == null || !ReferenceEquals(child.Icon, canonical.Icon)))
                    throw new InvalidOperationException("Stable adapter child is not published once under its actual source: " + symbol);
                var removed = new List<string>();
                foreach (var template in rootSpec.Templates)
                {
                    var original = SummonNativeOptionCatalog.All.Single(value => value.Family == family.Family && value.Tier == tier && value.Multiplicity == template.Multiplicity);
                    if (original.IsSemanticDuplicate || SummonNativeExpansionCatalog.Replaces(family.Family, tier, original.Guid)) removed.Add(template.Guid);
                }
                if (rootSpec.RootGuid == null)
                    removed.Add(manifest.ResolveActive(AlternateSummoningProjection.PreservedSymbol(family.Key, tier, quantity), typeof(BlueprintAbility)).Id.Value);
                var host = AlternateSummoningCoordinator.OriginalHostChildren(family.Key, root.AssetGuid) ?? new BlueprintAbility[0];
                var foreign = host.Where(value => !rootSpec.Templates.Any(template => template.Guid == value.AssetGuid)).Select(value => value.AssetGuid).ToArray();
                result.Add(new Cleanup2AlternateMenuRoute { Family = family, RootSpec = rootSpec, Placement = placement, Root = root, Child = child,
                    CanonicalIcon = canonical.Icon, Granted = granted, Book = book, Components = root.ComponentsArray.ToArray(), Variants = variants.ToArray(), RemovedGeneric = removed.ToArray(), Foreign = foreign });
            }
            return result.ToArray();
        }

        private IEnumerable<int> Cleanup2AlternatePopup(UnitEntityData caster, Cleanup2AlternateMenuRoute route,
            ActionBarGroupSlot source, AbilityData original, ActionBarSpellsGroup popup, Cleanup2TooltipInputs inputs,
            Cleanup2NativeDefaultScope mode, Stopwatch phase, NativeIconScreenEvidence screens, JArray results)
        {
            if (popup == null || !popup.gameObject.activeInHierarchy) throw new InvalidOperationException("Actual alternate popup missing: " + route.Label);
            var rows = popup.GetComponentsInChildren<ActionBarSlot>(true).Where(value => value.gameObject.activeInHierarchy &&
                (value.MechanicSlot?.GetContentData() as AbilityData)?.ConvertedFrom?.Blueprint == route.Root).ToArray();
            var row = rows.Single(value => ReferenceEquals((value.MechanicSlot.GetContentData() as AbilityData)?.Blueprint, route.Child));
            var ability = (AbilityData)row.MechanicSlot.GetContentData();
            var scroll = Cleanup2AcquirePopupSurface(source, popup, route.Root, rows, "alternate-" + route.Label);
            var position = scroll.IsStationary ? Vector2.zero : scroll.Position;
            var velocity = scroll.IsStationary ? Vector2.zero : scroll.Velocity; var anchored = scroll.Content.anchoredPosition;
            Func<bool> owns = () => mode.Current && phase.Elapsed.TotalSeconds < 180 && inputs.Unchanged() && route.GraphExact &&
                Cleanup2NoNarrativeOverlay() && ReferenceEquals(GroupController.Instance.GetCurrentCharacter(), caster) &&
                ReferenceEquals(source.MechanicSlot?.GetContentData(), original) && ReferenceEquals(row.MechanicSlot?.GetContentData(), ability) &&
                ReferenceEquals(ability.ConvertedFrom, original) && ReferenceEquals(ability.Spellbook, route.Book) && scroll.Active &&
                Game.Instance.SelectedAbilityHandler?.Ability == null;
            try
            {
                foreach (int frame in Cleanup2RevealPopupRow(scroll, row, owns)) yield return frame;
                var actual = rows.Select(value => (AbilityData)value.MechanicSlot.GetContentData()).ToArray();
                bool leaves = actual.All(value => !ReferenceEquals(value.Blueprint, route.Root)) &&
                    !actual.Any(value => route.RemovedGeneric.Contains(value.Blueprint.AssetGuid)) &&
                    route.Foreign.All(guid => actual.Count(value => value.Blueprint.AssetGuid == guid) == 1);
                string expected = SummonChoicePresentationPolicy.For("horse", route.Placement.Multiplicity).Label;
                bool pass = owns() && leaves && Cleanup2FullyVisible(scroll, row) && ability.Name == expected &&
                    route.Child.Name == expected && ReferenceEquals(row.Icon.sprite, route.Child.Icon) && route.Child.Icon != null;
                var record = new JObject { ["family"] = route.Family.Key, ["quantity"] = route.Placement.Multiplicity.ToString(),
                    ["nativeSource"] = route.Root.AssetGuid, ["stableChild"] = route.Child.AssetGuid, ["nativeGrant"] = route.RootSpec.FeatureGuid,
                    ["sourceMechanism"] = source.MechanicSlot.GetType().FullName, ["sourceLinkExact"] = ReferenceEquals(ability.ConvertedFrom, original),
                    ["book"] = route.Book?.Blueprint.AssetGuid, ["extractLevel"] = route.Book == null ? (int?)null : route.RootSpec.Ordinal,
                    ["name"] = ability.Name, ["expectedName"] = expected, ["rowIconExact"] = ReferenceEquals(row.Icon.sprite, route.Child.Icon),
                    ["canonicalHorseIconExact"] = ReferenceEquals(row.Icon.sprite, route.CanonicalIcon),
                    ["fullyVisible"] = Cleanup2FullyVisible(scroll, row), ["noRedundantGenericSelectableLeaf"] = leaves,
                    ["excludedExactGenericIdentities"] = new JArray(route.RemovedGeneric), ["foreignChoicesPreserved"] = new JArray(route.Foreign),
                    ["actualRows"] = new JArray(actual.Select(value => value.Blueprint.AssetGuid)), ["originalSourceChain"] = DescribeExpandedSummoningAbilityData(ability) };
                Cleanup2Check("alternate-native-row-" + route.Label, pass, record,
                    "The real native family source offers the exact stable Horse quantity, current name/icon, no exact replaced generic leaf, and unchanged foreign choices/source resources.");
                if (!pass) throw new InvalidOperationException("Actual alternate row presentation failed: " + route.Label);
                results.Add(record);
                if (route.Placement.Multiplicity == SummonMultiplicity.One)
                    foreach (int frame in Cleanup2AlternateHover(route, row, owns, screens)) yield return frame;
            }
            finally
            {
                if (!scroll.Active) throw new InvalidOperationException("Foreign alternate scroll owner; restore refused.");
                if (!scroll.IsStationary) { scroll.RestorePosition(position); scroll.Velocity = velocity; }
                if ((scroll.Content.anchoredPosition - anchored).sqrMagnitude > .0001f)
                    throw new InvalidOperationException("Exact alternate popup scroll restoration failed.");
            }
        }

        private IEnumerable<int> Cleanup2AlternateHover(Cleanup2AlternateMenuRoute route, ActionBarSlot row,
            Func<bool> owns, NativeIconScreenEvidence screens)
        {
            var close = new Cleanup2TooltipCloseState { Controller = Game.Instance.UI.TooltipsController,
                Constructor = Game.Instance.UI.TooltipsController.TooltipConstructor };
            var target = new Cleanup2TooltipTarget { Family = route.Family.Family, Tier = route.Placement.ParentTier,
                Key = "horse", Parent = route.Root, Child = route.Child };
            Exception failure = null;
            var iterator = Cleanup2AlternateHoverBody(route.Label, target, row, owns, screens, close).GetEnumerator();
            try
            {
                while (true)
                {
                    bool next = false; try { next = iterator.MoveNext(); } catch (Exception error) { failure = error; }
                    if (!next) break; yield return iterator.Current;
                }
            }
            finally { try { iterator.Dispose(); } catch (Exception error) { failure = failure == null ? error : new AggregateException(failure, error); } }
            var watch = Stopwatch.StartNew(); JObject state;
            for (int frame = 0; frame < 600 && watch.Elapsed.TotalSeconds < 8; frame++)
            { state = Cleanup2TooltipClosedState(close); if ((bool)state["closed"] || (bool)state["foreignOwner"]) break; yield return 0; }
            state = Cleanup2TooltipClosedState(close); state["originalFailure"] = failure?.ToString();
            Cleanup2Check("alternate-native-hover-close-" + route.Family.Key, (bool)state["closed"], state,
                "The exact owned native tooltip settles its real delay/fade state without hiding or rewriting a foreign context.");
            if (!(bool)state["closed"]) throw new InvalidOperationException("Alternate native hover closure failed: " + state, failure);
            if (failure != null) throw new InvalidOperationException("Alternate hover failed; original error retained after native closure.", failure);
        }

        private IEnumerable<int> Cleanup2AlternateHoverBody(string label, Cleanup2TooltipTarget target, ActionBarSlot row,
            Func<bool> owns, NativeIconScreenEvidence screens, Cleanup2TooltipCloseState close)
        {
            var ability = row.MechanicSlot.GetContentData() as AbilityData;
            var trigger = row.GetComponentsInChildren<TooltipTrigger>(true).SingleOrDefault(value =>
                value.gameObject.activeInHierarchy && ReferenceEquals(value.GetComponentInParent<ActionBarSlot>(), row));
            if (trigger == null || trigger.Disable || !trigger.isActiveAndEnabled || Game.Instance.UI.TooltipsController.ContextTooltipData != null)
                throw new InvalidOperationException("Exact native alternate hover trigger is unavailable or foreign.");
            TooltipData data = null;
            try
            {
                trigger.ShowTooltipManual(true); data = trigger.Data; close.Data = data;
                if (data == null || !ReferenceEquals(data.AbilityData, ability)) throw new InvalidOperationException("Native hover did not collect actual alternate row.");
                var watch = Stopwatch.StartNew(); JObject card = null;
                for (int frame = 0; frame < 600 && watch.Elapsed.TotalSeconds < 8; frame++)
                {
                    if (!owns() || !ReferenceEquals(Game.Instance.UI.TooltipsController.ContextTooltipData, data))
                        throw new InvalidOperationException("Alternate hover/source ownership changed.");
                    card = Cleanup2TooltipCard(target, row, trigger, data); if ((bool)card["passed"]) break; yield return 0;
                }
                bool pass = card != null && (bool)card["passed"];
                Cleanup2Check("alternate-native-hover-" + label, pass, card ?? new JObject(),
                    "Actual native text-only HeaderItem renders the current Horse title without clipping and retains the exact painted source-row icon; no tooltip art is fabricated.");
                if (!pass) throw new InvalidOperationException("Actual alternate native tooltip failed: " + label);
                foreach (int frame in screens.CaptureOwnedHover("alternate-hover-" + label, card,
                    () => owns() && (bool)Cleanup2TooltipCard(target, row, trigger, data)["passed"], trigger, data)) yield return frame;
            }
            finally
            {
                if (data != null)
                {
                    var context = Game.Instance.UI.TooltipsController.ContextTooltipData;
                    if (context != null && !ReferenceEquals(context, data)) throw new InvalidOperationException("Foreign tooltip is not hidden.");
                    trigger.ShowTooltipManual(false); close.CloseRequested = true;
                }
            }
        }

        // Called only in the fresh module-OFF process, with the fixed CotW
        // profile. Absence is not a PASS for any supported family.
        private void ReviewSummonCleanup2AlternateOff(ExpandedSummoningCorrectionFixture fixture)
        {
            Cleanup2AlternateAdmission("elementals-off", true);
            var casters = new List<UnitEntityData>(); var blueprints = new List<BlueprintUnit>(); object controller = null;
            var rows = new JArray();
            try
            {
                var manifest = BlueprintManifest.Load(_context.ModEntry.Path);
                foreach (var family in AlternateSummoningSourceCatalog.All)
                {
                    var caster = Cleanup2AlternateCaster(fixture, family, ref controller, casters, blueprints);
                    var routes = Cleanup2AlternateRoutes(caster, family, manifest, true);
                    var inputs = new Cleanup2TooltipInputs(caster); var pins = new TeleportationUiSettingsFixture(caster.UISettings);
                    var templates = AlternateSummoningCoordinator.TemplateRenders(family.Key);
                    foreach (var route in routes)
                    {
                        var host = AlternateSummoningCoordinator.OriginalHostChildren(family.Key, route.Root.AssetGuid);
                        bool original = route.RootSpec.RootGuid == null ? templates.ContainsKey(route.Root.AssetGuid) &&
                            templates[route.Root.AssetGuid] == SummonSignatureReader.Read(route.Root).Render() :
                            host != null && route.Root.Variants != null && route.Root.Variants.SequenceEqual(host);
                        bool exact = original && route.GraphExact && !AlternateSummoningExposure.IsPublishedRoot(route.Root) &&
                            !AlternateSummoningExposure.IsExposed(route.Child.AssetGuid) &&
                            !route.Variants.Any(value => ReferenceEquals(value, route.Child)) &&
                            (route.Granted == null ? route.Book.IsKnown(route.Root) : ReferenceEquals(route.Granted.Data.Blueprint, route.Root)) &&
                            inputs.Unchanged() && pins.IsRestored();
                        var row = new JObject { ["family"] = family.Key, ["quantity"] = route.Placement.Multiplicity.ToString(),
                            ["source"] = route.Root.AssetGuid, ["registeredAdapter"] = route.Child.AssetGuid,
                            ["originalTemplateOrChildrenExact"] = original, ["notPublished"] = !AlternateSummoningExposure.IsPublishedRoot(route.Root),
                            ["adapterNotExposed"] = !AlternateSummoningExposure.IsExposed(route.Child.AssetGuid),
                            ["actualSourceChildren"] = new JArray(route.Variants.Select(value => value.AssetGuid)),
                            ["originalGrantOrKnownExtractPresent"] = route.Granted != null || route.Book.IsKnown(route.Root),
                            ["sourceBudgets"] = inputs.Describe(), ["passed"] = exact };
                        rows.Add(row); Cleanup2Check("alternate-off-original-source-" + route.Label, exact, row,
                            "A fresh OFF process retains actual native grants/extracts and exact original source templates/child references while every stable adapter remains unexposed.");
                        if (!exact) throw new InvalidOperationException("OFF changed actual alternate source: " + route.Label);
                    }
                }
                Cleanup2Check("alternate-off-seven-real-families", rows.Count == 20, new JObject { ["routes"] = rows,
                    ["nativeUiBrowsed"] = false, ["moduleToggledAtRuntime"] = false,
                    ["scope"] = "Actual class/archetype grants and preserved source graphs in the fresh OFF process. No claim of console pixels or cold save hydration." },
                    "All twenty declared representative native source routes survive OFF; absent CotW would refuse instead of pass.");
            }
            finally
            {
                try { if (controller != null) controller.GetType().GetMethod("Cancel").Invoke(controller, null); }
                finally { Cleanup2AlternateDisposeCasters(fixture, casters, blueprints); }
            }
        }

        private IEnumerable<int> ReviewSummonCleanup2AlternateConsoleMenus(ExpandedSummoningCorrectionFixture fixture)
        {
            Cleanup2AlternateAdmission("alternate-console", false);
            if (!Cleanup2ConsoleNativeUiReady() || !Cleanup2NoNarrativeOverlay())
                throw new InvalidOperationException("Actual native scene-bound console action bar/input layer required.");
            var game = Game.Instance; var player = game.Player; var selection = UnitSelectionManager.Instance;
            var context = game.RootUiContext.InGameUiContext;
            var part = (InGameUiStaticPartContext)typeof(InGameUiContext).GetField("m_StaticPartContext", Cleanup2Fields).GetValue(context);
            var bar = (ActionBarVM)typeof(InGameUiStaticPartContext).GetField("m_ActionBarVm", Cleanup2Fields).GetValue(part);
            if (bar.SelectorVm.Value != null || bar.CurrentPage.Any(value => value.ConvertedVm.Value != null) ||
                game.UI.TooltipsController?.TooltipConstructor == null || game.UI.TooltipsController.ContextTooltipData != null ||
                game.SelectedAbilityHandler?.Ability != null)
                throw new InvalidOperationException("Native console selector/conversion/tooltip must be closed at intake.");
            var party = player.Party.ToArray(); var selectionState = new Cleanup2ConsoleSelectionSnapshot(selection);
            var layers = GamePad.Instance.Layers.ToArray();
            var originals = party.Select(value => new Cleanup2TooltipInputs(value)).ToArray();
            var originalPins = party.Select(value => new TeleportationUiSettingsFixture(value.UISettings)).ToArray();
            var casters = new List<UnitEntityData>(); var blueprints = new List<BlueprintUnit>();
            var watch = Stopwatch.StartNew(); object controller = null; var rows = new JArray(); var labelSamples = new JArray();
            Cleanup2NativeDefaultScope mode = null; NativeIconScreenEvidence screens = null;
            ActionBarSelectorVm ownedSelector = null; ActionBarSlotVM pinnedVm = null; ActionBarConvertedVm ownedConverted = null;
            UnitEntityData pinOwner = null; MechanicActionBarSlot[] pinArray = null, pinContents = null; MechanicActionBarSlot beforePin = null, ownedPin = null;
            TeleportationUiSettingsFixture pinSnapshot = null; int pinIndex = -1;
            try
            {
                mode = Cleanup2EnterNativeDefault(fixture, "alternate-console");
                // Keep the exact native Default stack while browsing/cancelling.
                // Do not request or remove a Pause/foreign input mode.
                screens = new NativeIconScreenEvidence(_request); var manifest = BlueprintManifest.Load(_context.ModEntry.Path);
                foreach (var family in AlternateSummoningSourceCatalog.All)
                {
                    Cleanup2AlternateBoundary(mode, watch, 240);
                    var caster = Cleanup2AlternateCaster(fixture, family, ref controller, casters, blueprints);
                    var routes = Cleanup2AlternateRoutes(caster, family, manifest, false);
                    player.PartyCharacters.Add(caster); player.InvalidateCharacterLists(); player.UpdateCharacterLists();
                    selection.SelectUnit(caster.View, true, true, false);
                    for (int frame = 0; frame < 8; frame++) yield return 0;
                    if (!ReferenceEquals(selection.CurrentSelectUnitValue, caster) || !ReferenceEquals(bar.Unit.Value, caster))
                        throw new InvalidOperationException("Real native console action bar did not bind actual alternate caster.");
                    // The existing level8 Summoner already owns its tier4
                    // granting feature/root. Visit that one additional native
                    // source for quantity and family-first labels; do not
                    // grant higher-tier access or create another caster.
                    var observedRoutes = routes.Where(value => value.Placement.Multiplicity == SummonMultiplicity.One ||
                        (family.Key == AlternateSummoningSourceCatalog.SummonerKey && value.Placement.Multiplicity == SummonMultiplicity.OneD4PlusOne)).ToArray();
                    foreach (var route in observedRoutes)
                    {
                        string routeLabel = family.Key + (route.Placement.Multiplicity == SummonMultiplicity.One ? "" : "-" + route.Placement.Multiplicity);
                        var input = new Cleanup2TooltipInputs(caster);
                        pinSnapshot = new TeleportationUiSettingsFixture(caster.UISettings);
                        var destination = bar.CurrentPage.FirstOrDefault(value => value.IsEmpty && value.Index >= 0 &&
                            value.Index < caster.UISettings.Slots.Length && caster.UISettings.Slots[value.Index] is MechanicActionBarSlotEmpty);
                        if (destination == null) throw new InvalidOperationException("Native console fixture has no visible empty slot.");
                        pinOwner = caster; pinArray = caster.UISettings.Slots; pinContents = pinArray.ToArray(); pinIndex = destination.Index; beforePin = pinArray[pinIndex];
                        typeof(ActionBarVM).GetMethod("SetSlotSelected", Cleanup2Fields).Invoke(bar, new object[] { destination });
                        typeof(ActionBarVM).GetMethod(route.Book == null ? "ShowAbilityMenu" : "ShowSpellMenu", Cleanup2Fields).Invoke(bar, null);
                        ownedSelector = bar.SelectorVm.Value;
                        for (int frame = 0; frame < 8; frame++) yield return 0;
                        if (ownedSelector == null || !ReferenceEquals(bar.SelectorVm.Value, ownedSelector)) throw new InvalidOperationException("Native alternate source selector absent.");
                        var selectorView = Resources.FindObjectsOfTypeAll<ActionBarSelectorView>().Single(value => value.gameObject.scene.IsValid() &&
                            value.gameObject.scene.isLoaded && value.gameObject.activeInHierarchy && ReferenceEquals(ElementalConsoleBookPresentation.Model(value), ownedSelector));
                        var item = ownedSelector.ItemVms.Single(value => {
                            var data = value.MechanicActionBarSlot.GetContentData() as AbilityData;
                            return ReferenceEquals(data?.Blueprint, route.Root) && ReferenceEquals(data?.Spellbook, route.Book) &&
                                (route.Granted == null || ReferenceEquals(data, route.Granted.Data));
                        });
                        var source = (AbilityData)item.MechanicActionBarSlot.GetContentData(); ownedPin = item.MechanicActionBarSlot;
                        var itemView = selectorView.GetComponentsInChildren<ActionBarSelectorItemView>(false)
                            .Single(value => ReferenceEquals(ElementalConsoleBookPresentation.Model(value), item));
                        var navigation = (ConsoleNavigationBehavior)typeof(ActionBarSelectorView).GetField("m_NavigationBehavior", Cleanup2Fields).GetValue(selectorView);
                        if (!navigation.SelectEntityManually(itemView) || !ReferenceEquals(navigation.GetCurrentSelectedEntity(), itemView))
                            throw new InvalidOperationException("Actual native alternate selector focus failed.");
                        // Installed native confirm pins this exact collected
                        // mechanism; it is not the Use/casting input.
                        InvokeTeleportGamepadInput(selectorView, "<GetInputLayer>b__11_0");
                        for (int frame = 0; frame < 8; frame++) yield return 0;
                        if (bar.SelectorVm.Value != null || !ReferenceEquals(caster.UISettings.Slots, pinArray) || !ReferenceEquals(pinArray[pinIndex], ownedPin))
                            throw new InvalidOperationException("Native console confirm did not retain exact original source mechanism.");
                        ownedSelector = null;
                        pinnedVm = bar.CurrentPage.Single(value => value.Index == pinIndex && ReferenceEquals(value.MechanicActionBarSlot, ownedPin));
                        var sourceView = Resources.FindObjectsOfTypeAll<ActionBarSlotView>().Single(value => value.gameObject.scene.IsValid() &&
                            value.gameObject.scene.isLoaded && value.gameObject.activeInHierarchy && ReferenceEquals(value.GetViewModel(), pinnedVm));
                        pinnedVm.OnShowConvertRequest(); ownedConverted = pinnedVm.ConvertedVm.Value;
                        for (int frame = 0; frame < 8; frame++) yield return 0;
                        var convertedView = (ActionBarConvertedView)typeof(ActionBarSlotView).GetField("m_ConvertedView", Cleanup2Fields).GetValue(sourceView);
                        if (ownedConverted == null || convertedView == null || !convertedView.gameObject.activeInHierarchy ||
                            !ReferenceEquals(ElementalConsoleBookPresentation.Model(convertedView), ownedConverted))
                            throw new InvalidOperationException("Actual native console converted panel did not bind the source's native conversion list.");
                        var childVm = ownedConverted.Slots.Single(value => ReferenceEquals((value.MechanicActionBarSlot.GetContentData() as AbilityData)?.Blueprint, route.Child));
                        var child = (AbilityData)childVm.MechanicActionBarSlot.GetContentData();
                        var childView = convertedView.GetComponentsInChildren<ActionBarSlotView>(false).Single(value => ReferenceEquals(value.GetViewModel(), childVm));
                        var nativeNavigation = (ConsoleMultiNavigationCollection)typeof(ActionBarConvertedView).GetField("m_NavigationCollection", Cleanup2Fields).GetValue(convertedView);
                        nativeNavigation.SetCurrentEntityManual(childView);
                        for (int frame = 0; frame < 4; frame++) yield return 0;
                        var icon = (Image)typeof(ActionBarSlotView).GetField("m_Icon", Cleanup2Fields).GetValue(childView);
                        var actual = ownedConverted.Slots.Select(value => value.MechanicActionBarSlot.GetContentData() as AbilityData).ToArray();
                        bool exact = mode.Current && input.Unchanged() && routes.All(value => value.GraphExact) &&
                            ReferenceEquals(child.ConvertedFrom, source) && ReferenceEquals(child.Spellbook, route.Book) &&
                            ReferenceEquals(nativeNavigation.CurrentEntity, childView) && child.Name == SummonChoicePresentationPolicy.For("horse", route.Placement.Multiplicity).Label &&
                            ReferenceEquals(icon.sprite, route.Child.Icon) && Cleanup2TooltipGraphicVisible(icon) &&
                            actual.All(value => value != null && !ReferenceEquals(value.Blueprint, route.Root) && !route.RemovedGeneric.Contains(value.Blueprint.AssetGuid)) &&
                            route.Foreign.All(guid => actual.Count(value => value.Blueprint.AssetGuid == guid) == 1);
                        var record = new JObject { ["family"] = family.Key, ["quantity"] = route.Placement.Multiplicity.ToString(),
                            ["source"] = route.Root.AssetGuid, ["child"] = child.Blueprint.AssetGuid,
                            ["sourceMechanism"] = ownedPin.GetType().FullName, ["sourceLinkExact"] = ReferenceEquals(child.ConvertedFrom, source),
                            ["book"] = route.Book?.Blueprint.AssetGuid, ["nativeSelectorView"] = selectorView.GetInstanceID(),
                            ["nativeConvertedView"] = convertedView.GetInstanceID(), ["actualRows"] = new JArray(actual.Select(value => value?.Blueprint.AssetGuid)),
                            ["paintedRowIconVisibleAndExact"] = ReferenceEquals(icon.sprite, route.Child.Icon) && Cleanup2TooltipGraphicVisible(icon),
                            ["sourceResourcesAndSlots"] = input.Describe(), ["pinIndex"] = pinIndex,
                            ["scope"] = "Actual native source selector confirm temporarily pins the original source; its native OnShowConvertRequest creates the real scene-bound converted panel. Native navigation focuses the stable Horse child without Use or cast." };
                        Cleanup2Check("alternate-console-native-row-" + routeLabel, exact, record,
                            "The real controller source and converted panel retain native source/book chains, stable child/icon/name, and source budgets without a redundant generic selectable leaf.");
                        if (!exact) throw new InvalidOperationException("Native console alternate row failed: " + family.Key);
                        if (route.Placement.Multiplicity == SummonMultiplicity.One) rows.Add(record);
                        foreach (int frame in Cleanup2AlternateConsoleHover(route, childView, child, () => mode.Current && input.Unchanged() && route.GraphExact &&
                            ReferenceEquals(pinnedVm.ConvertedVm.Value, ownedConverted) && ReferenceEquals(childView.GetViewModel(), childVm) &&
                            ReferenceEquals(selection.CurrentSelectUnitValue, caster) && ReferenceEquals(pinArray[pinIndex], ownedPin), screens)) yield return frame;
                        if (route.Placement.Multiplicity == SummonMultiplicity.OneD4PlusOne)
                        {
                            labelSamples.Add(new JObject { ["key"] = "horse", ["quantity"] = route.Placement.Multiplicity.ToString(),
                                ["source"] = route.Root.AssetGuid, ["child"] = child.Blueprint.AssetGuid, ["nativeTitle"] = child.Name });
                            foreach (string key in new[] { "small-fire-elemental", "air-mephit" })
                            {
                                Cleanup2AlternateBoundary(mode, watch, 240);
                                var quantity = key == "small-fire-elemental" ? SummonMultiplicity.OneD4PlusOne : SummonMultiplicity.One;
                                var placement = ExpandedSummoningCatalog.GenerateVariants(family.Family).Single(value => value.ParentTier == 4 &&
                                    value.Creature.Key == key && value.Multiplicity == quantity);
                                string symbol = AlternateSummoningProjection.CreatureSymbol(family.Key, placement);
                                var expectedChild = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                                    manifest.ResolveActive(symbol, typeof(BlueprintAbility)).Id.Value, symbol);
                                if (!SummonVisibilityCatalog.IsPublished(placement) || !ReferenceEquals(expectedChild.Parent, route.Root) ||
                                    route.Variants.Count(value => ReferenceEquals(value, expectedChild)) != 1 || !AlternateSummoningExposure.IsExposed(expectedChild.AssetGuid))
                                    throw new InvalidOperationException("Exact already-granted tier4 native label route is not published: " + symbol);
                                var sampleVm = ownedConverted.Slots.Single(value => ReferenceEquals((value.MechanicActionBarSlot.GetContentData() as AbilityData)?.Blueprint, expectedChild));
                                var sample = (AbilityData)sampleVm.MechanicActionBarSlot.GetContentData();
                                var sampleView = convertedView.GetComponentsInChildren<ActionBarSlotView>(false).Single(value => ReferenceEquals(value.GetViewModel(), sampleVm));
                                nativeNavigation.SetCurrentEntityManual(sampleView);
                                for (int frame = 0; frame < 4; frame++) yield return 0;
                                var sampleIcon = (Image)typeof(ActionBarSlotView).GetField("m_Icon", Cleanup2Fields).GetValue(sampleView);
                                Func<bool> sampleOwned = () => mode.Current && watch.Elapsed.TotalSeconds < 240 && input.Unchanged() && route.GraphExact &&
                                    ReferenceEquals(selection.CurrentSelectUnitValue, caster) && ReferenceEquals(bar.Unit.Value, caster) &&
                                    ReferenceEquals(caster.UISettings.Slots, pinArray) && ReferenceEquals(pinArray[pinIndex], ownedPin) &&
                                    ReferenceEquals(pinnedVm.ConvertedVm.Value, ownedConverted) && ReferenceEquals(ElementalConsoleBookPresentation.Model(convertedView), ownedConverted) &&
                                    ReferenceEquals(sampleView.GetViewModel(), sampleVm) && ReferenceEquals(sampleVm.MechanicActionBarSlot.GetContentData(), sample) &&
                                    ReferenceEquals(sample.ConvertedFrom, source) && ReferenceEquals(sample.Spellbook, route.Book) &&
                                    ReferenceEquals(nativeNavigation.CurrentEntity, sampleView) && game.SelectedAbilityHandler?.Ability == null && Cleanup2NoNarrativeOverlay();
                                string expectedTitle = SummonChoicePresentationPolicy.For(key, quantity).Label;
                                bool sampleExact = sampleOwned() && ReferenceEquals(sample.Blueprint, expectedChild) && sample.Name == expectedTitle &&
                                    ReferenceEquals(sampleIcon?.sprite, expectedChild.Icon) && Cleanup2TooltipGraphicVisible(sampleIcon);
                                var sampleRecord = new JObject { ["family"] = family.Key, ["key"] = key, ["quantity"] = quantity.ToString(),
                                    ["source"] = source.Blueprint.AssetGuid, ["grant"] = route.RootSpec.FeatureGuid, ["child"] = expectedChild.AssetGuid,
                                    ["nativeTitle"] = sample.Name, ["expectedTitle"] = expectedTitle, ["sourceChainExact"] = ReferenceEquals(sample.ConvertedFrom, source),
                                    ["nativeSourceBudgetsExact"] = input.Unchanged(), ["nativeIconVisibleAndExact"] = ReferenceEquals(sampleIcon?.sprite, expectedChild.Icon) && Cleanup2TooltipGraphicVisible(sampleIcon),
                                    ["scope"] = "Existing level8 Summoner's actual tier4 feature-granted native source; native converted row and navigation. No new access, source context, allocation or cast." };
                                Cleanup2Check("alternate-console-native-family-label-" + key, sampleExact, sampleRecord,
                                    "The actual controller converted row retains its existing native grant/source, stable adapter identity and family-first quantity title before source-owned native hover.");
                                if (!sampleExact) throw new InvalidOperationException("Native existing-grant controller label row failed: " + key);
                                foreach (int frame in Cleanup2AlternateConsoleHover(route, sampleView, sample, sampleOwned, screens, key, quantity)) yield return frame;
                                labelSamples.Add(sampleRecord);
                            }
                        }
                        InvokeTeleportGamepadInput(convertedView, "<GetInputLayer>b__8_1");
                        for (int frame = 0; frame < 8; frame++) yield return 0;
                        bool cancelled = pinnedVm.ConvertedVm.Value == null && GamePad.Instance.Layers.SequenceEqual(layers) && input.Unchanged();
                        Cleanup2Check("alternate-console-native-cancel-" + routeLabel, cancelled, new JObject { ["conversionClosed"] = pinnedVm.ConvertedVm.Value == null,
                            ["inputLayersExact"] = GamePad.Instance.Layers.SequenceEqual(layers), ["resourcesAndSlotsExact"] = input.Unchanged(),
                            ["targetingAbsent"] = game.SelectedAbilityHandler.Ability == null },
                            "The actual native controller Decline handler closes only the owned converted panel, returns its input layer, and spends nothing.");
                        if (!cancelled) throw new InvalidOperationException("Native console alternate cancel did not restore its exact input layer/budget.");
                        ownedConverted = null;
                        Cleanup2AlternateRestorePin(bar, pinOwner, pinArray, pinContents, pinIndex, beforePin, ownedPin, pinSnapshot);
                        pinnedVm = null; ownedPin = null; pinOwner = null; pinSnapshot = null; pinArray = null; pinContents = null; pinIndex = -1;
                    }
                }
                Cleanup2Check("alternate-console-family-label-sample-scope", labelSamples.Count == 3, new JObject {
                    ["nativeSamples"] = labelSamples, ["giantControllerObserved"] = false,
                    ["giantLimit"] = "Existing fixture classlevel8 exposes validated tiers2–4. Published Hill/Stone Giants begin tier6; no higher class levels or grants are added. Retained PC Giant tooltip evidence is separate.",
                    ["newNativeAccessGranted"] = false, ["nativeCastingInvoked"] = false },
                    "One already-owned tier4 source supplies native Elemental/Mephit and quantity-title observations without expanding fixture access; Giant controller coverage is explicitly unexecuted.");
                if (labelSamples.Count != 3) throw new InvalidOperationException("Bounded native controller family/quantity label samples incomplete.");
                Cleanup2Check("alternate-console-seven-family-menus", rows.Count == 7, new JObject { ["rows"] = rows,
                    ["actualInputMode"] = "console", ["castingQualification"] = false, ["quantityScope"] = "One exact single-Horse native menu per family, plus the existing Summoner tier4 source for Horse (1d4+1), Elemental, Fire, Small (1d4+1), and Mephit, Air native labels. Desktop separately checks all supported Horse quantities." },
                    "Seven actual bound native controller source/variant menus, hover cards, cancellation and restored temporary pins are distinct from desktop or cast qualification.");
                if (rows.Count != 7) throw new InvalidOperationException("Not all supported alternate console families were observed.");
            }
            finally
            {
                var errors = new List<string>();
                Action<string, Action> restore = (name, action) => { try { action(); } catch (Exception error) { errors.Add(name + ":" + error.Message); } };
                try
                {
                    restore("level-up", () => { if (controller != null) controller.GetType().GetMethod("Cancel").Invoke(controller, null); });
                    restore("selector", () => {
                        if (bar.SelectorVm.Value == null) return;
                        if (!ReferenceEquals(bar.SelectorVm.Value, ownedSelector)) throw new InvalidOperationException("Foreign selector preserved.");
                        ownedSelector.OnDecline();
                    });
                    restore("conversion", () => {
                        if (pinnedVm == null || pinnedVm.ConvertedVm.Value == null) return;
                        if (!ReferenceEquals(pinnedVm.ConvertedVm.Value, ownedConverted)) throw new InvalidOperationException("Foreign converted panel preserved.");
                        ownedConverted.Decline();
                    });
                    restore("temporary-pin", () => { if (pinOwner != null) Cleanup2AlternateRestorePin(bar, pinOwner, pinArray, pinContents, pinIndex, beforePin, ownedPin, pinSnapshot); });
                    bool ownsSelection = selection.CurrentSelectUnitValue == null || party.Contains(selection.CurrentSelectUnitValue) || casters.Contains(selection.CurrentSelectUnitValue);
                    if (!ownsSelection || game.UI.TooltipsController.ContextTooltipData != null || game.SelectedAbilityHandler?.Ability != null)
                        errors.Add("Foreign console selection/tooltip/target; original UI is not overwritten.");
                    else
                    {
                        restore("party", () => { foreach (var caster in casters) player.PartyCharacters.Remove(caster); player.InvalidateCharacterLists(); player.UpdateCharacterLists(); });
                        restore("selection", () => { if (!selectionState.Restore(fixture)) throw new InvalidOperationException(selectionState.Outcome); });
                    }
                    bool exact = errors.Count == 0 && player.Party.SequenceEqual(party) && selectionState.Restored &&
                        GamePad.Instance.Layers.SequenceEqual(layers) && originals.All(value => value.Unchanged()) && originalPins.All(value => value.IsRestored());
                    Cleanup2Check("alternate-console-owned-state-restored", exact, new JObject { ["partyExact"] = player.Party.SequenceEqual(party),
                        ["selection"] = selectionState.Describe(), ["inputLayersExact"] = GamePad.Instance.Layers.SequenceEqual(layers),
                        ["originalBudgetsAndHotbarsExact"] = originals.All(value => value.Unchanged()) && originalPins.All(value => value.IsRestored()), ["errors"] = new JArray(errors) },
                        "The native console owner restores original selection/group/party/input layers and hotbars without casting or saved mutation.");
                    if (!exact) errors.Add("Final console state comparison failed.");
                }
                finally
                {
                    try { screens?.Dispose(); }
                    finally { try { mode?.Dispose(); } finally { Cleanup2AlternateDisposeCasters(fixture, casters, blueprints); } }
                }
                if (errors.Count != 0) throw new InvalidOperationException("Alternate console cleanup failed: " + string.Join("|", errors.ToArray()));
            }
        }

        private IEnumerable<int> Cleanup2AlternateConsoleHover(Cleanup2AlternateMenuRoute route, ActionBarSlotView row,
            AbilityData ability, Func<bool> owns, NativeIconScreenEvidence screens,
            string key = "horse", SummonMultiplicity? quantity = null)
        {
            var actualQuantity = quantity ?? route.Placement.Multiplicity;
            string label = route.Family.Key + (key == "horse" && actualQuantity == SummonMultiplicity.One ? "" : "-" + key + "-" + actualQuantity);
            var close = new Cleanup2TooltipCloseState { Controller = Game.Instance.UI.TooltipsController,
                Constructor = Game.Instance.UI.TooltipsController.TooltipConstructor };
            Exception failure = null;
            var iterator = Cleanup2AlternateConsoleHoverBody(route, row, ability, owns, screens, close, key, actualQuantity, label).GetEnumerator();
            try
            {
                while (true)
                {
                    bool next = false; try { next = iterator.MoveNext(); } catch (Exception error) { failure = error; }
                    if (!next) break; yield return iterator.Current;
                }
            }
            finally { try { iterator.Dispose(); } catch (Exception error) { failure = failure == null ? error : new AggregateException(failure, error); } }
            var watch = Stopwatch.StartNew(); JObject state;
            for (int frame = 0; frame < 600 && watch.Elapsed.TotalSeconds < 8; frame++)
            { state = Cleanup2TooltipClosedState(close); if ((bool)state["closed"] || (bool)state["foreignOwner"]) break; yield return 0; }
            state = Cleanup2TooltipClosedState(close); state["originalFailure"] = failure?.ToString();
            Cleanup2Check("alternate-console-native-hover-close-" + label, (bool)state["closed"], state,
                "The exact native console-row hover closes through its own trigger and settles the native tooltip delay/fade before conversion cancellation.");
            if (!(bool)state["closed"]) throw new InvalidOperationException("Native console hover did not close; no foreign context is hidden.", failure);
            if (failure != null) throw new InvalidOperationException("Native console hover failed; original failure retained after closure.", failure);
        }

        private IEnumerable<int> Cleanup2AlternateConsoleHoverBody(Cleanup2AlternateMenuRoute route, ActionBarSlotView row,
            AbilityData ability, Func<bool> owns, NativeIconScreenEvidence screens, Cleanup2TooltipCloseState close,
            string key, SummonMultiplicity quantity, string label)
        {
            var trigger = (TooltipTrigger)typeof(ActionBarSlotView).GetField("m_TooltipTrigger", Cleanup2Fields).GetValue(row);
            var icon = (Image)typeof(ActionBarSlotView).GetField("m_Icon", Cleanup2Fields).GetValue(row);
            if (trigger == null || trigger.Disable || !trigger.isActiveAndEnabled || Game.Instance.UI.TooltipsController.ContextTooltipData != null)
                throw new InvalidOperationException("Native console alternate row tooltip is unavailable or already foreign.");
            TooltipData data = null;
            try
            {
                trigger.ShowTooltipManual(true); data = trigger.Data; close.Data = data;
                if (data == null || !ReferenceEquals(data.AbilityData, ability)) throw new InvalidOperationException("Native console tooltip did not collect exact converted row AbilityData.");
                var watch = Stopwatch.StartNew(); JObject card = null;
                for (int frame = 0; frame < 600 && watch.Elapsed.TotalSeconds < 8; frame++)
                {
                    if (!owns() || !ReferenceEquals(Game.Instance.UI.TooltipsController.ContextTooltipData, data))
                        throw new InvalidOperationException("Native console hover/source ownership changed.");
                    card = Cleanup2AlternateConsoleCard(route, ability, trigger, data, icon, key, quantity);
                    if ((bool)card["passed"]) break; yield return 0;
                }
                bool passed = card != null && (bool)card["passed"];
                Cleanup2Check("alternate-console-native-hover-" + label, passed, card ?? new JObject(),
                    "The exact native console converted-row tooltip renders its expected family/quantity HeaderItem title without clipping; the original child row icon remains visible. Native tooltip art is intentionally absent.");
                if (!passed) throw new InvalidOperationException("Native console tooltip title/icon did not become visible: " + label);
                foreach (int frame in screens.CaptureOwnedHover("alternate-console-hover-" + label, card,
                    () => owns() && (bool)Cleanup2AlternateConsoleCard(route, ability, trigger, data, icon, key, quantity)["passed"], trigger, data)) yield return frame;
            }
            finally
            {
                if (data != null)
                {
                    var context = Game.Instance.UI.TooltipsController.ContextTooltipData;
                    if (context != null && !ReferenceEquals(context, data)) throw new InvalidOperationException("Foreign console tooltip is not hidden.");
                    trigger.ShowTooltipManual(false); close.CloseRequested = true;
                }
            }
        }

        private static JObject Cleanup2AlternateConsoleCard(Cleanup2AlternateMenuRoute route, AbilityData ability,
            TooltipTrigger trigger, TooltipData data, Image icon, string key, SummonMultiplicity quantity)
        {
            var tips = Game.Instance.UI.TooltipsController; var constructor = tips.TooltipConstructor;
            var header = constructor.Body?.HeaderBox; var template = tips.Bricks?.HeaderItem;
            var field = typeof(DescriptionBricksBox).GetField("m_ShowList", Cleanup2Fields);
            var shown = header == null || field == null ? null : field.GetValue(header) as IList<DescriptionBrick>;
            var items = shown == null || template == null ? new DescriptionBrick[0] : shown.Where(value => value != null &&
                value.gameObject.activeInHierarchy && value.GetType() == template.GetType() && value.BlockID == template.BlockID &&
                value.transform.parent == header.Content).ToArray();
            var item = items.Length == 1 ? items[0] : null;
            var title = item?.TextFields == null || item.TextFields.Count == 0 ? null : item.TextFields[0];
            bool exact = items.Length == 1 && !string.IsNullOrEmpty(template?.BlockID) && ReferenceEquals(constructor.CurrentTooltipData, data);
            string expectedTitle = SummonChoicePresentationPolicy.For(key, quantity).Label;
            bool name = title != null && title.GetParsedText() == expectedTitle &&
                !title.isTextTruncated && !title.isTextOverflowing && Cleanup2TooltipGraphicVisible(title);
            bool textOnly = exact && item.Images != null && template.Images != null && item.Images.Count == template.Images.Count &&
                item.Images.All(value => value != null && !value.gameObject.activeInHierarchy);
            bool originalIcon = ability.Blueprint.Icon != null && ReferenceEquals(icon?.sprite, ability.Blueprint.Icon) && Cleanup2TooltipGraphicVisible(icon);
            return new JObject { ["passed"] = NativeIconScreenEvidence.OwnsHover(trigger, data) && exact && name && textOnly && originalIcon &&
                    ReferenceEquals(data.AbilityData, ability), ["family"] = route.Family.Key, ["key"] = key, ["quantity"] = quantity.ToString(),
                ["source"] = route.Root.AssetGuid, ["child"] = ability.Blueprint.AssetGuid, ["exactNativeHeaderItem"] = exact,
                ["headerBlockId"] = template?.BlockID, ["nativeTitle"] = title?.GetParsedText(), ["expectedTitle"] = expectedTitle, ["titleVisibleWithoutClipping"] = name,
                ["nativeTextOnlyHeader"] = textOnly, ["tooltipArtDisplayed"] = !textOnly,
                ["originalRowIconVisibleAndExact"] = originalIcon, ["nativeDataIsExactConvertedChild"] = ReferenceEquals(data.AbilityData, ability),
                ["scope"] = "Actual native console converted slot, bound TooltipTrigger and native HeaderItem; no manufactured tooltip data, art, text or source chain." };
        }

        private static void Cleanup2AlternateRestorePin(ActionBarVM bar, UnitEntityData owner, MechanicActionBarSlot[] slots, MechanicActionBarSlot[] contents,
            int index, MechanicActionBarSlot prior, MechanicActionBarSlot owned, TeleportationUiSettingsFixture snapshot)
        {
            if (!ReferenceEquals(UnitSelectionManager.Instance.CurrentSelectUnitValue, owner) || !ReferenceEquals(bar.Unit.Value, owner) ||
                !ReferenceEquals(owner.UISettings.Slots, slots) || index < 0 || index >= slots.Length ||
                contents == null || contents.Length != slots.Length || slots.Where((value, i) => i != index && !ReferenceEquals(value, contents[i])).Any() ||
                (!ReferenceEquals(slots[index], owned) && !ReferenceEquals(slots[index], prior)) ||
                Game.Instance.UI.TooltipsController.ContextTooltipData != null || Game.Instance.SelectedAbilityHandler.Ability != null)
                throw new InvalidOperationException("Foreign native console pin/selection/tooltip; temporary pin is not overwritten.");
            if (ReferenceEquals(slots[index], owned)) owner.UISettings.SetSlot(prior, index);
            // The full metadata snapshot belongs to this new disposable actor.
            // Before restoring it, all other serialized slot entries must
            // already match; the native call only dirtied metadata.
            snapshot.Restore();
            typeof(ActionBarVM).GetMethod("OnUnitChanged", Cleanup2Fields).Invoke(bar, null);
            if (!snapshot.IsRestored()) throw new InvalidOperationException("Exact original console pin/metadata did not restore.");
        }

        private void Cleanup2AlternateDisposeCasters(ExpandedSummoningCorrectionFixture fixture,
            List<UnitEntityData> casters, List<BlueprintUnit> blueprints)
        {
            foreach (var caster in casters) if (caster != null && !caster.Destroyed) CleanupExpandedSummoningUnit(caster);
            Game.Instance.EntityCreator.Tick();
            foreach (var blueprint in blueprints)
            {
                if (casters.Any(value => value != null && !value.Destroyed && ReferenceEquals(value.Blueprint, blueprint))) continue;
                _playerPathBlueprints.Remove(blueprint); UnityEngine.Object.Destroy(blueprint);
            }
            // fixture.Created retains the exact actors so the outer native
            // fixture census/destruction proof still owns any pending drain.
        }
    }
}
