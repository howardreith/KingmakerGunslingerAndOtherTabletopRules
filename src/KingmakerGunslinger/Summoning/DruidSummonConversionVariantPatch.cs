using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    // Project only its canonical Druid conversion through the existing native
    // variant factory. That factory owns child -> native conversion -> actual
    // preparation links; this adapter never assigns invocation-context fields.
    [HarmonyPatch(typeof(MechanicActionBarSlotMemorizedSpell), "GetConvertedAbilityData")]
    internal static class DruidSummonConversionVariantPatch
    {
        private const string DruidClassGuid = "610d836f3a3a9ed42a4349b62f002e96";
        private static readonly string[] AllyParents = ExpandedSummoningPublisher.CanonicalParentGuids.Skip(9).ToArray();
        internal static string Outcome = "not-used";
        private static readonly ConditionalWeakTable<AbilityData, Receipt> Receipts = new ConditionalWeakTable<AbilityData, Receipt>();
        internal sealed class Receipt
        {
            internal MechanicActionBarSlotMemorizedSpell Mechanism;
            internal AbilityData Source, Conversion;
            internal Spellbook Book;
            internal SpellSlot Slot;
            internal BlueprintAbility Parent;
            internal BlueprintAbility[] Variants, OwnedVariants;
            internal AbilityData[] Children, ProjectedRows;
            internal ElementalInvocationContextSnapshot SourceContext, ConversionContext;
            internal ElementalInvocationContextSnapshot[] ChildContexts;
            internal bool Current { get { return ExpandedSummoningVisualVariantPatch.IsModuleActive && BlueprintBootstrap.IsInitialized &&
                ReferenceEquals(Mechanism.SpellSlot, Slot) && ReferenceEquals(Slot.Spell, Source) && ReferenceEquals(Source.Spellbook, Book) &&
                ReferenceEquals(Conversion.ConvertedFrom, Source) && ReferenceEquals(Conversion.Blueprint, Parent) &&
                SourceContext.Current && ConversionContext.Current && ChildContexts.All(value => value.Current) && Parent.Variants != null && Parent.Variants.SequenceEqual(Variants) &&
                OwnedVariants.All(value => ReferenceEquals(value.Parent, Parent) && Variants.Count(variant => ReferenceEquals(variant, value)) == 1) &&
                Book.Owner.Spellbooks.Any(value => ReferenceEquals(value, Book)) && Children.All(value => ReferenceEquals(value.ConvertedFrom, Conversion) &&
                    ReferenceEquals(value.Spellbook, Book) && Variants.Any(variant => ReferenceEquals(value.Blueprint, variant))); } }
        }
        internal static Receipt For(AbilityData child)
        { Receipt receipt; return child != null && Receipts.TryGetValue(child, out receipt) && receipt.Current ? receipt : null; }

        private static bool Prepare()
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var prepared = typeof(MechanicActionBarSlotMemorizedSpell).GetMethod("GetConvertedAbilityData", flags);
            var variants = typeof(MechanicActionBarSlotSpontaneusSpell).GetMethod("GetConvertedAbilityData", flags);
            return prepared != null && variants != null && prepared.Module.ModuleVersionId.ToString() == ElementalSourceChooserRuntime.GameMvid &&
                prepared.MetadataToken == 0x06002fbc && prepared.GetMethodBody().GetILAsByteArray().Length == 287 &&
                variants.MetadataToken == 0x06002fe4 && variants.GetMethodBody().GetILAsByteArray().Length == 305;
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
                var parents = __result.Where(value => value?.Blueprint != null && ReferenceEquals(value.ConvertedFrom, source) &&
                    ReferenceEquals(value.Spellbook, book) && AllyParents.Contains(value.Blueprint.AssetGuid, StringComparer.Ordinal)).ToArray();
                if (parents.Length != 1) return;
                var conversion = parents[0]; var parent = conversion.Blueprint; int tier = Array.IndexOf(AllyParents, parent.AssetGuid) + 1;
                bool nativeGrant = source.Caster.Progression.Features.Enumerable.Any(feature => feature?.Blueprint != null && (feature.Blueprint.ComponentsArray ?? new BlueprintComponent[0])
                    .OfType<SpontaneousSpellConversion>().Any(component => ReferenceEquals(component.CharacterClass, book.Blueprint.CharacterClass) &&
                        component.SpellsByLevel != null && component.SpellsByLevel.Length >= 10 && Enumerable.Range(1, 9).All(level =>
                            component.SpellsByLevel[level]?.AssetGuid == AllyParents[level - 1])));
                if (!nativeGrant || !ExpandedSummoningPublisher.IsPublishedExpandedParent(parent) ||
                    book.GetSpontaneousConversionSpells(source).Count(value => ReferenceEquals(value, parent)) != 1) return;
                var variants = parent.Variants;
                var expected = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly).Where(value => value.ParentTier == tier && SummonVisibilityCatalog.IsPublished(value))
                    .Select(value => ExpandedSummoningBlueprints.Registered.BySymbol[ExpandedSummoningIdentityCatalog.AbilitySymbol(value)] as BlueprintAbility)
                    .Concat(SummonNativeExpansionCatalog.For(SummonFamily.NaturesAlly, tier)
                        .Select(value => ExpandedSummoningBlueprints.Registered.BySymbol[value.Symbol] as BlueprintAbility)).ToArray();
                // The publisher merges generated + retained-native wrappers
                // with compatible preserved original/foreign variants. Validate
                // our complete owned subset, then project the exact actual
                // parent.Variants through native code without dropping extras.
                if (variants == null || variants.Length == 0 || variants.Any(value => value == null) || expected.Any(value => value == null) ||
                    variants.Select(value => value.AssetGuid).Distinct(StringComparer.Ordinal).Count() != variants.Length ||
                    expected.Any(value => !ReferenceEquals(value.Parent, parent) || variants.Count(variant => ReferenceEquals(variant, value)) != 1) ||
                    __result.Any(value => value != null && variants.Any(variant => ReferenceEquals(variant, value.Blueprint))))
                { Outcome = "preserve-native-or-foreign-incomplete-conversion"; return; }
                var original = __result; var originalRows = original.ToArray(); var books = source.Caster.Spellbooks.ToArray();
                var bookState = books.Select(value => new { Book = value, Slots = Enumerable.Range(0, 10).SelectMany(value.GetMemorizedSpellSlots).ToArray() })
                    .Select(value => new { value.Book, value.Slots, Spells = value.Slots.Select(nativeSlot => nativeSlot.Spell).ToArray(),
                        Available = value.Slots.Select(nativeSlot => nativeSlot.Available).ToArray(), Known = value.Book.GetAllKnownSpells().ToArray(),
                        Custom = Enumerable.Range(0, 10).SelectMany(value.Book.GetCustomSpells).ToArray() }).ToArray();
                bool availability = slot.Available;
                var sourceContext = new ElementalInvocationContextSnapshot(() => References(source), () => Values(source));
                var conversionContext = new ElementalInvocationContextSnapshot(() => References(conversion), () => Values(conversion));
                var resources = new List<BlueprintScriptableObject>();
                foreach (BlueprintScriptableObject resource in source.Caster.Resources) resources.Add(resource);
                var amounts = resources.Select(source.Caster.Resources.GetResourceAmount).ToArray();
                // Native factory, including installed compatible patches. No
                // AbilityData or ConvertedFrom is authored by this adapter.
                var native = new MechanicActionBarSlotSpontaneusSpell { Unit = __instance.Unit, Spell = conversion };
                var produced = native.GetConvertedAbilityData();
                var children = produced == null ? new AbilityData[0] : produced.Where(value => value != null &&
                    variants.Any(variant => ReferenceEquals(variant, value.Blueprint))).ToArray();
                bool complete = children.Length == variants.Length && variants.All(variant => children.Count(value => ReferenceEquals(value.Blueprint, variant)) == 1) &&
                    children.All(value => ReferenceEquals(value.ConvertedFrom, conversion) && ReferenceEquals(value.Spellbook, book) &&
                        ReferenceEquals(value.Caster, source.Caster) && variants.Any(variant => ReferenceEquals(value.Blueprint, variant)));
                var currentResources = new List<BlueprintScriptableObject>();
                foreach (BlueprintScriptableObject resource in source.Caster.Resources) currentResources.Add(resource);
                bool intact = sourceContext.Current && conversionContext.Current && ReferenceEquals(__result, original) && original.SequenceEqual(originalRows) &&
                    source.Caster.Spellbooks.SequenceEqual(books) && ReferenceEquals(__instance.SpellSlot, slot) && ReferenceEquals(slot.Spell, source) && slot.Available == availability &&
                    bookState.All(value => Enumerable.Range(0, 10).SelectMany(value.Book.GetMemorizedSpellSlots).SequenceEqual(value.Slots) &&
                        value.Slots.Select(nativeSlot => nativeSlot.Spell).SequenceEqual(value.Spells) && value.Slots.Select(nativeSlot => nativeSlot.Available).SequenceEqual(value.Available) &&
                        value.Book.GetAllKnownSpells().SequenceEqual(value.Known) && Enumerable.Range(0, 10).SelectMany(value.Book.GetCustomSpells).SequenceEqual(value.Custom)) &&
                    currentResources.SequenceEqual(resources) && resources.Select((value, index) => source.Caster.Resources.GetResourceAmount(value) == amounts[index]).All(value => value);
                if (!complete || !intact) { Outcome = "preserve-unverified-native-variant-context"; return; }
                var projected = new List<AbilityData>();
                foreach (var row in originalRows) { if (ReferenceEquals(row, conversion)) projected.AddRange(children); else projected.Add(row); }
                var receipt = new Receipt { Mechanism = __instance, Source = source, Conversion = conversion, Book = book, Slot = slot,
                    Parent = parent, Variants = variants.ToArray(), OwnedVariants = expected, Children = children, ProjectedRows = projected.ToArray(),
                    SourceContext = sourceContext, ConversionContext = conversionContext,
                    ChildContexts = children.Select(child => new ElementalInvocationContextSnapshot(() => References(child), () => Values(child))).ToArray() };
                foreach (var child in children) Receipts.Add(child, receipt);
                __result = projected; Outcome = "native-druid-conversion-variants;original-parent-retained-in-chain;children=" + children.Length;
            }
            catch (Exception error) { Outcome = "preserve-native-druid-conversion:" + error.GetType().Name; }
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
            if (receipts.Length != 1) return;
            var receipt = receipts[0];
            if (!receipt.Current || !ReferenceEquals(receipt.Source, sourceSpell) || !ReferenceEquals(__state.MechanicSlot, receipt.Mechanism) || !rows.SequenceEqual(receipt.ProjectedRows) ||
                !ReferenceEquals(typeof(ActionBarSpellsGroup).GetField("m_SourceSpell", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance), sourceSpell) ||
                !ReferenceEquals(typeof(ActionBarGroupSlot).GetField("SubGroup", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__state), __instance)) return;
            try
            {
                // The layout snapshot intentionally names the actual prepared
                // source. The canonical SNA parent remains in the receipt's
                // native invocation chain, not substituted into popup state.
                ExpandedSummoningVariantMenuRuntime.Apply(__instance, ___m_Slots, sourceSpell, __state);
            }
            catch (Exception error)
            { ExpandedSummoningVariantMenuRuntime.RestoreNative(__instance); ExpandedSummoningVariantMenuRuntime.RecordFailure(error); }
        }
    }
}
