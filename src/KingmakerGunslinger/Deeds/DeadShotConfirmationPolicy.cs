using System;

namespace KingmakerGunslinger.Deeds
{
    /// <summary>
    /// Tabletop Dead Shot: every attack roll that hits with a natural roll in
    /// the weapon's critical range is a threat; the shot confirms a critical
    /// once, at the highest attack bonus -5, +1 per threat beyond the first
    /// (at most 0, DeadShotOutcomeService), and every other confirmation
    /// bonus on the roll still applies. The confirmation is an attack roll:
    /// a natural 1 always fails and a natural 20 always succeeds.
    /// </summary>
    internal static class DeadShotConfirmationPolicy
    {
        /// <summary>A probe roll threatens: it hit (a misfire never does) with a natural roll at or above the critical edge.</summary>
        internal static bool IsThreat(bool hit, bool misfire, int naturalRoll, int criticalEdge)
        {
            if (naturalRoll < 1 || naturalRoll > 20) throw new ArgumentOutOfRangeException("naturalRoll");
            return hit && !misfire && naturalRoll >= criticalEdge;
        }

        /// <summary>
        /// The one confirmation. It is an attack roll, so a natural 1 always
        /// fails and a natural 20 always succeeds; any other natural roll +
        /// attack bonus + every confirmation bonus (the Dead Shot penalty
        /// included) must reach the target's critical AC. (Kingmaker's own
        /// confirmation of an ordinary threat compares the total only.)
        /// </summary>
        internal static bool Confirms(int naturalRoll, int attackBonus, int confirmationBonus,
            int criticalArmorClass)
        {
            if (naturalRoll < 1 || naturalRoll > 20) throw new ArgumentOutOfRangeException("naturalRoll");
            if (naturalRoll == 1) return false;
            if (naturalRoll == 20) return true;
            return naturalRoll + attackBonus + confirmationBonus >= criticalArmorClass;
        }

        /// <summary>
        /// Why a threat rolls no confirmation at all, or null: target immunity,
        /// then the party critical setting, exactly as they block a native
        /// threat. A blocked threat never reaches a natural 20.
        /// </summary>
        internal static string Blocked(bool targetImmune, bool targetInParty, bool partyCriticalsAllowed)
        {
            if (targetImmune) return "target immune to critical hits";
            if (targetInParty && !partyCriticalsAllowed) return "critical hits against the party are off";
            return null;
        }
    }

    /// <summary>The one Dead Shot confirmation as the delivery made it (evidence).</summary>
    internal sealed class DeadShotConfirmationRecord
    {
        internal DeadShotConfirmationRecord(string blocked)
        {
            Blocked = blocked;
        }

        internal DeadShotConfirmationRecord(int naturalRoll, int attackBonus, int confirmationBonus,
            int penalty, int criticalArmorClass)
        {
            NaturalRoll = naturalRoll;
            AttackBonus = attackBonus;
            ConfirmationBonus = confirmationBonus;
            Penalty = penalty;
            CriticalArmorClass = criticalArmorClass;
            Confirmed = DeadShotConfirmationPolicy.Confirms(naturalRoll, attackBonus, confirmationBonus,
                criticalArmorClass);
        }

        /// <summary>Why no confirmation was rolled (the target is immune, or party criticals are off), or null.</summary>
        internal string Blocked { get; private set; }
        internal int NaturalRoll { get; private set; }
        internal int AttackBonus { get; private set; }
        /// <summary>Every confirmation bonus on the roll, the Dead Shot penalty included.</summary>
        internal int ConfirmationBonus { get; private set; }
        internal int Penalty { get; private set; }
        internal int CriticalArmorClass { get; private set; }
        internal bool Confirmed { get; private set; }
        /// <summary>Whether a natural 1 or 20 decided the confirmation regardless of the total (evidence).</summary>
        internal bool Automatic { get { return Blocked == null && (NaturalRoll == 1 || NaturalRoll == 20); } }
        /// <summary>Whether the total alone reaches the critical AC (evidence).</summary>
        internal bool TotalReaches
        {
            get { return Blocked == null && NaturalRoll + AttackBonus + ConfirmationBonus >= CriticalArmorClass; }
        }
    }
}
