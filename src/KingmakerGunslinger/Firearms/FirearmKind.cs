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

    /// <summary>
    /// Separates the current player-facing firearm catalog from stable legacy
    /// identities that remain readable for existing saves and deliberate
    /// out-of-band spawning.
    /// </summary>
    internal static class OfficialFirearmSupport
    {
        private static readonly FirearmKind[] Official =
        {
            FirearmKind.Pistol,
            FirearmKind.Musket,
            FirearmKind.Blunderbuss
        };

        private static readonly FirearmKind[] Recognized =
        {
            FirearmKind.Pistol,
            FirearmKind.Musket,
            FirearmKind.Blunderbuss,
            FirearmKind.Rifle,
            FirearmKind.Revolver
        };

        internal static FirearmKind[] Kinds
        {
            get { return (FirearmKind[])Official.Clone(); }
        }

        internal static FirearmKind[] RecognizedKinds
        {
            get { return (FirearmKind[])Recognized.Clone(); }
        }

        internal static bool IsOfficial(FirearmKind kind)
        {
            return kind == FirearmKind.Pistol || kind == FirearmKind.Musket ||
                kind == FirearmKind.Blunderbuss;
        }

        internal static bool IsLegacy(FirearmKind kind)
        {
            return kind == FirearmKind.Rifle || kind == FirearmKind.Revolver;
        }

        internal static bool IsRecognized(FirearmKind kind)
        {
            return IsOfficial(kind) || IsLegacy(kind);
        }
    }
}
