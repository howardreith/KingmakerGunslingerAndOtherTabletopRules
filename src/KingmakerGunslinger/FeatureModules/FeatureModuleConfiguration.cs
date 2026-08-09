using System;

namespace KingmakerGunslinger.FeatureModules
{
    internal sealed class FeatureModuleConfiguration : IEquatable<FeatureModuleConfiguration>
    {
        internal const string GunslingerId = "gunslinger";
        internal const string AcadamaeGraduateId = "acadamae-graduate";

        internal FeatureModuleConfiguration(bool gunslinger, bool acadamaeGraduate)
        {
            Gunslinger = gunslinger;
            AcadamaeGraduate = acadamaeGraduate;
        }

        internal bool Gunslinger { get; private set; }
        internal bool AcadamaeGraduate { get; private set; }
        internal static FeatureModuleConfiguration Defaults
        { get { return new FeatureModuleConfiguration(true, true); } }

        public bool Equals(FeatureModuleConfiguration other)
        {
            return other != null && Gunslinger == other.Gunslinger &&
                AcadamaeGraduate == other.AcadamaeGraduate;
        }

        public override bool Equals(object obj)
        { return Equals(obj as FeatureModuleConfiguration); }

        public override int GetHashCode()
        { return (Gunslinger ? 1 : 0) | (AcadamaeGraduate ? 2 : 0); }

        public override string ToString()
        { return "gunslinger=" + Gunslinger + ";acadamae-graduate=" + AcadamaeGraduate; }
    }
}
