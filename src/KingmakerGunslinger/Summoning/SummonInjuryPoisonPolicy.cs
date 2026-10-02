using System;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// The gate an injury poison needs and the native trigger does not have.
    ///
    /// <para>An injury poison is delivered by a wound. The native poison graph
    /// this project clones fires on <c>OnlyHit</c>, which is a weaker test: an
    /// attack that connects but whose damage is reduced to nothing - by damage
    /// reduction, by a hardness or resistance effect, by a minimum-damage floor
    /// of zero - has hit without wounding, and the tabletop poison does not
    /// trigger. Sprint 12's bite diseases already gate on positive final damage
    /// for exactly this reason; the poisons were left behind.</para>
    /// </summary>
    internal static class SummonInjuryPoisonPolicy
    {
        /// <summary>
        /// Whether a poison carried by a weapon should be delivered.
        ///
        /// <para>Three independent conditions, and the one that was missing is
        /// the third. The attack must have been made with the weapon the poison
        /// is gated to, it must have hit, and it must have dealt positive final
        /// damage.</para>
        /// </summary>
        internal static bool ShouldDeliver(bool exactWeapon, bool hit,
            int actualDamage)
        {
            if (actualDamage < 0)
                throw new ArgumentOutOfRangeException("actualDamage");
            return exactWeapon && hit && actualDamage > 0;
        }
    }
}
