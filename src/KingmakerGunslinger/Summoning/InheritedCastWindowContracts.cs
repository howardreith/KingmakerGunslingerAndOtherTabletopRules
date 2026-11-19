using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>One native die roll by the cast's exact caster, as the cast observer saw it.</summary>
    internal sealed class InheritedDieRoll
    {
        internal InheritedDieRoll(string die, int rolls, int result, bool overridden)
        {
            Die = die; Rolls = rolls; Result = result; Overridden = overridden;
        }

        internal string Die { get; private set; }
        internal int Rolls { get; private set; }
        internal int Result { get; private set; }
        internal bool Overridden { get; private set; }
    }

    internal enum InheritedQuantityVerdict
    {
        Pass = 1,
        /// <summary>The roll was controlled and the rank correct, but the count omits the bonus: a gameplay defect.</summary>
        ProductFailure = 2,
        /// <summary>The die roll was missing, repeated, unforced or of the wrong form: the observation cannot decide.</summary>
        ObserverFailure = 3,
        Failure = 4
    }

    internal sealed class InheritedControlledQuantityResult
    {
        internal InheritedQuantityVerdict Verdict;
        internal int ExpectedCount;
        internal IList<string> Failures = new List<string>();
    }

    /// <summary>
    /// DEF-05 cast observer correction 2: a d4+1 quantity proved with one controlled die. The expected count is
    /// the forced die plus the independently expected rank, never the observed output.
    /// </summary>
    internal static class InheritedControlledQuantityContract
    {
        internal static InheritedControlledQuantityResult Judge(bool contextRecalculated, int expectedRank, int recalculatedRank,
            string expectedDie, int forcedValue, IList<InheritedDieRoll> relevantRolls,
            string expectedUnit, IList<string> unitBlueprints, IList<string> unitIds, IList<bool> live)
        {
            var result = new InheritedControlledQuantityResult { ExpectedCount = forcedValue + expectedRank };
            IList<string> failures = result.Failures;
            relevantRolls = relevantRolls ?? new List<InheritedDieRoll>();
            if (relevantRolls.Count == 0) failures.Add("die-roll-missing");
            else if (relevantRolls.Count > 1) failures.Add("die-rolls:" + relevantRolls.Count);
            else
            {
                InheritedDieRoll roll = relevantRolls[0];
                if (roll.Die != expectedDie || roll.Rolls != 1) failures.Add("die-formula:" + roll.Rolls + roll.Die + "/1" + expectedDie);
                if (!roll.Overridden || roll.Result != forcedValue) failures.Add("die-not-forced:" + roll.Result + "/" + forcedValue);
            }
            if (failures.Count > 0) { result.Verdict = InheritedQuantityVerdict.ObserverFailure; return result; }
            if (!contextRecalculated) failures.Add("rank-not-recalculated");
            if (recalculatedRank != expectedRank) failures.Add("rank:" + recalculatedRank + "/" + expectedRank);
            IList<string> output = InheritedQuantityContract.Output(result.ExpectedCount, result.ExpectedCount, expectedUnit,
                unitBlueprints, unitIds, live);
            int count = unitBlueprints == null ? 0 : unitBlueprints.Count;
            if (failures.Count == 0 && count == forcedValue && output.All(value => value.StartsWith("quantity:", StringComparison.Ordinal)))
            {
                failures.Add("quantity-omits-bonus:" + count + "/" + result.ExpectedCount);
                result.Verdict = InheritedQuantityVerdict.ProductFailure;
                return result;
            }
            foreach (string value in output) failures.Add(value);
            result.Verdict = failures.Count == 0 ? InheritedQuantityVerdict.Pass : InheritedQuantityVerdict.Failure;
            return result;
        }
    }

    /// <summary>The fields of one ContextRankConfig, read from the engine.</summary>
    internal sealed class InheritedRankConfigView
    {
        internal string Type { get; set; }
        internal string BaseValueType { get; set; }
        internal string Progression { get; set; }
        internal string Feature { get; set; }
        internal string[] Classes { get; set; } = new string[0];
        internal bool ExceptClasses { get; set; }
        internal int StartLevel { get; set; }
        internal int StepLevel { get; set; }
        internal bool UseMin { get; set; }
        internal int Min { get; set; }
        internal bool UseMax { get; set; }
        internal int Max { get; set; }
    }

    /// <summary>
    /// DEF-05 cast observer correction 3: the Master Summoner duration. The structural expectation comes from the
    /// untouched host row (Minutes, bonus Rank(Default), Default ranked by Summoner class level) and the caster's
    /// class level; the native value is the recalculated execution context's Default rank. Neither comes from the
    /// spawned unit's observed duration.
    /// </summary>
    internal static class InheritedClassRankDurationContract
    {
        private static readonly string[] ClassLevelBases =
            { "ClassLevel", "SummClassLevelWithArchetype", "MaxClassLevelWithArchetype" };

        internal static IList<string> Judge(string durationRate, string durationDice, string bonusKind, string bonusRank,
            InheritedRankConfigView hostDefault, string summonerClass, int classLevel, int expectedClassLevel,
            bool contextRecalculated, int contextRank, IList<string> spawnedUnitIds,
            IList<Tuple<string, double, double>> observations, out int expectedRank, out double expectedSeconds)
        {
            var failures = new List<string>();
            expectedRank = -1; expectedSeconds = -1;
            if (durationRate != "Minutes") failures.Add("structure:rate:" + durationRate);
            if (durationDice != "Zero") failures.Add("structure:dice:" + durationDice);
            if (bonusKind != "Rank" || bonusRank != "Default") failures.Add("structure:bonus:" + bonusKind + ":" + bonusRank);
            if (hostDefault == null) failures.Add("structure:no-default-rank-config");
            else
            {
                if (hostDefault.Type != "Default") failures.Add("structure:rank-type:" + hostDefault.Type);
                if (!ClassLevelBases.Contains(hostDefault.BaseValueType)) failures.Add("structure:base:" + hostDefault.BaseValueType);
                if (hostDefault.ExceptClasses || !(hostDefault.Classes ?? new string[0]).Contains(summonerClass))
                    failures.Add("structure:classes");
                if (hostDefault.Progression != "AsIs") failures.Add("structure:progression:" + hostDefault.Progression);
            }
            if (classLevel != expectedClassLevel) failures.Add("class-level:" + classLevel + "/" + expectedClassLevel);
            if (failures.Count > 0) return failures;
            int rank = classLevel;
            if (hostDefault.UseMin) rank = Math.Max(rank, hostDefault.Min);
            if (hostDefault.UseMax) rank = Math.Min(rank, hostDefault.Max);
            expectedRank = rank;
            expectedSeconds = 60d * rank;
            if (!contextRecalculated) failures.Add("rank-not-recalculated");
            if (contextRank != expectedRank) failures.Add("context-rank:" + contextRank + "/" + expectedRank);
            failures.AddRange(InheritedDurationContract.Judge(spawnedUnitIds, observations, expectedSeconds, 0));
            return failures;
        }
    }

    /// <summary>One recorded donor carrier at one side of the cast window.</summary>
    internal sealed class InheritedDonorWindowEntry
    {
        internal InheritedDonorWindowEntry(string guid, string disposition, bool withheld,
            InheritedTemplateDonorReceipt receipt, IList<string> findings)
        {
            Guid = guid; Disposition = disposition; Withheld = withheld; Receipt = receipt;
            Findings = findings ?? new List<string>();
        }

        internal string Guid { get; private set; }
        /// <summary>The census donor verdict: "unchanged", "approved-tier-one-publication" or "mutated".</summary>
        internal string Disposition { get; private set; }
        internal bool Withheld { get; private set; }
        /// <summary>The current post-publication receipt of the live carrier.</summary>
        internal InheritedTemplateDonorReceipt Receipt { get; private set; }
        internal IList<string> Findings { get; private set; }
    }

    /// <summary>
    /// DEF-05 cast observer correction 4: the donor carriers across the cast window. Each side must pass the
    /// census donor validation; then every carrier's post-publication receipt is compared before and after,
    /// field by field, and must not differ. Pre-existing approved transitions are not cast mutations.
    /// </summary>
    internal static class InheritedDonorWindowContract
    {
        internal static IList<string> Validation(string phase, IList<InheritedDonorWindowEntry> entries,
            int expectedCarriers, int expectedApproved)
        {
            var failures = new List<string>();
            if (entries == null) { failures.Add(phase + ":missing"); return failures; }
            int unchanged = entries.Count(value => value.Disposition == "unchanged");
            int approved = entries.Count(value => value.Disposition == "approved-tier-one-publication");
            int withheld = entries.Count(value => value.Disposition == "approved-tier-one-publication" && value.Withheld);
            int other = entries.Count - unchanged - approved;
            if (entries.Count != expectedCarriers) failures.Add(phase + ":carriers:" + entries.Count + "/" + expectedCarriers);
            if (unchanged != expectedCarriers - expectedApproved)
                failures.Add(phase + ":unchanged:" + unchanged + "/" + (expectedCarriers - expectedApproved));
            if (approved != expectedApproved) failures.Add(phase + ":approved:" + approved + "/" + expectedApproved);
            if (withheld != expectedApproved) failures.Add(phase + ":withheld:" + withheld + "/" + expectedApproved);
            if (other != 0) failures.Add(phase + ":mutated:" + other);
            foreach (string guid in entries.GroupBy(value => value.Guid, StringComparer.Ordinal)
                .Where(group => group.Count() > 1).Select(group => group.Key))
                failures.Add(phase + ":duplicate-carrier:" + guid);
            foreach (InheritedDonorWindowEntry entry in entries)
                foreach (string finding in entry.Findings) failures.Add(phase + ":" + entry.Guid + ":" + finding);
            return failures;
        }

        /// <summary>Exact before/after receipt differences per carrier ("guid:field"), plus disposition changes.</summary>
        internal static IList<string> Compare(IList<InheritedDonorWindowEntry> before, IList<InheritedDonorWindowEntry> after)
        {
            var failures = new List<string>();
            if (before == null || after == null) { failures.Add("window:missing"); return failures; }
            var afterByGuid = after.GroupBy(value => value.Guid, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
            foreach (InheritedDonorWindowEntry was in before)
            {
                InheritedDonorWindowEntry now;
                if (!afterByGuid.TryGetValue(was.Guid, out now)) { failures.Add(was.Guid + ":carrier-missing-after"); continue; }
                if (was.Disposition != now.Disposition || was.Withheld != now.Withheld)
                    failures.Add(was.Guid + ":disposition:" + was.Disposition + "->" + now.Disposition);
                if (was.Receipt == null || now.Receipt == null) { failures.Add(was.Guid + ":receipt-missing"); continue; }
                foreach (string difference in InheritedTemplateDonorReceipt.Differences(was.Receipt, now.Receipt))
                    failures.Add(was.Guid + ":" + difference);
            }
            foreach (InheritedDonorWindowEntry now in after.Where(value => !before.Any(was => was.Guid == value.Guid)))
                failures.Add(now.Guid + ":carrier-new-after");
            return failures;
        }
    }
}
