using System;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Phase 1 Sprint 7 - Big-Cat Combat System. Leopard, Lion, Dire Lion and
    /// Smilodon (Dire Tiger) grab with their claws on the shared summon grapple
    /// lifecycle and rake only on a charge or while holding; the Lion wears a
    /// tawny tint on the leopard rig. Sizes, reach, templates and tiers are
    /// unchanged.
    /// </summary>
    internal static class ExpandedSummoningSprint7Tests
    {
        private static readonly string[] Keys = { "leopard", "lion", "dire-lion", "dire-tiger" };

        /// <summary>
        /// Identities Sprint 7 appended to the frozen ledger: four cat
        /// combat-trait carriers. No new units, placements or icons.
        /// </summary>
        internal const int AppendedLedgerIdentities = 4;

        internal static void RakeIsChargeOnly()
        {
            ExpandedSummoningSpecialProfiles.Validate();
            Assertions.Equal(2, ExpandedSummoningSpecialProfiles.CatRakeSlotCount,
                "A cat rakes with two claws.");
            Assertions.False(ExpandedSummoningSpecialProfiles.ShouldRakeApply(true, false, false),
                "A rake claw on an ordinary attack never strikes.");
            Assertions.True(ExpandedSummoningSpecialProfiles.ShouldRakeApply(true, true, false),
                "A rake claw strikes on a charge.");
            Assertions.True(ExpandedSummoningSpecialProfiles.ShouldRakeApply(true, false, true),
                "A rake claw strikes against a held foe.");
            Assertions.True(ExpandedSummoningSpecialProfiles.ShouldRakeApply(false, false, false),
                "A primary claw or bite is untouched.");
            string components = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Summoning",
                "ExpandedSummoningSpecialCombatComponents.cs"));
            foreach (string token in new[] {
                "class SummonRakeComponent", "RuleInitiatorLogicComponent<RuleAttackRoll>",
                "evt.RuleAttackWithWeapon.IsCharge", "UnitPartGrappleInitiator",
                "evt.AutoMiss = true;", "evt.SuspendCombatLog = true;",
                "ShouldRakeApply(isRakeWeapon", "CatRakeSlotCount" })
                Assertions.True(components.Contains(token), "Rake component contract is missing: " + token);
        }

        internal static void CatsCarryGrabAndRake()
        {
            ExpandedSummoningNaturalProfiles.Validate();
            foreach (string key in Keys)
            {
                NaturalSummonProfile profile = ExpandedSummoningNaturalProfiles.For(key);
                Assertions.True(profile.Facts.Contains("Pounce"), "Pounce stays native: " + key);
                Assertions.True(profile.Deviations.Any(value => value.Contains("charge-only rake component")),
                    "The rake cadence must be recorded: " + key);
                Assertions.True(profile.Deviations.Any(value => value.Contains("shared summon grapple lifecycle (Sprint 7)")),
                    "The claw grab must be recorded: " + key);
                Assertions.False(profile.Deviations.Any(value => value.Contains("requires runtime qualification")),
                    "The old placeholder deviation is retired: " + key);
            }
            Assertions.Equal(4, ExpandedSummoningNaturalProfiles.For("leopard").AdditionalWeapons.Count,
                "The leopard keeps two claws and two rake claws.");
            Assertions.Equal(2, ExpandedSummoningNaturalProfiles.For("dire-tiger").AdditionalSecondaryWeapons.Count,
                "The smilodon keeps its secondary rake pair.");
            var identities = ExpandedSummoningIdentityCatalog.Build();
            foreach (string symbol in new[] {
                "KMG.Summoning.Special.Leopard.CombatTraits", "KMG.Summoning.Special.Lion.CombatTraits",
                "KMG.Summoning.Special.DireLion.CombatTraits", "KMG.Summoning.Special.DireTiger.CombatTraits" })
                Assertions.Equal(1, identities.Count(value => value.Symbol == symbol &&
                    value.PlannedType == "BlueprintBuff"), "Cat carrier identity missing: " + symbol);
            string builder = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Blueprints", "ExpandedSummoningSpecialBuilder.cs"));
            foreach (string token in new[] {
                "ConfigureCat(library, bySymbol, LeopardUnitSymbol", "SmallClawGuid, hold, grappled);",
                "ConfigureCat(library, bySymbol, LionUnitSymbol", "MediumClawGuid, hold, grappled);",
                "ConfigureCat(library, bySymbol, DireLionUnitSymbol", "LargeClawGuid, hold, grappled);",
                "ConfigureCat(library, bySymbol, DireTigerUnitSymbol", "Claw2d4Guid, hold, grappled);",
                "ScriptableObject.CreateInstance<SummonRakeComponent>()",
                "800092a2b9a743b48ae8aeeb5d243dcc", "8afc47748d00b3e4a8aff2787d9ee350" })
                Assertions.True(builder.Contains(token), "Cat pack contract is missing: " + token);
            // Placement, size and template are the same as before the sprint.
            SummonCreatureSpec lion = ExpandedSummoningCatalog.All.Single(value => value.Key == "lion");
            Assertions.True(lion.MonsterTier == 4 && lion.NaturesAllyTier == 4 && lion.MonsterTemplated,
                "The Lion's tiers and template are unchanged.");
            SummonCreatureSpec smilodon = ExpandedSummoningCatalog.All.Single(value => value.Key == "dire-tiger");
            Assertions.True(smilodon.MonsterTier == 6 && smilodon.NaturesAllyTier == 6 && smilodon.MonsterTemplated,
                "The Smilodon's tiers and template are unchanged.");
        }

        internal static void LionVisualIsBounded()
        {
            SummonVisualTintProfile tint = ExpandedSummoningSpecialProfiles.LionVisualTint;
            Assertions.True(tint.IsBounded && tint.Key == "lion" && !tint.HasEmission,
                "The Lion's tint is a bounded, glow-free warm coat.");
            Assertions.True(tint.TintRed > tint.TintGreen && tint.TintGreen > tint.TintBlue,
                "The tint warms the leopard rig toward tawny.");
            string builder = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Blueprints", "ExpandedSummoningSpecialBuilder.cs"));
            Assertions.True(builder.Contains("InternalName(LionUnitSymbol), ExpandedSummoningSpecialProfiles.LionVisualTint"),
                "The Lion registers its visual variant on the shared patch.");
            Assertions.Equal("Leopard", ExpandedSummoningCatalog.All.Single(value => value.Key == "lion").Visual,
                "The Lion keeps the leopard rig.");
        }

        internal static void LedgerCoversTheCatCarriers()
        {
            string ledger = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "blueprints", "blueprints.json"));
            var entries = Newtonsoft.Json.Linq.JObject.Parse(ledger)["entries"]
                .Select(value => (string)value["symbol"]).ToArray();
            string[] appended = entries.Where(value =>
                value == "KMG.Summoning.Special.Leopard.CombatTraits" ||
                value == "KMG.Summoning.Special.Lion.CombatTraits" ||
                value == "KMG.Summoning.Special.DireLion.CombatTraits" ||
                value == "KMG.Summoning.Special.DireTiger.CombatTraits").ToArray();
            Assertions.Equal(AppendedLedgerIdentities, appended.Length,
                "Sprint 7 must append exactly its own identities to the ledger.");
            // Append-only: the Sprint 7 block sits directly before the Sprint 8
            // block at the ledger's tail, and directly after the Sprint 6 block.
            Assertions.True(entries.Skip(entries.Length - AppendedLedgerIdentities -
                    ExpandedSummoningSprint8Tests.AppendedLedgerIdentities)
                .Take(AppendedLedgerIdentities)
                .All(value => appended.Contains(value)),
                "The ledger is append-only: Sprint 7 identities sit directly before Sprint 8's.");
            Assertions.True(entries.Skip(entries.Length - AppendedLedgerIdentities -
                    ExpandedSummoningSprint8Tests.AppendedLedgerIdentities -
                    ExpandedSummoningSprint6Tests.AppendedLedgerIdentities)
                .Take(ExpandedSummoningSprint6Tests.AppendedLedgerIdentities)
                .All(value => !appended.Contains(value)),
                "The Sprint 6 append stays exactly before the Sprint 7 append.");
        }
    }
}
