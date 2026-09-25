using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Localization;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static class ExpandedSummoningSpecialBuilder
    {
        private const string LanternUnitSymbol =
            "KMG.Summoning.Unit.LanternArchon";
        private const string LanternRaySymbol =
            "KMG.Summoning.Special.LanternArchon.LightRay";
        private const string LanternAiSymbol =
            "KMG.Summoning.Special.LanternArchon.LightRayAi";
        private const string LanternBrainSymbol =
            "KMG.Summoning.Special.LanternArchon.Brain";
        private const string LanternDefenseSymbol =
            "KMG.Summoning.Special.LanternArchon.Defenses";
        private const string InvisibleStalkerUnitSymbol =
            "KMG.Summoning.Unit.InvisibleStalker";
        private const string ErinyesUnitSymbol =
            "KMG.Summoning.Unit.ErinyesDevil";
        private const string ShadowDemonUnitSymbol =
            "KMG.Summoning.Unit.ShadowDemon";
        private const string ShadowDemonCombatTraitsSymbol =
            "KMG.Summoning.Special.ShadowDemon.CombatTraits";
        private const string SalamanderUnitSymbol =
            "KMG.Summoning.Unit.Salamander";
        private const string SalamanderSpearTypeSymbol =
            "KMG.Summoning.Special.Salamander.SpearType";
        private const string SalamanderSpearSymbol =
            "KMG.Summoning.Special.Salamander.Spear";
        private const string SalamanderTailSymbol =
            "KMG.Summoning.Special.Salamander.Tail";
        private const string SalamanderCombatTraitsSymbol =
            "KMG.Summoning.Special.Salamander.CombatTraits";
        private const string SuccubusUnitSymbol =
            "KMG.Summoning.Unit.Succubus";
        private const string SuccubusDominateSymbol =
            "KMG.Summoning.Special.Succubus.Dominate";
        private const string SuccubusDominationSymbol =
            "KMG.Summoning.Special.Succubus.Domination";
        private const string SuccubusDominateAiSymbol =
            "KMG.Summoning.Special.Succubus.DominateAi";
        private const string SuccubusBrainSymbol =
            "KMG.Summoning.Special.Succubus.Brain";
        private const string SuccubusCombatTraitsSymbol =
            "KMG.Summoning.Special.Succubus.CombatTraits";
        private const string BebelithUnitSymbol =
            "KMG.Summoning.Unit.Bebelith";
        private const string BebelithClawSymbol =
            "KMG.Summoning.Special.Bebelith.Claw";
        private const string BebelithCombatTraitsSymbol =
            "KMG.Summoning.Special.Bebelith.CombatTraits";
        private const string BebelithDismantledArmorSymbol =
            "KMG.Summoning.Special.Bebelith.DismantledArmor";
        private const string PixieUnitSymbol = "KMG.Summoning.Unit.Pixie";
        private const string PixieSleepBowTypeSymbol =
            "KMG.Summoning.Special.Pixie.SleepBowType";
        private const string PixieSleepBowSymbol =
            "KMG.Summoning.Special.Pixie.SleepBow";
        private const string PixieDanceSymbol =
            "KMG.Summoning.Special.Pixie.IrresistibleDance";
        private const string PixieDanceStateSymbol =
            "KMG.Summoning.Special.Pixie.IrresistibleDanceState";
        private const string PixieDanceResourceSymbol =
            "KMG.Summoning.Special.Pixie.IrresistibleDanceResource";
        private const string PixieSleepResourceSymbol =
            "KMG.Summoning.Special.Pixie.SleepArrowResource";
        private const string PixieCombatTraitsSymbol =
            "KMG.Summoning.Special.Pixie.CombatTraits";
        private const string PixieDanceAiSymbol =
            "KMG.Summoning.Special.Pixie.IrresistibleDanceAi";
        private const string PixieBrainSymbol =
            "KMG.Summoning.Special.Pixie.Brain";
        private const string CyclopsUnitSymbol =
            "KMG.Summoning.Unit.Cyclops";
        private const string CyclopsFlashSymbol =
            "KMG.Summoning.Special.Cyclops.FlashOfInsight";
        private const string CyclopsFlashStateSymbol =
            "KMG.Summoning.Special.Cyclops.FlashOfInsightState";
        private const string CyclopsFlashResourceSymbol =
            "KMG.Summoning.Special.Cyclops.FlashOfInsightResource";
        private const string CyclopsCombatTraitsSymbol =
            "KMG.Summoning.Special.Cyclops.CombatTraits";
        private const string CyclopsFlashAiSymbol =
            "KMG.Summoning.Special.Cyclops.FlashOfInsightAi";
        private const string CyclopsBrainSymbol =
            "KMG.Summoning.Special.Cyclops.Brain";
        private const string GrappleHoldSymbol =
            "KMG.Summoning.Special.Grapple.Hold";
        private const string GrappleGrappledSymbol =
            "KMG.Summoning.Special.Grapple.Grappled";
        private const string OwlbearUnitSymbol = "KMG.Summoning.Unit.Owlbear";
        private const string OwlbearCombatTraitsSymbol =
            "KMG.Summoning.Special.Owlbear.CombatTraits";
        private const string ShamblingMoundUnitSymbol =
            "KMG.Summoning.Unit.ShamblingMound";
        private const string ShamblingMoundCombatTraitsSymbol =
            "KMG.Summoning.Special.ShamblingMound.CombatTraits";
        private const string GiantFlytrapUnitSymbol =
            "KMG.Summoning.Unit.GiantFlytrap";
        private const string GiantFlytrapCombatTraitsSymbol =
            "KMG.Summoning.Special.GiantFlytrap.CombatTraits";
        private const string PurpleWormUnitSymbol =
            "KMG.Summoning.Unit.PurpleWorm";
        private const string PurpleWormCombatTraitsSymbol =
            "KMG.Summoning.Special.PurpleWorm.CombatTraits";
        private const string PurpleWormSwallowedSymbol =
            "KMG.Summoning.Special.PurpleWorm.Swallowed";
        private const string NativePurpleWormSwallowedGuid =
            "368d1df7c1d0267459a584bf23ccadc8";
        // Sprint 5: exact identities from the deep native-donor audits
        // (20260924T2148497908566Z and 20260924T2214377611148Z).
        internal const string SickenedBuffGuid = "4e42460798665fd4cb9173ffa7ada323";
        private const string ColdVulnerabilityGuid = "b8bbe8f713da9ad44a899aa551ca6b5b";
        private const string FireVulnerabilityGuid = "8e934134fec60ab4c8972c85a7b62f89";
        private const string EarthSubtypeGuid = "e147258e5b7c40643893d80c9f2816e8";
        private const string WaterSubtypeGuid = "bf7ee56ec9e43c14fa17727997e91993";
        private const string AcidImmunityGuid = "c994f1a0dfce1c54f94420588da61617";
        private const string MephitAirBreathGuid = "1f08438786937954aaa6022c7f5ad286";
        private const string MephitEarthBreathGuid = "fa5ee5f4cd5c6394f8b497c773f8e14a";
        private const string MephitFireBreathGuid = "ab0616beb567c2c4d8d3f7447a01a0c8";
        private const string MephitWaterBreathGuid = "a54cd27999a5e8340976f3a40edfef3a";
        private const string MephitAirBlurGuid = "f98c9fd94b1be6947bd9637816226c1b";
        private const string MephitEarthChangeSizeGuid = "65dfd0a5324a76145b38c03250c25a7b";
        private const string MephitWaterStinkingCloudGuid = "34283d686f5f5a847b4d0d6470b52a65";
        private const string MephitWaterImmunitiesGuid = "e35ea268f6c8b0344b11c569973197c1";
        private const string ScorchingRayGuid = "cdb106d53c65bbc4086183d54c3b97c7";
        private const string AcidArrowGuid = "9a46dfd390f943647ab4395fc997936d";
        private const string MagicMissileGuid = "4ac47ddb9fa1eaf43a1b6809980cfbd2";
        private const string GlitterdustGuid = "ce7dad2b25acf85429b6c9550787b2d9";
        // Sprint 6: exact identities from the deep native-donor audit
        // (20260924T1942171737899Z) and the runner's weapon map.
        internal const string NativeWebGrappledGuid = "a719abac0ea0ce346b401060754cc1c0";
        internal const string MediumBite1d8Guid = "c988aa874d11ff84d873508ddc9b928f";
        private const string MonitorLizardUnitSymbol = "KMG.Summoning.Unit.MonitorLizard";
        private const string MonitorLizardCombatTraitsSymbol =
            "KMG.Summoning.Special.MonitorLizard.CombatTraits";
        private const string GrizzlyBearUnitSymbol = "KMG.Summoning.Unit.GrizzlyBear";
        private const string GrizzlyBearCombatTraitsSymbol =
            "KMG.Summoning.Special.GrizzlyBear.CombatTraits";
        private const string DireBearUnitSymbol = "KMG.Summoning.Unit.DireBear";
        private const string DireBearCombatTraitsSymbol =
            "KMG.Summoning.Special.DireBear.CombatTraits";
        private const string GiantSpiderUnitSymbol = "KMG.Summoning.Unit.GiantSpider";
        private const string GiantSpiderWebSymbol = "KMG.Summoning.Special.GiantSpider.Web";
        private const string GiantSpiderWebResourceSymbol =
            "KMG.Summoning.Special.GiantSpider.WebResource";
        private const string GiantSpiderWebAiSymbol = "KMG.Summoning.Special.GiantSpider.WebAi";
        private const string GiantSpiderBrainSymbol = "KMG.Summoning.Special.GiantSpider.Brain";
        private const string GiantSpiderCombatTraitsSymbol =
            "KMG.Summoning.Special.GiantSpider.CombatTraits";
        // Sprint 7: the cats.
        private const string LeopardUnitSymbol = "KMG.Summoning.Unit.Leopard";
        private const string LeopardCombatTraitsSymbol = "KMG.Summoning.Special.Leopard.CombatTraits";
        private const string LionUnitSymbol = "KMG.Summoning.Unit.Lion";
        private const string LionCombatTraitsSymbol = "KMG.Summoning.Special.Lion.CombatTraits";
        private const string DireLionUnitSymbol = "KMG.Summoning.Unit.DireLion";
        private const string DireLionCombatTraitsSymbol = "KMG.Summoning.Special.DireLion.CombatTraits";
        private const string DireTigerUnitSymbol = "KMG.Summoning.Unit.DireTiger";
        private const string DireTigerCombatTraitsSymbol = "KMG.Summoning.Special.DireTiger.CombatTraits";
        // Sprint 8: the tiger and the cheetah.
        private const string TigerUnitSymbol = "KMG.Summoning.Unit.Tiger";
        private const string TigerCombatTraitsSymbol = "KMG.Summoning.Special.Tiger.CombatTraits";
        private const string Claw1d8Symbol = "KMG.Summoning.Natural.Claw1d8";
        private const string CheetahUnitSymbol = "KMG.Summoning.Unit.Cheetah";
        private const string CheetahSprintSymbol = "KMG.Summoning.Special.Cheetah.Sprint";
        private const string CheetahSprintResourceSymbol =
            "KMG.Summoning.Special.Cheetah.SprintResource";
        private const string CheetahSprintStateSymbol =
            "KMG.Summoning.Special.Cheetah.SprintState";
        private const string CheetahSprintAiSymbol = "KMG.Summoning.Special.Cheetah.SprintAi";
        private const string CheetahBrainSymbol = "KMG.Summoning.Special.Cheetah.Brain";
        private const string CheetahCombatTraitsSymbol =
            "KMG.Summoning.Special.Cheetah.CombatTraits";
        internal const string SmallClawGuid = "800092a2b9a743b48ae8aeeb5d243dcc";
        internal const string MediumClawGuid = "118fdd03e569a66459ab01a20af6811a";
        internal const string Claw2d4Guid = "8afc47748d00b3e4a8aff2787d9ee350";
        internal const string ColdConeProjectileGuid = "5af8b717a209fd444a1e4d077ed776f0";
        internal const string AcidConeProjectileGuid = "f6544caac8fe528489327cd86a84b025";
        internal const string FireConeProjectileGuid = "6dfc5e4c7d9ae3048984744222dbd0fa";
        /// <summary>
        /// Native facts a mephit donor carries that belong to its element
        /// (subtype, immunities, breath, spell-like abilities), dropped from
        /// the variant before its own are added. Damage reduction, fast
        /// healing, natural armor, Dodge, Improved Initiative, the empty
        /// visual buff and the extraplanar subtype stay.
        /// </summary>
        internal static readonly string[] MephitDonorElementFactGuids = {
            MephitAirBreathGuid, MephitEarthBreathGuid, MephitFireBreathGuid,
            MephitWaterBreathGuid, MephitAirBlurGuid, MephitEarthChangeSizeGuid,
            MephitWaterStinkingCloudGuid, MephitWaterImmunitiesGuid,
            ScorchingRayGuid, AcidArrowGuid, ElectricityImmunityGuid,
            AcidImmunityGuid, ColdImmunityGuid, AirSubtypeGuid, EarthSubtypeGuid,
            WaterSubtypeGuid, FireSubtypeGuid
        };

        private const string NativeRayGuid = "33e8997912cf76b4c99dca0445082804";
        private const string NativeRayAiGuid = "dcfc5e9aec5bea540b36caf754989164";
        private const string OutsiderClassGuid = "92ab5f2fe00631b44810deffcc1a97fd";
        private const string ImprovedInitiativeGuid = "797f25d709f559546b29e7bcb181cc74";
        private const string NaturalArmor4Guid = "16fc201a83edcde4cbd64c291ebe0d07";
        private const string ElectricityImmunityGuid = "cd1e5ab641a833c49994aff99db98952";
        private const string GoodSubtypeGuid = "23247ff3b44fd3a42ab752cd04e629b0";
        private const string LawfulSubtypeGuid = "56af493a739e14f44aa56a6cba0b477b";
        private const string ExtraplanarSubtypeGuid = "136fa0343d5b4b348bdaa05d83408db3";
        private const string AirborneGuid = "70cffb448c132fa409e49156d013b175";
        private const string AuraOfMenaceBuffGuid = "1ce4878b5e714f659d0854a12f4b3cf2";
        private const string DumbBrainGuid = "5abc8884c6f15204c8604cb01a2efbab";
        private const string NaturalArmor6Guid = "987ba44303e88054c9504cb3083ba0c9";
        private const string NaturalInvisibilityGuid = "94b2838e8a492c44ebf89e7fe7a75a62";
        private const string IncorporealGuid = "c4a7f98d743bc784c9d4cf2105852c39";
        private const string ElementalSubtypeGuid = "198fd8924dabcb5478d0f78bd453c586";
        private const string AirSubtypeGuid = "dd3d0c7f4f57f304cbdbb68170b1b775";
        private const string ChaoticSubtypeGuid = "1dd712e7f147ab84bad6ffccd21a878d";
        private const string EvilSubtypeGuid = "5279fc8380dd9ba419b4471018ffadd1";
        private const string ColdImmunityGuid = "9ae23798a9284e044ad2716a772a410e";
        private const string PoisonImmunityGuid = "7e3f3228be49cce49bda37f7901bf246";
        private const string CombatReflexesGuid = "0f8939ae6f220984e8fb568abbdfba95";
        private const string LightningReflexesGuid = "15e7da6645a7f3d41bdad7c8c4b9de1e";
        private const string WeaponFocusSlamGuid = "8c046dfa8d1c64247af0e830a5909510";
        private const string WeaponFocusClawGuid = "153937f44fcd42a429a286a10babd82d";
        private const string LargeAirSlamGuid = "72aa06bd4e7a8fa4db8a20d1b5f1a103";
        private const string LargeClawGuid = "c76f72a862d168d44838206524366e1c";
        private const string LargeBiteGuid = "ec35ef997ed5a984280e1a6d87ae80a8";
        private const string NaturalArmor7Guid = "e73864391ccf0894997928443a29d755";
        private const string FireImmunityGuid = "11ac3433adfa74642a93111624376070";
        private const string FireSubtypeGuid = "23dc7b90d148b9d439f48e015a520a9c";
        private const string DrMagic10Guid = "ac5a99153e1790941b7bb93c06586ea5";
        private const string WeaponFocusSpearGuid = "8c7e86088025ad3448849d4972335dc8";
        private const string StandardSpearGuid = "4abc27631e2894f4b8b70270e31694f1";
        private const string LargeTailGuid = "ae822725634c6f0418b8c48bd29df255";
        private const string NativeGrabGuid = "efc1e80fb41e06544be46604983806d6";
        private const string NativeDominateGuid = "d7cbd2004ce66a042aeab2e95a3c5c61";
        private const string NativeDominationGuid = "c0f4e1c24c9cd334ca988ed1bd9d201f";
        private const string NativeEnergyDrainGuid = "ab966bf06859119419989ccb0061ba39";
        private const string DodgeGuid = "97e216dbb46ae3c4faef90cf6bbe6fd5";
        private const string WeaponFinesseGuid = "90e54424d682d104ab36436bd527af09";
        private const string AberrationTypeGuid = "3bec99efd9a363242a6c8d9957b75e91";
        private const string ConstructTypeGuid = "fd389783027d63343b4a5634bd81645f";
        private const string DragonTypeGuid = "455ac88e22f55804ab87c2467deff1d6";
        private const string FeyTypeGuid = "018af8005220ac94a9a4f47b3e9c2b4e";
        private const string OutsiderTypeGuid = "9054d3988d491d944ac144e27b6bc318";
        private const string PlantTypeGuid = "706e61781d692a042b35941f14bc41c5";
        private const string AnimalTypeGuid = "a95311b3dc996964cbaa30ff9965aaf6";
        private const string MonstrousHumanoidTypeGuid = "57614b50e8d86b24395931fffc5e409b";
        private const string MagicalBeastTypeGuid = "625827490ea69d84d8e599a33929fdc6";
        private const string VerminTypeGuid = "09478937695300944a179530664e42ec";
        private const string UndeadTypeGuid = "734a29b693e9ec346ba2951b27987e33";
        private const string FeyClassGuid = "f2e6e760ead99fb48ade27c7e9d4ac94";
        private const string HugeBiteGuid = "d2f99947db522e24293a7ec4eded453f";
        private const string StandardLongbowGuid = "201f6150321e09048bd59e9b7f558cb0";
        private const string NativeSleepingBuffGuid = "5e0cd801bac0e95429bb7e4d1bc61a23";

        internal static void Configure(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol,
            BlueprintFeature extraplanar)
        {
            if (extraplanar == null) throw new ArgumentNullException("extraplanar");
            ExpandedSummoningSpecialProfiles.Validate();
            BlueprintAbility ray = Require<BlueprintAbility>(bySymbol,
                LanternRaySymbol);
            BlueprintAiCastSpell ai = Require<BlueprintAiCastSpell>(bySymbol,
                LanternAiSymbol);
            BlueprintBrain brain = Require<BlueprintBrain>(bySymbol,
                LanternBrainSymbol);
            BlueprintBuff defenses = Require<BlueprintBuff>(bySymbol,
                LanternDefenseSymbol);
            ConfigureRay(library, ray);
            ConfigureAi(library, ai, ray);
            brain.name = InternalName(LanternBrainSymbol);
            brain.Actions = new BlueprintAiAction[] { ai };
            ConfigureDefenses(defenses);
            ConfigureUnit(library, Require<BlueprintUnit>(bySymbol,
                LanternUnitSymbol), ray, brain, defenses, extraplanar);
            ConfigureInvisibleStalker(library, Require<BlueprintUnit>(bySymbol,
                InvisibleStalkerUnitSymbol), extraplanar);
            ConfigureErinyes(library, Require<BlueprintUnit>(bySymbol,
                ErinyesUnitSymbol));
            BlueprintBuff shadowTraits = Require<BlueprintBuff>(bySymbol,
                ShadowDemonCombatTraitsSymbol);
            ConfigureShadowDemonCombatTraits(shadowTraits);
            ConfigureShadowDemon(library, Require<BlueprintUnit>(bySymbol,
                ShadowDemonUnitSymbol), shadowTraits, extraplanar);
            BlueprintWeaponType salamanderSpearType = Require<
                BlueprintWeaponType>(bySymbol, SalamanderSpearTypeSymbol);
            BlueprintItemWeapon salamanderSpear = Require<BlueprintItemWeapon>(
                bySymbol, SalamanderSpearSymbol);
            BlueprintItemWeapon salamanderTail = Require<BlueprintItemWeapon>(
                bySymbol, SalamanderTailSymbol);
            BlueprintBuff salamanderTraits = Require<BlueprintBuff>(bySymbol,
                SalamanderCombatTraitsSymbol);
            ConfigureSummonWeaponType(library, StandardSpearGuid,
                "standard 1d8 spear", SalamanderSpearTypeSymbol,
                salamanderSpearType);
            ConfigureSalamanderSpear(library, salamanderSpear,
                salamanderSpearType);
            ConfigureSalamanderTail(library, salamanderTail);
            ConfigureSalamanderCombatTraits(library, salamanderTraits,
                salamanderTail);
            ConfigureSalamander(library, Require<BlueprintUnit>(bySymbol,
                SalamanderUnitSymbol), salamanderSpear, salamanderTail,
                salamanderTraits, extraplanar);
            BlueprintBuff domination = Require<BlueprintBuff>(bySymbol,
                SuccubusDominationSymbol);
            BlueprintAbility dominate = Require<BlueprintAbility>(bySymbol,
                SuccubusDominateSymbol);
            BlueprintAiCastSpell dominateAi = Require<BlueprintAiCastSpell>(
                bySymbol, SuccubusDominateAiSymbol);
            BlueprintBrain succubusBrain = Require<BlueprintBrain>(bySymbol,
                SuccubusBrainSymbol);
            BlueprintBuff succubusTraits = Require<BlueprintBuff>(bySymbol,
                SuccubusCombatTraitsSymbol);
            ConfigureSuccubusDomination(library, domination);
            ConfigureSuccubusDominate(library, dominate, domination);
            ConfigureSuccubusAi(dominateAi, dominate, succubusBrain);
            ConfigureSuccubusCombatTraits(library, succubusTraits);
            ConfigureSuccubus(library, Require<BlueprintUnit>(bySymbol,
                SuccubusUnitSymbol), dominate, succubusBrain, succubusTraits,
                extraplanar);
            BlueprintItemWeapon bebelithClaw = Require<BlueprintItemWeapon>(
                bySymbol, BebelithClawSymbol);
            BlueprintBuff dismantledArmor = Require<BlueprintBuff>(bySymbol,
                BebelithDismantledArmorSymbol);
            BlueprintBuff bebelithTraits = Require<BlueprintBuff>(bySymbol,
                BebelithCombatTraitsSymbol);
            ConfigureBebelithClaw(library, bebelithClaw);
            ConfigureBebelithDismantledArmor(dismantledArmor);
            ConfigureBebelithCombatTraits(library, bebelithTraits,
                bebelithClaw, dismantledArmor);
            ConfigureBebelith(library, Require<BlueprintUnit>(bySymbol,
                BebelithUnitSymbol), bebelithClaw, bebelithTraits,
                extraplanar);
            BlueprintWeaponType pixieSleepBowType = Require<BlueprintWeaponType>(
                bySymbol, PixieSleepBowTypeSymbol);
            BlueprintItemWeapon pixieSleepBow = Require<BlueprintItemWeapon>(
                bySymbol, PixieSleepBowSymbol);
            BlueprintAbilityResource pixieDanceResource = Require<
                BlueprintAbilityResource>(bySymbol, PixieDanceResourceSymbol);
            BlueprintAbilityResource pixieSleepResource = Require<
                BlueprintAbilityResource>(bySymbol, PixieSleepResourceSymbol);
            BlueprintAbility pixieDance = Require<BlueprintAbility>(bySymbol,
                PixieDanceSymbol);
            BlueprintBuff pixieDanceState = Require<BlueprintBuff>(bySymbol,
                PixieDanceStateSymbol);
            BlueprintBuff pixieTraits = Require<BlueprintBuff>(bySymbol,
                PixieCombatTraitsSymbol);
            BlueprintAiCastSpell pixieDanceAi = Require<BlueprintAiCastSpell>(
                bySymbol, PixieDanceAiSymbol);
            BlueprintBrain pixieBrain = Require<BlueprintBrain>(bySymbol,
                PixieBrainSymbol);
            ConfigureSummonWeaponType(library, StandardLongbowGuid,
                "standard longbow arrow rig", PixieSleepBowTypeSymbol,
                pixieSleepBowType);
            ConfigurePixieSleepBow(library, pixieSleepBow,
                pixieSleepBowType);
            ConfigureResource(pixieDanceResource, "Irresistible Dance",
                ExpandedSummoningSpecialProfiles.PixieDanceUses);
            ConfigureResource(pixieSleepResource, "Sleep Arrows",
                ExpandedSummoningSpecialProfiles.PixieSleepArrowUses);
            ConfigurePixieDanceState(pixieDanceState);
            ConfigurePixieDance(pixieDance, pixieDanceState,
                pixieDanceResource, pixieSleepBow.Icon);
            ConfigurePixieCombatTraits(library, pixieTraits, pixieSleepBow,
                pixieSleepResource, pixieDanceResource);
            ConfigurePixieAi(pixieDanceAi, pixieDance, pixieBrain);
            ConfigurePixie(library, Require<BlueprintUnit>(bySymbol,
                PixieUnitSymbol), pixieSleepBow, pixieDance, pixieBrain,
                pixieTraits);
            ConfigureCyclops(bySymbol);
            ConfigureGrapplers(library, bySymbol);
            ConfigureMephitVariants(library, bySymbol);
        }

        /// <summary>
        /// Sprint 5: the six mephit variants. Each unit is the sanitized clone
        /// of its nearest native summoned mephit; here its element facts are
        /// swapped for the variant's, its breath is rebuilt from the donor's
        /// breath (energy, dice, cone visual, an enemy-only effect and the
        /// sickening rider), its spell-like abilities are native spells with
        /// one-use resources or project bursts, its brain is a project brain
        /// with one cast action per ability, its display name is its own and
        /// its visual variant (a tint on the shared rig) is registered.
        /// </summary>
        private static void ConfigureMephitVariants(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            BlueprintBuff sickened = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(
                library, SickenedBuffGuid, "native sickened condition");
            foreach (MephitVariantProfile profile in
                ExpandedSummoningSpecialProfiles.MephitVariants)
            {
                string token = MephitToken(profile.Key);
                string prefix = "KMG.Summoning.Special." + token + ".";
                BlueprintUnit unit = Require<BlueprintUnit>(bySymbol,
                    "KMG.Summoning.Unit." + token);
                BlueprintAbility nativeBreath = BlueprintLibraryLookup.RequireExact<
                    BlueprintAbility>(library, MephitDonorBreathGuid(profile.DonorKey),
                        "native mephit breath");
                BlueprintAbility breath = Require<BlueprintAbility>(bySymbol,
                    prefix + "Breath");
                ConfigureMephitBreath(library, breath, nativeBreath, profile, sickened,
                    prefix + "Breath", token);
                var grants = new List<BlueprintUnitFact> { breath };
                var aiActions = new List<BlueprintAiAction> {
                    ConfigureMephitAiAction(Require<BlueprintAiCastSpell>(bySymbol,
                        prefix + "BreathAi"), prefix + "BreathAi", breath,
                        ExpandedSummoningSpecialProfiles.MephitBreathAiCooldownRounds)
                };
                var resources = new List<BlueprintAbilityResource>();
                foreach (KeyValuePair<string, string> slot in MephitSpellLikeSlots(profile))
                {
                    BlueprintAbility ability = Require<BlueprintAbility>(bySymbol,
                        prefix + slot.Key);
                    BlueprintAbilityResource resource = Require<BlueprintAbilityResource>(
                        bySymbol, prefix + slot.Key + "Resource");
                    ConfigureMephitSpellLike(library, ability, resource, slot.Value,
                        prefix + slot.Key, token);
                    resources.Add(resource);
                    grants.Add(ability);
                    aiActions.Add(ConfigureMephitAiAction(Require<BlueprintAiCastSpell>(
                        bySymbol, prefix + slot.Key + "Ai"), prefix + slot.Key + "Ai",
                        ability, 0));
                }
                BlueprintBuff traits = Require<BlueprintBuff>(bySymbol,
                    prefix + "CombatTraits");
                traits.name = InternalName(prefix + "CombatTraits");
                traits.Stacking = StackingType.Replace;
                traits.IsClassFeature = true;
                traits.ComponentsArray = resources.Select(value =>
                    (BlueprintComponent)AddResource(value)).ToArray();
                BlueprintUnitFactAccess.Resolve().Configure(traits,
                    LocalizationService.Create("KMG.ExpandedSummoning." + token +
                        ".CombatTraits.Name", MephitDisplayName(profile.Key) + " Traits"),
                    LocalizationService.Create("KMG.ExpandedSummoning." + token +
                        ".CombatTraits.Description",
                        "One use of each spell-like ability for this summoning."),
                    null);
                grants.Add(traits);
                BlueprintBrain brain = Require<BlueprintBrain>(bySymbol, prefix + "Brain");
                brain.name = InternalName(prefix + "Brain");
                brain.Actions = aiActions.ToArray();
                unit.Brain = brain;
                // Element facts: drop the donor's, add the variant's.
                List<BlueprintUnitFact> facts = (unit.AddFacts ??
                    Array.Empty<BlueprintUnitFact>()).Where(value => value != null &&
                        !MephitDonorElementFactGuids.Contains(value.AssetGuid)).ToList();
                // Subtypes, immunities and vulnerabilities are all native
                // BlueprintFeatures; the lookup is exact by type.
                foreach (string guid in MephitVariantFactGuids(profile.Key))
                    facts.Add(BlueprintLibraryLookup.RequireExact<BlueprintFeature>(
                        library, guid, profile.Key + " element fact"));
                facts.AddRange(grants);
                unit.AddFacts = facts.ToArray();
                var name = ScriptableObject.CreateInstance<SharedStringAsset>();
                name.String = LocalizationService.Create(
                    "KMG.ExpandedSummoning." + token + ".Unit.Name",
                    MephitDisplayName(profile.Key));
                unit.LocalizedName = name;
                ExpandedSummoningVisualVariantPatch.Register(new SummonVisualVariant(
                    unit.name, ExpandedSummoningSpecialProfiles.MephitCoat(profile.Key)));
            }
        }

        internal static string MephitToken(string key)
        {
            return string.Concat(key.Split('-').Select(part =>
                char.ToUpperInvariant(part[0]) + part.Substring(1)).ToArray());
        }

        internal static string MephitDisplayName(string key)
        {
            return string.Join(" ", key.Split('-').Select(part =>
                char.ToUpperInvariant(part[0]) + part.Substring(1)).ToArray());
        }

        internal static IEnumerable<KeyValuePair<string, string>> MephitSpellLikeSlots(
            MephitVariantProfile profile)
        {
            if (!string.IsNullOrEmpty(profile.SpellLikeOne))
                yield return new KeyValuePair<string, string>("SpellLikeOne",
                    profile.SpellLikeOne);
            if (!string.IsNullOrEmpty(profile.SpellLikeTwo))
                yield return new KeyValuePair<string, string>("SpellLikeTwo",
                    profile.SpellLikeTwo);
        }

        private static string MephitDonorBreathGuid(string donorKey)
        {
            switch (donorKey)
            {
                case "air-mephit": return MephitAirBreathGuid;
                case "earth-mephit": return MephitEarthBreathGuid;
                case "fire-mephit": return MephitFireBreathGuid;
                case "water-mephit": return MephitWaterBreathGuid;
            }
            throw new InvalidOperationException("Unknown mephit donor " + donorKey + ".");
        }

        /// <summary>
        /// The native cone visual for the variant's breath energy: the cold
        /// cone for the physical dust and salt breaths and the ice breath, the
        /// acid cone for the ooze, the fire cone for magma and steam.
        /// </summary>
        internal static string MephitBreathProjectileGuid(string energy)
        {
            if (energy == "Fire") return FireConeProjectileGuid;
            if (energy == "Acid") return AcidConeProjectileGuid;
            return ColdConeProjectileGuid;
        }

        /// <summary>
        /// The variant's own element facts: its subtypes and, where the
        /// tabletop creature has them, its immunity and vulnerability. The
        /// native fire subtype carries fire immunity and cold vulnerability
        /// itself.
        /// </summary>
        internal static string[] MephitVariantFactGuids(string key)
        {
            switch (key)
            {
                case "dust-mephit": return new[] { AirSubtypeGuid };
                case "ice-mephit": return new[] { AirSubtypeGuid, ColdImmunityGuid,
                    FireVulnerabilityGuid };
                case "magma-mephit": return new[] { EarthSubtypeGuid, FireSubtypeGuid };
                case "ooze-mephit": return new[] { WaterSubtypeGuid };
                case "salt-mephit": return new[] { EarthSubtypeGuid };
                case "steam-mephit": return new[] { FireSubtypeGuid, WaterSubtypeGuid };
            }
            throw new InvalidOperationException("Unknown mephit variant " + key + ".");
        }

        /// <summary>
        /// The breath: the donor's 15-foot cone (delivery, save, descriptor and
        /// Constitution-based DC all kept) with the variant's energy and dice,
        /// its effect wrapped so only the caster's enemies in the cone are
        /// touched (the charter's ally-safe rule for every mephit area
        /// effect), and the tabletop sickening rider on a failed save.
        /// </summary>
        private static void ConfigureMephitBreath(LibraryScriptableObject library,
            BlueprintAbility breath, BlueprintAbility nativeBreath,
            MephitVariantProfile profile, BlueprintBuff sickened, string symbol,
            string token)
        {
            ExpandedSummoningAbilityBuilder.CopyFields(nativeBreath, breath);
            breath.name = InternalName(symbol);
            breath.ComponentsArray = (nativeBreath.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Where(value => value != null)
                .Select(ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            AbilityDeliverProjectile deliver = breath.ComponentsArray
                .OfType<AbilityDeliverProjectile>().Single();
            deliver.Projectiles = new[] { BlueprintLibraryLookup.RequireExact<
                BlueprintProjectile>(library, MephitBreathProjectileGuid(
                    profile.BreathEnergy), "native breath cone projectile") };
            AbilityEffectRunAction run = breath.ComponentsArray
                .OfType<AbilityEffectRunAction>().Single();
            ContextActionDealDamage damage = FindDamage(new BlueprintComponent[] { run });
            damage.DamageType = MephitBreathDamageType(profile.BreathEnergy);
            damage.Value = new ContextDiceValue {
                DiceType = profile.BreathDieSides == 8 ? DiceType.D8 : DiceType.D4,
                DiceCountValue = Simple(profile.BreathDice),
                BonusValue = Simple(0)
            };
            damage.IsAoE = true;
            var actions = new List<GameAction>();
            if (profile.Key == "ooze-mephit")
            {
                // The tabletop ooze breath: a Reflex save negates the damage
                // and the sickening together.
                damage.HalfIfSaved = false;
                var saved = ScriptableObject.CreateInstance<ContextActionConditionalSaved>();
                saved.Succeed = new ActionList { Actions = Array.Empty<GameAction>() };
                saved.Failed = new ActionList { Actions = new GameAction[] { damage,
                    MephitSickenAction(sickened) } };
                actions.Add(saved);
            }
            else
            {
                damage.HalfIfSaved = true;
                actions.Add(damage);
                if (profile.BreathSickens)
                {
                    var saved = ScriptableObject.CreateInstance<
                        ContextActionConditionalSaved>();
                    saved.Succeed = new ActionList { Actions = Array.Empty<GameAction>() };
                    saved.Failed = new ActionList { Actions = new GameAction[] {
                        MephitSickenAction(sickened) } };
                    actions.Add(saved);
                }
            }
            var enemy = ScriptableObject.CreateInstance<ContextConditionIsEnemy>();
            enemy.Not = false;
            var enemiesOnly = ScriptableObject.CreateInstance<Conditional>();
            enemiesOnly.Comment = "KMG mephit breath: enemies of the caster only";
            enemiesOnly.ConditionsChecker = new ConditionsChecker {
                Operation = Operation.And, Conditions = new Condition[] { enemy } };
            enemiesOnly.IfTrue = new ActionList { Actions = actions.ToArray() };
            enemiesOnly.IfFalse = new ActionList { Actions = Array.Empty<GameAction>() };
            run.Actions = new ActionList { Actions = new GameAction[] { enemiesOnly } };
            SpellDescriptorComponent descriptor = breath.ComponentsArray
                .OfType<SpellDescriptorComponent>().Single();
            descriptor.Descriptor = new SpellDescriptorWrapper(
                MephitBreathDescriptor(profile.BreathEnergy));
            // The native breath's icon stays with the clone; the helper
            // replaces the icon it is handed, so the copied one is passed back.
            BlueprintUnitFactAccess.Resolve().Configure(breath,
                LocalizationService.Create("KMG.ExpandedSummoning." + token +
                    ".Breath.Name", MephitDisplayName(profile.Key) + " Breath"),
                LocalizationService.Create("KMG.ExpandedSummoning." + token +
                    ".Breath.Description", MephitBreathDescription(profile)),
                nativeBreath.Icon);
        }

        private static ContextActionApplyBuff MephitSickenAction(BlueprintBuff sickened)
        {
            var apply = ScriptableObject.CreateInstance<ContextActionApplyBuff>();
            apply.Buff = sickened;
            apply.ToCaster = false;
            apply.DurationValue = new ContextDurationValue {
                Rate = DurationRate.Rounds,
                DiceType = DiceType.Zero,
                DiceCountValue = Simple(0),
                BonusValue = Simple(ExpandedSummoningSpecialProfiles.MephitSickenedRounds)
            };
            apply.IsFromSpell = false;
            apply.IsNotDispelable = false;
            return apply;
        }

        internal static DamageTypeDescription MephitBreathDamageType(string energy)
        {
            if (energy == "Slashing")
                return new DamageTypeDescription {
                    Type = DamageType.Physical,
                    Physical = new DamageTypeDescription.PhysicalData {
                        Form = PhysicalDamageForm.Slashing }
                };
            return new DamageTypeDescription {
                Type = DamageType.Energy,
                Energy = energy == "Fire" ? DamageEnergyType.Fire :
                    energy == "Cold" ? DamageEnergyType.Cold : DamageEnergyType.Acid
            };
        }

        private static SpellDescriptor MephitBreathDescriptor(string energy)
        {
            switch (energy)
            {
                case "Fire": return SpellDescriptor.Fire;
                case "Cold": return SpellDescriptor.Cold;
                case "Acid": return SpellDescriptor.Acid;
            }
            return SpellDescriptor.None;
        }

        private static string MephitBreathDescription(MephitVariantProfile profile)
        {
            string damage = profile.BreathDice + "d" + profile.BreathDieSides + " " +
                (profile.BreathEnergy == "Slashing" ? "slashing" :
                    profile.BreathEnergy.ToLowerInvariant()) + " damage";
            if (profile.Key == "ooze-mephit")
                return "A 15-foot cone of acid that touches only enemies: " + damage +
                    " and sickened for 3 rounds; a Reflex save negates both.";
            return "A 15-foot cone that touches only enemies: " + damage +
                (profile.BreathSickens ?
                    " (Reflex half) and, on a failed save, sickened for 3 rounds." :
                    " (Reflex half).");
        }

        /// <summary>
        /// A spell-like ability: a native spell (or the native mephit's own
        /// version of it) cloned with a one-use resource, or one of the two
        /// project bursts. Spell-list memberships are not carried.
        /// </summary>
        private static void ConfigureMephitSpellLike(LibraryScriptableObject library,
            BlueprintAbility ability, BlueprintAbilityResource resource, string kind,
            string symbol, string token)
        {
            string displayName;
            string description;
            switch (kind)
            {
                case "Blur":
                    CloneMephitSpell(library, ability, MephitAirBlurGuid, "native mephit blur");
                    displayName = "Blur";
                    description = "Blur, once per summoning.";
                    break;
                case "MagicMissile":
                    CloneMephitSpell(library, ability, MagicMissileGuid, "native magic missile");
                    displayName = "Magic Missile";
                    description = "Magic missile, once per summoning.";
                    break;
                case "AcidArrow":
                    CloneMephitSpell(library, ability, AcidArrowGuid, "native acid arrow");
                    displayName = "Acid Arrow";
                    description = "Acid arrow, once per summoning.";
                    break;
                case "StinkingCloud":
                    CloneMephitSpell(library, ability, MephitWaterStinkingCloudGuid,
                        "native mephit stinking cloud");
                    displayName = "Stinking Cloud";
                    description = "Stinking cloud, once per summoning.";
                    break;
                case "Glitterdust":
                    CloneMephitSpell(library, ability, GlitterdustGuid, "native glitterdust");
                    displayName = "Glitterdust";
                    description = "Glitterdust, once per summoning.";
                    break;
                case "Dehydrate":
                    ConfigureMephitBurst(ability, null,
                        ExpandedSummoningSpecialProfiles.DehydrateDice,
                        ExpandedSummoningSpecialProfiles.DehydrateDieSides);
                    displayName = "Dehydrate";
                    description = "Every enemy within 20 feet takes 2d8 damage (Fortitude half), once per summoning.";
                    break;
                case "BoilingRain":
                    ConfigureMephitBurst(ability, DamageEnergyType.Fire,
                        ExpandedSummoningSpecialProfiles.BoilingRainDice,
                        ExpandedSummoningSpecialProfiles.BoilingRainDieSides);
                    displayName = "Boiling Rain";
                    description = "Every enemy within 20 feet takes 2d6 fire damage (Fortitude half), once per summoning.";
                    break;
                default:
                    throw new InvalidOperationException(
                        "Unknown mephit spell-like ability " + kind + ".");
            }
            ability.name = InternalName(symbol);
            ability.Type = AbilityType.SpellLike;
            // A cloned native spell keeps its native icon; a project burst has
            // none yet and the icon builder gives it the mephit's own.
            Sprite nativeIcon = ability.Icon;
            ConfigureNamedResource(resource, symbol + "Resource",
                "KMG.ExpandedSummoning." + token + "." + kind + ".Resource", displayName,
                "Uses remaining for this summoned mephit.",
                ExpandedSummoningSpecialProfiles.MephitSpellLikeUses);
            var cost = ScriptableObject.CreateInstance<AbilityResourceLogic>();
            cost.RequiredResource = resource;
            cost.IsSpendResource = true;
            cost.CostIsCustom = false;
            cost.Amount = 1;
            ability.ComponentsArray = ability.ComponentsArray.Concat(
                new BlueprintComponent[] { cost }).ToArray();
            BlueprintUnitFactAccess.Resolve().Configure(ability,
                LocalizationService.Create("KMG.ExpandedSummoning." + token + "." +
                    kind + ".Name", displayName),
                LocalizationService.Create("KMG.ExpandedSummoning." + token + "." +
                    kind + ".Description", description), nativeIcon);
        }

        private static void CloneMephitSpell(LibraryScriptableObject library,
            BlueprintAbility ability, string nativeGuid, string role)
        {
            BlueprintAbility native = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                library, nativeGuid, role);
            ExpandedSummoningAbilityBuilder.CopyFields(native, ability);
            ability.ComponentsArray = (native.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Where(value => value != null &&
                    !(value is SpellListComponent))
                .Select(ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
        }

        /// <summary>
        /// A project burst centred on the mephit: enemies within 20 feet take
        /// the dice (Fortitude half, Constitution-based DC as the breath).
        /// Ally-safe by targeting, as the charter requires of every mephit
        /// area effect.
        /// </summary>
        private static void ConfigureMephitBurst(BlueprintAbility ability,
            DamageEnergyType? energy, int dice, int dieSides)
        {
            ability.Type = AbilityType.SpellLike;
            ability.Parent = null;
            ability.Hidden = false;
            ability.ActionBarAutoFillIgnored = false;
            ability.Range = AbilityRange.Personal;
            ability.CanTargetEnemies = false;
            ability.CanTargetSelf = true;
            ability.CanTargetFriends = false;
            ability.CanTargetPoint = false;
            ability.SpellResistance = false;
            ability.NeedEquipWeapons = false;
            ability.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            ability.EffectOnAlly = AbilityEffectOnUnit.None;
            ability.ActionType = UnitCommand.CommandType.Standard;
            ability.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Omni;
            ability.MaterialComponent = new BlueprintAbility.MaterialComponentData();
            ability.ResourceAssetIds = Array.Empty<string>();
            var around = ScriptableObject.CreateInstance<AbilityTargetsAround>();
            SetField(around, "m_Radius", new Feet(
                ExpandedSummoningSpecialProfiles.MephitBurstRadiusFeet));
            SetField(around, "m_TargetType",
                Kingmaker.UnitLogic.Abilities.Components.TargetType.Enemy);
            SetField(around, "m_IncludeDead", false);
            SetField(around, "m_Condition", new ConditionsChecker {
                Operation = Operation.And, Conditions = Array.Empty<Condition>() });
            SetField(around, "m_SpreadSpeed", new Feet(0));
            var damage = ScriptableObject.CreateInstance<ContextActionDealDamage>();
            damage.DamageType = energy.HasValue ?
                new DamageTypeDescription { Type = DamageType.Energy, Energy = energy.Value } :
                new DamageTypeDescription { Type = DamageType.Direct };
            damage.Value = new ContextDiceValue {
                DiceType = dieSides == 8 ? DiceType.D8 : DiceType.D6,
                DiceCountValue = Simple(dice), BonusValue = Simple(0)
            };
            damage.Duration = new ContextDurationValue {
                Rate = DurationRate.Rounds, DiceType = DiceType.Zero,
                DiceCountValue = Simple(0), BonusValue = Simple(0) };
            damage.IsAoE = true;
            damage.HalfIfSaved = true;
            var run = ScriptableObject.CreateInstance<AbilityEffectRunAction>();
            run.SavingThrowType = SavingThrowType.Fortitude;
            run.Actions = new ActionList { Actions = new GameAction[] { damage } };
            var parameters = ScriptableObject.CreateInstance<ContextCalculateAbilityParams>();
            parameters.StatType = StatType.Constitution;
            parameters.ReplaceCasterLevel = true;
            parameters.CasterLevel = Simple(3);
            parameters.ReplaceSpellLevel = true;
            parameters.SpellLevel = Simple(2);
            var descriptor = ScriptableObject.CreateInstance<SpellDescriptorComponent>();
            descriptor.Descriptor = new SpellDescriptorWrapper(energy.HasValue ?
                SpellDescriptor.Fire : SpellDescriptor.None);
            ability.ComponentsArray = new BlueprintComponent[] { around, run, parameters,
                descriptor };
        }

        /// <summary>
        /// A cast action for the variant's brain, the shape the Cyclops and
        /// Pixie actions have. The breath keeps a cooldown so the mephit also
        /// claws; a one-use spell-like ability needs none.
        /// </summary>
        private static BlueprintAiCastSpell ConfigureMephitAiAction(
            BlueprintAiCastSpell ai, string symbol, BlueprintAbility ability,
            int cooldownRounds)
        {
            ai.name = InternalName(symbol);
            ai.Ability = ability;
            ai.Variant = null;
            ai.BaseScore = 3;
            ai.CooldownRounds = cooldownRounds;
            ai.StartCooldownRounds = 0;
            ai.ActorConsiderations = Array.Empty<Kingmaker.Controllers.Brain
                .Blueprints.Considerations.Consideration>();
            ai.TargetConsiderations = Array.Empty<Kingmaker.Controllers.Brain
                .Blueprints.Considerations.Consideration>();
            ai.Locators = Array.Empty<EntityReference>();
            return ai;
        }

        /// <summary>
        /// Sprint 4: the shared summon grapple lifecycle. Two shared buffs
        /// (the holder's hold, the target's grappled state) and one
        /// combat-traits carrier per grabbing creature - Owlbear claws,
        /// Shambling Mound slams with constrict, Giant Flytrap bites, Purple
        /// Worm bite that swallows - each with the tabletop +4 grab bonus
        /// through the game's own ManeuverBonus. The worm's swallowed state is
        /// an exact clone of the native PurpleWormSwallowed components. Runs
        /// after the natural builder and only appends to the units.
        /// </summary>
        private static void ConfigureGrapplers(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            BlueprintBuff hold = Require<BlueprintBuff>(bySymbol, GrappleHoldSymbol);
            BlueprintBuff grappled = Require<BlueprintBuff>(bySymbol,
                GrappleGrappledSymbol);
            BlueprintBuff swallowed = Require<BlueprintBuff>(bySymbol,
                PurpleWormSwallowedSymbol);
            hold.name = InternalName(GrappleHoldSymbol);
            hold.Stacking = StackingType.Replace;
            hold.IsClassFeature = false;
            hold.ComponentsArray = new BlueprintComponent[] {
                ScriptableObject.CreateInstance<SummonHoldComponent>() };
            BlueprintUnitFactAccess.Resolve().Configure(hold,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Grapple.Hold.Name", "Holding"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Grapple.Hold.Description",
                    "This summoned creature is holding a grappled foe. Each round it makes a grapple check to maintain the hold, dealing its natural attack damage on a success and releasing on a failure; the foe may attempt to break free each round."),
                null);
            var entangled = ScriptableObject.CreateInstance<AddCondition>();
            entangled.Condition = UnitCondition.Entangled;
            grappled.name = InternalName(GrappleGrappledSymbol);
            grappled.Stacking = StackingType.Replace;
            grappled.IsClassFeature = false;
            grappled.ComponentsArray = new BlueprintComponent[] { entangled };
            BlueprintUnitFactAccess.Resolve().Configure(grappled,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Grapple.Grappled.Name", "Held"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Grapple.Grappled.Description",
                    "Held by a summoned creature: entangled and unable to move until the hold is broken."),
                null);
            BlueprintBuff nativeSwallowed = BlueprintLibraryLookup.RequireExact<
                BlueprintBuff>(library, NativePurpleWormSwallowedGuid,
                    "native purple worm swallowed state");
            swallowed.name = InternalName(PurpleWormSwallowedSymbol);
            swallowed.Stacking = StackingType.Replace;
            swallowed.IsClassFeature = false;
            swallowed.ComponentsArray = (nativeSwallowed.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Where(value => value != null)
                .Select(ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            BlueprintUnitFactAccess.Resolve().Configure(swallowed,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.PurpleWorm.Swallowed.Name",
                    "Swallowed Whole"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.PurpleWorm.Swallowed.Description",
                    "Swallowed by a summoned purple worm: crushed each round, unable to act, with a break-free attempt each round."),
                null);

            ConfigureGrabber(library, bySymbol, OwlbearUnitSymbol,
                OwlbearCombatTraitsSymbol, "Owlbear", "Owlbear Grab",
                "A claw hit lets the owlbear attempt to grab its foe.",
                new[] { LargeClawGuid }, hold, grappled, null, 0, 0);
            ConfigureGrabber(library, bySymbol, ShamblingMoundUnitSymbol,
                ShamblingMoundCombatTraitsSymbol, "ShamblingMound",
                "Shambling Mound Grab and Constrict",
                "A slam hit lets the mound attempt to grab its foe; a held foe is constricted for 2d6+7 as the grab lands and each round the hold is maintained.",
                new[] { "27eee74857c42db499b3a6b20cfa6211" }, hold, grappled, null,
                ExpandedSummoningSpecialProfiles.ShamblingMoundConstrictDice,
                ExpandedSummoningSpecialProfiles.ShamblingMoundConstrictBonus);
            ConfigureGrabber(library, bySymbol, GiantFlytrapUnitSymbol,
                GiantFlytrapCombatTraitsSymbol, "GiantFlytrap", "Giant Flytrap Grab",
                "A bite hit lets the flytrap attempt to grab its foe.",
                new[] { LargeBiteGuid }, hold, grappled, null, 0, 0);
            ConfigureGrabber(library, bySymbol, PurpleWormUnitSymbol,
                PurpleWormCombatTraitsSymbol, "PurpleWorm",
                "Purple Worm Swallow Whole",
                "A bite hit lets the worm attempt to grab its foe; success swallows the foe whole.",
                new[] { "7e4b9b41a9358264d9e3c69c183ca0a2" }, hold, grappled,
                swallowed, 0, 0);
            // Sprint 6: the existing grabbers the charter names, on the same
            // lifecycle; the mound-specific native grab graph stays unused.
            ConfigureGrabber(library, bySymbol, MonitorLizardUnitSymbol,
                MonitorLizardCombatTraitsSymbol, "MonitorLizard", "Monitor Lizard Grab",
                "A bite hit lets the lizard attempt to grab its foe.",
                new[] { MediumBite1d8Guid }, hold, grappled, null, 0, 0);
            ConfigureGrabber(library, bySymbol, GrizzlyBearUnitSymbol,
                GrizzlyBearCombatTraitsSymbol, "GrizzlyBear", "Grizzly Bear Grab",
                "A claw hit lets the bear attempt to grab its foe.",
                new[] { LargeClawGuid }, hold, grappled, null, 0, 0);
            ConfigureGrabber(library, bySymbol, DireBearUnitSymbol,
                DireBearCombatTraitsSymbol, "DireBear", "Dire Bear Grab",
                "A claw hit lets the bear attempt to grab its foe.",
                new[] { LargeClawGuid }, hold, grappled, null, 0, 0);
            ConfigureGiantSpiderWeb(library, bySymbol);
            // Sprint 7: the cats - claw grab on the same lifecycle, and the
            // charge-only rake gate on the same traits buff.
            ConfigureCat(library, bySymbol, LeopardUnitSymbol, LeopardCombatTraitsSymbol,
                "Leopard", "Leopard Grab and Rake", SmallClawGuid, hold, grappled);
            ConfigureCat(library, bySymbol, LionUnitSymbol, LionCombatTraitsSymbol,
                "Lion", "Lion Grab and Rake", MediumClawGuid, hold, grappled);
            ConfigureCat(library, bySymbol, DireLionUnitSymbol, DireLionCombatTraitsSymbol,
                "DireLion", "Dire Lion Grab and Rake", LargeClawGuid, hold, grappled);
            ConfigureCat(library, bySymbol, DireTigerUnitSymbol, DireTigerCombatTraitsSymbol,
                "DireTiger", "Smilodon Grab and Rake", Claw2d4Guid, hold, grappled);
            ExpandedSummoningVisualVariantPatch.Register(new SummonVisualVariant(
                InternalName(LionUnitSymbol), ExpandedSummoningSpecialProfiles.LionVisualTint));
            // Sprint 8: the tiger's grab-and-rake pack on the project 1d8 claw
            // and its striped coat; the cheetah's spotted coat and sprint.
            ConfigureCatWithWeapon(library, bySymbol, TigerUnitSymbol, TigerCombatTraitsSymbol,
                "Tiger", "Tiger Grab and Rake",
                Require<BlueprintItemWeapon>(bySymbol, Claw1d8Symbol), hold, grappled);
            ExpandedSummoningVisualVariantPatch.Register(new SummonVisualVariant(
                InternalName(TigerUnitSymbol), ExpandedSummoningSpecialProfiles.TigerCoat));
            ConfigureCheetahSprint(bySymbol);
            ExpandedSummoningVisualVariantPatch.Register(new SummonVisualVariant(
                InternalName(CheetahUnitSymbol), ExpandedSummoningSpecialProfiles.CheetahCoat));
        }

        /// <summary>
        /// A cat whose claw is a project weapon (the tiger's 1d8 claw).
        /// </summary>
        private static void ConfigureCatWithWeapon(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol, string unitSymbol,
            string traitsSymbol, string token, string displayName, BlueprintItemWeapon claw,
            BlueprintBuff hold, BlueprintBuff grappled)
        {
            BlueprintUnit unit = Require<BlueprintUnit>(bySymbol, unitSymbol);
            BlueprintBuff traits = Require<BlueprintBuff>(bySymbol, traitsSymbol);
            if (unit.ComponentsArray == null ||
                unit.ComponentsArray.OfType<AddClassLevels>().Count() != 1)
                throw new InvalidOperationException(
                    "The " + token + " chassis must be configured before its grab.");
            var grab = ScriptableObject.CreateInstance<SummonGrabComponent>();
            grab.GrabWeapons = new[] { claw };
            grab.HoldBuff = hold;
            grab.GrappledBuff = grappled;
            grab.SwallowedBuff = null;
            grab.ConstrictDiceCount = 0;
            grab.ConstrictDiceType = DiceType.D6;
            grab.ConstrictBonus = 0;
            var bonus = ScriptableObject.CreateInstance<ManeuverBonus>();
            bonus.Type = CombatManeuver.Grapple;
            bonus.Bonus = ExpandedSummoningSpecialProfiles.SummonGrabManeuverBonus;
            traits.name = InternalName(traitsSymbol);
            traits.Stacking = StackingType.Replace;
            traits.IsClassFeature = true;
            traits.ComponentsArray = new BlueprintComponent[] { grab, bonus,
                ScriptableObject.CreateInstance<SummonRakeComponent>() };
            BlueprintUnitFactAccess.Resolve().Configure(traits,
                LocalizationService.Create("KMG.ExpandedSummoning." + token +
                    ".CombatTraits.Name", displayName),
                LocalizationService.Create("KMG.ExpandedSummoning." + token +
                    ".CombatTraits.Description",
                    "A claw hit lets the cat attempt to grab its foe; its rake claws strike only on a charge or against a foe it holds."),
                null);
            unit.AddFacts = (unit.AddFacts ?? Array.Empty<BlueprintUnitFact>())
                .Concat(new BlueprintUnitFact[] { traits }).ToArray();
        }

        /// <summary>
        /// Sprint 8: the Cheetah's sprint. A swift extraordinary ability, one
        /// use per summoning, that applies a one-round state carrying an
        /// enhancement bonus to speed (the game's own speed cap still applies);
        /// a cast action on the cheetah's brain spends it once a fight starts.
        /// </summary>
        private static void ConfigureCheetahSprint(
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            BlueprintUnit unit = Require<BlueprintUnit>(bySymbol, CheetahUnitSymbol);
            BlueprintAbility sprint = Require<BlueprintAbility>(bySymbol, CheetahSprintSymbol);
            BlueprintAbilityResource resource = Require<BlueprintAbilityResource>(
                bySymbol, CheetahSprintResourceSymbol);
            BlueprintBuff state = Require<BlueprintBuff>(bySymbol, CheetahSprintStateSymbol);
            BlueprintAiCastSpell ai = Require<BlueprintAiCastSpell>(bySymbol,
                CheetahSprintAiSymbol);
            BlueprintBrain brain = Require<BlueprintBrain>(bySymbol, CheetahBrainSymbol);
            BlueprintBuff traits = Require<BlueprintBuff>(bySymbol, CheetahCombatTraitsSymbol);
            if (unit.ComponentsArray == null ||
                unit.ComponentsArray.OfType<AddClassLevels>().Count() != 1)
                throw new InvalidOperationException(
                    "The Cheetah chassis must be configured before its sprint.");
            var speed = ScriptableObject.CreateInstance<
                Kingmaker.Designers.Mechanics.Buffs.BuffMovementSpeed>();
            speed.Descriptor = ModifierDescriptor.Enhancement;
            speed.Value = ExpandedSummoningSpecialProfiles.CheetahSprintBonusFeet;
            speed.CappedOnMultiplier = false;
            speed.CappedMinimum = false;
            state.name = InternalName(CheetahSprintStateSymbol);
            state.Stacking = StackingType.Replace;
            state.IsClassFeature = false;
            state.ComponentsArray = new BlueprintComponent[] { speed };
            BlueprintUnitFactAccess.Resolve().Configure(state,
                LocalizationService.Create("KMG.ExpandedSummoning.Cheetah.SprintState.Name",
                    "Sprinting"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Cheetah.SprintState.Description",
                    "The cheetah's speed is increased by 30 feet for this round."),
                null);
            sprint.name = InternalName(CheetahSprintSymbol);
            sprint.Type = AbilityType.Extraordinary;
            sprint.Parent = null;
            sprint.Hidden = false;
            sprint.ActionBarAutoFillIgnored = false;
            sprint.Range = AbilityRange.Personal;
            sprint.CanTargetEnemies = false;
            sprint.CanTargetSelf = true;
            sprint.CanTargetFriends = false;
            sprint.CanTargetPoint = false;
            sprint.SpellResistance = false;
            sprint.NeedEquipWeapons = false;
            sprint.EffectOnEnemy = AbilityEffectOnUnit.None;
            sprint.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            sprint.ActionType = UnitCommand.CommandType.Swift;
            sprint.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Immediate;
            sprint.MaterialComponent = new BlueprintAbility.MaterialComponentData();
            sprint.ResourceAssetIds = Array.Empty<string>();
            var apply = ScriptableObject.CreateInstance<ContextActionApplyBuff>();
            apply.Buff = state;
            apply.ToCaster = true;
            apply.DurationValue = new ContextDurationValue {
                Rate = DurationRate.Rounds,
                DiceType = DiceType.Zero,
                DiceCountValue = Simple(0),
                BonusValue = Simple(ExpandedSummoningSpecialProfiles.CheetahSprintRounds)
            };
            apply.IsFromSpell = false;
            apply.IsNotDispelable = true;
            var effect = ScriptableObject.CreateInstance<AbilityEffectRunAction>();
            effect.Actions = new ActionList { Actions = new GameAction[] { apply } };
            var cost = ScriptableObject.CreateInstance<AbilityResourceLogic>();
            cost.RequiredResource = resource;
            cost.IsSpendResource = true;
            cost.CostIsCustom = false;
            cost.Amount = 1;
            sprint.ComponentsArray = new BlueprintComponent[] { cost, effect };
            BlueprintUnitFactAccess.Resolve().Configure(sprint,
                LocalizationService.Create("KMG.ExpandedSummoning.Cheetah.Sprint.Name", "Sprint"),
                LocalizationService.Create("KMG.ExpandedSummoning.Cheetah.Sprint.Description",
                    "Once per summoning, as a swift action, the cheetah sprints: +30 feet of speed for one round."),
                null);
            ConfigureNamedResource(resource, CheetahSprintResourceSymbol,
                "KMG.ExpandedSummoning.Cheetah.Sprint.Resource", "Sprint",
                "Sprints remaining for this summoned cheetah.",
                ExpandedSummoningSpecialProfiles.CheetahSprintUses);
            ai.name = InternalName(CheetahSprintAiSymbol);
            ai.Ability = sprint;
            ai.Variant = null;
            ai.BaseScore = 3;
            ai.CooldownRounds = 0;
            ai.StartCooldownRounds = 0;
            ai.ActorConsiderations = Array.Empty<Kingmaker.Controllers.Brain
                .Blueprints.Considerations.Consideration>();
            ai.TargetConsiderations = Array.Empty<Kingmaker.Controllers.Brain
                .Blueprints.Considerations.Consideration>();
            ai.Locators = Array.Empty<EntityReference>();
            brain.name = InternalName(CheetahBrainSymbol);
            brain.Actions = new BlueprintAiAction[] { ai };
            traits.name = InternalName(CheetahCombatTraitsSymbol);
            traits.Stacking = StackingType.Replace;
            traits.IsClassFeature = true;
            traits.ComponentsArray = new BlueprintComponent[] { AddResource(resource) };
            BlueprintUnitFactAccess.Resolve().Configure(traits,
                LocalizationService.Create("KMG.ExpandedSummoning.Cheetah.CombatTraits.Name",
                    "Cheetah Traits"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Cheetah.CombatTraits.Description",
                    "One sprint for this summoning."), null);
            var grant = ScriptableObject.CreateInstance<AddAbilityToCharacterComponent>();
            grant.Abilities = new[] { sprint };
            unit.ComponentsArray = unit.ComponentsArray.Concat(
                new BlueprintComponent[] { grant }).ToArray();
            unit.Brain = brain;
            unit.AddFacts = (unit.AddFacts ?? Array.Empty<BlueprintUnitFact>())
                .Concat(new BlueprintUnitFact[] { traits }).ToArray();
        }

        /// <summary>
        /// A cat: the shared-lifecycle claw grab (every claw slot, primary or
        /// rake, carries the same weapon blueprint) plus the rake gate.
        /// </summary>
        private static void ConfigureCat(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol, string unitSymbol,
            string traitsSymbol, string token, string displayName, string clawGuid,
            BlueprintBuff hold, BlueprintBuff grappled)
        {
            ConfigureGrabber(library, bySymbol, unitSymbol, traitsSymbol, token, displayName,
                "A claw hit lets the cat attempt to grab its foe; its rake claws strike only on a charge or against a foe it holds.",
                new[] { clawGuid }, hold, grappled, null, 0, 0);
            BlueprintBuff traits = Require<BlueprintBuff>(bySymbol, traitsSymbol);
            traits.ComponentsArray = traits.ComponentsArray.Concat(new BlueprintComponent[] {
                ScriptableObject.CreateInstance<SummonRakeComponent>() }).ToArray();
        }

        /// <summary>
        /// Sprint 6: the Giant Spider's ranged Web. A 50-foot ability against
        /// one foe: a Reflex save (DC 10 + half hit dice + Constitution, as
        /// the native spider poison scales) or the native web-grappled state
        /// for at most ten rounds - the state's own per-round break-free ends
        /// it sooner. Two uses per summoning on a named resource; the brain
        /// spends them when the spider fights. The spider itself carries the
        /// native web immunity (natural profile).
        /// </summary>
        private static void ConfigureGiantSpiderWeb(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            BlueprintUnit unit = Require<BlueprintUnit>(bySymbol, GiantSpiderUnitSymbol);
            BlueprintAbility web = Require<BlueprintAbility>(bySymbol, GiantSpiderWebSymbol);
            BlueprintAbilityResource resource = Require<BlueprintAbilityResource>(
                bySymbol, GiantSpiderWebResourceSymbol);
            BlueprintAiCastSpell ai = Require<BlueprintAiCastSpell>(bySymbol,
                GiantSpiderWebAiSymbol);
            BlueprintBrain brain = Require<BlueprintBrain>(bySymbol, GiantSpiderBrainSymbol);
            BlueprintBuff traits = Require<BlueprintBuff>(bySymbol,
                GiantSpiderCombatTraitsSymbol);
            if (unit.ComponentsArray == null ||
                unit.ComponentsArray.OfType<AddClassLevels>().Count() != 1)
                throw new InvalidOperationException(
                    "The Giant Spider chassis must be configured before its web.");
            BlueprintBuff webbed = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(
                library, NativeWebGrappledGuid, "native web-grappled state");
            web.name = InternalName(GiantSpiderWebSymbol);
            web.Type = AbilityType.Extraordinary;
            web.Parent = null;
            web.Hidden = false;
            web.ActionBarAutoFillIgnored = false;
            web.Range = AbilityRange.Custom;
            web.CustomRange = new Feet(ExpandedSummoningSpecialProfiles.GiantSpiderWebRangeFeet);
            web.CanTargetEnemies = true;
            web.CanTargetSelf = false;
            web.CanTargetFriends = false;
            web.CanTargetPoint = false;
            web.SpellResistance = false;
            web.NeedEquipWeapons = false;
            web.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            web.EffectOnAlly = AbilityEffectOnUnit.None;
            web.ActionType = UnitCommand.CommandType.Standard;
            web.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Point;
            web.MaterialComponent = new BlueprintAbility.MaterialComponentData();
            web.ResourceAssetIds = Array.Empty<string>();
            var apply = ScriptableObject.CreateInstance<ContextActionApplyBuff>();
            apply.Buff = webbed;
            apply.ToCaster = false;
            apply.DurationValue = new ContextDurationValue {
                Rate = DurationRate.Rounds,
                DiceType = DiceType.Zero,
                DiceCountValue = Simple(0),
                BonusValue = Simple(ExpandedSummoningSpecialProfiles.GiantSpiderWebRounds)
            };
            apply.IsFromSpell = false;
            apply.IsNotDispelable = false;
            var saved = ScriptableObject.CreateInstance<ContextActionConditionalSaved>();
            saved.Succeed = new ActionList { Actions = Array.Empty<GameAction>() };
            saved.Failed = new ActionList { Actions = new GameAction[] { apply } };
            var run = ScriptableObject.CreateInstance<AbilityEffectRunAction>();
            run.SavingThrowType = SavingThrowType.Reflex;
            run.Actions = new ActionList { Actions = new GameAction[] { saved } };
            var parameters = ScriptableObject.CreateInstance<ContextCalculateAbilityParams>();
            parameters.StatType = StatType.Constitution;
            parameters.ReplaceCasterLevel = true;
            parameters.CasterLevel = Simple(ExpandedSummoningNaturalProfiles.For("giant-spider").HitDice);
            parameters.ReplaceSpellLevel = true;
            parameters.SpellLevel = Simple(ExpandedSummoningSpecialProfiles.GiantSpiderWebSpellLevel);
            var cost = ScriptableObject.CreateInstance<AbilityResourceLogic>();
            cost.RequiredResource = resource;
            cost.IsSpendResource = true;
            cost.CostIsCustom = false;
            cost.Amount = 1;
            web.ComponentsArray = new BlueprintComponent[] { run, parameters, cost };
            BlueprintUnitFactAccess.Resolve().Configure(web,
                LocalizationService.Create("KMG.ExpandedSummoning.GiantSpider.Web.Name", "Web"),
                LocalizationService.Create("KMG.ExpandedSummoning.GiantSpider.Web.Description",
                    "The spider throws a web at one foe within 50 feet: on a failed Reflex save the foe is entangled and held in place until it breaks free (up to ten rounds). Two uses per summoning."),
                null);
            ConfigureNamedResource(resource, GiantSpiderWebResourceSymbol,
                "KMG.ExpandedSummoning.GiantSpider.Web.Resource", "Web",
                "Webs remaining for this summoned spider.",
                ExpandedSummoningSpecialProfiles.GiantSpiderWebUses);
            ai.name = InternalName(GiantSpiderWebAiSymbol);
            ai.Ability = web;
            ai.Variant = null;
            ai.BaseScore = 3;
            ai.CooldownRounds = 0;
            ai.StartCooldownRounds = 0;
            ai.ActorConsiderations = Array.Empty<Kingmaker.Controllers.Brain
                .Blueprints.Considerations.Consideration>();
            ai.TargetConsiderations = Array.Empty<Kingmaker.Controllers.Brain
                .Blueprints.Considerations.Consideration>();
            ai.Locators = Array.Empty<EntityReference>();
            brain.name = InternalName(GiantSpiderBrainSymbol);
            brain.Actions = new BlueprintAiAction[] { ai };
            traits.name = InternalName(GiantSpiderCombatTraitsSymbol);
            traits.Stacking = StackingType.Replace;
            traits.IsClassFeature = true;
            traits.ComponentsArray = new BlueprintComponent[] { AddResource(resource) };
            BlueprintUnitFactAccess.Resolve().Configure(traits,
                LocalizationService.Create("KMG.ExpandedSummoning.GiantSpider.CombatTraits.Name",
                    "Giant Spider Traits"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.GiantSpider.CombatTraits.Description",
                    "Two webs for this summoning."), null);
            unit.Brain = brain;
            unit.AddFacts = (unit.AddFacts ?? Array.Empty<BlueprintUnitFact>())
                .Concat(new BlueprintUnitFact[] { web, traits }).ToArray();
        }

        private static void ConfigureGrabber(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol,
            string unitSymbol, string traitsSymbol, string token,
            string displayName, string description, string[] weaponGuids,
            BlueprintBuff hold, BlueprintBuff grappled, BlueprintBuff swallowed,
            int constrictDice, int constrictBonus)
        {
            BlueprintUnit unit = Require<BlueprintUnit>(bySymbol, unitSymbol);
            BlueprintBuff traits = Require<BlueprintBuff>(bySymbol, traitsSymbol);
            if (unit.ComponentsArray == null ||
                unit.ComponentsArray.OfType<AddClassLevels>().Count() != 1)
                throw new InvalidOperationException(
                    "The " + token + " chassis must be configured before its grab.");
            var grab = ScriptableObject.CreateInstance<SummonGrabComponent>();
            grab.GrabWeapons = weaponGuids.Select(guid =>
                BlueprintLibraryLookup.RequireExact<BlueprintItemWeapon>(library,
                    guid, token + " grab weapon")).ToArray();
            grab.HoldBuff = hold;
            grab.GrappledBuff = grappled;
            grab.SwallowedBuff = swallowed;
            grab.ConstrictDiceCount = constrictDice;
            grab.ConstrictDiceType = DiceType.D6;
            grab.ConstrictBonus = constrictBonus;
            var bonus = ScriptableObject.CreateInstance<ManeuverBonus>();
            bonus.Type = CombatManeuver.Grapple;
            bonus.Bonus = ExpandedSummoningSpecialProfiles.SummonGrabManeuverBonus;
            var components = new List<BlueprintComponent> { grab, bonus };
            if (swallowed != null)
                components.Add(ScriptableObject.CreateInstance<
                    SummonSwallowLifecycleComponent>());
            traits.name = InternalName(traitsSymbol);
            traits.Stacking = StackingType.Replace;
            traits.IsClassFeature = true;
            traits.ComponentsArray = components.ToArray();
            BlueprintUnitFactAccess.Resolve().Configure(traits,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning." + token + ".CombatTraits.Name",
                    displayName),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning." + token + ".CombatTraits.Description",
                    description), null);
            unit.AddFacts = (unit.AddFacts ?? Array.Empty<BlueprintUnitFact>())
                .Concat(new BlueprintUnitFact[] { traits }).ToArray();
        }

        /// <summary>
        /// Cyclops Flash of Insight, bounded (Sprint 3). The natural builder
        /// owns the chassis; this adds one once-per-summoning swift ability
        /// whose one-round state makes the next attack an automatic critical
        /// hit, the resource that limits it, and a brain that
        /// spends it when the cyclops fights. It runs after the natural
        /// builder and only appends to what that builder configured.
        /// </summary>
        private static void ConfigureCyclops(
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            BlueprintUnit unit = Require<BlueprintUnit>(bySymbol, CyclopsUnitSymbol);
            BlueprintAbility flash = Require<BlueprintAbility>(bySymbol,
                CyclopsFlashSymbol);
            BlueprintBuff state = Require<BlueprintBuff>(bySymbol,
                CyclopsFlashStateSymbol);
            BlueprintAbilityResource resource = Require<BlueprintAbilityResource>(
                bySymbol, CyclopsFlashResourceSymbol);
            BlueprintBuff traits = Require<BlueprintBuff>(bySymbol,
                CyclopsCombatTraitsSymbol);
            BlueprintAiCastSpell ai = Require<BlueprintAiCastSpell>(bySymbol,
                CyclopsFlashAiSymbol);
            BlueprintBrain brain = Require<BlueprintBrain>(bySymbol,
                CyclopsBrainSymbol);
            if (unit.ComponentsArray == null ||
                unit.ComponentsArray.OfType<AddClassLevels>().Count() != 1)
                throw new InvalidOperationException(
                    "The Cyclops chassis must be configured before its special traits.");
            ConfigureNamedResource(resource, CyclopsFlashResourceSymbol,
                "KMG.ExpandedSummoning.Cyclops.FlashOfInsight.Resource",
                "Flash of Insight", "Uses remaining for this summoned Cyclops.",
                ExpandedSummoningSpecialProfiles.CyclopsFlashOfInsightUses);
            ConfigureCyclopsFlashState(state);
            ConfigureCyclopsFlash(flash, state, resource);
            ConfigureCyclopsCombatTraits(traits, resource);
            ai.name = InternalName(CyclopsFlashAiSymbol);
            ai.Ability = flash;
            ai.Variant = null;
            ai.BaseScore = 3;
            ai.CooldownRounds = 0;
            ai.StartCooldownRounds = 0;
            ai.ActorConsiderations = Array.Empty<Kingmaker.Controllers.Brain
                .Blueprints.Considerations.Consideration>();
            ai.TargetConsiderations = Array.Empty<Kingmaker.Controllers.Brain
                .Blueprints.Considerations.Consideration>();
            ai.Locators = Array.Empty<EntityReference>();
            brain.name = InternalName(CyclopsBrainSymbol);
            brain.Actions = new BlueprintAiAction[] { ai };
            var grant = ScriptableObject.CreateInstance<
                AddAbilityToCharacterComponent>();
            grant.Abilities = new[] { flash };
            unit.ComponentsArray = unit.ComponentsArray.Concat(
                new BlueprintComponent[] { grant }).ToArray();
            unit.Brain = brain;
            unit.AddFacts = (unit.AddFacts ?? Array.Empty<BlueprintUnitFact>())
                .Concat(new BlueprintUnitFact[] { traits }).ToArray();
        }

        private static void ConfigureCyclopsFlashState(BlueprintBuff buff)
        {
            var insight = ScriptableObject.CreateInstance<
                CyclopsFlashOfInsightComponent>();
            var oneAttack = ScriptableObject.CreateInstance<RemoveBuffOnAttack>();
            buff.name = InternalName(CyclopsFlashStateSymbol);
            buff.Stacking = StackingType.Replace;
            buff.IsClassFeature = false;
            buff.ComponentsArray = new BlueprintComponent[] { insight, oneAttack };
            BlueprintUnitFactAccess.Resolve().Configure(buff,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Cyclops.FlashOfInsight.State.Name",
                    "Flash of Insight"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Cyclops.FlashOfInsight.State.Description",
                    "This cyclops's next attack is an automatic critical hit."),
                null);
        }

        private static void ConfigureCyclopsFlash(BlueprintAbility ability,
            BlueprintBuff state, BlueprintAbilityResource resource)
        {
            ability.name = InternalName(CyclopsFlashSymbol);
            ability.Type = AbilityType.Supernatural;
            ability.Parent = null;
            ability.Hidden = false;
            ability.ActionBarAutoFillIgnored = false;
            ability.Range = AbilityRange.Personal;
            ability.CanTargetEnemies = false;
            ability.CanTargetSelf = true;
            ability.CanTargetFriends = false;
            ability.CanTargetPoint = false;
            ability.SpellResistance = false;
            ability.NeedEquipWeapons = false;
            ability.EffectOnEnemy = AbilityEffectOnUnit.None;
            ability.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            ability.ActionType = UnitCommand.CommandType.Swift;
            ability.Animation = UnitAnimationActionCastSpell.CastAnimationStyle
                .Immediate;
            ability.MaterialComponent = new BlueprintAbility.MaterialComponentData();
            ability.ResourceAssetIds = Array.Empty<string>();
            ContextActionApplyBuff arm = ScriptableObject.CreateInstance<
                ContextActionApplyBuff>();
            arm.Buff = state;
            arm.ToCaster = true;
            arm.DurationValue = new ContextDurationValue {
                    Rate = DurationRate.Rounds,
                    DiceType = DiceType.Zero,
                    DiceCountValue = Simple(0),
                    BonusValue = Simple(ExpandedSummoningSpecialProfiles
                        .CyclopsFlashOfInsightRounds)
                };
            arm.IsFromSpell = false;
            arm.IsNotDispelable = true;
            var effect = ScriptableObject.CreateInstance<AbilityEffectRunAction>();
            effect.Actions = new ActionList { Actions = new GameAction[] { arm } };
            var cost = ScriptableObject.CreateInstance<AbilityResourceLogic>();
            cost.RequiredResource = resource;
            cost.IsSpendResource = true;
            cost.CostIsCustom = false;
            cost.Amount = 1;
            ability.ComponentsArray = new BlueprintComponent[] { cost, effect };
            BlueprintUnitFactAccess.Resolve().Configure(ability,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Cyclops.FlashOfInsight.Name",
                    "Flash of Insight"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Cyclops.FlashOfInsight.Description",
                    "Once per summoning, as a swift action, the cyclops's next attack this round is an automatic critical hit."),
                null);
        }

        private static void ConfigureCyclopsCombatTraits(BlueprintBuff buff,
            BlueprintAbilityResource resource)
        {
            buff.name = InternalName(CyclopsCombatTraitsSymbol);
            buff.Stacking = StackingType.Replace;
            buff.IsClassFeature = true;
            buff.ComponentsArray = new BlueprintComponent[] {
                AddResource(resource) };
            BlueprintUnitFactAccess.Resolve().Configure(buff,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Cyclops.CombatTraits.Name",
                    "Cyclops Combat Traits"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Cyclops.CombatTraits.Description",
                    "One use of Flash of Insight for this summoning."),
                null);
        }

        private static AddClassLevels OutsiderLevels(
            LibraryScriptableObject library, int hitDice)
        {
            var levels = ScriptableObject.CreateInstance<AddClassLevels>();
            levels.CharacterClass = BlueprintLibraryLookup.RequireExact<
                BlueprintCharacterClass>(library, OutsiderClassGuid,
                    "native outsider class");
            levels.Levels = hitDice;
            levels.RaceStat = StatType.Constitution;
            levels.LevelsStat = StatType.Unknown;
            levels.Skills = new[] { StatType.SkillPerception,
                StatType.SkillMobility, StatType.SkillPersuasion };
            levels.Archetypes = Array.Empty<BlueprintArchetype>();
            levels.SelectSpells = Array.Empty<BlueprintAbility>();
            levels.MemorizeSpells = Array.Empty<BlueprintAbility>();
            levels.Selections = Array.Empty<SelectionEntry>();
            return levels;
        }

        private static AddClassLevels FeyLevels(LibraryScriptableObject library,
            int hitDice)
        {
            var levels = ScriptableObject.CreateInstance<AddClassLevels>();
            levels.CharacterClass = BlueprintLibraryLookup.RequireExact<
                BlueprintCharacterClass>(library, FeyClassGuid,
                    "native fey class");
            levels.Levels = hitDice;
            levels.RaceStat = StatType.Constitution;
            levels.LevelsStat = StatType.Unknown;
            levels.Skills = new[] { StatType.SkillPerception,
                StatType.SkillMobility, StatType.SkillPersuasion,
                StatType.SkillStealth };
            levels.Archetypes = Array.Empty<BlueprintArchetype>();
            levels.SelectSpells = Array.Empty<BlueprintAbility>();
            levels.MemorizeSpells = Array.Empty<BlueprintAbility>();
            levels.Selections = Array.Empty<SelectionEntry>();
            return levels;
        }

        private static BlueprintUnit.UnitBody NaturalBody(
            BlueprintItemWeapon primary, BlueprintItemWeapon[] additional,
            BlueprintItemWeapon[] secondary)
        {
            return new BlueprintUnit.UnitBody {
                DisableHands = false,
                PrimaryHand = primary,
                AdditionalLimbs = additional ?? Array.Empty<BlueprintItemWeapon>(),
                AdditionalSecondaryLimbs = secondary ??
                    Array.Empty<BlueprintItemWeapon>(),
                QuickSlots = Array.Empty<
                    Kingmaker.Blueprints.Items.Equipment.BlueprintItemEquipmentUsable>()
            };
        }

        private static void ConfigureUnitCore(BlueprintUnit unit, string key,
            string displayName, Alignment alignment, Size size, int strength,
            int dexterity, int constitution, int intelligence, int wisdom,
            int charisma, int speedFeet)
        {
            var unitName = ScriptableObject.CreateInstance<SharedStringAsset>();
            unitName.String = LocalizationService.Create(
                "KMG.ExpandedSummoning." + key + ".Unit.Name", displayName);
            unit.LocalizedName = unitName;
            unit.Alignment = alignment;
            unit.Size = size;
            unit.Strength = strength;
            unit.Dexterity = dexterity;
            unit.Constitution = constitution;
            unit.Intelligence = intelligence;
            unit.Wisdom = wisdom;
            unit.Charisma = charisma;
            unit.Speed = new Feet(speedFeet);
            unit.BaseAttackBonus = 0;
            unit.MaxHP = 0;
            unit.StartingInventory = Array.Empty<BlueprintItem>();
        }

        private static AddDamageResistanceEnergy Energy(DamageEnergyType type,
            int value)
        {
            var result = ScriptableObject.CreateInstance<
                AddDamageResistanceEnergy>();
            result.Type = type;
            result.Value = Simple(value);
            return result;
        }

        private static ContextActionDealDamage ColdDamage(int diceCount)
        { return EnergyDamage(DamageEnergyType.Cold, diceCount); }

        private static ContextActionDealDamage EnergyDamage(
            DamageEnergyType energy, int diceCount)
        {
            ContextActionDealDamage result = ScriptableObject.CreateInstance<
                ContextActionDealDamage>();
            result.DamageType = new DamageTypeDescription {
                    Type = DamageType.Energy,
                    Energy = energy
                };
            result.Value = new ContextDiceValue {
                    DiceType = DiceType.D6,
                    DiceCountValue = Simple(diceCount),
                    BonusValue = Simple(0)
                };
            return result;
        }

        private static void ConfigureInvisibleStalker(
            LibraryScriptableObject library, BlueprintUnit unit,
            BlueprintFeature extraplanar)
        {
            BlueprintItemWeapon slam = BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, LargeAirSlamGuid,
                    "Invisible Stalker 2d6 slam");
            unit.ComponentsArray = new BlueprintComponent[] {
                OutsiderLevels(library,
                    ExpandedSummoningSpecialProfiles.InvisibleStalkerHitDice)
            };
            unit.Body = NaturalBody(slam, new[] { slam },
                Array.Empty<BlueprintItemWeapon>());
            unit.Brain = BlueprintLibraryLookup.RequireExact<BlueprintBrain>(
                library, DumbBrainGuid, "bounded natural-attack brain");
            ConfigureUnitCore(unit, "InvisibleStalker", "Invisible Stalker",
                Alignment.TrueNeutral, Size.Medium,
                ExpandedSummoningSpecialProfiles.InvisibleStalkerStrength,
                ExpandedSummoningSpecialProfiles.InvisibleStalkerDexterity,
                ExpandedSummoningSpecialProfiles.InvisibleStalkerConstitution,
                ExpandedSummoningSpecialProfiles.InvisibleStalkerIntelligence,
                ExpandedSummoningSpecialProfiles.InvisibleStalkerWisdom,
                ExpandedSummoningSpecialProfiles.InvisibleStalkerCharisma,
                ExpandedSummoningSpecialProfiles.InvisibleStalkerSpeedFeet);
            unit.AddFacts = new BlueprintUnitFact[] {
                BlueprintLibraryLookup.RequireExact<BlueprintUnitFact>(library,
                    NaturalArmor6Guid, "natural armor +6"),
                Feature(library, ElementalSubtypeGuid, "elemental subtype"),
                Feature(library, AirSubtypeGuid, "air subtype"),
                extraplanar,
                Feature(library, ImprovedInitiativeGuid, "Improved Initiative"),
                Feature(library, CombatReflexesGuid, "Combat Reflexes"),
                Feature(library, LightningReflexesGuid, "Lightning Reflexes"),
                Feature(library, WeaponFocusSlamGuid, "Weapon Focus (slam)"),
                BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library,
                    NaturalInvisibilityGuid, "attack-safe natural invisibility")
            };
        }

        private static void ConfigureErinyes(LibraryScriptableObject library,
            BlueprintUnit unit)
        {
            BlueprintItemWeapon ranged = unit.Body == null ? null :
                unit.Body.PrimaryHand as BlueprintItemWeapon;
            if (ranged == null || !ranged.IsRanged || unit.Brain == null)
                throw new InvalidOperationException(
                    "Erinyes donor lacks its proven ranged body/brain chassis.");
            unit.ComponentsArray = new BlueprintComponent[] {
                OutsiderLevels(library,
                    ExpandedSummoningSpecialProfiles.ErinyesHitDice)
            };
            ConfigureUnitCore(unit, "ErinyesDevil", "Erinyes Devil",
                Alignment.LawfulEvil, Size.Medium,
                ExpandedSummoningSpecialProfiles.ErinyesStrength,
                ExpandedSummoningSpecialProfiles.ErinyesDexterity,
                ExpandedSummoningSpecialProfiles.ErinyesConstitution,
                ExpandedSummoningSpecialProfiles.ErinyesIntelligence,
                ExpandedSummoningSpecialProfiles.ErinyesWisdom,
                ExpandedSummoningSpecialProfiles.ErinyesCharisma,
                ExpandedSummoningSpecialProfiles.ErinyesSpeedFeet);
        }

        private static void ConfigureShadowDemonCombatTraits(BlueprintBuff buff)
        {
            var dr = ScriptableObject.CreateInstance<AddDamageResistancePhysical>();
            dr.Value = Simple(ExpandedSummoningSpecialProfiles
                .ShadowDemonDamageReduction);
            dr.Or = true;
            dr.BypassedByMaterial = true;
            dr.Material = PhysicalDamageMaterial.ColdIron;
            dr.BypassedByAlignment = true;
            dr.Alignment = DamageAlignment.Good;
            var acid = Energy(DamageEnergyType.Acid,
                ExpandedSummoningSpecialProfiles.ShadowDemonEnergyResistance);
            var fire = Energy(DamageEnergyType.Fire,
                ExpandedSummoningSpecialProfiles.ShadowDemonEnergyResistance);
            var sr = ScriptableObject.CreateInstance<AddSpellResistance>();
            sr.Value = Simple(ExpandedSummoningSpecialProfiles
                .ShadowDemonSpellResistance);
            sr.AddCR = false;
            var cold = ScriptableObject.CreateInstance<
                AddInitiatorAttackWithWeaponTrigger>();
            cold.OnlyHit = true;
            cold.AllNaturalAndUnarmed = true;
            cold.Action = new ActionList { Actions = new GameAction[] {
                ColdDamage(ExpandedSummoningSpecialProfiles
                    .ShadowDemonColdDamageDice)
            }};
            buff.Stacking = StackingType.Replace;
            buff.IsClassFeature = true;
            buff.ComponentsArray = new BlueprintComponent[] {
                dr, acid, fire, sr, cold
            };
            BlueprintUnitFactAccess.Resolve().Configure(buff,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.ShadowDemon.CombatTraits.Name",
                    "Shadow Demon Combat Traits"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.ShadowDemon.CombatTraits.Description",
                    "DR 10/cold iron or good, acid and fire resistance 10, spell resistance 17, and 1d6 cold damage on natural attacks."),
                null);
        }

        private static void ConfigureShadowDemon(LibraryScriptableObject library,
            BlueprintUnit unit, BlueprintBuff combatTraits,
            BlueprintFeature extraplanar)
        {
            BlueprintItemWeapon claw = BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, LargeClawGuid,
                    "Shadow Demon 1d6 claw");
            BlueprintItemWeapon bite = BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, LargeBiteGuid,
                    "Shadow Demon 1d8 bite");
            unit.ComponentsArray = new BlueprintComponent[] {
                OutsiderLevels(library,
                    ExpandedSummoningSpecialProfiles.ShadowDemonHitDice)
            };
            unit.Body = NaturalBody(claw, new[] { claw }, new[] { bite });
            unit.Brain = BlueprintLibraryLookup.RequireExact<BlueprintBrain>(
                library, DumbBrainGuid, "bounded natural-attack brain");
            ConfigureUnitCore(unit, "ShadowDemon", "Shadow Demon",
                Alignment.ChaoticEvil, Size.Medium,
                ExpandedSummoningSpecialProfiles.ShadowDemonStrength,
                ExpandedSummoningSpecialProfiles.ShadowDemonDexterity,
                ExpandedSummoningSpecialProfiles.ShadowDemonConstitution,
                ExpandedSummoningSpecialProfiles.ShadowDemonIntelligence,
                ExpandedSummoningSpecialProfiles.ShadowDemonWisdom,
                ExpandedSummoningSpecialProfiles.ShadowDemonCharisma,
                ExpandedSummoningSpecialProfiles.ShadowDemonSpeedFeet);
            unit.AddFacts = new BlueprintUnitFact[] {
                Feature(library, ChaoticSubtypeGuid, "chaotic subtype"),
                Feature(library, EvilSubtypeGuid, "evil subtype"),
                extraplanar,
                Feature(library, IncorporealGuid, "incorporeal defenses"),
                Feature(library, ColdImmunityGuid, "cold immunity"),
                Feature(library, ElectricityImmunityGuid, "electricity immunity"),
                Feature(library, PoisonImmunityGuid, "poison immunity"),
                Feature(library, ImprovedInitiativeGuid, "Improved Initiative"),
                Feature(library, WeaponFocusClawGuid, "Weapon Focus (claw)"),
                combatTraits
            };
        }

        private static void ConfigureSalamanderTail(
            LibraryScriptableObject library, BlueprintItemWeapon tail)
        {
            BlueprintItemWeapon native = BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, LargeTailGuid,
                    "native animated tail weapon");
            CopyFields(native, tail);
            tail.name = InternalName(SalamanderTailSymbol);
            tail.ComponentsArray = (native.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Select(
                    ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            SetField(tail, "m_OverrideDamageDice", true);
            SetField(tail, "m_DamageDice", new DiceFormula(2, DiceType.D6));
            SetField(tail, "m_Enchantments", Array.Empty<
                Kingmaker.Blueprints.Items.Ecnchantments.BlueprintWeaponEnchantment>());
        }

        private static void ConfigureSalamanderSpear(
            LibraryScriptableObject library, BlueprintItemWeapon spear,
            BlueprintWeaponType spearType)
        {
            BlueprintItemWeapon native = BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, StandardSpearGuid,
                    "standard 1d8 spear");
            CopyFields(native, spear);
            spear.name = InternalName(SalamanderSpearSymbol);
            spear.ComponentsArray = (native.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Select(
                    ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            SetField(spear, "m_Type", spearType);
            spear.IsNonRemovable = true;
            SetField(spear, "m_Cost", 0);
            SetField(spear, "m_Weight", 0f);
            SetField(spear, "m_Enchantments", Array.Empty<
                Kingmaker.Blueprints.Items.Ecnchantments.BlueprintWeaponEnchantment>());
        }

        private static void ConfigureSalamanderCombatTraits(
            LibraryScriptableObject library, BlueprintBuff buff,
            BlueprintItemWeapon tail)
        {
            BlueprintFeature nativeGrab = BlueprintLibraryLookup.RequireExact<
                BlueprintFeature>(library, NativeGrabGuid,
                    "native bounded grab/constrict graph");
            AddInitiatorAttackWithWeaponTrigger grab =
                (AddInitiatorAttackWithWeaponTrigger)
                ExpandedSummoningAbilityBuilder.DeepCloneComponent(
                    nativeGrab.ComponentsArray.OfType<
                        AddInitiatorAttackWithWeaponTrigger>().Single());
            grab.WeaponType = tail.Type;
            ContextActionDealDamage constrict = FindDamage(
                new BlueprintComponent[] { grab });
            constrict.Value = new ContextDiceValue {
                DiceType = DiceType.D6,
                DiceCountValue = Simple(ExpandedSummoningSpecialProfiles
                    .SalamanderConstrictDice),
                BonusValue = Simple(ExpandedSummoningSpecialProfiles
                    .SalamanderConstrictBonus)
            };
            ManeuverBonus grappleBonus = (ManeuverBonus)
                ExpandedSummoningAbilityBuilder.DeepCloneComponent(
                    nativeGrab.ComponentsArray.OfType<ManeuverBonus>().Single());
            var heat = ScriptableObject.CreateInstance<
                AddInitiatorAttackWithWeaponTrigger>();
            heat.OnlyHit = true;
            heat.Action = new ActionList { Actions = new GameAction[] {
                EnergyDamage(DamageEnergyType.Fire,
                    ExpandedSummoningSpecialProfiles.SalamanderHeatDice)
            }};
            buff.Stacking = StackingType.Replace;
            buff.IsClassFeature = true;
            buff.ComponentsArray = new BlueprintComponent[] {
                grab, grappleBonus, heat
            };
            BlueprintUnitFactAccess.Resolve().Configure(buff,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Salamander.CombatTraits.Name",
                    "Salamander Heat and Constrict"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Salamander.CombatTraits.Description",
                    "Successful attacks deal 1d6 fire damage; tail hits can grab and constrict for 2d6+4 damage."), null);
        }

        private static void ConfigureSalamander(LibraryScriptableObject library,
            BlueprintUnit unit, BlueprintItemWeapon spear,
            BlueprintItemWeapon tail,
            BlueprintBuff combatTraits, BlueprintFeature extraplanar)
        {
            unit.ComponentsArray = new BlueprintComponent[] {
                OutsiderLevels(library,
                    ExpandedSummoningSpecialProfiles.SalamanderHitDice)
            };
            unit.Body = NaturalBody(spear, Array.Empty<BlueprintItemWeapon>(),
                new[] { tail });
            unit.Brain = BlueprintLibraryLookup.RequireExact<BlueprintBrain>(
                library, DumbBrainGuid, "bounded natural-attack brain");
            ConfigureUnitCore(unit, "Salamander", "Salamander",
                Alignment.ChaoticEvil, Size.Medium,
                ExpandedSummoningSpecialProfiles.SalamanderStrength,
                ExpandedSummoningSpecialProfiles.SalamanderDexterity,
                ExpandedSummoningSpecialProfiles.SalamanderConstitution,
                ExpandedSummoningSpecialProfiles.SalamanderIntelligence,
                ExpandedSummoningSpecialProfiles.SalamanderWisdom,
                ExpandedSummoningSpecialProfiles.SalamanderCharisma,
                ExpandedSummoningSpecialProfiles.SalamanderSpeedFeet);
            unit.AddFacts = new BlueprintUnitFact[] {
                BlueprintLibraryLookup.RequireExact<BlueprintUnitFact>(library,
                    NaturalArmor7Guid, "natural armor +7"),
                Feature(library, DrMagic10Guid, "DR 10/magic"),
                Feature(library, FireSubtypeGuid, "fire subtype"),
                extraplanar,
                Feature(library, WeaponFocusSpearGuid, "Weapon Focus (spear)"),
                combatTraits
            };
        }

        private static void ConfigureSuccubusDomination(
            LibraryScriptableObject library, BlueprintBuff buff)
        {
            BlueprintBuff native = BlueprintLibraryLookup.RequireExact<
                BlueprintBuff>(library, NativeDominationGuid,
                    "native Dominate Person target state");
            CopyFields(native, buff);
            buff.name = InternalName(SuccubusDominationSymbol);
            var components = (native.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Select(
                    ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToList();
            components.Add(ScriptableObject.CreateInstance<
                RemoveBuffIfCasterIsMissing>());
            buff.ComponentsArray = components.ToArray();
            buff.ResourceAssetIds = Array.Empty<string>();
            BlueprintUnitFactAccess.Resolve().Configure(buff,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Succubus.Domination.Name",
                    "Succubus Domination"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Succubus.Domination.Description",
                    "A bounded domination effect that ends when its summoned caster is missing."),
                native.Icon);
        }

        private static void ConfigureSuccubusDominate(
            LibraryScriptableObject library, BlueprintAbility ability,
            BlueprintBuff domination)
        {
            BlueprintAbility native = BlueprintLibraryLookup.RequireExact<
                BlueprintAbility>(library, NativeDominateGuid,
                    "native Dominate Person presentation");
            ExpandedSummoningAbilityBuilder.CopyFields(native, ability);
            ability.name = InternalName(SuccubusDominateSymbol);
            ability.Type = AbilityType.SpellLike;
            ability.Hidden = false;
            ability.ActionBarAutoFillIgnored = false;
            ability.Parent = null;
            ability.MaterialComponent = new BlueprintAbility.MaterialComponentData();
            ability.ResourceAssetIds = Array.Empty<string>();
            var spell = ScriptableObject.CreateInstance<SpellComponent>();
            spell.School = SpellSchool.Enchantment;
            var descriptor = ScriptableObject.CreateInstance<
                SpellDescriptorComponent>();
            descriptor.Descriptor = SpellDescriptor.MindAffecting |
                SpellDescriptor.Compulsion;
            var parameters = ScriptableObject.CreateInstance<
                ContextCalculateAbilityParams>();
            parameters.StatType = StatType.Charisma;
            parameters.ReplaceCasterLevel = true;
            parameters.CasterLevel = Simple(
                ExpandedSummoningSpecialProfiles.SuccubusHitDice);
            parameters.ReplaceSpellLevel = true;
            parameters.SpellLevel = Simple(4);
            var targets = ScriptableObject.CreateInstance<AbilityTargetHasFact>();
            targets.Inverted = true;
            targets.CheckedFacts = new BlueprintUnitFact[] {
                Feature(library, AberrationTypeGuid, "aberration type"),
                Feature(library, ConstructTypeGuid, "construct type"),
                Feature(library, DragonTypeGuid, "dragon type"),
                Feature(library, FeyTypeGuid, "fey type"),
                Feature(library, OutsiderTypeGuid, "outsider type"),
                Feature(library, PlantTypeGuid, "plant type"),
                Feature(library, AnimalTypeGuid, "animal type"),
                Feature(library, MonstrousHumanoidTypeGuid,
                    "monstrous humanoid type"),
                Feature(library, MagicalBeastTypeGuid, "magical beast type"),
                Feature(library, VerminTypeGuid, "vermin type"),
                Feature(library, UndeadTypeGuid, "undead type")
            };
            ContextActionSuccubusDominate apply = ScriptableObject.CreateInstance<
                ContextActionSuccubusDominate>();
            apply.Domination = domination;
            // ContextActionSuccubusDominate applies the bounded
            // SuccubusDominateRounds duration after its native Will save.
            var effect = ScriptableObject.CreateInstance<AbilityEffectRunAction>();
            effect.SavingThrowType = SavingThrowType.Unknown;
            effect.Actions = new ActionList { Actions = new GameAction[] { apply } };
            ability.ComponentsArray = new BlueprintComponent[] {
                spell, descriptor, parameters, targets, effect
            };
            BlueprintUnitFactAccess.Resolve().Configure(ability,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Succubus.Dominate.Name",
                    "Dominate Person"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Succubus.Dominate.Description",
                    "A humanoid that fails a Will save is dominated for 3 rounds. The effect ends immediately if the summoned succubus is gone."),
                native.Icon);
        }

        private static void ConfigureSuccubusAi(BlueprintAiCastSpell ai,
            BlueprintAbility dominate, BlueprintBrain brain)
        {
            ai.name = InternalName(SuccubusDominateAiSymbol);
            ai.Ability = dominate;
            ai.Variant = null;
            ai.BaseScore = 2;
            ai.CooldownRounds = 1;
            ai.StartCooldownRounds = 0;
            ai.ActorConsiderations = Array.Empty<Kingmaker.Controllers.Brain
                .Blueprints.Considerations.Consideration>();
            ai.TargetConsiderations = Array.Empty<Kingmaker.Controllers.Brain
                .Blueprints.Considerations.Consideration>();
            ai.Locators = Array.Empty<EntityReference>();
            brain.name = InternalName(SuccubusBrainSymbol);
            brain.Actions = new BlueprintAiAction[] { ai };
        }

        private static void ConfigureSuccubusCombatTraits(
            LibraryScriptableObject library, BlueprintBuff buff)
        {
            var dr = ScriptableObject.CreateInstance<AddDamageResistancePhysical>();
            dr.Value = Simple(ExpandedSummoningSpecialProfiles
                .SuccubusDamageReduction);
            dr.Or = true;
            dr.BypassedByMaterial = true;
            dr.Material = PhysicalDamageMaterial.ColdIron;
            dr.BypassedByAlignment = true;
            dr.Alignment = DamageAlignment.Good;
            var acid = Energy(DamageEnergyType.Acid,
                ExpandedSummoningSpecialProfiles.SuccubusEnergyResistance);
            var cold = Energy(DamageEnergyType.Cold,
                ExpandedSummoningSpecialProfiles.SuccubusEnergyResistance);
            var sr = ScriptableObject.CreateInstance<AddSpellResistance>();
            sr.Value = Simple(ExpandedSummoningSpecialProfiles
                .SuccubusSpellResistance);
            BlueprintFeature nativeDrain = BlueprintLibraryLookup.RequireExact<
                BlueprintFeature>(library, NativeEnergyDrainGuid,
                    "native energy-drain attack trigger");
            AddInitiatorAttackWithWeaponTrigger drain =
                (AddInitiatorAttackWithWeaponTrigger)
                ExpandedSummoningAbilityBuilder.DeepCloneComponent(
                    nativeDrain.ComponentsArray.OfType<
                        AddInitiatorAttackWithWeaponTrigger>().Single());
            drain.OnlyOnFirstHit = true;
            ContextActionDealDamage drainAction = FindDamage(
                new BlueprintComponent[] { drain });
            drainAction.EnergyDrainType = EnergyDrainType.Temporary;
            drainAction.Duration = new ContextDurationValue {
                Rate = DurationRate.Rounds,
                BonusValue = ExpandedSummoningSpecialProfiles
                    .SuccubusEnergyDrainRounds
            };
            drainAction.Value = new ContextDiceValue {
                DiceType = DiceType.Zero,
                DiceCountValue = Simple(0),
                BonusValue = Simple(1)
            };
            buff.Stacking = StackingType.Replace;
            buff.IsClassFeature = true;
            buff.ComponentsArray = new BlueprintComponent[] {
                dr, acid, cold, sr, drain
            };
            BlueprintUnitFactAccess.Resolve().Configure(buff,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Succubus.CombatTraits.Name",
                    "Succubus Combat Traits"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Succubus.CombatTraits.Description",
                    "Demonic defenses and a first-hit energy drain that applies one temporary negative level for one round."), null);
        }

        private static void ConfigureSuccubus(LibraryScriptableObject library,
            BlueprintUnit unit, BlueprintAbility dominate,
            BlueprintBrain brain, BlueprintBuff combatTraits,
            BlueprintFeature extraplanar)
        {
            BlueprintItemWeapon claw = BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, LargeClawGuid,
                    "Succubus 1d6 claw");
            var grant = ScriptableObject.CreateInstance<
                AddAbilityToCharacterComponent>();
            grant.Abilities = new[] { dominate };
            unit.ComponentsArray = new BlueprintComponent[] {
                OutsiderLevels(library,
                    ExpandedSummoningSpecialProfiles.SuccubusHitDice), grant
            };
            unit.Body = NaturalBody(claw, new[] { claw },
                Array.Empty<BlueprintItemWeapon>());
            unit.Brain = brain;
            ConfigureUnitCore(unit, "Succubus", "Succubus",
                Alignment.ChaoticEvil, Size.Medium,
                ExpandedSummoningSpecialProfiles.SuccubusStrength,
                ExpandedSummoningSpecialProfiles.SuccubusDexterity,
                ExpandedSummoningSpecialProfiles.SuccubusConstitution,
                ExpandedSummoningSpecialProfiles.SuccubusIntelligence,
                ExpandedSummoningSpecialProfiles.SuccubusWisdom,
                ExpandedSummoningSpecialProfiles.SuccubusCharisma,
                ExpandedSummoningSpecialProfiles.SuccubusSpeedFeet);
            unit.AddFacts = new BlueprintUnitFact[] {
                BlueprintLibraryLookup.RequireExact<BlueprintUnitFact>(library,
                    NaturalArmor7Guid, "natural armor +7"),
                Feature(library, ChaoticSubtypeGuid, "chaotic subtype"),
                Feature(library, EvilSubtypeGuid, "evil subtype"),
                extraplanar,
                Feature(library, FireImmunityGuid, "fire immunity"),
                Feature(library, ElectricityImmunityGuid, "electricity immunity"),
                Feature(library, PoisonImmunityGuid, "poison immunity"),
                Feature(library, DodgeGuid, "Dodge"),
                Feature(library, WeaponFinesseGuid, "Weapon Finesse"),
                combatTraits
            };
        }

        private static void ConfigureBebelithClaw(
            LibraryScriptableObject library, BlueprintItemWeapon claw)
        {
            BlueprintItemWeapon native = BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, LargeClawGuid,
                    "native animated claw weapon");
            CopyFields(native, claw);
            claw.name = InternalName(BebelithClawSymbol);
            claw.ComponentsArray = (native.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Select(
                    ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            SetField(claw, "m_OverrideDamageDice", true);
            SetField(claw, "m_DamageDice", new DiceFormula(2, DiceType.D4));
            SetField(claw, "m_Enchantments", Array.Empty<
                Kingmaker.Blueprints.Items.Ecnchantments.BlueprintWeaponEnchantment>());
        }

        private static void ConfigureBebelithDismantledArmor(BlueprintBuff buff)
        {
            var penalty = ScriptableObject.CreateInstance<AddStatBonus>();
            penalty.Stat = StatType.AC;
            penalty.Descriptor = ModifierDescriptor.UntypedStackable;
            penalty.Value = -ExpandedSummoningSpecialProfiles
                .BebelithDismantleAcPenalty;
            buff.Stacking = StackingType.Replace;
            buff.IsClassFeature = false;
            buff.ComponentsArray = new BlueprintComponent[] { penalty };
            BlueprintUnitFactAccess.Resolve().Configure(buff,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Bebelith.DismantledArmor.Name",
                    "Dismantled Armor"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Bebelith.DismantledArmor.Description",
                    "A Bebelith caught and tore the armor. The target takes a bounded -2 AC penalty for one round; no equipped item is mutated."),
                null);
        }

        private static void ConfigureBebelithCombatTraits(
            LibraryScriptableObject library, BlueprintBuff buff,
            BlueprintItemWeapon claw, BlueprintBuff dismantledArmor)
        {
            BlueprintItemWeapon bite = BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, HugeBiteGuid,
                    "Bebelith 2d6 bite");
            var naturalArmor = ScriptableObject.CreateInstance<AddStatBonus>();
            naturalArmor.Stat = StatType.AC;
            naturalArmor.Descriptor = ModifierDescriptor.NaturalArmor;
            naturalArmor.Value = 13;
            var dr = ScriptableObject.CreateInstance<AddDamageResistancePhysical>();
            dr.Value = Simple(ExpandedSummoningSpecialProfiles
                .BebelithDamageReduction);
            dr.BypassedByAlignment = true;
            dr.Alignment = DamageAlignment.Good;
            var combat = ScriptableObject.CreateInstance<
                BebelithCombatComponent>();
            combat.Claw = claw;
            combat.Bite = bite;
            combat.OutsiderType = Feature(library, OutsiderTypeGuid,
                "outsider creature type");
            combat.DismantledArmor = dismantledArmor;
            buff.Stacking = StackingType.Replace;
            buff.IsClassFeature = true;
            buff.ComponentsArray = new BlueprintComponent[] {
                naturalArmor, dr, combat
            };
            BlueprintUnitFactAccess.Resolve().Configure(buff,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Bebelith.CombatTraits.Name",
                    "Bebelith Combat Traits"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Bebelith.CombatTraits.Description",
                    "Natural armor, DR 10/good, a bounded armor-dismantling claw sequence, and a +2 attack and damage bonus against chaotic evil outsiders."),
                null);
        }

        private static void ConfigureBebelith(LibraryScriptableObject library,
            BlueprintUnit unit, BlueprintItemWeapon claw,
            BlueprintBuff combatTraits, BlueprintFeature extraplanar)
        {
            BlueprintItemWeapon bite = BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, HugeBiteGuid,
                    "Bebelith 2d6 bite");
            unit.ComponentsArray = new BlueprintComponent[] {
                OutsiderLevels(library,
                    ExpandedSummoningSpecialProfiles.BebelithHitDice)
            };
            unit.Body = NaturalBody(claw, new[] { claw }, new[] { bite });
            unit.Brain = BlueprintLibraryLookup.RequireExact<BlueprintBrain>(
                library, DumbBrainGuid, "bounded natural-attack brain");
            ConfigureUnitCore(unit, "Bebelith", "Bebelith",
                Alignment.ChaoticEvil, Size.Huge,
                ExpandedSummoningSpecialProfiles.BebelithStrength,
                ExpandedSummoningSpecialProfiles.BebelithDexterity,
                ExpandedSummoningSpecialProfiles.BebelithConstitution,
                ExpandedSummoningSpecialProfiles.BebelithIntelligence,
                ExpandedSummoningSpecialProfiles.BebelithWisdom,
                ExpandedSummoningSpecialProfiles.BebelithCharisma,
                ExpandedSummoningSpecialProfiles.BebelithSpeedFeet);
            unit.AddFacts = new BlueprintUnitFact[] {
                Feature(library, OutsiderTypeGuid, "outsider creature type"),
                Feature(library, ChaoticSubtypeGuid, "chaotic subtype"),
                Feature(library, EvilSubtypeGuid, "evil subtype"),
                extraplanar,
                Feature(library, ImprovedInitiativeGuid, "Improved Initiative"),
                Feature(library, LightningReflexesGuid, "Lightning Reflexes"),
                combatTraits
            };
        }

        private static void ConfigurePixieSleepBow(
            LibraryScriptableObject library, BlueprintItemWeapon bow,
            BlueprintWeaponType bowType)
        {
            BlueprintItemWeapon native = BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, StandardLongbowGuid,
                    "standard longbow arrow rig");
            CopyFields(native, bow);
            bow.name = InternalName(PixieSleepBowSymbol);
            bow.ComponentsArray = (native.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Select(
                    ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            SetField(bow, "m_Type", bowType);
            SetField(bow, "m_OverrideDamageDice", true);
            SetField(bow, "m_DamageDice", new DiceFormula(0, DiceType.Zero));
            SetField(bow, "m_Enchantments", Array.Empty<
                Kingmaker.Blueprints.Items.Ecnchantments.BlueprintWeaponEnchantment>());
            bow.IsNonRemovable = true;
            SetField(bow, "m_Cost", 0);
            SetField(bow, "m_Weight", 0f);
        }

        private static void ConfigureSummonWeaponType(
            LibraryScriptableObject library, string sourceWeaponGuid,
            string sourceRole, string symbol, BlueprintWeaponType result)
        {
            BlueprintItemWeapon source = BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, sourceWeaponGuid, sourceRole);
            CopyFields(source.Type, result);
            result.name = InternalName(symbol);
            result.ComponentsArray = (source.Type.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Select(
                    ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            SetField(result, "m_IsNatural", true);
            SetField(result, "m_Weight", 0f);
            SetField(result, "m_Enchantments", Array.Empty<
                Kingmaker.Blueprints.Items.Ecnchantments.BlueprintWeaponEnchantment>());
        }

        private static void ConfigureResource(BlueprintAbilityResource resource,
            string displayName, int maximum)
        {
            ConfigureNamedResource(resource, displayName == "Sleep Arrows" ?
                PixieSleepResourceSymbol : PixieDanceResourceSymbol,
                "KMG.ExpandedSummoning.Pixie." +
                    displayName.Replace(" ", string.Empty) + ".Resource",
                displayName, "Uses remaining for this summoned Pixie.", maximum);
        }

        private static void ConfigureNamedResource(
            BlueprintAbilityResource resource, string symbol,
            string localizationPrefix, string displayName, string description,
            int maximum)
        {
            resource.name = InternalName(symbol);
            resource.LocalizedName = LocalizationService.Create(
                localizationPrefix + ".Name", displayName);
            resource.LocalizedDescription = LocalizationService.Create(
                localizationPrefix + ".Description", description);
            FieldInfo amountField = Fields(typeof(BlueprintAbilityResource))
                .SingleOrDefault(value => value.Name == "m_MaxAmount");
            if (amountField == null || !amountField.FieldType.IsValueType)
                throw new MissingFieldException(
                    typeof(BlueprintAbilityResource).FullName, "m_MaxAmount");
            object amount = Activator.CreateInstance(amountField.FieldType);
            FieldInfo baseField = amountField.FieldType.GetField("BaseValue");
            if (baseField == null || baseField.FieldType != typeof(int))
                throw new MissingFieldException(amountField.FieldType.FullName,
                    "BaseValue");
            baseField.SetValue(amount, maximum);
            foreach (string arrayName in new[] { "Class", "Archetypes",
                "ClassDiv", "ArchetypesDiv" })
            {
                FieldInfo arrayField = amountField.FieldType.GetField(arrayName);
                if (arrayField == null || !arrayField.FieldType.IsArray)
                    throw new MissingFieldException(amountField.FieldType.FullName,
                        arrayName);
                arrayField.SetValue(amount, Array.CreateInstance(
                    arrayField.FieldType.GetElementType(), 0));
            }
            amountField.SetValue(resource, amount);
        }

        private static void ConfigurePixieDanceState(BlueprintBuff buff)
        {
            var cannotAct = ScriptableObject.CreateInstance<AddCondition>();
            cannotAct.Condition = UnitCondition.CantAct;
            var armorClass = ScriptableObject.CreateInstance<AddStatBonus>();
            armorClass.Stat = StatType.AC;
            armorClass.Descriptor = ModifierDescriptor.UntypedStackable;
            armorClass.Value = -4;
            var reflex = ScriptableObject.CreateInstance<AddStatBonus>();
            reflex.Stat = StatType.SaveReflex;
            reflex.Descriptor = ModifierDescriptor.UntypedStackable;
            reflex.Value = -10;
            buff.name = InternalName(PixieDanceStateSymbol);
            buff.Stacking = StackingType.Replace;
            buff.IsClassFeature = false;
            buff.ComponentsArray = new BlueprintComponent[] {
                cannotAct, armorClass, reflex
            };
            BlueprintUnitFactAccess.Resolve().Configure(buff,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Pixie.IrresistibleDance.State.Name",
                    "Irresistible Dance"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Pixie.IrresistibleDance.State.Description",
                    "This creature can do nothing but dance and takes a -4 penalty to AC and a -10 penalty on Reflex saves."),
                null);
        }

        private static void ConfigurePixieDance(BlueprintAbility ability,
            BlueprintBuff danceState, BlueprintAbilityResource resource,
            Sprite icon)
        {
            ability.name = InternalName(PixieDanceSymbol);
            ability.Type = AbilityType.SpellLike;
            ability.Parent = null;
            ability.Hidden = false;
            ability.ActionBarAutoFillIgnored = false;
            ability.Range = AbilityRange.Touch;
            ability.CanTargetEnemies = true;
            ability.CanTargetSelf = false;
            ability.CanTargetFriends = false;
            ability.CanTargetPoint = false;
            ability.SpellResistance = true;
            ability.NeedEquipWeapons = false;
            ability.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            ability.EffectOnAlly = AbilityEffectOnUnit.None;
            ability.ActionType = UnitCommand.CommandType.Standard;
            ability.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Touch;
            ability.MaterialComponent = new BlueprintAbility.MaterialComponentData();
            ability.ResourceAssetIds = Array.Empty<string>();
            var spell = ScriptableObject.CreateInstance<SpellComponent>();
            spell.School = SpellSchool.Enchantment;
            var descriptor = ScriptableObject.CreateInstance<
                SpellDescriptorComponent>();
            descriptor.Descriptor = SpellDescriptor.MindAffecting |
                SpellDescriptor.Compulsion;
            var parameters = ScriptableObject.CreateInstance<
                ContextCalculateAbilityParams>();
            parameters.StatType = StatType.Charisma;
            parameters.ReplaceCasterLevel = true;
            parameters.CasterLevel = Simple(ExpandedSummoningSpecialProfiles
                .PixieDanceCasterLevel);
            parameters.ReplaceSpellLevel = true;
            parameters.SpellLevel = Simple(ExpandedSummoningSpecialProfiles
                .PixieDanceSpellLevel);
            ContextActionApplyBuff failedApply = ScriptableObject.CreateInstance<
                ContextActionApplyBuff>();
            failedApply.Buff = danceState;
            failedApply.DurationValue = new ContextDurationValue {
                    Rate = DurationRate.Rounds,
                    DiceType = DiceType.D4,
                    DiceCountValue = Simple(1),
                    BonusValue = Simple(1)
                };
            failedApply.IsFromSpell = false;
            failedApply.IsNotDispelable = false;
            ContextActionApplyBuff succeededApply = ScriptableObject.CreateInstance<
                ContextActionApplyBuff>();
            succeededApply.Buff = danceState;
            succeededApply.DurationValue = new ContextDurationValue {
                    Rate = DurationRate.Rounds,
                    DiceType = DiceType.Zero,
                    DiceCountValue = Simple(0),
                    BonusValue = Simple(1)
                };
            succeededApply.IsFromSpell = false;
            succeededApply.IsNotDispelable = false;
            ContextActionConditionalSaved saved = ScriptableObject.CreateInstance<
                ContextActionConditionalSaved>();
            saved.Failed = new ActionList { Actions = new GameAction[] {
                failedApply } };
            saved.Succeed = new ActionList { Actions = new GameAction[] {
                succeededApply } };
            var effect = ScriptableObject.CreateInstance<AbilityEffectRunAction>();
            effect.SavingThrowType = SavingThrowType.Will;
            effect.Actions = new ActionList { Actions = new GameAction[] { saved } };
            var cost = ScriptableObject.CreateInstance<AbilityResourceLogic>();
            cost.RequiredResource = resource;
            cost.IsSpendResource = true;
            cost.CostIsCustom = false;
            cost.Amount = 1;
            ability.ComponentsArray = new BlueprintComponent[] {
                spell, descriptor, parameters, cost, effect
            };
            BlueprintUnitFactAccess.Resolve().Configure(ability,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Pixie.IrresistibleDance.Name",
                    "Irresistible Dance"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Pixie.IrresistibleDance.Description",
                    "Once per summon, a touched creature dances for 1d4+1 rounds on a failed Will save or one round on a successful save."),
                icon);
        }

        private static AddAbilityResources AddResource(
            BlueprintAbilityResource resource)
        {
            var add = ScriptableObject.CreateInstance<AddAbilityResources>();
            add.UseThisAsResource = false;
            add.Resource = resource;
            add.Amount = 0;
            add.RestoreAmount = true;
            add.RestoreOnLevelUp = false;
            return add;
        }

        private static void ConfigurePixieCombatTraits(
            LibraryScriptableObject library, BlueprintBuff buff,
            BlueprintItemWeapon sleepBow,
            BlueprintAbilityResource sleepResource,
            BlueprintAbilityResource danceResource)
        {
            var naturalArmor = ScriptableObject.CreateInstance<AddStatBonus>();
            naturalArmor.Stat = StatType.AC;
            naturalArmor.Descriptor = ModifierDescriptor.NaturalArmor;
            naturalArmor.Value = 1;
            var dr = ScriptableObject.CreateInstance<AddDamageResistancePhysical>();
            dr.Value = Simple(ExpandedSummoningSpecialProfiles
                .PixieDamageReduction);
            dr.BypassedByMaterial = true;
            dr.Material = PhysicalDamageMaterial.ColdIron;
            var sr = ScriptableObject.CreateInstance<AddSpellResistance>();
            sr.Value = Simple(ExpandedSummoningSpecialProfiles
                .PixieSpellResistance);
            sr.AddCR = false;
            var arrows = ScriptableObject.CreateInstance<
                PixieSleepArrowComponent>();
            arrows.SleepBow = sleepBow;
            arrows.SleepArrowResource = sleepResource;
            arrows.SleepingBuff = BlueprintLibraryLookup.RequireExact<
                BlueprintBuff>(library, NativeSleepingBuffGuid,
                    "native bounded Sleeping state");
            buff.Stacking = StackingType.Replace;
            buff.IsClassFeature = true;
            buff.ComponentsArray = new BlueprintComponent[] {
                naturalArmor, dr, sr, AddResource(sleepResource),
                AddResource(danceResource), arrows
            };
            BlueprintUnitFactAccess.Resolve().Configure(buff,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Pixie.CombatTraits.Name",
                    "Pixie Combat Traits"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Pixie.CombatTraits.Description",
                    "DR 10/cold iron, spell resistance 15, sixteen no-damage sleep arrows (Will DC 15), and one use of irresistible dance."),
                null);
        }

        private static void ConfigurePixieAi(BlueprintAiCastSpell ai,
            BlueprintAbility dance, BlueprintBrain brain)
        {
            ai.name = InternalName(PixieDanceAiSymbol);
            ai.Ability = dance;
            ai.Variant = null;
            ai.BaseScore = 3;
            ai.CooldownRounds = 0;
            ai.StartCooldownRounds = 0;
            ai.ActorConsiderations = Array.Empty<Kingmaker.Controllers.Brain
                .Blueprints.Considerations.Consideration>();
            ai.TargetConsiderations = Array.Empty<Kingmaker.Controllers.Brain
                .Blueprints.Considerations.Consideration>();
            ai.Locators = Array.Empty<EntityReference>();
            brain.name = InternalName(PixieBrainSymbol);
            brain.Actions = new BlueprintAiAction[] { ai };
        }

        private static void ConfigurePixie(LibraryScriptableObject library,
            BlueprintUnit unit, BlueprintItemWeapon sleepBow,
            BlueprintAbility dance, BlueprintBrain brain,
            BlueprintBuff combatTraits)
        {
            var grant = ScriptableObject.CreateInstance<
                AddAbilityToCharacterComponent>();
            grant.Abilities = new[] { dance };
            unit.ComponentsArray = new BlueprintComponent[] {
                FeyLevels(library,
                    ExpandedSummoningSpecialProfiles.PixieHitDice), grant
            };
            unit.Body = NaturalBody(sleepBow,
                Array.Empty<BlueprintItemWeapon>(),
                Array.Empty<BlueprintItemWeapon>());
            unit.Brain = brain;
            ConfigureUnitCore(unit, "Pixie", "Pixie", Alignment.NeutralGood,
                Size.Small,
                ExpandedSummoningSpecialProfiles.PixieStrength,
                ExpandedSummoningSpecialProfiles.PixieDexterity,
                ExpandedSummoningSpecialProfiles.PixieConstitution,
                ExpandedSummoningSpecialProfiles.PixieIntelligence,
                ExpandedSummoningSpecialProfiles.PixieWisdom,
                ExpandedSummoningSpecialProfiles.PixieCharisma,
                ExpandedSummoningSpecialProfiles.PixieSpeedFeet);
            unit.AddFacts = new BlueprintUnitFact[] {
                Feature(library, FeyTypeGuid, "fey creature type"),
                Feature(library, AirborneGuid, "airborne movement"),
                BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library,
                    NaturalInvisibilityGuid,
                    "attack-safe natural invisibility"),
                Feature(library, DodgeGuid, "Dodge"),
                Feature(library, WeaponFinesseGuid, "Weapon Finesse"),
                combatTraits
            };
        }

        private static void ConfigureRay(LibraryScriptableObject library,
            BlueprintAbility ray)
        {
            BlueprintAbility native = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                library, NativeRayGuid, "native Ghaele two-projectile light ray");
            ExpandedSummoningAbilityBuilder.CopyFields(native, ray);
            ray.name = InternalName(LanternRaySymbol);
            ray.ComponentsArray = (native.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Select(
                    ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            ray.Range = AbilityRange.Custom;
            ray.CustomRange = new Feet(
                ExpandedSummoningSpecialProfiles.LanternRayRangeFeet);
            ray.CanTargetSelf = false;
            ray.CanTargetFriends = false;
            ray.SpellResistance = false;
            // The Will-o'-Wisp visual rig has no cast or attack clip. Use the
            // native immediate ability path so the two ray projectiles can use
            // CenterTorso as their bounded origin without waiting on a missing
            // Ghaele animation event.
            ray.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions
                .UnitAnimationActionCastSpell.CastAnimationStyle.Immediate;
            AbilityDeliverProjectile delivery = ray.ComponentsArray
                .OfType<AbilityDeliverProjectile>().Single();
            delivery.Length = new Feet(
                ExpandedSummoningSpecialProfiles.LanternRayRangeFeet);
            if ((delivery.Projectiles ?? Array.Empty<BlueprintProjectile>()).Length !=
                ExpandedSummoningSpecialProfiles.LanternRayProjectiles)
                throw new InvalidOperationException(
                    "Native Lantern ray donor no longer has two projectiles.");
            ContextActionDealDamage damage = FindDamage(ray.ComponentsArray);
            damage.Value = new ContextDiceValue {
                DiceType = DiceType.D6,
                DiceCountValue = Simple(
                    ExpandedSummoningSpecialProfiles.LanternRayDiceCount),
                BonusValue = Simple(0)
            };
            damage.DamageType.Type = DamageType.Direct;
            BlueprintUnitFactAccess.Resolve().Configure(ray,
                LocalizationService.Create("KMG.ExpandedSummoning.LanternArchon.LightRay.Name",
                    "Light Ray"),
                LocalizationService.Create("KMG.ExpandedSummoning.LanternArchon.LightRay.Description",
                    "Fires two ranged-touch rays to 30 feet. Each ray deals 1d6 direct light damage."),
                native.Icon);
        }

        private static void ConfigureAi(LibraryScriptableObject library,
            BlueprintAiCastSpell ai, BlueprintAbility ray)
        {
            BlueprintAiCastSpell native = BlueprintLibraryLookup.RequireExact<
                BlueprintAiCastSpell>(library, NativeRayAiGuid,
                    "native Ghaele light-ray AI action");
            CopyFields(native, ai);
            ai.name = InternalName(LanternAiSymbol);
            ai.Ability = ray;
            ai.Variant = null;
            ai.CooldownRounds = 0;
            ai.StartCooldownRounds = 0;
        }

        private static void ConfigureDefenses(BlueprintBuff defenses)
        {
            var dr = ScriptableObject.CreateInstance<AddDamageResistancePhysical>();
            dr.Value = Simple(ExpandedSummoningSpecialProfiles.LanternDamageReduction);
            dr.BypassedByAlignment = true;
            dr.Alignment = DamageAlignment.Evil;
            var poison = ScriptableObject.CreateInstance<
                SavingThrowBonusAgainstDescriptor>();
            poison.SpellDescriptor = SpellDescriptor.Poison;
            poison.ModifierDescriptor = ModifierDescriptor.Racial;
            poison.Value = ExpandedSummoningSpecialProfiles.LanternPoisonSaveBonus;
            poison.Bonus = Simple(0);
            var evilSave = ScriptableObject.CreateInstance<
                SavingThrowBonusAgainstAlignment>();
            evilSave.Alignment = AlignmentComponent.Evil;
            evilSave.Descriptor = ModifierDescriptor.Resistance;
            evilSave.Value = ExpandedSummoningSpecialProfiles
                .LanternEvilSaveAndAcBonus;
            var evilAc = ScriptableObject.CreateInstance<
                ArmorClassBonusAgainstAlignment>();
            evilAc.alignment = AlignmentComponent.Evil;
            evilAc.Descriptor = ModifierDescriptor.Deflection;
            evilAc.Value = ExpandedSummoningSpecialProfiles
                .LanternEvilSaveAndAcBonus;
            defenses.Stacking = StackingType.Replace;
            defenses.IsClassFeature = true;
            defenses.ComponentsArray = new BlueprintComponent[] {
                dr, poison, evilSave, evilAc
            };
            BlueprintUnitFactAccess.Resolve().Configure(defenses,
                LocalizationService.Create("KMG.ExpandedSummoning.LanternArchon.Defenses.Name",
                    "Archon Defenses"),
                LocalizationService.Create("KMG.ExpandedSummoning.LanternArchon.Defenses.Description",
                    "Damage reduction 10/evil, archon poison resistance, and defenses against evil creatures."),
                null);
        }

        private static void ConfigureUnit(LibraryScriptableObject library,
            BlueprintUnit unit, BlueprintAbility ray, BlueprintBrain brain,
            BlueprintBuff defenses, BlueprintFeature extraplanar)
        {
            var levels = ScriptableObject.CreateInstance<AddClassLevels>();
            levels.CharacterClass = BlueprintLibraryLookup.RequireExact<
                BlueprintCharacterClass>(library, OutsiderClassGuid,
                    "native outsider class");
            levels.Levels = ExpandedSummoningSpecialProfiles.LanternHitDice;
            levels.RaceStat = StatType.Constitution;
            levels.LevelsStat = StatType.Unknown;
            levels.Skills = new[] { StatType.SkillPerception,
                StatType.SkillMobility, StatType.SkillPersuasion };
            levels.Archetypes = Array.Empty<BlueprintArchetype>();
            levels.SelectSpells = Array.Empty<BlueprintAbility>();
            levels.MemorizeSpells = Array.Empty<BlueprintAbility>();
            levels.Selections = Array.Empty<SelectionEntry>();
            var grant = ScriptableObject.CreateInstance<
                AddAbilityToCharacterComponent>();
            grant.Abilities = new[] { ray };
            unit.ComponentsArray = new BlueprintComponent[] { levels, grant };
            unit.Body = new BlueprintUnit.UnitBody {
                DisableHands = true,
                AdditionalLimbs = Array.Empty<Kingmaker.Blueprints.Items.Weapons.BlueprintItemWeapon>(),
                AdditionalSecondaryLimbs = Array.Empty<Kingmaker.Blueprints.Items.Weapons.BlueprintItemWeapon>(),
                QuickSlots = Array.Empty<Kingmaker.Blueprints.Items.Equipment.BlueprintItemEquipmentUsable>()
            };
            unit.Brain = brain;
            var unitName = ScriptableObject.CreateInstance<SharedStringAsset>();
            unitName.String = LocalizationService.Create(
                "KMG.ExpandedSummoning.LanternArchon.Unit.Name", "Lantern Archon");
            unit.LocalizedName = unitName;
            unit.Alignment = Alignment.LawfulGood;
            unit.Size = Size.Small;
            unit.Strength = ExpandedSummoningSpecialProfiles.LanternStrength;
            unit.Dexterity = ExpandedSummoningSpecialProfiles.LanternDexterity;
            unit.Constitution = ExpandedSummoningSpecialProfiles.LanternConstitution;
            unit.Intelligence = ExpandedSummoningSpecialProfiles.LanternIntelligence;
            unit.Wisdom = ExpandedSummoningSpecialProfiles.LanternWisdom;
            unit.Charisma = ExpandedSummoningSpecialProfiles.LanternCharisma;
            unit.Speed = new Feet(ExpandedSummoningSpecialProfiles.LanternSpeedFeet);
            unit.BaseAttackBonus = 0;
            unit.MaxHP = 0;
            unit.StartingInventory = Array.Empty<BlueprintItem>();
            var facts = new List<BlueprintUnitFact> {
                Feature(library, ImprovedInitiativeGuid, "Improved Initiative"),
                BlueprintLibraryLookup.RequireExact<BlueprintUnitFact>(library,
                    NaturalArmor4Guid, "natural armor +4"),
                Feature(library, ElectricityImmunityGuid, "electricity immunity"),
                Feature(library, GoodSubtypeGuid, "good subtype"),
                Feature(library, LawfulSubtypeGuid, "lawful subtype"),
                extraplanar,
                Feature(library, AirborneGuid, "airborne movement"),
                defenses
            };
            BlueprintBuff optionalAura = OptionalExact<BlueprintBuff>(library,
                AuraOfMenaceBuffGuid, "optional native Aura of Menace");
            if (optionalAura != null) facts.Insert(facts.Count - 1, optionalAura);
            unit.AddFacts = facts.ToArray();
        }

        private static T OptionalExact<T>(LibraryScriptableObject library,
            string guid, string role) where T : BlueprintScriptableObject
        {
            BlueprintScriptableObject value;
            string id = BlueprintId.Parse(guid, "guid").Value;
            if (library.BlueprintsByAssetId == null ||
                !library.BlueprintsByAssetId.TryGetValue(id, out value) ||
                value == null) return null;
            if (value.GetType() != typeof(T) || string.IsNullOrWhiteSpace(value.name))
                throw new InvalidOperationException("Optional blueprint has an " +
                    "unexpected type or blank name: role='" + role +
                    "', guid='" + guid + "'.");
            return (T)value;
        }

        private static BlueprintFeature Feature(LibraryScriptableObject library,
            string guid, string purpose)
        { return BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library,
            guid, purpose); }

        private static ContextActionDealDamage FindDamage(
            IEnumerable<BlueprintComponent> components)
        {
            var found = new List<ContextActionDealDamage>();
            foreach (BlueprintComponent component in components)
                Find(component, found, new HashSet<object>());
            if (found.Count != 1) throw new InvalidOperationException(
                "Lantern light ray requires exactly one damage action.");
            return found[0];
        }

        private static void Find(object value, IList<ContextActionDealDamage> found,
            ISet<object> seen)
        {
            if (value == null || value is string || value.GetType().IsValueType ||
                value is BlueprintScriptableObject || !seen.Add(value)) return;
            ContextActionDealDamage damage = value as ContextActionDealDamage;
            if (damage != null) { found.Add(damage); return; }
            foreach (FieldInfo field in Fields(value.GetType()))
            {
                object child = field.GetValue(value);
                System.Collections.IEnumerable sequence = child as
                    System.Collections.IEnumerable;
                if (sequence != null && !(child is string))
                    foreach (object item in sequence) Find(item, found, seen);
                else Find(child, found, seen);
            }
        }

        private static void CopyFields(object source, object target)
        {
            foreach (FieldInfo field in Fields(source.GetType()))
            {
                if (field.Name == "m_AssetGuid" || field.IsInitOnly ||
                    field.DeclaringType == typeof(UnityEngine.Object)) continue;
                object value = field.GetValue(source);
                Array array = value as Array;
                field.SetValue(target, array == null ? value : array.Clone());
            }
        }

        private static void SetField(object target, string name, object value)
        {
            FieldInfo field = Fields(target.GetType()).SingleOrDefault(candidate =>
                candidate.Name == name);
            if (field == null) throw new MissingFieldException(
                target.GetType().FullName, name);
            field.SetValue(target, value);
        }

        private static IEnumerable<FieldInfo> Fields(Type type)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            for (Type current = type; current != null; current = current.BaseType)
                foreach (FieldInfo field in current.GetFields(flags)) yield return field;
        }

        private static ContextValue Simple(int value)
        { return new ContextValue { ValueType = ContextValueType.Simple, Value = value }; }
        private static T Require<T>(IDictionary<string, BlueprintScriptableObject> values,
            string symbol) where T : BlueprintScriptableObject
        { return (T)values[symbol]; }
        private static string InternalName(string symbol)
        { return symbol.Replace('.', '_').Replace('-', '_'); }
    }
}
