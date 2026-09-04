using System;
using System.IO;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ElementalFeatNativeAuditTests
    {
        internal static void GuardedAuditIsReadOnlyAndExact()
        {
            string scenario = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "ElementalFeatNativeAuditScenario.cs");
            string catalog = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestScenarioCatalog.cs");
            string runner = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs");
            string automation = Read("scripts",
                "RuntimeAutomation.Common.ps1");
            string preflight = Read("scripts",
                "Test-RuntimeScenarioPreflight.ps1");
            string compatibility = Read("scripts", "compatibility",
                "Invoke-KingmakerCompatibilityProfile.ps1");
            foreach (string token in new[]
            {
                "SaveStateTouched = false",
                "library.GetAllBlueprints()",
                "Enum.GetNames(",
                "SpellDescriptors",
                "HasNativeLightDescriptor",
                "AbilityDescriptor",
                "ParentAbilityGuid",
                "DirtyTrickBlind",
                "ContextActionSpawnMonster",
                "WeaponEnergyDamageDice",
                "AddConcealment",
                "ExactBlueprintContracts",
                "FormatContract(",
                "70cffb448c132fa409e49156d013b175",
                "08ae1c01155a2184db869e9ebedc758d",
                "25699a90ed3299e438b6fd5548930809",
                "61b312b8f91cc48418768b77cd6dcc02",
                "30f90becaaac51f41bf56641966c4121",
                "107788f47c4481f4db6da06498b28270",
                "04944455200bc224d955a8e9bbd64f3f",
                "56372b0a2749c224392a5ee74105c534"
            })
                Assertions.True(scenario.Contains(token),
                    "Release B native audit is missing exact evidence token " +
                    token + ".");
            foreach (string source in new[]
            {
                catalog, automation, preflight, compatibility
            })
                Assertions.True(source.Contains(
                        "observe-elemental-feat-native-contracts"),
                    "Release B native audit is outside a guarded allowlist or dispatch surface.");
            Assertions.True(runner.Contains(
                    ".ObserveElementalFeatNativeContracts") &&
                runner.Contains("ElementalFeatNativeAuditScenario.Run("),
                "Release B native audit is outside the central constant-based dispatch surface.");
            foreach (string forbidden in new[]
            {
                "SaveManager", "SaveGame", "AddFact(", "CreateUnit(",
                "UnitSpawner", "Spend(", "Restore("
            })
                Assertions.False(scenario.Contains(forbidden),
                    "Read-only Release B audit contains mutating surface " +
                    forbidden + ".");
        }

        private static string Read(params string[] path)
        {
            return File.ReadAllText(Path.Combine(FindRoot(),
                Path.Combine(path)));
        }

        private static string FindRoot()
        {
            DirectoryInfo current = new DirectoryInfo(
                AppDomain.CurrentDomain.BaseDirectory);
            while (current != null && !File.Exists(Path.Combine(
                current.FullName, "KingmakerGunslinger.sln")))
                current = current.Parent;
            if (current == null)
                throw new DirectoryNotFoundException(
                    "Could not locate the repository root.");
            return current.FullName;
        }

        internal static void MechanicsScenarioIsDedicatedAndGuarded()
        {
            string mechanics = Read("src", "KingmakerGunslinger",
                "ElementalRaces", "ElementalFeatRuleComponents.cs");
            string factory = Read("src", "KingmakerGunslinger",
                "ElementalRaces", "ElementalFeatBlueprintFactory.cs");
            string scenario = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "ElementalFeatMechanicsScenario.cs");
            string ifritScenario = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "ElementalIfritFeatScenario.cs");
            string catalog = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestScenarioCatalog.cs");
            string runner = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs");
            string project = Read("src", "KingmakerGunslinger",
                "KingmakerGunslinger.csproj");
            string automation = Read("scripts",
                "RuntimeAutomation.Common.ps1");
            string preflight = Read("scripts",
                "Test-RuntimeScenarioPreflight.ps1");
            string compatibility = Read("scripts", "compatibility",
                "Invoke-KingmakerCompatibilityProfile.ps1");

            foreach (string token in new[]
            {
                "RuleInitiatorLogicComponent<RulePrepareDamage>",
                "ConditionalWeakTable<RuleDealDamage, object>",
                "ReferenceEquals(evt.DamageBundle.Weapon, attack.Weapon)",
                "IsSpellDamage(damage)",
                "PreRolledValue = bonus",
                "ElementalWingsOfAirController",
                "ArmorProficiencyGroup.Light",
                "ElementalScorchingWeaponsAbilityLogic",
                "ElementalScorchingWeaponsDamage",
                "ElementalScorchingWeaponsSaveBonus",
                "RemoveOnUnequipItem = false",
                "ModifierDescriptor.Racial"
            })
                Assertions.True(mechanics.Contains(token),
                    "Elemental Feat runtime mechanics are missing boundary " +
                    token + ".");
            Assertions.False(mechanics.Contains("HarmonyPatch"),
                "The first Elemental Feat mechanics slice must not introduce a global patch.");
            foreach (string token in new[]
            {
                "ConfigureElementalStrike(strikeBuff, races)",
                "CreateWingsBuff(icon)",
                "ACBonusAgainstAttacks",
                "AddConditionImmunity",
                "BuffDescriptorImmunity",
                "ConfigureWingsFeature",
                "ConfigureScorchingWeapons"
            })
                Assertions.True(factory.Contains(token),
                    "Elemental Feat factory wiring is missing " + token + ".");
            foreach (string token in new[]
            {
                "UnitUseAbility",
                "RuleAttackWithWeapon",
                "RulePrepareDamage",
                "AttackRoll.ACRule.BonusSources",
                "AttackRoll.TargetAC",
                "IsTargetFlatFooted",
                "PrimaryHand.InsertItem",
                "MeleeAcLightWithoutWings",
                "CombatState.LeaveCombat",
                "Armor.RemoveItem(false)",
                "StandardHeavyCrossbowGuid",
                "UnitCondition.DifficultTerrain",
                "SpellDescriptor.Ground",
                "ArmorProficiencyGroup.Light",
                "ArmorProficiencyGroup.Medium",
                "SaveStateTouched = false"
            })
                Assertions.True(scenario.Contains(token),
                    "Dedicated Elemental Feat scenario is missing live boundary " +
                    token + ".");
            foreach (string token in new[]
            {
                "UnitUseAbility",
                "PrimaryHand.InsertItem",
                "SecondaryHand.InsertItem",
                "RemoveOnUnequipItem",
                "RuleAttackWithWeapon",
                "RulePrepareDamage",
                "RuleSavingThrow",
                "FlamingEnchantmentGuid",
                "WeaponSubCategory.Metal",
                "SaveStateTouched = false",
                "Game.Instance.State.Units.All.Remove(unit)"
            })
                Assertions.True(ifritScenario.Contains(token),
                    "Dedicated Ifrit feat scenario is missing live boundary " +
                    token + ".");
            foreach (string source in new[]
            {
                catalog, automation, preflight, compatibility
            })
                Assertions.True(source.Contains(
                        "disposable-elemental-feat-mechanics"),
                    "Elemental Feat mechanics are outside a guarded allowlist.");
            foreach (string source in new[]
            {
                catalog, automation, preflight, compatibility
            })
                Assertions.True(source.Contains(
                        "disposable-elemental-ifrit-feats"),
                    "Ifrit feat mechanics are outside a guarded allowlist.");
            Assertions.True(runner.Contains(
                    ".DisposableElementalFeatMechanics") &&
                runner.Contains("ElementalFeatMechanicsScenario.Run("),
                "Elemental Feat mechanics are outside constant-based dispatch.");
            Assertions.True(runner.Contains(
                    ".DisposableElementalIfritFeats") &&
                runner.Contains("ElementalIfritFeatScenario.Run("),
                "Ifrit feat mechanics are outside constant-based dispatch.");
            Assertions.Equal(2,
                catalog.Split(new[] { "DisposableElementalFeatMechanics" },
                    StringSplitOptions.None).Length - 1,
                "Elemental Feat mechanics must have one constant declaration and " +
                "one executable catalog entry.");
            Assertions.Equal(2,
                catalog.Split(new[] { "DisposableElementalIfritFeats" },
                    StringSplitOptions.None).Length - 1,
                "Ifrit feat mechanics must have one constant declaration and " +
                "one executable catalog entry.");
            Assertions.True(project.Contains(
                    "ElementalFeatRuleComponents.cs") &&
                project.Contains("ElementalFeatMechanicsScenario.cs") &&
                project.Contains("ElementalIfritFeatScenario.cs"),
                "Elemental Feat mechanics or scenario is outside the build.");
        }
    }
}
