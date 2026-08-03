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
                "rifle", "revolver", "lead-ball", "black-powder", "repair-kit" };
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
            BasicAmmunitionBlueprintSet ammunition, BlueprintItem repairKit,
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
            ApplyFact(reload, visited); ApplyFact(repair, visited); ApplyFact(overhaul, visited);
            BlueprintItemAccess items = BlueprintItemAccess.Resolve();
            items.SetIcon(firearms.Pistol.Item, Require("early-pistol"));
            items.SetIcon(firearms.Musket.Item, Require("musket"));
            items.SetIcon(firearms.Blunderbuss.Item, Require("blunderbuss"));
            items.SetIcon(firearms.AdvancedRifle.Item, Require("rifle"));
            items.SetIcon(firearms.AdvancedRevolver.Item, Require("revolver"));
            items.SetIcon(ammunition.LeadBall, Require("lead-ball"));
            items.SetIcon(ammunition.BlackPowder, Require("black-powder"));
            items.SetIcon(repairKit, Require("repair-kit"));
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
            foreach (AddFacts add in (fact.ComponentsArray ?? Array.Empty<BlueprintComponent>()).OfType<AddFacts>())
                foreach (BlueprintUnitFact child in add.Facts ?? Array.Empty<BlueprintUnitFact>())
                    ApplyFact(child, visited);
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
