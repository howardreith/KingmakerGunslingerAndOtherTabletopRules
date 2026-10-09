using System;
using System.Linq;
using Kingmaker;
using KingmakerGunslinger.Acquisition;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Development
{
    internal static partial class DevelopmentUi
    {
        private static int _recoverySelection;
        private static string _recoveryInspectedKey, _recoveryInspectedGameId;
        private static bool _recoveryAcknowledged;
        private static Kingmaker.EntitySystem.Entities.UnitEntityData _recoveryInspectedOwner;

        private static void DrawCampaignWeaponRecovery()
        {
            if (!BlueprintBootstrap.IsInitialized || Game.Instance == null || Game.Instance.Player == null ||
                Game.Instance.Player.MainCharacter.Value == null) return;
            var choices = CampaignWeaponRegistry.Read().Where(value => value.Relocated).ToArray();
            if (choices.Length == 0) return;
            _recoverySelection = Math.Max(0, Math.Min(_recoverySelection, choices.Length - 1));
            ImmediateModeGui.Label("Missed relocated campaign weapon recovery: " + choices[_recoverySelection].Item.Name);
            ImmediateModeGui.BeginHorizontal();
            if (ImmediateModeGui.Button("Previous recovery weapon"))
            { _recoverySelection = (_recoverySelection + choices.Length - 1) % choices.Length; ClearRecoveryInspection(); }
            if (ImmediateModeGui.Button("Next recovery weapon"))
            { _recoverySelection = (_recoverySelection + 1) % choices.Length; ClearRecoveryInspection(); }
            ImmediateModeGui.EndHorizontal();
            var selected = choices[_recoverySelection];
            if (ImmediateModeGui.Button("Inspect selected weapon before recovery"))
            {
                ClearRecoveryInspection();
                Run(() => {
                    var inspection = CampaignWeaponRecovery.Inspect(selected.Key);
                    if (inspection.CanRecover)
                    { _recoveryInspectedKey = selected.Key; _recoveryInspectedGameId = inspection.GameId;
                      _recoveryInspectedOwner = Game.Instance.Player.MainCharacter.Value; }
                    return DevelopmentActionResult.Success(inspection.Describe());
                });
            }
            if (_recoveryInspectedKey != selected.Key || _recoveryInspectedGameId != Game.Instance.Player.GameId ||
                !ReferenceEquals(_recoveryInspectedOwner, Game.Instance.Player.MainCharacter.Value)) return;
            ImmediateModeGui.Label(CampaignWeaponRecoveryPolicy.HistoricalUncertainty);
            _recoveryAcknowledged = ImmediateModeGui.Toggle(_recoveryAcknowledged,
                "I accept the unknown historical ownership and request one recovery of this selected weapon.");
            if (_recoveryAcknowledged && ImmediateModeGui.Button("Recover selected weapon once"))
            {
                string gameId = _recoveryInspectedGameId;
                ClearRecoveryInspection();
                Run(() => DevelopmentControls.RecoverCampaignWeapon(selected.Key, gameId, true));
            }
        }

        private static void ClearRecoveryInspection()
        { _recoveryInspectedKey = null; _recoveryInspectedGameId = null; _recoveryAcknowledged = false; _recoveryInspectedOwner = null; }
    }
}
