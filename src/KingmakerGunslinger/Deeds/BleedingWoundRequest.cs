using System;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class BleedingWoundRequest
    {
        internal BleedingWoundRequest(BleedingWoundKind kind,
            bool exactFirearm, bool eligibleAttack, bool hit, bool livingTarget,
            bool immuneToSneakAttack, int grit, int dexterityModifier)
        {
            if (!Enum.IsDefined(typeof(BleedingWoundKind), kind))
                throw new ArgumentOutOfRangeException("kind");
            if (grit < 0) throw new ArgumentOutOfRangeException("grit");
            Kind = kind;
            ExactFirearm = exactFirearm;
            EligibleAttack = eligibleAttack;
            Hit = hit;
            LivingTarget = livingTarget;
            ImmuneToSneakAttack = immuneToSneakAttack;
            Grit = grit;
            DexterityModifier = dexterityModifier;
        }

        internal BleedingWoundKind Kind { get; private set; }
        internal bool ExactFirearm { get; private set; }
        internal bool EligibleAttack { get; private set; }
        internal bool Hit { get; private set; }
        internal bool LivingTarget { get; private set; }
        internal bool ImmuneToSneakAttack { get; private set; }
        internal int Grit { get; private set; }
        internal int DexterityModifier { get; private set; }
    }
}
