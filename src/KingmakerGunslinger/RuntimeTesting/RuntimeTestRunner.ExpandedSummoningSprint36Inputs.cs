using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // The one bounded Sprint 36 installed-library/model check. It reads
        // exact metadata at mod load and never creates an actor, loads a save,
        // mutates a native blueprint, or exports proprietary assets.
        private RuntimeTestResult RunExpandedSummoningSprint36Inputs()
        {
            var checks = new List<RuntimeTestAssertion>();
            var document = new JObject {
                ["runId"] = _request.RunId,
                ["scope"] = "Sprint36 fixed Babau/Hezrou body, spell, condition and equipment-contract audit; research only"
            };
            try
            {
                BlueprintScriptableObject[] all = BlueprintBootstrap.Library
                    .GetAllBlueprints().Where(value => value != null).ToArray();
                Func<string, string> normalized = name => new string(
                    (name ?? string.Empty).Where(char.IsLetterOrDigit).ToArray());
                string[] species = { "Babau", "Hezrou" };
                BlueprintUnit[] units = all.OfType<BlueprintUnit>()
                    .Where(value => value.name != null &&
                        !value.name.StartsWith("KMG_", StringComparison.Ordinal) &&
                        species.Any(name => normalized(value.name).IndexOf(name,
                            StringComparison.OrdinalIgnoreCase) >= 0))
                    .OrderBy(value => value.name, StringComparer.Ordinal).ToArray();
                if (units.Length > 16)
                    throw new InvalidOperationException(
                        "Fixed Sprint36 exact-species audit exceeded 16 candidates.");
                var unitRows = new JArray();
                foreach (BlueprintUnit unit in units)
                {
                    var row = new JObject { ["nativeUnit"] =
                        DescribeNativeUnit(unit) };
                    if (unit.Prefab != null &&
                        !string.IsNullOrEmpty(unit.Prefab.AssetId))
                    {
                        var prefab = unit.Prefab.Load(false);
                        row["view"] = prefab == null ? null :
                            CapturePrimateCensusViewMetadata(prefab,
                                "sprint36-exact-species", unit.AssetGuid,
                                unit.name, unit.Prefab.AssetId);
                    }
                    row["graph"] = DescribeGraph(unit, 0,
                        new HashSet<object>(
                            NativeDonorReferenceComparer.Instance), 5);
                    unitRows.Add(row);
                }
                document["units"] = unitRows;
                document["speciesCounts"] = new JObject(species.Select(name =>
                    new JProperty(name, units.Count(value => normalized(
                        value.name).IndexOf(name,
                            StringComparison.OrdinalIgnoreCase) >= 0))));

                string[] consumers = { "DispelMagic", "SeeInvisibility",
                    "Darkness", "ChaosHammer", "UnholyBlight",
                    "GaseousForm", "Blasphemy", "Nauseated", "Sickened",
                    "PoisonImmunity", "Broken", "Hardness" };
                var graphs = new JArray();
                foreach (string name in consumers)
                {
                    BlueprintScriptableObject[] matches = all.Where(value =>
                        value.name != null &&
                        !value.name.StartsWith("KMG_", StringComparison.Ordinal) &&
                        value.name.IndexOf(name,
                            StringComparison.OrdinalIgnoreCase) >= 0)
                        .OrderBy(value => !string.Equals(value.name, name,
                            StringComparison.OrdinalIgnoreCase))
                        .ThenBy(value => value.name, StringComparer.Ordinal)
                        .Take(20).ToArray();
                    graphs.Add(new JObject {
                        ["query"] = name,
                        ["matches"] = new JArray(matches.Select(value =>
                            new JObject {
                                ["name"] = value.name,
                                ["guid"] = value.AssetGuid,
                                ["type"] = value.GetType().FullName,
                                ["graph"] = DescribeGraph(value, 0,
                                    new HashSet<object>(
                                        NativeDonorReferenceComparer.Instance),
                                    8)
                            }).ToArray())
                    });
                }
                document["consumers"] = graphs;

                string[] memberTerms = { "hard", "break", "broken",
                    "condition", "damage", "durab", "destroy", "ammo" };
                Type[] itemTypes = { typeof(ItemEntity),
                    typeof(ItemEntityWeapon), typeof(BlueprintItemWeapon),
                    typeof(BlueprintWeaponType) };
                document["equipmentContracts"] = new JArray(itemTypes.Select(
                    type => new JObject {
                        ["type"] = type.FullName,
                        ["members"] = new JArray(type.GetMembers(
                                BindingFlags.Instance | BindingFlags.Static |
                                BindingFlags.Public | BindingFlags.NonPublic)
                            .Where(member => memberTerms.Any(term =>
                                member.Name.IndexOf(term,
                                    StringComparison.OrdinalIgnoreCase) >= 0))
                            .OrderBy(member => member.Name,
                                StringComparer.Ordinal)
                            .Take(80).Select(member => new JObject {
                                ["name"] = member.Name,
                                ["kind"] = member.MemberType.ToString(),
                                ["declaringType"] = member.DeclaringType == null ?
                                    null : member.DeclaringType.FullName
                            }))
                    }).ToArray());

                checks.Add(Assertion("sprint36-native-input-capture",
                    "fixed installed input audit completes",
                    "units=" + unitRows.Count + ";consumerQueries=" +
                        graphs.Count + ";equipmentTypes=" + itemTypes.Length,
                    graphs.Count == consumers.Length,
                    "Absence or an unsuitable native graph remains explicit; this receipt is not gameplay proof."));
            }
            catch (Exception error)
            {
                document["exception"] =
                    DescribeExpandedSummoningCorrectionException(error);
                checks.Add(Assertion("sprint36-native-input-capture",
                    "bounded audit completes", error.ToString(), false,
                    "Unknown never establishes native suitability."));
            }
            string path = Path.Combine(_request.EvidenceDirectory,
                "sprint36-inputs.json");
            RuntimeTestResultWriter.WriteAtomic(path,
                document.ToString(Formatting.Indented));
            var result = CreateResult(checks.All(value =>
                value.Status == RuntimeTestStatuses.Pass) ?
                RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                checks, null);
            result.EvidenceFiles.Add(path);
            return result;
        }
    }
}
