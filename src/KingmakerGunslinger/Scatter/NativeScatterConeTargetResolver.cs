using System;
using System.Collections.Generic;
using System.Reflection;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.Utility;
using UnityEngine;

namespace KingmakerGunslinger.Scatter
{
    /// <summary>
    /// Exact Kingmaker adapter for PnP Blunderbuss cone geometry. Native code
    /// remains authoritative for line of sight, corpulence, angle, and range.
    /// </summary>
    internal sealed class NativeScatterConeTargetResolver
    {
        private static readonly ScatterConeDistanceService Distance =
            new ScatterConeDistanceService();
        private static readonly MethodInfo NativeConePredicate =
            ResolveNativeConePredicate();

        internal UnitEntityData[] Resolve(UnitEntityData caster,
            UnitEntityData aimedTarget)
        {
            if (caster == null) throw new ArgumentNullException("caster");
            if (aimedTarget == null) throw new ArgumentNullException("aimedTarget");
            Vector3 offset = aimedTarget.EyePosition - caster.EyePosition;
            var direction = new Vector2(offset.x, offset.z);
            if (direction.sqrMagnitude <= 0f)
                throw new InvalidOperationException(
                    "Scatter cone direction is zero; no target may be inferred.");
            direction.Normalize();

            ScatterConeDistanceDecision distance = Distance.ResolveBlunderbuss(
                Firearms.FirearmDefinitions.CreateEarlyBlunderbuss());
            IEnumerable<UnitEntityData> candidates = GameHelper.GetTargetsAround(
                caster.Position, new Feet(distance.DistanceFeet), true, false);
            if (candidates == null)
                throw new InvalidOperationException(
                    "Kingmaker's native scatter candidate query returned null.");

            var result = new List<UnitEntityData>();
            var seen = new HashSet<UnitEntityData>();
            foreach (UnitEntityData candidate in candidates)
            {
                if (candidate == null || ReferenceEquals(candidate, caster) ||
                    !seen.Add(candidate)) continue;
                object native = NativeConePredicate.Invoke(null, new object[]
                {
                    caster, candidate, caster.EyePosition, direction,
                    distance.DistanceMeters
                });
                if (!(native is bool))
                    throw new InvalidOperationException(
                        "Kingmaker's native cone predicate returned a non-Boolean value.");
                if ((bool)native) result.Add(candidate);
            }
            return result.ToArray();
        }

        private static MethodInfo ResolveNativeConePredicate()
        {
            MethodInfo method = typeof(AbilityDeliverProjectile).GetMethod(
                "WouldTargetUnitCone", BindingFlags.Static | BindingFlags.NonPublic,
                null, new[] { typeof(UnitEntityData), typeof(UnitEntityData),
                    typeof(Vector3), typeof(Vector2), typeof(float) }, null);
            if (method == null || method.ReturnType != typeof(bool))
                throw new InvalidOperationException(
                    "Exact native WouldTargetUnitCone contract is unavailable.");
            return method;
        }
    }
}
