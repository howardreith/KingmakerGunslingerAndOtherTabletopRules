using System;
using System.Reflection;

namespace KingmakerGunslinger.Misfires
{
    /// <summary>
    /// Exact reflection predicates for the two Kingmaker 2.1.7b methods used by
    /// Sprint 23. These predicates are dependency-free so contract regressions are
    /// executable in the domain-test harness.
    /// </summary>
    internal static class FirearmMisfirePatchContract
    {
        internal static bool IsCompatibleRollSetter(
            MethodInfo method,
            Type ruleAttackRollType,
            Type rollEntryType)
        {
            if (ruleAttackRollType == null)
            {
                throw new ArgumentNullException("ruleAttackRollType");
            }

            if (rollEntryType == null)
            {
                throw new ArgumentNullException("rollEntryType");
            }

            if (method == null ||
                method.DeclaringType != ruleAttackRollType ||
                method.IsStatic ||
                method.IsGenericMethodDefinition ||
                !method.IsPrivate ||
                !method.IsSpecialName ||
                !string.Equals(method.Name, "set_Roll", StringComparison.Ordinal) ||
                method.ReturnType != typeof(void))
            {
                return false;
            }

            ParameterInfo[] parameters = method.GetParameters();
            return parameters.Length == 1 &&
                parameters[0].ParameterType == rollEntryType;
        }

        internal static bool IsCompatibleSuccessRoll(
            MethodInfo method,
            Type ruleAttackRollType)
        {
            if (ruleAttackRollType == null)
            {
                throw new ArgumentNullException("ruleAttackRollType");
            }

            if (method == null ||
                method.DeclaringType != ruleAttackRollType ||
                method.IsStatic ||
                method.IsGenericMethodDefinition ||
                !method.IsPublic ||
                !string.Equals(method.Name, "IsSuccessRoll", StringComparison.Ordinal) ||
                method.ReturnType != typeof(bool))
            {
                return false;
            }

            ParameterInfo[] parameters = method.GetParameters();
            return parameters.Length == 1 &&
                parameters[0].ParameterType == typeof(int);
        }
    }
}
