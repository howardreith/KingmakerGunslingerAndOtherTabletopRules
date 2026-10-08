using System;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>Printed Medium land-use snakes; no Salamander or donor policy.</summary>
    internal sealed class SerpentineRulesProfile
    {
        internal SerpentineRulesProfile(string key, int mobilityRanks,
            int baseHitPoints, bool hasConstrict)
        {
            Key = key; MobilityRanks = mobilityRanks;
            BaseHitPoints = baseHitPoints; HasConstrict = hasConstrict;
        }
        internal string Key { get; private set; }
        internal int MobilityRanks { get; private set; }
        internal int PerceptionRanks { get { return 1; } }
        internal int StealthRanks { get { return 1; } }
        // Racial dice only: Constitution and Toughness remain native/live.
        internal int BaseHitPoints { get; private set; }
        internal bool HasConstrict { get; private set; }
        internal int GrabTargetSizeDelta { get { return 0; } }
    }

    internal static class SerpentineRulesPolicy
    {
        internal const int MobilityRacialBonus = 8;
        internal const int PerceptionRacialBonus = 4;
        internal const int StealthRacialBonus = 4;
        internal const int ViperPoisonExposures = 6;
        internal const int ViperPoisonSavesToCure = 1;
        internal const int MediumSize = 4;
        private static readonly SerpentineRulesProfile Viper =
            new SerpentineRulesProfile("viper", 0, 9, false);
        private static readonly SerpentineRulesProfile Constrictor =
            new SerpentineRulesProfile("constrictor-snake", 1, 13, true);

        internal static bool IsSnake(string key)
        { return key == "viper" || key == "constrictor-snake"; }

        internal static SerpentineRulesProfile For(string key)
        {
            if (key == Viper.Key) return Viper;
            if (key == Constrictor.Key) return Constrictor;
            throw new ArgumentException("Not a Sprint 17 land snake.", "key");
        }

        internal static void AllocateLandRanks(string key, ref int mobility,
            ref int perception, ref int stealth)
        {
            SerpentineRulesProfile rules = For(key);
            if (mobility != 0 || perception != 0 || stealth != 0)
                throw new InvalidOperationException(
                    "Snake class ranks must start unallocated.");
            mobility = rules.MobilityRanks;
            perception = rules.PerceptionRanks;
            stealth = rules.StealthRanks;
        }

        internal static int ViperPoisonDifficultyClass(int liveConstitutionBonus)
        { return 10 + 2 / 2 + liveConstitutionBonus; }

        // Both snakes have a single natural attack: positive Strength gets
        // 1.5x, while a penalty applies once. Constrict has the same baseline.
        internal static int SingleNaturalDamageBonus(int liveStrengthBonus)
        { return liveStrengthBonus + Math.Max(0, liveStrengthBonus) / 2; }

        internal static int LiveWeaponSize(int bodySize, int weaponSize,
            int calculatedWeaponSize)
        { return Math.Max(2, Math.Min(8, bodySize + calculatedWeaponSize - weaponSize)); }
    }
}
