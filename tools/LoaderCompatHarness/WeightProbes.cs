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
            // The lab's configuration source (never written by a session) and this session's staged copy of it.
            string stagedFile = Path.Combine(entry.Path, "Settings.xml");
            string pristineFile = Path.GetFullPath(Path.Combine(entry.Path, "..", "..", ".lab-pristine", "Mods", ModId, "Settings.xml"));
            Func<string, string> fileState = file =>
            {
                if (!File.Exists(file)) return "absent";
                string text = File.ReadAllText(file);
                return text.Contains("<toggleItemsWeighZero>" + System.Security.SecurityElement.Escape(on) + "</toggleItemsWeighZero>") ? "on" :
                    text.Contains("<toggleItemsWeighZero>" + System.Security.SecurityElement.Escape(off) + "</toggleItemsWeighZero>") ? "off" : "invalid";
            };
            result["pristineSettings"] = new JObject { ["path"] = pristineFile, ["sha256"] = WeightReaders.Sha256(pristineFile), ["noItemWeight"] = fileState(pristineFile),
                ["dllSha256"] = WeightReaders.Sha256(Path.Combine(Path.GetDirectoryName(pristineFile), "BagOfTricks.dll")) };
            result["stagedSettingsBefore"] = new JObject { ["sha256"] = WeightReaders.Sha256(stagedFile), ["noItemWeight"] = fileState(stagedFile) };
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
        public static JObject Run(string profileDir, string label, string expect, string type, IList<string> natives, IDictionary<string, string> invariants,
            out string problem)
        {
            var problems = new List<string>();
            var manifest = new JArray(WeightReaders.ManifestEntries());
            // invariants.extraManifestEntry "Symbol=guid:PlannedType": a synthetic required identity (self-test of the
            // missing-identity verdict); it is added to the expected set only.
            if (invariants != null && invariants.TryGetValue("extraManifestEntry", out string extra) && !string.IsNullOrWhiteSpace(extra))
            {
                string[] parts = extra.Split('=', ':');
                manifest.Add(new JObject { ["symbol"] = parts[0], ["guid"] = parts[1], ["plannedType"] = parts[2], ["status"] = "synthetic" });
            }

            Dictionary<string, string> contracts = (invariants ?? new Dictionary<string, string>())
                .Where(p => p.Key.StartsWith("contract:", StringComparison.Ordinal)).ToDictionary(p => p.Key.Substring(9), p => p.Value);
            var items = new JObject();
            var unresolvedPlanned = new JArray();
            var mistyped = new JArray();
            var byType = new SortedDictionary<string, int>(StringComparer.Ordinal);
            var byStatus = new SortedDictionary<string, int>(StringComparer.Ordinal);
            var byContract = new SortedDictionary<string, int>(StringComparer.Ordinal);
            int plannedItems = 0, normallyWeightless = 0, storedZero = 0, storedZeroInheriting = 0, storedPositive = 0;
            foreach (JToken entry in manifest)
            {
                string guid = (string)entry["guid"], planned = (string)entry["plannedType"] ?? string.Empty, status = (string)entry["status"] ?? "<none>";
                string symbol = (string)entry["symbol"];
                Type plannedType = PlannedType(planned);
                bool plannedItem = plannedType != null && typeof(BlueprintItem).IsAssignableFrom(plannedType);
                if (plannedItem) plannedItems++;
                BlueprintScriptableObject resolved = ResourcesLibrary.TryGetBlueprint<BlueprintScriptableObject>(guid);
                BlueprintItem item = resolved as BlueprintItem;
                if (plannedItem && item == null)
                {
                    // Every manifest item identity is registered unconditionally; a missing or non-item one is a defect.
                    string what = resolved == null ? "does not resolve" : "resolves to " + resolved.GetType().Name;
                    unresolvedPlanned.Add(symbol + " (" + guid + ", " + status + ") " + what);
                    problems.Add("planned item " + symbol + " (" + guid + ") " + what);
                    continue;
                }

                if (item == null) continue;
                if (plannedType == null || !plannedType.IsInstanceOfType(item))
                {
                    mistyped.Add(symbol + " planned " + planned + ", resolved " + item.GetType().Name);
                    problems.Add(symbol + " (" + guid + ") planned " + planned + " but resolves to " + item.GetType().Name);
                }

                JObject record = Record(item, symbol, status, expect, problems);
                Contract(record, item, symbol, contracts, problems);
                items[guid] = record;
                float stored = (float)record["stored"], intended = (float)record["intended"];
                if (intended == 0f) normallyWeightless++;
                if (stored == 0f) storedZero++; else storedPositive++;
                if (stored == 0f && intended > 0f) storedZeroInheriting++;
                string concrete = item.GetType().Name;
                byType[concrete] = byType.TryGetValue(concrete, out int t) ? t + 1 : 1;
                byStatus[status] = byStatus.TryGetValue(status, out int s) ? s + 1 : 1;
                string kind = ((string)record["contract"] ?? "none").Split(':')[0];
                byContract[kind] = byContract.TryGetValue(kind, out int c) ? c + 1 : 1;
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
                ["resolvedItems"] = items.Count, ["unresolvedPlannedItems"] = unresolvedPlanned, ["mistypedItems"] = mistyped,
                ["storedPositive"] = storedPositive, ["storedZero"] = storedZero, ["storedZeroInheritingPositiveWeight"] = storedZeroInheriting,
                ["normallyWeightless"] = normallyWeightless,
                ["byType"] = Counts(byType), ["byStatus"] = Counts(byStatus), ["byConstructionContract"] = Counts(byContract),
                ["getterPatchOwners"] = WeightReaders.PatchOwners(), ["items"] = items, ["natives"] = nativeRecords
            };
            if (items.Count == 0) problems.Add("no registered product item resolved");
            string path = Path.Combine(profileDir, "weight-census-" + label + ".json");
            File.WriteAllText(path, result.ToString(Newtonsoft.Json.Formatting.Indented));
            result["recordSha256"] = WeightReaders.Sha256(path);
            if (!string.IsNullOrEmpty(type) && type.StartsWith("compare:", StringComparison.Ordinal))
                result["compared"] = Compare(profileDir, type.Substring(8), items, nativeRecords, problems);

            // The file keeps every record; the harness result keeps the summary and only problem records.
            result["items"] = new JObject(items.Properties().Where(p => p.Value["problem"] != null || p.Value["contractProblem"] != null).Take(40));
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

        private static Type PlannedType(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            return typeof(BlueprintItem).Assembly.GetTypes().FirstOrDefault(t => t.Name == name && typeof(BlueprintScriptableObject).IsAssignableFrom(t));
        }

        /// <summary>
        /// The item's construction contract, by longest matching symbol prefix (invariants "contract:&lt;prefix&gt;"):
        /// "value:&lt;lb&gt;" - stored m_Weight is that catalog or source constant; "donor:&lt;guid&gt;[|&lt;fallback&gt;]" - stored
        /// m_Weight equals the stored m_Weight of the first donor that resolves (the constructor's own fallback order);
        /// "type" - the unpatched weight equals the item's weapon type's stored weight (firearm items and types share
        /// one catalog weight; clones without their own weight inherit the type's). An item without a contract fails.
        /// </summary>
        private static void Contract(JObject record, BlueprintItem item, string symbol, Dictionary<string, string> contracts, List<string> problems)
        {
            string key = contracts.Keys.Where(k => symbol.StartsWith(k, StringComparison.Ordinal)).OrderByDescending(k => k.Length).FirstOrDefault();
            if (key == null)
            {
                record["contract"] = "none";
                problems.Add(symbol + " " + item.name + ": no construction contract");
                return;
            }

            string rule = contracts[key];
            float stored = WeightReaders.Stored(item), intended = WeightReaders.Intended(item);
            string local = null;
            if (rule.StartsWith("value:", StringComparison.Ordinal))
            {
                float expected = float.Parse(rule.Substring(6), CultureInfo.InvariantCulture);
                record["contractExpected"] = expected;
                if (!stored.Equals(expected)) local = "stored " + WeightReaders.Text(stored) + ", contract " + WeightReaders.Text(expected);
            }
            else if (rule.StartsWith("donor:", StringComparison.Ordinal))
            {
                BlueprintItem donor = rule.Substring(6).Split('|').Select(g => ResourcesLibrary.TryGetBlueprint<BlueprintScriptableObject>(g) as BlueprintItem)
                    .FirstOrDefault(d => d != null);
                if (donor == null) local = "no contract donor resolves";
                else
                {
                    record["contractDonor"] = donor.AssetGuid + " " + donor.name;
                    record["contractExpected"] = WeightReaders.Stored(donor);
                    if (!stored.Equals(WeightReaders.Stored(donor))) local = "stored " + WeightReaders.Text(stored) + ", donor " + donor.name + " stores " + WeightReaders.Text(WeightReaders.Stored(donor));
                }
            }
            else if (rule == "type")
            {
                var weapon = item as BlueprintItemWeapon;
                BlueprintWeaponType weaponType = weapon == null ? null : WeightReaders.WeaponType(weapon);
                if (weaponType == null) local = "type contract on an item without a weapon type";
                else
                {
                    record["contractExpected"] = WeightReaders.Stored(weaponType);
                    if (!intended.Equals(WeightReaders.Stored(weaponType)) || !(intended > 0f))
                        local = "unpatched " + WeightReaders.Text(intended) + ", weapon type " + weaponType.name + " stores " + WeightReaders.Text(WeightReaders.Stored(weaponType));
                }
            }
            else local = "unknown contract rule " + rule;

            record["contract"] = rule;
            if (local != null)
            {
                record["contractProblem"] = local;
                problems.Add(symbol + " " + item.name + ": " + local);
            }
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
                int heldBefore = HeldCount(blueprint.AssetGuid);
                var yielded = new List<ItemEntity>();
                for (int i = 0; i < count; i++)
                {
                    ItemEntity one = Game.Instance.Player.Inventory.Add(blueprint);
                    if (one != null && !yielded.Any(y => ReferenceEquals(y, one))) yielded.Add(one);
                }

                ItemEntity item = yielded.LastOrDefault();
                if (item == null) { problems.Add("inventory add yielded nothing for " + spec); continue; }
                // The requested quantity arrived, and a stackable request formed one stack.
                int delta = HeldCount(blueprint.AssetGuid) - heldBefore;
                if (delta != count) problems.Add(spec + " added " + delta + " items, requested " + count);
                if (count > 1 && blueprint.IsActuallyStackable && yielded.Count != 1)
                    problems.Add(spec + " formed " + yielded.Count + " entries, expected one stack");
                if (yielded.Count > 1) foreach (ItemEntity other in yielded.Take(yielded.Count - 1)) Track(other);
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
                    string displayProblem = DisplayVerdict(item.Count, display, expectedDisplay);
                    if (displayProblem != null) problems.Add(item.Blueprint.name + " " + displayProblem);
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
                    // One tuple per inventory entry: its own quantity, enchantments and battered owner together.
                    ["entries"] = new JArray(EntryTuples(held).Cast<object>().ToArray()),
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
                string[] now = EntryTuples(held);
                float? stored = held.Length == 0 ? (float?)null : WeightReaders.Stored(held[0].Blueprint);
                var local = new List<string>();
                if (count != (int)record["heldCount"]) local.Add("count " + count + ", saved " + record["heldCount"]);
                if (held.Length != (int)record["heldEntries"]) local.Add("entries " + held.Length + ", saved " + record["heldEntries"]);
                string entryProblem = CompareEntries(((JArray)record["entries"]).Select(t => (string)t).ToArray(), now);
                if (entryProblem != null) local.Add(entryProblem);
                if (stored == null || !stored.Value.Equals((float)record["stored"])) local.Add("stored weight " + stored + ", saved " + record["stored"]);
                found.Add(new JObject { ["name"] = record["name"], ["count"] = count, ["entries"] = new JArray(now.Cast<object>().ToArray()), ["stored"] = stored,
                    ["problem"] = local.Count == 0 ? null : string.Join("; ", local.ToArray()) });
                if (local.Count > 0) problems.Add((string)record["name"] + ": " + string.Join("; ", local.ToArray()));
                foreach (ItemEntity item in held) Track(item);
            }

            return new JObject { ["fixtureSha256"] = WeightReaders.Sha256(path), ["mainCharacter"] = fixture["mainCharacter"], ["items"] = found };
        }

        private static int HeldCount(string guid) =>
            Game.Instance.Player.Inventory.Items.Where(i => i.Blueprint != null && i.Blueprint.AssetGuid == guid).Sum(i => i.Count);

        /// <summary>"count=N|enchantments=a,b|owner=id" per inventory entry, sorted (a multiset: 2.1.7b items have no unique id).</summary>
        private static string[] EntryTuples(IEnumerable<ItemEntity> held)
        {
            return held.Select(i => EntryTuple(i.Count,
                    i.Enchantments.Where(e => e != null && e.Blueprint != null).Select(e => e.Blueprint.name),
                    i is ItemEntityWeapon w ? ProductItems.BatteredOwner(w)?.UniqueId ?? "<none>" : "<not-weapon>"))
                .OrderBy(t => t, StringComparer.Ordinal).ToArray();
        }

        public static string EntryTuple(int count, IEnumerable<string> enchantments, string owner)
        {
            return "count=" + count.ToString(CultureInfo.InvariantCulture) + "|enchantments=" +
                string.Join(",", enchantments.OrderBy(n => n, StringComparer.Ordinal).ToArray()) + "|owner=" + owner;
        }

        /// <summary>Null when the saved and loaded entry multisets are identical; otherwise the differences.</summary>
        public static string CompareEntries(string[] saved, string[] now)
        {
            var missing = saved.ToList();
            var extra = new List<string>();
            foreach (string tuple in now)
                if (!missing.Remove(tuple)) extra.Add(tuple);
            if (missing.Count == 0 && extra.Count == 0) return null;
            return "entries differ: saved-only [" + string.Join("; ", missing.ToArray()) + "], loaded-only [" + string.Join("; ", extra.ToArray()) + "]";
        }

        /// <summary>Null when the tooltip text is readable and matches; an unreadable text (formatter threw) always fails.</summary>
        public static string DisplayVerdict(int count, string display, string expectedDisplay)
        {
            if (display == null || display.StartsWith("<throws", StringComparison.Ordinal)) return "tooltip text is unreadable: " + display;
            if (expectedDisplay == null || expectedDisplay.StartsWith("<throws", StringComparison.Ordinal)) return "expected tooltip text is unreadable: " + expectedDisplay;
            bool matches = count == 1 ? display == expectedDisplay : display.StartsWith(expectedDisplay, StringComparison.Ordinal);
            return matches ? null : "displays " + display + ", expected " + expectedDisplay;
        }
    }

    /// <summary>
    /// kmgPublicationStatus. match "Property=set|null,...": BlueprintBootstrap's published-state properties (for
    /// example MagicCirclePublication, which the bootstrap clears when it contains a Magic Circle publication failure
    /// and still completes). Detects a partially published startup that the global initialization state cannot.
    /// </summary>
    internal static class PublicationStatus
    {
        private const BindingFlags Any = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public static JObject Run(string match, out string problem)
        {
            var problems = new List<string>();
            var result = new JObject();
            Type bootstrap = ProductProbes.Entry().Assembly.GetType("KingmakerGunslinger.Bootstrap.BlueprintBootstrap", true);
            foreach (string pair in (match ?? string.Empty).Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] parts = pair.Split('=');
                string name = parts[0].Trim(), expected = parts.Length > 1 ? parts[1].Trim() : "set";
                PropertyInfo property = bootstrap.GetProperty(name, Any);
                if (property == null) { problems.Add("BlueprintBootstrap has no property " + name); continue; }
                object value = property.GetValue(null, null);
                bool set = value != null && !(value is Array array && array.Length == 0);
                result[name] = set ? "set" : "null";
                if ((expected == "set") != set) problems.Add(name + " is " + (set ? "set" : "null") + ", expected " + expected);
            }

            problem = problems.Count == 0 ? null : string.Join("; ", problems.ToArray());
            return result;
        }
    }

    /// <summary>Test-only conditions for this mission. failMagicCircleSpellListPublication: the product's Magic Circle
    /// spell-list publication throws, as a contained publication failure would.</summary>
    internal static class WeightConditions
    {
        public const string FailMagicCircleSpellListPublication = "failMagicCircleSpellListPublication";
        public const string InjectedMessage = "KmgLoaderCompatHarness injected Magic Circle publication failure";
        public static readonly string[] Known = { FailMagicCircleSpellListPublication };

        public static void Apply(HarmonyInstance harmony, IDictionary<string, string> conditions, JObject applied)
        {
            if (!conditions.TryGetValue(FailMagicCircleSpellListPublication, out string value) || string.IsNullOrWhiteSpace(value) ||
                string.Equals(value, "false", StringComparison.OrdinalIgnoreCase)) return;
            Type publication = ProductProbes.Entry().Assembly.GetType("KingmakerGunslinger.Blueprints.MagicCircleSpellListPublication", true);
            MethodInfo publish = publication.GetMethod("Publish", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) ??
                throw new MissingMethodException(publication.FullName, "Publish");
            harmony.Patch(publish, new HarmonyMethod(typeof(WeightConditions).GetMethod(nameof(FailPrefix), BindingFlags.Static | BindingFlags.NonPublic)), null);
            applied[FailMagicCircleSpellListPublication] = "prefix on MagicCircleSpellListPublication.Publish throws";
        }

        private static bool FailPrefix()
        {
            throw new InvalidOperationException(InjectedMessage);
        }
    }

    /// <summary>
    /// kmgInventoryWeightView. Opens the real inventory window (IServiceWindowUIHandler.HandleOpenInventory), waits for
    /// it to show, then toggles No Item Weight (match on/off) through Bag of Tricks' own setting and observes the view
    /// across the game's normal refresh: Kingmaker 2.1.7b Inventory.Update compares the party carrying capacity every
    /// frame and, when it changed, calls StashEncumbrance.Update (observed by a harness postfix) and Stash.Setup. Records
    /// the capacity bound to the view and the stash encumbrance label before the toggle, in the toggle frame and after
    /// the refresh, the refresh frame, and the tooltip weight text of the "contains" items (product and native control).
    /// </summary>
    internal static class InventoryWeightView
    {
        private const BindingFlags Any = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private enum Phase { Open, WaitShown, Toggled }
        private static string owner;
        private static Phase phase;
        private static int openFrame, toggleFrame;
        private static JObject record;
        private static bool patched;
        private static readonly List<JObject> Refreshes = new List<JObject>();
        private static readonly FieldInfo PartyCapacityField = typeof(Kingmaker.UI.ServiceWindow.Inventory).GetField("PartyCapacity", Any);
        private static readonly FieldInfo LabelField = typeof(Kingmaker.UI.ServiceWindow.Inventory.EncumbranceContainer).GetField("m_EncumbranceLabel", Any);

        /// <summary>True when the observation is finished (result and problem set).</summary>
        public static bool Poll(string label, string match, IList<string> references, out JObject result, out string problem)
        {
            result = null;
            problem = null;
            if (owner != label) { owner = label; phase = Phase.Open; record = new JObject { ["toggleTo"] = match }; Refreshes.Clear(); }
            int frame = UnityEngine.Time.frameCount;
            switch (phase)
            {
                case Phase.Open:
                    EnsurePatched();
                    Kingmaker.PubSubSystem.EventBus.RaiseEvent<Kingmaker.PubSubSystem.IServiceWindowUIHandler>(h => h.HandleOpenInventory());
                    openFrame = frame;
                    phase = Phase.WaitShown;
                    return false;
                case Phase.WaitShown:
                {
                    var inventory = UnityEngine.Object.FindObjectOfType<Kingmaker.UI.ServiceWindow.Inventory>();
                    if (inventory == null || !inventory.IsShow || frame - openFrame < 30)
                    {
                        if (frame - openFrame <= 900) return false;
                        return Finish(out result, out problem, "the inventory window did not open");
                    }

                    record["openedAfterFrames"] = frame - openFrame;
                    record["before"] = Snapshot(inventory, references);
                    record["toggle"] = BagOfTricksWeight.Run(match, false, match, out string toggleProblem);
                    if (toggleProblem != null) return Finish(out result, out problem, "toggle failed: " + toggleProblem);
                    // Same frame, before Inventory.Update has run again: the view still holds its old values.
                    record["toggleFrame"] = Snapshot(inventory, references);
                    toggleFrame = frame;
                    Refreshes.Clear();
                    phase = Phase.Toggled;
                    return false;
                }
                default:
                {
                    if (frame - toggleFrame < 30) return false;
                    var inventory = UnityEngine.Object.FindObjectOfType<Kingmaker.UI.ServiceWindow.Inventory>();
                    if (inventory == null) return Finish(out result, out problem, "the inventory window closed during the observation");
                    JObject after = Snapshot(inventory, references);
                    record["after"] = after;
                    record["stashRefreshes"] = new JArray(Refreshes.Cast<object>().ToArray());
                    var problems = new List<string>();
                    float before = (float)record["before"]["boundCurrentWeight"], stale = (float)record["toggleFrame"]["boundCurrentWeight"];
                    float bound = (float)after["boundCurrentWeight"], live = (float)after["liveCurrentWeight"], unpatched = (float)after["unpatchedSum"];
                    float expected = match == "on" ? 0f : unpatched;
                    if (!stale.Equals(before)) problems.Add("the view changed in the toggle frame, before its own refresh");
                    if (Math.Abs(bound - expected) > 0.01f) problems.Add("the inventory view holds carried weight " + bound + ", expected " + expected);
                    if (Math.Abs(live - expected) > 0.01f) problems.Add("live carried weight " + live + ", expected " + expected);
                    JObject refresh = Refreshes.FirstOrDefault(r => (bool)r["stash"]);
                    if (refresh == null) problems.Add("no StashEncumbrance refresh after the toggle");
                    else
                    {
                        record["refreshFramesAfterToggle"] = (int)refresh["frame"] - toggleFrame;
                        if (Math.Abs((float)refresh["currentWeight"] - expected) > 0.01f) problems.Add("the refresh bound " + refresh["currentWeight"] + ", expected " + expected);
                    }

                    string labelText = (string)after["stashLabel"], weightText = (string)after["boundWeightText"];
                    if (labelText != null && (weightText == null || labelText.IndexOf(weightText, StringComparison.Ordinal) < 0))
                        problems.Add("the stash encumbrance label '" + labelText + "' does not show the bound weight " + weightText);
                    foreach (JProperty item in ((JObject)after["items"]).Properties())
                    {
                        string verdict = WeightItems.DisplayVerdict((int)item.Value["count"], (string)item.Value["display"], (string)item.Value["expectedDisplay"]);
                        if (verdict != null) problems.Add(item.Name + " " + verdict);
                    }

                    return Finish(out result, out problem, problems.Count == 0 ? null : string.Join("; ", problems.ToArray()));
                }
            }
        }

        private static bool Finish(out JObject result, out string problem, string failure)
        {
            try
            {
                var tabs = Game.Instance.UI.ServiceWindow?.WindowTabs;
                if (tabs != null && tabs.IsShow) tabs.ToggleShow();
                record["closed"] = true;
            }
            catch (Exception exception) { record["closed"] = "<throws " + exception.GetType().Name + ">"; }
            result = record;
            problem = failure;
            owner = null;
            return true;
        }

        private static JObject Snapshot(Kingmaker.UI.ServiceWindow.Inventory inventory, IList<string> references)
        {
            var bound = (EncumbranceHelper.CarryingCapacity)PartyCapacityField.GetValue(inventory);
            EncumbranceHelper.CarryingCapacity live = EncumbranceHelper.GetPartyCarryingCapacity();
            float unpatched = 0f;
            foreach (ItemEntity entry in Game.Instance.Player.Inventory.Items)
                if (entry.Owner == null || Game.Instance.Player.Party.Contains(entry.Owner.Unit)) unpatched += entry.Count * WeightReaders.Intended(entry.Blueprint);
            string labelText = null;
            object labelObject = LabelField?.GetValue(inventory.StashEncumbrance);
            if (labelObject is UnityEngine.Object unityLabel && unityLabel != null)
                labelText = labelObject.GetType().GetProperty("text")?.GetValue(labelObject, null) as string;
            var items = new JObject();
            JArray manifest = WeightReaders.ManifestEntries();
            bool on = (string)BagOfTricksWeight.Run(null, false, null, out string _)["after"] == "on";
            foreach (string reference in references ?? new List<string>())
            {
                BlueprintItem blueprint = WeightReaders.ResolveItem(reference, manifest);
                ItemEntity held = Game.Instance.Player.Inventory.Items.FirstOrDefault(i => ReferenceEquals(i.Blueprint, blueprint));
                if (held == null) { items[reference] = new JObject { ["held"] = false, ["display"] = "<throws not-held>", ["count"] = 0 }; continue; }
                float expectedWeight = on ? 0f : WeightReaders.Intended(blueprint);
                string display, expectedDisplay;
                try { display = UIUtilityTexts.GetItemWeight(held); } catch (Exception e) { display = "<throws " + e.GetType().Name + ">"; }
                try
                {
                    expectedDisplay = held.Count == 1 ? UIUtilityTexts.GetItemWeight(expectedWeight)
                        : expectedWeight.ToString("#####0.##") + " (" + (expectedWeight * held.Count).ToString("#####0.##") + ")";
                }
                catch (Exception e) { expectedDisplay = "<throws " + e.GetType().Name + ">"; }
                items[reference] = new JObject { ["name"] = blueprint.name, ["count"] = held.Count, ["display"] = display, ["expectedDisplay"] = expectedDisplay };
            }

            string weightText;
            try { weightText = bound.GetCurrentWeightText(); } catch (Exception e) { weightText = "<throws " + e.GetType().Name + ">"; }
            return new JObject
            {
                ["frame"] = UnityEngine.Time.frameCount, ["botSetting"] = on ? "on" : "off", ["boundCurrentWeight"] = bound.CurrentWeight, ["boundWeightText"] = weightText,
                ["liveCurrentWeight"] = live.CurrentWeight, ["unpatchedSum"] = unpatched, ["stashLabel"] = labelText, ["items"] = items
            };
        }

        private static void EnsurePatched()
        {
            if (patched) return;
            MethodInfo update = typeof(Kingmaker.UI.ServiceWindow.Inventory.EncumbranceContainer).GetMethod("Update", BindingFlags.Instance | BindingFlags.Public) ??
                throw new MissingMethodException("Inventory.EncumbranceContainer.Update");
            HarmonyInstance harmony = SpearRepairConditions.Harmony ?? HarmonyInstance.Create(SpearRepairConditions.HarnessId + ".inventory-view");
            harmony.Patch(update, null, new HarmonyMethod(typeof(InventoryWeightView).GetMethod(nameof(UpdatePostfix), BindingFlags.Static | BindingFlags.NonPublic)));
            patched = true;
        }

        private static void UpdatePostfix(Kingmaker.UI.ServiceWindow.Inventory.EncumbranceContainer __instance, EncumbranceHelper.CarryingCapacity __0)
        {
            var inventory = UnityEngine.Object.FindObjectOfType<Kingmaker.UI.ServiceWindow.Inventory>();
            Refreshes.Add(new JObject
            {
                ["frame"] = UnityEngine.Time.frameCount, ["currentWeight"] = __0.CurrentWeight,
                ["stash"] = inventory != null && ReferenceEquals(inventory.StashEncumbrance, __instance)
            });
        }
    }

    /// <summary>
    /// kmgWeightProbeSelfTest. Synthetic cases for the probes' own verdict functions (no game state): a split stack and a
    /// reassigned per-entry enchantment or owner must fail the persistence comparison while an identical multiset passes;
    /// unreadable tooltip text must fail even when both formatter calls throw the same exception.
    /// </summary>
    internal static class WeightProbeSelfTest
    {
        public static JObject Run(out string problem)
        {
            var problems = new List<string>();
            var cases = new JObject();
            Func<int, string[], string, string> t = (n, e, o) => WeightItems.EntryTuple(n, e, o);
            string[] saved = { t(10, new string[0], "<not-weapon>") };
            string[] split = Enumerable.Repeat(t(1, new string[0], "<not-weapon>"), 10).ToArray();
            string[] pair = { t(1, new[] { "Enhancement1" }, "<none>"), t(1, new string[0], "owner-a") };
            string[] reassigned = { t(1, new string[0], "<none>"), t(1, new[] { "Enhancement1" }, "owner-a") };
            Action<string, bool, string> check = (name, shouldFail, verdict) =>
            {
                bool failed = verdict != null;
                cases[name] = new JObject { ["expectFailure"] = shouldFail, ["verdict"] = verdict };
                if (failed != shouldFail) problems.Add(name + (shouldFail ? " was accepted" : " was rejected: " + verdict));
            };
            check("identical-multiset", false, WeightItems.CompareEntries(saved, saved.ToArray()));
            check("identical-pair-reordered", false, WeightItems.CompareEntries(pair, pair.Reverse().ToArray()));
            check("stack-of-ten-split-into-singletons", true, WeightItems.CompareEntries(saved, split));
            check("enchantment-and-owner-reassigned", true, WeightItems.CompareEntries(pair, reassigned));
            check("both-formatters-throw-same-exception", true, WeightItems.DisplayVerdict(1, "<throws NullReferenceException>", "<throws NullReferenceException>"));
            check("readable-matching-display", false, WeightItems.DisplayVerdict(1, "0 lbs", "0 lbs"));
            problem = problems.Count == 0 ? null : string.Join("; ", problems.ToArray());
            return new JObject { ["cases"] = cases };
        }
    }
}
