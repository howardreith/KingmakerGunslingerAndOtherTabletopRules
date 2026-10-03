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
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.ElementsSystem;
using Kingmaker.Localization;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static class ExpandedSummoningNaturalBuilder
    {
        private const string Bite1d4Symbol =
            "KMG.Summoning.Natural.Bite1d4";
        private const string DireBatBlindsenseSymbol =
            "KMG.Summoning.Natural.DireBat.Blindsense";
        private const string Bite1d3Symbol =
            "KMG.Summoning.Natural.Bite1d3";
        /// <summary>
        /// The Poison Frog's printed bite deals a flat point, not a die. The
        /// value is exactly expressible because <c>DiceType</c> has a
        /// <c>One</c> member and this builder mints its own natural weapons.
        /// </summary>
        private const string Bite1Symbol =
            "KMG.Summoning.Natural.Bite1";
        private const string WolverineRageSymbol =
            "KMG.Summoning.Natural.Wolverine.Rage";
        private const string WolverineRageOnsetSymbol =
            "KMG.Summoning.Natural.Wolverine.RageOnset";
        private const string WolverineRageStateSymbol =
            "KMG.Summoning.Natural.Wolverine.RageState";
        private const string Tail1d12Symbol =
            "KMG.Summoning.Natural.Tail1d12";
        private const string Tail3d6Symbol =
            "KMG.Summoning.Natural.Tail3d6";
        private const string Bite2d8Symbol =
            "KMG.Summoning.Natural.Bite2d8";
        // Sprint 16: no native blueprint carries a 3d6 bite or a 4d8 tail
        // slap, so the Dire Crocodile's printed routine needs both.
        private const string Bite3d6Symbol =
            "KMG.Summoning.Natural.Bite3d6";
        private const string Tail4d8Symbol =
            "KMG.Summoning.Natural.Tail4d8";
        private const string Claw1d8Symbol =
            "KMG.Summoning.Natural.Claw1d8";
        private const string Talon2d6Symbol =
            "KMG.Summoning.Natural.Talon2d6";
        private const string WaspSting1d8Symbol =
            "KMG.Summoning.Natural.WaspSting1d8";
        private const string StirgeTouchSymbol =
            "KMG.Summoning.Natural.StirgeTouch";
        private const string AntSting1d4Symbol =
            "KMG.Summoning.Natural.AntSting1d4";
        private const string GiantAntPoisonSymbol =
            "KMG.Summoning.Natural.GiantAnt.Poison";
        private const string GiantAntVenomSymbol =
            "KMG.Summoning.Natural.GiantAnt.Venom";
        private const string FireBeetleLuminescenceSymbol =
            "KMG.Summoning.Natural.FireBeetle.Luminescence";
        private const string GiantAntRacialSkillsSymbol =
            "KMG.Summoning.Natural.GiantAnt.RacialSkills";
        private const string FireBeetleUnitTypeSymbol =
            "KMG.Summoning.Natural.FireBeetle.UnitType";
        private const string GiantStagBeetleUnitTypeSymbol =
            "KMG.Summoning.Natural.GiantStagBeetle.UnitType";
        private const string GiantAntUnitTypeSymbol =
            "KMG.Summoning.Natural.GiantAnt.UnitType";
        private const string NativeShockingGraspDeliveryGuid =
            "17451c1327c571641a1345bd31155209";
        private const string WaspPoisonSymbol =
            "KMG.Summoning.Natural.GiantWasp.Poison";
        private const string WaspVenomSymbol =
            "KMG.Summoning.Natural.GiantWasp.Venom";
        private const string WaspUnitTypeSymbol =
            "KMG.Summoning.Natural.GiantWasp.UnitType";
        private const string DireRatDiseaseSymbol =
            "KMG.Summoning.Natural.DireRat.Disease";
        private const string GoblinDogTraitsSymbol =
            "KMG.Summoning.Natural.GoblinDog.Traits";
        private const string GoblinDogAllergicReactionSymbol =
            "KMG.Summoning.Natural.GoblinDog.AllergicReaction";
        private const string NativeFilthFeverGuid =
            "9545a5550d89feb47a84edaeb4e63d0b";
        /// <summary>
        /// The printed Goblin Dog allergic reaction exempts the goblinoid
        /// subtype. Kingmaker has no goblinoid subtype fact, so the exemption
        /// is the exact enumerated set of native goblinoid unit types the
        /// installed library actually carries. The guarded unit-type census in
        /// run `20261001T1658000459746Z-observe-expanded-summoning-native-donors`
        /// enumerated all 106 `BlueprintUnitType` values in the installed
        /// library: `Goblin` (69 units) is the only goblinoid one, and there is
        /// no Hobgoblin and no Bugbear type to exempt. Selection is by exact
        /// asset id, never by name, so this set is complete for this
        /// installation rather than merely convenient.
        /// </summary>
        private static readonly string[] NativeGoblinoidUnitTypeGuids = {
            "d524df24b2f38cf4590525b2e7c4f34e" // Goblin
        };
        private const string NativeBite1d6Guid =
            "a000716f88c969c499a535dadcf09286";
        private const string NativeBite1d8Guid =
            "c988aa874d11ff84d873508ddc9b928f";
        private const string NativeBite2d6Guid =
            "d2f99947db522e24293a7ec4eded453f";
        private const string NativeClaw1d3Guid =
            "800092a2b9a743b48ae8aeeb5d243dcc";
        private const string NativeClaw1d4Guid =
            "118fdd03e569a66459ab01a20af6811a";
        private const string NativeClaw1d6Guid =
            "c76f72a862d168d44838206524366e1c";
        private const string NativeClaw2d4Guid =
            "8afc47748d00b3e4a8aff2787d9ee350";
        private const string NativeGore1d8Guid =
            "73ed4e955295e62469fe471f1d49d9ef";
        private const string NativeGore2d6Guid =
            "d1f80b5c5c73cc84db7854774850b08c";
        private const string NativeTail1d8Guid =
            "ae822725634c6f0418b8c48bd29df255";
        private const string NativeBiteLarge2d6Guid =
            "48647a4517e6512419e937f7a617ea5c";
        private const string NativeMastodonGoreGuid =
            "de42c58801037b84c9d992634ddd7220";
        private const string NativeMastodonSlamGuid =
            "c2ce7bc3559b2024ea91ddf5bb321f0a";
        private const string AnimalClassGuid =
            "4cd1757a0eea7694ba5c933729a53920";
        private const string VerminClassGuid =
            "d1a15612d1a96334d94edf5f1d3b8d29";
        private const string MagicalBeastClassGuid =
            "b9e97f47cb86f2d45a0784a096ff8037";
        private const string HumanoidClassGuid =
            "6ab4526f94d2e3e439af0599a29b6675";
        private const string PlantClassGuid =
            "9393cc36ea29d084bab7433e3a28d40b";
        // Sprint 4 natural weapons: the native Shambling Mound slam, the
        // native large bite the Giant Flytrap carries four of, and the
        // dedicated Purple Worm bite and sting.
        private const string NativePlantSlam2d6Guid =
            "27eee74857c42db499b3a6b20cfa6211";
        private const string NativeBiteLarge1d8Guid =
            "ec35ef997ed5a984280e1a6d87ae80a8";
        private const string NativePurpleWormBiteGuid =
            "7e4b9b41a9358264d9e3c69c183ca0a2";
        private const string NativePurpleWormStingGuid =
            "287cd06241fdaf8408410b226f744093";
        private const string NativeSpiderPoisonFeatureGuid =
            "094714bb08f4e1943a8e9d2384ebe573";
        private const string NativeSpiderPoisonBuffGuid =
            "56ec8788092b6314e8f3c1c502e8433f";
        private const string NativeSmallHoof1d3Guid =
            "085547b82eded104ba7e1870dd0563bf";
        private const string NativeHoof1d4Guid =
            "b0e472a49ff2a294f93faa3ab757a4a5";
        private const string NativeStandardGreataxeGuid =
            "6efea466862f014469cec6c3f2b85cb7";
        private const string DumbBrainGuid =
            "5abc8884c6f15204c8604cb01a2efbab";
        private static readonly IDictionary<int, string> NaturalArmorGuids =
            new Dictionary<int, string> {
                { 1, "10c7c5e3c5806bc4ca676e22d6fbf17e" },
                { 2, "45a52ce762f637f4c80cc741c91f58b7" },
                { 3, "f6e106931f95fec4eb995f0d0629fb84" },
                { 4, "16fc201a83edcde4cbd64c291ebe0d07" },
                { 5, "7661741dbb9604842a642457456fd0e4" },
                { 6, "987ba44303e88054c9504cb3083ba0c9" },
                { 7, "e73864391ccf0894997928443a29d755" },
                { 8, "b9342e2a6dc5165489ba3412c50ca3d1" },
                { 9, "da6417809bdedfa468dd2fd0cc74be92" },
                { 10, "4179c5c08d606a6439a62bf178b738e1" },
                { 12, "0b2d92c6aac8093489dfdadf1e448280" },
                { 14, "209a2920891b580418b4e5e80466e134" },
                { 22, "eee672c8f6555b445a89dbbb91361d64" }
            };
        private static readonly IDictionary<string, string> FactGuids =
            new Dictionary<string, string>(StringComparer.Ordinal) {
                { "TripDefenseFourLegs", "13c87ac5985cc85498ef9d1ac8b78923" },
                { "TripDefenseEightLegs", "a60900c666b2b37478a2bf4bb005973d" },
                { "TripImmune", "c1b26f97b974aec469613f968439e7bb" },
                { "TrippingBite", "f957b4444b6fb404e84ae2a5765797bb" },
                { "SkillFocusPerception", "f74c6bdf5c5f5374fb9302ecdc1f7d64" },
                { "WeaponFinesse", "90e54424d682d104ab36436bd527af09" },
                { "Airborne", "70cffb448c132fa409e49156d013b175" },
                { "PoisonFrog", "1a3f2f384bbef804d8f52db1f9aa62d3" },
                { "CentipedePoison", "6fed981bf0ef27a499969f369f35b5e8" },
                { "GiantSpiderPoison", "094714bb08f4e1943a8e9d2384ebe573" },
                { "Toughness", "d09b20029e9abfe4480b356c92095623" },
                { "ReducedReach", "c33f2d68d93ceee488aa4004347dffca" },
                { "Ferocity", "955e356c813de1743a98ab3485d5bc69" },
                { "Pounce", "1a8149c09e0bdfc48a305ee6ac3729a8" },
                { "SkillFocusStealth", "3a8d34905eae4a74892aae37df3352b9" },
                { "GreatFortitude", "79042cb55f030614ea29956177977c52" },
                { "MonitorLizardPoison", "d88236a83413baa45ae9c8e5ddce5a6c" },
                { "ImprovedInitiative", "797f25d709f559546b29e7bcb181cc74" },
                { "Stealthy", "c7e1d5ef809325943af97f093e149c4f" },
                { "WeaponFocusBite", "b97edcf55321a814ea6b7807d246726c" },
                { "Dodge", "97e216dbb46ae3c4faef90cf6bbe6fd5" },
                { "WeaponFocusClaw", "153937f44fcd42a429a286a10babd82d" },
                { "ImprovedCriticalBite", "8388e2e50bb779840adc8129b3d63bf1" },
                { "ImprovedCriticalClaw", "76a335b7d69691c4e8376f9379338778" },
                { "PowerAttack", "9972f33f977fc724c838e59641b2fca5" },
                { "IronWill", "175d1577bb6c9a04baf88eec99c66334" },
                { "LightningReflexes", "15e7da6645a7f3d41bdad7c8c4b9de1e" },
                { "Cleave", "d809b6c4ff2aaff4fa70d712a70f7d7b" },
                // Sprint 4
                { "FireResistance10", "24700a71dd3dc844ea585345f6dd18f6" },
                { "ElectricityImmunity", "cd1e5ab641a833c49994aff99db98952" },
                { "WeaponFocusSlam", "8c046dfa8d1c64247af0e830a5909510" },
                { "AcidResistance20", "416386972c8de2e42953533c4946599a" },
                { "Blindsight", "236ec7f226d3d784884f066aa4be1570" },
                { "PurpleWormPoison", "728446b9d0bf47144a1b621169299c2a" },
                { "CriticalFocus", "8ac59959b1b23c347a0361dc97cc786d" },
                { "SpiderWebImmunity", "3051e7002c803fc47a11bcfa381b9fbd" }
            };
        private static readonly ISet<string> BaseUnitFactKeys =
            new HashSet<string>(new[] { "ReducedReach", "Ferocity" },
                StringComparer.Ordinal);

        internal static void Configure(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol,
            BlueprintFeature extraplanar)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (bySymbol == null) throw new ArgumentNullException("bySymbol");
            if (extraplanar == null) throw new ArgumentNullException("extraplanar");
            ExpandedSummoningNaturalProfiles.Validate();
            ConfigureDireBatBlindsense(Require<BlueprintFeature>(bySymbol,
                DireBatBlindsenseSymbol));
            BlueprintItemWeapon nativeBite = BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativeBite1d6Guid,
                    "native bite animation weapon");
            ConfigureWeapon(nativeBite, Require<BlueprintItemWeapon>(bySymbol,
                Bite1d4Symbol), Bite1d4Symbol, 1, DiceType.D4);
            ConfigureWeapon(nativeBite, Require<BlueprintItemWeapon>(bySymbol,
                Bite1d3Symbol), Bite1d3Symbol, 1, DiceType.D3);
            ConfigureWeapon(nativeBite, Require<BlueprintItemWeapon>(bySymbol,
                Bite1Symbol), Bite1Symbol, 1, DiceType.One);
            BlueprintAbility nativeTouchDelivery = BlueprintLibraryLookup
                .RequireExact<BlueprintAbility>(library,
                    NativeShockingGraspDeliveryGuid,
                    "native held-touch weapon delivery");
            AbilityDeliverTouch touchComponent = nativeTouchDelivery
                .GetComponent<AbilityDeliverTouch>();
            BlueprintItemWeapon nativeTouch = touchComponent == null ? null :
                touchComponent.TouchWeapon;
            if (nativeTouch == null || nativeTouch.AttackType != AttackType.Touch)
                throw new InvalidOperationException(
                    "Native held-touch donor has no melee touch weapon.");
            BlueprintItemWeapon stirgeTouch = Require<BlueprintItemWeapon>(
                bySymbol, StirgeTouchSymbol);
            ConfigureWeapon(nativeTouch, stirgeTouch, StirgeTouchSymbol, 0,
                DiceType.Zero);
            // This is the creature's proboscis, not lootable held equipment.
            stirgeTouch.IsNonRemovable = true;
            ConfigureWeapon(BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativeTail1d8Guid,
                    "native animated tail weapon"),
                Require<BlueprintItemWeapon>(bySymbol, Tail1d12Symbol),
                Tail1d12Symbol, 1, DiceType.D12);
            ConfigureWeapon(BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativeTail1d8Guid,
                    "native animated tail weapon"),
                Require<BlueprintItemWeapon>(bySymbol, Tail3d6Symbol),
                Tail3d6Symbol, 3, DiceType.D6);
            ConfigureWeapon(BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativeBiteLarge2d6Guid,
                    "native large bite weapon"),
                Require<BlueprintItemWeapon>(bySymbol, Bite2d8Symbol),
                Bite2d8Symbol, 2, DiceType.D8);
            // Sprint 16: the Dire Crocodile's printed 3d6 bite and 4d8 tail
            // slap, each on the native animation its shape already uses.
            ConfigureWeapon(BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativeBiteLarge2d6Guid,
                    "native large bite weapon"),
                Require<BlueprintItemWeapon>(bySymbol, Bite3d6Symbol),
                Bite3d6Symbol, 3, DiceType.D6);
            ConfigureWeapon(BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativeTail1d8Guid,
                    "native animated tail weapon"),
                Require<BlueprintItemWeapon>(bySymbol, Tail4d8Symbol),
                Tail4d8Symbol, 4, DiceType.D8);
            ConfigureWeapon(BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativeClaw2d4Guid,
                    "native large claw animation weapon"),
                Require<BlueprintItemWeapon>(bySymbol, Talon2d6Symbol),
                Talon2d6Symbol, 2, DiceType.D6);
            // Sprint 8: the tiger's 1d8 claw on the native large claw animation.
            ConfigureWeapon(BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativeClaw1d6Guid,
                    "native large claw animation weapon"),
                Require<BlueprintItemWeapon>(bySymbol, Claw1d8Symbol),
                Claw1d8Symbol, 1, DiceType.D8);
            ConfigureWeapon(BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativePurpleWormStingGuid,
                    "native sting animation weapon"),
                Require<BlueprintItemWeapon>(bySymbol, WaspSting1d8Symbol),
                WaspSting1d8Symbol, 1, DiceType.D8);
            ConfigureWaspPoison(library,
                Require<BlueprintFeature>(bySymbol, WaspPoisonSymbol),
                Require<BlueprintBuff>(bySymbol, WaspVenomSymbol),
                Require<BlueprintItemWeapon>(bySymbol, WaspSting1d8Symbol));
            // The soldier's sting is its own weapon, built from the same
            // native sting animation the wasp uses, so the poison below can
            // gate on this weapon's type and never fire on the bite that
            // grabs.
            ConfigureWeapon(BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativePurpleWormStingGuid,
                    "native sting animation weapon"),
                Require<BlueprintItemWeapon>(bySymbol, AntSting1d4Symbol),
                AntSting1d4Symbol, 1, DiceType.D4);
            ConfigureGiantAntPoison(library,
                Require<BlueprintFeature>(bySymbol, GiantAntPoisonSymbol),
                Require<BlueprintBuff>(bySymbol, GiantAntVenomSymbol),
                Require<BlueprintItemWeapon>(bySymbol, AntSting1d4Symbol));
            ConfigureFireBeetleLuminescence(Require<BlueprintFeature>(
                bySymbol, FireBeetleLuminescenceSymbol));
            ConfigureGiantAntRacialSkills(Require<BlueprintFeature>(
                bySymbol, GiantAntRacialSkillsSymbol));
            ConfigureFireBeetleUnitType(Require<BlueprintUnitType>(
                bySymbol, FireBeetleUnitTypeSymbol));
            ConfigureGiantAntUnitType(Require<BlueprintUnitType>(
                bySymbol, GiantAntUnitTypeSymbol));
            ConfigureGiantStagBeetleUnitType(Require<BlueprintUnitType>(
                bySymbol, GiantStagBeetleUnitTypeSymbol));
            ConfigureWaspUnitType(Require<BlueprintUnitType>(bySymbol,
                WaspUnitTypeSymbol));
            BlueprintBuff filthFever = BlueprintLibraryLookup.RequireExact<
                BlueprintBuff>(library, NativeFilthFeverGuid,
                    "native Filth Fever disease payload");
            ConfigureDireRatDisease(Require<BlueprintFeature>(bySymbol,
                    DireRatDiseaseSymbol), filthFever,
                Require<BlueprintItemWeapon>(bySymbol, Bite1d4Symbol));
            ConfigureWolverineRage(
                Require<BlueprintFeature>(bySymbol, WolverineRageSymbol),
                Require<BlueprintBuff>(bySymbol, WolverineRageOnsetSymbol),
                Require<BlueprintBuff>(bySymbol, WolverineRageStateSymbol));
            ConfigureGoblinDogTraits(Require<BlueprintFeature>(bySymbol,
                    GoblinDogTraitsSymbol),
                Require<BlueprintBuff>(bySymbol,
                    GoblinDogAllergicReactionSymbol), nativeBite,
                NativeGoblinoidUnitTypeGuids.Select(guid =>
                    BlueprintLibraryLookup.RequireExact<BlueprintUnitType>(
                        library, guid,
                        "exact native goblinoid unit type")).ToArray());
            foreach (NaturalSummonProfile profile in
                ExpandedSummoningNaturalProfiles.All)
                ConfigureUnit(library, Require<BlueprintUnit>(bySymbol,
                    ExpandedSummoningIdentityCatalog.UnitSymbol(
                        ExpandedSummoningCatalog.All.Single(creature =>
                            creature.Key == profile.Key))), profile, bySymbol,
                    extraplanar);
        }

        private static void ConfigureWeapon(BlueprintItemWeapon native,
            BlueprintItemWeapon target, string symbol, int rolls, DiceType dice)
        {
            CopyFields(native, target);
            target.name = InternalName(symbol);
            target.ComponentsArray = (native.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Select(
                    ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            SetField(target, "m_OverrideDamageDice", true);
            SetField(target, "m_DamageDice", new DiceFormula(rolls, dice));
            SetField(target, "m_Enchantments", Array.Empty<Kingmaker.Blueprints
                .Items.Ecnchantments.BlueprintWeaponEnchantment>());
        }

        private static void ConfigureWaspPoison(LibraryScriptableObject library,
            BlueprintFeature feature, BlueprintBuff venom,
            BlueprintItemWeapon sting)
        {
            BlueprintBuff nativeBuff = BlueprintLibraryLookup.RequireExact<
                BlueprintBuff>(library, NativeSpiderPoisonBuffGuid,
                    "native saved poison lifecycle");
            CopyFields(nativeBuff, venom);
            venom.name = InternalName(WaspVenomSymbol);
            venom.ComponentsArray = (nativeBuff.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Select(
                    ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            venom.Stacking = StackingType.Poison;
            BuffPoisonStatDamage damage = venom.ComponentsArray.OfType<
                BuffPoisonStatDamage>().Single();
            damage.Stat = StatType.Dexterity;
            damage.Value = new DiceFormula(1, DiceType.D2);
            damage.Ticks = GiantWaspPoisonPolicy.Exposures;
            damage.SuccesfullSaves = GiantWaspPoisonPolicy.SavesToCure;
            damage.SaveType = SavingThrowType.Fortitude;
            BlueprintUnitFactAccess.Resolve().Configure(venom,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.GiantWasp.Venom.Name",
                    "Giant Wasp Venom"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.GiantWasp.Venom.Description",
                    "Injury poison: Fortitude DC 18; 1d2 Dexterity damage each round for six total exposures; one successful save cures it."),
                nativeBuff.Icon);

            BlueprintFeature nativeFeature = BlueprintLibraryLookup.RequireExact<
                BlueprintFeature>(library, NativeSpiderPoisonFeatureGuid,
                    "native poison-on-hit feature");
            CopyFields(nativeFeature, feature);
            feature.name = InternalName(WaspPoisonSymbol);
            feature.HideInUI = true;
            feature.IsClassFeature = false;
            BlueprintComponent[] components = (nativeFeature.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Select(
                    ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            AddInitiatorAttackWithWeaponTrigger trigger = components.OfType<
                AddInitiatorAttackWithWeaponTrigger>().Single();
            trigger.WeaponType = sting.Type;
            trigger.OnlyHit = true;
            ContextActionSavingThrow save = trigger.Action.Actions.OfType<
                ContextActionSavingThrow>().Single();
            ContextActionConditionalSaved outcome = save.Actions.Actions.OfType<
                ContextActionConditionalSaved>().Single();
            ContextActionApplyBuff apply = outcome.Failed.Actions.OfType<
                ContextActionApplyBuff>().Single();
            apply.Buff = venom;
            // The whole native graph moves inside a wound gate. An injury
            // poison is delivered by a wound, and OnlyHit is a weaker test: an
            // attack reduced to zero damage has hit without wounding. Sprint 12
            // already gates its bite diseases this way; this is the same
            // correction applied to the poison that shares the carrier.
            trigger.Action.Actions = new GameAction[] {
                new ContextActionOnlyIfWeaponWounded {
                    Actions = new ActionList {
                        Actions = (new GameAction[] {
                            new ContextActionSetWaspPoisonDc() }).Concat(
                                trigger.Action.Actions).ToArray() } } };
            feature.ComponentsArray = components;
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.GiantWasp.Poison.Name",
                    "Giant Wasp Poison"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.GiantWasp.Poison.Description",
                    "A sting delivers Giant Wasp venom on a hit."),
                null);
        }

        /// <summary>
        /// The Giant Ant (Soldier)'s printed sting poison.
        ///
        /// <para>The native Giant Spider poison Kingmaker already ships is
        /// this graph in every field but the damaged ability, so the clone
        /// changes Strength for its stat, four exposures for its frequency and
        /// one save to cure, and leaves everything else as the game wrote
        /// it.</para>
        ///
        /// <para>The trigger's weapon type is the sting's own, which is what
        /// keeps the poison off the bite. The bite is the primary limb and
        /// carries the grab; the sting is an additional limb and carries this.
        /// The two gates are independent - one on limb position, one on weapon
        /// type - so neither attack can acquire the other's rider.</para>
        /// </summary>
        private static void ConfigureGiantAntPoison(
            LibraryScriptableObject library, BlueprintFeature feature,
            BlueprintBuff venom, BlueprintItemWeapon sting)
        {
            BlueprintBuff nativeBuff = BlueprintLibraryLookup.RequireExact<
                BlueprintBuff>(library, NativeSpiderPoisonBuffGuid,
                    "native saved poison lifecycle");
            CopyFields(nativeBuff, venom);
            venom.name = InternalName(GiantAntVenomSymbol);
            venom.ComponentsArray = (nativeBuff.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Select(
                    ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            venom.Stacking = StackingType.Poison;
            BuffPoisonStatDamage damage = venom.ComponentsArray.OfType<
                BuffPoisonStatDamage>().Single();
            damage.Stat = StatType.Strength;
            damage.Value = new DiceFormula(1, DiceType.D2);
            damage.Ticks = GiantAntPoisonPolicy.Exposures;
            damage.SuccesfullSaves = GiantAntPoisonPolicy.SavesToCure;
            damage.SaveType = SavingThrowType.Fortitude;
            BlueprintUnitFactAccess.Resolve().Configure(venom,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.GiantAnt.Venom.Name",
                    "Giant Ant Venom"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.GiantAnt.Venom.Description",
                    "Injury poison: Fortitude DC 14; 1d2 Strength damage each round for four total exposures; one successful save cures it."),
                nativeBuff.Icon);

            BlueprintFeature nativeFeature = BlueprintLibraryLookup.RequireExact<
                BlueprintFeature>(library, NativeSpiderPoisonFeatureGuid,
                    "native poison-on-hit feature");
            CopyFields(nativeFeature, feature);
            feature.name = InternalName(GiantAntPoisonSymbol);
            feature.HideInUI = true;
            feature.IsClassFeature = false;
            BlueprintComponent[] components = (nativeFeature.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Select(
                    ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            AddInitiatorAttackWithWeaponTrigger trigger = components.OfType<
                AddInitiatorAttackWithWeaponTrigger>().Single();
            trigger.WeaponType = sting.Type;
            trigger.OnlyHit = true;
            ContextActionSavingThrow save = trigger.Action.Actions.OfType<
                ContextActionSavingThrow>().Single();
            ContextActionConditionalSaved outcome = save.Actions.Actions.OfType<
                ContextActionConditionalSaved>().Single();
            ContextActionApplyBuff apply = outcome.Failed.Actions.OfType<
                ContextActionApplyBuff>().Single();
            apply.Buff = venom;
            // The same wound gate the wasp's poison now carries: the sting
            // must hit and must deal positive final damage. The bite is
            // excluded by weapon type a layer above this and so never reaches
            // it at all.
            trigger.Action.Actions = new GameAction[] {
                new ContextActionOnlyIfWeaponWounded {
                    Actions = new ActionList {
                        Actions = (new GameAction[] {
                            new ContextActionSetGiantAntPoisonDc() }).Concat(
                                trigger.Action.Actions).ToArray() } } };
            feature.ComponentsArray = components;
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.GiantAnt.Poison.Name",
                    "Giant Ant Poison"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.GiantAnt.Poison.Description",
                    "A sting delivers Giant Ant venom on a hit. The bite does not."),
                null);
        }

        /// <summary>
        /// The Fire Beetle's luminescence, which carries no components at all.
        ///
        /// <para>That is deliberate and it is the whole point. The primary
        /// source gives the beetle a pair of glands that light a ten-foot
        /// radius, and Sprint 13 established that Kingmaker has no
        /// mechanics-layer illumination model: nothing in the rules layer
        /// consults light level, so a radius of light cannot grant or deny
        /// anything to anyone. A component here would have to either do
        /// nothing or invent a rule the tabletop does not have.</para>
        ///
        /// <para>What the player gets instead is honest: a visible feature
        /// that says the beetle glows, and a view-local light on the creature's
        /// own view that matches its painted glands. The feature deliberately
        /// does not claim an illumination system exists, and neither may any
        /// record of it.</para>
        /// </summary>
        private static void ConfigureFireBeetleLuminescence(
            BlueprintFeature feature)
        {
            feature.name = InternalName(FireBeetleLuminescenceSymbol);
            feature.IsClassFeature = false;
            feature.HideInUI = false;
            feature.ComponentsArray = Array.Empty<BlueprintComponent>();
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.FireBeetle.Luminescence.Name",
                    "Luminescence"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.FireBeetle.Luminescence.Description",
                    "A pair of glands above the beetle's eyes gives off a steady red glow. It is light and nothing else: Kingmaker does not model illumination, so the glow neither reveals nor conceals anything."),
                null);
        }

        /// <summary>
        /// The Giant Ant's printed racial skill bonus.
        ///
        /// <para>The stat block gives Toughness as its only feat and a +4
        /// racial bonus to Perception and Survival. The profiles previously
        /// carried Skill Focus (Perception), which is neither: it is a
        /// different feat with a different value that happens to touch the same
        /// skill.</para>
        ///
        /// <para>Kingmaker has no Survival skill, and this project has
        /// consistently omitted that half rather than substituting another -
        /// the Grizzly Bear, Dire Wolf and Tiger entries all record it as
        /// omitted. Lore (Nature) is a knowledge stat for identifying
        /// creatures, which is a different thing from tracking and foraging, so
        /// it is not used as an analogue. The Perception half is exact and the
        /// Survival half is disclosed on both castes' profiles.</para>
        /// </summary>
        /// <summary>
        /// The skill a profile names, as the engine's own stat. Unknown names
        /// are refused rather than ignored: a typo that silently removed a
        /// creature's ranks would be invisible.
        /// </summary>
        private static StatType SkillStat(string skill)
        {
            if (skill == "Perception") return StatType.SkillPerception;
            if (skill == "Mobility") return StatType.SkillMobility;
            if (skill == "Stealth") return StatType.SkillStealth;
            if (skill == "LoreNature") return StatType.SkillLoreNature;
            if (skill == "KnowledgeWorld") return StatType.SkillKnowledgeWorld;
            if (skill == "Athletics") return StatType.SkillAthletics;
            throw new InvalidOperationException(
                "Unknown natural profile skill: " + skill + ".");
        }

        private static void ConfigureGiantAntRacialSkills(
            BlueprintFeature feature)
        {
            var perception = ScriptableObject.CreateInstance<AddStatBonus>();
            perception.Stat = StatType.SkillPerception;
            perception.Value = NaturalSummonProfile
                .GiantAntRacialPerceptionBonus;
            perception.Descriptor = ModifierDescriptor.Racial;
            feature.name = InternalName(GiantAntRacialSkillsSymbol);
            feature.IsClassFeature = false;
            feature.HideInUI = false;
            feature.ComponentsArray = new BlueprintComponent[] { perception };
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.GiantAnt.RacialSkills.Name",
                    "Giant Ant Senses"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.GiantAnt.RacialSkills.Description",
                    "A giant ant has a +4 racial bonus on Perception checks. Its printed +4 Survival bonus has no Kingmaker equivalent and is omitted rather than substituted."),
                null);
        }

        private static void ConfigureFireBeetleUnitType(BlueprintUnitType type)
        {
            type.name = InternalName(FireBeetleUnitTypeSymbol);
            type.KnowledgeStat = StatType.SkillLoreNature;
            type.Name = LocalizationService.Create(
                "KMG.ExpandedSummoning.FireBeetle.UnitType.Name",
                "Fire Beetle");
            type.Description = LocalizationService.Create(
                "KMG.ExpandedSummoning.FireBeetle.UnitType.Description",
                "A small nocturnal vermin whose glands give off a steady red glow.");
            type.Image = null;
            type.SignatureAbilities = Array.Empty<BlueprintUnitFact>();
        }

        /// <summary>
        /// One type for all three castes, because they are one creature in
        /// three castes rather than three creatures. Sprint 15's Drone is the
        /// soldier with the advanced simple template and wings and shares it.
        /// </summary>
        private static void ConfigureGiantAntUnitType(BlueprintUnitType type)
        {
            type.name = InternalName(GiantAntUnitTypeSymbol);
            type.KnowledgeStat = StatType.SkillLoreNature;
            type.Name = LocalizationService.Create(
                "KMG.ExpandedSummoning.GiantAnt.UnitType.Name",
                "Giant Ant");
            type.Description = LocalizationService.Create(
                "KMG.ExpandedSummoning.GiantAnt.UnitType.Description",
                "A dog-sized colonial vermin with heavy mandibles; soldiers also carry a venomous sting.");
            type.Image = null;
            type.SignatureAbilities = Array.Empty<BlueprintUnitFact>();
        }

        /// <summary>
        /// Its own type rather than the Fire Beetle's. They are different
        /// creatures that happen to share a rig, and a type carries a display
        /// name a player reads.
        /// </summary>
        private static void ConfigureGiantStagBeetleUnitType(
            BlueprintUnitType type)
        {
            type.name = InternalName(GiantStagBeetleUnitTypeSymbol);
            type.KnowledgeStat = StatType.SkillLoreNature;
            type.Name = LocalizationService.Create(
                "KMG.ExpandedSummoning.GiantStagBeetle.UnitType.Name",
                "Giant Stag Beetle");
            type.Description = LocalizationService.Create(
                "KMG.ExpandedSummoning.GiantStagBeetle.UnitType.Description",
                "A heavy Large beetle whose enormous mandibles can bowl over anything smaller than itself.");
            type.Image = null;
            type.SignatureAbilities = Array.Empty<BlueprintUnitFact>();
        }

        private static void ConfigureWaspUnitType(BlueprintUnitType type)
        {
            type.name = InternalName(WaspUnitTypeSymbol);
            type.KnowledgeStat = StatType.SkillLoreNature;
            type.Name = LocalizationService.Create(
                "KMG.ExpandedSummoning.GiantWasp.UnitType.Name",
                "Giant Wasp");
            type.Description = LocalizationService.Create(
                "KMG.ExpandedSummoning.GiantWasp.UnitType.Description",
                "A large flying vermin with a venomous sting.");
            type.Image = null;
            type.SignatureAbilities = Array.Empty<BlueprintUnitFact>();
        }

        private static void ConfigureDireRatDisease(BlueprintFeature feature,
            BlueprintBuff filthFever, BlueprintItemWeapon bite)
        {
            var delivery = ScriptableObject.CreateInstance<
                SummonInjuryDiseaseComponent>();
            delivery.BiteWeapon = bite;
            delivery.DiseaseBuff = filthFever;
            delivery.ExcludedUnitTypes = Array.Empty<BlueprintUnitType>();
            delivery.FortitudeDc = SummonInjuryDiseasePolicy.DireRatFortitudeDc;
            delivery.DurationSeconds = 0;
            feature.name = InternalName(DireRatDiseaseSymbol);
            feature.Ranks = 1;
            feature.IsClassFeature = false;
            feature.HideInUI = true;
            feature.ComponentsArray = new BlueprintComponent[] { delivery };
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.DireRat.Disease.Name",
                    "Dire Rat Filth Fever"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.DireRat.Disease.Description",
                    "A bite that hits and deals damage exposes the target to native Filth Fever after a DC 11 Fortitude save."),
                null);
        }

        private static void ConfigureGoblinDogTraits(BlueprintFeature feature,
            BlueprintBuff reaction, BlueprintItemWeapon bite,
            BlueprintUnitType[] goblinoidTypes)
        {
            var descriptor = ScriptableObject.CreateInstance<
                SpellDescriptorComponent>();
            descriptor.Descriptor = SpellDescriptor.Disease;
            var dexterity = ScriptableObject.CreateInstance<AddStatBonus>();
            dexterity.Stat = StatType.Dexterity;
            dexterity.Value =
                SummonInjuryDiseasePolicy.GoblinDogAllergyAbilityPenalty;
            dexterity.Descriptor = ModifierDescriptor.Penalty;
            var charisma = ScriptableObject.CreateInstance<AddStatBonus>();
            charisma.Stat = StatType.Charisma;
            charisma.Value =
                SummonInjuryDiseasePolicy.GoblinDogAllergyAbilityPenalty;
            charisma.Descriptor = ModifierDescriptor.Penalty;
            var healing = ScriptableObject.CreateInstance<
                SummonAllergicReactionHealingComponent>();
            reaction.name = InternalName(GoblinDogAllergicReactionSymbol);
            reaction.Stacking = StackingType.Replace;
            SetBuffFlags(reaction, harmful: true);
            reaction.ComponentsArray = new BlueprintComponent[] {
                descriptor, dexterity, charisma, healing };
            BlueprintUnitFactAccess.Resolve().Configure(reaction,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.GoblinDog.Allergy.Name",
                    "Goblin Dog Allergic Reaction"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.GoblinDog.Allergy.Description",
                    "Disease: -2 Dexterity and -2 Charisma for one day. Positive magical healing or remove disease ends the reaction."),
                null);

            var immunity = ScriptableObject.CreateInstance<
                BuffDescriptorImmunity>();
            immunity.CheckFact = false;
            immunity.Descriptor = SpellDescriptor.Disease;
            immunity.FactToCheck = null;
            immunity.IgnoreFeature = null;
            var delivery = ScriptableObject.CreateInstance<
                SummonInjuryDiseaseComponent>();
            delivery.BiteWeapon = bite;
            delivery.DiseaseBuff = reaction;
            delivery.ExcludedUnitTypes = goblinoidTypes;
            delivery.FortitudeDc =
                SummonInjuryDiseasePolicy.GoblinDogFortitudeDc;
            delivery.DurationSeconds =
                SummonInjuryDiseasePolicy.GoblinDogAllergyDurationSeconds;
            // The printed rule also exposes a creature that damages the Goblin
            // Dog with a natural weapon or unarmed attack, and one that
            // attempts to grapple it. Both deliver the same save and payload.
            var counter = ScriptableObject.CreateInstance<
                SummonContactAllergyCounterComponent>();
            counter.DiseaseBuff = reaction;
            counter.ExcludedUnitTypes = goblinoidTypes;
            counter.FortitudeDc =
                SummonInjuryDiseasePolicy.GoblinDogFortitudeDc;
            counter.DurationSeconds =
                SummonInjuryDiseasePolicy.GoblinDogAllergyDurationSeconds;
            var maneuver = ScriptableObject.CreateInstance<
                SummonContactAllergyManeuverComponent>();
            maneuver.DiseaseBuff = reaction;
            maneuver.ExcludedUnitTypes = goblinoidTypes;
            maneuver.FortitudeDc =
                SummonInjuryDiseasePolicy.GoblinDogFortitudeDc;
            maneuver.DurationSeconds =
                SummonInjuryDiseasePolicy.GoblinDogAllergyDurationSeconds;
            feature.name = InternalName(GoblinDogTraitsSymbol);
            feature.Ranks = 1;
            feature.IsClassFeature = false;
            feature.HideInUI = true;
            feature.ComponentsArray = new BlueprintComponent[] {
                immunity, delivery, counter, maneuver };
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.GoblinDog.Traits.Name",
                    "Goblin Dog Disease Traits"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.GoblinDog.Traits.Description",
                    "Immune to disease. Its dander causes an allergic reaction on a failed DC 12 Fortitude save: when its bite deals damage, when a natural weapon or unarmed attack deals damage to it, and when a creature attempts to grapple it. Creatures with a native goblinoid unit type are exempt."),
                null);
        }

        private static void ConfigureUnit(LibraryScriptableObject library,
            BlueprintUnit unit, NaturalSummonProfile profile,
            IDictionary<string, BlueprintScriptableObject> bySymbol,
            BlueprintFeature extraplanar)
        {
            var levels = UnityEngine.ScriptableObject.CreateInstance<
                AddClassLevels>();
            levels.CharacterClass = BlueprintLibraryLookup.RequireExact<
                BlueprintCharacterClass>(library,
                    HitDieClassGuid(profile.HitDieClass),
                    profile.HitDieClass + " racial hit dice");
            levels.Levels = profile.HitDice;
            levels.RaceStat = StatType.Constitution;
            levels.LevelsStat = StatType.Unknown;
            // Profile-controlled. The default is the three this builder has
            // always given every reconstructed creature; a profile that names
            // its own gets exactly those, and a vermin whose stat block prints
            // no skill ranks names none. Giving ranks nobody printed is how a
            // Giant Ant read Perception 7 against a printed +5.
            levels.Skills = profile.Skills.Select(SkillStat).ToArray();
            levels.Archetypes = Array.Empty<BlueprintArchetype>();
            levels.SelectSpells = Array.Empty<BlueprintAbility>();
            levels.MemorizeSpells = Array.Empty<BlueprintAbility>();
            levels.Selections = Array.Empty<SelectionEntry>();
            unit.ComponentsArray = new BlueprintComponent[] { levels };

            BlueprintItemWeapon primary = Weapon(library, bySymbol,
                profile.PrimaryWeapon);
            BlueprintItemWeapon[] additional = profile.AdditionalWeapons.Select(
                key => Weapon(library, bySymbol, key)).ToArray();
            BlueprintItemWeapon[] secondary = profile.AdditionalSecondaryWeapons
                .Select(key => Weapon(library, bySymbol, key)).ToArray();
            unit.Body = new BlueprintUnit.UnitBody {
                PrimaryHand = primary,
                AdditionalLimbs = additional,
                AdditionalSecondaryLimbs = secondary,
                QuickSlots = Array.Empty<Kingmaker.Blueprints.Items.Equipment
                    .BlueprintItemEquipmentUsable>()
            };
            unit.Brain = BlueprintLibraryLookup.RequireExact<
                Kingmaker.Controllers.Brain.Blueprints.BlueprintBrain>(library,
                    DumbBrainGuid, "bounded natural-attack brain");
            SharedStringAsset name = UnityEngine.ScriptableObject.CreateInstance<
                SharedStringAsset>();
            name.String = LocalizationService.Create(
                "KMG.ExpandedSummoning." + Token(profile.Key) + ".Unit.Name",
                profile.DisplayName);
            unit.LocalizedName = name;
            // Without this a creature keeps whatever unit type its donor
            // had. Only the Wasp used to ask, so all three Sprint 14 insects
            // would have been classified as the Giant Spider they borrow.
            if (profile.Key == "giant-wasp")
                unit.Type = Require<BlueprintUnitType>(bySymbol,
                    WaspUnitTypeSymbol);
            else if (profile.Key == "fire-beetle")
                unit.Type = Require<BlueprintUnitType>(bySymbol,
                    FireBeetleUnitTypeSymbol);
            else if (profile.Key == "giant-ant-worker" ||
                profile.Key == "giant-ant-soldier" ||
                profile.Key == "giant-ant-drone")
                unit.Type = Require<BlueprintUnitType>(bySymbol,
                    GiantAntUnitTypeSymbol);
            else if (profile.Key == "giant-stag-beetle")
                unit.Type = Require<BlueprintUnitType>(bySymbol,
                    GiantStagBeetleUnitTypeSymbol);
            unit.Alignment = Alignment.TrueNeutral;
            unit.Size = ParseSize(profile.Size);
            unit.Strength = profile.Strength;
            unit.Dexterity = profile.Dexterity;
            unit.Constitution = profile.Constitution;
            unit.Intelligence = profile.Intelligence;
            unit.Wisdom = profile.Wisdom;
            unit.Charisma = profile.Charisma;
            unit.Speed = new Feet(profile.SpeedFeet);
            unit.BaseAttackBonus = 0;
            unit.MaxHP = 0;
            unit.StartingInventory = Array.Empty<BlueprintItem>();
            var facts = new List<BlueprintUnitFact>();
            if (profile.NaturalArmor != 0)
            {
                string armorGuid;
                if (!NaturalArmorGuids.TryGetValue(profile.NaturalArmor,
                        out armorGuid))
                    throw new InvalidOperationException(
                        "Unsupported natural armor value " +
                        profile.NaturalArmor + ".");
                facts.Add(BlueprintLibraryLookup.RequireExact<BlueprintUnitFact>(
                    library, armorGuid, "natural armor +" +
                        profile.NaturalArmor));
            }
            foreach (string fact in profile.Facts)
            {
                BlueprintUnitFact value = fact == "WaspPoison"
                    ? Require<BlueprintFeature>(bySymbol, WaspPoisonSymbol)
                    : fact == "DireBatBlindsense"
                    ? Require<BlueprintFeature>(bySymbol, DireBatBlindsenseSymbol)
                    : fact == "DireRatDisease"
                    ? Require<BlueprintFeature>(bySymbol, DireRatDiseaseSymbol)
                    : fact == "GoblinDogTraits"
                    ? Require<BlueprintFeature>(bySymbol, GoblinDogTraitsSymbol)
                    : fact == "WolverineRage"
                    ? Require<BlueprintFeature>(bySymbol, WolverineRageSymbol)
                    : fact == "GiantAntPoison"
                    ? Require<BlueprintFeature>(bySymbol, GiantAntPoisonSymbol)
                    : fact == "FireBeetleLuminescence"
                    ? Require<BlueprintFeature>(bySymbol,
                        FireBeetleLuminescenceSymbol)
                    : fact == "GiantAntRacialSkills"
                    ? Require<BlueprintFeature>(bySymbol,
                        GiantAntRacialSkillsSymbol)
                    : BaseUnitFactKeys.Contains(fact)
                    ? BlueprintLibraryLookup.RequireExact<BlueprintUnitFact>(
                        library, FactGuids[fact], profile.DisplayName + " " + fact)
                    : BlueprintLibraryLookup.RequireExact<BlueprintFeature>(
                        library, FactGuids[fact], profile.DisplayName + " " + fact);
                facts.Add(value);
            }
            facts.Add(extraplanar);
            unit.AddFacts = facts.ToArray();
        }

        private static void ConfigureDireBatBlindsense(BlueprintFeature feature)
        {
            var nativeSense = UnityEngine.ScriptableObject.CreateInstance<
                Kingmaker.Designers.Mechanics.Facts.Blindsense>();
            nativeSense.Blindsight = false;
            nativeSense.Range = new Feet(40);
            feature.name = InternalName(DireBatBlindsenseSymbol);
            feature.IsClassFeature = false;
            feature.HideInUI = true;
            feature.ComponentsArray = new BlueprintComponent[] { nativeSense };
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.DireBat.Blindsense.Name",
                    "Blindsense (40 feet)"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.DireBat.Blindsense.Description",
                    "This dire bat can detect creatures within 40 feet by sound, without seeing them precisely."),
                null);
        }

        internal static string HitDieClassGuid(string hitDieClass)
        {
            switch (hitDieClass)
            {
                case "Animal": return AnimalClassGuid;
                case "Vermin": return VerminClassGuid;
                case "MagicalBeast": return MagicalBeastClassGuid;
                case "Humanoid": return HumanoidClassGuid;
                case "Plant": return PlantClassGuid;
            }
            throw new InvalidOperationException("Unsupported natural hit-die class " +
                hitDieClass + ".");
        }

        private static BlueprintItemWeapon Weapon(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol, string key)
        {
            if (key == "Hoof1d3") return BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativeSmallHoof1d3Guid, "1d3 hoof");
            if (key == "Hoof1d4") return BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativeHoof1d4Guid, "1d4 hoof");
            if (key == "Greataxe") return BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativeStandardGreataxeGuid,
                    "standard greataxe");
            if (key == "SlamPlant2d6") return BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativePlantSlam2d6Guid,
                    "shambling mound 2d6 slam");
            if (key == "BiteLarge1d8") return BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativeBiteLarge1d8Guid,
                    "large 1d8 bite");
            if (key == "PurpleWormBite") return BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativePurpleWormBiteGuid,
                    "purple worm bite");
            if (key == "PurpleWormSting") return BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativePurpleWormStingGuid,
                    "purple worm sting");
            if (key == "Bite1d4") return Require<BlueprintItemWeapon>(bySymbol,
                Bite1d4Symbol);
            if (key == "Bite1d3") return Require<BlueprintItemWeapon>(bySymbol,
                Bite1d3Symbol);
            if (key == "Bite1") return Require<BlueprintItemWeapon>(bySymbol,
                Bite1Symbol);
            if (key == "Bite1d6") return BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativeBite1d6Guid, "1d6 bite");
            if (key == "Bite1d8") return BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativeBite1d8Guid, "1d8 bite");
            if (key == "Bite2d6") return BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativeBite2d6Guid, "2d6 bite");
            if (key == "Claw1d3") return BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativeClaw1d3Guid, "1d3 claw");
            if (key == "Claw1d4") return BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativeClaw1d4Guid, "1d4 claw");
            if (key == "Claw1d6") return BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativeClaw1d6Guid, "1d6 claw");
            if (key == "Claw2d4") return BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativeClaw2d4Guid, "2d4 claw");
            if (key == "Gore1d8") return BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativeGore1d8Guid, "1d8 gore");
            if (key == "Gore2d6") return BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativeGore2d6Guid, "2d6 gore");
            if (key == "Tail1d12") return Require<BlueprintItemWeapon>(bySymbol,
                Tail1d12Symbol);
            if (key == "Tail3d6") return Require<BlueprintItemWeapon>(bySymbol,
                Tail3d6Symbol);
            if (key == "BiteLarge2d6") return BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativeBiteLarge2d6Guid,
                    "large 2d6 bite");
            if (key == "Bite2d8") return Require<BlueprintItemWeapon>(bySymbol,
                Bite2d8Symbol);
            if (key == "Bite3d6") return Require<BlueprintItemWeapon>(bySymbol,
                Bite3d6Symbol);
            if (key == "Tail4d8") return Require<BlueprintItemWeapon>(bySymbol,
                Tail4d8Symbol);
            if (key == "Talon2d6") return Require<BlueprintItemWeapon>(bySymbol,
                Talon2d6Symbol);
            if (key == "Claw1d8") return Require<BlueprintItemWeapon>(bySymbol,
                Claw1d8Symbol);
            if (key == "WaspSting1d8") return Require<BlueprintItemWeapon>(
                bySymbol, WaspSting1d8Symbol);
            if (key == "AntSting1d4") return Require<BlueprintItemWeapon>(
                bySymbol, AntSting1d4Symbol);
            if (key == "StirgeTouch") return Require<BlueprintItemWeapon>(
                bySymbol, StirgeTouchSymbol);
            if (key == "Gore2d8") return BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativeMastodonGoreGuid,
                    "mastodon 2d8 gore");
            if (key == "Slam2d6") return BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativeMastodonSlamGuid,
                    "mastodon 2d6 slam");
            throw new InvalidOperationException("Unknown natural weapon key " +
                key + ".");
        }

        private static Size ParseSize(string value)
        {
            Size parsed;
            if (!Enum.TryParse(value, out parsed))
                throw new InvalidOperationException("Unknown size " + value + ".");
            return parsed;
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

        private static void SetBuffFlags(BlueprintBuff buff, bool harmful,
            bool hidden = false)
        {
            FieldInfo field = Fields(typeof(BlueprintBuff)).SingleOrDefault(
                candidate => candidate.Name == "m_Flags");
            if (field == null || !field.FieldType.IsEnum)
                throw new MissingFieldException(typeof(BlueprintBuff).FullName,
                    "m_Flags");
            int value = (harmful ?
                (int)Enum.Parse(field.FieldType, "Harmful") : 0) |
                (hidden ? (int)Enum.Parse(field.FieldType, "HiddenInUi") : 0);
            field.SetValue(buff, Enum.ToObject(field.FieldType, value));
        }

        /// <summary>
        /// The printed Wolverine rage, in three blueprints.
        ///
        /// <para>Bestiary text: "A wolverine that takes damage in combat flies
        /// into a rage on its next turn, clawing and biting madly until either
        /// it or its opponent is dead. It gains +4 to Strength, +4 to
        /// Constitution, and -2 to AC. The creature cannot end its rage
        /// voluntarily."</para>
        ///
        /// <para>The delay is the part worth building carefully. The native
        /// <see cref="SetBuffOnsetDelay"/> runs its action list exactly once,
        /// on the first round boundary after the buff carrying it lands, so
        /// routing the rage through a hidden onset marker means there is no
        /// code path at all from the damage event to the +4/+4/-2: the rage
        /// cannot begin on the turn the wolverine was hurt, however the damage
        /// arrives.</para>
        ///
        /// <para>The rage state is permanent because the printed rage has no
        /// duration - it runs "until either it or its opponent is dead" - and
        /// carries no voluntary end. It is applied to the wolverine alone, so
        /// it leaves when the summon does, and it is marked undispellable
        /// because nothing in the printed text offers a way to call it off.</para>
        /// </summary>
        private static void ConfigureWolverineRage(BlueprintFeature feature,
            BlueprintBuff onset, BlueprintBuff state)
        {
            var strength = ScriptableObject.CreateInstance<AddStatBonus>();
            strength.Stat = StatType.Strength;
            strength.Value = SummonRagePolicy.WolverineRageAbilityBonus;
            strength.Descriptor = ModifierDescriptor.Morale;
            var constitution = ScriptableObject.CreateInstance<AddStatBonus>();
            constitution.Stat = StatType.Constitution;
            constitution.Value = SummonRagePolicy.WolverineRageAbilityBonus;
            constitution.Descriptor = ModifierDescriptor.Morale;
            // The printed rage is not a pure benefit. Leaving this term out
            // would make the creature better than its own stat block.
            var armourClass = ScriptableObject.CreateInstance<AddStatBonus>();
            armourClass.Stat = StatType.AC;
            armourClass.Value = SummonRagePolicy.WolverineRageArmorClassPenalty;
            armourClass.Descriptor = ModifierDescriptor.Penalty;
            state.name = InternalName(WolverineRageStateSymbol);
            state.Stacking = StackingType.Replace;
            SetBuffFlags(state, harmful: false);
            state.ComponentsArray = new BlueprintComponent[] {
                strength, constitution, armourClass };
            BlueprintUnitFactAccess.Resolve().Configure(state,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Wolverine.Rage.Name",
                    "Rage"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Wolverine.Rage.Description",
                    "+4 Strength, +4 Constitution and -2 AC. The wolverine cannot end its rage voluntarily."),
                null);

            // The native SetBuffOnsetDelay ran its list on schedule - the
            // marker cleared - but the rage state never arrived, so the onset
            // carries this project's own round-boundary component instead and
            // applies the rage through RuleApplyBuff, the path the damage
            // trigger and the Sprint 12 disease rider both already prove. The
            // delay is still the engine's own round boundary.
            var onsetTick = ScriptableObject
                .CreateInstance<SummonRageOnsetComponent>();
            onsetTick.RageBuff = state;
            onset.name = InternalName(WolverineRageOnsetSymbol);
            onset.Stacking = StackingType.Replace;
            onset.Frequency = DurationRate.Rounds;
            SetBuffFlags(onset, harmful: false, hidden: true);
            onset.ComponentsArray = new BlueprintComponent[] { onsetTick };
            BlueprintUnitFactAccess.Resolve().Configure(onset,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Wolverine.RageOnset.Name",
                    "Rising Rage"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Wolverine.RageOnset.Description",
                    "The wolverine has been hurt and will fly into a rage on its next turn."),
                null);

            var trigger = ScriptableObject
                .CreateInstance<SummonRageOnDamageComponent>();
            trigger.OnsetBuff = onset;
            trigger.RageBuff = state;
            feature.name = InternalName(WolverineRageSymbol);
            feature.Ranks = 1;
            feature.IsClassFeature = false;
            feature.HideInUI = true;
            feature.ComponentsArray = new BlueprintComponent[] { trigger };
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Wolverine.RageTrigger.Name",
                    "Rage"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Wolverine.RageTrigger.Description",
                    "A wolverine that takes damage in combat flies into a rage on its next turn, gaining +4 Strength, +4 Constitution and -2 AC. It cannot end its rage voluntarily."),
                null);
        }

        private static ContextValue ContextValueZero()
        {
            return new ContextValue();
        }

        private static ContextValue ContextValueOf(int value)
        {
            return new ContextValue { Value = value };
        }

        private static IEnumerable<FieldInfo> Fields(Type type)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            for (Type current = type; current != null; current = current.BaseType)
                foreach (FieldInfo field in current.GetFields(flags)) yield return field;
        }

        private static T Require<T>(IDictionary<string, BlueprintScriptableObject>
            values, string symbol) where T : BlueprintScriptableObject
        { return (T)values[symbol]; }
        private static string InternalName(string symbol)
        { return symbol.Replace('.', '_').Replace('-', '_'); }
        private static string Token(string key)
        { return string.Concat(key.Split('-').Select(part =>
            char.ToUpperInvariant(part[0]) + part.Substring(1))); }
    }
}
