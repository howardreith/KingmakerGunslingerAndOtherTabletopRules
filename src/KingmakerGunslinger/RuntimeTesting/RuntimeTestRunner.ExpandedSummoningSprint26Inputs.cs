using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // One bounded missing-facts capture. No fixture, actor or save mutation.
        private RuntimeTestResult RunExpandedSummoningSprint26Inputs()
        {
            var checks = new List<RuntimeTestAssertion>();
            var rows = new JArray();
            var donors = new JArray();
            var doc = new JObject {
                ["runId"] = _request.RunId,
                ["scope"] = "Sprint26 exact creature lookup, two conditional quadruped donors, native petrification/cure/charge/sense graphs and reference pixels; metadata only, no native geometry or animation curves.",
                ["blueprints"] = rows, ["donors"] = donors
            };
            try
            {
                var all = BlueprintBootstrap.Library.GetAllBlueprints()
                    .Where(v => v != null).ToArray();
                string[] terms = { "gorgon", "bulette", "petrif", "fleshtostone",
                    "stonetoflesh", "tremorsense" };
                var selected = all.Where(v => !string.IsNullOrEmpty(v.name) &&
                    (terms.Any(t => v.name.IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    v.AssetGuid == "fa5ee5f4cd5c6394f8b497c773f8e14a" ||
                    v.name == "ChargeAbility" || v.name == "ChargeBuff" ||
                    v.name == "Pounce" || v.name == "OverrunAbility"))
                    .OrderBy(v => v.name, StringComparer.Ordinal).ToArray();
                if (selected.Length > 80) throw new InvalidOperationException(
                    "Bounded native input lookup exceeded 80 results: " + selected.Length);
                foreach (var v in selected)
                {
                    var row = new JObject { ["guid"] = v.AssetGuid,
                        ["name"] = v.name, ["type"] = v.GetType().FullName,
                        ["graph"] = DescribeGraph(v, 0, new HashSet<object>(
                            NativeDonorReferenceComparer.Instance), 8) };
                    var fact = v as BlueprintUnitFact;
                    if (fact != null && fact.Icon != null)
                        row["icon"] = CaptureSprint26ReferenceIcon(fact.Icon, v.name);
                    rows.Add(row);
                }
                var units = all.OfType<BlueprintUnit>().ToArray();
                foreach (string[] input in new[] {
                    new[] { "gorgon", "5bb9579fdb2b26b48bb10d61c81cfdfb" },
                    new[] { "bulette", "c3524f96954a1d94f8525b86e7626633" } })
                {
                    var exact = units.Where(v => v.name.IndexOf(input[0],
                        StringComparison.OrdinalIgnoreCase) >= 0 &&
                        !v.name.StartsWith("KMG_", StringComparison.Ordinal)).ToArray();
                    doc[input[0] + "ExactNativeMatches"] = new JArray(exact.Select(v =>
                        new JObject { ["guid"] = v.AssetGuid, ["name"] = v.name }));
                    // Exact species, if present, take precedence. Otherwise only
                    // the already measured Horse/Hodag candidates are inspected.
                    var candidates = exact.Length == 0 ? new[] { units.Single(v =>
                        v.AssetGuid == input[1]) } : exact.Take(2).ToArray();
                    foreach (var unit in candidates)
                    {
                        var prefab = unit.Prefab.Load(false);
                        if (prefab == null) throw new InvalidOperationException("Missing prefab: " + unit.name);
                        var row = CapturePrimateCensusViewMetadata(prefab, input[0],
                            unit.AssetGuid, unit.name, unit.Prefab.AssetId);
                        row["nativeUnit"] = DescribeNativeUnit(unit);
                        row["restHierarchy"] = CaptureSprint24RestHierarchy(prefab);
                        row["animationManagers"] = CaptureSprint24AnimationManagers(prefab);
                        row["colliders"] = CaptureSprint24Colliders(prefab);
                        donors.Add(row);
                    }
                }
                checks.Add(Assertion("sprint26-inputs-bounded-capture",
                    "native library and detached candidate metadata captured without mutation",
                    "graphs=" + rows.Count + ";donors=" + donors.Count,
                    rows.Count > 0 && donors.Count >= 2,
                    "Research only; absence of a native creature/sense remains a finding, not a product failure."));
            }
            catch (Exception e)
            {
                doc["exception"] = DescribeExpandedSummoningCorrectionException(e);
                checks.Add(Assertion("sprint26-inputs-capture", "bounded capture completes",
                    e.ToString(), false, "Unknown evidence is not native PASS."));
            }
            string path = Path.Combine(_request.EvidenceDirectory, "sprint26-native-inputs.json");
            RuntimeTestResultWriter.WriteAtomic(path, doc.ToString(Formatting.Indented));
            var result = CreateResult(checks.All(v => v.Status == RuntimeTestStatuses.Pass)
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, checks, null);
            result.EvidenceFiles.Add(path);
            return result;
        }

        private JObject CaptureSprint26ReferenceIcon(Sprite sprite, string name)
        {
            Rect rect = sprite.textureRect;
            RenderTexture prior = RenderTexture.active;
            var target = RenderTexture.GetTemporary(sprite.texture.width, sprite.texture.height, 0, RenderTextureFormat.ARGB32);
            Texture2D copy = null;
            try
            {
                Graphics.Blit(sprite.texture, target); RenderTexture.active = target;
                copy = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);
                copy.ReadPixels(rect, 0, 0); copy.Apply();
                string filename = "native-reference-" + name + ".png";
                File.WriteAllBytes(Path.Combine(_request.EvidenceDirectory, filename), EncodeExpandedSummoningPng(copy));
                return new JObject { ["file"] = filename, ["width"] = copy.width, ["height"] = copy.height };
            }
            finally
            {
                RenderTexture.active = prior; RenderTexture.ReleaseTemporary(target);
                if (copy != null) UnityEngine.Object.DestroyImmediate(copy);
            }
        }
    }
}
