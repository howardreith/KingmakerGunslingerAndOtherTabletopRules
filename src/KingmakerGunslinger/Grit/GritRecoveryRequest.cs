using System;

namespace KingmakerGunslinger.Grit
{
    internal sealed class GritRecoveryRequest
    {
        internal GritRecoveryRequest(GritRecoveryEventKind eventKind,
            bool qualifyingOutcome, bool isExactFirearm, bool isInCombat,
            bool isCreature, bool isHelplessOrUnaware, int targetHitDice,
            int characterLevel)
        {
            if (!Enum.IsDefined(typeof(GritRecoveryEventKind), eventKind))
                throw new ArgumentOutOfRangeException(nameof(eventKind));
            if (targetHitDice < 0)
                throw new ArgumentOutOfRangeException(nameof(targetHitDice));
            if (characterLevel <= 0)
                throw new ArgumentOutOfRangeException(nameof(characterLevel));

            EventKind = eventKind;
            QualifyingOutcome = qualifyingOutcome;
            IsExactFirearm = isExactFirearm;
            IsInCombat = isInCombat;
            IsCreature = isCreature;
            IsHelplessOrUnaware = isHelplessOrUnaware;
            TargetHitDice = targetHitDice;
            CharacterLevel = characterLevel;
        }

        internal GritRecoveryEventKind EventKind { get; private set; }
        internal bool QualifyingOutcome { get; private set; }
        internal bool IsExactFirearm { get; private set; }
        internal bool IsInCombat { get; private set; }
        internal bool IsCreature { get; private set; }
        internal bool IsHelplessOrUnaware { get; private set; }
        internal int TargetHitDice { get; private set; }
        internal int CharacterLevel { get; private set; }
    }
}
