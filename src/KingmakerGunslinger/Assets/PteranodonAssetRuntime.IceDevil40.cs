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
        private static readonly Sprint13CreatureVisual IceDevilVisual40 =
            new Sprint13CreatureVisual();

        internal static bool TryGetIceDevilVisual40(string key, out Mesh mesh,
            out string[] bones, out Texture2D albedo, out string status)
        {
            if (!IceDevilVisualPolicy40.Handles(key))
            {
                mesh = null;
                bones = null;
                albedo = null;
                status = "donor-visual:unknown-sprint40-ice-devil";
                return false;
            }
            lock (Sync)
            {
                mesh = IceDevilVisual40.Mesh;
                bones = IceDevilVisual40.Bones == null ? null :
                    (string[])IceDevilVisual40.Bones.Clone();
                albedo = IceDevilVisual40.Albedo;
                status = IceDevilVisual40.Status;
                return mesh != null && bones != null && albedo != null;
            }
        }

        private static void ConfigureIceDevilVisual40(ModContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (!context.FeatureModules.Active.ExpandedSummoning)
            {
                lock (Sync)
                    IceDevilVisual40.Status = "donor-visual:module-disabled";
                return;
            }
            lock (Sync)
                if (IceDevilVisual40.Mesh != null &&
                    IceDevilVisual40.Bones != null &&
                    IceDevilVisual40.Albedo != null) return;
            Mesh mesh = null;
            Texture2D albedo = null;
            try
            {
                string path = Path.Combine(context.ModEntry.Path,
                    ("assets/sprint40-ice-devil/ice-devil-mesh.json")
                        .Replace('/', Path.DirectorySeparatorChar));
                string json = File.ReadAllText(path);
                var payload = JObject.Parse(json);
                if ((string)payload["creature"] != IceDevilVisualPolicy40.Key ||
                    !Enumerable.SequenceEqual(payload["bones"].Values<string>(), InfernalVisualPolicy36.Bones) ||
                    (string)payload["rigSha256"] != InfernalVisualPolicy36.RigSha256 ||
                    (string)payload["nativeDonor"] != TheropodVisualPolicy.DonorGuid ||
                    (string)payload["nativePrefab"] != TheropodVisualPolicy.DonorPrefabGuid ||
                    (string)payload["bodyRenderer"] != TheropodVisualPolicy.BodyRendererName ||
                    (string)payload["accessoryRenderer"] != TheropodVisualPolicy.AccessoryRendererName ||
                    (int?)payload["nativeRendererCount"] != 2 ||
                    (int?)payload["visibleArms"] != 2 || (int?)payload["visibleLegs"] != 2 ||
                    (int?)payload["visibleWings"] != 0 ||
                    (string)payload["distinctSilhouette"] != "insectile-compound-eyed-segmented-gelugon" ||
                    (int?)payload["compoundEyes"] != 2 || (int?)payload["antennae"] != 2 ||
                    (int?)payload["mandibles"] != 4 || (int?)payload["tail"]?["segments"] != 7 ||
                    (bool?)payload["tailArticulatesIndependently"] != false ||
                    (bool?)payload["spearBakedIntoBody"] != false)
                    throw new InvalidDataException("Sprint40 original Gelugon anatomy/donor contract");
                string[] names;
                AlbedoRequirement requirement;
                mesh = BuildMesh(json, out names, out requirement,
                    InfernalVisualPolicy36.Bones);
                string reason;
                albedo = LoadAlbedo(Path.GetDirectoryName(path), requirement,
                    out reason);
                if (albedo == null)
                    throw new InvalidDataException("albedo:" + reason);
                mesh.name = "KMG_" + IceDevilVisualPolicy40.Key +
                    "_Source";
                albedo.name = "KMG_" + IceDevilVisualPolicy40.Key +
                    "_Albedo";
                lock (Sync)
                {
                    IceDevilVisual40.Mesh = mesh;
                    IceDevilVisual40.Bones = names;
                    IceDevilVisual40.Albedo = albedo;
                    IceDevilVisual40.Status = "visual:ready";
                }
            }
            catch (Exception error)
            {
                if (mesh != null) UnityEngine.Object.Destroy(mesh);
                if (albedo != null) UnityEngine.Object.Destroy(albedo);
                lock (Sync)
                {
                    IceDevilVisual40.Mesh = null;
                    IceDevilVisual40.Bones = null;
                    IceDevilVisual40.Albedo = null;
                    // Sprint 37 learned this distinction the hard way: a file
                    // that is absent and a file whose contents are wrong are
                    // different failures with different fixes, and one status
                    // for both hid two asset files that had never been added
                    // to the packaging scripts.
                    IceDevilVisual40.Status =
                        error is FileNotFoundException ||
                        error is DirectoryNotFoundException
                            ? "donor-visual:missing-mesh-file"
                            : "donor-visual:invalid-mesh-data";
                }
                context.Logger.Warning(IceDevilVisualPolicy40.Key,
                    "mesh.rejected",
                    "Sprint 40 original mesh rejected; donor remains active: " +
                    error.Message);
            }
        }
    }
}
