using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
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

        private static readonly DonorSpec[] Donors =
        {
            new DonorSpec(EasternWeaponFamily.Wakizashi,
                "a7da36e0e7bb60e42b9f23462ce2f4fc",
                "57c8994d1f1becf49ac4f642e5d8ca9d"),
            new DonorSpec(EasternWeaponFamily.Katana,
                "d2fe2c5516b56f04da1d5ea51ae3ddfe",
                "7b8a4a452f11022488b1c7bfb0ed7746"),
            new DonorSpec(EasternWeaponFamily.Nodachi,
                "6ddc9acbbb6e40746a6a1671df1f7b47",
                "ef8a8cb62410b8641960e9bd8f24a13f")
        };

        internal static EasternWeaponBlueprintSet Register(
            LibraryScriptableObject library, BlueprintRegistry registry,
            bool presentationEnabled, ModLogger logger)
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
                        typeAdapter.Configure(clone, definition);
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

            var result = new EasternWeaponBlueprintSet(families.ToArray());
            Validate(result, typeAccess, typeAdapter, itemAdapter);
            logger.Info("eastern-weapons", "generic-catalog.ready",
                "Registered three stable categories and twelve generic weapon items; presentation=" +
                    presentationEnabled + ".");
            return result;
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
        }

        private sealed class DonorSpec
        {
            internal DonorSpec(EasternWeaponFamily family, string typeGuid,
                string itemGuid)
            { Family = family; TypeGuid = typeGuid; ItemGuid = itemGuid; }
            internal EasternWeaponFamily Family { get; private set; }
            internal string TypeGuid { get; private set; }
            internal string ItemGuid { get; private set; }
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

        internal void Configure(BlueprintWeaponType type,
            CustomWeaponCategoryDefinition definition)
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
            _enchantments.SetValue(type, Array.Empty<BlueprintWeaponEnchantment>());
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
                type.Enchantments == null || type.Enchantments.Any())
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

        private static PhysicalDamageForm Forms(
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
                return "An exotic light melee weapon that deals piercing or slashing damage and works with native finesse rules.";
            if (definition.Key == "katana")
                return "An exotic one-handed sword that may be wielded in two hands; martial proficiency is sufficient only while it is actually wielded in two hands.";
            return "A martial two-handed sword that deals slashing or piercing damage. It is not a reach weapon and has no brace behavior.";
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

        internal void Configure(BlueprintItemWeapon item,
            CustomWeaponCategoryDefinition definition,
            EasternWeaponGenericSpec spec,
            BlueprintWeaponEnchantment[] enchantments)
        {
            string quality = spec.Enhancement == 1 ? "+1 magic " :
                spec.Masterwork ? "masterwork " : string.Empty;
            string material = spec.ColdIron ? "cold iron " : string.Empty;
            BlueprintItemAccess.Resolve().ConfigureWeapon(item,
                LocalizationService.Create(spec.Symbol + ".Name", spec.DisplayName),
                LocalizationService.Create(spec.Symbol + ".Description",
                    "This " + quality + material +
                    definition.Presentation.DisplayName.ToLowerInvariant() +
                    " uses the family's stable weapon category."),
                LocalizationService.Create(spec.Symbol + ".Flavor",
                    "A carefully proportioned curved blade imported through specialist trade."),
                spec.Cost, definition.WeightPounds);
            _enchantments.SetValue(item, enchantments == null
                ? Array.Empty<BlueprintWeaponEnchantment>()
                : enchantments.ToArray());
            _overrideDamageType.SetValue(item, spec.ColdIron);
            _damageType.SetValue(item,
                EasternWeaponTypeAccess.Physical(definition, spec.ColdIron));
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
                item.Description.IndexOf("Brace",
                    StringComparison.OrdinalIgnoreCase) >= 0)
                throw new InvalidOperationException("Eastern generic item is invalid: " +
                    spec.DisplayName + ".");
        }

        private static FieldInfo Require(string name)
        {
            FieldInfo field = typeof(BlueprintItemWeapon).GetField(name, Fields);
            if (field == null) throw new MissingFieldException(
                typeof(BlueprintItemWeapon).FullName, name);
            return field;
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
        internal EasternWeaponBlueprintSet(EasternWeaponFamilyBlueprintSet[] families)
        { Families = families ?? throw new ArgumentNullException("families"); }
        internal EasternWeaponFamilyBlueprintSet[] Families { get; private set; }
        internal EasternWeaponBlueprintEntry[] Entries
        { get { return Families.SelectMany(value => value.Entries).ToArray(); } }
        internal EasternWeaponFamilyBlueprintSet Require(EasternWeaponFamily family)
        { return Families.Single(value => value.Family == family); }
        internal EasternWeaponBlueprintEntry Require(EasternWeaponFamily family,
            EasternWeaponGenericKind kind)
        { return Require(family).Entries.Single(value => value.Spec.Kind == kind); }
    }
}
