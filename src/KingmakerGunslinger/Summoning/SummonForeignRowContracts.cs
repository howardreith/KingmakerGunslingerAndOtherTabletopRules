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
        /// <summary>The native ability Races Unleashed copied for this row,
        /// changing only the spawned unit.</summary>
        internal string DonorGuid;
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
        /// <summary>Every variants component on the row: the recognized
        /// inherited Expanded Summoning menu as
        /// <see cref="SummonForeignRowClassifier.InheritedPublishedMenu"/>,
        /// anything else by its own name.</summary>
        internal IReadOnlyList<string> VariantsComponents = new string[0];
        /// <summary>The loaded native donor's buff applications, one list per
        /// spawn in walk order; null when the donor cannot be read.</summary>
        internal IReadOnlyList<IReadOnlyList<string>> NativeApplications;
    }

    internal sealed class SummonForeignRowResult
    {
        internal SummonForeignContractOutcome Outcome;
        internal string Reason;
        internal SummonForeignRowContract Contract;
        private SummonSpellAlignment _direct, _evil, _nonEvil;

        internal const string RepairedReason = "verified-repaired-spider-contract";

        /// <summary>Exactly the integration's verified repaired result.</summary>
        internal bool IsRepaired { get { return Outcome == SummonForeignContractOutcome.VerifiedInherentlyLegal && Reason == RepairedReason; } }

        internal static SummonForeignRowResult Unresolved(SummonForeignRowContract contract, string reason)
        { return new SummonForeignRowResult { Outcome = SummonForeignContractOutcome.Unresolved, Contract = contract, Reason = reason }; }

        internal static SummonForeignRowResult Inherent(SummonForeignRowContract contract, SummonSpellAlignment alignment)
        { return new SummonForeignRowResult { Outcome = SummonForeignContractOutcome.VerifiedInherentlyLegal, Contract = contract, Reason = "verified-direct-contract", _direct = alignment }; }

        internal static SummonForeignRowResult Repaired(SummonForeignRowContract contract, SummonSpellAlignment alignment)
        { return new SummonForeignRowResult { Outcome = SummonForeignContractOutcome.VerifiedInherentlyLegal, Contract = contract, Reason = RepairedReason, _direct = alignment }; }

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
        internal const string FiendishTemplateI = "c707fdbe58d0c614a89a872b76777f4b", CelestialTemplateI = "83a8a909e7ad19b4fa57e306884cc8bd", CelestialTemplateII = "ec906031bb8766f42a37a52cf1d89836";
        internal const string TemplateAction = "KingmakerGunslinger.Summoning.ContextActionApplySummonTemplateByCaster";
        internal const string AlignmentAction = "KingmakerGunslinger.Summoning.ContextActionSetSummonAlignment";
        internal const string BenefitAction = "KingmakerGunslinger.Summoning.ContextActionApplySpiderSummonerBenefit";

        private static readonly string[] MonsterSummonBuffs = {
            "6fcdf014694b2b542a867763b4369cb3", "50d51854cf6a3434d96a87d050e1d09a", "0dff842f06edace43baf8a2f44207045" };
        private static readonly string[] AllySummonBuffs = {
            "2c3b98e56c1b4e9479f692d7ef95b2d2", "25629d7e78016a340b0e50818b6d8bb5", "932d27490e1701548a48b4cbc2f2caac" };

        internal static string SummonBuffFor(SummonFamily family, int tier)
        { return (family == SummonFamily.Monster ? MonsterSummonBuffs : AllySummonBuffs)[tier <= 3 ? 0 : tier <= 6 ? 1 : 2]; }

        /// <summary>The variants component Races Unleashed copies onto its two
        /// tier I rows: their donor is the native tier I parent, which already
        /// carries Expanded Summoning's own published menu when it is copied.</summary>
        internal const string InheritedPublishedMenu = "inherited-expanded-summoning-published-menu";

        // The native abilities Races Unleashed copies (SpiderSummoner.CreateSpiderSummon's `original`).
        private static readonly Dictionary<string, string> Donors = new Dictionary<string, string>(StringComparer.Ordinal) {
            { "Monster:giant-crab-spider", "8fd74eddd9b6c224693d9ab241f25e84" }, { "Monster:giant-black-widow", "efa433a38e9c7c14bb4e780f8a3fe559" },
            { "Monster:ogre-spider", "0964bf88b582bed41b74e79596c4f6d9" }, { "Monster:giant-tarantula", "2920d48574933c24391fbb9e18f87bf5" },
            { "NaturesAlly:giant-crab-spider", "c6147854641924442a3bb736080cfeb6" }, { "NaturesAlly:giant-black-widow", "71dfb899a04db614e9db458ed4e43f56" },
            { "NaturesAlly:ogre-spider", "28ea1b2e0c4a9094da208b4c186f5e4f" }, { "NaturesAlly:giant-tarantula", "6d8d59aa38713be4fa3be76c19107cc0" } };

        private static SummonForeignRowContract Base(SummonFamily family, SpiderSpeciesSpec species)
        {
            return new SummonForeignRowContract { Identity = SpiderSummonerCatalog.RowIdentity(family, species), InternalName = species.RowName(family), Family = family,
                Tier = species.BaseTier, Multiplicity = SummonMultiplicity.One, DirectUnit = species.UnitGuid, DonorGuid = Donors[family + ":" + species.Key] };
        }

        /// <summary>The loaded native donor of a known row, or null.</summary>
        internal static string DonorOf(string identity)
        {
            SummonForeignRowContract contract;
            return identity != null && Contracts.TryGetValue(identity, out contract) ? contract.DonorGuid : null;
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

        /// <summary>
        /// One decision for repair, classification and conversion admission.
        /// A row is the integration's exact repaired result, or exactly the
        /// known unrepaired input, or unresolved; nothing in between is
        /// accepted. An exact unrepaired input keeps its released cleric
        /// classification; only a repaired row is ever projected.
        /// </summary>
        internal static SummonForeignRowResult Classify(SummonForeignRowReading reading)
        {
            SummonForeignRowContract contract;
            string problem = PrologueProblem(reading, out contract);
            if (problem != null) return SummonForeignRowResult.Unresolved(contract, problem);
            SummonNode[] effective = reading.Graph.Actions.Where(value => value.Kind != SummonNodeKind.Clear).ToArray();
            if (RepairedProblem(contract, reading, effective) == null)
                return SummonForeignRowResult.Repaired(contract, SummonSpellAlignment.CasterMatched);
            string legacy = LegacyInputProblem(reading);
            if (legacy != null) return SummonForeignRowResult.Unresolved(contract, "unsupported-shape:" + legacy);
            if (contract.Branched)
                return SummonForeignRowResult.Governed(contract, SummonClericAlignmentCatalog.For(contract.EvilCreatureKey, false), SummonClericAlignmentCatalog.For(contract.NonEvilCreatureKey, false));
            // The unrepaired Summon Monster Crab and Widow apply a native
            // template; that alignment is not classified, as before.
            if (contract.TemplateEvilBuff != null) return SummonForeignRowResult.Unresolved(contract, "unrepaired-native-template");
            return SummonForeignRowResult.Inherent(contract, SummonSpellAlignment.Fixed(SummonAlignmentAxes.None,
                "RacesUnleashed vermin: exact known input, exact unit, no alignment subtype, no template or alignment action."));
        }

        /// <summary>Null when the reading is exactly the known unrepaired input
        /// for this row, so the integration may repair it in place; otherwise
        /// the precise difference. A repaired row is not an input. Only a tier
        /// I row may carry the inherited published menu, which the repair
        /// removes; any other variants component leaves the row untouched.</summary>
        internal static string LegacyInputProblem(SummonForeignRowReading reading)
        {
            SummonForeignRowContract contract;
            string problem = PrologueProblem(reading, out contract);
            if (problem != null) return problem;
            SummonNode[] nodes = reading.Graph.Actions.Where(value => value.Kind != SummonNodeKind.Clear).ToArray();
            string summonBuff = "apply-buff:" + SummonBuffFor(contract.Family, contract.Tier);
            string shape;
            if (contract.Branched)
            {
                if (nodes.Length != 1 || nodes[0].Kind != SummonNodeKind.Conditional) return "legacy-branch-shape-differs";
                SummonNode branch = nodes[0];
                if (branch.Operation != "And" || branch.Conditions.Count != 1 || !string.Equals(branch.Conditions[0], CasterEvilCondition, StringComparison.Ordinal))
                    return "legacy-branch-condition-differs";
                if (branch.IfTrue.Count != 1 || branch.IfTrue[0].Kind != SummonNodeKind.Spawn || branch.IfFalse.Count != 1 || branch.IfFalse[0].Kind != SummonNodeKind.Spawn)
                    return "legacy-branch-payload-differs";
                shape = ExactSpawnProblem(branch.IfTrue[0].Spawn, contract.EvilUnit, new[] { summonBuff }) ??
                    ExactSpawnProblem(branch.IfFalse[0].Spawn, contract.NonEvilUnit, new[] { summonBuff });
            }
            else
            {
                if (nodes.Length != 1 || nodes[0].Kind != SummonNodeKind.Spawn) return "legacy-direct-shape-differs";
                string[] expected = contract.TemplateEvilBuff == null ? new[] { summonBuff } : new[] { summonBuff,
                    string.Format("conditional:And[{0}]{{t:apply-buff:{1}}}{{f:apply-buff:{2}}}", CasterEvilCondition, contract.TemplateEvilBuff, contract.TemplateOtherBuff) };
                shape = ExactSpawnProblem(nodes[0].Spawn, contract.DirectUnit, expected);
            }
            if (shape != null) return shape;
            IReadOnlyList<string> variants = reading.VariantsComponents ?? new string[0];
            if (variants.Count > 1 || variants.Any(value => contract.Tier != 1 || value != InheritedPublishedMenu))
                return "variants-component-differs:" + string.Join(",", variants.ToArray());
            return ApplicationsProblem(reading);
        }

        /// <summary>Each spawn's buff applications, recipient, duration and
        /// flags included, are exactly the loaded native donor's.</summary>
        private static string ApplicationsProblem(SummonForeignRowReading reading)
        {
            if (reading.NativeApplications == null) return "native-donor-unreadable";
            SummonSpawn[] spawns = reading.Graph.Spawns().ToArray();
            if (spawns.Length != reading.NativeApplications.Count) return "native-donor-shape-differs";
            for (int index = 0; index < spawns.Length; index++)
            {
                List<string> applications = spawns[index].AfterSpawnApplications ?? new List<string>();
                if (!applications.SequenceEqual(reading.NativeApplications[index] ?? new string[0], StringComparer.Ordinal))
                    return "buff-application-differs:" + string.Join("|", applications.ToArray());
            }
            return null;
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
        /// direct spawn of the named spider, the summon buff applied exactly as
        /// the native donor applies it, the family's own alignment behavior and
        /// the benefit action, each exactly once and nothing else, and no
        /// variants component of any kind.</summary>
        private static string RepairedProblem(SummonForeignRowContract contract, SummonForeignRowReading reading, SummonNode[] nodes)
        {
            string unit = contract.DirectUnit ?? SpiderSummonerCatalog.Find("ogre-spider").UnitGuid;
            if (nodes.Length != 1 || nodes[0].Kind != SummonNodeKind.Spawn) return "not-direct";
            SummonSpawn spawn = nodes[0].Spawn;
            if (!string.Equals(spawn.Unit, unit, StringComparison.Ordinal)) return "unit";
            if (spawn.CountDice != "Zero" || !IsSimple(spawn.CountDiceCount, 0) || !IsSimple(spawn.CountBonus, 1)) return "count";
            string summonGuid = SummonBuffFor(contract.Family, contract.Tier);
            string familyAction = contract.Family == SummonFamily.Monster ? TemplateAction : AlignmentAction;
            string[] expected = { "apply-buff:" + summonGuid, familyAction, BenefitAction };
            List<string> detail = spawn.AfterSpawnDetail ?? new List<string>();
            if (!detail.SequenceEqual(expected, StringComparer.Ordinal)) return "post-spawn:" + string.Join("|", detail.ToArray());
            if ((reading.VariantsComponents ?? new string[0]).Count != 0) return "variants-component-present";
            if (reading.NativeApplications == null) return "native-donor-unreadable";
            string[] native = reading.NativeApplications.SelectMany(value => value ?? new string[0])
                .Where(value => value.StartsWith(SummonSignatureFormat.ApplicationPrefix(summonGuid), StringComparison.Ordinal)).Distinct(StringComparer.Ordinal).ToArray();
            if (native.Length != 1) return "native-summon-application-ambiguous:" + native.Length;
            List<string> applications = spawn.AfterSpawnApplications ?? new List<string>();
            return applications.Count == 1 && string.Equals(applications[0], native[0], StringComparison.Ordinal) ? null :
                "summon-application-differs:" + string.Join("|", applications.ToArray());
        }

        private static bool IsExactlyTheFeature(IReadOnlyList<string> facts)
        { return facts != null && facts.Count == 1 && string.Equals(facts[0], SpiderSummonerFeatureGuid, StringComparison.Ordinal); }

        private static bool IsSimple(SummonValue value, int number)
        { return value != null && value.Kind == "Simple" && value.Value == number; }
    }

    internal static class SummonForeignRowPolicy
    {
        /// <summary>A conversion route admits only the integration's verified
        /// repaired row, of its own spell family at its own published tier and
        /// quantity, and only when the row's native prerequisite shows it to
        /// this caster. An unrepaired or unfamiliar row is never projected.</summary>
        internal static bool AdmittedToConversion(SummonFamily routeFamily, int tier, SummonMultiplicity quantity, SummonForeignRowResult result, bool nativelyVisible)
        {
            return nativelyVisible && result != null && result.IsRepaired &&
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
