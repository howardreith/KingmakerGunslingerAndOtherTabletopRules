using System;
using System.Linq;
using KingmakerGunslinger.BrownFur;

namespace KingmakerGunslinger.DomainTests
{
    internal static class BrownFurArchetypePlanTests
    {
        internal static void NormalProgressionIsExact()
        {
            AssertPlan(CotwProgressionPolicy.Resolve(
                new[] { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19 }),
                "3:ArcanistExploit|9:ArcanistExploit|20:MagicalSupremacy");
        }

        internal static void BalanceProgressionIsExact()
        {
            AssertPlan(CotwProgressionPolicy.Resolve(
                new[] { 1, 4, 7, 10, 13, 16, 19 }),
                "4:ArcanistExploit|10:ArcanistExploit|20:MagicalSupremacy");
        }

        internal static void UnknownProgressionCannotBuildShell()
        {
            Assertions.Throws<InvalidOperationException>(() =>
                BrownFurArchetypePlan.Create(CotwProgressionPolicy.Resolve(
                    new[] { 1, 3, 6, 9 })),
                "Unknown CotW progression built a Brown-Fur archetype shell.");
        }

        internal static void BlueprintBuilderRetainsPlayerContract()
        {
            string source = System.IO.File.ReadAllText(System.IO.Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Blueprints", "BrownFurBlueprints.cs"));
            foreach (string token in new[] {
                "Brown-Fur Transmuter", "AbilityVariants",
                "BrownFurPowerfulChangeSelectionLogic.Create",
                "PowerfulChangeReplacementLevel",
                "ShareTransmutationReplacementLevel", "MagicalSupremacy",
                "ReplaceStartingEquipment = false", "Array.Empty<BlueprintItem>()",
                "No reservoir point is spent merely for activating this selection"
            }) Assertions.True(source.Contains(token),
                "Brown-Fur blueprint shell lost contract token: " + token);
            Assertions.False(source.Contains(
                    "contract.ArcanistClass.Archetypes ="),
                "Blueprint construction must not publish outside the transaction.");
        }

        private static void AssertPlan(CotwProgressionDecision decision,
            string removals)
        {
            BrownFurArchetypePlan plan = BrownFurArchetypePlan.Create(decision);
            Assertions.Equal(
                "3:PowerfulChange|9:ShareTransmutation|20:TransmutationSupremacy",
                string.Join("|", plan.Additions.Select(value =>
                    value.Level + ":" + value.Feature)),
                "Brown-Fur addition levels changed.");
            Assertions.Equal(removals, string.Join("|", plan.Removals.Select(
                value => value.Level + ":" + value.Feature)),
                "Brown-Fur removal levels changed.");
        }
    }
}
