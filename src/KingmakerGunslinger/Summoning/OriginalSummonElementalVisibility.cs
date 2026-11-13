using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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
    /// Presentation only: blueprints, spell lists, known/custom spells,
    /// preparations, action-bar slots and saves are never changed, so the
    /// option is reversible at any time. No restart; close and reopen the spell
    /// list to apply. (An open level-up list never rebuilds on a same-list slot
    /// switch; the PC spellbook rebuilds on a level or tab change.)
    /// </summary>
    internal static class OriginalSummonElementalVisibility
    {
        private static volatile bool _show;
        internal static string BookOutcome = "not-used";
        internal static string FavoritesOutcome = "not-used";
        internal static string FavoritesSeamState = "not-installed";
        private static int _favoritesHidden;

        /// <summary>The UMM option. Default false (hidden).</summary>
        internal static bool Show { get { return _show; } set { _show = value; } }

        /// <summary>Rows are hidden only while the module that replaces them is active.</summary>
        internal static bool Hidden { get { return StandaloneElementalPresentationPolicy.OriginalRowsHidden(_show, ElementalSourceChooserRuntime.Active); } }

        /// <summary>
        /// Native spellbook rows (known + special + custom). An entry whose
        /// blueprint is a special (domain or other granted) spell of this book at
        /// any level is kept, including its metamagic copies stored at a higher
        /// level: that slot may have no other choice.
        /// </summary>
        internal static List<AbilityData> FilterBookRows(List<AbilityData> rows, Spellbook book, int level, string surface)
        {
            if (rows == null || book == null || !Hidden || level < 0 || level > 9) return rows;
            try
            {
                int hidden;
                var result = Hide(rows, book, out hidden);
                BookOutcome = surface + ":hidden=" + hidden;
                return result;
            }
            catch (Exception error)
            { BookOutcome = surface + ":preserve-native-rows:" + error.GetType().Name; return rows; }
        }

        private static List<AbilityData> Hide(IEnumerable<AbilityData> rows, Spellbook book, out int hidden)
        {
            var special = new HashSet<object>(Enumerable.Range(0, 10).SelectMany(value => book.GetSpecialSpells(value))
                .Where(value => value != null && value.Blueprint != null).Select(value => (object)value.Blueprint));
            return StandaloneElementalPresentationPolicy.HideOriginalBookRows(rows,
                value => value.Blueprint == null ? null : value.Blueprint.AssetGuid,
                value => value.Blueprint != null && special.Contains(value.Blueprint), out hidden);
        }

        /// <summary>
        /// PC metamagic ("favorites") tab: replaces only the exact native
        /// GetCustomSpells(level) read inside SpellBookView.SetUpFavorite. The
        /// native enumeration is read first and returned unchanged unless the
        /// option hides original rows; custom spell data is never written.
        /// </summary>
        internal static IEnumerable<AbilityData> FavoriteCustomSpells(Spellbook book, int level)
        {
            IEnumerable<AbilityData> rows = book.GetCustomSpells(level);
            if (level == 0) _favoritesHidden = 0;
            if (!Hidden) { FavoritesOutcome = "pc-favorites:revealed-or-module-off"; return rows; }
            try
            {
                int hidden;
                var result = Hide(rows, book, out hidden);
                _favoritesHidden += hidden;
                FavoritesOutcome = "pc-favorites:hidden=" + _favoritesHidden;
                return result;
            }
            catch (Exception error)
            { FavoritesOutcome = "pc-favorites:preserve-native-rows:" + error.GetType().Name; return rows; }
        }

        internal static IEnumerable<CodeInstruction> RewriteFavoriteRead(IEnumerable<CodeInstruction> source)
        {
            var body = source.ToList();
            MethodInfo read = typeof(Spellbook).GetMethod("GetCustomSpells", new[] { typeof(int) });
            var calls = body.Where(value => (value.opcode == OpCodes.Call || value.opcode == OpCodes.Callvirt) &&
                Equals(value.operand, read)).ToArray();
            if (calls.Length != 1 || calls[0].blocks.Count != 0)
            { FavoritesSeamState = "preserve-getcustomspells-call-shape-changed"; return body; }
            // Stack (Spellbook, int) is unchanged; only the callee differs.
            calls[0].opcode = OpCodes.Call;
            calls[0].operand = typeof(OriginalSummonElementalVisibility).GetMethod("FavoriteCustomSpells",
                BindingFlags.Static | BindingFlags.NonPublic);
            FavoritesSeamState = "installed-exact-favorites-call-copy";
            return body;
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

    // PC metamagic/favorites tab: one exact GetCustomSpells call-site copy.
    [HarmonyPatch(typeof(SpellBookView), "SetUpFavorite")]
    internal static class OriginalSummonElementalPcFavoritesPatch
    {
        private static bool Prepare()
        {
            bool exact = ElementalConsoleBookPresentation.Exact(typeof(SpellBookView).GetMethod("SetUpFavorite", Type.EmptyTypes), 203);
            if (!exact) OriginalSummonElementalVisibility.FavoritesSeamState = "preserve-unverified-native-method";
            return exact;
        }
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        { return OriginalSummonElementalVisibility.RewriteFavoriteRead(instructions); }
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
