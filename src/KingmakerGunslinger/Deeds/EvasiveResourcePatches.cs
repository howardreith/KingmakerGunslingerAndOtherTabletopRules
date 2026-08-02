using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic;

namespace KingmakerGunslinger.Deeds
{
    [HarmonyPatch]
    internal static class EvasiveResourceMutationPatch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            return typeof(UnitAbilityResourceCollection).GetMethods(
                BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.DeclaredOnly).Where(method =>
                (method.Name == "Spend" || method.Name == "Restore") &&
                method.ReturnType == typeof(void) &&
                method.GetParameters().Length >= 1 &&
                method.GetParameters()[0].ParameterType ==
                    typeof(BlueprintScriptableObject));
        }

        private static void Postfix(BlueprintScriptableObject blueprint,
            UnitDescriptor ___m_Owner)
        {
            EvasiveRuntime.Refresh(___m_Owner, blueprint);
        }
    }
}
