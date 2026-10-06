using System;
namespace KingmakerGunslinger.ElementalRaces
{
    internal static class WhiteoutFoundationPolicy
    {
        internal static bool EnvironmentActive(bool exactProvider, bool knownMap, bool indoors,
            WhiteoutPrecipitation precipitation, int actual, bool module, bool contract, bool areaLoaded)
        {
            return knownMap && module && contract && areaLoaded &&
                WhiteoutPolicy.WeatherActive(exactProvider, precipitation, actual, indoors);
        }
    }
    // Cache Whiteout's own decision only. Every invocation still preserves its
    // current native false and revalidates current applicability. No unit references.
    internal sealed class WhiteoutNativeStageDecision
    {
        private readonly object _gate=new object();
        internal bool Decided { get; private set; }
        internal bool Applicable { get; private set; }
        internal bool RollAttempted { get; private set; }
        internal int? Roll { get; private set; }
        internal bool FailedOpen { get; private set; }
        private bool _continues=true;
        internal bool AfterNative(bool nativeSucceeded, bool currentlyApplicable, Func<int> roll)
        {
            if(!nativeSucceeded) return false;
            lock(_gate)
            {
                if(!Decided) { Decided=true; Applicable=currentlyApplicable; }
                if(!currentlyApplicable || !Applicable) return true;
                if(!RollAttempted)
                {
                    RollAttempted=true; // Even a throwing diagnostic/native RNG cannot retry.
                    try { if(roll==null) throw new ArgumentNullException("roll"); Roll=roll(); _continues=!WhiteoutPolicy.IsMiss(Roll.Value); }
                    catch(Exception) { FailedOpen=true; _continues=true; }
                }
                return _continues;
            }
        }
    }
}
