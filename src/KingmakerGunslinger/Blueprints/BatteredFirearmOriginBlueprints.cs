using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Ecnchantments;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Gunsmithing;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static class BatteredFirearmOriginBlueprints
    {
        internal const string Symbol = "KMG.Gunsmithing.BatteredOrigin";

        internal static BlueprintWeaponEnchantment Register(
            BlueprintRegistry registry)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            return registry.Register<BlueprintWeaponEnchantment>(Symbol,
                delegate
                {
                    var blueprint = ScriptableObject.CreateInstance<
                        BlueprintWeaponEnchantment>();
                    blueprint.name = "KMG_BatteredFirearm_Origin";
                    var marker = BatteredFirearmOriginComponent.Create();
                    marker.name = "$KMG_BatteredFirearm_Origin";
                    blueprint.ComponentsArray = new BlueprintComponent[] { marker };
                    return blueprint;
                });
        }
    }
}
