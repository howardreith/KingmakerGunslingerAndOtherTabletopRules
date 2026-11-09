using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    internal enum SummonForeignContractOutcome { VerifiedInherentlyLegal, ConditionallyGoverned, Unresolved }

    /// <summary>The exact loaded shape one supported third-party row must have.</summary>
    internal sealed class SummonForeignRowContract
    {
        internal string Identity, InternalName, DirectUnit, EvilUnit, NonEvilUnit, EvilCreatureKey, NonEvilCreatureKey, TemplateEvilBuff, TemplateOtherBuff;
        internal int Tier;
        internal SummonFamily Family;
        internal SummonMultiplicity Multiplicity;
        internal bool Branched { get { return EvilUnit != null; } }
    }

    /// <summary>What a loaded row actually is, read from the engine objects.</summary>
    internal sealed class SummonForeignRowReading
    {
        internal string Identity, InternalName;
        internal int PublishedTier;
        internal IReadOnlyList<string> RequiredFacts = new string[0], VisibilityFacts = new string[0];
        internal SummonAlignmentAxes LiveDescriptors;
        internal SummonSignature Graph;
    }

    internal sealed class SummonForeignRowResult
    {
        internal SummonForeignContractOutcome Outcome;
        internal string Reason;
        internal SummonForeignRowContract Contract;
        private SummonSpellAlignment _direct, _evil, _nonEvil;

        internal static SummonForeignRowResult Unresolved(SummonForeignRowContract contract, string reason)
        { return new SummonForeignRowResult { Outcome = SummonForeignContractOutcome.Unresolved, Contract = contract, Reason = reason }; }

        internal static SummonForeignRowResult Inherent(SummonForeignRowContract contract, SummonSpellAlignment alignment)
        { return new SummonForeignRowResult { Outcome = SummonForeignContractOutcome.VerifiedInherentlyLegal, Contract = contract, Reason = "verified-direct-contract", _direct = alignment }; }

        internal static SummonForeignRowResult Repaired(SummonForeignRowContract contract, SummonSpellAlignment alignment)
        { return new SummonForeignRowResult { Outcome = SummonForeignContractOutcome.VerifiedInherentlyLegal, Contract = contract, Reason = "verified-repaired-spider-contract", _direct = alignment }; }

        internal static SummonForeignRowResult Governed(SummonForeignRowContract contract, SummonSpellAlignment evil, SummonSpellAlignment nonEvil)
        { return new SummonForeignRowResult { Outcome = SummonForeignContractOutcome.ConditionallyGoverned, Contract = contract, Reason = "verified-caster-evil-branch-contract", _evil = evil, _nonEvil = nonEvil }; }

        /// <summary>The spell this caster's own branch executes, never a union
        /// of branches. The row's verified condition reads the caster's Evil
        /// component. An unresolved contract has no alignment to report.</summary>
        internal SummonSpellAlignment AlignmentFor(SummonAlignmentAxes caster)
        {
            if (Outcome == SummonForeignContractOutcome.Unresolved) return null;
            if (_direct != null) return _direct;
            return (caster & SummonAlignmentAxes.Evil) != 0 ? _evil : _nonEvil;
        }
    }

    /// <summary>
    /// The eight RacesUnleashed spider rows, classified by what the loaded
    /// graph does. A name never classifies a row and a change to any checked
    /// part leaves it unresolved. A row is either the known input to the
    /// integration's in-place repair or that repair's exact result.
    /// </summary>
    internal static class SummonForeignRowClassifier
    {
        internal const string SpiderSummonerFeatureGuid = "bd03ca4637e74d4caefc66961093f0a0";
        internal const string RedcapUnit = "af8bcff074b93c84b97938e486184222", BralaniUnit = "58574e8d1d4dc464c976f396d9115b1a";
        internal const string CrabSpiderUnit = "b7e1252281ee46ea8dcd045aa1e56ee9", BlackWidowUnit = "9b1fda47e300491ab44011be9f8ef755",
            OgreSpiderUnit = "4a205de6d159483d99eba5aa05dd8f48", TarantulaUnit = "2ea0fc9b62634a1b99073119c02e673f";
        internal const string CasterEvilCondition = "Kingmaker.UnitLogic.Mechanics.Conditions.ContextConditionAlignment{Alignment=Evil;CheckCaster=True;Not=False}";
        private const string CasterHasFactFormat = "conditional:And[Kingmaker.UnitLogic.Mechanics.Conditions.ContextConditionCasterHasFact{{Fact={0};Not=False}}]{{t:apply-buff:{1}}}{{f:}}";
        internal const string FiendishTemplateI = "c707fdbe58d0c614a89a872b76777f4b", CelestialTemplateI = "83a8a909e7ad19b4fa57e306884cc8bd", CelestialTemplateII = "ec906031bb8766f42a37a52cf1d89836";
        internal const string TemplateAction = "KingmakerGunslinger.Summoning.ContextActionApplySummonTemplateByCaster";
        internal const string AlignmentAction = "KingmakerGunslinger.Summoning.ContextActionSetSummonAlignment";
        internal const string BenefitAction = "KingmakerGunslinger.Summoning.ContextActionApplySpiderSummonerBenefit";

        private static readonly string[] MonsterSummonBuffs = {
            "6fcdf014694b2b542a867763b4369cb3", "50d51854cf6a3434d96a87d050e1d09a", "0dff842f06edace43baf8a2f44207045" };
        private static readonly string[] AllySummonBuffs = {
            "2c3b98e56c1b4e9479f692d7ef95b2d2", "25629d7e78016a340b0e50818b6d8bb5", "932d27490e1701548a48b4cbc2f2caac" };
        private static readonly string[] AugmentFacts = { "38155ca9e4055bb48a89240a2055dcc3", "1ad9828c09fd28c4293c7b12845e6d20" };
        private const string AugmentBuff = "169d03bbccdbdc542ae1a57d83673d80";

        internal static string SummonBuffFor(SummonFamily family, int tier)
        { return (family == SummonFamily.Monster ? MonsterSummonBuffs : AllySummonBuffs)[tier <= 3 ? 0 : tier <= 6 ? 1 : 2]; }

        private static SummonForeignRowContract Base(SummonFamily family, SpiderSpeciesSpec species)
        {
            return new SummonForeignRowContract { Identity = SpiderSummonerCatalog.RowIdentity(family, species), InternalName = species.RowName(family), Family = family,
                Tier = species.BaseTier, Multiplicity = SummonMultiplicity.One, DirectUnit = species.UnitGuid };
        }

        private static IEnumerable<SummonForeignRowContract> Build()
        {
            foreach (SummonFamily family in SpiderSummonerCatalog.Families)
                foreach (SpiderSpeciesSpec species in SpiderSummonerCatalog.All)
                {
                    SummonForeignRowContract contract = Base(family, species);
                    if (family == SummonFamily.Monster && species.Key == "ogre-spider")
                    { contract.DirectUnit = null; contract.EvilUnit = RedcapUnit; contract.NonEvilUnit = BralaniUnit; contract.EvilCreatureKey = "redcap"; contract.NonEvilCreatureKey = "bralani-azata"; }
                    if (family == SummonFamily.Monster && species.Key == "giant-crab-spider")
                    { contract.TemplateEvilBuff = FiendishTemplateI; contract.TemplateOtherBuff = CelestialTemplateI; }
                    if (family == SummonFamily.Monster && species.Key == "giant-black-widow")
                    { contract.TemplateEvilBuff = FiendishTemplateI; contract.TemplateOtherBuff = CelestialTemplateII; }
                    yield return contract;
                }
        }

        private static readonly Dictionary<string, SummonForeignRowContract> Contracts =
            Build().ToDictionary(value => value.Identity, StringComparer.Ordinal);

        internal static IEnumerable<SummonForeignRowContract> All { get { return Contracts.Values; } }

        internal static bool IsKnownIdentity(string identity)
        { return identity != null && Contracts.ContainsKey(identity); }

        internal static SummonForeignRowResult Classify(SummonForeignRowReading reading)
        {
            SummonForeignRowContract contract;
            string problem = PrologueProblem(reading, out contract);
            if (problem != null) return SummonForeignRowResult.Unresolved(contract, problem);
            SummonNode[] effective = reading.Graph.Actions.Where(value => value.Kind != SummonNodeKind.Clear).ToArray();
            if (RepairedProblem(contract, effective) == null)
                return SummonForeignRowResult.Repaired(contract, SummonSpellAlignment.CasterMatched);
            return contract.Branched ? ClassifyBranched(contract, effective) : ClassifyDirect(contract, effective);
        }

        /// <summary>Null when the reading is exactly the known unrepaired input
        /// for this row, so the integration may repair it in place; otherwise
        /// the precise difference. A repaired row is not an input.</summary>
        internal static string LegacyInputProblem(SummonForeignRowReading reading)
        {
            SummonForeignRowContract contract;
            string problem = PrologueProblem(reading, out contract);
            if (problem != null) return problem;
            SummonNode[] nodes = reading.Graph.Actions.Where(value => value.Kind != SummonNodeKind.Clear).ToArray();
            string summonBuff = "apply-buff:" + SummonBuffFor(contract.Family, contract.Tier);
            if (contract.Branched)
            {
                if (nodes.Length != 1 || nodes[0].Kind != SummonNodeKind.Conditional) return "legacy-branch-shape-differs";
                SummonNode branch = nodes[0];
                if (branch.Operation != "And" || branch.Conditions.Count != 1 || !string.Equals(branch.Conditions[0], CasterEvilCondition, StringComparison.Ordinal))
                    return "legacy-branch-condition-differs";
                if (branch.IfTrue.Count != 1 || branch.IfTrue[0].Kind != SummonNodeKind.Spawn || branch.IfFalse.Count != 1 || branch.IfFalse[0].Kind != SummonNodeKind.Spawn)
                    return "legacy-branch-payload-differs";
                return ExactSpawnProblem(branch.IfTrue[0].Spawn, contract.EvilUnit, new[] { summonBuff }) ??
                    ExactSpawnProblem(branch.IfFalse[0].Spawn, contract.NonEvilUnit, new[] { summonBuff });
            }
            if (nodes.Length != 1 || nodes[0].Kind != SummonNodeKind.Spawn) return "legacy-direct-shape-differs";
            string[] expected = contract.TemplateEvilBuff == null ? new[] { summonBuff } : new[] { summonBuff,
                string.Format("conditional:And[{0}]{{t:apply-buff:{1}}}{{f:apply-buff:{2}}}", CasterEvilCondition, contract.TemplateEvilBuff, contract.TemplateOtherBuff) };
            return ExactSpawnProblem(nodes[0].Spawn, contract.DirectUnit, expected);
        }

        private static string PrologueProblem(SummonForeignRowReading reading, out SummonForeignRowContract contract)
        {
            contract = null;
            if (reading == null || reading.Identity == null) return "no-reading";
            if (!Contracts.TryGetValue(reading.Identity, out contract)) return "unrecognized-identity:" + reading.Identity;
            if (reading.InternalName != contract.InternalName) return "internal-name-differs:" + reading.InternalName;
            if (reading.PublishedTier != contract.Tier) return "published-tier-differs:" + reading.PublishedTier;
            if (!IsExactlyTheFeature(reading.RequiredFacts)) return "required-feature-differs";
            if (!IsExactlyTheFeature(reading.VisibilityFacts)) return "visibility-feature-differs";
            if (reading.LiveDescriptors != SummonAlignmentAxes.None) return "alignment-descriptors-present:" + reading.LiveDescriptors;
            if (reading.Graph == null) return "no-graph";
            if (reading.Graph.RunActions != 1 || reading.Graph.HiddenSpawns != 0) return "run-action-shape:" + reading.Graph.RunActions + ":" + reading.Graph.HiddenSpawns;
            return null;
        }

        private static string ExactSpawnProblem(SummonSpawn spawn, string unit, IEnumerable<string> postSpawn)
        {
            if (!string.Equals(spawn.Unit, unit, StringComparison.Ordinal)) return "spawn-unit-differs:" + spawn.Unit;
            if (spawn.CountDice != "Zero" || !IsSimple(spawn.CountDiceCount, 0) || !IsSimple(spawn.CountBonus, 1)) return "quantity-differs";
            return (spawn.AfterSpawnDetail ?? new List<string>()).SequenceEqual(postSpawn, StringComparer.Ordinal) ? null :
                "post-spawn-differs:" + string.Join("|", (spawn.AfterSpawnDetail ?? new List<string>()).ToArray());
        }

        /// <summary>Null only for the integration's exact repaired result: one
        /// direct spawn of the named spider, one summon buff, the benefit
        /// action exactly once, and the family's own alignment behavior.</summary>
        private static string RepairedProblem(SummonForeignRowContract contract, SummonNode[] nodes)
        {
            string unit = contract.DirectUnit ?? SpiderSummonerCatalog.Find("ogre-spider").UnitGuid;
            if (nodes.Length != 1 || nodes[0].Kind != SummonNodeKind.Spawn) return "not-direct";
            SummonSpawn spawn = nodes[0].Spawn;
            if (!string.Equals(spawn.Unit, unit, StringComparison.Ordinal)) return "unit";
            if (spawn.CountDice != "Zero" || !IsSimple(spawn.CountDiceCount, 0) || !IsSimple(spawn.CountBonus, 1)) return "count";
            string summonBuff = "apply-buff:" + SummonBuffFor(contract.Family, contract.Tier);
            string familyAction = contract.Family == SummonFamily.Monster ? TemplateAction : AlignmentAction;
            List<string> detail = spawn.AfterSpawnDetail ?? new List<string>();
            if (detail.Count(value => value == summonBuff) != 1 || detail.Count(value => value == familyAction) != 1 || detail.Count(value => value == BenefitAction) != 1)
                return "required-actions";
            string unknown = detail.FirstOrDefault(value => value != summonBuff && value != familyAction && value != BenefitAction && !AugmentFacts.Any(fact => value == string.Format(CasterHasFactFormat, fact, AugmentBuff)));
            return unknown == null ? null : "unknown:" + unknown;
        }

        private static bool IsExactlyTheFeature(IReadOnlyList<string> facts)
        { return facts != null && facts.Count == 1 && string.Equals(facts[0], SpiderSummonerFeatureGuid, StringComparison.Ordinal); }

        private static SummonForeignRowResult ClassifyDirect(SummonForeignRowContract contract, SummonNode[] nodes)
        {
            if (nodes.Length != 1 || nodes[0].Kind != SummonNodeKind.Spawn) return SummonForeignRowResult.Unresolved(contract, "direct-shape-differs");
            string problem = SpawnProblem(nodes[0].Spawn, contract.DirectUnit);
            if (problem != null) return SummonForeignRowResult.Unresolved(contract, problem);
            return SummonForeignRowResult.Inherent(contract, SummonSpellAlignment.Fixed(SummonAlignmentAxes.None,
                "RacesUnleashed vermin: verified exact unit, no alignment subtype, no template or alignment action."));
        }

        private static SummonForeignRowResult ClassifyBranched(SummonForeignRowContract contract, SummonNode[] nodes)
        {
            if (nodes.Length != 1 || nodes[0].Kind != SummonNodeKind.Conditional) return SummonForeignRowResult.Unresolved(contract, "branch-shape-differs");
            SummonNode branch = nodes[0];
            if (branch.Operation != "And" || branch.Conditions.Count != 1 || !string.Equals(branch.Conditions[0], CasterEvilCondition, StringComparison.Ordinal))
                return SummonForeignRowResult.Unresolved(contract, "branch-condition-differs:" + string.Join(";", branch.Conditions.ToArray()));
            if (branch.IfTrue.Count != 1 || branch.IfTrue[0].Kind != SummonNodeKind.Spawn || branch.IfFalse.Count != 1 || branch.IfFalse[0].Kind != SummonNodeKind.Spawn)
                return SummonForeignRowResult.Unresolved(contract, "branch-payload-differs");
            string problem = SpawnProblem(branch.IfTrue[0].Spawn, contract.EvilUnit) ?? SpawnProblem(branch.IfFalse[0].Spawn, contract.NonEvilUnit);
            if (problem != null) return SummonForeignRowResult.Unresolved(contract, problem);
            return SummonForeignRowResult.Governed(contract, SummonClericAlignmentCatalog.For(contract.EvilCreatureKey, false), SummonClericAlignmentCatalog.For(contract.NonEvilCreatureKey, false));
        }

        private static string SpawnProblem(SummonSpawn spawn, string unit)
        {
            if (!string.Equals(spawn.Unit, unit, StringComparison.Ordinal)) return "spawn-unit-differs:" + spawn.Unit;
            if (spawn.CountDice != "Zero" || !IsSimple(spawn.CountDiceCount, 0) || !IsSimple(spawn.CountBonus, 1)) return "quantity-differs";
            string unknown = (spawn.AfterSpawnDetail ?? new List<string>()).FirstOrDefault(value => !IsKnownPostSpawn(value));
            return unknown == null ? null : "post-spawn-action-differs:" + unknown;
        }

        private static bool IsSimple(SummonValue value, int number)
        { return value != null && value.Kind == "Simple" && value.Value == number; }

        private static bool IsKnownPostSpawn(string detail)
        {
            return MonsterSummonBuffs.Concat(AllySummonBuffs).Any(buff => detail == "apply-buff:" + buff) ||
                AugmentFacts.Any(fact => detail == string.Format(CasterHasFactFormat, fact, AugmentBuff));
        }
    }

    internal static class SummonForeignRowPolicy
    {
        /// <summary>A conversion route admits a verified row of its own spell
        /// family at its own published tier and quantity, only when the row's
        /// native prerequisite shows it to this caster.</summary>
        internal static bool AdmittedToConversion(SummonFamily routeFamily, int tier, SummonMultiplicity quantity, SummonForeignRowResult result, bool nativelyVisible)
        {
            return nativelyVisible && result != null && result.Outcome != SummonForeignContractOutcome.Unresolved &&
                result.Contract.Family == routeFamily && result.Contract.Tier == tier && result.Contract.Multiplicity == quantity;
        }
    }

    /// <summary>The Expanded Summoning module boundary for this feature's hooks.
    /// Inactive leaves the native result; the rule can only remove availability.</summary>
    internal static class SummonClericModuleBoundary
    {
        internal static bool Available<T>(bool moduleActive, bool nativeAvailable, T row, Func<T, SummonClericDecision?> decide)
        {
            if (!nativeAvailable || !moduleActive) return nativeAvailable;
            SummonClericDecision? decision = decide(row);
            return decision == null || SummonClericAlignmentPolicy.Permits(decision.Value);
        }
    }

    /// <summary>One computed value per row, discarded when the library changes.</summary>
    internal sealed class SummonGovernedRowCache<TRow, TValue> where TRow : class
    {
        private object _owner;
        private readonly Dictionary<TRow, TValue> _values = new Dictionary<TRow, TValue>();

        /// <summary>Forgets every value, for a change the owner check cannot
        /// see, such as an in-place repair of a classified row.</summary>
        internal void Clear() { _values.Clear(); _owner = null; }

        internal TValue Get(object owner, TRow row, Func<TRow, TValue> compute)
        {
            if (!ReferenceEquals(_owner, owner)) { _values.Clear(); _owner = owner; }
            TValue value;
            if (!_values.TryGetValue(row, out value)) _values[row] = value = compute(row);
            return value;
        }
    }
}
