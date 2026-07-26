using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.Utility;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Firing;

namespace KingmakerGunslinger.Explosions
{
    /// <summary>
    /// Exact Kingmaker 2.1.7b adapter for the bounded Sprint 26 second-misfire
    /// burst. It validates the original attack, exact runtime item, current
    /// wielder, repository identity, committed empty/Wrecked state, and configured
    /// firearm radius; then it uses Kingmaker's native spatial query and resolves
    /// one fresh Reflex DC 12 save and one fresh base weapon-damage event for every
    /// unique qualified unit. The exact wielder is inserted explicitly and resolved
    /// last so lethal self-damage cannot prevent already-qualified nearby targets
    /// from receiving the nominally simultaneous consequence.
    /// </summary>
    internal static class FirearmExplosionRuntime
    {
        private static readonly ReferenceEventGate EventGate =
            new ReferenceEventGate();
        private static readonly FirearmExplosionTargetPlanService TargetPlanService =
            new FirearmExplosionTargetPlanService();

        internal static void Apply(
            RuleAttackRoll attackRoll,
            object firearmItem,
            UnitEntityData wielder,
            string repositoryIdentity,
            int burstRadiusFeet,
            string firearm)
        {
            if (attackRoll == null)
            {
                FirearmExplosionRuntimeDiagnostics.RecordRejected(
                    firearm,
                    "The correlated RuleAttackRoll was unavailable");
                return;
            }

            if (!EventGate.TryMark(attackRoll))
            {
                FirearmExplosionRuntimeDiagnostics.RecordDuplicate(firearm);
                return;
            }

            try
            {
                ValidateBurstRadius(burstRadiusFeet);

                string rejection;
                ItemEntityWeapon weapon = ValidateAndResolveWeapon(
                    attackRoll,
                    firearmItem,
                    wielder,
                    repositoryIdentity,
                    out rejection);
                if (weapon == null)
                {
                    FirearmExplosionRuntimeDiagnostics.RecordRejected(
                        firearm,
                        rejection);
                    LogWarning(
                        "explosion.rejected",
                        rejection + "; firearm=" + Normalize(firearm));
                    return;
                }

                string damageFormula = weapon.Damage.ToString();
                string wielderDisplay = ResolveDisplayName(wielder);
                string attackRollIdentity = FormatReferenceIdentity(attackRoll);
                FirearmExplosionRuntimeDiagnostics.RecordAttempt(
                    firearm,
                    wielderDisplay,
                    attackRollIdentity,
                    repositoryIdentity,
                    damageFormula,
                    burstRadiusFeet);

                FirearmExplosionTargetPlan plan = BuildTargetPlan(
                    wielder,
                    burstRadiusFeet);
                FirearmExplosionRuntimeDiagnostics.RecordQuery(
                    FormatPosition(wielder),
                    burstRadiusFeet,
                    plan);

                var appliedUnits = new HashSet<object>(
                    ReferenceIdentityComparer.Instance);
                var results = new List<FirearmExplosionTargetResult>(
                    plan.TargetCount);
                bool targetResolutionFaulted = false;

                foreach (FirearmExplosionTargetCandidate target in plan.Targets)
                {
                    if (!appliedUnits.Add(target.Unit))
                    {
                        targetResolutionFaulted = true;
                        FirearmExplosionRuntimeDiagnostics.RecordTargetDuplicate(
                            target);
                        continue;
                    }

                    FirearmExplosionRuntimeDiagnostics.RecordTargetAttempt(target);
                    try
                    {
                        VerifyCommittedState(
                            weapon,
                            repositoryIdentity);

                        UnitEntityData targetUnit = target.Unit as UnitEntityData;
                        if (targetUnit == null)
                        {
                            throw new InvalidOperationException(
                                "The deterministic explosion target was no longer a UnitEntityData instance.");
                        }

                        DamageBundle damage = CreateBaseWeaponDamageBundle(
                            weapon,
                            out rejection);
                        if (damage == null)
                        {
                            FirearmExplosionRuntimeDiagnostics.RecordTargetRejected(
                                target.DisplayName,
                                target.StableIdentity,
                                rejection);
                            targetResolutionFaulted = true;
                            continue;
                        }

                        FirearmExplosionTargetResult result = ResolveTarget(
                            attackRoll,
                            wielder,
                            targetUnit,
                            target,
                            damage);
                        results.Add(result);
                        FirearmExplosionRuntimeDiagnostics.RecordTargetApplied(
                            result);
                    }
                    catch (Exception exception)
                    {
                        targetResolutionFaulted = true;
                        FirearmExplosionRuntimeDiagnostics.RecordTargetFault(
                            exception,
                            target.DisplayName,
                            target.StableIdentity);
                        LogFault(
                            "explosion.target-failed",
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "Native burst resolution failed for one qualified target; no retry was attempted; target={0}; unitId={1}; exactWielder={2}.",
                                target.DisplayName,
                                target.StableIdentity,
                                target.IsExactWielder),
                            exception);
                    }
                }

                VerifyCommittedState(
                    weapon,
                    repositoryIdentity);

                if (targetResolutionFaulted || results.Count != plan.TargetCount)
                {
                    var exception = new InvalidOperationException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "The second-misfire burst did not apply exactly once to every planned target; planned={0}; applied={1}.",
                            plan.TargetCount,
                            results.Count));
                    FirearmExplosionRuntimeDiagnostics.RecordFault(
                        exception,
                        "native-burst-target-resolution",
                        firearm);
                    LogFault(
                        "explosion.partial-failure",
                        "The exact firearm remains empty/Wrecked, but at least one qualified target did not complete native save/damage resolution.",
                        exception);
                    return;
                }

                FirearmExplosionRuntimeDiagnostics.RecordApplied(
                    firearm,
                    wielderDisplay,
                    attackRollIdentity,
                    repositoryIdentity,
                    damageFormula,
                    burstRadiusFeet,
                    plan,
                    results);
                LogInfo(
                    "explosion.applied",
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Second misfire resolved one native Reflex DC {0} save and one fresh native base weapon-damage event for each unique qualified unit; firearm={1}; originWielder={2}; attackRoll={3}; repositoryIdentity={4}; weaponDamage={5}; burstRadiusFeet={6}; observedCandidates={7}; duplicateCandidates={8}; appliedTargets={9}; exactWielderLast=True; item remains empty/Wrecked.",
                        FirearmExplosionService.ReflexSaveDifficultyClass,
                        Normalize(firearm),
                        wielderDisplay,
                        attackRollIdentity,
                        Normalize(repositoryIdentity),
                        damageFormula,
                        burstRadiusFeet,
                        plan.ObservedCandidates,
                        plan.DuplicateCandidates,
                        results.Count));
            }
            catch (Exception exception)
            {
                FirearmExplosionRuntimeDiagnostics.RecordFault(
                    exception,
                    "native-spatial-reflex-and-damage",
                    firearm);
                LogFault(
                    "explosion.failed",
                    "The exact firearm had already become empty/Wrecked, but native spatial burst resolution failed. No broad fallback or retry was attempted.",
                    exception);
            }
        }

        private static FirearmExplosionTargetPlan BuildTargetPlan(
            UnitEntityData wielder,
            int burstRadiusFeet)
        {
            var exactWielder = new FirearmExplosionTargetCandidate(
                wielder,
                ResolveStableIdentity(wielder),
                ResolveDisplayName(wielder),
                0f,
                true);

            var nearby = new List<FirearmExplosionTargetCandidate>();
            IEnumerable<UnitEntityData> queried = GameHelper.GetTargetsAround(
                wielder.Position,
                new Feet((float)burstRadiusFeet),
                true,
                false);
            if (queried == null)
            {
                throw new InvalidOperationException(
                    "Kingmaker's native GetTargetsAround query returned null.");
            }

            foreach (UnitEntityData unit in queried)
            {
                if (unit == null)
                {
                    throw new InvalidOperationException(
                        "Kingmaker's native GetTargetsAround query yielded a null unit.");
                }

                nearby.Add(new FirearmExplosionTargetCandidate(
                    unit,
                    ResolveStableIdentity(unit),
                    ResolveDisplayName(unit),
                    unit.DistanceTo(wielder.Position),
                    false));
            }

            return TargetPlanService.Build(
                exactWielder,
                nearby);
        }

        private static FirearmExplosionTargetResult ResolveTarget(
            RuleAttackRoll attackRoll,
            UnitEntityData source,
            UnitEntityData targetUnit,
            FirearmExplosionTargetCandidate target,
            DamageBundle damage)
        {
            int hitPointsBefore = targetUnit.HPLeft;
            var savingThrow = new RuleSavingThrow(
                targetUnit,
                SavingThrowType.Reflex,
                FirearmExplosionService.ReflexSaveDifficultyClass);
            Rulebook.Trigger(savingThrow);

            bool halfBecauseSavingThrow = savingThrow.IsPassed;
            var dealDamage = new RuleDealDamage(
                source,
                targetUnit,
                damage)
            {
                AttackRoll = attackRoll,
                DisablePrecisionDamage = true,
                HalfBecauseSavingThrow = halfBecauseSavingThrow
            };
            Rulebook.Trigger(dealDamage);
            int hitPointsAfter = targetUnit.HPLeft;

            return new FirearmExplosionTargetResult(
                target.DisplayName,
                target.StableIdentity,
                target.DistanceMeters,
                target.IsExactWielder,
                savingThrow.D20.Value,
                savingThrow.RollResult,
                savingThrow.IsPassed,
                halfBecauseSavingThrow,
                dealDamage.DamageBeforeDifficulty,
                dealDamage.DamageWithoutReduction,
                dealDamage.Damage,
                hitPointsBefore,
                hitPointsAfter);
        }

        private static DamageBundle CreateBaseWeaponDamageBundle(
            ItemEntityWeapon weapon,
            out string rejection)
        {
            if (weapon.Blueprint == null)
            {
                rejection = "The exact firing weapon exposed no blueprint";
                return null;
            }

            if (weapon.Blueprint.DamageType == null)
            {
                rejection = "The exact firing weapon blueprint exposed no damage type";
                return null;
            }

            BaseDamage baseDamage = weapon.Blueprint.DamageType.CreateDamage(
                weapon.Damage,
                0);
            if (baseDamage == null)
            {
                rejection = "The exact firing weapon damage type did not produce base weapon damage";
                return null;
            }

            var damage = new DamageBundle(
                weapon,
                weapon.Size,
                baseDamage);
            if (damage.First == null)
            {
                rejection = "The exact firing weapon did not produce a native base weapon-damage bundle";
                return null;
            }

            rejection = null;
            return damage;
        }

        private static ItemEntityWeapon ValidateAndResolveWeapon(
            RuleAttackRoll attackRoll,
            object firearmItem,
            UnitEntityData wielder,
            string repositoryIdentity,
            out string rejection)
        {
            if (firearmItem == null)
            {
                rejection = "The exact firing item reference was unavailable";
                return null;
            }

            ItemEntityWeapon weapon = firearmItem as ItemEntityWeapon;
            if (weapon == null)
            {
                rejection = "The exact firing item was not an ItemEntityWeapon";
                return null;
            }

            if (wielder == null)
            {
                rejection = "The exact wielder reference was unavailable";
                return null;
            }

            if (!ReferenceEquals(attackRoll.Initiator, wielder))
            {
                rejection = "The correlated attack roll no longer referenced the recorded exact wielder";
                return null;
            }

            if (!ReferenceEquals(attackRoll.Weapon, weapon))
            {
                rejection = "The correlated attack roll no longer referenced the exact firing item";
                return null;
            }

            RuleAttackWithWeapon weaponAttack = attackRoll.RuleAttackWithWeapon;
            if (weaponAttack == null)
            {
                rejection = "The correlated attack roll exposed no RuleAttackWithWeapon source";
                return null;
            }

            if (!ReferenceEquals(weaponAttack.Initiator, wielder) ||
                !ReferenceEquals(weaponAttack.Weapon, weapon))
            {
                rejection = "The source RuleAttackWithWeapon did not retain the exact wielder and firing item";
                return null;
            }

            if (weapon.Wielder == null ||
                weapon.Wielder.Unit == null ||
                !ReferenceEquals(weapon.Wielder.Unit, wielder))
            {
                rejection = "The exact firing item's current wielder did not match the correlated attack initiator";
                return null;
            }

            if (string.IsNullOrWhiteSpace(repositoryIdentity))
            {
                rejection = "The recorded exact-item repository identity was unavailable";
                return null;
            }

            FirearmItemStateSnapshot current;
            string stateRejection;
            if (!FirearmRuntimeState.Service.TryGetExisting(
                    weapon,
                    out current,
                    out stateRejection))
            {
                rejection = "The exact firing item's committed state could not be re-read: " +
                    Normalize(stateRejection);
                return null;
            }

            if (!string.Equals(
                    current.Repository.RepositoryIdentity,
                    repositoryIdentity,
                    StringComparison.Ordinal))
            {
                rejection = "The exact firing item's repository identity changed before explosion damage";
                return null;
            }

            if (!current.Repository.State.IsEmpty ||
                current.Repository.State.Condition != FirearmCondition.Wrecked)
            {
                rejection = string.Format(
                    CultureInfo.InvariantCulture,
                    "The exact firing item was not empty/Wrecked before explosion damage; state=[{0}]",
                    current.Repository.State);
                return null;
            }

            rejection = null;
            return weapon;
        }

        private static void VerifyCommittedState(
            ItemEntityWeapon weapon,
            string repositoryIdentity)
        {
            FirearmItemStateSnapshot after;
            string rejection;
            if (!FirearmRuntimeState.Service.TryGetExisting(
                    weapon,
                    out after,
                    out rejection))
            {
                throw new InvalidOperationException(
                    "The exact firearm state could not be re-read after native explosion damage: " +
                    Normalize(rejection));
            }

            if (!string.Equals(
                    after.Repository.RepositoryIdentity,
                    repositoryIdentity,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "The post-damage firearm state was read through a different repository identity.");
            }

            if (!after.Repository.State.IsEmpty ||
                after.Repository.State.Condition != FirearmCondition.Wrecked)
            {
                throw new InvalidOperationException(
                    "Native explosion damage changed the exact firearm away from empty/Wrecked.");
            }
        }

        private static void ValidateBurstRadius(int burstRadiusFeet)
        {
            if (burstRadiusFeet < FirearmDefinition.MinimumMisfireBurstRadiusFeet ||
                burstRadiusFeet > FirearmDefinition.MaximumMisfireBurstRadiusFeet ||
                burstRadiusFeet % 5 != 0)
            {
                throw new ArgumentOutOfRangeException(
                    "burstRadiusFeet",
                    burstRadiusFeet,
                    "The exact firearm's misfire burst radius was outside the validated five-foot-step definition range.");
            }
        }

        private static string ResolveStableIdentity(UnitEntityData unit)
        {
            if (unit == null)
            {
                throw new ArgumentNullException("unit");
            }

            if (string.IsNullOrWhiteSpace(unit.UniqueId))
            {
                throw new InvalidOperationException(
                    "A qualified explosion target exposed no stable Kingmaker unit identity.");
            }

            return unit.UniqueId.Trim();
        }

        private static string ResolveDisplayName(UnitEntityData unit)
        {
            if (unit == null)
            {
                throw new ArgumentNullException("unit");
            }

            string name = unit.CharacterName;
            if (!string.IsNullOrWhiteSpace(name))
            {
                return name.Trim();
            }

            name = unit.ToString();
            return string.IsNullOrWhiteSpace(name)
                ? "<unnamed-unit>"
                : name.Trim();
        }

        private static string FormatPosition(UnitEntityData unit)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "({0:0.###},{1:0.###},{2:0.###})",
                unit.Position.x,
                unit.Position.y,
                unit.Position.z);
        }

        private static string FormatReferenceIdentity(object value)
        {
            return value == null
                ? "<null>"
                : "0x" + RuntimeHelpers.GetHashCode(value).ToString(
                    "x8",
                    CultureInfo.InvariantCulture);
        }

        private static void LogInfo(string eventName, string message)
        {
            ModContext context;
            if (ModContext.TryGet(out context))
            {
                context.Logger.Info("explosion", eventName, message);
            }
        }

        private static void LogWarning(string eventName, string message)
        {
            ModContext context;
            if (ModContext.TryGet(out context))
            {
                context.Logger.Warning("explosion", eventName, message);
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
                    "explosion",
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
    }
}
