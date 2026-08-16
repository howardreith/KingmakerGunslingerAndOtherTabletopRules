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
                "Brown-Fur Transmuter", "ScoreActivatables",
                "BrownFurActivatableGroupRuntime.PowerfulChangeGroup",
                "ResourceSpendType.Never",
                "PowerfulChangeReplacementLevel",
                "ShareTransmutationReplacementLevel", "MagicalSupremacy",
                "ReplaceStartingEquipment = false", "Array.Empty<BlueprintItem>()",
                "Activating or deactivating Share Transmutation costs nothing"
            }) Assertions.True(source.Contains(token),
                "Brown-Fur blueprint shell lost contract token: " + token);
            Assertions.False(source.Contains(
                    "contract.ArcanistClass.Archetypes ="),
                "Blueprint construction must not publish outside the transaction.");
        }

        internal static void HumanReviewPresentationRepairIsExplicit()
        {
            string source = System.IO.File.ReadAllText(System.IO.Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Blueprints", "BrownFurBlueprints.cs"));
            foreach (string token in new[] {
                "CreateScoreActivatable", "scoreActivatables.Cast<BlueprintUnitFact>()",
                "WeightInGroup = 1", "IsOnByDefault = false",
                "DeactivateIfCombatEnded = false", "OnlyInCombat = false",
                "ActivatableAbilityResourceLogic", "RequiredResource = reservoir",
                "ResourceSpendType.Never", "HideBuffInUi(buff)",
                "increase that spell's bonus to the chosen score by 2",
                "preserves its bonus type", "non-Arcanist spellbooks are not eligible",
                "does not spend a reservoir point or consume the selection",
                "enters creature-target selection", "within 30 feet",
                "Canceling target selection spends no reservoir point and no spell slot",
                "total cost of 2 reservoir points" })
                Assertions.True(source.Contains(token),
                    "Human-review presentation repair lacks token: " + token);

            foreach (string guid in new[] {
                "4c3d08935262b6544ae97599b3a9556d",
                "de7a025d48ad5da4991e7d3c682cf69d",
                "a900628aea19aa74aad0ece0e65d091a",
                "ae4d3ad6a8fda1542acf2e9bbc13d113",
                "f0455c9295b53904f9e02fc571dd2ce1",
                "446f7bf201dc1934f96ac0a26e324803",
                "5d4028eb28a106d4691ed1b92bbb1915" })
                Assertions.Equal(1, source.Split(new[] { guid },
                    StringSplitOptions.None).Length - 1,
                    "Native icon donor identity is missing or duplicated: " + guid);
            Assertions.True(source.Contains("all.Distinct().Count() != all.Length"),
                "Brown-Fur icon construction does not fail closed on duplicates.");
        }

        internal static void PreCommandTargetingRepairIsScoped()
        {
            string root = Environment.CurrentDirectory;
            string runtime = System.IO.File.ReadAllText(System.IO.Path.Combine(
                root, "src", "KingmakerGunslinger", "BrownFur",
                "BrownFurShareTargetingRuntime.cs"));
            string patches = System.IO.File.ReadAllText(System.IO.Path.Combine(
                root, "src", "KingmakerGunslinger", "BrownFur",
                "BrownFurShareTargetingPatches.cs"));
            string intent = System.IO.File.ReadAllText(System.IO.Path.Combine(
                root, "src", "KingmakerGunslinger", "BrownFur",
                "BrownFurCastIntentRuntime.cs"));
            string execution = System.IO.File.ReadAllText(System.IO.Path.Combine(
                root, "src", "KingmakerGunslinger", "BrownFur",
                "BrownFurCastExecutionRuntime.cs"));
            foreach (string token in new[] {
                "TryResolvePendingShareTargeting", "ability.Spellbook == null",
                "ability.SourceItem != null", "SpellSchool.Transmutation",
                "AbilityRange.Personal", "ShareTransmutationCompatibility",
                "anchor = AbilityTargetAnchor.Unit", "BrownFurShareTargetPolicy.Decide",
                "distance = ThirtyFeetMeters" })
                Assertions.True(runtime.Contains(token),
                    "Pre-command Share targeting lacks token: " + token);
            Assertions.True(patches.Contains("get_TargetAnchor") &&
                patches.Contains("CanTarget") &&
                patches.Contains("HarmonyAfter(\"CallOfTheWild\")"),
                "Share targeting is not applied at the early AbilityData boundary.");
            Assertions.False(intent.Contains("BrownFurPlayerIntentRuntime.Clear"),
                "Command construction still consumes pending player intent.");
            Assertions.True(execution.Contains("ConsumeCommittedIntent") &&
                execution.Contains("transaction.State != BrownFurCastTransactionState.Committed"),
                "Successful rule commitment is not the one-shot disarm boundary.");
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
