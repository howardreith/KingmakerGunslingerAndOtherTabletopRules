using System;
using System.Collections.Generic;
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
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using KingmakerGunslinger.Bootstrap;
using Harmony12;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Items;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>Native terrain lifetime continues normally; module OFF ends only this owned area.</summary>
    [Serializable]
    public sealed class IceDevilStormModuleLifetime40 : AbilityAreaEffectLogic
    {
        public BlueprintAbility OwningSpell;
        protected override void OnTick(MechanicsContext context, AreaEffectEntityData area)
        {
            ModContext mod;
            if (area != null && !area.IsEnded && OwningSpell != null &&
                ReferenceEquals(context?.SourceAbility, OwningSpell) &&
                ModContext.TryGet(out mod) &&
                mod.FeatureModules?.Active?.ExpandedSummoning != true)
                area.ForceEnd();
        }
        protected override void OnUnitEnter(MechanicsContext context, AreaEffectEntityData area, UnitEntityData unit) { }
        protected override void OnUnitExit(MechanicsContext context, AreaEffectEntityData area, UnitEntityData unit) { }
        protected override void OnUnitMove(MechanicsContext context, AreaEffectEntityData area, UnitEntityData unit) { }
    }

    public sealed class IceDevilMartialProfile40 : BlueprintComponent
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintItemWeapon Spear, Bite, Tail;
        internal static IceDevilMartialProfile40 For(UnitEntityData owner)
        {
            ModContext mod;
            if (owner?.Blueprint == null || !ModContext.TryGet(out mod) ||
                mod.FeatureModules?.Active?.ExpandedSummoning != true) return null;
            var profile = owner.Blueprint.GetComponent<IceDevilMartialProfile40>();
            return profile != null && ReferenceEquals(profile.OwningBlueprint,
                owner.Blueprint) ? profile : null;
        }
    }

    // Native empty-hand fallback uses the bite so standard/charge/AoO keep
    // a legal natural weapon after disarm. If native full-attack enumeration
    // also visits that same bite through its additional slot, retain it once.
    // Spear iteratives are never deduplicated and no attack is manufactured.
    [HarmonyPatch(typeof(UnitAttack), "CreateFullAttack")]
    internal static class IceDevilNaturalRoutine40
    {
        private static void Postfix(UnitAttack __instance, List<AttackHandInfo> __result)
        {
            var profile = IceDevilMartialProfile40.For(__instance?.Executor);
            if (profile?.Bite == null || __result == null) return;
            bool seen = false;
            __result.RemoveAll(attack => {
                bool bite = ReferenceEquals(attack?.Hand?.MaybeWeapon?.Blueprint, profile.Bite);
                bool keep = IceDevilRules40Policy.RetainBiteAttack(
                    ReferenceEquals(__instance.Executor.Body.PrimaryHand.MaybeWeapon?.Blueprint, profile.Bite),
                    bite, ReferenceEquals(attack?.Hand, __instance.Executor.Body.PrimaryHand), seen);
                if (bite) seen = true;
                return !keep;
            });
        }
    }

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
                    OwningBlueprint != null && ReferenceEquals(owner.Blueprint, OwningBlueprint) &&
                        ReferenceEquals(evt.Weapon?.Owner, owner.Descriptor),
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
