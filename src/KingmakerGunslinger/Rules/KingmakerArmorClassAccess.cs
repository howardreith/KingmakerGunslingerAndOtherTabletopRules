using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using KingmakerGunslinger.Development;

namespace KingmakerGunslinger.Rules
{
    /// <summary>
    /// Strict reflection adapter for the small installed-assembly surface needed by
    /// Sprint 9. It fails closed unless RuleCalculateAC exposes one writable Int32
    /// TargetAC member and the target exposes ordinary and touch AC values.
    /// </summary>
    internal static class KingmakerArmorClassAccess
    {
        private static readonly object AccessorGate = new object();
        private static readonly Dictionary<Type, Int32MemberAccessor> TargetAcAccessors =
            new Dictionary<Type, Int32MemberAccessor>();
        private static readonly HashSet<Type> UnsupportedTargetAcTypes =
            new HashSet<Type>();

        internal static bool TryReadParticipants(
            object ruleEvent,
            out object initiator,
            out object target)
        {
            initiator = null;
            target = null;
            if (ruleEvent == null)
            {
                return false;
            }

            string ignored;
            return ReflectionAccess.TryGetFirstNonNullMember(
                    ruleEvent,
                    new[] { "Initiator", "m_Initiator" },
                    out initiator,
                    out ignored) &&
                ReflectionAccess.TryGetFirstNonNullMember(
                    ruleEvent,
                    new[] { "Target", "m_Target" },
                    out target,
                    out ignored);
        }

        internal static bool TryReadDistanceMeters(
            object initiator,
            object target,
            out double distanceMeters)
        {
            distanceMeters = 0d;
            if (initiator == null || target == null)
            {
                return false;
            }

            object value;
            string ignored;
            if (!ReflectionAccess.TryInvokeAny(
                initiator,
                new[] { "DistanceTo" },
                new[] { new[] { target } },
                out value,
                out ignored))
            {
                return false;
            }

            return TryConvertFiniteNonNegativeDouble(value, out distanceMeters);
        }

        internal static bool TryReadTargetArmorClasses(
            object target,
            out int ordinaryArmorClass,
            out int touchArmorClass)
        {
            ordinaryArmorClass = 0;
            touchArmorClass = 0;
            if (target == null)
            {
                return false;
            }

            object ordinary;
            object touch;
            if (!TryGetFirstPath(
                    target,
                    new[]
                    {
                        "Stats.AC.ModifiedValue",
                        "Stats.AC.Value",
                        "Descriptor.Stats.AC.ModifiedValue"
                    },
                    out ordinary) ||
                !TryGetFirstPath(
                    target,
                    new[]
                    {
                        "Stats.AC.Touch",
                        "Stats.AC.TouchAC",
                        "Stats.AC.TouchValue",
                        "Descriptor.Stats.AC.Touch"
                    },
                    out touch))
            {
                return false;
            }

            return TryConvertExactInt32(ordinary, out ordinaryArmorClass) &&
                TryConvertExactInt32(touch, out touchArmorClass);
        }

        internal static bool TryReadTargetArmorClass(
            object ruleCalculateArmorClass,
            out int targetArmorClass,
            out string resolvedMember)
        {
            targetArmorClass = 0;
            resolvedMember = null;
            Int32MemberAccessor accessor;
            if (!TryGetTargetAcAccessor(ruleCalculateArmorClass, out accessor))
            {
                return false;
            }

            object value;
            if (!accessor.TryGet(ruleCalculateArmorClass, out value) ||
                !TryConvertExactInt32(value, out targetArmorClass))
            {
                return false;
            }

            resolvedMember = accessor.Description;
            return true;
        }

        internal static bool TryWriteTargetArmorClass(
            object ruleCalculateArmorClass,
            int targetArmorClass,
            out string resolvedMember)
        {
            resolvedMember = null;
            Int32MemberAccessor accessor;
            if (!TryGetTargetAcAccessor(ruleCalculateArmorClass, out accessor) ||
                !accessor.TrySet(ruleCalculateArmorClass, targetArmorClass))
            {
                return false;
            }

            resolvedMember = accessor.Description;
            return true;
        }

        private static bool TryGetTargetAcAccessor(
            object ruleCalculateArmorClass,
            out Int32MemberAccessor accessor)
        {
            accessor = null;
            if (ruleCalculateArmorClass == null)
            {
                return false;
            }

            Type type = ruleCalculateArmorClass.GetType();
            lock (AccessorGate)
            {
                if (TargetAcAccessors.TryGetValue(type, out accessor))
                {
                    return true;
                }

                if (UnsupportedTargetAcTypes.Contains(type))
                {
                    return false;
                }

                Int32MemberAccessor[] candidates = ResolveTargetAcAccessors(type).ToArray();
                if (candidates.Length != 1)
                {
                    UnsupportedTargetAcTypes.Add(type);
                    return false;
                }

                accessor = candidates[0];
                TargetAcAccessors.Add(type, accessor);
                return true;
            }
        }

        private static IEnumerable<Int32MemberAccessor> ResolveTargetAcAccessors(Type type)
        {
            var properties = new List<Int32MemberAccessor>();
            for (Type current = type; current != null; current = current.BaseType)
            {
                const BindingFlags Flags = BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly;
                PropertyInfo property = current.GetProperty("TargetAC", Flags);
                if (property != null &&
                    property.PropertyType == typeof(int) &&
                    property.GetIndexParameters().Length == 0 &&
                    property.GetGetMethod(true) != null &&
                    property.GetSetMethod(true) != null)
                {
                    properties.Add(new Int32MemberAccessor(property));
                }
            }

            if (properties.Count > 0)
            {
                return properties;
            }

            var fields = new List<Int32MemberAccessor>();
            string[] names = { "TargetAC", "m_TargetAC", "<TargetAC>k__BackingField" };
            for (Type current = type; current != null; current = current.BaseType)
            {
                const BindingFlags Flags = BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly;
                foreach (string name in names)
                {
                    FieldInfo field = current.GetField(name, Flags);
                    if (field != null &&
                        field.FieldType == typeof(int) &&
                        !field.IsInitOnly &&
                        !field.IsLiteral)
                    {
                        fields.Add(new Int32MemberAccessor(field));
                    }
                }
            }

            return fields;
        }

        private static bool TryGetFirstPath(
            object source,
            IEnumerable<string> paths,
            out object value)
        {
            value = null;
            foreach (string path in paths)
            {
                object candidate;
                if (ReflectionAccess.TryGetPath(source, path, out candidate) &&
                    candidate != null)
                {
                    value = candidate;
                    return true;
                }
            }

            return false;
        }

        private static bool TryConvertExactInt32(object value, out int result)
        {
            result = 0;
            if (value == null || value is bool)
            {
                return false;
            }

            Type type = value.GetType();
            if (type != typeof(byte) &&
                type != typeof(sbyte) &&
                type != typeof(short) &&
                type != typeof(ushort) &&
                type != typeof(int))
            {
                return false;
            }

            try
            {
                result = Convert.ToInt32(value, CultureInfo.InvariantCulture);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool TryConvertFiniteNonNegativeDouble(
            object value,
            out double result)
        {
            result = 0d;
            if (value == null || value is bool)
            {
                return false;
            }

            try
            {
                result = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                return !double.IsNaN(result) &&
                    !double.IsInfinity(result) &&
                    result >= 0d;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private sealed class Int32MemberAccessor
        {
            private readonly PropertyInfo _property;
            private readonly FieldInfo _field;

            internal Int32MemberAccessor(PropertyInfo property)
            {
                _property = property ?? throw new ArgumentNullException("property");
                Description = property.DeclaringType.FullName + "." + property.Name;
            }

            internal Int32MemberAccessor(FieldInfo field)
            {
                _field = field ?? throw new ArgumentNullException("field");
                Description = field.DeclaringType.FullName + "." + field.Name;
            }

            internal string Description { get; private set; }

            internal bool TryGet(object instance, out object value)
            {
                value = null;
                try
                {
                    value = _property != null
                        ? _property.GetValue(instance, null)
                        : _field.GetValue(instance);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            internal bool TrySet(object instance, int value)
            {
                try
                {
                    if (_property != null)
                    {
                        _property.SetValue(instance, value, null);
                    }
                    else
                    {
                        _field.SetValue(instance, value);
                    }

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }
}
