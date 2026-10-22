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
        private static readonly Sprint13CreatureVisual DretchVisual = new Sprint13CreatureVisual();
        internal static bool TryGetDretchVisual(out Mesh mesh, out string[] bones,
            out Texture2D albedo, out string status)
        {
            lock (Sync)
            {
                mesh = DretchVisual.Mesh;
                bones = DretchVisual.Bones == null ? null : (string[])DretchVisual.Bones.Clone();
                albedo = DretchVisual.Albedo;
                status = DretchVisual.Status;
                return mesh != null && bones != null && albedo != null;
            }
        }
        private static void ConfigureDretchVisual(ModContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (!context.FeatureModules.Active.ExpandedSummoning)
            {
                lock (Sync) DretchVisual.Status = "donor-visual:module-disabled";
                return;
            }
            lock (Sync) if (DretchVisual.Mesh != null && DretchVisual.Bones != null &&
                DretchVisual.Albedo != null) return;
            Mesh mesh = null;
            Texture2D albedo = null;
            try
            {
                string path = Path.Combine(context.ModEntry.Path,
                    "assets/outsider-celestial/dretch-mesh.json".Replace('/', Path.DirectorySeparatorChar));
                string json = File.ReadAllText(path);
                var payload = JObject.Parse(json);
                if (!DretchVisualPolicy.IsPermitted((string)payload["creature"],
                        payload["bones"].Values<string>()) ||
                    (string)payload["rigSha256"] != DretchVisualPolicy.RigSha256 ||
                    (string)payload["nativeDonor"] != TheropodVisualPolicy.DonorGuid ||
                    (string)payload["nativePrefab"] != TheropodVisualPolicy.DonorPrefabGuid ||
                    (string)payload["bodyRenderer"] != TheropodVisualPolicy.BodyRendererName ||
                    (string)payload["accessoryRenderer"] != TheropodVisualPolicy.AccessoryRendererName ||
                    (int?)payload["nativeRendererCount"] != 2 ||
                    (int?)payload["visibleArms"] != 2 || (int?)payload["visibleLegs"] != 2 ||
                    (bool?)payload["visibleTail"] != false ||
                    (bool?)payload["potbelly"] != true || (bool?)payload["largeEars"] != true ||
                    (int?)payload["jawJoints"] != 2)
                    throw new InvalidDataException("Dretch anatomy/donor contract");
                string[] names;
                AlbedoRequirement requirement;
                mesh = BuildMesh(json, out names, out requirement, DretchVisualPolicy.Bones);
                string reason;
                albedo = LoadAlbedo(Path.GetDirectoryName(path), requirement, out reason);
                if (albedo == null) throw new InvalidDataException("albedo:" + reason);
                mesh.name = "KMG_dretch_Source";
                albedo.name = "KMG_dretch_Albedo";
                lock (Sync)
                {
                    DretchVisual.Mesh = mesh;
                    DretchVisual.Bones = names;
                    DretchVisual.Albedo = albedo;
                    DretchVisual.Status = "visual:published";
                }
            }
            catch (Exception error)
            {
                if (mesh != null) UnityEngine.Object.Destroy(mesh);
                if (albedo != null) UnityEngine.Object.Destroy(albedo);
                lock (Sync)
                {
                    DretchVisual.Mesh = null;
                    DretchVisual.Bones = null;
                    DretchVisual.Albedo = null;
                    DretchVisual.Status = "donor-visual:invalid-mesh-data";
                }
                context.Logger.Warning("dretch", "mesh.rejected",
                    "Original mesh rejected; donor remains active: " + error.Message);
            }
        }
    }
}
