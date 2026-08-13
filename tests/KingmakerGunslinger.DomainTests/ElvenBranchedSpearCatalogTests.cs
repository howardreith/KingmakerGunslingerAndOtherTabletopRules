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

        internal static void NamedCatalogAndTriggerPoliciesAreExact()
        {
            NamedSpearSpec[] items = ElvenBranchedSpearNamedCatalog.All;
            Assertions.Equal(6, items.Length, "Named spear count changed.");
            Assertions.Equal(6, items.Select(value => value.Kind).Distinct().Count(),
                "Named spear kinds are not unique.");
            Assertions.Equal(6, items.Select(value => value.Symbol).Distinct().Count(),
                "Named spear symbols are not unique.");
            Assertions.True(items.Select(value => value.Cost).SequenceEqual(
                new[] { 5320, 14320, 18340, 70320, 72320, 202340 }),
                "Named spear prices changed.");
            Assertions.True(items.Select(value => value.Enhancement).SequenceEqual(
                new[] { 1, 1, 2, 3, 4, 5 }),
                "Named spear enhancement progression changed.");
            Assertions.True(items.Count(value => value.Agile) == 4 &&
                items.Count(value => value.ColdIron) == 2 &&
                items.Single(value => value.Keen).Kind == NamedSpearKind.Thornstep &&
                items.Single(value => value.Corrosive).Kind == NamedSpearKind.VipersReach &&
                items.Single(value => value.Speed).Kind ==
                    NamedSpearKind.SpearOfTheFirstBranch,
                "Named spear native property map changed.");

            Assertions.True(NamedSpearEffectPolicy.Boughkeeper(true, true),
                "Boughkeeper must trigger on an AoO hit.");
            Assertions.False(NamedSpearEffectPolicy.Boughkeeper(false, true) ||
                NamedSpearEffectPolicy.Boughkeeper(true, false),
                "Boughkeeper accepted a miss or ordinary hit.");
            Assertions.True(NamedSpearEffectPolicy.Thornstep(true, true, true, false),
                "Thornstep rejected its exact trigger.");
            Assertions.False(NamedSpearEffectPolicy.Thornstep(true, true, false, false) ||
                NamedSpearEffectPolicy.Thornstep(true, true, true, true),
                "Thornstep accepted nonmovement or a second round use.");
            Assertions.True(NamedSpearEffectPolicy.VipersReach(true, 1, false),
                "Viper's Reach rejected applied sneak damage.");
            Assertions.False(NamedSpearEffectPolicy.VipersReach(true, 0, false) ||
                NamedSpearEffectPolicy.VipersReach(false, 10, false),
                "Viper's Reach accepted zero or ineligible sneak damage.");
            Assertions.True(NamedSpearEffectPolicy.BriarCrowned(true, true, false,
                false, 1), "Briar-Crowned rejected its exact trigger.");
            Assertions.False(NamedSpearEffectPolicy.BriarCrowned(true, true, true,
                false, 1) || NamedSpearEffectPolicy.BriarCrowned(true, true,
                false, false, 0),
                "Briar-Crowned accepted recursion or no remaining AoO.");
            Assertions.True(NamedSpearEffectPolicy.FirstBranch(true, true, true,
                5, false, false), "First Branch rejected a combined trigger.");
            Assertions.False(NamedSpearEffectPolicy.FirstBranch(true, false, false,
                0, false, false) || NamedSpearEffectPolicy.FirstBranch(true, true,
                false, 0, true, false) || NamedSpearEffectPolicy.FirstBranch(true,
                true, false, 0, false, true),
                "First Branch accepted an ordinary, repeated, or generated trigger.");
            Assertions.Equal(10, NamedSpearEffectPolicy
                .FirstBranchDifficultyClass(1, 0), "Level-one DC changed.");
            Assertions.Equal(24, NamedSpearEffectPolicy
                .FirstBranchDifficultyClass(20, 4), "Level-twenty DC changed.");
            Assertions.Throws<System.ArgumentOutOfRangeException>(() =>
                NamedSpearEffectPolicy.FirstBranchDifficultyClass(0, 0),
                "Invalid character level must fail closed.");
        }

        internal static void NamedBlueprintSourceContractsAreExact()
        {
            string root = Environment.CurrentDirectory;
            string effects = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "ElvenBranchedSpear",
                "ElvenBranchedSpearNamedEffects.cs"));
            foreach (string token in new[] {
                "ReferenceEquals(evt.Weapon, Owner)",
                "evt.AttackRoll.IsSneakAttackUsed",
                "value.Source.Sneak",
                "value.FinalValue > 0",
                "MovementOpportunityAttackTracker.IsRunning",
                "BriarGeneratedOpportunityAttackTracker.IsRunning",
                "AttackOfOpportunity(target, false)",
                "evt.AddBonus(-5, Fact)",
                "RuleSavingThrow(target, SavingThrowType.Fortitude, dc)",
                "CharacterLevel",
                "Stats.Dexterity.Bonus",
                "TimeSpan.FromSeconds(6d)" })
                Assertions.True(effects.Contains(token),
                    "Named spear mechanics lack: " + token);
            Assertions.False(effects.Contains("IsCurrentUnit") ||
                effects.Contains("animation") || effects.Contains("distance"),
                "Named spear mechanics use a forbidden attack-context inference.");

            string blueprints = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Blueprints",
                "ElvenBranchedSpearNamedBlueprints.cs"));
            foreach (string token in new[] {
                "NativeAgileGuid", "NativeKeenGuid", "NativeCorrosiveGuid",
                "NativeSpeedGuid", "NativeDirtyTrickEntangledBuffGuid",
                "StackingType.Replace", "StatType.Speed, -10",
                "StatType.SaveReflex, -2", "ModifierDescriptor.Dodge",
                "itemAccess.ConfigureNamed", "typeAccess.Set(clone, weaponType)" })
                Assertions.True(blueprints.Contains(token) ||
                    effects.Contains(token),
                    "Named spear blueprint graph lacks: " + token);

            string manifest = File.ReadAllText(Path.Combine(root,
                "blueprints", "blueprints.json"));
            string[] guids = {
                "4a084b0226e077b58d79e33184018002",
                "676faa5f811d851c9f14204bf864e1ec",
                "403d62f6d3bb415c86939430176e55c0",
                "1cfe40563a9b816931bb35e69677ac27",
                "ee580f43f50a0f0afefaedb3ce7133f3",
                "85c18b96ebee3fdc87eb33da93c8fdf6",
                "c777f06ec91be851794518fcdcc9c596",
                "89a27b8a22715a0b609912bc728dcb31",
                "be3a16e947fe8496a8301cbb2476cbcb",
                "62ef4362d84631574bacc977ffdad3e1",
                "2bba46654f15079769b0e6c741e8f803",
                "064feb1123cfb1ae4f541ef5e4d138a1",
                "339e83672ea2116e55640d175fec0c84",
                "7e2b2d36433396535555d39cc4066763",
                "6ac410ab82b81915d64249a213e1815a",
                "dcc7832d9ed7558111ee97da668522fe",
                "89cea1f236074e36051a68ece37aa05c",
                "1bb02c32918071bfa8333a12de4d7e94",
                "27d76fe829cc0234b7e120b19462848b" };
            Assertions.Equal(19, guids.Distinct().Count(),
                "Named spear identity list contains a collision.");
            foreach (string guid in guids)
                Assertions.Equal(2, manifest.Split(new[] { guid },
                    StringSplitOptions.None).Length,
                    "Named spear identity is absent or duplicated: " + guid);
        }
    }
}
