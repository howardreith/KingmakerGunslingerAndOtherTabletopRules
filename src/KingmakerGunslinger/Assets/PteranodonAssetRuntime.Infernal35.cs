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
        private static BoneDevilTailVisual35.Tail _boneTail35;
        private static readonly Dictionary<string, Sprint13CreatureVisual>
            InfernalVisual35s =
                new Dictionary<string, Sprint13CreatureVisual> {
                    { InfernalVisualPolicy35.Barbed,
                        new Sprint13CreatureVisual() },
                    { InfernalVisualPolicy35.Bone,
                        new Sprint13CreatureVisual() }
                };

        internal static BoneDevilTailVisual35.Tail BoneTail35()
        {
            lock (Sync)
                return _boneTail35 == null ? null : _boneTail35.Copy();
        }

        internal static bool TryGetInfernalVisual35(string key, out Mesh mesh,
            out string[] bones, out Texture2D albedo, out string status)
        {
            Sprint13CreatureVisual visual;
            if (!InfernalVisual35s.TryGetValue(key, out visual))
            {
                mesh = null;
                bones = null;
                albedo = null;
                status = "donor-visual:unknown-sprint35-infernal";
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

        private static void ConfigureInfernalVisual35s(ModContext context)
        {
            foreach (string key in InfernalVisual35s.Keys)
                ConfigureInfernalVisual35(context, key);
        }

        private static void ConfigureInfernalVisual35(ModContext context,
            string key)
        {
            if (context == null) throw new ArgumentNullException("context");
            var visual = InfernalVisual35s[key];
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
                    ("assets/sprint35-infernal-reach/" + key +
                     "-mesh.json").Replace('/', Path.DirectorySeparatorChar));
                string json = File.ReadAllText(path);
                var payload = JObject.Parse(json);
                bool bone = key == InfernalVisualPolicy35.Bone;
                if ((string)payload["creature"] != key ||
                    !Enumerable.SequenceEqual(payload["bones"].Values<string>(),
                        InfernalVisualPolicy35.Bones) ||
                    (string)payload["rigSha256"] !=
                        InfernalVisualPolicy35.RigSha256 ||
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
                    (int?)payload["visibleSpines"] != (bone ? 0 : 20) ||
                    (bool?)payload["segmentedStingingTail"] != bone ||
                    (int?)payload["visibleWings"] != (bone ? 2 : 0) ||
                    (bone ? payload["tail"] == null ||
                        payload["tail"].Type == JTokenType.Null :
                        payload["tail"] != null &&
                        payload["tail"].Type != JTokenType.Null))
                    throw new InvalidDataException(
                        "Sprint 35 infernal anatomy/donor contract");
                string[] names;
                AlbedoRequirement requirement;
                mesh = BuildMesh(json, out names, out requirement,
                    InfernalVisualPolicy35.Bones);
                var tail = bone
                    ? BoneDevilTailVisual35.Tail.Read(payload["tail"], mesh,
                        names) : null;
                string reason;
                albedo = LoadAlbedo(Path.GetDirectoryName(path), requirement,
                    out reason);
                if (albedo == null)
                    throw new InvalidDataException("albedo:" + reason);
                mesh.name = "KMG_" + key + "_Source";
                albedo.name = "KMG_" + key + "_Albedo";
                lock (Sync)
                {
                    if (tail != null) _boneTail35 = tail;
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
                    if (key == InfernalVisualPolicy35.Bone)
                        _boneTail35 = null;
                    visual.Mesh = null;
                    visual.Bones = null;
                    visual.Albedo = null;
                    visual.Status = "donor-visual:invalid-mesh-data";
                }
                context.Logger.Warning(key, "mesh.rejected",
                    "Sprint 35 original mesh rejected; donor remains active: " +
                    error.Message);
            }
        }
    }
}
