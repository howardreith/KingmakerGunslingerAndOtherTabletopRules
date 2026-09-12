using System;
using System.IO;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Firing;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Regression coverage for the newly-Broken sequence interruption
    /// (mission Z-FIREARM-MAINTENANCE §4.3): a verified committed degradation
    /// of the exact firearm during an attack sequence stops the remainder of
    /// that sequence and its automatic continuations, while a later genuine
    /// player-issued attack with the Broken firearm stays possible and a
    /// repaired weapon is never locked.
    /// </summary>
    internal static class BrokenSequenceInterruptionTests
    {
        internal static void ConstructionUnsuppressedAlwaysAllows()
        {
            foreach (bool playerOrder in new[] { false, true })
            {
                Assertions.Equal(BrokenSequenceConstructionDecision.Allow,
                    BrokenSequenceInterruptionPolicy.EvaluateConstruction(
                        false, playerOrder),
                    "Without a committed-break suppression every construction passes: " +
                    playerOrder);
            }
        }

        internal static void ConstructionAutomaticAfterBreakRejected()
        {
            Assertions.Equal(
                BrokenSequenceConstructionDecision.RejectInterrupted,
                BrokenSequenceInterruptionPolicy.EvaluateConstruction(
                    true, false),
                "Automatic recreation of the interrupted order is rejected, including after the firearm was repaired or reloaded (readiness is not consent).");
        }

        internal static void ConstructionPlayerIssuedOrderConsumesSuppression()
        {
            Assertions.Equal(
                BrokenSequenceConstructionDecision.AllowAndConsume,
                BrokenSequenceInterruptionPolicy.EvaluateConstruction(
                    true, true),
                "A genuine player-issued attack order passes and consumes the suppression, whatever the firearm condition.");
        }

        internal static void ConstructionRepairDoesNotReviveCancelledOrder()
        {
            // Review R3: repairing the firearm restores readiness, not the
            // cancelled order. The policy takes no condition input at all —
            // only a genuine new player order releases suppression — so a
            // brain re-issue after Quick Clear/repair stays cancelled while
            // any later deliberate order works normally.
            Assertions.Equal(
                BrokenSequenceConstructionDecision.RejectInterrupted,
                BrokenSequenceInterruptionPolicy.EvaluateConstruction(
                    true, false),
                "A repaired firearm must not let the brain revive the cancelled order.");
            string policy = Read("src/KingmakerGunslinger/Firing",
                "BrokenSequenceInterruptionPolicy.cs");
            Assertions.False(policy.Contains("FirearmCondition"),
                "The interruption policy must not consult firearm condition: readiness is not player consent.");
        }

        internal static void FullAttackEndsAfterCommittedBreak()
        {
            Assertions.True(BrokenSequenceInterruptionPolicy.ShouldEndFullAttack(true),
                "A committed break during this command ends the remaining full attack.");
            Assertions.False(BrokenSequenceInterruptionPolicy.ShouldEndFullAttack(false),
                "Without a committed break the native full attack policy is unchanged.");
        }

        internal static void ResumeBlockedByLaterDegradation()
        {
            Assertions.True(
                BrokenSequenceInterruptionPolicy.MayResumeCapturedAttack(3, 3),
                "A captured reload-resume continues while the exact weapon did not degrade.");
            Assertions.False(
                BrokenSequenceInterruptionPolicy.MayResumeCapturedAttack(3, 4),
                "A committed degradation after capture cancels the pending reload-resume continuation.");
            Assertions.True(
                BrokenSequenceInterruptionPolicy.MayResumeCapturedAttack(0, 0),
                "A capture before any degradation resumes normally.");
        }

        internal static void WiringMisfireRuntimeRecordsCommittedDegradation()
        {
            string source = Read("src/KingmakerGunslinger/Misfires",
                "FirearmMisfireRuntime.cs");
            int commit = source.IndexOf(
                "CommitConditionTransition(context, condition);",
                StringComparison.Ordinal);
            int record = source.IndexOf(
                "RecordCommittedDegradation(attackRoll, context.FirearmItem);",
                StringComparison.Ordinal);
            int suppress = source.IndexOf(
                "Firing.BrokenSequenceSuppressionRuntime.OnCommittedDegradation(",
                StringComparison.Ordinal);
            Assertions.True(commit >= 0 && record > commit && suppress > record,
                "Committed degradations must be recorded and suppress the interrupted sequence immediately after the exact-item commit.");
            Assertions.True(source.Contains(
                    "internal static bool TryGetCommittedDegradation("),
                "The misfire runtime must expose the verified committed-degradation lookup for the sequence gates.");
        }

        internal static void WiringFullAttackEndsBeforeNextShot()
        {
            string source = Read("src/KingmakerGunslinger/Firing",
                "FreeActionFullAttackReloadPatch.cs");
            int gate = source.IndexOf("TryGetCommittedDegradation(",
                StringComparison.Ordinal);
            int state = source.IndexOf("FirearmState currentState",
                StringComparison.Ordinal);
            Assertions.True(gate >= 0 && state > gate,
                "The committed-break gate must run before the ordinary loaded/reload full-attack decisions.");
            Assertions.True(source.Contains("full-attack.ended-after-committed-break"),
                "Ending the full attack after a committed break must be observable in diagnostics.");
            Assertions.True(source.Contains("previous.AttackRoll"),
                "The gate must inspect the exact previous completed shot of this command.");
        }

        internal static void WiringConstructionGateAndPlayerClickContext()
        {
            string source = Read("src/KingmakerGunslinger/Firing",
                "EmptyFirearmAttackCommandPatch.cs");
            Assertions.True(source.Contains(
                    "BrokenSequenceInterruptionPolicy.EvaluateConstruction(") &&
                source.Contains("TryConsumePlayerAttackAuthorization(executor, __1)") &&
                source.Contains("BrokenSequenceSuppressionRuntime.ConsumeSuppression("),
                "Attack construction must gate on suppression, consume the exact-order player authorization, and consume suppression on a deliberate order.");
            Assertions.True(source.Contains("RejectInterrupted") &&
                source.Contains("broke during its attack sequence"),
                "The interrupted-sequence rejection must be visible to the player.");
            int gate = source.IndexOf(
                "BrokenSequenceInterruptionPolicy.EvaluateConstruction(",
                StringComparison.Ordinal);
            int emptyPolicy = source.IndexOf("EmptyFirearmAttackPolicy.Evaluate(",
                StringComparison.Ordinal);
            Assertions.True(emptyPolicy > gate,
                "The interruption gate must run before the empty/Wrecked/auto-reload policy so no replacement reload is queued for an interrupted sequence.");
            Assertions.True(source.Contains("degradationEpoch: BrokenSequenceSuppressionRuntime") &&
                source.Contains("MayResumeCapturedAttack(") &&
                source.Contains("GetDegradationEpoch("),
                "Pending reload-resume continuations must capture and recheck the degradation epoch.");
            Assertions.True(source.Contains("ClickUnitHandler") &&
                source.Contains("\"OnClick\"") &&
                source.Contains("PlayerClickPrefix") &&
                source.Contains("PlayerClickPostfix") &&
                source.Contains("BeginPlayerAttackClick") &&
                source.Contains("EndPlayerAttackClick"),
                "The native player click handler must open and close the order-scoped player authorization around itself.");
        }

        internal static void WiringAllCommitPathsNotifyInterruption()
        {
            // Review R2: every production committed-degradation writer must
            // route through the shared interruption notification — the
            // ordinary misfire, Dead Shot, and Scatter Shot.
            string shared = Read("src/KingmakerGunslinger/Firing",
                "BrokenSequenceSuppressionRuntime.cs");
            Assertions.True(shared.Contains(
                    "internal static void OnCommittedDegradation") &&
                shared.Contains(
                    "Dead Shot, and Scatter Shot all call this"),
                "The shared committed-degradation notification must document itself as the single entry.");
            string misfire = Read("src/KingmakerGunslinger/Misfires",
                "FirearmMisfireRuntime.cs");
            string deadShot = Read("src/KingmakerGunslinger/Deeds",
                "DeadShotRuntime.cs");
            string scatter = Read("src/KingmakerGunslinger/Scatter",
                "ScatterShotRuntime.cs");
            foreach (string source in new[] { misfire, deadShot, scatter })
            {
                Assertions.True(source.Contains(
                        ".OnCommittedDegradation("),
                    "A committed-degradation writer is not routed through the shared interruption notification.");
            }
            // The notification must fire only after the guarded transition
            // commits and verifies, never for rolled-back or prevented breaks.
            int deadCommit = deadShot.IndexOf(
                "conditionCommit = Transition(firearm,", StringComparison.Ordinal);
            int deadNotify = deadShot.IndexOf(
                "OnCommittedDegradation(casterEntity", StringComparison.Ordinal);
            Assertions.True(deadCommit >= 0 && deadNotify > deadCommit,
                "Dead Shot must notify only after its condition commit verifies.");
            int scatterCommit = scatter.IndexOf(
                "Transition(firearm, expected, condition.After)", StringComparison.Ordinal);
            int scatterNotify = scatter.IndexOf(
                "OnCommittedDegradation(caster", StringComparison.Ordinal);
            Assertions.True(scatterCommit >= 0 && scatterNotify > scatterCommit,
                "Scatter must notify only after its condition commit verifies.");
        }

        internal static void WiringSuppressionStateStaysTransient()
        {
            string policy = Read("src/KingmakerGunslinger/Firing",
                "BrokenSequenceInterruptionPolicy.cs");
            Assertions.False(policy.Contains("Kingmaker."),
                "The interruption policy must stay dependency-free.");
            string runtime = Read("src/KingmakerGunslinger/Firing",
                "BrokenSequenceSuppressionRuntime.cs");
            Assertions.True(runtime.Contains("ConditionalWeakTable"),
                "Suppression bookkeeping must be weakly keyed so scene transitions and save/load cannot leak it.");
            Assertions.True(runtime.Contains("IsNativePlayerClickOnStack") &&
                runtime.Contains("ClickUnitHandler"),
                "Production authorization consumption must prove the genuine native click handler is still on the call stack (review CR2-04).");
            Assertions.True(runtime.Contains("ClearForRuntimeTest"),
                "The guarded runtime-test seam must exist for deterministic test resets.");
        }

        private static string Read(params string[] parts)
        {
            string path = Environment.CurrentDirectory;
            foreach (string part in parts) path = Path.Combine(path, part);
            return File.ReadAllText(path);
        }
    }
}
