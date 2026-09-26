using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.FavoredClass;
using KingmakerGunslinger.FavoredClass.Mechanics;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private const string FcbDivineHunterGuid = "fec08c1a3187da549abd6b85f27e4432";
        private const string FcbDivineGuardianGuid = "5693945afac189a469ef970eac8f71d9";
        private const string FcbFlamewardenGuid = "300917c3479d27d47b4b4b52b1762e8d";
        private const string FcbAnimalCompanionRankGuid = "1670990255e4fe948a863bafd5dbda5d";
        private const string FcbCompanionLeopardGuid = "2ee2ba60850dd064e8b98bf5c2c946ba";
        private const string FcbCompanionWolfGuid = "67a9dc42b15d0954ca4689b13e8dedea";
        private const string FcbBarkskinBuffGuid = "533592a86adecda4e9fd5ed37a028432";
        private const string FcbAngelEidolonProgressionName = "AngelEidolonProgression";
        private const string FcbSorcererClassGuid = "b3a505fb61437dc4097f43c3f8f9a4cf";
        private const string FcbBloodlineSelectionGuid = "24bef8d1bee12274686f6da6ccbc8914";
        private const string FcbFireBloodlineGuid = "17cc794d47408bc4986c55265475c06f";
        private const string FcbAirBloodlineGuid = "cd788df497c6f10439c7025e87864ee4";
        private const string FcbFireRaySelectionGuid = "057754c2149e4acfa0c2a11896d1d6f7";
        private const string FcbAirRaySelectionGuid = "09a6540720424a798daf9e987420f624";
        private const string FcbFireRayFeatureGuid = "ce0889b5c1b392e48baf1e004d1efd67";
        private const string FcbAirRayFeatureGuid = "acf668c24dfbcdd499276eaf1881486e";
        private const string FcbFireBlastFeatureGuid = "3022a5066a5604a498dd289b37dfd8aa";
        private const string FcbFireRayAbilityGuid = "1b4989258e5964149a909e47c72b7f67";
        private const string FcbFireBlastAbilityGuid = "b2d1d39cd406e0f4185c52fecc73c3b5";
        private const string FcbBlastResourceGuid = "4c415d8268a451843a52d3a43fe2e4d2";
        private const string FcbRayResourceGuid = "ebbb59cea666c1249ba7da55addccfa2";

        // Phase 3 advanced rows: O06 paladin auras, O07 companion and O08
        // eidolon natural armor, with archetype, two-owner, replacement and
        // negative controls.
        private RuntimeTestResult RunFavoredClassElementalAdvanced()
        {
            var assertions = new List<RuntimeTestAssertion>();
            FavoredClassIntegrationStatus status = FavoredClassIntegrationStatusRegistry.Current;
            FavoredClassHostHandles host = FavoredClassIntegrationCoordinator.Host;
            FavoredClassBlueprintSet leaves = BlueprintBootstrap.FavoredClassLeaves;
            bool ready = status.Availability == FavoredClassIntegrationAvailability.Published &&
                host != null && leaves != null && FavoredClassRuntime.MechanicsEnabled &&
                FavoredClassIntegrationCoordinator.Aura != null;
            assertions.Add(Assertion("fcb-advanced-ready",
                "the exact host is published, mechanics are enabled and the native aura read point is committed",
                status + ";aura=" + (FavoredClassIntegrationCoordinator.Aura == null ? "none" :
                    string.Join("|", FavoredClassIntegrationCoordinator.Aura.Evidence.ToArray())),
                ready, "FavoredClassIntegrationStatusRegistry and FavoredClassIntegrationCoordinator.Aura"));
            if (!ready)
                return CreateResult(RuntimeTestStatuses.Fail, assertions, null);
            object player = ReadExactMember(Game.Instance, "Player");
            object state = ReadExactMember(Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);

            var evidence = new JObject();
            var menuFailures = new List<string>();
            var auraFailures = new List<string>();
            var companionFailures = new List<string>();
            var eidolonFailures = new List<string>();
            var powerFailures = new List<string>();
            bool cleaned = false;
            try
            {
                evidence["menus"] = RunAdvancedMenus(host, leaves, menuFailures);
                evidence["auras"] = ObserveAuraBonuses(leaves, auraFailures);
                evidence["pets"] = ObservePetArmor(leaves, companionFailures, eidolonFailures);
                evidence["bloodlinePowers"] = ObserveBloodlinePowers(host, leaves, powerFailures);
            }
            catch (Exception exception)
            {
                auraFailures.Add("exception=" + exception);
            }
            finally
            {
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits));
            }
            string evidencePath = WriteFavoredClassEvidence("favored-class-elemental-advanced.json", evidence);
            assertions.Add(Assertion("fcb-advanced-menus",
                "the Oread Paladin, Ranger and Summoner menus offer their counters; Humans get none; archetypes that replace every improved feature (Divine Hunter, Flamewarden) are not offered them, while Divine Guardian keeps the aura counter",
                Describe(evidence["menus"], menuFailures), menuFailures.Count == 0,
                "level-1 native visits per class/archetype; BlueprintFeatureSelection.CanSelect"));
            assertions.Add(Assertion("fcb-advanced-aura-bonus",
                "an ally inside each paladin's native ally buff gains exactly 4 + that paladin's earned steps against fear (Courage) and charm (Resolve) in the same Morale modifier; other saves are unchanged",
                Describe(evidence["auras"], auraFailures), auraFailures.Count == 0,
                "native Aura of Courage/Resolve ally buffs applied in each paladin's context; RuleSavingThrow.StatValue"));
            assertions.Add(Assertion("fcb-advanced-companion-armor",
                "the native companion gains exactly the ranger's earned steps as stacking natural armor (touch AC and the ranger's AC unchanged, Barkskin stacks), follows a replacement companion and leaves the old one",
                Describe(evidence["pets"] == null ? null : evidence["pets"]["companion"], companionFailures),
                companionFailures.Count == 0,
                "native AddPet spawn/level and SetMaster in the save-free fixture scene; Stats.AC"));
            assertions.Add(Assertion("fcb-advanced-eidolon-armor",
                "Call of the Wild's eidolon gains exactly the summoner's earned steps as natural armor",
                Describe(evidence["pets"] == null ? null : evidence["pets"]["eidolon"], eidolonFailures),
                eidolonFailures.Count == 0,
                "native AddPet of Call of the Wild's eidolon progression in the save-free fixture scene"));
            assertions.Add(Assertion("fcb-advanced-bloodline-powers",
                "an Ifrit or Sylph Sorcerer is offered only its own element's owned powers, and its level-1 reward pick counts the ray it chooses in the same level-up only inside that replayed pick's finally-closed scope (a failure injected there leaves nothing marked and the retried replay applies the reward); for fire (I08) and air (S06) alike, two steps in Elemental Blast raise exactly its caster level, dice and DC by the effective-level rule and its own extra uses at 17th and 20th level follow the effective bloodline level (at most two steps), two steps in Elemental Ray raise exactly its damage bonus rank, and the other power, Elemental Ray's uses and an unrelated spell are unchanged",
                Describe(evidence["bloodlinePowers"], powerFailures), powerFailures.Count == 0,
                "level-1 native Sorcerer visits with the chosen bloodline; AbilityData.CreateExecutionContext params and ranks"));
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

        private JArray RunAdvancedMenus(FavoredClassHostHandles host, FavoredClassBlueprintSet leaves,
            IList<string> failures)
        {
            var library = BlueprintBootstrap.Library;
            Func<string, BlueprintArchetype> archetype = guid =>
                BlueprintLibraryLookup.RequireExact<BlueprintArchetype>(library, guid, guid);
            var cases = new[]
            {
                Tuple.Create(FavoredClassAncestry.Oread, FavoredClassCatalog.Paladin, (string)null,
                    new[] { FavoredClassCatalog.EffectPaladinAuras }),
                Tuple.Create(FavoredClassAncestry.Human, FavoredClassCatalog.Paladin, (string)null, new string[0]),
                Tuple.Create(FavoredClassAncestry.Oread, FavoredClassCatalog.Paladin, FcbDivineHunterGuid, new string[0]),
                Tuple.Create(FavoredClassAncestry.Oread, FavoredClassCatalog.Paladin, FcbDivineGuardianGuid,
                    new[] { FavoredClassCatalog.EffectPaladinAuras }),
                Tuple.Create(FavoredClassAncestry.Oread, FavoredClassCatalog.Ranger, (string)null,
                    new[] { FavoredClassCatalog.EffectCompanionArmor }),
                Tuple.Create(FavoredClassAncestry.Human, FavoredClassCatalog.Ranger, (string)null, new string[0]),
                Tuple.Create(FavoredClassAncestry.Oread, FavoredClassCatalog.Ranger, FcbFlamewardenGuid, new string[0]),
                Tuple.Create(FavoredClassAncestry.Oread, FavoredClassCatalog.Summoner, (string)null,
                    new[] { FavoredClassCatalog.EffectEidolonArmor }),
                Tuple.Create(FavoredClassAncestry.Human, FavoredClassCatalog.Summoner, (string)null, new string[0]),
            };
            var rows = new JArray();
            foreach (var entry in cases)
            {
                var row = new JObject { ["ancestry"] = entry.Item1, ["class"] = entry.Item2,
                    ["archetype"] = entry.Item3 };
                UnitEntityData unit = null;
                LevelUpController controller = null;
                try
                {
                    FavoredClassLeafPair[] classPairs = leaves.Pairs.Where(pair =>
                        pair.Effect.ClassFamily == entry.Item2).ToArray();
                    BlueprintScriptableObject classBlueprint;
                    library.BlueprintsByAssetId.TryGetValue(classPairs[0].HostClassGuid, out classBlueprint);
                    var characterClass = classBlueprint as BlueprintCharacterClass;
                    BlueprintFeatureSelection selection = characterClass == null ? null :
                        host.BonusSelectionFor(characterClass.AssetGuid);
                    if (characterClass == null || selection == null)
                    {
                        row["provider"] = "absent";
                        failures.Add(entry.Item2 + ": class or host bonus selection unavailable");
                        rows.Add(row);
                        continue;
                    }
                    BlueprintRace race = BlueprintLibraryLookup.RequireExact<BlueprintRace>(library,
                        FavoredClassRaceIdentities.ForAncestry(entry.Item1).RaceGuid, entry.Item1);
                    unit = FavoredClassLevelUpHarness.CreateUnit(14);
                    controller = FavoredClassLevelUpHarness.Open(unit.Descriptor, race, characterClass,
                        "KMG FCB Advanced Menu", entry.Item3 == null ? null : archetype(entry.Item3));
                    if (FavoredClassLevelUpHarness.ChooseFavoredClass(controller, characterClass, row) == null)
                    {
                        failures.Add(entry.Item1 + "/" + entry.Item2 + ": favored class progression unavailable");
                        rows.Add(row);
                        continue;
                    }
                    FavoredClassLevelUpHarness.FillOthers(controller,
                        new HashSet<string>(StringComparer.Ordinal) { selection.AssetGuid });
                    FeatureSelectionState fcb = FavoredClassLevelUpHarness.FindOpenState(controller,
                        selection.AssetGuid);
                    string[] offered = fcb == null ? new string[0] : classPairs.Where(pair =>
                        FavoredClassLevelUpHarness.CanSelect(controller, fcb, pair.Full) ||
                        (pair.Partial != null && FavoredClassLevelUpHarness.CanSelect(controller, fcb, pair.Partial)))
                        .Select(pair => pair.Effect.Id).Distinct().ToArray();
                    row["offered"] = new JArray(offered);
                    if (!offered.OrderBy(value => value, StringComparer.Ordinal).SequenceEqual(
                            entry.Item4.OrderBy(value => value, StringComparer.Ordinal)))
                        failures.Add(entry.Item1 + "/" + entry.Item2 + (entry.Item3 == null ? "" : "/" +
                            entry.Item3) + ": offered " + string.Join(",", offered) + " expected " +
                            string.Join(",", entry.Item4));
                }
                catch (Exception exception)
                {
                    failures.Add(entry.Item1 + "/" + entry.Item2 + ": " + exception.GetType().Name + ": " +
                        exception.Message);
                }
                finally
                {
                    FavoredClassLevelUpHarness.Close(controller);
                    if (unit != null) unit.Dispose();
                }
                rows.Add(row);
            }
            return rows;
        }

        /// <summary>
        /// Review finding 5: the reward pick of a level-1 Sorcerer that chose
        /// the Fire bloodline and its ray replays before them (its native
        /// priority is earlier), so its owned-target check counts the pending
        /// ray. A failure injected inside that replayed pick's scope must
        /// leave nothing marked; the retried native replay counts the pending
        /// ray again and applies the reward.
        /// </summary>
        private static JObject ObserveReplayScope(LevelUpController controller, FeatureSelectionState fcb,
            FavoredClassLeafPair pair, BlueprintFeature ray, IList<string> failures)
        {
            var row = new JObject { ["installed"] = FavoredClassPendingPicks.Installed };
            if (fcb == null || pair == null || ray == null)
            {
                failures.Add("replay scope: the Fire Ray reward state or leaf is missing");
                return row;
            }
            BlueprintFeature leaf = FavoredClassLevelUpHarness.CanSelect(controller, fcb, pair.Full) ? pair.Full :
                pair.Partial;
            IFeatureSelection reward = fcb.Selection;
            bool faulted = false;
            string thrown = null;
            FavoredClassPendingPicks.FaultInjection = action =>
            {
                var pick = action as SelectFeature;
                if (faulted || pick == null || pick.Selection != reward)
                    return;
                faulted = true;
                throw new InvalidOperationException("KMG injected failure inside a replayed pick");
            };
            try
            {
                FavoredClassLevelUpHarness.Select(controller, fcb, leaf);
            }
            catch (Exception exception)
            {
                thrown = exception.GetType().Name + ": " + exception.Message;
            }
            finally
            {
                FavoredClassPendingPicks.FaultInjection = null;
            }
            row["leaf"] = leaf == null ? null : leaf.name;
            row["faulted"] = faulted;
            row["thrown"] = thrown;
            row["depthAfterFailure"] = FavoredClassPendingPicks.Depth;
            row["pendingRayCountedAfterFailure"] = FavoredClassPendingPicks.Selects(controller.State,
                new[] { ray.AssetGuid });
            row["leafRankAfterFailure"] = controller.Preview.Progression.Features.GetRank(leaf);
            // The retry: the same native replay of the same picks.
            try
            {
                typeof(LevelUpController).GetField("m_RecalculatePreview", BindingFlags.Instance |
                    BindingFlags.NonPublic).SetValue(controller, true);
                typeof(LevelUpController).GetMethod("UpdatePreview", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(controller, null);
            }
            catch (Exception exception)
            {
                row["retryThrown"] = (exception.InnerException ?? exception).GetType().Name + ": " +
                    (exception.InnerException ?? exception).Message;
            }
            row["depthAfterRetry"] = FavoredClassPendingPicks.Depth;
            row["leafRankAfterRetry"] = controller.Preview.Progression.Features.GetRank(leaf);
            row["rayAfterRetry"] = controller.Preview.HasFact(ray);
            if (!FavoredClassPendingPicks.Installed)
                failures.Add("replay scope: the scoped native replay is not installed");
            if (!faulted || thrown == null)
                failures.Add("replay scope: the injected failure did not interrupt the reward's replayed pick");
            if ((int)row["depthAfterFailure"] != 0 || (bool)row["pendingRayCountedAfterFailure"])
                failures.Add("replay scope: the interrupted replay left its pending picks marked");
            if ((int)row["leafRankAfterFailure"] != 0)
                failures.Add("replay scope: the interrupted pick was applied");
            if (row["retryThrown"] != null || (int)row["depthAfterRetry"] != 0 ||
                (int)row["leafRankAfterRetry"] != 1 || !(bool)row["rayAfterRetry"])
                failures.Add("replay scope: the retried replay did not count the pending ray and apply the reward");
            return row;
        }

        private JObject ObserveBloodlinePowers(FavoredClassHostHandles host, FavoredClassBlueprintSet leaves,
            IList<string> failures)
        {
            var result = new JObject();
            var library = BlueprintBootstrap.Library;
            string effect = FavoredClassCatalog.EffectSelectedBloodlinePower;
            BlueprintCharacterClass sorcerer = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(library,
                FcbSorcererClassGuid, "Sorcerer");
            BlueprintFeatureSelection bonus = host.BonusSelectionFor(sorcerer.AssetGuid);
            FavoredClassLeafPair[] pairs = leaves.Pairs.Where(pair => pair.Effect.Id == effect).ToArray();
            Func<string, BlueprintFeature> feature = guid =>
            {
                BlueprintScriptableObject value;
                library.BlueprintsByAssetId.TryGetValue(guid, out value);
                return value as BlueprintFeature;
            };
            // Native level-1 menus with the chosen bloodline and its first power.
            var cases = new[]
            {
                Tuple.Create(FavoredClassAncestry.Ifrit, FcbFireBloodlineGuid, FcbFireRaySelectionGuid,
                    FcbFireRayFeatureGuid, new[] { "FireRay" }),
                Tuple.Create(FavoredClassAncestry.Sylph, FcbAirBloodlineGuid, FcbAirRaySelectionGuid,
                    FcbAirRayFeatureGuid, new[] { "AirRay" }),
                Tuple.Create(FavoredClassAncestry.Human, FcbFireBloodlineGuid, FcbFireRaySelectionGuid,
                    FcbFireRayFeatureGuid, new string[0]),
                Tuple.Create(FavoredClassAncestry.Ifrit, FcbAirBloodlineGuid, FcbAirRaySelectionGuid,
                    FcbAirRayFeatureGuid, new string[0]),
            };
            var menus = new JArray();
            foreach (var entry in cases)
            {
                var row = new JObject { ["ancestry"] = entry.Item1, ["bloodline"] = entry.Item2 };
                UnitEntityData unit = FavoredClassLevelUpHarness.CreateUnit(14);
                LevelUpController controller = null;
                try
                {
                    BlueprintRace race = BlueprintLibraryLookup.RequireExact<BlueprintRace>(library,
                        FavoredClassRaceIdentities.ForAncestry(entry.Item1).RaceGuid, entry.Item1);
                    controller = FavoredClassLevelUpHarness.Open(unit.Descriptor, race, sorcerer,
                        "KMG FCB Bloodline Menu");
                    FeatureSelectionState bloodlineState = FavoredClassLevelUpHarness.FindOpenState(controller,
                        FcbBloodlineSelectionGuid);
                    row["bloodlineSelected"] = bloodlineState != null && FavoredClassLevelUpHarness.Select(
                        controller, bloodlineState, feature(entry.Item2));
                    FeatureSelectionState rayState = FavoredClassLevelUpHarness.FindOpenState(controller,
                        entry.Item3);
                    row["raySelection"] = rayState == null ? "none" : "open";
                    if (rayState != null)
                        row["raySelected"] = FavoredClassLevelUpHarness.Select(controller, rayState,
                            feature(entry.Item4));
                    row["rayOwned"] = controller.Preview.HasFact(feature(entry.Item4));
                    FavoredClassLevelUpHarness.ChooseFavoredClass(controller, sorcerer, row);
                    FavoredClassLevelUpHarness.FillOthers(controller,
                        new HashSet<string>(StringComparer.Ordinal) { bonus.AssetGuid });
                    FeatureSelectionState fcb = FavoredClassLevelUpHarness.FindOpenState(controller,
                        bonus.AssetGuid);
                    string[] offered = fcb == null ? new string[0] : pairs.Where(pair =>
                        FavoredClassLevelUpHarness.CanSelect(controller, fcb, pair.Full) ||
                        (pair.Partial != null && FavoredClassLevelUpHarness.CanSelect(controller, fcb, pair.Partial)))
                        .Select(pair => pair.TargetKey).ToArray();
                    row["offered"] = new JArray(offered);
                    // Review finding 5 on the real same-level replay (Ifrit, Fire).
                    if (entry.Item1 == FavoredClassAncestry.Ifrit && entry.Item2 == FcbFireBloodlineGuid)
                        result["replayScope"] = ObserveReplayScope(controller, fcb,
                            pairs.FirstOrDefault(pair => pair.TargetKey == "FireRay"), feature(entry.Item4), failures);
                    if (!(bool)row["rayOwned"])
                        failures.Add(entry.Item1 + " " + entry.Item2 + ": the first power was not gained");
                    if (!offered.SequenceEqual(entry.Item5))
                        failures.Add(entry.Item1 + " " + entry.Item2 + ": offered " + string.Join(",", offered) +
                            " expected " + string.Join(",", entry.Item5));
                }
                catch (Exception exception)
                {
                    failures.Add(entry.Item1 + ": " + exception.GetType().Name + ": " + exception.Message);
                }
                finally
                {
                    FavoredClassLevelUpHarness.Close(controller);
                    unit.Dispose();
                }
                menus.Add(row);
            }
            result["menus"] = menus;

            // Level-9 probes: real Sorcerer levels and the native power
            // features, for both elements (I08 fire, S06 air).
            var units = new List<UnitEntityData>();
            try
            {
                Func<UnitEntityData> create = () =>
                {
                    var unit = new Kingmaker.UI.LevelUp.ChargenUnit(BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
                    units.Add(unit);
                    return unit;
                };
                var fireball = BlueprintLibraryLookup.RequireExact<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>(
                    library, FcbFireballGuid, "Fireball");
                UnitEntityData target = create();
                foreach (var element in new[]
                {
                    Tuple.Create("fire", FcbFireBloodlineGuid, "FireRay", "FireBlast"),
                    Tuple.Create("air", FcbAirBloodlineGuid, "AirRay", "AirBlast"),
                })
                {
                    FavoredClassSelectedPowerLevel rayBinding = leaves.Pair(effect, element.Item3).Full
                        .GetComponent<FavoredClassSelectedPowerLevel>();
                    FavoredClassSelectedPowerLevel blastBinding = leaves.Pair(effect, element.Item4).Full
                        .GetComponent<FavoredClassSelectedPowerLevel>();
                    if (rayBinding == null || blastBinding == null || rayBinding.Ability == null ||
                        blastBinding.Ability == null || rayBinding.PowerFeature == null ||
                        blastBinding.PowerFeature == null)
                    {
                        failures.Add(element.Item1 + ": a power counter has no exact power binding");
                        continue;
                    }
                    BlueprintFeature ray = rayBinding.PowerFeature, blast = blastBinding.PowerFeature;
                    var rayAbility = rayBinding.Ability;
                    var blastAbility = blastBinding.Ability;
                    Func<UnitEntityData> sorcererAtNine = () =>
                    {
                        UnitEntityData unit = create();
                        for (int added = 0; added < 9; added++)
                            unit.Descriptor.Progression.AddClassLevel(sorcerer);
                        unit.Descriptor.AddFact(ray);
                        unit.Descriptor.AddFact(blast);
                        return unit;
                    };
                    UnitEntityData control = sorcererAtNine();
                    UnitEntityData rayUnit = sorcererAtNine();
                    UnitEntityData blastUnit = sorcererAtNine();
                    GrantFavoredClassRanks(rayUnit, leaves.Pair(effect, element.Item3).Full, 2);
                    GrantFavoredClassRanks(blastUnit, leaves.Pair(effect, element.Item4).Full, 2);
                    Func<UnitEntityData, Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility, JObject> probe =
                        (caster, ability) =>
                        {
                            var data = new Kingmaker.UnitLogic.Abilities.AbilityData(ability, caster.Descriptor);
                            var context = data.CreateExecutionContext(new TargetWrapper(target));
                            context.Recalculate();
                            return new JObject
                            {
                                ["casterLevel"] = context.Params.CasterLevel,
                                ["dc"] = context.Params.DC,
                                ["rankBonus"] = context.Params.RankBonus,
                                ["damageDice"] = context[AbilityRankType.DamageDice],
                                ["damageBonus"] = context[AbilityRankType.DamageBonus]
                            };
                        };
                    Func<JObject, JObject, string, int> delta = (after, before, key) => (int)after[key] - (int)before[key];
                    JObject blastControl = probe(control, blastAbility), blastInvested = probe(blastUnit, blastAbility),
                        blastNeighbor = probe(rayUnit, blastAbility), rayControl = probe(control, rayAbility),
                        rayInvested = probe(rayUnit, rayAbility), rayNeighbor = probe(blastUnit, rayAbility),
                        fireballControl = probe(control, fireball), fireballInvested = probe(blastUnit, fireball);
                    var row = new JObject
                    {
                        ["sorcererLevel"] = control.Descriptor.Progression.GetClassLevel(sorcerer),
                        ["blastAbility"] = blastAbility.AssetGuid,
                        ["rayAbility"] = rayAbility.AssetGuid,
                        ["blastControl"] = blastControl,
                        ["blastInvested"] = blastInvested,
                        ["blastNeighbor"] = blastNeighbor,
                        ["rayControl"] = rayControl,
                        ["rayInvested"] = rayInvested,
                        ["rayNeighbor"] = rayNeighbor,
                        ["fireballControl"] = fireballControl,
                        ["fireballInvested"] = fireballInvested
                    };
                    result[element.Item1] = row;
                    string label = element.Item1 + ": ";
                    if (delta(blastInvested, blastControl, "casterLevel") != 2 ||
                        delta(blastInvested, blastControl, "rankBonus") != 2 ||
                        delta(blastInvested, blastControl, "damageDice") != 2)
                        failures.Add(label + "two Blast steps did not add exactly +2 caster level, rank bonus and dice");
                    int level = (int)row["sorcererLevel"];
                    int expectedDc = FavoredClassMechanicsPolicy.HalfLevelDelta(level, 2);
                    if (delta(blastInvested, blastControl, "dc") != expectedDc)
                        failures.Add(label + "the Blast DC changed by " + delta(blastInvested, blastControl, "dc") +
                            ", expected " + expectedDc);
                    int expectedRay = (level + 2) / 2 - level / 2;
                    if (delta(rayInvested, rayControl, "damageBonus") != expectedRay ||
                        delta(rayInvested, rayControl, "rankBonus") != 2)
                        failures.Add(label + "two Ray steps did not raise exactly the Ray's damage bonus rank");
                    foreach (var pair in new[] { Tuple.Create(blastNeighbor, blastControl),
                        Tuple.Create(rayNeighbor, rayControl), Tuple.Create(fireballInvested, fireballControl) })
                        foreach (string key in new[] { "casterLevel", "dc", "rankBonus", "damageDice", "damageBonus" })
                            if (delta(pair.Item1, pair.Item2, key) != 0)
                                failures.Add(label + "an unchosen power or spell changed " + key);
                    row["blastUses"] = ObserveBlastUseThresholds(create, sorcerer, element.Item2, element.Item4,
                        ray, blast, leaves, failures);
                }
            }
            catch (Exception exception)
            {
                failures.Add("probe: " + exception.GetType().Name + ": " + exception.Message);
            }
            finally
            {
                foreach (UnitEntityData unit in units)
                    try { unit.Dispose(); } catch (Exception) { }
            }
            return result;
        }

        // Review finding 2 / charter 8.10: Elemental Blast's own use
        // thresholds (the bloodline's extra uses at 17th and 20th level) follow
        // the effective level of the owner's bloodline, at most two steps;
        // Elemental Ray's uses never change.
        private static JObject ObserveBlastUseThresholds(Func<UnitEntityData> create, BlueprintCharacterClass sorcerer,
            string bloodlineGuid, string blastKey, BlueprintFeature ray, BlueprintFeature blast,
            FavoredClassBlueprintSet leaves, IList<string> failures)
        {
            var library = BlueprintBootstrap.Library;
            string effect = FavoredClassCatalog.EffectSelectedBloodlinePower;
            var progression = BlueprintLibraryLookup.RequireExact<BlueprintProgression>(library, bloodlineGuid,
                "Elemental bloodline " + blastKey);
            var blastResource = BlueprintLibraryLookup.RequireExact<BlueprintAbilityResource>(library,
                FcbBlastResourceGuid, "Elemental Blast resource");
            var rayResource = BlueprintLibraryLookup.RequireExact<BlueprintAbilityResource>(library,
                FcbRayResourceGuid, "Elemental Ray resource");
            // The native layout keeps them in the bloodline's level entries;
            // Call of the Wild moves them into the Blast feature's own gates.
            IList<KeyValuePair<int, int>> thresholds =
                FavoredClassSelectedPowerLevel.UseThresholds(progression, blastResource);
            var gates = FavoredClassSelectedPowerLevel.UseGates(blast, blastResource);
            var row = new JObject
            {
                ["levelEntries"] = new JArray(thresholds.Select(value => value.Key + ":" + value.Value)),
                ["powerGates"] = new JArray(gates.Select(value => (value.Key.BeforeThisLevel ? "<" : "@") +
                    value.Key.Level + ":" + value.Value))
            };
            int[] useLevels = thresholds.Select(value => value.Key).Concat(gates.Where(value =>
                !value.Key.BeforeThisLevel).Select(value => value.Key.Level)).OrderBy(value => value).ToArray();
            if (!useLevels.SequenceEqual(new[] { 17, 20 }) || gates.Any(value => value.Key.BeforeThisLevel))
                failures.Add(blastKey + ": the Blast's own use thresholds were not exactly 17 and 20");
            Func<int, int, UnitEntityData> sorcererAt = (levels, steps) =>
            {
                UnitEntityData unit = create();
                for (int added = 0; added < levels; added++)
                    unit.Descriptor.Progression.AddClassLevel(sorcerer);
                unit.Descriptor.AddFact(progression);
                unit.Descriptor.AddFact(ray);
                unit.Descriptor.AddFact(blast);
                if (steps > 0)
                    GrantFavoredClassRanks(unit, leaves.Pair(effect, blastKey).Full, steps);
                return unit;
            };
            // (real level, steps, extra uses beyond the native ones)
            var cases = new[]
            {
                Tuple.Create(15, 0, 0), Tuple.Create(15, 1, 0), Tuple.Create(15, 2, 1),
                Tuple.Create(16, 1, 1), Tuple.Create(17, 2, 0), Tuple.Create(18, 2, 1), Tuple.Create(20, 2, 0)
            };
            var probes = new JArray();
            foreach (var entry in cases)
            {
                UnitEntityData control = sorcererAt(entry.Item1, 0);
                UnitEntityData invested = sorcererAt(entry.Item1, entry.Item2);
                int blastControl = blastResource.GetMaxAmount(control.Descriptor);
                int blastInvested = blastResource.GetMaxAmount(invested.Descriptor);
                int rayControl = rayResource.GetMaxAmount(control.Descriptor);
                int rayInvested = rayResource.GetMaxAmount(invested.Descriptor);
                probes.Add(new JObject
                {
                    ["level"] = entry.Item1,
                    ["steps"] = entry.Item2,
                    ["progressionLevel"] = progression.CalcLevel(invested.Descriptor),
                    ["blastControl"] = blastControl,
                    ["blastInvested"] = blastInvested,
                    ["rayControl"] = rayControl,
                    ["rayInvested"] = rayInvested
                });
                if (blastInvested - blastControl != entry.Item3)
                    failures.Add(blastKey + " at sorcerer " + entry.Item1 + " with " + entry.Item2 + " steps gained " +
                        (blastInvested - blastControl) + " uses, expected " + entry.Item3);
                if (rayInvested != rayControl)
                    failures.Add(blastKey + ": a Blast investment changed Elemental Ray's uses");
            }
            row["probes"] = probes;
            return row;
        }

        private static JObject ObserveAuraBonuses(FavoredClassBlueprintSet leaves, IList<string> failures)
        {
            var library = BlueprintBootstrap.Library;
            var row = new JObject();
            BlueprintBuff courage = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library,
                FavoredClassAuraPublication.CourageEffectBuffGuid, "Aura of Courage ally buff");
            BlueprintBuff resolve = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library,
                FavoredClassAuraPublication.ResolveEffectBuffGuid, "Aura of Resolve ally buff");
            BlueprintFeature full = leaves.Pair(FavoredClassCatalog.EffectPaladinAuras, null).Full;
            var disposables = new List<UnitEntityData>();
            Func<UnitEntityData> create = () =>
            {
                UnitEntityData unit = new Kingmaker.UI.LevelUp.ChargenUnit(
                    BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
                disposables.Add(unit);
                return unit;
            };
            try
            {
                UnitEntityData paladinTwo = create();
                UnitEntityData paladinOne = create();
                UnitEntityData paladinZero = create();
                UnitEntityData ally = create();
                UnitEntityData source = create();
                GrantFavoredClassRanks(paladinTwo, full, 2);
                GrantFavoredClassRanks(paladinOne, full, 1);
                // A neutral (force) reason context; only the added descriptor varies.
                var neutral = BlueprintLibraryLookup.RequireExact<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>(
                    library, FcbMagicMissileGuid, "MagicMissile");
                Func<SpellDescriptor, int> save = descriptor =>
                {
                    var context = new MechanicsContext(source, source.Descriptor, neutral);
                    if (descriptor != SpellDescriptor.None)
                        context.AddSpellDescriptor(descriptor);
                    using (context.GetDataScope(new TargetWrapper(ally)))
                        return context.TriggerRule(new RuleSavingThrow(ally, SavingThrowType.Will, 10)).StatValue;
                };
                Func<UnitEntityData, BlueprintBuff, SpellDescriptor, int> within = (paladin, buff, descriptor) =>
                {
                    var parent = new MechanicsContext(paladin, paladin.Descriptor, buff);
                    Buff applied = ally.Descriptor.AddBuff(buff, parent);
                    try { return save(descriptor); }
                    finally { if (applied != null) applied.Remove(); }
                };
                int plainFear = save(SpellDescriptor.Fear);
                int plainCharm = save(SpellDescriptor.Charm);
                row["noAuraFear"] = plainFear;
                row["courageZeroFear"] = within(paladinZero, courage, SpellDescriptor.Fear) - plainFear;
                row["courageOneFear"] = within(paladinOne, courage, SpellDescriptor.Fear) - plainFear;
                row["courageTwoFear"] = within(paladinTwo, courage, SpellDescriptor.Fear) - plainFear;
                row["courageTwoPlain"] = within(paladinTwo, courage, SpellDescriptor.None) - save(SpellDescriptor.None);
                row["resolveZeroCharm"] = within(paladinZero, resolve, SpellDescriptor.Charm) - plainCharm;
                row["resolveTwoCharm"] = within(paladinTwo, resolve, SpellDescriptor.Charm) - plainCharm;
                row["resolveTwoFear"] = within(paladinTwo, resolve, SpellDescriptor.Fear) - plainFear;
                row["paladinOwnFearWithoutAura"] = save(SpellDescriptor.Fear) - plainFear;
                if ((int)row["courageZeroFear"] != FavoredClassAuraPublication.NativeMoraleValue)
                    failures.Add("the native Courage bonus is not +4 without investment");
                if ((int)row["courageOneFear"] != 5 || (int)row["courageTwoFear"] != 6)
                    failures.Add("Courage does not add each paladin's own earned steps");
                if ((int)row["courageTwoPlain"] != 0)
                    failures.Add("Courage changed a save without the fear descriptor");
                if ((int)row["resolveZeroCharm"] != 4 || (int)row["resolveTwoCharm"] != 6)
                    failures.Add("Resolve does not add the earned steps against charm");
                if ((int)row["resolveTwoFear"] != 0)
                    failures.Add("Resolve changed a fear save");
            }
            finally
            {
                foreach (UnitEntityData unit in disposables)
                    try { unit.Dispose(); } catch (Exception) { }
            }
            return row;
        }

        private JObject ObservePetArmor(FavoredClassBlueprintSet leaves, IList<string> companionFailures,
            IList<string> eidolonFailures)
        {
            var result = new JObject();
            var diagnostics = new List<string>();
            var library = BlueprintBootstrap.Library;
            var scene = new ElementalUndineFeatScenario.PortalHarness(diagnostics);
            var pets = new List<UnitEntityData>();
            try
            {
                BlueprintRace human = BlueprintRoot.Instance.Progression.CharacterRaces.Single(race =>
                    race.AssetGuid == FcbHumanRace);
                UnitEntityData anchor = scene.Initialize(human);
                BlueprintFeature rank = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library,
                    FcbAnimalCompanionRankGuid, "AnimalCompanionRank");
                result["companion"] = ObserveCompanion(scene, anchor, human, rank, leaves, pets, companionFailures);
                result["eidolon"] = ObserveEidolon(scene, anchor, human, leaves, pets, eidolonFailures);
                result["diagnostics"] = new JArray(diagnostics);
            }
            finally
            {
                foreach (UnitEntityData pet in pets.Where(value => value != null).Distinct())
                    try
                    {
                        if (pet.Descriptor.Master.Value != null) pet.Descriptor.SetMaster(null);
                        pet.Destroy();
                    }
                    catch (Exception) { }
                try
                {
                    Game.Instance.EntityDestroyer.Tick();
                    Game.Instance.EntityDestroyer.Tick();
                }
                catch (Exception) { }
                scene.Dispose();
            }
            return result;
        }

        private static JObject ObserveCompanion(ElementalUndineFeatScenario.PortalHarness scene,
            UnitEntityData anchor, BlueprintRace race, BlueprintFeature rank, FavoredClassBlueprintSet leaves,
            List<UnitEntityData> pets, IList<string> failures)
        {
            var row = new JObject();
            var library = BlueprintBootstrap.Library;
            BlueprintFeature leopard = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library,
                FcbCompanionLeopardGuid, "AnimalCompanionFeatureLeopard");
            BlueprintFeature wolf = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library,
                FcbCompanionWolfGuid, "AnimalCompanionFeatureWolf");
            BlueprintBuff barkskin = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library,
                FcbBarkskinBuffGuid, "BarkskinBuff");
            FavoredClassLeafPair pair = leaves.Pair(FavoredClassCatalog.EffectCompanionArmor, null);
            UnitEntityData ranger = scene.SpawnFixtureUnit(race, anchor.Blueprint.Faction,
                new Vector3(2, 0, 0), "FavoredClassRanger");
            GrantFavoredClassRanks(ranger, rank, 4);
            ranger.Descriptor.AddFact(leopard);
            Game.Instance.EntityCreator.Tick();
            UnitEntityData first = ranger.Descriptor.Pet;
            if (first == null)
            {
                failures.Add("native AddPet spawned no companion in the fixture scene");
                return row;
            }
            pets.Add(first);
            string companionClass = BlueprintRoot.Instance.Progression.AnimalCompanion.AssetGuid;
            row["petQualified"] = FavoredClassPets.IsQualified(first, companionClass);
            row["petLevel"] = first.Descriptor.Progression.CharacterLevel;
            int ac0 = first.Stats.AC.ModifiedValue, touch0 = first.Stats.AC.Touch,
                flat0 = first.Stats.AC.FlatFooted, rangerAc0 = ranger.Stats.AC.ModifiedValue;
            GrantFavoredClassRanks(ranger, pair.Full, 2);
            row["acDelta"] = first.Stats.AC.ModifiedValue - ac0;
            row["touchDelta"] = first.Stats.AC.Touch - touch0;
            row["flatFootedDelta"] = first.Stats.AC.FlatFooted - flat0;
            row["rangerAcDelta"] = ranger.Stats.AC.ModifiedValue - rangerAc0;
            if (!(bool)row["petQualified"])
                failures.Add("the native companion is not recognized as an animal companion");
            if ((int)row["acDelta"] != 2 || (int)row["flatFootedDelta"] != 2)
                failures.Add("two earned steps did not add +2 natural armor to the companion");
            if ((int)row["touchDelta"] != 0)
                failures.Add("the companion's touch AC changed");
            if ((int)row["rangerAcDelta"] != 0)
                failures.Add("the ranger's own AC changed");
            // Barkskin (enhancement to natural armor) stacks.
            var context = new MechanicsContext(ranger, ranger.Descriptor, barkskin);
            Buff bark = first.Descriptor.AddBuff(barkskin, context);
            int withBark = first.Stats.AC.ModifiedValue;
            row["barkskinApplied"] = bark != null;
            row["acWithBarkskinDelta"] = withBark - ac0;
            if (bark != null) bark.Remove();
            if (bark == null || withBark - ac0 <= 2)
                failures.Add("Barkskin did not stack with the favored-class natural armor");
            // A further investment refreshes the current companion.
            GrantFavoredClassRanks(ranger, pair.Full, 3);
            row["acDeltaAtThree"] = first.Stats.AC.ModifiedValue - ac0;
            if ((int)row["acDeltaAtThree"] != 3)
                failures.Add("three earned steps did not refresh to +3");
            // Replacement: the old companion keeps nothing, the new one gains it once.
            ranger.Descriptor.RemoveFact(leopard);
            ranger.Descriptor.AddFact(wolf);
            Game.Instance.EntityCreator.Tick();
            UnitEntityData second = ranger.Descriptor.Pet;
            if (second == null || ReferenceEquals(second, first))
            {
                failures.Add("native AddPet did not spawn a replacement companion");
                return row;
            }
            pets.Add(second);
            BlueprintFeature petFeature = leaves.PetFeature(pair.Effect.Id);
            row["oldCompanionAcDelta"] = first.Stats.AC.ModifiedValue - ac0;
            row["oldCompanionHasPetFeature"] = first.Descriptor.HasFact(petFeature);
            row["newCompanionFavoredArmor"] = SumNaturalArmorFrom(second, petFeature);
            row["newCompanionPetFeatureCopies"] = second.Descriptor.Progression.Features.Enumerable
                .Count(fact => ReferenceEquals(fact.Blueprint, petFeature));
            if ((int)row["oldCompanionAcDelta"] != 0 || (bool)row["oldCompanionHasPetFeature"])
                failures.Add("the replaced companion kept an orphaned bonus");
            if ((int)row["newCompanionFavoredArmor"] != 3 || (int)row["newCompanionPetFeatureCopies"] != 1)
                failures.Add("the replacement companion did not gain the bonus exactly once");
            // Removing the investment (a respec) removes the projected bonus exactly.
            int withInvestment = second.Stats.AC.ModifiedValue;
            // RemoveFact removes one rank of a ranked feature; remove them all.
            for (int guard = 0; guard < 8 && ranger.Descriptor.HasFact(pair.Full); guard++)
                ranger.Descriptor.RemoveFact(pair.Full);
            row["investmentRemoved"] = !ranger.Descriptor.HasFact(pair.Full);
            row["removedInvestmentAcDelta"] = second.Stats.AC.ModifiedValue - withInvestment;
            row["removedInvestmentPetFeature"] = second.Descriptor.HasFact(petFeature);
            if ((int)row["removedInvestmentAcDelta"] != -3 || (bool)row["removedInvestmentPetFeature"])
                failures.Add("removing the investment did not remove exactly the projected +3");
            return row;
        }

        private static int SumNaturalArmorFrom(UnitEntityData unit, BlueprintFeature petFeature)
        {
            var fact = unit.Descriptor.Progression.Features.GetFact(petFeature);
            int value = 0;
            if (fact != null)
                fact.CallComponents<FavoredClassPetNaturalArmor>(component => value += component.AppliedValue);
            return value;
        }

        private static JObject ObserveEidolon(ElementalUndineFeatScenario.PortalHarness scene,
            UnitEntityData anchor, BlueprintRace race, FavoredClassBlueprintSet leaves,
            List<UnitEntityData> pets, IList<string> failures)
        {
            var row = new JObject();
            var library = BlueprintBootstrap.Library;
            BlueprintFeature progression = library.GetAllBlueprints().OfType<BlueprintFeature>()
                .Where(value => value != null && value.name == FcbAngelEidolonProgressionName)
                .OrderBy(value => value.AssetGuid, StringComparer.Ordinal).FirstOrDefault();
            row["eidolonProgression"] = progression == null ? null : progression.AssetGuid;
            if (progression == null)
            {
                failures.Add("Call of the Wild's eidolon progression is absent");
                return row;
            }
            FavoredClassLeafPair pair = leaves.Pair(FavoredClassCatalog.EffectEidolonArmor, null);
            UnitEntityData summoner = scene.SpawnFixtureUnit(race, anchor.Blueprint.Faction,
                new Vector3(-2, 0, 0), "FavoredClassSummoner");
            summoner.Descriptor.AddFact(progression);
            Game.Instance.EntityCreator.Tick();
            UnitEntityData eidolon = summoner.Descriptor.Pet;
            if (eidolon == null)
            {
                failures.Add("the eidolon progression spawned no pet in the fixture scene");
                return row;
            }
            pets.Add(eidolon);
            row["petQualified"] = FavoredClassPets.IsQualified(eidolon, FavoredClassBlueprints.EidolonClassGuid);
            int ac0 = eidolon.Stats.AC.ModifiedValue, touch0 = eidolon.Stats.AC.Touch;
            int summonerAc0 = summoner.Stats.AC.ModifiedValue;
            GrantFavoredClassRanks(summoner, pair.Full, 2);
            row["acDelta"] = eidolon.Stats.AC.ModifiedValue - ac0;
            row["touchDelta"] = eidolon.Stats.AC.Touch - touch0;
            row["summonerAcDelta"] = summoner.Stats.AC.ModifiedValue - summonerAc0;
            if (!(bool)row["petQualified"])
                failures.Add("the eidolon is not recognized as an eidolon");
            if ((int)row["acDelta"] != 2 || (int)row["touchDelta"] != 0 || (int)row["summonerAcDelta"] != 0)
                failures.Add("two earned steps did not add exactly +2 natural armor to the eidolon only");
            return row;
        }
    }
}
