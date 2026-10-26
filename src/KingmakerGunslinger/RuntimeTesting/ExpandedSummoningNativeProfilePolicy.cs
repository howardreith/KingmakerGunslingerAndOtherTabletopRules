namespace KingmakerGunslinger.RuntimeTesting
{
    // Admission to a save-free first-level fixture must never replace a
    // campaign, import a save or leave its game process open.
    public static class ExpandedSummoningNativeProfilePolicy
    {
        internal const string Sprint32SatyrControlPhase = "satyr-control";
        internal const string Sprint32LillendSupportPhase = "lillend-support";
        internal const string Sprint32SatyrTurnPhase = "satyr-turn";
        internal const string Sprint32SatyrAtWillPhase = "satyr-atwill";
        internal const string Sprint32SatyrAtWillTurnPhase = "satyr-atwill-turn";
        internal const string Sprint32LillendTurnPhase = "lillend-turn";
        internal const string Sprint32SuggestionPhase = "optional-suggestion";
        internal const string Sprint32SatyrRoutesPhase = "satyr-routes";
        internal const string Sprint32LillendRoutesPhase = "lillend-routes";
        internal const string Sprint32LifecyclePhase = "support-lifecycle";
        internal const string Sprint32DruidInputPhase = "druid-input";
        internal const string Sprint32WizardInputPhase = "wizard-input";
        internal const string Sprint32SorcererInputPhase = "sorcerer-input";
        internal const string Sprint32PerformancePhase = "lillend-performance";
        internal const string Sprint32HoldPhase = "lillend-hold";
        internal const string Sprint32HoldTurnPhase = "lillend-hold-turn";
        internal const string Sprint32ControlsPhase = "lillend-controls";
        internal const string Sprint32ControlsTurnPhase = "lillend-controls-turn";
        internal const string Sprint32GrabPhase = "lillend-grab";
        internal const string Sprint32CantripPhase = "lillend-cantrips";
        internal const string Sprint32FascinatePhase = "lillend-fascinate";
        internal const string Sprint32BardicSuggestionPhase = "lillend-bardic-suggestion";
        internal static bool Sprint32NativeInputPhase(string phase)
        { return phase == Sprint32DruidInputPhase || phase == Sprint32WizardInputPhase || phase == Sprint32SorcererInputPhase; }
        internal static bool Sprint32NativeDefaultPhase(string phase)
        { return Sprint32NativeInputPhase(phase) || phase == Sprint32PerformancePhase ||
            phase == Sprint32FascinatePhase || phase == Sprint32BardicSuggestionPhase || phase == Sprint32GrabPhase; }
        internal static bool Sprint32NativeDefaultOutdoor(string scenario, string phase)
        { return scenario == RuntimeTestScenarioCatalog.DisposableExpandedSummoningSprint32NativeReview && Sprint32NativeDefaultPhase(phase); }
        internal static bool Sprint32Phase(string phase)
        { return phase == Sprint32SatyrControlPhase || phase == Sprint32LillendSupportPhase ||
            phase == Sprint32SatyrTurnPhase || phase == Sprint32LillendTurnPhase || phase == Sprint32SuggestionPhase ||
            phase == Sprint32SatyrRoutesPhase || phase == Sprint32LillendRoutesPhase || phase == Sprint32LifecyclePhase ||
            phase == Sprint32CantripPhase || phase == Sprint32HoldPhase || phase == Sprint32HoldTurnPhase ||
            phase == Sprint32ControlsPhase || phase == Sprint32ControlsTurnPhase ||
            phase == Sprint32SatyrAtWillPhase ||
            phase == Sprint32SatyrAtWillTurnPhase || Sprint32NativeDefaultPhase(phase); }
        internal const string OutsiderBodyPhase = "outsider-bodies";
        internal const string SourceLifecyclePhase = "source-lifecycle";
        internal const string DefenseConsumersPhase = "defense-consumers";
        internal const string DretchMeleePhase = "dretch-melee";
        internal const string DretchMeleeAiPhase = "dretch-melee-ai";
        internal const string HoundSinglePhase = "hound-single";
        internal const string HoundAiPhase = "hound-ai";
        internal const string HoundTurnPhase = "hound-turn";
        internal const string HoundDisarmPhase = "hound-disarm";
        internal const string SharedCirclesPhase = "shared-circles";
        internal const string ArchonBoundariesPhase = "archon-boundaries";
        internal const string PublicRoutesPhase = "public-routes";
        internal const string LanternRayPhase = "lantern-rays";
        internal const string LanternBodyPhase = "lantern-body";
        internal const string AstralCorePhase = "astral-core";
        internal const string AstralHammerPhase = "astral-hammer";
        internal const string AstralBladePhase = "astral-blade";
        internal const string AstralSupportPhase = "astral-support";
        internal const string AstralAtWillPhase = "astral-atwill";
        internal const string AstralSmitePhase = "astral-smite";
        internal const string AstralDispelPhase = "astral-dispel";
        internal const string TrumpetDispelPhase = "trumpet-dispel";
        internal const string TrumpetBlastPhase = "trumpet-blast";
        internal const string TrumpetBlastAiPhase = "trumpet-blast-ai";
        internal const string TrumpetBlastAiSafePhase = "trumpet-blast-ai-safe";
        internal const string DispelUiCase = "sprints30-31-dispel-ui";
        internal const string DispelUiCreatures = "trumpet-archon,astral-deva";
        // Fixed supported subcases; requests cannot supply code or ordering.
        internal static string[] OutsiderBodySequence
        { get { return new[] { "lemure", "dretch" }; } }

        internal static bool Sprint25Phase(string phase)
        {
            return phase == "rtwp-foundation" || phase == "turn-based-commands" ||
                phase == "rtwp-ai" || phase == "routes" || phase == "visual-lifecycle" ||
                phase == "overrun" || phase == "great-cleave";
        }
        internal static bool Cleanup2Phase(string phase)
        { return phase == "giant-lineup" || phase == "giants-off" || phase == "menus" || phase == "elementals" || phase == "bodies" || phase == "cloth-probe" || phase == "ettin-lifecycle" || phase == "ettin-commands" || phase == "elementals-presentation" || phase == "elementals-console" || phase == "elementals-sources" || phase == "elementals-pin" || phase == "elementals-learning" || phase == "elementals-off" || phase == "menu-adapters" || phase == "scale-prototype" || phase == "tooltips" || phase == "crab-attack" || phase == "production-attacks" || phase == "gorgon-presentation" || phase == "recovery-bodies" || phase == "creature-commands" || phase == "alternate-console" || phase == "shared-controls"; }
        internal static bool Sprint28Phase(string phase)
        {
            return phase == "profiles" || phase == "modes" ||
                phase == "commands" || phase == "turns" || phase == "ownership" ||
                phase == "disarm" || phase == "catch" || phase == "ai" ||
                phase == "ai-turns" || phase == "cleave" || phase == "routes";
        }
        internal static bool Sprints30And31Phase(string phase)
        { return phase == "final-integration" || phase == "boundary-lemure" || phase == "boundary-dretch" || phase == "boundary-hound-archon" || phase == "boundary-lantern-archon" || phase == "boundary-astral-deva" || phase == "boundary-trumpet-archon" || phase == "celestial-cleave" || phase == PublicRoutesPhase || phase == ArchonBoundariesPhase || phase == HoundDisarmPhase || phase == SharedCirclesPhase || phase == HoundSinglePhase || phase == HoundAiPhase || phase == HoundTurnPhase || phase == DretchMeleePhase || phase == DretchMeleeAiPhase || phase == DefenseConsumersPhase || phase == SourceLifecyclePhase || phase == RuntimeTestScenarioCatalog.Sprints30And31OutsiderBodyPhase || phase == RuntimeTestScenarioCatalog.Sprints30And31LanternRayPhase || phase == RuntimeTestScenarioCatalog.Sprints30And31LanternBodyPhase || phase == RuntimeTestScenarioCatalog.Sprints30And31AstralCorePhase || phase == RuntimeTestScenarioCatalog.Sprints30And31AstralHammerPhase || phase == RuntimeTestScenarioCatalog.Sprints30And31AstralBladePhase || phase == RuntimeTestScenarioCatalog.Sprints30And31AstralSupportPhase || phase == RuntimeTestScenarioCatalog.Sprints30And31AstralAtWillPhase || phase == RuntimeTestScenarioCatalog.Sprints30And31AstralSmitePhase || phase == RuntimeTestScenarioCatalog.Sprints30And31AstralDispelPhase || phase == RuntimeTestScenarioCatalog.Sprints30And31TrumpetDispelPhase || phase == "bootstrap" || phase == "lemure-core" || phase == "lemure-full" || phase == "lemure-ai" || phase == "dretch-cloud" || phase == "dretch-ai" || phase == "dretch-ai-ally" || phase == "archon-core" || phase == "archon-hound" || phase == "hound-armed" || phase == "hound-natural" || phase == "trumpet-book" || phase == "trumpet-book-turn" || phase == "trumpet-caster" || phase == TrumpetBlastPhase || phase == TrumpetBlastAiPhase || phase == TrumpetBlastAiSafePhase; }
        internal static bool Sprints30And31Outdoor(string scenario, string phase)
        {
            return scenario == RuntimeTestScenarioCatalog.DisposableExpandedSummoningSprints30And31NativeReview &&
                phase == TrumpetBlastAiSafePhase;
        }
        internal static bool Sprints30And31DispelUi(bool automaticExit,
            string scenario, int parameterCount, string saveName,
            string creatures, string reviewCase)
        {
            return automaticExit && parameterCount == 3 &&
                scenario == RuntimeTestScenarioCatalog.WorkingSaveExpandedSummoningCreatureReview &&
                saveName == "KMG_AUTOMATION_WORKING" &&
                creatures == RuntimeTestScenarioCatalog.Sprints30And31DispelUiCreatures &&
                reviewCase == RuntimeTestScenarioCatalog.Sprints30And31DispelUiCase;
        }
        // Exact presentation diagnostics observed alongside a successful
        // native Working load. All native load/JSON/identity predicates still
        // apply; other errors, exceptions, traces and overflow remain blocking.
        public static bool WorkingUiPresentationDiagnostic(bool error,
            string message, string trace)
        {
            if (!error || !string.IsNullOrEmpty(trace)) return false;
            return message == @"The AssetBundle 'StreamingAssets\Bundles\common_assets' can't be loaded because another AssetBundle with the same files is already loaded." ||
                message == @"The AssetBundle 'StreamingAssets\Bundles\equipment_bundle' can't be loaded because another AssetBundle with the same files is already loaded." ||
                message == "FogOfWarStaticMaskHolder bounds is not completely placed inside in LocationMaskRenderer bounds [JamandisMansion_Mechanics]";
        }
        internal static bool Sprint29Phase(string phase)
        { return phase == "profiles" || phase == "modes" ||
            phase == "commands" || phase == "turns" ||
            phase == "cloud-magic" || phase == "storm-magic" ||
            phase == "ai" || phase == "ai-turns" ||
            phase == "items" || phase == "disarm" ||
            phase == "awesome" || phase == "awesome-turn" ||
            phase == "catch" || phase == "cleave" ||
            phase == "vital" || phase == "vital-turn" ||
            phase == "routes"; }
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
            if (scenario == RuntimeTestScenarioCatalog.DisposableExpandedSummoningSprint29NativeReview)
                return parameters == 1 && Sprint29Phase(reviewPhase);
            if (scenario == RuntimeTestScenarioCatalog.DisposableExpandedSummoningSprint32NativeReview)
                return parameters == 1 && Sprint32Phase(reviewPhase);
            if (scenario == RuntimeTestScenarioCatalog.DisposableExpandedSummoningSprints30And31NativeReview)
                return parameters == 1 && Sprints30And31Phase(reviewPhase);
            if (scenario == RuntimeTestScenarioCatalog.DisposableExpandedSummoningCleanup2NativeReview)
                return parameters == 1 && Cleanup2Phase(reviewPhase);
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
