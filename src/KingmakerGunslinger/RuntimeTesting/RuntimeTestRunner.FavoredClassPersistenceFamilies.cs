using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.Utility;
using Kingmaker.View.MapObjects.SriptZones;
using Kingmaker.Visual.CharacterSystem;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using KingmakerGunslinger.FavoredClass;
using KingmakerGunslinger.FavoredClass.Mechanics;
using KingmakerGunslinger.Spells.Teleportation;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private const string FcbPersistenceBardClassGuid = "772c83a25e2268e448e841dcd548235f";
        private const string FcbPersistenceSorcererClassGuid = "b3a505fb61437dc4097f43c3f8f9a4cf";
        private const string FcbPersistenceFireBreathAbilityGuid = "29e09b5e6855479486992ba5d721968e";
        private readonly List<UnitEntityData> _fcbFamilySubjects = new List<UnitEntityData>();

        // Fresh-process families (continuation item 5): each subject is built
        // in the prepare process, recorded exactly, saved in the same guarded
        // disposable save and compared in the verify process.
        private JObject PrepareFcbFamilySubjects(UnitEntityData anchor, Player player)
        {
            var library = BlueprintBootstrap.Library;
            FavoredClassBlueprintSet leaves = BlueprintBootstrap.FavoredClassLeaves;
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            FavoredClassHostHandles host = FavoredClassIntegrationCoordinator.Host;
            Func<string, BlueprintRace> race = ancestry => BlueprintLibraryLookup.RequireExact<BlueprintRace>(library,
                FavoredClassRaceIdentities.ForAncestry(ancestry).RaceGuid, ancestry);
            Func<string, BlueprintCharacterClass> characterClass = guid =>
                BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(library, guid, guid);
            var reserved = new HashSet<string>(StringComparer.Ordinal) { host.GunslingerSelection.AssetGuid };
            var expected = new JObject();

            // Performance: an invested bard performing Inspire Competence.
            UnitEntityData bard = SpawnFcbPartyUnit(anchor, player, race(FavoredClassAncestry.Oread),
                characterClass(FcbPersistenceBardClassGuid), "KMG FCB Persistence Bard");
            for (int level = 0; level < 3; level++)
                bard.Descriptor.Progression.AddClassLevel(characterClass(FcbPersistenceBardClassGuid));
            bard.Descriptor.AddFact(BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library,
                FavoredClassPerformanceManifest.For("InspireCompetence").FeatureGuid, "InspireCompetenceFeature"));
            GrantFavoredClassRanks(bard, leaves.Pair(FavoredClassCatalog.EffectPerformanceRange, "InspireCompetence").Full, 2);
            var performer = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library, FcbInspireCompetenceBuffGuid,
                "InspireCompetenceBuff");
            bard.Descriptor.AddBuff(performer, new MechanicsContext(bard, bard.Descriptor, performer));
            expected["bard"] = DescribeFcbFamilySubject(bard, null);

            // Aura: an invested paladin with Aura of Courage.
            UnitEntityData paladin = SpawnFcbPartyUnit(anchor, player, race(FavoredClassAncestry.Oread),
                characterClass(FcbPaladinClassGuid), "KMG FCB Persistence Paladin");
            for (int level = 0; level < 3; level++)
                paladin.Descriptor.Progression.AddClassLevel(characterClass(FcbPaladinClassGuid));
            paladin.Descriptor.AddFact(BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library,
                FcbAuraOfCourageFeatureGuid, "AuraOfCourageFeature"));
            GrantFavoredClassRanks(paladin, leaves.Pair(FavoredClassCatalog.EffectPaladinAuras, null).Full, 2);
            expected["paladin"] = DescribeFcbFamilySubject(paladin, null);

            // Pet projection: a ranger's companion with projected armor.
            UnitEntityData ranger = SpawnFcbPartyUnit(anchor, player, race(FavoredClassAncestry.Human),
                characterClass("cda0615668a6df14eb36ba19ee881af6"), "KMG FCB Persistence Ranger");
            GrantFavoredClassRanks(ranger, BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library,
                FcbAnimalCompanionRankGuid, "AnimalCompanionRank"), 4);
            ranger.Descriptor.AddFact(BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library,
                FcbCompanionLeopardGuid, "AnimalCompanionFeatureLeopard"));
            Game.Instance.EntityCreator.Tick();
            GrantFavoredClassRanks(ranger, leaves.Pair(FavoredClassCatalog.EffectCompanionArmor, null).Full, 2);
            if (ranger.Descriptor.Pet == null)
                throw new InvalidOperationException("The persistence ranger's companion did not spawn.");
            expected["ranger"] = DescribeFcbFamilySubject(ranger, null);

            // Mixed rates: an Undine Monk's grapple CMD and stunning uses.
            UnitEntityData monk = SpawnFcbPartyUnit(anchor, player, race(FavoredClassAncestry.Undine),
                characterClass(FcbMonkClassGuid), "KMG FCB Persistence Monk");
            LevelFcbClass(monk, race(FavoredClassAncestry.Undine), characterClass(FcbMonkClassGuid), 2);
            FavoredClassLeafPair grapple = leaves.Pair(FavoredClassCatalog.EffectGrappleStunning, null);
            GrantFavoredClassRanks(monk, grapple.Full, 1);
            if (grapple.Partial != null) GrantFavoredClassRanks(monk, grapple.Partial, 1);
            expected["monk"] = DescribeFcbFamilySubject(monk, null);

            // Selected revelation: an Ifrit Oracle's Fire Breath.
            BlueprintCharacterClass oracle = TeleportationFinalLiveReconciler.ResolveOracleClass(library);
            UnitEntityData oracleUnit = SpawnFcbPartyUnit(anchor, player, race(FavoredClassAncestry.Ifrit), oracle,
                "KMG FCB Persistence Revelation");
            for (int level = 0; level < 9; level++)
                oracleUnit.Descriptor.Progression.AddClassLevel(oracle);
            oracleUnit.Descriptor.AddFact(BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library,
                FavoredClassRevelationManifest.For("FireBreath").FeatureGuids[0], "Fire Breath"));
            GrantFavoredClassRanks(oracleUnit, leaves.Pair(FavoredClassCatalog.EffectSelectedRevelation, "FireBreath").Full, 2);
            expected["revelation"] = DescribeFcbFamilySubject(oracleUnit, FcbPersistenceFireBreathAbilityGuid);

            // Selected bloodline power: an Ifrit Sorcerer's Elemental Ray (Fire).
            FavoredClassLeafPair ray = leaves.Pair(FavoredClassCatalog.EffectSelectedBloodlinePower, "FireRay");
            FavoredClassSelectedPowerLevel power = ray.Full.GetComponent<FavoredClassSelectedPowerLevel>();
            if (power == null || power.Ability == null || power.PowerFeature == null)
                throw new InvalidOperationException("The Fire Ray counter has no exact power binding.");
            UnitEntityData sorcerer = SpawnFcbPartyUnit(anchor, player, race(FavoredClassAncestry.Ifrit),
                characterClass(FcbPersistenceSorcererClassGuid), "KMG FCB Persistence Bloodline");
            for (int level = 0; level < 5; level++)
                sorcerer.Descriptor.Progression.AddClassLevel(characterClass(FcbPersistenceSorcererClassGuid));
            sorcerer.Descriptor.AddFact(power.PowerFeature);
            GrantFavoredClassRanks(sorcerer, ray.Full, 2);
            expected["bloodline"] = DescribeFcbFamilySubject(sorcerer, power.Ability.AssetGuid);

            // Mostly Human: the human identity and the human grit it opens.
            ElementalMostlyHumanRaceBlueprints ifrit = BlueprintBootstrap.MostlyHuman.Races.Single(value =>
                ReferenceEquals(value.Race, race(FavoredClassAncestry.Ifrit)));
            UnitEntityData mostlyHuman = SpawnFcbPartyUnit(anchor, player, ifrit.Race, gunslinger.CharacterClass,
                "KMG FCB Persistence Mostly Human");
            var failures = new List<string>();
            LevelFcbRespecSubject(mostlyHuman, ifrit.Race,
                new[] { leaves.Pair(FavoredClassCatalog.EffectGrit, null).Partial }, reserved, failures, "persistence",
                ifrit.Trait);
            // Conditional effect: a Halfling Gunslinger's Nimble step.
            FavoredClassLeafPair nimble = leaves.Pair(FavoredClassCatalog.EffectHalflingNimble, null);
            UnitEntityData halfling = SpawnFcbPartyUnit(anchor, player, race(FavoredClassAncestry.Halfling),
                gunslinger.CharacterClass, "KMG FCB Persistence Nimble");
            LevelFcbRespecSubject(halfling, race(FavoredClassAncestry.Halfling), nimble.Partial == null
                ? new[] { nimble.Full, nimble.Full } : new[] { nimble.Partial, nimble.Partial, nimble.Partial, nimble.Full },
                reserved, failures, "persistence-nimble");
            if (failures.Count != 0)
                throw new InvalidOperationException(string.Join("; ", failures.ToArray()));
            expected["mostlyHuman"] = DescribeFcbFamilySubject(mostlyHuman, null);
            expected["nimble"] = DescribeFcbFamilySubject(halfling, null);
            FcbPersistenceAssert("families-prepare-committed",
                "every family subject was built: a performing invested bard, an invested paladin, a ranger with a projected companion, a mixed-rate Undine Monk, an invested Fire Breath Oracle, an invested Fire Ray Sorcerer, a Mostly Human Ifrit with human grit and a Halfling with a Nimble step",
                FcbFamiliesMeaningful(expected), expected);
            return expected;
        }

        private static bool FcbFamiliesMeaningful(JObject expected)
        {
            Func<string, JObject> subject = key => (JObject)expected[key];
            return ((JArray)subject("bard")["areas"]).Count == 1 &&
                (float)((JArray)subject("bard")["areas"])[0]["ringFactor"] > 1f &&
                ((JArray)subject("paladin")["areas"]).Count >= 1 &&
                subject("ranger")["census"]["pet"].Type == JTokenType.Object &&
                (int)subject("ranger")["census"]["pet"]["petFeatureRank"] == 2 &&
                ((JObject)subject("monk")["census"]["counters"]).Count >= 1 &&
                subject("revelation")["ability"].Type == JTokenType.Object &&
                subject("bloodline")["ability"].Type == JTokenType.Object &&
                (bool)subject("mostlyHuman")["census"]["mostlyHumanIdentity"] &&
                (bool)subject("mostlyHuman")["census"]["hostHumanAccess"] &&
                ((JArray)subject("nimble")["census"]["modifiers"]).Count >= 1;
        }

        private void VerifyFcbFamilySubjects(JObject expected, Player player)
        {
            foreach (JProperty entry in expected.Properties())
            {
                var saved = (JObject)entry.Value;
                string id = (string)saved["unitId"];
                UnitEntityData unit = player.Party.SingleOrDefault(value => value.UniqueId == id) ??
                    Game.Instance.State.Units.SingleOrDefault(value => value.UniqueId == id);
                if (unit == null)
                {
                    FcbPersistenceAssert("family-" + entry.Name + "-present", "the subject is in the reloaded save",
                        false, new { id });
                    continue;
                }
                _fcbFamilySubjects.Add(unit);
                JObject observed = DescribeFcbFamilySubject(unit, (string)saved["abilityGuid"]);
                FcbPersistenceAssert("family-" + entry.Name + "-reload",
                    "the fresh-process reload restores the exact counters, owned modifiers, resource maxima and spent amounts, pet projection, identity, owner-local areas and selected-power arithmetic",
                    JToken.DeepEquals(Normalize(saved), Normalize(observed)),
                    new { expected = saved, observed });
            }
            // The paladin's reloaded aura still gives its own +6 to an ally.
            var library = BlueprintBootstrap.Library;
            string paladinId = (string)expected["paladin"]["unitId"];
            UnitEntityData paladin = _fcbFamilySubjects.FirstOrDefault(value => value.UniqueId == paladinId);
            if (paladin != null)
            {
                var area = BlueprintLibraryLookup.RequireExact<BlueprintAbilityAreaEffect>(library,
                    FcbAuraOfCourageAreaGuid, "AuraOfCourageArea");
                AreaEffectEntityData aura = OwnedArea(paladin, area);
                int bonus = -1;
                UnitEntityData probe = null;
                BlueprintUnit blueprint = null;
                try
                {
                    blueprint = UnityEngine.Object.Instantiate(BlueprintRoot.Instance.DefaultPlayerCharacter);
                    blueprint.name = "KMG_Runtime_FcbPersistence_AuraProbe";
                    probe = Game.Instance.EntityCreator.SpawnUnit(blueprint, paladin.Position, Quaternion.identity,
                        Game.Instance.State.LoadedAreaState.MainState);
                    Game.Instance.EntityCreator.Tick();
                    if (probe != null) Game.Instance.CurrentScene.Area.InteractiveObjectGrid.MoveTo(probe,
                        paladin.Position.x, paladin.Position.z);
                    int plain = probe == null ? 0 : FearSaveBonus(probe);
                    if (aura != null && probe != null)
                    {
                        aura.Tick();
                        bonus = FearSaveBonus(probe) - plain;
                    }
                }
                finally
                {
                    if (probe != null) { probe.Destroy(); Game.Instance.EntityDestroyer.Tick(); }
                    if (blueprint != null) UnityEngine.Object.Destroy(blueprint);
                }
                FcbPersistenceAssert("family-paladin-aura-reload",
                    "after the fresh-process reload the paladin's own aura gives an ally exactly +6 against fear",
                    aura != null && bonus == 6, new { areaPresent = aura != null, bonus });
            }
            // The companion is replaced after the reload: the new one receives
            // the projection once and the old one keeps nothing.
            string rangerId = (string)expected["ranger"]["unitId"];
            UnitEntityData ranger = _fcbFamilySubjects.FirstOrDefault(value => value.UniqueId == rangerId);
            if (ranger != null)
            {
                FavoredClassBlueprintSet leaves = BlueprintBootstrap.FavoredClassLeaves;
                BlueprintFeature petFeature = leaves.PetFeature(FavoredClassCatalog.EffectCompanionArmor);
                UnitEntityData first = ranger.Descriptor.Pet;
                ranger.Descriptor.RemoveFact(BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library,
                    FcbCompanionLeopardGuid, "AnimalCompanionFeatureLeopard"));
                ranger.Descriptor.AddFact(BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library,
                    FcbCompanionWolfGuid, "AnimalCompanionFeatureWolf"));
                Game.Instance.EntityCreator.Tick();
                UnitEntityData second = ranger.Descriptor.Pet;
                int copies = second == null ? -1 : second.Descriptor.Progression.Features.Enumerable.Count(value =>
                    ReferenceEquals(value.Blueprint, petFeature));
                int rank = second == null ? -1 : second.Descriptor.Progression.Features.GetRank(petFeature);
                bool orphan = first != null && first.Descriptor.HasFact(petFeature);
                FcbPersistenceAssert("family-ranger-replacement-after-reload",
                    "replacing the reloaded companion gives the new companion the projected armor exactly once at the saved rank and leaves none on the old one",
                    second != null && !ReferenceEquals(first, second) && copies == 1 && rank == 2 && !orphan,
                    new { replaced = second != null && !ReferenceEquals(first, second), copies, rank, orphan });
            }
        }

        /// <summary>A subject's exact favored-class state, with optional ability arithmetic.</summary>
        private static JObject DescribeFcbFamilySubject(UnitEntityData unit, string abilityGuid)
        {
            var result = new JObject
            {
                ["unitId"] = unit.UniqueId,
                ["abilityGuid"] = abilityGuid,
                ["census"] = FcbCensus(unit),
            };
            var areas = new JArray();
            foreach (AreaEffectEntityData area in Game.Instance.State.AreaEffects.Where(value => value != null &&
                !value.IsEnded && value.Context != null && ReferenceEquals(value.Context.MaybeCaster, unit))
                .OrderBy(value => value.Blueprint.AssetGuid, StringComparer.Ordinal))
            {
                var cylinder = area.View == null ? null : area.View.Shape as ScriptZoneCylinder;
                GameObject ring = area.View == null ? null : typeof(Kingmaker.View.MapObjects.AreaEffectView)
                    .GetField("m_SpawnedFx", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(area.View) as GameObject;
                areas.Add(new JObject
                {
                    ["area"] = area.Blueprint.name,
                    ["radius"] = cylinder == null ? -1f : (float)Math.Round(cylinder.Radius, 3),
                    ["ringFactor"] = ring == null ? 1f : (float)Math.Round(FavoredClassPerformanceRing.FactorOf(ring), 3),
                });
            }
            result["areas"] = areas;
            if (abilityGuid != null)
            {
                var blueprint = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                    abilityGuid, abilityGuid);
                var data = new AbilityData(blueprint, unit.Descriptor);
                MechanicsContext context = data.CreateExecutionContext(new TargetWrapper(unit));
                context.Recalculate();
                result["ability"] = new JObject
                {
                    ["casterLevel"] = context.Params.CasterLevel,
                    ["dc"] = context.Params.DC,
                    ["rankBonus"] = context.Params.RankBonus,
                    ["default"] = context[AbilityRankType.Default],
                    ["damageDice"] = context[AbilityRankType.DamageDice],
                    ["damageBonus"] = context[AbilityRankType.DamageBonus],
                };
            }
            return result;
        }

        private static JObject Normalize(JObject subject)
        {
            var copy = (JObject)subject.DeepClone();
            copy.Remove("unitId");
            return copy;
        }

        /// <summary>A party unit with a real view in the cross-scene state, like the L01 subject.</summary>
        private UnitEntityData SpawnFcbPartyUnit(UnitEntityData anchor, Player player, BlueprintRace race,
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
                throw new InvalidOperationException("The persistence family subject has no real view.");
            view.Blueprint = game.BlueprintRoot.DefaultPlayerCharacter;
            view.UniqueId = Guid.NewGuid().ToString();
            view.transform.position = anchor.Position;
            var unit = game.EntityCreator.SpawnEntityWithView(view, player.CrossSceneState) as UnitEntityData;
            if (unit == null)
                throw new InvalidOperationException("Persistence family entity ownership transfer failed.");
            game.EntityCreator.Tick();
            unit.Descriptor.Doll = doll;
            unit.Descriptor.CustomGender = anchor.Descriptor.Gender;
            unit.Descriptor.CustomName = name;
            unit.Stats.Wisdom.BaseValue = 14;
            unit.Descriptor.TurnOn();
            if (unit.Descriptor.Progression.Race == null)
                unit.Descriptor.Progression.SetRace(race);
            _fcbFamilySubjects.Add(unit);
            player.PartyCharacters.Add(unit);
            player.InvalidateCharacterLists();
            player.UpdateCharacterLists();
            return unit;
        }

        // Both processes detach the family subjects; the prepare process also
        // disposes them (they belong to the disposable save only).
        private void DetachFcbFamilySubjects(Player player, bool dispose)
        {
            foreach (UnitEntityData unit in _fcbFamilySubjects.ToArray())
            {
                if (unit == null) continue;
                string id = unit.UniqueId;
                player.PartyCharacters.RemoveAll(value => value.UniqueId == id);
                if (!dispose) continue;
                UnitEntityData pet = unit.Descriptor.Pet;
                if (pet != null)
                {
                    pet.Descriptor.SetMaster(null);
                    if (pet.HoldingState != null && pet.HoldingState.AllEntityData.Contains(pet))
                        pet.HoldingState.RemoveEntityData(pet);
                    pet.Dispose();
                }
                if (unit.HoldingState != null && unit.HoldingState.AllEntityData.Contains(unit))
                    unit.HoldingState.RemoveEntityData(unit);
                unit.Dispose();
            }
            _fcbFamilySubjects.Clear();
            player.InvalidateCharacterLists();
            player.UpdateCharacterLists();
        }
    }
}
