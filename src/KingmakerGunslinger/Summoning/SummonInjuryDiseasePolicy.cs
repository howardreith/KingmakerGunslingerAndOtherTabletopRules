using System;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>Pure tabletop-to-runtime gates shared by the Sprint 12 bite
    /// disease components and their detached domain tests.</summary>
    internal static class SummonInjuryDiseasePolicy
    {
        internal const int DireRatFortitudeDc = 11;
        internal const int GoblinDogFortitudeDc = 12;
        internal const int GoblinDogAllergyDurationSeconds = 24 * 60 * 60;
        internal const int GoblinDogAllergyAbilityPenalty = -2;

        internal static bool ShouldResolve(bool exactBite, bool hit,
            int actualDamage, bool targetAvailable, bool excludedUnitType)
        {
            if (actualDamage < 0)
                throw new ArgumentOutOfRangeException("actualDamage");
            return exactBite && hit && actualDamage > 0 && targetAvailable &&
                !excludedUnitType;
        }

        /// <summary>
        /// The printed Goblin Dog allergic reaction also exposes a creature
        /// "who deals damage to a goblin dog with a natural weapon or unarmed
        /// attack". The attacker is the one who saves, so the gate needs the
        /// attack to have landed, to have dealt positive final damage, and to
        /// have been delivered by a natural weapon or an unarmed strike.
        /// A manufactured weapon that merely touches the creature is not the
        /// printed trigger and must not expose its wielder.
        /// </summary>
        internal static bool ShouldResolveNaturalCounterContact(bool hit,
            int actualDamage, bool naturalOrUnarmed, bool attackerAvailable,
            bool excludedUnitType)
        {
            if (actualDamage < 0)
                throw new ArgumentOutOfRangeException("actualDamage");
            return hit && actualDamage > 0 && naturalOrUnarmed &&
                attackerAvailable && !excludedUnitType;
        }

        /// <summary>
        /// The printed rule's third trigger is a creature "who otherwise comes
        /// into contact with a goblin dog (including attempts to grapple or
        /// ride the creature)". A grapple attempt is the one such contact the
        /// game models as its own rule event, and the printed text exposes the
        /// attempt rather than a successful hold, so success is not required.
        /// Riding is outside the charter, which excludes mounted combat
        /// entirely, and the other combat maneuvers are not named by the rule.
        /// </summary>
        internal static bool ShouldResolveManeuverContact(bool grappleAttempt,
            bool initiatorAvailable, bool excludedUnitType)
        {
            return grappleAttempt && initiatorAvailable && !excludedUnitType;
        }

        internal static bool IsMagicalAbility(bool spell, bool spellLike,
            bool supernatural)
        {
            return spell || spellLike || supernatural;
        }

        internal static bool ShouldRemoveAllergicReaction(int actualHealing,
            bool spell, bool spellLike, bool supernatural)
        {
            if (actualHealing < 0)
                throw new ArgumentOutOfRangeException("actualHealing");
            return actualHealing > 0 &&
                IsMagicalAbility(spell, spellLike, supernatural);
        }
    }
}
