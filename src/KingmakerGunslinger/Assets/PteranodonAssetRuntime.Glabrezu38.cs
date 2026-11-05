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
        private static readonly Sprint13CreatureVisual GlabrezuVisual38 =
            new Sprint13CreatureVisual();

        internal static bool TryGetGlabrezuVisual38(string key, out Mesh mesh,
            out string[] bones, out Texture2D albedo, out string status)
        {
            if (!GlabrezuVisualPolicy38.Handles(key))
            {
                mesh = null;
                bones = null;
                albedo = null;
                status = "donor-visual:unknown-sprint38-demon";
                return false;
            }
            lock (Sync)
            {
                mesh = GlabrezuVisual38.Mesh;
                bones = GlabrezuVisual38.Bones == null ? null :
                    (string[])GlabrezuVisual38.Bones.Clone();
                albedo = GlabrezuVisual38.Albedo;
                status = GlabrezuVisual38.Status;
                return mesh != null && bones != null && albedo != null;
            }
        }

        private static void ConfigureGlabrezuVisual38(ModContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (!context.FeatureModules.Active.ExpandedSummoning)
            {
                lock (Sync)
                    GlabrezuVisual38.Status = "donor-visual:module-disabled";
                return;
            }
            lock (Sync)
                if (GlabrezuVisual38.Mesh != null &&
                    GlabrezuVisual38.Bones != null &&
                    GlabrezuVisual38.Albedo != null) return;
            Mesh mesh = null;
            Texture2D albedo = null;
            try
            {
                string path = Path.Combine(context.ModEntry.Path,
                    ("assets/sprint38-glabrezu/glabrezu-mesh.json")
                        .Replace('/', Path.DirectorySeparatorChar));
                string json = File.ReadAllText(path);
                var payload = JObject.Parse(json);
                // The same measured free-hands capture and native animation
                // carrier the released Sprint 34-37 outsiders use. The
                // rendered mesh is this project's own; only the rig is shared.
                //
                // The anatomy assertions are what stop a donor silhouette or
                // another creature's export passing as this body: four visible
                // arms rather than the Vrock's two, two animated arm chains
                // carrying the pincers, and the smaller chest arms explicitly
                // recorded as not articulating.
                if ((string)payload["creature"] !=
                        GlabrezuVisualPolicy38.Glabrezu ||
                    !Enumerable.SequenceEqual(payload["bones"].Values<string>(),
                        InfernalVisualPolicy36.Bones) ||
                    !InfernalVisualPolicy36.IsReviewedRig((string)payload["rigSha256"]) ||
                    (string)payload["nativeDonor"] !=
                        TheropodVisualPolicy.DonorGuid ||
                    (string)payload["nativePrefab"] !=
                        TheropodVisualPolicy.DonorPrefabGuid ||
                    (string)payload["bodyRenderer"] !=
                        TheropodVisualPolicy.BodyRendererName ||
                    (string)payload["accessoryRenderer"] !=
                        TheropodVisualPolicy.AccessoryRendererName ||
                    (int?)payload["nativeRendererCount"] != 2 ||
                    (int?)payload["visibleArms"] !=
                        GlabrezuVisualPolicy38.VisibleArms ||
                    (int?)payload["visibleLegs"] != 2 ||
                    (bool?)payload["humanoidBody"] != true ||
                    (string)payload["distinctSilhouette"] !=
                        "four-armed-pincered-demon" ||
                    (bool?)payload["canineHead"] != true ||
                    (bool?)payload["sweptHorns"] != true ||
                    (int?)payload["visiblePincers"] != 2 ||
                    (int?)payload["visibleSmallArms"] != 2 ||
                    (int?)payload["articulatedArmChains"] !=
                        GlabrezuVisualPolicy38.ArticulatedArmChains ||
                    (bool?)payload["articulatedPincers"] != true ||
                    (bool?)payload["articulatedSmallArms"] != false ||
                    (bool?)payload["pincerJawsOnDigitChains"] != true)
                    throw new InvalidDataException(
                        "Sprint 38 glabrezu anatomy/donor contract");
                string[] names;
                AlbedoRequirement requirement;
                mesh = BuildMesh(json, out names, out requirement,
                    InfernalVisualPolicy36.Bones);
                string reason;
                albedo = LoadAlbedo(Path.GetDirectoryName(path), requirement,
                    out reason);
                if (albedo == null)
                    throw new InvalidDataException("albedo:" + reason);
                mesh.name = "KMG_" + GlabrezuVisualPolicy38.Glabrezu +
                    "_Source";
                albedo.name = "KMG_" + GlabrezuVisualPolicy38.Glabrezu +
                    "_Albedo";
                lock (Sync)
                {
                    GlabrezuVisual38.Mesh = mesh;
                    GlabrezuVisual38.Bones = names;
                    GlabrezuVisual38.Albedo = albedo;
                    GlabrezuVisual38.Status = "visual:published";
                }
            }
            catch (Exception error)
            {
                if (mesh != null) UnityEngine.Object.Destroy(mesh);
                if (albedo != null) UnityEngine.Object.Destroy(albedo);
                lock (Sync)
                {
                    GlabrezuVisual38.Mesh = null;
                    GlabrezuVisual38.Bones = null;
                    GlabrezuVisual38.Albedo = null;
                    // Sprint 37 learned this distinction the hard way: a file
                    // that is absent and a file whose contents are wrong are
                    // different failures with different fixes, and one status
                    // for both hid two asset files that had never been added
                    // to the packaging scripts.
                    GlabrezuVisual38.Status =
                        error is FileNotFoundException ||
                        error is DirectoryNotFoundException
                            ? "donor-visual:missing-mesh-file"
                            : "donor-visual:invalid-mesh-data";
                }
                context.Logger.Warning(GlabrezuVisualPolicy38.Glabrezu,
                    "mesh.rejected",
                    "Sprint 38 original mesh rejected; donor remains active: " +
                    error.Message);
            }
        }
    }
}
