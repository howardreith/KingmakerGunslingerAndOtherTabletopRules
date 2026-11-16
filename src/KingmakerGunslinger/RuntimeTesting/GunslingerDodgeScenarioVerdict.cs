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
        /// proves nothing and the check is not judged.
        /// </summary>
        internal static string Recast(string earlierActivation,
            string earlierRemoved, string laterActivation,
            bool laterOnUnitAtEarlierDeadline, int armorClassAtEarlierDeadline,
            bool laterEnded, double laterLastedSeconds, int armorClassBase,
            int armorClassAfterEnd)
        {
            var missing = new List<string>();
            if (earlierActivation != null)
                missing.Add("the earlier activation (" + earlierActivation + ")");
            if (earlierRemoved != null)
                missing.Add("the early removal (" + earlierRemoved + ")");
            if (laterActivation != null)
                missing.Add("the later activation (" + laterActivation + ")");
            if (missing.Count != 0)
                return NotJudged + string.Join("; ", missing.ToArray());
            if (!laterOnUnitAtEarlierDeadline)
                return "the later activation was gone when the earlier one's deadline passed";
            if (armorClassAtEarlierDeadline != armorClassBase + Bonus)
                return "armor class was " + armorClassAtEarlierDeadline +
                    " at the earlier deadline, expected the later activation's +" + Bonus;
            if (!laterEnded) return "the later activation did not end by itself";
            if (Math.Abs(laterLastedSeconds - RoundSeconds) > RoundToleranceSeconds)
                return "the later activation lasted " + laterLastedSeconds.ToString(
                    "0.##", CultureInfo.InvariantCulture) +
                    " s of game time, expected one round of its own";
            if (armorClassAfterEnd != armorClassBase)
                return "armor class is " + armorClassAfterEnd +
                    " after the later activation ended, " + armorClassBase +
                    " before any";
            return null;
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
