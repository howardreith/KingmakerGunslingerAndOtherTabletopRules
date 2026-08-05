using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints;
using Kingmaker.Utility;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.ElementsSystem;
using UnityEngine;

namespace KingmakerGunslinger.Deeds
{
    [Serializable]
    public sealed class GunslingerDodgeProneAbilityLogic : AbilityCustomLogic,
        IAbilityAvailabilityProvider
    {
        [SerializeField] private BlueprintUnitFact m_ArmedMarker;
        [SerializeField] private BlueprintBuff m_ArmorClassBuff;
        [SerializeField] private ContextActionApplyBuff m_ApplyBuff;
        [SerializeField] private ActionList m_ApplyBuffActions;

        internal static GunslingerDodgeProneAbilityLogic Create(
            BlueprintUnitFact armedMarker, BlueprintBuff armorClassBuff)
        {
            if (armedMarker == null) throw new ArgumentNullException("armedMarker");
            if (armorClassBuff == null) throw new ArgumentNullException("armorClassBuff");
            var result = ScriptableObject.CreateInstance<GunslingerDodgeProneAbilityLogic>();
            result.name = "$KMG_GunslingerDodge_ImmediateDelivery";
            result.m_ArmedMarker = armedMarker;
            result.m_ArmorClassBuff = armorClassBuff;
            result.m_ApplyBuff = new ContextActionApplyBuff
            {
                Buff = armorClassBuff,
                DurationValue = new ContextDurationValue
                {
                    Rate = DurationRate.Rounds,
                    BonusValue = 1
                },
                IsNotDispelable = true
            };
            result.m_ApplyBuffActions = new ActionList
            {
                Actions = new GameAction[] { result.m_ApplyBuff }
            };
            return result;
        }

        public bool IsAvailableFor(AbilityData ability)
        {
            if (ability == null || ability.Caster == null ||
                m_ArmorClassBuff == null ||
                ability.Caster.HasFact(m_ArmorClassBuff))
                return false;
            GunslingerClassBlueprintSet set = BlueprintBootstrap.GunslingerClass;
            return set != null && ability.Caster.Resources.GetResourceAmount(
                set.Grit.Resource) >= TrueGritRuntime.Evaluate(ability.Caster,
                    TrueGritDeed.GunslingersDodge, 1, false).EffectiveCost;
        }

        public string GetReason()
        {
            return "Requires enough Grit and no active Gunslinger's Dodge buff.";
        }

        public override IEnumerator<AbilityDeliveryTarget> Deliver(
            AbilityExecutionContext context, TargetWrapper target)
        {
            if (m_ArmedMarker == null || m_ArmorClassBuff == null ||
                context == null || context.Caster == null ||
                context.Caster.Descriptor == null)
                throw new InvalidOperationException("Dodge delivery lacks its caster or marker.");
            UnitDescriptor caster = context.Caster.Descriptor;
            if (caster.HasFact(m_ArmorClassBuff))
                throw new InvalidOperationException(
                    "Gunslinger's Dodge is already active on the caster.");
            if (caster.HasFact(m_ArmedMarker)) caster.RemoveFact(m_ArmedMarker);
            int acBefore = caster.Stats == null ? 0 : caster.Stats.AC.ModifiedValue;
            using (context.GetDataScope(new TargetWrapper(context.Caster)))
                m_ApplyBuffActions.Run();
            Buff buff = caster.Buffs.GetBuff(m_ArmorClassBuff);
            if (buff == null)
                throw new InvalidOperationException("Gunslinger's Dodge buff was not created.");
            GunslingerDodgeRuntimeDiagnostics.RecordDelivery(
                buff, acBefore, caster.Stats == null ? 0 : caster.Stats.AC.ModifiedValue);
            yield return new AbilityDeliveryTarget(new TargetWrapper(context.Caster));
        }

        public override void Cleanup(AbilityExecutionContext context) { }
        internal BlueprintUnitFact ArmedMarker { get { return m_ArmedMarker; } }
        internal BlueprintBuff ArmorClassBuff { get { return m_ArmorClassBuff; } }
        internal bool HasOneRoundTimedBuffAction
        {
            get
            {
                return m_ApplyBuff != null &&
                    ReferenceEquals(m_ApplyBuff.Buff, m_ArmorClassBuff) &&
                    !m_ApplyBuff.Permanent && m_ApplyBuff.IsNotDispelable &&
                    m_ApplyBuff.DurationValue.Rate == DurationRate.Rounds &&
                    m_ApplyBuff.DurationValue.BonusValue.Value == 1;
            }
        }
    }

    internal sealed class DodgeGritCostCalculator : BlueprintComponent,
        IAbilityResourceCostCalculator
    {
        public int Calculate(AbilityData ability)
        {
            if (ability == null || ability.Caster == null) return 1;
            return TrueGritRuntime.Evaluate(ability.Caster,
                TrueGritDeed.GunslingersDodge, 1, false)
                .EffectiveCost;
        }
    }
}
