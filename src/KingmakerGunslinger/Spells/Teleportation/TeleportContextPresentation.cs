using System;
using System.Globalization;

namespace KingmakerGunslinger.Spells.Teleportation
{
    internal static class TeleportContextPresentation
    {
        internal delegate string Translate(string key, string english);
        internal static string SpellName(TeleportSpellKind spell, Translate text)
        {
            switch (spell) {
                case TeleportSpellKind.Teleport: return text("Teleport.Name", "Teleport");
                case TeleportSpellKind.GreaterTeleport: return text("GreaterTeleport.Name", "Greater Teleport");
                case TeleportSpellKind.WordOfRecall: return text("WordofRecall.Name", "Word of Recall");
                default: throw new ArgumentOutOfRangeException("spell");
            }
        }
        internal static string Destination(TeleportDestinationSnapshot point, Translate text)
        {
            if (!string.IsNullOrWhiteSpace(point.Name)) return point.Name;
            return point.Kind == TeleportPointKind.Waypoint || point.Kind == TeleportPointKind.SystemWaypoint ?
                text("SelectedCrossroads", "Selected crossroads") : text("SelectedPoint", "Selected world-map point");
        }
        internal static string Caster(WorldMapPointSpellAction action)
        { return action.Source.CasterName + (action.ShowBook ? ", " + action.Source.BookName : string.Empty); }
        internal static string Uses(TeleportCastSourceSnapshot source, Translate text)
        {
            return Format(source.Kind == TeleportCastSourceKind.Prepared ? text("PreparedCount", "{0} prepared") :
                source.Uses == 1 ? text("SlotCount.Single", "{0} {1} slot") : text("SlotCount.Plural", "{0} {1} slots"),
                source.Uses, Level(source.SpellLevel, text));
        }
        internal static string Row(WorldMapPointSpellAction action, Translate text)
        { return Format(text("ActionRow", "{0}  {1} ({2})"), SpellName(action.Source.Spell, text), Caster(action), Uses(action.Source, text)); }
        // Compact two-line desktop rows: the full spell name as the title and
        // caster/cost detail beneath it, so controls stay within the native
        // parchment's inner content width on compact geometries.
        internal static string Title(WorldMapPointSpellAction action, Translate text)
        { return Format(text("ActionTitle", "Cast {0}"), SpellName(action.Source.Spell, text)); }
        internal static string Detail(WorldMapPointSpellAction action, Translate text)
        { return Format(text("ActionDetail", "{0} · {1}"), Caster(action), Uses(action.Source, text)); }
        internal static string CompactRow(WorldMapPointSpellAction action, Translate text)
        { return Title(action, text) + "\n" + Detail(action, text); }
        internal static string SettlementTeleportLabel(Translate text)
        { return text("SettlementTeleport", "Settlement Teleport"); }
        internal static string Confirmation(WorldMapPointSpellAction action, TeleportFamiliarity familiarity, Translate text)
        {
            string spell = SpellName(action.Source.Spell, text);
            string value = Format(text("Confirmation", "Cast {0}?\n\nCaster: {1}\nDestination: {2}\nAvailable: {3}\n\n"),
                spell, Caster(action), Destination(action.Destination, text), Uses(action.Source, text));
            if (action.Source.Spell == TeleportSpellKind.Teleport)
            {
                var odds = TeleportRollTable.For(familiarity);
                string category;
                switch (familiarity) {
                    case TeleportFamiliarity.VeryFamiliar: category = text("Familiarity.VeryFamiliar", "Very familiar"); break;
                    case TeleportFamiliarity.StudiedCarefully: category = text("Familiarity.StudiedCarefully", "Studied carefully"); break;
                    case TeleportFamiliarity.SeenCasually: category = text("Familiarity.SeenCasually", "Seen casually"); break;
                    default: category = text("Familiarity.ViewedOnce", "Viewed once"); break;
                }
                value += Format(text("Odds", "Familiarity: {0}\nOrdinary visits: {1}\n\nOn target: {2}%\nOff target: {3}%\nSimilar location: {4}%\nMishap: {5}%\n\n"),
                    category, action.Destination.OrdinaryArrivals, odds.OnTargetPercent, odds.OffTargetPercent,
                    odds.SimilarLocationPercent, odds.MishapPercent);
            }
            else value += action.Source.Spell == TeleportSpellKind.GreaterTeleport ?
                text("GreaterExact", "Greater Teleport arrives exactly at the selected world-map point.\n\n") :
                text("RecallExact", "Word of Recall returns the party exactly to this world-map point.\n\n");
            return value + (action.Source.Kind == TeleportCastSourceKind.Prepared ?
                Format(text("ConsumesPrepared", "This consumes one prepared {0}."), spell) :
                Format(text("ConsumesSlot", "This consumes one {0} spell slot."), Level(action.Source.SpellLevel, text)));
        }
        private static string Level(int level, Translate text)
        {
            string[] levels = { "", "first-level", "second-level", "third-level", "fourth-level", "fifth-level", "sixth-level", "seventh-level", "eighth-level", "ninth-level" };
            if (level < 1 || level > 9) throw new ArgumentOutOfRangeException("level");
            return text("Level." + level.ToString(CultureInfo.InvariantCulture), levels[level]);
        }
        private static string Format(string format, params object[] values)
        { return string.Format(CultureInfo.CurrentCulture, format, values); }
    }
}
