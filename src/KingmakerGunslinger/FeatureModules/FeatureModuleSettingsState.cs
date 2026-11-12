using System;

namespace KingmakerGunslinger.FeatureModules
{
    internal sealed class FeatureModuleSettingsState
    {
        internal FeatureModuleSettingsState(FeatureModuleConfiguration active,
            string path, string source, bool recovered)
        {
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
        /// <summary>
        /// True when this state came from a valid settings file whose schema
        /// migration could not be WRITTEN back. The parsed values are still in
        /// effect and the original file is unchanged; only persistence failed.
        /// This is deliberately distinct from <see cref="Recovered"/>, which
        /// means the file was malformed and defaults were substituted.
        /// </summary>
        internal bool MigrationWriteFailed { get; private set; }

        internal void MarkMigrationWriteFailed() { MigrationWriteFailed = true; }
        internal bool RestartRequired { get { return !Active.Equals(Pending); } }

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
