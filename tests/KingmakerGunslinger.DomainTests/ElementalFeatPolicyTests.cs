using System;
using System.Linq;
using KingmakerGunslinger.ElementalRaces;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ElementalFeatPolicyTests
    {
        internal static void NativeAuditAndReleaseBPoliciesAreExact()
        {
            ElementalFeatNativeAuditTests.GuardedAuditIsReadOnlyAndExact();
            CatalogAndPrerequisitesAreExact();
            HydraulicPrerequisitesAndFormulaAreExact();
            ElementalStrikeScalingAndAttackBoundaryAreExact();
            ScorchingWeaponsSnapshotAndReplacementAreExact();
            SylphVisionBreathingAndAuraBoundariesAreExact();
        }

        internal static void CatalogAndPrerequisitesAreExact()
        {
            ElementalFeatDefinition[] feats =
                ElementalFeatPolicy.Ordered().ToArray();
            Assertions.Equal(11, feats.Length,
                "Release B must expose exactly the required feat catalog.");
            Assertions.Equal(4, feats.Count(entry => entry.IsCombat),
                "Only Elemental Strike and the Ifrit weapon chain are Combat feats.");

            ElementalFeatDefinition strike = F(ElementalFeatId.ElementalStrike);
            foreach (ElementalHeritageRace race in Enum.GetValues(
                typeof(ElementalHeritageRace)))
            {
                Assertions.True(ElementalFeatPolicy.Qualifies(strike,
                    Q(race, 1, false)),
                    "Every exact parent race and every heritage must qualify for Elemental Strike.");
            }
            Assertions.False(ElementalFeatPolicy.Qualifies(strike,
                new ElementalFeatQualification(null, 20, false, null)),
                "A native or foreign lookalike without an exact project race must fail closed.");

            ElementalFeatDefinition inner = F(ElementalFeatId.InnerFlame);
            Assertions.False(ElementalFeatPolicy.Qualifies(inner,
                Q(ElementalHeritageRace.Ifrit, 6, false,
                    ElementalFeatId.ScorchingWeapons)),
                "Inner Flame must retain its exact 7th-level prerequisite.");
            Assertions.False(ElementalFeatPolicy.Qualifies(inner,
                Q(ElementalHeritageRace.Ifrit, 7, false)),
                "Inner Flame must require Scorching Weapons.");
            Assertions.True(ElementalFeatPolicy.Qualifies(inner,
                Q(ElementalHeritageRace.Ifrit, 7, false,
                    ElementalFeatId.ScorchingWeapons)),
                "An exact level-7 Ifrit with Scorching Weapons must qualify.");

            ElementalFeatDefinition aura = F(ElementalFeatId.BlazingAura);
            Assertions.False(ElementalFeatPolicy.Qualifies(aura,
                Q(ElementalHeritageRace.Ifrit, 12, false,
                    ElementalFeatId.ScorchingWeapons,
                    ElementalFeatId.InnerFlame)),
                "Blazing Aura must require character level 13.");
            Assertions.True(ElementalFeatPolicy.Qualifies(aura,
                Q(ElementalHeritageRace.Ifrit, 13, false,
                    ElementalFeatId.ScorchingWeapons,
                    ElementalFeatId.InnerFlame)),
                "The complete Ifrit feat chain must qualify at level 13.");

            ElementalFeatDefinition wings = F(ElementalFeatId.WingsOfAir);
            Assertions.False(ElementalFeatPolicy.Qualifies(wings,
                Q(ElementalHeritageRace.Sylph, 9, false)),
                "Wings of Air must require Airy Step.");
            Assertions.True(ElementalFeatPolicy.Qualifies(wings,
                Q(ElementalHeritageRace.Sylph, 9, false,
                    ElementalFeatId.AiryStep)),
                "Wings of Air must unlock at exact character level 9.");

            ElementalFeatDefinition breath = F(ElementalFeatId.InnerBreath);
            Assertions.False(ElementalFeatPolicy.Qualifies(breath,
                Q(ElementalHeritageRace.Sylph, 10, false)),
                "Inner Breath must retain its exact level-11 prerequisite.");
            Assertions.True(ElementalFeatPolicy.Qualifies(breath,
                Q(ElementalHeritageRace.Sylph, 11, false)),
                "Inner Breath must unlock at exact character level 11.");
        }

        internal static void HydraulicPrerequisitesAndFormulaAreExact()
        {
            ElementalFeatDefinition maneuver =
                F(ElementalFeatId.HydraulicManeuver);
            ElementalFeatDefinition portal = F(ElementalFeatId.TritonPortal);
            Assertions.False(ElementalFeatPolicy.Qualifies(maneuver,
                Q(ElementalHeritageRace.Undine, 20, false)),
                "An Undine without the active racial Hydraulic Push must not qualify.");
            Assertions.True(ElementalFeatPolicy.Qualifies(maneuver,
                Q(ElementalHeritageRace.Undine, 1, true)),
                "An Undine with active racial Hydraulic Push must qualify.");
            Assertions.False(ElementalFeatPolicy.Qualifies(portal,
                Q(ElementalHeritageRace.Undine, 4, true)),
                "Triton Portal must require character level 5.");
            Assertions.False(ElementalFeatPolicy.Qualifies(portal,
                Q(ElementalHeritageRace.Undine, 5, false)),
                "Triton Portal must become unavailable when a trait replaces Hydraulic Push.");
            Assertions.True(ElementalFeatPolicy.Qualifies(portal,
                Q(ElementalHeritageRace.Undine, 5, true)),
                "A level-5 Undine with active Hydraulic Push must qualify.");

            ElementalHydraulicManeuver[] allowed =
                ElementalFeatPolicy.HydraulicManeuvers();
            Assertions.Equal(4, allowed.Length,
                "Only genuine native maneuver paths may be published.");
            Assertions.True(allowed.SequenceEqual(new[]
                {
                    ElementalHydraulicManeuver.BullRush,
                    ElementalHydraulicManeuver.Disarm,
                    ElementalHydraulicManeuver.Trip,
                    ElementalHydraulicManeuver.DirtyTrickBlind
                }), "Hydraulic Maneuver order and native options drifted.");
            Assertions.Equal(17,
                ElementalFeatPolicy.HydraulicManeuverBonus(12, 1, 5, 3),
                "Hydraulic Maneuver must use total level plus the current best mental modifier.");
            Assertions.Equal(19,
                ElementalFeatPolicy.HydraulicManeuverBonus(12, 7, 5, 3),
                "A temporary Intelligence change must affect the current formula.");
            Assertions.Equal(0,
                ElementalFeatPolicy.HydraulicManeuverBonus(0, 7, 5, 3),
                "An invalid preview level must fail closed.");

            Assertions.Equal(1,
                ElementalFeatPolicy.TritonPortalSmallWaterElementalCount(1),
                "Triton Portal's 1d3 minimum must remain one creature.");
            Assertions.Equal(3,
                ElementalFeatPolicy.TritonPortalSmallWaterElementalCount(3),
                "Triton Portal's 1d3 maximum must remain three creatures.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                ElementalFeatPolicy.TritonPortalSmallWaterElementalCount(4),
                "A non-d3 summon result must fail closed.");
            Assertions.Equal(12,
                ElementalFeatPolicy.TritonPortalDurationRounds(12),
                "Triton Portal must retain the native one-round-per-level summon duration.");
        }

        internal static void ElementalStrikeScalingAndAttackBoundaryAreExact()
        {
            int[] levels = { 0, 1, 4, 5, 9, 10, 14, 15, 19, 20, 40 };
            int[] expected = { 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5 };
            for (int index = 0; index < levels.Length; index++)
                Assertions.Equal(expected[index],
                    ElementalFeatPolicy.ElementalStrikeBonus(levels[index]),
                    "Elemental Strike breakpoint drifted at level " +
                    levels[index] + ".");

            Assertions.Equal(ElementalFeatEnergy.Fire,
                ElementalFeatPolicy.ElementalStrikeEnergy(
                    ElementalHeritageRace.Ifrit),
                "Ifrit damage must be fire.");
            Assertions.Equal(ElementalFeatEnergy.Acid,
                ElementalFeatPolicy.ElementalStrikeEnergy(
                    ElementalHeritageRace.Oread),
                "Oread damage must be acid.");
            Assertions.Equal(ElementalFeatEnergy.Electricity,
                ElementalFeatPolicy.ElementalStrikeEnergy(
                    ElementalHeritageRace.Sylph),
                "Sylph damage must be electricity.");
            Assertions.Equal(ElementalFeatEnergy.Cold,
                ElementalFeatPolicy.ElementalStrikeEnergy(
                    ElementalHeritageRace.Undine),
                "Undine damage must be cold.");

            Assertions.True(
                ElementalFeatPolicy.IsQualifyingElementalStrikeDamage(
                    true, true, true, false),
                "An active successful weapon damage bundle must qualify.");
            Assertions.False(
                ElementalFeatPolicy.IsQualifyingElementalStrikeDamage(
                    true, true, true, true),
                "Spell damage must never receive Elemental Strike.");
            Assertions.False(
                ElementalFeatPolicy.IsQualifyingElementalStrikeDamage(
                    true, false, true, false),
                "An unrelated descriptor-bearing action must not qualify.");

            ElementalFeatEventLedger ledger = new ElementalFeatEventLedger();
            Assertions.True(ledger.TryClaim("buff-a", "attack-rule-a"),
                "The first exact attack event must be accepted.");
            Assertions.False(ledger.TryClaim("buff-a", "attack-rule-a"),
                "Multiple engine passes over one attack must not double-apply.");
            Assertions.True(ledger.TryClaim("buff-a", "attack-rule-b"),
                "A distinct attack in the same round must still qualify.");
            ledger.ReleaseEffect("buff-a");
            Assertions.True(ledger.TryClaim("buff-a", "attack-rule-a"),
                "Ending an effect must clear only that effect's event claims.");
        }

        internal static void ScorchingWeaponsSnapshotAndReplacementAreExact()
        {
            string[] snapshot = ElementalFeatPolicy.SnapshotScorchingWeapons(
                new[]
                {
                    W("primary", true, true, true),
                    W("secondary", true, true, true),
                    W("third", true, true, true),
                    W("bow", true, true, false),
                    W("claw", true, false, false),
                    W("unequipped", false, true, true),
                    W("primary", true, true, true)
                });
            Assertions.Equal(2, snapshot.Length,
                "Activation must snapshot at most two qualifying held weapons.");
            Assertions.Equal("primary", snapshot[0],
                "The primary held weapon must retain deterministic order.");
            Assertions.Equal("secondary", snapshot[1],
                "The secondary held weapon must retain deterministic order.");

            ElementalFeatDamageAmount baseDamage =
                ElementalFeatPolicy.ScorchingWeaponsDamage(true, false, false);
            Assertions.Equal(1, baseDamage.FlatBonus,
                "Scorching Weapons must add exactly one fire damage.");
            Assertions.Equal(0, baseDamage.DiceCount,
                "The base feat must not add a die.");
            ElementalFeatDamageAmount improved =
                ElementalFeatPolicy.ScorchingWeaponsDamage(true, true, false);
            Assertions.Equal(1, improved.DiceCount,
                "Inner Flame must replace base damage with one die.");
            Assertions.Equal(6, improved.DieSides,
                "Inner Flame must use a d6.");
            Assertions.Equal(0, improved.FlatBonus,
                "Inner Flame must not stack the base +1 with its d6.");
            Assertions.True(ElementalFeatPolicy.ScorchingWeaponsDamage(
                    true, true, true).IsEmpty,
                "Another fire-damage weapon effect must suppress Scorching Weapons.");

            Assertions.Equal(2,
                ElementalFeatPolicy.ScorchingWeaponsSaveBonus(
                    true, false, true, true, true),
                "Overlapping base save conditions must grant +2 only once.");
            Assertions.Equal(4,
                ElementalFeatPolicy.ScorchingWeaponsSaveBonus(
                    true, true, true, true, true),
                "Inner Flame must replace the save bonus with +4 total.");
            Assertions.Equal(0,
                ElementalFeatPolicy.ScorchingWeaponsSaveBonus(
                    true, true, false, false, false),
                "Unrelated saves must receive no bonus.");
        }

        internal static void SylphVisionBreathingAndAuraBoundariesAreExact()
        {
            Assertions.Equal(2, ElementalFeatPolicy.AiryStepSaveBonus(
                    true, false, true, true, true),
                "Descriptor and electricity-damage overlap must grant +2 once.");
            Assertions.Equal(4, ElementalFeatPolicy.AiryStepSaveBonus(
                    true, true, true, true, true),
                "Wings of Air must replace the Airy Step bonus with +4 total.");
            Assertions.Equal(0, ElementalFeatPolicy.AiryStepSaveBonus(
                    true, true, false, false, false),
                "Unrelated saves must receive no Sylph bonus.");
            Assertions.True(ElementalFeatPolicy.WingsOfAirIsActive(true, true),
                "No armor or light armor must permit the native flight abstraction.");
            Assertions.False(ElementalFeatPolicy.WingsOfAirIsActive(true, false),
                "Medium or heavy armor must dynamically suppress flight.");

            Assertions.True(ElementalFeatPolicy.FiresightIgnores(
                    ElementalConcealmentFamily.Fire) &&
                ElementalFeatPolicy.FiresightIgnores(
                    ElementalConcealmentFamily.Smoke),
                "Firesight must ignore only fire and smoke families.");
            Assertions.False(ElementalFeatPolicy.FiresightIgnores(
                    ElementalConcealmentFamily.FogMistOrCloud) ||
                ElementalFeatPolicy.FiresightIgnores(
                    ElementalConcealmentFamily.Blur) ||
                ElementalFeatPolicy.FiresightIgnores(
                    ElementalConcealmentFamily.Invisibility),
                "Firesight must not become blanket concealment immunity.");
            Assertions.True(ElementalFeatPolicy.CloudGazerIgnores(
                    ElementalConcealmentFamily.FogMistOrCloud),
                "Cloud Gazer must ignore its exact environmental family.");
            Assertions.False(ElementalFeatPolicy.CloudGazerIgnores(
                    ElementalConcealmentFamily.Smoke) ||
                ElementalFeatPolicy.CloudGazerIgnores(
                    ElementalConcealmentFamily.Darkness) ||
                ElementalFeatPolicy.CloudGazerIgnores(
                    ElementalConcealmentFamily.Displacement),
                "Cloud Gazer must not consume Firesight or unrelated concealment domains.");
            Assertions.True(ElementalFeatPolicy.InnerBreathGrantsImmunity(true),
                "An explicitly respiration-dependent effect must be blocked.");
            Assertions.False(ElementalFeatPolicy.InnerBreathGrantsImmunity(false),
                "Cloudkill-style effects that do not require breathing must remain effective.");

            Assertions.True(ElementalFeatPolicy.BlazingAuraAffectsTurnStart(
                    true, true, true),
                "An adjacent creature beginning its turn must be affected, regardless of faction.");
            Assertions.False(ElementalFeatPolicy.BlazingAuraAffectsTurnStart(
                    true, false, true),
                "Aura damage must not trigger outside creature-turn start.");
            ElementalFeatEventLedger ledger = new ElementalFeatEventLedger();
            Assertions.True(ledger.TryClaim("aura", "friendly-turn-12"),
                "The first friendly creature turn-start must qualify.");
            Assertions.False(ledger.TryClaim("aura", "friendly-turn-12"),
                "The same creature-turn must not trigger twice.");
        }

        private static ElementalFeatDefinition F(ElementalFeatId id)
        {
            return ElementalFeatPolicy.Ordered().Single(entry =>
                entry.Id == id);
        }

        private static ElementalFeatQualification Q(
            ElementalHeritageRace race, int level, bool hydraulic,
            params ElementalFeatId[] feats)
        {
            return new ElementalFeatQualification(race, level, hydraulic,
                feats);
        }

        private static ElementalHeldWeaponCandidate W(string id, bool held,
            bool manufactured, bool metallic)
        {
            return new ElementalHeldWeaponCandidate(id, held, manufactured,
                metallic);
        }
    }
}
