using System;
using KingmakerGunslinger.Development;
using KingmakerGunslinger.UrbanBarbarian;
using KingmakerGunslinger.AidAnotherCompatibility;
using KingmakerGunslinger.CraftMagicItemsCompatibility;
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
            bool urbanBarbarian = ImmediateModeGui.Toggle(
                _state.Pending.UrbanBarbarian, "Urban Barbarian");
            bool bodyguardFeats = ImmediateModeGui.Toggle(
                _state.Pending.BodyguardFeats,
                "Bodyguard, In Harms Way, and Helpful");
            bool protectionFromAlignmentControlImmunity = ImmediateModeGui.Toggle(
                _state.Pending.ProtectionFromAlignmentControlImmunity,
                "Protection from Alignment: control immunity");
            bool elementalRaces = ImmediateModeGui.Toggle(
                _state.Pending.ElementalRaces,
                "Elemental Races: Ifrit, Oread, Sylph, and Undine (preview)");
            _state.SetPending(gunslinger, acadamae, shieldOther, expandedSummoning,
                elvenBranchedSpears, easternWeapons, brownFurTransmuter,
                urbanBarbarian, bodyguardFeats,
                protectionFromAlignmentControlImmunity, elementalRaces);
            BrownFurFeatureStatus brownFurStatus =
                BrownFurFeatureStatusRegistry.Current;
            ImmediateModeGui.Label("Brown-Fur dependency: " +
                brownFurStatus.DependencyStatus);
            ImmediateModeGui.Label("Brown-Fur effective current-process state: " +
                brownFurStatus.PublicationStatus);
            UrbanCotwCompatibilityDecision urbanCotw =
                UrbanCotwCompatibilityStatusRegistry.Current;
            ImmediateModeGui.Label("Urban Barbarian core: " +
                urbanCotw.CoreStatus);
            ImmediateModeGui.Label("Urban Barbarian optional CotW interoperability: " +
                urbanCotw.InteroperabilityStatus);
            ImmediateModeGui.Label("Urban Barbarian CotW detail: " +
                urbanCotw.Diagnostic);
            AidAnotherCompatibilityStatus aidAnother =
                AidAnotherCompatibilityStatusRegistry.Current;
            ImmediateModeGui.Label("Aid Another compatibility: " +
                aidAnother.CotwStatus);
            ImmediateModeGui.Label("Aid Another traits: " +
                aidAnother.FavoredClassStatus);
            ImmediateModeGui.Label("Helpful publication: " +
                aidAnother.PublicationStatus);
            ImmediateModeGui.Label("Aid Another detail: " + aidAnother.Detail);
            ImmediateModeGui.Label("Craft Magic Items compatibility: " +
                CraftMagicItemsCompatibilityStatusRegistry.Current.Display);
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
