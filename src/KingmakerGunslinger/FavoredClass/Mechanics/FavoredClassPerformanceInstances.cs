using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.SriptZones;
using UnityEngine;

namespace KingmakerGunslinger.FavoredClass.Mechanics
{
    /// <summary>
    /// O01: the live area instances of every owner's performances, grouped
    /// per owner and performance (FavoredClassRangeGroup: widened only as a
    /// whole). The widening transaction asks whether an instance may widen
    /// and records every outcome; the owner's descriptions read the group.
    /// Narrowing restores the ring and the radius independently and then
    /// verifies both; an instance that cannot be verified native is ended,
    /// and the performance toggle whose own buff runs that area is turned
    /// off. Only live views are kept: nothing survives the end of its area,
    /// a save load or a new game, and no outcome is remembered.
    /// </summary>
    internal static class FavoredClassPerformanceInstances
    {
        private static readonly FieldInfo SpawnedFx = typeof(AreaEffectView).GetField("m_SpawnedFx",
            BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo AppliedBuff = typeof(ActivatableAbility).GetField("m_AppliedBuff",
            BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly object Gate = new object();
        private static readonly Dictionary<string, FavoredClassRangeGroup<AreaEffectView>> Groups =
            new Dictionary<string, FavoredClassRangeGroup<AreaEffectView>>(StringComparer.Ordinal);
        private static readonly Dictionary<AreaEffectView, float> NativeRadii =
            new Dictionary<AreaEffectView, float>();

        /// <summary>
        /// Guarded runtime qualification only: runs inside a sibling's
        /// narrowing before its ring is restored, so a restore failure can be
        /// injected into the real rollback; null in play.
        /// </summary>
        internal static Action<AreaEffectView> NarrowFaultForQualification;

        /// <summary>Whether a live instance may attempt widening: every other live instance of its group is widened.</summary>
        internal static bool MayWiden(AreaEffectView view, UnitEntityData owner, string key)
        {
            if (view == null || owner == null || key == null)
                return false;
            lock (Gate)
            {
                Purge();
                if (!IsLive(view))
                    return false;
                FavoredClassRangeGroup<AreaEffectView> group;
                return !Groups.TryGetValue(Name(owner.UniqueId, key), out group) || group.MayWiden(view);
            }
        }

        /// <summary>Records one live instance's outcome and restores its group's invariant.</summary>
        internal static void Record(AreaEffectView view, UnitEntityData owner, string key,
            FavoredClassWideningOutcome outcome, int feet, float native)
        {
            if (view == null || owner == null || key == null)
                return;
            lock (Gate)
            {
                Purge();
                if (!IsLive(view))
                    return;
                NativeRadii[view] = native;
                string name = Name(owner.UniqueId, key);
                FavoredClassRangeGroup<AreaEffectView> group;
                if (!Groups.TryGetValue(name, out group))
                    Groups[name] = group = new FavoredClassRangeGroup<AreaEffectView>();
                group.Record(view, outcome, feet, VerifyNative, Narrow, End);
            }
        }

        /// <summary>The range the owner's descriptions of the performance show, in feet, or null for native text.</summary>
        internal static int? Feet(UnitDescriptor owner, string key, int? configuredFeet)
        {
            UnitEntityData unit = owner == null ? null : owner.Unit;
            if (unit == null || key == null)
                return configuredFeet;
            lock (Gate)
            {
                Purge();
                FavoredClassRangeGroup<AreaEffectView> group;
                return Groups.TryGetValue(Name(unit.UniqueId, key), out group) ? group.Feet(configuredFeet) :
                    configuredFeet;
            }
        }

        /// <summary>The owner's live instances of the performance (qualification evidence).</summary>
        internal static int LiveCount(UnitDescriptor owner, string key)
        {
            UnitEntityData unit = owner == null ? null : owner.Unit;
            if (unit == null)
                return 0;
            lock (Gate)
            {
                Purge();
                FavoredClassRangeGroup<AreaEffectView> group;
                return Groups.TryGetValue(Name(unit.UniqueId, key), out group) ? group.Count : 0;
            }
        }

        /// <summary>One live instance's recorded outcome (qualification evidence).</summary>
        internal static FavoredClassWideningOutcome? OutcomeOf(UnitDescriptor owner, string key, AreaEffectView view)
        {
            UnitEntityData unit = owner == null ? null : owner.Unit;
            if (unit == null)
                return null;
            lock (Gate)
            {
                Purge();
                FavoredClassRangeGroup<AreaEffectView> group;
                return Groups.TryGetValue(Name(unit.UniqueId, key), out group) ? group.OutcomeOf(view) : null;
            }
        }

        private static string Name(string owner, string key)
        {
            return owner + "|" + key;
        }

        /// <summary>Forgets destroyed views, areas that ended and empty groups.</summary>
        private static void Purge()
        {
            foreach (KeyValuePair<string, FavoredClassRangeGroup<AreaEffectView>> pair in Groups.ToArray())
            {
                pair.Value.Purge(IsLive);
                if (pair.Value.Count == 0)
                    Groups.Remove(pair.Key);
            }
            foreach (AreaEffectView view in NativeRadii.Keys.ToArray())
                if (!IsLive(view))
                    NativeRadii.Remove(view);
        }

        private static bool IsLive(AreaEffectView view)
        {
            if (view == null)
                return false;
            var data = view.Data as AreaEffectEntityData;
            return data == null || (!data.Destroyed && !data.IsEnded);
        }

        private static GameObject Ring(AreaEffectView view)
        {
            try
            {
                return SpawnedFx == null ? null : SpawnedFx.GetValue(view) as GameObject;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>Whether the instance's cylinder is at its native radius and its ring carries no owner scale.</summary>
        private static bool VerifyNative(AreaEffectView view)
        {
            float native;
            if (view == null || !NativeRadii.TryGetValue(view, out native))
                return false;
            var cylinder = view.Shape as ScriptZoneCylinder;
            GameObject ring = Ring(view);
            return cylinder != null && Math.Abs(cylinder.Radius - native) < 0.0001f &&
                (ring == null || !FavoredClassPerformanceRing.IsScaled(ring));
        }

        /// <summary>Restores a widened instance: the ring and the radius independently, then verified.</summary>
        private static bool Narrow(AreaEffectView view)
        {
            GameObject ring = Ring(view);
            try
            {
                Action<AreaEffectView> fault = NarrowFaultForQualification;
                if (fault != null)
                    fault(view);
                if (ring != null)
                    FavoredClassPerformanceRing.Restore(ring);
            }
            catch (Exception)
            {
                // Verified below.
            }
            try
            {
                float native;
                var cylinder = view.Shape as ScriptZoneCylinder;
                if (cylinder != null && NativeRadii.TryGetValue(view, out native))
                    cylinder.Radius = native;
            }
            catch (Exception)
            {
                // Verified below.
            }
            return VerifyNative(view);
        }

        /// <summary>
        /// Ends an instance that cannot be verified native. The toggle whose
        /// own current buff runs that area is turned off, so the performance
        /// stops instead of spending rounds on an area that no longer exists;
        /// an older, lingering area never stops the current performance.
        /// </summary>
        private static void End(AreaEffectView view)
        {
            var data = view == null ? null : view.Data as AreaEffectEntityData;
            if (data == null)
                return;
            UnitEntityData owner = data.Context == null ? null : data.Context.MaybeCaster;
            data.ForceEnd();
            if (owner == null || AppliedBuff == null)
                return;
            foreach (ActivatableAbility toggle in owner.Descriptor.ActivatableAbilities.Enumerable.ToArray())
            {
                var buff = toggle.IsOn ? AppliedBuff.GetValue(toggle) as Buff : null;
                if (buff != null && ReferenceEquals(buff.Context, data.Context))
                    toggle.IsOn = false;
            }
        }
    }
}
