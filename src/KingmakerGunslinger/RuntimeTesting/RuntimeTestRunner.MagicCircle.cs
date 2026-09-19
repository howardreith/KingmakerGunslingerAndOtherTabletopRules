using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Read-only, save-free inventory. This does not construct a fixture or
        // claim gameplay correctness. The guarded request is its only entry.
        private RuntimeTestResult RunMagicCircleNativeAudit()
        {
            var all = BlueprintBootstrap.Library.GetAllBlueprints()
                .Where(value => value != null).ToArray();
            var records = new List<string>();
            var relevant = all.Where(value =>
                value.name.IndexOf("ProtectionFrom", StringComparison.OrdinalIgnoreCase) >= 0 ||
                value.name.IndexOf("MagicCircle", StringComparison.OrdinalIgnoreCase) >= 0 ||
                value.name.IndexOf("Magic_Circle", StringComparison.OrdinalIgnoreCase) >= 0 ||
                value.name.IndexOf("Abjuration", StringComparison.OrdinalIgnoreCase) >= 0);
            foreach (var value in relevant.OrderBy(value => value.AssetGuid))
            {
                var row = new JObject { ["id"] = value.AssetGuid,
                    ["name"] = value.name, ["type"] = value.GetType().Name };
                row["fields"] = CircleAuditFields(value, 0);
                row["components"] = new JArray((value.ComponentsArray ??
                    Array.Empty<BlueprintComponent>()).Select(component =>
                        CircleAuditValue(component, 0)));
                var buff = value as BlueprintBuff;
                if (buff != null) row["flags"] = typeof(BlueprintBuff).GetField(
                    "m_Flags", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(buff).ToString();
                records.Add("donor=" + row.ToString(Formatting.None));
            }
            foreach (var value in all.OfType<BlueprintSpellbook>().OrderBy(value => value.AssetGuid))
                records.Add("spellbook=" + value.AssetGuid + ";name=" + value.name +
                    ";list=" + CircleAuditIdentity(value.SpellList) +
                    ";spontaneous=" + value.Spontaneous + ";arcane=" + value.IsArcane);
            foreach (var value in all.OfType<BlueprintCharacterClass>().OrderBy(value => value.AssetGuid))
                records.Add("class=" + value.AssetGuid + ";name=" + value.name +
                    ";book=" + CircleAuditIdentity(value.Spellbook));
            foreach (var value in all.OfType<BlueprintItemEquipmentUsable>()
                .Where(value => value.Type == UsableItemType.Scroll &&
                    (value.Cost == 375 || value.name.IndexOf("ProtectionFrom",
                        StringComparison.OrdinalIgnoreCase) >= 0)).OrderBy(value => value.AssetGuid))
                records.Add("scroll=" + value.AssetGuid + ";name=" + value.name +
                    ";cost=" + value.Cost + ";level=" + value.SpellLevel +
                    ";cl=" + value.CasterLevel + ";spell=" + CircleAuditIdentity(value.Ability) +
                    ";icon=" + (value.Icon == null ? "<null>" : value.Icon.name) +
                    ";components=" + new JArray(value.ComponentsArray.Select(component =>
                        CircleAuditValue(component, 0))).ToString(Formatting.None));
            string[] required = { "eee384c813b6d74498d1b9cc720d61f4",
                "4a6911969911ce9499bf27dde9bfcedc", "c8876df41a13f9243b3bfdb15b84b129",
                "718fa2f04fe085842a4960022d33d7ac", "8deb9d5cef3472646ac5199eb9edfb87" };
            var assertions = required.Select(id => Assertion("magic-circle-donor-" + id,
                "exactly one native identity", all.Count(value => value.AssetGuid == id).ToString(),
                all.Count(value => value.AssetGuid == id) == 1,
                "Loaded native library; inventory only, not mechanics acceptance")).ToList();
            var result = CreateResult(assertions.All(value => value.Status == "PASS") ? "PASS" : "FAIL",
                assertions, null);
            result.Diagnostics.AddRange(records);
            return result;
        }

        private static string CircleAuditIdentity(BlueprintScriptableObject value)
        { return value == null ? "<null>" : value.AssetGuid + ":" + value.name; }

        private static JObject CircleAuditFields(object value, int depth)
        {
            var result = new JObject();
            foreach (var field in value.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(field => field.Name != "ComponentsArray").OrderBy(field => field.Name))
                result[field.Name] = CircleAuditValue(field.GetValue(value), depth + 1);
            return result;
        }

        private static JToken CircleAuditValue(object value, int depth)
        {
            if (value == null) return JValue.CreateNull();
            var blueprint = value as BlueprintScriptableObject;
            if (blueprint != null) return new JValue(CircleAuditIdentity(blueprint));
            var type = value.GetType();
            if (type.IsEnum || type.IsPrimitive || value is string || value is decimal)
                return new JValue(Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture));
            if (depth >= 7) return new JValue(type.FullName);
            var sequence = value as IEnumerable;
            if (sequence != null) return new JArray(sequence.Cast<object>().Take(80)
                .Select(item => CircleAuditValue(item, depth + 1)));
            var fields = CircleAuditFields(value, depth);
            fields["type"] = type.FullName;
            return fields;
        }
    }
}
