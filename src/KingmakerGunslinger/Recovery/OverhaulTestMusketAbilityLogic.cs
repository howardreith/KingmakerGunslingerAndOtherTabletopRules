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
    /// recovery. Mutation occurs only in Deliver after the full-round command completes,
    /// so cancellation or interruption before delivery consumes no repair kit and changes no state.
    /// </summary>
    [Serializable]
    public sealed class OverhaulTestMusketAbilityLogic :
        AbilityCustomLogic,
        IAbilityAvailabilityProvider
    {
        internal const float WorkDurationSeconds = 60f;

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
            return "Requires exactly one equipped Wrecked firearm, one Firearm Repair Kit, and one uninterrupted minute out of combat.";
        }

        public override IEnumerator<AbilityDeliveryTarget> Deliver(
            AbilityExecutionContext context,
            TargetWrapper target)
        {
            return DeliverTimed(context, target);
        }

        private IEnumerator<AbilityDeliveryTarget> DeliverTimed(
            AbilityExecutionContext context, TargetWrapper target)
        {
            FirearmOverhaulAvailability start;
            TimeSpan completion;
            if (!TryPrepare(context, out start, out completion))
            {
                yield return new AbilityDeliveryTarget(target);
                yield break;
            }
            while (Kingmaker.Game.Instance != null &&
                Kingmaker.Game.Instance.TimeController != null &&
                Kingmaker.Game.Instance.TimeController.GameTime < completion)
                yield return null;
            Complete(context, start);
            yield return new AbilityDeliveryTarget(target);
        }

        private bool TryPrepare(AbilityExecutionContext context,
            out FirearmOverhaulAvailability start, out TimeSpan completion)
        {
            start = null;
            completion = default(TimeSpan);
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
                if (Kingmaker.Game.Instance == null ||
                    Kingmaker.Game.Instance.TimeController == null)
                    throw new InvalidOperationException(
                        "The overhaul work timer is unavailable.");
                completion = Kingmaker.Game.Instance.TimeController.GameTime +
                    TimeSpan.FromSeconds(WorkDurationSeconds);
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
