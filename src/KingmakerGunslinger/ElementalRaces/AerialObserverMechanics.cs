using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.PubSubSystem;
using UnityEngine;
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
                typeof(Fact).GetProperty("Components")?.PropertyType != typeof(List<GameLogicComponent>) ||
                typeof(GameLogicComponent).GetProperty("Fact")?.SetMethod == null ||
                !typeof(GameLogicComponent).GetProperty("Fact").SetMethod.IsPublic ||
                !typeof(Fact).GetMethod("TurnOn", Type.EmptyTypes).IsVirtual ||
                !typeof(Fact).GetMethod("TurnOff", new[] { typeof(bool) }).IsVirtual ||
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

    // One native instance-local listener per provider/carrier pair. It lives only
    // in that owner's exact flight fact; registered blueprint components are unchanged.
    [Serializable]
    public sealed class AerialObserverFlightTransition : OwnedGameLogicComponent<UnitDescriptor>
    {
        [NonSerialized] internal AerialObserverPerceptionBonus Provider;
        [NonSerialized] internal bool NativeOn;
        public override void OnTurnOn() { NativeOn = true; Provider?.RefreshModifier(); }
        public override void OnTurnOff() { NativeOn = false; Provider?.RefreshModifier(); }
    }

    // Dormant without the unregistered provider. Native flight lifecycle maintains
    // the actual skill stat, including passive/raw consumers; no polling or patch.
    [Serializable]
    public sealed class AerialObserverPerceptionBonus : RuleInitiatorLogicComponent<RuleSkillCheck>, IUnitBuffHandler
    {
        public BlueprintBuff FlightCarrier;
        [NonSerialized] private ConditionalWeakTable<RuleSkillCheck, object> _seen;
        [NonSerialized] private Dictionary<Buff, AerialObserverFlightTransition> _bindings;
        [NonSerialized] private ModifiableValue.Modifier _modifier;
        [NonSerialized] private bool _enabled;

        public override void OnFactActivate()
        {
            AerialObserverFlightContract.VerifyCarrier(FlightCarrier);
            _enabled = true;
            Reconcile();
        }

        public override void OnFactDeactivate()
        {
            _enabled = false;
            RefreshModifier();
            if (_bindings != null)
            {
                foreach (var pair in _bindings.ToArray()) Detach(pair.Key, pair.Value);
                _bindings.Clear();
            }
            _seen = null;
        }

        public void HandleBuffDidAdded(Buff buff) { IfOwnedCarrierChanged(buff); }
        public void HandleBuffDidRemoved(Buff buff) { IfOwnedCarrierChanged(buff); }
        private void IfOwnedCarrierChanged(Buff buff)
        {
            if (_enabled && buff != null && ReferenceEquals(buff.Owner, Owner) &&
                ReferenceEquals(buff.Blueprint, FlightCarrier)) Reconcile();
        }

        private void Reconcile()
        {
            if (!_enabled || Owner == null) return;
            if (_bindings == null) _bindings = new Dictionary<Buff, AerialObserverFlightTransition>();
            var carriers = Owner.Buffs.Enumerable.Where(b => ReferenceEquals(b.Blueprint, FlightCarrier)).ToArray();
            foreach (var pair in _bindings.Where(p => !carriers.Contains(p.Key) || p.Key.IsDisposed).ToArray())
            {
                Detach(pair.Key, pair.Value);
                _bindings.Remove(pair.Key);
            }
            foreach (var carrier in carriers)
            {
                if (_bindings.ContainsKey(carrier) || carrier.IsDisposed) continue;
                var listener = ScriptableObject.CreateInstance<AerialObserverFlightTransition>();
                listener.name = "$UnpublishedAerialFlightTransition_" + Guid.NewGuid().ToString("N");
                listener.Fact = carrier;
                listener.Provider = this;
                listener.NativeOn = carrier.Active && carrier.IsTurnedOn && !carrier.IsSuppressed;
                carrier.Components.Add(listener);
                _bindings.Add(carrier, listener);
            }
            RefreshModifier();
        }

        internal void RefreshModifier()
        {
            if (Owner == null) return;
            bool flight = _enabled && _bindings != null && _bindings.Any(p => !p.Key.IsDisposed &&
                AerialObserverPolicy.ExactFlight(FlightCarrier, p.Key.Blueprint, p.Value.NativeOn, p.Key.IsSuppressed, true));
            int bonus = AerialObserverPolicy.Bonus(_enabled, flight, true, true);
            var stat = Owner.Stats.SkillPerception;
            if (bonus != 0)
            {
                if (_modifier != null && stat.Modifiers.Contains(_modifier)) return;
                _modifier = stat.AddModifier(bonus, Fact, GetType().FullName, ModifierDescriptor.Trait);
            }
            else
            {
                _modifier?.Remove();
                _modifier = null;
            }
            stat.UpdateValue();
        }

        private static void Detach(Buff carrier, AerialObserverFlightTransition listener)
        {
            if (ReferenceEquals(listener, null)) return;
            listener.Provider = null;
            listener.NativeOn = false;
            int index = carrier.Components.FindIndex(c => ReferenceEquals(c, listener));
            if (index >= 0) carrier.Components.RemoveAt(index);
            if (listener != null) UnityEngine.Object.Destroy(listener);
        }

        public override void OnEventAboutToTrigger(RuleSkillCheck evt)
        {
            if (evt == null || !_enabled || Owner == null || !Fact.Active || evt.StatType != StatType.SkillPerception) return;
            if (_seen == null) _seen = new ConditionalWeakTable<RuleSkillCheck, object>();
            object ignored;
            if (_seen.TryGetValue(evt, out ignored)) return;
            _seen.Add(evt, new object());
            Reconcile();
        }
        public override void OnEventDidTrigger(RuleSkillCheck evt) { }
    }
}
