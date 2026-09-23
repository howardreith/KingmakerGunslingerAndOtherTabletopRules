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
    /// operation, lost a precondition, repaired an invalid state, or
    /// substituted separate cancelled visits for a real pending-build change
    /// cannot be scored as a pass.
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

        private static JArray Bools(params bool[] values)
        {
            return new JArray(values.Select(value => (object)value));
        }

        internal static JObject RefusedParent()
        {
            return new JObject {
                ["case"] = "A.ordinary-feat-no-proficiency",
                ["slotPresent"] = true,
                ["fixture"] = Fixture(0, 0, 0),
                ["parentOffered"] = true,
                ["parentCanSelect"] = false,
                ["parentSelected"] = false,
                ["parentChoiceHeld"] = false,
                ["childEligibility"] = Bools(false, false, false),
                ["acquiredParent"] = false,
                ["acquiredChild"] = false };
        }

        internal static JObject ScopedChildRefusal()
        {
            return new JObject {
                ["case"] = "B.one-handed-refuses-musket",
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
                ["completeAfterLegalChoice"] = true,
                ["acquiredLegalChild"] = true,
                ["acquiredRefusedChild"] = false };
        }

        internal static JObject EmptySelection()
        {
            return new JObject {
                ["case"] = "B.empty-selection-blocked",
                ["slotPresent"] = true,
                ["fixture"] = Fixture(1, 0, 0),
                ["parentSelected"] = true,
                ["childStatePresent"] = true,
                ["childStateSelected"] = false,
                ["completeWhileEmpty"] = false,
                ["targetBlocksCompletion"] = true,
                ["acquiredParent"] = false,
                ["acquiredAnyChild"] = false };
        }

        internal static JObject Acquisition()
        {
            return new JObject {
                ["case"] = "C.ordinary-feat-full-proficiency",
                ["slotPresent"] = true,
                ["fixture"] = Fixture(1, 0, 0),
                ["parentCanSelect"] = true,
                ["parentSelected"] = true,
                ["childCanSelect"] = true,
                ["childSelected"] = true,
                ["completeAfterLegalChoice"] = true,
                ["acquiredChild"] = true };
        }

        internal static JObject PendingClassChange()
        {
            return new JObject {
                ["controllerInstances"] = 1,
                ["gunslingerFixture"] = Fixture(1, 0, 0),
                ["heldBeforeChange"] = true,
                ["heldChildName"] = "KMG_RapidReload_Pistol",
                ["fighterFixture"] = Fixture(0, 0, 0),
                ["fighterGunslingerLevel"] = 0,
                ["parentEligibleAfterChange"] = false,
                ["childEligibilityAfterChange"] = Bools(false, false, false),
                ["choiceSurvivedChange"] = false,
                ["restoredFixture"] = Fixture(1, 0, 0),
                ["parentEligibleAfterRestore"] = true,
                ["childEligibilityAfterRestore"] = Bools(true, true, true),
                ["choiceRestoredByEngine"] = false,
                ["reheldBeforeSecondChange"] = true,
                ["choiceSurvivedSecondChange"] = false,
                ["confirmedFighterLevel"] = 1,
                ["confirmedGunslingerLevel"] = 0,
                ["acquiredParentAfterConfirmation"] = false,
                ["acquiredAnyChildAfterConfirmation"] = false };
        }

        internal static JObject ArchetypeScopeChange()
        {
            return new JObject {
                ["controllerInstances"] = 1,
                ["baseFixture"] = Fixture(1, 0, 0),
                ["heldBeforeChange"] = true,
                ["archetypeApplied"] = true,
                ["archetypeFixture"] = Fixture(0, 0, 1),
                ["automaticMusketRankAfterChange"] = 1,
                ["parentEligibleAfterChange"] = true,
                ["childEligibilityAfterChange"] = Bools(false, false, true),
                ["childSelectableAfterChange"] = Bools(false, false, true),
                ["choiceSurvivedChange"] = false,
                ["archetypeRemoved"] = true,
                ["restoredFixture"] = Fixture(1, 0, 0),
                ["automaticMusketRankAfterRemoval"] = 0,
                ["childEligibilityAfterRemoval"] = Bools(true, true, true) };
        }

        internal static JObject MusketMaster()
        {
            return new JObject {
                ["levelOneGrantsRapidReloadMusket"] = true,
                ["fixture"] = Fixture(0, 0, 1),
                ["gunslingerLevel"] = 1,
                ["isMusketMasterArchetype"] = true,
                ["automaticMusketRank"] = 1,
                ["slotPresent"] = true,
                ["parentCanSelect"] = true,
                ["parentSelected"] = true,
                ["childStatePresent"] = true,
                ["ownedChildOffered"] = true,
                ["ownedChildCanSelect"] = false,
                ["ownedChildSelected"] = false,
                ["incompatibleChildCanSelect"] = false,
                ["legalChildCanSelect"] = true,
                ["legalChildSelected"] = true,
                ["acquiredLegalChild"] = true,
                ["acquiredOwnedChildRank"] = 1,
                ["acquiredIncompatibleChildRank"] = 0,
                ["featSlotsConsumed"] = 1 };
        }

        internal static JObject ClassIdentityControl()
        {
            return new JObject {
                ["committedGunslingerLevel"] = 1,
                ["committedFixture"] = Fixture(0, 0, 0),
                ["previewGunslingerLevel"] = 1,
                ["previewFixture"] = Fixture(0, 0, 0),
                ["parentEligible"] = false,
                ["childEligibility"] = Bools(false, false, false),
                ["slotPresent"] = true,
                ["parentOffered"] = true,
                ["parentCanSelect"] = false,
                ["parentSelected"] = false };
        }

        private static readonly Tuple<string, Func<JObject>, string[],
            Func<JObject, IList<string>, bool>>[] Evaluators = {
            Tuple.Create<string, Func<JObject>, string[], Func<JObject, IList<string>, bool>>(
                "refused-parent", RefusedParent,
                RapidReloadGateEvidenceRules.RefusedParentKeys,
                RapidReloadGateEvidenceRules.EvaluateRefusedParent),
            Tuple.Create<string, Func<JObject>, string[], Func<JObject, IList<string>, bool>>(
                "scoped-child-refusal", ScopedChildRefusal,
                RapidReloadGateEvidenceRules.ScopedChildRefusalKeys,
                RapidReloadGateEvidenceRules.EvaluateScopedChildRefusal),
            Tuple.Create<string, Func<JObject>, string[], Func<JObject, IList<string>, bool>>(
                "empty-selection", EmptySelection,
                RapidReloadGateEvidenceRules.EmptySelectionKeys,
                RapidReloadGateEvidenceRules.EvaluateEmptySelection),
            Tuple.Create<string, Func<JObject>, string[], Func<JObject, IList<string>, bool>>(
                "acquisition", Acquisition,
                RapidReloadGateEvidenceRules.AcquisitionKeys,
                RapidReloadGateEvidenceRules.EvaluateAcquisition),
            Tuple.Create<string, Func<JObject>, string[], Func<JObject, IList<string>, bool>>(
                "pending-class-change", PendingClassChange,
                RapidReloadGateEvidenceRules.PendingClassChangeKeys,
                RapidReloadGateEvidenceRules.EvaluatePendingClassChange),
            Tuple.Create<string, Func<JObject>, string[], Func<JObject, IList<string>, bool>>(
                "pending-archetype-change", ArchetypeScopeChange,
                RapidReloadGateEvidenceRules.ArchetypeScopeChangeKeys,
                RapidReloadGateEvidenceRules.EvaluateArchetypeScopeChange),
            Tuple.Create<string, Func<JObject>, string[], Func<JObject, IList<string>, bool>>(
                "musket-master", MusketMaster,
                RapidReloadGateEvidenceRules.MusketMasterKeys,
                RapidReloadGateEvidenceRules.EvaluateMusketMaster),
            Tuple.Create<string, Func<JObject>, string[], Func<JObject, IList<string>, bool>>(
                "class-identity-control", ClassIdentityControl,
                RapidReloadGateEvidenceRules.ClassIdentityControlKeys,
                RapidReloadGateEvidenceRules.EvaluateClassIdentityControl) };

        internal static void CompleteEvidenceIsAccepted()
        {
            foreach (var evaluator in Evaluators)
            {
                var failures = new List<string>();
                Assertions.True(evaluator.Item4(evaluator.Item2(), failures),
                    "Complete " + evaluator.Item1 + " evidence was rejected: " +
                    string.Join("|", failures.ToArray()));
                Assertions.Equal(0, failures.Count,
                    "Complete " + evaluator.Item1 + " evidence reported failures: " +
                    string.Join("|", failures.ToArray()));
            }
        }

        internal static void MissingObservationsCannotScorePass()
        {
            foreach (var evaluator in Evaluators)
            {
                Assertions.True(evaluator.Item3.Length > 0,
                    evaluator.Item1 + " declares no required observations.");
                foreach (string key in evaluator.Item3)
                {
                    JObject row = evaluator.Item2();
                    row.Remove(key);
                    var failures = new List<string>();
                    Assertions.False(evaluator.Item4(row, failures),
                        evaluator.Item1 + " accepted evidence with a missing " +
                        key + " observation.");
                    Assertions.True(failures.Any(value =>
                            value.EndsWith(":missing-observation:" + key,
                                StringComparison.Ordinal)),
                        evaluator.Item1 + " did not name the missing observation " + key +
                        ": " + string.Join("|", failures.ToArray()));
                }
            }
        }

        internal static void NullObservationsCannotScorePass()
        {
            foreach (var evaluator in Evaluators)
            {
                JObject row = evaluator.Item2();
                row[evaluator.Item3[0]] = JValue.CreateNull();
                var failures = new List<string>();
                Assertions.False(evaluator.Item4(row, failures),
                    evaluator.Item1 + " accepted a null observation.");
                var missingRow = new List<string>();
                Assertions.False(evaluator.Item4(null, missingRow),
                    evaluator.Item1 + " accepted an absent observation row.");
                Assertions.True(missingRow.Any(value =>
                        value.EndsWith(":observation-row-missing", StringComparison.Ordinal)),
                    evaluator.Item1 + " did not name an absent observation row.");
            }
        }

        internal static void IntendedGrantsAreNeverEvidence()
        {
            // A row that claims full proficiency while the fixture's observed
            // ranks are all zero must be rejected: only the measured facts
            // count.
            JObject acquisition = Acquisition();
            acquisition["fixture"] = Fixture(0, 0, 0);
            var failures = new List<string>();
            Assertions.False(RapidReloadGateEvidenceRules.EvaluateFixtureProficiency(
                    (JObject)acquisition["fixture"], "acquisition", true, false, false,
                    failures),
                "An unproven proficiency fixture was accepted.");
            Assertions.True(failures.Any(value => value.Contains("fullProficiencyRank=0")),
                "The unproven fixture failure did not name the observed rank.");

            // A refusal fixture that secretly carries proficiency is not a
            // negative control.
            JObject refused = RefusedParent();
            refused["fixture"] = Fixture(1, 0, 0);
            var refusedFailures = new List<string>();
            Assertions.False(RapidReloadGateEvidenceRules.EvaluateRefusedParent(
                    refused, refusedFailures),
                "A refusal case with hidden proficiency was accepted.");
        }

        internal static void CancelledVisitsCannotStandInForAPendingClassChange()
        {
            // The finding this guards: two controllers means the run cancelled a
            // visit and started an unrelated one instead of changing the class
            // inside a live pending build.
            JObject row = PendingClassChange();
            row["controllerInstances"] = 2;
            var failures = new List<string>();
            Assertions.False(RapidReloadGateEvidenceRules.EvaluatePendingClassChange(
                    row, failures),
                "Separate cancelled visits were accepted as a pending class change.");
            Assertions.True(failures.Any(value =>
                    value.EndsWith(":not-a-single-transaction", StringComparison.Ordinal)),
                "The multi-controller failure was not named.");

            JObject neverHeld = PendingClassChange();
            neverHeld["heldBeforeChange"] = false;
            var neverHeldFailures = new List<string>();
            Assertions.False(RapidReloadGateEvidenceRules.EvaluatePendingClassChange(
                    neverHeld, neverHeldFailures),
                "A class change with no held choice was accepted.");

            JObject survived = PendingClassChange();
            survived["choiceSurvivedChange"] = true;
            var survivedFailures = new List<string>();
            Assertions.False(RapidReloadGateEvidenceRules.EvaluatePendingClassChange(
                    survived, survivedFailures),
                "A choice that survived the class change was accepted.");

            JObject noRefresh = PendingClassChange();
            noRefresh["childEligibilityAfterRestore"] = Bools(true, false, true);
            var noRefreshFailures = new List<string>();
            Assertions.False(RapidReloadGateEvidenceRules.EvaluatePendingClassChange(
                    noRefresh, noRefreshFailures),
                "Partial child eligibility after the restore was accepted.");

            JObject stale = PendingClassChange();
            stale["acquiredAnyChildAfterConfirmation"] = true;
            var staleFailures = new List<string>();
            Assertions.False(RapidReloadGateEvidenceRules.EvaluatePendingClassChange(
                    stale, staleFailures),
                "A stale choice that survived confirmation was accepted.");
        }

        internal static void RepairedOrUngatedCompletionCannotScorePass()
        {
            // The finding this guards: observing completion only after the test
            // itself unselected the invalid choice.
            JObject completed = ScopedChildRefusal();
            completed["completeWhileEmpty"] = true;
            var failures = new List<string>();
            Assertions.False(RapidReloadGateEvidenceRules.EvaluateScopedChildRefusal(
                    completed, failures),
                "A build that could complete with an empty Rapid Reload choice was accepted.");

            JObject unattributed = ScopedChildRefusal();
            unattributed["targetBlocksCompletion"] = false;
            var unattributedFailures = new List<string>();
            Assertions.False(RapidReloadGateEvidenceRules.EvaluateScopedChildRefusal(
                    unattributed, unattributedFailures),
                "A completion block that is not attributable to Rapid Reload was accepted.");
            Assertions.True(unattributedFailures.Any(value =>
                    value.EndsWith(":completion-block-not-attributable",
                        StringComparison.Ordinal)),
                "The unattributed completion block was not named.");

            JObject stillBlocked = ScopedChildRefusal();
            stillBlocked["completeAfterLegalChoice"] = false;
            var stillBlockedFailures = new List<string>();
            Assertions.False(RapidReloadGateEvidenceRules.EvaluateScopedChildRefusal(
                    stillBlocked, stillBlockedFailures),
                "An unfinished character that never completes was accepted as attribution.");

            JObject banked = ScopedChildRefusal();
            banked["parentRankWhileEmpty"] = 1;
            var bankedFailures = new List<string>();
            Assertions.False(RapidReloadGateEvidenceRules.EvaluateScopedChildRefusal(
                    banked, bankedFailures),
                "An empty Rapid Reload choice that granted a fact was accepted.");

            JObject accepted = ScopedChildRefusal();
            accepted["refusedChildSelected"] = true;
            var acceptedFailures = new List<string>();
            Assertions.False(RapidReloadGateEvidenceRules.EvaluateScopedChildRefusal(
                    accepted, acceptedFailures),
                "An out-of-scope firearm that was selected anyway was accepted.");

            JObject emptyBanked = EmptySelection();
            emptyBanked["acquiredParent"] = true;
            var emptyBankedFailures = new List<string>();
            Assertions.False(RapidReloadGateEvidenceRules.EvaluateEmptySelection(
                    emptyBanked, emptyBankedFailures),
                "A banked empty Rapid Reload selection was accepted.");
        }

        internal static void ManuallyGrantedMusketMasterCannotScorePass()
        {
            // The finding this guards: a base Gunslinger with hand-granted
            // proficiency and a hand-granted Rapid Reload (Musket).
            JObject handGranted = MusketMaster();
            handGranted["fixture"] = Fixture(1, 0, 0);
            var failures = new List<string>();
            Assertions.False(RapidReloadGateEvidenceRules.EvaluateMusketMaster(
                    handGranted, failures),
                "A Musket Master fixture with full firearm proficiency was accepted.");

            JObject noArchetype = MusketMaster();
            noArchetype["isMusketMasterArchetype"] = false;
            var noArchetypeFailures = new List<string>();
            Assertions.False(RapidReloadGateEvidenceRules.EvaluateMusketMaster(
                    noArchetype, noArchetypeFailures),
                "A fixture without the Musket Master archetype identity was accepted.");
            Assertions.True(noArchetypeFailures.Any(value =>
                    value.EndsWith(":archetype-identity-absent", StringComparison.Ordinal)),
                "The absent archetype identity was not named.");

            JObject noGrant = MusketMaster();
            noGrant["automaticMusketRank"] = 0;
            var noGrantFailures = new List<string>();
            Assertions.False(RapidReloadGateEvidenceRules.EvaluateMusketMaster(
                    noGrant, noGrantFailures),
                "A Musket Master without its automatic Rapid Reload grant was accepted.");

            JObject duplicate = MusketMaster();
            duplicate["ownedChildCanSelect"] = true;
            var duplicateFailures = new List<string>();
            Assertions.False(RapidReloadGateEvidenceRules.EvaluateMusketMaster(
                    duplicate, duplicateFailures),
                "A re-selectable owned firearm choice was accepted.");

            JObject extraSlot = MusketMaster();
            extraSlot["featSlotsConsumed"] = 2;
            var extraSlotFailures = new List<string>();
            Assertions.False(RapidReloadGateEvidenceRules.EvaluateMusketMaster(
                    extraSlot, extraSlotFailures),
                "A second consumed feat slot was accepted.");

            JObject outOfScope = MusketMaster();
            outOfScope["incompatibleChildCanSelect"] = true;
            var outOfScopeFailures = new List<string>();
            Assertions.False(RapidReloadGateEvidenceRules.EvaluateMusketMaster(
                    outOfScope, outOfScopeFailures),
                "A one-handed firearm choice was accepted for a Musket Master.");
        }

        internal static void ClassIdentityControlDemandsRealClassLevels()
        {
            // The finding this guards: granting the class proficiency feature
            // does not give a character Gunslinger class levels.
            JObject noLevels = ClassIdentityControl();
            noLevels["committedGunslingerLevel"] = 0;
            var failures = new List<string>();
            Assertions.False(RapidReloadGateEvidenceRules.EvaluateClassIdentityControl(
                    noLevels, failures),
                "A negative control with no Gunslinger class level was accepted.");
            Assertions.True(failures.Any(value =>
                    value.EndsWith(":no-committed-gunslinger-level",
                        StringComparison.Ordinal)),
                "The missing class level was not named.");

            JObject lostIdentity = ClassIdentityControl();
            lostIdentity["previewGunslingerLevel"] = 0;
            var lostIdentityFailures = new List<string>();
            Assertions.False(RapidReloadGateEvidenceRules.EvaluateClassIdentityControl(
                    lostIdentity, lostIdentityFailures),
                "A preview that lost Gunslinger identity was accepted.");

            // If native restoration puts proficiency back, the fixture is
            // invalid rather than a negative result.
            JObject restored = ClassIdentityControl();
            restored["previewFixture"] = Fixture(1, 0, 0);
            var restoredFailures = new List<string>();
            Assertions.False(RapidReloadGateEvidenceRules.EvaluateClassIdentityControl(
                    restored, restoredFailures),
                "A control whose preview regained proficiency was accepted.");

            JObject qualified = ClassIdentityControl();
            qualified["parentEligible"] = true;
            var qualifiedFailures = new List<string>();
            Assertions.False(RapidReloadGateEvidenceRules.EvaluateClassIdentityControl(
                    qualified, qualifiedFailures),
                "Class identity alone was accepted as qualifying.");
            Assertions.True(qualifiedFailures.Any(value =>
                    value.EndsWith(":class-identity-alone-qualified",
                        StringComparison.Ordinal)),
                "The class-identity regression was not named.");

            JObject selectable = ClassIdentityControl();
            selectable["parentCanSelect"] = true;
            var selectableFailures = new List<string>();
            Assertions.False(RapidReloadGateEvidenceRules.EvaluateClassIdentityControl(
                    selectable, selectableFailures),
                "A selectable parent on a proficiency-free Gunslinger was accepted.");
        }

        internal static void ArchetypeScopeChangeDemandsRealScopeNarrowing()
        {
            JObject twoControllers = ArchetypeScopeChange();
            twoControllers["controllerInstances"] = 2;
            var failures = new List<string>();
            Assertions.False(RapidReloadGateEvidenceRules.EvaluateArchetypeScopeChange(
                    twoControllers, failures),
                "Separate visits were accepted as a pending archetype change.");

            JObject wideScope = ArchetypeScopeChange();
            wideScope["archetypeFixture"] = Fixture(1, 0, 0);
            var wideScopeFailures = new List<string>();
            Assertions.False(RapidReloadGateEvidenceRules.EvaluateArchetypeScopeChange(
                    wideScope, wideScopeFailures),
                "An archetype fixture that kept full proficiency was accepted.");

            JObject pistolStillLegal = ArchetypeScopeChange();
            pistolStillLegal["childEligibilityAfterChange"] = Bools(true, false, true);
            var pistolFailures = new List<string>();
            Assertions.False(RapidReloadGateEvidenceRules.EvaluateArchetypeScopeChange(
                    pistolStillLegal, pistolFailures),
                "A one-handed firearm that stayed eligible under Musket Master was accepted.");

            JObject ownedSelectable = ArchetypeScopeChange();
            ownedSelectable["childSelectableAfterChange"] = Bools(false, true, true);
            var ownedFailures = new List<string>();
            Assertions.False(RapidReloadGateEvidenceRules.EvaluateArchetypeScopeChange(
                    ownedSelectable, ownedFailures),
                "The automatically granted firearm stayed selectable and was accepted.");

            JObject survived = ArchetypeScopeChange();
            survived["choiceSurvivedChange"] = true;
            var survivedFailures = new List<string>();
            Assertions.False(RapidReloadGateEvidenceRules.EvaluateArchetypeScopeChange(
                    survived, survivedFailures),
                "An out-of-scope choice that survived the archetype change was accepted.");

            JObject grantSurvived = ArchetypeScopeChange();
            grantSurvived["automaticMusketRankAfterRemoval"] = 1;
            var grantFailures = new List<string>();
            Assertions.False(RapidReloadGateEvidenceRules.EvaluateArchetypeScopeChange(
                    grantSurvived, grantFailures),
                "An archetype grant that survived archetype removal was accepted.");
        }

        internal static void ScenarioUsesTheNativeOperationsItClaims()
        {
            string root = Environment.CurrentDirectory;
            string scenario = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.RapidReloadProficiencyGate.cs"));
            foreach (string token in new[] {
                "controller.SelectClass(ctx.Fighter, true)",
                "controller.SelectClass(ctx.Gunslinger, true)",
                "controller.AddArchetype(ctx.MusketMaster)",
                "controller.RemoveArchetype(ctx.MusketMaster)",
                "OpenRapidReloadVisit(ctx, descriptor, ctx.Gunslinger,",
                "ctx.MusketMaster);",
                "levelUpState.IsComplete()",
                "CanSelectAnything(levelUpState, preview)",
                "FindRapidReloadChildState(controller, ctx)",
                ".EvaluatePendingClassChange(",
                ".EvaluateArchetypeScopeChange(",
                ".EvaluateMusketMaster(",
                ".EvaluateClassIdentityControl("
            })
                Assertions.True(scenario.Contains(token),
                    "The Rapid Reload gate scenario lost a required native step: " + token);
            // The corrected scenario must not reach completion observations by
            // repairing the invalid state first.
            int unselectUses = scenario.Split(new[] { "controller.UnselectFeature(" },
                StringSplitOptions.None).Length - 1;
            Assertions.Equal(1, unselectUses,
                "UnselectFeature is only allowed once, to withdraw the archetype " +
                "selectability probe after its observation.");
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
