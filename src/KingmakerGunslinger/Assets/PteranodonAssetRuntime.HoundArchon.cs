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
        private static readonly Sprint13CreatureVisual HoundArchonVisual = new Sprint13CreatureVisual();
        internal static bool TryGetHoundArchonVisual(out Mesh mesh, out string[] bones,
            out Texture2D albedo, out string status)
        {
            lock (Sync)
            {
                mesh = HoundArchonVisual.Mesh;
                bones = HoundArchonVisual.Bones == null ? null : (string[])HoundArchonVisual.Bones.Clone();
                albedo = HoundArchonVisual.Albedo;
                status = HoundArchonVisual.Status;
                return mesh != null && bones != null && albedo != null;
            }
        }
        private static void ConfigureHoundArchonVisual(ModContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (!context.FeatureModules.Active.ExpandedSummoning)
            {
                lock (Sync) HoundArchonVisual.Status = "donor-visual:module-disabled";
                return;
            }
            lock (Sync) if (HoundArchonVisual.Mesh != null && HoundArchonVisual.Bones != null &&
                HoundArchonVisual.Albedo != null) return;
            Mesh mesh = null;
            Texture2D albedo = null;
            try
            {
                string path = Path.Combine(context.ModEntry.Path,
                    "assets/outsider-celestial/hound-archon-mesh.json".Replace('/', Path.DirectorySeparatorChar));
                string json = File.ReadAllText(path);
                var payload = JObject.Parse(json);
                if (!HoundArchonVisualPolicy.IsPermitted((string)payload["creature"],
                        payload["bones"].Values<string>()) ||
                    (string)payload["rigSha256"] != HoundArchonVisualPolicy.RigSha256 ||
                    (string)payload["nativeDonor"] != TheropodVisualPolicy.DonorGuid ||
                    (string)payload["nativePrefab"] != TheropodVisualPolicy.DonorPrefabGuid ||
                    (string)payload["bodyRenderer"] != TheropodVisualPolicy.BodyRendererName ||
                    (string)payload["accessoryRenderer"] != TheropodVisualPolicy.AccessoryRendererName ||
                    (int?)payload["nativeRendererCount"] != 2 ||
                    (int?)payload["visibleArms"] != 2 || (int?)payload["visibleLegs"] != 2 ||
                    (bool?)payload["visibleTail"] != false ||
                    (bool?)payload["canineHead"] != true || (bool?)payload["humanoidBody"] != true ||
                    (int?)payload["jawJoints"] != 2)
                    throw new InvalidDataException("HoundArchon anatomy/donor contract");
                string[] names;
                AlbedoRequirement requirement;
                mesh = BuildMesh(json, out names, out requirement, HoundArchonVisualPolicy.Bones);
                string reason;
                albedo = LoadAlbedo(Path.GetDirectoryName(path), requirement, out reason);
                if (albedo == null) throw new InvalidDataException("albedo:" + reason);
                mesh.name = "KMG_hound-archon_Source";
                albedo.name = "KMG_hound-archon_Albedo";
                lock (Sync)
                {
                    HoundArchonVisual.Mesh = mesh;
                    HoundArchonVisual.Bones = names;
                    HoundArchonVisual.Albedo = albedo;
                    HoundArchonVisual.Status = "visual:published";
                }
            }
            catch (Exception error)
            {
                if (mesh != null) UnityEngine.Object.Destroy(mesh);
                if (albedo != null) UnityEngine.Object.Destroy(albedo);
                lock (Sync)
                {
                    HoundArchonVisual.Mesh = null;
                    HoundArchonVisual.Bones = null;
                    HoundArchonVisual.Albedo = null;
                    HoundArchonVisual.Status = "donor-visual:invalid-mesh-data";
                }
                context.Logger.Warning("hound-archon", "mesh.rejected",
                    "Original mesh rejected; donor remains active: " + error.Message);
            }
        }
    }
}
