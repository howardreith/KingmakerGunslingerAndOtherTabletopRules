using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// The supported-template contracts and the adapter-child verifier, on
    /// pure signatures: each transform is exactly the provider's, and every
    /// significant change - rank input, duration, cost-reduction facts,
    /// quantity, pool or eidolon branching, an added or removed spawn - is
    /// a difference.
    /// </summary>
    internal static class AlternateSummoningContractTests
    {
        private const string TacticianResource = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        private const string TacticianPool = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
        private const string InquisitorClass = "cccccccccccccccccccccccccccccccc";
        private const string Wolf = "dddddddddddddddddddddddddddddddd";
        private const string Dog = "eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee";
        private const string KmgWolf = "ffffffffffffffffffffffffffffffff";

        internal static void MasterSummonerContractIsTheProvidersTransform()
        {
            SummonSignature source = TacticianTemplate();
            SummonSignature expected = AlternateSummoningContracts.ExpectedTemplate(
                AlternateSummoningSourceCatalog.MasterSummonerKey, 3, source);
            Assertions.Equal(AlternateSummoningContracts.MasterSummonerResource,
                expected.Resources.Single().Resource, "Master Summoner spends its own uses.");
            Assertions.True(expected.CasterCheckers.Contains(AlternateSummoningContracts
                .Checker(AlternateSummoningContracts.CompanionDeadOrPoolEmptyType,
                    SummonSignatureFormat.Field("SummonPool",
                        AlternateSummoningContracts.MasterSummonerFreePool),
                    SummonSignatureFormat.Field("not", "False"))),
                "The eidolon/free-pool caster condition is required.");
            SummonNode clear = expected.Actions[0];
            Assertions.True(clear.Kind == SummonNodeKind.Conditional &&
                clear.Conditions.Single().StartsWith(
                    AlternateSummoningContracts.PetIsAliveType) &&
                clear.IfTrue.Single().Pool == AlternateSummoningContracts.SummonerPool &&
                clear.IfFalse.Count == 0, "Clears run only while the eidolon lives.");
            SummonSpawn[] spawns = expected.Spawns().ToArray();
            Assertions.Equal(4, spawns.Length,
                "Each source spawn becomes an eidolon-bound and a free spawn.");
            SummonNode branch = expected.Nodes().First(value =>
                value.Kind == SummonNodeKind.Conditional && value.Conditions.Any(
                    condition => condition.StartsWith(
                        AlternateSummoningContracts.HasUnitsInPoolType)));
            Assertions.True(branch.IfTrue.Single().Spawn.Pool ==
                AlternateSummoningContracts.SummonerPool &&
                branch.IfFalse.Single().Spawn.Pool ==
                    AlternateSummoningContracts.MasterSummonerFreePool,
                "With units in the free pool the next group is eidolon-bound.");
            Assertions.True(expected.Ranks.Single(value => value.BasedOnClassLevel)
                .Classes.SequenceEqual(new[] { AlternateSummoningContracts.SummonerClass }) &&
                expected.Ranks.Single(value => !value.BasedOnClassLevel).Classes.Count == 0,
                "Only class-level ranks take the summoner class.");
            Assertions.True(SummonSignature.Difference(expected, expected.Clone()) == null,
                "An exact copy matches.");
        }

        internal static void VerifierRejectsEverySignificantChange()
        {
            SummonSignature expected = AlternateSummoningContracts.ExpectedTemplate(
                AlternateSummoningSourceCatalog.MasterSummonerKey, 3, TacticianTemplate());
            var changes = new Dictionary<string, Action<SummonSignature>> {
                { "rank class input", value => value.Ranks.Single(rank =>
                    rank.BasedOnClassLevel).Classes[0] = InquisitorClass },
                { "rank step", value => value.Ranks[0].StepLevel += 1 },
                { "duration bonus", value => value.Spawns().First().DurationBonus =
                    SummonValue.Simple(1) },
                { "duration rate", value => value.Spawns().First().DurationRate = "Hours" },
                { "quantity dice", value => value.Spawns().First().CountDice = "D6" },
                { "quantity bonus", value => value.Spawns().First().CountBonus =
                    SummonValue.Simple(2) },
                { "swapped eidolon branch", value =>
                    {
                        SummonNode branch = value.Nodes().First(node =>
                            node.Kind == SummonNodeKind.Conditional &&
                            node.IfFalse.Count == 1);
                        List<SummonNode> swap = branch.IfTrue;
                        branch.IfTrue = branch.IfFalse;
                        branch.IfFalse = swap;
                    } },
                { "unguarded clear", value => value.Actions[0] =
                    SummonNode.ForClear(AlternateSummoningContracts.SummonerPool) },
                { "extra spawn", value => value.Actions.Add(SummonNode.ForSpawn(
                    value.Spawns().First().Clone())) },
                { "resource spend", value => value.Resources[0].Amount = 2 },
                { "caster condition", value => value.CasterCheckers.RemoveAt(0) },
                { "creature", value => value.Spawns().First().Unit = Dog },
                { "hidden spawn", value => value.HiddenSpawns = 1 }
            };
            foreach (KeyValuePair<string, Action<SummonSignature>> change in changes)
            {
                SummonSignature actual = expected.Clone();
                change.Value(actual);
                Assertions.True(SummonSignature.Difference(expected, actual) != null,
                    "The verifier accepted a changed " + change.Key + ".");
            }
        }

        internal static void OccultistContractTracksTierCostAndCapstone()
        {
            for (int tier = 1; tier <= 9; tier++)
            {
                SummonSignature expected = AlternateSummoningContracts.ExpectedTemplate(
                    AlternateSummoningSourceCatalog.OccultistKey, tier, TacticianTemplate());
                SummonResource resource = expected.Resources.Single();
                Assertions.True(resource.Resource == AlternateSummoningContracts
                    .ArcaneReservoirResource && resource.Amount == tier &&
                    resource.CustomCost, "Reservoir cost must be the selected tier.");
                Assertions.True(expected.CostCalculators.Single() ==
                    AlternateSummoningContracts.Checker(
                        AlternateSummoningContracts.DecreasingCostType,
                        SummonSignatureFormat.Field("cost_increasing_facts", "[]"),
                        SummonSignatureFormat.Field("cost_reducing_facts",
                            SummonSignatureFormat.List(Enumerable.Repeat(
                                AlternateSummoningContracts.PerfectSummoner, tier)))),
                    "Perfect Summoner must remove the whole tier cost.");
                SummonRank capstone = expected.Ranks.Single(value =>
                    value.Type == "DamageDiceAlternative");
                Assertions.True(capstone.BaseValueType == "FeatureRank" &&
                    capstone.Feature == AlternateSummoningContracts.PerfectSummoner &&
                    capstone.StepLevel == 1420, "The one-day capstone rank is required.");
                Assertions.True(expected.Spawns().All(value => value.Pool ==
                    AlternateSummoningContracts.OccultistPool &&
                    value.DurationRate == "Minutes" && value.DurationDice == "One" &&
                    value.DurationBonus.ToString() == "Rank(DamageDiceAlternative)"),
                    "Minutes per level plus the capstone rank, one occultist pool.");
                SummonSignature fewerFacts = expected.Clone();
                fewerFacts.CostCalculators[0] = AlternateSummoningContracts.Checker(
                    AlternateSummoningContracts.DecreasingCostType,
                    SummonSignatureFormat.Field("cost_increasing_facts", "[]"),
                    SummonSignatureFormat.Field("cost_reducing_facts",
                        SummonSignatureFormat.List(Enumerable.Repeat(
                            AlternateSummoningContracts.PerfectSummoner, tier - 1))));
                Assertions.True(SummonSignature.Difference(expected, fewerFacts) != null,
                    "A changed cost-reduction fact list must be rejected.");
            }
        }

        internal static void NaturesAllyCopiesFollowTheirProviders()
        {
            SummonSignature source = NativeAllyChild();
            foreach (string key in new[] { AlternateSummoningSourceCatalog.NaturalistKey,
                AlternateSummoningSourceCatalog.FeralHunterKey })
            {
                SummonSignature expected = AlternateSummoningContracts.ExpectedTemplate(
                    key, 4, source);
                bool feral = key == AlternateSummoningSourceCatalog.FeralHunterKey;
                string pool = feral ? AlternateSummoningContracts.FeralHunterPool :
                    AlternateSummoningContracts.SummonerPool;
                Assertions.True(expected.SpellLists == 0 && !expected.FullRoundField &&
                    expected.Resources.Single().Resource == (feral ?
                        AlternateSummoningContracts.FeralHunterResource :
                        AlternateSummoningContracts.SummonerResource) &&
                    expected.Actions[0].Kind == SummonNodeKind.Clear &&
                    expected.Actions[0].Pool == pool &&
                    expected.Spawns().All(value => value.Pool == pool &&
                        value.DurationRate == "Minutes" &&
                        value.DurationBonus.ToString() == "Rank(Default)") &&
                    expected.Ranks.All(value => value.BaseValueType == "ClassLevel" &&
                        value.Classes.Single() == (feral ?
                            AlternateSummoningContracts.HunterClass :
                            AlternateSummoningContracts.SummonerClass)),
                    key + " template contract changed.");
            }
            SummonSignature extract = AlternateSummoningContracts.ExpectedTemplate(
                AlternateSummoningSourceCatalog.PreservationistKey, 4, source);
            SummonSignature reference = source.Clone();
            reference.SpellLists = 0;
            reference.FullRoundField = false;
            Assertions.True(SummonSignature.Difference(reference, extract) == null,
                "An extract keeps everything but its spell list and full-round flag.");
        }

        internal static void TacticianNeedsTheCanonicalQuantity()
        {
            SummonSignature tactician = TacticianTemplate();
            Assertions.True(AlternateSummoningContracts.CheckMonsterTactician(
                tactician, Spawning(Wolf, Dog)) == null,
                "The same creatures and quantities in the same order are supported.");
            Assertions.True(AlternateSummoningContracts.CheckMonsterTactician(
                tactician, Spawning(Dog, Wolf)) != null, "Reordered lineage is unknown.");
            SummonSignature other = tactician.Clone();
            other.Spawns().First().CountDice = "D4";
            Assertions.True(AlternateSummoningContracts.CheckMonsterTactician(
                other, Spawning(Wolf, Dog)) != null, "A changed quantity is unknown.");
            other = tactician.Clone();
            other.HiddenSpawns = 1;
            Assertions.True(AlternateSummoningContracts.CheckMonsterTactician(
                other, Spawning(Wolf, Dog)) != null, "Unmodelled spawns are unsupported.");
            other = tactician.Clone();
            other.Resources.Add(other.Resources[0].Clone());
            Assertions.True(AlternateSummoningContracts.CheckMonsterTactician(
                other, Spawning(Wolf, Dog)) != null, "Two resources are unsupported.");
        }

        internal static void AdapterChildAllowsOnlyTheDeliberateChanges()
        {
            SummonSignature template = AlternateSummoningContracts.ExpectedTemplate(
                AlternateSummoningSourceCatalog.MasterSummonerKey, 3, TacticianTemplate());
            template.CasterCheckers.Add(AlternateSummoningContracts.Checker(
                AlternateSummoningContracts.CasterAlignmentType,
                SummonSignatureFormat.Field("Alignment", "Good")));
            template.Descriptor = "Summoning, Good";
            SummonSignature child = AlternateSummoningContracts.ExpectedAdapterChild(
                template, KmgWolf, AlternateSummoningChoiceKind.Creature,
                SummonFamily.Monster, true);
            Assertions.True(child.Spawns().All(value => value.Unit == KmgWolf &&
                value.AfterSpawn.Last() == AlternateSummoningContracts.ApplyTemplateType),
                "Every spawn names the creature and applies the dynamic template.");
            Assertions.True(child.Descriptor == "Summoning" && !child.CasterCheckers.Any(
                value => value.StartsWith(AlternateSummoningContracts.CasterAlignmentType)),
                "A dynamic template drops a fixed alignment restriction and descriptor.");
            SummonSignature changedPool = child.Clone();
            changedPool.Spawns().First().Pool = AlternateSummoningContracts.OccultistPool;
            SummonSignature extraAction = child.Clone();
            extraAction.Spawns().First().AfterSpawn.Add("Foreign.Action");
            SummonSignature lostEidolon = child.Clone();
            lostEidolon.CasterCheckers.RemoveAll(value => value.StartsWith(
                AlternateSummoningContracts.CompanionDeadOrPoolEmptyType));
            foreach (SummonSignature wrong in new[] { changedPool, extraAction, lostEidolon })
                Assertions.True(SummonSignature.Difference(child, wrong) != null,
                    "An undeliberate adapter-child change was accepted.");
            SummonSignature ally = AlternateSummoningContracts.ExpectedAdapterChild(
                NativeAllyChild(), KmgWolf, AlternateSummoningChoiceKind.Creature,
                SummonFamily.NaturesAlly, false);
            Assertions.True(ally.Spawns().All(value => value.AfterSpawn.Last() ==
                AlternateSummoningContracts.SetAlignmentType),
                "Nature's Ally children take caster alignment.");
            SummonSignature native = AlternateSummoningContracts.ExpectedAdapterChild(
                template, Dog, AlternateSummoningChoiceKind.NativeOption,
                SummonFamily.Monster, false);
            Assertions.True(native.Spawns().All(value => value.Unit == Dog &&
                !value.AfterSpawn.Contains(AlternateSummoningContracts.ApplyTemplateType)),
                "A retained native option changes only its unit.");
        }

        private static SummonSignature TacticianTemplate()
        {
            var signature = new SummonSignature {
                AbilityType = "SpellLike", ActionType = "Standard", Range = "Close",
                School = "Conjuration", Descriptor = "Summoning", RunActions = 1 };
            signature.Resources.Add(new SummonResource { Resource = TacticianResource,
                Spend = true, Amount = 1 });
            signature.Ranks.Add(new SummonRank { Type = "Default",
                BaseValueType = "ClassLevel", Progression = "AsIs",
                Classes = new List<string> { InquisitorClass }, Archetype = "null",
                Feature = "null", Stat = "Unknown", CustomProperty = "null",
                CustomProgression = "[]", BasedOnClassLevel = true });
            signature.Ranks.Add(new SummonRank { Type = "ProjectilesCount",
                BaseValueType = "FeatureRank", Progression = "AsIs", Archetype = "null",
                Feature = "0477936c0f74841498b5c8753a8062a3", Stat = "Unknown",
                CustomProperty = "null", CustomProgression = "[]" });
            signature.Actions.Add(SummonNode.ForClear(TacticianPool));
            signature.Actions.Add(SummonNode.ForSpawn(Spawn(Wolf, TacticianPool, "D3")));
            signature.Actions.Add(SummonNode.ForSpawn(Spawn(Dog, TacticianPool, "D3")));
            return signature;
        }

        private static SummonSignature Spawning(string first, string second)
        {
            var signature = new SummonSignature { RunActions = 1 };
            signature.Actions.Add(SummonNode.ForSpawn(Spawn(first, "null", "D3")));
            signature.Actions.Add(SummonNode.ForSpawn(Spawn(second, "null", "D3")));
            return signature;
        }

        private static SummonSignature NativeAllyChild()
        {
            var signature = new SummonSignature {
                AbilityType = "Spell", ActionType = "Standard", FullRoundField = true,
                Range = "Close", School = "Conjuration", Descriptor = "Summoning",
                SpellLists = 1, RunActions = 1 };
            signature.Ranks.Add(new SummonRank { Type = "Default",
                BaseValueType = "CasterLevel", Progression = "AsIs", Archetype = "null",
                Feature = "null", Stat = "Unknown", CustomProperty = "null",
                CustomProgression = "[]" });
            signature.Actions.Add(SummonNode.ForSpawn(Spawn(Wolf, "null", "Zero")));
            return signature;
        }

        private static SummonSpawn Spawn(string unit, string pool, string dice)
        {
            return new SummonSpawn { Unit = unit, Pool = pool, CountDice = dice,
                CountDiceCount = SummonValue.Simple(1), CountBonus = SummonValue.Simple(0),
                DurationRate = "Rounds", DurationDice = "Zero",
                DurationDiceCount = SummonValue.Simple(0),
                DurationBonus = SummonValue.RankOf("Default"), DurationExtendable = true,
                Level = SummonValue.Simple(0) };
        }
    }
}
