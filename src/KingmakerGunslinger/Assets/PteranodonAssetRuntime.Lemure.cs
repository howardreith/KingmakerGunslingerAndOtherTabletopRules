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
        private static readonly Sprint13CreatureVisual LemureVisual = new Sprint13CreatureVisual();
        internal static bool TryGetLemureVisual(out Mesh mesh, out string[] bones,
            out Texture2D albedo, out string status)
        {
            lock (Sync)
            {
                mesh = LemureVisual.Mesh;
                bones = LemureVisual.Bones == null ? null : (string[])LemureVisual.Bones.Clone();
                albedo = LemureVisual.Albedo;
                status = LemureVisual.Status;
                return mesh != null && bones != null && albedo != null;
            }
        }
        private static void ConfigureLemureVisual(ModContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (!context.FeatureModules.Active.ExpandedSummoning)
            {
                lock (Sync) LemureVisual.Status = "donor-visual:module-disabled";
                return;
            }
            lock (Sync) if (LemureVisual.Mesh != null && LemureVisual.Bones != null &&
                LemureVisual.Albedo != null) return;
            Mesh mesh = null;
            Texture2D albedo = null;
            try
            {
                string path = Path.Combine(context.ModEntry.Path,
                    "assets/outsider-celestial/lemure-mesh.json".Replace('/', Path.DirectorySeparatorChar));
                string json = File.ReadAllText(path);
                var payload = JObject.Parse(json);
                if (!LemureVisualPolicy.IsPermitted((string)payload["creature"],
                        payload["bones"].Values<string>()) ||
                    (string)payload["rigSha256"] != LemureVisualPolicy.RigSha256 ||
                    (string)payload["nativeDonor"] != TheropodVisualPolicy.DonorGuid ||
                    (string)payload["nativePrefab"] != TheropodVisualPolicy.DonorPrefabGuid ||
                    (string)payload["bodyRenderer"] != TheropodVisualPolicy.BodyRendererName ||
                    (string)payload["accessoryRenderer"] != TheropodVisualPolicy.AccessoryRendererName ||
                    (int?)payload["nativeRendererCount"] != 2 ||
                    (int?)payload["visibleArms"] != 2 || (int?)payload["visibleLegs"] != 0 ||
                    (bool?)payload["visibleTail"] != false ||
                    (bool?)payload["amorphousLowerBody"] != true)
                    throw new InvalidDataException("Lemure anatomy/donor contract");
                string[] names;
                AlbedoRequirement requirement;
                mesh = BuildMesh(json, out names, out requirement, LemureVisualPolicy.Bones);
                string reason;
                albedo = LoadAlbedo(Path.GetDirectoryName(path), requirement, out reason);
                if (albedo == null) throw new InvalidDataException("albedo:" + reason);
                mesh.name = "KMG_lemure_Source";
                albedo.name = "KMG_lemure_Albedo";
                lock (Sync)
                {
                    LemureVisual.Mesh = mesh;
                    LemureVisual.Bones = names;
                    LemureVisual.Albedo = albedo;
                    LemureVisual.Status = "visual:published";
                }
            }
            catch (Exception error)
            {
                if (mesh != null) UnityEngine.Object.Destroy(mesh);
                if (albedo != null) UnityEngine.Object.Destroy(albedo);
                lock (Sync)
                {
                    LemureVisual.Mesh = null;
                    LemureVisual.Bones = null;
                    LemureVisual.Albedo = null;
                    LemureVisual.Status = "donor-visual:invalid-mesh-data";
                }
                context.Logger.Warning("lemure", "mesh.rejected",
                    "Original mesh rejected; donor remains active: " + error.Message);
            }
        }
    }
}
