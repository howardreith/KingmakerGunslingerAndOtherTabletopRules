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
            foreach (FirearmCondition condition in new[] {
                FirearmCondition.Normal,
                FirearmCondition.Broken,
                FirearmCondition.Wrecked })
            {
                foreach (bool playerContext in new[] { false, true })
                {
                    Assertions.Equal(BrokenSequenceConstructionDecision.Allow,
                        BrokenSequenceInterruptionPolicy.EvaluateConstruction(
                            false, playerContext, condition),
                        "Without a committed-break suppression every construction passes: " +
                        condition + "/" + playerContext);
                }
            }
        }

        internal static void ConstructionAutomaticAfterBreakRejected()
        {
            foreach (FirearmCondition condition in new[] {
                FirearmCondition.Broken,
                FirearmCondition.Wrecked })
            {
                Assertions.Equal(
                    BrokenSequenceConstructionDecision.RejectInterrupted,
                    BrokenSequenceInterruptionPolicy.EvaluateConstruction(
                        true, false, condition),
                    "Automatic recreation for a damaged weapon is interrupted: " +
                    condition);
            }
        }

        internal static void ConstructionPlayerIssuedOrderConsumesSuppression()
        {
            Assertions.Equal(
                BrokenSequenceConstructionDecision.AllowAndConsume,
                BrokenSequenceInterruptionPolicy.EvaluateConstruction(
                    true, true, FirearmCondition.Broken),
                "A genuine player-issued attack order with the Broken firearm passes and consumes the suppression.");
            Assertions.Equal(
                BrokenSequenceConstructionDecision.AllowAndConsume,
                BrokenSequenceInterruptionPolicy.EvaluateConstruction(
                    true, true, FirearmCondition.Wrecked),
                "A player-issued order is still the player's decision even for a Wrecked weapon.");
        }

        internal static void ConstructionRepairedWeaponNeverLocked()
        {
            Assertions.Equal(
                BrokenSequenceConstructionDecision.AllowAndConsume,
                BrokenSequenceInterruptionPolicy.EvaluateConstruction(
                    true, false, FirearmCondition.Normal),
                "A weapon repaired back to Normal (e.g. Quick Clear) must never be gated, even automatically.");
        }

        internal static void ConstructionInvalidConditionFailsClosed()
        {
            bool threw = false;
            try
            {
                BrokenSequenceInterruptionPolicy.EvaluateConstruction(
                    true, false, (FirearmCondition)99);
            }
            catch (ArgumentOutOfRangeException)
            {
                threw = true;
            }
            Assertions.True(threw,
                "An unknown condition must fail closed instead of guessing an interruption decision.");
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
                source.Contains("IsPlayerAttackContext") &&
                source.Contains("BrokenSequenceSuppressionRuntime.ConsumeSuppression("),
                "Attack construction must gate on suppression, player context, and consume on a deliberate order.");
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
                source.Contains("MarkPlayerAttackFrame"),
                "The native player click handler must mark the player-attack frame for constructions inside it.");
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
