using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

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
            if (source.Kind == TeleportCastSourceKind.Scroll)
                return Format(source.Uses == 1 ? text("ScrollCount.Single", "{0} shared scroll") :
                    text("ScrollCount.Plural", "{0} shared scrolls"), source.Uses);
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
        {
            return action.Source.Kind == TeleportCastSourceKind.Scroll ?
                Format(text("ActionTitle.Scroll", "Use {0} Scroll"), SpellName(action.Source.Spell, text)) :
                Format(text("ActionTitle", "Cast {0}"), SpellName(action.Source.Spell, text));
        }
        internal static string Detail(WorldMapPointSpellAction action, Translate text)
        {
            // Material variant identity is part of the choice: scroll rows name
            // the caster level so two same-count variants stay visibly distinct.
            return Format(text("ActionDetail", "{0} · {1}"), Caster(action), Uses(action.Source, text)) +
                (action.Source.Kind == TeleportCastSourceKind.Scroll ?
                    " · " + Format(text("CasterLevel", "CL {0}"), action.Source.CasterLevel.ToString(CultureInfo.InvariantCulture)) : string.Empty);
        }
        internal static string CompactRow(WorldMapPointSpellAction action, Translate text)
        { return Title(action, text) + "\n" + Detail(action, text); }
        internal static string SettlementTeleportLabel(Translate text)
        { return text("SettlementTeleport", "Settlement Teleport"); }
        // A verified successful Greater Teleport arrival announces nothing. Every
        // other result — arrival by ordinary Teleport, rules failures, uncertain
        // expenditure — keeps its actionable player message.
        internal static bool SuppressSuccessAnnouncement(TeleportSpellKind spell, TeleportExecutionStatus status)
        { return spell == TeleportSpellKind.GreaterTeleport && status == TeleportExecutionStatus.Arrived; }
        // Outcome-appropriate arrival sentences. An unnamed destination never
        // substitutes a noun into the "arrived at {destination}" template; the
        // complete sentence comes from its own localization entry.
        internal static string ArrivalMessage(TeleportSpellKind spell, TeleportOutcomeKind outcome,
            string destinationName, Translate text)
        {
            if (!string.IsNullOrWhiteSpace(destinationName))
                return Format(text("Result.Arrival.Named", "{0}: {1}. The party arrived at {2}."),
                    SpellName(spell, text), ArrivalOutcomeLabel(outcome, text), destinationName);
            if (outcome == TeleportOutcomeKind.OnTarget)
                return Format(text("Result.Arrival.TargetLocation", "{0}: On target. The party arrived at the target location."),
                    SpellName(spell, text));
            return Format(text("Result.Arrival.SomewhereElse", "{0}: {1}. The party arrived somewhere else."),
                SpellName(spell, text), ArrivalOutcomeLabel(outcome, text));
        }
        private static string ArrivalOutcomeLabel(TeleportOutcomeKind outcome, Translate text)
        {
            return outcome == TeleportOutcomeKind.OffTarget ? text("Result.OffTarget", "Off target") :
                outcome == TeleportOutcomeKind.SimilarLocation ? text("Result.Similar", "Similar location") :
                text("Result.OnTarget", "On target");
        }
        internal static string Confirmation(WorldMapPointSpellAction action, TeleportFamiliarity familiarity, Translate text)
        {
            // The single-string form joins the same sections the desktop
            // decorator separates with native-looking divider rules.
            var sections = ConfirmationSections(action, familiarity, text);
            return string.Join("\n\n", sections.ToArray()) + "\n";
        }
        // Confirmation groups in display order: casting facts, familiarity,
        // outcome probabilities, then resource cost. Every group keeps its full
        // disclosure; only the grouping is expressed here so the UI can place a
        // restrained separator rule between groups.
        internal static IReadOnlyList<string> ConfirmationSections(WorldMapPointSpellAction action, TeleportFamiliarity familiarity, Translate text)
        {
            string spell = SpellName(action.Source.Spell, text);
            var sections = new List<string> {
                Format(text("Confirmation", "Cast {0}?\n\nCaster: {1}\nDestination: {2}\nAvailable: {3}"),
                    spell, Caster(action), Destination(action.Destination, text), Uses(action.Source, text))
            };
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
                sections.Add(Format(text("FamiliarityGroup", "Familiarity: {0}\nOrdinary visits: {1}"), category, action.Destination.OrdinaryArrivals));
                sections.Add(Format(text("OddsGroup", "On target: {0}%\nOff target: {1}%\nSimilar location: {2}%\nMishap: {3}%"),
                    odds.OnTargetPercent, odds.OffTargetPercent, odds.SimilarLocationPercent, odds.MishapPercent));
            }
            else sections.Add(action.Source.Spell == TeleportSpellKind.GreaterTeleport ?
                text("GreaterExact", "Greater Teleport arrives exactly at the selected world-map point.") :
                text("RecallExact", "Word of Recall returns the party exactly to this world-map point."));
            if (action.Source.Kind == TeleportCastSourceKind.Scroll)
                sections.Add(Format(text("ScrollCasterLevel", "Scroll caster level: {0}."), action.Source.CasterLevel.ToString(CultureInfo.InvariantCulture)) + "\n" +
                    Format(text("ConsumesScroll", "This consumes one {0} scroll and no spell slot."), spell));
            else sections.Add(action.Source.Kind == TeleportCastSourceKind.Prepared ?
                Format(text("ConsumesPrepared", "This consumes one prepared {0}."), spell) :
                Format(text("ConsumesSlot", "This consumes one {0} spell slot."), Level(action.Source.SpellLevel, text)));
            return sections;
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
