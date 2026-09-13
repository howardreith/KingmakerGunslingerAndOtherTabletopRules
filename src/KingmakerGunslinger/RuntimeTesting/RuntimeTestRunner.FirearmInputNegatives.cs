using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Commands;
using KingmakerGunslinger.Firing;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private void CheckBrokenInputNegatives(FirearmInputFixture f, string prefix)
        {
            int shots = f.Shots.Count, powder = f.Powder;
            var liveCommands = f.Actor.Commands;
            UnitCommands previewCommands = null;
            long submission = BrokenSequenceSuppressionRuntime.Orders.Submission(f.Actor);
            using (new UnitCommands.Temporary(ref previewCommands, f.Actor))
                f.Click(f.Enemy, true);
            FirearmInputCheck(prefix + "preview-is-not-consent", ReferenceEquals(f.Actor.Commands, liveCommands) &&
                BrokenSequenceSuppressionRuntime.Orders.Submission(f.Actor) == submission &&
                BrokenSequenceSuppressionRuntime.Orders.Current(f.Actor) == null &&
                BrokenSequenceSuppressionRuntime.IsSuppressed(f.Actor, f.Weapon) &&
                f.Shots.Count == shots && f.Powder == powder,
                "Native Temporary commands and simulate=true cannot grant or cancel a real order.");
            f.Click(f.Actor);
            FirearmInputCheck(prefix + "selection-is-not-attack", f.Shots.Count == shots &&
                f.Powder == powder && BrokenSequenceSuppressionRuntime.Orders.Current(f.Actor) == null &&
                BrokenSequenceSuppressionRuntime.IsSuppressed(f.Actor, f.Weapon), f.Describe());
            var child = new GameObject("KMG_Runtime_Firearm_WrongClickedView");
            try
            {
                child.transform.SetParent(f.Enemy.View.transform, false);
                bool handled = true, rejectedView = false;
                try
                {
                    f.Turns.Execute(() => handled = new Kingmaker.Controllers.Clicks.Handlers.ClickUnitHandler()
                        .OnClick(child, f.Enemy.Position, 0, false, false));
                }
                catch (NullReferenceException)
                {
                    // The native click dispatcher promises a root unit view;
                    // OnClick rejects a child by dereferencing that absent
                    // component. Preserve that contract and prove no consent.
                    rejectedView = true;
                }
                FirearmInputCheck(prefix + "root-clicked-view-required", (rejectedView || !handled) &&
                    BrokenSequenceSuppressionRuntime.Orders.Current(f.Actor) == null &&
                    BrokenSequenceSuppressionRuntime.IsSuppressed(f.Actor, f.Weapon),
                    "A child object is not the native clicked UnitEntityView and grants no order; native rejected view=" + rejectedView);
            }
            finally { UnityEngine.Object.DestroyImmediate(child); }
            bool faulted = false;
            using (var fault = new FirearmInputFault(f.Actor, f.Enemy, _request.RunId))
            {
                try { f.Click(f.Enemy); }
                catch (InvalidOperationException exception)
                {
                    if (exception.Message != FirearmInputFault.Message) throw;
                    faulted = true;
                }
                FirearmInputCheck(prefix + "fault-after-native-submission", faulted && fault.Hits == 1 &&
                    fault.AcceptedBeforeFault && BrokenSequenceSuppressionRuntime.Orders.Current(f.Actor) == null &&
                    BrokenSequenceSuppressionRuntime.IsSuppressed(f.Actor, f.Weapon) &&
                    f.Shots.Count == shots && f.Powder == powder,
                    "faulted=" + faulted + ";hits=" + fault.Hits + ";earlyTargetUpdates=" + fault.EarlyHits +
                    ";acceptedBeforeFault=" + fault.AcceptedBeforeFault + ";" + f.Describe() +
                    ";nativeSubmission=" + fault.Detail);
            }
            f.Actor.Commands.RemoveFinishedAndUpdateQueue();
        }
        private sealed class FirearmInputFault : IDisposable
        {
            internal const string Message = "KMG request-local native input fault after submission";
            private static FirearmInputFault active;
            private readonly UnitEntityData actor, target;
            private readonly HarmonyInstance harmony;
            private readonly string id;
            private readonly MethodInfo handler;
            internal int Hits, EarlyHits;
            internal string Detail;
            internal bool AcceptedBeforeFault;
            internal FirearmInputFault(UnitEntityData actor, UnitEntityData target, string runId)
            {
                if (active != null) throw new InvalidOperationException("A native input fault probe is already active.");
                this.actor = actor; this.target = target;
                id = "KMG.FirearmInputFault." + runId;
                harmony = HarmonyInstance.Create(id);
                handler = typeof(Kingmaker.Controllers.Clicks.Handlers.ClickUnitHandler).GetMethod("OnClick");
                active = this;
                try { harmony.Patch(handler, null, null, new HarmonyMethod(typeof(FirearmInputFault).GetMethod("Inject",
                    BindingFlags.NonPublic | BindingFlags.Static))); }
                catch { Dispose(); throw; }
            }
            private static IEnumerable<CodeInstruction> Inject(IEnumerable<CodeInstruction> source)
            {
                var setter = typeof(UnitCombatState).GetProperty("ManualTarget").GetSetMethod(true);
                int count = 0;
                foreach (var instruction in source)
                {
                    if (instruction.opcode == OpCodes.Callvirt && Equals(instruction.operand, setter))
                    {
                        instruction.opcode = OpCodes.Call;
                        instruction.operand = typeof(FirearmInputFault).GetMethod("AfterSubmission",
                            BindingFlags.NonPublic | BindingFlags.Static);
                        count++;
                    }
                    yield return instruction;
                }
                if (count != 1) throw new InvalidOperationException("Native post-submission fault callsite changed.");
            }
            private static void AfterSubmission(UnitCombatState __instance, UnitEntityData __0)
            {
                // Instrument the exact handler callsite: a late detour of the
                // tiny native setter is invisible to already-inlined callers.
                __instance.ManualTarget = __0;
                var probe = active;
                if (probe == null || !ReferenceEquals(__instance, probe.actor.CombatState) ||
                    !ReferenceEquals(__0, probe.target)) return;
                var order = BrokenSequenceSuppressionRuntime.Orders.Current(probe.actor);
                probe.AcceptedBeforeFault = order != null && ReferenceEquals(order.Target, probe.target) &&
                    probe.actor.Commands.Raw.OfType<UnitUseAbility>().Any(command =>
                        ReferenceEquals(NativeFirearmAttackOrder.Get(command)?.Order, order));
                probe.Detail = "order=" + (order != null) + ";commands=" + string.Join(",",
                    probe.actor.Commands.Raw.Where(command => command != null).Select(command => {
                        var binding = NativeFirearmAttackOrder.Get(command);
                        return command.GetType().Name + ":" + command.Result + "/binding=" + (binding != null) +
                            "/proposal=" + (binding != null && binding.Proposal) +
                            "/submission=" + (binding == null ? 0 : binding.Submission);
                    })) + ";reloadAvailable=" + (probe.actor.AutoUseAbility != null && probe.actor.AutoUseAbility.IsAvailable) +
                    ";autoUseAvailable=" + (probe.actor.GetAvailableAutoUseAbility() != null);
                if (!probe.AcceptedBeforeFault) { probe.EarlyHits++; return; }
                probe.Hits++;
                throw new InvalidOperationException(Message);
            }
            public void Dispose()
            {
                if (handler != null) harmony.Unpatch(handler, HarmonyPatchType.All, id);
                if (ReferenceEquals(active, this)) active = null;
            }
        }
    }
}
