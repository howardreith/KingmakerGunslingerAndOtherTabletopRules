namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Resolves an arbitrary runtime candidate to one exact firearm item. A resolver
    /// must reject blueprints, wrappers, native crossbows, and ambiguous markers.
    /// </summary>
    internal interface IFirearmRuntimeItemResolver
    {
        bool TryResolve(
            object candidate,
            out ResolvedFirearmItem firearm,
            out string rejectionReason);
    }
}
