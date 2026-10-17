using System;
using System.Collections.Generic;
using System.IO;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.Assets
{
    internal static partial class PteranodonAssetRuntime
    {
        private const string GiantBodyDirectory = "assets/sprint25-giant-bodies/";
        private static readonly Dictionary<string, Sprint13CreatureVisual>
            GiantBodies = new Dictionary<string, Sprint13CreatureVisual>(
                StringComparer.Ordinal)
            {
                { GiantBodyRulesPolicy.BrachiosaurusKey,
                    new Sprint13CreatureVisual() },
                { GiantBodyRulesPolicy.ElephantKey,
                    new Sprint13CreatureVisual() }
            };

        internal static bool TryGetGiantBodyVisual(string key,
            out Mesh mesh, out string[] names, out Texture2D albedo,
            out string status)
        {
            lock (Sync)
            {
                Sprint13CreatureVisual visual;
                if (!GiantBodies.TryGetValue(key, out visual))
                {
                    mesh = null; names = null; albedo = null;
                    status = "donor-visual:unknown-giant-body";
                    return false;
                }
                mesh = visual.Mesh;
                names = visual.Bones == null ? null :
                    (string[])visual.Bones.Clone();
                albedo = visual.Albedo;
                status = visual.Status;
                return mesh != null && names != null && albedo != null;
            }
        }

        /// <summary>
        /// Loads the two Sprint 25 bodies, each refused unless its payload
        /// declares the measured donor, the single renderer that donor
        /// exposes, and the anatomy the body was authored against.
        ///
        /// <para>The Brachiosaurus payload declares
        /// <c>interimStiffNeck</c>. It is checked rather than ignored because
        /// it is the difference between the body the owner authorized for this
        /// release - its forward chain driven by <c>Head</c> alone, stiff by
        /// construction - and a body whose neck is chain-driven, which is not
        /// what the shipped weights are. A payload that claims the wrong one
        /// is rejected and the donor stays.</para>
        /// </summary>
        private static void ConfigureGiantBodyVisuals(ModContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            foreach (var entry in GiantBodies)
            {
                string key = entry.Key;
                Sprint13CreatureVisual visual = entry.Value;
                // Module OFF performs no file read and no Unity allocation.
                if (!context.FeatureModules.Active.ExpandedSummoning)
                {
                    lock (Sync) visual.Status =
                        "donor-visual:module-disabled";
                    continue;
                }
                lock (Sync)
                    if (visual.Mesh != null && visual.Bones != null &&
                        visual.Albedo != null)
                    {
                        visual.Status = "visual:published";
                        continue;
                    }
                string path = Path.Combine(context.ModEntry.Path,
                    (GiantBodyDirectory + key + "-mesh.json")
                        .Replace('/', Path.DirectorySeparatorChar));
                Mesh mesh = null;
                Texture2D albedo = null;
                try
                {
                    string json = File.ReadAllText(path);
                    JObject payload = JObject.Parse(json);
                    if (!GiantBodyVisualPolicy.IsPermitted(
                            (string)payload["creature"],
                            payload["bones"].Values<string>()) ||
                        (string)payload["creature"] != key ||
                        (string)payload["nativeDonor"] !=
                            GiantBodyVisualPolicy.DonorGuid ||
                        (string)payload["nativePrefab"] !=
                            GiantBodyVisualPolicy.DonorPrefabGuid ||
                        (string)payload["bodyRenderer"] !=
                            GiantBodyVisualPolicy.BodyRendererName ||
                        (int?)payload["nativeRendererCount"] != 1 ||
                        (bool?)payload["quadruped"] != true ||
                        (int?)payload["visibleLegs"] != 4 ||
                        (int?)payload["forwardChainJoints"] != 7 ||
                        (int?)payload["rearTailJoints"] != 5)
                        throw new InvalidDataException(
                            key + " anatomy/donor contract");
                    if (key == GiantBodyRulesPolicy.BrachiosaurusKey &&
                        ((string)payload["neckDriver"] != "Head" ||
                         (bool?)payload["neckIsChainDriven"] != false ||
                         (bool?)payload["interimStiffNeck"] != true ||
                         (int?)payload["tusks"] != 0 ||
                         (int?)payload["ears"] != 0))
                        throw new InvalidDataException(
                            "brachiosaurus body contract");
                    if (key == GiantBodyRulesPolicy.ElephantKey &&
                        ((string)payload["neckDriver"] != "chain" ||
                         (bool?)payload["neckIsChainDriven"] != true ||
                         (bool?)payload["interimStiffNeck"] != false ||
                         (int?)payload["tusks"] != 2 ||
                         (int?)payload["ears"] != 2))
                        throw new InvalidDataException(
                            "elephant body contract");
                    string[] names;
                    AlbedoRequirement requirement;
                    mesh = BuildMesh(json, out names, out requirement,
                        GiantBodyVisualPolicy.BonesFor(key));
                    string reason;
                    albedo = LoadAlbedo(Path.GetDirectoryName(path),
                        requirement, out reason);
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
                        "Original mesh rejected; donor remains active: " +
                        error.Message);
                }
            }
        }
    }
}
