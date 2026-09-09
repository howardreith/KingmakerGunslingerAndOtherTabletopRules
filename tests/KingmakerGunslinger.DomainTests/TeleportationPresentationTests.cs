using KingmakerGunslinger.Spells.Teleportation;

namespace KingmakerGunslinger.DomainTests
{
    internal static partial class TeleportationContextTests
    {
        private static string English(string key, string value) { return value; }
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
