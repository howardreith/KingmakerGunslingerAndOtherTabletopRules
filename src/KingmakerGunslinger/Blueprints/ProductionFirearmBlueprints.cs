using System;
using System.Globalization;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Weapons;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Blueprints
{
    /// <summary>
    /// Registers the first production early-firearm catalog as isolated native
    /// presentation clones with explicit custom mechanics and identity.
    /// </summary>
    internal static class ProductionFirearmBlueprints
    {
        internal const string PistolWeaponTypeSymbol = "KMG.Firearms.EarlyPistolWeaponType";
        internal const string PistolItemSymbol = "KMG.Firearms.EarlyPistolItem";
        internal const string MusketWeaponTypeSymbol = "KMG.Firearms.EarlyMusketWeaponType";
        internal const string MusketItemSymbol = "KMG.Firearms.EarlyMusketItem";
        internal const string BlunderbussWeaponTypeSymbol = "KMG.Firearms.EarlyBlunderbussWeaponType";
        internal const string BlunderbussItemSymbol = "KMG.Firearms.EarlyBlunderbussItem";
        internal const string AdvancedRifleWeaponTypeSymbol = "KMG.Firearms.AdvancedRifleWeaponType";
        internal const string AdvancedRifleItemSymbol = "KMG.Firearms.AdvancedRifleItem";
        internal const string AdvancedRevolverWeaponTypeSymbol = "KMG.Firearms.AdvancedRevolverWeaponType";
        internal const string AdvancedRevolverItemSymbol = "KMG.Firearms.AdvancedRevolverItem";

        internal const string NativeLightCrossbowWeaponTypeGuid =
            "d525e7a6d8d5aa648a976ac41194b8d0";
        internal const string NativeStandardLightCrossbowItemGuid =
            "511c97c1ea111444aa186b1a58496664";

        private const string MarkerComponentName = "$KMG_FirearmDefinition";
        private const string ProficiencyRestrictionName = "$KMG_FirearmProficiencyRestriction";
        private const string UnavailableRestrictionName = "$KMG_UnavailableUntilScatter";

        internal static ProductionFirearmBlueprintCatalog Register(
            LibraryScriptableObject library,
            BlueprintRegistry registry,
            ModLogger logger,
            BlueprintFeature firearmProficiency,
            FirearmScopedProficiencyBlueprintSet scopedProficiencies)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (registry == null) throw new ArgumentNullException("registry");
            if (logger == null) throw new ArgumentNullException("logger");
            if (firearmProficiency == null) throw new ArgumentNullException("firearmProficiency");
            if (scopedProficiencies == null)
                throw new ArgumentNullException("scopedProficiencies");

            BlueprintWeaponType lightType = BlueprintLibraryLookup.RequireExact<BlueprintWeaponType>(
                library,
                NativeLightCrossbowWeaponTypeGuid,
                "native Light Crossbow weapon type");
            BlueprintItemWeapon lightItem = BlueprintLibraryLookup.RequireExact<BlueprintItemWeapon>(
                library,
                NativeStandardLightCrossbowItemGuid,
                "native Standard Light Crossbow item");
            BlueprintWeaponType heavyType = BlueprintLibraryLookup.RequireExact<BlueprintWeaponType>(
                library,
                TestMusketBlueprints.NativeHeavyCrossbowWeaponTypeGuid,
                "native Heavy Crossbow weapon type");
            BlueprintItemWeapon heavyItem = BlueprintLibraryLookup.RequireExact<BlueprintItemWeapon>(
                library,
                TestMusketBlueprints.NativeStandardHeavyCrossbowItemGuid,
                "native Standard Heavy Crossbow item");

            WeaponBlueprintAccess itemTypeAccess = WeaponBlueprintAccess.Resolve();
            RequireSourceRelation(lightItem, lightType, itemTypeAccess, "Light Crossbow");
            RequireSourceRelation(heavyItem, heavyType, itemTypeAccess, "Heavy Crossbow");

            SourceSnapshot lightBefore = SourceSnapshot.Capture(lightType, lightItem, itemTypeAccess);
            SourceSnapshot heavyBefore = SourceSnapshot.Capture(heavyType, heavyItem, itemTypeAccess);
            WeaponTypeMechanicalAccess mechanicalAccess = WeaponTypeMechanicalAccess.Resolve();
            BlueprintItemAccess itemAccess = BlueprintItemAccess.Resolve();
            BlueprintProjectile firearmProjectile =
                FirearmProjectileBlueprints.Register(registry, lightType);

            ProductionFirearmBlueprintEntry pistol = RegisterOne(
                registry,
                firearmProficiency,
                scopedProficiencies,
                firearmProjectile,
                itemTypeAccess,
                mechanicalAccess,
                itemAccess,
                lightType,
                lightItem,
                ProductionFirearmCatalog.CreatePistol(),
                PistolWeaponTypeSymbol,
                PistolItemSymbol,
                "KMG_EarlyPistol_WeaponType",
                "KMG_EarlyPistol_Item");
            ProductionFirearmBlueprintEntry musket = RegisterOne(
                registry,
                firearmProficiency,
                scopedProficiencies,
                firearmProjectile,
                itemTypeAccess,
                mechanicalAccess,
                itemAccess,
                heavyType,
                heavyItem,
                ProductionFirearmCatalog.CreateMusket(),
                MusketWeaponTypeSymbol,
                MusketItemSymbol,
                "KMG_EarlyMusket_WeaponType",
                "KMG_EarlyMusket_Item");
            ProductionFirearmBlueprintEntry blunderbuss = RegisterOne(
                registry,
                firearmProficiency,
                scopedProficiencies,
                firearmProjectile,
                itemTypeAccess,
                mechanicalAccess,
                itemAccess,
                heavyType,
                heavyItem,
                ProductionFirearmCatalog.CreateBlunderbuss(),
                BlunderbussWeaponTypeSymbol,
                BlunderbussItemSymbol,
                "KMG_EarlyBlunderbuss_WeaponType",
                "KMG_EarlyBlunderbuss_Item");
            ProductionFirearmBlueprintEntry rifle = RegisterOne(
                registry, firearmProficiency, scopedProficiencies, firearmProjectile, itemTypeAccess, mechanicalAccess, itemAccess,
                heavyType, heavyItem, ProductionFirearmCatalog.CreateAdvancedRifle(),
                AdvancedRifleWeaponTypeSymbol, AdvancedRifleItemSymbol,
                "KMG_AdvancedRifle_WeaponType", "KMG_AdvancedRifle_Item");
            ProductionFirearmBlueprintEntry revolver = RegisterOne(
                registry, firearmProficiency, scopedProficiencies, firearmProjectile, itemTypeAccess, mechanicalAccess, itemAccess,
                lightType, lightItem, ProductionFirearmCatalog.CreateAdvancedRevolver(),
                AdvancedRevolverWeaponTypeSymbol, AdvancedRevolverItemSymbol,
                "KMG_AdvancedRevolver_WeaponType", "KMG_AdvancedRevolver_Item");

            var result = new ProductionFirearmBlueprintCatalog(
                pistol, musket, blunderbuss, rifle, revolver);
            Validate(result, itemTypeAccess, mechanicalAccess);
            lightBefore.VerifyUnchanged(lightType, lightItem, itemTypeAccess);
            heavyBefore.VerifyUnchanged(heavyType, heavyItem, itemTypeAccess);

            logger.Info(
                "firearms",
                "production-catalog.ready",
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Registered production Pistol={0}/{1}, Musket={2}/{3}, dual-mode Blunderbuss={4}/{5}, Advanced Rifle={6}/{7}, and Advanced Revolver={8}/{9}; native presentation sources remained unchanged.",
                    registry.ResolveGuid(PistolWeaponTypeSymbol),
                    registry.ResolveGuid(PistolItemSymbol),
                    registry.ResolveGuid(MusketWeaponTypeSymbol),
                    registry.ResolveGuid(MusketItemSymbol),
                    registry.ResolveGuid(BlunderbussWeaponTypeSymbol),
                    registry.ResolveGuid(BlunderbussItemSymbol),
                    registry.ResolveGuid(AdvancedRifleWeaponTypeSymbol),
                    registry.ResolveGuid(AdvancedRifleItemSymbol),
                    registry.ResolveGuid(AdvancedRevolverWeaponTypeSymbol),
                    registry.ResolveGuid(AdvancedRevolverItemSymbol)));
            return result;
        }

        internal static void Validate(
            ProductionFirearmBlueprintCatalog catalog,
            WeaponBlueprintAccess itemTypeAccess,
            WeaponTypeMechanicalAccess mechanicalAccess)
        {
            if (catalog == null) throw new ArgumentNullException("catalog");
            ValidateOne(catalog.Pistol, itemTypeAccess, mechanicalAccess);
            ValidateOne(catalog.Musket, itemTypeAccess, mechanicalAccess);
            ValidateOne(catalog.Blunderbuss, itemTypeAccess, mechanicalAccess);
            ValidateOne(catalog.AdvancedRifle, itemTypeAccess, mechanicalAccess);
            ValidateOne(catalog.AdvancedRevolver, itemTypeAccess, mechanicalAccess);
            ProductionFirearmBlueprintEntry[] entries = catalog.Entries;
            if (entries.Select(entry => entry.WeaponType).Distinct().Count() != entries.Length ||
                entries.Select(entry => entry.Item).Distinct().Count() != entries.Length)
            {
                throw new InvalidOperationException(
                    "Production firearm catalog entries must use distinct item and type instances.");
            }
        }

        private static ProductionFirearmBlueprintEntry RegisterOne(
            BlueprintRegistry registry,
            BlueprintFeature firearmProficiency,
            FirearmScopedProficiencyBlueprintSet scopedProficiencies,
            BlueprintProjectile firearmProjectile,
            WeaponBlueprintAccess itemTypeAccess,
            WeaponTypeMechanicalAccess mechanicalAccess,
            BlueprintItemAccess itemAccess,
            BlueprintWeaponType sourceType,
            BlueprintItemWeapon sourceItem,
            ProductionFirearmWeaponSpec spec,
            string weaponTypeSymbol,
            string itemSymbol,
            string weaponTypeInternalName,
            string itemInternalName)
        {
            string localizationStem = "KMG.Item." + spec.DisplayName.Replace(" ", string.Empty);
            string descriptionText = spec.Definition.IsScatter
                ? "Uses black powder and lead balls. It can fire a lead ball or use Scatter Shot to fire pellets in a 15-foot cone. It can misfire and must be reloaded."
                : "Uses black powder and lead balls. It can misfire and must be reloaded.";
            descriptionText += " " + FirearmPenetrationPresentation.Describe(
                spec.Definition);
            var name = LocalizationService.Create(localizationStem + ".Name", spec.DisplayName);
            var description = LocalizationService.Create(
                localizationStem + ".Description",
                descriptionText);
            var flavor = LocalizationService.Create(
                localizationStem + ".Flavor",
                "Black powder, lead shot, and careful maintenance turn this mechanism into a formidable weapon.");

            BlueprintWeaponType weaponType = registry.Register<BlueprintWeaponType>(
                weaponTypeSymbol,
                delegate
                {
                    BlueprintWeaponType clone = BlueprintCloneService.Clone(
                        sourceType,
                        weaponTypeInternalName);
                    mechanicalAccess.Configure(clone, spec, name, description);
                    AppendMarker(clone, spec.Definition);
                    FirearmWeaponPresentation.Apply(clone, spec.Definition,
                        firearmProjectile);
                    return clone;
                });
            BlueprintItemWeapon item = registry.Register<BlueprintItemWeapon>(
                itemSymbol,
                delegate
                {
                    BlueprintItemWeapon clone = BlueprintCloneService.Clone(
                        sourceItem,
                        itemInternalName);
                    itemTypeAccess.Set(clone, weaponType);
                    itemAccess.ConfigureWeapon(
                        clone,
                        name,
                        description,
                        flavor,
                        spec.CostGold,
                        spec.WeightPounds);
                    FirearmWeaponPresentation.Apply(clone, spec.Definition,
                        firearmProjectile);
                    FirearmWeaponPresentation.ApplyItemVariant(clone, itemSymbol,
                        spec.Definition.Kind);
                    AppendProficiencyRestriction(clone, firearmProficiency,
                        scopedProficiencies, spec.Definition.Kind);
                    if (!spec.IsPlayerFireable)
                    {
                        AppendUnavailableRestriction(clone);
                    }
                    return clone;
                });
            return new ProductionFirearmBlueprintEntry(
                weaponType,
                item,
                spec,
                firearmProficiency,
                scopedProficiencies.OneHanded,
                scopedProficiencies.TwoHanded,
                weaponTypeInternalName,
                itemInternalName,
                name,
                description);
        }

        private static void ValidateOne(
            ProductionFirearmBlueprintEntry entry,
            WeaponBlueprintAccess itemTypeAccess,
            WeaponTypeMechanicalAccess mechanicalAccess)
        {
            if (entry == null) throw new ArgumentNullException("entry");
            if (!string.Equals(entry.WeaponType.name, entry.WeaponTypeInternalName, StringComparison.Ordinal) ||
                !string.Equals(entry.Item.name, entry.ItemInternalName, StringComparison.Ordinal) ||
                !ReferenceEquals(itemTypeAccess.Get(entry.Item), entry.WeaponType))
            {
                throw new InvalidOperationException(
                    "A production firearm has incorrect internal identity or item/type wiring.");
            }

            FirearmDefinitionComponent[] markers =
                (entry.WeaponType.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .OfType<FirearmDefinitionComponent>()
                .ToArray();
            if (markers.Length != 1 ||
                !string.Equals(markers[0].name, MarkerComponentName, StringComparison.Ordinal) ||
                !entry.Spec.Definition.Equals(markers[0].Definition))
            {
                throw new InvalidOperationException(
                    "A production firearm type must contain exactly one canonical marker.");
            }

            FirearmProficiencyRestriction[] restrictions =
                (entry.Item.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .OfType<FirearmProficiencyRestriction>()
                .ToArray();
            if (restrictions.Length != 1 ||
                !string.Equals(restrictions[0].name, ProficiencyRestrictionName, StringComparison.Ordinal) ||
                !ReferenceEquals(restrictions[0].RequiredProficiency, entry.FirearmProficiency) ||
                !ReferenceEquals(restrictions[0].OneHandedProficiency,
                    entry.OneHandedProficiency) ||
                !ReferenceEquals(restrictions[0].TwoHandedProficiency,
                    entry.TwoHandedProficiency) ||
                restrictions[0].FirearmKind != entry.Spec.Definition.Kind)
            {
                throw new InvalidOperationException(
                    "A production firearm item must contain exactly one firearm-proficiency restriction.");
            }

            int unavailableCount =
                (entry.Item.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .OfType<UnavailableProductionFirearmRestriction>()
                .Count();
            if (unavailableCount != (entry.Spec.IsPlayerFireable ? 0 : 1))
            {
                throw new InvalidOperationException(
                    "Production firearm availability restriction does not match its catalog status.");
            }

            if (!string.Equals(entry.Item.Name, entry.Spec.DisplayName, StringComparison.Ordinal) ||
                entry.Item.Cost != entry.Spec.CostGold ||
                !entry.Item.Weight.Equals(entry.Spec.WeightPounds) ||
                entry.Item.IsActuallyStackable)
            {
                throw new InvalidOperationException(
                    "Production firearm item presentation, cost, weight, or stacking is incorrect.");
            }

            mechanicalAccess.Validate(
                entry.WeaponType,
                entry.Spec,
                entry.Name,
                entry.Description);
        }

        private static void AppendMarker(BlueprintWeaponType weaponType, FirearmDefinition definition)
        {
            BlueprintComponent[] components = weaponType.ComponentsArray ?? Array.Empty<BlueprintComponent>();
            if (components.OfType<FirearmDefinitionComponent>().Any())
            {
                throw new InvalidOperationException("A source clone already contains a firearm marker.");
            }
            FirearmDefinitionComponent marker = FirearmDefinitionComponent.Create(definition);
            marker.name = MarkerComponentName;
            weaponType.ComponentsArray = Append(components, marker);
        }

        private static void AppendProficiencyRestriction(
            BlueprintItemWeapon item,
            BlueprintFeature firearmProficiency,
            FirearmScopedProficiencyBlueprintSet scopedProficiencies,
            FirearmKind kind)
        {
            BlueprintComponent[] components = item.ComponentsArray ?? Array.Empty<BlueprintComponent>();
            if (components.OfType<FirearmProficiencyRestriction>().Any())
            {
                throw new InvalidOperationException(
                    "A source item clone already contains a firearm-proficiency restriction.");
            }
            FirearmProficiencyRestriction restriction =
                FirearmProficiencyRestriction.Create(firearmProficiency,
                    scopedProficiencies.OneHanded,
                    scopedProficiencies.TwoHanded, kind);
            restriction.name = ProficiencyRestrictionName;
            item.ComponentsArray = Append(components, restriction);
        }

        private static void AppendUnavailableRestriction(BlueprintItemWeapon item)
        {
            BlueprintComponent[] components = item.ComponentsArray ?? Array.Empty<BlueprintComponent>();
            UnavailableProductionFirearmRestriction restriction =
                UnavailableProductionFirearmRestriction.Create();
            restriction.name = UnavailableRestrictionName;
            item.ComponentsArray = Append(components, restriction);
        }

        private static BlueprintComponent[] Append(
            BlueprintComponent[] components,
            BlueprintComponent component)
        {
            var expanded = new BlueprintComponent[components.Length + 1];
            Array.Copy(components, expanded, components.Length);
            expanded[expanded.Length - 1] = component;
            return expanded;
        }

        private static void RequireSourceRelation(
            BlueprintItemWeapon item,
            BlueprintWeaponType weaponType,
            WeaponBlueprintAccess access,
            string label)
        {
            if (!ReferenceEquals(access.Get(item), weaponType))
            {
                throw new InvalidOperationException(
                    "The native " + label + " item does not reference the expected weapon type.");
            }
        }

        private sealed class SourceSnapshot
        {
            private readonly string _typeName;
            private readonly string _itemName;
            private readonly BlueprintComponent[] _typeComponents;
            private readonly BlueprintComponent[] _itemComponents;
            private readonly BlueprintWeaponType _itemType;

            private SourceSnapshot(
                string typeName,
                string itemName,
                BlueprintComponent[] typeComponents,
                BlueprintComponent[] itemComponents,
                BlueprintWeaponType itemType)
            {
                _typeName = typeName;
                _itemName = itemName;
                _typeComponents = typeComponents;
                _itemComponents = itemComponents;
                _itemType = itemType;
            }

            internal static SourceSnapshot Capture(
                BlueprintWeaponType type,
                BlueprintItemWeapon item,
                WeaponBlueprintAccess access)
            {
                return new SourceSnapshot(
                    type.name,
                    item.name,
                    (type.ComponentsArray ?? Array.Empty<BlueprintComponent>()).ToArray(),
                    (item.ComponentsArray ?? Array.Empty<BlueprintComponent>()).ToArray(),
                    access.Get(item));
            }

            internal void VerifyUnchanged(
                BlueprintWeaponType type,
                BlueprintItemWeapon item,
                WeaponBlueprintAccess access)
            {
                if (!string.Equals(type.name, _typeName, StringComparison.Ordinal) ||
                    !string.Equals(item.name, _itemName, StringComparison.Ordinal) ||
                    !ReferenceEquals(access.Get(item), _itemType) ||
                    !SameReferences(type.ComponentsArray, _typeComponents) ||
                    !SameReferences(item.ComponentsArray, _itemComponents))
                {
                    throw new InvalidOperationException(
                        "A native firearm presentation source changed during production catalog creation.");
                }
            }

            private static bool SameReferences(
                BlueprintComponent[] actual,
                BlueprintComponent[] expected)
            {
                actual = actual ?? Array.Empty<BlueprintComponent>();
                if (actual.Length != expected.Length) return false;
                for (int index = 0; index < actual.Length; index++)
                {
                    if (!ReferenceEquals(actual[index], expected[index])) return false;
                }
                return true;
            }
        }
    }

    internal sealed class ProductionFirearmBlueprintCatalog
    {
        internal ProductionFirearmBlueprintCatalog(
            ProductionFirearmBlueprintEntry pistol,
            ProductionFirearmBlueprintEntry musket,
            ProductionFirearmBlueprintEntry blunderbuss,
            ProductionFirearmBlueprintEntry advancedRifle,
            ProductionFirearmBlueprintEntry advancedRevolver)
        {
            Pistol = pistol ?? throw new ArgumentNullException("pistol");
            Musket = musket ?? throw new ArgumentNullException("musket");
            Blunderbuss = blunderbuss ?? throw new ArgumentNullException("blunderbuss");
            AdvancedRifle = advancedRifle ?? throw new ArgumentNullException("advancedRifle");
            AdvancedRevolver = advancedRevolver ?? throw new ArgumentNullException("advancedRevolver");
        }

        internal ProductionFirearmBlueprintEntry Pistol { get; private set; }
        internal ProductionFirearmBlueprintEntry Musket { get; private set; }
        internal ProductionFirearmBlueprintEntry Blunderbuss { get; private set; }
        internal ProductionFirearmBlueprintEntry AdvancedRifle { get; private set; }
        internal ProductionFirearmBlueprintEntry AdvancedRevolver { get; private set; }
        internal ProductionFirearmBlueprintEntry[] Entries
        {
            get { return new[] { Pistol, Musket, Blunderbuss, AdvancedRifle, AdvancedRevolver }; }
        }
        internal int Count { get { return 10; } }
    }

    internal sealed class ProductionFirearmBlueprintEntry
    {
        internal ProductionFirearmBlueprintEntry(
            BlueprintWeaponType weaponType,
            BlueprintItemWeapon item,
            ProductionFirearmWeaponSpec spec,
            BlueprintFeature firearmProficiency,
            BlueprintFeature oneHandedProficiency,
            BlueprintFeature twoHandedProficiency,
            string weaponTypeInternalName,
            string itemInternalName,
            Kingmaker.Localization.LocalizedString name,
            Kingmaker.Localization.LocalizedString description)
        {
            WeaponType = weaponType ?? throw new ArgumentNullException("weaponType");
            Item = item ?? throw new ArgumentNullException("item");
            Spec = spec ?? throw new ArgumentNullException("spec");
            FirearmProficiency = firearmProficiency ?? throw new ArgumentNullException("firearmProficiency");
            OneHandedProficiency = oneHandedProficiency ??
                throw new ArgumentNullException("oneHandedProficiency");
            TwoHandedProficiency = twoHandedProficiency ??
                throw new ArgumentNullException("twoHandedProficiency");
            WeaponTypeInternalName = weaponTypeInternalName ?? throw new ArgumentNullException("weaponTypeInternalName");
            ItemInternalName = itemInternalName ?? throw new ArgumentNullException("itemInternalName");
            Name = name ?? throw new ArgumentNullException("name");
            Description = description ?? throw new ArgumentNullException("description");
        }

        internal BlueprintWeaponType WeaponType { get; private set; }
        internal BlueprintItemWeapon Item { get; private set; }
        internal ProductionFirearmWeaponSpec Spec { get; private set; }
        internal BlueprintFeature FirearmProficiency { get; private set; }
        internal BlueprintFeature OneHandedProficiency { get; private set; }
        internal BlueprintFeature TwoHandedProficiency { get; private set; }
        internal string WeaponTypeInternalName { get; private set; }
        internal string ItemInternalName { get; private set; }
        internal Kingmaker.Localization.LocalizedString Name { get; private set; }
        internal Kingmaker.Localization.LocalizedString Description { get; private set; }
    }
}
