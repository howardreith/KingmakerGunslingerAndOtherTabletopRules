namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Stable firearm identity used by rules code. This identity is deliberately
    /// independent of any borrowed Kingmaker weapon category.
    /// </summary>
    public enum FirearmKind
    {
        Unknown = 0,
        Pistol = 1,
        Musket = 2,
        Blunderbuss = 3,
        Rifle = 4,
        Revolver = 5
    }
}
