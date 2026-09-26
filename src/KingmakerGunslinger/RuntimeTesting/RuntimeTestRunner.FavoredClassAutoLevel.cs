using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.FavoredClass;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private const string FcbLinziUnitGuid = "77c11edb92ce0fd408ad96b40fd27121";
        private const string FcbOctaviaUnitGuid = "f9161aa0b3f519c47acbce01f53ee217";
        private const string FcbErinyesUnitGuid = "bf08d0f66ffc4ed78dd40e5f7e68ae9d";

        private static readonly MethodInfo FcbNativeAddAction = typeof(LevelUpController).GetMethod("AddAction",
            BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(ILevelUpAction), typeof(bool) }, null);

        // E15 natively, save-free and settings-free. A stored level plan
        // (auto-level for companions, a pregen, an imported companion) is
        // applied by the controller's own constructor; the native trigger used
        // here is the imported-companion plan part, so the owner's auto-level
        // setting is never read or written. Each plan is applied once through
        // the scoped native path and once through the same native AddAction
        // calls made without the plan scope (the behavior before the fix):
        // - a recorded Ifrit Sorcerer level whose reward targets the ray chosen
        //   at that same level keeps the reward only in the scoped path;
        // - a host-only plan (the Fighter's hit point reward) and a story
        //   companion's own stored plan are accepted identically by both;
        // - the Gunslinger has no default build, so it never has a stored plan;
        // - an NPC class leveled by the game (a summon) has no plan, no favored
        //   class and no reward.
        private RuntimeTestResult RunFavoredClassAutoLevel()
        {
            var assertions = new List<RuntimeTestAssertion>();
            FavoredClassIntegrationStatus status = FavoredClassIntegrationStatusRegistry.Current;
            FavoredClassHostHandles host = FavoredClassIntegrationCoordinator.Host;
            FavoredClassBlueprintSet leaves = BlueprintBootstrap.FavoredClassLeaves;
            bool ready = status.Availability == FavoredClassIntegrationAvailability.Published &&
                host != null && leaves != null && FavoredClassRuntime.MechanicsEnabled &&
                FavoredClassPendingPicks.Installed && FavoredClassPendingPicks.PlanInstalled &&
                FcbNativeAddAction != null && !Game.Instance.State.Units.All.Any() &&
                !Game.Instance.Player.Party.Any() && Game.Instance.CurrentlyLoadedArea == null;
            assertions.Add(Assertion("fcb-auto-level-ready",
                "the exact host is published, mechanics are enabled, both scoped hooks are installed and the save-free world is empty",
                status + ";replay=" + FavoredClassPendingPicks.Installed + ";plan=" +
                FavoredClassPendingPicks.PlanInstalled, ready,
                "FavoredClassIntegrationStatusRegistry; FavoredClassPendingPicks; empty main-menu world"));
            if (!ready)
                return CreateResult(RuntimeTestStatuses.Fail, assertions, null);
            var evidence = new JObject();
            var failures = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            foreach (string key in new[] { "sameLevel", "control", "host", "companion", "gunslinger", "npc", "cleanup" })
                failures[key] = new List<string>();
            var units = new List<UnitEntityData>();
            try
            {
                var library = BlueprintBootstrap.Library;
                var kmgLeaves = new HashSet<string>(leaves.Pairs.SelectMany(pair => new[] { pair.Full, pair.Partial })
                    .Where(leaf => leaf != null).Select(leaf => leaf.AssetGuid), StringComparer.Ordinal);
                Func<UnitEntityData> create = () =>
                {
                    UnitEntityData unit = FavoredClassLevelUpHarness.CreateUnit(14);
                    units.Add(unit);
                    return unit;
                };
                try
                {
                    evidence["sameLevel"] = ObserveSameLevelPlan(host, leaves, create, failures);
                }
                catch (Exception exception)
                {
                    failures["sameLevel"].Add(exception.GetType().Name + ": " + exception.Message);
                }
                try
                {
                    evidence["hostPlan"] = ObserveHostOnlyPlan(host, kmgLeaves, create, failures["host"]);
                }
                catch (Exception exception)
                {
                    failures["host"].Add(exception.GetType().Name + ": " + exception.Message);
                }
                try
                {
                    evidence["storyCompanion"] = ObserveStoryCompanionPlan(host, kmgLeaves, units,
                        failures["companion"]);
                }
                catch (Exception exception)
                {
                    failures["companion"].Add(exception.GetType().Name + ": " + exception.Message);
                }
                BlueprintCharacterClass gunslinger = BlueprintBootstrap.GunslingerClass == null ? null :
                    BlueprintBootstrap.GunslingerClass.CharacterClass;
                evidence["gunslinger"] = gunslinger == null ? (JToken)"<absent>" : new JObject
                {
                    ["defaultBuild"] = gunslinger.DefaultBuild == null ? "<none>" : gunslinger.DefaultBuild.name,
                    ["hasDefaultProgression"] = gunslinger.HasDefaultProgression
                };
                if (gunslinger == null || gunslinger.DefaultBuild != null || gunslinger.HasDefaultProgression)
                    failures["gunslinger"].Add("the Gunslinger has a default build, so a stored plan could skip its choices");
                try
                {
                    evidence["npcClass"] = ObserveNpcClassUnit(host, kmgLeaves, units, failures["npc"]);
                }
                catch (Exception exception)
                {
                    failures["npc"].Add(exception.GetType().Name + ": " + exception.Message);
                }
            }
            finally
            {
                foreach (UnitEntityData unit in units)
                    try { unit.Dispose(); } catch (Exception) { }
                bool restored = FavoredClassPendingPicks.Depth == 0 && !Game.Instance.State.Units.All.Any() &&
                    !Game.Instance.Player.Party.Any();
                evidence["restored"] = restored;
                evidence["scopeDepthAfter"] = FavoredClassPendingPicks.Depth;
                if (!restored)
                    failures["cleanup"].Add("a scope stayed open or a unit stayed in the save-free world");
            }
            string evidencePath = WriteFavoredClassEvidence("favored-class-auto-level.json", evidence);
            Action<string, string, string, string> add = (key, id, expectation, source) => assertions.Add(Assertion(id,
                expectation, Describe(null, failures[key]), failures[key].Count == 0, source));
            add("sameLevel", "fcb-auto-level-same-level-plan",
                "a recorded Ifrit Sorcerer level that picks the Fire Ray counter before the ray it targets (the plan's priority order) is applied whole by the constructor's plan: the reward is kept, the visit is complete and still automatic, no scope stays open, and the confirmed level holds the counter and the ray",
                "LevelUpController constructor ApplyLevelUpPlan (imported-plan trigger); ApplyLevelup");
            add("control", "fcb-auto-level-native-control",
                "the same plan's native AddAction calls made without the plan scope reject exactly the reward (the level stays incomplete on the favored-class reward), the defect the scoped plan fixes",
                "LevelUpController.AddAction(action, ignoreOrder: true) inside ApplyingPlanScope");
            add("host", "fcb-auto-level-host-plan",
                "a host-only plan (a Human Fighter's hit point reward) is accepted identically with and without the plan scope, is complete and automatic, offers no KMG counter, and the confirmed level holds the reward",
                "recorded LevelPlanData; LevelUpController constructor; ApplyLevelup");
            add("companion", "fcb-auto-level-story-companion",
                "a story companion's own stored next-level plan (with the host's reward) is accepted identically with and without the plan scope, and no stored plan picks or grants a KMG counter",
                "ChargenUnit of the companion blueprint (AddClassLevels plans); LevelUpController constructor");
            add("gunslinger", "fcb-auto-level-gunslinger",
                "the Gunslinger has no default build, so it never has a stored plan; its counters are chosen in the player's own visit",
                "BlueprintCharacterClass.DefaultBuild and HasDefaultProgression");
            add("npc", "fcb-auto-level-npc-class",
                "an NPC class leveled by the game (the Erinyes summon's outsider levels) has no stored plan, no favored class, no host reward and no KMG counter",
                "ChargenUnit of the summon blueprint (AddClassLevels, non-player faction)");
            add("cleanup", "fcb-auto-level-cleanup", "every unit is disposed and no plan or replay scope stays open",
                "FavoredClassPendingPicks.Depth; empty main-menu world");
            assertions.Add(Assertion("loaded-mod-version", _request.ExpectedModVersion,
                _context.ModEntry.Info.Version,
                _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                "Unity Mod Manager ModEntry.Info.Version"));
            RuntimeTestResult result = CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions, null);
            result.EvidenceFiles.Add(evidencePath);
            return result;
        }

        private JObject ObserveSameLevelPlan(FavoredClassHostHandles host, FavoredClassBlueprintSet leaves,
            Func<UnitEntityData> create, Dictionary<string, List<string>> failures)
        {
            var library = BlueprintBootstrap.Library;
            var row = new JObject();
            BlueprintCharacterClass sorcerer = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(library,
                FcbSorcererClassGuid, "Sorcerer");
            BlueprintRace ifrit = BlueprintLibraryLookup.RequireExact<BlueprintRace>(library,
                FavoredClassRaceIdentities.ForAncestry(FavoredClassAncestry.Ifrit).RaceGuid, "Ifrit");
            BlueprintFeature bloodline = BlueprintLibraryLookup.RequireExact<BlueprintProgression>(library,
                FcbFireBloodlineGuid, "Fire bloodline");
            BlueprintScriptableObject rayBlueprint;
            library.BlueprintsByAssetId.TryGetValue(FcbFireRayFeatureGuid, out rayBlueprint);
            var ray = rayBlueprint as BlueprintFeature;
            if (ray == null)
                throw new InvalidOperationException("Missing provider feature " + FcbFireRayFeatureGuid);
            BlueprintFeatureSelection bonus = host.BonusSelectionFor(sorcerer.AssetGuid);
            FavoredClassLeafPair pair = leaves.Pair(FavoredClassCatalog.EffectSelectedBloodlinePower, "FireRay");

            // The player's own level-1 visit, recorded as the game records it.
            LevelPlanData plan;
            BlueprintFeature leaf;
            UnitEntityData recorder = create();
            LevelUpController recording = null;
            try
            {
                recording = FavoredClassLevelUpHarness.Open(recorder.Descriptor, ifrit, sorcerer,
                    "KMG FCB Auto Level");
                FeatureSelectionState bloodlineState = FavoredClassLevelUpHarness.FindOpenState(recording,
                    FcbBloodlineSelectionGuid);
                if (bloodlineState == null || !FavoredClassLevelUpHarness.Select(recording, bloodlineState, bloodline))
                    throw new InvalidOperationException("the Fire bloodline was not selected");
                FeatureSelectionState rayState = FavoredClassLevelUpHarness.FindOpenState(recording,
                    FcbFireRaySelectionGuid);
                if (rayState == null || !FavoredClassLevelUpHarness.Select(recording, rayState, ray))
                    throw new InvalidOperationException("the Fire Elemental Ray was not selected");
                if (FavoredClassLevelUpHarness.ChooseFavoredClass(recording, sorcerer, row) == null)
                    throw new InvalidOperationException("the favored Sorcerer progression was not chosen");
                FavoredClassLevelUpHarness.FillOthers(recording,
                    new HashSet<string>(StringComparer.Ordinal) { bonus.AssetGuid });
                FillFcbSpells(recording);
                FeatureSelectionState reward = FavoredClassLevelUpHarness.FindOpenState(recording, bonus.AssetGuid);
                leaf = reward == null ? null : FavoredClassLevelUpHarness.CanSelect(recording, reward, pair.Full) ?
                    pair.Full : pair.Partial;
                if (reward == null || !FavoredClassLevelUpHarness.Select(recording, reward, leaf))
                    throw new InvalidOperationException("the Fire Ray counter was not selectable at level 1");
                row["recordedComplete"] = recording.State.IsComplete();
                plan = recording.GetPlan();
            }
            finally
            {
                FavoredClassLevelUpHarness.Close(recording);
            }
            int rewardIndex = FcbPlanIndex(plan, bonus, leaf), rayIndex = FcbPlanIndex(plan, null, ray);
            row["planLevel"] = plan.Level;
            row["planActions"] = FcbDescribePlan(plan);
            row["rewardIndex"] = rewardIndex;
            row["rayIndex"] = rayIndex;
            row["leaf"] = leaf.name;
            if (!(bool)row["recordedComplete"] || rewardIndex < 0 || rayIndex < 0)
                failures["sameLevel"].Add("the recorded level is incomplete or lacks the reward or the ray pick");
            else if (rewardIndex > rayIndex)
                failures["sameLevel"].Add("the recorded plan lists the ray before the reward, so it cannot show the bypass");

            // The scoped path: the constructor applies the imported plan.
            UnitEntityData imported = create();
            FcbMarkImportedPlan(imported, plan.Level);
            imported.Descriptor.Progression.AddLevelPlan(plan);
            LevelUpController applied = null;
            try
            {
                applied = FavoredClassLevelUpHarness.OpenBare(imported.Descriptor);
                row["scoped"] = FcbPlanAcceptance(applied, plan);
                row["scopedDepth"] = FavoredClassPendingPicks.Depth;
                bool rewardKept = applied.LevelUpActions.Contains(plan.Actions[Math.Max(rewardIndex, 0)]);
                row["scopedRewardKept"] = rewardKept;
                row["scopedAutomatic"] = applied.IsAutoLevelup;
                var confirmation = new JObject();
                bool confirmed = FavoredClassLevelUpHarness.Confirm(applied, imported.Descriptor, confirmation);
                row["scopedConfirmation"] = confirmation;
                row["confirmedLeafRank"] = FavoredClassLevelUpHarness.Rank(imported.Descriptor, leaf);
                row["confirmedRay"] = imported.Descriptor.HasFact(ray);
                if (!rewardKept || (int)row["scoped"]["rejected"] != 0)
                    failures["sameLevel"].Add("the scoped plan rejected " + row["scoped"]["rejectedActions"]);
                if (!(bool)row["scopedAutomatic"] || !confirmed)
                    failures["sameLevel"].Add("the scoped plan's visit is not complete and automatic");
                if ((int)row["scopedDepth"] != 0)
                    failures["sameLevel"].Add("a plan scope stayed open after the constructor");
                if ((int)row["confirmedLeafRank"] != 1 || !(bool)row["confirmedRay"])
                    failures["sameLevel"].Add("the confirmed level does not hold the counter and the ray");
            }
            finally
            {
                FavoredClassLevelUpHarness.Close(applied);
            }

            // The native control: the same AddAction calls without the scope.
            UnitEntityData native = create();
            LevelUpController control = null;
            try
            {
                control = FavoredClassLevelUpHarness.OpenBare(native.Descriptor);
                row["controlPreapplied"] = control.LevelUpActions.Count;
                FcbApplyUnscoped(control, plan);
                row["control"] = FcbPlanAcceptance(control, plan);
                bool rewardKept = control.LevelUpActions.Contains(plan.Actions[Math.Max(rewardIndex, 0)]);
                row["controlRewardKept"] = rewardKept;
                row["controlComplete"] = control.State.IsComplete();
                row["controlBlockers"] = FavoredClassLevelUpHarness.Blockers(control);
                var rejected = (JArray)row["control"]["rejectedIndices"];
                if ((int)row["controlPreapplied"] != 0)
                    failures["control"].Add("the control's constructor applied actions before the unscoped plan");
                if (rewardKept || (bool)row["controlComplete"] || rejected.Count != 1 ||
                    (int)rejected[0] != rewardIndex)
                    failures["control"].Add("the unscoped plan did not reject exactly the reward (rejected " +
                        row["control"]["rejectedActions"] + ")");
            }
            finally
            {
                FavoredClassLevelUpHarness.Close(control);
            }
            return row;
        }

        private JObject ObserveHostOnlyPlan(FavoredClassHostHandles host, HashSet<string> kmgLeaves,
            Func<UnitEntityData> create, IList<string> failures)
        {
            var library = BlueprintBootstrap.Library;
            var row = new JObject();
            BlueprintCharacterClass fighter = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(library,
                FcbFighterClassGuid, "Fighter");
            BlueprintRace human = BlueprintLibraryLookup.RequireExact<BlueprintRace>(library, FcbHumanRace, "Human");
            BlueprintFeatureSelection bonus = host.BonusSelectionFor(fighter.AssetGuid);
            LevelPlanData plan;
            UnitEntityData recorder = create();
            LevelUpController recording = null;
            try
            {
                recording = FavoredClassLevelUpHarness.Open(recorder.Descriptor, human, fighter,
                    "KMG FCB Auto Level Host");
                if (FavoredClassLevelUpHarness.ChooseFavoredClass(recording, fighter, row) == null)
                    throw new InvalidOperationException("the favored Fighter progression was not chosen");
                FavoredClassLevelUpHarness.FillOthers(recording,
                    new HashSet<string>(StringComparer.Ordinal) { bonus.AssetGuid });
                FeatureSelectionState reward = FavoredClassLevelUpHarness.FindOpenState(recording, bonus.AssetGuid);
                if (reward == null)
                    throw new InvalidOperationException("the Fighter's favored-class reward did not open");
                IFeatureSelectionItem[] kmgItems = FavoredClassLevelUpHarness.Items(recording, reward)
                    .Where(item => kmgLeaves.Contains(item.Feature.AssetGuid)).ToArray();
                row["kmgListed"] = kmgItems.Length;
                row["kmgOffered"] = new JArray(kmgItems.Where(item =>
                    FavoredClassLevelUpHarness.CanSelect(recording, reward, item.Feature))
                    .Select(item => item.Feature.name));
                if (!FavoredClassLevelUpHarness.Select(recording, reward, host.GenericHitPoint))
                    throw new InvalidOperationException("the host's hit point reward was not selectable");
                row["recordedComplete"] = recording.State.IsComplete();
                plan = recording.GetPlan();
            }
            finally
            {
                FavoredClassLevelUpHarness.Close(recording);
            }
            row["planActions"] = FcbDescribePlan(plan);
            if (((JArray)row["kmgOffered"]).Count != 0)
                failures.Add("a KMG counter was offered in the Human Fighter's reward: " + row["kmgOffered"]);
            JObject scoped, native;
            UnitEntityData imported = create();
            FcbMarkImportedPlan(imported, plan.Level);
            imported.Descriptor.Progression.AddLevelPlan(plan);
            LevelUpController applied = null;
            try
            {
                applied = FavoredClassLevelUpHarness.OpenBare(imported.Descriptor);
                scoped = FcbPlanAcceptance(applied, plan);
                row["scoped"] = scoped;
                row["scopedAutomatic"] = applied.IsAutoLevelup;
                var confirmation = new JObject();
                row["scopedConfirmed"] = FavoredClassLevelUpHarness.Confirm(applied, imported.Descriptor,
                    confirmation);
                row["scopedConfirmation"] = confirmation;
                row["confirmedRewardRank"] = FavoredClassLevelUpHarness.Rank(imported.Descriptor, host.GenericHitPoint);
            }
            finally
            {
                FavoredClassLevelUpHarness.Close(applied);
            }
            UnitEntityData unscoped = create();
            LevelUpController control = null;
            try
            {
                control = FavoredClassLevelUpHarness.OpenBare(unscoped.Descriptor);
                FcbApplyUnscoped(control, plan);
                native = FcbPlanAcceptance(control, plan);
                row["native"] = native;
            }
            finally
            {
                FavoredClassLevelUpHarness.Close(control);
            }
            if (!JToken.DeepEquals(scoped["rejectedIndices"], native["rejectedIndices"]))
                failures.Add("the plan scope changed which host-only plan actions were accepted");
            if ((int)scoped["rejected"] != 0 || !(bool)row["scopedAutomatic"] || !(bool)row["scopedConfirmed"] ||
                (int)row["confirmedRewardRank"] != 1)
                failures.Add("the host-only plan was not applied whole, automatic and confirmed with its reward");
            return row;
        }

        private JObject ObserveStoryCompanionPlan(FavoredClassHostHandles host, HashSet<string> kmgLeaves,
            List<UnitEntityData> units, IList<string> failures)
        {
            var library = BlueprintBootstrap.Library;
            var bonusSelections = new HashSet<BlueprintScriptableObject>(host.BonusSelections.Select(value =>
                (BlueprintScriptableObject)value.Value));
            var rows = new JArray();
            bool observed = false;
            foreach (string guid in new[] { FcbLinziUnitGuid, FcbOctaviaUnitGuid })
            {
                var row = new JObject { ["unit"] = guid };
                rows.Add(row);
                BlueprintScriptableObject found;
                BlueprintUnit blueprint = library.BlueprintsByAssetId.TryGetValue(guid, out found) ?
                    found as BlueprintUnit : null;
                if (blueprint == null)
                {
                    row["result"] = "blueprint absent";
                    continue;
                }
                row["name"] = blueprint.name;
                Func<UnitEntityData> create = () =>
                {
                    UnitEntityData unit = new Kingmaker.UI.LevelUp.ChargenUnit(blueprint).Unit;
                    units.Add(unit);
                    if (!unit.Descriptor.IsTurnedOn)
                        unit.Descriptor.TurnOn();
                    return unit;
                };
                UnitEntityData probe = create();
                int level = probe.Descriptor.Progression.CharacterLevel;
                var planned = Enumerable.Range(1, 20).Select(value => probe.Descriptor.Progression.GetLevelPlan(value))
                    .Where(value => value != null).ToArray();
                row["characterLevel"] = level;
                row["playerFaction"] = probe.Descriptor.IsPlayerFaction;
                row["plannedLevels"] = new JArray(planned.Select(value => value.Level));
                int kmgPicks = planned.Sum(value => value.Actions.OfType<SelectFeature>().Count(pick =>
                    pick.Item != null && pick.Item.Feature != null && kmgLeaves.Contains(pick.Item.Feature.AssetGuid)));
                row["kmgPicksInPlans"] = kmgPicks;
                if (kmgPicks != 0)
                    failures.Add(blueprint.name + ": a stored plan picks a KMG counter");
                LevelPlanData next = probe.Descriptor.Progression.GetLevelPlan(level + 1);
                if (next == null)
                {
                    row["result"] = "no stored plan for the next level";
                    continue;
                }
                int[] rewardIndices = Enumerable.Range(0, next.Actions.Length).Where(index =>
                    next.Actions[index] is SelectFeature && bonusSelections.Contains(
                        ((SelectFeature)next.Actions[index]).Selection as BlueprintScriptableObject)).ToArray();
                SelectFeature[] rewards = rewardIndices.Select(index => (SelectFeature)next.Actions[index]).ToArray();
                row["hostRewards"] = new JArray(rewards.Select(pick => pick.Item == null || pick.Item.Feature == null ?
                    "<none>" : pick.Item.Feature.name));
                row["planActions"] = FcbDescribePlan(next);
                // Each unit created from the blueprint stores its own plan objects.
                UnitEntityData imported = create();
                LevelPlanData importedPlan = imported.Descriptor.Progression.GetLevelPlan(next.Level);
                FcbMarkImportedPlan(imported, next.Level);
                LevelUpController applied = null;
                JObject scoped, native;
                try
                {
                    if (importedPlan == null || !JToken.DeepEquals(FcbDescribePlan(importedPlan), row["planActions"]))
                        throw new InvalidOperationException("the scoped unit's stored plan differs from the probe's");
                    applied = FavoredClassLevelUpHarness.OpenBare(imported.Descriptor);
                    scoped = FcbPlanAcceptance(applied, importedPlan);
                    row["scoped"] = scoped;
                    row["scopedAutomatic"] = applied.IsAutoLevelup;
                    row["scopedComplete"] = applied.State.IsComplete();
                    row["scopedBlockers"] = FavoredClassLevelUpHarness.Blockers(applied);
                    row["scopedKmgPicked"] = applied.LevelUpActions.OfType<SelectFeature>().Count(pick =>
                        pick.Item != null && pick.Item.Feature != null && kmgLeaves.Contains(pick.Item.Feature.AssetGuid));
                }
                finally
                {
                    FavoredClassLevelUpHarness.Close(applied);
                }
                UnitEntityData unscoped = create();
                LevelPlanData controlPlan = unscoped.Descriptor.Progression.GetLevelPlan(next.Level);
                unscoped.Descriptor.Progression.DropLevelPlans();
                LevelUpController control = null;
                try
                {
                    if (controlPlan == null || !JToken.DeepEquals(FcbDescribePlan(controlPlan), row["planActions"]))
                        throw new InvalidOperationException("the control unit's stored plan differs from the probe's");
                    control = FavoredClassLevelUpHarness.OpenBare(unscoped.Descriptor);
                    row["controlPreapplied"] = control.LevelUpActions.Count;
                    FcbApplyUnscoped(control, controlPlan);
                    native = FcbPlanAcceptance(control, controlPlan);
                    row["native"] = native;
                }
                finally
                {
                    FavoredClassLevelUpHarness.Close(control);
                }
                if ((int)row["controlPreapplied"] != 0)
                    failures.Add(blueprint.name + ": the control's constructor applied a plan");
                if (!JToken.DeepEquals(scoped["rejectedIndices"], native["rejectedIndices"]))
                    failures.Add(blueprint.name + ": the plan scope changed which stored plan actions were accepted");
                var rejected = new HashSet<int>(((JArray)scoped["rejectedIndices"]).Select(value => (int)value));
                if (rewardIndices.Any(rejected.Contains))
                    failures.Add(blueprint.name + ": the host's reward pick was rejected");
                if ((int)row["scopedKmgPicked"] != 0)
                    failures.Add(blueprint.name + ": a KMG counter was picked in the companion's level");
                row["result"] = "observed";
                observed = true;
                break;
            }
            if (!observed)
                failures.Add("no story companion with a stored next-level plan could be observed");
            return new JObject { ["companions"] = rows };
        }

        private static JObject ObserveNpcClassUnit(FavoredClassHostHandles host, HashSet<string> kmgLeaves,
            List<UnitEntityData> units, IList<string> failures)
        {
            var library = BlueprintBootstrap.Library;
            BlueprintUnit blueprint = BlueprintLibraryLookup.RequireExact<BlueprintUnit>(library, FcbErinyesUnitGuid,
                "Erinyes summon");
            UnitEntityData unit = new Kingmaker.UI.LevelUp.ChargenUnit(blueprint).Unit;
            units.Add(unit);
            BlueprintScriptableObject found;
            var favoredChoice = library.BlueprintsByAssetId.TryGetValue(
                FavoredClassLevelUpHarness.FavoredClassSelectionGuid, out found) ? found as BlueprintFeatureSelection : null;
            var favoredProgressions = new HashSet<BlueprintScriptableObject>(favoredChoice == null ?
                Enumerable.Empty<BlueprintScriptableObject>() : favoredChoice.AllFeatures.Cast<BlueprintScriptableObject>());
            var rewards = new HashSet<BlueprintScriptableObject>(host.BonusSelections.SelectMany(value =>
                value.Value.AllFeatures.Cast<BlueprintScriptableObject>()));
            var facts = unit.Descriptor.Progression.Features.Enumerable.Where(fact => fact != null &&
                fact.Blueprint != null).ToArray();
            var row = new JObject
            {
                ["unit"] = blueprint.name,
                ["playerFaction"] = unit.Descriptor.IsPlayerFaction,
                ["classes"] = new JArray(unit.Descriptor.Progression.Classes.Select(value =>
                    value.CharacterClass.name + " " + value.Level)),
                ["plannedLevels"] = Enumerable.Range(1, 20).Count(value =>
                    unit.Descriptor.Progression.GetLevelPlan(value) != null),
                ["favoredClassFacts"] = facts.Count(fact => favoredProgressions.Contains(fact.Blueprint)),
                ["rewardFacts"] = facts.Count(fact => rewards.Contains(fact.Blueprint)),
                ["kmgFacts"] = facts.Count(fact => kmgLeaves.Contains(fact.Blueprint.AssetGuid)),
                ["favoredChoiceResolved"] = favoredChoice != null
            };
            if (favoredChoice == null)
                failures.Add("the host's favored class choice is not resolvable");
            if (!unit.Descriptor.Progression.Classes.Any(value => value.Level > 0))
                failures.Add("the NPC unit has no class levels, so it proves nothing");
            if ((int)row["plannedLevels"] != 0 || (int)row["favoredClassFacts"] != 0 || (int)row["rewardFacts"] != 0 ||
                (int)row["kmgFacts"] != 0)
                failures.Add("the NPC unit has a stored plan, a favored class, a host reward or a KMG counter");
            return row;
        }

        /// <summary>The native imported-companion plan part: the constructor applies its plan without settings.</summary>
        private static void FcbMarkImportedPlan(UnitEntityData unit, int importedLevel)
        {
            Type part = typeof(UnitEntityData).Assembly.GetType("Kingmaker.Dungeon.Units.UnitPartImportableCompanion",
                true);
            MethodInfo ensure = typeof(UnitEntityData).GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Single(method => method.Name == "Ensure" && method.IsGenericMethodDefinition &&
                    method.GetParameters().Length == 0).MakeGenericMethod(part);
            object instance = ensure.Invoke(unit, null);
            FieldInfo level = part.GetField("ImportedLevel", BindingFlags.Instance | BindingFlags.Public);
            if (instance == null || level == null)
                throw new InvalidOperationException("the imported-companion plan part is unavailable");
            level.SetValue(instance, importedLevel);
        }

        /// <summary>The plan's actions through the native AddAction calls of ApplyLevelUpPlan, without its scope.</summary>
        private static void FcbApplyUnscoped(LevelUpController controller, LevelPlanData plan)
        {
            using (controller.ApplyingPlanScope())
                foreach (ILevelUpAction action in plan.Actions)
                    FcbNativeAddAction.Invoke(controller, new object[] { action, true });
        }

        /// <summary>Which planned actions the controller kept, by index.</summary>
        private static JObject FcbPlanAcceptance(LevelUpController controller, LevelPlanData plan)
        {
            var rejected = Enumerable.Range(0, plan.Actions.Length)
                .Where(index => !controller.LevelUpActions.Contains(plan.Actions[index])).ToArray();
            return new JObject
            {
                ["planned"] = plan.Actions.Length,
                ["accepted"] = plan.Actions.Length - rejected.Length,
                ["rejected"] = rejected.Length,
                ["rejectedIndices"] = new JArray(rejected),
                ["rejectedActions"] = string.Join(",", rejected.Select(index => FcbDescribeAction(plan.Actions[index]))
                    .ToArray())
            };
        }

        /// <summary>The index of the plan's pick of <paramref name="feature"/> (in <paramref name="selection"/> when given).</summary>
        private static int FcbPlanIndex(LevelPlanData plan, BlueprintFeatureSelection selection, BlueprintFeature feature)
        {
            for (int index = 0; index < plan.Actions.Length; index++)
            {
                var pick = plan.Actions[index] as SelectFeature;
                if (pick != null && pick.Item != null && ReferenceEquals(pick.Item.Feature, feature) &&
                    (selection == null || ReferenceEquals(pick.Selection, selection)))
                    return index;
            }
            return -1;
        }

        private static JArray FcbDescribePlan(LevelPlanData plan)
        {
            return new JArray(plan.Actions.Select(FcbDescribeAction));
        }

        private static string FcbDescribeAction(ILevelUpAction action)
        {
            var pick = action as SelectFeature;
            if (pick == null)
                return action == null ? "<null>" : action.GetType().Name + "@" + action.Priority;
            return "SelectFeature@" + action.Priority + ":" + FavoredClassLevelUpHarness.Name(pick.Selection) + "=" +
                (pick.Item == null || pick.Item.Feature == null ? "<none>" : pick.Item.Feature.name);
        }
    }
}
