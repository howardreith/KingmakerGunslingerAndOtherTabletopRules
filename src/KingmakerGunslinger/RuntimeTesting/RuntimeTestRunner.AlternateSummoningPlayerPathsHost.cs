using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.AreaLogic.SummonPool;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Identities from the installed Call of the Wild 1.14.4c-2.1
        // blueprints.txt. The expectations in this file follow its Master
        // Summoner, Occultist and Preservationist construction, not this
        // module's code.
        private const string PlayerPathLesserEidolonSelection = "0e9a8293fb1341fd9aefcca0eb1b3f9b";
        private const string PlayerPathEidolonSummonAbility = "3b265d56fd0544488e44cea94ff01773";
        private const string PlayerPathEidolonUnsummonAbility = "422bfe1c020c455bb8b58151271b4a88";
        private const string PlayerPathEidolonUnsummonedBuff = "ef79cb8a0a5f4d2aa9e7fc679c50c0b1";
        private const string PlayerPathOccultistTierThreeRoot = "e63a46a9c0fc4abc880b0bae72fea726";
        private const string PlayerPathPreservationistSnaFourRoot = "975de352739241388a00d07d830d6678";

        /// <summary>How every cast in this fixture executes. Stated in the
        /// evidence of each assertion that rests on a cast.</summary>
        internal const string PlayerPathAssistedExecution =
            "ASSISTED native command fixture: native UnitUseAbility from the granted ability's own data under a request-local cutscene command context, cooldown ignored, Animation.IsActed set, command and execution process ticked synchronously and the animation finished. Not natural animation, action-bar interaction, scheduler or turn timing, or a saved hotbar.";

        /// <summary>Assertions about grants and exposure; every other
        /// player-path assertion rests on an assisted cast.</summary>
        private static bool PlayerPathCastFree(string name)
        {
            return name.EndsWith("-granted-roots", StringComparison.Ordinal) ||
                name == "player-ms-eidolon-granted" || name == "player-ms-preserved-in-menu" ||
                name == "player-paths-cleanup" ||
                name.StartsWith("player-section-", StringComparison.Ordinal);
        }

        private static void PlayerPathLabelAssisted(List<RuntimeTestAssertion> assertions)
        {
            foreach (RuntimeTestAssertion assertion in assertions)
                if (assertion.Name.StartsWith("player-", StringComparison.Ordinal) &&
                    !PlayerPathCastFree(assertion.Name))
                    assertion.Evidence = assertion.Evidence + " | " + PlayerPathAssistedExecution;
        }

        private static ISummonPool PlayerPathPool(string guid)
        {
            var pool = (BlueprintSummonPool)BlueprintBootstrap.Library.BlueprintsByAssetId[guid];
            return Game.Instance.SummonPools.GetPool(pool);
        }

        /// <summary>The engine's raw member count (the value the host's
        /// caster check reads) and the live members' identities.</summary>
        private static string PlayerPathPoolState(string guid)
        {
            ISummonPool pool = PlayerPathPool(guid);
            return pool.Count + "[" + string.Join(",", PlayerPathPoolUnits(guid)) + "]";
        }

        private static string[] PlayerPathPoolUnits(string guid)
        {
            return PlayerPathPool(guid).Units.Where(value => value != null && !value.Destroyed)
                .Select(value => value.UniqueId).OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
        }

        private static bool PlayerPathSameUnits(string guid, IEnumerable<UnitEntityData> units)
        {
            return PlayerPathPool(guid).Count == units.Count() && PlayerPathPoolUnits(guid)
                .SequenceEqual(units.Select(value => value.UniqueId).OrderBy(value => value,
                    StringComparer.Ordinal));
        }

        private static bool PlayerPathSummoned(UnitEntityData unit)
        {
            return unit != null && !unit.Destroyed && !unit.Descriptor.State.IsDead &&
                unit.Descriptor.HasFact(BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff);
        }

        /// <summary>The engine's own life tick for one unit, request-locally:
        /// a pet marked for death becomes dead exactly as the native
        /// controller would make it on its next game tick.</summary>
        private static void PlayerPathLifeTick(UnitEntityData unit)
        {
            MethodInfo tick = typeof(UnitLifeController).GetMethod("TickOnUnit",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (tick == null) throw new MissingMethodException("UnitLifeController.TickOnUnit");
            tick.Invoke(new UnitLifeController(), new object[] { unit });
            Game.Instance.EntityDestroyer.Tick();
        }

        /// <summary>
        /// Ends one summon the way its duration or a pool clear does: the
        /// summoned-unit buff is removed (ContextActionClearSummonPool and
        /// expiry both do exactly this). The engine then keeps the unit, and
        /// its pool membership, through a timed dismemberment it cannot
        /// advance synchronously; the fixture completes that delay by
        /// destroying the unit, and the pool releases it on the engine's own
        /// destroy event.
        /// </summary>
        private void PlayerPathEndSummon(UnitEntityData unit)
        {
            if (unit == null || unit.Destroyed) return;
            if (unit.Descriptor.HasFact(BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff))
                unit.Descriptor.RemoveFact(BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff);
            PlayerPathCompleteDismissal(unit);
        }

        private void PlayerPathCompleteDismissal(UnitEntityData unit)
        {
            CleanupExpandedSummoningUnit(unit);
            _playerPathKept.Remove(unit);
            Game.Instance.EntityDestroyer.Tick();
        }

        /// <summary>Runs one plain granted ability (no variants) on its
        /// caster as an assisted native command.</summary>
        private static string PlayerPathRunOwnAbility(UnitEntityData caster, Ability fact)
        {
            var data = new AbilityData(fact);
            UnitUseAbility command;
            var parameters = new Kingmaker.AreaLogic.Cutscenes.CutsceneParametersContext();
            using (parameters.Data) command = new UnitUseAbility(data, new TargetWrapper(caster));
            if (!data.IsAvailable || !command.CanStart)
                return "unavailable:" + data.GetUnavailableReason();
            command.IgnoreCooldown(TimeSpan.Zero);
            caster.Commands.Run(command);
            command.Start();
            if (!command.IsRunning) return "not-running";
            if (command.Animation != null) command.Animation.IsActed = true;
            command.Tick();
            if (command.ExecutionProcess != null)
                for (int tick = 0; tick < 5000 && !command.ExecutionProcess.IsEnded; tick++)
                    command.ExecutionProcess.Tick();
            if (command.Animation != null) FinishExpandedSummoningAnimation(command.Animation);
            if (!command.IsFinished) command.Tick();
            Game.Instance.EntityCreator.Tick();
            return command.Result.ToString();
        }

        /// <summary>
        /// Master Summoner with a real lesser eidolon taken through the native
        /// level-1 selection. Expected behavior is the installed CotW
        /// construction: a cast is allowed while the eidolon is dead or the
        /// free pool is empty (AbilityCasterCompanionDeadOrSummonPoolEmpty);
        /// it spawns into the free pool unless that already holds units, then
        /// into the summoner pool; returning the eidolon removes
        /// EidolonUnsummonedBuff, which resurrects it and clears the summoner
        /// pool.
        /// </summary>
        private void PlayerPathMasterSummonerEidolon(ExpandedSummoningCorrectionFixture fixture,
            List<RuntimeTestAssertion> assertions, JArray casts, JObject document)
        {
            LibraryScriptableObject library = BlueprintBootstrap.Library;
            var record = new JObject();
            document["masterSummonerEidolon"] = record;
            UnitEntityData caster = PlayerPathSecondCaster(fixture,
                "KMG_Runtime_AlternateSummoning_MasterSummonerEidolon");
            caster.Descriptor.Stats.Charisma.BaseValue = 18;
            var summoner = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(library,
                AlternateSummoningContracts.SummonerClass, "CotW Summoner class");
            var archetype = BlueprintLibraryLookup.RequireExact<BlueprintArchetype>(library,
                PlayerPathMasterSummonerArchetype, "CotW Master Summoner archetype");
            string chosen = null;
            AdvanceDisposableArchetypeCaster(caster.Descriptor, summoner, archetype, 5,
                ref _playerPathLevelController, controller =>
                {
                    FeatureSelectionState state = FavoredClassLevelUpHarness.FindOpenState(
                        controller, PlayerPathLesserEidolonSelection);
                    if (state == null) return;
                    IFeatureSelectionItem item = FavoredClassLevelUpHarness.Items(controller, state)
                        .OrderBy(value => value.Feature.AssetGuid, StringComparer.Ordinal)
                        .FirstOrDefault(value => state.Selection.CanSelect(controller.Preview,
                            controller.State, state, value));
                    if (item != null && controller.SelectFeature(state, item))
                        chosen = item.Feature.AssetGuid + ":" + item.Feature.name;
                });
            Game.Instance.EntityCreator.Tick();
            UnitEntityData pet = caster.Descriptor.Pet;
            if (pet != null) fixture.Created.Add(pet);
            Ability summon = caster.Descriptor.Abilities.RawFacts.OfType<Ability>().FirstOrDefault(
                value => value.Blueprint.AssetGuid == PlayerPathEidolonSummonAbility);
            Ability unsummon = caster.Descriptor.Abilities.RawFacts.OfType<Ability>().FirstOrDefault(
                value => value.Blueprint.AssetGuid == PlayerPathEidolonUnsummonAbility);
            record["eidolonFeature"] = chosen;
            record["pet"] = pet == null ? null : pet.UniqueId + ":" + pet.Blueprint.name;
            record["summonAbility"] = summon != null;
            record["unsummonAbility"] = unsummon != null;
            bool ready = chosen != null && pet != null && pet.IsInState &&
                !pet.Descriptor.State.IsDead && summon != null && unsummon != null;
            assertions.Add(Assertion("player-ms-eidolon-granted",
                "a level-5 Master Summoner takes a real lesser eidolon through the native level-1 selection; the pet is live in the area and the summoner holds Summon and Unsummon Eidolon",
                record.ToString(Formatting.None), ready,
                "LevelUpController selection of LesserEidolonFeatureSelection; AddPet; UnitDescriptor.Pet"));
            if (!ready) return;

            var resource = BlueprintLibraryLookup.RequireExact<BlueprintAbilityResource>(library,
                AlternateSummoningContracts.MasterSummonerResource, "Master Summoner resource");
            caster.Descriptor.Resources.Restore(resource);
            AlternateSummoningFamilySpec family = AlternateSummoningSourceCatalog.Find(
                AlternateSummoningSourceCatalog.MasterSummonerKey);
            Ability tierOne = PlayerPathRootFact(caster, family, 1);
            Ability tierThree = PlayerPathRootFact(caster, family, 3);
            BlueprintAbility single = PlayerPathChild(tierOne.Blueprint, true, "1");
            BlueprintAbility group = PlayerPathChild(tierThree.Blueprint, true, "d3");
            string free = AlternateSummoningContracts.MasterSummonerFreePool;
            string bound = AlternateSummoningContracts.SummonerPool;
            Func<string, string> pools = label => label + ":free=" + PlayerPathPoolState(free) +
                ";bound=" + PlayerPathPoolState(bound);
            record["poolsBefore"] = pools("before");
            if (PlayerPathPool(free).Count != 0 || PlayerPathPool(bound).Count != 0)
                throw new InvalidOperationException("The Master Summoner pools are not empty: " +
                    record["poolsBefore"]);
            object[] baseline = SnapshotReferences(fixture.SceneEntities);

            // 1. Eidolon present: one effect, in the free pool; a second is refused.
            AlternateSummoningCastRecord first = PlayerPathCast(fixture, caster, tierOne, single,
                resource, "ms-eidolon-first-effect", true, keep: true);
            casts.Add(first.ToJson());
            UnitEntityData[] firstUnits = _playerPathKept.ToArray();
            string afterFirst = pools("after-first");
            bool firstInFree = firstUnits.Length == 1 && PlayerPathSameUnits(free, firstUnits);
            AlternateSummoningCastRecord second = PlayerPathCast(fixture, caster, tierOne, single,
                resource, "ms-eidolon-second-refused", false);
            casts.Add(second.ToJson());
            assertions.Add(Assertion("player-ms-eidolon-one-effect",
                "with the eidolon present the first effect spends one use and lands in the free pool; a second attempt is refused by the host's caster check with nothing spent or summoned",
                afterFirst + ";first=" + first.ToJson().ToString(Formatting.None) + ";second=" +
                    second.ToJson().ToString(Formatting.None),
                first.Started && first.Result == "Success" &&
                    first.ResourceBefore - first.ResourceAfter == 1 && firstInFree &&
                    !second.Available && !second.Started &&
                    second.ResourceBefore == second.ResourceAfter && second.Summoned == 0,
                "AbilityCasterCompanionDeadOrSummonPoolEmpty; HasUnitsInSummonPool; SummonPoolsManager"));

            // 2. A group option is one effect, not one creature.
            foreach (UnitEntityData unit in firstUnits) PlayerPathEndSummon(unit);
            string afterEnd = pools("after-first-ends");
            bool freeEmptied = PlayerPathPool(free).Count == 0;
            AlternateSummoningCastRecord groupCast = PlayerPathCast(fixture, caster, tierThree,
                group, resource, "ms-eidolon-group-effect", true, keep: true);
            casts.Add(groupCast.ToJson());
            UnitEntityData[] groupUnits = _playerPathKept.ToArray();
            string afterGroup = pools("after-group");
            bool groupInFree = groupUnits.Length >= 1 && groupUnits.Length <= 3 &&
                PlayerPathSameUnits(free, groupUnits);
            AlternateSummoningCastRecord afterGroupAttempt = PlayerPathCast(fixture, caster,
                tierOne, single, resource, "ms-eidolon-after-group-refused", false);
            casts.Add(afterGroupAttempt.ToJson());
            assertions.Add(Assertion("player-ms-eidolon-group-is-one-effect",
                "once the first effect ends the free pool is empty; a 1d3 group then spends one use and all of its creatures share the free pool, and the next attempt is refused with nothing spent",
                afterEnd + ";" + afterGroup + ";group=" +
                    groupCast.ToJson().ToString(Formatting.None) + ";next=" +
                    afterGroupAttempt.ToJson().ToString(Formatting.None),
                freeEmptied && groupCast.Started && groupCast.Result == "Success" &&
                    groupCast.ResourceBefore - groupCast.ResourceAfter == 1 && groupInFree &&
                    !afterGroupAttempt.Available &&
                    afterGroupAttempt.ResourceBefore == afterGroupAttempt.ResourceAfter &&
                    afterGroupAttempt.Summoned == 0,
                "ContextActionSpawnMonster dice count; SummonPoolsManager membership"));

            // 3. Unsummon the eidolon: further effects coexist in the summoner pool.
            var unsummonedBuff = (BlueprintBuff)library.BlueprintsByAssetId[PlayerPathEidolonUnsummonedBuff];
            string unsummoned = PlayerPathRunOwnAbility(caster, unsummon);
            bool marked = pet.Descriptor.State.MarkedForDeath;
            PlayerPathLifeTick(pet);
            bool petAbsent = pet.Descriptor.State.IsDead && caster.Descriptor.HasFact(unsummonedBuff);
            AlternateSummoningCastRecord boundOne = PlayerPathCast(fixture, caster, tierOne, single,
                resource, "ms-eidolon-absent-bound-one", true, keep: true);
            AlternateSummoningCastRecord boundTwo = PlayerPathCast(fixture, caster, tierOne, single,
                resource, "ms-eidolon-absent-bound-two", true, keep: true);
            casts.Add(boundOne.ToJson());
            casts.Add(boundTwo.ToJson());
            UnitEntityData[] boundUnits = _playerPathKept.Except(groupUnits).ToArray();
            string afterBound = pools("after-unsummon");
            bool coexist = petAbsent && boundUnits.Length == 2 &&
                PlayerPathSameUnits(bound, boundUnits) && PlayerPathSameUnits(free, groupUnits) &&
                groupUnits.All(PlayerPathSummoned) && boundUnits.All(PlayerPathSummoned) &&
                boundOne.ResourceBefore - boundOne.ResourceAfter == 1 &&
                boundTwo.ResourceBefore - boundTwo.ResourceAfter == 1;
            assertions.Add(Assertion("player-ms-eidolon-absent-coexist",
                "after the real Unsummon Eidolon (and the engine's own life tick on the eidolon) the eidolon is absent; two more effects are allowed, spend one use each and land in the summoner pool beside the free-pool group",
                "unsummon=" + unsummoned + ";markedForDeath=" + marked + ";petDead=" +
                    pet.Descriptor.State.IsDead + ";" + afterBound + ";one=" +
                    boundOne.ToJson().ToString(Formatting.None) + ";two=" +
                    boundTwo.ToJson().ToString(Formatting.None),
                unsummoned == "Success" && coexist,
                "EidolonUnsummonAbility; UnitLifeController.TickOnUnit (request-local); SummonPoolsManager"));

            // 4. Return the eidolon: the summoner pool is dismissed, the free pool kept.
            // Resurrect lowers the eidolon's damage and clears its death
            // flags; the life controller's next tick makes it conscious.
            string returned = PlayerPathRunOwnAbility(caster, summon);
            bool buffRemoved = !caster.Descriptor.HasFact(unsummonedBuff);
            bool deadBeforeTick = pet.Descriptor.State.IsDead;
            PlayerPathLifeTick(pet);
            bool petBack = buffRemoved && !pet.Descriptor.State.IsDead;
            bool boundDismissed = boundUnits.All(value => !PlayerPathSummoned(value));
            bool freeKept = groupUnits.All(PlayerPathSummoned);
            string afterReturn = pools("after-return-before-dismemberment");
            foreach (UnitEntityData unit in boundUnits) PlayerPathCompleteDismissal(unit);
            bool boundReleased = PlayerPathPool(bound).Count == 0 &&
                PlayerPathSameUnits(free, groupUnits);
            AlternateSummoningCastRecord blocked = PlayerPathCast(fixture, caster, tierOne, single,
                resource, "ms-eidolon-returned-refused", false);
            casts.Add(blocked.ToJson());
            assertions.Add(Assertion("player-ms-eidolon-return",
                "the real Summon Eidolon returns the eidolon and dismisses every summoner-pool effect while the free-pool group stays; the pool releases them when they are gone; with the eidolon back and the free pool occupied a new attempt is refused with nothing spent",
                "summon=" + returned + ";unsummonedBuffRemoved=" + buffRemoved +
                    ";deadBeforeLifeTick=" + deadBeforeTick + ";petBack=" + petBack +
                    ";boundDismissed=" + boundDismissed +
                    ";freeKept=" + freeKept + ";" + afterReturn + ";" + pools("after-dismemberment") +
                    ";next=" + blocked.ToJson().ToString(Formatting.None),
                returned == "Success" && petBack && boundDismissed && freeKept && boundReleased &&
                    !blocked.Available && blocked.ResourceBefore == blocked.ResourceAfter &&
                    blocked.Summoned == 0,
                "EidolonSummonAbility; EidolonUnsummonedBuff removal (Resurrect, ClearSummonPool); UnitLifeController.TickOnUnit (request-local); SummonPoolsManager destroy event; dismemberment delay completed by the fixture"));

            // 5. The remaining effects belong to this summoner and end cleanly;
            //    one effect is then permitted again.
            var ownership = new JArray();
            bool owned = true;
            foreach (UnitEntityData unit in groupUnits)
            {
                Buff lifecycle = unit.Descriptor.Buffs.RawFacts.OfType<Buff>().FirstOrDefault(
                    value => ReferenceEquals(value.Blueprint,
                        BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff));
                bool mine = lifecycle != null && lifecycle.MaybeContext != null &&
                    ReferenceEquals(lifecycle.MaybeContext.MaybeCaster, caster);
                owned &= mine;
                ownership.Add(unit.UniqueId + ":" + (mine ? "owned" : "foreign"));
                PlayerPathEndSummon(unit);
            }
            bool allGone = groupUnits.Concat(boundUnits).All(value => value.Destroyed) &&
                PlayerPathPool(free).Count == 0 && PlayerPathPool(bound).Count == 0;
            AlternateSummoningCastRecord again = PlayerPathCast(fixture, caster, tierOne, single,
                resource, "ms-eidolon-after-cleanup", true, keep: true);
            casts.Add(again.ToJson());
            bool againFree = _playerPathKept.Count == 1 &&
                PlayerPathSameUnits(free, _playerPathKept);
            assertions.Add(Assertion("player-ms-eidolon-cleanup",
                "every remaining effect is owned by this summoner and leaves both pools when it ends; one effect is then permitted again in the free pool",
                "ownership=" + ownership.ToString(Formatting.None) + ";allGone=" + allGone +
                    ";again=" + again.ToJson().ToString(Formatting.None) + ";" + pools("final"),
                owned && allGone && again.Started && again.Result == "Success" &&
                    again.ResourceBefore - again.ResourceAfter == 1 && againFree,
                "SummonedUnitBuff context caster; SummonPoolsManager"));
            PlayerPathDisposeKept(fixture);
            DrainExpandedSummoningDestroyQueue(fixture.SceneEntities, baseline);
        }

        /// <summary>
        /// Occultist: a lower-tier creature group chosen through the tier III
        /// Conjurer's Focus costs the tier III price. CotW sets the cost of
        /// OccultistSummon{n}Base to n reservoir points whatever the entry, so
        /// the expected spend is that constant, not the child's own
        /// calculated cost.
        /// </summary>
        private void PlayerPathOccultistLowerTierGroup(ExpandedSummoningCorrectionFixture fixture,
            List<RuntimeTestAssertion> assertions, JArray casts, JObject document)
        {
            LibraryScriptableObject library = BlueprintBootstrap.Library;
            UnitEntityData caster = PlayerPathSecondCaster(fixture,
                "KMG_Runtime_AlternateSummoning_OccultistGroup");
            caster.Descriptor.Stats.Intelligence.BaseValue = 18;
            AdvanceDisposableArchetypeCaster(caster.Descriptor,
                BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(library,
                    AlternateSummoningContracts.ArcanistClass, "Arcanist class"),
                BlueprintLibraryLookup.RequireExact<BlueprintArchetype>(library,
                    PlayerPathOccultistArchetype, "CotW Occultist archetype"), 5,
                ref _playerPathLevelController);
            var reservoir = BlueprintLibraryLookup.RequireExact<BlueprintAbilityResource>(library,
                AlternateSummoningContracts.ArcaneReservoirResource, "Arcane reservoir");
            PlayerPathFillResource(caster, reservoir, 10);
            Ability root = PlayerPathRootFact(caster, AlternateSummoningSourceCatalog.Find(
                AlternateSummoningSourceCatalog.OccultistKey), 3);
            BlueprintAbility child = PlayerPathChild(root.Blueprint, true, "d3");
            AlternateSummoningChoice choice = PlayerPathChoice(
                AlternateSummoningSourceCatalog.OccultistKey, child);
            AlternateSummoningCastRecord cast = PlayerPathCast(fixture, caster, root, child,
                reservoir, "occultist-lower-tier-group", true);
            casts.Add(cast.ToJson());
            const int SelectedTierCost = 3;
            var record = new JObject {
                ["root"] = root.Blueprint.AssetGuid,
                ["menuTier"] = choice.Placement.ParentTier,
                ["creatureSourceTier"] = choice.Placement.SourceTier,
                ["creatureMonsterTier"] = choice.Placement.Creature.MonsterTier,
                ["implementationCalculatedCost"] = cast.ExpectedCost,
                ["contractCost"] = SelectedTierCost
            };
            document["occultistLowerTierGroup"] = record;
            assertions.Add(Assertion("player-occultist-lower-tier-group",
                "a 1d3 group of a lower-tier creature chosen through the tier III Conjurer's Focus spends 3 reservoir points (the selected tier), summons one to three of that creature and lasts one minute per Arcanist level",
                record.ToString(Formatting.None) + ";" + cast.ToJson().ToString(Formatting.None),
                root.Blueprint.AssetGuid == PlayerPathOccultistTierThreeRoot &&
                    choice.Placement.ParentTier == 3 && choice.Placement.SourceTier < 3 &&
                    cast.Started && cast.Result == "Success" &&
                    cast.ResourceBefore - cast.ResourceAfter == SelectedTierCost &&
                    cast.Summoned >= 1 && cast.Summoned <= 3 && cast.Live == cast.Summoned &&
                    cast.Durations.Split(',').All(value => value == "300"),
                "CotW OccultistSummon3Base cost 3; roster source tier of the chosen creature; RuleSummonUnit duration"));
        }

        /// <summary>
        /// Preservationist at Alchemist 8: CotW Bottled Ally III adds Summon
        /// Nature's Ally IV to the formula book as a 3rd-level extract. One
        /// prepared 3rd-level slot opens a tier IV creature; the copied spell
        /// lasts one round per caster level.
        /// </summary>
        private void PlayerPathPreservationistExtractMapping(
            ExpandedSummoningCorrectionFixture fixture, List<RuntimeTestAssertion> assertions,
            JArray casts, JObject document)
        {
            var record = new JObject { ["case"] = "preservationist-extract-3-sna-4" };
            document["preservationistExtractMapping"] = record;
            LibraryScriptableObject library = BlueprintBootstrap.Library;
            UnitEntityData caster = PlayerPathSecondCaster(fixture,
                "KMG_Runtime_AlternateSummoning_PreservationistEight");
            caster.Descriptor.Stats.Intelligence.BaseValue = 18;
            AdvanceDisposableArchetypeCaster(caster.Descriptor,
                BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(library,
                    PlayerPathAlchemistClass, "Alchemist class"),
                BlueprintLibraryLookup.RequireExact<BlueprintArchetype>(library,
                    PlayerPathPreservationistArchetype, "Preservationist archetype"), 8,
                ref _playerPathLevelController);
            var root = (BlueprintAbility)library.BlueprintsByAssetId[PlayerPathPreservationistSnaFourRoot];
            Spellbook book = caster.Descriptor.Spellbooks.FirstOrDefault(value => value.IsKnown(root));
            int knownLevel = book == null ? -1 : book.GetSpellLevel(root);
            record["knownLevel"] = knownLevel;
            record["casterLevel"] = book == null ? -1 : book.CasterLevel;
            if (knownLevel != 3)
                throw new InvalidOperationException("Summon Nature's Ally IV is not a known 3rd-level extract: " +
                    knownLevel + ".");
            BlueprintAbility child = PlayerPathChild(root, true, "1");
            AlternateSummoningChoice choice = PlayerPathChoice(
                AlternateSummoningSourceCatalog.PreservationistKey, child);
            AbilityData data = PrepareExpandedSummoningPlayerPathSpell(book, root, child, 3);
            int three = CountAvailableExpandedSummoningSlots(book, 3);
            int four = CountAvailableExpandedSummoningSlots(book, 4);
            object[] before = SnapshotReferences(fixture.SceneEntities);
            ExpandedSummoningRuleCapture.Clear();
            ExpandedSummoningRuleDurationCapture.Clear();
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
            string durations = string.Join(",", ExpandedSummoningRuleDurationCapture.Select(value =>
                ((int)Math.Round(value.TotalDuration.TotalSeconds)).ToString()).ToArray());
            SummonSpawn spawn = SummonSignatureReader.Read(child).Spawns().First();
            int threeAfter = CountAvailableExpandedSummoningSlots(book, 3);
            int fourAfter = CountAvailableExpandedSummoningSlots(book, 4);
            record["menuTier"] = choice.Placement.ParentTier;
            record["creatureSourceTier"] = choice.Placement.SourceTier;
            record["slots3"] = three + "->" + threeAfter;
            record["slots4"] = four + "->" + fourAfter;
            record["convertedFromRoot"] = data.ConvertedFrom != null &&
                ReferenceEquals(data.ConvertedFrom.Blueprint, root);
            record["available"] = available;
            record["result"] = command.Result.ToString();
            record["summoned"] = string.Join(",", summoned.Select(value =>
                value.Blueprint.AssetGuid + ":" + value.Blueprint.name).ToArray());
            record["childSpawnUnit"] = spawn.Unit;
            record["durationsSeconds"] = durations;
            record["execution"] = "assisted-command-fixture";
            bool exact = available && command.Result.ToString() == "Success" &&
                record.Value<bool>("convertedFromRoot") && choice.Placement.ParentTier == 4 &&
                choice.Placement.SourceTier == 4 && threeAfter == three - 1 && fourAfter == four &&
                summoned.Length == 1 && summoned[0].Blueprint.AssetGuid == spawn.Unit &&
                book.CasterLevel == 8 && durations == (book.CasterLevel * 6).ToString();
            foreach (UnitEntityData unit in summoned) CleanupExpandedSummoningUnit(unit);
            DrainExpandedSummoningDestroyQueue(fixture.SceneEntities, before);
            ExpandedSummoningRuleCapture.Clear();
            casts.Add(record);
            assertions.Add(Assertion("player-preservationist-extract-3-sna-4",
                "at Alchemist 8 Summon Nature's Ally IV is a known 3rd-level extract; one prepared 3rd-level slot (and no 4th-level slot) opens a tier IV roster creature through the root's variant data, for one round per caster level (8 rounds)",
                record.ToString(Formatting.None), exact,
                "CotW Bottled Ally III AddKnownSpell level 3; Spellbook slot; RuleSummonUnit duration"));
        }

        private static AlternateSummoningChoice PlayerPathChoice(string familyKey,
            BlueprintAbility child)
        {
            AlternateSummoningFamilyIdentity identity = AlternateSummoningSourceCatalog.Identities
                .Single(value => value.Key == familyKey);
            AlternateSummoningChoice[] matches = AlternateSummoningProjection.Identities(identity,
                    AlternateSummoningRoster.Production())
                .Where(value => value.Kind == AlternateSummoningChoiceKind.Creature &&
                    AlternateSummoningAdapterBuilder.InternalName(value.Symbol) == child.name)
                .ToArray();
            if (matches.Length != 1)
                throw new InvalidOperationException("No single roster choice for " + child.name +
                    ": " + matches.Length + ".");
            return matches[0];
        }
    }
}
