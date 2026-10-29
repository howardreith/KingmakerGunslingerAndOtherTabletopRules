using System;
using System.IO;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.Assets
{
    internal static partial class PteranodonAssetRuntime
    {
        private static readonly System.Collections.Generic.Dictionary<string, Sprint13CreatureVisual> ShadowOutsiderVisuals =
            new System.Collections.Generic.Dictionary<string, Sprint13CreatureVisual> {
                { ShadowOutsiderVisualPolicy.Stalker, new Sprint13CreatureVisual() },
                { ShadowOutsiderVisualPolicy.Demon, new Sprint13CreatureVisual() }
            };
        internal static bool TryGetShadowOutsiderVisual(string key, out Mesh mesh, out string[] bones,
            out Texture2D albedo, out string status)
        {
            var visual = ShadowOutsiderVisuals[key];
            lock (Sync)
            {
                mesh = visual.Mesh;
                bones = visual.Bones == null ? null : (string[])visual.Bones.Clone();
                albedo = visual.Albedo;
                status = visual.Status;
                return mesh != null && bones != null && albedo != null;
            }
        }
        private static void ConfigureShadowOutsiderVisuals(ModContext context)
        {
            foreach (string key in ShadowOutsiderVisuals.Keys) ConfigureShadowOutsiderVisual(context, key);
        }
        private static void ConfigureShadowOutsiderVisual(ModContext context, string key)
        {
            if (context == null) throw new ArgumentNullException("context");
            var visual = ShadowOutsiderVisuals[key];
            if (!context.FeatureModules.Active.ExpandedSummoning)
            {
                lock (Sync) visual.Status = "donor-visual:module-disabled";
                return;
            }
            lock (Sync) if (visual.Mesh != null && visual.Bones != null &&
                visual.Albedo != null) return;
            Mesh mesh = null;
            Texture2D albedo = null;
            try
            {
                string path = Path.Combine(context.ModEntry.Path,
                    ("assets/sprint33-shadow-outsiders/" + key + "-mesh.json").Replace('/', Path.DirectorySeparatorChar));
                string json = File.ReadAllText(path);
                var payload = JObject.Parse(json);
                if ((string)payload["creature"] != key ||
                    !System.Linq.Enumerable.SequenceEqual(payload["bones"].Values<string>(), ShadowOutsiderVisualPolicy.Bones) ||
                    (string)payload["rigSha256"] != ShadowOutsiderVisualPolicy.RigSha256 ||
                    (string)payload["nativeDonor"] != TheropodVisualPolicy.DonorGuid ||
                    (string)payload["nativePrefab"] != TheropodVisualPolicy.DonorPrefabGuid ||
                    (string)payload["bodyRenderer"] != TheropodVisualPolicy.BodyRendererName ||
                    (string)payload["accessoryRenderer"] != TheropodVisualPolicy.AccessoryRendererName ||
                    (int?)payload["nativeRendererCount"] != 2 ||
                    (int?)payload["visibleArms"] != 2 || (int?)payload["visibleLegs"] != 0 ||
                    (bool?)payload["humanoidBody"] != true ||
                    (int?)payload["visibleWings"] != (key == ShadowOutsiderVisualPolicy.Demon ? 2 : 0))
                    throw new InvalidDataException("Shadow outsider anatomy/donor contract");
                string[] names;
                AlbedoRequirement requirement;
                mesh = BuildMesh(json, out names, out requirement, ShadowOutsiderVisualPolicy.Bones);
                string reason;
                albedo = LoadAlbedo(Path.GetDirectoryName(path), requirement, out reason);
                if (albedo == null) throw new InvalidDataException("albedo:" + reason);
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
                    "Original mesh rejected; donor remains active: " + error.Message);
            }
        }
    }
}
