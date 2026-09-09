using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.ElementalRaces
{
    [Serializable]
    public sealed class ElementalNereidParameters : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
    {
        public BlueprintAbility Ability;
        public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            if (Owner == null || evt == null || !ReferenceEquals(evt.Blueprint, Ability)) return;
            evt.ReplaceCasterLevel = Math.Max(1, Owner.Progression.CharacterLevel);
            evt.ReplaceSpellLevel = 0;
            evt.ReplaceStat = StatType.Charisma;
            evt.ReplaceDC = ElementalNereidPolicy.DifficultyClass(Owner.Progression.CharacterLevel,
                Owner.Stats.Charisma.Bonus);
        }
        public override void OnEventDidTrigger(RuleCalculateAbilityParams evt) { }
    }

    [Serializable]
    public sealed class ElementalNereidActivate : ContextAction
    {
        public BlueprintBuff Aura;
        public override string GetCaption() { return "Create the owned Nereid fascination aura"; }
        public override void RunAction()
        {
            UnitEntityData caster = Context.MaybeCaster;
            if (caster == null || caster.Descriptor.State.IsDead || Aura == null) return;
            // The aura carrier does not fascinate its owner. Keep the native
            // supernatural source/parameters, but remove the inherited mental
            // descriptor only on this new benign child context.
            var carrier = Context.CloneFor(Aura, caster.Descriptor);
            carrier.RemoveSpellDescriptor(SpellDescriptor.MindAffecting);
            Buff buff = caster.Descriptor.AddBuff(Aura, carrier,
                ElementalNereidPolicy.DurationRounds(caster.Descriptor.Progression.CharacterLevel).Rounds().Seconds);
            if (buff != null) buff.IsNotDispelable = true;
        }
    }

    // This ledger belongs to the exact timed caster buff. Native area rebuilds
    // reuse it; neither loading nor area entry restarts duration or saving throws.
    [Serializable]
    public sealed class ElementalNereidAuraState : BuffLogic, IUnitLostFactHandler
    {
        public BlueprintFeature Marker;
        public BlueprintBuff Fascinated;
        public BlueprintBuff Assistance;
        // Native Fact.PostLoad assigns JsonExtensionData values directly.
        // Its array token must be accepted before this component decodes the
        // exact owned key/value ledger; a typed Dictionary aborts native load.
        [JsonProperty] private JArray _responses = new JArray();
        [NonSerialized] private Dictionary<string, int> _responseLookup;
        private Dictionary<string, int> Responses
        {
            get
            {
                if (_responseLookup != null) return _responseLookup;
                var entries = new List<KeyValuePair<string, int>>();
                if (_responses != null)
                    foreach (var token in _responses)
                    {
                        var row = token as JObject;
                        if (row == null || row["Key"]?.Type != JTokenType.String || row["Value"]?.Type != JTokenType.Integer)
                            throw new InvalidOperationException("Malformed owned Nereid response entry.");
                        entries.Add(new KeyValuePair<string, int>((string)row["Key"], (int)row["Value"]));
                    }
                return _responseLookup = ElementalNereidPolicy.RestoreResponses(entries);
            }
        }
        // Commands are transient in native saves. Keep this memory on the
        // activation, so leaving/re-entering or rebuilding a target buff cannot
        // grant another save for the same still-running hostile approach.
        [NonSerialized] private Dictionary<string, Dictionary<string, UnitCommand>> _approaches;

        private void ObserveQualificationHydration(string phase)
        {
            if (!ElementalAlternateTraitPolicy.NereidQualificationActive) return;
            ModContext context;
            if (!ModContext.TryGet(out context)) return;
            context.Logger.Info("elemental-races", "nereid.hydration",
                "phase=" + phase + ";component=" + GetInstanceID() +
                ";fact=" + (Buff == null ? 0 : System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(Buff)) +
                ";owner=" + Owner?.Unit?.UniqueId + ";ownerTurnedOn=" + Owner?.IsTurnedOn +
                ";initialized=" + Buff?.Initialized + ";active=" + Buff?.Active +
                ";endTicks=" + Buff?.EndTime.Ticks + ";responses=" + Responses.Count);
        }
        public override void PreSave()
        {
            _responses = new JArray(Responses.OrderBy(value => value.Key, StringComparer.Ordinal)
                .Select(value => new JObject { ["Key"] = value.Key, ["Value"] = value.Value }));
            ObserveQualificationHydration("pre-save");
            base.PreSave();
        }
        public override void PostLoad()
        {
            base.PostLoad();
            _responseLookup = null;
            // Decode before any area entry. Saved resistance/interruption must
            // never be mistaken for first exposure during native reconstruction.
            var restored = Responses;
            ObserveQualificationHydration("post-load");
        }
        public override void OnFactActivate()
        {
            ObserveQualificationHydration("fact-activate");
            KingmakerGunslinger.RuntimeTesting.ElementalNereidFactLoadObservation.Observe(Buff, "fact-activation");
        }
        public override void OnTurnOn() { ObserveQualificationHydration("turn-on"); }

        internal bool SaveApproach(UnitEntityData target, UnitEntityData approaching, UnitCommand command,
            MechanicsContext context)
        {
            if (_approaches == null)
                _approaches = new Dictionary<string, Dictionary<string, UnitCommand>>(StringComparer.Ordinal);
            Dictionary<string, UnitCommand> targetApproaches;
            if (!_approaches.TryGetValue(target.UniqueId, out targetApproaches))
                _approaches[target.UniqueId] = targetApproaches =
                    new Dictionary<string, UnitCommand>(StringComparer.Ordinal);
            UnitCommand previous;
            if (targetApproaches.TryGetValue(approaching.UniqueId, out previous) &&
                ReferenceEquals(previous, command)) return false;
            targetApproaches[approaching.UniqueId] = command;
            return Save(target, context);
        }

        internal ElementalNereidResponse Response(UnitEntityData target)
        {
            int result;
            return target != null && Responses.TryGetValue(target.UniqueId, out result)
                ? (ElementalNereidResponse)result : ElementalNereidResponse.Unseen;
        }
        internal void Interrupt(UnitEntityData target)
        {
            if (target == null) return;
            Response(target);
            Responses[target.UniqueId] = (int)ElementalNereidResponse.Interrupted;
        }
        internal bool Save(UnitEntityData target, MechanicsContext context)
        {
            RuleSavingThrow save = context.TriggerRule(new RuleSavingThrow(target,
                Kingmaker.EntitySystem.Stats.SavingThrowType.Will, context.Params.DC) { Reason = context });
            Responses[target.UniqueId] = (int)(save.IsPassed ? ElementalNereidResponse.Resisted : ElementalNereidResponse.Affected);
            return save.IsPassed;
        }
        internal void Enter(UnitEntityData target, MechanicsContext context, AreaEffectEntityData area,
            AbilityTargetHasFact person)
        {
            if (target == null || Buff == null || Buff.TimeLeft <= TimeSpan.Zero) return;
            bool eligible = !ReferenceEquals(target, Owner.Unit) && person != null &&
                person.CanTarget(Owner.Unit, new TargetWrapper(target));
            ElementalNereidEntry entry = ElementalNereidPolicy.Enter(Response(target), eligible, Buff.TimeLeft > TimeSpan.Zero);
            if (entry == ElementalNereidEntry.Ignore) return;
            if (entry == ElementalNereidEntry.Save && Save(target, context)) return;
            Buff effect = AddOwned(target, Fascinated, context, area);
            if (effect == null || !target.Descriptor.State.HasCondition(UnitCondition.Dazed)) Interrupt(target);
        }
        private Buff AddOwned(UnitEntityData target, BlueprintBuff blueprint, MechanicsContext context, AreaEffectEntityData area)
        {
            bool assistance = ReferenceEquals(blueprint, Assistance);
            Buff existing = target.Buffs.Enumerable.SingleOrDefault(value =>
                ReferenceEquals(value.Blueprint, blueprint) && (assistance
                    ? IncludesContext(value.Context, Buff.Context) : value.SourceAreaEffectId == area.UniqueId));
            if (existing != null) return existing;
            var application = context;
            if (ReferenceEquals(blueprint, Assistance))
            {
                application = context.CloneFor(blueprint, target.Descriptor);
                application.RemoveSpellDescriptor(SpellDescriptor.MindAffecting);
            }
            Buff added = target.Descriptor.AddBuff(blueprint, application, Buff.TimeLeft);
            if (added != null)
            {
                if (!assistance) added.SourceAreaEffectId = area.UniqueId;
                added.EndTime = Buff.EndTime;
                added.IsNotDispelable = true;
            }
            return added;
        }
        internal void RefreshAssistance(MechanicsContext context, AreaEffectEntityData area)
        {
            if (Buff == null || Buff.TimeLeft <= TimeSpan.Zero || area.IsEnded) return;
            var affected = area.UnitsInside.Where(unit => unit.Buffs.Enumerable.Any(value =>
                ReferenceEquals(value.Blueprint, Fascinated) && value.SourceAreaEffectId == area.UniqueId)).ToArray();
            if (affected.Length == 0) return;
            // Any ally may approach and shake a subject free. Ability availability
            // has no arbitrary aura-edge restriction; native touch targeting and
            // the exact fascinated-target checker govern the actual command.
            // These temporary grants belong to this aura context, not membership
            // in its harmful area, and never grant another racial ability.
            foreach (var helper in Game.Instance.State.Units.All.Where(unit => unit.IsInGame &&
                !unit.Descriptor.State.IsDead && affected.Any(target => !unit.IsEnemy(target))).ToArray())
                AddOwned(helper, Assistance, context, area);
        }
        public override void OnTurnOff()
        {
            ObserveQualificationHydration("turn-off");
            if (Game.Instance == null || Game.Instance.State == null) return;
            foreach (var unit in Game.Instance.State.Units.All.ToArray())
                foreach (var effect in unit.Buffs.Enumerable.Where(value =>
                    ReferenceEquals(value.Blueprint, Assistance) && IncludesContext(value.Context, Buff.Context)).ToArray())
                    effect.Remove();
        }
        internal static bool IncludesContext(MechanicsContext context, MechanicsContext ancestor)
        {
            var seen = new HashSet<MechanicsContext>();
            for (var current = context; current != null && seen.Add(current); current = current.ParentContext)
                if (ReferenceEquals(current, ancestor)) return true;
            return false;
        }
        public void HandleUnitLostFact(Fact fact)
        {
            Feature feature = fact as Feature;
            if (feature != null && ReferenceEquals(feature.Owner, Owner) && ReferenceEquals(feature.Blueprint, Marker))
                Buff.Remove();
        }
        internal static ElementalNereidAuraState Find(MechanicsContext context, BlueprintBuff aura)
        {
            UnitEntityData caster = context == null ? null : context.MaybeCaster;
            if (caster == null || aura == null) return null;
            var matches = caster.Buffs.Enumerable.Where(value => ReferenceEquals(value.Blueprint, aura) && IncludesContext(context, value.Context))
                .SelectMany(value => value.SelectComponents<ElementalNereidAuraState>()).ToArray();
            return matches.Length == 1 ? matches[0] : null;
        }
    }

    [Serializable]
    public sealed class ElementalNereidArea : AbilityAreaEffectLogic
    {
        public BlueprintBuff Aura;
        public AbilityTargetHasFact PersonCheck;
        public BlueprintBuff Fascinated;
        public BlueprintBuff Assistance;
        protected override void OnUnitEnter(MechanicsContext context, AreaEffectEntityData area, UnitEntityData unit)
        {
            var state = ElementalNereidAuraState.Find(context, Aura);
            if (state != null) state.Enter(unit, context, area, PersonCheck);
        }
        protected override void OnTick(MechanicsContext context, AreaEffectEntityData area)
        {
            var state = ElementalNereidAuraState.Find(context, Aura);
            if (state == null || area.IsEnded) return;
            foreach (var unit in area.UnitsInside.ToArray())
                foreach (var effect in unit.Buffs.Enumerable.Where(value =>
                    ReferenceEquals(value.Blueprint, Fascinated) && value.SourceAreaEffectId == area.UniqueId).ToArray())
                    foreach (var logic in effect.SelectComponents<ElementalNereidFascinated>().ToArray())
                        logic.ObserveThreats();
            state.RefreshAssistance(context, area);
        }
        protected override void OnUnitExit(MechanicsContext context, AreaEffectEntityData area, UnitEntityData unit)
        {
            foreach (Buff effect in unit.Buffs.Enumerable.Where(value => value.SourceAreaEffectId == area.UniqueId &&
                ReferenceEquals(value.Blueprint, Fascinated)).ToArray())
            {
                foreach (var logic in effect.SelectComponents<ElementalNereidFascinated>()) logic.LeavingArea = true;
                effect.Remove();
            }
        }
    }

    [Serializable]
    public sealed class ElementalNereidAssistance : BuffLogic
    {
        public BlueprintBuff Aura;
        public override void OnTurnOn()
        {
            // A helper loaded without its exact live source must not survive as
            // an orphaned ability. A legitimate reconstructed area can regrant it.
            var source = ElementalNereidAuraState.Find(Buff.Context, Aura);
            if (ElementalAlternateTraitPolicy.NereidQualificationActive)
            {
                ModContext context;
                if (ModContext.TryGet(out context)) context.Logger.Info("elemental-races", "nereid.helper-hydration",
                    "owner=" + Owner?.Unit?.UniqueId + ";sourceFound=" + (source != null) +
                    ";sourceComponent=" + (source == null ? 0 : source.GetInstanceID()) +
                    ";caster=" + Buff.Context?.MaybeCaster?.UniqueId);
            }
            if (source == null) Buff.Remove();
        }
    }

    // One condition penalty per affected creature, even when independent auras
    // overlap. The native modifier remains attached to one living source fact;
    // ending that source hands it to another, without touching foreign penalties.
    [Serializable]
    public sealed class ElementalNereidPerception : BuffLogic
    {
        [NonSerialized] private bool _present;
        [NonSerialized] private ModifiableValue.Modifier _modifier;
        public override void OnTurnOn()
        {
            _present = true;
            if (!Owner.Stats.SkillPerception.Modifiers.Any(value =>
                value.Source != null && ReferenceEquals(value.Source.Blueprint, Buff.Blueprint)))
                _modifier = Owner.Stats.SkillPerception.AddModifier(-4, this, Kingmaker.Enums.ModifierDescriptor.Penalty);
        }
        public override void OnTurnOff()
        {
            _present = false;
            if (_modifier == null) return;
            _modifier.Remove();
            _modifier = null;
            var next = Owner.Buffs.Enumerable.Where(value => ReferenceEquals(value.Blueprint, Buff.Blueprint))
                .SelectMany(value => value.SelectComponents<ElementalNereidPerception>())
                .FirstOrDefault(value => !ReferenceEquals(value, this) && value._present);
            if (next != null)
                next._modifier = Owner.Stats.SkillPerception.AddModifier(-4, next, Kingmaker.Enums.ModifierDescriptor.Penalty);
        }
    }

    // Native command-start notifications occur before damage. Only this buff
    // listens: native Bard and other mods' fascination are never patched.
    [Serializable]
    public sealed class ElementalNereidFascinated : BuffLogic, IUnitCommandStartHandler,
        IUnitActiveEquipmentSetHandler, ITargetRulebookHandler<RuleDealDamage>,
        ITargetRulebookHandler<RuleAttackWithWeapon>
    {
        public BlueprintBuff Aura;
        [NonSerialized] internal bool LeavingArea;
        [NonSerialized] private Dictionary<string, Vector3> _positions;
        [NonSerialized] private Dictionary<string, bool> _drawnWeapons;
        private ElementalNereidAuraState Source { get { return ElementalNereidAuraState.Find(Buff.Context, Aura); } }

        public override void OnTurnOn()
        {
            _positions = new Dictionary<string, Vector3>(StringComparer.Ordinal);
            _drawnWeapons = new Dictionary<string, bool>(StringComparer.Ordinal);
            if (Game.Instance == null || Game.Instance.State == null) return;
            foreach (var unit in Game.Instance.State.Units.All)
            {
                _positions[unit.UniqueId] = unit.Position;
                _drawnWeapons[unit.UniqueId] = HasDrawnWeapon(unit);
            }
        }

        private static bool HasDrawnWeapon(UnitEntityData unit)
        {
            if (unit == null || unit.View == null || unit.View.HandsEquipment == null ||
                !unit.View.HandsEquipment.InCombat) return false;
            return new[] { unit.Descriptor.Body.PrimaryHand, unit.Descriptor.Body.SecondaryHand }.Any(hand =>
                hand.MaybeWeapon != null && !hand.MaybeWeapon.Blueprint.IsUnarmed &&
                !hand.MaybeWeapon.Blueprint.IsNatural);
        }

        internal void ObserveThreats()
        {
            if (_positions == null || _drawnWeapons == null) OnTurnOn();
            foreach (var unit in Game.Instance.State.Units.All.ToArray())
            {
                Vector3 previous;
                bool previouslyDrawn;
                bool observed = _positions.TryGetValue(unit.UniqueId, out previous);
                bool drawn = HasDrawnWeapon(unit);
                bool drew = _drawnWeapons.TryGetValue(unit.UniqueId, out previouslyDrawn) && !previouslyDrawn && drawn;
                _positions[unit.UniqueId] = unit.Position;
                _drawnWeapons[unit.UniqueId] = drawn;
                if (drew && Perceives(unit)) { Break(); return; }
                if (!observed || !unit.IsEnemy(Owner.Unit) || !Perceives(unit, false) || unit.View == null ||
                    unit.View.MovementAgent == null || !unit.View.MovementAgent.IsReallyMoving ||
                    (unit.Position - Owner.Unit.Position).sqrMagnitude >=
                        (previous - Owner.Unit.Position).sqrMagnitude) continue;

                // Native UnitMoveTo starts after approach movement completes.
                // Observe actual displacement while its ordinary command/path
                // executes. Queueing alone is not a threat and this is at most
                // one additional save for that command, never one per tick.
                UnitCommand command = unit.Commands.Move;
                if (command == null) command = unit.Commands.Standard;
                if (command == null) command = unit.Commands.MoveContiniously;
                if (command == null || command.IsFinished) continue;
                var source = Source;
                if (source != null && source.SaveApproach(Owner.Unit, unit, command, Buff.Context)) { Break(); return; }
            }
        }

        private bool Perceives(UnitEntityData unit, bool precise = true)
        {
            if (unit == null || ReferenceEquals(unit, Owner.Unit) || unit.View == null || Owner.Unit.View == null)
                return false;
            // Native group memory can be supplied by another party member.
            // Only this subject's own senses identify a threat. Blindsense
            // locates an approaching creature; precise cues require sight or
            // active native blindsight at its ranked range and native LOS.
            var sense = Owner.Unit.Get<Kingmaker.UnitLogic.Parts.UnitPartBlindsense>();
            if (sense != null && sense.Reach(unit))
            {
                if (!precise) return true;
                float distance = Owner.Unit.DistanceTo(unit) - Owner.Unit.View.Corpulence - unit.View.Corpulence;
                if (Owner.Progression.Features.Enumerable.Cast<Kingmaker.Blueprints.Facts.Fact>()
                    .Concat(Owner.Buffs.Enumerable).Where(fact => fact.IsTurnedOn)
                    .SelectMany(fact => fact.SelectComponents<Kingmaker.Designers.Mechanics.Facts.Blindsense>())
                    .Any(value => value.Blindsight && distance <= value.Range.Meters * value.Fact.GetRank())) return true;
            }
            return Owner.Unit.Memory != null && Owner.Unit.Memory.ContainsVisible(unit) &&
                !Owner.State.HasCondition(UnitCondition.Blindness) && Owner.Unit.HasLOS(unit) &&
                (!unit.Descriptor.State.HasCondition(UnitCondition.Invisible) || Owner.IsSeeInvisibility) &&
                Kingmaker.UnitLogic.Parts.UnitPartConcealment.Calculate(Owner.Unit, unit, false) != Kingmaker.Enums.Concealment.Total;
        }
        private void Break()
        {
            var source = Source;
            if (source != null) source.Interrupt(Owner.Unit);
            Buff.Remove();
        }
        public void HandleUnitCommandDidStart(UnitCommand command)
        {
            if (command == null || !Perceives(command.Executor)) return;
            UnitAttack attack = command as UnitAttack;
            UnitUseAbility use = command as UnitUseAbility;
            if ((attack != null && ReferenceEquals(attack.Target, Owner.Unit)) ||
                (use != null && use.Spell != null && (use.Spell.Blueprint.Type == AbilityType.Spell ||
                    use.Spell.Blueprint.Type == AbilityType.SpellLike)))
            { Break(); return; }
        }
        public void HandleUnitChangeActiveEquipmentSet(UnitDescriptor unit)
        {
            if (unit != null && Perceives(unit.Unit) && HasDrawnWeapon(unit.Unit)) Break();
        }
        // Attacks of opportunity and other native rule-only attacks can bypass
        // UnitAttack command start. Break before their attack roll, including a
        // later miss, when this subject perceives the attacker.
        public void OnEventAboutToTrigger(RuleAttackWithWeapon evt)
        {
            if (evt != null && ReferenceEquals(evt.Target, Owner.Unit) && Perceives(evt.Initiator)) Break();
        }
        public void OnEventDidTrigger(RuleAttackWithWeapon evt) { }

        public void OnEventAboutToTrigger(RuleDealDamage evt) { }
        public void OnEventDidTrigger(RuleDealDamage evt)
        {
            if (evt != null && !evt.IsFake && ReferenceEquals(evt.Target, Owner.Unit) && evt.Damage > 0) Break();
        }
        public override void OnFactDeactivate()
        {
            if (!LeavingArea && Owner != null && Owner.IsTurnedOn)
            {
                var source = Source;
                if (source != null) source.Interrupt(Owner.Unit);
            }
        }
    }

    [Serializable]
    public sealed class ElementalNereidShakeTarget : BlueprintComponent, IAbilityTargetChecker
    {
        public BlueprintBuff Fascinated;
        public bool CanTarget(UnitEntityData caster, TargetWrapper target)
        {
            return caster != null && target != null && target.Unit != null &&
                !ReferenceEquals(caster, target.Unit) && !caster.IsEnemy(target.Unit) &&
                target.Unit.Buffs.Enumerable.Any(value => ReferenceEquals(value.Blueprint, Fascinated));
        }
    }

    [Serializable]
    public sealed class ElementalNereidShakeFree : ContextAction
    {
        public BlueprintBuff Fascinated;
        public override string GetCaption() { return "Shake an ally free of Nereid fascination"; }
        public override void RunAction()
        {
            if (Context.MaybeCaster == null || Target.Unit == null || Context.MaybeCaster.IsEnemy(Target.Unit)) return;
            foreach (Buff buff in Target.Unit.Buffs.Enumerable.Where(value =>
                ReferenceEquals(value.Blueprint, Fascinated)).ToArray()) buff.Remove();
        }
    }
}
