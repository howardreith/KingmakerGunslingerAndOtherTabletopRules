using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.UI;
using TMPro;
using UnityEngine;

namespace KingmakerGunslinger.Spells.Teleportation
{
    // Diagnostics handle for a synchronously settled direct cast; the presenter
    // component itself is destroyed immediately after settlement.
    internal sealed class TeleportDirectCastOutcome
    {
        internal TeleportCastTransaction Transaction;
        internal TeleportationCastExecution Execution;
        internal int Frame;
    }

    // A component only for the lifetime of one native confirmation. The static
    // reference is an overlap guard, never a spell/ledger/campaign source of truth.
    internal sealed class TeleportContextConfirmationPresenter : MonoBehaviour
    {
        private static TeleportContextConfirmationPresenter _current;
        private TeleportationConfirmationSurface _surface;
        private bool _opened;
        private bool _direct;
        private WorldMapPointSpellAction _action;
        private TeleportationWorldMapContext _openedContext;
        private Kingmaker.UnitLogic.Spellbook _openedBook;
        private Action<DialogMessageBoxBase.BoxButton> _callback;
        private TeleportFamiliarity _familiarity;
        private bool _settled;
        private readonly List<GameObject> _sectionRules = new List<GameObject>();
        private bool _sectionsDecorated;
        private static readonly FieldInfo MessageLabelField = typeof(DialogMessageBox).GetField("m_Messagelabel", BindingFlags.Instance | BindingFlags.NonPublic);
        internal IReadOnlyList<GameObject> SectionRules { get { return _sectionRules.ToArray(); } }
        internal TeleportCastTransaction Transaction { get; private set; }
        internal TeleportationCastExecution Execution { get; private set; }
        internal string Message { get; private set; }
        internal static bool Pending { get { return _current != null; } }
        internal static TeleportContextConfirmationPresenter Current { get { return _current; } }
        // The most recent settled direct cast, for structured diagnostics only.
        internal static TeleportDirectCastOutcome LastDirectCast { get; private set; }
        // Guarded fixtures reset the diagnostics handle before invoking a row.
        internal static void ResetDirectCastDiagnostics() { LastDirectCast = null; }
        internal static bool CanOpen { get { return !Pending && TeleportationConfirmationSurface.Available() != null; } }
        // Per-action executability: an unrelated active modal blocks everything;
        // a merely unavailable confirmation presenter blocks only confirmed
        // spells, so Greater Teleport stays usable without letting ordinary
        // Teleport bypass its confirmation.
        internal static bool CanExecute(WorldMapPointSpellAction action)
        {
            return action != null && TeleportBeginPolicy.CanExecuteAction(Pending,
                TeleportationConfirmationSurface.UnrelatedModalShown(),
                TeleportationConfirmationSurface.Available() != null, action.Source.Spell);
        }
        // Rows compose when at least one offered action can execute.
        internal static bool CanBegin(IReadOnlyList<WorldMapPointSpellAction> actions)
        {
            if (actions == null) return false;
            return TeleportBeginPolicy.OffersAnyAction(Pending,
                TeleportationConfirmationSurface.UnrelatedModalShown(),
                TeleportationConfirmationSurface.Available() != null,
                actions.Select(value => value == null ? (TeleportSpellKind)(-1) : value.Source.Spell).ToArray());
        }
        // One entry point for every destination action. Greater Teleport casts
        // immediately; every other spell opens its owned native confirmation.
        internal static void Begin(WorldMapPointSpellAction action, TeleportationWorldMapContext context, ITeleportationRolls qualificationRolls = null)
        {
            if (action != null && action.Source.Spell == TeleportSpellKind.GreaterTeleport) OpenDirect(action, context, qualificationRolls);
            else Open(action, context, qualificationRolls);
        }

        internal static void Open(WorldMapPointSpellAction action, TeleportationWorldMapContext context, ITeleportationRolls qualificationRolls = null)
        {
            if (Pending || action == null || context == null || !context.Usable) return;
            var surface = TeleportationConfirmationSurface.Available();
            if (surface == null) return;
            Kingmaker.UnitLogic.Spellbook openedBook = null;
            if (action.Source.Kind == TeleportCastSourceKind.Scroll)
            {
                // Inventory-backed sources bind the shared stock, not a book;
                // the exact item is re-resolved at capture time.
                if (TeleportationScrollAdapter.Resolve(action.Source) == null) return;
            }
            else
            {
                var source = TeleportationSpellbookAdapter.Resolve(action.Source);
                if (source == null) return;
                openedBook = source.Book;
            }
            var self = surface.Host.AddComponent<TeleportContextConfirmationPresenter>();
            try
            {
                self._surface = surface;
                self._action = action;
                self._openedContext = context;
                self._openedBook = openedBook;
                self._familiarity = TeleportationCastExecution.FamiliarityFor(context, action.Destination.Id);
                self.Transaction = new TeleportCastTransaction(action);
                self.Execution = qualificationRolls == null ? new TeleportationCastExecution() : new TeleportationCastExecution(qualificationRolls);
                self.Message = TeleportContextPresentation.Confirmation(action, self._familiarity, TeleportationText.Get);
                self._callback = self.Closed;
                _current = self;
                EventBus.RaiseEvent<IDialogMessageBoxUIHandler>(handler => handler.HandleOpen(self.Message,
                    DialogMessageBoxBase.BoxType.Dialog, self._callback,
                    TeleportationText.Get("Confirm", "Cast"), TeleportationText.Get("Cancel", "Cancel"), null, null));
                self._opened = true;
                if (!surface.Shown || !self.OwnsCallback()) self.Cancel(false);
            }
            catch { self.Cancel(true); throw; }
        }
        // The Greater Teleport direct flow: no confirmation dialog is opened and
        // no confirmation surface is required. The cast revalidates and settles
        // synchronously through the same transaction, execution, reentrancy
        // (Pending) and announcement machinery as a confirmed cast.
        private static void OpenDirect(WorldMapPointSpellAction action, TeleportationWorldMapContext context, ITeleportationRolls qualificationRolls)
        {
            if (Pending || action == null || context == null || !context.Usable) return;
            // A direct cast never needs the confirmation presenter, but an
            // unrelated active modal still blocks it outright.
            if (TeleportationConfirmationSurface.UnrelatedModalShown()) return;
            if (action.Source.Kind == TeleportCastSourceKind.Scroll)
            {
                if (TeleportationScrollAdapter.Resolve(action.Source) == null) return;
            }
            else if (TeleportationSpellbookAdapter.Resolve(action.Source) == null) return;
            var host = new GameObject("KMG_TeleportDirectCast");
            host.SetActive(false);
            var self = host.AddComponent<TeleportContextConfirmationPresenter>();
            try
            {
                self._action = action;
                self._openedContext = context;
                self._familiarity = TeleportationCastExecution.FamiliarityFor(context, action.Destination.Id);
                self.Transaction = new TeleportCastTransaction(action);
                self.Execution = qualificationRolls == null ? new TeleportationCastExecution() : new TeleportationCastExecution(qualificationRolls);
                self._direct = true;
                _current = self;
                self.Settle();
            }
            finally
            {
                if (ReferenceEquals(_current, self)) _current = null;
                // Diagnostics only: the settled request's own objects, kept for
                // structured evidence the same way confirmed casts expose theirs.
                // Never a spell, ledger, or campaign source of truth.
                LastDirectCast = new TeleportDirectCastOutcome { Transaction = self.Transaction, Execution = self.Execution, Frame = Time.frameCount };
                UnityEngine.Object.Destroy(host);
            }
        }
        private bool OwnsCallback()
        { return _surface != null && _surface.Owns(_callback); }
        private bool StillValid()
        {
            var context = TeleportationWorldMapAdapter.Capture(false);
            if (_surface == null || !_surface.ControllerMatches || !context.Usable || context.OriginId != _action.OriginId ||
                !ReferenceEquals(context.Player, _openedContext.Player) || !ReferenceEquals(context.Map, _openedContext.Map) ||
                !ReferenceEquals(context.Rules, _openedContext.Rules)) return false;
            var point = ResourcesLibrary.TryGetBlueprint<BlueprintLocation>(_action.Destination.Id);
            var fresh = TeleportationWorldMapAdapter.Compose(context, point).SingleOrDefault(value => value.Key == _action.Key);
            var source = fresh == null ? null : TeleportationSpellbookAdapter.Resolve(fresh.Source);
            if (fresh != null && fresh.Source.Kind == TeleportCastSourceKind.Scroll)
            {
                // Inventory-backed sources stay valid while their shared stock is
                // unchanged; no physical book identity participates.
                return TeleportationScrollAdapter.Resolve(fresh.Source) != null &&
                    fresh.Source.Uses == _action.Source.Uses && fresh.Source.Kind == _action.Source.Kind &&
                    fresh.Source.SpellLevel == _action.Source.SpellLevel &&
                    fresh.Destination.OrdinaryArrivals == _action.Destination.OrdinaryArrivals &&
                    TeleportationCastExecution.FamiliarityFor(context, point.AssetGuid) == _familiarity;
            }
            return source != null && ReferenceEquals(source.Book, _openedBook) && fresh.Source.Uses == _action.Source.Uses && fresh.Source.Kind == _action.Source.Kind &&
                fresh.Source.SpellLevel == _action.Source.SpellLevel && fresh.Destination.OrdinaryArrivals == _action.Destination.OrdinaryArrivals &&
                TeleportationCastExecution.FamiliarityFor(context, point.AssetGuid) == _familiarity;
        }
        private void Update()
        {
            // The direct cast settles synchronously on an inactive self-owned
            // host; it owns no confirmation surface to watch.
            if (_direct || _settled) return;
            try
            {
                if (_surface == null || !_surface.Shown || !OwnsCallback()) Cancel(false);
                else
                {
                    DecorateConfirmationSections();
                    if (!_settled && !StillValid()) Cancel(true);
                }
            }
            catch (Exception exception) { Cancel(true); WorldMapPointSpellActionRuntime.Report(exception); }
        }        // Desktop confirmations read like native UI: the text itself is never
        // mutated — restrained hairline rules are placed in the measured gap
        // between the confirmation's factual groups. Console keeps its plain
        // single-string presentation.
        private void DecorateConfirmationSections()
        {
            if (_sectionsDecorated) return;
            if (_surface == null || _surface.Model != null) { _sectionsDecorated = true; return; }
            if (MessageLabelField == null)
            {
                // The native message-label contract differs: the confirmation
                // still works, only its section rules cannot be placed. Surface
                // that instead of silently claiming decoration.
                _sectionsDecorated = true;
                WorldMapPointSpellActionRuntime.Report(new MissingFieldException(typeof(DialogMessageBox).FullName, "m_Messagelabel"));
                return;
            }
            var box = DialogMessageBox.Instance;
            var label = box == null ? null : MessageLabelField.GetValue(box) as TextMeshProUGUI;
            if (label == null) return;
            if (!string.Equals(label.text, Message)) { RemoveSectionRules(); return; }
            TMP_TextInfo info = label.textInfo;
            if (info == null || info.characterInfo == null || info.characterInfo.Length == 0) return; // not rendered yet
            try
            {
                var sections = TeleportContextPresentation.ConfirmationSections(_action, _familiarity, TeleportationText.Get);
                int searched = 0;
                for (int index = 1; index < sections.Count; index++)
                {
                    int boundary = SectionBoundary(info, sections[index], ref searched);
                    if (boundary < 0) continue;
                    _sectionRules.Add(TeleportationUiDivider.CreateRule((RectTransform)label.transform,
                        "KMG_ConfirmSectionRule" + index.ToString(CultureInfo.InvariantCulture),
                        DividerY(info, boundary), label.color));
                }
                _sectionsDecorated = true;
            }
            catch (Exception exception) { _sectionsDecorated = true; RemoveSectionRules(); WorldMapPointSpellActionRuntime.Report(exception); }
        }
        // Locates the first rendered character of a confirmation group, anchored
        // at its exact position in the shown message.
        private int SectionBoundary(TMP_TextInfo info, string section, ref int searched)
        {
            int at = Message.IndexOf(section, searched, StringComparison.Ordinal);
            if (at < 0) return -1;
            searched = at + 1;
            for (int index = 0; index < info.characterInfo.Length; index++)
            {
                TMP_CharacterInfo character = info.characterInfo[index];
                if (character.index < at || !character.isVisible) continue;
                return index;
            }
            return -1;
        }
        private static float DividerY(TMP_TextInfo info, int boundary)
        {
            for (int index = boundary - 1; index >= 0; index--)
                if (info.characterInfo[index].isVisible)
                    return (info.characterInfo[index].bottomLeft.y + info.characterInfo[boundary].topLeft.y) * 0.5f;
            return info.characterInfo[boundary].topLeft.y + 8f;
        }
        private void RemoveSectionRules()
        {
            foreach (GameObject rule in _sectionRules) if (rule != null) Destroy(rule);
            _sectionRules.Clear();
        }
        private void Closed(DialogMessageBoxBase.BoxButton button)
        {
            if (_settled || !ReferenceEquals(_current, this)) return;
            try
            {
                if (button != DialogMessageBoxBase.BoxButton.Yes || !StillValid()) { Cancel(false); return; }
                Settle();
            }
            finally
            {
                RemoveSectionRules();
                if (ReferenceEquals(_current, this)) _current = null;
                Destroy(this);
            }
        }
        // Shared settlement for confirmed and direct casts: exactly-once
        // transaction commit through revalidation, then evidence recording and
        // the player-facing result (which may be suppressed on success).
        private void Settle()
        {
            _settled = true;
            Transaction.Confirm(Execution);
            Execution.RecordTransaction(_action, Transaction);
            try { PublishResult(); } catch (Exception exception) { WorldMapPointSpellActionRuntime.Report(exception); }
        }
        private void Cancel(bool closeOwnedDialog)
        {
            if (_settled) return;
            _settled = true;
            RemoveSectionRules();
            if (Transaction != null) Transaction.Cancel();
            if (closeOwnedDialog && _surface != null && _surface.Shown && OwnsCallback()) _surface.Close();
            if (ReferenceEquals(_current, this)) _current = null;
            Destroy(this);
        }
        private void OnDisable()
        { if (_opened && !_settled) Cancel(false); }
        private void OnDestroy()
        {
            RemoveSectionRules();
            if (!_settled && Transaction != null) Transaction.Cancel();
            if (ReferenceEquals(_current, this)) _current = null;
        }
        private void PublishResult()
        {
            string message;
            if (Transaction.Result != null)
            {
                var result = Transaction.Result;
                if (result.Status == TeleportExecutionStatus.NoLegalAlternate)
                    message = TeleportationText.Get("Result.NoAlternate", "Teleport failed: no legal alternate destination exists. The party remains at its origin. The spell use was consumed.");
                else if (result.Status == TeleportExecutionStatus.DefensiveMishapLimit)
                    message = TeleportationText.Get("Result.MishapLimit", "Teleport failed after repeated mishaps. The party remains at its origin. The spell use was consumed.");
                else if (TeleportContextPresentation.SuppressSuccessAnnouncement(_action.Source.Spell, result.Status))
                    // Structured diagnostics already recorded the verified arrival;
                    // only the player-facing success banner is suppressed.
                    message = null;
                else
                {
                    var destination = ResourcesLibrary.TryGetBlueprint<BlueprintLocation>(result.DestinationId);
                    string name = destination == null ? string.Empty : (string)destination.Name;
                    message = TeleportContextPresentation.ArrivalMessage(_action.Source.Spell, result.Outcome, name, TeleportationText.Get);
                }
            }
            else message = Transaction.State == TeleportTransactionState.ActivationRefused ?
                TeleportationText.Get("Result.ActivationRefused", "{0} could not activate the scroll. Nothing was consumed; you may try again or choose another reader.") :
                Transaction.State == TeleportTransactionState.ActivationFailedSpent ?
                TeleportationText.Get("Result.ActivationFailedSpent", "{0} failed to activate the scroll and it was consumed. No teleport occurred.") :
                Transaction.State == TeleportTransactionState.TechnicalFailureCompensated ?
                TeleportationText.Get("Result.Compensated", "The cast could not complete. Its exact spell use was restored.") :
                Transaction.State == TeleportTransactionState.TechnicalFailureSpent || Transaction.State == TeleportTransactionState.AmbiguousExpenditure ?
                TeleportationText.Get("Result.Uncertain", "The cast encountered a technical failure. Check the party and the selected spellbook; the spell use could not be safely restored.") :
                TeleportationText.Get("Result.Unspent", "The cast was canceled. No spell use was spent.");
            if (message != null) TeleportationCombatLog.Publish(message, Transaction.State);
        }
    }
}
