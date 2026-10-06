using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>Fixed research targets, not a request-supplied asset loader.</summary>
    internal static class SerpentineRigSurveyPolicy
    {
        internal static string[] Keys { get { return new[] { "medium-water-elemental", "salamander", "purple-worm" }; } }

        // Archived native donor census, October 2. This is a read-only prefab
        // comparison, never a fourth campaign actor or a summon registration.
        internal const string HybridWeaponKey = "lizardfolk-greatclub";
        internal const string HybridWeaponBlueprint = "f080877221934ea40b29e1d9fa71bc1c";
        internal const string HybridWeaponPrefab = "31cb7e484faf8734fa2c0ef1936b1806";
        internal const string HybridPrimaryWeapon = "c926ffbdccc4d124c8e8dedfe2e6f499";

        internal static bool MatchesHybridWeaponSource(string blueprint, string prefab,
            string primaryWeapon, bool hasOffhand)
        {
            return blueprint == HybridWeaponBlueprint && prefab == HybridWeaponPrefab &&
                primaryWeapon == HybridPrimaryWeapon && !hasOffhand;
        }

        internal static string NativeBlueprint(string key)
        {
            if (key == "medium-water-elemental") return "62a3e860e6e72e6499c38bb8b2fe303e";
            if (key == "salamander") return "e8276e28b2234a745900fed80670bfdb";
            if (key == "purple-worm") return "bf2216f48b3f4d24c9c502007649340d";
            throw new ArgumentException("Not a Sprint 17 native rig survey target.", "key");
        }

        internal static string Prefab(string key)
        {
            if (key == "medium-water-elemental") return "dc296683c2a3d2648afa516aeb030fb8";
            if (key == "salamander") return "9b1744531a4428e44aa9837ca984513a";
            if (key == "purple-worm") return "130f0866af3249a4e817ec7e6e9ecd89";
            throw new ArgumentException("Not a Sprint 17 native rig survey target.", "key");
        }

        internal static bool MatchesNativeSource(string key, string blueprint, string prefab)
        {
            return Array.IndexOf(Keys, key) >= 0 &&
                blueprint == NativeBlueprint(key) && prefab == Prefab(key);
        }

        // A native action may select animation indirectly and expose no clip
        // enumeration at all. Preserve that distinction from an empty list;
        // research metadata must neither throw nor infer absent behavior.
        internal static int? CountPresentClips(IEnumerable<bool> clipPresence)
        {
            return clipPresence == null ? (int?)null : clipPresence.Count(value => value);
        }

        // A navigation/actor origin is not a measured floor. Keep negative
        // clearance (penetration) as evidence; never clamp it into a PASS.
        internal static float? MeasuredGroundClearance(float vertexY, bool hit,
            float floorY, float normalY, bool ownedCollider)
        {
            if (!hit || ownedCollider || !Finite(vertexY) || !Finite(floorY) ||
                !Finite(normalY) || normalY < .2f || normalY > 1f) return null;
            float clearance = vertexY - floorY;
            return Finite(clearance) ? (float?)clearance : null;
        }

        internal static bool Finite(float value)
        { return !float.IsNaN(value) && !float.IsInfinity(value); }
    }
}
