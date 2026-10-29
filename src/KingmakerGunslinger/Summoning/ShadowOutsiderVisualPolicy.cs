using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.View;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    // Two existing summoned identities. Only the rendered geometry/rig changes;
    // source donor, creature statistics, weapons and public routes stay separate.
    internal static class ShadowOutsiderVisualPolicy
    {
        internal const string Stalker = "invisible-stalker";
        internal const string Demon = "shadow-demon";
        internal const string RigSha256 = "727645bba689fc6623b8a690d524d751c96d9072809d4c8ce22e8ed6191ad7a4";
        internal static readonly string[] Bones = {
            "Head", "L_Arm_Lower", "L_Arm_Upper", "L_Bfinger1", "L_Bfinger2", "L_Palm", "L_finger1", "L_finger2",
            "R_Arm_Lower", "R_Arm_Upper", "R_Bfinger1", "R_Bfinger2", "R_Palm", "R_finger1", "R_finger2",
            "Torso_Lower", "Torso_Upper", "neck", "neck1"
        };
        internal static bool Handles(string key) { return key == Stalker || key == Demon; }
        internal static bool IsExactIdentity(string key, string guid, string name, string prefab)
        {
            return Handles(key) && prefab == TheropodVisualPolicy.DonorPrefabGuid &&
                guid == (key == Stalker ? "3cd7b4f2c65b4d35929dd4d969dfaa41" : "627c1841a5eb4e32b1a94c0f43ec8a60") &&
                name == (key == Stalker ? "KMG_Summoning_Unit_InvisibleStalker" : "KMG_Summoning_Unit_ShadowDemon");
        }
        internal static UnitEntityView SourceView(string key)
        {
            if (!Handles(key)) throw new ArgumentException("Unselected shadow outsider.");
            var source = BlueprintLibraryLookup.RequireExact<BlueprintUnit>(BlueprintBootstrap.Library,
                key == Stalker ? "676f8b7d0a170674cb6e504e0e30b4f0" : "1832be68f9814254dbbdab6df7fd5d0b",
                "unchanged shadow outsider source presentation");
            string expected = key == Stalker ? "bb723aa891b618a46b1e9b35888795ed" : "17326d2ed9b8e9d4e9ab20fdbb895a06";
            if (source.Prefab == null || source.Prefab.AssetId != expected)
                throw new InvalidOperationException("Source presentation identity changed.");
            var view = source.Prefab.Load(false);
            if (view == null) throw new InvalidOperationException("Source presentation unavailable.");
            return view;
        }
        internal static Material BorrowedMaterial(string key)
        {
            string name = key == Stalker ? "air_elemental:elem_body" : "Soul_Eater";
            var renderer = SourceView(key).GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .SingleOrDefault(value => value.name == name);
            if (renderer == null || renderer.sharedMaterial == null || !renderer.sharedMaterial.HasProperty("_MainTex"))
                throw new InvalidOperationException("Exact native wind/shadow material unavailable.");
            return renderer.sharedMaterial; // Caller clones; this shared native object is never written.
        }
    }
}
