using System;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>Frozen adult PF1 terms for the Astral Deva. Racial Hit Dice,
    /// SLA caster level and aura caster level are deliberately separate.</summary>
    internal static class AstralDevaRulesPolicy
    {
        internal const int HitDice = 15;
        internal const int SpellLikeCasterLevel = 13;
        internal const int AuraCasterLevel = 15;
        internal const int Strength = 26;
        internal const int Dexterity = 19;
        internal const int Constitution = 21;
        internal const int Intelligence = 18;
        internal const int Wisdom = 18;
        internal const int Charisma = 23;
        internal const int BaseRacialHitPoints = 82;
        internal const int BaseFortitude = 9;
        internal const int BaseReflex = 9;
        // The printed Astral entry is Will +11. Its explicit base is five
        // after live Wisdom and Iron Will, despite the general outsider table.
        internal const int BaseWill = 5;
        internal const int NaturalArmor = 15;
        internal const int SpellResistance = 25;
        internal const int AuraRadiusFeet = 20;
        internal const float AuraRadiusMeters = 6.096f;
        internal const int LesserGlobeMaximumSpellLevel = 3;
        internal const int BladeBarrierSpellLevel = 6;
        internal const int BladeBarrierUses = 1;
        internal const int CureLightWoundsSpellLevel = 1;
        internal const int CureLightWoundsUses = 7;
        internal const int SeeInvisibilitySpellLevel = 2;
        internal const int SeeInvisibilityUses = 7;
        internal const int AidSpellLevel = 2;
        internal const int DispelMagicSpellLevel = 3;
        internal const int HolyAuraSpellLevel = 8;
        internal const int HolyWordSpellLevel = 7;
        internal const int InvisibilitySpellLevel = 2;
        internal const int RemoveCurseSpellLevel = 3;
        internal const int RemoveDiseaseSpellLevel = 3;
        internal const int RemoveFearSpellLevel = 1;
        internal const int HolySmiteSpellLevel = 4;
        internal const int HolySmiteRadiusFeet = 20;
        internal const int HolySmiteOrdinaryDice = 5;
        internal const int HolySmiteEvilOutsiderDice = 10;
        internal const int HolySmiteBlindRounds = 1;
        internal static bool HasGoodSummoningDescriptor { get { return true; } }
        internal static bool HasLawSummoningDescriptor { get { return false; } }

        internal static int Modifier(int score)
        { return (int)Math.Floor((score - 10) / 2.0); }

        internal static int AdultHitPoints()
        { return BaseRacialHitPoints + HitDice * Modifier(Constitution) + HitDice; }

        internal static int StunDifficultyClass()
        { return StunDifficultyClass(Modifier(Strength)); }

        internal static int StunDifficultyClass(int liveStrengthModifier)
        { return 10 + HitDice / 2 + liveStrengthModifier; }

        internal static int WarhammerAttackBonus()
        { return HitDice + Modifier(Strength) + 2 + 1; }

        internal static int WarhammerDamageBonus()
        { return Modifier(Strength) * 3 / 2 + 2; }

        internal static int SlamAttackBonus()
        { return HitDice + Modifier(Strength); }

        internal static int SlamDamageBonus()
        { return Modifier(Strength) * 3 / 2; }

        internal static int HolySmiteWillDc(int liveCharismaScore)
        { return 10 + HolySmiteSpellLevel + Modifier(liveCharismaScore); }

        internal static int HolySmiteDiceCount(bool evilOutsider)
        { return evilOutsider ? HolySmiteEvilOutsiderDice : HolySmiteOrdinaryDice; }

        internal static int HolySmiteDieSides(bool evilOutsider)
        { return evilOutsider ? 6 : 8; }

        internal static int HolySmiteDamage(int rolledDamage, bool evil,
            bool saved)
        {
            if (rolledDamage < 0) throw new ArgumentOutOfRangeException(
                "rolledDamage");
            int divisor = evil ? (saved ? 2 : 1) : (saved ? 4 : 2);
            return rolledDamage / divisor;
        }

        internal static bool HolySmiteBlinds(bool evil, bool saved)
        { return evil && !saved; }

        internal static bool BlocksLesserGlobe(int spellLevel, bool spell,
            bool spellLike, float sourceDistanceMeters, float sourceCorpulenceMeters)
        {
            return spellLevel >= 0 && spellLevel <= LesserGlobeMaximumSpellLevel &&
                (spell || spellLike) &&
                sourceDistanceMeters > AuraRadiusMeters +
                    Math.Max(0f, sourceCorpulenceMeters);
        }

        internal static void Validate()
        {
            if (AdultHitPoints() != 172 || StunDifficultyClass() != 25 ||
                WarhammerAttackBonus() != 26 || WarhammerDamageBonus() != 14 ||
                SlamAttackBonus() != 23 || SlamDamageBonus() != 12 ||
                BladeBarrierSpellLevel != 6 || BladeBarrierUses != 1 ||
                CureLightWoundsSpellLevel != 1 || CureLightWoundsUses != 7 ||
                SeeInvisibilitySpellLevel != 2 || SeeInvisibilityUses != 7 ||
                AidSpellLevel != 2 || DispelMagicSpellLevel != 3 ||
                HolyAuraSpellLevel != 8 || HolyWordSpellLevel != 7 ||
                InvisibilitySpellLevel != 2 || RemoveCurseSpellLevel != 3 ||
                RemoveDiseaseSpellLevel != 3 || RemoveFearSpellLevel != 1 ||
                HolySmiteSpellLevel != 4 || HolySmiteRadiusFeet != 20 ||
                HolySmiteDiceCount(false) != 5 || HolySmiteDieSides(false) != 8 ||
                HolySmiteDiceCount(true) != 10 || HolySmiteDieSides(true) != 6 ||
                HolySmiteWillDc(Charisma) != 20 ||
                HolySmiteDamage(40, false, true) != 10 ||
                SpellLikeCasterLevel == HitDice || !HasGoodSummoningDescriptor ||
                HasLawSummoningDescriptor)
                throw new InvalidOperationException("Astral adult rules policy changed.");
        }
    }
}
