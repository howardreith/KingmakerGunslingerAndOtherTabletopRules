using System;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    // Registration survives module-OFF saves. Only the active process setting
    // permits view effects; the deferred callback must not run while disabled.
    internal static class SummonVisualVariantModulePolicy
    {
        internal const string DisabledOutcome = "variant:skipped:module-disabled";
        internal static string[] Keys { get { return new[] {
            ExpandedSummoningSpecialProfiles.CheetahCoat.Key,
            ExpandedSummoningSpecialProfiles.LionVisualTint.Key,
            ExpandedSummoningSpecialProfiles.TigerCoat.Key }.Concat(
                ExpandedSummoningSpecialProfiles.MephitVisualTints.Select(p => p.Key)).ToArray(); } }

        internal static T WhenActive<T>(bool moduleActive, Func<T> effect, T disabled)
        {
            return moduleActive ? effect() : disabled;
        }
    }
}
