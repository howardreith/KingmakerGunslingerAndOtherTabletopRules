using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Items;
using Newtonsoft.Json.Linq;

namespace KmgLoaderCompatHarness
{
    /// <summary>
    /// Craft Magic Items' own upgrade path applied to a Tabletop Rules Expansion firearm: CMI's custom-recipe GUID builder,
    /// its ResourcesLibrary custom-blueprint patches and its CraftItem (which the product hooks to transfer item-owned
    /// firearm state). CMI is reached by reflection; a missing member fails the step. Adapted from the Kingmaker Cutscene
    /// Skip lab harness (CoexistenceOps), with the base item named instead of "first mundane weapon".
    /// </summary>
    internal static class CraftOps
    {
        private const BindingFlags Any = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static Type CmiMain()
        {
            Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "CraftMagicItems");
            if (assembly == null) throw new InvalidOperationException("Craft Magic Items is not loaded");
            return assembly.GetType("CraftMagicItems.Main", true);
        }

        public static ItemEntityWeapon GiveWeapon(string guid)
        {
            BlueprintItemWeapon blueprint = ResourcesLibrary.TryGetBlueprint<BlueprintItemWeapon>(guid);
            if (blueprint == null) throw new InvalidOperationException("no weapon blueprint " + guid);
            ItemEntity added = Game.Instance.Player.Inventory.Add(blueprint);
            if (!(added is ItemEntityWeapon weapon)) throw new InvalidOperationException("inventory add did not yield a weapon for " + guid);
            return weapon;
        }

        /// <summary>The deterministic CMI custom GUID for base + enchantments, from CMI's own builder.</summary>
        public static string CustomGuid(string baseGuid, string enchantmentGuid)
        {
            object patcher = CmiMain().GetField("blueprintPatcher", Any).GetValue(null);
            if (patcher == null) throw new InvalidOperationException("Craft Magic Items has no blueprint patcher (mod not enabled?)");
            MethodInfo build = patcher.GetType().GetMethod("BuildCustomRecipeItemGuid", Any);
            ParameterInfo[] parameters = build.GetParameters();
            object[] arguments = new object[parameters.Length];
            for (int index = 0; index < parameters.Length; index++) arguments[index] = parameters[index].DefaultValue;
            arguments[0] = baseGuid;
            arguments[1] = (IEnumerable<string>)new[] { enchantmentGuid };
            return (string)build.Invoke(patcher, arguments);
        }

        public static BlueprintWeaponEnchantment Enchantment(string name)
        {
            BlueprintWeaponEnchantment enchantment = ResourcesLibrary.GetBlueprints<BlueprintWeaponEnchantment>()
                .FirstOrDefault(candidate => candidate != null && string.Equals(candidate.name, name, StringComparison.Ordinal));
            if (enchantment == null) throw new InvalidOperationException("no weapon enchantment named " + name);
            return enchantment;
        }

        /// <summary>Upgrades the newest inventory copy of the base firearm through CMI's CraftItem. Returns the custom GUID.</summary>
        public static string Upgrade(string baseGuid, string enchantmentName, JObject evidence)
        {
            ItemEntityWeapon weapon = Game.Instance.Player.Inventory.OfType<ItemEntityWeapon>()
                .LastOrDefault(item => item.Blueprint != null && item.Blueprint.AssetGuid == baseGuid);
            if (weapon == null) throw new InvalidOperationException("base firearm " + baseGuid + " is not in the inventory");
            evidence["cmiBaseBefore"] = Describe(weapon);
            BlueprintWeaponEnchantment enchantment = Enchantment(enchantmentName);
            string guid = CustomGuid(baseGuid, enchantment.AssetGuid);
            BlueprintItem custom = ResourcesLibrary.TryGetBlueprint(guid) as BlueprintItem;
            if (custom == null) throw new InvalidOperationException("Craft Magic Items did not resolve the custom blueprint " + guid);
            ItemEntity result = custom.CreateEntity();
            CmiMain().GetMethod("CraftItem", Any).Invoke(null, new object[] { result, weapon });
            evidence["cmiCustomGuid"] = guid;
            evidence["cmiEnchantment"] = enchantment.name + " " + enchantment.AssetGuid;
            return guid;
        }

        /// <summary>
        /// The crafted firearm is in the inventory under the custom GUID, carries the enchantment, keeps the base weapon
        /// type, is classified as a project firearm by the product's CMI catalogue and carries exactly one item-owned
        /// firearm state token. Returns the observation; a violated expectation throws.
        /// </summary>
        public static JObject Verify(string guid, string enchantmentName, string baseGuid, string expectedToken)
        {
            ItemEntityWeapon item = Game.Instance.Player.Inventory.OfType<ItemEntityWeapon>()
                .FirstOrDefault(candidate => candidate?.Blueprint != null && candidate.Blueprint.AssetGuid == guid);
            if (item == null) throw new InvalidOperationException("crafted firearm is not in the inventory: " + guid);
            JObject observed = Describe(item);
            BlueprintWeaponEnchantment enchantment = Enchantment(enchantmentName);
            if (!item.Enchantments.Any(e => e.Blueprint == enchantment))
                throw new InvalidOperationException("crafted firearm lacks " + enchantmentName);
            BlueprintItemWeapon baseBlueprint = ResourcesLibrary.TryGetBlueprint<BlueprintItemWeapon>(baseGuid);
            if (item.Blueprint.Type != baseBlueprint.Type)
                throw new InvalidOperationException("crafted firearm weapon type " + item.Blueprint.Type?.name + " differs from base " + baseBlueprint.Type?.name);

            Assembly product = ProductProbes.Entry().Assembly;
            MethodInfo isFirearm = product.GetType("KingmakerGunslinger.CraftMagicItemsCompatibility.CraftMagicItemsRegistrationCatalog", true)
                .GetMethod("IsFirearm", Any, null, new[] { typeof(BlueprintItemWeapon) }, null);
            if (isFirearm == null) throw new MissingMethodException("CraftMagicItemsRegistrationCatalog.IsFirearm");
            bool firearm = (bool)isFirearm.Invoke(null, new object[] { item.Blueprint });
            observed["productClassifiesAsFirearm"] = firearm;
            if (!firearm) throw new InvalidOperationException("the product does not classify the crafted item as a firearm");

            AssertTokens(observed, expectedToken);
            return observed;
        }

        /// <summary>
        /// "expected" names the exact set of item-owned firearm state token GUIDs (comma separated), or "none" for the
        /// default state, which carries no token. Null skips the check.
        /// </summary>
        public static void AssertTokens(JObject observed, string expected)
        {
            if (expected == null) return;
            string[] want = expected == "none" ? new string[0] : expected.Split(',').Select(v => v.Trim()).Where(v => v.Length > 0).OrderBy(v => v).ToArray();
            string[] have = ((JArray)observed["stateTokens"]).Select(t => (string)t["guid"]).OrderBy(v => v).ToArray();
            if (!want.SequenceEqual(have))
                throw new InvalidOperationException("firearm state tokens [" + string.Join(",", have) + "] differ from expected [" + string.Join(",", want) + "]");
        }

        public static JObject Describe(ItemEntityWeapon item)
        {
            JArray enchantments = new JArray();
            JArray tokens = new JArray();
            foreach (ItemEnchantment enchantment in item.Enchantments)
            {
                BlueprintItemEnchantment blueprint = enchantment.Blueprint;
                enchantments.Add(blueprint.name + " " + blueprint.AssetGuid);
                if (blueprint.ComponentsArray != null && blueprint.ComponentsArray.Any(c => c != null && c.GetType().Name == "FirearmStateTokenComponent"))
                    tokens.Add(new JObject { ["name"] = blueprint.name, ["guid"] = blueprint.AssetGuid });
            }

            return new JObject
            {
                ["blueprint"] = item.Blueprint.name,
                ["guid"] = item.Blueprint.AssetGuid,
                ["weaponType"] = item.Blueprint.Type?.name,
                ["enchantmentValue"] = item.EnchantmentValue,
                ["enchantments"] = enchantments,
                ["stateTokens"] = tokens,
                ["inventoryCount"] = Game.Instance.Player.Inventory.Count(i => i?.Blueprint == item.Blueprint)
            };
        }
    }
}
