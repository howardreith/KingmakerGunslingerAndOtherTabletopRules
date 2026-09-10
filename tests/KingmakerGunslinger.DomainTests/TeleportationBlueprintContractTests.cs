using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using KingmakerGunslinger.Spells.ShieldOther;

namespace KingmakerGunslinger.DomainTests
{
    internal static class TeleportationBlueprintContractTests
    {
        internal static void StableIdentities()
        {
            var expected = new Dictionary<string, string> {
                { "KMG.Spells.Teleport.Ability", "82e3fb1dce1647b58d3b7169c8520af0" },
                { "KMG.Spells.GreaterTeleport.Ability", "73d19adfe18743e0a2a3a21abf4af5f3" },
                { "KMG.Spells.WordOfRecall.Ability", "596d85a666204d6ea5c0188e53f4b4de" }
            };
            var entries = JObject.Parse(File.ReadAllText("blueprints/blueprints.json"))["entries"].ToArray();
            foreach (var pair in expected) {
                var row = entries.Single(value => (string)value["symbol"] == pair.Key);
                Assertions.Equal(pair.Value, (string)row["guid"], "Immutable strategic spell identity");
                Assertions.Equal("BlueprintAbility", (string)row["plannedType"], "Real spell blueprint type");
                Assertions.Equal("active", (string)row["status"], "Active identity");
                Assertions.Equal(1, entries.Count(value => (string)value["guid"] == pair.Value), "Unique stable GUID");
            }
            Assertions.True(!entries.Any(value => ((string)value["symbol"]).Contains("TeleportWithoutError")),
                "Greater Teleport must have no duplicate synonym spell");
        }
        internal static void BlueprintPresentationAndModuleIsolation()
        {
            string factory = File.ReadAllText("src/KingmakerGunslinger/Blueprints/TeleportationSpellBlueprints.cs");
            foreach (string token in new[] { "ability.Parent = null", "SpellSchool.Conjuration", "CommandType.Standard",
                "ability.ActionBarAutoFillIgnored = true", "ability.MaterialComponent = new BlueprintAbility.MaterialComponentData()",
                "ability.CanTargetPoint = false", "ability.CanTargetSelf = true", "ability.CanTargetEnemies = false",
                "ability.CanTargetFriends = false", "ability.AvailableMetamagic = 0", "ability.SpellResistance = false",
                "new BlueprintComponent[] { school, checker }", "value.GetType() != typeof(SpellListComponent)", "GameModeType.GlobalMap", "Teleport Without Error" })
                Assertions.True(factory.Contains(token), "Strategic blueprint contract missing " + token);
            string bootstrap = File.ReadAllText("src/KingmakerGunslinger/Bootstrap/BlueprintBootstrap.cs");
            Assertions.True(bootstrap.Contains("teleportationRegistry = new BlueprintRegistry") &&
                bootstrap.Contains("if (publicationPlan.TeleportationSpellLists)") &&
                bootstrap.Contains("teleportationRegistry.RollbackAll()") &&
                bootstrap.Contains("other module transactions are preserved"), "Independent identity/publication failure scope");
        }
        internal static void ScrollItemsUseApprovedEconomicsAndCanonicalSpells()
        {
            string source = File.ReadAllText("src/KingmakerGunslinger/Blueprints/TeleportationScrollBlueprints.cs");
            foreach (string token in new[] {
                "TeleportDonorId = \"02086fbbda266ed4b8e9124abe5abd75\"",
                "GreaterTeleportDonorId = \"0033529da3b90bd226232e1962ca34ba\"",
                "WordOfRecallDonorId = \"00843bddf42908953a0d77e7155c20f0\"",
                "cost: 1125, casterLevel: 9, spellLevel: 5",
                "cost: 2275, casterLevel: 13, spellLevel: 7",
                "cost: 1650, casterLevel: 11, spellLevel: 6",
                "scroll.Ability = spell", "copies[0].CustomSpell = spell",
                "donor.Cost != cost || donor.CasterLevel != casterLevel" })
                Assertions.True(source.Contains(token), "Scroll contract missing " + token);
            string bootstrap = File.ReadAllText("src/KingmakerGunslinger/Bootstrap/BlueprintBootstrap.cs");
            Assertions.True(bootstrap.Contains("TeleportationScrollBlueprints.Register(library, teleportationRegistry, teleportation)"),
                "Scrolls register inside the Teleportation module transaction with the canonical spells.");
        }
        internal static void ScrollVendorStockUsesVerifiedTablesAndFiniteBatches()
        {
            string source = File.ReadAllText("src/KingmakerGunslinger/Blueprints/TeleportationScrollVendorPublication.cs");
            foreach (string token in new[] {
                "ArcaneTableId = \"5450d563aab78134196ee9a932e88671\"",
                "PriestTableId = \"afa2c7f292b8e1c4d9c835f0e8047dd3\"",
                "ArcaneTableName = \"ArcaneScrollsVendorTableI\"",
                "PriestTableName = \"C11_JhodVendorTable\"",
                "TeleportStock = 5", "GreaterTeleportStock = 3", "WordOfRecallStock = 5",
                "string.Equals(table.name, expectedName, StringComparison.Ordinal)",
                "VendorCatalogPublication<BlueprintComponent>.Create(retained, additions)" })
                Assertions.True(source.Contains(token), "Vendor stock contract missing " + token);
            string migration = File.ReadAllText("src/KingmakerGunslinger/Spells/Teleportation/TeleportationScrollVendorMigration.cs");
            foreach (string token in new[] {
                "\"shared:\" + sharedTable.AssetGuid",
                "player.SharedVendorTables.GetTable(sharedTable)",
                "HasScrollVendorGrant(target)", "RecordScrollVendorGrant(target)",
                "if (absent)", "else if (!complete)" })
                Assertions.True(migration.Contains(token), "Migration contract missing " + token);
        }
        internal static void PublicationUsesExactListsAndDuplicateSafeMerge()
        {
            string publication = File.ReadAllText("src/KingmakerGunslinger/Blueprints/TeleportationSpellListPublication.cs");
            foreach (string token in new[] { "WizardListId, 5, spells.Teleport", "WizardListId, 7, spells.GreaterTeleport",
                "ClericListId, 6, spells.WordOfRecall", "DruidListId, 8, spells.WordOfRecall",
                "TravelListId, 5, spells.Teleport", "TravelListId, 7, spells.GreaterTeleport",
                "ConjurationListId, 5, spells.Teleport", "ConjurationListId, 7, spells.GreaterTeleport",
                "if (travelPresent)", "mutation.Level.Spells = mutation.Before", "mutation.CacheBefore",
                "ReferenceEquals(mutation.Level.Spells, mutation.After)" })
                Assertions.True(publication.Contains(token), "Publication contract missing " + token);
            foreach (string id in new[] { "teleport", "greater", "recall" }) {
                var native = new Spell("native"); var other = new Spell("other"); var spell = new Spell(id);
                var before = new List<Spell> { native, new Spell(id), other, spell, spell };
                var after = ShieldOtherSpellListMergePolicy.Merge(before, spell, value => value.Id);
                Assertions.True(after.SequenceEqual(new[] { native, other, spell }) && before.Count == 5,
                    "Publication must preserve foreign instance order without mutating original collection");
                Assertions.True(ReferenceEquals(after, ShieldOtherSpellListMergePolicy.Merge(after, spell, value => value.Id)),
                    "Repeated publication must retain the exact already valid list instance");
                Assertions.True(ShieldOtherSpellListMergePolicy.CanRollback(after, after) &&
                    !ShieldOtherSpellListMergePolicy.CanRollback(new List<Spell>(after), after), "Exact rollback ownership");
            }
        }
        private sealed class Spell { internal Spell(string id) { Id = id; } internal string Id { get; private set; } }
    }
}
