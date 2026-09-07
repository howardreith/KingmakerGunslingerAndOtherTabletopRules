namespace KingmakerGunslinger.Spells.Teleportation
{
    // Values define contextual action order after the unchanged native actions.
    internal enum TeleportSpellKind { WordOfRecall = 0, GreaterTeleport = 1, Teleport = 2 }
    internal enum TeleportFamiliarity { Unvisited = 0, ViewedOnce = 1, SeenCasually = 2,
        StudiedCarefully = 3, VeryFamiliar = 4 }
    internal enum TeleportOutcomeKind { OnTarget, OffTarget, SimilarLocation, Mishap }
}
