using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.BrownFur
{
    internal sealed class BrownFurPlayerIntentInput
    {
        internal bool HasPowerfulChange { get; set; }
        internal bool HasShareTransmutation { get; set; }
        internal bool HasTransmutationSupremacy { get; set; }
        internal IEnumerable<BrownFurAbilityScore> PendingAbilityScores
        { get; set; }
        internal bool ShareTransmutationPending { get; set; }
    }

    internal sealed class BrownFurPlayerIntentDecision
    {
        internal BrownFurPlayerIntentDecision(bool valid, string failure,
            bool ownsBrownFur, bool hasPowerfulChange,
            bool hasShareTransmutation, bool hasTransmutationSupremacy,
            bool powerfulChangeRequested,
            BrownFurAbilityScore selectedAbilityScore,
            bool shareTransmutationRequested)
        {
            Valid = valid;
            Failure = failure ?? string.Empty;
            CasterOwnsBrownFur = ownsBrownFur;
            HasPowerfulChange = hasPowerfulChange;
            HasShareTransmutation = hasShareTransmutation;
            HasTransmutationSupremacy = hasTransmutationSupremacy;
            PowerfulChangeRequested = powerfulChangeRequested;
            SelectedAbilityScore = selectedAbilityScore;
            ShareTransmutationRequested = shareTransmutationRequested;
        }

        internal bool Valid { get; private set; }
        internal string Failure { get; private set; }
        internal bool CasterOwnsBrownFur { get; private set; }
        internal bool HasPowerfulChange { get; private set; }
        internal bool HasShareTransmutation { get; private set; }
        internal bool HasTransmutationSupremacy { get; private set; }
        internal bool PowerfulChangeRequested { get; private set; }
        internal BrownFurAbilityScore SelectedAbilityScore { get; private set; }
        internal bool ShareTransmutationRequested { get; private set; }
    }

    internal static class BrownFurPlayerIntentPolicy
    {
        internal static BrownFurPlayerIntentDecision Decide(
            BrownFurPlayerIntentInput input)
        {
            if (input == null) return Reject("intent-input-missing", false,
                false, false, false, false);
            BrownFurAbilityScore[] selected = (input.PendingAbilityScores ??
                Enumerable.Empty<BrownFurAbilityScore>()).Where(value =>
                    value != BrownFurAbilityScore.None).Distinct().OrderBy(
                    value => (int)value).ToArray();
            bool owns = input.HasPowerfulChange ||
                input.HasShareTransmutation ||
                input.HasTransmutationSupremacy;
            if (selected.Length != 0 && !input.HasPowerfulChange)
                return Reject("powerful-feature-missing", owns,
                    input.HasPowerfulChange, input.HasShareTransmutation,
                    input.HasTransmutationSupremacy,
                    input.ShareTransmutationPending);
            if (input.ShareTransmutationPending &&
                !input.HasShareTransmutation)
                return Reject("share-feature-missing", owns,
                    input.HasPowerfulChange, input.HasShareTransmutation,
                    input.HasTransmutationSupremacy, false);
            if (selected.Length > 1) return Reject(
                "powerful-selection-ambiguous", owns,
                input.HasPowerfulChange, input.HasShareTransmutation,
                input.HasTransmutationSupremacy,
                input.ShareTransmutationPending);
            BrownFurAbilityScore score = selected.Length == 1 ? selected[0] :
                BrownFurAbilityScore.None;
            return new BrownFurPlayerIntentDecision(true, string.Empty, owns,
                input.HasPowerfulChange, input.HasShareTransmutation,
                input.HasTransmutationSupremacy, selected.Length == 1, score,
                input.ShareTransmutationPending);
        }

        private static BrownFurPlayerIntentDecision Reject(string failure,
            bool owns, bool powerful, bool share, bool supremacy,
            bool sharePending)
        {
            return new BrownFurPlayerIntentDecision(false, failure, owns,
                powerful, share, supremacy, false,
                BrownFurAbilityScore.None, sharePending);
        }
    }
}
