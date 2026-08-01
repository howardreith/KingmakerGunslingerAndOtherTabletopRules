using System;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Utility;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using KingmakerGunslinger.Deeds;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class PistolWhipBlueprintSet
    {
        internal PistolWhipBlueprintSet(BlueprintFeature feature,
            BlueprintAbility ability, BlueprintWeaponType oneType,
            BlueprintItemWeapon oneItem, BlueprintWeaponType twoType,
            BlueprintItemWeapon twoItem)
        {
            Feature = feature; Ability = ability;
            OneHandedType = oneType; OneHandedItem = oneItem;
            TwoHandedType = twoType; TwoHandedItem = twoItem;
        }
        internal BlueprintFeature Feature { get; private set; }
        internal BlueprintAbility Ability { get; private set; }
        internal BlueprintWeaponType OneHandedType { get; private set; }
        internal BlueprintItemWeapon OneHandedItem { get; private set; }
        internal BlueprintWeaponType TwoHandedType { get; private set; }
        internal BlueprintItemWeapon TwoHandedItem { get; private set; }
        internal int Count { get { return 6; } }
    }

    internal static class PistolWhipBlueprints
    {
        internal const string FeatureSymbol = "KMG.Deeds.PistolWhipFeature";
        internal const string AbilitySymbol = "KMG.Deeds.PistolWhipAbility";
        internal const string OneTypeSymbol = "KMG.Deeds.PistolWhipOneHandedType";
        internal const string OneItemSymbol = "KMG.Deeds.PistolWhipOneHandedItem";
        internal const string TwoTypeSymbol = "KMG.Deeds.PistolWhipTwoHandedType";
        internal const string TwoItemSymbol = "KMG.Deeds.PistolWhipTwoHandedItem";
        private const BindingFlags Fields = BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic;

        internal static PistolWhipBlueprintSet Register(BlueprintRegistry registry,
            BlueprintItemWeapon sourceItem)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            if (sourceItem == null) throw new ArgumentNullException("sourceItem");
            WeaponBlueprintAccess itemType = WeaponBlueprintAccess.Resolve();
            BlueprintWeaponType sourceType = itemType.Get(sourceItem);
            BlueprintWeaponType oneType = registry.Register<BlueprintWeaponType>(
                OneTypeSymbol, () => CreateType(sourceType,
                    "KMG_PistolWhip_OneHanded_Type", false, DiceType.D6));
            BlueprintItemWeapon oneItem = registry.Register<BlueprintItemWeapon>(
                OneItemSymbol, () => CreateItem(sourceItem, oneType,
                    "KMG_PistolWhip_OneHanded_Item", itemType));
            BlueprintWeaponType twoType = registry.Register<BlueprintWeaponType>(
                TwoTypeSymbol, () => CreateType(sourceType,
                    "KMG_PistolWhip_TwoHanded_Type", true, DiceType.D10));
            BlueprintItemWeapon twoItem = registry.Register<BlueprintItemWeapon>(
                TwoItemSymbol, () => CreateItem(sourceItem, twoType,
                    "KMG_PistolWhip_TwoHanded_Item", itemType));
            BlueprintAbility ability = registry.Register<BlueprintAbility>(
                AbilitySymbol, () => CreateAbility(oneItem, twoItem));
            BlueprintFeature feature = registry.Register<BlueprintFeature>(
                FeatureSymbol, () => CreateFeature(ability));
            Validate(feature, ability, oneType, oneItem, twoType, twoItem,
                itemType);
            return new PistolWhipBlueprintSet(feature, ability, oneType,
                oneItem, twoType, twoItem);
        }

        private static BlueprintWeaponType CreateType(BlueprintWeaponType source,
            string name, bool twoHanded, DiceType die)
        {
            BlueprintWeaponType result = BlueprintCloneService.Clone(source, name);
            result.ComponentsArray = Array.Empty<BlueprintComponent>();
            Set(result, "m_AttackType", AttackType.Melee);
            Set(result, "m_AttackRange", new Feet(5));
            Set(result, "m_BaseDamage", new DiceFormula(1, die));
            Set(result, "m_DamageType", Bludgeoning());
            Set(result, "m_CriticalRollEdge", 20);
            Set(result, "m_CriticalModifier", DamageCriticalModifierType.X2);
            Set(result, "m_IsTwoHanded", twoHanded);
            Set(result, "m_IsLight", false);
            return result;
        }

        private static BlueprintItemWeapon CreateItem(BlueprintItemWeapon source,
            BlueprintWeaponType type, string name, WeaponBlueprintAccess access)
        {
            BlueprintItemWeapon result = BlueprintCloneService.Clone(source, name);
            result.ComponentsArray = Array.Empty<BlueprintComponent>();
            access.Set(result, type);
            Set(result, "m_Enchantments", Array.Empty<BlueprintWeaponEnchantment>());
            Set(result, "m_OverrideDamageDice", false);
            Set(result, "m_OverrideDamageType", false);
            return result;
        }

        private static BlueprintAbility CreateAbility(BlueprintItemWeapon one,
            BlueprintItemWeapon two)
        {
            var result = ScriptableObject.CreateInstance<BlueprintAbility>();
            result.name = "KMG_PistolWhip_Ability";
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.PistolWhip.Ability.Name", "Pistol-Whip"),
                LocalizationService.Create("KMG.PistolWhip.Ability.Description",
                    "Spend 1 grit as a standard action to strike an adjacent enemy with your equipped firearm. A hit also attempts to knock the target prone."), null);
            result.Type = AbilityType.Extraordinary;
            result.Range = AbilityRange.Touch;
            result.CanTargetEnemies = true;
            result.CanTargetSelf = result.CanTargetFriends = result.CanTargetPoint = false;
            result.SpellResistance = false; result.Hidden = false;
            result.ActionBarAutoFillIgnored = false; result.NeedEquipWeapons = false;
            result.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            result.EffectOnAlly = AbilityEffectOnUnit.None;
            result.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Touch;
            result.ActionType = UnitCommand.CommandType.Standard;
            result.ResourceAssetIds = Array.Empty<string>();
            result.ComponentsArray = new BlueprintComponent[] {
                PistolWhipAbilityLogic.Create(one, two) };
            return result;
        }

        private static BlueprintFeature CreateFeature(BlueprintAbility ability)
        {
            var result = ScriptableObject.CreateInstance<BlueprintFeature>();
            result.name = "KMG_PistolWhip_Feature";
            result.Ranks = 1; result.IsClassFeature = true; result.HideInUI = false;
            var add = ScriptableObject.CreateInstance<AddFacts>();
            add.name = "$KMG_PistolWhip_Grant";
            add.Facts = new BlueprintUnitFact[] { ability };
            add.DoNotRestoreMissingFacts = false;
            result.ComponentsArray = new BlueprintComponent[] { add };
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.PistolWhip.Feature.Name", "Pistol-Whip"),
                LocalizationService.Create("KMG.PistolWhip.Feature.Description",
                    "Strike with an equipped firearm and attempt a trip maneuver."), null);
            return result;
        }

        private static void Validate(BlueprintFeature feature, BlueprintAbility ability,
            BlueprintWeaponType oneType, BlueprintItemWeapon oneItem,
            BlueprintWeaponType twoType, BlueprintItemWeapon twoItem,
            WeaponBlueprintAccess access)
        {
            PistolWhipAbilityLogic logic = ability.ComponentsArray
                .OfType<PistolWhipAbilityLogic>().Single();
            AddFacts grant = feature.ComponentsArray.OfType<AddFacts>().Single();
            if (ability.ActionType != UnitCommand.CommandType.Standard ||
                ability.Range != AbilityRange.Touch || !ability.CanTargetEnemies ||
                grant.Facts.Length != 1 || !ReferenceEquals(grant.Facts[0], ability) ||
                !ReferenceEquals(logic.OneHandedSurrogate, oneItem) ||
                !ReferenceEquals(logic.TwoHandedSurrogate, twoItem) ||
                !ReferenceEquals(access.Get(oneItem), oneType) ||
                !ReferenceEquals(access.Get(twoItem), twoType) ||
                oneType.AttackType != AttackType.Melee ||
                twoType.AttackType != AttackType.Melee || oneType.IsTwoHanded ||
                !twoType.IsTwoHanded || !oneType.BaseDamage.Equals(new DiceFormula(1, DiceType.D6)) ||
                !twoType.BaseDamage.Equals(new DiceFormula(1, DiceType.D10)) ||
                oneType.CriticalRollEdge != 20 || twoType.CriticalRollEdge != 20 ||
                oneType.CriticalModifier != DamageCriticalModifierType.X2 ||
                twoType.CriticalModifier != DamageCriticalModifierType.X2)
                throw new InvalidOperationException("Pistol-Whip blueprint contract is incomplete.");
        }

        private static DamageTypeDescription Bludgeoning()
        {
            return new DamageTypeDescription {
                Type = DamageType.Physical,
                Common = new DamageTypeDescription.CommomData(),
                Physical = new DamageTypeDescription.PhysicalData {
                    Form = PhysicalDamageForm.Bludgeoning }
            };
        }

        private static void Set(object target, string name, object value)
        {
            FieldInfo field = target.GetType().GetField(name, Fields);
            if (field == null) throw new MissingFieldException(target.GetType().FullName, name);
            field.SetValue(target, value);
        }
    }
}
