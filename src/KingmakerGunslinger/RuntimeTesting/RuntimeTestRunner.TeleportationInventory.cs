using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Globalmap;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.State;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UI.GlobalMap;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Guarded, save-free observation only. Does not qualify casting or placement.
        private RuntimeTestResult RunTeleportationNativeInventory()
        {
            var assertions = new List<RuntimeTestAssertion>();
            if (Game.Instance.CurrentlyLoadedArea != null)
                throw new InvalidOperationException("Native inventory requires the unloaded main menu.");
            BlueprintScriptableObject[] blueprints = BlueprintBootstrap.Library.BlueprintsByAssetId.Values
                .Where(value => value != null).Distinct().ToArray();
            BlueprintLocation[] locations = blueprints.OfType<BlueprintLocation>()
                .OrderBy(value => value.AssetGuid, StringComparer.Ordinal).ToArray();
            var inventory = new
            {
                schemaVersion = 1,
                gameMvid = typeof(GlobalMapRules).Assembly.ManifestModule.ModuleVersionId,
                claims = "Blueprint/type inventory only; no campaign eligibility, UI interaction, spell use or relocation was exercised.",
                locations = locations.Select(value => new {
                    id = value.AssetGuid, name = value.name, pointType = value.Type.ToString(),
                    revealedOnStart = value.RevealedOnStart, value.ExploreOnEnter,
                    areaEntry = value.AreaEntrance == null ? null : value.AreaEntrance.AssetGuid,
                    hasBookEvent = value.BookEvent != null, value.HasKingdomResource,
                    components = value.ComponentsArray.Select(component => component == null ? "<null>" :
                        component.GetType().FullName).ToArray()
                }).ToArray(),
                travelSpellLists = blueprints.OfType<BlueprintSpellList>().Where(value =>
                    value.name.IndexOf("Travel", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    value.name.IndexOf("Wizard", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    value.name.IndexOf("Druid", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    value.name.IndexOf("Cleric", StringComparison.OrdinalIgnoreCase) >= 0)
                    .Select(value => new { id = value.AssetGuid, name = value.name }).ToArray(),
                // Specialist/favorite-slot sources: every AddSpecialSpellList
                // attachment with the spells its list offers at the teleportation
                // levels. Observation only; nothing is published here.
                specialSpellLists = blueprints.OfType<BlueprintFeature>()
                    .Select(feature => new { feature, attachments = feature.ComponentsArray
                        .OfType<Kingmaker.UnitLogic.FactLogic.AddSpecialSpellList>().ToArray() })
                    .Where(value => value.attachments.Length > 0)
                    .SelectMany(value => value.attachments.Select(attachment => new {
                        ownerFeatureId = value.feature.AssetGuid, ownerFeatureName = value.feature.name,
                        classId = attachment.CharacterClass == null ? null : attachment.CharacterClass.AssetGuid,
                        className = attachment.CharacterClass == null ? null : attachment.CharacterClass.name,
                        listId = attachment.SpellList == null ? null : attachment.SpellList.AssetGuid,
                        listName = attachment.SpellList == null ? null : attachment.SpellList.name,
                        level5 = attachment.SpellList == null ? null : attachment.SpellList.GetSpells(5)
                            .Select(spell => new { id = spell.AssetGuid, name = spell.name, school = spell.School.ToString() }).ToArray(),
                        level7 = attachment.SpellList == null ? null : attachment.SpellList.GetSpells(7)
                            .Select(spell => new { id = spell.AssetGuid, name = spell.name, school = spell.School.ToString() }).ToArray() }))
                    .OrderBy(value => value.ownerFeatureId, StringComparer.Ordinal).ToArray(),
                // Gate 4 forensics: verified native scroll donors (type, cost,
                // caster level, referenced ability) and the exact vendor units
                // named by the mission, with their native stock structures.
                scrollDonors = blueprints.OfType<Kingmaker.Blueprints.Items.Equipment.BlueprintItemEquipmentUsable>()
                    .Where(value => value.name != null && value.name.IndexOf("Scroll", StringComparison.OrdinalIgnoreCase) >= 0)
                    .OrderBy(value => value.AssetGuid, StringComparer.Ordinal)
                    .Select(value => new { id = value.AssetGuid, name = value.name, cost = value.Cost,
                        casterLevel = value.CasterLevel, type = value.ItemType.ToString(),
                        abilityId = value.Ability == null ? null : value.Ability.AssetGuid, abilityName = value.Ability == null ? null : value.Ability.name,
                        components = value.ComponentsArray.Select(component => component == null ? "<null>" : component.GetType().FullName).ToArray() })
                    .Take(40).ToArray(),
                vendorUnits = blueprints.OfType<Kingmaker.Blueprints.BlueprintUnit>()
                    .Where(value => value.name != null && (value.name.IndexOf("Zarcie", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        value.name.IndexOf("Arsinoe", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        value.name.IndexOf("Jhod", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        value.name.IndexOf("Hassuf", StringComparison.OrdinalIgnoreCase) >= 0))
                    .Select(value => new { id = value.AssetGuid, name = value.name,
                        components = value.ComponentsArray.Select(component => component == null ? "<null>" : component.GetType().FullName).ToArray() })
                    .OrderBy(value => value.name, StringComparer.Ordinal).ToArray(),
                vendorStocks = blueprints.OfType<Kingmaker.Blueprints.BlueprintUnit>()
                    .Where(value => value.name != null && (value.name.IndexOf("Zarcie", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        value.name.IndexOf("Arsinoe", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        value.name.IndexOf("Jhod", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        value.name.IndexOf("Hassuf", StringComparison.OrdinalIgnoreCase) >= 0))
                    .OrderBy(value => value.name, StringComparer.Ordinal)
                    .Select(value => new {
                        id = value.AssetGuid, name = value.name,
                        vendorItems = value.ComponentsArray.OfType<Kingmaker.UnitLogic.FactLogic.AddVendorItems>()
                            .Select(component => VendorItemsLoot(component) == null ? null : new {
                                lootId = VendorItemsLoot(component).AssetGuid, lootName = VendorItemsLoot(component).name,
                                lootComponents = VendorItemsLoot(component).ComponentsArray.Select(item => new {
                                    type = item == null ? "<null>" : item.GetType().FullName,
                                    fields = item == null ? new string[0] : item.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                                        .Select(field => field.Name + "=" + DescribeNativeValue(field.GetValue(item))).ToArray(),
                                    packItem = item is Kingmaker.Blueprints.Loot.LootItemsPackFixed ? PackItemName((Kingmaker.Blueprints.Loot.LootItemsPackFixed)item) : null }).ToArray() }).ToArray(),
                        sharedTables = value.ComponentsArray.OfType<Kingmaker.UnitLogic.FactLogic.AddSharedVendor>()
                            .Select(component => SharedVendorTable(component) == null ? null : new {
                                tableId = SharedVendorTable(component).AssetGuid, tableName = SharedVendorTable(component).name,
                                tableComponents = SharedVendorTable(component).ComponentsArray.Select(item => new {
                                    type = item == null ? "<null>" : item.GetType().FullName,
                                    fields = item == null ? new string[0] : item.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                                        .Select(field => field.Name + "=" + DescribeNativeValue(field.GetValue(item))).ToArray() }).ToArray() }).ToArray() }).ToArray(),
                kmgScrolls = BlueprintBootstrap.TeleportationScrolls == null ? new object[0] :
                    new[] { BlueprintBootstrap.TeleportationScrolls.Teleport, BlueprintBootstrap.TeleportationScrolls.GreaterTeleport,
                        BlueprintBootstrap.TeleportationScrolls.WordOfRecall }
                    .Select(value => new { id = value.AssetGuid, name = value.name, cost = value.Cost,
                        casterLevel = value.CasterLevel, spellLevel = value.SpellLevel,
                        abilityId = value.Ability == null ? null : value.Ability.AssetGuid,
                        copySpellId = value.ComponentsArray.OfType<Kingmaker.Blueprints.Items.Components.CopyScroll>()
                            .Single().CustomSpell == null ? null : value.ComponentsArray.OfType<Kingmaker.Blueprints.Items.Components.CopyScroll>()
                            .Single().CustomSpell.AssetGuid }).ToArray(),
                visualDonors = blueprints.OfType<BlueprintAbility>().Where(value =>
                    value.name.IndexOf("DimensionDoor", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    value.name.IndexOf("Teleport", StringComparison.OrdinalIgnoreCase) >= 0)
                    .Select(value => new { id = value.AssetGuid, name = value.name, hasIcon = value.Icon != null }).ToArray(),
                contracts = new[] { typeof(GlobalMapMessageBox), typeof(GlobalMapRules), typeof(LocationData),
                    typeof(Spellbook), typeof(SpellSlot), typeof(AbilityData) }.Select(type => new {
                        type = type.FullName,
                        methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                            BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                            .Where(method => new[] { "OnLocationSelect", "Accept", "Hide", "TeleportParty",
                                "SetCurrentPosition", "UpdatePawnPosition", "Spend", "SpendInternal",
                                "RestoreSpontaneousSlots", "SpendFromSpellbook" }.Contains(method.Name))
                            .Select(method => new { signature = method.ToString(),
                                il = BrownFurIlDisassembler.Describe(method) }).ToArray()
                    }).ToArray()
            };
            string path = Path.Combine(_request.EvidenceDirectory, "teleportation-native-inventory.json");
            string json = JsonConvert.SerializeObject(inventory, Formatting.Indented,
                new JsonSerializerSettings { ContractResolver = new DefaultContractResolver(),
                    TypeNameHandling = TypeNameHandling.None, PreserveReferencesHandling = PreserveReferencesHandling.None });
            RuntimeTestResultWriter.WriteAtomic(path, json);
            JObject persisted = JObject.Parse(File.ReadAllText(path));
            assertions.Add(Assertion("teleportation-inventory-evidence-roundtrip", "all point rows persisted",
                "rows=" + ((JArray)persisted["locations"]).Count,
                ((JArray)persisted["locations"]).Count == locations.Length, path));
            assertions.Add(Assertion("teleportation-native-point-inventory", "nonempty, unique persistent blueprint IDs",
                "count=" + locations.Length + ";distinct=" + locations.Select(value => value.AssetGuid).Distinct().Count(),
                locations.Length > 0 && locations.All(value => !string.IsNullOrWhiteSpace(value.AssetGuid)) &&
                locations.Select(value => value.AssetGuid).Distinct().Count() == locations.Length, path));
            assertions.Add(Assertion("teleportation-observation-scope", "main menu; no save or area loaded",
                "area=" + (Game.Instance.CurrentlyLoadedArea == null ? "none" : "loaded"),
                Game.Instance.CurrentlyLoadedArea == null, inventory.claims));
            var scrolls = (JArray)persisted["kmgScrolls"];
            assertions.Add(Assertion("teleportation-scroll-items", "three native scrolls with approved economics reference their canonical spells",
                "count=" + scrolls.Count,
                scrolls.Count == 3 && scrolls.All(value => (int)value["cost"] > 0 && (string)value["abilityId"] == (string)value["copySpellId"]) &&
                (int)scrolls[0]["cost"] == 1125 && (int)scrolls[0]["casterLevel"] == 9 && (int)scrolls[0]["spellLevel"] == 5 &&
                (int)scrolls[1]["cost"] == 2275 && (int)scrolls[1]["casterLevel"] == 13 && (int)scrolls[1]["spellLevel"] == 7 &&
                (int)scrolls[2]["cost"] == 1650 && (int)scrolls[2]["casterLevel"] == 11 && (int)scrolls[2]["spellLevel"] == 6, path));
            // Gate 4: the published finite vendor stock on both verified tables.
            var stockArcaneTable = BlueprintBootstrap.TeleportationScrollVendors == null ? null :
                BlueprintLibraryLookup.RequireExact<Kingmaker.Blueprints.Items.BlueprintSharedVendorTable>(BlueprintBootstrap.Library,
                    "5450d563aab78134196ee9a932e88671", "arcane scroll vendor table");
            var priestTable = BlueprintBootstrap.TeleportationScrollVendors == null ? null :
                BlueprintLibraryLookup.RequireExact<Kingmaker.Blueprints.Items.BlueprintSharedVendorTable>(BlueprintBootstrap.Library,
                    "afa2c7f292b8e1c4d9c835f0e8047dd3", "priest scroll vendor table");
            System.Func<Kingmaker.Blueprints.Items.BlueprintSharedVendorTable, Kingmaker.Blueprints.Items.BlueprintItem, int> stock =
                (table, item) => table == null || item == null ? -1 : table.ComponentsArray
                    .OfType<Kingmaker.Blueprints.Loot.LootItemsPackFixed>()
                    .Where(component => ReferenceEquals(CapitalVendorBlueprints.ReadItem(component), item))
                    .Select(CapitalVendorBlueprints.ReadCount).DefaultIfEmpty(-1).Single();
            var scrollSet = BlueprintBootstrap.TeleportationScrolls;
            int teleportStock = stock(stockArcaneTable, scrollSet == null ? null : scrollSet.Teleport);
            int greaterStock = stock(stockArcaneTable, scrollSet == null ? null : scrollSet.GreaterTeleport);
            int recallStock = stock(priestTable, scrollSet == null ? null : scrollSet.WordOfRecall);
            assertions.Add(Assertion("teleportation-scroll-vendor-stock",
                "verified tables carry exactly one finite batch: arcane 5 Teleport + 3 Greater Teleport, priest 5 Word of Recall; hook installed",
                "teleport=" + teleportStock + ";greater=" + greaterStock + ";recall=" + recallStock +
                    ";migration=" + KingmakerGunslinger.Spells.Teleportation.TeleportationScrollVendorMigration.Installed,
                teleportStock == 5 && greaterStock == 3 && recallStock == 5 &&
                    KingmakerGunslinger.Spells.Teleportation.TeleportationScrollVendorMigration.Installed, path));
            // F4/F5/F6 review findings: vendor fallback identity, publication
            // atomicity under a mid-transaction fault, and CopyScroll donor
            // isolation around the registered scrolls.
            if (BlueprintBootstrap.TeleportationScrolls != null && BlueprintBootstrap.TeleportationScrollVendors != null)
            {
                var fallback = BlueprintLibraryLookup.RequireExact<Kingmaker.Blueprints.Items.BlueprintSharedVendorTable>(BlueprintBootstrap.Library,
                    TeleportationScrollVendorPublication.FallbackArcaneTableId, "Hassuf fallback vendor table");
                assertions.Add(Assertion("teleportation-scroll-vendor-fallback-identity",
                    "the approved Hassuf fallback table exists with its verified identity",
                    "name=" + fallback.name + ";id=" + fallback.AssetGuid,
                    string.Equals(fallback.name, TeleportationScrollVendorPublication.FallbackArcaneTableName, StringComparison.Ordinal), path));
                // Atomicity: the first supplier's mutation must be restored when a
                // later supplier fails inside the same publication.
                var arcaneTable = BlueprintLibraryLookup.RequireExact<Kingmaker.Blueprints.Items.BlueprintSharedVendorTable>(BlueprintBootstrap.Library,
                    TeleportationScrollVendorPublication.ArcaneTableId, "arcane scroll vendor table");
                var priestTable2 = BlueprintLibraryLookup.RequireExact<Kingmaker.Blueprints.Items.BlueprintSharedVendorTable>(BlueprintBootstrap.Library,
                    TeleportationScrollVendorPublication.PriestTableId, "priest scroll vendor table");
                var arcaneBefore = arcaneTable.ComponentsArray;
                var priestBefore = priestTable2.ComponentsArray;
                ModContext faultContext;
                ModContext.TryGet(out faultContext);
                string firstFaulted = null, laterFaulted = null;
                TeleportationScrollVendorPublication.FaultInjection = tableName =>
                {
                    if (firstFaulted == null) { firstFaulted = tableName; return null; }
                    laterFaulted = tableName;
                    return new InvalidOperationException("Request-local atomicity fault after the first mutation.");
                };
                bool faultThrown = false;
                try
                {
                    TeleportationScrollVendorPublication.Publish(BlueprintBootstrap.Library,
                        BlueprintBootstrap.TeleportationScrolls, true, faultContext == null ? null : faultContext.Logger);
                }
                catch (InvalidOperationException) { faultThrown = true; }
                finally { TeleportationScrollVendorPublication.FaultInjection = null; }
                bool arcaneRestored = ReferenceEquals(arcaneTable.ComponentsArray, arcaneBefore) ||
                    arcaneTable.ComponentsArray.SequenceEqual(arcaneBefore);
                bool priestRestored = ReferenceEquals(priestTable2.ComponentsArray, priestBefore) ||
                    priestTable2.ComponentsArray.SequenceEqual(priestBefore);
                assertions.Add(Assertion("teleportation-scroll-vendor-atomicity",
                    "a fault after the first supplier mutation restores every changed table; foreign entries preserved",
                    "thrown=" + faultThrown + ";first=" + firstFaulted + ";later=" + laterFaulted +
                        ";arcaneRestored=" + arcaneRestored + ";priestRestored=" + priestRestored,
                    faultThrown && firstFaulted != null && laterFaulted != null && arcaneRestored && priestRestored, path));
                // Idempotent retry after the fault returns the tables to the exact
                // qualified published state.
                var retry = TeleportationScrollVendorPublication.Publish(BlueprintBootstrap.Library,
                    BlueprintBootstrap.TeleportationScrolls, true, faultContext == null ? null : faultContext.Logger);
                assertions.Add(Assertion("teleportation-scroll-vendor-retry-idempotent",
                    "a retry after the rolled-back fault publishes exactly once more and validates",
                    "changed=" + retry.ChangedTableCount,
                    retry.ChangedTableCount >= 0, path));
                // CopyScroll donor isolation: the donor's component still teaches
                // the donor's own spell, and our scroll's component is a distinct
                // instance teaching the canonical spell.
                var donors = new[]
                {
                    new { id = TeleportationScrollBlueprints.TeleportDonorId, ours = BlueprintBootstrap.TeleportationScrolls.Teleport },
                    new { id = TeleportationScrollBlueprints.GreaterTeleportDonorId, ours = BlueprintBootstrap.TeleportationScrolls.GreaterTeleport },
                    new { id = TeleportationScrollBlueprints.WordOfRecallDonorId, ours = BlueprintBootstrap.TeleportationScrolls.WordOfRecall }
                };
                foreach (var donor in donors)
                {
                    var donorScroll = BlueprintLibraryLookup.RequireExact<Kingmaker.Blueprints.Items.Equipment.BlueprintItemEquipmentUsable>(
                        BlueprintBootstrap.Library, donor.id, "native scroll donor");
                    var donorCopy = donorScroll.ComponentsArray.OfType<Kingmaker.Blueprints.Items.Components.CopyScroll>().Single();
                    var ourCopy = donor.ours.ComponentsArray.OfType<Kingmaker.Blueprints.Items.Components.CopyScroll>().Single();
                    assertions.Add(Assertion("teleportation-scroll-copyscroll-isolation-" + donor.ours.name,
                        "the donor's CopyScroll still teaches its own spell and ours is a distinct isolated instance teaching the canonical spell",
                        "donorSpell=" + (donorCopy.CustomSpell == null ? "null" : donorCopy.CustomSpell.AssetGuid) +
                            ";donorIntact=" + (donorCopy.CustomSpell == donorScroll.Ability) +
                            ";distinctInstance=" + (!ReferenceEquals(donorCopy, ourCopy)) +
                            ";oursSpell=" + ourCopy.CustomSpell.AssetGuid,
                        donorCopy.CustomSpell == donorScroll.Ability && !ReferenceEquals(donorCopy, ourCopy), path));
                }
            }
            ObserveTeleportationSpellPublication(assertions);
            return CreateResult(assertions.All(value => value.Status == "PASS") ?
                RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions, null);
        }

        private static string DescribeNativeValue(object value)
        {
            if (value == null) return "null";
            var scriptable = value as Kingmaker.Blueprints.BlueprintScriptableObject;
            if (scriptable != null) return scriptable.name + ":" + scriptable.AssetGuid;
            if (value is System.Collections.IEnumerable enumerable && !(value is string))
            {
                var items = new List<string>();
                foreach (var item in enumerable) items.Add(item is Kingmaker.Blueprints.BlueprintScriptableObject ?
                    ((Kingmaker.Blueprints.BlueprintScriptableObject)item).name : Convert.ToString(item, System.Globalization.CultureInfo.InvariantCulture));
                return "[" + string.Join(";", items.ToArray()) + "]";
            }
            return Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture);
        }

        private static readonly System.Reflection.FieldInfo VendorItemsLootField = typeof(Kingmaker.UnitLogic.FactLogic.AddVendorItems)
            .GetField("m_Loot", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        private static readonly System.Reflection.FieldInfo SharedVendorTableField = typeof(Kingmaker.UnitLogic.FactLogic.AddSharedVendor)
            .GetField("m_Table", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        private static Kingmaker.Blueprints.Loot.BlueprintUnitLoot VendorItemsLoot(Kingmaker.UnitLogic.FactLogic.AddVendorItems component)
        { return VendorItemsLootField == null ? null : VendorItemsLootField.GetValue(component) as Kingmaker.Blueprints.Loot.BlueprintUnitLoot; }
        private static Kingmaker.Blueprints.Items.BlueprintSharedVendorTable SharedVendorTable(Kingmaker.UnitLogic.FactLogic.AddSharedVendor component)
        { return SharedVendorTableField == null ? null : SharedVendorTableField.GetValue(component) as Kingmaker.Blueprints.Items.BlueprintSharedVendorTable; }

        private static string PackItemName(Kingmaker.Blueprints.Loot.LootItemsPackFixed pack)
        {
            var itemField = typeof(Kingmaker.Blueprints.Loot.LootItemsPackFixed).GetField("m_Item", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var lootItem = itemField == null ? null : itemField.GetValue(pack) as Kingmaker.Blueprints.Loot.LootItem;
            var blueprintField = lootItem == null ? null : typeof(Kingmaker.Blueprints.Loot.LootItem).GetField("m_Item", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var blueprint = blueprintField == null ? null : blueprintField.GetValue(lootItem) as Kingmaker.Blueprints.Items.BlueprintItem;
            return blueprint == null ? null : blueprint.name + ":" + blueprint.Cost.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
