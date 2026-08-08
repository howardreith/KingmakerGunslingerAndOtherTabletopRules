using System;
using System.Linq;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Items;

namespace KingmakerGunslinger.Enchantments
{
    internal static class SeekingExactItemResolver
    {
        private static BlueprintWeaponEnchantment _seeking;

        internal static void Configure(BlueprintWeaponEnchantment seeking)
        {
            SeekingBlueprints.Validate(seeking);
            _seeking = seeking;
        }

        internal static bool IsAuthorized(ItemEntityWeapon weapon)
        {
            try
            {
                BlueprintWeaponEnchantment seeking = _seeking;
                if (weapon == null || seeking == null || weapon.Blueprint == null ||
                    !weapon.Blueprint.IsRanged || weapon.Enchantments == null)
                {
                    return false;
                }

                int exact = 0;
                bool foreignMarker = false;
                foreach (ItemEnchantment enchantment in weapon.Enchantments)
                {
                    BlueprintWeaponEnchantment blueprint = enchantment == null
                        ? null : enchantment.Blueprint as BlueprintWeaponEnchantment;
                    if (blueprint == null) continue;
                    int markers = blueprint.ComponentsArray == null ? 0 :
                        blueprint.ComponentsArray.Count(component =>
                            component is SeekingWeaponEnchantmentComponent);
                    if (ReferenceEquals(blueprint, seeking))
                    {
                        if (markers != 1) return false;
                        exact++;
                    }
                    else if (markers != 0)
                    {
                        foreignMarker = true;
                    }
                }

                return exact == 1 && !foreignMarker;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
