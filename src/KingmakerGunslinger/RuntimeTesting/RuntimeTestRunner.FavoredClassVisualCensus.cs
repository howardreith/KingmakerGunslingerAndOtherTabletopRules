using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Assets.UI.LevelUp;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.LevelUp;
using Kingmaker.UI.LevelUp.Phase;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.Visual.CharacterSystem;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using KingmakerGunslinger.FavoredClass;
using KingmakerGunslinger.FavoredClass.Mechanics;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private const BindingFlags FcbCensusMembers = BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.Instance;
        private const int FcbCensusSettle = 12;

        private IEnumerator<object> _fcbCensusSteps;
        private readonly List<RuntimeTestAssertion> _fcbCensusAssertions = new List<RuntimeTestAssertion>();
        private JObject _fcbCensusEvidence;

        private sealed class FcbCensusVisit
        {
            internal BlueprintCharacterClass Class;
            internal BlueprintFeatureSelection Reward;
            internal string Ancestry;
            internal BlueprintRace Race;
            internal readonly List<FavoredClassLeafPair> Pairs = new List<FavoredClassLeafPair>();
        }

        // Continuation item 6: the actual native level-up screens render every
        // published favored-class reward selector (one real level-up per
        // class and ancestry, once at zero investment and once when a Full
        // step is next) and each Mostly Human selector in the real creator,
        // with every row's exact icon: no blank or placeholder icon and no
        // empty selector. Write-free in the loaded working save.
        private void PollFcbVisualCensus()
        {
            if (!_request.ExitAfterCompletion || _workingSaveSmoke == null || !_workingSaveSmoke.Complete ||
                _workingSaveSmoke.WriteObserved)
                throw new InvalidOperationException(
                    "The favored-class visual census lacks its guarded working save or intact write boundary.");
            if (_fcbCensusSteps == null)
            {
                _fcbCensusEvidence = new JObject();
                _fcbCensusSteps = RunFcbVisualCensus().GetEnumerator();
            }
            Exception failure = null;
            try { if (_fcbCensusSteps.MoveNext()) return; }
            catch (Exception exception) { failure = exception; }
            try { _fcbCensusSteps.Dispose(); }
            catch (Exception exception) { if (failure == null) failure = exception; }
            _fcbCensusSteps = null;
            string path = WriteFavoredClassEvidence("favored-class-visual-census.json", _fcbCensusEvidence);
            var assertions = new List<RuntimeTestAssertion>(_fcbCensusAssertions);
            assertions.Add(Assertion("fcb-census-write-boundary", "no save write in the whole lane",
                "writeObserved=" + _workingSaveSmoke.WriteObserved, !_workingSaveSmoke.WriteObserved,
                "working-save write guard"));
            assertions.Add(Assertion("loaded-mod-version", _request.ExpectedModVersion, _context.ModEntry.Info.Version,
                _request.ExpectedModVersion == _context.ModEntry.Info.Version, "Unity Mod Manager ModEntry.Info.Version"));
            bool pass = failure == null && assertions.TrueForAll(value => value.Status == "PASS");
            RuntimeTestResult result = CreateResult(failure != null ? RuntimeTestStatuses.Error :
                pass ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions,
                failure == null ? null : failure.ToString());
            result.EvidenceFiles.Add(path);
            Complete(result);
        }

        private IEnumerable<object> RunFcbVisualCensus()
        {
            FavoredClassBlueprintSet leaves = BlueprintBootstrap.FavoredClassLeaves;
            FavoredClassHostHandles host = FavoredClassIntegrationCoordinator.Host;
            if (FavoredClassIntegrationStatusRegistry.Current.Availability !=
                    FavoredClassIntegrationAvailability.Published || leaves == null || host == null)
                throw new InvalidOperationException("The visual census requires the published integration.");
            UnitEntityData anchor = Game.Instance.Player.Party.FirstOrDefault(value => value != null &&
                value.View != null && value.IsInGame);
            if (anchor == null)
                throw new InvalidOperationException("The working save has no active-area party anchor.");
            var skipped = new JArray();
            List<FcbCensusVisit> visits = FcbCensusVisits(leaves, host, skipped);
            _fcbCensusEvidence["skipped"] = skipped;
            var records = new JArray();
            _fcbCensusEvidence["levelUps"] = records;
            var failures = new List<string>();
            var rendered = new HashSet<BlueprintFeature>();
            foreach (FcbCensusVisit visit in visits)
            {
                bool hasPartial = visit.Pairs.Any(pair => pair.Partial != null);
                foreach (bool fullNext in hasPartial ? new[] { false, true } : new[] { false })
                    foreach (object step in FcbCensusLevelUp(visit, fullNext, anchor, records, rendered, failures))
                        yield return step;
            }
            var published = new HashSet<BlueprintFeature>(visits.SelectMany(visit => visit.Pairs)
                .SelectMany(pair => pair.Leaves));
            BlueprintFeature[] unrendered = published.Where(leaf => !rendered.Contains(leaf))
                .OrderBy(leaf => leaf.name, StringComparer.Ordinal).ToArray();
            _fcbCensusEvidence["coverage"] = new JObject
            {
                ["visits"] = visits.Count,
                ["levelUps"] = records.Count,
                ["publishedLeaves"] = published.Count,
                ["renderedLeaves"] = published.Count(rendered.Contains),
                ["unrendered"] = new JArray(unrendered.Select(leaf => leaf.name)),
            };
            _fcbCensusAssertions.Add(Assertion("fcb-census-reward-selectors",
                "every favored-class level of every published class and ancestry opens its reward selector in the actual level-up screen; every rendered KMG row shows its own exact icon (never the acronym placeholder; the host's own rows are recorded), no selector is empty, and every leaf the native backend offers is rendered",
                Describe(new JObject { ["levelUps"] = records.Count, ["visits"] = visits.Count }, failures),
                failures.Count == 0 && visits.Count > 0, "CharBPhaseFeatures, CharBFeatureSelector and CharBuildSelectorItem rows"));
            _fcbCensusAssertions.Add(Assertion("fcb-census-coverage",
                "every published reward leaf was rendered with its exact icon in at least one actual level-up screen",
                _fcbCensusEvidence["coverage"].ToString(Newtonsoft.Json.Formatting.None),
                published.Count > 0 && unrendered.Length == 0, "rendered rows across all census level-ups"));

            // The Mostly Human selector of each parent race in the actual creator.
            var mostlyHuman = new JArray();
            _fcbCensusEvidence["mostlyHuman"] = mostlyHuman;
            var mostlyHumanFailures = new List<string>();
            foreach (ElementalMostlyHumanRaceBlueprints race in BlueprintBootstrap.MostlyHuman.Races)
                foreach (object step in FcbCensusMostlyHuman(race, mostlyHuman, mostlyHumanFailures))
                    yield return step;
            _fcbCensusAssertions.Add(Assertion("fcb-census-mostly-human",
                "the actual creator's Heritage phase shows each parent race's Mostly Human selector with its own icon and its two rows (standard and Mostly Human) with their exact icons",
                Describe(mostlyHuman, mostlyHumanFailures), mostlyHumanFailures.Count == 0 &&
                    mostlyHuman.Count == BlueprintBootstrap.MostlyHuman.Races.Count,
                "CharacterBuildController.HandleLevelUpStart (CharGen), CharBPhaseFeatures Determinators"));
        }

        /// <summary>One visit per published class and ancestry whose reward selection holds its leaves.</summary>
        private static List<FcbCensusVisit> FcbCensusVisits(FavoredClassBlueprintSet leaves,
            FavoredClassHostHandles host, JArray skipped)
        {
            var library = BlueprintBootstrap.Library;
            var visits = new Dictionary<string, FcbCensusVisit>(StringComparer.Ordinal);
            foreach (FavoredClassLeafPair pair in leaves.Pairs)
            {
                BlueprintFeatureSelection reward = host.BonusSelectionFor(pair.HostClassGuid);
                if (reward == null || !(reward.AllFeatures ?? new BlueprintFeature[0]).Contains(pair.Full))
                {
                    skipped.Add(pair.Full.name + ": not in a published reward selection");
                    continue;
                }
                if (FavoredClassRuntime.IsEffectUnavailable(pair.Effect.Id) ||
                    FavoredClassRuntime.IsTargetUnavailable(pair.Effect.Id, pair.TargetKey))
                {
                    skipped.Add(pair.Full.name + ": effect or target unavailable in this profile");
                    continue;
                }
                BlueprintScriptableObject rawClass;
                if (!library.BlueprintsByAssetId.TryGetValue(pair.HostClassGuid, out rawClass) ||
                    !(rawClass is BlueprintCharacterClass))
                {
                    skipped.Add(pair.Full.name + ": host class unavailable");
                    continue;
                }
                foreach (string rowId in FavoredClassLeafCatalog.TargetRows(pair.Effect.Id, pair.TargetKey))
                {
                    FavoredClassSourceRow row = FavoredClassCatalog.Row(rowId);
                    if (row == null || !row.IsScheduled)
                        continue;
                    if (!FavoredClassRuntime.Profile.Offers(row.Profile))
                    {
                        skipped.Add(pair.Full.name + " via " + rowId + ": " + row.Profile + " profile off");
                        continue;
                    }
                    FavoredClassRaceIdentity identity = FavoredClassRaceIdentities.ForAncestry(row.Ancestry);
                    BlueprintScriptableObject rawRace = null;
                    if (identity == null || !library.BlueprintsByAssetId.TryGetValue(identity.RaceGuid, out rawRace) ||
                        !(rawRace is BlueprintRace))
                    {
                        skipped.Add(pair.Full.name + " via " + rowId + ": race " + row.Ancestry + " unavailable");
                        continue;
                    }
                    string key = pair.HostClassGuid + "|" + row.Ancestry;
                    FcbCensusVisit visit;
                    if (!visits.TryGetValue(key, out visit))
                        visits[key] = visit = new FcbCensusVisit
                        {
                            Class = (BlueprintCharacterClass)rawClass,
                            Reward = reward,
                            Ancestry = row.Ancestry,
                            Race = (BlueprintRace)rawRace,
                        };
                    if (!visit.Pairs.Contains(pair))
                        visit.Pairs.Add(pair);
                }
            }
            return visits.Values.OrderBy(value => value.Class.name + "|" + value.Ancestry, StringComparer.Ordinal)
                .ToList();
        }

        private IEnumerable<object> FcbCensusLevelUp(FcbCensusVisit visit, bool fullNext, UnitEntityData anchor,
            JArray records, HashSet<BlueprintFeature> rendered, List<string> failures)
        {
            var library = BlueprintBootstrap.Library;
            var ui = Game.Instance.UI;
            CharacterBuildController presenter = ui.CharacterBuildController;
            LevelUpController priorBackend = ui.LevelUpController;
            UnitDescriptor priorUnit = presenter.Unit;
            string label = visit.Class.name + "/" + visit.Ancestry + (fullNext ? "/full-next" : "/zero");
            var record = new JObject { ["visit"] = label };
            records.Add(record);
            var reserved = new HashSet<string>(StringComparer.Ordinal) { visit.Reward.AssetGuid };
            BlueprintFeature hitPoint = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library,
                FcbHostHitPointRewardGuid, "host favored-class hit point");
            UnitEntityData unit = null;
            LevelUpController backend = null;
            Toggle showAll = null;
            bool originalShowAll = false;
            try
            {
                unit = SpawnFcbCensusUnit(anchor, visit.Race, visit.Class, "KMG FCB Census " + label);
                var seedRow = new JObject();
                LevelUpController seed = null;
                try
                {
                    seed = FavoredClassLevelUpHarness.Open(unit.Descriptor, visit.Race, visit.Class,
                        "KMG FCB Census");
                    if (FavoredClassLevelUpHarness.ChooseFavoredClass(seed, visit.Class, seedRow) == null)
                        throw new InvalidOperationException("the favored " + visit.Class.name +
                            " progression is unavailable");
                    FavoredClassLevelUpHarness.FillOthers(seed, reserved);
                    FeatureSelectionState seedReward = FavoredClassLevelUpHarness.FindOpenState(seed,
                        visit.Reward.AssetGuid);
                    if (seedReward == null || !FavoredClassLevelUpHarness.Select(seed, seedReward, hitPoint))
                        throw new InvalidOperationException("level 1 could not take the host hit point reward");
                    FavoredClassLevelUpHarness.FillOthers(seed, reserved);
                    FillFcbSpells(seed);
                    FavoredClassLevelUpHarness.FillOthers(seed, reserved);
                    if (!FavoredClassLevelUpHarness.Confirm(seed, unit.Descriptor, seedRow))
                        throw new InvalidOperationException("level 1 is incomplete: " + seedRow["completion"]);
                }
                finally
                {
                    FavoredClassLevelUpHarness.Close(seed);
                }
                GrantFcbCensusTargets(unit, visit);
                if (fullNext)
                    foreach (FavoredClassLeafPair pair in visit.Pairs.Where(value => value.Partial != null))
                        GrantFavoredClassRanks(unit, pair.Partial, pair.Effect.Rate.Divisor - 1);
                typeof(UnitProgressionData).GetProperty("Experience").SetValue(unit.Descriptor.Progression,
                    Game.Instance.BlueprintRoot.Progression.XPTable.GetBonus(2), null);
            }
            catch (Exception exception)
            {
                failures.Add(label + ": seed: " + exception.GetType().Name + ": " + exception.Message);
                record["failure"] = exception.Message;
                DestroyFcbCensusUnit(unit);
                yield break;
            }
            try
            {
                presenter.HandleLevelUpStart(unit.Descriptor, null, () => { });
                backend = presenter.LevelUpController;
                if (backend == null || !presenter.IsShow || ReferenceEquals(backend.Preview, unit.Descriptor))
                    throw new InvalidOperationException("the actual level-up screen did not open");
            }
            catch (Exception exception)
            {
                failures.Add(label + ": open: " + exception.Message);
                record["failure"] = exception.Message;
                CloseFcbCensus(presenter, backend, priorBackend, priorUnit);
                DestroyFcbCensusUnit(unit);
                yield break;
            }
            for (int frame = 0; frame < FcbCensusSettle; frame++) yield return null;
            string problem = null;
            FeatureSelectionState state = null;
            try
            {
                presenter.SetClass(visit.Class);
                state = backend.State.Selections.FirstOrDefault(value => !value.Selected &&
                    ReferenceEquals(value.Selection, visit.Reward));
                if (state == null)
                    problem = "the favored-class level opened no reward selector";
                else
                {
                    FavoredClassLevelUpHarness.FillOthers(backend, reserved);
                    FillFcbSpells(backend);
                    RefreshFcbCensusScreen(presenter);
                    // Every backend pick rebuilds the level-up state.
                    state = FcbCensusRewardState(backend, visit) ?? state;
                }
            }
            catch (Exception exception)
            {
                problem = "class: " + exception.Message;
            }
            // The screen routes a selection by its feature group: the host's
            // reward selection is shown in the Determinator phase, other
            // groups in Abilities.
            CharBPhaseFeatures holder = null;
            CharBPhase.Type holderType = CharBPhase.Type.Abilities;
            if (problem == null)
            {
                for (int frame = 0; frame < FcbCensusSettle; frame++) yield return null;
                if (presenter.Determinators.FeatureCollections != null &&
                    presenter.Determinators.FeatureCollections.Contains(state))
                {
                    holder = presenter.Determinators;
                    holderType = CharBPhase.Type.Determinator;
                }
                else if (presenter.Abilities.FeatureCollections != null &&
                    presenter.Abilities.FeatureCollections.Contains(state))
                    holder = presenter.Abilities;
                record["phase"] = holder == null ? "none" : holderType.ToString();
                if (holder == null)
                    problem = "no feature phase holds the reward selector";
            }
            if (problem == null)
            {
                foreach (object step in FcbCensusAdvanceTo(presenter, holderType, record))
                    yield return step;
                if (presenter.CurrentPhase != holderType)
                {
                    record["phaseTerms"] = FcbCensusPhaseTerms(presenter, backend);
                    problem = "the " + holderType + " phase did not open: " + presenter.CurrentPhase;
                }
            }
            CharBFeatureSelector selector = null;
            CharBSelectionSwitchItem switchItem = null;
            if (problem == null)
            {
                problem = ShowFcbCensusCollection(holder, state, out selector, out switchItem);
                for (int frame = 0; frame < FcbCensusSettle; frame++) yield return null;
            }
            if (problem == null)
            {
                showAll = FcbCensusShowAll(selector);
                if (showAll != null)
                {
                    originalShowAll = showAll.isOn;
                    showAll.isOn = true;
                }
                for (int frame = 0; frame < 2 * FcbCensusSettle; frame++) yield return null;
                // A refresh may re-activate the phase's first empty collection;
                // the reward collection is switched to again until it shows.
                var shown = new JArray();
                record["shown"] = shown;
                state = FcbCensusRewardState(backend, visit) ?? state;
                for (int attempt = 0; attempt < 3 &&
                    !SameFcbCensusState(selector.SelectorLayerBody.CurrentSelectionState, state); attempt++)
                {
                    FeatureSelectionState current = selector.SelectorLayerBody.CurrentSelectionState;
                    shown.Add(current == null || current.Selection == null ? "none" :
                        FavoredClassLevelUpHarness.Name(current.Selection) + "#" + current.Index);
                    problem = ShowFcbCensusCollection(holder, state, out selector, out switchItem);
                    if (problem != null)
                        break;
                    for (int frame = 0; frame < 2 * FcbCensusSettle; frame++) yield return null;
                }
            }
            if (problem == null)
            {
                try
                {
                    problem = CaptureFcbCensusRows(visit, state, backend, selector, switchItem, record, rendered);
                }
                catch (Exception exception)
                {
                    problem = "capture: " + exception.GetType().Name + ": " + exception.Message;
                }
            }
            if (problem != null)
            {
                failures.Add(label + ": " + problem);
                record["failure"] = problem;
            }
            try { if (showAll != null) showAll.isOn = originalShowAll; } catch (Exception) { }
            CloseFcbCensus(presenter, backend, priorBackend, priorUnit);
            for (int frame = 0; frame < 4; frame++) yield return null;
            DestroyFcbCensusUnit(unit);
        }

        /// <summary>
        /// Presses the screen's own Next (its button setup, then ToNextPhase),
        /// as a player does, until the phase opens or Next stops moving.
        /// </summary>
        private static IEnumerable<object> FcbCensusAdvanceTo(CharacterBuildController presenter,
            CharBPhase.Type target, JObject record)
        {
            var trail = record["navigation"] as JArray ?? new JArray();
            record["navigation"] = trail;
            for (int step = 0; step < 8 && presenter.CurrentPhase != target; step++)
            {
                CharBPhase.Type? before = presenter.CurrentPhase;
                typeof(CharacterBuildController).GetMethod("SetupButton", FcbCensusMembers).Invoke(presenter, null);
                presenter.ToNextPhase();
                for (int frame = 0; frame < FcbCensusSettle; frame++) yield return null;
                trail.Add((before.HasValue ? before.Value.ToString() : "none") + "->" +
                    (presenter.CurrentPhase.HasValue ? presenter.CurrentPhase.Value.ToString() : "none"));
                if (presenter.CurrentPhase == before)
                    yield break;
            }
        }

        /// <summary>The terms of the native phase unlock rule, for a phase that did not open.</summary>
        private static JObject FcbCensusPhaseTerms(CharacterBuildController presenter, LevelUpController backend)
        {
            return new JObject
            {
                ["current"] = presenter.CurrentPhase.HasValue ? presenter.CurrentPhase.Value.ToString() : "none",
                ["next"] = presenter.NextPhase.ToString(),
                ["classSelected"] = presenter.Class.IsSelected() || presenter.ClassInChargen.IsSelected(),
                ["determinatorsAvailable"] = presenter.Determinators.IsAvailible,
                ["determinatorsSelected"] = presenter.Determinators.IsSelected(),
                ["skillsAvailable"] = presenter.Skills.IsAvailible,
                ["skillsSelected"] = presenter.Skills.IsSelected(),
                ["abilitiesAvailable"] = presenter.Abilities.IsAvailible,
                ["abilitiesUnlocked"] = presenter.Abilities.IsUnlocked,
                ["abilitiesSelected"] = presenter.Abilities.IsSelected(),
                ["skillPointsRemaining"] = backend == null ? -1 : backend.State.SkillPointsRemaining,
                ["blockers"] = backend == null ? new JArray() : FavoredClassLevelUpHarness.Blockers(backend),
            };
        }

        private static FeatureSelectionState FcbCensusRewardState(LevelUpController backend, FcbCensusVisit visit)
        {
            return backend.State.Selections.FirstOrDefault(value => !value.Selected &&
                ReferenceEquals(value.Selection, visit.Reward));
        }

        /// <summary>The same selection state across a level-up state rebuild.</summary>
        private static bool SameFcbCensusState(FeatureSelectionState a, FeatureSelectionState b)
        {
            return a != null && b != null && (ReferenceEquals(a, b) ||
                (ReferenceEquals(a.Selection, b.Selection) && a.Index == b.Index));
        }

        /// <summary>Owned targets reveal the leaves that name them (performances, revelations, bloodline powers).</summary>
        private static void GrantFcbCensusTargets(UnitEntityData unit, FcbCensusVisit visit)
        {
            var library = BlueprintBootstrap.Library;
            foreach (FavoredClassLeafPair pair in visit.Pairs.Where(value => value.TargetKey != null))
            {
                var guids = new List<string>();
                switch (pair.Effect.Id)
                {
                    case FavoredClassCatalog.EffectPerformanceRange:
                        guids.Add(FavoredClassPerformanceManifest.For(pair.TargetKey).FeatureGuid);
                        break;
                    case FavoredClassCatalog.EffectSelectedRevelation:
                        guids.Add(FavoredClassRevelationManifest.For(pair.TargetKey).FeatureGuids[0]);
                        break;
                    case FavoredClassCatalog.EffectSelectedBloodlinePower:
                        FavoredClassSelectedPowerLevel power = pair.Full.GetComponent<FavoredClassSelectedPowerLevel>();
                        if (power != null && power.PowerFeature != null)
                            guids.Add(power.PowerFeature.AssetGuid);
                        guids.Add(FavoredClassLeafCatalog.EligibleBloodlines(pair.TargetKey).Value.First(guid =>
                            library.BlueprintsByAssetId.ContainsKey(guid)));
                        break;
                }
                foreach (string guid in guids)
                {
                    BlueprintScriptableObject raw;
                    if (library.BlueprintsByAssetId.TryGetValue(guid, out raw) && raw is BlueprintFeature &&
                        !unit.Descriptor.HasFact((BlueprintFeature)raw))
                        unit.Descriptor.AddFact((BlueprintFeature)raw);
                }
            }
        }

        /// <summary>Refreshes the open screen after backend choices, as its own feature setter does.</summary>
        private static void RefreshFcbCensusScreen(CharacterBuildController presenter)
        {
            presenter.Determinators.IsDirty = true;
            presenter.Abilities.IsDirty = true;
            presenter.Skills.IsDirty = true;
            typeof(CharacterBuildController).GetMethod("DefineAvailibleData", FcbCensusMembers)
                .Invoke(presenter, null);
            typeof(CharacterBuildController).GetMethod("SetupUI", FcbCensusMembers).Invoke(presenter, null);
        }

        /// <summary>Switches the phase's collection to the state through its own switch item.</summary>
        private static string ShowFcbCensusCollection(CharBPhaseFeatures phase, FeatureSelectionState state,
            out CharBFeatureSelector selector, out CharBSelectionSwitchItem item)
        {
            selector = typeof(CharBPhaseFeatures).GetField("m_Selector", FcbCensusMembers).GetValue(phase)
                as CharBFeatureSelector;
            var switcher = typeof(CharBPhaseFeatures).GetField("m_CollectionSwitcher", FcbCensusMembers)
                .GetValue(phase) as CharBSelectionSwitchFeatures;
            item = null;
            if (selector == null || switcher == null)
                return "the phase has no selector or collection switcher";
            var showed = typeof(CharBSelectionSwitchFeatures).GetField("m_ShowedFeatureCollections",
                FcbCensusMembers).GetValue(switcher) as List<FeatureSelectionState>;
            int index = showed == null ? -1 : showed.FindIndex(value => ReferenceEquals(value, state) ||
                (ReferenceEquals(value.Selection, state.Selection) && value.Index == state.Index));
            if (index < 0 || index >= switcher.Items.Count)
                return "the selector is not in the phase's collection switcher";
            item = switcher.Items[index];
            if (!item.Toggle.isOn)
                item.Toggle.isOn = true;
            switcher.OnChangeSelectedItem(item);
            return null;
        }

        private static Toggle FcbCensusShowAll(CharBFeatureSelector selector)
        {
            if (selector == null || selector.Filter == null)
                return null;
            var toggle = typeof(CharBSelectorFilter).GetField("m_ShowAllButton", FcbCensusMembers)
                .GetValue(selector.Filter) as Toggle;
            return toggle != null && toggle.isActiveAndEnabled && toggle.interactable ? toggle : null;
        }

        /// <summary>The rendered rows of one selector: exact icons, non-empty, every offered leaf present.</summary>
        private static string CaptureFcbCensusRows(FcbCensusVisit visit, FeatureSelectionState state,
            LevelUpController backend, CharBFeatureSelector selector, CharBSelectionSwitchItem switchItem,
            JObject record, HashSet<BlueprintFeature> rendered)
        {
            var problems = new List<string>();
            if (!SameFcbCensusState(selector.SelectorLayerBody.CurrentSelectionState, state))
                problems.Add("the selector does not show the reward state");
            CharBuildSelectorItem[] rows = selector.SelectorLayerBody.SelectorItems.Where(value => value != null &&
                value.gameObject.activeInHierarchy && SameFcbCensusState(value.FeatureSelection, state) &&
                value.Feature != null && value.Feature.Feature != null).ToArray();
            var ours = new HashSet<BlueprintFeature>(visit.Pairs.SelectMany(pair => pair.Leaves));
            var allOurs = new HashSet<BlueprintFeature>(BlueprintBootstrap.FavoredClassLeaves.Pairs
                .SelectMany(pair => pair.Leaves));
            var rowRecords = new JArray();
            var hostPlaceholders = new JArray();
            foreach (CharBuildSelectorItem row in rows)
            {
                var icon = typeof(CharBuildSelectorItem).GetField("m_ItemIcon", FcbCensusMembers).GetValue(row)
                    as Image;
                var acronym = typeof(CharBuildSelectorItem).GetField("m_AcronimText", FcbCensusMembers)
                    .GetValue(row) as TextMeshProUGUI;
                BlueprintFeature feature = row.Feature.Feature;
                bool exact = row.Feature.Icon != null && icon != null && icon.isActiveAndEnabled &&
                    ReferenceEquals(icon.sprite, row.Feature.Icon) &&
                    (acronym == null || !acronym.gameObject.activeSelf);
                bool kmg = allOurs.Contains(feature);
                rowRecords.Add(new JObject
                {
                    ["feature"] = feature.name,
                    ["kmg"] = kmg,
                    ["icon"] = row.Feature.Icon == null ? null : row.Feature.Icon.name,
                    ["exactIcon"] = exact,
                    ["selectable"] = row.Toggle.interactable,
                });
                // KMG rows must show their own icon; the host's own rewards
                // keep the host's presentation and are recorded.
                if (!exact && kmg)
                    problems.Add("row " + feature.name + " shows no exact icon");
                else if (!exact)
                    hostPlaceholders.Add(feature.name);
                if (kmg && exact && ours.Contains(feature))
                    rendered.Add(feature);
            }
            var offered = new JArray();
            foreach (BlueprintFeature leaf in ours)
                if (FavoredClassLevelUpHarness.CanSelect(backend, state, leaf))
                {
                    offered.Add(leaf.name);
                    if (!rows.Any(row => ReferenceEquals(row.Feature.Feature, leaf)))
                        problems.Add("offered leaf " + leaf.name + " is not rendered");
                }
            if (rows.Length == 0)
                problems.Add("the selector is empty");
            if (offered.Count == 0)
                problems.Add("the native backend offers none of this route's leaves");
            var switchAcronym = switchItem == null ? null : typeof(CharBSelectionSwitchItem).GetField("m_AcronimText",
                FcbCensusMembers).GetValue(switchItem) as TextMeshProUGUI;
            record["rows"] = rowRecords;
            record["offered"] = offered;
            record["hostRowsWithoutIcon"] = hostPlaceholders;
            record["selector"] = new JObject
            {
                ["selection"] = visit.Reward.name,
                ["provider"] = "Favored Class host",
                ["icon"] = visit.Reward.Icon == null ? null : visit.Reward.Icon.name,
                ["switchPlaceholder"] = switchAcronym != null && switchAcronym.gameObject.activeSelf,
            };
            record["problems"] = new JArray(problems);
            return problems.Count == 0 ? null : string.Join("; ", problems.ToArray());
        }

        private IEnumerable<object> FcbCensusMostlyHuman(ElementalMostlyHumanRaceBlueprints ancestry,
            JArray records, List<string> failures)
        {
            var ui = Game.Instance.UI;
            CharacterBuildController presenter = ui.CharacterBuildController;
            LevelUpController priorBackend = ui.LevelUpController;
            UnitDescriptor priorUnit = presenter.Unit;
            var record = new JObject { ["race"] = ancestry.Race.name };
            records.Add(record);
            UnitEntityData unit = null;
            LevelUpController backend = null;
            Toggle showAll = null;
            bool originalShowAll = false;
            string problem = null;
            try
            {
                unit = new ChargenUnit(BlueprintRoot.Instance.CustomCompanion).Unit;
                presenter.HandleLevelUpStart(unit.Descriptor, null, () => { }, LevelUpState.CharBuildMode.CharGen);
                backend = presenter.LevelUpController;
                if (backend == null || !presenter.IsShow || backend.State.NextLevel != 1)
                    problem = "the actual creator did not open";
            }
            catch (Exception exception)
            {
                problem = "open: " + exception.Message;
            }
            // The creator's own order: Portrait, Race, Class, then Heritage.
            if (problem == null)
            {
                for (int frame = 0; frame < FcbCensusSettle; frame++) yield return null;
                try { presenter.SetPortrait(BlueprintRoot.Instance.CharGen.Portraits.First(value => value != null)); }
                catch (Exception exception) { problem = "portrait: " + exception.Message; }
            }
            if (problem == null)
            {
                foreach (object step in FcbCensusAdvanceTo(presenter, CharBPhase.Type.Race, record)) yield return step;
                try
                {
                    presenter.SetRace(ancestry.Race);
                    presenter.SetGender(Kingmaker.Blueprints.Gender.Male);
                }
                catch (Exception exception)
                {
                    problem = "race: " + exception.GetType().Name + ": " + exception.Message;
                }
            }
            if (problem == null)
            {
                for (int frame = 0; frame < FcbCensusSettle; frame++) yield return null;
                foreach (object step in FcbCensusAdvanceTo(presenter, CharBPhase.Type.ClassInChargen, record))
                    yield return step;
                try { presenter.SetClass(BlueprintBootstrap.GunslingerClass.CharacterClass); }
                catch (Exception exception) { problem = "class: " + exception.GetType().Name + ": " + exception.Message; }
            }
            FeatureSelectionState state = null;
            if (problem == null)
            {
                for (int frame = 0; frame < FcbCensusSettle; frame++) yield return null;
                state = backend.State.Selections.FirstOrDefault(value => !value.Selected &&
                    ReferenceEquals(value.Selection, ancestry.Selection));
                if (state == null)
                    problem = "the creator offered no Mostly Human selector";
                else
                {
                    foreach (object step in FcbCensusAdvanceTo(presenter, CharBPhase.Type.Determinator, record))
                        yield return step;
                    if (presenter.CurrentPhase != CharBPhase.Type.Determinator)
                    {
                        record["phaseTerms"] = FcbCensusPhaseTerms(presenter, backend);
                        problem = "the Heritage phase did not open: " + presenter.CurrentPhase;
                    }
                }
            }
            CharBFeatureSelector selector = null;
            CharBSelectionSwitchItem switchItem = null;
            if (problem == null)
            {
                problem = ShowFcbCensusCollection(presenter.Determinators, state, out selector, out switchItem);
                for (int frame = 0; frame < FcbCensusSettle; frame++) yield return null;
            }
            if (problem == null)
            {
                showAll = FcbCensusShowAll(selector);
                if (showAll != null)
                {
                    originalShowAll = showAll.isOn;
                    showAll.isOn = true;
                }
                for (int frame = 0; frame < 2 * FcbCensusSettle; frame++) yield return null;
                try
                {
                    CharBuildSelectorItem[] rows = selector.SelectorLayerBody.SelectorItems.Where(value =>
                        value != null && value.gameObject.activeInHierarchy &&
                        ReferenceEquals(value.FeatureSelection, state) && value.Feature != null &&
                        value.Feature.Feature != null).ToArray();
                    var rowRecords = new JArray();
                    var problems = new List<string>();
                    foreach (BlueprintFeature expected in new[] { ancestry.Standard, ancestry.Trait })
                    {
                        CharBuildSelectorItem row = rows.FirstOrDefault(value =>
                            ReferenceEquals(value.Feature.Feature, expected));
                        var icon = row == null ? null : typeof(CharBuildSelectorItem).GetField("m_ItemIcon",
                            FcbCensusMembers).GetValue(row) as Image;
                        var acronym = row == null ? null : typeof(CharBuildSelectorItem).GetField("m_AcronimText",
                            FcbCensusMembers).GetValue(row) as TextMeshProUGUI;
                        bool exact = row != null && expected.Icon != null && icon != null &&
                            icon.isActiveAndEnabled && ReferenceEquals(icon.sprite, expected.Icon) &&
                            (acronym == null || !acronym.gameObject.activeSelf);
                        rowRecords.Add(new JObject { ["feature"] = expected.name, ["rendered"] = row != null,
                            ["icon"] = expected.Icon == null ? null : expected.Icon.name, ["exactIcon"] = exact });
                        if (!exact)
                            problems.Add("row " + expected.name + " is missing or shows no exact icon");
                    }
                    var switchAcronym = switchItem == null ? null : typeof(CharBSelectionSwitchItem).GetField(
                        "m_AcronimText", FcbCensusMembers).GetValue(switchItem) as TextMeshProUGUI;
                    bool selectorExact = ancestry.Selection.Icon != null && switchItem != null &&
                        switchItem.Icon != null && ReferenceEquals(switchItem.Icon.sprite, ancestry.Selection.Icon) &&
                        (switchAcronym == null || !switchAcronym.gameObject.activeSelf);
                    if (!selectorExact)
                        problems.Add("the Mostly Human selector shows no exact icon");
                    if (rows.Length != 2)
                        problems.Add("the selector renders " + rows.Length + " rows instead of 2");
                    record["rows"] = rowRecords;
                    record["selectorIcon"] = ancestry.Selection.Icon == null ? null : ancestry.Selection.Icon.name;
                    record["selectorExact"] = selectorExact;
                    record["problems"] = new JArray(problems);
                    if (problems.Count != 0)
                        problem = string.Join("; ", problems.ToArray());
                }
                catch (Exception exception)
                {
                    problem = "capture: " + exception.GetType().Name + ": " + exception.Message;
                }
            }
            if (problem != null)
            {
                failures.Add(ancestry.Race.name + ": " + problem);
                record["failure"] = problem;
            }
            try { if (showAll != null) showAll.isOn = originalShowAll; } catch (Exception) { }
            CloseFcbCensus(presenter, backend, priorBackend, priorUnit);
            for (int frame = 0; frame < 4; frame++) yield return null;
            try
            {
                if (unit != null && !unit.Destroyed && unit.Descriptor.Body != null)
                    unit.Dispose();
            }
            catch (Exception) { }
        }

        private static void CloseFcbCensus(CharacterBuildController presenter, LevelUpController backend,
            LevelUpController priorBackend, UnitDescriptor priorUnit)
        {
            try
            {
                if (backend != null && presenter.IsShow && ReferenceEquals(presenter.LevelUpController, backend))
                    presenter.Show(false);
            }
            catch (Exception) { }
            try { if (backend != null) backend.Cancel(); } catch (Exception) { }
            Game.Instance.UI.LevelUpController = priorBackend;
            presenter.Unit = priorUnit;
        }

        /// <summary>A detached real-view character in the cross-scene state, outside the party.</summary>
        private static UnitEntityData SpawnFcbCensusUnit(UnitEntityData anchor, BlueprintRace race,
            BlueprintCharacterClass characterClass, string name)
        {
            Game game = Game.Instance;
            var dollState = new DollState();
            dollState.SetGender(anchor.Descriptor.Gender);
            dollState.SetRace(race);
            dollState.SetClass(characterClass);
            var doll = dollState.CreateData();
            var view = doll.CreateUnitView(false);
            if (view == null)
                throw new InvalidOperationException("The census character has no real view.");
            view.Blueprint = game.BlueprintRoot.DefaultPlayerCharacter;
            view.UniqueId = Guid.NewGuid().ToString();
            view.transform.position = anchor.Position;
            var unit = game.EntityCreator.SpawnEntityWithView(view, game.Player.CrossSceneState) as UnitEntityData;
            if (unit == null)
                throw new InvalidOperationException("The census character did not enter the cross-scene state.");
            game.EntityCreator.Tick();
            unit.Descriptor.Doll = doll;
            unit.Descriptor.CustomGender = anchor.Descriptor.Gender;
            unit.Descriptor.CustomName = name;
            unit.Stats.Strength.BaseValue = 14;
            unit.Stats.Dexterity.BaseValue = 14;
            unit.Stats.Constitution.BaseValue = 14;
            unit.Stats.Intelligence.BaseValue = 14;
            unit.Stats.Wisdom.BaseValue = 14;
            unit.Stats.Charisma.BaseValue = 14;
            unit.Descriptor.TurnOn();
            return unit;
        }

        private static void DestroyFcbCensusUnit(UnitEntityData unit)
        {
            if (unit == null)
                return;
            try
            {
                UnitEntityData pet = unit.Descriptor.Pet;
                if (pet != null)
                {
                    pet.Descriptor.SetMaster(null);
                    pet.Destroy();
                }
                unit.Destroy();
                Game.Instance.EntityDestroyer.Tick();
            }
            catch (Exception) { }
        }
    }
}
