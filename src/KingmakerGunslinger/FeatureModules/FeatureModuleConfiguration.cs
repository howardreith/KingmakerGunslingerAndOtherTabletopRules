using System;

namespace KingmakerGunslinger.FeatureModules
{
    internal sealed class FeatureModuleConfiguration : IEquatable<FeatureModuleConfiguration>
    {
        internal const string GunslingerId = "gunslinger";
        internal const string AcadamaeGraduateId = "acadamae-graduate";
        internal const string ShieldOtherId = "shield-other";

        internal FeatureModuleConfiguration(bool gunslinger, bool acadamaeGraduate,
            bool shieldOther)
        {
            Gunslinger = gunslinger;
            AcadamaeGraduate = acadamaeGraduate;
            ShieldOther = shieldOther;
        }

        internal bool Gunslinger { get; private set; }
        internal bool AcadamaeGraduate { get; private set; }
        internal bool ShieldOther { get; private set; }
        internal static FeatureModuleConfiguration Defaults
        { get { return new FeatureModuleConfiguration(true, true, true); } }

        public bool Equals(FeatureModuleConfiguration other)
        {
            return other != null && Gunslinger == other.Gunslinger &&
                AcadamaeGraduate == other.AcadamaeGraduate &&
                ShieldOther == other.ShieldOther;
        }

        public override bool Equals(object obj)
        { return Equals(obj as FeatureModuleConfiguration); }

        public override int GetHashCode()
        { return (Gunslinger ? 1 : 0) | (AcadamaeGraduate ? 2 : 0) |
            (ShieldOther ? 4 : 0); }

        public override string ToString()
        { return "gunslinger=" + Gunslinger + ";acadamae-graduate=" +
            AcadamaeGraduate + ";shield-other=" + ShieldOther; }
    }
}
