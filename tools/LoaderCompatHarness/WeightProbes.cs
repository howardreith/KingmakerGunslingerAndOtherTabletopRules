using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Shields;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Newtonsoft.Json.Linq;
using UnityModManagerNet;

namespace KmgLoaderCompatHarness
{
    /// <summary>
    /// Bag of Tricks No Item Weight compatibility (docs/BAG-OF-TRICKS-NO-ITEM-WEIGHT-COMPATIBILITY.md). Shared readers:
    /// serialized (authored) weights, the unpatched Kingmaker 2.1.7b getter formula computed from those fields, the live
    /// (effective) getters, and the Harmony owners of the six weight getters Bag of Tricks 1.16.4 patches.
    /// </summary>
    internal static class WeightReaders
    {
        private const BindingFlags Fields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly FieldInfo ItemWeight = Require(typeof(BlueprintItem), "m_Weight");
        private static readonly FieldInfo WeaponTypeWeight = Require(typeof(BlueprintWeaponType), "m_Weight");
        private static readonly FieldInfo ItemWeaponType = Require(typeof(BlueprintItemWeapon), "m_Type");
        private static readonly FieldInfo ItemArmorType = Require(typeof(BlueprintItemArmor), "m_Type");
        private static readonly FieldInfo ArmorTypeWeight = Require(typeof(BlueprintArmorType), "m_Weight");
        private static readonly FieldInfo ShieldArmor = Require(typeof(BlueprintItemShield), "m_ArmorComponent");

        private static FieldInfo Require(Type type, string name) => type.GetField(name, Fields) ?? throw new MissingFieldException(type.FullName, name);

        public static float Stored(BlueprintItem item) => (float)ItemWeight.GetValue(item);

        public static float Stored(BlueprintWeaponType type) => (float)WeaponTypeWeight.GetValue(type);

        public static BlueprintWeaponType WeaponType(BlueprintItemWeapon weapon) => (BlueprintWeaponType)ItemWeaponType.GetValue(weapon);

        /// <summary>
        /// What the unpatched 2.1.7b getter returns, from serialized fields: BlueprintItemShield reads its armor
        /// component; BlueprintItemWeapon and BlueprintItemArmor fall back to their type's m_Weight when their own
        /// m_Weight is not above zero; every other item is BlueprintItem.Weight, its m_Weight.
        /// </summary>
        public static float Intended(BlueprintItem item)
        {
            var shield = item as BlueprintItemShield;
            if (shield != null)
            {
                var armor = ShieldArmor.GetValue(shield) as BlueprintItemArmor;
                return armor != null ? Intended(armor) : Stored(item);
            }

            float stored = Stored(item);
            if (stored > 0f) return stored;
            var weapon = item as BlueprintItemWeapon;
            if (weapon != null)
            {
                BlueprintWeaponType type = WeaponType(weapon);
                return type == null ? stored : Stored(type);
            }

            var armorItem = item as BlueprintItemArmor;
            if (armorItem != null)
            {
                var type = (BlueprintArmorType)ItemArmorType.GetValue(armorItem);
                return type == null ? stored : (float)ArmorTypeWeight.GetValue(type);
            }

            return stored;
        }

        public static string Effective(BlueprintItem item)
        {
            try { return Text(item.Weight); }
            catch (Exception exception) { return "<throws " + (exception.InnerException ?? exception).GetType().Name + ">"; }
        }

        public static string Effective(BlueprintWeaponType type)
        {
            try { return Text(type.Weight); }
            catch (Exception exception) { return "<throws " + (exception.InnerException ?? exception).GetType().Name + ">"; }
        }

        public static string Text(float value) => value.ToString("R", CultureInfo.InvariantCulture);

        /// <summary>The six getters Bag of Tricks 1.16.4 declares weight postfixes for, keyed by declaring type.</summary>
        public static IEnumerable<KeyValuePair<string, MethodInfo>> Getters()
        {
            foreach (Type type in new[] { typeof(BlueprintItem), typeof(BlueprintItemWeapon), typeof(BlueprintItemArmor), typeof(BlueprintItemShield),
                typeof(BlueprintWeaponType), typeof(BlueprintArmorType) })
            {
                PropertyInfo property = type.GetProperty("Weight", BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                yield return new KeyValuePair<string, MethodInfo>(type.Name + ".Weight", property?.GetGetMethod());
            }
        }

        public static JObject PatchOwners()
        {
            var result = new JObject();
            HarmonyInstance harmony = SpearRepairConditions.Harmony ?? HarmonyInstance.Create(SpearRepairConditions.HarnessId + ".weight-reader");
            foreach (KeyValuePair<string, MethodInfo> getter in Getters())
            {
                if (getter.Value == null) { result[getter.Key] = "<no declared getter>"; continue; }
                try
                {
                    Patches patches = harmony.GetPatchInfo(getter.Value);
                    result[getter.Key] = patches == null ? "none" : string.Join(",", patches.Prefixes.Concat(patches.Postfixes).Concat(patches.Transpilers)
                        .Select(p => p.owner).Distinct().OrderBy(o => o, StringComparer.Ordinal).ToArray());
                }
                catch (Exception exception) { result[getter.Key] = "unavailable:" + exception.GetType().Name; }
            }

            return result;
        }

        public static string Sha256(string path)
        {
            if (!File.Exists(path)) return null;
            using (var sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
                return string.Concat(sha.ComputeHash(stream).Select(b => b.ToString("x2", CultureInfo.InvariantCulture)).ToArray());
        }

        /// <summary>The deployed product's blueprint manifest: every registered identity, in manifest order.</summary>
        public static JArray ManifestEntries()
        {
            string path = Path.Combine(ProductProbes.Entry().Path, "blueprints", "blueprints.json");
            return (JArray)JObject.Parse(File.ReadAllText(path))["entries"];
        }

        public static BlueprintItem ResolveItem(string reference, JArray manifest)
        {
            string guid = reference.StartsWith("guid:", StringComparison.Ordinal) ? reference.Substring(5)
                : (string)manifest.FirstOrDefault(e => (string)e["symbol"] == reference)?["guid"];
            if (guid == null) throw new InvalidOperationException("no manifest symbol " + reference);
            return ResourcesLibrary.TryGetBlueprint<BlueprintItem>(guid) ?? throw new InvalidOperationException(reference + " (" + guid + ") does not resolve to an item");
        }
    }

    /// <summary>
    /// botWeightSetting. Reads, and for match "on"/"off" sets, Bag of Tricks' own No Item Weight setting
    /// (Settings.toggleItemsWeighZero, compared with Storage.isTrueString/isFalseString exactly as its getter postfixes do)
    /// on the settings object its HarmonyPatches read. save: persist through Bag of Tricks' own ModSettings.Save so a
    /// later fresh process starts with that value. expect: "on", "off" or "absent" (Bag of Tricks not loaded).
    /// </summary>
    internal static class BagOfTricksWeight
    {
        private const BindingFlags Any = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        public const string ModId = "BagOfTricks";

        private static Type BotType(string name) => AppDomain.CurrentDomain.GetAssemblies().Select(a =>
        {
            try { return a.GetType(name, false); } catch (Exception) { return null; }
        }).FirstOrDefault(t => t != null);

        public static JObject Run(string set, bool save, string expect, out string problem)
        {
            var problems = new List<string>();
            var result = new JObject { ["getterPatchOwners"] = WeightReaders.PatchOwners() };
            UnityModManager.ModEntry entry = UnityModManager.modEntries.FirstOrDefault(e => e.Info != null && e.Info.Id == ModId);
            result["present"] = entry != null;
            if (entry == null)
            {
                if (expect != "absent") problems.Add("Bag of Tricks is not installed");
                problem = problems.Count == 0 ? null : string.Join("; ", problems.ToArray());
                return result;
            }

            string dll = Path.Combine(entry.Path, "BagOfTricks.dll");
            result["version"] = entry.Info.Version;
            result["active"] = entry.Active;
            result["loaded"] = entry.Loaded;
            result["dllSha256"] = WeightReaders.Sha256(dll);
            if (expect == "absent") problems.Add("Bag of Tricks is installed");
            Type main = BotType("BagOfTricks.Main"), patches = BotType("BagOfTricks.HarmonyPatches"), storage = BotType("BagOfTricks.Storage");
            if (main == null || patches == null || storage == null)
            {
                problems.Add("Bag of Tricks types are not loaded (Main/HarmonyPatches/Storage)");
                problem = string.Join("; ", problems.ToArray());
                return result;
            }

            object settings = main.GetField("settings", Any)?.GetValue(null);
            object patchSettings = patches.GetField("settings", Any)?.GetValue(null);
            FieldInfo toggle = settings?.GetType().GetField("toggleItemsWeighZero", Any);
            string on = storage.GetField("isTrueString", Any)?.GetValue(null) as string;
            string off = storage.GetField("isFalseString", Any)?.GetValue(null) as string;
            result["sameSettingsObject"] = ReferenceEquals(settings, patchSettings);
            if (settings == null || toggle == null || on == null || off == null)
            {
                problems.Add("Bag of Tricks No Item Weight setting members are unavailable");
                problem = string.Join("; ", problems.ToArray());
                return result;
            }

            if (!ReferenceEquals(settings, patchSettings)) problems.Add("Bag of Tricks' patches read a different settings object than Main.settings");
            Func<object, string> state = target =>
            {
                string value = target == null ? null : toggle.GetValue(target) as string;
                return value == on ? "on" : value == off ? "off" : "invalid";
            };
            result["before"] = state(settings);
            if (set == "on" || set == "off")
            {
                toggle.SetValue(settings, set == "on" ? on : off);
                result["set"] = set;
            }
            else if (!string.IsNullOrEmpty(set)) problems.Add("botWeightSetting match must be on, off or empty, not " + set);

            if (save)
            {
                ((UnityModManager.ModSettings)settings).Save(entry);
                string file = Path.Combine(entry.Path, "Settings.xml");
                string text = File.Exists(file) ? File.ReadAllText(file) : null;
                string expected = "<toggleItemsWeighZero>" + System.Security.SecurityElement.Escape(state(settings) == "on" ? on : off) + "</toggleItemsWeighZero>";
                result["settingsFile"] = new JObject { ["sha256"] = WeightReaders.Sha256(file), ["holdsSavedValue"] = text != null && text.Contains(expected) };
                if (text == null || !text.Contains(expected)) problems.Add("Bag of Tricks Settings.xml does not hold the saved No Item Weight value");
            }

            result["after"] = state(settings);
            result["patchesRead"] = state(patchSettings);
            if ((expect == "on" || expect == "off") && state(patchSettings) != expect)
                problems.Add("Bag of Tricks No Item Weight is " + state(patchSettings) + ", expected " + expect);
            problem = problems.Count == 0 ? null : string.Join("; ", problems.ToArray());
            return result;
        }
    }

    /// <summary>
    /// kmgWeightCensus. Every identity in the deployed product's manifest that resolves to a BlueprintItem (any status:
    /// disabled-publication and legacy identities are registered too), plus the "contains" native comparison GUIDs. For
    /// each: concrete type, stored m_Weight, the unpatched-engine weight from fields, the live getter, and for weapons
    /// the weapon type's stored and live weight. expect "zero": every live getter reads 0; "intended": every live getter
    /// equals the unpatched value. Writes weight-census-&lt;label&gt;.json to the session profile; type
    /// "compare:&lt;label&gt;" requires exactly the same identities with equal stored and unpatched weights as that record
    /// (construction under one setting against construction under the other).
    /// </summary>
    internal static class WeightCensus
    {
        public static JObject Run(string profileDir, string label, string expect, string type, IList<string> natives, out string problem)
        {
            var problems = new List<string>();
            JArray manifest = WeightReaders.ManifestEntries();
            var items = new JObject();
            var unresolvedPlanned = new JArray();
            var byType = new SortedDictionary<string, int>(StringComparer.Ordinal);
            var byStatus = new SortedDictionary<string, int>(StringComparer.Ordinal);
            int plannedItems = 0, naturallyWeightless = 0;
            foreach (JToken entry in manifest)
            {
                string guid = (string)entry["guid"], planned = (string)entry["plannedType"] ?? string.Empty, status = (string)entry["status"] ?? "<none>";
                bool plannedItem = planned.StartsWith("BlueprintItem", StringComparison.Ordinal) && !planned.Contains("Enchantment");
                if (plannedItem) plannedItems++;
                BlueprintItem item = ResourcesLibrary.TryGetBlueprint<BlueprintScriptableObject>(guid) as BlueprintItem;
                if (item == null)
                {
                    if (plannedItem) unresolvedPlanned.Add((string)entry["symbol"] + "/" + status);
                    continue;
                }

                JObject record = Record(item, (string)entry["symbol"], status, expect, problems);
                items[guid] = record;
                if ((float)record["intended"] == 0f) naturallyWeightless++;
                string concrete = item.GetType().Name;
                byType[concrete] = byType.TryGetValue(concrete, out int t) ? t + 1 : 1;
                byStatus[status] = byStatus.TryGetValue(status, out int s) ? s + 1 : 1;
            }

            var nativeRecords = new JObject();
            foreach (string guid in natives ?? new List<string>())
            {
                BlueprintItem item = ResourcesLibrary.TryGetBlueprint<BlueprintScriptableObject>(guid) as BlueprintItem;
                if (item == null) { problems.Add("native comparison item " + guid + " does not resolve"); continue; }
                nativeRecords[guid] = Record(item, "native", "native", expect, problems);
            }

            var result = new JObject
            {
                ["label"] = label, ["expect"] = expect, ["manifestEntries"] = manifest.Count, ["plannedItemEntries"] = plannedItems,
                ["resolvedItems"] = items.Count, ["unresolvedPlannedItems"] = unresolvedPlanned, ["naturallyWeightless"] = naturallyWeightless,
                ["byType"] = Counts(byType), ["byStatus"] = Counts(byStatus),
                ["getterPatchOwners"] = WeightReaders.PatchOwners(), ["items"] = items, ["natives"] = nativeRecords
            };
            if (items.Count == 0) problems.Add("no registered product item resolved");
            string path = Path.Combine(profileDir, "weight-census-" + label + ".json");
            File.WriteAllText(path, result.ToString(Newtonsoft.Json.Formatting.Indented));
            result["recordSha256"] = WeightReaders.Sha256(path);
            if (!string.IsNullOrEmpty(type) && type.StartsWith("compare:", StringComparison.Ordinal))
                result["compared"] = Compare(profileDir, type.Substring(8), items, nativeRecords, problems);

            // The file keeps every record; the harness result keeps the summary and only problem records.
            result["items"] = new JObject(items.Properties().Where(p => p.Value["problem"] != null).Take(40));
            problem = problems.Count == 0 ? null : string.Join("; ", problems.Take(30).ToArray()) + (problems.Count > 30 ? "; (+" + (problems.Count - 30) + " more)" : string.Empty);
            return result;
        }

        private static JObject Record(BlueprintItem item, string symbol, string status, string expect, List<string> problems)
        {
            float stored = WeightReaders.Stored(item), intended = WeightReaders.Intended(item);
            string effective = WeightReaders.Effective(item);
            var record = new JObject
            {
                ["symbol"] = symbol, ["name"] = item.name, ["status"] = status, ["type"] = item.GetType().Name,
                ["stored"] = stored, ["intended"] = intended, ["effective"] = effective
            };
            var local = new List<string>();
            if (float.IsNaN(intended) || float.IsInfinity(intended) || intended < 0f) local.Add("unpatched weight " + intended + " is not finite and nonnegative");
            var weapon = item as BlueprintItemWeapon;
            BlueprintWeaponType weaponType = weapon == null ? null : WeaponReadersType(weapon);
            if (weaponType != null)
            {
                record["weaponType"] = weaponType.name;
                record["weaponTypeStored"] = WeightReaders.Stored(weaponType);
                record["weaponTypeEffective"] = WeightReaders.Effective(weaponType);
            }

            if (expect == "zero")
            {
                if (effective != "0") local.Add("live weight " + effective + ", expected 0");
                if (weaponType != null && (string)record["weaponTypeEffective"] != "0") local.Add("weapon type live weight " + record["weaponTypeEffective"] + ", expected 0");
            }
            else if (expect == "intended")
            {
                if (effective != WeightReaders.Text(intended)) local.Add("live weight " + effective + ", expected " + WeightReaders.Text(intended));
                if (weaponType != null && (string)record["weaponTypeEffective"] != WeightReaders.Text(WeightReaders.Stored(weaponType)))
                    local.Add("weapon type live weight " + record["weaponTypeEffective"] + ", expected " + WeightReaders.Text(WeightReaders.Stored(weaponType)));
            }

            if (local.Count > 0)
            {
                record["problem"] = string.Join("; ", local.ToArray());
                problems.Add(symbol + " " + item.name + ": " + record["problem"]);
            }

            return record;
        }

        // Built by hand: another mod may change Newtonsoft's process-wide defaults (dictionaries as arrays).
        private static JObject Counts(SortedDictionary<string, int> values)
        {
            var result = new JObject();
            foreach (KeyValuePair<string, int> pair in values) result[pair.Key] = pair.Value;
            return result;
        }

        private static BlueprintWeaponType WeaponReadersType(BlueprintItemWeapon weapon) => WeightReaders.WeaponType(weapon);

        private static JObject Compare(string profileDir, string label, JObject items, JObject natives, List<string> problems)
        {
            string path = Path.Combine(profileDir, "weight-census-" + label + ".json");
            if (!File.Exists(path)) { problems.Add("no census record " + label + " to compare with"); return new JObject(); }
            JObject prior = JObject.Parse(File.ReadAllText(path));
            var differences = new JArray();
            int compared = 0;
            foreach (string section in new[] { "items", "natives" })
            {
                JObject before = (JObject)prior[section] ?? new JObject(), now = section == "items" ? items : natives;
                foreach (string guid in before.Properties().Select(p => p.Name).Union(now.Properties().Select(p => p.Name)).Distinct())
                {
                    JToken a = before[guid], b = now[guid];
                    compared++;
                    if (a == null || b == null) { differences.Add(guid + " present only " + (a == null ? "now" : "in " + label)); continue; }
                    // Compared as single-precision values: a float token written this process and the same value read
                    // back from the record as a double (0.1f vs 0.1) are the same authored weight.
                    foreach (string field in new[] { "stored", "intended", "weaponTypeStored" })
                    {
                        JToken x = a[field], y = b[field];
                        bool same = (x == null || x.Type == JTokenType.Null) && (y == null || y.Type == JTokenType.Null) ||
                            x != null && y != null && x.Type != JTokenType.Null && y.Type != JTokenType.Null && ((float)x).Equals((float)y);
                        if (!same) differences.Add((string)b["name"] + " " + field + " " + x + " -> " + y);
                    }
                }
            }

            if (differences.Count > 0) problems.Add(differences.Count + " authored-weight differences against census " + label + ": " +
                string.Join(", ", differences.Take(10).Select(d => (string)d).ToArray()));
            return new JObject { ["against"] = label, ["againstSha256"] = WeightReaders.Sha256(path), ["compared"] = compared, ["differences"] = differences };
        }
    }

    /// <summary>
    /// kmgWeightItems. Real inventory instances. match: references to add to the party inventory ("Symbol*N" or
    /// "guid:hex*N"); invariants.battered: a reference whose added instance is bound as a battered firearm to the main
    /// character (ownership). Every instance this process added or verified is tracked by reference, so later calls prove
    /// the same objects are still held, with the same counts and stored weights (no replacement, no mutation). expect
    /// "zero" or "intended": each tracked instance's Blueprint.Weight, TotalWeight and the inventory tooltip text
    /// (UIUtilityTexts.GetItemWeight), and the party's carried weight (EncumbranceHelper), must follow the setting. type
    /// "write": record the tracked identities beside the session profile (before a save); "verify": in a fresh process
    /// that loaded that save, find every recorded identity by blueprint and count (2.1.7b items carry no unique id) with
    /// its enchantments, battered owner and stored weight, and track it.
    /// </summary>
    internal static class WeightItems
    {
        private const string FileName = "weight-items-fixture.json";
        private static readonly List<ItemEntity> Tracked = new List<ItemEntity>();
        private static readonly Dictionary<ItemEntity, int> FirstCount = new Dictionary<ItemEntity, int>();
        private static readonly Dictionary<ItemEntity, float> FirstStored = new Dictionary<ItemEntity, float>();

        public static JObject Run(string profileDir, string label, string add, string type, string expect, IDictionary<string, string> invariants,
            out string problem)
        {
            var problems = new List<string>();
            var result = new JObject { ["label"] = label };
            JArray manifest = WeightReaders.ManifestEntries();
            UnitEntityData main = Game.Instance.Player.MainCharacter.Value;
            string battered = invariants != null && invariants.TryGetValue("battered", out string b) ? b : null;
            var added = new JArray();
            foreach (string spec in (add ?? string.Empty).Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(v => v.Trim()))
            {
                string[] parts = spec.Split('*');
                int count = parts.Length > 1 ? int.Parse(parts[1], CultureInfo.InvariantCulture) : 1;
                BlueprintItem blueprint = WeightReaders.ResolveItem(parts[0], manifest);
                ItemEntity item = null;
                for (int i = 0; i < count; i++) item = Game.Instance.Player.Inventory.Add(blueprint);
                if (item == null) { problems.Add("inventory add yielded nothing for " + spec); continue; }
                if (parts[0] == battered)
                {
                    var weapon = item as ItemEntityWeapon ?? throw new InvalidOperationException(spec + " is not a weapon instance");
                    ProductItems.BindBattered(weapon, main);
                }

                Track(item);
                added.Add(spec + " -> " + item.Blueprint.name + " x" + item.Count);
            }

            result["added"] = added;
            if (type == "verify") result["verified"] = Verify(profileDir, manifest, problems);

            // The same objects, counts and stored weights as when first tracked.
            var instances = new JArray();
            foreach (ItemEntity item in Tracked)
            {
                bool held = Game.Instance.Player.Inventory.Items.Any(i => ReferenceEquals(i, item));
                float stored = WeightReaders.Stored(item.Blueprint), intended = WeightReaders.Intended(item.Blueprint);
                string blueprintWeight = WeightReaders.Effective(item.Blueprint), total, display;
                try { total = WeightReaders.Text(item.TotalWeight); } catch (Exception e) { total = "<throws " + e.GetType().Name + ">"; }
                try { display = UIUtilityTexts.GetItemWeight(item); } catch (Exception e) { display = "<throws " + e.GetType().Name + ">"; }
                float expectedWeight = expect == "zero" ? 0f : intended;
                string expectedDisplay;
                try
                {
                    expectedDisplay = item.Count == 1 ? UIUtilityTexts.GetItemWeight(expectedWeight)
                        : expectedWeight.ToString("#####0.##") + " (" + (expectedWeight * item.Count).ToString("#####0.##") + ")";
                }
                catch (Exception e) { expectedDisplay = "<throws " + e.GetType().Name + ">"; }

                var record = new JObject
                {
                    ["name"] = item.Blueprint.name, ["guid"] = item.Blueprint.AssetGuid, ["count"] = item.Count, ["held"] = held,
                    ["sameCount"] = item.Count == FirstCount[item], ["stored"] = stored, ["storedUnchanged"] = stored.Equals(FirstStored[item]),
                    ["intended"] = intended, ["blueprintWeight"] = blueprintWeight, ["totalWeight"] = total, ["display"] = display,
                    ["expectedDisplay"] = expectedDisplay
                };
                if (!held) problems.Add(item.Blueprint.name + " instance is no longer held (replaced or removed)");
                if (item.Count != FirstCount[item]) problems.Add(item.Blueprint.name + " count changed " + FirstCount[item] + " -> " + item.Count);
                if (!stored.Equals(FirstStored[item])) problems.Add(item.Blueprint.name + " stored weight changed " + FirstStored[item] + " -> " + stored);
                if (expect == "zero" || expect == "intended")
                {
                    if (blueprintWeight != WeightReaders.Text(expectedWeight)) problems.Add(item.Blueprint.name + " Blueprint.Weight " + blueprintWeight + ", expected " + expectedWeight);
                    if (total != WeightReaders.Text(expectedWeight * item.Count)) problems.Add(item.Blueprint.name + " TotalWeight " + total + ", expected " + expectedWeight * item.Count);
                    bool displayMatches = item.Count == 1 ? display == expectedDisplay : display.StartsWith(expectedDisplay, StringComparison.Ordinal);
                    if (!displayMatches) problems.Add(item.Blueprint.name + " displays " + display + ", expected " + expectedDisplay);
                }

                instances.Add(record);
            }

            result["instances"] = instances;
            result["carried"] = Carried(expect, problems);
            result["botSetting"] = BagOfTricksWeight.Run(null, false, null, out string _)["after"];
            if (type == "write")
            {
                JObject fixture = Fixture(main);
                string path = Path.Combine(profileDir, FileName);
                File.WriteAllText(path, fixture.ToString(Newtonsoft.Json.Formatting.Indented));
                result["fixture"] = fixture;
                result["fixtureSha256"] = WeightReaders.Sha256(path);
            }

            if (Tracked.Count == 0) problems.Add("no tracked inventory instance");
            problem = problems.Count == 0 ? null : string.Join("; ", problems.ToArray());
            return result;
        }

        private static void Track(ItemEntity item)
        {
            if (Tracked.Any(i => ReferenceEquals(i, item))) return;
            Tracked.Add(item);
            FirstCount[item] = item.Count;
            FirstStored[item] = WeightReaders.Stored(item.Blueprint);
        }

        private static JObject Carried(string expect, List<string> problems)
        {
            Player player = Game.Instance.Player;
            float current = EncumbranceHelper.GetPartyCarryingCapacity().CurrentWeight;
            // The same owner filter EncumbranceHelper applies (party and detached members), over unpatched weights.
            float unpatched = 0f;
            foreach (ItemEntity item in player.Inventory.Items)
            {
                if (item.Owner != null && !player.Party.Contains(item.Owner.Unit) && !player.DetachedPartyCharacters.Any(r => r.Value == item.Owner.Unit)) continue;
                unpatched += item.Count * WeightReaders.Intended(item.Blueprint);
            }

            var result = new JObject { ["currentWeight"] = current, ["unpatchedSum"] = unpatched, ["inventoryEntries"] = player.Inventory.Items.Count() };
            if (expect == "zero" && current != 0f) problems.Add("party carried weight " + current + ", expected 0");
            if (expect == "intended" && Math.Abs(current - unpatched) > 0.01f) problems.Add("party carried weight " + current + ", expected the unpatched sum " + unpatched);
            return result;
        }

        private static JObject Fixture(UnitEntityData main)
        {
            var records = new JArray();
            foreach (IGrouping<string, ItemEntity> group in Tracked.GroupBy(i => i.Blueprint.AssetGuid))
            {
                ItemEntity[] held = Game.Instance.Player.Inventory.Items.Where(i => i.Blueprint != null && i.Blueprint.AssetGuid == group.Key).ToArray();
                ItemEntity first = group.First();
                var weapon = first as ItemEntityWeapon;
                records.Add(new JObject
                {
                    ["guid"] = group.Key, ["name"] = first.Blueprint.name, ["heldCount"] = held.Sum(i => i.Count), ["heldEntries"] = held.Length,
                    ["stored"] = WeightReaders.Stored(first.Blueprint), ["intended"] = WeightReaders.Intended(first.Blueprint),
                    ["enchantments"] = new JArray(held.SelectMany(i => i.Enchantments.Where(e => e != null && e.Blueprint != null).Select(e => (object)e.Blueprint.name))
                        .OrderBy(n => (string)n, StringComparer.Ordinal).ToArray()),
                    ["batteredOwners"] = new JArray(held.OfType<ItemEntityWeapon>().Select(w => (object)(ProductItems.BatteredOwner(w)?.UniqueId ?? "<none>"))
                        .OrderBy(n => (string)n, StringComparer.Ordinal).ToArray()),
                    ["weapon"] = weapon != null
                });
            }

            return new JObject { ["mainCharacter"] = main.UniqueId, ["items"] = records };
        }

        private static JObject Verify(string profileDir, JArray manifest, List<string> problems)
        {
            string path = Path.Combine(profileDir, FileName);
            if (!File.Exists(path)) { problems.Add("no weight item fixture at " + path); return new JObject(); }
            JObject fixture = JObject.Parse(File.ReadAllText(path));
            var found = new JArray();
            foreach (JToken record in (JArray)fixture["items"])
            {
                string guid = (string)record["guid"];
                ItemEntity[] held = Game.Instance.Player.Inventory.Items.Where(i => i.Blueprint != null && i.Blueprint.AssetGuid == guid).ToArray();
                int count = held.Sum(i => i.Count);
                string[] enchantments = held.SelectMany(i => i.Enchantments.Where(e => e != null && e.Blueprint != null).Select(e => e.Blueprint.name))
                    .OrderBy(n => n, StringComparer.Ordinal).ToArray();
                string[] owners = held.OfType<ItemEntityWeapon>().Select(w => ProductItems.BatteredOwner(w)?.UniqueId ?? "<none>")
                    .OrderBy(n => n, StringComparer.Ordinal).ToArray();
                float? stored = held.Length == 0 ? (float?)null : WeightReaders.Stored(held[0].Blueprint);
                var local = new List<string>();
                if (count != (int)record["heldCount"]) local.Add("count " + count + ", saved " + record["heldCount"]);
                if (!enchantments.SequenceEqual(((JArray)record["enchantments"]).Select(t => (string)t))) local.Add("enchantments " + string.Join(",", enchantments) + ", saved " + record["enchantments"].ToString(Newtonsoft.Json.Formatting.None));
                if (!owners.SequenceEqual(((JArray)record["batteredOwners"]).Select(t => (string)t))) local.Add("battered owners " + string.Join(",", owners) + ", saved " + record["batteredOwners"].ToString(Newtonsoft.Json.Formatting.None));
                if (stored == null || !stored.Value.Equals((float)record["stored"])) local.Add("stored weight " + stored + ", saved " + record["stored"]);
                found.Add(new JObject { ["name"] = record["name"], ["count"] = count, ["stored"] = stored, ["problem"] = local.Count == 0 ? null : string.Join("; ", local.ToArray()) });
                if (local.Count > 0) problems.Add((string)record["name"] + ": " + string.Join("; ", local.ToArray()));
                foreach (ItemEntity item in held) Track(item);
            }

            return new JObject { ["fixtureSha256"] = WeightReaders.Sha256(path), ["mainCharacter"] = fixture["mainCharacter"], ["items"] = found };
        }
    }
}
