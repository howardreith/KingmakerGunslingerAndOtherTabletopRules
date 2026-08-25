using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElvenBranchedSpear;
using KingmakerGunslinger.EasternWeapons;
using KingmakerGunslinger.FeatureModules;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.CraftMagicItemsCompatibility
{
    internal sealed class CraftMagicItemsWeaponRegistration
    {
        internal CraftMagicItemsWeaponRegistration(BlueprintItemWeapon item,
            CraftMagicItemsCatalogEntry policy)
        {
            Item = item ?? throw new ArgumentNullException("item");
            Policy = policy ?? throw new ArgumentNullException("policy");
            if (!string.Equals(item.AssetGuid, policy.Identity,
                    StringComparison.Ordinal))
                throw new InvalidOperationException(
                    "Weapon registration identity does not match its blueprint.");
        }

        internal BlueprintItemWeapon Item { get; private set; }
        internal CraftMagicItemsCatalogEntry Policy { get; private set; }
    }

    internal sealed class CraftMagicItemsAmmunitionRegistration
    {
        internal CraftMagicItemsAmmunitionRegistration(BlueprintItem item,
            CraftMagicItemsAmmunitionRecipePlan plan)
        {
            Item = item ?? throw new ArgumentNullException("item");
            Plan = plan ?? throw new ArgumentNullException("plan");
            if (!string.Equals(item.AssetGuid, plan.Identity,
                    StringComparison.Ordinal) || item.Cost != plan.UnitCost)
                throw new InvalidOperationException(
                    "Ammunition recipe identity or unit cost changed.");
        }

        internal BlueprintItem Item { get; private set; }
        internal CraftMagicItemsAmmunitionRecipePlan Plan { get; private set; }
    }

    internal sealed class CraftMagicItemsRegistrationCatalog
    {
        private CraftMagicItemsRegistrationCatalog(
            CraftMagicItemsWeaponRegistration[] weapons,
            CraftMagicItemsCatalogDecision decision,
            CraftMagicItemsAmmunitionRegistration[] ammunition,
            BlueprintWeaponEnchantment reliable,
            CraftMagicItemsModuleState modules)
        {
            Weapons = weapons;
            Decision = decision;
            Ammunition = ammunition;
            Reliable = reliable;
            Modules = modules;
        }

        internal CraftMagicItemsWeaponRegistration[] Weapons
        { get; private set; }
        internal CraftMagicItemsCatalogDecision Decision { get; private set; }
        internal CraftMagicItemsAmmunitionRegistration[] Ammunition
        { get; private set; }
        internal BlueprintWeaponEnchantment Reliable { get; private set; }
        internal CraftMagicItemsModuleState Modules { get; private set; }

        internal BlueprintItemWeapon[] FirearmCreationBases
        { get { return Resolve(Decision.FirearmCreationBases); } }
        internal BlueprintItemWeapon[] FirearmRecognitionBases
        { get { return Resolve(Decision.FirearmRecognitionBases); } }
        internal BlueprintItemWeapon[] MartialCreationBases
        { get { return Resolve(Decision.MartialBases); } }
        internal BlueprintItemWeapon[] ExoticCreationBases
        { get { return Resolve(Decision.ExoticBases); } }
        internal BlueprintItemWeapon[] NamedUpgradeOnly
        { get { return Resolve(Decision.NamedUpgradeOnly); } }
        internal BlueprintItemWeapon[] AuthoredGenericTargets
        { get { return Resolve(Decision.AuthoredTargets); } }

        internal BlueprintItemWeapon[] CustomFamilyRecognitionBases
        {
            get { return Resolve(Decision.CustomFamilyRecognitionBases); }
        }

        internal static CraftMagicItemsRegistrationCatalog Create(
            FeatureModuleConfiguration active)
        {
            if (active == null) throw new ArgumentNullException("active");
            ProductionFirearmBlueprintCatalog firearms =
                BlueprintBootstrap.ProductionFirearms;
            MagicFirearmBlueprintCatalog magic = BlueprintBootstrap.MagicFirearms;
            EasternWeaponBlueprintSet eastern = BlueprintBootstrap.EasternWeapons;
            ElvenBranchedSpearBlueprintSet spear =
                BlueprintBootstrap.ElvenBranchedSpears;
            BasicAmmunitionBlueprintSet ammunition =
                BlueprintBootstrap.BasicAmmunition;
            if (firearms == null || magic == null || eastern == null ||
                eastern.Named == null || spear == null || spear.Named == null ||
                ammunition == null || magic.Reliable == null)
                throw new InvalidOperationException(
                    "The finalized Gunslinger blueprint catalogs are incomplete.");

            var modules = new CraftMagicItemsModuleState(active.Gunslinger,
                active.EasternWeapons, active.ElvenBranchedSpears);
            var registrations = new List<CraftMagicItemsWeaponRegistration>();
            foreach (ProductionFirearmBlueprintEntry value in firearms.Entries)
            {
                int markerCount = MarkerCount(value.Item);
                bool unavailable = value.Item.ComponentsArray.OfType<
                    UnavailableProductionFirearmRestriction>().Any();
                bool authorized = value.Spec.IsPlayerFireable &&
                    markerCount == 1;
                CraftMagicItemsCatalogRole role = unavailable || !authorized
                    ? CraftMagicItemsCatalogRole.Unavailable
                    : value.Spec.AcquisitionRole ==
                        ProductionFirearmAcquisitionRole
                            .OrdinaryCampaignCraftingBase
                        ? CraftMagicItemsCatalogRole.CanonicalCreationBase
                        : CraftMagicItemsCatalogRole
                            .SupportedRecognitionOnly;
                Add(registrations, value.Item,
                    CraftMagicItemsCatalogFamily.Firearm,
                    role,
                    CraftMagicItemsOwningModule.Gunslinger, authorized,
                    unavailable);
            }
            foreach (MagicFirearmBlueprintEntry value in magic.GenericEntries)
                Add(registrations, value.Item,
                    CraftMagicItemsCatalogFamily.Firearm,
                    CraftMagicItemsCatalogRole.AuthoredGenericTarget,
                    CraftMagicItemsOwningModule.Gunslinger, true, false);
            foreach (MagicFirearmBlueprintEntry value in magic.NamedEntries)
                Add(registrations, value.Item,
                    CraftMagicItemsCatalogFamily.Firearm,
                    CraftMagicItemsCatalogRole.NamedUpgradeOnly,
                    CraftMagicItemsOwningModule.Gunslinger, true, false);

            foreach (EasternWeaponBlueprintEntry value in eastern.Entries)
            {
                CraftMagicItemsCatalogFamily family = Family(value.Spec.Family);
                Add(registrations, value.Item, family,
                    value.Spec.Kind == EasternWeaponGenericKind.Mundane
                        ? CraftMagicItemsCatalogRole.CanonicalCreationBase
                        : CraftMagicItemsCatalogRole.AuthoredGenericTarget,
                    CraftMagicItemsOwningModule.EasternWeapons, true, false);
            }
            foreach (EasternWeaponNamedBlueprintEntry value in
                eastern.Named.Entries)
                Add(registrations, value.Item, Family(value.Spec.Family),
                    CraftMagicItemsCatalogRole.NamedUpgradeOnly,
                    CraftMagicItemsOwningModule.EasternWeapons, true, false);

            foreach (ElvenBranchedSpearBlueprintEntry value in spear.Entries)
                Add(registrations, value.Item,
                    CraftMagicItemsCatalogFamily.ElvenBranchedSpear,
                    value.Spec.Kind == ElvenBranchedSpearItemKind.Mundane
                        ? CraftMagicItemsCatalogRole.CanonicalCreationBase
                        : CraftMagicItemsCatalogRole.AuthoredGenericTarget,
                    CraftMagicItemsOwningModule.ElvenBranchedSpears, true,
                    false);
            foreach (NamedSpearBlueprintEntry value in spear.Named.Entries)
                Add(registrations, value.Item,
                    CraftMagicItemsCatalogFamily.ElvenBranchedSpear,
                    CraftMagicItemsCatalogRole.NamedUpgradeOnly,
                    CraftMagicItemsOwningModule.ElvenBranchedSpears, true,
                    false);

            CraftMagicItemsCatalogDecision decision =
                CraftMagicItemsCompatibilityPolicy.BuildCatalog(
                    registrations.Select(value => value.Policy), modules);
            CraftMagicItemsAmmunitionRegistration[] ammunitionPlans =
                modules.Gunslinger ? new[]
                {
                    Ammo(ammunition.BlackPowder),
                    Ammo(ammunition.LeadBall),
                    Ammo(ammunition.PaperCartridge)
                } : new CraftMagicItemsAmmunitionRegistration[0];
            var result = new CraftMagicItemsRegistrationCatalog(
                registrations.ToArray(), decision, ammunitionPlans,
                magic.Reliable, modules);
            result.Validate();
            return result;
        }

        internal static bool IsFirearm(BlueprintItemWeapon weapon)
        { return MarkerCount(weapon) == 1; }

        internal bool IsNamedUpgradeOnly(BlueprintItemWeapon weapon)
        {
            if (weapon == null || string.IsNullOrWhiteSpace(weapon.AssetGuid))
                return false;
            return Decision.NamedUpgradeOnly.Any(value => string.Equals(
                    value.Identity, weapon.AssetGuid, StringComparison.Ordinal) ||
                weapon.AssetGuid.StartsWith(value.Identity +
                    "#CraftMagicItems", StringComparison.Ordinal));
        }

        private BlueprintItemWeapon[] Resolve(
            IEnumerable<CraftMagicItemsCatalogEntry> entries)
        {
            Dictionary<string, BlueprintItemWeapon> byIdentity = Weapons
                .ToDictionary(value => value.Policy.Identity,
                    value => value.Item, StringComparer.Ordinal);
            return entries.Select(value => byIdentity[value.Identity]).ToArray();
        }

        private void Validate()
        {
            if (Reliable.EnchantmentCost !=
                    CraftMagicItemsCompatibilityPolicy
                        .ReliableEquivalentBonus ||
                Weapons.GroupBy(value => value.Item.AssetGuid,
                    StringComparer.Ordinal).Any(group => group.Count() != 1) ||
                FirearmCreationBases.Any(value => !IsFirearm(value)) ||
                FirearmRecognitionBases.Any(value => !IsFirearm(value)) ||
                FirearmCreationBases.Any(value => !FirearmRecognitionBases
                    .Contains(value)) ||
                NamedUpgradeOnly.Any(value => Decision.AllCreationBases.Any(
                    candidate => string.Equals(candidate.Identity,
                        value.AssetGuid, StringComparison.Ordinal))) ||
                Ammunition.Any(value => value.Plan.Count !=
                    CraftMagicItemsCompatibilityPolicy.AmmunitionBatchCount))
                throw new InvalidOperationException(
                    "The finalized CMI compatibility catalog is malformed.");
        }

        private static void Add(
            ICollection<CraftMagicItemsWeaponRegistration> target,
            BlueprintItemWeapon item, CraftMagicItemsCatalogFamily family,
            CraftMagicItemsCatalogRole role,
            CraftMagicItemsOwningModule module, bool authorized,
            bool unavailable)
        {
            target.Add(new CraftMagicItemsWeaponRegistration(item,
                new CraftMagicItemsCatalogEntry(item.AssetGuid, item.Name,
                    family, role, module, authorized, unavailable)));
        }

        private static CraftMagicItemsAmmunitionRegistration Ammo(
            BlueprintItem item)
        {
            return new CraftMagicItemsAmmunitionRegistration(item,
                new CraftMagicItemsAmmunitionRecipePlan(item.AssetGuid,
                    item.Name, item.Cost,
                    CraftMagicItemsCompatibilityPolicy.AmmunitionBatchCount));
        }

        private static CraftMagicItemsCatalogFamily Family(
            EasternWeaponFamily family)
        {
            return family == EasternWeaponFamily.Wakizashi
                ? CraftMagicItemsCatalogFamily.Wakizashi
                : family == EasternWeaponFamily.Katana
                ? CraftMagicItemsCatalogFamily.Katana
                : CraftMagicItemsCatalogFamily.Nodachi;
        }

        private static int MarkerCount(BlueprintItemWeapon weapon)
        {
            return weapon == null || weapon.Type == null ||
                weapon.Type.ComponentsArray == null ? 0 :
                weapon.Type.ComponentsArray.OfType<
                    FirearmDefinitionComponent>().Count();
        }
    }
}
