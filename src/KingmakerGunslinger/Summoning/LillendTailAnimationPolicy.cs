using System;
using System.Linq;
namespace KingmakerGunslinger.Summoning
{
    internal static class LillendTailAnimationPolicy
    {
        internal const string UnitGuid = "71a8adf916b4409680023e853d28583b";
        internal const string UnitName = "KMG_Summoning_Unit_LillendAzata";
        internal const string Prefab = "ced3729f4b4abab4da4ef63d8489f857";
        internal const string Sword = "f2a606d19b0240abb62255da5e4e1973";
        internal const float Duration = 1.4f;
        internal const float ActTime = .60f;
        internal static string[] TailNames { get { return Enumerable.Range(0,10)
            .Select(i => "KMG_LillendTail" + i.ToString("00",System.Globalization.CultureInfo.InvariantCulture)).ToArray(); } }
        internal static bool ExactTailNames(string[] names) { return names != null && names.SequenceEqual(TailNames); }
        internal static bool PermitsBinding(bool enabled,string guid,string name,string prefab,string sword,bool ownedPlayer,bool ownedBones)
        { return enabled && guid == UnitGuid && name == UnitName && prefab == Prefab && sword == Sword && ownedPlayer && ownedBones; }
        internal static bool Finite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
    }
    internal static class LillendProductionViewPolicy
    {
        internal static bool Permits(bool enabled,string guid,string name,string prefab,string sword,bool polymorphed)
        { return !polymorphed && LillendTailAnimationPolicy.PermitsBinding(enabled,guid,name,prefab,sword,true,true); }
    }
}
