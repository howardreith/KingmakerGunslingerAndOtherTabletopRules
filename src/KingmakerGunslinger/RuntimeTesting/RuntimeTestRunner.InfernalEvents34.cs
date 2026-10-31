using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Subscribed only inside the bounded disposable Sprint34 cell. Record
        // all recipients; filtering to the intended patient would conceal a
        // dispatch error. This observer never changes a rule or game state.
        private sealed class InfernalEvents34 : IDisposable, IUnitCommandStartHandler,
            IUnitCommandActHandler, IUnitCommandEndHandler,
            IUnitMakeOffensiveActionGlobalHandler,
            IGlobalRulebookHandler<RuleAttackWithWeapon>, IGlobalRulebookHandler<RuleAttackRoll>,
            IGlobalRulebookHandler<RuleDealDamage>, IGlobalRulebookHandler<RuleDealStatDamage>,
            IGlobalRulebookHandler<RuleHealDamage>, IGlobalRulebookHandler<RuleSavingThrow>,
            IGlobalRulebookHandler<RuleApplyBuff>, IGlobalRulebookHandler<RuleSpellResistanceCheck>
        {
            internal readonly JArray Rows = new JArray();
            private readonly Dictionary<RuleHealDamage, JObject> heals = new Dictionary<RuleHealDamage, JObject>();
            private readonly Dictionary<RuleDealDamage, JObject> damage = new Dictionary<RuleDealDamage, JObject>();
            internal InfernalEvents34() { EventBus.Subscribe(this); }
            public void Dispose() { EventBus.Unsubscribe(this); }
            private static int? ReferenceId(object value)
            { return value == null ? (int?)null : RuntimeHelpers.GetHashCode(value); }
            private JObject Row(string kind, UnitEntityData actor, UnitEntityData target)
            {
                var row = new JObject { ["event"] = kind, ["owner"] = actor?.UniqueId,
                    ["target"] = target?.UniqueId, ["gameSeconds"] = Game.Instance.TimeController.GameTime.TotalSeconds };
                Rows.Add(row); return row;
            }
            private static JArray Buffs(UnitEntityData unit)
            {
                return new JArray(unit.Buffs.Enumerable.Select(buff => new JObject {
                    ["guid"] = buff.Blueprint.AssetGuid, ["name"] = buff.Blueprint.name,
                    ["source"] = buff.Context?.MaybeCaster?.UniqueId,
                    ["active"] = buff.IsTurnedOn, ["endTime"] = buff.EndTime.ToString() }));
            }
            private void Command(UnitCommand command, string phase)
            {
                var row = Row(phase, command.Executor, command.TargetUnit);
                row["native"] = Sprint27AiCommand(command);
                row["commandReference"] = RuntimeHelpers.GetHashCode(command);
                row["ability"] = (command as UnitUseAbility)?.Spell?.Blueprint?.AssetGuid;
                row["nativeCommandType"] = ReadExpandedSummoningOptionalMember(command, "Type")?.ToString();
                row["ignoreCooldown"] = command.IsIgnoreCooldown;
                if (phase == "command-end" && command.Result == UnitCommand.ResultType.Interrupt)
                    row["interruptionCaller"] = Environment.StackTrace;
                row["turnBased"] = TurnBased.Controllers.CombatController.IsInTurnBasedCombat();
                if ((bool)row["turnBased"])
                    row["turn"] = Sprint22TurnState(command.Executor, command.TargetUnit);
                var attack = command as UnitAttack;
                if (attack != null) row["plan"] = new JArray(attack.AllAttacks.Select(info => new JObject {
                    ["weapon"] = info.Hand?.MaybeWeapon?.Blueprint?.AssetGuid,
                    ["itemReference"] = ReferenceId(info.Hand?.MaybeWeapon),
                    ["penalty"] = info.AttackBonusPenalty, ["number"] = info.AttackNumber }));
            }
            public void HandleUnitCommandDidStart(UnitCommand command) { Command(command, "command-start"); }
            public void HandleUnitCommandDidAct(UnitCommand command) { Command(command, "command-act"); }
            public void HandleUnitCommandDidEnd(UnitCommand command) { Command(command, "command-end"); }
            public void HandleUnitMakeOffensiveAction(UnitEntityData unit,
                UnitEntityData target)
            {
                var row = Row("offensive-action-global", unit, target);
                row["ownerIsEnemy"] = unit != null && target != null &&
                    unit.IsEnemy(target);
                row["ownerIsInGame"] = unit?.IsInGame;
                row["targetIsInGame"] = target?.IsInGame;
            }
            public void OnEventAboutToTrigger(RuleSpellResistanceCheck evt) { }
            public void OnEventDidTrigger(RuleSpellResistanceCheck evt)
            {
                var row = Row("spell-resistance", evt.Initiator, evt.Target);
                row["ability"] = evt.Ability?.AssetGuid; row["contextAbility"] = evt.Context?.SourceAbility?.AssetGuid;
                row["sr"] = evt.SpellResistance; row["penetration"] = evt.SpellPenetration;
                row["rollRequired"] = evt.HasResistanceRoll; row["d20"] = evt.HasResistanceRoll ? (JToken)new JValue(evt.Roll.Value) : JValue.CreateNull();
                row["immune"] = evt.TargetIsImmune; row["resisted"] = evt.IsSpellResisted;
            }
            public void OnEventAboutToTrigger(RuleAttackRoll evt) { }
            public void OnEventDidTrigger(RuleAttackRoll evt)
            {
                var row = Row("attack-roll", evt.Initiator, evt.Target);
                row["attackType"] = evt.AttackType.ToString(); row["hit"] = evt.IsHit;
                row["d20"] = evt.Roll.Value; row["bonus"] = evt.AttackBonus; row["ac"] = evt.TargetAC;
                row["autoHit"] = evt.AutoHit; row["ability"] = evt.Reason?.Ability?.Blueprint?.AssetGuid;
                row["contextAbility"] = evt.Reason?.Context?.SourceAbility?.AssetGuid;
            }
            public void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }
            public void OnEventDidTrigger(RuleAttackWithWeapon evt)
            {
                var row = Row("attack", evt.Initiator, evt.Target);
                row["weapon"] = evt.Weapon?.Blueprint?.AssetGuid; row["itemReference"] = ReferenceId(evt.Weapon);
                row["hit"] = evt.AttackRoll?.IsHit; row["hpDamage"] = evt.MeleeDamage?.Damage;
                row["rangeFeet"] = evt.Weapon?.AttackRange.Value;
                row["weaponDamage"] = evt.AttackRoll?.WeaponStats == null ? null : new JArray(evt.AttackRoll.WeaponStats.DamageDescription.Select(value =>
                    new JObject { ["dice"] = value.Dice.ToString(), ["bonus"] = value.Bonus }));
                row["damagePackets"] = evt.MeleeDamage == null ? null : new JArray(evt.MeleeDamage.DamageBundle.Select(value =>
                    new JObject { ["type"] = value.GetType().Name, ["dice"] = value.Dice.ToString(), ["bonus"] = value.Bonus,
                        ["criticalModifier"] = ReadExpandedSummoningOptionalMember(value, "CriticalModifier")?.ToString() }));
                row["attackCommand"] = evt.Initiator.Commands.Attack == null ? (int?)null :
                    RuntimeHelpers.GetHashCode(evt.Initiator.Commands.Attack);
            }
            public void OnEventAboutToTrigger(RuleDealDamage evt)
            {
                var row = Row("damage", evt.Initiator, evt.Target);
                row["before"] = evt.Target.Damage; damage.Add(evt, row);
            }
            public void OnEventDidTrigger(RuleDealDamage evt)
            {
                JObject row;
                if (!damage.TryGetValue(evt, out row)) return;
                row["after"] = evt.Target.Damage; row["actual"] = evt.Damage;
                row["weapon"] = evt.DamageBundle.Weapon?.Blueprint?.AssetGuid;
                row["hasAttackRoll"] = evt.AttackRoll != null;
                row["packets"] = new JArray(evt.DamageBundle.Select(value => new JObject {
                    ["type"] = value.GetType().Name, ["dice"] = value.Dice.ToString(), ["bonus"] = value.Bonus }));
                row["source"] = evt.Reason?.Context?.MaybeCaster?.UniqueId;
                row["ability"] = (evt.SourceAbility ?? evt.Reason?.Ability?.Blueprint ?? evt.Reason?.Context?.SourceAbility)?.AssetGuid;
                row["contextAbility"] = evt.Reason?.Context?.SourceAbility?.AssetGuid;
            }
            public void OnEventAboutToTrigger(RuleHealDamage evt)
            {
                var row = Row("heal", evt.Initiator, evt.Target);
                row["before"] = evt.Target.Damage; row["buffsBefore"] = Buffs(evt.Target);
                row["ability"] = evt.Reason?.Ability?.Blueprint?.AssetGuid;
                row["contextAbility"] = evt.Reason?.Context?.SourceAbility?.AssetGuid;
                heals.Add(evt, row);
            }
            public void OnEventDidTrigger(RuleHealDamage evt)
            {
                JObject row;
                if (!heals.TryGetValue(evt, out row)) return;
                row["after"] = evt.Target.Damage; row["actual"] = evt.Value;
                row["buffsAtDidTrigger"] = Buffs(evt.Target);
                var check = InfernalWound34.Observe(evt);
                if (check != null) row["woundCheck"] = new JObject { ["spell"] = check.Spell,
                    ["casterLevel"] = check.CasterLevel, ["d20"] = check.Roll, ["passed"] = check.Passed };
            }
            public void OnEventAboutToTrigger(RuleDealStatDamage evt) { }
            public void OnEventDidTrigger(RuleDealStatDamage evt)
            {
                var row = Row("stat-damage", evt.Initiator, evt.Target);
                row["stat"] = evt.Stat?.Type.ToString(); row["damage"] = evt.Damage;
                row["dice"] = evt.Dices.ToString(); row["bonus"] = evt.Bonus;
                row["fact"] = evt.Reason?.Fact?.Blueprint?.AssetGuid; row["source"] = evt.Reason?.Caster?.UniqueId;
            }
            public void OnEventAboutToTrigger(RuleSavingThrow evt) { }
            public void OnEventDidTrigger(RuleSavingThrow evt)
            {
                var row = Row("save", evt.Reason?.Context?.MaybeCaster, evt.Initiator);
                row["type"] = evt.StatType.ToString(); row["dc"] = evt.DifficultyClass;
                row["d20"] = evt.D20.Value; row["passed"] = evt.IsPassed;
            }
            public void OnEventAboutToTrigger(RuleApplyBuff evt) { }
            public void OnEventDidTrigger(RuleApplyBuff evt)
            {
                var row = Row("effect-application", evt.Reason?.Context?.MaybeCaster, evt.Initiator);
                row["blueprint"] = evt.Blueprint?.AssetGuid; row["resultingBuffs"] = Buffs(evt.Initiator);
            }
        }

        // Read-only exact-owner listener for diagnosing the native actor-local
        // offensive-action boundary. It records the same event that
        // BuffInvisibility consumes and never changes rule or fact state.
        private sealed class InfernalOffensiveOwner35 : IDisposable,
            IUnitMakeOffensiveActionHandler, IUnitSubscriber
        {
            private readonly UnitEntityData owner;
            private readonly JArray rows;

            internal InfernalOffensiveOwner35(UnitEntityData owner,
                JArray rows)
            {
                this.owner = owner ?? throw new ArgumentNullException(
                    nameof(owner));
                this.rows = rows ?? throw new ArgumentNullException(
                    nameof(rows));
                EventBus.Subscribe(this);
            }

            public UnitEntityData GetSubscribingUnit() { return owner; }

            public void HandleUnitMakeOffensiveAction(UnitEntityData target)
            {
                rows.Add(new JObject {
                    ["event"] = "offensive-action-owner",
                    ["owner"] = owner.UniqueId,
                    ["target"] = target?.UniqueId,
                    ["gameSeconds"] = Game.Instance.TimeController.GameTime
                        .TotalSeconds,
                    ["ownerIsEnemy"] = target != null &&
                        owner.IsEnemy(target),
                    ["ownerIsInGame"] = owner.IsInGame,
                    ["targetIsInGame"] = target?.IsInGame
                });
            }

            public void Dispose() { EventBus.Unsubscribe(this); }
        }
    }
}
