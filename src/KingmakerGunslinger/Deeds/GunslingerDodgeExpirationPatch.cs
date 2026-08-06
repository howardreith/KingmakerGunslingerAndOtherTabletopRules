using System;
using System.Globalization;
using System.Threading;
using Harmony12;
using Kingmaker;
using Kingmaker.UnitLogic.Buffs;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Deeds
{
    /// <summary>
    /// Narrow expiration guard for Gunslinger's Dodge.
    ///
    /// The live game assigns the custom buff a bounded EndTime, displays the
    /// countdown, and activates its +2 AC component, but the fact remains after
    /// that deadline in the tested mod stack. This postfix leaves the normal
    /// queue untouched and removes only this exact buff after its already-
    /// established native deadline has elapsed.
    /// </summary>
    [HarmonyPatch(typeof(BuffCollection), "Tick")]
    internal static class GunslingerDodgeExpirationPatch
    {
        private static long _activeObservations;
        private static long _expiredRemovals;
        private static long _faults;
        private static long _lastTimeLeftTicks;

        internal static long ActiveObservations
        {
            get { return Interlocked.Read(ref _activeObservations); }
        }

        internal static long ExpiredRemovals
        {
            get { return Interlocked.Read(ref _expiredRemovals); }
        }

        internal static long Faults
        {
            get { return Interlocked.Read(ref _faults); }
        }

        internal static TimeSpan LastTimeLeft
        {
            get { return TimeSpan.FromTicks(Interlocked.Read(ref _lastTimeLeftTicks)); }
        }

        private static void Postfix(BuffCollection __instance)
        {
            if (__instance == null) return;

            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            GunslingerDodgeBlueprintSet dodge = gunslinger == null ? null : gunslinger.Dodge;
            if (dodge == null || dodge.ArmorClassBuff == null) return;

            try
            {
                Buff buff = __instance.GetBuff(dodge.ArmorClassBuff);
                if (buff == null) return;

                Interlocked.Increment(ref _activeObservations);
                TimeSpan timeLeft = buff.TimeLeft;
                Interlocked.Exchange(ref _lastTimeLeftTicks, timeLeft.Ticks);

                Game game = Game.Instance;
                if (game == null || game.TimeController == null) return;

                TimeSpan now = game.TimeController.GameTime;
                bool expiredByTimeLeft = timeLeft <= TimeSpan.Zero;
                bool expiredByEndTime = buff.EndTime != TimeSpan.MaxValue &&
                    buff.EndTime <= now;
                if (!expiredByTimeLeft && !expiredByEndTime) return;

                __instance.RemoveFact(buff);
                Interlocked.Increment(ref _expiredRemovals);

                ModContext context;
                if (ModContext.TryGet(out context))
                {
                    context.Logger.Info(
                        "dodge",
                        "expiration.guard.removed",
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Removed Gunslinger's Dodge after its native deadline; endTicks={0}; nowTicks={1}; timeLeftTicks={2}.",
                            buff.EndTime.Ticks,
                            now.Ticks,
                            timeLeft.Ticks));
                }
            }
            catch (Exception exception)
            {
                long fault = Interlocked.Increment(ref _faults);
                if (fault == 1)
                {
                    ModContext context;
                    if (ModContext.TryGet(out context))
                    {
                        context.Logger.Failure(
                            "dodge",
                            "expiration.guard.failed",
                            "The targeted Gunslinger's Dodge expiration guard failed.",
                            exception);
                    }
                }
            }
        }

        internal static string Describe()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "observations={0}; expiredRemovals={1}; faults={2}; lastTimeLeftMs={3:0.###}",
                ActiveObservations,
                ExpiredRemovals,
                Faults,
                LastTimeLeft.TotalMilliseconds);
        }
    }
}
