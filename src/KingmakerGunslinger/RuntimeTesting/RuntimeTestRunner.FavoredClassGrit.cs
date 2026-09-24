using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.FavoredClass;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private const string FcbHumanRace = "0a5d473ead98b0646b94495af250fdc4";

        // G07 vertical slice: a Human Gunslinger takes the grit favored-class
        // reward at every level 1..20 through real LevelUpController visits,
        // beside a lockstep control that takes the host's hit-point reward.
        private RuntimeTestResult RunFavoredClassGrit()
        {
            var assertions = new List<RuntimeTestAssertion>();
            var evidence = new JObject();
            FavoredClassIntegrationStatus status = FavoredClassIntegrationStatusRegistry.Current;
            FavoredClassHostHandles host = FavoredClassIntegrationCoordinator.Host;
            FavoredClassBlueprintSet leaves = BlueprintBootstrap.FavoredClassLeaves;
            GunslingerClassBlueprintSet gunslingerSet = BlueprintBootstrap.GunslingerClass;
            bool ready = status.Availability == FavoredClassIntegrationAvailability.Published &&
                host != null && host.GunslingerSelection != null && leaves != null &&
                gunslingerSet != null;
            assertions.Add(Assertion("fcb-integration-published",
                "the exact qualified host is ready and every eligible owned leaf is published",
                status.ToString(), ready, "FavoredClassIntegrationStatusRegistry after the first update"));
            if (!ready)
                return CreateResult(RuntimeTestStatuses.Fail, assertions, null);

            FavoredClassLeafPair grit = leaves.Pairs.Single(pair =>
                pair.Effect.Id == FavoredClassCatalog.EffectGrit);
            BlueprintCharacterClass gunslinger = gunslingerSet.CharacterClass;
            BlueprintAbilityResource gritResource = gunslingerSet.Grit.Resource;
            BlueprintFeatureSelection gunslingerSelection = host.GunslingerSelection;
            var reserved = new HashSet<string>(StringComparer.Ordinal) { gunslingerSelection.AssetGuid };
            BlueprintRace human = BlueprintLibraryLookup.RequireExact<BlueprintRace>(
                BlueprintBootstrap.Library, FcbHumanRace, "native Human");

            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);

            var levels = new JArray();
            var levelFailures = new List<string>();
            var refillFailures = new List<string>();
            var cancelFailures = new List<string>();
            JObject cancel = null;
            var raceMatrix = new JArray();
            var raceFailures = new List<string>();
            JObject fighterControl = null;
            var fighterFailures = new List<string>();
            int refillChecks = 0;
            bool progressionComplete = false;
            bool closedAfterTwenty = false;
            UnitEntityData test = null;
            UnitEntityData control = null;
            bool cleaned = false;
            try
            {
                test = FavoredClassLevelUpHarness.CreateUnit(14);
                control = FavoredClassLevelUpHarness.CreateUnit(14);
                for (int level = 1; level <= 20; level++)
                {
                    var row = new JObject { ["level"] = level };
                    int before = FavoredClassLevelUpHarness.Rank(test.Descriptor, grit.Full) +
                        FavoredClassLevelUpHarness.Rank(test.Descriptor, grit.Partial);
                    bool expectFull = FavoredClassRankPolicy.NextInvestmentIsFull(before, 4);
                    row["investmentsBefore"] = before;
                    row["expectFull"] = expectFull;
                    int spentBefore = -1;
                    if (expectFull && level > 1)
                    {
                        if (level == 4)
                            cancel = RunCancelledCompletingPick(test, human, gunslinger, grit,
                                gritResource, reserved, gunslingerSelection, cancelFailures);
                        if (test.Descriptor.Resources.GetResourceAmount(gritResource) > 0)
                            test.Descriptor.Resources.Spend(gritResource, 1);
                        spentBefore = test.Descriptor.Resources.GetResourceAmount(gritResource);
                        row["currentGritAfterSpend"] = spentBefore;
                    }
                    int testMaxBefore = gritResource.GetMaxAmount(test.Descriptor);
                    row["test"] = RunGritVisit(test, human, gunslinger, grit, gunslingerSelection,
                        reserved, level, expectFull ? grit.Full : grit.Partial, grit, levelFailures);
                    row["control"] = RunGritVisit(control, human, gunslinger, grit,
                        gunslingerSelection, reserved, level, host.GenericHitPoint, null, levelFailures);
                    int full = FavoredClassLevelUpHarness.Rank(test.Descriptor, grit.Full);
                    int partial = FavoredClassLevelUpHarness.Rank(test.Descriptor, grit.Partial);
                    int testMax = gritResource.GetMaxAmount(test.Descriptor);
                    int controlMax = gritResource.GetMaxAmount(control.Descriptor);
                    row["fullRank"] = full;
                    row["partialRank"] = partial;
                    row["testGritMax"] = testMax;
                    row["controlGritMax"] = controlMax;
                    row["gritDelta"] = testMax - controlMax;
                    row["controlGritRanks"] = FavoredClassLevelUpHarness.Rank(control.Descriptor, grit.Full) +
                        FavoredClassLevelUpHarness.Rank(control.Descriptor, grit.Partial);
                    if (full != level / 4 || partial != level - level / 4)
                        levelFailures.Add(Fmt("level {0}: full={1} partial={2}", level, full, partial));
                    if (testMax - controlMax != level / 4)
                        levelFailures.Add(Fmt("level {0}: grit delta={1} expected={2}", level,
                            testMax - controlMax, level / 4));
                    if ((int)row["controlGritRanks"] != 0)
                        levelFailures.Add(Fmt("level {0}: control holds grit investment", level));
                    if (spentBefore >= 0)
                    {
                        refillChecks++;
                        int after = test.Descriptor.Resources.GetResourceAmount(gritResource);
                        row["currentGritAfterLevelUp"] = after;
                        row["gritMaxBeforeLevelUp"] = testMaxBefore;
                        if (after != spentBefore)
                            refillFailures.Add(Fmt("level {0}: current {1} -> {2} after a completing pick",
                                level, spentBefore, after));
                        if (testMax != testMaxBefore + 1)
                            refillFailures.Add(Fmt("level {0}: max {1} -> {2}", level, testMaxBefore, testMax));
                    }
                    levels.Add(row);
                }
                int hpDelta = control.Descriptor.Stats.HitPoints.ModifiedValue -
                    test.Descriptor.Stats.HitPoints.ModifiedValue;
                evidence["hitPointDeltaControlMinusTest"] = hpDelta;
                if (hpDelta != 20)
                    levelFailures.Add("host hit-point reward delta=" + hpDelta + " expected 20");
                // The closure and refill claims are meaningful only after the
                // real progression reached level twenty with 5 full and 15
                // partial investments; otherwise they are not observed.
                progressionComplete = levels.Count == 20 &&
                    test.Descriptor.Progression.GetClassLevel(gunslinger) == 20 &&
                    FavoredClassLevelUpHarness.Rank(test.Descriptor, grit.Full) == 5 &&
                    FavoredClassLevelUpHarness.Rank(test.Descriptor, grit.Partial) == 15;
                bool openFull = grit.Full.MeetsPrerequisites(null, test.Descriptor, null);
                bool openPartial = grit.Partial.MeetsPrerequisites(null, test.Descriptor, null);
                closedAfterTwenty = progressionComplete && !openFull && !openPartial;
                evidence["progressionComplete"] = progressionComplete;
                evidence["fullOpenAfterTwenty"] = openFull;
                evidence["partialOpenAfterTwenty"] = openPartial;
                evidence["closedAfterTwenty"] = closedAfterTwenty;

                foreach (FavoredClassRaceIdentity identity in FavoredClassRaceIdentities.All)
                    raceMatrix.Add(RunGritRaceRow(identity, gunslinger, grit, gunslingerSelection,
                        reserved, raceFailures));
                fighterControl = RunFighterMenuControl(human, grit, host, fighterFailures);
            }
            catch (Exception exception)
            {
                levelFailures.Add("exception=" + exception);
            }
            finally
            {
                if (test != null) test.Dispose();
                if (control != null) control.Dispose();
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits));
            }

            if (levels.Count != 20)
                levelFailures.Add("level-rows=" + levels.Count);
            evidence["levels"] = levels;
            evidence["cancel"] = cancel;
            evidence["raceMatrix"] = raceMatrix;
            evidence["fighterControl"] = fighterControl;
            string evidencePath = WriteFavoredClassEvidence("favored-class-grit.json", evidence);

            assertions.Add(Assertion("fcb-grit-native-progression",
                "twenty native LevelUpController visits open exactly the policy leaf each level (full iff the investment is a multiple of four), bank floor(N/4) full and N - floor(N/4) partial ranks, and raise maximum grit by exactly floor(N/4) over a lockstep control that takes the host hit-point reward (20 HP more)",
                Describe(evidence, levelFailures), levelFailures.Count == 0,
                "LevelUpController.SelectFeature + LevelUpState.IsComplete gate + ApplyLevelup; BlueprintAbilityResource.GetMaxAmount"));
            assertions.Add(Assertion("fcb-grit-no-refill",
                "each completing pick raises maximum grit by one without refilling grit spent before the level-up",
                "checks=" + refillChecks + ";" + Describe(levels, refillFailures),
                refillChecks == 5 && refillFailures.Count == 0,
                "UnitAbilityResourceCollection.GetResourceAmount before and after ApplyLevelup"));
            assertions.Add(Assertion("fcb-grit-cancel-no-leak",
                "a completing pick selected in a cancelled visit leaves the real character's ranks and grit unchanged",
                Describe(cancel, cancelFailures), cancel != null && cancelFailures.Count == 0,
                "LevelUpController.Cancel after SelectFeature"));
            assertions.Add(Assertion("fcb-grit-closed-at-twenty",
                "after twenty investments no grit leaf is selectable",
                "progressionComplete=" + progressionComplete + ";closed=" + closedAfterTwenty,
                closedAfterTwenty,
                "native BlueprintFeature.MeetsPrerequisites on the committed unit"));
            assertions.Add(Assertion("fcb-grit-race-matrix",
                "for every present source-addressable race the Gunslinger menu offers grit exactly to Human, Half-elf, Half-orc, Aasimar, Tiefling, Hobgoblin and Fetchling; absent optional races are reported as provider absence",
                Describe(raceMatrix, raceFailures), raceFailures.Count == 0,
                "level-1 native visit per race; BlueprintFeatureSelection.CanSelect"));
            assertions.Add(Assertion("fcb-grit-other-class-menu",
                "a Human Fighter's favored-class menu never contains a Gunslinger grit leaf",
                Describe(fighterControl, fighterFailures), fighterFailures.Count == 0,
                "the host's Fighter bonus selection extracted items"));
            assertions.Add(Assertion("external-isolation", "unchanged party and global-unit snapshots",
                "cleaned=" + cleaned, cleaned, "detached entity disposal and exact reference snapshots"));
            assertions.Add(Assertion("loaded-mod-version", _request.ExpectedModVersion,
                _context.ModEntry.Info.Version,
                _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                "Unity Mod Manager ModEntry.Info.Version"));
            RuntimeTestResult result = CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions, null);
            result.EvidenceFiles.Add(evidencePath);
            return result;
        }

        private static JObject RunGritVisit(UnitEntityData unit, BlueprintRace race,
            BlueprintCharacterClass gunslinger, FavoredClassLeafPair grit,
            BlueprintFeatureSelection gunslingerSelection, ICollection<string> reserved, int level,
            BlueprintFeature choice, FavoredClassLeafPair tested, IList<string> failures)
        {
            var row = new JObject();
            LevelUpController controller = null;
            try
            {
                controller = FavoredClassLevelUpHarness.Open(unit.Descriptor, race, gunslinger,
                    "KMG FCB Grit");
                if (level == 1 && FavoredClassLevelUpHarness.ChooseFavoredClass(controller,
                        gunslinger, row) == null)
                    failures.Add("level 1: favored Gunslinger progression not selectable");
                row["fillBefore"] = FavoredClassLevelUpHarness.FillOthers(controller, reserved);
                FeatureSelectionState fcb = FavoredClassLevelUpHarness.FindOpenState(controller,
                    gunslingerSelection.AssetGuid);
                row["fcbStateOpen"] = fcb != null;
                if (fcb == null)
                {
                    failures.Add(Fmt("level {0}: no open Gunslinger favored-class selection", level));
                    return row;
                }
                bool canFull = FavoredClassLevelUpHarness.CanSelect(controller, fcb, grit.Full);
                bool canPartial = FavoredClassLevelUpHarness.CanSelect(controller, fcb, grit.Partial);
                row["canSelectFull"] = canFull;
                row["canSelectPartial"] = canPartial;
                if (tested != null)
                {
                    bool expectFull = ReferenceEquals(choice, grit.Full);
                    if (canFull != expectFull || canPartial == expectFull)
                        failures.Add(Fmt("level {0}: full={1} partial={2} expectFull={3}", level,
                            canFull, canPartial, expectFull));
                }
                else if (!canFull && !canPartial && level < 20)
                {
                    failures.Add(Fmt("level {0}: control saw no open grit leaf", level));
                }
                row["candidates"] = new JArray(FavoredClassLevelUpHarness.Items(controller, fcb)
                    .Select(item => item.Feature.name));
                if (!FavoredClassLevelUpHarness.Select(controller, fcb, choice))
                {
                    failures.Add(Fmt("level {0}: SelectFeature refused {1}", level, choice.name));
                    return row;
                }
                row["selected"] = choice.name;
                row["fillAfter"] = FavoredClassLevelUpHarness.FillOthers(controller, reserved);
                if (!FavoredClassLevelUpHarness.Confirm(controller, unit.Descriptor, row))
                    failures.Add(Fmt("level {0}: build incomplete: {1}", level, row["blockers"]));
                row["classLevel"] = unit.Descriptor.Progression.GetClassLevel(gunslinger);
                if ((int)row["classLevel"] != level)
                    failures.Add(Fmt("level {0}: class level {1}", level, row["classLevel"]));
                return row;
            }
            finally
            {
                FavoredClassLevelUpHarness.Close(controller);
            }
        }

        private JObject RunCancelledCompletingPick(UnitEntityData unit, BlueprintRace race,
            BlueprintCharacterClass gunslinger, FavoredClassLeafPair grit,
            BlueprintAbilityResource gritResource, ICollection<string> reserved,
            BlueprintFeatureSelection gunslingerSelection, IList<string> failures)
        {
            var row = new JObject();
            int full = FavoredClassLevelUpHarness.Rank(unit.Descriptor, grit.Full);
            int partial = FavoredClassLevelUpHarness.Rank(unit.Descriptor, grit.Partial);
            int max = gritResource.GetMaxAmount(unit.Descriptor);
            int current = unit.Descriptor.Resources.GetResourceAmount(gritResource);
            LevelUpController controller = null;
            try
            {
                controller = FavoredClassLevelUpHarness.Open(unit.Descriptor, race, gunslinger,
                    "KMG FCB Grit");
                FavoredClassLevelUpHarness.FillOthers(controller, reserved);
                FeatureSelectionState fcb = FavoredClassLevelUpHarness.FindOpenState(controller,
                    gunslingerSelection.AssetGuid);
                bool selected = fcb != null &&
                    FavoredClassLevelUpHarness.Select(controller, fcb, grit.Full);
                row["previewSelected"] = selected;
                row["previewFullRank"] = FavoredClassLevelUpHarness.Rank(controller.Preview, grit.Full);
                row["previewGritMax"] = gritResource.GetMaxAmount(controller.Preview);
                if (!selected)
                    failures.Add("the completing pick could not be selected in the preview");
            }
            finally
            {
                FavoredClassLevelUpHarness.Close(controller);
            }
            row["unitFullRank"] = FavoredClassLevelUpHarness.Rank(unit.Descriptor, grit.Full);
            row["unitPartialRank"] = FavoredClassLevelUpHarness.Rank(unit.Descriptor, grit.Partial);
            row["unitGritMax"] = gritResource.GetMaxAmount(unit.Descriptor);
            row["unitGritCurrent"] = unit.Descriptor.Resources.GetResourceAmount(gritResource);
            if ((int)row["unitFullRank"] != full || (int)row["unitPartialRank"] != partial ||
                (int)row["unitGritMax"] != max || (int)row["unitGritCurrent"] != current)
                failures.Add("the cancelled visit changed the real character: " + row);
            if ((int)row["previewFullRank"] != full + 1)
                failures.Add("the preview did not hold the completing pick");
            return row;
        }

        private JObject RunGritRaceRow(FavoredClassRaceIdentity identity,
            BlueprintCharacterClass gunslinger, FavoredClassLeafPair grit,
            BlueprintFeatureSelection gunslingerSelection, ICollection<string> reserved,
            IList<string> failures)
        {
            var row = new JObject { ["ancestry"] = identity.Ancestry, ["race"] = identity.RaceGuid };
            BlueprintScriptableObject resolved;
            BlueprintBootstrap.Library.BlueprintsByAssetId.TryGetValue(identity.RaceGuid, out resolved);
            var race = resolved as BlueprintRace;
            bool expected = new[] { FavoredClassAncestry.Human, FavoredClassAncestry.HalfElf,
                    FavoredClassAncestry.HalfOrc, FavoredClassAncestry.Aasimar,
                    FavoredClassAncestry.Tiefling, FavoredClassAncestry.Hobgoblin,
                    FavoredClassAncestry.Fetchling }.Contains(identity.Ancestry);
            row["expectedGrit"] = expected;
            if (race == null)
            {
                row["disposition"] = "EXPECTED PROVIDER ABSENCE";
                if (identity.Provider != FavoredClassRaceProvider.Optional)
                    failures.Add(identity.Ancestry + ": non-optional race identity is missing");
                return row;
            }
            bool playable = Kingmaker.Blueprints.Root.BlueprintRoot.Instance.Progression
                .CharacterRaces.Contains(race);
            row["playable"] = playable;
            if (!playable)
            {
                // An NPC-only race blueprint cannot be chosen at character
                // creation; its route is recorded, never scored as offered.
                row["disposition"] = "PRESENT BUT NOT PLAYABLE";
                if (identity.Provider != FavoredClassRaceProvider.Optional)
                    failures.Add(identity.Ancestry + ": native or KMG race is not playable");
                return row;
            }
            UnitEntityData unit = null;
            LevelUpController controller = null;
            try
            {
                unit = FavoredClassLevelUpHarness.CreateUnit(14);
                controller = FavoredClassLevelUpHarness.Open(unit.Descriptor, race, gunslinger,
                    "KMG FCB Race " + identity.Ancestry);
                row["raceApplied"] = controller.Preview.Progression.Race == race;
                if (FavoredClassLevelUpHarness.ChooseFavoredClass(controller, gunslinger, row) == null)
                {
                    failures.Add(identity.Ancestry + ": favored Gunslinger progression unavailable");
                    return row;
                }
                FavoredClassLevelUpHarness.FillOthers(controller, reserved);
                FeatureSelectionState fcb = FavoredClassLevelUpHarness.FindOpenState(controller,
                    gunslingerSelection.AssetGuid);
                if (fcb == null)
                {
                    failures.Add(identity.Ancestry + ": no Gunslinger favored-class state");
                    return row;
                }
                bool offered = FavoredClassLevelUpHarness.CanSelect(controller, fcb, grit.Partial) ||
                    FavoredClassLevelUpHarness.CanSelect(controller, fcb, grit.Full);
                row["gritOffered"] = offered;
                row["gritItemCount"] = FavoredClassLevelUpHarness.Items(controller, fcb)
                    .Count(item => ReferenceEquals(item.Feature, grit.Full) ||
                        ReferenceEquals(item.Feature, grit.Partial));
                row["hiddenWhenUnavailable"] = grit.Full.HideNotAvailibleInUI &&
                    grit.Partial.HideNotAvailibleInUI;
                if (offered != expected)
                    failures.Add(Fmt("{0}: grit offered={1} expected={2}", identity.Ancestry, offered,
                        expected));
                if ((int)row["gritItemCount"] != 2)
                    failures.Add(identity.Ancestry + ": grit leaves not exactly once each in the menu");
                return row;
            }
            catch (Exception exception)
            {
                row["exception"] = exception.GetType().Name + ": " + exception.Message;
                failures.Add(identity.Ancestry + ": " + row["exception"]);
                return row;
            }
            finally
            {
                FavoredClassLevelUpHarness.Close(controller);
                if (unit != null) unit.Dispose();
            }
        }

        private static JObject RunFighterMenuControl(BlueprintRace human, FavoredClassLeafPair grit,
            FavoredClassHostHandles host, IList<string> failures)
        {
            var row = new JObject();
            BlueprintCharacterClass fighter = BlueprintBootstrap.Library.GetAllBlueprints()
                .OfType<BlueprintCharacterClass>().SingleOrDefault(value => value.name == "FighterClass");
            BlueprintFeatureSelection fighterSelection = fighter == null ? null :
                host.BonusSelectionFor(fighter.AssetGuid);
            row["fighterSelection"] = fighterSelection == null ? "<absent>" : fighterSelection.name;
            if (fighterSelection == null)
            {
                failures.Add("the host Fighter bonus selection is unavailable");
                return row;
            }
            bool contains = fighterSelection.AllFeatures.Contains(grit.Full) ||
                fighterSelection.AllFeatures.Contains(grit.Partial);
            row["containsGrit"] = contains;
            if (contains)
                failures.Add("the Fighter favored-class menu contains a grit leaf");
            return row;
        }

        private string WriteFavoredClassEvidence(string fileName, JObject evidence)
        {
            string path = System.IO.Path.Combine(_request.EvidenceDirectory, fileName);
            System.IO.File.WriteAllText(path, evidence.ToString(Newtonsoft.Json.Formatting.Indented),
                new System.Text.UTF8Encoding(false));
            return path;
        }

        private static string Fmt(string format, params object[] values)
        {
            return string.Format(CultureInfo.InvariantCulture, format, values);
        }
    }
}
