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
        private const string ArmoredDinosaurDirectory =
            "assets/sprint23-armored-dinosaurs/";
        private static readonly Dictionary<string, Sprint13CreatureVisual>
            ArmoredDinosaurs = new Dictionary<string, Sprint13CreatureVisual>(
                StringComparer.Ordinal)
            {
                { ArmoredDinosaurRulesPolicy.TriceratopsKey,
                    new Sprint13CreatureVisual() },
                { ArmoredDinosaurRulesPolicy.StegosaurusKey,
                    new Sprint13CreatureVisual() }
            };

        internal static bool TryGetArmoredDinosaurVisual(string key,
            out Mesh mesh, out string[] names, out Texture2D albedo,
            out string status)
        {
            lock (Sync)
            {
                Sprint13CreatureVisual visual;
                if (!ArmoredDinosaurs.TryGetValue(key, out visual))
                {
                    mesh = null; names = null; albedo = null;
                    status = "donor-visual:unknown-armored-dinosaur";
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

        private static void ConfigureArmoredDinosaurVisuals(
            ModContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            foreach (var entry in ArmoredDinosaurs)
            {
                string key = entry.Key;
                Sprint13CreatureVisual visual = entry.Value;
                // The module gate precedes every file read and allocation.
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
                    (ArmoredDinosaurDirectory + key + "-mesh.json")
                        .Replace('/', Path.DirectorySeparatorChar));
                Mesh mesh = null;
                Texture2D albedo = null;
                try
                {
                    string json = File.ReadAllText(path);
                    JObject payload = JObject.Parse(json);
                    string[] expected = ArmoredDinosaurVisualPolicy
                        .BonesFor(key);
                    if (!ArmoredDinosaurVisualPolicy.IsPermitted(
                            (string)payload["creature"],
                            payload["bones"].Values<string>()) ||
                        (string)payload["nativeDonor"] !=
                            ArmoredDinosaurVisualPolicy.DonorGuidFor(key) ||
                        (int?)payload["visibleLegs"] != 4 ||
                        (bool?)payload["terminalClub"] != false)
                        throw new InvalidDataException(
                            key + " anatomy/bone contract");
                    if (key == ArmoredDinosaurRulesPolicy.TriceratopsKey &&
                        ((int?)payload["horns"] != 3 ||
                         (bool?)payload["frill"] != true ||
                         (int?)payload["dorsalPlates"] != 0 ||
                         (int?)payload["tailSpikes"] != 0))
                        throw new InvalidDataException(
                            "triceratops silhouette contract");
                    if (key == ArmoredDinosaurRulesPolicy.StegosaurusKey &&
                        ((int?)payload["horns"] != 0 ||
                         (bool?)payload["frill"] != false ||
                         (int?)payload["dorsalPlates"] != 9 ||
                         (int?)payload["tailJoints"] != 6 ||
                         (int?)payload["tailSpikes"] != 4))
                        throw new InvalidDataException(
                            "stegosaurus silhouette contract");
                    string[] names;
                    AlbedoRequirement requirement;
                    mesh = BuildMesh(json, out names, out requirement,
                        expected);
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
                        visual.Status =
                            "donor-visual:invalid-mesh-data";
                    }
                    context.Logger.Warning(key, "mesh.rejected",
                        "Original mesh rejected; donor remains active: " +
                        error.Message);
                }
            }
        }
    }
}
