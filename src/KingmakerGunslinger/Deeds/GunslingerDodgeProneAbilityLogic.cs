using System;
using System.Collections.Generic;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using UnityEngine;

namespace KingmakerGunslinger.Deeds
{
    [Serializable]
    public sealed class GunslingerDodgeProneAbilityLogic : AbilityCustomLogic,
        IAbilityAvailabilityProvider
    {
        internal static readonly TimeSpan OneRoundDuration =
            TimeSpan.FromSeconds(6d);

        [SerializeField] private BlueprintUnitFact m_ArmedMarker;
        [SerializeField] private BlueprintBuff m_ArmorClassBuff;

        internal static GunslingerDodgeProneAbilityLogic Create(
            BlueprintUnitFact armedMarker, BlueprintBuff armorClassBuff)
        {
            if (armedMarker == null) throw new ArgumentNullException("armedMarker");
            if (armorClassBuff == null) throw new ArgumentNullException("armorClassBuff");
            var result = ScriptableObject.CreateInstance<GunslingerDodgeProneAbilityLogic>();
            result.name = "$KMG_GunslingerDodge_ImmediateDelivery";
            result.m_ArmedMarker = armedMarker;
            result.m_ArmorClassBuff = armorClassBuff;
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
            int acBefore = caster.Stats == null ? 0 : caster.Stats.AC.ModifiedValue;
            GunslingerDodgeRuntimeDiagnostics.RecordDeliveryEntered(acBefore);

            try
            {
                if (caster.HasFact(m_ArmorClassBuff))
                    throw new InvalidOperationException(
                        "Gunslinger's Dodge is already active on the caster.");
                if (caster.HasFact(m_ArmedMarker)) caster.RemoveFact(m_ArmedMarker);

                var buffContext = new MechanicsContext(context.Caster, caster,
                    context.Ability.Blueprint, null,
                    new TargetWrapper(context.Caster));
                Buff buff = caster.Buffs.AddBuff(
                    m_ArmorClassBuff,
                    buffContext,
                    OneRoundDuration);
                if (buff == null)
                    throw new InvalidOperationException(
                        "Gunslinger's Dodge buff was not created.");

                // Kingmaker accepted the fact and its AC component but did not
                // schedule expiration when this overload was called from custom
                // ability delivery.  Set the absolute native game-time deadline
                // and refresh the BuffCollection event queue explicitly.  This is
                // the same lifecycle seam used by the Kingmaker Turn-Based mod
                // when changing a live Buff's duration.
                TimeSpan scheduledEnd = Game.Instance.TimeController.GameTime +
                    OneRoundDuration;
                buff.EndTime = scheduledEnd;
                caster.Buffs.UpdateNextEvent();
                if (buff.IsPermanent || buff.EndTime != scheduledEnd ||
                    buff.TimeLeft <= TimeSpan.Zero ||
                    buff.TimeLeft > OneRoundDuration)
                    throw new InvalidOperationException(
                        "Gunslinger's Dodge buff did not retain its bounded " +
                        "one-round native expiration.");

                int acAfter = caster.Stats == null ? 0 :
                    caster.Stats.AC.ModifiedValue;
                GunslingerDodgeRuntimeDiagnostics.RecordDeliveryApplied(
                    buff, acBefore, acAfter);
            }
            catch (Exception exception)
            {
                GunslingerDodgeRuntimeDiagnostics.RecordDeliveryFault(
                    exception, acBefore,
                    caster.Stats == null ? 0 : caster.Stats.AC.ModifiedValue);
                throw;
            }

            yield return new AbilityDeliveryTarget(new TargetWrapper(context.Caster));
        }

        public override void Cleanup(AbilityExecutionContext context) { }
        internal BlueprintUnitFact ArmedMarker { get { return m_ArmedMarker; } }
        internal BlueprintBuff ArmorClassBuff { get { return m_ArmorClassBuff; } }
        internal TimeSpan Duration { get { return OneRoundDuration; } }
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
