using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private static Sprint16LifecycleDamageWitness _sprint16LifecycleDamageWitness;

        // Read-only native setter witnesses. Never change an argument, return
        // value, context, health, command, buff, relationship or native rule.
        private static void BeforeSprint16HealthMutation(object __instance, MethodBase __originalMethod, out JObject __state)
        {
            JObject snapshot = null;
            var witness = _sprint16LifecycleDamageWitness;
            if (witness != null) witness.Guard(() => snapshot = witness.BeforeMutation(__instance, __originalMethod));
            __state = snapshot;
        }

        private static void AfterSprint16HealthMutation(object __instance, JObject __state)
        {
            var witness = _sprint16LifecycleDamageWitness;
            if (witness != null && __state != null) witness.Guard(() => witness.AfterMutation(__instance, __state));
        }

        private sealed class Sprint16LifecycleDamageWitness : IDisposable,
            IGlobalRulebookHandler<RuleDealDamage>, IGlobalRulebookHandler<RuleHealDamage>
        {
            private readonly HarmonyInstance _harmony;
            private readonly UnitEntityData _owner, _target;
            private readonly Summoning.SummonGrabComponent _grab;
            private readonly HashSet<UnitEntityData> _fixtureUnits;
            private readonly List<MethodInfo> _patched = new List<MethodInfo>();
            private readonly Dictionary<RulebookEvent, JObject> _rules = new Dictionary<RulebookEvent, JObject>();
            private readonly MethodInfo _prefix, _postfix;
            private readonly JArray _events = new JArray(), _errors = new JArray();
            private readonly CrocodilianDamageAttributionLedger _ledger;
            private readonly JObject _before;
            private JObject _after;
            private int _lastHp;
            private bool _subscribed, _closed;

            internal Sprint16LifecycleDamageWitness(HarmonyInstance harmony, UnitEntityData owner,
                UnitEntityData target, Summoning.SummonGrabComponent grab, IEnumerable<UnitEntityData> fixtureUnits)
            {
                if (_sprint16LifecycleDamageWitness != null) throw new InvalidOperationException("Damage witness already owned.");
                _harmony = harmony; _owner = owner; _target = target; _grab = grab;
                _fixtureUnits = new HashSet<UnitEntityData>(fixtureUnits);
                _ledger = new CrocodilianDamageAttributionLedger(target.Damage);
                _before = Snapshot(); _lastHp = target.Descriptor.Stats.HitPoints.ModifiedValue;
                _prefix = typeof(RuntimeTestRunner).GetMethod("BeforeSprint16HealthMutation", BindingFlags.Static | BindingFlags.NonPublic);
                _postfix = typeof(RuntimeTestRunner).GetMethod("AfterSprint16HealthMutation", BindingFlags.Static | BindingFlags.NonPublic);
                try
                {
                    _sprint16LifecycleDamageWitness = this;
                    Patch(typeof(UnitDescriptor).GetProperty("Damage").GetSetMethod(true));
                    foreach (string property in new[] { "BaseValue", "ModifiedValue", "ModifiedValueRaw" })
                        Patch(typeof(ModifiableValue).GetProperty(property).GetSetMethod(true));
                    EventBus.Subscribe(this); _subscribed = true;
                }
                catch { Dispose(); throw; }
            }

            private void Patch(MethodInfo method)
            {
                if (method == null || _prefix == null || _postfix == null)
                    throw new InvalidOperationException("Inspected native health mutation seam is absent.");
                _patched.Add(method);
                _harmony.Patch(method, new HarmonyMethod(_prefix), new HarmonyMethod(_postfix), null);
            }

            internal void Guard(Action observation)
            {
                try { observation(); }
                catch (Exception ex) { _errors.Add(ex.GetType().FullName + ":" + ex.Message); }
            }

            private bool BoundaryReached
            { get { return _owner.Destroyed || !_owner.Descriptor.HasFact(Kingmaker.Blueprints.Root.BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff); } }

            private JObject Snapshot()
            {
                return new JObject {
                    ["frame"] = Time.frameCount, ["gameTime"] = Game.Instance.Player.GameTime.ToString(),
                    ["owner"] = Unit(_owner), ["prey"] = Unit(_target),
                    ["ownerAlive"] = !_owner.Destroyed && !_owner.Descriptor.State.IsDead,
                    ["ownerDead"] = _owner.Descriptor.State.IsDead, ["ownerDestroyed"] = _owner.Destroyed,
                    ["expiryBoundaryReached"] = BoundaryReached,
                    ["relationshipReleased"] = Sprint16RelationshipReleased(_owner, _target, _grab),
                    ["damage"] = _target.Damage,
                    ["hpBase"] = _target.Descriptor.Stats.HitPoints.BaseValue,
                    ["hpModified"] = _target.Descriptor.Stats.HitPoints.ModifiedValue,
                    ["temporaryHp"] = _target.Descriptor.Stats.TemporaryHitPoints.ModifiedValue,
                    ["constitution"] = _target.Descriptor.Stats.Constitution.ModifiedValue,
                    ["nonlethal"] = _target.DamageNonLethal,
                    ["buffs"] = new JArray(_target.Descriptor.Buffs.RawFacts.OfType<Buff>().Select(buff => new JObject {
                        ["instance"] = RuntimeHelpers.GetHashCode(buff), ["blueprint"] = Blueprint(buff.Blueprint),
                        ["harmful"] = buff.Blueprint.Harmful, ["context"] = Context(buff.Context),
                        ["components"] = new JArray(buff.Blueprint.ComponentsArray.Select(value => value == null ? null : value.GetType().FullName)) })),
                    ["conditions"] = new JArray(Enum.GetValues(typeof(UnitCondition)).Cast<UnitCondition>().Distinct()
                        .Where(value => _target.Descriptor.State.HasCondition(value)).Select(value => value.ToString()))
                };
            }

            private static JObject Unit(UnitEntityData unit)
            { return unit == null ? null : new JObject { ["id"] = unit.UniqueId, ["blueprint"] = Blueprint(unit.Blueprint), ["destroyed"] = unit.Destroyed }; }

            private static JObject Blueprint(BlueprintScriptableObject blueprint)
            { return blueprint == null ? null : new JObject { ["guid"] = blueprint.AssetGuid, ["name"] = blueprint.name, ["type"] = blueprint.GetType().FullName }; }

            private static JObject Context(MechanicsContext context)
            {
                if (context == null) return null;
                return new JObject { ["instance"] = RuntimeHelpers.GetHashCode(context),
                    ["type"] = context.GetType().FullName, ["blueprint"] = Blueprint(context.AssociatedBlueprint),
                    ["caster"] = Unit(context.MaybeCaster), ["owner"] = Unit(context.MaybeOwner),
                    ["ability"] = Blueprint(context.SourceAbility),
                    ["parentInstance"] = context.ParentContext == null ? (int?)null : RuntimeHelpers.GetHashCode(context.ParentContext),
                    ["parentBlueprint"] = context.ParentContext == null ? null : Blueprint(context.ParentContext.AssociatedBlueprint) };
            }

            private static JObject Reason(RuleReason reason)
            {
                if (reason == null) return null;
                return new JObject { ["context"] = Context(reason.Context), ["fact"] = reason.Fact == null ? null : Blueprint(reason.Fact.Blueprint),
                    ["factInstance"] = reason.Fact == null ? (int?)null : RuntimeHelpers.GetHashCode(reason.Fact),
                    ["buff"] = reason.Fact is Buff ? Blueprint(((Buff)reason.Fact).Blueprint) : null,
                    ["ability"] = reason.Ability == null ? null : Blueprint(reason.Ability.Blueprint),
                    ["caster"] = Unit(reason.Caster), ["sourceUnit"] = Unit(reason.SourceUnit),
                    ["item"] = reason.Item == null ? null : Blueprint(reason.Item.Blueprint),
                    ["parentRule"] = reason.Rule == null ? null : reason.Rule.GetType().FullName };
            }

            private string Source(RulebookEvent rule)
            {
                var reason = rule == null ? null : rule.Reason;
                UnitEntityData[] sources = { rule == null ? null : rule.Initiator, reason == null ? null : reason.Caster,
                    reason == null ? null : reason.SourceUnit, reason == null || reason.Context == null ? null : reason.Context.MaybeCaster };
                return CrocodilianDamageAttributionPolicy.Source(sources.Any(value => ReferenceEquals(value, _owner)),
                    sources.Any(value => ReferenceEquals(value, _target)), sources.Any(value => value != null && _fixtureUnits.Contains(value)),
                    sources.Any(value => value != null), reason != null && reason.Fact is Buff);
            }

            private JObject CurrentRule()
            {
                var context = Rulebook.CurrentContext;
                var rule = context == null ? null : context.CurrentEvent;
                return new JObject { ["type"] = rule == null ? null : rule.GetType().FullName,
                    ["initiator"] = rule == null ? null : Unit(rule.Initiator), ["reason"] = rule == null ? null : Reason(rule.Reason),
                    ["classification"] = Source(rule), ["parents"] = context == null ? new JArray() :
                        new JArray(context.EventStack.Select(value => new JObject { ["type"] = value.GetType().FullName,
                            ["initiator"] = Unit(value.Initiator), ["reason"] = Reason(value.Reason) })) };
            }

            private static JArray NativeStack()
            {
                return new JArray((new System.Diagnostics.StackTrace(false).GetFrames() ?? new System.Diagnostics.StackFrame[0])
                    .Take(32).Select(value => { var method = value.GetMethod(); return method == null ? null :
                        (method.DeclaringType == null ? "" : method.DeclaringType.FullName + ".") + method.Name; }));
            }

            internal JObject BeforeMutation(object instance, MethodBase method)
            {
                bool tracked = CrocodilianDamageAttributionPolicy.IsExactTarget(_target.Descriptor, instance) ||
                    CrocodilianDamageAttributionPolicy.IsExactTarget(_target.Descriptor.Stats.HitPoints, instance) ||
                    CrocodilianDamageAttributionPolicy.IsExactTarget(_target.Descriptor.Stats.TemporaryHitPoints, instance) ||
                    CrocodilianDamageAttributionPolicy.IsExactTarget(_target.Descriptor.Stats.Constitution, instance);
                if (!tracked) return null;
                return new JObject { ["kind"] = "native-health-mutation", ["seam"] = method.DeclaringType.FullName + "." + method.Name,
                    ["stat"] = instance is ModifiableValue ? ((ModifiableValue)instance).Type.ToString() : "aggregate-damage",
                    ["before"] = Snapshot(), ["rule"] = CurrentRule(), ["nativeStack"] = NativeStack() };
            }

            internal void AfterMutation(object instance, JObject row)
            {
                JObject after = Snapshot(); row["after"] = after;
                if ((int)row["before"]["damage"] != (int)after["damage"] ||
                    (int)row["before"]["hpModified"] != (int)after["hpModified"] ||
                    (int)row["before"]["temporaryHp"] != (int)after["temporaryHp"])
                {
                    _ledger.Record((int)row["before"]["damage"], (int)after["damage"], (string)row["rule"]["classification"], BoundaryReached);
                    _events.Add(row); _lastHp = (int)after["hpModified"];
                }
            }

            private void BeginRule(RulebookEvent rule, UnitEntityData target)
            {
                if (!CrocodilianDamageAttributionPolicy.IsExactTarget(_target, target)) return;
                JObject row = new JObject { ["kind"] = rule.GetType().Name, ["instance"] = RuntimeHelpers.GetHashCode(rule),
                    ["before"] = Snapshot(), ["initiator"] = Unit(rule.Initiator), ["target"] = Unit(target),
                    ["reason"] = Reason(rule.Reason), ["ruleParents"] = CurrentRule(), ["sourceClassification"] = Source(rule), ["nativeStack"] = NativeStack() };
                _rules[rule] = row; _events.Add(row);
            }

            private void EndRule(RulebookEvent rule)
            {
                JObject row;
                if (!_rules.TryGetValue(rule, out row)) return;
                row["after"] = Snapshot(); row["reasonAfter"] = Reason(rule.Reason);
                if (_ledger.LastDamage != _target.Damage)
                    _ledger.Record(_ledger.LastDamage, _target.Damage, Source(rule), BoundaryReached);
                _lastHp = _target.Descriptor.Stats.HitPoints.ModifiedValue;
                var damage = rule as RuleDealDamage;
                if (damage != null)
                {
                    row["damage"] = Sprint16DamageEvent(damage);
                    row["bundleWeapon"] = damage.DamageBundle.Weapon == null ? null : Blueprint(damage.DamageBundle.Weapon.Blueprint);
                    row["sourceAbility"] = Blueprint(damage.SourceAbility); row["sourceArea"] = Blueprint(damage.SourceArea);
                    row["isDot"] = damage.IsDot; row["isFake"] = damage.IsFake;
                    row["results"] = new JArray((damage.ResultDamage ?? new List<DamageValue>()).Select(value => new JObject {
                        ["component"] = Sprint16DamageLine(value.Source), ["rolled"] = value.RolledValue,
                        ["withoutReduction"] = value.ValueWithoutReduction, ["final"] = value.FinalValue, ["reduction"] = value.Reduction }));
                    row["attackWeapon"] = damage.AttackRoll == null || damage.AttackRoll.Weapon == null ? null : Blueprint(damage.AttackRoll.Weapon.Blueprint);
                }
                var healing = rule as RuleHealDamage;
                if (healing != null) row["healing"] = new JObject { ["dice"] = healing.HealFormula.ToString(), ["bonus"] = healing.Bonus, ["value"] = healing.Value };
            }

            public void OnEventAboutToTrigger(RuleDealDamage rule) { Guard(() => BeginRule(rule, rule.Target)); }
            public void OnEventDidTrigger(RuleDealDamage rule) { Guard(() => EndRule(rule)); }
            public void OnEventAboutToTrigger(RuleHealDamage rule) { Guard(() => BeginRule(rule, rule.Target)); }
            public void OnEventDidTrigger(RuleHealDamage rule) { Guard(() => EndRule(rule)); }

            internal void Poll()
            {
                Guard(() => {
                    if (_target.Damage != _ledger.LastDamage || _target.Descriptor.Stats.HitPoints.ModifiedValue != _lastHp)
                        _events.Add(new JObject { ["kind"] = "frame-mutation-without-observed-seam", ["lastDamage"] = _ledger.LastDamage,
                            ["after"] = Snapshot(), ["rule"] = CurrentRule() });
                    _ledger.Poll(_target.Damage, BoundaryReached); _lastHp = _target.Descriptor.Stats.HitPoints.ModifiedValue;
                });
            }

            internal JObject Report()
            {
                return new JObject { ["schemaVersion"] = 1, ["scope"] = "request-owned crocodile-cooldown-expiry only",
                    ["readOnly"] = true, ["beforeExpiry"] = _before, ["afterWindow"] = _after,
                    ["mutationSeams"] = new JArray(_patched.Select(value => value.DeclaringType.FullName + "." + value.Name)),
                    ["events"] = _events, ["errors"] = _errors,
                    ["changes"] = new JArray(_ledger.Changes.Select(value => new JObject { ["before"] = value.Before,
                        ["after"] = value.After, ["source"] = value.Source, ["boundaryReached"] = value.BoundaryReached })),
                    ["observerRemoved"] = _closed && _sprint16LifecycleDamageWitness == null && _errors.Count == 0 };
            }

            public void Dispose()
            {
                if (_closed) return;
                Poll(); Guard(() => _after = Snapshot());
                _sprint16LifecycleDamageWitness = null;
                try { if (_subscribed) EventBus.Unsubscribe(this); }
                finally
                {
                    foreach (MethodInfo method in _patched)
                    { Guard(() => _harmony.Unpatch(method, _prefix)); Guard(() => _harmony.Unpatch(method, _postfix)); }
                    _closed = true;
                }
            }
        }
    }
}
