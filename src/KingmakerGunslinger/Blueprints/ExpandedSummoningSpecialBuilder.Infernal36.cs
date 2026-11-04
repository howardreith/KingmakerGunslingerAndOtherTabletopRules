using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.Controllers.Brain.Blueprints.Considerations;
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
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningSpecialBuilder
    {
        internal const string Babau36Prefix =
            "KMG.Summoning.Infernal.Babau.";
        internal const string Hezrou36Prefix =
            "KMG.Summoning.Infernal.Hezrou.";
        private const string NativeDispelMagicTarget36 =
            "143775c49ae6b7446b805d3b2e702298";
        private const string NativeSeeInvisibilityBuff36 =
            "0adecbf63b614e846bfe15c33f34507e";
        private const string NativeSickened36 =
            "4e42460798665fd4cb9173ffa7ada323";
        private const string NativeNauseated36 =
            "956331dba5125ef48afe41875a00ca0e";
        private const string NativeSlow36 =
            "0bc608c3f2b548b44b7146b7530613ac";
        private const string NativeBlasphemy36 =
            "bd10c534a09f44f4ea632c8b8ae97145";
        private const string NativeBlindFight36 =
            "4e219f5894ad0ea4daa0699e28c37b1d";
        private const string NativeGreatCleave36 =
            "cc9c862ef2e03af4f89be5088851ea35";
        private const string SkillFocusStealth36 =
            "3a8d34905eae4a74892aae37df3352b9";

        private static void ConfigureInfernal36(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> symbols)
        {
            InfernalRules36Policy.Validate();
            ConfigureBabau36(library, symbols);
            ConfigureHezrou36(library, symbols);
            foreach (var pair in symbols.Where(pair =>
                pair.Value is BlueprintUnitFact &&
                (pair.Key.StartsWith(Babau36Prefix,
                    StringComparison.Ordinal) ||
                 pair.Key.StartsWith(Hezrou36Prefix,
                    StringComparison.Ordinal))))
            {
                var fact = (BlueprintUnitFact)pair.Value;
                if (fact.name != InternalName(pair.Key)) continue;
                NamePrivateSummonFactComponents(fact, pair.Key,
                    "Sprint36");
            }
        }

        private static BlueprintUnit InfernalProfile36(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> symbols,
            string token, string creatureKey, string prefix, string title,
            int hitDice, int hitPoints, int fortitude, int reflex, int will,
            int perception, int stealth, int mobility)
        {
            var unit = Require<BlueprintUnit>(symbols,
                "KMG.Summoning.Unit." + token);
            var type = Require<BlueprintUnitType>(symbols,
                prefix + "UnitType");
            type.name = InternalName(prefix + "UnitType");
            type.Name = LocalizationService.Create(prefix + "UnitType.Name",
                title);
            type.Description = LocalizationService.Create(
                prefix + "UnitType.Description",
                title + ", a chaotic evil summoned outsider.");
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
                "Sprint 36 measured original-body animation carrier");
            if (donor.Prefab?.AssetId !=
                TheropodVisualPolicy.DonorPrefabGuid)
                throw new InvalidOperationException(
                    "Sprint 36 demon original-body rig changed.");
            unit.Prefab = donor.Prefab;
            ConfigureOutsiderOriginalExpiryVisual(library, unit);
            return unit;
        }

        private static List<BlueprintComponent> InfernalDefenses36(
            BlueprintUnit unit, int naturalArmor, int spellResistance,
            bool coldIronOrGood)
        {
            var dr = ScriptableObject.CreateInstance<
                AddDamageResistancePhysical>();
            dr.Value = Simple(10);
            dr.Or = coldIronOrGood;
            dr.BypassedByAlignment = true;
            dr.Alignment = DamageAlignment.Good;
            dr.BypassedByMaterial = coldIronOrGood;
            dr.Material = PhysicalDamageMaterial.ColdIron;
            var armor = ScriptableObject.CreateInstance<AddStatBonus>();
            armor.Stat = StatType.AC;
            armor.Value = naturalArmor;
            armor.Descriptor = ModifierDescriptor.NaturalArmor;
            var sr = ScriptableObject.CreateInstance<AddSpellResistance>();
            sr.Value = Simple(spellResistance);
            sr.AddCR = false;
            var chaotic = ScriptableObject.CreateInstance<
                AddOutgoingPhysicalDamageProperty>();
            chaotic.AffectAnyPhysicalDamage = true;
            chaotic.AddAlignment = true;
            chaotic.Alignment = DamageAlignment.Chaotic;
            var evil = ScriptableObject.CreateInstance<
                AddOutgoingPhysicalDamageProperty>();
            evil.AffectAnyPhysicalDamage = true;
            evil.AddAlignment = true;
            evil.Alignment = DamageAlignment.Evil;
            var lifetime = ScriptableObject.CreateInstance<
                InfernalSourceLifetime34>();
            lifetime.OwningBlueprint = unit;
            var commands = ScriptableObject.CreateInstance<
                OutsiderLifetimeBoundary>();
            commands.OwningBlueprint = unit;
            return new List<BlueprintComponent> { dr, armor, sr, chaotic,
                evil, Energy(DamageEnergyType.Acid, 10),
                Energy(DamageEnergyType.Cold, 10),
                Energy(DamageEnergyType.Fire, 10), lifetime, commands };
        }

        private static BlueprintUnitFact[] InfernalFacts36(
            LibraryScriptableObject library, BlueprintFeature traits)
        {
            return new BlueprintUnitFact[] {
                Feature(library, OutsiderTypeGuid, "outsider type"),
                Feature(library, EvilSubtypeGuid, "evil subtype"),
                Feature(library, ChaoticSubtypeGuid, "chaotic subtype"),
                Feature(library, ExtraplanarSubtypeGuid,
                    "native extraplanar spell consumer"),
                Feature(library, ElectricityImmunityGuid,
                    "demon electricity immunity"),
                Feature(library, PoisonImmunityGuid,
                    "demon poison immunity"), traits };
        }

        private static void ConfigureBabau36(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> symbols)
        {
            string p = Babau36Prefix;
            var unit = InfernalProfile36(library, symbols, "Babau", "babau",
                p, "Babau", 7, 73, 5, 5, 2, 7, 7, 7);
            ConfigureUnitCore(unit, "Babau", "Babau",
                Alignment.ChaoticEvil, Size.Medium, 21, 13, 20, 14, 13, 16,
                30);
            var bite = Sprint29Weapon(library, symbols, p + "Bite",
                p + "BiteType", NativeBite1d6Guid, Size.Medium, 1,
                DiceType.D6, 9);
            var claw = Sprint29Weapon(library, symbols, p + "Claw",
                p + "ClawType", NativeClaw1d4Guid, Size.Medium, 1,
                DiceType.D6, 9);
            var spear = Sprint29Weapon(library, symbols, p + "Longspear",
                p + "LongspearType",
                ElvenBranchedSpearBlueprints.NativeLongspearItemGuid,
                Size.Medium, 1, DiceType.D8, 10);
            BindBarbazuNativeHand34(unit, spear);
            unit.Body = NaturalBody(spear, new[] { claw, bite },
                Array.Empty<BlueprintItemWeapon>());
            unit.Body.PrimaryHandAlternative1 = claw;
            unit.Body.EmptyHandWeapon = null;
            var profile = ScriptableObject.CreateInstance<
                InfernalMartialProfile36>();
            profile.OwningBlueprint = unit;
            profile.Bite = bite;
            profile.Claw = claw;
            profile.Longspear = spear;
            unit.ComponentsArray = unit.ComponentsArray.Concat(
                new BlueprintComponent[] { profile }).ToArray();
            var parts = InfernalDefenses36(unit, 8, 17, true);
            var strength = ScriptableObject.CreateInstance<
                DeinonychusPrimaryTalonStrength>();
            strength.OwningBlueprint = unit;
            strength.Talon = claw;
            var ownership = ScriptableObject.CreateInstance<
                SummonGiantEquipmentOwnership>();
            ownership.OwningBlueprint = unit;
            ownership.GreatWeapon = spear;
            ownership.Slam = claw;
            ownership.Rock = bite;
            var proficiency = ScriptableObject.CreateInstance<
                AddProficiencies>();
            proficiency.WeaponProficiencies = new[] { spear.Category };
            proficiency.ArmorProficiencies =
                Array.Empty<ArmorProficiencyGroup>();
            var slime = ScriptableObject.CreateInstance<ProtectiveSlime36>();
            slime.OwningBlueprint = unit;
            var sneak = ScriptableObject.CreateInstance<AddStatBonus>();
            sneak.Stat = StatType.SneakAttack;
            sneak.Value = 2;
            sneak.Descriptor = ModifierDescriptor.UntypedStackable;
            var perception = ScriptableObject.CreateInstance<AddStatBonus>();
            perception.Stat = StatType.SkillPerception;
            perception.Value = 8;
            perception.Descriptor = ModifierDescriptor.Racial;
            var stealth = ScriptableObject.CreateInstance<AddStatBonus>();
            stealth.Stat = StatType.SkillStealth;
            stealth.Value = 8;
            stealth.Descriptor = ModifierDescriptor.Racial;
            parts.AddRange(new BlueprintComponent[] { strength, ownership,
                proficiency, slime, sneak, perception, stealth });
            var dispel = ConfigureOwnedSpell35(library, symbols,
                p + "DispelMagic", NativeDispelMagicTarget36,
                "Dispel Magic", 3, UnitCommand.CommandType.Standard,
                InfernalRules36Policy.BabauDispelCasterLevel,
                "At will at caster level 7, make the native dispel check against one creature's spell effects. The ability remains available to the player; AI selection requires a visible dispellable effect.");
            var parameters = ScriptableObject.CreateInstance<
                Sprint29GiantSpellLikeParameters>();
            parameters.OwningBlueprint = unit;
            parameters.Abilities = new[] { dispel };
            parameters.SpellLevels = new[] { 3 };
            parameters.CasterLevel =
                InfernalRules36Policy.BabauDispelCasterLevel;
            parts.Add(parameters);
            var traits = Require<BlueprintFeature>(symbols, p + "Traits");
            BindInfernalTraits35(traits, p, "Babau",
                "DR10/cold iron or good; electricity and poison immunity; acid, cold and fire resistance 10; SR17. Natural and armed routines remain alternatives. Natural or unarmed strikes can trigger Protective Slime (Reflex DC18, 1d8 acid); the manufactured-item branch is not represented by wielder damage.",
                parts);
            unit.AddFacts = InfernalFacts36(library, traits).Concat(
                new BlueprintUnitFact[] {
                    Feature(library, CombatReflexesGuid,
                        "Combat Reflexes"),
                    Feature(library, ImprovedInitiativeGuid,
                        "Improved Initiative"),
                    Feature(library, IronWillGuid, "Iron Will"),
                    Feature(library, SkillFocusStealth36,
                        "Skill Focus (Stealth)"),
                    BlueprintLibraryLookup.RequireExact<BlueprintBuff>(
                        library, NativeSeeInvisibilityBuff36,
                        "native constant see invisibility consumer"),
                    dispel }).ToArray();
            var mode = Require<BabauWeaponModeAi36>(symbols,
                p + "WeaponModeAi");
            mode.name = InternalName(p + "WeaponModeAi");
            mode.OwningBlueprint = unit;
            mode.BaseScore = 7;
            mode.CooldownRounds = 1;
            mode.ActorConsiderations = mode.TargetConsiderations =
                Array.Empty<Consideration>();
            var dispelAi = Require<BabauDispelAi36>(symbols,
                p + "DispelAi");
            ConfigureMephitAiAction(dispelAi, p + "DispelAi", dispel, 0);
            dispelAi.OwningBlueprint = unit;
            dispelAi.BaseScore = 6;
            unit.Brain = InfernalBrain35(library, symbols, p, dispelAi, mode);
        }

        private static BlueprintBuff ConfigureCondition36(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> symbols,
            string symbol, string nativeGuid, string title,
            string description, string iconKey, bool poison)
        {
            var native = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(
                library, nativeGuid, "native " + title + " condition");
            var buff = Require<BlueprintBuff>(symbols, symbol);
            CopyFields(native, buff);
            buff.name = InternalName(symbol);
            buff.Stacking = StackingType.Stack;
            var components = (native.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Select(
                    ExpandedSummoningAbilityBuilder.DeepCloneComponent)
                .ToList();
            if (poison)
            {
                var descriptor = ScriptableObject.CreateInstance<
                    SpellDescriptorComponent>();
                descriptor.Descriptor = SpellDescriptor.Poison;
                components.Add(descriptor);
            }
            buff.ComponentsArray = components.ToArray();
            BlueprintUnitFactAccess.Resolve().Configure(buff,
                LocalizationService.Create(symbol + ".Name", title),
                LocalizationService.Create(symbol + ".Description",
                    description),
                ExpandedSummoningProjectIcons.Require(iconKey));
            return buff;
        }

        private static void ConfigureHezrou36(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> symbols)
        {
            string p = Hezrou36Prefix;
            var unit = InfernalProfile36(library, symbols, "Hezrou",
                "hezrou", p, "Hezrou", 10, 145, 7, 3, 7, 10, 0, 0);
            ConfigureUnitCore(unit, "Hezrou", "Hezrou",
                Alignment.ChaoticEvil, Size.Large, 27, 11, 29, 14, 14, 18,
                30);
            var bite = Sprint29Weapon(library, symbols, p + "Bite",
                p + "BiteType", NativeBite1d6Guid, Size.Large, 4,
                DiceType.D4, 10);
            var claw = Sprint29Weapon(library, symbols, p + "Claw",
                p + "ClawType", NativeClaw1d4Guid, Size.Large, 1,
                DiceType.D8, 10);
            unit.Body = NaturalBody(bite, new[] { claw, claw },
                Array.Empty<BlueprintItemWeapon>());
            var profile = ScriptableObject.CreateInstance<
                InfernalMartialProfile36>();
            profile.OwningBlueprint = unit;
            profile.Bite = bite;
            profile.Claw = claw;
            unit.ComponentsArray = unit.ComponentsArray.Concat(
                new BlueprintComponent[] { profile }).ToArray();
            var parts = InfernalDefenses36(unit, 16, 22, false);
            var strength = ScriptableObject.CreateInstance<
                DeinonychusPrimaryTalonStrength>();
            strength.OwningBlueprint = unit;
            strength.Talon = bite;
            var ownership = ScriptableObject.CreateInstance<
                SummonGiantEquipmentOwnership>();
            ownership.OwningBlueprint = unit;
            ownership.GreatWeapon = bite;
            ownership.Slam = claw;
            var grab = ScriptableObject.CreateInstance<SummonGrabComponent>();
            grab.GrabWithPrimaryHand = true;
            grab.GrabAdditionalLimbCount = 2;
            grab.MaxTargetSizeDelta = 0;
            grab.MaxHeldTargets = 3;
            grab.DeclineGrabDuringCleaveOwner = unit;
            grab.HoldBuff = Require<BlueprintBuff>(symbols,
                GrappleMultiHoldSymbol);
            grab.GrappledBuff = Require<BlueprintBuff>(symbols,
                GrappleMultiHeldSymbol);
            var maneuver = ScriptableObject.CreateInstance<ManeuverBonus>();
            maneuver.Type = CombatManeuver.Grapple;
            maneuver.Bonus = ExpandedSummoningSpecialProfiles
                .SummonGrabManeuverBonus;
            parts.AddRange(new BlueprintComponent[] { strength, ownership,
                grab, maneuver });

            var sickened = ConfigureCondition36(library, symbols,
                p + "StenchSickened", NativeSickened36, "Hezrou Stench",
                "Poison: sickened for 10 rounds after a failed Fortitude save against this hezrou's stench.",
                "hezrou", true);
            var memory = Require<BlueprintBuff>(symbols,
                p + "StenchMemory");
            memory.name = InternalName(p + "StenchMemory");
            memory.Stacking = StackingType.Stack;
            SetBuffFlags(memory, false, true);
            memory.ComponentsArray = Array.Empty<BlueprintComponent>();
            BlueprintUnitFactAccess.Resolve().Configure(memory,
                LocalizationService.Create(p + "StenchMemory.Name",
                    "Hezrou Stench Memory"),
                LocalizationService.Create(p + "StenchMemory.Description",
                    "Source-specific stench exposure memory. A successful save protects against only this hezrou for 24 hours."),
                null);
            var area = Require<BlueprintAbilityAreaEffect>(symbols,
                p + "StenchArea");
            area.name = InternalName(p + "StenchArea");
            area.Shape = AreaEffectShape.Cylinder;
            area.Size = new Feet(
                InfernalRules36Policy.StenchRadiusFeet);
            // Native area collection always includes allies; this flag adds
            // enemies. The logic deliberately does no faction filtering.
            area.AffectEnemies = true;
            area.AffectDead = false;
            area.AggroEnemies = false;
            area.IgnoreSleepingUnits = false;
            area.SpellResistance = false;
            area.Fx = new Kingmaker.ResourceLinks.PrefabLink();
            var stench = ScriptableObject.CreateInstance<HezrouStench36>();
            stench.OwningBlueprint = unit;
            stench.Sickened = sickened;
            stench.SourceMemory = memory;
            stench.PoisonImmunity = Feature(library, PoisonImmunityGuid,
                "native poison immunity");
            area.ComponentsArray = new BlueprintComponent[] { stench };
            var carrier = Require<BlueprintBuff>(symbols,
                p + "StenchCarrier");
            carrier.name = InternalName(p + "StenchCarrier");
            carrier.Stacking = StackingType.Replace;
            SetBuffFlags(carrier, false, true);
            var emanation = ScriptableObject.CreateInstance<AddAreaEffect>();
            emanation.AreaEffect = area;
            carrier.ComponentsArray = new BlueprintComponent[] { emanation };
            BlueprintUnitFactAccess.Resolve().Configure(carrier,
                LocalizationService.Create(p + "StenchCarrier.Name",
                    "Stench"),
                LocalizationService.Create(p + "StenchCarrier.Description",
                    "Living creatures within 30 feet, including allies, attempt a Fortitude save against this poison effect. Failure sickens for 10 rounds; success grants source-specific immunity for 24 hours."),
                ExpandedSummoningProjectIcons.Require("hezrou"));
            var nausea = ConfigureCondition36(library, symbols,
                p + "HeldNauseated", NativeNauseated36,
                "Hezrou Grapple Nausea",
                "Poison: nausea from being held by this hezrou. A later successful round save ends it; after release it expires in one minute.",
                "hezrou", true);
            var held = ScriptableObject.CreateInstance<HezrouHeldNausea36>();
            held.OwningBlueprint = unit;
            held.Nauseated = nausea;
            held.PoisonImmunity = Feature(library, PoisonImmunityGuid,
                "native poison immunity");
            var nauseaLifetime = ScriptableObject.CreateInstance<
                HezrouHeldNauseaDuration36>();
            nausea.ComponentsArray = nausea.ComponentsArray.Concat(
                new BlueprintComponent[] { nauseaLifetime }).ToArray();
            var perception = ScriptableObject.CreateInstance<AddStatBonus>();
            perception.Stat = StatType.SkillPerception;
            perception.Value = 8;
            perception.Descriptor = ModifierDescriptor.Racial;
            parts.AddRange(new BlueprintComponent[] { held, perception });

            TrumpetBlindFight.VerifyContract();
            var nativeBlind = BlueprintLibraryLookup.RequireExact<
                BlueprintFeature>(library, NativeBlindFight36,
                    "native Blind-Fight presentation and defense");
            var blind = ScriptableObject.CreateInstance<TrumpetBlindFight>();
            blind.OwningBlueprint = unit;
            parts.Add(blind);
            parts.Add(ExpandedSummoningAbilityBuilder.DeepCloneComponent(
                nativeBlind.ComponentsArray.OfType<FlatFootedIgnore>().Single()));

            var chaos = ConfigureAlignmentBurst36(library, symbols, unit,
                true);
            var unholy = ConfigureAlignmentBurst36(library, symbols, unit,
                false);
            var blasphemyResource = Require<BlueprintAbilityResource>(symbols,
                p + "BlasphemyResource");
            ConfigureNamedResource(blasphemyResource,
                p + "BlasphemyResource", p + "BlasphemyResource",
                "Blasphemy", "One Blasphemy use for this summoned Hezrou.",
                1);
            var blasphemy = ConfigureOwnedSpell35(library, symbols,
                p + "Blasphemy", NativeBlasphemy36, "Blasphemy", 7,
                UnitCommand.CommandType.Standard,
                InfernalRules36Policy.HezrouSpellLikeCasterLevel,
                "Once per summoning at caster level 13. The installed native spell owns non-evil eligibility, spell resistance, Will resolution and hit-dice-dependent effects.");
            var blasphemyCost = ScriptableObject.CreateInstance<
                AbilityResourceLogic>();
            blasphemyCost.RequiredResource = blasphemyResource;
            blasphemyCost.IsSpendResource = true;
            blasphemyCost.Amount = 1;
            blasphemy.ComponentsArray = blasphemy.ComponentsArray.Concat(
                new BlueprintComponent[] { blasphemyCost }).ToArray();
            var parameters = ScriptableObject.CreateInstance<
                Sprint29GiantSpellLikeParameters>();
            parameters.OwningBlueprint = unit;
            parameters.Abilities = new[] { chaos, unholy, blasphemy };
            parameters.SpellLevels = new[] { 4, 4, 7 };
            parameters.CasterLevel =
                InfernalRules36Policy.HezrouSpellLikeCasterLevel;
            parts.AddRange(new BlueprintComponent[] { parameters,
                AddResource(blasphemyResource) });

            var traits = Require<BlueprintFeature>(symbols, p + "Traits");
            BindInfernalTraits35(traits, p, "Hezrou",
                "DR10/good; electricity and poison immunity; acid, cold and fire resistance 10; SR22. Bite and two claws have Grab without constrict, impale or swallow. Stench and held-victim nausea have separate source-owned saves and recovery. Chaos Hammer and Unholy Blight are at will; Blasphemy is once per summoning.",
                parts);
            unit.AddFacts = InfernalFacts36(library, traits).Concat(
                new BlueprintUnitFact[] {
                    Require<BlueprintFeature>(symbols,
                        GiantCleaveFeature),
                    Feature(library, NativeGreatCleave36,
                        "Great Cleave"),
                    Feature(library, ImprovedInitiativeGuid,
                        "Improved Initiative"),
                    Feature(library, PowerAttackGuid, "Power Attack"),
                    carrier, chaos, unholy, blasphemy }).ToArray();
            var chaosAi = ConfigureInfernalAi35(symbols, p + "ChaosAi",
                unit, chaos, false, true, 4);
            chaosAi.AvoidFriendlyArea = true;
            chaosAi.ChaoticExemptFriendlyArea = true;
            chaosAi.SharedSpellCooldownRounds = 2;
            chaosAi.AreaRadiusFeet = 20;
            var unholyAi = ConfigureInfernalAi35(symbols, p + "UnholyAi",
                unit, unholy, false, true, 4);
            unholyAi.AvoidFriendlyArea = true;
            unholyAi.SharedSpellCooldownRounds = 2;
            unholyAi.AreaRadiusFeet = 20;
            var blasphemyAi = ConfigureInfernalAi35(symbols,
                p + "BlasphemyAi", unit, blasphemy, true, false, 6);
            blasphemyAi.AvoidFriendlyArea = true;
            blasphemyAi.RequireAffectedEnemyArea = true;
            blasphemyAi.MaximumAffectedHitDice =
                InfernalRules36Policy.HezrouSpellLikeCasterLevel;
            blasphemyAi.SharedSpellCooldownRounds = 2;
            blasphemyAi.AreaRadiusFeet = 40;
            unit.Brain = InfernalBrain35(library, symbols, p, blasphemyAi,
                chaosAi, unholyAi);
        }

        /// <summary>
        /// One released alignment burst - chaos hammer or unholy blight -
        /// built from this project's own resolution action.
        ///
        /// <para><paramref name="prefix"/> and <paramref name="casterLevel"/>
        /// default to the Sprint 36 Hezrou's, which is what every released
        /// call passes, so the Hezrou's two abilities are unchanged. Sprint 38
        /// passes its own prefix and caster level 14 rather than cloning this
        /// method, because both creatures print the same two spell-likes and
        /// the printed dice caps (5d8, and 10d6 against the opposed outsider)
        /// are already reached at caster level 13.</para>
        /// </summary>
        private static BlueprintAbility ConfigureAlignmentBurst36(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> symbols,
            BlueprintUnit unit, bool chaos, string prefix = null,
            int casterLevel = 0)
        {
            prefix = prefix ?? Hezrou36Prefix;
            if (casterLevel <= 0)
                casterLevel = InfernalRules36Policy
                    .HezrouSpellLikeCasterLevel;
            string symbol = prefix +
                (chaos ? "ChaosHammer" : "UnholyBlight");
            var ability = Require<BlueprintAbility>(symbols, symbol);
            var around = ScriptableObject.CreateInstance<
                AbilityTargetsAround>();
            SetField(around, "m_Radius", new Feet(20));
            SetField(around, "m_TargetType",
                Kingmaker.UnitLogic.Abilities.Components.TargetType.Any);
            SetField(around, "m_IncludeDead", false);
            SetField(around, "m_Condition", new ConditionsChecker {
                Operation = Operation.And,
                Conditions = Array.Empty<Condition>() });
            SetField(around, "m_SpreadSpeed", new Feet(0));
            ContextAction resolve;
            if (chaos)
            {
                var action = ScriptableObject.CreateInstance<
                    ContextActionInfernalChaosHammer36>();
                action.SourceUnit = unit;
                action.OutsiderType = Feature(library, OutsiderTypeGuid,
                    "native outsider type");
                action.Slowed = BlueprintLibraryLookup.RequireExact<
                    BlueprintBuff>(library, NativeSlow36,
                        "native slowed condition");
                action.CasterLevel = casterLevel;
                action.SpellLevel = 4;
                action.DamageDiceCount = 5;
                action.OutsiderDamageDiceCount = 10;
                resolve = action;
            }
            else
            {
                var action = ScriptableObject.CreateInstance<
                    ContextActionInfernalUnholyBlight35>();
                action.SourceUnit = unit;
                action.OutsiderType = Feature(library, OutsiderTypeGuid,
                    "native outsider type");
                action.Sickened = BlueprintLibraryLookup.RequireExact<
                    BlueprintBuff>(library, NativeSickened36,
                        "native sickened condition");
                action.CasterLevel = casterLevel;
                action.SpellLevel = 4;
                action.DamageDiceCount = 5;
                action.OutsiderDamageDiceCount = 10;
                resolve = action;
            }
            var run = ScriptableObject.CreateInstance<
                AbilityEffectRunAction>();
            run.SavingThrowType = SavingThrowType.Unknown;
            run.Actions = new ActionList {
                Actions = new GameAction[] { resolve } };
            var school = ScriptableObject.CreateInstance<SpellComponent>();
            school.School = SpellSchool.Evocation;
            var descriptor = ScriptableObject.CreateInstance<
                SpellDescriptorComponent>();
            descriptor.Descriptor = chaos ? SpellDescriptor.Chaos :
                SpellDescriptor.Evil;
            var parameters = ScriptableObject.CreateInstance<
                ContextCalculateAbilityParams>();
            parameters.StatType = StatType.Charisma;
            parameters.ReplaceCasterLevel = true;
            parameters.CasterLevel = Simple(casterLevel);
            parameters.ReplaceSpellLevel = true;
            parameters.SpellLevel = Simple(4);
            ability.name = InternalName(symbol);
            ability.Parent = null;
            ability.Type = AbilityType.SpellLike;
            ability.Range = AbilityRange.Medium;
            ability.CanTargetPoint = true;
            ability.CanTargetSelf = false;
            ability.CanTargetFriends = true;
            ability.CanTargetEnemies = true;
            ability.SpellResistance = true;
            ability.NeedEquipWeapons = false;
            ability.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            ability.EffectOnAlly = AbilityEffectOnUnit.Harmful;
            ability.ActionType = UnitCommand.CommandType.Standard;
            ability.SetIsFullRoundAction(false);
            ability.Animation =
                UnitAnimationActionCastSpell.CastAnimationStyle.Omni;
            ability.Hidden = false;
            ability.ActionBarAutoFillIgnored = false;
            ability.MaterialComponent =
                new BlueprintAbility.MaterialComponentData();
            ability.ResourceAssetIds = Array.Empty<string>();
            ability.ComponentsArray = new BlueprintComponent[] { around, run,
                school, descriptor, parameters };
            var icon = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                library, NativeBlasphemy36,
                "native evil burst presentation").Icon;
            string title = chaos ? "Chaos Hammer" : "Unholy Blight";
            string primary = chaos ? "lawful" : "good";
            string exempt = chaos ? "chaotic" : "evil";
            BlueprintUnitFactAccess.Resolve().Configure(ability,
                LocalizationService.Create(symbol + ".Name", title),
                LocalizationService.Create(symbol + ".Description",
                    "At will at caster level " + casterLevel +
                    ", burst power in a 20-foot radius at medium range. " +
                    exempt + " creatures are unaffected; " + primary +
                    " creatures take full alignment-sensitive damage and the condition only on a failed Will save; neutral creatures take half damage and no condition. Spell resistance applies. Allies are not automatically excluded."),
                icon);
            return ability;
        }
    }
}
