namespace KingmakerGunslinger.Scatter
{
    /// <summary>
    /// Result of the separate engine-facing cone geometry evaluation. Unknown is
    /// deliberately distinct from Outside so missing geometry cannot silently
    /// become a valid or harmless target plan.
    /// </summary>
    internal enum ScatterGeometryDisposition
    {
        Unknown = 0,
        Inside = 1,
        Outside = 2
    }
}
