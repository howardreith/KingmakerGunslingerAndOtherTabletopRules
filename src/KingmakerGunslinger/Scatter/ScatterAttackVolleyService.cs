using System;
using System.Collections.Generic;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Scatter
{
    /// <summary>
    /// Validates exactly one native roll per planned target and aggregates the
    /// tabletop all-roll misfire rule without replacing native hit/critical work.
    /// </summary>
    internal sealed class ScatterAttackVolleyService
    {
        internal ScatterAttackVolleyDecision Evaluate(
            FirearmDefinition definition,
            ScatterTargetPlan plan,
            IEnumerable<ScatterAttackRollObservation> rolls)
        {
            if (definition == null) throw new ArgumentNullException("definition");
            if (!definition.IsScatter)
                throw new ArgumentException("Only a scatter firearm can evaluate a scatter volley.", "definition");
            if (plan == null) throw new ArgumentNullException("plan");
            if (rolls == null) throw new ArgumentNullException("rolls");

            var planned = new HashSet<object>(ReferenceIdentityComparer.Instance);
            foreach (ScatterTargetCandidate target in plan.Targets) planned.Add(target.Unit);
            var observed = new HashSet<object>(ReferenceIdentityComparer.Instance);
            int count = 0;
            int hits = 0;
            int misfires = 0;
            int threats = 0;
            int confirmed = 0;
            foreach (ScatterAttackRollObservation roll in rolls)
            {
                if (roll == null)
                    throw new ArgumentException("A scatter volley cannot contain a null roll.", "rolls");
                if (!planned.Contains(roll.Target))
                    throw new ArgumentException("A scatter roll target is not in the exact target plan.", "rolls");
                if (!observed.Add(roll.Target))
                    throw new ArgumentException("A scatter target received more than one attack roll.", "rolls");
                count++;
                if (roll.IsHit) hits++;
                if (roll.IsMisfire(definition.MisfireValue)) misfires++;
                if (roll.IsCriticalThreat) threats++;
                if (roll.IsCriticalConfirmed) confirmed++;
            }
            if (count != plan.TargetCount)
                throw new InvalidOperationException("Every planned scatter target requires exactly one attack roll.");

            return new ScatterAttackVolleyDecision(
                count, hits, misfires, threats, confirmed);
        }
    }
}
