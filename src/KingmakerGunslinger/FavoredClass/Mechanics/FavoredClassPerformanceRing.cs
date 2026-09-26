using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace KingmakerGunslinger.FavoredClass.Mechanics
{
    /// <summary>
    /// O01 visible boundary. A performance's ring effect is authored at the
    /// native radius (its emitter circles and ring meshes sit at the area's
    /// Size), so one owner's widened area scales that one spawned effect
    /// horizontally by owner radius / native radius. Particle systems in
    /// Local scaling mode ignore every parent, so each is scaled on its own
    /// transform; a Hierarchy or Shape system inherits a scaled ancestor
    /// system and is scaled only when it has none. Only the axes that lie
    /// horizontally in the world are scaled. Pooled effects are reused
    /// without any reset, so the exact previous local scales are recorded
    /// and restored before the instance returns to the pool: no other
    /// performer ever receives this owner's ring.
    /// </summary>
    internal static class FavoredClassPerformanceRing
    {
        private sealed class Record
        {
            internal GameObject Effect;
            internal float Factor;
            internal readonly List<KeyValuePair<Transform, Vector3>> Scales =
                new List<KeyValuePair<Transform, Vector3>>();
        }

        private const int LocalScalingMode = 1;
        private static readonly Type ParticleSystemType = FindType("UnityEngine.ParticleSystem");
        private static readonly PropertyInfo MainProperty = ParticleSystemType == null ? null :
            ParticleSystemType.GetProperty("main");
        private static readonly Dictionary<int, Record> Scaled = new Dictionary<int, Record>();

        /// <summary>Whether the Unity particle members this needs exist in this process.</summary>
        internal static bool Available
        {
            get { return ParticleSystemType != null && MainProperty != null; }
        }

        /// <summary>Instances currently scaled (released instances are forgotten).</summary>
        internal static int ScaledCount
        {
            get
            {
                Purge();
                return Scaled.Count;
            }
        }

        /// <summary>Whether this effect instance currently carries an owner scale.</summary>
        internal static bool IsScaled(GameObject effect)
        {
            if (effect == null) return false;
            Purge();
            return Scaled.ContainsKey(effect.GetInstanceID());
        }

        /// <summary>The factor applied to an effect instance, or 1 when it is native.</summary>
        internal static float FactorOf(GameObject effect)
        {
            Record record;
            return effect != null && Scaled.TryGetValue(effect.GetInstanceID(), out record) ? record.Factor : 1f;
        }

        /// <summary>
        /// Scales one spawned ring effect; returns the number of particle
        /// systems whose own transform was scaled (0 when nothing applies).
        /// </summary>
        internal static int Scale(GameObject effect, float factor)
        {
            if (effect == null || !Available || factor <= 0f || Math.Abs(factor - 1f) < 0.0001f)
                return 0;
            Purge();
            int id = effect.GetInstanceID();
            if (Scaled.ContainsKey(id))
                throw new InvalidOperationException("A performance ring was scaled twice.");
            var record = new Record { Effect = effect, Factor = factor };
            var scaledTransforms = new HashSet<Transform>();
            Component[] systems = effect.GetComponentsInChildren(ParticleSystemType, true);
            try
            {
                foreach (Component system in systems)
                {
                    Transform transform = system.transform;
                    int mode = Convert.ToInt32(ReadMode(system));
                    if (mode != LocalScalingMode && HasScaledAncestor(transform, effect.transform, scaledTransforms))
                        continue;
                    Vector3 before = transform.localScale;
                    transform.localScale = Horizontal(transform, before, factor);
                    record.Scales.Add(new KeyValuePair<Transform, Vector3>(transform, before));
                    scaledTransforms.Add(transform);
                }
            }
            catch
            {
                Restore(record);
                throw;
            }
            Scaled[id] = record;
            return record.Scales.Count;
        }

        /// <summary>
        /// Restores a scaled instance exactly; returns whether it is native
        /// afterwards. Every recorded transform is restored independently; if
        /// any cannot be, the record is kept, so the instance still reports
        /// scaled and is never taken for native.
        /// </summary>
        internal static bool Restore(GameObject effect)
        {
            if (effect == null) return true;
            int id = effect.GetInstanceID();
            Record record;
            if (!Scaled.TryGetValue(id, out record))
                return true;
            if (!Restore(record))
                return false;
            Scaled.Remove(id);
            return true;
        }

        private static bool Restore(Record record)
        {
            bool complete = true;
            for (int index = record.Scales.Count - 1; index >= 0; index--)
            {
                Transform transform = record.Scales[index].Key;
                try
                {
                    if (transform != null)
                        transform.localScale = record.Scales[index].Value;
                }
                catch (Exception)
                {
                    complete = false;
                }
            }
            return complete;
        }

        /// <summary>Forgets instances Unity destroyed without releasing them to the pool.</summary>
        private static void Purge()
        {
            foreach (int id in Scaled.Where(pair => pair.Value.Effect == null).Select(pair => pair.Key).ToArray())
                Scaled.Remove(id);
        }

        private static object ReadMode(Component system)
        {
            object main = MainProperty.GetValue(system, null);
            return main.GetType().GetProperty("scalingMode").GetValue(main, null);
        }

        private static bool HasScaledAncestor(Transform transform, Transform root, HashSet<Transform> scaled)
        {
            for (Transform cursor = transform.parent; cursor != null; cursor = cursor.parent)
            {
                if (scaled.Contains(cursor)) return true;
                if (cursor == root) break;
            }
            return false;
        }

        /// <summary>Multiplies every local axis that lies horizontally in the world.</summary>
        private static Vector3 Horizontal(Transform transform, Vector3 scale, float factor)
        {
            Func<Vector3, float> multiplier = axis =>
                Math.Abs((transform.rotation * axis).y) < 0.7071f ? factor : 1f;
            return new Vector3(scale.x * multiplier(Vector3.right), scale.y * multiplier(Vector3.up),
                scale.z * multiplier(Vector3.forward));
        }

        private static Type FindType(string fullName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(fullName, false);
                if (type != null) return type;
            }
            return null;
        }
    }
}
