using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Firing;
using KingmakerGunslinger.Misfires;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<object> FirearmReviewBreak(FirearmInputFixture f, string prefix)
        {
            FirearmMisfireRuntime.QueueForcedNaturalRoll(1);
            f.Click(f.Enemy);
            for (int tick = 0; tick < 240 && f.Shots.Count == 0; tick++)
            { f.Pump(); f.Record(tick); yield return null; }
            int powder = f.Powder;
            foreach (object step in DriveFirearmIdle(f)) yield return step;
            FirearmInputCheck(prefix + "real-break-stopped", f.Shots.Count == 1 &&
                f.State.Condition == FirearmCondition.Broken && f.State.IsEmpty &&
                f.Powder == powder && BrokenSequenceSuppressionRuntime.IsSuppressed(f.Actor, f.Weapon), f.Describe());
        }
        private IEnumerable<object> FirearmReviewCoexistenceCases()
        {
            foreach (bool turnBased in new[] { false, true })
            foreach (string state in new[] { "normal", "broken-loaded", "broken-empty" })
            foreach (bool running in new[] { false, true })
            {
                string prefix = "cr01-" + (turnBased ? "tb-" : "rtwp-") + state +
                    (running ? "-running-" : "-pending-");
                _firearmInputStage = prefix + "fixture";
                _firearmInputFixture = new FirearmInputFixture(turnBased, _firearmInputRows);
                var f = _firearmInputFixture;
                using (var costs = new FirearmReviewCosts(f.Actor, _firearmInputRows))
                {
                    if (state != "normal")
                    {
                        foreach (object step in FirearmReviewBreak(f, prefix)) yield return step;
                        if (turnBased) { f.Turns.EndCurrentTurn(); f.Turns.ReachCasterTurn(); }
                        if (state == "broken-loaded")
                        {
                            var manual = f.ClickAbility(BlueprintBootstrap.ReloadTestMusketAbility);
                            for (int tick = 0; tick < 240 && f.State.IsEmpty; tick++)
                            { f.Pump(); f.Record(tick); yield return null; }
                            foreach (object step in DriveFirearmIdle(f)) yield return step;
                            FirearmInputCheck(prefix + "manual-reload-alone", f.Shots.Count == 1 &&
                                f.State.Condition == FirearmCondition.Broken && f.State.LoadedRounds == 1 &&
                                BrokenSequenceSuppressionRuntime.Orders.Current(f.Actor) == null &&
                                costs.Exact(manual, turnBased), f.Describe() + ";exact native reload cost");
                        }
                    }
                    var gunslinger = BlueprintBootstrap.GunslingerClass;
                    var stranger = gunslinger.MysteriousStranger;
                    f.Actor.Stats.Charisma.BaseValue = 14;
                    f.Actor.Descriptor.AddFact(stranger.Grit);
                    f.Actor.Descriptor.AddFact(stranger.FocusedAim);
                    // Begin with the real resource's fully restored maximum.
                    // Fact activation initially restores before its amount-bonus
                    // handler is active; no resource is restored after spending.
                    f.Actor.Descriptor.Resources.Restore(gunslinger.Grit.Resource);
                    var ability = stranger.FocusedAim.ComponentsArray.OfType<AddFacts>().Single()
                        .Facts.OfType<BlueprintAbility>().Single();
                    int grit = f.Actor.Descriptor.Resources.GetResourceAmount(gunslinger.Grit.Resource);
                    FirearmInputCheck(prefix + "native-ability-ready", grit >= 1 &&
                        f.Actor.Descriptor.Abilities.GetAbility(ability).Data.IsAvailable &&
                        ability.ActionType == UnitCommand.CommandType.Swift,
                        "Real granted Focused Aim; grit=" + grit + "; action=" + ability.ActionType);
                    UnitUseAbility swift = null;
                    BlueprintAbility rejectedAbility = null;
                    if (turnBased)
                    {
                        // Native TB hotbar requires Commands.Empty. Execute the
                        // legal Swift first, then verify another otherwise-ready
                        // personal ability is rejected while the attack is owned.
                        swift = f.ClickAbility(ability);
                        for (int tick = 0; tick < 240 && (!swift.IsFinished ||
                            f.Actor.Descriptor.Resources.GetResourceAmount(gunslinger.Grit.Resource) == grit); tick++)
                        { f.Pump(); f.Record(tick); yield return null; }
                        FirearmInputCheck(prefix + "swift-before-attack", swift.IsActed &&
                            costs.Exact(swift, true) && f.Actor.Commands.Empty &&
                            f.Actor.Descriptor.Resources.GetResourceAmount(gunslinger.Grit.Resource) == grit - 1,
                            "Native TB hotbar accepted Focused Aim while idle; exact Swift charge and one grit spent.");
                        f.Actor.Descriptor.AddFact(stranger.ClippingShot);
                        rejectedAbility = stranger.ClippingShot.ComponentsArray.OfType<AddFacts>().Single()
                            .Facts.OfType<BlueprintAbility>().Single();
                    }
                    if (!turnBased && !running) f.Turns.SetFirearmPaused(true);
                    int shots = f.Shots.Count, powder = f.Powder;
                    FirearmMisfireRuntime.QueueForcedNaturalRoll(10);
                    f.Click(f.Enemy);
                    var command = f.Actor.Commands.Raw.Single(c => c != null && NativeFirearmAttackOrder.Get(c)?.Proposal == false);
                    var order = BrokenSequenceSuppressionRuntime.Orders.Current(f.Actor);
                    if (running)
                        for (int tick = 0; tick < 240 && !command.IsStarted; tick++)
                        { f.Pump(); f.Record(tick); yield return null; }
                    FirearmInputCheck(prefix + "accepted-attack", order != null && !command.IsFinished &&
                        command.IsStarted == running && f.Shots.Count == shots &&
                        (turnBased || running || f.LastClickPaused), f.Describe());
                    long submission = BrokenSequenceSuppressionRuntime.Orders.Submission(f.Actor);
                    var realCommands = f.Actor.Commands;
                    UnitCommands temporary = null;
                    using (new UnitCommands.Temporary(ref temporary, f.Actor)) f.Click(f.Enemy, true);
                    FirearmInputCheck(prefix + "preview-preserves-owner", ReferenceEquals(realCommands, f.Actor.Commands) &&
                        BrokenSequenceSuppressionRuntime.Orders.IsCurrent(order) &&
                        BrokenSequenceSuppressionRuntime.Orders.Submission(f.Actor) == submission &&
                        !order.OwnerSlotRemoved, "Native preview creation/disposal cannot revoke the accepted real command container.");
                    bool bothOwned;
                    if (turnBased)
                    {
                        var data = f.Actor.Descriptor.Abilities.GetAbility(rejectedAbility).Data;
                        int fortune = f.Actor.Descriptor.Resources.GetResourceAmount(gunslinger.Grit.Resource);
                        bool ready = data.IsAvailable;
                        var rejected = f.ClickAbility(rejectedAbility, false);
                        FirearmInputCheck(prefix + "native-hotbar-rejects-during-command", ready &&
                            rejected == null && f.Actor.Commands.Raw.Contains(command) &&
                            f.Actor.Descriptor.Resources.GetResourceAmount(gunslinger.Grit.Resource) == fortune &&
                            !HasBuff(f.Actor.Descriptor, stranger.ClippingShotBuff),
                            "Native CanEndTurnAndNoActing requires Commands.Empty; ready=" + ready +
                            ";rejected=" + (rejected == null) + ";attackOwned=" + f.Actor.Commands.Raw.Contains(command) +
                            ";gritBefore=" + fortune + ";gritAfter=" + f.Actor.Descriptor.Resources.GetResourceAmount(gunslinger.Grit.Resource) +
                            ";armed=" + HasBuff(f.Actor.Descriptor, stranger.ClippingShotBuff));
                        bothOwned = f.Actor.Commands.Raw.Contains(command);
                    }
                    else
                    {
                        swift = f.ClickAbility(ability);
                        bothOwned = f.Actor.Commands.Raw.Contains(command) &&
                            (f.Actor.Commands.Raw.Contains(swift) || f.Actor.Commands.Queue.Contains(swift));
                    }
                    bool retained = BrokenSequenceSuppressionRuntime.Orders.IsCurrent(order);
                    long afterSubmission = BrokenSequenceSuppressionRuntime.Orders.Submission(f.Actor);
                    _firearmInputRows.Add(new JObject { ["review"] = "CR-01", ["case"] = prefix,
                        ["nativeBothOwned"] = !turnBased && bothOwned, ["nativeTbHotbarSerial"] = turnBased, ["swiftQueued"] = f.Actor.Commands.Queue.Contains(swift),
                        ["orderRetained"] = retained, ["submissionBefore"] = submission,
                        ["submissionAfter"] = afterSubmission });
                    yield return null; yield return null;
                    if (!turnBased && !running) f.Turns.SetFirearmPaused(false);
                    for (int tick = 0; tick < 240 && (f.Shots.Count == shots ||
                        f.Actor.Descriptor.Resources.GetResourceAmount(gunslinger.Grit.Resource) == grit); tick++)
                    {
                        if (!FirearmMisfireRuntime.PendingForcedNaturalRoll.HasValue) FirearmMisfireRuntime.QueueForcedNaturalRoll(10);
                        f.Pump(); f.Record(tick); yield return null;
                    }
                    FirearmInputCheck(prefix + (turnBased ? "native-turn-economy" : "native-coexistence"), bothOwned && swift.IsActed &&
                        f.Actor.Descriptor.Resources.GetResourceAmount(gunslinger.Grit.Resource) == grit - 1 &&
                        costs.Exact(swift, turnBased), f.Describe() + ";swift=" + swift.Result +
                        ";gritBefore=" + grit + ";gritAfter=" + f.Actor.Descriptor.Resources.GetResourceAmount(gunslinger.Grit.Resource) +
                        ";exact native Swift cost; chargeCount=" + costs.Count(swift));
                    FirearmInputCheck(prefix + "valid-attack-retained", retained && submission == afterSubmission &&
                        f.Shots.Count == shots + 1 &&
                        ReferenceEquals(NativeFirearmAttackOrder.Get(f.LastShotCommand)?.Order, order) &&
                        f.State.Condition == (state == "normal" ? FirearmCondition.Normal : FirearmCondition.Broken) &&
                        f.SameItemIdentity && f.Powder == powder - (state == "broken-empty" ? 1 : 0) &&
                        costs.Exact(f.LastShotCommand, turnBased) &&
                        (state != "broken-empty" || (command is UnitUseAbility && costs.Exact(command, turnBased))),
                        f.Describe() + ";orderRetained=" + retained + ";submissionBefore=" + submission +
                        ";submissionAfter=" + afterSubmission + ";exact native attack/reload cost");
                }
                f.Dispose();
                FirearmInputCheck(prefix + "cleanup", f.Restored, "Native fixture restored exactly.");
                _firearmInputFixture = null;
            }
        }
    }
}
