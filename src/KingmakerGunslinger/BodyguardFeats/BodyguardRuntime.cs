using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics.ContextData;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Rules;

namespace KingmakerGunslinger.BodyguardFeats
{
    /// <summary>
    /// Attack-scoped Owlcat adapter for Bodyguard and In Harm's Way. All lasting
    /// action state belongs to UnitCombatState; this runtime retains only exact
    /// rule-event correlation and temporary delivery mutations.
    /// </summary>
    internal static class BodyguardRuntime
    {
        private const double MetersToFeet = 3.280839895013123d;
        private const double AdjacencyFeet = 5d;
        private const double DistanceToleranceFeet = 0.0011d;
        private static readonly object StateGate = new object();
        private static ConditionalWeakTable<RuleAttackRoll, RuntimeFrame>
            FramesByRoll = new ConditionalWeakTable<RuleAttackRoll, RuntimeFrame>();
        private static ConditionalWeakTable<RuleAttackWithWeapon, RuntimeFrame>
            FramesByWeapon = new ConditionalWeakTable<RuleAttackWithWeapon,
                RuntimeFrame>();
        private static ConditionalWeakTable<AbilityDeliveryTarget,
            RuntimeDelivery> DeliveriesByTarget = new ConditionalWeakTable<
                AbilityDeliveryTarget, RuntimeDelivery>();
        private static readonly HashSet<RuntimeFrame> RedirectedFrames =
            new HashSet<RuntimeFrame>(ReferenceComparer<RuntimeFrame>.Instance);
        private static long _nextAttackIdentity;

        [ThreadStatic]
        private static Stack<RuntimeFrame> _activeFrames;

        internal static void BeforeAttackRoll(RuleAttackRoll attack)
        {
            if (attack == null || BodyguardSyntheticAidContext.IsActive ||
                !ModuleEnabled()) return;
            RuntimeFrame duplicate;
            if (FramesByRoll.TryGetValue(attack, out duplicate))
            {
                BodyguardRuntimeDiagnostics.Duplicate(
                    "stage=attack-prefix;attack=" + duplicate.Identity);
                return;
            }

            UnitEntityData attacker = attack.Initiator;
            UnitEntityData target = attack.Target;
            if (!IsSupportedHostileAttack(attack, attacker, target)) return;

            try
            {
                List<RuntimeCandidate> candidates = GatherCandidates(attacker,
                    target);
                if (candidates.Count == 0) return;

                string identity = "bodyguard-attack-" +
                    Interlocked.Increment(ref _nextAttackIdentity).ToString(
                        CultureInfo.InvariantCulture);
                var frame = new RuntimeFrame(identity, attack, attacker, target);
                FramesByRoll.Add(attack, frame);
                if (attack.RuleAttackWithWeapon != null)
                    FramesByWeapon.Add(attack.RuleAttackWithWeapon, frame);
                Push(frame);

                foreach (RuntimeCandidate candidate in candidates)
                    CommitAttempt(frame, candidate);
                frame.Policy.FinishBodyguard();
                frame.BodyguardFinished = true;
                frame.CandidateOrder = string.Join(",", candidates.Select(value =>
                    value.Protector.UniqueId).ToArray());
                BodyguardRuntimeDiagnostics.Frame(DescribeFrame(frame,
                    "bodyguard-resolved"));
                LogInfo("frame.bodyguard", DescribeFrame(frame,
                    "bodyguard-resolved"));
            }
            catch (Exception exception)
            {
                RuntimeFrame frame;
                if (FramesByRoll.TryGetValue(attack, out frame))
                {
                    frame.Policy.Fault();
                    RemoveUncommittedFrame(frame);
                }
                RecordFault("attack-prefix", exception,
                    "attacker=" + Identity(attacker) + ";target=" +
                    Identity(target));
            }
        }

        internal static void AfterCalculateArmorClass(RuleCalculateAC armorClass)
        {
            RuntimeFrame frame = Peek();
            if (frame == null || armorClass == null || !frame.BodyguardFinished ||
                !ReferenceEquals(armorClass.Initiator, frame.Attacker) ||
                !ReferenceEquals(armorClass.Target, frame.OriginalTarget)) return;
            int bonus = frame.Policy.ArmorClassBonus;
            if (bonus <= 0)
            {
                frame.ArmorClassObserved = true;
                frame.ArmorClassBefore = armorClass.TargetAC;
                frame.ArmorClassUsed = armorClass.TargetAC;
                return;
            }
            if (!frame.Policy.TryApplyArmorClass(armorClass))
            {
                BodyguardRuntimeDiagnostics.Duplicate(
                    "stage=armor-class;attack=" + frame.Identity);
                return;
            }
            try
            {
                int before;
                string member;
                if (!KingmakerArmorClassAccess.TryReadTargetArmorClass(
                        armorClass, out before, out member) ||
                    !KingmakerArmorClassAccess.TryWriteTargetArmorClass(
                        armorClass, checked(before + bonus), out member))
                    throw new InvalidOperationException(
                        "RuleCalculateAC did not expose the exact writable TargetAC contract.");
                frame.ArmorClassObserved = true;
                frame.ArmorClassBefore = before;
                frame.ArmorClassUsed = armorClass.TargetAC;
                LogInfo("armor-class.applied", DescribeFrame(frame,
                    "armor-class") + ";acBefore=" + before + ";contribution=" +
                    bonus + ";acUsed=" + armorClass.TargetAC);
                BodyguardRuntimeDiagnostics.Observation(DescribeFrame(frame,
                    "armor-class") + ";acBefore=" + before +
                    ";contribution=" + bonus + ";acUsed=" +
                    armorClass.TargetAC);
            }
            catch (Exception exception)
            {
                frame.ArmorClassFault = true;
                RecordFault("armor-class", exception,
                    DescribeFrame(frame, "armor-class-fault"));
            }
        }

        internal static void AfterAttackRoll(RuleAttackRoll attack)
        {
            if (attack == null) return;
            RuntimeFrame frame;
            if (!FramesByRoll.TryGetValue(attack, out frame)) return;
            if (frame.ResultProcessed)
            {
                BodyguardRuntimeDiagnostics.Duplicate(
                    "stage=attack-result;attack=" + frame.Identity);
                return;
            }
            frame.ResultProcessed = true;

            try
            {
                bool hit = attack.IsHit;
                if (!frame.Policy.TryResolveAttack(hit))
                {
                    BodyguardRuntimeDiagnostics.Duplicate(
                        "stage=attack-resolve;attack=" + frame.Identity);
                    return;
                }
                frame.AttackHit = hit;
                frame.AttackRoll = attack.Roll;
                frame.AttackBonus = attack.AttackBonus;
                frame.AttackTargetArmorClass = attack.TargetAC;
                if (!hit || frame.ArmorClassFault ||
                    !frame.ArmorClassObserved ||
                    frame.SuccessfulAttempts.Count() == 0)
                {
                    CompleteWithoutInterception(frame,
                        hit ? "hit-no-interceptor" : "miss");
                    return;
                }

                if (!InHarmsWayDeliveryAccess.ContractAvailable)
                {
                    CompleteWithoutInterception(frame,
                        "delivery-contract-unavailable");
                    LogWarningOnce("delivery-contract.unavailable",
                        InHarmsWayDeliveryAccess.ContractDescription);
                    return;
                }

                BodyguardFeatBlueprintSet blueprints = BlueprintBootstrap.BodyguardFeats;
                BodyguardInterceptorCandidate[] ordered =
                    BodyguardInterceptionPolicy.OrderEligible(ModuleEnabled(),
                        hit, frame.Interceptor != null,
                        frame.SuccessfulAttempts.Select(value =>
                            CreateInterceptorCandidate(value, blueprints)));
                frame.ArbitrationOrder = string.Join(",", ordered.Select(value =>
                    value.PersistentId).ToArray());

                foreach (BodyguardInterceptorCandidate orderedCandidate in ordered)
                {
                    RuntimeAttempt attempt = frame.SuccessfulAttempts.First(value =>
                        string.Equals(value.Protector.UniqueId,
                            orderedCandidate.PersistentId,
                            StringComparison.Ordinal));
                    float before;
                    float after;
                    if (!BodyguardActionEconomyAccess.TrySpendImmediateAction(
                            attempt.Protector, out before, out after)) continue;

                    frame.Interceptor = attempt.Protector;
                    bool redirected;
                    try
                    {
                        redirected = TryCommitRedirection(frame,
                            attempt.Protector);
                    }
                    catch
                    {
                        BodyguardActionEconomyAccess.TryRestoreImmediateAction(
                            attempt.Protector, before, before +
                            BodyguardActionEconomyAccess
                                .SwiftActionCooldownSeconds);
                        throw;
                    }
                    if (!redirected)
                    {
                        bool restored = BodyguardActionEconomyAccess
                            .TryRestoreImmediateAction(attempt.Protector, before,
                                before + BodyguardActionEconomyAccess
                                    .SwiftActionCooldownSeconds);
                        frame.Interceptor = null;
                        RecordFault("redirection", null, DescribeFrame(frame,
                            "redirection-rejected") + ";candidate=" +
                            Identity(attempt.Protector) +
                            ";immediateRestored=" + restored);
                        continue;
                    }

                    if (!frame.Policy.TryIntercept(attempt.Protector.UniqueId))
                    {
                        RestoreTargets(frame, "policy-rejected");
                        frame.Interceptor = null;
                        BodyguardActionEconomyAccess.TryRestoreImmediateAction(
                            attempt.Protector, before, before +
                            BodyguardActionEconomyAccess
                                .SwiftActionCooldownSeconds);
                        continue;
                    }

                    frame.ImmediateBefore = before;
                    frame.ImmediateAfter = after;
                    lock (StateGate) RedirectedFrames.Add(frame);
                    BodyguardRuntimeDiagnostics.Intercept(DescribeFrame(frame,
                        "intercepted"));
                    LogInfo("interception.committed", DescribeFrame(frame,
                        "intercepted") + ";arbitration=" +
                        frame.ArbitrationOrder + ";swiftBefore=" + before +
                        ";swiftAfter=" + after + ";contract={" +
                        InHarmsWayDeliveryAccess.ContractDescription + "}");
                    BodyguardCombatLog.PublishInterception(
                        attempt.Protector.CharacterName,
                        frame.OriginalTarget.CharacterName,
                        frame.Attacker.CharacterName);
                    Pop(frame);
                    return;
                }

                CompleteWithoutInterception(frame, "immediate-unavailable");
            }
            catch (Exception exception)
            {
                RestoreTargets(frame, "attack-result-exception");
                frame.Policy.Fault();
                RemoveUncommittedFrame(frame);
                RecordFault("attack-result", exception,
                    DescribeFrame(frame, "faulted"));
            }
        }

        internal static void RuleEventCompleted(RulebookEvent ruleEvent)
        {
            if (ruleEvent == null) return;
            var roll = ruleEvent as RuleAttackRoll;
            if (roll != null)
            {
                RuntimeFrame frame;
                if (FramesByRoll.TryGetValue(roll, out frame) &&
                    frame.Interceptor != null && frame.RollTargetRedirected)
                {
                    bool restored = InHarmsWayDeliveryAccess.TryRestoreRuleTarget(
                        roll, frame.Interceptor, frame.OriginalTarget);
                    frame.RollTargetRedirected = !restored;
                    if (!restored) RecordFault("roll-target-restore", null,
                        DescribeFrame(frame, "restore-failed"));
                }
                else if (FramesByRoll.TryGetValue(roll, out frame) &&
                    !frame.ResultProcessed)
                    FaultAndRestore(frame, "attack-roll-incomplete", null);
                return;
            }

            var weapon = ruleEvent as RuleAttackWithWeapon;
            if (weapon != null)
            {
                RuntimeFrame frame;
                if (!FramesByWeapon.TryGetValue(weapon, out frame) ||
                    frame.Interceptor == null) return;
                if (weapon.Projectile == null)
                {
                    RestoreTargets(frame, "melee-delivery-complete");
                    CompleteIntercepted(frame, "melee-delivery-complete");
                }
                else
                {
                    frame.PendingProjectileResolves =
                        ExpectedProjectileResolveCount(weapon);
                    if (frame.PendingProjectileResolves <= 0)
                        frame.PendingProjectileResolves = 1;
                }
                return;
            }

            var resolve = ruleEvent as RuleAttackWithWeaponResolve;
            if (resolve == null || resolve.AttackWithWeapon == null) return;
            RuntimeFrame resolvedFrame;
            if (!FramesByWeapon.TryGetValue(resolve.AttackWithWeapon,
                    out resolvedFrame) || resolvedFrame.Interceptor == null ||
                resolvedFrame.PendingProjectileResolves <= 0) return;
            resolvedFrame.PendingProjectileResolves--;
            if (resolvedFrame.PendingProjectileResolves == 0)
            {
                RestoreTargets(resolvedFrame, "projectile-delivery-complete");
                CompleteIntercepted(resolvedFrame,
                    "projectile-delivery-complete");
            }
        }

        internal static void AbilityDeliveryTargetAssigned(
            AbilityDeliveryTarget delivery, RuleAttackRoll attackRoll)
        {
            if (delivery == null || attackRoll == null) return;
            RuntimeFrame frame;
            if (!FramesByRoll.TryGetValue(attackRoll, out frame) ||
                frame.Interceptor == null) return;
            RuntimeDelivery duplicate;
            if (DeliveriesByTarget.TryGetValue(delivery, out duplicate))
            {
                BodyguardRuntimeDiagnostics.Duplicate(
                    "stage=ability-target;attack=" + frame.Identity);
                return;
            }
            TargetWrapper original;
            TargetWrapper redirectedTarget;
            bool redirected = InHarmsWayDeliveryAccess.TryRedirectAbilityTarget(
                delivery, attackRoll, frame.Interceptor, out original,
                out redirectedTarget);
            if (!redirected)
            {
                bool restored = BodyguardActionEconomyAccess
                    .TryRestoreImmediateAction(frame.Interceptor,
                        frame.ImmediateBefore, frame.ImmediateAfter);
                FaultAndRestore(frame, "ability-delivery-target", null);
                RecordFault("ability-delivery-target", null,
                    DescribeFrame(frame, "ability-target-failed") +
                    ";immediateRestored=" + restored);
                return;
            }
            var runtimeDelivery = new RuntimeDelivery(frame, delivery, original,
                redirectedTarget);
            DeliveriesByTarget.Add(delivery, runtimeDelivery);
            frame.Deliveries.Add(runtimeDelivery);
            frame.AbilityDeliveryObserved = true;
            BodyguardRuntimeDiagnostics.Observation(DescribeFrame(frame,
                "ability-target") + ";deliveryTarget=" +
                Identity(frame.Interceptor));
            LogInfo("ability-target.redirected", DescribeFrame(frame,
                "ability-target") + ";deliveryTarget=" +
                Identity(frame.Interceptor));
        }

        internal static void AbilityAttackContextDisposed(
            ContextAttackData context)
        {
            if (context == null || context.AttackRoll == null) return;
            RuntimeFrame frame;
            if (!FramesByRoll.TryGetValue(context.AttackRoll, out frame) ||
                frame.Deliveries.Count == 0) return;
            foreach (RuntimeDelivery delivery in frame.Deliveries.ToArray())
                AbilityEffectCompleted(delivery.Delivery,
                    "context-attack-data-dispose");
        }

        internal static void AbilityEffectCompleted(
            AbilityDeliveryTarget delivery, string completionSeam)
        {
            if (delivery == null) return;
            RuntimeDelivery runtimeDelivery;
            if (!DeliveriesByTarget.TryGetValue(delivery, out runtimeDelivery))
                return;
            RuntimeFrame frame = runtimeDelivery.Frame;
            bool restored = InHarmsWayDeliveryAccess.TryRestoreAbilityTarget(
                delivery, runtimeDelivery.Redirected,
                runtimeDelivery.Original);
            DeliveriesByTarget.Remove(delivery);
            frame.Deliveries.Remove(runtimeDelivery);
            if (!restored)
                RecordFault("ability-apply-effect", null,
                    DescribeFrame(frame, "ability-effect-finalizer") +
                    ";targetRestored=" + restored + ";completionSeam=" +
                    completionSeam);
            if (frame.WeaponAttack == null)
            {
                if (!restored)
                    FaultAndRestore(frame, "ability-target-restore", null);
                else
                    CompleteIntercepted(frame,
                        "ability-delivery-complete-" + completionSeam);
            }
            else if (frame.Completed && frame.Deliveries.Count == 0)
            {
                lock (StateGate) RedirectedFrames.Remove(frame);
            }
        }

        internal static UnitEntityData ResolveDeliveryTarget(
            RuleAttackRoll attackRoll)
        {
            RuntimeFrame frame;
            return attackRoll != null && FramesByRoll.TryGetValue(attackRoll,
                    out frame) && frame.Interceptor != null ?
                frame.Interceptor : attackRoll == null ? null : attackRoll.Target;
        }

        internal static void ClearAll(string reason)
        {
            RuntimeFrame[] redirected;
            lock (StateGate)
            {
                redirected = RedirectedFrames.ToArray();
                RedirectedFrames.Clear();
                FramesByRoll = new ConditionalWeakTable<RuleAttackRoll,
                    RuntimeFrame>();
                FramesByWeapon = new ConditionalWeakTable<RuleAttackWithWeapon,
                    RuntimeFrame>();
                DeliveriesByTarget = new ConditionalWeakTable<
                    AbilityDeliveryTarget, RuntimeDelivery>();
            }
            foreach (RuntimeFrame frame in redirected)
                RestoreTargets(frame, "cleanup-" + reason);
            if (_activeFrames != null) _activeFrames.Clear();
            LogInfo("frames.cleanup", "reason=" + reason +
                ";restored=" + redirected.Length);
        }

        private static void CommitAttempt(RuntimeFrame frame,
            RuntimeCandidate candidate)
        {
            int before = 0;
            int after = 0;
            BodyguardAttemptExecution execution =
                BodyguardAttemptCoordinator.Execute(
                    candidate.Protector.UniqueId,
                    candidate.SelectedAttack.AttackBonus,
                    delegate
                    {
                        return BodyguardActionEconomyAccess
                            .TrySpendAttackOfOpportunity(candidate.Protector,
                                frame.Attacker, out before, out after);
                    },
                    delegate
                    {
                        BodyguardSyntheticAidContext.EnterCalculation();
                        try
                        {
                            var roll = new RuleRollD20(candidate.Protector);
                            BodyguardQualificationControl
                                .TryApplyAidOverride(roll);
                            Rulebook.Trigger(roll);
                            return roll.Result;
                        }
                        finally
                        { BodyguardSyntheticAidContext.ExitCalculation(); }
                    });
            if (!execution.Spent)
            {
                if (execution.Fault != null)
                    RecordFault("aoo-spend", execution.Fault,
                        DescribeFrame(frame, "aoo-spend-fault") +
                        ";protector=" + Identity(candidate.Protector));
                return;
            }
            if (execution.Fault != null || execution.Result == null)
            {
                RecordFault("aid-roll", execution.Fault, DescribeFrame(frame,
                    "aid-roll-fault") + ";protector=" +
                    Identity(candidate.Protector) + ";aooBefore=" + before +
                    ";aooAfter=" + after + ";aooRetained=true");
                return;
            }
            BodyguardAidResult result = execution.Result;
            if (!frame.Policy.TryRecordAttempt(result))
            {
                BodyguardRuntimeDiagnostics.Duplicate(
                    "stage=attempt;attack=" + frame.Identity +
                    ";protector=" + candidate.Protector.UniqueId);
                return;
            }
            var attempt = new RuntimeAttempt(candidate, result, before, after);
            frame.Attempts.Add(attempt);
            string detail = DescribeFrame(frame, "attempt") +
                ";protector=" + Identity(candidate.Protector) +
                ";allyEdgeFeet=" + candidate.EdgeDistanceFeet.ToString(
                    "0.###", CultureInfo.InvariantCulture) +
                ";selectedAttack=" + candidate.SelectedAttack.Identity +
                ";aooBefore=" + before + ";aooSpent=true;aooAfter=" + after +
                ";aidD20=" + result.NaturalRoll + ";aidBonus=" +
                candidate.SelectedAttack.AttackBonus + ";aidTotal=" +
                result.Total + ";aidTarget=10;aidSuccess=" + result.Success +
                ";acContribution=" + result.ArmorClassBonus;
            BodyguardRuntimeDiagnostics.Attempt(result.Success, detail);
            LogInfo("attempt.committed", detail);
            BodyguardCombatLog.PublishAttempt(
                candidate.Protector.CharacterName,
                frame.OriginalTarget.CharacterName,
                frame.Attacker.CharacterName, result.NaturalRoll,
                candidate.SelectedAttack.AttackBonus, result.Success);
        }

        private static List<RuntimeCandidate> GatherCandidates(
            UnitEntityData attacker, UnitEntityData target)
        {
            var candidates = new List<RuntimeCandidate>();
            BodyguardFeatBlueprintSet blueprints = BlueprintBootstrap.BodyguardFeats;
            if (blueprints == null || Game.Instance == null ||
                Game.Instance.State == null || Game.Instance.State.Units == null)
                return candidates;

            foreach (UnitEntityData protector in Game.Instance.State.Units.All)
            {
                if (protector == null || protector.Descriptor == null ||
                    protector.Descriptor.State == null ||
                    string.IsNullOrWhiteSpace(protector.UniqueId)) continue;
                bool hasFeat = protector.Descriptor.HasFact(blueprints.Bodyguard);
                bool mode = protector.Descriptor.Buffs.GetBuff(
                    blueprints.Modes.BodyguardMarker) != null;
                if (!hasFeat && !mode) continue;

                int remaining;
                bool nativeAoo = BodyguardActionEconomyAccess
                    .CanSpendAttackOfOpportunity(protector, attacker,
                        out remaining);
                string nativeAooState = BodyguardActionEconomyAccess
                    .DescribeAttackOfOpportunityState(protector, attacker);
                double edgeFeet = EdgeDistanceFeet(protector, target);
                BodyguardSelectedAttack selected = null;
                if (!ReferenceEquals(protector, attacker) &&
                    !ReferenceEquals(protector, target) &&
                    protector.IsAlly(target) && target.IsEnemy(attacker) &&
                    hasFeat && mode && protector.Descriptor.State.IsConscious &&
                    !protector.Descriptor.State.IsDead &&
                    protector.Descriptor.State.CanAct && nativeAoo &&
                    remaining > 0 && edgeFeet <= AdjacencyFeet +
                        DistanceToleranceFeet)
                    selected = BodyguardThreatAccess.SelectBestThreateningAttack(
                        protector, attacker);

                var request = new BodyguardEligibilityRequest
                {
                    ModuleEnabled = true,
                    HostileAttackRoll = true,
                    TargetIsAlly = protector.IsAlly(target),
                    AttackerIsHostile = target.IsEnemy(attacker),
                    ProtectorIsAttacker = ReferenceEquals(protector, attacker),
                    ProtectorIsTarget = ReferenceEquals(protector, target),
                    HasBodyguard = hasFeat,
                    BodyguardModeActive = mode,
                    Alive = !protector.Descriptor.State.IsDead,
                    Conscious = protector.Descriptor.State.IsConscious,
                    AbleToAct = protector.Descriptor.State.CanAct,
                    NativeAooAllowed = nativeAoo,
                    AooRemaining = remaining,
                    ProtectorTargetEdgeDistanceFeet = edgeFeet,
                    AdjacencyFeet = AdjacencyFeet,
                    DistanceToleranceFeet = DistanceToleranceFeet,
                    ThreatensAttacker = selected != null
                };
                BodyguardEligibilityDecision decision =
                    BodyguardEligibilityPolicy.Evaluate(request);
                if (!decision.Eligible)
                {
                    BodyguardRuntimeDiagnostics.Observation(
                        "stage=candidate-skipped;protector=" +
                        Identity(protector) + ";ally=" + Identity(target) +
                        ";attacker=" + Identity(attacker) + ";reason=" +
                        decision.Failure + ";edgeFeet=" + edgeFeet.ToString(
                            "0.###", CultureInfo.InvariantCulture) +
                        ";aooRemaining=" + remaining + ";" + nativeAooState);
                    LogInfo("candidate.skipped", "protector=" +
                        Identity(protector) + ";ally=" + Identity(target) +
                        ";attacker=" + Identity(attacker) + ";reason=" +
                        decision.Failure + ";edgeFeet=" + edgeFeet.ToString(
                            "0.###", CultureInfo.InvariantCulture) +
                        ";aooRemaining=" + remaining + ";" + nativeAooState);
                    continue;
                }
                candidates.Add(new RuntimeCandidate(protector, selected,
                    PartyOrder(protector), edgeFeet));
            }

            return candidates.GroupBy(value => value.Protector.UniqueId,
                    StringComparer.Ordinal).Select(value => value.First())
                .OrderBy(value => value.PartyOrder)
                .ThenBy(value => value.Protector.UniqueId,
                    StringComparer.Ordinal).ToList();
        }

        private static BodyguardInterceptorCandidate CreateInterceptorCandidate(
            RuntimeAttempt attempt, BodyguardFeatBlueprintSet blueprints)
        {
            float cooldown;
            bool immediate = BodyguardActionEconomyAccess
                .CanSpendImmediateAction(attempt.Protector, out cooldown);
            bool hasFeat = blueprints != null && attempt.Protector.Descriptor
                .HasFact(blueprints.InHarmsWay);
            bool mode = blueprints != null && attempt.Protector.Descriptor.Buffs
                .GetBuff(blueprints.Modes.InHarmsWayMarker) != null;
            return new BodyguardInterceptorCandidate(
                attempt.Protector.UniqueId, attempt.PartyOrder, true,
                attempt.Result.Success, hasFeat, mode, immediate);
        }

        private static bool TryCommitRedirection(RuntimeFrame frame,
            UnitEntityData interceptor)
        {
            bool rollRedirected = InHarmsWayDeliveryAccess.TryRedirectRuleTarget(
                frame.Roll, frame.OriginalTarget, interceptor);
            frame.RollTargetRedirected = rollRedirected ||
                InHarmsWayDeliveryAccess.IsRuleTarget(frame.Roll, interceptor);
            if (!rollRedirected)
            {
                bool rollRestored = !frame.RollTargetRedirected &&
                        InHarmsWayDeliveryAccess.IsRuleTarget(frame.Roll,
                            frame.OriginalTarget) ||
                    frame.RollTargetRedirected &&
                        InHarmsWayDeliveryAccess.TryRestoreRuleTarget(frame.Roll,
                            interceptor, frame.OriginalTarget);
                frame.RollTargetRedirected = !rollRestored;
                if (!rollRestored)
                    throw new InvalidOperationException(
                        "Failed roll-target redirection did not restore its original recipient.");
                return false;
            }
            if (frame.WeaponAttack == null) return true;
            bool weaponRedirected = InHarmsWayDeliveryAccess.TryRedirectRuleTarget(
                frame.WeaponAttack, frame.OriginalTarget, interceptor);
            frame.WeaponTargetRedirected = weaponRedirected ||
                InHarmsWayDeliveryAccess.IsRuleTarget(frame.WeaponAttack,
                    interceptor);
            if (weaponRedirected)
            {
                return true;
            }
            bool weaponRestored = !frame.WeaponTargetRedirected &&
                    InHarmsWayDeliveryAccess.IsRuleTarget(frame.WeaponAttack,
                        frame.OriginalTarget) ||
                frame.WeaponTargetRedirected &&
                    InHarmsWayDeliveryAccess.TryRestoreRuleTarget(
                        frame.WeaponAttack, interceptor, frame.OriginalTarget);
            bool rollRestoredAfterWeaponFailure =
                InHarmsWayDeliveryAccess.TryRestoreRuleTarget(frame.Roll,
                    interceptor, frame.OriginalTarget);
            frame.WeaponTargetRedirected = !weaponRestored;
            frame.RollTargetRedirected = !rollRestoredAfterWeaponFailure;
            if (!weaponRestored || !rollRestoredAfterWeaponFailure)
                throw new InvalidOperationException(
                    "Partial weapon-target redirection did not restore every original recipient.");
            return false;
        }

        private static void CompleteWithoutInterception(RuntimeFrame frame,
            string stage)
        {
            frame.Policy.Complete();
            frame.Completed = true;
            FramesByRoll.Remove(frame.Roll);
            if (frame.WeaponAttack != null)
                FramesByWeapon.Remove(frame.WeaponAttack);
            Pop(frame);
            BodyguardRuntimeDiagnostics.Complete(DescribeFrame(frame, stage));
            LogInfo("frame.complete", DescribeFrame(frame, stage));
        }

        private static void CompleteIntercepted(RuntimeFrame frame, string stage)
        {
            if (!frame.Completed)
            {
                frame.Policy.Complete();
                frame.Completed = true;
                BodyguardRuntimeDiagnostics.Complete(DescribeFrame(frame, stage));
                LogInfo("frame.complete", DescribeFrame(frame, stage));
            }
            if (frame.WeaponAttack != null)
                FramesByWeapon.Remove(frame.WeaponAttack);
            if (frame.Deliveries.Count == 0)
                lock (StateGate) RedirectedFrames.Remove(frame);
        }

        private static void FaultAndRestore(RuntimeFrame frame, string stage,
            Exception exception)
        {
            RestoreTargets(frame, stage + "-fault");
            frame.Policy.Fault();
            frame.Completed = true;
            FramesByRoll.Remove(frame.Roll);
            if (frame.WeaponAttack != null)
                FramesByWeapon.Remove(frame.WeaponAttack);
            Pop(frame);
            lock (StateGate) RedirectedFrames.Remove(frame);
            RecordFault(stage, exception, DescribeFrame(frame, "faulted"));
        }

        private static void RestoreTargets(RuntimeFrame frame, string stage)
        {
            if (frame == null || frame.Interceptor == null) return;
            foreach (RuntimeDelivery delivery in frame.Deliveries.ToArray())
            {
                bool ability = InHarmsWayDeliveryAccess.TryRestoreAbilityTarget(
                    delivery.Delivery, delivery.Redirected,
                    delivery.Original);
                DeliveriesByTarget.Remove(delivery.Delivery);
                frame.Deliveries.Remove(delivery);
                if (!ability)
                    RecordFault("ability-target-restore", null,
                        DescribeFrame(frame, stage));
            }
            bool roll = !frame.RollTargetRedirected ||
                InHarmsWayDeliveryAccess.TryRestoreRuleTarget(frame.Roll,
                    frame.Interceptor, frame.OriginalTarget);
            bool weapon = frame.WeaponAttack == null ||
                !frame.WeaponTargetRedirected ||
                InHarmsWayDeliveryAccess.TryRestoreRuleTarget(frame.WeaponAttack,
                    frame.Interceptor, frame.OriginalTarget);
            frame.RollTargetRedirected = !roll;
            frame.WeaponTargetRedirected = !weapon;
            if (!roll || !weapon)
                RecordFault("target-restore", null, DescribeFrame(frame, stage) +
                    ";rollRestored=" + roll + ";weaponRestored=" + weapon);
        }

        private static void RemoveUncommittedFrame(RuntimeFrame frame)
        {
            if (frame == null) return;
            FramesByRoll.Remove(frame.Roll);
            if (frame.WeaponAttack != null)
                FramesByWeapon.Remove(frame.WeaponAttack);
            Pop(frame);
        }

        private static bool IsSupportedHostileAttack(RuleAttackRoll attack,
            UnitEntityData attacker, UnitEntityData target)
        {
            if (attacker == null || target == null ||
                string.IsNullOrWhiteSpace(attacker.UniqueId) ||
                string.IsNullOrWhiteSpace(target.UniqueId) ||
                ReferenceEquals(attacker, target) || !target.IsEnemy(attacker))
                return false;
            if (attack.RuleAttackWithWeapon != null)
                return ReferenceEquals(attack.RuleAttackWithWeapon.Initiator,
                        attacker) && ReferenceEquals(
                        attack.RuleAttackWithWeapon.Target, target) &&
                    !attack.RuleAttackWithWeapon.ReplaceTarget &&
                    attack.RuleAttackWithWeapon.NewTarget == null;
            return attack.Reason != null &&
                attack.Reason.Context is AbilityExecutionContext;
        }

        private static bool ModuleEnabled()
        {
            ModContext context;
            return ModContext.TryGet(out context) && context.FeatureModules != null &&
                context.FeatureModules.Active.BodyguardFeats &&
                BlueprintBootstrap.BodyguardFeats != null;
        }

        private static int PartyOrder(UnitEntityData unit)
        {
            if (Game.Instance == null || Game.Instance.Player == null ||
                Game.Instance.Player.Party == null) return int.MaxValue;
            for (int index = 0; index < Game.Instance.Player.Party.Count; index++)
                if (ReferenceEquals(Game.Instance.Player.Party[index], unit))
                    return index;
            return int.MaxValue;
        }

        private static double EdgeDistanceFeet(UnitEntityData left,
            UnitEntityData right)
        {
            if (left == null || right == null) return double.PositiveInfinity;
            double meters = left.DistanceTo(right) - left.Corpulence -
                right.Corpulence;
            return Math.Max(0d, meters) * MetersToFeet;
        }

        private static int ExpectedProjectileResolveCount(
            RuleAttackWithWeapon attack)
        {
            try
            {
                if (attack == null || attack.Weapon == null ||
                    attack.Weapon.Blueprint == null ||
                    attack.Weapon.Blueprint.VisualParameters == null ||
                    attack.Weapon.Blueprint.VisualParameters.Projectiles == null)
                    return 1;
                int count = attack.Weapon.Blueprint.VisualParameters.Projectiles
                    .Count(value => value != null);
                if (count <= 0) count = 1;
                bool manyshot = attack.Initiator != null &&
                    attack.Initiator.Descriptor != null &&
                    attack.Initiator.Descriptor.State != null &&
                    attack.Initiator.Descriptor.State.Features != null &&
                    attack.Initiator.Descriptor.State.Features.Manyshot &&
                    attack.IsFirstAttack && attack.IsFullAttack &&
                    (int)attack.Weapon.Blueprint.Type.FighterGroup == 4;
                return manyshot ? checked(count * 2) : count;
            }
            catch { return 1; }
        }

        private static void Push(RuntimeFrame frame)
        {
            if (_activeFrames == null) _activeFrames = new Stack<RuntimeFrame>();
            _activeFrames.Push(frame);
        }

        private static RuntimeFrame Peek()
        {
            return _activeFrames == null || _activeFrames.Count == 0 ? null :
                _activeFrames.Peek();
        }

        private static void Pop(RuntimeFrame frame)
        {
            if (_activeFrames == null || _activeFrames.Count == 0) return;
            if (ReferenceEquals(_activeFrames.Peek(), frame))
            {
                _activeFrames.Pop();
                return;
            }
            BodyguardRuntimeDiagnostics.Duplicate("stage=frame-pop-mismatch;attack=" +
                (frame == null ? "<null>" : frame.Identity));
            _activeFrames.Clear();
        }

        private static string DescribeFrame(RuntimeFrame frame, string stage)
        {
            return "stage=" + stage + ";attack=" + frame.Identity +
                ";family=" + (frame.WeaponAttack == null ? "ability-attack-roll" :
                    frame.WeaponAttack.Weapon == null ? "weapon-unknown" :
                    frame.WeaponAttack.Weapon.Blueprint.IsRanged ?
                        "ranged-weapon" : "melee-weapon") +
                ";attacker=" + Identity(frame.Attacker) +
                ";originalTarget=" + Identity(frame.OriginalTarget) +
                ";attempts=" + frame.Attempts.Count +
                ";successes=" + frame.SuccessfulAttempts.Count() +
                ";candidateOrder=" + (frame.CandidateOrder ?? "") +
                ";acContribution=" + frame.Policy.ArmorClassBonus +
                ";acBefore=" + frame.ArmorClassBefore +
                ";acUsed=" + frame.ArmorClassUsed +
                ";attackD20=" + frame.AttackRoll +
                ";attackBonus=" + frame.AttackBonus +
                ";attackTargetAc=" + frame.AttackTargetArmorClass +
                ";attackHit=" + (frame.AttackHit.HasValue ?
                    frame.AttackHit.Value.ToString() : "unknown") +
                ";arbitration=" + (frame.ArbitrationOrder ?? "") +
                ";swiftBefore=" + frame.ImmediateBefore.ToString("R",
                    CultureInfo.InvariantCulture) +
                ";swiftAfter=" + frame.ImmediateAfter.ToString("R",
                    CultureInfo.InvariantCulture) +
                ";interceptor=" + Identity(frame.Interceptor) +
                ";finalTarget=" + Identity(frame.Interceptor ??
                    frame.OriginalTarget) + ";abilityDelivery=" +
                frame.AbilityDeliveryObserved + ";pendingProjectiles=" +
                frame.PendingProjectileResolves + ";completed=" +
                frame.Completed;
        }

        private static string Identity(UnitEntityData unit)
        {
            return unit == null ? "<null>" : (unit.UniqueId ?? "<no-id>") +
                "/" + (string.IsNullOrWhiteSpace(unit.CharacterName) ?
                    "<unnamed>" : unit.CharacterName);
        }

        private static void LogInfo(string eventName, string message)
        {
            ModContext context;
            if (ModContext.TryGet(out context))
                context.Logger.Info("bodyguard", eventName, message);
        }

        private static string _lastWarning;
        private static void LogWarningOnce(string eventName, string message)
        {
            if (string.Equals(_lastWarning, eventName, StringComparison.Ordinal))
                return;
            _lastWarning = eventName;
            ModContext context;
            if (ModContext.TryGet(out context))
                context.Logger.Warning("bodyguard", eventName, message);
        }

        private static void RecordFault(string stage, Exception exception,
            string detail)
        {
            string observation = "stage=" + stage + ";detail=" + detail +
                ";exception=" + (exception == null ? "<none>" :
                    exception.GetType().FullName + ":" + exception.Message);
            BodyguardRuntimeDiagnostics.Fault(observation);
            ModContext context;
            if (!ModContext.TryGet(out context)) return;
            if (exception == null)
                context.Logger.Warning("bodyguard", stage + ".failed",
                    observation);
            else
                context.Logger.Failure("bodyguard", stage + ".failed",
                    detail, exception);
        }

        private sealed class RuntimeCandidate
        {
            internal RuntimeCandidate(UnitEntityData protector,
                BodyguardSelectedAttack selectedAttack, int partyOrder,
                double edgeDistanceFeet)
            {
                Protector = protector;
                SelectedAttack = selectedAttack;
                PartyOrder = partyOrder;
                EdgeDistanceFeet = edgeDistanceFeet;
            }
            internal UnitEntityData Protector { get; private set; }
            internal BodyguardSelectedAttack SelectedAttack { get; private set; }
            internal int PartyOrder { get; private set; }
            internal double EdgeDistanceFeet { get; private set; }
        }

        private sealed class RuntimeAttempt
        {
            internal RuntimeAttempt(RuntimeCandidate candidate,
                BodyguardAidResult result, int aooBefore, int aooAfter)
            {
                Protector = candidate.Protector;
                SelectedAttack = candidate.SelectedAttack;
                PartyOrder = candidate.PartyOrder;
                EdgeDistanceFeet = candidate.EdgeDistanceFeet;
                Result = result;
                AooBefore = aooBefore;
                AooAfter = aooAfter;
            }
            internal UnitEntityData Protector { get; private set; }
            internal BodyguardSelectedAttack SelectedAttack { get; private set; }
            internal int PartyOrder { get; private set; }
            internal double EdgeDistanceFeet { get; private set; }
            internal BodyguardAidResult Result { get; private set; }
            internal int AooBefore { get; private set; }
            internal int AooAfter { get; private set; }
        }

        private sealed class RuntimeFrame
        {
            internal RuntimeFrame(string identity, RuleAttackRoll roll,
                UnitEntityData attacker, UnitEntityData originalTarget)
            {
                Identity = identity;
                Roll = roll;
                WeaponAttack = roll.RuleAttackWithWeapon;
                Attacker = attacker;
                OriginalTarget = originalTarget;
                Policy = new BodyguardAttackFrame(identity, attacker.UniqueId,
                    originalTarget.UniqueId);
                Attempts = new List<RuntimeAttempt>();
                ImmediateBefore = float.NaN;
                ImmediateAfter = float.NaN;
                Deliveries = new List<RuntimeDelivery>();
            }
            internal string Identity { get; private set; }
            internal RuleAttackRoll Roll { get; private set; }
            internal RuleAttackWithWeapon WeaponAttack { get; private set; }
            internal UnitEntityData Attacker { get; private set; }
            internal UnitEntityData OriginalTarget { get; private set; }
            internal BodyguardAttackFrame Policy { get; private set; }
            internal List<RuntimeAttempt> Attempts { get; private set; }
            internal IEnumerable<RuntimeAttempt> SuccessfulAttempts
            { get { return Attempts.Where(value => value.Result.Success); } }
            internal bool BodyguardFinished { get; set; }
            internal bool ArmorClassObserved { get; set; }
            internal bool ArmorClassFault { get; set; }
            internal int ArmorClassBefore { get; set; }
            internal int ArmorClassUsed { get; set; }
            internal bool ResultProcessed { get; set; }
            internal bool? AttackHit { get; set; }
            internal int AttackRoll { get; set; }
            internal int AttackBonus { get; set; }
            internal int AttackTargetArmorClass { get; set; }
            internal UnitEntityData Interceptor { get; set; }
            internal bool RollTargetRedirected { get; set; }
            internal bool WeaponTargetRedirected { get; set; }
            internal float ImmediateBefore { get; set; }
            internal float ImmediateAfter { get; set; }
            internal string ArbitrationOrder { get; set; }
            internal string CandidateOrder { get; set; }
            internal int PendingProjectileResolves { get; set; }
            internal bool AbilityDeliveryObserved { get; set; }
            internal List<RuntimeDelivery> Deliveries { get; private set; }
            internal bool Completed { get; set; }
        }

        private sealed class RuntimeDelivery
        {
            internal RuntimeDelivery(RuntimeFrame frame,
                AbilityDeliveryTarget delivery, TargetWrapper original,
                TargetWrapper redirected)
            {
                Frame = frame;
                Delivery = delivery;
                Original = original;
                Redirected = redirected;
            }
            internal RuntimeFrame Frame { get; private set; }
            internal AbilityDeliveryTarget Delivery { get; private set; }
            internal TargetWrapper Original { get; private set; }
            internal TargetWrapper Redirected { get; private set; }
        }

        private sealed class ReferenceComparer<T> : IEqualityComparer<T>
            where T : class
        {
            internal static readonly ReferenceComparer<T> Instance =
                new ReferenceComparer<T>();
            public bool Equals(T left, T right)
            { return ReferenceEquals(left, right); }
            public int GetHashCode(T value)
            { return RuntimeHelpers.GetHashCode(value); }
        }
    }
}
