using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.ActivatableAbilities;
using KingmakerGunslinger.Blueprints;

namespace KingmakerGunslinger.BrownFur
{
    internal static class BrownFurPlayerIntentRuntime
    {
        private static readonly BrownFurAbilityScore[] Scores = {
            BrownFurAbilityScore.Strength, BrownFurAbilityScore.Dexterity,
            BrownFurAbilityScore.Constitution, BrownFurAbilityScore.Intelligence,
            BrownFurAbilityScore.Wisdom, BrownFurAbilityScore.Charisma };

        internal static BrownFurPlayerIntentDecision Observe(
            UnitDescriptor owner, BrownFurBlueprintSet blueprints)
        {
            if (owner == null || blueprints == null)
                return BrownFurPlayerIntentPolicy.Decide(null);
            if (blueprints.ScoreBuffs == null ||
                blueprints.ScoreBuffs.Length != Scores.Length ||
                blueprints.ScoreActivatables == null ||
                blueprints.ScoreActivatables.Length != Scores.Length)
                throw new InvalidOperationException(
                    "Brown-Fur score intent blueprints are incomplete.");
            var pending = new List<BrownFurAbilityScore>();
            for (int index = 0; index < Scores.Length; index++)
            {
                ActivatableAbility activatable = Find(owner,
                    blueprints.ScoreActivatables[index]);
                bool marker = blueprints.ScoreBuffs[index] != null &&
                    owner.HasFact(blueprints.ScoreBuffs[index]);
                bool active = activatable != null && activatable.IsOn;
                if (marker != active)
                {
                    if (activatable != null && active) activatable.IsOn = false;
                    if (marker) owner.RemoveFact(blueprints.ScoreBuffs[index]);
                    active = false;
                }
                if (active)
                    pending.Add(Scores[index]);
            }
            ActivatableAbility share = Find(owner,
                blueprints.ShareTransmutationAbility);
            bool shareMarker = blueprints.ShareTransmutationBuff != null &&
                owner.HasFact(blueprints.ShareTransmutationBuff);
            bool shareActive = share != null && share.IsOn;
            if (shareMarker != shareActive)
            {
                if (share != null && shareActive) share.IsOn = false;
                if (shareMarker)
                    owner.RemoveFact(blueprints.ShareTransmutationBuff);
                shareActive = false;
            }
            return BrownFurPlayerIntentPolicy.Decide(
                new BrownFurPlayerIntentInput {
                    HasPowerfulChange = blueprints.PowerfulChange != null &&
                        owner.HasFact(blueprints.PowerfulChange),
                    HasShareTransmutation =
                        blueprints.ShareTransmutation != null &&
                        owner.HasFact(blueprints.ShareTransmutation),
                    HasTransmutationSupremacy =
                        blueprints.TransmutationSupremacy != null &&
                        owner.HasFact(blueprints.TransmutationSupremacy),
                    PendingAbilityScores = pending,
                    ShareTransmutationPending = shareActive
                });
        }

        internal static void Consume(UnitDescriptor owner,
            BrownFurBlueprintSet blueprints, BrownFurCastDecision decision)
        {
            if (owner == null || blueprints == null || decision == null) return;
            if (decision.PowerfulChange &&
                decision.SelectedAbilityScore != BrownFurAbilityScore.None)
            {
                int index = Array.IndexOf(Scores,
                    decision.SelectedAbilityScore);
                if (index >= 0 && blueprints.ScoreActivatables != null &&
                    index < blueprints.ScoreActivatables.Length)
                {
                    ActivatableAbility score = Find(owner,
                        blueprints.ScoreActivatables[index]);
                    if (score != null && score.IsOn) score.IsOn = false;
                    if (blueprints.ScoreBuffs != null &&
                        index < blueprints.ScoreBuffs.Length &&
                        blueprints.ScoreBuffs[index] != null &&
                        owner.HasFact(blueprints.ScoreBuffs[index]))
                        owner.RemoveFact(blueprints.ScoreBuffs[index]);
                }
            }
            if (decision.ShareTransmutation)
            {
                ActivatableAbility share = Find(owner,
                    blueprints.ShareTransmutationAbility);
                if (share != null && share.IsOn) share.IsOn = false;
                if (blueprints.ShareTransmutationBuff != null &&
                    owner.HasFact(blueprints.ShareTransmutationBuff))
                    owner.RemoveFact(blueprints.ShareTransmutationBuff);
            }
        }

        internal static void Clear(UnitDescriptor owner,
            BrownFurBlueprintSet blueprints)
        {
            if (owner == null || blueprints == null) return;
            foreach (BlueprintActivatableAbility blueprint in
                (blueprints.ScoreActivatables ??
                    new BlueprintActivatableAbility[0]).Concat(
                        new[] { blueprints.ShareTransmutationAbility }))
            {
                ActivatableAbility ability = Find(owner, blueprint);
                if (ability != null && ability.IsOn) ability.IsOn = false;
            }
            foreach (var pending in blueprints.ScoreBuffs ??
                new Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff[0])
                if (pending != null && owner.HasFact(pending))
                    owner.RemoveFact(pending);
            if (blueprints.ShareTransmutationBuff != null &&
                owner.HasFact(blueprints.ShareTransmutationBuff))
                owner.RemoveFact(blueprints.ShareTransmutationBuff);
        }

        internal static ActivatableAbility Find(UnitDescriptor owner,
            BlueprintActivatableAbility blueprint)
        {
            if (owner == null || blueprint == null ||
                owner.ActivatableAbilities == null) return null;
            return owner.ActivatableAbilities.Enumerable.SingleOrDefault(value =>
                value != null && ReferenceEquals(value.Blueprint, blueprint));
        }
    }
}
