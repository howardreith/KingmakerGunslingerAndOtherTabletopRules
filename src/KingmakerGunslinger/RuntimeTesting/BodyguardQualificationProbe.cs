using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Request-local attack-linked saving-throw/condition rider. It deliberately
    /// uses RuleAttackRoll.Target, so its live recipient proves that the shared
    /// attack-result event is redirected before generic on-hit subscribers run.
    /// </summary>
    public sealed class BodyguardQualificationRiderComponent :
        RuleInitiatorLogicComponent<RuleAttackRoll>
    {
        [ThreadStatic] private static bool _enabled;
        [ThreadStatic] private static int _invocations;
        [ThreadStatic] private static UnitEntityData _target;
        [ThreadStatic] private static UnitEntityData _saveTarget;
        [ThreadStatic] private static bool _critical;
        public BlueprintBuff Rider;

        internal static void Reset(bool enabled)
        {
            _enabled = enabled;
            _invocations = 0;
            _target = null;
            _saveTarget = null;
            _critical = false;
        }

        internal static string Describe()
        {
            return "riderEnabled=" + _enabled + ";invocations=" +
                _invocations + ";target=" + (_target == null ? "<null>" :
                    _target.UniqueId) + ";saveTarget=" + (_saveTarget == null ?
                    "<null>" : _saveTarget.UniqueId) + ";critical=" +
                _critical;
        }

        public override void OnEventAboutToTrigger(RuleAttackRoll evt) { }

        public override void OnEventDidTrigger(RuleAttackRoll evt)
        {
            if (!_enabled || evt == null || !evt.IsHit || evt.Target == null ||
                Rider == null) return;
            _invocations++;
            _target = evt.Target;
            _critical = evt.IsCriticalConfirmed;
            var saving = new RuleSavingThrow(evt.Target,
                SavingThrowType.Fortitude, 100);
            Rulebook.Trigger(saving);
            _saveTarget = saving.Initiator;
            evt.Target.Descriptor.Buffs.AddBuff(Rider,
                Fact == null ? null : Fact.MaybeContext,
                TimeSpan.FromMinutes(1));
        }
    }

    /// <summary>Records finalized native damage recipients and packet types.</summary>
    public sealed class BodyguardQualificationDamageProbe :
        RuleTargetLogicComponent<RuleDealDamage>
    {
        [ThreadStatic] private static bool _enabled;
        [ThreadStatic] private static List<string> _events;

        internal static void Reset(bool enabled)
        {
            _enabled = enabled;
            _events = new List<string>();
        }

        internal static string[] Snapshot()
        { return _events == null ? new string[0] : _events.ToArray(); }

        public override void OnEventAboutToTrigger(RuleDealDamage evt) { }

        public override void OnEventDidTrigger(RuleDealDamage evt)
        {
            if (!_enabled || evt == null || evt.Target == null) return;
            if (_events == null) _events = new List<string>();
            _events.Add("target=" + evt.Target.UniqueId + ";damage=" +
                evt.Damage + ";beforeReduction=" + evt.DamageWithoutReduction +
                ";attack=" + (evt.AttackRoll == null ? "<none>" :
                    System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(
                        evt.AttackRoll).ToString()) + ";packets=" +
                string.Join(",", (evt.ResultDamage ??
                    new List<DamageValue>()).Select(value =>
                        (value.Source == null ? "<null>" :
                            value.Source.GetType().Name) + ":" +
                        value.FinalValue).ToArray()));
        }
    }
}
