using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Assets.UI.LevelUp;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UI.LevelUp;
using Kingmaker.UI.LevelUp.Phase;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Class.LevelUp;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class ElementalCharacterCreationBaselineScenario
    {
        private object[] _crossSceneBefore;
        private UnitEntityData[] _remoteCompanionsBefore;
        private UnitEntityData[] _partyCharactersBefore;
        private object[] _playerInventoryBefore;
        private int[] _playerInventoryCountsBefore;
        private ItemEntity[] _commitItemsBefore;
        private int[] _commitCountsBefore;
        private ItemEntity[] _commitItemsAfter;
        private int[] _commitCountsAfter;
        private long _commitMoneyBefore;
        private long _commitMoneyAfter;
        private bool _commitMoneyRestored;
        private long _playerMoneyBefore;
        private readonly bool _regression;
        private bool _racialCheckPending;
        private bool _commitCleanupPending;
        private int _commitRegistrationWait;
        private int _revisionViewWait;
        private JObject _creatorCleanupEvidence;
        private bool _revising;
        private int _revision;
        private int _revisionOperations;
        private int[] _revisionRoute;
        private int[] _allocatedBases;
        private JToken _allocatedDistribution;
        private readonly Queue<KeyValuePair<BlueprintFeatureSelection, BlueprintFeature>> _revisionChoices =
            new Queue<KeyValuePair<BlueprintFeatureSelection, BlueprintFeature>>();
        private static readonly StatType[] AbilityStats = { StatType.Strength, StatType.Dexterity,
            StatType.Constitution, StatType.Intelligence, StatType.Wisdom, StatType.Charisma };

        private void CaptureCreatorMembership()
        {
            if (!_canCommit) return;
            var player = Game.Instance.Player;
            _crossSceneBefore = player.CrossSceneState.AllEntityData.Cast<object>().ToArray();
            _remoteCompanionsBefore = player.RemoteCompanions.Select(value => value.Value).ToArray();
            _partyCharactersBefore = player.PartyCharacters.Select(value => value.Value).ToArray();
            _playerInventoryBefore = player.Inventory.Items.Cast<object>().ToArray();
            _playerInventoryCountsBefore = player.Inventory.Items.Select(item => item.Count).ToArray();
            _playerMoneyBefore = player.Money;
            _creatorPauseBefore = Game.Instance.IsPaused;
        }

        private void CommitOwnedCreator()
        {
            var inventory = Game.Instance.Player.Inventory;
            _commitMoneyRestored = false;
            _commitMoneyBefore = Game.Instance.Player.Money;
            _commitItemsBefore = inventory.Items.ToArray();
            _commitCountsBefore = _commitItemsBefore.Select(item => item.Count).ToArray();
            try { _build.Commit(); }
            finally
            {
                // Capture the synchronous native grant even if Commit throws. Only
                // these exact item references/count deltas belong to this fixture.
                _commitMoneyAfter = Game.Instance.Player.Money;
                _commitItemsAfter = inventory.Items.ToArray();
                _commitCountsAfter = _commitItemsAfter.Select(item => item.Count).ToArray();
            }
        }

        private void PollCommittedCreatorCleanup()
        {
            if (!(_nativeRespec && _respecOriginal != null) && _unit.Descriptor.IsCustomCompanion() &&
                !ReferenceEquals(_unit.HoldingState, Game.Instance.Player.CrossSceneState))
            {
                // Native AddEntity queues registration until EntityCreator.Tick.
                // Never dispose the unit while that exact native entry is pending.
                if (_unit.HoldingState != null || ++_commitRegistrationWait > 90)
                    throw new InvalidOperationException("Native mercenary cross-scene registration did not complete.");
                _settle = 2; return;
            }
            _commitCleanupPending = false;
            EndCharacter();
        }

        private void CleanupCreatorItems()
        {
            if (_commitItemsAfter == null) return;
            var player = Game.Instance.Player;
            _character["nativeCommitMoney"] = new JObject { ["before"] = _commitMoneyBefore,
                ["after"] = _commitMoneyAfter, ["beforeCleanup"] = player.Money };
            if (player.Money != (_commitMoneyRestored ? _commitMoneyBefore : _commitMoneyAfter) || _commitMoneyBefore != _playerMoneyBefore)
                throw new InvalidOperationException("Money changed outside the owned synchronous creator commit.");
            if (!_commitMoneyRestored && _commitMoneyAfter != _commitMoneyBefore) player.GainMoney(_commitMoneyBefore - _commitMoneyAfter);
            _commitMoneyRestored = true;
            _character["nativeCommitMoney"]["restored"] = player.Money;
            if (player.Money != _commitMoneyBefore) throw new InvalidOperationException("Native creator money rollback was not exact.");
            var inventory = player.Inventory;
            if (!CharacterCreationObservationIdentity.SameOrderedReferences(_commitItemsAfter, inventory.Items.Cast<object>().ToArray()) ||
                !_commitCountsAfter.SequenceEqual(inventory.Items.Select(item => item.Count)))
                throw new InvalidOperationException("Inventory changed outside the owned synchronous creator commit.");
            foreach (var item in _commitItemsBefore)
                if (!_commitItemsAfter.Any(value => ReferenceEquals(value, item)))
                    throw new InvalidOperationException("Native creator removed a preexisting inventory reference.");
            var removed = new JArray();
            for (int index = 0; index < _commitItemsAfter.Length; ++index)
            {
                ItemEntity item = _commitItemsAfter[index];
                int prior = Array.FindIndex(_commitItemsBefore, value => ReferenceEquals(value, item));
                int expected = prior < 0 ? 0 : _commitCountsBefore[prior];
                int excess = _commitCountsAfter[index] - expected;
                if (excess < 0) throw new InvalidOperationException("Native creator consumed a preexisting inventory stack.");
                if (excess == 0) continue;
                removed.Add(new JObject { ["guid"] = item.Blueprint.AssetGuid, ["count"] = excess,
                    ["newReference"] = prior < 0 });
                inventory.Remove(item, excess).Dispose();
            }
            bool exact = CharacterCreationObservationIdentity.SameOrderedReferences(_commitItemsBefore, inventory.Items.Cast<object>().ToArray()) &&
                _commitCountsBefore.SequenceEqual(inventory.Items.Select(item => item.Count));
            _character["nativeStarterCleanup"] = new JObject { ["removed"] = removed, ["exactReferencesAndCounts"] = exact };
            if (!exact) throw new InvalidOperationException("Native starting-item rollback did not restore exact references and counts.");
            _commitItemsBefore = null; _commitItemsAfter = null;
        }

        private void CleanupCreatorMembership()
        {
            var player = Game.Instance.Player;
            CleanupCreatorItems();
            if (_canCommit && _unit.Descriptor.IsCustomCompanion())
            {
                int remote = player.RemoteCompanions.Count(value => ReferenceEquals(value.Value, _unit));
                if (remote != (_committed ? 1 : 0))
                    _failures.Add("Native mercenary registration count diverged: " + remote);
                for (int index = player.RemoteCompanions.Count - 1; index >= 0; --index)
                    if (ReferenceEquals(player.RemoteCompanions[index].Value, _unit)) player.RemoteCompanions.RemoveAt(index);
                for (int index = player.PartyCharacters.Count - 1; index >= 0; --index)
                    if (ReferenceEquals(player.PartyCharacters[index].Value, _unit)) player.PartyCharacters.RemoveAt(index);
                player.InvalidateCharacterLists(); player.UpdateCharacterLists();
                _character["nativeMercenaryRegistration"] = new JObject { ["remoteBeforeCleanup"] = remote,
                    ["exactCrossSceneOwner"] = !_committed || ReferenceEquals(_unit.HoldingState, player.CrossSceneState) };
            }
            if (_unit.HoldingState != null && _unit.HoldingState.AllEntityData.Any(value => ReferenceEquals(value, _unit)))
                _unit.HoldingState.RemoveEntityData(_unit);
            else _unit.Dispose();
        }

        private bool CreatorMembershipRestored()
        {
            if (_crossSceneBefore == null) return true;
            var player = Game.Instance.Player;
            _creatorCleanupEvidence = new JObject {
                ["crossSceneExact"] = CharacterCreationObservationIdentity.SameOrderedReferences(_crossSceneBefore, player.CrossSceneState.AllEntityData.Cast<object>().ToArray()),
                ["remoteCompanionsExact"] = CharacterCreationObservationIdentity.SameOrderedReferences(_remoteCompanionsBefore, player.RemoteCompanions.Select(value => value.Value).ToArray()),
                ["partyCharactersExact"] = CharacterCreationObservationIdentity.SameOrderedReferences(_partyCharactersBefore, player.PartyCharacters.Select(value => value.Value).ToArray()),
                ["inventoryReferencesExact"] = CharacterCreationObservationIdentity.SameOrderedReferences(_playerInventoryBefore, player.Inventory.Items.Cast<object>().ToArray()),
                ["inventoryCountsExact"] = _playerInventoryCountsBefore.SequenceEqual(player.Inventory.Items.Select(item => item.Count)),
                ["moneyExact"] = _playerMoneyBefore == player.Money,
                ["respecPauseExact"] = !_nativeRespec || _creatorPauseBefore == Game.Instance.IsPaused };
            return _creatorCleanupEvidence.Properties().All(value => (bool)value.Value);
        }

        private ElementalRaceBlueprints RegressionRace => BlueprintBootstrap.ElementalRaces.OrderedBlueprints()
            .Single(value => ReferenceEquals(value.Race, _races[_raceIndex]));

        private void BeginRegressionCharacter()
        {
            _racialCheckPending = false; _commitCleanupPending = false; _commitRegistrationWait = 0; _revisionViewWait = 0; _revising = false; _revision = 0; _revisionOperations = 0;
            _revisionChoices.Clear(); _allocatedBases = null; _allocatedDistribution = null;
            if (!_regression) return;
            _revisionRoute = _nativeRespec ? new[] { ElementalCharacterCreationRegressionPlan.NativeRespecChoice(_raceIndex) } :
                ElementalCharacterCreationRegressionPlan.Route(_raceIndex);
            _character["heritageRevisionRoute"] = new JArray(_revisionRoute);
            _character["racialGraphs"] = new JArray();
            _character["nativeFinalReviews"] = new JArray();
        }

        private BlueprintFeature PreferredRegressionChoice(BlueprintFeatureSelection selection)
        {
            if (!_regression || selection == null) return null;
            ElementalRaceBlueprints race = RegressionRace;
            int choice = _revisionRoute[_revision];
            if (ReferenceEquals(selection, race.Heritages.Selection)) return race.Heritages.Choices()[choice].Marker;
            var slot = race.AlternateTraits.Selections().SingleOrDefault(value => ReferenceEquals(value.Selection, selection));
            if (slot == null) return null;
            var wanted = _nativeRespec ? ElementalCharacterCreationRegressionPlan.NativeRespecTraits(race.Heritages.Race, choice)
                : ElementalCharacterCreationRegressionPlan.Traits(race.Heritages.Race, choice);
            return slot.Choices.SingleOrDefault(value => wanted.Contains(value.Definition.Id))?.Marker ?? slot.RetainMarker;
        }

        private static void RejectDeferredChoices(IEnumerable<IFeatureSelectionItem> items)
        {
            if (items.Any(item => item.Feature != null &&
                (item.Feature.AssetGuid == "e117e1e0a17a4acec001000000000031" ||
                 item.Feature.AssetGuid == "e117e1e0a17a4acec001000000000040")))
                throw new InvalidOperationException("An exact deferred no-op marker reached the actual native choice list.");
        }

        private FeatureSelectionState ExactOwnedSelection(BlueprintFeatureSelection selection)
        {
            var matches = _controller.State.Selections.Where(value => ReferenceEquals(value.Selection, selection)).ToArray();
            if (matches.Length != 1) throw new InvalidOperationException("Owned native selection state absent or duplicated: " + selection.AssetGuid);
            return matches[0];
        }

        private bool RevisitOrQualifyFinalReview()
        {
            VerifySelectedRacialGraph("native-final-review");
            int[] bases = AbilityStats.Select(stat => _controller.Preview.Stats.GetStat(stat).BaseValue).ToArray();
            JToken distribution = JToken.FromObject(_controller.State.StatsDistribution.StatValues);
            if (_allocatedBases == null)
            { _allocatedBases = bases; _allocatedDistribution = distribution.DeepClone(); }
            bool exact = bases.SequenceEqual(_allocatedBases) && JToken.DeepEquals(distribution, _allocatedDistribution);
            var review = new JObject { ["revision"] = _revision, ["heritageIndex"] = _revisionRoute[_revision],
                ["baseValues"] = new JArray(bases), ["distributionValues"] = distribution,
                ["initialBaseValues"] = new JArray(_allocatedBases), ["allocationBaselineExact"] = exact,
                ["remainingSelections"] = _controller.State.RemainingSelections(),
                ["stateComplete"] = _controller.State.IsComplete(), ["nativeCompleteButton"] = NextEnabled(),
                ["skillPointsRemaining"] = _controller.State.SkillPointsRemaining };
            ((JArray)_character["nativeFinalReviews"]).Add(review);
            if (!exact) throw new InvalidOperationException("Heritage back-navigation contaminated the native ability allocation baseline: " + review);
            if (_useRoll) VerifyCurrentRollOwner(review);
            foreach (var selection in new[] { RegressionRace.Heritages.Selection }.Concat(
                RegressionRace.AlternateTraits.Selections().Select(value => value.Selection)))
                if (!ReferenceEquals(ExactOwnedSelection(selection).SelectedItem?.Feature, PreferredRegressionChoice(selection)))
                    throw new InvalidOperationException("The native final review did not retain the planned racial choices.");
            Write();
            if (_revision + 1 == _revisionRoute.Length) return false;
            ++_revision;
            // Reset all replacement slots through their unconditional retain choices
            // before selecting a new multi-slot combination. Never remove facts directly.
            foreach (var slot in RegressionRace.AlternateTraits.Selections())
                _revisionChoices.Enqueue(new KeyValuePair<BlueprintFeatureSelection, BlueprintFeature>(slot.Selection, slot.RetainMarker));
            _revisionChoices.Enqueue(new KeyValuePair<BlueprintFeatureSelection, BlueprintFeature>(
                RegressionRace.Heritages.Selection, PreferredRegressionChoice(RegressionRace.Heritages.Selection)));
            foreach (var slot in RegressionRace.AlternateTraits.Selections())
                _revisionChoices.Enqueue(new KeyValuePair<BlueprintFeatureSelection, BlueprintFeature>(slot.Selection, PreferredRegressionChoice(slot.Selection)));
            _revising = true; _settle = 8;
            return true;
        }

        private void VerifyCurrentRollOwner(JObject evidence)
        {
            var entry = UnityModManagerNet.UnityModManager.modEntries.Single(value => value.Info.Id == "KingmakerDiceRoller" && value.Active);
            var bridge = entry.Assembly.GetType("KingmakerDiceRoller.Patches.KingmakerPatchBridge", true);
            object panel = bridge.GetField("panel", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).GetValue(null);
            object commands = panel.GetType().GetField("commands", Members).GetValue(panel);
            object session = commands.GetType().GetProperty("ActiveSession", Members).GetValue(commands, null);
            Func<string, object> read = name => session.GetType().GetProperty(name, Members).GetValue(session, null);
            object assignment = read("Assignment");
            int[] values = assignment == null ? new int[0] : (int[])assignment.GetType().GetMethod("ToAssignedArray", Members).Invoke(assignment, null);
            bool exact = ReferenceEquals(read("Controller"), _controller) && ReferenceEquals(read("State"), _controller.State) &&
                ReferenceEquals(read("Unit"), _controller.Preview) && (bool)read("IsRollMode") && (bool)read("IsApplied") &&
                !(bool)read("CandidateBaselineContaminated") && values.SequenceEqual(_allocatedBases);
            evidence["diceAssignment"] = new JArray(values); evidence["diceOwnerAndBaselineExact"] = exact;
            if (!exact) throw new InvalidOperationException("Dice Roller ownership/assignment changed across native racial back-navigation.");
        }

        private static UnityEngine.Events.UnityAction RenderedNativeAction(CharBuildSelectorItem item)
        {
            return (UnityEngine.Events.UnityAction)typeof(CharBuildSelectorItem).GetField("Action", Members).GetValue(item);
        }

        private bool MatchesNativeSelectionIdentity(FeatureSelectionState view, FeatureSelectionState current)
        {
            // Native SelectFeature.GetSelectionState resolves exact Selection+Index.
            // A rebuilt state may retain a rendered view with a distinct object.
            return view != null && current != null && ReferenceEquals(view.Selection, current.Selection) &&
                view.Index == current.Index && view.Level == current.Level &&
                ReferenceEquals(view.SourceFeature, current.SourceFeature) &&
                _controller.State.Selections.Count(value => ReferenceEquals(value.Selection, view.Selection) &&
                    value.Index == view.Index) == 1;
        }

        private void DriveRacialRevision()
        {
            if (++_revisionOperations > 260) throw new InvalidOperationException("Native racial revision did not converge.");
            if (_build.CurrentPhase != CharBPhase.Type.Determinator)
            {
                if (!_build.CurrentPhase.HasValue || (int)_build.CurrentPhase.Value < (int)CharBPhase.Type.Determinator)
                    throw new InvalidOperationException("Native Back skipped the unlocked Heritage phase.");
                var before = _build.CurrentPhase;
                _build.Back();
                ((JArray)_character["steps"]).Add(new JObject { ["action"] = "native-back", ["before"] = before.ToString(),
                    ["after"] = _build.CurrentPhase.ToString(), ["revision"] = _revision });
                _settle = 12; return;
            }
            if (_revisionChoices.Count == 0)
            {
                VerifySelectedRacialGraph("native-racial-revision-complete");
                _revising = false; Advance(); return;
            }
            var change = _revisionChoices.Peek();
            FeatureSelectionState selection = ExactOwnedSelection(change.Key);
            if (ReferenceEquals(selection.SelectedItem?.Feature, change.Value))
            { _revisionChoices.Dequeue(); _settle = 2; return; }
            var phase = _build.Determinators;
            var current = (FeatureSelectionState)typeof(CharBPhaseFeatures).GetProperty("CurrentFeatureCollection", Members).GetValue(phase, null);
            if (!ReferenceEquals(current, selection))
            {
                var switcher = (CharBSelectionSwitch)typeof(CharBPhaseFeatures).GetField("m_CollectionSwitcher", Members).GetValue(phase);
                var tabs = switcher.Items.Where(item => ReferenceEquals(item.FeatureSelection, change.Key) &&
                    item.gameObject.activeInHierarchy && item.Toggle != null && item.Toggle.interactable).ToArray();
                if (tabs.Length != 1) throw new InvalidOperationException("Native Heritage selection tab is absent or ambiguous: " + change.Key.AssetGuid);
                tabs[0].Toggle.isOn = true;
                _settle = 8; return;
            }
            var items = selection.Selection.ExtractSelectionItems(_controller.Unit, _controller.Preview).ToArray();
            RejectDeferredChoices(items);
            IFeatureSelectionItem choice = items.SingleOrDefault(item => ReferenceEquals(item.Feature, change.Value));
            var renderedItems = _build.GetComponentsInChildren<CharBuildSelectorItem>(true).Where(item =>
                item.gameObject.activeInHierarchy && item.Toggle != null && item.Toggle.interactable && RenderedNativeAction(item) != null &&
                MatchesNativeSelectionIdentity(item.FeatureSelection, selection) && ReferenceEquals(item.Feature?.Feature, change.Value)).ToArray();
            if (renderedItems.Length > 1) throw new InvalidOperationException("The native racial view identity is ambiguous.");
            bool rendered = renderedItems.Length == 1;
            bool legal = choice != null && selection.Selection.CanSelect(_controller.Preview, _controller.State, selection, choice);
            if (!legal || !rendered)
            {
                if (_revisionViewWait == 0) ((JArray)_character["steps"]).Add(new JObject {
                    ["action"] = "native-revision-choice-readiness", ["choiceGuid"] = change.Value.AssetGuid,
                    ["extracted"] = choice != null, ["legal"] = legal, ["rendered"] = rendered,
                    ["views"] = new JArray(_build.GetComponentsInChildren<CharBuildSelectorItem>(true).Where(item =>
                        item.gameObject.activeInHierarchy || ReferenceEquals(item.FeatureSelection?.Selection, selection.Selection)).Select(item => new JObject {
                        ["guid"] = item.Feature?.Feature?.AssetGuid, ["active"] = item.gameObject.activeInHierarchy,
                        ["interactable"] = item.Toggle != null && item.Toggle.interactable,
                        ["exactSelection"] = ReferenceEquals(item.FeatureSelection, selection),
                        ["nativeIdentityExact"] = MatchesNativeSelectionIdentity(item.FeatureSelection, selection),
                        ["nativeValueEquality"] = item.FeatureSelection != null && item.FeatureSelection.Equals(selection) })) });
                if (legal && ++_revisionViewWait < 90) { _settle = 2; return; }
                Write();
                throw new InvalidOperationException("Native back-navigation cannot visibly select the requested legal racial choice: " + change.Value.AssetGuid);
            }
            _revisionViewWait = 0;
            Capture("rendered-racial-revision-ready:" + change.Key.AssetGuid);
            string old = selection.SelectedItem?.Feature?.AssetGuid;
            var viewContract = new JObject { ["sameStateReference"] = ReferenceEquals(renderedItems[0].FeatureSelection, selection),
                ["nativeValueEquality"] = renderedItems[0].FeatureSelection.Equals(selection),
                ["exactSelectionReference"] = ReferenceEquals(renderedItems[0].FeatureSelection.Selection, selection.Selection),
                ["viewIndex"] = renderedItems[0].FeatureSelection.Index, ["activeIndex"] = selection.Index,
                ["viewLevel"] = renderedItems[0].FeatureSelection.Level, ["activeLevel"] = selection.Level,
                ["renderedActionInvoked"] = true };
            RenderedNativeAction(renderedItems[0]).Invoke();
            ((JArray)_character["steps"]).Add(new JObject { ["action"] = "native-racial-reselection", ["revision"] = _revision,
                ["selectionGuid"] = change.Key.AssetGuid, ["oldChoiceGuid"] = old, ["choiceGuid"] = change.Value.AssetGuid,
                ["phase"] = _build.CurrentPhase.ToString(), ["extractedCount"] = items.Length, ["nativeViewContract"] = viewContract });
            _revisionChoices.Dequeue(); _racialCheckPending = true; _settle = 8;
        }

        private void VerifySelectedRacialGraph(string checkpoint, UnitDescriptor committedOwner = null)
        {
            if (!_regression) return;
            UnitDescriptor owner = committedOwner ?? _controller.Preview;
            ElementalRaceBlueprints race = RegressionRace;
            var selectedHeritage = ExactOwnedSelection(race.Heritages.Selection).SelectedItem?.Feature;
            var heritage = race.Heritages.Choices().SingleOrDefault(value => ReferenceEquals(value.Marker, selectedHeritage)) ?? race.Heritages.General;
            var selectedTraits = race.AlternateTraits.Selections().Select(value => ExactOwnedSelection(value.Selection).SelectedItem?.Feature).ToArray();
            var traits = race.AlternateTraits.Traits().Where(value => selectedTraits.Any(marker => ReferenceEquals(marker, value.Marker))).ToArray();
            ElementalRacialTraitSlot replaced = traits.Aggregate(ElementalRacialTraitSlot.None, (slots, value) => slots | value.Definition.ReplacedSlots);
            var facts = new JArray(); var resources = new JArray(); var abilities = new JArray();
            bool exact = ReferenceEquals(owner.Progression.Race, race.Race);
            int[] committedBases = committedOwner == null ? null : AbilityStats.Select(stat => owner.Stats.GetStat(stat).BaseValue).ToArray();
            if (committedBases != null) exact &= _allocatedBases != null && committedBases.SequenceEqual(_allocatedBases);
            Action<BlueprintFeature, int> fact = (blueprint, expected) => {
                int count = owner.Progression.Features.Enumerable.Count(value => ReferenceEquals(value.Blueprint, blueprint));
                int rank = owner.Progression.Features.GetRank(blueprint);
                facts.Add(new JObject { ["guid"] = blueprint.AssetGuid, ["expected"] = expected, ["count"] = count, ["rank"] = rank });
                exact &= count == expected && rank == expected;
            };
            Action<BlueprintAbilityResource, int> resource = (blueprint, expected) => {
                int count = owner.Resources.PersistantResources.Count(value => value != null && ReferenceEquals(value.Blueprint, blueprint));
                int amount = owner.Resources.GetResourceAmount(blueprint);
                resources.Add(new JObject { ["guid"] = blueprint.AssetGuid, ["expectedCount"] = expected, ["count"] = count, ["amount"] = amount,
                    ["expectedAmount"] = expected == 0 ? 0 : _nativeRespec && _respecSpent.ContainsKey(blueprint.AssetGuid)
                        ? _respecSpent[blueprint.AssetGuid] : 1 });
                exact &= count == expected && (_nativeRespec
                    ? ObserveRespecResourceAmount(checkpoint, blueprint, expected, amount, committedOwner != null)
                    : expected == 0 || amount == 1);
            };
            Action<BlueprintAbility, int> ability = (blueprint, expected) => {
                int count = owner.Abilities.Enumerable.Count(value => ReferenceEquals(value.Blueprint, blueprint));
                abilities.Add(new JObject { ["guid"] = blueprint.AssetGuid, ["expected"] = expected, ["count"] = count });
                exact &= count == expected;
            };
            fact(race.Resistance, (replaced & ElementalRacialTraitSlot.EnergyResistance) == 0 ? 1 : 0);
            foreach (var choice in race.Heritages.Choices())
            {
                bool current = ReferenceEquals(heritage, choice);
                fact(choice.Marker, ReferenceEquals(selectedHeritage, choice.Marker) ? 1 : 0);
                fact(choice.Affinity, current && (replaced & ElementalRacialTraitSlot.ElementalAffinity) == 0 ? 1 : 0);
                int sla = current && (replaced & ElementalRacialTraitSlot.RacialSpellLikeAbility) == 0 ? 1 : 0;
                fact(choice.SlaFeature, sla); resource(choice.SlaResource, sla); ability(choice.SlaAbility, sla);
            }
            foreach (var trait in race.AlternateTraits.Traits())
            {
                int active = traits.Contains(trait) ? 1 : 0;
                exact &= active == 0 || trait.Definition.IsPublished;
                fact(trait.Marker, active); fact(trait.Provider, active);
                foreach (var item in trait.Mechanics().OfType<BlueprintAbilityResource>()) resource(item, active);
                foreach (var item in trait.Mechanics().OfType<BlueprintAbility>()) ability(item, item.Parent == null ? active : 0);
            }
            foreach (var slot in race.AlternateTraits.Selections())
            {
                var selection = ExactOwnedSelection(slot.Selection);
                fact(slot.RetainMarker, ReferenceEquals(selection.SelectedItem?.Feature, slot.RetainMarker) ? 1 : 0);
                var items = selection.Selection.ExtractSelectionItems(_controller.Unit, owner).ToArray();
                RejectDeferredChoices(items);
                if (committedOwner == null && !ReferenceEquals(selection.SelectedItem?.Feature, slot.RetainMarker))
                {
                    var retain = items.Single(item => ReferenceEquals(item.Feature, slot.RetainMarker));
                    exact &= selection.Selection.CanSelect(owner, _controller.State, selection, retain);
                }
            }
            exact &= ObserveNativeRespecBlood(checkpoint, owner, committedOwner != null);
            int[] overlay = AbilityStats.Select(stat => owner.Stats.GetStat(stat).ModifiedValue - owner.Stats.GetStat(stat).BaseValue).ToArray();
            int[] expectedOverlay = Enumerable.Range(0, 6).Select(index => heritage.Definition.ModifierFor((ElementalHeritageStat)index)).ToArray();
            exact &= overlay.SequenceEqual(expectedOverlay);
            var graph = new JObject { ["checkpoint"] = checkpoint, ["revision"] = _revision,
                ["heritage"] = heritage.Definition.Id.ToString(), ["traits"] = new JArray(traits.Select(value => value.Definition.Id.ToString())),
                ["overlay"] = new JArray(overlay), ["expectedOverlay"] = new JArray(expectedOverlay),
                ["facts"] = facts, ["resources"] = resources, ["abilities"] = abilities,
                ["committedBaseValues"] = committedBases == null ? (JToken)JValue.CreateNull() : new JArray(committedBases), ["exact"] = exact };
            ((JArray)_character["racialGraphs"]).Add(graph);
            if (!exact) { Write(); throw new InvalidOperationException("The actual native racial graph diverged at " + checkpoint + "; see racialGraphs."); }
        }
    }
}
