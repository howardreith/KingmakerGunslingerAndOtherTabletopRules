using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Firing;
using KingmakerGunslinger.Misfires;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private sealed class KillChainVerdict
        {
            internal string Name, Detail;
            internal bool Passed;
        }

        // CR-04 (owner RTWP report): one native full attack with four planned
        // attacks against several legal enemies. Native UpdateTarget chooses
        // every replacement after real projectile damage and native death; no
        // target, attack progress, cooldown, action or order is assigned. Each
        // lane records its observations and restores its fixture before the
        // verdicts are asserted, so a failing lane cannot hide the others.
        private IEnumerable<object> FirearmReviewKillChainCases()
        {
            var verdicts = new List<KillChainVerdict>();
            foreach (string lane in new[] { "loaded", "empty", "loaded-paused", "empty-paused" })
                foreach (object step in FirearmKillChainLane(lane, verdicts)) yield return step;
            foreach (object step in OrdinaryBowKillChainControl(verdicts)) yield return step;
            foreach (var verdict in verdicts) FirearmInputCheck(verdict.Name, verdict.Passed, verdict.Detail);
        }

        private static KillChainVerdict KillChainCheck(string name, bool passed, string detail)
        {
            return new KillChainVerdict { Name = name, Passed = passed, Detail = detail };
        }

        // A is the clicked target; B and C are mortal; D is durable so a shot
        // after the native fourth attack would be visible. Native nearest-first
        // selection is expected to choose B, C, then D from these positions.
        private static UnitEntityData[] PrepareKillChainTargets(FirearmInputFixture f)
        {
            var targets = new[] { f.Enemy, f.SpawnAdditional(false, "KillChainB"),
                f.SpawnAdditional(false, "KillChainC"), f.SpawnAdditional(false, "KillChainDurable") };
            targets[1].Position = new Vector3(1f, 0f, 2.5f);
            targets[2].Position = new Vector3(-1.5f, 0f, 3f);
            targets[3].Position = new Vector3(2f, 0f, 3.5f);
            for (int index = 0; index < 3; index++)
            {
                targets[index].Stats.Constitution.BaseValue = 1;
                targets[index].Stats.HitPoints.BaseValue = 1;
                targets[index].Descriptor.State.Immortality.Release();
            }
            return targets;
        }

        private static void KillChainResolveImpacts(FirearmInputFixture f, IEnumerable<UnitEntityData> targets)
        {
            MethodInfo tick = typeof(UnitLifeController).GetMethod("TickOnUnit",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var life = new UnitLifeController();
            f.Turns.Execute(() => {
                new Kingmaker.Controllers.Projectiles.ProjectileHitController().Tick();
                foreach (var unit in targets) tick.Invoke(life, new object[] { unit });
            });
        }

        private IEnumerable<object> FirearmKillChainLane(string lane, List<KillChainVerdict> verdicts)
        {
            bool loaded = lane.StartsWith("loaded", StringComparison.Ordinal);
            bool paused = lane.EndsWith("-paused", StringComparison.Ordinal);
            string prefix = "cr04-" + lane + "-four-attack-kill-chain-";
            _firearmInputStage = prefix + "fixture";
            _firearmInputFixture = new FirearmInputFixture(false, _firearmInputRows, initiallyLoaded: loaded);
            var f = _firearmInputFixture;
            using (var costs = new FirearmReviewCosts(f.Actor, _firearmInputRows))
            using (var trace = new FirearmKillChainTrace(f.Actor, _firearmInputRows, prefix))
            {
                // Ordinary pistol, matching Rapid Reload and paper cartridges:
                // the existing Free reload. BAB 16 gives four native attacks.
                f.Actor.Descriptor.AddFact(BlueprintBootstrap.GunslingerClass.GunTraining.ChoiceFor(FirearmKind.Pistol));
                f.Actor.Stats.Dexterity.BaseValue = 30;
                f.Actor.Stats.BaseAttackBonus.BaseValue = 16;
                var targets = PrepareKillChainTargets(f);
                var cartridge = BlueprintBootstrap.BasicAmmunition.PaperCartridge;
                Game.Instance.Player.Inventory.Add(cartridge, 12);
                f.Actor.Descriptor.ActivatableAbilities.Enumerable.Single(a =>
                    ReferenceEquals(a.Blueprint, BlueprintBootstrap.PaperCartridgeMode.Ability)).IsOn = true;
                for (int tick = 0; tick < 16; tick++) { f.Turns.PumpCommands(false); yield return null; }
                int planned = 0;
                f.Turns.Execute(() => planned = UnitAttack.EstimateFullAttacks(f.Actor));
                int cartridges = Game.Instance.Player.Inventory.Count(cartridge);
                long reloads = FreeActionFullAttackReloadPatch.Succeeded;
                long ended = FreeActionFullAttackReloadPatch.Interrupted;
                long faults = FreeActionFullAttackReloadPatch.Failed;
                bool ready = f.Actor.Commands.Empty && planned == 4 &&
                    !f.Actor.CombatState.IsFullAttackRestrictedBecauseOfMoveAction &&
                    f.State.LoadedRounds == (loaded ? 1 : 0) && f.State.Condition == FirearmCondition.Normal &&
                    ReferenceEquals(f.Actor.AutoUseAbility?.Blueprint, BlueprintBootstrap.ReloadTestMusketAbility);
                string setup = "planned=" + planned + ";loadedRounds=" + f.State.LoadedRounds +
                    ";cartridges=" + cartridges + ";autoUseReload=" +
                    ReferenceEquals(f.Actor.AutoUseAbility?.Blueprint, BlueprintBootstrap.ReloadTestMusketAbility) +
                    ";restricted=" + f.Actor.CombatState.IsFullAttackRestrictedBecauseOfMoveAction +
                    ";distances=" + string.Join(",", targets.Select(t => f.Actor.DistanceTo(t).ToString("F2")).ToArray());
                verdicts.Add(KillChainCheck(prefix + "native-ready", ready, setup));

                // Supported deterministic control: natural 10 on each firearm
                // attack roll (hit, no misfire). Damage and death are native.
                FirearmMisfireRuntime.QueueForcedNaturalRoll(10);
                if (paused) f.Turns.SetFirearmPaused(true);
                f.Click(targets[0]);
                var order = BrokenSequenceSuppressionRuntime.Orders.Current(f.Actor);
                trace.Note("click", f, null, targets);
                if (paused)
                {
                    for (int frame = 0; frame < 3; frame++) yield return null;
                    trace.Note("paused-frames", f, null, targets);
                    f.Turns.SetFirearmPaused(false);
                }
                UnitAttack chain = null;
                for (int tick = 0; tick < 200 && (chain == null || !chain.IsFinished); tick++)
                {
                    if (!FirearmMisfireRuntime.PendingForcedNaturalRoll.HasValue)
                        FirearmMisfireRuntime.QueueForcedNaturalRoll(10);
                    f.Pump(); KillChainResolveImpacts(f, targets);
                    if (chain == null && trace.Shots.Count != 0) chain = trace.Shots[0].Command;
                    trace.Sample(tick, f, chain, targets);
                    yield return null;
                }
                trace.Note("finished", f, chain, targets);

                var own = trace.Shots.Where(s => ReferenceEquals(s.Command, chain)).ToArray();
                var foreign = trace.Shots.Where(s => !ReferenceEquals(s.Command, chain)).ToArray();
                int spent = cartridges - Game.Instance.Player.Inventory.Count(cartridge);
                string detail = setup + ";" + trace.Summary(chain, targets) + ";spentCartridges=" + spent +
                    ";inlineReloads=" + (FreeActionFullAttackReloadPatch.Succeeded - reloads) +
                    ";endedFullAttacks=" + (FreeActionFullAttackReloadPatch.Interrupted - ended) +
                    ";reloadFaults=" + (FreeActionFullAttackReloadPatch.Failed - faults) +
                    ";orderCurrent=" + BrokenSequenceSuppressionRuntime.Orders.IsCurrent(order) +
                    ";orderOwnerIsChain=" + (order != null && ReferenceEquals(order.Owner, chain)) +
                    ";orderTarget=" + (order?.Target as UnitEntityData)?.UniqueId + ";" + f.Describe();
                verdicts.Add(KillChainCheck(prefix + "one-command-four-native-attacks",
                    chain != null && chain.IsFinished && chain.Result == UnitCommand.ResultType.Success &&
                    chain.IsFullAttack && chain.AllAttacks.Count == 4 && chain.GetAttackIndex() == 4 &&
                    own.Length == 4 && foreign.Length == 0 &&
                    own.Select(s => s.AttackNumber).SequenceEqual(new[] { 0, 1, 2, 3 }), detail));
                verdicts.Add(KillChainCheck(prefix + "native-replacements-after-kills",
                    own.Select(s => s.Target).SequenceEqual(targets) && own.All(s => s.Hit) &&
                    targets.Take(3).All(t => t.Descriptor.State.IsDead) && !targets[3].Descriptor.State.IsDead, detail));
                verdicts.Add(KillChainCheck(prefix + "ammunition-and-action-economy",
                    spent == (loaded ? 3 : 4) && f.State.IsEmpty && f.State.Condition == FirearmCondition.Normal &&
                    FreeActionFullAttackReloadPatch.Succeeded - reloads == 3 &&
                    FreeActionFullAttackReloadPatch.Interrupted == ended &&
                    FreeActionFullAttackReloadPatch.Failed == faults && chain != null && costs.Exact(chain, false), detail));
                verdicts.Add(KillChainCheck(prefix + "order-follows-surviving-command",
                    order != null && BrokenSequenceSuppressionRuntime.Orders.IsCurrent(order) &&
                    ReferenceEquals(order.Owner, chain) && ReferenceEquals(order.Target, targets[3]) &&
                    trace.Cancellations == 0, detail));
            }
            f.Dispose();
            verdicts.Add(KillChainCheck(prefix + "cleanup", f.Restored, "Native fixture restored exactly."));
            _firearmInputFixture = null;
        }

        // Ordinary native longbow under the same four-attack geometry. Its
        // rolls are unforced, so a miss may legitimately repeat a target; the
        // control asserts native continuation, not a kill sequence.
        private IEnumerable<object> OrdinaryBowKillChainControl(List<KillChainVerdict> verdicts)
        {
            const string prefix = "cr04-ordinary-longbow-control-";
            _firearmInputStage = prefix + "fixture";
            _firearmInputFixture = new FirearmInputFixture(false, _firearmInputRows);
            var f = _firearmInputFixture;
            var targets = PrepareKillChainTargets(f);
            var ally = f.SpawnAdditional(true, "KillChainLongbowAlly");
            ally.Position = new Vector3(0f, 0f, -1f);
            ally.Stats.BaseAttackBonus.BaseValue = 16;
            ally.Stats.Dexterity.BaseValue = 30;
            var bow = ResourcesLibrary.TryGetBlueprint<BlueprintItemWeapon>("201f6150321e09048bd59e9b7f558cb0");
            if (bow == null) throw new InvalidOperationException("Native ordinary longbow fixture is unavailable.");
            ally.Body.PrimaryHand.InsertItem(new ItemEntityWeapon(bow));
            Game.Instance.Player.PartyCharacters.Add(ally);
            Game.Instance.Player.InvalidateCharacterLists(); Game.Instance.Player.UpdateCharacterLists();
            using (var trace = new FirearmKillChainTrace(ally, _firearmInputRows, prefix))
            {
                for (int tick = 0; tick < 16; tick++) { f.Turns.PumpCommands(false); yield return null; }
                f.Turns.Execute(() => Game.Instance.UI.SelectionManager.SelectUnit(ally.View, true, false, false));
                f.Click(targets[0]);
                var attack = ally.Commands.Raw.OfType<UnitAttack>().SingleOrDefault();
                var actions = new UnitActionController();
                var cooldowns = new Kingmaker.Controllers.Combat.UnitCombatCooldownsController();
                MethodInfo tickCooldown = cooldowns.GetType().GetMethod("TickOnUnit", BindingFlags.Instance | BindingFlags.NonPublic);
                MethodInfo tickUnit = typeof(UnitActionController).GetMethod("TickOnUnit", BindingFlags.Instance | BindingFlags.NonPublic);
                for (int tick = 0; tick < 240 && attack != null && !attack.IsFinished; tick++)
                {
                    f.Turns.PumpCommands(false);
                    f.Turns.Execute(() => {
                        tickCooldown.Invoke(cooldowns, new object[] { ally });
                        ally.View.AnimationManager.Tick(); ally.View.AnimationManager.Update(0.25f);
                        foreach (var command in ally.Commands.Raw.Where(command => command != null))
                            if (command.Animation != null) command.Animation.IsActed = true;
                        tickUnit.Invoke(actions, new object[] { ally });
                    });
                    KillChainResolveImpacts(f, targets);
                    trace.Sample(tick, f, attack, targets);
                    yield return null;
                }
                var own = trace.Shots.Where(s => ReferenceEquals(s.Command, attack)).ToArray();
                string detail = trace.Summary(attack, targets);
                // After any shot that left its target unconscious or dead, the
                // next shot of the same command chose a conscious replacement.
                bool retargeted = own.Length == 4;
                for (int index = 1; index < own.Length; index++)
                    if (!own[index - 1].TargetConsciousAfter && ReferenceEquals(own[index].Target, own[index - 1].Target))
                        retargeted = false;
                verdicts.Add(KillChainCheck(prefix + "native-continuation", attack != null && attack.IsFullAttack &&
                    attack.AllAttacks.Count == 4 && attack.GetAttackIndex() == 4 && retargeted &&
                    ReferenceEquals(own[0].Target, targets[0]) && NativeFirearmAttackOrder.Get(attack) == null, detail));
                f.Turns.Execute(() => Game.Instance.UI.SelectionManager.SelectUnit(f.Actor.View, true, false, false));
            }
            f.Dispose();
            verdicts.Add(KillChainCheck(prefix + "cleanup", f.Restored, "Native fixture restored exactly."));
            _firearmInputFixture = null;
        }

        // Fixture-only observer for the reproduction. It records native and
        // adapter transitions with their call stacks; it never alters a result,
        // target, order or command.
        private sealed class FirearmKillChainTrace : IDisposable
        {
            internal sealed class Shot
            {
                internal UnitAttack Command;
                internal UnitEntityData Target;
                internal int AttackNumber;
                internal bool Hit, TargetConsciousAfter;
                internal RuleAttackWithWeapon Rule;
            }
            private const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.Static;
            private static FirearmKillChainTrace active;
            private readonly UnitEntityData actor;
            private readonly JArray rows;
            private readonly string lane;
            private readonly string id = "KMG.FirearmKillChainTrace." + Guid.NewGuid().ToString("N");
            private readonly HarmonyInstance harmony;
            private readonly List<MethodBase> patched = new List<MethodBase>();
            private readonly Dictionary<UnitAttack, RuleAttackWithWeapon> lastRules =
                new Dictionary<UnitAttack, RuleAttackWithWeapon>();
            private int rejectedExecutions;
            internal readonly List<Shot> Shots = new List<Shot>();
            internal int Cancellations;

            internal FirearmKillChainTrace(UnitEntityData actor, JArray rows, string lane)
            {
                if (active != null) throw new InvalidOperationException("Nested kill-chain observation.");
                this.actor = actor; this.rows = rows; this.lane = lane;
                harmony = HarmonyInstance.Create(id);
                active = this;
                try
                {
                    Patch(typeof(FirearmAttackOrderLedger).GetMethod("Cancel", All), "Cancelling", null);
                    Patch(typeof(UnitCommand).GetMethod("OnEnded", All), null, "Ended");
                    Patch(typeof(NativeFirearmAttackOrder).GetMethod("MayExecute", All), null, "Executed");
                    Patch(typeof(UnitAttack).GetMethod("UpdateTarget", All, null, Type.EmptyTypes, null), "Resolving", "Resolved");
                    Patch(typeof(UnitAttack).GetMethod("OnAction", All), "Acting", "Acted");
                }
                catch { Dispose(); throw; }
            }
            private void Patch(MethodBase method, string prefix, string postfix)
            {
                if (method == null) throw new MissingMethodException("Kill-chain observation boundary.");
                harmony.Patch(method,
                    prefix == null ? null : new HarmonyMethod(typeof(FirearmKillChainTrace).GetMethod(prefix, All)),
                    postfix == null ? null : new HarmonyMethod(typeof(FirearmKillChainTrace).GetMethod(postfix, All)), null);
                patched.Add(method);
            }
            private static string Stack()
            {
                var frames = new StackTrace(2, false).GetFrames() ?? new StackFrame[0];
                return string.Join(" < ", frames.Take(16).Select(frame => {
                    MethodBase method = frame.GetMethod();
                    return method == null ? "?" : (method.DeclaringType == null ? "" : method.DeclaringType.Name + ".") + method.Name;
                }).ToArray());
            }
            private static string Id(object value)
            {
                return value == null ? null : value.GetType().Name + "#" + RuntimeHelpers.GetHashCode(value);
            }
            private static string UnitId(object value)
            {
                var unit = value as UnitEntityData;
                return unit == null ? null : unit.UniqueId;
            }
            private void Add(JObject row)
            {
                row["killChainLane"] = lane; row["frame"] = Time.frameCount;
                rows.Add(row);
            }
            private static JToken Attack(UnitAttack command)
            {
                if (command == null) return JValue.CreateNull();
                var binding = NativeFirearmAttackOrder.Get(command);
                var order = binding == null ? null : binding.Order;
                return new JObject { ["command"] = Id(command), ["ai"] = command.AiAction != null,
                    ["started"] = command.IsStarted, ["acted"] = command.IsActed, ["finished"] = command.IsFinished,
                    ["result"] = command.Result.ToString(), ["fullAttack"] = command.IsFullAttack,
                    ["attackIndex"] = command.GetAttackIndex(), ["planned"] = command.AllAttacks.Count,
                    ["timeSinceStart"] = command.TimeSinceStart, ["target"] = UnitId(command.Target),
                    ["bindingTarget"] = binding == null ? null : UnitId(binding.Target),
                    ["proposal"] = binding != null && binding.Proposal, ["epoch"] = binding == null ? -1 : binding.Epoch,
                    ["order"] = Id(order), ["orderTarget"] = order == null ? null : UnitId(order.Target),
                    ["orderOwner"] = order == null ? null : Id(order.Owner),
                    ["orderCurrent"] = BrokenSequenceSuppressionRuntime.Orders.IsCurrent(order),
                    ["orderCancelled"] = order != null && order.Cancelled };
            }
            internal void Note(string what, FirearmInputFixture f, UnitAttack chain, UnitEntityData[] targets)
            {
                var current = BrokenSequenceSuppressionRuntime.Orders.Current(actor);
                Add(new JObject { ["killChainNote"] = what, ["chain"] = Attack(chain),
                    ["commands"] = new JArray(actor.Commands.Raw.Where(c => c != null).Concat(actor.Commands.Queue)
                        .Select(c => c is UnitAttack ? Attack((UnitAttack)c) : new JObject {
                            ["command"] = Id(c), ["finished"] = c.IsFinished, ["result"] = c.Result.ToString() })),
                    ["currentOrder"] = Id(current), ["currentOrderTarget"] = current == null ? null : UnitId(current.Target),
                    ["currentOrderOwner"] = current == null ? null : Id(current.Owner),
                    ["targets"] = new JArray(targets.Select(t => t.UniqueId + ":" + (t.Descriptor.State.IsDead ? "dead" :
                        t.Descriptor.State.IsConscious ? "up" : "down"))),
                    ["state"] = f.Describe() });
            }
            internal void Sample(int tick, FirearmInputFixture f, UnitAttack chain, UnitEntityData[] targets)
            {
                if (tick < 40 || tick % 10 == 0 || (chain != null && chain.IsFinished)) Note("pump-" + tick, f, chain, targets);
            }
            internal string Summary(UnitAttack chain, UnitEntityData[] targets)
            {
                return "chain=" + Id(chain) + ";chainResult=" + (chain == null ? "none" : chain.Result.ToString()) +
                    ";attackIndex=" + (chain == null ? -1 : chain.GetAttackIndex()) +
                    ";planned=" + (chain == null ? -1 : chain.AllAttacks.Count) +
                    ";shots=" + string.Join(",", Shots.Select(s => Id(s.Command) + ">" +
                        Array.IndexOf(targets, s.Target) + "#" + s.AttackNumber + (s.Hit ? "h" : "m")).ToArray()) +
                    ";targets=" + string.Join(",", targets.Select(t => t.Descriptor.State.IsDead ? "dead" :
                        t.Descriptor.State.IsConscious ? "up" : "down").ToArray()) +
                    ";orderCancellations=" + Cancellations + ";rejectedExecutions=" + rejectedExecutions;
            }
            private static void Cancelling(FirearmAttackOrderLedger __instance, FirearmAttackOrderLedger.Order __0)
            {
                if (active == null || __0 == null || !ReferenceEquals(__0.Actor, active.actor) ||
                    !__instance.IsCurrent(__0)) return;
                active.Cancellations++;
                active.Add(new JObject { ["killChainCancel"] = Id(__0), ["target"] = UnitId(__0.Target),
                    ["owner"] = Id(__0.Owner), ["stack"] = Stack() });
            }
            private static void Ended(UnitCommand __instance)
            {
                if (active == null || !ReferenceEquals(__instance.Executor, active.actor)) return;
                active.Add(new JObject { ["killChainEnded"] = Id(__instance), ["result"] = __instance.Result.ToString(),
                    ["attack"] = Attack(__instance as UnitAttack), ["stack"] = Stack() });
            }
            private static void Executed(UnitAttack __0, bool __result)
            {
                if (active == null || __result || __0 == null || !ReferenceEquals(__0.Executor, active.actor) ||
                    active.rejectedExecutions++ > 12) return;
                active.Add(new JObject { ["killChainMayExecute"] = false, ["attack"] = Attack(__0), ["stack"] = Stack() });
            }
            private static void Resolving(UnitAttack __instance, out UnitEntityData __state)
            { __state = __instance.Target; }
            private static void Resolved(UnitAttack __instance, UnitEntityData __state, bool __result)
            {
                if (active == null || !ReferenceEquals(__instance.Executor, active.actor) ||
                    (__result && ReferenceEquals(__state, __instance.Target))) return;
                active.Add(new JObject { ["killChainUpdateTarget"] = __result, ["previous"] = UnitId(__state),
                    ["previousDead"] = __state != null && __state.Descriptor.State.IsDead, ["attack"] = Attack(__instance) });
            }
            private static void Acting(UnitAttack __instance, out int __state)
            { __state = __instance.GetAttackIndex(); }
            private static void Acted(UnitAttack __instance, int __state, UnitCommand.ResultType __result)
            {
                if (active == null || !ReferenceEquals(__instance.Executor, active.actor)) return;
                RuleAttackWithWeapon rule = __instance.LastAttackRule, previous;
                active.lastRules.TryGetValue(__instance, out previous);
                bool shot = rule != null && !ReferenceEquals(rule, previous);
                if (shot)
                {
                    active.lastRules[__instance] = rule;
                    active.Shots.Add(new Shot { Command = __instance, Target = rule.Target, AttackNumber = rule.AttackNumber,
                        Hit = rule.AttackRoll != null && rule.AttackRoll.IsHit, Rule = rule,
                        TargetConsciousAfter = rule.Target.Descriptor.State.IsConscious });
                }
                active.Add(new JObject { ["killChainOnAction"] = __result.ToString(), ["indexBefore"] = __state,
                    ["shot"] = shot, ["shotTarget"] = shot ? rule.Target.UniqueId : null,
                    ["attackNumber"] = shot ? rule.AttackNumber : -1, ["attack"] = Attack(__instance) });
            }
            public void Dispose()
            {
                foreach (var method in patched) harmony.Unpatch(method, HarmonyPatchType.All, id);
                patched.Clear();
                if (ReferenceEquals(active, this)) active = null;
            }
        }
    }
}
