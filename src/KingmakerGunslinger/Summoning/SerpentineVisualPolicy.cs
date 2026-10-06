using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>Closed Sprint 17 original-body binding contract. No native
    /// transforms, meshes, textures or animation curves are distributed.</summary>
    internal static class SerpentineVisualPolicy
    {
        internal const string WormPrefab = "130f0866af3249a4e817ec7e6e9ecd89";
        internal const string ClubShieldPrefab = "9b1744531a4428e44aa9837ca984513a";
        internal const string TwoHandPrefab = "31cb7e484faf8734fa2c0ef1936b1806";
        internal const string HybridSupport = "KMG_SalamanderSupport";
        internal const string OutwardWinding = "authored-outward-sprint17";
        internal const string ProjectSpear = "99394d453c6f425f84d4b92f7a8deea0";
        internal static string[] Keys { get { return new[] { "viper", "constrictor-snake", "salamander" }; } }
        internal static bool IsSnake(string key) { return key == "viper" || key == "constrictor-snake"; }

        internal static bool PermitsOriginalWinding(string key, string marker)
        { return Keys.Contains(key, StringComparer.Ordinal) && marker == OutwardWinding; }

        internal static bool PermitsNativeSpearResearch(string key, string prefab, string weapon,
            string category, string model, string mesh, string pivot, string target)
        {
            return key == "salamander" && prefab == TwoHandPrefab && weapon == ProjectSpear &&
                category == "Spear" && model == "TH_SpearArmy" && mesh == "WP_SpearArmy" &&
                pivot == "WeaponPivot" && target == "R_Palm";
        }

        /// <summary>Two existing palms must fit inside the unscaled shaft.
        /// The rear hand is ten percent from the butt; the forward hand has
        /// at least ten percent tip clearance. No target/reach input.</summary>
        internal static bool TrySpearRearGrip(float shaftLength, float handSpacing, out float fromButt)
        {
            fromButt = 0;
            if (float.IsNaN(shaftLength) || float.IsInfinity(shaftLength) || shaftLength <= 0 ||
                float.IsNaN(handSpacing) || float.IsInfinity(handSpacing) ||
                handSpacing < shaftLength * .04f || handSpacing > shaftLength * .8f) return false;
            fromButt = shaftLength * .1f;
            return fromButt > 0 && !float.IsInfinity(fromButt);
        }

        internal static string[] Bones(string key)
        {
            if (IsSnake(key))
                return new[] { "Hips_Joints" }.Concat(Enumerable.Range(2, 13)
                    .Select(index => "Body0" + index)).Concat(new[] { "Head", "Jaw_Down" }).ToArray();
            if (key != "salamander") return new string[0];
            return new[] { "Torso_Lower", "Torso_Upper", "neck", "neck1", "Head", "jaw", "jaw1",
                "tail", "tail1", "tail2", "tail3" }.Concat(new[] { "L", "R" }.SelectMany(side =>
                    new[] { "clavicle", "Arm_Upper", "Arm_Lower", "Palm", "finger1", "finger2",
                        "Bfinger1", "Bfinger2" }.Select(part => side + "_" + part)))
                .Concat(new[] { HybridSupport }).ToArray();
        }

        /// <summary>Only this original hybrid support uses the renderer frame.
        /// All other slots preserve the exact native bone AND native bindpose.
        /// -1 is an explicit renderer slot, never an unknown-bone fallback.</summary>
        internal static bool TryResolveDriverSlots(string key, string[] originalNames,
            string[] nativeNames, out int[] slots)
        {
            slots = null;
            if (!PermitsBones(key, originalNames) || nativeNames == null ||
                nativeNames.Length != (IsSnake(key) ? 40 : 39) ||
                nativeNames.Any(string.IsNullOrEmpty) || nativeNames.Contains(HybridSupport) ||
                nativeNames.Distinct(StringComparer.Ordinal).Count() != nativeNames.Length) return false;
            var found = new int[originalNames.Length];
            for (int i = 0; i < originalNames.Length; i++)
            {
                if (key == "salamander" && originalNames[i] == HybridSupport) found[i] = -1;
                else
                {
                    found[i] = Array.IndexOf(nativeNames, originalNames[i]);
                    if (found[i] < 0) return false;
                }
            }
            slots = found;
            return true;
        }

        internal static string BodyRenderer(string key)
        { return IsSnake(key) ? "Purple_Worm" : key == "salamander" ? "_lizardman001" : null; }

        internal static string AuxiliaryRenderer(string key)
        { return IsSnake(key) ? "Purple_Worm_Stones" : key == "salamander" ? "_ammunition04" : null; }

        internal static bool ExactSet(IEnumerable<string> actual, IEnumerable<string> expected)
        {
            if (actual == null || expected == null) return false;
            string[] rows = actual.ToArray(), wanted = expected.ToArray();
            return rows.Length == wanted.Length && !rows.Any(string.IsNullOrEmpty) &&
                rows.Distinct(StringComparer.Ordinal).Count() == rows.Length &&
                new HashSet<string>(rows, StringComparer.Ordinal).SetEquals(wanted);
        }

        internal static bool PermitsBones(string key, IEnumerable<string> actual)
        { return Keys.Contains(key, StringComparer.Ordinal) && ExactSet(actual, Bones(key)); }

        internal static bool PermitsDonor(string key, string prefab, IEnumerable<string> skinNames,
            int bodyBoneCount, int auxiliaryBoneCount, string rootBone, IEnumerable<string> staticMeshes)
        {
            if (!Keys.Contains(key, StringComparer.Ordinal) || !ExactSet(skinNames,
                new[] { BodyRenderer(key), AuxiliaryRenderer(key) })) return false;
            if (IsSnake(key))
                return prefab == WormPrefab && bodyBoneCount == 40 && auxiliaryBoneCount == 40 &&
                    rootBone == "Hips_Joints" && ExactSet(staticMeshes, new string[0]);
            if (bodyBoneCount != 39 || auxiliaryBoneCount != 19 || rootBone != "Torso_Lower") return false;
            // Identical measured frames, but neither two-hand grip nor weapon
            // contact is inferred from accepting this original BODY binding.
            return prefab == ClubShieldPrefab ? ExactSet(staticMeshes,
                new[] { "WP_ShieldLightDamaged", "lizardman_club" }) : prefab == TwoHandPrefab &&
                ExactSet(staticMeshes, new[] { "lizardman_club" });
        }
    }
}
