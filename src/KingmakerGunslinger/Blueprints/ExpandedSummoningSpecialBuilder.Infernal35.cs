using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningSpecialBuilder
    {
        internal const string Barbed35Prefix =
            "KMG.Summoning.Infernal.BarbedDevil.";
        internal const string Bone35Prefix =
            "KMG.Summoning.Infernal.BoneDevil.";
        private const string NativeFrightened35 =
            "f08a7239aa961f34c8301518e71d4cdf";
        private const string NativeInvisibility35 =
            "89940cde01689fb46946b2f8cd7b66b7";
        private const string NativeHoldPerson35 =
            "c7104f7526c4c524f91474614054547e";
        private const string NativeSpiderPoisonBuff35 =
            "56ec8788092b6314e8f3c1c502e8433f";
        private const string NativeSpiderPoisonFeature35 =
            "094714bb08f4e1943a8e9d2384ebe573";
        private const string ImprovedCriticalClaw35 =
            "76a335b7d69691c4e8376f9379338778";

        private static void ConfigureInfernal35(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> symbols)
        {
            InfernalRules35.Validate();
            ConfigureBarbed35(library, symbols);
            ConfigureBone35(library, symbols);
            ConfigureWallPrototype35(library, symbols);
            foreach (var pair in symbols.Where(pair =>
                pair.Value is BlueprintUnitFact &&
                (pair.Key.StartsWith(Barbed35Prefix, StringComparison.Ordinal) ||
                 pair.Key.StartsWith(Bone35Prefix, StringComparison.Ordinal))))
            {
                var fact = (BlueprintUnitFact)pair.Value;
                if (fact.name != InternalName(pair.Key)) continue;
                NamePrivateSummonFactComponents(fact, pair.Key, "Sprint35");
            }
        }

        private static BlueprintUnit InfernalProfile35(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> symbols,
            string token, string creatureKey, string prefix, string title,
            int hitDice, int hitPoints, int fortitude, int reflex, int will,
            int perception, int stealth, int mobility)
        {
            var unit = Require<BlueprintUnit>(symbols,
                "KMG.Summoning.Unit." + token);
            var type = Require<BlueprintUnitType>(symbols, prefix + "UnitType");
            type.name = InternalName(prefix + "UnitType");
            type.Name = LocalizationService.Create(prefix + "UnitType.Name",
                title);
            type.Description = LocalizationService.Create(
                prefix + "UnitType.Description",
                title + ", a lawful evil summoned outsider.");
            type.Image = ExpandedSummoningProjectIcons.Require(creatureKey);
            type.KnowledgeStat = StatType.SkillLoreReligion;
            type.SignatureAbilities = Array.Empty<BlueprintUnitFact>();
            var levels = OutsiderLevels(library, hitDice);
            levels.Skills = Array.Empty<StatType>();
            var racial = ScriptableObject.CreateInstance<
                SummonGiantRacialProfile>();
            racial.OwningBlueprint = unit;
            racial.BaseRacialHitPoints = hitPoints;
            racial.BaseFortitude = fortitude;
            racial.BaseReflex = reflex;
            racial.BaseWill = will;
            racial.PerceptionRanks = perception;
            racial.StealthRanks = stealth;
            racial.MobilityRanks = mobility;
            var extraplanar = ScriptableObject.CreateInstance<
                OutsiderExtraplanarIdentity>();
            extraplanar.OwningBlueprint = unit;
            extraplanar.NativeMarker = Feature(library,
                ExtraplanarSubtypeGuid, "native extraplanar identity");
            unit.ComponentsArray = new BlueprintComponent[] {
                levels, racial, extraplanar };
            unit.Skills = new BlueprintUnit.UnitSkills();
            unit.Race = null;
            unit.Type = type;
            var donor = BlueprintLibraryLookup.RequireExact<BlueprintUnit>(
                library, TheropodVisualPolicy.DonorGuid,
                "Sprint 35 measured original-body animation carrier");
            if (donor.Prefab?.AssetId != TheropodVisualPolicy.DonorPrefabGuid)
                throw new InvalidOperationException(
                    "Sprint 35 infernal original-body rig changed.");
            unit.Prefab = donor.Prefab;
            ConfigureOutsiderOriginalExpiryVisual(library, unit);
            return unit;
        }

        private static List<BlueprintComponent> InfernalDefenses35(
            BlueprintUnit unit, int naturalArmor, int spellResistance)
        {
            var dr = ScriptableObject.CreateInstance<
                AddDamageResistancePhysical>();
            dr.Value = Simple(10);
            dr.Or = false;
            dr.BypassedByAlignment = true;
            dr.Alignment = DamageAlignment.Good;
            dr.BypassedByMaterial = false;
            var armor = ScriptableObject.CreateInstance<AddStatBonus>();
            armor.Stat = StatType.AC;
            armor.Value = naturalArmor;
            armor.Descriptor = ModifierDescriptor.NaturalArmor;
            var sr = ScriptableObject.CreateInstance<AddSpellResistance>();
            sr.Value = Simple(spellResistance);
            sr.AddCR = false;
            var poison = ScriptableObject.CreateInstance<
                BuffDescriptorImmunity>();
            poison.CheckFact = false;
            poison.Descriptor = SpellDescriptor.Poison;
            var poisonSpells = ScriptableObject.CreateInstance<
                SpellImmunityToSpellDescriptor>();
            poisonSpells.Descriptor = SpellDescriptor.Poison;
            var lawful = ScriptableObject.CreateInstance<
                AddOutgoingPhysicalDamageProperty>();
            lawful.AffectAnyPhysicalDamage = true;
            lawful.AddAlignment = true;
            lawful.Alignment = DamageAlignment.Lawful;
            var evil = ScriptableObject.CreateInstance<
                AddOutgoingPhysicalDamageProperty>();
            evil.AffectAnyPhysicalDamage = true;
            evil.AddAlignment = true;
            evil.Alignment = DamageAlignment.Evil;
            var alertness = ScriptableObject.CreateInstance<AddStatBonus>();
            alertness.Stat = StatType.SkillPerception;
            alertness.Value = 2;
            alertness.Descriptor = ModifierDescriptor.UntypedStackable;
            var lifetime = ScriptableObject.CreateInstance<
                InfernalSourceLifetime34>();
            lifetime.OwningBlueprint = unit;
            var commands = ScriptableObject.CreateInstance<
                OutsiderLifetimeBoundary>();
            commands.OwningBlueprint = unit;
            return new List<BlueprintComponent> { dr, armor, sr, poison,
                poisonSpells, lawful, evil, alertness,
                Energy(DamageEnergyType.Acid, 10),
                Energy(DamageEnergyType.Cold, 10), lifetime, commands };
        }

        private static void BindInfernalTraits35(BlueprintFeature traits,
            string prefix, string title, string description,
            IEnumerable<BlueprintComponent> components)
        {
            traits.name = InternalName(prefix + "Traits");
            traits.HideInUI = true;
            traits.IsClassFeature = false;
            traits.ComponentsArray = components.ToArray();
            BlueprintUnitFactAccess.Resolve().Configure(traits,
                LocalizationService.Create(prefix + "Traits.Name",
                    title + " Traits"),
                LocalizationService.Create(prefix + "Traits.Description",
                    description), null);
        }

        private static BlueprintUnitFact[] InfernalFacts35(
            LibraryScriptableObject library, BlueprintFeature traits)
        {
            return new BlueprintUnitFact[] {
                Feature(library, OutsiderTypeGuid, "outsider type"),
                Feature(library, EvilSubtypeGuid, "evil subtype"),
                Feature(library, LawfulSubtypeGuid, "lawful subtype"),
                Feature(library, ExtraplanarSubtypeGuid,
                    "native extraplanar spell consumer"),
                Feature(library, FireImmunityGuid, "devil fire immunity"),
                traits };
        }

        private static BlueprintBuff InfernalFrightened35(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> symbols,
            string symbol, string iconKey)
        {
            var native = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(
                library, NativeFrightened35, "native frightened state");
            var buff = Require<BlueprintBuff>(symbols, symbol);
            CopyFields(native, buff);
            buff.name = InternalName(symbol);
            buff.Stacking = StackingType.Stack;
            buff.ComponentsArray = (native.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Select(
                    ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            buff.ResourceAssetIds = Array.Empty<string>();
            BlueprintUnitFactAccess.Resolve().Configure(buff,
                LocalizationService.Create(symbol + ".Name", "Frightened"),
                LocalizationService.Create(symbol + ".Description",
                    "Fleeing in supernatural fear."),
                ExpandedSummoningProjectIcons.Require(iconKey));
            return buff;
        }

        private static void ConfigureBarbed35(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> symbols)
        {
            string p = Barbed35Prefix;
            var unit = InfernalProfile35(library, symbols, "BarbedDevil",
                "barbed-devil", p, "Barbed Devil (Hamatula)", 12, 138,
                8, 8, 4, 15, 8, 8);
            ConfigureUnitCore(unit, "BarbedDevil",
                "Barbed Devil (Hamatula)", Alignment.LawfulEvil,
                Size.Medium, 23, 23, 22, 12, 15, 18, 30);
            var claw = Sprint29Weapon(library, symbols, p + "Claw",
                p + "ClawType", NativeClaw1d4Guid, Size.Medium, 2,
                DiceType.D8, 9);
            if (claw.Type.CriticalRollEdge !=
                InfernalRules35.BarbedBaseCriticalThreatMinimum)
                throw new InvalidOperationException(
                    "Barbed claw donor no longer has the expected base critical range.");
            unit.Body = NaturalBody(claw, new[] { claw },
                Array.Empty<BlueprintItemWeapon>());
            var profile = ScriptableObject.CreateInstance<
                InfernalMartialProfile35>();
            profile.OwningBlueprint = unit;
            profile.Claw = claw;
            unit.ComponentsArray = unit.ComponentsArray.Concat(
                new BlueprintComponent[] { profile }).ToArray();
            var frightened = InfernalFrightened35(library, symbols,
                p + "FearFrightened", "barbed-devil");
            var parts = InfernalDefenses35(unit, 10, 22);
            var primaryStrength = ScriptableObject.CreateInstance<
                DeinonychusPrimaryTalonStrength>();
            primaryStrength.OwningBlueprint = unit;
            primaryStrength.Talon = claw;
            var equipment = ScriptableObject.CreateInstance<
                SummonGiantEquipmentOwnership>();
            equipment.OwningBlueprint = unit;
            equipment.GreatWeapon = claw;
            var defense = ScriptableObject.CreateInstance<BarbedDefense35>();
            defense.OwningBlueprint = unit;
            var fear = ScriptableObject.CreateInstance<BarbedClawFear35>();
            fear.OwningBlueprint = unit;
            fear.Claw = claw;
            fear.Frightened = frightened;
            var grab = ScriptableObject.CreateInstance<SummonGrabComponent>();
            grab.GrabWithPrimaryHand = true;
            grab.GrabAdditionalLimbCount = 1;
            grab.MaxTargetSizeDelta = 0;
            grab.MaxHeldTargets = 1;
            grab.HoldBuff = Require<BlueprintBuff>(symbols,
                GrappleHoldSymbol);
            grab.GrappledBuff = Require<BlueprintBuff>(symbols,
                GrappleGrappledSymbol);
            grab.ImpaleProfileOwner = unit;
            var maneuver = ScriptableObject.CreateInstance<ManeuverBonus>();
            maneuver.Type = CombatManeuver.Grapple;
            maneuver.Bonus = ExpandedSummoningSpecialProfiles
                .SummonGrabManeuverBonus;
            parts.AddRange(new BlueprintComponent[] { primaryStrength,
                equipment, defense, fear, grab, maneuver });
            var hold = ConfigureOwnedSpell35(library, symbols, p +
                "HoldPerson", NativeHoldPerson35, "Hold Person", 3,
                UnitCommand.CommandType.Standard);
            var scorching = ConfigureBarbedScorchingRay35(library, symbols);
            var parameters = ScriptableObject.CreateInstance<
                Sprint29GiantSpellLikeParameters>();
            parameters.OwningBlueprint = unit;
            parameters.Abilities = new[] { hold, scorching };
            parameters.SpellLevels = new[] { 3, 2 };
            parameters.CasterLevel = 12;
            parts.Add(parameters);
            var traits = Require<BlueprintFeature>(symbols, p + "Traits");
            BindInfernalTraits35(traits, p, "Hamatula",
                "DR10/good; fire and poison immunity; acid/cold resistance 10; SR22. Two primary claws threaten 19-20. Wounding claws cause fear; incoming non-reach melee hits trigger Barbed Defense; claws grab up to Medium and a later successful maintain check impales.",
                parts);
            unit.AddFacts = InfernalFacts35(library, traits).Concat(
                new BlueprintUnitFact[] {
                    Require<BlueprintFeature>(symbols, GiantCleaveFeature),
                    Feature(library, CombatReflexesGuid, "Combat Reflexes"),
                    Feature(library, ImprovedCriticalClaw35,
                        "Improved Critical (claw)"),
                    Feature(library, IronWillGuid, "Iron Will"),
                    Feature(library, PowerAttackGuid, "Power Attack"),
                    hold, scorching }).ToArray();
            var holdAi = ConfigureInfernalAi35(symbols, p + "HoldAi", unit,
                hold, false, true, 4);
            var rayAi = ConfigureInfernalAi35(symbols,
                p + "ScorchingRayAi", unit, scorching, false, true, 5);
            unit.Brain = InfernalBrain35(library, symbols, p, holdAi, rayAi);
        }

        private static BlueprintAbility ConfigureBarbedScorchingRay35(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> symbols)
        {
            string symbol = Barbed35Prefix + "ScorchingRay";
            var ability = ConfigureOwnedSpell35(library, symbols, symbol,
                ScorchingRayGuid, "Scorching Ray (Two Rays)", 2,
                UnitCommand.CommandType.Standard,
                "Spell-like ability at caster level 12. This hamatula's listed version fires exactly two rays.");
            var delivery = ability.ComponentsArray.OfType<
                AbilityDeliverProjectile>().Single();
            if (delivery.Projectiles == null || delivery.Projectiles.Length < 2)
                throw new InvalidOperationException(
                    "Native Scorching Ray no longer exposes two projectiles.");
            delivery.Projectiles = delivery.Projectiles.Take(2).ToArray();
            delivery.UseMaxProjectilesCount = false;
            return ability;
        }

        private static void ConfigureBone35(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> symbols)
        {
            string p = Bone35Prefix;
            var unit = InfernalProfile35(library, symbols, "BoneDevil",
                "bone-devil", p, "Bone Devil (Osyluth)", 10, 105, 7, 7,
                3, 13, 10, 10);
            ConfigureUnitCore(unit, "BoneDevil", "Bone Devil (Osyluth)",
                Alignment.LawfulEvil, Size.Large, 21, 21, 20, 16, 15, 18,
                60);
            var bite = Sprint29Weapon(library, symbols, p + "Bite",
                p + "BiteType", NativeBite1d6Guid, Size.Large, 1,
                DiceType.D8, 9);
            var claw = Sprint29Weapon(library, symbols, p + "Claw",
                p + "ClawType", NativeClaw1d4Guid, Size.Large, 1,
                DiceType.D6, 9);
            var sting = Sprint29Weapon(library, symbols, p + "Sting",
                p + "StingType", LargeTailGuid, Size.Large, 3,
                DiceType.D4, 9);
            SetField(sting.Type, "m_TypeNameText",
                LocalizationService.Create(p + "StingType.Name", "Sting"));
            unit.Body = NaturalBody(bite, new[] { claw, claw, sting },
                Array.Empty<BlueprintItemWeapon>());
            var profile = ScriptableObject.CreateInstance<
                InfernalMartialProfile35>();
            profile.OwningBlueprint = unit;
            profile.Bite = bite;
            profile.Claw = claw;
            profile.Sting = sting;
            unit.ComponentsArray = unit.ComponentsArray.Concat(
                new BlueprintComponent[] { profile }).ToArray();
            var parts = InfernalDefenses35(unit, 11, 20);
            var primaryStrength = ScriptableObject.CreateInstance<
                DeinonychusPrimaryTalonStrength>();
            primaryStrength.OwningBlueprint = unit;
            primaryStrength.Talon = bite;
            var equipment = ScriptableObject.CreateInstance<
                SummonGiantEquipmentOwnership>();
            equipment.OwningBlueprint = unit;
            equipment.GreatWeapon = bite;
            equipment.Slam = claw;
            equipment.Rock = sting;
            parts.AddRange(new BlueprintComponent[] { primaryStrength,
                equipment });
            ConfigureBonePoison35(library, symbols, parts, sting);
            var frightened = InfernalFrightened35(library, symbols,
                p + "FearFrightened", "bone-devil");
            var memory = Require<BlueprintBuff>(symbols,
                p + "FearImmunity");
            memory.name = InternalName(p + "FearImmunity");
            memory.Stacking = StackingType.Stack;
            SetBuffFlags(memory, false, true);
            memory.ComponentsArray = Array.Empty<BlueprintComponent>();
            BlueprintUnitFactAccess.Resolve().Configure(memory,
                LocalizationService.Create(p + "FearImmunity.Name",
                    "Osyluth Fear Memory"),
                LocalizationService.Create(p + "FearImmunity.Description",
                    "Source-specific repeat protection for one bone devil's fear aura."),
                null);
            var area = Require<BlueprintAbilityAreaEffect>(symbols,
                p + "FearArea");
            area.name = InternalName(p + "FearArea");
            area.Shape = AreaEffectShape.Cylinder;
            area.Size = new Feet(5);
            area.AffectEnemies = true;
            area.AffectDead = false;
            area.AggroEnemies = false;
            area.IgnoreSleepingUnits = false;
            area.SpellResistance = false;
            area.Fx = new Kingmaker.ResourceLinks.PrefabLink();
            var aura = ScriptableObject.CreateInstance<BoneFearAura35>();
            aura.OwningBlueprint = unit;
            aura.Frightened = frightened;
            aura.SourceMemory = memory;
            area.ComponentsArray = new BlueprintComponent[] { aura };
            var carrier = Require<BlueprintBuff>(symbols,
                p + "FearCarrier");
            carrier.name = InternalName(p + "FearCarrier");
            carrier.Stacking = StackingType.Replace;
            SetBuffFlags(carrier, false, true);
            var emanation = ScriptableObject.CreateInstance<AddAreaEffect>();
            emanation.AreaEffect = area;
            carrier.ComponentsArray = new BlueprintComponent[] { emanation };
            BlueprintUnitFactAccess.Resolve().Configure(carrier,
                LocalizationService.Create(p + "FearCarrier.Name",
                    "Fear Aura"),
                LocalizationService.Create(p + "FearCarrier.Description",
                    "Creatures entering within 5 feet save against this bone devil's fear aura (Will DC 19)."),
                ExpandedSummoningProjectIcons.Require("bone-devil"));
            var invisibility = ConfigureOwnedSpell35(library, symbols,
                p + "Invisibility", NativeInvisibility35, "Invisibility",
                2, UnitCommand.CommandType.Standard);
            invisibility.Range = AbilityRange.Personal;
            invisibility.CanTargetEnemies = false;
            invisibility.CanTargetFriends = false;
            invisibility.CanTargetSelf = true;
            var quick = ConfigureOwnedSpell35(library, symbols,
                p + "QuickenedInvisibility", NativeInvisibility35,
                "Quickened Invisibility", 2, UnitCommand.CommandType.Swift);
            quick.Range = AbilityRange.Personal;
            quick.CanTargetEnemies = false;
            quick.CanTargetFriends = false;
            quick.CanTargetSelf = true;
            var quickResource = Require<BlueprintAbilityResource>(symbols,
                p + "QuickenedInvisibilityResource");
            ConfigureNamedResource(quickResource,
                p + "QuickenedInvisibilityResource",
                p + "QuickenedInvisibilityResource", "Quickened Invisibility",
                "Three uses for this summoned Bone Devil.", 3);
            var quickCost = ScriptableObject.CreateInstance<
                AbilityResourceLogic>();
            quickCost.RequiredResource = quickResource;
            quickCost.IsSpendResource = true;
            quickCost.Amount = 1;
            quick.ComponentsArray = quick.ComponentsArray.Concat(
                new BlueprintComponent[] { quickCost }).ToArray();
            parts.Add(AddResource(quickResource));
            var wall = ConfigureWallAbility35(symbols);
            var parameters = ScriptableObject.CreateInstance<
                Sprint29GiantSpellLikeParameters>();
            parameters.OwningBlueprint = unit;
            parameters.Abilities = new[] { invisibility, quick, wall };
            parameters.SpellLevels = new[] { 2, 2, 4 };
            parameters.CasterLevel = 12;
            parts.Add(parameters);
            var traits = Require<BlueprintFeature>(symbols, p + "Traits");
            BindInfernalTraits35(traits, p, "Osyluth",
                "DR10/good; fire and poison immunity; acid/cold resistance 10; SR20. Bite, two claws, and sting are primary attacks. A wounding sting delivers finite Strength poison. Its 5-foot fear aura and ordinary attack-breaking invisibility retain separate native consumers.",
                parts);
            unit.AddFacts = InfernalFacts35(library, traits).Concat(
                new BlueprintUnitFact[] {
                    Feature(library, AirborneGuid,
                        "accepted ordinary-map flight representation"),
                    Feature(library, CombatReflexesGuid, "Combat Reflexes"),
                    Feature(library, ImprovedInitiativeGuid,
                        "Improved Initiative"),
                    Feature(library, IronWillGuid, "Iron Will"), carrier,
                    invisibility, quick, wall }).ToArray();
            var invisAi = ConfigureInfernalAi35(symbols,
                p + "InvisibilityAi", unit, invisibility, true, false, 2);
            var quickAi = ConfigureInfernalAi35(symbols,
                p + "QuickenedInvisibilityAi", unit, quick, true, false, 6);
            unit.Brain = InfernalBrain35(library, symbols, p, quickAi, invisAi);
        }

        private static void ConfigureBonePoison35(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> symbols,
            List<BlueprintComponent> traits, BlueprintItemWeapon sting)
        {
            string p = Bone35Prefix;
            var nativeBuff = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(
                library, NativeSpiderPoisonBuff35,
                "native saved injury poison lifecycle");
            var poison = Require<BlueprintBuff>(symbols, p + "Poison");
            CopyFields(nativeBuff, poison);
            poison.name = InternalName(p + "Poison");
            poison.ComponentsArray = (nativeBuff.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Select(
                    ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            poison.Stacking = StackingType.Poison;
            var damage = poison.ComponentsArray.OfType<
                BuffPoisonStatDamage>().Single();
            damage.Stat = StatType.Strength;
            damage.Value = new DiceFormula(1, DiceType.D3);
            damage.Bonus = 0;
            damage.Ticks = 6;
            damage.SuccesfullSaves = 2;
            damage.SaveType = SavingThrowType.Fortitude;
            BlueprintUnitFactAccess.Resolve().Configure(poison,
                LocalizationService.Create(p + "Poison.Name",
                    "Bone Devil Venom"),
                LocalizationService.Create(p + "Poison.Description",
                    "Injury poison: Fortitude DC 20; 1d3 Strength damage once per round for six exposures; two consecutive successful saves cure it."),
                nativeBuff.Icon);
            var nativeFeature = BlueprintLibraryLookup.RequireExact<
                BlueprintFeature>(library, NativeSpiderPoisonFeature35,
                "native poison-on-hit feature");
            var components = (nativeFeature.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Select(
                    ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            var trigger = components.OfType<
                AddInitiatorAttackWithWeaponTrigger>().Single();
            trigger.WeaponType = sting.Type;
            trigger.OnlyHit = true;
            var save = trigger.Action.Actions.OfType<
                ContextActionSavingThrow>().Single();
            save.Type = SavingThrowType.Fortitude;
            var outcome = save.Actions.Actions.OfType<
                ContextActionConditionalSaved>().Single();
            var apply = outcome.Failed.Actions.OfType<
                ContextActionApplyBuff>().Single();
            apply.Buff = poison;
            trigger.Action.Actions = new GameAction[] {
                new ContextActionOnlyIfWeaponWounded {
                    Actions = new ActionList { Actions =
                        (new GameAction[] {
                            new ContextActionSetBonePoisonDc35() })
                        .Concat(trigger.Action.Actions).ToArray() } } };
            traits.AddRange(components);
        }

        private static BlueprintAbility ConfigureOwnedSpell35(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> symbols,
            string symbol, string nativeGuid, string title, int spellLevel,
            UnitCommand.CommandType action, string description = null)
        {
            var ability = Require<BlueprintAbility>(symbols, symbol);
            CloneMephitSpell(library, ability, nativeGuid,
                "native " + title + " spell graph");
            ability.name = InternalName(symbol);
            ability.Parent = null;
            ability.Type = AbilityType.SpellLike;
            ability.ActionType = action;
            ability.SetIsFullRoundAction(false);
            ability.MaterialComponent = new BlueprintAbility.MaterialComponentData();
            ability.ResourceAssetIds = Array.Empty<string>();
            var parameters = ScriptableObject.CreateInstance<
                ContextCalculateAbilityParams>();
            parameters.StatType = StatType.Charisma;
            parameters.ReplaceCasterLevel = true;
            parameters.CasterLevel = Simple(12);
            parameters.ReplaceSpellLevel = true;
            parameters.SpellLevel = Simple(spellLevel);
            ability.ComponentsArray = ability.ComponentsArray.Concat(
                new BlueprintComponent[] { parameters }).ToArray();
            BlueprintUnitFactAccess.Resolve().Configure(ability,
                LocalizationService.Create(symbol + ".Name", title),
                LocalizationService.Create(symbol + ".Description",
                    description ?? title +
                        " as a spell-like ability at caster level 12."),
                ability.Icon);
            return ability;
        }

        private static InfernalSpellAi35 ConfigureInfernalAi35(
            IDictionary<string, BlueprintScriptableObject> symbols,
            string symbol, BlueprintUnit owner, BlueprintAbility ability,
            bool self, bool enemy, int score)
        {
            var ai = Require<InfernalSpellAi35>(symbols, symbol);
            ConfigureMephitAiAction(ai, symbol, ability, 0);
            ai.OwningBlueprint = owner;
            ai.TargetSelf = self;
            ai.TargetEnemy = enemy;
            ai.BaseScore = score;
            return ai;
        }

        private static BlueprintBrain InfernalBrain35(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> symbols,
            string prefix, params BlueprintAiAction[] actions)
        {
            var donor = BlueprintLibraryLookup.RequireExact<BlueprintBrain>(
                library, DumbBrainGuid, "native ordinary attack brain");
            var brain = Require<BlueprintBrain>(symbols, prefix + "Brain");
            CopyFields(donor, brain);
            brain.name = InternalName(prefix + "Brain");
            brain.ComponentsArray = (donor.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Select(
                    ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            brain.Actions = actions.Concat(donor.Actions ??
                Array.Empty<BlueprintAiAction>()).ToArray();
            return brain;
        }

        private static BlueprintAbility ConfigureWallAbility35(
            IDictionary<string, BlueprintScriptableObject> symbols)
        {
            string symbol = Bone35Prefix + "WallOfIce";
            var ability = Require<BlueprintAbility>(symbols, symbol);
            ability.name = InternalName(symbol);
            ability.Type = AbilityType.SpellLike;
            ability.Parent = null;
            ability.ActionType = UnitCommand.CommandType.Standard;
            ability.SetIsFullRoundAction(false);
            ability.Range = AbilityRange.Long;
            ability.CanTargetEnemies = false;
            ability.CanTargetFriends = false;
            ability.CanTargetSelf = false;
            ability.CanTargetPoint = true;
            ability.SpellResistance = true;
            ability.NeedEquipWeapons = false;
            ability.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            ability.EffectOnAlly = AbilityEffectOnUnit.Harmful;
            ability.MaterialComponent = new BlueprintAbility.MaterialComponentData();
            ability.ResourceAssetIds = Array.Empty<string>();
            var spawn = ScriptableObject.CreateInstance<
                ContextActionSpawnMonster>();
            spawn.Blueprint = Require<BlueprintUnit>(symbols,
                Bone35Prefix + "WallSectionUnit");
            spawn.CountValue = new ContextDiceValue {
                DiceType = DiceType.Zero, DiceCountValue = Simple(0),
                BonusValue = Simple(1) };
            spawn.LevelValue = Simple(0);
            spawn.DurationValue = new ContextDurationValue {
                Rate = DurationRate.Minutes, DiceType = DiceType.Zero,
                DiceCountValue = Simple(0), BonusValue = Simple(12) };
            spawn.SummonPool = null;
            spawn.DoNotLinkToCaster = false;
            spawn.IsDirectlyControllable = false;
            spawn.AfterSpawn = new ActionList {
                Actions = Array.Empty<GameAction>() };
            var run = ScriptableObject.CreateInstance<AbilityEffectRunAction>();
            run.SavingThrowType = SavingThrowType.Reflex;
            run.Actions = new ActionList {
                Actions = new GameAction[] { spawn } };
            var parameters = ScriptableObject.CreateInstance<
                ContextCalculateAbilityParams>();
            parameters.StatType = StatType.Charisma;
            parameters.ReplaceCasterLevel = true;
            parameters.CasterLevel = Simple(12);
            parameters.ReplaceSpellLevel = true;
            parameters.SpellLevel = Simple(4);
            var cold = ScriptableObject.CreateInstance<
                SpellDescriptorComponent>();
            cold.Descriptor = SpellDescriptor.Cold;
            ability.ComponentsArray = new BlueprintComponent[] {
                run, parameters, cold };
            BlueprintUnitFactAccess.Resolve().Configure(ability,
                LocalizationService.Create(symbol + ".Name",
                    "Wall of Ice (Section Viability Candidate)"),
                LocalizationService.Create(symbol + ".Description",
                    "Hidden Sprint 35 feasibility candidate: forms one temporary attackable section for native obstruction/removal measurement. It is not a published substitute for the complete Wall of Ice."),
                ExpandedSummoningProjectIcons.Require("bone-devil"));
            return ability;
        }

        private static void ConfigureWallPrototype35(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> symbols)
        {
            string p = Bone35Prefix;
            var unit = Require<BlueprintUnit>(symbols, p + "WallSectionUnit");
            var type = Require<BlueprintUnitType>(symbols,
                p + "WallSectionUnitType");
            type.name = InternalName(p + "WallSectionUnitType");
            type.Name = LocalizationService.Create(
                p + "WallSectionUnitType.Name", "Wall of Ice Section");
            type.Description = LocalizationService.Create(
                p + "WallSectionUnitType.Description",
                "A private temporary section used to prove native movement obstruction and restored navigation.");
            type.Image = ExpandedSummoningProjectIcons.Require("bone-devil");
            type.KnowledgeStat = StatType.SkillKnowledgeArcana;
            type.SignatureAbilities = Array.Empty<BlueprintUnitFact>();
            var levels = OutsiderLevels(library, 1);
            levels.Skills = Array.Empty<StatType>();
            var racial = ScriptableObject.CreateInstance<
                SummonGiantRacialProfile>();
            racial.OwningBlueprint = unit;
            racial.BaseRacialHitPoints = 36;
            racial.BaseFortitude = 0;
            racial.BaseReflex = 0;
            racial.BaseWill = 0;
            unit.ComponentsArray = new BlueprintComponent[] { levels, racial };
            unit.Skills = new BlueprintUnit.UnitSkills();
            unit.Race = null;
            unit.Type = type;
            var donor = BlueprintLibraryLookup.RequireExact<BlueprintUnit>(
                library, TheropodVisualPolicy.DonorGuid,
                "temporary wall section standing-corpulence carrier");
            unit.Prefab = donor.Prefab;
            ConfigureUnitCore(unit, "BoneDevilWallSection",
                "Wall of Ice Section", Alignment.TrueNeutral, Size.Large,
                10, 1, 10, 0, 10, 1, 0);
            unit.Body = NaturalBody(null, Array.Empty<BlueprintItemWeapon>(),
                Array.Empty<BlueprintItemWeapon>());
            var immobile = ScriptableObject.CreateInstance<AddCondition>();
            immobile.Condition = UnitCondition.CantAct;
            var traits = Require<BlueprintFeature>(symbols,
                p + "WallSectionTraits");
            traits.name = InternalName(p + "WallSectionTraits");
            traits.HideInUI = true;
            traits.IsClassFeature = false;
            traits.ComponentsArray = new BlueprintComponent[] { immobile };
            BlueprintUnitFactAccess.Resolve().Configure(traits,
                LocalizationService.Create(p + "WallSectionTraits.Name",
                    "Ice Section"),
                LocalizationService.Create(p + "WallSectionTraits.Description",
                    "Stationary, 36 hit points, and linked to the casting Bone Devil's temporary summon duration."), null);
            unit.AddFacts = new BlueprintUnitFact[] { traits };
            var brainDonor = BlueprintLibraryLookup.RequireExact<BlueprintBrain>(
                library, DumbBrainGuid, "inert wall section brain donor");
            var brain = Require<BlueprintBrain>(symbols,
                p + "WallSectionBrain");
            CopyFields(brainDonor, brain);
            brain.name = InternalName(p + "WallSectionBrain");
            brain.Actions = Array.Empty<BlueprintAiAction>();
            brain.ComponentsArray = (brainDonor.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Select(
                    ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            unit.Brain = brain;
            var residual = Require<BlueprintAbilityAreaEffect>(symbols,
                p + "WallResidualArea");
            residual.name = InternalName(p + "WallResidualArea");
            residual.Shape = AreaEffectShape.Cylinder;
            residual.Size = new Feet(5);
            residual.AffectEnemies = true;
            residual.AffectDead = false;
            residual.AggroEnemies = false;
            residual.IgnoreSleepingUnits = false;
            residual.SpellResistance = true;
            residual.Fx = new Kingmaker.ResourceLinks.PrefabLink();
            residual.ComponentsArray = Array.Empty<BlueprintComponent>();
        }
    }
}
