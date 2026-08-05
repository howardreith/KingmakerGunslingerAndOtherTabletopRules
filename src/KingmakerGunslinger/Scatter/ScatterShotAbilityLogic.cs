using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Scatter
{
    [Serializable]
    public sealed class ScatterShotAbilityLogic : AbilityDeliverProjectile,
        IAbilityAvailabilityProvider
    {
        internal static ScatterShotAbilityLogic Create(
            AbilityDeliverProjectile nativeCone)
        {
            if (nativeCone == null) throw new ArgumentNullException("nativeCone");
            if (nativeCone.Type != AbilityProjectileType.Cone ||
                nativeCone.Projectiles == null || nativeCone.Projectiles.Length == 0 ||
                nativeCone.Length.Value != 15 || nativeCone.NeedAttackRoll)
                throw new ArgumentException(
                    "Scatter Shot requires the exact native 15-foot, no-attack-roll cone presentation.",
                    "nativeCone");

            var result = UnityEngine.ScriptableObject.CreateInstance<
                ScatterShotAbilityLogic>();
            result.name = "$KMG_ScatterShot_NativeConeDelivery";
            // AbilityDeliverProjectile exposes its complete serialized presentation
            // contract as public fields in the installed Kingmaker build. Copy the
            // proven Burning Hands values rather than attempting to reconstruct the
            // cone projectile, timing, or line-width contract independently.
            result.Projectiles = (BlueprintProjectile[])
                nativeCone.Projectiles.Clone();
            result.Type = nativeCone.Type;
            result.IsHandOfTheApprentice = nativeCone.IsHandOfTheApprentice;
            result.Length = nativeCone.Length;
            result.LineWidth = nativeCone.LineWidth;
            result.NeedAttackRoll = nativeCone.NeedAttackRoll;
            result.Weapon = nativeCone.Weapon;
            result.ReplaceAttackRollBonusStat =
                nativeCone.ReplaceAttackRollBonusStat;
            result.AttackRollBonusStat = nativeCone.AttackRollBonusStat;
            result.UseMaxProjectilesCount = nativeCone.UseMaxProjectilesCount;
            result.MaxProjectilesCountRank = nativeCone.MaxProjectilesCountRank;
            result.DelayBetweenProjectiles = nativeCone.DelayBetweenProjectiles;
            return result;
        }

        public bool IsAvailableFor(AbilityData ability)
        {
            string reason;
            return ability != null && ability.Caster != null &&
                ScatterShotRuntime.IsAvailable(ability.Caster, out reason);
        }

        public string GetReason()
        {
            return "Requires exactly one equipped, loaded, non-Wrecked Blunderbuss.";
        }

        public override IEnumerator<AbilityDeliveryTarget> Deliver(
            AbilityExecutionContext context, TargetWrapper target)
        {
            if (context == null || context.MaybeCaster == null || target == null)
                throw new InvalidOperationException(
                    "Scatter Shot requires a caster and a direction point.");

            // Reuse Kingmaker's native Burning Hands cone solely for cast
            // orientation and VFX timing. Presentation is deliberately best-effort:
            // an unavailable projectile view must not erase an otherwise valid
            // firearm discharge or damage transaction.
            IEnumerator<AbilityDeliveryTarget> presentation = null;
            try
            {
                presentation = base.Deliver(context, target);
            }
            catch (Exception exception)
            {
                LogPresentationFailure(exception);
            }

            if (presentation != null)
            {
                try
                {
                    while (true)
                    {
                        bool moved;
                        AbilityDeliveryTarget current = null;
                        try
                        {
                            moved = presentation.MoveNext();
                            if (moved) current = presentation.Current;
                        }
                        catch (Exception exception)
                        {
                            LogPresentationFailure(exception);
                            break;
                        }
                        if (!moved) break;
                        yield return current;
                    }
                }
                finally
                {
                    try
                    {
                        presentation.Dispose();
                    }
                    catch (Exception exception)
                    {
                        LogPresentationFailure(exception);
                    }
                }
            }

            ScatterShotRuntime.ExecuteFromAbility(context, target.Point);
            yield return new AbilityDeliveryTarget(target);
        }

        private static void LogPresentationFailure(Exception exception)
        {
            ModContext mod;
            if (ModContext.TryGet(out mod))
                mod.Logger.Failure("scatter", "presentation.failed",
                    "Native cone presentation failed; Scatter Shot mechanics will continue through the firearm transaction.",
                    exception);
        }
    }
}
