namespace KingmakerGunslinger.RuntimeTesting
{
    // Admission to a save-free first-level fixture must never replace a
    // campaign, import a save or leave its game process open.
    public static class ExpandedSummoningNativeProfilePolicy
    {
        public static bool ReadyForWorldLoad(bool entryCallback, bool registeredBaseScene, bool nativeLoadingOwnsEntry)
        {
            return entryCallback && registeredBaseScene && nativeLoadingOwnsEntry;
        }
        public static bool StartupComplete(int nativeStarters, bool progressPresent, bool progressActive)
        {
            // Native GameStarter hides this object only at its startup epilogue.
            return nativeStarters == 1 && progressPresent && !progressActive;
        }
        public static bool ReadyForMechanics(bool baseMechanicsLoaded, bool exactArea,
            bool loadedAreaState, bool mainCharacter, bool partyWithHoldingState)
        {
            return baseMechanicsLoaded && exactArea && loadedAreaState && mainCharacter && partyWithHoldingState;
        }
        public static bool CanStart(bool automaticExit, int parameters,
            int worldUnits, int partyCharacters, int crossSceneEntities,
            bool loadedArea, bool mainCharacter, bool importSave,
            bool pendingNewGameUnit, bool idleSubscribedMenu)
        {
            return automaticExit && parameters == 0 && worldUnits == 0 &&
                partyCharacters == 0 && crossSceneEntities == 0 && !loadedArea &&
                !mainCharacter && !importSave && !pendingNewGameUnit && idleSubscribedMenu;
        }
    }
}
