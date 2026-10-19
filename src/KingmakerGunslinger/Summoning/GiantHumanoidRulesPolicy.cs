using System;


namespace KingmakerGunslinger.Summoning
{
    /// <summary>The three adult, untemplated Sprint 28 giant rules shared by the
    /// builder and the live attack/catch adapters.</summary>
    internal static class GiantHumanoidRulesPolicy
    {
        internal const string Hill = "hill-giant";
        internal const string Stone = "stone-giant";
        internal const string Fire = "fire-giant";
        internal const int AppendedCreatureCount = 3;
        internal const int AppendedRootCount = 11;
        internal const int AppendedPrivateIdentityCount = 19;
        internal const int AppendedCatalogIdentityCount =
            AppendedCreatureCount + AppendedRootCount +
            AppendedPrivateIdentityCount;
        internal const int BorrowedVisualCount = 2;
        internal const int RacialRockAttackBonus = 1;
        internal const int StoneRockCatchBonus = 4;
        internal const int LargestRockIncrementCount = 5;

        internal static bool IsNewIdentity(string symbol)
        {
            if (symbol == null) return false;
            return symbol.StartsWith("KMG.Summoning.Giant.",
                StringComparison.Ordinal) ||
                symbol == "KMG.Summoning.Unit.HillGiant" ||
                symbol == "KMG.Summoning.Unit.StoneGiant" ||
                symbol == "KMG.Summoning.Unit.FireGiant" ||
                symbol.IndexOf(".HillGiant.", StringComparison.Ordinal) >= 0 ||
                symbol.IndexOf(".StoneGiant.", StringComparison.Ordinal) >= 0 ||
                symbol.IndexOf(".FireGiant.", StringComparison.Ordinal) >= 0;
        }

        internal static bool IsGiant(string key)
        {
            return key == Hill || key == Stone || key == Fire;
        }

        internal static int RockRangeIncrementFeet(string key)
        {
            if (key == Stone) return 180;
            if (key == Hill || key == Fire) return 120;
            throw new ArgumentOutOfRangeException("key", key,
                "Only the three adult Sprint 28 giants throw these rocks.");
        }

        internal static int RockFullAttackCount(string key)
        {
            if (key == Stone) return 2; // Its prepared Quick Draw routine.
            if (key == Hill || key == Fire) return 1;
            throw new ArgumentOutOfRangeException("key", key,
                "Only the three adult Sprint 28 giants throw these rocks.");
        }

        /// <summary>Native Kingmaker applies a penalty once and truncates a
        /// positive one-and-a-half multiplier toward zero. Preserve that
        /// convention for the live two-handed and thrown-rock consumers.</summary>
        internal static int OneAndHalfStrength(int modifier)
        {
            return modifier > 0 ? modifier * 3 / 2 : modifier;
        }

        internal static int CatchDifficultyClass(string projectileSize,
            int magicAttackEnhancement)
        {
            if (magicAttackEnhancement < 0)
                throw new ArgumentOutOfRangeException("magicAttackEnhancement");
            switch (projectileSize)
            {
                case "Small": return 15 + magicAttackEnhancement;
                case "Medium": return 20 + magicAttackEnhancement;
                case "Large": return 25 + magicAttackEnhancement;
                default: throw new ArgumentOutOfRangeException("projectileSize",
                    projectileSize, "Only a Small, Medium or Large rock is catchable.");
            }
        }
    }
}
