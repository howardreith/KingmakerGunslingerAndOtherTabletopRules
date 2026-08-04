using System;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using KingmakerGunslinger.Assets;

namespace KingmakerGunslinger.Blueprints
{
    internal static class FirearmProjectileBlueprints
    {
        internal static BlueprintProjectile Projectile { get; private set; }
        private const BindingFlags Fields = BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic;

        internal static BlueprintProjectile Register(BlueprintRegistry registry,
            BlueprintWeaponType nativeLightCrossbow)
        {
            object visual = nativeLightCrossbow.GetType().GetField(
                "m_VisualParameters", Fields).GetValue(nativeLightCrossbow);
            var projectiles = (BlueprintProjectile[])visual.GetType().GetField(
                "m_Projectiles", Fields).GetValue(visual);
            if (projectiles == null || projectiles.Length == 0 || projectiles[0] == null)
                throw new InvalidOperationException("Native crossbow projectile source is unavailable.");
            BlueprintProjectile source = projectiles[0];
            Projectile = registry.Register<BlueprintProjectile>("KMG.Firearms.Projectile",
                delegate
                {
                    BlueprintProjectile result = BlueprintCloneService.Clone(source,
                        "KMG_FirearmProjectile");
                    result.DeflectedArrowPrefab = null;
                    return result;
                });
            return Projectile;
        }
    }
}
