using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Deeds;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Grit;
using KingmakerGunslinger.Misfires;
using KingmakerGunslinger.Rules;
using KingmakerGunslinger.Scatter;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// REP-02 / S4 T2: observations for the Up Close and Deadly component
    /// fixture. Everything here is a component test: the armed state is put
    /// on with AddBuff and the deed's attack handler is invoked by hand on a
    /// standalone RuleAttackRoll. It does not show the registered ability or
    /// the attack command; the loader-compat probe does that.
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        /// <summary>
        /// Every RuleDealDamage this attacker deals while it is subscribed,
        /// sorted into the deed's own packet and the rest, and the removal of
        /// the exact armed instance, each numbered in the order it happened.
        /// </summary>
        private sealed class UpCloseDamageCapture : IGlobalRulebookHandler<RuleDealDamage>, IUnitBuffHandler, IDisposable
        {
            private readonly UnitEntityData _attacker;
            private readonly Buff _armed;
            private bool _subscribed;
            private int _sequence;
            internal readonly JArray Deed = new JArray();
            internal readonly JArray Other = new JArray();
            internal int? RemovalSequence;
            internal double? RemovalGameSeconds;
            internal int? DeedDamageSequence;
            internal int OtherInstanceRemovals;

            internal UpCloseDamageCapture(UnitEntityData attacker, Buff armed)
            {
                _attacker = attacker;
                _armed = armed;
                EventBus.Subscribe(this);
                _subscribed = true;
            }

            internal int Mark() { return ++_sequence; }

            public void OnEventAboutToTrigger(RuleDealDamage evt) { }

            public void OnEventDidTrigger(RuleDealDamage evt)
            {
                if (evt == null || !ReferenceEquals(evt.Initiator, _attacker)) return;
                JObject row = DescribeDamageEvent(evt);
                int sequence = Mark();
                row["sequence"] = sequence;
                row["armedInstanceOnUnitAtThisEvent"] = _armed != null &&
                    _attacker.Descriptor.Buffs.RawFacts.Contains(_armed);
                if ((bool)row["deedShape"])
                {
                    Deed.Add(row);
                    if (!DeedDamageSequence.HasValue) DeedDamageSequence = sequence;
                }
                else Other.Add(row);
            }

            public void HandleBuffDidAdded(Buff buff) { }

            public void HandleBuffDidRemoved(Buff buff)
            {
                if (buff == null || _armed == null) return;
                if (ReferenceEquals(buff, _armed))
                {
                    RemovalSequence = Mark();
                    RemovalGameSeconds = Kingmaker.Game.Instance.TimeController.GameTime.TotalSeconds;
                }
                else if (ReferenceEquals(buff.Blueprint, _armed.Blueprint) &&
                    buff.Owner != null && ReferenceEquals(buff.Owner.Unit, _attacker))
                    OtherInstanceRemovals++;
            }

            public void Dispose()
            {
                if (!_subscribed) return;
                EventBus.Unsubscribe(this);
                _subscribed = false;
            }
        }

        /// <summary>
        /// One damage event as the engine resolved it. The deed's packet is
        /// recognised by its construction (no attack roll, precision
        /// discovery disabled, one d6 physical or fixed direct packet), not by
        /// the hit points it took.
        /// </summary>
        private static JObject DescribeDamageEvent(RuleDealDamage evt)
        {
            var row = new JObject
            {
                ["target"] = evt.Target == null ? null : evt.Target.CharacterName,
                ["hasAttackRoll"] = evt.AttackRoll != null,
                ["precisionDisabled"] = evt.DisablePrecisionDamage,
                ["applied"] = evt.Damage,
                ["beforeDifficulty"] = evt.DamageBeforeDifficulty,
                ["difficultyModifier"] = evt.DifficultyModifier
            };
            List<DamageValue> values = evt.ResultDamage;
            if (values == null || values.Count == 0)
            {
                row["kind"] = "none";
                row["deedShape"] = false;
                return row;
            }
            DamageValue value = values[0];
            BaseDamage source = value.Source;
            string kind = source is PhysicalDamage ? "physical" : source is DirectDamage ? "direct" : "other";
            row["packets"] = values.Count;
            row["kind"] = kind;
            row["diceRolls"] = source.Dice.Rolls;
            row["diceSides"] = source.Dice.Dice.Sides();
            row["bonus"] = source.Bonus;
            row["criticalModifier"] = source.CriticalModifier.HasValue ? (JToken)source.CriticalModifier.Value : JValue.CreateNull();
            row["rolled"] = value.RolledValue;
            row["final"] = value.FinalValue;
            row["deedShape"] = evt.AttackRoll == null && evt.DisablePrecisionDamage && values.Count == 1 &&
                ((kind == "physical" && source.Dice.Dice == DiceType.D6) ||
                 (kind == "direct" && source.Dice.Rolls == 0));
            return row;
        }

        private static JObject ReadUpCloseRoll(RuleAttackRoll roll)
        {
            if (roll == null) return new JObject { ["roll"] = "absent" };
            return new JObject
            {
                ["hit"] = roll.IsHit,
                ["result"] = roll.Result.ToString(),
                ["natural"] = (int)roll.Roll,
                ["attackBonus"] = roll.AttackBonus,
                ["targetArmorClass"] = roll.TargetAC,
                ["autoHit"] = roll.AutoHit,
                ["criticalRoll"] = roll.IsCriticalRoll,
                ["criticalConfirmed"] = roll.IsCriticalConfirmed,
                ["precisionImmune"] = roll.ImmuneToSneakAttack,
                ["completedNonMisfire"] = FirearmMisfireRuntime.WasCompletedNonMisfire(roll)
            };
        }

        private struct RecoveryMark
        {
            internal int Applied;
            internal int Ignored;
        }

        private static RecoveryMark MarkRecovery()
        {
            return new RecoveryMark
            {
                Applied = FirearmGritRecoveryRuntimeDiagnostics.CriticalApplied +
                    FirearmGritRecoveryRuntimeDiagnostics.KillingBlowApplied,
                Ignored = FirearmGritRecoveryRuntimeDiagnostics.Ignored
            };
        }

        /// <summary>
        /// Recovery since the mark. No recovery applied anywhere is a valid
        /// zero for this actor. Any applied recovery is unknown: the recovery
        /// diagnostics are global and do not name the unit they restored, so
        /// the amount cannot be assigned to this actor.
        /// </summary>
        private static void RestoredSince(RecoveryMark mark, JObject row)
        {
            int applied = FirearmGritRecoveryRuntimeDiagnostics.CriticalApplied +
                FirearmGritRecoveryRuntimeDiagnostics.KillingBlowApplied - mark.Applied;
            row["recoveryAppliedAnywhere"] = applied;
            if (applied == 0)
            {
                row["gritRestored"] = 0;
                row["gritRestoredAttribution"] = UpCloseAndDeadlyCaseVerdict.NoRecoveryApplied;
            }
            else
            {
                row["gritRestored"] = JValue.CreateNull();
                row["gritRestoredAttribution"] = "unattributed";
            }
        }

        /// <summary>
        /// Runs one hand-invoked delivery and records what the evaluator needs:
        /// the roll as resolved, the deed's damage events, grit with recovery
        /// separated, hit points, and the exact armed instance.
        /// </summary>
        private static JObject ObserveUpCloseDelivery(UnitEntityData attacker, UnitEntityData target,
            BlueprintAbilityResource grit, Buff marker, RuleAttackWithWeapon attack, string criticalSource,
            Action deliver)
        {
            var row = ReadUpCloseRoll(attack.AttackRoll);
            row["criticalSource"] = criticalSource;
            bool identified = marker != null && attacker.Descriptor.Buffs.RawFacts.Contains(marker);
            row["armedInstanceOnUnitBefore"] = identified;
            bool anyArmedBefore = marker != null && attacker.Descriptor.Buffs.RawFacts.OfType<Buff>()
                .Any(value => ReferenceEquals(value.Blueprint, marker.Blueprint));
            row["gritBefore"] = attacker.Descriptor.Resources.GetResourceAmount(grit);
            row["targetHitPointsBefore"] = target.HPLeft;
            double now = Kingmaker.Game.Instance.TimeController.GameTime.TotalSeconds;
            JToken deadline = identified && !marker.IsPermanent
                ? (JToken)(now + marker.TimeLeft.TotalSeconds) : JValue.CreateNull();
            RecoveryMark mark = MarkRecovery();
            using (var capture = new UpCloseDamageCapture(attacker, marker))
            {
                int start = capture.Mark();
                deliver();
                // Exact-reference presence immediately after the delivery,
                // before any fixture cleanup touches the instance.
                bool presentAtEnd = marker != null && attacker.Descriptor.Buffs.RawFacts.Contains(marker);
                int end = capture.Mark();
                row["deedPackets"] = capture.Deed;
                row["otherDamage"] = capture.Other;
                row["armedInstance"] = new JObject
                {
                    ["identified"] = identified,
                    ["presentBeforeShot"] = anyArmedBefore,
                    ["presentAtDeliveryEnd"] = presentAtEnd,
                    ["deliveryStartSequence"] = start,
                    ["deliveryEndSequence"] = end,
                    ["deedDamageSequence"] = capture.DeedDamageSequence.HasValue ? (JToken)capture.DeedDamageSequence.Value : JValue.CreateNull(),
                    ["removalSequence"] = capture.RemovalSequence.HasValue ? (JToken)capture.RemovalSequence.Value : JValue.CreateNull(),
                    ["removalGameSeconds"] = capture.RemovalGameSeconds.HasValue ? (JToken)capture.RemovalGameSeconds.Value : JValue.CreateNull(),
                    ["expiryDeadlineGameSeconds"] = deadline,
                    ["otherInstanceRemovals"] = capture.OtherInstanceRemovals
                };
            }
            int[] drawn = Archetypes.UpCloseAndDeadlyDeliveryRecords.MissRollFor(attack, attacker.UniqueId);
            row["missRoll"] = drawn == null ? JValue.CreateNull() : (JToken)new JObject { ["dice"] = new JArray(drawn) };
            RestoredSince(mark, row);
            row["recoveryIgnored"] = FirearmGritRecoveryRuntimeDiagnostics.Ignored - mark.Ignored;
            row["gritAfter"] = attacker.Descriptor.Resources.GetResourceAmount(grit);
            row["targetHitPointsAfter"] = target.HPLeft;
            row["targetDead"] = target.Descriptor.State.IsDead;
            // The final read is recorded but never decides consumption.
            row["armedInstanceAbsentAtFinalRead"] = marker == null ? JValue.CreateNull() :
                (JToken)!attacker.Descriptor.Buffs.RawFacts.Contains(marker);
            return row;
        }

        /// <summary>
        /// A standalone firearm attack roll whose outcome is set before it
        /// resolves: one loaded round, the product's forced natural roll, the
        /// native generator seeded to the same natural, and, for a critical,
        /// the engine's own AutoCriticalThreat and AutoCriticalConfirmation.
        /// The engine then decides hit, critical and discharge itself.
        /// </summary>
        private static RuleAttackWithWeapon CreateControlledPistoleroAttack(UnitEntityData attacker,
            UnitEntityData target, ItemEntityWeapon weapon, int natural, bool critical)
        {
            var attack = new RuleAttackWithWeapon(attacker, target, weapon, 0);
            var roll = new RuleAttackRoll(attacker, target, weapon, 0)
            {
                RuleAttackWithWeapon = attack,
                AutoHit = false,
                AutoCriticalThreat = critical,
                AutoCriticalConfirmation = critical
            };
            SetExactProperty(attack, "AttackRoll", roll);
            FirearmRuntimeState.Service.Set(weapon, new FirearmState(
                FirearmState.CurrentSchemaVersion, 1,
                FirearmStateTokenCatalog.DiagnosticLeadBall, FirearmCondition.Normal));
            int seed = FindNativeD20Seed(natural);
            FirearmMisfireRuntime.QueueForcedNaturalRoll(natural);
            try
            {
                UnityEngine.Random.InitState(seed);
                Rulebook.Trigger(roll);
            }
            finally { FirearmMisfireRuntime.CancelForcedNaturalRoll(); }
            return attack;
        }

        private static JObject UpCloseExpectation(string outcome, string damage, int dice, int charge,
            bool consume, bool? completedNonMisfire = null)
        {
            var value = new JObject
            {
                ["outcome"] = outcome, ["damage"] = damage, ["dice"] = dice,
                ["charge"] = charge, ["consume"] = consume
            };
            if (completedNonMisfire.HasValue) value["completedNonMisfire"] = completedNonMisfire.Value;
            return value;
        }

        /// <summary>
        /// Adds the four separate assertions for one case and returns the
        /// case's aggregate, computed only after every part was evaluated.
        /// </summary>
        private static bool AddUpCloseCaseAssertions(List<RuntimeTestAssertion> assertions, string name,
            JObject expectation, JObject observation, string route)
        {
            JObject verdict = UpCloseAndDeadlyCaseVerdict.Evaluate(expectation, observation);
            observation["verdict"] = verdict;
            foreach (string part in new[] { UpCloseAndDeadlyCaseVerdict.Outcome, UpCloseAndDeadlyCaseVerdict.Damage,
                UpCloseAndDeadlyCaseVerdict.Cost, UpCloseAndDeadlyCaseVerdict.Consumption })
            {
                var item = (JObject)verdict[part];
                assertions.Add(Assertion(name + "." + part, (string)item["expected"], (string)item["observed"],
                    (bool)item["pass"], route));
            }
            return (bool)verdict["pass"];
        }

        /// <summary>
        /// The 2026-11-15 sequence, replayed unchanged (one shared target, the
        /// same rolls, seeds and order) with every stage measured. Recorded to
        /// attribute the old split; it carries no contract verdict.
        /// </summary>
        private JObject ReplayOriginalPistoleroSequence(UnitEntityData attacker, UnitEntityData target,
            ItemEntityWeapon weapon, PistoleroBlueprintSet pistolero, BlueprintAbilityResource grit,
            MechanicsContext context, IList<string> unreadable)
        {
            var stages = new JObject();
            Func<Buff, RuleAttackWithWeapon, string, JObject> deliver = (marker, attack, source) =>
                ObserveUpCloseDelivery(attacker, target, grit, marker, attack, source, () =>
                    marker.CallComponents<IInitiatorRulebookHandler<RuleAttackWithWeapon>>(
                        handler => handler.OnEventDidTrigger(attack)));

            Buff hitMarker = ArmPistoleroMarker(attacker, pistolero, context);
            int hp = target.HPLeft;
            RuleAttackWithWeapon hit = CreateResolvedPistoleroAttack(attacker, target, weapon, true, false, 19);
            UnityEngine.Random.InitState(FindNativeD6Seed(6));
            stages["hit"] = deliver(hitMarker, hit, "none");
            int hitDamage = hp - target.HPLeft;

            FirearmRuntimeState.Service.Set(weapon, new FirearmState(FirearmState.CurrentSchemaVersion, 1,
                FirearmStateTokenCatalog.DiagnosticLeadBall, FirearmCondition.Normal));
            Buff missMarker = ArmPistoleroMarker(attacker, pistolero, context);
            target.Descriptor.Stats.Dexterity.BaseValue = 100;
            hp = target.HPLeft;
            RuleAttackWithWeapon miss = CreateResolvedPistoleroAttack(attacker, target, weapon, false, false, 19);
            UnityEngine.Random.InitState(FindNativeD6Seed(6));
            stages["miss"] = deliver(missMarker, miss, "none");
            int missDamage = hp - target.HPLeft;
            target.Descriptor.Stats.Dexterity.BaseValue = 10;

            FirearmRuntimeState.Service.Set(weapon, new FirearmState(FirearmState.CurrentSchemaVersion, 1,
                FirearmStateTokenCatalog.DiagnosticLeadBall, FirearmCondition.Normal));
            Buff immuneMarker = ArmPistoleroMarker(attacker, pistolero, context);
            RuleAttackWithWeapon immune = CreateResolvedPistoleroAttack(attacker, target, weapon, true, true, 19);
            stages["precisionImmunity"] = deliver(immuneMarker, immune, "none");

            Buff criticalMarker = ArmPistoleroMarker(attacker, pistolero, context);
            hp = target.HPLeft;
            RuleAttackWithWeapon critical = CreateResolvedPistoleroAttack(attacker, target, weapon, true, false, 19);
            bool engineCritical = critical.AttackRoll.IsCriticalConfirmed;
            SetExactProperty(critical.AttackRoll, "IsCriticalConfirmed", true);
            UnityEngine.Random.InitState(FindNativeD6Seed(6));
            JObject criticalStage = deliver(criticalMarker, critical, "set-after-resolution");
            criticalStage["criticalConfirmedByEngineBeforeOverride"] = engineCritical;
            stages["critical"] = criticalStage;
            int criticalDamage = hp - target.HPLeft;
            bool legacyCritical = criticalDamage == hitDamage &&
                !attacker.Descriptor.HasFact(pistolero.UpCloseArmed);

            foreach (JProperty stage in stages.Properties())
            {
                var row = (JObject)stage.Value;
                foreach (string key in new[] { "hit", "deedPackets", "gritBefore", "gritAfter", "gritRestored", "armedInstance" })
                    if (row[key] == null || row[key].Type == JTokenType.Null)
                        unreadable.Add(stage.Name + "." + key);
            }
            return new JObject
            {
                ["route"] = "component fixture, original 2026-11-15 order: shared target, AddBuff arming, " +
                    "hand-invoked handler, forced natural 19 without a seeded native d20 for the rolls",
                ["stages"] = stages,
                ["legacyAggregate"] = new JObject
                {
                    ["hitDamageByHitPoints"] = hitDamage,
                    ["missDamageByHitPoints"] = missDamage,
                    ["criticalDamageByHitPoints"] = criticalDamage,
                    ["upCloseCriticalAsBefore"] = legacyCritical,
                    ["formula"] = "critical hit-point loss == hit-stage hit-point loss && armed state gone"
                }
            };
        }

        /// <summary>
        /// The controlled component cases: a fresh healthy target for each,
        /// the outcome set before the roll resolves, and four separate results
        /// per case. Aggregates keep their old names and are computed last.
        /// </summary>
        private JObject RunControlledPistoleroCases(UnitEntityData attacker, ItemEntityWeapon weapon,
            PistoleroBlueprintSet pistolero, BlueprintAbilityResource grit, MechanicsContext context,
            int dice, List<RuntimeTestAssertion> assertions, List<UnitEntityData> disposables)
        {
            BlueprintUnit source = Kingmaker.Blueprints.Root.BlueprintRoot.Instance.DefaultPlayerCharacter;
            const string route = "component fixture: AddBuff arming, standalone RuleAttackRoll resolved by the engine, " +
                "handler invoked by hand; not the registered ability or the attack command";
            var cases = new JObject();
            var aggregates = new Dictionary<string, bool>();
            var afterCase = new List<Action>();
            attacker.Descriptor.Stats.BaseAttackBonus.BaseValue = 100;

            Func<int, UnitEntityData> freshTarget = armorBonus =>
            {
                UnitEntityData unit = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                disposables.Add(unit);
                unit.Descriptor.Stats.HitPoints.AddModifier(500, (GameLogicComponent)null, ModifierDescriptor.None);
                // Deflection, because a firearm shot in its first range
                // increment is resolved against touch armor class, which
                // ignores natural armor.
                if (armorBonus != 0)
                    unit.Descriptor.Stats.AC.AddModifier(armorBonus, (GameLogicComponent)null, ModifierDescriptor.Deflection);
                return unit;
            };
            Func<string, int, int, bool, bool, Func<UnitEntityData, RuleAttackWithWeapon, bool>, JObject> run =
                (name, natural, armorBonus, critical, deliverTwice, prepare) =>
                {
                    UnitEntityData target = freshTarget(armorBonus);
                    attacker.Descriptor.Resources.Restore(grit, 100);
                    Buff marker = ArmPistoleroMarker(attacker, pistolero, context);
                    RuleAttackWithWeapon attack = CreateControlledPistoleroAttack(attacker, target, weapon, natural, critical);
                    bool keepGrit = prepare == null || prepare(target, attack);
                    if (!keepGrit)
                        attacker.Descriptor.Resources.Spend(grit, attacker.Descriptor.Resources.GetResourceAmount(grit));
                    JObject row = ObserveUpCloseDelivery(attacker, target, grit, marker, attack,
                        critical ? "engine" : "none", () =>
                        {
                            marker.CallComponents<IInitiatorRulebookHandler<RuleAttackWithWeapon>>(
                                handler => handler.OnEventDidTrigger(attack));
                            if (deliverTwice)
                                marker.CallComponents<IInitiatorRulebookHandler<RuleAttackWithWeapon>>(
                                    handler => handler.OnEventDidTrigger(attack));
                        });
                    row["requestedNatural"] = natural;
                    row["targetArmorBonus"] = armorBonus;
                    row["handlerInvocations"] = deliverTwice ? 2 : 1;
                    if (attacker.Descriptor.Buffs.RawFacts.Contains(marker))
                        attacker.Descriptor.Buffs.RemoveFact(marker);
                    foreach (Action cleanup in afterCase) cleanup();
                    afterCase.Clear();
                    cases[name] = row;
                    return row;
                };

            aggregates["up-close-hit"] = AddUpCloseCaseAssertions(assertions, "up-close-hit",
                UpCloseExpectation("hit", "full", dice, 1, true, true),
                run("hit", 15, 0, false, false, null), route);
            aggregates["up-close-miss"] = AddUpCloseCaseAssertions(assertions, "up-close-miss",
                UpCloseExpectation("miss", "half", dice, 1, true, true),
                run("miss", 10, 500, false, false, null), route);
            aggregates["up-close-critical"] = AddUpCloseCaseAssertions(assertions, "up-close-critical",
                UpCloseExpectation("critical", "full", dice, 1, true, true),
                run("critical", 20, 0, true, false, null), route);
            aggregates["up-close-precision-immunity"] = AddUpCloseCaseAssertions(assertions, "up-close-precision-immunity",
                UpCloseExpectation("hit", "none", dice, 0, true, true),
                run("precisionImmunity", 15, 0, false, false, (target, attack) =>
                {
                    // Supplied: the roll's precision-immunity flag is set after it resolved.
                    SetExactProperty(attack.AttackRoll, "ImmuneToSneakAttack", true);
                    return true;
                }), route + "; precision immunity set on the resolved roll");
            aggregates["up-close-insufficient-grit"] = AddUpCloseCaseAssertions(assertions, "up-close-insufficient-grit",
                UpCloseExpectation("hit", "none", dice, 0, true, true),
                run("insufficientGrit", 15, 0, false, false, (target, attack) => false), route + "; grit spent to 0 after arming");
            bool misfire = AddUpCloseCaseAssertions(assertions, "up-close-misfire",
                UpCloseExpectation("any", "none", dice, 0, false, false),
                run("misfire", 1, 0, false, false, null), route);
            bool scatter = AddUpCloseCaseAssertions(assertions, "up-close-scatter",
                UpCloseExpectation("hit", "none", dice, 0, false),
                run("scatter", 15, 0, false, false, (target, attack) =>
                {
                    ScatterVolleyRuntime.Register(attack, target, target.UniqueId, 1, 19);
                    afterCase.Add(() => ScatterVolleyRuntime.Cancel(attack));
                    return true;
                }), route + "; registered as a scatter volley");
            aggregates["up-close-misfire-and-scatter"] = misfire && scatter;
            aggregates["up-close-dead-shot-once"] = AddUpCloseCaseAssertions(assertions, "up-close-dead-shot-once",
                UpCloseExpectation("hit", "full", dice, 1, true, true),
                run("deadShotDuplicate", 15, 0, false, true, (target, attack) =>
                {
                    DeadShotRuntime.RegisterDelivery(attack, true, false, 0, 3);
                    afterCase.Add(() => DeadShotRuntime.CancelDelivery(attack));
                    return true;
                }), route + "; registered as a Dead Shot delivery, handler invoked twice");

            foreach (KeyValuePair<string, bool> aggregate in aggregates)
                assertions.Add(Assertion(aggregate.Key,
                    "every separate result of the case passes (aggregate computed after each was evaluated)",
                    string.Join(",", assertions.Where(value => value.Name.StartsWith(aggregate.Key + ".", StringComparison.Ordinal) ||
                        (aggregate.Key == "up-close-misfire-and-scatter" &&
                         (value.Name.StartsWith("up-close-misfire.", StringComparison.Ordinal) ||
                          value.Name.StartsWith("up-close-scatter.", StringComparison.Ordinal))))
                        .Select(value => value.Name + "=" + value.Status).ToArray()),
                    aggregate.Value, route));
            return cases;
        }
    }
}
