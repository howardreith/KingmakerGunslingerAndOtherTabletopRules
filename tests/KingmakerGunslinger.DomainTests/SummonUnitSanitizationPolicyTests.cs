using System;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class SummonUnitSanitizationPolicyTests
    {
        internal static void RemovesEveryForbiddenCampaignSurface()
        {
            foreach (SummonDonorHazard hazard in Enum.GetValues(typeof(SummonDonorHazard)))
            {
                if (hazard == SummonDonorHazard.None) continue;
                SummonUnitSanitizationPlan plan = SummonUnitSanitizationPolicy.CreatePlan(
                    new[] { new SummonDonorMember(hazard.ToString(), hazard, false) });
                Assertions.Equal(0, plan.Retained.Count, "Forbidden hazard was retained: " + hazard);
                Assertions.Equal(1, plan.Removed.Count, "Forbidden hazard was not removed: " + hazard);
            }
        }

        internal static void RetainsCombatAndRequiresSafeReplacement()
        {
            var attack = new SummonDonorMember("natural-attack", SummonDonorHazard.None, true);
            var unsafeBreath = new SummonDonorMember("breath-with-conjuration",
                SummonDonorHazard.CreatureSummoningOrConjuration, true);
            var loot = new SummonDonorMember("campaign-loot", SummonDonorHazard.Loot, false);
            SummonUnitSanitizationPlan plan = SummonUnitSanitizationPolicy.CreatePlan(
                new[] { attack, unsafeBreath, loot });
            Assertions.Equal(1, plan.Retained.Count, "Safe combat mechanics must be retained.");
            Assertions.True(ReferenceEquals(attack, plan.Retained[0]), "Policy must preserve retained references.");
            Assertions.Equal(2, plan.Removed.Count, "Both unsafe members must be removed.");
            Assertions.Equal(1, plan.RequiredReplacements.Count, "Removed required mechanics need explicit replacements.");
            Assertions.True(ReferenceEquals(unsafeBreath, plan.RequiredReplacements[0]), "Replacement identity changed.");
        }

        internal static void RejectsMalformedInventories()
        {
            Assertions.Throws<ArgumentNullException>(() => SummonUnitSanitizationPolicy.CreatePlan(null),
                "Null inventories must fail.");
            Assertions.Throws<ArgumentException>(() => SummonUnitSanitizationPolicy.CreatePlan(
                new SummonDonorMember[] { null }), "Null members must fail.");
            Assertions.Throws<ArgumentException>(() => SummonUnitSanitizationPolicy.CreatePlan(new[] {
                new SummonDonorMember("same", SummonDonorHazard.None, false),
                new SummonDonorMember("same", SummonDonorHazard.Loot, false)
            }), "Duplicate member identities must fail.");
        }
    }
}
