using System;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Frozen Sprint 23 tabletop arithmetic and exact private identity names.
    /// Runtime components still obtain live Strength, racial HD, size and
    /// native rule outcomes; this class contains no combat state.
    /// </summary>
    internal static class ArmoredDinosaurRulesPolicy
    {
        internal const string TriceratopsKey = "triceratops";
        internal const string StegosaurusKey = "stegosaurus";

        internal const string TriceratopsGoreSymbol =
            "KMG.Summoning.Natural.Triceratops.Gore2d10";
        internal const string StegosaurusTailSymbol =
            "KMG.Summoning.Natural.Stegosaurus.Tail4d6";
        internal const string TriceratopsUnitTypeSymbol =
            "KMG.Summoning.Natural.Triceratops.UnitType";
        internal const string StegosaurusUnitTypeSymbol =
            "KMG.Summoning.Natural.Stegosaurus.UnitType";
        internal const string TriceratopsChassisSymbol =
            "KMG.Summoning.Natural.Triceratops.Chassis";
        internal const string StegosaurusChassisSymbol =
            "KMG.Summoning.Natural.Stegosaurus.Chassis";
        internal const string TriceratopsPowerfulChargeSymbol =
            "KMG.Summoning.Special.Triceratops.PowerfulCharge";
        internal const string TriceratopsTrampleSymbol =
            "KMG.Summoning.Special.Triceratops.Trample";
        internal const string StegosaurusTripSymbol =
            "KMG.Summoning.Special.Stegosaurus.Trip";
        internal const string StegosaurusTailTypeSymbol =
            "KMG.Summoning.Natural.Stegosaurus.TailType";

        internal const int TriceratopsHitDice = 14;
        internal const int StegosaurusHitDice = 12;
        internal const int TriceratopsPerceptionRanks = 14;
        internal const int StegosaurusPerceptionRanks = 12;
        // Kingmaker's native Skill Focus carrier remains +3 after ten ranks,
        // while the tabletop feat becomes +6.  The Triceratops has fourteen
        // real Perception ranks and owns that printed feat, so its private
        // chassis supplies only the missing half of the feat progression.
        internal const int TriceratopsSkillFocusCompletionBonus = 3;
        internal const int TriceratopsBaseRacialHitPoints = 63;
        internal const int StegosaurusBaseRacialHitPoints = 54;

        // The released Sprint 22 native readback measured the generic Huge
        // reconstructed-animal chassis at twelve feet. Both Sprint 23 animals
        // use that same size construction, so the independently derived gap
        // to their printed fifteen feet is three. The Stegosaurus also rides
        // the exact captured Hodag/tail transform: its native tail exposes
        // nine feet until a private type stores nine (which the engine's
        // four-foot melee allowance exposes as five). No native type changes.
        internal const int ReachBonusFeet = 3;
        internal const int StegosaurusTailTypeStoredRangeFeet = 9;

        // At BAB +9/+10 native Power Attack supplies +6 to a lone primary
        // natural weapon. The printed 1.5x carrier is +9, leaving exactly +3.
        internal const int PowerAttackAdditionalDamage = 3;

        internal static int PowerfulChargeLiveBonus(int strengthModifier)
        {
            // The charge raises the ordinary 1.5x Strength contribution to
            // 2x. Pathfinder rounds fractional bonuses down, including when a
            // temporary penalty makes the modifier odd or negative.
            return (int)Math.Floor(strengthModifier / 2d);
        }

        internal static bool CanAttemptTailTrip(bool exactOwner,
            bool exactWeapon, bool hit, bool targetAvailable,
            bool firstCallback, bool alreadyProne, bool maneuverImmune,
            int ownerSizeOrder, int targetSizeOrder)
        {
            return exactOwner && exactWeapon && hit && targetAvailable &&
                firstCallback && !alreadyProne && !maneuverImmune &&
                ownerSizeOrder >= 0 && targetSizeOrder >= 0 &&
                targetSizeOrder <= ownerSizeOrder + 1;
        }

        internal static bool UsesExactRanks(string key)
        {
            return key == TriceratopsKey || key == StegosaurusKey;
        }

        internal static void Validate()
        {
            if (PowerfulChargeLiveBonus(8) != 4 ||
                PowerfulChargeLiveBonus(10) != 5 ||
                PowerfulChargeLiveBonus(6) != 3 ||
                ReachBonusFeet != 3 ||
                PowerAttackAdditionalDamage != 3 ||
                TriceratopsPerceptionRanks + 3 + 1 + 3 +
                    TriceratopsSkillFocusCompletionBonus != 24 ||
                TriceratopsBaseRacialHitPoints + 14 * 4 != 119 ||
                StegosaurusBaseRacialHitPoints + 12 * 3 != 90)
                throw new InvalidOperationException(
                    "Sprint 23 armored-dinosaur arithmetic changed.");
        }
    }
}
