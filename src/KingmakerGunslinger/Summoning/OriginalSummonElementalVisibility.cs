using System;
using System.Collections.Generic;
using System.Linq;
using Harmony12;
using Kingmaker.UI.Common;
using Kingmaker.UI.ServiceWindow;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Owner directive (2026-11-12): while Expanded Summoning is active, the
    /// original Summon Elemental spells are folded into Summon Monster and
    /// Summon Nature's Ally and do not appear in the player's spell learning or
    /// spellbook lists. The UMM option "Show original Summon Elemental spells"
    /// reveals them as ordinary native rows, with no disclosure or header.
    /// Presentation only: blueprints, spell lists, known spells, preparations,
    /// action-bar slots and saves are never changed, so the option is reversible
    /// at any time and applies immediately.
    /// </summary>
    internal static class OriginalSummonElementalVisibility
    {
        private static volatile bool _show;
        internal static string BookOutcome = "not-used";

        /// <summary>The UMM option. Default false (hidden).</summary>
        internal static bool Show { get { return _show; } set { _show = value; } }

        /// <summary>Rows are hidden only while the module that replaces them is active.</summary>
        internal static bool Hidden { get { return !_show && ElementalSourceChooserRuntime.Active; } }

        /// <summary>
        /// Native spellbook rows (known + special + custom). A special (domain or
        /// other granted) entry is kept: its slot may have no other choice.
        /// </summary>
        internal static List<AbilityData> FilterBookRows(List<AbilityData> rows, Spellbook book, int level, string surface)
        {
            if (rows == null || book == null || !Hidden || level < 0 || level > 9) return rows;
            try
            {
                var special = book.GetSpecialSpells(level).ToList();
                int hidden;
                var result = StandaloneElementalPresentationPolicy.HideOriginalBookRows(rows,
                    value => value.Blueprint == null ? null : value.Blueprint.AssetGuid,
                    value => special.Any(entry => ReferenceEquals(entry, value) ||
                        (entry != null && ReferenceEquals(entry.Blueprint, value.Blueprint))), out hidden);
                BookOutcome = surface + ":hidden=" + hidden;
                return result;
            }
            catch (Exception error)
            { BookOutcome = surface + ":preserve-native-rows:" + error.GetType().Name; return rows; }
        }
    }

    // PC spellbook: the native page list, page count and refresh all read this.
    [HarmonyPatch(typeof(SpellBookView), "GetSpellsForLevel")]
    internal static class OriginalSummonElementalPcBookPatch
    {
        private static bool Prepare()
        { return ElementalConsoleBookPresentation.Exact(typeof(SpellBookView).GetMethod("GetSpellsForLevel", new[] { typeof(int) }), 84); }
        private static void Postfix(SpellBookView __instance, int level, ref List<AbilityData> __result)
        { __result = OriginalSummonElementalVisibility.FilterBookRows(__result, __instance.CurrentSpellbook, level, "pc-book"); }
    }

    // Controller spellbook known-spell list and memorizing panel.
    [HarmonyPatch(typeof(UIUtilityUnit), "GetKnownSpellsForLevel")]
    internal static class OriginalSummonElementalConsoleBookPatch
    {
        private static bool Prepare()
        { return ElementalConsoleBookPresentation.Exact(typeof(UIUtilityUnit).GetMethod("GetKnownSpellsForLevel", new[] { typeof(int), typeof(Spellbook) }), 68); }
        private static void Postfix(int level, Spellbook spellbook, ref List<AbilityData> __result)
        { __result = OriginalSummonElementalVisibility.FilterBookRows(__result, spellbook, level, "console-book"); }
    }
}
