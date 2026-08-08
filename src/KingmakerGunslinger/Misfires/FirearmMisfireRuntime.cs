using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Explosions;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Deeds;

namespace KingmakerGunslinger.Misfires
{
    /// <summary>
    /// Kingmaker adapter for the bounded Sprint 26 misfire and burst-consequence slice. Only a
    /// successfully discharged exact firearm receives an attack context. A native
    /// Heavy Crossbow, an empty firearm, and a Wrecked firearm therefore cannot
    /// consume the diagnostic forced-roll slot or enter misfire evaluation. A
    /// detected misfire mutates only the exact discharged item's token-backed state;
    /// only Broken-to-Wrecked schedules one definition-sized native burst around
    /// the exact wielder.
    /// </summary>
    internal static class FirearmMisfireRuntime
    {
        private static readonly object ContextGate = new object();
        private static readonly ConditionalWeakTable<RuleAttackRoll, EligibleAttackContext>
            EligibleAttacks =
                new ConditionalWeakTable<RuleAttackRoll, EligibleAttackContext>();
        private static readonly ForcedNaturalRollQueue ForcedRolls =
            new ForcedNaturalRollQueue();
        private static readonly FirearmMisfireService Service =
            new FirearmMisfireService();
        private static readonly FirearmMisfireConditionService ConditionService =
            new FirearmMisfireConditionService();
        private static readonly FirearmExplosionService ExplosionService =
            new FirearmExplosionService();

        internal static int? PendingForcedNaturalRoll
        {
            get { return ForcedRolls.Pending; }
        }

        internal static bool TryRegisterEligibleAttack(
            RuleAttackRoll attackRoll,
            object firearmItem,
            FirearmItemStateSnapshot postDischarge,
            FirearmCondition effectiveCondition)
        {
            try
            {
                if (attackRoll == null)
                {
                    throw new ArgumentNullException("attackRoll");
                }

                if (firearmItem == null)
                {
                    throw new ArgumentNullException("firearmItem");
                }

                if (firearmItem.GetType().IsValueType)
                {
                    throw new ArgumentException(
                        "A runtime firearm item must have reference identity.",
                        "firearmItem");
                }

                if (postDischarge == null)
                {
                    throw new ArgumentNullException("postDischarge");
                }

                FirearmState postDischargeState = postDischarge.Repository.State;
                if (!postDischargeState.IsEmpty)
                {
                    throw new ArgumentException(
                        "An eligible misfire context requires the exact firearm's loaded round to have been discharged.",
                        "postDischarge");
                }

                if (effectiveCondition == FirearmCondition.Wrecked)
                {
                    throw new ArgumentException(
                        "A Wrecked firearm cannot be registered as a successfully discharged attack.",
                        "postDischarge");
                }

                UnitEntityData wielder = attackRoll.Initiator;
                if (wielder == null)
                {
                    throw new ArgumentException(
                        "An eligible firearm attack requires the exact Kingmaker wielder reference.",
                        "attackRoll");
                }

                var context = new EligibleAttackContext(
                    firearmItem,
                    wielder,
                    postDischargeState,
                    effectiveCondition,
                    postDischarge.Repository.RepositoryIdentity,
                    Classes.GunTrainingPolicy.EffectiveMisfireValue(
                        postDischarge.Definition.MisfireValue,
                        effectiveCondition,
                        Classes.FirearmTrainingRuntime.Resolve(wielder,
                            postDischarge.Definition.Kind).ReducedBrokenMisfire),
                    postDischarge.Definition.MisfireBurstRadiusFeet,
                    Normalize(postDischarge.ItemDisplayName),
                    postDischarge.Definition.Kind);
                lock (ContextGate)
                {
                    EligibleAttacks.Remove(attackRoll);
                    EligibleAttacks.Add(attackRoll, context);
                }

                FirearmMisfireRuntimeDiagnostics.RecordEligible(
                    context.Firearm,
                    context.MisfireValue,
                    context.PostDischargeState.Condition);
                return true;
            }
            catch (Exception exception)
            {
                FirearmMisfireRuntimeDiagnostics.RecordFault(
                    exception,
                    "register-eligible-attack");
                LogFault(
                    "eligible-context.failed",
                    "The fired firearm could not be registered for natural-roll misfire evaluation.",
                    exception);
                return false;
            }
        }

        internal static void BeforeSetRoll(
            RuleAttackRoll attackRoll,
            ref RulebookEvent.RollEntry value)
        {
            if (attackRoll == null)
            {
                return;
            }

            EligibleAttackContext context;
            if (!EligibleAttacks.TryGetValue(attackRoll, out context))
            {
                return;
            }

            try
            {
                if (!context.TryBeginRollAssignment())
                {
                    FirearmMisfireRuntimeDiagnostics.RecordDuplicateRollAssignment(
                        context.Firearm);
                    return;
                }

                int originalNaturalRoll = value.Value;
                int finalNaturalRoll = originalNaturalRoll;
                int forcedNaturalRoll;
                bool forced = ForcedRolls.TryConsume(out forcedNaturalRoll);
                if (forced)
                {
                    value = WithForcedNaturalRoll(value, forcedNaturalRoll);
                    finalNaturalRoll = forcedNaturalRoll;
                }

                context.RecordNaturalRoll(
                    originalNaturalRoll,
                    finalNaturalRoll,
                    forced);
                FirearmMisfireRuntimeDiagnostics.RecordNaturalRoll(
                    context.Firearm,
                    originalNaturalRoll,
                    finalNaturalRoll,
                    forced);

                if (forced)
                {
                    LogInfo(
                        "diagnostic.force-next-applied",
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Applied forced natural d20={0} to the next eligible exact-firearm attack; originalNaturalD20={1}; firearm={2}.",
                            finalNaturalRoll,
                            originalNaturalRoll,
                            context.Firearm));
                }
            }
            catch (Exception exception)
            {
                FirearmMisfireRuntimeDiagnostics.RecordFault(
                    exception,
                    "assign-natural-roll");
                LogFault(
                    "natural-roll.assignment-failed",
                    "An eligible firearm roll could not be observed or overridden; its later success evaluation will fail closed.",
                    exception);
                context.RecordAssignmentFault();
            }
        }

        internal static void AfterIsSuccessRoll(
            RuleAttackRoll attackRoll,
            int naturalRoll,
            ref bool nativeResult)
        {
            if (attackRoll == null)
            {
                return;
            }

            EligibleAttackContext context;
            if (!EligibleAttacks.TryGetValue(attackRoll, out context))
            {
                return;
            }

            try
            {
                if (context.AssignmentFaulted)
                {
                    throw new InvalidOperationException(
                        "The eligible firearm's natural-roll assignment previously faulted.");
                }

                if (context.FinalNaturalRoll == 0)
                {
                    throw new InvalidOperationException(
                        "IsSuccessRoll ran before the exact Roll setter was observed for the eligible firearm.");
                }

                if (naturalRoll != context.FinalNaturalRoll)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "The IsSuccessRoll argument {0} did not match the observed final natural d20 {1}.",
                            naturalRoll,
                            context.FinalNaturalRoll));
                }

                bool firstEvaluation = context.TryBeginEvaluation();
                bool nativeSuccess = nativeResult;
                FirearmMisfireDecision decision = Service.Evaluate(
                    naturalRoll,
                    context.MisfireValue,
                    nativeResult);
                bool fortuneIgnored = firstEvaluation && decision.IsMisfire &&
                    BlueprintBootstrap.GunslingerClass != null &&
                    BlueprintBootstrap.GunslingerClass.MysteriousStranger != null &&
                    BlueprintBootstrap.GunslingerClass.MysteriousStranger.TryIgnoreMisfire(
                        context.Wielder == null ? null : context.Wielder.Descriptor);
                if (fortuneIgnored)
                {
                    nativeResult = nativeSuccess;
                    Audio.FirearmSoundRuntime.TryPostCommittedDischarge(
                        context.Kind, context.Wielder, "ordinary-attack-fortune-ignored");
                    LogInfo("natural-roll.fortune-ignored",
                        "Stranger's Fortune ignored the armed firearm misfire.");
                    return;
                }
                nativeResult = decision.FinalSuccess;

                if (!firstEvaluation)
                {
                    FirearmMisfireRuntimeDiagnostics.RecordDuplicateEvaluation(
                        context.Firearm);
                    return;
                }

                FirearmMisfireConditionDecision condition =
                    ConditionService.Evaluate(
                        decision,
                        context.PostDischargeState,
                        context.EffectiveCondition);
                condition = ExpertLoadingRuntime.Apply(attackRoll, decision,
                    condition, firstEvaluation);
                if (condition.ChangesCondition)
                {
                    CommitConditionTransition(context, condition);
                }

                if (decision.IsMisfire)
                {
                    FirearmExplosionDecision explosion =
                        ExplosionService.Evaluate(condition);
                    FirearmExplosionRuntimeDiagnostics.RecordDecision(
                        explosion,
                        context.Firearm,
                        context.MisfireBurstRadiusFeet);
                    if (explosion.RequiresBurstDamage &&
                        !context.TryScheduleExplosion())
                    {
                        FirearmExplosionRuntimeDiagnostics.RecordDuplicate(
                            context.Firearm);
                    }
                }

                FirearmMisfireRuntimeDiagnostics.RecordDecision(
                    decision,
                    condition,
                    context.Firearm,
                    context.Forced);
                if (!decision.IsMisfire)
                    Audio.FirearmSoundRuntime.TryPostCommittedDischarge(
                        context.Kind, context.Wielder, "ordinary-attack");
                LogInfo(
                    decision.IsMisfire
                        ? "natural-roll.misfire"
                        : "natural-roll.ordinary",
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "{0}; forced={1}; originalNaturalD20={2}; firearm={3}; {4}.",
                        decision,
                        context.Forced,
                        context.OriginalNaturalRoll,
                        context.Firearm,
                        condition));
            }
            catch (Exception exception)
            {
                nativeResult = false;
                FirearmMisfireRuntimeDiagnostics.RecordFault(
                    exception,
                    "evaluate-natural-roll");
                LogFault(
                    "natural-roll.evaluation-failed",
                    "Natural-roll misfire evaluation or exact-item condition mutation failed; the attack was forced to miss.",
                    exception);
            }
        }

        internal static string QueueForcedNaturalRoll(int naturalRoll)
        {
            int? previous = ForcedRolls.Set(naturalRoll);
            string message = previous.HasValue
                ? string.Format(
                    CultureInfo.InvariantCulture,
                    "Forced natural d20={0} is queued for the next eligible exact-firearm roll, replacing pending d20={1}.",
                    naturalRoll,
                    previous.Value)
                : string.Format(
                    CultureInfo.InvariantCulture,
                    "Forced natural d20={0} is queued for the next eligible exact-firearm roll.",
                    naturalRoll);
            FirearmMisfireRuntimeDiagnostics.RecordQueueChange(message);
            LogInfo("diagnostic.force-next-queued", message);
            return message;
        }

        internal static string CancelForcedNaturalRoll()
        {
            int? previous = ForcedRolls.Cancel();
            string message = previous.HasValue
                ? string.Format(
                    CultureInfo.InvariantCulture,
                    "Canceled pending forced natural d20={0}.",
                    previous.Value)
                : "No forced natural d20 was pending.";
            FirearmMisfireRuntimeDiagnostics.RecordQueueChange(message);
            LogInfo("diagnostic.force-next-canceled", message);
            return message;
        }

        internal static string Describe()
        {
            return FirearmMisfireRuntimeDiagnostics.Describe(
                PendingForcedNaturalRoll);
        }

        internal static void FinishAttack(RuleAttackRoll attackRoll)
        {
            if (attackRoll == null)
            {
                return;
            }

            EligibleAttackContext context;
            lock (ContextGate)
            {
                if (!EligibleAttacks.TryGetValue(attackRoll, out context))
                {
                    return;
                }

                EligibleAttacks.Remove(attackRoll);
            }

            if (context.FinalNaturalRoll == 0)
            {
                FirearmMisfireRuntimeDiagnostics.RecordCompletedWithoutNaturalRoll(
                    context.Firearm);
                return;
            }

            if (context.ExplosionRequired)
            {
                if (!context.TryBeginExplosion())
                {
                    FirearmExplosionRuntimeDiagnostics.RecordDuplicate(
                        context.Firearm);
                    return;
                }

                FirearmExplosionRuntime.Apply(
                    attackRoll,
                    context.FirearmItem,
                    context.Wielder,
                    context.RepositoryIdentity,
                    context.MisfireBurstRadiusFeet,
                    context.Firearm);
            }
        }

        internal static bool IsEligibleAttack(RuleAttackRoll attackRoll)
        {
            if (attackRoll == null) return false;
            lock (ContextGate)
            {
                EligibleAttackContext ignored;
                return EligibleAttacks.TryGetValue(attackRoll, out ignored);
            }
        }

        private static void CommitConditionTransition(
            EligibleAttackContext context,
            FirearmMisfireConditionDecision condition)
        {
            FirearmItemStateSnapshot committed = FirearmRuntimeState.Service.Transition(
                context.FirearmItem,
                current =>
                {
                    if (current != condition.Before)
                    {
                        throw new InvalidOperationException(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "The exact firearm state changed after discharge and before misfire damage; expected=[{0}]; current=[{1}].",
                                condition.Before,
                                current));
                    }

                    return condition.After;
                });

            if (committed.Repository.State != condition.After)
            {
                throw new InvalidOperationException(
                    "The exact firearm's misfire condition transition did not verify after commit.");
            }

            if (!string.Equals(
                    committed.Repository.RepositoryIdentity,
                    context.RepositoryIdentity,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "The misfire condition transition committed through a different repository identity than the discharged firearm.");
            }

            FirearmConditionCombatLog.Publish(
                committed.ItemDisplayName,
                condition.Before.Condition,
                condition.After.Condition,
                "misfire");
        }

        private static RulebookEvent.RollEntry WithForcedNaturalRoll(
            RulebookEvent.RollEntry original,
            int forcedNaturalRoll)
        {
            List<int> history = original.RollHistory == null
                ? new List<int>()
                : new List<int>(original.RollHistory);
            if (history.Count == 0)
            {
                history.Add(forcedNaturalRoll);
            }
            else
            {
                history[history.Count - 1] = forcedNaturalRoll;
            }

            original.Value = forcedNaturalRoll;
            original.RollHistory = history;
            return original;
        }

        private static void LogInfo(string eventName, string message)
        {
            ModContext context;
            if (ModContext.TryGet(out context))
            {
                context.Logger.Info("misfire", eventName, message);
            }
        }

        private static void LogFault(
            string eventName,
            string message,
            Exception exception)
        {
            ModContext context;
            if (ModContext.TryGet(out context))
            {
                context.Logger.Failure(
                    "misfire",
                    eventName,
                    message,
                    exception);
            }
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? "<unavailable>"
                : value.Trim();
        }

        private sealed class EligibleAttackContext
        {
            private int _rollAssignmentStarted;
            private int _evaluationStarted;
            private int _assignmentFaulted;
            private int _explosionRequired;
            private int _explosionStarted;
            private int _originalNaturalRoll;
            private int _finalNaturalRoll;
            private int _forced;

            internal EligibleAttackContext(
                object firearmItem,
                UnitEntityData wielder,
                FirearmState postDischargeState,
                FirearmCondition effectiveCondition,
                string repositoryIdentity,
                int misfireValue,
                int misfireBurstRadiusFeet,
                string firearm,
                FirearmKind kind)
            {
                if (firearmItem == null)
                {
                    throw new ArgumentNullException("firearmItem");
                }

                if (wielder == null)
                {
                    throw new ArgumentNullException("wielder");
                }

                if (postDischargeState == null)
                {
                    throw new ArgumentNullException("postDischargeState");
                }

                if (!postDischargeState.IsEmpty ||
                    effectiveCondition == FirearmCondition.Wrecked)
                {
                    throw new ArgumentException(
                        "An eligible attack context requires an empty Normal or Broken post-discharge state.",
                        "postDischargeState");
                }

                if (string.IsNullOrWhiteSpace(repositoryIdentity))
                {
                    throw new ArgumentException(
                        "A repository identity is required.",
                        "repositoryIdentity");
                }

                if (misfireValue < FirearmDefinition.MinimumMisfireValue ||
                    misfireValue > FirearmDefinition.MaximumMisfireValue)
                {
                    throw new ArgumentOutOfRangeException("misfireValue");
                }

                if (misfireBurstRadiusFeet <
                        FirearmDefinition.MinimumMisfireBurstRadiusFeet ||
                    misfireBurstRadiusFeet >
                        FirearmDefinition.MaximumMisfireBurstRadiusFeet ||
                    misfireBurstRadiusFeet % 5 != 0)
                {
                    throw new ArgumentOutOfRangeException(
                        "misfireBurstRadiusFeet");
                }

                FirearmItem = firearmItem;
                Wielder = wielder;
                PostDischargeState = postDischargeState;
                EffectiveCondition = effectiveCondition;
                RepositoryIdentity = repositoryIdentity.Trim();
                MisfireValue = misfireValue;
                MisfireBurstRadiusFeet = misfireBurstRadiusFeet;
                Firearm = firearm ?? throw new ArgumentNullException("firearm");
                Kind = kind;
            }

            internal object FirearmItem { get; private set; }

            internal UnitEntityData Wielder { get; private set; }

            internal FirearmState PostDischargeState { get; private set; }

            internal FirearmCondition EffectiveCondition { get; private set; }

            internal string RepositoryIdentity { get; private set; }

            internal int MisfireValue { get; private set; }

            internal int MisfireBurstRadiusFeet { get; private set; }

            internal string Firearm { get; private set; }
            internal FirearmKind Kind { get; private set; }

            internal int OriginalNaturalRoll
            {
                get { return Interlocked.CompareExchange(ref _originalNaturalRoll, 0, 0); }
            }

            internal int FinalNaturalRoll
            {
                get { return Interlocked.CompareExchange(ref _finalNaturalRoll, 0, 0); }
            }

            internal bool Forced
            {
                get { return Interlocked.CompareExchange(ref _forced, 0, 0) != 0; }
            }

            internal bool AssignmentFaulted
            {
                get { return Interlocked.CompareExchange(ref _assignmentFaulted, 0, 0) != 0; }
            }

            internal bool ExplosionRequired
            {
                get { return Interlocked.CompareExchange(ref _explosionRequired, 0, 0) != 0; }
            }

            internal bool TryBeginRollAssignment()
            {
                return Interlocked.CompareExchange(
                    ref _rollAssignmentStarted,
                    1,
                    0) == 0;
            }

            internal void RecordNaturalRoll(
                int originalNaturalRoll,
                int finalNaturalRoll,
                bool forced)
            {
                Interlocked.Exchange(
                    ref _originalNaturalRoll,
                    originalNaturalRoll);
                Interlocked.Exchange(
                    ref _finalNaturalRoll,
                    finalNaturalRoll);
                Interlocked.Exchange(ref _forced, forced ? 1 : 0);
            }

            internal void RecordAssignmentFault()
            {
                Interlocked.Exchange(ref _assignmentFaulted, 1);
            }

            internal bool TryBeginEvaluation()
            {
                return Interlocked.CompareExchange(
                    ref _evaluationStarted,
                    1,
                    0) == 0;
            }

            internal bool TryScheduleExplosion()
            {
                return Interlocked.CompareExchange(
                    ref _explosionRequired,
                    1,
                    0) == 0;
            }

            internal bool TryBeginExplosion()
            {
                return Interlocked.CompareExchange(
                    ref _explosionStarted,
                    1,
                    0) == 0;
            }
        }
    }
}
