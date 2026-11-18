using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>One spawn branch of a generated (owned) ability graph, as read from the registered blueprint.</summary>
    internal sealed class InheritedTemplateBranch
    {
        internal InheritedTemplateBranch(int lastNativeApplyIndex, int[] removalIndices, bool removalsBoundToPlacementUnit,
            bool removalIdentitiesExact, bool deliberateTemplateReplacesNative, int donorOrdinal = -1,
            bool shapeMatchesDonor = false)
        {
            LastNativeApplyIndex = lastNativeApplyIndex;
            RemovalIndices = removalIndices ?? new int[0];
            RemovalsBoundToPlacementUnit = removalsBoundToPlacementUnit;
            RemovalIdentitiesExact = removalIdentitiesExact;
            DeliberateTemplateReplacesNative = deliberateTemplateReplacesNative;
            DonorOrdinal = donorOrdinal;
            ShapeMatchesDonor = shapeMatchesDonor;
        }

        /// <summary>Index in the branch's after-spawn list of the last action that contains a native template application; -1 when none.</summary>
        internal int LastNativeApplyIndex { get; private set; }
        internal int[] RemovalIndices { get; private set; }
        internal bool RemovalsBoundToPlacementUnit { get; private set; }
        internal bool RemovalIdentitiesExact { get; private set; }
        /// <summary>A project template action, after the donor's applications, that names the four native identities it replaces.</summary>
        internal bool DeliberateTemplateReplacesNative { get; private set; }
        /// <summary>For a canonical clone: the donor spawn branch this generated branch was paired with; -1 when unpaired or not a clone.</summary>
        internal int DonorOrdinal { get; private set; }
        /// <summary>The generated branch's after-spawn list starts with exactly the donor branch's action types, in order.</summary>
        internal bool ShapeMatchesDonor { get; private set; }
    }

    /// <summary>
    /// One published ability for one placement and one caller. Intent (family,
    /// templating) comes from the catalog; the donor's identity, branches and
    /// applications from the recorded donor carrier; branches from the
    /// generated graph. Expected results are stated here, not taken from
    /// InheritedTemplatePolicy, and the donor itself is checked against the
    /// independent InheritedTemplateDonorContract.
    /// </summary>
    internal sealed class InheritedTemplateObservation
    {
        internal InheritedTemplateObservation(string key, string caller, SummonFamily family, bool monsterTemplated,
            bool deliberateTemplateAbility, string[] donorAppliedTemplateIds, InheritedTemplateBranch[] branches,
            int parentTier = 0, SummonMultiplicity multiplicity = SummonMultiplicity.One, string donorGuid = null,
            string donorName = null, bool[] donorBranchApplies = null, string[] donorReceiptDifferences = null)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("key");
            if (string.IsNullOrEmpty(caller)) throw new ArgumentException("caller");
            Key = key; Caller = caller; Family = family; MonsterTemplated = monsterTemplated;
            DeliberateTemplateAbility = deliberateTemplateAbility;
            DonorAppliedTemplateIds = donorAppliedTemplateIds ?? new string[0];
            Branches = branches ?? new InheritedTemplateBranch[0];
            ParentTier = parentTier; Multiplicity = multiplicity; DonorGuid = donorGuid; DonorName = donorName;
            DonorBranchApplies = donorBranchApplies;
            DonorReceiptDifferences = donorReceiptDifferences ?? new string[0];
        }

        internal string Key { get; private set; }
        /// <summary>"canonical", or "alternate:" plus the alternate family key.</summary>
        internal string Caller { get; private set; }
        internal SummonFamily Family { get; private set; }
        internal bool MonsterTemplated { get; private set; }
        internal bool DeliberateTemplateAbility { get; private set; }
        internal string[] DonorAppliedTemplateIds { get; private set; }
        internal InheritedTemplateBranch[] Branches { get; private set; }
        internal int ParentTier { get; private set; }
        internal SummonMultiplicity Multiplicity { get; private set; }
        internal string DonorGuid { get; private set; }
        internal string DonorName { get; private set; }
        /// <summary>Per donor spawn branch, whether it applies a native template; null for an alternate child, whose graph is the caller's host graph, not a donor clone.</summary>
        internal bool[] DonorBranchApplies { get; private set; }
        internal string[] DonorReceiptDifferences { get; private set; }
        internal bool IsAlternate { get { return Caller.StartsWith("alternate:", StringComparison.Ordinal); } }
        internal bool IsClone { get { return DonorBranchApplies != null; } }
    }

    internal sealed class InheritedTemplateCensusResult
    {
        internal InheritedTemplateCensusResult(IList<string> findings, IList<string> justifiedRedundant,
            IDictionary<InheritedTemplateDisposition, int> dispositions, int observations, IList<string> notRetainedShapes)
        {
            Findings = findings; JustifiedRedundant = justifiedRedundant; Dispositions = dispositions;
            Observations = observations; NotRetainedShapes = notRetainedShapes;
        }

        internal IList<string> Findings { get; private set; }
        internal IList<string> JustifiedRedundant { get; private set; }
        internal IDictionary<InheritedTemplateDisposition, int> Dispositions { get; private set; }
        internal int Observations { get; private set; }
        /// <summary>Summon Monster donor shapes whose template signature the retained census never observed: recorded now, not proved.</summary>
        internal IList<string> NotRetainedShapes { get; private set; }
        internal bool Passed { get { return Findings.Count == 0; } }
    }

    /// <summary>
    /// Judges observed graphs against independently derived intent. Any finding
    /// fails the census:
    /// skipped, duplicate (observation), no-spawn-branch;
    /// wrong-donor (InheritedTemplateDonorContract), shape-donor-inconsistent
    /// (one shape, several donors), shapes-share-donor (two quantities of a tier
    /// on one donor), donor-mutated (receipt difference);
    /// branch-count, branch-unpaired, branch-duplicate-pairing, branch-skipped
    /// (canonical clone branches against donor branches);
    /// omitted, duplicate-removal, wrong-owner, identities, before-donor,
    /// application-lost (a donor branch's native application missing from its
    /// clone), application-not-in-donor, extra, unremoved-application,
    /// deliberate-template-missing, ally-native-template.
    /// A removal with nothing to remove on its branch is listed in
    /// JustifiedRedundant with its reason, for any caller.
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
            IEnumerable<string> expectedKeys, ICollection<string> nativeTemplateIds, bool checkDonors = false)
        {
            if (observations == null) throw new ArgumentNullException("observations");
            if (expectedKeys == null) throw new ArgumentNullException("expectedKeys");
            var findings = new List<string>();
            var redundant = new List<string>();
            var notRetained = new SortedSet<string>(StringComparer.Ordinal);
            var dispositions = new Dictionary<InheritedTemplateDisposition, int>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var donorByShape = new Dictionary<string, string>(StringComparer.Ordinal);
            int count = 0;
            foreach (InheritedTemplateObservation value in observations)
            {
                count++;
                string id = value.Caller + "|" + value.Key;
                if (!seen.Add(id)) { findings.Add(id + ":duplicate"); continue; }
                InheritedTemplateDisposition expected = Expected(value, nativeTemplateIds);
                dispositions[expected] = dispositions.ContainsKey(expected) ? dispositions[expected] + 1 : 1;
                if (checkDonors)
                {
                    bool retained;
                    string donor = InheritedTemplateDonorContract.Check(value.Family, value.ParentTier,
                        value.Multiplicity, value.DonorName, value.DonorGuid, value.DonorAppliedTemplateIds, out retained);
                    if (donor != null) findings.Add(id + ":wrong-donor:" + donor);
                    string shape = value.Family + "." + value.ParentTier + "." + value.Multiplicity;
                    if (!retained && donor == null) notRetained.Add(shape + "=" + string.Join(",", value.DonorAppliedTemplateIds));
                    string known;
                    if (value.DonorGuid != null)
                    {
                        if (donorByShape.TryGetValue(shape, out known) && known != value.DonorGuid)
                            findings.Add(id + ":shape-donor-inconsistent:" + known + "/" + value.DonorGuid);
                        else donorByShape[shape] = value.DonorGuid;
                    }
                    foreach (string difference in value.DonorReceiptDifferences)
                        findings.Add(id + ":donor-mutated:" + difference);
                }
                if (value.Branches.Length == 0) { findings.Add(id + ":no-spawn-branch"); continue; }
                if (expected == InheritedTemplateDisposition.OutsideSummonMonster &&
                    value.DonorAppliedTemplateIds.Any(nativeTemplateIds.Contains))
                    findings.Add(id + ":ally-native-template");
                if (value.IsClone) CheckPairing(value, id, findings);
                for (int index = 0; index < value.Branches.Length; index++)
                    CheckBranch(value, expected, value.Branches[index], id + ":branch" + index + ":", findings, redundant);
            }
            if (checkDonors)
                foreach (var tier in donorByShape.GroupBy(entry => entry.Key.Substring(0, entry.Key.LastIndexOf('.'))))
                    foreach (var shared in tier.GroupBy(entry => entry.Value).Where(group => group.Count() > 1))
                        findings.Add(tier.Key + ":shapes-share-donor:" + shared.Key);
            foreach (string key in expectedKeys)
                if (!seen.Contains(key)) findings.Add(key + ":skipped");
            return new InheritedTemplateCensusResult(findings, redundant, dispositions, count, notRetained.ToList());
        }

        private static void CheckPairing(InheritedTemplateObservation value, string id, IList<string> findings)
        {
            int donors = value.DonorBranchApplies.Length;
            if (value.Branches.Length != donors)
                findings.Add(id + ":branch-count:generated=" + value.Branches.Length + ";donor=" + donors);
            var used = new HashSet<int>();
            for (int index = 0; index < value.Branches.Length; index++)
            {
                InheritedTemplateBranch branch = value.Branches[index];
                if (branch.DonorOrdinal < 0 || branch.DonorOrdinal >= donors || !branch.ShapeMatchesDonor)
                    findings.Add(id + ":branch" + index + ":branch-unpaired");
                else if (!used.Add(branch.DonorOrdinal))
                    findings.Add(id + ":branch" + index + ":branch-duplicate-pairing:" + branch.DonorOrdinal);
            }
            for (int ordinal = 0; ordinal < donors; ordinal++)
                if (!used.Contains(ordinal)) findings.Add(id + ":donor-branch" + ordinal + ":branch-skipped");
        }

        private static void CheckBranch(InheritedTemplateObservation value, InheritedTemplateDisposition expected,
            InheritedTemplateBranch branch, string where, IList<string> findings, IList<string> redundant)
        {
            bool removes = branch.RemovalIndices.Length > 0;
            bool paired = value.IsClone && branch.DonorOrdinal >= 0 && branch.DonorOrdinal < value.DonorBranchApplies.Length;
            bool donorApplies = paired && value.DonorBranchApplies[branch.DonorOrdinal];
            if (paired && !donorApplies && branch.LastNativeApplyIndex >= 0)
                findings.Add(where + "application-not-in-donor");
            if (expected != InheritedTemplateDisposition.RemoveInherited)
            {
                if (removes) findings.Add(where + "extra");
                if (expected == InheritedTemplateDisposition.PreserveDeliberateTemplate)
                {
                    if (value.DeliberateTemplateAbility && !branch.DeliberateTemplateReplacesNative)
                        findings.Add(where + "deliberate-template-missing");
                }
                else if (branch.LastNativeApplyIndex >= 0) findings.Add(where + "unremoved-application");
                return;
            }
            if (donorApplies && branch.LastNativeApplyIndex < 0) findings.Add(where + "application-lost");
            if (!removes) { findings.Add(where + "omitted"); return; }
            if (branch.RemovalIndices.Length > 1) findings.Add(where + "duplicate-removal");
            if (!branch.RemovalsBoundToPlacementUnit) findings.Add(where + "wrong-owner");
            if (!branch.RemovalIdentitiesExact) findings.Add(where + "identities");
            if (branch.LastNativeApplyIndex >= 0 && branch.RemovalIndices.Min() <= branch.LastNativeApplyIndex)
                findings.Add(where + "before-donor");
            if (branch.LastNativeApplyIndex < 0)
                redundant.Add(where + (value.IsAlternate ? "mirrored-canonical-removal;alternate-host-applies-none" :
                    "placement-removal-on-donor-branch-without-application"));
        }
    }
}
