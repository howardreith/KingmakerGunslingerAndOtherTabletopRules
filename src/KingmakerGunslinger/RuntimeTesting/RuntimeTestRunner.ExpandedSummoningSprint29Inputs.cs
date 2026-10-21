using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Research only: one bounded read-only pass over actual 2.1.7b
        // blueprints. It spawns no actor, selects no save, and mutates no asset.
        private RuntimeTestResult RunExpandedSummoningSprint29Inputs()
        {
            var checks = new List<RuntimeTestAssertion>();
            var document = new JObject {
                ["runId"] = _request.RunId,
                ["scope"] = "Sprint 29 installed donor, weapon, spell and feat input census; not gameplay proof"
            };
            try
            {
                BlueprintScriptableObject[] all = BlueprintBootstrap.Library
                    .GetAllBlueprints().Where(value => value != null).ToArray();
                string[] names = { "CloudGiant", "StormGiant", "Ettin" };
                BlueprintUnit[] candidates = all.OfType<BlueprintUnit>()
                    .Where(value => value.name != null &&
                        !value.name.StartsWith("KMG_", StringComparison.Ordinal) &&
                        names.Any(name => value.name.IndexOf(name,
                            StringComparison.OrdinalIgnoreCase) >= 0))
                    .OrderBy(value => value.name, StringComparer.Ordinal).ToArray();
                if (candidates.Length > 64)
                    throw new InvalidOperationException("Sprint 29 giant unit census exceeded 64 candidates.");
                var units = new JArray();
                foreach (BlueprintUnit unit in candidates)
                {
                    var row = new JObject {
                        ["nativeUnit"] = DescribeNativeUnit(unit),
                        ["startingInventoryCount"] = unit.StartingInventory == null ? 0 : unit.StartingInventory.Length,
                        ["primary"] = unit.Body == null ? null : DescribeWeapon(unit.Body.PrimaryHand),
                        ["secondary"] = unit.Body == null ? null : DescribeWeapon(unit.Body.SecondaryHand),
                        ["armor"] = unit.Body == null || unit.Body.Armor == null ? null : unit.Body.Armor.name
                    };
                    // Detached metadata from at most one likely adult view per
                    // species. Loading additional campaign variants is unnecessary.
                    if (names.Any(name => string.Equals(unit.name, name,
                            StringComparison.OrdinalIgnoreCase)) &&
                        unit.Prefab != null && !string.IsNullOrEmpty(unit.Prefab.AssetId))
                    {
                        var prefab = unit.Prefab.Load(false);
                        row["view"] = prefab == null ? null :
                            CapturePrimateCensusViewMetadata(prefab,
                                "sprint29-giant-donor", unit.AssetGuid,
                                unit.name, unit.Prefab.AssetId);
                    }
                    units.Add(row);
                }
                document["units"] = units;
                document["speciesCounts"] = new JObject(names.Select(name =>
                    new JProperty(name, candidates.Count(value =>
                        value.name.IndexOf(name,
                            StringComparison.OrdinalIgnoreCase) >= 0))));

                string[] spellNames = { "ObscuringMist", "FogCloud",
                    "FreedomOfMovement", "CallLightning", "ChainLightning",
                    "Levitate", "ControlWeather" };
                var spells = new JArray();
                foreach (string name in spellNames)
                {
                    BlueprintAbility[] matches = all.OfType<BlueprintAbility>()
                        .Where(value => value.name != null &&
                            value.name.IndexOf(name,
                                StringComparison.OrdinalIgnoreCase) >= 0)
                        .OrderBy(value => value.name, StringComparer.Ordinal)
                        .Take(12).ToArray();
                    spells.Add(new JObject {
                        ["query"] = name,
                        ["matches"] = new JArray(matches.Select(value =>
                            new JObject {
                                ["name"] = value.name,
                                ["guid"] = value.AssetGuid,
                                ["graph"] = DescribeGraph(value, 0,
                                    new HashSet<object>(NativeDonorReferenceComparer.Instance), 4)
                            }).ToArray())
                    });
                }
                document["spells"] = spells;

                string[] weaponNames = { "Morningstar", "Greatsword",
                    "CompositeLongbow", "Longbow", "Flail", "Javelin",
                    "Rock" };
                var weapons = new JArray();
                foreach (string name in weaponNames)
                    weapons.Add(new JObject {
                        ["query"] = name,
                        ["matches"] = new JArray(all.OfType<BlueprintItemWeapon>()
                            .Where(value => value.name != null &&
                                value.name.IndexOf(name,
                                    StringComparison.OrdinalIgnoreCase) >= 0)
                            .OrderBy(value => value.name, StringComparer.Ordinal)
                            .Take(18).Select(value => new JObject {
                                ["name"] = value.name,
                                ["guid"] = value.AssetGuid,
                                ["weapon"] = DescribeWeapon(value)
                            }).ToArray())
                    });
                document["weapons"] = weapons;

                string[] featNames = { "VitalStrike", "ImprovedCritical",
                    "CombatReflexes", "Cleave", "RockCatch",
                    "AwesomeBlow", "ImprovedBullRush", "ImprovedOverrun" };
                var feats = new JArray();
                foreach (string name in featNames)
                    feats.Add(new JObject {
                        ["query"] = name,
                        ["matches"] = new JArray(all.OfType<BlueprintFeature>()
                            .Where(value => value.name != null &&
                                value.name.IndexOf(name,
                                    StringComparison.OrdinalIgnoreCase) >= 0)
                            .OrderBy(value => value.name, StringComparer.Ordinal)
                            .Take(12).Select(value => new JObject {
                                ["name"] = value.name,
                                ["guid"] = value.AssetGuid,
                                ["graph"] = DescribeGraph(value, 0,
                                    new HashSet<object>(NativeDonorReferenceComparer.Instance), 3)
                            }).ToArray())
                    });
                document["feats"] = feats;

                document["armor"] = new JArray(all.OfType<BlueprintItemArmor>()
                    .Where(value => value.name != null &&
                        (value.name.IndexOf("ChainShirt", StringComparison.OrdinalIgnoreCase) >= 0 ||
                         value.name.IndexOf("Breastplate", StringComparison.OrdinalIgnoreCase) >= 0 ||
                         value.name.IndexOf("LeatherArmor", StringComparison.OrdinalIgnoreCase) >= 0))
                    .OrderBy(value => value.name, StringComparer.Ordinal).Take(36)
                    .Select(value => new JObject {
                        ["name"] = value.name,
                        ["guid"] = value.AssetGuid,
                        ["type"] = value.Type == null ? null : value.Type.name
                    }).ToArray());

                checks.Add(Assertion("sprint29-native-input-capture",
                    "bounded exact installed blueprint metadata captured",
                    "units=" + units.Count + ";spells=" + spells.Count +
                    ";weapons=" + weapons.Count,
                    spells.Count == spellNames.Length &&
                    weapons.Count == weaponNames.Length,
                    "Capture success does not mean a donor or faithful spell exists."));
            }
            catch (Exception error)
            {
                document["exception"] = DescribeExpandedSummoningCorrectionException(error);
                checks.Add(Assertion("sprint29-native-input-capture",
                    "bounded read-only audit completes", error.ToString(), false,
                    "Unknown is not donor or gameplay PASS."));
            }
            string path = Path.Combine(_request.EvidenceDirectory, "s29-inputs.json");
            RuntimeTestResultWriter.WriteAtomic(path, document.ToString(Formatting.Indented));
            var result = CreateResult(checks.All(value =>
                value.Status == RuntimeTestStatuses.Pass) ?
                RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                checks, null);
            result.EvidenceFiles.Add(path);
            return result;
        }
    }
}
