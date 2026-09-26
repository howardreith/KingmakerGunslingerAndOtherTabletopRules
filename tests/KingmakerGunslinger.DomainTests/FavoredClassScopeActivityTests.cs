using System;
using System.IO;
using System.Linq;
using KingmakerGunslinger.FavoredClass;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// PR #24 second review, finding 2: an excluded, partial, unadmitted or
    /// withheld revelation target keeps its saved ranks but is mechanically
    /// inert; the same withholding guard covers the power and performance
    /// counters.
    /// </summary>
    internal static class FavoredClassScopeActivityTests
    {
        internal static void OnlyActiveRevelationScopesGiveBenefits()
        {
            Assertions.True(FavoredClassMechanicsPolicy.ScopeActive(true, null, true, true, false),
                "A published, complete, admitted target is active.");
            Assertions.False(FavoredClassMechanicsPolicy.ScopeActive(false, null, true, true, false),
                "An excluded target (Spirit of the Warrior) is inert.");
            Assertions.False(FavoredClassMechanicsPolicy.ScopeActive(true, "shared gate", true, true, false),
                "A target withheld as partial is inert.");
            Assertions.False(FavoredClassMechanicsPolicy.ScopeActive(true, null, false, true, false),
                "A target without read points is inert.");
            Assertions.False(FavoredClassMechanicsPolicy.ScopeActive(true, null, true, false, false),
                "A target not admitted to the indexes is inert.");
            Assertions.False(FavoredClassMechanicsPolicy.ScopeActive(true, null, true, true, true),
                "A withheld or unavailable target is inert.");
        }

        private static string Source(params string[] parts)
        {
            return File.ReadAllText(Path.Combine(new[] { Environment.CurrentDirectory, "src",
                "KingmakerGunslinger", "FavoredClass" }.Concat(parts).ToArray())).Replace("\r\n", "\n");
        }

        internal static void InactiveScopesAreWired()
        {
            string scopes = Source("FavoredClassRevelationScopes.cs");
            Assertions.True(scopes.Contains("FavoredClassRuntime.MechanicsEnabled ||\n                !Active)") &&
                scopes.Contains("scope.Admitted = true;") &&
                scopes.Contains("FavoredClassMechanicsPolicy.ScopeActive(Target.Published, PartialReason, HasReadPoints,") &&
                scopes.Contains("FavoredClassRuntime.IsTargetUnavailable(FavoredClassCatalog.EffectSelectedRevelation, Key)"),
                "Every revelation read point goes through the active scope's earned steps.");
            int admitted = scopes.IndexOf("scope.Admitted = true;", StringComparison.Ordinal);
            Assertions.True(admitted > scopes.IndexOf("withheld.Add(scope.Key);", StringComparison.Ordinal) &&
                admitted > scopes.IndexOf("\"excluded-target:\"", StringComparison.Ordinal),
                "Only a target past the exclusion and withholding checks is admitted.");
            string level = Source("Mechanics", "FavoredClassSelectedRevelationLevel.cs");
            Assertions.True(level.Contains("int earned = scope.EarnedSteps(Owner);") &&
                !level.Contains("GetRank()"),
                "The revelation counter reads its steps only through the scope.");
            string power = Source("Mechanics", "FavoredClassSelectedPowerLevel.cs");
            Assertions.True(power.Contains(
                "FavoredClassRuntime.IsTargetUnavailable(FavoredClassCatalog.EffectSelectedBloodlinePower, TargetKey))"),
                "A withheld bloodline power target stays native.");
            Assertions.True(Source("FavoredClassBlueprints.cs").Contains("level.TargetKey = targetKey;"),
                "The power counter knows its target.");
            Assertions.True(Source("Hooks", "FavoredClassPerformanceRangePatch.cs").Contains(
                    "FavoredClassRuntime.IsTargetUnavailable(FavoredClassCatalog.EffectPerformanceRange, key))") &&
                Source("Hooks", "FavoredClassPerformanceTextPatch.cs").Contains(
                    "FavoredClassRuntime.IsTargetUnavailable(FavoredClassCatalog.EffectPerformanceRange, key))"),
                "A withheld performance target keeps its native area and text.");
        }
    }
}
