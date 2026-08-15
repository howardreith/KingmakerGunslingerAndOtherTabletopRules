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
                blueprints.ScoreBuffs.Length != Scores.Length)
                throw new InvalidOperationException(
                    "Brown-Fur score intent blueprints are incomplete.");
            var pending = new List<BrownFurAbilityScore>();
            for (int index = 0; index < Scores.Length; index++)
                if (blueprints.ScoreBuffs[index] != null &&
                    owner.HasFact(blueprints.ScoreBuffs[index]))
                    pending.Add(Scores[index]);
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
                    ShareTransmutationPending =
                        blueprints.ShareTransmutationBuff != null &&
                        owner.HasFact(blueprints.ShareTransmutationBuff)
                });
        }

        internal static void Clear(UnitDescriptor owner,
            BrownFurBlueprintSet blueprints)
        {
            if (owner == null || blueprints == null) return;
            ActivatableAbility[] shareAbilities =
                owner.ActivatableAbilities == null ?
                new ActivatableAbility[0] :
                owner.ActivatableAbilities.Enumerable.Where(value =>
                    value != null && ReferenceEquals(value.Blueprint,
                        blueprints.ShareTransmutationAbility)).ToArray();
            foreach (ActivatableAbility share in shareAbilities)
                if (share.IsOn) share.IsOn = false;
            foreach (var pending in blueprints.ScoreBuffs ??
                new Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff[0])
                if (pending != null && owner.HasFact(pending))
                    owner.RemoveFact(pending);
            if (blueprints.ShareTransmutationBuff != null &&
                owner.HasFact(blueprints.ShareTransmutationBuff))
                owner.RemoveFact(blueprints.ShareTransmutationBuff);
        }
    }
}
