using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Commands.Base;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Read-only sampling around the engine's real cost application. Never
        // sets cooldowns, action availability, command state, or resources.
        private sealed class FirearmReviewCosts : IDisposable
        {
            private static FirearmReviewCosts active;
            private readonly UnitEntityData actor;
            private readonly JArray rows;
            private readonly HarmonyInstance harmony;
            private readonly MethodInfo method;
            private readonly MethodInfo forceFinish = typeof(UnitCommand).GetMethod("ForceFinishForTurnBased");
            private readonly List<UnitCommand> forced = new List<UnitCommand>();
            private readonly string id = "KMG.FirearmReviewCosts." + Guid.NewGuid().ToString("N");
            private readonly List<Charge> charges = new List<Charge>();
            private sealed class Charge
            {
                internal UnitCommand Command;
                internal float[] Before, After;
                internal float Elapsed;
                internal bool TurnBased, FullRound, ExactTurn;
            }
            internal FirearmReviewCosts(UnitEntityData actor, JArray rows)
            {
                if (active != null) throw new InvalidOperationException("Nested native cost observation.");
                this.actor = actor; this.rows = rows;
                method = typeof(UnitActionController).GetMethod("UpdateCooldowns");
                if (method == null) throw new MissingMethodException("Native command cost boundary.");
                harmony = HarmonyInstance.Create(id);
                harmony.Patch(method,
                    new HarmonyMethod(typeof(FirearmReviewCosts).GetMethod("Before", BindingFlags.Static | BindingFlags.NonPublic)),
                    new HarmonyMethod(typeof(FirearmReviewCosts).GetMethod("After", BindingFlags.Static | BindingFlags.NonPublic)));
                harmony.Patch(forceFinish, null, new HarmonyMethod(typeof(FirearmReviewCosts)
                    .GetMethod("Forced", BindingFlags.Static | BindingFlags.NonPublic)));
                active = this;
            }
            private static float[] Snapshot(UnitEntityData unit)
            {
                var c = unit.CombatState.Cooldown;
                return new[] { c.StandardAction, c.MoveAction, c.SwiftAction };
            }
            private static void Before(UnitCommand __0, out Charge __state)
            {
                __state = null;
                if (active == null || !ReferenceEquals(__0.Executor, active.actor)) return;
                var turn = Game.Instance.TurnBasedCombatController.CurrentTurn;
                __state = new Charge { Command = __0, Before = Snapshot(active.actor),
                    Elapsed = __0.TimeSinceStart,
                    TurnBased = TurnBased.Controllers.CombatController.IsInTurnBasedCombat(),
                    FullRound = TurnBased.Utility.UnitCommandExtensions.IsFullRoundAction(__0),
                    ExactTurn = turn != null && ReferenceEquals(turn.Unit, active.actor) };
            }
            private static void After(Charge __state)
            {
                if (__state == null) return;
                __state.After = Snapshot(active.actor);
                active.charges.Add(__state);
                active.rows.Add(new JObject { ["nativeCost"] = __state.Command.GetType().Name,
                    ["type"] = __state.Command.Type.ToString(), ["turnBased"] = __state.TurnBased,
                    ["exactActorTurn"] = __state.ExactTurn, ["fullRound"] = __state.FullRound,
                    ["before"] = new JArray(__state.Before), ["after"] = new JArray(__state.After),
                    ["elapsed"] = __state.Elapsed });
            }
            private static void Forced(UnitCommand __instance)
            {
                if (active == null || !ReferenceEquals(__instance.Executor, active.actor)) return;
                active.forced.Add(__instance);
                active.rows.Add(new JObject { ["nativeForceFinishForTurnBased"] = __instance.GetType().Name,
                    ["started"] = __instance.IsStarted, ["acted"] = __instance.IsActed,
                    ["finished"] = __instance.IsFinished, ["result"] = __instance.Result.ToString(),
                    ["costApplications"] = active.Count(__instance) });
            }
            internal int ForcedCount(UnitCommand command) { return forced.Count(value => ReferenceEquals(value, command)); }
            internal int Count(UnitCommand command) { return charges.Count(c => ReferenceEquals(c.Command, command)); }
            internal bool Exact(UnitCommand command, bool turnBased)
            {
                var matches = charges.Where(value => ReferenceEquals(value.Command, command)).ToArray();
                if (matches.Length != 1 || command.Cutscene || command.IsIgnoreCooldown) return false;
                var c = matches[0];
                if (c.TurnBased != turnBased || (turnBased && !c.ExactTurn)) return false;
                var expected = (float[])c.Before.Clone();
                int slot = command.Type == UnitCommand.CommandType.Standard ? 0 :
                    (command.Type == UnitCommand.CommandType.Move ||
                     (!turnBased && command.Type == UnitCommand.CommandType.Free)) ? 1 :
                    command.Type == UnitCommand.CommandType.Swift ? 2 : -1;
                if (slot < 0) return false;
                if (turnBased)
                {
                    expected[slot] += slot == 1 ? 3 : 6;
                    if (slot == 0 && c.FullRound) expected[1] += 3;
                }
                else expected[slot] = (slot == 1 ? 3 : 6) - c.Elapsed;
                return expected.Zip(c.After, (a, b) => Math.Abs(a - b) < 0.0001f).All(equal => equal);
            }
            public void Dispose()
            {
                harmony.Unpatch(method, HarmonyPatchType.All, id);
                harmony.Unpatch(forceFinish, HarmonyPatchType.All, id);
                if (ReferenceEquals(active, this)) active = null;
            }
        }
    }
}
