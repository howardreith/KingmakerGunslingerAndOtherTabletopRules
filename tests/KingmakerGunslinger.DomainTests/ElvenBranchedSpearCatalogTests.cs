using System;
using System.IO;
using System.Linq;
using KingmakerGunslinger.ElvenBranchedSpear;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ElvenBranchedSpearCatalogTests
    {
        internal static void LockedProfileAndFoundationCatalogAreExact()
        {
            Assertions.Equal(0x004b4d47,
                ElvenBranchedSpearCatalog.WeaponCategoryValue,
                "Spear category identity changed.");
            Assertions.True(ElvenBranchedSpearCatalog.DamageDieCount == 1 &&
                ElvenBranchedSpearCatalog.DamageDieSides == 8 &&
                ElvenBranchedSpearCatalog.CriticalThreatMinimum == 20 &&
                ElvenBranchedSpearCatalog.CriticalMultiplier == 3 &&
                ElvenBranchedSpearCatalog.WeightPounds == 10 &&
                ElvenBranchedSpearCatalog.MovementAttackOfOpportunityBonus == 2,
                "Locked base profile changed.");
            ElvenBranchedSpearItemSpec[] items = ElvenBranchedSpearCatalog.All;
            Assertions.Equal(6, items.Length, "Foundation item count changed.");
            Assertions.Equal(6, items.Select(value => value.Kind).Distinct().Count(),
                "Foundation kinds are not unique.");
            Assertions.Equal(6, items.Select(value => value.Symbol).Distinct().Count(),
                "Foundation symbols are not unique.");
            Assertions.True(items.Select(value => value.Cost).SequenceEqual(
                new[] { 20, 320, 40, 340, 2320, 4340 }),
                "Foundation prices changed.");
            Assertions.True(items.Where(value => value.Enhancement > 0)
                .All(value => value.Masterwork),
                "A magic spear is not masterwork.");
            Assertions.True(items.Count(value => value.ColdIron) == 3 &&
                items.Count(value => value.Enhancement == 1) == 2,
                "Cold-iron or +1 coverage changed.");
            Assertions.False(items.Any(value =>
                value.DisplayName.IndexOf("Brace",
                    System.StringComparison.OrdinalIgnoreCase) >= 0),
                "Foundation catalog claims Brace.");
        }

        internal static void FoundationSourceContractsAreExact()
        {
            string root = Environment.CurrentDirectory;
            string blueprints = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Blueprints",
                "ElvenBranchedSpearBlueprints.cs"));
            foreach (string token in new[] {
                "NativeLongspearTypeGuid",
                "NativeElvenWeaponFamiliarityGuid",
                "NativeExoticWeaponProficiencySelectionGuid",
                "NativeFinesseTrainingSelectionGuid",
                "MovementOpportunityAccuracySymbol",
                "PhysicalDamageMaterial.ColdIron",
                "replacement.OnlyOneHanded = false",
                "replacement.TwoHandedBonus = true",
                "typeAdapter.Configure(clone, category, name, description,",
                "ConfigureEnchantmentText(value,",
                "movementAccuracy.EnchantmentCost != 0" })
                Assertions.True(blueprints.Contains(token),
                    "Spear blueprint foundation lacks: " + token);
            Assertions.False(blueprints.Contains("race ==") ||
                blueprints.Contains("Race ==") || blueprints.Contains("Brace" +
                    " attacks"),
                "Spear proficiency or description broadened beyond the locked rules.");

            string tracker = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "ElvenBranchedSpear",
                "MovementOpportunityAttackTracker.cs"));
            foreach (string token in new[] {
                "ConditionalWeakTable<UnitAttackOfOpportunity, Marker>",
                "method.DeclaringType != typeof(UnitCombatState)",
                "\"AttackOfOpportunity\"",
                "\"Disengage\"",
                "opportunity.IsRunning",
                "MethodType.Constructor" })
                Assertions.True(tracker.Contains(token),
                    "Movement-AoO correlation lacks: " + token);
            Assertions.False(tracker.Contains("IsCurrentUnit") ||
                tracker.Contains("distance") || tracker.Contains("animation"),
                "Movement-AoO correlation uses a forbidden inference.");

            string component = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "ElvenBranchedSpear",
                "MovementOpportunityAccuracyComponent.cs"));
            Assertions.True(component.Contains(
                "IInitiatorRulebookHandler<RuleCalculateAttackBonusWithoutTarget>") &&
                component.Contains("MovementOpportunityAttackTracker.IsRunning") &&
                component.Contains("evt.AddBonus(") && component.Contains("Fact);"),
                "Movement-AoO bonus must apply before resolution with a fact source.");

            string manifest = File.ReadAllText(Path.Combine(root,
                "blueprints", "blueprints.json"));
            foreach (string guid in new[] {
                "77f72b0febaf212a5650e7193c00361f",
                "6edc216d68810960f85417237748b042",
                "9c9edabf91f2117fd1b642c4d39b9574",
                "8c0de00a236fe0f532d31711dcaa00a2",
                "b16c34215cae9d60345042157149a4c0",
                "66111becd22690a2a19444a5c6bd0c7b",
                "25d8f6c6f4767b3168f4700a2890954f",
                "017d586ec4546feabf6eaaa67ce74a3f",
                "3843c643ffcc617faf9121a5f801a70e",
                "b0cabc2a4ac0135fab2f89c689dea389" })
                Assertions.Equal(2, manifest.Split(new[] { guid },
                    StringSplitOptions.None).Length,
                    "Stable spear identity is absent or duplicated: " + guid);
        }
    }
}
