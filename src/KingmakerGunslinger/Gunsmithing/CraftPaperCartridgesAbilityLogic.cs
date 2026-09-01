using System;
using System.Collections.Generic;
using Kingmaker;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using KingmakerGunslinger.Ammunition;
using KingmakerGunslinger.Bootstrap;
using UnityEngine;

namespace KingmakerGunslinger.Gunsmithing
{
    [Serializable]
    public sealed class CraftPaperCartridgesAbilityLogic : AbilityCustomLogic,
        IAbilityAvailabilityProvider
    {
        internal const int BatchSize = 20;
        [SerializeField] private BlueprintItem m_PaperCartridge;
        [SerializeField] private BlueprintItem m_GunsmithKit;
        [SerializeField] private BlueprintUnitFact m_UsedMarker;

        internal static CraftPaperCartridgesAbilityLogic Create(
            BlueprintItem paper, BlueprintItem tool, BlueprintUnitFact marker)
        {
            var value = ScriptableObject.CreateInstance<CraftPaperCartridgesAbilityLogic>();
            value.m_PaperCartridge = paper; value.m_GunsmithKit = tool;
            value.m_UsedMarker = marker; value.Validate(); return value;
        }

        internal int GoldCost
        {
            get { return AmmunitionCraftingCostPolicy.ForBatch(
                m_PaperCartridge.Cost, BatchSize); }
        }

        public bool IsAvailableFor(AbilityData ability)
        {
            string reason;
            bool available = TryGetAvailabilityReason(ability, out reason);
            GunsmithingPlayerFacingReasonPolicy.Remember(available ?
                "Ready to craft." : reason);
            return available;
        }

        public string GetReason()
        { return GunsmithingPlayerFacingReasonPolicy.CurrentOrFallback(); }

        public override IEnumerator<AbilityDeliveryTarget> Deliver(
            AbilityExecutionContext context, TargetWrapper target)
        {
            if (context == null || context.Caster == null ||
                !IsAvailableFor(context.Ability))
            {
                yield return new AbilityDeliveryTarget(target);
                yield break;
            }
            try { Complete(context.Caster.Descriptor); }
            catch (Exception exception)
            {
                GunsmithingPlayerFacingReasonPolicy.Remember(
                    HasEnoughGold() ? "Cannot craft now." : "Not enough gold.");
                ModContext modContext;
                if (ModContext.TryGet(out modContext)) modContext.Logger.Failure(
                    "gunsmithing", "ammunition-craft.failed",
                    "Ammunition crafting failed; the transaction rolled back its owned state.",
                    exception);
            }
            yield return new AbilityDeliveryTarget(target);
        }

        internal void Complete(UnitDescriptor caster)
        {
            FirearmCraftingTransactionService.Complete(caster, m_GunsmithKit,
                m_UsedMarker, GoldCost, new[] { m_PaperCartridge },
                new[] { BatchSize });
        }

        public override void Cleanup(AbilityExecutionContext context) { }

        internal void Validate()
        {
            if (m_PaperCartridge == null || m_GunsmithKit == null ||
                m_UsedMarker == null)
                throw new InvalidOperationException(
                    "Paper Cartridge crafting configuration is incomplete.");
        }

        private bool TryGetAvailabilityReason(AbilityData ability,
            out string reason)
        {
            if (ability == null || ability.Caster == null ||
                ability.Caster.Unit == null)
            {
                reason = "Cannot craft now.";
                return false;
            }
            if (ability.Caster.Unit.IsInCombat)
            {
                reason = "Cannot craft in combat.";
                return false;
            }
            if (!ability.Caster.State.IsConscious || !ability.Caster.State.CanAct)
            {
                reason = "Cannot craft now.";
                return false;
            }
            if (m_UsedMarker == null || m_GunsmithKit == null ||
                m_PaperCartridge == null)
            {
                reason = "Cannot craft now.";
                return false;
            }
            if (ability.Caster.HasFact(m_UsedMarker))
            {
                reason = "Already crafted this rest.";
                return false;
            }
            var player = Game.Instance == null ? null : Game.Instance.Player;
            if (player == null || player.Inventory == null)
            {
                reason = "Cannot craft now.";
                return false;
            }
            if (player.Inventory.Count(m_GunsmithKit) < 1)
            {
                reason = "Need a Gunsmith's Kit.";
                return false;
            }
            if (player.Money < GoldCost)
            {
                reason = "Not enough gold.";
                return false;
            }
            reason = "Ready to craft.";
            return true;
        }

        private bool HasEnoughGold()
        {
            var player = Game.Instance == null ? null : Game.Instance.Player;
            return player != null && player.Money >= GoldCost;
        }
    }
}
