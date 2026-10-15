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
using Kingmaker.Visual.Animation.Actions;
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

        private sealed class KillChainLane
        {
            internal string Name;
            internal bool Loaded, Paused, FinalKill, NoReplacement, Broken, Repaired;
            internal bool Explicit = true;
        }

        private static readonly MethodInfo NativeAnimationFinish = typeof(AnimationActionHandle)
            .GetMethod("FinishInternal", BindingFlags.Instance | BindingFlags.NonPublic);

        // CR-04 (owner RTWP report): one native full attack with four planned
        // attacks against several legal enemies. Native UpdateTarget chooses
        // every replacement after real projectile damage and native death; no
        // target, attack progress, cooldown, action or order is assigned. Each
        // lane records its observations and restores its fixture before the
        // verdicts are asserted, so a failing lane cannot hide the others.
        private IEnumerable<object> FirearmReviewKillChainCases()
        {
            var verdicts = new List<KillChainVerdict>();
            var lanes = new[] {
                new KillChainLane { Name = "loaded", Loaded = true },
                new KillChainLane { Name = "empty" },
                new KillChainLane { Name = "loaded-paused", Loaded = true, Paused = true },
                new KillChainLane { Name = "empty-paused", Paused = true },
                new KillChainLane { Name = "final-kill", Loaded = true, FinalKill = true },
                new KillChainLane { Name = "ai-only-final-kill", Loaded = true, FinalKill = true, Explicit = false },
                new KillChainLane { Name = "broken-final-kill", FinalKill = true, Broken = true },
                new KillChainLane { Name = "repaired-final-kill", FinalKill = true, Broken = true, Repaired = true },
                new KillChainLane { Name = "no-replacement", Loaded = true, NoReplacement = true } };
            foreach (var lane in lanes)
                foreach (object step in FirearmKillChainLane(lane, verdicts)) yield return step;
            foreach (object step in FirearmRepairedReloadGapLane(verdicts)) yield return step;
            foreach (object step in OrdinaryBowKillChainControl(verdicts)) yield return step;
            foreach (var verdict in verdicts) FirearmInputCheck(verdict.Name, verdict.Passed, verdict.Detail);
        }

        private static KillChainVerdict KillChainCheck(string name, bool passed, string detail)
        {
            return new KillChainVerdict { Name = name, Passed = passed, Detail = detail };
        }

        // A is the clicked target. Native nearest-first selection is expected
        // to choose B, C, D, then E from these positions. The last unit is
        // durable, so a shot beyond the native fourth attack stays visible.
        // Without a replacement, the others stand beyond pistol range + 5 ft.
        private static UnitEntityData[] PrepareKillChainTargets(FirearmInputFixture f,
            bool finalKill, bool noReplacement)
        {
            var targets = new List<UnitEntityData> { f.Enemy, f.SpawnAdditional(false, "KillChainB"),
                f.SpawnAdditional(false, "KillChainC"), f.SpawnAdditional(false, "KillChainD") };
            if (finalKill) targets.Add(f.SpawnAdditional(false, "KillChainE"));
            Vector3[] positions = noReplacement
                ? new[] { Vector3.zero, new Vector3(0f, 0f, 14f), new Vector3(1f, 0f, 15f), new Vector3(-1f, 0f, 16f) }
                : new[] { Vector3.zero, new Vector3(1f, 0f, 2.5f), new Vector3(-1.5f, 0f, 3f),
                    new Vector3(2f, 0f, 3.5f), new Vector3(-2f, 0f, 4.5f) };
            for (int index = 1; index < targets.Count; index++) targets[index].Position = positions[index];
            for (int index = 0; index < targets.Count - 1; index++)
            {
                targets[index].Stats.Constitution.BaseValue = 1;
                targets[index].Stats.HitPoints.BaseValue = 1;
                targets[index].Descriptor.State.Immortality.Release();
            }
            return targets.ToArray();
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

        // The save-free host plays no animation clips, so the fixture already
        // supplies each native act cue. It likewise supplies the finish cue
        // after the act, letting native UnitCommand.Tick end a completed
        // sequence with Success as live animation playback does.
        private static void KillChainFinishActedAnimations(FirearmInputFixture f, UnitEntityData unit)
        {
            if (NativeAnimationFinish == null) throw new MissingMethodException("Native animation finish boundary.");
            f.Turns.Execute(() => {
                foreach (var command in unit.Commands.Raw.Where(c => c != null && c.Animation != null &&
                    c.Animation.IsActed && !c.Animation.IsFinished).ToArray())
                    NativeAnimationFinish.Invoke(command.Animation, null);
            });
        }

        // One fixture frame in native GameModesFactory order: scheduled
        // callbacks, AI, unit actions with same-frame ability delivery,
        // projectiles, hits, then life. Natural 10 is the disclosed roll
        // control for firearm attacks (hit, no misfire).
        private static void KillChainPump(FirearmInputFixture f, UnitEntityData[] targets, bool ai)
        {
            if (!FirearmMisfireRuntime.PendingForcedNaturalRoll.HasValue)
                FirearmMisfireRuntime.QueueForcedNaturalRoll(10);
            f.Turns.FlushFirearmCallbacks();
            if (ai) f.Turns.Execute(() => new Kingmaker.Controllers.Brain.AiBrainController().Tick());
            f.Pump(false);
            KillChainResolveImpacts(f, targets);
            KillChainFinishActedAnimations(f, f.Actor);
        }

        // Ordinary pistol, matching Rapid Reload and paper cartridges: the
        // existing Free reload. BAB 16 gives four native attacks.
        private static UnitEntityData[] KillChainArmPistol(FirearmInputFixture f, bool finalKill, bool noReplacement)
        {
            f.Actor.Descriptor.AddFact(BlueprintBootstrap.GunslingerClass.GunTraining.ChoiceFor(FirearmKind.Pistol));
            f.Actor.Stats.Dexterity.BaseValue = 30;
            f.Actor.Stats.BaseAttackBonus.BaseValue = 16;
            var targets = PrepareKillChainTargets(f, finalKill, noReplacement);
            Game.Instance.Player.Inventory.Add(BlueprintBootstrap.BasicAmmunition.PaperCartridge, 12);
            f.Actor.Descriptor.ActivatableAbilities.Enumerable.Single(a =>
                ReferenceEquals(a.Blueprint, BlueprintBootstrap.PaperCartridgeMode.Ability)).IsOn = true;
            return targets;
        }

        // The production Quick Clear (native action bar, standard action)
        // repairs the misfire-Broken item. Repair alone neither resurrects nor
        // releases the cancelled order, and the degradation epoch stays.
        private IEnumerable<object> KillChainQuickClear(FirearmInputFixture f, string prefix,
            FirearmReviewCosts costs, List<KillChainVerdict> verdicts)
        {
            var gunslinger = BlueprintBootstrap.GunslingerClass;
            f.Actor.Descriptor.AddFact(gunslinger.Grit.Feature);
            f.Actor.Descriptor.AddFact(gunslinger.QuickClear.Feature);
            f.Actor.Descriptor.Resources.Restore(gunslinger.Grit.Resource);
            int epoch = BrokenSequenceSuppressionRuntime.GetDegradationEpoch(f.Actor, f.Weapon);
            int grit = f.Actor.Descriptor.Resources.GetResourceAmount(gunslinger.Grit.Resource);
            int shots = f.Shots.Count;
            var broken = f.State.Condition;
            var quickClear = f.ClickAbility(gunslinger.QuickClear.StandardAbility);
            for (int tick = 0; tick < 240 && !quickClear.IsFinished; tick++) { f.Pump(); yield return null; }
            foreach (object step in DriveFirearmIdle(f)) yield return step;
            verdicts.Add(KillChainCheck(prefix + "quick-clear-normal-nonzero-epoch",
                broken == FirearmCondition.Broken && quickClear.IsFinished &&
                quickClear.Result == UnitCommand.ResultType.Success && f.State.Condition == FirearmCondition.Normal &&
                epoch > 0 && BrokenSequenceSuppressionRuntime.GetDegradationEpoch(f.Actor, f.Weapon) == epoch &&
                BrokenSequenceSuppressionRuntime.IsSuppressed(f.Actor, f.Weapon) &&
                BrokenSequenceSuppressionRuntime.Orders.RequiresOrder(f.Actor, f.Weapon) &&
                BrokenSequenceSuppressionRuntime.Orders.Current(f.Actor) == null && f.Shots.Count == shots &&
                f.Actor.Descriptor.Resources.GetResourceAmount(gunslinger.Grit.Resource) == grit &&
                costs.Exact(quickClear, false), f.Describe() + ";epoch=" + epoch + ";gritBefore=" + grit));
        }

        private IEnumerable<object> FirearmKillChainLane(KillChainLane lane, List<KillChainVerdict> verdicts)
        {
            string prefix = "cr04-" + lane.Name + "-";
            _firearmInputStage = prefix + "fixture";
            _firearmInputFixture = new FirearmInputFixture(false, _firearmInputRows,
                initiallyLoaded: lane.Loaded || lane.Broken);
            var f = _firearmInputFixture;
            // A real natural-1 misfire commits Broken and stops that sequence;
            // the lane's explicit order is then a legitimate new Broken order,
            // or, after Quick Clear, a new order on the repaired Normal item.
            if (lane.Broken) foreach (object step in FirearmReviewBreak(f, prefix)) yield return step;
            var condition = lane.Broken && !lane.Repaired ? FirearmCondition.Broken : FirearmCondition.Normal;
            using (var costs = new FirearmReviewCosts(f.Actor, _firearmInputRows))
            using (var trace = new FirearmKillChainTrace(f.Actor, _firearmInputRows, prefix))
            {
                if (lane.Repaired)
                    foreach (object step in KillChainQuickClear(f, prefix, costs, verdicts)) yield return step;
                int epoch = BrokenSequenceSuppressionRuntime.GetDegradationEpoch(f.Actor, f.Weapon);
                var targets = KillChainArmPistol(f, lane.FinalKill, lane.NoReplacement);
                var cartridge = BlueprintBootstrap.BasicAmmunition.PaperCartridge;
                for (int tick = 0; tick < 16; tick++) { f.Turns.PumpCommands(false); yield return null; }
                int planned = 0;
                f.Turns.Execute(() => planned = UnitAttack.EstimateFullAttacks(f.Actor));
                int cartridges = Game.Instance.Player.Inventory.Count(cartridge);
                int loadedAtSetup = f.State.LoadedRounds;
                long attempted = FreeActionFullAttackReloadPatch.Attempted;
                long reloads = FreeActionFullAttackReloadPatch.Succeeded;
                long ended = FreeActionFullAttackReloadPatch.Interrupted;
                long faults = FreeActionFullAttackReloadPatch.Failed;
                bool ready = f.Actor.Commands.Empty && planned == 4 &&
                    !f.Actor.CombatState.IsFullAttackRestrictedBecauseOfMoveAction &&
                    f.State.LoadedRounds == (lane.Loaded ? 1 : 0) && f.State.Condition == condition &&
                    ReferenceEquals(f.Actor.AutoUseAbility?.Blueprint, BlueprintBootstrap.ReloadTestMusketAbility);
                string setup = "planned=" + planned + ";loadedRounds=" + f.State.LoadedRounds +
                    ";cartridges=" + cartridges + ";autoUseReload=" +
                    ReferenceEquals(f.Actor.AutoUseAbility?.Blueprint, BlueprintBootstrap.ReloadTestMusketAbility) +
                    ";restricted=" + f.Actor.CombatState.IsFullAttackRestrictedBecauseOfMoveAction +
                    ";distances=" + string.Join(",", targets.Select(t => f.Actor.DistanceTo(t).ToString("F2")).ToArray());
                verdicts.Add(KillChainCheck(prefix + "native-ready", ready, setup));

                FirearmAttackOrderLedger.Order order = null;
                if (lane.Explicit)
                {
                    if (lane.Paused) f.Turns.SetFirearmPaused(true);
                    f.Click(targets[0]);
                    order = BrokenSequenceSuppressionRuntime.Orders.Current(f.Actor);
                    trace.Note("click", f, null, targets);
                    if (lane.Paused)
                    {
                        for (int frame = 0; frame < 3; frame++) yield return null;
                        trace.Note("paused-frames", f, null, targets);
                        f.Turns.SetFirearmPaused(false);
                    }
                }
                // Party AI stays enabled as in ordinary RTWP play; it cannot
                // preempt a started player command. Distant units in the
                // no-replacement lane would otherwise draw an approach order.
                bool ai = !lane.NoReplacement;
                UnitAttack chain = null;
                for (int tick = 0; tick < 200 && (chain == null || !chain.IsFinished); tick++)
                {
                    KillChainPump(f, targets, ai);
                    if (chain == null && trace.Shots.Count != 0) chain = trace.Shots[0].Command;
                    trace.Sample(tick, f, chain, targets);
                    yield return null;
                }
                trace.Note("chain-finished", f, chain, targets);
                var own = trace.Shots.Where(s => ReferenceEquals(s.Command, chain)).ToArray();
                int chainPhaseShots = trace.Shots.Count, chainCancellations = trace.Cancellations;
                int spent = cartridges - Game.Instance.Player.Inventory.Count(cartridge);
                bool orderFollowed = order != null && BrokenSequenceSuppressionRuntime.Orders.IsCurrent(order) &&
                    ReferenceEquals(order.Owner, chain) && ReferenceEquals(order.Target, own.LastOrDefault()?.Target);
                // Snapshot the chain's own economy before the next cycle spends more.
                bool economy = spent == (lane.Loaded ? 3 : 4) && f.State.Condition == condition &&
                    FreeActionFullAttackReloadPatch.Succeeded - reloads == 3 &&
                    FreeActionFullAttackReloadPatch.Interrupted == ended &&
                    FreeActionFullAttackReloadPatch.Failed == faults && chain != null && costs.Exact(chain, false);
                string detail = setup + ";" + trace.Summary(chain, targets) + ";spentCartridges=" + spent +
                    ";inlineReloads=" + (FreeActionFullAttackReloadPatch.Succeeded - reloads) +
                    ";reloadAttempts=" + (FreeActionFullAttackReloadPatch.Attempted - attempted) +
                    ";endedFullAttacks=" + (FreeActionFullAttackReloadPatch.Interrupted - ended) +
                    ";reloadFaults=" + (FreeActionFullAttackReloadPatch.Failed - faults) +
                    ";orderCurrent=" + BrokenSequenceSuppressionRuntime.Orders.IsCurrent(order) +
                    ";orderOwnerIsChain=" + (order != null && ReferenceEquals(order.Owner, chain)) +
                    ";orderTarget=" + (order?.Target as UnitEntityData)?.UniqueId + ";" + f.Describe();

                // The next ordinary native cycle, distinct from the chain.
                for (int tick = 0; tick < 60; tick++)
                {
                    KillChainPump(f, targets, ai);
                    trace.Sample(200 + tick, f, chain, targets);
                    yield return null;
                }
                trace.Note("next-cycle", f, chain, targets);
                var next = trace.Shots.Skip(chainPhaseShots).ToArray();
                string cycle = detail + ";nextCycle=" + string.Join(",", next.Select(s =>
                    Array.IndexOf(targets, s.Target) + (s.Hit ? "h" : "m") +
                    (ReferenceEquals(s.Command, chain) ? "@chain" : "@new")).ToArray());

                if (lane.NoReplacement)
                {
                    verdicts.Add(KillChainCheck(prefix + "clean-native-completion",
                        chain != null && chain.IsFinished && chain.GetAttackIndex() == 1 && own.Length == 1 &&
                        ReferenceEquals(own[0].Target, targets[0]) && targets[0].Descriptor.State.IsDead &&
                        targets.Skip(1).All(t => !t.Descriptor.State.IsDead) &&
                        FreeActionFullAttackReloadPatch.Attempted == attempted && spent == 0 &&
                        next.Length == 0 && f.Actor.Commands.Empty && f.State.IsEmpty, cycle));
                }
                else
                {
                    var expected = lane.Explicit ? targets.Take(4).ToArray() : null;
                    verdicts.Add(KillChainCheck(prefix + "one-command-four-native-attacks",
                        chain != null && chain.IsFinished && chain.Result == UnitCommand.ResultType.Success &&
                        chain.IsFullAttack && chain.AllAttacks.Count == 4 && chain.GetAttackIndex() == 4 &&
                        own.Length == 4 && chainPhaseShots == 4 && own.All(s => s.Hit) &&
                        own.Select(s => s.AttackNumber).SequenceEqual(new[] { 0, 1, 2, 3 }) &&
                        (expected == null || own.Select(s => s.Target).SequenceEqual(expected)) &&
                        own.Select(s => s.Target).Distinct().Count() == 4, detail));
                    int mortal = lane.FinalKill ? 4 : 3;
                    verdicts.Add(KillChainCheck(prefix + "native-replacements-after-kills",
                        targets.Take(mortal).All(t => t.Descriptor.State.IsDead) &&
                        !targets.Last().Descriptor.State.IsDead, detail));
                    verdicts.Add(KillChainCheck(prefix + "ammunition-and-action-economy", economy, detail));
                    if (lane.Explicit)
                        verdicts.Add(KillChainCheck(prefix + "order-follows-surviving-command",
                            orderFollowed && chainCancellations == 0, detail));
                    if (condition == FirearmCondition.Normal)
                        verdicts.Add(KillChainCheck(prefix + "next-native-cycle-continues",
                            next.Length != 0 && next.All(s => !ReferenceEquals(s.Command, chain)) &&
                            ReferenceEquals(next[0].Target, targets.Last()), cycle));
                    if (lane.FinalKill && condition == FirearmCondition.Normal)
                    {
                        // Every cartridge spent became exactly one shot or the
                        // round still chambered; each new native command was
                        // charged once, exactly; no reload faulted.
                        int spentAll = cartridges - Game.Instance.Player.Inventory.Count(cartridge);
                        var nextCommands = next.Select(s => s.Command).Distinct().ToArray();
                        verdicts.Add(KillChainCheck(prefix + "next-cycle-exact-economy",
                            nextCommands.Length != 0 && nextCommands.All(c => costs.Exact(c, false)) &&
                            spentAll == trace.Shots.Count + f.State.LoadedRounds - loadedAtSetup &&
                            FreeActionFullAttackReloadPatch.Failed == faults && f.State.Condition == condition,
                            cycle + ";spentAll=" + spentAll + ";loadedNow=" + f.State.LoadedRounds +
                            ";nextCommands=" + nextCommands.Length));
                    }
                    if (lane.Repaired)
                    {
                        // Normal completion, not cancellation: no current order
                        // was cancelled, suppression stayed off, and the
                        // degradation epoch was not reset by repair or completion.
                        verdicts.Add(KillChainCheck(prefix + "completed-without-suppression",
                            order != null && order.Epoch == epoch && epoch > 0 &&
                            !BrokenSequenceSuppressionRuntime.Orders.IsCurrent(order) &&
                            BrokenSequenceSuppressionRuntime.Orders.Current(f.Actor) == null &&
                            trace.Cancellations == chainCancellations &&
                            !BrokenSequenceSuppressionRuntime.IsSuppressed(f.Actor, f.Weapon) &&
                            !BrokenSequenceSuppressionRuntime.Orders.RequiresOrder(f.Actor, f.Weapon) &&
                            BrokenSequenceSuppressionRuntime.GetDegradationEpoch(f.Actor, f.Weapon) == epoch &&
                            f.State.Condition == FirearmCondition.Normal, cycle + ";epoch=" + epoch));
                    }
                }
                if (condition == FirearmCondition.Broken)
                {
                    // A degraded firearm still needs a new explicit order once
                    // its accepted target is gone; that order then fires.
                    verdicts.Add(KillChainCheck(prefix + "degraded-order-unchanged", next.Length == 0 &&
                        BrokenSequenceSuppressionRuntime.Orders.IsCurrent(order) &&
                        ReferenceEquals(order.Target, targets[3]) && targets[3].Descriptor.State.IsDead, cycle));
                    int before = trace.Shots.Count;
                    f.Click(targets.Last());
                    var reorder = BrokenSequenceSuppressionRuntime.Orders.Current(f.Actor);
                    for (int tick = 0; tick < 80 && trace.Shots.Count == before; tick++)
                    { KillChainPump(f, targets, true); yield return null; }
                    trace.Note("explicit-broken-reattack", f, chain, targets);
                    var shot = trace.Shots.Skip(before).FirstOrDefault();
                    verdicts.Add(KillChainCheck(prefix + "explicit-broken-reattack", shot != null &&
                        !ReferenceEquals(shot.Command, chain) && ReferenceEquals(shot.Target, targets.Last()) &&
                        reorder != null && !ReferenceEquals(reorder, order) &&
                        ReferenceEquals(NativeFirearmAttackOrder.Get(shot.Command)?.Order, reorder) &&
                        f.State.Condition == FirearmCondition.Broken, trace.Summary(chain, targets) + ";" + f.Describe()));
                }
            }
            f.Dispose();
            verdicts.Add(KillChainCheck(prefix + "cleanup", f.Restored, "Native fixture restored exactly."));
            _firearmInputFixture = null;
        }

        // On the repaired Normal item, the explicit order's target dies after
        // its auto-use reload ended but before the scheduled resume runs, as
        // an ally's hit lands in native frame order (native damage, native
        // life controller). The resume completes the order instead of
        // cancelling it, so native AI carries on without another click.
        private IEnumerable<object> FirearmRepairedReloadGapLane(List<KillChainVerdict> verdicts)
        {
            const string prefix = "cr04-repaired-reload-gap-kill-";
            _firearmInputStage = prefix + "fixture";
            _firearmInputFixture = new FirearmInputFixture(false, _firearmInputRows, initiallyLoaded: true);
            var f = _firearmInputFixture;
            foreach (object step in FirearmReviewBreak(f, prefix)) yield return step;
            using (var costs = new FirearmReviewCosts(f.Actor, _firearmInputRows))
            using (var trace = new FirearmKillChainTrace(f.Actor, _firearmInputRows, prefix))
            {
                foreach (object step in KillChainQuickClear(f, prefix, costs, verdicts)) yield return step;
                int epoch = BrokenSequenceSuppressionRuntime.GetDegradationEpoch(f.Actor, f.Weapon);
                var targets = KillChainArmPistol(f, false, false);
                var cartridge = BlueprintBootstrap.BasicAmmunition.PaperCartridge;
                for (int tick = 0; tick < 16; tick++) { f.Turns.PumpCommands(false); yield return null; }
                int cartridges = Game.Instance.Player.Inventory.Count(cartridge);
                f.Click(targets[0]);
                var order = BrokenSequenceSuppressionRuntime.Orders.Current(f.Actor);
                var reload = f.Actor.Commands.Raw.Concat(f.Actor.Commands.Queue).OfType<UnitUseAbility>().SingleOrDefault();
                trace.Note("click", f, null, targets);
                for (int tick = 0; tick < 20 && reload != null && !reload.IsFinished; tick++)
                { KillChainPump(f, targets, true); yield return null; }
                int callbacks = FirearmScheduledCallbacks();
                bool boundary = order != null && order.Epoch == epoch && epoch > 0 &&
                    BrokenSequenceSuppressionRuntime.Orders.IsCurrent(order) && ReferenceEquals(order.Owner, reload) &&
                    reload.IsFinished && reload.Result == UnitCommand.ResultType.Success && callbacks == 1 &&
                    f.State.LoadedRounds == 1 && trace.Shots.Count == 0 && costs.Exact(reload, false);
                targets[0].Descriptor.Damage = 20;
                KillChainResolveImpacts(f, targets);
                trace.Note("gap-kill", f, null, targets);
                verdicts.Add(KillChainCheck(prefix + "reload-gap-boundary", boundary &&
                    targets[0].Descriptor.State.IsDead && FirearmScheduledCallbacks() == 1,
                    trace.Summary(null, targets) + ";callbacks=" + callbacks + ";" + f.Describe()));
                for (int tick = 0; tick < 80 && trace.Shots.Count < 2; tick++)
                { KillChainPump(f, targets, true); trace.Sample(tick, f, null, targets); yield return null; }
                trace.Note("after-gap", f, null, targets);
                string detail = trace.Summary(null, targets) + ";epoch=" + epoch + ";" + f.Describe();
                verdicts.Add(KillChainCheck(prefix + "resume-completes-without-suppression",
                    !BrokenSequenceSuppressionRuntime.Orders.IsCurrent(order) &&
                    BrokenSequenceSuppressionRuntime.Orders.Current(f.Actor) == null && trace.Cancellations == 0 &&
                    !BrokenSequenceSuppressionRuntime.IsSuppressed(f.Actor, f.Weapon) &&
                    !BrokenSequenceSuppressionRuntime.Orders.RequiresOrder(f.Actor, f.Weapon) &&
                    BrokenSequenceSuppressionRuntime.GetDegradationEpoch(f.Actor, f.Weapon) == epoch &&
                    f.State.Condition == FirearmCondition.Normal, detail));
                var first = trace.Shots.FirstOrDefault();
                int spent = cartridges - Game.Instance.Player.Inventory.Count(cartridge);
                verdicts.Add(KillChainCheck(prefix + "native-ai-continues-exactly",
                    first != null && first.Command.AiAction != null && first.Hit &&
                    !ReferenceEquals(first.Target, targets[0]) && targets.Skip(1).Contains(first.Target) &&
                    trace.Shots.Select(s => s.Command).Distinct().All(c => costs.Exact(c, false)) &&
                    spent == trace.Shots.Count + f.State.LoadedRounds,
                    detail + ";spentCartridges=" + spent));
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
            var targets = PrepareKillChainTargets(f, false, false);
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
                    KillChainFinishActedAnimations(f, ally);
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
            private int rejectedExecutions, rejectedSubmissions;
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
                    Patch(typeof(NativeFirearmAttackOrder).GetMethod("Submitting", All), null, "Submitted");
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
                    ";orderCancellations=" + Cancellations + ";rejectedExecutions=" + rejectedExecutions +
                    ";rejectedSubmissions=" + rejectedSubmissions;
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
            private static void Submitted(UnitEntityData __0, UnitCommand __1, bool __result)
            {
                if (active == null || __result || __1 == null || !ReferenceEquals(__0, active.actor) ||
                    active.rejectedSubmissions++ > 12) return;
                var binding = NativeFirearmAttackOrder.Get(__1);
                var order = binding == null ? null : binding.Order;
                var orderTarget = order == null ? null : order.Target as UnitEntityData;
                var owner = order == null ? null : order.Owner as UnitCommand;
                active.Add(new JObject { ["killChainSubmitRejected"] = Id(__1), ["ai"] = __1.AiAction != null,
                    ["commandTarget"] = UnitId((__1 as UnitAttack)?.Target),
                    ["bindingTarget"] = binding == null ? null : UnitId(binding.Target),
                    ["order"] = Id(order), ["orderTarget"] = UnitId(orderTarget),
                    ["orderTargetDead"] = orderTarget != null && orderTarget.Descriptor.State.IsDead,
                    ["orderOwner"] = Id(owner), ["orderOwnerFinished"] = owner != null && owner.IsFinished,
                    ["orderCurrent"] = BrokenSequenceSuppressionRuntime.Orders.IsCurrent(order), ["stack"] = Stack() });
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
