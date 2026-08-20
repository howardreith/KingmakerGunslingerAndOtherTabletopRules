using System;
using System.Collections.Generic;
using System.IO;
using KingmakerGunslinger.Ammunition;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Reloading;
using KingmakerGunslinger.Deeds;
using KingmakerGunslinger.Misfires;

namespace KingmakerGunslinger.DomainTests
{
    internal static class PaperCartridgeFoundationTests
    {
        internal static void ProfilesAreExact()
        {
            ReloadAmmunitionProfile loose = ReloadAmmunitionProfileCatalog.LooseBasic;
            ReloadAmmunitionProfile paper = ReloadAmmunitionProfileCatalog.PaperCartridge;
            Assertions.Equal("kmg.debug.lead-ball", loose.LoadedAmmunition.Value,
                "old loaded lead identity");
            Assertions.Equal(0, loose.ReloadStepReduction, "loose reduction");
            Assertions.Equal(0, loose.MisfireModifier, "loose modifier");
            Assertions.Equal(ReloadAmmunitionSourceKind.LooseBasic, loose.SourceKind,
                "loose source");
            Assertions.Equal("kmg.ammunition.paper-cartridge",
                paper.LoadedAmmunition.Value, "paper ID");
            Assertions.Equal(1, paper.ReloadStepReduction, "paper reduction");
            Assertions.Equal(1, paper.MisfireModifier, "paper modifier");
            Assertions.Equal(1, paper.RoundsPerLoad, "paper rounds");
            Assertions.Equal(ReloadAmmunitionSourceKind.PaperCartridge,
                paper.SourceKind, "paper source");
        }

        internal static void CompatibilityIsDefinitionDriven()
        {
            ReloadAmmunitionProfile paper = ReloadAmmunitionProfileCatalog.PaperCartridge;
            Assertions.True(paper.IsCompatible(FirearmDefinitions.CreateEarlyPistol()),
                "early pistol");
            Assertions.True(paper.IsCompatible(FirearmDefinitions.CreateEarlyMusket()),
                "early musket");
            Assertions.True(paper.IsCompatible(FirearmDefinitions.CreateEarlyBlunderbuss()),
                "early blunderbuss");
            Assertions.False(paper.IsCompatible(FirearmDefinitions.CreateAdvancedRifle()),
                "advanced rifle");
            Assertions.False(paper.IsCompatible(FirearmDefinitions.CreateAdvancedRevolver()),
                "advanced revolver");
            Assertions.True(ReloadAmmunitionProfileCatalog.LooseBasic.IsCompatible(
                FirearmDefinitions.CreateAdvancedRevolver()), "loose advanced control");
            FirearmStateRules early = FirearmStateRules.CreateForDefinition(
                FirearmDefinitions.CreateEarlyPistol());
            FirearmStateRules advanced = FirearmStateRules.CreateForDefinition(
                FirearmDefinitions.CreateAdvancedRifle());
            Assertions.True(early.IsCompatible(paper.LoadedAmmunition),
                "early state rules include paper");
            Assertions.False(advanced.IsCompatible(paper.LoadedAmmunition),
                "advanced state rules reject paper");
        }

        internal static void UnknownIdentityFailsClosed()
        {
            ReloadAmmunitionProfile profile;
            Assertions.False(ReloadAmmunitionProfileCatalog.TryResolve(
                new AmmunitionId("kmg.ammunition.unknown"), out profile), "unknown ID");
            Assertions.Equal(null, profile, "unknown profile");
            Assertions.Throws<KeyNotFoundException>(() =>
                ReloadAmmunitionProfileCatalog.Require(
                    new AmmunitionId("kmg.ammunition.unknown")),
                "unknown ammunition must fail closed");
        }

        internal static void MisfireAuthoritativeOrder()
        {
            AmmunitionId loose = ReloadAmmunitionProfileCatalog.LooseBasic.LoadedAmmunition;
            AmmunitionId paper = ReloadAmmunitionProfileCatalog.PaperCartridge.LoadedAmmunition;
            Assertions.Equal(1, EffectiveFirearmMisfireValuePolicy.Evaluate(
                1, FirearmCondition.Normal, false, loose, 0), "loose control");
            Assertions.Equal(2, EffectiveFirearmMisfireValuePolicy.Evaluate(
                1, FirearmCondition.Normal, false, paper, 0), "paper plus one");
            Assertions.Equal(0, EffectiveFirearmMisfireValuePolicy.Evaluate(
                1, FirearmCondition.Normal, false, loose, 1), "Reliable loose zero");
            Assertions.Equal(1, EffectiveFirearmMisfireValuePolicy.Evaluate(
                1, FirearmCondition.Normal, false, paper, 1), "paper before Reliable");
            Assertions.Equal(6, EffectiveFirearmMisfireValuePolicy.Evaluate(
                2, FirearmCondition.Broken, false, paper, 1),
                "broken untrained then paper then Reliable");
            Assertions.Equal(4, EffectiveFirearmMisfireValuePolicy.Evaluate(
                2, FirearmCondition.Broken, true, paper, 1),
                "broken trained then paper then Reliable");
            Assertions.Equal(20, EffectiveFirearmMisfireValuePolicy.Evaluate(
                20, FirearmCondition.Normal, false, paper, 0), "maximum clamp");
            Assertions.Equal(0, EffectiveFirearmMisfireValuePolicy.MinimumEffectiveValue,
                "truthful zero threshold");
            Assertions.Throws<KeyNotFoundException>(() =>
                EffectiveFirearmMisfireValuePolicy.Evaluate(1,
                    FirearmCondition.Normal, false,
                    new AmmunitionId("kmg.ammunition.unknown"), 0),
                "unknown ammunition fails closed");
        }

        internal static void MisfireCentralConsumers()
        {
            string root = Environment.CurrentDirectory;
            string ordinary = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Firing", "FirearmDischargeRuntime.cs"));
            string deadShot = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Deeds", "DeadShotRuntime.cs"));
            string scatter = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Scatter", "ScatterShotRuntime.cs"));
            string ordinaryMisfire = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Misfires", "FirearmMisfireRuntime.cs"));
            Assertions.True(ordinary.Contains("before.Repository.State.LoadedAmmunition"),
                "ordinary captures fired ammunition before discharge");
            Assertions.True(deadShot.Contains("firearm.Weapon, before.LoadedAmmunition"),
                "Dead Shot uses pre-discharge ammunition");
            Assertions.True(scatter.Contains("firearm.Weapon, before.LoadedAmmunition"),
                "Scatter uses pre-discharge ammunition");
            Assertions.True(deadShot.Contains("MinimumEffectiveValue") &&
                deadShot.Contains("MaximumEffectiveValue"),
                "Dead Shot accepts centralized zero-to-twenty thresholds");
            Assertions.True(ordinaryMisfire.Contains(
                    "bool forced = ForcedRolls.TryConsume(out forcedNaturalRoll)") &&
                ordinaryMisfire.Contains(
                    "if (!context.Forced && naturalRoll != context.FinalNaturalRoll)") &&
                ordinaryMisfire.Contains(
                    "Service.Evaluate(\n                    context.FinalNaturalRoll,"),
                "ordinary direct-field fallback preserves the exact forced-roll hook");
        }

        internal static void CraftingSharedTransactionContract()
        {
            string root = Environment.CurrentDirectory;
            string paper = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Gunsmithing",
                "CraftPaperCartridgesAbilityLogic.cs"));
            string transaction = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Gunsmithing",
                "FirearmCraftingTransactionService.cs"));
            string blueprints = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Blueprints",
                "GunsmithingCraftingBlueprints.cs"));
            string grants = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Blueprints", "GunsmithingBlueprints.cs"));
            string sale = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Gunsmithing",
                "BasicAmmunitionSaleValuePatch.cs"));
            foreach (string token in new[] { "BatchSize = 20", "GoldCost = 120",
                "FirearmCraftingTransactionService.Complete", "m_UsedMarker" })
                Assertions.True(paper.Contains(token), "paper craft: " + token);
            foreach (string token in new[] { "SpendMoney(goldCost)",
                "caster.RemoveFact(marker)", "GainMoney(missingMoney)",
                "countsBefore[index]" })
                Assertions.True(transaction.Contains(token),
                    "shared rollback: " + token);
            Assertions.True(blueprints.Contains("PaperAbilitySymbol") &&
                blueprints.Contains("CraftPaperCartridgesAbilityLogic.Create"),
                "paper recipe blueprint");
            Assertions.True(grants.Contains("Facts.Length != 4") &&
                grants.Contains("paperCraftingAbility"), "shared Gunsmithing grants");
            Assertions.True(sale.Contains("ammo.PaperCartridge"),
                "paper zero resale");
        }

        internal static void VendorNormalizationContract()
        {
            string root = Environment.CurrentDirectory;
            string capital = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Blueprints", "CapitalVendorBlueprints.cs"));
            string btsl = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Blueprints",
                "BeneathStolenLandsVendorBlueprints.cs"));
            Assertions.True(capital.Contains("7de959347266092448d8a72089ef9778") &&
                capital.Contains("SmithVendorTable"), "exact capital authority");
            Assertions.True(capital.Contains("cordOfStubbornResolve") &&
                capital.Contains("publishGunslinger") && capital.Contains("publishCord") &&
                capital.Contains("new[] { 1 }"),
                "Capital publication must compose exact module gates and one Cord row.");
            Assertions.True(Count(capital, "ammunition.PaperCartridge") >= 2 &&
                capital.Contains("AmmunitionCount = 200"),
                "capital desired and owned Paper stock");
            Assertions.True(Count(btsl, "ammunition.PaperCartridge") >= 2 &&
                btsl.Contains("200, 200, 200"),
                "BTSL desired and owned Paper stock");
            Assertions.False(capital.Contains("afa2c7f292b8e1c4d9c835f0e8047dd3"),
                "rejected Jhod absent");
        }

        internal static void OlegMaintenanceStockContract()
        {
            string root = Environment.CurrentDirectory;
            string source = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Blueprints",
                "OlegMaintenanceVendorBlueprints.cs"));
            foreach (string token in new[] {
                "f720440559fc00949900bfa1575196ac",
                "C11_OlegVendorTable",
                "5db389e0409ef534d81358555e6ab99d",
                "OTP_Oleg",
                "67db4b8bacc69e643880f0a4ed6dff6f",
                "OTP_Oleg_FirstVisit",
                "RepairKitCount = 5",
                "OverhaulKitCount = 2",
                "BlueprintLibraryLookup.RequireExact<BlueprintSharedVendorTable>",
                "VendorCatalogPublication<BlueprintComponent>.Create",
                "CapitalVendorPublication.Unchanged",
                "publication.Validate()",
                "owned.Contains",
                "ReferenceEquals"
            }) Assertions.True(source.Contains(token),
                "Oleg maintenance publication contract missing: " + token);

            string bootstrap = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Bootstrap", "BlueprintBootstrap.cs"));
            int publish = bootstrap.IndexOf(
                "OlegMaintenanceVendorBlueprints.Publish", StringComparison.Ordinal);
            int rollback = bootstrap.IndexOf(
                "olegMaintenancePublication.Rollback()", StringComparison.Ordinal);
            int capitalRollback = bootstrap.IndexOf(
                "capitalVendorPublication.Rollback()", StringComparison.Ordinal);
            Assertions.True(publish >= 0 && rollback > publish &&
                capitalRollback > rollback && bootstrap.Contains(
                    "publicationPlan.CapitalGunslingerStock"),
                "Oleg stock must be module-gated and roll back before the older capital snapshot.");
        }

        internal static void BokkenAcquisitionForensicsContract()
        {
            string source = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs"));
            foreach (string token in new[] {
                "bokken-acquisition-forensics",
                "IsBokkenAcquisitionSearchSurface",
                "DescribeBokkenTextMatches",
                "CollectBokkenTextMatches",
                "DescribeDirectBlueprintReferences",
                "BuildDirectBlueprintReferenceIndex(allBlueprints,",
                "depth > 3",
                "Math.Min(array.Length, 64)",
                "bokkenCandidateRecords.Length <= 64",
                "read-only depth-three metadata scan"
            }) Assertions.True(source.Contains(token),
                "Bokken bounded-forensics contract missing: " + token);
        }

        internal static void BokkenAmmunitionStockContract()
        {
            string root = Environment.CurrentDirectory;
            string source = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Blueprints",
                "BokkenAmmunitionVendorBlueprints.cs"));
            foreach (string token in new[] {
                "4778ecb5df5d48742b9be5a204ed4657",
                "C11_BokkenVendorTable",
                "4f5acdb403f6ef642959f6bedc051ac7",
                "OTP_Bokken",
                "57f84fdde3cc2994284fb3acc4a3cb97",
                "OTP_Bokken_ZeroState",
                "AmmunitionCount = 100",
                "BlueprintLibraryLookup.RequireExact<BlueprintUnitLoot>",
                "ammunition.BlackPowder",
                "ammunition.LeadBall",
                "ammunition.PaperCartridge",
                "VendorCatalogPublication<BlueprintComponent>.Create",
                "BokkenVendorPublication.Unchanged",
                "publication.Validate()",
                "ReferenceEquals"
            }) Assertions.True(source.Contains(token),
                "Bokken ammunition publication contract missing: " + token);

            string bootstrap = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Bootstrap", "BlueprintBootstrap.cs"));
            int publish = bootstrap.IndexOf(
                "BokkenAmmunitionVendorBlueprints.Publish",
                StringComparison.Ordinal);
            int rollback = bootstrap.IndexOf(
                "bokkenAmmunitionPublication.Rollback()",
                StringComparison.Ordinal);
            int olegRollback = bootstrap.IndexOf(
                "olegMaintenancePublication.Rollback()", StringComparison.Ordinal);
            Assertions.True(publish >= 0 && rollback > publish &&
                olegRollback > rollback && bootstrap.Contains(
                    "publicationPlan.CapitalGunslingerStock"),
                "Bokken stock must be module-gated and roll back before the prior Oleg snapshot.");
        }

        private static int Count(string source, string token)
        {
            int count = 0, index = 0;
            while ((index = source.IndexOf(token, index,
                StringComparison.Ordinal)) >= 0)
            { count++; index += token.Length; }
            return count;
        }

        internal static void PaperTokensRoundTrip()
        {
            FirearmStateTokenCatalog catalog =
                FirearmStateTokenCatalog.CreateCapacityOneDiagnostic();
            var paper = ReloadAmmunitionProfileCatalog.PaperCartridge.LoadedAmmunition;
            var normal = new FirearmState(FirearmState.CurrentSchemaVersion, 1,
                paper, FirearmCondition.Normal);
            var broken = new FirearmState(FirearmState.CurrentSchemaVersion, 1,
                paper, FirearmCondition.Broken);
            Assertions.Equal(FirearmStateTokenCatalog.PaperLoadedNormalTokenId,
                catalog.Encode(normal), "normal paper token");
            Assertions.Equal(FirearmStateTokenCatalog.PaperBrokenLoadedTokenId,
                catalog.Encode(broken), "broken paper token");
            Assertions.Equal(normal, catalog.Decode(new[] {
                FirearmStateTokenCatalog.PaperLoadedNormalTokenId }), "normal decode");
            Assertions.Equal(broken, catalog.Decode(new[] {
                FirearmStateTokenCatalog.PaperBrokenLoadedTokenId }), "broken decode");
            Assertions.Equal(6, catalog.Definitions.Count, "exact token total");
        }

        internal static void OldTokensRemainExact()
        {
            Assertions.Equal("kmg.state.v1.loaded-normal.lead-ball",
                FirearmStateTokenCatalog.LoadedNormalTokenId, "old normal token");
            Assertions.Equal("kmg.state.v1.broken-empty",
                FirearmStateTokenCatalog.BrokenEmptyTokenId, "old broken empty token");
            Assertions.Equal("kmg.state.v1.broken-loaded.lead-ball",
                FirearmStateTokenCatalog.BrokenLoadedTokenId, "old broken lead token");
            Assertions.Equal("kmg.state.v1.wrecked",
                FirearmStateTokenCatalog.WreckedTokenId, "old wrecked token");
        }

        internal static void BlueprintSourceContract()
        {
            string source = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Blueprints",
                "BasicAmmunitionBlueprints.cs"));
            foreach (string token in new[] { "KMG.Ammunition.PaperCartridge",
                "Paper Cartridge", "PaperCartridgeCost = 12",
                "PaperCartridgeWeight = 0f", "ComponentsArray = Array.Empty<BlueprintComponent>()",
                "reduces reload time by one step", "increases misfire by 1" })
                Assertions.True(source.Contains(token), "paper item contract: " + token);
            Assertions.False(source.Contains("Dragon"), "no unrelated cartridge");
        }

        internal static void ActionMatrix()
        {
            ReloadAmmunitionProfile loose = ReloadAmmunitionProfileCatalog.LooseBasic;
            ReloadAmmunitionProfile paper = ReloadAmmunitionProfileCatalog.PaperCartridge;
            FirearmDefinition pistol = FirearmDefinitions.CreateEarlyPistol();
            FirearmDefinition musket = FirearmDefinitions.CreateEarlyMusket();
            Assertions.Equal(EffectiveReloadAction.Standard, Action(pistol, loose, false, false), "pistol loose");
            Assertions.Equal(EffectiveReloadAction.Move, Action(pistol, loose, false, true), "pistol rapid loose");
            Assertions.Equal(EffectiveReloadAction.Move, Action(pistol, paper, false, false), "pistol paper");
            Assertions.Equal(EffectiveReloadAction.Free, Action(pistol, paper, false, true), "pistol rapid paper");
            Assertions.Equal(EffectiveReloadAction.FullRound, Action(musket, loose, false, false), "musket loose");
            Assertions.Equal(EffectiveReloadAction.Standard, Action(musket, loose, false, true), "musket rapid loose");
            Assertions.Equal(EffectiveReloadAction.Standard, Action(musket, paper, false, false), "musket paper");
            Assertions.Equal(EffectiveReloadAction.Move, Action(musket, paper, false, true), "musket rapid paper");
            Assertions.Equal(EffectiveReloadAction.Standard, Action(musket, loose, true, false), "fast musket loose");
            Assertions.Equal(EffectiveReloadAction.Move, Action(musket, loose, true, true), "fast rapid loose");
            Assertions.Equal(EffectiveReloadAction.Move, Action(musket, paper, true, false), "fast paper");
            Assertions.Equal(EffectiveReloadAction.Free, Action(musket, paper, true, true), "fast rapid paper");
        }

        internal static void NoFallback()
        {
            object unit = new object(); object item = new object();
            FirearmReloadPlan missing = FirearmReloadPlanner.Evaluate(unit, item,
                FirearmDefinitions.CreateEarlyPistol(), FirearmState.CreateEmpty(),
                ReloadAmmunitionProfileCatalog.PaperCartridge,
                new ReloadAmmunitionInventorySnapshot(20, 20, 0), false, true, 1);
            Assertions.Equal(FirearmReloadPlanStatus.MissingAmmunition, missing.Status,
                "paper mode must not fall back to loose stock");
            FirearmReloadPlan advanced = FirearmReloadPlanner.Evaluate(unit, item,
                FirearmDefinitions.CreateAdvancedRifle(), FirearmState.CreateEmpty(),
                ReloadAmmunitionProfileCatalog.PaperCartridge,
                new ReloadAmmunitionInventorySnapshot(20, 20, 20), false, true, 1);
            Assertions.Equal(FirearmReloadPlanStatus.IncompatibleAmmunition,
                advanced.Status, "advanced paper rejection");
        }

        internal static void AtomicSources()
        {
            var inventory = new ReloadInventory(5, 5, 5);
            var service = new ReloadAmmunitionTransactionService();
            ReloadAmmunitionInventorySnapshot loose = service.Consume(inventory,
                ReloadAmmunitionProfileCatalog.LooseBasic, 1);
            Assertions.Equal(4, loose.BlackPowderCharges, "loose powder");
            Assertions.Equal(4, loose.LeadBalls, "loose ball");
            Assertions.Equal(5, loose.PaperCartridges, "loose paper untouched");
            ReloadAmmunitionInventorySnapshot paper = service.Consume(inventory,
                ReloadAmmunitionProfileCatalog.PaperCartridge, 1);
            Assertions.Equal(4, paper.BlackPowderCharges, "paper powder untouched");
            Assertions.Equal(4, paper.LeadBalls, "paper ball untouched");
            Assertions.Equal(4, paper.PaperCartridges, "paper consumed");
        }

        internal static void PaperReloadTransactionSuccess()
        {
            var inventory = new ReloadInventory(8, 9, 3);
            var store = new ReloadStateStore(FirearmState.CreateEmpty());
            FirearmReloadResult result = new FirearmReloadTransactionService().TryReloadRounds(
                store, inventory,
                FirearmStateRules.CreateForDefinition(FirearmDefinitions.CreateEarlyPistol()),
                ReloadAmmunitionProfileCatalog.PaperCartridge, 1);
            Assertions.True(result.Succeeded, "paper transaction success");
            Assertions.Equal(1, result.AfterState.LoadedRounds, "loaded chamber");
            Assertions.Equal(ReloadAmmunitionProfileCatalog.PaperCartridge.LoadedAmmunition,
                result.AfterState.LoadedAmmunition, "loaded paper identity");
            Assertions.Equal(8, inventory.Count(ReloadInventoryComponent.BlackPowderCharge),
                "powder untouched");
            Assertions.Equal(9, inventory.Count(ReloadInventoryComponent.LeadBall),
                "ball untouched");
            Assertions.Equal(2, inventory.Count(ReloadInventoryComponent.PaperCartridge),
                "one cartridge consumed");
        }

        internal static void PaperStateFailureRestoresInventory()
        {
            var inventory = new ReloadInventory(4, 5, 2);
            var store = new ReloadStateStore(FirearmState.CreateEmpty()) { ThrowAfterWrite = true };
            FirearmReloadTransactionException failure = Assertions.Throws<FirearmReloadTransactionException>(
                () => new FirearmReloadTransactionService().TryReloadRounds(store, inventory,
                    FirearmStateRules.CreateForDefinition(FirearmDefinitions.CreateEarlyPistol()),
                    ReloadAmmunitionProfileCatalog.PaperCartridge, 1),
                "late state failure must surface transaction failure");
            Assertions.True(failure.RollbackSucceeded, "paper rollback must verify");
            Assertions.Equal(FirearmState.CreateEmpty(), store.Read(), "state restored");
            Assertions.Equal(2, inventory.Count(ReloadInventoryComponent.PaperCartridge),
                "paper restored");
            Assertions.Equal(4, inventory.Count(ReloadInventoryComponent.BlackPowderCharge),
                "powder exact");
            Assertions.Equal(5, inventory.Count(ReloadInventoryComponent.LeadBall),
                "ball exact");
        }

        internal static void MixedIdentityRejected()
        {
            var loadedLoose = new FirearmState(FirearmState.CurrentSchemaVersion, 1,
                ReloadAmmunitionProfileCatalog.LooseBasic.LoadedAmmunition,
                FirearmCondition.Normal);
            var store = new ReloadStateStore(loadedLoose);
            var inventory = new ReloadInventory(10, 10, 10);
            Assertions.Throws<FirearmStateTransitionException>(() =>
                new FirearmReloadTransactionService().TryReloadRounds(store, inventory,
                    new FirearmStateRules(2, new[] {
                        ReloadAmmunitionProfileCatalog.LooseBasic.LoadedAmmunition,
                        ReloadAmmunitionProfileCatalog.PaperCartridge.LoadedAmmunition }),
                    ReloadAmmunitionProfileCatalog.PaperCartridge, 1),
                "partially loaded firearms may not mix ammunition");
            Assertions.Equal(10, inventory.Count(ReloadInventoryComponent.PaperCartridge),
                "mixed rejection consumed nothing");
            Assertions.Equal(loadedLoose, store.Read(), "mixed rejection changed state");
        }

        internal static void ModeSourceContract()
        {
            string root = Environment.CurrentDirectory;
            string mode = File.ReadAllText(Path.Combine(root, "src", "KingmakerGunslinger",
                "Blueprints", "PaperCartridgeModeBlueprints.cs"));
            foreach (string token in new[] { "BlueprintActivatableAbility", "IsOnByDefault = false",
                "AbilityActivationType.Immediately", "There is no fallback",
                "DeactivateIfCombatEnded = false", "HiddenInUi",
                "marker.FxOnStart = new PrefabLink()",
                "marker.FxOnRemove = new PrefabLink()",
                "marker.ResourceAssetIds = Array.Empty<string>()",
                "ability.ResourceAssetIds = Array.Empty<string>()",
                "marker.FxOnStart == null", "marker.FxOnRemove == null" })
                Assertions.True(mode.Contains(token), "mode contract: " + token);
            string full = File.ReadAllText(Path.Combine(root, "src", "KingmakerGunslinger",
                "Blueprints", "FirearmProficiencyBlueprints.cs"));
            string scoped = File.ReadAllText(Path.Combine(root, "src", "KingmakerGunslinger",
                "Blueprints", "FirearmScopedProficiencyBlueprints.cs"));
            Assertions.True(full.Contains("grant.Facts.Length != 3"),
                "full proficiency grants reload, scatter, and mode exactly once");
            Assertions.True(scoped.Contains("Attach(set.OneHanded, reload, paperCartridgeMode)"),
                "one-handed scoped mode grant");
            Assertions.True(scoped.Contains("Attach(set.TwoHanded, reload, scatter, paperCartridgeMode)"),
                "two-handed scoped mode grant");
            string runtime = File.ReadAllText(Path.Combine(root, "src", "KingmakerGunslinger",
                "Reloading", "PaperCartridgeModeRuntime.cs"));
            Assertions.False(runtime.Contains("_isActive") || runtime.Contains("Dictionary<Unit"),
                "mode runtime must not own global mutable selection state");
            string localBuild = File.ReadAllText(Path.Combine(root, "scripts", "Build-Local.ps1"));
            string package = File.ReadAllText(Path.Combine(root, "scripts",
                "package.ps1"));
            string packager = File.ReadAllText(Path.Combine(root, "tools",
                "create_deterministic_package.py"));
            Assertions.True(localBuild.Contains("{ 137 } else { 135 }") &&
                packager.Contains("135, 137)"),
                "deterministic package counts include all project-owned runtime icons");
            Assertions.True(package.Contains("create_deterministic_package.py") &&
                package.Contains("expectedPackageFileCount") &&
                !package.Contains("Compress-Archive"),
                "standalone release package must use the deterministic ZIP builder");
        }

        internal static void LightningReloadDynamicActions()
        {
            var service = new LightningReloadService();
            LightningReloadDecision swift = service.Evaluate(new LightningReloadRequest(
                true, FirearmCondition.Normal, 0, 1, 1, true, false,
                LightningReloadAction.Swift));
            LightningReloadDecision free = service.Evaluate(new LightningReloadRequest(
                true, FirearmCondition.Normal, 0, 1, 1, true, false,
                LightningReloadAction.Free));
            Assertions.True(swift.IsAvailable, "loose Lightning available");
            Assertions.Equal(LightningReloadAction.Swift, swift.Action,
                "loose/no Rapid is Swift");
            Assertions.Equal(LightningReloadAction.Free, free.Action,
                "paper or matching Rapid is Free");
            LightningReloadDecision missing = service.Evaluate(new LightningReloadRequest(
                true, FirearmCondition.Normal, 0, 1, 1, false, false,
                LightningReloadAction.Free));
            Assertions.False(missing.IsAvailable, "selected source missing");
            Assertions.Equal(LightningReloadAction.Unknown, missing.Action,
                "unavailable action fails closed");
            LightningReloadDecision used = service.Evaluate(new LightningReloadRequest(
                true, FirearmCondition.Normal, 0, 1, 1, true, true,
                LightningReloadAction.Free));
            Assertions.False(used.IsAvailable, "one use per round");
        }

        internal static void FullAttackReloadBranches()
        {
            FirearmState empty = FirearmState.CreateEmpty();
            Assertions.Equal(FullAttackReloadDecision.EndFullAttack,
                FullAttackAutoReloadPolicy.Evaluate(true, true, true, true, true,
                    false, true, EffectiveReloadAction.Free, false, empty,
                    FirearmCondition.Normal), "auto-use off ends attack");
            Assertions.Equal(FullAttackReloadDecision.EndFullAttack,
                FullAttackAutoReloadPolicy.Evaluate(true, true, true, true, true,
                    true, false, EffectiveReloadAction.Free, true, empty,
                    FirearmCondition.Normal), "missing selected ammunition ends attack");
            Assertions.Equal(FullAttackReloadDecision.Reload,
                FullAttackAutoReloadPolicy.Evaluate(true, true, true, true, true,
                    true, true, EffectiveReloadAction.Free, true, empty,
                    FirearmCondition.Normal), "normal Free takes priority");
            Assertions.Equal(FullAttackReloadDecision.LightningReload,
                FullAttackAutoReloadPolicy.Evaluate(true, true, true, true, true,
                    true, true, EffectiveReloadAction.Move, true, empty,
                    FirearmCondition.Normal), "one free Lightning fallback");
            Assertions.Equal(FullAttackReloadDecision.EndFullAttack,
                FullAttackAutoReloadPolicy.Evaluate(true, true, true, true, true,
                    true, true, EffectiveReloadAction.Move, false, empty,
                    FirearmCondition.Normal), "non-free plan without fallback ends");
        }

        private static EffectiveReloadAction Action(FirearmDefinition definition,
            ReloadAmmunitionProfile profile, bool fast, bool rapid)
        {
            return FirearmReloadPlanner.Evaluate(new object(), new object(), definition,
                FirearmState.CreateEmpty(), profile,
                new ReloadAmmunitionInventorySnapshot(10, 10, 10), fast, rapid, 1).Action;
        }

        private sealed class ReloadInventory : IReloadAmmunitionInventory
        {
            private readonly int[] _counts = new int[4];
            internal ReloadInventory(int powder, int balls, int paper)
            { _counts[1] = powder; _counts[2] = balls; _counts[3] = paper; }
            public int Count(ReloadInventoryComponent component) { return _counts[(int)component]; }
            public void Add(ReloadInventoryComponent component, int amount) { _counts[(int)component] += amount; }
            public void Remove(ReloadInventoryComponent component, int amount)
            { if (_counts[(int)component] < amount) throw new InvalidOperationException(); _counts[(int)component] -= amount; }
        }

        private sealed class ReloadStateStore : IFirearmReloadStateStore
        {
            private FirearmState _state;
            internal ReloadStateStore(FirearmState state) { _state = state; }
            internal bool ThrowAfterWrite { get; set; }
            public FirearmState Read() { return _state; }
            public void Replace(FirearmState expectedCurrent, FirearmState replacement)
            {
                if (_state != expectedCurrent) throw new InvalidOperationException("stale state");
                _state = replacement;
                if (ThrowAfterWrite)
                {
                    ThrowAfterWrite = false;
                    throw new InvalidOperationException("synthetic post-write failure");
                }
            }
        }
    }
}
