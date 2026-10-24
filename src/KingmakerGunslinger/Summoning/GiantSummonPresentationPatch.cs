using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
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
    /// Exact owned palettes and each summoned giant family's size. Height is
    /// the view's own original scale and a Huge giant's space its own
    /// corpulence, so the installed view applies both to every native consumer
    /// and keeps them through selection, renderer refreshes and size effects.
    /// Unit Size, statistics, weapon reach and blueprints are not written.
    /// </summary>
    [HarmonyPatch(typeof(UnitEntityView), "OnDataAttached")]
    internal static class GiantSummonPresentationPatch
    {
        private sealed class State
        {
            internal UnitEntityData Owner;
            internal string Key;
            internal string Outcome;
            internal GiantPresentationScale IntakeScale, AppliedScale;
            internal float IntakeCorpulence, AppliedCorpulence;
            internal bool Applied, Refused;
        }

        private const BindingFlags Fields = BindingFlags.Instance | BindingFlags.NonPublic;
        // Installed UnitEntityView.Awake captures m_OriginalScale; OnDataAttached
        // and LateUpdate write localScale = m_OriginalScale * m_Scale, and
        // get_Corpulence returns max(0.5, GetSizeScale() * m_Corpulence).
        private static readonly FieldInfo OriginalScaleField = typeof(UnitEntityView).GetField("m_OriginalScale", Fields);
        private static readonly FieldInfo SizeScaleField = typeof(UnitEntityView).GetField("m_Scale", Fields);
        private static readonly FieldInfo CorpulenceField = typeof(UnitEntityView).GetField("m_Corpulence", Fields);
        private static readonly ConditionalWeakTable<UnitEntityView, State> States =
            new ConditionalWeakTable<UnitEntityView, State>();
        private static string _probeRunId;

        internal static bool NativeSizeContract
        {
            get
            {
                return OriginalScaleField != null && OriginalScaleField.FieldType == typeof(Vector3) &&
                    SizeScaleField != null && SizeScaleField.FieldType == typeof(float) &&
                    CorpulenceField != null && CorpulenceField.FieldType == typeof(float);
            }
        }

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

        // Before native OnDataAttached builds the root scale, selection
        // colliders and movement agent from these two bases.
        private static void Prefix(UnitEntityView __instance)
        {
            try { ApplyNativeSize(__instance); }
            catch (Exception) { }
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
                GiantSummonRenderScaleLifetime lifetime = view.GetComponent<GiantSummonRenderScaleLifetime>();
                if (lifetime == null) lifetime = view.gameObject.AddComponent<GiantSummonRenderScaleLifetime>();
                lifetime.View = view;
                return ApplyNativeSize(view);
            }
            catch (Exception exception)
            {
                // Presentation never interrupts the native attach/buff lifecycle.
                return "giant:exception:" + exception.GetType().Name;
            }
        }

        internal static string ApplyNativeSize(UnitEntityView view)
        {
            if (view == null) return "giant:no-view";
            if (!ExpandedSummoningVisualVariantPatch.IsModuleActive)
            { Release(view); return "giant:module-off"; }
            UnitEntityData unit = view.EntityData;
            if (unit == null || unit.Blueprint == null) return "giant:no-unit";
            State state;
            if (States.TryGetValue(view, out state) && !ReferenceEquals(state.Owner, unit))
            {
                Release(view);
                state = null;
            }
            if (state != null && (state.Refused || state.Applied && CurrentSizeExact(view, state)))
                return state.Outcome;
            // A native Frost view may attach before its summoned buff exists;
            // the buff patch and the lifetime retry once provenance is live.
            GiantSummonPresentationProfile profile = GiantSummonPresentationPolicy.Resolve(true,
                unit.Blueprint.AssetGuid, SummonedSourceGuid(unit));
            if (profile == null) return state != null ? state.Outcome : Describe(view);
            if (!NativeSizeContract)
                return Refuse(view, state, unit, profile, "native-size-contract-unavailable");
            if (state == null)
            {
                state = new State { Owner = unit, Key = profile.Key,
                    IntakeScale = Read((Vector3)OriginalScaleField.GetValue(view)),
                    IntakeCorpulence = (float)CorpulenceField.GetValue(view) };
                States.Add(view, state);
            }
            if (_probeRunId != null)
                return state.Outcome = "giant:" + profile.Key + ";height=request-owned-native-baseline;run=" + _probeRunId;
            GiantPresentationScale target; float corpulence;
            string plan = GiantSummonPresentationPolicy.PlanNativeSize(profile, state.IntakeScale, state.IntakeCorpulence,
                Read((Vector3)OriginalScaleField.GetValue(view)), (float)CorpulenceField.GetValue(view), out target, out corpulence);
            if (plan != "apply-native-size" && plan != "already-applied")
                return Refuse(view, state, unit, profile, plan);
            if (plan == "apply-native-size")
            {
                Vector3 scale = new Vector3(target.X, target.Y, target.Z);
                OriginalScaleField.SetValue(view, scale);
                CorpulenceField.SetValue(view, corpulence);
                view.transform.localScale = scale * (float)SizeScaleField.GetValue(view);
            }
            state.AppliedScale = target; state.AppliedCorpulence = corpulence; state.Applied = true;
            return state.Outcome = "giant:" + profile.Key + ";size=native;factor=" +
                profile.RelativeVisualFactor.ToString("0.###", CultureInfo.InvariantCulture) + ";footprint=" +
                profile.FootprintFactor.ToString("0.###", CultureInfo.InvariantCulture);
        }

        private static string Refuse(UnitEntityView view, State state, UnitEntityData unit,
            GiantSummonPresentationProfile profile, string reason)
        {
            if (state == null)
            {
                state = new State { Owner = unit, Key = profile.Key };
                States.Add(view, state);
            }
            state.Refused = true;
            return state.Outcome = "giant:" + profile.Key + ";size=refused:" + reason;
        }

        private static bool CurrentSizeExact(UnitEntityView view, State state)
        {
            return NativeSizeContract &&
                GiantSummonPresentationPolicy.Same(Read((Vector3)OriginalScaleField.GetValue(view)), state.AppliedScale) &&
                GiantSummonPresentationPolicy.Near((float)CorpulenceField.GetValue(view), state.AppliedCorpulence);
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

        // Restores the native bases only while they still hold this patch's
        // values; a later foreign size change is kept.
        internal static void Release(UnitEntityView view)
        {
            if (view == null) return;
            State state;
            if (!States.TryGetValue(view, out state)) return;
            States.Remove(view);
            if (!state.Applied || !CurrentSizeExact(view, state)) return;
            Vector3 scale = new Vector3(state.IntakeScale.X, state.IntakeScale.Y, state.IntakeScale.Z);
            OriginalScaleField.SetValue(view, scale);
            CorpulenceField.SetValue(view, state.IntakeCorpulence);
            view.transform.localScale = scale * (float)SizeScaleField.GetValue(view);
        }

        // Read-only evidence of the current native size. This never applies,
        // retries or restores anything.
        internal static JObject ObserveNativeSize(UnitEntityView view)
        {
            State state;
            var result = new JObject { ["outcome"] = Describe(view), ["nativeSizeContract"] = NativeSizeContract };
            if (view == null || !NativeSizeContract) { result["ownerPresent"] = false; return result; }
            Vector3 original = (Vector3)OriginalScaleField.GetValue(view);
            float sizeScale = (float)SizeScaleField.GetValue(view), corpulence = (float)CorpulenceField.GetValue(view);
            result["currentOriginalScale"] = Vector(original);
            result["currentLocalScale"] = Vector(view.transform.localScale);
            result["nativeSizeScale"] = sizeScale;
            result["currentCorpulenceBase"] = corpulence;
            result["viewCorpulence"] = view.Corpulence;
            result["mechanicalSize"] = view.EntityData == null ? null : view.EntityData.Descriptor.State.Size.ToString();
            bool owned = States.TryGetValue(view, out state);
            result["ownerPresent"] = owned;
            if (!owned) return result;
            GiantSummonPresentationProfile profile = GiantSummonPresentationPolicy.All.Single(value => value.Key == state.Key);
            result["key"] = state.Key;
            result["unitOwnerExact"] = ReferenceEquals(state.Owner, view.EntityData);
            result["applied"] = state.Applied; result["refused"] = state.Refused;
            result["uniformVisualFactor"] = profile.RelativeVisualFactor;
            result["footprintFactor"] = profile.FootprintFactor;
            result["huge"] = profile.Huge;
            result["intakeOriginalScale"] = Vector(new Vector3(state.IntakeScale.X, state.IntakeScale.Y, state.IntakeScale.Z));
            result["intakeCorpulenceBase"] = state.IntakeCorpulence;
            bool exact = state.Applied && CurrentSizeExact(view, state);
            result["currentSizeExact"] = exact;
            result["rootScaleFollowsOriginal"] = GiantSummonPresentationPolicy.Same(Read(view.transform.localScale),
                Read(original * sizeScale));
            result["heightOverIntake"] = state.IntakeScale.Y > 0f ? original.y / state.IntakeScale.Y : float.NaN;
            result["footprintOverIntake"] = state.IntakeCorpulence > 0f ? corpulence / state.IntakeCorpulence : float.NaN;
            return result;
        }

        private static GiantPresentationScale Read(Vector3 scale)
        { return new GiantPresentationScale(scale.x, scale.y, scale.z); }

        private static JArray Vector(Vector3 value)
        { return new JArray(value.x, value.y, value.z); }
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

    // Release size ownership before the default-order palette owner can
    // discharge its independent materials on module-off. Applied views only
    // compare their two native bases each frame.
    [DefaultExecutionOrder(-10)]
    internal sealed class GiantSummonRenderScaleLifetime : MonoBehaviour
    {
        internal UnitEntityView View;
        private void LateUpdate()
        { if (View != null) GiantSummonPresentationPatch.ApplyNativeSize(View); }
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

    // Native UnitEntityView.OnDataAttached calls SpawnFx and HandleDeath calls
    // HandleUnitDeath. Skipping both for Fire and Storm leaves nothing spawned,
    // so there is nothing to restore; native Frost keeps its own effects.
    internal static class GiantSummonDonorFrostEffects
    {
        internal static bool Suppresses(SpawnFxOnStart component)
        {
            try
            {
                UnitEntityView view = component == null ? null : component.GetComponent<UnitEntityView>();
                UnitEntityData unit = view == null ? null : view.EntityData;
                return unit != null && unit.Blueprint != null &&
                    GiantSummonPresentationPolicy.SuppressesDonorFrostEffects(
                        ExpandedSummoningVisualVariantPatch.IsModuleActive, unit.Blueprint.AssetGuid);
            }
            catch (Exception) { return false; }
        }
    }

    [HarmonyPatch(typeof(SpawnFxOnStart), "SpawnFx")]
    internal static class GiantSummonDonorFrostStartFxPatch
    {
        private static bool Prefix(SpawnFxOnStart __instance)
        { return !GiantSummonDonorFrostEffects.Suppresses(__instance); }
    }

    [HarmonyPatch(typeof(SpawnFxOnStart), "HandleUnitDeath")]
    internal static class GiantSummonDonorFrostDeathFxPatch
    {
        private static bool Prefix(SpawnFxOnStart __instance)
        { return !GiantSummonDonorFrostEffects.Suppresses(__instance); }
    }
}
