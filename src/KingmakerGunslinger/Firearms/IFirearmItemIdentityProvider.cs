namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Resolves the identity that Kingmaker itself assigned to a concrete item.
    /// Implementations must fail closed and must never generate or write an identity.
    /// </summary>
    internal interface IFirearmItemIdentityProvider
    {
        bool TryGetIdentity(
            object itemInstance,
            out FirearmItemId identity,
            out string rejectionReason);
    }
}
