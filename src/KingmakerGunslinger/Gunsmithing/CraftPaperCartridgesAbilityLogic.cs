using System;
using System.Collections.Generic;
using Kingmaker;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using UnityEngine;

namespace KingmakerGunslinger.Gunsmithing
{
    [Serializable]
    public sealed class CraftPaperCartridgesAbilityLogic : AbilityCustomLogic,
        IAbilityAvailabilityProvider
    {
        internal const int BatchSize = 20;
        internal const int GoldCost = 120;
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

        public bool IsAvailableFor(AbilityData ability)
        {
            return ability != null && ability.Caster != null &&
                !ability.Caster.Unit.IsInCombat && ability.Caster.State.IsConscious &&
                ability.Caster.State.CanAct && !ability.Caster.HasFact(m_UsedMarker) &&
                Game.Instance != null && Game.Instance.Player != null &&
                Game.Instance.Player.Inventory != null &&
                Game.Instance.Player.Inventory.Count(m_GunsmithKit) > 0 &&
                Game.Instance.Player.Money >= GoldCost;
        }

        public string GetReason()
        {
            return "Requires a Gunsmith's Kit, 120 gp, no combat, and an unused shared once-per-rest crafting entitlement.";
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
    }
}
