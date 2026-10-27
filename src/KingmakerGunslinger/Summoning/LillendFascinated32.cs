using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    // Private adaptation of the qualified Nereid command/threat observer.
    // Only source lookup, printed visibility, and the bardic exception differ.
    [Serializable]
    public sealed class LillendFascinated32 : BuffLogic, IUnitCommandStartHandler,
        IUnitActiveEquipmentSetHandler, ITargetRulebookHandler<RuleDealDamage>,
        ITargetRulebookHandler<RuleAttackWithWeapon>
    {
        public BlueprintBuff Aura;
        [NonSerialized] private Dictionary<string, Vector3> _positions;
        [NonSerialized] private Dictionary<string, bool> _drawnWeapons;
        private LillendFascinateState32 Source { get { return LillendFascinateState32.Find(Buff.Context, Aura, Buff.SourceAreaEffectId); } }

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
            return LillendPerformanceSight32.Sees(Owner.Unit, unit, 90);
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
            var source = Source;
            // This exact performer's bardic Suggestion does not interrupt any
            // subject of this performance. Ordinary/provider book spells and
            // another performer's command remain obvious threats.
            if (use != null && source != null && source.Active &&
                ReferenceEquals(command.Executor, Buff.Context?.MaybeCaster) &&
                ReferenceEquals(use.Spell?.Blueprint, source.BardicSuggestion) &&
                source.Owns(use.Target?.Unit)) return;
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
            if (Owner != null && Owner.IsTurnedOn)
            {
                var source = Source;
                if (source != null) source.Interrupt(Owner.Unit);
            }
        }
    }

}
