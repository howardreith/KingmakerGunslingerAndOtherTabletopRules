namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Rules generation used by a firearm definition. Unknown is a serialization
    /// sentinel and is rejected by <see cref="FirearmDefinition"/>.
    /// </summary>
    internal enum FirearmEra
    {
        Unknown = 0,
        Early = 1,
        Advanced = 2
    }
}
