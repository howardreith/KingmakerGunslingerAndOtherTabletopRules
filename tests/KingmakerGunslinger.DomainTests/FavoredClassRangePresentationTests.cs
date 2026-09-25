using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.FavoredClass;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// PR #24 reviews: a performance's descriptions follow the live areas of
    /// the owner's performance, which widen only as a whole (all widened or
    /// all native); rollbacks are verified; no outcome outlives its area.
    /// </summary>
    internal static class FavoredClassRangePresentationTests
    {
        private const int Configured = 60;

        /// <summary>A fake area: its actual state and fault injection.</summary>
        private sealed class Area
        {
            internal readonly string Name;
            internal bool Widened;
            internal bool Ended;
            internal bool RestoreFails;

            internal Area(string name)
            {
                Name = name;
            }
        }

        private sealed class Group
        {
            internal readonly FavoredClassRangeGroup<Area> Areas = new FavoredClassRangeGroup<Area>();
            internal readonly List<string> Ends = new List<string>();

            internal void Record(Area area, FavoredClassWideningOutcome outcome)
            {
                Areas.Purge(value => !value.Ended);
                area.Widened = FavoredClassRangePresentation.IsWidened(outcome);
                Areas.Record(area, outcome, Configured, value => !value.Widened, value =>
                {
                    if (value.RestoreFails)
                        throw new InvalidOperationException("injected ring restore failure");
                    value.Widened = false;
                    return true;
                }, value =>
                {
                    value.Ended = true;
                    Ends.Add(value.Name);
                });
            }

            /// <summary>The widening transaction: attempted only when the group allows it.</summary>
            internal void Cast(Area area, FavoredClassWideningOutcome attempted)
            {
                Areas.Purge(value => !value.Ended);
                Record(area, Areas.MayWiden(area) ? attempted : FavoredClassWideningOutcome.Held);
            }

            internal int? Feet()
            {
                Areas.Purge(value => !value.Ended);
                return Areas.Feet(Configured);
            }
        }

        private static FavoredClassLiveRange Live(FavoredClassWideningOutcome outcome, int feet = Configured)
        {
            return new FavoredClassLiveRange(outcome, feet);
        }

        internal static void FailedOrDeferredLiveAreasKeepTheTextNative()
        {
            Func<FavoredClassLiveRange[], int?> feet = live => FavoredClassRangePresentation.Feet(live, Configured);
            Assertions.True(feet(new[] { Live(FavoredClassWideningOutcome.Failed) }) == null, "A failed live area.");
            Assertions.True(feet(new[] { Live(FavoredClassWideningOutcome.Deferred) }) == null, "A deferred live area.");
            Assertions.True(feet(new[] { Live(FavoredClassWideningOutcome.Held) }) == null, "A held live area.");
            Assertions.True(feet(new[] { Live(FavoredClassWideningOutcome.Narrowed) }) == null, "A narrowed live area.");
            Assertions.Equal(Configured, feet(new[] { Live(FavoredClassWideningOutcome.Widened) }).Value, "Widened.");
            Assertions.Equal(Configured, feet(new[] { Live(FavoredClassWideningOutcome.WidenedRingless) }).Value,
                "Ringless.");
            Assertions.Equal(55, feet(new[] { Live(FavoredClassWideningOutcome.Widened, 55) }).Value,
                "The live area's actual range, not the current steps.");
        }

        internal static void WithoutALiveAreaTheConfiguredRangeShows()
        {
            Assertions.Equal(Configured, FavoredClassRangePresentation.Feet(new FavoredClassLiveRange[0], Configured).Value,
                "No live area: the configured range.");
            Assertions.True(FavoredClassRangePresentation.Feet(null, null) == null, "No steps and no area: native.");
            // Nothing is remembered: a failed area that ended leaves the configured range.
            var group = new Group();
            var failed = new Area("failed");
            group.Cast(failed, FavoredClassWideningOutcome.Failed);
            Assertions.True(group.Feet() == null, "Native while the failed area is live.");
            failed.Ended = true;
            Assertions.Equal(Configured, group.Feet().Value, "The configured range once it ended.");
            Assertions.Equal(0, group.Areas.Count, "The ended area is forgotten.");
        }

        // Review 3, finding 1: a success beside native live siblings.
        internal static void ASuccessBesideNativeSiblingsNeverWidensAlone()
        {
            var group = new Group();
            Area a = new Area("A"), b = new Area("B"), c = new Area("C");
            group.Cast(a, FavoredClassWideningOutcome.Widened);
            Assertions.True(a.Widened && group.Feet() == Configured, "A widens alone.");
            group.Cast(b, FavoredClassWideningOutcome.Failed);
            Assertions.True(!a.Widened && !b.Widened && group.Feet() == null, "B's failure narrows A.");
            Assertions.True(group.Areas.OutcomeOf(a) == FavoredClassWideningOutcome.Narrowed, "A is narrowed.");
            group.Cast(c, FavoredClassWideningOutcome.Widened);
            Assertions.True(!c.Widened && group.Areas.OutcomeOf(c) == FavoredClassWideningOutcome.Held,
                "While A and B are live, C is held native.");
            Assertions.True(group.Feet() == null && !a.Widened && !b.Widened, "A, B, C and the text converge on native.");
            // Even a widening recorded without asking first is rolled back.
            var d = new Area("D");
            group.Record(d, FavoredClassWideningOutcome.Widened);
            Assertions.True(!d.Widened && group.Areas.OutcomeOf(d) == FavoredClassWideningOutcome.Narrowed,
                "A widened member beside native siblings is rolled back.");
            a.Ended = b.Ended = c.Ended = d.Ended = true;
            var e = new Area("E");
            group.Cast(e, FavoredClassWideningOutcome.Widened);
            Assertions.True(e.Widened && group.Feet() == Configured, "Once they ended, the next cast widens.");
        }

        internal static void ASuccessBesideADeferredSiblingIsHeld()
        {
            var group = new Group();
            Area d = new Area("D"), c = new Area("C");
            group.Cast(d, FavoredClassWideningOutcome.Deferred);
            group.Cast(c, FavoredClassWideningOutcome.Widened);
            Assertions.True(!c.Widened && group.Areas.OutcomeOf(c) == FavoredClassWideningOutcome.Held,
                "A success beside a deferred sibling is held.");
            Assertions.True(group.Feet() == null, "The text is native.");
            // D's late ring: C is native, so D stays native too.
            group.Cast(d, FavoredClassWideningOutcome.Widened);
            Assertions.True(!d.Widened && !c.Widened && group.Feet() == null, "The group stays native as a whole.");
            // A deferral beside a widened sibling narrows it.
            var fresh = new Group();
            Area w = new Area("W"), late = new Area("late");
            fresh.Cast(w, FavoredClassWideningOutcome.Widened);
            fresh.Cast(late, FavoredClassWideningOutcome.Deferred);
            Assertions.True(!w.Widened && fresh.Feet() == null, "A deferred sibling narrows the widened one.");
        }

        // Review 3, finding 2: an unverifiable rollback ends the area.
        internal static void AnUnverifiableRollbackEndsTheArea()
        {
            var group = new Group();
            Area a = new Area("A"), b = new Area("B");
            group.Cast(a, FavoredClassWideningOutcome.Widened);
            a.RestoreFails = true;
            group.Cast(b, FavoredClassWideningOutcome.Failed);
            Assertions.True(a.Ended && group.Ends.Contains("A"), "A sibling that cannot be narrowed is ended.");
            Assertions.True(group.Areas.OutcomeOf(a) == null && group.Areas.Count == 1,
                "It is forgotten; only the failed area is live.");
            Assertions.True(group.Feet() == null && !b.Widened, "No live widened area remains beside native text.");
            // A member recorded native that is not actually native is ended too.
            var fresh = new Group();
            var liar = new Area("liar") { Widened = true };
            fresh.Areas.Record(liar, FavoredClassWideningOutcome.Failed, Configured, value => !value.Widened,
                value => true, value => { value.Ended = true; fresh.Ends.Add(value.Name); });
            Assertions.True(liar.Ended && fresh.Areas.Count == 0, "An unverified native member is ended.");
        }

        // Every sequence keeps the live areas homogeneous and the text truthful.
        internal static void LiveAreasAndTextAlwaysAgree()
        {
            var random = new Random(24);
            FavoredClassWideningOutcome[] attempts =
            {
                FavoredClassWideningOutcome.Widened, FavoredClassWideningOutcome.WidenedRingless,
                FavoredClassWideningOutcome.Failed, FavoredClassWideningOutcome.Deferred
            };
            for (int run = 0; run < 200; run++)
            {
                var group = new Group();
                var areas = new List<Area>();
                for (int step = 0; step < 12; step++)
                {
                    int action = random.Next(4);
                    if (action == 0 || areas.Count == 0)
                    {
                        var area = new Area("area" + step) { RestoreFails = random.Next(5) == 0 };
                        areas.Add(area);
                        group.Cast(area, attempts[random.Next(attempts.Length)]);
                    }
                    else if (action == 1)
                        areas[random.Next(areas.Count)].Ended = true;
                    else
                    {
                        // A late ring or a retry of an existing live area.
                        Area area = areas[random.Next(areas.Count)];
                        if (!area.Ended && !area.Widened)
                            group.Cast(area, attempts[random.Next(attempts.Length)]);
                    }
                    Area[] live = areas.Where(value => !value.Ended).ToArray();
                    int? feet = group.Feet();
                    bool allWidened = live.All(value => value.Widened);
                    bool noneWidened = live.All(value => !value.Widened);
                    Assertions.True(allWidened || noneWidened,
                        "run " + run + " step " + step + ": the live areas are all widened or all native.");
                    Assertions.True(live.Length == 0 ? feet == Configured : (feet == null) == noneWidened,
                        "run " + run + " step " + step + ": the text states the live areas' actual range.");
                }
            }
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
                text.Contains("steps > 0 ? FavoredClassPerformanceManifest.OwnerFeet(target, steps) : (int?)null);") &&
                text.Contains("return feet == null ? null :") &&
                text.Contains("FavoredClassRuntime.IsTargetUnavailable(FavoredClassCatalog.EffectPerformanceRange, key))"),
                "The owner text reads the owner's live areas, else the configured range.");
            string hook = Source("Hooks", "FavoredClassPerformanceRangePatch.cs");
            Assertions.True(hook.Contains("FavoredClassWideningOutcome outcome = MayWiden(__instance, context, blueprint)") &&
                hook.Contains(": FavoredClassWideningOutcome.Held;") &&
                hook.Contains("Record(__instance, context, blueprint, outcome, feet, native);") &&
                hook.Contains("Record(view, view.Context, blueprint, MayWiden(view, view.Context, blueprint)") &&
                hook.Contains("if (DeferRingsForQualification)"),
                "Every instance asks its group before widening and records its outcome.");
            string instances = Source("Mechanics", "FavoredClassPerformanceInstances.cs");
            Assertions.True(instances.Contains("group.Record(view, outcome, feet, VerifyNative, Narrow, End);") &&
                instances.Contains("FavoredClassPerformanceRing.Restore(ring);") &&
                instances.Contains("return VerifyNative(view);") &&
                instances.Contains("data.ForceEnd();") &&
                instances.Contains("ReferenceEquals(buff.Context, data.Context)") &&
                !instances.Contains("LastCompleted"),
                "Narrowing is verified, an unverifiable area is ended, and no outcome is remembered.");
            string ring = Source("Mechanics", "FavoredClassPerformanceRing.cs");
            Assertions.True(ring.Contains("if (!Restore(record))\n                return false;") &&
                ring.Contains("complete = false;"),
                "A ring that cannot be fully restored keeps reporting scaled.");
        }
    }
}
