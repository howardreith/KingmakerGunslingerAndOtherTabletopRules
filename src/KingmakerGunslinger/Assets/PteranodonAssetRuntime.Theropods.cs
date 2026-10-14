using System;
using System.Collections.Generic;
using System.IO;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.Assets
{
    internal static partial class PteranodonAssetRuntime
    {
        private const string TheropodDirectory = "assets/sprint24-theropods/";
        private static readonly Dictionary<string, Sprint13CreatureVisual>
            Theropods = new Dictionary<string, Sprint13CreatureVisual>(
                StringComparer.Ordinal)
            {
                { TheropodRulesPolicy.DeinonychusKey,
                    new Sprint13CreatureVisual() },
                { TheropodRulesPolicy.TyrannosaurusKey,
                    new Sprint13CreatureVisual() }
            };

        internal static bool TryGetTheropodVisual(string key,
            out Mesh mesh, out string[] names, out Texture2D albedo,
            out string status)
        {
            lock (Sync)
            {
                Sprint13CreatureVisual visual;
                if (!Theropods.TryGetValue(key, out visual))
                {
                    mesh = null; names = null; albedo = null;
                    status = "donor-visual:unknown-theropod";
                    return false;
                }
                mesh = visual.Mesh;
                names = visual.Bones == null ? null :
                    (string[])visual.Bones.Clone();
                albedo = visual.Albedo;
                status = visual.Status;
                return mesh != null && names != null && albedo != null;
            }
        }

        private static void ConfigureTheropodVisuals(ModContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            foreach (var entry in Theropods)
            {
                string key = entry.Key;
                Sprint13CreatureVisual visual = entry.Value;
                // Module OFF performs no file read and no Unity allocation.
                if (!context.FeatureModules.Active.ExpandedSummoning)
                {
                    lock (Sync) visual.Status =
                        "donor-visual:module-disabled";
                    continue;
                }
                lock (Sync)
                    if (visual.Mesh != null && visual.Bones != null &&
                        visual.Albedo != null)
                    {
                        visual.Status = "visual:published";
                        continue;
                    }
                string path = Path.Combine(context.ModEntry.Path,
                    (TheropodDirectory + key + "-mesh.json")
                        .Replace('/', Path.DirectorySeparatorChar));
                Mesh mesh = null;
                Texture2D albedo = null;
                try
                {
                    string json = File.ReadAllText(path);
                    JObject payload = JObject.Parse(json);
                    if (!TheropodVisualPolicy.IsPermitted(
                            (string)payload["creature"],
                            payload["bones"].Values<string>()) ||
                        (string)payload["creature"] != key ||
                        (string)payload["nativeDonor"] !=
                            TheropodVisualPolicy.DonorGuid ||
                        (string)payload["nativePrefab"] !=
                            TheropodVisualPolicy.DonorPrefabGuid ||
                        (string)payload["bodyRenderer"] !=
                            TheropodVisualPolicy.BodyRendererName ||
                        (string)payload["accessoryRenderer"] !=
                            TheropodVisualPolicy.AccessoryRendererName ||
                        (int?)payload["nativeRendererCount"] != 2 ||
                        (int?)payload["visibleLegs"] != 2 ||
                        (int?)payload["visibleForelimbs"] != 2 ||
                        (bool?)payload["biped"] != true ||
                        (int?)payload["jawJoints"] != 2 ||
                        (int?)payload["tailJoints"] != 4 ||
                        (bool?)payload["balancedCountertail"] != true)
                        throw new InvalidDataException(
                            key + " anatomy/donor contract");
                    if (key == TheropodRulesPolicy.DeinonychusKey &&
                        ((int?)payload["sickleFootClaws"] != 2 ||
                         (bool?)payload["massiveSkull"] != false ||
                         (bool?)payload["tinyForearms"] != false))
                        throw new InvalidDataException(
                            "deinonychus silhouette contract");
                    if (key == TheropodRulesPolicy.TyrannosaurusKey &&
                        ((int?)payload["sickleFootClaws"] != 0 ||
                         (bool?)payload["massiveSkull"] != true ||
                         (bool?)payload["tinyForearms"] != true))
                        throw new InvalidDataException(
                            "tyrannosaurus silhouette contract");
                    string[] names;
                    AlbedoRequirement requirement;
                    mesh = BuildMesh(json, out names, out requirement,
                        TheropodVisualPolicy.Bones);
                    string reason;
                    albedo = LoadAlbedo(Path.GetDirectoryName(path),
                        requirement, out reason);
                    if (albedo == null)
                        throw new InvalidDataException("albedo:" + reason);
                    mesh.name = "KMG_" + key + "_Source";
                    albedo.name = "KMG_" + key + "_Albedo";
                    lock (Sync)
                    {
                        visual.Mesh = mesh;
                        visual.Bones = names;
                        visual.Albedo = albedo;
                        visual.Status = "visual:published";
                    }
                }
                catch (Exception error)
                {
                    if (mesh != null) UnityEngine.Object.Destroy(mesh);
                    if (albedo != null) UnityEngine.Object.Destroy(albedo);
                    lock (Sync)
                    {
                        visual.Mesh = null;
                        visual.Bones = null;
                        visual.Albedo = null;
                        visual.Status = "donor-visual:invalid-mesh-data";
                    }
                    context.Logger.Warning(key, "mesh.rejected",
                        "Original mesh rejected; donor remains active: " +
                        error.Message);
                }
            }
        }
    }
}
