using System;
using System.Collections.Generic;
using System.Linq;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UI;
using Kingmaker.UI.ActionBar;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// The cleric alignment restriction on one owned Summon Monster choice.
    /// Native UnitUseAbility.OnAction checks IsAvailable immediately before
    /// spending, so a held selection or saved shortcut is judged at that moment.
    /// </summary>
    [Serializable]
    public sealed class SummonClericAlignmentGate : BlueprintComponent,
        IAbilityVisibilityProvider, IAbilityAvailabilityProvider
    {
        public string CreatureKey;
        public bool MonsterTemplated;

        public bool IsAbilityVisible(AbilityData ability) { return Permits(ability); }

        public bool IsAvailableFor(AbilityData ability) { return Permits(ability); }

        public string GetReason()
        { return "This summon is forbidden by your alignment or your deity's."; }

        private bool Permits(AbilityData ability)
        {
            // The rule concerns spells cast by a cleric; items and non-spellbook
            // abilities keep their native rules.
            if (!SummonClericAlignmentRuntime.ModuleActive || ability == null || ability.Caster == null ||
                ability.Spellbook == null || ability.SourceItem != null) return true;
            return SummonClericAlignmentPolicy.Permits(SummonClericAlignmentRuntime.Decide(
                ability.Caster, SummonClericAlignmentCatalog.For(CreatureKey, MonsterTemplated)));
        }
    }

    internal static class SummonClericAlignmentRuntime
    {
        private static object _library;
        private static Dictionary<BlueprintFeature, string> _deityOptions;

        /// <summary>The repository's own initialized and active module state.</summary>
        internal static bool ModuleActive
        { get { return BlueprintBootstrap.IsInitialized && ExpandedSummoningVisualVariantPatch.IsModuleActive; } }

        internal static SummonClericDecision Decide(UnitDescriptor owner, SummonSpellAlignment spell)
        { return SummonClericAlignmentPolicy.Decide(Read(owner), spell); }

        internal static SummonClericFacts Read(UnitDescriptor owner)
        {
            string[] classes = owner.Progression.Classes.Where(value => value != null &&
                value.CharacterClass != null && value.Level > 0)
                .Select(value => value.CharacterClass.AssetGuid).ToArray();
            if (!classes.Contains(SummonClericAlignmentPolicy.ClericClassGuid, StringComparer.Ordinal))
                return new SummonClericFacts(classes, 0, null);
            Dictionary<BlueprintFeature, string> options = DeityOptions();
            var deities = new List<string>();
            foreach (Feature feature in owner.Progression.Features.Enumerable)
            {
                string guid;
                if (feature != null && feature.Blueprint != null && options.TryGetValue(feature.Blueprint, out guid))
                    deities.Add(guid);
            }
            return new SummonClericFacts(classes, (int)owner.Alignment.Value, deities);
        }

        /// <summary>The cleric rule for one candidate row, shared by menus and execution.</summary>
        internal static bool PermitsRow(BlueprintAbility row, UnitDescriptor caster, Spellbook book)
        {
            SummonClericAlignmentGate gate = (row.ComponentsArray ?? new BlueprintComponent[0]).OfType<SummonClericAlignmentGate>().FirstOrDefault();
            if (gate != null) return gate.IsAbilityVisible(new AbilityData(row, book));
            SummonClericDecision? decision = DecideGovernedRow(row, caster);
            return decision == null || SummonClericAlignmentPolicy.Permits(decision.Value);
        }

        /// <summary>The rule for a row with no owned gate: a retained native row
        /// or a verified third-party contract. Null means this feature has no
        /// opinion on the row. Menus and execution both use this.</summary>
        internal static SummonClericDecision? DecideGovernedRow(BlueprintAbility row, UnitDescriptor caster)
        {
            SummonNativeOptionSpec native;
            if (GovernedNativeRows().TryGetValue(row, out native)) return DecideNative(caster, native);
            SummonForeignRowResult foreign = SummonForeignRowRuntime.Resolve(row);
            if (foreign == null || foreign.Outcome == SummonForeignContractOutcome.Unresolved) return null;
            SummonClericFacts facts = Read(caster);
            if (!SummonClericAlignmentPolicy.IsCleric(facts)) return SummonClericDecision.NotCleric;
            return SummonClericAlignmentPolicy.Decide(facts, foreign.AlignmentFor(SummonClericAlignmentPolicy.AxesOf(facts.AlignmentValue)));
        }

        internal static SummonClericDecision DecideNative(UnitDescriptor owner, SummonNativeOptionSpec native)
        {
            SummonClericFacts facts = Read(owner);
            if (!SummonClericAlignmentPolicy.IsCleric(facts)) return SummonClericDecision.NotCleric;
            return SummonClericAlignmentPolicy.Decide(facts, SummonClericAlignmentCatalog.ForNativeOption(native, SummonClericAlignmentPolicy.AxesOf(facts.AlignmentValue)));
        }

        private static object _nativeLibrary;
        private static Dictionary<BlueprintAbility, SummonNativeOptionSpec> _nativeRows;

        /// <summary>Exact native Summon Monster quantity rows above tier I.</summary>
        internal static Dictionary<BlueprintAbility, SummonNativeOptionSpec> GovernedNativeRows()
        {
            LibraryScriptableObject library = BlueprintBootstrap.Library;
            if (_nativeRows == null || !ReferenceEquals(_nativeLibrary, library))
            {
                var rows = new Dictionary<BlueprintAbility, SummonNativeOptionSpec>();
                foreach (SummonNativeOptionSpec native in SummonNativeOptionCatalog.All.Where(value => value.Family == SummonFamily.Monster && value.Tier >= 2))
                {
                    BlueprintScriptableObject found;
                    if (library != null && library.BlueprintsByAssetId.TryGetValue(native.Guid, out found) && found is BlueprintAbility)
                        rows[(BlueprintAbility)found] = native;
                }
                _nativeRows = rows;
                _nativeLibrary = library;
            }
            return _nativeRows;
        }

        private static Dictionary<BlueprintFeature, string> DeityOptions()
        {
            LibraryScriptableObject library = BlueprintBootstrap.Library;
            if (_deityOptions == null || !ReferenceEquals(_library, library))
            {
                var options = new Dictionary<BlueprintFeature, string>();
                BlueprintFeatureSelection selection = BlueprintLibraryLookup.RequireExact<BlueprintFeatureSelection>(
                    library, SummonClericAlignmentPolicy.DeitySelectionGuid, "native deity selection");
                foreach (BlueprintFeature feature in selection.AllFeatures.Where(value => value != null))
                    options[feature] = feature.AssetGuid;
                _deityOptions = options;
                _library = library;
            }
            return _deityOptions;
        }
    }

    /// <summary>Loads a third-party row's real components and graph for its contract.</summary>
    internal static class SummonForeignRowRuntime
    {
        private static readonly SummonGovernedRowCache<BlueprintAbility, SummonForeignRowResult> Results =
            new SummonGovernedRowCache<BlueprintAbility, SummonForeignRowResult>();

        /// <summary>Null for any row that is not a known third-party identity.</summary>
        internal static SummonForeignRowResult Resolve(BlueprintAbility row)
        {
            if (row == null || !SummonForeignRowClassifier.IsKnownIdentity(row.AssetGuid)) return null;
            return Results.Get(BlueprintBootstrap.Library, row, Compute);
        }

        private static SummonForeignRowResult Compute(BlueprintAbility row)
        {
            try { return SummonForeignRowClassifier.Classify(Read(row)); }
            catch (Exception error) { return SummonForeignRowResult.Unresolved(null, "reading-failed:" + error.GetType().Name + ":" + error.Message); }
        }

        internal static SummonForeignRowReading Read(BlueprintAbility row)
        {
            BlueprintComponent[] components = row.ComponentsArray ?? new BlueprintComponent[0];
            return new SummonForeignRowReading {
                Identity = row.AssetGuid, InternalName = row.name, PublishedTier = PublishedTier(row),
                RequiredFacts = components.OfType<AbilityCasterHasFacts>().SelectMany(value => value.Facts ?? new BlueprintUnitFact[0])
                    .Select(value => value == null ? "null" : value.AssetGuid).ToArray(),
                VisibilityFacts = components.OfType<AbilityShowIfCasterHasFact>().Select(value => value.UnitFact == null ? "null" : value.UnitFact.AssetGuid).ToArray(),
                LiveDescriptors = LiveAxes(row), Graph = SummonSignatureReader.Read(row) };
        }

        private static int PublishedTier(BlueprintAbility row)
        {
            for (int tier = 1; tier <= 9; tier++)
            {
                BlueprintAbility parent = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                    ExpandedSummoningPublisher.CanonicalParentGuids[tier - 1], "published Summon Monster parent");
                if ((parent.Variants ?? new BlueprintAbility[0]).Any(value => ReferenceEquals(value, row))) return tier;
            }
            return 0;
        }

        internal static SummonAlignmentAxes LiveAxes(BlueprintAbility ability)
        {
            SpellDescriptorComponent component = (ability.ComponentsArray ?? new BlueprintComponent[0]).OfType<SpellDescriptorComponent>().FirstOrDefault();
            SpellDescriptor value = component == null ? SpellDescriptor.None : component.Descriptor.Value;
            SummonAlignmentAxes axes = SummonAlignmentAxes.None;
            if ((value & SpellDescriptor.Good) != 0) axes |= SummonAlignmentAxes.Good;
            if ((value & SpellDescriptor.Evil) != 0) axes |= SummonAlignmentAxes.Evil;
            if ((value & SpellDescriptor.Law) != 0) axes |= SummonAlignmentAxes.Law;
            if ((value & SpellDescriptor.Chaos) != 0) axes |= SummonAlignmentAxes.Chaos;
            return axes;
        }
    }

    /// <summary>
    /// A retained native quantity ability or a third-party row is an unowned
    /// blueprint, so it is judged here by exact identity and the branch this
    /// caster would summon. Native UnitUseAbility.OnAction reads this before
    /// spending. It can only remove availability, and only with the module active.
    /// </summary>
    [HarmonyPatch(typeof(AbilityData), "IsAvailableForCast", MethodType.Getter)]
    internal static class SummonClericNativeRowAvailabilityPatch
    {
        private static readonly Func<AbilityData, SummonClericDecision?> Decide = ability =>
            ability == null || ability.Spellbook == null || ability.SourceItem != null || ability.Caster == null || ability.Blueprint == null ? (SummonClericDecision?)null :
                SummonClericAlignmentRuntime.DecideGovernedRow(ability.Blueprint, ability.Caster);

        private static void Postfix(AbilityData __instance, ref bool __result)
        { __result = SummonClericModuleBoundary.Available(SummonClericAlignmentRuntime.ModuleActive, __result, __instance, Decide); }
    }

    /// <summary>
    /// The native popup does not consult visibility when it lists a spell's
    /// variants, so forbidden rows are removed from the popup's own widgets.
    /// The shared blueprint array is never changed.
    /// </summary>
    [HarmonyPatch(typeof(ActionBarSpellsGroup), "Fill", new[] { typeof(Kingmaker.EntitySystem.Entities.UnitEntityData),
        typeof(IEnumerable<AbilityData>), typeof(AbilityData) })]
    internal static class SummonClericAlignmentMenuPatch
    {
        private static bool Prepare() { return DruidSummonConversionPopupRowsPatch.ExactVariantFill(); }

        private static void Postfix(List<ActionBarSpontaneousConvertedSlot> ___m_Slots)
        {
            if (!SummonClericAlignmentRuntime.ModuleActive || ___m_Slots == null) return;
            for (int index = ___m_Slots.Count - 1; index >= 0; index--)
            {
                ActionBarSpontaneousConvertedSlot slot = ___m_Slots[index];
                AbilityData data = slot == null || slot.MechanicSlot == null ? null : slot.MechanicSlot.GetContentData() as AbilityData;
                if (data == null || data.Blueprint == null || data.Caster == null || data.Spellbook == null || data.SourceItem != null ||
                    SummonClericAlignmentRuntime.PermitsRow(data.Blueprint, data.Caster, data.Spellbook)) continue;
                WidgetFactory.DisposeWidget(slot);
                ___m_Slots.RemoveAt(index);
            }
        }
    }
}
