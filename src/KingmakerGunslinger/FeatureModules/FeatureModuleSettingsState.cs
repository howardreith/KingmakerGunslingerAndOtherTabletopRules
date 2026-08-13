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
                active.ExpandedSummoning, active.ElvenBranchedSpears);
            Path = path ?? string.Empty;
            Source = source ?? string.Empty;
            Recovered = recovered;
        }

        internal FeatureModuleConfiguration Active { get; private set; }
        internal FeatureModuleConfiguration Pending { get; private set; }
        internal string Path { get; private set; }
        internal string Source { get; private set; }
        internal bool Recovered { get; private set; }
        internal bool RestartRequired { get { return !Active.Equals(Pending); } }

        internal void SetPending(bool gunslinger, bool acadamaeGraduate,
            bool shieldOther, bool expandedSummoning, bool elvenBranchedSpears)
        { Pending = new FeatureModuleConfiguration(gunslinger, acadamaeGraduate,
            shieldOther, expandedSummoning, elvenBranchedSpears); }
    }
}
