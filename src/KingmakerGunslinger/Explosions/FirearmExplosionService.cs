using System;
using KingmakerGunslinger.Misfires;

namespace KingmakerGunslinger.Explosions
{
    /// <summary>
    /// Pure bounded explosion policy. A Normal-to-Broken misfire does not explode.
    /// Only the already-proven Broken-to-Wrecked decision schedules one native
    /// Reflex-half burst using the exact firearm definition's verified radius.
    /// </summary>
    internal sealed class FirearmExplosionService
    {
        internal const int ReflexSaveDifficultyClass = 12;

        internal FirearmExplosionDecision Evaluate(
            FirearmMisfireConditionDecision condition)
        {
            if (condition == null)
            {
                throw new ArgumentNullException("condition");
            }

            FirearmExplosionDisposition disposition =
                condition.Transition == FirearmMisfireConditionTransition.BrokenToWrecked
                    ? FirearmExplosionDisposition.DamageBurst
                    : FirearmExplosionDisposition.None;
            return new FirearmExplosionDecision(condition, disposition);
        }
    }
}
