using System;
using System.Collections.Generic;
using System.Globalization;
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
        internal const string PiercingDonorBlueprint = "9f7a7364b76d65d43b72086aedce68ae";
        internal const string PiercingDonorPrefab = "c664715ff7165984285f66acc764b4b3";
        internal const string PiercingDonorWeapon = "926d02c8af0352b46874791d4de9764f";
        internal static string[] Keys { get { return new[] { "viper", "constrictor-snake", "salamander" }; } }
        internal static bool IsSnake(string key) { return key == "viper" || key == "constrictor-snake"; }

        // Exact instance-delimited names for the guarded crowd's read-only
        // resource census. A neighbouring instance or native asset must not
        // be counted as this fixture's resource.
        internal static bool IsSnakeInstanceResource(string key, string meshName, string resourceName)
        {
            if (!IsSnake(key) || string.IsNullOrEmpty(meshName) || string.IsNullOrEmpty(resourceName))
                return false;
            string stem = "KMG_" + key + "_Original_";
            int instance;
            if (!meshName.StartsWith(stem, StringComparison.Ordinal) ||
                !int.TryParse(meshName.Substring(stem.Length), NumberStyles.AllowLeadingSign,
                    CultureInfo.InvariantCulture, out instance) ||
                meshName != stem + instance.ToString(CultureInfo.InvariantCulture)) return false;
            return resourceName == meshName || resourceName.StartsWith(meshName + "_", StringComparison.Ordinal) ||
                resourceName.StartsWith(meshName + " (", StringComparison.Ordinal);
        }

        // The production hook is narrower than the three-key research helper.
        // Match immutable identity, name AND prefab; never bind native worms,
        // the existing Salamander or request-local visual research carriers.
        internal const float SnakeViewMultiplier = .20f;
        internal static float ScaleSnakeBaseCorpulence(float nativeRadius)
        {
            if (nativeRadius <= 0 || float.IsNaN(nativeRadius) || float.IsInfinity(nativeRadius))
                throw new ArgumentOutOfRangeException("nativeRadius");
            float scaled = nativeRadius * SnakeViewMultiplier;
            if (scaled <= 0) throw new ArgumentOutOfRangeException("nativeRadius");
            // Scale the donor's base footprint with the original body. The
            // native getter still owns its .5m floor and live size multiplier.
            return scaled;
        }

        internal static bool TryProductionSnake(bool moduleEnabled, string guid,
            string blueprintName, string prefab, out string key)
        {
            key = null;
            if (!moduleEnabled || prefab != WormPrefab) return false;
            if (guid == "d8be82543ab64dc988c33e9f13608bad" &&
                blueprintName == "KMG_Summoning_Unit_Viper") key = "viper";
            else if (guid == "f1a2eadf588e4c3b9fb670724d706364" &&
                blueprintName == "KMG_Summoning_Unit_ConstrictorSnake") key = "constrictor-snake";
            return key != null;
        }

        internal static bool TrySnakeBiteAnimationDistance(bool moduleEnabled, string guid,
            string blueprintName, string prefab, bool originalBody, bool bite,
            float worldDistance, out float animationDistance)
        {
            animationDistance = worldDistance;
            string key;
            if (!originalBody || !bite || !TryProductionSnake(moduleEnabled, guid,
                blueprintName, prefab, out key) || worldDistance < 0 ||
                float.IsNaN(worldDistance) || float.IsInfinity(worldDistance)) return false;
            float corrected = worldDistance / SnakeViewMultiplier;
            if (float.IsInfinity(corrected)) return false;
            // Only native visual range selection uses this projection. The
            // stored handle distance, command approach and weapon reach stay
            // in world metres. Native clips/events/rigs are never rewritten.
            animationDistance = corrected;
            return true;
        }

        internal static bool IsNativeSnakeBiteAction(string actionName, IEnumerable<string> clipNames)
        {
            return actionName == "Purple_Worm_AnimationSet_Bite 1" && clipNames != null &&
                clipNames.SequenceEqual(new[] { "BiteAttack01_Short_3.5m", "BiteAttack01_Long_8m",
                    "BiteAttack02_Short_3.5m", "BiteAttack02_Long_8m" });
        }

        internal static bool PermitsOriginalWinding(string key, string marker)
        { return Keys.Contains(key, StringComparer.Ordinal) && marker == OutwardWinding; }

        internal static bool PermitsNativeSpearResearch(string key, string prefab, string weapon,
            string category, string model, string mesh, string pivot, string target)
        {
            return key == "salamander" && prefab == TwoHandPrefab && weapon == ProjectSpear &&
                category == "Spear" && model == "TH_SpearArmy" && mesh == "WP_SpearArmy" &&
                pivot == "WeaponPivot" && target == "R_Palm";
        }

        internal static bool PermitsNativePiercingAction(string key, string ownerPrefab, string weapon,
            string donorBlueprint, string donorPrefab, string donorWeapon, bool donorOffhand,
            bool piercingSupported, bool exactRig)
        { return key == "salamander" && ownerPrefab == TwoHandPrefab && weapon == ProjectSpear &&
            donorBlueprint == PiercingDonorBlueprint && donorPrefab == PiercingDonorPrefab &&
            donorWeapon == PiercingDonorWeapon && !donorOffhand && piercingSupported && exactRig; }

        // Copy exactly one reference, never mutate a shared native list or
        // discard another action. The adapter separately pins source identity.
        internal static T[] CopyWithOneNativeSpearAction<T>(T[] source, T original, T replacement) where T : class
        {
            if (source == null || source.Length == 0 || source.Length > 128 ||
                original == null || replacement == null || ReferenceEquals(original, replacement) ||
                source.Any(value => value == null) || source.Count(value => ReferenceEquals(value, original)) != 1)
                throw new ArgumentException("One exact original hand action and a distinct replacement are required.");
            T[] result = (T[])source.Clone();
            result[Array.FindIndex(source, value => ReferenceEquals(value, original))] = replacement;
            return result;
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
