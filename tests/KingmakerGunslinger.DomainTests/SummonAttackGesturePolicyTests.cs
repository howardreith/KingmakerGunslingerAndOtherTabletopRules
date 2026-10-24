using System;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class SummonAttackGesturePolicyTests
    {
        internal static void ExactSourcesAndEveryOwnershipGate()
        {
            foreach (string key in new[] { SummonAttackGesturePolicy.Crab, SummonAttackGesturePolicy.Brachiosaurus })
            {
                foreach (string source in key == SummonAttackGesturePolicy.Crab ? CrabSummonAnimationBindingPolicy.SourceGuids : SummonAttackGesturePolicy.BrachiosaurusSources)
                { var valid = Valid(key); valid.SourceGuid = source; Assertions.True(SummonAttackGesturePolicy.Permits(valid), "Exact released source refused."); }
                Action<SummonAttackGestureScope>[] mutations = { s => s.ModuleActive = false, s => s.RegisteredUnit = false,
                    s => s.RegisteredSource = false, s => s.PositiveCaster = false, s => s.LiveSummon = false,
                    s => s.ExactViewReceipt = false, s => s.ExactNativePalette = false, s => s.MissingNativeHandle = false,
                    s => s.NativeSchedule = false, s => s.CurrentPoseEquality = false, s => s.Key = "native-donor",
                    s => s.UnitGuid = "foreign", s => s.UnitName += "_Foreign", s => s.PrefabGuid = "foreign",
                    s => s.WeaponGuid = "foreign", s => s.SourceGuid = null, s => s.SourceGuid = "foreign" };
                foreach (var mutation in mutations)
                { var scope = Valid(key); mutation(scope); Assertions.False(SummonAttackGesturePolicy.Permits(scope), "Foreign/missing native presentation admission was waived."); }
            }
            Assertions.False(SummonAttackGesturePolicy.Permits(null), "Missing scope accepted.");
        }
        internal static void TailSourceCensusMatchesPublishedSummonAdapters()
        {
            var entries = JObject.Parse(File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "blueprints", "blueprints.json")))["entries"];
            string[] actual = entries.Where(value => (string)value["plannedType"] == "BlueprintAbility" && (string)value["status"] == "active" &&
                ((string)value["symbol"]).Contains("Brachiosaurus") &&
                (((string)value["symbol"]).StartsWith("KMG.Summoning.Ability.", StringComparison.Ordinal) ||
                 ((string)value["symbol"]).StartsWith("KMG.Summoning.Adapter.", StringComparison.Ordinal)))
                .Select(value => (string)value["guid"]).OrderBy(value => value, StringComparer.Ordinal).ToArray();
            Assertions.True(actual.Length > 0 && actual.SequenceEqual(SummonAttackGesturePolicy.BrachiosaurusSources.OrderBy(value => value, StringComparer.Ordinal)),
                "Tail admission must match actual published canonical/adapter sources, excluding Trample/Overrun.");
        }
        internal static void OriginalCurvesAreFiniteBoundedAndReturnToZero()
        {
            Assertions.True(SummonAttackGesturePolicy.ImmediateNativeAct(false,false,false,false,true,0f,.0336125f,.0336125f,1), "Native no-animation first tick is legitimate before its stored deadline.");
            Assertions.True(SummonAttackGesturePolicy.ImmediateNativeAct(false,false,false,false,true,3f,3f,.04f,1), "Schedule created inside current OnTick may act within that same tick.");
            Assertions.False(SummonAttackGesturePolicy.ImmediateNativeAct(true,false,false,false,true,0f,.03f,.03f,1), "Native animation branch cannot masquerade as immediate fallback.");
            Assertions.False(SummonAttackGesturePolicy.ImmediateNativeAct(false,false,false,false,false,0f,.03f,.03f,1), "A direct/unsupplied native handled transition cannot qualify.");
            Assertions.False(SummonAttackGesturePolicy.ImmediateNativeAct(false,false,false,false,true,0f,.03f,.03f,2), "Second eligible tick cannot masquerade as first.");
            Assertions.False(SummonAttackGesturePolicy.ImmediateNativeAct(false,false,false,false,true,0f,1f,.03f,1), "Invented one-second minimum cannot replace the native branch.");
            Assertions.Equal(0f,SummonAttackGesturePolicy.ActualActCurveTime(false,.7f),"No pre-act windup is synthesized.");
            Assertions.Equal(1f,SummonAttackGesturePolicy.ActualActCurveTime(true,0f),"Original strike starts at actual native act.");
            Assertions.Equal(1.5f,SummonAttackGesturePolicy.ActualActCurveTime(true,.5f),"Recovery uses actual post-act GameTime only.");
            for (int i = 0; i < 4; i++)
            {
                Assertions.Equal(0f, SummonAttackGesturePolicy.TailYaw(i, 0f), "Tail starts neutral.");
                Assertions.Equal(0f, SummonAttackGesturePolicy.TailPitch(i, 1.5f), "Tail recovery returns neutral.");
                for (int sample = -10; sample <= 200; sample++)
                {
                    float yaw = SummonAttackGesturePolicy.TailYaw(i, sample / 100f), pitch = SummonAttackGesturePolicy.TailPitch(i, sample / 100f);
                    Assertions.True(!float.IsNaN(yaw) && !float.IsInfinity(yaw) && Math.Abs(yaw) <= 25f && Math.Abs(pitch) <= 8f, "Original distal curve exceeded authored envelope.");
                }
            }
            foreach (float invalid in new[] { float.NaN, float.PositiveInfinity, -1f })
                Assertions.Equal(0f, SummonAttackGesturePolicy.Angle(invalid, 3, 5, 2), "Invalid elapsed time cannot pose a skin.");
            for (int i = 0; i < 7; i++)
            {
                Assertions.Equal(0f, SummonAttackGesturePolicy.ChelaYaw(i, true, 1.5f), "Chela recovers neutral.");
                Assertions.Equal(-SummonAttackGesturePolicy.ChelaYaw(i, true, 1f), SummonAttackGesturePolicy.ChelaYaw(i, false, 1f), "Original left/right chela sweep is mirrored.");
            }
        }
        private static SummonAttackGestureScope Valid(string key)
        {
            bool crab = key == SummonAttackGesturePolicy.Crab;
            return new SummonAttackGestureScope { Key = key, UnitGuid = crab ? CrabSummonAnimationBindingPolicy.UnitGuid : SummonAttackGesturePolicy.BrachiosaurusUnit,
                UnitName = crab ? CrabSummonAnimationBindingPolicy.UnitName : "KMG_Summoning_Unit_Brachiosaurus",
                PrefabGuid = crab ? CrabSummonAnimationBindingPolicy.PrefabGuid : "fe9f25fe62e0f3b49a13910a996470b2",
                SourceGuid = crab ? CrabSummonAnimationBindingPolicy.SourceGuids[0] : SummonAttackGesturePolicy.BrachiosaurusSources[0],
                WeaponGuid = crab ? CrabSummonAnimationBindingPolicy.ClawGuid : SummonAttackGesturePolicy.TailWeapon,
                ModuleActive = true, RegisteredUnit = true, RegisteredSource = true, PositiveCaster = true, LiveSummon = true,
                ExactViewReceipt = true, ExactNativePalette = true, MissingNativeHandle = true, NativeSchedule = true, CurrentPoseEquality = true };
        }
    }
}
