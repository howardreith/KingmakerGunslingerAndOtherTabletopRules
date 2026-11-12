using System;

namespace KingmakerGunslinger.FeatureModules
{
    internal sealed class FeatureModuleSettingsState
    {
        internal FeatureModuleSettingsState(FeatureModuleConfiguration active,
            string path, string source, bool recovered,
            bool showOriginalSummonElemental = false)
        {
            ShowOriginalSummonElemental = showOriginalSummonElemental;
            Active = active ?? throw new ArgumentNullException("active");
            Pending = new FeatureModuleConfiguration(active.Gunslinger,
                active.AcadamaeGraduate, active.ShieldOther,
                active.ExpandedSummoning, active.ElvenBranchedSpears,
                active.EasternWeapons, active.BrownFurTransmuter,
                active.UrbanBarbarian, active.BodyguardFeats,
                active.ProtectionFromAlignmentControlImmunity,
                active.ElementalRaces, active.TeleportationSpells, active.MagicCircleSpells);
            Path = path ?? string.Empty;
            Source = source ?? string.Empty;
            Recovered = recovered;
        }

        internal event Action ElementalCharacterTraitsChanged;
        internal FeatureModuleConfiguration Active { get; private set; }
        internal FeatureModuleConfiguration Pending { get; private set; }
        internal string Path { get; private set; }
        internal string Source { get; private set; }
        internal bool Recovered { get; private set; }
        internal bool RestartRequired { get { return !Active.Equals(Pending); } }

        // Presentation option, not a feature module: no restart; close and
        // reopen the spell list to apply. Persisted on save and never part of
        // restart decisions.
        internal bool ShowOriginalSummonElemental { get; private set; }

        internal void SetShowOriginalSummonElemental(bool value)
        { ShowOriginalSummonElemental = value; }

        internal void SetPending(bool gunslinger, bool acadamaeGraduate,
            bool shieldOther, bool expandedSummoning, bool elvenBranchedSpears,
            bool easternWeapons, bool brownFurTransmuter, bool urbanBarbarian,
            bool bodyguardFeats, bool protectionFromAlignmentControlImmunity,
            bool elementalRaces, bool teleportationSpells, bool magicCircleSpells = true)
        {
            bool traitsChanged=Pending.ElementalRaces!=elementalRaces;
            Pending = new FeatureModuleConfiguration(gunslinger, acadamaeGraduate,
                shieldOther, expandedSummoning, elvenBranchedSpears, easternWeapons,
                brownFurTransmuter, urbanBarbarian, bodyguardFeats,
                protectionFromAlignmentControlImmunity, elementalRaces, teleportationSpells, magicCircleSpells);
            if(traitsChanged) ElementalCharacterTraitsChanged?.Invoke();
        }
    }
}
