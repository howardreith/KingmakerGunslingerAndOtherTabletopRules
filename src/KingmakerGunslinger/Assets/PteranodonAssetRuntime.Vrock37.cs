using System;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.Assets
{
    internal static partial class PteranodonAssetRuntime
    {
        private static readonly Sprint13CreatureVisual VrockVisual37 =
            new Sprint13CreatureVisual();

        internal static bool TryGetVrockVisual37(string key, out Mesh mesh,
            out string[] bones, out Texture2D albedo, out string status)
        {
            if (!VrockVisualPolicy37.Handles(key))
            {
                mesh = null;
                bones = null;
                albedo = null;
                status = "donor-visual:unknown-sprint37-demon";
                return false;
            }
            lock (Sync)
            {
                mesh = VrockVisual37.Mesh;
                bones = VrockVisual37.Bones == null ? null :
                    (string[])VrockVisual37.Bones.Clone();
                albedo = VrockVisual37.Albedo;
                status = VrockVisual37.Status;
                return mesh != null && bones != null && albedo != null;
            }
        }

        private static void ConfigureVrockVisual37(ModContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (!context.FeatureModules.Active.ExpandedSummoning)
            {
                lock (Sync)
                    VrockVisual37.Status = "donor-visual:module-disabled";
                return;
            }
            lock (Sync)
                if (VrockVisual37.Mesh != null &&
                    VrockVisual37.Bones != null &&
                    VrockVisual37.Albedo != null) return;
            Mesh mesh = null;
            Texture2D albedo = null;
            try
            {
                string path = Path.Combine(context.ModEntry.Path,
                    ("assets/sprint37-vrock/vrock-mesh.json")
                        .Replace('/', Path.DirectorySeparatorChar));
                string json = File.ReadAllText(path);
                var payload = JObject.Parse(json);
                // The same measured free-hands capture and native animation
                // carrier the released Sprint 34-36 outsiders use. The rendered
                // mesh is this project's own; only the rig is shared.
                if ((string)payload["creature"] != VrockVisualPolicy37.Vrock ||
                    !Enumerable.SequenceEqual(payload["bones"].Values<string>(),
                        InfernalVisualPolicy36.Bones) ||
                    (string)payload["rigSha256"] !=
                        InfernalVisualPolicy36.RigSha256 ||
                    (string)payload["nativeDonor"] !=
                        TheropodVisualPolicy.DonorGuid ||
                    (string)payload["nativePrefab"] !=
                        TheropodVisualPolicy.DonorPrefabGuid ||
                    (string)payload["bodyRenderer"] !=
                        TheropodVisualPolicy.BodyRendererName ||
                    (string)payload["accessoryRenderer"] !=
                        TheropodVisualPolicy.AccessoryRendererName ||
                    (int?)payload["nativeRendererCount"] != 2 ||
                    (int?)payload["visibleArms"] != 2 ||
                    (int?)payload["visibleLegs"] != 2 ||
                    (bool?)payload["humanoidBody"] != true ||
                    (string)payload["distinctSilhouette"] !=
                        "vulture-headed-winged-demon" ||
                    (bool?)payload["vultureHead"] != true ||
                    (bool?)payload["hookedBeak"] != true ||
                    (bool?)payload["rigidWings"] != true ||
                    (int?)payload["visibleWings"] != 2 ||
                    (int?)payload["visibleTalons"] != 2 ||
                    (bool?)payload["articulatedWingBones"] != false)
                    throw new InvalidDataException(
                        "Sprint 37 vrock anatomy/donor contract");
                string[] names;
                AlbedoRequirement requirement;
                mesh = BuildMesh(json, out names, out requirement,
                    InfernalVisualPolicy36.Bones);
                string reason;
                albedo = LoadAlbedo(Path.GetDirectoryName(path), requirement,
                    out reason);
                if (albedo == null)
                    throw new InvalidDataException("albedo:" + reason);
                mesh.name = "KMG_" + VrockVisualPolicy37.Vrock + "_Source";
                albedo.name = "KMG_" + VrockVisualPolicy37.Vrock + "_Albedo";
                lock (Sync)
                {
                    VrockVisual37.Mesh = mesh;
                    VrockVisual37.Bones = names;
                    VrockVisual37.Albedo = albedo;
                    VrockVisual37.Status = "visual:published";
                }
            }
            catch (Exception error)
            {
                if (mesh != null) UnityEngine.Object.Destroy(mesh);
                if (albedo != null) UnityEngine.Object.Destroy(albedo);
                lock (Sync)
                {
                    VrockVisual37.Mesh = null;
                    VrockVisual37.Bones = null;
                    VrockVisual37.Albedo = null;
                    VrockVisual37.Status = "donor-visual:invalid-mesh-data";
                }
                context.Logger.Warning(VrockVisualPolicy37.Vrock,
                    "mesh.rejected",
                    "Sprint 37 original mesh rejected; donor remains active: " +
                    error.Message);
            }
        }
    }
}
