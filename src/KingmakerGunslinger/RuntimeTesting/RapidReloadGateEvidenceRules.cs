using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// The firearm-proficiency scope a Rapid Reload gate case was set up with.
    /// It is stated by the case definition next to the proficiency fact the
    /// fixture actually grants, never derived from the measured ranks and never
    /// inferred from a human-readable case label.
    /// </summary>
    internal sealed class RapidReloadExpectedScope
    {
        private RapidReloadExpectedScope(string name, bool full, bool oneHanded,
            bool twoHanded)
        {
            Name = name;
            Full = full;
            OneHanded = oneHanded;
            TwoHanded = twoHanded;
        }

        internal string Name { get; private set; }
        internal bool Full { get; private set; }
        internal bool OneHanded { get; private set; }
        internal bool TwoHanded { get; private set; }

        internal static readonly RapidReloadExpectedScope NoProficiency =
            new RapidReloadExpectedScope("no-proficiency", false, false, false);
        internal static readonly RapidReloadExpectedScope AnyFirearm =
            new RapidReloadExpectedScope("full", true, false, false);
        internal static readonly RapidReloadExpectedScope OneHandedOnly =
            new RapidReloadExpectedScope("one-handed", false, true, false);
        internal static readonly RapidReloadExpectedScope TwoHandedOnly =
            new RapidReloadExpectedScope("two-handed", false, false, true);
    }

    /// <summary>
    /// Scoring rules for the Rapid Reload proficiency-gate runtime scenario.
    /// The scenario records what it actually observed; these rules decide
    /// whether that evidence is complete and acceptable, and the domain suite
    /// exercises them with corrupted and truncated fixtures. Every evaluator
    /// demands its full observation set first and validates the fixture's
    /// measured proficiency ranks against the scope its case declared, so a
    /// missing precondition, a skipped native operation, an absent feat slot or
    /// a silently different fixture can never be scored as a pass.
    /// </summary>
    internal static class RapidReloadGateEvidenceRules
    {
        /// <summary>Proficiency facts a fixture must positively prove before
        /// its eligibility answers mean anything.</summary>
        internal static readonly string[] FixtureProficiencyKeys = {
            "fullProficiencyRank", "oneHandedProficiencyRank",
            "twoHandedProficiencyRank" };

        /// <summary>The reserved native feat slot a case claims to exercise.</summary>
        internal static readonly string[] ReservationKeys = {
            "reservedSelection", "reservedIndex", "reservedSlotResolved",
            "reservedSlotUnselected", "reservedSlotSelectionMatches" };

        /// <summary>A character with no firearm proficiency attempting the
        /// parent through a real feat slot, then completing the level normally
        /// once the reservation is released.</summary>
        internal static readonly string[] RefusedParentKeys = {
            "case", "reservation", "slotPresent", "fixture", "parentOffered",
            "parentCanSelect", "parentSelected", "hold", "childEligibility",
            "reservationReleasedBeforeConfirmation", "completeBeforeConfirmation",
            "confirmationApplied", "acquiredParent", "acquiredChild" };

        /// <summary>A proficient character attempting a firearm outside its
        /// proficiency scope, observed before any cleanup.</summary>
        internal static readonly string[] ScopedChildRefusalKeys = {
            "case", "reservation", "slotPresent", "fixture", "parentCanSelect",
            "parentSelected", "childStatePresent", "refusedChildOffered",
            "refusedChildCanSelect", "refusedChildSelected",
            "childStateSelectedAfterRefusal", "completeWhileEmpty",
            "targetBlocksCompletion", "parentRankWhileEmpty",
            "refusedChildRankWhileEmpty", "legalChildSelected",
            "targetBlocksCompletionAfterLegalChoice", "completeBeforeConfirmation",
            "confirmationApplied", "acquiredLegalChild", "acquiredRefusedChild" };

        /// <summary>A parent held with no child. This case deliberately applies
        /// a build the native completion gate refuses; it is a defensive probe,
        /// never a claim that confirmation was permitted.</summary>
        internal static readonly string[] EmptySelectionKeys = {
            "case", "reservation", "slotPresent", "fixture", "parentSelected",
            "childStatePresent", "childStateSelected", "completeWhileEmpty",
            "targetBlocksCompletion", "appliedWithoutNativeCompletion",
            "acquiredParent", "acquiredAnyChild" };

        /// <summary>A qualifying character acquiring a legal choice.</summary>
        internal static readonly string[] AcquisitionKeys = {
            "case", "reservation", "slotPresent", "fixture", "parentCanSelect",
            "parentSelected", "childCanSelect", "childSelected",
            "completeBeforeConfirmation", "confirmationApplied", "acquiredChild" };

        /// <summary>A pending class change inside one live level-up
        /// transaction.</summary>
        internal static readonly string[] PendingClassChangeKeys = {
            "controllerInstances", "reservation", "slotPresent",
            "gunslingerFixture", "heldBeforeChange", "heldChildName",
            "fighterFixture", "fighterGunslingerLevel",
            "parentEligibleAfterChange", "childEligibilityAfterChange",
            "holdAfterChange", "restoredFixture", "parentEligibleAfterRestore",
            "childEligibilityAfterRestore", "holdAfterRestore",
            "reheldBeforeSecondChange", "holdAfterSecondChange",
            "reservationReleasedBeforeConfirmation", "completeBeforeConfirmation",
            "confirmationApplied", "confirmedFighterLevel",
            "confirmedGunslingerLevel", "acquiredParentAfterConfirmation",
            "acquiredAnyChildAfterConfirmation" };

        /// <summary>A pending archetype change inside one live level-up
        /// transaction.</summary>
        internal static readonly string[] ArchetypeScopeChangeKeys = {
            "controllerInstances", "reservation", "slotPresent", "baseFixture",
            "heldBeforeChange", "archetypeApplied", "archetypeFixture",
            "automaticMusketRankAfterChange", "parentEligibleAfterChange",
            "childEligibilityAfterChange", "holdAfterChange",
            "invalidChildRankAfterChange", "childSelectableAfterChange",
            "childSelectabilitySource", "archetypeRemoved", "restoredFixture",
            "automaticMusketRankAfterRemoval", "childEligibilityAfterRemoval" };

        /// <summary>A natively built Musket Master.</summary>
        internal static readonly string[] MusketMasterKeys = {
            "levelOneGrantsRapidReloadMusket", "reservation", "slotPresent",
            "fixture", "gunslingerLevel", "isMusketMasterArchetype",
            "automaticMusketRank", "parentCanSelect", "parentSelected",
            "childStatePresent", "ownedChildOffered", "ownedChildCanSelect",
            "ownedChildSelected", "incompatibleChildCanSelect",
            "legalChildCanSelect", "legalChildSelected",
            "completeBeforeConfirmation", "confirmationApplied",
            "acquiredLegalChild", "acquiredOwnedChildRank",
            "acquiredIncompatibleChildRank", "featSlotsConsumed" };

        /// <summary>Real Gunslinger levels with every firearm-proficiency fact
        /// removed.</summary>
        internal static readonly string[] ClassIdentityControlKeys = {
            "buildCompleteBeforeConfirmation", "buildConfirmationApplied",
            "committedGunslingerLevel", "committedFixture",
            "previewGunslingerLevel", "previewFixture", "parentEligible",
            "childEligibility", "reservation", "slotPresent", "parentOffered",
            "parentCanSelect", "parentSelected" };

        /// <summary>The registered parent's and official children's
        /// engine-recognized classification (BlueprintFeature.HasGroup), next
        /// to a native combat feat read through the same call.</summary>
        internal static readonly string[] ClassificationKeys = {
            "parentHasFeatGroup", "parentHasCombatFeatGroup",
            "childrenHaveFeatGroup", "childrenHaveCombatFeatGroup",
            "nativeReferenceHasFeatGroup", "nativeReferenceHasCombatFeatGroup",
            "parentHideInUI", "parentHideNotAvailableInUI" };

        /// <summary>A real Gunslinger 1 taking its first Fighter level and
        /// acquiring Rapid Reload through that level's Fighter bonus combat feat
        /// slot, as offered by the slot's own native choice generation.</summary>
        internal static readonly string[] FighterBonusSlotKeys = {
            "case", "buildCompleteBeforeConfirmation", "buildConfirmationApplied",
            "builtGunslingerLevel", "reservation", "slotPresent", "fixture",
            "previewFighterLevel", "ordinaryFeatSlotOpen", "parentOffered",
            "parentCanSelect", "parentSelected", "childOffered", "childCanSelect",
            "childSelected", "parentHeldByFighterSlot", "parentHeldByOrdinarySlot",
            "completeBeforeConfirmation", "confirmationApplied",
            "confirmedFighterLevel", "acquiredChild" };

        internal static bool RequireObservations(JObject row, string label,
            IEnumerable<string> keys, IList<string> failures)
        {
            if (failures == null) throw new ArgumentNullException("failures");
            if (keys == null) throw new ArgumentNullException("keys");
            if (row == null)
            {
                failures.Add(label + ":observation-row-missing");
                return false;
            }
            bool complete = true;
            foreach (string key in keys)
            {
                JToken value = row[key];
                if (value == null || value.Type == JTokenType.Null)
                {
                    failures.Add(label + ":missing-observation:" + key);
                    complete = false;
                }
            }
            return complete;
        }

        /// <summary>The fixture must positively prove the exact proficiency
        /// facts its case declared. An intended grant is never evidence; only
        /// the observed ranks are.</summary>
        internal static bool EvaluateFixtureProficiency(JObject fixture,
            string label, bool expectFull, bool expectOneHanded,
            bool expectTwoHanded, IList<string> failures)
        {
            if (!RequireObservations(fixture, label, FixtureProficiencyKeys, failures))
                return false;
            bool exact = true;
            exact &= CheckRank(fixture, label, "fullProficiencyRank", expectFull, failures);
            exact &= CheckRank(fixture, label, "oneHandedProficiencyRank",
                expectOneHanded, failures);
            exact &= CheckRank(fixture, label, "twoHandedProficiencyRank",
                expectTwoHanded, failures);
            return exact;
        }

        /// <summary>The scoped entry point every case evaluator uses, so the
        /// declared scope is always checked by the same call the runtime
        /// scenario makes.</summary>
        private static bool EvaluateScope(JObject row, string label, string key,
            RapidReloadExpectedScope scope, IList<string> failures)
        {
            if (scope == null)
            {
                failures.Add(label + ":expected-scope-missing");
                return false;
            }
            JObject fixture = row[key] as JObject;
            if (fixture == null)
            {
                failures.Add(label + ":" + key + "-not-an-object");
                return false;
            }
            return EvaluateFixtureProficiency(fixture, label + "." + scope.Name,
                scope.Full, scope.OneHanded, scope.TwoHanded, failures);
        }

        private static bool CheckRank(JObject fixture, string label, string key,
            bool expectPresent, IList<string> failures)
        {
            JToken token = fixture[key];
            if (token == null || token.Type != JTokenType.Integer)
            {
                failures.Add(label + ":" + key + "-not-an-integer");
                return false;
            }
            int rank = (int)token;
            if (expectPresent ? rank <= 0 : rank != 0)
            {
                failures.Add(label + ":" + key + "=" + rank);
                return false;
            }
            return true;
        }

        /// <summary>The case must prove it really held the native feat slot it
        /// claims to have exercised: a filler feat in that slot is a setup
        /// failure, not evidence of a refusal.</summary>
        private static bool EvaluateReservation(JObject row, string label,
            IList<string> failures)
        {
            JObject reservation = row["reservation"] as JObject;
            if (reservation == null)
            {
                failures.Add(label + ":reservation-not-an-object");
                return false;
            }
            if (!RequireObservations(reservation, label + ".reservation",
                ReservationKeys, failures)) return false;
            bool ok = true;
            if (!(bool)reservation["reservedSlotResolved"])
            { failures.Add(label + ":reserved-slot-not-resolved"); ok = false; }
            if (!(bool)reservation["reservedSlotUnselected"])
            { failures.Add(label + ":reserved-slot-already-filled"); ok = false; }
            if (!(bool)reservation["reservedSlotSelectionMatches"])
            { failures.Add(label + ":reserved-slot-wrong-selection"); ok = false; }
            if ((int)reservation["reservedIndex"] < 0)
            { failures.Add(label + ":reserved-slot-index"); ok = false; }
            if (!(bool)row["slotPresent"])
            { failures.Add(label + ":slot-absent"); ok = false; }
            return ok;
        }

        /// <summary>A claimed successful confirmation must observe the native
        /// completion gate as true immediately before the level is applied.
        /// </summary>
        private static bool EvaluateConfirmation(JObject row, string label,
            string completeKey, string appliedKey, IList<string> failures)
        {
            bool ok = true;
            JToken complete = row[completeKey];
            if (complete == null || complete.Type != JTokenType.Boolean)
            { failures.Add(label + ":" + completeKey + "-not-observed"); return false; }
            if (!(bool)complete)
            { failures.Add(label + ":native-completion-refused-the-build"); ok = false; }
            JToken applied = row[appliedKey];
            if (applied == null || applied.Type != JTokenType.Boolean)
            { failures.Add(label + ":" + appliedKey + "-not-observed"); return false; }
            if (!(bool)applied)
            { failures.Add(label + ":level-was-not-applied"); ok = false; }
            return ok;
        }

        /// <summary>
        /// R6: a cancellation failure recorded on a case row fails that case.
        /// Descriptive error text is not a failed assertion, so any row that
        /// carries `cleanupError` is rejected here even when its behavioural
        /// observations all look correct.
        /// </summary>
        private static bool EvaluateCleanup(JObject row, string label,
            IList<string> failures)
        {
            JToken error = row["cleanupError"];
            if (error == null || error.Type == JTokenType.Null) return true;
            failures.Add(label + RapidReloadVisitCleanupRules.CleanupFailurePrefix +
                (string)error);
            return false;
        }

        private static bool EvaluateHold(JObject row, string label, string key,
            bool expectParentHeld, bool expectTrackedChild, IList<string> failures)
        {
            JObject hold = row[key] as JObject;
            if (hold == null)
            { failures.Add(label + ":" + key + "-not-an-object"); return false; }
            bool ok = true;
            foreach (string field in new[] { "parentHeld", "nestedStatePresent",
                "trackedChildStillSelected", "trackedChildRank" })
                if (hold[field] == null || hold[field].Type == JTokenType.Null)
                { failures.Add(label + ":" + key + "-missing:" + field); ok = false; }
            if (!ok) return false;
            if ((bool)hold["parentHeld"] != expectParentHeld)
            { failures.Add(label + ":" + key + ".parentHeld=" + hold["parentHeld"]); ok = false; }
            if ((bool)hold["trackedChildStillSelected"] != expectTrackedChild)
            {
                failures.Add(label + ":" + key + ".trackedChildStillSelected=" +
                    hold["trackedChildStillSelected"]);
                ok = false;
            }
            if (!expectTrackedChild && (int)hold["trackedChildRank"] != 0)
            { failures.Add(label + ":" + key + ".trackedChildRank=" + hold["trackedChildRank"]); ok = false; }
            return ok;
        }

        internal static bool EvaluateRefusedParent(JObject row,
            RapidReloadExpectedScope scope, IList<string> failures)
        {
            string label = Label(row, "refused-parent");
            if (!RequireObservations(row, label, RefusedParentKeys, failures)) return false;
            bool ok = EvaluateCleanup(row, label, failures);
            ok &= EvaluateScope(row, label, "fixture", scope, failures);
            ok &= EvaluateReservation(row, label, failures);
            // The feat may remain visible while the native UI shows an unmet
            // prerequisite, but it must not be acquirable.
            if (!(bool)row["parentOffered"])
            { failures.Add(label + ":parent-not-offered"); ok = false; }
            if ((bool)row["parentCanSelect"])
            { failures.Add(label + ":parent-can-select"); ok = false; }
            if ((bool)row["parentSelected"])
            { failures.Add(label + ":parent-selected"); ok = false; }
            ok &= EvaluateHold(row, label, "hold", false, false, failures);
            JArray children = row["childEligibility"] as JArray;
            if (children == null || children.Count == 0)
            { failures.Add(label + ":child-eligibility-missing"); ok = false; }
            else if (children.Any(value => value.Type == JTokenType.Boolean && (bool)value))
            { failures.Add(label + ":child-eligible-without-proficiency"); ok = false; }
            // The reservation is released and the level then completes normally,
            // proving the character was otherwise finishable and still has no
            // Rapid Reload.
            if (!(bool)row["reservationReleasedBeforeConfirmation"])
            { failures.Add(label + ":reservation-not-released"); ok = false; }
            ok &= EvaluateConfirmation(row, label, "completeBeforeConfirmation",
                "confirmationApplied", failures);
            if ((bool)row["acquiredParent"] || (bool)row["acquiredChild"])
            { failures.Add(label + ":acquired-without-proficiency"); ok = false; }
            return ok;
        }

        internal static bool EvaluateScopedChildRefusal(JObject row,
            RapidReloadExpectedScope scope, IList<string> failures)
        {
            string label = Label(row, "scoped-child-refusal");
            if (!RequireObservations(row, label, ScopedChildRefusalKeys, failures))
                return false;
            bool ok = EvaluateCleanup(row, label, failures);
            ok &= EvaluateScope(row, label, "fixture", scope, failures);
            ok &= EvaluateReservation(row, label, failures);
            if (!(bool)row["parentCanSelect"] || !(bool)row["parentSelected"])
            { failures.Add(label + ":parent-not-selected"); ok = false; }
            if (!(bool)row["childStatePresent"])
            { failures.Add(label + ":nested-child-state-absent"); ok = false; }
            if (!(bool)row["refusedChildOffered"])
            { failures.Add(label + ":refused-child-not-offered"); ok = false; }
            if ((bool)row["refusedChildCanSelect"] || (bool)row["refusedChildSelected"])
            { failures.Add(label + ":out-of-scope-child-accepted"); ok = false; }
            if ((bool)row["childStateSelectedAfterRefusal"])
            { failures.Add(label + ":child-state-selected-after-refusal"); ok = false; }
            // Observed before any cleanup: the engine refuses to complete while
            // the Rapid Reload choice is empty, and the empty choice banks
            // nothing.
            if ((bool)row["completeWhileEmpty"])
            { failures.Add(label + ":complete-while-empty"); ok = false; }
            if (!(bool)row["targetBlocksCompletion"])
            { failures.Add(label + ":completion-block-not-attributable"); ok = false; }
            // The engine applies a chosen selection's own feature to the preview
            // at once (observed natively: parentRankWhileEmpty = 1), exactly as
            // for any selection feat. That is not a firearm grant: the refused
            // firearm must hold no rank, and the empty choice must still block
            // completion (checked above). parentRankWhileEmpty stays recorded.
            if ((int)row["refusedChildRankWhileEmpty"] != 0)
            { failures.Add(label + ":empty-selection-granted-a-firearm"); ok = false; }
            // Attribution: the same build completes once a legal choice is made.
            if (!(bool)row["legalChildSelected"])
            { failures.Add(label + ":legal-child-refused"); ok = false; }
            if ((bool)row["targetBlocksCompletionAfterLegalChoice"])
            { failures.Add(label + ":target-still-blocks-after-legal-choice"); ok = false; }
            ok &= EvaluateConfirmation(row, label, "completeBeforeConfirmation",
                "confirmationApplied", failures);
            if (!(bool)row["acquiredLegalChild"])
            { failures.Add(label + ":legal-child-not-acquired"); ok = false; }
            if ((bool)row["acquiredRefusedChild"])
            { failures.Add(label + ":refused-child-acquired"); ok = false; }
            return ok;
        }

        internal static bool EvaluateEmptySelection(JObject row,
            RapidReloadExpectedScope scope, IList<string> failures)
        {
            string label = Label(row, "empty-selection");
            if (!RequireObservations(row, label, EmptySelectionKeys, failures))
                return false;
            bool ok = EvaluateCleanup(row, label, failures);
            ok &= EvaluateScope(row, label, "fixture", scope, failures);
            ok &= EvaluateReservation(row, label, failures);
            if (!(bool)row["parentSelected"])
            { failures.Add(label + ":parent-not-selected"); ok = false; }
            if (!(bool)row["childStatePresent"])
            { failures.Add(label + ":nested-child-state-absent"); ok = false; }
            if ((bool)row["childStateSelected"])
            { failures.Add(label + ":child-state-unexpectedly-selected"); ok = false; }
            if ((bool)row["completeWhileEmpty"])
            { failures.Add(label + ":native-completion-allowed-an-empty-choice"); ok = false; }
            if (!(bool)row["targetBlocksCompletion"])
            { failures.Add(label + ":completion-block-not-attributable"); ok = false; }
            // This case is explicitly a defensive lower-level application of a
            // build the completion gate refused. It must say so, and it can
            // never stand in for a claimed successful confirmation.
            if (!(bool)row["appliedWithoutNativeCompletion"])
            { failures.Add(label + ":defensive-probe-not-declared"); ok = false; }
            // The probe bypasses the native completion gate, so the engine
            // applies whatever is selected, including the parent selection
            // feature (acquiredParent stays recorded). A player cannot reach
            // this state; what the probe proves is that no firearm is banked.
            if ((bool)row["acquiredAnyChild"])
            { failures.Add(label + ":empty-choice-banked-a-firearm"); ok = false; }
            return ok;
        }

        internal static bool EvaluateAcquisition(JObject row,
            RapidReloadExpectedScope scope, IList<string> failures)
        {
            string label = Label(row, "acquisition");
            if (!RequireObservations(row, label, AcquisitionKeys, failures)) return false;
            bool ok = EvaluateCleanup(row, label, failures);
            ok &= EvaluateScope(row, label, "fixture", scope, failures);
            ok &= EvaluateReservation(row, label, failures);
            if (!(bool)row["parentCanSelect"] || !(bool)row["parentSelected"])
            { failures.Add(label + ":parent-refused"); ok = false; }
            if (!(bool)row["childCanSelect"] || !(bool)row["childSelected"])
            { failures.Add(label + ":legal-child-refused"); ok = false; }
            ok &= EvaluateConfirmation(row, label, "completeBeforeConfirmation",
                "confirmationApplied", failures);
            if (!(bool)row["acquiredChild"])
            { failures.Add(label + ":legal-child-not-acquired"); ok = false; }
            return ok;
        }

        internal static bool EvaluatePendingClassChange(JObject row,
            IList<string> failures)
        {
            const string label = "pending-class-change";
            if (!RequireObservations(row, label, PendingClassChangeKeys, failures))
                return false;
            bool ok = EvaluateCleanup(row, label, failures);
            // One live transaction: the scenario must not have substituted
            // Cancel() plus a fresh visit for the native class change.
            if ((int)row["controllerInstances"] != 1)
            { failures.Add(label + ":not-a-single-transaction"); ok = false; }
            ok &= EvaluateReservation(row, label, failures);
            ok &= EvaluateScope(row, label + ".gunslinger", "gunslingerFixture",
                RapidReloadExpectedScope.AnyFirearm, failures);
            if (!(bool)row["heldBeforeChange"])
            { failures.Add(label + ":choice-was-never-held"); ok = false; }
            ok &= EvaluateScope(row, label + ".fighter", "fighterFixture",
                RapidReloadExpectedScope.NoProficiency, failures);
            if ((int)row["fighterGunslingerLevel"] != 0)
            { failures.Add(label + ":pending-class-did-not-change"); ok = false; }
            if ((bool)row["parentEligibleAfterChange"])
            { failures.Add(label + ":parent-eligible-after-change"); ok = false; }
            if (AnyTrue(row["childEligibilityAfterChange"]))
            { failures.Add(label + ":child-eligible-after-change"); ok = false; }
            // The parent itself stops qualifying as a Fighter, so the engine
            // must drop both the parent and the exact child.
            ok &= EvaluateHold(row, label, "holdAfterChange", false, false, failures);
            ok &= EvaluateScope(row, label + ".restored", "restoredFixture",
                RapidReloadExpectedScope.AnyFirearm, failures);
            if (!(bool)row["parentEligibleAfterRestore"])
            { failures.Add(label + ":parent-not-eligible-after-restore"); ok = false; }
            if (!AllTrue(row["childEligibilityAfterRestore"]))
            { failures.Add(label + ":child-not-eligible-after-restore"); ok = false; }
            ok &= EvaluateHold(row, label, "holdAfterRestore", false, false, failures);
            if (!(bool)row["reheldBeforeSecondChange"])
            { failures.Add(label + ":choice-was-never-reheld"); ok = false; }
            ok &= EvaluateHold(row, label, "holdAfterSecondChange", false, false, failures);
            if (!(bool)row["reservationReleasedBeforeConfirmation"])
            { failures.Add(label + ":reservation-not-released"); ok = false; }
            ok &= EvaluateConfirmation(row, label, "completeBeforeConfirmation",
                "confirmationApplied", failures);
            if ((int)row["confirmedFighterLevel"] <= 0)
            { failures.Add(label + ":confirmation-did-not-apply"); ok = false; }
            if ((int)row["confirmedGunslingerLevel"] != 0)
            { failures.Add(label + ":confirmation-kept-gunslinger-levels"); ok = false; }
            if ((bool)row["acquiredParentAfterConfirmation"] ||
                (bool)row["acquiredAnyChildAfterConfirmation"])
            { failures.Add(label + ":stale-choice-survived-confirmation"); ok = false; }
            return ok;
        }

        internal static bool EvaluateArchetypeScopeChange(JObject row,
            IList<string> failures)
        {
            const string label = "pending-archetype-change";
            if (!RequireObservations(row, label, ArchetypeScopeChangeKeys, failures))
                return false;
            bool ok = EvaluateCleanup(row, label, failures);
            if ((int)row["controllerInstances"] != 1)
            { failures.Add(label + ":not-a-single-transaction"); ok = false; }
            ok &= EvaluateReservation(row, label, failures);
            ok &= EvaluateScope(row, label + ".base", "baseFixture",
                RapidReloadExpectedScope.AnyFirearm, failures);
            if (!(bool)row["heldBeforeChange"])
            { failures.Add(label + ":choice-was-never-held"); ok = false; }
            if (!(bool)row["archetypeApplied"])
            { failures.Add(label + ":archetype-not-applied"); ok = false; }
            // Musket Master narrows the scope to two-handed firearms.
            ok &= EvaluateScope(row, label + ".archetype", "archetypeFixture",
                RapidReloadExpectedScope.TwoHandedOnly, failures);
            if ((int)row["automaticMusketRankAfterChange"] <= 0)
            { failures.Add(label + ":archetype-grant-missing"); ok = false; }
            // The parent still qualifies through two-handed proficiency, so the
            // engine may legitimately keep it. What must not survive is the
            // now-invalid one-handed child: it must be neither selected nor
            // granted, and it must not be selectable.
            JObject hold = row["holdAfterChange"] as JObject;
            if (hold == null)
            { failures.Add(label + ":holdAfterChange-not-an-object"); ok = false; }
            else
            {
                if (hold["trackedChildStillSelected"] == null ||
                    hold["trackedChildStillSelected"].Type != JTokenType.Boolean)
                { failures.Add(label + ":holdAfterChange-missing:trackedChildStillSelected"); ok = false; }
                else if ((bool)hold["trackedChildStillSelected"])
                { failures.Add(label + ":invalid-child-survived-the-archetype-change"); ok = false; }
                if (hold["parentHeld"] == null ||
                    hold["parentHeld"].Type != JTokenType.Boolean)
                { failures.Add(label + ":holdAfterChange-missing:parentHeld"); ok = false; }
            }
            if ((int)row["invalidChildRankAfterChange"] != 0)
            { failures.Add(label + ":invalid-child-was-granted"); ok = false; }
            if (!(bool)row["parentEligibleAfterChange"])
            { failures.Add(label + ":parent-not-eligible-under-archetype"); ok = false; }
            JArray eligibility = row["childEligibilityAfterChange"] as JArray;
            JArray selectable = row["childSelectableAfterChange"] as JArray;
            if (eligibility == null || eligibility.Count != 3 ||
                selectable == null || selectable.Count != 3)
            { failures.Add(label + ":child-observation-shape"); ok = false; }
            else
            {
                // Pistol falls outside the Musket Master proficiency scope.
                // Musket is inside it but is already owned through the
                // archetype's automatic grant, which the native rank check
                // refuses on its own. Blunderbuss stays acquirable.
                if ((bool)eligibility[0]) { failures.Add(label + ":pistol-eligible-under-archetype"); ok = false; }
                if ((bool)eligibility[1]) { failures.Add(label + ":owned-musket-eligible-under-archetype"); ok = false; }
                if (!(bool)eligibility[2]) { failures.Add(label + ":blunderbuss-not-eligible-under-archetype"); ok = false; }
                if ((bool)selectable[0]) { failures.Add(label + ":pistol-selectable-under-archetype"); ok = false; }
                if ((bool)selectable[1]) { failures.Add(label + ":owned-musket-selectable-under-archetype"); ok = false; }
                if (!(bool)selectable[2]) { failures.Add(label + ":blunderbuss-not-selectable-under-archetype"); ok = false; }
            }
            string source = (string)row["childSelectabilitySource"];
            if (source != "retained-nested-state" && source != "probe")
            { failures.Add(label + ":child-selectability-source=" + source); ok = false; }
            if (!(bool)row["archetypeRemoved"])
            { failures.Add(label + ":archetype-not-removed"); ok = false; }
            ok &= EvaluateScope(row, label + ".restored", "restoredFixture",
                RapidReloadExpectedScope.AnyFirearm, failures);
            if ((int)row["automaticMusketRankAfterRemoval"] != 0)
            { failures.Add(label + ":archetype-grant-survived-removal"); ok = false; }
            if (!AllTrue(row["childEligibilityAfterRemoval"]))
            { failures.Add(label + ":child-eligibility-did-not-return"); ok = false; }
            return ok;
        }

        internal static bool EvaluateMusketMaster(JObject row, IList<string> failures)
        {
            const string label = "musket-master";
            if (!RequireObservations(row, label, MusketMasterKeys, failures)) return false;
            bool ok = EvaluateCleanup(row, label, failures);
            if (!(bool)row["levelOneGrantsRapidReloadMusket"])
            { failures.Add(label + ":level-one-blueprint-grant-missing"); ok = false; }
            // The fixture is a natively built Musket Master: scope is
            // two-handed only, and nothing was granted by hand.
            ok &= EvaluateScope(row, label, "fixture",
                RapidReloadExpectedScope.TwoHandedOnly, failures);
            ok &= EvaluateReservation(row, label, failures);
            if ((int)row["gunslingerLevel"] <= 0)
            { failures.Add(label + ":no-gunslinger-level"); ok = false; }
            if (!(bool)row["isMusketMasterArchetype"])
            { failures.Add(label + ":archetype-identity-absent"); ok = false; }
            if ((int)row["automaticMusketRank"] <= 0)
            { failures.Add(label + ":automatic-musket-grant-missing"); ok = false; }
            if (!(bool)row["parentCanSelect"] || !(bool)row["parentSelected"])
            { failures.Add(label + ":parent-refused"); ok = false; }
            if (!(bool)row["childStatePresent"])
            { failures.Add(label + ":nested-child-state-absent"); ok = false; }
            if (!(bool)row["ownedChildOffered"])
            { failures.Add(label + ":owned-child-not-offered"); ok = false; }
            if ((bool)row["ownedChildCanSelect"] || (bool)row["ownedChildSelected"])
            { failures.Add(label + ":owned-child-consumed-another-feat"); ok = false; }
            if ((bool)row["incompatibleChildCanSelect"])
            { failures.Add(label + ":out-of-scope-child-selectable"); ok = false; }
            if (!(bool)row["legalChildCanSelect"] || !(bool)row["legalChildSelected"])
            { failures.Add(label + ":legal-unowned-child-refused"); ok = false; }
            ok &= EvaluateConfirmation(row, label, "completeBeforeConfirmation",
                "confirmationApplied", failures);
            if (!(bool)row["acquiredLegalChild"])
            { failures.Add(label + ":legal-child-not-acquired"); ok = false; }
            if ((int)row["acquiredOwnedChildRank"] != 1)
            { failures.Add(label + ":automatic-grant-rank-changed"); ok = false; }
            if ((int)row["acquiredIncompatibleChildRank"] != 0)
            { failures.Add(label + ":out-of-scope-child-acquired"); ok = false; }
            if ((int)row["featSlotsConsumed"] != 1)
            { failures.Add(label + ":feat-slot-accounting"); ok = false; }
            return ok;
        }

        internal static bool EvaluateClassIdentityControl(JObject row,
            IList<string> failures)
        {
            const string label = "class-identity-control";
            if (!RequireObservations(row, label, ClassIdentityControlKeys, failures))
                return false;
            bool ok = EvaluateCleanup(row, label, failures);
            // The Gunslinger the control strips was itself confirmed through
            // the native completion gate.
            ok &= EvaluateConfirmation(row, label + ".build",
                "buildCompleteBeforeConfirmation", "buildConfirmationApplied", failures);
            // A genuine Gunslinger: real class levels, no proficiency facts at
            // evaluation time. If either half is unproven the control is
            // invalid and must fail rather than score as a negative result.
            if ((int)row["committedGunslingerLevel"] <= 0)
            { failures.Add(label + ":no-committed-gunslinger-level"); ok = false; }
            ok &= EvaluateScope(row, label + ".committed", "committedFixture",
                RapidReloadExpectedScope.NoProficiency, failures);
            if ((int)row["previewGunslingerLevel"] <= 0)
            { failures.Add(label + ":preview-lost-gunslinger-identity"); ok = false; }
            ok &= EvaluateScope(row, label + ".preview", "previewFixture",
                RapidReloadExpectedScope.NoProficiency, failures);
            if ((bool)row["parentEligible"])
            { failures.Add(label + ":class-identity-alone-qualified"); ok = false; }
            if (AnyTrue(row["childEligibility"]))
            { failures.Add(label + ":child-qualified-on-class-identity"); ok = false; }
            ok &= EvaluateReservation(row, label, failures);
            if (!(bool)row["parentOffered"])
            { failures.Add(label + ":parent-not-offered"); ok = false; }
            if ((bool)row["parentCanSelect"] || (bool)row["parentSelected"])
            { failures.Add(label + ":parent-selectable-on-class-identity"); ok = false; }
            return ok;
        }

        /// <summary>The parent must be a feat and a combat feat by the same
        /// native call that classifies native combat feats. Catalog membership
        /// alone never satisfies this.</summary>
        internal static bool EvaluateClassification(JObject row,
            IList<string> failures)
        {
            const string label = "classification";
            if (!RequireObservations(row, label, ClassificationKeys, failures))
                return false;
            bool ok = true;
            if (!(bool)row["nativeReferenceHasFeatGroup"] ||
                !(bool)row["nativeReferenceHasCombatFeatGroup"])
            { failures.Add(label + ":native-reference-not-a-combat-feat"); ok = false; }
            if (!(bool)row["parentHasFeatGroup"])
            { failures.Add(label + ":parent-not-a-feat"); ok = false; }
            if (!(bool)row["parentHasCombatFeatGroup"])
            { failures.Add(label + ":parent-not-a-combat-feat"); ok = false; }
            if (!AllTrue(row["childrenHaveFeatGroup"]))
            { failures.Add(label + ":child-not-a-feat"); ok = false; }
            if (!AllTrue(row["childrenHaveCombatFeatGroup"]))
            { failures.Add(label + ":child-not-a-combat-feat"); ok = false; }
            if ((bool)row["parentHideInUI"] || (bool)row["parentHideNotAvailableInUI"])
            { failures.Add(label + ":parent-hidden-from-menus"); ok = false; }
            return ok;
        }

        internal static bool EvaluateFighterBonusSlot(JObject row,
            IList<string> failures)
        {
            string label = Label(row, "fighter-bonus-slot");
            if (!RequireObservations(row, label, FighterBonusSlotKeys, failures))
                return false;
            bool ok = EvaluateCleanup(row, label, failures);
            // The Gunslinger level is itself built and confirmed natively, so
            // its class package is the only proficiency source.
            ok &= EvaluateConfirmation(row, label + ".build",
                "buildCompleteBeforeConfirmation", "buildConfirmationApplied", failures);
            if ((int)row["builtGunslingerLevel"] != 1)
            { failures.Add(label + ":gunslinger-level=" + row["builtGunslingerLevel"]); ok = false; }
            ok &= EvaluateScope(row, label, "fixture",
                RapidReloadExpectedScope.AnyFirearm, failures);
            ok &= EvaluateReservation(row, label, failures);
            if ((int)row["previewFighterLevel"] != 1)
            { failures.Add(label + ":fighter-level=" + row["previewFighterLevel"]); ok = false; }
            // Character level two grants no ordinary feat, so the only feat
            // this visit can consume is the Fighter bonus combat feat.
            if ((bool)row["ordinaryFeatSlotOpen"])
            { failures.Add(label + ":ordinary-feat-slot-present"); ok = false; }
            if (!(bool)row["parentOffered"])
            { failures.Add(label + ":parent-not-offered-by-combat-menu"); ok = false; }
            if (!(bool)row["parentCanSelect"] || !(bool)row["parentSelected"])
            { failures.Add(label + ":parent-refused"); ok = false; }
            if (!(bool)row["childOffered"] || !(bool)row["childCanSelect"] ||
                !(bool)row["childSelected"])
            { failures.Add(label + ":legal-child-refused"); ok = false; }
            if (!(bool)row["parentHeldByFighterSlot"])
            { failures.Add(label + ":fighter-slot-not-consumed"); ok = false; }
            if ((bool)row["parentHeldByOrdinarySlot"])
            { failures.Add(label + ":ordinary-slot-consumed"); ok = false; }
            ok &= EvaluateConfirmation(row, label, "completeBeforeConfirmation",
                "confirmationApplied", failures);
            if ((int)row["confirmedFighterLevel"] != 1)
            { failures.Add(label + ":fighter-level-not-applied"); ok = false; }
            if (!(bool)row["acquiredChild"])
            { failures.Add(label + ":legal-child-not-acquired"); ok = false; }
            return ok;
        }

        private static string Label(JObject row, string fallback)
        {
            if (row == null) return fallback;
            JToken value = row["case"];
            return value == null || value.Type == JTokenType.Null
                ? fallback : (string)value;
        }

        private static bool AnyTrue(JToken token)
        {
            JArray array = token as JArray;
            return array != null && array.Any(value => value.Type == JTokenType.Boolean &&
                (bool)value);
        }

        private static bool AllTrue(JToken token)
        {
            JArray array = token as JArray;
            return array != null && array.Count > 0 &&
                array.All(value => value.Type == JTokenType.Boolean && (bool)value);
        }
    }
}
