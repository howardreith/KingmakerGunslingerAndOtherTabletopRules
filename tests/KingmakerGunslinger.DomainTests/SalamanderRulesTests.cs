using System;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class SalamanderRulesTests
    {
        internal static void RacialContributionsProducePrintedTotals()
        {
            Assertions.Equal(76, SalamanderRulesPolicy.BaseHitPoints + 8 * 4, "8d10 average plus live Constitution.");
            Assertions.Equal(10, SalamanderRulesPolicy.GoodSave + 4, "Fortitude plus Constitution.");
            Assertions.Equal(7, SalamanderRulesPolicy.GoodSave + 1, "Reflex plus Dexterity.");
            Assertions.Equal(6, SalamanderRulesPolicy.PoorSave + 2 + 2, "Poor Will plus Wisdom and native Iron Will.");
            int mobility = 0, perception = 0, persuasion = 0;
            SalamanderRulesPolicy.AllocateLandRanks(ref mobility, ref perception, ref persuasion);
            Assertions.Equal(16, perception + 3 + 2 + 3, "Eight ranks, class skill, Wisdom and Skill Focus.");
            Assertions.Equal(0, mobility + persuasion, "No donor Mobility or Persuasion ranks.");
            Assertions.Equal(10, SalamanderRulesPolicy.TailReachFeet, "Only tail type has printed ten-foot reach.");
            Assertions.False(ExpandedSummoningSpecialProfiles.SalamanderSpearIsNatural, "Spear retains manufactured iteratives.");
        }

        internal static void ExactOwnerRejectsPrototypeAndDonors()
        {
            Assertions.True(SalamanderRulesPolicy.IsOwner(SalamanderRulesPolicy.UnitGuid, SalamanderRulesPolicy.UnitName), "Exact published identity.");
            foreach (string guid in new[] { null, "", "foreign", SalamanderRulesPolicy.UnitGuid.ToUpperInvariant() })
                Assertions.False(SalamanderRulesPolicy.IsOwner(guid, SalamanderRulesPolicy.UnitName), "GUID required.");
            foreach (string name in new[] { null, "", "KMG_Runtime_Sprint17_SalamanderHumanTail", "KMG_Summoning_Unit_ConstrictorSnake", "Lizardfolk" })
                Assertions.False(SalamanderRulesPolicy.IsOwner(SalamanderRulesPolicy.UnitGuid, name), "No prototype, snake or donor leakage.");
        }

        internal static void LandRanksRejectDonorAndRepeatedAllocation()
        {
            for (int index = 0; index < 3; index++)
            foreach (int value in new[] { -1, 1, 5, 8 })
            {
                int m = index == 0 ? value : 0, p = index == 1 ? value : 0, d = index == 2 ? value : 0;
                Assertions.Throws<InvalidOperationException>(() => SalamanderRulesPolicy.AllocateLandRanks(ref m, ref p, ref d), "Unexpected rank input rejects atomically.");
                Assertions.Equal(value, m + p + d, "No silent compensation of donor ranks.");
            }
        }

        internal static void ConstrictUsesLivePositiveAndNegativeStrength()
        {
            foreach (int strength in Enumerable.Range(-5, 19))
                Assertions.Equal(strength > 0 ? strength * 3 / 2 : strength,
                    SalamanderRulesPolicy.ConstrictStrengthBonus(strength), "1.5 positive Strength, negative penalty once.");
            Assertions.Equal(4, SalamanderRulesPolicy.ConstrictStrengthBonus(3), "Printed 2d6+4 plus separate fire.");
        }

        internal static void HeatClaimsArePerExactWeaponRuleWithoutReplay()
        {
            var claims = new SalamanderHeatClaims();
            var rule = new object();
            Assertions.False(claims.TryClaim(null, true, true), "No event.");
            Assertions.False(claims.TryClaim(rule, false, true), "Foreign owner.");
            Assertions.False(claims.TryClaim(rule, true, false), "Foreign weapon, spell or unarmed carrier.");
            Assertions.True(claims.TryClaim(rule, true, true), "Rejected requests did not consume the actual weapon rule.");
            Assertions.False(claims.TryClaim(rule, true, true), "Replayed event or second subscriber adds no second packet.");
            Assertions.True(claims.TryClaim(new object(), true, true), "Next real attack or maintain calculation gets one packet.");
        }

        internal static void RegistrationPreservesFivePublishedRoutesAndHiddenSnakes()
        {
            var ids = ExpandedSummoningIdentityCatalog.Build();
            Assertions.Equal(1, ids.Count(i => i.Symbol == "KMG.Summoning.Special.Salamander.TailType" && i.PlannedType == "BlueprintWeaponType"), "Only own tail type.");
            Assertions.Equal(1, ids.Count(i => i.Symbol == "KMG.Summoning.Special.Salamander.UnitType" && i.PlannedType == "BlueprintUnitType"), "Own inspection identity.");
            var variants = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                .Concat(ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly)).ToArray();
            var salamander = variants.Where(v => v.Creature.Key == "salamander").ToArray();
            Assertions.Equal(5, salamander.Length, "Existing five routes retained.");
            Assertions.True(salamander.All(SummonVisibilityCatalog.IsPublished), "No accidental Salamander suppression.");
            Assertions.Equal(0, variants.Count(v => !SummonVisibilityCatalog.IsPublished(v)), "Independent snake publication does not move or suppress existing Salamander roots.");
            Assertions.Equal("salamander", SummonIconCatalog.PassiveTraitIconFor(SummonIconCatalog.SalamanderTraitsSymbol), "Existing original species painting, no new art or action icon.");
        }
    }
}
