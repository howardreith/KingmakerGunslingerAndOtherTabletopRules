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
    /// Registers a mechanically ordinary heavy-crossbow clone whose custom weapon
    /// type carries the passive firearm marker. A dedicated firearm-
    /// proficiency equipment gate while retaining no ordinary acquisition route,
    /// touch-AC rule, ammunition, reload, or misfire behavior.
    /// </summary>
    internal static class TestMusketBlueprints
    {
        internal const string WeaponTypeSymbol = "KMG.Test.TestMusketWeaponType";
        internal const string ItemSymbol = "KMG.Test.TestMusketItem";

        internal const string NativeHeavyCrossbowWeaponTypeGuid =
            "36d0551b8a28587438a47fcbbf53c083";
        internal const string NativeStandardHeavyCrossbowItemGuid =
            "19a5092244dcf99478dcd73c974828b1";

        internal const string WeaponTypeInternalName = "KMG_TestMusket_WeaponType";
        internal const string ItemInternalName = "KMG_TestMusket_Item";
        private const string MarkerComponentName = "$KMG_FirearmDefinition";
        private const string ProficiencyRestrictionName = "$KMG_FirearmProficiencyRestriction";

        internal static TestMusketBlueprintSet Register(
            LibraryScriptableObject library,
            BlueprintRegistry registry,
            ModLogger logger,
            BlueprintFeature firearmProficiency)
        {
            if (library == null)
            {
                throw new ArgumentNullException("library");
            }

            if (registry == null)
            {
                throw new ArgumentNullException("registry");
            }

            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            if (firearmProficiency == null)
            {
                throw new ArgumentNullException("firearmProficiency");
            }

            BlueprintWeaponType nativeWeaponType =
                BlueprintLibraryLookup.RequireExact<BlueprintWeaponType>(
                    library,
                    NativeHeavyCrossbowWeaponTypeGuid,
                    "native Heavy Crossbow weapon type");
            BlueprintItemWeapon nativeItem =
                BlueprintLibraryLookup.RequireExact<BlueprintItemWeapon>(
                    library,
                    NativeStandardHeavyCrossbowItemGuid,
                    "native Standard Heavy Crossbow item");

            WeaponBlueprintAccess itemTypeAccess = WeaponBlueprintAccess.Resolve();
            if (!ReferenceEquals(itemTypeAccess.Get(nativeItem), nativeWeaponType))
            {
                throw new InvalidOperationException(
                    "The native Standard Heavy Crossbow item does not reference the expected Heavy Crossbow weapon type.");
            }

            SourceSnapshot sourceSnapshot = SourceSnapshot.Capture(
                nativeWeaponType,
                nativeItem,
                itemTypeAccess);
            FirearmDefinition definition = FirearmDefinitions.CreateEarlyMusket();

            BlueprintWeaponType testWeaponType = registry.Register<BlueprintWeaponType>(
                WeaponTypeSymbol,
                delegate
                {
                    BlueprintWeaponType clone = BlueprintCloneService.Clone(
                        nativeWeaponType,
                        WeaponTypeInternalName);
                    AppendMarker(clone, definition);
                    return clone;
                });

            BlueprintItemWeapon testItem = registry.Register<BlueprintItemWeapon>(
                ItemSymbol,
                delegate
                {
                    BlueprintItemWeapon clone = BlueprintCloneService.Clone(
                        nativeItem,
                        ItemInternalName);
                    itemTypeAccess.Set(clone, testWeaponType);
                    AppendProficiencyRestriction(clone, firearmProficiency);
                    return clone;
                });

            var result = new TestMusketBlueprintSet(
                nativeWeaponType,
                nativeItem,
                testWeaponType,
                testItem,
                definition,
                itemTypeAccess.MemberDescription,
                firearmProficiency);
            Validate(result, sourceSnapshot, itemTypeAccess);

            logger.Info(
                "firearms",
                "test-musket.ready",
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Registered Test Musket weaponTypeGuid={0}; itemGuid={1}; sourceTypeGuid={2}; sourceItemGuid={3}; itemTypeMember={4}; proficiency={5}; definition={6}.",
                    registry.ResolveGuid(WeaponTypeSymbol),
                    registry.ResolveGuid(ItemSymbol),
                    NativeHeavyCrossbowWeaponTypeGuid,
                    NativeStandardHeavyCrossbowItemGuid,
                    itemTypeAccess.MemberDescription,
                    firearmProficiency.name,
                    definition));

            return result;
        }

        private static void AppendProficiencyRestriction(
            BlueprintItemWeapon item,
            BlueprintFeature firearmProficiency)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (firearmProficiency == null)
            {
                throw new ArgumentNullException("firearmProficiency");
            }

            BlueprintComponent[] components = item.ComponentsArray ??
                Array.Empty<BlueprintComponent>();
            if (components.OfType<FirearmProficiencyRestriction>().Any())
            {
                throw new InvalidOperationException(
                    "The Heavy Crossbow item clone unexpectedly already contains a firearm-proficiency restriction.");
            }

            FirearmProficiencyRestriction restriction =
                FirearmProficiencyRestriction.Create(firearmProficiency);
            restriction.name = ProficiencyRestrictionName;

            var expanded = new BlueprintComponent[components.Length + 1];
            Array.Copy(components, expanded, components.Length);
            expanded[expanded.Length - 1] = restriction;
            item.ComponentsArray = expanded;
        }

        internal static void Validate(
            TestMusketBlueprintSet set,
            WeaponBlueprintAccess itemTypeAccess)
        {
            if (set == null)
            {
                throw new ArgumentNullException("set");
            }

            if (itemTypeAccess == null)
            {
                throw new ArgumentNullException("itemTypeAccess");
            }

            Validate(set, null, itemTypeAccess);
        }

        private static void AppendMarker(
            BlueprintWeaponType weaponType,
            FirearmDefinition definition)
        {
            if (weaponType == null)
            {
                throw new ArgumentNullException("weaponType");
            }

            BlueprintComponent[] components = weaponType.ComponentsArray ??
                Array.Empty<BlueprintComponent>();
            if (components.OfType<FirearmDefinitionComponent>().Any())
            {
                throw new InvalidOperationException(
                    "The Heavy Crossbow clone unexpectedly already contains a firearm marker.");
            }

            FirearmDefinitionComponent marker = FirearmDefinitionComponent.Create(definition);
            marker.name = MarkerComponentName;

            var expanded = new BlueprintComponent[components.Length + 1];
            Array.Copy(components, expanded, components.Length);
            expanded[expanded.Length - 1] = marker;
            weaponType.ComponentsArray = expanded;
        }

        private static void Validate(
            TestMusketBlueprintSet set,
            SourceSnapshot sourceSnapshot,
            WeaponBlueprintAccess itemTypeAccess)
        {
            if (set.NativeWeaponType == null ||
                set.NativeItem == null ||
                set.WeaponType == null ||
                set.Item == null ||
                set.Definition == null)
            {
                throw new InvalidOperationException("The Test Musket blueprint set is incomplete.");
            }

            if (ReferenceEquals(set.NativeWeaponType, set.WeaponType) ||
                ReferenceEquals(set.NativeItem, set.Item))
            {
                throw new InvalidOperationException("A Test Musket blueprint reused a native source instance.");
            }

            if (!string.Equals(set.WeaponType.name, WeaponTypeInternalName, StringComparison.Ordinal) ||
                !string.Equals(set.Item.name, ItemInternalName, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("A Test Musket blueprint has an unexpected internal name.");
            }

            FirearmDefinitionComponent[] markers =
                (set.WeaponType.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .OfType<FirearmDefinitionComponent>()
                .ToArray();
            if (markers.Length != 1)
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "The Test Musket weapon type must contain exactly one firearm marker; actual={0}.",
                        markers.Length));
            }

            if (!string.Equals(markers[0].name, MarkerComponentName, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("The Test Musket firearm marker has an unstable component name.");
            }

            if (!set.Definition.Equals(markers[0].Definition))
            {
                throw new InvalidOperationException(
                    "The Test Musket firearm marker does not preserve the canonical musket definition.");
            }

            if (!ReferenceEquals(itemTypeAccess.Get(set.Item), set.WeaponType))
            {
                throw new InvalidOperationException(
                    "The Test Musket item does not reference the custom Test Musket weapon type.");
            }

            FirearmProficiencyRestriction[] restrictions =
                (set.Item.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .OfType<FirearmProficiencyRestriction>()
                .ToArray();
            if (restrictions.Length != 1)
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "The Test Musket item must contain exactly one firearm-proficiency restriction; actual={0}.",
                        restrictions.Length));
            }

            if (!string.Equals(
                restrictions[0].name,
                ProficiencyRestrictionName,
                StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "The Test Musket proficiency restriction has an unstable component name.");
            }

            if (!ReferenceEquals(
                restrictions[0].RequiredProficiency,
                set.FirearmProficiency))
            {
                throw new InvalidOperationException(
                    "The Test Musket proficiency restriction references the wrong feature.");
            }

            if (!ReferenceEquals(itemTypeAccess.Get(set.NativeItem), set.NativeWeaponType))
            {
                throw new InvalidOperationException(
                    "The native Heavy Crossbow item/type relation was mutated during Test Musket creation.");
            }

            if (sourceSnapshot != null)
            {
                sourceSnapshot.VerifyUnchanged(
                    set.NativeWeaponType,
                    set.NativeItem,
                    itemTypeAccess);
            }
        }

        private sealed class SourceSnapshot
        {
            private readonly string _weaponTypeName;
            private readonly string _itemName;
            private readonly BlueprintComponent[] _weaponTypeComponents;
            private readonly BlueprintComponent[] _itemComponents;
            private readonly BlueprintWeaponType _itemWeaponType;

            private SourceSnapshot(
                string weaponTypeName,
                string itemName,
                BlueprintComponent[] weaponTypeComponents,
                BlueprintComponent[] itemComponents,
                BlueprintWeaponType itemWeaponType)
            {
                _weaponTypeName = weaponTypeName;
                _itemName = itemName;
                _weaponTypeComponents = weaponTypeComponents;
                _itemComponents = itemComponents;
                _itemWeaponType = itemWeaponType;
            }

            internal static SourceSnapshot Capture(
                BlueprintWeaponType weaponType,
                BlueprintItemWeapon item,
                WeaponBlueprintAccess itemTypeAccess)
            {
                BlueprintComponent[] sourceComponents = weaponType.ComponentsArray ??
                    Array.Empty<BlueprintComponent>();
                var weaponTypeComponentReferences = new BlueprintComponent[sourceComponents.Length];
                Array.Copy(
                    sourceComponents,
                    weaponTypeComponentReferences,
                    sourceComponents.Length);
                BlueprintComponent[] sourceItemComponents = item.ComponentsArray ??
                    Array.Empty<BlueprintComponent>();
                var itemComponentReferences = new BlueprintComponent[sourceItemComponents.Length];
                Array.Copy(
                    sourceItemComponents,
                    itemComponentReferences,
                    sourceItemComponents.Length);
                return new SourceSnapshot(
                    weaponType.name,
                    item.name,
                    weaponTypeComponentReferences,
                    itemComponentReferences,
                    itemTypeAccess.Get(item));
            }

            internal void VerifyUnchanged(
                BlueprintWeaponType weaponType,
                BlueprintItemWeapon item,
                WeaponBlueprintAccess itemTypeAccess)
            {
                if (!string.Equals(weaponType.name, _weaponTypeName, StringComparison.Ordinal) ||
                    !string.Equals(item.name, _itemName, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "A native Heavy Crossbow internal name changed during cloning.");
                }

                BlueprintComponent[] actual = weaponType.ComponentsArray ??
                    Array.Empty<BlueprintComponent>();
                if (actual.Length != _weaponTypeComponents.Length)
                {
                    throw new InvalidOperationException(
                        "The native Heavy Crossbow component count changed during cloning.");
                }

                for (int index = 0; index < actual.Length; index++)
                {
                    if (!ReferenceEquals(actual[index], _weaponTypeComponents[index]))
                    {
                        throw new InvalidOperationException(
                            "A native Heavy Crossbow component reference changed during cloning.");
                    }
                }

                BlueprintComponent[] actualItemComponents = item.ComponentsArray ??
                    Array.Empty<BlueprintComponent>();
                if (actualItemComponents.Length != _itemComponents.Length)
                {
                    throw new InvalidOperationException(
                        "The native Standard Heavy Crossbow item component count changed during cloning.");
                }

                for (int index = 0; index < actualItemComponents.Length; index++)
                {
                    if (!ReferenceEquals(actualItemComponents[index], _itemComponents[index]))
                    {
                        throw new InvalidOperationException(
                            "A native Standard Heavy Crossbow item component reference changed during cloning.");
                    }
                }

                if (!ReferenceEquals(itemTypeAccess.Get(item), _itemWeaponType))
                {
                    throw new InvalidOperationException(
                        "The native Standard Heavy Crossbow item type changed during cloning.");
                }
            }
        }
    }

    internal sealed class TestMusketBlueprintSet
    {
        internal TestMusketBlueprintSet(
            BlueprintWeaponType nativeWeaponType,
            BlueprintItemWeapon nativeItem,
            BlueprintWeaponType weaponType,
            BlueprintItemWeapon item,
            FirearmDefinition definition,
            string itemTypeMember,
            BlueprintFeature firearmProficiency)
        {
            NativeWeaponType = nativeWeaponType ?? throw new ArgumentNullException("nativeWeaponType");
            NativeItem = nativeItem ?? throw new ArgumentNullException("nativeItem");
            WeaponType = weaponType ?? throw new ArgumentNullException("weaponType");
            Item = item ?? throw new ArgumentNullException("item");
            Definition = definition ?? throw new ArgumentNullException("definition");
            ItemTypeMember = itemTypeMember ?? throw new ArgumentNullException("itemTypeMember");
            FirearmProficiency = firearmProficiency ?? throw new ArgumentNullException("firearmProficiency");
        }

        internal BlueprintWeaponType NativeWeaponType { get; private set; }

        internal BlueprintItemWeapon NativeItem { get; private set; }

        internal BlueprintWeaponType WeaponType { get; private set; }

        internal BlueprintItemWeapon Item { get; private set; }

        internal FirearmDefinition Definition { get; private set; }

        internal string ItemTypeMember { get; private set; }

        internal BlueprintFeature FirearmProficiency { get; private set; }
    }
}
