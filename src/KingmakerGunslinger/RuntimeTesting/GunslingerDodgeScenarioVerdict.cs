using System;
using System.Collections.Generic;
using System.Globalization;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// What one use of the Gunslinger's Dodge ability command was seen to do.
    /// The scenario fills this from the game; the verdicts below read nothing
    /// else, so they can be exercised without the game.
    /// </summary>
    internal sealed class DodgeCommandObservation
    {
        internal bool AvailableBeforeUse;
        internal bool TimedOut;
        internal bool CommandStarted;
        internal bool CommandFinished;
        internal string CommandResult = "<not-run>";
        internal bool HadExecutionProcess;
        internal bool DeliveryEnded;
        internal int GritBefore;
        internal int GritAfter;
        internal int BuffsBefore;
        internal int BuffsAfter;
        internal bool InstanceIsNew;
        internal int AddedNotices;
        internal int ArmorClassBefore;
        internal int ArmorClassAfter;
        internal bool MarkedNotDispelable;
        internal bool Permanent;
        internal double TimeLeftSeconds;

        internal string Describe()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "available={0};timedOut={1};started={2};finished={3};result={4};" +
                "executionProcess={5};deliveryEnded={6};grit={7}->{8};buffs={9}->{10};" +
                "newInstance={11};addedNotices={12};ac={13}->{14};notDispelable={15};" +
                "permanent={16};timeLeft={17:0.##}",
                AvailableBeforeUse, TimedOut, CommandStarted, CommandFinished,
                CommandResult, HadExecutionProcess, DeliveryEnded, GritBefore,
                GritAfter, BuffsBefore, BuffsAfter, InstanceIsNew, AddedNotices,
                ArmorClassBefore, ArmorClassAfter, MarkedNotDispelable, Permanent,
                TimeLeftSeconds);
        }
    }

    /// <summary>The armor class one incoming attack roll was resolved against.</summary>
    internal sealed class DodgeIncomingAttack
    {
        internal DodgeIncomingAttack(bool resolved, int targetArmorClass,
            bool targetFlatFooted)
        {
            Resolved = resolved;
            TargetArmorClass = targetArmorClass;
            TargetFlatFooted = targetFlatFooted;
        }

        internal bool Resolved { get; private set; }
        internal int TargetArmorClass { get; private set; }
        internal bool TargetFlatFooted { get; private set; }

        internal string Describe()
        {
            return Resolved ? TargetArmorClass.ToString(CultureInfo.InvariantCulture) +
                (TargetFlatFooted ? "(flat-footed)" : "") : "<no roll>";
        }
    }

    /// <summary>
    /// One wait of the scenario. A wait ends either because its condition
    /// held or because its real-time limit ran out; only the first counts as
    /// having been reached, and the answer is final once the wait has ended.
    /// </summary>
    internal sealed class DodgeWait
    {
        internal DodgeWait(string name, double limitRealSeconds)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("name");
            if (limitRealSeconds <= 0d) throw new ArgumentOutOfRangeException("limitRealSeconds");
            Name = name;
            LimitRealSeconds = limitRealSeconds;
        }

        internal string Name { get; private set; }
        internal double LimitRealSeconds { get; private set; }
        internal bool Ended { get; private set; }
        internal bool Reached { get; private set; }
        internal double WaitedRealSeconds { get; private set; }

        /// <summary>One poll per frame. True once the wait is over.</summary>
        internal bool Poll(bool conditionHolds, double elapsedRealSeconds)
        {
            if (Ended) return true;
            WaitedRealSeconds = elapsedRealSeconds;
            if (conditionHolds)
            {
                Reached = true;
                Ended = true;
            }
            else if (elapsedRealSeconds >= LimitRealSeconds) Ended = true;
            return Ended;
        }

        internal string Describe()
        {
            return Name + "=" + (!Ended ? "not-ended" : Reached ? "reached" :
                "limit-of-" + LimitRealSeconds.ToString("0.#",
                    CultureInfo.InvariantCulture) + "s-ran-out") + "(" +
                WaitedRealSeconds.ToString("0.##", CultureInfo.InvariantCulture) + "s)";
        }
    }

    /// <summary>
    /// The times of the recast check, in seconds of game time, and what was
    /// on the unit when it was read. The earlier deadline is the earlier
    /// buff's own end time, read before the fixture removed it.
    /// </summary>
    internal sealed class DodgeRecastObservation
    {
        internal bool EarlierDeadlineCaptured;
        internal double EarlierAddedAt = -1d;
        internal double EarlierDeadline = -1d;
        internal double EarlierRemovedAt = -1d;
        internal double LaterAddedAt = -1d;
        internal DodgeWait DeadlineWait;
        internal double SurvivalObservedAt = -1d;
        internal bool LaterInstanceOnUnit;
        internal int ArmorClassAtObservation = -1;
        internal DodgeWait EndWait;
        internal bool LaterEnded;
        internal double LaterEndedAt = -1d;
        internal int ArmorClassAfterEnd = -1;

        internal string Describe()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "earlierAddedAt={0:0.##};earlierDeadline={1:0.##}(captured={2});" +
                "earlierRemovedAt={3:0.##};laterAddedAt={4:0.##};{5};" +
                "survivalObservedAt={6:0.##};laterInstanceOnUnit={7};acAtObservation={8};" +
                "{9};laterEnded={10};laterEndedAt={11:0.##};acAfterEnd={12}",
                EarlierAddedAt, EarlierDeadline, EarlierDeadlineCaptured,
                EarlierRemovedAt, LaterAddedAt,
                DeadlineWait == null ? "deadlineWait=not-started" : DeadlineWait.Describe(),
                SurvivalObservedAt, LaterInstanceOnUnit, ArmorClassAtObservation,
                EndWait == null ? "endWait=not-started" : EndWait.Describe(),
                LaterEnded, LaterEndedAt, ArmorClassAfterEnd);
        }
    }

    /// <summary>
    /// The scenario's releases at cleanup. Each is attempted on its own, so
    /// one that throws does not stop the rest, and every failure is kept.
    /// </summary>
    internal sealed class DodgeReleases
    {
        private readonly List<string> _attempted = new List<string>();
        private readonly List<string> _failures = new List<string>();

        internal IList<string> Attempted { get { return _attempted; } }
        internal IList<string> Failures { get { return _failures; } }

        internal void Run(string name, Action release)
        {
            if (release == null) throw new ArgumentNullException("release");
            _attempted.Add(name);
            try { release(); }
            catch (Exception error) { _failures.Add(name + ": " + error.Message); }
        }

        internal string Describe()
        {
            return "attempted=" + string.Join(",", _attempted.ToArray()) +
                ";releaseFailures=" + (_failures.Count == 0 ? "none" :
                    string.Join(" || ", _failures.ToArray()));
        }
    }

    /// <summary>What finishing the scenario did to a result.</summary>
    internal sealed class DodgeFinalization
    {
        internal bool Aborted;
        internal Exception DisposalFailure;
        internal int Attached;
    }

    /// <summary>
    /// The scenario's assertions and its running steps, and the one place
    /// that ends them. The runner may complete with a result of its own
    /// while the steps are still running (a save-guard failure, an exception
    /// in its own observation); that result then receives what the scenario
    /// had observed, the cleanup that ending the steps performs, and any
    /// exception that ending them raised. Ending and attaching are safe to
    /// repeat: the steps are disposed once and no assertion is added twice.
    /// </summary>
    internal sealed class DodgeScenarioSession<TAssertion>
    {
        private readonly Func<string, string, string, TAssertion> _failure;
        private readonly List<TAssertion> _assertions = new List<TAssertion>();
        private IDisposable _steps;
        private int _attached;

        /// <param name="failure">Builds a failing assertion from a name, what was expected and what was observed.</param>
        internal DodgeScenarioSession(Func<string, string, string, TAssertion> failure)
        {
            if (failure == null) throw new ArgumentNullException("failure");
            _failure = failure;
            Stage = "not-started";
        }

        internal List<TAssertion> Assertions { get { return _assertions; } }
        internal string Stage { get; set; }
        internal bool Active { get { return _steps != null; } }
        internal int Disposals { get; private set; }
        internal bool HasUnattached
        {
            get { return _steps != null || _attached < _assertions.Count; }
        }

        internal void Begin(IDisposable steps)
        {
            if (steps == null) throw new ArgumentNullException("steps");
            if (_steps != null) throw new InvalidOperationException(
                "The Dodge scenario's steps are already running.");
            _steps = steps;
        }

        /// <summary>
        /// Ends the running steps, which runs the scenario's cleanup. Returns
        /// the exception that raised, if any. Does nothing when no steps run.
        /// </summary>
        internal Exception End()
        {
            IDisposable steps = _steps;
            _steps = null;
            if (steps == null) return null;
            Disposals++;
            try
            {
                steps.Dispose();
                return null;
            }
            catch (Exception error) { return error; }
        }

        /// <summary>A result built on <see cref="Assertions"/> already holds everything so far.</summary>
        internal void MarkPublished()
        {
            _attached = _assertions.Count;
        }

        /// <summary>
        /// Called with every result the runner completes with. If the steps
        /// are still running the scenario was aborted: that is recorded, the
        /// steps are ended, and the result's own list receives the scenario's
        /// assertions after its own.
        /// </summary>
        internal DodgeFinalization Finish(IList<TAssertion> resultAssertions)
        {
            var finalization = new DodgeFinalization();
            if (_steps != null)
            {
                finalization.Aborted = true;
                _assertions.Add(_failure("dodge-scenario-aborted",
                    "the scenario runs to its own end",
                    "the runner completed with another result while the scenario was at stage " +
                        Stage));
                finalization.DisposalFailure = End();
                if (finalization.DisposalFailure != null)
                    _assertions.Add(_failure("dodge-cleanup-disposal",
                        "ending the scenario's steps raises nothing",
                        finalization.DisposalFailure.ToString()));
            }
            if (resultAssertions != null &&
                !ReferenceEquals(resultAssertions, _assertions))
                for (int index = _attached; index < _assertions.Count; index++)
                {
                    resultAssertions.Add(_assertions[index]);
                    finalization.Attached++;
                }
            _attached = _assertions.Count;
            return finalization;
        }
    }

    /// <summary>The scenario's own admission rule, applied before anything is loaded.</summary>
    internal static class GunslingerDodgeScenarioAdmission
    {
        internal const string Scenario = "disposable-gunslinger-dodge";
        internal const string ExitRequired = "gunslinger-dodge-exit-required";

        /// <summary>
        /// The reason a request is refused, or null. The scenario leaves a
        /// loaded game with time having run; it has no state to hand to a
        /// person, so it must exit when it completes.
        /// </summary>
        internal static string Rejection(string scenario, bool exitAfterCompletion)
        {
            return string.Equals(scenario, Scenario, StringComparison.Ordinal) &&
                !exitAfterCompletion ? ExitRequired : null;
        }
    }

    /// <summary>
    /// Verdicts for the Dodge scenario. Each returns null when the
    /// observation meets the contract, and otherwise the reason it does not.
    /// A reason that starts with <see cref="NotJudged"/> means a prerequisite
    /// was missing, which is a failure of the check and never a pass.
    /// </summary>
    internal static class GunslingerDodgeScenarioVerdict
    {
        internal const int Bonus = 2;
        internal const double RoundSeconds = 6d;
        internal const double RoundToleranceSeconds = 0.35d;
        internal const string NotJudged = "not judged: ";
        /// <summary>Game time allowed to pass after the earlier deadline before the unit is read.</summary>
        internal const double DeadlineMarginSeconds = 0.5d;

        /// <summary>
        /// An accepted activation: the command ran, its native delivery
        /// ended, exactly the cost was charged and exactly one new timed buff
        /// brought exactly the bonus.
        /// </summary>
        internal static string Accepted(DodgeCommandObservation use, int cost)
        {
            if (use == null) throw new ArgumentNullException("use");
            if (cost < 0) throw new ArgumentOutOfRangeException("cost");
            if (!use.AvailableBeforeUse)
                return "the ability read as unavailable before the command was issued";
            if (use.TimedOut)
                return "timed out waiting for the command (started=" + use.CommandStarted +
                    ", finished=" + use.CommandFinished + ", result=" + use.CommandResult +
                    ", execution process=" + use.HadExecutionProcess + ")";
            if (!use.HadExecutionProcess)
                return "the command ended (" + use.CommandResult +
                    ") before an execution process existed";
            if (use.CommandResult != "Success")
                return "the command ended " + use.CommandResult + ", not Success";
            if (!use.DeliveryEnded) return "the native delivery did not end";
            if (use.GritBefore - use.GritAfter != cost)
                return "grit went from " + use.GritBefore + " to " + use.GritAfter +
                    ", expected " + cost + " spent";
            if (use.BuffsBefore != 0 || use.BuffsAfter != 1 || !use.InstanceIsNew)
                return "Dodge buffs on the unit went from " + use.BuffsBefore + " to " +
                    use.BuffsAfter + " (new instance " + use.InstanceIsNew +
                    "), expected exactly one new buff";
            if (use.AddedNotices != 1)
                return "the game raised " + use.AddedNotices +
                    " buff-added notices, expected 1";
            if (!use.MarkedNotDispelable)
                return "the native apply-buff action did not receive the buff back";
            if (use.Permanent || use.TimeLeftSeconds <= 0d ||
                use.TimeLeftSeconds > RoundSeconds)
                return "the buff is not timed within one round (permanent=" +
                    use.Permanent + ", time left " + use.TimeLeftSeconds.ToString(
                        "0.##", CultureInfo.InvariantCulture) + " s)";
            if (use.ArmorClassAfter != use.ArmorClassBefore + Bonus)
                return "armor class went from " + use.ArmorClassBefore + " to " +
                    use.ArmorClassAfter + ", expected +" + Bonus;
            return null;
        }

        /// <summary>
        /// A command attempted while the ability is not available: whatever
        /// the command reports, nothing may be delivered, charged or applied.
        /// </summary>
        internal static string Refused(DodgeCommandObservation attempt)
        {
            if (attempt == null) throw new ArgumentNullException("attempt");
            if (attempt.AvailableBeforeUse)
                return NotJudged + "the ability read as available, so this was not a refusal case";
            if (attempt.TimedOut)
                return "the attempted command neither ran nor was refused before the wait ended";
            if (attempt.DeliveryEnded || attempt.CommandResult == "Success")
                return "the attempted command was carried out (result " +
                    attempt.CommandResult + ", delivery ended " + attempt.DeliveryEnded + ")";
            if (attempt.GritAfter != attempt.GritBefore)
                return "the refused attempt changed grit from " + attempt.GritBefore +
                    " to " + attempt.GritAfter;
            if (attempt.BuffsAfter != attempt.BuffsBefore || attempt.InstanceIsNew ||
                attempt.AddedNotices != 0)
                return "the refused attempt changed the Dodge buffs (" + attempt.BuffsBefore +
                    " to " + attempt.BuffsAfter + ", new instance " + attempt.InstanceIsNew +
                    ", added notices " + attempt.AddedNotices + ")";
            if (attempt.ArmorClassAfter != attempt.ArmorClassBefore)
                return "the refused attempt changed armor class from " +
                    attempt.ArmorClassBefore + " to " + attempt.ArmorClassAfter;
            return null;
        }

        /// <summary>
        /// The bonus as an attacker meets it: the same attack before, during
        /// and after. A dodge bonus is lost by a flat-footed target, so a roll
        /// that caught the target flat-footed cannot be read.
        /// </summary>
        internal static string IncomingAttacks(DodgeIncomingAttack before,
            DodgeIncomingAttack during, DodgeIncomingAttack after)
        {
            foreach (DodgeIncomingAttack roll in new[] { before, during, after })
            {
                if (roll == null || !roll.Resolved)
                    return NotJudged + "an incoming attack produced no attack roll";
                if (roll.TargetFlatFooted)
                    return NotJudged + "an incoming attack caught the target flat-footed";
            }
            if (during.TargetArmorClass != before.TargetArmorClass + Bonus)
                return "the attack during Dodge was resolved against " +
                    during.TargetArmorClass + ", before it against " +
                    before.TargetArmorClass + "; expected +" + Bonus;
            if (after.TargetArmorClass != before.TargetArmorClass)
                return "the attack after Dodge was resolved against " +
                    after.TargetArmorClass + ", before it against " +
                    before.TargetArmorClass;
            return null;
        }

        /// <summary>An unused activation ends by itself after one round and takes its bonus away once.</summary>
        internal static string Expiry(bool ended, double lastedSeconds,
            int removedNotices, int armorClassBase, int armorClassAfter,
            long bonusTurnedOn, long bonusTurnedOff, long activeBonusModifiers)
        {
            if (!ended) return "the buff did not end by itself";
            if (Math.Abs(lastedSeconds - RoundSeconds) > RoundToleranceSeconds)
                return "the buff lasted " + lastedSeconds.ToString("0.##",
                    CultureInfo.InvariantCulture) + " s of game time, expected one round";
            if (removedNotices != 1)
                return "the game raised " + removedNotices +
                    " buff-removed notices, expected 1";
            if (armorClassAfter != armorClassBase)
                return "armor class is " + armorClassAfter + " after the end, " +
                    armorClassBase + " before the activation";
            if (bonusTurnedOn != 1 || bonusTurnedOff != 1 || activeBonusModifiers != 0)
                return "the bonus was turned on " + bonusTurnedOn + " and off " +
                    bonusTurnedOff + " times and " + activeBonusModifiers +
                    " modifiers remain, expected 1, 1 and 0";
            return null;
        }

        /// <summary>
        /// Early removal and recast. The first three arguments are the
        /// prerequisites; without all of them the later activation's survival
        /// proves nothing and the check is not judged. Nor is it judged
        /// unless the measured times show the overlap it is about: the
        /// earlier instance removed before its own deadline, the later one
        /// applied before that deadline, and the unit read after it.
        /// </summary>
        internal static string Recast(string earlierActivation,
            string earlierRemoved, string laterActivation,
            DodgeRecastObservation timing, int armorClassBase)
        {
            if (timing == null) throw new ArgumentNullException("timing");
            var missing = new List<string>();
            if (earlierActivation != null)
                missing.Add("the earlier activation (" + earlierActivation + ")");
            if (earlierRemoved != null)
                missing.Add("the early removal (" + earlierRemoved + ")");
            if (laterActivation != null)
                missing.Add("the later activation (" + laterActivation + ")");
            if (missing.Count != 0)
                return NotJudged + string.Join("; ", missing.ToArray());
            if (!timing.EarlierDeadlineCaptured || timing.EarlierAddedAt < 0d ||
                timing.EarlierDeadline <= timing.EarlierAddedAt)
                return NotJudged + "the earlier instance's own deadline was not captured";
            if (timing.EarlierRemovedAt < timing.EarlierAddedAt ||
                timing.EarlierRemovedAt >= timing.EarlierDeadline)
                return NotJudged + "the earlier instance was not removed early (removed at " +
                    Seconds(timing.EarlierRemovedAt) + ", its deadline " +
                    Seconds(timing.EarlierDeadline) + ")";
            if (timing.LaterAddedAt < timing.EarlierRemovedAt ||
                timing.LaterAddedAt >= timing.EarlierDeadline)
                return NotJudged + "the later activation did not come between the removal and the earlier deadline (applied at " +
                    Seconds(timing.LaterAddedAt) + ", removal " +
                    Seconds(timing.EarlierRemovedAt) + ", deadline " +
                    Seconds(timing.EarlierDeadline) + "), so nothing overlapped";
            if (timing.DeadlineWait == null || !timing.DeadlineWait.Reached)
                return NotJudged + "the wait for the earlier deadline did not reach it (" +
                    (timing.DeadlineWait == null ? "not started" :
                        timing.DeadlineWait.Describe()) + ")";
            if (timing.SurvivalObservedAt < timing.EarlierDeadline + DeadlineMarginSeconds)
                return NotJudged + "the unit was read at " +
                    Seconds(timing.SurvivalObservedAt) + ", not at least " +
                    Seconds(DeadlineMarginSeconds) + " s after the earlier deadline " +
                    Seconds(timing.EarlierDeadline);
            if (!timing.LaterInstanceOnUnit)
                return "the later activation's instance was not on the unit once the earlier one's deadline had passed";
            if (timing.ArmorClassAtObservation != armorClassBase + Bonus)
                return "armor class was " + timing.ArmorClassAtObservation +
                    " after the earlier deadline, expected the later activation's +" + Bonus;
            if (timing.EndWait == null || !timing.EndWait.Reached || !timing.LaterEnded)
                return "the later activation did not end by itself (" +
                    (timing.EndWait == null ? "wait not started" :
                        timing.EndWait.Describe()) + ")";
            double lasted = timing.LaterEndedAt - timing.LaterAddedAt;
            if (Math.Abs(lasted - RoundSeconds) > RoundToleranceSeconds)
                return "the later activation lasted " + Seconds(lasted) +
                    " s of game time, expected one round of its own";
            if (timing.ArmorClassAfterEnd != armorClassBase)
                return "armor class is " + timing.ArmorClassAfterEnd +
                    " after the later activation ended, " + armorClassBase +
                    " before any";
            return null;
        }

        /// <summary>
        /// Every wait of the scenario must have ended because its condition
        /// held. One that ran out its real-time limit proves nothing about
        /// what it was waiting for.
        /// </summary>
        internal static string Waits(IEnumerable<DodgeWait> waits)
        {
            if (waits == null) throw new ArgumentNullException("waits");
            var unreached = new List<string>();
            foreach (DodgeWait wait in waits)
                if (wait == null || !wait.Reached)
                    unreached.Add(wait == null ? "<missing>" : wait.Describe());
            return unreached.Count == 0 ? null :
                "ended without their condition holding: " +
                    string.Join("; ", unreached.ToArray());
        }

        private static string Seconds(double value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }

        /// <summary>The early removal took away the earlier activation's own instance, once.</summary>
        internal static string EarlyRemoval(bool instanceWasTheEarlierOne,
            bool earlierInstanceGone, int removedNotices, int armorClassBase,
            int armorClassAfter)
        {
            if (!instanceWasTheEarlierOne)
                return "the buff on the unit was not the earlier activation's instance";
            if (!earlierInstanceGone) return "the earlier instance is still on the unit";
            if (removedNotices != 1)
                return "the removal raised " + removedNotices +
                    " buff-removed notices, expected 1";
            if (armorClassAfter != armorClassBase)
                return "armor class is " + armorClassAfter + " after the removal, " +
                    armorClassBase + " before the activation";
            return null;
        }
    }
}
