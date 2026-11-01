using System;
using System.Collections.Generic;
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
        private static readonly Dictionary<string, Sprint13CreatureVisual>
            InfernalVisual36s =
                new Dictionary<string, Sprint13CreatureVisual> {
                    { InfernalVisualPolicy36.Babau,
                        new Sprint13CreatureVisual() },
                    { InfernalVisualPolicy36.Hezrou,
                        new Sprint13CreatureVisual() }
                };

        internal static bool TryGetInfernalVisual36(string key, out Mesh mesh,
            out string[] bones, out Texture2D albedo, out string status)
        {
            Sprint13CreatureVisual visual;
            if (!InfernalVisual36s.TryGetValue(key, out visual))
            {
                mesh = null;
                bones = null;
                albedo = null;
                status = "donor-visual:unknown-sprint36-infernal";
                return false;
            }
            lock (Sync)
            {
                mesh = visual.Mesh;
                bones = visual.Bones == null ? null :
                    (string[])visual.Bones.Clone();
                albedo = visual.Albedo;
                status = visual.Status;
                return mesh != null && bones != null && albedo != null;
            }
        }

        private static void ConfigureInfernalVisual36s(ModContext context)
        {
            foreach (string key in InfernalVisual36s.Keys)
                ConfigureInfernalVisual36(context, key);
        }

        private static void ConfigureInfernalVisual36(ModContext context,
            string key)
        {
            if (context == null) throw new ArgumentNullException("context");
            var visual = InfernalVisual36s[key];
            if (!context.FeatureModules.Active.ExpandedSummoning)
            {
                lock (Sync) visual.Status = "donor-visual:module-disabled";
                return;
            }
            lock (Sync)
                if (visual.Mesh != null && visual.Bones != null &&
                    visual.Albedo != null) return;
            Mesh mesh = null;
            Texture2D albedo = null;
            try
            {
                string path = Path.Combine(context.ModEntry.Path,
                    ("assets/sprint36-demons/" + key +
                     "-mesh.json").Replace('/', Path.DirectorySeparatorChar));
                string json = File.ReadAllText(path);
                var payload = JObject.Parse(json);
                bool babau = key == InfernalVisualPolicy36.Babau;
                if ((string)payload["creature"] != key ||
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
                    (string)payload["distinctSilhouette"] != (babau
                        ? "lean-horned-skirmisher"
                        : "broad-squat-toad-bruiser") ||
                    (bool?)payload["protectiveSlimeSurface"] != babau ||
                    (bool?)payload["toadBody"] != !babau ||
                    (int?)payload["visibleHorns"] != (babau ? 2 : 0) ||
                    (int?)payload["visibleSlimeLobes"] != (babau ? 4 : 0) ||
                    (int?)payload["visibleWarts"] != (babau ? 0 : 7))
                    throw new InvalidDataException(
                        "Sprint 36 demon anatomy/donor contract");
                string[] names;
                AlbedoRequirement requirement;
                mesh = BuildMesh(json, out names, out requirement,
                    InfernalVisualPolicy36.Bones);
                string reason;
                albedo = LoadAlbedo(Path.GetDirectoryName(path), requirement,
                    out reason);
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
                    visual.Status = "donor-visual:invalid-mesh-data";
                }
                context.Logger.Warning(key, "mesh.rejected",
                    "Sprint 36 original mesh rejected; donor remains active: " +
                    error.Message);
            }
        }
    }
}
