using System;
using System.Globalization;
using KingmakerGunslinger.Misfires;

namespace KingmakerGunslinger.Explosions
{
    /// <summary>
    /// Immutable pure decision joining a proven misfire-condition result to the
    /// bounded native Reflex-half burst consequence.
    /// </summary>
    internal sealed class FirearmExplosionDecision
    {
        internal FirearmExplosionDecision(
            FirearmMisfireConditionDecision condition,
            FirearmExplosionDisposition disposition)
        {
            Condition = condition ?? throw new ArgumentNullException("condition");
            if (!Enum.IsDefined(typeof(FirearmExplosionDisposition), disposition))
            {
                throw new ArgumentOutOfRangeException(
                    "disposition",
                    disposition,
                    "A defined firearm explosion disposition is required.");
            }

            bool requiresBurst =
                condition.Transition == FirearmMisfireConditionTransition.BrokenToWrecked;
            if (requiresBurst !=
                (disposition == FirearmExplosionDisposition.DamageBurst))
            {
                throw new ArgumentException(
                    "Only a proven BrokenToWrecked misfire may damage the firearm burst; every other condition result must select no explosion.",
                    "disposition");
            }

            Disposition = disposition;
        }

        internal FirearmMisfireConditionDecision Condition { get; private set; }

        internal FirearmExplosionDisposition Disposition { get; private set; }

        internal bool RequiresBurstDamage
        {
            get { return Disposition == FirearmExplosionDisposition.DamageBurst; }
        }

        internal bool ShouldApply
        {
            get { return RequiresBurstDamage; }
        }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "explosionDisposition={0}; requiresBurstDamage={1}; reflexDC={2}; conditionTransition={3}",
                Disposition,
                RequiresBurstDamage,
                FirearmExplosionService.ReflexSaveDifficultyClass,
                Condition.Transition);
        }
    }
}
