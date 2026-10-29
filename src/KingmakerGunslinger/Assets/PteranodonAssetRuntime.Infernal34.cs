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
        private static KytonChainVisual34.Chain[] _kytonChains34;
        internal static KytonChainVisual34.Chain[] KytonChains34()
        { lock (Sync) return _kytonChains34 == null ? null : (KytonChainVisual34.Chain[])_kytonChains34.Clone(); }
        private static readonly System.Collections.Generic.Dictionary<string, Sprint13CreatureVisual> InfernalVisual34s =
            new System.Collections.Generic.Dictionary<string, Sprint13CreatureVisual> {
                { InfernalVisualPolicy34.Barbazu, new Sprint13CreatureVisual() },
                { InfernalVisualPolicy34.Kyton, new Sprint13CreatureVisual() }
            };
        internal static bool TryGetInfernalVisual34(string key, out Mesh mesh, out string[] bones,
            out Texture2D albedo, out string status)
        {
            var visual = InfernalVisual34s[key];
            lock (Sync)
            {
                mesh = visual.Mesh;
                bones = visual.Bones == null ? null : (string[])visual.Bones.Clone();
                albedo = visual.Albedo;
                status = visual.Status;
                return mesh != null && bones != null && albedo != null;
            }
        }
        private static void ConfigureInfernalVisual34s(ModContext context)
        {
            foreach (string key in InfernalVisual34s.Keys) ConfigureInfernalVisual34(context, key);
        }
        private static void ConfigureInfernalVisual34(ModContext context, string key)
        {
            if (context == null) throw new ArgumentNullException("context");
            var visual = InfernalVisual34s[key];
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
                    ("assets/sprint34-infernal-outsiders/" + key + "-mesh.json").Replace('/', Path.DirectorySeparatorChar));
                string json = File.ReadAllText(path);
                var payload = JObject.Parse(json);
                if ((string)payload["creature"] != key ||
                    !System.Linq.Enumerable.SequenceEqual(payload["bones"].Values<string>(), InfernalVisualPolicy34.Bones) ||
                    (string)payload["rigSha256"] != InfernalVisualPolicy34.RigSha256 ||
                    (string)payload["nativeDonor"] != TheropodVisualPolicy.DonorGuid ||
                    (string)payload["nativePrefab"] != TheropodVisualPolicy.DonorPrefabGuid ||
                    (string)payload["bodyRenderer"] != TheropodVisualPolicy.BodyRendererName ||
                    (string)payload["accessoryRenderer"] != TheropodVisualPolicy.AccessoryRendererName ||
                    (int?)payload["nativeRendererCount"] != 2 ||
                    (int?)payload["visibleArms"] != 2 || (int?)payload["visibleLegs"] != 2 ||
                    (bool?)payload["humanoidBody"] != true ||
                    (int?)payload["visibleChains"] != (key == InfernalVisualPolicy34.Kyton ? 4 : 0) ||
                    (bool?)payload["visibleBeard"] != (key == InfernalVisualPolicy34.Barbazu))
                    throw new InvalidDataException("Infernal outsider anatomy/donor contract");
                string[] names;
                AlbedoRequirement requirement;
                mesh = BuildMesh(json, out names, out requirement, InfernalVisualPolicy34.Bones);
                var chains = key == InfernalVisualPolicy34.Kyton
                    ? KytonChainVisual34.Chain.Read(payload["chains"], mesh, names) : null;
                string reason;
                albedo = LoadAlbedo(Path.GetDirectoryName(path), requirement, out reason);
                if (albedo == null) throw new InvalidDataException("albedo:" + reason);
                mesh.name = "KMG_" + key + "_Source";
                albedo.name = "KMG_" + key + "_Albedo";
                lock (Sync)
                {
                    if (chains != null) _kytonChains34 = chains;
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
