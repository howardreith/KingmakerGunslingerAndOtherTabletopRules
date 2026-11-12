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
        /// GS-AUD-0025. InheritedTemplateRemovalCreatures is a hard-coded list
        /// of twelve creature keys, and 63 Summon Monster creatures are
        /// untemplated - so membership is NOT simply "untemplated". The list
        /// asserts a property of the installed native library: that this
        /// creature's native carrier still applies a celestial/fiendish
        /// template after spawn. Nothing measured that, which means a
        /// thirteenth affected creature would silently keep a template the
        /// ordinary spell removes, and the repair would look complete.
        ///
        /// So derive the set from the live library instead of trusting it. For
        /// every untemplated Summon Monster creature, read the registered
        /// canonical ability's own spawn graph: it is a copy of the native
        /// carrier, with the spawn unit replaced, so an inherited template
        /// shows up as a ContextActionApplyBuff carrying one of the four native
        /// template identities. The derived set must equal the declared set
        /// exactly, and exactly those creatures must carry the removal action.
        ///
        /// This runs at mod load against the installed library, so it needs no
        /// save and no cast.
        /// </summary>
        private static void InheritedTemplateRemovalSetIsDerived(
            List<RuntimeTestAssertion> assertions, JObject record)
        {
            var nativeIds = new HashSet<string>(
                ExpandedSummoningAbilityBuilder.NativeTemplateIds, StringComparer.Ordinal);
            var inherits = new SortedSet<string>(StringComparer.Ordinal);
            var carriesRemoval = new SortedSet<string>(StringComparer.Ordinal);
            var unresolved = new SortedSet<string>(StringComparer.Ordinal);
            var examined = new SortedSet<string>(StringComparer.Ordinal);

            foreach (SummonVariantSpec variant in ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster))
            {
                if (variant.Creature.MonsterTemplated) continue;
                string key = variant.Creature.Key;
                examined.Add(key);
                BlueprintScriptableObject registered;
                if (!ExpandedSummoningBlueprints.Registered.BySymbol.TryGetValue(
                        ExpandedSummoningIdentityCatalog.AbilitySymbol(variant), out registered))
                {
                    unresolved.Add(key);
                    continue;
                }
                var ability = registered as BlueprintAbility;
                if (ability == null) { unresolved.Add(key); continue; }
                if (AlternateSummoningGraph.Objects<ContextActionApplyBuff>(
                        ability.ComponentsArray).Any(action => action.Buff != null &&
                        nativeIds.Contains(action.Buff.AssetGuid)))
                    inherits.Add(key);
                if (AlternateSummoningGraph
                        .Objects<ContextActionRemoveInheritedSummonTemplates>(
                            ability.ComponentsArray).Any())
                    carriesRemoval.Add(key);
            }

            var declared = new SortedSet<string>(ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster)
                .Select(variant => variant.Creature)
                .Where(creature => ExpandedSummoningAbilityBuilder
                    .RequiresInheritedTemplateRemoval(SummonFamily.Monster, creature))
                .Select(creature => creature.Key), StringComparer.Ordinal);

            string[] extra = inherits.Except(declared, StringComparer.Ordinal).ToArray();
            string[] missing = declared.Except(inherits, StringComparer.Ordinal).ToArray();
            bool removalMatches = carriesRemoval.SetEquals(declared);
            record["derivedInheriting"] = string.Join(",", inherits.ToArray());
            record["declaredRemovalSet"] = string.Join(",", declared.ToArray());
            record["untemplatedExamined"] = examined.Count;

            assertions.Add(Assertion("inherited-template-removal-set-is-derived",
                "every untemplated Summon Monster creature whose registered graph still applies a " +
                    "native celestial/fiendish template is declared in " +
                    "InheritedTemplateRemovalCreatures, and exactly those creatures carry the " +
                    "removal action",
                "untemplatedExamined=" + examined.Count + ";declared=" + declared.Count +
                    ";derived=" + inherits.Count + ";undeclaredButInheriting=[" +
                    string.Join(",", extra) + "];declaredButNotInheriting=[" +
                    string.Join(",", missing) + "];removalCarriers=" + carriesRemoval.Count +
                    ";removalSetMatches=" + removalMatches + ";unresolved=[" +
                    string.Join(",", unresolved.ToArray()) + "]",
                unresolved.Count == 0 && examined.Count > declared.Count &&
                    extra.Length == 0 && missing.Length == 0 && removalMatches,
                "registered canonical graphs in the installed library; " +
                    "ContextActionApplyBuff identities compared against " +
                    "ExpandedSummoningAbilityBuilder.NativeTemplateIds"));
        }

        /// <summary>
        /// GS-AUD-0025. The controls above run on a templated creature and on a
        /// Nature's Ally creature, so NONE of them enters the TemplateRemoval
        /// comparison branch - which is the branch the inherited-template
        /// removal, and the multiset identity repair, actually live in. A
        /// passing composition observation over those two subjects therefore
        /// says nothing about the removal. These controls use an AFFECTED
        /// subject: an untemplated Summon Monster creature whose native carrier
        /// still applies a celestial/fiendish template after spawn.
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
