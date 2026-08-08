using System;
using System.Linq;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Items;

namespace KingmakerGunslinger.Enchantments
{
    internal static class FirearmMisfireReductionResolver
    {
        private static BlueprintWeaponEnchantment _reliable;

        internal static void Configure(BlueprintWeaponEnchantment reliable)
        {
            ReliableBlueprints.Validate(reliable);
            _reliable = reliable;
        }

        internal static int Resolve(ItemEntityWeapon weapon)
        {
            try
            {
                BlueprintWeaponEnchantment reliable = _reliable;
                if (reliable == null || weapon == null || weapon.Blueprint == null ||
                    !weapon.Blueprint.IsRanged || weapon.Enchantments == null)
                    return 0;
                int matches = 0;
                foreach (ItemEnchantment enchantment in weapon.Enchantments)
                {
                    BlueprintWeaponEnchantment blueprint = enchantment == null ? null :
                        enchantment.Blueprint as BlueprintWeaponEnchantment;
                    FirearmMisfireReductionComponent[] markers = blueprint == null ||
                        blueprint.ComponentsArray == null ?
                        new FirearmMisfireReductionComponent[0] :
                        blueprint.ComponentsArray.OfType<FirearmMisfireReductionComponent>().ToArray();
                    if (blueprint == reliable)
                    {
                        if (markers == null || markers.Length != 1 || markers[0].Reduction != 1)
                            return 0;
                        matches++;
                    }
                    else if (markers != null && markers.Length != 0)
                        return 0;
                }
                return matches == 1 ? 1 : 0;
            }
            catch
            {
                return 0;
            }
        }
    }
}
