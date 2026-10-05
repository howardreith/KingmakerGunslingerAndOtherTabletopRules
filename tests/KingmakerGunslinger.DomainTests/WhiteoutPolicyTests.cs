using System;
using System.IO;
using System.Linq;
using KingmakerGunslinger.ElementalRaces;

namespace KingmakerGunslinger.DomainTests
{
    internal static class WhiteoutPolicyTests
    {
        private static bool Active(bool marker, WhiteoutPrecipitation weather, int intensity)
        { return WhiteoutPolicy.WeatherActive(marker, weather, intensity, false); }
        private static bool Eligible(bool active = true, bool stage = true, bool native = true,
            bool ignore = false, bool seeking = false, bool module = true, bool contract = true)
        { return WhiteoutPolicy.ShouldRoll(active, stage, native, ignore, seeking, module, contract); }
        private static void Check(bool expected, bool actual) { Assertions.Equal(expected, actual, "Whiteout frozen policy."); }
        internal static void ClearWithMarkerInactive()
        { Check(false, Active(true, WhiteoutPrecipitation.Rain, 0)); }
        internal static void NormalWithMarkerInactive()
        { Check(false, Active(true, WhiteoutPrecipitation.Normal, 4)); }
        internal static void LightRainActive()
        { Check(true, Active(true, WhiteoutPrecipitation.Rain, 1)); }
        internal static void ModerateHeavyStormRainActive()
        { for(int i=2;i<=4;i++) Check(true, Active(true, WhiteoutPrecipitation.Rain, i)); }
        internal static void LightThroughStormSnowActive()
        { for(int i=1;i<=4;i++) Check(true, Active(true, WhiteoutPrecipitation.Snow, i)); }
        internal static void PrecipitationWithoutMarkerInactive()
        { foreach(var weather in new[]{WhiteoutPrecipitation.Rain,WhiteoutPrecipitation.Snow}) for(int i=1;i<=4;i++) Check(false, Active(false,weather,i)); }
        internal static void MagicalFogAloneInactive()
        { Check(false, Active(true, WhiteoutPrecipitation.Normal, 0)); Check(false, Active(true, (WhiteoutPrecipitation)3, 4)); }
        internal static void SprayAndVfxHaveNoActivationInput()
        { Assertions.Equal(3, Enum.GetValues(typeof(WhiteoutPrecipitation)).Length, "Only Normal/Rain/Snow, no VFX or spell-descriptor activation."); }
        internal static void RollOneMisses()
        { Check(true, WhiteoutPolicy.IsMiss(1)); }
        internal static void RollTenMisses()
        { Check(true, WhiteoutPolicy.IsMiss(10)); }
        internal static void RollElevenContinues()
        { Check(false, WhiteoutPolicy.IsMiss(11)); }
        internal static void RollHundredContinues()
        { Check(false, WhiteoutPolicy.IsMiss(100)); }
        internal static void NativeMissShortCircuits()
        { var d=new WhiteoutAttackDecision(); int rolls=0; Check(false,d.Resolve(false,Eligible(native:false),()=>{rolls++;return 1;})); Check(false,d.Resolve(false,true,()=>{rolls++;return 1;})); Assertions.Equal(0,rolls,"No second roll/miss processing."); }
        internal static void NativeSuccessThenWhiteoutMiss()
        { Check(false,new WhiteoutAttackDecision().Resolve(true,Eligible(),()=>10)); }
        internal static void NativeSuccessThenWhiteoutSuccess()
        { Check(true,new WhiteoutAttackDecision().Resolve(true,Eligible(),()=>11)); }
        internal static void IndependentTwentyPlusTenIsTwentyEight()
        { int misses=0; for(int ordinary=1;ordinary<=100;ordinary++) for(int whiteout=1;whiteout<=100;whiteout++) { bool native=ordinary>20; if(!new WhiteoutAttackDecision().Resolve(native,Eligible(native:native),()=>whiteout)) misses++; } Assertions.Equal(2800,misses,"Exhaustive Cartesian decision table, not Monte Carlo or additive 30%."); }
        internal static void IgnoreConcealmentAndSeekingBypass()
        { Check(false,Eligible(ignore:true)); Check(false,Eligible(seeking:true)); }
        internal static void MeleeEligibleWithoutWeaponFilter()
        { Check(true,Eligible(active:Active(true,WhiteoutPrecipitation.Rain,1))); }
        internal static void RangedEligibleWithoutWeaponFilter()
        { Check(true,Eligible(active:Active(true,WhiteoutPrecipitation.Snow,1))); }
        internal static void UnmarkedAttackConsumesNoRoll()
        { int rolls=0; Check(true,new WhiteoutAttackDecision().Resolve(true,Eligible(active:Active(false,WhiteoutPrecipitation.Rain,4)),()=>{rolls++;return 1;})); Assertions.Equal(0,rolls,"Unrelated attack RNG untouched."); }
        internal static void TwoUnitsHaveIndependentState()
        { var a=new WhiteoutWeatherState(); var b=new WhiteoutWeatherState(); a.Reconcile(true,WhiteoutPrecipitation.Rain,1,false,true,true); Check(true,a.Active); Check(false,b.Active); b.Reconcile(true,WhiteoutPrecipitation.Snow,1,false,true,true); a.OnMarkerRemoved(); Check(false,a.Active); Check(true,b.Active); }
        internal static void WeatherTransitionsOnce()
        { var state=new WhiteoutWeatherState(); Check(true,state.Reconcile(true,WhiteoutPrecipitation.Rain,1,false,true,true)); Check(true,state.Reconcile(true,WhiteoutPrecipitation.Normal,0,false,true,true)); Assertions.Equal(2,state.TransitionCount,"One apply and one removal."); }
        internal static void RepeatedEventIdempotent()
        { var state=new WhiteoutWeatherState(); for(int i=0;i<8;i++) state.Reconcile(true,WhiteoutPrecipitation.Rain,1,false,true,true); Assertions.Equal(1,state.TransitionCount,"Repeated identical weather event."); }
        internal static void AreaTransitionClearsAndReconciles()
        { var state=new WhiteoutWeatherState(); state.Reconcile(true,WhiteoutPrecipitation.Rain,1,false,true,true); state.OnAreaUnloading(); Check(false,state.Active); state.Reconcile(true,WhiteoutPrecipitation.Rain,1,false,true,false); Check(false,state.Active); state.Reconcile(true,WhiteoutPrecipitation.Snow,1,false,true,true); Check(true,state.Active); }
        internal static void MarkerRemovalClears()
        { var state=new WhiteoutWeatherState(); state.Reconcile(true,WhiteoutPrecipitation.Rain,1,false,true,true); Check(true,state.OnMarkerRemoved()); Check(false,state.OnMarkerRemoved()); Check(false,state.Active); }
        internal static void DisabledOrAbsentFoundationUntouched()
        { Check(false,Eligible(module:false)); Check(false,Eligible(active:false)); var state=new WhiteoutWeatherState(); state.Reconcile(true,WhiteoutPrecipitation.Rain,1,false,false,true); Check(false,state.Active); }
        internal static void PatchContractMismatchFailsClosed()
        { Check(false,Eligible(contract:false)); Check(false,Eligible(stage:false)); }
        internal static void NoSaveVisibleAcquisitionIdentity()
        { string registry=File.ReadAllText(Path.Combine(Environment.CurrentDirectory,"blueprints","blueprints.json")); Assertions.False(registry.Contains("Whiteout"),"No registered/serialized or published Whiteout identity at the observation stage."); }
        internal static void NoTraitOrFeatPublication()
        { string root=Path.Combine(Environment.CurrentDirectory,"src","KingmakerGunslinger"); foreach(string file in Directory.GetFiles(root,"*.cs",SearchOption.AllDirectories).Where(f=>!f.Contains("RuntimeTesting") && !f.EndsWith("WhiteoutPolicy.cs"))) Assertions.False(File.ReadAllText(file).Contains("Whiteout"),"No acquisition/native patch binding at observation stage: "+file); }
        internal static void NoIconOrVisibleLocalization()
        { string source=File.ReadAllText(Path.Combine(Environment.CurrentDirectory,"src","KingmakerGunslinger","ElementalRaces","WhiteoutPolicy.cs")); foreach(string token in new[]{"m_Icon","LocalizationService","BlueprintFeature","BlueprintBuff","HarmonyPatch"}) Assertions.False(source.Contains(token),"Pure policy is unpublished: "+token); }
        internal static void ReplayedRuleUsesOneRoll()
        { int rolls=0; var d=new WhiteoutAttackDecision(); for(int i=0;i<5;i++) Check(false,d.Resolve(true,Eligible(),()=>{rolls++;return 10;})); Assertions.Equal(1,rolls,"One independent roll per exact future rule association."); }
        internal static void InvalidRollFailsBeforeDecisionMutation()
        { var d=new WhiteoutAttackDecision(); Assertions.Throws<ArgumentOutOfRangeException>(()=>d.Resolve(true,true,()=>0),"Invalid d100."); Assertions.Throws<ArgumentOutOfRangeException>(()=>WhiteoutPolicy.IsMiss(101),"Invalid d100."); Check(true,d.Resolve(true,true,()=>100)); }
        internal static void UnknownWeatherFailsClosed()
        { Check(false,Active(true,(WhiteoutPrecipitation)(-1),1)); Check(false,Active(true,WhiteoutPrecipitation.Rain,5)); Check(false,Active(true,WhiteoutPrecipitation.Snow,-1)); }
        internal static void IndoorExclusionIsExplicitObservedInput()
        { Check(false,WhiteoutPolicy.WeatherActive(true,WhiteoutPrecipitation.Rain,1,true)); Check(true,WhiteoutPolicy.WeatherActive(true,WhiteoutPrecipitation.Rain,1,false)); }
    }
}
