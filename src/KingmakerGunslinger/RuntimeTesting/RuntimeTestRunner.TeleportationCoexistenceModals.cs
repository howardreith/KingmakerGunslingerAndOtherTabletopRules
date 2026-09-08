using System;
using System.Collections.Generic;
using Kingmaker;
using Kingmaker.Globalmap;
using Kingmaker.PubSubSystem;
using Kingmaker.UI;
using Kingmaker.UI.GlobalMap;
using Kingmaker.UI._ConsoleUI.GlobalMap;
using KingmakerGunslinger.Spells.Teleportation;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> RunTeleportCoexistenceModals(GlobalMapMessageBox desktop, GlobalMapMessageBoxView pad,
            GlobalMapLocation target, TeleportCoexistenceForeignAction foreign, Func<string> resources)
        {
            bool console = pad != null; string before = resources();
            foreach (string mode in new[] { "escape", "force-close", "replacement" })
            {
                var cast = console ? OpenTeleportGamepadSpell(target, TeleportSpellKind.Teleport, TeleportCastSourceKind.Prepared,
                    new TeleportationFixtureRolls(new[] { 1 })) : OpenTeleportationFixtureConfirmation(desktop, target,
                    TeleportSpellKind.Teleport, TeleportCastSourceKind.Prepared, new TeleportationFixtureRolls(new[] { 1 }));
                if (console) { foreach (int tick in WaitTeleportGamepadModal()) yield return tick; }
                else { for (int frame = 0; frame < 6; frame++) yield return 0; }
                TeleportInteractionAssert("foreign-modal-open-" + mode, "Opening the native spell confirmation preserves the independent foreign control and callback",
                    "state=" + cast.Transaction.State, cast.Transaction.State == TeleportTransactionState.Pending && before == resources() &&
                    foreign.ControlMatches() && foreign.NavigationMatches(false) && foreign.Fires == 1);
                int replacementCalls = 0;
                Action<DialogMessageBoxBase.BoxButton> replacement = button => replacementCalls++;
                if (mode == "escape")
                { if (console) InvokeTeleportGamepadInput(TeleportGamepadDialog(), "OnDeclineClicked"); else Game.Instance.UI.EscManager.OnEscPressed(); }
                else if (mode == "force-close")
                { if (console) TeleportGamepadDialogModel().ForceHide(); else DialogMessageBox.Instance.HandleForceClose(); }
                else
                {
                    if (!console) DialogMessageBox.Instance.HandleForceClose();
                    EventBus.RaiseEvent<IDialogMessageBoxUIHandler>(handler => handler.HandleOpen("Guarded foreign replacement modal",
                        DialogMessageBoxBase.BoxType.Dialog, replacement, "OK", "Cancel", null, null));
                }
                for (int frame = 0; frame < 6; frame++) yield return 0;
                bool replacementRetained = mode != "replacement" || (console ? TeleportGamepadDialogModel() != null &&
                    ReferenceEquals(TeleportationConfirmationSurface.ConsoleCallback.GetValue(TeleportGamepadDialogModel()), replacement) :
                    DialogMessageBox.Instance.IsShown && ReferenceEquals(WorldMapPointSpellActionPatches.ConfirmationCallbackField.GetValue(DialogMessageBox.Instance), replacement));
                TeleportInteractionAssert("foreign-modal-close-" + mode, "Escape/ForceClose/replacement cancels KMG only; no source debit, foreign mutation or replacement callback invocation",
                    "state=" + cast.Transaction.State, cast.Transaction.State == TeleportTransactionState.Cancelled && !TeleportContextConfirmationPresenter.Pending &&
                    before == resources() && foreign.ControlMatches() && foreign.NavigationMatches(false) && foreign.Fires == 1 && replacementCalls == 0 && replacementRetained);
                CaptureTeleportInteraction("foreign-modal-" + mode, new { foreign = foreign.Evidence(), replacementCalls, replacementRetained, state = cast.Transaction.State.ToString() });
                if (mode == "replacement")
                {
                    if (console) InvokeTeleportGamepadInput(TeleportGamepadDialog(), "OnDeclineClicked");
                    else TeleportationFixtureDialogButton("m_ButtonNo").onClick.Invoke();
                    TeleportInteractionAssert("foreign-replacement-callback-once", "The replacement's original native callback fires once on its own dismissal",
                        "calls=" + replacementCalls, replacementCalls == 1 && before == resources() && foreign.ControlMatches());
                }
            }
        }
    }
}
