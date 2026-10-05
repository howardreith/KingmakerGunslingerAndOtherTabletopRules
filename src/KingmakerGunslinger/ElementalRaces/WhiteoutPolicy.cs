using System;

namespace KingmakerGunslinger.ElementalRaces
{
    // Unpublished policy only. No feature, buff, setting, patch, localization or
    // acquisition path is registered by this file. Native binding requires the
    // separately observed weather/indoor and concealment-stage contracts.
    internal enum WhiteoutPrecipitation { Normal = 0, Rain = 1, Snow = 2 }

    internal static class WhiteoutPolicy
    {
        internal static bool WeatherActive(bool marker, WhiteoutPrecipitation precipitation,
            int actualInclemency, bool excludedIndoors)
        {
            return marker && !excludedIndoors && actualInclemency >= 1 && actualInclemency <= 4 &&
                (precipitation == WhiteoutPrecipitation.Rain || precipitation == WhiteoutPrecipitation.Snow);
        }

        // Deliberately attack-kind agnostic: both melee and ranged share this
        // stage. Seeking bypass is checked separately because the qualified mod
        // Seeking patch succeeds the native check without setting IgnoreConcealment.
        internal static bool ShouldRoll(bool active, bool atConcealmentStage,
            bool nativeSucceeded, bool ignoreConcealment, bool seeking,
            bool moduleEnabled, bool exactPatchContract)
        {
            return active && atConcealmentStage && nativeSucceeded &&
                !ignoreConcealment && !seeking && moduleEnabled && exactPatchContract;
        }

        internal static bool IsMiss(int d100)
        {
            if (d100 < 1 || d100 > 100) throw new ArgumentOutOfRangeException("d100");
            return d100 <= 10;
        }
    }

    // One instance per eventual marker owner; no static/shared unit state.
    // Event adapters, not polling, must call reconciliation. Indoor exclusion
    // is an explicit observed input, not a speculative engine assumption.
    internal sealed class WhiteoutWeatherState
    {
        internal bool Active { get; private set; }
        internal int TransitionCount { get; private set; }
        internal bool Reconcile(bool marker, WhiteoutPrecipitation precipitation,
            int actualInclemency, bool excludedIndoors, bool moduleEnabled, bool areaLoaded)
        {
            return Set(moduleEnabled && areaLoaded && WhiteoutPolicy.WeatherActive(
                marker, precipitation, actualInclemency, excludedIndoors));
        }
        internal bool OnAreaUnloading() { return Set(false); }
        internal bool OnMarkerRemoved() { return Set(false); }
        private bool Set(bool active)
        {
            if (Active == active) return false;
            Active = active; TransitionCount++; return true;
        }
    }

    // A future native adapter must associate this with one exact RuleAttackRoll,
    // never with a unit globally. Re-reading/replaying that rule cannot roll twice.
    internal sealed class WhiteoutAttackDecision
    {
        private bool _processed;
        private bool _result;
        internal bool Resolve(bool nativeSucceeded, bool shouldRoll, Func<int> roll)
        {
            if (_processed) return _result;
            bool result = nativeSucceeded;
            if (nativeSucceeded && shouldRoll)
            {
                if (roll == null) throw new ArgumentNullException("roll");
                result = !WhiteoutPolicy.IsMiss(roll());
            }
            _result = result; _processed = true;
            return result;
        }
    }
}
