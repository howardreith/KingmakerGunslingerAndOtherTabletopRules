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
        private static readonly Sprint13CreatureVisual Ankylosaurus = new Sprint13CreatureVisual();

        internal static bool TryGetAnkylosaurusVisual(out Mesh mesh, out string[] names,
            out Texture2D albedo, out string status)
        {
            lock (Sync)
            {
                mesh=Ankylosaurus.Mesh; names=Ankylosaurus.Bones == null ? null : (string[])Ankylosaurus.Bones.Clone();
                albedo=Ankylosaurus.Albedo; status=Ankylosaurus.Status;
                return mesh != null && names != null && albedo != null;
            }
        }

        private static void ConfigureAnkylosaurusVisual(ModContext context)
        {
            if (!context.FeatureModules.Active.ExpandedSummoning)
            {
                lock (Sync) Ankylosaurus.Status="donor-visual:module-disabled";
                return;
            }
            lock (Sync)
                if (Ankylosaurus.Mesh != null && Ankylosaurus.Bones != null && Ankylosaurus.Albedo != null)
                { Ankylosaurus.Status="visual:published"; return; }
            string path=Path.Combine(context.ModEntry.Path,"assets","ankylosaurus","ankylosaurus-mesh.json");
            Mesh mesh=null; Texture2D albedo=null;
            try
            {
                string json=File.ReadAllText(path); var payload=JObject.Parse(json);
                if (!AnkylosaurusVisualPolicy.IsPermitted((string)payload["creature"],payload["bones"].Values<string>()) ||
                    (string)payload["nativeDonor"] != AnkylosaurusVisualPolicy.DonorGuid ||
                    (int?)payload["visibleLegs"] != 4 || (int?)payload["tailJoints"] != 6 || (bool?)payload["terminalClub"] != true)
                    throw new InvalidDataException("Ankylosaurus anatomy/bone contract");
                string[] names; AlbedoRequirement requirement;
                mesh=BuildMesh(json,out names,out requirement,AnkylosaurusVisualPolicy.Bones);
                string reason; albedo=LoadAlbedo(Path.GetDirectoryName(path),requirement,out reason);
                if (albedo == null) throw new InvalidDataException("albedo:"+reason);
                mesh.name="KMG_ankylosaurus"; albedo.name="KMG_ankylosaurus_Albedo";
                lock (Sync)
                { Ankylosaurus.Mesh=mesh; Ankylosaurus.Bones=names; Ankylosaurus.Albedo=albedo; Ankylosaurus.Status="visual:published"; }
            }
            catch (Exception error)
            {
                if (mesh != null) UnityEngine.Object.Destroy(mesh);
                if (albedo != null) UnityEngine.Object.Destroy(albedo);
                lock (Sync) { Ankylosaurus.Mesh=null; Ankylosaurus.Bones=null; Ankylosaurus.Albedo=null; Ankylosaurus.Status="donor-visual:invalid-mesh-data"; }
                context.Logger.Warning("ankylosaurus","mesh.rejected","Original mesh rejected; donor remains active: "+error.Message);
            }
        }
    }
}
