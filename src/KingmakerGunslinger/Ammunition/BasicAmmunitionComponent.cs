namespace KingmakerGunslinger.Ammunition
{
    /// <summary>
    /// The two inventory components required by the first early-firearm load.
    /// This is deliberately distinct from AmmunitionId, which describes the
    /// projectile stored inside an individual loaded firearm.
    /// </summary>
    internal enum BasicAmmunitionComponent
    {
        BlackPowderCharge = 1,
        LeadBall = 2
    }
}
