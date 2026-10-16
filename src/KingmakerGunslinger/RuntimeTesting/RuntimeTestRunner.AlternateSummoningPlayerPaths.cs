using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private const string PlayerPathMasterSummonerArchetype = "cfbf35c3efa04ff78602780be9eeea22";
        private const string PlayerPathOccultistArchetype = "3dbd81a35cf74aefb4dba140a0a22079";

        private sealed class AlternateSummoningCastRecord
        {
            internal string Case;
            internal string Root;
            internal string Child;
            internal bool Converted;
            internal bool Available;
            internal bool Started;
            internal string Result = "NotStarted";
            internal int ResourceBefore;
            internal int ResourceAfter;
            internal int ExpectedCost;
            internal int Summoned;
            internal int Live;
            internal string Units = "";
            internal string Durations = "";
            internal string Exception = "<none>";

            internal JObject ToJson()
            {
                return new JObject {
                    ["case"] = Case, ["root"] = Root, ["child"] = Child,
                    ["convertedFromRoot"] = Converted, ["available"] = Available,
                    ["started"] = Started, ["result"] = Result,
                    ["resourceBefore"] = ResourceBefore, ["resourceAfter"] = ResourceAfter,
                    ["expectedCost"] = ExpectedCost, ["summoned"] = Summoned, ["live"] = Live,
                    ["units"] = Units, ["durationsSeconds"] = Durations, ["exception"] = Exception
                };
            }
        }

        /// <summary>
        /// Save-free player paths for Master Summoner and Occultist on the
        /// real first native area. Each caster is a disposable in-area unit
        /// that gains its class and archetype through native level-up visits,
        /// so its summon abilities are the real granted host roots. Every cast
        /// goes through the root's own variant data (the action bar's
        /// converted-ability path) and the native UnitUseAbility command, and
        /// the spent resource, summoned units and their durations are read
        /// back from the engine. Every created unit is destroyed and the
        /// native census restored; nothing is saved.
        /// </summary>
        private RuntimeTestResult RunAlternateSummoningPlayerPaths()
        {
            WriteLifecycleStage("alternate-summoning-player-paths-start");
            var assertions = new List<RuntimeTestAssertion>();
            var document = new JObject {
                ["scope"] = "Save-free native first area. Disposable casters levelled by native LevelUpController visits; casts through the granted host root's variant data and UnitUseAbility. No save, no UI popup click, no hotbar slot.",
                ["runId"] = _request.RunId
            };
            var casts = new JArray();
            document["casts"] = casts;
            ExpandedSummoningCorrectionFixture fixture = null;
            bool cleaned = false;
            object levelController = null;
            try
            {
                foreach (string key in new[] { AlternateSummoningSourceCatalog.MasterSummonerKey,
                    AlternateSummoningSourceCatalog.OccultistKey })
                {
                    AlternateSummoningFamilyStatus status = AlternateSummoningStatusRegistry.Find(key);
                    if (status == null || status.Disposition != AlternateSummoningDisposition.Published)
                        throw new InvalidOperationException("Family " + key + " is not published: " +
                            (status == null ? "<no status>" : status.Disposition + " " + status.Reason));
                }
                fixture = BeginExpandedSummoningCorrectionFixture("KMG_Runtime_AlternateSummoning_Player");
                PlayerPathMasterSummoner(fixture, assertions, casts, document, ref levelController);
                PlayerPathOccultist(fixture, assertions, casts, document, ref levelController);
            }
            catch (Exception exception)
            {
                document["exception"] = exception.ToString();
                assertions.Add(Assertion("alternate-summoning-player-paths",
                    "fixture completes", exception.GetType().Name + ":" + exception.Message,
                    false, "unknown state is not qualification"));
            }
            finally
            {
                if (levelController != null)
                    try { levelController.GetType().GetMethod("Cancel").Invoke(levelController, null); }
                    catch (Exception) { }
                foreach (UnitEntityData unit in _playerPathKept.ToArray())
                    CleanupExpandedSummoningUnit(unit);
                _playerPathKept.Clear();
                EndExpandedSummoningCorrectionFixture(fixture, out cleaned);
                foreach (BlueprintUnit blueprint in _playerPathBlueprints)
                    if (blueprint != null) UnityEngine.Object.Destroy(blueprint);
                _playerPathBlueprints.Clear();
            }
            assertions.Add(Assertion("player-paths-cleanup",
                "every disposable caster and summoned unit is gone and the native census is unchanged",
                "cleaned=" + cleaned, cleaned, "exact unit/party/area reference snapshots"));
            RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory,
                "alternate-summoning-player-paths.json"), document.ToString(Formatting.Indented));
            WriteLifecycleStage("alternate-summoning-player-paths-written");
            assertions.Add(Assertion("loaded-mod-version", _request.ExpectedModVersion,
                _context.ModEntry.Info.Version,
                _context.ModEntry.Info.Version == _request.ExpectedModVersion,
                "Unity Mod Manager ModEntry.Info.Version"));
            return CreateResult(assertions.All(value => value.Status == "PASS") ? "PASS" : "FAIL",
                assertions, null);
        }

        private void PlayerPathMasterSummoner(ExpandedSummoningCorrectionFixture fixture,
            List<RuntimeTestAssertion> assertions, JArray casts, JObject document,
            ref object levelController)
        {
            LibraryScriptableObject library = BlueprintBootstrap.Library;
            UnitEntityData caster = fixture.Caster;
            caster.Descriptor.Stats.Charisma.BaseValue = 18;
            var summoner = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(library,
                AlternateSummoningContracts.SummonerClass, "CotW Summoner class");
            var archetype = BlueprintLibraryLookup.RequireExact<BlueprintArchetype>(library,
                PlayerPathMasterSummonerArchetype, "CotW Master Summoner archetype");
            AdvanceDisposableArchetypeCaster(caster.Descriptor, summoner, archetype, 5,
                ref levelController);
            AlternateSummoningFamilySpec family = AlternateSummoningSourceCatalog.Find(
                AlternateSummoningSourceCatalog.MasterSummonerKey);
            var resource = BlueprintLibraryLookup.RequireExact<BlueprintAbilityResource>(library,
                AlternateSummoningContracts.MasterSummonerResource, "Master Summoner resource");
            JObject granted = PlayerPathGranted(caster, family, 3);
            document["masterSummonerGranted"] = granted;
            bool grantedExact = granted.Value<bool>("exact");
            assertions.Add(Assertion("player-ms-granted-roots",
                "a level-5 Master Summoner holds exactly its tier I-III granting features and their published host roots, through native level-up",
                granted.ToString(Formatting.None), grantedExact,
                "LevelUpController visits with AddArchetype; UnitDescriptor facts and Abilities"));
            if (!grantedExact) return;
            caster.Descriptor.Resources.Restore(resource);

            // Tier I: the preserved original cast and one project creature,
            // both through the tier-I root's own menu.
            Ability tierOne = PlayerPathRootFact(caster, family, 1);
            BlueprintAbility originalCopy = PlayerPathPreserved(tierOne.Blueprint, family.Key);
            BlueprintAbility tierOneCreature = PlayerPathChild(tierOne.Blueprint, true, "1");
            foreach (var pair in new[] { Tuple.Create("ms-tier1-original", originalCopy),
                Tuple.Create("ms-tier1-creature", tierOneCreature) })
            {
                AlternateSummoningCastRecord record = PlayerPathCast(fixture, caster, tierOne,
                    pair.Item2, resource, pair.Item1, true);
                casts.Add(record.ToJson());
                assertions.Add(Assertion("player-" + pair.Item1,
                    "the tier-I root's variant (its preserved original cast, or a project creature) casts natively, spends exactly its own cost from the Master Summoner resource and summons exactly one unit",
                    record.ToJson().ToString(Formatting.None),
                    record.Converted && record.Available && record.Started && record.Result == "Success" &&
                        record.ExpectedCost == 1 && record.ResourceBefore - record.ResourceAfter == 1 &&
                        record.Summoned == 1 && record.Live == 1,
                    "AbilityData(parent, child) + UnitUseAbility; UnitAbilityResourceCollection"));
            }

            // Tier III group: a 1d3 option.
            Ability tierThree = PlayerPathRootFact(caster, family, 3);
            BlueprintAbility group = PlayerPathChild(tierThree.Blueprint, true, "d3");
            AlternateSummoningCastRecord groupCast = PlayerPathCast(fixture, caster, tierThree,
                group, resource, "ms-tier3-group", true);
            casts.Add(groupCast.ToJson());
            assertions.Add(Assertion("player-ms-tier3-group",
                "a tier III 1d3 option summons one to three units of one kind and spends exactly one use",
                groupCast.ToJson().ToString(Formatting.None),
                groupCast.Started && groupCast.Result == "Success" && groupCast.ExpectedCost == 1 &&
                    groupCast.ResourceBefore - groupCast.ResourceAfter == 1 &&
                    groupCast.Summoned >= 1 && groupCast.Summoned <= 3 &&
                    groupCast.Live == groupCast.Summoned,
                "AbilityData(parent, child) + UnitUseAbility"));

            // A failed attempt with no uses left spends nothing and summons nothing.
            int remaining = caster.Descriptor.Resources.GetResourceAmount(resource);
            caster.Descriptor.Resources.Spend(resource, remaining);
            AlternateSummoningCastRecord empty = PlayerPathCast(fixture, caster, tierOne,
                tierOneCreature, resource, "ms-no-uses", false);
            casts.Add(empty.ToJson());
            assertions.Add(Assertion("player-ms-no-uses-refused",
                "with no uses left the variant is unavailable, no command starts and nothing is spent or summoned",
                empty.ToJson().ToString(Formatting.None),
                !empty.Available && !empty.Started && empty.ResourceBefore == 0 &&
                    empty.ResourceAfter == 0 && empty.Summoned == 0,
                "AbilityData.IsAvailable and UnitUseAbility.CanStart"));
            caster.Descriptor.Resources.Restore(resource);

            // A cancelled attempt: the command starts and is interrupted
            // before its act; nothing is spent or summoned.
            AlternateSummoningCastRecord cancelled = PlayerPathCancelled(fixture, caster, tierOne,
                tierOneCreature, resource, "ms-cancelled");
            casts.Add(cancelled.ToJson());
            assertions.Add(Assertion("player-ms-cancelled-no-spend",
                "a started command interrupted before its act spends nothing and summons nothing",
                cancelled.ToJson().ToString(Formatting.None),
                cancelled.Started && cancelled.ResourceBefore == cancelled.ResourceAfter &&
                    cancelled.Summoned == 0,
                "UnitUseAbility.Interrupt before IsActed"));

            // Without an eidolon the Master Summoner's summons coexist.
            AlternateSummoningCastRecord first = PlayerPathCast(fixture, caster, tierOne,
                tierOneCreature, resource, "ms-coexist-first", true, keep: true);
            AlternateSummoningCastRecord second = PlayerPathCast(fixture, caster, tierOne,
                tierOneCreature, resource, "ms-coexist-second", true, keep: true);
            casts.Add(first.ToJson());
            casts.Add(second.ToJson());
            int liveTogether = PlayerPathLiveKept(fixture);
            assertions.Add(Assertion("player-ms-coexist-without-eidolon",
                "with no eidolon, two Master Summoner casts leave both summons alive",
                "first=" + first.Live + ";second=" + second.Live + ";liveTogether=" + liveTogether,
                first.Live == 1 && second.Live == 1 && liveTogether == 2,
                "HasUnitsInSummonPool/ClearSummonPool host branch, live unit census"));
            PlayerPathDisposeKept(fixture);
        }

        private void PlayerPathOccultist(ExpandedSummoningCorrectionFixture fixture,
            List<RuntimeTestAssertion> assertions, JArray casts, JObject document,
            ref object levelController)
        {
            LibraryScriptableObject library = BlueprintBootstrap.Library;
            UnitEntityData caster = PlayerPathSecondCaster(fixture, "KMG_Runtime_AlternateSummoning_Occultist");
            caster.Descriptor.Stats.Intelligence.BaseValue = 18;
            var arcanist = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(library,
                AlternateSummoningContracts.ArcanistClass, "Arcanist class");
            var archetype = BlueprintLibraryLookup.RequireExact<BlueprintArchetype>(library,
                PlayerPathOccultistArchetype, "CotW Occultist archetype");
            AdvanceDisposableArchetypeCaster(caster.Descriptor, arcanist, archetype, 5,
                ref levelController);
            AlternateSummoningFamilySpec family = AlternateSummoningSourceCatalog.Find(
                AlternateSummoningSourceCatalog.OccultistKey);
            var reservoir = BlueprintLibraryLookup.RequireExact<BlueprintAbilityResource>(library,
                AlternateSummoningContracts.ArcaneReservoirResource, "Arcane reservoir");
            JObject granted = PlayerPathGranted(caster, family, 3);
            document["occultistGranted"] = granted;
            bool grantedExact = granted.Value<bool>("exact");
            assertions.Add(Assertion("player-occultist-granted-roots",
                "a level-5 Occultist holds exactly its tier I-III Conjurer's Focus features and their published host roots, through native level-up",
                granted.ToString(Formatting.None), grantedExact,
                "LevelUpController visits with AddArchetype"));
            if (!grantedExact) return;
            PlayerPathFillResource(caster, reservoir, 10);

            Ability tierThree = PlayerPathRootFact(caster, family, 3);
            BlueprintAbility single = PlayerPathChild(tierThree.Blueprint, true, "1");
            AlternateSummoningCastRecord first = PlayerPathCast(fixture, caster, tierThree,
                single, reservoir, "occultist-tier3", true, keep: true);
            casts.Add(first.ToJson());
            assertions.Add(Assertion("player-occultist-tier-cost-and-duration",
                "a tier III Conjurer's Focus option spends exactly 3 reservoir points and its summon lasts one minute per Arcanist level",
                first.ToJson().ToString(Formatting.None),
                first.Started && first.Result == "Success" && first.ExpectedCost == 3 &&
                    first.ResourceBefore - first.ResourceAfter == 3 && first.Live == 1 &&
                    first.Durations == "300",
                "AbilityResourceLogic custom cost; RuleSummonUnit duration"));
            AlternateSummoningCastRecord second = PlayerPathCast(fixture, caster, tierThree,
                single, reservoir, "occultist-replacement", true, keep: true);
            casts.Add(second.ToJson());
            int liveAfterReplacement = PlayerPathLiveKept(fixture);
            assertions.Add(Assertion("player-occultist-replacement",
                "a second Conjurer's Focus cast ends the first summon: only the new summon remains",
                "first=" + first.Units + ";second=" + second.Units + ";live=" + liveAfterReplacement,
                second.Live == 1 && liveAfterReplacement == 1,
                "occultist summon pool clear on the adapter child"));
            PlayerPathDisposeKept(fixture);

            // The level-20 capstone: Perfect Summoner removes the cost and the
            // duration becomes one day plus a minute per level.
            AdvanceDisposableArchetypeCaster(caster.Descriptor, arcanist, archetype, 15,
                ref levelController);
            var perfect = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library,
                AlternateSummoningContracts.PerfectSummoner, "Perfect Summoner");
            PlayerPathFillResource(caster, reservoir, 10);
            Ability tierThreeAt20 = PlayerPathRootFact(caster, family, 3);
            AlternateSummoningCastRecord capstone = PlayerPathCast(fixture, caster, tierThreeAt20,
                single, reservoir, "occultist-capstone", true);
            casts.Add(capstone.ToJson());
            assertions.Add(Assertion("player-occultist-capstone",
                "at level 20 Perfect Summoner makes the tier III option free and its summon lasts 1440 minutes",
                "perfect=" + caster.Descriptor.HasFact(perfect) + ";" +
                    capstone.ToJson().ToString(Formatting.None),
                caster.Descriptor.HasFact(perfect) && capstone.Started &&
                    capstone.Result == "Success" && capstone.ExpectedCost == 0 &&
                    capstone.ResourceBefore == capstone.ResourceAfter && capstone.Live == 1 &&
                    capstone.Durations == (1440 * 60).ToString(),
                "ResourseCostCalculatorWithDecreasingFacts; ContextRankConfig FeatureRank x1420"));
        }

        private static void PlayerPathFillResource(UnitEntityData caster,
            BlueprintAbilityResource resource, int amount)
        {
            caster.Descriptor.Resources.Restore(resource);
            int current = caster.Descriptor.Resources.GetResourceAmount(resource);
            if (current < amount)
                caster.Descriptor.Resources.Restore(resource, amount - current);
        }

        private JObject PlayerPathGranted(UnitEntityData caster,
            AlternateSummoningFamilySpec family, int maximumTier)
        {
            var roots = new JArray();
            bool exact = true;
            foreach (AlternateSummoningRootSpec root in family.Roots)
            {
                var feature = (BlueprintFeature)BlueprintBootstrap.Library.BlueprintsByAssetId[root.FeatureGuid];
                bool hasFeature = caster.Descriptor.HasFact(feature);
                Ability fact = caster.Descriptor.Abilities.RawFacts.OfType<Ability>().FirstOrDefault(
                    value => value.Blueprint != null && value.Blueprint.AssetGuid == root.RootGuid);
                bool expected = root.Tier <= maximumTier;
                bool published = fact != null && AlternateSummoningExposure.IsPublishedRoot(fact.Blueprint);
                exact &= hasFeature == expected && (fact != null) == expected &&
                    (!expected || published);
                roots.Add(new JObject { ["tier"] = root.Tier, ["feature"] = hasFeature,
                    ["root"] = fact != null, ["published"] = published,
                    ["variants"] = fact == null ? 0 : (fact.Blueprint.ComponentsArray
                        .OfType<AbilityVariants>().SingleOrDefault()?.Variants?.Length ?? 0) });
            }
            return new JObject { ["exact"] = exact, ["roots"] = roots };
        }

        private static Ability PlayerPathRootFact(UnitEntityData caster,
            AlternateSummoningFamilySpec family, int tier)
        {
            string guid = family.Roots.Single(value => value.Tier == tier).RootGuid;
            return caster.Descriptor.Abilities.RawFacts.OfType<Ability>().Single(value =>
                value.Blueprint != null && value.Blueprint.AssetGuid == guid);
        }

        /// <summary>
        /// One exposed adapter child of a published root. A project child
        /// spawns a registered Expanded Summoning unit; the other kind is the
        /// frozen copy of the root's original cast.
        /// </summary>
        private static BlueprintAbility PlayerPathChild(BlueprintAbility root, bool project,
            string quantity)
        {
            AbilityVariants variants = root.ComponentsArray.OfType<AbilityVariants>().Single();
            foreach (BlueprintAbility child in variants.Variants)
            {
                if (child == null || !AlternateSummoningExposure.IsExposed(child.AssetGuid)) continue;
                SummonSpawn[] spawns = SummonSignatureReader.Read(child).Spawns().ToArray();
                if (spawns.Length == 0) continue;
                BlueprintScriptableObject unit;
                BlueprintBootstrap.Library.BlueprintsByAssetId.TryGetValue(spawns[0].Unit, out unit);
                bool projectUnit = unit != null &&
                    unit.name.StartsWith("KMG_Summoning_", StringComparison.Ordinal);
                if (projectUnit != project) continue;
                if (quantity != null)
                {
                    string dice = spawns[0].CountDice;
                    bool matches = quantity == "1" ? dice == "Zero" || dice == "One" :
                        quantity == "d3" ? dice == "D3" : dice == "D4";
                    if (!matches) continue;
                }
                return child;
            }
            throw new InvalidOperationException("No exposed " + (project ? "project" : "original") +
                " child with quantity " + (quantity ?? "any") + " under " + root.AssetGuid + ".");
        }

        /// <summary>The exact frozen copy of a direct root's original cast,
        /// found by its own adapter identity in the root's live menu.</summary>
        private static BlueprintAbility PlayerPathPreserved(BlueprintAbility root, string familyKey)
        {
            string name = AlternateSummoningAdapterBuilder.InternalName(
                AlternateSummoningProjection.PreservedSymbol(familyKey, 1, SummonMultiplicity.One));
            BlueprintAbility[] matches = root.ComponentsArray.OfType<AbilityVariants>().Single()
                .Variants.Where(value => value != null && value.name == name &&
                    AlternateSummoningExposure.IsExposed(value.AssetGuid)).ToArray();
            if (matches.Length != 1)
                throw new InvalidOperationException("The preserved tier-I cast " + name +
                    " is not exactly once in " + root.AssetGuid + "'s menu.");
            return matches[0];
        }

        private static void AdvanceDisposableArchetypeCaster(UnitDescriptor owner,
            BlueprintCharacterClass characterClass, BlueprintArchetype archetype, int levels,
            ref object activeController)
        {
            MethodInfo start = typeof(LevelUpController).GetMethods(BindingFlags.Public |
                BindingFlags.NonPublic | BindingFlags.Static).Single(value =>
                    value.Name == "StartWithoutAssigningStaticInstance" &&
                    value.GetParameters().Length == 5);
            MethodInfo apply = typeof(LevelUpController).GetMethod("ApplyLevelup",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            object charGen = Enum.Parse(start.GetParameters()[4].ParameterType, "CharGen", false);
            for (int index = 0; index < levels; index++)
            {
                var controller = (LevelUpController)start.Invoke(null,
                    new object[] { owner, false, null, null, charGen });
                activeController = controller;
                if (owner.Progression.GetClassLevel(characterClass) == 0 &&
                    !controller.AddArchetype(characterClass, archetype))
                    throw new InvalidOperationException("Native archetype selection rejected " +
                        archetype.name + ".");
                if (!controller.SelectClass(characterClass, false))
                    throw new InvalidOperationException("Disposable caster class selection failed at level " +
                        (owner.Progression.GetClassLevel(characterClass) + 1) + ".");
                controller.ApplyClassMechanics();
                apply.Invoke(controller, new object[] { owner });
                controller.Cancel();
                activeController = null;
            }
            if (!owner.Progression.IsArchetype(archetype))
                throw new InvalidOperationException("The disposable caster did not keep " +
                    archetype.name + ".");
        }

        private UnitEntityData PlayerPathSecondCaster(ExpandedSummoningCorrectionFixture fixture,
            string name)
        {
            BlueprintUnit blueprint = UnityEngine.Object.Instantiate(fixture.CasterBlueprint);
            blueprint.name = name;
            UnitEntityData caster = Game.Instance.EntityCreator.SpawnUnit(blueprint,
                fixture.Caster.Position + Vector3.right, Quaternion.identity, fixture.Scene);
            Game.Instance.EntityCreator.Tick();
            if (caster == null || !caster.IsInState || caster.View == null)
                throw new InvalidOperationException("The second disposable caster did not enter the area.");
            fixture.Created.Add(caster);
            _playerPathBlueprints.Add(blueprint);
            caster.Descriptor.Stats.HitPoints.BaseValue = 100000;
            caster.Descriptor.Alignment.Set(Alignment.NeutralGood);
            SetExpandedSummoningBrainActive(caster, false);
            PlaceExpandedSummoningUnit(caster, caster.Position);
            return caster;
        }

        private readonly List<BlueprintUnit> _playerPathBlueprints = new List<BlueprintUnit>();
        private readonly List<UnitEntityData> _playerPathKept = new List<UnitEntityData>();
        private object[] _playerPathKeptBaseline;

        private AlternateSummoningCastRecord PlayerPathCast(ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData caster, Ability root, BlueprintAbility child,
            BlueprintAbilityResource resource, string name, bool expectStart, bool keep = false)
        {
            var record = new AlternateSummoningCastRecord { Case = name,
                Root = root.Blueprint.AssetGuid, Child = child.AssetGuid };
            object[] before = SnapshotReferences(fixture.SceneEntities);
            ExpandedSummoningRuleCapture.Clear();
            ExpandedSummoningRuleDurationCapture.Clear();
            record.ResourceBefore = caster.Descriptor.Resources.GetResourceAmount(resource);
            try
            {
                var parent = new AbilityData(root);
                var data = new AbilityData(parent, child);
                record.Converted = ReferenceEquals(data.ConvertedFrom, parent) &&
                    ReferenceEquals(data.Fact, root) && root.Blueprint.HasVariant(child);
                AbilityResourceLogic logic = child.ComponentsArray.OfType<AbilityResourceLogic>()
                    .SingleOrDefault();
                record.ExpectedCost = logic == null ? 0 : logic.CalculateCost(data);
                var target = new TargetWrapper(caster.Position);
                UnitUseAbility command;
                var parameters = new Kingmaker.AreaLogic.Cutscenes.CutsceneParametersContext();
                using (parameters.Data) command = new UnitUseAbility(data, target);
                record.Available = data.IsAvailable;
                if (data.IsAvailable && command.CanStart)
                {
                    command.IgnoreCooldown(TimeSpan.Zero);
                    caster.Commands.Run(command);
                    command.Start();
                    record.Started = command.IsRunning;
                    if (command.IsRunning)
                    {
                        if (command.Animation != null) command.Animation.IsActed = true;
                        command.Tick();
                        if (command.ExecutionProcess != null)
                            for (int tick = 0; tick < 5000 && !command.ExecutionProcess.IsEnded; tick++)
                                command.ExecutionProcess.Tick();
                        if (command.Animation != null) FinishExpandedSummoningAnimation(command.Animation);
                        if (!command.IsFinished) command.Tick();
                    }
                    record.Result = command.Result.ToString();
                }
            }
            catch (Exception exception)
            {
                record.Exception = exception.GetType().Name + ":" + exception.Message;
            }
            Game.Instance.EntityCreator.Tick();
            record.ResourceAfter = caster.Descriptor.Resources.GetResourceAmount(resource);
            UnitEntityData[] summoned = ExpandedSummoningRuleCapture.ToArray();
            record.Summoned = summoned.Length;
            UnitEntityData[] live = summoned.Where(value => value != null && !value.Destroyed &&
                value.IsInState && ContainsReference(fixture.SceneEntities, value)).ToArray();
            record.Live = live.Length;
            record.Units = string.Join(",", summoned.Select(value => value == null ? "<null>" :
                value.Blueprint.name).ToArray());
            record.Durations = string.Join(",", ExpandedSummoningRuleDurationCapture.Select(value =>
                ((int)Math.Round(value.TotalDuration.TotalSeconds)).ToString()).ToArray());
            if (keep)
            {
                if (_playerPathKept.Count == 0) _playerPathKeptBaseline = before;
                _playerPathKept.AddRange(live);
            }
            else
            {
                foreach (UnitEntityData unit in summoned) CleanupExpandedSummoningUnit(unit);
                DrainExpandedSummoningDestroyQueue(fixture.SceneEntities, before);
            }
            ExpandedSummoningRuleCapture.Clear();
            return record;
        }

        private AlternateSummoningCastRecord PlayerPathCancelled(ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData caster, Ability root, BlueprintAbility child,
            BlueprintAbilityResource resource, string name)
        {
            var record = new AlternateSummoningCastRecord { Case = name,
                Root = root.Blueprint.AssetGuid, Child = child.AssetGuid };
            ExpandedSummoningRuleCapture.Clear();
            record.ResourceBefore = caster.Descriptor.Resources.GetResourceAmount(resource);
            try
            {
                var data = new AbilityData(new AbilityData(root), child);
                record.Converted = data.ConvertedFrom != null;
                record.Available = data.IsAvailable;
                UnitUseAbility command;
                var parameters = new Kingmaker.AreaLogic.Cutscenes.CutsceneParametersContext();
                using (parameters.Data) command = new UnitUseAbility(data, new TargetWrapper(caster.Position));
                if (data.IsAvailable && command.CanStart)
                {
                    command.IgnoreCooldown(TimeSpan.Zero);
                    caster.Commands.Run(command);
                    command.Start();
                    record.Started = command.IsRunning;
                    command.Interrupt();
                    if (!command.IsFinished) command.Tick();
                    record.Result = command.Result.ToString();
                }
            }
            catch (Exception exception)
            {
                record.Exception = exception.GetType().Name + ":" + exception.Message;
            }
            Game.Instance.EntityCreator.Tick();
            record.ResourceAfter = caster.Descriptor.Resources.GetResourceAmount(resource);
            record.Summoned = ExpandedSummoningRuleCapture.Count;
            foreach (UnitEntityData unit in ExpandedSummoningRuleCapture.ToArray())
                CleanupExpandedSummoningUnit(unit);
            ExpandedSummoningRuleCapture.Clear();
            return record;
        }

        private int PlayerPathLiveKept(ExpandedSummoningCorrectionFixture fixture)
        {
            Game.Instance.EntityCreator.Tick();
            Game.Instance.EntityDestroyer.Tick();
            BlueprintBuff summoned = Game.Instance.BlueprintRoot.SystemMechanics.SummonedUnitBuff;
            return _playerPathKept.Count(value => value != null && !value.Destroyed &&
                value.IsInState && ContainsReference(fixture.SceneEntities, value) &&
                value.Descriptor.HasFact(summoned));
        }

        private void PlayerPathDisposeKept(ExpandedSummoningCorrectionFixture fixture)
        {
            foreach (UnitEntityData unit in _playerPathKept.ToArray())
                CleanupExpandedSummoningUnit(unit);
            _playerPathKept.Clear();
            DrainExpandedSummoningDestroyQueue(fixture.SceneEntities, _playerPathKeptBaseline);
            _playerPathKeptBaseline = null;
        }
    }
}
