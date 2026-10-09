using System;
using System.Linq;
using KingmakerGunslinger.FeatureModules;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class SummonVisualVariantModuleTests
    {
        internal static void AllVariantsObeyActiveBeforeEffects()
        {
            Assertions.True(SummonVisualVariantModulePolicy.Keys.SequenceEqual(new[] {
                "cheetah", "lion", "tiger", "dust-mephit", "ice-mephit", "magma-mephit",
                "ooze-mephit", "salt-mephit", "steam-mephit" }), "Exactly the nine registered legacy variants.");
            foreach (string key in SummonVisualVariantModulePolicy.Keys)
            {
                int allocations = 0, mutations = 0;
                Func<string> attach = () => { allocations++; mutations++; return "variant:applied;key=" + key; };
                Assertions.Equal(SummonVisualVariantModulePolicy.DisabledOutcome,
                    SummonVisualVariantModulePolicy.WhenActive(false, attach, SummonVisualVariantModulePolicy.DisabledOutcome), key);
                Assertions.Equal(0, allocations, key + " must not allocate while disabled.");
                Assertions.Equal(0, mutations, key + " must not mutate a renderer while disabled.");
                Assertions.Equal("variant:applied;key=" + key,
                    SummonVisualVariantModulePolicy.WhenActive(true, attach, SummonVisualVariantModulePolicy.DisabledOutcome), key);
                Assertions.Equal(1, allocations, key + " enabled attach invoked once.");
                Assertions.Equal(1, mutations, key + " enabled renderer mutation invoked once.");
            }
            Assertions.False(SummonVisualVariantModulePolicy.Keys.Contains("air-mephit") ||
                SummonVisualVariantModulePolicy.Keys.Contains("pteranodon"), "Native and original-body controls are not variants.");
        }

        internal static void DisabledRimDoesNotResolveOrReturnCachedColour()
        {
            int lookups = 0;
            Func<int?> resolve = () => { lookups++; return 42; };
            Assertions.Equal((int?)42, SummonVisualVariantModulePolicy.WhenActive(true, resolve, (int?)null), "Enabled lookup.");
            Assertions.Equal((int?)null, SummonVisualVariantModulePolicy.WhenActive(false, resolve, (int?)null), "Disabled lookup cannot return a previous colour.");
            Assertions.Equal(1, lookups, "Disabled rim does not even resolve a registered profile.");
        }

        internal static void DisableAndReenableUseRestartSnapshot()
        {
            var running = new FeatureModuleSettingsState(FeatureModuleConfiguration.Defaults, "fixture", "fixture", false);
            running.SetPending(true,true,true,false,true,true,true,true,true,true,true,true);
            Assertions.True(running.Active.ExpandedSummoning && !running.Pending.ExpandedSummoning && running.RestartRequired,
                "Expanded Summoning is not a hot toggle; no invented live state.");
            var disabled = new FeatureModuleSettingsState(running.Pending, "fixture", "fixture", false);
            int effects = 0;
            Func<int> attach = () => ++effects;
            Assertions.Equal(0, SummonVisualVariantModulePolicy.WhenActive(disabled.Active.ExpandedSummoning, attach, 0), "Next process starts OFF.");
            disabled.SetPending(true,true,true,true,true,true,true,true,true,true,true,true);
            Assertions.False(disabled.Active.ExpandedSummoning, "Pending ON cannot mutate current OFF views.");
            var enabled = new FeatureModuleSettingsState(disabled.Pending, "fixture", "fixture", false);
            Assertions.Equal(1, SummonVisualVariantModulePolicy.WhenActive(enabled.Active.ExpandedSummoning, attach, 0), "Fresh enabled attach invokes effects once.");
            Assertions.Equal(9, SummonVisualVariantModulePolicy.Keys.Length, "Disabling effects never unregisters profiles.");
        }
    }
}
