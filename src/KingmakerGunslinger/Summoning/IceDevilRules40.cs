using System;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>One exact owner's hit rider; never a weapon enchantment.</summary>
    [Serializable]
    public sealed class IceDevilSlowOnHit40 :
        RuleInitiatorLogicComponent<RuleAttackWithWeapon>
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintItemWeapon Spear;
        public BlueprintItemWeapon Tail;
        public BlueprintBuff Slowed;
        // Only the native failed-save condition/haste-counter branch is
        // reused. The builder supplies native 1d6 duration and supernatural
        // terminal flags. No cast target selection, Will save or SR is copied.
        public ActionList OnFailedSave;
        private static readonly ConditionalWeakTable<UnitEntityData,
            ConditionalWeakTable<RuleAttackWithWeapon, object>> Resolved =
            new ConditionalWeakTable<UnitEntityData,
                ConditionalWeakTable<RuleAttackWithWeapon, object>>();

        public override void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }

        public override void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
            var owner = Owner?.Unit;
            var target = evt?.Target;
            var weapon = evt?.Weapon?.Blueprint;
            ModContext mod;
            if (!ModContext.TryGet(out mod) ||
                mod.FeatureModules?.Active?.ExpandedSummoning != true ||
                owner == null || owner.Destroyed || owner.Descriptor.State.IsDead ||
                target == null || target.Destroyed || target.Descriptor.State.IsDead ||
                Slowed == null || OnFailedSave == null ||
                !IceDevilRules40Policy.QualifiesForSlow(
                    OwningBlueprint != null && ReferenceEquals(owner.Blueprint, OwningBlueprint),
                    ReferenceEquals(evt.Initiator, owner),
                    Spear != null && ReferenceEquals(weapon, Spear),
                    Tail != null && ReferenceEquals(weapon, Tail),
                    evt.AttackRoll?.IsHit == true)) return;
            var events = Resolved.GetValue(owner,
                unit => new ConditionalWeakTable<RuleAttackWithWeapon, object>());
            object prior;
            if (events.TryGetValue(evt, out prior)) return;
            events.Add(evt, new object());

            int dc = IceDevilRules40Policy.SlowDc(owner.Stats.Constitution.Bonus);
            var context = new MechanicsContext(owner, target.Descriptor,
                Slowed, Fact?.MaybeContext, new TargetWrapper(target));
            context.Params.DC = dc;
            var save = new RuleSavingThrow(target, SavingThrowType.Fortitude, dc)
                { Reason = context };
            context.TriggerRule(save);
            if (save.IsPassed) return;
            using (context.GetDataScope(new TargetWrapper(target)))
                OnFailedSave.Run();
        }

        public override void OnTurnOff()
        {
            if (Owner?.Unit != null) Resolved.Remove(Owner.Unit);
            base.OnTurnOff();
        }
    }

    /// <summary>The printed bite keeps +6 Strength beside the armed spear.</summary>
    [Serializable]
    public sealed class IceDevilBiteStrength40 :
        RuleInitiatorLogicComponent<RuleCalculateWeaponStats>
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintItemWeapon Bite;
        public override void OnEventAboutToTrigger(RuleCalculateWeaponStats evt) { }
        public override void OnEventDidTrigger(RuleCalculateWeaponStats evt)
        {
            var owner = Owner?.Unit;
            if (owner == null || OwningBlueprint == null || Bite == null ||
                evt?.Weapon == null || !ReferenceEquals(evt.Initiator, owner) ||
                !ReferenceEquals(owner.Blueprint, OwningBlueprint) ||
                !ReferenceEquals(evt.Weapon.Blueprint, Bite) ||
                evt.DamageBonusStat != StatType.Strength ||
                evt.DamageDescription == null || evt.DamageDescription.Count == 0)
                return;
            int correction = IceDevilRules40Policy.BiteStrengthCorrection(
                owner.Stats.Strength.Bonus, evt.DamageBonusStatMultiplier,
                evt.Weapon.IsSecondary || evt.SecondaryWeapon);
            var damage = evt.DamageDescription[0];
            damage.Bonus += correction;
            evt.DamageDescription[0] = damage;
        }
    }
}
