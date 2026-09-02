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
                { "KMG.ElementalRaces.Undine.HydraulicPushAbility", "df0f9d05341a4eb59af8c369e447843f" },
                { "KMG.ElementalRaces.Ifrit.Visual.Body", "cf78c5410e484e6c8c1ce8faf19656e3" },
                { "KMG.ElementalRaces.Ifrit.Visual.Preset.Standard", "8078a649cb8a4621b3e29bbbf48d324d" },
                { "KMG.ElementalRaces.Ifrit.Visual.Preset.Heavy", "9e6b73f4ee0345559bdd5a5077892a40" },
                { "KMG.ElementalRaces.Ifrit.Visual.Preset.Slender", "a0657cb122404c5894e2a7b34c71dad5" },
                { "KMG.ElementalRaces.Oread.Visual.Body", "53ae478cf4374a86a7ba9fc5087aea0e" },
                { "KMG.ElementalRaces.Oread.Visual.Preset.Standard", "327137756be04458a3108a0e907b69f8" },
                { "KMG.ElementalRaces.Oread.Visual.Preset.Heavy", "557616abd28d496c84ad57f4e8939afe" },
                { "KMG.ElementalRaces.Oread.Visual.Preset.Slender", "438c3be835424b07afd2355167b563a4" },
                { "KMG.ElementalRaces.Sylph.Visual.Body", "95e3efb6e8d24873b00ef66440357929" },
                { "KMG.ElementalRaces.Sylph.Visual.Preset.Standard", "c72bb83d23f4496ea610af7850aa8680" },
                { "KMG.ElementalRaces.Sylph.Visual.Preset.Heavy", "b5f893c4aab34f0bb5c942b6371cf3d7" },
                { "KMG.ElementalRaces.Sylph.Visual.Preset.Slender", "82316d0b7e2942f498fab229fed1077b" },
                { "KMG.ElementalRaces.Undine.Visual.Body", "9a499ab69c7243039e9f487c5a34f6d0" },
                { "KMG.ElementalRaces.Undine.Visual.Preset.Standard", "daab362867ab49449fb8b99fa497f41d" },
                { "KMG.ElementalRaces.Undine.Visual.Preset.Heavy", "c170dfe7425248e6adced7c0380c3a71" },
                { "KMG.ElementalRaces.Undine.Visual.Preset.Slender", "5d70583ff8194a368bcac7959947d946" },
                { "KMG.ElementalRaces.Ifrit.Visual.Body.Male", "2b436bad4d4f480db61e5c16bc4f7e50" },
                { "KMG.ElementalRaces.Ifrit.Visual.Body.Female", "c43cc8f3302746a8a7b2efc3fe263af0" },
                { "KMG.ElementalRaces.Ifrit.Visual.Head.Male.01", "873ac0bdc4a04475a75d9015702a52ae" },
                { "KMG.ElementalRaces.Ifrit.Visual.Head.Male.02", "f9f2552915d94088a4138ffe7b546bab" },
                { "KMG.ElementalRaces.Ifrit.Visual.Head.Female.01", "956c61b782be4250b1f3d53e829b990b" },
                { "KMG.ElementalRaces.Ifrit.Visual.Head.Female.02", "25f062ea439f446a9b470e78577b55bf" },
                { "KMG.ElementalRaces.Ifrit.Visual.Horn.Male.01", "6abe2a5c06ea42a7b25ec8bcada7c3c4" },
                { "KMG.ElementalRaces.Ifrit.Visual.Horn.Male.02", "7db56f21efda4d8a83dcdce193342750" },
                { "KMG.ElementalRaces.Ifrit.Visual.Horn.Female.01", "8c97cbf1ebeb409283aaf6176343a5bb" },
                { "KMG.ElementalRaces.Ifrit.Visual.Horn.Female.02", "16c89d1048e2455989c1ba577230da46" },
                { "KMG.ElementalRaces.Oread.Visual.Body.Male", "1d661d42bdc24e8cb79a16f27e8e2a9e" },
                { "KMG.ElementalRaces.Oread.Visual.Body.Female", "e415f44e6d5f415ea79f36b57ff0cc0a" },
                { "KMG.ElementalRaces.Oread.Visual.Head.Male.01", "d44896914e9e459385313890fcad7b56" },
                { "KMG.ElementalRaces.Oread.Visual.Head.Male.02", "b156af8682ed4bed8c5004eb1e75c477" },
                { "KMG.ElementalRaces.Oread.Visual.Head.Female.01", "3eee36079ef14022b299bf82fab425d4" },
                { "KMG.ElementalRaces.Oread.Visual.Head.Female.02", "3208a94a9f4d4f9f885feb8c0ad1fd51" },
                { "KMG.ElementalRaces.Sylph.Visual.Body.Male", "4c8952d69afd412cabc1ff7b4016fee0" },
                { "KMG.ElementalRaces.Sylph.Visual.Body.Female", "3422858bb40b4071867ae5038ce412d1" },
                { "KMG.ElementalRaces.Sylph.Visual.Head.Male.01", "629aca3295324088aee8d2705e4045ce" },
                { "KMG.ElementalRaces.Sylph.Visual.Head.Male.02", "f0394ff493a340c2b7e7369081503572" },
                { "KMG.ElementalRaces.Sylph.Visual.Head.Female.01", "25a85ece629e4c1aa4089239448f1e78" },
                { "KMG.ElementalRaces.Sylph.Visual.Head.Female.02", "98bd78ec8008446faa61568ca1428bc1" },
                { "KMG.ElementalRaces.Undine.Visual.Body.Male", "5f46cadce318428dbf08a1d549eea3d2" },
                { "KMG.ElementalRaces.Undine.Visual.Body.Female", "0d40de45aa9b4d60ac58600293cf85c0" },
                { "KMG.ElementalRaces.Undine.Visual.Head.Male.01", "4a5c63dc835943c9a32b4d11c0fc0af1" },
                { "KMG.ElementalRaces.Undine.Visual.Head.Male.02", "7dacab6ed41a473795b5246f382ac7c2" },
                { "KMG.ElementalRaces.Undine.Visual.Head.Female.01", "2275367bdc5648f98df13f506c5cdf32" },
                { "KMG.ElementalRaces.Undine.Visual.Head.Female.02", "e2bd207dbf6b4f15b84ff7d4f6c7efc7" }
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
            Assertions.Equal(68, elemental.Length,
                "Production elemental identity count changed.");
            Assertions.Equal(1706, all.Length,
                "Manifest total must include 68 production elemental identities.");
            Assertions.Equal(1704, all.Count(value => string.Equals(
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
            string visualCatalog = SourceVisual(
                "ElementalRaceVisualCatalog.cs");
            string inventory = catalog + visualCatalog;
            Assertions.True(catalog.Contains("MechanicIdentityCount = 24") &&
                catalog.Contains("ManifestIdentityCount = IdentityCount +") &&
                ExpectedIds.Keys.All(inventory.Contains),
                "Identity catalog and manifest symbols drifted.");
            VisualCatalogAndResourceRegistryAreSaveSafe();
        }

        internal static void VisualCatalogAndResourceRegistryAreSaveSafe()
        {
            JObject manifest = JObject.Parse(Read("blueprints",
                "blueprints.json"));
            JToken[] elemental = manifest["entries"].Where(value =>
                ((string)value["symbol"]).StartsWith(
                    "KMG.ElementalRaces.", StringComparison.Ordinal) &&
                string.Equals((string)value["status"], "active",
                    StringComparison.Ordinal)).ToArray();
            Assertions.Equal(28, elemental.Count(value => string.Equals(
                (string)value["plannedType"], "EquipmentEntity",
                StringComparison.Ordinal)),
                "Visual resource proxy identity count changed.");
            Assertions.Equal(4, elemental.Count(value => string.Equals(
                (string)value["plannedType"], "KingmakerEquipmentEntity",
                StringComparison.Ordinal)),
                "Race body-wrapper identity count changed.");
            Assertions.Equal(12, elemental.Count(value => string.Equals(
                (string)value["plannedType"], "BlueprintRaceVisualPreset",
                StringComparison.Ordinal)),
                "Race visual-preset identity count changed.");

            string catalog = SourceVisual("ElementalRaceVisualCatalog.cs");
            string definition = SourceVisual(
                "ElementalRaceVisualDefinition.cs");
            string factory = SourceVisual("ElementalRaceVisualFactory.cs");
            string resources = SourceVisual(
                "ElementalRaceVisualResourceRegistry.cs");
            foreach (string token in new[]
            {
                "BlueprintIdentityCount", "ResourceIdentityCount = 28",
                "SkinRampCount = 7", "BuildIfrit(), BuildOread(), BuildSylph(), BuildUndine()",
                "At least two visual head proxies are required",
                "At least four native hair choices are required",
                "Exactly seven unique native skin ramps are required",
                "640e57f7890fa044ea78914930ddac5b",
                "d529cb3def52a584f93a4aff5e20316a",
                "00fa5240ec151e8419cb60c34fb96e0e",
                "preset.RaceId = donor.RaceId",
                "Preserve that exact split",
                "proxy.ColorsProfile = null",
                "PrimaryRampsField.SetValue", "SecondaryRampsField.SetValue",
                "NormalizeFallbackPalette", "CreateBodyWrapper",
                "CreateOptions", "TailSkinColors = Array.Empty",
                "texture.width == 256", "TextureFormat.RGB24",
                "FilterMode.Bilinear", "TextureWrapMode.Clamp",
                "s_LoadedResources", "LoadedResource contract changed",
                "EnsureAvailable", "Visual resource GUID collision",
                "RollbackAll", "rollback refused a foreign replacement"
            })
                Assertions.True((catalog + definition + factory + resources)
                    .Contains(token),
                    "Elemental visual safety token is absent: " + token);
            Assertions.False(factory.Contains("new Texture2D") ||
                factory.Contains("HarmonyPatch") ||
                factory.Contains("CharacterRaces"),
                "Visual construction must reference native ramps without custom textures or global patches.");
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
                "same creature-type interactions as its Aasimar and Tieflings",
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
            string runner = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs");
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
                    "ElementalRaceBlueprintFactory.Register(library,") &&
                bootstrap.Contains("manifest, registry, context.Logger") &&
                bootstrap.Contains("publicationPlan.ElementalRaceSelectors") &&
                bootstrap.Contains("ElementalRaceIdentityCatalog.IdentityCount") &&
                bootstrap.Contains("elementalRacePublication.Rollback()") &&
                bootstrap.Contains("elementalRaces.RollbackVisualResources()"),
                "Bootstrap does not unconditionally register and transactionally publish elemental identities.");
            Assertions.False(factory.Contains("CharacterRaces"),
                "Race identity construction must remain separate from selector publication.");
            foreach (string token in new[]
            {
                "ElementalRaceBlueprintSet elementalSet =",
                "BlueprintRoot.Instance.Progression.CharacterRaces",
                "elementalReferences.All(value => value == 1)",
                "elementalIndexes.All(value => value < 0)",
                "feature-module-elemental-races-publication"
            })
                Assertions.True(runner.Contains(token),
                    "Live elemental selector-publication proof is absent: " + token);
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

        internal static void RuntimeNativeIdentityScenarioUsesLiveEngineRules()
        {
            string scenario = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "ElementalRaceNativeIdentityScenario.cs");
            foreach (string token in new[]
            {
                "new ItemEntityArmor", "Body.Armor.InsertItem",
                "EncumbranceHelper", ".GetCarryingCapacity(owner)",
                "owner.Ensure<UnitPartEncumbrance>()", ".Init(encumbrance)",
                "UnitPartEncumbrance.GetSpeedPenalty",
                "new AbilityData(hold",
                "CanTarget(wrapper)",
                "PrerequisiteFeature", "PrerequisiteNoFeature",
                "SameReferences(unitsBefore",
                "ContractResolver = new DefaultContractResolver()"
            })
                Assertions.True(scenario.Contains(token),
                    "Native identity runtime token is absent: " + token);
            const string name =
                "disposable-elemental-race-native-identity";
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
                runner.Contains("ElementalRaceNativeIdentityScenario.Run(") &&
                automation.Contains("'" + name +
                    "' = [pscustomobject]") &&
                preflight.Contains("'" + name + "'") &&
                project.Contains("ElementalRaceNativeIdentityScenario.cs"),
                "Native identity scenario is not wired through every guarded surface.");
            Assertions.False(scenario.Contains("SaveManager") ||
                scenario.Contains("Game.Instance.Player.Party") ||
                scenario.Contains("KMG_AUTOMATION_BASELINE"),
                "Native identity qualification must remain save-free and detached from protected state.");
        }

        internal static void RacesUnleashedCompatibilityIsExactAndSaveFree()
        {
            string scenario = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "ElementalRaceCompatibilityScenario.cs");
            foreach (string token in new[]
            {
                "RacesUnleashedVersion = \"1.0.11\"",
                "e9b9acb5-9b3f-41ad-bbd7-74494d5d7680",
                "6d18168cb90ffe60931addc8ee11e42b3ef647ef0e6d4b7ce8980d44659f4cb0",
                "d1335380a70e4bd7aa535f36770b93de",
                "3cfdcda8edd74212a58d3b0d9d4041a4",
                "ElementalRacePublication.Apply(set, true)",
                "ReferenceEquals(catalogReference, afterFirst)",
                "SameReferences(before, afterSecond)",
                "ContractResolver = new DefaultContractResolver()",
                "third-party-race-order-preserved"
            })
                Assertions.True(scenario.Contains(token),
                    "Races Unleashed compatibility token is absent: " +
                        token);
            const string name =
                "elemental-races-races-unleashed-compatibility";
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
            string wrapper = Read("scripts", "compatibility",
                "Invoke-KingmakerCompatibilityProfile.ps1");
            Assertions.True(catalog.Contains(name) &&
                runner.Contains("ElementalRaceCompatibilityScenario.Run(") &&
                automation.Contains("'" + name +
                    "' = [pscustomobject]") &&
                preflight.Contains("'" + name + "'") &&
                project.Contains("ElementalRaceCompatibilityScenario.cs") &&
                wrapper.Contains("'" + name + "'"),
                "Compatibility observer is not wired through every guarded surface.");
            JObject profiles = JObject.Parse(Read("compatibility",
                "profiles.json"));
            JToken[] exact = profiles["profiles"].Where(value =>
                (string)value["id"] == "gunslinger-races-unleashed" ||
                (string)value["id"] ==
                    "gunslinger-call-of-the-wild-races-unleashed").ToArray();
            Assertions.Equal(2, exact.Length,
                "Both exact Races Unleashed profiles are required.");
            Assertions.True(exact.All(value =>
                    value["scenarios"].Any(item =>
                        (string)item == name)) &&
                exact.Count(value => value["modKeys"].Any(item =>
                    (string)item == "races-unleashed")) == 2,
                "Exact Races Unleashed profiles must include the focused observer.");
            Assertions.False(scenario.Contains("SaveManager") ||
                scenario.Contains("Game.Instance.Player.Party") ||
                scenario.Contains("KMG_AUTOMATION_BASELINE"),
                "Compatibility qualification must remain save-free.");
        }

        private static string Source(string file)
        {
            return Read("src", "KingmakerGunslinger", "ElementalRaces", file);
        }

        private static string SourceVisual(string file)
        {
            return Read("src", "KingmakerGunslinger", "ElementalRaces",
                "Visuals", file);
        }

        private static string Read(params string[] parts)
        {
            string path = Environment.CurrentDirectory;
            foreach (string part in parts) path = Path.Combine(path, part);
            return File.ReadAllText(path);
        }
    }
}
