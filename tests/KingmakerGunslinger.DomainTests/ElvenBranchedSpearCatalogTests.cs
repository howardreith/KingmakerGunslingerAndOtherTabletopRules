using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
                "typeof(UnitCombatState), \"Disengage\"",
                "EnterDisengage",
                "DisengageDepth <= 0",
                "EnterOpportunityAction",
                "typeof(UnitAttackOfOpportunity), \"OnAction\"",
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
            Assertions.True(component.Contains(
                "ElvenBranchedSpearProficiencyPenaltyComponent") &&
                component.Contains("Descriptor.Proficiencies.Contains(") &&
                component.Contains(
                    "evt.SetAttackBonusPenalty(evt.AttackBonusPenalty + 4)"),
                "The custom category lacks exact native-style nonproficiency enforcement.");

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
                "BriarGeneratedOpportunityAttackTracker.EnterGeneration",
                "ActiveGeneratedAttack",
                "GenerationDepth",
                "BriarOpportunityActionBoundaryPatch",
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

        internal static void CampaignPublicationContractsAreExact()
        {
            string root = Environment.CurrentDirectory;
            string source = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Blueprints",
                "ElvenBranchedSpearCampaignBlueprints.cs"));
            foreach (string token in new[] {
                "f720440559fc00949900bfa1575196ac",
                "CapitalVendorBlueprints.TableGuid",
                "f072a8f6889b5f345b7f4e7c74cb3e4c",
                "e5ab1fccf37c55f41a20a80c6ba6a460",
                "59cb0ac65b4093440ad341b9a2f372cf",
                "70c4615a8d667dc4cb740c22ee7b5eed",
                "193b1222846a0114197e716cb35d3ce8",
                "7e6448d1d8a7e4f4d9cc340b8f15e732",
                "NamedSpearKind.Thornstep",
                "NamedSpearKind.BriarCrownedSpear",
                "owned.Contains", "CreateFixedEntry(item, 1)",
                "ReferenceEquals", "Rollback()" })
                Assertions.True(source.Contains(token),
                    "Campaign publication lacks: " + token);
            Assertions.Equal(4, source.Split(new[] { "new VendorSpec(" },
                StringSplitOptions.None).Length - 1,
                "Vendor placement count changed.");
            Assertions.Equal(4, source.Split(new[] { "new LootSpec(" },
                StringSplitOptions.None).Length - 1,
                "Fixed-loot placement count changed.");

            string bootstrap = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Bootstrap", "BlueprintBootstrap.cs"));
            Assertions.True(bootstrap.Contains(
                "publicationPlan.ElvenBranchedSpearCommerce") &&
                bootstrap.Contains("spearCampaignPublication.Rollback()"),
                "Campaign publication is not module-gated and rollback-owned.");

            string manifest = File.ReadAllText(Path.Combine(root, "docs",
                "ELVEN-BRANCHED-SPEAR-PLACEMENT-MANIFEST.md"));
            foreach (string token in new[] { "Act I", "Act II", "Act III",
                "Act IV", "Act V", "Final", "append", "module OFF",
                "replenishment" })
                Assertions.True(manifest.IndexOf(token,
                    StringComparison.OrdinalIgnoreCase) >= 0,
                    "Placement manifest lacks: " + token);
        }

        internal static void OriginalAssetPipelineContractsAreExact()
        {
            string root = Environment.CurrentDirectory;
            string sourceRoot = Path.Combine(root, "assets-source",
                "original-models", "elven-branched-spear");
            string script = File.ReadAllText(Path.Combine(sourceRoot,
                "generate_elven_branched_spear.py"));
            string report = File.ReadAllText(Path.Combine(sourceRoot,
                "elven-branched-spear-build-report.json"));
            Assertions.True(script.Contains("branches = [") &&
                script.Contains("bpy.ops.export_scene.fbx") &&
                script.Contains("bpy.ops.wm.save_as_mainfile") &&
                script.Contains("scene.render.film_transparent = True") &&
                report.Contains("\"triangles\": 900") &&
                report.Contains("Original project-owned asset"),
                "Original Blender source is not deterministic and documented.");
            Assertions.Equal("8A79B5FE83285BA8D95B4111008A9C2E330DC61BFE4BA7CC2212D0C7CB25474B",
                Sha256(Path.Combine(sourceRoot, "elven-branched-spear.fbx")),
                "Generated spear FBX hash changed.");
            Assertions.Equal("3AB56092F363AA96C627287095E2CA549EEA7ED50D39C73BCD943646BFBE0EBE",
                Sha256(Path.Combine(root, "assets", "bundles",
                    "kingmakergunslinger.elvenbranchedspear")),
                "Dedicated spear bundle hash changed.");
            Assertions.Equal("2F3CF65793CCE8A1F79F6E907887FDC42698188150844B1A7D7B75C79C433186",
                Sha256(Path.Combine(root, "assets", "game", "icons",
                    "elven-branched-spear.png")),
                "Runtime spear icon hash changed.");

            string builder = File.ReadAllText(Path.Combine(root, "tools",
                "unity", "BuildElvenBranchedSpearBundle.cs"));
            string runtime = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Assets",
                "ElvenBranchedSpearAssetRuntime.cs"));
            foreach (string token in new[] { "2018.4.10f1",
                "kingmakergunslinger.elvenbranchedspear", "Grip",
                "SupportHandTarget", "Tip", "Butt", "Standard" })
                Assertions.True(builder.Contains(token) || runtime.Contains(token),
                    "Dedicated asset pipeline lacks: " + token);
            foreach (string token in new[] { "AssetBundle.LoadFromFile",
                "candidate.Unload(false)", "native-fallback:bundle-missing",
                "native-fallback:bundle-rejected", "ApplyTo",
                "ReferenceEquals(weaponType.VisualParameters.Model, prefab)",
                "bundle.reused", "native-fallback:model-assignment-rejected",
                "RejectAssignment" })
                Assertions.True(runtime.Contains(token),
                    "Fail-safe runtime lacks: " + token);
            Assertions.False(runtime.Contains("FirearmKind") ||
                runtime.Contains("FirearmAssetRuntime"),
                "Spear presentation was coupled to firearm identity/runtime.");
        }

        internal static void RuntimeCombatScenarioContractsAreExact()
        {
            string root = Environment.CurrentDirectory;
            string scenario = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "ElvenBranchedSpearCombatScenario.cs"));
            foreach (string token in new[] { "SceneEntitiesState",
                "RuleCalculateAttackBonusWithoutTarget",
                "RuleCalculateWeaponStats", "NativeElvenWeaponFamiliarityGuid",
                "WeaponFinesseGuid", "AttackOfOpportunity(target, false)",
                "CombatState.Engage(target)", "CombatState.Disengage(target)",
                "MovementOpportunityAccuracyDiagnostics.Applied == 2",
                "spear-named-boughkeeper", "spear-named-thornstep",
                "spear-named-vipers-reach", "spear-named-briar-crowned",
                "spear-named-first-branch", "AppliedSneakDamage",
                "NativeHitAttack", "FirstBranchDifficultyClass",
                "BriarPenaltyApplications", "GeneratedEvaluations",
                "InstantiatePrefab", "SameReferences" })
                Assertions.True(scenario.Contains(token),
                    "Spear combat scenario lacks: " + token);
            Assertions.False(scenario.Contains("SaveManager") ||
                scenario.Contains("SaveGame") || scenario.Contains("Input."),
                "Disposable spear combat scenario must remain save-free and input-free.");
            string catalog = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestScenarioCatalog.cs"));
            string automation = File.ReadAllText(Path.Combine(root, "scripts",
                "RuntimeAutomation.Common.ps1"));
            const string name = "disposable-elven-branched-spear-combat";
            Assertions.True(catalog.Contains(name) && automation.Contains(name),
                "Spear combat scenario is not allowlisted by both harness layers.");
        }

        internal static void DevelopmentGrantContractsAreExact()
        {
            string root = Environment.CurrentDirectory;
            string bridge = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Development",
                "KingmakerDevelopmentBridge.ElvenBranchedSpears.cs"));
            foreach (string token in new[] {
                "BlueprintBootstrap.ElvenBranchedSpears",
                "set.Entries.Select(value => value.Item).Concat(",
                "set.Named.Entries.Select(value => value.Item)",
                "items.Length != 12", "items.Distinct().Count() != 12",
                "AddExact(inventory, item)",
                "CountMatchingInventoryItems(inventory, item)",
                "No proficiency, feat, class level, vendor, loot, or campaign state changed" })
                Assertions.True(bridge.Contains(token),
                    "Development spear grant lacks: " + token);
            Assertions.False(bridge.Contains("Remove") ||
                bridge.Contains("SaveManager") || bridge.Contains("SaveGame"),
                "Development spear grants may not delete items or invoke save APIs.");

            string controls = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Development",
                "DevelopmentControls.cs"));
            string ui = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Development", "DevelopmentUi.cs"));
            string project = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "KingmakerGunslinger.csproj"));
            foreach (string token in new[] {
                "DescribeElvenBranchedSpearCatalog",
                "AddElvenBranchedSpearSet", "AddElvenBranchedSpear" })
                Assertions.True(controls.Contains(token) && ui.Contains(token),
                    "Development control/UI wiring lacks: " + token);
            Assertions.True(controls.Contains(
                    "AddElvenBranchedSpear(int index)") && ui.Contains(
                    "Elven Branched Spear Acceptance (DEVELOPMENT ONLY)") &&
                ui.Contains("KMG_AUTOMATION_WORKING") &&
                project.Contains(
                    "KingmakerDevelopmentBridge.ElvenBranchedSpears.cs"),
                "Development-only safety label or project inclusion is absent.");
        }

        internal static void WorkingSavePersistenceContractsAreExact()
        {
            string root = Environment.CurrentDirectory;
            string catalog = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestScenarioCatalog.cs"));
            string request = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRequest.cs"));
            string runner = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.cs"));
            string automation = File.ReadAllText(Path.Combine(root, "scripts",
                "RuntimeAutomation.Common.ps1"));
            string invocation = File.ReadAllText(Path.Combine(root, "scripts",
                "Invoke-KingmakerRuntimeTest.ps1"));
            foreach (string name in new[] {
                "working-save-elven-branched-spear-prepare",
                "working-save-elven-branched-spear-verify-cleanup",
                "working-save-elven-branched-spear-verify-absent" })
                Assertions.True(catalog.Contains(name) &&
                    automation.Contains(name) && invocation.Contains(name),
                    "Working-save spear phase is not allowlisted end to end: " +
                    name);
            foreach (string token in new[] {
                "IsElvenBranchedSpearPersistenceScenario()",
                "DevelopmentControls.AddElvenBranchedSpearSet()",
                "before.Any(value => value != 0)",
                "items.Length != 12", "instances.Length == 12",
                "instance.Blueprint.Type, set.WeaponType",
                "Game.Instance.Player.Inventory.Remove(item, 1)",
                "_workingSaveSmoke.ArmExactWorkingSaveWrite()",
                "ExpectedWorkingSaveRoutineCount == 1",
                "ExpectedWorkingSaveRoutineCount == 0" })
                Assertions.True(runner.Contains(token),
                    "Working-save spear verifier lacks: " + token);
            Assertions.True(request.Contains(
                    "WorkingSaveElvenBranchedSpearPrepare") &&
                request.Contains("save-name-required") &&
                request.Contains("baseline-save-forbidden"),
                "Spear persistence does not share the guarded working-save request gate.");
            Assertions.False(runner.Contains(
                "WorkingSaveElvenBranchedSpearPrepare)\n            {\n                foreach"),
                "Spear prepare must inspect the clean inventory before mutation.");
        }

        private static string Sha256(string path)
        {
            using (FileStream stream = File.OpenRead(path))
            using (SHA256 value = SHA256.Create())
                return BitConverter.ToString(value.ComputeHash(stream))
                    .Replace("-", string.Empty);
        }
    }
}
