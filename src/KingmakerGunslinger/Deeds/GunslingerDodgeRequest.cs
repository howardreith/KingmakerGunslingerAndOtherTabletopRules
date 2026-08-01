using System;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class GunslingerDodgeRequest
    {
        internal GunslingerDodgeRequest(bool isArmed, GunslingerDodgeMode mode,
            bool isRangedAttack, GunslingerDodgeArmor armor,
            GunslingerDodgeLoad load, int currentGrit)
            : this(isArmed, mode, isRangedAttack, armor, load, currentGrit, true)
        {
        }

        internal GunslingerDodgeRequest(bool isArmed, GunslingerDodgeMode mode,
            bool isRangedAttack, GunslingerDodgeArmor armor,
            GunslingerDodgeLoad load, int currentGrit, bool canDropProne)
        {
            if (!Enum.IsDefined(typeof(GunslingerDodgeMode), mode) ||
                mode == GunslingerDodgeMode.Unknown)
                throw new ArgumentOutOfRangeException(nameof(mode));
            if (!Enum.IsDefined(typeof(GunslingerDodgeArmor), armor) ||
                armor == GunslingerDodgeArmor.Unknown)
                throw new ArgumentOutOfRangeException(nameof(armor));
            if (!Enum.IsDefined(typeof(GunslingerDodgeLoad), load) ||
                load == GunslingerDodgeLoad.Unknown)
                throw new ArgumentOutOfRangeException(nameof(load));
            if (currentGrit < 0) throw new ArgumentOutOfRangeException(nameof(currentGrit));
            IsArmed = isArmed; Mode = mode; IsRangedAttack = isRangedAttack;
            Armor = armor; Load = load; CurrentGrit = currentGrit;
            CanDropProne = canDropProne;
        }
        internal bool IsArmed { get; private set; }
        internal GunslingerDodgeMode Mode { get; private set; }
        internal bool IsRangedAttack { get; private set; }
        internal GunslingerDodgeArmor Armor { get; private set; }
        internal GunslingerDodgeLoad Load { get; private set; }
        internal int CurrentGrit { get; private set; }
        internal bool CanDropProne { get; private set; }
    }
}
