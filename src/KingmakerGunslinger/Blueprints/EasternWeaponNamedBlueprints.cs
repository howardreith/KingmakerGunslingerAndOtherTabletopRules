using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.EasternWeapons;

namespace KingmakerGunslinger.Blueprints
{
    internal static class EasternWeaponNamedBlueprints
    {
        internal const string EnhancementTwoGuid =
            "eb2faccc4c9487d43b3575d7e77ff3f5";
        internal const string EnhancementThreeGuid =
            "80bb8a737579e35498177e1e3c75899b";
        internal const string EnhancementFourGuid =
            "783d7d496da6ac44f9511011fc5f1979";
        internal const string EnhancementFiveGuid =
            "bdba267e951851449af552aa9f9e3992";
        internal const string FlamingGuid =
            "30f90becaaac51f41bf56641966c4121";
        internal const string FrostGuid =
            "421e54078b7719d40915ce0672511d0b";
        internal const string AgileGuid =
            "a36ad92c51789b44fa8a1c5c116a1328";
        internal const string KeenGuid =
            "102a9c8c9b7a75e4fb5844e79deaf4c0";
        internal const string GhostTouchGuid =
            "47857e1a5a3ec1a46adf6491b1423b4f";
        internal const string ShockGuid =
            "7bda5277d36ad114f9f9fd21d0dab658";
        internal const string ThunderingGuid =
            "690e762f7704e1f4aa1ac69ef0ce6a96";
        internal const string HolyGuid =
            "28a9964d81fedae44bae3ca45710c140";
        internal const string BrilliantEnergyGuid =
            "66e9e299c9002ea4bb65b6f300e43770";
        internal const string SpeedGuid =
            "f1c0c50108025d546b2554674ea1c006";

        internal static EasternWeaponNamedBlueprintSet Register(
            LibraryScriptableObject library, BlueprintRegistry registry,
            EasternWeaponBlueprintSet eastern, ModLogger logger)
        {
            if (library == null || registry == null || eastern == null ||
                logger == null) throw new ArgumentNullException(
                    "Eastern named registration inputs are incomplete.");
            Dictionary<string, BlueprintWeaponEnchantment> native =
                LoadNative(library);
            var entries = new List<EasternWeaponNamedBlueprintEntry>();
            var typeAccess = WeaponBlueprintAccess.Resolve();
            var itemAccess = new EasternWeaponItemAccess();
            foreach (EasternWeaponNamedSpec spec in EasternWeaponNamedCatalog.All)
            {
                EasternWeaponFamilyBlueprintSet family = eastern.Require(spec.Family);
                BlueprintItemWeapon donor = family.Entries[0].Item;
                BlueprintWeaponEnchantment[] enchantments = Build(spec, native);
                BlueprintItemWeapon item = registry.Register<BlueprintItemWeapon>(
                    spec.Symbol, delegate
                    {
                        BlueprintItemWeapon clone = BlueprintCloneService.Clone(
                            donor, "KMG_EasternWeapons_" + spec.Kind);
                        typeAccess.Set(clone, family.WeaponType);
                        itemAccess.ConfigureNamed(clone, spec, enchantments,
                            Describe(spec));
                        return clone;
                    });
                itemAccess.ValidateNamed(item, spec, enchantments);
                entries.Add(new EasternWeaponNamedBlueprintEntry(spec, item));
            }
            EasternWeaponNamedBlueprintEntry[] result = entries.ToArray();
            if (result.Length != 18 ||
                result.Select(value => value.Item).Distinct().Count() != 18 ||
                result.Any(value => !ReferenceEquals(typeAccess.Get(value.Item),
                    eastern.Require(value.Spec.Family).WeaponType)))
                throw new InvalidOperationException(
                    "Eastern named item registration is malformed.");
            logger.Info("eastern-weapons", "named-native.ready",
                "Registered all eighteen save-stable named Eastern weapons with exact native enchantment references; bespoke-effect enchantments remain a separate qualified slice.");
            return new EasternWeaponNamedBlueprintSet(result);
        }

        private static Dictionary<string, BlueprintWeaponEnchantment> LoadNative(
            LibraryScriptableObject library)
        {
            string[] guids = {
                EasternWeaponBlueprints.NativeEnhancementOneGuid,
                EnhancementTwoGuid, EnhancementThreeGuid, EnhancementFourGuid,
                EnhancementFiveGuid, FlamingGuid, FrostGuid, AgileGuid, KeenGuid,
                GhostTouchGuid, ShockGuid, ThunderingGuid, HolyGuid,
                BrilliantEnergyGuid, SpeedGuid };
            return guids.ToDictionary(value => value, value =>
                BlueprintLibraryLookup.RequireExact<BlueprintWeaponEnchantment>(
                    library, value, "native Eastern weapon enchantment"));
        }

        private static BlueprintWeaponEnchantment[] Build(
            EasternWeaponNamedSpec spec,
            IDictionary<string, BlueprintWeaponEnchantment> native)
        {
            var result = new List<BlueprintWeaponEnchantment>
            {
                native[spec.Enhancement == 1
                    ? EasternWeaponBlueprints.NativeEnhancementOneGuid
                    : spec.Enhancement == 2 ? EnhancementTwoGuid
                    : spec.Enhancement == 3 ? EnhancementThreeGuid
                    : spec.Enhancement == 4 ? EnhancementFourGuid
                    : EnhancementFiveGuid]
            };
            Add(result, native, spec, EasternWeaponNativeProperty.Flaming,
                FlamingGuid);
            Add(result, native, spec, EasternWeaponNativeProperty.Frost,
                FrostGuid);
            Add(result, native, spec, EasternWeaponNativeProperty.Agile,
                AgileGuid);
            Add(result, native, spec, EasternWeaponNativeProperty.Keen,
                KeenGuid);
            Add(result, native, spec, EasternWeaponNativeProperty.GhostTouch,
                GhostTouchGuid);
            Add(result, native, spec, EasternWeaponNativeProperty.Shock,
                ShockGuid);
            Add(result, native, spec, EasternWeaponNativeProperty.Thundering,
                ThunderingGuid);
            Add(result, native, spec, EasternWeaponNativeProperty.Holy,
                HolyGuid);
            Add(result, native, spec,
                EasternWeaponNativeProperty.BrilliantEnergy,
                BrilliantEnergyGuid);
            Add(result, native, spec, EasternWeaponNativeProperty.Speed,
                SpeedGuid);
            return result.ToArray();
        }

        private static void Add(ICollection<BlueprintWeaponEnchantment> result,
            IDictionary<string, BlueprintWeaponEnchantment> native,
            EasternWeaponNamedSpec spec, EasternWeaponNativeProperty property,
            string guid)
        {
            if (spec.Has(property)) result.Add(native[guid]);
        }

        private static string Describe(EasternWeaponNamedSpec spec)
        {
            var properties = new List<string> { "+" + spec.Enhancement };
            foreach (EasternWeaponNativeProperty property in Enum.GetValues(
                typeof(EasternWeaponNativeProperty)))
                if (property != EasternWeaponNativeProperty.None &&
                    spec.Has(property)) properties.Add(PropertyName(property));
            if (spec.ColdIron) properties.Add("Cold Iron");
            return string.Join(", ", properties.ToArray()) + " " +
                spec.Family + ". It uses the family's single stable weapon type and category.";
        }

        private static string PropertyName(EasternWeaponNativeProperty property)
        {
            return property == EasternWeaponNativeProperty.GhostTouch
                ? "Ghost Touch" :
                property == EasternWeaponNativeProperty.BrilliantEnergy
                ? "Brilliant Energy" : property.ToString();
        }
    }

    internal sealed class EasternWeaponNamedBlueprintEntry
    {
        internal EasternWeaponNamedBlueprintEntry(EasternWeaponNamedSpec spec,
            BlueprintItemWeapon item) { Spec = spec; Item = item; }
        internal EasternWeaponNamedSpec Spec { get; private set; }
        internal BlueprintItemWeapon Item { get; private set; }
    }

    internal sealed class EasternWeaponNamedBlueprintSet
    {
        internal EasternWeaponNamedBlueprintSet(
            EasternWeaponNamedBlueprintEntry[] entries)
        { Entries = entries ?? throw new ArgumentNullException("entries"); }
        internal EasternWeaponNamedBlueprintEntry[] Entries { get; private set; }
        internal EasternWeaponNamedBlueprintEntry Require(
            EasternWeaponNamedKind kind)
        { return Entries.Single(value => value.Spec.Kind == kind); }
    }
}
