using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics.Actions;
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
        /// summoning graph after reconciliation. Every adapter child's full
        /// significant-mechanics signature - resource and spend, custom cost
        /// facts, caster conditions with their pools, every rank expression,
        /// spell traits, and the run action's spawn, quantity, duration, pool,
        /// clear and conditional structure in order - must equal its caller
        /// template's with only the deliberate creature and template/alignment
        /// changes. Every authorized host template must still render exactly as
        /// it did when it was resolved. With <paramref name="blockedFamily"/>
        /// the named family must instead be blocked with placeholders and an
        /// unchanged host. It casts nothing and changes nothing; it cannot
        /// qualify character menus, resource consumption, animation or
        /// persistence.
        /// </summary>
        private RuntimeTestResult RunAlternateSummoningSourcesObservation(
            string blockedFamily)
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
                ["blockedFamilyDrill"] = blockedFamily,
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
                document["moduleActive"] = moduleActive;
                document["coordinatorPendingUpdates"] =
                    AlternateSummoningCoordinator.PendingUpdates;
                document["exposedChildren"] = AlternateSummoningExposure.ExposedCount;
                document["publishedRoots"] = AlternateSummoningExposure.PublishedRootCount;
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
                    bool drilled = string.Equals(family.Key, blockedFamily,
                        StringComparison.Ordinal);
                    bool pass;
                    string expected;
                    if (drilled)
                    {
                        expected = "Blocked by the guarded fault drill, every identity an unavailable placeholder, host menus unchanged";
                        record["placeholders"] = VerifyBlockedFamily(library, family,
                            choices, failures);
                        pass = status != null && status.Disposition ==
                            AlternateSummoningDisposition.Blocked &&
                            status.Reason.Contains(".fault-drill:") &&
                            registered == choices.Count && failures.Count == 0;
                    }
                    else if (status == null)
                    {
                        expected = "a settled status";
                        pass = false;
                    }
                    else if (status.Disposition == AlternateSummoningDisposition.Published)
                    {
                        expected = "Published; every adapter child equal to its template signature with only the deliberate changes; templates unchanged since resolution";
                        record["roots"] = VerifyAlternateFamily(library, abilities,
                            family, identity, roster, choices, failures);
                        pass = moduleActive && registered == choices.Count &&
                            failures.Count == 0;
                    }
                    else if (status.Disposition ==
                        AlternateSummoningDisposition.RegisteredModuleOff)
                    {
                        expected = "RegisteredModuleOff with every identity registered and nothing exposed";
                        pass = !moduleActive && registered == choices.Count &&
                            AlternateSummoningExposure.ExposedCount == 0;
                    }
                    else if (status.Disposition ==
                        AlternateSummoningDisposition.AbsentInProfile)
                    {
                        expected = "AbsentInProfile without its provider";
                        pass = family.Provider == AlternateSummoningProvider
                            .CallOfTheWild && registered == 0;
                    }
                    else
                    {
                        expected = "Published (or AbsentInProfile without CotW)";
                        pass = false;
                    }
                    record["failures"] = new JArray(failures.Take(80));
                    families.Add(record);
                    assertions.Add(Assertion("alternate-summoning-" + family.Key,
                        expected, (status == null ? "missing" : status.Disposition +
                            ":" + status.Reason) + ";registered=" + registered + "/" +
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

                CensusSpawnGraphs(all, parents, census, document);
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
            IReadOnlyDictionary<string, string> renders =
                AlternateSummoningCoordinator.TemplateRenders(family.Key);
            foreach (AlternateSummoningRootSpec spec in family.Roots)
            {
                BlueprintAbility[] roots = spec.RootGuid != null ?
                    new[] { BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                        library, spec.RootGuid, family.Key + " root") } :
                    AlternateSummoningGraph.References<BlueprintAbility>(
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
                        CompareAdapterChild(abilities, child, root, template, choice,
                            failures);
                    }
                    var templates = spec.RootGuid == null || spec.IsDirect ?
                        new[] { root } : spec.Templates.Select(value =>
                            BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                                library, value.Guid, "template")).ToArray();
                    foreach (BlueprintAbility template in templates)
                    {
                        string before;
                        if (!renders.TryGetValue(template.AssetGuid, out before))
                            failures.Add(template.name + ":template-not-recorded");
                        else if (!string.Equals(before, SummonSignatureReader
                            .Read(template).Render(), StringComparison.Ordinal))
                            failures.Add(template.name + ":host-template-changed");
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

        private static void CompareAdapterChild(
            ILookup<string, BlueprintAbility> abilities, BlueprintAbility child,
            BlueprintAbility root, BlueprintAbility template,
            AlternateSummoningChoice choice, List<string> failures)
        {
            string id = child.name + ":" + child.AssetGuid;
            if (!ReferenceEquals(child.Parent, root)) failures.Add(id + ":parent");
            AlternateSummoningAvailability[] gates = (child.ComponentsArray ??
                new BlueprintComponent[0]).OfType<AlternateSummoningAvailability>()
                .ToArray();
            if (gates.Length != 1) failures.Add(id + ":availability-gate");
            if (!AlternateSummoningExposure.IsExposed(child.AssetGuid))
                failures.Add(id + ":not-exposed");
            IAbilityAvailabilityProvider[] providers = child.AvailabilityProviders;
            if (gates.Length == 1 && !providers.Any(value => ReferenceEquals(value,
                    gates[0])))
                failures.Add(id + ":own-gate-not-in-provider-cache");
            if (providers.Any(value => template.ComponentsArray != null &&
                template.ComponentsArray.Any(component => ReferenceEquals(component, value))))
                failures.Add(id + ":provider-cache-shared-with-template");
            string unit;
            if (choice.Kind == AlternateSummoningChoiceKind.Preserved)
                unit = null;
            else
            {
                BlueprintAbility canonical = abilities[AlternateSummoningAdapterBuilder
                    .InternalName(choice.CanonicalSymbol)].SingleOrDefault();
                if (canonical == null) { failures.Add(id + ":canonical-missing"); return; }
                string[] units = SummonSignatureReader.Read(canonical).Spawns()
                    .Select(value => value.Unit).Distinct().ToArray();
                if (units.Length != 1) { failures.Add(id + ":canonical-unit"); return; }
                unit = units[0];
                BlueprintUnitFactAccess access = BlueprintUnitFactAccess.Resolve();
                if (!ReferenceEquals(access.GetDisplayName(child),
                        access.GetDisplayName(canonical)) ||
                    !ReferenceEquals(access.GetDescription(child),
                        access.GetDescription(canonical)) ||
                    !ReferenceEquals(child.Icon, canonical.Icon) || child.Icon == null)
                    failures.Add(id + ":presentation");
            }
            SummonVariantSpec placement = choice.Placement;
            SummonSignature expected = AlternateSummoningContracts.ExpectedAdapterChild(
                SummonSignatureReader.Read(template), unit, choice.Kind,
                placement == null ? choice.NativeOption == null ? SummonFamily.Monster :
                    choice.NativeOption.Family : placement.Family,
                placement != null && placement.Creature.MonsterTemplated);
            string difference = SummonSignature.Difference(expected,
                SummonSignatureReader.Read(child));
            if (difference != null)
                failures.Add(id + ":signature:" + difference);
            if (choice.Kind != AlternateSummoningChoiceKind.Creature) return;
            int spawns = AlternateSummoningGraph.Objects<ContextActionSpawnMonster>(
                child.ComponentsArray).Count;
            int templateActions = AlternateSummoningGraph.Objects<
                ContextActionApplySummonTemplateByCaster>(child.ComponentsArray).Count;
            if (placement.Family == SummonFamily.Monster &&
                placement.Creature.MonsterTemplated)
            {
                // The appended template action must carry the canonical
                // child's exact template, smite and neutral-mode buffs.
                BlueprintAbility canonical = abilities[AlternateSummoningAdapterBuilder
                    .InternalName(choice.CanonicalSymbol)].Single();
                ContextActionApplySummonTemplateByCaster model = AlternateSummoningGraph
                    .Objects<ContextActionApplySummonTemplateByCaster>(
                        canonical.ComponentsArray).First();
                if (templateActions != spawns || AlternateSummoningGraph.Objects<
                    ContextActionApplySummonTemplateByCaster>(child.ComponentsArray)
                    .Any(value => !ReferenceEquals(value.CelestialTemplate,
                        model.CelestialTemplate) || !ReferenceEquals(value.FiendishTemplate,
                        model.FiendishTemplate) || !ReferenceEquals(value.CelestialSmite,
                        model.CelestialSmite) || !ReferenceEquals(value.FiendishSmite,
                        model.FiendishSmite) || !ReferenceEquals(value.NeutralFiendishMode,
                        model.NeutralFiendishMode)))
                    failures.Add(id + ":template-buffs");
            }
        }

        /// <summary>The drilled family: every identity is resolvable as an
        /// unavailable placeholder with no execution graph, and every host
        /// root still has exactly the menu the host built.</summary>
        private static JObject VerifyBlockedFamily(LibraryScriptableObject library,
            AlternateSummoningFamilySpec family,
            IReadOnlyList<AlternateSummoningChoice> choices, List<string> failures)
        {
            IReadOnlyDictionary<string, BlueprintAbility> owned =
                AlternateSummoningCoordinator.OwnedIdentities(family.Key);
            int placeholders = 0;
            foreach (AlternateSummoningChoice choice in choices)
            {
                BlueprintAbility ability;
                if (!owned.TryGetValue(choice.Symbol, out ability))
                { failures.Add(choice.Symbol + ":not-owned"); continue; }
                BlueprintScriptableObject resolved;
                if (!library.BlueprintsByAssetId.TryGetValue(ability.AssetGuid,
                    out resolved) || !ReferenceEquals(resolved, ability))
                    failures.Add(choice.Symbol + ":not-resolvable");
                BlueprintComponent[] components = ability.ComponentsArray ??
                    new BlueprintComponent[0];
                if (components.Length != 1 || !(components[0] is
                    AlternateSummoningUnavailable) || ability.Parent != null ||
                    !ability.Hidden || AlternateSummoningExposure.IsExposed(
                        ability.AssetGuid) || !ability.AvailabilityProviders.Any(value =>
                            ReferenceEquals(value, components.FirstOrDefault())))
                    failures.Add(choice.Symbol + ":not-a-placeholder");
                else placeholders++;
            }
            int roots = 0;
            foreach (AlternateSummoningRootSpec spec in family.Roots)
            {
                if (spec.RootGuid == null) continue;
                BlueprintAbility root = BlueprintLibraryLookup.RequireExact<
                    BlueprintAbility>(library, spec.RootGuid, family.Key + " root");
                bool marked = (root.ComponentsArray ?? new BlueprintComponent[0])
                    .OfType<AbilityVariants>().Any(value => string.Equals(value.name,
                        AlternateSummoningExposure.VariantsComponentName,
                        StringComparison.Ordinal));
                BlueprintAbility[] variants = root.Variants ?? new BlueprintAbility[0];
                if (marked || AlternateSummoningExposure.IsPublishedRoot(root) ||
                    variants.Any(value => value != null && value.name.StartsWith(
                        AdapterNamePrefix, StringComparison.Ordinal)) ||
                    spec.Templates.Any(template => !variants.Any(value => value != null &&
                        value.AssetGuid == template.Guid)))
                    failures.Add(root.name + ":host-menu-changed");
                roots++;
            }
            return new JObject { ["placeholders"] = placeholders, ["rootsChecked"] = roots };
        }

        private static void CensusSpawnGraphs(BlueprintScriptableObject[] all,
            BlueprintAbility[] canonical, JArray census, JObject document)
        {
            var groups = new Dictionary<string, JObject>(StringComparer.Ordinal);
            int spawning = 0;
            var canonicalSet = new HashSet<BlueprintAbility>(canonical);
            foreach (BlueprintAbility ability in all.OfType<BlueprintAbility>())
            {
                if (ability.ComponentsArray == null || AlternateSummoningGraph
                    .Objects<ContextActionSpawnMonster>(ability.ComponentsArray)
                    .Count == 0) continue;
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
    }
}
