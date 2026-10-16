using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics.Actions;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private const string ContractFixtureForeignGuid = "f1c7a5e0000040008000000000000001";

        /// <summary>
        /// Save-free contract fixture. It exercises the production resolver,
        /// contracts and builder against transient clones of the real
        /// installed Master Summoner templates, substituted through the
        /// resolver's host-children seam:
        /// <list type="bullet">
        /// <item>an unsupported spawn lineage, an extra spawn, an altered
        /// quantity and a changed caller condition must each block with the
        /// template contract;</item>
        /// <item>an unknown extra menu option must be preserved;</item>
        /// <item>a built child must not share execution data, nor a provider
        /// cache, with its host template;</item>
        /// <item>repeating reconciliation must do no work.</item>
        /// </list>
        /// Nothing is registered in, or removed from, the library; every clone
        /// is destroyed afterwards.
        /// </summary>
        private RuntimeTestResult RunAlternateSummoningContractFixture()
        {
            WriteLifecycleStage("alternate-summoning-contracts-start");
            var assertions = new List<RuntimeTestAssertion>();
            var created = new List<UnityEngine.Object>();
            var document = new JObject {
                ["scope"] = "Save-free fixture: transient clones of real installed templates passed through the production resolver, contracts and builder. Nothing is registered in or removed from the library.",
                ["runId"] = _request.RunId
            };
            LibraryScriptableObject library = BlueprintBootstrap.Library;
            int libraryCount = library.BlueprintsByAssetId.Count;
            try
            {
                Func<string, BlueprintScriptableObject> lookup =
                    AlternateSummoningHostResolver.LibraryLookup(library);
                AlternateSummoningTacticianSources tactician =
                    AlternateSummoningHostResolver.ResolveTactician(lookup);
                assertions.Add(Assertion("contracts-tactician-resolves",
                    "native Monster Tactician sources satisfy their contract",
                    tactician.Failure ?? "entries=" + tactician.Entries.Count,
                    tactician.Failure == null && tactician.Entries.Count == 24,
                    "production ResolveTactician on the live library"));

                AlternateSummoningFamilySpec family = AlternateSummoningSourceCatalog
                    .Find(AlternateSummoningSourceCatalog.MasterSummonerKey);
                AlternateSummoningFamilyIdentity identity = AlternateSummoningSourceCatalog
                    .Identities.Single(value => value.Key == family.Key);
                Func<BlueprintAbility, BlueprintAbility[]> original = root =>
                    AlternateSummoningCoordinator.OriginalHostChildren(family.Key,
                        root.AssetGuid);
                AlternateSummoningResolvedFamily baseline = AlternateSummoningHostResolver
                    .Resolve(lookup, family, identity, tactician, original);
                assertions.Add(Assertion("contracts-baseline-resolves",
                    "the unmodified installed Master Summoner graph is supported",
                    "roots=" + baseline.Roots.Count, baseline.Roots.Count == 9,
                    "production Resolve with the host's own pre-publication menus"));

                AlternateSummoningRootSpec tier = family.Roots.Single(value =>
                    value.Tier == 3);
                AlternateSummoningTemplateSpec target = tier.Templates.Single(value =>
                    value.Multiplicity == SummonMultiplicity.OneD3);
                BlueprintAbility template = BlueprintLibraryLookup.RequireExact<
                    BlueprintAbility>(library, target.Guid, "Master Summoner III d3");
                string templateBefore = SummonSignatureReader.Read(template).Render();
                string otherUnit = SummonSignatureReader.Read(AlternateSummoningHostResolver
                    .NativeSource(lookup, SummonFamily.Monster, 3, SummonMultiplicity.One))
                    .Spawns().First().Unit;
                BlueprintUnit replacement = (BlueprintUnit)lookup(otherUnit);
                var negatives = new JObject();
                document["negatives"] = negatives;

                Negative(assertions, negatives, "unsupported-lineage", family, identity,
                    tactician, lookup, original, tier, template, created, clone =>
                    {
                        foreach (ContextActionSpawnMonster spawn in AlternateSummoningGraph
                            .Objects<ContextActionSpawnMonster>(clone.ComponentsArray))
                            spawn.Blueprint = replacement;
                    }, otherUnit);
                Negative(assertions, negatives, "additional-spawn", family, identity,
                    tactician, lookup, original, tier, template, created, clone =>
                    {
                        AbilityEffectRunAction run = clone.ComponentsArray
                            .OfType<AbilityEffectRunAction>().Single();
                        GameAction extra = (GameAction)ExpandedSummoningAbilityBuilder
                            .DeepCloneComponentObject(AlternateSummoningGraph
                                .Objects<ContextActionSpawnMonster>(clone.ComponentsArray)
                                .First());
                        run.Actions = new ActionList { Actions = run.Actions.Actions
                            .Concat(new[] { extra }).ToArray() };
                    }, "spawn unit=");
                Negative(assertions, negatives, "altered-quantity", family, identity,
                    tactician, lookup, original, tier, template, created, clone =>
                    {
                        foreach (ContextActionSpawnMonster spawn in AlternateSummoningGraph
                            .Objects<ContextActionSpawnMonster>(clone.ComponentsArray))
                            spawn.CountValue.DiceType = DiceType.D6;
                    }, "D6");
                Negative(assertions, negatives, "changed-caller-condition", family,
                    identity, tactician, lookup, original, tier, template, created, clone =>
                    {
                        BlueprintComponent checker = clone.ComponentsArray.Single(value =>
                            value != null && value.GetType().FullName ==
                                AlternateSummoningContracts.CompanionDeadOrPoolEmptyType);
                        FieldInfo pool = checker.GetType().GetField("SummonPool");
                        pool.SetValue(checker, lookup(AlternateSummoningContracts.SummonerPool));
                    }, "AbilityCasterCompanionDeadOrSummonPoolEmpty");

                // An unknown extra option: kept untouched, never authorizing.
                BlueprintAbility foreign = CloneForFixture(AlternateSummoningHostResolver
                    .NativeSource(lookup, SummonFamily.Monster, 3, SummonMultiplicity.One),
                    ContractFixtureForeignGuid, "KMGFixtureForeignOption", created);
                AlternateSummoningResolvedFamily withForeign = AlternateSummoningHostResolver
                    .Resolve(lookup, family, identity, tactician, root =>
                        root.AssetGuid == tier.RootGuid ? original(root).Concat(
                            new[] { foreign }).ToArray() : original(root));
                AlternateSummoningResolvedRoot foreignRoot = withForeign.Roots.Single(
                    value => value.Spec.Tier == 3);
                AlternateSummoningMenuPlan<BlueprintAbility> plan =
                    AlternateSummoningMenuPolicy.Compose(SummonFamily.Monster, 3, null,
                        foreignRoot.HostChildren, value =>
                        {
                            string lineage;
                            return foreignRoot.HostLineage.TryGetValue(value.AssetGuid,
                                out lineage) ? lineage : null;
                        }, value => foreignRoot.HostQuantity.ContainsKey(value.AssetGuid) ?
                            (SummonMultiplicity?)foreignRoot.HostQuantity[value.AssetGuid] :
                            null, new AlternateSummoningChoice[0], choice => null,
                        new SummonNativeExpansionSpec[0], native => null);
                assertions.Add(Assertion("contracts-unknown-option-preserved",
                    "an extra non-template option resolves as unknown and stays in the menu",
                    "inHost=" + foreignRoot.HostChildren.Contains(foreign) + ";lineage=" +
                        foreignRoot.HostLineage.ContainsKey(ContractFixtureForeignGuid) +
                        ";kept=" + plan.KeptHost.Contains(foreign),
                    foreignRoot.HostChildren.Contains(foreign) &&
                        !foreignRoot.HostLineage.ContainsKey(ContractFixtureForeignGuid) &&
                        plan.KeptHost.Contains(foreign) && !plan.RemovedHost.Contains(foreign),
                    "production Resolve and Compose with one foreign child"));

                // The production builder on the real resolved family, tier III
                // only, into unregistered shells.
                var shells = new List<BlueprintAbility>();
                AlternateSummoningBuiltFamily built = AlternateSummoningAdapterBuilder.Build(
                    lookup, symbol =>
                    {
                        BlueprintAbility shell = AlternateSummoningAdapterBuilder
                            .CreateShell(symbol);
                        shells.Add(shell);
                        created.Add(shell);
                        return shell;
                    }, baseline, AlternateSummoningRoster.Production(),
                    ExpandedSummoningBlueprints.Registered, choice => choice.Tier == 3 &&
                        choice.Kind == AlternateSummoningChoiceKind.Creature &&
                        choice.Multiplicity == SummonMultiplicity.OneD3);
                BlueprintAbility child = built.BySymbol.Values.First();
                string unchangedAfterBuild = SummonSignatureReader.Read(template).Render();
                IList<ContextActionSpawnMonster> childSpawns = AlternateSummoningGraph
                    .Objects<ContextActionSpawnMonster>(child.ComponentsArray);
                childSpawns[0].Blueprint = replacement;
                string unchangedAfterMutation = SummonSignatureReader.Read(template).Render();
                AlternateSummoningAvailability gate = child.ComponentsArray
                    .OfType<AlternateSummoningAvailability>().Single();
                IAbilityAvailabilityProvider[] providers = child.AvailabilityProviders;
                bool sharedProvider = providers.Any(value => template.ComponentsArray
                    .Any(component => ReferenceEquals(component, value)));
                bool sharedAction = AlternateSummoningGraph.Objects<GameAction>(
                    child.ComponentsArray).Any(action => AlternateSummoningGraph
                        .Objects<GameAction>(template.ComponentsArray).Any(other =>
                            ReferenceEquals(action, other)));
                assertions.Add(Assertion("contracts-builder-isolation",
                    "building and then changing a child leaves its host template's execution data unchanged and shares no action object",
                    "built=" + built.Registered + ";afterBuild=" +
                        (unchangedAfterBuild == templateBefore) + ";afterMutation=" +
                        (unchangedAfterMutation == templateBefore) + ";sharedAction=" +
                        sharedAction,
                    built.Registered > 0 && unchangedAfterBuild == templateBefore &&
                        unchangedAfterMutation == templateBefore && !sharedAction,
                    "production AlternateSummoningAdapterBuilder.Build"));
                assertions.Add(Assertion("contracts-own-availability-gate",
                    "the executed child's provider cache holds its own gate and none of the template's components",
                    "ownGate=" + providers.Any(value => ReferenceEquals(value, gate)) +
                        ";sharedWithTemplate=" + sharedProvider + ";exposed=" +
                        AlternateSummoningExposure.IsExposed(child.AssetGuid),
                    providers.Any(value => ReferenceEquals(value, gate)) && !sharedProvider &&
                        !AlternateSummoningExposure.IsExposed(child.AssetGuid),
                    "BlueprintAbility.AvailabilityProviders on the built child"));

                // Repeating reconciliation must not register or publish again.
                AlternateSummoningFamilyStatus[] before =
                    AlternateSummoningStatusRegistry.Snapshot().ToArray();
                int exposed = AlternateSummoningExposure.ExposedCount;
                int roots = AlternateSummoningExposure.PublishedRootCount;
                int registeredBefore = library.BlueprintsByAssetId.Count;
                bool settled = AlternateSummoningCoordinator.TryReconcile(
                    "contract-fixture-repeat");
                AlternateSummoningFamilyStatus[] after =
                    AlternateSummoningStatusRegistry.Snapshot().ToArray();
                bool same = before.Length == after.Length && before.Zip(after,
                    (left, right) => ReferenceEquals(left, right)).All(value => value);
                assertions.Add(Assertion("contracts-repeat-reconcile-idempotent",
                    "a repeated reconciliation settles without new registration, exposure or status",
                    "settled=" + settled + ";sameStatuses=" + same + ";exposed=" +
                        exposed + "->" + AlternateSummoningExposure.ExposedCount +
                        ";roots=" + roots + "->" + AlternateSummoningExposure.PublishedRootCount +
                        ";library=" + registeredBefore + "->" + library.BlueprintsByAssetId.Count,
                    settled && same && exposed == AlternateSummoningExposure.ExposedCount &&
                        roots == AlternateSummoningExposure.PublishedRootCount &&
                        registeredBefore == library.BlueprintsByAssetId.Count,
                    "AlternateSummoningCoordinator.TryReconcile"));
            }
            catch (Exception exception)
            {
                document["exception"] = exception.ToString();
                assertions.Add(Assertion("alternate-summoning-contracts",
                    "fixture completes", exception.GetType().Name + ":" +
                        exception.Message, false, "unknown state is not qualification"));
            }
            finally
            {
                foreach (UnityEngine.Object value in created)
                    if (value != null) UnityEngine.Object.DestroyImmediate(value);
            }
            assertions.Add(Assertion("contracts-library-unchanged",
                "the fixture registers and removes nothing in the blueprint library",
                library.BlueprintsByAssetId.Count + ";foreign=" +
                    library.BlueprintsByAssetId.ContainsKey(ContractFixtureForeignGuid),
                library.BlueprintsByAssetId.Count == libraryCount &&
                    !library.BlueprintsByAssetId.ContainsKey(ContractFixtureForeignGuid),
                "LibraryScriptableObject.BlueprintsByAssetId"));
            RuntimeTestResultWriter.WriteAtomic(Path.Combine(
                _request.EvidenceDirectory, "alternate-summoning-contracts.json"),
                document.ToString(Formatting.Indented));
            WriteLifecycleStage("alternate-summoning-contracts-written");
            assertions.Add(Assertion("loaded-mod-version",
                _request.ExpectedModVersion, _context.ModEntry.Info.Version,
                _context.ModEntry.Info.Version == _request.ExpectedModVersion,
                "Unity Mod Manager ModEntry.Info.Version"));
            return CreateResult(assertions.All(value => value.Status == "PASS")
                ? "PASS" : "FAIL", assertions, null);
        }

        private static void Negative(List<RuntimeTestAssertion> assertions,
            JObject record, string name, AlternateSummoningFamilySpec family,
            AlternateSummoningFamilyIdentity identity,
            AlternateSummoningTacticianSources tactician,
            Func<string, BlueprintScriptableObject> lookup,
            Func<BlueprintAbility, BlueprintAbility[]> original,
            AlternateSummoningRootSpec tier, BlueprintAbility template,
            List<UnityEngine.Object> created, Action<BlueprintAbility> mutate,
            string expectedToken)
        {
            BlueprintAbility clone = CloneForFixture(template, template.AssetGuid,
                template.name, created);
            mutate(clone);
            string reason = null;
            try
            {
                AlternateSummoningHostResolver.Resolve(lookup, family, identity,
                    tactician, root => root.AssetGuid == tier.RootGuid ?
                        original(root).Select(value => ReferenceEquals(value, template) ?
                            clone : value).ToArray() : original(root));
            }
            catch (AlternateSummoningResolutionException exception)
            {
                reason = exception.Check;
            }
            record[name] = reason;
            string prefix = family.Key + ".tier3.template-OneD3-contract:";
            assertions.Add(Assertion("contracts-rejects-" + name,
                "the resolver blocks the family at the changed template", reason ?? "accepted",
                reason != null && reason.StartsWith(prefix, StringComparison.Ordinal) &&
                    reason.Contains(expectedToken),
                "production Resolve with one substituted template clone"));
        }

        /// <summary>A transient, unregistered copy of a real ability with its
        /// own components, parent and identity fields.</summary>
        private static BlueprintAbility CloneForFixture(BlueprintAbility source,
            string guid, string name, List<UnityEngine.Object> created)
        {
            BlueprintAbility clone = ScriptableObject.CreateInstance<BlueprintAbility>();
            ExpandedSummoningAbilityBuilder.CopyFields(source, clone);
            AlternateSummoningAdapterBuilder.ResetComponentCaches(clone);
            clone.name = name;
            clone.ComponentsArray = (source.ComponentsArray ?? new BlueprintComponent[0])
                .Select(ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            typeof(BlueprintScriptableObject).GetField("m_AssetGuid",
                BindingFlags.Instance | BindingFlags.NonPublic).SetValue(clone, guid);
            created.Add(clone);
            return clone;
        }
    }
}
