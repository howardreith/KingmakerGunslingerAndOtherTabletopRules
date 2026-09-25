using System;
using System.IO;
using System.Linq;
using KingmakerGunslinger.FavoredClass;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Review finding 5: the level-up replay and demoralize scopes are closed
    /// in finally blocks, restore the previous (outer) frame and survive
    /// exceptions and retries.
    /// </summary>
    internal static class FavoredClassScopeStackTests
    {
        private sealed class Frame
        {
            internal readonly string Name;

            internal Frame(string name)
            {
                Name = name;
            }
        }

        private static void Throw(string message)
        {
            throw new InvalidOperationException(message);
        }

        internal static void NestedScopesRestoreThePreviousFrame()
        {
            var stack = new FavoredClassScopeStack<Frame>();
            var outer = new Frame("outer");
            var inner = new Frame("inner");
            string seen = stack.Run(outer, () =>
            {
                string innerSeen = stack.Run(inner, () => stack.Current.Name);
                return innerSeen + "/" + stack.Current.Name;
            });
            Assertions.Equal("inner/outer", seen, "A nested scope sees itself, then its outer scope again.");
            Assertions.True(stack.Depth == 0 && stack.Current == null, "Closing the outer scope leaves nothing open.");
            // The Harmony prefix/postfix pair: each postfix restores its own
            // prefix's depth, so an inner close never clears the outer frame.
            int outerDepth = stack.Push(outer);
            int innerDepth = stack.Push(inner);
            stack.Restore(innerDepth);
            Assertions.True(ReferenceEquals(stack.Current, outer) && stack.Depth == 1,
                "An inner postfix restores the outer frame.");
            stack.Restore(outerDepth);
            Assertions.True(stack.Depth == 0, "The outer postfix closes its own frame.");
        }

        internal static void AnExceptionClosesTheScopeAndKeepsTheOuterFrame()
        {
            var stack = new FavoredClassScopeStack<Frame>();
            var outer = new Frame("outer");
            int outerDepth = stack.Push(outer);
            bool caught = false;
            try
            {
                stack.Run(new Frame("replayed pick"), () => Throw("injected failure inside the scope"));
            }
            catch (InvalidOperationException)
            {
                caught = true;
            }
            Assertions.True(caught, "The exception still reaches the caller.");
            Assertions.True(ReferenceEquals(stack.Current, outer) && stack.Depth == 1,
                "A throwing scope leaves only the outer frame open.");
            caught = false;
            try
            {
                stack.Run<bool>(new Frame("checked pick"), () =>
                {
                    Throw("injected failure in a checked call");
                    return true;
                });
            }
            catch (InvalidOperationException)
            {
                caught = true;
            }
            Assertions.True(caught && stack.Depth == 1, "A throwing value scope is closed too.");
            stack.Restore(outerDepth);
            Assertions.True(stack.Depth == 0, "The outer frame closes normally.");
        }

        internal static void AGuardClosesAFrameItsCalleeLeftOpen()
        {
            var stack = new FavoredClassScopeStack<Frame>();
            var outer = new Frame("outer demoralize");
            var stale = new Frame("throwing demoralize");
            stack.Push(outer);
            bool caught = false;
            try
            {
                // The prefix opened a frame and the action threw before its
                // postfix: the per-action guard restores the entry depth.
                stack.Guard(stale, frame =>
                {
                    stack.Push(frame);
                    Throw("injected failure after the prefix");
                });
            }
            catch (InvalidOperationException)
            {
                caught = true;
            }
            Assertions.True(caught, "The action's exception still reaches the native handler.");
            Assertions.True(ReferenceEquals(stack.Current, outer) && stack.Depth == 1,
                "The next action never sees the throwing action's frame.");
            // A normal action inside the guard leaves the depth unchanged.
            stack.Guard(stale, frame =>
            {
                int depth = stack.Push(frame);
                stack.Restore(depth);
            });
            Assertions.True(stack.Depth == 1, "A completed action changes nothing.");
        }

        internal static void ARetryAfterAFailureOpensAFreshScope()
        {
            var stack = new FavoredClassScopeStack<Frame>();
            var controller = new Frame("controller");
            int attempts = 0;
            Func<bool> replay = () =>
            {
                attempts++;
                if (attempts == 1)
                    Throw("the first replay fails");
                return ReferenceEquals(stack.Current, controller);
            };
            bool firstFailed = false;
            try
            {
                stack.Run(controller, replay);
            }
            catch (InvalidOperationException)
            {
                firstFailed = true;
            }
            Assertions.True(firstFailed && stack.Depth == 0 && stack.Current == null,
                "Nothing stays marked after the failed replay.");
            Assertions.True(stack.Run(controller, replay), "The retry runs inside its own fresh scope.");
            Assertions.True(stack.Depth == 0 && attempts == 2, "The retry closes its scope as well.");
            // Restoring a depth that is already closed is harmless.
            stack.Restore(3);
            stack.Restore(-1);
            Assertions.True(stack.Depth == 0, "Idempotent restore.");
        }

        private static string Source(params string[] parts)
        {
            return File.ReadAllText(Path.Combine(new[] { Environment.CurrentDirectory, "src",
                "KingmakerGunslinger", "FavoredClass" }.Concat(parts).ToArray())).Replace("\r\n", "\n");
        }

        // No scope is closed by a postfix alone: the replay's picks and every
        // native action run through finally-closed helpers, the demoralize
        // postfix restores its own depth, and I07 is withheld without the
        // envelope.
        internal static void ScopesCloseInFinallyBlocks()
        {
            string envelope = Source("Hooks", "FavoredClassActionEnvelopePatch.cs");
            string demoralizeHook = Source("Hooks", "FavoredClassDemoralizeScopePatch.cs");
            string scope = Source("Mechanics", "FavoredClassDemoralizeScope.cs");
            string coordinator = Source("FavoredClassIntegrationCoordinator.cs");
            string scoping = Source("FavoredClassCallScoping.cs");
            Assertions.True(envelope.Contains("[HarmonyPatch(typeof(ActionList), \"Run\")]") &&
                envelope.Contains("FavoredClassCallScoping.ReplaceSingle(values, NativeRun, ScopedRun, false)") &&
                envelope.Contains("FavoredClassDemoralizeScope.EnvelopeInstalled = scoped;") &&
                envelope.Contains("return scoped ? values : original;"),
                "ActionList.Run's one per-action call is routed through the finally-closed envelope, or left untouched.");
            Assertions.True(scope.Contains("Frames.Guard(action, RunNative);") &&
                scope.Contains("action => action.RunAction()") &&
                scope.Contains("context != null && EnvelopeInstalled && s_Frames != null &&"),
                "The envelope makes the same native call and a check qualifies only while it is installed.");
            Assertions.True(demoralizeHook.Contains("private static void Prefix(out int __state)") &&
                demoralizeHook.Contains("__state = FavoredClassDemoralizeScope.Enter();") &&
                demoralizeHook.Contains("private static void Postfix(int __state)") &&
                demoralizeHook.Contains("FavoredClassDemoralizeScope.Exit(__state);") &&
                demoralizeHook.Contains("[HarmonyPriority(Priority.First)]"),
                "The demoralize postfix restores its own prefix's depth, before Call of the Wild's replacement.");
            Assertions.True(coordinator.Contains("!Mechanics.FavoredClassDemoralizeScope.EnvelopeInstalled") &&
                coordinator.Contains("unavailable.Add(FavoredClassCatalog.EffectDemoralize);"),
                "I07 is withheld when the envelope is not installed.");
            Assertions.True(scoping.Contains("if (index >= 0)\n                    return false;") &&
                scoping.Contains("values[index].blocks.Count != 0") &&
                scoping.Contains("instance.labels.AddRange(call.labels);"),
                "Only an exact single call site is replaced, and branches to it keep their stack.");
        }
    }
}
