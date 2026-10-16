using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
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
                // The shared fixture marks its disposable caster a cheater, and
                // AbilityResourceLogic.Spend skips cheaters. These casters must
                // pay their real costs, so this fixture's own blueprint is not.
                fixture.CasterBlueprint.IsCheater = false;
                PlayerPathSection("master-summoner", assertions, document, () =>
                    PlayerPathMasterSummoner(fixture, assertions, casts, document));
                PlayerPathSection("occultist", assertions, document, () =>
                    PlayerPathOccultist(fixture, assertions, casts, document));
                PlayerPathSection("remaining-families", assertions, document, () =>
                    PlayerPathRemainingFamilies(fixture, assertions, casts, document));
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
                levelController = _playerPathLevelController;
                _playerPathLevelController = null;
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

        private object _playerPathLevelController;

        /// <summary>One section's exception becomes that section's failure;
        /// the next section still runs on the same disposable world.</summary>
        private void PlayerPathSection(string name, List<RuntimeTestAssertion> assertions,
            JObject document, Action body)
        {
            try { body(); }
            catch (Exception exception)
            {
                document["exception-" + name] = exception.ToString();
                assertions.Add(Assertion("player-section-" + name, "section completes",
                    exception.GetType().Name + ":" + exception.Message, false,
                    "unknown state is not qualification"));
                foreach (UnitEntityData unit in _playerPathKept.ToArray())
                    CleanupExpandedSummoningUnit(unit);
                _playerPathKept.Clear();
            }
        }

        private void PlayerPathMasterSummoner(ExpandedSummoningCorrectionFixture fixture,
            List<RuntimeTestAssertion> assertions, JArray casts, JObject document)
        {
            LibraryScriptableObject library = BlueprintBootstrap.Library;
            UnitEntityData caster = fixture.Caster;
            caster.Descriptor.Stats.Charisma.BaseValue = 18;
            var summoner = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(library,
                AlternateSummoningContracts.SummonerClass, "CotW Summoner class");
            var archetype = BlueprintLibraryLookup.RequireExact<BlueprintArchetype>(library,
                PlayerPathMasterSummonerArchetype, "CotW Master Summoner archetype");
            AdvanceDisposableArchetypeCaster(caster.Descriptor, summoner, archetype, 5,
                ref _playerPathLevelController);
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

            // Tier I: the direct root became a menu. Record it, then cast a
            // project creature and (when the menu keeps one) a native option.
            Ability tierOne = PlayerPathRootFact(caster, family, 1);
            document["masterSummonerTierOneMenu"] = PlayerPathMenu(tierOne.Blueprint);
            BlueprintAbility tierOneCreature = PlayerPathChild(tierOne.Blueprint, true, "1");
            BlueprintAbility nativeOption = PlayerPathChildOrNull(tierOne.Blueprint, false, "1");
            var tierOneCasts = new List<Tuple<string, BlueprintAbility>> {
                Tuple.Create("ms-tier1-creature", tierOneCreature) };
            if (nativeOption != null)
                tierOneCasts.Add(Tuple.Create("ms-tier1-native-option", nativeOption));
            foreach (var pair in tierOneCasts)
            {
                AlternateSummoningCastRecord record = PlayerPathCast(fixture, caster, tierOne,
                    pair.Item2, resource, pair.Item1, true);
                casts.Add(record.ToJson());
                assertions.Add(Assertion("player-" + pair.Item1,
                    "a tier-I menu option casts natively through the root, spends exactly its own cost from the Master Summoner resource and summons exactly one unit",
                    record.ToJson().ToString(Formatting.None),
                    record.Converted && record.Available && record.Started && record.Result == "Success" &&
                        record.ExpectedCost == 1 && record.ResourceBefore - record.ResourceAfter == 1 &&
                        record.Summoned == 1 && record.Live == 1,
                    "AbilityData(parent, child) + UnitUseAbility; UnitAbilityResourceCollection"));
            }

            // The frozen copy of the original tier-I cast is a registered
            // identity. When the menu policy replaced its lineage it is not
            // exposed, and a cast through the root must be refused.
            BlueprintAbility preserved = PlayerPathPreserved(family.Key);
            bool preservedInMenu = tierOne.Blueprint.ComponentsArray.OfType<AbilityVariants>()
                .Single().Variants.Contains(preserved);
            if (!preservedInMenu)
            {
                AlternateSummoningCastRecord hidden = PlayerPathCast(fixture, caster, tierOne,
                    preserved, resource, "ms-unexposed-preserved", false);
                casts.Add(hidden.ToJson());
                assertions.Add(Assertion("player-ms-unexposed-preserved-refused",
                    "the registered but unexposed preserved tier-I cast is unavailable through the root: no command, no spend, no summon",
                    "exposed=" + AlternateSummoningExposure.IsExposed(preserved.AssetGuid) + ";" +
                        hidden.ToJson().ToString(Formatting.None),
                    !AlternateSummoningExposure.IsExposed(preserved.AssetGuid) && !hidden.Available &&
                        !hidden.Started && hidden.ResourceBefore == hidden.ResourceAfter &&
                        hidden.Summoned == 0,
                    "AlternateSummoningAvailability on the executed child"));
            }
            else assertions.Add(Assertion("player-ms-preserved-in-menu",
                "the preserved tier-I cast is exposed exactly when the menu keeps it",
                "exposed=" + AlternateSummoningExposure.IsExposed(preserved.AssetGuid),
                AlternateSummoningExposure.IsExposed(preserved.AssetGuid),
                "AlternateSummoningExposure"));

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
            List<RuntimeTestAssertion> assertions, JArray casts, JObject document)
        {
            LibraryScriptableObject library = BlueprintBootstrap.Library;
            UnitEntityData caster = PlayerPathSecondCaster(fixture, "KMG_Runtime_AlternateSummoning_Occultist");
            caster.Descriptor.Stats.Intelligence.BaseValue = 18;
            var arcanist = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(library,
                AlternateSummoningContracts.ArcanistClass, "Arcanist class");
            var archetype = BlueprintLibraryLookup.RequireExact<BlueprintArchetype>(library,
                PlayerPathOccultistArchetype, "CotW Occultist archetype");
            AdvanceDisposableArchetypeCaster(caster.Descriptor, arcanist, archetype, 5,
                ref _playerPathLevelController);
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
                ref _playerPathLevelController);
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

        // ---- remaining families: one real granted cast per distinct contract ----

        private const string PlayerPathInquisitorClass = "f1a70d9e1b0b41e49874e1fa9052a1ce";
        private const string PlayerPathAlchemistClass = "0937bec61c0dabc468428f496580c721";
        private const string PlayerPathTwinnedSummonerArchetype = "e775f3730d5d4186a38bcf2ea67b8009";
        private const string PlayerPathNaturalistArchetype = "5794865f6aa541ce9c09c8e682cea799";
        private const string PlayerPathFeralHunterArchetype = "f7eae9611b8540e6978776123a85cba0";
        private const string PlayerPathPreservationistArchetype = "a2c1e3724a304fb6a98c708fc0f3448c";

        private void PlayerPathRemainingFamilies(ExpandedSummoningCorrectionFixture fixture,
            List<RuntimeTestAssertion> assertions, JArray casts, JObject document)
        {
            LibraryScriptableObject library = BlueprintBootstrap.Library;
            var cases = new[] {
                Tuple.Create(AlternateSummoningSourceCatalog.MonsterTacticianKey,
                    PlayerPathInquisitorClass, "discover", 6, StatType.Wisdom),
                Tuple.Create(AlternateSummoningSourceCatalog.SummonerKey,
                    AlternateSummoningContracts.SummonerClass, (string)null, 3, StatType.Charisma),
                Tuple.Create(AlternateSummoningSourceCatalog.SummonerKey,
                    AlternateSummoningContracts.SummonerClass, PlayerPathTwinnedSummonerArchetype, 3, StatType.Charisma),
                Tuple.Create(AlternateSummoningSourceCatalog.NaturalistKey,
                    AlternateSummoningContracts.SummonerClass, PlayerPathNaturalistArchetype, 3, StatType.Charisma),
                Tuple.Create(AlternateSummoningSourceCatalog.FeralHunterKey,
                    AlternateSummoningContracts.HunterClass, PlayerPathFeralHunterArchetype, 3, StatType.Wisdom)
            };
            foreach (var item in cases)
            {
                string label = item.Item1 + (item.Item3 == null || item.Item3 == "discover" ? "" :
                    ":" + item.Item3.Substring(0, 8));
                JObject record = new JObject { ["family"] = item.Item1, ["archetype"] = item.Item3 };
                bool exact = false;
                try
                {
                    UnitEntityData caster = PlayerPathSecondCaster(fixture,
                        "KMG_Runtime_AlternateSummoning_" + item.Item1);
                    caster.Descriptor.Stats.GetStat(item.Item5).BaseValue = 18;
                    var characterClass = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(
                        library, item.Item2, item.Item1 + " class");
                    BlueprintArchetype archetype = item.Item3 == null ? null :
                        item.Item3 == "discover" ? PlayerPathTacticianArchetype(record) :
                        BlueprintLibraryLookup.RequireExact<BlueprintArchetype>(library, item.Item3,
                            item.Item1 + " archetype");
                    AdvanceDisposableArchetypeCaster(caster.Descriptor, characterClass, archetype,
                        item.Item4, ref _playerPathLevelController);
                    AlternateSummoningFamilySpec family = AlternateSummoningSourceCatalog.Find(item.Item1);
                    Ability root = PlayerPathLowestGrantedRoot(caster, family, record);
                    BlueprintAbility child = PlayerPathChild(root.Blueprint, true, "1");
                    AbilityResourceLogic logic = child.ComponentsArray.OfType<AbilityResourceLogic>()
                        .SingleOrDefault();
                    BlueprintAbilityResource resource = logic == null ? null : logic.RequiredResource;
                    if (resource != null) caster.Descriptor.Resources.Restore(resource);
                    AlternateSummoningCastRecord cast = PlayerPathCast(fixture, caster, root, child,
                        resource, "family-" + label, true);
                    casts.Add(cast.ToJson());
                    record["cast"] = cast.ToJson();
                    exact = cast.Converted && cast.Started && cast.Result == "Success" &&
                        cast.Summoned == 1 && cast.Live == 1 &&
                        (resource == null || cast.ResourceBefore - cast.ResourceAfter == cast.ExpectedCost);
                }
                catch (Exception exception)
                {
                    record["exception"] = exception.GetType().Name + ":" + exception.Message;
                }
                document["family-" + label] = record;
                assertions.Add(Assertion("player-family-" + label,
                    "a natively levelled caster's lowest granted root offers a project creature that casts natively, spends exactly its own cost and summons one unit",
                    record.ToString(Formatting.None), exact,
                    "LevelUpController visits; AbilityData(parent, child) + UnitUseAbility"));
            }
            PlayerPathPreservationist(fixture, assertions, casts, document);
        }

        /// <summary>
        /// The native Monster Tactician archetype: the one archetype whose
        /// added features reach the native tier-I Monster Tactician feature,
        /// directly or through a progression, selection or component
        /// reference. Found by that exact feature identity, never by name.
        /// </summary>
        private static BlueprintArchetype PlayerPathTacticianArchetype(JObject record)
        {
            string tierOne = AlternateSummoningSourceCatalog.Find(
                AlternateSummoningSourceCatalog.MonsterTacticianKey).Roots
                .Single(value => value.Tier == 1).FeatureGuid;
            var found = new List<BlueprintArchetype>();
            foreach (BlueprintArchetype archetype in BlueprintBootstrap.Library.GetAllBlueprints()
                .OfType<BlueprintArchetype>())
            {
                var seen = new HashSet<BlueprintScriptableObject>();
                var queue = new Queue<KeyValuePair<BlueprintScriptableObject, int>>();
                foreach (LevelEntry level in archetype.AddFeatures ?? new LevelEntry[0])
                    foreach (BlueprintFeatureBase feature in level.Features)
                        if (feature != null)
                            queue.Enqueue(new KeyValuePair<BlueprintScriptableObject, int>(feature, 0));
                while (queue.Count > 0)
                {
                    KeyValuePair<BlueprintScriptableObject, int> item = queue.Dequeue();
                    if (!seen.Add(item.Key)) continue;
                    if (item.Key.AssetGuid == tierOne) { found.Add(archetype); break; }
                    if (item.Value >= 4) continue;
                    foreach (BlueprintScriptableObject next in PlayerPathReferencedFacts(item.Key))
                        queue.Enqueue(new KeyValuePair<BlueprintScriptableObject, int>(next,
                            item.Value + 1));
                }
            }
            record["tacticianArchetypes"] = new JArray(found.Select(value =>
                value.AssetGuid + ":" + value.name));
            if (found.Count != 1)
                throw new InvalidOperationException("The native Monster Tactician archetype is not unique: " +
                    found.Count + ".");
            return found[0];
        }

        private static IEnumerable<BlueprintScriptableObject> PlayerPathReferencedFacts(
            BlueprintScriptableObject source)
        {
            var progression = source as BlueprintProgression;
            if (progression != null)
                foreach (LevelEntry level in progression.LevelEntries ?? new LevelEntry[0])
                    foreach (BlueprintFeatureBase feature in level.Features)
                        if (feature != null) yield return feature;
            var selection = source as Kingmaker.Blueprints.Classes.Selection.BlueprintFeatureSelection;
            if (selection != null)
                foreach (BlueprintFeature feature in selection.AllFeatures ?? new BlueprintFeature[0])
                    if (feature != null) yield return feature;
            foreach (BlueprintComponent component in source.ComponentsArray ?? new BlueprintComponent[0])
            {
                if (component == null) continue;
                foreach (FieldInfo field in component.GetType().GetFields(BindingFlags.Instance |
                    BindingFlags.Public | BindingFlags.NonPublic))
                {
                    object value = field.GetValue(component);
                    if (value is BlueprintFeatureBase)
                    {
                        yield return (BlueprintScriptableObject)value;
                        continue;
                    }
                    var many = value as System.Collections.IEnumerable;
                    if (many == null || value is string) continue;
                    foreach (object element in many)
                        if (element is BlueprintFeatureBase)
                            yield return (BlueprintScriptableObject)element;
                }
            }
        }

        /// <summary>The granted root of the lowest tier the caster holds:
        /// a direct root by its GUID, or for Monster Tactician the single
        /// ability its tier feature adds.</summary>
        private static Ability PlayerPathLowestGrantedRoot(UnitEntityData caster,
            AlternateSummoningFamilySpec family, JObject record)
        {
            foreach (AlternateSummoningRootSpec root in family.Roots.OrderBy(value => value.Tier))
            {
                var feature = (BlueprintFeature)BlueprintBootstrap.Library.BlueprintsByAssetId[root.FeatureGuid];
                if (!caster.Descriptor.HasFact(feature)) continue;
                IEnumerable<string> guids = root.RootGuid != null ? new[] { root.RootGuid } :
                    feature.ComponentsArray.OfType<Kingmaker.UnitLogic.FactLogic.AddFacts>()
                        .SelectMany(value => value.Facts).OfType<BlueprintAbility>()
                        .Select(value => value.AssetGuid);
                Ability fact = caster.Descriptor.Abilities.RawFacts.OfType<Ability>().FirstOrDefault(
                    value => value.Blueprint != null && guids.Contains(value.Blueprint.AssetGuid) &&
                        AlternateSummoningExposure.IsPublishedRoot(value.Blueprint));
                record["grantedTier"] = root.Tier;
                record["feature"] = feature.AssetGuid;
                record["root"] = fact == null ? null : fact.Blueprint.AssetGuid;
                if (fact == null)
                    throw new InvalidOperationException("Tier " + root.Tier + " feature is granted but no published root fact exists.");
                return fact;
            }
            throw new InvalidOperationException("No " + family.Key + " tier feature was granted.");
        }

        /// <summary>Preservationist: Bottled Ally is a known extract in the
        /// Alchemist's formula book; prepare it in a native slot and open the
        /// bottle through the slot's own variant data.</summary>
        private void PlayerPathPreservationist(ExpandedSummoningCorrectionFixture fixture,
            List<RuntimeTestAssertion> assertions, JArray casts, JObject document)
        {
            var record = new JObject { ["family"] = AlternateSummoningSourceCatalog.PreservationistKey };
            bool exact = false;
            try
            {
                LibraryScriptableObject library = BlueprintBootstrap.Library;
                UnitEntityData caster = PlayerPathSecondCaster(fixture, "KMG_Runtime_AlternateSummoning_Preservationist");
                caster.Descriptor.Stats.Intelligence.BaseValue = 18;
                var alchemist = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(library,
                    PlayerPathAlchemistClass, "Alchemist class");
                var archetype = BlueprintLibraryLookup.RequireExact<BlueprintArchetype>(library,
                    PlayerPathPreservationistArchetype, "Preservationist archetype");
                AdvanceDisposableArchetypeCaster(caster.Descriptor, alchemist, archetype, 2,
                    ref _playerPathLevelController);
                AlternateSummoningFamilySpec family = AlternateSummoningSourceCatalog.Find(
                    AlternateSummoningSourceCatalog.PreservationistKey);
                AlternateSummoningRootSpec tierOne = family.Roots.Single(value => value.Tier == 1);
                var feature = (BlueprintFeature)library.BlueprintsByAssetId[tierOne.FeatureGuid];
                var root = (BlueprintAbility)library.BlueprintsByAssetId[tierOne.RootGuid];
                // The formula book that knows the extract, recorded with every
                // spellbook the caster holds so a miss is diagnosable.
                Spellbook book = caster.Descriptor.Spellbooks.FirstOrDefault(value =>
                    value.IsKnown(root)) ?? caster.Descriptor.GetSpellbook(alchemist);
                bool known = book != null && book.IsKnown(root);
                record["spellbooks"] = new JArray(caster.Descriptor.Spellbooks.Select(value =>
                    new JObject { ["blueprint"] = value.Blueprint.AssetGuid + ":" + value.Blueprint.name,
                        ["casterLevel"] = value.CasterLevel,
                        ["known1"] = new JArray(value.GetKnownSpells(1).Select(spell =>
                            spell.Blueprint.AssetGuid + ":" + spell.Blueprint.name)) }));
                record["feature"] = caster.Descriptor.HasFact(feature);
                record["known"] = known;
                record["published"] = AlternateSummoningExposure.IsPublishedRoot(root);
                if (!caster.Descriptor.HasFact(feature) || !known)
                    throw new InvalidOperationException("Bottled Ally I is not granted as a known extract.");
                BlueprintAbility child = PlayerPathChild(root, true, "1");
                AbilityData data = PrepareExpandedSummoningPlayerPathSpell(book, root, child, 1);
                int slotsBefore = CountAvailableExpandedSummoningSlots(book, 1);
                object[] before = SnapshotReferences(fixture.SceneEntities);
                ExpandedSummoningRuleCapture.Clear();
                UnitUseAbility command;
                var parameters = new Kingmaker.AreaLogic.Cutscenes.CutsceneParametersContext();
                using (parameters.Data) command = new UnitUseAbility(data, new TargetWrapper(caster.Position));
                bool available = data.IsAvailable && command.CanStart;
                if (available)
                {
                    command.IgnoreCooldown(TimeSpan.Zero);
                    caster.Commands.Run(command);
                    command.Start();
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
                }
                Game.Instance.EntityCreator.Tick();
                UnitEntityData[] summoned = ExpandedSummoningRuleCapture.ToArray();
                int slotsAfter = CountAvailableExpandedSummoningSlots(book, 1);
                record["available"] = available;
                record["result"] = command.Result.ToString();
                record["slotsBefore"] = slotsBefore;
                record["slotsAfter"] = slotsAfter;
                record["summoned"] = string.Join(",", summoned.Select(value => value.Blueprint.name).ToArray());
                exact = available && command.Result.ToString() == "Success" &&
                    slotsAfter == slotsBefore - 1 && summoned.Length == 1 &&
                    ReferenceEquals(data.ConvertedFrom == null ? null : data.ConvertedFrom.Blueprint, root);
                foreach (UnitEntityData unit in summoned) CleanupExpandedSummoningUnit(unit);
                DrainExpandedSummoningDestroyQueue(fixture.SceneEntities, before);
                ExpandedSummoningRuleCapture.Clear();
            }
            catch (Exception exception)
            {
                record["exception"] = exception.GetType().Name + ":" + exception.Message;
            }
            document["family-Preservationist"] = record;
            casts.Add(record);
            assertions.Add(Assertion("player-family-Preservationist",
                "a level-2 Preservationist knows Bottled Ally I as an extract; a prepared slot opens a project creature through the root's variant data, consuming exactly that slot",
                record.ToString(Formatting.None), exact,
                "LevelUpController visits; Spellbook.Memorize + AbilityData(slot spell, child) + UnitUseAbility"));
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

        /// <summary>The exact frozen copy of a direct root's original tier-I
        /// cast: the family's own registered identity of that symbol.</summary>
        private static BlueprintAbility PlayerPathPreserved(string familyKey)
        {
            string name = AlternateSummoningAdapterBuilder.InternalName(
                AlternateSummoningProjection.PreservedSymbol(familyKey, 1, SummonMultiplicity.One));
            BlueprintAbility[] matches = BlueprintBootstrap.Library.BlueprintsByAssetId.Values
                .OfType<BlueprintAbility>().Where(value => value != null && value.name == name)
                .ToArray();
            if (matches.Length != 1)
                throw new InvalidOperationException("The preserved tier-I cast " + name +
                    " is not registered exactly once: " + matches.Length + ".");
            return matches[0];
        }

        private static JArray PlayerPathMenu(BlueprintAbility root)
        {
            return new JArray(root.ComponentsArray.OfType<AbilityVariants>().Single().Variants
                .Select(child =>
                {
                    SummonSpawn spawn = child == null ? null :
                        SummonSignatureReader.Read(child).Spawns().FirstOrDefault();
                    return new JObject { ["guid"] = child?.AssetGuid, ["name"] = child?.name,
                        ["exposed"] = child != null && AlternateSummoningExposure.IsExposed(child.AssetGuid),
                        ["unit"] = spawn?.Unit, ["count"] = spawn?.CountDice };
                }));
        }

        private static BlueprintAbility PlayerPathChildOrNull(BlueprintAbility root, bool project,
            string quantity)
        {
            try { return PlayerPathChild(root, project, quantity); }
            catch (InvalidOperationException) { return null; }
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
                if (archetype != null && owner.Progression.GetClassLevel(characterClass) == 0 &&
                    !controller.AddArchetype(characterClass, archetype))
                    throw new InvalidOperationException("Native archetype selection rejected " +
                        archetype.name + ".");
                if (!controller.SelectClass(characterClass, false))
                    throw new InvalidOperationException("Disposable caster class selection failed at level " +
                        (owner.Progression.GetClassLevel(characterClass) + 1) + ".");
                controller.ApplyClassMechanics();
                // The native visit's spellbook step, as the level-up harness
                // takes it: without it a prepared caster has no formula book.
                controller.ApplySpellbook();
                apply.Invoke(controller, new object[] { owner });
                controller.Cancel();
                activeController = null;
            }
            if (archetype != null && !owner.Progression.IsArchetype(archetype))
                throw new InvalidOperationException("The disposable caster did not keep " +
                    archetype.name + ".");
        }

        private UnitEntityData PlayerPathSecondCaster(ExpandedSummoningCorrectionFixture fixture,
            string name)
        {
            BlueprintUnit blueprint = UnityEngine.Object.Instantiate(fixture.CasterBlueprint);
            blueprint.name = name;
            blueprint.IsCheater = false;
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
