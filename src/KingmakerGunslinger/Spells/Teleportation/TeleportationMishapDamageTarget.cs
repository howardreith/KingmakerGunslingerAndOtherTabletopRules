using System;
using System.Linq;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;

namespace KingmakerGunslinger.Spells.Teleportation
{
    // The native controller has no instance state. Expose only its protected
    // per-unit boundary; do not tick, register or replace a global controller.
    internal sealed class TeleportationMishapDamageTarget : UnitLifeController, ITeleportMishapDamageTarget
    {
        private readonly UnitEntityData _target;
        private readonly TeleportationNativeCastSource _source;
        internal RuleDealDamage Rule { get; private set; }
        internal TeleportationMishapDamageTarget(UnitEntityData target, TeleportationNativeCastSource source)
        { _target = target; _source = source; }
        public bool Living { get { return !_target.Descriptor.State.IsDead; } }
        internal static bool CanApply(TeleportationTravelers travelers)
        { return travelers.Units.All(unit => unit.Descriptor.State.IsDead || unit.View != null); }
        public void SynchronizeNativeLifeState()
        {
            if (_target.View == null) throw new InvalidOperationException("Native life-state update requires the traveling unit's view.");
            if (ShouldTickOnUnit(_target)) TickOnUnit(_target);
        }
        public void ApplyCanonicalDamage(int amount)
        {
            Rule = new RuleDealDamage(_source.Caster, _target,
                new DamageBundle(new DirectDamage(new DiceFormula(0, DiceType.D10), amount))) {
                SourceAbility = _source.Ability.Blueprint
            };
            Rulebook.Trigger(Rule);
        }
    }
}
