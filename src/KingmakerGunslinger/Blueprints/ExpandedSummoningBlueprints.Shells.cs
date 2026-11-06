using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Ecnchantments;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningBlueprints
    {
        // Kept separate only so the real dispatch and registry can execute in
        // the offline fixture with the Unity/engine boundary substituted.
        internal static BlueprintScriptableObject RegisterAdditionalShell(
            string symbol, string plannedType, BlueprintRegistry registry)
        {
            if (plannedType == "BlueprintWeaponEnchantment")
                return registry.Register<BlueprintWeaponEnchantment>(symbol, () =>
                {
                    var value = ScriptableObject.CreateInstance<BlueprintWeaponEnchantment>();
                    value.name = InternalName(symbol);
                    return value;
                });
            throw new InvalidOperationException(
                "Unsupported Expanded Summoning planned type " + plannedType + ".");
        }

        private static string InternalName(string symbol)
        { return symbol.Replace('.', '_').Replace('-', '_'); }
    }
}
