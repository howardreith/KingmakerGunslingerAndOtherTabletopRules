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
