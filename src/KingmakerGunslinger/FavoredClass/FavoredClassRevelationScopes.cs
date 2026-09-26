using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;

namespace KingmakerGunslinger.FavoredClass
{
    /// <summary>
    /// A native ability resource's amount, read once from its private fields,
    /// and the unit sums its native maximum counts.
    /// </summary>
    internal sealed class FavoredClassResourceFormula
    {
        private const BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly FieldInfo MaxAmountField =
            typeof(BlueprintAbilityResource).GetField("m_MaxAmount", AnyInstance);
        private static readonly FieldInfo UseMaxField = typeof(BlueprintAbilityResource).GetField("m_UseMax", AnyInstance);
        private static readonly FieldInfo MaxField = typeof(BlueprintAbilityResource).GetField("m_Max", AnyInstance);

        private readonly BlueprintCharacterClass[] _classes;
        private readonly BlueprintArchetype[] _archetypes;
        private readonly BlueprintCharacterClass[] _divClasses;
        private readonly BlueprintArchetype[] _divArchetypes;
        private readonly StatType _stat;

        private FavoredClassResourceFormula(FavoredClassResourceAmount amount, BlueprintCharacterClass[] classes,
            BlueprintArchetype[] archetypes, BlueprintCharacterClass[] divClasses, BlueprintArchetype[] divArchetypes,
            StatType stat)
        {
            Amount = amount;
            _classes = classes ?? new BlueprintCharacterClass[0];
            _archetypes = archetypes ?? new BlueprintArchetype[0];
            _divClasses = divClasses ?? new BlueprintCharacterClass[0];
            _divArchetypes = divArchetypes ?? new BlueprintArchetype[0];
            _stat = stat;
        }

        internal FavoredClassResourceAmount Amount { get; private set; }

        /// <summary>The resource's amount, or null when its native layout is not the audited one.</summary>
        internal static FavoredClassResourceFormula Read(BlueprintAbilityResource resource,
            BlueprintCharacterClass oracle)
        {
            if (resource == null || MaxAmountField == null || UseMaxField == null || MaxField == null)
                return null;
            object boxed = MaxAmountField.GetValue(resource);
            if (boxed == null)
                return null;
            Func<string, object> field = name =>
            {
                FieldInfo info = boxed.GetType().GetField(name, AnyInstance);
                if (info == null)
                    throw new MissingFieldException(boxed.GetType().FullName, name);
                return info.GetValue(boxed);
            };
            var classes = (BlueprintCharacterClass[])field("Class");
            var divClasses = (BlueprintCharacterClass[])field("ClassDiv");
            var amount = new FavoredClassResourceAmount
            {
                BaseValue = (int)field("BaseValue"),
                IncreasedByLevel = (bool)field("IncreasedByLevel"),
                LevelIncrease = (int)field("LevelIncrease"),
                IncreasedByStat = (bool)field("IncreasedByStat"),
                IncreasedByLevelStartPlusDivStep = (bool)field("IncreasedByLevelStartPlusDivStep"),
                StartingLevel = (int)field("StartingLevel"),
                StartingIncrease = (int)field("StartingIncrease"),
                LevelStep = (int)field("LevelStep"),
                PerStepIncrease = (int)field("PerStepIncrease"),
                MinClassLevelIncrease = (int)field("MinClassLevelIncrease"),
                OtherClassesModifier = (float)field("OtherClassesModifier"),
                UseMax = (bool)UseMaxField.GetValue(resource),
                Max = (int)MaxField.GetValue(resource),
                LevelScalesWithOracle = classes != null && classes.Contains(oracle),
                DivScalesWithOracle = divClasses != null && divClasses.Contains(oracle)
            };
            return new FavoredClassResourceFormula(amount, classes, (BlueprintArchetype[])field("Archetypes"),
                divClasses, (BlueprintArchetype[])field("ArchetypesDiv"), (StatType)field("ResourceBonusStat"));
        }

        /// <summary>The handler bonus for the unit at its effective oracle level.</summary>
        internal int Delta(UnitDescriptor unit, int effective)
        {
            if (unit == null || effective <= 0)
                return 0;
            int statBonus = 0;
            if (Amount.IncreasedByStat)
            {
                var stat = unit.Stats.GetStat(_stat) as ModifiableValueAttributeStat;
                statBonus = stat == null ? 0 : stat.Bonus;
            }
            return FavoredClassMechanicsPolicy.ResourceDelta(Amount, CountedLevels(unit, _classes, _archetypes),
                CountedLevels(unit, _divClasses, _divArchetypes), unit.Progression.CharacterLevel, statBonus,
                effective);
        }

        // The native archetype gate, including its last-archetype-wins loop.
        private static int CountedLevels(UnitDescriptor unit, BlueprintCharacterClass[] classes,
            BlueprintArchetype[] archetypes)
        {
            int sum = 0;
            foreach (BlueprintCharacterClass characterClass in classes)
            {
                if (characterClass == null)
                    continue;
                bool counted = true;
                foreach (BlueprintArchetype archetype in archetypes)
                {
                    ClassData data = unit.Progression.GetClassData(characterClass);
                    counted = data == null || !characterClass.Archetypes.Contains(archetype) ||
                        data.Archetypes.Contains(archetype);
                }
                sum += counted ? unit.Progression.GetClassLevel(characterClass) : 0;
            }
            return sum;
        }
    }

    /// <summary>The audited read points of one selected revelation target (I06/S04).</summary>
    internal sealed class FavoredClassRevelationScope
    {
        internal FavoredClassRevelationScope(FavoredClassRevelationTarget target, BlueprintFeature fullLeaf,
            FavoredClassRate rate)
        {
            Target = target;
            FullLeaf = fullLeaf;
            Rate = rate;
            Features = new List<BlueprintFeature>();
            ParamsAbilities = new HashSet<BlueprintScriptableObject>(FavoredClassRevelationScopes.ReferenceComparer);
            Resources = new Dictionary<BlueprintAbilityResource, FavoredClassResourceFormula>(
                FavoredClassRevelationScopes.ResourceComparer);
            RankSources = new List<KeyValuePair<ContextRankConfig, BlueprintScriptableObject>>();
            RefreshFeatures = new HashSet<BlueprintScriptableObject>(FavoredClassRevelationScopes.ReferenceComparer);
            Gates = new List<FavoredClassRevelationGate>();
            GateOwners = new HashSet<BlueprintScriptableObject>(FavoredClassRevelationScopes.ReferenceComparer);
            Evidence = new List<string>();
        }

        /// <summary>
        /// The revelation's own native level gates (AddFeatureOnClassLevel on
        /// the Oracle engine's classes anywhere in its graph): the abilities
        /// and forms it grants at later oracle levels, evaluated at the
        /// effective level.
        /// </summary>
        internal List<FavoredClassRevelationGate> Gates { get; private set; }

        /// <summary>The blueprints that hold those gates.</summary>
        internal HashSet<BlueprintScriptableObject> GateOwners { get; private set; }

        /// <summary>Why this target cannot be published completely in this process, or null.</summary>
        internal string PartialReason { get; set; }

        /// <summary>Whether Build admitted this target's read points and gates to the indexes.</summary>
        internal bool Admitted { get; set; }

        /// <summary>
        /// Whether this target's counter gives any benefit in this process
        /// (FavoredClassMechanicsPolicy.ScopeActive). An excluded, partial,
        /// read-point-less, unadmitted, withheld or unavailable target keeps
        /// its saved ranks and every one of its read points stays native:
        /// ranks, resources, ability parameters and level gates.
        /// </summary>
        internal bool Active
        {
            get
            {
                return FavoredClassMechanicsPolicy.ScopeActive(Target.Published, PartialReason, HasReadPoints,
                    Admitted, FavoredClassRuntime.IsEffectUnavailable(FavoredClassCatalog.EffectSelectedRevelation) ||
                    FavoredClassRuntime.IsTargetUnavailable(FavoredClassCatalog.EffectSelectedRevelation, Key));
            }
        }

        internal FavoredClassRevelationTarget Target { get; private set; }
        internal string Key { get { return Target.Key; } }
        internal BlueprintFeature FullLeaf { get; private set; }
        internal FavoredClassRate Rate { get; private set; }

        /// <summary>The revelation's own selectable features present in this process.</summary>
        internal List<BlueprintFeature> Features { get; private set; }

        /// <summary>Family C: abilities whose parameters the Oracle engine computes from oracle level.</summary>
        internal HashSet<BlueprintScriptableObject> ParamsAbilities { get; private set; }

        /// <summary>Family B: resources whose maximum scales with oracle level.</summary>
        internal Dictionary<BlueprintAbilityResource, FavoredClassResourceFormula> Resources { get; private set; }

        /// <summary>Family A: (rank config, the context blueprint that evaluates it).</summary>
        internal List<KeyValuePair<ContextRankConfig, BlueprintScriptableObject>> RankSources { get; private set; }

        /// <summary>Features whose own persistent context holds a scaled rank.</summary>
        internal HashSet<BlueprintScriptableObject> RefreshFeatures { get; private set; }

        /// <summary>Held-back, excluded and shared read points (review evidence).</summary>
        internal List<string> Evidence { get; private set; }

        internal bool HasReadPoints
        {
            get { return ParamsAbilities.Count + Resources.Count + RankSources.Count + Gates.Count > 0; }
        }

        /// <summary>The families actually found: A rank configs, B resources, C parameters.</summary>
        internal string Families
        {
            get
            {
                return (RankSources.Count > 0 ? "A" : string.Empty) + (Resources.Count > 0 ? "B" : string.Empty) +
                    (ParamsAbilities.Count > 0 ? "C" : string.Empty) + (Gates.Count > 0 ? "D" : string.Empty);
            }
        }

        /// <summary>
        /// The unit's earned whole steps in this target's own counter. A leaf
        /// that is being removed no longer counts while its target refreshes.
        /// </summary>
        internal int EarnedSteps(UnitDescriptor unit)
        {
            if (unit == null || unit.Progression == null || FullLeaf == null || !FavoredClassRuntime.MechanicsEnabled ||
                !Active)
                return 0;
            Fact leaf = unit.Progression.Features.GetFact(FullLeaf);
            if (leaf == null || FavoredClassRevelationScopes.IsDeparting(leaf))
                return 0;
            return FavoredClassRankPolicy.BenefitSteps(Rate, leaf.GetRank());
        }

        internal int ResourceDelta(BlueprintAbilityResource resource, UnitDescriptor unit, int effective)
        {
            FavoredClassResourceFormula formula;
            return resource != null && Resources.TryGetValue(resource, out formula) ? formula.Delta(unit, effective) : 0;
        }
    }

    /// <summary>One native level gate of an owned revelation.</summary>
    internal sealed class FavoredClassRevelationGate
    {
        internal FavoredClassRevelationGate(BlueprintScriptableObject owner, AddFeatureOnClassLevel gate)
        {
            Owner = owner;
            ComponentName = gate.name;
            Level = gate.Level;
            BeforeThisLevel = gate.BeforeThisLevel;
            Feature = gate.Feature;
        }

        /// <summary>The blueprint whose fact carries the gate component.</summary>
        internal BlueprintScriptableObject Owner { get; private set; }

        /// <summary>The component's name, kept by every per-fact copy of it.</summary>
        internal string ComponentName { get; private set; }

        internal int Level { get; private set; }
        internal bool BeforeThisLevel { get; private set; }
        internal BlueprintFeature Feature { get; private set; }

        internal string Label
        {
            get
            {
                return Owner.name + "|" + ComponentName + (BeforeThisLevel ? "<" : "@") + Level + ":" +
                    (Feature == null ? "null" : Feature.name);
            }
        }
    }

    /// <summary>
    /// Builds, when the publication commits (after the optional provider
    /// has created the Oracle), each revelation target's read points by a
    /// bounded, cycle-safe walk of the live blueprint graph from its own
    /// revelation features: granted facts, level-gated features (walked, never
    /// moved), variants, touch deliveries, buffs, areas, activatable buffs
    /// and their action lists. Every value the owned revelation computes from
    /// the Oracle engine's class level (the sum of Oracle levels and Demon
    /// Hunter levels) uses the effective level: its rank configs including
    /// their steps, tiers and breakpoint tables, its resources including their
    /// single-level thresholds, its caster level and DC, and its own level
    /// gates, so the abilities and forms the owned revelation grants at later
    /// oracle levels follow the effective level too (charter 8.10: the owned
    /// power's effect thresholds). No other revelation, no revelation choice
    /// and no class feature is ever granted early. A target whose effect
    /// cannot be implemented without breaking a charter rule (the possession
    /// BAB) is excluded from publication, never published partially. Read
    /// points and gates reached from two targets are withheld from both, and a
    /// target that loses a gate that way is withheld whole.
    /// </summary>
    internal static class FavoredClassRevelationScopes
    {
        private const BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const int MaximumDepth = 12;
        private const int MaximumNodes = 20000;
        private const string OracleParamsCalculator = "ContextCalculateAbilityParamsBasedOnClasses";
        private const string TierAction = "RunActionsDependingOnContextValue";

        private static readonly FieldInfo RankBase = typeof(ContextRankConfig).GetField("m_BaseValueType", AnyInstance);
        private static readonly FieldInfo RankProgression = typeof(ContextRankConfig).GetField("m_Progression", AnyInstance);
        private static readonly FieldInfo RankClasses = typeof(ContextRankConfig).GetField("m_Class", AnyInstance);
        private static readonly FieldInfo RankExcept = typeof(ContextRankConfig).GetField("m_ExceptClasses", AnyInstance);
        private static readonly FieldInfo RankArchetype = typeof(ContextRankConfig).GetField("Archetype", AnyInstance);

        internal static readonly IEqualityComparer<BlueprintScriptableObject> ReferenceComparer =
            new Reference<BlueprintScriptableObject>();
        internal static readonly IEqualityComparer<BlueprintAbilityResource> ResourceComparer =
            new Reference<BlueprintAbilityResource>();

        private static readonly object Gate = new object();
        private static Dictionary<string, FavoredClassRevelationScope> _byKey =
            new Dictionary<string, FavoredClassRevelationScope>(StringComparer.Ordinal);
        private static Dictionary<ContextRankConfig, Dictionary<BlueprintScriptableObject, FavoredClassRevelationScope>> _ranks =
            NewRankIndex();
        private static Dictionary<BlueprintScriptableObject, Dictionary<string, FavoredClassRevelationScope>> _gates =
            NewGateIndex();
        private static readonly HashSet<Fact> Departing = new HashSet<Fact>(new Reference<Fact>());

        internal static int ScopeCount
        {
            get { lock (Gate) return _byKey.Count; }
        }

        internal static IList<FavoredClassRevelationScope> All
        {
            get { lock (Gate) return _byKey.Values.ToList().AsReadOnly(); }
        }

        internal static FavoredClassRevelationScope ForKey(string key)
        {
            FavoredClassRevelationScope scope;
            Dictionary<string, FavoredClassRevelationScope> byKey = _byKey;
            return key != null && byKey.TryGetValue(key, out scope) ? scope : null;
        }

        /// <summary>
        /// Family A read (called for every rank evaluation, so it returns at
        /// once unless this exact config is scoped for this context's own
        /// blueprint): the caster's earned steps in that revelation's counter.
        /// </summary>
        internal static int RankBonus(ContextRankConfig config, MechanicsContext context)
        {
            Dictionary<ContextRankConfig, Dictionary<BlueprintScriptableObject, FavoredClassRevelationScope>> ranks = _ranks;
            if (ranks.Count == 0 || config == null || context == null)
                return 0;
            Dictionary<BlueprintScriptableObject, FavoredClassRevelationScope> owners;
            FavoredClassRevelationScope scope;
            if (!ranks.TryGetValue(config, out owners) || context.AssociatedBlueprint == null ||
                !owners.TryGetValue(context.AssociatedBlueprint, out scope))
                return 0;
            return context.MaybeCaster == null ? 0 : scope.EarnedSteps(context.MaybeCaster.Descriptor);
        }

        /// <summary>
        /// Level-gate read (called for every native AddFeatureOnClassLevel
        /// decision, so it returns at once unless this exact gate of an owned
        /// revelation is scoped): with earned steps the gate decides at the
        /// Oracle engine's class level plus those steps, exactly like the
        /// native decision at that level.
        /// </summary>
        internal static void GateResult(AddFeatureOnClassLevel gate, ref bool result)
        {
            Dictionary<BlueprintScriptableObject, Dictionary<string, FavoredClassRevelationScope>> gates = _gates;
            if (gates.Count == 0 || gate == null)
                return;
            Fact fact = gate.Fact;
            Dictionary<string, FavoredClassRevelationScope> byName;
            FavoredClassRevelationScope scope;
            if (fact == null || fact.Blueprint == null || !gates.TryGetValue(fact.Blueprint, out byName) ||
                gate.name == null || !byName.TryGetValue(gate.name, out scope))
                return;
            UnitDescriptor owner = gate.Owner;
            int steps = scope.EarnedSteps(owner);
            if (steps <= 0)
                return;
            int level = ReplaceCasterLevelOfAbility.CalculateClassLevel(gate.Class, gate.AdditionalClasses, owner,
                gate.Archetypes);
            result = FavoredClassMechanicsPolicy.GateApplies(level + steps, gate.Level, gate.BeforeThisLevel);
        }

        internal static bool IsDeparting(Fact fact)
        {
            return fact != null && Departing.Count > 0 && Departing.Contains(fact);
        }

        /// <summary>Marks a leaf that is being removed so its target refreshes without it.</summary>
        internal static void BeginDeparture(Fact fact)
        {
            if (fact != null)
                Departing.Add(fact);
        }

        internal static void EndDeparture(Fact fact)
        {
            if (fact != null)
                Departing.Remove(fact);
        }

        internal static void Clear()
        {
            lock (Gate)
            {
                _byKey = new Dictionary<string, FavoredClassRevelationScope>(StringComparer.Ordinal);
                _ranks = NewRankIndex();
                _gates = NewGateIndex();
            }
        }

        /// <summary>
        /// Builds every scope. Returns the keys of targets without any read
        /// point in this process (withheld from publication).
        /// </summary>
        internal static IList<string> Build(LibraryScriptableObject library, FavoredClassBlueprintSet set)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (set == null) throw new ArgumentNullException("set");
            if (RankBase == null || RankProgression == null || RankClasses == null || RankExcept == null ||
                RankArchetype == null)
                throw new InvalidOperationException("The native rank config layout changed.");
            var byKey = new Dictionary<string, FavoredClassRevelationScope>(StringComparer.Ordinal);
            var withheld = new List<string>();
            BlueprintScriptableObject oracleBlueprint;
            library.BlueprintsByAssetId.TryGetValue(FavoredClassRevelationManifest.OracleClassGuid, out oracleBlueprint);
            var oracle = oracleBlueprint as BlueprintCharacterClass;
            BlueprintScriptableObject hunterBlueprint;
            library.BlueprintsByAssetId.TryGetValue(FavoredClassRevelationManifest.RavenerHunterArchetypeGuid,
                out hunterBlueprint);
            var hunter = hunterBlueprint as BlueprintArchetype;
            if (oracle == null || hunter == null)
            {
                Clear();
                return FavoredClassRevelationManifest.All.Select(target => target.Key).ToList().AsReadOnly();
            }
            foreach (FavoredClassRevelationTarget target in FavoredClassRevelationManifest.All)
            {
                FavoredClassLeafPair pair = set.Pair(FavoredClassCatalog.EffectSelectedRevelation, target.Key);
                if (pair == null)
                    throw new InvalidOperationException("No registered counter for revelation " + target.Key);
                var scope = new FavoredClassRevelationScope(target, pair.Full, pair.Effect.Rate);
                var roots = new List<BlueprintScriptableObject>();
                foreach (string guid in target.FeatureGuids)
                {
                    BlueprintScriptableObject feature;
                    if (library.BlueprintsByAssetId.TryGetValue(guid, out feature) && feature is BlueprintFeature)
                    {
                        scope.Features.Add((BlueprintFeature)feature);
                        roots.Add(feature);
                    }
                }
                foreach (string guid in target.ExtraRoots)
                {
                    BlueprintScriptableObject extra;
                    if (library.BlueprintsByAssetId.TryGetValue(guid, out extra) && extra != null)
                        roots.Add(extra);
                }
                if (scope.Features.Count > 0)
                    Walk(scope, roots, oracle, hunter);
                byKey[target.Key] = scope;
            }
            WithholdShared(byKey.Values.ToList());
            var ranks = NewRankIndex();
            var gates = NewGateIndex();
            foreach (FavoredClassRevelationScope scope in byKey.Values)
            {
                // An excluded target is registered but never published; none
                // of its read points or gates is indexed.
                if (!scope.Target.Published)
                {
                    scope.Evidence.Add("excluded-target:" + scope.Target.ExcludedReason);
                    continue;
                }
                if (!scope.HasReadPoints || scope.PartialReason != null)
                {
                    if (scope.PartialReason != null)
                        scope.Evidence.Add("withheld-partial:" + scope.PartialReason);
                    withheld.Add(scope.Key);
                    continue;
                }
                scope.Admitted = true;
                foreach (FavoredClassRevelationGate gate in scope.Gates)
                {
                    Dictionary<string, FavoredClassRevelationScope> byName;
                    if (!gates.TryGetValue(gate.Owner, out byName))
                        gates[gate.Owner] = byName = new Dictionary<string, FavoredClassRevelationScope>(
                            StringComparer.Ordinal);
                    byName[gate.ComponentName] = scope;
                    scope.GateOwners.Add(gate.Owner);
                }
                foreach (KeyValuePair<ContextRankConfig, BlueprintScriptableObject> source in scope.RankSources)
                {
                    Dictionary<BlueprintScriptableObject, FavoredClassRevelationScope> owners;
                    if (!ranks.TryGetValue(source.Key, out owners))
                        ranks[source.Key] = owners =
                            new Dictionary<BlueprintScriptableObject, FavoredClassRevelationScope>(ReferenceComparer);
                    owners[source.Value] = scope;
                    if (source.Value is BlueprintFeature)
                        scope.RefreshFeatures.Add(source.Value);
                }
            }
            lock (Gate)
            {
                _byKey = byKey;
                _ranks = ranks;
                _gates = gates;
            }
            return withheld.AsReadOnly();
        }

        private static void Walk(FavoredClassRevelationScope scope, IEnumerable<BlueprintScriptableObject> roots,
            BlueprintCharacterClass oracle, BlueprintArchetype hunter)
        {
            var visited = new HashSet<object>(new Reference<object>());
            var pending = new Queue<Node>();
            var tierRanks = new HashSet<string>(StringComparer.Ordinal);
            var contexts = new List<BlueprintScriptableObject>();
            foreach (BlueprintScriptableObject root in roots)
                pending.Enqueue(new Node(root, 0, null));
            int nodes = 0;
            while (pending.Count > 0)
            {
                Node current = pending.Dequeue();
                if (current.Value == null || current.Depth > MaximumDepth || !visited.Add(current.Value))
                    continue;
                if (++nodes > MaximumNodes)
                    throw new InvalidOperationException("The revelation graph of " + scope.Key +
                        " exceeds the walk bound.");
                var blueprint = current.Value as BlueprintScriptableObject;
                if (blueprint != null)
                {
                    var resource = blueprint as BlueprintAbilityResource;
                    if (resource != null)
                    {
                        FavoredClassResourceFormula formula = FavoredClassResourceFormula.Read(resource, oracle);
                        if (formula != null && formula.Amount.ScalesWithOracle)
                        {
                            scope.Resources[resource] = formula;
                            if (formula.Amount.IncreasedByLevelStartPlusDivStep && formula.Amount.PerStepIncrease == 0)
                                scope.Evidence.Add("threshold-resource:" + resource.name);
                        }
                        continue;
                    }
                    contexts.Add(blueprint);
                    if (blueprint is BlueprintAbility && IsOracleParams(blueprint, oracle))
                        scope.ParamsAbilities.Add(blueprint);
                    EnqueueFields(blueprint, current.Depth + 1, blueprint, pending, false);
                    foreach (BlueprintComponent component in blueprint.ComponentsArray ?? new BlueprintComponent[0])
                        if (component != null && !IsRestriction(component))
                            pending.Enqueue(new Node(component, current.Depth + 1, blueprint));
                    continue;
                }
                // The revelation's own level gates (its later abilities and
                // forms) are recorded; their granted features are walked too.
                var levelGate = current.Value as AddFeatureOnClassLevel;
                if (levelGate != null && current.Owner != null && IsOracleGate(levelGate, oracle))
                {
                    var recorded = new FavoredClassRevelationGate(current.Owner, levelGate);
                    scope.Gates.Add(recorded);
                    scope.Evidence.Add("gate:" + recorded.Label);
                }
                if (current.Owner != null && current.Value.GetType().Name == TierAction)
                {
                    FieldInfo valueField = current.Value.GetType().GetField("value", AnyInstance);
                    var value = valueField == null ? null : valueField.GetValue(current.Value) as ContextValue;
                    if (value != null && value.ValueType == ContextValueType.Rank)
                        tierRanks.Add(current.Owner.AssetGuid + "|" + value.ValueRank);
                }
                if (!current.Value.GetType().Name.StartsWith("ContextActionRemove", StringComparison.Ordinal))
                    EnqueueFields(current.Value, current.Depth + 1, current.Owner, pending,
                        IsRuleReaction(current.Value));
            }
            foreach (BlueprintScriptableObject blueprint in contexts)
                foreach (ContextRankConfig config in (blueprint.ComponentsArray ?? new BlueprintComponent[0])
                    .OfType<ContextRankConfig>())
                {
                    if (!IsOracleEngineRank(config, oracle, hunter))
                        continue;
                    string key = blueprint.AssetGuid + "|" + config.Type;
                    string label = blueprint.name + "|" + config.Type;
                    if (scope.Target.ExcludedRanks.Contains(key))
                    {
                        scope.Evidence.Add("excluded-read:" + label);
                        continue;
                    }
                    scope.RankSources.Add(new KeyValuePair<ContextRankConfig, BlueprintScriptableObject>(config,
                        blueprint));
                    if ((ContextRankProgression)RankProgression.GetValue(config) == ContextRankProgression.Custom)
                        scope.Evidence.Add("breakpoint-table:" + label);
                    if (tierRanks.Contains(key))
                        scope.Evidence.Add("tier:" + label);
                }
        }

        /// <param name="actionsOnly">
        /// For a rule reaction, only the actions it executes: its blueprint
        /// references are filters (for example the channels a channel
        /// resistance bonus applies against), never grants.
        /// </param>
        private static void EnqueueFields(object holder, int depth, BlueprintScriptableObject owner, Queue<Node> pending,
            bool actionsOnly)
        {
            foreach (FieldInfo field in holder.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                object value = field.GetValue(holder);
                if (value == null)
                    continue;
                if (Follows(value, actionsOnly))
                    pending.Enqueue(new Node(value, depth, owner));
                else if (value is IEnumerable && !(value is string))
                    foreach (object item in (IEnumerable)value)
                        if (item != null && Follows(item, actionsOnly))
                            pending.Enqueue(new Node(item, depth, owner));
            }
        }

        private static bool IsRuleReaction(object component)
        {
            return component is BlueprintComponent && (component is IInitiatorRulebookSubscriber ||
                component is ITargetRulebookSubscriber || component is IGlobalRulebookSubscriber);
        }

        // Grants and executed effects only: conditions, prerequisites,
        // restrictions, rule-reaction filters, summoned units, classes and
        // selections are never followed.
        private static bool Follows(object value, bool actionsOnly)
        {
            if (value is GameAction || value is ActionList)
                return true;
            if (actionsOnly || value is BlueprintFeatureSelection)
                return false;
            return value is BlueprintFeature || value is BlueprintAbility || value is BlueprintBuff ||
                value is BlueprintActivatableAbility || value is BlueprintAbilityAreaEffect ||
                value is BlueprintAbilityResource;
        }

        private static bool IsRestriction(BlueprintComponent component)
        {
            string name = component.GetType().Name;
            return component is Prerequisite || name.StartsWith("AbilityTarget", StringComparison.Ordinal) ||
                name.StartsWith("AbilityCaster", StringComparison.Ordinal);
        }

        private static bool IsOracleParams(BlueprintScriptableObject blueprint, BlueprintCharacterClass oracle)
        {
            foreach (BlueprintComponent component in blueprint.ComponentsArray ?? new BlueprintComponent[0])
            {
                if (component == null || component.GetType().Name != OracleParamsCalculator)
                    continue;
                FieldInfo classes = component.GetType().GetField("CharacterClasses", AnyInstance);
                var value = classes == null ? null : classes.GetValue(component) as BlueprintCharacterClass[];
                if (value != null && value.Contains(oracle))
                    return true;
            }
            return false;
        }

        /// <summary>A native level gate on the Oracle engine's classes.</summary>
        private static bool IsOracleGate(AddFeatureOnClassLevel gate, BlueprintCharacterClass oracle)
        {
            return gate.Feature != null && (ReferenceEquals(gate.Class, oracle) ||
                (gate.AdditionalClasses != null && gate.AdditionalClasses.Contains(oracle)));
        }

        /// <summary>
        /// The Oracle engine's class-level rank: the sum of the listed class
        /// levels (Oracle, and Inquisitor only for the Demon Hunter).
        /// </summary>
        private static bool IsOracleEngineRank(ContextRankConfig config, BlueprintCharacterClass oracle,
            BlueprintArchetype hunter)
        {
            if ((ContextRankBaseValueType)RankBase.GetValue(config) !=
                ContextRankBaseValueType.SummClassLevelWithArchetype || (bool)RankExcept.GetValue(config))
                return false;
            var classes = RankClasses.GetValue(config) as BlueprintCharacterClass[];
            return classes != null && classes.Contains(oracle) &&
                ReferenceEquals(RankArchetype.GetValue(config), hunter);
        }

        private static void WithholdShared(IList<FavoredClassRevelationScope> scopes)
        {
            var rankOwners = new Dictionary<ContextRankConfig, Dictionary<BlueprintScriptableObject, int>>(
                new Reference<ContextRankConfig>());
            var resourceOwners = new Dictionary<BlueprintAbilityResource, int>(ResourceComparer);
            var paramsOwners = new Dictionary<BlueprintScriptableObject, int>(ReferenceComparer);
            var gateOwners = new Dictionary<string, int>(StringComparer.Ordinal);
            Func<FavoredClassRevelationGate, string> gateKey = gate =>
                RuntimeHelpers.GetHashCode(gate.Owner) + ":" + gate.Owner.AssetGuid + "|" + gate.ComponentName;
            foreach (FavoredClassRevelationScope scope in scopes)
            {
                foreach (string key in scope.Gates.Select(gateKey).Distinct(StringComparer.Ordinal))
                    gateOwners[key] = (gateOwners.ContainsKey(key) ? gateOwners[key] : 0) + 1;
                var seen = new HashSet<KeyValuePair<ContextRankConfig, BlueprintScriptableObject>>(new SourceComparer());
                foreach (KeyValuePair<ContextRankConfig, BlueprintScriptableObject> source in scope.RankSources)
                {
                    if (!seen.Add(source))
                        continue;
                    Dictionary<BlueprintScriptableObject, int> owners;
                    if (!rankOwners.TryGetValue(source.Key, out owners))
                        rankOwners[source.Key] = owners = new Dictionary<BlueprintScriptableObject, int>(ReferenceComparer);
                    owners[source.Value] = (owners.ContainsKey(source.Value) ? owners[source.Value] : 0) + 1;
                }
                foreach (BlueprintAbilityResource resource in scope.Resources.Keys)
                    resourceOwners[resource] = (resourceOwners.ContainsKey(resource) ? resourceOwners[resource] : 0) + 1;
                foreach (BlueprintScriptableObject ability in scope.ParamsAbilities)
                    paramsOwners[ability] = (paramsOwners.ContainsKey(ability) ? paramsOwners[ability] : 0) + 1;
            }
            foreach (FavoredClassRevelationScope scope in scopes)
            {
                foreach (KeyValuePair<ContextRankConfig, BlueprintScriptableObject> source in scope.RankSources
                    .Where(source => rankOwners[source.Key][source.Value] > 1).ToList())
                {
                    scope.RankSources.Remove(source);
                    scope.Evidence.Add("withheld-shared-rank:" + source.Value.name + "|" + source.Key.Type);
                }
                foreach (BlueprintAbilityResource resource in scope.Resources.Keys
                    .Where(resource => resourceOwners[resource] > 1).ToList())
                {
                    scope.Resources.Remove(resource);
                    scope.Evidence.Add("withheld-shared-resource:" + resource.name);
                }
                foreach (BlueprintScriptableObject ability in scope.ParamsAbilities
                    .Where(ability => paramsOwners[ability] > 1).ToList())
                {
                    scope.ParamsAbilities.Remove(ability);
                    scope.Evidence.Add("withheld-shared-params:" + ability.name);
                }
                // A shared gate cannot follow one target's steps; the target
                // would be partial, so it is withheld whole.
                foreach (FavoredClassRevelationGate gate in scope.Gates.Where(gate => gateOwners[gateKey(gate)] > 1)
                    .ToList())
                {
                    scope.Gates.Remove(gate);
                    scope.Evidence.Add("withheld-shared-gate:" + gate.Label);
                    scope.PartialReason = "a level gate shared with another revelation (" + gate.Label + ")";
                }
            }
        }

        private static Dictionary<BlueprintScriptableObject, Dictionary<string, FavoredClassRevelationScope>>
            NewGateIndex()
        {
            return new Dictionary<BlueprintScriptableObject, Dictionary<string, FavoredClassRevelationScope>>(
                ReferenceComparer);
        }

        private static Dictionary<ContextRankConfig, Dictionary<BlueprintScriptableObject, FavoredClassRevelationScope>>
            NewRankIndex()
        {
            return new Dictionary<ContextRankConfig, Dictionary<BlueprintScriptableObject, FavoredClassRevelationScope>>(
                new Reference<ContextRankConfig>());
        }

        private struct Node
        {
            internal Node(object value, int depth, BlueprintScriptableObject owner)
            {
                Value = value;
                Depth = depth;
                Owner = owner;
            }

            internal readonly object Value;
            internal readonly int Depth;

            /// <summary>The blueprint whose own context evaluates this component or action.</summary>
            internal readonly BlueprintScriptableObject Owner;
        }

        private sealed class Reference<T> : IEqualityComparer<T> where T : class
        {
            public bool Equals(T x, T y) { return ReferenceEquals(x, y); }

            public int GetHashCode(T value) { return RuntimeHelpers.GetHashCode(value); }
        }

        private sealed class SourceComparer : IEqualityComparer<KeyValuePair<ContextRankConfig, BlueprintScriptableObject>>
        {
            public bool Equals(KeyValuePair<ContextRankConfig, BlueprintScriptableObject> x,
                KeyValuePair<ContextRankConfig, BlueprintScriptableObject> y)
            {
                return ReferenceEquals(x.Key, y.Key) && ReferenceEquals(x.Value, y.Value);
            }

            public int GetHashCode(KeyValuePair<ContextRankConfig, BlueprintScriptableObject> value)
            {
                return RuntimeHelpers.GetHashCode(value.Key) * 397 ^ RuntimeHelpers.GetHashCode(value.Value);
            }
        }
    }
}
