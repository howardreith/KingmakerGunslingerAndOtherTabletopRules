using System;

namespace KingmakerGunslinger.Summoning
{
    internal static class InfernalRules36Policy
    {
        internal const int BabauHitDice = 7;
        internal const int BabauProtectiveSlimeDc = 18;
        internal const int BabauDispelCasterLevel = 7;
        internal const int HezrouHitDice = 10;
        internal const int HezrouStenchDc = 24;
        internal const int HezrouSpellLikeCasterLevel = 13;
        internal const int HezrouSpellLikeDc = 18;
        internal const int StenchRadiusFeet = 30;
        internal const int StenchSickenedRounds = 10;
        internal const int SuccessfulStenchMemoryHours = 24;
        internal const int HeldNauseaRecoveryRounds = 10;

        internal static int ConstitutionDc(int hitDice, int constitutionBonus)
        { return 10 + hitDice / 2 + constitutionBonus; }

        internal static int SpellLikeDc(int spellLevel, int charismaScore)
        { return 10 + spellLevel + (charismaScore - 10) / 2; }

        internal static bool ProtectiveSlimeNaturalStrike(bool hit,
            bool melee, bool natural, bool unarmed)
        { return hit && melee && (natural || unarmed); }

        internal static bool CanExposeStench(bool available,
            bool exactSource, bool poisonImmune, bool sourceMemory)
        { return available && exactSource && !poisonImmune && !sourceMemory; }

        internal static bool CanTestHeldNausea(bool available,
            bool exactSource, bool heldBySource, bool poisonImmune)
        { return available && exactSource && heldBySource && !poisonImmune; }

        internal static bool UsefulDispelEffect(bool active, bool visible,
            bool fromSpell, bool notDispellable, bool hasContext)
        { return active && visible && fromSpell && !notDispellable && hasContext; }

        internal static int AlignmentBurstDamage(int rolled, bool primary,
            bool exempt, bool savePassed)
        {
            if (rolled <= 0 || exempt) return 0;
            int amount = primary ? rolled : rolled / 2;
            return savePassed ? amount / 2 : amount;
        }

        internal static bool AlignmentBurstCondition(bool primary,
            bool exempt, bool savePassed)
        { return primary && !exempt && !savePassed; }

        internal static void Validate()
        {
            if (ConstitutionDc(BabauHitDice, 5) !=
                    BabauProtectiveSlimeDc ||
                ConstitutionDc(HezrouHitDice, 9) != HezrouStenchDc ||
                SpellLikeDc(4, 18) != HezrouSpellLikeDc ||
                !ProtectiveSlimeNaturalStrike(true, true, true, false) ||
                !ProtectiveSlimeNaturalStrike(true, true, false, true) ||
                ProtectiveSlimeNaturalStrike(false, true, true, false) ||
                ProtectiveSlimeNaturalStrike(true, false, true, false) ||
                ProtectiveSlimeNaturalStrike(true, true, false, false) ||
                !CanExposeStench(true, true, false, false) ||
                CanExposeStench(true, true, true, false) ||
                CanExposeStench(true, true, false, true) ||
                !CanTestHeldNausea(true, true, true, false) ||
                CanTestHeldNausea(true, true, false, false) ||
                !UsefulDispelEffect(true, true, true, false, true) ||
                UsefulDispelEffect(true, true, false, false, true) ||
                UsefulDispelEffect(true, false, true, false, true) ||
                AlignmentBurstDamage(40, true, false, false) != 40 ||
                AlignmentBurstDamage(40, true, false, true) != 20 ||
                AlignmentBurstDamage(40, false, false, false) != 20 ||
                AlignmentBurstDamage(40, false, false, true) != 10 ||
                AlignmentBurstDamage(40, true, true, false) != 0 ||
                !AlignmentBurstCondition(true, false, false) ||
                AlignmentBurstCondition(true, false, true) ||
                StenchRadiusFeet != 30 || StenchSickenedRounds != 10 ||
                SuccessfulStenchMemoryHours != 24 ||
                HeldNauseaRecoveryRounds != 10)
                throw new InvalidOperationException(
                    "Sprint 36 infernal rules contract changed.");
        }
    }
}
