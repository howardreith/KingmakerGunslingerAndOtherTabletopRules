using System;

namespace KingmakerGunslinger.Summoning
{
    internal static class Sprint26MagicalBeastRules
    {
        internal const int AppendedCreatureCount = 2;
        internal const int AppendedRootCount = 6;
        internal const int AppendedPrivateIdentityCount = 18;
        internal const int AppendedCatalogIdentityCount = AppendedCreatureCount + AppendedRootCount + AppendedPrivateIdentityCount;
        internal static bool UsesExactRanks(string key) { return key == "gorgon" || key == "bulette"; }

        internal const string GorgonKey = "gorgon";
        internal const string BuletteKey = "bulette";
        internal const string GorgonPrefix = "KMG.Summoning.Special.Gorgon.";
        internal const string BulettePrefix = "KMG.Summoning.Special.Bulette.";
        internal const string GorgonUnit = "KMG.Summoning.Unit.Gorgon";
        internal const string BuletteUnit = "KMG.Summoning.Unit.Bulette";
        internal const string GorgonDonor = "5bb9579fdb2b26b48bb10d61c81cfdfb";
        internal const string BuletteDonor = "18fa241bd1afd77438564ec92614f7f1";
        internal const string PetrifiedDonor = "ea0361160ee769242b6de33b4e82c093";
        internal const string ChargeDonor = "c78506dd0e14f7c45a599990e4e65038";
        internal const string ConeDonor = "fa5ee5f4cd5c6394f8b497c773f8e14a";

        internal const string GorgonUnitGuid = "ce2fc58942b54e558208ea67d595cb7b";
        internal const string BuletteUnitGuid = "f84a0babcc864630bae225b2019d6bba";
        internal static int ReachAdjustment(string owner, string weapon)
        {
            // Native reduced-reach chassis exposes 2 feet for these Gorgon
            // types and 5 feet for these Bulette types. Add only the measured
            // missing term, retaining live native reach bonuses and radii.
            if (owner == GorgonUnitGuid && (weapon == "6fa1a7d22d31434b85708d7ba7735649" ||
                weapon == "fab6e52557c5426e8e09fbd714443c2c")) return 3;
            if (owner == BuletteUnitGuid && (weapon == "f7801f4601554a478920e839ec40d9a4" ||
                weapon == "d70dfa263eff44f2a547c9f47be0f87e")) return 5;
            return 0;
        }

        internal const string PetrificationGuid = "139703f90a8d4929992b783ad0e30bd7";
        internal static bool SuppressesNativePetrificationDeath(bool nativePetrified,
            int conditionCount, bool ownInitialized, bool ownPermanent)
        { return nativePetrified && conditionCount == 1 && ownInitialized && !ownPermanent; }

        internal static int BreathDc(int hitDice, int constitutionModifier)
        {
            if (hitDice < 1) throw new ArgumentOutOfRangeException("hitDice");
            return checked(10 + hitDice / 2 + constitutionModifier);
        }

        internal static int CooldownRounds(int d4)
        {
            if (d4 < 1 || d4 > 4) throw new ArgumentOutOfRangeException("d4");
            return d4 + 1;
        }

        internal static bool CanEscalate(bool initialized, bool permanent,
            string firstActivation, string lastActivation, string activation)
        {
            return initialized && !permanent && !string.IsNullOrEmpty(activation) &&
                !string.Equals(firstActivation, activation, StringComparison.Ordinal) &&
                !string.Equals(lastActivation, activation, StringComparison.Ordinal);
        }

        internal static int JumpOnlyBonus(int speedFeet)
        {
            if (speedFeet < 0) throw new ArgumentOutOfRangeException("speedFeet");
            return checked(4 + (speedFeet - 30) / 10 * 4);
        }

        internal static int SavageBiteStrength(int strengthModifier)
        { return checked((int)Math.Floor(strengthModifier * 1.5d)); }

        internal static int LonePrimaryExcess(int strengthModifier)
        { return strengthModifier > 0 ? strengthModifier / 2 : 0; }
    }
}
