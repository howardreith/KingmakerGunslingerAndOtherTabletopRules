using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using KingmakerGunslinger.ElementalRaces;
using KingmakerGunslinger.Spells.ProtectionFromAlignment;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.Summoning
{
    // Private bard7 performance parameters. Returning a new object preserves
    // the native immutable/default parameter object and live Charisma.
    [Serializable]
    public sealed class LillendFascinateParameters32 : ContextAbilityParamsCalculator
    {
        public override AbilityParams Calculate(MechanicsContext context)
        {
            var caster = context?.MaybeCaster;
            return new AbilityParams { CasterLevel = 7, SpellLevel = 0,
                DC = 13 + (caster?.Stats.Charisma.Bonus ?? 0) };
        }
    }

    internal static class LillendFascinateEligibility32
    {
        private static readonly MethodInfo NativeImmunity = AccessTools.Method(
            typeof(BuffDescriptorImmunity), "IsImmune", new[] { typeof(MechanicsContext) }) ??
            throw new MissingMethodException(typeof(BuffDescriptorImmunity).FullName, "IsImmune");

        internal static bool Attentive(UnitEntityData actor, UnitEntityData target)
        {
            if (actor == null || target == null || ReferenceEquals(actor, target) ||
                actor.Destroyed || !actor.IsInGame || actor.IsInCombat || !actor.Descriptor.State.IsConscious ||
                target.Destroyed || !target.IsInGame || target.IsInCombat || !target.Descriptor.State.IsConscious ||
                target.Descriptor.State.HasCondition(UnitCondition.Sleeping) ||
                !LillendPerformanceSight32.Sees(actor, target, 90) ||
                !LillendPerformanceSight32.Sees(target, actor, 90)) return false;
            // A nearby witnessed fight distracts even an unengaged listener.
            // Do not suppress combat, change memory or confer an immunity to it.
            return !Game.Instance.State.Units.All.Any(unit => unit != null && unit.IsInCombat &&
                (LillendPerformanceSight32.Sees(actor, unit, 90) || LillendPerformanceSight32.Sees(target, unit, 90)));
        }
        internal static bool Immune(UnitEntityData actor, UnitEntityData target, BlueprintBuff effect)
        {
            if (target.Descriptor.State.HasConditionImmunity(UnitCondition.Dazed)) return true;
            var context = new MechanicsContext(actor, target.Descriptor, effect, null, new TargetWrapper(target));
            var facts = target.Descriptor.Progression.Features.Enumerable.Cast<Fact>()
                .Concat(target.Buffs.Enumerable).Where(fact => fact.IsTurnedOn).ToArray();
            return facts.SelectMany(fact => fact.SelectComponents<SpecificBuffImmunity>())
                .Any(value => ReferenceEquals(value.Buff, effect)) || facts
                .SelectMany(fact => fact.SelectComponents<BuffDescriptorImmunity>())
                .Any(value => (bool)NativeImmunity.Invoke(value, new object[] { context }));
        }
        internal static bool Selectable(UnitEntityData actor, UnitEntityData target,
            BlueprintBuff effect, BlueprintBuff resistance)
        {
            return Attentive(actor, target) && actor.IsEnemy(target) &&
                !target.Descriptor.State.HasCondition(UnitCondition.Dazed) && !Immune(actor, target, effect) &&
                !ContextActionArchonMenace.HasSource(target, resistance, actor);
        }
    }

    // A frozen set of at most three identities belongs to this activation.
    // Save resistance is a separate native 24-hour buff, not this ledger.
    [Serializable]
    public sealed class LillendFascinateState32 : BuffLogic, IActivatableAbilityWillStopHandler
    {
        public BlueprintBuff Fascinated;
        public BlueprintBuff Assistance;
        public BlueprintBuff Resistance;
        public BlueprintAbility BardicSuggestion;
        [JsonProperty] private JArray _responses = new JArray();
        [JsonProperty] private bool _ended;
        [NonSerialized] private bool _ready;
        [NonSerialized] private Dictionary<string, int> _lookup;
        [NonSerialized] private Dictionary<string, Dictionary<string, UnitCommand>> _approaches;
        private Dictionary<string, int> Responses
        {
            get
            {
                if (_lookup != null) return _lookup;
                var rows = new List<KeyValuePair<string, int>>();
                foreach (var token in _responses ?? new JArray())
                {
                    var row = token as JObject;
                    if (row == null || row["Key"]?.Type != JTokenType.String || row["Value"]?.Type != JTokenType.Integer)
                        throw new InvalidOperationException("Malformed Lillend Fascinate response.");
                    rows.Add(new KeyValuePair<string, int>((string)row["Key"], (int)row["Value"]));
                }
                if (rows.Count > 3) throw new InvalidOperationException("Lillend Fascinate exceeds its three selected targets.");
                return _lookup = ElementalNereidPolicy.RestoreResponses(rows);
            }
        }
        public override void OnFactActivate()
        {
            var selected = LillendNoncombat32.Find(Owner.Unit)?.TakeFascinateSelection();
            if (selected == null) return;
            _lookup = selected.ToDictionary(value => value, value => 0, StringComparer.Ordinal);
            _ready = true;
        }
        public override void PreSave()
        {
            _responses = new JArray(Responses.OrderBy(row => row.Key, StringComparer.Ordinal)
                .Select(row => new JObject { ["Key"] = row.Key, ["Value"] = row.Value }));
            base.PreSave();
        }
        public override void PostLoad()
        {
            base.PostLoad(); _lookup = null;
            var restored = Responses; _ready = true;
        }
        internal int Response(UnitEntityData target)
        {
            int result;
            return target != null && Responses.TryGetValue(target.UniqueId, out result) ? result : -1;
        }
        internal bool Active => _ready && !_ended && Buff.Active;
        internal bool Owns(UnitEntityData target) => Active && Response(target) == 1 &&
            target.Buffs.Enumerable.Any(value => ReferenceEquals(value.Blueprint, Fascinated) &&
                ElementalNereidAuraState.IncludesContext(value.Context, Buff.Context));
        internal bool ShouldStop => _ready && (_ended || !Game.Instance.State.Units.All.Any(target =>
            (Response(target) == 0 || Response(target) == 1) &&
            LillendFascinateEligibility32.Attentive(Owner.Unit, target)));
        internal void Interrupt(UnitEntityData target)
        { if (target != null && Responses.ContainsKey(target.UniqueId)) Responses[target.UniqueId] = 3; }
        internal bool Save(UnitEntityData target, MechanicsContext context)
        {
            var save = context.TriggerRule(new RuleSavingThrow(target, SavingThrowType.Will,
                13 + Owner.Stats.Charisma.Bonus) { Reason = context });
            Responses[target.UniqueId] = save.IsPassed ? 2 : 1;
            if (save.IsPassed && !ContextActionArchonMenace.HasSource(target, Resistance, Owner.Unit))
            {
                // Benign, independent, source-specific memory. Neither ending
                // this performance nor losing sight removes the 24-hour refusal.
                var memory = new MechanicsContext(Owner.Unit, target.Descriptor, Resistance, null, new TargetWrapper(target));
                var marker = target.Descriptor.AddBuff(Resistance, memory, TimeSpan.FromHours(24));
                if (marker != null) { marker.IsNotDispelable = true; marker.IsFromSpell = false; }
            }
            return save.IsPassed;
        }
        internal bool SaveApproach(UnitEntityData target, UnitEntityData approaching, UnitCommand command, MechanicsContext context)
        {
            if (_approaches == null) _approaches = new Dictionary<string, Dictionary<string, UnitCommand>>(StringComparer.Ordinal);
            Dictionary<string, UnitCommand> seen;
            if (!_approaches.TryGetValue(target.UniqueId, out seen))
                _approaches[target.UniqueId] = seen = new Dictionary<string, UnitCommand>(StringComparer.Ordinal);
            UnitCommand old;
            if (seen.TryGetValue(approaching.UniqueId, out old) && ReferenceEquals(old, command)) return false;
            seen[approaching.UniqueId] = command;
            return Save(target, context);
        }
        internal void Enter(UnitEntityData target, MechanicsContext context, AreaEffectEntityData area)
        {
            if (!Active || target == null || area.IsEnded) return;
            int response = Response(target);
            if (response != 0 && response != 1) return;
            if (response == 0 && ContextActionArchonMenace.HasSource(target, Resistance, Owner.Unit))
            { Responses[target.UniqueId] = 2; return; }
            if (response == 0 && !LillendFascinateEligibility32.Selectable(Owner.Unit, target, Fascinated, Resistance))
            { Interrupt(target); return; }
            if (!LillendFascinateEligibility32.Attentive(Owner.Unit, target) ||
                LillendFascinateEligibility32.Immune(Owner.Unit, target, Fascinated))
            { Interrupt(target); RemoveTarget(target); return; }
            if (response == 0 && Save(target, context)) return;
            if (Owns(target)) return;
            var effect = target.Descriptor.AddBuff(Fascinated, context, null);
            if (effect == null || !target.Descriptor.State.HasCondition(UnitCondition.Dazed)) { Interrupt(target); return; }
            effect.SourceAreaEffectId = area.UniqueId; effect.IsNotDispelable = true; effect.IsFromSpell = false;
        }
        internal void RemoveTarget(UnitEntityData target)
        {
            if (target == null) return;
            foreach (var effect in target.Buffs.Enumerable.Where(value => ReferenceEquals(value.Blueprint, Fascinated) &&
                ElementalNereidAuraState.IncludesContext(value.Context, Buff.Context)).ToArray()) effect.Remove();
        }
        internal void RefreshAssistance(MechanicsContext context)
        {
            if (!Active) return;
            var affected = Game.Instance.State.Units.All.Where(Owns).ToArray();
            foreach (var helper in Game.Instance.State.Units.All.ToArray())
            {
                var grants = helper.Buffs.Enumerable.Where(value => ReferenceEquals(value.Blueprint, Assistance) &&
                    ElementalNereidAuraState.IncludesContext(value.Context, Buff.Context)).ToArray();
                bool useful = helper.IsInGame && !helper.Descriptor.State.IsDead && affected.Any(target => !helper.IsEnemy(target));
                if (!useful) { foreach (var grant in grants) grant.Remove(); continue; }
                if (grants.Length != 0) continue;
                var benign = context.CloneFor(Assistance, helper.Descriptor);
                benign.RemoveSpellDescriptor(SpellDescriptor.MindAffecting);
                benign.RemoveSpellDescriptor(SpellDescriptor.Compulsion);
                var added = helper.Descriptor.AddBuff(Assistance, benign, null);
                if (added != null) { added.IsNotDispelable = true; added.IsFromSpell = false; }
            }
        }
        public void HandleActivatableAbilityWillStop(ActivatableAbility ability)
        {
            if (ReferenceEquals(ability.Owner, Owner) && ReferenceEquals(ability.Blueprint.Buff, Buff.Blueprint)) End();
        }
        private void End()
        {
            _ended = true;
            if (Game.Instance?.State?.Units == null) return;
            foreach (var unit in Game.Instance.State.Units.All.ToArray())
                foreach (var grant in unit.Buffs.Enumerable.Where(value =>
                    (ReferenceEquals(value.Blueprint, Fascinated) || ReferenceEquals(value.Blueprint, Assistance)) &&
                    ElementalNereidAuraState.IncludesContext(value.Context, Buff.Context)).ToArray()) grant.Remove();
        }
        public override void OnTurnOff() { End(); }
        internal static LillendFascinateState32 Find(MechanicsContext context, BlueprintBuff carrier)
        {
            return context?.MaybeCaster?.Buffs.Enumerable.Where(value => value.Active && ReferenceEquals(value.Blueprint, carrier) &&
                ElementalNereidAuraState.IncludesContext(context, value.Context))
                .SelectMany(value => value.SelectComponents<LillendFascinateState32>()).SingleOrDefault();
        }
    }

    [Serializable]
    public sealed class LillendFascinateArea32 : AbilityAreaEffectLogic
    {
        public BlueprintBuff Carrier;
        protected override void OnUnitEnter(MechanicsContext context, AreaEffectEntityData area, UnitEntityData unit)
        { LillendFascinateState32.Find(context, Carrier)?.Enter(unit, context, area); }
        protected override void OnTick(MechanicsContext context, AreaEffectEntityData area)
        {
            var state = LillendFascinateState32.Find(context, Carrier);
            if (state == null || area.IsEnded) return;
            foreach (var unit in area.UnitsInside.ToArray())
            {
                state.Enter(unit, context, area);
                foreach (var logic in unit.Buffs.Enumerable.Where(value => value.SourceAreaEffectId == area.UniqueId)
                    .SelectMany(value => value.SelectComponents<LillendFascinated32>()).ToArray()) logic.ObserveThreats();
            }
            state.RefreshAssistance(context);
            if (state.ShouldStop)
            {
                var toggle = context.MaybeCaster.Descriptor.ActivatableAbilities.Enumerable.SingleOrDefault(value =>
                    ReferenceEquals(value.Blueprint.Buff, Carrier));
                if (toggle != null) toggle.IsOn = false;
            }
        }
        protected override void OnUnitExit(MechanicsContext context, AreaEffectEntityData area, UnitEntityData unit)
        {
            var state = LillendFascinateState32.Find(context, Carrier);
            state?.Interrupt(unit); state?.RemoveTarget(unit);
        }
    }

    [Serializable]
    public sealed class LillendFascinateAssistance32 : BuffLogic
    {
        public BlueprintBuff Carrier;
        public override void OnTurnOn()
        {
            if (LillendFascinateState32.Find(Buff.Context, Carrier)?.Active != true) Buff.Remove();
        }
    }

    // The bardic SLA can target an already fascinated subject. The ordinary
    // combat spell chooser intentionally rejects Dazed subjects and therefore
    // is not the contract for this separate, exact-performance route.
    [Serializable]
    public sealed class LillendBardicSuggestionTarget32 : BlueprintComponent, IAbilityTargetChecker
    {
        public BlueprintBuff Carrier;
        public BlueprintBuff Effect;
        public BlueprintAbility Ability;
        public bool CanTarget(UnitEntityData caster, TargetWrapper target)
        {
            var unit = target?.Unit;
            if (!Sprint32Suggestion.Active || caster == null || unit == null || Ability == null ||
                !LillendFascinateEligibility32.Attentive(caster, unit) ||
                LillendFascinateEligibility32.Immune(caster, unit, Effect) ||
                unit.Buffs.Enumerable.Any(value => ReferenceEquals(value.Blueprint, Effect))) return false;
            var state = caster.Buffs.Enumerable.Where(value => value.Active && ReferenceEquals(value.Blueprint, Carrier))
                .SelectMany(value => value.SelectComponents<LillendFascinateState32>()).SingleOrDefault();
            if (state == null || !state.Owns(unit)) return false;
            var context = new MechanicsContext(caster, caster.Descriptor, Ability, null, target);
            var immunity = unit.Get<Kingmaker.UnitLogic.Parts.UnitPartSpellResistance>();
            if (immunity != null && immunity.IsImmune(context)) return false;
            foreach (var guard in unit.Descriptor.Progression.Features.Enumerable.Cast<Fact>()
                .Concat(unit.Buffs.Enumerable).Where(fact => fact.IsTurnedOn)
                .SelectMany(fact => fact.SelectComponents<ProtectionFromAlignmentControlImmunityComponent>()))
            {
                ProtectionAlignment against;
                if (ProtectionFromAlignmentRuntime.TryMapProtectedAlignment(guard.ProtectedAgainstAlignment, out against) &&
                    ProtectionFromAlignmentRuntime.Evaluate(new ProtectionControlImmunityRequest(against,
                        Ability.AssetGuid, Effect.AssetGuid, caster.Descriptor.Alignment != null,
                        caster.Descriptor.Alignment == null ? ProtectionAlignment.None :
                        ProtectionFromAlignmentRuntime.FromNativeAlignment(caster.Descriptor.Alignment.Value))).ShouldBlock) return false;
            }
            return true;
        }
    }
}
