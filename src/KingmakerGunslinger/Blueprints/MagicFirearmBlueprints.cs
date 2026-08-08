using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Blueprints
{
    internal static class MagicFirearmBlueprints
    {
        internal const string PistolPlus1Symbol = "KMG.Firearms.PistolPlus1Item";
        internal const string MusketPlus1Symbol = "KMG.Firearms.MusketPlus1Item";
        internal const string BlunderbussPlus1Symbol = "KMG.Firearms.BlunderbussPlus1Item";
        internal const string DuelistsRebuttalSymbol = "KMG.Firearms.DuelistsRebuttalItem";
        internal const string RiverKingsMeasureSymbol = "KMG.Firearms.RiverKingsMeasureItem";
        internal const string IrovettisOvationSymbol = "KMG.Firearms.IrovettisOvationItem";
        internal const string TheLastWordSymbol = "KMG.Firearms.TheLastWordItem";
        internal const string WatchAtWorldsEndSymbol = "KMG.Firearms.WatchAtTheWorldsEndItem";

        internal const string Enhancement1Guid = "d42fc23b92c640846ac137dc26e000d4";
        internal const string Enhancement2Guid = "eb2faccc4c9487d43b3575d7e77ff3f5";
        internal const string Enhancement4Guid = "783d7d496da6ac44f9511011fc5f1979";
        internal const string Enhancement5Guid = "bdba267e951851449af552aa9f9e3992";
        internal const string FeyBaneGuid = "b6948040cdb601242884744a543050d4";
        private const BindingFlags Fields = BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic;

        internal static MagicFirearmBlueprintCatalog Register(
            LibraryScriptableObject library, BlueprintRegistry registry,
            ProductionFirearmBlueprintCatalog firearms,
            BlueprintWeaponEnchantment reliable,
            BlueprintWeaponEnchantment seeking, ModLogger logger)
        {
            if (library == null || registry == null || firearms == null ||
                reliable == null || seeking == null || logger == null)
                throw new ArgumentNullException("Magic firearm registration inputs are incomplete.");
            BlueprintWeaponEnchantment plus1 = Native(library, Enhancement1Guid, "+1");
            BlueprintWeaponEnchantment plus2 = Native(library, Enhancement2Guid, "+2");
            BlueprintWeaponEnchantment plus4 = Native(library, Enhancement4Guid, "+4");
            BlueprintWeaponEnchantment plus5 = Native(library, Enhancement5Guid, "+5");
            BlueprintWeaponEnchantment feyBane = Native(library, FeyBaneGuid, "Fey Bane");

            MagicFirearmItemSpec[] specs =
            {
                new MagicFirearmItemSpec(PistolPlus1Symbol, "KMG_PistolPlus1_Item",
                    "Pistol +1", FirearmKind.Pistol, 3300, 1,
                    "This masterwork pistol bears a +1 enhancement bonus.", "", plus1),
                new MagicFirearmItemSpec(MusketPlus1Symbol, "KMG_MusketPlus1_Item",
                    "Musket +1", FirearmKind.Musket, 3800, 1,
                    "This masterwork musket bears a +1 enhancement bonus.", "", plus1),
                new MagicFirearmItemSpec(BlunderbussPlus1Symbol, "KMG_BlunderbussPlus1_Item",
                    "Blunderbuss +1", FirearmKind.Blunderbuss, 4300, 1,
                    "This masterwork blunderbuss bears a +1 enhancement bonus.", "", plus1),
                new MagicFirearmItemSpec(DuelistsRebuttalSymbol, "KMG_DuelistsRebuttal_Item",
                    "Duelist's Rebuttal", FirearmKind.Pistol, 19300, 3,
                    "+2 Reliable pistol. Reliable reduces this firearm's misfire value by 1 after other increases, to a minimum of 0; a natural 1 remains a miss.",
                    "Fashioned for a foreign duelist who considered a drawn sword an unnecessarily long reply.", plus2, reliable),
                new MagicFirearmItemSpec(RiverKingsMeasureSymbol, "KMG_RiverKingsMeasure_Item",
                    "The River King's Measure", FirearmKind.Musket, 51800, 5,
                    "+4 Reliable musket. Reliable reduces this firearm's misfire value by 1 after other increases, to a minimum of 0; a natural 1 remains a miss.",
                    "Irovetti commissioned the weapon to prove that everything within sight was already within his reach.", plus4, reliable),
                new MagicFirearmItemSpec(IrovettisOvationSymbol, "KMG_IrovettisOvation_Item",
                    "Irovetti's Ovation", FirearmKind.Blunderbuss, 52300, 5,
                    "+4 Reliable blunderbuss. Reliable reduces this firearm's misfire value by 1 after other increases, to a minimum of 0; a natural 1 remains a miss.",
                    "The king demanded applause after every performance. This was the instrument used when the audience proved reluctant.", plus4, reliable),
                new MagicFirearmItemSpec(TheLastWordSymbol, "KMG_TheLastWord_Item",
                    "The Last Word", FirearmKind.Pistol, 99300, 7,
                    "+5 Reliable Seeking pistol. Reliable reduces this firearm's misfire value by 1 after other increases, to a minimum of 0; a natural 1 remains a miss. Seeking ignores concealment miss chances without revealing unseen creatures or bypassing other defenses.",
                    "A weapon made for the end of negotiations—and found at the end of a kingdom.", plus5, reliable, seeking),
                new MagicFirearmItemSpec(WatchAtWorldsEndSymbol, "KMG_WatchAtWorldsEnd_Item",
                    "Watch at the World's End", FirearmKind.Musket, 99800, 7,
                    "+5 Reliable Fey Bane musket. Reliable reduces this firearm's misfire value by 1 after other increases, to a minimum of 0; a natural 1 remains a miss. Fey Bane uses the native bane property against Fey creatures.",
                    "Its first bearer kept vigil at a place where the world grew thin. The watch ended. The weapon remained.", plus5, reliable, feyBane)
            };
            var entries = new List<MagicFirearmBlueprintEntry>();
            foreach (MagicFirearmItemSpec spec in specs)
            {
                ProductionFirearmBlueprintEntry family = Family(firearms, spec.Kind);
                BlueprintItemWeapon source = family.Item;
                BlueprintWeaponEnchantment[] sourceEnchantments = GetEnchantments(source);
                BlueprintItemWeapon item = registry.Register<BlueprintItemWeapon>(spec.Symbol,
                    delegate
                    {
                        BlueprintItemWeapon clone = BlueprintCloneService.Clone(source,
                            spec.InternalName);
                        BlueprintItemAccess.Resolve().ConfigureWeapon(clone,
                            LocalizationService.Create(spec.Symbol + ".Name", spec.DisplayName),
                            LocalizationService.Create(spec.Symbol + ".Description", spec.Description),
                            LocalizationService.Create(spec.Symbol + ".Flavor", spec.Flavor),
                            spec.Cost, source.Weight);
                        SetEnchantments(clone, spec.Enchantments.ToArray());
                        return clone;
                    });
                if (!ReferenceEquals(GetWeaponType(item), family.WeaponType))
                    throw new InvalidOperationException(spec.DisplayName + " changed canonical family weapon type.");
                if (!ReferenceEquals(GetEnchantments(source), sourceEnchantments))
                    throw new InvalidOperationException(spec.DisplayName + " mutated its canonical source enchantment array.");
                entries.Add(new MagicFirearmBlueprintEntry(spec, item, family));
            }
            var result = new MagicFirearmBlueprintCatalog(entries.ToArray(), reliable, seeking);
            Validate(result);
            logger.Info("firearms", "magic-catalog.ready",
                "Registered Reliable plus eight isolated early-firearm magic item blueprints using exact canonical family types and native magic properties.");
            return result;
        }

        internal static void Validate(MagicFirearmBlueprintCatalog catalog)
        {
            if (catalog == null || catalog.Entries.Length != 8 ||
                catalog.Entries.Select(value => value.Item).Distinct().Count() != 8)
                throw new InvalidOperationException("Magic firearm catalog identity/count mismatch.");
            foreach (MagicFirearmBlueprintEntry entry in catalog.Entries)
            {
                BlueprintWeaponEnchantment[] actual = GetEnchantments(entry.Item);
                if (actual.Length != entry.Spec.Enchantments.Length ||
                    actual.Distinct().Count() != actual.Length ||
                    !actual.SequenceEqual(entry.Spec.Enchantments) ||
                    !ReferenceEquals(GetWeaponType(entry.Item), entry.Family.WeaponType) ||
                    entry.Item.Cost != entry.Spec.Cost ||
                    !entry.Item.Weight.Equals(entry.Family.Item.Weight) ||
                    BlueprintItemAccess.Resolve().Capture(entry.Item).IsStackable ||
                    entry.Spec.Kind == FirearmKind.Rifle ||
                    entry.Spec.Kind == FirearmKind.Revolver)
                    throw new InvalidOperationException("Magic firearm contract mismatch: " + entry.Spec.DisplayName);
            }
        }

        private static BlueprintWeaponEnchantment Native(LibraryScriptableObject library,
            string guid, string role)
        {
            BlueprintWeaponEnchantment value = BlueprintLibraryLookup.RequireExact<BlueprintWeaponEnchantment>(
                library, guid, "native " + role + " weapon enchantment");
            if (value.EnchantmentCost <= 0)
                throw new InvalidOperationException("Native " + role + " enchantment has invalid cost metadata.");
            return value;
        }

        private static ProductionFirearmBlueprintEntry Family(
            ProductionFirearmBlueprintCatalog catalog, FirearmKind kind)
        {
            if (kind == FirearmKind.Pistol) return catalog.Pistol;
            if (kind == FirearmKind.Musket) return catalog.Musket;
            if (kind == FirearmKind.Blunderbuss) return catalog.Blunderbuss;
            throw new ArgumentOutOfRangeException("kind");
        }

        private static BlueprintWeaponType GetWeaponType(BlueprintItemWeapon item)
        {
            FieldInfo field = typeof(BlueprintItemWeapon).GetField("m_Type", Fields);
            if (field == null) throw new MissingFieldException(typeof(BlueprintItemWeapon).FullName, "m_Type");
            return field.GetValue(item) as BlueprintWeaponType;
        }

        private static BlueprintWeaponEnchantment[] GetEnchantments(BlueprintItemWeapon item)
        {
            FieldInfo field = typeof(BlueprintItemWeapon).GetField("m_Enchantments", Fields);
            if (field == null || field.FieldType != typeof(BlueprintWeaponEnchantment[]))
                throw new MissingFieldException(typeof(BlueprintItemWeapon).FullName, "m_Enchantments");
            return (BlueprintWeaponEnchantment[])field.GetValue(item) ??
                new BlueprintWeaponEnchantment[0];
        }

        private static void SetEnchantments(BlueprintItemWeapon item,
            BlueprintWeaponEnchantment[] enchantments)
        {
            FieldInfo field = typeof(BlueprintItemWeapon).GetField("m_Enchantments", Fields);
            if (field == null || field.FieldType != typeof(BlueprintWeaponEnchantment[]))
                throw new MissingFieldException(typeof(BlueprintItemWeapon).FullName, "m_Enchantments");
            field.SetValue(item, enchantments == null ?
                new BlueprintWeaponEnchantment[0] : enchantments.ToArray());
            if (!GetEnchantments(item).SequenceEqual(enchantments))
                throw new InvalidOperationException("Static enchantment assignment did not verify.");
        }
    }

    internal sealed class MagicFirearmItemSpec
    {
        internal MagicFirearmItemSpec(string symbol, string internalName,
            string displayName, FirearmKind kind, int cost, int equivalentBonus,
            string description, string flavor,
            params BlueprintWeaponEnchantment[] enchantments)
        {
            Symbol = symbol; InternalName = internalName; DisplayName = displayName;
            Kind = kind; Cost = cost; EquivalentBonus = equivalentBonus;
            Description = description; Flavor = flavor;
            Enchantments = enchantments == null ? new BlueprintWeaponEnchantment[0] :
                enchantments.ToArray();
        }
        internal string Symbol { get; private set; }
        internal string InternalName { get; private set; }
        internal string DisplayName { get; private set; }
        internal FirearmKind Kind { get; private set; }
        internal int Cost { get; private set; }
        internal int EquivalentBonus { get; private set; }
        internal string Description { get; private set; }
        internal string Flavor { get; private set; }
        internal BlueprintWeaponEnchantment[] Enchantments { get; private set; }
    }

    internal sealed class MagicFirearmBlueprintEntry
    {
        internal MagicFirearmBlueprintEntry(MagicFirearmItemSpec spec,
            BlueprintItemWeapon item, ProductionFirearmBlueprintEntry family)
        { Spec = spec; Item = item; Family = family; }
        internal MagicFirearmItemSpec Spec { get; private set; }
        internal BlueprintItemWeapon Item { get; private set; }
        internal ProductionFirearmBlueprintEntry Family { get; private set; }
    }

    internal sealed class MagicFirearmBlueprintCatalog
    {
        internal MagicFirearmBlueprintCatalog(MagicFirearmBlueprintEntry[] entries,
            BlueprintWeaponEnchantment reliable, BlueprintWeaponEnchantment seeking)
        { Entries = entries; Reliable = reliable; Seeking = seeking; }
        internal MagicFirearmBlueprintEntry[] Entries { get; private set; }
        internal BlueprintWeaponEnchantment Reliable { get; private set; }
        internal BlueprintWeaponEnchantment Seeking { get; private set; }
        internal MagicFirearmBlueprintEntry Require(string symbol)
        {
            return Entries.Single(value => value.Spec.Symbol == symbol);
        }
    }
}
