using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Deeds;
using KingmakerGunslinger.FavoredClass;
using KingmakerGunslinger.FavoredClass.Mechanics;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Spells.Teleportation;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Row-level fresh-process cases (L01) for the rows no family subject
        // reaches: row id, subject, effect, target. The subjects share the
        // guarded disposable save. After the reload each row's own native
        // mechanic is measured with its counter and again after that counter
        // is removed (the verify process never saves): the difference must
        // equal the steps recorded before saving, and a neighboring context
        // must not change.
        private static readonly string[][] FcbPersistenceRows =
        {
            new[] { "G05", "halfOrc", FavoredClassCatalog.EffectPistolWhip, null },
            new[] { "G06", "halfling", FavoredClassCatalog.EffectHalflingDodge, null },
            new[] { "G11", "ifrit", FavoredClassCatalog.EffectInitiative, null },
            new[] { "G17", "drow", FavoredClassCatalog.EffectDrowNimble, null },
            new[] { "G21", "tiefling", FavoredClassCatalog.EffectDirtyTrickTrip, null },
            new[] { "I01", "ifrit", FavoredClassCatalog.EffectBombDamage, null },
            new[] { "I05", "ifrit", FavoredClassCatalog.EffectFireIntimidate, null },
            new[] { "I07", "ifrit", FavoredClassCatalog.EffectDemoralize, null },
            new[] { "O04", "oread", FavoredClassCatalog.EffectBullRushDragDefense, null },
            new[] { "O05", "oread", FavoredClassCatalog.EffectUnarmedConfirmation, null },
            new[] { "O08", "oread", FavoredClassCatalog.EffectEidolonArmor, null },
            new[] { "U02", "undine", FavoredClassCatalog.EffectAquaticPenetration, null },
            new[] { "S04", "sylph", FavoredClassCatalog.EffectSelectedRevelation, "LightningBreath" },
            new[] { "S06", "sylph", FavoredClassCatalog.EffectSelectedBloodlinePower, "AirRay" },
        };

        private static readonly Dictionary<string, int> FcbPersistenceRowRanks =
            new Dictionary<string, int>(StringComparer.Ordinal)
            {
                { "G05", 3 }, { "G06", 3 }, { "G11", 4 }, { "G17", 2 }, { "G21", 3 }, { "I01", 3 },
                { "I05", 2 }, { "I07", 2 }, { "O04", 2 }, { "O05", 5 }, { "O08", 2 }, { "U02", 3 },
                { "S04", 2 }, { "S06", 2 },
            };

        private JObject PrepareFcbRowSubjects(UnitEntityData anchor, Player player)
        {
            var library = BlueprintBootstrap.Library;
            FavoredClassBlueprintSet leaves = BlueprintBootstrap.FavoredClassLeaves;
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            Func<string, BlueprintRace> race = ancestry => BlueprintLibraryLookup.RequireExact<BlueprintRace>(library,
                FavoredClassRaceIdentities.ForAncestry(ancestry).RaceGuid, ancestry);
            var subjects = new Dictionary<string, UnitEntityData>(StringComparer.Ordinal);

            // Gunslingers: one class level and grit, saved with 1 point.
            foreach (Tuple<string, string> entry in new[]
            {
                Tuple.Create("halfOrc", FavoredClassAncestry.HalfOrc),
                Tuple.Create("halfling", FavoredClassAncestry.Halfling),
                Tuple.Create("ifrit", FavoredClassAncestry.Ifrit),
                Tuple.Create("drow", FavoredClassAncestry.Drow),
                Tuple.Create("tiefling", FavoredClassAncestry.Tiefling),
            })
            {
                UnitEntityData unit = SpawnFcbPartyUnit(anchor, player, race(entry.Item2), gunslinger.CharacterClass,
                    "KMG FCB Persistence Row " + entry.Item1);
                unit.Descriptor.Progression.AddClassLevel(gunslinger.CharacterClass);
                unit.Descriptor.AddFact(gunslinger.Grit.Feature);
                unit.Descriptor.Resources.Restore(gunslinger.Grit.Resource, 1);
                subjects[entry.Item1] = unit;
            }
            subjects["ifrit"].Descriptor.AddFact(gunslinger.Initiative);
            subjects["drow"].Descriptor.AddFact(gunslinger.Nimble.Features[0]);

            // Oread: the Fighter and Monk counters and Call of the Wild's
            // eidolon (its progression spawns the pet, as for the ranger).
            UnitEntityData oread = SpawnFcbPartyUnit(anchor, player, race(FavoredClassAncestry.Oread),
                gunslinger.CharacterClass, "KMG FCB Persistence Row oread");
            BlueprintFeature eidolon = library.GetAllBlueprints().OfType<BlueprintFeature>()
                .Where(value => value != null && value.name == FcbAngelEidolonProgressionName)
                .OrderBy(value => value.AssetGuid, StringComparer.Ordinal).FirstOrDefault();
            if (eidolon == null)
                throw new InvalidOperationException("Call of the Wild's eidolon progression is absent.");
            oread.Descriptor.AddFact(eidolon);
            Game.Instance.EntityCreator.Tick();
            if (oread.Descriptor.Pet == null)
                throw new InvalidOperationException("The persistence Oread's eidolon did not spawn.");
            subjects["oread"] = oread;
            subjects["undine"] = SpawnFcbPartyUnit(anchor, player, race(FavoredClassAncestry.Undine),
                gunslinger.CharacterClass, "KMG FCB Persistence Row undine");

            // Sylph: Oracle 9 with Lightning Breath and Sorcerer 5 with the
            // Elemental Ray (Air), each at its own class level.
            BlueprintCharacterClass oracle = TeleportationFinalLiveReconciler.ResolveOracleClass(library);
            BlueprintCharacterClass sorcerer = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(library,
                FcbPersistenceSorcererClassGuid, "Sorcerer");
            UnitEntityData sylph = SpawnFcbPartyUnit(anchor, player, race(FavoredClassAncestry.Sylph), oracle,
                "KMG FCB Persistence Row sylph");
            for (int level = 0; level < 9; level++)
                sylph.Descriptor.Progression.AddClassLevel(oracle);
            for (int level = 0; level < 5; level++)
                sylph.Descriptor.Progression.AddClassLevel(sorcerer);
            sylph.Descriptor.AddFact(BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library,
                FavoredClassRevelationManifest.For("LightningBreath").FeatureGuids[0], "Lightning Breath"));
            FavoredClassSelectedPowerLevel airRay = leaves.Pair(FavoredClassCatalog.EffectSelectedBloodlinePower,
                "AirRay").Full.GetComponent<FavoredClassSelectedPowerLevel>();
            if (airRay == null || airRay.Ability == null || airRay.PowerFeature == null)
                throw new InvalidOperationException("The Air Ray counter has no exact power binding.");
            sylph.Descriptor.AddFact(airRay.PowerFeature);
            subjects["sylph"] = sylph;

            var rows = new JObject();
            foreach (string[] row in FcbPersistenceRows)
            {
                FavoredClassLeafPair pair = leaves.Pair(row[2], row[3]);
                UnitEntityData unit = subjects[row[1]];
                GrantFavoredClassRanks(unit, pair.Full, FcbPersistenceRowRanks[row[0]]);
                rows[row[0]] = new JObject
                {
                    ["subject"] = row[1],
                    ["effect"] = row[2],
                    ["target"] = row[3] == null ? JValue.CreateNull() : (JToken)row[3],
                    ["steps"] = FavoredClassEarnedSteps.For(unit.Descriptor, row[2], row[3]),
                };
            }
            BlueprintAbility lightningBreath = FcbRevelationAbility("LightningBreath");
            var described = new JObject();
            foreach (KeyValuePair<string, UnitEntityData> subject in subjects)
            {
                JObject census = DescribeFcbFamilySubject(subject.Value, null);
                if (subject.Key == "sylph")
                    census["abilities"] = new JObject
                    {
                        ["lightningBreath"] = FcbAbilityArithmetic(subject.Value, lightningBreath),
                        ["airRay"] = FcbAbilityArithmetic(subject.Value, airRay.Ability),
                    };
                described[subject.Key] = census;
            }
            var expected = new JObject
            {
                ["subjects"] = described,
                ["rows"] = rows,
                ["lightningBreathAbility"] = lightningBreath.AssetGuid,
            };
            bool meaningful = rows.Properties().All(value => (int)value.Value["steps"] > 0) &&
                ((JObject)described["oread"]["census"])["pet"] != null &&
                ((JObject)described["oread"]["census"])["pet"].Type == JTokenType.Object;
            FcbPersistenceAssert("rows-prepare-committed",
                "every row subject was built with its counters: Half-orc Pistol-Whip, Halfling Dodge, Ifrit Initiative, bombs, fire Intimidate and Demoralize, Drow Nimble, Tiefling trip, Oread bull-rush CMD, unarmed confirmation and eidolon armor, Undine spell penetration, and a Sylph's Lightning Breath and Air Ray; each row earns at least one step",
                meaningful, expected);
            return expected;
        }

        private void VerifyFcbRowSubjects(JObject expected, Player player)
        {
            var library = BlueprintBootstrap.Library;
            FavoredClassBlueprintSet leaves = BlueprintBootstrap.FavoredClassLeaves;
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            var subjects = new Dictionary<string, UnitEntityData>(StringComparer.Ordinal);
            BlueprintAbility lightningBreath = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library,
                (string)expected["lightningBreathAbility"], "Lightning Breath ability");
            BlueprintAbility airRay = leaves.Pair(FavoredClassCatalog.EffectSelectedBloodlinePower, "AirRay").Full
                .GetComponent<FavoredClassSelectedPowerLevel>().Ability;
            foreach (JProperty entry in ((JObject)expected["subjects"]).Properties())
            {
                var saved = (JObject)entry.Value;
                string id = (string)saved["unitId"];
                UnitEntityData unit = player.Party.SingleOrDefault(value => value.UniqueId == id) ??
                    Game.Instance.State.Units.SingleOrDefault(value => value.UniqueId == id);
                if (unit == null)
                {
                    FcbPersistenceAssert("row-subject-" + entry.Name + "-present",
                        "the row subject is in the reloaded save", false, new { id });
                    continue;
                }
                _fcbFamilySubjects.Add(unit);
                subjects[entry.Name] = unit;
                JObject observed = DescribeFcbFamilySubject(unit, null);
                if (entry.Name == "sylph")
                    observed["abilities"] = new JObject
                    {
                        ["lightningBreath"] = FcbAbilityArithmetic(unit, lightningBreath),
                        ["airRay"] = FcbAbilityArithmetic(unit, airRay),
                    };
                FcbPersistenceAssert("row-subject-" + entry.Name + "-reload",
                    "the fresh-process reload restores the subject's exact counters, owned modifiers, resources, pet projection and selected-power arithmetic",
                    JToken.DeepEquals(Normalize(saved), Normalize(observed)),
                    new { expected = saved, observed });
            }
            if (subjects.Count != ((JObject)expected["subjects"]).Count)
                return;

            UnitEntityData anchor = subjects["halfOrc"];
            var spawned = new List<UnitEntityData>();
            var blueprints = new List<BlueprintUnit>();
            Func<string, UnitEntityData> spawnTarget = name =>
            {
                BlueprintUnit blueprint = UnityEngine.Object.Instantiate(BlueprintRoot.Instance.DefaultPlayerCharacter);
                blueprint.name = "KMG_Runtime_FcbPersistence_Row" + name;
                blueprints.Add(blueprint);
                UnitEntityData unit = Game.Instance.EntityCreator.SpawnUnit(blueprint, anchor.Position,
                    Quaternion.identity, Game.Instance.State.LoadedAreaState.MainState);
                Game.Instance.EntityCreator.Tick();
                if (unit == null)
                    throw new InvalidOperationException("The persistence row target did not spawn.");
                spawned.Add(unit);
                unit.Descriptor.State.Immortality.Retain();
                unit.Descriptor.Stats.HitPoints.BaseValue = 1000;
                return unit;
            };
            var observer = new FavoredClassSkillCheckObserver();
            EventBus.Subscribe(observer);
            try
            {
                UnitEntityData plain = spawnTarget("Plain");
                UnitEntityData fire = spawnTarget("Fire");
                fire.Descriptor.AddFact(BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library,
                    FavoredClassBlueprints.SubtypeFireGuid, "SubtypeFire"));
                UnitEntityData water = spawnTarget("Water");
                water.Descriptor.AddFact(BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library,
                    FavoredClassBlueprints.SubtypeWaterGuid, "SubtypeWater"));
                var probes = new FcbRowProbes(gunslinger, plain, fire, water, observer);
                foreach (string[] row in FcbPersistenceRows)
                {
                    JObject saved = (JObject)expected["rows"][row[0]];
                    UnitEntityData unit = subjects[row[1]];
                    FavoredClassLeafPair pair = leaves.Pair(row[2], row[3]);
                    int steps = (int)saved["steps"];
                    int stepsNow = FavoredClassEarnedSteps.For(unit.Descriptor, row[2], row[3]);
                    FcbRowMeasure measure = probes.For(row[0], unit, lightningBreath, airRay);
                    int withOwn = measure.Own(), withNeighbor = measure.Neighbor();
                    RemoveFavoredClassRanks(unit, pair.Full);
                    RemoveFavoredClassRanks(unit, pair.Partial);
                    int withoutOwn = measure.Own(), withoutNeighbor = measure.Neighbor();
                    FcbPersistenceAssert("row-" + row[0] + "-mechanic-after-reload",
                        "after the fresh-process reload the row's own native mechanic (" + measure.Description +
                            ") gives exactly the steps saved before the reload, removing its counter takes exactly that away, and the neighboring context (" +
                            measure.NeighborDescription + ") does not change",
                        steps > 0 && stepsNow == steps && withOwn - withoutOwn == steps &&
                            withNeighbor == withoutNeighbor,
                        new { steps, stepsNow, withOwn, withoutOwn, withNeighbor, withoutNeighbor });
                }
            }
            finally
            {
                EventBus.Unsubscribe(observer);
                foreach (UnitEntityData unit in spawned)
                {
                    unit.Descriptor.State.Immortality.ReleaseAll();
                    unit.Destroy();
                }
                Game.Instance.EntityDestroyer.Tick();
                foreach (BlueprintUnit blueprint in blueprints)
                    UnityEngine.Object.Destroy(blueprint);
            }
        }

        /// <summary>The first ability whose parameters a revelation target's scope raises.</summary>
        private static BlueprintAbility FcbRevelationAbility(string key)
        {
            FavoredClassRevelationScope scope = FavoredClassRevelationScopes.ForKey(key);
            BlueprintAbility ability = scope == null ? null : scope.ParamsAbilities.OfType<BlueprintAbility>()
                .OrderBy(value => value.AssetGuid, StringComparer.Ordinal).FirstOrDefault();
            if (ability == null)
                throw new InvalidOperationException("The " + key + " revelation scope raises no ability.");
            return ability;
        }

        private static JObject FcbAbilityArithmetic(UnitEntityData unit, BlueprintAbility ability)
        {
            var data = new AbilityData(ability, unit.Descriptor);
            MechanicsContext context = data.CreateExecutionContext(new TargetWrapper(unit));
            context.Recalculate();
            return new JObject
            {
                ["ability"] = ability.AssetGuid,
                ["casterLevel"] = context.Params.CasterLevel,
                ["dc"] = context.Params.DC,
                ["rankBonus"] = context.Params.RankBonus,
                ["default"] = context[AbilityRankType.Default],
                ["damageDice"] = context[AbilityRankType.DamageDice],
                ["damageBonus"] = context[AbilityRankType.DamageBonus],
            };
        }

        /// <summary>A row's own native value and one neighboring value it must leave unchanged.</summary>
        private sealed class FcbRowMeasure
        {
            internal FcbRowMeasure(string description, Func<int> own, string neighborDescription,
                Func<int> neighbor)
            {
                Description = description;
                Own = own;
                NeighborDescription = neighborDescription;
                Neighbor = neighbor;
            }

            internal string Description { get; private set; }
            internal Func<int> Own { get; private set; }
            internal string NeighborDescription { get; private set; }
            internal Func<int> Neighbor { get; private set; }
        }

        private sealed class FcbRowProbes
        {
            private readonly GunslingerClassBlueprintSet _gunslinger;
            private readonly UnitEntityData _plain, _fire, _water;
            private readonly FavoredClassSkillCheckObserver _observer;

            internal FcbRowProbes(GunslingerClassBlueprintSet gunslinger, UnitEntityData plain,
                UnitEntityData fire, UnitEntityData water, FavoredClassSkillCheckObserver observer)
            {
                _gunslinger = gunslinger;
                _plain = plain;
                _fire = fire;
                _water = water;
                _observer = observer;
            }

            internal FcbRowMeasure For(string row, UnitEntityData unit, BlueprintAbility lightningBreath,
                BlueprintAbility airRay)
            {
                var library = BlueprintBootstrap.Library;
                switch (row)
                {
                    case "G05":
                        return new FcbRowMeasure("the Pistol-Whip deed attack's attack bonus", () => Whip(unit),
                            "an ordinary pistol shot's attack bonus", () => FireOrdinaryShot(unit, _plain).AttackBonus);
                    case "G06":
                        return new FcbRowMeasure("AC while the Gunslinger's Dodge buff is active",
                            () => DodgeArmorClass(unit), "AC without the buff", () => unit.Stats.AC.ModifiedValue);
                    case "G11":
                        return new FcbRowMeasure("the Initiative deed's bonus on a native initiative roll with grit",
                            () => Initiative(unit), "the unit's Initiative stat", () => unit.Stats.Initiative.ModifiedValue);
                    case "G17":
                        return new FcbRowMeasure("Nimble AC", () => unit.Stats.AC.ModifiedValue,
                            "flat-footed AC", () => unit.Stats.AC.FlatFooted);
                    case "G21":
                        return new FcbRowMeasure("trip CMB", () => Cmb(unit, CombatManeuver.Trip),
                            "bull rush CMB", () => Cmb(unit, CombatManeuver.BullRush));
                    case "I01":
                        var bomb = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library, FcbBombStandardGuid,
                            "BombStandart");
                        var fireball = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library, FcbFireballGuid,
                            "Fireball");
                        return new FcbRowMeasure("a bomb's fixed 5-point fire damage", () => BombProbe(unit, bomb),
                            "the same damage from Fireball", () => BombProbe(unit, fireball));
                    case "I05":
                        return new FcbRowMeasure("a demoralize check against a fire creature",
                            () => Intimidate(unit, _fire, true), "a demoralize check against a creature without the fire subtype",
                            () => Intimidate(unit, _plain, true));
                    case "I07":
                        return new FcbRowMeasure("a demoralize check", () => Intimidate(unit, _plain, true),
                            "an Intimidate check that is not a demoralize", () => Intimidate(unit, _plain, false));
                    case "O04":
                        return new FcbRowMeasure("CMD against bull rush", () => CmdAgainst(_plain, unit, CombatManeuver.BullRush),
                            "CMD against trip", () => CmdAgainst(_plain, unit, CombatManeuver.Trip));
                    case "O05":
                        var longsword = BlueprintLibraryLookup.RequireExact<BlueprintItemWeapon>(library, FcbLongswordGuid,
                            "Longsword");
                        return new FcbRowMeasure("an unarmed strike's critical confirmation bonus",
                            () => Confirmation(unit, unit.Body.EmptyHandWeapon),
                            "a longsword's critical confirmation bonus",
                            () => Confirmation(unit, new ItemEntityWeapon(longsword)));
                    case "O08":
                        return new FcbRowMeasure("the eidolon's AC", () => unit.Descriptor.Pet == null ? int.MinValue :
                                unit.Descriptor.Pet.Stats.AC.ModifiedValue,
                            "the eidolon's touch AC", () => unit.Descriptor.Pet == null ? int.MinValue :
                                unit.Descriptor.Pet.Stats.AC.Touch);
                    case "U02":
                        var magicMissile = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library,
                            FcbMagicMissileGuid, "MagicMissile");
                        return new FcbRowMeasure("spell penetration against a water creature",
                            () => Penetration(unit, magicMissile, _water), "spell penetration against a plain creature",
                            () => Penetration(unit, magicMissile, _plain));
                    case "S04":
                        return new FcbRowMeasure("Lightning Breath's caster level", () => CasterLevel(unit, lightningBreath),
                            "the Air Ray's caster level", () => CasterLevel(unit, airRay));
                    case "S06":
                        return new FcbRowMeasure("the Air Ray's caster level", () => CasterLevel(unit, airRay),
                            "Lightning Breath's caster level", () => CasterLevel(unit, lightningBreath));
                    default:
                        throw new InvalidOperationException("No persistence probe for row " + row + ".");
                }
            }

            private int Whip(UnitEntityData attacker)
            {
                var weapon = new ItemEntityWeapon(BlueprintBootstrap.ProductionFirearms.Pistol.Item);
                if (attacker.Body.PrimaryHand.MaybeItem != null)
                    throw new InvalidOperationException("The Pistol-Whip subject already holds a weapon.");
                attacker.Body.PrimaryHand.InsertItem(weapon);
                try
                {
                    FirearmRuntimeState.Service.Set(weapon, new FirearmState(FirearmState.CurrentSchemaVersion, 1,
                        FirearmStateTokenCatalog.DiagnosticLeadBall, FirearmCondition.Normal));
                    attacker.Descriptor.Resources.Restore(_gunslinger.Grit.Resource, 1);
                    UnityEngine.Random.InitState(FindNativeD20Seed(15));
                    PistolWhipResult result = PistolWhipRuntime.ExecuteForRuntimeTest(attacker, _plain,
                        _gunslinger.PistolWhip.OneHandedItem, _gunslinger.PistolWhip.TwoHandedItem, false);
                    return result == null || result.Attack == null || result.Attack.AttackRoll == null
                        ? int.MinValue : result.Attack.AttackRoll.AttackBonus;
                }
                finally
                {
                    FirearmRuntimeState.Service.Forget(weapon);
                    if (attacker.Body.PrimaryHand.MaybeItem != null) attacker.Body.PrimaryHand.RemoveItem(false);
                    attacker.Descriptor.Resources.Restore(_gunslinger.Grit.Resource, 1);
                }
            }

            private int DodgeArmorClass(UnitEntityData unit)
            {
                unit.Descriptor.Buffs.AddBuff(_gunslinger.Dodge.ArmorClassBuff, unit, TimeSpan.FromSeconds(6));
                try { return unit.Stats.AC.ModifiedValue; }
                finally { unit.Descriptor.RemoveFact(_gunslinger.Dodge.ArmorClassBuff); }
            }

            private int Initiative(UnitEntityData unit)
            {
                unit.Descriptor.Resources.Restore(_gunslinger.Grit.Resource, 1);
                var rule = new RuleInitiativeRoll(unit);
                Rulebook.Trigger(rule);
                EventBus.RaiseEvent<IUnitInitiativeHandler>(handler => handler.HandleUnitRollsInitiative(rule));
                return rule.Modifier - unit.Stats.Initiative.ModifiedValue;
            }

            private int Cmb(UnitEntityData unit, CombatManeuver maneuver)
            {
                return Rulebook.Trigger(new RuleCalculateCMB(unit, _plain, maneuver)).Result;
            }

            private int BombProbe(UnitEntityData caster, BlueprintScriptableObject blueprint)
            {
                var context = new MechanicsContext(caster, caster.Descriptor, blueprint);
                using (context.GetDataScope(new TargetWrapper(_plain)))
                {
                    RuleDealDamage deal = context.TriggerRule(new RuleDealDamage(caster, _plain, new DamageBundle(
                        new EnergyDamage(new DiceFormula(FcbBombProbeBase, DiceType.One), DamageEnergyType.Fire))));
                    return deal.Damage;
                }
            }

            private int Intimidate(UnitEntityData caster, UnitEntityData victim, bool viaDemoralize)
            {
                BlueprintAbility persuasion = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                    BlueprintBootstrap.Library, FcbPersuasionUseAbilityGuid, "PersuasionUseAbility");
                Demoralize demoralize = FindAction<Demoralize>(persuasion);
                if (demoralize == null)
                    throw new InvalidOperationException("The native Demoralize element was not found.");
                _observer.Checks.Clear();
                var context = new MechanicsContext(caster, caster.Descriptor, persuasion);
                using (context.GetDataScope(new TargetWrapper(victim)))
                {
                    if (viaDemoralize)
                        demoralize.RunAction();
                    else
                        context.TriggerRule(new RuleSkillCheck(caster, StatType.CheckIntimidate, 10));
                }
                Tuple<UnitEntityData, int> check = _observer.Checks.LastOrDefault(value =>
                    ReferenceEquals(value.Item1, caster));
                return check == null ? int.MinValue : check.Item2;
            }

            private int Confirmation(UnitEntityData attacker, ItemEntityWeapon weapon)
            {
                var roll = new RuleAttackRoll(attacker, _plain, weapon, 0);
                Rulebook.Trigger(roll);
                return roll.CriticalConfirmationBonus;
            }

            private static int Penetration(UnitEntityData caster, BlueprintAbility spell, UnitEntityData victim)
            {
                var context = new MechanicsContext(caster, caster.Descriptor, spell);
                return Rulebook.Trigger(new RuleSpellResistanceCheck(context, victim)).AdditionalSpellPenetration;
            }

            private static int CasterLevel(UnitEntityData unit, BlueprintAbility ability)
            {
                return (int)FcbAbilityArithmetic(unit, ability)["casterLevel"];
            }
        }
    }
}
