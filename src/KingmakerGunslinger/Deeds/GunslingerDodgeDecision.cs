using System;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class GunslingerDodgeDecision
    {
        internal GunslingerDodgeDecision(GunslingerDodgeStatus status,
            GunslingerDodgeMode mode, int armorClassBonus, int gritCost)
        {
            if (!Enum.IsDefined(typeof(GunslingerDodgeStatus), status) ||
                status == GunslingerDodgeStatus.Unknown)
                throw new ArgumentOutOfRangeException(nameof(status));
            if (!Enum.IsDefined(typeof(GunslingerDodgeMode), mode) ||
                mode == GunslingerDodgeMode.Unknown)
                throw new ArgumentOutOfRangeException(nameof(mode));
            if (armorClassBonus < 0 || gritCost < 0)
                throw new ArgumentOutOfRangeException();
            if (status == GunslingerDodgeStatus.Eligible &&
                (gritCost != 1 || armorClassBonus !=
                    (mode == GunslingerDodgeMode.MoveFiveFeet ? 2 : 4)))
                throw new ArgumentException("Eligible dodge values changed.");
            if (status != GunslingerDodgeStatus.Eligible &&
                (gritCost != 0 || armorClassBonus != 0))
                throw new ArgumentException("Rejected dodge cannot expose effects.");
            Status = status; Mode = mode; ArmorClassBonus = armorClassBonus;
            GritCost = gritCost;
        }
        internal GunslingerDodgeStatus Status { get; private set; }
        internal GunslingerDodgeMode Mode { get; private set; }
        internal int ArmorClassBonus { get; private set; }
        internal int GritCost { get; private set; }
        internal bool ShouldApply { get { return Status == GunslingerDodgeStatus.Eligible; } }
        internal bool ShouldDropProne
        {
            get { return ShouldApply && Mode == GunslingerDodgeMode.DropProne; }
        }
    }
}
