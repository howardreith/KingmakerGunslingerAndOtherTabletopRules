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
