using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using KingmakerGunslinger.BrownFur;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace KingmakerGunslinger.DomainTests
{
    // S6 T4 / RISK-03 (A2): Brown-Fur Powerful Change persistence. Drives the
    // production converter and store through the game's own Newtonsoft 8 with
    // the engine's DefaultJsonSettings choices (StringEnumConverter, references
    // preserved for objects, automatic type names). The engine boundary itself
    // (its contract resolver, real save files, PreSave/PostLoad and the restored
    // modifiers) is covered by the loader-compat session risk03-bf-persist.
    internal static class BrownFurPersistenceRecoveryTests
    {
        // The record a real committed Bull's Strength cast (disposable CotW
        // Arcanist, Strength toggle on) wrote into a save file, exactly as read
        // back from Manual_1_bfchar1.zks in characterization session
        // 20261122T152012Z-owner-0.33.0-risk03-bf-characterize. The engine's
        // "$id" is part of the released form.
        internal const string Written = @"{""$id"":""1235"",""SchemaVersion"":1,""BuffGuid"":""b175001b42b1a02479881b72fe132116"",""SpellGuid"":""4c3d08935262b6544ae97599b3a9556d"",""CasterId"":""e9d20d9d-7ba5-4f71-a47c-d893e7505806"",""AbilityScore"":""Strength"",""Increase"":2,""OriginalValue"":4,""OriginalDescriptor"":""Enhancement"",""CarrierFamily"":""AddStatBonus"",""EndTimeTicks"":438309050000}";

        private const string Buff = "b175001b42b1a02479881b72fe132116";
        private const string Spell = "4c3d08935262b6544ae97599b3a9556d";

        [JsonObject(MemberSerialization.OptIn)]
        private sealed class Part
        {
            [JsonProperty]
            [JsonConverter(typeof(BrownFurPersistedModifierListConverter))]
            internal List<BrownFurPersistedModifierRecord> _records =
                new List<BrownFurPersistedModifierRecord>();

            [JsonProperty]
            [JsonConverter(typeof(BrownFurPersistedModifierListConverter))]
            internal List<BrownFurPersistedModifierRecord> _rejectedRecords;

            internal BrownFurPersistedModifierState Read()
            {
                return BrownFurPersistedModifierStore.Normalize(_records,
                    _rejectedRecords);
            }

            internal void Write(BrownFurPersistedModifierState state)
            {
                _records = state.Active;
                _rejectedRecords = state.Retained;
            }
        }

        // The released typed member, for downgrade checks.
        [JsonObject(MemberSerialization.OptIn)]
        private sealed class ReleasedPart
        {
            [JsonProperty]
            internal List<BrownFurPersistedModifierRecord> _records;
        }

        private static JsonSerializerSettings Settings()
        {
            return new JsonSerializerSettings {
                Converters = new List<JsonConverter> { new StringEnumConverter() },
                ContractResolver = new DefaultContractResolver(),
                TypeNameHandling = TypeNameHandling.Auto,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                DefaultValueHandling = DefaultValueHandling.Include,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize };
        }

        private static Part Load(string json)
        {
            return (Part)JsonSerializer.Create(Settings()).Deserialize(
                new JsonTextReader(new StringReader(json)), typeof(Part));
        }

        private static string Save(Part part)
        {
            var text = new StringWriter(CultureInfo.InvariantCulture);
            JsonSerializer.Create(Settings()).Serialize(text, part, typeof(Part));
            return text.ToString();
        }

        // Inspects written text without the reader turning date-like strings
        // into dates or decimals into doubles.
        private static JObject Parse(string json)
        {
            var reader = new JsonTextReader(new StringReader(json)) {
                DateParseHandling = DateParseHandling.None,
                FloatParseHandling = FloatParseHandling.Decimal };
            return (JObject)JToken.ReadFrom(reader);
        }

        private static JObject WrittenRecord()
        {
            return JObject.Parse(Written);
        }

        private static string Document(params JToken[] records)
        {
            return new JObject { ["_records"] = new JArray(records) }
                .ToString(Formatting.None);
        }

        private static JObject Without(JObject record, string member)
        {
            var copy = (JObject)record.DeepClone();
            copy.Remove(member);
            return copy;
        }

        private static JObject With(JObject record, string member, JToken value)
        {
            var copy = (JObject)record.DeepClone();
            copy.Remove("$id");
            copy[member] = value;
            return copy;
        }

        private static BrownFurPersistedModifierProbe Probe(
            BrownFurPersistedModifierRecord record)
        {
            return new BrownFurPersistedModifierProbe {
                BuffGuid = record.BuffGuid, SpellGuid = record.SpellGuid,
                CasterId = record.CasterId, AbilityScore = record.AbilityScore,
                OriginalValue = record.OriginalValue,
                OriginalDescriptor = record.OriginalDescriptor,
                CarrierFamily = record.CarrierFamily,
                EndTimeTicks = record.EndTimeTicks };
        }

        internal static void ReleasedRecordRestoresExactlyAndWritesTheReleasedForm()
        {
            Part part = Load(Document(WrittenRecord()));
            BrownFurPersistedModifierState state = part.Read();
            Assertions.True(state.Active.Count == 1 && state.Retained == null &&
                state.NewlyRejected.Count == 0,
                "The released record was not read as one active record.");
            BrownFurPersistedModifierRecord record = state.Active[0];
            JObject written = WrittenRecord();
            Assertions.True(record.BuffGuid == (string)written["BuffGuid"] &&
                record.SpellGuid == (string)written["SpellGuid"] &&
                record.CasterId == (string)written["CasterId"] &&
                record.AbilityScore.ToString() == (string)written["AbilityScore"] &&
                record.Increase == (int)written["Increase"] &&
                record.OriginalValue == (int)written["OriginalValue"] &&
                record.OriginalDescriptor == (string)written["OriginalDescriptor"] &&
                record.CarrierFamily == (string)written["CarrierFamily"] &&
                record.EndTimeTicks == (long)written["EndTimeTicks"],
                "The released record's meaning changed.");
            BrownFurPersistedModifierProbe probe = Probe(record);
            Assertions.Equal(record.Increase,
                BrownFurPersistedModifierPolicy.ResolveIncrease(state.Active, probe),
                "The exact modifier did not restore its increase.");
            foreach (Action<BrownFurPersistedModifierProbe> change in new Action<BrownFurPersistedModifierProbe>[] {
                p => p.BuffGuid = "0123456789abcdef0123456789abcdef",
                p => p.SpellGuid = "0123456789abcdef0123456789abcdef",
                p => p.CasterId = "another-caster",
                p => p.AbilityScore = BrownFurAbilityScore.Dexterity,
                p => p.OriginalValue++, p => p.OriginalDescriptor = "UntypedStackable",
                p => p.CarrierFamily = "AddContextStatBonus", p => p.EndTimeTicks++ })
            {
                BrownFurPersistedModifierProbe other = Probe(record);
                change(other);
                Assertions.Equal(0, BrownFurPersistedModifierPolicy.ResolveIncrease(
                    state.Active, other),
                    "A different buff, spell, caster, score, value, descriptor, carrier or lifetime inherited the increase.");
            }

            part.Write(state);
            JToken saved = Parse(Save(part))["_records"];
            Assertions.True(saved is JArray && ((JArray)saved).Count == 1 &&
                JToken.DeepEquals(saved[0], Without(written, "$id")),
                "The active record was not written in its released form: " + saved);
            ReleasedPart released = (ReleasedPart)JsonSerializer.Create(Settings())
                .Deserialize(new JsonTextReader(new StringReader(Save(part))),
                    typeof(ReleasedPart));
            Assertions.True(released._records.Count == 1 &&
                BrownFurPersistedModifierPolicy.ResolveIncrease(released._records,
                    probe) == record.Increase,
                "The released typed reader could not read the written record.");

            // A record the product itself creates writes the same form.
            var created = new BrownFurPersistedModifierRecord(record.BuffGuid,
                record.SpellGuid, record.CasterId, record.AbilityScore,
                record.Increase, record.OriginalValue, record.OriginalDescriptor,
                record.CarrierFamily, record.EndTimeTicks);
            Assertions.True(JToken.DeepEquals(BrownFurPersistedModifierJson.Write(created),
                Without(written, "$id")),
                "A newly created record is not written in the released member order and value forms.");
            var typed = new ReleasedPart { _records = new List<BrownFurPersistedModifierRecord> { created } };
            var typedText = new StringWriter(CultureInfo.InvariantCulture);
            JsonSerializer.Create(Settings()).Serialize(typedText, typed, typeof(ReleasedPart));
            JToken typedRecord = JObject.Parse(typedText.ToString())["_records"][0];
            Assertions.True(JToken.DeepEquals(Without((JObject)typedRecord, "$id"),
                Without(written, "$id")) &&
                string.Join(",", ((JObject)typedRecord).Properties().Select(p => p.Name).ToArray()) ==
                string.Join(",", written.Properties().Select(p => p.Name).ToArray()),
                "The released typed writer under engine settings differs from the natively written record.");
        }

        internal static void UnconvertibleAndUnsupportedEntriesAreRetainedWithTheirContent()
        {
            JObject written = WrittenRecord();
            var cases = new[] {
                new KeyValuePair<string, JToken>("unknown-member", With(written, "FutureField",
                    new JObject { ["nested"] = new JArray(1, "two", true, null, 1.5m, "2026-11-22T10:00:00Z") })),
                new KeyValuePair<string, JToken>("unsupported-schema", With(written, "SchemaVersion", 2)),
                new KeyValuePair<string, JToken>("unsupported-ability-score", With(written, "AbilityScore", "Strenght")),
                new KeyValuePair<string, JToken>("unsupported-ability-score", With(written, "AbilityScore", "strength")),
                new KeyValuePair<string, JToken>("unsupported-ability-score", With(written, "AbilityScore", 99)),
                new KeyValuePair<string, JToken>("unsupported-ability-score", With(written, "AbilityScore", 1)),
                new KeyValuePair<string, JToken>("invalid-type", With(written, "Increase", "four")),
                new KeyValuePair<string, JToken>("invalid-type", With(written, "EndTimeTicks", 1.5m)),
                new KeyValuePair<string, JToken>("missing-member", Without(Without(written, "$id"), "OriginalDescriptor")),
                new KeyValuePair<string, JToken>("invalid-record", With(written, "Increase", 3)),
                new KeyValuePair<string, JToken>("entry-not-an-object", new JValue("garbage")),
                new KeyValuePair<string, JToken>("null-entry", JValue.CreateNull()) };
            foreach (KeyValuePair<string, JToken> item in cases)
            {
                string label = item.Key + " " + item.Value.ToString(Formatting.None);
                Part part = Load(Document(item.Value));
                BrownFurPersistedModifierState state = part.Read();
                Assertions.True(state.Active.Count == 0 && state.RetainedCount == 1 &&
                    state.NewlyRejected.Count == 1 && state.NewlyRejected[0].Key == item.Key,
                    "Entry was not retained with the expected reason: " + label + " -> " +
                    (state.NewlyRejected.Count == 0 ? "none" : state.NewlyRejected[0].Key));
                Assertions.Equal(0, BrownFurPersistedModifierPolicy.ResolveIncrease(
                    state.Active, Probe(Load(Document(written)).Read().Active[0])),
                    "A retained entry restored an increase: " + label);
                for (int cycle = 0; cycle < 3; cycle++)
                {
                    part.Write(part.Read());
                    part = Load(Save(part));
                    BrownFurPersistedModifierState again = part.Read();
                    JToken saved = Parse(Save(part))["_rejectedRecords"];
                    Assertions.True(again.Active.Count == 0 && again.RetainedCount == 1 &&
                        again.NewlyRejected.Count == 0 && saved is JArray &&
                        JToken.DeepEquals(saved[0], item.Value is JObject ?
                            Without((JObject)item.Value, "$id") : item.Value),
                        "Retained content changed across save and reload: " + label + " -> " + saved);
                    Assertions.True(JToken.DeepEquals(Parse(Save(part))["_records"], new JArray()),
                        "A retained entry stayed in the active member: " + label);
                }
            }

            // Unknown content keeps its token types and text, including decimals and date-like strings.
            Part future = Load(Document(cases[0].Value));
            future.Write(future.Read());
            JToken nested = Parse(Save(future))["_rejectedRecords"][0]["FutureField"]["nested"];
            Assertions.True(nested[4].Type == JTokenType.Float && (decimal)nested[4] == 1.5m &&
                nested[5].Type == JTokenType.String && (string)nested[5] == "2026-11-22T10:00:00Z",
                "Unknown content changed token type or text: " + nested.ToString(Formatting.None));

            // A non-array member is retained as one entry and the member is written as an array again.
            Part container = Load("{\"_records\":{\"unexpected\":true}}");
            BrownFurPersistedModifierState shape = container.Read();
            Assertions.True(shape.Active.Count == 0 && shape.NewlyRejected.Count == 1 &&
                shape.NewlyRejected[0].Key == "records-not-an-array",
                "A non-array records member was not retained.");
            container.Write(shape);
            JObject saved2 = Parse(Save(container));
            Assertions.True(JToken.DeepEquals(saved2["_records"], new JArray()) &&
                JToken.DeepEquals(saved2["_rejectedRecords"][0], JObject.Parse("{\"unexpected\":true}")),
                "The non-array member was not preserved: " + saved2);

            // Absence is not rejection.
            foreach (string json in new[] { "{}", "{\"_records\":null}", "{\"_records\":[]}" })
            {
                BrownFurPersistedModifierState empty = Load(json).Read();
                Assertions.True(empty.Active.Count == 0 && empty.Retained == null &&
                    empty.NewlyRejected.Count == 0, "Legitimate absence was rejected: " + json);
            }
        }

        internal static void DuplicatesAndAmbiguousSiblingsNeverChooseOrMultiply()
        {
            JObject written = WrittenRecord();
            JObject copy = With(written, "Increase", (int)written["Increase"]);
            BrownFurPersistedModifierRecord original = Load(Document(written)).Read().Active[0];
            BrownFurPersistedModifierProbe probe = Probe(original);

            BrownFurPersistedModifierState exact = Load(Document(written, copy)).Read();
            Assertions.True(exact.Active.Count == 1 && exact.RetainedCount == 1 &&
                exact.NewlyRejected[0].Key == "exact-duplicate" &&
                BrownFurPersistedModifierPolicy.ResolveIncrease(exact.Active, probe) == original.Increase,
                "An exact duplicate was multiplied, made ambiguous or lost.");

            JObject other = With(written, "Increase", original.Increase == 2 ? 4 : 2);
            BrownFurPersistedModifierState conflict = Load(Document(written, other)).Read();
            Assertions.True(conflict.Active.Count == 0 && conflict.RetainedCount == 2 &&
                conflict.NewlyRejected.All(value => value.Key == "conflicting-duplicate") &&
                BrownFurPersistedModifierPolicy.ResolveIncrease(conflict.Active, probe) == 0,
                "A conflicting duplicate group chose a record.");

            // A sibling for another buff keeps its meaning beside a rejected entry that cannot refer to it.
            JObject sibling = With(written, "BuffGuid", "0123456789abcdef0123456789abcdef");
            JObject unknownMember = With(written, "FutureField", 1);
            BrownFurPersistedModifierState siblings = Load(Document(sibling, unknownMember)).Read();
            Assertions.True(siblings.Active.Count == 1 && siblings.Active[0].BuffGuid ==
                (string)sibling["BuffGuid"] && siblings.RetainedCount == 1,
                "An unrelated valid sibling was not kept active.");

            // A rejected entry that could be the same modifier withholds the valid record.
            JObject misspelled = With(written, "AbilityScore", "Strenght");
            BrownFurPersistedModifierState ambiguous = Load(Document(written, misspelled, sibling)).Read();
            Assertions.True(ambiguous.Active.Count == 1 && ambiguous.Active[0].BuffGuid ==
                (string)sibling["BuffGuid"] && ambiguous.RetainedCount == 2 &&
                ambiguous.NewlyRejected.Any(value => value.Key == "ambiguous-with-rejected-entry"),
                "A record a rejected entry could refer to stayed active, or the unrelated sibling was lost.");
            BrownFurPersistedModifierState unattributable = Load(Document(written, new JValue("garbage"))).Read();
            Assertions.True(unattributable.Active.Count == 0 && unattributable.RetainedCount == 2,
                "An entry that states no identity did not withhold the valid record.");
        }

        internal static void OperationsAndSavesPreserveRetainedEntries()
        {
            JObject written = WrittenRecord();
            JObject misspelled = With(written, "AbilityScore", "Strenght");
            JObject sibling = With(written, "BuffGuid", "0123456789abcdef0123456789abcdef");
            Part part = Load(Document(written, misspelled, sibling));
            BrownFurPersistedModifierState state = part.Read();
            part.Write(state);
            List<BrownFurPersistedModifierRecord> retained = part._rejectedRecords;
            Assertions.True(part._records.Count == 1 && retained.Count == 2,
                "The first normalization did not split active and retained entries.");
            for (int cycle = 0; cycle < 5; cycle++)
            {
                BrownFurPersistedModifierState again = part.Read();
                Assertions.True(ReferenceEquals(again.Active, part._records) &&
                    ReferenceEquals(again.Retained, retained) && again.NewlyRejected.Count == 0,
                    "A repeated read changed or rebuilt the persisted lists.");
                part.Write(again);
            }

            // A newly committed cast establishes current state for the withheld modifier;
            // the retained history is untouched and does not withhold it later.
            BrownFurPersistedModifierRecord original = Load(Document(written)).Read().Active[0];
            var recast = new BrownFurPersistedModifierRecord(original.BuffGuid,
                original.SpellGuid, original.CasterId, original.AbilityScore, 4,
                original.OriginalValue, original.OriginalDescriptor,
                original.CarrierFamily, original.EndTimeTicks + 1);
            part.Write(BrownFurPersistedModifierStore.Remember(part.Read(), recast));
            Assertions.True(part._records.Count == 2 && ReferenceEquals(part._rejectedRecords, retained) &&
                retained.Count == 2, "A new cast changed or dropped retained entries.");
            part = Load(Save(part));
            BrownFurPersistedModifierState reloaded = part.Read();
            BrownFurPersistedModifierProbe recastProbe = Probe(recast);
            Assertions.True(reloaded.Active.Count == 2 && reloaded.RetainedCount == 2 &&
                BrownFurPersistedModifierPolicy.ResolveIncrease(reloaded.Active, recastProbe) == 4,
                "The new cast's record did not survive a save beside retained history.");
            Assertions.True(BrownFurPersistedModifierPolicy.ResolveOrdinaryRecast(reloaded.Active,
                new BrownFurOrdinaryRecastProbe {
                    BuffGuid = recast.BuffGuid, SpellGuid = recast.SpellGuid, CasterId = recast.CasterId,
                    AbilityScore = recast.AbilityScore, CurrentValue = recast.OriginalValue + 4,
                    CurrentDescriptor = recast.OriginalDescriptor, CarrierFamily = recast.CarrierFamily }) != null,
                "Ordinary recast no longer finds the active record beside retained history.");

            // Forget (removal or expiry) removes active records only.
            int removed;
            BrownFurPersistedModifierState forgotten = BrownFurPersistedModifierStore.Forget(reloaded,
                recast.BuffGuid, recast.SpellGuid, recast.CasterId, out removed);
            Assertions.True(removed == 1 && forgotten.Active.Count == 1 && forgotten.RetainedCount == 2,
                "Forget removed the wrong records or touched retained entries.");
            part.Write(forgotten);
            JObject saved = Parse(Save(part));
            Assertions.True(((JArray)saved["_rejectedRecords"]).Count == 2 &&
                JToken.DeepEquals(saved["_rejectedRecords"][0], Without(written, "$id")) &&
                JToken.DeepEquals(saved["_rejectedRecords"][1], misspelled),
                "Retained entries changed after Forget: " + saved["_rejectedRecords"]);
        }

        internal static void RefusedWritesLeaveActiveAndRetainedStateIntact()
        {
            JObject written = WrittenRecord();
            Part part = Load(Document(written, With(written, "AbilityScore", "Strenght"),
                With(written, "BuffGuid", "0123456789abcdef0123456789abcdef")));
            part.Write(part.Read());
            BrownFurPersistedModifierState before = part.Read();
            string text = Save(part);
            BrownFurPersistedModifierRecord original = Load(Document(written)).Read().Active[0];
            foreach (BrownFurPersistedModifierRecord invalid in new[] {
                new BrownFurPersistedModifierRecord(original.BuffGuid, original.SpellGuid, original.CasterId,
                    (BrownFurAbilityScore)99, 2, 4, "Enhancement", "AddStatBonus", 1L),
                new BrownFurPersistedModifierRecord(original.BuffGuid, original.SpellGuid, original.CasterId,
                    BrownFurAbilityScore.None, 2, 4, "Enhancement", "AddStatBonus", 1L),
                new BrownFurPersistedModifierRecord(original.BuffGuid, original.SpellGuid, original.CasterId,
                    BrownFurAbilityScore.Strength, 3, 4, "Enhancement", "AddStatBonus", 1L),
                new BrownFurPersistedModifierRecord(original.BuffGuid, original.SpellGuid, string.Empty,
                    BrownFurAbilityScore.Strength, 2, 4, "Enhancement", "AddStatBonus", 1L) })
            {
                Assertions.True(ThrowsAny(() => part.Write(
                    BrownFurPersistedModifierStore.Remember(part.Read(), invalid))),
                    "An invalid new write was accepted.");
                Assertions.True(Save(part) == text && ReferenceEquals(part.Read().Active, before.Active) &&
                    ReferenceEquals(part.Read().Retained, before.Retained),
                    "A refused write changed the active or retained state.");
            }
        }

        // ---- A2 review: raw JSON text at the input boundary ----
        // Every input below is a raw string fed to the production converter through a JsonTextReader with the engine
        // reader's defaults (dates parsed, floats as double). Expected text is the same string; nothing is materialized
        // through JObject on either side of a preservation assertion.

        private const string R =
            "{\"SchemaVersion\":1,\"BuffGuid\":\"b175001b42b1a02479881b72fe132116\"," +
            "\"SpellGuid\":\"4c3d08935262b6544ae97599b3a9556d\",\"CasterId\":\"e9d20d9d-7ba5-4f71-a47c-d893e7505806\"," +
            "\"AbilityScore\":\"Strength\",\"Increase\":2,\"OriginalValue\":4,\"OriginalDescriptor\":\"Enhancement\"," +
            "\"CarrierFamily\":\"AddStatBonus\",\"EndTimeTicks\":438309050000}";

        private static string Extend(string record, string members)
        {
            return record.Substring(0, record.Length - 1) + "," + members + "}";
        }

        private static string Rebuff(string record)
        {
            return record.Replace("b175001b42b1a02479881b72fe132116", OtherBuffGuid);
        }

        private const string OtherBuffGuid = "0123456789abcdef0123456789abcdef";

        private static string RawDocument(params string[] records)
        {
            return "{\"_records\":[" + string.Join(",", records) + "]}";
        }

        private static string[] RetainedTexts(BrownFurPersistedModifierState state)
        {
            return state.Retained == null ? new string[0] :
                state.Retained.Select(BrownFurPersistedModifierJson.Text).ToArray();
        }

        private static string[] ActiveTexts(BrownFurPersistedModifierState state)
        {
            return state.Active.Select(BrownFurPersistedModifierJson.Text).ToArray();
        }

        /// <summary>Loads raw text, normalizes, then saves and reloads three times; returns each cycle's state.</summary>
        private static List<BrownFurPersistedModifierState> Cycles(string json)
        {
            var states = new List<BrownFurPersistedModifierState>();
            Part part = Load(json);
            for (int cycle = 0; cycle < 4; cycle++)
            {
                BrownFurPersistedModifierState state = part.Read();
                states.Add(state);
                part.Write(state);
                part = Load(Save(part));
            }
            return states;
        }

        internal static void NumbersKeepTheirTextAndNeverThrow()
        {
            string[] futures = {
                "\"FutureField\":1e100",
                "\"FutureField\":0.1000000000000000055511151231257827",
                "\"FutureField\":123456789012345678901234567890",
                "\"FutureField\":1e400",
                "\"FutureField\":-1.5E-10",
                "\"FutureField\":{\"nested\":[1,\"two\",true,null,1.5,1e100,\"2026-11-22T10:00:00+02:00\"]}" };
            foreach (string future in futures)
            {
                string unsupported = Rebuff(Extend(R, future));
                string json = RawDocument(R, unsupported);
                List<BrownFurPersistedModifierState> states;
                try { states = Cycles(json); }
                catch (Exception exception)
                {
                    throw new InvalidOperationException("Raw numeric input threw instead of being retained: " +
                        future + " -> " + exception.GetType().Name + ": " + exception.Message);
                }
                foreach (BrownFurPersistedModifierState state in states)
                {
                    Assertions.True(ActiveTexts(state).SequenceEqual(new[] { R }),
                        "The valid sibling was not kept exactly beside " + future + ": " + string.Join(" | ", ActiveTexts(state)));
                    Assertions.True(RetainedTexts(state).SequenceEqual(new[] { unsupported }),
                        "Retained numeric text changed for " + future + ": " + string.Join(" | ", RetainedTexts(state)));
                }
                Part reloaded = Load(json);
                reloaded.Write(reloaded.Read());
                Assertions.True(Save(reloaded).Contains(unsupported),
                    "The saved text does not contain the retained entry verbatim: " + future);
            }

            // Content already in _rejectedRecords stays verbatim and inactive.
            string retainedOnly = "{\"_records\":[],\"_rejectedRecords\":[" + Extend(R, "\"FutureField\":1e100") + "," +
                Extend(R, "\"FutureField\":0.1000000000000000055511151231257827") + "]}";
            foreach (BrownFurPersistedModifierState state in Cycles(retainedOnly))
                Assertions.True(state.Active.Count == 0 && RetainedTexts(state).SequenceEqual(new[] {
                    Extend(R, "\"FutureField\":1e100"), Extend(R, "\"FutureField\":0.1000000000000000055511151231257827") }),
                    "Retained-member numeric text changed or became active: " + string.Join(" | ", RetainedTexts(state)));
        }

        internal static void RepeatedMembersAreRetainedWithEveryOccurrence()
        {
            string conflicting = R.Replace("\"Increase\":2,", "\"Increase\":2,\"Increase\":4,");
            string identical = R.Replace("\"Increase\":2,", "\"Increase\":2,\"Increase\":2,");
            string sibling = Rebuff(R);
            foreach (string repeated in new[] { conflicting, identical })
            {
                foreach (BrownFurPersistedModifierState state in Cycles(RawDocument(repeated, sibling)))
                {
                    Assertions.True(ActiveTexts(state).SequenceEqual(new[] { sibling }),
                        "A record with repeated members became active, or the unrelated sibling was lost: " +
                        string.Join(" | ", ActiveTexts(state)));
                    Assertions.True(RetainedTexts(state).SequenceEqual(new[] { repeated }),
                        "The repeated members were not retained with every occurrence: " + string.Join(" | ", RetainedTexts(state)));
                }
                BrownFurPersistedModifierState first = Load(RawDocument(repeated, sibling)).Read();
                Assertions.True(first.NewlyRejected.Count == 1 && first.NewlyRejected[0].Key == "repeated-member",
                    "A repeated member was not rejected as such.");
                BrownFurPersistedModifierRecord original = Load(Document(WrittenRecord())).Read().Active[0];
                Assertions.Equal(0, BrownFurPersistedModifierPolicy.ResolveIncrease(first.Active, Probe(original)),
                    "A record with repeated members restored an increase.");
            }
            // The repeated record still states its identity, so it withholds a valid record of the same modifier.
            BrownFurPersistedModifierState ambiguous = Load(RawDocument(R, conflicting)).Read();
            Assertions.True(ambiguous.Active.Count == 0 && ambiguous.RetainedCount == 2,
                "A valid record stayed active beside a repeated-member record of the same modifier.");
        }

        internal static void UnsupportedMetadataAndScalarMembersArePreserved()
        {
            string linked = "{\"$id\":\"r\",\"SchemaVersion\":2,\"BuffGuid\":\"" + OtherBuffGuid + "\",\"Link\":{\"$ref\":\"r\"}}";
            foreach (BrownFurPersistedModifierState state in Cycles(RawDocument(R, linked)))
                Assertions.True(ActiveTexts(state).SequenceEqual(new[] { R }) &&
                    RetainedTexts(state).SequenceEqual(new[] { linked }),
                    "Unsupported metadata or reference text changed: " + string.Join(" | ", RetainedTexts(state)));

            // The released record's own "$id" is the one proven-current normalization.
            string withId = "{\"$id\":\"1235\"," + R.Substring(1);
            BrownFurPersistedModifierState released = Load(RawDocument(withId)).Read();
            Assertions.True(ActiveTexts(released).SequenceEqual(new[] { R }),
                "The released record's $id was not the only change: " + string.Join(" | ", ActiveTexts(released)));

            // Scalar member values the reader converts before the member converter runs.
            foreach (string scalar in new[] { "\"2026-11-22T10:00:00+02:00\"", "1e100", "0.1000000000000000055511151231257827", "true", "\"text\"" })
                foreach (BrownFurPersistedModifierState state in Cycles("{\"_records\":" + scalar + "}"))
                    Assertions.True(state.Active.Count == 0 && RetainedTexts(state).SequenceEqual(new[] { scalar }),
                        "A scalar records member was not retained verbatim: " + scalar + " -> " + string.Join(" | ", RetainedTexts(state)));

            // Known limit: an integer beyond Int64 as the member's own value fails in the engine reader before any
            // member converter runs (Newtonsoft 8 ParseNumber); it cannot be contained at this member.
            Assertions.True(ThrowsJson(() => Load("{\"_records\":123456789012345678901234567890}")),
                "The documented Int64 overflow limit at the member's own value changed.");
        }

        // The engine path for unit parts: Kingmaker's DictionaryConverter (UnitPartsManager.m_Parts) runs
        // JArray.Load(reader) on the file reader, then item["Value"].ToObject(type, serializer). The member converter
        // therefore reads a JTokenReader over tokens the engine has already materialized. This test reproduces those
        // two calls; the engine converter itself is exercised natively (risk03-bf-persist).
        private static Part LoadPartThroughEngine(string valueObject)
        {
            string parts = "[{\"Key\":\"part\",\"Value\":" + valueObject + "}]";
            var reader = new JsonTextReader(new StringReader(parts));
            reader.Read();
            JArray loaded = JArray.Load(reader);
            return (Part)loaded[0]["Value"].ToObject(typeof(Part), JsonSerializer.Create(Settings()));
        }

        private static Part LoadThroughEngineDictionary(string recordsValue)
        {
            return LoadPartThroughEngine("{\"_records\":" + recordsValue + "}");
        }

        // What the engine hands the owner for one element: the file reader's JToken, written by a plain writer.
        private static string EngineText(string element)
        {
            return JToken.ReadFrom(new JsonTextReader(new StringReader(element))).ToString(Formatting.None);
        }

        internal static void EngineDictionaryPathKeepsWhatReachesTheOwner()
        {
            string future = Rebuff(Extend(R, "\"FutureField\":{\"nested\":[1e100,0.1000000000000000055511151231257827,\"2026-11-22T10:00:00+02:00\"]}"));
            string linked = "{\"$id\":\"r\",\"SchemaVersion\":2,\"BuffGuid\":\"" + OtherBuffGuid + "\",\"Link\":{\"$ref\":\"r\"}}";
            Part part = LoadThroughEngineDictionary("[" + R + "," + future + "," + linked + "]");
            for (int cycle = 0; cycle < 3; cycle++)
            {
                BrownFurPersistedModifierState state = part.Read();
                Assertions.True(ActiveTexts(state).SequenceEqual(new[] { R }),
                    "The valid record changed on the engine path: " + string.Join(" | ", ActiveTexts(state)));
                Assertions.True(RetainedTexts(state).SequenceEqual(new[] { EngineText(future), linked }),
                    "Retained content differs from what the engine handed over: " + string.Join(" | ", RetainedTexts(state)) +
                    " expected " + EngineText(future));
                part.Write(state);
                part = LoadPartThroughEngine(Save(part));
            }

            // Documented engine limits, asserted so a change is noticed:
            // repeated members arrive collapsed (last occurrence wins), so the owner cannot see them;
            string repeated = R.Replace("\"Increase\":2,", "\"Increase\":2,\"Increase\":4,");
            BrownFurPersistedModifierState collapsed = LoadThroughEngineDictionary("[" + repeated + "]").Read();
            Assertions.True(collapsed.Active.Count == 1 && collapsed.Active[0].Increase == 4,
                "The engine's collapse of repeated members changed: " + string.Join(" | ", ActiveTexts(collapsed)));
            // an integer beyond Int64 fails JArray.Load before the owner runs.
            Assertions.True(ThrowsJson(() => LoadThroughEngineDictionary("[" + Extend(R, "\"FutureField\":123456789012345678901234567890") + "]")),
                "The engine's Int64 limit inside a part changed.");
        }

        private static bool ThrowsJson(Action action)
        {
            try { action(); return false; }
            catch (JsonReaderException) { return true; }
        }

        private static bool ThrowsAny(Action action)
        {
            try { action(); return false; }
            catch (InvalidOperationException) { return true; }
            catch (NotSupportedException) { return true; }
        }
    }
}
