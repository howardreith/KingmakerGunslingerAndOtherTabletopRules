using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Deeds;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// RISK-04 / S4 T6: whether True Grit's two level-20 choices can commit the
    /// same deed, observed through the real LevelUpController. A disposable
    /// Gunslinger is advanced to 19 natively; the level-20 visit offers two
    /// states of the one registered True Grit selection. Each step records
    /// what the engine offers, accepts and holds, and checks after every step
    /// that no deed occupies both states and no preview rank exceeds one.
    /// Cases: cancellation, an injected failure inside a visit, the duplicate
    /// attempts with back and replacement before a gated commit, then a
    /// cancelled and a committed native respec.
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        private const string TrueGritHumanRaceGuid = "0a5d473ead98b0646b94495af250fdc4";

        private RuntimeTestResult RunTrueGritLevelUpTransaction()
        {
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            if (gunslinger == null || gunslinger.TrueGrit == null || gunslinger.Grit == null)
                throw new InvalidOperationException("The True Grit fixture inputs are incomplete.");
            TrueGritBlueprintSet set = gunslinger.TrueGrit;
            object player = ReadExactMember(Game.Instance, "Player");
            object state = ReadExactMember(Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            var shape = new List<string>();
            var cancelFailures = new List<string>();
            var flowFailures = new List<string>();
            var respecFailures = new List<string>();
            var evidence = new JObject();
            bool cleaned = false;
            string stage = "fixture";
            UnitEntityData unit = null;
            object advance = null;
            try
            {
                int levelEntries = gunslinger.Progression.LevelEntries[19].Features
                    .Count(value => ReferenceEquals(value, set.Selection));
                evidence["shape"] = new JObject
                {
                    ["selection"] = set.Selection.AssetGuid, ["selectionRanks"] = set.Selection.Ranks,
                    ["choices"] = set.Choices.Length, ["levelTwentyEntries"] = levelEntries,
                    ["oneRankChoices"] = set.Choices.Count(value => value.Ranks == 1),
                    ["ignorePrerequisites"] = set.Selection.IgnorePrerequisites
                };
                if (levelEntries != 2) shape.Add("level 20 grants the selection " + levelEntries + " times");
                if (set.Choices.Any(value => value.Ranks != 1)) shape.Add("a True Grit choice has more than one rank");

                unit = FavoredClassLevelUpHarness.CreateUnit(14);
                AdvanceDisposableGunslinger(unit.Descriptor, gunslinger, 19, ref advance);
                JObject before = DescribeTrueGritState(unit.Descriptor, gunslinger);
                evidence["before"] = before;
                if ((int)before["gunslingerLevel"] != 19 || (int)before["trueGritFacts"] != 0)
                    shape.Add("fixture is not a level-19 Gunslinger without True Grit: " + TrueGritCompact(before));

                stage = "cancel";
                evidence["cancel"] = RunTrueGritVisit(unit.Descriptor, gunslinger, cancelFailures, (controller, row) =>
                {
                    TrueGritPick picks = TrueGritPicks(controller, set, row);
                    row["firstAccepted"] = SelectTrueGrit(controller, set, 0, picks.A);
                    row["secondAccepted"] = SelectTrueGrit(controller, set, 1, picks.B);
                    row["held"] = DescribeTrueGritSlots(controller, set);
                    return false;
                });
                JObject afterCancel = DescribeTrueGritState(unit.Descriptor, gunslinger);
                evidence["afterCancel"] = afterCancel;
                if (!JToken.DeepEquals(before, afterCancel))
                    cancelFailures.Add("the cancelled visit changed the character: " + TrueGritCompact(afterCancel));

                stage = "injected-failure";
                evidence["injectedFailure"] = RunTrueGritVisit(unit.Descriptor, gunslinger, cancelFailures, (controller, row) =>
                {
                    TrueGritPick picks = TrueGritPicks(controller, set, row);
                    row["firstAccepted"] = SelectTrueGrit(controller, set, 0, picks.A);
                    row["secondAccepted"] = SelectTrueGrit(controller, set, 1, picks.B);
                    throw new InvalidOperationException("injected True Grit fixture failure after both choices were held");
                }, expectInjected: true);
                JObject afterInjected = DescribeTrueGritState(unit.Descriptor, gunslinger);
                evidence["afterInjectedFailure"] = afterInjected;
                if (!JToken.DeepEquals(before, afterInjected))
                    cancelFailures.Add("the failed visit changed the character: " + TrueGritCompact(afterInjected));

                stage = "duplicate-attempts-and-commit";
                TrueGritPick committed = null;
                JObject commit = RunTrueGritVisit(unit.Descriptor, gunslinger, flowFailures, (controller, row) =>
                {
                    row["filled"] = FavoredClassLevelUpHarness.FillOthers(controller,
                        new HashSet<string>(StringComparer.Ordinal) { set.Selection.AssetGuid });
                    TrueGritPick picks = TrueGritPicks(controller, set, row);
                    var steps = new JArray();
                    row["steps"] = steps;
                    TrueGritStep(controller, set, steps, flowFailures, "first: A into state 0",
                        () => SelectTrueGrit(controller, set, 0, picks.A), true);
                    TrueGritStep(controller, set, steps, flowFailures, "duplicate: A into state 1",
                        () => SelectTrueGrit(controller, set, 1, picks.A), false, offered: () => OffersTrueGrit(controller, set, 1, picks.A));
                    TrueGritStep(controller, set, steps, flowFailures, "second: B into state 1",
                        () => SelectTrueGrit(controller, set, 1, picks.B), true);
                    TrueGritStep(controller, set, steps, flowFailures, "re-select without back: B over A in state 0",
                        () => SelectTrueGrit(controller, set, 0, picks.B), false, offered: () => OffersTrueGrit(controller, set, 0, picks.B));
                    TrueGritStep(controller, set, steps, flowFailures, "back: clear state 0",
                        () => UnselectTrueGrit(controller, set, 0), true);
                    TrueGritStep(controller, set, steps, flowFailures, "duplicate after back: B into state 0",
                        () => SelectTrueGrit(controller, set, 0, picks.B), false, offered: () => OffersTrueGrit(controller, set, 0, picks.B));
                    TrueGritStep(controller, set, steps, flowFailures, "replacement: C into state 0",
                        () => SelectTrueGrit(controller, set, 0, picks.C), true);
                    TrueGritStep(controller, set, steps, flowFailures, "back: clear state 1",
                        () => UnselectTrueGrit(controller, set, 1), true);
                    TrueGritStep(controller, set, steps, flowFailures, "replacement: freed A into state 1",
                        () => SelectTrueGrit(controller, set, 1, picks.A), true);
                    row["final"] = DescribeTrueGritSlots(controller, set);
                    committed = new TrueGritPick { A = picks.C, B = picks.A };
                    return true;
                });
                evidence["commit"] = commit;
                JObject afterCommit = DescribeTrueGritState(unit.Descriptor, gunslinger);
                evidence["afterCommit"] = afterCommit;
                if (!(bool)commit["applied"])
                    flowFailures.Add("the level-20 visit did not reach a gated commit: " + TrueGritCompact(commit["confirmation"]));
                else if (committed != null)
                {
                    JObject ranks = (JObject)afterCommit["ranks"];
                    if ((int)afterCommit["gunslingerLevel"] != 20 || (int)afterCommit["trueGritFacts"] != 2 ||
                        ranks.Count != 2 || (int?)ranks[committed.A.ToString()] != 1 ||
                        (int?)ranks[committed.B.ToString()] != 1)
                        flowFailures.Add("the committed facts are not exactly the two final deeds: " + TrueGritCompact(afterCommit));
                    TrueGritDecision kept = TrueGritRuntime.Evaluate(unit.Descriptor, committed.B, 1, false);
                    TrueGritDecision first = TrueGritRuntime.Evaluate(unit.Descriptor, committed.A, 1, false);
                    afterCommit["chosenDeedCosts"] = first.EffectiveCost + "," + kept.EffectiveCost;
                    if (first.EffectiveCost != 0 || kept.EffectiveCost != 0)
                        flowFailures.Add("a committed True Grit deed is not reduced: " + TrueGritCompact(afterCommit));
                }

                stage = "respec-cancel";
                BlueprintRace human = BlueprintLibraryLookup.RequireExact<BlueprintRace>(
                    BlueprintBootstrap.Library, TrueGritHumanRaceGuid, "native Human");
                JObject respecCancel = RunFcbRespec(unit, (controller, row) =>
                {
                    FavoredClassLevelUpHarness.Configure(controller, controller.Unit, human,
                        gunslinger.CharacterClass, "KMG True Grit Respec", null);
                    row["previewTrueGrit"] = DescribeTrueGritState(controller.Preview, gunslinger);
                    return false;
                }, respecFailures, "respec-cancel");
                JObject afterRespecCancel = DescribeTrueGritState(unit.Descriptor, gunslinger);
                respecCancel["after"] = afterRespecCancel;
                evidence["respecCancel"] = respecCancel;
                if (!(bool)respecCancel["invoked"] || (bool)respecCancel["committed"])
                    respecFailures.Add("the cancelled respec did not stop before commit: " + TrueGritCompact(respecCancel));
                if (!JToken.DeepEquals(afterCommit, afterRespecCancel))
                    respecFailures.Add("the cancelled respec changed the character: " + TrueGritCompact(afterRespecCancel));

                stage = "respec-commit";
                JObject respecCommit = RunFcbRespec(unit, (controller, row) =>
                {
                    FavoredClassLevelUpHarness.Configure(controller, controller.Unit, human,
                        gunslinger.CharacterClass, "KMG True Grit Respec", null);
                    FavoredClassLevelUpHarness.ChooseFavoredClass(controller, gunslinger.CharacterClass, row);
                    row["filled"] = FavoredClassLevelUpHarness.FillOthers(controller, null);
                    row["trueGritStatesOffered"] = TrueGritStates(controller, set).Length;
                    bool complete = controller.State.IsComplete();
                    row["isComplete"] = complete;
                    if (!complete) row["completion"] = FavoredClassLevelUpHarness.Completion(controller);
                    return complete;
                }, respecFailures, "respec-commit");
                JObject afterRespec = DescribeTrueGritState(unit.Descriptor, gunslinger);
                respecCommit["after"] = afterRespec;
                evidence["respecCommit"] = respecCommit;
                if (!(bool)respecCommit["committed"])
                    respecFailures.Add("the respec did not commit: " + TrueGritCompact(respecCommit));
                else if ((int)afterRespec["trueGritFacts"] != 0 || (int)afterRespec["gunslingerLevel"] != 1)
                    respecFailures.Add("the respecced character kept True Grit or did not restart at Gunslinger 1: " + TrueGritCompact(afterRespec));
            }
            catch (Exception exception)
            {
                flowFailures.Add("exception at " + stage + ": " + exception.GetType().Name + ": " + exception.Message);
            }
            finally
            {
                Exception advanceCleanup = TryCancelRapidReloadVisit(advance as LevelUpController);
                if (advanceCleanup != null) flowFailures.Add("advance cleanup: " + advanceCleanup.Message);
                if (unit != null) unit.Dispose();
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits));
            }

            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("true-grit-selection-shape",
                    "level 20 grants the one True Grit selection twice and every choice has one rank",
                    TrueGritCompact(evidence["shape"]) + TrueGritFailures(shape), shape.Count == 0,
                    "registered progression and selection blueprints"),
                Assertion("true-grit-cancel-and-failure-restore",
                    "a cancelled level-20 visit holding two deeds, and a visit that throws after holding two, leave the level-19 character, its facts and grit exactly as before",
                    TrueGritCompact(new JObject { ["cancel"] = evidence["cancel"], ["injectedFailure"] = evidence["injectedFailure"] }) +
                        TrueGritFailures(cancelFailures), cancelFailures.Count == 0,
                    "native LevelUpController.Cancel through the guarded visit"),
                Assertion("true-grit-distinct-through-level-up",
                    "through the real level-up controller the deed held by one True Grit state is neither offered nor accepted by the other, before or after back navigation; replacement and freed choices work; the gated commit leaves exactly the two final deeds, each rank one and each reduced",
                    TrueGritCompact(evidence["commit"]) + ";after=" + TrueGritCompact(evidence["afterCommit"]) + TrueGritFailures(flowFailures),
                    flowFailures.Count == 0,
                    "LevelUpController.SelectFeature / UnselectFeature, BlueprintFeatureSelection.CanSelect, LevelUpState.IsComplete then ApplyLevelup"),
                Assertion("true-grit-respec",
                    "a cancelled native respec leaves both deeds; a committed one restarts the character at Gunslinger 1 with no True Grit fact",
                    TrueGritCompact(new JObject { ["cancel"] = evidence["respecCancel"], ["commit"] = evidence["respecCommit"] }) +
                        TrueGritFailures(respecFailures), respecFailures.Count == 0,
                    "Player.RespecCompanion through the native level-up start event"),
                Assertion("external-isolation", "unchanged party and global-unit snapshots",
                    "cleaned=" + cleaned, cleaned, "detached entity disposal and exact reference snapshots"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions, null);
        }

        private sealed class TrueGritPick
        {
            internal TrueGritDeed A, B, C;
        }

        /// <summary>One guarded level-20 visit: opened, driven, applied only when complete, always cancelled.</summary>
        private static JObject RunTrueGritVisit(UnitDescriptor unit, GunslingerClassBlueprintSet gunslinger,
            IList<string> failures, Func<LevelUpController, JObject, bool> drive, bool expectInjected = false)
        {
            var row = new JObject { ["applied"] = false };
            LevelUpController controller = null;
            try
            {
                controller = FavoredClassLevelUpHarness.OpenBare(unit);
                FavoredClassLevelUpHarness.Configure(controller, unit, null, gunslinger.CharacterClass,
                    "KMG True Grit Fixture", null);
                row["trueGritStates"] = TrueGritStates(controller, gunslinger.TrueGrit).Length;
                bool apply = drive(controller, row);
                if (apply)
                {
                    var confirmation = new JObject();
                    row["confirmation"] = confirmation;
                    row["applied"] = FavoredClassLevelUpHarness.Confirm(controller, unit, confirmation);
                }
            }
            catch (InvalidOperationException injected) when (expectInjected &&
                injected.Message.StartsWith("injected True Grit fixture failure", StringComparison.Ordinal))
            {
                row["injectedFailureCaught"] = injected.Message;
            }
            catch (Exception exception)
            {
                failures.Add("visit failed: " + exception.GetType().Name + ": " + exception.Message);
                row["error"] = exception.GetType().Name + ": " + exception.Message;
            }
            finally
            {
                Exception cleanup = TryCancelRapidReloadVisit(controller);
                row["cancelled"] = controller != null && cleanup == null;
                if (cleanup != null) failures.Add("visit cleanup: " + cleanup.Message);
            }
            if (expectInjected && row["injectedFailureCaught"] == null)
                failures.Add("the injected failure did not occur inside the visit");
            return row;
        }

        private static FeatureSelectionState[] TrueGritStates(LevelUpController controller, TrueGritBlueprintSet set)
        {
            return controller.State.Selections.Where(value => ReferenceEquals(value.Selection, set.Selection))
                .OrderBy(value => value.Index).ToArray();
        }

        private static FeatureSelectionState TrueGritState(LevelUpController controller, TrueGritBlueprintSet set, int ordinal)
        {
            FeatureSelectionState[] states = TrueGritStates(controller, set);
            if (states.Length != 2)
                throw new InvalidOperationException("the level-20 visit offers " + states.Length + " True Grit states");
            return states[ordinal];
        }

        private static bool SelectTrueGrit(LevelUpController controller, TrueGritBlueprintSet set, int ordinal, TrueGritDeed deed)
        {
            return FavoredClassLevelUpHarness.Select(controller, TrueGritState(controller, set, ordinal), set.ChoiceFor(deed));
        }

        private static bool OffersTrueGrit(LevelUpController controller, TrueGritBlueprintSet set, int ordinal, TrueGritDeed deed)
        {
            return FavoredClassLevelUpHarness.CanSelect(controller, TrueGritState(controller, set, ordinal), set.ChoiceFor(deed));
        }

        private static bool UnselectTrueGrit(LevelUpController controller, TrueGritBlueprintSet set, int ordinal)
        {
            FeatureSelectionState state = TrueGritState(controller, set, ordinal);
            if (!state.Selected) return false;
            controller.UnselectFeature(state);
            return !TrueGritState(controller, set, ordinal).Selected;
        }

        /// <summary>The first three deeds, in catalog order, the first open state offers.</summary>
        private static TrueGritPick TrueGritPicks(LevelUpController controller, TrueGritBlueprintSet set, JObject row)
        {
            FeatureSelectionState first = TrueGritState(controller, set, 0);
            TrueGritDeed[] offered = set.Deeds.Where(deed =>
                FavoredClassLevelUpHarness.CanSelect(controller, first, set.ChoiceFor(deed))).ToArray();
            row["offeredDeeds"] = new JArray(offered.Select(value => value.ToString()));
            if (offered.Length < 3)
                throw new InvalidOperationException("fewer than three True Grit deeds are offered: " + offered.Length);
            return new TrueGritPick { A = offered[0], B = offered[1], C = offered[2] };
        }

        private static void TrueGritStep(LevelUpController controller, TrueGritBlueprintSet set, JArray steps,
            IList<string> failures, string label, Func<bool> action, bool expected, Func<bool> offered = null)
        {
            var step = new JObject { ["step"] = label };
            if (offered != null) step["offered"] = offered();
            bool result = action();
            step["accepted"] = result;
            JObject slots = DescribeTrueGritSlots(controller, set);
            step["held"] = slots;
            steps.Add(step);
            if (result != expected) failures.Add(label + ": accepted=" + result + ", expected " + expected);
            if (offered != null && !expected && (bool)step["offered"])
                failures.Add(label + ": the held deed was offered to the other state");
            if ((bool)slots["duplicate"]) failures.Add(label + ": the same deed is held twice: " + TrueGritCompact(slots));
        }

        private static JObject DescribeTrueGritSlots(LevelUpController controller, TrueGritBlueprintSet set)
        {
            FeatureSelectionState[] states = TrueGritStates(controller, set);
            string[] held = states.Select(value => value.Selected && value.SelectedItem != null &&
                value.SelectedItem.Feature != null ? value.SelectedItem.Feature.name : "-").ToArray();
            int maxPreviewRank = set.Choices.Max(value => controller.Preview.Progression.Features.GetRank(value));
            bool duplicate = held.Where(value => value != "-").GroupBy(value => value).Any(group => group.Count() > 1) ||
                maxPreviewRank > 1;
            return new JObject
            {
                ["states"] = new JArray(held),
                ["previewTrueGritFacts"] = set.Choices.Count(value => controller.Preview.Progression.Features.GetRank(value) > 0),
                ["maxPreviewRank"] = maxPreviewRank,
                ["duplicate"] = duplicate
            };
        }

        private static JObject DescribeTrueGritState(UnitDescriptor unit, GunslingerClassBlueprintSet gunslinger)
        {
            TrueGritBlueprintSet set = gunslinger.TrueGrit;
            var ranks = new JObject();
            for (int index = 0; index < set.Choices.Length; index++)
            {
                int rank = unit.Progression.Features.GetRank(set.Choices[index]);
                if (rank > 0) ranks[set.Deeds[index].ToString()] = rank;
            }
            return new JObject
            {
                ["gunslingerLevel"] = unit.Progression.GetClassLevel(gunslinger.CharacterClass),
                ["characterLevel"] = unit.Progression.CharacterLevel,
                ["trueGritFacts"] = unit.Progression.Features.Enumerable.Count(fact =>
                    set.Choices.Any(choice => ReferenceEquals(fact.Blueprint, choice))),
                ["ranks"] = ranks,
                ["gritAmount"] = unit.Resources.GetResourceAmount(gunslinger.Grit.Resource),
                ["gritMaximum"] = gunslinger.Grit.Resource.GetMaxAmount(unit)
            };
        }

        private static string TrueGritCompact(JToken value)
        {
            return value == null ? "<null>" : value.ToString(Newtonsoft.Json.Formatting.None);
        }

        private static string TrueGritFailures(IList<string> failures)
        {
            return failures.Count == 0 ? string.Empty : ";failures=" + string.Join(" | ", failures.ToArray());
        }
    }
}
