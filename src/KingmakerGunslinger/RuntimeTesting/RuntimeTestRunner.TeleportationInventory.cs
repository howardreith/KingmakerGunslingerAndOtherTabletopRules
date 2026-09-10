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
            ObserveTeleportationSpellPublication(assertions);
            return CreateResult(assertions.All(value => value.Status == "PASS") ?
                RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions, null);
        }
    }
}
