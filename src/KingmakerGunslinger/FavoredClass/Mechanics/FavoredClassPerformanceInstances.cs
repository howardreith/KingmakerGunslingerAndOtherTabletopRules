using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;
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
    /// off. An instance is forgotten only once it is verified no longer live
    /// (its view destroyed, or its area ended or destroyed): an ending that
    /// throws, does nothing or finds no area data, and a liveness read that
    /// throws, keep it tracked and unresolved (no widening for its group,
    /// native descriptions, a logged diagnostic) until a later widening
    /// attempt or recording retries it. Nothing survives the end of its area,
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
        private const int DiagnosticCapacity = 64;
        private static readonly List<string> Diagnostics = new List<string>();

        /// <summary>
        /// Guarded runtime qualification only: runs inside a sibling's
        /// narrowing before its ring is restored, so a restore failure can be
        /// injected into the real rollback; null in play.
        /// </summary>
        internal static Action<AreaEffectView> NarrowFaultForQualification;

        /// <summary>
        /// Guarded runtime qualification only: runs as an instance's ending
        /// starts; it may throw (an ending that fails) or return false (an
        /// ending that does nothing). Null in play.
        /// </summary>
        internal static Func<AreaEffectView, bool> EndFaultForQualification;

        /// <summary>Guarded runtime qualification only: an instance whose area data reads as unavailable; null in play.</summary>
        internal static Func<AreaEffectView, bool> DataUnavailableForQualification;

        /// <summary>Guarded runtime qualification only: an instance whose liveness read throws; null in play.</summary>
        internal static Func<AreaEffectView, bool> LivenessFaultForQualification;

        /// <summary>Whether a live instance may attempt widening: no instance of its group is unresolved and every other one is widened.</summary>
        internal static bool MayWiden(AreaEffectView view, UnitEntityData owner, string key)
        {
            if (view == null || owner == null || key == null)
                return false;
            lock (Gate)
            {
                Maintain(true);
                if (!IsLive(view))
                    return false;
                FavoredClassRangeGroup<AreaEffectView> group;
                return !Groups.TryGetValue(Name(owner.UniqueId, key), out group) || group.MayWiden(view);
            }
        }

        /// <summary>Records one instance's outcome and restores its group's invariant.</summary>
        internal static void Record(AreaEffectView view, UnitEntityData owner, string key,
            FavoredClassWideningOutcome outcome, int feet, float native)
        {
            if (view == null || owner == null || key == null)
                return;
            lock (Gate)
            {
                Maintain(true);
                // Only an instance verified no longer live is skipped; one
                // whose liveness cannot be read is recorded (its group then
                // tracks it as unresolved).
                bool live;
                try
                {
                    live = IsLive(view);
                }
                catch (Exception)
                {
                    live = true;
                }
                if (!live)
                    return;
                NativeRadii[view] = native;
                string name = Name(owner.UniqueId, key);
                FavoredClassRangeGroup<AreaEffectView> group;
                if (!Groups.TryGetValue(name, out group))
                    Groups[name] = group = new FavoredClassRangeGroup<AreaEffectView>(IsLive, VerifyNative, Narrow,
                        End, Diagnose);
                group.Record(view, outcome, feet);
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
                Maintain(false);
                FavoredClassRangeGroup<AreaEffectView> group;
                return Groups.TryGetValue(Name(unit.UniqueId, key), out group) ? group.Feet(configuredFeet) :
                    configuredFeet;
            }
        }

        /// <summary>The owner's tracked instances of the performance, unresolved ones included (qualification evidence).</summary>
        internal static int LiveCount(UnitDescriptor owner, string key)
        {
            UnitEntityData unit = owner == null ? null : owner.Unit;
            if (unit == null)
                return 0;
            lock (Gate)
            {
                Maintain(false);
                FavoredClassRangeGroup<AreaEffectView> group;
                return Groups.TryGetValue(Name(unit.UniqueId, key), out group) ? group.Count : 0;
            }
        }

        /// <summary>The owner's unresolved instances of the performance (qualification evidence).</summary>
        internal static int UnresolvedCount(UnitDescriptor owner, string key)
        {
            UnitEntityData unit = owner == null ? null : owner.Unit;
            if (unit == null)
                return 0;
            lock (Gate)
            {
                Maintain(false);
                FavoredClassRangeGroup<AreaEffectView> group;
                return Groups.TryGetValue(Name(unit.UniqueId, key), out group) ? group.UnresolvedCount : 0;
            }
        }

        /// <summary>One tracked instance's recorded outcome (qualification evidence).</summary>
        internal static FavoredClassWideningOutcome? OutcomeOf(UnitDescriptor owner, string key, AreaEffectView view)
        {
            UnitEntityData unit = owner == null ? null : owner.Unit;
            if (unit == null)
                return null;
            lock (Gate)
            {
                Maintain(false);
                FavoredClassRangeGroup<AreaEffectView> group;
                return Groups.TryGetValue(Name(unit.UniqueId, key), out group) ? group.OutcomeOf(view) : null;
            }
        }

        /// <summary>Why one tracked instance is unresolved, or null (qualification evidence).</summary>
        internal static string ProblemOf(UnitDescriptor owner, string key, AreaEffectView view)
        {
            UnitEntityData unit = owner == null ? null : owner.Unit;
            if (unit == null)
                return null;
            lock (Gate)
            {
                Maintain(false);
                FavoredClassRangeGroup<AreaEffectView> group;
                return Groups.TryGetValue(Name(unit.UniqueId, key), out group) ? group.ProblemOf(view) : null;
            }
        }

        /// <summary>The most recent unresolved and resolved diagnostics (qualification evidence).</summary>
        internal static string[] RecentDiagnostics()
        {
            lock (Gate)
            {
                return Diagnostics.ToArray();
            }
        }

        private static string Name(string owner, string key)
        {
            return owner + "|" + key;
        }

        /// <summary>
        /// Every group forgets the instances verified no longer live (a
        /// read-only point for the descriptions and evidence); a mechanics
        /// point (a widening attempt or a recording) also retries every
        /// unresolved instance. Empty groups and untracked radii are dropped.
        /// </summary>
        private static void Maintain(bool settle)
        {
            foreach (KeyValuePair<string, FavoredClassRangeGroup<AreaEffectView>> pair in Groups.ToArray())
            {
                if (settle)
                    pair.Value.Settle();
                else
                    pair.Value.Purge();
                if (pair.Value.Count == 0)
                    Groups.Remove(pair.Key);
            }
            foreach (AreaEffectView view in NativeRadii.Keys.ToArray())
                if (!Groups.Values.Any(group => group.Contains(view)))
                    NativeRadii.Remove(view);
        }

        /// <summary>
        /// Whether the instance may still be live: false only for a destroyed
        /// view or an ended or destroyed area; unavailable area data counts as
        /// live. It throws when the read itself fails.
        /// </summary>
        private static bool IsLive(AreaEffectView view)
        {
            if (view == null)
                return false;
            Func<AreaEffectView, bool> fault = LivenessFaultForQualification;
            if (fault != null && fault(view))
                throw new InvalidOperationException("KMG injected liveness read failure");
            AreaEffectEntityData data = DataOf(view);
            return data == null || (!data.Destroyed && !data.IsEnded);
        }

        private static AreaEffectEntityData DataOf(AreaEffectView view)
        {
            Func<AreaEffectView, bool> unavailable = DataUnavailableForQualification;
            if (unavailable != null && unavailable(view))
                return null;
            return view.Data as AreaEffectEntityData;
        }

        private static void Diagnose(AreaEffectView view, string message)
        {
            string area = "unknown";
            try
            {
                AreaEffectEntityData data = view == null ? null : view.Data as AreaEffectEntityData;
                if (data != null && data.Blueprint != null)
                    area = data.Blueprint.name + "#" + data.UniqueId;
            }
            catch (Exception)
            {
                // The diagnostic keeps "unknown".
            }
            string line = "area=" + area + ";" + message;
            Diagnostics.Add(line);
            if (Diagnostics.Count > DiagnosticCapacity)
                Diagnostics.RemoveAt(0);
            Bootstrap.ModContext context;
            if (Bootstrap.ModContext.TryGet(out context))
                context.Logger.Warning("favored-class", message.StartsWith("resolved", StringComparison.Ordinal)
                    ? "performance-range.resolved" : "performance-range.unresolved", line);
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
        /// an older, lingering area never stops the current performance. The
        /// area runs under a clone of its buff's context (CloneFor), so the
        /// buff is found among the ancestors of the area's context. Without
        /// area data nothing can be ended, which throws; the group verifies
        /// every ending with IsLive.
        /// </summary>
        private static void End(AreaEffectView view)
        {
            AreaEffectEntityData data = view == null ? null : DataOf(view);
            if (data == null)
                throw new InvalidOperationException("the area's data is unavailable, so it cannot be ended");
            Func<AreaEffectView, bool> fault = EndFaultForQualification;
            if (fault != null && !fault(view))
                return;
            MechanicsContext context = data.Context;
            UnitEntityData owner = context == null ? null : context.MaybeCaster;
            data.ForceEnd();
            if (owner == null || AppliedBuff == null)
                return;
            foreach (ActivatableAbility toggle in owner.Descriptor.ActivatableAbilities.Enumerable.ToArray())
            {
                var buff = toggle.IsOn ? AppliedBuff.GetValue(toggle) as Buff : null;
                if (buff != null && FavoredClassContextLineage.Descends(context, buff.Context,
                        value => value.ParentContext))
                    toggle.IsOn = false;
            }
        }
    }
}
