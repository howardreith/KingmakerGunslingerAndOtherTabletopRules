using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using Kingmaker.View;
using UnityEngine;

namespace KingmakerGunslinger.ElementalRaces
{
    /// <summary>
    /// Rejects non-finite location data before a command can commit Hydraulic
    /// Push's shared use. Native range and navigation checks still decide
    /// whether a real point is reachable ground; a unit-occupied ground point
    /// remains a legal location.
    /// </summary>
    [Serializable]
    public sealed class ElementalTritonPortalGroundTargetChecker :
        BlueprintComponent, IAbilityTargetChecker
    {
        public bool CanTarget(UnitEntityData caster, TargetWrapper target)
        {
            if (caster == null || target == null) return false;
            Vector3 point = target.Point;
            if (float.IsNaN(point.x) || float.IsInfinity(point.x) ||
                float.IsNaN(point.y) || float.IsInfinity(point.y) ||
                float.IsNaN(point.z) || float.IsInfinity(point.z))
                return false;
            Vector3 projected = ObstacleAnalyzer.GetNearestNode(point)
                .clampedPosition;
            return !float.IsNaN(projected.x) &&
                !float.IsInfinity(projected.x) &&
                !float.IsNaN(projected.y) &&
                !float.IsInfinity(projected.y) &&
                !float.IsNaN(projected.z) &&
                !float.IsInfinity(projected.z) &&
                Mathf.Abs(projected.y - point.y) <= 1f;
        }
    }

    /// <summary>
    /// Keeps every Hydraulic Maneuver variant and Triton Portal inert unless
    /// the caster still has the exact project Undine race, active racial
    /// Hydraulic Push provider, and its shared daily use.
    /// </summary>
    [Serializable]
    public sealed class ElementalHydraulicSharedResourceAvailability :
        BlueprintComponent, IAbilityAvailabilityProvider
    {
        public BlueprintRace Undine;
        public BlueprintFeature HydraulicPushFeature;
        public BlueprintAbility HydraulicPushAbility;
        public BlueprintAbilityResource Resource;

        public bool IsAvailableFor(AbilityData ability)
        {
            if (ability == null || ability.Caster == null || Undine == null ||
                HydraulicPushFeature == null || HydraulicPushAbility == null ||
                Resource == null || ability.Caster.Progression == null ||
                !ReferenceEquals(ability.Caster.Progression.Race, Undine) ||
                !ability.Caster.HasFact(HydraulicPushFeature) ||
                ability.Caster.Abilities.GetAbility(HydraulicPushAbility) ==
                    null)
                return false;
            return ability.Caster.Resources != null &&
                ability.Caster.Resources.GetResourceAmount(Resource) > 0;
        }

        public string GetReason()
        {
            return "Requires an active racial Hydraulic Push and its shared daily use.";
        }
    }

    /// <summary>
    /// Deep-clones only the native component/action graph selected for Triton
    /// Portal. Blueprint references remain references; mutable component and
    /// action instances are project-owned, so configuring the 1d3 branch can
    /// never mutate the native donor or another module.
    /// </summary>
    internal static class ElementalUndineNativeComponentClone
    {
        internal static BlueprintComponent Clone(BlueprintComponent source)
        {
            if (source == null) throw new ArgumentNullException("source");
            return (BlueprintComponent)Clone(source,
                new Dictionary<object, object>(ReferenceComparer.Instance));
        }

        private static object Clone(object source,
            IDictionary<object, object> seen)
        {
            if (source == null) return null;
            Type type = source.GetType();
            if (type == typeof(ActionList))
            {
                ActionList actions = (ActionList)source;
                return new ActionList
                {
                    Actions = (actions.Actions ?? Array.Empty<GameAction>())
                        .Select(value => (GameAction)Clone(value, seen))
                        .ToArray()
                };
            }
            if (type.IsPrimitive || type.IsEnum || type == typeof(string) ||
                type == typeof(decimal) || type.IsValueType)
                return source;
            if (source is BlueprintScriptableObject) return source;
            UnityEngine.Object unity = source as UnityEngine.Object;
            if (unity != null && !(source is BlueprintComponent) &&
                !(source is GameAction)) return unity;
            object existing;
            if (seen.TryGetValue(source, out existing)) return existing;
            Array array = source as Array;
            if (array != null)
            {
                Array copy = Array.CreateInstance(type.GetElementType(),
                    array.Length);
                seen.Add(source, copy);
                for (int index = 0; index < array.Length; index++)
                    copy.SetValue(Clone(array.GetValue(index), seen), index);
                return copy;
            }
            object result = source is BlueprintComponent ||
                source is GameAction ? ScriptableObject.CreateInstance(type) :
                FormatterServices.GetUninitializedObject(type);
            seen.Add(source, result);
            foreach (FieldInfo field in Fields(type))
            {
                if (field.IsInitOnly ||
                    field.DeclaringType == typeof(UnityEngine.Object))
                    continue;
                field.SetValue(result, Clone(field.GetValue(source), seen));
            }
            return result;
        }

        private static IEnumerable<FieldInfo> Fields(Type type)
        {
            const BindingFlags flags = BindingFlags.Instance |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly;
            for (Type current = type; current != null &&
                current != typeof(UnityEngine.Object); current = current.BaseType)
                foreach (FieldInfo field in current.GetFields(flags))
                    yield return field;
        }

        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            internal static readonly ReferenceComparer Instance =
                new ReferenceComparer();

            public new bool Equals(object x, object y)
            {
                return ReferenceEquals(x, y);
            }

            public int GetHashCode(object value)
            {
                return RuntimeHelpers.GetHashCode(value);
            }
        }
    }
}
