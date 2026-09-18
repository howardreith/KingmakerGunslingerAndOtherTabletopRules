using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.RuntimeTesting;

namespace KingmakerGunslinger.DomainTests
{
    internal static class NativeRacialActionIconFixtureTests
    {
        internal static void ExactConsumerSetIsFixed()
        {
            string[] all = NativeRacialActionIconRules.AllSymbols();
            Assertions.Equal(39, all.Length,
                "The native racial action consumer set is not exactly 39 identities.");
            Assertions.True(all.Distinct().Count() == all.Length,
                "A symbol appears twice in the native racial action set.");
            foreach (string race in new[] { "Ifrit", "Oread", "Sylph", "Undine" })
            {
                string[] set = NativeRacialActionIconRules.SymbolsForRace(race);
                Assertions.True(set.Length > 0 && NativeRacialActionIconRules.IsValidRaceSet(race, set),
                    "The " + race + " action set diverged from the fixed catalog plan.");
                Assertions.Equal(0, set.Count(symbol => !all.Contains(symbol)),
                    "The " + race + " action set introduced a foreign symbol.");
            }
            var union = new[] { "Ifrit", "Oread", "Sylph", "Undine" }
                .SelectMany(NativeRacialActionIconRules.SymbolsForRace).Distinct().ToArray();
            Assertions.True(union.Length == all.Length && union.All(all.Contains),
                "The per-race action sets do not partition the exact 39 consumers.");
            Assertions.Equal(0, NativeRacialActionIconRules.SymbolsForRace("Human").Length,
                "An unsupported race acquired an action set.");
        }

        internal static void RaceSetCorruptionIsRejected()
        {
            string[] reference = NativeRacialActionIconRules.SymbolsForRace("Ifrit");
            Assertions.False(NativeRacialActionIconRules.IsValidRaceSet("Ifrit", reference.Skip(1).ToArray()),
                "A missing action consumer was accepted.");
            Assertions.False(NativeRacialActionIconRules.IsValidRaceSet("Ifrit", reference.Concat(new[] { reference[0] }).ToArray()),
                "A duplicated action consumer was accepted.");
            Assertions.False(NativeRacialActionIconRules.IsValidRaceSet("Ifrit",
                    reference.Take(reference.Length - 1).Concat(new[] { "KMG.ElementalRaces.Foreign.Ability" }).ToArray()),
                "A foreign action consumer was accepted.");
            Assertions.False(NativeRacialActionIconRules.IsValidRaceSet("Oread",
                    NativeRacialActionIconRules.SymbolsForRace("Sylph")),
                "A different race's set was accepted.");
            Assertions.False(NativeRacialActionIconRules.IsValidRaceSet("Ifrit", null),
                "A null action set was accepted.");
        }

        internal static void ModifierCacheOwnershipIsExact()
        {
            Assertions.Equal(NativeRacialActionIconRules.ModifierCachePlan.OwnAndRemoveIfStillEmpty,
                NativeRacialActionIconRules.PlanModifierCache(false, 0),
                "An initially absent empty cache must be owned and removed by the request.");
            Assertions.Equal(NativeRacialActionIconRules.ModifierCachePlan.PreserveExisting,
                NativeRacialActionIconRules.PlanModifierCache(true, 0),
                "A preexisting empty cache must be preserved.");
            Assertions.Equal(NativeRacialActionIconRules.ModifierCachePlan.Reject,
                NativeRacialActionIconRules.PlanModifierCache(true, 1),
                "A populated cache must fail the fixture closed.");
            Assertions.Equal(NativeRacialActionIconRules.ModifierCachePlan.Reject,
                NativeRacialActionIconRules.PlanModifierCache(false, 2),
                "An unexpectedly populated new cache must fail the fixture closed.");
        }

        internal static void RowClassificationIsDerived()
        {
            Assertions.Equal(NativeRacialActionIconRules.NativeActionRowKind.Activatable,
                NativeRacialActionIconRules.Classify(true, true),
                "An activatable row must never be classified as a variant fact.");
            Assertions.Equal(NativeRacialActionIconRules.NativeActionRowKind.Activatable,
                NativeRacialActionIconRules.Classify(true, false),
                "An activatable row must be classified from its own type.");
            Assertions.Equal(NativeRacialActionIconRules.NativeActionRowKind.Variant,
                NativeRacialActionIconRules.Classify(false, true),
                "A parent-owned ability must render as a detached variant row.");
            Assertions.Equal(NativeRacialActionIconRules.NativeActionRowKind.Parent,
                NativeRacialActionIconRules.Classify(false, false),
                "An unparented ability must render as a group parent row.");
        }

        internal static void RequestCaseIsNarrowlyAllowlisted()
        {
            string root = Environment.CurrentDirectory;
            string parser = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting", "RuntimeTestRequest.cs"));
            Assertions.True(parser.Contains("bool nativeActionCase = request.Scenario == RuntimeTestScenarioCatalog.WorkingSaveElementalCharacterCreationRegression"),
                "The native action case must be keyed to the exact creator regression scenario.");
            Assertions.True(parser.Contains("nativeActionCase ? 5 :"),
                "The native action case must widen the parameter count by exactly one member.");
            Assertions.True(parser.Contains("return \"native-action-case-not-allowed\";"),
                "The native action case must fail closed on a wrong case value, class, allocation or exit mode.");
            string orchestrator = File.ReadAllText(Path.Combine(root, "scripts",
                "Invoke-KingmakerRuntimeTest.ps1"));
            Assertions.True(orchestrator.Contains("nativeActionCase=racial-actions with Fighter and point-buy and automatic exit"),
                "The PowerShell orchestrator must preflight the same native action case constraints.");
        }

        internal static void FixtureDispatchAndRestorationAreWired()
        {
            string root = Environment.CurrentDirectory;
            string regression = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting", "ElementalCharacterCreationRegression.cs"));
            Assertions.True(regression.Contains("if (PauseForNativeIconSheet()) return;") &&
                regression.Contains("if (PauseForNativeRacialActionGroup()) return;") &&
                regression.IndexOf("if (PauseForNativeIconSheet()) return;") <
                    regression.IndexOf("if (PauseForNativeRacialActionGroup()) return;"),
                "The action fixture must pause after the sheet pause inside committed creator cleanup.");
            string baseline = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting", "ElementalCharacterCreationBaselineScenario.cs"));
            Assertions.True(baseline.Contains("AppendNativeRacialBuffAssertion();") &&
                baseline.Contains("AppendNativeRacialActionAssertion();"),
                "The action assertion must be appended with the other native icon assertions.");
            string fixture = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting", "ElementalCharacterCreationNativeActionIcons.cs"));
            foreach (string token in new[] {
                "NativeRacialActionIconRules.ControlGuid",
                "NativeRacialActionIconRules.ControlName",
                "var anchorSlotParent = group.transform;",
                "Kingmaker.UI.WidgetFactory.GetWidget(groupPrefab)",
                "slot.Set(owner, data)",
                "ownedWidgets.Add",
                "Kingmaker.UI.WidgetFactory.DisposeWidget(widget)",
                "NativeRacialActionIconRules.DecideOwnedModifierCacheCleanup",
                "ModifierCachePlan.Reject",
                "evidence[\"controlRow\"]",
                "evidence[\"pauseRestored\"]",
                "evidence[\"barStateNotBorrowed\"]",
                "evidence[\"ownedWidgetsDisposed\"]",
                "owner.Descriptor.Remove<UnitPartAbilityModifiers>()",
                "selectionUntouched",
                "SaveWritingApiObserved",
                "game.Player.Party.Contains(owner)",
                "owner.Descriptor.IsCustomCompanion()"
            })
                Assertions.True(fixture.Contains(token),
                    "The action fixture lost its required native guard or restoration step: " + token);
            Assertions.True(fixture.IndexOf("ownedWidgets.Add(controlWidget)") <
                    fixture.IndexOf("controlWidget.Initialize()"),
                "The control widget must be tracked at acquisition, before initialization.");
            Assertions.True(fixture.Contains("_raceIndex != 0") &&
                fixture.Contains("_nativeIconStages.Add"),
                "The action fixture must run exactly once for the first committed mercenary.");
        }

        private static Newtonsoft.Json.Linq.JObject CompleteActionEvidence()
        {
            var race = "Ifrit";
            string[] symbols = NativeRacialActionIconRules.SymbolsForRace(race);
            return new Newtonsoft.Json.Linq.JObject {
                ["race"] = race,
                ["expectedGuids"] = new Newtonsoft.Json.Linq.JArray(symbols),
                ["capturedGuids"] = new Newtonsoft.Json.Linq.JArray(symbols),
                ["restored"] = true,
                ["controlRow"] = new Newtonsoft.Json.Linq.JObject {
                    ["guid"] = NativeRacialActionIconRules.ControlGuid,
                    ["name"] = NativeRacialActionIconRules.ControlName,
                    ["captured"] = true, ["spriteExact"] = true,
                    ["rowActive"] = true, ["statePreserved"] = true } };
        }

        internal static void ActionEvidenceEvaluationRequiresRenderedControl()
        {
            var failures = new List<string>();
            Assertions.True(NativeRacialActionIconRules.EvaluateNativeRacialActionEvidence(
                CompleteActionEvidence(), "Ifrit", failures),
                "Complete evidence must pass. failures=" + string.Join("|", failures));

            var missingControl = CompleteActionEvidence();
            missingControl.Remove("controlRow");
            failures.Clear();
            Assertions.False(NativeRacialActionIconRules.EvaluateNativeRacialActionEvidence(
                missingControl, "Ifrit", failures), "A missing control observation must fail.");
            Assertions.True(failures.Contains("control-observation-missing"),
                "A missing control observation must be named.");

            var wrongIdentity = CompleteActionEvidence();
            wrongIdentity["controlRow"]["guid"] = "00000000000000000000000000000000";
            failures.Clear();
            Assertions.False(NativeRacialActionIconRules.EvaluateNativeRacialActionEvidence(
                wrongIdentity, "Ifrit", failures), "A wrong control identity must fail.");
            Assertions.True(failures.Contains("control-identity"), "A wrong control identity must be named.");

            var wrongName = CompleteActionEvidence();
            wrongName["controlRow"]["name"] = "OtherToggle";
            failures.Clear();
            Assertions.False(NativeRacialActionIconRules.EvaluateNativeRacialActionEvidence(
                wrongName, "Ifrit", failures), "A wrong control name must fail.");

            var notCaptured = CompleteActionEvidence();
            notCaptured["controlRow"]["captured"] = false;
            failures.Clear();
            Assertions.False(NativeRacialActionIconRules.EvaluateNativeRacialActionEvidence(
                notCaptured, "Ifrit", failures), "A control without a capture record must fail.");
            Assertions.True(failures.Contains("control-capture-record"), "A missing capture record must be named.");

            var wrongSprite = CompleteActionEvidence();
            wrongSprite["controlRow"]["spriteExact"] = false;
            failures.Clear();
            Assertions.False(NativeRacialActionIconRules.EvaluateNativeRacialActionEvidence(
                wrongSprite, "Ifrit", failures), "A wrong rendered control sprite must fail.");
            Assertions.True(failures.Contains("control-rendered-sprite"), "A wrong control sprite must be named.");

            var inactiveRow = CompleteActionEvidence();
            inactiveRow["controlRow"]["rowActive"] = false;
            failures.Clear();
            Assertions.False(NativeRacialActionIconRules.EvaluateNativeRacialActionEvidence(
                inactiveRow, "Ifrit", failures), "An inactive control row must fail.");

            var stateChanged = CompleteActionEvidence();
            stateChanged["controlRow"]["statePreserved"] = false;
            failures.Clear();
            Assertions.False(NativeRacialActionIconRules.EvaluateNativeRacialActionEvidence(
                stateChanged, "Ifrit", failures), "A changed initial control state must fail.");

            var substitute = CompleteActionEvidence();
            var captured = (Newtonsoft.Json.Linq.JArray)substitute["capturedGuids"];
            captured[0] = NativeRacialActionIconRules.ControlGuid;
            failures.Clear();
            Assertions.False(NativeRacialActionIconRules.EvaluateNativeRacialActionEvidence(
                substitute, "Ifrit", failures), "A control GUID must not replace a consumer capture.");
            Assertions.True(failures.Contains("captured-sequence"), "A substituted sequence must be named.");

            var notRestored = CompleteActionEvidence();
            notRestored["restored"] = false;
            failures.Clear();
            Assertions.False(NativeRacialActionIconRules.EvaluateNativeRacialActionEvidence(
                notRestored, "Ifrit", failures), "Unrestored evidence must fail.");
            Assertions.True(failures.Contains("restoration"), "A restoration failure must be named.");

            var countMismatch = CompleteActionEvidence();
            ((Newtonsoft.Json.Linq.JArray)countMismatch["expectedGuids"]).RemoveAt(0);
            failures.Clear();
            Assertions.False(NativeRacialActionIconRules.EvaluateNativeRacialActionEvidence(
                countMismatch, "Ifrit", failures), "A wrong expected consumer count must fail.");

            Assertions.False(NativeRacialActionIconRules.EvaluateNativeRacialActionEvidence(
                null, "Ifrit", failures), "Missing evidence must fail closed.");
        }

        internal static void ModifierCacheCleanupDispatchIsExact()
        {
            Assertions.Equal(NativeRacialActionIconRules.ModifierCacheCleanupDecision.NoRemovalNeeded,
                NativeRacialActionIconRules.DecideOwnedModifierCacheCleanup(false, false, 0),
                "An absent cache that never materialized needs no removal.");
            Assertions.Equal(NativeRacialActionIconRules.ModifierCacheCleanupDecision.RemoveOwnedEmptyPart,
                NativeRacialActionIconRules.DecideOwnedModifierCacheCleanup(true, true, 0),
                "The exact observed empty instance may be removed.");
            Assertions.Equal(NativeRacialActionIconRules.ModifierCacheCleanupDecision.FailReplacedPart,
                NativeRacialActionIconRules.DecideOwnedModifierCacheCleanup(true, false, 0),
                "An unexpectedly replaced part must fail without removal.");
            Assertions.Equal(NativeRacialActionIconRules.ModifierCacheCleanupDecision.FailPopulated,
                NativeRacialActionIconRules.DecideOwnedModifierCacheCleanup(true, true, 2),
                "A populated cache must fail without removal, even as the observed instance.");
            Assertions.Equal(NativeRacialActionIconRules.ModifierCachePlan.Reject,
                NativeRacialActionIconRules.PlanModifierCache(true, 1),
                "A populated pre-existing cache must be rejected before any mutation.");
        }

        internal static void ExistingCachePreservationIsExact()
        {
            string[] empty = new string[0];
            string[] a = { "guid1:1", "guid2:2" };
            Assertions.True(NativeRacialActionIconRules.ExistingCachePreserved(true, a, a),
                "Same instance and identical ordered entries are preserved.");
            Assertions.False(NativeRacialActionIconRules.ExistingCachePreserved(true, a, a.Reverse().ToArray()),
                "Reordered entries are not preserved.");
            Assertions.False(NativeRacialActionIconRules.ExistingCachePreserved(true, a, new[] { "guid1:1" }),
                "A dropped entry is not preserved.");
            Assertions.False(NativeRacialActionIconRules.ExistingCachePreserved(true, a, new[] { "guid1:1", "guid2:3" }),
                "A changed source fact is not preserved.");
            Assertions.False(NativeRacialActionIconRules.ExistingCachePreserved(false, a, a),
                "A replaced instance is not preserved even with equal entries.");
            Assertions.True(NativeRacialActionIconRules.ExistingCachePreserved(true, empty, empty),
                "An existing empty cache is preserved exactly.");
        }
    }
}
