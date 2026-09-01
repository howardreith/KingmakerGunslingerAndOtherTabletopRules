using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;
using UnityEngine;

namespace KingmakerGunslinger.Reloading
{
    /// <summary>
    /// Reload delivery consumes the exact immutable plan selected at command
    /// creation. A changed paper-cartridge toggle invalidates the queued command
    /// instead of allowing mismatched action economy and ammunition.
    /// </summary>
    [Serializable]
    public sealed class ReloadTestMusketAbilityLogic : AbilityCustomLogic,
        IAbilityAvailabilityProvider
    {
        [SerializeField] private BlueprintItemWeapon m_TestMusket;
        [SerializeField] private BlueprintItem m_BlackPowder;
        [SerializeField] private BlueprintItem m_LeadBall;
        [SerializeField] private EffectiveReloadAction m_Action;
        [SerializeField] private bool m_DynamicAction;

        internal static ReloadTestMusketAbilityLogic Create(
            BlueprintItemWeapon testMusket, BlueprintItem blackPowder,
            BlueprintItem leadBall, EffectiveReloadAction action)
        {
            if (testMusket == null) throw new ArgumentNullException("testMusket");
            if (blackPowder == null) throw new ArgumentNullException("blackPowder");
            if (leadBall == null) throw new ArgumentNullException("leadBall");
            ReloadTestMusketAbilityLogic component = ScriptableObject
                .CreateInstance<ReloadTestMusketAbilityLogic>();
            component.m_TestMusket = testMusket;
            component.m_BlackPowder = blackPowder;
            component.m_LeadBall = leadBall;
            component.m_Action = action;
            component.ValidateConfiguration();
            return component;
        }

        internal static ReloadTestMusketAbilityLogic CreateDynamic(
            BlueprintItemWeapon testMusket, BlueprintItem blackPowder,
            BlueprintItem leadBall)
        {
            ReloadTestMusketAbilityLogic component = Create(testMusket,
                blackPowder, leadBall, EffectiveReloadAction.Standard);
            component.m_DynamicAction = true;
            return component;
        }

        public bool IsAvailableFor(AbilityData ability)
        {
            try
            {
                ValidateConfiguration();
                if (ability == null)
                {
                    ReloadPlayerFacingReasonPolicy.Remember("Cannot reload now.");
                    return false;
                }
                ReloadTestMusketAvailability result =
                    ReloadTestMusketRuntime.Evaluate(ability.Caster,
                        m_TestMusket, m_BlackPowder, m_LeadBall);
                bool available = result.IsAvailable && (m_DynamicAction ||
                    result.Plan.Action == m_Action);
                ReloadPlayerFacingReasonPolicy.Remember(available ?
                    "Ready to reload." : result.Reason);
                return available;
            }
            catch
            {
                ReloadPlayerFacingReasonPolicy.Remember("Cannot reload now.");
                return false;
            }
        }

        public string GetReason()
        { return ReloadPlayerFacingReasonPolicy.CurrentOrFallback(); }

        public override IEnumerator<AbilityDeliveryTarget> Deliver(
            AbilityExecutionContext context, TargetWrapper target)
        {
            try
            {
                ValidateConfiguration();
                if (context == null || context.Caster == null ||
                    context.Caster.Descriptor == null)
                {
                    RecordUnavailable("Cannot reload now.",
                        "The reload delivery has no concrete caster descriptor.");
                }
                else
                {
                    ReloadTestMusketAvailability availability =
                        ReloadTestMusketRuntime.Evaluate(
                            context.Caster.Descriptor, m_TestMusket,
                            m_BlackPowder, m_LeadBall);
                    if (!availability.IsAvailable)
                    {
                        RecordUnavailable(availability.Reason,
                            availability.TechnicalReason);
                    }
                    else if (!ReloadQueuedPlanBinding.IsCurrent(context.Ability,
                            availability.Plan))
                    {
                        RecordUnavailable(
                            ReloadPlayerFacingReasonPolicy.ForQueuedPlanChange(),
                            "The queued reload plan no longer matches the current paper-cartridge mode.");
                    }
                    else
                    {
                        FirearmReloadResult result = ReloadTestMusketRuntime
                            .Execute(context.Caster.Descriptor, m_BlackPowder,
                                m_LeadBall, availability);
                        ReloadRuntimeDiagnostics.Record(result);
                        ModContext modContext;
                        if (ModContext.TryGet(out modContext))
                            modContext.Logger.Info("reload", result.Succeeded ?
                                "ability.loaded" : "ability.rejected",
                                result.ToString());
                    }
                }
            }
            catch (Exception exception)
            {
                ReloadRuntimeDiagnostics.RecordFault(exception);
                ModContext modContext;
                if (ModContext.TryGet(out modContext)) modContext.Logger.Failure(
                    "reload", "ability.failed",
                    "Reload Firearm failed during delivery; the transaction rolled back its owned state.",
                    exception);
            }
            finally
            {
                ReloadQueuedPlanBinding.Forget(context == null ? null :
                    context.Ability);
            }
            yield return new AbilityDeliveryTarget(target);
        }

        public override void Cleanup(AbilityExecutionContext context) { }

        internal void ValidateConfiguration()
        {
            if (m_TestMusket == null || m_BlackPowder == null ||
                m_LeadBall == null || !Enum.IsDefined(
                    typeof(EffectiveReloadAction), m_Action) ||
                m_Action == EffectiveReloadAction.Unknown)
                throw new InvalidOperationException(
                    "Reload Firearm has incomplete blueprint dependencies.");
            if (ReferenceEquals(m_BlackPowder, m_LeadBall))
                throw new InvalidOperationException(
                    "Reload Firearm requires distinct powder and projectile blueprints.");
        }

        private static void RecordUnavailable(string playerReason,
            string technicalReason)
        {
            ReloadPlayerFacingReasonPolicy.Remember(playerReason);
            ReloadRuntimeDiagnostics.RecordRejected(technicalReason);
            ModContext context;
            if (ModContext.TryGet(out context)) context.Logger.Info("reload",
                "ability.unavailable", "playerReason=" + playerReason +
                ";technicalReason=" + technicalReason);
        }
    }
}
