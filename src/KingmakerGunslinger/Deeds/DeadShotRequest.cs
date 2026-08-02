using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class DeadShotRequest
    {
        internal DeadShotRequest(bool exactEquippedFirearm, bool scatterWeapon,
            FirearmCondition condition, int loadedChambers, int currentGrit,
            int baseAttackBonus)
        {
            if (!Enum.IsDefined(typeof(FirearmCondition), condition))
                throw new ArgumentOutOfRangeException("condition");
            if (loadedChambers < 0) throw new ArgumentOutOfRangeException("loadedChambers");
            if (currentGrit < 0) throw new ArgumentOutOfRangeException("currentGrit");
            if (baseAttackBonus < 0 || baseAttackBonus > 20)
                throw new ArgumentOutOfRangeException("baseAttackBonus");
            ExactEquippedFirearm = exactEquippedFirearm;
            ScatterWeapon = scatterWeapon;
            Condition = condition;
            LoadedChambers = loadedChambers;
            CurrentGrit = currentGrit;
            BaseAttackBonus = baseAttackBonus;
        }

        internal bool ExactEquippedFirearm { get; private set; }
        internal bool ScatterWeapon { get; private set; }
        internal FirearmCondition Condition { get; private set; }
        internal int LoadedChambers { get; private set; }
        internal int CurrentGrit { get; private set; }
        internal int BaseAttackBonus { get; private set; }
    }
}
