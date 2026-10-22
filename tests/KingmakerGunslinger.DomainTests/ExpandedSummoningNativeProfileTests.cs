using KingmakerGunslinger.RuntimeTesting;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningNativeProfileTests
    {
        internal static void Cleanup2RequiresExactFreshSaveFreePhase()
        {
            string scenario = RuntimeTestScenarioCatalog.DisposableExpandedSummoningCleanup2NativeReview;
            foreach (string phase in new[] { "giant-lineup", "menus", "elementals", "bodies", "cloth-probe", "ettin-lifecycle", "scale-prototype", "tooltips", "crab-attack" })
            {
                Assertions.True(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 1, phase, 0, 0, 0, false, false, false, false, true), "Exact fresh Cleanup2 phase is admitted: " + phase);
                Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(false, scenario, 1, phase, 0, 0, 0, false, false, false, false, true), "Cleanup2 must exit automatically.");
                Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 1, phase, 1, 0, 0, false, false, false, false, true), "Cleanup2 must preserve an existing world.");
                Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 1, phase, 0, 0, 0, false, false, true, false, true), "Cleanup2 must never import a save.");
                Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 1, phase, 0, 0, 0, false, false, false, false, false), "Cleanup2 needs owned idle menu admission.");
                Assertions.False(ExpandedSummoningNativeProfilePolicy.SaveFreeParameterScope(scenario, 2, phase), "A save or other extra parameter is refused.");
            }
            foreach (string phase in new[] { null, "all", "Menus", "giant-lineup ", "routes" })
                Assertions.False(ExpandedSummoningNativeProfilePolicy.SaveFreeParameterScope(scenario, 1, phase), "Unqualified or foreign phases are refused.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.SaveFreeParameterScope(scenario, 0, null), "Cleanup2 requires exactly one explicit phase.");
        }
        internal static void BitesCannotSatisfyClawDismantle()
        {
            Assertions.True(ExpandedSummoningNativeProfilePolicy.BiteArmorControl(true,true,true,false,true), "Two bites leave exact armor unchanged.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.BiteArmorControl(true,true,true,true,true), "Unexpected bite dismantle is a regression.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.BiteArmorControl(true,true,true,false,false), "Item mutation remains a failure.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.BiteArmorControl(false,true,true,false,true), "Wrong weapon cannot satisfy bite control.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.BiteArmorControl(true,false,true,false,true), "Unexecuted attack cannot satisfy control.");
        }
        internal static void AttemptExposureRetainsSourceAndDc()
        {
            Assertions.True(ExpandedSummoningNativeProfilePolicy.ContactAttemptExposed(true,true,true,true,true), "Exact grapple attempt exposure admitted regardless of native Success getter.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.ContactAttemptExposed(false,true,true,true,true), "Trip cannot substitute for contact.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.ContactAttemptExposed(true,false,true,true,true), "Uncontrolled native hold is not this isolated fixture.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.ContactAttemptExposed(true,true,false,true,true), "Missing buff remains failure.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.ContactAttemptExposed(true,true,true,false,true), "Wrong DC remains failure.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.ContactAttemptExposed(true,true,true,true,false), "Wrong caster remains failure.");
        }
        internal static void VisualAccountingRejectsMissingDuplicateAndSaturatedEvents()
        {
            Assertions.True(ExpandedSummoningNativeProfilePolicy.OneOutcomePerEligibleView(12,12,12,3,15), "Every eligible family counted once.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.OneOutcomePerEligibleView(13,12,12,3,16), "Duplicate event rejected.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.OneOutcomePerEligibleView(11,12,12,3,14), "Missing event rejected.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.OneOutcomePerEligibleView(12,12,11,3,15), "Failed attachment rejected.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.OneOutcomePerEligibleView(12,12,12,244,256), "Bounded ring saturation cannot prove event count.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.OneOutcomePerEligibleView(0,0,0,0,0), "Empty visual selection refused.");
        }
        internal static void EntryCannotReturnToUnownedTick()
        {
            Assertions.True(ExpandedSummoningNativeProfilePolicy.ReadyForWorldLoad(true,true,true), "Native loading owns transition to a registered real scene.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.ReadyForWorldLoad(true,true,false), "Valid scene alone cannot admit an idle callback before native Tick.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.ReadyForWorldLoad(true,false,true), "Queued loader cannot admit an unregistered scene.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.ReadyForWorldLoad(false,true,true), "Entry callback must actually execute.");
        }
        internal static void FailedSceneHistoryRetainsActualHandles()
        {
            var evidence = new JObject { ["sceneObserverRemoved"] = true, ["sceneHistory"] = new JArray(
                new JObject { ["event"] = "scene-loaded:Additive", ["target"] = "BaseMechanics", ["handle"] = 17,
                    ["scenes"] = new JArray(new JObject { ["name"] = "BaseMechanics", ["handle"] = 17, ["valid"] = true, ["loaded"] = true }) },
                new JObject { ["event"] = "UnloadSceneAsync", ["target"] = "BaseMechanics", ["caller"] = "native caller" },
                new JObject { ["event"] = "scene-unloaded", ["target"] = "BaseMechanics", ["handle"] = 17 }) };
            var parsed = JObject.Parse(BoundaryObservationPolicy.Serialize(evidence));
            Assertions.Equal(3, ((JArray)parsed["sceneHistory"]).Count, "Loaded, unload call and completed unload remain distinct.");
            Assertions.Equal(17, (int)parsed["sceneHistory"][0]["scenes"][0]["handle"], "Exact scene handle retained.");
            Assertions.Equal("native caller", (string)parsed["sceneHistory"][1]["caller"], "Owning native caller retained.");
            Assertions.Equal("scene-unloaded", (string)parsed["sceneHistory"][2]["event"], "Adverse transition retained.");
            Assertions.True((bool)parsed["sceneObserverRemoved"], "Observer cleanup remains a separate fact.");
        }
        internal static void NativeStartupOverlayMustFinish()
        {
            Assertions.True(ExpandedSummoningNativeProfilePolicy.StartupComplete(1,true,false), "Native startup epilogue proven.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.StartupComplete(1,true,true), "Scene/input availability cannot admit an active startup overlay.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.StartupComplete(0,true,false), "Absent starter cannot prove readiness.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.StartupComplete(2,true,false), "Ambiguous starter refused.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.StartupComplete(1,false,false), "Missing startup completion predicate refused.");
        }
        internal static void RealSceneServicesRequired()
        {
            Assertions.True(ExpandedSummoningNativeProfilePolicy.ReadyForMechanics(true,true,true,true,true), "Real registered native world admitted.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.ReadyForMechanics(false,true,true,true,true), "No mechanics without BaseMechanics scene.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.ReadyForMechanics(true,false,true,true,true), "Wrong area refused.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.ReadyForMechanics(true,true,false,true,true), "Missing loaded-state refused.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.ReadyForMechanics(true,true,true,false,true), "Missing main character refused.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.ReadyForMechanics(true,true,true,true,false), "Missing registered party anchor refused.");
        }
        internal static void ExistingWorldNeverAdmitted()
        {
            string scenario = RuntimeTestScenarioCatalog.DisposableExpandedSummoningNativeProfile;
            Assertions.True(ExpandedSummoningNativeProfilePolicy.CanStart(true,scenario,0,null,0,0,0,false,false,false,false,true), "Idle fresh native world admitted.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true,scenario,0,null,1,0,0,false,false,false,false,true), "Existing world unit protected.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true,scenario,0,null,0,1,0,false,false,false,false,true), "Existing party protected.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true,scenario,0,null,0,0,1,false,false,false,false,true), "Cross-scene entity protected.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true,scenario,0,null,0,0,0,true,false,false,false,true), "Loaded area protected.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true,scenario,0,null,0,0,0,false,true,false,false,true), "Main character protected.");
        }
        internal static void SaveAndHumanEntryNeverAdmitted()
        {
            string basic = RuntimeTestScenarioCatalog.DisposableExpandedSummoningNativeProfile;
            string sprint22 = RuntimeTestScenarioCatalog.DisposableExpandedSummoningSprint22NativeReview;
            string sprint23 = RuntimeTestScenarioCatalog.DisposableExpandedSummoningSprint23NativeReview;
            string sprint25 = RuntimeTestScenarioCatalog.DisposableExpandedSummoningSprint25NativeReview;
            string sprint27 = RuntimeTestScenarioCatalog.DisposableExpandedSummoningSprint27NativeReview;
            foreach(string phase in new[]{"rtwp-foundation","pounce-rtwp","pounce-turn-based","rtwp-ai","charge-boundaries","visual-lifecycle","routes","outdoor-locomotion","shared-controls","expiry-boundary","qualification-combat","qualification-presentation"})
                Assertions.True(ExpandedSummoningNativeProfilePolicy.CanStart(true,sprint27,1,phase,0,0,0,false,false,false,false,true), "Exact Sprint27 bounded phase enters only a fresh empty owned menu: "+phase);
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(false,sprint27,1,"rtwp-foundation",0,0,0,false,false,false,false,true), "Sprint27 requires automatic exit.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true,sprint27,1,"rtwp-foundation",1,0,0,false,false,false,false,true), "Sprint27 cannot replace an existing world.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.SaveFreeParameterScope(sprint27,2,"rtwp-foundation"), "Sprint27 forbids save/import and extra parameters.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.SaveFreeParameterScope(sprint27,1,"all"), "Sprint27 forbids an unimplemented campaign.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.SaveFreeParameterScope(sprint27,1,"RTWP-FOUNDATION"), "Sprint27 phase identity is exact.");
            Assertions.True(ExpandedSummoningNativeProfilePolicy.SaveFreeParameterScope(sprint25,1,"rtwp-foundation"), "Sprint 25 admits the implemented bounded phase.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.SaveFreeParameterScope(sprint25,0,null), "Sprint 25 requires its phase.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.SaveFreeParameterScope(sprint25,2,"rtwp-foundation"), "Sprint 25 rejects a save/import parameter.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.SaveFreeParameterScope(sprint25,1,"all"), "Sprint 25 rejects broad implicit campaigns.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(false,sprint25,1,"rtwp-foundation",0,0,0,false,false,false,false,true), "Sprint 25 cannot leave its fixture open.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true,sprint25,1,"rtwp-foundation",1,0,0,false,false,false,false,true), "Sprint 25 cannot replace an existing native world.");
            Assertions.True(ExpandedSummoningNativeProfilePolicy.SaveFreeParameterScope(basic,0,null), "Original profile remains parameter-free.");
            Assertions.True(ExpandedSummoningNativeProfilePolicy.SaveFreeParameterScope(sprint22,1,"rtwp-foundation"), "RTWP phase is one save-free fixture parameter.");
            Assertions.True(ExpandedSummoningNativeProfilePolicy.SaveFreeParameterScope(sprint22,1,"turn-based-control"), "Turn-based phase is one save-free fixture parameter.");
            Assertions.True(ExpandedSummoningNativeProfilePolicy.SaveFreeParameterScope(sprint23,1,"rtwp-foundation"), "Sprint 23 RTWP phase is one save-free fixture parameter.");
            Assertions.True(ExpandedSummoningNativeProfilePolicy.SaveFreeParameterScope(sprint23,1,"turn-based-control"), "Sprint 23 turn-based phase is one save-free fixture parameter.");
            Assertions.True(ExpandedSummoningNativeProfilePolicy.SaveFreeParameterScope(sprint23,1,"charge-rounding-rtwp"), "Sprint 23 charge-rounding RTWP phase is one save-free fixture parameter.");
            Assertions.True(ExpandedSummoningNativeProfilePolicy.SaveFreeParameterScope(sprint23,1,"charge-rounding-turn-based"), "Sprint 23 charge-rounding turn-based phase is one save-free fixture parameter.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.SaveFreeParameterScope(basic,1,"rtwp-foundation"), "Original profile cannot borrow Sprint 22 scope.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.SaveFreeParameterScope(sprint22,0,null), "Sprint 22 requires its exact phase.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.SaveFreeParameterScope(sprint22,2,"rtwp-foundation"), "Extra Sprint 22 parameters remain forbidden.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.SaveFreeParameterScope(sprint22,1,"all"), "Broad Sprint 22 phase remains forbidden.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.SaveFreeParameterScope(sprint22,1,"charge-rounding-rtwp"), "Sprint 22 cannot borrow the Sprint 23 charge phase.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.SaveFreeParameterScope(sprint23,0,null), "Sprint 23 requires its exact phase.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.SaveFreeParameterScope(sprint23,2,"rtwp-foundation"), "Extra Sprint 23 parameters remain forbidden.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.SaveFreeParameterScope(sprint23,1,"all"), "Broad Sprint 23 phase remains forbidden.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.SaveFreeParameterScope("foreign",0,null), "Unrelated scenario cannot enter the fixture.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(false,basic,0,null,0,0,0,false,false,false,false,true), "Cannot leave fixture game open.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true,basic,1,null,0,0,0,false,false,false,false,true), "No save/import parameter allowed.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true,basic,0,null,0,0,0,false,false,true,false,true), "Import descriptor protected.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true,basic,0,null,0,0,0,false,false,false,true,true), "Pending human creation protected.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true,basic,0,null,0,0,0,false,false,false,false,false), "Menu ownership must be proven.");
        }
    }
}
