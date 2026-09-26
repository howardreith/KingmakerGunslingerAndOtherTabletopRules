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
            internal bool EndThrows;
            internal bool EndNoOp;
            internal bool DataUnavailable;
            internal bool LivenessThrows;

            internal Area(string name)
            {
                Name = name;
            }
        }

        /// <summary>
        /// The group as FavoredClassPerformanceInstances drives it: unavailable
        /// area data counts as live and cannot be ended; a widening attempt
        /// and a recording are mechanics points (Settle), the text a read-only
        /// point (Purge).
        /// </summary>
        private sealed class Group
        {
            internal readonly FavoredClassRangeGroup<Area> Areas;
            internal readonly List<string> Ends = new List<string>();
            internal readonly List<string> Diagnostics = new List<string>();

            internal Group()
            {
                Areas = new FavoredClassRangeGroup<Area>(value =>
                {
                    if (value.LivenessThrows)
                        throw new InvalidOperationException("injected liveness read failure");
                    return value.DataUnavailable || !value.Ended;
                }, value => !value.Widened, value =>
                {
                    if (value.RestoreFails)
                        throw new InvalidOperationException("injected ring restore failure");
                    value.Widened = false;
                    return true;
                }, value =>
                {
                    if (value.DataUnavailable)
                        throw new InvalidOperationException("the area's data is unavailable, so it cannot be ended");
                    if (value.EndThrows)
                        throw new InvalidOperationException("injected ending failure");
                    if (value.EndNoOp)
                        return;
                    value.Ended = true;
                    Ends.Add(value.Name);
                }, (value, message) => Diagnostics.Add(value.Name + ": " + message));
            }

            internal void Record(Area area, FavoredClassWideningOutcome outcome)
            {
                area.Widened = FavoredClassRangePresentation.IsWidened(outcome);
                Areas.Record(area, outcome, Configured);
            }

            /// <summary>The widening transaction: attempted only when the group allows it.</summary>
            internal void Cast(Area area, FavoredClassWideningOutcome attempted)
            {
                Areas.Settle();
                Record(area, Areas.MayWiden(area) ? attempted : FavoredClassWideningOutcome.Held);
            }

            internal int? Feet()
            {
                Areas.Purge();
                return Areas.Feet(Configured);
            }

            internal bool Reported(string area, string prefix)
            {
                return Diagnostics.Any(value => value.StartsWith(area + ": " + prefix, StringComparison.Ordinal));
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
            fresh.Areas.Record(liar, FavoredClassWideningOutcome.Failed, Configured);
            Assertions.True(liar.Ended && fresh.Areas.Count == 0, "An unverified native member is ended.");
        }

        // Fourth review, finding 2: an ending that throws leaves the area
        // tracked and unresolved; a later mechanics point retries it.
        internal static void AnEndThatThrowsKeepsTheAreaTracked()
        {
            var group = new Group();
            Area a = new Area("A"), b = new Area("B");
            group.Cast(a, FavoredClassWideningOutcome.Widened);
            a.RestoreFails = true;
            a.EndThrows = true;
            group.Cast(b, FavoredClassWideningOutcome.Failed);
            Assertions.True(!a.Ended && a.Widened, "A cannot be narrowed or ended: it is still live and widened.");
            Assertions.True(group.Areas.Contains(a) && group.Areas.ProblemOf(a) != null &&
                group.Areas.ProblemOf(a).Contains("ending it threw"), "A stays tracked, unresolved, with the reason.");
            Assertions.True(group.Reported("A", "unresolved: its narrowing was not verified native"),
                "A diagnostic names the unresolved area.");
            Assertions.True(group.Feet() == null, "The text is native while A is unresolved.");
            var c = new Area("C");
            group.Cast(c, FavoredClassWideningOutcome.Widened);
            Assertions.True(!c.Widened && group.Areas.OutcomeOf(c) == FavoredClassWideningOutcome.Held,
                "Widening is blocked while A is unresolved.");
            // A read-only point never narrows or ends.
            a.RestoreFails = false;
            a.EndThrows = false;
            Assertions.True(group.Feet() == null && a.Widened && group.Areas.ProblemOf(a) != null,
                "The text point retried nothing.");
            // The next mechanics point retries and now narrows A.
            group.Areas.Settle();
            Assertions.True(!a.Widened && !a.Ended && group.Areas.OutcomeOf(a) == FavoredClassWideningOutcome.Narrowed &&
                group.Areas.ProblemOf(a) == null, "The retry narrowed A and verified it native.");
            Assertions.True(group.Reported("A", "resolved"), "The resolution is reported.");
            Assertions.Equal(0, group.Areas.UnresolvedCount, "Nothing is unresolved.");
            b.Ended = c.Ended = a.Ended = true;
            Assertions.Equal(Configured, group.Feet().Value, "Once every area ended, the configured range.");
        }

        // Fourth review, finding 2: an ending that does nothing is not an ending.
        internal static void AnEndThatDoesNothingKeepsTheAreaTracked()
        {
            var group = new Group();
            Area a = new Area("A"), b = new Area("B");
            group.Cast(a, FavoredClassWideningOutcome.Widened);
            a.RestoreFails = true;
            a.EndNoOp = true;
            group.Cast(b, FavoredClassWideningOutcome.Failed);
            Assertions.True(!a.Ended && group.Areas.Contains(a) && group.Areas.ProblemOf(a) != null &&
                group.Areas.ProblemOf(a).Contains("still live after ending"),
                "A no-op ending keeps A tracked and unresolved.");
            Assertions.True(group.Feet() == null && !group.Areas.MayWiden(new Area("X")),
                "Native text, no widening.");
            // The retry: narrowing still fails, the ending now works and is verified.
            a.EndNoOp = false;
            var d = new Area("D");
            group.Cast(d, FavoredClassWideningOutcome.Widened);
            Assertions.True(a.Ended && !group.Areas.Contains(a) && group.Ends.Contains("A"),
                "The next widening attempt retried the ending, verified it and forgot A.");
            Assertions.True(group.Reported("A", "resolved: ended"), "The resolution is reported.");
            Assertions.True(!d.Widened && group.Areas.OutcomeOf(d) == FavoredClassWideningOutcome.Held,
                "D was held: B is a live native area.");
        }

        // Fourth review, finding 2: without area data nothing can be ended,
        // and the area counts as live.
        internal static void UnavailableDataKeepsTheAreaTracked()
        {
            var group = new Group();
            Area a = new Area("A"), b = new Area("B");
            group.Cast(a, FavoredClassWideningOutcome.Widened);
            a.RestoreFails = true;
            a.DataUnavailable = true;
            group.Cast(b, FavoredClassWideningOutcome.Failed);
            Assertions.True(!a.Ended && group.Areas.Contains(a) && group.Areas.ProblemOf(a) != null &&
                group.Areas.ProblemOf(a).Contains("data is unavailable"),
                "Unavailable data keeps A tracked and unresolved.");
            // Even if A really ended meanwhile, it is never forgotten unverified.
            a.Ended = true;
            Assertions.True(group.Feet() == null && group.Areas.Contains(a),
                "An area whose data is unavailable is never forgotten.");
            // The data returns: the read-only point verifies the ending.
            a.DataUnavailable = false;
            group.Feet();
            Assertions.True(!group.Areas.Contains(a) && group.Reported("A", "resolved"),
                "Verified no longer live, A is forgotten and resolved.");
            // A live area whose data returns is narrowed by the retry.
            var fresh = new Group();
            Area e = new Area("E"), f = new Area("F");
            fresh.Cast(e, FavoredClassWideningOutcome.Widened);
            e.RestoreFails = true;
            e.DataUnavailable = true;
            fresh.Cast(f, FavoredClassWideningOutcome.Failed);
            e.RestoreFails = false;
            e.DataUnavailable = false;
            fresh.Areas.Settle();
            Assertions.True(!e.Widened && fresh.Areas.OutcomeOf(e) == FavoredClassWideningOutcome.Narrowed &&
                fresh.Areas.UnresolvedCount == 0, "The retry narrowed E once its data returned.");
        }

        // Fourth review, finding 2: a liveness read that throws forgets nothing.
        internal static void ALivenessReadThatThrowsForgetsNothing()
        {
            var group = new Group();
            var a = new Area("A");
            group.Cast(a, FavoredClassWideningOutcome.Widened);
            a.LivenessThrows = true;
            Assertions.True(group.Feet() == Configured && group.Areas.Contains(a),
                "A stays tracked; its recorded widening still decides the text.");
            Assertions.True(group.Areas.ProblemOf(a) != null && group.Areas.ProblemOf(a).Contains("liveness"),
                "A is unresolved: its liveness cannot be read.");
            Assertions.True(group.Reported("A", "unresolved: its liveness probe threw"), "A diagnostic is reported.");
            Assertions.True(!group.Areas.MayWiden(new Area("X")), "Widening is blocked meanwhile.");
            a.LivenessThrows = false;
            Assertions.True(group.Feet() == Configured && a.Widened && group.Areas.ProblemOf(a) == null,
                "Readable again, A is kept as it was: never narrowed or forgotten by the read.");
            Assertions.True(group.Areas.MayWiden(new Area("Y")) && group.Reported("A", "resolved"),
                "Widening is allowed again and the resolution reported.");
            // Ended while unreadable: forgotten only once the read works.
            a.LivenessThrows = true;
            a.Ended = true;
            group.Feet();
            Assertions.True(group.Areas.Contains(a), "An unreadable member is never forgotten.");
            a.LivenessThrows = false;
            Assertions.True(group.Feet() == Configured && !group.Areas.Contains(a),
                "Verified ended, it is forgotten and the configured range shows.");
            // A new area beside an unreadable widened one is held and narrows it.
            var fresh = new Group();
            Area w = new Area("W"), n = new Area("N");
            fresh.Cast(w, FavoredClassWideningOutcome.Widened);
            w.LivenessThrows = true;
            fresh.Cast(n, FavoredClassWideningOutcome.Widened);
            Assertions.True(!n.Widened && fresh.Areas.OutcomeOf(n) == FavoredClassWideningOutcome.Held &&
                !w.Widened && fresh.Areas.OutcomeOf(w) == FavoredClassWideningOutcome.Narrowed &&
                fresh.Feet() == null, "Held beside an unresolved sibling, which the held area narrows.");
        }

        /// <summary>A fake mechanics context: the engine clones a buff's context for its area.</summary>
        private sealed class Context
        {
            internal readonly Context Parent;

            internal Context(Context parent)
            {
                Parent = parent;
            }

            /// <summary>MechanicsContext.CloneFor: a new context whose parent is this one.</summary>
            internal Context CloneFor()
            {
                return new Context(this);
            }
        }

        // Review 3, finding 2: the toggle turned off is the one whose own
        // current buff runs the unverifiable area. The area runs under a clone
        // of that buff's context (AreaEffectsController.Spawn), so its own
        // context is never the buff's.
        internal static void TheRunningBuffIsAnAncestorOfItsArea()
        {
            Func<Context, Context> parent = value => value.Parent;
            // ActivatableAbility.TryStart: the toggle's context, its applied buff, the buff's area.
            Context buff = new Context(null).CloneFor();
            Context area = buff.CloneFor();
            Assertions.True(!ReferenceEquals(area, buff), "The area's own context is never its buff's.");
            Assertions.True(FavoredClassContextLineage.Descends(area, buff, parent),
                "The toggle's current buff is found among the area's ancestors.");
            Context applied = buff.CloneFor();
            Assertions.True(FavoredClassContextLineage.Descends(applied.CloneFor(), buff, parent),
                "An area run by a buff the performance applied belongs to the performance.");
            // Lingering Performance: the older buff outlives the toggle's reference to it.
            Context lingering = new Context(null).CloneFor();
            Context lingeringArea = lingering.CloneFor();
            Assertions.True(!FavoredClassContextLineage.Descends(lingeringArea, buff, parent),
                "A lingering older area never stops the current performance.");
            Assertions.True(!FavoredClassContextLineage.Descends(area, lingering, parent),
                "The current area is not the lingering buff's.");
            Assertions.True(FavoredClassContextLineage.Descends(buff, buff, parent), "A context descends from itself.");
            Assertions.True(!FavoredClassContextLineage.Descends(area, null, parent), "No buff: nothing is turned off.");
            Assertions.True(!FavoredClassContextLineage.Descends(null, buff, parent), "No area context: nothing matches.");
            Assertions.True(!FavoredClassContextLineage.Descends(area, lingering, value => value),
                "A malformed cyclic chain ends without a match.");
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

        // Fourth review, finding 2: under every fault (a failed narrowing, an
        // ending that throws or does nothing, unavailable area data, a
        // liveness read that throws, and their recovery) no live area is ever
        // untracked, a widened live area beside native or configured text is
        // always tracked as unresolved, widened text means every live area is
        // widened, and an unresolved member blocks widening.
        internal static void NoUntrackedWidenedAreaBesideNativeText()
        {
            var random = new Random(4);
            FavoredClassWideningOutcome[] attempts =
            {
                FavoredClassWideningOutcome.Widened, FavoredClassWideningOutcome.WidenedRingless,
                FavoredClassWideningOutcome.Failed, FavoredClassWideningOutcome.Deferred
            };
            int unresolvedSeen = 0, resolvedSeen = 0;
            for (int run = 0; run < 400; run++)
            {
                var group = new Group();
                var areas = new List<Area>();
                for (int step = 0; step < 16; step++)
                {
                    int action = random.Next(6);
                    if (action == 0 || areas.Count == 0)
                    {
                        var area = new Area("area" + step)
                        {
                            RestoreFails = random.Next(4) == 0,
                            EndThrows = random.Next(6) == 0,
                            EndNoOp = random.Next(6) == 0,
                            DataUnavailable = random.Next(8) == 0
                        };
                        areas.Add(area);
                        group.Cast(area, attempts[random.Next(attempts.Length)]);
                    }
                    else if (action == 1)
                        areas[random.Next(areas.Count)].Ended = true;
                    else if (action == 2)
                    {
                        Area area = areas[random.Next(areas.Count)];
                        area.LivenessThrows = !area.LivenessThrows;
                    }
                    else if (action == 3)
                    {
                        // A fault clears: the next mechanics point may resolve it.
                        Area area = areas[random.Next(areas.Count)];
                        area.RestoreFails = area.EndThrows = area.EndNoOp = area.DataUnavailable = false;
                    }
                    else if (action == 4)
                        group.Areas.Settle();
                    else
                    {
                        Area area = areas[random.Next(areas.Count)];
                        if (!area.Ended && !area.Widened)
                            group.Cast(area, attempts[random.Next(attempts.Length)]);
                    }
                    string at = "run " + run + " step " + step + ": ";
                    Area[] live = areas.Where(value => !value.Ended).ToArray();
                    int? feet = group.Feet();
                    foreach (Area area in live)
                        Assertions.True(group.Areas.Contains(area), at + "the live " + area.Name + " is untracked.");
                    foreach (Area area in live.Where(value => value.Widened))
                        Assertions.True(feet == Configured || group.Areas.ProblemOf(area) != null,
                            at + "the widened " + area.Name + " is live beside native text without being unresolved.");
                    if (feet == Configured && live.Length > 0)
                        Assertions.True(live.All(value => value.Widened), at + "the widened text beside a native area.");
                    if (group.Areas.UnresolvedCount > 0)
                    {
                        unresolvedSeen++;
                        Assertions.True(!group.Areas.MayWiden(new Area("probe")), at + "an unresolved member allowed widening.");
                    }
                    if (group.Diagnostics.Any(value => value.Contains(": resolved")))
                        resolvedSeen++;
                }
            }
            Assertions.True(unresolvedSeen > 0 && resolvedSeen > 0,
                "The sequences reached unresolved members and resolved them (" + unresolvedSeen + ", " + resolvedSeen + ").");
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
            // Fourth review, finding 2: an ending is verified by the liveness
            // read; unavailable data counts as live and cannot be ended; a
            // failed liveness read never skips a recording; the text reads
            // without narrowing or ending, a widening attempt or recording
            // retries.
            Assertions.True(instances.Contains("new FavoredClassRangeGroup<AreaEffectView>(IsLive, VerifyNative, Narrow,") &&
                instances.Contains("End, Diagnose);") &&
                instances.Contains("group.Record(view, outcome, feet);") &&
                instances.Contains("throw new InvalidOperationException(\"the area's data is unavailable, so it cannot be ended\");") &&
                instances.Contains("return data == null || (!data.Destroyed && !data.IsEnded);") &&
                instances.Contains("catch (Exception)\n                {\n                    live = true;\n                }") &&
                instances.Contains("context.Logger.Warning(\"favored-class\""),
                "Instances are ended only verifiably, never forgotten on a failed read, and diagnosed.");
            int mayWiden = instances.IndexOf("internal static bool MayWiden(", StringComparison.Ordinal);
            int record = instances.IndexOf("internal static void Record(", StringComparison.Ordinal);
            int feet = instances.IndexOf("internal static int? Feet(", StringComparison.Ordinal);
            Assertions.True(instances.IndexOf("Maintain(true);", mayWiden, StringComparison.Ordinal) < record &&
                instances.IndexOf("Maintain(true);", record, StringComparison.Ordinal) < feet &&
                instances.IndexOf("Maintain(false);", feet, StringComparison.Ordinal) > feet &&
                instances.IndexOf("Maintain(true);", feet, StringComparison.Ordinal) < 0,
                "Widening attempts and recordings retry unresolved instances; the text and evidence only read.");
            Assertions.True(
                !Source("FavoredClassRangeGroup.cs").Contains("// The member is forgotten either way."),
                "No path forgets a member whose ending failed.");
            Assertions.True(instances.Contains("group.Record(view, outcome, feet);") &&
                instances.Contains("FavoredClassPerformanceRing.Restore(ring);") &&
                instances.Contains("return VerifyNative(view);") &&
                instances.Contains("data.ForceEnd();") &&
                instances.Contains("FavoredClassContextLineage.Descends(context, buff.Context,") &&
                instances.Contains("value => value.ParentContext))") &&
                !instances.Contains("ReferenceEquals(buff.Context, data.Context)") &&
                !instances.Contains("LastCompleted"),
                "Narrowing is verified, an unverifiable area is ended with the toggle whose buff runs it, and no outcome is remembered.");
            string ring = Source("Mechanics", "FavoredClassPerformanceRing.cs");
            Assertions.True(ring.Contains("if (!Restore(record))\n                return false;") &&
                ring.Contains("complete = false;"),
                "A ring that cannot be fully restored keeps reporting scaled.");
        }
    }
}
