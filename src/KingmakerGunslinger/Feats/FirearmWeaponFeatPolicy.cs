using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Feats
{
    internal enum FirearmWeaponFeatEffect
    {
        Attack = 0,
        Damage = 1,
        DoubleCriticalEdge = 2
    }

    internal sealed class FirearmWeaponFeatDecision
    {
        internal FirearmWeaponFeatDecision(int attackBonus, int damageBonus,
            bool doubleCriticalEdge)
        { AttackBonus = attackBonus; DamageBonus = damageBonus;
          DoubleCriticalEdge = doubleCriticalEdge; }
        internal int AttackBonus { get; private set; }
        internal int DamageBonus { get; private set; }
        internal bool DoubleCriticalEdge { get; private set; }
    }

    internal static class FirearmWeaponFeatPolicy
    {
        internal static FirearmWeaponFeatDecision Evaluate(FirearmKind selected,
            FirearmKind actual, FirearmWeaponFeatEffect effect, int bonus)
        {
            if (!Enum.IsDefined(typeof(FirearmKind), selected) ||
                !Enum.IsDefined(typeof(FirearmKind), actual))
                throw new ArgumentOutOfRangeException("selected");
            if (!Enum.IsDefined(typeof(FirearmWeaponFeatEffect), effect))
                throw new ArgumentOutOfRangeException("effect");
            if (bonus < 0 || bonus > 4) throw new ArgumentOutOfRangeException("bonus");
            if (selected != actual) return new FirearmWeaponFeatDecision(0, 0, false);
            return new FirearmWeaponFeatDecision(
                effect == FirearmWeaponFeatEffect.Attack ? bonus : 0,
                effect == FirearmWeaponFeatEffect.Damage ? bonus : 0,
                effect == FirearmWeaponFeatEffect.DoubleCriticalEdge);
        }
    }
}
