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
            return "Requires exactly one equipped Wrecked Test Musket and one Firearm Repair Kit.";
        }

        public override IEnumerator<AbilityDeliveryTarget> Deliver(
            AbilityExecutionContext context,
            TargetWrapper target)
        {
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

                FirearmOverhaulRuntimeResult result = OverhaulTestMusketRuntime.Execute(
                    context.Caster.Descriptor,
                    m_TestMusket,
                    m_RepairKit);
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
                OverhaulRuntimeDiagnostics.RecordFault(exception);
                ModContext modContext;
                if (ModContext.TryGet(out modContext))
                {
                    modContext.Logger.Failure(
                        "recovery",
                        "overhaul.failed",
                        "Overhaul Test Musket failed during delivery. The transaction attempted to restore both the exact item-owned state and the Firearm Repair Kit count.",
                        exception);
                }
            }

            yield return new AbilityDeliveryTarget(target);
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
                    "Overhaul Test Musket has incomplete blueprint dependencies.");
            }
        }
    }
}
