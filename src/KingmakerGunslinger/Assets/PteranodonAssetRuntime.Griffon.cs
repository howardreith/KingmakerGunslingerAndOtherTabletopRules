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
        private static readonly Sprint13CreatureVisual GriffonVisual=new Sprint13CreatureVisual();
        internal static bool TryGetGriffonVisual(out Mesh mesh,out string[] bones,out Texture2D paint,out string status)
        {
            lock(Sync){mesh=GriffonVisual.Mesh;bones=GriffonVisual.Bones==null?null:(string[])GriffonVisual.Bones.Clone();paint=GriffonVisual.Albedo;status=GriffonVisual.Status;return mesh!=null&&bones!=null&&paint!=null;}
        }
        private static void ConfigureGriffonVisual(ModContext context)
        {
            if(context==null)throw new ArgumentNullException("context");
            if(!context.FeatureModules.Active.ExpandedSummoning){lock(Sync)GriffonVisual.Status="donor-visual:module-disabled";return;}
            lock(Sync)if(GriffonVisual.Mesh!=null&&GriffonVisual.Bones!=null&&GriffonVisual.Albedo!=null)return;
            Mesh mesh=null;Texture2D paint=null;
            try{
                string path=Path.Combine(context.ModEntry.Path,"assets/sprint27-griffon/griffon-mesh.json".Replace('/',Path.DirectorySeparatorChar));
                string json=File.ReadAllText(path);var payload=JObject.Parse(json);
                if(!GriffonVisualPolicy.IsPermitted((string)payload["creature"],payload["bones"].Values<string>()) ||
                    (string)payload["nativeDonor"]!=GriffonVisualPolicy.DonorGuid || (string)payload["nativePrefab"]!=GriffonVisualPolicy.PrefabGuid ||
                    (string)payload["bodyRenderer"]!="manticoreMesh" || (int?)payload["nativeRendererCount"]!=1 || (int?)payload["nativeBoneCount"]!=57 ||
                    (int?)payload["visibleLegs"]!=4 || (int?)payload["featheredWings"]!=2 || (bool?)payload["eagleHead"]!=true || (bool?)payload["felineHindquarters"]!=true)
                    throw new InvalidDataException("Griffon anatomy/donor contract");
                string[] names;AlbedoRequirement requirement;
                mesh=BuildMesh(json,out names,out requirement,GriffonVisualPolicy.Bones);
                string reason;paint=LoadAlbedo(Path.GetDirectoryName(path),requirement,out reason);
                if(paint==null)throw new InvalidDataException("albedo:"+reason);
                mesh.name="KMG_griffon_Source";paint.name="KMG_griffon_Albedo";
                lock(Sync){GriffonVisual.Mesh=mesh;GriffonVisual.Bones=names;GriffonVisual.Albedo=paint;GriffonVisual.Status="visual:published";}
            }catch(Exception e){
                if(mesh!=null)UnityEngine.Object.Destroy(mesh);if(paint!=null)UnityEngine.Object.Destroy(paint);
                lock(Sync){GriffonVisual.Mesh=null;GriffonVisual.Bones=null;GriffonVisual.Albedo=null;GriffonVisual.Status="donor-visual:invalid-mesh-data";}
                context.Logger.Warning("griffon","mesh.rejected","Original mesh rejected; donor remains active: "+e.Message);
            }
        }
    }
}
