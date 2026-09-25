using System;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Mechanics;

namespace KingmakerGunslinger.FavoredClass.Mechanics
{
    /// <summary>
    /// The mechanics contexts of the demoralize actions currently resolving on
    /// this thread, innermost last. The native Demoralize.RunAction hook opens
    /// a frame before Call of the Wild's replacing prefix and its postfix
    /// restores the depth it opened at, so a nested demoralize never clears an
    /// outer one. Harmony 1.2 has no finalizers: the native per-action call of
    /// ActionList.Run is made through RunAction, which restores the entry
    /// depth in a finally block, so a frame left by a demoralize (or its
    /// replacement) that threw is closed before the next action runs. A check
    /// qualifies only when its own context is the innermost frame's exact
    /// context, and never while that envelope is not installed.
    /// </summary>
    internal static class FavoredClassDemoralizeScope
    {
        [ThreadStatic]
        private static FavoredClassScopeStack<MechanicsContext> s_Frames;

        private static readonly Action<GameAction> RunNative = action => action.RunAction();

        /// <summary>
        /// Guarded runtime qualification only: runs inside a newly opened
        /// frame, so a failure can be injected into a real demoralize; null in
        /// play.
        /// </summary>
        internal static Action FaultInjection;

        /// <summary>Whether ActionList.Run makes its per-action call through RunAction (set when it applies).</summary>
        internal static bool EnvelopeInstalled { get; set; }

        /// <summary>Open demoralize frames on this thread.</summary>
        internal static int Depth
        {
            get { return s_Frames == null ? 0 : s_Frames.Depth; }
        }

        private static FavoredClassScopeStack<MechanicsContext> Frames
        {
            get { return s_Frames ?? (s_Frames = new FavoredClassScopeStack<MechanicsContext>()); }
        }

        /// <summary>Opens the frame of the demoralize now resolving; returns the depth its close restores.</summary>
        internal static int Enter()
        {
            var data = ElementsContext.GetData<MechanicsContext.Data>();
            int depth = Frames.Push(data == null ? null : data.Context);
            Action fault = FaultInjection;
            if (fault != null)
                fault();
            return depth;
        }

        /// <summary>Closes the frame opened at <paramref name="depth"/> and anything opened above it.</summary>
        internal static void Exit(int depth)
        {
            Frames.Restore(depth);
        }

        internal static bool IsCurrent(MechanicsContext context)
        {
            return context != null && EnvelopeInstalled && s_Frames != null &&
                ReferenceEquals(s_Frames.Current, context);
        }

        /// <summary>
        /// The native per-action call of ActionList.Run: the same
        /// action.RunAction() with the entry depth restored in a finally
        /// block. The exception, if any, still reaches the native handler.
        /// </summary>
        internal static void RunAction(GameAction action)
        {
            Frames.Guard(action, RunNative);
        }
    }
}
