using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private const string AdapterNamePrefix = "KMG_Summoning_Adapter_";

        /// <summary>The scenario waits until every alternate family has
        /// settled; reconciliation runs on the first update after the
        /// blueprint dictionary has loaded.</summary>
        private static bool AlternateSummoningSettled()
        {
            IReadOnlyList<AlternateSummoningFamilyStatus> statuses =
                AlternateSummoningStatusRegistry.Snapshot();
            return statuses.Count == AlternateSummoningSourceCatalog.All.Count &&
                statuses.All(value => value.Disposition !=
                    AlternateSummoningDisposition.Pending);
        }

        /// <summary>
        /// Read-only, save-free observation of the installed alternate
        /// summoning graph after reconciliation. For every published caller it
        /// compares each adapter child with that caller's exact template -
        /// resource and spend, pools and their clearing, duration rate, action,
        /// checkers and custom cost - and with the canonical child it composes
        /// from, and checks the per-tier roster against the central catalog. It
        /// then lists every other spawn graph in the library and every
        /// component that references one of the eighteen ordinary spell
        /// parents. It casts nothing and changes nothing; it cannot qualify
        /// character menus, resource consumption, animation or persistence.
        /// </summary>
        private RuntimeTestResult RunAlternateSummoningSourcesObservation()
        {
            WriteLifecycleStage("alternate-summoning-sources-start");
            var assertions = new List<RuntimeTestAssertion>();
            var families = new JArray();
            var census = new JArray();
            var references = new JArray();
            var document = new JObject {
                ["scope"] = "Save-free read-only observation of alternate summoning adapters, host templates, discovered spawn graphs and canonical-parent references. No cast, input, save or mutation.",
                ["limits"] = "Structural and graph evidence only. Character ability menus, actual resource consumption, summon execution, animation and save persistence are not exercised.",
                ["runId"] = _request.RunId,
                ["families"] = families,
                ["spawnGraphCensus"] = census,
                ["canonicalParentReferences"] = references
            };
            try
            {
                LibraryScriptableObject library = BlueprintBootstrap.Library;
                bool moduleActive = _context.FeatureModules.Active.ExpandedSummoning;
                BlueprintScriptableObject[] all = library.GetAllBlueprints()
                    .Where(value => value != null).ToArray();
                ILookup<string, BlueprintAbility> abilities = all
                    .OfType<BlueprintAbility>().ToLookup(value => value.name,
                        StringComparer.Ordinal);
                AlternateSummoningRoster roster = AlternateSummoningRoster.Production();
                bool cotwActive = AlternateSummoningSourceCatalog.All.Any(family =>
                    family.Provider == AlternateSummoningProvider.CallOfTheWild &&
                    Status(family.Key) != AlternateSummoningDisposition.AbsentInProfile);
                document["moduleActive"] = moduleActive;
                document["callOfTheWildObserved"] = cotwActive;
                document["coordinatorPendingUpdates"] =
                    AlternateSummoningCoordinator.PendingUpdates;
                foreach (AlternateSummoningFamilySpec family in
                    AlternateSummoningSourceCatalog.All)
                {
                    AlternateSummoningFamilyStatus status =
                        AlternateSummoningStatusRegistry.Find(family.Key);
                    AlternateSummoningFamilyIdentity identity =
                        AlternateSummoningSourceCatalog.Identities.Single(value =>
                            value.Key == family.Key);
                    IReadOnlyList<AlternateSummoningChoice> choices =
                        AlternateSummoningProjection.Identities(identity, roster);
                    int registered = choices.Count(value => abilities[
                        AlternateSummoningAdapterBuilder.InternalName(value.Symbol)]
                            .Count() == 1);
                    var failures = new List<string>();
                    var record = new JObject {
                        ["family"] = family.Key,
                        ["provider"] = family.Provider.ToString(),
                        ["status"] = status == null ? "missing" : status.ToString(),
                        ["identities"] = choices.Count,
                        ["registered"] = registered
                    };
                    bool pass;
                    if (status == null)
                        pass = false;
                    else if (status.Disposition == AlternateSummoningDisposition.Published)
                    {
                        record["roots"] = VerifyAlternateFamily(library,
                            abilities, family, identity, roster, choices, failures);
                        pass = moduleActive && registered == choices.Count &&
                            failures.Count == 0;
                    }
                    else if (status.Disposition ==
                        AlternateSummoningDisposition.RegisteredModuleOff)
                        pass = !moduleActive && registered == choices.Count;
                    else if (status.Disposition ==
                        AlternateSummoningDisposition.AbsentInProfile)
                        pass = family.Provider == AlternateSummoningProvider
                            .CallOfTheWild && registered == 0;
                    else
                        pass = false;
                    record["failures"] = new JArray(failures.Take(60));
                    families.Add(record);
                    assertions.Add(Assertion("alternate-summoning-" + family.Key,
                        moduleActive ? "Published with every adapter child matching its caller template and canonical child (or AbsentInProfile without CotW)" :
                            "RegisteredModuleOff with every identity registered",
                        (status == null ? "missing" : status.Disposition + ":" +
                            status.Reason) + ";registered=" + registered + "/" +
                            choices.Count + ";failures=" + failures.Count +
                            (failures.Count == 0 ? "" : ";first=" + failures[0]),
                        pass, "live library after first-update reconciliation"));
                }

                int canonicalReferences;
                bool canonicalExact = ExpandedSummoningPublisher
                    .RequiredBasePublicationIsExact(library, moduleActive,
                        out canonicalReferences);
                BlueprintAbility[] parents = ExpandedSummoningPublisher
                    .CanonicalParentGuids.Select(guid => BlueprintLibraryLookup
                        .RequireExact<BlueprintAbility>(library, guid,
                            "canonical parent")).ToArray();
                int adapterUnderCanonical = parents.Sum(parent => (parent.Variants ??
                    new BlueprintAbility[0]).Count(value => value != null &&
                        value.name.StartsWith(AdapterNamePrefix, StringComparison.Ordinal)));
                assertions.Add(Assertion("alternate-summoning-canonical-parents-unchanged",
                    "eighteen canonical parents exact; no adapter child among them",
                    "exact=" + canonicalExact + ";references=" + canonicalReferences +
                        ";adapterChildren=" + adapterUnderCanonical,
                    canonicalExact && adapterUnderCanonical == 0,
                    "RequiredBasePublicationIsExact on the canonical inventory"));

                CensusSpawnGraphs(library, all, parents, census, document);
                CensusCanonicalReferences(all, parents, references);
            }
            catch (Exception exception)
            {
                document["exception"] = exception.ToString();
                assertions.Add(Assertion("alternate-summoning-observation",
                    "read-only observation completes", exception.GetType().Name +
                        ":" + exception.Message, false,
                    "unknown state is not qualification"));
            }

            RuntimeTestResultWriter.WriteAtomic(Path.Combine(
                _request.EvidenceDirectory, "alternate-summoning-sources.json"),
                document.ToString(Formatting.Indented));
            WriteLifecycleStage("alternate-summoning-sources-written");
            assertions.Add(Assertion("loaded-mod-version",
                _request.ExpectedModVersion, _context.ModEntry.Info.Version,
                _context.ModEntry.Info.Version == _request.ExpectedModVersion,
                "Unity Mod Manager ModEntry.Info.Version"));
            return CreateResult(assertions.All(value => value.Status == "PASS")
                ? "PASS" : "FAIL", assertions, null);
        }

        private static AlternateSummoningDisposition? Status(string key)
        {
            AlternateSummoningFamilyStatus status =
                AlternateSummoningStatusRegistry.Find(key);
            return status == null ? (AlternateSummoningDisposition?)null :
                status.Disposition;
        }

        private static JArray VerifyAlternateFamily(LibraryScriptableObject library,
            ILookup<string, BlueprintAbility> abilities,
            AlternateSummoningFamilySpec family, AlternateSummoningFamilyIdentity identity,
            AlternateSummoningRoster roster, IReadOnlyList<AlternateSummoningChoice> choices,
            List<string> failures)
        {
            var rows = new JArray();
            var byName = choices.ToDictionary(value =>
                AlternateSummoningAdapterBuilder.InternalName(value.Symbol),
                StringComparer.Ordinal);
            var kmgUnits = new HashSet<string>(ExpandedSummoningCatalog.All.Select(
                creature => AlternateSummoningAdapterBuilder.InternalName(
                    ExpandedSummoningIdentityCatalog.UnitSymbol(creature))),
                StringComparer.Ordinal);
            foreach (AlternateSummoningRootSpec spec in family.Roots)
            {
                BlueprintAbility[] roots;
                if (spec.RootGuid != null)
                    roots = new[] { BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                        library, spec.RootGuid, family.Key + " root") };
                else
                    roots = AlternateSummoningGraph.References<BlueprintAbility>(
                        BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library,
                            spec.FeatureGuid, family.Key + " feature").ComponentsArray)
                        .Distinct().ToArray();
                var tierChildren = new List<string>();
                foreach (BlueprintAbility root in roots)
                {
                    BlueprintAbility[] variants = root.Variants ?? new BlueprintAbility[0];
                    BlueprintAbility[] adapters = variants.Where(value => value != null &&
                        value.name.StartsWith(AdapterNamePrefix, StringComparison.Ordinal))
                        .ToArray();
                    BlueprintAbility[] host = variants.Where(value => value != null &&
                        !value.name.StartsWith(AdapterNamePrefix, StringComparison.Ordinal))
                        .ToArray();
                    bool published = AlternateSummoningExposure.IsPublishedRoot(root);
                    if (!published) failures.Add(root.name + ":root-not-published");
                    var quantities = new HashSet<SummonMultiplicity>();
                    foreach (BlueprintAbility child in adapters)
                    {
                        AlternateSummoningChoice choice;
                        if (!byName.TryGetValue(child.name, out choice))
                        { failures.Add(child.name + ":unknown-adapter-child"); continue; }
                        if (choice.Kind != AlternateSummoningChoiceKind.Preserved)
                            tierChildren.Add(child.name);
                        quantities.Add(choice.Multiplicity);
                        BlueprintAbility template = spec.IsDirect || spec.RootGuid == null ?
                            root : BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                                library, spec.Templates.Single(value =>
                                    value.Multiplicity == choice.Multiplicity).Guid,
                                "template");
                        CompareAdapterChild(library, abilities, child, root, template,
                            choice, failures);
                    }
                    foreach (AlternateSummoningTemplateSpec template in spec.Templates)
                    {
                        BlueprintAbility live = BlueprintLibraryLookup.RequireExact<
                            BlueprintAbility>(library, template.Guid, "template");
                        if (Spawns(live).Any(value => value.Blueprint != null &&
                            kmgUnits.Contains(value.Blueprint.name)))
                            failures.Add(live.name + ":host-template-mutated");
                    }
                    if (spec.RootGuid == null && quantities.Count > 1)
                        failures.Add(root.name + ":direct-root-mixes-quantities");
                    rows.Add(new JObject {
                        ["tier"] = spec.Tier, ["ordinal"] = spec.Ordinal,
                        ["root"] = root.name, ["guid"] = root.AssetGuid,
                        ["published"] = published, ["variants"] = variants.Length,
                        ["adapterChildren"] = adapters.Length,
                        ["hostChildren"] = new JArray(host.Select(value =>
                            value.name + ":" + value.AssetGuid))
                    });
                }
                string[] expected = AlternateSummoningProjection.Exposed(identity, roster,
                    spec.Tier, null).Select(value => AlternateSummoningAdapterBuilder
                        .InternalName(value.Symbol)).OrderBy(value => value,
                            StringComparer.Ordinal).ToArray();
                string[] observed = tierChildren.Where(value =>
                    byName[value].Kind == AlternateSummoningChoiceKind.Creature)
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray();
                if (!expected.SequenceEqual(observed, StringComparer.Ordinal))
                    failures.Add(family.Key + ".tier" + spec.Tier +
                        ":roster-parity expected=" + expected.Length + " observed=" +
                        observed.Length + " missing=" + string.Join(",",
                            expected.Except(observed).Take(5).ToArray()) + " extra=" +
                        string.Join(",", observed.Except(expected).Take(5).ToArray()));
            }
            return rows;
        }

        private static void CompareAdapterChild(LibraryScriptableObject library,
            ILookup<string, BlueprintAbility> abilities, BlueprintAbility child,
            BlueprintAbility root, BlueprintAbility template,
            AlternateSummoningChoice choice, List<string> failures)
        {
            string id = child.name;
            if (!ReferenceEquals(child.Parent, root)) failures.Add(id + ":parent");
            if (child.ComponentsArray.OfType<AbilityVariants>().Any())
                failures.Add(id + ":nested-variants");
            if (child.ComponentsArray.OfType<AlternateSummoningAvailability>().Count() != 1)
                failures.Add(id + ":availability-gate");
            if (!AlternateSummoningExposure.IsExposed(child.AssetGuid))
                failures.Add(id + ":not-exposed");
            if (child.AvailabilityProviders.OfType<AlternateSummoningAvailability>()
                .Count() != 1)
                failures.Add(id + ":gate-not-in-provider-cache");
            if (child.Type != template.Type || child.ActionType != template.ActionType ||
                child.Range != template.Range ||
                child.IsFullRoundAction != template.IsFullRoundAction ||
                !ReferenceEquals(child.LocalizedDuration, template.LocalizedDuration))
                failures.Add(id + ":ability-shape");
            if (Describe(child.ComponentsArray.OfType<AbilityResourceLogic>()) !=
                Describe(template.ComponentsArray.OfType<AbilityResourceLogic>()))
                failures.Add(id + ":resource-logic");
            if (TypeSet(child.ComponentsArray.OfType<IAbilityCasterChecker>()) !=
                TypeSet(template.ComponentsArray.OfType<IAbilityCasterChecker>()) ||
                TypeSet(child.ComponentsArray.OfType<IAbilityResourceCostCalculator>()) !=
                TypeSet(template.ComponentsArray.OfType<IAbilityResourceCostCalculator>()) ||
                child.ComponentsArray.OfType<ContextRankConfig>().Count() !=
                template.ComponentsArray.OfType<ContextRankConfig>().Count() ||
                child.ComponentsArray.OfType<SpellComponent>().Select(value =>
                    value.School).FirstOrDefault() != template.ComponentsArray
                    .OfType<SpellComponent>().Select(value => value.School).FirstOrDefault())
                failures.Add(id + ":caller-components");
            ContextActionSpawnMonster[] mine = Spawns(child);
            ContextActionSpawnMonster[] theirs = Spawns(template);
            if (mine.Length == 0 || mine.Length != theirs.Length ||
                PoolSet(mine.Select(value => value.SummonPool)) !=
                    PoolSet(theirs.Select(value => value.SummonPool)) ||
                string.Join(",", mine.Select(value => value.DurationValue == null ? "" :
                    value.DurationValue.Rate.ToString()).ToArray()) !=
                string.Join(",", theirs.Select(value => value.DurationValue == null ? "" :
                    value.DurationValue.Rate.ToString()).ToArray()))
                failures.Add(id + ":spawn-pools-or-duration");
            if (PoolSet(AlternateSummoningGraph.Objects<ContextActionClearSummonPool>(
                    child.ComponentsArray).Select(value => value.SummonPool)) !=
                PoolSet(AlternateSummoningGraph.Objects<ContextActionClearSummonPool>(
                    template.ComponentsArray).Select(value => value.SummonPool)))
                failures.Add(id + ":pool-clearing");
            if (choice.Kind == AlternateSummoningChoiceKind.Preserved) return;
            BlueprintAbility canonical = abilities[AlternateSummoningAdapterBuilder
                .InternalName(choice.CanonicalSymbol)].SingleOrDefault();
            if (canonical == null) { failures.Add(id + ":canonical-missing"); return; }
            BlueprintUnit expectedUnit = Spawns(canonical).Select(value =>
                value.Blueprint).Distinct().SingleOrDefault();
            if (expectedUnit == null || mine.Any(value =>
                !ReferenceEquals(value.Blueprint, expectedUnit)))
                failures.Add(id + ":spawned-unit");
            BlueprintUnitFactAccess access = BlueprintUnitFactAccess.Resolve();
            if (!ReferenceEquals(access.GetDisplayName(child),
                    access.GetDisplayName(canonical)) ||
                !ReferenceEquals(access.GetDescription(child),
                    access.GetDescription(canonical)) ||
                !ReferenceEquals(child.Icon, canonical.Icon) || child.Icon == null)
                failures.Add(id + ":presentation");
            int templates = AlternateSummoningGraph.Objects<
                ContextActionApplySummonTemplateByCaster>(child.ComponentsArray).Count;
            int alignments = AlternateSummoningGraph.Objects<
                ContextActionSetSummonAlignment>(child.ComponentsArray).Count;
            if (choice.Kind == AlternateSummoningChoiceKind.Creature)
            {
                SummonVariantSpec placement = choice.Placement;
                bool dynamic = placement.Family == SummonFamily.Monster &&
                    placement.Creature.MonsterTemplated;
                bool ally = placement.Family == SummonFamily.NaturesAlly;
                if ((dynamic && (templates != mine.Length || alignments != 0)) ||
                    (ally && (alignments != mine.Length || templates != 0)) ||
                    (!dynamic && !ally && (templates != 0 || alignments != 0)))
                    failures.Add(id + ":template-or-alignment templates=" + templates +
                        " alignments=" + alignments + " spawns=" + mine.Length);
            }
            else if (templates != 0 || alignments != 0)
                failures.Add(id + ":native-option-composed-template");
        }

        private static void CensusSpawnGraphs(LibraryScriptableObject library,
            BlueprintScriptableObject[] all, BlueprintAbility[] canonical,
            JArray census, JObject document)
        {
            var groups = new Dictionary<string, JObject>(StringComparer.Ordinal);
            int spawning = 0;
            var canonicalSet = new HashSet<BlueprintAbility>(canonical);
            foreach (BlueprintAbility ability in all.OfType<BlueprintAbility>())
            {
                if (ability.ComponentsArray == null || Spawns(ability).Length == 0)
                    continue;
                spawning++;
                BlueprintAbility top = ability;
                var seen = new HashSet<BlueprintAbility>();
                while (top.Parent != null && seen.Add(top)) top = top.Parent;
                string kind = canonicalSet.Contains(top) ? "canonical-parent" :
                    AlternateSummoningExposure.IsPublishedRoot(top) ? "adapter-published-root" :
                    top.name.StartsWith("KMG_", StringComparison.Ordinal) ? "kmg-owned" :
                    "other";
                string key = kind + "|" + top.name + "|" + top.AssetGuid;
                JObject group;
                if (!groups.TryGetValue(key, out group))
                {
                    group = new JObject { ["kind"] = kind, ["root"] = top.name,
                        ["guid"] = top.AssetGuid, ["type"] = top.Type.ToString(),
                        ["spawningAbilities"] = 0, ["sample"] = ability.name };
                    groups.Add(key, group);
                }
                group["spawningAbilities"] = (int)group["spawningAbilities"] + 1;
            }
            foreach (JObject group in groups.Values.OrderBy(value =>
                (string)value["kind"], StringComparer.Ordinal).ThenBy(value =>
                (string)value["root"], StringComparer.Ordinal))
                census.Add(group);
            document["spawningAbilityCount"] = spawning;
            document["spawnGraphRootCount"] = groups.Count;
        }

        private static void CensusCanonicalReferences(BlueprintScriptableObject[] all,
            BlueprintAbility[] canonical, JArray references)
        {
            var canonicalSet = new HashSet<BlueprintAbility>(canonical);
            foreach (BlueprintScriptableObject owner in all)
            {
                if (owner is BlueprintAbility && canonicalSet.Contains(
                    (BlueprintAbility)owner)) continue;
                if (owner.ComponentsArray == null || owner.name.StartsWith(
                    "KMG_Summoning_", StringComparison.Ordinal)) continue;
                foreach (BlueprintComponent component in owner.ComponentsArray)
                {
                    if (component == null) continue;
                    BlueprintAbility[] hits = AlternateSummoningGraph
                        .References<BlueprintAbility>(new[] { component })
                        .Where(canonicalSet.Contains).Distinct().ToArray();
                    if (hits.Length == 0) continue;
                    references.Add(new JObject {
                        ["owner"] = owner.name, ["guid"] = owner.AssetGuid,
                        ["ownerType"] = owner.GetType().Name,
                        ["component"] = component.GetType().FullName,
                        ["parents"] = new JArray(hits.Select(value => value.name))
                    });
                }
            }
        }

        private static ContextActionSpawnMonster[] Spawns(BlueprintAbility ability)
        {
            return ability == null ? new ContextActionSpawnMonster[0] :
                AlternateSummoningGraph.Objects<ContextActionSpawnMonster>(
                    ability.ComponentsArray).ToArray();
        }

        private static string PoolSet(IEnumerable<BlueprintSummonPool> pools)
        {
            return string.Join(",", pools.Select(value => value == null ? "none" :
                value.AssetGuid).Distinct().OrderBy(value => value,
                    StringComparer.Ordinal).ToArray());
        }

        private static string TypeSet(IEnumerable<object> values)
        {
            return string.Join(",", values.Where(value => value != null &&
                !(value is AlternateSummoningAvailability)).Select(value =>
                    value.GetType().FullName).OrderBy(value => value,
                        StringComparer.Ordinal).ToArray());
        }

        private static string Describe(IEnumerable<AbilityResourceLogic> logic)
        {
            return string.Join(";", logic.Select(value => (value.RequiredResource ==
                null ? "none" : value.RequiredResource.AssetGuid) + "/" +
                value.IsSpendResource + "/" + value.Amount + "/" + value.CostIsCustom)
                .ToArray());
        }
    }
}
