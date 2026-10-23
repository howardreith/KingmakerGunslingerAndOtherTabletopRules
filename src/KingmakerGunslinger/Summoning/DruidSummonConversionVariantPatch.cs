using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UI.ActionBar;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Summoning
{
    // The native converted popup contains simple rows, not nested groups.
    // Project only proven native Druid quantities through the existing native
    // variant factory. That factory owns child -> native quantity -> actual
    // preparation links; this adapter never assigns invocation-context fields.
    [HarmonyPatch(typeof(MechanicActionBarSlotMemorizedSpell), "GetConvertedAbilityData")]
    internal static class DruidSummonConversionVariantPatch
    {
        private const string DruidClassGuid = DruidSummonConversionPolicy.DruidClassGuid;
        private static readonly string[] AllyParents = ExpandedSummoningPublisher.CanonicalParentGuids.Skip(9).ToArray();
        internal static string Outcome = "not-used";
        private static readonly ConditionalWeakTable<AbilityData, Receipt> Receipts = new ConditionalWeakTable<AbilityData, Receipt>();
        internal sealed class Receipt
        {
            internal MechanicActionBarSlotMemorizedSpell Mechanism;
            internal AbilityData Source, Conversion;
            internal Spellbook Book;
            internal SpellSlot Slot;
            internal BlueprintFeature Grant;
            internal BlueprintAbility Parent, NativeConversionBlueprint;
            internal BlueprintAbility[] NativeVariantArray, Variants, OwnedVariants, ProjectionVariants;
            internal AbilityData[] Children, ProjectedRows;
            internal ElementalInvocationContextSnapshot SourceContext, ConversionContext;
            internal ElementalInvocationContextSnapshot[] ChildContexts;
            internal bool Current { get { return ExpandedSummoningVisualVariantPatch.IsModuleActive && BlueprintBootstrap.IsInitialized &&
                ReferenceEquals(Mechanism.SpellSlot, Slot) && ReferenceEquals(Slot.Spell, Source) && ReferenceEquals(Source.Spellbook, Book) &&
                ReferenceEquals(Conversion.ConvertedFrom, Source) && ReferenceEquals(Conversion.Blueprint, NativeConversionBlueprint) &&
                SourceContext.Current && ConversionContext.Current && ChildContexts.All(value => value.Current) &&
                Source.Caster.Progression.Features.Enumerable.Any(value => ReferenceEquals(value?.Blueprint, Grant)) && ExactNativeFeature(Grant, Book.Blueprint.CharacterClass) &&
                Book.GetSpontaneousConversionSpells(Source).Count(value => ReferenceEquals(value, NativeConversionBlueprint)) == 1 &&
                ReferenceEquals(Parent.Variants, NativeVariantArray) && NativeVariantArray != null && NativeVariantArray.SequenceEqual(Variants) &&
                OwnedVariants.All(value => ReferenceEquals(value.Parent, Parent) && Variants.Count(variant => ReferenceEquals(variant, value)) == 1) &&
                Book.Owner.Spellbooks.Any(value => ReferenceEquals(value, Book)) && Children.All(value => ReferenceEquals(value.ConvertedFrom, Conversion) &&
                    ReferenceEquals(value.Spellbook, Book) && ProjectionVariants.Any(variant => ReferenceEquals(value.Blueprint, variant))); } }
        }
        internal static Receipt For(AbilityData child)
        { Receipt receipt; return child != null && Receipts.TryGetValue(child, out receipt) && receipt.Current ? receipt : null; }

        private static bool Prepare() { return ExactFactory(); }
        internal static bool ExactFactory()
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var prepared = typeof(MechanicActionBarSlotMemorizedSpell).GetMethod("GetConvertedAbilityData", flags);
            var variants = typeof(MechanicActionBarSlotSpontaneusSpell).GetMethod("GetConvertedAbilityData", flags);
            return prepared != null && variants != null && prepared.Module.ModuleVersionId.ToString() == ElementalSourceChooserRuntime.GameMvid &&
                prepared.MetadataToken == 0x06002fbc && prepared.GetMethodBody().GetILAsByteArray().Length == 287 &&
                variants.MetadataToken == 0x06002fe4 && variants.GetMethodBody().GetILAsByteArray().Length == 305;
        }

        internal static bool ExactNativeFeature(BlueprintFeature feature, BlueprintCharacterClass druid)
        {
            if (feature == null || feature.AssetGuid != DruidSummonConversionPolicy.FeatureGuid || druid == null ||
                druid.AssetGuid != DruidClassGuid) return false;
            var components = (feature.ComponentsArray ?? new BlueprintComponent[0]).OfType<SpontaneousSpellConversion>().ToArray();
            return components.Length == 3 && components.All(value => ReferenceEquals(value.CharacterClass, druid)) &&
                DruidSummonConversionPolicy.MatchesArrays(components.Select(value => value.SpellsByLevel?.Select(spell => spell?.AssetGuid))) &&
                ReferenceEquals(feature, BlueprintLibraryLookup.RequireExact<BlueprintFeature>(BlueprintBootstrap.Library, feature.AssetGuid, "native Druid conversion feature")) &&
                components.SelectMany(value => value.SpellsByLevel).All(value => value == null || ReferenceEquals(value,
                    BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library, value.AssetGuid, "native Druid quantity conversion")));
        }

        private sealed class FactoryProjection
        {
            internal BlueprintAbility SourceBlueprint;
            internal AbilityData Conversion;
            internal BlueprintAbility[] Variants;
            internal Func<bool> Current;
            internal int Reads;
        }
        private static readonly ConditionalWeakTable<MechanicActionBarSlotSpontaneusSpell, FactoryProjection> FactoryProjections =
            new ConditionalWeakTable<MechanicActionBarSlotSpontaneusSpell, FactoryProjection>();
        internal static BlueprintAbility[] NativeFactoryVariants(BlueprintAbility blueprint, MechanicActionBarSlotSpontaneusSpell factory)
        {
            FactoryProjection projection;
            if (factory == null || !FactoryProjections.TryGetValue(factory, out projection)) return blueprint.Variants;
            if (!ReferenceEquals(projection.SourceBlueprint, blueprint) || !ReferenceEquals(factory.Spell, projection.Conversion) ||
                !ReferenceEquals(factory.Unit, projection.Conversion.Caster.Unit))
                throw new InvalidOperationException("Exact native Druid factory source changed.");
            if (!projection.Current()) throw new InvalidOperationException("Native Druid quantity context changed inside its factory.");
            projection.Reads++; return projection.Variants;
        }
        private static List<AbilityData> NativeChildren(UnitDescriptor caster, AbilityData conversion, BlueprintAbility[] variants, Func<bool> current)
        {
            if (!DruidSummonConversionNativeFactoryPatch.Installed)
                throw new InvalidOperationException("Exact native Druid variant factory required.");
            var native = new MechanicActionBarSlotSpontaneusSpell { Unit = caster.Unit, Spell = conversion };
            var projection = new FactoryProjection { SourceBlueprint = conversion.Blueprint, Conversion = conversion, Variants = variants, Current = current };
            FactoryProjections.Add(native, projection);
            try
            {
                // Installed 06002FE4 IL00E5-012F reads Blueprint.Variants
                // twice, constructs each child (BlueprintAbility,Spellbook),
                // and sets ConvertedFrom to this retained quantity object.
                var result = native.GetConvertedAbilityData();
                if (projection.Reads != 2 || !current()) throw new InvalidOperationException("Native Druid factory projection did not remain exact.");
                return result;
            }
            finally { FactoryProjections.Remove(native); }
        }

        private static object[] References(AbilityData data)
        { return new object[] { data.Blueprint, data.Blueprint.Parent, data.Caster, data.Spellbook, data.Fact, data.ConvertedFrom, data.MetamagicData,
            data.ParamSpellSlot, data.ParamSpellbook, data.SourceItem, data.Blueprint.ComponentsArray }; }
        private static long[] Values(AbilityData data)
        { var meta = data.MetamagicData; return new long[] { data.ParamSpellLevel ?? -1, data.OverrideDC ?? -1, data.OverrideSpellLevel ?? -1,
            data.IsSpellCopy ? 1 : 0, data.PotionForOther ? 1 : 0, meta == null ? 0 : Convert.ToInt64(meta.MetamagicMask),
            meta == null ? 0 : meta.SpellLevelCost, meta == null ? 0 : meta.HeightenLevel }; }

        private static void Postfix(MechanicActionBarSlotMemorizedSpell __instance, ref List<AbilityData> __result)
        {
            if (!BlueprintBootstrap.IsInitialized || ExpandedSummoningBlueprints.Registered == null || !ExpandedSummoningVisualVariantPatch.IsModuleActive ||
                __instance == null || __result == null) return;
            try
            {
                var source = __instance.Spell; var slot = __instance.SpellSlot; var book = source?.Spellbook;
                if (source == null || book == null || slot == null || slot.Type != SpellSlotType.Common || !ReferenceEquals(slot.Spell, source) ||
                    source.Caster?.Unit == null || source.Caster.Unit.Destroyed || source.SourceItem != null || book.Blueprint.Spontaneous || book.Blueprint.CharacterClass?.AssetGuid != DruidClassGuid ||
                    !ReferenceEquals(book.Owner, source.Caster) || !ReferenceEquals(__instance.Unit, source.Caster.Unit) ||
                    !source.Caster.Spellbooks.Any(value => ReferenceEquals(value, book)) ||
                    !book.GetMemorizedSpellSlots(slot.SpellLevel).Any(value => ReferenceEquals(value, slot))) return;
                var druid = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library, DruidClassGuid, "native Druid conversion class");
                if (!ReferenceEquals(book.Blueprint.CharacterClass, druid) || !ReferenceEquals(book.Blueprint, druid.Spellbook) ||
                    !ReferenceEquals(source.Caster.GetSpellbook(druid), book)) return;
                var grants = source.Caster.Progression.Features.Enumerable.Where(feature => ExactNativeFeature(feature?.Blueprint, druid)).ToArray();
                if (grants.Length != 1) return;
                var granted = grants[0].Blueprint.ComponentsArray.OfType<SpontaneousSpellConversion>().SelectMany(value => value.SpellsByLevel).Where(value => value != null).ToArray();
                var nativeConversions = book.GetSpontaneousConversionSpells(source).ToArray();
                var candidates = __result.Where(value => value?.Blueprint != null && ReferenceEquals(value.ConvertedFrom, source) &&
                    ReferenceEquals(value.Spellbook, book) && granted.Any(blueprint => ReferenceEquals(blueprint, value.Blueprint)) &&
                    nativeConversions.Count(blueprint => ReferenceEquals(blueprint, value.Blueprint)) == 1).ToArray();
                if (candidates.Length == 0 || candidates.GroupBy(value => value.Blueprint).Any(value => value.Count() != 1)) return;
                var original = __result; var originalRows = original.ToArray(); var books = source.Caster.Spellbooks.ToArray();
                var bookState = books.Select(value => new { Book = value, Slots = Enumerable.Range(0, 10).SelectMany(value.GetMemorizedSpellSlots).ToArray() })
                    .Select(value => new { value.Book, value.Slots, Spells = value.Slots.Select(nativeSlot => nativeSlot.Spell).ToArray(),
                        Available = value.Slots.Select(nativeSlot => nativeSlot.Available).ToArray(), Known = value.Book.GetAllKnownSpells().ToArray(),
                        Custom = Enumerable.Range(0, 10).SelectMany(value.Book.GetCustomSpells).ToArray() }).ToArray();
                bool availability = slot.Available;
                var sourceContext = new ElementalInvocationContextSnapshot(() => References(source), () => Values(source));
                var resources = new List<BlueprintScriptableObject>();
                foreach (BlueprintScriptableObject resource in source.Caster.Resources) resources.Add(resource);
                var amounts = resources.Select(source.Caster.Resources.GetResourceAmount).ToArray();
                var replacements = new Dictionary<AbilityData, AbilityData[]>(); var receipts = new List<Receipt>();
                foreach (var conversion in candidates)
                {
                    int tier; SummonMultiplicity quantity;
                    if (!DruidSummonConversionPolicy.TryResolve(conversion.Blueprint.AssetGuid, out tier, out quantity)) continue;
                    var parent = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library, AllyParents[tier - 1], "published native Druid SNA parent");
                    if (!ExpandedSummoningPublisher.IsPublishedExpandedParent(parent)) continue;
                    var variantArray = parent.Variants; var variants = variantArray?.ToArray();
                    var expected = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly).Where(value => value.ParentTier == tier &&
                        value.Multiplicity == quantity && SummonVisibilityCatalog.IsPublished(value))
                        .Select(value => ExpandedSummoningBlueprints.Registered.BySymbol[ExpandedSummoningIdentityCatalog.AbilitySymbol(value)] as BlueprintAbility)
                        .Concat(SummonNativeExpansionCatalog.For(SummonFamily.NaturesAlly, tier).Where(value => value.Multiplicity == quantity)
                            .Select(value => ExpandedSummoningBlueprints.Registered.BySymbol[value.Symbol] as BlueprintAbility)).ToArray();
                    if (variants == null || variants.Length == 0 || variants.Any(value => value == null) || expected.Length == 0 || expected.Any(value => value == null) ||
                        variants.Select(value => value.AssetGuid).Distinct(StringComparer.Ordinal).Count() != variants.Length ||
                        expected.Any(value => !ReferenceEquals(value.Parent, parent) || variants.Count(variant => ReferenceEquals(variant, value)) != 1))
                    { Outcome = "preserve-native-or-foreign-incomplete-quantity"; continue; }
                    // Only catalog-proven quantity choices are admitted. A
                    // retained original (e.g. Mastodon) remains selectable;
                    // unknown original conversion rows remain untouched.
                    var projection = variants.Where(value => expected.Any(owned => ReferenceEquals(owned, value)) ||
                        SummonNativeOptionCatalog.All.Any(native => native.Family == SummonFamily.NaturesAlly && native.Tier == tier &&
                            native.Multiplicity == quantity && native.Guid == value.AssetGuid)).ToArray();
                    if (originalRows.Any(value => value != null && !ReferenceEquals(value, conversion) &&
                        projection.Any(variant => ReferenceEquals(variant, value.Blueprint)))) continue;
                    var conversionContext = new ElementalInvocationContextSnapshot(() => References(conversion), () => Values(conversion));
                    Func<bool> current = () => sourceContext.Current && conversionContext.Current && ReferenceEquals(__instance.SpellSlot, slot) &&
                        ReferenceEquals(slot.Spell, source) && slot.Available == availability && ReferenceEquals(parent.Variants, variantArray) && variantArray.SequenceEqual(variants) &&
                        ExactNativeFeature(grants[0].Blueprint, druid);
                    var produced = NativeChildren(source.Caster, conversion, projection, current);
                    var children = produced == null ? new AbilityData[0] : produced.Where(value => value != null &&
                        projection.Any(variant => ReferenceEquals(variant, value.Blueprint))).ToArray();
                    bool complete = children.Length == projection.Length && projection.All(variant => children.Count(value => ReferenceEquals(value.Blueprint, variant)) == 1) &&
                        children.All(value => ReferenceEquals(value.ConvertedFrom, conversion) && ReferenceEquals(value.Spellbook, book) && ReferenceEquals(value.Caster, source.Caster));
                    if (!complete || !current()) { Outcome = "preserve-unverified-native-quantity-context"; return; }
                    replacements.Add(conversion, children);
                    receipts.Add(new Receipt { Mechanism = __instance, Source = source, Conversion = conversion, Book = book, Slot = slot, Grant = grants[0].Blueprint,
                        Parent = parent, NativeConversionBlueprint = conversion.Blueprint, NativeVariantArray = variantArray, Variants = variants, OwnedVariants = expected,
                        ProjectionVariants = projection, Children = children, SourceContext = sourceContext, ConversionContext = conversionContext,
                        ChildContexts = children.Select(child => new ElementalInvocationContextSnapshot(() => References(child), () => Values(child))).ToArray() });
                }
                var currentResources = new List<BlueprintScriptableObject>();
                foreach (BlueprintScriptableObject resource in source.Caster.Resources) currentResources.Add(resource);
                bool intact = sourceContext.Current && receipts.All(value => value.Current) && ReferenceEquals(__result, original) && original.SequenceEqual(originalRows) &&
                    source.Caster.Spellbooks.SequenceEqual(books) && ReferenceEquals(__instance.SpellSlot, slot) && ReferenceEquals(slot.Spell, source) && slot.Available == availability &&
                    bookState.All(value => Enumerable.Range(0, 10).SelectMany(value.Book.GetMemorizedSpellSlots).SequenceEqual(value.Slots) &&
                        value.Slots.Select(nativeSlot => nativeSlot.Spell).SequenceEqual(value.Spells) && value.Slots.Select(nativeSlot => nativeSlot.Available).SequenceEqual(value.Available) &&
                        value.Book.GetAllKnownSpells().SequenceEqual(value.Known) && Enumerable.Range(0, 10).SelectMany(value.Book.GetCustomSpells).SequenceEqual(value.Custom)) &&
                    currentResources.SequenceEqual(resources) && resources.Select((value, index) => source.Caster.Resources.GetResourceAmount(value) == amounts[index]).All(value => value);
                if (!intact || replacements.Count == 0) { Outcome = "preserve-unverified-native-variant-context"; return; }
                var projected = DruidSummonConversionPolicy.Project(originalRows, replacements);
                foreach (var receipt in receipts)
                {
                    receipt.ProjectedRows = projected;
                    foreach (var child in receipt.Children) Receipts.Add(child, receipt);
                }
                __result = projected.ToList(); Outcome = "native-druid-quantity-variants;original-quantities-retained-in-chain;quantities=" + replacements.Count + ";children=" + receipts.Sum(value => value.Children.Length);
            }
            catch (Exception error) { Outcome = "preserve-native-druid-conversion:" + error.GetType().Name; }
        }
    }

    [HarmonyPatch(typeof(MechanicActionBarSlotSpontaneusSpell), "GetConvertedAbilityData")]
    internal static class DruidSummonConversionNativeFactoryPatch
    {
        internal static bool Installed;
        private static bool Prepare() { return DruidSummonConversionVariantPatch.ExactFactory(); }
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var getter = typeof(BlueprintAbility).GetProperty("Variants").GetGetMethod();
            var helper = typeof(DruidSummonConversionVariantPatch).GetMethod("NativeFactoryVariants", BindingFlags.Static | BindingFlags.NonPublic);
            var reads = codes.Where(value => value.opcode == OpCodes.Callvirt && Equals(value.operand, getter)).ToArray();
            if (reads.Length != 2 || helper == null) throw new InvalidOperationException("Installed native Druid variant-factory reads changed.");
            var patched = new List<CodeInstruction>();
            foreach (var code in codes)
            {
                if (reads.Contains(code))
                {
                    if (code.blocks.Count != 0) throw new InvalidOperationException("Unexpected native Druid getter exception boundary.");
                    var self = new CodeInstruction(OpCodes.Ldarg_0); self.labels.AddRange(code.labels); code.labels.Clear(); patched.Add(self);
                    code.opcode = OpCodes.Call; code.operand = helper;
                }
                patched.Add(code);
            }
            Installed = true;
            return patched;
        }
    }

    [HarmonyPatch(typeof(ActionBarGroupSlot), "OnToggleGroupClick")]
    internal static class DruidSummonConversionSourceSlotPatch
    {
        private sealed class Source { internal ActionBarGroupSlot Slot; }
        private static readonly ConditionalWeakTable<ActionBarSpellsGroup, Source> Sources = new ConditionalWeakTable<ActionBarSpellsGroup, Source>();
        private static void Prefix(ActionBarGroupSlot __instance, ActionBarSpellsGroup ___SubGroup)
        {
            if (__instance == null || ___SubGroup == null) return;
            Sources.Remove(___SubGroup); Sources.Add(___SubGroup, new Source { Slot = __instance });
        }
        internal static ActionBarGroupSlot Consume(ActionBarSpellsGroup group)
        { Source source; if (group == null || !Sources.TryGetValue(group, out source)) return null; Sources.Remove(group); return source.Slot; }
    }

    [HarmonyPatch(typeof(ActionBarSpellsGroup), "Toggle", new[] { typeof(Kingmaker.EntitySystem.Entities.UnitEntityData), typeof(IEnumerable<AbilityData>), typeof(AbilityData) })]
    internal static class DruidSummonConversionMenuPatch
    {
        private static void Prefix(ActionBarSpellsGroup __instance, out ActionBarGroupSlot __state)
        { __state = DruidSummonConversionSourceSlotPatch.Consume(__instance); }
        private static void Postfix(ActionBarSpellsGroup __instance, AbilityData sourceSpell, bool ___m_IsActive,
            List<ActionBarSpontaneousConvertedSlot> ___m_Slots, ActionBarGroupSlot __state)
        {
            if (!___m_IsActive || __instance == null || __state == null || sourceSpell == null || ___m_Slots == null ||
                !ReferenceEquals(__state.MechanicSlot?.GetContentData(), sourceSpell) || !__state.gameObject.activeInHierarchy) return;
            var rows = ___m_Slots.Select(value => value?.MechanicSlot?.GetContentData() as AbilityData).ToArray();
            var receipts = rows.Select(DruidSummonConversionVariantPatch.For).Where(value => value != null).Distinct().ToArray();
            if (receipts.Length == 0) return;
            if (!receipts.All(value => value.Current && ReferenceEquals(value.Source, sourceSpell) && ReferenceEquals(__state.MechanicSlot, value.Mechanism) && rows.SequenceEqual(value.ProjectedRows)) ||
                !ReferenceEquals(typeof(ActionBarSpellsGroup).GetField("m_SourceSpell", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance), sourceSpell) ||
                !ReferenceEquals(typeof(ActionBarGroupSlot).GetField("SubGroup", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__state), __instance)) return;
            try
            {
                // The layout snapshot intentionally names the actual prepared
                // source. Original native quantity conversions remain in
                // every projected child's invocation chain, not popup state.
                ExpandedSummoningVariantMenuRuntime.Apply(__instance, ___m_Slots, sourceSpell, __state);
            }
            catch (Exception error)
            { ExpandedSummoningVariantMenuRuntime.RestoreNative(__instance); ExpandedSummoningVariantMenuRuntime.RecordFailure(error); }
        }
    }
}
