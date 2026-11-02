using System;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Sprint 37 Vrock contract. Printed values live here once; the live
    /// difficulty classes are still derived from the actual runtime stats
    /// through the released Sprint 36 formulas rather than hard-coded.
    /// </summary>
    internal static class VrockRules37Policy
    {
        internal const int VrockHitDice = 9;

        // 10 + 9/2 + Con 25 bonus 7 = 21, the printed Stunning Screech DC.
        // Checked against the live Constitution at cast time; this constant
        // only pins the printed expectation.
        internal const int PrintedScreechDc = 21;
        internal const int ScreechRadiusFeet = 30;
        internal const int ScreechStunRounds = 1;
        internal const int ScreechRechargeHours = 1;

        internal const int SporeRadiusFeet = 10;
        internal const int SporeCadenceRounds = 3;
        internal const int SporeVineRounds = 10;
        internal const int SporeImpactDiceCount = 1;
        internal const int SporeVineDiceCount = 1;

        internal const int SpellLikeCasterLevel = 12;

        // Kingmaker exposes one movement speed. The effective flying speed is
        // used with the established Airborne navigation fact; the land speed is
        // recorded and omitted, never summed or redistributed.
        internal const int EffectiveFlySpeedFeet = 50;
        internal const int OmittedLandSpeedFeet = 30;

        // Printed save totals, retained only so an observer can compare them
        // against the live values. The blueprint carries the base values below;
        // genuine native difficulty modifiers remain a separate finding and are
        // never compensated for in the gameplay numbers.
        internal const int PrintedFortitude = 13;
        internal const int PrintedReflex = 10;
        internal const int PrintedWill = 6;
        internal const int BaseFortitude = 6;
        internal const int BaseReflex = 6;
        internal const int BaseWill = 3;

        internal const int PrintedPerception = 23;
        internal const int PrintedStealth = 10;
        internal const int SkillRanks = 9;
        internal const int RacialPerceptionBonus = 8;

        internal const int NaturalArmor = 11;
        internal const int SpellResistance = 20;
        internal const int HitPoints = 112;
        internal const int ReachFeet = 10;

        /// <summary>
        /// The live screech DC. Constitution-based exactly like the released
        /// Sprint 36 stench, so the formula is shared rather than repeated.
        /// </summary>
        internal static int ScreechDifficultyClass(int constitutionBonus)
        {
            return InfernalRules36Policy.ConstitutionDc(VrockHitDice,
                constitutionBonus);
        }

        /// <summary>
        /// Whether one activation of a recharging, exact-owner ability may
        /// proceed. Shared by the screech's hour recharge and the spores'
        /// three-round cadence; both gate through the native
        /// <c>AbilityCasterHasNoFacts</c> on their own marker buff, and this
        /// predicate is the same decision in a testable form.
        /// </summary>
        internal static bool CanActivate(bool exactOwner, bool blocked,
            bool grantedAvailable)
        { return exactOwner && !blocked && grantedAvailable; }

        /// <summary>
        /// Whether the screech reaches one collected creature. The real effect
        /// keeps native all-creature eligibility: only the printed demon
        /// exemption is applied here, and native stun immunity remains with its
        /// own consumer rather than being pre-filtered.
        /// </summary>
        internal static bool ScreechReaches(bool exactSource, bool demon)
        { return exactSource && !demon; }

        /// <summary>
        /// Whether the spore cloud reaches one collected creature. The printed
        /// ability has no save, no spell resistance and no exemption, so only
        /// the exact source and a live target are required.
        /// </summary>
        internal static bool SporesReach(bool exactSource, bool liveTarget)
        { return exactSource && liveTarget; }

        internal static void Validate()
        {
            if (ScreechDifficultyClass(7) != PrintedScreechDc ||
                !CanActivate(true, false, true) ||
                CanActivate(false, false, true) ||
                CanActivate(true, true, true) ||
                CanActivate(true, false, false) ||
                !ScreechReaches(true, false) ||
                ScreechReaches(true, true) ||
                ScreechReaches(false, false) ||
                !SporesReach(true, true) ||
                SporesReach(true, false) ||
                SporesReach(false, true) ||
                BaseFortitude + 7 != PrintedFortitude ||
                BaseReflex + 2 + 2 != PrintedReflex ||
                BaseWill + 3 != PrintedWill ||
                SkillRanks + 3 + 3 + RacialPerceptionBonus !=
                    PrintedPerception ||
                SkillRanks + 3 + 2 - 4 != PrintedStealth ||
                ScreechRadiusFeet != 30 || ScreechStunRounds != 1 ||
                ScreechRechargeHours != 1 || SporeRadiusFeet != ReachFeet ||
                SporeCadenceRounds != 3 || SporeVineRounds != 10 ||
                SporeImpactDiceCount != 1 || SporeVineDiceCount != 1 ||
                SpellLikeCasterLevel != 12 ||
                EffectiveFlySpeedFeet != 50 ||
                OmittedLandSpeedFeet != 30 ||
                NaturalArmor != 11 || SpellResistance != 20 ||
                HitPoints != 112 || VrockHitDice != 9)
                throw new InvalidOperationException(
                    "Sprint 37 Vrock rules contract changed.");
        }
    }
}
