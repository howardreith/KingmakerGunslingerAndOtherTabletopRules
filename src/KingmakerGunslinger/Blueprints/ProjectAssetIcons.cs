using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Gunsmithing;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static class ProjectAssetIcons
    {
        private static readonly Dictionary<string, Sprite> Icons =
            new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);

        internal static void Load(ModContext context)
        {
            if (context == null || context.ModEntry == null)
                throw new ArgumentNullException("context");
            Icons.Clear();
            string directory = Path.Combine(context.ModEntry.Path, "assets", "icons");
            string[] names = { "gunslinger-class", "firearm-proficiency",
                "gunsmithing", "grit", "deeds", "nimble", "bonus-feat",
                "gun-training", "true-grit", "rapid-reload",
                "weapon-focus-firearm", "deadeye", "gunslingers-dodge",
                "quick-clear", "reload-firearm", "repair-firearm",
                "overhaul-firearm", "early-pistol", "musket", "blunderbuss",
                "rifle", "revolver", "lead-ball", "black-powder", "repair-kit",
                "gunsmith-kit", "overhaul-kit", "paper-cartridge", "focused-aim",
                "cord-of-stubborn-resolve", "shield-other" };
            names = names.Concat(new[] { "elven-branched-spear" }).ToArray();
            foreach (string name in names)
            {
                string path = Path.Combine(directory, name + ".png");
                if (!File.Exists(path))
                    throw new FileNotFoundException("Required project icon is missing.", path);
                byte[] bytes = File.ReadAllBytes(path);
                var texture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                if (!LoadImage(texture, bytes))
                    throw new InvalidOperationException("Project icon could not be decoded: " + name);
                texture.name = "KMG_Icon_" + name;
                Sprite sprite = Sprite.Create(texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f), 100f);
                sprite.name = "KMG_Icon_" + name;
                Icons.Add(name, sprite);
            }
        }

        private static bool LoadImage(Texture2D texture, byte[] bytes)
        {
            Type imageConversion = Type.GetType(
                "UnityEngine.ImageConversion, UnityEngine.ImageConversionModule", false);
            MethodInfo method = imageConversion == null ? null : imageConversion.GetMethod(
                "LoadImage", BindingFlags.Public | BindingFlags.Static, null,
                new[] { typeof(Texture2D), typeof(byte[]), typeof(bool) }, null);
            if (method == null)
                throw new MissingMethodException(
                    "Kingmaker Unity runtime does not expose ImageConversion.LoadImage(Texture2D, byte[], bool).");
            return (bool)method.Invoke(null, new object[] { texture, bytes, false });
        }

        internal static void Apply(GunslingerClassBlueprintSet gunslinger,
            FirearmFeatBlueprintSet feats, ProductionFirearmBlueprintCatalog firearms,
            MagicFirearmBlueprintCatalog magicFirearms,
            BasicAmmunitionBlueprintSet ammunition, BlueprintItem repairKit,
            GunsmithingSupplyBlueprintSet supplies,
            PaperCartridgeModeBlueprintSet paperCartridgeMode,
            AcadamaeGraduateModeBlueprintSet acadamaeGraduateMode,
            BlueprintItem cordOfStubbornResolve,
            ElvenBranchedSpearBlueprintSet elvenBranchedSpears,
            BlueprintAbility reload, BlueprintAbility repair, BlueprintAbility overhaul)
        {
            if (Icons.Count == 0) throw new InvalidOperationException("Project icons were not loaded.");
            gunslinger.CharacterClass.m_Icon = Require("gunslinger-class");
            var visited = new HashSet<BlueprintUnitFact>();
            ApplyFact(gunslinger.Progression, visited);
            foreach (BlueprintFeature choice in feats.WeaponFocusChoices) ApplyFact(choice, visited);
            foreach (BlueprintFeature choice in feats.RapidReloadChoices) ApplyFact(choice, visited);
            ApplyFact(feats.WeaponFocus, visited);
            ApplyFact(feats.RapidReload, visited);
            ApplyFact(feats.ExoticWeaponProficiency, visited);
            ApplyFact(gunslinger.QuickClear.Feature, visited);
            foreach (BlueprintArchetype archetype in gunslinger.CharacterClass
                .Archetypes ?? Array.Empty<BlueprintArchetype>())
                foreach (LevelEntry entry in archetype == null ||
                    archetype.AddFeatures == null ? Array.Empty<LevelEntry>() :
                    archetype.AddFeatures)
                    foreach (BlueprintFeatureBase feature in entry == null ||
                        entry.Features == null ? new List<BlueprintFeatureBase>() :
                        entry.Features)
                        ApplyFact(feature, visited);
            BlueprintUnitFactAccess.Resolve().SetIcon(
                gunslinger.Dodge.ArmorClassBuff, Require("gunslingers-dodge"));
            ApplyFact(reload, visited); ApplyFact(repair, visited); ApplyFact(overhaul, visited);
            BlueprintItemAccess items = BlueprintItemAccess.Resolve();
            items.SetIcon(firearms.Pistol.Item, Require("early-pistol"));
            items.SetIcon(firearms.Musket.Item, Require("musket"));
            items.SetIcon(firearms.Blunderbuss.Item, Require("blunderbuss"));
            items.SetIcon(firearms.AdvancedRifle.Item, Require("rifle"));
            items.SetIcon(firearms.AdvancedRevolver.Item, Require("revolver"));
            foreach (MagicFirearmBlueprintEntry entry in magicFirearms.Entries)
            {
                string key = entry.Spec.Kind == Firearms.FirearmKind.Pistol ?
                    "early-pistol" : entry.Spec.Kind == Firearms.FirearmKind.Musket ?
                    "musket" : "blunderbuss";
                items.SetIcon(entry.Item, Require(key));
            }
            items.SetIcon(ammunition.LeadBall, Require("lead-ball"));
            items.SetIcon(ammunition.BlackPowder, Require("black-powder"));
            items.SetIcon(ammunition.PaperCartridge, Require("paper-cartridge"));
            BlueprintUnitFactAccess.Resolve().SetIcon(
                paperCartridgeMode.Ability, Require("paper-cartridge"));
            if (acadamaeGraduateMode == null || acadamaeGraduateMode.Ability.Icon == null)
                throw new InvalidOperationException("Acadamae Graduate mode icon is missing.");
            Sprite cordDonorIcon = cordOfStubbornResolve == null ? null :
                cordOfStubbornResolve.Icon;
            Sprite cordIcon = Require("cord-of-stubborn-resolve");
            items.SetIcon(cordOfStubbornResolve, cordIcon);
            if (cordDonorIcon == null || ReferenceEquals(cordDonorIcon, cordIcon) ||
                !ReferenceEquals(cordOfStubbornResolve.Icon, cordIcon) ||
                !string.Equals(cordIcon.name, "KMG_Icon_cord-of-stubborn-resolve",
                    StringComparison.Ordinal))
                throw new InvalidOperationException(
                    "Cord of Stubborn Resolve must use its distinct project-owned icon.");
            items.SetIcon(repairKit, Require("repair-kit"));
            items.SetIcon(supplies.GunsmithKit, Require("gunsmith-kit"));
            items.SetIcon(supplies.OverhaulKit, Require("overhaul-kit"));
            if (elvenBranchedSpears == null || elvenBranchedSpears.Named == null)
                throw new ArgumentNullException("elvenBranchedSpears");
            Sprite spearIcon = Require("elven-branched-spear");
            foreach (ElvenBranchedSpearBlueprintEntry entry in
                elvenBranchedSpears.Entries)
                items.SetIcon(entry.Item, spearIcon);
            foreach (NamedSpearBlueprintEntry entry in
                elvenBranchedSpears.Named.Entries)
                items.SetIcon(entry.Item, spearIcon);
            ValidateDistinctSupplyIcons(ammunition, repairKit, supplies);
        }

        internal static void ValidateSupplyPublication(
            BlueprintRegistry registry, BasicAmmunitionBlueprintSet ammunition,
            BlueprintItem repairKit, GunsmithingSupplyBlueprintSet supplies,
            GunsmithingCraftingBlueprintSet crafting,
            CapitalVendorPublication capitalVendor,
            BeneathStolenLandsVendorPublication btslVendors,
            ModLogger logger)
        {
            if (registry == null || ammunition == null || repairKit == null ||
                supplies == null || crafting == null || capitalVendor == null ||
                btslVendors == null || logger == null)
                throw new ArgumentNullException("Supply icon publication inputs are incomplete.");
            var craft = crafting.Ability.ComponentsArray
                .OfType<CraftBasicAmmunitionAbilityLogic>().Single();
            var mappings = new[]
            {
                new SupplyIconMapping(BasicAmmunitionBlueprints.LeadBallSymbol,
                    ammunition.LeadBall, "lead-ball", craft.LeadBall),
                new SupplyIconMapping(BasicAmmunitionBlueprints.BlackPowderSymbol,
                    ammunition.BlackPowder, "black-powder", craft.BlackPowder),
                new SupplyIconMapping(FirearmRepairKitBlueprints.Symbol,
                    repairKit, "repair-kit", null),
                new SupplyIconMapping(GunsmithingSupplyBlueprints.GunsmithKitSymbol,
                    supplies.GunsmithKit, "gunsmith-kit", craft.GunsmithKit),
                new SupplyIconMapping(GunsmithingSupplyBlueprints.OverhaulKitSymbol,
                    supplies.OverhaulKit, "overhaul-kit", null)
            };
            foreach (SupplyIconMapping mapping in mappings)
            {
                Sprite expected = Require(mapping.IconKey);
                bool capitalExact = capitalVendor.ContainsExact(mapping.Item);
                bool btslExact = btslVendors.ContainsExact(mapping.Item);
                bool craftExact = mapping.CraftingItem == null ||
                    ReferenceEquals(mapping.CraftingItem, mapping.Item);
                if (!ReferenceEquals(mapping.Item.Icon, expected) ||
                    !capitalExact || !btslExact || !craftExact)
                    throw new InvalidOperationException(
                        "Supply icon publication did not preserve exact blueprint identity: " +
                        mapping.Symbol);
                logger.Info("presentation", "supply-icon.published",
                    string.Format(System.Globalization.CultureInfo.InvariantCulture,
                        "name={0};guid={1};iconKey={2};sprite={3};vendorExact={4};craftingExact={5}",
                        mapping.Item.name, registry.ResolveGuid(mapping.Symbol),
                        mapping.IconKey, expected.name, capitalExact && btslExact,
                        craftExact));
            }
        }

        private static void ValidateDistinctSupplyIcons(
            BasicAmmunitionBlueprintSet ammunition, BlueprintItem repairKit,
            GunsmithingSupplyBlueprintSet supplies)
        {
            BlueprintItem[] items = { ammunition.LeadBall, ammunition.BlackPowder,
                ammunition.PaperCartridge,
                repairKit, supplies.GunsmithKit, supplies.OverhaulKit };
            if (items.Any(item => item == null || item.Icon == null) ||
                items.Select(item => item.Icon).Distinct().Count() != items.Length ||
                items.Any(item => ReferenceEquals(item.Icon, ammunition.Source.Icon)))
                throw new InvalidOperationException(
                    "Every Gunslinger supply item must have one distinct non-template icon.");
        }

        private sealed class SupplyIconMapping
        {
            internal SupplyIconMapping(string symbol, BlueprintItem item,
                string iconKey, BlueprintItem craftingItem)
            { Symbol = symbol; Item = item; IconKey = iconKey; CraftingItem = craftingItem; }
            internal string Symbol { get; private set; }
            internal BlueprintItem Item { get; private set; }
            internal string IconKey { get; private set; }
            internal BlueprintItem CraftingItem { get; private set; }
        }

        internal static Sprite RequireIcon(string name)
        {
            return Require(name);
        }

        private static void ApplyFact(BlueprintUnitFact fact,
            HashSet<BlueprintUnitFact> visited)
        {
            if (fact == null || !visited.Add(fact)) return;
            string factName = fact.name ?? string.Empty;
            // Never repaint native blueprints reached through a Gunslinger
            // selection. Native feat identity includes its original icon.
            if (!factName.StartsWith("KMG_", StringComparison.Ordinal)) return;
            string key = Choose(factName);
            BlueprintUnitFactAccess.Resolve().SetIcon(fact, Require(key));
            BlueprintFeatureSelection selection = fact as BlueprintFeatureSelection;
            if (selection != null && selection.AllFeatures != null)
                foreach (BlueprintFeature child in selection.AllFeatures) ApplyFact(child, visited);
            BlueprintProgression progression = fact as BlueprintProgression;
            if (progression != null && progression.LevelEntries != null)
                foreach (LevelEntry entry in progression.LevelEntries)
                    if (entry != null && entry.Features != null)
                        foreach (BlueprintFeatureBase child in entry.Features)
                            ApplyFact(child, visited);
            foreach (BlueprintComponent component in fact.ComponentsArray ??
                Array.Empty<BlueprintComponent>())
                foreach (FieldInfo field in component.GetType().GetFields(
                    BindingFlags.Instance | BindingFlags.Public))
                {
                    object value = field.GetValue(component);
                    BlueprintUnitFact child = value as BlueprintUnitFact;
                    if (child != null) ApplyFact(child, visited);
                    var children = value as IEnumerable<BlueprintUnitFact>;
                    if (children != null)
                        foreach (BlueprintUnitFact nested in children)
                            ApplyFact(nested, visited);
                }
        }

        private static string Choose(string name)
        {
            string value = name.ToLowerInvariant();
            if (value.Contains("progression") || value.EndsWith("_class")) return "gunslinger-class";
            if (value.Contains("proficien")) return "firearm-proficiency";
            if (value.Contains("gunsmith")) return "gunsmithing";
            if (value.Contains("truegrit") || value.Contains("true_grit")) return "true-grit";
            if (value.Contains("grit")) return "grit";
            if (value.Contains("nimble")) return "nimble";
            if (value.Contains("bonus") && value.Contains("feat")) return "bonus-feat";
            if (value.Contains("guntraining") || value.Contains("gun_training")) return "gun-training";
            if (value.Contains("deadeye")) return "deadeye";
            if (value.Contains("dodge")) return "gunslingers-dodge";
            if (value.Contains("quickclear") || value.Contains("quick_clear")) return "quick-clear";
            if (value.Contains("focusedaim") || value.Contains("focused_aim")) return "focused-aim";
            if (value.Contains("rapidreload") || value.Contains("rapid_reload")) return "rapid-reload";
            if (value.Contains("weaponfocus") || value.Contains("weapon_focus")) return "weapon-focus-firearm";
            if (value.Contains("reload")) return "reload-firearm";
            if (value.Contains("repair")) return "repair-firearm";
            if (value.Contains("overhaul")) return "overhaul-firearm";
            if (value.Contains("pistol")) return "early-pistol";
            if (value.Contains("musket")) return "musket";
            if (value.Contains("blunderbuss")) return "blunderbuss";
            if (value.Contains("rifle")) return "rifle";
            if (value.Contains("revolver")) return "revolver";
            return "deeds";
        }

        private static Sprite Require(string name)
        {
            Sprite value;
            if (!Icons.TryGetValue(name, out value))
                throw new InvalidOperationException("Project icon was not loaded: " + name);
            return value;
        }
    }
}
