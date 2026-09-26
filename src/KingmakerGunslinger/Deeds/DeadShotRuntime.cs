using System;
using System.Collections.Generic;
using System.Threading;
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
        private static long _registeredProbes;
        private static long _rollSetterProbes;
        private static long _successProbes;
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
                firearm.Definition.IsScatter, firearm.EffectiveCondition,
                state.LoadedRounds,
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
            return ExecuteForRuntimeTest(caster, target, null, forcedRolls);
        }

        /// <summary>Runtime test: forced probe rolls and a forced confirmation roll.</summary>
        internal static DeadShotExecutionResult ExecuteForRuntimeTestConfirming(
            UnitEntityData caster, UnitEntityData target, int confirmationRoll,
            params int[] forcedRolls)
        {
            if (confirmationRoll < 1 || confirmationRoll > 20)
                throw new ArgumentOutOfRangeException("confirmationRoll");
            return ExecuteForRuntimeTest(caster, target, confirmationRoll, forcedRolls);
        }

        /// <summary>
        /// Runtime test: as ExecuteForRuntimeTestConfirming, with the party
        /// critical setting supplied to this shot's own confirmation only; the
        /// game's EnemyCriticalHits setting is never read or written.
        /// </summary>
        internal static DeadShotExecutionResult ExecuteForRuntimeTestConfirming(
            UnitEntityData caster, UnitEntityData target, int confirmationRoll,
            bool partyCriticalsAllowed, params int[] forcedRolls)
        {
            if (confirmationRoll < 1 || confirmationRoll > 20)
                throw new ArgumentOutOfRangeException("confirmationRoll");
            return ExecuteForRuntimeTest(caster, target, confirmationRoll, forcedRolls,
                partyCriticalsAllowed);
        }

        private static DeadShotExecutionResult ExecuteForRuntimeTest(
            UnitEntityData caster, UnitEntityData target, int? confirmationRoll,
            int[] forcedRolls, bool? partyCriticalsAllowed = null)
        {
            if (caster == null || target == null) throw new ArgumentNullException("caster");
            return Execute(caster.Descriptor, caster, target,
                delegate(RuleAttackRoll rule) {
                    PrepareRuntimeTestRoll(rule);
                    Rulebook.Trigger(rule);
                },
                delegate(RuleAttackWithWeapon rule) { Rulebook.Trigger(rule); },
                forcedRolls, confirmationRoll, partyCriticalsAllowed);
        }

        private static DeadShotExecutionResult Execute(UnitDescriptor caster,
            UnitEntityData casterEntity, UnitEntityData target,
            Action<RuleAttackRoll> triggerProbe,
            Action<RuleAttackWithWeapon> triggerDelivery,
            int[] forcedRolls = null, int? forcedConfirmationRoll = null,
            bool? forcedPartyCriticals = null)
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
                FirearmDischargeResult discharge = Discharge.Evaluate(
                    before, firearm.EffectiveCondition);
                if (discharge.Status != FirearmDischargeStatus.Fired)
                    throw new InvalidOperationException("Accepted Dead Shot did not discharge.");
                FirearmItemStateSnapshot postDischarge = Transition(firearm,
                    expectedCurrent, discharge.After);
                expectedCurrent = postDischarge.Repository.State;

                int threshold = global::KingmakerGunslinger.Misfires.EffectiveFirearmMisfirePolicy.Evaluate(
                    firearm.Definition.MisfireValue, firearm.EffectiveCondition,
                    Classes.FirearmTrainingRuntime.Resolve(casterEntity,
                        firearm.Definition.Kind).ReducedBrokenMisfire,
                    firearm.Weapon, before.LoadedAmmunition,
                    global::KingmakerGunslinger.FavoredClass.Mechanics.FavoredClassEarnedSteps
                        .MisfireReduction(casterEntity, firearm.Definition.Kind));
                var probes = new RuleAttackRoll[decision.AttackBonuses.Length];
                var observations = new DeadShotRollObservation[probes.Length];
                for (int index = 0; index < probes.Length; index++)
                {
                    var probe = new RuleAttackRoll(casterEntity, target,
                        firearm.Weapon, -(index * 5));
                    // A probe only reports hit, misfire and threat (its natural
                    // roll against the critical edge, ConsumeProbe); it never
                    // rolls a confirmation or fires critical triggers of its
                    // own. The shot confirms once, in its delivery.
                    probe.ImmuneToCriticalHit = true;
                    RegisterProbe(probe, threshold, forcedRolls == null ?
                        (int?)null : forcedRolls[index]);
                    try { triggerProbe(probe); observations[index] = ConsumeProbe(probe); }
                    catch { CancelProbe(probe); throw; }
                    probes[index] = probe;
                }
                DeadShotOutcome outcome = Outcomes.Evaluate(decision, observations);
                FirearmMisfireConditionDecision condition = null;
                FirearmItemStateSnapshot conditionCommit = null;
                if (outcome.Misfires)
                {
                    condition = Conditions.Evaluate(Misfires.Evaluate(1, threshold,
                        false), expectedCurrent, firearm.EffectiveCondition);
                    if (condition.ChangesCondition)
                    {
                        conditionCommit = Transition(firearm,
                            expectedCurrent, condition.After);
                        expectedCurrent = conditionCommit.Repository.State;
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
                    outcome.ConfirmationPenalty ?? 0, Math.Max(1, outcome.BaseDamageDicePackets),
                    forcedConfirmationRoll, forcedPartyCriticals);
                DeadShotConfirmationRecord confirmation;
                try
                {
                    triggerDelivery(delivery);
                    confirmation = ConfirmationOf(delivery);
                }
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
                if (!outcome.Misfires)
                    Audio.FirearmSoundRuntime.TryPostCommittedDischarge(
                        firearm.Definition.Kind, casterEntity, "dead-shot");
                var result = new DeadShotExecutionResult(decision, outcome, probes,
                    delivery, before, expectedCurrent);
                result.Confirmation = confirmation;
                if (conditionCommit != null)
                {
                    FirearmConditionTopNotification
                        .PublishAfterCommittedDegradation(
                            casterEntity.CharacterName,
                            conditionCommit.ItemDisplayName,
                            condition.Before.Condition,
                            condition.After.Condition,
                            "Dead Shot misfire");
                    // Publish the interruption only at this verified
                    // irrevocable boundary - after every fallible step of
                    // the composite shot has completed - so a later failure
                    // that rolls the firearm back can never leave a stale
                    // suppression/epoch behind (review CR2-02). No
                    // automatic continuation can run before this method
                    // returns.
                    Firing.BrokenSequenceSuppressionRuntime
                        .OnCommittedDegradation(casterEntity, firearm.Weapon);
                }
                return result;
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

        internal static void RegisterProbe(RuleAttackRoll attackRoll,
            int misfireThreshold, int? forcedNaturalRoll = null)
        {
            if (attackRoll == null) throw new ArgumentNullException("attackRoll");
            if (misfireThreshold <
                    global::KingmakerGunslinger.Misfires.EffectiveFirearmMisfirePolicy.MinimumEffectiveValue ||
                misfireThreshold >
                    global::KingmakerGunslinger.Misfires.EffectiveFirearmMisfirePolicy.MaximumEffectiveValue)
                throw new ArgumentOutOfRangeException("misfireThreshold");
            if (forcedNaturalRoll.HasValue && (forcedNaturalRoll.Value < 1 ||
                forcedNaturalRoll.Value > 20))
                throw new ArgumentOutOfRangeException("forcedNaturalRoll");
            lock (Gate)
            {
                Probes.Remove(attackRoll);
                Probes.Add(attackRoll, new ProbeContext(misfireThreshold,
                    forcedNaturalRoll));
                Interlocked.Increment(ref _registeredProbes);
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
            bool shouldHit, bool criticalThreat, int confirmationPenalty,
            int hitCount = 1, int? forcedConfirmationRoll = null,
            bool? forcedPartyCriticals = null)
        {
            if (attack == null) throw new ArgumentNullException("attack");
            lock (Gate)
            {
                Deliveries.Remove(attack);
                Deliveries.Add(attack, new DeliveryMarker(shouldHit,
                    criticalThreat, confirmationPenalty, hitCount,
                    forcedConfirmationRoll, forcedPartyCriticals));
            }
        }

        /// <summary>The confirmation the delivery made, or null when it had no threat.</summary>
        private static DeadShotConfirmationRecord ConfirmationOf(RuleAttackWithWeapon attack)
        {
            DeliveryMarker marker;
            lock (Gate)
            {
                return Deliveries.TryGetValue(attack, out marker) ? marker.Confirmation : null;
            }
        }

        internal static void CancelDelivery(RuleAttackWithWeapon attack)
        {
            if (attack == null) return;
            lock (Gate) { Deliveries.Remove(attack); }
        }

        internal static int FocusedAimMultiplier(RuleAttackWithWeapon attack)
        {
            if (attack == null) return 1;
            DeliveryMarker marker;
            lock (Gate) { return Deliveries.TryGetValue(attack, out marker) ?
                marker.HitCount : 1; }
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
            DeliveryMarker marker = DeliveryOf(attackRoll);
            if (marker == null) return;
            attackRoll.AutoHit = marker.ShouldHit;
            attackRoll.AutoMiss = !marker.ShouldHit;
            // An auto-hit roll never threatens natively; ConfirmDelivery
            // makes the shot's one confirmation.
            attackRoll.AutoCriticalThreat = false;
        }

        /// <summary>
        /// D2: the delivery's one critical confirmation. It runs after the
        /// attack's firearm AC frame is pushed (FirearmArmorClassRuntime
        /// .BeforeAttackRoll), so its critical AC takes the firearm touch-AC
        /// rule and Deadeye exactly as a native roll's would.
        /// </summary>
        internal static void ConfirmDelivery(RuleAttackRoll attackRoll)
        {
            DeliveryMarker marker = DeliveryOf(attackRoll);
            if (marker == null || !marker.ShouldHit || !marker.CriticalThreat)
                return;
            // The native roll confirms only when it rolls to hit; the delivery
            // auto-hits (its probes decided the hit), so the one confirmation
            // is made here, before OnTrigger, with the native rules: the
            // penalty is added to every confirmation bonus already on the roll
            // (Critical Focus, favored-class bonuses), never assigned over them.
            attackRoll.CriticalConfirmationBonus += marker.ConfirmationPenalty;
            DeadShotConfirmationRecord confirmation;
            try
            {
                confirmation = Confirm(attackRoll, marker.ConfirmationPenalty,
                    marker.ForcedConfirmationRoll, marker.ForcedPartyCriticals);
            }
            catch (Exception exception)
            {
                // Contained: the attack prefix must not throw with the AC
                // frame pushed. The shot stays an ordinary hit.
                confirmation = new DeadShotConfirmationRecord(
                    "confirmation failed: " + exception.GetType().Name);
                ModContext context;
                if (ModContext.TryGet(out context))
                    context.Logger.Failure("deeds", "dead-shot.confirmation.failed",
                        "The Dead Shot critical confirmation failed; the shot " +
                        "resolves as an ordinary hit.", exception);
            }
            marker.Confirmation = confirmation;
            attackRoll.AutoCriticalThreat = confirmation.Blocked == null;
            attackRoll.AutoCriticalConfirmation = confirmation.Confirmed;
        }

        private static DeliveryMarker DeliveryOf(RuleAttackRoll attackRoll)
        {
            if (attackRoll == null || attackRoll.RuleAttackWithWeapon == null)
                return null;
            DeliveryMarker marker;
            lock (Gate)
            {
                return Deliveries.TryGetValue(attackRoll.RuleAttackWithWeapon,
                    out marker) ? marker : null;
            }
        }

        private static DeadShotConfirmationRecord Confirm(RuleAttackRoll attackRoll, int penalty,
            int? forcedRoll, bool? forcedPartyCriticals)
        {
            // Target immunity (AddImmunityToCriticalHits) and the party
            // critical setting apply exactly as to the native threat, before
            // any roll: a blocked threat never reaches a natural 20.
            string blocked = DeadShotConfirmationPolicy.Blocked(attackRoll.ImmuneToCriticalHit,
                attackRoll.Target.IsPlayerFaction,
                forcedPartyCriticals ?? PartyCriticalsAllowed());
            if (blocked != null)
                return new DeadShotConfirmationRecord(blocked);
            int attackBonus = Rulebook.Trigger(new RuleCalculateAttackBonus(attackRoll.Initiator,
                attackRoll.Target, attackRoll.Weapon, attackRoll.AttackBonusPenalty)).Result;
            int criticalArmorClass = Rulebook.Trigger(new RuleCalculateAC(attackRoll.Initiator,
                attackRoll.Target, attackRoll.AttackType) { IsCritical = true }).TargetAC;
            int roll = forcedRoll ?? RulebookEvent.Dice.D20.Value;
            return new DeadShotConfirmationRecord(roll, attackBonus,
                attackRoll.CriticalConfirmationBonus, penalty, criticalArmorClass);
        }

        /// <summary>The native RuleAttackRoll's party rule: criticals on the party only at Weak or Normal.</summary>
        private static bool PartyCriticalsAllowed()
        {
            Kingmaker.UI.SettingsUI.CriticalHitPower critsOnParty =
                Kingmaker.Game.Instance.Player.Difficulty.CritsOnParty;
            return critsOnParty == Kingmaker.UI.SettingsUI.CriticalHitPower.Weak ||
                critsOnParty == Kingmaker.UI.SettingsUI.CriticalHitPower.Normal;
        }

        internal static void BeforeSetRoll(RuleAttackRoll attackRoll,
            ref Kingmaker.RuleSystem.RulebookEvent.RollEntry value)
        {
            ProbeContext context;
            if (!TryGet(attackRoll, out context)) return;
            Interlocked.Increment(ref _rollSetterProbes);
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
            Interlocked.Increment(ref _successProbes);
            if (!context.HasNaturalRoll)
                context.RecordNaturalRoll(naturalRoll);
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
                throw new InvalidOperationException(
                    "Dead Shot probe exposed no natural roll; registered=" +
                    Interlocked.Read(ref _registeredProbes) + ";rollSetter=" +
                    Interlocked.Read(ref _rollSetterProbes) + ";success=" +
                    Interlocked.Read(ref _successProbes) + ".");
            return new DeadShotRollObservation(attackRoll.IsHit,
                context.IsMisfire, DeadShotConfirmationPolicy.IsThreat(attackRoll.IsHit,
                    context.IsMisfire, context.NaturalRoll, attackRoll.WeaponStats.CriticalEdge));
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

        private static void PrepareRuntimeTestRoll(RuleAttackRoll attackRoll)
        {
            ProbeContext context;
            if (!TryGet(attackRoll, out context) ||
                !context.ForcedNaturalRoll.HasValue) return;
            int expected = context.ForcedNaturalRoll.Value;
            for (int seed = 1; seed <= 100000; seed++)
            {
                UnityEngine.Random.InitState(seed);
                if (RulebookEvent.Dice.D20.Value != expected) continue;
                UnityEngine.Random.InitState(seed);
                return;
            }
            throw new InvalidOperationException(
                "No deterministic native d20 seed produced " + expected + ".");
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
            internal int NaturalRoll { get { return _naturalRoll; } }
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
                if (ForcedNaturalRoll.HasValue &&
                    ForcedNaturalRoll.Value != value)
                    throw new InvalidOperationException(
                        "Dead Shot runtime-test forced roll did not match the native d20.");
            }
        }

        private sealed class DeliveryMarker
        {
            internal DeliveryMarker(bool shouldHit, bool criticalThreat,
                int confirmationPenalty, int hitCount, int? forcedConfirmationRoll,
                bool? forcedPartyCriticals)
            {
                ShouldHit = shouldHit;
                CriticalThreat = criticalThreat;
                ConfirmationPenalty = confirmationPenalty;
                HitCount = hitCount;
                ForcedConfirmationRoll = forcedConfirmationRoll;
                ForcedPartyCriticals = forcedPartyCriticals;
            }
            internal bool ShouldHit { get; private set; }
            internal bool CriticalThreat { get; private set; }
            internal int ConfirmationPenalty { get; private set; }
            internal int HitCount { get; private set; }
            /// <summary>Runtime test only: the confirmation's natural roll; null in play.</summary>
            internal int? ForcedConfirmationRoll { get; private set; }
            /// <summary>Runtime test only: the party critical setting for this confirmation; null in play (the game setting).</summary>
            internal bool? ForcedPartyCriticals { get; private set; }
            internal DeadShotConfirmationRecord Confirmation { get; set; }
        }
    }
}
