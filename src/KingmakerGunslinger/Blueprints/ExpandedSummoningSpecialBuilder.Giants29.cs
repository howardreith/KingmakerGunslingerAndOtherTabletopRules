using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Localization;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
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
        private const string Giant29Prefix = GiantPrefix + "Sprint29.";
        private const string Cloud29Unit = "KMG.Summoning.Unit.CloudGiant";
        private const string Storm29Unit = "KMG.Summoning.Unit.StormGiant";
        private const string Ettin29Unit = "KMG.Summoning.Unit.Ettin";
        private const string NativeMorningstar29 =
            "2b6874ccbee523640819f8a8d66167b6";
        private const string NativeChainshirt29 =
            "c65f6fc979d5556489b20e478189cbdd";
        private const string NativeObscuringMist29 =
            "15ba44b3fcf48ce418dc72fadac6408a";
        private const string NativeImprovedBullRush29 =
            "b3614622866fe7046b787a548bbd7f59";
        private const string Awesome29Action =
            Giant29Prefix + "AwesomeBlowAction";
        private const string Awesome29Feature =
            Giant29Prefix + "AwesomeBlowFeature";

        // Each new private identity is appended after the released Sprint 28
        // tail. No donor or campaign blueprint is patched in place.
        private static void ConfigureSprint29Giants(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol,
            BlueprintFeature extraplanar)
        {
            ConfigureSprint29AwesomeBlow(library, bySymbol);
            ConfigureSprint29Cloud(library, bySymbol, extraplanar);
            ConfigureSprint29Storm(library, bySymbol, extraplanar);
            ConfigureSprint29Ettin(library, bySymbol, extraplanar);
        }

        private static void ConfigureSprint29Ettin(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol,
            BlueprintFeature extraplanar)
        {
            const string flailType = Giant29Prefix + "EttinFlailType";
            const string mainFlailSymbol =
                Giant29Prefix + "EttinMainFlail2d6";
            const string offFlailSymbol =
                Giant29Prefix + "EttinOffFlail2d6";
            const string javelinType = Giant29Prefix + "EttinJavelinType";
            const string mainJavelinSymbol =
                Giant29Prefix + "EttinMainJavelin1d8";
            const string offJavelinSymbol =
                Giant29Prefix + "EttinOffJavelin1d8";
            const string armorSymbol = Giant29Prefix + "EttinLeather";
            const string naturalSymbol = Giant29Prefix + "EttinNaturalArmor8";
            const string typeSymbol = Giant29Prefix + "EttinUnitType";
            const string traitsSymbol = Giant29Prefix + "EttinTraits";

            BlueprintItemWeapon mainFlail = Sprint29Weapon(library, bySymbol,
                mainFlailSymbol, flailType,
                "6cba82e1188132c46b4656b0cefeb9b0",
                Size.Large, 2, DiceType.D6, 10);
            BlueprintItemWeapon offFlail = Sprint29Weapon(library, bySymbol,
                offFlailSymbol, flailType,
                "6cba82e1188132c46b4656b0cefeb9b0",
                Size.Large, 2, DiceType.D6, 10);
            BlueprintItemWeapon mainJavelin = Sprint29Weapon(library,
                bySymbol, mainJavelinSymbol, javelinType,
                "e24f753bbbbefb84cb94223df00d837a",
                Size.Large, 1, DiceType.D8, 0);
            BlueprintItemWeapon offJavelin = Sprint29Weapon(library,
                bySymbol, offJavelinSymbol, javelinType,
                "e24f753bbbbefb84cb94223df00d837a",
                Size.Large, 1, DiceType.D8, 0);
            SetField(mainJavelin, "m_IsStackable", false);
            SetField(offJavelin, "m_IsStackable", false);
            BlueprintItemArmor leather = GiantArmor(library, bySymbol,
                armorSymbol, "9f76e9a3353e914479c5ddb4b4a82fb4");
            BlueprintFeature natural = Require<BlueprintFeature>(bySymbol,
                naturalSymbol);
            Sprint29NaturalArmor(natural, naturalSymbol, 8,
                "Ettin Natural Armor");
            ConfigureGiantUnit(library, bySymbol, Ettin29Unit,
                typeSymbol, traitsSymbol, "Ettin", "Ettin",
                Alignment.ChaoticEvil, 10, 23, 8, 15, 6, 10, 11,
                45, 7, 3, 3, 5, 0, 0,
                mainFlail, null, null, leather, natural, extraplanar,
                new[] { ImprovedInitiativeGuid, IronWillGuid,
                    PowerAttackGuid }, false);
            BlueprintUnit unit = Require<BlueprintUnit>(bySymbol, Ettin29Unit);
            unit.Body.SecondaryHand = offFlail;
            unit.Body.PrimaryHandAlternative1 = mainJavelin;
            unit.Body.SecondaryHandAlternative1 = offJavelin;
            unit.Body.PrimaryHandAlternative2 = null;
            unit.Body.SecondaryHandAlternative2 = null;
            unit.Body.EmptyHandWeapon = null;
            BlueprintFeature traits = Require<BlueprintFeature>(bySymbol,
                traitsSymbol);
            AddProficiencies proficiency = traits.GetComponents<
                AddProficiencies>().Single();
            proficiency.WeaponProficiencies = new[] {
                mainFlail.Category, mainJavelin.Category };
            SummonGiantEquipmentOwnership equipment = traits.GetComponents<
                SummonGiantEquipmentOwnership>().Single();
            equipment.OffhandWeapon = offFlail;
            equipment.RangedWeapon = mainJavelin;
            equipment.OffhandRangedWeapon = offJavelin;
            var strength = ScriptableObject.CreateInstance<
                SummonEttinOffhandStrength>();
            strength.OwningBlueprint = unit;
            strength.OffhandFlail = offFlail;
            strength.OffhandJavelin = offJavelin;
            var perception = ScriptableObject.CreateInstance<AddStatBonus>();
            perception.Stat = StatType.SkillPerception;
            perception.Descriptor = ModifierDescriptor.Racial;
            perception.Value = 4;
            traits.ComponentsArray = traits.ComponentsArray.Where(value =>
                !(value is SummonGiantRockAttack) &&
                !(value is SummonGiantRockCatch)).Concat(
                    new BlueprintComponent[] { strength, perception }).ToArray();
            unit.AddFacts = unit.AddFacts.Concat(new BlueprintUnitFact[] {
                Require<BlueprintFeature>(bySymbol, GiantCleaveFeature),
                Require<BlueprintFeature>(bySymbol, FireImprovedOverrun)
            }).ToArray();
        }

        private static void ConfigureSprint29Storm(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol,
            BlueprintFeature extraplanar)
        {
            const string swordSymbol = Giant29Prefix + "StormGreatsword4d6";
            const string swordType = Giant29Prefix + "StormGreatswordType";
            const string slamSymbol = Giant29Prefix + "StormSlam2d6";
            const string slamType = Giant29Prefix + "StormSlamType";
            const string bowSymbol = Giant29Prefix + "StormCompositeBow3d6";
            const string bowType = Giant29Prefix + "StormCompositeBowType";
            const string armorSymbol = Giant29Prefix + "StormBreastplate";
            const string naturalSymbol = Giant29Prefix + "StormNaturalArmor12";
            const string typeSymbol = Giant29Prefix + "StormUnitType";
            const string traitsSymbol = Giant29Prefix + "StormTraits";
            const string improvedVitalSymbol =
                Giant29Prefix + "StormImprovedVitalStrike";
            const string brainSymbol = Giant29Prefix + "StormBrain";
            const string callAiSymbol = Giant29Prefix + "StormCallAi";
            const string boltAiSymbol = Giant29Prefix + "StormBoltAi";
            const string chainAiSymbol = Giant29Prefix + "StormChainAi";
            const string lightningSafetySymbol =
                Giant29Prefix + "StormLightningSafety";
            const string callSymbol = Giant29Prefix + "StormCallLightning";
            const string boltSymbol = Giant29Prefix + "StormLightningBolt";
            const string stateSymbol = Giant29Prefix + "StormLightningState";
            const string callResourceSymbol = Giant29Prefix + "StormCallResource";
            const string chainSymbol = Giant29Prefix + "StormChainLightning";
            const string chainResourceSymbol = Giant29Prefix + "StormChainResource";

            BlueprintItemWeapon sword = Sprint29Weapon(library, bySymbol,
                swordSymbol, swordType, NativeGreatsword, Size.Huge,
                4, DiceType.D6, 15);
            BlueprintItemWeapon slam = Sprint29Weapon(library, bySymbol,
                slamSymbol, slamType, NativeSlam, Size.Huge,
                2, DiceType.D6, 15);
            BlueprintItemWeapon bow = Sprint29Weapon(library, bySymbol,
                bowSymbol, bowType, "7998cd1409fe1194583b64180df4f216",
                Size.Huge, 3, DiceType.D6, 0);
            BlueprintWeaponEnchantment masterwork =
                BlueprintLibraryLookup.RequireExact<BlueprintWeaponEnchantment>(
                    library, EasternWeaponBlueprints.NativeMasterworkGuid,
                    "native masterwork accuracy for Storm armament");
            SetField(sword, "m_Enchantments", new[] { masterwork });
            SetField(bow, "m_Enchantments", new[] { masterwork });
            // Its feat doubles the native greatsword threat range. The
            // project clone has a separate weapon-type identity, so install
            // the resulting 17-20 range on that private type once.
            SetField(sword.Type, "m_CriticalRollEdge", 17);
            BlueprintItemArmor breastplate = GiantArmor(library, bySymbol,
                armorSymbol, "9809987cc12d94545a64ff20e6fdb216");
            SetField(breastplate, "m_Size", Size.Huge);
            BlueprintFeature natural = Require<BlueprintFeature>(bySymbol,
                naturalSymbol);
            Sprint29NaturalArmor(natural, naturalSymbol, 12,
                "Storm Giant Natural Armor");
            Sprint29ImprovedVitalStrike(library,
                Require<BlueprintFeature>(bySymbol, improvedVitalSymbol),
                improvedVitalSymbol);

            ConfigureGiantUnit(library, bySymbol, Storm29Unit,
                typeSymbol, traitsSymbol, "StormGiant", "Storm Giant",
                Alignment.ChaoticGood, 19, 39, 14, 23, 16, 20, 15,
                85, 11, 6, 6, 19, 15, 0,
                sword, slam, null, breastplate, natural, extraplanar,
                new[] { "0f8939ae6f220984e8fb568abbdfba95",
                    NativeImprovedBullRush29,
                    "6b6e53330d0739d41bc50d7b4ca5ceea",
                    NativeImprovedSunder,
                    "14a1fc1356df9f146900e1e42142fc9d",
                    IronWillGuid, PowerAttackGuid }, false,
                new[] { ElectricityImmunityGuid });
            BlueprintUnit unit = Require<BlueprintUnit>(bySymbol, Storm29Unit);
            unit.Size = Size.Huge;
            unit.Speed = new Kingmaker.Utility.Feet(50);
            unit.GetComponents<SummonGiantRacialProfile>().Single()
                .MobilityRanks = 13;
            unit.Body.PrimaryHandAlternative2 = bow;
            BlueprintFeature traits = Require<BlueprintFeature>(bySymbol,
                traitsSymbol);
            AddProficiencies proficiency = traits.GetComponents<
                AddProficiencies>().Single();
            proficiency.WeaponProficiencies = new[] {
                sword.Category, bow.Category };
            SummonGiantEquipmentOwnership equipment = traits.GetComponents<
                SummonGiantEquipmentOwnership>().Single();
            equipment.RangedWeapon = bow;
            var rating = ScriptableObject.CreateInstance<
                SummonStormCompositeBowRating>();
            rating.OwningBlueprint = unit;
            rating.Bow = bow;
            rating.StrengthRating = 14;
            var armoredSpeed = ScriptableObject.CreateInstance<
                SummonStormBreastplateSpeed>();
            armoredSpeed.OwningBlueprint = unit;
            armoredSpeed.Breastplate = breastplate;
            traits.ComponentsArray = traits.ComponentsArray.Concat(
                new BlueprintComponent[] { rating, armoredSpeed }).ToArray();

            // Constant Freedom of Movement is carried on this summoned unit
            // only. The native buff supplies its exact movement protections.
            BlueprintBuff freedom = BlueprintLibraryLookup.RequireExact<
                BlueprintBuff>(library,
                    "1533e782fca42b84ea370fc1dcbf4fc1",
                    "native Freedom of Movement protection");
            unit.AddFacts = unit.AddFacts.Concat(new BlueprintUnitFact[] {
                Require<BlueprintFeature>(bySymbol, GiantCleaveFeature),
                Require<BlueprintFeature>(bySymbol, improvedVitalSymbol),
                Require<BlueprintFeature>(bySymbol, Awesome29Feature),
                freedom }).ToArray();

            BlueprintAbilityResource callResource =
                Require<BlueprintAbilityResource>(bySymbol, callResourceSymbol);
            BlueprintAbilityResource chainResource =
                Require<BlueprintAbilityResource>(bySymbol, chainResourceSymbol);
            ConfigureNamedResource(callResource, callResourceSymbol,
                "KMG.ExpandedSummoning.StormGiant.CallResource",
                "Call Lightning", "One activation per summoning.", 1);
            ConfigureNamedResource(chainResource, chainResourceSymbol,
                "KMG.ExpandedSummoning.StormGiant.ChainResource",
                "Chain Lightning", "One cast per summoning.", 1);
            BlueprintAbility call = Require<BlueprintAbility>(bySymbol,
                callSymbol);
            BlueprintAbility bolt = Require<BlueprintAbility>(bySymbol,
                boltSymbol);
            BlueprintBuff state = Require<BlueprintBuff>(bySymbol,
                stateSymbol);
            Sprint29CallLightning(library, call, bolt, state,
                callSymbol, boltSymbol, stateSymbol, callResource);
            BlueprintAbility chain = Require<BlueprintAbility>(bySymbol,
                chainSymbol);
            Sprint29LimitedNativeSpell(library, chain, chainSymbol,
                "645558d63604747428d55f0dd3a4cb58",
                15, 6, chainResource, "Chain Lightning");
            traits.ComponentsArray = traits.ComponentsArray.Concat(
                new BlueprintComponent[] { AddResource(callResource),
                    AddResource(chainResource) }).ToArray();
            unit.AddFacts = unit.AddFacts.Concat(new BlueprintUnitFact[] {
                call, chain }).ToArray();
            BlueprintBrain brain = Require<BlueprintBrain>(bySymbol,
                brainSymbol);
            brain.name = InternalName(brainSymbol);
            Sprint29LightningAiSafety safety = Require<
                Sprint29LightningAiSafety>(bySymbol, lightningSafetySymbol);
            safety.OwningBlueprint = unit;
            safety.KnownElectricityImmunity = BlueprintLibraryLookup.RequireExact<
                BlueprintFeature>(library, ElectricityImmunityGuid,
                    "installed electricity immunity for AI screening");
            brain.Actions = new BlueprintAiAction[] {
                ConfigureMephitAiAction(Require<BlueprintAiCastSpell>(
                    bySymbol, chainAiSymbol), chainAiSymbol, chain, 0),
                ConfigureMephitAiAction(Require<BlueprintAiCastSpell>(
                    bySymbol, callAiSymbol), callAiSymbol, call, 0),
                ConfigureMephitAiAction(Require<BlueprintAiCastSpell>(
                    bySymbol, boltAiSymbol), boltAiSymbol, bolt, 0)
            };
            foreach (BlueprintAiCastSpell action in brain.Actions.OfType<
                BlueprintAiCastSpell>())
                action.TargetConsiderations = new Kingmaker.Controllers.Brain
                    .Blueprints.Considerations.Consideration[] { safety };
            unit.Brain = brain;
        }

        private static void Sprint29ImprovedVitalStrike(
            LibraryScriptableObject library, BlueprintFeature feature,
            string symbol)
        {
            BlueprintFeature native = BlueprintLibraryLookup.RequireExact<
                BlueprintFeature>(library,
                    "52913092cd018da47845f36e6fbe240f",
                    "native Improved Vital Strike");
            BlueprintComponent[] source = native.ComponentsArray ??
                Array.Empty<BlueprintComponent>();
            if (source.Count(value => value != null &&
                value.GetType().Name == "RemoveFeatureOnApply") != 1 ||
                source.OfType<AddFacts>().Count() != 1)
                throw new InvalidOperationException(
                    "Installed Improved Vital Strike no longer has one action replacement.");
            CopyFields(native, feature);
            feature.name = InternalName(symbol);
            feature.ComponentsArray = source.Where(value => value != null &&
                value.GetType().Name != "RemoveFeatureOnApply").Select(
                    ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
        }

        private static void Sprint29LimitedNativeSpell(
            LibraryScriptableObject library, BlueprintAbility ability,
            string symbol, string nativeGuid, int casterLevel,
            int spellLevel, BlueprintAbilityResource resource,
            string title)
        {
            CloneMephitSpell(library, ability, nativeGuid,
                "native " + title + " spell effect");
            ability.name = InternalName(symbol);
            ability.Type = AbilityType.SpellLike;
            var parameters = ScriptableObject.CreateInstance<
                ContextCalculateAbilityParams>();
            parameters.StatType = StatType.Charisma;
            parameters.ReplaceCasterLevel = true;
            parameters.CasterLevel = Simple(casterLevel);
            parameters.ReplaceSpellLevel = true;
            parameters.SpellLevel = Simple(spellLevel);
            var cost = ScriptableObject.CreateInstance<AbilityResourceLogic>();
            cost.RequiredResource = resource;
            cost.IsSpendResource = true;
            cost.Amount = 1;
            ability.ComponentsArray = ability.ComponentsArray.Concat(
                new BlueprintComponent[] { parameters, cost }).ToArray();
            BlueprintUnitFactAccess.Resolve().Configure(ability,
                LocalizationService.Create(symbol + ".Name", title),
                LocalizationService.Create(symbol + ".Description",
                    title + " at caster level " + casterLevel +
                    ", once per summoning."), ability.Icon);
        }

        private static void Sprint29CallLightning(
            LibraryScriptableObject library, BlueprintAbility activation,
            BlueprintAbility bolt, BlueprintBuff state,
            string activationSymbol, string boltSymbol, string stateSymbol,
            BlueprintAbilityResource resource)
        {
            const string nativeStateGuid =
                "dd91a3c3df275984592edcadb8e90749";
            Sprint29LimitedNativeSpell(library, activation,
                activationSymbol,
                "2a9ef0e0b5822a24d88b16673a267456",
                15, 3, resource, "Call Lightning");
            CloneMephitSpell(library, bolt,
                "0bd54216d38852947930320f6269a9d7",
                "native Call Lightning bolt action");
            bolt.name = InternalName(boltSymbol);
            bolt.Type = AbilityType.SpellLike;
            var parameters = ScriptableObject.CreateInstance<
                ContextCalculateAbilityParams>();
            parameters.StatType = StatType.Charisma;
            parameters.ReplaceCasterLevel = true;
            parameters.CasterLevel = Simple(15);
            parameters.ReplaceSpellLevel = true;
            parameters.SpellLevel = Simple(3);
            bolt.ComponentsArray = bolt.ComponentsArray.Concat(
                new BlueprintComponent[] { parameters }).ToArray();
            BlueprintUnitFactAccess.Resolve().Configure(bolt,
                LocalizationService.Create(boltSymbol + ".Name",
                    "Call Lightning Bolt"),
                LocalizationService.Create(boltSymbol + ".Description",
                    "Use one remaining bolt from this giant's active Call Lightning effect."),
                bolt.Icon);
            BlueprintBuff nativeState = BlueprintLibraryLookup.RequireExact<
                BlueprintBuff>(library, nativeStateGuid,
                    "native Call Lightning activation state");
            CopyFields(nativeState, state);
            state.name = InternalName(stateSymbol);
            state.ComponentsArray = (nativeState.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Select(
                    ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            AddFacts grant = state.GetComponents<AddFacts>().Single();
            if (grant.Facts == null || grant.Facts.Length != 1 ||
                grant.Facts[0].AssetGuid !=
                    "0bd54216d38852947930320f6269a9d7")
                throw new InvalidOperationException(
                    "Native Call Lightning state changed its bolt grant.");
            grant.Facts = new BlueprintUnitFact[] { bolt };
            BlueprintComponent replacement = state.ComponentsArray.Single(value =>
                value != null && value.GetType().Name ==
                    "ReplaceAbilityParamsWithContext");
            SetField(replacement, "Ability", bolt);
            ContextActionApplyBuff apply = activation.ComponentsArray
                .OfType<AbilityEffectRunAction>()
                .SelectMany(value => value.Actions.Actions)
                .OfType<ContextActionApplyBuff>().Single();
            if (apply.Buff == null || apply.Buff.AssetGuid != nativeStateGuid)
                throw new InvalidOperationException(
                    "Native Call Lightning activation no longer applies its state.");
            apply.Buff = state;
            ContextActionReduceBuffDuration reduce = bolt.ComponentsArray
                .OfType<AbilityExecuteActionOnCast>()
                .SelectMany(value => value.Actions.Actions)
                .OfType<ContextActionReduceBuffDuration>().Single();
            if (reduce.TargetBuff == null ||
                reduce.TargetBuff.AssetGuid != nativeStateGuid)
                throw new InvalidOperationException(
                    "Native Call Lightning bolt no longer spends state duration.");
            reduce.TargetBuff = state;
        }

        private static void ConfigureSprint29Cloud(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol,
            BlueprintFeature extraplanar)
        {
            const string morningstarSymbol = Giant29Prefix + "CloudMorningstar4d6";
            const string morningstarType = Giant29Prefix + "CloudMorningstarType";
            const string slamSymbol = GiantPrefix + "CloudSlam2d6";
            const string slamType = Giant29Prefix + "CloudSlamType";
            const string rockSymbol = GiantPrefix + "CloudRock2d6";
            const string rockType = GiantPrefix + "CloudRockType";
            const string armorSymbol = Giant29Prefix + "CloudChainshirt";
            const string naturalSymbol = Giant29Prefix + "CloudNaturalArmor12";
            const string typeSymbol = Giant29Prefix + "CloudUnitType";
            const string traitsSymbol = Giant29Prefix + "CloudTraits";
            const string mistSymbol = Giant29Prefix + "CloudObscuringMist";
            const string fogSymbol = Giant29Prefix + "CloudFogCloud";
            const string fogResourceSymbol = Giant29Prefix + "CloudFogResource";
            const string brainSymbol = Giant29Prefix + "CloudBrain";
            const string mistAiSymbol = Giant29Prefix + "CloudMistAi";
            const string fogAiSymbol = Giant29Prefix + "CloudFogAi";
            const string mistSafetySymbol = Giant29Prefix + "CloudMistSafety";
            const string fogSafetySymbol = Giant29Prefix + "CloudFogSafety";

            BlueprintItemWeapon morningstar = Sprint29Weapon(library,
                bySymbol, morningstarSymbol, morningstarType,
                NativeMorningstar29, Size.Gargantuan, 4, DiceType.D6, 15);
            // Kingmaker's installed morningstar items use HeavyMace as their
            // type. Preserve that native animation/weapon action, but the
            // project clone carries the printed bludgeoning/piercing form.
            SetField(morningstar, "m_OverrideDamageType", true);
            SetField(morningstar, "m_DamageType", new DamageTypeDescription {
                Type = DamageType.Physical,
                Common = new DamageTypeDescription.CommomData(),
                Physical = new DamageTypeDescription.PhysicalData {
                    Form = PhysicalDamageForm.Bludgeoning |
                        PhysicalDamageForm.Piercing }
            });
            BlueprintItemWeapon slam = Sprint29Weapon(library, bySymbol,
                slamSymbol, slamType, NativeSlam, Size.Huge, 2,
                DiceType.D6, 15);
            BlueprintItemWeapon rock = GiantRock(library, bySymbol,
                rockSymbol, rockType, 140);
            SetField(rock, "m_Size", Size.Huge);
            SetField(rock, "m_DamageDice", new DiceFormula(2, DiceType.D6));
            SetField(rock.Type, "m_BaseDamage", new DiceFormula(2, DiceType.D6));
            BlueprintItemArmor chainshirt = GiantArmor(library, bySymbol,
                armorSymbol, NativeChainshirt29);
            SetField(chainshirt, "m_Size", Size.Huge);

            BlueprintFeature natural = Require<BlueprintFeature>(bySymbol,
                naturalSymbol);
            Sprint29NaturalArmor(natural, naturalSymbol, 12,
                "Cloud Giant Natural Armor");
            ConfigureGiantUnit(library, bySymbol, Cloud29Unit,
                typeSymbol, traitsSymbol, "CloudGiant", "Cloud Giant",
                Alignment.NeutralGood, 16, 35, 13, 23, 12, 16, 12,
                72, 10, 5, 5, 11, 8, 0,
                morningstar, slam, rock, chainshirt, natural, extraplanar,
                new[] { NativeGreatCleaveFeature, NativeImprovedBullRush29,
                    NativeIntimidatingProwess, IronWillGuid, PowerAttackGuid },
                false);
            BlueprintUnit unit = Require<BlueprintUnit>(bySymbol, Cloud29Unit);
            unit.Size = Size.Huge;
            unit.Speed = new Kingmaker.Utility.Feet(50);
            // The first giant builder grants private Cleave only to its two
            // released owners. Cloud has Great Cleave and uses that same
            // qualified private action with the greater-capable discriminator.
            unit.AddFacts = unit.AddFacts.Concat(new BlueprintUnitFact[] {
                Require<BlueprintFeature>(bySymbol, GiantCleaveFeature),
                Require<BlueprintFeature>(bySymbol, FireImprovedOverrun),
                Require<BlueprintFeature>(bySymbol, Awesome29Feature)
            }).ToArray();
            BlueprintFeature traits = Require<BlueprintFeature>(bySymbol,
                traitsSymbol);
            var oversized = ScriptableObject.CreateInstance<
                SummonCloudOversizedMorningstar>();
            oversized.OwningBlueprint = unit;
            oversized.Morningstar = morningstar;
            var intimidateRanks = ScriptableObject.CreateInstance<
                SummonCloudIntimidateRanks>();
            intimidateRanks.OwningBlueprint = unit;
            traits.ComponentsArray = traits.ComponentsArray.Concat(
                new BlueprintComponent[] { oversized, intimidateRanks })
                .ToArray();

            BlueprintAbility mist = Require<BlueprintAbility>(bySymbol,
                mistSymbol);
            BlueprintAbility fog = Require<BlueprintAbility>(bySymbol,
                fogSymbol);
            BlueprintAbilityResource resource = Require<BlueprintAbilityResource>(
                bySymbol, fogResourceSymbol);
            Sprint29CloneFogSpell(library, mist, mistSymbol, false, null);
            ConfigureNamedResource(resource, fogResourceSymbol,
                "KMG.ExpandedSummoning.CloudGiant.FogResource",
                "Fog Cloud", "One fog cloud per summoning.", 1);
            Sprint29CloneFogSpell(library, fog, fogSymbol, true, resource);
            traits.ComponentsArray = traits.ComponentsArray.Concat(
                new BlueprintComponent[] { AddResource(resource) }).ToArray();
            unit.AddFacts = unit.AddFacts.Concat(new BlueprintUnitFact[] {
                mist, fog }).ToArray();
            BlueprintAbilityAreaEffect area = BlueprintLibraryLookup.RequireExact<
                BlueprintAbilityAreaEffect>(library,
                    "e09010a73354a794293ebc7b33c2d130",
                    "Cloud fog all-creature area");
            Sprint29FogAiSafety mistSafety = Require<Sprint29FogAiSafety>(
                bySymbol, mistSafetySymbol);
            mistSafety.OwningBlueprint = unit;
            mistSafety.FogArea = area;
            mistSafety.CenterOnCaster = true;
            Sprint29FogAiSafety fogSafety = Require<Sprint29FogAiSafety>(
                bySymbol, fogSafetySymbol);
            fogSafety.OwningBlueprint = unit;
            fogSafety.FogArea = area;
            fogSafety.CenterOnCaster = false;
            BlueprintAiCastSpell fogAi = ConfigureMephitAiAction(
                Require<BlueprintAiCastSpell>(bySymbol, fogAiSymbol),
                fogAiSymbol, fog, 0);
            fogAi.TargetConsiderations = new Kingmaker.Controllers.Brain
                .Blueprints.Considerations.Consideration[] { fogSafety };
            BlueprintAiCastSpell mistAi = ConfigureMephitAiAction(
                Require<BlueprintAiCastSpell>(bySymbol, mistAiSymbol),
                mistAiSymbol, mist, 100);
            mistAi.ActorConsiderations = new Kingmaker.Controllers.Brain
                .Blueprints.Considerations.Consideration[] { mistSafety };
            BlueprintBrain brain = Require<BlueprintBrain>(bySymbol,
                brainSymbol);
            brain.name = InternalName(brainSymbol);
            brain.Actions = new BlueprintAiAction[] { fogAi, mistAi };
            unit.Brain = brain;
        }

        private static BlueprintItemWeapon Sprint29Weapon(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol,
            string symbol, string typeSymbol, string donorGuid, Size size,
            int dice, DiceType die, int rangeFeet)
        {
            BlueprintItemWeapon donor = BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, donorGuid, symbol + " donor");
            BlueprintWeaponType type = Require<BlueprintWeaponType>(bySymbol,
                typeSymbol);
            CopyFields(donor.Type, type);
            type.name = InternalName(typeSymbol);
            type.ComponentsArray = (donor.Type.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Select(
                    ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            if (rangeFeet > 0)
                SetField(type, "m_AttackRange",
                    new Kingmaker.Utility.Feet(rangeFeet));
            BlueprintItemWeapon item = Require<BlueprintItemWeapon>(bySymbol,
                symbol);
            CopyFields(donor, item);
            item.name = InternalName(symbol);
            item.ComponentsArray = (donor.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Select(
                    ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            SetField(item, "m_Type", type);
            SetField(item, "m_Size", size);
            SetField(item, "m_OverrideDamageDice", true);
            SetField(item, "m_DamageDice", new DiceFormula(dice, die));
            SetField(item, "m_Enchantments",
                Array.Empty<BlueprintWeaponEnchantment>());
            SetField(item, "m_Cost", 0);
            item.IsNonRemovable = true;
            return item;
        }

        private static void ConfigureSprint29AwesomeBlow(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            BlueprintAbility native = BlueprintLibraryLookup.RequireExact<
                BlueprintAbility>(library, NativeCleaveAction,
                    "native standard melee ability carrier");
            BlueprintAbility action = Require<BlueprintAbility>(bySymbol,
                Awesome29Action);
            CopyFields(native, action);
            action.name = InternalName(Awesome29Action);
            action.Type = AbilityType.Extraordinary;
            action.ActionType = Kingmaker.UnitLogic.Commands.Base.UnitCommand
                .CommandType.Standard;
            action.Range = AbilityRange.Touch;
            action.CanTargetEnemies = true;
            action.CanTargetFriends = false;
            action.CanTargetSelf = false;
            action.CanTargetPoint = false;
            var impact = ScriptableObject.CreateInstance<
                Sprint29AwesomeBlow>();
            impact.Cloud = Require<BlueprintUnit>(bySymbol, Cloud29Unit);
            impact.Storm = Require<BlueprintUnit>(bySymbol, Storm29Unit);
            impact.Incorporeal = BlueprintLibraryLookup.RequireExact<
                BlueprintFeature>(library, IncorporealGuid,
                    "native incorporeal exclusion for Awesome Blow");
            impact.Knockdown = new ActionList { Actions = new GameAction[] {
                ScriptableObject.CreateInstance<ContextActionKnockdownTarget>()
            } };
            var run = ScriptableObject.CreateInstance<
                AbilityEffectRunAction>();
            run.Actions = new ActionList { Actions = new GameAction[] {
                impact
            } };
            action.ComponentsArray = new BlueprintComponent[] { run };
            BlueprintUnitFactAccess.Resolve().Configure(action,
                LocalizationService.Create(Awesome29Action + ".Name",
                    "Awesome Blow"),
                LocalizationService.Create(Awesome29Action + ".Description",
                    "Standard action: make a combat maneuver against a smaller corporeal foe; on success deal slam damage, push 10 feet and knock prone."),
                native.Icon);
            BlueprintFeature feature = Require<BlueprintFeature>(bySymbol,
                Awesome29Feature);
            feature.name = InternalName(Awesome29Feature);
            feature.IsClassFeature = false;
            feature.HideInUI = false;
            var grant = ScriptableObject.CreateInstance<AddFacts>();
            grant.Facts = new BlueprintUnitFact[] { action };
            feature.ComponentsArray = new BlueprintComponent[] { grant };
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create(Awesome29Feature + ".Name",
                    "Awesome Blow"),
                LocalizationService.Create(Awesome29Feature +
                    ".Description", "Grants the owned Awesome Blow action."),
                native.Icon);
        }

        private static void Sprint29NaturalArmor(BlueprintFeature feature,
            string symbol, int amount, string title)
        {
            var bonus = ScriptableObject.CreateInstance<AddStatBonus>();
            bonus.Stat = StatType.AC;
            bonus.Descriptor = ModifierDescriptor.NaturalArmor;
            bonus.Value = amount;
            feature.name = InternalName(symbol);
            feature.IsClassFeature = true;
            feature.HideInUI = true;
            feature.ComponentsArray = new BlueprintComponent[] { bonus };
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create(symbol + ".Name", title),
                LocalizationService.Create(symbol + ".Description",
                    "The adult giant's +" + amount + " natural armor."), null);
        }

        private static void Sprint29CloneFogSpell(
            LibraryScriptableObject library, BlueprintAbility ability,
            string symbol, bool remote, BlueprintAbilityResource resource)
        {
            CloneMephitSpell(library, ability, NativeObscuringMist29,
                "native Obscuring Mist area/effect carrier");
            ability.name = InternalName(symbol);
            ability.Type = AbilityType.SpellLike;
            var components = new List<BlueprintComponent>(
                ability.ComponentsArray ?? Array.Empty<BlueprintComponent>());
            var parameters = ScriptableObject.CreateInstance<
                ContextCalculateAbilityParams>();
            parameters.StatType = StatType.Charisma;
            parameters.ReplaceCasterLevel = true;
            parameters.CasterLevel = Simple(16);
            parameters.ReplaceSpellLevel = true;
            parameters.SpellLevel = Simple(remote ? 2 : 1);
            components.Add(parameters);
            ContextActionSpawnAreaEffect spawn = FindSpawnAreaEffect(ability);
            if (spawn == null || spawn.AreaEffect == null ||
                spawn.AreaEffect.AssetGuid !=
                    "e09010a73354a794293ebc7b33c2d130")
                throw new InvalidOperationException(
                    "Installed Obscuring Mist no longer spawns the audited area.");
            if (remote)
            {
                ability.CanTargetPoint = true;
                ability.CanTargetSelf = false;
                ability.CanTargetFriends = false;
                ability.CanTargetEnemies = false;
                ability.Range = AbilityRange.Medium;
                spawn.OnUnit = false;
                spawn.DurationValue = new ContextDurationValue {
                    Rate = DurationRate.Minutes, DiceType = DiceType.Zero,
                    DiceCountValue = Simple(0), BonusValue = Simple(160) };
                var cost = ScriptableObject.CreateInstance<
                    AbilityResourceLogic>();
                cost.RequiredResource = resource;
                cost.IsSpendResource = true;
                cost.Amount = 1;
                components.Add(cost);
            }
            ability.ComponentsArray = components.ToArray();
            BlueprintUnitFactAccess.Resolve().Configure(ability,
                LocalizationService.Create(symbol + ".Name",
                    remote ? "Fog Cloud" : "Obscuring Mist"),
                LocalizationService.Create(symbol + ".Description",
                    remote ? "Once per summoning, create a stationary 20-foot obscuring fog at range for 160 minutes (CL 16)." :
                        "At will, surround yourself with a 20-foot obscuring mist for 16 minutes (CL 16)."),
                ability.Icon);
        }
    }
}
