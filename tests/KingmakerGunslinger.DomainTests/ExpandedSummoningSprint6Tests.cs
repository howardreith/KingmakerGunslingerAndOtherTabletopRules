using System;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Phase 1 Sprint 6 - Existing Signature Mechanics Repair. The Monitor
    /// Lizard, Grizzly Bear and Dire Bear grab on the shared summon grapple
    /// lifecycle (no donor constrict imported); the Giant Spider gains
    /// tremorsense (native blindsight), web immunity and a bounded ranged
    /// Web with its own brain; the Pixie's sleep arrows and irresistible
    /// dance stay exact and resource-bounded.
    /// </summary>
    internal static class ExpandedSummoningSprint6Tests
    {
        /// <summary>
        /// Identities Sprint 6 appended to the frozen ledger: three grab
        /// carriers and the spider's web, resource, cast action, brain and
        /// traits. No new units, placements or icons.
        /// </summary>
        internal const int AppendedLedgerIdentities = 8;

        internal static void GrabCarriersRideTheSharedLifecycle()
        {
            ExpandedSummoningNaturalProfiles.Validate();
            foreach (string key in new[] { "monitor-lizard", "grizzly-bear", "dire-bear" })
            {
                NaturalSummonProfile profile = ExpandedSummoningNaturalProfiles.For(key);
                Assertions.True(profile.Deviations.Any(value =>
                        value.Contains("shared summon grapple lifecycle")),
                    "The grab must be recorded as riding the shared lifecycle: " + key);
                Assertions.False(profile.Deviations.Any(value => value.Contains("Grab is omitted")),
                    "The grab is no longer omitted: " + key);
            }
            var identities = ExpandedSummoningIdentityCatalog.Build();
            foreach (string symbol in new[] {
                "KMG.Summoning.Special.MonitorLizard.CombatTraits",
                "KMG.Summoning.Special.GrizzlyBear.CombatTraits",
                "KMG.Summoning.Special.DireBear.CombatTraits" })
                Assertions.Equal(1, identities.Count(value => value.Symbol == symbol &&
                    value.PlannedType == "BlueprintBuff"), "Grab carrier identity missing: " + symbol);
            string builder = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Blueprints", "ExpandedSummoningSpecialBuilder.cs"));
            foreach (string token in new[] {
                "MonitorLizardCombatTraitsSymbol, \"MonitorLizard\", \"Monitor Lizard Grab\"",
                "GrizzlyBearCombatTraitsSymbol, \"GrizzlyBear\", \"Grizzly Bear Grab\"",
                "DireBearCombatTraitsSymbol, \"DireBear\", \"Dire Bear Grab\"",
                "new[] { MediumBite1d8Guid }, hold, grappled, null, 0, 0",
                "c988aa874d11ff84d873508ddc9b928f" })
                Assertions.True(builder.Contains(token), "Grab carrier contract is missing: " + token);
            // The carriers never import the mound's constrict: only the mound
            // passes constrict dice to the grabber.
            int constricting = builder.Split(new[] { "ConfigureGrabber(library, bySymbol," },
                StringSplitOptions.None).Skip(1).Count(value =>
                    value.Contains("ShamblingMoundConstrictDice"));
            Assertions.Equal(1, constricting, "Only the Shambling Mound constricts.");
        }

        internal static void GiantSpiderWebIsBounded()
        {
            ExpandedSummoningSpecialProfiles.Validate();
            Assertions.Equal(2, ExpandedSummoningSpecialProfiles.GiantSpiderWebUses,
                "Two webs per summoning.");
            Assertions.Equal(50, ExpandedSummoningSpecialProfiles.GiantSpiderWebRangeFeet,
                "The web reaches 50 feet.");
            Assertions.Equal(10, ExpandedSummoningSpecialProfiles.GiantSpiderWebRounds,
                "A web holds at most ten rounds.");
            Assertions.Equal(1, ExpandedSummoningSpecialProfiles.GiantSpiderWebSpellLevel,
                "The web's DC scales as a level-one effect (10 + half hit dice + Constitution).");
            NaturalSummonProfile spider = ExpandedSummoningNaturalProfiles.For("giant-spider");
            Assertions.True(spider.Facts.Contains("Blindsight") &&
                spider.Facts.Contains("SpiderWebImmunity") &&
                spider.Facts.Contains("GiantSpiderPoison") &&
                spider.Facts.Contains("TripDefenseEightLegs"),
                "The spider carries blindsight (tremorsense), web immunity, poison and its eight-leg trip defense.");
            Assertions.True(spider.Deviations.Any(value => value.Contains("Web is a bounded ranged ability")) &&
                spider.Deviations.Any(value => value.Contains("climb movement is omitted")),
                "The spider's deviations record the bounded web and the omitted climb.");
            var identities = ExpandedSummoningIdentityCatalog.Build();
            foreach (string[] pair in new[] {
                new[] { "KMG.Summoning.Special.GiantSpider.Web", "BlueprintAbility" },
                new[] { "KMG.Summoning.Special.GiantSpider.WebResource", "BlueprintAbilityResource" },
                new[] { "KMG.Summoning.Special.GiantSpider.WebAi", "BlueprintAiCastSpell" },
                new[] { "KMG.Summoning.Special.GiantSpider.Brain", "BlueprintBrain" },
                new[] { "KMG.Summoning.Special.GiantSpider.CombatTraits", "BlueprintBuff" } })
                Assertions.Equal(1, identities.Count(value => value.Symbol == pair[0] &&
                    value.PlannedType == pair[1]), "Web identity missing: " + pair[0]);
            string builder = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Blueprints", "ExpandedSummoningSpecialBuilder.cs"));
            foreach (string token in new[] {
                "ConfigureGiantSpiderWeb(library, bySymbol)", "a719abac0ea0ce346b401060754cc1c0",
                "AbilityRange.Custom", "GiantSpiderWebRangeFeet", "SavingThrowType.Reflex",
                "GiantSpiderWebRounds", "GiantSpiderWebUses", "web.CanTargetEnemies = true",
                "web.CanTargetFriends = false" })
                Assertions.True(builder.Contains(token), "Web contract is missing: " + token);
            string natural = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Blueprints", "ExpandedSummoningNaturalBuilder.cs"));
            Assertions.True(natural.Contains("\"SpiderWebImmunity\", \"3051e7002c803fc47a11bcfa381b9fbd\""),
                "The natural builder must know the native web immunity.");
        }

        internal static void PixieIsVerifiedNotChanged()
        {
            ExpandedSummoningSpecialProfiles.Validate();
            Assertions.Equal(16, ExpandedSummoningSpecialProfiles.PixieSleepArrowUses,
                "Sixteen sleep arrows per summoning.");
            Assertions.Equal(15, ExpandedSummoningSpecialProfiles.PixieSleepArrowWillDc,
                "Sleep arrow Will DC 15.");
            string builder = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Blueprints", "ExpandedSummoningSpecialBuilder.cs"));
            // The dance is one use on a named resource and its cast action has
            // no cooldown loop to spin on: the resource bounds the AI.
            Assertions.True(builder.Contains("PixieDanceResourceSymbol") &&
                builder.Contains("ai.name = InternalName(PixieDanceAiSymbol);") &&
                builder.Contains("brain.Actions = new BlueprintAiAction[] { ai };"),
                "The Pixie's dance stays resource-bounded with one cast action.");
            var identities = ExpandedSummoningIdentityCatalog.Build();
            Assertions.Equal(9, identities.Count(value =>
                    value.Symbol.StartsWith("KMG.Summoning.Special.Pixie.", StringComparison.Ordinal)),
                "The Pixie's nine specials are unchanged by the repair sprint.");
            string runner = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "RuntimeTesting", "RuntimeTestRunner.cs"));
            Assertions.True(runner.Contains("danceApplied") && runner.Contains("sleepApplied") &&
                runner.Contains("KMG_Summoning_Special_Pixie_IrresistibleDanceResource"),
                "The mechanical scenario keeps proving the dance and the sleep arrows live.");
        }

        internal static void LedgerCoversTheRepairSpecials()
        {
            string ledger = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "blueprints", "blueprints.json"));
            var entries = Newtonsoft.Json.Linq.JObject.Parse(ledger)["entries"]
                .Select(value => (string)value["symbol"]).ToArray();
            string[] appended = entries.Where(value =>
                value == "KMG.Summoning.Special.MonitorLizard.CombatTraits" ||
                value == "KMG.Summoning.Special.GrizzlyBear.CombatTraits" ||
                value == "KMG.Summoning.Special.DireBear.CombatTraits" ||
                value.StartsWith("KMG.Summoning.Special.GiantSpider.", StringComparison.Ordinal))
                .ToArray();
            Assertions.Equal(AppendedLedgerIdentities, appended.Length,
                "Sprint 6 must append exactly its own identities to the ledger.");
            // Append-only: the Sprint 6 block sits directly before the Sprint 7
            // block at the ledger's tail, and directly after the Sprint 5 block.
            Assertions.True(entries.Skip(entries.Length - AppendedLedgerIdentities -
                    ExpandedSummoningSprint7Tests.AppendedLedgerIdentities -
                    ExpandedSummoningSprint8Tests.AppendedLedgerIdentities)
                .Take(AppendedLedgerIdentities)
                .All(value => appended.Contains(value)),
                "The ledger is append-only: Sprint 6 identities sit directly before Sprint 7's.");
            Assertions.True(entries.Skip(entries.Length - AppendedLedgerIdentities -
                    ExpandedSummoningSprint7Tests.AppendedLedgerIdentities -
                    ExpandedSummoningSprint8Tests.AppendedLedgerIdentities -
                    ExpandedSummoningSprint5Tests.AppendedLedgerIdentities)
                .Take(ExpandedSummoningSprint5Tests.AppendedLedgerIdentities)
                .All(value => !appended.Contains(value)),
                "The Sprint 5 append stays exactly before the Sprint 6 append.");
            SummonIconCatalog.Validate();
            Assertions.Equal(91, SummonIconCatalog.All.Count,
                "Sprint 6 adds no icon of its own: the catalog holds the ninety earlier icons and the Sprint 8 Tiger.");
        }
    }
}
