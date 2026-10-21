using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.View;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Visual transforms only, with measured calibration required. The existing
    /// visual-variant API owns all material clones and controller reinitializing;
    /// this patch creates no renderer, material, texture or blueprint object.
    /// </summary>
    [HarmonyPatch(typeof(UnitEntityView), "OnDataAttached")]
    internal static class GiantSummonPresentationPatch
    {
        private sealed class State
        {
            internal UnitEntityData Owner;
            internal GiantPresentationScale Baseline;
            internal GiantPresentationScale? Applied;
            internal string Outcome;
            internal string Key;
        }

        private static readonly ConditionalWeakTable<UnitEntityView, State> States =
            new ConditionalWeakTable<UnitEntityView, State>();
        // Intentionally empty until actual native rest-pose measurements and
        // the human control are recorded. Missing calibration is observable.
        private static readonly Dictionary<string, GiantSummonHeightCalibration> Calibrations =
            new Dictionary<string, GiantSummonHeightCalibration>(StringComparer.Ordinal);

        internal static void RegisterCalibration(GiantSummonHeightCalibration calibration)
        {
            if (calibration == null) throw new ArgumentNullException("calibration");
            GiantSummonPresentationProfile profile = GiantSummonPresentationPolicy.All
                .SingleOrDefault(value => value.Key == calibration.CreatureKey);
            GiantPresentationScale ignored;
            string verdict = GiantSummonPresentationPolicy.Calculate(profile, calibration,
                calibration.PrefabId, calibration.BaselineScale, calibration.BaselineScale, out ignored);
            if (verdict != "apply-measured-height" && verdict != "already-applied")
                throw new InvalidOperationException("Giant calibration refused: " + verdict);
            lock (Calibrations)
            {
                GiantSummonHeightCalibration existing;
                if (Calibrations.TryGetValue(profile.Key, out existing))
                {
                    if (ReferenceEquals(existing, calibration)) return;
                    throw new InvalidOperationException("Giant calibration already fixed for this process: " + profile.Key);
                }
                Calibrations.Add(profile.Key, calibration);
            }
        }

        internal static string Describe(UnitEntityView view)
        {
            State state;
            if (view != null && States.TryGetValue(view, out state)) return state.Outcome;
            return view != null && view.EntityData != null && view.EntityData.Blueprint != null &&
                view.EntityData.Blueprint.AssetGuid == GiantSummonPresentationPolicy.NativeFrostGuid
                ? "giant:native-frost-without-authorized-provenance" : "giant:not-owned";
        }

        private static void Postfix(UnitEntityView __instance)
        { Apply(__instance); }

        internal static string Apply(UnitEntityView view)
        {
            try
            {
                if (!ExpandedSummoningVisualVariantPatch.IsModuleActive)
                    return "giant:module-off";
                UnitEntityData unit = view == null ? null : view.EntityData;
                if (unit == null || unit.Blueprint == null) return "giant:no-unit";
                GiantSummonPresentationProfile profile = GiantSummonPresentationPolicy.Resolve(true,
                    unit.Blueprint.AssetGuid, SummonedSourceGuid(unit));
                if (profile == null) return "giant:not-owned";
                State state;
                if (States.TryGetValue(view, out state) && !ReferenceEquals(state.Owner, unit))
                {
                    Release(view);
                    state = null;
                }
                if (state == null)
                {
                    state = new State { Owner = unit, Key = profile.Key,
                        Baseline = Read(view.transform.localScale) };
                    States.Add(view, state);
                }
                GiantSummonHeightCalibration calibration;
                lock (Calibrations) Calibrations.TryGetValue(profile.Key, out calibration);
                GiantPresentationScale target;
                string decision = GiantSummonPresentationPolicy.Calculate(profile, calibration,
                    unit.Blueprint.Prefab == null ? null : unit.Blueprint.Prefab.AssetId,
                    state.Baseline, Read(view.transform.localScale), out target);
                if (decision == "apply-measured-height")
                {
                    view.transform.localScale = new Vector3(target.X, target.Y, target.Z);
                    state.Applied = target;
                }
                state.Outcome = "giant:" + profile.Key + ";height=" + decision +
                    ";referenceFeet=" + profile.ReferenceHeightFeet.ToString("0.##", CultureInfo.InvariantCulture) +
                    ";calibration=" + (calibration == null ? "<none>" : calibration.EvidenceIdentity);
                return state.Outcome;
            }
            catch (Exception exception)
            {
                // Presentation never interrupts the native attach/buff lifecycle.
                return "giant:exception:" + exception.GetType().Name;
            }
        }

        private static string SummonedSourceGuid(UnitEntityData unit)
        {
            BlueprintRoot root = BlueprintRoot.Instance;
            var summoned = root == null || root.SystemMechanics == null ? null :
                root.SystemMechanics.SummonedUnitBuff;
            if (summoned == null || unit.Descriptor == null) return null;
            // Kingmaker 2.1.7b exposes a foreach pattern here, not an
            // IEnumerable<Buff> contract for LINQ.
            var matches = new List<Buff>();
            foreach (Buff value in unit.Descriptor.Buffs)
                if (value != null && value.Blueprint != null &&
                    value.Blueprint.AssetGuid == summoned.AssetGuid)
                    matches.Add(value);
            Buff[] buffs = matches.ToArray();
            if (buffs.Length != 1 || buffs[0].Context == null ||
                buffs[0].Context.SourceAbility == null) return null;
            return buffs[0].Context.SourceAbility.AssetGuid;
        }

        internal static void Release(UnitEntityView view)
        {
            if (view == null) return;
            State state;
            if (!States.TryGetValue(view, out state)) return;
            // Restore only the transform this record still owns. Foreign
            // rescaling is preserved, including during view reuse/destruction.
            if (state.Applied.HasValue && GiantSummonPresentationPolicy.Same(
                Read(view.transform.localScale), state.Applied.Value))
                view.transform.localScale = new Vector3(state.Baseline.X,
                    state.Baseline.Y, state.Baseline.Z);
            States.Remove(view);
        }

        private static GiantPresentationScale Read(Vector3 scale)
        { return new GiantPresentationScale(scale.x, scale.y, scale.z); }
    }

    // A fresh native summon may attach its view before SummonedUnitBuff gains
    // its context. Retry after that exact buff, also covering cold reconstruction.
    [HarmonyPatch(typeof(Buff), "SpawnParticleEffect")]
    internal static class GiantSummonPresentationBuffPatch
    {
        private static void Postfix(Buff __instance)
        {
            try
            {
                BlueprintRoot root = BlueprintRoot.Instance;
                var summoned = root == null || root.SystemMechanics == null ? null :
                    root.SystemMechanics.SummonedUnitBuff;
                if (__instance == null || __instance.Blueprint == null || summoned == null ||
                    __instance.Blueprint.AssetGuid != summoned.AssetGuid || __instance.Owner == null) return;
                UnitEntityData unit = __instance.Owner.Unit;
                if (unit != null && unit.View != null) GiantSummonPresentationPatch.Apply(unit.View);
            }
            catch (Exception) { }
        }
    }

    [HarmonyPatch(typeof(UnitEntityView), "OnDestroy")]
    internal static class GiantSummonPresentationDestroyPatch
    {
        private static void Prefix(UnitEntityView __instance)
        {
            try { GiantSummonPresentationPatch.Release(__instance); }
            catch (Exception) { }
        }
    }
}
