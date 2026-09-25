using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.FavoredClass;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// PR #24 second review, finding 1: a performance's descriptions follow
    /// the actual widening outcome of the owner's live areas.
    /// </summary>
    internal static class FavoredClassRangePresentationTests
    {
        private const int Configured = 60;

        private static FavoredClassLiveRange Live(FavoredClassWideningOutcome outcome, int feet = Configured)
        {
            return new FavoredClassLiveRange(outcome, feet);
        }

        private static int? Feet(FavoredClassWideningOutcome? last, params FavoredClassLiveRange[] live)
        {
            return FavoredClassRangePresentation.Feet(live, last, Configured);
        }

        internal static void FailedOrDeferredLiveAreasKeepTheTextNative()
        {
            Assertions.True(Feet(null, Live(FavoredClassWideningOutcome.Failed)) == null,
                "A throwing or empty scale leaves the live area native, and so its text.");
            Assertions.True(Feet(FavoredClassWideningOutcome.Widened, Live(FavoredClassWideningOutcome.Deferred)) == null,
                "A live area waiting for its ring keeps the native text until it widens.");
            Assertions.True(Feet(null, Live(FavoredClassWideningOutcome.Widened), Live(FavoredClassWideningOutcome.Failed)) == null,
                "Any live area that is not widened keeps the text native.");
            Assertions.Equal(Configured, Feet(null, Live(FavoredClassWideningOutcome.Widened)).Value,
                "A widened live area shows its range.");
            Assertions.Equal(Configured, Feet(null, Live(FavoredClassWideningOutcome.WidenedRingless)).Value,
                "An intentionally ringless area widened alone shows its range.");
            Assertions.Equal(55, Feet(null, Live(FavoredClassWideningOutcome.Widened, 55)).Value,
                "The text states the range the live area actually has, not the current steps.");
            Assertions.Equal(65, Feet(null, Live(FavoredClassWideningOutcome.Widened, 55),
                Live(FavoredClassWideningOutcome.Widened, 65)).Value, "The most recent live area decides.");
        }

        internal static void WithoutALiveAreaAFailureHoldsUntilALaterSuccess()
        {
            Assertions.Equal(Configured, Feet(null).Value, "Before any widening the configured range is shown.");
            Assertions.True(Feet(FavoredClassWideningOutcome.Failed) == null,
                "After a failed widening the text stays native.");
            Assertions.Equal(Configured, Feet(FavoredClassWideningOutcome.Widened).Value,
                "A later successful widening restores the owner's range.");
            Assertions.Equal(Configured, Feet(FavoredClassWideningOutcome.WidenedRingless).Value,
                "So does a ringless widening.");
            // The last completed outcome: a deferral never replaces it.
            Assertions.True(FavoredClassRangePresentation.NextLastCompleted(FavoredClassWideningOutcome.Failed,
                FavoredClassWideningOutcome.Deferred) == FavoredClassWideningOutcome.Failed, "Deferred keeps Failed.");
            Assertions.True(FavoredClassRangePresentation.NextLastCompleted(null,
                FavoredClassWideningOutcome.Deferred) == null, "Deferred records nothing.");
            Assertions.True(FavoredClassRangePresentation.NextLastCompleted(FavoredClassWideningOutcome.Failed,
                FavoredClassWideningOutcome.Widened) == FavoredClassWideningOutcome.Widened, "Recovery replaces it.");
            Assertions.True(FavoredClassRangePresentation.NextLastCompleted(FavoredClassWideningOutcome.Widened,
                FavoredClassWideningOutcome.Failed) == FavoredClassWideningOutcome.Failed, "A failure replaces it.");
        }

        private static string Source(params string[] parts)
        {
            return File.ReadAllText(Path.Combine(new[] { Environment.CurrentDirectory, "src",
                "KingmakerGunslinger", "FavoredClass" }.Concat(parts).ToArray())).Replace("\r\n", "\n");
        }

        internal static void PresentationIsWired()
        {
            string text = Source("Hooks", "FavoredClassPerformanceTextPatch.cs");
            Assertions.True(text.Contains("int? feet = FavoredClassPerformanceInstances.Feet(owner, key,") &&
                text.Contains("return feet == null ? null :") &&
                text.Contains("FavoredClassRuntime.IsTargetUnavailable(FavoredClassCatalog.EffectPerformanceRange, key))"),
                "The owner text reads the recorded outcome of the owner's live areas.");
            string hook = Source("Hooks", "FavoredClassPerformanceRangePatch.cs");
            Assertions.True(hook.Contains("Record(__instance, context, blueprint, outcome, feet, native);") &&
                hook.Contains("Record(view, view.Context, blueprint, Widen(cylinder, ring, native, widened, true), feet, native);") &&
                hook.Contains("FavoredClassRuntime.IsTargetUnavailable(FavoredClassCatalog.EffectPerformanceRange, key))") &&
                hook.Contains("if (DeferRingsForQualification)"),
                "Every widening outcome is recorded; a withheld target stays native.");
            string instances = Source("Mechanics", "FavoredClassPerformanceInstances.cs");
            Assertions.True(instances.Contains("FavoredClassRangePresentation.NextLastCompleted(") &&
                instances.Contains("if (outcome == FavoredClassWideningOutcome.Failed)") &&
                instances.Contains("FavoredClassPerformanceRing.Restore(ring);") &&
                instances.Contains("return FavoredClassRangePresentation.Feet(live, last, configuredFeet);") &&
                instances.Contains("value.View == null || Ended(value.View)"),
                "A failure narrows the owner's other live areas of the target; ended areas are forgotten.");
        }
    }
}
