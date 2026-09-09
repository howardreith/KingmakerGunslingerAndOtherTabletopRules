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
    /// Player-facing availability and delivery component for exact-item unified repair:
    /// Broken or Wrecked to Normal with one reusable shared-inventory Gunsmith's Kit.
    /// Mutation occurs only at the ability-delivery boundary after the full-round
    /// command completes, so cancellation or interruption before delivery consumes
    /// nothing and changes no state. Nothing is ever consumed on success either.
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
            return "Requires exactly one equipped Broken or Wrecked firearm and a reusable Gunsmith's Kit in the shared inventory. Loaded ammunition is preserved and nothing is consumed.";
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
                    throw new InvalidOperationException(
                        "The repair delivery has no concrete caster descriptor.");
                }

                start = RepairTestMusketRuntime.Evaluate(
                    context.Caster.Descriptor,
                    m_TestMusket,
                    m_GunsmithKit);
                if (!start.IsAvailable || start.Weapon == null)
                    throw new InvalidOperationException(start.Reason);
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
                    throw new InvalidOperationException(
                        "Repair Firearm was interrupted or its exact item context changed.");
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
