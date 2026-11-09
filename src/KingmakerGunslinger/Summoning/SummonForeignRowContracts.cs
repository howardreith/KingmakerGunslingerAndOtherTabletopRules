using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    internal enum SummonForeignContractOutcome { VerifiedInherentlyLegal, ConditionallyGoverned, Unresolved }

    /// <summary>The exact loaded shape one supported third-party row must have.</summary>
    internal sealed class SummonForeignRowContract
    {
        internal string Identity, InternalName, DirectUnit, EvilUnit, NonEvilUnit, EvilCreatureKey, NonEvilCreatureKey;
        internal int Tier;
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
    /// The four RacesUnleashed Summon Monster rows, classified by what the
    /// loaded graph does. A name never classifies a row, a change to any
    /// checked part leaves it unresolved, and no third-party blueprint changes.
    /// </summary>
    internal static class SummonForeignRowClassifier
    {
        internal const string SpiderSummonerFeatureGuid = "bd03ca4637e74d4caefc66961093f0a0";
        internal const string RedcapUnit = "af8bcff074b93c84b97938e486184222", BralaniUnit = "58574e8d1d4dc464c976f396d9115b1a";
        internal const string CrabSpiderUnit = "b7e1252281ee46ea8dcd045aa1e56ee9", BlackWidowUnit = "9b1fda47e300491ab44011be9f8ef755",
            OgreSpiderUnit = "4a205de6d159483d99eba5aa05dd8f48", TarantulaUnit = "2ea0fc9b62634a1b99073119c02e673f";
        private const string IdentityPrefix = "130211b7fcb443e9b67d5561d75cf6a0";
        internal const string CasterEvilCondition = "Kingmaker.UnitLogic.Mechanics.Conditions.ContextConditionAlignment{Alignment=Evil;CheckCaster=True;Not=False}";
        private const string CasterHasFactFormat = "conditional:And[Kingmaker.UnitLogic.Mechanics.Conditions.ContextConditionCasterHasFact{{Fact={0};Not=False}}]{{t:apply-buff:{1}}}{{f:}}";

        private static readonly string[] SpawnBuffs = {
            "6fcdf014694b2b542a867763b4369cb3", "50d51854cf6a3434d96a87d050e1d09a", "0dff842f06edace43baf8a2f44207045" };
        private static readonly string[] AugmentFacts = { "38155ca9e4055bb48a89240a2055dcc3", "1ad9828c09fd28c4293c7b12845e6d20" };
        private const string AugmentBuff = "169d03bbccdbdc542ae1a57d83673d80";

        private static SummonForeignRowContract Direct(string name, int tier, string unit)
        { return new SummonForeignRowContract { Identity = IdentityPrefix + name, InternalName = name, Tier = tier, Multiplicity = SummonMultiplicity.One, DirectUnit = unit }; }

        private static readonly Dictionary<string, SummonForeignRowContract> Contracts = new[] {
            Direct("SummonMonsterIGiantCrabSpider", 1, CrabSpiderUnit),
            Direct("SummonMonsterGiantBlackWidow", 4, BlackWidowUnit),
            new SummonForeignRowContract { Identity = IdentityPrefix + "SummonMonsterOgreSpider", InternalName = "SummonMonsterOgreSpider", Tier = 5,
                Multiplicity = SummonMultiplicity.One, EvilUnit = RedcapUnit, NonEvilUnit = BralaniUnit, EvilCreatureKey = "redcap", NonEvilCreatureKey = "bralani-azata" },
            Direct("SummonMonsterGiantTarantula", 7, TarantulaUnit),
        }.ToDictionary(value => value.Identity, StringComparer.Ordinal);

        internal static IEnumerable<SummonForeignRowContract> All { get { return Contracts.Values; } }

        internal static bool IsKnownIdentity(string identity)
        { return identity != null && Contracts.ContainsKey(identity); }

        internal static SummonForeignRowResult Classify(SummonForeignRowReading reading)
        {
            if (reading == null || reading.Identity == null) return SummonForeignRowResult.Unresolved(null, "no-reading");
            SummonForeignRowContract contract;
            if (!Contracts.TryGetValue(reading.Identity, out contract)) return SummonForeignRowResult.Unresolved(null, "unrecognized-identity:" + reading.Identity);
            if (reading.InternalName != contract.InternalName) return SummonForeignRowResult.Unresolved(contract, "internal-name-differs:" + reading.InternalName);
            if (reading.PublishedTier != contract.Tier) return SummonForeignRowResult.Unresolved(contract, "published-tier-differs:" + reading.PublishedTier);
            if (!IsExactlyTheFeature(reading.RequiredFacts)) return SummonForeignRowResult.Unresolved(contract, "required-feature-differs");
            if (!IsExactlyTheFeature(reading.VisibilityFacts)) return SummonForeignRowResult.Unresolved(contract, "visibility-feature-differs");
            if (reading.LiveDescriptors != SummonAlignmentAxes.None) return SummonForeignRowResult.Unresolved(contract, "alignment-descriptors-present:" + reading.LiveDescriptors);
            SummonSignature graph = reading.Graph;
            if (graph == null) return SummonForeignRowResult.Unresolved(contract, "no-graph");
            if (graph.RunActions != 1 || graph.HiddenSpawns != 0) return SummonForeignRowResult.Unresolved(contract, "run-action-shape:" + graph.RunActions + ":" + graph.HiddenSpawns);
            SummonNode[] effective = graph.Actions.Where(value => value.Kind != SummonNodeKind.Clear).ToArray();
            return contract.Branched ? ClassifyBranched(contract, effective) : ClassifyDirect(contract, effective);
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
            return SpawnBuffs.Any(buff => detail == "apply-buff:" + buff) ||
                AugmentFacts.Any(fact => detail == string.Format(CasterHasFactFormat, fact, AugmentBuff));
        }
    }

    internal static class SummonForeignRowPolicy
    {
        /// <summary>Herald Caller's projection admits a verified row at its own
        /// published tier and its own quantity, only when its native
        /// prerequisite shows it to this caster. Other routes are unchanged.</summary>
        internal static bool AdmittedToConversion(bool heraldRoute, int tier, SummonMultiplicity quantity, SummonForeignRowResult result, bool nativelyVisible)
        {
            return heraldRoute && nativelyVisible && result != null && result.Outcome != SummonForeignContractOutcome.Unresolved &&
                result.Contract.Tier == tier && result.Contract.Multiplicity == quantity;
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

        internal TValue Get(object owner, TRow row, Func<TRow, TValue> compute)
        {
            if (!ReferenceEquals(_owner, owner)) { _values.Clear(); _owner = owner; }
            TValue value;
            if (!_values.TryGetValue(row, out value)) _values[row] = value = compute(row);
            return value;
        }
    }
}
