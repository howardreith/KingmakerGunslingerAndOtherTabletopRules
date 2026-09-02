using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ElementalRaceProductionTests
    {
        private static readonly IDictionary<string, string> ExpectedIds =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "KMG.ElementalRaces.Ifrit.Race", "556a2d9ae0c6401eaed87614a2caf539" },
                { "KMG.ElementalRaces.Ifrit.FireResistance", "34bd9d1bb97d4c5a90a5440dbea13462" },
                { "KMG.ElementalRaces.Ifrit.FireAffinity", "7e209cef26bd414fb3ab9e66de3fd2d9" },
                { "KMG.ElementalRaces.Ifrit.BurningHandsFeature", "71ccaea82a4948cfa82c50b9bea5ff5c" },
                { "KMG.ElementalRaces.Ifrit.BurningHandsResource", "04e5fa42bffd4ab4b305e56dec7ccb0d" },
                { "KMG.ElementalRaces.Ifrit.BurningHandsAbility", "3f2c575bec854971bfe5390aa01fcf83" },
                { "KMG.ElementalRaces.Oread.Race", "7ef60bcda0204429bf4859e2faa3cbf8" },
                { "KMG.ElementalRaces.Oread.AcidResistance", "718bed8a80514e1bab6a71a217412c42" },
                { "KMG.ElementalRaces.Oread.AcidAffinity", "619db6814d9c45ae90f1d3dde9815402" },
                { "KMG.ElementalRaces.Oread.StoneFistFeature", "b217ceb3f24f4f7fbd4b33a5d14e2869" },
                { "KMG.ElementalRaces.Oread.StoneFistResource", "e90357b48674496da461a8f36a1080a3" },
                { "KMG.ElementalRaces.Oread.StoneFistAbility", "991d605c411343308177004ed88aa693" },
                { "KMG.ElementalRaces.Sylph.Race", "68b64570c6e943f1bcbe4571e88bf285" },
                { "KMG.ElementalRaces.Sylph.ElectricityResistance", "4e8386b0fc4545cd9a6ce02bd0de8563" },
                { "KMG.ElementalRaces.Sylph.AirAffinity", "3a83a5a03f8d4354b4060316e50e7784" },
                { "KMG.ElementalRaces.Sylph.FeatherStepFeature", "a78d04243b894158b1802fa97d42f770" },
                { "KMG.ElementalRaces.Sylph.FeatherStepResource", "57397d931b344693b9d8f08e6cda1655" },
                { "KMG.ElementalRaces.Sylph.FeatherStepAbility", "39ced3ba38884b28820772a4de517ef9" },
                { "KMG.ElementalRaces.Undine.Race", "557dea40c2cc440f8afe7d678d2d283a" },
                { "KMG.ElementalRaces.Undine.ColdResistance", "672808b5db3146dfae98879ebd9edab7" },
                { "KMG.ElementalRaces.Undine.WaterAffinity", "0ecda8a5185742e5a80a6c6deb0bf609" },
                { "KMG.ElementalRaces.Undine.HydraulicPushFeature", "7a040c051ef04e33921a3224ff03f4b0" },
                { "KMG.ElementalRaces.Undine.HydraulicPushResource", "c2663ff520804fd5840e209c23725dda" },
                { "KMG.ElementalRaces.Undine.HydraulicPushAbility", "df0f9d05341a4eb59af8c369e447843f" }
            };

        internal static void StableManifestInventoryIsExact()
        {
            JObject manifest = JObject.Parse(Read("blueprints",
                "blueprints.json"));
            JToken[] all = manifest["entries"].ToArray();
            JToken[] elemental = all.Where(value =>
                ((string)value["symbol"]).StartsWith(
                    "KMG.ElementalRaces.", StringComparison.Ordinal) &&
                string.Equals((string)value["status"], "active",
                    StringComparison.Ordinal)).ToArray();
            Assertions.Equal(24, elemental.Length,
                "Production elemental identity count changed.");
            Assertions.Equal(1662, all.Length,
                "Manifest total must include 24 production elemental identities.");
            Assertions.Equal(1660, all.Count(value => string.Equals(
                (string)value["status"], "active", StringComparison.Ordinal)),
                "Manifest active count must include all elemental identities.");
            Assertions.Equal(all.Length, all.Select(value =>
                (string)value["guid"]).Distinct(StringComparer.Ordinal).Count(),
                "Manifest contains a GUID collision.");
            foreach (KeyValuePair<string, string> expected in ExpectedIds)
            {
                JToken[] matches = elemental.Where(value => string.Equals(
                    (string)value["symbol"], expected.Key,
                    StringComparison.Ordinal)).ToArray();
                Assertions.Equal(1, matches.Length,
                    "Missing or duplicate elemental identity " + expected.Key);
                Assertions.Equal(expected.Value, (string)matches[0]["guid"],
                    "Elemental GUID changed for " + expected.Key);
            }
            string catalog = Source("ElementalRaceIdentityCatalog.cs");
            Assertions.True(catalog.Contains("IdentityCount = 24") &&
                ExpectedIds.Keys.All(catalog.Contains),
                "Identity catalog and manifest symbols drifted.");
        }

        internal static void CatalogMatchesApprovedRules()
        {
            string catalog = Source("ElementalRaceCatalog.cs");
            foreach (string token in new[]
            {
                "ElementalRaceKind.Ifrit", "StatType.Dexterity, 2",
                "StatType.Charisma, 2", "StatType.Wisdom, -2",
                "DamageEnergyType.Fire, SpellDescriptor.Fire",
                "ElementalRaceKind.Oread", "StatType.Strength, 2",
                "StatType.Wisdom, 2", "StatType.Charisma, -2",
                "DamageEnergyType.Acid, SpellDescriptor.Acid, true",
                "ElementalRaceKind.Sylph", "StatType.Intelligence, 2",
                "StatType.Constitution, -2",
                "DamageEnergyType.Electricity",
                "SpellDescriptor.Electricity",
                "Feather Step is Kingmaker's practical substitute for Feather Fall",
                "ElementalRaceKind.Undine", "StatType.Strength, -2",
                "DamageEnergyType.Cold", "SpellDescriptor.Cold",
                "Kingmaker has no ordinary player swimming system",
                "total character level"
            })
                Assertions.True(catalog.Contains(token),
                    "Approved elemental race rule token is absent: " + token);
            Assertions.False(catalog.Contains("darkvision") ||
                catalog.Contains("swim speed") || catalog.Contains("caster level bonus"),
                "Deferred tabletop systems leaked into the base-race catalog.");
        }

        internal static void SlaAffinityAndHydraulicContractsAreNarrow()
        {
            string ability = Source("ElementalRaceAbilityFactory.cs");
            string rules = Source("ElementalRaceRuleComponents.cs");
            foreach (string token in new[]
            {
                "ability.Type = AbilityType.SpellLike",
                "ability.Parent = null", "component is SpellListComponent",
                "component is AbilityResourceLogic",
                "fullName.StartsWith(\"Kingmaker.\"",
                "RequiredResource = resource",
                "Amount = 1", "ResourceCost(resource, true)",
                "RestoreAmount = true",
                "RestoreOnLevelUp = false", "UseThisAsResource = false",
                "CombatManeuver.BullRush",
                "maneuver.ReplaceStat = true",
                "UseCasterLevelAsBaseAttack = true",
                "UseBestMentalStat = true",
                "ElementalHydraulicResourceCommit",
                "ResourceCost(resource, true)",
                "SavingThrowType.Unknown", "SpellResistance = true"
            })
                Assertions.True(ability.Contains(token),
                    "Racial SLA contract is absent: " + token);
            foreach (string token in new[]
            {
                "evt.AddBonusDC(1)", "current = current.Parent",
                "public int DescriptorMask",
                "(SpellDescriptor)DescriptorMask",
                "Owner.Progression.CharacterLevel",
                "evt.ReplaceCasterLevel", "evt.ReplaceSpellLevel",
                "evt.ReplaceStat = Stat", "StatType.Charisma",
                "ElementalHydraulicResourceCommit",
                "GetResourceAmount(Resource) <= 0",
                "Resources.Spend(Resource, 1)"
            })
                Assertions.True(rules.Contains(token),
                    "Racial parameter/affinity contract is absent: " + token);
            Assertions.False(rules.Contains("AddBonusCasterLevel") ||
                rules.Contains("HarmonyPatch"),
                "Elemental affinity must not add caster level or patch spells globally.");
        }

        internal static void RegistrationAndPublicationAreSaveSafe()
        {
            string factory = Source("ElementalRaceBlueprintFactory.cs");
            string publication = Source("ElementalRacePublication.cs");
            string bootstrap = Read("src", "KingmakerGunslinger",
                "Bootstrap", "BlueprintBootstrap.cs");
            foreach (string token in new[]
            {
                "AasimarRaceGuid", "OutsiderTypeGuid", "KeenSensesGuid",
                "SlowAndSteadyGuid", "BlueprintCloneService.Clone(aasimar",
                "race.RaceId != aasimar.RaceId",
                "ModifierDescriptor.Racial", "ResistanceValue = 5"
            })
                Assertions.True(factory.Contains(token),
                    "Race factory safety contract is absent: " + token);
            foreach (string token in new[]
            {
                "previous.Concat(missing).ToArray()",
                "root.Progression.CharacterRaces = published",
                "root.Progression.CharacterRaces = previous",
                "ReferenceEquals(published[index], previous[index])",
                "published.Count(value => ReferenceEquals(value, race)) != 1",
                "missing.Contains(value)", "rollback refused"
            })
                Assertions.True(publication.Contains(token),
                    "Atomic race publication contract is absent: " + token);
            Assertions.True(bootstrap.Contains(
                    "ElementalRaceBlueprintFactory.Register(library, registry)") &&
                bootstrap.Contains("publicationPlan.ElementalRaceSelectors") &&
                bootstrap.Contains("ElementalRaceIdentityCatalog.IdentityCount") &&
                bootstrap.Contains("elementalRacePublication.Rollback()"),
                "Bootstrap does not unconditionally register and transactionally publish elemental identities.");
            Assertions.False(factory.Contains("CharacterRaces"),
                "Race identity construction must remain separate from selector publication.");
        }

        internal static void RuntimeMechanicsScenarioIsGuardedAndNative()
        {
            string scenario = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "ElementalRaceMechanicsScenario.cs");
            foreach (string token in new[]
            {
                "Rulebook.Trigger(new RuleDealDamage",
                "new EnergyDamage(", "LevelUpController",
                "GetClassLevel(fighter)", "GetClassLevel(wizard)",
                "new AbilityData(granted)", "GetAvailableForCastCount()",
                "AbilityData).GetMethod(", "Spend",
                "RestController.ApplyRest(owner)",
                "PersistantResources.Single", "DefaultJsonSettings",
                "ModifierDescriptor.Racial", "AddAssertions(assertions",
                "SameReferences(unitsBefore",
                "ContractResolver = new DefaultContractResolver()",
                "PreserveReferencesHandling.None"
            })
                Assertions.True(scenario.Contains(token),
                    "Native guarded mechanics token is absent: " + token);
            string catalog = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestScenarioCatalog.cs");
            string runner = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs");
            string automation = Read("scripts",
                "RuntimeAutomation.Common.ps1");
            string preflight = Read("scripts",
                "Test-RuntimeScenarioPreflight.ps1");
            string project = Read("src", "KingmakerGunslinger",
                "KingmakerGunslinger.csproj");
            const string name = "disposable-elemental-race-mechanics";
            Assertions.True(catalog.Contains(name) &&
                runner.Contains("ElementalRaceMechanicsScenario.Run(") &&
                automation.Contains("'" + name +
                    "' = [pscustomobject]") &&
                preflight.Contains("'" + name + "'") &&
                project.Contains("ElementalRaceMechanicsScenario.cs"),
                "Elemental mechanics scenario is not wired through every guarded surface.");
            Assertions.False(scenario.Contains("Game.Instance.Player.Party") ||
                scenario.Contains("SaveManager") ||
                scenario.Contains("KMG_AUTOMATION_BASELINE"),
                "The mechanics scenario must remain save-free and detached from protected state.");
        }

        internal static void RuntimeSlaScenarioUsesNativeDelivery()
        {
            string scenario = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "ElementalRaceSlaScenario.cs");
            foreach (string token in new[]
            {
                "new UnitUseAbility(data, target)",
                "GetMethod(\"OnAction\"",
                "AbilityExecutionProcess process",
                "process.InstantDeliver()", "effect.Apply(",
                "AbilityDeliverProjectile", "ContextActionApplyBuff",
                "FindNativeD20Seed(10)", "SavingThrowType.Reflex",
                "m_EndTime", "Buffs.UpdateNextEvent()",
                "RestController.ApplyRest", "DefaultContractResolver",
                "SameReferences(unitsBefore"
            })
                Assertions.True(scenario.Contains(token),
                    "Native SLA delivery token is absent: " + token);
            const string name = "disposable-elemental-race-slas";
            string catalog = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestScenarioCatalog.cs");
            string runner = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs");
            string automation = Read("scripts",
                "RuntimeAutomation.Common.ps1");
            string preflight = Read("scripts",
                "Test-RuntimeScenarioPreflight.ps1");
            string project = Read("src", "KingmakerGunslinger",
                "KingmakerGunslinger.csproj");
            Assertions.True(catalog.Contains(name) &&
                runner.Contains("ElementalRaceSlaScenario.Run(") &&
                automation.Contains("'" + name +
                    "' = [pscustomobject]") &&
                preflight.Contains("'" + name + "'") &&
                project.Contains("ElementalRaceSlaScenario.cs"),
                "Elemental SLA scenario is not wired through every guarded surface.");
            Assertions.False(scenario.Contains("SaveManager") ||
                scenario.Contains("Game.Instance.Player.Party") ||
                scenario.Contains("KMG_AUTOMATION_BASELINE"),
                "The SLA delivery scenario must remain save-free and detached from protected saves.");
        }

        internal static void RuntimeHydraulicPushScenarioUsesNativeManeuver()
        {
            string scenario = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "HydraulicPushScenario.cs");
            foreach (string token in new[]
            {
                "ContextActionCombatManeuver",
                "RuleCombatManeuver",
                "ReplaceAttackBonus",
                "ReplaceBaseStat",
                "InitiatorCMB",
                "TargetCMD",
                "UnitAttackOfOpportunity",
                "IInitiatorRulebookHandler<RuleAttackRoll>",
                "IInitiatorRulebookHandler<RuleSavingThrow>",
                "FindNativeD20Seed(10)",
                "RestController.ApplyRest",
                "BrownFurIlDisassembler.Describe",
                "UnitPartForceMove.Push",
                "SameReferences(unitsBefore",
                "ContractResolver = new DefaultContractResolver()"
            })
                Assertions.True(scenario.Contains(token),
                    "Native Hydraulic Push runtime token is absent: " + token);
            const string name = "disposable-hydraulic-push";
            string catalog = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestScenarioCatalog.cs");
            string runner = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs");
            string automation = Read("scripts",
                "RuntimeAutomation.Common.ps1");
            string preflight = Read("scripts",
                "Test-RuntimeScenarioPreflight.ps1");
            string project = Read("src", "KingmakerGunslinger",
                "KingmakerGunslinger.csproj");
            Assertions.True(catalog.Contains(name) &&
                runner.Contains("HydraulicPushScenario.Run(") &&
                automation.Contains("'" + name +
                    "' = [pscustomobject]") &&
                preflight.Contains("'" + name + "'") &&
                project.Contains("HydraulicPushScenario.cs"),
                "Hydraulic Push is not wired through every guarded surface.");
            Assertions.False(scenario.Contains("SaveManager") ||
                scenario.Contains("Game.Instance.Player.Party") ||
                scenario.Contains("KMG_AUTOMATION_BASELINE"),
                "Hydraulic Push qualification must remain save-free and detached from protected state.");
        }

        private static string Source(string file)
        {
            return Read("src", "KingmakerGunslinger", "ElementalRaces", file);
        }

        private static string Read(params string[] parts)
        {
            string path = Environment.CurrentDirectory;
            foreach (string part in parts) path = Path.Combine(path, part);
            return File.ReadAllText(path);
        }
    }
}
