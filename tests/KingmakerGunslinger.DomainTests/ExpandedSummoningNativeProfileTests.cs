using KingmakerGunslinger.RuntimeTesting;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningNativeProfileTests
    {
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
            Assertions.True(ExpandedSummoningNativeProfilePolicy.CanStart(true,0,0,0,0,false,false,false,false,true), "Idle fresh native world admitted.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true,0,1,0,0,false,false,false,false,true), "Existing world unit protected.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true,0,0,1,0,false,false,false,false,true), "Existing party protected.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true,0,0,0,1,false,false,false,false,true), "Cross-scene entity protected.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true,0,0,0,0,true,false,false,false,true), "Loaded area protected.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true,0,0,0,0,false,true,false,false,true), "Main character protected.");
        }
        internal static void SaveAndHumanEntryNeverAdmitted()
        {
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(false,0,0,0,0,false,false,false,false,true), "Cannot leave fixture game open.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true,1,0,0,0,false,false,false,false,true), "No save/import parameter allowed.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true,0,0,0,0,false,false,true,false,true), "Import descriptor protected.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true,0,0,0,0,false,false,false,true,true), "Pending human creation protected.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true,0,0,0,0,false,false,false,false,false), "Menu ownership must be proven.");
        }
    }
}
