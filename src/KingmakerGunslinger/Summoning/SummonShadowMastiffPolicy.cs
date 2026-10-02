using System;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// The Shadow Mastiff's two printed supernatural abilities, stated as pure
    /// decisions so each clause can be tested without a loaded game.
    /// </summary>
    internal static class SummonShadowMastiffPolicy
    {
        /// <summary>
        /// Printed bay: "When a shadow mastiff howls or barks, all creatures
        /// within a 300-foot spread except evil outsiders must succeed at a DC
        /// 16 Will save or become panicked for 1d4 rounds. This is a sonic,
        /// mind-affecting fear effect. A creature that successfully saves
        /// cannot be affected by the same mastiff's bay for 24 hours. ... The
        /// save DC is Charisma-based and includes a +2 racial bonus."
        ///
        /// <para>The DC is computed from its components rather than written
        /// down as 16, so a buffed or drained Charisma moves it the way the
        /// engine expects. At the printed stat block this is
        /// 10 + 3 (half of 6 hit dice) + 1 (Charisma 13) + 2 racial = 16.</para>
        /// </summary>
        internal static int BayWillDc(int hitDice, int charisma)
        {
            return 10 + hitDice / 2 + AbilityModifier(charisma) +
                ExpandedSummoningSpecialProfiles.ShadowMastiffBayRacialSaveBonus;
        }

        internal static int AbilityModifier(int score)
        {
            return (int)Math.Floor((score - 10) / 2d);
        }

        /// <summary>
        /// True when bay should roll a save against this creature. The printed
        /// spread catches everything in range except evil outsiders, and a
        /// creature that already saved against <em>this</em> mastiff is inside
        /// its own 24-hour window. The mastiff is itself an evil outsider, so
        /// it is exempt by the same clause rather than by a special case.
        /// </summary>
        internal static bool ShouldRollBay(bool targetAvailable,
            bool targetIsEvilOutsider, bool targetIsImmuneToThisMastiff)
        {
            return targetAvailable && !targetIsEvilOutsider &&
                !targetIsImmuneToThisMastiff;
        }

        /// <summary>
        /// The printed effect on a failed save is panic. Kingmaker models one
        /// authorable flee state, <c>UnitCondition.Frightened</c>, which drives
        /// <c>UnitFearController</c>: it interrupts every command each tick and
        /// runs the creature away from its remembered enemies along a partly
        /// random path. That is behaviourally what PF1 assigns to panicked, so
        /// bay applies it for the printed 1d4 rounds.
        /// </summary>
        internal static bool AppliesPanicOnFailedSave(bool saved)
        {
            return !saved;
        }

        /// <summary>
        /// The printed 24-hour immunity is granted only by a successful save,
        /// and only against the mastiff that caused it.
        /// </summary>
        internal static bool GrantsImmunityOnSave(bool saved)
        {
            return saved;
        }

        /// <summary>
        /// Printed shadow blend: "In any condition of illumination other than
        /// full daylight, a shadow mastiff disappears into the shadows, giving
        /// it concealment (50% miss chance). Artificial illumination, even a
        /// light or continual flame spell, does not negate this ability; a
        /// daylight spell, however, does."
        ///
        /// <para>Both negations are honoured and nothing else is: artificial
        /// light is deliberately absent from this decision, because the printed
        /// text says it does not matter.</para>
        /// </summary>
        internal static bool GrantsShadowConcealment(bool fullDaylight,
            bool daylightEffectPresent)
        {
            return !fullDaylight && !daylightEffectPresent;
        }

        /// <summary>
        /// Guards the printed magnitudes and the derived save DC against a
        /// silent edit.
        /// </summary>
        internal static void Validate()
        {
            if (BayWillDc(ExpandedSummoningSpecialProfiles.ShadowMastiffHitDice,
                    ExpandedSummoningSpecialProfiles.ShadowMastiffCharisma) !=
                ExpandedSummoningSpecialProfiles.ShadowMastiffBayPrintedWillDc)
                throw new InvalidOperationException(
                    "Shadow Mastiff bay save DC no longer derives to its printed value.");
            if (AbilityModifier(13) != 1 || AbilityModifier(10) != 0 ||
                AbilityModifier(9) != -1 || AbilityModifier(8) != -1 ||
                AbilityModifier(7) != -2)
                throw new InvalidOperationException(
                    "Ability modifier rounding changed.");
            if (!GrantsShadowConcealment(false, false) ||
                GrantsShadowConcealment(true, false) ||
                GrantsShadowConcealment(false, true))
                throw new InvalidOperationException(
                    "Shadow blend negation clauses changed.");
        }
    }
}
