using System;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    internal sealed class SummonAttackGestureScope
    {
        internal string Key, UnitGuid, UnitName, PrefabGuid, SourceGuid, WeaponGuid;
        internal bool ModuleActive, RegisteredUnit, RegisteredSource, PositiveCaster, LiveSummon,
            ExactViewReceipt, ExactNativePalette, MissingNativeHandle, NativeSchedule, CurrentPoseEquality;
    }

    // Original render poses only. None of these values selects, creates,
    // schedules or acts a native attack/animation/rule.
    internal static class SummonAttackGesturePolicy
    {
        internal const string Crab = "giant-crab", Brachiosaurus = "brachiosaurus";
        internal const string BrachiosaurusUnit = "9093aef39ecc416092ef68655a7e56fb";
        internal const string TailWeapon = "13b51070d6f84a15bfed98a04c98fa30";
        internal const float NativeFallbackDelay = 1f, RecoverySeconds = .5f;
        internal static readonly string[] BrachiosaurusSources = {
            "5ff406f72e834d868276c43786a05133", "7ab83d22b4284fbe8ab038776d443507", "10a645ecd39f41d88fc5ea0018e5210e",
            "8edc4ca5bab3563ba34e18326926842e", "78fc5dcf4d5650df9c08e71b4d32682c", "c87b82cb13b5538080ae8d1864266fe0",
            "cf254d4c99a85a95b7876fb2a1d08c9c", "90e0a532280e5390a4123c5415d611dd", "cf5e6a5c24875880b92778eb64992be2",
            "10c8c8e7bd7e58169fca4489d1ad1545", "f5306d9802655dca854d9f94e092da2c" };
        internal static bool Handles(string key) { return key == Crab || key == Brachiosaurus; }
        internal static bool Permits(SummonAttackGestureScope s)
        {
            if (s == null || !Handles(s.Key) || !s.ModuleActive || !s.RegisteredUnit || !s.RegisteredSource ||
                !s.PositiveCaster || !s.LiveSummon || !s.ExactViewReceipt || !s.ExactNativePalette || !s.MissingNativeHandle ||
                !s.NativeSchedule || !s.CurrentPoseEquality) return false;
            return s.Key == Crab ? s.UnitGuid == CrabSummonAnimationBindingPolicy.UnitGuid &&
                s.UnitName == CrabSummonAnimationBindingPolicy.UnitName && s.PrefabGuid == CrabSummonAnimationBindingPolicy.PrefabGuid &&
                s.WeaponGuid == CrabSummonAnimationBindingPolicy.ClawGuid && CrabSummonAnimationBindingPolicy.SourceGuids.Contains(s.SourceGuid) :
                s.UnitGuid == BrachiosaurusUnit && s.UnitName == "KMG_Summoning_Unit_Brachiosaurus" &&
                s.PrefabGuid == "fe9f25fe62e0f3b49a13910a996470b2" && s.WeaponGuid == TailWeapon && BrachiosaurusSources.Contains(s.SourceGuid);
        }

        internal static float Smooth(float t) { t = Math.Max(0f, Math.Min(1f, t)); return t * t * (3f - 2f * t); }
        private static float Mix(float a, float b, float t) { return a + (b - a) * Smooth(t); }
        internal static float Angle(float elapsed, float windup, float act, float recovery)
        {
            if (float.IsNaN(elapsed) || float.IsInfinity(elapsed) || elapsed < 0f) return 0f;
            if (elapsed <= .45f) return Mix(0f, windup, elapsed / .45f);
            if (elapsed <= 1f) return Mix(windup, act, (elapsed - .45f) / .55f);
            if (elapsed <= 1.25f) return Mix(act, recovery, (elapsed - 1f) / .25f);
            return Mix(recovery, 0f, (elapsed - 1.25f) / .25f);
        }
        internal static float TailYaw(int index, float elapsed)
        { return Angle(elapsed, new[] { -15f, -20f, -15f, -8f }[index], new[] { 15f, 25f, 25f, 10f }[index], new[] { 5f, 8f, 6f, 3f }[index]); }
        internal static float TailPitch(int index, float elapsed)
        { return Angle(elapsed, new[] { -6f, -8f, -4f, -2f }[index], new[] { 6f, 8f, 4f, 2f }[index], new[] { 2f, 3f, 1f, 0f }[index]); }
        internal static float ChelaYaw(int joint, bool left, float elapsed)
        { return (left ? 1f : -1f) * Angle(elapsed, joint < 3 ? -10f : -3f, joint < 3 ? 12f : 4f, joint < 3 ? 4f : 1f); }
        internal static float ChelaPitch(int joint, float elapsed)
        { return Angle(elapsed, joint < 3 ? -12f : -4f, joint < 3 ? 10f : 3f, joint < 3 ? 3f : 1f); }
    }
}
