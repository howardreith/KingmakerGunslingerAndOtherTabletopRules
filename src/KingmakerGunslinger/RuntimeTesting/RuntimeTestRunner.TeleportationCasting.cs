using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Globalmap;
using Kingmaker.Globalmap.State;
using Kingmaker.Kingdom;
using Kingmaker.UI;
using Kingmaker.UI.GlobalMap;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.Spells.Teleportation;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private string _teleportationNativeActions;
        private static GlobalMapMessageBox TeleportationFixturePanel()
        {
            return Resources.FindObjectsOfTypeAll<GlobalMapMessageBox>().Single(value => value != null &&
                value.gameObject.scene.IsValid() && value.gameObject.scene.isLoaded);
        }
        private void ObserveTeleportationVanillaPanel(GlobalMapLocation target, List<RuntimeTestAssertion> assertions, List<object> captures, string path)
        {
            if (_request.Scenario != RuntimeTestScenarioCatalog.DisposableTeleportationCasting || _workingSaveSmoke.WriteObserved)
                throw new InvalidOperationException("Only the guarded casting request may drive this native destination fixture.");
            var panel = TeleportationFixturePanel();
            var map = GlobalMapRules.State;
            var position = map.PartyPosition;
            var time = Game.Instance.Player.GameTime;
            panel.OnLocationSelect(target.Blueprint, false);
            _teleportationNativeActions = TeleportationNativeButtons(panel);
            captures.Add(new { step = "native-no-spell-interaction", selectedId = target.Blueprint.AssetGuid,
                nativeActions = _teleportationNativeActions, visible = panel.gameObject.activeInHierarchy,
                customRows = panel.GetComponentsInChildren<TeleportDestinationRows>(true).Length });
            assertions.Add(Assertion("teleportation-ui-vanilla-no-spell", "native selection opens only its existing panel/actions; no spell UI or travel change",
                "rows=" + panel.GetComponentsInChildren<TeleportDestinationRows>(true).Length,
                WorldMapPointSpellActionPatches.Installed && panel.gameObject.activeInHierarchy &&
                panel.GetComponentsInChildren<TeleportDestinationRows>(true).Length == 0 && !TeleportContextConfirmationPresenter.Pending &&
                !DialogMessageBox.Instance.IsShown && ReferenceEquals(position, map.PartyPosition) && map.TravelData == null &&
                time == Game.Instance.Player.GameTime, path));
            panel.Hide();
        }
        private void RunTeleportationContextualCasts(GlobalMapLocation origin, GlobalMapLocation target, GlobalMapLocation oleg,
            GlobalMapLocation capital, Spellbook[] books, RegionState capitalRegion, MethodInfo setClaimed,
            List<RuntimeTestAssertion> assertions, List<object> captures, string path)
        {
            if (_request.Scenario != RuntimeTestScenarioCatalog.DisposableTeleportationCasting || !_request.ExitAfterCompletion ||
                _workingSaveSmoke.WriteObserved || !WorldMapPointSpellActionPatches.Installed)
                throw new InvalidOperationException("Actual contextual casting requires the guarded disposable casting scenario.");
            var panel = TeleportationFixturePanel();
            var rules = GlobalMapRules.Instance;
            var map = GlobalMapRules.State;
            setClaimed.Invoke(capitalRegion, new object[] { false });
            Func<string> slots = () => string.Join("|", books.Select(TeleportResourceFingerprint));
            Action restoreOrigin = () => {
                panel.Hide(); rules.SetCurrentPosition(new MapPosition(origin.Blueprint)); rules.UpdatePawnPosition();
                foreach (var book in books) book.Rest();
            };
            restoreOrigin();
            string before = slots();
            panel.OnLocationSelect(target.Blueprint, false);
            var rows = panel.GetComponentsInChildren<TeleportDestinationRows>(true).SingleOrDefault();
            captures.Add(new { step = "native-augmented-interaction", selectedId = target.Blueprint.AssetGuid,
                hooksInstalled = WorldMapPointSpellActionPatches.Installed, context = TeleportationWorldMapAdapter.Capture(false).Diagnostic,
                rowCount = rows == null ? 0 : rows.Actions.Count, nativeActions = TeleportationNativeButtons(panel),
                panel = DescribeTeleportationNativePanel(panel) });
            if (rows == null) throw new InvalidOperationException("No live contextual spell rows were appended to the native panel.");
            assertions.Add(Assertion("teleportation-ui-native-actions-preserved", "native buttons, order, labels, active state and serialized callbacks unchanged",
                "nativeButtonsMatch=" + (_teleportationNativeActions == TeleportationNativeButtons(panel)),
                _teleportationNativeActions == TeleportationNativeButtons(panel), path));
            assertions.Add(Assertion("teleportation-ui-real-source-rows", "six distinct caster/book rows with exact current labels and isolated callbacks",
                "rows=" + rows.Actions.Count, rows.Actions.Count == 6 && rows.Actions.Select(value => value.Key).Distinct().Count() == 6 &&
                rows.Buttons.Select((button, index) => button.onClick.GetPersistentEventCount() == 0 && button.interactable &&
                    button.GetComponentInChildren<TextMeshProUGUI>(true).text == TeleportContextPresentation.Row(rows.Actions[index], TeleportationText.Get)).All(value => value), path));
            var viewport = (RectTransform)rows.transform;
            assertions.Add(Assertion("teleportation-ui-native-layout", "appended native style row viewport has positive measured geometry",
                "width=" + viewport.rect.width + ";height=" + viewport.rect.height,
                viewport.rect.width > 300 && viewport.rect.height > 0 &&
                rows.Buttons.Select(value => ((RectTransform)value.transform).anchoredPosition.y).Distinct().Count() == rows.Actions.Count &&
                rows.Buttons.All(value => ((RectTransform)value.transform).rect.width >= viewport.rect.width - 1 &&
                    ((RectTransform)value.transform).rect.height > 0), path));
            panel.OnLocationSelect(target.Blueprint, false);
            assertions.Add(Assertion("teleportation-ui-reopen-deduplicates", "one row container and one row per source after repeated native selection",
                "containers=" + panel.GetComponentsInChildren<TeleportDestinationRows>(true).Length,
                panel.GetComponentsInChildren<TeleportDestinationRows>(true).Length == 1 &&
                panel.GetComponentInChildren<TeleportDestinationRows>(true).Actions.Count == 6 && before == slots(), path));
            panel.Hide();
            assertions.Add(Assertion("teleportation-ui-cancel-context", "no slot expenditure, travel, or remaining rows",
                "slotsUnchanged=" + (before == slots()), before == slots() && map.TravelData == null &&
                map.PartyLocation == origin.Blueprint && panel.GetComponentsInChildren<TeleportDestinationRows>(true).Length == 0, path));

            var cancelled = OpenTeleportationFixtureConfirmation(panel, target, TeleportSpellKind.Teleport, TeleportCastSourceKind.Prepared,
                new TeleportationFixtureRolls(new[] { 1 }));
            string confirmation = cancelled.Message;
            TeleportationFixtureDialogButton("m_ButtonNo").onClick.Invoke();
            assertions.Add(Assertion("teleportation-ui-cancel-confirmation", "native Cancel closes confirmation and spends no spell use",
                "transaction=" + cancelled.Transaction.State, cancelled.Transaction.State == TeleportTransactionState.Cancelled &&
                before == slots() && map.PartyLocation == origin.Blueprint && map.TravelData == null && !TeleportContextConfirmationPresenter.Pending, path));
            assertions.Add(Assertion("teleportation-ui-confirmation-odds", "selected caster, destination, resource and exact Viewed once d100 percentages",
                confirmation, confirmation.Contains("On target: 76%") && confirmation.Contains("Off target: 12%") &&
                confirmation.Contains("Similar location: 8%") && confirmation.Contains("Mishap: 4%") && confirmation.Contains("Ordinary visits: 1"), path));

            foreach (var spec in new[] {
                new { Name = "teleport-on-target", Spell = TeleportSpellKind.Teleport, Source = TeleportCastSourceKind.Prepared, Target = target, Roll = 76, Outcome = TeleportOutcomeKind.OnTarget },
                new { Name = "greater-exact", Spell = TeleportSpellKind.GreaterTeleport, Source = TeleportCastSourceKind.Spontaneous, Target = target, Roll = 0, Outcome = TeleportOutcomeKind.OnTarget },
                new { Name = "recall-precapital", Spell = TeleportSpellKind.WordOfRecall, Source = TeleportCastSourceKind.Prepared, Target = oleg, Roll = 0, Outcome = TeleportOutcomeKind.OnTarget },
                new { Name = "recall-established-capital", Spell = TeleportSpellKind.WordOfRecall, Source = TeleportCastSourceKind.Prepared, Target = capital, Roll = 0, Outcome = TeleportOutcomeKind.OnTarget },
                new { Name = "teleport-off-target", Spell = TeleportSpellKind.Teleport, Source = TeleportCastSourceKind.Prepared, Target = target, Roll = 77, Outcome = TeleportOutcomeKind.OffTarget },
                new { Name = "teleport-similar-location", Spell = TeleportSpellKind.Teleport, Source = TeleportCastSourceKind.Prepared, Target = target, Roll = 96, Outcome = TeleportOutcomeKind.SimilarLocation }
            })
            {
                restoreOrigin();
                setClaimed.Invoke(capitalRegion, new object[] { spec.Target == capital });
                var dice = new TeleportationFixtureRolls(spec.Roll == 0 ? new int[0] : new[] { spec.Roll });
                var request = OpenTeleportationFixtureConfirmation(panel, spec.Target, spec.Spell, spec.Source, dice);
                var yes = TeleportationFixtureDialogButton("m_ButtonYes");
                yes.onClick.Invoke();
                captures.Add(new { step = spec.Name, confirmation = request.Message, transaction = request.Transaction.State.ToString(),
                    diagnostic = request.Transaction.Diagnostic, result = request.Execution.LastEvidence,
                    resource = request.Execution.Resource == null ? null : request.Execution.Resource.Evidence() });
                if (request.Transaction.State != TeleportTransactionState.Completed)
                    throw new InvalidOperationException(spec.Name + " did not complete: " + request.Transaction.State + ";" + request.Transaction.Diagnostic);
                var result = request.Transaction.Result;
                bool exact = spec.Outcome == TeleportOutcomeKind.OnTarget;
                assertions.Add(Assertion("teleportation-ui-" + spec.Name, "native row -> native Cast confirmation -> one real slot -> protected canonical relocation",
                    "outcome=" + result.Outcome + ";point=" + result.DestinationId,
                    result.Status == TeleportExecutionStatus.Arrived && result.Outcome == spec.Outcome &&
                    (exact ? result.DestinationId == spec.Target.Blueprint.AssetGuid : result.DestinationId != origin.Blueprint.AssetGuid &&
                        result.DestinationId != spec.Target.Blueprint.AssetGuid && result.Alternate != null && result.Alternate.Found) &&
                    request.Execution.Resource.ObserveExpenditure() == TeleportExpenditure.ExactlyOne &&
                    dice.D100Count == (spec.Spell == TeleportSpellKind.Teleport ? 1 : 0) && dice.D10Count == 0 &&
                    map.PartyLocation.AssetGuid == result.DestinationId && !TeleportContextConfirmationPresenter.Pending, path));
                string spent = slots();
                yes.onClick.Invoke();
                assertions.Add(Assertion("teleportation-ui-" + spec.Name + "-duplicate-confirmation", "duplicate native callback spends nothing further",
                    "unchanged=" + (spent == slots()), spent == slots() && request.Execution.Resource.ObserveExpenditure() == TeleportExpenditure.ExactlyOne, path));
            }
            restoreOrigin();
            // Oleg is Very familiar: the normal canonical RNG can be exercised
            // without authorizing unpredictable mishap damage in this fixture.
            var canonical = OpenTeleportationFixtureConfirmation(panel, oleg, TeleportSpellKind.Teleport, TeleportCastSourceKind.Prepared, null);
            TeleportationFixtureDialogButton("m_ButtonYes").onClick.Invoke();
            captures.Add(new { step = "canonical-native-dice", transaction = canonical.Transaction.State.ToString(), result = canonical.Execution.LastEvidence });
            var canonicalResult = canonical.Transaction.Result;
            assertions.Add(Assertion("teleportation-ui-canonical-native-dice", "normal production RNG through the same row and confirmation; no injected sequence",
                "roll=" + (canonicalResult == null ? 0 : canonicalResult.D100), canonical.Transaction.State == TeleportTransactionState.Completed &&
                canonicalResult != null && canonicalResult.Status == TeleportExecutionStatus.Arrived && canonicalResult.D100 >= 1 && canonicalResult.D100 <= 100 &&
                canonicalResult.Outcome == TeleportRollTable.Resolve(TeleportFamiliarity.VeryFamiliar, canonicalResult.D100) &&
                canonical.Execution.Resource.ObserveExpenditure() == TeleportExpenditure.ExactlyOne, path));
            restoreOrigin();
            var contextForAlternates = TeleportationWorldMapAdapter.Capture(false);
            var otherLegal = rules.AllLocations.Where(value => value.Blueprint != target.Blueprint &&
                TeleportDestinationPolicy.Evaluate(TeleportationWorldMapAdapter.ReadDestination(contextForAlternates, value.Blueprint),
                    origin.Blueprint.AssetGuid, TeleportationWorldMapAdapter.Forbidden).Eligible)
                .Select(value => map.Locations[value.Blueprint]).ToArray();
            bool[] originalClosed = otherLegal.Select(value => value.IsClosed).ToArray();
            try
            {
                foreach (var location in otherLegal) location.IsClosed = true;
                var request = OpenTeleportationFixtureConfirmation(panel, target, TeleportSpellKind.Teleport, TeleportCastSourceKind.Prepared,
                    new TeleportationFixtureRolls(new[] { 77 }));
                TeleportationFixtureDialogButton("m_ButtonYes").onClick.Invoke();
                captures.Add(new { step = "no-legal-alternate", transaction = request.Transaction.State.ToString(), result = request.Execution.LastEvidence });
                assertions.Add(Assertion("teleportation-ui-no-legal-alternate", "rules-level failure stays at origin with one real use spent",
                    "transaction=" + request.Transaction.State, request.Transaction.State == TeleportTransactionState.Completed &&
                    request.Transaction.Result.Status == TeleportExecutionStatus.NoLegalAlternate && map.PartyLocation == origin.Blueprint &&
                    request.Execution.Resource.ObserveExpenditure() == TeleportExpenditure.ExactlyOne, path));
            }
            finally { for (int index = 0; index < otherLegal.Length; index++) otherLegal[index].IsClosed = originalClosed[index]; }
            restoreOrigin();
            before = slots();
            var compensated = OpenTeleportationFixtureConfirmation(panel, target, TeleportSpellKind.Teleport, TeleportCastSourceKind.Prepared,
                new TeleportationFixtureRolls(new int[0]));
            TeleportationFixtureDialogButton("m_ButtonYes").onClick.Invoke();
            captures.Add(new { step = "pre-effect-technical-compensation", transaction = compensated.Transaction.State.ToString(),
                diagnostic = compensated.Transaction.Diagnostic, resource = compensated.Execution.Resource == null ? null : compensated.Execution.Resource.Evidence() });
            assertions.Add(Assertion("teleportation-ui-pre-effect-compensation", "request-local dice failure restores the exact native use before any damage or relocation",
                "transaction=" + compensated.Transaction.State, compensated.Transaction.State == TeleportTransactionState.TechnicalFailureCompensated &&
                !compensated.Transaction.MaterialEffectStarted && compensated.Execution.Resource.ObserveExpenditure() == TeleportExpenditure.None &&
                before == slots() && map.PartyLocation == origin.Blueprint, path));

            foreach (bool repeated in new[] { false, true })
            {
                restoreOrigin();
                var travelers = TeleportationTravelers.Read(Game.Instance.Player).Units;
                int[] originalDamage = travelers.Select(value => value.Damage).ToArray();
                if (travelers.Any(value => value.Descriptor.State.IsDead || value.Descriptor.State.IsUnconscious ||
                    value.Descriptor.Stats.HitPoints.ModifiedValue - value.Damage <= 3))
                    throw new InvalidOperationException("Disposable mishap qualification requires living travelers with more than three current HP.");
                var dice = new TeleportationFixtureRolls(repeated ? new[] { 97, 100, 76 } : new[] { 97, 76 },
                    repeated ? new[] { 1, 2 } : new[] { 1 });
                try
                {
                    var request = OpenTeleportationFixtureConfirmation(panel, target, TeleportSpellKind.Teleport, TeleportCastSourceKind.Prepared, dice);
                    TeleportationFixtureDialogButton("m_ButtonYes").onClick.Invoke();
                    captures.Add(new { step = repeated ? "repeated-mishap" : "mishap", transaction = request.Transaction.State.ToString(),
                        diagnostic = request.Transaction.Diagnostic, result = request.Execution.LastEvidence,
                        damageBefore = originalDamage, damageAfter = travelers.Select(value => value.Damage).ToArray() });
                    var result = request.Transaction.Result;
                    assertions.Add(Assertion(repeated ? "teleportation-ui-repeated-mishap" : "teleportation-ui-mishap",
                        "native contextual cast deals exact shared d10 damage to every living traveler, rerolls and spends once",
                        "transaction=" + request.Transaction.State + ";d10Count=" + dice.D10Count,
                        request.Transaction.State == TeleportTransactionState.Completed && result != null &&
                        result.Status == TeleportExecutionStatus.Arrived && result.Mishaps == (repeated ? 2 : 1) &&
                        result.DestinationId == target.Blueprint.AssetGuid && dice.D100Count == (repeated ? 3 : 2) &&
                        dice.D10Count == (repeated ? 2 : 1) &&
                        request.Execution.Resource.ObserveExpenditure() == TeleportExpenditure.ExactlyOne &&
                        travelers.Select((value, index) => value.Damage - originalDamage[index] == (repeated ? 3 : 1)).All(value => value), path));
                }
                finally
                {
                    // Request-local damage cleanup only. The production adapter
                    // uses RuleDealDamage and never restores HP or imposes a floor.
                    for (int index = 0; index < travelers.Length; index++) travelers[index].Damage = originalDamage[index];
                    if (!travelers.Select(value => value.Damage).SequenceEqual(originalDamage))
                        throw new InvalidOperationException("Exact disposable mishap HP restoration failed.");
                }
            }
            restoreOrigin();
            var stale = OpenTeleportationFixtureConfirmation(panel, target, TeleportSpellKind.Teleport, TeleportCastSourceKind.Prepared,
                new TeleportationFixtureRolls(new[] { 1 }));
            before = slots();
            map.Locations[target.Blueprint].IsClosed = true;
            TeleportationFixtureDialogButton("m_ButtonYes").onClick.Invoke();
            assertions.Add(Assertion("teleportation-ui-stale-destination", "native confirmation revalidates destination and spends zero",
                "transaction=" + stale.Transaction.State, stale.Transaction.State == TeleportTransactionState.Cancelled && before == slots() &&
                map.PartyLocation == origin.Blueprint, path));
            map.Locations[target.Blueprint].IsClosed = false;
            restoreOrigin();
            before = slots();
            var staleCaster = OpenTeleportationFixtureConfirmation(panel, target, TeleportSpellKind.Teleport, TeleportCastSourceKind.Prepared,
                new TeleportationFixtureRolls(new[] { 1 }));
            var caster = books[0].Owner.Unit;
            bool originalDetached = caster.IsDetached;
            try
            {
                caster.IsDetached = true;
                TeleportationFixtureDialogButton("m_ButtonYes").onClick.Invoke();
                assertions.Add(Assertion("teleportation-ui-stale-caster", "lost active caster cancels native confirmation with zero spell expenditure",
                    "transaction=" + staleCaster.Transaction.State, staleCaster.Transaction.State == TeleportTransactionState.Cancelled && before == slots() &&
                    map.PartyLocation == origin.Blueprint, path));
            }
            finally { caster.IsDetached = originalDetached; }
            restoreOrigin();
            before = slots();
            var replaced = OpenTeleportationFixtureConfirmation(panel, target, TeleportSpellKind.Teleport, TeleportCastSourceKind.Prepared,
                new TeleportationFixtureRolls(new[] { 1 }));
            Spellbook originalBook = books[0];
            var dictionary = (Dictionary<Kingmaker.Blueprints.Classes.Spells.BlueprintSpellbook, Spellbook>)typeof(Kingmaker.UnitLogic.UnitDescriptor)
                .GetField("m_Spellbooks", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(originalBook.Owner);
            Spellbook replacement = null;
            try
            {
                // Native DeleteSpellbook only removes this dictionary entry. The
                // original instance remains untouched for exact fixture restoration.
                originalBook.Owner.DeleteSpellbook(originalBook.Blueprint);
                replacement = originalBook.Owner.DemandSpellbook(originalBook.Blueprint);
                for (int level = 0; level < 20; level++) replacement.AddCasterLevel();
                replacement.UpdateAllSlotsSize(false);
                replacement.AddKnown(5, Bootstrap.BlueprintBootstrap.Teleportation.Teleport, true);
                if (!replacement.Memorize(new Kingmaker.UnitLogic.Abilities.AbilityData(Bootstrap.BlueprintBootstrap.Teleportation.Teleport, replacement), null))
                    throw new InvalidOperationException("Replacement native prepared book fixture failed.");
                replacement.Rest();
                string replacementBefore = TeleportResourceFingerprint(replacement);
                TeleportationFixtureDialogButton("m_ButtonYes").onClick.Invoke();
                assertions.Add(Assertion("teleportation-ui-replaced-spellbook", "a new physical book with the same blueprint/caster/count cannot satisfy an existing confirmation",
                    "transaction=" + replaced.Transaction.State, replaced.Transaction.State == TeleportTransactionState.Cancelled &&
                    before == slots() && replacementBefore == TeleportResourceFingerprint(replacement) && map.PartyLocation == origin.Blueprint, path));
            }
            finally
            {
                if (replacement != null) { originalBook.Owner.DeleteSpellbook(replacement.Blueprint); replacement.Dispose(); }
                dictionary[originalBook.Blueprint] = originalBook;
                if (!ReferenceEquals(originalBook.Owner.GetSpellbook(originalBook.Blueprint), originalBook) || before != slots())
                    throw new InvalidOperationException("Exact original spellbook fixture restoration failed.");
            }
            restoreOrigin();
        }
        private static TeleportContextConfirmationPresenter OpenTeleportationFixtureConfirmation(GlobalMapMessageBox panel,
            GlobalMapLocation point, TeleportSpellKind spell, TeleportCastSourceKind kind, TeleportationFixtureRolls rolls)
        {
            panel.OnLocationSelect(point.Blueprint, false);
            var rows = panel.GetComponentInChildren<TeleportDestinationRows>(true);
            if (rows == null) throw new InvalidOperationException("No contextual actions for guarded " + spell + " cast at " + point.Blueprint.AssetGuid);
            int index = Array.FindIndex(rows.Actions.ToArray(), value => value.Source.Spell == spell && value.Source.Kind == kind);
            if (index < 0) throw new InvalidOperationException("Guarded cast source is absent from native contextual actions.");
            // This private runtime method is reached only after the request/parser,
            // named working-save fingerprint, fixture and write-sentinel gates.
            rows.QualificationRolls = rolls;
            rows.Buttons[index].onClick.Invoke();
            var request = TeleportContextConfirmationPresenter.Current;
            if (request == null || !DialogMessageBox.Instance.IsShown || panel.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Selected native spell row did not open its own confirmation and close the destination presenter.");
            return request;
        }
        private static Button TeleportationFixtureDialogButton(string field)
        { return (Button)typeof(DialogMessageBox).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(DialogMessageBox.Instance); }
        private static string TeleportationNativeButtons(GlobalMapMessageBox panel)
        {
            return JsonConvert.SerializeObject(panel.GetComponentsInChildren<Button>(true).Where(value => value.GetComponentInParent<TeleportDestinationRows>() == null)
                .Select(value => new { name = value.name, active = value.gameObject.activeSelf, interactable = value.interactable,
                    labels = value.GetComponentsInChildren<TextMeshProUGUI>(true).Select(label => label.text).ToArray(),
                    callbacks = Enumerable.Range(0, value.onClick.GetPersistentEventCount()).Select(value.onClick.GetPersistentMethodName).ToArray() }).ToArray());
        }
        private static void CloseTeleportationFixturePanels()
        {
            if (DialogMessageBox.Instance != null && DialogMessageBox.Instance.IsShown)
                TeleportationFixtureDialogButton("m_ButtonNo").onClick.Invoke();
            var panel = TeleportationFixturePanel();
            if (panel.gameObject.activeSelf) panel.Hide();
        }
        private sealed class TeleportationFixtureRolls : ITeleportationRolls
        {
            private readonly Queue<int> _d100;
            private readonly Queue<int> _d10;
            internal int D100Count { get; private set; }
            internal int D10Count { get; private set; }
            internal TeleportationFixtureRolls(IEnumerable<int> d100, IEnumerable<int> d10 = null)
            { _d100 = new Queue<int>(d100); _d10 = new Queue<int>(d10 ?? new int[0]); }
            public int D100() { D100Count++; return _d100.Dequeue(); }
            public int D10() { D10Count++; return _d10.Dequeue(); }
            public int Index(int count) { if (count < 1) throw new InvalidOperationException("No alternate bucket."); return 0; }
        }
    }
}
