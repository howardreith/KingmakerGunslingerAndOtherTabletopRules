using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Diagnostics;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Grit
{
    /// <summary>
    /// Binds the exact firearm critical and killing-blow clauses to native rule
    /// events. Weak reference-identity contexts make each clause apply at most
    /// once per attack/target without retaining completed combat events.
    /// </summary>
    internal static class FirearmGritRecoveryRuntime
    {
        private static readonly object ContextGate = new object();
        private static readonly ConditionalWeakTable<RuleAttackRoll, AttackRecoveryContext>
            AttackContexts =
                new ConditionalWeakTable<RuleAttackRoll, AttackRecoveryContext>();
        private static readonly GritRecoveryService Service = new GritRecoveryService();

        internal static void AfterAttackRoll(RuleAttackRoll attackRoll)
        {
            if (Deeds.DeadShotRuntime.IsProbe(attackRoll)) return;
            if (attackRoll == null)
                return;

            try
            {
                AttackRecoveryContext context = GetAttackContext(attackRoll);
                if (!context.TryMarkCritical())
                {
                    FirearmGritRecoveryRuntimeDiagnostics.RecordDuplicate(
                        GritRecoveryEventKind.ConfirmedCritical);
                    return;
                }

                EvaluateAndRestore(attackRoll, attackRoll.Target,
                    GritRecoveryEventKind.ConfirmedCritical,
                    attackRoll.IsCriticalConfirmed,
                    attackRoll.Target != null && attackRoll.Target.IsInCombat,
                    attackRoll.Target != null && attackRoll.Target.Descriptor != null &&
                        attackRoll.Target.Descriptor.State != null &&
                        attackRoll.Target.Descriptor.State.IsHelpless,
                    CharacterLevel(attackRoll.Target));
            }
            catch (Exception exception)
            {
                FirearmGritRecoveryRuntimeDiagnostics.RecordFault(
                    GritRecoveryEventKind.ConfirmedCritical, exception);
                LogFault("critical-recovery.failed", exception);
            }
        }

        internal static void AfterWeaponResolve(RuleAttackWithWeaponResolve resolve)
        {
            if (resolve == null || resolve.Damage == null ||
                resolve.AttackRoll == null) return;

            try
            {
                RuleDealDamage damage = resolve.Damage;
                RuleAttackRoll attackRoll = resolve.AttackRoll;
                UnitEntityData target = damage.Target;
                if (target == null || !ReferenceEquals(target, attackRoll.Target) ||
                    !ReferenceEquals(damage.AttackRoll, attackRoll) ||
                    !FirearmMarkerLookup.ReadFromRuleEvent(resolve).IsExactFirearm)
                {
                    FirearmGritRecoveryRuntimeDiagnostics.RecordIgnored(
                        GritRecoveryEventKind.KillingBlow,
                        GritRecoveryStatus.NotQualifyingOutcome);
                    return;
                }

                AttackRecoveryContext context = GetAttackContext(attackRoll);
                if (!context.TryMarkKillingBlow(target))
                {
                    FirearmGritRecoveryRuntimeDiagnostics.RecordDuplicate(
                        GritRecoveryEventKind.KillingBlow);
                    return;
                }

                // RuleAttackWithWeaponResolve is raised after its real ranged
                // RuleDealDamage. Reconstruct the exact pre-hit HP boundary
                // from the applied damage instead of relying on MeleeDamage.
                bool killingBlow = damage.Damage > 0 && target.HPLeft <= 0 &&
                    target.HPLeft + damage.Damage > 0;
                EvaluateAndRestore(attackRoll, target,
                    GritRecoveryEventKind.KillingBlow, killingBlow,
                    target.IsInCombat,
                    target.Descriptor != null && target.Descriptor.State != null &&
                        target.Descriptor.State.IsHelpless,
                    CharacterLevel(target));
            }
            catch (Exception exception)
            {
                FirearmGritRecoveryRuntimeDiagnostics.RecordFault(
                    GritRecoveryEventKind.KillingBlow, exception);
                LogFault("killing-blow-recovery.failed", exception);
            }
        }

        private static void EvaluateAndRestore(RuleAttackRoll attackRoll,
            UnitEntityData target, GritRecoveryEventKind kind,
            bool qualifyingOutcome, bool targetWasInCombat,
            bool targetWasHelpless, int targetCharacterLevel)
        {
            UnitEntityData initiator = attackRoll.Initiator;
            GunslingerClassBlueprintSet blueprints = BlueprintBootstrap.GunslingerClass;
            if (blueprints != null && initiator != null &&
                initiator.Descriptor.Buffs.RawFacts.Any(value =>
                    ReferenceEquals(value.Blueprint,
                        blueprints.DeathsShot.ArmedMarker)))
            {
                FirearmGritRecoveryRuntimeDiagnostics.RecordIgnored(kind,
                    GritRecoveryStatus.NotQualifyingOutcome);
                return;
            }
            bool exactFirearm = FirearmMarkerLookup.ReadFromRuleEvent(attackRoll)
                .IsExactFirearm;
            int characterLevel = CharacterLevel(initiator);
            bool hasGunslinger = blueprints != null && initiator != null &&
                initiator.Descriptor != null &&
                initiator.Descriptor.Progression.GetClassLevel(
                    blueprints.CharacterClass) > 0;

            if (!hasGunslinger || characterLevel <= 0)
            {
                FirearmGritRecoveryRuntimeDiagnostics.RecordIgnored(
                    kind, GritRecoveryStatus.NotQualifyingOutcome);
                return;
            }

            GritRecoveryDecision decision = Service.Evaluate(
                new GritRecoveryRequest(kind, qualifyingOutcome, exactFirearm,
                    initiator.IsInCombat, target != null && target.Descriptor != null,
                    targetWasHelpless || !targetWasInCombat,
                    Math.Max(0, targetCharacterLevel), characterLevel));
            if (!decision.ShouldRestore)
            {
                FirearmGritRecoveryRuntimeDiagnostics.RecordIgnored(
                    kind, decision.Status);
                return;
            }

            int before = initiator.Descriptor.Resources.GetResourceAmount(
                blueprints.Grit.Resource);
            initiator.Descriptor.Resources.Restore(blueprints.Grit.Resource, 1);
            int after = initiator.Descriptor.Resources.GetResourceAmount(
                blueprints.Grit.Resource);
            FirearmGritRecoveryRuntimeDiagnostics.RecordApplied(kind, before, after);
            ModContext mod;
            if (ModContext.TryGet(out mod))
                mod.Logger.Info("grit", "recovery.immediate",
                    "callback=" + kind + ";before=" + before + ";after=" + after +
                    ";eventSequence=" +
                    FirearmGritRecoveryRuntimeDiagnostics.LastAppliedSequence);
        }

        private static int CharacterLevel(UnitEntityData unit)
        {
            return unit == null || unit.Descriptor == null ||
                unit.Descriptor.Progression == null
                ? 0
                : unit.Descriptor.Progression.CharacterLevel;
        }

        private static AttackRecoveryContext GetAttackContext(
            RuleAttackRoll attackRoll)
        {
            lock (ContextGate)
            {
                AttackRecoveryContext context;
                if (!AttackContexts.TryGetValue(attackRoll, out context))
                {
                    context = new AttackRecoveryContext();
                    AttackContexts.Add(attackRoll, context);
                }
                return context;
            }
        }

        private static void LogFault(string eventName, Exception exception)
        {
            ModContext context;
            if (ModContext.TryGet(out context))
                context.Logger.Failure("grit", eventName,
                    "Firearm grit recovery failed closed.", exception);
        }

        private sealed class AttackRecoveryContext
        {
            private readonly HashSet<UnitEntityData> _killingBlowTargets =
                new HashSet<UnitEntityData>(ReferenceComparer.Instance);
            private bool _criticalMarked;

            internal bool TryMarkCritical()
            {
                lock (this)
                {
                    if (_criticalMarked) return false;
                    _criticalMarked = true;
                    return true;
                }
            }

            internal bool TryMarkKillingBlow(UnitEntityData target)
            {
                if (target == null) return false;
                lock (this) { return _killingBlowTargets.Add(target); }
            }
        }

        private sealed class ReferenceComparer : IEqualityComparer<UnitEntityData>
        {
            internal static readonly ReferenceComparer Instance =
                new ReferenceComparer();
            public bool Equals(UnitEntityData left, UnitEntityData right)
            {
                return ReferenceEquals(left, right);
            }
            public int GetHashCode(UnitEntityData value)
            {
                return RuntimeHelpers.GetHashCode(value);
            }
        }
    }
}
