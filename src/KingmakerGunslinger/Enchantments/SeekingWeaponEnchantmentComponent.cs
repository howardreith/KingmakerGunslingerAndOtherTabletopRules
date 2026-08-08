using Kingmaker.Blueprints.Items.Ecnchantments;
using UnityEngine;

namespace KingmakerGunslinger.Enchantments
{
    /// <summary>Inert exact-blueprint marker for the project Seeking property.</summary>
    internal sealed class SeekingWeaponEnchantmentComponent : WeaponEnchantmentLogic
    {
        internal static SeekingWeaponEnchantmentComponent Create()
        {
            return ScriptableObject.CreateInstance<SeekingWeaponEnchantmentComponent>();
        }
    }
}
