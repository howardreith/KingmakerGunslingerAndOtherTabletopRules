using System;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// The printed Wolverine rage, stated once as pure decisions so the rule
    /// can be tested without a loaded game.
    ///
    /// <para>Bestiary text: "A wolverine that takes damage in combat flies into
    /// a rage on its next turn, clawing and biting madly until either it or its
    /// opponent is dead. It gains +4 to Strength, +4 to Constitution, and -2 to
    /// AC. The creature cannot end its rage voluntarily."</para>
    ///
    /// <para>Four parts of that sentence are load-bearing and each is encoded
    /// here rather than left to the component: the trigger is <em>taking
    /// damage</em> and not merely being attacked, the onset is the creature's
    /// <em>next</em> turn and not the moment of the hit, the effect includes an
    /// AC <em>penalty</em> that a pure-upside rage would quietly drop, and the
    /// rage has no voluntary end.</para>
    /// </summary>
    internal static class SummonRagePolicy
    {
        /// <summary>
        /// Printed morale bonus to Strength and Constitution.
        /// </summary>
        internal const int WolverineRageAbilityBonus = 4;

        /// <summary>
        /// Printed penalty to Armor Class. Negative on purpose: the rage is
        /// not a pure benefit, and recording it as a bonus of -2 keeps the
        /// sign visible at every call site.
        /// </summary>
        internal const int WolverineRageArmorClassPenalty = -2;

        /// <summary>
        /// Rounds between taking damage and the rage beginning. The printed
        /// rule says "on its next turn", which is one round boundary away.
        /// </summary>
        internal const int WolverineRageOnsetRounds = 1;

        /// <summary>
        /// How long the hidden onset marker lives. One round boundary has to
        /// arrive inside this window for the rage to begin, and nothing longer
        /// is wanted: a marker that outlives its purpose is hidden state
        /// lingering on the creature.
        /// </summary>
        internal const int WolverineRageOnsetMarkerRounds = 2;

        /// <summary>
        /// True when a damage event should schedule the rage. The creature
        /// must have taken actual positive damage - a miss, a fully resisted
        /// hit, or a zero-damage effect is not "takes damage" - and must not
        /// already be raging or already waiting for an onset, because the
        /// printed rage neither stacks nor restarts.
        /// </summary>
        internal static bool ShouldScheduleRage(int actualDamage,
            bool ownerIsTarget, bool ownerAvailable, bool alreadyRaging,
            bool onsetPending)
        {
            return actualDamage > 0 && ownerIsTarget && ownerAvailable &&
                !alreadyRaging && !onsetPending;
        }

        /// <summary>
        /// True when the scheduled onset has waited its printed delay and the
        /// rage should now begin. Separated from <see cref="ShouldScheduleRage"/>
        /// so a test can show that no arrangement of damage events makes the
        /// rage start on the same turn it was triggered.
        /// </summary>
        internal static bool ShouldBeginRage(int roundsWaited,
            bool ownerAvailable, bool alreadyRaging)
        {
            return ownerAvailable && !alreadyRaging &&
                roundsWaited >= WolverineRageOnsetRounds;
        }

        /// <summary>
        /// The printed rage cannot be ended voluntarily, so nothing in the
        /// game may remove it on request. It ends only with the creature:
        /// death, expiry, dismissal, an area change that destroys the summon,
        /// or the module being disabled. This predicate exists so that any
        /// future caller asking "may the player switch this off?" gets a
        /// single, documented no.
        /// </summary>
        internal static bool MayEndVoluntarily()
        {
            return false;
        }

        /// <summary>
        /// Guards the three printed magnitudes against a silent edit. Called
        /// from the natural-profile validator so a changed constant fails the
        /// build's own checks rather than shipping.
        /// </summary>
        internal static void Validate()
        {
            if (WolverineRageAbilityBonus != 4 ||
                WolverineRageArmorClassPenalty != -2 ||
                WolverineRageOnsetRounds != 1 || MayEndVoluntarily())
                throw new InvalidOperationException(
                    "Wolverine rage printed contract changed.");
        }
    }
}
