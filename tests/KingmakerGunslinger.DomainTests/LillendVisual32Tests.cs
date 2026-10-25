using System;
using System.Linq;
using KingmakerGunslinger.Summoning;
namespace KingmakerGunslinger.DomainTests
{
    internal static class LillendVisual32Tests
    {
        internal static void ExactOwnerAndNativeCarrierOnly()
        {
            var g=LillendTailAnimationPolicy.UnitGuid; var n=LillendTailAnimationPolicy.UnitName;
            var p=LillendTailAnimationPolicy.Prefab; var w=LillendTailAnimationPolicy.Sword;
            Assertions.True(LillendProductionViewPolicy.Permits(true,g,n,p,w,false), "Exact original-body owner admitted.");
            Assertions.False(LillendProductionViewPolicy.Permits(false,g,n,p,w,false), "Disabled module.");
            Assertions.False(LillendProductionViewPolicy.Permits(true,"foreign",n,p,w,false), "Foreign blueprint.");
            Assertions.False(LillendProductionViewPolicy.Permits(true,g,"foreign",p,w,false), "Foreign name.");
            Assertions.False(LillendProductionViewPolicy.Permits(true,g,n,"foreign",w,false), "Foreign native carrier.");
            Assertions.False(LillendProductionViewPolicy.Permits(true,g,n,p,"foreign",false), "Foreign weapon.");
            Assertions.False(LillendProductionViewPolicy.Permits(true,g,n,p,w,true), "Polymorph excluded.");
            Assertions.False(LillendTailAnimationPolicy.PermitsBinding(true,g,n,p,w,false,true), "Borrowed player excluded.");
            Assertions.False(LillendTailAnimationPolicy.PermitsBinding(true,g,n,p,w,true,false), "Borrowed tail excluded.");
        }
        internal static void ExportedDriversKeepNativeAndOriginalOwnership()
        {
            var logical=LillendHumanBindingPolicy.NativeNames.Concat(LillendTailAnimationPolicy.TailNames).ToArray();
            var names=LillendHumanBindingPolicy.Names;
            Assertions.True(names.Select(name=>logical[LillendHumanBindingPolicy.BindingIndex(name)]).SequenceEqual(names),
                "Sorted exported weights retain their exact native/original driver mapping.");
            Assertions.False(LillendTailAnimationPolicy.ExactTailNames(SalamanderTailAnimationPolicy.TailNames), "Other creature tail cannot bind.");
            Assertions.True(LillendTailAnimationPolicy.ExactTailNames(LillendTailAnimationPolicy.TailNames), "Exact authored tail accepted.");
            Assertions.False(LillendTailAnimationPolicy.ExactTailNames(LillendTailAnimationPolicy.TailNames.Reverse().ToArray()), "Tail ordering is anatomical.");
            Assertions.Throws<ArgumentException>(()=>LillendHumanBindingPolicy.BindingIndex("Weapons"), "Storage is never an anatomical driver.");
            Assertions.True(LillendHumanBindingPolicy.IsReviewedAuxiliary("Cape_Red_M(Clone)","CP_Cape2Sided_M_Any",0), "Only reviewed auxiliary suppressed.");
            Assertions.False(LillendHumanBindingPolicy.IsReviewedAuxiliary("Equipment","CP_Cape2Sided_M_Any",0), "Equipment cannot be suppressed.");
        }
    }
}
