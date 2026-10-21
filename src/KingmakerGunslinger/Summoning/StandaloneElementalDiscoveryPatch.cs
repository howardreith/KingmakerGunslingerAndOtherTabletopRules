using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony12;
using Kingmaker.Assets.UI.LevelUp;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UI._ConsoleUI.CharGen.Phases.Spells;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Class.LevelUp;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Learning UI copies only. No shared list, known spell, selected spell,
    /// prepared slot, hotbar object, domain, item or NPC fact is written here.
    /// Both native UI call sites use this same proof and budget decision.
    /// </summary>
    internal static class StandaloneElementalDiscovery
    {
        internal const string GameMvid = "07fa1e4d-8618-41b3-9b8d-faa17d3b26f7";
        private const BindingFlags Instance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly FieldInfo ConsoleSelection = typeof(CharGenSpellsPhaseVM)
            .GetField("m_SelectionData", Instance);
        private static readonly FieldInfo ConsolePreviewBook = typeof(CharGenSpellsPhaseVM)
            .GetField("PreviewSpellbook", Instance);
        private static readonly List<string> Observations = new List<string>();
        private static readonly HashSet<string> Observed = new HashSet<string>(StringComparer.Ordinal);
        internal static string PcSeamState = "not-installed";
        internal static string ConsoleSeamState = "not-installed";

        internal static IReadOnlyList<string> SnapshotDispositions()
        { lock (Observations) return Observations.ToArray(); }

        internal static List<BlueprintAbility> PcLearningSpells(BlueprintSpellList list, int level,
            SpellSelectionData selection, CharBSelectorLayer layer)
        {
            // Read the native list before entering optional presentation code;
            // an exception from GetSpells retains native exception semantics.
            List<BlueprintAbility> rows = list.GetSpells(level);
            try
            {
                LevelUpController controller = layer == null || layer.CharBuildController == null ?
                    null : layer.CharBuildController.LevelUpController;
                return FilterLearningSpells(rows, list, level, selection, PreviewBook(controller, selection));
            }
            catch (Exception exception)
            { Record(list, level, null, "preserve-pc-context-exception:" + exception.GetType().Name); return rows.ToList(); }
        }

        internal static List<BlueprintAbility> ConsoleLearningSpells(BlueprintSpellList list, int level,
            CharGenSpellsPhaseVM vm, LevelUpController controller)
        {
            List<BlueprintAbility> rows = list.GetSpells(level);
            try
            {
                SpellSelectionData selection = vm == null || ConsoleSelection == null ? null :
                    ConsoleSelection.GetValue(vm) as SpellSelectionData;
                Spellbook preview = PreviewBook(controller, selection);
                Spellbook uiBook = vm == null || ConsolePreviewBook == null ? null :
                    ConsolePreviewBook.GetValue(vm) as Spellbook;
                // A different VM book is an unclassified special route.
                if (!ReferenceEquals(preview, uiBook)) preview = null;
                return FilterLearningSpells(rows, list, level, selection, preview);
            }
            catch (Exception exception)
            { Record(list, level, null, "preserve-console-context-exception:" + exception.GetType().Name); return rows.ToList(); }
        }

        private static Spellbook PreviewBook(LevelUpController controller, SpellSelectionData selection)
        {
            if (controller == null || controller.Preview == null || selection == null || selection.Spellbook == null)
                return null;
            Spellbook[] books = controller.Preview.Spellbooks.Where(value =>
                ReferenceEquals(value.Blueprint, selection.Spellbook)).ToArray();
            return books.Length == 1 ? books[0] : null;
        }

        /// <summary>Native observer entry point; also the exact UI decision seam.</summary>
        internal static List<BlueprintAbility> FilterLearningSpells(IEnumerable<BlueprintAbility> rows,
            BlueprintSpellList list, int level, SpellSelectionData selection, Spellbook preview)
        {
            var original = rows.ToList();
            try
            {
                if (!ExpandedSummoningVisualVariantPatch.IsModuleActive || !BlueprintBootstrap.IsInitialized)
                { Record(list, level, null, "preserve-module-off-or-not-initialized"); return original; }
                if (list == null || selection == null || preview == null || selection.Spellbook == null ||
                    !ReferenceEquals(selection.SpellList, list) ||
                    !ReferenceEquals(preview.Blueprint, selection.Spellbook) ||
                    !ReferenceEquals(preview.Blueprint.SpellList, list) ||
                    !StandaloneElementalPresentationPolicy.IsObservedOrdinaryList(list.AssetGuid) ||
                    level < 0 || level > 9 || selection.LevelCount == null || selection.ExtraByStat)
                { Record(list, level, null, "preserve-unclassified-selection-context"); return original; }

                var selected = new HashSet<string>(StringComparer.Ordinal);
                foreach (SpellSelectionData.SpellSelectionState state in selection.LevelCount)
                    if (state != null && state.SpellSelections != null)
                        foreach (BlueprintAbility ability in state.SpellSelections)
                            if (ability != null) selected.Add(ability.AssetGuid);
                foreach (BlueprintAbility ability in selection.ExtraSelected ?? new BlueprintAbility[0])
                    if (ability != null) selected.Add(ability.AssetGuid);
                var known = new HashSet<string>(original.Where(value => value != null && preview.IsKnown(value))
                    .Select(value => value.AssetGuid), StringComparer.Ordinal);
                // PC SetupSpellSelectorView omits UniqueSpellComponent rows.
                // Charge that stricter availability to both UI budgets.
                var unavailable = new HashSet<string>(original.Where(value => value != null &&
                    (value.ComponentsArray ?? new BlueprintComponent[0]).OfType<UniqueSpellComponent>().Any())
                    .Select(value => value.AssetGuid), StringComparer.Ordinal);
                int freeLevel = level < selection.LevelCount.Length && selection.LevelCount[level] != null &&
                    selection.LevelCount[level].SpellSelections != null ?
                    selection.LevelCount[level].SpellSelections.Count(value => value == null) : 0;
                int freeExtra = level <= selection.ExtraMaxLevel && selection.ExtraSelected != null ?
                    selection.ExtraSelected.Count(value => value == null) : 0;
                var eligible = new HashSet<string>(StringComparer.Ordinal);
                foreach (BlueprintAbility ability in original)
                {
                    if (ability == null) continue;
                    StandaloneElementalAbilitySpec spec = StandaloneElementalPresentationPolicy.Find(ability.AssetGuid);
                    if (spec == null) continue;
                    if (selected.Contains(ability.AssetGuid) || known.Contains(ability.AssetGuid))
                    { Record(list, level, ability, "preserve-selected-or-known-legacy-invocation"); continue; }
                    string reason;
                    StandaloneElementalEquivalenceEvidence proof = Prove(list, level, preview, original, ability, spec, out reason);
                    if (proof != null && StandaloneElementalPresentationPolicy.Disposition(true, spec.Guid,
                        StandaloneElementalConsumerRoute.OrdinaryClassLearning, proof) == "eligible-redundant-discovery-only")
                        eligible.Add(ability.AssetGuid);
                    Record(list, level, ability, reason);
                }
                string budget;
                List<BlueprintAbility> result = StandaloneElementalPresentationPolicy.FilterLearningRows(original,
                    value => value == null ? null : value.AssetGuid, eligible, selected, known,
                    unavailable, freeLevel, freeExtra, true, out budget);
                Record(list, level, null, budget);
                return result;
            }
            catch (Exception exception)
            { Record(list, level, null, "preserve-proof-exception:" + exception.GetType().Name); return original; }
        }

        private static StandaloneElementalEquivalenceEvidence Prove(BlueprintSpellList list, int level,
            Spellbook book, IList<BlueprintAbility> rows, BlueprintAbility source,
            StandaloneElementalAbilitySpec spec, out string reason)
        {
            reason = "preserve-unique-or-child-discovery-route";
            // Ordinary lists observed by the complete class-list census expose
            // the six parents. A child listed independently is a distinct grant
            // and deliberately receives no parent-level inference.
            if (!spec.IsParent) return null;
            if (level != spec.CanonicalTier || rows.Count(value => ReferenceEquals(value, source)) != 1 ||
                rows.Any(value => value != null && value.AssetGuid == spec.Guid && !ReferenceEquals(value, source)))
            { reason = "preserve-source-membership-or-level-ambiguous"; return null; }
            ExpandedSummoningBlueprintSet registered = ExpandedSummoningBlueprints.Registered;
            if (registered == null || typeof(BlueprintAbility).Module.ModuleVersionId.ToString() != GameMvid)
            { reason = "preserve-unverified-engine-or-publication"; return null; }
            var attempts = new List<string>();
            foreach (SummonFamily family in new[] { SummonFamily.Monster, SummonFamily.NaturesAlly })
            {
                string canonicalGuid = family == SummonFamily.Monster ? spec.MonsterParentGuid : spec.NaturesAllyParentGuid;
                BlueprintAbility[] roots = rows.Where(value => value != null && value.AssetGuid == canonicalGuid).ToArray();
                if (roots.Length != 1 || !ExpandedSummoningPublisher.IsPublishedExpandedParent(roots[0]))
                { attempts.Add(family + ":preserve-canonical-parent-not-published-on-same-level"); continue; }
                BlueprintAbility canonical = roots[0];
                string detail;
                if (!StandaloneElementalInvocationContract.Equal(source, canonical, null, null, out detail))
                { attempts.Add(family + ":parent:" + detail); continue; }
                AbilityVariants[] nativeMenus = (source.ComponentsArray ?? new BlueprintComponent[0])
                    .OfType<AbilityVariants>().ToArray();
                AbilityVariants[] menus = (canonical.ComponentsArray ?? new BlueprintComponent[0])
                    .OfType<AbilityVariants>().ToArray();
                if (nativeMenus.Length != 1 || nativeMenus[0].Variants == null ||
                    nativeMenus[0].Variants.Length != 4 || menus.Length != 1 || menus[0].Variants == null)
                { attempts.Add(family + ":preserve-child-menu-shape-unproved"); continue; }
                var covered = new List<string>();
                bool exact = true;
                foreach (string key in spec.CreatureKeys)
                {
                    StandaloneElementalAbilitySpec childSpec = StandaloneElementalPresentationPolicy.All.Single(value =>
                        value.ParentGuid == spec.Guid && value.CreatureKeys.Contains(key));
                    BlueprintAbility[] nativeChildren = nativeMenus[0].Variants.Where(value =>
                        value != null && value.AssetGuid == childSpec.Guid).ToArray();
                    SummonVariantSpec variant = ExpandedSummoningCatalog.GenerateVariants(family).Single(value =>
                        value.ParentTier == level && value.Multiplicity == SummonMultiplicity.One && value.Creature.Key == key);
                    BlueprintScriptableObject canonicalObject, unitObject;
                    if (nativeChildren.Length != 1 || !SummonVisibilityCatalog.IsPublished(variant) ||
                        !registered.BySymbol.TryGetValue(ExpandedSummoningIdentityCatalog.AbilitySymbol(variant), out canonicalObject) ||
                        !registered.BySymbol.TryGetValue(ExpandedSummoningIdentityCatalog.UnitSymbol(variant.Creature), out unitObject))
                    { attempts.Add(family + ":" + key + ":preserve-child-identity-or-publication-unproved"); exact = false; break; }
                    var child = canonicalObject as BlueprintAbility;
                    var unit = unitObject as BlueprintUnit;
                    if (child == null || unit == null || menus[0].Variants.Count(value => ReferenceEquals(value, child)) != 1 ||
                        menus[0].Variants.Any(value => value != null && value.AssetGuid == child.AssetGuid && !ReferenceEquals(value, child)))
                    { attempts.Add(family + ":" + key + ":preserve-live-child-coverage-unproved"); exact = false; break; }
                    if (!StandaloneElementalInvocationContract.Equal(nativeChildren[0], child, key, unit, out detail))
                    { attempts.Add(family + ":" + key + ":" + detail); exact = false; break; }
                    covered.Add(key);
                }
                if (!exact) continue;
                reason = "eligible-redundant-discovery-only:" + family;
                return new StandaloneElementalEquivalenceEvidence {
                    // Completeness is scoped to this exact observed ordinary
                    // list and UI call, never the incomplete NPC/pregen census.
                    ConsumerInventoryComplete = true,
                    SourceSpellListGuid = list.AssetGuid, CanonicalSpellListGuid = list.AssetGuid,
                    SourceSpellbookGuid = book.Blueprint.AssetGuid, CanonicalSpellbookGuid = book.Blueprint.AssetGuid,
                    SourceSpellLevel = level, CanonicalSpellLevel = level, CanonicalParentGuid = canonical.AssetGuid,
                    PublishedCanonicalCreatureKeys = covered.ToArray(), InvocationContractEqual = true,
                    // This helper only constructs a fresh UI collection. Native
                    // objects, definitions, lists and execution remain intact.
                    ExistingSerializedReferencesIntact = true, OriginalSlotAndResourcesRetained = true,
                    LegacyInvocationStillReachable = true, LearningSelectionBudgetVerified = true
                };
            }
            reason = string.Join("|", attempts.ToArray());
            return null;
        }

        private static void Record(BlueprintSpellList list, int level, BlueprintAbility ability, string reason)
        {
            string entry = (list == null ? "<null-list>" : list.AssetGuid) + ":" + level + ":" +
                (ability == null ? "<collection>" : ability.AssetGuid) + ":" + reason;
            if (entry.Length > 768) entry = entry.Substring(0, 768);
            lock (Observations)
                if (Observations.Count < 192 && Observed.Add(entry)) Observations.Add(entry);
        }

        internal static bool VerifyUiContract(MethodBase method, int length)
        {
            return method != null && method.Module.ModuleVersionId.ToString() == GameMvid &&
                method.GetMethodBody() != null && method.GetMethodBody().GetILAsByteArray().Length == length &&
                ConsoleSelection != null && ConsoleSelection.FieldType == typeof(SpellSelectionData) &&
                ConsolePreviewBook != null && ConsolePreviewBook.FieldType == typeof(Spellbook);
        }

        internal static IEnumerable<CodeInstruction> RewriteLearningCall(IEnumerable<CodeInstruction> source,
            bool console)
        {
            var body = source.ToList();
            MethodInfo read = typeof(BlueprintSpellList).GetMethod("GetSpells", new[] { typeof(int) });
            CodeInstruction[] calls = body.Where(value =>
                (value.opcode == OpCodes.Call || value.opcode == OpCodes.Callvirt) && Equals(value.operand, read)).ToArray();
            if (calls.Length != 1 || calls[0].blocks.Count != 0)
            {
                if (console) ConsoleSeamState = "preserve-getspells-call-shape-changed";
                else PcSeamState = "preserve-getspells-call-shape-changed";
                return body;
            }
            int index = body.IndexOf(calls[0]);
            var first = new CodeInstruction(console ? OpCodes.Ldarg_0 : OpCodes.Ldarg_1);
            first.labels.AddRange(calls[0].labels);
            calls[0].labels.Clear();
            body.InsertRange(index, new[] { first,
                new CodeInstruction(console ? OpCodes.Ldarg_1 : OpCodes.Ldarg_0) });
            calls[0].opcode = OpCodes.Call;
            calls[0].operand = typeof(StandaloneElementalDiscovery).GetMethod(
                console ? "ConsoleLearningSpells" : "PcLearningSpells", BindingFlags.Static | BindingFlags.NonPublic);
            if (console) ConsoleSeamState = "installed-exact-learning-call-copy";
            else PcSeamState = "installed-exact-learning-call-copy";
            return body;
        }
    }

    [HarmonyPatch]
    internal static class StandaloneElementalPcLearningPatch
    {
        private static MethodBase _target;
        private static bool Prepare()
        {
            _target = typeof(CharBSelectorLayer).GetMethod("FillSpellLevel", new[] {
                typeof(SpellSelectionData), typeof(int), typeof(bool), typeof(CharBFeatureSelector) });
            bool valid = StandaloneElementalDiscovery.VerifyUiContract(_target, 511);
            if (!valid) StandaloneElementalDiscovery.PcSeamState = "preserve-unverified-native-method";
            return valid;
        }
        private static MethodBase TargetMethod() { return _target; }
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        { return StandaloneElementalDiscovery.RewriteLearningCall(instructions, false); }
    }

    [HarmonyPatch]
    internal static class StandaloneElementalConsoleLearningPatch
    {
        private static MethodBase _target;
        private static bool Prepare()
        {
            _target = typeof(CharGenSpellsPhaseVM).GetMethod("GetSpellsCollection",
                BindingFlags.Instance | BindingFlags.NonPublic, null,
                new[] { typeof(LevelUpController), typeof(BlueprintSpellList) }, null);
            bool valid = StandaloneElementalDiscovery.VerifyUiContract(_target, 298);
            if (!valid) StandaloneElementalDiscovery.ConsoleSeamState = "preserve-unverified-native-method";
            return valid;
        }
        private static MethodBase TargetMethod() { return _target; }
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        { return StandaloneElementalDiscovery.RewriteLearningCall(instructions, true); }
    }
}
