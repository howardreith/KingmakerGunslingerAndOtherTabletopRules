using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Assets.UI.LevelUp;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UI._ConsoleUI.CharGen.Phases.Spells;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        /// <summary>
        /// Bounded first diagnostic phase. Every input ability/list/book is a
        /// live native or registered blueprint; selection allocations belong to
        /// this disposable caster only. This does not open a real level-up UI,
        /// commit progression, mutate shared lists or touch saved characters.
        /// </summary>
        private IEnumerable<int> ReviewSummonCleanup2Elementals(ExpandedSummoningCorrectionFixture fixture)
        {
            if (!IsSummonCleanup2Review || !_request.ExitAfterCompletion || _request.Parameters == null ||
                _request.Parameters.Count != 1 || (string)_request.Parameters["reviewPhase"] != "elementals" ||
                fixture == null || fixture.Caster == null || fixture.Caster.Blueprint != fixture.CasterBlueprint ||
                fixture.CasterBlueprint == BlueprintRoot.Instance.DefaultPlayerCharacter ||
                fixture.UnitsBefore.Any(value => ReferenceEquals(value, fixture.Caster)) || _workingSaveSmoke.WriteObserved)
                throw new InvalidOperationException("Exact save-free disposable elemental review fixture required.");

            BlueprintSpellList[] ordinaryLists = fixture.Blueprints.OfType<BlueprintSpellList>()
                .Where(value => StandaloneElementalPresentationPolicy.IsObservedOrdinaryList(value.AssetGuid))
                .OrderBy(value => value.AssetGuid, StringComparer.Ordinal).ToArray();
            StandaloneElementalAbilitySpec[] parents = StandaloneElementalPresentationPolicy.All
                .Where(value => value.IsParent).ToArray();
            var addedBooks = new List<Spellbook>();
            Spellbook[] booksBefore = fixture.Caster.Descriptor.Spellbooks.ToArray();
            var surfaces = new Cleanup2ElementalSurfaces(fixture.Blueprints, ordinaryLists);
            try
            {
                MethodInfo pc = typeof(CharBSelectorLayer).GetMethod("FillSpellLevel", new[] {
                    typeof(SpellSelectionData), typeof(int), typeof(bool), typeof(CharBFeatureSelector) });
                MethodInfo console = typeof(CharGenSpellsPhaseVM).GetMethod("GetSpellsCollection",
                    BindingFlags.Instance | BindingFlags.NonPublic, null,
                    new[] { typeof(LevelUpController), typeof(BlueprintSpellList) }, null);
                bool pcOwner = Cleanup2ElementalOwnTranspiler(pc, typeof(StandaloneElementalPcLearningPatch));
                bool consoleOwner = Cleanup2ElementalOwnTranspiler(console, typeof(StandaloneElementalConsoleLearningPatch));
                Cleanup2Check("elemental-learning-seams", pcOwner && consoleOwner &&
                    StandaloneElementalDiscovery.PcSeamState == "installed-exact-learning-call-copy" &&
                    StandaloneElementalDiscovery.ConsoleSeamState == "installed-exact-learning-call-copy",
                    new JObject {
                        ["pc"] = StandaloneElementalDiscovery.PcSeamState,
                        ["console"] = StandaloneElementalDiscovery.ConsoleSeamState,
                        ["pcOwnedTranspiler"] = pcOwner, ["consoleOwnedTranspiler"] = consoleOwner,
                        ["gameMvid"] = typeof(BlueprintAbility).Module.ModuleVersionId.ToString(),
                        ["pcBytes"] = pc?.GetMethodBody()?.GetILAsByteArray().Length,
                        ["consoleBytes"] = console?.GetMethodBody()?.GetILAsByteArray().Length,
                        ["consoleSelectionField"] = typeof(CharGenSpellsPhaseVM).GetField("m_SelectionData", Cleanup2Fields)?.FieldType.FullName,
                        ["consolePreviewBookField"] = typeof(CharGenSpellsPhaseVM).GetField("PreviewSpellbook", Cleanup2Fields)?.FieldType.FullName,
                        ["scope"] = "Actual installed transpiler ownership and verified native fields; no visible learning UI or controller-input claim."
                    }, "Both exact installed native learning methods have this module's scoped transpiler and verified contract state.");

                JArray contracts = Cleanup2ElementalContracts(fixture);
                Cleanup2Check("elemental-live-contract-census", contracts.Count == 60 &&
                    contracts.Cast<JObject>().All(value => value["observed"]?.Value<bool>() == true),
                    new JObject { ["pairs"] = contracts,
                        ["equalPairs"] = contracts.Cast<JObject>().Count(value => value["equivalent"]?.Value<bool>() == true),
                        ["scope"] = "Twelve actual parent and forty-eight native/canonical child comparisons. A complete diagnostic census is not proof that any pair is equivalent or that MENU-02 is complete." },
                    "Every exact native source and both canonical families receive an observed live verdict, including all rejected fields.");
                yield return 0;

                int probes = 0, removed = 0;
                foreach (BlueprintSpellList list in ordinaryLists)
                {
                    BlueprintSpellbook[] candidates = fixture.Blueprints.OfType<BlueprintSpellbook>()
                        .Where(value => ReferenceEquals(value.SpellList, list) && value.CharacterClass != null &&
                            value.SpellsPerDay != null && value.SpellsPerDay.Levels != null && value.SpellsPerDay.Levels.Length >= 21)
                        .OrderBy(value => value.AssetGuid, StringComparer.Ordinal).ToArray();
                    Spellbook book = fixture.Caster.Descriptor.Spellbooks.FirstOrDefault(value =>
                        ReferenceEquals(value.Blueprint.SpellList, list));
                    if (book == null && candidates.Length > 0)
                    {
                        book = fixture.Caster.Descriptor.DemandSpellbook(candidates[0]);
                        addedBooks.Add(book); // Record ownership before native setup can throw.
                        // The native getter clamps to the daily table length.
                        // Never spin if a modified book cannot reach CL20.
                        for (int increment = 0; book.CasterLevel < 20 && increment < 20; increment++) book.AddCasterLevel();
                        if (book.CasterLevel != 20)
                            throw new InvalidOperationException("The bounded native fixture book did not reach CL20: " + book.Blueprint.AssetGuid);
                        book.UpdateAllSlotsSize(false);
                    }
                    foreach (StandaloneElementalAbilitySpec spec in parents)
                    {
                        int level = spec.CanonicalTier;
                        List<BlueprintAbility> rows = list.GetSpells(level);
                        BlueprintAbility[] sources = rows.Where(value => value != null && value.AssetGuid == spec.Guid).ToArray();
                        var row = new JObject {
                            ["list"] = list.AssetGuid, ["listName"] = list.name, ["level"] = level,
                            ["source"] = spec.Guid, ["sourceMembershipCount"] = sources.Length,
                            ["actualBook"] = book?.Blueprint.AssetGuid, ["bookCandidateCount"] = candidates.Length,
                            ["casterLevel"] = book == null ? -1 : book.CasterLevel,
                            ["maxSpellLevel"] = book == null ? -1 : book.MaxSpellLevel,
                            ["scope"] = "Actual attached disposable native book/list and actual SpellSelectionData allocation; no natural class-level progression or visible UI claim."
                        };
                        if (sources.Length != 1 || book == null)
                        {
                            row["disposition"] = sources.Length == 0 ? "source-not-on-this-list-level" :
                                book == null ? "preserve-no-actual-book-for-this-list" : "preserve-ambiguous-source-membership";
                            Cleanup2Check("elemental-list-exception-" + list.AssetGuid + "-" + level,
                                surfaces.Unchanged(), row,
                                "Absent membership and lists without an exact real book remain explicit exceptions without constructed replacement blueprints.");
                            continue;
                        }
                        BlueprintAbility source = sources[0];
                        var selection = new SpellSelectionData(book.Blueprint, list);
                        selection.SetLevelSpells(level, 1);
                        bool knownBefore = book.IsKnown(source);
                        var bookSnapshot = new Cleanup2ElementalBookSnapshot(book);
                        var barSnapshot = new TeleportationUiSettingsFixture(fixture.Caster.UISettings);
                        object state = selection.LevelCount[level];
                        BlueprintAbility[] selectedArray = selection.LevelCount[level].SpellSelections;
                        BlueprintAbility[] originalOrder = rows.ToArray();
                        List<BlueprintAbility> filtered = StandaloneElementalDiscovery.FilterLearningSpells(rows,
                            list, level, selection, book);
                        bool sourceRetained = filtered.Any(value => ReferenceEquals(value, source));
                        if (!sourceRetained) removed++;
                        probes++;
                        row["knownBefore"] = knownBefore;
                        row["sourceRetained"] = sourceRetained;
                        row["inputCount"] = rows.Count; row["outputCount"] = filtered.Count;
                        row["proof"] = Cleanup2ElementalLiveDisposition(list, level, book, rows, source, spec);
                        row["boundedUiDispositions"] = new JArray(StandaloneElementalDiscovery.SnapshotDispositions()
                            .Where(value => value.StartsWith(list.AssetGuid + ":" + level + ":", StringComparison.Ordinal)));
                        bool copy = !ReferenceEquals(rows, filtered) && Cleanup2SameElementalRefs(originalOrder, rows) &&
                            Cleanup2ElementalFilteredSubset(originalOrder, filtered, book);
                        bool intact = surfaces.Unchanged() && bookSnapshot.Unchanged(book) && barSnapshot.IsRestored() &&
                            ReferenceEquals(state, selection.LevelCount[level]) &&
                            ReferenceEquals(selectedArray, selection.LevelCount[level].SpellSelections) &&
                            selectedArray.Length == 1 && selectedArray[0] == null && selection.CanSpendSlot(level, 0);
                        row["freshLegalSubset"] = copy; row["originalObjectsIntact"] = intact;
                        Cleanup2Check("elemental-list-helper-" + list.AssetGuid + "-" + level, copy && intact,
                            row, "The actual UI helper returns a fresh legal subset and preserves native list, component, variant, book, slot, selection and hotbar objects.");

                        // Actual native allocation API, not hand-written row logic.
                        selection.SpendSlot(level, source, 0);
                        List<BlueprintAbility> selected = StandaloneElementalDiscovery.FilterLearningSpells(rows,
                            list, level, selection, book);
                        bool selectedIntact = selected.Any(value => ReferenceEquals(value, source)) &&
                            ReferenceEquals(selection.LevelCount[level].SpellSelections, selectedArray) &&
                            ReferenceEquals(selectedArray[0], source) && !selection.CanSpendSlot(level, 0) &&
                            bookSnapshot.Unchanged(book) && surfaces.Unchanged() && barSnapshot.IsRestored();
                        Cleanup2Check("elemental-selected-legacy-" + list.AssetGuid + "-" + level, selectedIntact,
                            new JObject { ["list"] = list.AssetGuid, ["source"] = source.AssetGuid,
                                ["selectedReferenceRetained"] = ReferenceEquals(selectedArray[0], source),
                                ["visibleSelectedRow"] = selected.Any(value => ReferenceEquals(value, source)),
                                ["scope"] = "Actual SpellSelectionData.SpendSlot allocation remains visible and undoable; no commit or known-spell mutation." },
                            "An actual native selection allocation retains its exact legacy row and allocation objects.");

                        // Known objects are granted only to the disposable caster.
                        // AddKnown(isCopy:true) avoids a learn/autofill event.
                        book.AddKnown(level, source, true);
                        var knownSnapshot = new Cleanup2ElementalBookSnapshot(book);
                        var emptySelection = new SpellSelectionData(book.Blueprint, list);
                        emptySelection.SetLevelSpells(level, 1);
                        AbilityData knownObject = book.GetKnownSpells(level).First(value => ReferenceEquals(value.Blueprint, source));
                        List<BlueprintAbility> knownRows = StandaloneElementalDiscovery.FilterLearningSpells(rows,
                            list, level, emptySelection, book);
                        bool knownIntact = knownRows.Any(value => ReferenceEquals(value, source)) &&
                            book.GetKnownSpells(level).Any(value => ReferenceEquals(value, knownObject)) &&
                            knownSnapshot.Unchanged(book) && surfaces.Unchanged();
                        Cleanup2Check("elemental-known-legacy-" + list.AssetGuid + "-" + level, knownIntact,
                            new JObject { ["list"] = list.AssetGuid, ["source"] = source.AssetGuid,
                                ["knownSpellObject"] = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(knownObject),
                                ["visibleKnownLegacyRow"] = knownRows.Any(value => ReferenceEquals(value, source)),
                                ["oldKnownReferenceIntact"] = knownIntact },
                            "An existing actual native known AbilityData remains the same reachable object after the learning helper.");
                    }
                    yield return 0;
                }

                JArray exceptions = Cleanup2ElementalSpecialLists(fixture, surfaces);
                Cleanup2Check("elemental-individual-special-consumers", exceptions.Count > 0 && surfaces.Unchanged(),
                    new JObject { ["rows"] = exceptions,
                        ["wrigglingMan"] = Cleanup2ElementalAbility(fixture, StandaloneElementalPresentationPolicy.WrigglingManAbilityGuid).AssetGuid,
                        ["wrigglingDisposition"] = StandaloneElementalPresentationPolicy.Disposition(true,
                            StandaloneElementalPresentationPolicy.WrigglingManAbilityGuid, StandaloneElementalConsumerRoute.NpcOrAi, null),
                        ["scope"] = "Every actual standalone child/raw special-list membership is named individually. No domain, item or NPC execution is remapped." },
                    "Actual independently listed children and special lists remain explicit protected exceptions, including unique Wriggling Man.");

                Spellbook wizardBook = fixture.Caster.Descriptor.Spellbooks.Single(value =>
                    value.Blueprint.SpellList.AssetGuid == "ba0401fdeb4062f40a7aa95b6f07fe89");
                foreach (int frame in Cleanup2ElementalCastPair(fixture, wizardBook, surfaces)) yield return frame;
                Cleanup2Check("elemental-diagnostic-summary", ordinaryLists.Length == 10 && probes > 0 && surfaces.Unchanged(),
                    new JObject { ["ordinaryLists"] = ordinaryLists.Length, ["actualHelperProbes"] = probes,
                        ["actuallyRemovedRows"] = removed,
                        ["uiDispositions"] = new JArray(StandaloneElementalDiscovery.SnapshotDispositions()),
                        ["scope"] = "Counts distinguish a live installed hook, observed equivalence and actual filter removal. Zero removals is retained diagnostic evidence and does not complete MENU-02.",
                        ["noVisibleLearningUiClaim"] = true, ["saveWriteObserved"] = _workingSaveSmoke.WriteObserved },
                    "All ten observed ordinary lists receive bounded native diagnostics and at least one actual helper probe without shared blueprint mutation.");
            }
            finally
            {
                foreach (Spellbook added in addedBooks.AsEnumerable().Reverse())
                {
                    if (!ReferenceEquals(fixture.Caster.Descriptor.GetSpellbook(added.Blueprint), added))
                        throw new InvalidOperationException("Elemental review refuses to remove an unowned replacement book.");
                    fixture.Caster.Descriptor.DeleteSpellbook(added.Blueprint);
                    added.Dispose();
                }
                Cleanup2Check("elemental-book-cleanup", Cleanup2SameElementalRefs(booksBefore,
                    fixture.Caster.Descriptor.Spellbooks) && surfaces.Unchanged() && !_workingSaveSmoke.WriteObserved,
                    new JObject { ["addedBooks"] = addedBooks.Count,
                        ["originalBookReferencesRestored"] = Cleanup2SameElementalRefs(booksBefore, fixture.Caster.Descriptor.Spellbooks),
                        ["sharedSurfacesUnchanged"] = surfaces.Unchanged(),
                        ["scope"] = "Extra books are removed only from the disposable caster; that caster and fixture-granted known/prepared spells are destroyed by the shared fixture teardown." },
                    "Only owned added books are removed; original book references and all shared blueprint/list surfaces remain exact.");
            }
        }

        private bool Cleanup2ElementalOwnTranspiler(MethodBase method, Type patchType)
        {
            if (method == null || !_context.Harmony.GetPatchedMethods().Contains(method)) return false;
            return _context.Harmony.GetPatchInfo(method).Transpilers.Count(value => value.owner == _context.ModId &&
                value.patch.DeclaringType == patchType && value.patch.Name == "Transpiler") == 1;
        }

        private static BlueprintAbility Cleanup2ElementalAbility(ExpandedSummoningCorrectionFixture fixture, string guid)
        { return fixture.Blueprints.OfType<BlueprintAbility>().Single(value => value.AssetGuid == guid); }

        private static JArray Cleanup2ElementalContracts(ExpandedSummoningCorrectionFixture fixture)
        {
            var result = new JArray();
            foreach (StandaloneElementalAbilitySpec spec in StandaloneElementalPresentationPolicy.All.Where(value => value.IsParent))
            foreach (SummonFamily family in new[] { SummonFamily.Monster, SummonFamily.NaturesAlly })
            {
                BlueprintAbility source = Cleanup2ElementalAbility(fixture, spec.Guid);
                BlueprintAbility canonical = Cleanup2ElementalAbility(fixture, family == SummonFamily.Monster ?
                    spec.MonsterParentGuid : spec.NaturesAllyParentGuid);
                result.Add(Cleanup2ElementalContractRow(source, canonical, null, null, family, spec.CanonicalTier));
                foreach (string key in spec.CreatureKeys)
                {
                    var childSpec = StandaloneElementalPresentationPolicy.All.Single(value =>
                        value.ParentGuid == spec.Guid && value.CreatureKeys.Contains(key));
                    var variant = ExpandedSummoningCatalog.GenerateVariants(family).Single(value =>
                        value.ParentTier == spec.CanonicalTier && value.Multiplicity == SummonMultiplicity.One && value.Creature.Key == key);
                    BlueprintAbility child = (BlueprintAbility)ExpandedSummoningBlueprints.Registered.BySymbol[
                        ExpandedSummoningIdentityCatalog.AbilitySymbol(variant)];
                    BlueprintUnit unit = (BlueprintUnit)ExpandedSummoningBlueprints.Registered.BySymbol[
                        ExpandedSummoningIdentityCatalog.UnitSymbol(variant.Creature)];
                    JObject row = Cleanup2ElementalContractRow(Cleanup2ElementalAbility(fixture, childSpec.Guid),
                        child, key, unit, family, spec.CanonicalTier);
                    row["canonicalPublishedCount"] = canonical.Variants.Count(value => ReferenceEquals(value, child));
                    row["sourcePublishedCount"] = source.Variants.Count(value => value != null && value.AssetGuid == childSpec.Guid);
                    result.Add(row);
                }
            }
            return result;
        }

        private static JObject Cleanup2ElementalContractRow(BlueprintAbility source, BlueprintAbility canonical,
            string key, BlueprintUnit unit, SummonFamily family, int level)
        {
            string reason;
            bool equal = StandaloneElementalInvocationContract.Equal(source, canonical, key, unit, out reason);
            var row = new JObject { ["source"] = source.AssetGuid, ["canonical"] = canonical.AssetGuid,
                ["sourceName"] = source.name, ["canonicalName"] = canonical.name, ["key"] = key,
                ["family"] = family.ToString(), ["level"] = level, ["observed"] = true,
                ["equivalent"] = equal, ["disposition"] = reason,
                ["sourceSignature"] = SummonSignatureReader.Read(source).Render(),
                ["canonicalSignature"] = SummonSignatureReader.Read(canonical).Render() };
            // Exact diagnostic reuse of the production reader, not a competing
            // equality implementation. Export only bounded scalar differences.
            try
            {
                MethodInfo capture = typeof(StandaloneElementalInvocationContract).GetMethod("Capture",
                    BindingFlags.Static | BindingFlags.NonPublic, null,
                    new[] { typeof(BlueprintAbility), typeof(string), typeof(string) }, null);
                if (capture == null) throw new MissingMethodException("StandaloneElementalInvocationContract.Capture");
                var left = (SortedDictionary<string, string>)capture.Invoke(null, new object[] {
                    source, key == null ? null : ExpandedSummoningDonorCatalog.For(key).Guid, key });
                var right = (SortedDictionary<string, string>)capture.Invoke(null, new object[] {
                    canonical, unit == null ? null : unit.AssetGuid, key });
                string[] differences = left.Keys.Concat(right.Keys).Distinct(StringComparer.Ordinal)
                    .Where(path => !left.ContainsKey(path) || !right.ContainsKey(path) || left[path] != right[path])
                    .OrderBy(path => path, StringComparer.Ordinal).ToArray();
                row["differenceCount"] = differences.Length;
                row["firstDifferences"] = new JArray(differences.Take(16).Select(path => new JObject {
                    ["path"] = path, ["source"] = left.ContainsKey(path) ? Cleanup2ElementalBound(left[path], 1024) : "<missing>",
                    ["canonical"] = right.ContainsKey(path) ? Cleanup2ElementalBound(right[path], 1024) : "<missing>" }));
            }
            catch (Exception exception)
            { row["differenceReaderException"] = Cleanup2ElementalBound((exception.InnerException ?? exception).ToString(), 1600); }
            return row;
        }

        private static string Cleanup2ElementalLiveDisposition(BlueprintSpellList list, int level, Spellbook book,
            IList<BlueprintAbility> rows, BlueprintAbility source, StandaloneElementalAbilitySpec spec)
        {
            MethodInfo prove = typeof(StandaloneElementalDiscovery).GetMethod("Prove", BindingFlags.Static | BindingFlags.NonPublic);
            if (prove == null) throw new MissingMethodException("StandaloneElementalDiscovery.Prove");
            object[] args = { list, level, book, rows, source, spec, null };
            prove.Invoke(null, args);
            return (string)args[6];
        }

        private static bool Cleanup2ElementalFilteredSubset(IList<BlueprintAbility> before,
            IList<BlueprintAbility> after, Spellbook book)
        {
            int cursor = 0;
            foreach (BlueprintAbility item in after)
            {
                while (cursor < before.Count && !ReferenceEquals(before[cursor], item)) cursor++;
                if (cursor == before.Count) return false;
                cursor++;
            }
            return before.Where(value => !after.Any(item => ReferenceEquals(item, value))).All(value =>
                value != null && StandaloneElementalPresentationPolicy.Find(value.AssetGuid)?.IsParent == true && !book.IsKnown(value));
        }

        private static JArray Cleanup2ElementalSpecialLists(ExpandedSummoningCorrectionFixture fixture,
            Cleanup2ElementalSurfaces surfaces)
        {
            var result = new JArray();
            bool unchanged = surfaces.Unchanged();
            foreach (BlueprintSpellList list in fixture.Blueprints.OfType<BlueprintSpellList>())
            foreach (SpellLevelList level in list.SpellsByLevel ?? new SpellLevelList[0])
            foreach (BlueprintAbility ability in level?.Spells ?? new List<BlueprintAbility>())
            {
                StandaloneElementalAbilitySpec spec = ability == null ? null : StandaloneElementalPresentationPolicy.Find(ability.AssetGuid);
                if (spec == null || spec.IsParent && StandaloneElementalPresentationPolicy.IsObservedOrdinaryList(list.AssetGuid)) continue;
                result.Add(new JObject { ["list"] = list.AssetGuid, ["listName"] = list.name,
                    ["level"] = level.SpellLevel, ["ability"] = ability.AssetGuid,
                    ["ordinaryList"] = StandaloneElementalPresentationPolicy.IsObservedOrdinaryList(list.AssetGuid),
                    ["independentlyListedChild"] = !spec.IsParent && !spec.UniqueNpc,
                    ["disposition"] = spec.UniqueNpc ? "preserve-unique-wriggling-man" :
                        !spec.IsParent ? "preserve-independently-listed-child-access" : "preserve-domain-or-special-list",
                    ["sharedSurfacesUnchanged"] = unchanged });
            }
            return result;
        }

        private IEnumerable<int> Cleanup2ElementalCastPair(ExpandedSummoningCorrectionFixture fixture,
            Spellbook book, Cleanup2ElementalSurfaces surfaces)
        {
            var hooks = HarmonyInstance.Create("kmg.cleanup2.elementals." + _request.RunId);
            const BindingFlags statics = BindingFlags.Static | BindingFlags.NonPublic;
            try
            {
                hooks.Patch(typeof(RuleCastSpell).GetMethod("OnTrigger"), null,
                    new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("ExpandedSummoningPlayerPathRuleCastPostfix", statics)), null);
                hooks.Patch(typeof(ContextActionSpawnMonster).GetMethod("RunAction"),
                    new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("ExpandedSummoningPlayerPathSpawnPrefix", statics)), null, null);
                _expandedSummoningPlayerPathCaptureActive = true;
                var sourceSpec = StandaloneElementalPresentationPolicy.All.Single(value => value.IsParent && value.CanonicalTier == 2);
                var childSpec = StandaloneElementalPresentationPolicy.All.Single(value => value.ParentGuid == sourceSpec.Guid &&
                    value.CreatureKeys.Contains("small-air-elemental"));
                var variant = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster).Single(value =>
                    value.ParentTier == 2 && value.Multiplicity == SummonMultiplicity.One && value.Creature.Key == "small-air-elemental");
                BlueprintAbility nativeParent = Cleanup2ElementalAbility(fixture, sourceSpec.Guid);
                BlueprintAbility nativeChild = Cleanup2ElementalAbility(fixture, childSpec.Guid);
                BlueprintAbility canonicalParent = Cleanup2ElementalAbility(fixture, sourceSpec.MonsterParentGuid);
                BlueprintAbility canonicalChild = (BlueprintAbility)ExpandedSummoningBlueprints.Registered.BySymbol[
                    ExpandedSummoningIdentityCatalog.AbilitySymbol(variant)];
                BlueprintUnit canonicalUnit = (BlueprintUnit)ExpandedSummoningBlueprints.Registered.BySymbol[
                    ExpandedSummoningIdentityCatalog.UnitSymbol(variant.Creature)];
                foreach (bool legacy in new[] { true, false })
                {
                    BlueprintAbility parent = legacy ? nativeParent : canonicalParent;
                    BlueprintAbility child = legacy ? nativeChild : canonicalChild;
                    BlueprintUnit expectedUnit = legacy ? fixture.Blueprints.OfType<BlueprintUnit>().Single(value =>
                        value.AssetGuid == ExpandedSummoningDonorCatalog.For("small-air-elemental").Guid) : canonicalUnit;
                    book.Rest();
                    AbilityData data = PrepareExpandedSummoningPlayerPathSpell(book, parent, child, 2);
                    SpellSlot slot = data.ParamSpellSlot;
                    AbilityData originalSlotSpell = slot.Spell;
                    AbilityData originalKnown = book.GetKnownSpells(2).First(value => ReferenceEquals(value.Blueprint, parent));
                    var selection = new SpellSelectionData(book.Blueprint, book.Blueprint.SpellList);
                    selection.SetLevelSpells(2, 1);
                    var preparedSnapshot = new Cleanup2ElementalBookSnapshot(book);
                    StandaloneElementalDiscovery.FilterLearningSpells(book.Blueprint.SpellList.GetSpells(2),
                        book.Blueprint.SpellList, 2, selection, book);
                    bool preserved = preparedSnapshot.Unchanged(book) && ReferenceEquals(slot.Spell, originalSlotSpell) &&
                        book.GetKnownSpells(2).Any(value => ReferenceEquals(value, originalKnown)) &&
                        ReferenceEquals(data.ParamSpellSlot, slot) && ReferenceEquals(data.ConvertedFrom, originalSlotSpell);
                    int slotsBefore = CountAvailableExpandedSummoningSlots(book, 2);
                    string resourcesBefore = Cleanup2ElementalResources(fixture.Caster);
                    ExpandedSummoningRuleCapture.Clear();
                    ExpandedSummoningPlayerPathEvents.Clear();
                    _expandedSummoningPlayerPathRuleCastCount = 0;
                    _expandedSummoningPlayerPathSpawnActionCount = 0;
                    UnitUseAbility command;
                    var parameters = new Kingmaker.AreaLogic.Cutscenes.CutsceneParametersContext();
                    using (parameters.Data) command = new UnitUseAbility(data, new TargetWrapper(fixture.Caster.Position));
                    bool canCast = data.IsAvailable && command.CanStart;
                    try
                    {
                        if (canCast)
                        {
                            command.IgnoreCooldown(TimeSpan.Zero);
                            fixture.Caster.Commands.Run(command);
                            command.Start();
                            if (command.IsRunning)
                            {
                                if (command.Animation != null) command.Animation.IsActed = true;
                                command.Tick();
                                if (command.ExecutionProcess != null)
                                    for (int tick = 0; tick < 5000 && !command.ExecutionProcess.IsEnded; tick++) command.ExecutionProcess.Tick();
                                if (command.Animation != null) FinishExpandedSummoningAnimation(command.Animation);
                                if (!command.IsFinished) command.Tick();
                            }
                        }
                        Game.Instance.EntityCreator.Tick();
                    }
                    finally
                    {
                        foreach (UnitEntityData unit in ExpandedSummoningRuleCapture.Where(value => value != null &&
                            value.Blueprint == expectedUnit && ExpandedSummoningPlayerPathUnitExact(value, fixture.Caster)))
                            if (!fixture.Created.Contains(unit)) fixture.Created.Add(unit);
                    }
                    UnitEntityData[] units = ExpandedSummoningRuleCapture.Where(value => value != null).Distinct().ToArray();
                    int slotsAfter = CountAvailableExpandedSummoningSlots(book, 2);
                    string resourcesAfter = Cleanup2ElementalResources(fixture.Caster);
                    int resourceComponents = new[] { parent, child }.SelectMany(value => value.ComponentsArray)
                        .OfType<AbilityResourceLogic>().Count();
                    bool exact = preserved && canCast && command.Result.ToString() == "Success" &&
                        slotsAfter == slotsBefore - 1 && !slot.Available && units.Length == 1 && units[0].Blueprint == expectedUnit &&
                        ExpandedSummoningPlayerPathUnitExact(units[0], fixture.Caster) &&
                        _expandedSummoningPlayerPathRuleCastCount == 1 && _expandedSummoningPlayerPathSpawnActionCount == 1 &&
                        resourceComponents == 0 && resourcesBefore == resourcesAfter && surfaces.Unchanged();
                    Cleanup2Check("elemental-native-cast-" + (legacy ? "legacy" : "canonical"), exact,
                        new JObject { ["legacy"] = legacy, ["parent"] = parent.AssetGuid, ["selected"] = child.AssetGuid,
                            ["expectedUnit"] = expectedUnit.AssetGuid, ["canCast"] = canCast,
                            ["commandResult"] = command.Result.ToString(), ["originalPreparedAndKnownObjectsPreserved"] = preserved,
                            ["slotBefore"] = slotsBefore, ["slotAfter"] = slotsAfter, ["originalSlotAvailableAfter"] = slot.Available,
                            ["originalSlotSpellReferenceIntact"] = ReferenceEquals(slot.Spell, originalSlotSpell),
                            ["invocationLinks"] = DescribeExpandedSummoningAbilityData(data),
                            ["ruleCastCount"] = _expandedSummoningPlayerPathRuleCastCount,
                            ["spawnActionCount"] = _expandedSummoningPlayerPathSpawnActionCount,
                            ["resourceComponentCount"] = resourceComponents, ["resourcesBefore"] = resourcesBefore,
                            ["resourcesAfter"] = resourcesAfter, ["units"] = new JArray(units.Select(value => new JObject {
                                ["id"] = value.UniqueId, ["blueprint"] = value.Blueprint.AssetGuid,
                                ["exactProvenance"] = ExpandedSummoningPlayerPathUnitExact(value, fixture.Caster) })),
                            ["scope"] = "Existing original known/prepared objects survive the actual filter and cast through native UnitUseAbility/RuleCastSpell/spawn; synchronous qualified fixture advancement does not prove natural input timing or old-save serialization." },
                        "A retained legacy invocation and canonical counterpart each debit exactly their own real native prepared slot and spawn their exact unit without resource remapping.");
                    DisposeExpandedSummoningUnits(fixture.Created, units.Where(value => fixture.Created.Contains(value)).ToArray());
                    if (!exact) throw new InvalidOperationException("Elemental native cast path failed: " + (legacy ? "legacy" : "canonical"));
                    yield return 0;
                }
            }
            finally
            {
                _expandedSummoningPlayerPathCaptureActive = false;
                hooks.UnpatchAll(hooks.Id);
                ExpandedSummoningRuleCapture.Clear();
            }
        }

        private static string Cleanup2ElementalResources(UnitEntityData unit)
        {
            FieldInfo field = typeof(UnitAbilityResourceCollection).GetField("m_Resources", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null) throw new MissingFieldException("UnitAbilityResourceCollection.m_Resources");
            return string.Join("|", ((IDictionary)field.GetValue(unit.Descriptor.Resources)).Values.Cast<UnitAbilityResource>()
                .OrderBy(value => value.Blueprint.AssetGuid, StringComparer.Ordinal).Select(value =>
                    value.Blueprint.AssetGuid + ":" + System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value) + ":" + value.Amount).ToArray());
        }

        private static string Cleanup2ElementalBound(string value, int length)
        { return value == null || value.Length <= length ? value : value.Substring(0, length) + "<truncated>"; }

        private static bool Cleanup2SameElementalRefs<T>(IEnumerable<T> left, IEnumerable<T> right) where T : class
        {
            T[] a = left.ToArray(), b = right.ToArray();
            return a.Length == b.Length && a.Select((value, index) => ReferenceEquals(value, b[index])).All(value => value);
        }

        private sealed class Cleanup2ElementalBookSnapshot
        {
            private readonly AbilityData[] _known;
            private readonly SpellSlot[] _slots;
            private readonly AbilityData[] _prepared;
            private readonly string _fingerprint;
            internal Cleanup2ElementalBookSnapshot(Spellbook book)
            {
                _known = book.GetAllKnownSpells().ToArray();
                _slots = Enumerable.Range(0, 10).SelectMany(book.GetMemorizedSpellSlots).ToArray();
                _prepared = _slots.Select(value => value.Spell).ToArray();
                _fingerprint = TeleportResourceFingerprint(book);
            }
            internal bool Unchanged(Spellbook book)
            {
                return _fingerprint == TeleportResourceFingerprint(book) &&
                    Cleanup2SameElementalRefs(_known, book.GetAllKnownSpells()) &&
                    Cleanup2SameElementalRefs(_slots, Enumerable.Range(0, 10).SelectMany(book.GetMemorizedSpellSlots)) &&
                    Cleanup2SameElementalRefs(_prepared, _slots.Select(value => value.Spell));
            }
        }

        private sealed class Cleanup2ElementalSurfaces
        {
            private readonly List<Func<bool>> _same = new List<Func<bool>>();
            internal Cleanup2ElementalSurfaces(IEnumerable<BlueprintScriptableObject> blueprints,
                IEnumerable<BlueprintSpellList> ordinaryLists)
            {
                var observed = new HashSet<string>(StandaloneElementalPresentationPolicy.All.Select(value => value.Guid), StringComparer.Ordinal);
                BlueprintSpellList[] watchedLists = ordinaryLists.Concat(blueprints.OfType<BlueprintSpellList>().Where(list =>
                    (list.SpellsByLevel ?? new SpellLevelList[0]).Any(level => level != null && level.Spells != null &&
                        level.Spells.Any(ability => ability != null && observed.Contains(ability.AssetGuid)))))
                    .Distinct().ToArray();
                foreach (BlueprintSpellList list in watchedLists)
                {
                    SpellLevelList[] levels = list.SpellsByLevel;
                    _same.Add(() => ReferenceEquals(list.SpellsByLevel, levels));
                    for (int index = 0; index < levels.Length; index++)
                    {
                        int at = index;
                        SpellLevelList level = levels[at];
                        if (level == null) continue;
                        List<BlueprintAbility> raw = level.Spells;
                        BlueprintAbility[] rawOrder = raw.ToArray();
                        List<BlueprintAbility> cached = list.GetSpells(at);
                        BlueprintAbility[] cachedOrder = cached.ToArray();
                        _same.Add(() => ReferenceEquals(levels[at], level) && ReferenceEquals(level.Spells, raw) &&
                            Cleanup2SameElementalRefs(rawOrder, raw) && ReferenceEquals(list.GetSpells(at), cached) &&
                            Cleanup2SameElementalRefs(cachedOrder, cached));
                    }
                }
                var guids = new HashSet<string>(StandaloneElementalPresentationPolicy.All.Select(value => value.Guid), StringComparer.Ordinal);
                foreach (StandaloneElementalAbilitySpec spec in StandaloneElementalPresentationPolicy.All.Where(value => value.IsParent))
                { guids.Add(spec.MonsterParentGuid); guids.Add(spec.NaturesAllyParentGuid); }
                var keys = new HashSet<string>(StandaloneElementalPresentationPolicy.All.Where(value => value.IsParent)
                    .SelectMany(value => value.CreatureKeys), StringComparer.Ordinal);
                BlueprintAbility[] canonicalChildren = new[] { SummonFamily.Monster, SummonFamily.NaturesAlly }
                    .SelectMany(ExpandedSummoningCatalog.GenerateVariants).Where(value =>
                        value.Multiplicity == SummonMultiplicity.One && keys.Contains(value.Creature.Key))
                    .Select(value => (BlueprintAbility)ExpandedSummoningBlueprints.Registered.BySymbol[
                        ExpandedSummoningIdentityCatalog.AbilitySymbol(value)]).ToArray();
                BlueprintAbility[] abilities = blueprints.OfType<BlueprintAbility>().Where(value => guids.Contains(value.AssetGuid))
                    .Concat(canonicalChildren).Distinct().ToArray();
                foreach (BlueprintAbility ability in abilities)
                {
                    BlueprintComponent[] components = ability.ComponentsArray;
                    BlueprintComponent[] componentOrder = components.ToArray();
                    object[] graph = AlternateSummoningGraph.Objects<object>(components).ToArray();
                    string signature = SummonSignatureReader.Read(ability).Render();
                    _same.Add(() => ReferenceEquals(ability.ComponentsArray, components) &&
                        Cleanup2SameElementalRefs(componentOrder, components) &&
                        Cleanup2SameElementalRefs(graph, AlternateSummoningGraph.Objects<object>(components)) &&
                        signature == SummonSignatureReader.Read(ability).Render());
                    foreach (AbilityVariants menu in components.OfType<AbilityVariants>())
                    {
                        BlueprintAbility[] variants = menu.Variants;
                        BlueprintAbility[] order = variants?.ToArray();
                        _same.Add(() => ReferenceEquals(menu.Variants, variants) &&
                            (order == null || Cleanup2SameElementalRefs(order, menu.Variants)));
                    }
                }
            }
            internal bool Unchanged() { return _same.All(read => read()); }
        }
    }
}
