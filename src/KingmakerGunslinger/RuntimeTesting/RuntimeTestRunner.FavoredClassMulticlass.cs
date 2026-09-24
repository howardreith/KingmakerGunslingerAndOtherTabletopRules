using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Class.LevelUp;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.FavoredClass;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private const string FcbFighterClassGuid = "48ac8db94d5de7645906c7d0ad3bcfbd";
        private const string FcbMultitalentedGuid = "e6a72a23e75545bc9a57a6b94ffc8b69";
        private const string FcbHostHitPointRewardGuid = "c16eded5cc1948faab43177135fc7845";

        // E11: ordinary and Half-elf (Multitalented) multiclass progressions.
        // Native level-up visits: only a favored class's own levels open that
        // class's host reward selection, exactly once per level, and KMG
        // counters appear only in the Gunslinger's reward selection.
        private RuntimeTestResult RunFavoredClassMulticlass()
        {
            var assertions = new List<RuntimeTestAssertion>();
            FavoredClassHostHandles host = FavoredClassIntegrationCoordinator.Host;
            FavoredClassBlueprintSet leaves = BlueprintBootstrap.FavoredClassLeaves;
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            bool ready = FavoredClassIntegrationStatusRegistry.Current.Availability ==
                    FavoredClassIntegrationAvailability.Published && host != null && leaves != null &&
                gunslinger != null && host.GunslingerSelection != null;
            assertions.Add(Assertion("fcb-multiclass-ready", "the exact host is published with the Gunslinger reward selection",
                FavoredClassIntegrationStatusRegistry.Current.ToString(), ready, "FavoredClassIntegrationStatusRegistry"));
            if (!ready)
                return CreateResult(RuntimeTestStatuses.Fail, assertions, null);
            object[] partyBefore = SnapshotReferences(ReadExactMember(ReadExactMember(Game.Instance, "Player"), "Party"));
            object[] unitsBefore = SnapshotReferences(ReadExactMember(ReadExactMember(Game.Instance, "State"), "AllUnits"));
            var library = BlueprintBootstrap.Library;
            BlueprintCharacterClass fighter = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(library,
                FcbFighterClassGuid, "Fighter");
            BlueprintCharacterClass rogue = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(library,
                FcbRogueClassGuid, "Rogue");
            var classes = new[] { gunslinger.CharacterClass, fighter, rogue };
            var rewardByClass = classes.ToDictionary(value => value, value => host.BonusSelectionFor(value.AssetGuid));
            var failures = new List<string>();
            var evidence = new JObject();
            if (rewardByClass.Values.Any(value => value == null))
                failures.Add("a host reward selection is missing: " + string.Join(",", rewardByClass
                    .Where(pair => pair.Value == null).Select(pair => pair.Key.name).ToArray()));
            else
            {
                var owned = new HashSet<BlueprintFeature>(leaves.Pairs.SelectMany(pair => pair.Leaves));
                evidence["ordinary"] = RunFcbMulticlassCase("Human", FavoredClassAncestry.Human, null,
                    new[] { gunslinger.CharacterClass, fighter, gunslinger.CharacterClass, rogue },
                    new[] { gunslinger.CharacterClass }, rewardByClass, leaves, owned, failures);
                evidence["halfElf"] = RunFcbMulticlassCase("Half-elf", FavoredClassAncestry.HalfElf, fighter,
                    new[] { gunslinger.CharacterClass, fighter, rogue, gunslinger.CharacterClass, fighter },
                    new[] { gunslinger.CharacterClass, fighter }, rewardByClass, leaves, owned, failures);
            }
            bool cleaned = SameReferences(partyBefore, SnapshotReferences(ReadExactMember(
                    ReadExactMember(Game.Instance, "Player"), "Party"))) &&
                SameReferences(unitsBefore, SnapshotReferences(ReadExactMember(
                    ReadExactMember(Game.Instance, "State"), "AllUnits")));
            string evidencePath = WriteFavoredClassEvidence("favored-class-multiclass.json", evidence);
            assertions.Add(Assertion("fcb-multiclass-rewards",
                "each level of a favored class opens exactly its own host reward selection once and a pick closes it; levels of other classes open none; a Multitalented Half-elf earns rewards on both chosen classes and nowhere else; KMG counters appear only in the Gunslinger reward selection",
                Describe(evidence, failures), failures.Count == 0,
                "native level-up visits: LevelUpState.Selections of the host reward selections"));
            assertions.Add(Assertion("external-isolation", "unchanged party and global-unit snapshots",
                "cleaned=" + cleaned, cleaned, "detached unit disposal and exact reference snapshots"));
            assertions.Add(Assertion("loaded-mod-version", _request.ExpectedModVersion, _context.ModEntry.Info.Version,
                _request.ExpectedModVersion == _context.ModEntry.Info.Version, "Unity Mod Manager ModEntry.Info.Version"));
            RuntimeTestResult result = CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions, null);
            result.EvidenceFiles.Add(evidencePath);
            return result;
        }

        private JArray RunFcbMulticlassCase(string label, string ancestry, BlueprintCharacterClass multitalented,
            BlueprintCharacterClass[] visits, BlueprintCharacterClass[] favored,
            Dictionary<BlueprintCharacterClass, BlueprintFeatureSelection> rewardByClass,
            FavoredClassBlueprintSet leaves, HashSet<BlueprintFeature> owned, IList<string> failures)
        {
            var library = BlueprintBootstrap.Library;
            BlueprintRace race = BlueprintLibraryLookup.RequireExact<BlueprintRace>(library,
                FavoredClassRaceIdentities.ForAncestry(ancestry).RaceGuid, ancestry);
            BlueprintFeature hitPoint = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library,
                FcbHostHitPointRewardGuid, "host favored-class hit point");
            FavoredClassLeafPair grit = leaves.Pair(FavoredClassCatalog.EffectGrit, null);
            var reserved = new HashSet<string>(rewardByClass.Values.Select(value => value.AssetGuid),
                StringComparer.Ordinal);
            var rows = new JArray();
            UnitEntityData unit = FavoredClassLevelUpHarness.CreateUnit(14);
            var expectedTaken = new Dictionary<BlueprintCharacterClass, int>();
            try
            {
                for (int index = 0; index < visits.Length; index++)
                {
                    BlueprintCharacterClass visit = visits[index];
                    var row = new JObject { ["visit"] = index + 1, ["class"] = visit.name };
                    LevelUpController controller = null;
                    try
                    {
                        controller = FavoredClassLevelUpHarness.Open(unit.Descriptor, race, visit,
                            "KMG FCB Multiclass " + label);
                        if (index == 0)
                        {
                            if (FavoredClassLevelUpHarness.ChooseFavoredClass(controller, favored[0], row) == null)
                                throw new InvalidOperationException("the favored class could not be chosen");
                            if (multitalented != null)
                            {
                                FeatureSelectionState second = FavoredClassLevelUpHarness.FindOpenState(controller,
                                    FcbMultitalentedGuid);
                                row["multitalentedOpen"] = second != null;
                                IFeatureSelectionItem choice = second == null ? null :
                                    FavoredClassLevelUpHarness.Items(controller, second).FirstOrDefault(item =>
                                        item.Feature is BlueprintProgression &&
                                        ((BlueprintProgression)item.Feature).Classes != null &&
                                        ((BlueprintProgression)item.Feature).Classes.Length == 1 &&
                                        ReferenceEquals(((BlueprintProgression)item.Feature).Classes[0], multitalented));
                                if (choice == null || !controller.SelectFeature(second, choice))
                                    throw new InvalidOperationException("Multitalented could not choose " +
                                        multitalented.name);
                                row["multitalented"] = choice.Feature.name;
                            }
                        }
                        FavoredClassLevelUpHarness.FillOthers(controller, reserved);
                        var open = new JObject();
                        foreach (KeyValuePair<BlueprintCharacterClass, BlueprintFeatureSelection> pair in rewardByClass)
                            open[pair.Key.name] = controller.State.Selections.Count(value => !value.Selected &&
                                ReferenceEquals(value.Selection, pair.Value));
                        row["openRewardStates"] = open;
                        bool rewarded = favored.Contains(visit);
                        foreach (KeyValuePair<BlueprintCharacterClass, BlueprintFeatureSelection> pair in rewardByClass)
                        {
                            int expected = rewarded && ReferenceEquals(pair.Key, visit) ? 1 : 0;
                            if ((int)open[pair.Key.name] != expected)
                                failures.Add(label + " visit " + (index + 1) + " (" + visit.name + "): " + pair.Key.name +
                                    " reward states " + open[pair.Key.name] + ", expected " + expected);
                        }
                        if (rewarded)
                        {
                            FeatureSelectionState state = FavoredClassLevelUpHarness.FindOpenState(controller,
                                rewardByClass[visit].AssetGuid);
                            if (state == null)
                                throw new InvalidOperationException("the favored level has no open reward state");
                            string[] kmg = FavoredClassLevelUpHarness.Items(controller, state)
                                .Where(item => owned.Contains(item.Feature)).Select(item => item.Feature.name).ToArray();
                            row["kmgItems"] = kmg.Length;
                            bool gunslingerReward = ReferenceEquals(visit, BlueprintBootstrap.GunslingerClass.CharacterClass);
                            if (!gunslingerReward && kmg.Length != 0)
                                failures.Add(label + " visit " + (index + 1) + ": " + visit.name +
                                    " reward selection lists KMG counters " + string.Join(",", kmg));
                            BlueprintFeature pick = gunslingerReward && ancestry == FavoredClassAncestry.Human ?
                                grit.Partial : hitPoint;
                            if (!FavoredClassLevelUpHarness.Select(controller, state, pick))
                                throw new InvalidOperationException("the reward " + pick.name + " could not be taken");
                            row["pick"] = pick.name;
                            int taken;
                            expectedTaken.TryGetValue(visit, out taken);
                            expectedTaken[visit] = taken + 1;
                            int remaining = controller.State.Selections.Count(value => !value.Selected &&
                                rewardByClass.Values.Contains(value.Selection as BlueprintFeatureSelection));
                            row["openAfterPick"] = remaining;
                            if (remaining != 0)
                                failures.Add(label + " visit " + (index + 1) + ": a second reward stayed open");
                        }
                        FavoredClassLevelUpHarness.FillOthers(controller, reserved);
                        if (!FavoredClassLevelUpHarness.Confirm(controller, unit.Descriptor, row))
                            throw new InvalidOperationException("the level is incomplete: " + row["completion"]);
                    }
                    catch (Exception exception)
                    {
                        failures.Add(label + " visit " + (index + 1) + ": " + exception.GetType().Name + ": " +
                            exception.Message);
                        rows.Add(row);
                        break;
                    }
                    finally
                    {
                        FavoredClassLevelUpHarness.Close(controller);
                    }
                    rows.Add(row);
                }
                var levels = new JObject();
                foreach (BlueprintCharacterClass value in visits.Distinct())
                    levels[value.name] = unit.Descriptor.Progression.GetClassLevel(value);
                int gritRank = FavoredClassLevelUpHarness.Rank(unit.Descriptor, grit.Partial);
                int hitPoints = FavoredClassLevelUpHarness.Rank(unit.Descriptor, hitPoint);
                int gunslingerTaken, others = 0;
                expectedTaken.TryGetValue(BlueprintBootstrap.GunslingerClass.CharacterClass, out gunslingerTaken);
                foreach (KeyValuePair<BlueprintCharacterClass, int> pair in expectedTaken)
                    if (!ReferenceEquals(pair.Key, BlueprintBootstrap.GunslingerClass.CharacterClass))
                        others += pair.Value;
                rows.Add(new JObject { ["classLevels"] = levels, ["gritPartialRank"] = gritRank,
                    ["hostHitPointRank"] = hitPoints });
                int expectedGrit = ancestry == FavoredClassAncestry.Human ? gunslingerTaken : 0;
                int expectedHitPoints = (ancestry == FavoredClassAncestry.Human ? 0 : gunslingerTaken) + others;
                if (gritRank != expectedGrit || hitPoints != expectedHitPoints)
                    failures.Add(label + ": committed rewards grit=" + gritRank + " hp=" + hitPoints +
                        ", expected grit=" + expectedGrit + " hp=" + expectedHitPoints);
            }
            finally
            {
                unit.Dispose();
            }
            return rows;
        }
    }
}
