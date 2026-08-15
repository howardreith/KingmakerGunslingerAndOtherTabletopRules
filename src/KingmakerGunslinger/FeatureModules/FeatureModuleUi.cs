using System;
using KingmakerGunslinger.Development;
using UnityModManagerNet;

namespace KingmakerGunslinger.FeatureModules
{
    internal static class FeatureModuleUi
    {
        private static FeatureModuleSettingsState _state;

        internal static void Attach(UnityModManager.ModEntry entry,
            FeatureModuleSettingsState state)
        {
            if (entry == null) throw new ArgumentNullException("entry");
            _state = state ?? throw new ArgumentNullException("state");
            entry.OnGUI = Draw;
            entry.OnSaveGUI = Save;
        }

        private static void Draw(UnityModManager.ModEntry entry)
        {
            ImmediateModeGui.Label("Feature modules (changes take effect after a complete Kingmaker restart)");
            bool gunslinger = ImmediateModeGui.Toggle(_state.Pending.Gunslinger,
                "Gunslinger");
            bool acadamae = ImmediateModeGui.Toggle(_state.Pending.AcadamaeGraduate,
                "Acadamae Graduate and Cord of Stubborn Resolve");
            bool shieldOther = ImmediateModeGui.Toggle(_state.Pending.ShieldOther,
                "Shield Other");
            bool expandedSummoning = ImmediateModeGui.Toggle(
                _state.Pending.ExpandedSummoning, "Expanded Summoning");
            bool elvenBranchedSpears = ImmediateModeGui.Toggle(
                _state.Pending.ElvenBranchedSpears, "Elven Branched Spears");
            bool easternWeapons = ImmediateModeGui.Toggle(
                _state.Pending.EasternWeapons, "Eastern Weapons");
            bool brownFurTransmuter = ImmediateModeGui.Toggle(
                _state.Pending.BrownFurTransmuter,
                "Brown-Fur Transmuter  requires Call of the Wild");
            _state.SetPending(gunslinger, acadamae, shieldOther, expandedSummoning,
                elvenBranchedSpears, easternWeapons, brownFurTransmuter);
            BrownFurFeatureStatus brownFurStatus =
                BrownFurFeatureStatusRegistry.Current;
            ImmediateModeGui.Label("Brown-Fur dependency: " +
                brownFurStatus.DependencyStatus);
            ImmediateModeGui.Label("Brown-Fur effective current-process state: " +
                brownFurStatus.PublicationStatus);
            ImmediateModeGui.Label("Active this process: " + _state.Active);
            ImmediateModeGui.Label("Saved for next restart: " + _state.Pending);
            if (_state.RestartRequired)
                ImmediateModeGui.Label("RESTART REQUIRED: saved module choices do not change this process.");
            ImmediateModeGui.Space(10f);
            DevelopmentUi.Draw(entry);
        }

        private static void Save(UnityModManager.ModEntry entry)
        { FeatureModuleSettingsStore.Save(_state); }
    }
}
