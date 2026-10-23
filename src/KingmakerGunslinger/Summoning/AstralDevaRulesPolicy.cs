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
                SpellLikeCasterLevel == HitDice || !HasGoodSummoningDescriptor ||
                HasLawSummoningDescriptor)
                throw new InvalidOperationException("Astral adult rules policy changed.");
        }
    }
}
