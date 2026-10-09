namespace KingmakerGunslinger.Acquisition
{
    internal enum WeaponEvidenceState { Unverified, Pass, Fail }

    internal sealed class CampaignWeaponQualification
    {
        internal WeaponEvidenceState BlueprintIdentity { get; set; }
        internal WeaponEvidenceState NativeTreasure { get; set; }
        internal WeaponEvidenceState ScenePresence { get; set; }
        internal WeaponEvidenceState NormalRoute { get; set; }
        internal WeaponEvidenceState NormalPickup { get; set; }
        internal WeaponEvidenceState Revisit { get; set; }
        internal WeaponEvidenceState SaveReload { get; set; }
        internal bool IsPhysicallyQualified
        {
            get
            {
                return BlueprintIdentity == WeaponEvidenceState.Pass && NativeTreasure == WeaponEvidenceState.Pass &&
                    ScenePresence == WeaponEvidenceState.Pass && NormalRoute == WeaponEvidenceState.Pass &&
                    NormalPickup == WeaponEvidenceState.Pass && Revisit == WeaponEvidenceState.Pass && SaveReload == WeaponEvidenceState.Pass;
            }
        }
    }
}
