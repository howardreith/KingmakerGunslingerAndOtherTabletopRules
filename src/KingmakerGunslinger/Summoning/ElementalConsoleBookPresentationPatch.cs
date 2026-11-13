using System.Reflection;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    // Shared native-binding helpers only. Every elemental disclosure was
    // withdrawn by owner request: the console spellbook projection on
    // 2026-11-11, and the console level-up learning projection, its
    // "Elementals / Show sources" header VM and its intake, virtual-list,
    // header-text, confirm and tooltip patches on 2026-11-12. Original
    // Summon Elemental rows are now either hidden by the UMM option (see
    // OriginalSummonElementalVisibility) or shown as ordinary native rows.
    internal static class ElementalConsoleBookPresentation
    {
        internal const BindingFlags Fields=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;

        internal static object Model(Component view)
        {return view==null?null:view.GetType().GetProperty("ViewModel",Fields)?.GetValue(view,null);}
        internal static bool Exact(MethodBase method,int bytes)
        {return method!=null && method.Module.ModuleVersionId.ToString()==ElementalSourceChooserRuntime.GameMvid &&
            method.GetMethodBody()?.GetILAsByteArray().Length==bytes;}
    }
}
