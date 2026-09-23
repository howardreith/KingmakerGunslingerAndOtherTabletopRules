using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Scoring rules for the Rapid Reload proficiency-gate runtime scenario.
    /// The scenario records what it actually observed; these rules decide
    /// whether that evidence is complete and acceptable, and the domain suite
    /// exercises them with corrupted and truncated fixtures. Every evaluator
    /// demands its full observation set first, so a missing precondition, a
    /// skipped native operation or an absent feat slot can never be silently
    /// logged while the scenario still reports PASS.
    /// </summary>
    internal static class RapidReloadGateEvidenceRules
    {
        /// <summary>Proficiency facts a fixture must positively prove before
        /// its eligibility answers mean anything.</summary>
        internal static readonly string[] FixtureProficiencyKeys = {
            "fullProficiencyRank", "oneHandedProficiencyRank",
            "twoHandedProficiencyRank" };

        /// <summary>A character with no firearm proficiency attempting the
        /// parent through a real feat slot.</summary>
        internal static readonly string[] RefusedParentKeys = {
            "case", "slotPresent", "fixture", "parentOffered", "parentCanSelect",
            "parentSelected", "parentChoiceHeld", "childEligibility",
            "acquiredParent", "acquiredChild" };

        /// <summary>A proficient character attempting a firearm outside its
        /// proficiency scope, observed before any cleanup.</summary>
        internal static readonly string[] ScopedChildRefusalKeys = {
            "case", "slotPresent", "fixture", "parentCanSelect", "parentSelected",
            "childStatePresent", "refusedChildOffered", "refusedChildCanSelect",
            "refusedChildSelected", "childStateSelectedAfterRefusal",
            "completeWhileEmpty", "targetBlocksCompletion",
            "parentRankWhileEmpty", "refusedChildRankWhileEmpty",
            "legalChildSelected", "targetBlocksCompletionAfterLegalChoice",
            "completeAfterLegalChoice", "acquiredLegalChild",
            "acquiredRefusedChild" };

        /// <summary>A parent held with no child, confirmed without any
        /// test-driven repair.</summary>
        internal static readonly string[] EmptySelectionKeys = {
            "case", "slotPresent", "fixture", "parentSelected",
            "childStatePresent", "childStateSelected", "completeWhileEmpty",
            "targetBlocksCompletion", "acquiredParent", "acquiredAnyChild" };

        /// <summary>A qualifying character acquiring a legal choice.</summary>
        internal static readonly string[] AcquisitionKeys = {
            "case", "slotPresent", "fixture", "parentCanSelect", "parentSelected",
            "childCanSelect", "childSelected", "completeAfterLegalChoice",
            "acquiredChild" };

        /// <summary>A pending class change inside one live level-up
        /// transaction.</summary>
        internal static readonly string[] PendingClassChangeKeys = {
            "controllerInstances", "gunslingerFixture", "heldBeforeChange",
            "heldChildName", "fighterFixture", "fighterGunslingerLevel",
            "parentEligibleAfterChange", "childEligibilityAfterChange",
            "choiceSurvivedChange", "restoredFixture",
            "parentEligibleAfterRestore", "childEligibilityAfterRestore",
            "choiceRestoredByEngine", "reheldBeforeSecondChange",
            "choiceSurvivedSecondChange", "confirmedFighterLevel",
            "confirmedGunslingerLevel", "acquiredParentAfterConfirmation",
            "acquiredAnyChildAfterConfirmation" };

        /// <summary>A pending archetype change inside one live level-up
        /// transaction.</summary>
        internal static readonly string[] ArchetypeScopeChangeKeys = {
            "controllerInstances", "baseFixture", "heldBeforeChange",
            "archetypeApplied", "archetypeFixture", "automaticMusketRankAfterChange",
            "parentEligibleAfterChange", "childEligibilityAfterChange",
            "childSelectableAfterChange", "choiceSurvivedChange",
            "archetypeRemoved", "restoredFixture", "automaticMusketRankAfterRemoval",
            "childEligibilityAfterRemoval" };

        /// <summary>A natively built Musket Master.</summary>
        internal static readonly string[] MusketMasterKeys = {
            "levelOneGrantsRapidReloadMusket", "fixture", "gunslingerLevel",
            "isMusketMasterArchetype", "automaticMusketRank", "slotPresent",
            "parentCanSelect", "parentSelected", "childStatePresent",
            "ownedChildOffered", "ownedChildCanSelect", "ownedChildSelected",
            "incompatibleChildCanSelect", "legalChildCanSelect",
            "legalChildSelected", "acquiredLegalChild", "acquiredOwnedChildRank",
            "acquiredIncompatibleChildRank", "featSlotsConsumed" };

        /// <summary>Real Gunslinger levels with every firearm-proficiency fact
        /// removed.</summary>
        internal static readonly string[] ClassIdentityControlKeys = {
            "committedGunslingerLevel", "committedFixture", "previewGunslingerLevel",
            "previewFixture", "parentEligible", "childEligibility", "slotPresent",
            "parentOffered", "parentCanSelect", "parentSelected" };

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
        /// facts it claims. An intended grant is never evidence; only the
        /// observed ranks are.</summary>
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

        private static bool CheckRank(JObject fixture, string label, string key,
            bool expectPresent, IList<string> failures)
        {
            int rank = (int)fixture[key];
            if (expectPresent ? rank <= 0 : rank != 0)
            {
                failures.Add(label + ":" + key + "=" + rank);
                return false;
            }
            return true;
        }

        internal static bool EvaluateRefusedParent(JObject row, IList<string> failures)
        {
            string label = Label(row, "refused-parent");
            if (!RequireObservations(row, label, RefusedParentKeys, failures)) return false;
            bool ok = EvaluateFixtureProficiency((JObject)row["fixture"], label,
                false, false, false, failures);
            if (!(bool)row["slotPresent"]) { failures.Add(label + ":slot-absent"); ok = false; }
            // The feat may remain visible while the native UI shows an unmet
            // prerequisite, but it must not be acquirable.
            if (!(bool)row["parentOffered"])
            { failures.Add(label + ":parent-not-offered"); ok = false; }
            if ((bool)row["parentCanSelect"])
            { failures.Add(label + ":parent-can-select"); ok = false; }
            if ((bool)row["parentSelected"])
            { failures.Add(label + ":parent-selected"); ok = false; }
            if ((bool)row["parentChoiceHeld"])
            { failures.Add(label + ":parent-choice-held"); ok = false; }
            JArray children = (JArray)row["childEligibility"];
            if (children == null || children.Count == 0)
            { failures.Add(label + ":child-eligibility-missing"); ok = false; }
            else if (children.Any(value => (bool)value))
            { failures.Add(label + ":child-eligible-without-proficiency"); ok = false; }
            if ((bool)row["acquiredParent"] || (bool)row["acquiredChild"])
            { failures.Add(label + ":acquired-without-proficiency"); ok = false; }
            return ok;
        }

        internal static bool EvaluateScopedChildRefusal(JObject row,
            IList<string> failures)
        {
            string label = Label(row, "scoped-child-refusal");
            if (!RequireObservations(row, label, ScopedChildRefusalKeys, failures))
                return false;
            bool ok = true;
            if (!(bool)row["slotPresent"]) { failures.Add(label + ":slot-absent"); ok = false; }
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
            if ((int)row["parentRankWhileEmpty"] != 0 ||
                (int)row["refusedChildRankWhileEmpty"] != 0)
            { failures.Add(label + ":empty-selection-granted-a-fact"); ok = false; }
            // Attribution: the same build completes once a legal choice is made.
            if (!(bool)row["legalChildSelected"])
            { failures.Add(label + ":legal-child-refused"); ok = false; }
            if ((bool)row["targetBlocksCompletionAfterLegalChoice"])
            { failures.Add(label + ":target-still-blocks-after-legal-choice"); ok = false; }
            if (!(bool)row["completeAfterLegalChoice"])
            { failures.Add(label + ":not-complete-after-legal-choice"); ok = false; }
            if (!(bool)row["acquiredLegalChild"])
            { failures.Add(label + ":legal-child-not-acquired"); ok = false; }
            if ((bool)row["acquiredRefusedChild"])
            { failures.Add(label + ":refused-child-acquired"); ok = false; }
            return ok;
        }

        internal static bool EvaluateEmptySelection(JObject row, IList<string> failures)
        {
            string label = Label(row, "empty-selection");
            if (!RequireObservations(row, label, EmptySelectionKeys, failures))
                return false;
            bool ok = true;
            if (!(bool)row["slotPresent"]) { failures.Add(label + ":slot-absent"); ok = false; }
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
            if ((bool)row["acquiredParent"] || (bool)row["acquiredAnyChild"])
            { failures.Add(label + ":empty-choice-was-banked"); ok = false; }
            return ok;
        }

        internal static bool EvaluateAcquisition(JObject row, IList<string> failures)
        {
            string label = Label(row, "acquisition");
            if (!RequireObservations(row, label, AcquisitionKeys, failures)) return false;
            bool ok = true;
            if (!(bool)row["slotPresent"]) { failures.Add(label + ":slot-absent"); ok = false; }
            if (!(bool)row["parentCanSelect"] || !(bool)row["parentSelected"])
            { failures.Add(label + ":parent-refused"); ok = false; }
            if (!(bool)row["childCanSelect"] || !(bool)row["childSelected"])
            { failures.Add(label + ":legal-child-refused"); ok = false; }
            if (!(bool)row["completeAfterLegalChoice"])
            { failures.Add(label + ":not-complete-after-legal-choice"); ok = false; }
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
            bool ok = true;
            // One live transaction: the scenario must not have substituted
            // Cancel() plus a fresh visit for the native class change.
            if ((int)row["controllerInstances"] != 1)
            { failures.Add(label + ":not-a-single-transaction"); ok = false; }
            ok &= EvaluateFixtureProficiency((JObject)row["gunslingerFixture"],
                label + ".gunslinger", true, false, false, failures);
            if (!(bool)row["heldBeforeChange"])
            { failures.Add(label + ":choice-was-never-held"); ok = false; }
            ok &= EvaluateFixtureProficiency((JObject)row["fighterFixture"],
                label + ".fighter", false, false, false, failures);
            if ((int)row["fighterGunslingerLevel"] != 0)
            { failures.Add(label + ":pending-class-did-not-change"); ok = false; }
            if ((bool)row["parentEligibleAfterChange"])
            { failures.Add(label + ":parent-eligible-after-change"); ok = false; }
            if (AnyTrue(row["childEligibilityAfterChange"]))
            { failures.Add(label + ":child-eligible-after-change"); ok = false; }
            if ((bool)row["choiceSurvivedChange"])
            { failures.Add(label + ":stale-choice-survived-the-class-change"); ok = false; }
            ok &= EvaluateFixtureProficiency((JObject)row["restoredFixture"],
                label + ".restored", true, false, false, failures);
            if (!(bool)row["parentEligibleAfterRestore"])
            { failures.Add(label + ":parent-not-eligible-after-restore"); ok = false; }
            if (!AllTrue(row["childEligibilityAfterRestore"]))
            { failures.Add(label + ":child-not-eligible-after-restore"); ok = false; }
            if ((bool)row["choiceRestoredByEngine"])
            { failures.Add(label + ":dropped-choice-reappeared"); ok = false; }
            if (!(bool)row["reheldBeforeSecondChange"])
            { failures.Add(label + ":choice-was-never-reheld"); ok = false; }
            if ((bool)row["choiceSurvivedSecondChange"])
            { failures.Add(label + ":stale-choice-survived-the-second-change"); ok = false; }
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
            bool ok = true;
            if ((int)row["controllerInstances"] != 1)
            { failures.Add(label + ":not-a-single-transaction"); ok = false; }
            ok &= EvaluateFixtureProficiency((JObject)row["baseFixture"],
                label + ".base", true, false, false, failures);
            if (!(bool)row["heldBeforeChange"])
            { failures.Add(label + ":choice-was-never-held"); ok = false; }
            if (!(bool)row["archetypeApplied"])
            { failures.Add(label + ":archetype-not-applied"); ok = false; }
            // Musket Master narrows the scope to two-handed firearms.
            ok &= EvaluateFixtureProficiency((JObject)row["archetypeFixture"],
                label + ".archetype", false, false, true, failures);
            if ((int)row["automaticMusketRankAfterChange"] <= 0)
            { failures.Add(label + ":archetype-grant-missing"); ok = false; }
            if (!(bool)row["parentEligibleAfterChange"])
            { failures.Add(label + ":parent-not-eligible-under-archetype"); ok = false; }
            JArray eligibility = (JArray)row["childEligibilityAfterChange"];
            JArray selectable = (JArray)row["childSelectableAfterChange"];
            if (eligibility == null || eligibility.Count != 3 ||
                selectable == null || selectable.Count != 3)
            { failures.Add(label + ":child-observation-shape"); ok = false; }
            else
            {
                // Pistol falls outside the Musket Master proficiency scope.
                // Musket is inside it but is already owned through the
                // archetype's automatic grant, which the native rank check
                // refuses on its own (automaticMusketRankAfterChange proves
                // why). Blunderbuss stays both eligible and acquirable.
                if ((bool)eligibility[0]) { failures.Add(label + ":pistol-eligible-under-archetype"); ok = false; }
                if ((bool)eligibility[1]) { failures.Add(label + ":owned-musket-eligible-under-archetype"); ok = false; }
                if (!(bool)eligibility[2]) { failures.Add(label + ":blunderbuss-not-eligible-under-archetype"); ok = false; }
                if ((bool)selectable[0]) { failures.Add(label + ":pistol-selectable-under-archetype"); ok = false; }
                if ((bool)selectable[1]) { failures.Add(label + ":owned-musket-selectable-under-archetype"); ok = false; }
                if (!(bool)selectable[2]) { failures.Add(label + ":blunderbuss-not-selectable-under-archetype"); ok = false; }
            }
            if ((bool)row["choiceSurvivedChange"])
            { failures.Add(label + ":out-of-scope-choice-survived-the-archetype-change"); ok = false; }
            if (!(bool)row["archetypeRemoved"])
            { failures.Add(label + ":archetype-not-removed"); ok = false; }
            ok &= EvaluateFixtureProficiency((JObject)row["restoredFixture"],
                label + ".restored", true, false, false, failures);
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
            bool ok = true;
            if (!(bool)row["levelOneGrantsRapidReloadMusket"])
            { failures.Add(label + ":level-one-blueprint-grant-missing"); ok = false; }
            // The fixture is a natively built Musket Master: scope is
            // two-handed only, and nothing was granted by hand.
            ok &= EvaluateFixtureProficiency((JObject)row["fixture"], label,
                false, false, true, failures);
            if ((int)row["gunslingerLevel"] <= 0)
            { failures.Add(label + ":no-gunslinger-level"); ok = false; }
            if (!(bool)row["isMusketMasterArchetype"])
            { failures.Add(label + ":archetype-identity-absent"); ok = false; }
            if ((int)row["automaticMusketRank"] <= 0)
            { failures.Add(label + ":automatic-musket-grant-missing"); ok = false; }
            if (!(bool)row["slotPresent"]) { failures.Add(label + ":slot-absent"); ok = false; }
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
            bool ok = true;
            // A genuine Gunslinger: real class levels, no proficiency facts at
            // evaluation time. If either half is unproven the control is
            // invalid and must fail rather than score as a negative result.
            if ((int)row["committedGunslingerLevel"] <= 0)
            { failures.Add(label + ":no-committed-gunslinger-level"); ok = false; }
            ok &= EvaluateFixtureProficiency((JObject)row["committedFixture"],
                label + ".committed", false, false, false, failures);
            if ((int)row["previewGunslingerLevel"] <= 0)
            { failures.Add(label + ":preview-lost-gunslinger-identity"); ok = false; }
            ok &= EvaluateFixtureProficiency((JObject)row["previewFixture"],
                label + ".preview", false, false, false, failures);
            if ((bool)row["parentEligible"])
            { failures.Add(label + ":class-identity-alone-qualified"); ok = false; }
            if (AnyTrue(row["childEligibility"]))
            { failures.Add(label + ":child-qualified-on-class-identity"); ok = false; }
            if (!(bool)row["slotPresent"]) { failures.Add(label + ":slot-absent"); ok = false; }
            if (!(bool)row["parentOffered"])
            { failures.Add(label + ":parent-not-offered"); ok = false; }
            if ((bool)row["parentCanSelect"] || (bool)row["parentSelected"])
            { failures.Add(label + ":parent-selectable-on-class-identity"); ok = false; }
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
