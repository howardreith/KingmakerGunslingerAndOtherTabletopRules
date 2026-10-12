namespace KingmakerGunslinger.RuntimeTesting
{
    // Admission to a save-free first-level fixture must never replace a
    // campaign, import a save or leave its game process open.
    public static class ExpandedSummoningNativeProfilePolicy
    {
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
