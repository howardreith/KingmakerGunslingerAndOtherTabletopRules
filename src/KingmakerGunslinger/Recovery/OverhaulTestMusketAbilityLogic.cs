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
    /// Player-facing availability and delivery component for exact-item Wrecked-to-Broken
    /// recovery. Mutation occurs only at the ordinary ability-delivery boundary after the
    /// full-round command completes, so cancellation before delivery consumes no repair kit
    /// and changes no state. Delivery itself performs no background or game-time wait.
    /// </summary>
    [Serializable]
    public sealed class OverhaulTestMusketAbilityLogic :
        AbilityCustomLogic,
        IAbilityAvailabilityProvider
    {
        [SerializeField]
        private BlueprintItemWeapon m_TestMusket;

        [SerializeField]
        private BlueprintItem m_RepairKit;

        internal static OverhaulTestMusketAbilityLogic Create(
            BlueprintItemWeapon testMusket,
            BlueprintItem repairKit)
        {
            if (testMusket == null)
            {
                throw new ArgumentNullException("testMusket");
            }

            if (repairKit == null)
            {
                throw new ArgumentNullException("repairKit");
            }

            OverhaulTestMusketAbilityLogic component =
                ScriptableObject.CreateInstance<OverhaulTestMusketAbilityLogic>();
            component.m_TestMusket = testMusket;
            component.m_RepairKit = repairKit;
            component.ValidateConfiguration();
            return component;
        }

        public bool IsAvailableFor(AbilityData ability)
        {
            try
            {
                ValidateConfiguration();
                return ability != null &&
                    OverhaulTestMusketRuntime.Evaluate(
                        ability.Caster,
                        m_TestMusket,
                        m_RepairKit)
                    .IsAvailable;
            }
            catch
            {
                return false;
            }
        }

        public string GetReason()
        {
            return "Requires exactly one equipped empty Wrecked firearm, one Firearm Repair Kit, and no active combat.";
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
            FirearmOverhaulAvailability start;
            if (!TryPrepare(context, out start))
            {
                yield return new AbilityDeliveryTarget(target);
                yield break;
            }
            Complete(context, start);
            yield return new AbilityDeliveryTarget(target);
        }

        private bool TryPrepare(AbilityExecutionContext context,
            out FirearmOverhaulAvailability start)
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
                        "The overhaul delivery has no concrete caster descriptor.");
                }

                start = OverhaulTestMusketRuntime.Evaluate(
                    context.Caster.Descriptor,
                    m_TestMusket,
                    m_RepairKit);
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
            FirearmOverhaulAvailability start)
        {
            try
            {
                FirearmOverhaulAvailability completed = OverhaulTestMusketRuntime.Evaluate(
                    context.Caster.Descriptor, m_TestMusket, m_RepairKit);
                if (!completed.IsAvailable ||
                    !ReferenceEquals(completed.Weapon, start.Weapon))
                    throw new InvalidOperationException(
                        "Overhaul Firearm was interrupted or its exact item context changed.");
                FirearmOverhaulRuntimeResult result = OverhaulTestMusketRuntime.Execute(
                    context.Caster.Descriptor, m_TestMusket, m_RepairKit);
                OverhaulRuntimeDiagnostics.Record(result);

                ModContext modContext;
                if (ModContext.TryGet(out modContext))
                {
                    modContext.Logger.Info(
                        "recovery",
                        result.Succeeded
                            ? "overhaul.completed"
                            : "overhaul.rejected",
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
            OverhaulRuntimeDiagnostics.RecordFault(exception);
            ModContext modContext;
            if (ModContext.TryGet(out modContext))
                modContext.Logger.Failure("recovery", "overhaul.failed",
                    "Overhaul Firearm failed or was interrupted; no kit or exact item state should change before completion.",
                    exception);
        }

        public override void Cleanup(AbilityExecutionContext context)
        {
            // No approach effects or retained runtime objects are created by this ability.
        }

        internal void ValidateConfiguration()
        {
            if (m_TestMusket == null || m_RepairKit == null)
            {
                throw new InvalidOperationException(
                    "Overhaul Firearm has incomplete blueprint dependencies.");
            }
        }
    }
}
