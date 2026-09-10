using KingmakerGunslinger.Spells.Teleportation;

namespace KingmakerGunslinger.DomainTests
{
    internal static partial class TeleportationContextTests
    {
        private static string English(string key, string value) { return value; }
        private static WorldMapPointSpellAction ScrollRow(int uses)
        {
            return new WorldMapPointSpellAction(Point(), Origin, new TeleportCastSourceSnapshot("reader", 0, "caster-a",
                "0123456789abcdef0123456789abcdef", "Scroll", TeleportSpellKind.Teleport,
                TeleportCastSourceKind.Scroll, 5, uses,
                TeleportCastSourceFacts.ActiveParty | TeleportCastSourceFacts.LivingAvailableCaster |
                TeleportCastSourceFacts.ScrollStock | TeleportCastSourceFacts.ExactSpell |
                TeleportCastSourceFacts.RealResource), false);
        }
        internal static void PreparedRowIncludesCurrentCount()
        {
            var row = new WorldMapPointSpellAction(Point(), Origin, Source(uses: 2), false);
            Assertions.Equal("Teleport  caster-a (2 prepared)", TeleportContextPresentation.Row(row, English), "One summarized prepared source row.");
        }
        internal static void SpontaneousRowsIncludeCorrectLevelAndPlural()
        {
            foreach (int count in new[] { 1, 2 })
            {
                var row = new WorldMapPointSpellAction(Point(), Origin,
                    Source(TeleportSpellKind.GreaterTeleport, uses: count, kind: TeleportCastSourceKind.Spontaneous), false);
                Assertions.Equal("Greater Teleport  caster-a (" + count + " seventh-level slot" + (count == 1 ? ")" : "s)"),
                    TeleportContextPresentation.Row(row, English), "Correct-level spontaneous pool and plural.");
            }
        }
        internal static void AmbiguousCasterRowsNameTheBook()
        {
            var row = new WorldMapPointSpellAction(Point(), Origin, Source(), true);
            Assertions.True(TeleportContextPresentation.Row(row, English).Contains("caster-a, Wizard"), "Distinct book labels disambiguate one caster.");
        }
        internal static void CompactRowUsesTitleAndDetailLines()
        {
            var row = new WorldMapPointSpellAction(Point(), Origin, Source(uses: 2), false);
            Assertions.Equal("Cast Teleport\ncaster-a · 2 prepared", TeleportContextPresentation.CompactRow(row, English),
                "Compact desktop row: full spell name title over caster/cost detail.");
        }
        internal static void CompactSpontaneousRowNamesLevelAndBookWhenAmbiguous()
        {
            var row = new WorldMapPointSpellAction(Point(), Origin,
                Source(TeleportSpellKind.GreaterTeleport, uses: 2, kind: TeleportCastSourceKind.Spontaneous), true);
            Assertions.Equal("Cast Greater Teleport\ncaster-a, Wizard · 2 seventh-level slots", TeleportContextPresentation.CompactRow(row, English),
                "Spontaneous compact row keeps level plural and disambiguating book.");
        }
        internal static void ScrollRowsUseSharedStockWording()
        {
            var scroll = ScrollRow(uses: 3);
            Assertions.Equal("Use Teleport Scroll\ncaster-a \u00b7 3 shared scrolls", TeleportContextPresentation.CompactRow(scroll, English),
                "Scroll compact row: use-scroll title over reader and shared stock.");
            var single = ScrollRow(uses: 1);
            Assertions.Equal("Use Teleport Scroll\ncaster-a \u00b7 1 shared scroll", TeleportContextPresentation.CompactRow(single, English),
                "Singular shared-scroll wording.");
            string confirmation = TeleportContextPresentation.Confirmation(scroll, TeleportFamiliarity.VeryFamiliar, English);
            Assertions.True(confirmation.Contains("This consumes one Teleport scroll and no spell slot."),
                "Scroll confirmation states the one-scroll cost and no slot.");
        }
        internal static void ScrollSourcesNeedNoSpellbookFacts()
        {
            var required = TeleportCastSourceFacts.ActiveParty | TeleportCastSourceFacts.LivingAvailableCaster |
                TeleportCastSourceFacts.ScrollStock | TeleportCastSourceFacts.ExactSpell | TeleportCastSourceFacts.RealResource;
            var scroll = new TeleportCastSourceSnapshot("reader", 0, "reader-a", "0123456789abcdef0123456789abcdef", "Scroll",
                TeleportSpellKind.Teleport, TeleportCastSourceKind.Scroll, 5, 2, required);
            Assertions.True(TeleportCastAvailabilityPolicy.Usable(scroll), "A reader with shared stock is a usable scroll source.");
            var withBook = new TeleportCastSourceSnapshot("reader", 0, "reader-a", "0123456789abcdef0123456789abcdef", "Scroll",
                TeleportSpellKind.Teleport, TeleportCastSourceKind.Scroll, 5, 2, required | TeleportCastSourceFacts.OwnedSpellbook);
            Assertions.False(TeleportCastAvailabilityPolicy.Usable(withBook), "Scroll sources never claim spellbook facts.");
            var noStock = new TeleportCastSourceSnapshot("reader", 0, "reader-a", "0123456789abcdef0123456789abcdef", "Scroll",
                TeleportSpellKind.Teleport, TeleportCastSourceKind.Scroll, 5, 2,
                required & ~TeleportCastSourceFacts.ScrollStock);
            Assertions.False(TeleportCastAvailabilityPolicy.Usable(noStock), "Scroll sources require the shared stock fact.");
            Assertions.Equal("reader/0123456789abcdef0123456789abcdef/" + ((int)TeleportSpellKind.Teleport).ToString(System.Globalization.CultureInfo.InvariantCulture),
                scroll.Key, "Scroll source key binds reader and scroll identity.");
        }
        internal static void SettlementLabelDistinguishesNativeTeleport()
        { Assertions.Equal("Settlement Teleport", TeleportContextPresentation.SettlementTeleportLabel(English), "Native settlement label text."); }
        internal static void ConfirmationShowsExactOddsAndOrdinaryCount()
        {
            var row = new WorldMapPointSpellAction(Point(visits: 3), Origin, Source(), false);
            string value = TeleportContextPresentation.Confirmation(row, TeleportFamiliarity.StudiedCarefully, English);
            foreach (string expected in new[] { "Cast Teleport?", "Caster: caster-a", "Destination: A displayed name", "Available: 1 prepared",
                "Familiarity: Studied carefully", "Ordinary visits: 3", "On target: 94%", "Off target: 3%", "Similar location: 2%", "Mishap: 1%",
                "This consumes one prepared Teleport." }) Assertions.True(value.Contains(expected), "Selected source/destination/table evidence: " + expected);
        }
        internal static void ExactSpellConfirmationHasNoDestinationOdds()
        {
            var row = ActionRow(TeleportSpellKind.GreaterTeleport, TeleportCastSourceKind.Spontaneous);
            string value = TeleportContextPresentation.Confirmation(row, TeleportFamiliarity.Unvisited, English);
            Assertions.True(value.Contains("arrives exactly at the selected world-map point") && value.Contains("one seventh-level spell slot"), "Exact arrival and real resource statement.");
            Assertions.False(value.Contains("On target:") || value.Contains("Mishap:"), "Exact spells never request a d100 familiarity table.");
        }
        internal static void RecallConfirmationNamesSelectedDestination()
        {
            var row = new WorldMapPointSpellAction(Point(WordOfRecallDestinationPolicy.OlegId), Origin,
                Source(TeleportSpellKind.WordOfRecall), false);
            string value = TeleportContextPresentation.Confirmation(row, TeleportFamiliarity.VeryFamiliar, English);
            Assertions.True(value.Contains("Cast Word of Recall?") && value.Contains("Destination: A displayed name") &&
                value.Contains("one prepared Word of Recall"), "Confirmation binds the resolved native point and prepared Recall use.");
        }
    }
}
