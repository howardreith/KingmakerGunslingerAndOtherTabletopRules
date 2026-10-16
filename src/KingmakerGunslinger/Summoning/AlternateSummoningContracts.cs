using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// The supported execution templates, each as an explicit transform of
    /// its exact native source. Call of the Wild 1.14.4c builds every caller
    /// template by copying a native ability and changing a few named things;
    /// the expected template is that source with exactly those changes, and
    /// anything else - another spawn, a different quantity, rank input,
    /// duration, pool route, condition or cost - is a mismatch that blocks the
    /// family. The transforms are read from the installed provider's code
    /// (Summoner.createSummonMonster / createMasterSummons /
    /// createSummonNatureAlly, Arcanist.createOccultistSummoning,
    /// Hunter.createSummonNatureAlly, Preservationist.createBottledAlly).
    /// </summary>
    internal static class AlternateSummoningContracts
    {
        internal const string SummonerResource = "46dd52bfe6ea44a1b45b9983deb76aec";
        internal const string SummonerPool = "e71b38e5b7004ac7b046d5bbfec42d95";
        internal const string SummonerClass = "0f4c4ada51334b43a802350c5c0b85f5";
        internal const string MasterSummonerResource = "0b76da2fc6094b7b95acea13c32f5fff";
        internal const string MasterSummonerFreePool = "ae450ab8479a4c44a9ce8d6e330eed16";
        internal const string ArcaneReservoirResource = "3b775ee982444493b3de8f7bc31bd872";
        internal const string OccultistPool = "a13742e65f51420ab9582f29ded32e90";
        internal const string ArcanistClass = "19c3cf3d51cf4cbf9a136a600c26585a";
        internal const string PerfectSummoner = "6190f32ae58b4f98a8985b1919952b03";
        internal const string FeralHunterResource = "e736b67f1c364ca98c838005380644fc";
        internal const string FeralHunterPool = "629c1db9a7b349c38432271c97ee7a30";
        internal const string HunterClass = "32486dcfda61462fbfd66b5644786b39";

        internal const string CompanionDeadType =
            "CallOfTheWild.NewMechanics.AbilityCasterCompanionDead";
        internal const string CompanionDeadOrPoolEmptyType =
            "CallOfTheWild.CompanionMechanics.AbilityCasterCompanionDeadOrSummonPoolEmpty";
        internal const string HasUnitsInPoolType =
            "CallOfTheWild.NewMechanics.HasUnitsInSummonPool";
        internal const string PetIsAliveType =
            "CallOfTheWild.CompanionMechanics.ContextActionPetIsAlive";
        internal const string DecreasingCostType =
            "CallOfTheWild.NewMechanics.ResourseCostCalculatorWithDecreasingFacts";
        internal const string CasterAlignmentType =
            "Kingmaker.UnitLogic.Abilities.Components.CasterCheckers.AbilityCasterAlignment";
        internal const string ApplyTemplateType =
            "KingmakerGunslinger.Summoning.ContextActionApplySummonTemplateByCaster";
        internal const string SetAlignmentType =
            "KingmakerGunslinger.Summoning.ContextActionSetSummonAlignment";

        /// <summary>The template a caller's code builds from this source.
        /// Throws for a family with no supported template contract.</summary>
        internal static SummonSignature ExpectedTemplate(string familyKey, int tier,
            SummonSignature source)
        {
            if (source == null) throw new ArgumentNullException("source");
            switch (familyKey)
            {
                case AlternateSummoningSourceCatalog.SummonerKey:
                    return Summoner(source);
                case AlternateSummoningSourceCatalog.MasterSummonerKey:
                    return MasterSummoner(source);
                case AlternateSummoningSourceCatalog.OccultistKey:
                    return Occultist(source, tier);
                case AlternateSummoningSourceCatalog.NaturalistKey:
                    return AllyCopy(source, SummonerResource, SummonerPool, SummonerClass);
                case AlternateSummoningSourceCatalog.FeralHunterKey:
                    return AllyCopy(source, FeralHunterResource, FeralHunterPool,
                        HunterClass);
                case AlternateSummoningSourceCatalog.PreservationistKey:
                    return Extract(source);
                default:
                    throw new InvalidOperationException(
                        "No template contract for family " + familyKey + ".");
            }
        }

        /// <summary>
        /// The native Monster Tactician ability of a tier and quantity must
        /// summon exactly what the canonical native option of that tier and
        /// quantity summons, in the same order, with the same quantity, level
        /// and link expressions; its own resource, pool and duration are the
        /// tactician's. Returns null when supported.
        /// </summary>
        internal static string CheckMonsterTactician(SummonSignature tactician,
            SummonSignature canonical)
        {
            if (tactician.RunActions != 1 || tactician.Variants != 0)
                return "tactician-shape:runActions=" + tactician.RunActions +
                    ",variants=" + tactician.Variants;
            if (tactician.HiddenSpawns != 0)
                return "tactician-hidden-spawns:" + tactician.HiddenSpawns;
            if (tactician.Resources.Count != 1)
                return "tactician-resource-count:" + tactician.Resources.Count;
            string[] mine = tactician.Spawns().Select(value => value.Quantity()).ToArray();
            string[] theirs = canonical.Spawns().Select(value => value.Quantity()).ToArray();
            if (mine.Length == 0 || !mine.SequenceEqual(theirs, StringComparer.Ordinal))
                return "tactician-quantity-lineage:expected=" + string.Join("|", theirs) +
                    ";actual=" + string.Join("|", mine);
            return null;
        }

        /// <summary>
        /// What an adapter child must be, given its caller template: the same
        /// signature with exactly the deliberate changes - every spawn names
        /// the creature, a templated Summon Monster creature takes the
        /// dynamic celestial/fiendish template after each spawn and drops a
        /// fixed caster-alignment restriction and good/evil descriptors, a
        /// Nature's Ally creature takes caster alignment after each spawn.
        /// </summary>
        internal static SummonSignature ExpectedAdapterChild(SummonSignature template,
            string unit, AlternateSummoningChoiceKind kind, SummonFamily family,
            bool templated)
        {
            SummonSignature expected = template.Clone();
            expected.Variants = 0;
            if (kind == AlternateSummoningChoiceKind.Preserved) return expected;
            foreach (SummonSpawn spawn in expected.Spawns()) spawn.Unit = unit;
            if (kind != AlternateSummoningChoiceKind.Creature) return expected;
            if (family == SummonFamily.Monster && templated)
            {
                expected.CasterCheckers.RemoveAll(value => value.StartsWith(
                    CasterAlignmentType + "{", StringComparison.Ordinal));
                expected.Descriptor = WithoutFlags(expected.Descriptor, "Good", "Evil");
                foreach (SummonSpawn spawn in expected.Spawns())
                    spawn.AfterSpawn.Add(ApplyTemplateType);
            }
            else if (family == SummonFamily.NaturesAlly)
                foreach (SummonSpawn spawn in expected.Spawns())
                    spawn.AfterSpawn.Add(SetAlignmentType);
            return expected;
        }

        internal static string Checker(string type, params KeyValuePair<string, string>[] fields)
        { return SummonSignatureFormat.Component(type, fields); }

        private static SummonSignature Summoner(SummonSignature source)
        {
            SummonSignature t = source.Clone();
            ReplaceResource(t, SummonerResource);
            ClassLevelRanks(t, SummonerClass, false);
            foreach (SummonNode node in t.Nodes())
            {
                if (node.Kind == SummonNodeKind.Spawn) node.Spawn.Pool = SummonerPool;
                if (node.Kind == SummonNodeKind.Clear) node.Pool = SummonerPool;
            }
            t.CasterCheckers.Add(Checker(CompanionDeadType,
                SummonSignatureFormat.Field("not", "False")));
            return t;
        }

        private static SummonSignature MasterSummoner(SummonSignature source)
        {
            SummonSignature t = source.Clone();
            ReplaceResource(t, MasterSummonerResource);
            ClassLevelRanks(t, SummonerClass, false);
            t.Actions = ReplaceClears(t.Actions);
            t.Actions = ReplaceSpawns(t.Actions);
            t.CasterCheckers.Add(Checker(CompanionDeadOrPoolEmptyType,
                SummonSignatureFormat.Field("SummonPool", MasterSummonerFreePool),
                SummonSignatureFormat.Field("not", "False")));
            return t;
        }

        // Common.replaceActions<ContextActionClearSummonPool>: every clear,
        // wherever it is, becomes "if the eidolon lives, clear the summoner
        // pool" - one shared conditional object.
        private static List<SummonNode> ReplaceClears(IEnumerable<SummonNode> nodes)
        {
            var result = new List<SummonNode>();
            foreach (SummonNode node in nodes)
            {
                if (node.Kind == SummonNodeKind.Clear)
                    result.Add(SummonNode.ForConditional("And",
                        new[] { Checker(PetIsAliveType,
                            SummonSignatureFormat.Field("Not", "False")) },
                        new[] { SummonNode.ForClear(SummonerPool) },
                        new SummonNode[0]));
                else if (node.Kind == SummonNodeKind.Conditional)
                {
                    SummonNode copy = node.Clone();
                    copy.IfTrue = ReplaceClears(node.IfTrue);
                    copy.IfFalse = ReplaceClears(node.IfFalse);
                    result.Add(copy);
                }
                else result.Add(node.Clone());
            }
            return result;
        }

        // Common.replaceActions<ContextActionSpawnMonster>: a spawn that is
        // not already routed to one of the two summoner pools becomes "if
        // the free pool holds units, spawn into the eidolon-bound summoner
        // pool, otherwise into the free pool".
        private static List<SummonNode> ReplaceSpawns(IEnumerable<SummonNode> nodes)
        {
            var result = new List<SummonNode>();
            foreach (SummonNode node in nodes)
            {
                if (node.Kind == SummonNodeKind.Spawn)
                {
                    if (node.Spawn.Pool == SummonerPool ||
                        node.Spawn.Pool == MasterSummonerFreePool)
                    { result.Add(node.Clone()); continue; }
                    SummonSpawn bound = node.Spawn.Clone();
                    bound.Pool = SummonerPool;
                    SummonSpawn free = node.Spawn.Clone();
                    free.Pool = MasterSummonerFreePool;
                    result.Add(SummonNode.ForConditional("And",
                        new[] { Checker(HasUnitsInPoolType,
                            SummonSignatureFormat.Field("Not", "False"),
                            SummonSignatureFormat.Field("SummonPool",
                                MasterSummonerFreePool)) },
                        new[] { SummonNode.ForSpawn(bound) },
                        new[] { SummonNode.ForSpawn(free) }));
                }
                else if (node.Kind == SummonNodeKind.Conditional)
                {
                    SummonNode copy = node.Clone();
                    copy.IfTrue = ReplaceSpawns(node.IfTrue);
                    copy.IfFalse = ReplaceSpawns(node.IfFalse);
                    result.Add(copy);
                }
                else result.Add(node.Clone());
            }
            return result;
        }

        private static SummonSignature Occultist(SummonSignature source, int tier)
        {
            SummonSignature t = source.Clone();
            if (t.Resources.Count != 1)
                throw new InvalidOperationException("Occultist source resource count.");
            t.Resources[0].Resource = ArcaneReservoirResource;
            t.Resources[0].Amount = tier;
            t.Resources[0].CustomCost = true;
            t.CostCalculators.Add(Checker(DecreasingCostType,
                SummonSignatureFormat.Field("cost_increasing_facts",
                    SummonSignatureFormat.List(new string[0])),
                SummonSignatureFormat.Field("cost_reducing_facts",
                    SummonSignatureFormat.List(Enumerable.Repeat(PerfectSummoner, tier)))));
            ClassLevelRanks(t, ArcanistClass, false);
            foreach (SummonNode node in t.Nodes())
            {
                if (node.Kind == SummonNodeKind.Clear) node.Pool = OccultistPool;
                if (node.Kind != SummonNodeKind.Spawn) continue;
                SummonSpawn spawn = node.Spawn;
                SummonValue sourceBonus = spawn.DurationBonus;
                spawn.Pool = OccultistPool;
                // CreateContextDuration(rank, Minutes, DiceType.One, bonus):
                // the rank is the bonus, the old bonus becomes the dice count.
                spawn.DurationRate = "Minutes";
                spawn.DurationDice = "One";
                spawn.DurationDiceCount = sourceBonus;
                spawn.DurationBonus = SummonValue.RankOf("DamageDiceAlternative");
                spawn.DurationExtendable = true;
            }
            t.Ranks.Add(new SummonRank {
                Type = "DamageDiceAlternative", BaseValueType = "FeatureRank",
                Progression = "MultiplyByModifier", Archetype = "null",
                Feature = PerfectSummoner, Stat = "Unknown", StartLevel = 0,
                StepLevel = 1420, UseMin = false, Min = 0, UseMax = false, Max = 0,
                ExceptClasses = false, CustomProperty = "null", CustomProgression = "[]",
                BasedOnClassLevel = false, FeatureListBased = false });
            return t;
        }

        private static SummonSignature AllyCopy(SummonSignature source,
            string resource, string pool, string characterClass)
        {
            SummonSignature t = source.Clone();
            t.SpellLists = 0;
            t.Resources.Add(new SummonResource { Resource = resource, Spend = true,
                Amount = 1, CustomCost = false });
            ClassLevelRanks(t, characterClass, true);
            foreach (SummonSpawn spawn in t.Spawns())
            {
                spawn.Pool = pool;
                // CreateContextDuration(bonus, Minutes): no dice, same bonus.
                spawn.DurationRate = "Minutes";
                spawn.DurationDice = "Zero";
                spawn.DurationDiceCount = SummonValue.Simple(0);
                spawn.DurationExtendable = true;
            }
            t.Actions.Insert(0, SummonNode.ForClear(pool));
            t.CasterCheckers.Add(Checker(CompanionDeadType,
                SummonSignatureFormat.Field("not", "False")));
            t.FullRoundField = false;
            return t;
        }

        private static SummonSignature Extract(SummonSignature source)
        {
            SummonSignature t = source.Clone();
            t.SpellLists = 0;
            t.FullRoundField = false;
            return t;
        }

        private static void ReplaceResource(SummonSignature t, string resource)
        {
            if (t.Resources.Count != 1)
                throw new InvalidOperationException("Source resource count " +
                    t.Resources.Count + ".");
            t.Resources[0].Resource = resource;
        }

        /// <summary>The Summon Monster copies replace class-level ranks with
        /// the caller's class; the Nature's Ally copies replace every
        /// non-feature-list rank with the caller's class level.</summary>
        private static void ClassLevelRanks(SummonSignature t, string characterClass,
            bool everyNonFeatureList)
        {
            foreach (SummonRank rank in t.Ranks)
            {
                if (everyNonFeatureList ? rank.FeatureListBased : !rank.BasedOnClassLevel)
                    continue;
                rank.Classes = new List<string> { characterClass };
                if (everyNonFeatureList)
                {
                    rank.BaseValueType = "ClassLevel";
                    rank.BasedOnClassLevel = true;
                }
            }
        }

        private static string WithoutFlags(string descriptor, params string[] flags)
        {
            if (string.IsNullOrEmpty(descriptor)) return descriptor;
            string[] kept = descriptor.Split(new[] { ", " }, StringSplitOptions.None)
                .Where(value => !flags.Contains(value)).ToArray();
            return kept.Length == 0 ? "None" : string.Join(", ", kept);
        }
    }
}
