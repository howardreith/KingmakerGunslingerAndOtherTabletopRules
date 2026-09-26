using System;
using Kingmaker.RuleSystem.Rules;

namespace KingmakerGunslinger.Deeds
{
    /// <summary>
    /// The Pistol-Whip deed's own attack while it resolves (its attack roll
    /// and the attack bonus computed for it). Anything resolved outside it is
    /// not the deed's attack roll: the following trip, or a maneuver bonus
    /// another mod derives from a weapon's attack bonus while that trip
    /// resolves. The scope is closed in a finally block and a nested deed
    /// restores the outer one.
    /// </summary>
    internal static class PistolWhipAttackScope
    {
        [ThreadStatic]
        private static RuleAttackWithWeapon s_Current;

        /// <summary>The deed attack being resolved on this thread, or null.</summary>
        internal static RuleAttackWithWeapon Current
        {
            get { return s_Current; }
        }

        internal static void Run(RuleAttackWithWeapon attack, Action<RuleAttackWithWeapon> trigger)
        {
            if (trigger == null) throw new ArgumentNullException("trigger");
            RuleAttackWithWeapon previous = s_Current;
            s_Current = attack;
            try
            {
                trigger(attack);
            }
            finally
            {
                s_Current = previous;
            }
        }
    }
}
