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
                return Format(text("ScrollAvailable", "{0} available"), source.Uses);
            return Format(source.Kind == TeleportCastSourceKind.Prepared ? text("PreparedCount", "{0} prepared") :
                source.Uses == 1 ? text("SlotCount.Single", "{0} {1} slot") : text("SlotCount.Plural", "{0} {1} slots"),
                source.Uses, Level(source.SpellLevel, text));
        }
        internal static string Row(WorldMapPointSpellAction action, Translate text)
        { return action.Source.Kind == TeleportCastSourceKind.Scroll ? Title(action, text) + " (" + Detail(action, text) + ")" :
            Format(text("ActionRow", "{0}  {1} ({2})"), SpellName(action.Source.Spell, text), Caster(action), Uses(action.Source, text)); }
        // Compact two-line desktop rows: the full spell name as the title and
        // caster/cost detail beneath it, so controls stay within the native
        // parchment's inner content width on compact geometries.
        internal static string Title(WorldMapPointSpellAction action, Translate text)
        {
            return action.Source.Kind == TeleportCastSourceKind.Scroll ?
                Format(text("UseScroll", "Use Scroll of {0}"), SpellName(action.Source.Spell, text)) :
                Format(text("ActionTitle", "Cast {0}"), SpellName(action.Source.Spell, text));
        }
        internal static string Detail(WorldMapPointSpellAction action, Translate text)
        {
            if (action.Source.Kind == TeleportCastSourceKind.Scroll)
                return Uses(action.Source, text) + (action.Source.ScrollCost == TeleportScrollCostKind.Reusable ? " · " + text("ScrollReusable", "Reusable") :
                    action.Source.ScrollCost == TeleportScrollCostKind.Charge ? " · " + Format(text("ScrollCharges", "{0} charges each"), action.Source.ScrollCharges) : "") + (action.ScrollVariant > 0 ? " · " +
                    Format(text("ScrollVariant", "Variant {0}: CL {1}, SL {2}"), action.ScrollVariant, action.Source.CasterLevel, action.Source.SpellLevel) : "");
            return Format(text("ActionDetail", "{0} · {1}"), Caster(action), Uses(action.Source, text));
        }

        internal static string CompactRow(WorldMapPointSpellAction action, Translate text)
        { return Title(action, text) + "\n" + Detail(action, text); }
        internal static string SettlementTeleportLabel(Translate text)
        { return text("SettlementTeleport", "Settlement Teleport"); }
        // Verified Greater Teleport and Recall arrivals announce nothing. Every
        // other result — arrival by ordinary Teleport, rules failures, uncertain
        // expenditure — keeps its actionable player message.
        internal static bool SuppressSuccessAnnouncement(TeleportSpellKind spell, TeleportExecutionStatus status)
        { return TeleportBeginPolicy.IsDirect(spell) && status == TeleportExecutionStatus.Arrived; }
        internal static string ScrollActivationFailure(string reader, TeleportSpellKind spell,
            TeleportExpenditure spent, Translate text, bool chargeOnly = false)
        {
            string outcome = spent == TeleportExpenditure.None ?
                text("Result.ScrollUnconsumed", "No scroll was consumed.") :
                spent == TeleportExpenditure.ExactlyOne ?
                (chargeOnly ? text("Result.ScrollChargeConsumed", "One scroll charge was consumed. No teleport occurred.") :
                    text("Result.ScrollConsumed", "One scroll was consumed. No teleport occurred.")) :
                text("Result.ScrollConsumptionUnknown", "Scroll consumption could not be verified. Check your inventory before trying again.");
            return Format(text("Result.ScrollActivationFailed", "{0} failed to activate the Scroll of {1}."),
                reader, SpellName(spell, text)) + "\n" + outcome;
        }
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
                action.Source.Kind == TeleportCastSourceKind.Scroll ?
                    Format(text("ScrollConfirmation", "Use Scroll of {0}?\n\nDestination: {1}\n{2}"), spell,
                        Destination(action.Destination, text), Detail(action, text)) :
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
                    (action.Source.ScrollCost == TeleportScrollCostKind.Reusable ? text("UsesReusableScroll", "This reusable scroll spends no charge or spell slot.") :
                     action.Source.ScrollPreservationPossible ? text("UsesPreservableScroll", "Uses one scroll charge and no spell slot. Preservation effects may prevent consumption.") :
                     action.Source.ScrollCost == TeleportScrollCostKind.Charge ? text("ConsumesScrollCharge", "This consumes one scroll charge and no spell slot.") :
                     Format(text("ConsumesScroll", "This consumes one {0} scroll and no spell slot."), spell)));
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
