using KingmakerGunslinger.RuntimeTesting;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningNativeProfileTests
    {
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
