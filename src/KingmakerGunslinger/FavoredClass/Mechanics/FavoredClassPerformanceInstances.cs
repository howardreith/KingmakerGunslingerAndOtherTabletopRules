using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.SriptZones;
using UnityEngine;

namespace KingmakerGunslinger.FavoredClass.Mechanics
{
    /// <summary>
    /// O01: the actual widening outcome of every live performance area
    /// instance, per owner and target, recorded by the widening transaction
    /// itself (at initialization and when a late ring spawns). The owner's
    /// descriptions read it (FavoredClassRangePresentation), so they state
    /// the range the owner's live areas actually have. A failure narrows every
    /// other live instance of the same owner and target to its native radius
    /// and ring, so all of that owner's live areas and every description of
    /// the target agree.
    /// </summary>
    internal static class FavoredClassPerformanceInstances
    {
        private sealed class Instance
        {
            internal AreaEffectView View;
            internal string Owner;
            internal string Key;
            internal FavoredClassWideningOutcome Outcome;
            internal int Feet;
            internal float Native;
        }

        private static readonly FieldInfo SpawnedFx = typeof(AreaEffectView).GetField("m_SpawnedFx",
            BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly object Gate = new object();
        private static readonly List<Instance> Live = new List<Instance>();
        private static readonly Dictionary<string, FavoredClassWideningOutcome> LastCompleted =
            new Dictionary<string, FavoredClassWideningOutcome>(StringComparer.Ordinal);

        /// <summary>Records one instance's widening outcome (a later outcome of the same instance replaces it).</summary>
        internal static void Record(AreaEffectView view, UnitEntityData owner, string key,
            FavoredClassWideningOutcome outcome, int feet, float native)
        {
            if (view == null || owner == null || key == null)
                return;
            string ownerId = owner.UniqueId;
            List<Instance> narrow = null;
            lock (Gate)
            {
                Purge();
                Instance entry = Live.FirstOrDefault(value => ReferenceEquals(value.View, view));
                if (entry == null)
                {
                    entry = new Instance { View = view, Owner = ownerId, Key = key };
                    Live.Add(entry);
                }
                entry.Outcome = outcome;
                entry.Feet = feet;
                entry.Native = native;
                string memory = Memory(ownerId, key);
                FavoredClassWideningOutcome? next = FavoredClassRangePresentation.NextLastCompleted(
                    LastOutcome(memory), outcome);
                if (next.HasValue)
                    LastCompleted[memory] = next.Value;
                if (outcome == FavoredClassWideningOutcome.Failed)
                    narrow = Live.Where(value => !ReferenceEquals(value, entry) && value.Owner == ownerId &&
                        value.Key == key && FavoredClassRangePresentation.IsWidened(value.Outcome)).ToList();
            }
            if (narrow != null)
                foreach (Instance sibling in narrow)
                    Narrow(sibling);
        }

        /// <summary>The range the owner's descriptions of the target show, in feet, or null for native text.</summary>
        internal static int? Feet(UnitDescriptor owner, string key, int configuredFeet)
        {
            UnitEntityData unit = owner == null ? null : owner.Unit;
            if (unit == null || key == null)
                return configuredFeet;
            var live = new List<FavoredClassLiveRange>();
            FavoredClassWideningOutcome? last;
            lock (Gate)
            {
                Purge();
                foreach (Instance value in Live)
                    if (value.Owner == unit.UniqueId && value.Key == key)
                        live.Add(new FavoredClassLiveRange(value.Outcome, value.Feet));
                last = LastOutcome(Memory(unit.UniqueId, key));
            }
            return FavoredClassRangePresentation.Feet(live, last, configuredFeet);
        }

        /// <summary>The owner's live instances of the target (qualification evidence).</summary>
        internal static int LiveCount(UnitDescriptor owner, string key)
        {
            UnitEntityData unit = owner == null ? null : owner.Unit;
            if (unit == null)
                return 0;
            lock (Gate)
            {
                Purge();
                return Live.Count(value => value.Owner == unit.UniqueId && value.Key == key);
            }
        }

        private static string Memory(string owner, string key)
        {
            return owner + "|" + key;
        }

        private static FavoredClassWideningOutcome? LastOutcome(string memory)
        {
            FavoredClassWideningOutcome value;
            return LastCompleted.TryGetValue(memory, out value) ? value : (FavoredClassWideningOutcome?)null;
        }

        /// <summary>Forgets destroyed views and areas that ended.</summary>
        private static void Purge()
        {
            Live.RemoveAll(value => value.View == null || Ended(value.View));
        }

        private static bool Ended(AreaEffectView view)
        {
            var data = view.Data as AreaEffectEntityData;
            return data != null && (data.Destroyed || data.IsEnded);
        }

        /// <summary>Restores a widened sibling to its native radius and ring.</summary>
        private static void Narrow(Instance instance)
        {
            try
            {
                AreaEffectView view = instance.View;
                if (view == null)
                    return;
                var ring = SpawnedFx == null ? null : SpawnedFx.GetValue(view) as GameObject;
                if (ring != null)
                    FavoredClassPerformanceRing.Restore(ring);
                var cylinder = view.Shape as ScriptZoneCylinder;
                if (cylinder != null && instance.Native > 0f)
                    cylinder.Radius = instance.Native;
            }
            catch (Exception)
            {
                // Fail safe: the instance is reported native either way.
            }
            finally
            {
                lock (Gate)
                    instance.Outcome = FavoredClassWideningOutcome.Failed;
            }
        }
    }
}
