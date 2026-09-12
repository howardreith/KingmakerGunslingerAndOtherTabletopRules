using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using KingmakerGunslinger.Bootstrap;
using UnityEngine;

namespace KingmakerGunslinger.Recovery
{
    /// <summary>
    /// Player-facing availability and delivery component for exact-item field
    /// repair: Broken to Normal, outside combat, with one reusable
    /// shared-inventory Gunsmith's Kit. The exact firearm is bound at genuine
    /// command commencement and must still match at delivery, so cancellation,
    /// combat, or a target/context change across the preceding full-round
    /// command consumes nothing and changes no state. Nothing is ever consumed
    /// on success either. Wrecked firearms are restored only by a completed
    /// full rest. Mutation occurs only at the ability-delivery boundary.
    /// </summary>
    [Serializable]
    public sealed class RepairTestMusketAbilityLogic :
        AbilityCustomLogic,
        IAbilityAvailabilityProvider
    {
        [SerializeField]
        private BlueprintItemWeapon m_TestMusket;

        [SerializeField]
        private BlueprintItem m_GunsmithKit;

        internal static RepairTestMusketAbilityLogic Create(
            BlueprintItemWeapon testMusket,
            BlueprintItem gunsmithKit)
        {
            if (testMusket == null)
            {
                throw new ArgumentNullException("testMusket");
            }

            if (gunsmithKit == null)
            {
                throw new ArgumentNullException("gunsmithKit");
            }

            RepairTestMusketAbilityLogic component =
                ScriptableObject.CreateInstance<RepairTestMusketAbilityLogic>();
            component.m_TestMusket = testMusket;
            component.m_GunsmithKit = gunsmithKit;
            component.ValidateConfiguration();
            return component;
        }

        internal BlueprintItem GunsmithKit
        {
            get { return m_GunsmithKit; }
        }

        public bool IsAvailableFor(AbilityData ability)
        {
            try
            {
                ValidateConfiguration();
                return ability != null &&
                    RepairTestMusketRuntime.Evaluate(
                        ability.Caster,
                        m_TestMusket,
                        m_GunsmithKit)
                    .IsAvailable;
            }
            catch
            {
                return false;
            }
        }

        public string GetReason()
        {
            // The native interface has no caster argument. Never cache a
            // caster-specific failure on this shared blueprint component.
            return "Cannot repair right now.";
        }

        internal string GetReasonFor(AbilityData ability)
        {
            try
            {
                ValidateConfiguration();
                FirearmRepairAvailability availability = RepairTestMusketRuntime.Evaluate(
                    ability == null ? null : ability.Caster, m_TestMusket, m_GunsmithKit);
                return availability.IsAvailable ? GetReason() : availability.Reason;
            }
            catch { return GetReason(); }
        }

        public override IEnumerator<AbilityDeliveryTarget> Deliver(
            AbilityExecutionContext context,
            TargetWrapper target)
        {
            return DeliverPromptly(context, target);
        }

        private IEnumerator<AbilityDeliveryTarget> DeliverPromptly(
            AbilityExecutionContext context, TargetWrapper target)
        {
            FirearmRepairAvailability start;
            if (!TryPrepare(context, out start))
            {
                yield return new AbilityDeliveryTarget(target);
                yield break;
            }
            Complete(context, start);
            yield return new AbilityDeliveryTarget(target);
        }

        private bool TryPrepare(AbilityExecutionContext context,
            out FirearmRepairAvailability start)
        {
            start = null;
            try
            {
                ValidateConfiguration();
                if (context == null ||
                    context.Caster == null ||
                    context.Caster.Descriptor == null)
                {
                    RecordRejection("Cannot repair right now.");
                    return false;
                }

                start = RepairTestMusketRuntime.Evaluate(
                    context.Caster.Descriptor,
                    m_TestMusket,
                    m_GunsmithKit);
                if (!start.IsAvailable || start.Weapon == null)
                {
                    RecordRejection(start.Reason);
                    return false;
                }
                Kingmaker.Items.ItemEntityWeapon boundAtCommandStart;
                if (!RepairCommandStartBinding.TryGetBoundWeaponForDelivery(
                        context, out boundAtCommandStart) ||
                    !ReferenceEquals(boundAtCommandStart, start.Weapon))
                {
                    RecordRejection("Cannot repair right now.");
                    return false;
                }
                return true;
            }
            catch (Exception exception)
            {
                RecordFailure(exception);
                return false;
            }
        }

        private void Complete(AbilityExecutionContext context,
            FirearmRepairAvailability start)
        {
            try
            {
                FirearmRepairAvailability completed = RepairTestMusketRuntime.Evaluate(
                    context.Caster.Descriptor, m_TestMusket, m_GunsmithKit);
                if (!completed.IsAvailable ||
                    !ReferenceEquals(completed.Weapon, start.Weapon))
                {
                    RecordRejection(completed.IsAvailable
                        ? "Cannot repair right now." : completed.Reason);
                    return;
                }
                FirearmRepairRuntimeResult result = RepairTestMusketRuntime.Execute(
                    context.Caster.Descriptor, m_TestMusket, m_GunsmithKit);
                RepairRuntimeDiagnostics.Record(result);

                ModContext modContext;
                if (ModContext.TryGet(out modContext))
                {
                    modContext.Logger.Info(
                        "recovery",
                        result.Succeeded
                            ? "repair.completed"
                            : "repair.rejected",
                        result.ToString());
                }
            }
            catch (Exception exception)
            {
                RecordFailure(exception);
            }
        }

        private static void RecordRejection(string reason)
        {
            ModContext modContext;
            if (ModContext.TryGet(out modContext))
                modContext.Logger.Info("recovery", "repair.rejected", reason);
        }

        private static void RecordFailure(Exception exception)
        {
            RepairRuntimeDiagnostics.RecordFault(exception);
            ModContext modContext;
            if (ModContext.TryGet(out modContext))
                modContext.Logger.Failure("recovery", "repair.failed",
                    "Repair Firearm failed or was interrupted; the exact item state is restored on failure and the reusable Gunsmith's Kit is never consumed.",
                    exception);
        }

        public override void Cleanup(AbilityExecutionContext context)
        {
            // No approach effects or retained runtime objects are created by this ability.
        }

        internal void ValidateConfiguration()
        {
            if (m_TestMusket == null || m_GunsmithKit == null)
            {
                throw new InvalidOperationException(
                    "Repair Firearm has incomplete blueprint dependencies.");
            }
        }
    }
}
