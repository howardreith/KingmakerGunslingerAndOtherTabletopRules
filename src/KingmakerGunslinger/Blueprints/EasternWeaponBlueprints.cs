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
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.FactLogic;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.CustomWeapons;
using KingmakerGunslinger.EasternWeapons;

namespace KingmakerGunslinger.Blueprints
{
    internal static class EasternWeaponBlueprints
    {
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
        internal const string NativeMartialWeaponProficiencyGuid =
            "203992ef5b35c864390b4e4a1e200629";
        internal const string WakizashiVisualDonorGuid =
            "d9fbec4637d71bd4ebc977628de3daf3";
        internal const string KatanaVisualDonorGuid =
            "d2fe2c5516b56f04da1d5ea51ae3ddfe";
        internal const string NodachiVisualDonorGuid =
            "5f824fbb0766a3543bbd6ae50248688f";
        internal const string ElvenBranchedSpearProficiencyGuid =
            "017d586ec4546feabf6eaaa67ce74a3f";
        internal const string ProficiencyPolicyEnchantmentSymbol =
            "KMG.EasternWeapons.ProficiencyPolicyEnchantment";
        internal const string WakizashiProficiencySymbol =
            "KMG.EasternWeapons.Wakizashi.ExoticWeaponProficiency";
        internal const string KatanaProficiencySymbol =
            "KMG.EasternWeapons.Katana.ExoticWeaponProficiency";
        internal const string WakizashiFinesseTrainingSymbol =
            "KMG.EasternWeapons.Wakizashi.FinesseTraining";

        internal static readonly string[] ParameterSelectorGuids =
        {
            "1e1f627d26ad36f43bbd26cc2bf8ac7e",
            "09c9e82965fb4334b984a1e9df3bd088",
            "f4201c85a991369408740c6888362e20",
            "31470b17e8446ae4ea0dacd6c5817d86",
            "7cf5edc65e785a24f9cf93af987d66b3",
            "c0b4ec0175e3ff940a45fc21f318a39a",
            "38ae5ac04463a8947b7c06a6c72dd6bb"
        };

        private static readonly DonorSpec[] Donors =
        {
            new DonorSpec(EasternWeaponFamily.Wakizashi,
                "a7da36e0e7bb60e42b9f23462ce2f4fc",
                "57c8994d1f1becf49ac4f642e5d8ca9d",
                WakizashiVisualDonorGuid),
            new DonorSpec(EasternWeaponFamily.Katana,
                "d2fe2c5516b56f04da1d5ea51ae3ddfe",
                "7b8a4a452f11022488b1c7bfb0ed7746",
                KatanaVisualDonorGuid),
            new DonorSpec(EasternWeaponFamily.Nodachi,
                "6ddc9acbbb6e40746a6a1671df1f7b47",
                "ef8a8cb62410b8641960e9bd8f24a13f",
                NodachiVisualDonorGuid)
        };

        internal static EasternWeaponBlueprintSet Register(
            LibraryScriptableObject library, BlueprintRegistry registry,
            bool publishSelectors, bool presentationEnabled, ModLogger logger)
        {
            if (library == null || registry == null || logger == null)
                throw new ArgumentNullException(
                    "Eastern weapon registration inputs are incomplete.");
            ValidateCategoryCollisions(library);
            EasternWeaponCategoryRuntime.Configure(presentationEnabled);
            BlueprintWeaponEnchantment masterwork = BlueprintLibraryLookup
                .RequireExact<BlueprintWeaponEnchantment>(library,
                    NativeMasterworkGuid, "native masterwork enchantment");
            BlueprintWeaponEnchantment plusOne = BlueprintLibraryLookup
                .RequireExact<BlueprintWeaponEnchantment>(library,
                    NativeEnhancementOneGuid, "native +1 weapon enchantment");
            BlueprintWeaponEnchantment proficiencyPolicy = registry.Register<
                BlueprintWeaponEnchantment>(ProficiencyPolicyEnchantmentSymbol,
                    delegate
                    {
                        BlueprintWeaponEnchantment value =
                            UnityEngine.ScriptableObject.CreateInstance<
                                BlueprintWeaponEnchantment>();
                        value.name =
                            "KMG_EasternWeapons_ProficiencyPolicyEnchantment";
                        ConfigureEnchantmentText(value, string.Empty,
                            string.Empty, 0);
                        EasternWeaponProficiencyPenaltyComponent component =
                            EasternWeaponProficiencyPenaltyComponent.Create();
                        component.name =
                            "$KMG_EasternWeapons_ProficiencyPenalty";
                        value.ComponentsArray = new BlueprintComponent[]
                            { component };
                        return value;
                    });
            WeaponBlueprintAccess typeAccess = WeaponBlueprintAccess.Resolve();
            var typeAdapter = new EasternWeaponTypeAccess();
            var itemAdapter = new EasternWeaponItemAccess();
            var families = new List<EasternWeaponFamilyBlueprintSet>();

            foreach (DonorSpec donor in Donors)
            {
                CustomWeaponCategoryDefinition definition =
                    EasternWeaponCatalog.RequireCategory(donor.Family);
                BlueprintWeaponType nativeType = BlueprintLibraryLookup
                    .RequireExact<BlueprintWeaponType>(library, donor.TypeGuid,
                        "native " + donor.Family + " weapon-type donor");
                BlueprintItemWeapon nativeItem = BlueprintLibraryLookup
                    .RequireExact<BlueprintItemWeapon>(library, donor.ItemGuid,
                        "native " + donor.Family + " item donor");
                BlueprintWeaponType visualDonor = BlueprintLibraryLookup
                    .RequireExact<BlueprintWeaponType>(library,
                        donor.VisualTypeGuid, "native " + donor.Family +
                        " visual/animation donor");
                if (!ReferenceEquals(typeAccess.Get(nativeItem), nativeType))
                    throw new InvalidOperationException("Eastern donor item/type relation changed: " +
                        donor.Family + ".");
                WeaponCategory donorCategory = nativeType.Category;
                BlueprintWeaponType weaponType = registry.Register<BlueprintWeaponType>(
                    definition.WeaponTypeSymbol, delegate
                    {
                        BlueprintWeaponType clone = BlueprintCloneService.Clone(
                            nativeType, "KMG_EasternWeapons_" +
                                definition.Presentation.DisplayName + "_WeaponType");
                        typeAdapter.Configure(clone, definition,
                            proficiencyPolicy);
                        typeAdapter.UseVisualDonor(clone, visualDonor);
                        Assets.EasternWeaponAssetRuntime.ApplyTo(clone,
                            donor.Family);
                        if (clone.VisualParameters == null ||
                            visualDonor.VisualParameters == null ||
                            clone.VisualParameters.AnimStyle !=
                                visualDonor.VisualParameters.AnimStyle)
                            throw new InvalidOperationException(
                                "Eastern visual donor animation did not round-trip: " +
                                donor.Family + ".");
                        return clone;
                    });
                var entries = new List<EasternWeaponBlueprintEntry>();
                foreach (EasternWeaponGenericSpec spec in
                    EasternWeaponCatalog.AllGenericItems.Where(value =>
                        value.Family == donor.Family))
                {
                    BlueprintItemWeapon item = registry.Register<BlueprintItemWeapon>(
                        spec.Symbol, delegate
                        {
                            BlueprintItemWeapon clone = BlueprintCloneService.Clone(
                                nativeItem, spec.InternalName);
                            typeAccess.Set(clone, weaponType);
                            BlueprintWeaponEnchantment[] enchantments =
                                spec.Enhancement == 1 ? new[] { plusOne } :
                                spec.Masterwork ? new[] { masterwork } :
                                Array.Empty<BlueprintWeaponEnchantment>();
                            itemAdapter.Configure(clone, definition, spec,
                                enchantments);
                            Assets.EasternWeaponAssetRuntime.ApplyTo(clone,
                                spec.Symbol, spec.Family);
                            return clone;
                        });
                    entries.Add(new EasternWeaponBlueprintEntry(spec, item));
                }
                if (!nativeType.Category.Equals(donorCategory) ||
                    !ReferenceEquals(typeAccess.Get(nativeItem), nativeType))
                    throw new InvalidOperationException("Eastern donor was mutated: " +
                        donor.Family + ".");
                families.Add(new EasternWeaponFamilyBlueprintSet(donor.Family,
                    weaponType, entries.ToArray()));
            }

            BlueprintItemWeapon masterworkNodachi = families.Single(value =>
                value.Family == EasternWeaponFamily.Nodachi).Entries.Single(
                    value => value.Spec.Kind ==
                        EasternWeaponGenericKind.Masterwork).Item;
            HeirloomNodachiBlueprintSet heirloomNodachi =
                HeirloomNodachiBlueprints.Register(registry,
                    masterworkNodachi);

            BlueprintFeature nativeEwpDonor = BlueprintLibraryLookup
                .RequireExact<BlueprintFeature>(library,
                    NativeElvenCurveBladeProficiencyGuid,
                    "native Elven Curve Blade proficiency child");
            BlueprintFeature wakizashiEwp = registry.Register<BlueprintFeature>(
                WakizashiProficiencySymbol, delegate
                {
                    BlueprintFeature clone = CloneFeatureWithComponents(
                        nativeEwpDonor,
                        "KMG_WeaponProficiency_Wakizashi");
                    ConfigureProficiencyFeature(clone,
                        EasternWeaponFamily.Wakizashi, nativeEwpDonor.Icon);
                    return clone;
                });
            BlueprintFeature katanaEwp = registry.Register<BlueprintFeature>(
                KatanaProficiencySymbol, delegate
                {
                    BlueprintFeature clone = CloneFeatureWithComponents(
                        nativeEwpDonor,
                        "KMG_WeaponProficiency_Katana");
                    ConfigureProficiencyFeature(clone,
                        EasternWeaponFamily.Katana, nativeEwpDonor.Icon);
                    return clone;
                });
            BlueprintFeature nativeFinesseDonor = BlueprintLibraryLookup
                .RequireExact<BlueprintFeature>(library,
                    NativeFinesseTrainingElvenCurveBladeGuid,
                    "native Finesse Training Elven Curve Blade child");
            BlueprintFeature wakizashiFinesse = registry.Register<BlueprintFeature>(
                WakizashiFinesseTrainingSymbol, delegate
                {
                    BlueprintFeature clone = CloneFeatureWithComponents(
                        nativeFinesseDonor,
                        "KMG_FinesseTraining_Wakizashi");
                    ConfigureFinesseTrainingFeature(clone,
                        EasternWeaponCategoryRuntime.Category(
                            EasternWeaponFamily.Wakizashi),
                        nativeFinesseDonor.Icon);
                    return clone;
                });
            BlueprintParametrizedFeature[] parameterSelectors =
                ParameterSelectorGuids.Select(guid => BlueprintLibraryLookup
                    .RequireExact<BlueprintParametrizedFeature>(library, guid,
                        "native generic chosen-weapon selector")).ToArray();
            EasternWeaponSelectorPublication publication =
                EasternWeaponSelectorPublication.Publish(library, wakizashiEwp,
                    katanaEwp, wakizashiFinesse, parameterSelectors,
                    publishSelectors);

            var result = new EasternWeaponBlueprintSet(families.ToArray(),
                proficiencyPolicy, wakizashiEwp, katanaEwp,
                wakizashiFinesse, heirloomNodachi, publication);
            Validate(result, typeAccess, typeAdapter, itemAdapter);
            logger.Info("eastern-weapons", "generic-catalog.ready",
                "Registered three stable categories, twelve generic items, exact proficiency children, and one merged selector publication; selectors=" +
                    publishSelectors + ";presentation=" +
                    presentationEnabled + ".");
            return result;
        }

        private static BlueprintFeature CloneFeatureWithComponents(
            BlueprintFeature donor, string internalName)
        {
            BlueprintFeature clone = BlueprintCloneService.Clone(donor,
                internalName);
            clone.ComponentsArray = (donor.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Select(value =>
                    (BlueprintComponent)UnityEngine.Object.Instantiate(value))
                .ToArray();
            return clone;
        }

        private static void ConfigureProficiencyFeature(BlueprintFeature feature,
            EasternWeaponFamily family, UnityEngine.Sprite nativeIcon)
        {
            WeaponCategory category = EasternWeaponCategoryRuntime.Category(family);
            string display = EasternWeaponCatalog.RequireCategory(family)
                .Presentation.DisplayName;
            AddProficiencies grant = feature.ComponentsArray
                .OfType<AddProficiencies>().Single();
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
            equipment.BasicItems = Array.Empty<
                Kingmaker.Blueprints.Items.BlueprintItem>();
            equipment.CategoryItems = new[] { category };
            equipment.ParametrizedCategory = false;
            string stem = "KMG.EasternWeapons." + display + ".EWP";
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create(stem + ".Name",
                    "Weapon Proficiency (" + display + ")"),
                LocalizationService.Create(stem + ".Description",
                    "You are proficient with the exotic " + display + "."),
                nativeIcon);
        }

        private static void ConfigureFinesseTrainingFeature(
            BlueprintFeature feature, WeaponCategory category,
            UnityEngine.Sprite nativeIcon)
        {
            WeaponTypeDamageStatReplacement replacement = feature.ComponentsArray
                .OfType<WeaponTypeDamageStatReplacement>().Single();
            replacement.Category = category;
            replacement.OnlyOneHanded = true;
            replacement.TwoHandedBonus = false;
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create(
                    "KMG.EasternWeapons.Wakizashi.FinesseTraining.Name",
                    "Finesse Training (Wakizashi)"),
                LocalizationService.Create(
                    "KMG.EasternWeapons.Wakizashi.FinesseTraining.Description",
                    "Use Dexterity instead of Strength for damage rolls with Wakizashis."),
                nativeIcon);
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
                ProficiencyPolicyEnchantmentSymbol + ".Name", name));
            descriptionField.SetValue(enchantment, LocalizationService.Create(
                ProficiencyPolicyEnchantmentSymbol + ".Description",
                description));
            costField.SetValue(enchantment, cost);
        }

        private static void ValidateCategoryCollisions(
            LibraryScriptableObject library)
        {
            var owned = new CustomWeaponCategoryRegistry();
            foreach (CustomWeaponCategoryDefinition definition in
                EasternWeaponCatalog.AllCategories) owned.Add(definition);
            owned.ValidateLoadedValues(library.GetAllBlueprints()
                .OfType<BlueprintWeaponType>().Where(value => value != null)
                .Select(value => new KeyValuePair<int, string>(
                    (int)value.Category, value.name + ":" + value.AssetGuid)));
        }

        private static void Validate(EasternWeaponBlueprintSet set,
            WeaponBlueprintAccess typeAccess, EasternWeaponTypeAccess typeAdapter,
            EasternWeaponItemAccess itemAdapter)
        {
            if (set == null || set.Families.Length != 3 ||
                set.Families.Select(value => value.WeaponType).Distinct().Count() != 3 ||
                set.Entries.Length != 12 ||
                set.Entries.Select(value => value.Item).Distinct().Count() != 12)
                throw new InvalidOperationException(
                    "Eastern generic blueprint cardinality is invalid.");
            foreach (EasternWeaponFamilyBlueprintSet family in set.Families)
            {
                CustomWeaponCategoryDefinition definition =
                    EasternWeaponCatalog.RequireCategory(family.Family);
                typeAdapter.Validate(family.WeaponType, definition);
                foreach (EasternWeaponBlueprintEntry entry in family.Entries)
                {
                    if (!ReferenceEquals(typeAccess.Get(entry.Item), family.WeaponType))
                        throw new InvalidOperationException(
                            "An Eastern item left its stable family weapon type.");
                    itemAdapter.Validate(entry.Item, definition, entry.Spec);
                }
            }
            if (set.ProficiencyPolicy.EnchantmentCost != 0 ||
                set.ProficiencyPolicy.ComponentsArray == null ||
                set.ProficiencyPolicy.ComponentsArray.OfType<
                    EasternWeaponProficiencyPenaltyComponent>().Count() != 1 ||
                set.WakizashiProficiency.Name !=
                    "Weapon Proficiency (Wakizashi)" ||
                set.KatanaProficiency.Name !=
                    "Weapon Proficiency (Katana)" ||
                set.WakizashiFinesseTraining.Name !=
                    "Finesse Training (Wakizashi)")
                throw new InvalidOperationException(
                    "Eastern proficiency blueprint contract is invalid.");
        }

        private sealed class DonorSpec
        {
            internal DonorSpec(EasternWeaponFamily family, string typeGuid,
                string itemGuid, string visualTypeGuid)
            { Family = family; TypeGuid = typeGuid; ItemGuid = itemGuid;
              VisualTypeGuid = visualTypeGuid; }
            internal EasternWeaponFamily Family { get; private set; }
            internal string TypeGuid { get; private set; }
            internal string ItemGuid { get; private set; }
            internal string VisualTypeGuid { get; private set; }
        }
    }

    internal sealed class EasternWeaponTypeAccess
    {
        private const BindingFlags Fields = BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic;
        private readonly FieldInfo _typeName = Require("m_TypeNameText");
        private readonly FieldInfo _defaultName = Require("m_DefaultNameText");
        private readonly FieldInfo _description = Require("m_DescriptionText");
        private readonly FieldInfo _masterworkDescription =
            Require("m_MasterworkDescriptionText");
        private readonly FieldInfo _magicDescription =
            Require("m_MagicDescriptionText");
        private readonly FieldInfo _baseDamage = Require("m_BaseDamage");
        private readonly FieldInfo _damageType = Require("m_DamageType");
        private readonly FieldInfo _criticalEdge = Require("m_CriticalRollEdge");
        private readonly FieldInfo _criticalMultiplier =
            Require("m_CriticalModifier");
        private readonly FieldInfo _fighterGroup = Require("m_FighterGroup");
        private readonly FieldInfo _weight = Require("m_Weight");
        private readonly FieldInfo _isTwoHanded = Require("m_IsTwoHanded");
        private readonly FieldInfo _isLight = Require("m_IsLight");
        private readonly FieldInfo _enchantments = Require("m_Enchantments");
        private readonly FieldInfo _visualParameters =
            Require("m_VisualParameters");

        internal void UseVisualDonor(BlueprintWeaponType type,
            BlueprintWeaponType visualDonor)
        {
            if (type == null || visualDonor == null ||
                visualDonor.VisualParameters == null)
                throw new ArgumentNullException(
                    "Eastern visual donor inputs are incomplete.");
            _visualParameters.SetValue(type, visualDonor.VisualParameters);
            if (!ReferenceEquals(type.VisualParameters,
                    visualDonor.VisualParameters))
                throw new InvalidOperationException(
                    "Eastern visual donor assignment did not round-trip.");
        }

        internal void Configure(BlueprintWeaponType type,
            CustomWeaponCategoryDefinition definition,
            BlueprintWeaponEnchantment proficiencyPolicy)
        {
            string display = definition.Presentation.DisplayName;
            var name = LocalizationService.Create(definition.WeaponTypeSymbol +
                ".Name", display);
            var description = LocalizationService.Create(
                definition.WeaponTypeSymbol + ".Description",
                DescribeType(definition));
            type.Category = (WeaponCategory)definition.CategoryValue;
            _typeName.SetValue(type, name);
            _defaultName.SetValue(type, name);
            _description.SetValue(type, description);
            _masterworkDescription.SetValue(type, description);
            _magicDescription.SetValue(type, description);
            _baseDamage.SetValue(type, DamageDice(definition));
            _damageType.SetValue(type, Physical(definition, false));
            _criticalEdge.SetValue(type, definition.CriticalThreatMinimum);
            _criticalMultiplier.SetValue(type, DamageCriticalModifierType.X2);
            _fighterGroup.SetValue(type, PrimaryGroup(definition));
            _weight.SetValue(type, (float)definition.WeightPounds);
            _isTwoHanded.SetValue(type,
                definition.Handedness != CustomWeaponHandedness.Light &&
                definition.Handedness != CustomWeaponHandedness.OneHandedVersatile);
            _isLight.SetValue(type,
                definition.Handedness == CustomWeaponHandedness.Light);
            _enchantments.SetValue(type,
                new[] { proficiencyPolicy ?? throw new ArgumentNullException(
                    "proficiencyPolicy") });
        }

        internal void Validate(BlueprintWeaponType type,
            CustomWeaponCategoryDefinition definition)
        {
            if ((int)type.Category != definition.CategoryValue ||
                type.AttackType != AttackType.Melee ||
                !type.BaseDamage.Equals(DamageDice(definition)) ||
                type.DamageType.Type != DamageType.Physical ||
                type.DamageType.Physical.Form != Forms(definition) ||
                type.CriticalRollEdge != definition.CriticalThreatMinimum ||
                type.CriticalModifier != DamageCriticalModifierType.X2 ||
                type.FighterGroup != PrimaryGroup(definition) ||
                type.IsTwoHanded !=
                    (definition.Handedness == CustomWeaponHandedness.TwoHanded) ||
                type.IsLight !=
                    (definition.Handedness == CustomWeaponHandedness.Light) ||
                type.IsNatural || type.AttackRange.Value != 2 ||
                !type.Weight.Equals((float)definition.WeightPounds) ||
                type.Enchantments == null || type.Enchantments.Count() != 1 ||
                type.Enchantments.Single() == null || !string.Equals(
                    type.Enchantments.Single().name,
                    "KMG_EasternWeapons_ProficiencyPolicyEnchantment",
                    StringComparison.Ordinal))
                throw new InvalidOperationException("Eastern weapon-type profile is invalid: " +
                    definition.Key + ".");
            if (definition.Handedness ==
                    CustomWeaponHandedness.OneHandedVersatile &&
                !type.IsOneHandedWhichCanBeUsedWithTwoHands)
                throw new InvalidOperationException(
                    "Katana versatile hand contract is not active.");
        }

        internal static DamageTypeDescription Physical(
            CustomWeaponCategoryDefinition definition, bool coldIron)
        {
            return new DamageTypeDescription
            {
                Type = DamageType.Physical,
                Common = new DamageTypeDescription.CommomData(),
                Physical = new DamageTypeDescription.PhysicalData
                {
                    Form = Forms(definition),
                    Material = coldIron ? PhysicalDamageMaterial.ColdIron : 0
                }
            };
        }

        private static DiceFormula DamageDice(
            CustomWeaponCategoryDefinition definition)
        {
            DiceType die = definition.DamageDieSides == 6 ? DiceType.D6 :
                definition.DamageDieSides == 8 ? DiceType.D8 :
                definition.DamageDieSides == 10 ? DiceType.D10 :
                throw new InvalidOperationException("Unsupported Eastern damage die.");
            return new DiceFormula(definition.DamageDiceCount, die);
        }

        internal static PhysicalDamageForm Forms(
            CustomWeaponCategoryDefinition definition)
        {
            PhysicalDamageForm value = 0;
            if ((definition.DamageForms & CustomWeaponDamageForm.Piercing) != 0)
                value |= PhysicalDamageForm.Piercing;
            if ((definition.DamageForms & CustomWeaponDamageForm.Slashing) != 0)
                value |= PhysicalDamageForm.Slashing;
            return value;
        }

        private static WeaponFighterGroup PrimaryGroup(
            CustomWeaponCategoryDefinition definition)
        {
            return (definition.FighterGroups &
                CustomWeaponFighterGroupPolicy.LightBlades) != 0
                ? WeaponFighterGroup.BladesLight
                : WeaponFighterGroup.BladesHeavy;
        }

        private static string DescribeType(
            CustomWeaponCategoryDefinition definition)
        {
            if (definition.Key == "wakizashi")
                return "An exotic light melee weapon that deals piercing or slashing damage and can be used with Weapon Finesse.";
            if (definition.Key == "katana")
                return "An exotic one-handed sword that can be wielded in two hands. Martial Weapon Proficiency is sufficient only when it is wielded in two hands.";
            return "A martial two-handed sword that deals slashing or piercing damage. It is neither a reach weapon nor braced against charges.";
        }

        private static FieldInfo Require(string name)
        {
            FieldInfo field = typeof(BlueprintWeaponType).GetField(name, Fields);
            if (field == null) throw new MissingFieldException(
                typeof(BlueprintWeaponType).FullName, name);
            return field;
        }
    }

    internal sealed class EasternWeaponItemAccess
    {
        private const BindingFlags Fields = BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic;
        private readonly FieldInfo _enchantments = Require("m_Enchantments");
        private readonly FieldInfo _overrideDamageType =
            Require("m_OverrideDamageType");
        private readonly FieldInfo _damageType = Require("m_DamageType");
        private readonly FieldInfo _visualParameters =
            RequireRecursive("m_VisualParameters");

        internal void Configure(BlueprintItemWeapon item,
            CustomWeaponCategoryDefinition definition,
            EasternWeaponGenericSpec spec,
            BlueprintWeaponEnchantment[] enchantments)
        {
            BlueprintItemAccess.Resolve().ConfigureWeapon(item,
                LocalizationService.Create(spec.Symbol + ".Name", spec.DisplayName),
                LocalizationService.Create(spec.Symbol + ".Description",
                    DescribeItem(definition)),
                LocalizationService.Create(spec.Symbol + ".Flavor",
                    "A carefully proportioned curved blade imported through specialist trade."),
                spec.Cost, definition.WeightPounds);
            _enchantments.SetValue(item, enchantments == null
                ? Array.Empty<BlueprintWeaponEnchantment>()
                : enchantments.ToArray());
            _overrideDamageType.SetValue(item, spec.ColdIron);
            _damageType.SetValue(item,
                EasternWeaponTypeAccess.Physical(definition, spec.ColdIron));
            _visualParameters.SetValue(item, item.Type.VisualParameters);
        }

        private static string DescribeItem(
            CustomWeaponCategoryDefinition definition)
        {
            return definition.Key == "wakizashi" ?
                "Usable with Weapon Finesse." :
                definition.Key == "katana" ?
                "Martial Weapon Proficiency is sufficient when wielded two-handed." :
                "A long cavalry blade designed for sweeping cuts.";
        }

        internal void Validate(BlueprintItemWeapon item,
            CustomWeaponCategoryDefinition definition,
            EasternWeaponGenericSpec spec)
        {
            BlueprintWeaponEnchantment[] enchantments =
                (BlueprintWeaponEnchantment[])_enchantments.GetValue(item) ??
                Array.Empty<BlueprintWeaponEnchantment>();
            DamageTypeDescription damage =
                (DamageTypeDescription)_damageType.GetValue(item);
            int expectedEnchantments = spec.Enhancement == 1 || spec.Masterwork ? 1 : 0;
            if (item.Cost != spec.Cost ||
                !item.Weight.Equals((float)definition.WeightPounds) ||
                item.IsActuallyStackable ||
                item.IsMasterwork != (spec.Masterwork && spec.Enhancement == 0) ||
                (bool)_overrideDamageType.GetValue(item) != spec.ColdIron ||
                damage == null || damage.Type != DamageType.Physical ||
                damage.Physical.Material != (spec.ColdIron
                    ? PhysicalDamageMaterial.ColdIron : 0) ||
                enchantments.Length != expectedEnchantments ||
                enchantments.Any(value => value == null) ||
                !ReferenceEquals(_visualParameters.GetValue(item),
                    item.VisualParameters) ||
                !Assets.EasternWeaponAssetRuntime
                    .HasApprovedVisualOrNativeFallback(item, spec.Symbol) ||
                item.Description.IndexOf("Brace",
                    StringComparison.OrdinalIgnoreCase) >= 0)
                throw new InvalidOperationException("Eastern generic item is invalid: " +
                    spec.DisplayName + ".");
        }

        internal void ConfigureNamed(BlueprintItemWeapon item,
            EasternWeaponNamedSpec spec,
            BlueprintWeaponEnchantment[] enchantments, string description)
        {
            CustomWeaponCategoryDefinition definition =
                EasternWeaponCatalog.RequireCategory(spec.Family);
            BlueprintItemAccess.Resolve().ConfigureWeapon(item,
                LocalizationService.Create(spec.Symbol + ".Name",
                    spec.DisplayName),
                LocalizationService.Create(spec.Symbol + ".Description",
                    description),
                LocalizationService.Create(spec.Symbol + ".Flavor",
                    "A named curved blade carried along the specialist eastern trade routes."),
                spec.FinalCost, definition.WeightPounds);
            _enchantments.SetValue(item, enchantments == null
                ? Array.Empty<BlueprintWeaponEnchantment>()
                : enchantments.ToArray());
            _overrideDamageType.SetValue(item, spec.ColdIron);
            _damageType.SetValue(item,
                EasternWeaponTypeAccess.Physical(definition, spec.ColdIron));
            _visualParameters.SetValue(item, item.Type.VisualParameters);
        }

        internal void ValidateNamed(BlueprintItemWeapon item,
            EasternWeaponNamedSpec spec,
            BlueprintWeaponEnchantment[] expectedEnchantments)
        {
            CustomWeaponCategoryDefinition definition =
                EasternWeaponCatalog.RequireCategory(spec.Family);
            BlueprintWeaponEnchantment[] enchantments =
                (BlueprintWeaponEnchantment[])_enchantments.GetValue(item) ??
                Array.Empty<BlueprintWeaponEnchantment>();
            DamageTypeDescription damage =
                (DamageTypeDescription)_damageType.GetValue(item);
            if (item.Cost != spec.FinalCost ||
                !item.Weight.Equals((float)definition.WeightPounds) ||
                item.IsActuallyStackable ||
                (bool)_overrideDamageType.GetValue(item) != spec.ColdIron ||
                damage == null || damage.Type != DamageType.Physical ||
                damage.Physical.Form != EasternWeaponTypeAccess.Forms(definition) ||
                damage.Physical.Material != (spec.ColdIron
                    ? PhysicalDamageMaterial.ColdIron : 0) ||
                !enchantments.SequenceEqual(expectedEnchantments ??
                    Array.Empty<BlueprintWeaponEnchantment>()) ||
                enchantments.Any(value => value == null) ||
                !ReferenceEquals(_visualParameters.GetValue(item),
                    item.VisualParameters) ||
                !Assets.EasternWeaponAssetRuntime
                    .HasApprovedVisualOrNativeFallback(item, spec.Symbol) ||
                item.Description.IndexOf("Brace",
                    StringComparison.OrdinalIgnoreCase) >= 0)
                throw new InvalidOperationException(
                    "Eastern named item is invalid: " + spec.DisplayName + ".");
        }

        private static FieldInfo Require(string name)
        {
            FieldInfo field = typeof(BlueprintItemWeapon).GetField(name, Fields);
            if (field == null) throw new MissingFieldException(
                typeof(BlueprintItemWeapon).FullName, name);
            return field;
        }

        private static FieldInfo RequireRecursive(string name)
        {
            for (Type type = typeof(BlueprintItemWeapon); type != null;
                type = type.BaseType)
            {
                FieldInfo field = type.GetField(name, Fields |
                    BindingFlags.DeclaredOnly);
                if (field != null) return field;
            }
            throw new MissingFieldException(
                typeof(BlueprintItemWeapon).FullName, name);
        }
    }

    internal sealed class EasternWeaponBlueprintEntry
    {
        internal EasternWeaponBlueprintEntry(EasternWeaponGenericSpec spec,
            BlueprintItemWeapon item)
        { Spec = spec; Item = item; }
        internal EasternWeaponGenericSpec Spec { get; private set; }
        internal BlueprintItemWeapon Item { get; private set; }
    }

    internal sealed class EasternWeaponFamilyBlueprintSet
    {
        internal EasternWeaponFamilyBlueprintSet(EasternWeaponFamily family,
            BlueprintWeaponType weaponType, EasternWeaponBlueprintEntry[] entries)
        { Family = family; WeaponType = weaponType; Entries = entries; }
        internal EasternWeaponFamily Family { get; private set; }
        internal BlueprintWeaponType WeaponType { get; private set; }
        internal EasternWeaponBlueprintEntry[] Entries { get; private set; }
    }

    internal sealed class EasternWeaponBlueprintSet
    {
        internal EasternWeaponBlueprintSet(EasternWeaponFamilyBlueprintSet[] families,
            BlueprintWeaponEnchantment proficiencyPolicy,
            BlueprintFeature wakizashiProficiency,
            BlueprintFeature katanaProficiency,
            BlueprintFeature wakizashiFinesseTraining,
            HeirloomNodachiBlueprintSet heirloomNodachi,
            EasternWeaponSelectorPublication publication)
        {
            Families = families ?? throw new ArgumentNullException("families");
            ProficiencyPolicy = proficiencyPolicy ??
                throw new ArgumentNullException("proficiencyPolicy");
            WakizashiProficiency = wakizashiProficiency ??
                throw new ArgumentNullException("wakizashiProficiency");
            KatanaProficiency = katanaProficiency ??
                throw new ArgumentNullException("katanaProficiency");
            WakizashiFinesseTraining = wakizashiFinesseTraining ??
                throw new ArgumentNullException("wakizashiFinesseTraining");
            HeirloomNodachi = heirloomNodachi ??
                throw new ArgumentNullException("heirloomNodachi");
            Publication = publication ?? throw new ArgumentNullException(
                "publication");
        }
        internal EasternWeaponFamilyBlueprintSet[] Families { get; private set; }
        internal BlueprintWeaponEnchantment ProficiencyPolicy { get; private set; }
        internal BlueprintFeature WakizashiProficiency { get; private set; }
        internal BlueprintFeature KatanaProficiency { get; private set; }
        internal BlueprintFeature WakizashiFinesseTraining { get; private set; }
        internal HeirloomNodachiBlueprintSet HeirloomNodachi
        { get; private set; }
        internal EasternWeaponSelectorPublication Publication { get; private set; }
        internal EasternWeaponNamedBlueprintSet Named { get; private set; }
        internal EasternWeaponCampaignPublication Campaign { get; private set; }
        internal EasternWeaponBlueprintEntry[] Entries
        { get { return Families.SelectMany(value => value.Entries).ToArray(); } }
        internal EasternWeaponFamilyBlueprintSet Require(EasternWeaponFamily family)
        { return Families.Single(value => value.Family == family); }
        internal EasternWeaponBlueprintEntry Require(EasternWeaponFamily family,
            EasternWeaponGenericKind kind)
        { return Require(family).Entries.Single(value => value.Spec.Kind == kind); }
        internal void AttachNamed(EasternWeaponNamedBlueprintSet named)
        {
            if (Named != null || named == null)
                throw new InvalidOperationException(
                    "Eastern named catalog attachment is invalid.");
            Named = named;
        }
        internal void AttachCampaign(EasternWeaponCampaignPublication campaign)
        {
            if (Campaign != null || campaign == null)
                throw new InvalidOperationException(
                    "Eastern campaign publication attachment is invalid.");
            Campaign = campaign;
        }
    }
}
