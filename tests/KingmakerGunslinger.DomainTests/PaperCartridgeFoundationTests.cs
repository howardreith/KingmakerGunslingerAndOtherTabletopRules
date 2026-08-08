using System;
using System.Collections.Generic;
using System.IO;
using KingmakerGunslinger.Ammunition;
using KingmakerGunslinger.Firearms;

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
    }
}
