using System;
using System.Collections.Generic;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using UnityEngine;

namespace KingmakerGunslinger.BrownFur
{
    [Serializable]
    public sealed class BrownFurPowerfulChangeSelectionLogic : AbilityCustomLogic
    {
        [SerializeField] private BlueprintBuff m_Selected;
        [SerializeField] private BlueprintBuff[] m_All;

        internal static BrownFurPowerfulChangeSelectionLogic Create(
            BlueprintBuff selected, BlueprintBuff[] all)
        {
            if (selected == null) throw new ArgumentNullException("selected");
            if (all == null || all.Length != 6) throw new ArgumentException(
                "Powerful Change requires the six exact pending-stat buffs.", "all");
            var result = ScriptableObject.CreateInstance<
                BrownFurPowerfulChangeSelectionLogic>();
            result.name = "$KMG_BrownFur_SelectPowerfulChange";
            result.m_Selected = selected;
            result.m_All = (BlueprintBuff[])all.Clone();
            return result;
        }

        public override IEnumerator<AbilityDeliveryTarget> Deliver(
            AbilityExecutionContext context, TargetWrapper target)
        {
            if (context == null || context.Caster == null ||
                context.Caster.Descriptor == null || m_Selected == null ||
                m_All == null || m_All.Length != 6)
                throw new InvalidOperationException(
                    "Powerful Change selection is missing its caster or exact buff set.");
            UnitDescriptor caster = context.Caster.Descriptor;
            foreach (BlueprintBuff pending in m_All)
            {
                if (pending == null) throw new InvalidOperationException(
                    "Powerful Change selection contains a null pending buff.");
                if (caster.HasFact(pending)) caster.RemoveFact(pending);
            }
            if (caster.AddFact(m_Selected) == null) throw new InvalidOperationException(
                "Powerful Change could not persist the selected ability score.");
            yield return new AbilityDeliveryTarget(target);
        }

        public override void Cleanup(AbilityExecutionContext context) { }

        internal BlueprintBuff Selected { get { return m_Selected; } }
        internal BlueprintBuff[] All { get { return (BlueprintBuff[])m_All.Clone(); } }
    }
}
