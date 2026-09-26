using System;
using System.Collections.Generic;

namespace KingmakerGunslinger.FavoredClass
{
    /// <summary>
    /// A stack of open scopes whose closing never depends on a postfix alone
    /// (Harmony 1.2 has no finalizers). A scope is closed by the method that
    /// opened it, in a finally block (Run, Guard), and a close restores
    /// exactly the depth that existed before the matching open: a nested
    /// scope never clears an outer one, a frame left behind by a callee that
    /// threw is removed by the enclosing close before anything else runs, and
    /// a retry opens a fresh frame.
    /// </summary>
    internal sealed class FavoredClassScopeStack<T> where T : class
    {
        private readonly List<T> _frames = new List<T>();

        /// <summary>The number of open frames.</summary>
        internal int Depth
        {
            get { return _frames.Count; }
        }

        /// <summary>The innermost open frame, or null when none is open.</summary>
        internal T Current
        {
            get { return _frames.Count == 0 ? null : _frames[_frames.Count - 1]; }
        }

        /// <summary>Opens a frame; returns the depth that closing it restores.</summary>
        internal int Push(T frame)
        {
            int depth = _frames.Count;
            _frames.Add(frame);
            return depth;
        }

        /// <summary>Closes every frame opened at or above <paramref name="depth"/>.</summary>
        internal void Restore(int depth)
        {
            if (depth < 0)
                depth = 0;
            if (depth < _frames.Count)
                _frames.RemoveRange(depth, _frames.Count - depth);
        }

        /// <summary>Runs <paramref name="body"/> inside one frame closed in a finally block.</summary>
        internal TResult Run<TResult>(T frame, Func<TResult> body)
        {
            if (body == null) throw new ArgumentNullException("body");
            int depth = Push(frame);
            try
            {
                return body();
            }
            finally
            {
                Restore(depth);
            }
        }

        /// <summary>Runs <paramref name="body"/> inside one frame closed in a finally block.</summary>
        internal void Run(T frame, Action body)
        {
            if (body == null) throw new ArgumentNullException("body");
            int depth = Push(frame);
            try
            {
                body();
            }
            finally
            {
                Restore(depth);
            }
        }

        /// <summary>
        /// Runs <paramref name="body"/> on <paramref name="argument"/> without
        /// opening a frame and restores the entry depth in a finally block, so
        /// any frame the body opened and failed to close is closed here.
        /// </summary>
        internal void Guard<TArgument>(TArgument argument, Action<TArgument> body)
        {
            if (body == null) throw new ArgumentNullException("body");
            int depth = _frames.Count;
            try
            {
                body(argument);
            }
            finally
            {
                Restore(depth);
            }
        }
    }
}
