namespace KingmakerGunslinger.FeatureModules
{
    /// <summary>
    /// Player-readable lines for the settings panel. Unity Mod Manager marks a
    /// mod active as soon as its entry point returns, which happens before
    /// Kingmaker loads its blueprints; that light says nothing about whether
    /// the mod's content was registered. These lines report the content state
    /// itself. Object names and stack traces belong in the diagnostic log.
    /// </summary>
    internal static class ContentInitializationStatus
    {
        internal const string Initialized = "Content: initialized.";

        internal const string Pending =
            "Content: not initialized yet. Kingmaker has not finished loading its blueprints.";

        internal const string Failed =
            "Content: initialization failed. The mod is loaded, but its classes, items and spells are not available in this session. " +
            "See the Unity Mod Manager log for details (search for initialize.root-cause).";

        /// <summary>The loaded mod's own name and version, as its loader entry reports them.</summary>
        internal static string Header(string displayName, string version)
        {
            string name = string.IsNullOrWhiteSpace(displayName)
                ? "Tabletop Rules Expansion" : displayName.Trim();
            return string.IsNullOrWhiteSpace(version)
                ? name + " (version unknown)"
                : name + " " + version.Trim();
        }

        /// <summary>A failed initialization is never retried in the same process, so it outranks every other state.</summary>
        internal static string Describe(bool initialized, bool failed)
        {
            if (failed) return Failed;
            return initialized ? Initialized : Pending;
        }
    }
}
