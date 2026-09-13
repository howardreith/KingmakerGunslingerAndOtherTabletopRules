using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Utility;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Firing;
using KingmakerGunslinger.Misfires;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<object> FirearmReviewOwnershipCases()
        {
            foreach (string action in new[] { "coexist-after-reload", "replace-after-reload", "cancel-after-reload", "replace-pending-reload" })
            {
                string prefix = "cr01-" + action + "-";
                bool pending = action == "replace-pending-reload", coexist = action == "coexist-after-reload";
                _firearmInputStage = prefix + "fixture";
                _firearmInputFixture = new FirearmInputFixture(false, _firearmInputRows);
                var f = _firearmInputFixture;
                var gunslinger = BlueprintBootstrap.GunslingerClass;
                f.Actor.Descriptor.AddFact(gunslinger.Grit.Feature);
                f.Actor.Descriptor.AddFact(gunslinger.QuickClear.Feature);
                f.Actor.Descriptor.AddFact(gunslinger.MysteriousStranger.FocusedAim);
                f.Actor.Descriptor.Resources.Restore(gunslinger.Grit.Resource);
                using (var costs = new FirearmReviewCosts(f.Actor, _firearmInputRows))
                {
                    foreach (object step in FirearmReviewBreak(f, prefix)) yield return step;
                    if (pending) f.Turns.SetFirearmPaused(true);
                    f.Click(f.Enemy);
                    var reload = f.Actor.Commands.Raw.OfType<UnitUseAbility>().Single();
                    var order = BrokenSequenceSuppressionRuntime.Orders.Current(f.Actor);
                    if (!pending)
                        for (int tick = 0; tick < 240 && !reload.IsFinished; tick++)
                        { f.Pump(); yield return null; }
                    FirearmInputCheck(prefix + "native-boundary", order != null &&
                        BrokenSequenceSuppressionRuntime.Orders.IsCurrent(order) &&
                        (pending ? !reload.IsStarted && f.State.IsEmpty && costs.Count(reload) == 0 :
                            reload.IsFinished && f.State.LoadedRounds == 1 && costs.Exact(reload, false) && FirearmScheduledCallbacks() == 1),
                        f.Describe() + ";pending=" + pending + ";callbacks=" + FirearmScheduledCallbacks());
                    // AI may have already accepted the legitimate attack
                    // continuation in the reload-completion tick. Snapshot the
                    // actual boundary owner/submission, not the earlier reload.
                    long submission = BrokenSequenceSuppressionRuntime.Orders.Submission(f.Actor);
                    var boundaryOwner = order.Owner;
                    // Native Run rejects an already-initialized different
                    // executor before any slot or queue mutation.
                    var rejected = new UnitUseAbility(f.Actor.AutoUseAbility, new TargetWrapper(f.Actor));
                    rejected.Init(f.Enemy);
                    f.Turns.Execute(() => f.Actor.Commands.Run(rejected));
                    UnitCommands temporary = null;
                    var container = f.Actor.Commands;
                    using (new UnitCommands.Temporary(ref temporary, f.Actor)) f.Click(f.Enemy, true);
                    FirearmInputCheck(prefix + "rejection-and-preview-preserve", ReferenceEquals(container, f.Actor.Commands) &&
                        BrokenSequenceSuppressionRuntime.Orders.IsCurrent(order) && !order.OwnerSlotRemoved &&
                        BrokenSequenceSuppressionRuntime.Orders.Submission(f.Actor) == submission &&
                        ReferenceEquals(order.Owner, boundaryOwner) && FirearmScheduledCallbacks() == (pending ? 0 : 1) &&
                        !f.Actor.Commands.Raw.Contains(rejected) && costs.Count(rejected) == 0,
                        "Native wrong-executor rejection and Temporary prediction leave the exact accepted owner/callback unchanged.");
                    int grit = f.Actor.Descriptor.Resources.GetResourceAmount(gunslinger.Grit.Resource);
                    UnitUseAbility personal = null;
                    if (action == "cancel-after-reload")
                        f.Turns.Execute(() => f.Actor.Commands.InterruptAll(true));
                    else
                    {
                        var ability = coexist ? gunslinger.MysteriousStranger.FocusedAim.ComponentsArray.OfType<AddFacts>()
                            .Single().Facts.OfType<BlueprintAbility>().Single() : gunslinger.QuickClear.StandardAbility;
                        personal = f.ClickAbility(ability);
                    }
                    FirearmInputCheck(prefix + "native-ownership-outcome",
                        BrokenSequenceSuppressionRuntime.Orders.IsCurrent(order) == coexist &&
                        (coexist ? BrokenSequenceSuppressionRuntime.Orders.Submission(f.Actor) == submission :
                            BrokenSequenceSuppressionRuntime.IsSuppressed(f.Actor, f.Weapon)),
                        "Native retained/removed slots decide ownership, including a completed reload awaiting its callback.");
                    if (pending) f.Turns.SetFirearmPaused(false);
                    FirearmMisfireRuntime.QueueForcedNaturalRoll(10);
                    f.Turns.FlushFirearmCallbacks();
                    if (coexist)
                    {
                        for (int tick = 0; tick < 240 && (f.Shots.Count == 1 ||
                            f.Actor.Descriptor.Resources.GetResourceAmount(gunslinger.Grit.Resource) == grit); tick++)
                        { f.Pump(); yield return null; }
                        FirearmInputCheck(prefix + "continues-once", f.Shots.Count == 2 &&
                            f.State.Condition == FirearmCondition.Broken && f.State.IsEmpty && f.Powder == 11 &&
                            costs.Exact(personal, false) && costs.Exact(f.LastShotCommand, false) &&
                            f.Actor.Descriptor.Resources.GetResourceAmount(gunslinger.Grit.Resource) == grit - 1 &&
                            ReferenceEquals(NativeFirearmAttackOrder.Get(f.LastShotCommand)?.Order, order),
                            f.Describe() + ";one Swift, one grit, original reload continuation, no extra click");
                    }
                    else
                    {
                        foreach (object step in DriveFirearmIdle(f)) yield return step;
                        FirearmInputCheck(prefix + "old-work-remains-cancelled", f.Shots.Count == 1 &&
                            f.Powder == (pending ? 12 : 11) && f.State.LoadedRounds == (pending ? 0 : 1) &&
                            f.State.Condition == (personal == null ? FirearmCondition.Broken : FirearmCondition.Normal) &&
                            FirearmScheduledCallbacks() == 0 && BrokenSequenceSuppressionRuntime.Orders.Current(f.Actor) == null &&
                            f.Actor.Descriptor.Resources.GetResourceAmount(gunslinger.Grit.Resource) == grit &&
                            (personal == null || costs.Exact(personal, false)),
                            f.Describe() + ";native cancellation/Quick Clear cannot resurrect the attack; no refunded costs");
                        FirearmMisfireRuntime.QueueForcedNaturalRoll(10);
                        f.Click(f.Enemy);
                        for (int tick = 0; tick < 240 && f.Shots.Count == 1; tick++)
                        { f.Pump(); yield return null; }
                        FirearmInputCheck(prefix + "later-input-valid", f.Shots.Count == 2 && f.Powder == 11 &&
                            f.SameItemIdentity && costs.Exact(f.LastShotCommand, false),
                            f.Describe() + ";later explicit ordinary input remains valid");
                    }
                }
                f.Dispose();
                FirearmInputCheck(prefix + "cleanup", f.Restored, "Native fixture restored exactly.");
                _firearmInputFixture = null;
            }
        }
    }
}
