using System;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    // Exactly one human/Salamander anatomical seam. Storage/cape/leg frames
    // are never deduplicated into an animation or skinning driver.
    internal static class SalamanderHumanBindingPolicy
    {
        internal const string BodyName = "Renderer_Character_Diffuse_Cutout";
        internal const string AssetHash = "2c4b76f0bbf0691ae0f9d9fc2340e77f958b999e27dda2628c82b26642c16670";
        internal static bool IsReviewedAuxiliary(string renderer, string mesh, int bones)
        {
            return renderer == "Cape_Red_M(Clone)" && mesh == "CP_Cape2Sided_M_Any" && bones == 0;
        }

        internal static string[] NativeNames
        {
            get { return new[] { "Pelvis", "Spine_1", "Spine_2", "Spine_3", "Neck", "Head" }
                .Concat(new[] { "L", "R" }.SelectMany(side => new[] { "Clavicle", "Up_arm", "ForeArm",
                    "Hand", "Toe_1_01", "Toe_1_02", "Toe_2_01", "Toe_2_02", "Toe_3_01", "Toe_3_02" }
                    .Select(part => side + "_" + part))).ToArray(); }
        }
        internal static string[] Names
        { get { return NativeNames.Concat(SalamanderTailAnimationPolicy.TailNames)
            .OrderBy(name => name, StringComparer.Ordinal).ToArray(); } }

        // The mesh exporter sorts names; anatomical order and skin palette
        // order are deliberately separate. Never assume an index range.
        internal static int BindingIndex(string name)
        {
            int native = Array.IndexOf(NativeNames, name);
            if (native >= 0) return native;
            int tail = Array.IndexOf(SalamanderTailAnimationPolicy.TailNames, name);
            if (tail < 0) throw new ArgumentException("Unreviewed Salamander driver.");
            return 26 + tail;
        }

        internal static string NativeSetRejection(bool exactHumanSet, bool rawTailPresent, bool reviewedEffectiveLookup)
        {
            if (!exactHumanSet) return "not-exact-native-human-set";
            if (rawTailPresent) return "existing-native-tail-action";
            return reviewedEffectiveLookup ? null : "unreviewed-effective-tail-lookup";
        }

        internal static bool IsReviewedEffectiveTail<T>(T effective, T exactNativeSlam) where T : class
        {
            // Empty native lookup or the observed CoTW fallback to the exact
            // borrowed Slam. This does not adopt/relabel/play that Slam.
            return effective == null || (exactNativeSlam != null && ReferenceEquals(effective, exactNativeSlam));
        }

        // The installed Harmony12 bridge cannot convert a null Harmony2
        // patch record. Its registry establishes absence; never query an
        // unregistered method. A registered-method failure still propagates.
        internal static T ReadRegisteredPatchMetadata<T>(bool registered, Func<T> read) where T : class
        {
            if (!registered) return null;
            if (read == null) throw new ArgumentNullException("read");
            return read();
        }

        internal static string Parent(string name)
        {
            switch (name)
            {
                case "Pelvis": return "Position";
                case "Spine_1": return "Pelvis";
                case "Spine_2": return "Spine_1";
                case "Spine_3": return "Spine_2";
                case "Neck": return "Spine_3";
                case "Head": return "Neck";
            }
            if (!NativeNames.Contains(name, StringComparer.Ordinal)) return null;
            string side = name.Substring(0, 1);
            if (name.EndsWith("Clavicle", StringComparison.Ordinal)) return "Spine_3";
            if (name.EndsWith("Up_arm", StringComparison.Ordinal)) return side + "_Clavicle";
            if (name.EndsWith("ForeArm", StringComparison.Ordinal)) return side + "_Up_arm";
            if (name.EndsWith("Hand", StringComparison.Ordinal)) return side + "_ForeArm";
            return name.EndsWith("_01", StringComparison.Ordinal) ? side + "_Hand" :
                name.Substring(0, name.Length - 1) + "1";
        }

        // Every selected duplicate must have the SAME live Transform identity
        // and bind, not merely a matching name or first palette index.
        internal static bool TrySlots(string[] paletteNames, int[] identities, string[] parents,
            float[][] binds, out int[] slots)
        {
            slots = null;
            if (paletteNames == null || identities == null || parents == null || binds == null ||
                paletteNames.Length != identities.Length || parents.Length != identities.Length ||
                binds.Length != identities.Length || paletteNames.Length > 4096) return false;
            var result = new int[26];
            string[] names = NativeNames;
            for (int i = 0; i < names.Length; i++)
            {
                int[] candidates = Enumerable.Range(0, paletteNames.Length).Where(j => paletteNames[j] == names[i]).ToArray();
                if (candidates.Length == 0) return false;
                int first = candidates[0];
                foreach (int index in candidates)
                {
                    if (identities[index] == 0 || identities[index] != identities[first] ||
                        parents[index] != Parent(names[i]) || binds[index] == null || binds[index].Length != 16 ||
                        binds[index].Any(value => !SalamanderTailAnimationPolicy.Finite(value))) return false;
                    if (index != first && Enumerable.Range(0, 16).Any(k =>
                        Math.Abs(binds[index][k] - binds[first][k]) > .00001f)) return false;
                }
                result[i] = first;
            }
            if (result.Select(i => identities[i]).Distinct().Count() != 26) return false;
            slots = result;
            return true;
        }

        internal static T[] AppendOneTail<T>(T[] native, T tail) where T : class
        {
            if (native == null || native.Length != 24 || native.Any(value => value == null) || tail == null ||
                native.Any(value => ReferenceEquals(value, tail)) || native.Distinct().Count() != native.Length)
                throw new ArgumentException("The exact native human action list and one new Tail are required.");
            return native.Concat(new[] { tail }).ToArray();
        }
    }
}
