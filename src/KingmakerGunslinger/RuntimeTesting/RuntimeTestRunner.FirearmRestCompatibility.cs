using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Controllers.Rest;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Interaction;
using Kingmaker.UnitLogic.Parts;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Firing;
using KingmakerGunslinger.Gunsmithing;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private sealed class FirearmFixtureInteraction : IUnitInteraction
        {
            private readonly UnitEntityData actor, target;
            internal FirearmFixtureInteraction(UnitEntityData actor, UnitEntityData target)
            { this.actor = actor; this.target = target; }
            public float Distance { get { return 3; } }
            public bool IsApproach { get { return false; } }
            public float ApproachCooldown { get { return 0; } }
            public bool MainPlayerPrefered { get { return false; } }
            public bool IsAvailable(UnitEntityData user, UnitEntityData other)
            { return ReferenceEquals(user, actor) && ReferenceEquals(other, target); }
            public UnitCommand.ResultType Interact(UnitEntityData user, UnitEntityData other)
            { return IsAvailable(user, other) ? UnitCommand.ResultType.Success : UnitCommand.ResultType.Fail; }
        }
        private void CheckFirearmInteractionInputs(FirearmInputFixture f)
        {
            var interaction = new FirearmFixtureInteraction(f.Actor, f.Enemy);
            f.Enemy.Ensure<UnitPartInteractions>().AddInteraction(interaction);
            FirearmInputCheck("interaction-native-context", !f.Actor.CanAttack(f.Enemy) &&
                !f.Enemy.IsDirectlyControllable && !f.Actor.IsInCombat && !f.Enemy.IsInCombat &&
                ReferenceEquals(f.Enemy.SelectClickInteraction(f.Actor), interaction),
                "Native peaceful interaction: canAttack=" + f.Actor.CanAttack(f.Enemy) +
                ";controllable=" + f.Enemy.IsDirectlyControllable + ";combat=" + f.Actor.IsInCombat + "/" + f.Enemy.IsInCombat +
                ";resolved=" + ReferenceEquals(f.Enemy.SelectClickInteraction(f.Actor), interaction));
            foreach (bool console in new[] { false, true })
            {
                if (console) f.EnableController();
                try
                {
                    if (console) f.ControllerInteract(f.Enemy); else f.Click(f.Enemy);
                    FirearmInputCheck((console ? "controller-" : "desktop-") + "interaction-is-not-consent",
                        f.Actor.Commands.Raw.OfType<UnitInteractWithUnit>().Any() &&
                        !f.Actor.Commands.Raw.OfType<UnitAttack>().Any(value => !value.IsFinished) &&
                        BrokenSequenceSuppressionRuntime.Orders.Current(f.Actor) == null &&
                        BrokenSequenceSuppressionRuntime.IsSuppressed(f.Actor, f.Weapon) && f.Shots.Count == 1,
                        "Ordinary native interaction submission does not authorize or revive the firearm attack.");
                    f.Actor.Commands.InterruptAll(true); f.Actor.Commands.RemoveFinishedAndUpdateQueue();
                }
                finally { if (console) f.DisableController(); }
            }
            f.Enemy.Ensure<UnitPartInteractions>().RemoveInteraction(interaction);
        }

        // Only the owned rest completion's UI/autosave coroutine is deferred.
        // Native TickSleepPhase still invokes the actual StopRestProcess adapter.
        // The guarded save-free fixture must never enumerate its save-writing tail.
        private sealed class FirearmRestCompletionDelivery : IDisposable
        {
            [ThreadStatic] private static FirearmRestCompletionDelivery active;
            private readonly RestController controller;
            private readonly HarmonyInstance harmony;
            private readonly MethodInfo method;
            private readonly string id;
            internal int Captured;
            internal FirearmRestCompletionDelivery(RestController controller, string runId)
            {
                if (active != null) throw new InvalidOperationException("Rest completion delivery is already scoped.");
                this.controller = controller;
                id = "KMG.FirearmRestDelivery." + runId;
                harmony = HarmonyInstance.Create(id);
                method = typeof(LoadingProcess).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Single(value => value.Name == "StartLoadingProcess" && value.GetParameters().Length == 4 &&
                        value.GetParameters()[0].ParameterType == typeof(IEnumerator));
                active = this;
                try { harmony.Patch(method, new HarmonyMethod(typeof(FirearmRestCompletionDelivery).GetMethod("Defer",
                    BindingFlags.NonPublic | BindingFlags.Static))); }
                catch { Dispose(); throw; }
            }
            private static bool Defer(IEnumerator __0)
            {
                if (active == null) return true;
                var owner = __0 == null ? null : __0.GetType().GetField("<>4__this",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (__0 == null || __0.GetType().DeclaringType != typeof(RestController) ||
                    !__0.GetType().Name.StartsWith("<StopRestProcess>", StringComparison.Ordinal) ||
                    owner == null || !ReferenceEquals(owner.GetValue(__0), active.controller))
                    throw new InvalidOperationException("Unexpected loading work during owned rest completion.");
                active.Captured++;
                ((IDisposable)__0).Dispose();
                return false;
            }
            public void Dispose()
            {
                if (method != null) harmony.Unpatch(method, HarmonyPatchType.All, id);
                if (ReferenceEquals(active, this)) active = null;
            }
        }

        private void CheckFirearmCompletedRest(FirearmInputFixture f)
        {
            _firearmInputStage = "native-completed-rest";
            f.Turns.LeaveCombatForRecovery();
            const BindingFlags flags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var remaining = typeof(RestController).GetProperty("RemainingTime", flags);
            var stop = typeof(RestController).GetMethod("StopRestProcess", flags);
            var tick = typeof(RestController).GetMethod("TickSleepPhase", flags);
            var processed = typeof(CompletedRestMaintenancePatch).GetField("_processedStatus", flags);
            object oldRemaining = remaining.GetValue(null, null), oldProcessed = processed.GetValue(null);
            TimeSpan clock = Game.Instance.Player.GameTime;
            var kit = Game.Instance.Player.Inventory.Items.Single(item =>
                ReferenceEquals(item.Blueprint, BlueprintBootstrap.GunsmithingSupplies.GunsmithKit));
            int shots = f.Shots.Count, powder = f.Powder;
            FirearmInputCheck("rest-empty-equipment-slots", f.Actor.Body.AllSlots.Any(slot => slot.MaybeItem == null),
                "Native empty slots must be skipped during full-rest firearm collection.");
            var controller = new RestController();
            try
            {
                foreach (var rejected in new[] {
                    new RestStatus { RestSucceeded = false },
                    new RestStatus { RestSucceeded = true, SkipTime = true },
                    new RestStatus { RestSucceeded = true, NightRandomEncounter = true } })
                {
                    controller.Status = rejected;
                    ((IDisposable)stop.Invoke(controller, null)).Dispose();
                    FirearmInputCheck("rest-rejects-" + (rejected.SkipTime ? "time-skip" :
                        rejected.NightRandomEncounter ? "encounter" : "incomplete"),
                        f.State.Condition == FirearmCondition.Wrecked && f.SameItemIdentity,
                        "Native rest termination rejects nonqualifying maintenance; naturally Wrecked firearm unchanged.");
                }
                // Execute the native full sleeping interval, which sets success,
                // advances the clock and applies normal rest effects. No direct
                // assignment of success or firearm condition qualifies this case.
                controller.Status = new RestStatus { ApplyRest = true };
                remaining.SetValue(null, TimeSpan.FromHours(8), null);
                f.Actor.Descriptor.State.AddCondition(UnitCondition.Sleeping, null);
                f.Turns.Execute(() => tick.Invoke(controller, null));
                FirearmInputCheck("native-full-sleep-interval", controller.Status.RestSucceeded &&
                    controller.Status.SleepTime == TimeSpan.FromHours(8) &&
                    (TimeSpan)remaining.GetValue(null, null) == TimeSpan.Zero &&
                    Game.Instance.Player.GameTime == clock + TimeSpan.FromHours(8) &&
                    f.State.Condition == FirearmCondition.Wrecked,
                    "Native TickSleepPhase completed eight hours and native rest effects before firearm maintenance.");
                long completions = CompletedRestMaintenancePatch.MaintenanceRuns;
                // Invoke the real shared native completion boundary. Its returned
                // coroutine is deliberately not enumerated: it only presents the
                // camping completion UI and performs the trailing autosave.
                int captured;
                using (var delivery = new FirearmRestCompletionDelivery(controller, _request.RunId))
                {
                    f.Turns.Execute(() => tick.Invoke(controller, null));
                    captured = delivery.Captured;
                }
                FirearmInputCheck("native-rest-restores-natural-wrecked", f.State.Condition == FirearmCondition.Normal &&
                    f.State.IsEmpty && f.SameItemIdentity && kit.Count == 1 &&
                    Game.Instance.Player.Inventory.Items.Contains(kit) && f.Shots.Count == shots && f.Powder == powder &&
                    captured == 1 && CompletedRestMaintenancePatch.MaintenanceRuns == completions + 1 &&
                    BrokenSequenceSuppressionRuntime.Orders.Current(f.Actor) == null &&
                    BrokenSequenceSuppressionRuntime.IsSuppressed(f.Actor, f.Weapon),
                    f.Describe() + ";nativeCompletionCaptured=" + captured +
                    ";maintenanceRuns=" + (CompletedRestMaintenancePatch.MaintenanceRuns - completions) +
                    ";completionSeen=" + CompletedRestMaintenancePatch.RestCompletionsSeen +
                    ";processedStatus=" + ReferenceEquals(processed.GetValue(null), controller.Status) +
                    ";participant=" + Game.Instance.Player.AllCharacters.Contains(f.Actor) +
                    ";capable=" + Recovery.FirearmMaintenanceCapability.CanMaintainFirearmsAtCompletedRest(f.Actor.Descriptor) +
                    ";unconscious=" + f.Actor.Descriptor.State.IsUnconscious +
                    ";kit=" + kit.Count + ";native completion once; no trailing UI/autosave enumeration");
                ((IDisposable)stop.Invoke(controller, null)).Dispose();
                FirearmInputCheck("native-rest-once-per-completion", CompletedRestMaintenancePatch.MaintenanceRuns == completions + 1,
                    "Repeated native termination of the same status cannot repeat maintenance.");
            }
            finally
            {
                f.Actor.Descriptor.State.RemoveCondition(UnitCondition.Sleeping);
                remaining.SetValue(null, oldRemaining, null);
                processed.SetValue(null, oldProcessed);
                Game.Instance.Player.GameTime = clock;
            }
        }
    }
}
