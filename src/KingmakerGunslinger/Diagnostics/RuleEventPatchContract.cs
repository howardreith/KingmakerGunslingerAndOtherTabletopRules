using System;
using System.Reflection;

namespace KingmakerGunslinger.Diagnostics
{
    /// <summary>
    /// Defines the exact Kingmaker rule-event callback shape accepted by Harmony
    /// patch target resolution. Keeping the reflection predicate independent of
    /// Kingmaker types makes the contract executable in the dependency-free suite.
    /// </summary>
    internal static class RuleEventPatchContract
    {
        internal static bool IsCompatibleOnTrigger(
            MethodInfo method,
            Type eventContextType)
        {
            if (eventContextType == null)
            {
                throw new ArgumentNullException("eventContextType");
            }

            if (method == null ||
                method.IsStatic ||
                method.IsGenericMethodDefinition ||
                !string.Equals(method.Name, "OnTrigger", StringComparison.Ordinal) ||
                method.ReturnType != typeof(void))
            {
                return false;
            }

            ParameterInfo[] parameters = method.GetParameters();
            return parameters.Length == 1 &&
                parameters[0].ParameterType == eventContextType;
        }
    }
}
