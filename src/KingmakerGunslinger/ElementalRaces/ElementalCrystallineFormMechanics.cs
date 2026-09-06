using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Components;

namespace KingmakerGunslinger.ElementalRaces
{
    internal static class ElementalCrystallineFormRuntime
    {
        internal static bool IsRay(RuleAttackRoll roll)
        {
            if (roll == null || roll.Weapon == null ||
                roll.Weapon.Blueprint.Category != WeaponCategory.Ray) return false;
            BlueprintAbility ability = roll.Reason == null ? null :
                roll.Reason.Ability != null ? roll.Reason.Ability.Blueprint :
                roll.Reason.Context == null ? null : roll.Reason.Context.SourceAbility;
            var chain = new List<BlueprintAbility>();
            var seen = new HashSet<BlueprintAbility>();
            for (BlueprintAbility current = ability; current != null && seen.Add(current); current = current.Parent)
                chain.Add(current);
            // The effective delivery still has to be an actual single-target
            // ray attack; inheriting a ray's parent name/ID is not sufficient.
            AbilityDeliverProjectile delivery = chain.SelectMany(value =>
                value.ComponentsArray ?? new BlueprintComponent[0]).OfType<AbilityDeliverProjectile>().FirstOrDefault();
            return delivery != null && ReferenceEquals(delivery.Weapon, roll.Weapon.Blueprint) &&
                ElementalCrystallineFormPolicy.IsRay(chain.Select(value => value.AssetGuid),
                    delivery.Type == AbilityProjectileType.Simple, delivery.NeedAttackRoll,
                    delivery.Weapon != null && delivery.Weapon.Category == WeaponCategory.Ray,
                    delivery.IsHandOfTheApprentice);
        }

        internal static bool HasFreeHand(UnitDescriptor owner)
        {
            UnitBody body = owner == null ? null : owner.Body;
            if (body == null || !body.HandsAreEnabled || body.PrimaryHand == null || body.SecondaryHand == null)
                return false;
            // Native HoldInTwoHands includes double/two-handed weapons and the
            // game's current one-handed-in-two-hands state. An empty off-hand
            // slot alone is not enough. Empty-hand unarmed weapons are not items.
            if ((body.PrimaryHand.MaybeWeapon != null && body.PrimaryHand.MaybeWeapon.HoldInTwoHands) ||
                (body.SecondaryHand.MaybeWeapon != null && body.SecondaryHand.MaybeWeapon.HoldInTwoHands)) return false;
            return (!body.PrimaryHand.Disabled && !body.PrimaryHand.HasItem) ||
                (!body.SecondaryHand.Disabled && !body.SecondaryHand.HasItem);
        }
    }

    [Serializable]
    public sealed class ElementalCrystallineRayArmorClass : RuleTargetLogicComponent<RuleCalculateAC>
    {
        public override void OnEventAboutToTrigger(RuleCalculateAC evt)
        {
            RuleAttackRoll roll = Rulebook.CurrentContext == null ? null :
                Rulebook.CurrentContext.LastEvent<RuleAttackRoll>();
            if (evt == null || roll == null || Owner == null ||
                !ReferenceEquals(evt.Target, Owner.Unit) || !ReferenceEquals(roll.Target, evt.Target) ||
                !ReferenceEquals(roll.Initiator, evt.Initiator) || roll.AttackType != evt.AttackType ||
                !ElementalCrystallineFormRuntime.IsRay(roll)) return;
            ModifiableValue.Modifier modifier = Owner.Stats.AC.AddModifier(2, Fact,
                GetType().FullName, ModifierDescriptor.Racial);
            if (modifier == null) return;
            Owner.Stats.AC.UpdateValue();
            evt.AddTemporaryModifier(modifier);
        }
        public override void OnEventDidTrigger(RuleCalculateAC evt) { }
    }

    /// <summary>Local owned fact subscriber, not a global Harmony rewrite.
    /// Native Projectile.OnHit raises this before native effect application,
    /// after delivery's ForceAlwaysHit override. Native ApplyEffect reads IsHit.</summary>
    [Serializable]
    public sealed class ElementalCrystallineRayDeflection : OwnedGameLogicComponent<UnitDescriptor>, IProjectileHitHandler
    {
        public BlueprintAbilityResource Resource;
        public BlueprintBuff ArmedBuff;
        public BlueprintActivatableAbility Mode;

        public void HandleProjectileHit(Projectile projectile)
        {
            RuleAttackRoll roll = projectile == null ? null : projectile.AttackRoll;
            if (Owner == null || roll == null || projectile.IsFromWeapon || projectile.Target == null ||
                !ReferenceEquals(projectile.Target.Unit, Owner.Unit) || !ReferenceEquals(roll.Target, Owner.Unit) ||
                !roll.IsHit || !Owner.HasFact(ArmedBuff) || !Owner.ActivatableAbilities.Enumerable.Any(value =>
                    ReferenceEquals(value.Blueprint, Mode) && value.IsOn) ||
                !ElementalCrystallineFormRuntime.IsRay(roll)) return;
            bool conscious = !Owner.State.IsDead && !Owner.State.IsHelpless && !Owner.State.IsUnconscious;
            if (!conscious || !ElementalCrystallineFormRuntime.HasFreeHand(Owner) ||
                !Owner.Resources.HasEnoughResource(Resource, 1)) return;
            bool aware = !Rulebook.Trigger(new RuleCheckTargetFlatFooted(roll.Initiator, Owner.Unit)).IsFlatFooted;
            if (!ElementalCrystallineFormPolicy.CanDeflect(true, true, roll.IsHit, Owner.HasFact(ArmedBuff),
                    Owner.Resources.GetResourceAmount(Resource), conscious, aware, true)) return;
            // Replace the resolved result, not AutoMiss (which is too late here).
            // The next native delivery observes this as a miss and skips effects.
            roll.SetFake(AttackResult.Parried);
            projectile.AttackResult = AttackResult.Parried;
            Owner.Resources.Spend(Resource, 1);
            foreach (ActivatableAbility mode in Owner.ActivatableAbilities.Enumerable.Where(value =>
                ReferenceEquals(value.Blueprint, Mode)).ToArray()) mode.IsOn = false;
        }
    }
}
