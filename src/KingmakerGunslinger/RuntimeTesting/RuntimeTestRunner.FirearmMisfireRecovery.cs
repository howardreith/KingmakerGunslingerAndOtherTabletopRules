using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Kingmaker;
using Kingmaker.Controllers.Rest;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Firing;
using KingmakerGunslinger.Gunsmithing;
using KingmakerGunslinger.Misfires;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// POL-01 / S4 T3, owner decision B: native lanes for recovery after a
    /// misfire break. Same save-free fixture, native click handler, native
    /// commands, native AI brain controller and native rest completion as
    /// disposable-firearm-break-interruption. Every lane records the
    /// pre-break order and command, the break epoch, the recovery and its
    /// own effects separately from whatever comes after it, and, when the
    /// AI acts again, the exact fresh command it constructed.
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        private static string Id(object value)
        {
            return value == null ? null : value.GetType().Name + "#" + RuntimeHelpers.GetHashCode(value);
        }

        private int LeadBalls { get { return Game.Instance.Player.Inventory.Count(BlueprintBootstrap.BasicAmmunition.LeadBall); } }

        private IEnumerable<object> DriveFirearmIdleWithoutAi(FirearmInputFixture f)
        {
            float until = Time.realtimeSinceStartup + 1f;
            for (int tick = 0; tick < 40 || Time.realtimeSinceStartup < until; tick++)
            { f.Pump(false); f.Record(tick); yield return null; }
        }

        private sealed class PreBreak
        {
            internal UnitCommand StaleAttack;
            internal FirearmAttackOrderLedger.Order Order;
            internal UnitAttack Command;
            internal int Epoch;
            internal JObject Row;
        }

        /// <summary>
        /// A player order whose shot misfires to Broken: records the order,
        /// its command, the epoch before and after, its cancellation, and a
        /// native attack command constructed before the break (kept to be run
        /// after recovery). Then the AI retry window passes with no shot.
        /// </summary>
        private IEnumerable<object> BreakWithRecordedOrder(FirearmInputFixture f, string prefix, PreBreak record)
        {
            record.StaleAttack = UnitAttack.CreateAttackCommand(f.Actor, f.Enemy);
            int epochBefore = BrokenSequenceSuppressionRuntime.GetDegradationEpoch(f.Actor, f.Weapon);
            FirearmMisfireRuntime.QueueForcedNaturalRoll(1);
            f.Click(f.Enemy);
            record.Order = BrokenSequenceSuppressionRuntime.Orders.Current(f.Actor);
            for (int tick = 0; tick < 240 && f.Shots.Count == 0; tick++)
            { f.Pump(); f.Record(tick); yield return null; }
            record.Command = f.LastShotCommand;
            int powder = f.Powder;
            foreach (object step in DriveFirearmIdle(f)) yield return step;
            record.Epoch = BrokenSequenceSuppressionRuntime.GetDegradationEpoch(f.Actor, f.Weapon);
            record.Row = new JObject
            {
                ["preBreakOrder"] = Id(record.Order), ["preBreakCommand"] = Id(record.Command),
                ["preBreakConstructedAttack"] = Id(record.StaleAttack),
                ["epochBefore"] = epochBefore, ["breakEpoch"] = record.Epoch,
                ["orderCancelled"] = record.Order != null && record.Order.Cancelled,
                ["orderCurrent"] = BrokenSequenceSuppressionRuntime.Orders.IsCurrent(record.Order),
                ["commandFinished"] = record.Command != null && record.Command.IsFinished,
                ["state"] = f.State.ToString(), ["shots"] = f.Shots.Count,
                ["suppressed"] = BrokenSequenceSuppressionRuntime.IsSuppressed(f.Actor, f.Weapon)
            };
            FirearmInputCheck(prefix + "break-cancels-the-sequence", record.Order != null && record.Command != null &&
                record.StaleAttack != null && record.Epoch == epochBefore + 1 && record.Order.Cancelled &&
                !BrokenSequenceSuppressionRuntime.Orders.IsCurrent(record.Order) && f.Shots.Count == 1 &&
                f.State.Condition == FirearmCondition.Broken && f.Powder == powder &&
                BrokenSequenceSuppressionRuntime.IsSuppressed(f.Actor, f.Weapon), record.Row.ToString(Newtonsoft.Json.Formatting.None));
        }

        /// <summary>The recovery's own effects, read when it has just committed and before any AI tick.</summary>
        private JObject RecoveryOwnEffects(FirearmInputFixture f, string prefix, string action, PreBreak record,
            int shots, int powder, int balls, long submission, long releasesBefore, int grit)
        {
            var gunslinger = BlueprintBootstrap.GunslingerClass;
            var row = new JObject
            {
                ["action"] = action, ["firearm"] = Id(f.Weapon), ["state"] = f.State.ToString(),
                ["epoch"] = BrokenSequenceSuppressionRuntime.GetDegradationEpoch(f.Actor, f.Weapon),
                ["released"] = BrokenSequenceSuppressionRuntime.RecoveryReleases - releasesBefore,
                ["suppressed"] = BrokenSequenceSuppressionRuntime.IsSuppressed(f.Actor, f.Weapon),
                ["requiresOrder"] = BrokenSequenceSuppressionRuntime.Orders.RequiresOrder(f.Actor, f.Weapon),
                ["currentOrder"] = Id(BrokenSequenceSuppressionRuntime.Orders.Current(f.Actor)),
                ["submissionDelta"] = BrokenSequenceSuppressionRuntime.Orders.Submission(f.Actor) - submission,
                ["shotDelta"] = f.Shots.Count - shots, ["powderDelta"] = f.Powder - powder, ["ballDelta"] = LeadBalls - balls,
                ["gritDelta"] = f.Actor.Descriptor.Resources.GetResourceAmount(gunslinger.Grit.Resource) - grit,
                ["preBreakOrderCurrent"] = BrokenSequenceSuppressionRuntime.Orders.IsCurrent(record.Order),
                ["frame"] = Time.frameCount, ["gameSeconds"] = Game.Instance.Player.GameTime.TotalSeconds
            };
            FirearmInputCheck(prefix + "recovery-commits-and-releases-only", f.State.Condition == FirearmCondition.Normal &&
                (int)row["epoch"] == record.Epoch && (long)row["released"] == 1 && !(bool)row["suppressed"] &&
                !(bool)row["requiresOrder"] && row["currentOrder"].Type == JTokenType.Null && (long)row["submissionDelta"] == 0 &&
                (int)row["shotDelta"] == 0 && (int)row["powderDelta"] == 0 && (int)row["ballDelta"] == 0 &&
                !(bool)row["preBreakOrderCurrent"], row.ToString(Newtonsoft.Json.Formatting.None));
            return row;
        }

        /// <summary>The command constructed before the break, run now: it must be refused.</summary>
        private JObject StaleWorkRefused(FirearmInputFixture f, string prefix, PreBreak record)
        {
            int shots = f.Shots.Count;
            f.Turns.Execute(() => f.Actor.Commands.Run(record.StaleAttack));
            var binding = NativeFirearmAttackOrder.Get(record.StaleAttack);
            var row = new JObject
            {
                ["staleCommand"] = Id(record.StaleAttack),
                ["admitted"] = f.Actor.Commands.Raw.Contains(record.StaleAttack) || f.Actor.Commands.Queue.Contains(record.StaleAttack),
                ["bindingEpoch"] = binding == null ? -1 : binding.Epoch,
                ["currentEpoch"] = BrokenSequenceSuppressionRuntime.GetDegradationEpoch(f.Actor, f.Weapon),
                ["preBreakOrderCurrent"] = BrokenSequenceSuppressionRuntime.Orders.IsCurrent(record.Order),
                ["preBreakOrderMayResume"] = record.Order != null && BrokenSequenceSuppressionRuntime.Orders.MayResume(
                    f.Actor, f.Weapon, record.Order.Epoch, BrokenSequenceSuppressionRuntime.Orders.Submission(f.Actor), record.Order),
                ["capturedAttackMayResume"] = BrokenSequenceInterruptionPolicy.MayResumeCapturedAttack(
                    record.Epoch - 1, BrokenSequenceSuppressionRuntime.GetDegradationEpoch(f.Actor, f.Weapon))
            };
            FirearmInputCheck(prefix + "pre-break-work-stays-refused", !(bool)row["admitted"] && f.Shots.Count == shots &&
                !(bool)row["preBreakOrderCurrent"] && !(bool)row["preBreakOrderMayResume"] && !(bool)row["capturedAttackMayResume"],
                row.ToString(Newtonsoft.Json.Formatting.None));
            return row;
        }

        /// <summary>
        /// With the native AI brain ticking, waits for the next shot and records every command the actor gains
        /// after recovery: the first one the AI constructed, its target, binding, action commitment and the
        /// exact ammunition and shot that follow.
        /// </summary>
        private IEnumerable<object> FreshAiDecision(FirearmInputFixture f, string prefix, PreBreak record, int recoveryFrame,
            JObject lane)
        {
            var seen = new HashSet<UnitCommand>(f.Actor.Commands.Raw.Concat(f.Actor.Commands.Queue).Where(c => c != null));
            var gained = new JArray();
            lane["commandsGained"] = gained;
            int shots = f.Shots.Count, powder = f.Powder, balls = LeadBalls;
            UnitCommand firstAi = null;
            float started = Time.realtimeSinceStartup;
            FirearmMisfireRuntime.QueueForcedNaturalRoll(10);
            for (int tick = 0; tick < 1200 && f.Shots.Count == shots && Time.realtimeSinceStartup < started + 30f; tick++)
            {
                f.Pump();
                foreach (UnitCommand command in f.Actor.Commands.Raw.Concat(f.Actor.Commands.Queue).Where(c => c != null).ToArray())
                {
                    if (!seen.Add(command)) continue;
                    var binding = NativeFirearmAttackOrder.Get(command);
                    var attack = command as UnitAttack;
                    gained.Add(new JObject
                    {
                        ["command"] = Id(command), ["ai"] = command.AiAction == null ? null : command.AiAction.GetType().Name,
                        ["frame"] = Time.frameCount, ["realSecondsAfterRecovery"] = Math.Round(Time.realtimeSinceStartup - started, 3),
                        ["gameSeconds"] = Game.Instance.Player.GameTime.TotalSeconds,
                        ["target"] = attack == null ? null : attack.Target == null ? null : attack.Target.UniqueId,
                        ["bindingEpoch"] = binding == null ? -1 : binding.Epoch,
                        ["bindingOrder"] = binding == null ? null : Id(binding.Order),
                        ["isPreBreakCommand"] = ReferenceEquals(command, record.Command) || ReferenceEquals(command, record.StaleAttack)
                    });
                    if (firstAi == null && command.AiAction != null) firstAi = command;
                }
                f.Record(tick);
                yield return null;
            }
            FirearmMisfireRuntime.CancelForcedNaturalRoll();
            UnitAttack shotCommand = f.Shots.Count > shots ? f.LastShotCommand : null;
            var shotBinding = shotCommand == null ? null : NativeFirearmAttackOrder.Get(shotCommand);
            lane["firstAiCommand"] = Id(firstAi);
            lane["shotCommand"] = Id(shotCommand);
            lane["shotCommandAi"] = shotCommand == null || shotCommand.AiAction == null ? null : shotCommand.AiAction.GetType().Name;
            lane["shotTarget"] = shotCommand == null ? null : shotCommand.Target.UniqueId;
            lane["shotBindingOrder"] = shotBinding == null ? null : Id(shotBinding.Order);
            lane["shotBindingEpoch"] = shotBinding == null ? -1 : shotBinding.Epoch;
            lane["shotDelta"] = f.Shots.Count - shots;
            lane["powderDebit"] = powder - f.Powder;
            lane["ballDebit"] = balls - LeadBalls;
            lane["standardCooldownAfterShot"] = f.Actor.CombatState.Cooldown.StandardAction;
            var reload = firstAi as UnitUseAbility;
            lane["firstAiReloadActionCost"] = reload == null ? JValue.CreateNull() : (JToken)f.PeakActionCost(reload);
            lane["stateAfterShot"] = f.State.ToString();
            FirearmInputCheck(prefix + "fresh-ai-decision-fires", firstAi != null && recoveryFrame < (int)gained
                    .First(row => (string)row["command"] == Id(firstAi))["frame"] &&
                !ReferenceEquals(firstAi, record.Command) && !ReferenceEquals(firstAi, record.StaleAttack) &&
                shotCommand != null && !ReferenceEquals(shotCommand, record.Command) &&
                (shotBinding == null || (shotBinding.Order == null && shotBinding.Epoch == record.Epoch)) &&
                ReferenceEquals(shotCommand.Target, f.Enemy) && f.Shots.Count == shots + 1 &&
                (int)lane["powderDebit"] == 1,
                lane.ToString(Newtonsoft.Json.Formatting.None));
        }

        /// <summary>
        /// The native stop (UnitEntityData.TryCancelCommands: clears hold, interrupts every command, drops the
        /// manual and last targets) while the AI's next post-recovery command is pending: nothing it carried fires.
        /// </summary>
        private IEnumerable<object> StopAfterRecovery(FirearmInputFixture f, string prefix, JObject lane)
        {
            UnitCommand next = null;
            int shots = f.Shots.Count;
            for (int tick = 0; tick < 600 && next == null && f.Shots.Count == shots; tick++)
            {
                f.Pump();
                next = f.Actor.Commands.Raw.Concat(f.Actor.Commands.Queue).FirstOrDefault(c => c != null && !c.IsFinished &&
                    !c.IsActed && c.AiAction != null);
                yield return null;
            }
            bool running = f.Actor.Commands.IsRunning();
            f.Turns.Execute(() => { if (running) f.Actor.Commands.InterruptAll(true); else f.Actor.TryCancelCommands(); });
            int afterStop = f.Shots.Count;
            foreach (object step in DriveFirearmIdleWithoutAi(f)) yield return step;
            lane["stop"] = new JObject
            {
                ["pendingAiCommand"] = Id(next), ["route"] = running ? "Commands.InterruptAll (a command was running)" : "TryCancelCommands",
                ["pendingCommandFinished"] = next != null && next.IsFinished, ["pendingCommandActed"] = next != null && next.IsActed,
                ["openCommands"] = f.Actor.Commands.Raw.Count(c => c != null && !c.IsFinished),
                ["shotsDuringStopWindow"] = f.Shots.Count - afterStop, ["shotsBeforeStop"] = afterStop - shots,
                ["holdState"] = f.Actor.HoldState
            };
            FirearmInputCheck(prefix + "deliberate-stop-holds", next != null && next.IsFinished && !next.IsActed &&
                f.Shots.Count == afterStop && afterStop == shots,
                lane["stop"].ToString(Newtonsoft.Json.Formatting.None));
        }

        /// <summary>Fixture setup after an out-of-combat recovery: both units join combat natively. No order is given.</summary>
        private static void RejoinCombat(FirearmInputFixture f, JObject lane)
        {
            f.Turns.Execute(() =>
            {
                f.Actor.CombatState.JoinCombat();
                f.Enemy.CombatState.JoinCombat();
                Game.Instance.Player.UpdateIsInCombat();
            });
            lane["combatRejoined"] = new JObject { ["actor"] = f.Actor.IsInCombat, ["enemy"] = f.Enemy.IsInCombat,
                ["currentOrder"] = Id(BrokenSequenceSuppressionRuntime.Orders.Current(f.Actor)) };
        }

        private IEnumerable<object> FirearmMisfireRecoveryCases()
        {
            var gunslinger = BlueprintBootstrap.GunslingerClass;
            Func<FirearmInputFixture, PreBreak, string, JObject> lane = (f, record, name) =>
            {
                var row = new JObject { ["lane"] = name, ["preBreak"] = record.Row };
                _firearmInputRows.Add(row);
                return row;
            };

            // 1. RTWP: misfire -> Quick Clear -> fresh AI decision -> ordinary shot; then a deliberate stop.
            {
                string prefix = "pol01-rtwp-quick-clear-";
                _firearmInputStage = prefix + "fixture";
                _firearmInputFixture = new FirearmInputFixture(false, _firearmInputRows);
                var f = _firearmInputFixture;
                f.Actor.Descriptor.AddFact(gunslinger.Grit.Feature);
                f.Actor.Descriptor.AddFact(gunslinger.QuickClear.Feature);
                var record = new PreBreak();
                foreach (object step in BreakWithRecordedOrder(f, prefix, record)) yield return step;
                var row = lane(f, record, "rtwp-quick-clear");
                _firearmInputStage = prefix + "recovery";
                int shots = f.Shots.Count, powder = f.Powder, balls = LeadBalls;
                long releases = BrokenSequenceSuppressionRuntime.RecoveryReleases;
                int grit = f.Actor.Descriptor.Resources.GetResourceAmount(gunslinger.Grit.Resource);
                var quickClear = f.ClickAbility(gunslinger.QuickClear.StandardAbility);
                long submission = BrokenSequenceSuppressionRuntime.Orders.Submission(f.Actor);
                for (int tick = 0; tick < 240 && !quickClear.IsFinished; tick++) { f.Pump(false); f.Record(tick); yield return null; }
                row["recovery"] = RecoveryOwnEffects(f, prefix, "Quick Clear (standard)", record, shots, powder, balls, submission, releases, grit);
                row["recovery"]["actionCost"] = f.PeakActionCost(quickClear);
                row["recovery"]["command"] = Id(quickClear);
                row["recovery"]["result"] = quickClear.Result.ToString();
                int recoveryFrame = Time.frameCount;
                _firearmInputStage = prefix + "stale-work";
                row["staleWork"] = StaleWorkRefused(f, prefix, record);
                _firearmInputStage = prefix + "fresh-ai";
                foreach (object step in FreshAiDecision(f, prefix, record, recoveryFrame, row)) yield return step;
                _firearmInputStage = prefix + "stop";
                foreach (object step in StopAfterRecovery(f, prefix, row)) yield return step;
                f.Dispose();
                FirearmInputCheck(prefix + "cleanup", f.Restored, "Native fixture restored exactly.");
                _firearmInputFixture = null;
            }

            // 2. RTWP: misfire -> out-of-combat Repair Firearm -> combat resumes -> fresh AI decision.
            {
                string prefix = "pol01-rtwp-repair-";
                _firearmInputStage = prefix + "fixture";
                _firearmInputFixture = new FirearmInputFixture(false, _firearmInputRows);
                var f = _firearmInputFixture;
                var record = new PreBreak();
                foreach (object step in BreakWithRecordedOrder(f, prefix, record)) yield return step;
                var row = lane(f, record, "rtwp-repair-firearm");
                _firearmInputStage = prefix + "recovery";
                f.MakeEnemyPeaceful(true);
                f.Turns.LeaveCombatForRecovery();
                int shots = f.Shots.Count, powder = f.Powder, balls = LeadBalls;
                long releases = BrokenSequenceSuppressionRuntime.RecoveryReleases;
                int grit = f.Actor.Descriptor.Resources.GetResourceAmount(gunslinger.Grit.Resource);
                var repair = f.ClickAbility(BlueprintBootstrap.RepairTestMusketAbility);
                long submission = BrokenSequenceSuppressionRuntime.Orders.Submission(f.Actor);
                for (int tick = 0; tick < 240 && f.State.Condition == FirearmCondition.Broken; tick++) { f.Pump(false); f.Record(tick); yield return null; }
                for (int tick = 0; tick < 240 && !repair.IsFinished; tick++) { f.Pump(false); yield return null; }
                row["recovery"] = RecoveryOwnEffects(f, prefix, "Repair Firearm (out of combat)", record, shots, powder, balls, submission, releases, grit);
                row["recovery"]["actionCost"] = f.PeakActionCost(repair);
                row["recovery"]["command"] = Id(repair);
                row["recovery"]["partyInCombat"] = Game.Instance.Player.IsInCombat;
                int recoveryFrame = Time.frameCount;
                row["staleWork"] = StaleWorkRefused(f, prefix, record);
                _firearmInputStage = prefix + "fresh-ai";
                f.MakeEnemyPeaceful(false);
                RejoinCombat(f, row);
                foreach (object step in FreshAiDecision(f, prefix, record, recoveryFrame, row)) yield return step;
                f.Dispose();
                FirearmInputCheck(prefix + "cleanup", f.Restored, "Native fixture restored exactly.");
                _firearmInputFixture = null;
            }

            // 3. RTWP: misfire -> the gun leaves its wielder (shared inventory; another participant) -> completed rest.
            foreach (string where in new[] { "shared-inventory", "another-participant" })
            {
                string prefix = "pol01-rtwp-rest-" + where + "-";
                _firearmInputStage = prefix + "fixture";
                _firearmInputFixture = new FirearmInputFixture(false, _firearmInputRows);
                var f = _firearmInputFixture;
                var record = new PreBreak();
                foreach (object step in BreakWithRecordedOrder(f, prefix, record)) yield return step;
                var row = lane(f, record, "rtwp-rest-" + where);
                f.MakeEnemyPeaceful(true);
                f.Turns.LeaveCombatForRecovery();
                _firearmInputStage = prefix + "transfer";
                UnitEntityData holder = null;
                f.Turns.Execute(() => f.Actor.Body.PrimaryHand.RemoveItem());
                if (where == "another-participant")
                {
                    holder = f.SpawnAdditional(true, "RestHolder");
                    Game.Instance.Player.PartyCharacters.Add(holder);
                    Game.Instance.Player.InvalidateCharacterLists(); Game.Instance.Player.UpdateCharacterLists();
                    var weapon = f.Weapon;
                    f.Turns.Execute(() => holder.Body.PrimaryHand.InsertItem(weapon));
                }
                row["transfer"] = new JObject
                {
                    ["where"] = where, ["wieldedByOriginal"] = ReferenceEquals(f.Actor.Body.PrimaryHand.MaybeItem, f.Weapon),
                    ["inSharedInventory"] = Game.Instance.Player.Inventory.Items.Contains(f.Weapon),
                    ["holder"] = holder == null ? null : holder.UniqueId,
                    ["wieldedByHolder"] = holder != null && ReferenceEquals(holder.Body.PrimaryHand.MaybeItem, f.Weapon),
                    ["holderParticipant"] = holder != null && Game.Instance.Player.AllCharacters.Contains(holder),
                    ["originalStillSuppressed"] = BrokenSequenceSuppressionRuntime.IsSuppressed(f.Actor, f.Weapon),
                    ["holderSuppressed"] = holder != null && BrokenSequenceSuppressionRuntime.IsSuppressed(holder, f.Weapon)
                };
                FirearmInputCheck(prefix + "transferred", (where == "shared-inventory" ? (bool)row["transfer"]["inSharedInventory"]
                    : (bool)row["transfer"]["wieldedByHolder"] && (bool)row["transfer"]["holderParticipant"]) &&
                    !(bool)row["transfer"]["wieldedByOriginal"] && (bool)row["transfer"]["originalStillSuppressed"] &&
                    !(bool)row["transfer"]["holderSuppressed"], row["transfer"].ToString(Newtonsoft.Json.Formatting.None));
                _firearmInputStage = prefix + "rest";
                int shots = f.Shots.Count, powder = f.Powder, balls = LeadBalls;
                long releases = BrokenSequenceSuppressionRuntime.RecoveryReleases;
                int grit = f.Actor.Descriptor.Resources.GetResourceAmount(gunslinger.Grit.Resource);
                long submission = BrokenSequenceSuppressionRuntime.Orders.Submission(f.Actor);
                row["rest"] = NativeCompletedRest(f, prefix);
                row["recovery"] = RecoveryOwnEffects(f, prefix, "completed rest maintenance (" + where + ")", record, shots,
                    powder, balls, submission, releases, grit);
                int recoveryFrame = Time.frameCount;
                _firearmInputStage = prefix + "return";
                if (holder != null) f.Turns.Execute(() => holder.Body.PrimaryHand.RemoveItem());
                var gun = f.Weapon;
                f.Turns.Execute(() => f.Actor.Body.PrimaryHand.InsertItem(gun));
                FirearmInputCheck(prefix + "returned-to-wielder", f.SameItemIdentity && f.State.Condition == FirearmCondition.Normal,
                    f.Describe());
                row["staleWork"] = StaleWorkRefused(f, prefix, record);
                _firearmInputStage = prefix + "fresh-ai";
                f.MakeEnemyPeaceful(false);
                RejoinCombat(f, row);
                foreach (object step in FreshAiDecision(f, prefix, record, recoveryFrame, row)) yield return step;
                f.Dispose();
                FirearmInputCheck(prefix + "cleanup", f.Restored, "Native fixture restored exactly.");
                _firearmInputFixture = null;
            }

            // 4. Negative controls, then the positive Quick Clear on the same gun.
            {
                string prefix = "pol01-rtwp-negative-";
                _firearmInputStage = prefix + "fixture";
                _firearmInputFixture = new FirearmInputFixture(false, _firearmInputRows);
                var f = _firearmInputFixture;
                f.Actor.Descriptor.AddFact(gunslinger.Grit.Feature);
                f.Actor.Descriptor.AddFact(gunslinger.QuickClear.Feature);
                var record = new PreBreak();
                foreach (object step in BreakWithRecordedOrder(f, prefix, record)) yield return step;
                var row = lane(f, record, "rtwp-negative-controls");
                long calls = BrokenSequenceSuppressionRuntime.RecoveryCalls, releases = BrokenSequenceSuppressionRuntime.RecoveryReleases;

                _firearmInputStage = prefix + "repair-in-combat";
                var refused = f.ClickAbility(BlueprintBootstrap.RepairTestMusketAbility, false);
                for (int tick = 0; tick < 40; tick++) { f.Pump(false); yield return null; }
                row["failedRepair"] = new JObject { ["submitted"] = refused != null, ["state"] = f.State.ToString(),
                    ["recoveryCalls"] = BrokenSequenceSuppressionRuntime.RecoveryCalls - calls,
                    ["released"] = BrokenSequenceSuppressionRuntime.RecoveryReleases - releases,
                    ["suppressed"] = BrokenSequenceSuppressionRuntime.IsSuppressed(f.Actor, f.Weapon) };
                FirearmInputCheck(prefix + "failed-repair-releases-nothing", f.State.Condition == FirearmCondition.Broken &&
                    BrokenSequenceSuppressionRuntime.RecoveryReleases == releases &&
                    BrokenSequenceSuppressionRuntime.IsSuppressed(f.Actor, f.Weapon), row["failedRepair"].ToString(Newtonsoft.Json.Formatting.None));

                _firearmInputStage = prefix + "reload-only";
                var reload = f.ClickAbility(BlueprintBootstrap.ReloadTestMusketAbility);
                for (int tick = 0; tick < 240 && f.State.IsEmpty; tick++) { f.Pump(false); yield return null; }
                foreach (object step in DriveFirearmIdle(f)) yield return step;
                row["reloadOnly"] = new JObject { ["acted"] = reload.IsActed, ["state"] = f.State.ToString(),
                    ["released"] = BrokenSequenceSuppressionRuntime.RecoveryReleases - releases,
                    ["suppressed"] = BrokenSequenceSuppressionRuntime.IsSuppressed(f.Actor, f.Weapon), ["shots"] = f.Shots.Count };
                FirearmInputCheck(prefix + "reload-releases-nothing", reload.IsActed && f.State.Condition == FirearmCondition.Broken &&
                    f.State.LoadedRounds == 1 && BrokenSequenceSuppressionRuntime.RecoveryReleases == releases &&
                    BrokenSequenceSuppressionRuntime.IsSuppressed(f.Actor, f.Weapon) && f.Shots.Count == 1,
                    row["reloadOnly"].ToString(Newtonsoft.Json.Formatting.None));

                _firearmInputStage = prefix + "other-firearm";
                var original = f.Weapon;
                var other = new ItemEntityWeapon(original.Blueprint);
                FirearmRuntimeState.Service.Set(other, new FirearmState(FirearmState.CurrentSchemaVersion, 0, null, FirearmCondition.Broken));
                f.Turns.Execute(() => { f.Actor.Body.PrimaryHand.RemoveItem(); f.Actor.Body.PrimaryHand.InsertItem(other); });
                f.Turns.Execute(() => f.Actor.CombatState.Cooldown.StandardAction = 0f);
                var otherClear = f.ClickAbility(gunslinger.QuickClear.StandardAbility);
                for (int tick = 0; tick < 240 && !otherClear.IsFinished; tick++) { f.Pump(false); yield return null; }
                row["otherFirearm"] = new JObject { ["otherState"] = FirearmRuntimeState.Service.GetOrCreate(other).Repository.State.ToString(),
                    ["released"] = BrokenSequenceSuppressionRuntime.RecoveryReleases - releases,
                    ["originalSuppressed"] = BrokenSequenceSuppressionRuntime.IsSuppressed(f.Actor, original),
                    ["originalState"] = FirearmRuntimeState.Service.GetOrCreate(original).Repository.State.ToString() };
                FirearmInputCheck(prefix + "other-firearm-releases-nothing",
                    FirearmRuntimeState.Service.GetOrCreate(other).Repository.State.Condition == FirearmCondition.Normal &&
                    BrokenSequenceSuppressionRuntime.RecoveryReleases == releases &&
                    BrokenSequenceSuppressionRuntime.IsSuppressed(f.Actor, original),
                    row["otherFirearm"].ToString(Newtonsoft.Json.Formatting.None));
                f.Turns.Execute(() => { f.Actor.Body.PrimaryHand.RemoveItem(); f.Actor.Body.PrimaryHand.InsertItem(original); });
                FirearmRuntimeState.Service.Forget(other);
                foreach (var stack in Game.Instance.Player.Inventory.Items.Where(item => ReferenceEquals(item, other)).ToArray())
                    Game.Instance.Player.Inventory.Remove(stack, stack.Count);

                _firearmInputStage = prefix + "positive-control";
                f.Turns.Execute(() => f.Actor.CombatState.Cooldown.StandardAction = 0f);
                var clear = f.ClickAbility(gunslinger.QuickClear.StandardAbility);
                for (int tick = 0; tick < 240 && !clear.IsFinished; tick++) { f.Pump(false); yield return null; }
                row["positiveControl"] = new JObject { ["state"] = f.State.ToString(),
                    ["released"] = BrokenSequenceSuppressionRuntime.RecoveryReleases - releases,
                    ["suppressed"] = BrokenSequenceSuppressionRuntime.IsSuppressed(f.Actor, f.Weapon) };
                FirearmInputCheck(prefix + "same-gun-quick-clear-releases", f.SameItemIdentity &&
                    f.State.Condition == FirearmCondition.Normal && BrokenSequenceSuppressionRuntime.RecoveryReleases == releases + 1 &&
                    !BrokenSequenceSuppressionRuntime.IsSuppressed(f.Actor, f.Weapon),
                    row["positiveControl"].ToString(Newtonsoft.Json.Formatting.None));
                f.Dispose();
                FirearmInputCheck(prefix + "cleanup", f.Restored, "Native fixture restored exactly.");
                _firearmInputFixture = null;
            }

            // 5. Manual-order control: a new player order still fires the Broken gun before any recovery.
            {
                string prefix = "pol01-rtwp-manual-order-";
                _firearmInputStage = prefix + "fixture";
                _firearmInputFixture = new FirearmInputFixture(false, _firearmInputRows);
                var f = _firearmInputFixture;
                var record = new PreBreak();
                foreach (object step in BreakWithRecordedOrder(f, prefix, record)) yield return step;
                var row = lane(f, record, "rtwp-manual-order-control");
                int powder = f.Powder;
                FirearmMisfireRuntime.QueueForcedNaturalRoll(10);
                f.Click(f.Enemy);
                var order = BrokenSequenceSuppressionRuntime.Orders.Current(f.Actor);
                for (int tick = 0; tick < 300 && f.Shots.Count < 2; tick++) { f.Pump(); f.Record(tick); yield return null; }
                row["manualOrder"] = new JObject { ["order"] = Id(order), ["shotCommand"] = Id(f.LastShotCommand),
                    ["shots"] = f.Shots.Count, ["state"] = f.State.ToString(), ["powderDebit"] = powder - f.Powder,
                    ["recoveryReleases"] = BrokenSequenceSuppressionRuntime.RecoveryReleases };
                FirearmInputCheck(prefix + "player-order-fires-the-broken-gun", order != null && !ReferenceEquals(order, record.Order) &&
                    f.Shots.Count == 2 && f.State.Condition == FirearmCondition.Broken && f.LastShotCommand != null &&
                    !ReferenceEquals(f.LastShotCommand, record.Command), row["manualOrder"].ToString(Newtonsoft.Json.Formatting.None));
                f.Dispose();
                FirearmInputCheck(prefix + "cleanup", f.Restored, "Native fixture restored exactly.");
                _firearmInputFixture = null;
            }

            // 6. Turn-based: stale work, recovery and the manual order. The party's own AI is not driven in turn-based
            //    combat, so no autonomous resumption is claimed; any AI command the actor gains is recorded.
            {
                string prefix = "pol01-tb-quick-clear-";
                _firearmInputStage = prefix + "fixture";
                _firearmInputFixture = new FirearmInputFixture(true, _firearmInputRows);
                var f = _firearmInputFixture;
                f.Actor.Descriptor.AddFact(gunslinger.Grit.Feature);
                f.Actor.Descriptor.AddFact(gunslinger.QuickClear.Feature);
                var record = new PreBreak();
                foreach (object step in BreakWithRecordedOrder(f, prefix, record)) yield return step;
                var row = lane(f, record, "tb-quick-clear");
                f.Turns.EndCurrentTurn(); f.Turns.ReachCasterTurn();
                int shots = f.Shots.Count, powder = f.Powder, balls = LeadBalls;
                long releases = BrokenSequenceSuppressionRuntime.RecoveryReleases;
                int grit = f.Actor.Descriptor.Resources.GetResourceAmount(gunslinger.Grit.Resource);
                var quickClear = f.ClickAbility(gunslinger.QuickClear.StandardAbility);
                long submission = BrokenSequenceSuppressionRuntime.Orders.Submission(f.Actor);
                for (int tick = 0; tick < 240 && !quickClear.IsFinished; tick++) { f.Pump(); f.Record(tick); yield return null; }
                row["recovery"] = RecoveryOwnEffects(f, prefix, "Quick Clear (standard, turn-based)", record, shots, powder, balls, submission, releases, grit);
                row["staleWork"] = StaleWorkRefused(f, prefix, record);
                var aiCommands = f.Actor.Commands.Raw.Concat(f.Actor.Commands.Queue).Count(c => c != null && c.AiAction != null && !c.IsFinished);
                row["aiCommandsAfterRecovery"] = aiCommands;
                f.Turns.EndCurrentTurn(); f.Turns.ReachCasterTurn();
                FirearmMisfireRuntime.QueueForcedNaturalRoll(10);
                powder = f.Powder;
                f.Click(f.Enemy);
                var order = BrokenSequenceSuppressionRuntime.Orders.Current(f.Actor);
                for (int tick = 0; tick < 300 && f.Shots.Count == shots; tick++) { f.Pump(); f.Record(tick); yield return null; }
                row["manualOrder"] = new JObject { ["order"] = Id(order), ["shotCommand"] = Id(f.LastShotCommand),
                    ["shotDelta"] = f.Shots.Count - shots, ["powderDebit"] = powder - f.Powder, ["state"] = f.State.ToString() };
                FirearmInputCheck(prefix + "manual-order-after-recovery-fires", order != null && f.Shots.Count == shots + 1 &&
                    f.State.Condition == FirearmCondition.Normal && !ReferenceEquals(f.LastShotCommand, record.Command),
                    row["manualOrder"].ToString(Newtonsoft.Json.Formatting.None));
                f.Dispose();
                FirearmInputCheck(prefix + "cleanup", f.Restored, "Native fixture restored exactly.");
                _firearmInputFixture = null;
            }
        }

        /// <summary>The native completed-rest boundary, as in CheckFirearmCompletedRest, with clock and status restored after.</summary>
        private JObject NativeCompletedRest(FirearmInputFixture f, string prefix)
        {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var remaining = typeof(RestController).GetProperty("RemainingTime", flags);
            var tick = typeof(RestController).GetMethod("TickSleepPhase", flags);
            var processed = typeof(CompletedRestMaintenancePatch).GetField("_processedStatus", flags);
            object oldRemaining = remaining.GetValue(null, null), oldProcessed = processed.GetValue(null);
            TimeSpan clock = Game.Instance.Player.GameTime;
            var controller = new RestController();
            long runs = CompletedRestMaintenancePatch.MaintenanceRuns;
            int captured;
            var sleepers = Game.Instance.Player.AllCharacters.ToArray();
            try
            {
                controller.Status = new RestStatus { ApplyRest = true };
                remaining.SetValue(null, TimeSpan.FromHours(8), null);
                foreach (var unit in sleepers) unit.Descriptor.State.AddCondition(UnitCondition.Sleeping, null);
                f.Turns.Execute(() => tick.Invoke(controller, null));
                using (var delivery = new FirearmRestCompletionDelivery(controller, _request.RunId))
                {
                    f.Turns.Execute(() => tick.Invoke(controller, null));
                    captured = delivery.Captured;
                }
            }
            finally
            {
                foreach (var unit in sleepers) unit.Descriptor.State.RemoveCondition(UnitCondition.Sleeping);
                remaining.SetValue(null, oldRemaining, null);
                processed.SetValue(null, oldProcessed);
                Game.Instance.Player.GameTime = clock;
            }
            var row = new JObject { ["restSucceeded"] = controller.Status.RestSucceeded, ["completionCaptured"] = captured,
                ["maintenanceRuns"] = CompletedRestMaintenancePatch.MaintenanceRuns - runs, ["state"] = f.State.ToString(),
                ["participants"] = sleepers.Length };
            FirearmInputCheck(prefix + "native-completed-rest", controller.Status.RestSucceeded && captured == 1 &&
                CompletedRestMaintenancePatch.MaintenanceRuns == runs + 1 && f.State.Condition == FirearmCondition.Normal,
                row.ToString(Newtonsoft.Json.Formatting.None));
            return row;
        }
    }
}
