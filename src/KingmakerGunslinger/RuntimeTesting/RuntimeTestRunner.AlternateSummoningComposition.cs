using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
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
        /// observation per published ability: every canonical placement of both
        /// families (a templated Summon Monster placement contributes its
        /// caster-chosen, celestial and fiendish abilities), and every Master
        /// Summoner and Naturalist alternate child built through the production
        /// adapter. Each observation carries three separately read inputs:
        /// intent from ExpandedSummoningCatalog, the native template identities
        /// the placement's donor carrier applies (read here from the donor
        /// graph), and the generated graph's spawn branches. The engine-free
        /// InheritedTemplateCensus judges them with its own statement of the
        /// contract, so InheritedTemplatePolicy does not certify itself.
        ///
        /// Graph evidence only: a spawned unit's buffs are a native-cast
        /// question. The v0.0.186 observation this replaces (65 apply without
        /// removal, 36 removal without application) is retained under its own
        /// session identity in docs/EXPANDED-SUMMONING-INHERITED-TEMPLATES.md.
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

            foreach (SummonFamily family in new[] { SummonFamily.Monster, SummonFamily.NaturesAlly })
            foreach (SummonVariantSpec variant in ExpandedSummoningCatalog.GenerateVariants(family))
            {
                string symbol = ExpandedSummoningIdentityCatalog.AbilitySymbol(variant);
                bool templated = family == SummonFamily.Monster && variant.Creature.MonsterTemplated;
                string[] donorApplied = CensusDonorApplied(lookup, variant, nativeIds, unresolved);
                BlueprintUnit unit = CensusUnit(variant, unresolved);
                foreach (string published in templated ?
                    new[] { symbol, symbol + ".Celestial", symbol + ".Fiendish" } : new[] { symbol })
                {
                    expected.Add("canonical|" + published);
                    BlueprintScriptableObject registered;
                    if (!ExpandedSummoningBlueprints.Registered.BySymbol.TryGetValue(published, out registered) ||
                        !(registered is BlueprintAbility) || unit == null)
                    { unresolved.Add(published); continue; }
                    observations.Add(CensusObservation(published, "canonical", variant, templated,
                        donorApplied, (BlueprintAbility)registered, unit, nativeIds, rows));
                }
            }

            foreach (string familyKey in new[] { AlternateSummoningSourceCatalog.MasterSummonerKey,
                AlternateSummoningSourceCatalog.NaturalistKey })
            {
                AlternateSummoningFamilySpec family = AlternateSummoningSourceCatalog.Find(familyKey);
                AlternateSummoningFamilyIdentity identity = AlternateSummoningSourceCatalog
                    .Identities.Single(value => value.Key == family.Key);
                AlternateSummoningResolvedFamily resolved = AlternateSummoningHostResolver.Resolve(
                    lookup, family, identity, tactician, rootAbility =>
                        AlternateSummoningCoordinator.OriginalHostChildren(family.Key, rootAbility.AssetGuid));
                // Expected keys from the projection; actual children from the production builder.
                AlternateSummoningChoice[] choices = AlternateSummoningProjection
                    .Identities(identity, AlternateSummoningRoster.Production())
                    .Where(choice => choice.Kind == AlternateSummoningChoiceKind.Creature).ToArray();
                foreach (AlternateSummoningChoice choice in choices)
                    expected.Add("alternate:" + familyKey + "|" + choice.Symbol);
                AlternateSummoningBuiltFamily built = AlternateSummoningAdapterBuilder.Build(
                    lookup, symbol =>
                    {
                        BlueprintAbility shell = AlternateSummoningAdapterBuilder.CreateShell(symbol);
                        created.Add(shell);
                        return shell;
                    }, resolved, AlternateSummoningRoster.Production(),
                    ExpandedSummoningBlueprints.Registered,
                    choice => choice.Kind == AlternateSummoningChoiceKind.Creature);
                foreach (AlternateSummoningChoice choice in choices)
                {
                    BlueprintAbility child;
                    if (!built.BySymbol.TryGetValue(choice.Symbol, out child)) continue;
                    SummonVariantSpec placement = choice.Placement;
                    BlueprintUnit unit = CensusUnit(placement, unresolved);
                    if (unit == null) continue;
                    observations.Add(CensusObservation(choice.Symbol, "alternate:" + familyKey, placement,
                        placement.Family == SummonFamily.Monster && placement.Creature.MonsterTemplated,
                        CensusDonorApplied(lookup, placement, nativeIds, unresolved), child, unit, nativeIds, rows));
                }
            }

            InheritedTemplateCensusResult result = InheritedTemplateCensus.Evaluate(observations, expected, nativeIds);
            var dispositions = new JObject(result.Dispositions.OrderBy(entry => entry.Key)
                .Select(entry => new JProperty(entry.Key.ToString(), entry.Value)));
            record["inheritedTemplateCensus"] = rows;
            record["inheritedTemplateCensusSummary"] = new JObject {
                ["observations"] = result.Observations,
                ["expected"] = expected.Count,
                ["canonical"] = observations.Count(value => value.Caller == "canonical"),
                ["alternate"] = observations.Count(value => value.IsAlternate),
                ["dispositions"] = dispositions,
                ["findings"] = new JArray(result.Findings),
                ["justifiedRedundant"] = new JArray(result.JustifiedRedundant),
                ["unresolved"] = new JArray(unresolved) };
            int quantityRemovals = observations.Count(value => value.Caller == "canonical" &&
                value.Key.IndexOf("OneD", StringComparison.Ordinal) >= 0 &&
                InheritedTemplateCensus.Expected(value, nativeIds) == InheritedTemplateDisposition.RemoveInherited);
            int deliberate = observations.Count(value => value.DeliberateTemplateAbility);
            assertions.Add(Assertion("inherited-template-census-per-placement",
                "every published Summon Monster and Nature's Ally ability, canonical and alternate, has the " +
                    "inherited-template disposition its own placement requires: an untemplated Summon Monster " +
                    "placement whose donor applies a native celestial/fiendish template removes exactly the " +
                    "native identities after the donor's applications, bound to its own unit; no other " +
                    "placement carries a removal or keeps an unremoved native application; deliberately " +
                    "templated abilities keep their project template; no expected placement is skipped; " +
                    "every redundant removal is listed separately with its reason",
                "observations=" + result.Observations + "/" + expected.Count + ";dispositions=" +
                    dispositions.ToString(Newtonsoft.Json.Formatting.None) + ";quantityRemovals=" +
                    quantityRemovals + ";deliberateTemplateAbilities=" + deliberate + ";justifiedRedundant=" +
                    result.JustifiedRedundant.Count + ";findings=[" + string.Join(",", result.Findings.Take(40).ToArray()) +
                    "];unresolved=[" + string.Join(",", unresolved.Take(20).ToArray()) + "]",
                result.Passed && unresolved.Count == 0 && result.Observations == expected.Count &&
                    quantityRemovals > 0 && deliberate > 0,
                "registered canonical abilities and production-built alternate children in the installed " +
                    "library; donor carriers read separately; InheritedTemplateCensus. Graph evidence only"));
        }

        private static string[] CensusDonorApplied(Func<string, BlueprintScriptableObject> lookup,
            SummonVariantSpec variant, ICollection<string> nativeIds, ISet<string> unresolved)
        {
            BlueprintAbility donor;
            try { donor = ExpandedSummoningAbilityBuilder.DonorTemplate(lookup, variant); }
            catch (InvalidOperationException) { unresolved.Add("donor:" + variant.StableKey); return new string[0]; }
            return AlternateSummoningGraph.Objects<ContextActionApplyBuff>(donor.ComponentsArray)
                .Where(action => action.Buff != null && nativeIds.Contains(action.Buff.AssetGuid))
                .Select(action => action.Buff.AssetGuid).Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
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

        private static InheritedTemplateObservation CensusObservation(string key, string caller,
            SummonVariantSpec variant, bool templated, string[] donorApplied, BlueprintAbility ability,
            BlueprintUnit unit, ICollection<string> nativeIds, JArray rows)
        {
            bool deliberateAbility = templated;
            var branches = new List<InheritedTemplateBranch>();
            var branchRows = new JArray();
            foreach (ContextActionSpawnMonster spawn in AlternateSummoningGraph
                .Objects<ContextActionSpawnMonster>(ability.ComponentsArray))
            {
                GameAction[] actions = spawn.AfterSpawn == null || spawn.AfterSpawn.Actions == null ?
                    new GameAction[0] : spawn.AfterSpawn.Actions;
                int lastApply = -1, deliberateIndex = -1;
                var removals = new List<int>();
                bool bound = true, exact = true;
                for (int index = 0; index < actions.Length; index++)
                {
                    GameAction action = actions[index];
                    if (action == null) continue;
                    bool appliesNative = (action is ContextActionApplyBuff &&
                            ((ContextActionApplyBuff)action).Buff != null &&
                            nativeIds.Contains(((ContextActionApplyBuff)action).Buff.AssetGuid)) ||
                        AlternateSummoningGraph.Objects<ContextActionApplyBuff>(action).Any(apply =>
                            apply.Buff != null && nativeIds.Contains(apply.Buff.AssetGuid));
                    if (appliesNative) lastApply = index;
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
                branches.Add(new InheritedTemplateBranch(lastApply, removals.ToArray(), bound, exact, deliberate));
                branchRows.Add(new JObject { ["actions"] = actions.Length, ["lastNativeApply"] = lastApply,
                    ["removals"] = new JArray(removals), ["bound"] = bound, ["exact"] = exact,
                    ["deliberateTemplateAfterDonor"] = deliberate });
            }
            var observation = new InheritedTemplateObservation(key, caller, variant.Family, templated,
                deliberateAbility, donorApplied, branches.ToArray());
            rows.Add(new JObject {
                ["key"] = key, ["caller"] = caller, ["family"] = variant.Family.ToString(),
                ["creature"] = variant.Creature.Key, ["placement"] = variant.StableKey,
                ["parentTier"] = variant.ParentTier, ["multiplicity"] = variant.Multiplicity.ToString(),
                ["templated"] = templated, ["donorApplied"] = new JArray(donorApplied),
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
                            choice.Placement, ExpandedSummoningAbilityBuilder.DonorTemplate(
                                lookup, choice.Placement));
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
