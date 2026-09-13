using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker.EntitySystem.Entities;
using Kingmaker;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Firing;
using KingmakerGunslinger.Misfires;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private sealed class FirearmReloadMergeObservation : IDisposable
        {
            private static FirearmReloadMergeObservation active;
            private readonly UnitEntityData actor;
            private readonly JArray rows;
            private readonly HarmonyInstance harmony;
            private readonly string id = "KMG.FirearmReloadMergeObservation." + Guid.NewGuid().ToString("N");
            private readonly MethodInfo method = typeof(UnitUseAbility).GetMethod("TryMergeInto");
            internal UnitCommand Survivor;
            internal int Merges;
            internal FirearmReloadMergeObservation(UnitEntityData actor, JArray rows)
            {
                if (active != null) throw new InvalidOperationException("Nested reload merge observation.");
                this.actor = actor; this.rows = rows;
                harmony = HarmonyInstance.Create(id);
                harmony.Patch(method, null, new HarmonyMethod(typeof(FirearmReloadMergeObservation)
                    .GetMethod("After", BindingFlags.NonPublic | BindingFlags.Static)));
                active = this;
            }
            private static void After(UnitUseAbility __instance, UnitCommand __0, bool __result)
            {
                if (active == null || !ReferenceEquals(__instance.Executor, active.actor)) return;
                active.rows.Add(new JObject { ["nativeReloadTryMergeInto"] = __result,
                    ["survivorRunning"] = __0 != null && __0.IsRunning,
                    ["proposal"] = NativeFirearmAttackOrder.Get(__instance)?.Proposal,
                    ["sameActor"] = __0 != null && ReferenceEquals(__0.Executor, active.actor) });
                if (__result) { active.Survivor = __0; active.Merges++; }
            }
            public void Dispose()
            {
                harmony.Unpatch(method, HarmonyPatchType.All, id);
                if (ReferenceEquals(active, this)) active = null;
            }
        }
        private IEnumerable<object> FirearmReviewReloadMergeCases()
        {
            foreach (bool running in new[] { false, true })
            {
                string prefix = "cr02-reload-" + (running ? "running-merge-" : "paused-replace-");
                _firearmInputStage = prefix + "fixture";
                _firearmInputFixture = new FirearmInputFixture(false, _firearmInputRows);
                var f = _firearmInputFixture;
                using (var costs = new FirearmReviewCosts(f.Actor, _firearmInputRows))
                using (var merges = new FirearmReloadMergeObservation(f.Actor, _firearmInputRows))
                {
                    foreach (object step in FirearmReviewBreak(f, prefix)) yield return step;
                    var second = f.SpawnAdditional(false, "NativeReloadMergeTarget");
                    if (!running) f.Turns.SetFirearmPaused(true);
                    f.Click(f.Enemy);
                    var reload = f.Actor.Commands.Raw.OfType<UnitUseAbility>().Single();
                    var old = BrokenSequenceSuppressionRuntime.Orders.Current(f.Actor);
                    if (running)
                    {
                        for (int tick = 0; tick < 240 && !reload.IsStarted; tick++)
                        { f.Pump(); yield return null; }
                        f.Turns.SetFirearmPaused(true);
                    }
                    var stale = f.CreateNativeAiCommand(f.Enemy);
                    float elapsed = reload.TimeSinceStart;
                    FirearmInputCheck(prefix + "before-input", reload.IsStarted == running &&
                        !reload.IsActed && !reload.IsFinished && costs.Count(reload) == 0 &&
                        f.State.IsEmpty && f.Shots.Count == 1,
                        f.Describe() + ";real pending/running native reload before delivery");
                    f.Click(second);
                    var accepted = BrokenSequenceSuppressionRuntime.Orders.Current(f.Actor);
                    var survivor = f.Actor.Commands.Raw.OfType<UnitUseAbility>().Single();
                    FirearmInputCheck(prefix + "native-survivor-owned", accepted != null && old.Cancelled &&
                        !ReferenceEquals(old, accepted) && ReferenceEquals(accepted.Target, second) &&
                        ReferenceEquals(accepted.Owner, survivor) &&
                        ReferenceEquals(NativeFirearmAttackOrder.Get(survivor)?.Order, accepted) &&
                        ReferenceEquals(survivor, reload) == running && merges.Merges == (running ? 1 : 0) &&
                        (!running || ReferenceEquals(merges.Survivor, survivor)) &&
                        (!running || survivor.TimeSinceStart == elapsed) && f.State.IsEmpty &&
                        f.Powder == 12 && f.Shots.Count == 1 && costs.Count(survivor) == 0,
                        f.Describe() + ";nativeMerges=" + merges.Merges + ";sameSurvivor=" + ReferenceEquals(merges.Survivor, survivor) +
                        ";oldCancelled=" + old.Cancelled + ";currentIsOld=" + ReferenceEquals(accepted, old) +
                        ";currentTargetsB=" + ReferenceEquals(accepted?.Target, second) +
                        ";native reload merging/replacement must retain the newest explicit target without spending or resetting progress");
                    f.Turns.Execute(() => f.Actor.Commands.Run(stale));
                    FirearmInputCheck(prefix + "stale-reload-rejected", stale is UnitUseAbility &&
                        !f.Actor.Commands.Raw.Contains(stale) && !f.Actor.Commands.Queue.Contains(stale) &&
                        BrokenSequenceSuppressionRuntime.Orders.IsCurrent(accepted) &&
                        f.Actor.Commands.Raw.Contains(survivor) && f.State.IsEmpty && f.Powder == 12,
                        "The old order's captured native AI reload cannot borrow the accepted survivor's authority.");
                    FirearmMisfireRuntime.QueueForcedNaturalRoll(10);
                    f.Turns.SetFirearmPaused(false);
                    for (int tick = 0; tick < 240 && f.Shots.Count < 2; tick++)
                    { f.Pump(); yield return null; }
                    FirearmInputCheck(prefix + "continuation-exact-new-target", f.Shots.Count == 2 &&
                        ReferenceEquals(f.Shots.Last().Target, second) && f.State.IsEmpty &&
                        f.State.Condition == FirearmCondition.Broken && f.Powder == 11 &&
                        costs.Exact(survivor, false) && costs.Exact(f.LastShotCommand, false) &&
                        ReferenceEquals(NativeFirearmAttackOrder.Get(f.LastShotCommand)?.Order, accepted) &&
                        f.SameItemIdentity && FirearmScheduledCallbacks() == 0,
                        f.Describe() + ";one native reload, one shot at B, no second click or extra resource/action cost");
                }
                f.Dispose();
                FirearmInputCheck(prefix + "cleanup", f.Restored, "Native fixture restored exactly.");
                _firearmInputFixture = null;
            }
        }
        private IEnumerable<object> FirearmReviewMergeCases()
        {
            foreach (object step in FirearmReviewReloadMergeCases()) yield return step;
            foreach (bool running in new[] { false, true })
            {
                string prefix = "cr02-" + (running ? "running-spent-merge-" : "paused-unstarted-replace-");
                _firearmInputStage = prefix + "fixture";
                // Use the shipped single-shot pistol. A real coexisting paper
                // reload supplies the loaded running-merge boundary after the
                // first shot, preserving its spent action and attack progress.
                _firearmInputFixture = new FirearmInputFixture(false, _firearmInputRows);
                var f = _firearmInputFixture;
                using (var costs = new FirearmReviewCosts(f.Actor, _firearmInputRows))
                {
                    foreach (object step in FirearmReviewBreak(f, prefix)) yield return step;
                    var manual = f.ClickAbility(BlueprintBootstrap.ReloadTestMusketAbility);
                    for (int tick = 0; tick < 240 && !manual.IsFinished; tick++)
                    { f.Pump(); yield return null; }
                    foreach (object step in DriveFirearmIdle(f)) yield return step;
                    FirearmInputCheck(prefix + "real-manual-reload", f.Shots.Count == 1 &&
                        f.State.LoadedRounds == 1 && f.Powder == 11 &&
                        f.State.Condition == FirearmCondition.Broken && costs.Exact(manual, false) &&
                        BrokenSequenceSuppressionRuntime.Orders.Current(f.Actor) == null,
                        f.Describe() + ";native Move reload alone leaves old sequence cancelled");
                    var second = f.SpawnAdditional(false, "NativeMergeTarget");
                    var cartridge = BlueprintBootstrap.BasicAmmunition.PaperCartridge;
                    if (running)
                    {
                        Game.Instance.Player.Inventory.Add(cartridge, 12);
                        f.Actor.Descriptor.ActivatableAbilities.Enumerable.Single(a =>
                            ReferenceEquals(a.Blueprint, BlueprintBootstrap.PaperCartridgeMode.Ability)).IsOn = true;
                    }
                    FirearmMisfireRuntime.QueueForcedNaturalRoll(10);
                    if (!running) f.Turns.SetFirearmPaused(true);
                    f.Click(f.Enemy);
                    var first = f.Actor.Commands.Raw.OfType<UnitAttack>().Single();
                    var original = BrokenSequenceSuppressionRuntime.Orders.Current(f.Actor);
                    var stale = f.CreateNativeAiCommand(f.Enemy);
                    if (running)
                    {
                        for (int tick = 0; tick < 240 && f.Shots.Count < 2; tick++)
                        { f.Pump(); yield return null; }
                        var refill = f.ClickAbility(BlueprintBootstrap.ReloadTestMusketAbility);
                        for (int tick = 0; tick < 240 && !refill.IsFinished; tick++)
                        { f.Pump(); yield return null; }
                        FirearmInputCheck(prefix + "coexisting-native-paper-reload", f.Shots.Count == 2 &&
                            f.State.LoadedRounds == 1 && f.State.Condition == FirearmCondition.Broken &&
                            refill.Type == UnitCommand.CommandType.Free && costs.Exact(refill, false) &&
                            Game.Instance.Player.Inventory.Count(cartridge) == 11 && f.Powder == 11 &&
                            f.Actor.Commands.Raw.Contains(first) && BrokenSequenceSuppressionRuntime.Orders.IsCurrent(original),
                            f.Describe() + ";native Free-slot reload retains the running attack and charges the native RTWP cooldown");
                        f.Turns.SetFirearmPaused(true);
                    }
                    var indexField = typeof(UnitAttack).GetField("m_AttackIndex", BindingFlags.Instance | BindingFlags.NonPublic);
                    int index = (int)indexField.GetValue(first), rounds = f.State.LoadedRounds;
                    var lastRule = first.LastAttackRule;
                    float elapsed = first.TimeSinceStart, standard = f.Actor.CombatState.Cooldown.StandardAction;
                    FirearmInputCheck(prefix + "before-input", first.IsStarted == running && first.IsActed == running &&
                        !first.IsFinished && f.Shots.Count == (running ? 2 : 1) &&
                        costs.Count(first) == (running ? 1 : 0) && (!running || index > 0),
                        f.Describe() + ";attackIndex=" + index + ";charges=" + costs.Count(first));
                    f.Click(second);
                    var accepted = BrokenSequenceSuppressionRuntime.Orders.Current(f.Actor);
                    var survivor = f.Actor.Commands.Raw.OfType<UnitAttack>().Single();
                    FirearmInputCheck(prefix + "native-outcome", accepted != null && original.Cancelled &&
                        !ReferenceEquals(accepted, original) && ReferenceEquals(accepted.Owner, survivor) &&
                        ReferenceEquals(NativeFirearmAttackOrder.Get(survivor)?.Order, accepted) &&
                        ReferenceEquals(survivor.Target, second) && ReferenceEquals(first, survivor) == running &&
                        f.State.LoadedRounds == rounds && f.Actor.CombatState.Cooldown.StandardAction == standard &&
                        (running ? (int)indexField.GetValue(survivor) == index &&
                            ReferenceEquals(survivor.LastAttackRule, lastRule) && survivor.TimeSinceStart == elapsed :
                            !survivor.IsStarted && !survivor.IsActed),
                        "Native TryMergeInto retains running progress; native replacement owns an unstarted paused command. No action or target assignment.");
                    f.Turns.Execute(() => f.Actor.Commands.Run(stale));
                    FirearmInputCheck(prefix + "stale-order-rejected", !f.Actor.Commands.Raw.Contains(stale) &&
                        !f.Actor.Commands.Queue.Contains(stale) && BrokenSequenceSuppressionRuntime.Orders.IsCurrent(accepted) &&
                        f.Actor.Commands.Raw.Contains(survivor), "The old target's captured native AI command cannot borrow the replacement order.");
                    FirearmMisfireRuntime.QueueForcedNaturalRoll(10);
                    f.Turns.SetFirearmPaused(false);
                    int expected = running ? 3 : 2;
                    for (int tick = 0; tick < 240 && f.Shots.Count < expected; tick++)
                    { f.Pump(); yield return null; }
                    FirearmInputCheck(prefix + "next-shot-normal-cost", f.Shots.Count == expected &&
                        ReferenceEquals(f.Shots.Last().Target, second) && ReferenceEquals(f.LastShotCommand, survivor) &&
                        f.State.LoadedRounds == rounds - 1 && f.State.Condition == FirearmCondition.Broken &&
                        f.Powder == 11 && (!running || Game.Instance.Player.Inventory.Count(cartridge) == 11) &&
                        costs.Exact(survivor, false) && f.SameItemIdentity,
                        f.Describe() + ";same one native attack charge; spent progress preserved; no extra attack/action");
                }
                f.Dispose();
                FirearmInputCheck(prefix + "cleanup", f.Restored, "Native fixture restored exactly.");
                _firearmInputFixture = null;
            }
        }
    }
}
