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
        internal const float WorkDurationSeconds = 60f;
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
            TimeSpan finish = Game.Instance.TimeController.GameTime +
                TimeSpan.FromSeconds(WorkDurationSeconds);
            while (Game.Instance != null && Game.Instance.TimeController != null &&
                Game.Instance.TimeController.GameTime < finish) yield return null;
            if (!IsAvailableFor(context.Ability))
            { yield return new AbilityDeliveryTarget(target); yield break; }
            Complete(context.Caster.Descriptor);
            yield return new AbilityDeliveryTarget(target);
        }

        internal void Complete(UnitDescriptor caster)
        {
            var player = Game.Instance.Player;
            int powderBefore = player.Inventory.Count(m_BlackPowder);
            int ballBefore = player.Inventory.Count(m_LeadBall);
            int cost = GoldCost;
            if (!player.SpendMoney(cost))
                throw new InvalidOperationException("Crafting gold removal failed.");
            try
            {
                player.Inventory.Add(m_BlackPowder, BatchSize);
                player.Inventory.Add(m_LeadBall, BatchSize);
                if (player.Inventory.Count(m_BlackPowder) != powderBefore + BatchSize ||
                    player.Inventory.Count(m_LeadBall) != ballBefore + BatchSize)
                    throw new InvalidOperationException("Crafted ammunition inventory verification failed.");
                if (caster.AddFact(m_UsedMarker) == null)
                    throw new InvalidOperationException("Crafting entitlement marker was not persisted.");
            }
            catch
            {
                int powderAdded = player.Inventory.Count(m_BlackPowder) - powderBefore;
                int ballsAdded = player.Inventory.Count(m_LeadBall) - ballBefore;
                if (powderAdded > 0) player.Inventory.Remove(m_BlackPowder, powderAdded);
                if (ballsAdded > 0) player.Inventory.Remove(m_LeadBall, ballsAdded);
                player.GainMoney(cost);
                throw;
            }
        }

        public override void Cleanup(AbilityExecutionContext context) { }
        internal BlueprintUnitFact UsedMarker { get { return m_UsedMarker; } }
        internal void Validate()
        {
            if (m_BlackPowder == null || m_LeadBall == null ||
                m_GunsmithKit == null || m_UsedMarker == null)
                throw new InvalidOperationException("Crafting configuration is incomplete.");
        }
    }
}
