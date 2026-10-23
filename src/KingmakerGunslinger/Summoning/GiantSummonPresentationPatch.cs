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
using KingmakerGunslinger.RuntimeTesting;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Exact owned palettes and relative render-only family scale. No native
    /// gameplay root, skeleton pose, collider or mechanical size is scaled.
    /// </summary>
    [HarmonyPatch(typeof(UnitEntityView), "OnDataAttached")]
    internal static class GiantSummonPresentationPatch
    {
        private sealed class State
        {
            internal UnitEntityData Owner;
            internal GiantPresentationScale Baseline;
            internal string Outcome;
            internal string Key;
            internal Cleanup2GiantRenderScalePrototype RenderOwner;
            internal bool Refused;
        }

        private static readonly ConditionalWeakTable<UnitEntityView, State> States =
            new ConditionalWeakTable<UnitEntityView, State>();
        private static string _probeRunId;

        internal static IDisposable BeginRenderScaleProbe(RuntimeTestRequest request)
        {
            if (request == null || !request.Enabled || !request.ExitAfterCompletion ||
                request.Scenario != "disposable-expanded-summoning-cleanup2-native-review" ||
                request.Parameters == null || request.Parameters.Count != 1 ||
                (string)request.Parameters["reviewPhase"] != "scale-prototype" ||
                string.IsNullOrWhiteSpace(request.RunId) || _probeRunId != null)
                throw new InvalidOperationException("One exact disposable scale-prototype request may retain the native scale baseline.");
            _probeRunId = request.RunId;
            return new ProbeScope(request.RunId);
        }

        private sealed class ProbeScope : IDisposable
        {
            private string _run;
            internal ProbeScope(string run) { _run = run; }
            public void Dispose()
            {
                if (_run == null) return;
                if (_probeRunId != _run) throw new InvalidOperationException("Foreign scale probe scope.");
                _probeRunId = null; _run = null;
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
                { Release(view); return "giant:module-off"; }
                UnitEntityData unit = view == null ? null : view.EntityData;
                if (unit == null || unit.Blueprint == null) return "giant:no-unit";
                GiantSummonPresentationProfile profile = GiantSummonPresentationPolicy.Resolve(true,
                    unit.Blueprint.AssetGuid, SummonedSourceGuid(unit));
                if (profile == null) return "giant:not-owned";
                GiantSummonPaletteRuntime.Schedule(view, profile);
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
                if (_probeRunId != null)
                    return state.Outcome = "giant:" + profile.Key + ";height=request-owned-native-baseline;run=" + _probeRunId;
                GiantSummonRenderScaleLifetime lifetime = view.GetComponent<GiantSummonRenderScaleLifetime>();
                if (lifetime == null) lifetime = view.gameObject.AddComponent<GiantSummonRenderScaleLifetime>();
                lifetime.View = view;
                return ApplyRenderHeight(view);
            }
            catch (Exception exception)
            {
                // Presentation never interrupts the native attach/buff lifecycle.
                return "giant:exception:" + exception.GetType().Name;
            }
        }

        internal static string ApplyRenderHeight(UnitEntityView view)
        {
            State state;
            if (view == null || !States.TryGetValue(view, out state)) return "giant:not-owned";
            if (!ExpandedSummoningVisualVariantPatch.IsModuleActive)
            { Release(view); return "giant:module-off"; }
            if (!ReferenceEquals(view.EntityData, state.Owner))
            { Release(view); return "giant:foreign-view-owner"; }
            if (state.Refused) return state.Outcome;
            // Unity's destroyed-object null operator must not turn a retained
            // refused owner into permission to claim the native view again.
            if (!ReferenceEquals(state.RenderOwner, null))
            {
                if (state.RenderOwner.IsDisposed || state.RenderOwner == null)
                { state.Refused = true; return state.Outcome = "giant:" + state.Key + ";height=owned-renderer-refused:" + state.RenderOwner.Status; }
                return state.Outcome;
            }
            string source = SummonedSourceGuid(state.Owner);
            var profile = GiantSummonPresentationPolicy.Resolve(true, state.Owner.Blueprint.AssetGuid, source);
            if (profile == null || source == null) return state.Outcome = "giant:" + state.Key + ";height=await-live-summon-context";
            if (!GiantSummonPresentationPolicy.Same(state.Baseline, Read(view.transform.localScale)))
            { state.Refused = true; return state.Outcome = "giant:" + state.Key + ";height=foreign-native-root-scale-refused"; }
            if (state.Owner.Blueprint.Prefab?.AssetId != profile.NativePrefabId)
            { state.Refused = true; return state.Outcome = "giant:" + state.Key + ";height=native-prefab-mismatch"; }
            if (profile.RelativeVisualFactor == 1f)
                return state.Outcome = "giant:" + state.Key + ";height=relative-baseline;factor=1";
            string clothAdmission = Cleanup2GiantRenderScalePrototype.ProductionClothAdmission(state.Owner, profile);
            if (clothAdmission != "no-cloth" && clothAdmission != "qualified-frozen-cape")
                return state.Outcome = "giant:" + state.Key + ";height=" + clothAdmission;
            if (GiantSummonPalettePolicy.ProductionProof(profile.Key) != null &&
                !GiantSummonPaletteRuntime.VerifyProductionOwnership(view).StartsWith("palette:verified-production-owner;", StringComparison.Ordinal))
                return state.Outcome = "giant:" + state.Key + ";height=await-owned-production-palette";
            try
            {
                state.RenderOwner = Cleanup2GiantRenderScalePrototype.TakeProduction(state.Owner, profile);
                return state.Outcome = "giant:" + state.Key + ";height=owned-relative-render-scale;factor=" +
                    profile.RelativeVisualFactor.ToString("0.###", CultureInfo.InvariantCulture);
            }
            catch (Exception error)
            {
                state.Refused = true;
                return state.Outcome = "giant:" + state.Key + ";height=refused:" + error.GetType().Name + ":" + error.Message;
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
            if (!ReferenceEquals(state.RenderOwner, null)) state.RenderOwner.Dispose();
            States.Remove(view);
        }

        // Read-only evidence from the actual production owner. This never
        // attaches, retries, constructs a prototype or changes a native view.
        internal static JObject ObserveProductionOwnership(UnitEntityView view)
        {
            State state;
            if (view == null || !States.TryGetValue(view, out state))
                return new JObject { ["ownerPresent"] = false, ["outcome"] = Describe(view),
                    ["copiedClothProductionGateOpen"] = Cleanup2GiantRenderScalePrototype.ProductionCopiedClothEnabled };
            bool retainedOwner = !ReferenceEquals(state.RenderOwner, null);
            JObject result = retainedOwner ? state.RenderOwner.ObserveProduction() : new JObject { ["ownerPresent"] = false };
            result["retainedOwnerReference"] = retainedOwner;
            result["retainedOwnerUnityAlive"] = retainedOwner && state.RenderOwner != null;
            result["outcome"] = state.Outcome; result["key"] = state.Key;
            result["unitOwnerExact"] = ReferenceEquals(state.Owner, view.EntityData);
            result["nativeRootScaleExact"] = GiantSummonPresentationPolicy.Same(state.Baseline, Read(view.transform.localScale));
            result["copiedClothProductionGateOpen"] = Cleanup2GiantRenderScalePrototype.ProductionCopiedClothEnabled;
            return result;
        }

        internal static Cleanup2GiantRenderScalePrototype ProductionOwner(UnitEntityView view)
        {
            State state;
            return view != null && States.TryGetValue(view, out state) ? state.RenderOwner : null;
        }

        private static GiantPresentationScale Read(Vector3 scale)
        { return new GiantPresentationScale(scale.x, scale.y, scale.z); }
    }

    // The installed fader writes EntityViewBase.Renderers, not a hierarchy
    // census. Owned visual receivers participate in that exact native path;
    // displaced native renderers remain suppressed after its authentic write.
    [HarmonyPatch(typeof(EntityFader), "SetRendererVisibility", new[] { typeof(bool) })]
    internal static class GiantSummonNativeVisibilityPatch
    {
        private static void Postfix(EntityFader __instance, bool visible)
        { Cleanup2GiantRenderScalePrototype.NativeVisibilityChanged(__instance, visible); }
    }

    // Release scale ownership before the default-order palette owner can
    // discharge its independent materials on module-off. Normal mirror pose
    // following stays at10010 after native WeaponSnap.
    [DefaultExecutionOrder(-10)]
    internal sealed class GiantSummonRenderScaleLifetime : MonoBehaviour
    {
        internal UnitEntityView View;
        private void LateUpdate()
        { if (View != null) GiantSummonPresentationPatch.ApplyRenderHeight(View); }
        private void OnDestroy()
        { if (View != null) GiantSummonPresentationPatch.Release(View); }
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
