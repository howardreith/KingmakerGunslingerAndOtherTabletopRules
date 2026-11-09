using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.ActionBar;
using Kingmaker.UI.Group;
using Kingmaker.UI.Selection;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Save-free observation of the alphabetical menus: a census of every
        // published menu plus real native popups. The quantity suffix parsing
        // below is observer-only, as an independent check of the product order.
        private sealed class AlphaMenu
        {
            internal ActionBarSpellsGroup Popup;
            internal ActionBarSlot[] Rows = new ActionBarSlot[0];
            internal AbilityData[] Data = new AbilityData[0];
        }

        private static int AlphaQuantityRank(string label)
        { return label.EndsWith(" (1d4+1)", StringComparison.Ordinal) ? 2 : label.EndsWith(" (1d3)", StringComparison.Ordinal) ? 1 : 0; }

        private static string AlphaCreatureName(string label)
        {
            foreach (string suffix in new[] { " (1d3)", " (1d4+1)" })
                if (label.EndsWith(suffix, StringComparison.Ordinal)) return label.Substring(0, label.Length - suffix.Length);
            return label;
        }

        private static JObject AlphaOrderVerdict(IList<string> names, IList<bool> creature)
        {
            int violation = -1; bool tail = false; string previous = null;
            for (int index = 0; index < names.Count && violation < 0; index++)
            {
                if (!creature[index]) { tail = true; continue; }
                if (tail) { violation = index; break; }
                if (previous != null)
                {
                    int compare = string.Compare(AlphaCreatureName(previous), AlphaCreatureName(names[index]), StringComparison.OrdinalIgnoreCase);
                    if (compare > 0 || (compare == 0 && AlphaQuantityRank(previous) > AlphaQuantityRank(names[index]))) violation = index;
                }
                previous = names[index];
            }
            return new JObject { ["rows"] = names.Count, ["sorted"] = violation < 0, ["violationIndex"] = violation,
                ["unclassifiedRows"] = creature.Count(value => !value),
                ["first"] = names.FirstOrDefault(), ["last"] = names.LastOrDefault() };
        }

        private static bool AlphaIsCreatureChoice(BlueprintAbility ability)
        {
            return ability != null && !string.IsNullOrWhiteSpace(ability.Name) && AlternateSummoningGraph.SpawnUnits(ability).Count > 0;
        }

        private JObject AlphaCensus(out bool passed)
        {
            var menus = new JArray(); var unclassified = new JArray();
            bool allSorted = true; int rowCount = 0, stirge = 0, canonical = 0, adapter = 0;
            var library = BlueprintBootstrap.Library;
            Action<string, BlueprintAbility> inspect = (kind, root) =>
            {
                BlueprintAbility[] variants = root?.Variants ?? new BlueprintAbility[0];
                string[] names = variants.Select(value => value == null ? "<null>" : value.Name ?? "<null>").ToArray();
                bool[] creature = variants.Select(AlphaIsCreatureChoice).ToArray();
                JObject verdict = AlphaOrderVerdict(names, creature);
                verdict["kind"] = kind; verdict["root"] = root.AssetGuid; verdict["rootName"] = root.name;
                menus.Add(verdict);
                allSorted &= (bool)verdict["sorted"]; rowCount += variants.Length;
                stirge += names.Count(value => value.IndexOf("Stirge", StringComparison.OrdinalIgnoreCase) >= 0);
                foreach (BlueprintAbility row in variants.Where(value => !AlphaIsCreatureChoice(value)))
                    unclassified.Add(new JObject { ["menu"] = root.name, ["ability"] = row?.AssetGuid, ["name"] = row?.Name, ["internalName"] = row?.name });
            };
            foreach (string guid in ExpandedSummoningPublisher.CanonicalParentGuids)
            {
                inspect("canonical", BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library, guid, "alphabetical census parent")); canonical++;
            }
            foreach (BlueprintAbility ability in library.BlueprintsByAssetId.Values.OfType<BlueprintAbility>()
                .Where(AlternateSummoningExposure.IsPublishedRoot).OrderBy(value => value.AssetGuid, StringComparer.Ordinal))
            {
                inspect("alternate", ability); adapter++;
            }
            passed = allSorted && canonical == 18 && adapter > 0 && stirge == 0;
            return new JObject { ["canonicalMenus"] = canonical, ["alternateMenus"] = adapter, ["rows"] = rowCount,
                ["allSorted"] = allSorted, ["stirgeRows"] = stirge, ["unclassified"] = unclassified, ["menus"] = menus,
                ["scope"] = "Structural census of published host arrays; not popups opened in game." };
        }

        // Monster Tactician roots are reached only through their granting features.
        private static BlueprintAbility AlphaResolveRoot(AlternateSummoningRootSpec spec)
        {
            if (spec == null) return null;
            var library = BlueprintBootstrap.Library.BlueprintsByAssetId;
            BlueprintScriptableObject found;
            IEnumerable<BlueprintAbility> candidates = spec.RootGuid != null ?
                (library.TryGetValue(spec.RootGuid, out found) && found is BlueprintAbility ? new[] { (BlueprintAbility)found } : new BlueprintAbility[0]) :
                !library.TryGetValue(spec.FeatureGuid, out found) ? new BlueprintAbility[0] :
                    (found.ComponentsArray ?? new BlueprintComponent[0]).OfType<Kingmaker.UnitLogic.FactLogic.AddFacts>().SelectMany(value => value.Facts).OfType<BlueprintAbility>();
            return candidates.Where(AlternateSummoningExposure.IsPublishedRoot).OrderBy(value => value.AssetGuid, StringComparer.Ordinal).FirstOrDefault();
        }

        private static AlphaMenu AlphaReadPopup(ActionBarGroupSlot source)
        {
            var popup = (ActionBarSpellsGroup)typeof(ActionBarGroupSlot).GetField("SubGroup", Cleanup2Fields).GetValue(source);
            if (popup == null || !popup.gameObject.activeInHierarchy) throw new InvalidOperationException("Actual alphabetical popup absent.");
            var slots = (System.Collections.IEnumerable)typeof(ActionBarSpellsGroup).GetField("m_Slots", Cleanup2Fields).GetValue(popup);
            ActionBarSlot[] rows = slots.Cast<ActionBarSlot>().Where(value => value != null && value.gameObject.activeInHierarchy).ToArray();
            return new AlphaMenu { Popup = popup, Rows = rows,
                Data = rows.Select(value => value.MechanicSlot?.GetContentData() as AbilityData).ToArray() };
        }

        private static JArray AlphaRowsEvidence(AlphaMenu menu)
        {
            return new JArray(menu.Rows.Select((row, index) => new JObject { ["index"] = index,
                ["label"] = menu.Data[index]?.Name, ["ability"] = menu.Data[index]?.Blueprint.AssetGuid,
                ["internalName"] = menu.Data[index]?.Blueprint.name,
                ["convertedFrom"] = menu.Data[index]?.ConvertedFrom?.Blueprint.AssetGuid,
                ["iconIsOwn"] = row.Icon != null && menu.Data[index] != null && ReferenceEquals(row.Icon.sprite, menu.Data[index].Blueprint.Icon) }));
        }

        private IEnumerable<int> AlphaObserveMenu(string label, ActionBarGroupSlot source, BlueprintAbility parent, AbilityData original,
            Cleanup2NativeDefaultScope mode, Stopwatch watch, NativeIconScreenEvidence screens, Cleanup2TooltipInputs inputs, Action<AlphaMenu> result, bool checkEnds, bool click)
        {
            Cleanup2MenuAdapterBoundary(mode, watch);
            foreach (int frame in Cleanup2AwaitStationaryNativeHud(source, "alpha-" + label)) yield return frame;
            source.OnToggleGroupClick();
            for (int frame = 0; frame < 10; frame++) yield return 0;
            try
            {
                AlphaMenu menu = AlphaReadPopup(source);
                string[] names = menu.Data.Select(value => value?.Name ?? "<null>").ToArray();
                bool[] creature = menu.Data.Select(value => AlphaIsCreatureChoice(value?.Blueprint)).ToArray();
                JObject verdict = AlphaOrderVerdict(names, creature);
                BlueprintAbility[] live = menu.Data.Select(value => value?.Blueprint).ToArray();
                BlueprintAbility[] variants = parent.Variants ?? new BlueprintAbility[0];
                int[] parentPositions = live.Select(value => Array.IndexOf(variants, value)).ToArray();
                bool parentOrder = parentPositions.All(value => value >= 0) && parentPositions.SequenceEqual(parentPositions.OrderBy(value => value));
                var evidence = new JObject { ["menu"] = label, ["parent"] = parent.AssetGuid, ["verdict"] = verdict,
                    ["followsPublishedParentOrder"] = parentOrder, ["duplicateAbilities"] = live.Length - live.Distinct().Count(),
                    ["rows"] = AlphaRowsEvidence(menu), ["inputsUnchanged"] = inputs.Unchanged() };
                Func<bool> owns = () => mode.Current && Cleanup2NoNarrativeOverlay() && inputs.Unchanged() &&
                    Game.Instance.SelectedAbilityHandler.Ability == null && ReferenceEquals(source.MechanicSlot?.GetContentData(), original) && menu.Popup.gameObject.activeInHierarchy;
                bool ordered = (bool)verdict["sorted"] && parentOrder && live.Distinct().Count() == live.Length && inputs.Unchanged();
                Sprint25Check("alpha-menu-" + label, ordered, evidence, "The actual UI row objects follow the alphabetical contract and the published parent order, with no duplicates.");
                result(menu);
                foreach (int frame in screens.Capture("alpha-" + label + "-top", evidence, owns)) yield return frame;
                if (checkEnds)
                {
                    var scroll = Cleanup2AcquirePopupSurface(source, menu.Popup, parent, menu.Rows, label);
                    foreach (ActionBarSlot end in new[] { menu.Rows.First(), menu.Rows.Last() })
                    {
                        foreach (int frame in Cleanup2RevealPopupRow(scroll, end, owns)) yield return frame;
                        bool visible = Cleanup2FullyVisible(scroll, end);
                        string which = ReferenceEquals(end, menu.Rows.First()) ? "first" : "last";
                        var endEvidence = new JObject { ["menu"] = label, ["end"] = which, ["label"] = (end.MechanicSlot.GetContentData() as AbilityData)?.Name,
                            ["fullyVisible"] = visible, ["rowCount"] = menu.Rows.Length, ["scrollStationary"] = scroll.IsStationary,
                            ["navigation"] = "Cleanup2RevealPopupRow; existing scrolling and focus retained" };
                        Sprint25Check("alpha-menu-" + label + "-" + which, visible, endEvidence, "The menu's first and last row can be brought fully into view.");
                        foreach (int frame in screens.Capture("alpha-" + label + "-" + which, endEvidence, owns)) yield return frame;
                    }
                }
                if (click)
                {
                    int target = Array.FindLastIndex(menu.Rows, row => AlphaQuantityRank((row.MechanicSlot.GetContentData() as AbilityData)?.Name ?? "") == 0);
                    if (target >= 0)
                    {
                        var row = menu.Rows[target]; var data = (AbilityData)row.MechanicSlot.GetContentData();
                        bool available = data.IsAvailableForCast;
                        row.OnClick();
                        bool selected = ReferenceEquals(Game.Instance.SelectedAbilityHandler.Ability, data);
                        Sprint25Check("alpha-click-" + label, !available || selected, new JObject { ["menu"] = label, ["rowIndex"] = target, ["label"] = data.Name,
                            ["ability"] = data.Blueprint.AssetGuid, ["available"] = available, ["selectedExactRowObject"] = selected,
                            ["limit"] = available ? "selection only; no cast in this case" : "row unavailable for this fixture caster; selection not exercised" },
                            "Selecting a reordered row selects that row's own ability object.");
                        if (selected) Game.Instance.SelectedAbilityHandler.DropAbility();
                    }
                }
            }
            finally { Cleanup2MenuAdapterHide(source, original); }
        }

        private IEnumerable<int> ReviewSummonAlphabeticalMenus(ExpandedSummoningCorrectionFixture fixture, Cleanup2NativeDefaultScope mode)
        {
            if (!IsSprint32Review || !_request.ExitAfterCompletion || !ReferenceEquals(fixture, _sprint25Fixture) || fixture?.Caster == null ||
                !_context.FeatureModules.Active.ExpandedSummoning || mode == null || !mode.Current || !Cleanup2NoNarrativeOverlay())
                throw new InvalidOperationException("Exact fresh alphabetical-menu fixture with the summoning module enabled required.");
            var game = Game.Instance; var player = game.Player; var ui = game.UI; var group = GroupController.Instance; var bar = ActionBarManager.Instance;
            if (group == null || bar?.Group == null || ui?.SelectionManagerPC == null || game.IsControllerGamepad)
                throw new InvalidOperationException("Actual desktop native action bar required.");
            bool censusPassed;
            JObject census = AlphaCensus(out censusPassed);
            Sprint25Check("alpha-census", censusPassed, census, "Every published canonical and alternate menu array follows the alphabetical contract, and Stirge stays absent.");
            var party = player.Party.ToArray(); var selected = ui.SelectionManagerPC.SelectedUnits.ToArray(); var selectedGroup = group.GetCurrentCharacter();
            var groups = bar.Group.GroupElements.ToArray(); bool[] toggles = groups.Select(value => value.ToggleState).ToArray();
            var addedBooks = new TeleportResourceFixtureOwner(fixture.Caster); var facts = new List<Action>();
            var watch = Stopwatch.StartNew(); bool added = false; NativeIconScreenEvidence screens = null;
            try
            {
                if (toggles.Any(value => value)) throw new InvalidOperationException("Closed native groups required at intake.");
                var sorcerer = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library, "b3a505fb61437dc4097f43c3f8f9a4cf", "native Sorcerer");
                var druidClass = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library, "610d836f3a3a9ed42a4349b62f002e96", "native Druid");
                var known = addedBooks.AddBook(sorcerer.Spellbook);
                Func<SummonFamily, int, BlueprintAbility> canonical = (family, tier) => BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                    ExpandedSummoningPublisher.CanonicalParentGuids[(family == SummonFamily.Monster ? 0 : 9) + tier - 1], "alphabetical canonical parent");
                int mephitTier = Enumerable.Range(1, 9).OrderByDescending(tier => canonical(SummonFamily.Monster, tier).Variants.Count(value =>
                    value.Name.StartsWith("Mephit, ", StringComparison.Ordinal))).ThenBy(tier => tier).First();
                BlueprintAbility mephitMenu = canonical(SummonFamily.Monster, mephitTier);
                int allyTier = Enumerable.Range(1, 9).OrderByDescending(tier => canonical(SummonFamily.NaturesAlly, tier).Variants.Length).ThenBy(tier => tier).First();
                BlueprintAbility allyMenu = canonical(SummonFamily.NaturesAlly, allyTier);
                known.AddKnown(mephitTier, mephitMenu, true); known.AddKnown(allyTier, allyMenu, true); known.Rest();
                var druid = addedBooks.AddBook(druidClass.Spellbook);
                var feature = druidClass.Progression.LevelEntries.SelectMany(value => value.Features).OfType<BlueprintFeature>().Distinct()
                    .Single(value => DruidSummonConversionVariantPatch.ExactNativeFeature(value, druidClass));
                var conversionFact = fixture.Caster.Descriptor.AddFact(feature);
                facts.Add(() => fixture.Caster.Descriptor.RemoveFact(conversionFact));
                const int DruidTier = 5;
                var druidParent = canonical(SummonFamily.NaturesAlly, DruidTier);
                var ordinary = druid.Blueprint.SpellList.GetSpells(DruidTier).Where(value => value != null && !ReferenceEquals(value, druidParent) &&
                    (value.Variants == null || value.Variants.Length == 0)).OrderBy(value => value.AssetGuid, StringComparer.Ordinal).First();
                var druidPrepared = PrepareExpandedSummoningPlayerPathSpell(druid, ordinary, ordinary, DruidTier);
                var ownPrepared = PrepareExpandedSummoningPlayerPathSpell(druid, druidParent, druidParent, DruidTier);
                druid.Rest();
                var alternates = new List<BlueprintAbility>();
                foreach (string familyKey in new[] { AlternateSummoningSourceCatalog.MonsterTacticianKey, AlternateSummoningSourceCatalog.MasterSummonerKey })
                {
                    AlternateSummoningRootSpec rootSpec = AlternateSummoningSourceCatalog.All
                        .Single(value => value.Key == familyKey).Roots.SingleOrDefault(value => value.Tier == 5);
                    BlueprintAbility root = AlphaResolveRoot(rootSpec);
                    if (root == null)
                    {
                        Sprint25Check("alpha-alternate-provider-" + familyKey, true, new JObject { ["family"] = familyKey, ["available"] = false,
                            ["status"] = AlternateSummoningStatusRegistry.Find(familyKey)?.Disposition.ToString() }, "An absent provider stays absent and is reported, not substituted.");
                        continue;
                    }
                    var fact = fixture.Caster.Descriptor.AddFact(root);
                    facts.Add(() => fixture.Caster.Descriptor.RemoveFact(fact));
                    alternates.Add(root);
                }
                if (!player.Party.Contains(fixture.Caster))
                { player.PartyCharacters.Add(fixture.Caster); added = true; player.InvalidateCharacterLists(); player.UpdateCharacterLists(); }
                screens = new NativeIconScreenEvidence(_request);
                ui.SelectionManagerPC.SelectUnit(fixture.Caster.View, true, true, false); group.SelectUnit(fixture.Caster);
                for (int frame = 0; frame < 12; frame++) yield return 0;
                if (!mode.Current) throw new InvalidOperationException("Native Default did not settle after alphabetical book setup.");
                var inputs = new Cleanup2TooltipInputs(fixture.Caster);
                var spellGroup = groups.Single(value => (ActionBarSlotType)typeof(ActionBarGroupElement).GetField("SlotType", Cleanup2Fields).GetValue(value) == ActionBarSlotType.Spell);
                var levels = (ActionBarSubGroupLevels)typeof(ActionBarGroupElement).GetField("m_Levels", Cleanup2Fields).GetValue(spellGroup);
                if (!spellGroup.ToggleState) spellGroup.OnClick();
                spellGroup.Fill(fixture.Caster);
                Func<int, Func<AbilityData, bool>, ActionBarGroupSlot> findSource = (tier, match) => spellGroup.GetComponentsInChildren<ActionBarGroupSlot>(true).Single(value =>
                    value.gameObject.activeInHierarchy && match(value.MechanicSlot?.GetContentData() as AbilityData));

                levels.OnClick(mephitTier);
                for (int frame = 0; frame < 10; frame++) yield return 0;
                var mephitSource = findSource(mephitTier, data => ReferenceEquals(data?.Blueprint, mephitMenu) && ReferenceEquals(data?.Spellbook, known));
                var mephitData = (AbilityData)mephitSource.MechanicSlot.GetContentData();
                AlphaMenu mephits = null;
                foreach (int frame in AlphaObserveMenu("summon-monster-" + mephitTier, mephitSource, mephitMenu, mephitData, mode, watch, screens, inputs, value => mephits = value, false, true)) yield return frame;
                Sprint25Check("alpha-mephit-rows", mephits != null && mephits.Data.Count(value => value.Name.StartsWith("Mephit, ", StringComparison.Ordinal)) > 1,
                    new JObject { ["tier"] = mephitTier, ["mephits"] = mephits == null ? (JToken)JValue.CreateNull() : new JArray(mephits.Data.Where(value => value.Name.StartsWith("Mephit, ", StringComparison.Ordinal)).Select(value => value.Name)) },
                    "The Summon Monster menu shows several family-first mephits adjacent in alphabetical position.");

                levels.OnClick(allyTier);
                for (int frame = 0; frame < 10; frame++) yield return 0;
                var allySource = findSource(allyTier, data => ReferenceEquals(data?.Blueprint, allyMenu) && ReferenceEquals(data?.Spellbook, known));
                var allyData = (AbilityData)allySource.MechanicSlot.GetContentData();
                AlphaMenu allies = null;
                foreach (int frame in AlphaObserveMenu("natures-ally-" + allyTier, allySource, allyMenu, allyData, mode, watch, screens, inputs, value => allies = value, true, false)) yield return frame;
                Sprint25Check("alpha-ally-quantities", allies != null && allies.Data.Any(value => AlphaQuantityRank(value.Name) == 1) && allies.Data.Any(value => AlphaQuantityRank(value.Name) == 2) &&
                    allies.Data.Any(value => AlphaQuantityRank(value.Name) == 0),
                    new JObject { ["tier"] = allyTier, ["rows"] = allies?.Rows.Length ?? 0 }, "The long Nature's Ally menu interleaves single, 1d3 and 1d4+1 rows by creature name.");

                levels.OnClick(DruidTier);
                for (int frame = 0; frame < 10; frame++) yield return 0;
                var druidSource = findSource(DruidTier, data => ReferenceEquals(data, druidPrepared));
                var factory = druidSource.MechanicSlot.GetConvertedAbilityData().ToArray();
                AlphaMenu flat = null;
                foreach (int frame in AlphaObserveMenu("druid-conversion-" + DruidTier, druidSource, druidParent, druidPrepared, mode, watch, screens, inputs, value => flat = value, true, false)) yield return frame;
                bool factoryMatches = flat != null && factory.Length == flat.Data.Length && factory.Select(value => value.Blueprint).SequenceEqual(flat.Data.Select(value => value.Blueprint));
                Sprint25Check("alpha-druid-factory-matches-popup", factoryMatches, new JObject { ["conversionAdapter"] = DruidSummonConversionVariantPatch.Outcome,
                    ["factory"] = new JArray(factory.Select(value => value.Blueprint.AssetGuid)), ["popupRows"] = flat?.Rows.Length ?? 0 },
                    "The native conversion factory and the PC popup list the Druid choices in the same order.");

                var ownSource = findSource(DruidTier, data => ReferenceEquals(data, ownPrepared));
                var ownFactory = ownSource.MechanicSlot.GetConvertedAbilityData().ToArray();
                AlphaMenu own = null;
                foreach (int frame in AlphaObserveMenu("druid-prepared-parent-" + DruidTier, ownSource, druidParent, ownPrepared, mode, watch, screens, inputs, value => own = value, false, false)) yield return frame;
                Sprint25Check("alpha-druid-prepared-parent", own != null && own.Data.Select(value => value.Blueprint).Distinct().Count() == own.Data.Length &&
                    ownFactory.Select(value => value.Blueprint).Distinct().Count() == ownFactory.Length,
                    new JObject { ["popupRows"] = own?.Rows.Length ?? 0, ["factoryRows"] = ownFactory.Length, ["conversionAdapter"] = DruidSummonConversionVariantPatch.Outcome },
                    "A prepared Summon Nature's Ally shows each creature once, in order.");

                // A single row that quantity-first order would have placed
                // before any group row, cast through the existing narrow route.
                var flatRows = flat.Rows; var flatData = flat.Data;
                int lastGroup = Array.FindLastIndex(flatData, value => AlphaQuantityRank(value.Name) != 0);
                int castIndex = Array.FindLastIndex(flatData, value => AlphaQuantityRank(value.Name) == 0 && SummonSignatureReader.Read(value.Blueprint).Spawns().Count() == 1);
                bool reordered = castIndex >= 0 && Array.FindIndex(flatData, value => AlphaQuantityRank(value.Name) != 0) < castIndex && lastGroup >= 0;
                Sprint25Check("alpha-druid-cast-target-reordered", reordered, new JObject { ["castIndex"] = castIndex, ["label"] = castIndex < 0 ? null : flatData[castIndex].Name },
                    "The cast target is a single whose position differs from the retired quantity-first order.");
                if (reordered)
                {
                    foreach (int frame in Cleanup2AwaitStationaryNativeHud(druidSource, "alpha-druid-cast")) yield return frame;
                    druidSource.OnToggleGroupClick();
                    for (int frame = 0; frame < 10; frame++) yield return 0;
                    AlphaMenu reopened = AlphaReadPopup(druidSource);
                    int reopenedIndex = Array.FindIndex(reopened.Data, value => ReferenceEquals(value?.Blueprint, flatData[castIndex].Blueprint));
                    AbilityData selectedRow = reopened.Data[reopenedIndex];
                    ActionBarSlot rowObject = reopened.Rows[reopenedIndex];
                    Func<bool> owns = () => mode.Current && Cleanup2NoNarrativeOverlay() && Game.Instance.SelectedAbilityHandler.Ability == null &&
                        ReferenceEquals(druidSource.MechanicSlot?.GetContentData(), druidPrepared) && reopened.Popup.gameObject.activeInHierarchy;
                    var scroll = Cleanup2DruidPopupSurface(druidSource, reopened.Popup, druidPrepared, selectedRow, owns);
                    foreach (int frame in Cleanup2RevealPopupRow(scroll, rowObject, owns)) yield return frame;
                    var debit = new Cleanup2SourceDebit(fixture.Caster);
                    foreach (int frame in Cleanup2MenuAdapterDruidCast(fixture, selectedRow, druidPrepared, druidParent, rowObject, debit, mode, watch,
                        () => Cleanup2MenuAdapterHide(druidSource, druidPrepared), "alphabetical-reordered")) yield return frame;
                }

                foreach (BlueprintAbility root in alternates)
                {
                    var abilityGroups = groups.Where(value => (ActionBarSlotType)typeof(ActionBarGroupElement).GetField("SlotType", Cleanup2Fields).GetValue(value) == ActionBarSlotType.ActivatableAbility).ToArray();
                    foreach (ActionBarGroupElement element in abilityGroups) { element.Fill(fixture.Caster); if (!element.ToggleState) element.OnClick(); }
                    for (int frame = 0; frame < 16; frame++) yield return 0;
                    var sources = abilityGroups.SelectMany(value => value.GetComponentsInChildren<ActionBarGroupSlot>(true)).Where(value => value.gameObject.activeInHierarchy &&
                        ReferenceEquals((value.MechanicSlot?.GetContentData() as AbilityData)?.Blueprint, root)).ToArray();
                    Sprint25Check("alpha-alternate-slot-" + root.name, sources.Length == 1, new JObject { ["root"] = root.AssetGuid, ["slots"] = sources.Length },
                        "The granted alternate root appears once on the action bar.");
                    if (sources.Length != 1) continue;
                    var data = (AbilityData)sources[0].MechanicSlot.GetContentData();
                    AlphaMenu alternate = null;
                    foreach (int frame in AlphaObserveMenu("alternate-" + root.name, sources[0], root, data, mode, watch, screens, inputs, value => alternate = value, false, true)) yield return frame;
                }
                Sprint25Check("alpha-surface-limits", true, new JObject {
                    ["openedInGame"] = "Known-book Summon Monster and Nature's Ally popups, Druid conversion and prepared parent, granted alternate roots.",
                    ["structuralOnly"] = "Other tiers and families in the census were not opened as popups; console navigation was not exercised.",
                    ["fixtureAccess"] = "Books and root facts were granted to the disposable caster; not a natural character or saved hotbar.", ["wallSeconds"] = watch.Elapsed.TotalSeconds },
                    "Structural and opened surfaces are reported separately.");
            }
            finally
            {
                var errors = new List<string>();
                Action<string, Action> restore = (name, action) => { try { action(); } catch (Exception error) { errors.Add(name + ":" + error.Message); } };
                for (int index = facts.Count - 1; index >= 0; index--) restore("fact-" + index, facts[index]);
                restore("books", () => addedBooks.Restore());
                restore("party", () => { if (added) { player.PartyCharacters.Remove(fixture.Caster); player.InvalidateCharacterLists(); player.UpdateCharacterLists(); } });
                restore("selection", () => { group.SelectUnit(selectedGroup); ui.SelectionManagerPC.MultiSelect(selected.Where(value => value?.View != null).Select(value => value.View).ToArray(), false); });
                for (int index = 0; index < groups.Length; index++) { int scoped = index; restore("toggle-" + scoped, () => groups[scoped].Toggle(toggles[scoped], true)); }
                restore("screens", () => screens?.Dispose());
                Sprint25Check("alpha-owned-state-restored", errors.Count == 0 && player.Party.SequenceEqual(party), new JObject { ["errors"] = new JArray(errors) },
                    "Only the disposable caster's owned books, facts and UI selection were changed, and they are restored.");
            }
        }
    }
}
