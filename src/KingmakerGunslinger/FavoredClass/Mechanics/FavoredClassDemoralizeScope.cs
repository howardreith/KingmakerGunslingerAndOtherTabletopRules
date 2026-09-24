using System;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Mechanics;

namespace KingmakerGunslinger.FavoredClass.Mechanics
{
    /// <summary>
    /// The mechanics context of the demoralize action currently resolving on
    /// this thread, recorded by the native Demoralize.RunAction hook (it
    /// wraps both the native body and Call of the Wild's replacement). A
    /// check qualifies only when its own context is that exact context, so a
    /// scope left behind by an exception can never match a later action.
    /// </summary>
    internal static class FavoredClassDemoralizeScope
    {
        [ThreadStatic]
        private static MechanicsContext _current;

        internal static void Enter()
        {
            var data = ElementsContext.GetData<MechanicsContext.Data>();
            _current = data == null ? null : data.Context;
        }

        internal static void Exit()
        {
            _current = null;
        }

        internal static bool IsCurrent(MechanicsContext context)
        {
            return context != null && ReferenceEquals(_current, context);
        }
    }
}
