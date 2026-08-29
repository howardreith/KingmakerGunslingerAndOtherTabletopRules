using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.DomainTests
{
    internal static class FirearmConditionNotificationTests
    {
        internal static void BrokenMessageIsExact()
        {
            string message;
            Assertions.True(FirearmConditionNotificationDispatcher.TryFormat(
                    "Akasa", "Pistol", FirearmCondition.Normal,
                    FirearmCondition.Broken, out message),
                "Normal-to-Broken did not produce a notification.");
            Assertions.Equal("Akasa's Pistol is now broken.", message,
                "Broken notification text changed.");
        }

        internal static void WreckedMessageIsExactAndConcise()
        {
            string message;
            Assertions.True(FirearmConditionNotificationDispatcher.TryFormat(
                    "Akasa", "Musket", FirearmCondition.Broken,
                    FirearmCondition.Wrecked, out message),
                "Broken-to-Wrecked did not produce a notification.");
            Assertions.Equal("Akasa's Musket is now wrecked.", message,
                "Wrecked notification text changed.");
            Assertions.Equal(1, Count(message, "Akasa"),
                "The player name did not appear exactly once.");
            Assertions.Equal(1, Count(message, "Musket"),
                "The item name did not appear exactly once.");
            foreach (string forbidden in new[] { "roll", "misfire range",
                "confirmation", "repair", "\n" })
                Assertions.False(message.IndexOf(forbidden,
                        StringComparison.OrdinalIgnoreCase) >= 0,
                    "Top notification contains technical explanation: " +
                        forbidden);
            Assertions.True(message.EndsWith(".", StringComparison.Ordinal) &&
                    message.Contains("wrecked") && !message.Contains("Wrecked"),
                "Condition casing or final punctuation changed.");
        }

        internal static void MissingWielderUsesItemFallback()
        {
            string message;
            Assertions.True(FirearmConditionNotificationDispatcher.TryFormat(
                    "  ", " Pistol ", FirearmCondition.Normal,
                    FirearmCondition.Broken, out message),
                "Missing-wielder fallback was rejected.");
            Assertions.Equal("Pistol is now broken.", message,
                "Missing-wielder fallback changed.");
        }

        internal static void NormalToBrokenDispatchesOnce()
        {
            var sink = new RecordingTopSink();
            var dispatcher =
                new FirearmConditionNotificationDispatcher(sink);
            Assertions.True(dispatcher.Publish("Akasa", "Pistol",
                    FirearmCondition.Normal, FirearmCondition.Broken, null),
                "Normal-to-Broken dispatch failed.");
            Assertions.Equal(1, sink.Count,
                "Normal-to-Broken did not dispatch exactly once.");
            Assertions.Equal(1L, dispatcher.Published,
                "Successful publication counter changed.");
            Assertions.Equal(1L, dispatcher.Attempts,
                "Successful publication attempt count changed.");
        }

        internal static void BrokenToWreckedDispatchesOnce()
        {
            var sink = new RecordingTopSink();
            var dispatcher =
                new FirearmConditionNotificationDispatcher(sink);
            Assertions.True(dispatcher.Publish("Akasa", "Musket",
                    FirearmCondition.Broken, FirearmCondition.Wrecked, null),
                "Broken-to-Wrecked dispatch failed.");
            Assertions.Equal(1, sink.Count,
                "Broken-to-Wrecked did not dispatch exactly once.");
            Assertions.Equal("Akasa's Musket is now wrecked.",
                dispatcher.LastMessage,
                "Dispatcher did not retain the exact last message.");
        }

        internal static void UnchangedAndRecoveryTransitionsDoNotDispatch()
        {
            var sink = new RecordingTopSink();
            var dispatcher =
                new FirearmConditionNotificationDispatcher(sink);
            foreach (FirearmCondition[] transition in new[]
            {
                new[] { FirearmCondition.Normal, FirearmCondition.Normal },
                new[] { FirearmCondition.Broken, FirearmCondition.Broken },
                new[] { FirearmCondition.Wrecked, FirearmCondition.Broken },
                new[] { FirearmCondition.Broken, FirearmCondition.Normal }
            })
                Assertions.False(dispatcher.Publish("Akasa", "Pistol",
                        transition[0], transition[1], null),
                    "A non-degradation transition dispatched a top notification.");
            Assertions.Equal(0, sink.Count,
                "Unchanged or recovery transitions reached the top sink.");
            Assertions.Equal(0L, dispatcher.Attempts,
                "Unchanged or recovery transitions counted as dispatch attempts.");
        }

        internal static void CombatLogPrecedesTopNotification()
        {
            var order = new List<string>();
            var top = new RecordingTopSink(order);
            var dispatcher =
                new FirearmConditionNotificationDispatcher(top);
            var combat = new RecordingCombatSink(order);
            var feedback = new FirearmConditionDegradationFeedback(
                combat, dispatcher);
            Assertions.True(feedback.PublishAfterCommit("Akasa", "Pistol",
                    FirearmCondition.Normal, FirearmCondition.Broken,
                    "misfire", null),
                "Committed degradation feedback failed.");
            Assertions.Equal(2, order.Count,
                "Committed degradation did not publish both feedback channels.");
            Assertions.Equal("combat", order[0],
                "Top notification preceded the combat log.");
            Assertions.Equal("top", order[1],
                "Top notification was not second.");
            Assertions.Equal(1, combat.Count,
                "Existing combat-log publication was not retained exactly once.");
        }

        internal static void SinkFailureDoesNotUndoCommittedState()
        {
            FirearmState committed = FirearmStateMachine.ApplyMisfireDamage(
                FirearmState.CreateEmpty());
            var sink = new RecordingTopSink { Throw = true };
            var dispatcher =
                new FirearmConditionNotificationDispatcher(sink);
            var combat = new RecordingCombatSink();
            var feedback = new FirearmConditionDegradationFeedback(
                combat, dispatcher);
            int failures = 0;
            Assertions.False(feedback.PublishAfterCommit("Akasa", "Pistol",
                    FirearmCondition.Normal, committed.Condition,
                    "misfire", exception => failures++),
                "Throwing top sink was reported as successful.");
            Assertions.Equal(FirearmCondition.Broken, committed.Condition,
                "Presentation failure altered committed firearm condition.");
            Assertions.Equal(1, failures,
                "Top-sink failure did not invoke one diagnostic hook.");
            Assertions.Equal(1L, dispatcher.Faults,
                "Top-sink failure counter changed.");
            Assertions.Equal(1, combat.Count,
                "Top-sink failure suppressed the earlier combat-log publication.");
        }

        internal static void ProductionBoundaryIsPostCommitAndExclusive()
        {
            string root = Environment.CurrentDirectory;
            string sourceRoot = Path.Combine(root, "src",
                "KingmakerGunslinger");
            string ordinary = Read(sourceRoot, "Misfires",
                "FirearmMisfireRuntime.cs");
            string deadShot = Read(sourceRoot, "Deeds", "DeadShotRuntime.cs");
            string scatter = Read(sourceRoot, "Scatter",
                "ScatterShotRuntime.cs");
            string native = Read(sourceRoot, "Firearms",
                "FirearmConditionTopNotification.cs");
            string dispatcher = Read(sourceRoot, "Firearms",
                "FirearmConditionNotificationDispatcher.cs");
            string runtimeProbe = Read(sourceRoot, "RuntimeTesting",
                "RuntimeTestRunner.cs");

            Assertions.True(ordinary.IndexOf(
                    "committed.Repository.State != condition.After",
                    StringComparison.Ordinal) <
                ordinary.IndexOf("PublishAfterCommittedDegradation",
                    StringComparison.Ordinal),
                "Ordinary misfire notification is not after state verification.");
            Assertions.True(ordinary.IndexOf("if (fortuneIgnored)",
                    StringComparison.Ordinal) <
                ordinary.IndexOf("ConditionService.Evaluate",
                    StringComparison.Ordinal) &&
                ordinary.IndexOf("if (!firstEvaluation)",
                    StringComparison.Ordinal) <
                ordinary.IndexOf("ConditionService.Evaluate",
                    StringComparison.Ordinal) &&
                ordinary.IndexOf("ExpertLoadingRuntime.Apply",
                    StringComparison.Ordinal) <
                ordinary.IndexOf("if (condition.ChangesCondition)",
                    StringComparison.Ordinal),
                "Prevention or duplicate-evaluation exits moved after notification eligibility.");
            Assertions.True(deadShot.IndexOf(
                    "new DeadShotExecutionResult",
                    StringComparison.Ordinal) <
                deadShot.IndexOf("PublishAfterCommittedDegradation",
                    StringComparison.Ordinal),
                "Dead Shot publishes before its rollback-capable work completes.");
            Assertions.True(scatter.Contains(
                    "FirearmItemStateSnapshot conditionCommit =") &&
                scatter.IndexOf("conditionCommit.Repository.State",
                    StringComparison.Ordinal) <
                scatter.IndexOf("PublishAfterCommittedDegradation",
                    StringComparison.Ordinal),
                "Scatter Shot does not bind feedback to the committed snapshot.");

            string runtimeRoot = Path.Combine(sourceRoot, "RuntimeTesting") +
                Path.DirectorySeparatorChar;
            string[] callers = Directory.GetFiles(sourceRoot, "*.cs",
                    SearchOption.AllDirectories).Where(path =>
                        !path.StartsWith(runtimeRoot,
                            StringComparison.OrdinalIgnoreCase) &&
                        File.ReadAllText(path).Contains(
                            "PublishAfterCommittedDegradation"))
                .Select(Path.GetFileName).OrderBy(value => value,
                    StringComparer.Ordinal).ToArray();
            Assertions.Equal(4, callers.Length,
                "Unexpected hydration, reconciliation, recovery, or probe code can dispatch degradation notifications.");
            Assertions.True(callers.Contains("FirearmMisfireRuntime.cs") &&
                    callers.Contains("DeadShotRuntime.cs") &&
                    callers.Contains("ScatterShotRuntime.cs") &&
                    callers.Contains("FirearmConditionTopNotification.cs"),
                "The exact three player-visible degradation paths are not the exclusive callers.");
            int runtimeCall = runtimeProbe.IndexOf(
                "PublishAfterCommittedDegradation",
                StringComparison.Ordinal);
            string runtimeBoundary = runtimeCall < 0 ? string.Empty :
                runtimeProbe.Substring(runtimeCall, Math.Min(600,
                    runtimeProbe.Length - runtimeCall));
            Assertions.True(Count(runtimeProbe,
                    "PublishAfterCommittedDegradation") == 1 &&
                    Count(runtimeBoundary, "FirearmCondition.Broken") == 2 &&
                    runtimeBoundary.Contains(
                        "runtime diagnostic unchanged probe") &&
                    runtimeProbe.Contains("!unchangedPublished"),
                "The request-local diagnostic probe can publish a real degradation notification.");
            Assertions.True(native.Contains("UIUtility.SendWarning(message)") &&
                    native.Contains("FirearmConditionCombatLog.Publish") &&
                    dispatcher.IndexOf("_combatLog.Publish",
                        StringComparison.Ordinal) <
                    dispatcher.IndexOf("return _notification.Publish",
                        StringComparison.Ordinal),
                "Native toast route or combat-log-before-toast order changed.");
        }

        private static string Read(string root, params string[] parts)
        {
            string path = root;
            foreach (string part in parts) path = Path.Combine(path, part);
            return File.ReadAllText(path);
        }

        private static int Count(string value, string token)
        {
            return value.Split(new[] { token },
                StringSplitOptions.None).Length - 1;
        }

        private sealed class RecordingCombatSink :
            IFirearmConditionCombatLogSink
        {
            private readonly IList<string> _order;

            internal RecordingCombatSink(IList<string> order = null)
            {
                _order = order;
            }

            internal int Count { get; private set; }

            public void Publish(string itemDisplayName,
                FirearmCondition before, FirearmCondition after, string reason)
            {
                Count++;
                if (_order != null) _order.Add("combat");
            }
        }

        private sealed class RecordingTopSink :
            IFirearmConditionTopNotificationSink
        {
            private readonly IList<string> _order;

            internal RecordingTopSink(IList<string> order = null)
            {
                _order = order;
            }

            internal int Count { get; private set; }
            internal bool Throw { get; set; }

            public void Publish(string message)
            {
                Count++;
                if (_order != null) _order.Add("top");
                if (Throw) throw new InvalidOperationException(
                    "Synthetic top-notification failure.");
            }
        }
    }
}
