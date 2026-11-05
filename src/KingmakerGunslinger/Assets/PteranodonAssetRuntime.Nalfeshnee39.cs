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
        private static readonly Sprint13CreatureVisual NalfeshneeVisual39 =
            new Sprint13CreatureVisual();

        internal static bool TryGetNalfeshneeVisual39(string key, out Mesh mesh,
            out string[] bones, out Texture2D albedo, out string status)
        {
            if (!NalfeshneeVisualPolicy39.Handles(key))
            {
                mesh = null;
                bones = null;
                albedo = null;
                status = "donor-visual:unknown-sprint39-demon";
                return false;
            }
            lock (Sync)
            {
                mesh = NalfeshneeVisual39.Mesh;
                bones = NalfeshneeVisual39.Bones == null ? null :
                    (string[])NalfeshneeVisual39.Bones.Clone();
                albedo = NalfeshneeVisual39.Albedo;
                status = NalfeshneeVisual39.Status;
                return mesh != null && bones != null && albedo != null;
            }
        }

        private static void ConfigureNalfeshneeVisual39(ModContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (!context.FeatureModules.Active.ExpandedSummoning)
            {
                lock (Sync)
                    NalfeshneeVisual39.Status = "donor-visual:module-disabled";
                return;
            }
            lock (Sync)
                if (NalfeshneeVisual39.Mesh != null &&
                    NalfeshneeVisual39.Bones != null &&
                    NalfeshneeVisual39.Albedo != null) return;
            Mesh mesh = null;
            Texture2D albedo = null;
            try
            {
                string path = Path.Combine(context.ModEntry.Path,
                    ("assets/sprint39-nalfeshnee/nalfeshnee-mesh.json")
                        .Replace('/', Path.DirectorySeparatorChar));
                string json = File.ReadAllText(path);
                var payload = JObject.Parse(json);
                // The same measured free-hands capture and native animation
                // carrier the released Sprint 34-37 outsiders use. The
                // rendered mesh is this project's own; only the rig is shared.
                //
                // The anatomy assertions are what stop a donor silhouette
                // or another creature's export passing as this body. They
                // are this creature's own facts, not the Glabrezu's: two
                // visible arms rather than four, no pincers at all, two
                // wings that are explicitly recorded as NOT articulating,
                // and the boar head and corpulent build that separate this
                // silhouette from every other outsider on the shared rig.
                if ((string)payload["creature"] !=
                        NalfeshneeVisualPolicy39.Nalfeshnee ||
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
                        NalfeshneeVisualPolicy39.VisibleArms ||
                    (int?)payload["visibleLegs"] != 2 ||
                    (bool?)payload["humanoidBody"] != true ||
                    (string)payload["distinctSilhouette"] !=
                        "corpulent-boar-headed-winged-demon" ||
                    (bool?)payload["boarHead"] != true ||
                    (bool?)payload["tusked"] != true ||
                    (bool?)payload["corpulent"] != true ||
                    (int?)payload["visibleWings"] !=
                        NalfeshneeVisualPolicy39.VisibleWings ||
                    (int?)payload["visibleTusks"] != 4 ||
                    (int?)payload["articulatedArmChains"] !=
                        NalfeshneeVisualPolicy39.ArticulatedArmChains ||
                    (bool?)payload["articulatedArms"] != true ||
                    // The wings do not animate, and the export has to say so.
                    // A body that silently claimed wing articulation would be
                    // a different and unverified presentation claim.
                    (bool?)payload["articulatedWings"] != false ||
                    (bool?)payload["handClawsOnDigitChains"] != true)
                    throw new InvalidDataException(
                        "Sprint 39 nalfeshnee anatomy/donor contract");
                string[] names;
                AlbedoRequirement requirement;
                mesh = BuildMesh(json, out names, out requirement,
                    InfernalVisualPolicy36.Bones);
                string reason;
                albedo = LoadAlbedo(Path.GetDirectoryName(path), requirement,
                    out reason);
                if (albedo == null)
                    throw new InvalidDataException("albedo:" + reason);
                mesh.name = "KMG_" + NalfeshneeVisualPolicy39.Nalfeshnee +
                    "_Source";
                albedo.name = "KMG_" + NalfeshneeVisualPolicy39.Nalfeshnee +
                    "_Albedo";
                lock (Sync)
                {
                    NalfeshneeVisual39.Mesh = mesh;
                    NalfeshneeVisual39.Bones = names;
                    NalfeshneeVisual39.Albedo = albedo;
                    NalfeshneeVisual39.Status = "visual:published";
                }
            }
            catch (Exception error)
            {
                if (mesh != null) UnityEngine.Object.Destroy(mesh);
                if (albedo != null) UnityEngine.Object.Destroy(albedo);
                lock (Sync)
                {
                    NalfeshneeVisual39.Mesh = null;
                    NalfeshneeVisual39.Bones = null;
                    NalfeshneeVisual39.Albedo = null;
                    // Sprint 37 learned this distinction the hard way: a file
                    // that is absent and a file whose contents are wrong are
                    // different failures with different fixes, and one status
                    // for both hid two asset files that had never been added
                    // to the packaging scripts.
                    NalfeshneeVisual39.Status =
                        error is FileNotFoundException ||
                        error is DirectoryNotFoundException
                            ? "donor-visual:missing-mesh-file"
                            : "donor-visual:invalid-mesh-data";
                }
                context.Logger.Warning(NalfeshneeVisualPolicy39.Nalfeshnee,
                    "mesh.rejected",
                    "Sprint 39 original mesh rejected; donor remains active: " +
                    error.Message);
            }
        }
    }
}
