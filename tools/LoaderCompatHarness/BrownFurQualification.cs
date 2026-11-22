using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Buffs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KmgLoaderCompatHarness
{
    /// <summary>
    /// A2 qualification modes ("create", "reload", "final") on the corrected product. Six disposable targets receive
    /// real committed Powerful Change casts from the disposable Arcanist. At the save, after the product's own PreSave,
    /// the observer writes SYNTHETIC entries beside the real records on five of them (an ambiguous misspelled sibling,
    /// a conflicting duplicate, an exact duplicate, unsupported and unknown content, an entry that states no identity).
    /// A fresh process checks the native load, the first consumer, restored modifiers, retained content and repeated
    /// reads, then makes a real recast, a removal and a forced refused write; a final fresh process checks the result.
    /// </summary>
    internal sealed partial class BrownFurProbe
    {
        private static readonly string[] Targets = { "valid", "siblings", "conflict", "exact", "unsupported", "unattributable" };
        private const string OtherBuff = "0123456789abcdef0123456789abcdef";

        private static object refuseOwner;
        private static int refusedCount;
        private static readonly List<string> refusedRecords = new List<string>();
        private static HashSet<object> refuseExisting;

        private Type jsonType, storeType;

        private IEnumerable<object> Qualification()
        {
            jsonType = product.GetType("KingmakerGunslinger.BrownFur.BrownFurPersistedModifierJson", true);
            storeType = product.GetType("KingmakerGunslinger.BrownFur.BrownFurPersistedModifierStore", true);
            switch (mode)
            {
                case "create": return Create();
                case "reload": return Reload();
                case "final": return Final();
                default:
                    problems.Add("unknown mode " + mode);
                    return Enumerable.Empty<object>();
            }
        }

        // ---- create ----

        private IEnumerable<object> Create()
        {
            foreach (object step in AwaitBrownFur()) yield return step;
            if (!ResolveBrownFur()) yield break;
            UnitEntityData caster;
            if (!SpawnCaster(out caster)) yield break;
            var units = new JObject { ["caster"] = caster.UniqueId };
            var targets = new Dictionary<string, UnitEntityData>();
            for (int index = 0; index < Targets.Length; index++)
            {
                UnitEntityData target;
                if (!SpawnTarget(Targets[index], index, out target)) yield break;
                targets[Targets[index]] = target;
                units[Targets[index]] = target.UniqueId;
            }
            result["units"] = units;
            yield return null;

            var casts = new JObject();
            foreach (string name in Targets)
                casts[name + ".bullsStrength"] = CastAbilityBonus(caster, targets[name], BullsStrength, StatType.Strength, 0, 6);
            casts["siblings.catsGrace"] = CastAbilityBonus(caster, targets["siblings"], CatsGrace, StatType.Dexterity, 1, 6);
            result["casts"] = casts;
            if (problems.Count != 0) yield break;
            foreach (string name in Targets)
                ExpectPart(targets[name], name + ".created", name == "siblings" ? 2 : 1, 0);
            if (problems.Count != 0) yield break;
            File.WriteAllText(Path.Combine(profileDir, FixtureFile), new JObject { ["units"] = units }.ToString());

            // SYNTHETIC entries, built from each target's real record as the product writes it.
            var injections = new JObject();
            JObject strength = RealRecord(targets["siblings"], "Strength");
            JObject dexterity = RealRecord(targets["siblings"], "Dexterity");
            Arm(targets["siblings"], injections, "siblings", new JToken[] { dexterity, strength, With(strength, "AbilityScore", "Strenght") });
            JObject conflict = RealRecord(targets["conflict"], "Strength");
            Arm(targets["conflict"], injections, "conflict", new JToken[] { conflict, With(conflict, "Increase", (int)conflict["Increase"] == 2 ? 4 : 2) });
            JObject exact = RealRecord(targets["exact"], "Strength");
            Arm(targets["exact"], injections, "exact", new JToken[] { exact, exact.DeepClone() });
            JObject unsupported = RealRecord(targets["unsupported"], "Strength");
            Arm(targets["unsupported"], injections, "unsupported", new JToken[] {
                unsupported,
                With(With(unsupported, "SchemaVersion", 2), "BuffGuid", OtherBuff),
                With(With(unsupported, "AbilityScore", 99), "BuffGuid", OtherBuff),
                With(With(unsupported, "FutureField", new JObject { ["nested"] = new JArray(1, "two", true, null, 1.5m, "2026-11-22T10:00:00Z") }),
                    "BuffGuid", OtherBuff) });
            JObject unattributable = RealRecord(targets["unattributable"], "Strength");
            Arm(targets["unattributable"], injections, "unattributable", new JToken[] { unattributable, new JValue("garbage") });
            result["syntheticInjections"] = injections;

            int callIndex = BrownFurObserver.Calls.Count;
            foreach (object step in Save()) yield return step;
            JArray calls = BrownFurObserver.CallsSince(callIndex);
            result["saveCalls"] = calls;
            RestoreAfterSave();
            foreach (JProperty injected in injections.Properties())
            {
                string owner = (string)units[injected.Name];
                if (!calls.OfType<JObject>().Any(c => (string)c["kind"] == "PreSave" && (string)c["owner"] == owner && (bool)c["completed"] && c["syntheticArmed"] != null))
                    problems.Add(injected.Name + ": the save's PreSave did not complete with the synthetic injection");
            }
            if (!calls.OfType<JObject>().Any(c => (string)c["kind"] == "PreSave" && (string)c["owner"] == (string)units["valid"] && (bool)c["completed"]))
                problems.Add("valid: the save's PreSave did not complete");

            // What the file holds for each target, read back from the written save.
            JObject file = WrittenParts(targets.Values.Select(t => t.UniqueId));
            result["writtenParts"] = file;
            foreach (JProperty injected in injections.Properties())
            {
                JToken written = file[(string)units[injected.Name]]?["_records"];
                if (!JToken.DeepEquals(written, injected.Value["synthetic"]))
                    problems.Add(injected.Name + ": the file's _records differ from the synthetic injection: " + (written?.ToString(Formatting.None) ?? "null"));
            }
            JToken valid = file[(string)units["valid"]]?["_records"];
            if (!(valid is JArray) || ((JArray)valid).Count != 1 || !(valid[0] is JObject))
                problems.Add("valid: the file does not hold exactly one record: " + (valid?.ToString(Formatting.None) ?? "null"));
            else
                result["serializerMatrix"] = SerializerMatrix((JObject)valid[0]);
            result["serializerSettings"] = SettingsDescription();
        }

        /// <summary>Each named unit's persisted Brown-Fur members, as written in the save file.</summary>
        private JObject WrittenParts(IEnumerable<string> unitIds)
        {
            var found = new JObject();
            var wanted = new HashSet<string>(unitIds);
            string path = Directory.GetFiles(Game.Instance.SaveManager.SavePath, "*_" + saveLabel + ".zks")
                .OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault();
            if (path == null) { problems.Add("the written save " + saveLabel + " was not found"); return found; }
            result["writtenSave"] = Path.GetFileName(path);
            Type saverType = typeof(Kingmaker.EntitySystem.Persistence.SaveManager).Assembly.GetType("Kingmaker.EntitySystem.Persistence.ZipSaver", true);
            object saver = Activator.CreateInstance(saverType, path);
            try
            {
                var files = (IEnumerable<string>)saverType.GetMethod("GetAllFiles", Any).Invoke(saver, null);
                foreach (string entry in files.Where(n => n.EndsWith(".json", StringComparison.Ordinal)).ToArray())
                {
                    var text = (string)saverType.GetMethod("ReadJson", Any).Invoke(saver, new object[] { entry.Substring(0, entry.Length - 5) });
                    if (text == null || !wanted.Any(text.Contains)) continue;
                    var reader = new JsonTextReader(new StringReader(text)) { DateParseHandling = DateParseHandling.None, FloatParseHandling = FloatParseHandling.Decimal };
                    foreach (JObject unit in ((JContainer)JToken.ReadFrom(reader)).Descendants().OfType<JObject>()
                        .Where(o => o["UniqueId"] is JValue && wanted.Contains((string)o["UniqueId"])))
                    {
                        JObject part = unit.Descendants().OfType<JObject>().FirstOrDefault(o => o["_records"] != null);
                        if (part == null || found[(string)unit["UniqueId"]] != null) continue;
                        found[(string)unit["UniqueId"]] = new JObject { ["entry"] = entry, ["_records"] = part["_records"], ["_rejectedRecords"] = part["_rejectedRecords"] };
                    }
                }
            }
            finally { saverType.GetMethod("Dispose", Any).Invoke(saver, null); }
            return found;
        }

        private JObject RealRecord(UnitEntityData target, string score)
        {
            foreach (object record in (IEnumerable)BrownFurObserver.RecordsField.GetValue(Part(target)))
                if (record.GetType().GetProperty("AbilityScore", Any).GetValue(record, null).ToString() == score)
                    return (JObject)((JObject)jsonType.GetMethod("Write", Any).Invoke(null, new[] { record })).DeepClone();
            problems.Add(target.UniqueId + ": no real " + score + " record");
            return new JObject();
        }

        private static JObject With(JObject record, string member, JToken value)
        {
            var copy = (JObject)record.DeepClone();
            copy[member] = value;
            return copy;
        }

        private readonly Dictionary<object, Tuple<object, object>> productValues = new Dictionary<object, Tuple<object, object>>();

        private void Arm(UnitEntityData target, JObject record, string name, JToken[] entries)
        {
            object part = Part(target);
            productValues[part] = Tuple.Create(BrownFurObserver.RecordsField.GetValue(part), BrownFurObserver.RejectedField.GetValue(part));
            Type listType = BrownFurObserver.RecordsField.FieldType;
            var list = (IList)Activator.CreateInstance(listType);
            MethodInfo read = jsonType.GetMethod("Read", Any);
            foreach (JToken entry in entries) list.Add(read.Invoke(null, new object[] { entry }));
            BrownFurObserver.Injections[part] = Tuple.Create((object)list, (object)null);
            cleanups.Add(() => RestoreAfterSave());
            record[name] = new JObject { ["owner"] = target.UniqueId, ["synthetic"] = new JArray(entries.Select(e => (object)e.DeepClone()).ToArray()) };
        }

        private void RestoreAfterSave()
        {
            foreach (KeyValuePair<object, Tuple<object, object>> pair in productValues)
            {
                BrownFurObserver.RecordsField.SetValue(pair.Key, pair.Value.Item1);
                BrownFurObserver.RejectedField.SetValue(pair.Key, pair.Value.Item2);
                BrownFurObserver.Injections.Remove(pair.Key);
            }
            productValues.Clear();
        }

        // ---- reload ----

        private IEnumerable<object> Reload()
        {
            foreach (object step in AwaitBrownFur()) yield return step;
            if (!ResolveBrownFur()) yield break;
            Dictionary<string, UnitEntityData> units = Units();
            if (units == null) yield break;
            JArray loadCalls = BrownFurObserver.CallsSince(0);
            var load = new JObject();
            foreach (string name in Targets) load[name] = LoadOrder(units[name]);
            result["load"] = load;
            ExpectLoaded(units, "loaded");
            if (problems.Count != 0) yield break;

            // Repeated production reads change nothing.
            var repeated = new JObject();
            foreach (string name in Targets)
            {
                JObject before = PartState(units[name]);
                for (int index = 0; index < 3; index++)
                {
                    PartCount(units[name]);
                    PartRetained(units[name]);
                }
                JObject after = PartState(units[name]);
                repeated[name] = after;
                if (!JToken.DeepEquals(before, after)) problems.Add(name + ": repeated reads changed the part: " + before.ToString(Formatting.None) + " -> " + after.ToString(Formatting.None));
            }
            result["repeated"] = repeated;
            if (problems.Count != 0) yield break;

            UnitEntityData caster = units["caster"];
            var mutations = new JObject();
            // A new committed cast establishes current state for the withheld Strength modifier.
            JObject siblingsBefore = PartState(units["siblings"]);
            mutations["siblings.recast"] = CastAbilityBonus(caster, units["siblings"], BullsStrength, StatType.Strength, 0, 6);
            mutations["siblings.afterRecast"] = PartState(units["siblings"]);
            ExpectRetainedUnchanged(siblingsBefore, (JObject)mutations["siblings.afterRecast"], "siblings.recast");
            // Removal: the buff's removal forgets its active record; retained entries stay.
            JObject unsupportedBefore = PartState(units["unsupported"]);
            Buff[] strengthBuffs = units["unsupported"].Descriptor.Buffs.RawFacts.OfType<Buff>()
                .Where(b => b.Blueprint.AssetGuid == BullsStrengthBuff).ToArray();
            mutations["unsupported.buffsBeforeRemoval"] = strengthBuffs.Length;
            foreach (Buff buff in strengthBuffs) buff.Remove();
            yield return null;
            mutations["unsupported.afterRemoval"] = PartState(units["unsupported"]);
            mutations["unsupported.strengthAfterRemoval"] = Modifiers(units["unsupported"], StatType.Strength, BullsStrength);
            ExpectRetainedUnchanged(unsupportedBefore, (JObject)mutations["unsupported.afterRemoval"], "unsupported.removal");
            // A forced refusal of a new write, on a part that holds an active and a retained record.
            mutations["exact.refusedWrite"] = RefusedWrite(caster, units["exact"]);
            result["mutations"] = mutations;
            ExpectMutated(units, "mutated");
        }

        private JObject RefusedWrite(UnitEntityData caster, UnitEntityData target)
        {
            object part = Part(target);
            JObject before = PartState(target);
            JObject strengthBefore = Modifiers(target, StatType.Strength, BullsStrength);
            refuseExisting = new HashSet<object>(new ReferenceComparer());
            foreach (object entry in (IEnumerable)BrownFurObserver.RecordsField.GetValue(part)) refuseExisting.Add(entry);
            foreach (object entry in (IEnumerable)BrownFurObserver.RejectedField.GetValue(part) ?? new object[0]) refuseExisting.Add(entry);
            refusedCount = 0;
            refusedRecords.Clear();
            refuseOwner = part;
            MethodInfo validate = product.GetType("KingmakerGunslinger.BrownFur.BrownFurPersistedModifierPolicy", true).GetMethod("Validate", Any);
            harmony.Patch(validate, new HarmonyMethod(typeof(BrownFurProbe).GetMethod(nameof(RefusePrefix), BindingFlags.Static | BindingFlags.NonPublic)), null);
            JObject cast;
            try { cast = CastAbilityBonus(caster, target, CatsGrace, StatType.Dexterity, 1, 4); }
            finally
            {
                refuseOwner = null;
                harmony.Unpatch(validate, HarmonyPatchType.Prefix, harmony.Id);
            }
            var row = new JObject
            {
                ["forced"] = "BrownFurPersistedModifierPolicy.Validate throws for the new record (not already in the part) while armed",
                ["refusedCount"] = refusedCount, ["refusedRecords"] = new JArray(refusedRecords.Cast<object>().ToArray()),
                ["cast"] = cast, ["before"] = before, ["after"] = PartState(target),
                ["strengthBefore"] = strengthBefore, ["strengthAfter"] = Modifiers(target, StatType.Strength, BullsStrength)
            };
            if (refusedCount < 1) problems.Add("exact: the forced refusal never fired");
            if (!JToken.DeepEquals(before, row["after"])) problems.Add("exact: the refused write changed the part: " + before.ToString(Formatting.None) + " -> " + row["after"].ToString(Formatting.None));
            if (!JToken.DeepEquals(strengthBefore, row["strengthAfter"])) problems.Add("exact: the refused write changed the Strength modifier");
            return row;
        }

        private static bool RefusePrefix(object record)
        {
            if (refuseOwner == null || record == null || refuseExisting.Contains(record)) return true;
            refusedCount++;
            if (refusedRecords.Count < 8) refusedRecords.Add(BrownFurObserver.Entry(record).ToString(Formatting.None));
            throw new InvalidOperationException("SYNTHETIC refused Powerful Change persistence write (A2 qualification).");
        }

        // ---- final ----

        private IEnumerable<object> Final()
        {
            foreach (object step in AwaitBrownFur()) yield return step;
            if (!ResolveBrownFur()) yield break;
            Dictionary<string, UnitEntityData> units = Units();
            if (units == null) yield break;
            var load = new JObject();
            foreach (string name in Targets) load[name] = LoadOrder(units[name]);
            result["load"] = load;
            ExpectMutated(units, "final");
        }

        // ---- expectations ----

        private void ExpectLoaded(Dictionary<string, UnitEntityData> units, string phase)
        {
            ExpectPart(units["valid"], "valid." + phase, 1, 0);
            ExpectPart(units["siblings"], "siblings." + phase, 1, 2);
            ExpectPart(units["conflict"], "conflict." + phase, 0, 2);
            ExpectPart(units["exact"], "exact." + phase, 1, 1);
            ExpectPart(units["unsupported"], "unsupported." + phase, 1, 3);
            ExpectPart(units["unattributable"], "unattributable." + phase, 0, 2);
            var stats = new JObject();
            ExpectModifier(units["valid"], stats, "valid", StatType.Strength, BullsStrength, 6);
            ExpectModifier(units["siblings"], stats, "siblings.strength", StatType.Strength, BullsStrength, 4);
            ExpectModifier(units["siblings"], stats, "siblings.dexterity", StatType.Dexterity, CatsGrace, 6);
            ExpectModifier(units["conflict"], stats, "conflict", StatType.Strength, BullsStrength, 4);
            ExpectModifier(units["exact"], stats, "exact", StatType.Strength, BullsStrength, 6);
            ExpectModifier(units["unsupported"], stats, "unsupported", StatType.Strength, BullsStrength, 6);
            ExpectModifier(units["unattributable"], stats, "unattributable", StatType.Strength, BullsStrength, 4);
            result["stats." + phase] = stats;
        }

        private void ExpectMutated(Dictionary<string, UnitEntityData> units, string phase)
        {
            ExpectPart(units["valid"], "valid." + phase, 1, 0);
            ExpectPart(units["siblings"], "siblings." + phase, 2, 2);
            ExpectPart(units["conflict"], "conflict." + phase, 0, 2);
            ExpectPart(units["exact"], "exact." + phase, 1, 1);
            ExpectPart(units["unsupported"], "unsupported." + phase, 0, 3);
            ExpectPart(units["unattributable"], "unattributable." + phase, 0, 2);
            var stats = new JObject();
            ExpectModifier(units["valid"], stats, "valid", StatType.Strength, BullsStrength, 6);
            ExpectModifier(units["siblings"], stats, "siblings.strength", StatType.Strength, BullsStrength, 6);
            ExpectModifier(units["siblings"], stats, "siblings.dexterity", StatType.Dexterity, CatsGrace, 6);
            ExpectModifier(units["conflict"], stats, "conflict", StatType.Strength, BullsStrength, 4);
            ExpectModifier(units["exact"], stats, "exact.strength", StatType.Strength, BullsStrength, 6);
            ExpectModifier(units["exact"], stats, "exact.dexterity", StatType.Dexterity, CatsGrace, 4);
            ExpectModifier(units["unsupported"], stats, "unsupported", StatType.Strength, BullsStrength, 0);
            ExpectModifier(units["unattributable"], stats, "unattributable", StatType.Strength, BullsStrength, 4);
            result["stats." + phase] = stats;
        }

        private void ExpectPart(UnitEntityData unit, string name, int active, int retained)
        {
            JObject state = PartState(unit);
            result["part." + name] = state;
            if ((int)state["count"] != active || (int)state["retainedCount"] != retained)
                problems.Add(name + ": active " + state["count"] + " / retained " + state["retainedCount"] + ", expected " + active + " / " + retained);
        }

        private void ExpectModifier(UnitEntityData unit, JObject record, string name, StatType stat, string spell, int value)
        {
            JObject observed = Modifiers(unit, stat, spell);
            record[name] = observed;
            if (value == 0 ? (int)observed["count"] != 0 :
                (int)observed["count"] != 1 || (int)observed["value"] != value || (string)observed["descriptor"] != "Enhancement")
                problems.Add(name + ": " + stat + " modifier " + observed.ToString(Formatting.None) + ", expected " + (value == 0 ? "none" : "one +" + value + " Enhancement"));
        }

        private void ExpectRetainedUnchanged(JObject before, JObject after, string name)
        {
            if (!JToken.DeepEquals(before["rejected"], after["rejected"]))
                problems.Add(name + ": retained entries changed: " + before["rejected"].ToString(Formatting.None) + " -> " + after["rejected"].ToString(Formatting.None));
        }

        private JObject LoadOrder(UnitEntityData unit)
        {
            object part = Part(unit);
            int id = part == null ? -1 : RuntimeHelpers.GetHashCode(part);
            JObject[] own = BrownFurObserver.Calls.Where(c => (int)c["part"] == id).ToArray();
            JObject postLoad = own.FirstOrDefault(c => (string)c["kind"] == "PostLoad");
            JObject firstResolve = own.FirstOrDefault(c => (string)c["kind"] == "ResolveIncrease");
            return new JObject
            {
                ["calls"] = new JArray(own.Take(12).Cast<object>().ToArray()),
                ["firstConsumer"] = own.Length == 0 ? "none" : (string)own[0]["kind"],
                ["resolveBeforePostLoad"] = firstResolve != null && postLoad != null && (int)firstResolve["seq"] < (int)postLoad["seq"],
                ["postLoadCompleted"] = postLoad != null && (bool)postLoad["completed"],
                ["fileRecords"] = own.Length == 0 ? null : own[0]["recordsBefore"],
                ["fileRejected"] = own.Length == 0 ? null : own[0]["rejectedBefore"]
            };
        }

        private JObject PartState(UnitEntityData unit)
        {
            object part = Part(unit);
            if (part == null) return new JObject { ["part"] = false, ["count"] = 0, ["retainedCount"] = 0 };
            return new JObject
            {
                ["part"] = true, ["count"] = PartCount(unit), ["retainedCount"] = PartRetained(unit),
                ["records"] = BrownFurObserver.Describe(BrownFurObserver.RecordsField.GetValue(part)),
                ["rejected"] = BrownFurObserver.Describe(BrownFurObserver.RejectedField.GetValue(part))
            };
        }

        private static int PartCount(UnitEntityData unit) =>
            (int)BrownFurObserver.PartType.GetProperty("Count", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Part(unit), null);

        private static int PartRetained(UnitEntityData unit) =>
            (int)BrownFurObserver.PartType.GetProperty("RetainedCount", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Part(unit), null);

        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            public new bool Equals(object x, object y) => ReferenceEquals(x, y);
            public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
        }
    }
}
