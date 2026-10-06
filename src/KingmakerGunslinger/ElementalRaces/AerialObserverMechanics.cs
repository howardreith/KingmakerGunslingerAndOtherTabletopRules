using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Components;

namespace KingmakerGunslinger.ElementalRaces
{
    internal static class AerialObserverFlightContract
    {
        internal const string CarrierGuid = "e116e1e0a17a4aceb001000000000019";

        internal static void VerifyNativeContract(Type skillRule)
        {
            var trigger = skillRule?.GetMethod("OnTrigger", new[] { typeof(RulebookEventContext) });
            if (skillRule != typeof(RuleSkillCheck) || trigger == null || trigger.ReturnType != typeof(void) ||
                skillRule.GetProperty("StatType")?.PropertyType != typeof(StatType) ||
                typeof(RuleCachedPerceptionCheck).BaseType != typeof(RuleSkillCheck) ||
                typeof(RuleCachedPerceptionCheck).GetConstructor(new[] { typeof(Kingmaker.EntitySystem.Entities.UnitEntityData), typeof(int) }) == null ||
                typeof(RulebookEvent).GetMethod("AddTemporaryModifier", new[] { typeof(ModifiableValue.Modifier) }) == null ||
                typeof(Buff).GetProperty("Active")?.PropertyType != typeof(bool) ||
                typeof(Buff).GetProperty("IsSuppressed")?.PropertyType != typeof(bool) ||
                (int)StatType.SkillPerception != 20)
                throw new InvalidOperationException("Exact Aerial Observer Perception/temporary-modifier contract absent.");
        }

        internal static void VerifyCarrier(BlueprintBuff carrier)
        {
            VerifyNativeContract(typeof(RuleSkillCheck));
            if (carrier == null || carrier.AssetGuid != CarrierGuid ||
                !ReferenceEquals(carrier, ResourcesLibrary.TryGetBlueprint<BlueprintBuff>(CarrierGuid)))
                throw new InvalidOperationException("Aerial Observer requires the canonical released Wings of Air carrier.");
            var components = carrier.ComponentsArray ?? Array.Empty<BlueprintComponent>();
            var ac = components.OfType<ACBonusAgainstAttacks>().SingleOrDefault();
            var terrain = components.OfType<AddConditionImmunity>().SingleOrDefault();
            var ground = components.OfType<BuffDescriptorImmunity>().SingleOrDefault();
            if (components.Length != 3 || ac == null || terrain == null || ground == null ||
                ac.ArmorClassBonus != 3 || ac.Descriptor != ModifierDescriptor.Dodge ||
                !ac.AgainstMeleeOnly || ac.AgainstRangedOnly || ac.CheckArmorCategory ||
                ac.NoShield || ac.NotTouch || ac.OnlyAttacksOfOpportunity ||
                terrain.Condition != UnitCondition.DifficultTerrain || ground.CheckFact ||
                (SpellDescriptor)ground.Descriptor != SpellDescriptor.Ground ||
                ground.FactToCheck != null || ground.IgnoreFeature != null)
                throw new InvalidOperationException("The exact released mechanical-flight graph does not match.");
        }

        internal static bool HasExactFlight(UnitDescriptor owner, BlueprintBuff canonicalCarrier)
        {
            if (owner?.Buffs == null || canonicalCarrier == null) return false;
            return owner.Buffs.Enumerable.Any(buff => AerialObserverPolicy.ExactFlight(
                canonicalCarrier, buff.Blueprint, buff.Active, buff.IsSuppressed, true));
        }
    }

    // Dormant unless attached by the request-local fixture (or a later authorized
    // publication). Skill and cached Perception share the native stat resolution.
    // A temporary native stat modifier preserves Trait stacking; no lasting overlay.
    [Serializable]
    public sealed class AerialObserverPerceptionBonus : RuleInitiatorLogicComponent<RuleSkillCheck>
    {
        public BlueprintBuff FlightCarrier;
        [NonSerialized] private ConditionalWeakTable<RuleSkillCheck, object> _seen;

        public override void OnFactActivate()
        {
            AerialObserverFlightContract.VerifyCarrier(FlightCarrier);
        }

        public override void OnFactDeactivate() { _seen = null; }

        public override void OnEventAboutToTrigger(RuleSkillCheck evt)
        {
            if (evt == null || Owner == null || !Fact.Active || evt.StatType != StatType.SkillPerception) return;
            if (_seen == null) _seen = new ConditionalWeakTable<RuleSkillCheck, object>();
            object ignored;
            if (_seen.TryGetValue(evt, out ignored)) return;
            _seen.Add(evt, new object());
            int bonus = AerialObserverPolicy.Bonus(true,
                AerialObserverFlightContract.HasExactFlight(Owner, FlightCarrier), true, true);
            if (bonus == 0) return;
            var stat = Owner.Stats.SkillPerception;
            var modifier = stat.AddModifier(bonus, Fact, GetType().FullName, ModifierDescriptor.Trait);
            if (modifier == null) return;
            stat.UpdateValue();
            evt.AddTemporaryModifier(modifier);
        }

        public override void OnEventDidTrigger(RuleSkillCheck evt) { }
    }
}
