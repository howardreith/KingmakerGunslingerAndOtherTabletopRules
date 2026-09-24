using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using KingmakerGunslinger.FavoredClass;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>O01 Oread Bard selected performance range.</summary>
    internal static class FavoredClassPerformanceTests
    {
        private static readonly Regex Guid = new Regex("^[0-9a-f]{32}$");

        private static string Source(params string[] parts)
        {
            return File.ReadAllText(Path.Combine(new[] { Environment.CurrentDirectory, "src",
                "KingmakerGunslinger", "FavoredClass" }.Concat(parts).ToArray()));
        }

        // The audited persistent performance areas, each area owned by exactly one target.
        internal static void ManifestIsTheAuditedAreaSet()
        {
            IList<FavoredClassPerformanceTarget> all = FavoredClassPerformanceManifest.All;
            Assertions.Equal(15, all.Count, "Audited performance targets.");
            Assertions.Equal(15, all.Select(target => target.Key).Distinct(StringComparer.Ordinal).Count(),
                "Unique keys.");
            foreach (FavoredClassPerformanceTarget target in all)
            {
                Assertions.True(Guid.IsMatch(target.FeatureGuid) && target.AreaGuids.Length > 0 &&
                    target.AreaGuids.All(Guid.IsMatch), target.Key + " has exact identities.");
                foreach (string area in target.AreaGuids)
                    Assertions.Equal(target.Key, FavoredClassPerformanceManifest.KeyForArea(area),
                        target.Key + " owns its area.");
            }
            Assertions.Equal(17, all.SelectMany(target => target.AreaGuids).Distinct(StringComparer.Ordinal).Count(),
                "Seventeen distinct areas (Incite Rage has three).");
            Assertions.Equal(3, FavoredClassPerformanceManifest.For("InciteRage").AreaGuids.Length,
                "Incite Rage: enemies, allies and all.");
            Assertions.Equal(5, all.Count(target => target.Provider), "Five provider performances.");
            Assertions.True(FavoredClassPerformanceManifest.KeyForArea("0000000000000000000000000000000f") == null,
                "Other areas belong to no target.");
            Assertions.True(FavoredClassPerformanceManifest.KeyForArea(null) == null, "No area, no target.");
            foreach (string excluded in new[] { "SoothingPerformance", "DeadlyPerformance", "ThunderCall",
                "DanceOfTheDead", "DiscordantVoice" })
                Assertions.False(all.Any(target => target.Key == excluded), excluded + " is an owner decision.");
            Assertions.Equal("772c83a25e2268e448e841dcd548235f", FavoredClassPerformanceManifest.BardClassGuid,
                "Kingmaker Bard.");
        }

        // Fifteen full-only leaves (divisor 1), six ranks each (+30 feet).
        internal static void PerformanceLeavesAreCappedCounters()
        {
            string effect = FavoredClassCatalog.EffectPerformanceRange;
            Assertions.True(FavoredClassLeafCatalog.IsImplemented(effect), "O01 is implemented.");
            IList<FavoredClassLeafSpec> leaves = FavoredClassLeafCatalog.LeavesFor(effect);
            Assertions.Equal(15, leaves.Count, "One leaf per performance.");
            foreach (FavoredClassLeafSpec leaf in leaves)
            {
                Assertions.Equal(FavoredClassInvestmentRole.Full, leaf.Role, leaf.Symbol + " is a full leaf.");
                Assertions.Equal(6, leaf.Ranks, leaf.Symbol + " stops at +30 feet.");
                Assertions.Equal("KMG.FavoredClass.Bard.PerformanceRange." + leaf.TargetKey + ".Full", leaf.Symbol,
                    "Performance symbol.");
                Assertions.Equal("BlueprintFeature", FavoredClassIdentityCatalog.ForSymbol(leaf.Symbol).PlannedType,
                    leaf.Symbol + " has a committed identity.");
                FavoredClassPerformanceTarget target = FavoredClassPerformanceManifest.For(leaf.TargetKey);
                Assertions.True(leaf.Description.Contains("Oread") &&
                    leaf.Description.Contains("+5 feet to the radius of " + target.Title) &&
                    leaf.Description.Contains("Each performance keeps its own separate count") &&
                    leaf.Description.Contains("limited to 6 steps") &&
                    leaf.Description.Contains("visual ring keeps its standard size"),
                    leaf.Symbol + " discloses its step, counter, cap and presentation limit.");
                Assertions.Equal(target.Provider, leaf.Description.Contains("provided by Call of the Wild"),
                    leaf.Symbol + " names its provider exactly when it has one.");
            }
            Assertions.True(FavoredClassLeafCatalog.TargetRows(effect, "InspireCourage").SequenceEqual(new[] { "O01" }),
                "Only the Oread row opens performance range.");
        }

        // Five feet per step in Kingmaker's own feet-to-meters ratio.
        internal static void RadiusFollowsTheEarnedSteps()
        {
            float courage = 50 * FavoredClassMechanicsPolicy.FeetToMeters;
            Assertions.True(Math.Abs(FavoredClassMechanicsPolicy.PerformanceRadiusMeters(courage, 0) - courage) < 1e-4f,
                "No steps, native radius.");
            Assertions.True(Math.Abs(FavoredClassMechanicsPolicy.PerformanceRadiusMeters(courage, 2) -
                60 * FavoredClassMechanicsPolicy.FeetToMeters) < 1e-4f, "Two steps: 60 feet.");
            Assertions.True(Math.Abs(FavoredClassMechanicsPolicy.PerformanceRadiusMeters(30 *
                FavoredClassMechanicsPolicy.FeetToMeters, 6) - 60 * FavoredClassMechanicsPolicy.FeetToMeters) < 1e-4f,
                "Six steps: +30 feet.");
            Assertions.True(Math.Abs(FavoredClassMechanicsPolicy.PerformanceRadiusMeters(courage, -3) - courage) < 1e-4f,
                "Negative steps never shrink the area.");
        }

        // The hook widens only the manifest area's own view instance, for its own caster.
        internal static void RangeHookIsCasterAndInstanceScoped()
        {
            string hook = Source("Hooks", "FavoredClassPerformanceRangePatch.cs");
            foreach (string token in new[]
            {
                "[HarmonyPatch(typeof(AreaEffectView), \"InitAtRuntime\")]",
                "blueprint.Shape != AreaEffectShape.Cylinder",
                "FavoredClassPerformanceManifest.KeyForArea(blueprint.AssetGuid)",
                "context.MaybeCaster",
                "FavoredClassEarnedSteps.For(caster.Descriptor, FavoredClassCatalog.EffectPerformanceRange, key)",
                "__instance.Shape as ScriptZoneCylinder",
                "FavoredClassMechanicsPolicy.PerformanceRadiusMeters(blueprint.Size.Meters, steps)"
            })
                Assertions.True(hook.Contains(token), "Range hook: " + token);
            Assertions.False(hook.Contains("blueprint.Size =") || hook.Contains("Fx"),
                "The shared area blueprint and its visual effect are never changed.");
            string blueprints = Source("FavoredClassBlueprints.cs");
            Assertions.True(blueprints.Contains("{ FavoredClassCatalog.Bard, FavoredClassPerformanceManifest.BardClassGuid }") &&
                blueprints.Contains("owned.Title = \"the performance \" + performance.Title;") &&
                blueprints.Contains("case FavoredClassCatalog.EffectPerformanceRange:"),
                "Performance leaves require the owned performance and publish into the Bard selection.");
        }
    }
}
