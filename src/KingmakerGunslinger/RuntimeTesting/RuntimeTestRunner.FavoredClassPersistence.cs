using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.LevelUp;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.FavoredClass;
using KingmakerGunslinger.FavoredClass.Mechanics;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    // L01 Gunslinger subject of the guarded Favored Class persistence
    // transaction. The prepare phase adds a request-local Half-elf Pistolero
    // to the same disposable save through five native level-ups: grit is
    // completed once (N=4, ranks 1/3) and firearm confirmation is left
    // mid-fraction (N=1, rank 0/1), and one grit point is spent. The fresh
    // verify process proves the exact leaf identities and ranks, the
    // maximum and the spent current grit (no refill on load) and the earned
    // steps.
    internal sealed partial class RuntimeTestRunner
    {
        private const string FcbHalfElfRace = "b3646842ffbd01643ab4dac7479b20b0";
        private const string FcbPersistenceGunslingerName = "KMG FCB Persistence Gunslinger";

        private UnitEntityData PrepareGunslingerFcbPersistence(UnitEntityData anchor, Player player,
            out JObject expected)
        {
            FavoredClassBlueprintSet leaves = BlueprintBootstrap.FavoredClassLeaves;
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            FavoredClassHostHandles host = FavoredClassIntegrationCoordinator.Host;
            if (leaves == null || gunslinger == null || gunslinger.Pistolero == null || host == null ||
                host.GunslingerSelection == null ||
                FavoredClassIntegrationStatusRegistry.Current.Availability !=
                    FavoredClassIntegrationAvailability.Published)
                throw new InvalidOperationException(
                    "The Gunslinger favored-class persistence subject requires the published integration.");
            BlueprintRace race = BlueprintLibraryLookup.RequireExact<BlueprintRace>(
                BlueprintBootstrap.Library, FcbHalfElfRace, "native Half-elf");
            FavoredClassLeafPair grit = leaves.Pair(FavoredClassCatalog.EffectGrit, null);
            FavoredClassLeafPair confirmation = leaves.Pair(FavoredClassCatalog.EffectFirearmConfirmation, null);
            BlueprintFeature[] picks = { grit.Partial, grit.Partial, grit.Partial, grit.Full,
                confirmation.Partial };

            Game game = Game.Instance;
            var dollState = new DollState();
            dollState.SetGender(anchor.Descriptor.Gender);
            dollState.SetRace(race);
            dollState.SetClass(gunslinger.CharacterClass);
            var doll = dollState.CreateData();
            var view = doll.CreateUnitView(false);
            if (view == null)
                throw new InvalidOperationException("The Gunslinger persistence fixture has no real view.");
            view.Blueprint = game.BlueprintRoot.DefaultPlayerCharacter;
            view.UniqueId = Guid.NewGuid().ToString();
            view.transform.position = anchor.Position;
            var pending = (System.Collections.IList)game.EntityCreator.GetType()
                .GetField("m_ToCreate", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(game.EntityCreator);
            if (pending.Count != 0)
                throw new InvalidOperationException("Unrelated native entity creation is pending.");
            var unit = game.EntityCreator.SpawnEntityWithView(view, player.CrossSceneState) as UnitEntityData;
            if (unit == null)
                throw new InvalidOperationException("Gunslinger persistence entity ownership transfer failed.");
            game.EntityCreator.Tick();
            unit.Descriptor.Doll = doll;
            unit.Descriptor.CustomGender = anchor.Descriptor.Gender;
            unit.Descriptor.CustomName = FcbPersistenceGunslingerName;
            unit.Stats.Wisdom.BaseValue = 14;
            unit.Descriptor.TurnOn();
            var reserved = new HashSet<string>(StringComparer.Ordinal) { host.GunslingerSelection.AssetGuid };
            for (int level = 1; level <= picks.Length; level++)
            {
                var row = new JObject();
                LevelUpController controller = null;
                try
                {
                    controller = FavoredClassLevelUpHarness.Open(unit.Descriptor, race,
                        gunslinger.CharacterClass, FcbPersistenceGunslingerName, gunslinger.Pistolero.Archetype);
                    if (level == 1 && FavoredClassLevelUpHarness.ChooseFavoredClass(controller,
                            gunslinger.CharacterClass, row) == null)
                        throw new InvalidOperationException("The favored Gunslinger progression is unavailable.");
                    FavoredClassLevelUpHarness.FillOthers(controller, reserved);
                    var state = FavoredClassLevelUpHarness.FindOpenState(controller,
                        host.GunslingerSelection.AssetGuid);
                    if (state == null || !FavoredClassLevelUpHarness.Select(controller, state, picks[level - 1]))
                        throw new InvalidOperationException("Persistence level " + level + " could not take " +
                            picks[level - 1].name);
                    FavoredClassLevelUpHarness.FillOthers(controller, reserved);
                    if (!FavoredClassLevelUpHarness.Confirm(controller, unit.Descriptor, row))
                        throw new InvalidOperationException("Persistence level " + level + " is incomplete: " +
                            row["completion"]);
                }
                finally
                {
                    FavoredClassLevelUpHarness.Close(controller);
                }
            }
            BlueprintAbilityResource resource = gunslinger.Grit.Resource;
            if (unit.Descriptor.Resources.GetResourceAmount(resource) < 1)
                throw new InvalidOperationException("The persistence Gunslinger has no grit to spend.");
            unit.Descriptor.Resources.Spend(resource, 1);
            expected = DescribeGunslingerFcbState(unit, leaves, gunslinger);
            expected["unitId"] = unit.UniqueId;
            expected["unitName"] = FcbPersistenceGunslingerName;
            FcbPersistenceAssert("gunslinger-prepare-committed",
                "five native level-ups bank grit (1 full, 3 partial: +1 maximum grit) and confirmation (0 full, 1 partial), and one grit point is spent before saving",
                (int)expected["classLevel"] == 5 && (int)expected["gritFull"] == 1 &&
                    (int)expected["gritPartial"] == 3 && (int)expected["confirmationFull"] == 0 &&
                    (int)expected["confirmationPartial"] == 1 && (int)expected["gritSteps"] == 1 &&
                    (int)expected["confirmationSteps"] == 0 &&
                    (int)expected["gritCurrent"] == (int)expected["gritMax"] - 1,
                expected);
            player.PartyCharacters.Add(unit);
            player.InvalidateCharacterLists();
            player.UpdateCharacterLists();
            if (!player.Party.Contains(unit))
                throw new InvalidOperationException("The persistence Gunslinger did not enter the traveling party.");
            return unit;
        }

        private UnitEntityData VerifyGunslingerFcbPersistence(JObject expected, Player player)
        {
            FavoredClassBlueprintSet leaves = BlueprintBootstrap.FavoredClassLeaves;
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            string unitId = (string)expected["unitId"];
            UnitEntityData unit = player.Party.SingleOrDefault(value => value.UniqueId == unitId) ??
                Game.Instance.State.Units.SingleOrDefault(value => value.UniqueId == unitId);
            if (unit == null)
                throw new InvalidOperationException("The saved persistence Gunslinger is absent from the reloaded save.");
            JObject observed = DescribeGunslingerFcbState(unit, leaves, gunslinger);
            observed["unitId"] = unit.UniqueId;
            observed["unitName"] = unit.CharacterName;
            FcbPersistenceAssert("gunslinger-reload-ranks",
                "the fresh-process reload restores the exact leaf identities and ranks on both sides of the completed fraction",
                observed["leafRanks"].ToString() == expected["leafRanks"].ToString() &&
                    (int)observed["classLevel"] == (int)expected["classLevel"] &&
                    (string)observed["unitName"] == (string)expected["unitName"],
                new { expected = expected["leafRanks"], observed = observed["leafRanks"],
                    classLevel = observed["classLevel"] });
            FcbPersistenceAssert("gunslinger-reload-resources",
                "the reloaded maximum grit includes the earned step and the spent current grit is not refilled by loading",
                (int)observed["gritMax"] == (int)expected["gritMax"] &&
                    (int)observed["gritCurrent"] == (int)expected["gritCurrent"],
                new { gritMax = observed["gritMax"], gritCurrent = observed["gritCurrent"],
                    expectedMax = expected["gritMax"], expectedCurrent = expected["gritCurrent"] });
            FcbPersistenceAssert("gunslinger-reload-effects",
                "the reloaded earned steps are unchanged: +1 grit and no confirmation bonus before its third investment",
                (int)observed["gritSteps"] == 1 && (int)observed["confirmationSteps"] == 0,
                new { gritSteps = observed["gritSteps"], confirmationSteps = observed["confirmationSteps"] });
            return unit;
        }

        // The prepare process removes the request-owned Gunslinger from the
        // party and its holding state and disposes it, exactly like the Oracle.
        private static void DetachGunslingerFcbPersistence(Player player, ref UnitEntityData unit)
        {
            if (unit == null)
                return;
            string id = unit.UniqueId;
            player.PartyCharacters.RemoveAll(value => value.UniqueId == id);
            if (unit.HoldingState != null && unit.HoldingState.AllEntityData.Contains(unit))
                unit.HoldingState.RemoveEntityData(unit);
            player.InvalidateCharacterLists();
            player.UpdateCharacterLists();
            unit.Dispose();
            unit = null;
        }

        private static JObject DescribeGunslingerFcbState(UnitEntityData unit, FavoredClassBlueprintSet leaves,
            GunslingerClassBlueprintSet gunslinger)
        {
            FavoredClassLeafPair grit = leaves.Pair(FavoredClassCatalog.EffectGrit, null);
            FavoredClassLeafPair confirmation = leaves.Pair(FavoredClassCatalog.EffectFirearmConfirmation, null);
            var ranks = new JObject();
            foreach (Feature feature in unit.Descriptor.Progression.Features.Enumerable
                .Where(value => value.Blueprint != null && leaves.Pairs.Any(pair =>
                    pair.Leaves.Any(leaf => leaf.AssetGuid == value.Blueprint.AssetGuid)))
                .OrderBy(value => value.Blueprint.AssetGuid, StringComparer.Ordinal))
                ranks[feature.Blueprint.AssetGuid] = feature.Rank;
            BlueprintAbilityResource resource = gunslinger.Grit.Resource;
            return new JObject
            {
                ["classLevel"] = unit.Descriptor.Progression.GetClassLevel(gunslinger.CharacterClass),
                ["leafRanks"] = ranks,
                ["gritFull"] = FavoredClassLevelUpHarness.Rank(unit.Descriptor, grit.Full),
                ["gritPartial"] = FavoredClassLevelUpHarness.Rank(unit.Descriptor, grit.Partial),
                ["confirmationFull"] = FavoredClassLevelUpHarness.Rank(unit.Descriptor, confirmation.Full),
                ["confirmationPartial"] = FavoredClassLevelUpHarness.Rank(unit.Descriptor, confirmation.Partial),
                ["gritSteps"] = FavoredClassEarnedSteps.For(unit.Descriptor, FavoredClassCatalog.EffectGrit, null),
                ["confirmationSteps"] = FavoredClassEarnedSteps.For(unit.Descriptor,
                    FavoredClassCatalog.EffectFirearmConfirmation, null),
                ["gritMax"] = resource.GetMaxAmount(unit.Descriptor),
                ["gritCurrent"] = unit.Descriptor.Resources.GetResourceAmount(resource)
            };
        }
    }
}
