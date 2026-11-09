using System;
using System.Collections.Generic;
using System.Linq;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.UI;
using Kingmaker.UI.ActionBar;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
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
            if (ability == null || ability.Caster == null || ability.Spellbook == null ||
                ability.SourceItem != null) return true;
            return SummonClericAlignmentPolicy.Permits(SummonClericAlignmentRuntime.Decide(
                ability.Caster, SummonClericAlignmentCatalog.For(CreatureKey, MonsterTemplated)));
        }
    }

    internal static class SummonClericAlignmentRuntime
    {
        private static object _library;
        private static Dictionary<BlueprintFeature, string> _deityOptions;

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
            SummonNativeOptionSpec native;
            return !GovernedNativeRows().TryGetValue(row, out native) || SummonClericAlignmentPolicy.Permits(DecideNative(caster, native));
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

    /// <summary>
    /// A retained native quantity ability is an unowned shared blueprint, so it
    /// is judged here by exact identity and the branch this caster would summon.
    /// Native UnitUseAbility.OnAction reads this before spending.
    /// </summary>
    [HarmonyPatch(typeof(AbilityData), "IsAvailableForCast", MethodType.Getter)]
    internal static class SummonClericNativeRowAvailabilityPatch
    {
        private static void Postfix(AbilityData __instance, ref bool __result)
        {
            if (!__result || __instance == null || __instance.Spellbook == null || __instance.SourceItem != null || __instance.Caster == null ||
                __instance.Blueprint == null) return;
            SummonNativeOptionSpec native;
            if (SummonClericAlignmentRuntime.GovernedNativeRows().TryGetValue(__instance.Blueprint, out native) &&
                !SummonClericAlignmentPolicy.Permits(SummonClericAlignmentRuntime.DecideNative(__instance.Caster, native)))
                __result = false;
        }
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
            if (___m_Slots == null) return;
            for (int index = ___m_Slots.Count - 1; index >= 0; index--)
            {
                ActionBarSpontaneousConvertedSlot slot = ___m_Slots[index];
                AbilityData data = slot == null || slot.MechanicSlot == null ? null : slot.MechanicSlot.GetContentData() as AbilityData;
                SummonClericAlignmentGate gate = data == null || data.Blueprint == null ? null :
                    (data.Blueprint.ComponentsArray ?? new BlueprintComponent[0]).OfType<SummonClericAlignmentGate>().FirstOrDefault();
                if (gate == null || gate.IsAbilityVisible(data)) continue;
                WidgetFactory.DisposeWidget(slot);
                ___m_Slots.RemoveAt(index);
            }
        }
    }
}
