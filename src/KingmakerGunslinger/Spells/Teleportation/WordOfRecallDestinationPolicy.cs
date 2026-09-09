namespace KingmakerGunslinger.Spells.Teleportation
{
    internal static class WordOfRecallDestinationPolicy
    {
        internal const string OlegId = "758559f44d15fc844bf30a10a83154d5";
        internal const string CapitalId = "f83de5c382e087b4ab6ce0b7397a2a13";
        internal static string Resolve(bool capitalEstablished, string nativeCapitalId)
        {
            if (!capitalEstablished) return OlegId;
            return nativeCapitalId == CapitalId ? CapitalId : null;
        }
        internal static bool Matches(string selectedId, bool capitalEstablished, string nativeCapitalId)
        {
            string required = Resolve(capitalEstablished, nativeCapitalId);
            return required != null && selectedId == required;
        }
    }
}
