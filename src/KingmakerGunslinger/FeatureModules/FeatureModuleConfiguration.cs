using System;

namespace KingmakerGunslinger.FeatureModules
{
    internal sealed class FeatureModuleConfiguration : IEquatable<FeatureModuleConfiguration>
    {
        internal const string GunslingerId = "gunslinger";
        internal const string AcadamaeGraduateId = "acadamae-graduate";
        internal const string ShieldOtherId = "shield-other";
        internal const string ExpandedSummoningId = "expanded-summoning";
        internal const string ElvenBranchedSpearsId = "elven-branched-spears";
        internal const string EasternWeaponsId = "eastern-weapons";
        internal const string BrownFurTransmuterId = "brown-fur-transmuter";
        internal const string UrbanBarbarianId = "urban-barbarian";

        internal FeatureModuleConfiguration(bool gunslinger, bool acadamaeGraduate,
            bool shieldOther, bool expandedSummoning, bool elvenBranchedSpears,
            bool easternWeapons, bool brownFurTransmuter, bool urbanBarbarian)
        {
            Gunslinger = gunslinger;
            AcadamaeGraduate = acadamaeGraduate;
            ShieldOther = shieldOther;
            ExpandedSummoning = expandedSummoning;
            ElvenBranchedSpears = elvenBranchedSpears;
            EasternWeapons = easternWeapons;
            BrownFurTransmuter = brownFurTransmuter;
            UrbanBarbarian = urbanBarbarian;
        }

        internal bool Gunslinger { get; private set; }
        internal bool AcadamaeGraduate { get; private set; }
        internal bool ShieldOther { get; private set; }
        internal bool ExpandedSummoning { get; private set; }
        internal bool ElvenBranchedSpears { get; private set; }
        internal bool EasternWeapons { get; private set; }
        internal bool BrownFurTransmuter { get; private set; }
        internal bool UrbanBarbarian { get; private set; }
        internal static FeatureModuleConfiguration Defaults
        { get { return new FeatureModuleConfiguration(true, true, true, true, true, true,
            true, true); } }

        public bool Equals(FeatureModuleConfiguration other)
        {
            return other != null && Gunslinger == other.Gunslinger &&
                AcadamaeGraduate == other.AcadamaeGraduate &&
                ShieldOther == other.ShieldOther &&
                ExpandedSummoning == other.ExpandedSummoning &&
                ElvenBranchedSpears == other.ElvenBranchedSpears &&
                EasternWeapons == other.EasternWeapons &&
                BrownFurTransmuter == other.BrownFurTransmuter &&
                UrbanBarbarian == other.UrbanBarbarian;
        }

        public override bool Equals(object obj)
        { return Equals(obj as FeatureModuleConfiguration); }

        public override int GetHashCode()
        { return (Gunslinger ? 1 : 0) | (AcadamaeGraduate ? 2 : 0) |
            (ShieldOther ? 4 : 0) | (ExpandedSummoning ? 8 : 0) |
            (ElvenBranchedSpears ? 16 : 0) | (EasternWeapons ? 32 : 0) |
            (BrownFurTransmuter ? 64 : 0) | (UrbanBarbarian ? 128 : 0); }

        public override string ToString()
        { return "gunslinger=" + Gunslinger + ";acadamae-graduate=" +
            AcadamaeGraduate + ";shield-other=" + ShieldOther +
            ";expanded-summoning=" + ExpandedSummoning +
            ";elven-branched-spears=" + ElvenBranchedSpears +
            ";eastern-weapons=" + EasternWeapons +
            ";brown-fur-transmuter=" + BrownFurTransmuter +
            ";urban-barbarian=" + UrbanBarbarian; }
    }
}
