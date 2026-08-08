using System;
using System.Collections.Generic;
using System.IO;
using KingmakerGunslinger.Ammunition;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Reloading;

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
                "DeactivateIfCombatEnded = false", "HiddenInUi" })
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
