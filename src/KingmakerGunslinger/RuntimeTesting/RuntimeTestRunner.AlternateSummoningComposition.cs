using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        /// <summary>
        /// Negative controls for the composed-action contract, run against
        /// the actual verifier on disposable clones of real built adapter
        /// children: a templated Master Summoner tier-I creature (two spawn
        /// branches) and a Naturalist tier-I creature. Every mutation is made
        /// on a clone whose composed actions are first proved to be its own.
        /// </summary>
        private static void AlternateSummoningCompositionControls(
            List<RuntimeTestAssertion> assertions, JObject document,
            Func<string, BlueprintScriptableObject> lookup,
            AlternateSummoningTacticianSources tactician, List<UnityEngine.Object> created)
        {
            var record = new JObject();
            document["composition"] = record;
            BlueprintAbility canonicalTemplated, canonicalAlly;
            BlueprintAbility templated = BuildCompositionSubject(lookup, tactician, created,
                AlternateSummoningSourceCatalog.MasterSummonerKey, choice =>
                    choice.Kind == AlternateSummoningChoiceKind.Creature && choice.Tier == 1 &&
                    choice.Multiplicity == SummonMultiplicity.One &&
                    choice.Placement.Creature.MonsterTemplated, out canonicalTemplated);
            BlueprintAbility ally = BuildCompositionSubject(lookup, tactician, created,
                AlternateSummoningSourceCatalog.NaturalistKey, choice =>
                    choice.Kind == AlternateSummoningChoiceKind.Creature && choice.Tier == 1 &&
                    choice.Multiplicity == SummonMultiplicity.One, out canonicalAlly);
            int templatedBranches = AlternateSummoningCompositionReader.Branches(templated).Count;
            record["templated"] = templated.name;
            record["templatedBranches"] = templatedBranches;
            record["ally"] = ally.name;

            string templatedPositive = AlternateSummoningCompositionReader.Verify(templated,
                canonicalTemplated);
            string allyPositive = AlternateSummoningCompositionReader.Verify(ally, canonicalAlly);
            BlueprintAbility untouched = CompositionClone(templated, "Untouched", created);
            string clonePositive = AlternateSummoningCompositionReader.Verify(untouched,
                canonicalTemplated);
            assertions.Add(Assertion("composition-positive-control",
                "unmodified built children and an unmodified clone keep the composed-action contract",
                "templated=" + (templatedPositive ?? "ok") + ";ally=" + (allyPositive ?? "ok") +
                    ";clone=" + (clonePositive ?? "ok") + ";branches=" + templatedBranches,
                templatedPositive == null && allyPositive == null && clonePositive == null &&
                    templatedBranches >= 2,
                "production builder; AlternateSummoningCompositionReader.Verify"));

            CompositionNegative(assertions, record, "alignment-mode", ally, canonicalAlly,
                created, clone =>
                {
                    foreach (ContextActionSetSummonAlignment action in AlternateSummoningGraph
                        .Objects<ContextActionSetSummonAlignment>(clone.ComponentsArray))
                        action.Mode = action.Mode == SummonAlignmentMode.Caster ?
                            SummonAlignmentMode.Celestial : SummonAlignmentMode.Caster;
                }, "branch0:alignment-mode:", true);
            CompositionNegative(assertions, record, "replaced-native-template-buff",
                templated, canonicalTemplated, created, clone =>
                {
                    foreach (ContextActionApplySummonTemplateByCaster action in
                        AlternateSummoningGraph.Objects<ContextActionApplySummonTemplateByCaster>(
                            clone.ComponentsArray))
                    {
                        BlueprintBuff[] replaced = action.ReplacedNativeTemplateBuffs ??
                            new BlueprintBuff[0];
                        // A new array: the built child's list is never mutated.
                        action.ReplacedNativeTemplateBuffs = replaced.Length > 0 ?
                            replaced.Skip(1).ToArray() :
                            new[] { action.FiendishTemplate };
                    }
                }, "branch0:replaced-native-template-buffs:", false);
            CompositionNegative(assertions, record, "template-reference", templated,
                canonicalTemplated, created, clone =>
                {
                    foreach (ContextActionApplySummonTemplateByCaster action in
                        AlternateSummoningGraph.Objects<ContextActionApplySummonTemplateByCaster>(
                            clone.ComponentsArray))
                        action.CelestialTemplate = action.FiendishTemplate;
                }, "branch0:template-buffs:celestial-template:", false);
            CompositionNegative(assertions, record, "placement", templated, canonicalTemplated,
                created, clone =>
                {
                    ContextActionSpawnMonster last = AlternateSummoningGraph
                        .Objects<ContextActionSpawnMonster>(clone.ComponentsArray).Last();
                    last.AfterSpawn.Actions = last.AfterSpawn.Actions.Where(value =>
                        !(value is ContextActionApplySummonTemplateByCaster)).ToArray();
                }, "placement:branch" + (templatedBranches - 1) + ":", false);

            AlternateSummoningTemplateRemovalControls(assertions, record, lookup, tactician,
                created);
            InheritedTemplateRemovalSetIsDerived(assertions, record, lookup, tactician, created);
        }

        /// <summary>
        /// S5 T1 / DEF-05: the full registered inherited-template census, one
        /// observation per published ability:
        /// - every canonical placement of both families (a templated Summon
        ///   Monster placement contributes its caster-chosen, celestial and
        ///   fiendish abilities), with every spawn branch paired to its donor
        ///   branch by after-spawn shape;
        /// - every child of every family in AlternateSummoningSourceCatalog.All
        ///   built through the production adapter; a family that cannot be
        ///   resolved or built is reported, and its expected children stay
        ///   expected (so they are reported as skipped, never silently dropped).
        /// Expected keys come from the catalog and the projection, actual
        /// children from the builder. Each recorded donor is checked against the
        /// independent InheritedTemplateDonorContract and against the receipt
        /// taken before it was cloned. Graph evidence only.
        /// </summary>
        private static void InheritedTemplateRemovalSetIsDerived(
            List<RuntimeTestAssertion> assertions, JObject record,
            Func<string, BlueprintScriptableObject> lookup,
            AlternateSummoningTacticianSources tactician, List<UnityEngine.Object> created)
        {
            var nativeIds = new HashSet<string>(
                ExpandedSummoningAbilityBuilder.NativeTemplateIds, StringComparer.Ordinal);
            var observations = new List<InheritedTemplateObservation>();
            var expected = new List<string>();
            var rows = new JArray();
            var unresolved = new SortedSet<string>(StringComparer.Ordinal);
            var families = new JObject();
            var donors = new JObject();
            var childFindings = new List<string>();

            foreach (SummonFamily family in new[] { SummonFamily.Monster, SummonFamily.NaturesAlly })
            foreach (SummonVariantSpec variant in ExpandedSummoningCatalog.GenerateVariants(family))
            {
                string symbol = ExpandedSummoningIdentityCatalog.AbilitySymbol(variant);
                bool templated = family == SummonFamily.Monster && variant.Creature.MonsterTemplated;
                ExpandedSummoningAbilityBuilder.DonorRecord donor = CensusDonor(variant, unresolved, donors);
                BlueprintUnit unit = CensusUnit(variant, unresolved);
                foreach (string published in templated ?
                    new[] { symbol, symbol + ".Celestial", symbol + ".Fiendish" } : new[] { symbol })
                {
                    expected.Add("canonical|" + published);
                    BlueprintScriptableObject registered;
                    if (!ExpandedSummoningBlueprints.Registered.BySymbol.TryGetValue(published, out registered) ||
                        !(registered is BlueprintAbility) || unit == null || donor == null)
                    { unresolved.Add(published); continue; }
                    observations.Add(CensusObservation(published, "canonical", variant, templated, donor, true,
                        (BlueprintAbility)registered, unit, nativeIds, rows));
                }
            }

            foreach (AlternateSummoningFamilySpec family in AlternateSummoningSourceCatalog.All)
            {
                var summary = new JObject { ["spellFamily"] = family.Family.ToString() };
                families[family.Key] = summary;
                AlternateSummoningFamilyIdentity identity = AlternateSummoningSourceCatalog
                    .Identities.SingleOrDefault(value => value.Key == family.Key);
                if (identity == null) { unresolved.Add("family-identity:" + family.Key); summary["status"] = "no-identity"; continue; }
                // Expected keys from the projection, independently of what the builder returns.
                AlternateSummoningChoice[] choices = AlternateSummoningProjection
                    .Identities(identity, AlternateSummoningRoster.Production())
                    .Where(choice => choice.Kind == AlternateSummoningChoiceKind.Creature).ToArray();
                foreach (AlternateSummoningChoice choice in choices)
                    expected.Add("alternate:" + family.Key + "|" + choice.Symbol);
                summary["expected"] = choices.Length;
                AlternateSummoningBuiltFamily built;
                try
                {
                    AlternateSummoningResolvedFamily resolved = AlternateSummoningHostResolver.Resolve(
                        lookup, family, identity, tactician, rootAbility =>
                            AlternateSummoningCoordinator.OriginalHostChildren(family.Key, rootAbility.AssetGuid));
                    built = AlternateSummoningAdapterBuilder.Build(lookup, symbol =>
                        {
                            BlueprintAbility shell = AlternateSummoningAdapterBuilder.CreateShell(symbol);
                            created.Add(shell);
                            return shell;
                        }, resolved, AlternateSummoningRoster.Production(),
                        ExpandedSummoningBlueprints.Registered);
                }
                catch (Exception exception)
                {
                    unresolved.Add("family-failed:" + family.Key + ":" + exception.GetType().Name + ":" + exception.Message);
                    summary["status"] = "failed:" + exception.Message;
                    summary["observed"] = 0;
                    continue;
                }
                // Review correction 5: the actual creature children are classified from the builder's
                // own output (not the projection): a built record that is not a preserved root copy and
                // spawns a registered Expanded Summoning unit. Native options and preserved roots are
                // excluded by that classification. Expected and actual sets are compared both ways.
                var choiceBySymbol = choices.ToDictionary(choice => choice.Symbol, StringComparer.Ordinal);
                var projectUnits = new HashSet<string>(ExpandedSummoningBlueprints.Registered.BySymbol
                    .Where(entry => entry.Key.StartsWith("KMG.Summoning.Unit.", StringComparison.Ordinal) && entry.Value != null)
                    .Select(entry => entry.Value.AssetGuid), StringComparer.Ordinal);
                var preserved = new HashSet<BlueprintAbility>(built.Preserved.Values);
                var expectedChildren = choices.ToDictionary(choice => choice.Symbol, choice => choice.Placement == null ?
                    Tuple.Create((string)null, (SummonFamily?)null) :
                    Tuple.Create(choice.Placement.StableKey, (SummonFamily?)choice.Placement.Family), StringComparer.Ordinal);
                var actualChildren = new Dictionary<string, Tuple<string, SummonFamily?>>(StringComparer.Ordinal);
                int nonCreature = 0;
                foreach (KeyValuePair<string, BlueprintAbility> entry in built.BySymbol)
                {
                    SummonSpawn[] spawns = SummonSignatureReader.Read(entry.Value).Spawns().ToArray();
                    if (preserved.Contains(entry.Value) || spawns.Length == 0 || !spawns.All(spawn => projectUnits.Contains(spawn.Unit)))
                    { nonCreature++; continue; }
                    AlternateSummoningChoice choice;
                    choiceBySymbol.TryGetValue(entry.Key, out choice);
                    string expectedUnit = null;
                    if (choice != null && choice.Placement != null)
                    {
                        BlueprintScriptableObject unitBlueprint;
                        if (ExpandedSummoningBlueprints.Registered.BySymbol.TryGetValue(
                                ExpandedSummoningIdentityCatalog.UnitSymbol(choice.Placement.Creature), out unitBlueprint) && unitBlueprint != null)
                            expectedUnit = unitBlueprint.AssetGuid;
                    }
                    // The placement association holds only when the built child spawns that placement's own unit.
                    bool associated = expectedUnit != null && spawns.All(spawn => spawn.Unit == expectedUnit);
                    actualChildren[entry.Key] = associated ?
                        Tuple.Create(choice.Placement.StableKey, (SummonFamily?)choice.Placement.Family) :
                        Tuple.Create((string)null, (SummonFamily?)null);
                }
                IList<string> childSet = InheritedChildSetContract.Compare(family.Key, family.Family, expectedChildren, actualChildren);
                foreach (string finding in childSet) childFindings.Add(finding);
                summary["actualCreatureChildren"] = actualChildren.Count;
                summary["nonCreatureRecords"] = nonCreature;
                summary["childSetFindings"] = childSet.Count;
                int observed = 0;
                foreach (string symbol in actualChildren.Keys.OrderBy(value => value, StringComparer.Ordinal))
                {
                    BlueprintAbility child = built.BySymbol[symbol];
                    AlternateSummoningChoice choice;
                    choiceBySymbol.TryGetValue(symbol, out choice);
                    SummonVariantSpec placement = choice == null ? null : choice.Placement;
                    if (actualChildren[symbol].Item1 == null || placement == null || placement.Family != family.Family)
                    {
                        // Uncensusable without a placement: recorded, and the child-set finding fails the census.
                        rows.Add(new JObject { ["key"] = symbol, ["caller"] = "alternate:" + family.Key, ["placement"] = null,
                            ["expected"] = "unclassifiable" });
                        continue;
                    }
                    BlueprintUnit unit = CensusUnit(placement, unresolved);
                    ExpandedSummoningAbilityBuilder.DonorRecord donor = CensusDonor(placement, unresolved, donors);
                    if (unit == null || donor == null) continue;
                    observations.Add(CensusObservation(symbol, "alternate:" + family.Key, placement,
                        placement.Family == SummonFamily.Monster && placement.Creature.MonsterTemplated,
                        donor, false, child, unit, nativeIds, rows));
                    observed++;
                }
                summary["status"] = "built";
                summary["observed"] = observed;
            }

            InheritedTemplateCensusResult result = InheritedTemplateCensus.Evaluate(observations, expected, nativeIds, true);
            // Donor validation once per recorded carrier (owner decision: one approved tier-one publication transition).
            var donorFindings = new List<string>();
            var transitions = new JArray();
            var donorCounts = new Dictionary<string, int>(StringComparer.Ordinal) {
                { "unchanged", 0 }, { "approved-tier-one-publication", 0 }, { "mutated", 0 } };
            foreach (ExpandedSummoningAbilityBuilder.DonorRecord carrier in ExpandedSummoningAbilityBuilder.RecordedCarriers()
                .OrderBy(value => value.Ability.AssetGuid, StringComparer.Ordinal))
            {
                InheritedTierOneObservation donorView = CensusDonorView(carrier);
                var failures = new List<string>();
                string disposition = InheritedTierOnePublicationContract.Judge(donorView, failures);
                donorCounts[disposition] = donorCounts[disposition] + 1;
                foreach (string failure in failures) donorFindings.Add("donor|" + carrier.Ability.AssetGuid + ":" + failure);
                if (disposition == "approved-tier-one-publication")
                    transitions.Add(new JObject {
                        ["parent"] = carrier.Ability.AssetGuid + ":" + carrier.Ability.name,
                        ["receiptDifferences"] = new JArray(donorView.ReceiptDifferences),
                        ["originalComponents"] = donorView.OriginalComponents.Count,
                        ["currentComponents"] = new JArray(donorView.CurrentComponents.Select(view => view.Type + ":" + view.Name)),
                        ["differencesExcludingMenu"] = new JArray(donorView.DifferencesExcludingMenu),
                        ["preservedChild"] = donorView.PreservedGuid,
                        ["menuChildren"] = donorView.MenuChildren.Count,
                        ["expectedOwnedChildren"] = donorView.ExpectedOwnedChildren.Count,
                        ["foreignMenuRows"] = donorView.MenuChildren.Count(child => child != donorView.PreservedGuid &&
                            !donorView.ExpectedOwnedChildren.Contains(child)),
                        ["preservedSignatureEqual"] = donorView.PreservedSignature == donorView.DonorSignature,
                        ["preservedActionTypes"] = donorView.PreservedActionTypes.Count,
                        ["preservedApplyBuffIds"] = new JArray(donorView.PreservedApplyBuffIds),
                        ["preservedSharesInstances"] = donorView.PreservedSharesInstances,
                        ["publicationRecord"] = donorView.RecordDisagreement ?? "consistent" });
            }
            var dispositions = new JObject(result.Dispositions.OrderBy(entry => entry.Key)
                .Select(entry => new JProperty(entry.Key.ToString(), entry.Value)));
            int canonicalBranches = observations.Where(value => !value.IsAlternate).Sum(value => value.Branches.Length);
            int alternateBranches = observations.Where(value => value.IsAlternate).Sum(value => value.Branches.Length);
            int multiBranch = observations.Count(value => value.Branches.Length > 1);
            var bySpellFamily = new JObject(observations.GroupBy(value => (value.IsAlternate ? "alternate." : "canonical.") +
                value.Family).OrderBy(group => group.Key).Select(group => new JProperty(group.Key, group.Count())));
            int quantityRemovals = observations.Count(value => !value.IsAlternate &&
                value.Multiplicity != SummonMultiplicity.One &&
                InheritedTemplateCensus.Expected(value, nativeIds) == InheritedTemplateDisposition.RemoveInherited);
            int deliberate = observations.Count(value => value.DeliberateTemplateAbility);
            bool everyFamily = AlternateSummoningSourceCatalog.All.All(family =>
                families[family.Key] != null && (string)families[family.Key]["status"] == "built" &&
                (int)families[family.Key]["observed"] == (int)families[family.Key]["expected"] &&
                (int)families[family.Key]["actualCreatureChildren"] == (int)families[family.Key]["expected"] &&
                (int)families[family.Key]["expected"] > 0);
            record["inheritedTemplateCensus"] = rows;
            record["inheritedTemplateDonors"] = donors;
            record["inheritedTemplateCensusSummary"] = new JObject {
                ["observations"] = result.Observations,
                ["expected"] = expected.Count,
                ["canonical"] = observations.Count(value => !value.IsAlternate),
                ["alternate"] = observations.Count(value => value.IsAlternate),
                ["bySpellFamily"] = bySpellFamily,
                ["alternateFamilies"] = families,
                ["canonicalBranches"] = canonicalBranches,
                ["alternateBranches"] = alternateBranches,
                ["multiBranchAbilities"] = multiBranch,
                ["dispositions"] = dispositions,
                ["quantityRemovals"] = quantityRemovals,
                ["deliberateTemplateAbilities"] = deliberate,
                ["notRetainedDonorShapes"] = new JArray(result.NotRetainedShapes),
                ["findings"] = new JArray(result.Findings),
                ["childSetFindings"] = new JArray(childFindings),
                ["donorValidation"] = new JObject { ["carriers"] = donorCounts.Values.Sum(),
                    ["unchanged"] = donorCounts["unchanged"], ["approvedTierOnePublication"] = donorCounts["approved-tier-one-publication"],
                    ["mutated"] = donorCounts["mutated"], ["findings"] = new JArray(donorFindings), ["approvedTransitions"] = transitions },
                ["justifiedRedundant"] = new JArray(result.JustifiedRedundant),
                ["unresolved"] = new JArray(unresolved) };
            assertions.Add(Assertion("inherited-template-census-per-placement",
                "every published Summon Monster and Nature's Ally ability, canonical and every child of all seven " +
                    "alternate families (the builder's actual creature children equal the projection's, both ways, " +
                    "with no missing, extra, duplicate, null or wrong-family placement), " +
                    "has the inherited-template disposition its placement requires: each recorded " +
                    "donor matches the independent donor contract and its pre-clone receipt; every canonical spawn " +
                    "branch is paired once with its donor branch by shape; an untemplated Summon Monster branch " +
                    "whose donor applies removes exactly the native identities after the donor's applications, " +
                    "bound to its own unit; no other placement removes or keeps an unremoved native application; " +
                    "deliberately templated abilities keep their project template; nothing expected is skipped; " +
                    "every removal with nothing to remove is listed with its reason",
                "observations=" + result.Observations + "/" + expected.Count + ";bySpellFamily=" +
                    bySpellFamily.ToString(Newtonsoft.Json.Formatting.None) + ";families=" +
                    string.Join(",", families.Properties().Select(property => property.Name + ":" +
                        property.Value["status"] + ":" + property.Value["observed"] + "/" + property.Value["expected"]).ToArray()) +
                    ";branches=" + canonicalBranches + "+" + alternateBranches + ";multiBranch=" + multiBranch +
                    ";dispositions=" + dispositions.ToString(Newtonsoft.Json.Formatting.None) + ";quantityRemovals=" +
                    quantityRemovals + ";deliberateTemplateAbilities=" + deliberate + ";justifiedRedundant=" +
                    result.JustifiedRedundant.Count + ";notRetainedShapes=" + result.NotRetainedShapes.Count +
                    ";childSetFindings=[" + string.Join(",", childFindings.Take(20).ToArray()) + "]" +
                    ";donors=" + donorCounts.Values.Sum() + ":unchanged=" + donorCounts["unchanged"] + ",approvedTierOne=" +
                    donorCounts["approved-tier-one-publication"] + ",mutated=" + donorCounts["mutated"] +
                    ";donorFindings=[" + string.Join(",", donorFindings.Take(20).ToArray()) + "]" +
                    ";findings=[" + string.Join(",", result.Findings.Take(40).ToArray()) +
                    "];unresolved=[" + string.Join(",", unresolved.Take(20).ToArray()) + "]",
                result.Passed && childFindings.Count == 0 && donorFindings.Count == 0 && donorCounts["mutated"] == 0 &&
                    donorCounts["approved-tier-one-publication"] == 2 &&
                    unresolved.Count == 0 && result.Observations == expected.Count && everyFamily &&
                    quantityRemovals > 0 && deliberate > 0,
                "registered canonical abilities and production-built children of AlternateSummoningSourceCatalog.All; " +
                    "recorded donors with receipts; InheritedTemplateDonorContract; InheritedTemplateCensus. Graph evidence only"));
        }

        /// <summary>
        /// Everything the tier-one publication judge needs about one recorded donor carrier, read from the
        /// pre-clone receipt, the live parent, the stable preserved child and, independently, the publisher's record.
        /// </summary>
        private static InheritedTierOneObservation CensusDonorView(ExpandedSummoningAbilityBuilder.DonorRecord carrier)
        {
            BlueprintAbility parent = carrier.Ability;
            BlueprintComponent[] current = parent.ComponentsArray ?? new BlueprintComponent[0];
            var view = new InheritedTierOneObservation {
                ParentGuid = parent.AssetGuid,
                ReceiptDifferences = carrier.Differences(),
                OriginalComponents = carrier.Receipt.Components.ToList(),
                CurrentComponents = current.Select(component => new InheritedComponentView(component,
                    component == null ? "<null>" : component.GetType().FullName, component == null ? "" : component.name)).ToList() };
            if (view.ReceiptDifferences.Count == 0) return view;
            bool monster = parent.AssetGuid == InheritedTierOnePublicationContract.SummonMonsterI;
            if (!monster && parent.AssetGuid != InheritedTierOnePublicationContract.SummonNaturesAllyI) return view;
            BlueprintComponent[] added = current.Where(component => !carrier.Receipt.Components.Any(original =>
                ReferenceEquals(original, component))).ToArray();
            BlueprintComponent[] withoutAdded = current.Where(component => !added.Contains(component)).ToArray();
            AbilityVariants menu = added.OfType<AbilityVariants>().FirstOrDefault();
            view.MenuChildren = (menu == null || menu.Variants == null ? new BlueprintAbility[0] : menu.Variants)
                .Select(child => child == null ? "<null>" : child.AssetGuid).ToList();
            // The live parent with the one added component excluded, against the receipt. The components-array
            // reference is the receipt's own: the array is replaced by design; its contents are what is compared.
            view.DifferencesExcludingMenu = InheritedTemplateDonorReceipt.Differences(carrier.Receipt,
                ExpandedSummoningAbilityBuilder.CaptureReceipt(parent.AssetGuid, parent.name, carrier.Receipt.ComponentsArray, withoutAdded));
            BlueprintScriptableObject preservedObject;
            ExpandedSummoningBlueprints.Registered.BySymbol.TryGetValue(monster ?
                ExpandedSummoningIdentityCatalog.NativeMonsterTierOneSymbol :
                ExpandedSummoningIdentityCatalog.NativeNaturesAllyTierOneSymbol, out preservedObject);
            var preserved = preservedObject as BlueprintAbility;
            view.PreservedIsAbility = preserved != null;
            view.PreservedGuid = preservedObject == null ? null : preservedObject.AssetGuid;
            view.DonorSignature = carrier.PrePublicationSignature;
            view.DonorActionTypes = carrier.Receipt.ActionTypes.ToList();
            view.DonorApplyBuffIds = carrier.Receipt.ApplyBuffIds.ToList();
            if (preserved != null)
            {
                BlueprintComponent[] preservedComponents = preserved.ComponentsArray ?? new BlueprintComponent[0];
                view.PreservedSignature = SummonSignatureReader.Read(preserved).Render();
                GameAction[] preservedActions = AlternateSummoningGraph.Objects<GameAction>(preservedComponents).ToArray();
                view.PreservedActionTypes = preservedActions.Select(action => action.GetType().FullName).ToList();
                view.PreservedApplyBuffIds = AlternateSummoningGraph.Objects<ContextActionApplyBuff>(preservedComponents)
                    .Select(action => action.Buff == null ? "<null>" : action.Buff.AssetGuid).ToList();
                view.PreservedSharesInstances = preservedComponents.Any(component => carrier.Receipt.Components.Any(original =>
                        ReferenceEquals(original, component))) ||
                    preservedActions.Any(action => carrier.Receipt.Actions.Any(original => ReferenceEquals(original, action)));
            }
            SummonFamily family = monster ? SummonFamily.Monster : SummonFamily.NaturesAlly;
            var owned = new List<string>();
            foreach (SummonVariantSpec variant in ExpandedSummoningCatalog.GenerateVariants(family)
                .Where(value => value.ParentTier == 1 && SummonVisibilityCatalog.IsPublished(value)))
            {
                BlueprintScriptableObject child;
                owned.Add(ExpandedSummoningBlueprints.Registered.BySymbol.TryGetValue(
                    ExpandedSummoningIdentityCatalog.AbilitySymbol(variant), out child) && child != null ? child.AssetGuid : "<unresolved>");
            }
            foreach (SummonNativeExpansionSpec spec in SummonNativeExpansionCatalog.For(family, 1))
            {
                BlueprintScriptableObject child;
                owned.Add(ExpandedSummoningBlueprints.Registered.BySymbol.TryGetValue(spec.Symbol, out child) && child != null
                    ? child.AssetGuid : "<unresolved>");
            }
            view.ExpectedOwnedChildren = owned;
            // The publisher's own record, checked against the receipt and the live parent, never trusted alone.
            ExpandedSummoningPublication publication = ExpandedSummoningPublisher.Last;
            ExpandedSummoningPublication.Record record = publication == null ? null :
                publication.Records.SingleOrDefault(value => ReferenceEquals(value.Parent, parent));
            if (record == null) view.RecordDisagreement = "no-record";
            else if (!(record.OriginalComponents ?? new BlueprintComponent[0]).Cast<object>().SequenceEqual(carrier.Receipt.Components))
                view.RecordDisagreement = "original-components-differ-from-receipt";
            else if (!ReferenceEquals(record.PublishedComponents, parent.ComponentsArray))
                view.RecordDisagreement = "published-components-not-live";
            else if (menu == null || !ReferenceEquals(record.PublishedVariants, menu))
                view.RecordDisagreement = "published-variants-not-the-added-component";
            return view;
        }

        private static ExpandedSummoningAbilityBuilder.DonorRecord CensusDonor(SummonVariantSpec variant,
            ISet<string> unresolved, JObject donors)
        {
            ExpandedSummoningAbilityBuilder.DonorRecord record;
            try { record = ExpandedSummoningAbilityBuilder.RecordedDonor(variant); }
            catch (InvalidOperationException) { unresolved.Add("donor:" + variant.StableKey); return null; }
            string shape = variant.Family + "." + variant.ParentTier + "." + variant.Multiplicity;
            if (donors[shape] == null)
                donors[shape] = new JObject { ["guid"] = record.Ability.AssetGuid, ["name"] = record.Ability.name };
            return record;
        }

        private static BlueprintUnit CensusUnit(SummonVariantSpec variant, ISet<string> unresolved)
        {
            BlueprintScriptableObject unit;
            if (ExpandedSummoningBlueprints.Registered.BySymbol.TryGetValue(
                    ExpandedSummoningIdentityCatalog.UnitSymbol(variant.Creature), out unit) && unit is BlueprintUnit)
                return (BlueprintUnit)unit;
            unresolved.Add("unit:" + variant.Creature.Key);
            return null;
        }

        private static bool CensusAppliesNative(GameAction action, ICollection<string> nativeIds)
        {
            return action != null && AlternateSummoningGraph.Objects<ContextActionApplyBuff>(action).Any(apply =>
                apply.Buff != null && nativeIds.Contains(apply.Buff.AssetGuid));
        }

        private static GameAction[] CensusAfterSpawn(ContextActionSpawnMonster spawn)
        {
            return spawn.AfterSpawn == null || spawn.AfterSpawn.Actions == null ?
                new GameAction[0] : spawn.AfterSpawn.Actions;
        }

        private static InheritedTemplateObservation CensusObservation(string key, string caller,
            SummonVariantSpec variant, bool templated, ExpandedSummoningAbilityBuilder.DonorRecord donor, bool clone,
            BlueprintAbility ability, BlueprintUnit unit, ICollection<string> nativeIds, JArray rows)
        {
            ContextActionSpawnMonster[] donorSpawns = AlternateSummoningGraph
                .Objects<ContextActionSpawnMonster>(donor.Ability.ComponentsArray).ToArray();
            string[][] donorShapes = donorSpawns.Select(spawn => CensusAfterSpawn(spawn)
                .Select(action => action == null ? "<null>" : action.GetType().FullName).ToArray()).ToArray();
            bool[] donorApplies = donorSpawns.Select(spawn => CensusAfterSpawn(spawn)
                .Any(action => CensusAppliesNative(action, nativeIds))).ToArray();
            string[] donorApplied = AlternateSummoningGraph.Objects<ContextActionApplyBuff>(donor.Ability.ComponentsArray)
                .Where(action => action.Buff != null && nativeIds.Contains(action.Buff.AssetGuid))
                .Select(action => action.Buff.AssetGuid).Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            var branches = new List<InheritedTemplateBranch>();
            var branchRows = new JArray();
            ContextActionSpawnMonster[] spawns = AlternateSummoningGraph
                .Objects<ContextActionSpawnMonster>(ability.ComponentsArray).ToArray();
            for (int ordinal = 0; ordinal < spawns.Length; ordinal++)
            {
                GameAction[] actions = CensusAfterSpawn(spawns[ordinal]);
                string[] types = actions.Select(action => action == null ? "<null>" : action.GetType().FullName).ToArray();
                // Pair by after-spawn shape: the donor branch whose action types prefix this branch's,
                // preferring the same traversal position.
                Func<int, bool> prefix = candidate => donorShapes[candidate].Length <= types.Length &&
                    donorShapes[candidate].SequenceEqual(types.Take(donorShapes[candidate].Length), StringComparer.Ordinal);
                int paired = !clone ? -1 : ordinal < donorShapes.Length && prefix(ordinal) ? ordinal :
                    Enumerable.Range(0, donorShapes.Length).Where(prefix).DefaultIfEmpty(-1).First();
                int lastApply = -1, deliberateIndex = -1;
                var removals = new List<int>();
                bool bound = true, exact = true;
                for (int index = 0; index < actions.Length; index++)
                {
                    GameAction action = actions[index];
                    if (action == null) continue;
                    if (CensusAppliesNative(action, nativeIds)) lastApply = index;
                    var removal = action as ContextActionRemoveInheritedSummonTemplates;
                    if (removal != null)
                    {
                        removals.Add(index);
                        bound &= ReferenceEquals(removal.OwningBlueprint, unit);
                        string[] ids = (removal.NativeTemplates ?? new BlueprintBuff[0])
                            .Select(buff => buff == null ? null : buff.AssetGuid).ToArray();
                        exact &= ids.Length == nativeIds.Count && ids.All(id => id != null && nativeIds.Contains(id)) &&
                            ids.Distinct(StringComparer.Ordinal).Count() == ids.Length;
                    }
                    var summonBuff = action as ContextActionApplySummonBuff;
                    var byCaster = action as ContextActionApplySummonTemplateByCaster;
                    BlueprintBuff[] replaced = summonBuff != null ? summonBuff.ReplacedNativeTemplateBuffs :
                        byCaster != null ? byCaster.ReplacedNativeTemplateBuffs : null;
                    if (replaced != null && replaced.Length == nativeIds.Count &&
                        replaced.All(buff => buff != null && nativeIds.Contains(buff.AssetGuid)))
                        deliberateIndex = index;
                }
                bool deliberate = deliberateIndex >= 0 && deliberateIndex > lastApply;
                branches.Add(new InheritedTemplateBranch(lastApply, removals.ToArray(), bound, exact, deliberate,
                    paired, paired >= 0));
                branchRows.Add(new JObject { ["actions"] = actions.Length, ["donorBranch"] = paired,
                    ["lastNativeApply"] = lastApply, ["removals"] = new JArray(removals), ["bound"] = bound,
                    ["exact"] = exact, ["deliberateTemplateAfterDonor"] = deliberate });
            }
            var observation = new InheritedTemplateObservation(key, caller, variant.Family, templated,
                templated, donorApplied, branches.ToArray(), variant.ParentTier, variant.Multiplicity,
                donor.Ability.AssetGuid, donor.Ability.name, clone ? donorApplies : null,
                null);
            rows.Add(new JObject {
                ["key"] = key, ["caller"] = caller, ["family"] = variant.Family.ToString(),
                ["creature"] = variant.Creature.Key, ["placement"] = variant.StableKey,
                ["parentTier"] = variant.ParentTier, ["multiplicity"] = variant.Multiplicity.ToString(),
                ["templated"] = templated, ["donor"] = donor.Ability.AssetGuid,
                ["donorApplied"] = new JArray(donorApplied), ["donorBranchApplies"] = new JArray(donorApplies),
                ["expected"] = InheritedTemplateCensus.Expected(observation, nativeIds).ToString(),
                ["branches"] = branchRows });
            return observation;
        }

        /// <summary>
        /// GS-AUD-0025. The controls above run on a templated creature and on a
        /// Nature's Ally creature, so NONE of them enters the TemplateRemoval
        /// comparison branch - which is the branch the inherited-template
        /// removal, and the multiset identity repair, actually live in. A
        /// passing composition observation over those two subjects therefore
        /// says nothing about the removal. These controls use an AFFECTED
        /// subject, meaning an untemplated Summon Monster creature that
        /// RequiresInheritedTemplateRemoval declares - which is a property of
        /// the declared set, not a measured claim about its native carrier. See
        /// GS-REP-0002: the declared set and the installed library disagree.
        ///
        /// Still graph/reader evidence only. It does not cast, so it does not
        /// discharge the execution half of the acceptance recorded on
        /// ExpandedSummoningAbilityBuilder.InheritedTemplateRemovalCreatures.
        /// </summary>
        private static void AlternateSummoningTemplateRemovalControls(
            List<RuntimeTestAssertion> assertions, JObject record,
            Func<string, BlueprintScriptableObject> lookup,
            AlternateSummoningTacticianSources tactician, List<UnityEngine.Object> created)
        {
            BlueprintAbility canonicalAffected;
            AlternateSummoningChoice affectedChoice = null;
            BlueprintAbility affected = BuildCompositionSubject(lookup, tactician, created,
                AlternateSummoningSourceCatalog.MasterSummonerKey, choice =>
                {
                    bool match = choice.Kind == AlternateSummoningChoiceKind.Creature &&
                        choice.Multiplicity == SummonMultiplicity.One &&
                        ExpandedSummoningAbilityBuilder.RequiresInheritedTemplateRemoval(
                            choice.Placement, ExpandedSummoningAbilityBuilder.RecordedDonor(
                                choice.Placement).Ability);
                    if (match) affectedChoice = choice;
                    return match;
                }, out canonicalAffected);

            ComposedActionView[] removals = AlternateSummoningCompositionReader
                .Branches(affected).SelectMany(branch => branch)
                .Where(value => value.Kind == ComposedActionKind.TemplateRemoval).ToArray();
            string affectedKey = affectedChoice == null ? "<none>" :
                affectedChoice.Placement.Creature.Key;
            record["affectedSubject"] = affected.name;
            record["affectedCreature"] = affectedKey;
            record["affectedTier"] = affectedChoice == null ? -1 : affectedChoice.Tier;
            record["affectedRemovalActions"] = removals.Length;

            string affectedPositive = AlternateSummoningCompositionReader.Verify(affected,
                canonicalAffected);
            assertions.Add(Assertion("composition-affected-subject-positive-control",
                "an untemplated template-inheriting child carries at least one removal action " +
                    "bound to the four native identities, and matches its canonical child",
                "creature=" + affectedKey + ";tier=" +
                    (affectedChoice == null ? -1 : affectedChoice.Tier) + ";removalActions=" +
                    removals.Length + ";identities=" + (removals.Length == 0 ? "<none>" :
                        removals[0].ReplacedNativeTemplateBuffs.Length.ToString()) +
                    ";verify=" + (affectedPositive ?? "ok"),
                affectedPositive == null && removals.Length >= 1 && removals.All(value =>
                    value.ReplacedNativeTemplateBuffs != null &&
                    value.ReplacedNativeTemplateBuffs.Length ==
                        ExpandedSummoningAbilityBuilder.NativeTemplateIds.Count),
                "production builder on an affected creature; AlternateSummoningCompositionReader.Verify"));

            // THE multiset regression, demonstrated by execution rather than
            // argued: a duplicated native identity. The run-time action
            // revalidates that NativeTemplates holds exactly the four
            // identities and throws otherwise, so this composition would fail
            // to execute - and the superseded set comparison called [a,b,c,d,a]
            // equal to [a,b,c,d] and accepted it.
            CompositionNegative(assertions, record, "template-removal-duplicate-identity",
                affected, canonicalAffected, created, clone =>
                {
                    foreach (ContextActionRemoveInheritedSummonTemplates action in
                        AlternateSummoningGraph
                            .Objects<ContextActionRemoveInheritedSummonTemplates>(
                                clone.ComponentsArray))
                        action.NativeTemplates = action.NativeTemplates
                            .Concat(new[] { action.NativeTemplates[0] }).ToArray();
                }, "branch0:template-removal-native-buffs:", false);

            CompositionNegative(assertions, record, "template-removal-missing-identity",
                affected, canonicalAffected, created, clone =>
                {
                    foreach (ContextActionRemoveInheritedSummonTemplates action in
                        AlternateSummoningGraph
                            .Objects<ContextActionRemoveInheritedSummonTemplates>(
                                clone.ComponentsArray))
                        action.NativeTemplates = action.NativeTemplates.Skip(1).ToArray();
                }, "branch0:template-removal-native-buffs:", false);

            CompositionNegative(assertions, record, "template-removal-owner",
                affected, canonicalAffected, created, clone =>
                {
                    foreach (ContextActionRemoveInheritedSummonTemplates action in
                        AlternateSummoningGraph
                            .Objects<ContextActionRemoveInheritedSummonTemplates>(
                                clone.ComponentsArray))
                        action.OwningBlueprint = null;
                }, "branch0:template-removal-owner:", false);

            CompositionNegative(assertions, record, "template-removal-absent",
                affected, canonicalAffected, created, clone =>
                {
                    foreach (ContextActionSpawnMonster spawn in AlternateSummoningGraph
                        .Objects<ContextActionSpawnMonster>(clone.ComponentsArray))
                        spawn.AfterSpawn.Actions = spawn.AfterSpawn.Actions.Where(value =>
                            !(value is ContextActionRemoveInheritedSummonTemplates)).ToArray();
                }, "placement:branch0:", false);
        }

        private static BlueprintAbility BuildCompositionSubject(
            Func<string, BlueprintScriptableObject> lookup,
            AlternateSummoningTacticianSources tactician, List<UnityEngine.Object> created,
            string familyKey, Func<AlternateSummoningChoice, bool> predicate,
            out BlueprintAbility canonical)
        {
            AlternateSummoningFamilySpec family = AlternateSummoningSourceCatalog.Find(familyKey);
            AlternateSummoningFamilyIdentity identity = AlternateSummoningSourceCatalog
                .Identities.Single(value => value.Key == family.Key);
            AlternateSummoningResolvedFamily resolved = AlternateSummoningHostResolver.Resolve(
                lookup, family, identity, tactician, root =>
                    AlternateSummoningCoordinator.OriginalHostChildren(family.Key, root.AssetGuid));
            AlternateSummoningChoice chosen = null;
            AlternateSummoningBuiltFamily built = AlternateSummoningAdapterBuilder.Build(
                lookup, symbol =>
                {
                    BlueprintAbility shell = AlternateSummoningAdapterBuilder.CreateShell(symbol);
                    created.Add(shell);
                    return shell;
                }, resolved, AlternateSummoningRoster.Production(),
                ExpandedSummoningBlueprints.Registered, choice =>
                {
                    if (chosen != null || !predicate(choice)) return false;
                    chosen = choice;
                    return true;
                });
            if (chosen == null || !built.BySymbol.ContainsKey(chosen.Symbol))
                throw new InvalidOperationException("No " + familyKey +
                    " composition subject was built.");
            canonical = (BlueprintAbility)ExpandedSummoningBlueprints.Registered
                .BySymbol[chosen.CanonicalSymbol];
            return built.BySymbol[chosen.Symbol];
        }

        private static BlueprintAbility CompositionClone(BlueprintAbility source, string tag,
            List<UnityEngine.Object> created)
        {
            BlueprintAbility clone = CloneForFixture(source, "f1c7a5e000004000800000000000c0" +
                (created.Count % 100).ToString("00"), source.name + "_Fixture" + tag, created);
            GameAction[] mine = AlternateSummoningGraph.Objects<GameAction>(clone.ComponentsArray)
                .ToArray();
            GameAction[] theirs = AlternateSummoningGraph.Objects<GameAction>(
                source.ComponentsArray).ToArray();
            if (mine.Any(action => theirs.Any(other => ReferenceEquals(action, other))))
                throw new InvalidOperationException("A fixture clone shares an action with " +
                    source.name + "; no mutation is made.");
            return clone;
        }

        private static void CompositionNegative(List<RuntimeTestAssertion> assertions,
            JObject record, string name, BlueprintAbility subject, BlueprintAbility canonical,
            List<UnityEngine.Object> created, Action<BlueprintAbility> mutate,
            string expectedPrefix, bool signatureBlind)
        {
            BlueprintAbility clone = CompositionClone(subject, name, created);
            string before = AlternateSummoningCompositionReader.Verify(subject, canonical);
            mutate(clone);
            string reason = AlternateSummoningCompositionReader.Verify(clone, canonical);
            string signature = SummonSignature.Difference(SummonSignatureReader.Read(subject),
                SummonSignatureReader.Read(clone));
            string after = AlternateSummoningCompositionReader.Verify(subject, canonical);
            record[name] = new JObject { ["reason"] = reason, ["signatureDifference"] = signature,
                ["subjectUnchanged"] = before == null && after == null };
            assertions.Add(Assertion("composition-rejects-" + name,
                "the composed-action verifier reports the changed clone precisely" +
                    (signatureBlind ? ", a change the summon signature alone does not see" : "") +
                    ", and the built child itself is unchanged",
                (reason ?? "accepted") + ";signature=" + (signature == null ? "same" : "differs") +
                    ";subjectUnchanged=" + (before == null && after == null),
                reason != null && reason.StartsWith(expectedPrefix, StringComparison.Ordinal) &&
                    before == null && after == null && (!signatureBlind || signature == null),
                "AlternateSummoningCompositionReader.Verify on a disposable clone"));
        }
    }
}
