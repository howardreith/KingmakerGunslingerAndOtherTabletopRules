using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.BrownFur
{
    /// <summary>
    /// Exact installed identities for the optional combined Arcanist
    /// archetypes. Brown-Fur remains after CotW's standalone archetypes and
    /// before this compatibility-owned mixed block.
    /// </summary>
    internal static class BrownFurArchetypeOrdering
    {
        private static readonly string[] CombinedArchetypeGuids =
        {
            "0579e8ed3ded006b2ef40c7fc5ed226c",
            "1a84628d8fcd0c6a2a89a9c9c24b52a5",
            "56155d681d350f5a2658a83237171f14",
            "841a65dccb4a08e03360c76b4d6980cd",
            "b46e7ee9cbf002370c1e64b9daf9e3f2"
        };

        private static readonly HashSet<string> CombinedArchetypeGuidSet =
            new HashSet<string>(CombinedArchetypeGuids,
                StringComparer.Ordinal);

        internal static IReadOnlyList<string> KnownCombinedArchetypeGuids
        { get { return CombinedArchetypeGuids.ToArray(); } }

        internal static bool IsKnownCombinedArchetype(string assetGuid)
        {
            return !string.IsNullOrWhiteSpace(assetGuid) &&
                CombinedArchetypeGuidSet.Contains(assetGuid);
        }
    }
}
