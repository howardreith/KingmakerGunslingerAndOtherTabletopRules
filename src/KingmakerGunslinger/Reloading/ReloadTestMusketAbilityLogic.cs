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
    /// Delivery and availability component for the first real reload ability. Mutation
    /// occurs only inside Deliver, after Kingmaker has completed the full-round command.
    /// Cancellation before delivery therefore consumes neither ammunition nor firearm state.
    /// </summary>
    [Serializable]
    public sealed class ReloadTestMusketAbilityLogic :
        AbilityCustomLogic,
        IAbilityAvailabilityProvider
    {
        [SerializeField]
        private BlueprintItemWeapon m_TestMusket;

        [SerializeField]
        private BlueprintItem m_BlackPowder;

        [SerializeField]
        private BlueprintItem m_LeadBall;

        [SerializeField]
        private EffectiveReloadAction m_Action;

        [SerializeField]
        private bool m_DynamicAction;

        internal static ReloadTestMusketAbilityLogic Create(
            BlueprintItemWeapon testMusket,
            BlueprintItem blackPowder,
            BlueprintItem leadBall,
            EffectiveReloadAction action)
        {
            if (testMusket == null)
            {
                throw new ArgumentNullException("testMusket");
            }

            if (blackPowder == null)
            {
                throw new ArgumentNullException("blackPowder");
            }

            if (leadBall == null)
            {
                throw new ArgumentNullException("leadBall");
            }

            ReloadTestMusketAbilityLogic component =
                ScriptableObject.CreateInstance<ReloadTestMusketAbilityLogic>();
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
                if (ability == null) return false;
                ReloadTestMusketAvailability result = ReloadTestMusketRuntime.Evaluate(
                        ability.Caster,
                        m_TestMusket,
                        m_BlackPowder,
                        m_LeadBall);
                return result.IsAvailable && (m_DynamicAction || ReloadActionEconomy.Evaluate(
                    result.Firearm.Definition,
                    RapidReloadRuntime.HasMatchingChoice(ability.Caster,
                        result.Firearm.Definition.Kind)) == m_Action);
            }
            catch
            {
                return false;
            }
        }

        public string GetReason()
        {
            return "Requires one unambiguous equipped firearm that is not full or Wrecked, plus compatible Black Powder Charges and Lead Balls.";
        }

        public override IEnumerator<AbilityDeliveryTarget> Deliver(
            AbilityExecutionContext context,
            TargetWrapper target)
        {
            try
            {
                ValidateConfiguration();
                if (context == null || context.Caster == null || context.Caster.Descriptor == null)
                {
                    throw new InvalidOperationException(
                        "The reload delivery has no concrete caster descriptor.");
                }

                FirearmReloadResult result = ReloadTestMusketRuntime.Execute(
                    context.Caster.Descriptor,
                    m_TestMusket,
                    m_BlackPowder,
                    m_LeadBall);
                ReloadRuntimeDiagnostics.Record(result);

                ModContext modContext;
                if (ModContext.TryGet(out modContext))
                {
                    modContext.Logger.Info(
                        "reload",
                        result.Succeeded ? "ability.loaded" : "ability.rejected",
                        result.ToString());
                }
            }
            catch (Exception exception)
            {
                ReloadRuntimeDiagnostics.RecordFault(exception);
                ModContext modContext;
                if (ModContext.TryGet(out modContext))
                {
                    modContext.Logger.Failure(
                        "reload",
                        "ability.failed",
                        "Reload Firearm failed during delivery. The transaction attempted to restore both firearm state and ammunition.",
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
            if (m_TestMusket == null || m_BlackPowder == null || m_LeadBall == null ||
                !Enum.IsDefined(typeof(EffectiveReloadAction), m_Action) ||
                m_Action == EffectiveReloadAction.Unknown)
            {
                throw new InvalidOperationException(
                    "Reload Firearm has incomplete blueprint dependencies.");
            }

            if (ReferenceEquals(m_BlackPowder, m_LeadBall))
            {
                throw new InvalidOperationException(
                    "Reload Firearm requires distinct powder and projectile blueprints.");
            }
        }
    }
}
