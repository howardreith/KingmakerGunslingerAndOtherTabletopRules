using System;

namespace KingmakerGunslinger.Spells.ShieldOther
{
    internal static class ShieldOtherDamageSplitPolicy
    {
        internal static ShieldOtherDamageSplit Split(int finalizedHitPointDamage,
            bool linkValid, bool transferredEvent)
        {
            if (finalizedHitPointDamage < 0)
                throw new ArgumentOutOfRangeException("finalizedHitPointDamage");
            if (transferredEvent)
                return Unchanged(finalizedHitPointDamage, "transferred-event");
            if (!linkValid)
                return Unchanged(finalizedHitPointDamage, "invalid-link");
            int subjectShare = finalizedHitPointDamage / 2;
            int casterShare = finalizedHitPointDamage - subjectShare;
            return new ShieldOtherDamageSplit(subjectShare, casterShare,
                finalizedHitPointDamage == 0 ? "zero-damage" : "split");
        }

        private static ShieldOtherDamageSplit Unchanged(int damage, string status)
        { return new ShieldOtherDamageSplit(damage, 0, status); }
    }
}
