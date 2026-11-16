using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.EasternWeapons;
using KingmakerGunslinger.ElvenBranchedSpear;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Presentation
{
    /// <summary>One firearm item that carries its own approved visual variant.</summary>
    internal sealed class FirearmVisualRosterEntry
    {
        internal FirearmVisualRosterEntry(string symbol, FirearmKind kind,
            string source)
        {
            Symbol = symbol; Kind = kind; Source = source;
        }

        internal string Symbol { get; private set; }
        internal FirearmKind Kind { get; private set; }
        /// <summary>Which declaration the identity comes from.</summary>
        internal string Source { get; private set; }
    }

    /// <summary>
    /// The identities the presentation observers are expected to find, taken
    /// from the declarations that publish them and not from the collections
    /// a running game reports. The observers compare what they see against
    /// these; nothing observed can supply its own expectation.
    ///
    /// Firearm item visuals (16): the items whose builders apply an exact
    /// item visual variant. That is the development Test Musket, the five
    /// production firearms, the eight magic firearms declared by the magic
    /// firearm builder and the two midgame firearms that builder appends from
    /// <see cref="MidgameFirearmCatalog"/>. The count was 14 before the two
    /// midgame firearms were added. The generic progression firearms (+2 to
    /// +5 and Reliable) have visual-catalog mappings as well but are built by
    /// the progression builder, which applies no item variant of its own;
    /// they are not part of this roster.
    ///
    /// Presentation weapons (57): the five production firearms, the ten
    /// magic firearms (eight plus the two midgame), the twelve generic and
    /// eighteen named Eastern weapons and the six generic and six named Elven
    /// branched spears. With the Cord of Stubborn Resolve that is 58 items.
    /// Both were two lower before the midgame firearms.
    ///
    /// Custom weapon enchantments on those weapons and their types (12):
    /// eleven a player can read, and one policy-only enchantment whose name
    /// and description are intentionally blank.
    /// </summary>
    internal static class ProjectItemRosters
    {
        internal const string WeaponType = "BlueprintItemWeapon";
        internal const string BeltType = "BlueprintItemEquipmentBelt";
        internal const string WeaponEnchantmentType = "BlueprintWeaponEnchantment";

        internal const string TestMusketSymbol = "KMG.Test.TestMusketItem";
        internal const string CordSymbol = "KMG.Items.CordOfStubbornResolve";
        internal const string PolicyOnlyEnchantmentSymbol =
            "KMG.EasternWeapons.ProficiencyPolicyEnchantment";

        private static readonly FirearmVisualRosterEntry[] ProductionFirearms =
        {
            new FirearmVisualRosterEntry("KMG.Firearms.EarlyPistolItem",
                FirearmKind.Pistol, "production"),
            new FirearmVisualRosterEntry("KMG.Firearms.EarlyMusketItem",
                FirearmKind.Musket, "production"),
            new FirearmVisualRosterEntry("KMG.Firearms.EarlyBlunderbussItem",
                FirearmKind.Blunderbuss, "production"),
            new FirearmVisualRosterEntry("KMG.Firearms.AdvancedRifleItem",
                FirearmKind.Rifle, "production"),
            new FirearmVisualRosterEntry("KMG.Firearms.AdvancedRevolverItem",
                FirearmKind.Revolver, "production")
        };

        private static readonly FirearmVisualRosterEntry[] DeclaredMagicFirearms =
        {
            new FirearmVisualRosterEntry("KMG.Firearms.PistolPlus1Item",
                FirearmKind.Pistol, "magic"),
            new FirearmVisualRosterEntry("KMG.Firearms.MusketPlus1Item",
                FirearmKind.Musket, "magic"),
            new FirearmVisualRosterEntry("KMG.Firearms.BlunderbussPlus1Item",
                FirearmKind.Blunderbuss, "magic"),
            new FirearmVisualRosterEntry("KMG.Firearms.DuelistsRebuttalItem",
                FirearmKind.Pistol, "magic"),
            new FirearmVisualRosterEntry("KMG.Firearms.RiverKingsMeasureItem",
                FirearmKind.Musket, "magic"),
            new FirearmVisualRosterEntry("KMG.Firearms.IrovettisOvationItem",
                FirearmKind.Blunderbuss, "magic"),
            new FirearmVisualRosterEntry("KMG.Firearms.TheLastWordItem",
                FirearmKind.Pistol, "magic"),
            new FirearmVisualRosterEntry("KMG.Firearms.WatchAtTheWorldsEndItem",
                FirearmKind.Musket, "magic")
        };

        private static readonly string[] Enchantments =
        {
            "KMG.Firearms.ReliableEnchantment",
            "KMG.Enchantments.Seeking",
            "KMG.EasternWeapons.Wakizashi.FallingPetal.EffectEnchantment",
            "KMG.EasternWeapons.Nodachi.MountainSunder.EffectEnchantment",
            "KMG.EasternWeapons.Nodachi.UnfixedForm.EffectEnchantment",
            PolicyOnlyEnchantmentSymbol,
            "KMG.ElvenBranchedSpear.MovementOpportunityAccuracy",
            "KMG.ElvenBranchedSpear.BoughkeeperEnchantment",
            "KMG.ElvenBranchedSpear.ThornstepEnchantment",
            "KMG.ElvenBranchedSpear.VipersReachEnchantment",
            "KMG.ElvenBranchedSpear.BriarCrownedEnchantment",
            "KMG.ElvenBranchedSpear.FirstBranchEnchantment"
        };

        /// <summary>The two midgame firearms, in the magic builder's order.</summary>
        internal static FirearmVisualRosterEntry[] MidgameFirearms
        {
            get
            {
                return MidgameFirearmCatalog.Entries.Select(spec =>
                    new FirearmVisualRosterEntry(spec.Symbol, spec.Kind, "midgame"))
                    .ToArray();
            }
        }

        /// <summary>The magic firearm builder's ten items: eight declared, two midgame.</summary>
        internal static FirearmVisualRosterEntry[] MagicFirearms
        {
            get { return DeclaredMagicFirearms.Concat(MidgameFirearms).ToArray(); }
        }

        /// <summary>Every firearm item that carries its own visual variant.</summary>
        internal static FirearmVisualRosterEntry[] FirearmItemVisuals
        {
            get
            {
                return new[] { new FirearmVisualRosterEntry(TestMusketSymbol,
                        FirearmKind.Musket, "development") }
                    .Concat(ProductionFirearms).Concat(MagicFirearms).ToArray();
            }
        }

        /// <summary>The weapons whose player-facing text the presentation observer reads.</summary>
        internal static string[] PresentationWeapons
        {
            get
            {
                return ProductionFirearms.Select(value => value.Symbol)
                    .Concat(MagicFirearms.Select(value => value.Symbol))
                    .Concat(EasternWeaponCatalog.AllGenericItems.Select(
                        value => value.Symbol))
                    .Concat(EasternWeaponNamedCatalog.All.Select(
                        value => value.Symbol))
                    .Concat(ElvenBranchedSpearCatalog.All.Select(
                        value => value.Symbol))
                    .Concat(ElvenBranchedSpearNamedCatalog.All.Select(
                        value => value.Symbol))
                    .ToArray();
            }
        }

        /// <summary>The presentation weapons and the Cord of Stubborn Resolve.</summary>
        internal static KeyValuePair<string, string>[] PresentationItems
        {
            get
            {
                return PresentationWeapons.Select(symbol =>
                        new KeyValuePair<string, string>(symbol, WeaponType))
                    .Concat(new[] { new KeyValuePair<string, string>(
                        CordSymbol, BeltType) }).ToArray();
            }
        }

        /// <summary>The custom weapon enchantments those weapons and their types carry.</summary>
        internal static string[] CustomWeaponEnchantments
        {
            get { return (string[])Enchantments.Clone(); }
        }
    }
}
