using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.FactLogic;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElvenBranchedSpear;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static class ElvenBranchedSpearBlueprints
    {
        internal const string NativeLongspearTypeGuid =
            "fa2dd17cbde7d3f4aa918d467c30516e";
        internal const string NativeLongspearItemGuid =
            "f28f6031c2908d84d945865a80f67177";
        internal const string NativeMasterworkGuid =
            "6b38844e2bffbac48b63036b66e735be";
        internal const string NativeEnhancementOneGuid =
            "d42fc23b92c640846ac137dc26e000d4";
        internal const string NativeExoticWeaponProficiencySelectionGuid =
            "9a01b6815d6c3684cb25f30b8bf20932";
        internal const string NativeElvenCurveBladeProficiencyGuid =
            "0fca9259e370cd049a1dd50bede687f7";
        internal const string NativeFinesseTrainingSelectionGuid =
            "b78d146cea711a84598f0acef69462ea";
        internal const string NativeFinesseTrainingElvenCurveBladeGuid =
            "04f3b956e5a5cf649bce83774e0bfe4a";
        internal const string NativeElvenWeaponFamiliarityGuid =
            "03fd1e043fc678a4baf73fe67c3780ce";
        internal const string ExoticWeaponProficiencySymbol =
            "KMG.ElvenBranchedSpear.ExoticWeaponProficiency";
        internal const string FinesseTrainingSymbol =
            "KMG.ElvenBranchedSpear.FinesseTraining";
        internal const string MovementOpportunityAccuracySymbol =
            "KMG.ElvenBranchedSpear.MovementOpportunityAccuracy";

        private static readonly string[] ParameterSelectorGuids =
        {
            "1e1f627d26ad36f43bbd26cc2bf8ac7e", // Weapon Focus
            "09c9e82965fb4334b984a1e9df3bd088", // Greater Weapon Focus
            "f4201c85a991369408740c6888362e20", // Improved Critical
            "31470b17e8446ae4ea0dacd6c5817d86", // Weapon Specialization
            "7cf5edc65e785a24f9cf93af987d66b3", // Greater Weapon Specialization
            "c0b4ec0175e3ff940a45fc21f318a39a", // Sword Saint chosen weapon
            "38ae5ac04463a8947b7c06a6c72dd6bb"  // Weapon Mastery
        };

        internal static ElvenBranchedSpearBlueprintSet Register(
            LibraryScriptableObject library, BlueprintRegistry registry,
            bool publishSelectors, ModLogger logger)
        {
            if (library == null || registry == null || logger == null)
                throw new ArgumentNullException("Spear registration inputs are incomplete.");
            WeaponCategory category = ElvenBranchedSpearCategoryRuntime.Category;
            BlueprintWeaponType collision = library.GetAllBlueprints()
                .OfType<BlueprintWeaponType>().FirstOrDefault(value =>
                    value != null && value.Category.Equals(category));
            if (collision != null)
                throw new InvalidOperationException("Elven Branched Spear category collision: " +
                    collision.name + ":" + collision.AssetGuid + ".");

            BlueprintWeaponType nativeType = BlueprintLibraryLookup.RequireExact<
                BlueprintWeaponType>(library, NativeLongspearTypeGuid,
                    "native Longspear weapon type");
            BlueprintItemWeapon nativeItem = BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativeLongspearItemGuid,
                    "native Standard Longspear item");
            BlueprintWeaponEnchantment masterwork = BlueprintLibraryLookup.RequireExact<
                BlueprintWeaponEnchantment>(library, NativeMasterworkGuid,
                    "native masterwork enchantment");
            BlueprintWeaponEnchantment plusOne = BlueprintLibraryLookup.RequireExact<
                BlueprintWeaponEnchantment>(library, NativeEnhancementOneGuid,
                    "native +1 weapon enchantment");
            WeaponBlueprintAccess typeAccess = WeaponBlueprintAccess.Resolve();
            if (!ReferenceEquals(typeAccess.Get(nativeItem), nativeType))
                throw new InvalidOperationException("Native Longspear item/type relation changed.");
            float nativeWeight = nativeType.Weight;
            WeaponCategory nativeCategory = nativeType.Category;

            var typeAdapter = new SpearWeaponTypeAccess();
            var itemAdapter = new SpearItemAccess();
            var name = LocalizationService.Create(
                "KMG.ElvenBranchedSpear.Type.Name", "Elven Branched Spear");
            var description = LocalizationService.Create(
                "KMG.ElvenBranchedSpear.Type.Description",
                "This exotic two-handed reach spear can be used with Weapon Finesse. It grants a +2 bonus on attacks of opportunity provoked by movement.");
            BlueprintWeaponEnchantment movementAccuracy =
                registry.Register<BlueprintWeaponEnchantment>(
                    MovementOpportunityAccuracySymbol, delegate
                    {
                        BlueprintWeaponEnchantment value = ScriptableObject
                            .CreateInstance<BlueprintWeaponEnchantment>();
                        value.name = "KMG_ElvenBranchedSpear_MovementOpportunityAccuracy";
                        ConfigureEnchantmentText(value,
                            "Elven Branched Spear",
                            "This weapon grants a +2 bonus on attacks of opportunity provoked by movement.", 0);
                        MovementOpportunityAccuracyComponent component =
                            MovementOpportunityAccuracyComponent.Create();
                        component.name = "$KMG_ElvenBranchedSpear_MovementOpportunityAccuracy";
                        ElvenBranchedSpearProficiencyPenaltyComponent penalty =
                            ElvenBranchedSpearProficiencyPenaltyComponent.Create();
                        penalty.name = "$KMG_ElvenBranchedSpear_ProficiencyPenalty";
                        value.ComponentsArray = new BlueprintComponent[] {
                            component, penalty };
                        return value;
                    });
            BlueprintWeaponType weaponType = registry.Register<BlueprintWeaponType>(
                ElvenBranchedSpearCatalog.WeaponTypeSymbol, delegate
                {
                    BlueprintWeaponType clone = BlueprintCloneService.Clone(nativeType,
                        "KMG_ElvenBranchedSpear_WeaponType");
                    typeAdapter.Configure(clone, category, name, description,
                        movementAccuracy);
                    Assets.ElvenBranchedSpearAssetRuntime.ApplyTo(clone);
                    return clone;
                });

            var entries = new List<ElvenBranchedSpearBlueprintEntry>();
            foreach (ElvenBranchedSpearItemSpec spec in ElvenBranchedSpearCatalog.All)
            {
                BlueprintItemWeapon item = registry.Register<BlueprintItemWeapon>(
                    spec.Symbol, delegate
                    {
                        BlueprintItemWeapon clone = BlueprintCloneService.Clone(
                            nativeItem, spec.InternalName);
                        typeAccess.Set(clone, weaponType);
                        BlueprintWeaponEnchantment[] enchantments = spec.Enhancement == 1
                            ? new[] { plusOne }
                            : spec.Masterwork ? new[] { masterwork }
                            : Array.Empty<BlueprintWeaponEnchantment>();
                        itemAdapter.Configure(clone, spec, enchantments);
                        Assets.ElvenBranchedSpearAssetRuntime.ApplyTo(clone,
                            spec.Symbol);
                        return clone;
                    });
                entries.Add(new ElvenBranchedSpearBlueprintEntry(spec, item));
            }

            BlueprintFeature ewp = registry.Register<BlueprintFeature>(
                ExoticWeaponProficiencySymbol, delegate
                {
                    BlueprintFeature donor = BlueprintLibraryLookup.RequireExact<
                        BlueprintFeature>(library,
                            NativeElvenCurveBladeProficiencyGuid,
                            "native Elven Curve Blade proficiency option");
                    BlueprintFeature clone = CloneFeatureWithComponents(donor,
                        "KMG_ExoticWeaponProficiency_ElvenBranchedSpear");
                    ConfigureProficiencyFeature(clone, category, donor.Icon);
                    return clone;
                });
            BlueprintFeature finesseTraining = registry.Register<BlueprintFeature>(
                FinesseTrainingSymbol, delegate
                {
                    BlueprintFeature donor = BlueprintLibraryLookup.RequireExact<
                        BlueprintFeature>(library,
                            NativeFinesseTrainingElvenCurveBladeGuid,
                            "native Elven Curve Blade Finesse Training option");
                    BlueprintFeature clone = CloneFeatureWithComponents(donor,
                        "KMG_FinesseTraining_ElvenBranchedSpear");
                    ConfigureFinesseTrainingFeature(clone, category, donor.Icon);
                    return clone;
                });

            BlueprintFeatureSelection ewpSelection = BlueprintLibraryLookup.RequireExact<
                BlueprintFeatureSelection>(library,
                    NativeExoticWeaponProficiencySelectionGuid,
                    "native Exotic Weapon Proficiency selection");
            BlueprintFeatureSelection finesseSelection = BlueprintLibraryLookup.RequireExact<
                BlueprintFeatureSelection>(library,
                    NativeFinesseTrainingSelectionGuid,
                    "native Rogue Finesse Training selection");
            BlueprintFeature familiarity = BlueprintLibraryLookup.RequireExact<
                BlueprintFeature>(library, NativeElvenWeaponFamiliarityGuid,
                    "native Elven Weapon Familiarity feature");
            bool ewpUsesCategoryIcon = NativeChildrenUseCategoryIcons(
                ewpSelection, ewp);
            bool finesseUsesCategoryIcon = NativeChildrenUseCategoryIcons(
                finesseSelection, finesseTraining);
            BlueprintParametrizedFeature[] parameterSelectors = ParameterSelectorGuids
                .Select(guid => BlueprintLibraryLookup.RequireExact<
                    BlueprintParametrizedFeature>(library, guid,
                        "native chosen-weapon selector")).ToArray();
            ElvenBranchedSpearSelectorPublication publication =
                ElvenBranchedSpearSelectorPublication.Publish(ewpSelection, ewp,
                    BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library,
                        NativeElvenCurveBladeProficiencyGuid,
                        "native Elven Curve Blade proficiency ordering anchor"),
                    finesseSelection, finesseTraining, familiarity, category,
                    parameterSelectors, publishSelectors);

            var result = new ElvenBranchedSpearBlueprintSet(weaponType,
                entries.ToArray(), ewp, finesseTraining, publication,
                ewpUsesCategoryIcon, finesseUsesCategoryIcon,
                ewp.Icon, finesseTraining.Icon);
            if (movementAccuracy.EnchantmentCost != 0 ||
                movementAccuracy.ComponentsArray == null ||
                movementAccuracy.ComponentsArray.Length != 2 ||
                movementAccuracy.ComponentsArray.OfType<
                    MovementOpportunityAccuracyComponent>().Count() != 1 ||
                movementAccuracy.ComponentsArray.OfType<
                    ElvenBranchedSpearProficiencyPenaltyComponent>().Count() != 1)
                throw new InvalidOperationException(
                    "Movement-opportunity accuracy enchantment is malformed.");
            Validate(result, typeAccess, typeAdapter, itemAdapter);
            if (!nativeType.Category.Equals(nativeCategory) ||
                !nativeType.Weight.Equals(nativeWeight) ||
                !ReferenceEquals(typeAccess.Get(nativeItem), nativeType))
                throw new InvalidOperationException("Native Longspear donor was mutated.");
            logger.Info("elven-branched-spear", "foundation.ready",
                "Registered one stable category, six foundation items, native selector options, and Elf familiarity integration; selector publication=" +
                publishSelectors + ".");
            return result;
        }

        internal static void Validate(ElvenBranchedSpearBlueprintSet set,
            WeaponBlueprintAccess typeAccess, SpearWeaponTypeAccess typeAdapter,
            SpearItemAccess itemAdapter)
        {
            if (set == null || set.Entries.Length != 6 ||
                set.Entries.Select(value => value.Item).Distinct().Count() != 6)
                throw new InvalidOperationException("Spear foundation identity count is invalid.");
            typeAdapter.Validate(set.WeaponType);
            foreach (ElvenBranchedSpearBlueprintEntry entry in set.Entries)
            {
                if (!ReferenceEquals(typeAccess.Get(entry.Item), set.WeaponType))
                    throw new InvalidOperationException("A spear item left the shared category family.");
                itemAdapter.Validate(entry.Item, entry.Spec);
            }
        }

        private static BlueprintFeature CloneFeatureWithComponents(
            BlueprintFeature donor, string internalName)
        {
            BlueprintFeature clone = BlueprintCloneService.Clone(donor, internalName);
            clone.ComponentsArray = (donor.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Select(value =>
                    (BlueprintComponent)UnityEngine.Object.Instantiate(value)).ToArray();
            return clone;
        }

        private static void ConfigureEnchantmentText(
            BlueprintWeaponEnchantment enchantment, string name,
            string description, int cost)
        {
            const BindingFlags fields = BindingFlags.Instance |
                BindingFlags.NonPublic;
            Type owner = typeof(BlueprintItemEnchantment);
            FieldInfo nameField = owner.GetField("m_EnchantName", fields);
            FieldInfo descriptionField = owner.GetField("m_Description", fields);
            FieldInfo costField = owner.GetField("m_EnchantmentCost", fields);
            if (nameField == null || descriptionField == null || costField == null)
                throw new MissingFieldException(owner.FullName,
                    "m_EnchantName/m_Description/m_EnchantmentCost");
            nameField.SetValue(enchantment, LocalizationService.Create(
                MovementOpportunityAccuracySymbol + ".Name", name));
            descriptionField.SetValue(enchantment, LocalizationService.Create(
                MovementOpportunityAccuracySymbol + ".Description", description));
            costField.SetValue(enchantment, cost);
        }

        private static void ConfigureProficiencyFeature(BlueprintFeature feature,
            WeaponCategory category, Sprite nativeIcon)
        {
            AddProficiencies grant = feature.ComponentsArray.OfType<AddProficiencies>()
                .Single();
            grant.RaceRestriction = null;
            grant.ArmorProficiencies = Array.Empty<
                Kingmaker.Blueprints.Items.Armors.ArmorProficiencyGroup>();
            grant.WeaponProficiencies = new[] { category };
            PrerequisiteNotProficient absent = feature.ComponentsArray
                .OfType<PrerequisiteNotProficient>().Single();
            absent.ArmorProficiencies = Array.Empty<
                Kingmaker.Blueprints.Items.Armors.ArmorProficiencyGroup>();
            absent.WeaponProficiencies = new[] { category };
            AddStartingEquipment equipment = feature.ComponentsArray
                .OfType<AddStartingEquipment>().Single();
            equipment.BasicItems = Array.Empty<Kingmaker.Blueprints.Items.BlueprintItem>();
            equipment.CategoryItems = new[] { category };
            equipment.ParametrizedCategory = false;
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create("KMG.ElvenBranchedSpear.EWP.Name",
                    "Weapon Proficiency (Elven Branched Spear)"),
                LocalizationService.Create("KMG.ElvenBranchedSpear.EWP.Description",
                    "You are proficient with the exotic Elven Branched Spear."),
                nativeIcon);
        }

        private static void ConfigureFinesseTrainingFeature(BlueprintFeature feature,
            WeaponCategory category, Sprite nativeIcon)
        {
            WeaponTypeDamageStatReplacement replacement = feature.ComponentsArray
                .OfType<WeaponTypeDamageStatReplacement>().Single();
            replacement.Category = category;
            replacement.OnlyOneHanded = false;
                replacement.TwoHandedBonus = true;
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create("KMG.ElvenBranchedSpear.FinesseTraining.Name",
                    "Finesse Training (Elven Branched Spear)"),
                LocalizationService.Create(
                    "KMG.ElvenBranchedSpear.FinesseTraining.Description",
                    "Use Dexterity instead of Strength for damage rolls with Elven Branched Spears."),
                nativeIcon);
        }

        private static bool NativeChildrenUseCategoryIcons(
            BlueprintFeatureSelection selection, BlueprintFeature customFeature)
        {
            BlueprintFeature[] native = (selection.AllFeatures ??
                Array.Empty<BlueprintFeature>()).Where(value => value != null &&
                    !ReferenceEquals(value, customFeature)).ToArray();
            Sprite[] icons = native.Select(value => value.Icon)
                .Where(value => value != null).ToArray();
            if (customFeature.Icon == null || icons.Length == 0)
                return true;
            return icons.Distinct().Count() > 1;
        }
    }

    internal sealed class SpearWeaponTypeAccess
    {
        private const BindingFlags Fields = BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic;
        private readonly FieldInfo _typeName = Require("m_TypeNameText");
        private readonly FieldInfo _defaultName = Require("m_DefaultNameText");
        private readonly FieldInfo _description = Require("m_DescriptionText");
        private readonly FieldInfo _masterworkDescription =
            Require("m_MasterworkDescriptionText");
        private readonly FieldInfo _magicDescription = Require("m_MagicDescriptionText");
        private readonly FieldInfo _weight = Require("m_Weight");
        private readonly FieldInfo _enchantments = Require("m_Enchantments");

        internal void Configure(BlueprintWeaponType type, WeaponCategory category,
            Kingmaker.Localization.LocalizedString name,
            Kingmaker.Localization.LocalizedString description,
            BlueprintWeaponEnchantment movementAccuracy)
        {
            type.Category = category;
            _typeName.SetValue(type, name);
            _defaultName.SetValue(type, name);
            _description.SetValue(type, description);
            _masterworkDescription.SetValue(type, description);
            _magicDescription.SetValue(type, description);
            _weight.SetValue(type, (float)ElvenBranchedSpearCatalog.WeightPounds);
            _enchantments.SetValue(type, new[] { movementAccuracy });
            Validate(type);
        }

        internal void Validate(BlueprintWeaponType type)
        {
            if (!type.Category.Equals(ElvenBranchedSpearCategoryRuntime.Category) ||
                type.AttackType != AttackType.Melee ||
                !type.BaseDamage.Equals(new DiceFormula(1, DiceType.D8)) ||
                type.DamageType.Type != DamageType.Physical ||
                type.DamageType.Physical.Form != PhysicalDamageForm.Piercing ||
                type.CriticalRollEdge != 20 ||
                type.CriticalModifier != DamageCriticalModifierType.X3 ||
                type.FighterGroup != WeaponFighterGroup.Spears ||
                !type.IsTwoHanded || type.IsLight || type.IsNatural ||
                type.AttackRange.Value != 6 || !type.Weight.Equals(10f))
                throw new InvalidOperationException("Elven Branched Spear type profile is invalid.");
            BlueprintWeaponEnchantment[] enchantments =
                (BlueprintWeaponEnchantment[])_enchantments.GetValue(type) ??
                Array.Empty<BlueprintWeaponEnchantment>();
            if (enchantments.Length != 1 || enchantments[0] == null ||
                !string.Equals(enchantments[0].name,
                    "KMG_ElvenBranchedSpear_MovementOpportunityAccuracy",
                    StringComparison.Ordinal))
                throw new InvalidOperationException(
                    "Spear inherent movement-opportunity enchantment is invalid.");
            WeaponSubCategory[] expected = {
                WeaponSubCategory.Melee, WeaponSubCategory.Finessable,
                WeaponSubCategory.TwoHanded, WeaponSubCategory.Exotic,
                WeaponSubCategory.Metal };
            if (!expected.All(ElvenBranchedSpearCategoryRuntime.HasSubCategory) ||
                ElvenBranchedSpearCategoryRuntime.HasSubCategory(WeaponSubCategory.Light) ||
                ElvenBranchedSpearCategoryRuntime.HasSubCategory(WeaponSubCategory.Martial) ||
                ElvenBranchedSpearCategoryRuntime.HasSubCategory(
                    WeaponSubCategory.OneHandedPiercing) ||
                ElvenBranchedSpearCategoryRuntime.HasSubCategory(
                    WeaponSubCategory.OneHandedSlashing) ||
                ElvenBranchedSpearCategoryRuntime.HasSubCategory(WeaponSubCategory.Thrown))
                throw new InvalidOperationException("Spear subcategory policy is invalid.");
        }

        private static FieldInfo Require(string name)
        {
            FieldInfo field = typeof(BlueprintWeaponType).GetField(name, Fields);
            if (field == null) throw new MissingFieldException(
                typeof(BlueprintWeaponType).FullName, name);
            return field;
        }
    }

    internal sealed class SpearItemAccess
    {
        private const BindingFlags Fields = BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic;
        private readonly FieldInfo _enchantments = Require("m_Enchantments");
        private readonly FieldInfo _overrideDamageType = Require("m_OverrideDamageType");
        private readonly FieldInfo _damageType = Require("m_DamageType");

        internal void Configure(BlueprintItemWeapon item,
            ElvenBranchedSpearItemSpec spec,
            BlueprintWeaponEnchantment[] enchantments)
        {
            string description = Describe(spec);
            BlueprintItemAccess.Resolve().ConfigureWeapon(item,
                LocalizationService.Create(spec.Symbol + ".Name", spec.DisplayName),
                LocalizationService.Create(spec.Symbol + ".Description", description),
                LocalizationService.Create(spec.Symbol + ".Flavor",
                    "Forward-angled leaf blades give this elegant elven spear its unmistakable silhouette."),
                spec.Cost, ElvenBranchedSpearCatalog.WeightPounds);
            _enchantments.SetValue(item, enchantments == null
                ? Array.Empty<BlueprintWeaponEnchantment>()
                : enchantments.ToArray());
            _overrideDamageType.SetValue(item, spec.ColdIron);
            _damageType.SetValue(item, PhysicalPiercing(spec.ColdIron));
        }

        internal void Validate(BlueprintItemWeapon item,
            ElvenBranchedSpearItemSpec spec)
        {
            BlueprintWeaponEnchantment[] enchantments =
                (BlueprintWeaponEnchantment[])_enchantments.GetValue(item) ??
                Array.Empty<BlueprintWeaponEnchantment>();
            DamageTypeDescription damage = (DamageTypeDescription)_damageType.GetValue(item);
            if (item.Cost != spec.Cost || !item.Weight.Equals(10f) ||
                item.IsActuallyStackable || item.IsMasterwork !=
                    (spec.Masterwork && spec.Enhancement == 0) ||
                (bool)_overrideDamageType.GetValue(item) != spec.ColdIron ||
                damage == null || damage.Type != DamageType.Physical ||
                damage.Physical.Form != PhysicalDamageForm.Piercing ||
                damage.Physical.Material != (spec.ColdIron
                    ? PhysicalDamageMaterial.ColdIron : 0) ||
                enchantments.Length != (spec.Enhancement == 1 || spec.Masterwork ? 1 : 0) ||
                item.Description.IndexOf("Brace", StringComparison.OrdinalIgnoreCase) >= 0)
                throw new InvalidOperationException("Spear item contract is invalid: " +
                    spec.DisplayName + ";cost=" + item.Cost + ";weight=" +
                    item.Weight + ";stackable=" + item.IsActuallyStackable +
                    ";nativeMasterworkFlag=" + item.IsMasterwork +
                    ";overrideDamage=" + _overrideDamageType.GetValue(item) +
                    ";damage=" + (damage == null ? "<null>" :
                        damage.Type + "/" + damage.Physical.Form + "/" +
                        damage.Physical.Material) + ";enchantments=" +
                    enchantments.Length + ".");
        }

        internal void ConfigureNamed(BlueprintItemWeapon item,
            NamedSpearSpec spec, BlueprintWeaponEnchantment[] enchantments,
            string description)
        {
            BlueprintItemAccess.Resolve().ConfigureWeapon(item,
                LocalizationService.Create(spec.Symbol + ".Name", spec.DisplayName),
                LocalizationService.Create(spec.Symbol + ".Description",
                    description),
                LocalizationService.Create(spec.Symbol + ".Flavor",
                    "An elven polearm whose forward-angled branch blades carry a distinctive magical finish."),
                spec.Cost, ElvenBranchedSpearCatalog.WeightPounds);
            _enchantments.SetValue(item, enchantments == null
                ? Array.Empty<BlueprintWeaponEnchantment>()
                : enchantments.ToArray());
            _overrideDamageType.SetValue(item, spec.ColdIron);
            _damageType.SetValue(item, PhysicalPiercing(spec.ColdIron));
        }

        internal void ValidateNamed(BlueprintItemWeapon item,
            NamedSpearSpec spec, int expectedEnchantments)
        {
            BlueprintWeaponEnchantment[] enchantments =
                (BlueprintWeaponEnchantment[])_enchantments.GetValue(item) ??
                Array.Empty<BlueprintWeaponEnchantment>();
            DamageTypeDescription damage =
                (DamageTypeDescription)_damageType.GetValue(item);
            if (item.Cost != spec.Cost || !item.Weight.Equals(10f) ||
                item.IsActuallyStackable ||
                (bool)_overrideDamageType.GetValue(item) != spec.ColdIron ||
                damage == null || damage.Type != DamageType.Physical ||
                damage.Physical.Form != PhysicalDamageForm.Piercing ||
                damage.Physical.Material != (spec.ColdIron
                    ? PhysicalDamageMaterial.ColdIron : 0) ||
                enchantments.Length != expectedEnchantments ||
                enchantments.Any(value => value == null) ||
                item.Description.IndexOf("Brace",
                    StringComparison.OrdinalIgnoreCase) >= 0)
                throw new InvalidOperationException(
                    "Named spear item contract is invalid: " +
                    spec.DisplayName + ".");
        }

        private static string Describe(ElvenBranchedSpearItemSpec spec)
        {
            string prefix = spec.ColdIron ? "This cold iron " : "This ";
            string quality = spec.Enhancement == 1 ? "+1 magic " :
                spec.Masterwork ? "masterwork " : string.Empty;
            return prefix + quality +
                "two-handed reach weapon can be used with Weapon Finesse and grants a +2 bonus on attacks of opportunity provoked by movement.";
        }

        private static DamageTypeDescription PhysicalPiercing(bool coldIron)
        {
            return new DamageTypeDescription
            {
                Type = DamageType.Physical,
                Common = new DamageTypeDescription.CommomData(),
                Physical = new DamageTypeDescription.PhysicalData
                {
                    Form = PhysicalDamageForm.Piercing,
                    Material = coldIron ? PhysicalDamageMaterial.ColdIron : 0
                }
            };
        }

        private static FieldInfo Require(string name)
        {
            FieldInfo field = typeof(BlueprintItemWeapon).GetField(name, Fields);
            if (field == null) throw new MissingFieldException(
                typeof(BlueprintItemWeapon).FullName, name);
            return field;
        }
    }

    internal sealed class ElvenBranchedSpearBlueprintEntry
    {
        internal ElvenBranchedSpearBlueprintEntry(ElvenBranchedSpearItemSpec spec,
            BlueprintItemWeapon item)
        { Spec = spec; Item = item; }
        internal ElvenBranchedSpearItemSpec Spec { get; private set; }
        internal BlueprintItemWeapon Item { get; private set; }
    }

    internal sealed class ElvenBranchedSpearBlueprintSet
    {
        internal ElvenBranchedSpearBlueprintSet(BlueprintWeaponType weaponType,
            ElvenBranchedSpearBlueprintEntry[] entries,
            BlueprintFeature exoticWeaponProficiency,
            BlueprintFeature finesseTraining,
            ElvenBranchedSpearSelectorPublication publication,
            bool exoticWeaponProficiencyUsesCategoryIcon,
            bool finesseTrainingUsesCategoryIcon,
            Sprite exoticWeaponProficiencyNativeIcon,
            Sprite finesseTrainingNativeIcon)
        {
            WeaponType = weaponType;
            Entries = entries;
            ExoticWeaponProficiency = exoticWeaponProficiency;
            FinesseTraining = finesseTraining;
            Publication = publication;
            ExoticWeaponProficiencyUsesCategoryIcon =
                exoticWeaponProficiencyUsesCategoryIcon;
            FinesseTrainingUsesCategoryIcon = finesseTrainingUsesCategoryIcon;
            ExoticWeaponProficiencyNativeIcon = exoticWeaponProficiencyNativeIcon;
            FinesseTrainingNativeIcon = finesseTrainingNativeIcon;
        }
        internal BlueprintWeaponType WeaponType { get; private set; }
        internal ElvenBranchedSpearBlueprintEntry[] Entries { get; private set; }
        internal BlueprintFeature ExoticWeaponProficiency { get; private set; }
        internal BlueprintFeature FinesseTraining { get; private set; }
        internal bool ExoticWeaponProficiencyUsesCategoryIcon { get; private set; }
        internal bool FinesseTrainingUsesCategoryIcon { get; private set; }
        internal Sprite ExoticWeaponProficiencyNativeIcon { get; private set; }
        internal Sprite FinesseTrainingNativeIcon { get; private set; }
        internal ElvenBranchedSpearSelectorPublication Publication { get; private set; }
        internal ElvenBranchedSpearNamedBlueprintSet Named { get; private set; }
        internal void AttachNamed(ElvenBranchedSpearNamedBlueprintSet named)
        {
            if (named == null || Named != null)
                throw new InvalidOperationException(
                    "Named spear registration may be attached exactly once.");
            Named = named;
        }
        internal ElvenBranchedSpearBlueprintEntry Require(
            ElvenBranchedSpearItemKind kind)
        { return Entries.Single(value => value.Spec.Kind == kind); }
    }
}
