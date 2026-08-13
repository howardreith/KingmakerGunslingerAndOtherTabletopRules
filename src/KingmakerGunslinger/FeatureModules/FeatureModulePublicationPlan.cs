namespace KingmakerGunslinger.FeatureModules
{
    internal sealed class FeatureModulePublicationPlan
    {
        internal FeatureModulePublicationPlan(FeatureModuleConfiguration active)
        {
            GunslingerClass = active.Gunslinger;
            GunslingerFeats = active.Gunslinger;
            FirearmParameters = active.Gunslinger;
            CapitalGunslingerStock = active.Gunslinger;
            BeneathStolenLandsStock = active.Gunslinger;
            RareFirearmLoot = active.Gunslinger;
            AcadamaeFeat = active.AcadamaeGraduate;
            CordCapitalStock = active.AcadamaeGraduate;
            ShieldOtherSpellLists = active.ShieldOther;
            ExpandedSummoningParents = active.ExpandedSummoning;
            ElvenBranchedSpearSelectors = active.ElvenBranchedSpears;
            ElvenBranchedSpearCommerce = active.ElvenBranchedSpears;
            ElvenBranchedSpearPresentation = active.ElvenBranchedSpears;
        }
        internal bool GunslingerClass { get; private set; }
        internal bool GunslingerFeats { get; private set; }
        internal bool FirearmParameters { get; private set; }
        internal bool CapitalGunslingerStock { get; private set; }
        internal bool BeneathStolenLandsStock { get; private set; }
        internal bool RareFirearmLoot { get; private set; }
        internal bool AcadamaeFeat { get; private set; }
        internal bool CordCapitalStock { get; private set; }
        internal bool ShieldOtherSpellLists { get; private set; }
        internal bool ExpandedSummoningParents { get; private set; }
        internal bool ElvenBranchedSpearSelectors { get; private set; }
        internal bool ElvenBranchedSpearCommerce { get; private set; }
        internal bool ElvenBranchedSpearPresentation { get; private set; }
    }
}
