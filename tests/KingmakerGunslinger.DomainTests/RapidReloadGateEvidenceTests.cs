using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.RuntimeTesting;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Regression coverage for the way the Rapid Reload proficiency-gate
    /// runtime scenario scores its own evidence. These tests do not prove the
    /// registered blueprints or the native selection flow — only the guarded
    /// scenario run can do that. They prove that a run which skipped a native
    /// operation, lost a precondition, never really held the feat slot it
    /// claims, silently used a different proficiency fixture, repaired an
    /// invalid state, substituted cancelled visits for a real pending-build
    /// change, or applied a build the native completion gate refused cannot be
    /// scored as a pass. Every mutation is scored through the same evaluator
    /// entry point the runtime scenario calls.
    /// </summary>
    internal static class RapidReloadGateEvidenceTests
    {
        private static JObject Fixture(int full, int one, int two)
        {
            return new JObject {
                ["fullProficiencyRank"] = full,
                ["oneHandedProficiencyRank"] = one,
                ["twoHandedProficiencyRank"] = two };
        }

        private static JObject Reservation(string selection)
        {
            return new JObject {
                ["reservedSelection"] = selection,
                ["reservedIndex"] = 0,
                ["reservedLevel"] = 1,
                ["reservedSlotResolved"] = true,
                ["reservedSlotUnselected"] = true,
                ["reservedSlotSelectionMatches"] = true,
                ["released"] = false };
        }

        private static JObject Hold(bool parentHeld, bool trackedChildSelected,
            int trackedChildRank)
        {
            return new JObject {
                ["parentHeld"] = parentHeld,
                ["parentHoldCount"] = parentHeld ? 1 : 0,
                ["nestedStatePresent"] = parentHeld,
                ["selectedChildName"] = trackedChildSelected ?
                    "KMG_RapidReload_Pistol" : "<none>",
                ["trackedChild"] = "KMG_RapidReload_Pistol",
                ["trackedChildStillSelected"] = trackedChildSelected,
                ["trackedChildRank"] = trackedChildRank };
        }

        private static JArray Bools(params bool[] values)
        {
            return new JArray(values.Select(value => (object)value));
        }

        internal static JObject RefusedParent()
        {
            return new JObject {
                ["case"] = "A.ordinary-feat-no-proficiency",
                ["reservation"] = Reservation("BasicFeatSelection"),
                ["slotPresent"] = true,
                ["fixture"] = Fixture(0, 0, 0),
                ["parentOffered"] = true,
                ["parentCanSelect"] = false,
                ["parentSelected"] = false,
                ["hold"] = Hold(false, false, 0),
                ["childEligibility"] = Bools(false, false, false),
                ["reservationReleasedBeforeConfirmation"] = true,
                ["completeBeforeConfirmation"] = true,
                ["confirmationApplied"] = true,
                ["acquiredParent"] = false,
                ["acquiredChild"] = false };
        }

        internal static JObject ScopedChildRefusal()
        {
            return new JObject {
                ["case"] = "B.one-handed-refuses-musket",
                ["reservation"] = Reservation("BasicFeatSelection"),
                ["slotPresent"] = true,
                ["fixture"] = Fixture(0, 1, 0),
                ["parentCanSelect"] = true,
                ["parentSelected"] = true,
                ["childStatePresent"] = true,
                ["refusedChildOffered"] = true,
                ["refusedChildCanSelect"] = false,
                ["refusedChildSelected"] = false,
                ["childStateSelectedAfterRefusal"] = false,
                ["completeWhileEmpty"] = false,
                ["targetBlocksCompletion"] = true,
                ["parentRankWhileEmpty"] = 0,
                ["refusedChildRankWhileEmpty"] = 0,
                ["legalChildSelected"] = true,
                ["targetBlocksCompletionAfterLegalChoice"] = false,
                ["completeBeforeConfirmation"] = true,
                ["confirmationApplied"] = true,
                ["acquiredLegalChild"] = true,
                ["acquiredRefusedChild"] = false };
        }

        internal static JObject EmptySelection()
        {
            return new JObject {
                ["case"] = "B.empty-selection-blocked",
                ["reservation"] = Reservation("BasicFeatSelection"),
                ["slotPresent"] = true,
                ["fixture"] = Fixture(1, 0, 0),
                ["parentSelected"] = true,
                ["childStatePresent"] = true,
                ["childStateSelected"] = false,
                ["completeWhileEmpty"] = false,
                ["targetBlocksCompletion"] = true,
                ["appliedWithoutNativeCompletion"] = true,
                ["acquiredParent"] = false,
                ["acquiredAnyChild"] = false };
        }

        internal static JObject Acquisition()
        {
            return new JObject {
                ["case"] = "C.ordinary-feat-full-proficiency",
                ["reservation"] = Reservation("BasicFeatSelection"),
                ["slotPresent"] = true,
                ["fixture"] = Fixture(1, 0, 0),
                ["parentCanSelect"] = true,
                ["parentSelected"] = true,
                ["childCanSelect"] = true,
                ["childSelected"] = true,
                ["completeBeforeConfirmation"] = true,
                ["confirmationApplied"] = true,
                ["acquiredChild"] = true };
        }

        internal static JObject PendingClassChange()
        {
            return new JObject {
                ["controllerInstances"] = 1,
                ["reservation"] = Reservation("BasicFeatSelection"),
                ["slotPresent"] = true,
                ["gunslingerFixture"] = Fixture(1, 0, 0),
                ["heldBeforeChange"] = true,
                ["heldChildName"] = "KMG_RapidReload_Pistol",
                ["fighterFixture"] = Fixture(0, 0, 0),
                ["fighterGunslingerLevel"] = 0,
                ["parentEligibleAfterChange"] = false,
                ["childEligibilityAfterChange"] = Bools(false, false, false),
                ["holdAfterChange"] = Hold(false, false, 0),
                ["restoredFixture"] = Fixture(1, 0, 0),
                ["parentEligibleAfterRestore"] = true,
                ["childEligibilityAfterRestore"] = Bools(true, true, true),
                ["holdAfterRestore"] = Hold(false, false, 0),
                ["reheldBeforeSecondChange"] = true,
                ["holdAfterSecondChange"] = Hold(false, false, 0),
                ["reservationReleasedBeforeConfirmation"] = true,
                ["completeBeforeConfirmation"] = true,
                ["confirmationApplied"] = true,
                ["confirmedFighterLevel"] = 1,
                ["confirmedGunslingerLevel"] = 0,
                ["acquiredParentAfterConfirmation"] = false,
                ["acquiredAnyChildAfterConfirmation"] = false };
        }

        /// <summary>
        /// The accepted Musket Master transition: the parent still qualifies
        /// through two-handed proficiency and the engine keeps it, while the
        /// now-invalid Pistol child is cleared.
        /// </summary>
        internal static JObject ArchetypeScopeChange()
        {
            return new JObject {
                ["controllerInstances"] = 1,
                ["reservation"] = Reservation("BasicFeatSelection"),
                ["slotPresent"] = true,
                ["baseFixture"] = Fixture(1, 0, 0),
                ["heldBeforeChange"] = true,
                ["archetypeApplied"] = true,
                ["archetypeFixture"] = Fixture(0, 0, 1),
                ["automaticMusketRankAfterChange"] = 1,
                ["parentEligibleAfterChange"] = true,
                ["childEligibilityAfterChange"] = Bools(false, false, true),
                ["holdAfterChange"] = Hold(true, false, 0),
                ["invalidChildRankAfterChange"] = 0,
                ["childSelectableAfterChange"] = Bools(false, false, true),
                ["childSelectabilitySource"] = "retained-nested-state",
                ["archetypeRemoved"] = true,
                ["restoredFixture"] = Fixture(1, 0, 0),
                ["automaticMusketRankAfterRemoval"] = 0,
                ["childEligibilityAfterRemoval"] = Bools(true, true, true) };
        }

        internal static JObject MusketMaster()
        {
            return new JObject {
                ["levelOneGrantsRapidReloadMusket"] = true,
                ["reservation"] = Reservation("BasicFeatSelection"),
                ["slotPresent"] = true,
                ["fixture"] = Fixture(0, 0, 1),
                ["gunslingerLevel"] = 1,
                ["isMusketMasterArchetype"] = true,
                ["automaticMusketRank"] = 1,
                ["parentCanSelect"] = true,
                ["parentSelected"] = true,
                ["childStatePresent"] = true,
                ["ownedChildOffered"] = true,
                ["ownedChildCanSelect"] = false,
                ["ownedChildSelected"] = false,
                ["incompatibleChildCanSelect"] = false,
                ["legalChildCanSelect"] = true,
                ["legalChildSelected"] = true,
                ["completeBeforeConfirmation"] = true,
                ["confirmationApplied"] = true,
                ["acquiredLegalChild"] = true,
                ["acquiredOwnedChildRank"] = 1,
                ["acquiredIncompatibleChildRank"] = 0,
                ["featSlotsConsumed"] = 1 };
        }

        internal static JObject ClassIdentityControl()
        {
            return new JObject {
                ["buildCompleteBeforeConfirmation"] = true,
                ["buildConfirmationApplied"] = true,
                ["committedGunslingerLevel"] = 1,
                ["committedFixture"] = Fixture(0, 0, 0),
                ["previewGunslingerLevel"] = 1,
                ["previewFixture"] = Fixture(0, 0, 0),
                ["parentEligible"] = false,
                ["childEligibility"] = Bools(false, false, false),
                ["reservation"] = Reservation("FighterFeatSelection"),
                ["slotPresent"] = true,
                ["parentOffered"] = true,
                ["parentCanSelect"] = false,
                ["parentSelected"] = false };
        }

        private sealed class EvidenceCase
        {
            internal EvidenceCase(string name, Func<JObject> build, string[] keys,
                Func<JObject, IList<string>, bool> evaluate)
            {
                Name = name;
                Build = build;
                Keys = keys;
                Evaluate = evaluate;
            }

            internal string Name { get; private set; }
            internal Func<JObject> Build { get; private set; }
            internal string[] Keys { get; private set; }
            internal Func<JObject, IList<string>, bool> Evaluate { get; private set; }
        }

        // The scope each case declares is the same one the runtime scenario
        // passes for that case; it is never derived from the measured ranks.
        private static readonly EvidenceCase[] Cases = {
            new EvidenceCase("refused-parent", RefusedParent,
                RapidReloadGateEvidenceRules.RefusedParentKeys,
                (row, failures) => RapidReloadGateEvidenceRules.EvaluateRefusedParent(
                    row, RapidReloadExpectedScope.NoProficiency, failures)),
            new EvidenceCase("scoped-child-refusal", ScopedChildRefusal,
                RapidReloadGateEvidenceRules.ScopedChildRefusalKeys,
                (row, failures) => RapidReloadGateEvidenceRules.EvaluateScopedChildRefusal(
                    row, RapidReloadExpectedScope.OneHandedOnly, failures)),
            new EvidenceCase("empty-selection", EmptySelection,
                RapidReloadGateEvidenceRules.EmptySelectionKeys,
                (row, failures) => RapidReloadGateEvidenceRules.EvaluateEmptySelection(
                    row, RapidReloadExpectedScope.AnyFirearm, failures)),
            new EvidenceCase("acquisition", Acquisition,
                RapidReloadGateEvidenceRules.AcquisitionKeys,
                (row, failures) => RapidReloadGateEvidenceRules.EvaluateAcquisition(
                    row, RapidReloadExpectedScope.AnyFirearm, failures)),
            new EvidenceCase("pending-class-change", PendingClassChange,
                RapidReloadGateEvidenceRules.PendingClassChangeKeys,
                RapidReloadGateEvidenceRules.EvaluatePendingClassChange),
            new EvidenceCase("pending-archetype-change", ArchetypeScopeChange,
                RapidReloadGateEvidenceRules.ArchetypeScopeChangeKeys,
                RapidReloadGateEvidenceRules.EvaluateArchetypeScopeChange),
            new EvidenceCase("musket-master", MusketMaster,
                RapidReloadGateEvidenceRules.MusketMasterKeys,
                RapidReloadGateEvidenceRules.EvaluateMusketMaster),
            new EvidenceCase("class-identity-control", ClassIdentityControl,
                RapidReloadGateEvidenceRules.ClassIdentityControlKeys,
                RapidReloadGateEvidenceRules.EvaluateClassIdentityControl) };

        private static void Reject(EvidenceCase evidence, JObject row, string message,
            string expectedFailureFragment)
        {
            var failures = new List<string>();
            Assertions.False(evidence.Evaluate(row, failures), message);
            Assertions.True(failures.Count > 0,
                evidence.Name + " rejected evidence without a diagnostic: " + message);
            if (expectedFailureFragment != null)
                Assertions.True(
                    failures.Any(value => value.Contains(expectedFailureFragment)),
                    evidence.Name + " did not name " + expectedFailureFragment + ": " +
                    string.Join("|", failures.ToArray()));
        }

        private static EvidenceCase Find(string name)
        {
            return Cases.Single(value => value.Name == name);
        }

        internal static void CompleteEvidenceIsAccepted()
        {
            foreach (EvidenceCase evidence in Cases)
            {
                var failures = new List<string>();
                Assertions.True(evidence.Evaluate(evidence.Build(), failures),
                    "Complete " + evidence.Name + " evidence was rejected: " +
                    string.Join("|", failures.ToArray()));
                Assertions.Equal(0, failures.Count,
                    "Complete " + evidence.Name + " evidence reported failures: " +
                    string.Join("|", failures.ToArray()));
            }
        }

        internal static void MissingObservationsCannotScorePass()
        {
            foreach (EvidenceCase evidence in Cases)
            {
                Assertions.True(evidence.Keys.Length > 0,
                    evidence.Name + " declares no required observations.");
                foreach (string key in evidence.Keys)
                {
                    JObject row = evidence.Build();
                    row.Remove(key);
                    Reject(evidence, row,
                        evidence.Name + " accepted evidence with a missing " + key +
                        " observation.", ":missing-observation:" + key);
                }
            }
        }

        internal static void NullObservationsCannotScorePass()
        {
            foreach (EvidenceCase evidence in Cases)
            {
                JObject row = evidence.Build();
                row[evidence.Keys[0]] = JValue.CreateNull();
                Reject(evidence, row, evidence.Name + " accepted a null observation.",
                    ":missing-observation:" + evidence.Keys[0]);
                var missingRow = new List<string>();
                Assertions.False(evidence.Evaluate(null, missingRow),
                    evidence.Name + " accepted an absent observation row.");
                Assertions.True(missingRow.Any(value =>
                        value.EndsWith(":observation-row-missing", StringComparison.Ordinal)),
                    evidence.Name + " did not name an absent observation row.");
            }
        }

        // R1: a filler feat in the slot the case claims to exercise is a setup
        // failure, not evidence that an invalid feat was refused.
        internal static void UnreservedOrFilledFeatSlotsCannotScorePass()
        {
            foreach (EvidenceCase evidence in Cases)
            {
                JObject unresolved = evidence.Build();
                ((JObject)unresolved["reservation"])["reservedSlotResolved"] = false;
                Reject(evidence, unresolved,
                    evidence.Name + " accepted an unresolved reserved slot.",
                    ":reserved-slot-not-resolved");

                JObject filled = evidence.Build();
                ((JObject)filled["reservation"])["reservedSlotUnselected"] = false;
                Reject(evidence, filled,
                    evidence.Name + " accepted a reserved slot a filler feat already took.",
                    ":reserved-slot-already-filled");

                JObject wrongSelection = evidence.Build();
                ((JObject)wrongSelection["reservation"])["reservedSlotSelectionMatches"] =
                    false;
                Reject(evidence, wrongSelection,
                    evidence.Name + " accepted a slot from the wrong native selection.",
                    ":reserved-slot-wrong-selection");

                JObject absentSlot = evidence.Build();
                absentSlot["slotPresent"] = false;
                Reject(evidence, absentSlot,
                    evidence.Name + " accepted an absent feat slot.", ":slot-absent");

                JObject notAnObject = evidence.Build();
                notAnObject["reservation"] = "BasicFeatSelection";
                Reject(evidence, notAnObject,
                    evidence.Name + " accepted a reservation that is not an object.",
                    ":reservation-not-an-object");

                JObject truncated = evidence.Build();
                ((JObject)truncated["reservation"]).Remove("reservedIndex");
                Reject(evidence, truncated,
                    evidence.Name + " accepted a reservation missing its occurrence index.",
                    ":missing-observation:reservedIndex");
            }
        }

        // R3: the evaluators the runtime scenario actually calls must validate
        // the measured proficiency ranks against the scope the case declared.
        internal static void FixtureProficiencyIsValidatedByTheScoringEvaluators()
        {
            EvidenceCase acquisition = Find("acquisition");
            JObject empty = Acquisition();
            empty["fixture"] = new JObject();
            Reject(acquisition, empty,
                "The acquisition evaluator accepted an empty proficiency fixture.",
                ":missing-observation:fullProficiencyRank");

            JObject missingRank = Acquisition();
            ((JObject)missingRank["fixture"]).Remove("twoHandedProficiencyRank");
            Reject(acquisition, missingRank,
                "The acquisition evaluator accepted a fixture missing a rank.",
                ":missing-observation:twoHandedProficiencyRank");

            JObject nullRank = Acquisition();
            ((JObject)nullRank["fixture"])["oneHandedProficiencyRank"] = JValue.CreateNull();
            Reject(acquisition, nullRank,
                "The acquisition evaluator accepted a null proficiency rank.",
                ":missing-observation:oneHandedProficiencyRank");

            JObject invalidRank = Acquisition();
            ((JObject)invalidRank["fixture"])["fullProficiencyRank"] = "one";
            Reject(acquisition, invalidRank,
                "The acquisition evaluator accepted a non-integer proficiency rank.",
                "fullProficiencyRank-not-an-integer");

            JObject noProficiency = Acquisition();
            noProficiency["fixture"] = Fixture(0, 0, 0);
            Reject(acquisition, noProficiency,
                "The acquisition evaluator accepted a fixture with no proficiency at all.",
                "fullProficiencyRank=0");

            JObject notAnObject = Acquisition();
            notAnObject["fixture"] = 1;
            Reject(acquisition, notAnObject,
                "The acquisition evaluator accepted a fixture that is not an object.",
                ":fixture-not-an-object");

            // A scoped case must reject a broader fixture, and the wrong scope.
            EvidenceCase scoped = Find("scoped-child-refusal");
            JObject broader = ScopedChildRefusal();
            broader["fixture"] = Fixture(1, 0, 0);
            Reject(scoped, broader,
                "The scoped-refusal evaluator accepted full proficiency where the case " +
                "declared one-handed only.", "fullProficiencyRank=1");

            JObject wrongScope = ScopedChildRefusal();
            wrongScope["fixture"] = Fixture(0, 0, 1);
            Reject(scoped, wrongScope,
                "The scoped-refusal evaluator accepted the wrong scoped proficiency.",
                "oneHandedProficiencyRank=0");

            EvidenceCase emptySelection = Find("empty-selection");
            JObject unproven = EmptySelection();
            unproven["fixture"] = Fixture(0, 0, 0);
            Reject(emptySelection, unproven,
                "The empty-selection evaluator accepted an unqualified fixture.",
                "fullProficiencyRank=0");

            EvidenceCase refused = Find("refused-parent");
            JObject hidden = RefusedParent();
            hidden["fixture"] = Fixture(1, 0, 0);
            Reject(refused, hidden,
                "The refusal evaluator accepted a fixture that secretly had proficiency.",
                "fullProficiencyRank=1");

            // And the correct fixture and behaviour are accepted.
            foreach (EvidenceCase evidence in new[] { acquisition, scoped,
                emptySelection, refused })
            {
                var failures = new List<string>();
                Assertions.True(evidence.Evaluate(evidence.Build(), failures),
                    evidence.Name + " rejected its own correct fixture: " +
                    string.Join("|", failures.ToArray()));
            }
        }

        // R4: a claimed successful confirmation must observe the native
        // completion gate as true immediately before the level is applied.
        internal static void ConfirmationRequiresNativeCompleteness()
        {
            var scored = new List<Tuple<EvidenceCase, string, string>> {
                Tuple.Create(Find("pending-class-change"),
                    "completeBeforeConfirmation", "confirmationApplied"),
                Tuple.Create(Find("musket-master"),
                    "completeBeforeConfirmation", "confirmationApplied"),
                Tuple.Create(Find("acquisition"),
                    "completeBeforeConfirmation", "confirmationApplied"),
                Tuple.Create(Find("scoped-child-refusal"),
                    "completeBeforeConfirmation", "confirmationApplied"),
                Tuple.Create(Find("refused-parent"),
                    "completeBeforeConfirmation", "confirmationApplied"),
                Tuple.Create(Find("class-identity-control"),
                    "buildCompleteBeforeConfirmation", "buildConfirmationApplied") };
            foreach (var entry in scored)
            {
                EvidenceCase evidence = entry.Item1;

                JObject incomplete = evidence.Build();
                incomplete[entry.Item2] = false;
                Reject(evidence, incomplete,
                    evidence.Name + " accepted a confirmation the native completion " +
                    "gate refused.", ":native-completion-refused-the-build");

                JObject missing = evidence.Build();
                missing.Remove(entry.Item2);
                Reject(evidence, missing,
                    evidence.Name + " accepted a confirmation with no completeness " +
                    "observation.", ":missing-observation:" + entry.Item2);

                JObject nulled = evidence.Build();
                nulled[entry.Item2] = JValue.CreateNull();
                Reject(evidence, nulled,
                    evidence.Name + " accepted a null completeness observation.",
                    ":missing-observation:" + entry.Item2);

                JObject notApplied = evidence.Build();
                notApplied[entry.Item3] = false;
                Reject(evidence, notApplied,
                    evidence.Name + " accepted a level that was never applied.",
                    ":level-was-not-applied");
            }

            // The defensive probe must declare itself as one; it can never
            // stand in for a claimed confirmation.
            EvidenceCase probe = Find("empty-selection");
            JObject undeclared = EmptySelection();
            undeclared["appliedWithoutNativeCompletion"] = false;
            Reject(probe, undeclared,
                "The defensive lower-level application was accepted without declaring " +
                "itself.", ":defensive-probe-not-declared");
        }

        // R1/R4: the pending class change must stay a single live transaction
        // that ends at the native completion gate.
        internal static void CancelledVisitsCannotStandInForAPendingClassChange()
        {
            EvidenceCase evidence = Find("pending-class-change");

            JObject twoControllers = PendingClassChange();
            twoControllers["controllerInstances"] = 2;
            Reject(evidence, twoControllers,
                "Separate cancelled visits were accepted as a pending class change.",
                ":not-a-single-transaction");

            JObject neverHeld = PendingClassChange();
            neverHeld["heldBeforeChange"] = false;
            Reject(evidence, neverHeld,
                "A class change with no held choice was accepted.",
                ":choice-was-never-held");

            JObject survived = PendingClassChange();
            survived["holdAfterChange"] = Hold(true, true, 0);
            Reject(evidence, survived,
                "A choice that survived the class change was accepted.", null);

            JObject grantedAnyway = PendingClassChange();
            grantedAnyway["holdAfterChange"] = Hold(false, false, 1);
            Reject(evidence, grantedAnyway,
                "A dropped choice that still granted its fact was accepted.",
                "trackedChildRank=1");

            JObject noRefresh = PendingClassChange();
            noRefresh["childEligibilityAfterRestore"] = Bools(true, false, true);
            Reject(evidence, noRefresh,
                "Partial child eligibility after the restore was accepted.",
                ":child-not-eligible-after-restore");

            JObject notReleased = PendingClassChange();
            notReleased["reservationReleasedBeforeConfirmation"] = false;
            Reject(evidence, notReleased,
                "A confirmation that never released the test reservation was accepted.",
                ":reservation-not-released");

            JObject stale = PendingClassChange();
            stale["acquiredAnyChildAfterConfirmation"] = true;
            Reject(evidence, stale,
                "A stale choice that survived confirmation was accepted.",
                ":stale-choice-survived-confirmation");
        }

        internal static void RepairedOrUngatedCompletionCannotScorePass()
        {
            EvidenceCase scoped = Find("scoped-child-refusal");

            JObject completed = ScopedChildRefusal();
            completed["completeWhileEmpty"] = true;
            Reject(scoped, completed,
                "A build that could complete with an empty Rapid Reload choice was " +
                "accepted.", ":complete-while-empty");

            JObject unattributed = ScopedChildRefusal();
            unattributed["targetBlocksCompletion"] = false;
            Reject(scoped, unattributed,
                "A completion block that is not attributable to Rapid Reload was accepted.",
                ":completion-block-not-attributable");

            JObject stillBlocked = ScopedChildRefusal();
            stillBlocked["targetBlocksCompletionAfterLegalChoice"] = true;
            Reject(scoped, stillBlocked,
                "A target that still blocked after the legal choice was accepted.",
                ":target-still-blocks-after-legal-choice");

            JObject banked = ScopedChildRefusal();
            banked["parentRankWhileEmpty"] = 1;
            Reject(scoped, banked,
                "An empty Rapid Reload choice that granted a fact was accepted.",
                ":empty-selection-granted-a-fact");

            JObject accepted = ScopedChildRefusal();
            accepted["refusedChildSelected"] = true;
            Reject(scoped, accepted,
                "An out-of-scope firearm that was selected anyway was accepted.",
                ":out-of-scope-child-accepted");

            EvidenceCase probe = Find("empty-selection");
            JObject emptyBanked = EmptySelection();
            emptyBanked["acquiredParent"] = true;
            Reject(probe, emptyBanked,
                "A banked empty Rapid Reload selection was accepted.",
                ":empty-choice-was-banked");

            JObject emptyCompleted = EmptySelection();
            emptyCompleted["completeWhileEmpty"] = true;
            Reject(probe, emptyCompleted,
                "Native completion of an empty Rapid Reload choice was accepted.",
                ":native-completion-allowed-an-empty-choice");
        }

        internal static void ManuallyGrantedMusketMasterCannotScorePass()
        {
            EvidenceCase evidence = Find("musket-master");

            JObject handGranted = MusketMaster();
            handGranted["fixture"] = Fixture(1, 0, 0);
            Reject(evidence, handGranted,
                "A Musket Master fixture with full firearm proficiency was accepted.",
                "fullProficiencyRank=1");

            JObject noArchetype = MusketMaster();
            noArchetype["isMusketMasterArchetype"] = false;
            Reject(evidence, noArchetype,
                "A fixture without the Musket Master archetype identity was accepted.",
                ":archetype-identity-absent");

            JObject noGrant = MusketMaster();
            noGrant["automaticMusketRank"] = 0;
            Reject(evidence, noGrant,
                "A Musket Master without its automatic Rapid Reload grant was accepted.",
                ":automatic-musket-grant-missing");

            JObject duplicate = MusketMaster();
            duplicate["ownedChildCanSelect"] = true;
            Reject(evidence, duplicate,
                "A re-selectable owned firearm choice was accepted.",
                ":owned-child-consumed-another-feat");

            JObject extraSlot = MusketMaster();
            extraSlot["featSlotsConsumed"] = 2;
            Reject(evidence, extraSlot, "A second consumed feat slot was accepted.",
                ":feat-slot-accounting");

            JObject outOfScope = MusketMaster();
            outOfScope["incompatibleChildCanSelect"] = true;
            Reject(evidence, outOfScope,
                "A one-handed firearm choice was accepted for a Musket Master.",
                ":out-of-scope-child-selectable");
        }

        internal static void ClassIdentityControlDemandsRealClassLevels()
        {
            EvidenceCase evidence = Find("class-identity-control");

            JObject noLevels = ClassIdentityControl();
            noLevels["committedGunslingerLevel"] = 0;
            Reject(evidence, noLevels,
                "A negative control with no Gunslinger class level was accepted.",
                ":no-committed-gunslinger-level");

            JObject lostIdentity = ClassIdentityControl();
            lostIdentity["previewGunslingerLevel"] = 0;
            Reject(evidence, lostIdentity,
                "A preview that lost Gunslinger identity was accepted.",
                ":preview-lost-gunslinger-identity");

            // If native restoration puts proficiency back, the fixture is
            // invalid rather than a negative result.
            JObject restored = ClassIdentityControl();
            restored["previewFixture"] = Fixture(1, 0, 0);
            Reject(evidence, restored,
                "A control whose preview regained proficiency was accepted.",
                "fullProficiencyRank=1");

            JObject qualified = ClassIdentityControl();
            qualified["parentEligible"] = true;
            Reject(evidence, qualified,
                "Class identity alone was accepted as qualifying.",
                ":class-identity-alone-qualified");

            JObject selectable = ClassIdentityControl();
            selectable["parentCanSelect"] = true;
            Reject(evidence, selectable,
                "A selectable parent on a proficiency-free Gunslinger was accepted.",
                ":parent-selectable-on-class-identity");
        }

        // R2: the accepted native outcomes for a pending archetype change.
        internal static void ArchetypeScopeChangeTracksTheInvalidChildNotTheParent()
        {
            EvidenceCase evidence = Find("pending-archetype-change");

            // A retained valid parent with the invalid child cleared is correct
            // and is the baseline fixture; the engine removing both is equally
            // correct.
            JObject parentRemoved = ArchetypeScopeChange();
            parentRemoved["holdAfterChange"] = Hold(false, false, 0);
            parentRemoved["childSelectabilitySource"] = "probe";
            var removedFailures = new List<string>();
            Assertions.True(evidence.Evaluate(parentRemoved, removedFailures),
                "The engine removing the whole selection was rejected: " +
                string.Join("|", removedFailures.ToArray()));

            JObject pistolHeld = ArchetypeScopeChange();
            pistolHeld["holdAfterChange"] = Hold(true, true, 0);
            Reject(evidence, pistolHeld,
                "A retained invalid Pistol child was accepted.",
                ":invalid-child-survived-the-archetype-change");

            JObject pistolGranted = ArchetypeScopeChange();
            pistolGranted["invalidChildRankAfterChange"] = 1;
            Reject(evidence, pistolGranted,
                "A granted invalid Pistol child was accepted.",
                ":invalid-child-was-granted");

            JObject pistolSelectable = ArchetypeScopeChange();
            pistolSelectable["childSelectableAfterChange"] = Bools(true, false, true);
            Reject(evidence, pistolSelectable,
                "A selectable Pistol choice under Musket Master was accepted.",
                ":pistol-selectable-under-archetype");

            JObject pistolEligible = ArchetypeScopeChange();
            pistolEligible["childEligibilityAfterChange"] = Bools(true, false, true);
            Reject(evidence, pistolEligible,
                "An eligible Pistol choice under Musket Master was accepted.",
                ":pistol-eligible-under-archetype");

            JObject parentDisqualified = ArchetypeScopeChange();
            parentDisqualified["parentEligibleAfterChange"] = false;
            Reject(evidence, parentDisqualified,
                "A Musket Master that no longer qualifies for the parent was accepted.",
                ":parent-not-eligible-under-archetype");

            JObject ownedSelectable = ArchetypeScopeChange();
            ownedSelectable["childSelectableAfterChange"] = Bools(false, true, true);
            Reject(evidence, ownedSelectable,
                "The automatically granted firearm stayed selectable and was accepted.",
                ":owned-musket-selectable-under-archetype");

            JObject unknownSource = ArchetypeScopeChange();
            unknownSource["childSelectabilitySource"] = "<reserved-slot-unavailable>";
            Reject(evidence, unknownSource,
                "Child selectability from an unavailable slot was accepted.",
                ":child-selectability-source=");

            JObject twoControllers = ArchetypeScopeChange();
            twoControllers["controllerInstances"] = 2;
            Reject(evidence, twoControllers,
                "Separate visits were accepted as a pending archetype change.",
                ":not-a-single-transaction");

            JObject wideScope = ArchetypeScopeChange();
            wideScope["archetypeFixture"] = Fixture(1, 0, 0);
            Reject(evidence, wideScope,
                "An archetype fixture that kept full proficiency was accepted.",
                "fullProficiencyRank=1");

            JObject grantSurvived = ArchetypeScopeChange();
            grantSurvived["automaticMusketRankAfterRemoval"] = 1;
            Reject(evidence, grantSurvived,
                "An archetype grant that survived archetype removal was accepted.",
                ":archetype-grant-survived-removal");
        }

        // R5: a failed visit initialisation must keep its original error, and a
        // cleanup failure on top of it must reach the run as well. This calls
        // the same composition the production failure path calls.
        internal static void FailedVisitInitializationNeverDiscardsAnError()
        {
            var setupError = new InvalidOperationException(
                "Native archetype selection rejected the Rapid Reload gate visit.");

            // Cleanup succeeded: the caller rethrows the original, so its stack
            // and diagnostics are untouched.
            Assertions.True(
                RapidReloadVisitCleanupRules.Compose(setupError, null) == null,
                "A clean cancellation did not leave the original setup failure to be rethrown.");

            // Cleanup also failed: both failures must survive, and the result is
            // still an exception, so it can never read as success.
            var cleanupError = new InvalidOperationException(
                "Native controller cancellation threw.");
            Exception combined = RapidReloadVisitCleanupRules.Compose(setupError,
                cleanupError);
            Assertions.True(combined != null,
                "A cleanup failure was swallowed instead of being reported.");
            var aggregate = combined as AggregateException;
            Assertions.True(aggregate != null,
                "The combined failure is not an AggregateException: " +
                combined.GetType().FullName);
            Assertions.Equal(2, aggregate.InnerExceptions.Count,
                "The combined failure did not carry exactly the setup and cleanup errors.");
            Assertions.True(ReferenceEquals(aggregate.InnerExceptions[0], setupError),
                "The original setup failure is not the first inner exception.");
            Assertions.True(ReferenceEquals(aggregate.InnerExceptions[1], cleanupError),
                "The cleanup failure is not the second inner exception.");
            Assertions.True(aggregate.Message.Contains(
                    RapidReloadVisitCleanupRules.CombinedFailureMessage),
                "The combined failure does not explain what happened.");
            string rendered = aggregate.ToString();
            Assertions.True(rendered.Contains(setupError.Message) &&
                    rendered.Contains(cleanupError.Message),
                "The recorded exception summary would not show both failures.");

            // A cleanup failure can never be composed without the original.
            Assertions.Throws<ArgumentNullException>(
                () => RapidReloadVisitCleanupRules.Compose(null, cleanupError),
                "A missing setup failure was accepted.");
        }

        // R6: a controller that initialised successfully but whose cancellation
        // later threw must not report PASS. Every case evaluator is driven, so
        // the guard cannot be lost from one of them.
        internal static void SuccessPathCleanupFailureCannotScorePass()
        {
            foreach (EvidenceCase evidence in Cases)
            {
                var cleanFailures = new List<string>();
                Assertions.True(evidence.Evaluate(evidence.Build(), cleanFailures),
                    evidence.Name + " rejected evidence with no cleanup error: " +
                    string.Join("|", cleanFailures.ToArray()));

                JObject failedCleanup = evidence.Build();
                failedCleanup["cleanupError"] = "Native controller cancellation threw.";
                Reject(evidence, failedCleanup,
                    evidence.Name + " reported success despite a recorded cleanup " +
                    "failure.", RapidReloadVisitCleanupRules.CleanupFailurePrefix);
            }
        }

        // R6: the reporting half of the caller-side cleanup boundary, which is
        // the path CloseRapidReloadVisit uses. Driving the native Cancel() half
        // needs Kingmaker and is covered by the scenario's own injection check.
        internal static void CleanupReportingBoundaryScoresAndKeepsDiagnostics()
        {
            // A clean cancellation writes nothing and reports clean.
            var row = new JObject();
            var failures = new List<string>();
            Assertions.True(
                RapidReloadVisitCleanupRules.Report(null, row, failures, "case"),
                "A clean cancellation was not reported as clean.");
            Assertions.Equal(0, failures.Count,
                "A clean cancellation invented a failure.");
            Assertions.True(row["cleanupError"] == null,
                "A clean cancellation wrote a cleanup error.");

            // A failed cancellation reaches the scored collection and keeps its
            // diagnostics on the row.
            var cleanupError = new InvalidOperationException(
                "Native controller cancellation threw.");
            Assertions.False(
                RapidReloadVisitCleanupRules.Report(cleanupError, row, failures,
                    "visit-ownership.successful-initialization"),
                "A failed cancellation was reported as clean.");
            Assertions.Equal(1, failures.Count,
                "A failed cancellation did not reach the scored failure collection.");
            Assertions.True(failures[0].StartsWith(
                    "visit-ownership.successful-initialization" +
                    RapidReloadVisitCleanupRules.CleanupFailurePrefix,
                    StringComparison.Ordinal),
                "The scored cleanup failure did not name its boundary: " + failures[0]);
            Assertions.True(failures[0].Contains(cleanupError.Message),
                "The scored cleanup failure lost its message.");
            Assertions.Equal(cleanupError.Message, (string)row["cleanupError"],
                "The cleanup error message was not recorded on the row.");
            Assertions.True(row["cleanupErrorDetail"] != null &&
                    ((string)row["cleanupErrorDetail"]).Contains(cleanupError.Message),
                "The cleanup error detail was not preserved on the row.");

            // The boundary never throws, so a caller's finally still reaches its
            // fixture disposal, and an unlabelled or unscored call is tolerated
            // rather than crashing the run.
            Assertions.False(
                RapidReloadVisitCleanupRules.Report(cleanupError, null, null, null),
                "An unscored cleanup failure was reported as clean.");
        }

        internal static void ScenarioUsesTheNativeOperationsItClaims()
        {
            string root = Environment.CurrentDirectory;
            string scenario = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.RapidReloadProficiencyGate.cs"));
            foreach (string token in new[] {
                // R1: an explicit, rebuild-tolerant slot reservation.
                "ReserveRapidReloadSlot(",
                "ResolveReservedRapidReloadSlot(",
                "!IsReservedRapidReloadSlot(value, reservation)",
                "ReleaseRapidReloadReservation(reservation)",
                "RapidReloadReservationHolds(reserved)",
                // R2: parent and exact child tracked separately.
                "DescribeRapidReloadHold(ctx, controller, ctx.Children[0])",
                "retained-nested-state",
                "WithdrawRapidReloadProbe(controller, reservation)",
                // R4: the native completion boundary before any confirmation.
                "ConfirmRapidReloadLevel(ctx, controller, descriptor, row,",
                "levelUpState.IsComplete()",
                "CanSelectAnything(levelUpState, preview)",
                "appliedWithoutNativeCompletion",
                // Single-transaction native pending-build changes.
                "controller.SelectClass(ctx.Fighter, true)",
                "controller.SelectClass(ctx.Gunslinger, true)",
                "controller.AddArchetype(ctx.MusketMaster)",
                "controller.RemoveArchetype(ctx.MusketMaster)",
                "ctx.MusketMaster);",
                ".EvaluatePendingClassChange(",
                ".EvaluateArchetypeScopeChange(",
                ".EvaluateMusketMaster(",
                ".EvaluateClassIdentityControl(",
                // R5: exception-safe ownership of a newly created controller.
                "out LevelUpController created",
                "catch (Exception setupError)",
                "TryCancelRapidReloadVisit(controller)",
                "RapidReloadVisitCleanupRules.Compose(setupError,",
                "RunRapidReloadVisitOwnershipCheck(",
                // R6: caller-owned cleanup reports into the scored collection,
                // and the boundary itself is fault-injected in the scenario.
                "RapidReloadVisitCleanupRules.Report(",
                "row[\"successCleanupClean\"] = CloseRapidReloadVisit(successController,",
                "RunRapidReloadCleanupReportingInjection(",
                "brokenController.Preview = null;"
            })
                Assertions.True(scenario.Contains(token),
                    "The Rapid Reload gate scenario lost a required native step: " + token);
            // The corrected scenario must not reach completion observations by
            // repairing the invalid state first: the single UnselectFeature call
            // lives in the probe withdrawal helper.
            int unselectUses = scenario.Split(new[] { "controller.UnselectFeature(" },
                StringSplitOptions.None).Length - 1;
            Assertions.Equal(1, unselectUses,
                "UnselectFeature is only allowed once, to withdraw a probe-created " +
                "selection after its observation.");
            Assertions.False(scenario.Contains("CloseRapidReloadVisit(controller, row);"),
                "A caller still uses the log-only cleanup form that could report PASS " +
                "despite a failed cancellation.");
            string catalog = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting", "RuntimeTestScenarioCatalog.cs"));
            Assertions.True(catalog.Contains("disposable-rapid-reload-proficiency-gate"),
                "The scenario is registered in the production allowlist catalog.");
            string automation = File.ReadAllText(Path.Combine(root, "scripts",
                "RuntimeAutomation.Common.ps1"));
            Assertions.True(
                automation.Contains("'disposable-rapid-reload-proficiency-gate'"),
                "The PowerShell metadata table knows the scenario.");
        }
    }
}
