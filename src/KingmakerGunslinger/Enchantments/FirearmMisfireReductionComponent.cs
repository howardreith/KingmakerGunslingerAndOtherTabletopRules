using Kingmaker.Blueprints.Items.Ecnchantments;
using UnityEngine;

namespace KingmakerGunslinger.Enchantments
{
    internal sealed class FirearmMisfireReductionComponent : WeaponEnchantmentLogic
    {
        internal int Reduction;

        internal static FirearmMisfireReductionComponent Create(int reduction)
        {
            FirearmMisfireReductionComponent value =
                ScriptableObject.CreateInstance<FirearmMisfireReductionComponent>();
            value.Reduction = reduction;
            return value;
        }
    }
}
