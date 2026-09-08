using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Actual native stat, save and damage rules for Release C passives.
    /// Only request-local units, items and unregistered fixture contexts change.
    /// </summary>
    internal static class ElementalAlternateTraitPassiveScenario
    {
        internal static void Exercise(RuntimeTestRequest request,
            ICollection<RuntimeTestAssertion> assertions,
            ICollection<string> evidenceFiles)
        {
            var rows = new JArray();
            var units = new List<UnitEntityData>();
            var temporary = new List<UnityEngine.Object>();
            var items = new List<ItemEntityWeapon>();
            UnitEntityData[] before = Game.Instance.State.Units.All.ToArray();
            try
            {
                ElementalRaceBlueprintSet races = BlueprintBootstrap.ElementalRaces;
                UnitEntityData source = Create(null, units, temporary);
                foreach (ElementalAlternateTraitId id in new[]
                {
                    ElementalAlternateTraitId.WildfireHeart,
                    ElementalAlternateTraitId.GraniteSkin,
                    ElementalAlternateTraitId.LikeTheWind,
                    ElementalAlternateTraitId.WhisperingWind
                })
                {
                    ElementalRaceBlueprints race = RaceFor(races, id);
                    UnitEntityData unit = Create(race, units, temporary);
                    ElementalAlternateTraitBlueprints trait =
                        race.AlternateTraits.Require(id);
                    StatType stat = id == ElementalAlternateTraitId.WildfireHeart
                        ? StatType.Initiative : id == ElementalAlternateTraitId
                            .GraniteSkin ? StatType.AC :
                            id == ElementalAlternateTraitId.LikeTheWind
                                ? StatType.Speed : StatType.SkillStealth;
                    int expected = id == ElementalAlternateTraitId.GraniteSkin
                        ? 1 : id == ElementalAlternateTraitId.LikeTheWind ? 5 : 4;
                    int baseline = Measure(unit, stat);
                    Select(unit, race, trait, assertions, rows);
                    int active = Measure(unit, stat);
                    Fact provider = unit.Descriptor.GetFact(trait.Provider);
                    ModifiableValue.Modifier[] owned = unit.Stats.GetStat(stat)
                        .Modifiers.Where(value => ReferenceEquals(value.Source,
                            provider)).ToArray();
                    ModifierDescriptor descriptor = id ==
                        ElementalAlternateTraitId.GraniteSkin
                            ? ModifierDescriptor.NaturalArmor :
                            ModifierDescriptor.Racial;
                    Check(assertions, rows, id + "-native-stat",
                        active - baseline == expected && owned.Length == 1 &&
                        owned[0].ModValue == expected &&
                        owned[0].ModDescriptor == descriptor,
                        "before=" + baseline + ";active=" + active +
                        ";owned=" + owned.Length + ";descriptor=" + descriptor);
                    ElementalHeritageRuntime.Reconcile(unit.Descriptor, null, null);
                    Check(assertions, rows, id + "-no-duplicate",
                        Measure(unit, stat) == active,
                        "native value after repeated reconciliation=" + Measure(unit, stat));
                    if (id == ElementalAlternateTraitId.LikeTheWind)
                        Movement(unit, trait, assertions, rows);
                    unit.Descriptor.RemoveFact(trait.Marker);
                    Check(assertions, rows, id + "-remove",
                        Measure(unit, stat) == baseline &&
                        !unit.Descriptor.HasFact(trait.Provider),
                        "restored=" + Measure(unit, stat));
                }
                foreach (ElementalAlternateTraitId id in new[]
                {
                    ElementalAlternateTraitId.ForgeHardened,
                    ElementalAlternateTraitId.Secretive
                })
                    Saves(Create(RaceFor(races, id), units, temporary), source,
                        RaceFor(races, id), id, temporary, assertions, rows);
                Resistance(Create(races.Sylph, units, temporary), source,
                    races.Sylph, assertions, rows);
                Brazen(Create(races.Ifrit, units, temporary), source,
                    races.Ifrit, items, assertions, rows);
            }
            finally
            {
                foreach (UnitEntityData unit in units.AsEnumerable().Reverse())
                {
                    unit.Commands.InterruptAll(true);
                    if (unit.CombatState.IsInCombat) unit.CombatState.LeaveCombat();
                    if (unit.Body.PrimaryHand.MaybeItem != null)
                        unit.Body.PrimaryHand.RemoveItem(false);
                }
                foreach (ItemEntityWeapon item in items) item.Dispose();
                foreach (UnitEntityData unit in units.AsEnumerable().Reverse())
                {
                    Game.Instance.State.Units.All.Remove(unit);
                    unit.Descriptor.State.Immortality.ReleaseAll();
                    unit.Dispose();
                }
                foreach (UnityEngine.Object value in temporary
                    .AsEnumerable().Reverse())
                    UnityEngine.Object.DestroyImmediate(value);
                bool clean = before.Length == Game.Instance.State.Units.All.Count &&
                    before.All(value => Game.Instance.State.Units.All.Contains(value));
                Check(assertions, rows, "passive-fixture-cleanup", clean,
                    "created=" + units.Count + ";original=" + before.Length +
                    ";after=" + Game.Instance.State.Units.All.Count);
                string path = Path.Combine(request.EvidenceDirectory,
                    "elemental-alternate-trait-passives.json");
                File.WriteAllText(path, new JObject
                {
                    { "schemaVersion", 1 }, { "saveStateTouched", false },
                    { "createdUnits", units.Count }, { "cleanupExact", clean },
                    { "observations", rows }
                }.ToString(Formatting.Indented));
                evidenceFiles.Add(path);
            }
        }

        private static int Measure(UnitEntityData unit, StatType stat)
        {
            if (stat == StatType.Initiative)
                return Rulebook.Trigger(new RuleInitiativeRoll(unit, null)).Modifier;
            if (stat == StatType.SkillStealth)
                return Rulebook.Trigger(new RuleSkillCheck(unit, stat, 100)
                    { IgnoreDifficultyBonusToDC = true }).StatValue;
            return unit.Stats.GetStat(stat).ModifiedValue;
        }

        private static void Movement(UnitEntityData unit,
            ElementalAlternateTraitBlueprints trait,
            ICollection<RuntimeTestAssertion> assertions, JArray rows)
        {
            BlueprintBuff haste = Exact<BlueprintBuff>(
                "03464790f40c3c24aa684b57155f3280");
            BlueprintBuff slow = BlueprintBootstrap.Library.GetAllBlueprints()
                .OfType<BlueprintBuff>().Single(value => value != null &&
                    string.Equals(value.name, "SlowBuff", StringComparison.Ordinal));
            int normal = unit.Stats.Speed.ModifiedValue;
            foreach (BlueprintBuff buff in new[] { haste, slow })
            {
                var context = new MechanicsContext(unit, unit.Descriptor, buff,
                    null, new TargetWrapper(unit));
                Buff active = unit.Descriptor.Buffs.AddBuff(buff, context,
                    TimeSpan.FromSeconds(60));
                if (active == null) throw new InvalidOperationException(
                    "Native movement buff was rejected.");
                try
                {
                    int observed = unit.Stats.Speed.ModifiedValue;
                    bool condition = !ReferenceEquals(buff, slow) ||
                        unit.Descriptor.State.HasCondition(UnitCondition.Slowed);
                    Check(assertions, rows, "like-wind-" + buff.name,
                        condition && (ReferenceEquals(buff, haste)
                            ? observed == normal + 30 : observed == normal),
                        "nativeSpeed=" + observed + ";normal=" + normal +
                        ";nativeSlowCondition=" + condition +
                        ";buffGuid=" + buff.AssetGuid);
                }
                finally { unit.Descriptor.Buffs.RemoveFact(active); }
            }
            unit.Descriptor.State.AddCondition(UnitCondition.DifficultTerrain, null);
            try
            {
                Check(assertions, rows, "like-wind-native-terrain",
                    unit.Descriptor.State.HasCondition(UnitCondition.DifficultTerrain) &&
                    unit.Stats.Speed.ModifiedValue == normal,
                    "native difficult-terrain condition retained; stat speed=" +
                        unit.Stats.Speed.ModifiedValue);
            }
            finally
            {
                unit.Descriptor.State.RemoveCondition(UnitCondition.DifficultTerrain);
            }
            BlueprintFeature movement = Exact<BlueprintFeature>(
                UrbanBarbarianBlueprints.FastMovementGuid);
            unit.Descriptor.AddFact(movement);
            try
            {
                Check(assertions, rows, "like-wind-native-class-movement",
                    unit.Stats.Speed.ModifiedValue == normal + 10,
                    "native Barbarian movement speed=" +
                        unit.Stats.Speed.ModifiedValue);
            }
            finally { unit.Descriptor.RemoveFact(movement); }

            BlueprintItemArmor armor = Exact<BlueprintItemArmor>(
                "559b0b6f194656c428c403a000ceee78");
            var item = new ItemEntityArmor(armor);
            int strengthBefore = unit.Stats.Strength.BaseValue;
            try
            {
                unit.Stats.Strength.BaseValue = 30;
                unit.Body.Armor.InsertItem(item);
                Check(assertions, rows, "like-wind-native-armor",
                    ReferenceEquals(unit.Body.Armor.Armor, item) &&
                    unit.Stats.Speed.ModifiedValue == normal - 10,
                    "native heavy armor speed=" + unit.Stats.Speed.ModifiedValue);
                unit.Body.Armor.RemoveItem(false);
                bool found = false;
                for (int strength = 1; strength <= 30; strength++)
                {
                    unit.Stats.Strength.BaseValue = strength;
                    var capacity = EncumbranceHelper.GetCarryingCapacity(
                        unit.Descriptor);
                    if (armor.Weight > capacity.Medium &&
                        armor.Weight <= capacity.Heavy)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found) throw new InvalidOperationException(
                    "Native heavy encumbrance fixture could not be established.");
                unit.Body.Armor.InsertItem(item);
                Encumbrance load = EncumbranceHelper.GetEncumbrance(unit.Descriptor);
                unit.Descriptor.Encumbrance = load;
                unit.Descriptor.Ensure<UnitPartEncumbrance>().Init(load);
                unit.Body.Armor.RemoveItem(false);
                Check(assertions, rows, "like-wind-native-encumbrance",
                    load == Encumbrance.Heavy &&
                    unit.Stats.Speed.ModifiedValue == normal - 10,
                    "calculated load=" + load +
                    ";native encumbrance-only speed=" + unit.Stats.Speed.ModifiedValue);
            }
            finally
            {
                if (unit.Body.Armor.HasArmor) unit.Body.Armor.RemoveItem(false);
                item.Dispose();
                unit.Stats.Strength.BaseValue = strengthBefore;
                Encumbrance load = EncumbranceHelper.GetEncumbrance(unit.Descriptor);
                unit.Descriptor.Encumbrance = load;
                unit.Descriptor.Ensure<UnitPartEncumbrance>().Init(load);
            }
            Check(assertions, rows, "like-wind-layering-restored",
                unit.Stats.Speed.ModifiedValue == normal,
                "native speed restored after class/armor/load changes=" +
                    unit.Stats.Speed.ModifiedValue);
        }

        private static void Saves(UnitEntityData unit, UnitEntityData source,
            ElementalRaceBlueprints race, ElementalAlternateTraitId id,
            ICollection<UnityEngine.Object> temporary,
            ICollection<RuntimeTestAssertion> assertions, JArray rows)
        {
            ElementalAlternateTraitBlueprints trait = race.AlternateTraits.Require(id);
            BlueprintAbility fatigue = ContextAbility(SpellDescriptor.Fatigue,
                SpellSchool.Necromancy, null, temporary);
            BlueprintAbility exhaustion = ContextAbility(SpellDescriptor.Exhausted,
                SpellSchool.None, null, temporary);
            BlueprintAbility both = ContextAbility(SpellDescriptor.Fatigue |
                SpellDescriptor.Exhausted, SpellSchool.Enchantment, null, temporary);
            BlueprintAbility divination = ContextAbility(SpellDescriptor.None,
                SpellSchool.Divination, null, temporary);
            BlueprintAbility parentVariant = ContextAbility(SpellDescriptor.None,
                SpellSchool.None, both, temporary);
            BlueprintAbility schoolOverlap = ContextAbility(SpellDescriptor.None,
                SpellSchool.Enchantment, divination, temporary);
            BlueprintAbility poison = ContextAbility(SpellDescriptor.Poison,
                SpellSchool.Necromancy, null, temporary);
            BlueprintAbility[] abilities =
                { null, fatigue, exhaustion, both, divination,
                    parentVariant, schoolOverlap, poison };
            int[] expected = id == ElementalAlternateTraitId.ForgeHardened
                ? new[] { 0, 2, 2, 2, 0, 2, 0, 0 }
                : new[] { 0, 0, 0, 2, 2, 2, 2, 0 };
            foreach (SavingThrowType save in new[] { SavingThrowType.Fortitude,
                SavingThrowType.Reflex, SavingThrowType.Will })
            {
                int[] before = abilities.Select(ability =>
                    Save(unit, source, ability, save)).ToArray();
                Select(unit, race, trait, assertions, rows);
                for (int index = 0; index < abilities.Length; index++)
                {
                    int observed = Save(unit, source, abilities[index], save) - before[index];
                    Check(assertions, rows, id + "-" + save + "-source-" + index,
                        observed == expected[index], "bonus=" + observed +
                        ";expected=" + expected[index]);
                }
                unit.Descriptor.RemoveFact(trait.Marker);
                Check(assertions, rows, id + "-" + save + "-temporary-cleanup",
                    Save(unit, source, both, save) == before[3],
                    "save restored after native trait removal");
            }
            if (id == ElementalAlternateTraitId.ForgeHardened)
            {
                RuleSavingThrow baseline = KingmakerGunslinger.Acadamae
                    .AcadamaeCastingRuntime.CreateFatigueSavingThrow(unit, 1);
                Rulebook.Trigger(baseline);
                Select(unit, race, trait, assertions, rows);
                RuleSavingThrow guarded = KingmakerGunslinger.Acadamae
                    .AcadamaeCastingRuntime.CreateFatigueSavingThrow(unit, 1);
                Rulebook.Trigger(guarded);
                Check(assertions, rows, "forge-acadamae-production-save",
                    guarded.DifficultyClass == 16 &&
                    guarded.StatValue - baseline.StatValue == 2 &&
                    guarded.Reason.Context != null &&
                    guarded.Reason.Context.AssociatedBlueprint.AssetGuid ==
                        KingmakerGunslinger.Fatigue
                            .CanonicalFatigueApplicationRuntime.FatiguedGuid,
                    "native fatigue descriptor=" +
                        guarded.Reason.Context.SpellDescriptor +
                        ";before=" + baseline.StatValue +
                        ";guarded=" + guarded.StatValue);
                unit.Descriptor.RemoveFact(trait.Marker);
            }
            Select(unit, race, trait, assertions, rows);
            ModifiableValue stat = unit.Stats.SaveFortitude;
            ModifiableValue.Modifier other = stat.AddModifier(4,
                unit.Descriptor.GetFact(race.Race), "runtime-racial-control",
                ModifierDescriptor.Racial);
            try
            {
                stat.UpdateValue();
                int ordinary = Save(unit, source, null, SavingThrowType.Fortitude);
                int matching = Save(unit, source, both, SavingThrowType.Fortitude);
                // The installed native DefaultStackingDescriptors explicitly
                // includes Racial. Preserve independent native modifiers; the
                // trait contributes once even when both predicates match.
                Check(assertions, rows, id + "-native-racial-layering",
                    other.Stacks && matching == ordinary + 2 &&
                        stat.Modifiers.Contains(other) && other.ModValue == 4,
                    "native independent racial +4 preserved; one trait +2; matching=" +
                    matching + ";ordinary=" + ordinary + ";nativeStacks=" + other.Stacks);
            }
            finally
            {
                stat.RemoveModifier(other);
                stat.UpdateValue();
                unit.Descriptor.RemoveFact(trait.Marker);
            }
        }

        private static int Save(UnitEntityData unit, UnitEntityData source,
            BlueprintAbility ability, SavingThrowType save)
        {
            var rule = new RuleSavingThrow(unit, save, 100);
            if (ability == null) Rulebook.Trigger(rule);
            else
            {
                var context = new MechanicsContext(source, source.Descriptor,
                    ability, null, new TargetWrapper(unit));
                rule.Reason = context;
                context.TriggerRule(rule);
            }
            return rule.StatValue;
        }

        private static BlueprintAbility ContextAbility(SpellDescriptor descriptor,
            SpellSchool school, BlueprintAbility parent,
            ICollection<UnityEngine.Object> temporary)
        {
            var ability = ScriptableObject.CreateInstance<BlueprintAbility>();
            var descriptors = ScriptableObject.CreateInstance<SpellDescriptorComponent>();
            var spell = ScriptableObject.CreateInstance<SpellComponent>();
            descriptors.Descriptor = descriptor;
            spell.School = school;
            ability.name = "KMG_Runtime_TraitSave_" + descriptor + "_" + school;
            ability.Type = AbilityType.Spell;
            ability.Parent = parent;
            ability.ComponentsArray = new BlueprintComponent[] { descriptors, spell };
            temporary.Add(descriptors);
            temporary.Add(spell);
            temporary.Add(ability);
            return ability;
        }

        private static void Resistance(UnitEntityData unit, UnitEntityData source,
            ElementalRaceBlueprints race,
            ICollection<RuntimeTestAssertion> assertions, JArray rows)
        {
            ElementalAlternateTraitBlueprints trait = race.AlternateTraits.Require(
                ElementalAlternateTraitId.ThunderousResilience);
            int sonicBefore = Energy(source, unit, DamageEnergyType.Sonic);
            int electricityBefore = Energy(source, unit, DamageEnergyType.Electricity);
            Select(unit, race, trait, assertions, rows);
            Check(assertions, rows, "thunderous-sonic-five",
                sonicBefore == 10 && Energy(source, unit, DamageEnergyType.Sonic) == 5,
                "native sonic damage before=" + sonicBefore);
            Check(assertions, rows, "thunderous-replaces-electricity",
                electricityBefore == 5 &&
                Energy(source, unit, DamageEnergyType.Electricity) == 10,
                "native electricity damage before=" + electricityBefore);
            unit.Descriptor.RemoveFact(trait.Marker);
            Check(assertions, rows, "thunderous-removal",
                Energy(source, unit, DamageEnergyType.Sonic) == 10 &&
                Energy(source, unit, DamageEnergyType.Electricity) == 5,
                "native resistance provider restored");
        }

        private static int Energy(UnitEntityData source, UnitEntityData target,
            DamageEnergyType energy)
        {
            int before = target.Descriptor.Damage;
            try
            {
                var packet = new EnergyDamage(new DiceFormula(0, DiceType.D6),
                    energy) { PreRolledValue = 10 };
                return Rulebook.Trigger(new RuleDealDamage(source, target,
                    new DamageBundle(packet))).ResultDamage.Sum(value =>
                        value.FinalValue);
            }
            finally { target.Descriptor.Damage = before; }
        }

        private static void Brazen(UnitEntityData attacker, UnitEntityData target,
            ElementalRaceBlueprints race, ICollection<ItemEntityWeapon> items,
            ICollection<RuntimeTestAssertion> assertions, JArray rows)
        {
            var melee = new ItemEntityWeapon(Exact<BlueprintItemWeapon>(
                "f28f6031c2908d84d945865a80f67177"));
            var ranged = new ItemEntityWeapon(Exact<BlueprintItemWeapon>(
                "19a5092244dcf99478dcd73c974828b1"));
            items.Add(melee);
            items.Add(ranged);
            attacker.Body.PrimaryHand.InsertItem(melee);
            ElementalAlternateTraitBlueprints trait = race.AlternateTraits.Require(
                ElementalAlternateTraitId.BrazenFlame);
            Select(attacker, race, trait, assertions, rows);
            RuleAttackWithWeapon attack = Hit(attacker, target, melee);
            EnergyDamage[] fire = Fire(attack.MeleeDamage);
            Check(assertions, rows, "brazen-melee-hit",
                fire.Length == 1 && fire[0].PreRolledValue == 1 &&
                attack.MeleeDamage.ResultDamage.Where(value =>
                    ReferenceEquals(value.Source, fire[0])).Sum(value =>
                        value.FinalValue) == 1,
                "fire packets=" + fire.Length);
            Rulebook.Trigger(new RulePrepareDamage(attack.MeleeDamage));
            Check(assertions, rows, "brazen-damage-replay",
                Fire(attack.MeleeDamage).Length == 1,
                "same native damage event retains one fire packet");
            foreach (BlueprintAbility spell in new[] {
                Exact<BlueprintAbility>(ElementalRaceIdentityCatalog.BurningHandsGuid),
                race.SlaAbility })
            {
                var damage = new RuleDealDamage(attacker, target,
                    new DamageBundle(melee, attack.WeaponStats.WeaponSize,
                        new DirectDamage(new DiceFormula(0, DiceType.D6), 1)))
                { AttackRoll = attack.AttackRoll, SourceAbility = spell };
                Rulebook.Trigger(new RulePrepareDamage(damage));
                Check(assertions, rows, "brazen-spell-excluded-" + spell.Type,
                    Fire(damage).Length == 0, "correlated spell/SLA damage adds no fire");
            }
            attacker.Body.PrimaryHand.RemoveItem(false);
            attacker.Body.PrimaryHand.InsertItem(ranged);
            RuleAttackWithWeapon rangedAttack = Hit(attacker, target, ranged);
            var rangedDamage = new RuleDealDamage(attacker, target,
                new DamageBundle(ranged, rangedAttack.WeaponStats.WeaponSize,
                    new DirectDamage(new DiceFormula(0, DiceType.D6), 1)))
            { AttackRoll = rangedAttack.AttackRoll };
            Rulebook.Trigger(new RulePrepareDamage(rangedDamage));
            Check(assertions, rows, "brazen-ranged-excluded",
                !ranged.Blueprint.IsMelee && Fire(rangedDamage).Length == 0,
                "actual native ranged attack correlation adds no fire");
            attacker.Body.PrimaryHand.RemoveItem(false);
            ItemEntityWeapon unarmed = attacker.Body.EmptyHandWeapon;
            RuleAttackWithWeapon unarmedAttack = Hit(attacker, target, unarmed);
            Check(assertions, rows, "brazen-native-unarmed",
                unarmed.Blueprint.IsMelee && unarmed.Blueprint.IsUnarmed &&
                Fire(unarmedAttack.MeleeDamage).Length == 1 &&
                Fire(unarmedAttack.MeleeDamage)[0].PreRolledValue == 1,
                "native unarmed hit adds one fire packet;weapon=" +
                    unarmed.Blueprint.AssetGuid);
            BlueprintItemWeapon naturalBlueprint = BlueprintBootstrap.Library
                .GetAllBlueprints().OfType<BlueprintItemWeapon>()
                .Where(value => value != null && value.IsNatural &&
                    value.IsMelee && !string.IsNullOrEmpty(value.AssetGuid))
                .OrderBy(value => value.AssetGuid, StringComparer.Ordinal).First();
            var natural = new ItemEntityWeapon(naturalBlueprint);
            items.Add(natural);
            attacker.Body.PrimaryHand.InsertItem(natural);
            RuleAttackWithWeapon naturalAttack = Hit(attacker, target, natural);
            Check(assertions, rows, "brazen-native-natural",
                Fire(naturalAttack.MeleeDamage).Length == 1 &&
                Fire(naturalAttack.MeleeDamage)[0].PreRolledValue == 1,
                "native natural hit adds one fire packet;weapon=" +
                    naturalBlueprint.AssetGuid);
            attacker.Descriptor.RemoveFact(trait.Marker);
            attacker.Body.PrimaryHand.RemoveItem(false);
            attacker.Body.PrimaryHand.InsertItem(melee);
            Check(assertions, rows, "brazen-remove",
                Fire(Hit(attacker, target, melee).MeleeDamage).Length == 0,
                "removing trait removes melee fire provider");
            ElementalIfritFeatScenario.ExerciseBrazenNonstacking(attacker,
                target, trait.Marker, items, assertions, rows);
        }

        private static RuleAttackWithWeapon Hit(UnitEntityData attacker,
            UnitEntityData target, ItemEntityWeapon weapon)
        {
            int before = target.Descriptor.Damage;
            try
            {
                RuleAttackWithWeapon attack = Rulebook.Trigger(
                    new RuleAttackWithWeapon(attacker, target, weapon, 0)
                        { AutoHit = true });
                if (attack.AttackRoll == null || !attack.AttackRoll.IsHit)
                    throw new InvalidOperationException("Native attack did not hit.");
                return attack;
            }
            finally { target.Descriptor.Damage = before; }
        }

        private static EnergyDamage[] Fire(RuleDealDamage damage)
        {
            return damage == null ? new EnergyDamage[0] :
                damage.DamageBundle.OfType<EnergyDamage>().Where(value =>
                    value.EnergyType == DamageEnergyType.Fire).ToArray();
        }

        private static void Select(UnitEntityData unit, ElementalRaceBlueprints race,
            ElementalAlternateTraitBlueprints trait,
            ICollection<RuntimeTestAssertion> assertions, JArray rows)
        {
            if (!unit.Descriptor.HasFact(trait.Marker))
                unit.Descriptor.AddFact(trait.Marker);
            bool resistance = !trait.Definition.Replaces(
                ElementalRacialTraitSlot.EnergyResistance);
            bool sla = !trait.Definition.Replaces(
                ElementalRacialTraitSlot.RacialSpellLikeAbility);
            Check(assertions, rows, trait.Definition.Id + "-replacement",
                unit.Descriptor.HasFact(trait.Provider) &&
                unit.Descriptor.HasFact(race.Resistance) == resistance &&
                unit.Descriptor.HasFact(race.SlaFeature) == sla &&
                (unit.Descriptor.Abilities.GetAbility(race.SlaAbility) != null) == sla,
                "exact own provider present; resistance=" + resistance + ";sla=" + sla);
        }

        private static UnitEntityData Create(ElementalRaceBlueprints race,
            ICollection<UnitEntityData> units, ICollection<UnityEngine.Object> temporary)
        {
            BlueprintUnit blueprint = UnityEngine.Object.Instantiate(
                BlueprintRoot.Instance.DefaultPlayerCharacter);
            blueprint.name = "KMG_Runtime_TraitPassive_" + units.Count;
            if (race != null) blueprint.Race = race.Race;
            blueprint.Brain = null;
            blueprint.IsCheater = false;
            temporary.Add(blueprint);
            UnitEntityData unit = new Kingmaker.UI.LevelUp.ChargenUnit(blueprint).Unit;
            if (unit == null || unit.Descriptor == null ||
                (race != null && !ReferenceEquals(unit.Descriptor.Progression.Race,
                    race.Race))) throw new InvalidOperationException(
                        "Native trait fixture race could not be established.");
            unit.Stats.HitPoints.BaseValue = 500;
            unit.Descriptor.State.Immortality.Retain();
            Game.Instance.State.Units.All.Add(unit);
            units.Add(unit);
            return unit;
        }

        private static ElementalRaceBlueprints RaceFor(ElementalRaceBlueprintSet set,
            ElementalAlternateTraitId id)
        {
            return set.OrderedBlueprints().Single(race =>
                race.AlternateTraits.Traits().Any(value => value.Definition.Id == id));
        }

        private static T Exact<T>(string guid) where T : BlueprintScriptableObject
        {
            return BlueprintLibraryLookup.RequireExact<T>(BlueprintBootstrap.Library,
                guid, "native passive-trait fixture donor");
        }

        private static void Check(ICollection<RuntimeTestAssertion> assertions,
            JArray rows, string name, bool pass, string observed)
        {
            assertions.Add(new RuntimeTestAssertion
            {
                Name = "elemental-trait-" + name,
                Expected = "exact native trait contract",
                Observed = observed,
                Status = pass ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                Evidence = "live request-local native rule/stat/provider state"
            });
            rows.Add(new JObject { { "name", name }, { "pass", pass },
                { "observed", observed } });
        }
    }
}
