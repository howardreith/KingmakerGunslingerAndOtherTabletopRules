using System;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class DeadShotOutcomeService
    {
        internal DeadShotOutcome Evaluate(DeadShotDecision decision,
            DeadShotRollObservation[] rolls)
        {
            if (decision == null) throw new ArgumentNullException("decision");
            if (!decision.ShouldAttack) throw new ArgumentException(
                "A rejected Dead Shot has no outcome.", "decision");
            if (rolls == null) throw new ArgumentNullException("rolls");
            if (rolls.Length != decision.AttackBonuses.Length)
                throw new ArgumentException("Roll count does not match the Dead Shot plan.", "rolls");
            int hits = 0;
            int threats = 0;
            bool allMisfire = rolls.Length > 0;
            for (int index = 0; index < rolls.Length; index++)
            {
                DeadShotRollObservation roll = rolls[index];
                if (roll == null) throw new ArgumentException("A roll is missing.", "rolls");
                if (roll.Hit) hits++;
                if (roll.CriticalThreat) threats++;
                if (!roll.Misfire) allMisfire = false;
            }
            int? penalty = threats == 0 ? (int?)null :
                Math.Min(0, -5 + (threats - 1));
            return new DeadShotOutcome(hits, hits, allMisfire, threats, penalty);
        }
    }
}
