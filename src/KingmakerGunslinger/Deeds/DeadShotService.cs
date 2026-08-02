using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class DeadShotService
    {
        internal DeadShotDecision Evaluate(DeadShotRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (!request.ExactEquippedFirearm)
                return Reject(DeadShotStatus.NotExactEquippedFirearm);
            if (request.ScatterWeapon) return Reject(DeadShotStatus.ScatterWeapon);
            if (request.Condition == FirearmCondition.Wrecked)
                return Reject(DeadShotStatus.Wrecked);
            if (request.LoadedChambers < 1) return Reject(DeadShotStatus.Empty);
            if (request.CurrentGrit < 1) return Reject(DeadShotStatus.InsufficientGrit);
            int count = request.BaseAttackBonus < 1 ? 0 :
                1 + ((request.BaseAttackBonus - 1) / 5);
            if (count < 1) return Reject(DeadShotStatus.NoAttacks);
            var bonuses = new int[count];
            for (int index = 0; index < count; index++)
                bonuses[index] = request.BaseAttackBonus - (index * 5);
            return new DeadShotDecision(DeadShotStatus.Eligible, bonuses, 1, 1);
        }

        private static DeadShotDecision Reject(DeadShotStatus status)
        {
            return new DeadShotDecision(status, new int[0], 0, 0);
        }
    }
}
