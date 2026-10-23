using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker;
using Kingmaker.Assets.UI.LevelUp;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.GameModes;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UI.ActionBar;
using Kingmaker.UI.Group;
using Kingmaker.UI.ServiceWindow;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UI._ConsoleUI.CharGen.Phases.Spells;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Mechanics.Actions;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        /// <summary>
        /// Read-only, world-free extension of the existing guarded module boundary
        /// observer. No selection/book/unit/save/UI is created. GetSpells may
        /// perform its own native lazy cache reads; this code never writes one.
        /// </summary>
        private List<RuntimeTestAssertion> ObserveSummonCleanup2ModuleOff()
        {
            if (_request.Scenario != "observe-expanded-summoning-module-boundary" ||
                (bool)_request.Parameters["expandedSummoning"] || _context.FeatureModules.Active.ExpandedSummoning)
                throw new InvalidOperationException("Exact existing module-OFF boundary request required.");
            const string file = "summon-cleanup2-module-off.json";
            string path = Path.Combine(_request.EvidenceDirectory, file);
            var assertions = new List<RuntimeTestAssertion>();
            var records = new JArray();
            var definitions = new JArray();
            var memberships = new JArray();
            var parentLists = new JArray();
            var specialParentLists = new JArray();
            var document = new JObject {
                ["schemaVersion"] = 1, ["runId"] = _request.RunId, ["scenario"] = _request.Scenario,
                ["expectedExpandedSummoning"] = false,
                ["actualExpandedSummoning"] = _context.FeatureModules.Active.ExpandedSummoning,
                ["helperModuleActive"] = ExpandedSummoningVisualVariantPatch.IsModuleActive,
                ["blueprintBootstrapInitialized"] = BlueprintBootstrap.IsInitialized,
                ["scope"] = "Actual loaded native list/definition/helper observation only. No visible learning UI, natural progression, world, cold-save or serialized-reference claim.",
                ["definitions"] = definitions, ["rawMemberships"] = memberships,
                ["ordinaryParentMembershipsAndExceptions"] = parentLists,
                ["specialParentMembershipsAndExceptions"] = specialParentLists, ["checks"] = records
            };
            var surfaces = new Cleanup2ModuleOffSurfaces();
            int probes = 0, positiveParents = 0, positiveHelperParentRows = 0;
            try
            {
                MethodInfo pc = typeof(CharBSelectorLayer).GetMethod("FillSpellLevel", new[] {
                    typeof(SpellSelectionData), typeof(int), typeof(bool), typeof(CharBFeatureSelector) });
                MethodInfo console = typeof(CharGenSpellsPhaseVM).GetMethod("GetSpellsCollection",
                    BindingFlags.Instance | BindingFlags.NonPublic, null,
                    new[] { typeof(LevelUpController), typeof(BlueprintSpellList) }, null);
                bool pcOwner = Cleanup2ElementalOwnTranspiler(pc, typeof(StandaloneElementalPcLearningPatch));
                bool consoleOwner = Cleanup2ElementalOwnTranspiler(console, typeof(StandaloneElementalConsoleLearningPatch));
                Cleanup2ModuleOffCheck(assertions, records, "learning-hooks", pcOwner && consoleOwner &&
                    !ExpandedSummoningVisualVariantPatch.IsModuleActive &&
                    StandaloneElementalDiscovery.PcSeamState == "installed-exact-learning-call-copy" &&
                    StandaloneElementalDiscovery.ConsoleSeamState == "installed-exact-learning-call-copy",
                    new JObject { ["pcMethodResolved"] = pc != null, ["consoleMethodResolved"] = console != null,
                        ["pcOwnedTranspiler"] = pcOwner, ["consoleOwnedTranspiler"] = consoleOwner,
                        ["pcState"] = StandaloneElementalDiscovery.PcSeamState,
                        ["consoleState"] = StandaloneElementalDiscovery.ConsoleSeamState,
                        ["actualHelperModuleActive"] = ExpandedSummoningVisualVariantPatch.IsModuleActive,
                        ["gameMvid"] = typeof(BlueprintAbility).Module.ModuleVersionId.ToString() },
                    "Exact installed PC/console hooks are observed while the actual module helper guard is OFF.", file);

                StandaloneElementalAbilitySpec[] specs = StandaloneElementalPresentationPolicy.All.ToArray();
                StandaloneElementalAbilitySpec[] parents = specs.Where(value => value.IsParent).ToArray();
                var live = new Dictionary<string, BlueprintAbility>(StringComparer.Ordinal);
                foreach (StandaloneElementalAbilitySpec spec in specs)
                {
                    var row = new JObject { ["guid"] = spec.Guid, ["catalogName"] = spec.InternalName,
                        ["catalogParent"] = spec.ParentGuid, ["canonicalTier"] = spec.CanonicalTier,
                        ["kind"] = spec.UniqueNpc ? "unique-wriggling-man" : spec.IsParent ? "standalone-parent" : "standalone-child" };
                    bool resolved = false;
                    try
                    {
                        BlueprintAbility ability = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                            BlueprintBootstrap.Library, spec.Guid, "Cleanup 2 module-off exact standalone elemental definition");
                        live.Add(spec.Guid, ability); surfaces.Ability(ability);
                        resolved = ability.AssetGuid == spec.Guid;
                        row["name"] = ability.name; row["reference"] = Cleanup2ModuleOffReference(ability);
                        row["actualParent"] = ability.Parent == null ? null : ability.Parent.AssetGuid;
                        row["actualParentReference"] = Cleanup2ModuleOffReference(ability.Parent);
                        row["componentArrayReference"] = Cleanup2ModuleOffReference(ability.ComponentsArray);
                        row["components"] = new JArray((ability.ComponentsArray ?? new BlueprintComponent[0]).Select(
                            component => new JObject { ["reference"] = Cleanup2ModuleOffReference(component),
                                ["type"] = component == null ? null : component.GetType().FullName }));
                        row["variants"] = new JArray((ability.ComponentsArray ?? new BlueprintComponent[0]).OfType<AbilityVariants>()
                            .Select(menu => new JObject { ["componentReference"] = Cleanup2ModuleOffReference(menu),
                                ["arrayReference"] = Cleanup2ModuleOffReference(menu.Variants),
                                ["rows"] = Cleanup2ModuleOffAbilityRows(menu.Variants) }));
                    }
                    catch (Exception exception) { row["exception"] = Cleanup2ElementalBound(exception.Message, 1200); }
                    row["resolved"] = resolved; definitions.Add(row);
                    Cleanup2ModuleOffCheck(assertions, records, "definition-" + spec.Guid, resolved,
                        new JObject { ["guid"] = spec.Guid, ["resolved"] = resolved },
                        "Exact native parent, child or unique Wriggling Man definition resolves without constructing a replacement.", file);
                }

                BlueprintSpellList[] allLists = BlueprintBootstrap.Library.BlueprintsByAssetId.Values
                    .OfType<BlueprintSpellList>().Distinct().OrderBy(value => value.AssetGuid, StringComparer.Ordinal).ToArray();
                var catalog = new HashSet<string>(specs.Select(value => value.Guid), StringComparer.Ordinal);
                foreach (BlueprintSpellList list in allLists)
                {
                    bool consumer = false;
                    SpellLevelList[] levels = list.SpellsByLevel;
                    for (int index = 0; index < (levels == null ? 0 : levels.Length); index++)
                    {
                        SpellLevelList level = levels[index];
                        List<BlueprintAbility> raw = level == null ? null : level.Spells;
                        for (int ordinal = 0; ordinal < (raw == null ? 0 : raw.Count); ordinal++)
                        {
                            BlueprintAbility ability = raw[ordinal];
                            if (ability == null || !catalog.Contains(ability.AssetGuid)) continue;
                            consumer = true;
                            StandaloneElementalAbilitySpec spec = StandaloneElementalPresentationPolicy.Find(ability.AssetGuid);
                            BlueprintAbility exact;
                            memberships.Add(new JObject { ["list"] = list.AssetGuid, ["listName"] = list.name,
                                ["ordinaryList"] = StandaloneElementalPresentationPolicy.IsObservedOrdinaryList(list.AssetGuid),
                                ["rawLevelIndex"] = index, ["level"] = level.SpellLevel, ["rawOrdinal"] = ordinal,
                                ["rawArrayReference"] = Cleanup2ModuleOffReference(levels),
                                ["rawListReference"] = Cleanup2ModuleOffReference(raw),
                                ["ability"] = ability.AssetGuid, ["abilityReference"] = Cleanup2ModuleOffReference(ability),
                                ["sameResolvedDefinition"] = live.TryGetValue(ability.AssetGuid, out exact) && ReferenceEquals(ability, exact),
                                ["disposition"] = spec.UniqueNpc ? "preserve-unique-wriggling-man" :
                                    !spec.IsParent ? "preserve-independently-listed-child" :
                                    StandaloneElementalPresentationPolicy.IsObservedOrdinaryList(list.AssetGuid) ?
                                    "preserve-ordinary-parent-module-off" : "preserve-special-list-parent" });
                        }
                    }
                    if (consumer) surfaces.List(list);
                }

                // Special consumers are discovered from their actual raw
                // catalog memberships, not inferred from an ordinary list.
                // Their missing parent/level pairs are observations, never a
                // requirement that a domain/NPC/item list contain all six.
                foreach (BlueprintSpellList list in allLists.Where(value =>
                    !StandaloneElementalPresentationPolicy.IsObservedOrdinaryList(value.AssetGuid) &&
                    memberships.OfType<JObject>().Any(row => (string)row["list"] == value.AssetGuid)))
                foreach (StandaloneElementalAbilitySpec spec in parents)
                {
                    int count = memberships.OfType<JObject>().Count(row =>
                        (string)row["list"] == list.AssetGuid && (string)row["ability"] == spec.Guid &&
                        (int)row["level"] == spec.CanonicalTier && (int)row["rawLevelIndex"] == spec.CanonicalTier);
                    specialParentLists.Add(new JObject { ["list"] = list.AssetGuid, ["listName"] = list.name,
                        ["parent"] = spec.Guid, ["level"] = spec.CanonicalTier,
                        ["actualRawMemberships"] = count,
                        ["disposition"] = count == 0 ? "explicit-special-consumer-list-level-absence" :
                            "preserve-observed-special-parent-membership" });
                }

                string[] ordinaryGuids = StandaloneElementalPresentationPolicy.ObservedOrdinarySpellListGuids.ToArray();
                int listsResolved = 0;
                foreach (string guid in ordinaryGuids)
                {
                    BlueprintSpellList list;
                    try { list = BlueprintLibraryLookup.RequireExact<BlueprintSpellList>(BlueprintBootstrap.Library,
                        guid, "Cleanup 2 module-off observed ordinary spell list"); }
                    catch (Exception exception)
                    {
                        Cleanup2ModuleOffCheck(assertions, records, "list-" + guid, false,
                            new JObject { ["list"] = guid, ["exception"] = Cleanup2ElementalBound(exception.Message, 1200) },
                            "Each exact observed ordinary native spell list must resolve.", file);
                        continue;
                    }
                    listsResolved++; surfaces.List(list);
                    foreach (StandaloneElementalAbilitySpec spec in parents)
                    {
                        JObject[] raw = memberships.OfType<JObject>().Where(row =>
                            (string)row["list"] == guid && (string)row["ability"] == spec.Guid &&
                            (int)row["level"] == spec.CanonicalTier &&
                            (int)row["rawLevelIndex"] == spec.CanonicalTier).ToArray();
                        positiveParents += raw.Length;
                        parentLists.Add(new JObject { ["list"] = guid, ["listName"] = list.name,
                            ["parent"] = spec.Guid, ["level"] = spec.CanonicalTier,
                            ["actualRawMemberships"] = raw.Length,
                            ["disposition"] = raw.Length == 0 ? "explicit-native-list-level-absence" : "observed-native-parent-membership" });
                    }
                    // Actual installed GetSpells uses an array index, not the
                    // SpellLevel field. Out-of-range catalog tiers still get
                    // the native EmptyAbilitiesList and an explicit absence.
                    int[] levels = Enumerable.Range(0, (list.SpellsByLevel ?? new SpellLevelList[0]).Length)
                        .Concat(parents.Select(value => value.CanonicalTier))
                        .Distinct().OrderBy(value => value).ToArray();
                    foreach (int level in levels)
                    {
                        var row = new JObject { ["list"] = guid, ["listName"] = list.name, ["level"] = level,
                            ["selection"] = JValue.CreateNull(), ["book"] = JValue.CreateNull() };
                        bool copied = false;
                        try
                        {
                            List<BlueprintAbility> rows = list.GetSpells(level);
                            BlueprintAbility[] order = rows.ToArray();
                            SpellLevelList rawLevel = level < list.SpellsByLevel.Length ? list.SpellsByLevel[level] : null;
                            row["rawLevelObjectReference"] = Cleanup2ModuleOffReference(rawLevel);
                            row["rawDeclaredLevel"] = rawLevel == null ? (JToken)JValue.CreateNull() : new JValue(rawLevel.SpellLevel);
                            row["rawListReference"] = Cleanup2ModuleOffReference(rawLevel == null ? null : rawLevel.Spells);
                            row["nativeOutOfRangeEmptyList"] = level >= list.SpellsByLevel.Length;
                            surfaces.Cache(list, level, rows);
                            List<BlueprintAbility> first = StandaloneElementalDiscovery.FilterLearningSpells(rows, list, level, null, null);
                            List<BlueprintAbility> second = StandaloneElementalDiscovery.FilterLearningSpells(rows, list, level, null, null);
                            copied = !ReferenceEquals(rows, first) && !ReferenceEquals(rows, second) && !ReferenceEquals(first, second) &&
                                Cleanup2SameElementalRefs(order, first) && Cleanup2SameElementalRefs(order, second) &&
                                Cleanup2SameElementalRefs(order, rows) && ReferenceEquals(list.GetSpells(level), rows);
                            row["sourceReference"] = Cleanup2ModuleOffReference(rows);
                            row["firstCopyReference"] = Cleanup2ModuleOffReference(first);
                            row["secondCopyReference"] = Cleanup2ModuleOffReference(second);
                            row["sourceRows"] = Cleanup2ModuleOffAbilityRows(rows);
                            row["firstCopyRows"] = Cleanup2ModuleOffAbilityRows(first);
                            row["secondCopyRows"] = Cleanup2ModuleOffAbilityRows(second);
                            string[] nativeParents = rows.Where(value => value != null &&
                                StandaloneElementalPresentationPolicy.Find(value.AssetGuid)?.IsParent == true)
                                .Select(value => value.AssetGuid).ToArray();
                            row["actualParentRowsInHelperInput"] = new JArray(nativeParents);
                            positiveHelperParentRows += nativeParents.Length;
                            probes++;
                        }
                        catch (Exception exception) { row["exception"] = Cleanup2ElementalBound(exception.Message, 1200); }
                        row["freshSameOrderAndReferenceCopies"] = copied;
                        Cleanup2ModuleOffCheck(assertions, records, "copy-" + guid + "-" + level, copied, row,
                            "Both actual helper calls return distinct fresh exact-order/reference copies without changing the native source/cache.", file);
                    }
                }
                foreach (StandaloneElementalAbilitySpec parent in parents)
                {
                    int count = parentLists.OfType<JObject>().Where(row => (string)row["parent"] == parent.Guid)
                        .Sum(row => (int)row["actualRawMemberships"]);
                    Cleanup2ModuleOffCheck(assertions, records, "positive-parent-" + parent.Guid, count > 0,
                        new JObject { ["parent"] = parent.Guid, ["positiveOrdinaryRawMemberships"] = count },
                        "Each catalog parent has positive actual ordinary-list membership somewhere; absence on another list remains an individual exception.", file);
                }
                Cleanup2ModuleOffCheck(assertions, records, "summary", ordinaryGuids.Length == 10 &&
                    listsResolved == ordinaryGuids.Length && parents.Length == 6 &&
                    specs.Count(value => !value.IsParent && !value.UniqueNpc) == 24 &&
                    specs.Count(value => value.UniqueNpc) == 1 && live.Count == specs.Length &&
                    probes > 0 && positiveParents > 0 && positiveHelperParentRows > 0,
                    new JObject { ["ordinaryCatalogLists"] = ordinaryGuids.Length, ["actualListsResolved"] = listsResolved,
                        ["parents"] = parents.Length, ["children"] = specs.Count(value => !value.IsParent && !value.UniqueNpc),
                        ["uniqueNpcDefinitions"] = specs.Count(value => value.UniqueNpc), ["definitionsResolved"] = live.Count,
                        ["actualHelperProbes"] = probes, ["positiveParentMemberships"] = positiveParents,
                        ["actualParentRowsInHelperInputs"] = positiveHelperParentRows,
                        ["dispositions"] = new JArray(StandaloneElementalDiscovery.SnapshotDispositions()),
                        ["noVisibleLearningUiClaim"] = true, ["noColdSaveClaim"] = true },
                    "The explicit ten-list/six-parent/24-child/unique-NPC catalog receives nonvacuous actual module-off observation.", file);
            }
            catch (Exception exception)
            {
                Cleanup2ModuleOffCheck(assertions, records, "observer-exception", false,
                    new JObject { ["type"] = exception.GetType().FullName,
                        ["message"] = Cleanup2ElementalBound(exception.Message, 1600) },
                    "Read-only observation must finish without an unclassified native exception.", file);
            }
            bool intact = surfaces.Unchanged();
            bool stillOff = !_context.FeatureModules.Active.ExpandedSummoning && !ExpandedSummoningVisualVariantPatch.IsModuleActive;
            Cleanup2ModuleOffCheck(assertions, records, "shared-surfaces-unchanged", intact && stillOff,
                new JObject { ["sourceCachesRawListsComponentsAndVariantsUnchanged"] = intact,
                    ["actualExpandedSummoning"] = _context.FeatureModules.Active.ExpandedSummoning,
                    ["helperModuleActive"] = ExpandedSummoningVisualVariantPatch.IsModuleActive },
                "All captured native source/cache lists, raw level objects, component arrays and variant references remain exact.", file);
            document["status"] = assertions.All(value => value.Status == "PASS") ? "PASS" : "FAIL";
            RuntimeTestResultWriter.WriteAtomic(path, document.ToString(Formatting.Indented));
            return assertions;
        }

        // Fresh-process acceptance only. This does not turn a live module off,
        // use the enabled registration set, create a character-build screen, or
        // claim save hydration. Native source/preparation objects are granted
        // only to the shared disposable Wizard before observation begins.
        private IEnumerable<int> ReviewSummonCleanup2ElementalOff(ExpandedSummoningCorrectionFixture fixture, Spellbook book)
        {
            if (!IsSummonCleanup2Review || !_request.ExitAfterCompletion || _request.Parameters == null ||
                _request.Parameters.Count != 1 || (string)_request.Parameters["reviewPhase"] != "elementals-off" ||
                _context.FeatureModules.Active.ExpandedSummoning || ExpandedSummoningVisualVariantPatch.IsModuleActive ||
                typeof(BlueprintAbility).Module.ModuleVersionId.ToString() != ElementalSourceChooserRuntime.GameMvid ||
                ElementalSourceChooserRuntime.Active || _expandedSummoningPlayerPathCaptureActive ||
                fixture?.Caster == null || book == null || !ReferenceEquals(book.Owner, fixture.Caster.Descriptor) ||
                book.Blueprint.SpellList?.AssetGuid != "ba0401fdeb4062f40a7aa95b6f07fe89")
                throw new InvalidOperationException("Exact fresh module-OFF save-free native Wizard phase required.");
            var game = Game.Instance; var player = game.Player; var ui = game.UI;
            var group = GroupController.Instance; var bar = ActionBarManager.Instance;
            if (!Cleanup2NoNarrativeOverlay() || game.IsControllerGamepad || group == null || bar?.Group == null ||
                ui?.SelectionManagerPC == null || ui.ServiceWindow == null || ui.ServiceWindow.WindowTabs.IsShow ||
                ui.DescriptionController.DescWindow.gameObject.activeInHierarchy ||
                ui.TooltipsController?.ContextTooltipData != null || !fixture.Caster.Commands.Empty ||
                ElementalSourceChooserView.IsOpen || (_workingSaveSmoke != null && _workingSaveSmoke.WriteObserved))
                throw new InvalidOperationException("Closed idle native desktop surface and no save write required.");
            var party = player.Party.ToArray(); var selection = ui.SelectionManagerPC.SelectedUnits.ToArray();
            var originalGroup = group.GetCurrentCharacter(); var groups = bar.Group.GroupElements.ToArray();
            var toggles = groups.Select(value => value.ToggleState).ToArray();
            if (toggles.Any(value => value)) throw new InvalidOperationException("Closed native action groups required.");
            var originalInputs = party.Select(value => new Cleanup2TooltipInputs(value)).ToArray();
            var originalBars = party.Select(value => new TeleportationUiSettingsFixture(value.UISettings)).ToArray();
            var surfaces = new Cleanup2ModuleOffSurfaces(); var presentationReads = new List<Func<bool>>();
            var targets = new List<Tuple<BlueprintAbility, int, string, BlueprintAbility[]>>();
            var parents = StandaloneElementalPresentationPolicy.All.Where(value => value.IsParent).ToArray();
            var menus = new JArray(); var watch = Stopwatch.StartNew(); bool added = false, selectedFixture = false;
            var hooks = HarmonyInstance.Create("kmg.cleanup2.elementals-off." + _request.RunId);
            Cleanup2NativeDefaultScope mode = null; NativeIconScreenEvidence screens = null;
            ActionBarSlot smallFireRow = null; AbilityData smallFire = null; MechanicActionBarSlot smallFireMechanism = null;
            try
            {
                foreach (var spec in parents)
                {
                    var parent = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library, spec.Guid, "OFF original elemental parent");
                    var children = StandaloneElementalPresentationPolicy.All.Where(value => value.ParentGuid == spec.Guid)
                        .Select(value => BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library, value.Guid, "OFF original elemental child")).ToArray();
                    if (children.Length != 4 || parent.Variants == null || parent.Variants.Length != 4 ||
                        children.Any(child => parent.Variants.Count(value => ReferenceEquals(value, child)) != 1))
                        throw new InvalidOperationException("Original standalone four-child definition changed: " + spec.Guid);
                    targets.Add(Tuple.Create(parent, spec.CanonicalTier, "standalone-" + spec.CanonicalTier, children));
                }
                for (int rank = 1; rank <= 9; rank++)
                {
                    var parent = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                        ExpandedSummoningPublisher.CanonicalParentGuids[9 + rank - 1], "OFF original SNA parent");
                    // Tier I is a direct native spell. Its preservation child
                    // belongs to enabled publication and is not a base route.
                    var children = rank == 1 ? new BlueprintAbility[0] : SummonNativeOptionCatalog.All.Where(value =>
                        value.Family == SummonFamily.NaturesAlly && value.Tier == rank).Select(value =>
                            BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library, value.Guid, "OFF original SNA child")).ToArray();
                    var actual = parent.Variants ?? new BlueprintAbility[0];
                    bool exact = !ExpandedSummoningPublisher.IsPublishedExpandedParent(parent) &&
                        (rank != 1 || actual.Length == 0) && actual.All(child => child != null) &&
                        actual.Distinct().Count() == actual.Length && children.All(child => actual.Count(value => ReferenceEquals(value, child)) == 1);
                    Cleanup2Check("elementals-off-native-sna-definition-" + rank, exact, new JObject {
                        ["parent"] = parent.AssetGuid, ["rank"] = rank, ["directNativeSpell"] = rank == 1,
                        ["actualChildren"] = Cleanup2ModuleOffAbilityRows(actual), ["requiredOriginalNativeChildren"] = Cleanup2ModuleOffAbilityRows(children),
                        ["additionalCurrentStackUnownedChildren"] = Cleanup2ModuleOffAbilityRows(actual.Where(child => !children.Contains(child))),
                        ["expandedPublicationComponentAbsent"] = !ExpandedSummoningPublisher.IsPublishedExpandedParent(parent) },
                        "The fresh OFF process retains the base SNA routes and any additional unowned current-stack children; SNA I is direct and II-IX retain every original native option.");
                    if (!exact) throw new InvalidOperationException("Original SNA definition changed: " + rank);
                    targets.Add(Tuple.Create(parent, rank, "sna-" + rank, actual.ToArray()));
                }
                foreach (var ability in targets.SelectMany(value => new[] { value.Item1 }.Concat(value.Item4)).Distinct())
                {
                    surfaces.Ability(ability); string name = ability.Name; Sprite icon = ability.Icon;
                    presentationReads.Add(() => ability.Name == name && ReferenceEquals(ability.Icon, icon));
                }
                var list = book.Blueprint.SpellList; surfaces.List(list);
                for (int level = 0; level <= 9; level++) surfaces.Cache(list, level, list.GetSpells(level));
                var learning = new JArray();
                foreach (var spec in parents)
                {
                    var rows = list.GetSpells(spec.CanonicalTier); var order = rows.ToArray();
                    var allocation = new SpellSelectionData(book.Blueprint, list); allocation.SetLevelSpells(spec.CanonicalTier, 1);
                    var allocated = allocation.LevelCount[spec.CanonicalTier].SpellSelections;
                    var copy = StandaloneElementalDiscovery.FilterLearningSpells(rows, list, spec.CanonicalTier, allocation, book);
                    bool exact = !ReferenceEquals(copy, rows) && Cleanup2SameElementalRefs(order, copy) &&
                        Cleanup2SameElementalRefs(order, rows) && rows.Any(value => value.AssetGuid == spec.Guid) &&
                        ReferenceEquals(allocated, allocation.LevelCount[spec.CanonicalTier].SpellSelections) &&
                        allocated.Length == 1 && allocated[0] == null && allocation.CanSpendSlot(spec.CanonicalTier, 0);
                    learning.Add(new JObject { ["parent"] = spec.Guid, ["level"] = spec.CanonicalTier,
                        ["actualNativeListRows"] = rows.Count, ["freshSameOrderReferenceCopy"] = exact,
                        ["nativeSelectionBudgetUnspent"] = allocation.CanSpendSlot(spec.CanonicalTier, 0) });
                    if (!exact) throw new InvalidOperationException("OFF learning helper did not preserve native rows: " + spec.Guid);
                }
                Cleanup2Check("elementals-off-learning-source-copy", learning.Count == 6 && surfaces.Unchanged(),
                    new JObject { ["rows"] = learning, ["visibleLearningUi"] = false, ["failingSharedReads"] = surfaces.Failing(),
                        ["scope"] = "Real native Wizard list, book and uncommitted SpellSelectionData; no character-build UI was constructed or natural learning claimed." },
                    "The OFF helper preserves all six original Wizard source rows and exact reference order without spending the native selection allocation.");
                foreach (var target in targets) PrepareExpandedSummoningPlayerPathSpell(book, target.Item1, target.Item1, target.Item2);
                book.Rest();
                var bookSnapshot = new Cleanup2ElementalBookSnapshot(book); var fixturePins = new TeleportationUiSettingsFixture(fixture.Caster.UISettings);
                if (!party.Contains(fixture.Caster))
                { player.PartyCharacters.Add(fixture.Caster); added = true; player.InvalidateCharacterLists(); player.UpdateCharacterLists(); }
                mode = Cleanup2EnterNativeDefault(fixture, "elementals-off"); screens = new NativeIconScreenEvidence(_request);
                // Preserve the exact Default lease. Observer Pause would add
                // a separate native mode; unchanged source snapshots prove
                // browsing/cancel remains free without altering that stack.
                ui.SelectionManagerPC.SelectUnit(fixture.Caster.View, true, true, false); group.SelectUnit(fixture.Caster); selectedFixture = true;
                for (int frame = 0; frame < 12; frame++) yield return 0;
                var spellGroup = groups.Single(value => (ActionBarSlotType)typeof(ActionBarGroupElement).GetField("SlotType", Cleanup2Fields).GetValue(value) == ActionBarSlotType.Spell);
                if (!spellGroup.ToggleState) spellGroup.OnClick();
                for (int frame = 0; frame < 8; frame++) yield return 0;
                foreach (var target in targets)
                {
                    Cleanup2OffBoundary(mode, watch);
                    ((ActionBarSubGroupLevels)typeof(ActionBarGroupElement).GetField("m_Levels", Cleanup2Fields).GetValue(spellGroup)).OnClick(target.Item2);
                    for (int frame = 0; frame < 5; frame++) yield return 0;
                    var source = spellGroup.GetComponentsInChildren<ActionBarGroupSlot>(true).Single(value => value.gameObject.activeInHierarchy &&
                        ReferenceEquals((value.MechanicSlot?.GetContentData() as AbilityData)?.Blueprint, target.Item1) &&
                        ReferenceEquals((value.MechanicSlot.GetContentData() as AbilityData)?.Spellbook, book));
                    var original = (AbilityData)source.MechanicSlot.GetContentData();
                    bool nativeSource = source.MechanicSlot is MechanicActionBarSlotMemorizedSpell && target.Item1.Icon != null &&
                        !string.IsNullOrEmpty(original.Name) &&
                        source.MechanicSlot.GetTitle() == original.Name && ReferenceEquals(source.Icon.sprite, target.Item1.Icon) &&
                        ElementalSourceChooserRuntime.Session(spellGroup) == null && Cleanup2OffNoOwnedPresentation(bar);
                    Cleanup2Check("elementals-off-native-source-" + target.Item3, nativeSource, new JObject {
                        ["parent"] = target.Item1.AssetGuid, ["sourceIdentity"] = Cleanup2ModuleOffReference(original),
                        ["mechanism"] = source.MechanicSlot.GetType().FullName, ["title"] = source.MechanicSlot.GetTitle(),
                        ["nativeTitle"] = target.Item1.Name, ["nativeIconExact"] = ReferenceEquals(source.Icon.sprite, target.Item1.Icon),
                        ["noOwnedChooserOrProjection"] = ElementalSourceChooserRuntime.Session(spellGroup) == null && Cleanup2OffNoOwnedPresentation(bar) },
                        "The actual desktop spell tab retains its native memorized source, title and icon without an owned launcher or projection.");
                    if (!nativeSource) throw new InvalidOperationException("OFF native source presentation changed: " + target.Item3);
                    if (target.Item4.Length == 0)
                    {
                        foreach (int frame in screens.Capture("elementals-off-" + target.Item3, new JObject {
                            ["parent"] = target.Item1.AssetGuid, ["directNativeSpell"] = true, ["moduleEnabled"] = false },
                            () => mode.Current && Cleanup2OffState() && ReferenceEquals(source.MechanicSlot.GetContentData(), original))) yield return frame;
                        continue;
                    }
                    foreach (int frame in Cleanup2AwaitStationaryNativeHud(source, "off-" + target.Item3)) yield return frame;
                    source.OnToggleGroupClick();
                    try
                    {
                        for (int frame = 0; frame < 6; frame++) yield return 0;
                        foreach (int frame in Cleanup2OffPopup(source, original, target.Item4, target.Item3, mode, watch, screens, menus,
                            row => { smallFireRow = row; smallFire = (AbilityData)row.MechanicSlot.GetContentData(); smallFireMechanism = row.MechanicSlot; })) yield return frame;
                    }
                    finally { Cleanup2MenuAdapterHide(source, original); }
                }
                bool unchanged = bookSnapshot.Unchanged(book) && fixturePins.IsRestored() && surfaces.Unchanged() && presentationReads.All(read => read());
                int nativeSnaRows = menus.OfType<JObject>().Count(row => ((string)row["route"]).StartsWith("sna-", StringComparison.Ordinal) &&
                    SummonNativeOptionCatalog.All.Any(spec => spec.Family == SummonFamily.NaturesAlly && spec.Tier > 1 && spec.Guid == (string)row["child"]));
                int expectedRows = targets.Sum(target => target.Item4.Length);
                Cleanup2Check("elementals-off-original-desktop-menus", unchanged && menus.Count == expectedRows && nativeSnaRows == 23,
                    new JObject { ["rows"] = menus, ["standaloneParents"] = parents.Length,
                        ["standaloneChildren"] = menus.OfType<JObject>().Count(row => ((string)row["route"]).StartsWith("standalone-", StringComparison.Ordinal)),
                        ["nativeSnaDirectRoutes"] = 1, ["requiredNativeSnaVariantRows"] = nativeSnaRows,
                        ["additionalCurrentStackSnaRows"] = menus.Count - 24 - nativeSnaRows,
                        ["sourceBookListsComponentsVariantsPresentationAndPinsExact"] = unchanged },
                    "All 24 standalone children and 23 SNA II-IX choices remain original native desktop rows; SNA I retains its direct native source.");
                if (!unchanged || menus.Count != expectedRows || nativeSnaRows != 23 || smallFireRow == null) throw new InvalidOperationException("OFF original menu coverage/source preservation incomplete.");
                foreach (int frame in Cleanup2OffPreparation(fixture, book, targets, mode, watch, screens)) yield return frame;
                if (!bookSnapshot.Unchanged(book)) throw new InvalidOperationException("Native preparation observation changed source spell state.");
                const BindingFlags statics = BindingFlags.Static | BindingFlags.NonPublic;
                hooks.Patch(typeof(RuleCastSpell).GetMethod("OnTrigger"), null,
                    new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("ExpandedSummoningPlayerPathRuleCastPostfix", statics)), null);
                hooks.Patch(typeof(ContextActionSpawnMonster).GetMethod("RunAction"),
                    new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("ExpandedSummoningPlayerPathSpawnPrefix", statics)), null, null);
                _expandedSummoningPlayerPathCaptureActive = true;
                foreach (int frame in Cleanup2OffPinAndCast(fixture, book, spellGroup, smallFire, smallFireMechanism, mode, watch)) yield return frame;
                Cleanup2Check("elementals-off-completion-scope", Cleanup2OffState() && surfaces.Unchanged() &&
                    presentationReads.All(read => read()) && fixturePins.IsRestored() && Cleanup2OffNoOwnedPresentation(bar) &&
                    (_workingSaveSmoke == null || !_workingSaveSmoke.WriteObserved), new JObject {
                        ["moduleEnabled"] = _context.FeatureModules.Active.ExpandedSummoning, ["visibleNativeDesktopMenus"] = true,
                        ["visibleNativePreparationRows"] = true, ["originalSmallFirePinnedAndCast"] = true,
                        ["visibleLearningUi"] = false, ["consoleUi"] = false, ["coldHydration"] = false, ["dynamicOffRollback"] = false,
                        ["wallSeconds"] = watch.Elapsed.TotalSeconds, ["maximumWallSeconds"] = 360,
                        ["fixtureAccess"] = "Explicit original native source grants on one save-free CL20 Wizard, not natural learning or saved-character access." },
                    "Fresh OFF native desktop routes, original child cast/debit/pin and unchanged shared surfaces are observed without toggling Active or requiring enabled blueprints.");
            }
            finally
            {
                var errors = new List<string>();
                Action<string, Action> restore = (name, action) => { try { action(); } catch (Exception error) { errors.Add(name + ":" + error.Message); } };
                try
                {
                    restore("capture-hooks", () => { _expandedSummoningPlayerPathCaptureActive = false; hooks.UnpatchAll(hooks.Id); });
                    bool untouched = !selectedFixture && ReferenceEquals(group.GetCurrentCharacter(), originalGroup) && ui.SelectionManagerPC.SelectedUnits.SequenceEqual(selection);
                    bool owns = ReferenceEquals(GroupController.Instance, group) && ReferenceEquals(ActionBarManager.Instance, bar) &&
                        bar.Group.GroupElements.SequenceEqual(groups) && ui.TooltipsController.ContextTooltipData == null &&
                        game.SelectedAbilityHandler?.Ability == null && !ui.ServiceWindow.WindowTabs.IsShow &&
                        (untouched || ReferenceEquals(group.GetCurrentCharacter(), fixture.Caster) &&
                            ui.SelectionManagerPC.SelectedUnits.Count == 1 && ReferenceEquals(ui.SelectionManagerPC.SelectedUnits.First(), fixture.Caster));
                    if (!owns) errors.Add("Foreign native UI/input owner; restoration refused.");
                    else
                    {
                        if (!untouched) for (int i = 0; i < groups.Length; i++) { int index = i; restore("group-toggle", () => groups[index].Toggle(toggles[index], true)); }
                        if (added) restore("own-party-entry", () => { player.PartyCharacters.Remove(fixture.Caster); player.InvalidateCharacterLists(); player.UpdateCharacterLists(); });
                        if (!untouched)
                        {
                            restore("native-group", () => group.SelectUnit(originalGroup));
                            restore("native-selection", () => ui.SelectionManagerPC.MultiSelect(selection.Where(value => value?.View != null).Select(value => value.View).ToArray(), false));
                        }
                    }
                    bool exact = errors.Count == 0 && player.Party.SequenceEqual(party) && ui.SelectionManagerPC.SelectedUnits.SequenceEqual(selection) &&
                        ReferenceEquals(group.GetCurrentCharacter(), originalGroup) && originalInputs.All(value => value.Unchanged()) &&
                        originalBars.All(value => value.IsRestored()) && surfaces.Unchanged() && presentationReads.All(read => read()) && Cleanup2OffState();
                    Cleanup2Check("elementals-off-owned-ui-restored", exact, new JObject {
                        ["partyExact"] = player.Party.SequenceEqual(party), ["selectionExact"] = ui.SelectionManagerPC.SelectedUnits.SequenceEqual(selection),
                        ["originalInputsAndPinsExact"] = originalInputs.All(value => value.Unchanged()) && originalBars.All(value => value.IsRestored()),
                        ["sharedNativeSourcesExact"] = surfaces.Unchanged() && presentationReads.All(read => read()), ["failingSharedReads"] = surfaces.Failing(),
                        ["failingPresentationReads"] = presentationReads.Count(read => !read()), ["moduleStillOff"] = Cleanup2OffState(),
                        ["errors"] = new JArray(errors) }, "Only owned fixture UI/pin state is restored; actual OFF configuration and all original party/shared source objects remain exact.");
                    if (!exact) errors.Add("Original UI/shared source restoration unverified.");
                }
                finally { try { screens?.Dispose(); } finally { mode?.Dispose(); } }
                if (errors.Count != 0) throw new InvalidOperationException(string.Join("|", errors.ToArray()));
            }
        }

        private IEnumerable<int> Cleanup2OffPopup(ActionBarGroupSlot source, AbilityData original,
            BlueprintAbility[] expected, string route, Cleanup2NativeDefaultScope mode, Stopwatch watch,
            NativeIconScreenEvidence screens, JArray evidence, Action<ActionBarSlot> smallFire)
        {
            Cleanup2OffBoundary(mode, watch);
            var popup = (ActionBarSpellsGroup)typeof(ActionBarGroupSlot).GetField("SubGroup", Cleanup2Fields).GetValue(source);
            if (popup == null || !popup.gameObject.activeInHierarchy) throw new InvalidOperationException("OFF native popup absent: " + route);
            var rows = popup.GetComponentsInChildren<ActionBarSlot>(true).Where(value => value.gameObject.activeInHierarchy &&
                ReferenceEquals((value.MechanicSlot?.GetContentData() as AbilityData)?.ConvertedFrom, original)).ToArray();
            if (rows.Length != expected.Length || expected.Any(child => rows.Count(value =>
                ReferenceEquals((value.MechanicSlot.GetContentData() as AbilityData)?.Blueprint, child)) != 1))
                throw new InvalidOperationException("OFF native popup child set changed: " + route);
            var scroll = Cleanup2AcquireOffPopupSurface(source, popup, original.Blueprint, rows, "off-" + route);
            Vector2 position = scroll.IsStationary ? Vector2.zero : scroll.Position;
            Vector2 velocity = scroll.IsStationary ? Vector2.zero : scroll.Velocity;
            Vector2 anchored = scroll.Content.anchoredPosition; bool horizontal = scroll.Horizontal, vertical = scroll.Vertical;
            Func<bool> owns = () => mode.Current && Cleanup2OffState() && watch.Elapsed.TotalSeconds <= 360 &&
                Cleanup2NoNarrativeOverlay() && !ElementalSourceChooserView.IsOpen && popup.gameObject.activeInHierarchy &&
                Game.Instance.SelectedAbilityHandler.Ability == null && ReferenceEquals(source.MechanicSlot.GetContentData(), original) &&
                scroll.Active && scroll.Horizontal == horizontal && scroll.Vertical == vertical && Cleanup2OffNoOwnedPresentation(popup);
            try
            {
                foreach (var row in rows)
                {
                    foreach (int frame in Cleanup2RevealPopupRow(scroll, row, owns)) yield return frame;
                    var data = (AbilityData)row.MechanicSlot.GetContentData();
                    bool exact = owns() && Cleanup2FullyVisible(scroll, row) && data.Blueprint.Icon != null && !string.IsNullOrEmpty(data.Name) && ReferenceEquals(data.ConvertedFrom, original) &&
                        ReferenceEquals(data.Spellbook, original.Spellbook) && data.Name == data.Blueprint.Name &&
                        row.MechanicSlot.GetTitle() == data.Name && ReferenceEquals(row.Icon.sprite, data.Blueprint.Icon) &&
                        ReferenceEquals(row.MechanicSlot.GetIcon(), data.Blueprint.Icon);
                    var record = new JObject { ["route"] = route, ["parent"] = original.Blueprint.AssetGuid,
                        ["child"] = data.Blueprint.AssetGuid, ["sourceIdentity"] = Cleanup2ModuleOffReference(original),
                        ["childIdentity"] = Cleanup2ModuleOffReference(data), ["book"] = data.Spellbook?.Blueprint.AssetGuid,
                        ["title"] = row.MechanicSlot.GetTitle(), ["blueprintTitle"] = data.Blueprint.Name,
                        ["renderedText"] = new JArray(row.GetComponentsInChildren<TextMeshProUGUI>(true).Select(value => value.text)),
                        ["nativeIconExact"] = ReferenceEquals(row.Icon.sprite, data.Blueprint.Icon), ["fullyVisible"] = Cleanup2FullyVisible(scroll, row),
                        ["nativeSourceLinksExact"] = ReferenceEquals(data.ConvertedFrom, original), ["noOwnedChooserOrProjection"] = Cleanup2OffNoOwnedPresentation(popup) };
                    evidence.Add(record);
                    Cleanup2Check("elementals-off-native-choice-" + data.Blueprint.AssetGuid, exact, record,
                        "The actual visible native OFF child retains its original blueprint title/icon and exact source/book links, without an owned presentation row.");
                    if (!exact) throw new InvalidOperationException("OFF native child presentation changed: " + data.Blueprint.AssetGuid);
                    if (StandaloneElementalPresentationPolicy.Find(data.Blueprint.AssetGuid)?.CreatureKeys.Contains("small-fire-elemental") == true)
                        smallFire(row);
                    if (ReferenceEquals(row, rows[0]))
                        foreach (int frame in screens.Capture("elementals-off-" + route, record, owns)) yield return frame;
                }
            }
            finally
            {
                if (!scroll.Active || scroll.Horizontal != horizontal || scroll.Vertical != vertical)
                    throw new InvalidOperationException("Foreign OFF popup scroll; restoration refused.");
                if (!scroll.IsStationary)
                { scroll.Velocity = Vector2.zero; Cleanup2SetPopupPosition(scroll, position); scroll.Content.anchoredPosition = anchored; scroll.Velocity = velocity; }
                if (!scroll.Content.anchoredPosition.Equals(anchored) || !scroll.IsStationary && !scroll.Velocity.Equals(velocity))
                    throw new InvalidOperationException("OFF popup scroll restoration unverified.");
            }
        }

        private IEnumerable<int> Cleanup2OffPreparation(ExpandedSummoningCorrectionFixture fixture, Spellbook book,
            List<Tuple<BlueprintAbility, int, string, BlueprintAbility[]>> targets, Cleanup2NativeDefaultScope mode,
            Stopwatch watch, NativeIconScreenEvidence screens)
        {
            Cleanup2OffBoundary(mode, watch);
            var game = Game.Instance; var ui = game.UI; var controller = ui.SpellBookController;
            var modesField = typeof(Game).GetField("m_GameModes", Cleanup2Fields);
            var countsField = typeof(Game).GetField("m_ModesCount", Cleanup2Fields);
            var stack = (Stack<GameMode>)modesField.GetValue(game); var beforeModes = stack.ToArray();
            var beforeCounts = ((int[])countsField.GetValue(game)).ToArray();
            var beforeHandler = game.SelectedAbilityHandler; var beforePointer = game.ClickEventsController;
            GameMode ownedWindow = null; bool opened = false;
            Func<bool> owns = () => ownedWindow != null && Cleanup2OffState() &&
                game.CurrentMode == GameModeType.FullScreenUi && ReferenceEquals(modesField.GetValue(game), stack) &&
                stack.Count == beforeModes.Length + 1 && ReferenceEquals(stack.Peek(), ownedWindow) && stack.Skip(1).SequenceEqual(beforeModes) &&
                ((int[])countsField.GetValue(game)).Select((count, index) => count == beforeCounts[index] +
                    (index == (int)GameModeType.FullScreenUi ? 1 : 0)).All(value => value) &&
                ReferenceEquals(ui.SpellBookController, controller) && ui.ServiceWindow.WindowTabs.IsShow && controller.IsShow &&
                ReferenceEquals(controller.CurrentCharacter, fixture.Caster) && Cleanup2NoNarrativeOverlay();
            try
            {
                if (ui.ServiceWindow.WindowTabs.IsShow) throw new InvalidOperationException("Foreign native service window already open.");
                ui.ServiceWindow.HandleOpenSpellbook(); opened = true;
                if (stack.Count == beforeModes.Length + 1 && stack.Skip(1).SequenceEqual(beforeModes)) ownedWindow = stack.Peek();
                for (int frame = 0; frame < 24; frame++) yield return 0;
                if (!owns()) throw new InvalidOperationException("OFF native spellbook ownership did not settle.");
                var records = new JArray();
                foreach (var target in targets)
                {
                    if (!owns() || watch.Elapsed.TotalSeconds > 360) throw new InvalidOperationException("OFF native preparation window owner/time boundary changed.");
                    SpellItem row = null;
                    foreach (int frame in Cleanup2SourcesBookRow(controller, book, target.Item1, target.Item2, false, value => row = value)) yield return frame;
                    var text = (TextMeshProUGUI)typeof(SpellItem).GetField("m_SpellNameLabel", Cleanup2Fields).GetValue(row);
                    bool exact = owns() && row.gameObject.activeInHierarchy && text != null && !string.IsNullOrEmpty(target.Item1.Name) &&
                        target.Item1.Icon != null && text.text == target.Item1.Name &&
                        ReferenceEquals(row.SpellImage.sprite, target.Item1.Icon) && ReferenceEquals(row.SpellData.Blueprint, target.Item1) &&
                        ReferenceEquals(row.SpellData.Spellbook, book) && Cleanup2OffNoOwnedPresentation(controller.SpellBookView);
                    var record = new JObject { ["route"] = target.Item3, ["parent"] = target.Item1.AssetGuid,
                        ["rowIdentity"] = row.GetInstanceID(), ["sourceDataIdentity"] = Cleanup2ModuleOffReference(row.SpellData),
                        ["nativeTitle"] = target.Item1.Name, ["renderedTitle"] = text?.text,
                        ["nativeIconExact"] = ReferenceEquals(row.SpellImage.sprite, target.Item1.Icon),
                        ["noOwnedDisclosureOrProjection"] = Cleanup2OffNoOwnedPresentation(controller.SpellBookView) };
                    records.Add(record); Cleanup2Check("elementals-off-preparation-" + target.Item3, exact, record,
                        "The actual native preparation screen keeps the original source row, native label/icon and original Spellbook reference without an owned fold.");
                    if (!exact) throw new InvalidOperationException("OFF native preparation source changed: " + target.Item3);
                    if (target.Item3 == "standalone-2" || target.Item3 == "sna-2")
                        foreach (int frame in screens.Capture("elementals-off-preparation-" + target.Item3, record,
                            () => owns() && watch.Elapsed.TotalSeconds <= 360)) yield return frame;
                }
                Cleanup2Check("elementals-off-native-preparation-summary", records.Count == 15 && owns(), new JObject { ["rows"] = records },
                    "All six standalone and nine SNA original sources are reached through the native class/level/page UI without altering preparation.");
            }
            finally
            {
                if (opened)
                {
                    if (!owns()) throw new InvalidOperationException("Foreign OFF spellbook window; close refused.");
                    ui.ServiceWindow.HandleOpenSpellbook();
                    bool exact = stack.SequenceEqual(beforeModes) && ((int[])countsField.GetValue(game)).SequenceEqual(beforeCounts) &&
                        mode.Current && ReferenceEquals(game.SelectedAbilityHandler, beforeHandler) && ReferenceEquals(game.ClickEventsController, beforePointer);
                    Cleanup2Check("elementals-off-native-preparation-mode-restored", exact, new JObject {
                        ["stackCountsAndDefaultInputExact"] = exact, ["manualClockSuspended"] = _sprint25WaitingForUnityAppearance },
                        "The exact native spellbook child mode closes and restores the existing Default stack/counts/input owners.");
                    if (!exact) throw new InvalidOperationException("OFF native spellbook mode restoration unverified.");
                }
            }
        }

        private IEnumerable<int> Cleanup2OffPinAndCast(ExpandedSummoningCorrectionFixture fixture, Spellbook book,
            ActionBarGroupElement spellGroup, AbilityData child, MechanicActionBarSlot mechanism,
            Cleanup2NativeDefaultScope mode, Stopwatch watch)
        {
            Cleanup2OffBoundary(mode, watch);
            if (child == null || mechanism == null || !ReferenceEquals(mechanism.GetContentData(), child) ||
                StandaloneElementalPresentationPolicy.Find(child.Blueprint.AssetGuid)?.CreatureKeys.Contains("small-fire-elemental") != true)
                throw new InvalidOperationException("Exact observed original Small Fire native mechanism required.");
            var parent = child.ConvertedFrom;
            var preparation = RawSlots(book, 2).Single(value => ReferenceEquals(value.Spell, parent) && value.Type == SpellSlotType.Common);
            var settings = fixture.Caster.UISettings; var slots = settings.Slots; var baseline = new TeleportationUiSettingsFixture(settings);
            var indexField = typeof(ActionBarIndexSlot).GetField("m_Index", Cleanup2Fields);
            var destination = ActionBarManager.Instance.GetComponentsInChildren<ActionBarIndexSlot>(true).First(value => value.gameObject.activeInHierarchy &&
                ReferenceEquals(Cleanup2MenuAdapterSelected(value), fixture.Caster) && (int)indexField.GetValue(value) >= 0 &&
                (int)indexField.GetValue(value) < slots.Length && slots[(int)indexField.GetValue(value)] is MechanicActionBarSlotEmpty);
            int index = (int)indexField.GetValue(destination); var previous = slots[index]; bool requested = false;
            try
            {
                requested = true; destination.TryToSet(mechanism); spellGroup.Fill(fixture.Caster);
                for (int frame = 0; frame < 8; frame++) yield return 0;
                bool exact = ReferenceEquals(settings.Slots, slots) && ReferenceEquals(slots[index], mechanism) &&
                    ReferenceEquals(destination.MechanicSlot, mechanism) && ReferenceEquals(mechanism.GetContentData(), child) &&
                    ReferenceEquals(child.ConvertedFrom, parent) && ElementalSourceChooserRuntime.Session(spellGroup) == null;
                Cleanup2Check("elementals-off-original-small-fire-pin", exact, new JObject {
                    ["index"] = index, ["originalChild"] = child.Blueprint.AssetGuid, ["originalParent"] = parent.Blueprint.AssetGuid,
                    ["sameNativeMechanismAfterTabRebuild"] = exact, ["noSaveWrite"] = true, ["coldHydration"] = false },
                    "The original native Small Fire mechanism pins through the rendered native index slot and retains the same child/source after tab reconstruction.");
                if (!exact) throw new InvalidOperationException("OFF original child pin identity changed.");
                JObject cast = null;
                foreach (int frame in Cleanup2SourcesCast(fixture, child, preparation, destination.OnClick, mode, watch,
                    "module-off-small-fire-original-pinned-child", row => cast = (JObject)row.DeepClone())) yield return frame;
                // The reused helper throws on failed native cast/debit before
                // this success receipt can be emitted. Preserve its own row.
                Cleanup2Check("elementals-off-original-small-fire-cast", cast != null && Cleanup2OffState(), cast ?? new JObject(),
                    "The OFF original child reaches native targeting/command, its exact native summon and exactly one original preparation debit; no chooser dispatch or forced command.");
            }
            finally
            {
                if (requested)
                {
                    if (!ReferenceEquals(settings.Slots, slots) || !ReferenceEquals(Cleanup2MenuAdapterSelected(destination), fixture.Caster) ||
                        (!ReferenceEquals(slots[index], mechanism) && !ReferenceEquals(slots[index], previous)) ||
                        (!ReferenceEquals(destination.MechanicSlot, mechanism) && !ReferenceEquals(destination.MechanicSlot, previous)))
                        throw new InvalidOperationException("Foreign OFF pin replacement; restoration refused.");
                    if (ReferenceEquals(slots[index], mechanism)) settings.SetSlot(previous, index);
                    destination.Set(fixture.Caster);
                    Cleanup2Check("elementals-off-original-pin-restored", baseline.IsRestored(), new JObject { ["exactPinArrayAndMetadata"] = baseline.IsRestored() },
                        "Only the owned native Small Fire pin is restored; all prior slot references and metadata remain exact.");
                    if (!baseline.IsRestored()) throw new InvalidOperationException("OFF native pin restoration unverified.");
                }
            }
        }

        private bool Cleanup2OffState()
        { return !_context.FeatureModules.Active.ExpandedSummoning && !ExpandedSummoningVisualVariantPatch.IsModuleActive && !ElementalSourceChooserRuntime.Active; }

        private void Cleanup2OffBoundary(Cleanup2NativeDefaultScope mode, Stopwatch watch)
        {
            if (!Cleanup2OffState() || watch.Elapsed.TotalSeconds > 360 || !mode.Current || !Cleanup2NoNarrativeOverlay() ||
                Game.Instance.IsControllerGamepad || Game.Instance.SelectedAbilityHandler.Ability != null || ElementalSourceChooserView.IsOpen)
                throw new InvalidOperationException("Fresh OFF phase lost its native desktop/input/time boundary.");
        }

        private static bool Cleanup2OffNoOwnedPresentation(Component owner)
        {
            return owner != null && !ElementalSourceChooserView.IsOpen &&
                !owner.GetComponentsInChildren<ElementalNativeSourceFold>(true).Any(value => value.HasFold) &&
                !owner.GetComponentsInChildren<ActionBarSlot>(true).Any(value => value.MechanicSlot is ElementalChooserLauncher || value.MechanicSlot is ElementalConsoleNode) &&
                !owner.GetComponentsInChildren<Transform>(true).Any(value => value.name == "KMG_ElementalSourceDisclosure" || value.name == "KMG_ElementalSourceChooser");
        }

        private static int? Cleanup2ModuleOffReference(object value)
        { return value == null ? (int?)null : RuntimeHelpers.GetHashCode(value); }

        private static JArray Cleanup2ModuleOffAbilityRows(IEnumerable<BlueprintAbility> values)
        {
            return values == null ? new JArray() : new JArray(values.Select((ability, ordinal) => new JObject {
                ["ordinal"] = ordinal, ["guid"] = ability == null ? null : ability.AssetGuid,
                ["reference"] = Cleanup2ModuleOffReference(ability) }));
        }

        private static void Cleanup2ModuleOffCheck(ICollection<RuntimeTestAssertion> assertions, JArray records,
            string id, bool pass, JObject row, string expected, string file)
        {
            row["predicate"] = "cleanup2-module-off-" + id; row["passed"] = pass;
            records.Add(row);
            assertions.Add(Assertion("cleanup2-module-off-" + id, expected,
                row.ToString(Formatting.None), pass, file + "; actual read-only native observation; no visible UI or cold-save claim"));
        }

        private sealed class Cleanup2ModuleOffSurfaces
        {
            // Native41: each read carries its surface label so a changed
            // shared object is named in evidence instead of a bare false.
            private readonly List<KeyValuePair<string, Func<bool>>> _reads = new List<KeyValuePair<string, Func<bool>>>();
            private readonly HashSet<BlueprintSpellList> _lists = new HashSet<BlueprintSpellList>();
            private void Read(string label, Func<bool> read) { _reads.Add(new KeyValuePair<string, Func<bool>>(label, read)); }
            internal void Ability(BlueprintAbility ability)
            {
                BlueprintAbility parent = ability.Parent;
                Read("ability-parent:" + ability.AssetGuid, () => ReferenceEquals(ability.Parent, parent));
                BlueprintComponent[] components = ability.ComponentsArray;
                BlueprintComponent[] order = components == null ? null : components.ToArray();
                Read("ability-components:" + ability.AssetGuid, () => ReferenceEquals(ability.ComponentsArray, components) &&
                    (order == null || Cleanup2SameElementalRefs(order, ability.ComponentsArray)));
                foreach (AbilityVariants variants in (components ?? new BlueprintComponent[0]).OfType<AbilityVariants>())
                {
                    BlueprintAbility[] raw = variants.Variants;
                    BlueprintAbility[] copy = raw == null ? null : raw.ToArray();
                    Read("ability-variants:" + ability.AssetGuid, () => ReferenceEquals(variants.Variants, raw) &&
                        (copy == null || Cleanup2SameElementalRefs(copy, variants.Variants)));
                }
            }
            internal void List(BlueprintSpellList list)
            {
                if (!_lists.Add(list)) return;
                SpellLevelList[] levels = list.SpellsByLevel;
                SpellLevelList[] order = levels == null ? null : levels.ToArray();
                Read("list-levels:" + list.AssetGuid, () => ReferenceEquals(list.SpellsByLevel, levels) &&
                    (order == null || Cleanup2SameElementalRefs(order, list.SpellsByLevel)));
                foreach (SpellLevelList level in levels ?? new SpellLevelList[0])
                {
                    if (level == null) continue;
                    List<BlueprintAbility> raw = level.Spells;
                    BlueprintAbility[] copy = raw == null ? null : raw.ToArray();
                    int number = level.SpellLevel;
                    Read("list-spells:" + list.AssetGuid + ":" + number, () => level.SpellLevel == number && ReferenceEquals(level.Spells, raw) &&
                        (copy == null || Cleanup2SameElementalRefs(copy, level.Spells)));
                }
            }
            internal void Cache(BlueprintSpellList list, int level, List<BlueprintAbility> cache)
            {
                // Native42: KMG's own Magic Circle/Shield Other/Teleportation
                // publishers reset the native m_SpellsFiltered cache, so its
                // instance is legitimately rebuilt; its content is the contract.
                BlueprintAbility[] order = cache.ToArray();
                Read("list-cache:" + list.AssetGuid + ":" + level, () => Cleanup2SameElementalRefs(order, list.GetSpells(level)));
            }
            internal bool Unchanged() { return Failing().Count == 0; }
            internal JArray Failing()
            {
                var failing = new JArray();
                foreach (var read in _reads)
                {
                    bool same;
                    try { same = read.Value(); } catch (Exception) { same = false; }
                    if (!same) failing.Add(read.Key);
                }
                return failing;
            }
        }
    }
}
