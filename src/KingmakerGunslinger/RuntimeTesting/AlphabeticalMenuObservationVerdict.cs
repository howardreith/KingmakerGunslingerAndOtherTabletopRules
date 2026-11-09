namespace KingmakerGunslinger.RuntimeTesting
{
    internal enum AlphabeticalSelectionOutcome { Selected, SelectionFailed, NotExercised }

    internal enum AlphabeticalLateRowOutcome { VisibleAndSorted, ExpectedAbsent, NotApplicable, Failed }

    /// <summary>
    /// How the alphabetical-menu observer classifies a row click and a late
    /// other-mod row. An interaction that did not happen is never a success.
    /// </summary>
    internal static class AlphabeticalMenuObservationVerdict
    {
        internal static AlphabeticalSelectionOutcome ClassifySelection(bool available,
            bool selectedExactRow, bool quantityMatches, bool contextMatches)
        {
            if (!available) return AlphabeticalSelectionOutcome.NotExercised;
            return selectedExactRow && quantityMatches && contextMatches ?
                AlphabeticalSelectionOutcome.Selected : AlphabeticalSelectionOutcome.SelectionFailed;
        }

        /// <summary>A gated row is expected only for a caster with its fact.
        /// Its absence for any other caster is correct, but it says nothing
        /// about how the row sorts when it is visible.</summary>
        internal static AlphabeticalLateRowOutcome ClassifyLateRow(bool publishedInParent,
            bool gated, bool casterHasRequiredFact, bool visibleInMenu, bool menuSorted)
        {
            if (!publishedInParent) return AlphabeticalLateRowOutcome.NotApplicable;
            if (gated && !casterHasRequiredFact)
                return visibleInMenu ? AlphabeticalLateRowOutcome.Failed :
                    AlphabeticalLateRowOutcome.ExpectedAbsent;
            return visibleInMenu && menuSorted ? AlphabeticalLateRowOutcome.VisibleAndSorted :
                AlphabeticalLateRowOutcome.Failed;
        }
    }
}
