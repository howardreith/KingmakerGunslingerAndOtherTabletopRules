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
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Buffs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KmgLoaderCompatHarness
{
    /// <summary>
    /// A2 qualification modes ("create", "reload", "final") on the corrected product. Seven disposable targets receive
    /// real committed Powerful Change casts from the disposable Arcanist. At the save, after the product's own PreSave,
    /// the observer writes SYNTHETIC entries, given as raw JSON text, beside the real records on six of them. The
    /// expected active and retained content of every target is written to a manifest as plain strings before the
    /// product sees the entries; the written save file must contain every injected text verbatim; each fresh load must
    /// reproduce the expected texts exactly. All comparisons are ordinal string comparisons of the product's own
    /// entry text, so a repeated member, a number's digits or a metadata member cannot vanish on both sides.
    /// </summary>
    internal sealed partial class BrownFurProbe
    {
        private static readonly string[] Targets = { "valid", "siblings", "conflict", "exact", "unsupported", "unattributable", "repeatedMember" };
        private const string OtherBuff = "0123456789abcdef0123456789abcdef";
        private const string ExpectedFile = "brown-fur-persistence-expected.json";

        private static object refuseOwner;
        private static int refusedCount;
        private static readonly List<string> refusedRecords = new List<string>();
        private static HashSet<object> refuseExisting;

        private Type jsonType;

        private IEnumerable<object> Qualification()
        {
            jsonType = product.GetType("KingmakerGunslinger.BrownFur.BrownFurPersistedModifierJson", true);
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

        // ---- raw text helpers (strings only; nothing passes through JObject) ----

        private static string Extend(string record, string members) => record.Substring(0, record.Length - 1) + "," + members + "}";

        private static string Rebuff(string record, string buffGuid) =>
            System.Text.RegularExpressions.Regex.Replace(record, "\"BuffGuid\":\"[0-9a-f]{32}\"", "\"BuffGuid\":\"" + buffGuid + "\"");

        private static string Member(string record, string member, string value) =>
            System.Text.RegularExpressions.Regex.Replace(record, "\"" + member + "\":(\"[^\"]*\"|-?[0-9]+)", "\"" + member + "\":" + value);

        private string Text(object record) => (string)jsonType.GetMethod("Text", Any).Invoke(null, new[] { record });

        private string[] Texts(object list) =>
            list == null ? new string[0] : ((IEnumerable)list).Cast<object>().Select(Text).ToArray();

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

            // Each target's real record text as the product writes it, then SYNTHETIC raw entries around it.
            Func<string, string, string> real = (name, score) => RealRecordText(targets[name], score);
            string str = real("siblings", "Strength"), dex = real("siblings", "Dexterity");
            var injected = new Dictionary<string, string[]>
            {
                ["siblings"] = new[] { dex, str, Member(str, "AbilityScore", "\"Strenght\"") },
                ["conflict"] = new[] { real("conflict", "Strength"), Member(real("conflict", "Strength"), "Increase", "4") },
                ["exact"] = new[] { real("exact", "Strength"), real("exact", "Strength") },
                ["unsupported"] = new[] {
                    real("unsupported", "Strength"),
                    Rebuff(Member(real("unsupported", "Strength"), "SchemaVersion", "2"), OtherBuff),
                    Rebuff(Member(real("unsupported", "Strength"), "AbilityScore", "99"), OtherBuff),
                    Rebuff(Extend(real("unsupported", "Strength"), "\"FutureField\":{\"nested\":[1,\"two\",true,null,1.5," +
                        "1e100,0.1000000000000000055511151231257827,123456789012345678901234567890,1e400,\"2026-11-22T10:00:00+02:00\"]}"), OtherBuff),
                    Rebuff(real("unsupported", "Strength").Replace("\"CarrierFamily\":\"AddStatBonus\"",
                        "\"CarrierFamily\":\"AddStatBonus\",\"CarrierFamily\":\"AddStatBonus\""), OtherBuff),
                    "{\"$id\":\"r\",\"SchemaVersion\":2,\"BuffGuid\":\"" + OtherBuff + "\",\"Link\":{\"$ref\":\"r\"}}" },
                ["unattributable"] = new[] { real("unattributable", "Strength"), "\"garbage\"" },
                ["repeatedMember"] = new[] { real("repeatedMember", "Strength").Replace("\"Increase\":2,", "\"Increase\":2,\"Increase\":4,") }
            };
            if (injected["repeatedMember"][0] == real("repeatedMember", "Strength")) problems.Add("repeatedMember: the fixture builder did not repeat the member");

            // Expected content after the first fresh load, from the injected strings and the reviewed policy.
            var expected = new JObject();
            Action<string, string[], string[]> expect = (name, active, retained) =>
                expected[name] = new JObject { ["active"] = new JArray(active.Cast<object>().ToArray()), ["retained"] = new JArray(retained.Cast<object>().ToArray()) };
            string[] u = injected["unsupported"];
            expect("valid", new[] { real("valid", "Strength") }, new string[0]);
            expect("siblings", new[] { dex }, new[] { str, injected["siblings"][2] });
            expect("conflict", new string[0], injected["conflict"]);
            expect("exact", new[] { injected["exact"][0] }, new[] { injected["exact"][1] });
            expect("unsupported", new[] { u[0] }, u.Skip(1).ToArray());
            expect("unattributable", new string[0], injected["unattributable"]);
            expect("repeatedMember", new string[0], injected["repeatedMember"]);
            var manifest = new JObject
            {
                ["fixture"] = "KMCS_AFTER_NORMAL_012_MODONLY.zks", ["units"] = units,
                ["injected"] = new JObject(injected.Select(pair => new JProperty(pair.Key, new JArray(pair.Value.Cast<object>().ToArray())))), ["expectedLoaded"] = expected
            };
            File.WriteAllText(Path.Combine(profileDir, ExpectedFile), manifest.ToString(Formatting.Indented));
            File.WriteAllText(Path.Combine(profileDir, FixtureFile), new JObject { ["units"] = units }.ToString());
            result["expectedManifest"] = manifest;

            var injections = new JObject();
            foreach (KeyValuePair<string, string[]> pair in injected) Arm(targets[pair.Key], injections, pair.Key, pair.Value);
            result["syntheticInjections"] = injections;
            if (problems.Count != 0) yield break;

            int callIndex = BrownFurObserver.Calls.Count;
            foreach (object step in Save()) yield return step;
            JArray calls = BrownFurObserver.CallsSince(callIndex);
            result["saveCalls"] = calls;
            RestoreAfterSave();
            foreach (string name in Targets)
            {
                string owner = (string)units[name];
                bool armed = injected.ContainsKey(name);
                if (!calls.OfType<JObject>().Any(c => (string)c["kind"] == "PreSave" && (string)c["owner"] == owner && (bool)c["completed"] &&
                        (!armed || c["syntheticArmed"] != null)))
                    problems.Add(name + ": the save's PreSave did not complete" + (armed ? " with the synthetic injection" : ""));
            }

            // The written file must hold every injected text verbatim and in order as one target's _records member:
            // each expected member text must occur exactly once in the save's JSON text.
            string saved = SavedText();
            var occurrences = new JObject();
            foreach (KeyValuePair<string, string[]> pair in injected.Concat(new[] {
                new KeyValuePair<string, string[]>("valid", new[] { real("valid", "Strength") }) }))
            {
                string member = "\"_records\":[" + string.Join(",", pair.Value) + "]";
                int count = Occurrences(saved, member);
                occurrences[pair.Key] = count;
                if (count != 1) problems.Add(pair.Key + ": the written save holds the expected _records text " + count + " times, expected once: " + member);
            }
            result["writtenOccurrences"] = occurrences;
            result["serializerMatrix"] = QualifiedMatrix(real("valid", "Strength"));
            result["serializerSettings"] = SettingsDescription();
        }

        private string RealRecordText(UnitEntityData target, string score)
        {
            foreach (object record in (IEnumerable)BrownFurObserver.RecordsField.GetValue(Part(target)))
                if (record.GetType().GetProperty("AbilityScore", Any).GetValue(record, null).ToString() == score)
                    return Text(record);
            problems.Add(target.UniqueId + ": no real " + score + " record");
            return "{}";
        }

        private readonly Dictionary<object, Tuple<object, object>> productValues = new Dictionary<object, Tuple<object, object>>();

        private void Arm(UnitEntityData target, JObject record, string name, string[] entries)
        {
            object part = Part(target);
            productValues[part] = Tuple.Create(BrownFurObserver.RecordsField.GetValue(part), BrownFurObserver.RejectedField.GetValue(part));
            var list = (IList)Activator.CreateInstance(BrownFurObserver.RecordsField.FieldType);
            MethodInfo read = jsonType.GetMethod("ReadText", Any);
            foreach (string entry in entries) list.Add(read.Invoke(null, new object[] { entry }));
            string[] roundTrip = Texts(list);
            if (!roundTrip.SequenceEqual(entries))
                problems.Add(name + ": the product's entries do not reproduce the injected text: " + string.Join(" | ", roundTrip));
            BrownFurObserver.Injections[part] = Tuple.Create((object)list, (object)null);
            cleanups.Add(() => RestoreAfterSave());
            record[name] = new JObject { ["owner"] = target.UniqueId, ["synthetic"] = new JArray(entries.Cast<object>().ToArray()) };
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

        /// <summary>The JSON text of every entry of the written save, as the game wrote it.</summary>
        private string SavedText()
        {
            string path = Directory.GetFiles(Game.Instance.SaveManager.SavePath, "*_" + saveLabel + ".zks")
                .OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault();
            if (path == null) { problems.Add("the written save " + saveLabel + " was not found"); return string.Empty; }
            result["writtenSave"] = Path.GetFileName(path);
            Type saverType = typeof(Kingmaker.EntitySystem.Persistence.SaveManager).Assembly.GetType("Kingmaker.EntitySystem.Persistence.ZipSaver", true);
            object saver = Activator.CreateInstance(saverType, path);
            var all = new System.Text.StringBuilder();
            try
            {
                var files = (IEnumerable<string>)saverType.GetMethod("GetAllFiles", Any).Invoke(saver, null);
                foreach (string entry in files.Where(n => n.EndsWith(".json", StringComparison.Ordinal)).ToArray())
                    all.Append((string)saverType.GetMethod("ReadJson", Any).Invoke(saver, new object[] { entry.Substring(0, entry.Length - 5) })).Append(' ');
            }
            finally { saverType.GetMethod("Dispose", Any).Invoke(saver, null); }
            return all.ToString();
        }

        private static int Occurrences(string text, string value)
        {
            int count = 0;
            for (int index = text.IndexOf(value, StringComparison.Ordinal); index >= 0; index = text.IndexOf(value, index + 1, StringComparison.Ordinal)) count++;
            return count;
        }

        // ---- corrected serializer matrix: explicit expected outcome for every row ----

        private JObject QualifiedMatrix(string record)
        {
            matrixIdentity = JObject.Parse(record);
            string id = "{\"$id\":\"1235\"," + record.Substring(1);
            Func<string, string> other = r => Rebuff(r, OtherBuff);
            var rows = new List<Tuple<string, string, string, int, int, string[]>>
            {
                // name, raw _records value, expected deserialize, active, retained, texts that must appear verbatim in the rewrite
                Row("valid-as-written", "[" + id + "]", 1, 0, record),
                Row("valid-without-id", "[" + record + "]", 1, 0, record),
                Row("unknown-member", "[" + Extend(record, "\"FutureField\":{\"nested\":[1,\"two\",true,null,1.5]}") + "]", 0, 1,
                    Extend(record, "\"FutureField\":{\"nested\":[1,\"two\",true,null,1.5]}")),
                Row("schema-2", "[" + Member(record, "SchemaVersion", "2") + "]", 0, 1, Member(record, "SchemaVersion", "2")),
                Row("ability-name-unknown", "[" + Member(record, "AbilityScore", "\"Strenght\"") + "]", 0, 1, Member(record, "AbilityScore", "\"Strenght\"")),
                Row("ability-name-lowercase", "[" + Member(record, "AbilityScore", "\"strength\"") + "]", 0, 1, Member(record, "AbilityScore", "\"strength\"")),
                Row("ability-number-undefined", "[" + Member(record, "AbilityScore", "99") + "]", 0, 1, Member(record, "AbilityScore", "99")),
                Row("ability-number-defined", "[" + Member(record, "AbilityScore", "1") + "]", 0, 1, Member(record, "AbilityScore", "1")),
                Row("increase-string", "[" + Member(record, "Increase", "\"four\"") + "]", 0, 1, Member(record, "Increase", "\"four\"")),
                Row("ticks-float", "[" + Member(record, "EndTimeTicks", "1.5") + "]", 0, 1, Member(record, "EndTimeTicks", "1.5")),
                Row("element-string", "[\"garbage\"]", 0, 1, "\"garbage\""),
                Row("element-null", "[null]", 0, 1, "null"),
                Row("container-object", "{\"unexpected\":true}", 0, 1, "{\"unexpected\":true}"),
                Row("duplicate-exact", "[" + record + "," + record + "]", 1, 1, record),
                Row("duplicate-conflicting", "[" + record + "," + Member(record, "Increase", "4") + "]", 0, 2, record, Member(record, "Increase", "4")),
                Row("raw-exponent-1e100", "[" + record + "," + other(Extend(record, "\"FutureField\":1e100")) + "]", 1, 1, record, other(Extend(record, "\"FutureField\":1e100"))),
                Row("raw-high-precision", "[" + other(Extend(record, "\"FutureField\":0.1000000000000000055511151231257827")) + "]", 0, 1,
                    other(Extend(record, "\"FutureField\":0.1000000000000000055511151231257827"))),
                Row("raw-int64-overflow-nested", "[" + other(Extend(record, "\"FutureField\":123456789012345678901234567890")) + "]", 0, 1,
                    other(Extend(record, "\"FutureField\":123456789012345678901234567890"))),
                Row("raw-double-overflow-nested", "[" + other(Extend(record, "\"FutureField\":1e400")) + "]", 0, 1, other(Extend(record, "\"FutureField\":1e400"))),
                Row("raw-repeated-conflicting", "[" + record.Replace("\"Increase\":2,", "\"Increase\":2,\"Increase\":4,") + "]", 0, 1,
                    record.Replace("\"Increase\":2,", "\"Increase\":2,\"Increase\":4,")),
                Row("raw-repeated-identical", "[" + record.Replace("\"Increase\":2,", "\"Increase\":2,\"Increase\":2,") + "]", 0, 1,
                    record.Replace("\"Increase\":2,", "\"Increase\":2,\"Increase\":2,")),
                Row("raw-metadata-reference", "[" + record + ",{\"$id\":\"r\",\"SchemaVersion\":2,\"BuffGuid\":\"" + OtherBuff + "\",\"Link\":{\"$ref\":\"r\"}}]", 1, 1,
                    record, "{\"$id\":\"r\",\"SchemaVersion\":2,\"BuffGuid\":\"" + OtherBuff + "\",\"Link\":{\"$ref\":\"r\"}}"),
                Row("scalar-date-string", "\"2026-11-22T10:00:00+02:00\"", 0, 1, "\"2026-11-22T10:00:00+02:00\""),
                Row("scalar-exponent", "1e100", 0, 1, "1e100"),
                // Documented limit: the engine reader throws on this member value before any member converter runs.
                Tuple.Create("scalar-int64-overflow", "123456789012345678901234567890", "throws", -1, -1, new string[0])
            };
            var matrix = new JObject();
            foreach (var row in rows)
            {
                JObject observed = DeserializeText("{\"_records\":" + row.Item2 + "}");
                observed["expected"] = new JObject { ["deserialize"] = row.Item3, ["active"] = row.Item4, ["retained"] = row.Item5 };
                matrix[row.Item1] = observed;
                string outcome = (string)observed["deserialize"];
                if (row.Item3 == "throws")
                {
                    if (!outcome.StartsWith("threw", StringComparison.Ordinal)) problems.Add("matrix " + row.Item1 + ": the documented engine limit no longer throws: " + outcome);
                    continue;
                }
                if (outcome != "ok") { problems.Add("matrix " + row.Item1 + ": " + outcome); continue; }
                if ((int?)observed["active"] != row.Item4 || (int?)observed["retained"] != row.Item5)
                    problems.Add("matrix " + row.Item1 + ": active/retained " + observed["active"] + "/" + observed["retained"] + ", expected " + row.Item4 + "/" + row.Item5);
                if ((int?)observed["increase"] != (row.Item4 == 0 ? 0 : 2))
                    problems.Add("matrix " + row.Item1 + ": restored increase " + observed["increase"]);
                string rewritten = (string)observed["normalizedRewrite"];
                foreach (string text in row.Item6)
                    if (rewritten == null || !rewritten.Contains(text)) problems.Add("matrix " + row.Item1 + ": the rewrite does not contain " + text);
            }
            return matrix;
        }

        private static Tuple<string, string, string, int, int, string[]> Row(string name, string value, int active, int retained, params string[] preserved) =>
            Tuple.Create(name, value, "ok", active, retained, preserved);

        /// <summary>
        /// Raw document text into the product's part type with the engine's DefaultJsonSettings; then the owner's reads,
        /// a normalization (the part's own PreSave path) and a rewrite by the same serializer.
        /// </summary>
        private JObject DeserializeText(string document)
        {
            var row = new JObject { ["input"] = document };
            JsonSerializer serializer = JsonSerializer.Create(Kingmaker.EntitySystem.Persistence.JsonUtility.DefaultJsonSettings.DefaultSettings);
            object part;
            try
            {
                using (var reader = new JsonTextReader(new StringReader(document)))
                    part = serializer.Deserialize(reader, BrownFurObserver.PartType);
            }
            catch (Exception exception)
            {
                row["deserialize"] = "threw " + exception.GetType().FullName + ": " + (exception.Message.Length > 300 ? exception.Message.Substring(0, 300) : exception.Message);
                return row;
            }
            row["deserialize"] = part == null ? "null" : "ok";
            if (part == null) return row;
            try
            {
                row["active"] = (int)BrownFurObserver.PartType.GetProperty("Count", Any).GetValue(part, null);
                row["retained"] = (int)BrownFurObserver.PartType.GetProperty("RetainedCount", Any).GetValue(part, null);
                row["increase"] = ProbeIncrease(part, document);
                // The owner's own normalization, as PreSave performs it, then the rewrite.
                MethodInfo read = BrownFurObserver.PartType.GetMethod("Read", Any), write = BrownFurObserver.PartType.GetMethod("Write", Any);
                write.Invoke(part, new[] { read.Invoke(part, null) });
                var text = new StringWriter(CultureInfo.InvariantCulture);
                serializer.Serialize(text, part, BrownFurObserver.PartType);
                row["normalizedRewrite"] = text.ToString();
            }
            catch (Exception exception)
            {
                Exception inner = exception is TargetInvocationException && exception.InnerException != null ? exception.InnerException : exception;
                row["ownerRead"] = "threw " + inner.GetType().FullName + ": " + inner.Message;
                problems.Add("matrix owner read threw for " + document.Substring(0, Math.Min(120, document.Length)) + ": " + inner.Message);
            }
            return row;
        }

        /// <summary>ResolveIncrease for the valid record's exact identity, built from the matrix's real record text.</summary>
        private int ProbeIncrease(object part, string document)
        {
            Type probeType = product.GetType("KingmakerGunslinger.BrownFur.BrownFurPersistedModifierProbe", true);
            object probe = Activator.CreateInstance(probeType, true);
            Action<string, object> set = (name, value) => probeType.GetProperty(name, Any).SetValue(probe, value, null);
            set("BuffGuid", (string)matrixIdentity["BuffGuid"]);
            set("SpellGuid", (string)matrixIdentity["SpellGuid"]);
            set("CasterId", (string)matrixIdentity["CasterId"]);
            set("AbilityScore", Enum.Parse(product.GetType("KingmakerGunslinger.BrownFur.BrownFurAbilityScore", true), (string)matrixIdentity["AbilityScore"]));
            set("OriginalValue", (int)(long)matrixIdentity["OriginalValue"]);
            set("OriginalDescriptor", (string)matrixIdentity["OriginalDescriptor"]);
            set("CarrierFamily", (string)matrixIdentity["CarrierFamily"]);
            set("EndTimeTicks", (long)matrixIdentity["EndTimeTicks"]);
            return (int)BrownFurObserver.PartType.GetMethod("ResolveIncrease", Any).Invoke(part, new[] { probe });
        }

        private JObject matrixIdentity;

        // ---- reload ----

        private IEnumerable<object> Reload()
        {
            foreach (object step in AwaitBrownFur()) yield return step;
            if (!ResolveBrownFur()) yield break;
            Dictionary<string, UnitEntityData> units = Units();
            JObject manifest = Manifest();
            if (units == null || manifest == null) yield break;
            var load = new JObject();
            foreach (string name in Targets) load[name] = LoadOrder(units[name], name);
            result["load"] = load;
            ExpectContent(units, (JObject)manifest["expectedLoaded"], "loaded");
            ExpectLoaded(units, "loaded");
            if (problems.Count != 0) yield break;

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
            JObject siblingsBefore = PartState(units["siblings"]);
            mutations["siblings.recast"] = CastAbilityBonus(caster, units["siblings"], BullsStrength, StatType.Strength, 0, 6);
            mutations["siblings.afterRecast"] = PartState(units["siblings"]);
            ExpectRetainedUnchanged(siblingsBefore, (JObject)mutations["siblings.afterRecast"], "siblings.recast");
            JObject unsupportedBefore = PartState(units["unsupported"]);
            Buff[] strengthBuffs = units["unsupported"].Descriptor.Buffs.RawFacts.OfType<Buff>()
                .Where(b => b.Blueprint.AssetGuid == BullsStrengthBuff).ToArray();
            mutations["unsupported.buffsBeforeRemoval"] = strengthBuffs.Length;
            foreach (Buff buff in strengthBuffs) buff.Remove();
            yield return null;
            mutations["unsupported.afterRemoval"] = PartState(units["unsupported"]);
            mutations["unsupported.strengthAfterRemoval"] = Modifiers(units["unsupported"], StatType.Strength, BullsStrength);
            ExpectRetainedUnchanged(unsupportedBefore, (JObject)mutations["unsupported.afterRemoval"], "unsupported.removal");
            mutations["exact.refusedWrite"] = RefusedWrite(caster, units["exact"]);
            result["mutations"] = mutations;
            ExpectMutated(units, "mutated");

            // Expected content for the final fresh load: the loaded expectation with the reviewed mutations applied.
            var final = (JObject)manifest["expectedLoaded"].DeepClone();
            string recast = Texts(BrownFurObserver.RecordsField.GetValue(Part(units["siblings"])))
                .FirstOrDefault(t => t.Contains("\"BuffGuid\":\"" + BullsStrengthBuff + "\""));
            if (recast == null) problems.Add("siblings: no active Strength record after the recast");
            else ((JArray)final["siblings"]["active"]).Add(recast);
            final["unsupported"]["active"] = new JArray();
            ExpectContent(units, final, "mutated");
            manifest["expectedFinal"] = final;
            File.WriteAllText(Path.Combine(profileDir, ExpectedFile), manifest.ToString(Formatting.Indented));
        }

        private JObject Manifest()
        {
            string path = Path.Combine(profileDir, ExpectedFile);
            if (!File.Exists(path)) { problems.Add("no expected-content manifest at " + path); return null; }
            JObject manifest = JObject.Parse(File.ReadAllText(path));
            result["expectedManifest"] = manifest;
            return manifest;
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
                ["forced"] = "BrownFurPersistedModifierPolicy.Validate throws for a record not already in the part while armed",
                ["refusedCount"] = refusedCount, ["refusedRecords"] = new JArray(refusedRecords.Cast<object>().ToArray()),
                ["cast"] = cast, ["before"] = before, ["after"] = PartState(target),
                ["strengthBefore"] = strengthBefore, ["strengthAfter"] = Modifiers(target, StatType.Strength, BullsStrength)
            };
            if (refusedCount != 1) problems.Add("exact: the forced refusal fired " + refusedCount + " times, expected exactly once");
            string refused = refusedRecords.FirstOrDefault() ?? string.Empty;
            if (!refused.Contains("\"SpellGuid\":\"" + CatsGrace + "\"") || !refused.Contains("\"AbilityScore\":\"Dexterity\"") ||
                !refused.Contains("\"CasterId\":\"" + caster.UniqueId + "\""))
                problems.Add("exact: the refused record is not the new Cat's Grace Dexterity record: " + refused);
            if (!JToken.DeepEquals(before, row["after"])) problems.Add("exact: the refused write changed the part: " + before.ToString(Formatting.None) + " -> " + row["after"].ToString(Formatting.None));
            if (!JToken.DeepEquals(strengthBefore, row["strengthAfter"])) problems.Add("exact: the refused write changed the Strength modifier");
            return row;
        }

        private static bool RefusePrefix(object record)
        {
            if (refuseOwner == null || record == null || refuseExisting.Contains(record)) return true;
            refusedCount++;
            if (refusedRecords.Count < 8)
                refusedRecords.Add((string)record.GetType().Assembly.GetType("KingmakerGunslinger.BrownFur.BrownFurPersistedModifierJson", true)
                    .GetMethod("Text", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new[] { record }));
            throw new InvalidOperationException("SYNTHETIC refused Powerful Change persistence write (A2 qualification).");
        }

        // ---- final ----

        private IEnumerable<object> Final()
        {
            foreach (object step in AwaitBrownFur()) yield return step;
            if (!ResolveBrownFur()) yield break;
            Dictionary<string, UnitEntityData> units = Units();
            JObject manifest = Manifest();
            if (units == null || manifest == null) yield break;
            if (manifest["expectedFinal"] == null) { problems.Add("the manifest holds no expected final content"); yield break; }
            var load = new JObject();
            foreach (string name in Targets) load[name] = LoadOrder(units[name], name);
            result["load"] = load;
            ExpectContent(units, (JObject)manifest["expectedFinal"], "final");
            ExpectMutated(units, "final");
        }

        // ---- expectations ----

        /// <summary>Ordinal comparison of every target's active and retained entry texts with the manifest.</summary>
        private void ExpectContent(Dictionary<string, UnitEntityData> units, JObject expected, string phase)
        {
            var observed = new JObject();
            foreach (string name in Targets)
            {
                object part = Part(units[name]);
                if (part == null) { problems.Add(name + "." + phase + ": no persistence part"); continue; }
                // The owner's normalization as every read performs it, without writing.
                object state = BrownFurObserver.PartType.GetMethod("Read", Any).Invoke(part, null);
                string[] active = Texts(state.GetType().GetProperty("Active", Any).GetValue(state, null));
                string[] retained = Texts(state.GetType().GetProperty("Retained", Any).GetValue(state, null));
                observed[name] = new JObject { ["active"] = new JArray(active.Cast<object>().ToArray()), ["retained"] = new JArray(retained.Cast<object>().ToArray()) };
                string[] wantActive = ((JArray)expected[name]["active"]).Select(t => (string)t).ToArray();
                string[] wantRetained = ((JArray)expected[name]["retained"]).Select(t => (string)t).ToArray();
                if (!active.SequenceEqual(wantActive, StringComparer.Ordinal))
                    problems.Add(name + "." + phase + ": active content differs: " + string.Join(" | ", active));
                if (!retained.SequenceEqual(wantRetained, StringComparer.Ordinal))
                    problems.Add(name + "." + phase + ": retained content differs: " + string.Join(" | ", retained));
            }
            result["content." + phase] = observed;
        }

        private void ExpectLoaded(Dictionary<string, UnitEntityData> units, string phase)
        {
            var stats = new JObject();
            ExpectModifier(units["valid"], stats, "valid", StatType.Strength, BullsStrength, 6);
            ExpectModifier(units["siblings"], stats, "siblings.strength", StatType.Strength, BullsStrength, 4);
            ExpectModifier(units["siblings"], stats, "siblings.dexterity", StatType.Dexterity, CatsGrace, 6);
            ExpectModifier(units["conflict"], stats, "conflict", StatType.Strength, BullsStrength, 4);
            ExpectModifier(units["exact"], stats, "exact", StatType.Strength, BullsStrength, 6);
            ExpectModifier(units["unsupported"], stats, "unsupported", StatType.Strength, BullsStrength, 6);
            ExpectModifier(units["unattributable"], stats, "unattributable", StatType.Strength, BullsStrength, 4);
            ExpectModifier(units["repeatedMember"], stats, "repeatedMember", StatType.Strength, BullsStrength, 4);
            result["stats." + phase] = stats;
        }

        private void ExpectMutated(Dictionary<string, UnitEntityData> units, string phase)
        {
            var stats = new JObject();
            ExpectModifier(units["valid"], stats, "valid", StatType.Strength, BullsStrength, 6);
            ExpectModifier(units["siblings"], stats, "siblings.strength", StatType.Strength, BullsStrength, 6);
            ExpectModifier(units["siblings"], stats, "siblings.dexterity", StatType.Dexterity, CatsGrace, 6);
            ExpectModifier(units["conflict"], stats, "conflict", StatType.Strength, BullsStrength, 4);
            ExpectModifier(units["exact"], stats, "exact.strength", StatType.Strength, BullsStrength, 6);
            ExpectModifier(units["exact"], stats, "exact.dexterity", StatType.Dexterity, CatsGrace, 4);
            ExpectModifier(units["unsupported"], stats, "unsupported", StatType.Strength, BullsStrength, 0);
            ExpectModifier(units["unattributable"], stats, "unattributable", StatType.Strength, BullsStrength, 4);
            ExpectModifier(units["repeatedMember"], stats, "repeatedMember", StatType.Strength, BullsStrength, 4);
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
            if (!JToken.DeepEquals(before["retainedTexts"], after["retainedTexts"]))
                problems.Add(name + ": retained entries changed: " + before["retainedTexts"].ToString(Formatting.None) + " -> " + after["retainedTexts"].ToString(Formatting.None));
        }

        /// <summary>
        /// The part's native PostLoad must have been observed and completed; the first consumer is recorded (the owner
        /// reads lazily and totally, so its results do not depend on that order).
        /// </summary>
        private JObject LoadOrder(UnitEntityData unit, string name)
        {
            object part = Part(unit);
            int id = part == null ? -1 : RuntimeHelpers.GetHashCode(part);
            JObject[] own = BrownFurObserver.Calls.Where(c => (int)c["part"] == id).ToArray();
            JObject[] postLoads = own.Where(c => (string)c["kind"] == "PostLoad").ToArray();
            JObject firstResolve = own.FirstOrDefault(c => (string)c["kind"] == "ResolveIncrease");
            var order = new JObject
            {
                ["calls"] = new JArray(own.Take(12).Cast<object>().ToArray()),
                ["firstConsumer"] = own.Length == 0 ? "none" : (string)own[0]["kind"],
                ["postLoadCalls"] = postLoads.Length,
                ["postLoadCompleted"] = postLoads.Length == 1 && (bool)postLoads[0]["completed"],
                ["resolveBeforePostLoad"] = firstResolve != null && postLoads.Length == 1 && (int)firstResolve["seq"] < (int)postLoads[0]["seq"]
            };
            if (part == null) problems.Add(name + ": no persistence part after load");
            else if (postLoads.Length != 1 || !(bool)postLoads[0]["completed"])
                problems.Add(name + ": " + postLoads.Length + " native PostLoad calls observed, expected exactly one completed");
            return order;
        }

        private JObject PartState(UnitEntityData unit)
        {
            object part = Part(unit);
            if (part == null) return new JObject { ["part"] = false, ["count"] = 0, ["retainedCount"] = 0 };
            return new JObject
            {
                ["part"] = true, ["count"] = PartCount(unit), ["retainedCount"] = PartRetained(unit),
                ["recordTexts"] = new JArray(Texts(BrownFurObserver.RecordsField.GetValue(part)).Cast<object>().ToArray()),
                ["retainedTexts"] = new JArray(Texts(BrownFurObserver.RejectedField.GetValue(part)).Cast<object>().ToArray())
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
