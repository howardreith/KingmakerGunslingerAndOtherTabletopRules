using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningSpecialBuilder
    {
        internal const string IceDevil40Prefix = "KMG.Summoning.Infernal.IceDevil.";
        private const string NativeConeOfCold40 = "e7c530f8137630f4d9d7ee1aa7b1edc0";
        private const string NativeIceStorm40 = "fcb028205a71ee64d98175ff39a0abf9";
        private const string NativeIceStormArea40 = "4c695315962bf9a4ea7fc7e2bb3e2f60";
        private const string NativeSpearType40 = "4b289eccefe6d704093201e52eb6d123";

        private static void ConfigureIceDevil40(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> symbols)
        {
            string p = IceDevil40Prefix;
            var unit = InfernalProfile35(library, symbols, "IceDevil", "ice-devil",
                p, "Ice Devil (Gelugon)", IceDevilRules40Policy.HitDice,
                IceDevilRules40Policy.RacialBaseHitPoints, IceDevilRules40Policy.FortitudeBase,
                IceDevilRules40Policy.ReflexBase, IceDevilRules40Policy.WillBase,
                14, 14, 14);
            ConfigureUnitCore(unit, "IceDevil", "Ice Devil (Gelugon)",
                Alignment.LawfulEvil, Size.Large, IceDevilRules40Policy.Strength,
                IceDevilRules40Policy.Dexterity, IceDevilRules40Policy.Constitution,
                IceDevilRules40Policy.Intelligence, IceDevilRules40Policy.Wisdom,
                IceDevilRules40Policy.Charisma, IceDevilRules40Policy.FlySpeedFeet);

            // The retained longspear supplies only a native item/view shell.
            // The captured native SPEAR type supplies category, two-hand grip,
            // piercing and x3 critical semantics. It is not a reach longspear.
            var spearType = BlueprintLibraryLookup.RequireExact<BlueprintWeaponType>(
                library, NativeSpearType40, "native Spear type");
            var spear = Sprint29Weapon(library, symbols, p + "Spear", p + "SpearType",
                ElvenBranchedSpearBlueprints.NativeLongspearItemGuid, Size.Large,
                2, DiceType.D6, IceDevilRules40Policy.ReachFeet, 20, spearType);
            SetField(spear, "m_Enchantments", new[] {
                BlueprintLibraryLookup.RequireExact<BlueprintWeaponEnchantment>(library,
                    MagicFirearmBlueprints.Enhancement1Guid, "native +1 enhancement"),
                BlueprintLibraryLookup.RequireExact<BlueprintWeaponEnchantment>(library,
                    EasternWeaponNamedBlueprints.FrostGuid, "native frost packet") });
            BindBarbazuNativeHand34(unit, spear);
            var bite = Sprint29Weapon(library, symbols, p + "Bite", p + "BiteType",
                NativeBite1d6Guid, Size.Large, 2, DiceType.D6,
                IceDevilRules40Policy.ReachFeet);
            var tail = Sprint29Weapon(library, symbols, p + "Tail", p + "TailType",
                NativeSlam, Size.Large, 3, DiceType.D6,
                IceDevilRules40Policy.ReachFeet);
            SetField(tail.Type, "m_TypeNameText",
                LocalizationService.Create(p + "TailType.Name", "Tail"));
            BlueprintItemAccess.Resolve().Configure(spear,
                LocalizationService.Create(p + "Spear.Name", "+1 Frost Spear"),
                LocalizationService.Create(p + "Spear.Description",
                    "Large +1 frost spear. Slowing a struck enemy is the ice devil's own supernatural power, not a transferable property of this weapon."),
                LocalizationService.Create(p + "Spear.Flavor", ""), 0, 0);
            BlueprintItemAccess.Resolve().Configure(bite,
                LocalizationService.Create(p + "Bite.Name", "Bite"),
                LocalizationService.Create(p + "Bite.Description", "The gelugon's natural bite."),
                LocalizationService.Create(p + "Bite.Flavor", ""), 0, 0);
            BlueprintItemAccess.Resolve().Configure(tail,
                LocalizationService.Create(p + "Tail.Name", "Tail"),
                LocalizationService.Create(p + "Tail.Description", "The gelugon's secondary bludgeoning tail strike."),
                LocalizationService.Create(p + "Tail.Flavor", ""), 0, 0);
            unit.Body = NaturalBody(spear, new[] { bite }, new[] { tail });
            unit.Body.EmptyHandWeapon = bite;
            var profile = ScriptableObject.CreateInstance<IceDevilMartialProfile40>();
            profile.OwningBlueprint = unit;
            profile.Spear = spear; profile.Bite = bite; profile.Tail = tail;
            unit.ComponentsArray = unit.ComponentsArray.Concat(new BlueprintComponent[] { profile }).ToArray();

            var parts = InfernalDefenses35(unit, IceDevilRules40Policy.NaturalArmor,
                IceDevilRules40Policy.SpellResistance, alertnessBonus: 4, coldResistance: false);
            var focus = ScriptableObject.CreateInstance<WeaponFocus>();
            focus.WeaponType = spear.Type; focus.AttackBonus = 1;
            var proficiency = ScriptableObject.CreateInstance<AddProficiencies>();
            proficiency.WeaponProficiencies = new[] { spear.Category };
            proficiency.ArmorProficiencies = Array.Empty<ArmorProficiencyGroup>();
            var ownership = ScriptableObject.CreateInstance<SummonGiantEquipmentOwnership>();
            ownership.OwningBlueprint = unit;
            ownership.GreatWeapon = spear; ownership.Slam = bite; ownership.Rock = tail;
            var biteStrength = ScriptableObject.CreateInstance<IceDevilBiteStrength40>();
            biteStrength.OwningBlueprint = unit; biteStrength.Bite = bite;
            // Alertness's +4 Perception comes from the shared helper's own
            // parameter, once. Sense Motive has no separate native consumer.
            var intimidate = ScriptableObject.CreateInstance<InfernalIntimidate34>();
            intimidate.OwningBlueprint = unit; intimidate.TrainedContribution = 14;
            var rider = ScriptableObject.CreateInstance<IceDevilSlowOnHit40>();
            rider.OwningBlueprint = unit; rider.Spear = spear; rider.Tail = tail;
            rider.Slowed = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(
                library, NativeSlow36, "native Slow condition");
            rider.OnFailedSave = IceDevilSlowBranch40(library, rider.Slowed);
            parts.AddRange(new BlueprintComponent[] {
                focus, proficiency, ownership, biteStrength, intimidate, rider });

            var cone = ConfigureIceDevilColdSpell40(library, symbols, "ConeOfCold",
                NativeConeOfCold40, "Cone of Cold", 5);
            var storm = ConfigureIceDevilColdSpell40(library, symbols, "IceStorm",
                NativeIceStorm40, "Ice Storm", 4);
            var wall = ConfigureWallAbility35(symbols, p, "ice-devil",
                IceDevilRules40Policy.UnitSymbol, IceDevilRules40Policy.SpellLikeCasterLevel,
                100 + 10 * IceDevilRules40Policy.SpellLikeCasterLevel);
            ConfigureWallPrototype35(library, symbols, p, "ice-devil",
                IceDevilRules40Policy.UnitSymbol, IceDevilRules40Policy.SpellLikeCasterLevel);
            var wallLifetime = ScriptableObject.CreateInstance<InfernalWallSourceLifetime35>();
            wallLifetime.OwningBlueprint = unit;
            wallLifetime.WallSection = Require<BlueprintUnit>(symbols, p + "WallSectionUnit");
            wallLifetime.WallAbility = wall;
            wallLifetime.ResidualArea = Require<BlueprintAbilityAreaEffect>(symbols, p + "WallResidualArea");
            wallLifetime.FogArea = BlueprintLibraryLookup.RequireExact<BlueprintAbilityAreaEffect>(
                library, NativeObscuringMistArea35, "native fire-breach fog");
            parts.Add(wallLifetime);
            var parameters = ScriptableObject.CreateInstance<Sprint29GiantSpellLikeParameters>();
            parameters.OwningBlueprint = unit;
            parameters.Abilities = new[] { cone, storm, wall };
            parameters.SpellLevels = new[] { 5, 4, 4 };
            parameters.CasterLevel = IceDevilRules40Policy.SpellLikeCasterLevel;
            parts.Add(parameters);

            var fear = ConfigureIceDevilFear40(library, symbols, unit);
            var regeneration = ConfigureIceDevilRegeneration40(symbols, unit);
            var traits = Require<BlueprintFeature>(symbols, p + "Traits");
            BindInfernalTraits35(traits, p, "Gelugon",
                "Devil: DR10/good; fire, cold and poison immunity; acid resistance10; SR24. " +
                "Regeneration5 is suppressed by good weapons/spells. Spear iteratives, bite and secondary tail; " +
                "only this owner's spear/tail hits attempt supernatural Slow (Fortitude, live Constitution, 1d6 rounds). " +
                "Fear aura10 feet. Effective flight60 feet; separate land40 mode unmodeled. " +
                "Persistent Image is not represented; travel and nested summons excluded.", parts);
            unit.AddFacts = InfernalFacts35(library, traits).Concat(new BlueprintUnitFact[] {
                Feature(library, ColdImmunityGuid, "Ice Devil cold immunity"),
                Feature(library, AirborneGuid, "ordinary-map flight navigation, no extra speed"),
                Feature(library, ImprovedInitiativeGuid, "Improved Initiative"),
                Feature(library, IronWillGuid, "Iron Will"),
                Feature(library, PowerAttackGuid, "Power Attack"),
                Feature(library, CombatReflexesGuid, "Combat Reflexes"),
                Require<BlueprintFeature>(symbols, GiantCleaveFeature),
                cone, storm, wall, fear, regeneration }).ToArray();

            var coneAi = ConfigureInfernalAi35(symbols, p + "ConeOfColdAi", unit,
                cone, false, true, 6);
            coneAi.AvoidFriendlyArea = true; coneAi.NoAlignmentExemption = true;
            coneAi.UseNativeProjectileShape = true; coneAi.AreaRadiusFeet = 60;
            coneAi.AvoidColdImmuneTarget = true;
            var stormAi = ConfigureInfernalAi35(symbols, p + "IceStormAi", unit,
                storm, false, true, 5);
            stormAi.AvoidFriendlyArea = true; stormAi.NoAlignmentExemption = true;
            stormAi.AreaRadiusFeet = 20;
            var wallAi = ConfigureInfernalAi35(symbols, p + "WallOfIceAi", unit,
                wall, false, true, 4);
            wallAi.WallPlacement = true; wallAi.WallSection = wallLifetime.WallSection;
            // No resources or cooldowns are introduced on these at-will casts.
            coneAi.SharedSpellCooldownRounds = stormAi.SharedSpellCooldownRounds =
                wallAi.SharedSpellCooldownRounds = 0;
            unit.Brain = InfernalBrain35(library, symbols, p, coneAi, stormAi, wallAi);
            foreach (var pair in symbols.Where(pair => pair.Value is BlueprintUnitFact &&
                pair.Key.StartsWith(p, StringComparison.Ordinal)))
            {
                var fact = (BlueprintUnitFact)pair.Value;
                if (fact.name == InternalName(pair.Key))
                    NamePrivateSummonFactComponents(fact, pair.Key, "Sprint40");
            }
        }

        private static ActionList IceDevilSlowBranch40(LibraryScriptableObject library,
            BlueprintBuff slow)
        {
            var native = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library,
                NativeSlowSpell39, "native Slow haste-counter/condition branch");
            var run = (AbilityEffectRunAction)ExpandedSummoningAbilityBuilder.DeepCloneComponent(
                native.GetComponent<AbilityEffectRunAction>());
            var failed = run.Actions.Actions.OfType<ContextActionConditionalSaved>().Single().Failed;
            var conditional = failed.Actions.OfType<Conditional>().Single();
            var applications = conditional.IfTrue.Actions.Concat(conditional.IfFalse.Actions)
                .OfType<ContextActionApplyBuff>().Where(value => ReferenceEquals(value.Buff, slow)).ToArray();
            if (applications.Length != 1)
                throw new InvalidOperationException("Native Slow condition/haste branch changed.");
            var apply = applications[0];
            apply.Permanent = false; apply.UseDurationSeconds = false;
            apply.DurationValue = new ContextDurationValue {
                Rate = DurationRate.Rounds, DiceType = DiceType.D6,
                DiceCountValue = Simple(1), BonusValue = Simple(0) };
            apply.IsFromSpell = false; apply.IsNotDispelable = true; apply.AsChild = false;
            return failed;
        }

        private static BlueprintAbility ConfigureIceDevilColdSpell40(
            LibraryScriptableObject library, IDictionary<string, BlueprintScriptableObject> symbols,
            string suffix, string nativeGuid, string title, int level)
        {
            string p = IceDevil40Prefix;
            var ability = ConfigureOwnedSpell35(library, symbols, p + suffix, nativeGuid,
                title, level, UnitCommand.CommandType.Standard,
                IceDevilRules40Policy.SpellLikeCasterLevel,
                suffix == "ConeOfCold" ? "At will: a 60-foot cone, 13d6 cold at CL13, Reflex half and spell resistance." :
                "At will: 3d6 bludgeoning plus 2d6 cold once on casting, no save, spell resistance; 20-foot radius with native difficult terrain and Perception penalty for 13 rounds.");
            ability.Hidden = false; ability.ActionBarAutoFillIgnored = false;
            ability.SpellResistance = true;
            ability.Range = AbilityRange.Custom;
            ability.CustomRange = new Kingmaker.Utility.Feet(suffix == "ConeOfCold" ? 60 :
                400 + 40 * IceDevilRules40Policy.SpellLikeCasterLevel);
            var run = ability.GetComponent<AbilityEffectRunAction>();
            var damage = run.Actions.Actions.OfType<ContextActionDealDamage>().ToArray();
            if (damage.Length != (suffix == "ConeOfCold" ? 1 : 2))
                throw new InvalidOperationException("Native cold spell packet graph changed.");
            foreach (var packet in damage) packet.Value.DiceType = DiceType.D6;
            if (suffix == "ConeOfCold")
            {
                var delivery = ability.GetComponent<AbilityDeliverProjectile>();
                if (delivery == null) throw new InvalidOperationException("Native Cone delivery absent.");
                delivery.Length = new Kingmaker.Utility.Feet(60);
                foreach (var rank in ability.GetComponents<ContextRankConfig>())
                {
                    SetField(rank, "m_Feature", null);
                    SetField(rank, "m_UseMax", true); SetField(rank, "m_Max", 15);
                }
            }
            else
            {
                SetField(ability.GetComponent<AbilityTargetsAround>(), "m_Radius", new Kingmaker.Utility.Feet(20));
                var area = Require<BlueprintAbilityAreaEffect>(symbols, p + "IceStormArea");
                var nativeArea = BlueprintLibraryLookup.RequireExact<BlueprintAbilityAreaEffect>(
                    library, NativeIceStormArea40, "native Ice Storm terrain area");
                CopyFields(nativeArea, area); area.name = InternalName(p + "IceStormArea");
                area.ComponentsArray = nativeArea.ComponentsArray.Select(
                    ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
                area.Size = new Kingmaker.Utility.Feet(20);
                var lifetime = ScriptableObject.CreateInstance<IceDevilStormModuleLifetime40>();
                lifetime.OwningSpell = ability;
                area.ComponentsArray = area.ComponentsArray.Concat(new BlueprintComponent[] { lifetime }).ToArray();
                var spawn = ability.GetComponent<AbilityEffectRunActionOnClickedTarget>()
                    .Action.Actions.OfType<ContextActionSpawnAreaEffect>().Single();
                spawn.AreaEffect = area;
            }
            AlternateSummoningAdapterBuilder.ResetComponentCaches(ability);
            return ability;
        }

        private static BlueprintBuff ConfigureIceDevilFear40(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> symbols, BlueprintUnit unit)
        {
            string p = IceDevil40Prefix;
            var frightened = InfernalFrightened35(library, symbols, p + "FearFrightened", "ice-devil");
            var memory = Require<BlueprintBuff>(symbols, p + "FearImmunity");
            memory.name = InternalName(p + "FearImmunity"); memory.Stacking = StackingType.Stack;
            SetBuffFlags(memory, false, true); memory.ComponentsArray = Array.Empty<BlueprintComponent>();
            BlueprintUnitFactAccess.Resolve().Configure(memory,
                LocalizationService.Create(p + "FearImmunity.Name", "Gelugon Fear Memory"),
                LocalizationService.Create(p + "FearImmunity.Description", "Exact-source fear repeat protection."), null);
            var area = Require<BlueprintAbilityAreaEffect>(symbols, p + "FearArea");
            area.name = InternalName(p + "FearArea"); area.Shape = AreaEffectShape.Cylinder;
            area.Size = new Kingmaker.Utility.Feet(IceDevilRules40Policy.FearRadiusFeet);
            area.AffectEnemies = true; area.AffectDead = false; area.AggroEnemies = false;
            area.IgnoreSleepingUnits = false; area.SpellResistance = false;
            area.Fx = new Kingmaker.ResourceLinks.PrefabLink();
            var aura = ScriptableObject.CreateInstance<BoneFearAura35>();
            aura.OwningBlueprint = unit; aura.HitDice = IceDevilRules40Policy.HitDice;
            aura.Frightened = frightened; aura.SourceMemory = memory;
            area.ComponentsArray = new BlueprintComponent[] { aura };
            var carrier = Require<BlueprintBuff>(symbols, p + "FearCarrier");
            carrier.name = InternalName(p + "FearCarrier"); carrier.Stacking = StackingType.Replace;
            SetBuffFlags(carrier, false, true);
            var emanation = ScriptableObject.CreateInstance<AddAreaEffect>(); emanation.AreaEffect = area;
            carrier.ComponentsArray = new BlueprintComponent[] { emanation };
            BlueprintUnitFactAccess.Resolve().Configure(carrier,
                LocalizationService.Create(p + "FearCarrier.Name", "Fear Aura"),
                LocalizationService.Create(p + "FearCarrier.Description", "10-foot fear aura; Will DC22 from live Charisma, native 1d6 rounds and source-specific repeat protection."),
                ExpandedSummoningProjectIcons.Require("ice-devil"));
            return carrier;
        }

        private static BlueprintBuff ConfigureIceDevilRegeneration40(
            IDictionary<string, BlueprintScriptableObject> symbols, BlueprintUnit unit)
        {
            string p = IceDevil40Prefix;
            KytonRegeneration34.VerifyContract();
            var native = ScriptableObject.CreateInstance<KytonRegeneration34>();
            native.OwningBlueprint = unit; native.GoodOnly = true;
            native.Heal = IceDevilRules40Policy.Regeneration; native.Unremovable = false;
            native.CancelByMagicWeapon = false;
            native.CancelDamageEnergyTypes = Array.Empty<DamageEnergyType>();
            native.CancelDamageAlignmentTypes = new[] { DamageAlignment.Good };
            native.CancelDamageMaterials = Array.Empty<PhysicalDamageMaterial>();
            var buff = Require<BlueprintBuff>(symbols, p + "Regeneration");
            buff.name = InternalName(p + "Regeneration"); buff.Frequency = DurationRate.Rounds;
            buff.Stacking = StackingType.Replace; SetBuffFlags(buff, false, true);
            buff.ComponentsArray = new BlueprintComponent[] { native };
            BlueprintUnitFactAccess.Resolve().Configure(buff,
                LocalizationService.Create(p + "Regeneration.Name", "Regeneration"),
                LocalizationService.Create(p + "Regeneration.Description", "Regeneration 5; good weapons and good spells suppress it for the following round."),
                ExpandedSummoningProjectIcons.Require("ice-devil"));
            return buff;
        }
    }
}
