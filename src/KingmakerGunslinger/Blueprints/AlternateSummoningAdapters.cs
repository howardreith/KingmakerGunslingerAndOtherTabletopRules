using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Mechanics.Actions;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    /// <summary>Cycle-safe reads over a blueprint's own action graph. Other
    /// blueprints are leaves: the walk never enters a referenced unit, buff
    /// or ability.</summary>
    internal static class AlternateSummoningGraph
    {
        internal static IList<T> Objects<T>(object root) where T : class
        {
            var found = new List<T>();
            Visit(root, new HashSet<object>(ReferenceComparer.Instance), found, true);
            return found;
        }

        /// <summary>Blueprint references held directly by a component graph,
        /// without entering them.</summary>
        internal static IList<T> References<T>(object root)
            where T : BlueprintScriptableObject
        {
            var found = new List<T>();
            VisitReferences(root, new HashSet<object>(ReferenceComparer.Instance),
                found, true);
            return found;
        }

        internal static IReadOnlyList<string> SpawnUnits(BlueprintAbility ability)
        {
            if (ability == null) return new string[0];
            IList<ContextActionSpawnMonster> spawns = Objects<ContextActionSpawnMonster>(
                ability.ComponentsArray);
            if (spawns.Any(value => value.Blueprint == null)) return new string[0];
            return AlternateSummoningLineagePolicy.Signature(
                spawns.Select(value => value.Blueprint.AssetGuid));
        }

        private static void Visit<T>(object value, ISet<object> seen,
            ICollection<T> found, bool root) where T : class
        {
            if (value == null || value is string || value.GetType().IsValueType ||
                (!root && value is BlueprintScriptableObject) || !seen.Add(value))
                return;
            T match = value as T;
            if (match != null) found.Add(match);
            IEnumerable sequence = value as IEnumerable;
            if (sequence != null)
            {
                foreach (object item in sequence) Visit(item, seen, found, false);
                return;
            }
            foreach (FieldInfo field in Fields(value.GetType()))
                Visit(field.GetValue(value), seen, found, false);
        }

        private static void VisitReferences<T>(object value, ISet<object> seen,
            ICollection<T> found, bool root) where T : BlueprintScriptableObject
        {
            if (value == null || value is string || value.GetType().IsValueType ||
                !seen.Add(value)) return;
            BlueprintScriptableObject blueprint = value as BlueprintScriptableObject;
            if (blueprint != null && !root)
            {
                T match = blueprint as T;
                if (match != null) found.Add(match);
                return;
            }
            IEnumerable sequence = value as IEnumerable;
            if (sequence != null)
            {
                foreach (object item in sequence) VisitReferences(item, seen, found, false);
                return;
            }
            if (value is UnityEngine.Object && !(value is BlueprintComponent) &&
                !(value is Kingmaker.ElementsSystem.Element)) return;
            foreach (FieldInfo field in Fields(value.GetType()))
                VisitReferences(field.GetValue(value), seen, found, false);
        }

        private static IEnumerable<FieldInfo> Fields(Type type)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            for (Type current = type; current != null &&
                current != typeof(ScriptableObject) &&
                current != typeof(UnityEngine.Object); current = current.BaseType)
                foreach (FieldInfo field in current.GetFields(flags)) yield return field;
        }

        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            internal static readonly ReferenceComparer Instance = new ReferenceComparer();
            public new bool Equals(object x, object y) { return ReferenceEquals(x, y); }
            public int GetHashCode(object obj) { return RuntimeHelpers.GetHashCode(obj); }
        }
    }

    internal sealed class AlternateSummoningResolvedRoot
    {
        internal AlternateSummoningRootSpec Spec;
        internal BlueprintAbility Root;
        internal bool IsDirect;
        /// <summary>The quantity a direct root casts; null for a menu root.</summary>
        internal SummonMultiplicity? Quantity;
        /// <summary>The supported execution template of each quantity. Only a
        /// template that passed its family contract is ever placed here.</summary>
        internal Dictionary<SummonMultiplicity, BlueprintAbility> Templates =
            new Dictionary<SummonMultiplicity, BlueprintAbility>();
        /// <summary>A menu root's live children, including unknown ones.</summary>
        internal BlueprintAbility[] HostChildren = new BlueprintAbility[0];
        /// <summary>Template GUID -> the canonical native option its contract
        /// proves it descends from. Unknown options have no entry.</summary>
        internal Dictionary<string, string> HostLineage =
            new Dictionary<string, string>(StringComparer.Ordinal);
        internal Dictionary<string, SummonMultiplicity> HostQuantity =
            new Dictionary<string, SummonMultiplicity>(StringComparer.Ordinal);
        /// <summary>The lineage of a direct root's own cast.</summary>
        internal string RootLineage;
    }

    internal sealed class AlternateSummoningResolvedFamily
    {
        internal AlternateSummoningFamilySpec Spec;
        internal AlternateSummoningFamilyIdentity Identity;
        internal List<AlternateSummoningResolvedRoot> Roots =
            new List<AlternateSummoningResolvedRoot>();
    }

    /// <summary>
    /// The native Monster Tactician abilities, resolved once per
    /// reconciliation. They are the tactician family's own roots and the
    /// exact sources Call of the Wild's three Summon Monster callers copy, so
    /// a tactician that is not understood blocks all four.
    /// </summary>
    internal sealed class AlternateSummoningTacticianSources
    {
        internal string Failure;
        internal List<Entry> Entries = new List<Entry>();

        internal Entry Find(int tier, SummonMultiplicity quantity)
        {
            return Entries.SingleOrDefault(value => value.Tier == tier &&
                value.Quantity == quantity);
        }

        internal sealed class Entry
        {
            internal int Tier;
            internal SummonMultiplicity Quantity;
            internal BlueprintAbility Ability;
            internal SummonSignature Signature;
            internal string Lineage;
        }
    }

    /// <summary>
    /// Resolves a caller's installed host graph by exact identity and
    /// authorizes each execution template only through its family contract
    /// against its exact native source (<see cref="AlternateSummoningContracts"/>).
    /// A required template that does not match blocks the family with the
    /// first differing mechanic; an additional menu option that is not a
    /// template is never a reason to block and is preserved untouched.
    /// </summary>
    internal static class AlternateSummoningHostResolver
    {
        internal static Func<string, BlueprintScriptableObject> LibraryLookup(
            LibraryScriptableObject library)
        {
            if (library == null) throw new ArgumentNullException("library");
            return guid =>
            {
                BlueprintScriptableObject found;
                return library.BlueprintsByAssetId != null &&
                    library.BlueprintsByAssetId.TryGetValue(guid, out found) ?
                    found : null;
            };
        }

        internal static AlternateSummoningTacticianSources ResolveTactician(
            Func<string, BlueprintScriptableObject> lookup)
        {
            var result = new AlternateSummoningTacticianSources();
            try
            {
                string[] features = AlternateSummoningSourceCatalog
                    .MonsterTacticianFeatureGuids;
                for (int index = 0; index < features.Length; index++)
                {
                    int tier = index + 1;
                    BlueprintFeature feature = Require<BlueprintFeature>(lookup,
                        features[index], null, "MonsterTactician tier " + tier + " feature");
                    BlueprintAbility[] granted = AlternateSummoningGraph
                        .References<BlueprintAbility>(feature.ComponentsArray)
                        .Distinct().ToArray();
                    SummonMultiplicity[] quantities = AlternateSummoningSourceCatalog
                        .Quantities(tier).ToArray();
                    if (granted.Length != quantities.Length)
                        throw Failure("MonsterTactician", tier,
                            "granted-ability-count-" + granted.Length);
                    var seen = new HashSet<SummonMultiplicity>();
                    foreach (BlueprintAbility ability in granted)
                    {
                        SummonSignature signature = SummonSignatureReader.Read(ability);
                        var matches = new List<SummonMultiplicity>();
                        string firstReason = null;
                        foreach (SummonMultiplicity quantity in quantities)
                        {
                            SummonSignature canonical = SummonSignatureReader.Read(
                                NativeSource(lookup, SummonFamily.Monster, tier, quantity));
                            string reason = AlternateSummoningContracts
                                .CheckMonsterTactician(signature, canonical);
                            if (reason == null) matches.Add(quantity);
                            else if (firstReason == null) firstReason = reason;
                        }
                        if (matches.Count != 1 || !seen.Add(matches[0]))
                            throw Failure("MonsterTactician", tier,
                                "tactician-contract:" + ability.name + ":" +
                                (matches.Count == 0 ? firstReason :
                                    "ambiguous-or-repeated-quantity"));
                        result.Entries.Add(new AlternateSummoningTacticianSources.Entry {
                            Tier = tier, Quantity = matches[0], Ability = ability,
                            Signature = signature,
                            Lineage = NativeOption(SummonFamily.Monster, tier,
                                matches[0]).Guid });
                    }
                }
                string[] resources = result.Entries.Select(value =>
                    value.Signature.Resources[0].ToString()).Distinct().ToArray();
                if (resources.Length != 1)
                    throw new AlternateSummoningResolutionException(
                        "MonsterTactician.tactician-resource-inconsistent:" +
                        string.Join("|", resources));
            }
            catch (AlternateSummoningResolutionException exception)
            {
                result.Failure = exception.Check;
                result.Entries.Clear();
            }
            return result;
        }

        internal static AlternateSummoningResolvedFamily Resolve(
            Func<string, BlueprintScriptableObject> lookup,
            AlternateSummoningFamilySpec family, AlternateSummoningFamilyIdentity identity,
            AlternateSummoningTacticianSources tactician,
            Func<BlueprintAbility, BlueprintAbility[]> hostChildren = null)
        {
            if (lookup == null) throw new ArgumentNullException("lookup");
            if (tactician == null) throw new ArgumentNullException("tactician");
            var result = new AlternateSummoningResolvedFamily {
                Spec = family, Identity = identity };
            if (family.Provider == AlternateSummoningProvider.Native)
            {
                if (tactician.Failure != null)
                    throw new AlternateSummoningResolutionException(tactician.Failure);
                foreach (AlternateSummoningTacticianSources.Entry entry in
                    tactician.Entries)
                {
                    var root = new AlternateSummoningResolvedRoot {
                        Spec = family.Roots.Single(value => value.Tier == entry.Tier),
                        Root = entry.Ability, IsDirect = true,
                        Quantity = entry.Quantity, RootLineage = entry.Lineage };
                    root.Templates.Add(entry.Quantity, entry.Ability);
                    result.Roots.Add(root);
                }
                return result;
            }
            foreach (AlternateSummoningRootSpec spec in family.Roots)
            {
                BlueprintFeature feature = Require<BlueprintFeature>(lookup,
                    spec.FeatureGuid, spec.FeatureName, family.Key + " tier " +
                    spec.Tier + " feature");
                IList<BlueprintAbility> granted = AlternateSummoningGraph
                    .References<BlueprintAbility>(feature.ComponentsArray);
                BlueprintAbility root = Require<BlueprintAbility>(lookup,
                    spec.RootGuid, spec.RootName, family.Key + " tier " + spec.Tier +
                    " root");
                if (!granted.Any(value => ReferenceEquals(value, root)))
                    throw Failure(family.Key, spec.Tier, "feature-does-not-grant-root");
                var resolved = new AlternateSummoningResolvedRoot {
                    Spec = spec, Root = root, IsDirect = spec.IsDirect };
                if (spec.IsDirect)
                {
                    resolved.Quantity = SummonMultiplicity.One;
                    resolved.RootLineage = CheckTemplate(lookup, family, spec.Tier,
                        SummonMultiplicity.One, root, tactician);
                    resolved.Templates.Add(SummonMultiplicity.One, root);
                    result.Roots.Add(resolved);
                    continue;
                }
                BlueprintAbility[] children = hostChildren != null ?
                    hostChildren(root) : LiveHostChildren(family.Key, spec.Tier, root);
                if (children == null)
                    throw Failure(family.Key, spec.Tier, "menu-root-children-missing");
                if (children.Any(value => value == null) || children.Select(
                    value => value.AssetGuid).Distinct(StringComparer.Ordinal)
                    .Count() != children.Length)
                    throw Failure(family.Key, spec.Tier,
                        "menu-root-null-or-duplicate-child");
                foreach (AlternateSummoningTemplateSpec template in spec.Templates)
                {
                    BlueprintAbility child = children.SingleOrDefault(value =>
                        string.Equals(value.AssetGuid, template.Guid,
                            StringComparison.Ordinal));
                    if (child == null || !string.Equals(child.name, template.Name,
                        StringComparison.Ordinal) || !ReferenceEquals(child.Parent, root))
                        throw Failure(family.Key, spec.Tier,
                            "template-" + template.Multiplicity + "-identity");
                    string lineage = CheckTemplate(lookup, family, spec.Tier,
                        template.Multiplicity, child, tactician);
                    resolved.Templates.Add(template.Multiplicity, child);
                    resolved.HostQuantity.Add(child.AssetGuid, template.Multiplicity);
                    resolved.HostLineage.Add(child.AssetGuid, lineage);
                }
                // Every other child, whether another mod's option or anything
                // this module does not recognise, stays in the menu untouched.
                resolved.HostChildren = children.ToArray();
                result.Roots.Add(resolved);
            }
            return result;
        }

        /// <summary>
        /// Authorizes one execution template: its signature must equal the
        /// family contract applied to its exact native source. Returns the
        /// canonical native option the template is thereby proven to descend
        /// from.
        /// </summary>
        /// <summary>The host's own menu: exactly one variants component that
        /// this module has not already published.</summary>
        private static BlueprintAbility[] LiveHostChildren(string family, int tier,
            BlueprintAbility root)
        {
            AbilityVariants[] variants = (root.ComponentsArray ??
                new BlueprintComponent[0]).OfType<AbilityVariants>().ToArray();
            if (variants.Length != 1 || variants[0].Variants == null ||
                string.Equals(variants[0].name,
                    AlternateSummoningExposure.VariantsComponentName,
                    StringComparison.Ordinal))
                throw Failure(family, tier, "menu-root-variants");
            return variants[0].Variants;
        }

        internal static string CheckTemplate(Func<string, BlueprintScriptableObject> lookup,
            AlternateSummoningFamilySpec family, int tier, SummonMultiplicity quantity,
            BlueprintAbility template, AlternateSummoningTacticianSources tactician)
        {
            SummonSignature source;
            string lineage;
            if (family.Family == SummonFamily.Monster)
            {
                if (tactician.Failure != null)
                    throw Failure(family.Key, tier, "source-tactician-unresolved:" +
                        tactician.Failure);
                AlternateSummoningTacticianSources.Entry entry =
                    tactician.Find(tier, quantity);
                if (entry == null)
                    throw Failure(family.Key, tier, "source-tactician-missing-" + quantity);
                source = entry.Signature;
                lineage = entry.Lineage;
            }
            else
            {
                source = SummonSignatureReader.Read(NativeSource(lookup,
                    SummonFamily.NaturesAlly, tier, quantity));
                lineage = NativeOption(SummonFamily.NaturesAlly, tier, quantity).Guid;
            }
            SummonSignature actual = SummonSignatureReader.Read(template);
            if (actual.RunActions != 1 || actual.Variants != 0 || actual.HiddenSpawns != 0)
                throw Failure(family.Key, tier, "template-" + quantity + "-shape:" +
                    template.name + ":runActions=" + actual.RunActions + ",variants=" +
                    actual.Variants + ",hiddenSpawns=" + actual.HiddenSpawns);
            SummonSignature expected;
            try
            {
                expected = AlternateSummoningContracts.ExpectedTemplate(family.Key, tier,
                    source);
            }
            catch (InvalidOperationException exception)
            {
                throw Failure(family.Key, tier, "template-" + quantity +
                    "-source-unsupported:" + exception.Message);
            }
            string difference = SummonSignature.Difference(expected, actual);
            if (difference != null)
                throw Failure(family.Key, tier, "template-" + quantity + "-contract:" +
                    template.name + ":" + difference);
            return lineage;
        }

        internal static SummonNativeOptionSpec NativeOption(SummonFamily family,
            int tier, SummonMultiplicity quantity)
        {
            return SummonNativeOptionCatalog.All.Single(value => value.Family == family &&
                value.Tier == tier && value.Multiplicity == quantity);
        }

        /// <summary>The exact native child of a tier and quantity, as the
        /// native option catalog names it (at tier I, this module's frozen copy
        /// of the direct native spell).</summary>
        internal static BlueprintAbility NativeSource(
            Func<string, BlueprintScriptableObject> lookup, SummonFamily family,
            int tier, SummonMultiplicity quantity)
        {
            SummonNativeOptionSpec option = NativeOption(family, tier, quantity);
            return Require<BlueprintAbility>(lookup, option.Guid, null,
                family + " tier " + tier + " " + quantity + " native source");
        }

        private static T Require<T>(Func<string, BlueprintScriptableObject> lookup,
            string guid, string expectedName, string role) where T : BlueprintScriptableObject
        {
            T value = lookup(guid) as T;
            if (value == null)
                throw new AlternateSummoningResolutionException(role + "-missing:" + guid);
            if (expectedName != null && !string.Equals(value.name, expectedName,
                StringComparison.Ordinal))
                throw new AlternateSummoningResolutionException(role +
                    "-name-mismatch:" + guid + ":" + value.name);
            return value;
        }

        private static AlternateSummoningResolutionException Failure(string family,
            int tier, string check)
        {
            return new AlternateSummoningResolutionException(family + ".tier" + tier +
                "." + check);
        }
    }

    internal sealed class AlternateSummoningResolutionException : Exception
    {
        internal AlternateSummoningResolutionException(string check) : base(check)
        { Check = check; }

        internal string Check { get; private set; }
    }

    internal sealed class AlternateSummoningBuiltFamily
    {
        internal Dictionary<string, BlueprintAbility> BySymbol =
            new Dictionary<string, BlueprintAbility>(StringComparer.Ordinal);
        internal Dictionary<AlternateSummoningResolvedRoot, BlueprintAbility> Preserved =
            new Dictionary<AlternateSummoningResolvedRoot, BlueprintAbility>();
        internal int Registered { get { return BySymbol.Count; } }
    }

    /// <summary>
    /// Builds each caller-specific child from the caller's own template for
    /// its tier and quantity. The template supplies everything the caller
    /// owns - ability type, action, range, resource and its exact spend,
    /// class scaling, duration, pools and their clearing, prerequisites and
    /// eidolon conditions - and only the spawned unit and the canonical
    /// template/alignment actions are composed in, read from the canonical
    /// child of the same placement. Name, description and icon are the
    /// canonical child's own references.
    /// </summary>
    internal static class AlternateSummoningAdapterBuilder
    {
        /// <param name="shell">Creates the identity for a symbol. Production
        /// registers it through the manifest registry; a guarded fixture may
        /// supply an unregistered shell so it can exercise this exact builder
        /// against real templates without touching the library.</param>
        /// <param name="include">Optional subset of choices to build.</param>
        internal static AlternateSummoningBuiltFamily Build(
            Func<string, BlueprintScriptableObject> lookup,
            Func<string, BlueprintAbility> shell,
            AlternateSummoningResolvedFamily family, AlternateSummoningRoster roster,
            ExpandedSummoningBlueprintSet canonical,
            Func<AlternateSummoningChoice, bool> include = null)
        {
            if (lookup == null) throw new ArgumentNullException("lookup");
            if (shell == null) throw new ArgumentNullException("shell");
            if (canonical == null) throw new ArgumentNullException("canonical");
            BlueprintUnitFactAccess access = BlueprintUnitFactAccess.Resolve();
            var built = new AlternateSummoningBuiltFamily();
            foreach (AlternateSummoningChoice choice in AlternateSummoningProjection
                .Identities(family.Identity, roster))
            {
                if (include != null && !include(choice)) continue;
                if (choice.Kind == AlternateSummoningChoiceKind.Preserved)
                {
                    AlternateSummoningResolvedRoot root = family.Roots.Single(value =>
                        value.IsDirect && value.Spec.Tier == choice.Tier &&
                        value.Quantity == choice.Multiplicity);
                    BlueprintAbility preserved = shell(choice.Symbol);
                    ConfigurePreserved(preserved, root.Root, choice.Symbol);
                    built.BySymbol.Add(choice.Symbol, preserved);
                    built.Preserved.Add(root, preserved);
                    continue;
                }
                AlternateSummoningResolvedRoot owner = Owner(family, choice.Tier,
                    choice.Multiplicity);
                BlueprintAbility template = owner.Templates[choice.Multiplicity];
                BlueprintAbility target = shell(choice.Symbol);
                if (choice.Kind == AlternateSummoningChoiceKind.Creature)
                    ConfigureCreature(target, template, owner.Root, choice, canonical,
                        access);
                else
                    ConfigureNative(target, template, owner.Root, choice, canonical,
                        lookup, access);
                built.BySymbol.Add(choice.Symbol, target);
            }
            return built;
        }

        internal static AlternateSummoningResolvedRoot Owner(
            AlternateSummoningResolvedFamily family, int tier,
            SummonMultiplicity quantity)
        {
            AlternateSummoningResolvedRoot[] owners = family.Roots.Where(value =>
                value.Spec.Tier == tier && value.Templates.ContainsKey(quantity))
                .ToArray();
            if (owners.Length != 1) throw new AlternateSummoningResolutionException(
                family.Spec.Key + ".tier" + tier + ".no-unique-template-" + quantity);
            return owners[0];
        }

        /// <summary>A named, empty, unregistered ability for a symbol.</summary>
        internal static BlueprintAbility CreateShell(string symbol)
        {
            BlueprintAbility shell = ScriptableObject.CreateInstance<BlueprintAbility>();
            shell.name = InternalName(symbol);
            shell.Hidden = true;
            shell.ActionBarAutoFillIgnored = true;
            shell.ComponentsArray = new BlueprintComponent[0];
            shell.ResourceAssetIds = new string[0];
            shell.MaterialComponent = new BlueprintAbility.MaterialComponentData();
            return shell;
        }

        /// <summary>
        /// Turns an owned identity into a safe placeholder: no execution graph,
        /// no parent, hidden from menus and auto-fill, and permanently
        /// unavailable with a reason. A saved reference still resolves to this
        /// mod's own blueprint and can never cast. Text and icon are the
        /// canonical child's when one exists, so a saved shortcut stays
        /// recognisable.
        /// </summary>
        internal static void MakePlaceholder(BlueprintAbility ability,
            Kingmaker.Blueprints.Facts.BlueprintUnitFact presentation,
            Kingmaker.Localization.LocalizedString name,
            Kingmaker.Localization.LocalizedString description, string reason)
        {
            ability.Parent = null;
            ability.Hidden = true;
            ability.ActionBarAutoFillIgnored = true;
            AlternateSummoningUnavailable gate = ScriptableObject
                .CreateInstance<AlternateSummoningUnavailable>();
            gate.name = "$KMG_AlternateSummoning_Unavailable";
            gate.Reason = reason;
            ability.ComponentsArray = new BlueprintComponent[] { gate };
            ResetComponentCaches(ability);
            BlueprintUnitFactAccess access = BlueprintUnitFactAccess.Resolve();
            access.Configure(ability, presentation == null ? name :
                access.GetDisplayName(presentation) ?? name, description,
                presentation == null ? null : presentation.Icon);
        }

        /// <summary>
        /// Kingmaker 2.1.7b caches an ability's availability providers, target
        /// checkers and caster checkers on first use. A field copy of a host
        /// ability that has already been read would carry the host's arrays,
        /// and the clone would then ignore its own components - including its
        /// availability gate - so a clone always starts with empty caches.
        /// </summary>
        private static readonly string[] ComponentCacheFields = {
            "m_CachedAvailabilityProviders", "m_CachedTargetCheckers",
            "m_CachedCasterCheckers" };

        internal static void ResetComponentCaches(BlueprintAbility ability)
        {
            foreach (string name in ComponentCacheFields)
            {
                FieldInfo field = typeof(BlueprintAbility).GetField(name,
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null && field.FieldType.IsArray)
                    field.SetValue(ability, null);
            }
        }

        private static void CloneExecution(BlueprintAbility target,
            BlueprintAbility template, BlueprintAbility root, string symbol)
        {
            ExpandedSummoningAbilityBuilder.CopyFields(template, target);
            ResetComponentCaches(target);
            target.name = InternalName(symbol);
            target.ComponentsArray = (template.ComponentsArray ??
                new BlueprintComponent[0]).Where(value => !(value is AbilityVariants))
                .Select(ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            target.Parent = root;
            target.Hidden = false;
            target.ActionBarAutoFillIgnored = false;
            target.MaterialComponent = template.MaterialComponent ??
                new BlueprintAbility.MaterialComponentData();
        }

        private static void ConfigureCreature(BlueprintAbility target,
            BlueprintAbility template, BlueprintAbility root,
            AlternateSummoningChoice choice, ExpandedSummoningBlueprintSet canonical,
            BlueprintUnitFactAccess access)
        {
            SummonVariantSpec placement = choice.Placement;
            BlueprintAbility source = RequireCanonical<BlueprintAbility>(canonical,
                choice.CanonicalSymbol);
            BlueprintUnit unit = RequireCanonical<BlueprintUnit>(canonical,
                ExpandedSummoningIdentityCatalog.UnitSymbol(placement.Creature));
            CloneExecution(target, template, root, choice.Symbol);
            ContextActionApplySummonTemplateByCaster[] templates =
                AlternateSummoningGraph.Objects<ContextActionApplySummonTemplateByCaster>(
                    source.ComponentsArray).ToArray();
            ContextActionSetSummonAlignment[] alignments = AlternateSummoningGraph
                .Objects<ContextActionSetSummonAlignment>(source.ComponentsArray).ToArray();
            bool dynamic = placement.Family == SummonFamily.Monster &&
                placement.Creature.MonsterTemplated;
            if (dynamic)
            {
                if (templates.Length < 1 || alignments.Length != 0)
                    throw Mismatch(choice, "canonical-template-composition");
                ContextActionApplySummonTemplateByCaster model = templates[0];
                target.ComponentsArray = target.ComponentsArray.Where(value =>
                    !(value is AbilityCasterAlignment)).ToArray();
                int spawns = ExpandedSummoningAbilityBuilder.ReplaceSpawnUnits(
                    target.ComponentsArray, unit);
                if (spawns < 1) throw Mismatch(choice, "no-spawn");
                ExpandedSummoningAbilityBuilder.AppendDynamicTemplate(
                    target.ComponentsArray, model.CelestialTemplate,
                    model.FiendishTemplate, model.CelestialSmite, model.FiendishSmite,
                    model.NeutralFiendishMode, model.ReplacedNativeTemplateBuffs);
                foreach (SpellDescriptorComponent descriptor in target.ComponentsArray
                    .OfType<SpellDescriptorComponent>())
                    descriptor.Descriptor &= ~(SpellDescriptor.Good | SpellDescriptor.Evil);
            }
            else
            {
                if (templates.Length != 0)
                    throw Mismatch(choice, "canonical-untemplated-composition");
                int spawns = ExpandedSummoningAbilityBuilder.ReplaceSpawnUnits(
                    target.ComponentsArray, unit);
                if (spawns < 1) throw Mismatch(choice, "no-spawn");
                if (alignments.Length > 0)
                {
                    SummonAlignmentMode mode = alignments[0].Mode;
                    if (alignments.Any(value => value.Mode != mode))
                        throw Mismatch(choice, "canonical-alignment-modes-differ");
                    ExpandedSummoningAbilityBuilder.AppendAlignmentAction(
                        target.ComponentsArray, mode);
                }
            }
            Finish(target, source, access, unit);
        }

        private static void ConfigureNative(BlueprintAbility target,
            BlueprintAbility template, BlueprintAbility root,
            AlternateSummoningChoice choice, ExpandedSummoningBlueprintSet canonical,
            Func<string, BlueprintScriptableObject> lookup, BlueprintUnitFactAccess access)
        {
            SummonNativeExpansionSpec spec = choice.NativeOption;
            BlueprintAbility source = RequireCanonical<BlueprintAbility>(canonical,
                spec.Symbol);
            BlueprintUnit unit = lookup(spec.UnitGuid) as BlueprintUnit;
            if (unit == null) throw Mismatch(choice, "native-unit-missing");
            CloneExecution(target, template, root, choice.Symbol);
            if (ExpandedSummoningAbilityBuilder.ReplaceSpawnUnits(
                target.ComponentsArray, unit) < 1)
                throw Mismatch(choice, "no-spawn");
            Finish(target, source, access, unit);
        }

        private static void ConfigurePreserved(BlueprintAbility target,
            BlueprintAbility root, string symbol)
        {
            ExpandedSummoningAbilityBuilder.CopyFields(root, target);
            ResetComponentCaches(target);
            target.name = InternalName(symbol);
            target.ComponentsArray = (root.ComponentsArray ??
                new BlueprintComponent[0]).Where(value => !(value is AbilityVariants))
                .Select(ExpandedSummoningAbilityBuilder.DeepCloneComponent)
                .Concat(new BlueprintComponent[] { Gate() }).ToArray();
            target.Parent = root;
            target.MaterialComponent = root.MaterialComponent ??
                new BlueprintAbility.MaterialComponentData();
            if (AlternateSummoningGraph.SpawnUnits(target).Count == 0 ||
                target.ComponentsArray.OfType<AbilityVariants>().Any())
                throw new AlternateSummoningResolutionException(
                    "preserved-clone-not-executable:" + symbol);
        }

        private static void Finish(BlueprintAbility target, BlueprintAbility source,
            BlueprintUnitFactAccess access, BlueprintUnit unit)
        {
            access.Configure(target, access.GetDisplayName(source),
                access.GetDescription(source), source.Icon);
            target.ComponentsArray = target.ComponentsArray.Concat(
                new BlueprintComponent[] { Gate() }).ToArray();
            IList<ContextActionSpawnMonster> spawns = AlternateSummoningGraph
                .Objects<ContextActionSpawnMonster>(target.ComponentsArray);
            if (spawns.Count == 0 || spawns.Any(value =>
                    !ReferenceEquals(value.Blueprint, unit)) ||
                target.ComponentsArray.OfType<AbilityVariants>().Any() ||
                target.Parent == null || ReferenceEquals(target.Parent, target))
                throw new AlternateSummoningResolutionException(
                    "adapter-child-validation:" + target.name);
        }

        private static AlternateSummoningAvailability Gate()
        {
            AlternateSummoningAvailability gate = ScriptableObject
                .CreateInstance<AlternateSummoningAvailability>();
            gate.name = "$KMG_AlternateSummoning_Availability";
            return gate;
        }

        private static T RequireCanonical<T>(ExpandedSummoningBlueprintSet canonical,
            string symbol) where T : BlueprintScriptableObject
        {
            BlueprintScriptableObject value;
            T result = symbol != null && canonical.BySymbol.TryGetValue(symbol,
                out value) ? value as T : null;
            if (result == null) throw new AlternateSummoningResolutionException(
                "canonical-identity-missing:" + symbol);
            return result;
        }

        private static Exception Mismatch(AlternateSummoningChoice choice,
            string check)
        {
            return new AlternateSummoningResolutionException(choice.Symbol + ":" + check);
        }

        internal static string InternalName(string symbol)
        { return symbol.Replace('.', '_').Replace('-', '_'); }
    }

    /// <summary>
    /// One family's publication: every host root it changed, the exact
    /// component arrays before and after, and the identities it exposed.
    /// Publication is all or nothing per family; rollback refuses to
    /// overwrite a root another mutation has changed since.
    /// </summary>
    internal sealed class AlternateSummoningFamilyPublication
    {
        private readonly AlternateSummoningMutationLog<BlueprintComponent[]> _log =
            new AlternateSummoningMutationLog<BlueprintComponent[]>();
        private readonly List<string> _roots = new List<string>();
        private readonly List<string> _children = new List<string>();
        private bool _exposed;

        internal int RootCount { get { return _roots.Count; } }
        internal int ExposedCount { get { return _children.Count; } }
        internal int KeptHost { get; private set; }
        internal int RemovedHost { get; private set; }
        internal IReadOnlyList<string> Roots { get { return _roots.AsReadOnly(); } }
        internal IReadOnlyList<string> Children { get { return _children.AsReadOnly(); } }

        internal static AlternateSummoningFamilyPublication Publish(
            AlternateSummoningResolvedFamily family, AlternateSummoningBuiltFamily built,
            AlternateSummoningRoster roster)
        {
            var publication = new AlternateSummoningFamilyPublication();
            try
            {
                foreach (AlternateSummoningResolvedRoot root in family.Roots)
                    publication.PublishRoot(family, built, roster, root);
                AlternateSummoningExposure.Add(publication._roots,
                    publication._children);
                publication._exposed = true;
                return publication;
            }
            catch
            {
                publication.Rollback();
                throw;
            }
        }

        private void PublishRoot(AlternateSummoningResolvedFamily family,
            AlternateSummoningBuiltFamily built, AlternateSummoningRoster roster,
            AlternateSummoningResolvedRoot root)
        {
            BlueprintAbility preserved = null;
            IReadOnlyList<BlueprintAbility> host;
            if (root.IsDirect)
            {
                if (!built.Preserved.TryGetValue(root, out preserved))
                    throw new AlternateSummoningResolutionException(
                        family.Spec.Key + ".tier" + root.Spec.Tier +
                        ".direct-root-has-no-preservation");
                host = new[] { preserved };
            }
            else host = root.HostChildren;
            SummonMultiplicity? scope = root.IsDirect ? root.Quantity : null;
            IReadOnlyList<AlternateSummoningChoice> exposed = AlternateSummoningProjection
                .Exposed(family.Identity, roster, root.Spec.Tier, scope);
            AlternateSummoningMenuPlan<BlueprintAbility> plan =
                AlternateSummoningMenuPolicy.Compose(family.Spec.Family,
                    root.Spec.Tier, scope, host,
                    value => root.IsDirect ? root.RootLineage :
                        Lookup(root.HostLineage, value.AssetGuid),
                    value => root.IsDirect ? root.Quantity :
                        root.HostQuantity.ContainsKey(value.AssetGuid) ?
                            (SummonMultiplicity?)root.HostQuantity[value.AssetGuid] : null,
                    exposed, choice => built.BySymbol[choice.Symbol],
                    SummonNativeExpansionCatalog.All,
                    native => built.BySymbol[AlternateSummoningProjection
                        .NativeOptionSymbol(family.Spec.Key, native)]);
            // A direct root that would offer nothing but its own original cast
            // stays exactly as the host made it.
            if (root.IsDirect && plan.Added.Count == 0) return;
            BlueprintComponent[] before = root.Root.ComponentsArray ??
                new BlueprintComponent[0];
            AbilityVariants existing = before.OfType<AbilityVariants>().SingleOrDefault();
            if (root.IsDirect != (existing == null))
                throw new AlternateSummoningResolutionException(family.Spec.Key +
                    ".tier" + root.Spec.Tier + ".root-shape-changed");
            if (plan.Final.Any(value => ReferenceEquals(value, root.Root)) ||
                plan.Added.Any(value => !ReferenceEquals(value.Parent, root.Root)))
                throw new AlternateSummoningResolutionException(family.Spec.Key +
                    ".tier" + root.Spec.Tier + ".parent-cycle-or-reparent");
            var variants = ScriptableObject.CreateInstance<AbilityVariants>();
            variants.name = AlternateSummoningExposure.VariantsComponentName;
            variants.Variants = plan.Final.ToArray();
            BlueprintComponent[] after = existing == null ?
                before.Concat(new BlueprintComponent[] { variants }).ToArray() :
                before.Select(value => ReferenceEquals(value, existing) ?
                    (BlueprintComponent)variants : value).ToArray();
            BlueprintAbility target = root.Root;
            _log.Apply(target.AssetGuid, () => target.ComponentsArray,
                value => target.ComponentsArray = value, after);
            if (root.Root.Variants == null ||
                root.Root.Variants.Length != plan.Final.Count ||
                plan.KeptHost.Any(value => !root.Root.Variants.Contains(value)) ||
                plan.RemovedHost.Any(value => root.Root.Variants.Contains(value)))
                throw new AlternateSummoningResolutionException(family.Spec.Key +
                    ".tier" + root.Spec.Tier + ".publication-not-retained");
            _roots.Add(root.Root.AssetGuid);
            _children.AddRange(plan.Added.Select(value => value.AssetGuid));
            if (preserved != null && plan.KeptHost.Contains(preserved))
                _children.Add(preserved.AssetGuid);
            KeptHost += plan.KeptHost.Count;
            RemovedHost += plan.RemovedHost.Count;
        }

        internal void Rollback()
        {
            if (_exposed)
            {
                AlternateSummoningExposure.Remove(_roots, _children);
                _exposed = false;
            }
            _log.Rollback();
            _roots.Clear();
            _children.Clear();
        }

        private static string Lookup(Dictionary<string, string> map, string key)
        {
            string value;
            return map.TryGetValue(key, out value) ? value : null;
        }
    }
}
