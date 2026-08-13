using System;

namespace KingmakerGunslinger.ElvenBranchedSpear
{
    internal enum NamedSpearKind
    {
        Boughkeeper = 0,
        Thornstep = 1,
        MoonlitFork = 2,
        VipersReach = 3,
        BriarCrownedSpear = 4,
        SpearOfTheFirstBranch = 5
    }

    internal sealed class NamedSpearSpec
    {
        internal NamedSpearSpec(NamedSpearKind kind, string symbol,
            string displayName, int cost, int enhancement, bool coldIron,
            bool agile, bool keen, bool corrosive, bool speed)
        {
            if (string.IsNullOrWhiteSpace(symbol) ||
                string.IsNullOrWhiteSpace(displayName) || cost <= 0 ||
                enhancement < 1 || enhancement > 5)
                throw new ArgumentException("Named spear specification is invalid.");
            Kind = kind; Symbol = symbol; DisplayName = displayName;
            Cost = cost; Enhancement = enhancement; ColdIron = coldIron;
            Agile = agile; Keen = keen; Corrosive = corrosive; Speed = speed;
        }

        internal NamedSpearKind Kind { get; private set; }
        internal string Symbol { get; private set; }
        internal string DisplayName { get; private set; }
        internal int Cost { get; private set; }
        internal int Enhancement { get; private set; }
        internal bool ColdIron { get; private set; }
        internal bool Agile { get; private set; }
        internal bool Keen { get; private set; }
        internal bool Corrosive { get; private set; }
        internal bool Speed { get; private set; }
    }

    internal static class ElvenBranchedSpearNamedCatalog
    {
        private static readonly NamedSpearSpec[] Items =
        {
            new NamedSpearSpec(NamedSpearKind.Boughkeeper,
                "KMG.ElvenBranchedSpear.Boughkeeper", "Boughkeeper",
                5320, 1, false, false, false, false, false),
            new NamedSpearSpec(NamedSpearKind.Thornstep,
                "KMG.ElvenBranchedSpear.Thornstep", "Thornstep",
                14320, 1, false, false, true, false, false),
            new NamedSpearSpec(NamedSpearKind.MoonlitFork,
                "KMG.ElvenBranchedSpear.MoonlitFork", "Moonlit Fork",
                18340, 2, true, true, false, false, false),
            new NamedSpearSpec(NamedSpearKind.VipersReach,
                "KMG.ElvenBranchedSpear.VipersReach", "Viper's Reach",
                70320, 3, false, true, false, true, false),
            new NamedSpearSpec(NamedSpearKind.BriarCrownedSpear,
                "KMG.ElvenBranchedSpear.BriarCrownedSpear",
                "Briar-Crowned Spear", 72320, 4, false, true, false,
                false, false),
            new NamedSpearSpec(NamedSpearKind.SpearOfTheFirstBranch,
                "KMG.ElvenBranchedSpear.SpearOfTheFirstBranch",
                "Spear of the First Branch", 202340, 5, true, true,
                false, false, true)
        };

        internal static NamedSpearSpec[] All
        { get { return (NamedSpearSpec[])Items.Clone(); } }

        internal static NamedSpearSpec Require(NamedSpearKind kind)
        {
            for (int index = 0; index < Items.Length; index++)
                if (Items[index].Kind == kind) return Items[index];
            throw new ArgumentOutOfRangeException("kind");
        }
    }

    internal static class NamedSpearEffectPolicy
    {
        internal static bool Boughkeeper(bool hit, bool attackOfOpportunity)
        { return hit && attackOfOpportunity; }

        internal static bool Thornstep(bool hit, bool attackOfOpportunity,
            bool movementProvocation, bool usedThisRound)
        { return hit && attackOfOpportunity && movementProvocation &&
            !usedThisRound; }

        internal static bool VipersReach(bool sneakAttackUsed,
            int appliedSneakDamage, bool usedThisRound)
        { return sneakAttackUsed && appliedSneakDamage > 0 && !usedThisRound; }

        internal static bool BriarCrowned(bool hit, bool attackOfOpportunity,
            bool generatedAttack, bool usedThisRound, int remainingOpportunities)
        { return hit && attackOfOpportunity && !generatedAttack &&
            !usedThisRound && remainingOpportunities > 0; }

        internal static bool FirstBranch(bool hit, bool attackOfOpportunity,
            bool sneakAttackUsed, int appliedSneakDamage, bool usedThisRound,
            bool generatedOrSecondary)
        {
            bool opportunity = hit && attackOfOpportunity;
            bool sneak = sneakAttackUsed && appliedSneakDamage > 0;
            return !usedThisRound && !generatedOrSecondary &&
                (opportunity || sneak);
        }

        internal static int FirstBranchDifficultyClass(int characterLevel,
            int dexterityModifier)
        {
            if (characterLevel < 1) throw new ArgumentOutOfRangeException(
                "characterLevel");
            return 10 + characterLevel / 2 + dexterityModifier;
        }
    }
}
