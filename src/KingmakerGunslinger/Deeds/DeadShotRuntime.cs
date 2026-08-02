using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using KingmakerGunslinger.Actions;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Explosions;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Firing;
using KingmakerGunslinger.Misfires;

namespace KingmakerGunslinger.Deeds
{
    internal static class DeadShotRuntime
    {
        private static readonly object Gate = new object();
        private static readonly ConditionalWeakTable<RuleAttackRoll, ProbeContext>
            Probes = new ConditionalWeakTable<RuleAttackRoll, ProbeContext>();
        private static readonly ConditionalWeakTable<RuleAttackWithWeapon, DeliveryMarker>
            Deliveries = new ConditionalWeakTable<RuleAttackWithWeapon, DeliveryMarker>();
        private static readonly DeadShotService Policy = new DeadShotService();
        private static readonly DeadShotOutcomeService Outcomes =
            new DeadShotOutcomeService();
        private static readonly FirearmDischargeService Discharge =
            new FirearmDischargeService();
        private static readonly FirearmMisfireService Misfires =
            new FirearmMisfireService();
        private static readonly FirearmMisfireConditionService Conditions =
            new FirearmMisfireConditionService();

        internal static DeadShotDecision Evaluate(UnitDescriptor caster,
            out ExactEquippedFirearmContext firearm, out string reason)
        {
            firearm = null;
            if (!ExactEquippedFirearmResolver.TryResolve(caster, out firearm,
                    out reason))
                return Policy.Evaluate(new DeadShotRequest(false, false,
                    FirearmCondition.Normal, 0, ReadGrit(caster), ReadBab(caster)));
            FirearmState state = firearm.Firearm.Repository.State;
            DeadShotDecision decision = Policy.Evaluate(new DeadShotRequest(true,
                firearm.Definition.IsScatter, state.Condition, state.LoadedRounds,
                ReadGrit(caster, TrueGritDeed.DeadShot, 1), ReadBab(caster)));
            reason = decision.Status.ToString();
            return decision;
        }

        internal static DeadShotExecutionResult Execute(
            AbilityExecutionContext context, UnitEntityData target)
        {
            if (context == null || context.Caster == null || target == null)
                throw new ArgumentNullException("context");
            return Execute(context.Caster.Descriptor, context.Caster, target,
                delegate(RuleAttackRoll rule) { rule.Reason = context;
                    context.TriggerRule(rule); },
                delegate(RuleAttackWithWeapon rule) { rule.Reason = context;
                    context.TriggerRule(rule); });
        }

        internal static DeadShotExecutionResult ExecuteForRuntimeTest(
            UnitEntityData caster, UnitEntityData target, params int[] forcedRolls)
        {
            if (caster == null || target == null) throw new ArgumentNullException("caster");
            return Execute(caster.Descriptor, caster, target,
                delegate(RuleAttackRoll rule) { Rulebook.Trigger(rule); },
                delegate(RuleAttackWithWeapon rule) { Rulebook.Trigger(rule); },
                forcedRolls);
        }

        private static DeadShotExecutionResult Execute(UnitDescriptor caster,
            UnitEntityData casterEntity, UnitEntityData target,
            Action<RuleAttackRoll> triggerProbe,
            Action<RuleAttackWithWeapon> triggerDelivery,
            int[] forcedRolls = null)
        {
            ExactEquippedFirearmContext firearm;
            string reason;
            DeadShotDecision decision = Evaluate(caster, out firearm, out reason);
            if (!decision.ShouldAttack)
                return new DeadShotExecutionResult(decision, null, null, null,
                    firearm == null ? null : firearm.Firearm.Repository.State,
                    firearm == null ? null : firearm.Firearm.Repository.State);
            if (forcedRolls != null && forcedRolls.Length !=
                decision.AttackBonuses.Length)
                throw new ArgumentException(
                    "Forced Dead Shot roll count does not match BAB.",
                    "forcedRolls");

            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            TrueGritDecision trueGrit = TrueGritRuntime.Evaluate(caster,
                TrueGritDeed.DeadShot, decision.GritCost, false);
            FirearmState before = firearm.Firearm.Repository.State;
            FirearmState expectedCurrent = before;
            bool spent = false;
            try
            {
                caster.Resources.Spend(gunslinger.Grit.Resource,
                    trueGrit.EffectiveCost);
                spent = true;
                FirearmDischargeResult discharge = Discharge.Evaluate(before);
                if (discharge.Status != FirearmDischargeStatus.Fired)
                    throw new InvalidOperationException("Accepted Dead Shot did not discharge.");
                FirearmItemStateSnapshot postDischarge = Transition(firearm,
                    expectedCurrent, discharge.After);
                expectedCurrent = postDischarge.Repository.State;

                int threshold = Classes.GunTrainingPolicy.EffectiveMisfireValue(
                    firearm.Definition.MisfireValue, expectedCurrent.Condition,
                    HasGunTraining(casterEntity, firearm.Definition.Kind));
                var probes = new RuleAttackRoll[decision.AttackBonuses.Length];
                var observations = new DeadShotRollObservation[probes.Length];
                for (int index = 0; index < probes.Length; index++)
                {
                    var probe = new RuleAttackRoll(casterEntity, target,
                        firearm.Weapon, -(index * 5));
                    probe.ImmuneToCriticalHit = true;
                    RegisterProbe(probe, threshold, forcedRolls == null ?
                        (int?)null : forcedRolls[index]);
                    try { triggerProbe(probe); observations[index] = ConsumeProbe(probe); }
                    catch { CancelProbe(probe); throw; }
                    probes[index] = probe;
                }
                DeadShotOutcome outcome = Outcomes.Evaluate(decision, observations);
                FirearmMisfireConditionDecision condition = null;
                if (outcome.Misfires)
                {
                    condition = Conditions.Evaluate(Misfires.Evaluate(1, threshold,
                        false), expectedCurrent);
                    if (condition.ChangesCondition)
                    {
                        FirearmItemStateSnapshot changed = Transition(firearm,
                            expectedCurrent, condition.After);
                        expectedCurrent = changed.Repository.State;
                    }
                }

                var delivery = new RuleAttackWithWeapon(casterEntity, target,
                    firearm.Weapon, 0);
                bool deliveryHit = outcome.IsHit && !outcome.Misfires;
                if (outcome.BaseDamageDicePackets > 1 && deliveryHit)
                {
                    var original = firearm.Weapon.Damage;
                    delivery.WeaponStats.WeaponDamageDiceOverride =
                        new DiceFormula(original.Rolls *
                            outcome.BaseDamageDicePackets, original.Dice);
                }
                RegisterDelivery(delivery, deliveryHit,
                    outcome.ThreatCount > 0 && deliveryHit,
                    outcome.ConfirmationPenalty ?? 0);
                try { triggerDelivery(delivery); }
                finally { CancelDelivery(delivery); }

                if (condition != null && condition.Transition ==
                    FirearmMisfireConditionTransition.BrokenToWrecked)
                {
                    FirearmExplosionRuntime.Apply(delivery.AttackRoll,
                        firearm.Weapon, casterEntity,
                        postDischarge.Repository.RepositoryIdentity,
                        firearm.Definition.MisfireBurstRadiusFeet,
                        firearm.Firearm.ItemDisplayName);
                }
                FirearmDischargeRuntimeDiagnostics.Record(discharge,
                    firearm.Firearm.ItemDisplayName);
                return new DeadShotExecutionResult(decision, outcome, probes,
                    delivery, before, expectedCurrent);
            }
            catch
            {
                TryRollback(firearm, expectedCurrent, before);
                if (spent) caster.Resources.Restore(gunslinger.Grit.Resource,
                    trueGrit.EffectiveCost);
                throw;
            }
        }

        private static FirearmItemStateSnapshot Transition(
            ExactEquippedFirearmContext firearm, FirearmState expected,
            FirearmState replacement)
        {
            return FirearmRuntimeState.Service.Transition(firearm.Weapon,
                current => {
                    if (current != expected) throw new InvalidOperationException(
                        "Dead Shot firearm state changed during delivery.");
                    return replacement;
                });
        }

        private static void TryRollback(ExactEquippedFirearmContext firearm,
            FirearmState expected, FirearmState before)
        {
            if (firearm == null || expected == null || before == null) return;
            Transition(firearm, expected, before);
        }

        private static int ReadGrit(UnitDescriptor caster)
        {
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            return caster == null || gunslinger == null ? 0 :
                caster.Resources.GetResourceAmount(gunslinger.Grit.Resource);
        }

        private static int ReadGrit(UnitDescriptor caster, TrueGritDeed deed,
            int ordinaryCost)
        {
            int current = ReadGrit(caster);
            return TrueGritRuntime.Evaluate(caster, deed, ordinaryCost, false)
                .Available ? Math.Max(ordinaryCost, current) : current;
        }

        private static int ReadBab(UnitDescriptor caster)
        {
            return caster == null || caster.Stats == null ? 0 :
                caster.Stats.BaseAttackBonus.ModifiedValue;
        }

        private static bool HasGunTraining(UnitEntityData caster, FirearmKind kind)
        {
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            return caster != null && gunslinger != null &&
                gunslinger.GunTraining != null &&
                Classes.GunTrainingPolicy.IsSupportedKind(kind) &&
                caster.Descriptor.HasFact(gunslinger.GunTraining.ChoiceFor(kind));
        }

        internal static void RegisterProbe(RuleAttackRoll attackRoll,
            int misfireThreshold, int? forcedNaturalRoll = null)
        {
            if (attackRoll == null) throw new ArgumentNullException("attackRoll");
            if (misfireThreshold < 1 || misfireThreshold > 20)
                throw new ArgumentOutOfRangeException("misfireThreshold");
            if (forcedNaturalRoll.HasValue && (forcedNaturalRoll.Value < 1 ||
                forcedNaturalRoll.Value > 20))
                throw new ArgumentOutOfRangeException("forcedNaturalRoll");
            lock (Gate)
            {
                Probes.Remove(attackRoll);
                Probes.Add(attackRoll, new ProbeContext(misfireThreshold,
                    forcedNaturalRoll));
            }
        }

        internal static bool IsProbe(RuleAttackRoll attackRoll)
        {
            if (attackRoll == null) return false;
            lock (Gate)
            {
                ProbeContext ignored;
                return Probes.TryGetValue(attackRoll, out ignored);
            }
        }

        internal static void RegisterDelivery(RuleAttackWithWeapon attack,
            bool shouldHit, bool criticalThreat, int confirmationPenalty)
        {
            if (attack == null) throw new ArgumentNullException("attack");
            lock (Gate)
            {
                Deliveries.Remove(attack);
                Deliveries.Add(attack, new DeliveryMarker(shouldHit,
                    criticalThreat, confirmationPenalty));
            }
        }

        internal static void CancelDelivery(RuleAttackWithWeapon attack)
        {
            if (attack == null) return;
            lock (Gate) { Deliveries.Remove(attack); }
        }

        internal static bool ShouldBypassDischarge(RuleAttackRoll attackRoll)
        {
            if (IsProbe(attackRoll)) return true;
            if (attackRoll == null || attackRoll.RuleAttackWithWeapon == null)
                return false;
            lock (Gate)
            {
                DeliveryMarker ignored;
                return Deliveries.TryGetValue(attackRoll.RuleAttackWithWeapon,
                    out ignored);
            }
        }

        internal static void ConfigureDelivery(RuleAttackRoll attackRoll)
        {
            if (attackRoll == null || attackRoll.RuleAttackWithWeapon == null)
                return;
            DeliveryMarker marker;
            lock (Gate)
            {
                if (!Deliveries.TryGetValue(attackRoll.RuleAttackWithWeapon,
                        out marker)) return;
            }
            attackRoll.AutoHit = marker.ShouldHit;
            attackRoll.AutoMiss = !marker.ShouldHit;
            attackRoll.AutoCriticalThreat = marker.CriticalThreat;
            attackRoll.CriticalConfirmationBonus = marker.ConfirmationPenalty;
        }

        internal static void BeforeSetRoll(RuleAttackRoll attackRoll,
            ref Kingmaker.RuleSystem.RulebookEvent.RollEntry value)
        {
            ProbeContext context;
            if (!TryGet(attackRoll, out context)) return;
            if (context.ForcedNaturalRoll.HasValue)
            {
                int forced = context.ForcedNaturalRoll.Value;
                List<int> history = value.RollHistory == null ? new List<int>() :
                    new List<int>(value.RollHistory);
                if (history.Count == 0) history.Add(forced);
                else history[history.Count - 1] = forced;
                value.Value = forced;
                value.RollHistory = history;
            }
            context.RecordNaturalRoll(value.Value);
        }

        internal static void AfterIsSuccessRoll(RuleAttackRoll attackRoll,
            int naturalRoll, ref bool nativeResult)
        {
            ProbeContext context;
            if (!TryGet(attackRoll, out context)) return;
            context.VerifyNaturalRoll(naturalRoll);
            if (context.IsMisfire) nativeResult = false;
        }

        internal static DeadShotRollObservation ConsumeProbe(RuleAttackRoll attackRoll)
        {
            if (attackRoll == null) throw new ArgumentNullException("attackRoll");
            ProbeContext context;
            lock (Gate)
            {
                if (!Probes.TryGetValue(attackRoll, out context))
                    throw new InvalidOperationException("Dead Shot probe context is missing.");
                Probes.Remove(attackRoll);
            }
            if (!context.HasNaturalRoll)
                throw new InvalidOperationException("Dead Shot probe exposed no natural roll.");
            return new DeadShotRollObservation(attackRoll.IsHit,
                context.IsMisfire, attackRoll.IsCriticalRoll && attackRoll.IsHit);
        }

        internal static void CancelProbe(RuleAttackRoll attackRoll)
        {
            if (attackRoll == null) return;
            lock (Gate) { Probes.Remove(attackRoll); }
        }

        private static bool TryGet(RuleAttackRoll attackRoll, out ProbeContext context)
        {
            context = null;
            if (attackRoll == null) return false;
            lock (Gate) { return Probes.TryGetValue(attackRoll, out context); }
        }

        private sealed class ProbeContext
        {
            private int _naturalRoll;
            internal ProbeContext(int misfireThreshold, int? forcedNaturalRoll)
            {
                MisfireThreshold = misfireThreshold;
                ForcedNaturalRoll = forcedNaturalRoll;
            }
            internal int MisfireThreshold { get; private set; }
            internal int? ForcedNaturalRoll { get; private set; }
            internal bool HasNaturalRoll { get { return _naturalRoll != 0; } }
            internal bool IsMisfire { get { return _naturalRoll > 0 &&
                _naturalRoll <= MisfireThreshold; } }
            internal void RecordNaturalRoll(int value)
            {
                if (value < 1 || value > 20) throw new ArgumentOutOfRangeException("value");
                if (_naturalRoll != 0) throw new InvalidOperationException(
                    "Dead Shot probe assigned its natural roll more than once.");
                _naturalRoll = value;
            }
            internal void VerifyNaturalRoll(int value)
            {
                if (_naturalRoll == 0 || _naturalRoll != value)
                    throw new InvalidOperationException(
                        "Dead Shot success evaluation did not match its natural roll.");
            }
        }

        private sealed class DeliveryMarker
        {
            internal DeliveryMarker(bool shouldHit, bool criticalThreat,
                int confirmationPenalty)
            {
                ShouldHit = shouldHit;
                CriticalThreat = criticalThreat;
                ConfirmationPenalty = confirmationPenalty;
            }
            internal bool ShouldHit { get; private set; }
            internal bool CriticalThreat { get; private set; }
            internal int ConfirmationPenalty { get; private set; }
        }
    }
}
