namespace KingmakerGunslinger.RuntimeTesting
{
    // Admission to a save-free first-level fixture must never replace a
    // campaign, import a save or leave its game process open.
    public static class ExpandedSummoningNativeProfilePolicy
    {
        internal static bool Sprint25Phase(string phase)
        {
            return phase == "rtwp-foundation" || phase == "turn-based-commands" ||
                phase == "rtwp-ai" || phase == "routes" || phase == "visual-lifecycle" ||
                phase == "overrun" || phase == "great-cleave";
        }
        internal static bool Sprint28Phase(string phase)
        {
            return phase == "profiles" || phase == "modes" ||
                phase == "commands" || phase == "ownership" ||
                phase == "catch";
        }
        internal static bool Sprint27Phase(string phase)
        {
            return phase == "rtwp-foundation" || phase == "pounce-rtwp" ||
                phase == "pounce-turn-based" || phase == "turn-based-abort" || phase == "turn-based-target-loss" || phase == "rtwp-ai" || phase == "charge-boundaries" || phase == "visual-lifecycle" || phase == "routes" ||
                phase == "outdoor-locomotion" || phase == "shared-controls" || phase == "expiry-boundary" || phase == "qualification-combat" || phase == "qualification-presentation";
        }
        internal static bool Sprint26Phase(string phase)
        {
            return phase == "rtwp-foundation" ||
                phase == "turn-based-commands" ||
                phase == "gorgon-breath-rtwp" ||
                phase == "gorgon-breath-turn-based" ||
                phase == "bulette-leap-rtwp" ||
                phase == "bulette-leap-turn-based" ||
                phase == "rtwp-ai" ||
                phase == "routes" ||
                phase == "visual-lifecycle";
        }

        public static bool BiteArmorControl(bool biteWeapon, bool firstHit,
            bool secondHit, bool dismantled, bool exactArmorUnchanged)
        {
            // Sprint 21 requires both claws. Two bites are a negative control.
            return biteWeapon && firstHit && secondHit && !dismantled && exactArmorUnchanged;
        }
        public static bool ContactAttemptExposed(bool grappleAttempt, bool nativeHoldPrevented,
            bool reaction, bool exactDc, bool exactSource)
        {
            // AutoFailure skips native maneuver execution. Its uninitialized
            // Success getter can still report 0 >= 0; that is not a held grapple.
            return grappleAttempt && nativeHoldPrevented && reaction && exactDc && exactSource;
        }
        public static bool OneOutcomePerEligibleView(int observed, int eligible, int attached,
            int outcomesBefore, int outcomesAfter)
        {
            // The shared observation ring is bounded at 256. Refuse saturation
            // rather than treating its count as a monotonic event counter.
            return eligible > 0 && observed == eligible && attached == eligible &&
                outcomesBefore >= 0 && outcomesAfter < 256;
        }
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
        public static bool SaveFreeParameterScope(string scenario,
            int parameters, string reviewPhase)
        {
            if (scenario == RuntimeTestScenarioCatalog
                    .DisposableExpandedSummoningNativeProfile ||
                scenario == RuntimeTestScenarioCatalog
                    .DisposableAlternateSummoningPlayerPaths)
                return parameters == 0 && reviewPhase == null;
            if (scenario == RuntimeTestScenarioCatalog
                    .DisposableExpandedSummoningSprint22NativeReview)
                return parameters == 1 && (reviewPhase ==
                    "rtwp-foundation" || reviewPhase ==
                    "turn-based-control");
            if (scenario == RuntimeTestScenarioCatalog
                    .DisposableExpandedSummoningSprint23NativeReview)
                return parameters == 1 && (reviewPhase ==
                    "rtwp-foundation" || reviewPhase ==
                    "turn-based-control" || reviewPhase ==
                    "charge-rounding-rtwp" || reviewPhase ==
                    "charge-rounding-turn-based");
            if (scenario == RuntimeTestScenarioCatalog
                    .DisposableExpandedSummoningSprint24NativeReview)
                return parameters == 1 && (reviewPhase ==
                    "rtwp-foundation" || reviewPhase ==
                    "rtwp-deinonychus" || reviewPhase ==
                    "turn-based-deinonychus" || reviewPhase ==
                    "rtwp-tyrannosaurus" || reviewPhase ==
                    "turn-based-tyrannosaurus" || reviewPhase ==
                    "visual-lifecycle" || reviewPhase ==
                    "shared-controls");
            if (scenario == RuntimeTestScenarioCatalog
                    .DisposableExpandedSummoningSprint25NativeReview)
                return parameters == 1 && Sprint25Phase(reviewPhase);
            if (scenario == RuntimeTestScenarioCatalog.DisposableExpandedSummoningSprint26NativeReview)
                return parameters == 1 && Sprint26Phase(reviewPhase);
            if (scenario == RuntimeTestScenarioCatalog.DisposableExpandedSummoningSprint27NativeReview)
                return parameters == 1 && Sprint27Phase(reviewPhase);
            if (scenario == RuntimeTestScenarioCatalog.DisposableExpandedSummoningSprint28NativeReview)
                return parameters == 1 && Sprint28Phase(reviewPhase);
            return false;
        }
        public static bool CanStart(bool automaticExit, string scenario,
            int parameters, string reviewPhase,
            int worldUnits, int partyCharacters, int crossSceneEntities,
            bool loadedArea, bool mainCharacter, bool importSave,
            bool pendingNewGameUnit, bool idleSubscribedMenu)
        {
            return automaticExit && SaveFreeParameterScope(scenario,
                parameters, reviewPhase) && worldUnits == 0 &&
                partyCharacters == 0 && crossSceneEntities == 0 && !loadedArea &&
                !mainCharacter && !importSave && !pendingNewGameUnit && idleSubscribedMenu;
        }
    }
}
