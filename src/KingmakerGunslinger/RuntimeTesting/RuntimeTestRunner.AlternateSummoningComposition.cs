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
            InheritedTemplateRemovalSetIsDerived(assertions, record);
        }

        /// <summary>
        /// GS-AUD-0025 / GS-REP-0002. InheritedTemplateRemovalCreatures is a
        /// hard-coded list of twelve creature KEYS, and 63 untemplated Summon
        /// Monster creatures exist, so membership is not simply "untemplated".
        /// The list asserts a property of the installed native library: that a
        /// creature's native carrier still applies a celestial/fiendish
        /// template after spawn. Nothing measured that.
        ///
        /// This observation is deliberately kept PER PLACEMENT, not per
        /// creature. ExpandedSummoningCatalog.GenerateVariants admits a
        /// creature under every parent tier from its source tier to 9, so one
        /// creature has up to eight placements and they need not agree. An
        /// earlier version of this observation reduced everything to sets of
        /// creature keys and then labelled each key with the ParentTier of its
        /// FIRST variant - which is just its source tier - and on that basis a
        /// "parent tier 2-4 versus 5-9" boundary was reported. That conclusion
        /// was not supported: a creature-level set cannot say which of its
        /// placements carries the action, and a creature-level set also cannot
        /// notice a removal action disappearing from ONE placement while
        /// another still has it.
        ///
        /// So each placement is recorded on its own with its StableKey, source
        /// tier, parent tier, multiplicity, the native template identities its
        /// graph applies, and whether it carries the removal. Creature-level
        /// summaries are derived from those rows rather than replacing them.
        ///
        /// This is GRAPH evidence only. It says what a registered ability's
        /// action graph contains. It is not evidence that any particular summon
        /// retained or lost a buff at run time; the spawned-unit observation in
        /// disposable-alternate-summoning-natural is a separate thing and
        /// covers only the one route it casts.
        /// </summary>
        private static void InheritedTemplateRemovalSetIsDerived(
            List<RuntimeTestAssertion> assertions, JObject record)
        {
            var nativeIds = new HashSet<string>(
                ExpandedSummoningAbilityBuilder.NativeTemplateIds, StringComparer.Ordinal);
            var rows = new JArray();
            var unresolved = new SortedSet<string>(StringComparer.Ordinal);
            var creatureApplies = new Dictionary<string, int>(StringComparer.Ordinal);
            var creaturePlacements = new Dictionary<string, int>(StringComparer.Ordinal);
            var wiringMismatches = new List<string>();
            var appliesByTierPair = new SortedDictionary<string, int>(StringComparer.Ordinal);
            int placements = 0;

            foreach (SummonVariantSpec variant in ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster))
            {
                if (variant.Creature.MonsterTemplated) continue;
                string key = variant.Creature.Key;
                placements++;
                creaturePlacements[key] = creaturePlacements.ContainsKey(key) ?
                    creaturePlacements[key] + 1 : 1;
                if (!creatureApplies.ContainsKey(key)) creatureApplies[key] = 0;

                BlueprintScriptableObject registered;
                if (!ExpandedSummoningBlueprints.Registered.BySymbol.TryGetValue(
                        ExpandedSummoningIdentityCatalog.AbilitySymbol(variant), out registered))
                { unresolved.Add(variant.StableKey); continue; }
                var ability = registered as BlueprintAbility;
                if (ability == null) { unresolved.Add(variant.StableKey); continue; }

                string[] applied = AlternateSummoningGraph
                    .Objects<ContextActionApplyBuff>(ability.ComponentsArray)
                    .Where(action => action.Buff != null && nativeIds.Contains(action.Buff.AssetGuid))
                    .Select(action => action.Buff.AssetGuid)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray();
                bool removal = AlternateSummoningGraph
                    .Objects<ContextActionRemoveInheritedSummonTemplates>(
                        ability.ComponentsArray).Any();
                bool declared = ExpandedSummoningAbilityBuilder
                    .RequiresInheritedTemplateRemoval(SummonFamily.Monster, variant.Creature);

                if (applied.Length > 0)
                {
                    creatureApplies[key] = creatureApplies[key] + 1;
                    string pair = "source" + variant.SourceTier + "/parent" + variant.ParentTier;
                    appliesByTierPair[pair] = appliesByTierPair.ContainsKey(pair) ?
                        appliesByTierPair[pair] + 1 : 1;
                }
                // Per PLACEMENT, which is what makes this able to see a single
                // placement losing its removal action.
                if (removal != declared)
                    wiringMismatches.Add(variant.StableKey + ":removal=" + removal +
                        ";declared=" + declared);

                rows.Add(new JObject {
                    ["stableKey"] = variant.StableKey,
                    ["creature"] = key,
                    ["sourceTier"] = variant.SourceTier,
                    ["parentTier"] = variant.ParentTier,
                    ["multiplicity"] = variant.Multiplicity.ToString(),
                    ["appliedNativeTemplates"] = new JArray(applied),
                    ["carriesRemoval"] = removal,
                    ["declared"] = declared });
            }

            string[] declaredKeys = creaturePlacements.Keys
                .Where(key => ExpandedSummoningAbilityBuilder.RequiresInheritedTemplateRemoval(
                    SummonFamily.Monster, ExpandedSummoningCatalog.GenerateVariants(
                        SummonFamily.Monster).First(value => value.Creature.Key == key).Creature))
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            // "Every placement applies" and "some placement applies" are
            // different claims, so both are reported rather than conflated.
            string[] allPlacementsApply = creatureApplies.Where(entry =>
                entry.Value > 0 && entry.Value == creaturePlacements[entry.Key])
                .Select(entry => entry.Key).OrderBy(value => value, StringComparer.Ordinal).ToArray();
            string[] somePlacementsApply = creatureApplies.Where(entry =>
                entry.Value > 0 && entry.Value < creaturePlacements[entry.Key])
                .Select(entry => entry.Key).OrderBy(value => value, StringComparer.Ordinal).ToArray();

            record["inheritedTemplatePlacements"] = rows;
            record["inheritedTemplateSummary"] = new JObject {
                ["placements"] = placements,
                ["creatures"] = creaturePlacements.Count,
                ["declaredCreatures"] = new JArray(declaredKeys),
                ["everyPlacementApplies"] = new JArray(allPlacementsApply),
                ["somePlacementsApply"] = new JArray(somePlacementsApply),
                ["appliesByTierPair"] = new JObject(appliesByTierPair
                    .Select(entry => new JProperty(entry.Key, entry.Value))),
                ["wiringMismatches"] = new JArray(wiringMismatches),
                ["unresolved"] = new JArray(unresolved) };

            // What is asserted here is only the WIRING invariant this branch is
            // responsible for, per placement: the removal action is present on
            // exactly the placements whose creature is declared. The
            // declared-versus-measured discrepancy itself is GS-REP-0002, is
            // pre-existing on master, and is reported as evidence above rather
            // than asserted - the earlier version of this assertion pinned a
            // creature-level summary and a tier boundary that the data does not
            // establish.
            assertions.Add(Assertion("inherited-template-removal-wiring-per-placement",
                "every untemplated Summon Monster placement carries the inherited-template removal " +
                    "action exactly when its creature is declared, checked per placement rather " +
                    "than per creature so one placement losing the action cannot hide behind " +
                    "another that keeps it; every symbol resolves; and more placements are " +
                    "examined than there are creatures. The declared-versus-measured set " +
                    "discrepancy is GS-REP-0002, recorded as evidence here and NOT asserted",
                "placements=" + placements + ";creatures=" + creaturePlacements.Count +
                    ";declaredCreatures=" + declaredKeys.Length +
                    ";everyPlacementApplies=" + allPlacementsApply.Length +
                    ";somePlacementsApply=" + somePlacementsApply.Length +
                    ";wiringMismatches=[" + string.Join(",", wiringMismatches.ToArray()) +
                    "];unresolved=[" + string.Join(",", unresolved.ToArray()) + "]",
                unresolved.Count == 0 && wiringMismatches.Count == 0 &&
                    placements > creaturePlacements.Count,
                "registered canonical graphs in the installed library, one record per placement; " +
                    "ContextActionApplyBuff identities compared against " +
                    "ExpandedSummoningAbilityBuilder.NativeTemplateIds. Graph evidence only - it " +
                    "is not evidence that any summon retained or lost a buff at run time"));
        }        /// <summary>
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
                            choice.Placement.Family, choice.Placement.Creature);
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
