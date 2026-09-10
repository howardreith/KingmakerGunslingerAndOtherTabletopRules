using System;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.UI;
using UnityEngine;

namespace KingmakerGunslinger.Spells.Teleportation
{
    // A component only for the lifetime of one native confirmation. The static
    // reference is an overlap guard, never a spell/ledger/campaign source of truth.
    internal sealed class TeleportContextConfirmationPresenter : MonoBehaviour
    {
        private static TeleportContextConfirmationPresenter _current;
        private TeleportationConfirmationSurface _surface;
        private bool _opened;
        private WorldMapPointSpellAction _action;
        private TeleportationWorldMapContext _openedContext;
        private Kingmaker.UnitLogic.Spellbook _openedBook;
        private Action<DialogMessageBoxBase.BoxButton> _callback;
        private TeleportFamiliarity _familiarity;
        private bool _settled;
        internal TeleportCastTransaction Transaction { get; private set; }
        internal TeleportationCastExecution Execution { get; private set; }
        internal string Message { get; private set; }
        internal static bool Pending { get { return _current != null; } }
        internal static TeleportContextConfirmationPresenter Current { get { return _current; } }
        internal static bool CanOpen { get { return !Pending && TeleportationConfirmationSurface.Available() != null; } }

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
            if (_settled) return;
            try
            {
                if (_surface == null || !_surface.Shown || !OwnsCallback()) Cancel(false);
                else if (!StillValid()) Cancel(true);
            }
            catch (Exception exception) { Cancel(true); WorldMapPointSpellActionRuntime.Report(exception); }
        }
        private void Closed(DialogMessageBoxBase.BoxButton button)
        {
            if (_settled || !ReferenceEquals(_current, this)) return;
            try
            {
                if (button != DialogMessageBoxBase.BoxButton.Yes || !StillValid()) { Cancel(false); return; }
                _settled = true;
                Transaction.Confirm(Execution);
                Execution.RecordTransaction(_action, Transaction);
                try { PublishResult(); } catch (Exception exception) { WorldMapPointSpellActionRuntime.Report(exception); }
            }
            finally
            {
                if (ReferenceEquals(_current, this)) _current = null;
                Destroy(this);
            }
        }
        private void Cancel(bool closeOwnedDialog)
        {
            if (_settled) return;
            _settled = true;
            if (Transaction != null) Transaction.Cancel();
            if (closeOwnedDialog && _surface != null && _surface.Shown && OwnsCallback()) _surface.Close();
            if (ReferenceEquals(_current, this)) _current = null;
            Destroy(this);
        }
        private void OnDisable()
        { if (_opened && !_settled) Cancel(false); }
        private void OnDestroy()
        {
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
                else
                {
                    var destination = ResourcesLibrary.TryGetBlueprint<BlueprintLocation>(result.DestinationId);
                    string name = destination == null ? string.Empty : (string)destination.Name;
                    if (string.IsNullOrWhiteSpace(name)) name = TeleportationText.Get("Result.UnnamedPoint", "a previously visited world-map point");
                    string outcome = result.Outcome == TeleportOutcomeKind.OffTarget ? TeleportationText.Get("Result.OffTarget", "Off target") :
                        result.Outcome == TeleportOutcomeKind.SimilarLocation ? TeleportationText.Get("Result.Similar", "Similar location") :
                        TeleportationText.Get("Result.OnTarget", "On target");
                    message = string.Format(TeleportationText.Get("Result.Arrived", "{0}: {1}. The party arrived at {2}."),
                        TeleportContextPresentation.SpellName(_action.Source.Spell, TeleportationText.Get), outcome, name);
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
            TeleportationCombatLog.Publish(message, Transaction.State);
        }
    }
}
