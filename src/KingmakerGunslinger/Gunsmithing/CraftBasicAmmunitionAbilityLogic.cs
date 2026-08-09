using System;
using System.Collections.Generic;
using Kingmaker;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using UnityEngine;

namespace KingmakerGunslinger.Gunsmithing
{
    [Serializable]
    public sealed class CraftBasicAmmunitionAbilityLogic : AbilityCustomLogic,
        IAbilityAvailabilityProvider
    {
        internal const int BatchSize = 20;
        [SerializeField] private BlueprintItem m_BlackPowder;
        [SerializeField] private BlueprintItem m_LeadBall;
        [SerializeField] private BlueprintItem m_GunsmithKit;
        [SerializeField] private BlueprintUnitFact m_UsedMarker;

        internal static CraftBasicAmmunitionAbilityLogic Create(BlueprintItem powder,
            BlueprintItem ball, BlueprintItem tool, BlueprintUnitFact marker)
        {
            var value = ScriptableObject.CreateInstance<CraftBasicAmmunitionAbilityLogic>();
            value.m_BlackPowder = powder; value.m_LeadBall = ball;
            value.m_GunsmithKit = tool; value.m_UsedMarker = marker;
            value.Validate(); return value;
        }

        internal int GoldCost
        {
            get { return Math.Max(1, (int)Math.Ceiling(
                ((m_BlackPowder.Cost + m_LeadBall.Cost) * BatchSize) * 0.10d)); }
        }

        public bool IsAvailableFor(AbilityData ability)
        {
            return ability != null && ability.Caster != null &&
                !ability.Caster.Unit.IsInCombat &&
                ability.Caster.State.IsConscious && ability.Caster.State.CanAct &&
                !ability.Caster.HasFact(m_UsedMarker) && Game.Instance != null &&
                Game.Instance.Player != null && Game.Instance.Player.Inventory != null &&
                Game.Instance.Player.Inventory.Count(m_GunsmithKit) > 0 &&
                Game.Instance.Player.Money >= GoldCost;
        }

        public string GetReason()
        {
            return "Requires a Gunsmith's Kit, sufficient gold, no combat, and an unused once-per-rest crafting entitlement.";
        }

        public override IEnumerator<AbilityDeliveryTarget> Deliver(
            AbilityExecutionContext context, TargetWrapper target)
        {
            if (context == null || context.Caster == null ||
                !IsAvailableFor(context.Ability))
            { yield return new AbilityDeliveryTarget(target); yield break; }
            Complete(context.Caster.Descriptor);
            yield return new AbilityDeliveryTarget(target);
        }

        internal void Complete(UnitDescriptor caster)
        {
            FirearmCraftingTransactionService.Complete(caster, m_GunsmithKit,
                m_UsedMarker, GoldCost, new[] { m_BlackPowder, m_LeadBall },
                new[] { BatchSize, BatchSize });
        }

        public override void Cleanup(AbilityExecutionContext context) { }
        internal BlueprintUnitFact UsedMarker { get { return m_UsedMarker; } }
        internal BlueprintItem BlackPowder { get { return m_BlackPowder; } }
        internal BlueprintItem LeadBall { get { return m_LeadBall; } }
        internal BlueprintItem GunsmithKit { get { return m_GunsmithKit; } }
        internal void Validate()
        {
            if (m_BlackPowder == null || m_LeadBall == null ||
                m_GunsmithKit == null || m_UsedMarker == null)
                throw new InvalidOperationException("Crafting configuration is incomplete.");
        }
    }
}
