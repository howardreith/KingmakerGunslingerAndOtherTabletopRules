using System;
using System.Linq;
using System.Reflection;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;

namespace KingmakerGunslinger.BrownFur
{
    internal sealed class CotwSharedSpellsBridge
    {
        private CotwSharedSpellsBridge(MethodInfo canShareSpell,
            MethodInfo isValidShareSpellTarget)
        {
            CanShareSpell = canShareSpell;
            IsValidShareSpellTarget = isValidShareSpellTarget;
        }

        internal MethodInfo CanShareSpell { get; private set; }
        internal MethodInfo IsValidShareSpellTarget { get; private set; }

        internal string[] Signatures
        {
            get
            {
                return new[] { Describe(CanShareSpell),
                    Describe(IsValidShareSpellTarget) };
            }
        }

        internal static bool TryResolve(Assembly assembly,
            out CotwSharedSpellsBridge bridge)
        {
            bridge = null;
            Type type = assembly == null ? null : assembly.GetType(
                "CallOfTheWild.SharedSpells", false, false);
            MethodInfo canShare = Exact(type, "canShareSpell",
                new[] { typeof(AbilityData) });
            MethodInfo validTarget = Exact(type, "isValidShareSpellTarget",
                new[] { typeof(UnitEntityData), typeof(UnitDescriptor) });
            if (canShare == null || validTarget == null) return false;
            bridge = new CotwSharedSpellsBridge(canShare, validTarget);
            return true;
        }

        private static MethodInfo Exact(Type type, string name, Type[] parameters)
        {
            if (type == null) return null;
            MethodInfo[] matches = type.GetMethods(BindingFlags.Static |
                BindingFlags.Public | BindingFlags.NonPublic).Where(value =>
                    string.Equals(value.Name, name, StringComparison.Ordinal) &&
                    value.ReturnType == typeof(bool) && !value.IsGenericMethod &&
                    ParametersEqual(value.GetParameters(), parameters)).ToArray();
            return matches.Length == 1 ? matches[0] : null;
        }

        private static bool ParametersEqual(ParameterInfo[] actual,
            Type[] expected)
        {
            return actual.Length == expected.Length && actual.Select(
                (value, index) => value.ParameterType == expected[index]).All(
                    value => value);
        }

        private static string Describe(MethodInfo method)
        {
            return method == null ? "<unavailable>" : method.ReturnType.FullName +
                " " + method.DeclaringType.FullName + "." + method.Name + "(" +
                string.Join(",", method.GetParameters().Select(value =>
                    value.ParameterType.FullName).ToArray()) + ")";
        }
    }
}
