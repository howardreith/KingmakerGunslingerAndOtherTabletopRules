using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Kingmaker.Assets.UI.LevelUp;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UI._ConsoleUI.CharGen.Phases.Spells;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Class.LevelUp;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            private readonly List<Func<bool>> _reads = new List<Func<bool>>();
            private readonly HashSet<BlueprintSpellList> _lists = new HashSet<BlueprintSpellList>();
            internal void Ability(BlueprintAbility ability)
            {
                BlueprintAbility parent = ability.Parent;
                _reads.Add(() => ReferenceEquals(ability.Parent, parent));
                BlueprintComponent[] components = ability.ComponentsArray;
                BlueprintComponent[] order = components == null ? null : components.ToArray();
                _reads.Add(() => ReferenceEquals(ability.ComponentsArray, components) &&
                    (order == null || Cleanup2SameElementalRefs(order, ability.ComponentsArray)));
                foreach (AbilityVariants variants in (components ?? new BlueprintComponent[0]).OfType<AbilityVariants>())
                {
                    BlueprintAbility[] raw = variants.Variants;
                    BlueprintAbility[] copy = raw == null ? null : raw.ToArray();
                    _reads.Add(() => ReferenceEquals(variants.Variants, raw) &&
                        (copy == null || Cleanup2SameElementalRefs(copy, variants.Variants)));
                }
            }
            internal void List(BlueprintSpellList list)
            {
                if (!_lists.Add(list)) return;
                SpellLevelList[] levels = list.SpellsByLevel;
                SpellLevelList[] order = levels == null ? null : levels.ToArray();
                _reads.Add(() => ReferenceEquals(list.SpellsByLevel, levels) &&
                    (order == null || Cleanup2SameElementalRefs(order, list.SpellsByLevel)));
                foreach (SpellLevelList level in levels ?? new SpellLevelList[0])
                {
                    if (level == null) continue;
                    List<BlueprintAbility> raw = level.Spells;
                    BlueprintAbility[] copy = raw == null ? null : raw.ToArray();
                    int number = level.SpellLevel;
                    _reads.Add(() => level.SpellLevel == number && ReferenceEquals(level.Spells, raw) &&
                        (copy == null || Cleanup2SameElementalRefs(copy, level.Spells)));
                }
            }
            internal void Cache(BlueprintSpellList list, int level, List<BlueprintAbility> cache)
            {
                BlueprintAbility[] order = cache.ToArray();
                _reads.Add(() => ReferenceEquals(list.GetSpells(level), cache) &&
                    Cleanup2SameElementalRefs(order, cache));
            }
            internal bool Unchanged()
            {
                try { return _reads.All(read => read()); }
                catch (Exception) { return false; }
            }
        }
    }
}
