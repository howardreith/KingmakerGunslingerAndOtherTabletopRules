using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>One spawn branch of a generated (owned) ability graph, as read from the registered blueprint.</summary>
    internal sealed class InheritedTemplateBranch
    {
        internal InheritedTemplateBranch(int lastNativeApplyIndex, int[] removalIndices, bool removalsBoundToPlacementUnit,
            bool removalIdentitiesExact, bool deliberateTemplateReplacesNative)
        {
            LastNativeApplyIndex = lastNativeApplyIndex;
            RemovalIndices = removalIndices ?? new int[0];
            RemovalsBoundToPlacementUnit = removalsBoundToPlacementUnit;
            RemovalIdentitiesExact = removalIdentitiesExact;
            DeliberateTemplateReplacesNative = deliberateTemplateReplacesNative;
        }

        /// <summary>Index in the branch's after-spawn list of the last action that contains a native template application; -1 when none.</summary>
        internal int LastNativeApplyIndex { get; private set; }
        internal int[] RemovalIndices { get; private set; }
        internal bool RemovalsBoundToPlacementUnit { get; private set; }
        internal bool RemovalIdentitiesExact { get; private set; }
        /// <summary>A project template action on this branch that names the four native identities it replaces.</summary>
        internal bool DeliberateTemplateReplacesNative { get; private set; }
    }

    /// <summary>
    /// One published ability for one placement and one caller. Intent (family,
    /// templating) comes from the catalog; donor applications come from the
    /// donor carrier's graph; branches come from the generated graph. The three
    /// inputs are read separately, so the census does not take its expected
    /// result from the generated graph it judges.
    /// </summary>
    internal sealed class InheritedTemplateObservation
    {
        internal InheritedTemplateObservation(string key, string caller, SummonFamily family, bool monsterTemplated,
            bool deliberateTemplateAbility, string[] donorAppliedTemplateIds, InheritedTemplateBranch[] branches)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("key");
            if (string.IsNullOrEmpty(caller)) throw new ArgumentException("caller");
            Key = key; Caller = caller; Family = family; MonsterTemplated = monsterTemplated;
            DeliberateTemplateAbility = deliberateTemplateAbility;
            DonorAppliedTemplateIds = donorAppliedTemplateIds ?? new string[0];
            Branches = branches ?? new InheritedTemplateBranch[0];
        }

        internal string Key { get; private set; }
        /// <summary>"canonical", or "alternate:" plus the alternate family key.</summary>
        internal string Caller { get; private set; }
        internal SummonFamily Family { get; private set; }
        internal bool MonsterTemplated { get; private set; }
        /// <summary>The celestial or fiendish ability of a templated placement (or its caster-chosen dynamic ability).</summary>
        internal bool DeliberateTemplateAbility { get; private set; }
        internal string[] DonorAppliedTemplateIds { get; private set; }
        internal InheritedTemplateBranch[] Branches { get; private set; }
        internal bool IsAlternate { get { return Caller.StartsWith("alternate:", StringComparison.Ordinal); } }
    }

    internal sealed class InheritedTemplateCensusResult
    {
        internal InheritedTemplateCensusResult(IList<string> findings, IList<string> justifiedRedundant,
            IDictionary<InheritedTemplateDisposition, int> dispositions, int observations)
        {
            Findings = findings; JustifiedRedundant = justifiedRedundant; Dispositions = dispositions;
            Observations = observations;
        }

        internal IList<string> Findings { get; private set; }
        internal IList<string> JustifiedRedundant { get; private set; }
        internal IDictionary<InheritedTemplateDisposition, int> Dispositions { get; private set; }
        internal int Observations { get; private set; }
        internal bool Passed { get { return Findings.Count == 0; } }
    }

    /// <summary>
    /// Judges observed graphs against independently derived placement intent.
    /// Findings (any one fails the census): skipped (an expected placement and
    /// caller with no observation), duplicate, no-spawn-branch, omitted
    /// (inherited template not removed), extra (removal where none is wanted),
    /// before-donor (removal precedes a donor application in its branch),
    /// wrong-owner (removal bound to another unit), identities (not exactly the
    /// native identities), deliberate-template-missing, unremoved-application (a
    /// generated branch applies a native template nothing removes: a wrong donor), ally-native-template
    /// (a Nature's Ally donor applying a native template: out of contract).
    /// A mirrored removal on an alternate child whose own graph applies nothing
    /// is listed separately as justified redundancy, never silently accepted.
    /// </summary>
    internal static class InheritedTemplateCensus
    {
        /// <summary>The approved contract, stated independently of InheritedTemplatePolicy.</summary>
        internal static InheritedTemplateDisposition Expected(InheritedTemplateObservation value,
            ICollection<string> nativeTemplateIds)
        {
            if (value.Family == SummonFamily.NaturesAlly) return InheritedTemplateDisposition.OutsideSummonMonster;
            if (value.MonsterTemplated) return InheritedTemplateDisposition.PreserveDeliberateTemplate;
            foreach (string applied in value.DonorAppliedTemplateIds)
                if (nativeTemplateIds.Contains(applied)) return InheritedTemplateDisposition.RemoveInherited;
            return InheritedTemplateDisposition.NoInheritedTemplate;
        }

        internal static InheritedTemplateCensusResult Evaluate(IEnumerable<InheritedTemplateObservation> observations,
            IEnumerable<string> expectedKeys, ICollection<string> nativeTemplateIds)
        {
            if (observations == null) throw new ArgumentNullException("observations");
            if (expectedKeys == null) throw new ArgumentNullException("expectedKeys");
            var findings = new List<string>();
            var redundant = new List<string>();
            var dispositions = new Dictionary<InheritedTemplateDisposition, int>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            int count = 0;
            foreach (InheritedTemplateObservation value in observations)
            {
                count++;
                string id = value.Caller + "|" + value.Key;
                if (!seen.Add(id)) { findings.Add(id + ":duplicate"); continue; }
                // Expected disposition from catalog intent and the donor graph, never from the
                // generated graph, and restated here rather than taken from InheritedTemplatePolicy
                // so a policy defect cannot certify itself.
                InheritedTemplateDisposition expected = Expected(value, nativeTemplateIds);
                dispositions[expected] = dispositions.ContainsKey(expected) ? dispositions[expected] + 1 : 1;
                if (value.Branches.Length == 0) { findings.Add(id + ":no-spawn-branch"); continue; }
                if (expected == InheritedTemplateDisposition.OutsideSummonMonster &&
                    value.DonorAppliedTemplateIds.Any(nativeTemplateIds.Contains))
                    findings.Add(id + ":ally-native-template");
                for (int index = 0; index < value.Branches.Length; index++)
                {
                    InheritedTemplateBranch branch = value.Branches[index];
                    string where = id + ":branch" + index + ":";
                    bool removes = branch.RemovalIndices.Length > 0;
                    if (expected == InheritedTemplateDisposition.RemoveInherited)
                    {
                        if (!removes) { findings.Add(where + "omitted"); continue; }
                        if (branch.RemovalIndices.Length > 1) findings.Add(where + "duplicate-removal");
                        if (!branch.RemovalsBoundToPlacementUnit) findings.Add(where + "wrong-owner");
                        if (!branch.RemovalIdentitiesExact) findings.Add(where + "identities");
                        if (branch.LastNativeApplyIndex >= 0 && branch.RemovalIndices.Min() <= branch.LastNativeApplyIndex)
                            findings.Add(where + "before-donor");
                        if (value.IsAlternate && branch.LastNativeApplyIndex < 0)
                            redundant.Add(where + "mirrored-canonical-removal;own-graph-applies-none");
                    }
                    else
                    {
                        if (removes) findings.Add(where + "extra");
                        if (expected == InheritedTemplateDisposition.PreserveDeliberateTemplate)
                        {
                            if (value.DeliberateTemplateAbility && !branch.DeliberateTemplateReplacesNative)
                                findings.Add(where + "deliberate-template-missing");
                        }
                        else if (branch.LastNativeApplyIndex >= 0)
                            // The generated graph applies a native template its expected disposition
                            // does not remove: a wrong donor, or an ally graph out of contract.
                            findings.Add(where + "unremoved-application");
                    }
                }
            }
            foreach (string key in expectedKeys)
                if (!seen.Contains(key)) findings.Add(key + ":skipped");
            return new InheritedTemplateCensusResult(findings, redundant, dispositions, count);
        }
    }
}
