using System;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.FeatureModules;
using KingmakerGunslinger.Presentation;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Engine-free halves of the missing-Gunslinger-class repair: the decision that tells assigned unit-fact text from
    /// an engine placeholder, the exact-restore rollback, and the settings-panel content status. The engine halves
    /// (the real builders, the progression traversal and the bootstrap cleanup) are exercised in the game through
    /// scripts/compatibility/loader-compat/sessions/gunslinger-missing-class.json.
    /// </summary>
    internal static class ArmedPresentationRepairTests
    {
        private const string StockUnassigned =
            PlayerFacingTextAssignment.EngineUnassignedPlaceholder;

        internal static void UnassignedTextIsNeverAccepted()
        {
            // Stock Kingmaker shows "<null>" for a name that was never stored.
            // It is not empty, which is how an unnamed armed buff passed.
            Assertions.Equal(PlayerFacingTextState.Unassigned,
                PlayerFacingTextAssignment.Classify(false, "<null>", StockUnassigned),
                "The stock engine placeholder was accepted as a name.");
            // The reported installation resolved the same gap to nothing.
            Assertions.Equal(PlayerFacingTextState.Unassigned,
                PlayerFacingTextAssignment.Classify(false, string.Empty, string.Empty),
                "A blank resolution of unassigned text was not reported as unassigned.");
            Assertions.Equal(PlayerFacingTextState.Unassigned,
                PlayerFacingTextAssignment.Classify(false, null, null),
                "A null resolution of unassigned text was not reported as unassigned.");
            // Whatever an installation substitutes, an unstored string that
            // resolves to that substitute is still unassigned.
            Assertions.Equal(PlayerFacingTextState.Unassigned,
                PlayerFacingTextAssignment.Classify(false, "???", "???"),
                "An environment-specific stand-in was accepted as a name.");
            Assertions.Equal(PlayerFacingTextState.Unassigned,
                PlayerFacingTextAssignment.Classify(false, "<null>", string.Empty),
                "The stock placeholder was accepted where unassigned text resolves blank.");
        }

        internal static void StoredTextMustResolve()
        {
            Assertions.Equal(PlayerFacingTextState.Blank,
                PlayerFacingTextAssignment.Classify(true, string.Empty, StockUnassigned),
                "A stored string that resolves to nothing was accepted.");
            Assertions.Equal(PlayerFacingTextState.Blank,
                PlayerFacingTextAssignment.Classify(true, "  \t", StockUnassigned),
                "A stored string that resolves to white space was accepted.");
            Assertions.Equal(PlayerFacingTextState.UnknownKey,
                PlayerFacingTextAssignment.Classify(true,
                    "[unknown key: KMG.BleedingWound.HitPoints.Armed.Name]",
                    StockUnassigned),
                "The engine's missing-key placeholder was accepted as text.");
        }

        internal static void AssignedTextPassesInEveryResolutionMode()
        {
            foreach (string unassigned in new[] { StockUnassigned, string.Empty, null })
            {
                Assertions.Equal(PlayerFacingTextState.Assigned,
                    PlayerFacingTextAssignment.Classify(true,
                        "Bleeding Wound Armed — Hit Points", unassigned),
                    "Assigned English text was rejected.");
                Assertions.Equal(PlayerFacingTextState.Assigned,
                    PlayerFacingTextAssignment.Classify(true,
                        "流血伤口", unassigned),
                    "Assigned non-English text was rejected.");
                // A subtype that supplies its own text still shows real text.
                Assertions.Equal(PlayerFacingTextState.Assigned,
                    PlayerFacingTextAssignment.Classify(false,
                        "Supplied by the fact itself", unassigned),
                    "Text supplied without the stored field was rejected.");
            }
            foreach (PlayerFacingTextState state in
                (PlayerFacingTextState[])Enum.GetValues(typeof(PlayerFacingTextState)))
                Assertions.False(string.IsNullOrWhiteSpace(
                    PlayerFacingTextAssignment.Describe(state)),
                    "A text state has no diagnostic name: " + state);
        }

        internal static void RollbackRestoresTheExactCapturedOriginal()
        {
            // A null original and an original that displays as nothing are both
            // states the installation really had. Each must come back as the
            // very same reference, with no display-text condition.
            var blankOriginal = new Slot { Text = string.Empty };
            var nullSlot = new Holder(null);
            var blankSlot = new Holder(blankOriginal);
            var nullPublished = new Slot { Text = "published" };
            var blankPublished = new Slot { Text = "published" };
            nullSlot.Value = nullPublished;
            blankSlot.Value = blankPublished;

            var rollback = new OwnedMutationRollback();
            rollback.Restore("null-original", () => nullSlot.Value,
                value => nullSlot.Value = value, nullPublished, null);
            rollback.Restore("blank-original", () => blankSlot.Value,
                value => blankSlot.Value = value, blankPublished, blankOriginal);
            OwnedMutationRollbackReport report = rollback.Complete();

            Assertions.True(nullSlot.Value == null,
                "A null original description was not restored.");
            Assertions.True(ReferenceEquals(blankSlot.Value, blankOriginal),
                "A blank original description was not restored as the captured object.");
            Assertions.True(report.IsComplete && report.ResolvedCount == 2 &&
                report.UnresolvedCount == 0,
                "An exact restoration was not reported complete: " + report.Describe());
        }

        internal static void RollbackKeepsForeignChangesAndContinues()
        {
            var first = new Holder(new Slot { Text = "first-original" });
            var foreign = new Holder(new Slot { Text = "foreign-original" });
            var failing = new Holder(new Slot { Text = "failing-original" });
            var last = new Holder(new Slot { Text = "last-original" });
            Holder[] holders = { first, foreign, failing, last };
            var originals = new Slot[holders.Length];
            var published = new Slot[holders.Length];
            for (int index = 0; index < holders.Length; index++)
            {
                originals[index] = holders[index].Value;
                published[index] = new Slot { Text = "published-" + index };
                holders[index].Value = published[index];
            }
            // Another mod replaces one slot after publication.
            var laterForeign = new Slot { Text = "another mod" };
            foreign.Value = laterForeign;

            var rollback = new OwnedMutationRollback();
            // Same order as the product: last published, first restored.
            rollback.Restore("last", () => last.Value, value => last.Value = value,
                published[3], originals[3]);
            rollback.Restore("failing", () => failing.Value,
                value => { throw new InvalidOperationException("controlled write failure"); },
                published[2], originals[2]);
            rollback.Restore("foreign", () => foreign.Value,
                value => foreign.Value = value, published[1], originals[1]);
            rollback.Restore("first", () => first.Value, value => first.Value = value,
                published[0], originals[0]);
            OwnedMutationRollbackReport report = rollback.Complete();

            Assertions.True(ReferenceEquals(foreign.Value, laterForeign),
                "Rollback overwrote a change made by someone else after publication.");
            Assertions.True(ReferenceEquals(failing.Value, published[2]),
                "A failed restoration reported state it did not leave behind.");
            Assertions.True(ReferenceEquals(last.Value, originals[3]),
                "An independent entry before the failure was not restored.");
            Assertions.True(ReferenceEquals(first.Value, originals[0]),
                "Cleanup stopped at the first conflict instead of restoring the rest.");
            Assertions.False(report.IsComplete,
                "A rollback with unresolved entries was reported complete.");
            Assertions.Equal(2, report.ResolvedCount, "Restored entry count is wrong.");
            Assertions.Equal(2, report.UnresolvedCount, "Unresolved entry count is wrong.");
            string text = report.Describe();
            foreach (string token in new[] { "owned-mutations=4", "restored=2",
                "unresolved=2", "foreign:foreign-change-preserved",
                "failing:failed", "controlled write failure" })
                Assertions.True(text.Contains(token),
                    "The rollback report omits '" + token + "': " + text);
        }

        internal static void RollbackVerifiesAndIsRepeatable()
        {
            var original = new Slot { Text = "original" };
            var published = new Slot { Text = "published" };
            var ignoring = new Holder(published);
            var rollback = new OwnedMutationRollback();
            // A write that silently does nothing must not count as restored.
            rollback.Restore("ignored-write", () => ignoring.Value, value => { },
                published, original);
            OwnedMutationRollbackReport unverified = rollback.Complete();
            Assertions.False(unverified.IsComplete,
                "An unverified restoration was reported complete.");
            Assertions.True(unverified.Describe().Contains("did not verify"),
                "The unverified restoration is not explained: " + unverified.Describe());

            var holder = new Holder(published);
            var firstPass = new OwnedMutationRollback();
            firstPass.Restore("slot", () => holder.Value, value => holder.Value = value,
                published, original);
            var secondPass = new OwnedMutationRollback();
            secondPass.Restore("slot", () => holder.Value,
                value => { throw new InvalidOperationException("must not write again"); },
                published, original);
            OwnedMutationRollbackReport repeated = secondPass.Complete();
            Assertions.True(firstPass.Complete().IsComplete && repeated.IsComplete &&
                repeated.Entries[0].Status == OwnedMutationRollbackStatus.AlreadyOriginal &&
                ReferenceEquals(holder.Value, original),
                "A repeated rollback was not a no-op: " + repeated.Describe());

            var throwingRead = new OwnedMutationRollback();
            throwingRead.Restore(null,
                () => { throw new InvalidOperationException("controlled read failure"); },
                value => { }, published, original);
            OwnedMutationRollbackReport unreadable = throwingRead.Complete();
            Assertions.True(!unreadable.IsComplete &&
                unreadable.Entries[0].Status == OwnedMutationRollbackStatus.Failed &&
                unreadable.Describe().Contains("<unnamed>:failed"),
                "An unreadable slot was not reported as failed: " + unreadable.Describe());
        }

        internal static void RollbackCoversAWriteThatFailedPartWay()
        {
            // The publication journals a mutation before it writes it. A
            // setter that stores the value and then throws leaves the slot
            // changed; one that throws first leaves it untouched. Rollback
            // must restore the first and leave the second alone.
            var writtenOriginal = new Slot { Text = "written-original" };
            var untouchedOriginal = new Slot { Text = "untouched-original" };
            var written = new Holder(writtenOriginal);
            var untouched = new Holder(untouchedOriginal);
            var writtenPublished = new Slot { Text = "published" };
            var untouchedPublished = new Slot { Text = "published" };
            Assertions.Throws<InvalidOperationException>(() =>
            {
                written.Value = writtenPublished;
                throw new InvalidOperationException("failed after the write");
            }, "The controlled part-way write did not fail.");

            var rollback = new OwnedMutationRollback();
            rollback.Restore("untouched", () => untouched.Value,
                value => { throw new InvalidOperationException("must not write"); },
                untouchedPublished, untouchedOriginal);
            rollback.Restore("written", () => written.Value,
                value => written.Value = value, writtenPublished, writtenOriginal);
            OwnedMutationRollbackReport report = rollback.Complete();

            Assertions.True(ReferenceEquals(written.Value, writtenOriginal),
                "A value written before its setter failed was not restored.");
            Assertions.True(ReferenceEquals(untouched.Value, untouchedOriginal),
                "A journalled entry that was never written was disturbed.");
            Assertions.True(report.IsComplete &&
                report.Entries[0].Status == OwnedMutationRollbackStatus.AlreadyOriginal &&
                report.Entries[1].Status == OwnedMutationRollbackStatus.Restored,
                "The part-way write was not reported exactly: " + report.Describe());
        }

        internal static void ContentStatusSeparatesLoaderActivityFromContent()
        {
            Assertions.Equal(ContentInitializationStatus.Initialized,
                ContentInitializationStatus.Describe(true, false),
                "Completed initialization is not reported.");
            Assertions.Equal(ContentInitializationStatus.Pending,
                ContentInitializationStatus.Describe(false, false),
                "A loaded mod without content was reported as initialized or failed.");
            Assertions.Equal(ContentInitializationStatus.Failed,
                ContentInitializationStatus.Describe(false, true),
                "A failed initialization is not reported.");
            Assertions.Equal(ContentInitializationStatus.Failed,
                ContentInitializationStatus.Describe(true, true),
                "A failure must outrank any other state.");
            Assertions.True(ContentInitializationStatus.Failed.Contains("not available") &&
                ContentInitializationStatus.Failed.Contains("initialize.root-cause"),
                "The failure line does not say what is missing or where to look.");
            foreach (string leak in new[] { "KMG_", "Exception", " at ", "guid" })
                Assertions.False(ContentInitializationStatus.Failed.Contains(leak) ||
                    ContentInitializationStatus.Pending.Contains(leak) ||
                    ContentInitializationStatus.Initialized.Contains(leak),
                    "Player-facing status text contains diagnostic detail: " + leak);
            Assertions.Equal("Mr. Bard's Tabletop Rules Expansion 0.0.189",
                ContentInitializationStatus.Header(
                    " Mr. Bard's Tabletop Rules Expansion ", "0.0.189 "),
                "The panel header does not show the loaded name and version.");
            Assertions.Equal("Tabletop Rules Expansion (version unknown)",
                ContentInitializationStatus.Header(null, " "),
                "A missing loader entry produced an unreadable header.");
        }

        private sealed class Slot
        {
            internal string Text;
        }

        private sealed class Holder
        {
            internal Holder(Slot value) { Value = value; }
            internal Slot Value;
        }
    }
}
