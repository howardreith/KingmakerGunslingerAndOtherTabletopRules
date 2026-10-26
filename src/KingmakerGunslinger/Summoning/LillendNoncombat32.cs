using System;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json;

namespace KingmakerGunslinger.Summoning
{
    // Native HasLOS includes the short enemy-acquisition radius. These two
    // private performances instead use their printed range and the native eye
    // geometry, visibility conditions and spotted-stealth predicate. They do
    // not add enemies to memory or delay combat. The engine has no separate
    // hearing/deafness state on the supported surface.
    internal static class LillendPerformanceSight32
    {
        internal static bool Sees(UnitEntityData observer, UnitEntityData subject, int feet)
        {
            var geometry = Kingmaker.Visual.FogOfWar.LineOfSightGeometry.Instance;
            return observer != null && subject != null && observer.View != null && subject.View != null &&
                geometry != null && observer.DistanceTo(subject) <= new Kingmaker.Utility.Feet(feet).Meters &&
                !observer.Descriptor.State.HasCondition(UnitCondition.Blindness) &&
                (!subject.Descriptor.State.HasCondition(UnitCondition.Invisible) || observer.Descriptor.IsSeeInvisibility) &&
                !subject.InStealthFor(observer.Group) &&
                Kingmaker.UnitLogic.Parts.UnitPartConcealment.Calculate(observer, subject, false) != Kingmaker.Enums.Concealment.Total &&
                !geometry.HasObstacle(observer.EyePosition, subject.EyePosition, 0);
        }
    }

    // The owner-approved noncombat route is private to this summon. It queues
    // an ordinary native Move activation; the native toggle owns upkeep,
    // performance exclusivity, effects and resource costs.
    [Serializable]
    public sealed class LillendNoncombat32 : BuffLogic,
        ITickEachRound, IUnitCombatHandler
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintActivatableAbility Competence;
        public BlueprintBuff Recipient;
        [NonSerialized] private UnitActivateAbility _pending;
        [NonSerialized] private string _selected;

        internal static LillendNoncombat32 Find(UnitEntityData unit)
        {
            return unit?.Buffs.Enumerable.Where(value => value.Active)
                .SelectMany(value => value.SelectComponents<LillendNoncombat32>())
                .Where(value => ReferenceEquals(value.OwningBlueprint, unit.Blueprint)).SingleOrDefault();
        }
        public override void OnTurnOn()
        {
            Buff.IsNotDispelable = true;
            Buff.IsFromSpell = false;
        }

        private ActivatableAbility Toggle => Owner?.ActivatableAbilities.Enumerable.SingleOrDefault(
            value => ReferenceEquals(value.Blueprint, Competence));
        internal static bool Eligible(UnitEntityData actor, UnitEntityData target)
        {
            return actor != null && target != null && !ReferenceEquals(actor, target) &&
                !actor.Destroyed && actor.IsInGame && !actor.Descriptor.State.IsDead && !actor.IsInCombat &&
                !target.Destroyed && target.IsInGame && target.Descriptor.State.IsConscious &&
                !target.Descriptor.State.HasCondition(UnitCondition.Sleeping) && !target.IsInCombat &&
                Game.Instance?.Player != null && Game.Instance.Player.Party.Contains(target) &&
                !actor.IsEnemy(target) && !target.IsEnemy(actor) &&
                actor.DistanceTo(target) <= new Kingmaker.Utility.Feet(30).Meters;
        }
        // Visible initial choices are bounded AI policy. Competence itself is
        // audible only: becoming blind or concealed must not end an already
        // selected ally's bonus. Use the native performance range mapping;
        // this engine exposes no separate hearing/deafness state.
        internal static bool Selectable(UnitEntityData actor, UnitEntityData target) =>
            Eligible(actor, target) && LillendPerformanceSight32.Sees(actor, target, 30);
        internal bool CanStart(UnitActivateAbility command)
        {
            var actor = Owner?.Unit;
            return ReferenceEquals(command, _pending) && actor != null && actor.IsBrainActive &&
                !actor.IsInCombat && Fact != null && Fact.Active &&
                Game.Instance.Player.Party.Any(value => value.UniqueId == _selected && Eligible(actor, value));
        }
        internal string TakeSelection()
        {
            if (_pending == null || !ReferenceEquals(_pending.Executor, Owner?.Unit) ||
                !ReferenceEquals(_pending.Ability?.Blueprint, Competence) || !_pending.IsStarted || !CanStart(_pending)) return null;
            string selected = _selected; _selected = null; _pending = null;
            return selected;
        }
        internal bool HasPendingSelection => _pending != null;
        public void OnNewRound()
        {
            var actor = Owner?.Unit; var toggle = Toggle;
            if (actor == null || !ReferenceEquals(actor.Blueprint, OwningBlueprint) ||
                Fact == null || !Fact.Active || !actor.IsBrainActive || actor.Destroyed || !actor.IsInGame ||
                actor.Descriptor.State.IsDead || actor.IsInCombat || toggle == null) return;
            if (_pending != null)
            {
                if (!_pending.IsFinished) return;
                _pending = null; _selected = null;
            }
            if (toggle.IsOn || toggle.IsRunning)
            {
                var state = actor.Buffs.Enumerable.Where(value => ReferenceEquals(value.Blueprint, Competence.Buff))
                    .SelectMany(value => value.SelectComponents<LillendCompetenceState32>()).SingleOrDefault();
                if (state != null && state.ShouldStop) toggle.IsOn = false;
                return;
            }
            if (!toggle.IsAvailable || !actor.Commands.Empty ||
                Owner.ActivatableAbilities.Enumerable.Any(value => value.IsOn || value.IsRunning)) return;
            var target = Game.Instance.Player.Party.Where(value => Selectable(actor, value) &&
                !value.Buffs.Enumerable.Any(buff => ReferenceEquals(buff.Blueprint, Recipient)))
                .OrderBy(value => actor.DistanceTo(value)).ThenBy(value => value.UniqueId, StringComparer.Ordinal).FirstOrDefault();
            if (target == null) return;
            _selected = target.UniqueId;
            _pending = new UnitActivateAbility(toggle);
            actor.Commands.Run(_pending);
        }
        public void HandleUnitJoinCombat(UnitEntityData unit)
        {
            if (!ReferenceEquals(unit, Owner?.Unit)) return;
            StopOwned();
        }
        public override void OnTurnOff() { StopOwned(); }
        private void StopOwned()
        {
            if (_pending != null && !_pending.IsFinished) _pending.Interrupt(true);
            _pending = null; _selected = null;
            var toggle = Toggle;
            if (toggle != null && (toggle.IsOn || toggle.IsRunning)) toggle.IsOn = false;
        }
        public void HandleUnitLeaveCombat(UnitEntityData unit) { }
    }

    // Selection belongs to this exact native performance carrier. Loading
    // restores the same recipient; area reconstruction never makes a choice.
    [Serializable]
    public sealed class LillendCompetenceState32 : BuffLogic
    {
        public BlueprintBuff Recipient;
        [JsonProperty] private string _recipientId;
        [NonSerialized] private bool _ready;
        public override void OnFactActivate()
        {
            var dispatcher = LillendNoncombat32.Find(Owner.Unit);
            _ready = dispatcher != null && dispatcher.HasPendingSelection;
            _recipientId = dispatcher?.TakeSelection();
        }
        public override void PostLoad()
        {
            base.PostLoad();
            _ready = true;
        }
        internal bool Selected(UnitEntityData unit) => _ready && unit != null && unit.UniqueId == _recipientId;
        internal bool ShouldStop => _ready && !Game.Instance.Player.Party.Any(unit =>
            Selected(unit) && LillendNoncombat32.Eligible(Owner.Unit, unit));
        internal void StopIfIneligible()
        {
            if (!ShouldStop) return;
            var toggle = Owner.ActivatableAbilities.Enumerable.SingleOrDefault(value =>
                ReferenceEquals(value.Blueprint.Buff, Buff.Blueprint));
            if (toggle != null) toggle.IsOn = false;
        }
        internal static LillendCompetenceState32 Find(MechanicsContext context, BlueprintBuff carrier)
        {
            var caster = context?.MaybeCaster;
            if (caster == null) return null;
            return caster.Buffs.Enumerable.Where(value => value.Active && ReferenceEquals(value.Blueprint, carrier) &&
                ElementalNereidAuraState.IncludesContext(context, value.Context))
                .SelectMany(value => value.SelectComponents<LillendCompetenceState32>()).SingleOrDefault();
        }
        public override void OnTurnOff()
        {
            if (Game.Instance?.State?.Units == null) return;
            foreach (var unit in Game.Instance.State.Units.All.ToArray())
                foreach (var grant in unit.Buffs.Enumerable.Where(value => ReferenceEquals(value.Blueprint, Recipient) &&
                    ElementalNereidAuraState.IncludesContext(value.Context, Buff.Context)).ToArray()) grant.Remove();
        }
    }

    [Serializable]
    public sealed class LillendCompetenceArea32 : AbilityAreaEffectLogic
    {
        public BlueprintAbilityAreaEffect OwningArea;
        public BlueprintBuff Carrier;
        public BlueprintBuff Recipient;
        private void Reconcile(MechanicsContext context, AreaEffectEntityData area, UnitEntityData unit)
        {
            if (unit == null || !ReferenceEquals(area?.Blueprint, OwningArea)) return;
            var state = LillendCompetenceState32.Find(context, Carrier);
            if (area.IsEnded || state == null || !state.Selected(unit) ||
                !LillendNoncombat32.Eligible(context.MaybeCaster, unit))
            { OnUnitExit(context, area, unit); return; }
            if (unit.Buffs.Enumerable.Any(value => ReferenceEquals(value.Blueprint, Recipient) && value.SourceAreaEffectId == area.UniqueId)) return;
            var grant = unit.Descriptor.AddBuff(Recipient, context, null);
            if (grant != null) { grant.SourceAreaEffectId = area.UniqueId; grant.IsNotDispelable = true; }
        }
        protected override void OnUnitEnter(MechanicsContext context, AreaEffectEntityData area, UnitEntityData unit)
        { Reconcile(context, area, unit); }
        protected override void OnUnitMove(MechanicsContext context, AreaEffectEntityData area, UnitEntityData unit)
        { Reconcile(context, area, unit); }
        protected override void OnTick(MechanicsContext context, AreaEffectEntityData area)
        {
            var state = LillendCompetenceState32.Find(context, Carrier);
            state?.StopIfIneligible();
            foreach (var unit in area.UnitsInside.ToArray()) Reconcile(context, area, unit);
        }
        protected override void OnUnitExit(MechanicsContext context, AreaEffectEntityData area, UnitEntityData unit)
        {
            if (unit == null || !ReferenceEquals(area?.Blueprint, OwningArea)) return;
            foreach (var grant in unit.Buffs.Enumerable.Where(value => ReferenceEquals(value.Blueprint, Recipient) &&
                value.SourceAreaEffectId == area.UniqueId).ToArray()) grant.Remove();
        }
    }
}
