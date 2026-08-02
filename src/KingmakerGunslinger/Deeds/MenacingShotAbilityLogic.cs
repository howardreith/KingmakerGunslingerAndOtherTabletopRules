using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using KingmakerGunslinger.Actions;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Firing;
using UnityEngine;

namespace KingmakerGunslinger.Deeds
{
    [Serializable]
    public sealed class MenacingShotAbilityLogic : AbilityCustomLogic,
        IAbilityAvailabilityProvider
    {
        private static readonly MenacingShotService Policy =
            new MenacingShotService();
        private static readonly FirearmDischargeService Discharge =
            new FirearmDischargeService();

        internal static MenacingShotAbilityLogic Create()
        { return ScriptableObject.CreateInstance<MenacingShotAbilityLogic>(); }

        public bool IsAvailableFor(AbilityData ability)
        {
            if (ability == null || ability.Caster == null) return false;
            ExactEquippedFirearmContext firearm;
            MenacingShotDecision decision = Evaluate(ability.Caster,
                out firearm);
            return decision.ShouldApply;
        }

        public string GetReason()
        { return "Requires Gunslinger level 15, 1 grit, and one equipped loaded, non-Wrecked firearm."; }

        public override IEnumerator<AbilityDeliveryTarget> Deliver(
            AbilityExecutionContext context, TargetWrapper target)
        {
            if (context == null || context.Caster == null)
                throw new InvalidOperationException(
                    "Menacing Shot requires an exact caster context.");
            UnitDescriptor owner = context.Caster.Descriptor;
            ExactEquippedFirearmContext firearm;
            MenacingShotDecision decision = Evaluate(owner, out firearm);
            if (!decision.ShouldApply)
                throw new InvalidOperationException(
                    "Menacing Shot became unavailable before delivery: " +
                    decision.Status + ".");

            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            int gritBefore = owner.Resources.GetResourceAmount(gunslinger.Grit.Resource);
            TrueGritDecision trueGrit = TrueGritRuntime.Evaluate(owner,
                TrueGritDeed.MenacingShot, decision.GritCost, false);
            FirearmState before = firearm.Firearm.Repository.State;
            bool discharged = false, spent = false, completed = false;
            try
            {
                FirearmDischargeResult discharge = Discharge.Evaluate(before);
                if (discharge.Status != FirearmDischargeStatus.Fired)
                    throw new InvalidOperationException(
                        "Eligible Menacing Shot did not produce a discharge.");
                FirearmRuntimeState.Service.Transition(firearm.Weapon, current =>
                {
                    if (current != before)
                        throw new InvalidOperationException(
                            "Firearm state changed before Menacing Shot delivery.");
                    return discharge.After;
                });
                discharged = true;
                owner.Resources.Spend(gunslinger.Grit.Resource,
                    trueGrit.EffectiveCost);
                if (owner.Resources.GetResourceAmount(gunslinger.Grit.Resource) !=
                    gritBefore - trueGrit.EffectiveCost)
                    throw new InvalidOperationException(
                        "Menacing Shot grit spend was not exact.");
                spent = true;
                context.Params.DC = decision.DifficultyClass;
                context.Params.CasterLevel = decision.FrightenedRounds;

                foreach (UnitEntityData unit in LivingTargets(context.Caster))
                    yield return new AbilityDeliveryTarget(new TargetWrapper(unit));
                completed = true;
            }
            finally
            {
                if (!completed)
                {
                    if (spent)
                        owner.Resources.Restore(gunslinger.Grit.Resource,
                            trueGrit.EffectiveCost);
                    if (discharged)
                        FirearmRuntimeState.Service.Transition(firearm.Weapon,
                            current => before);
                }
            }
        }

        public override void Cleanup(AbilityExecutionContext context) { }

        private static MenacingShotDecision Evaluate(UnitDescriptor owner,
            out ExactEquippedFirearmContext firearm)
        {
            firearm = null;
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            string rejection;
            bool exact = owner != null && ExactEquippedFirearmResolver.TryResolve(
                owner, out firearm, out rejection);
            FirearmState state = exact ? firearm.Firearm.Repository.State :
                FirearmState.CreateEmpty();
            int level = owner == null || gunslinger == null ? 0 :
                owner.Progression.GetClassLevel(gunslinger.CharacterClass);
            int wisdom = owner == null ? 0 : owner.Stats.Wisdom.Bonus;
            int grit = owner == null || gunslinger == null ? 0 :
                owner.Resources.GetResourceAmount(gunslinger.Grit.Resource);
            if (owner != null && gunslinger != null &&
                TrueGritRuntime.Evaluate(owner, TrueGritDeed.MenacingShot,
                    1, false).Available) grit = Math.Max(1, grit);
            return Policy.Evaluate(new MenacingShotRequest(level, wisdom, exact,
                state.Condition, state.LoadedRounds, grit));
        }

        private static IEnumerable<UnitEntityData> LivingTargets(
            UnitEntityData caster)
        {
            IEnumerable<UnitEntityData> queried = GameHelper.GetTargetsAround(
                caster.Position, new Feet(30f), true, false);
            if (queried == null)
                throw new InvalidOperationException(
                    "Kingmaker's native 30-foot target query returned null.");
            var seen = new HashSet<UnitEntityData>();
            foreach (UnitEntityData unit in queried.Concat(new[] { caster }))
            {
                if (unit == null || !seen.Add(unit) || unit.Descriptor == null ||
                    unit.Descriptor.State.IsDead || unit.Descriptor.IsUndead)
                    continue;
                if (!new MenacingShotTargetDecision(true,
                    unit.DistanceTo(caster.Position)).IsAffected) continue;
                yield return unit;
            }
        }
    }
}
