using System;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums.Damage;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Newtonsoft.Json;

namespace KingmakerGunslinger.ElementalRaces
{
    [Serializable]
    public sealed class ElementalUnerringWeaponTargetChecker :
        BlueprintComponent, IAbilityTargetChecker
    {
        public bool SecondaryHand;

        public bool CanTarget(UnitEntityData caster, TargetWrapper target)
        {
            if (caster == null || caster.Body == null || target.Unit == null ||
                !ReferenceEquals(caster, target.Unit)) return false;
            ItemEntityWeapon weapon = SecondaryHand
                ? caster.Body.SecondaryHand.MaybeWeapon
                : caster.Body.PrimaryHand.MaybeWeapon;
            return weapon != null && weapon.Blueprint != null &&
                !weapon.Blueprint.IsNatural;
        }
    }

    [Serializable]
    public sealed class ElementalUnerringWeaponEnchantment :
        WeaponEnchantmentLogic, IInitiatorRulebookHandler<RuleAttackRoll>
    {
        public override void OnTurnOn() { }
        public override void OnTurnOff() { }

        public void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            if (evt == null || Owner == null ||
                !ReferenceEquals(evt.Weapon, Owner)) return;
            ItemEnchantment enchantment = Fact as ItemEnchantment;
            int casterLevel = enchantment == null ||
                enchantment.ParentContext == null ? 0 :
                enchantment.ParentContext.Params.CasterLevel;
            if (casterLevel < 1) return;
            evt.CriticalConfirmationBonus += ElementalHeritageSlaPolicy
                .UnerringConfirmationBonus(casterLevel);
        }

        public void OnEventDidTrigger(RuleAttackRoll evt) { }
    }

    public sealed class UnitPartElementalChillTouch : UnitPart
    {
        [JsonProperty]
        private string _deliveryGuid;
        [JsonProperty]
        private int _remainingTouches;

        internal void Begin(BlueprintAbility delivery, int casterLevel)
        {
            if (delivery == null || string.IsNullOrWhiteSpace(
                    delivery.AssetGuid))
                throw new ArgumentException(
                    "Chill Touch requires its exact delivery ability.");
            _deliveryGuid = delivery.AssetGuid;
            _remainingTouches = ElementalHeritageSlaPolicy.ChillTouchCount(
                casterLevel);
        }

        internal bool Matches(BlueprintAbility delivery)
        {
            return delivery != null && ElementalHeritageSlaPolicy
                .ExactDeliveryMatch(_deliveryGuid, delivery.AssetGuid,
                    delivery.AssetGuid);
        }

        internal bool Matches(BlueprintAbility heldDelivery,
            BlueprintAbility executingDelivery)
        {
            return heldDelivery != null && executingDelivery != null &&
                ElementalHeritageSlaPolicy.ExactDeliveryMatch(_deliveryGuid,
                    heldDelivery.AssetGuid, executingDelivery.AssetGuid);
        }

        internal bool ConsumeAndRetain()
        {
            if (_remainingTouches <= 0) return false;
            _remainingTouches--;
            return _remainingTouches > 0;
        }

        internal int RemainingTouches { get { return _remainingTouches; } }
    }

    [Serializable]
    public sealed class ElementalChillTouchStickyTouch :
        AbilityEffectStickyTouch
    {
        public override void Apply(AbilityExecutionContext context,
            TargetWrapper target)
        {
            UnitEntityData caster = context == null ? null :
                context.MaybeCaster;
            if (caster == null || TouchDeliveryAbility == null)
                throw new InvalidOperationException(
                    "Chill Touch has no caster or delivery ability.");
            caster.Descriptor.Ensure<UnitPartElementalChillTouch>().Begin(
                TouchDeliveryAbility, context.Params.CasterLevel);
            base.Apply(context, target);
        }
    }

    [Serializable]
    public sealed class ContextActionElementalChillTouch : ContextAction
    {
        public override string GetCaption()
        {
            return "Resolve Chill Touch against a living or undead creature";
        }

        public override void RunAction()
        {
            UnitEntityData caster = Context == null ? null :
                Context.MaybeCaster;
            UnitEntityData target = Target.Unit;
            if (caster == null || target == null ||
                target.Descriptor == null) return;
            int dc = Math.Max(0, Context.Params.DC);
            if (target.Descriptor.IsUndead)
            {
                var will = new RuleSavingThrow(target,
                    SavingThrowType.Will, dc) { Reason = Context };
                Rulebook.Trigger(will);
                if (will.IsPassed) return;
                int roll = Rulebook.Trigger(new RuleRollDice(caster,
                    new DiceFormula(1, DiceType.D4))).Result;
                int rounds = ElementalHeritageSlaPolicy
                    .ChillTouchUndeadPanicRounds(roll,
                        Context.Params.CasterLevel);
                target.Descriptor.Buffs.AddBuff(
                    Kingmaker.Blueprints.Root.BlueprintRoot.Instance
                        .SystemMechanics.FrightenedBuff,
                    Context, TimeSpan.FromSeconds(6d * rounds));
                return;
            }

            var damage = new RuleDealDamage(caster, target,
                new DamageBundle(new EnergyDamage(
                    new DiceFormula(1, DiceType.D6),
                    DamageEnergyType.NegativeEnergy)))
            {
                DisablePrecisionDamage = true,
                Reason = Context
            };
            Rulebook.Trigger(damage);
            var fortitude = new RuleSavingThrow(target,
                SavingThrowType.Fortitude, dc) { Reason = Context };
            Rulebook.Trigger(fortitude);
            if (!fortitude.IsPassed)
                Rulebook.Trigger(new RuleDealStatDamage(caster, target,
                    StatType.Strength,
                    new DiceFormula(0, DiceType.D6), 1)
                { Reason = Context });
        }
    }

    internal static class ElementalChillTouchRuntime
    {
        internal static bool RetainAfterApplied(
            AbilityExecutionContext context)
        {
            UnitEntityData caster = context == null ? null :
                context.MaybeCaster;
            BlueprintAbility delivery = context == null ||
                context.Ability == null ? null : context.Ability.Blueprint;
            UnitPartTouch touch = caster == null ? null :
                caster.Get<UnitPartTouch>();
            UnitPartElementalChillTouch state = caster == null ? null :
                caster.Get<UnitPartElementalChillTouch>();
            if (touch == null || touch.Ability == null ||
                touch.Ability.Data == null ||
                context.Ability == null ||
                state == null || !state.Matches(
                    touch.Ability.Data.Blueprint, delivery)) return false;
            bool retain = state.ConsumeAndRetain();
            if (!retain)
            {
                caster.Remove<UnitPartElementalChillTouch>();
                return false;
            }
            if (!caster.IsAutoUseAbility(context.Ability) &&
                caster.CombatState != null)
                caster.CombatState.ManualTarget = null;
            return true;
        }

        internal static void HandleTouchRemoved(UnitPartTouch touch)
        {
            if (touch == null || touch.Owner == null || touch.Ability == null)
                return;
            UnitPartElementalChillTouch state = touch.Owner.Get<
                UnitPartElementalChillTouch>();
            if (state != null && state.Matches(touch.Ability.Data.Blueprint))
                touch.Owner.Remove<UnitPartElementalChillTouch>();
        }
    }

    [HarmonyPatch(typeof(TouchSpellsController), "OnAbilityEffectApplied")]
    [HarmonyBefore("CallOfTheWild")]
    internal static class ElementalChillTouchAppliedPatch
    {
        private static bool Prefix(AbilityExecutionContext context)
        {
            return !ElementalChillTouchRuntime.RetainAfterApplied(context);
        }
    }

    [HarmonyPatch(typeof(UnitPartTouch), "OnRemove")]
    internal static class ElementalChillTouchRemovedPatch
    {
        private static void Prefix(UnitPartTouch __instance)
        {
            ElementalChillTouchRuntime.HandleTouchRemoved(__instance);
        }
    }
}
