using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    internal sealed class CrabAnimationBindingScope
    {
        internal bool ModuleActive, MissingResult, RequestedClaw, RegisteredUnit, RegisteredSource,
            ExactManagerView, LiveOriginalBody, LivePositiveSummon, ExactClawWeapons, NativeReceiptUnchanged;
        internal string UnitGuid, UnitName, PrefabGuid, SourceGuid;
    }

    internal static class CrabSummonAnimationBindingPolicy
    {
        internal const string UnitGuid = "5965d242ffe1409d98e38f2793543aad";
        internal const string UnitName = "KMG_Summoning_Unit_GiantCrab";
        internal const string PrefabGuid = "54e0335882f2dea4188214ea3c8c93de";
        internal const string SpiderGuid = "9e120b5e0ad3c794491c049aa24b9fde";
        internal const string ClawGuid = "118fdd03e569a66459ab01a20af6811a";
        internal const string NativeSet = "SpiderAnimset", NativeAction = "SpiderAnimset_Bite", NativeClip = "Spider_anim_Bite attack";
        // Exact released manifest identities. No name/prefix grants provenance.
        internal static readonly string[] SourceGuids = {
            "6937eb7feef94ed89fb8707b7d21a30c", "29a069ec978942dc8284fe5fc77b107c", "e730f3531ee3405fbe4b247c9e34a0b0",
            "c920d859d3d64c95bed453e7bc9fecec", "e1f369be10eb4c379aee17f39addb610", "1a17731029f4437c9bc58d9fd531275e", "70997728acfd4d7d8b1cc067b6dd6cae",
            "4fe86961f4aa511aa21fc4eb6359dc0f", "4a9b1a911ec65658bb21d1d3144286e8", "9fc9b6cc75235cd7a40f57475bbdfb42",
            "b2e7d201c32d5f34aee560ab13d5139c", "e87bb3e7f7af5e6f86df968f17312513", "e54114fe643d5cf9a2bc29cf9e6fce37", "0ea980ba62955480b405f4764e6df494",
            "abf21c9aa8595127b0f4b2d53d09c304", "4194476d6ede5c5d863817f7a9fa191c", "07e64d955657567c948b0ff91bdcfff3",
            "01e2225ccd0c5006b4d88df2b0f70f03", "1b998f47c7bf50869e2ea0cf53d2fc26", "9f968f630c165ce8bc6f3b5422cc99fe", "8ba6cc87266e56da8d00e6d6da5fa50c",
            "1f7438cde4d9505ebc0b54613de9783e", "99e624bc438c5d13a1fd591a1d326d1d", "196ff3285d185f1c983da1bba390c8c8", "518cdfdae85154d2894f365e25f94c52"
        };

        internal static bool Permits(CrabAnimationBindingScope scope)
        {
            return scope != null && scope.ModuleActive && scope.MissingResult && scope.RequestedClaw && scope.RegisteredUnit &&
                scope.RegisteredSource && scope.ExactManagerView && scope.LiveOriginalBody && scope.LivePositiveSummon &&
                scope.ExactClawWeapons && scope.NativeReceiptUnchanged && scope.UnitGuid == UnitGuid && scope.UnitName == UnitName &&
                scope.PrefabGuid == PrefabGuid && SourceGuids.Contains(scope.SourceGuid, StringComparer.Ordinal);
        }
    }
}
