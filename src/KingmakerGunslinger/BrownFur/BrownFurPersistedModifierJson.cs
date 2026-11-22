using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.BrownFur
{
    /// <summary>
    /// Member-level converter for the persisted Powerful Change record lists (RISK-03, A2). The engine's
    /// DefaultJsonSettings have no error handler, so a record the typed reader cannot convert throws out of the whole
    /// area or party file before any owner callback runs. This converter reads the member token by token instead
    /// (BrownFurJsonCapture): every element becomes either a strictly valid released record or a rejected entry kept as
    /// canonical JSON text. Valid records are written in the released member names, order and value forms.
    /// </summary>
    public sealed class BrownFurPersistedModifierListConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(List<BrownFurPersistedModifierRecord>);
        }

        public override object ReadJson(JsonReader reader, Type objectType,
            object existingValue, JsonSerializer serializer)
        {
            DateParseHandling dates = reader.DateParseHandling;
            FloatParseHandling floats = reader.FloatParseHandling;
            // Inner strings stay strings. Floats are read as double only to
            // advance the reader; their text is taken from the source (Decimal
            // would throw on 1e100 and neither keeps every digit).
            reader.DateParseHandling = DateParseHandling.None;
            reader.FloatParseHandling = FloatParseHandling.Double;
            try
            {
                return BrownFurPersistedModifierJson.ReadList(reader);
            }
            finally
            {
                reader.DateParseHandling = dates;
                reader.FloatParseHandling = floats;
            }
        }

        public override void WriteJson(JsonWriter writer, object value,
            JsonSerializer serializer)
        {
            BrownFurPersistedModifierJson.WriteList(writer,
                value as List<BrownFurPersistedModifierRecord>);
        }
    }

    /// <summary>One captured JSON value.</summary>
    internal sealed class BrownFurCapturedValue
    {
        /// <summary>The value with the first occurrence of each member; numbers are exact longs or raw text.</summary>
        internal JToken Token;

        /// <summary>
        /// Compact JSON text of every token in source order, including repeated members, with each number's source
        /// text. Whitespace and string escapes are canonical (the writer's), not the source's.
        /// </summary>
        internal string Text;

        /// <summary>True when any object in the value repeats a member name.</summary>
        internal bool Repeated;

        /// <summary>For an object: each member name with the text of every occurrence's value.</summary>
        internal Dictionary<string, List<string>> Members;
    }

    /// <summary>
    /// Reads one JSON value token by token without materializing it through JObject (which keeps only the last of
    /// repeated members) and without narrowing numbers. This holds when the converter reads a JsonTextReader. Unit
    /// parts in a save do not: the engine's DictionaryConverter (UnitPartsManager.m_Parts) first loads the whole
    /// parts array with JArray.Load at the file reader's settings, so repeated members are already collapsed (last
    /// wins), numbers are already long or double, date-like strings are already dates, and an integer beyond Int64
    /// fails that load before any owner code runs. From that token reader the capture is total over what it receives. Newtonsoft 8 (the engine's
    /// pinned 8.0.0.0) does not expose a number's source text and throws for integers beyond Int64 and doubles out of
    /// range after it has consumed the number; the source text is therefore read from the JsonTextReader's character
    /// buffer, and that one number failure is resumed, both guarded by the reader's state and the JSON number grammar.
    /// Anything else malformed still throws, as the text is then not JSON.
    /// </summary>
    internal static class BrownFurJsonCapture
    {
        private const BindingFlags Field = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly FieldInfo Chars = typeof(JsonTextReader).GetField("_chars", Field);
        private static readonly FieldInfo Position = typeof(JsonTextReader).GetField("_charPos", Field);
        private static readonly FieldInfo RecentString = typeof(JsonTextReader).GetField("_stringReference", Field);
        private static readonly FieldInfo State = typeof(JsonReader).GetField("_currentState", Field);
        private static readonly Regex Number = new Regex(
            "^-?[0-9]+(\\.[0-9]+)?([eE][+-]?[0-9]+)?$", RegexOptions.CultureInvariant);
        private static readonly Regex Integer = new Regex(
            "^-?(0|[1-9][0-9]*)$", RegexOptions.CultureInvariant);

        /// <summary>Moves to the next token. Returns a resumed number's text, or null after an ordinary token.</summary>
        internal static string Advance(JsonReader reader)
        {
            try
            {
                do
                {
                    if (!reader.Read())
                        throw new JsonSerializationException(
                            "Unexpected end of the Powerful Change records.");
                }
                while (reader.TokenType == JsonToken.Comment);
                return null;
            }
            catch (JsonReaderException)
            {
                string text = ResumableNumber(reader);
                if (text == null) throw;
                return text;
            }
        }

        /// <summary>Captures the value whose first token is current (or the resumed number).</summary>
        internal static BrownFurCapturedValue Capture(JsonReader reader, string resumed)
        {
            var text = new StringWriter(CultureInfo.InvariantCulture);
            var writer = new JsonTextWriter(text) { Formatting = Formatting.None };
            var value = new BrownFurCapturedValue();
            if (resumed != null)
            {
                writer.WriteRawValue(resumed);
                value.Token = NumberToken(resumed);
            }
            else
                value.Token = Write(reader, writer, value);
            writer.Flush();
            value.Text = text.ToString();
            return value;
        }

        private static JToken Write(JsonReader reader, JsonTextWriter writer,
            BrownFurCapturedValue value)
        {
            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                {
                    var result = new JObject();
                    value.Members = new Dictionary<string, List<string>>(StringComparer.Ordinal);
                    writer.WriteStartObject();
                    while (true)
                    {
                        string resumed = Advance(reader);
                        if (resumed == null && reader.TokenType == JsonToken.EndObject) break;
                        if (resumed != null || reader.TokenType != JsonToken.PropertyName)
                            throw new JsonSerializationException(
                                "Unexpected token in a Powerful Change record object.");
                        string name = (string)reader.Value;
                        BrownFurCapturedValue member = Capture(reader, Advance(reader));
                        writer.WritePropertyName(name);
                        writer.WriteRawValue(member.Text);
                        List<string> occurrences;
                        if (!value.Members.TryGetValue(name, out occurrences))
                        {
                            value.Members.Add(name, occurrences = new List<string>());
                            result.Add(name, member.Token);
                        }
                        else value.Repeated = true;
                        occurrences.Add(member.Text);
                        value.Repeated |= member.Repeated;
                    }
                    writer.WriteEndObject();
                    return result;
                }
                case JsonToken.StartArray:
                {
                    var result = new JArray();
                    writer.WriteStartArray();
                    while (true)
                    {
                        string resumed = Advance(reader);
                        if (resumed == null && reader.TokenType == JsonToken.EndArray) break;
                        BrownFurCapturedValue item = Capture(reader, resumed);
                        writer.WriteRawValue(item.Text);
                        result.Add(item.Token);
                        value.Repeated |= item.Repeated;
                    }
                    writer.WriteEndArray();
                    return result;
                }
                case JsonToken.Integer:
                case JsonToken.Float:
                {
                    string source = SourceNumber(reader);
                    if (source == null)
                    {
                        // Not a source number (for example a NaN literal): kept as the reader produced it.
                        var parsed = new JValue(reader.Value);
                        parsed.WriteTo(writer);
                        return parsed;
                    }
                    writer.WriteRawValue(source);
                    return NumberToken(source);
                }
                case JsonToken.Date:
                {
                    // From the file's own reader only the member's first token can be a date (read with the
                    // engine reader's settings before this converter ran); its source string is still the
                    // reader's most recent string. From the engine's token reader (unit parts arrive through
                    // DictionaryConverter's JArray.Load) the source text no longer exists: the date value is kept
                    // as the engine produced it and written as the engine's writer writes dates.
                    string source = RecentStringText(reader);
                    if (source != null)
                    {
                        writer.WriteValue(source);
                        return new JValue(source);
                    }
                    var date = new JValue(reader.Value);
                    date.WriteTo(writer);
                    return date;
                }
                case JsonToken.String:
                    writer.WriteValue((string)reader.Value);
                    return new JValue((string)reader.Value);
                case JsonToken.Boolean:
                    writer.WriteValue((bool)reader.Value);
                    return new JValue((bool)reader.Value);
                case JsonToken.Null:
                    writer.WriteNull();
                    return JValue.CreateNull();
                case JsonToken.Undefined:
                    writer.WriteUndefined();
                    return JValue.CreateUndefined();
                default:
                {
                    var other = new JValue(reader.Value);
                    other.WriteTo(writer);
                    return other;
                }
            }
        }

        private static JToken NumberToken(string text)
        {
            long value;
            return Integer.IsMatch(text) && long.TryParse(text, NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture, out value) ? (JToken)new JValue(value) : new JRaw(text);
        }

        /// <summary>The source text of the number token just read: the characters before the reader's position.</summary>
        private static string SourceNumber(JsonReader reader)
        {
            if (!(reader is JsonTextReader) || Chars == null || Position == null) return null;
            var chars = (char[])Chars.GetValue(reader);
            int end = (int)Position.GetValue(reader);
            int start = end;
            while (start > 0 && "0123456789+-.eE".IndexOf(chars[start - 1]) >= 0) start--;
            string text = new string(chars, start, end - start);
            return Number.IsMatch(text) ? text : null;
        }

        /// <summary>
        /// After Newtonsoft 8's ParseNumber threw because the number did not fit Int64 or double: it has already moved
        /// past the number and set the post-value state, so reading can resume after it.
        /// </summary>
        private static string ResumableNumber(JsonReader reader)
        {
            if (State == null || !string.Equals(Convert.ToString(State.GetValue(reader),
                    CultureInfo.InvariantCulture), "PostValue", StringComparison.Ordinal))
                return null;
            return SourceNumber(reader);
        }

        private static string RecentStringText(JsonReader reader)
        {
            if (!(reader is JsonTextReader) || RecentString == null) return null;
            object recent = RecentString.GetValue(reader);
            string text = recent == null ? null : recent.ToString();
            return string.IsNullOrEmpty(text) ? null : text;
        }
    }

    internal static class BrownFurPersistedModifierJson
    {
        // The released record's members, in the released writer's order.
        internal static readonly string[] Members = {
            "SchemaVersion", "BuffGuid", "SpellGuid", "CasterId", "AbilityScore",
            "Increase", "OriginalValue", "OriginalDescriptor", "CarrierFamily",
            "EndTimeTicks" };

        /// <summary>The member whose first token is current.</summary>
        internal static List<BrownFurPersistedModifierRecord> ReadList(
            JsonReader reader)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            if (reader.TokenType != JsonToken.StartArray)
                return new List<BrownFurPersistedModifierRecord> {
                    BrownFurPersistedModifierRecord.Rejected(
                        new JRaw(BrownFurJsonCapture.Capture(reader, null).Text),
                        "records-not-an-array", Unreadable()) };
            var records = new List<BrownFurPersistedModifierRecord>();
            while (true)
            {
                string resumed = BrownFurJsonCapture.Advance(reader);
                if (resumed == null && reader.TokenType == JsonToken.EndArray) break;
                records.Add(Classify(BrownFurJsonCapture.Capture(reader, resumed)));
            }
            return records;
        }

        /// <summary>One element given as JSON text (fixtures and tests); read exactly as the converter reads it.</summary>
        internal static BrownFurPersistedModifierRecord ReadText(string json)
        {
            var reader = new JsonTextReader(new StringReader(json)) {
                DateParseHandling = DateParseHandling.None,
                FloatParseHandling = FloatParseHandling.Double };
            string resumed = BrownFurJsonCapture.Advance(reader);
            return Classify(BrownFurJsonCapture.Capture(reader, resumed));
        }

        internal static void WriteList(JsonWriter writer,
            IList<BrownFurPersistedModifierRecord> records)
        {
            if (records == null)
            {
                writer.WriteNull();
                return;
            }
            writer.WriteStartArray();
            foreach (BrownFurPersistedModifierRecord record in records)
                (record == null ? new JRaw("null") : record.Raw ?? Write(record))
                    .WriteTo(writer);
            writer.WriteEndArray();
        }

        /// <summary>The released wire form of a strictly valid record.</summary>
        internal static JObject Write(BrownFurPersistedModifierRecord record)
        {
            return new JObject {
                { "SchemaVersion", record.SchemaVersion },
                { "BuffGuid", record.BuffGuid },
                { "SpellGuid", record.SpellGuid },
                { "CasterId", record.CasterId },
                { "AbilityScore", record.AbilityScore.ToString() },
                { "Increase", record.Increase },
                { "OriginalValue", record.OriginalValue },
                { "OriginalDescriptor", record.OriginalDescriptor },
                { "CarrierFamily", record.CarrierFamily },
                { "EndTimeTicks", record.EndTimeTicks } };
        }

        internal static BrownFurPersistedModifierRecord NullEntry()
        {
            return BrownFurPersistedModifierRecord.Rejected(new JRaw("null"),
                "null-entry", Unreadable());
        }

        /// <summary>
        /// Only the released writer's form becomes a record: one object with exactly the ten released members (each
        /// once) and their released token types, plus at most the engine's own "$id" (a string or integer). That
        /// "$id" is the one proven normalization: nothing refers to a released record and the engine renumbers every
        /// save, so an accepted record is written without it. Every other entry is rejected and kept as its full
        /// captured text, metadata and references included.
        /// </summary>
        private static BrownFurPersistedModifierRecord Classify(
            BrownFurCapturedValue value)
        {
            if (value.Token == null || value.Token.Type == JTokenType.Null)
                return BrownFurPersistedModifierRecord.Rejected(new JRaw(value.Text),
                    "null-entry", Unreadable());
            var source = value.Token as JObject;
            if (source == null)
                return BrownFurPersistedModifierRecord.Rejected(new JRaw(value.Text),
                    "entry-not-an-object", Unreadable());
            string[] identity = Identity(source, value.Members);
            if (value.Repeated)
                return BrownFurPersistedModifierRecord.Rejected(new JRaw(value.Text),
                    "repeated-member", identity);
            string reason = Structure(source);
            if (reason != null)
                return BrownFurPersistedModifierRecord.Rejected(new JRaw(value.Text),
                    reason, identity);
            var record = new BrownFurPersistedModifierRecord(
                (string)source["BuffGuid"], (string)source["SpellGuid"],
                (string)source["CasterId"],
                (BrownFurAbilityScore)Enum.Parse(typeof(BrownFurAbilityScore),
                    (string)source["AbilityScore"], false),
                (int)(long)source["Increase"], (int)(long)source["OriginalValue"],
                (string)source["OriginalDescriptor"],
                (string)source["CarrierFamily"], (long)source["EndTimeTicks"]);
            record.SchemaVersion = (int)(long)source["SchemaVersion"];
            try { BrownFurPersistedModifierPolicy.Validate(record); }
            catch (InvalidOperationException)
            {
                return BrownFurPersistedModifierRecord.Rejected(new JRaw(value.Text),
                    "invalid-record", identity);
            }
            catch (NotSupportedException)
            {
                return BrownFurPersistedModifierRecord.Rejected(new JRaw(value.Text),
                    "invalid-record", identity);
            }
            var released = (JObject)source.DeepClone();
            released.Remove("$id");
            record.Raw = released;
            return record;
        }

        private static string Structure(JObject raw)
        {
            foreach (JProperty property in raw.Properties())
            {
                if (property.Name == "$id")
                {
                    if (property.Value.Type != JTokenType.String &&
                        property.Value.Type != JTokenType.Integer)
                        return "unsupported-metadata";
                    continue;
                }
                if (Array.IndexOf(Members, property.Name) < 0)
                    return "unknown-member";
            }
            foreach (string member in Members)
                if (raw[member] == null) return "missing-member";
            if (!IsInt32(raw["SchemaVersion"])) return "invalid-type";
            if ((long)raw["SchemaVersion"] !=
                BrownFurPersistedModifierRecord.CurrentSchemaVersion)
                return "unsupported-schema";
            foreach (string member in new[] { "BuffGuid", "SpellGuid",
                "CasterId", "OriginalDescriptor", "CarrierFamily" })
                if (raw[member].Type != JTokenType.String) return "invalid-type";
            if (raw["AbilityScore"].Type != JTokenType.String ||
                AbilityScore((string)raw["AbilityScore"]) ==
                    BrownFurAbilityScore.None)
                return "unsupported-ability-score";
            if (!IsInt32(raw["Increase"]) || !IsInt32(raw["OriginalValue"]) ||
                !IsInt64(raw["EndTimeTicks"]))
                return "invalid-type";
            return null;
        }

        /// <summary>
        /// The identity fields an entry states in a usable form: buff, spell, caster, ability score, carrier. A field
        /// that is absent, fails its own check, or occurs more than once with different values is null, meaning
        /// "could be any value".
        /// </summary>
        private static string[] Identity(JObject raw,
            Dictionary<string, List<string>> members)
        {
            Func<string, JToken> single = name =>
            {
                List<string> occurrences;
                return members != null && members.TryGetValue(name, out occurrences) &&
                    occurrences.Distinct(StringComparer.Ordinal).Count() > 1 ? null : raw[name];
            };
            JToken caster = single("CasterId"), score = single("AbilityScore"),
                carrier = single("CarrierFamily");
            return new[] {
                Guid(single("BuffGuid")), Guid(single("SpellGuid")),
                caster != null && caster.Type == JTokenType.String &&
                    !string.IsNullOrWhiteSpace((string)caster) ? (string)caster : null,
                score != null && score.Type == JTokenType.String &&
                    AbilityScore((string)score) != BrownFurAbilityScore.None ?
                    (string)score : null,
                carrier != null && carrier.Type == JTokenType.String &&
                    BrownFurModifierAdjustmentPolicy.IsSupportedCarrier((string)carrier) ?
                    (string)carrier : null };
        }

        internal static string[] Identity(BrownFurPersistedModifierRecord record)
        {
            return new[] { record.BuffGuid, record.SpellGuid, record.CasterId,
                record.AbilityScore.ToString(), record.CarrierFamily };
        }

        internal static string[] Unreadable()
        {
            return new string[5];
        }

        private static string Guid(JToken token)
        {
            if (token == null || token.Type != JTokenType.String) return null;
            string value = (string)token;
            System.Guid parsed;
            return value.Length == 32 && System.Guid.TryParseExact(value, "N",
                out parsed) && string.Equals(value, value.ToLowerInvariant(),
                    StringComparison.Ordinal) ? value : null;
        }

        private static BrownFurAbilityScore AbilityScore(string name)
        {
            foreach (BrownFurAbilityScore value in Enum.GetValues(
                typeof(BrownFurAbilityScore)))
                if (value != BrownFurAbilityScore.None && string.Equals(
                        value.ToString(), name, StringComparison.Ordinal))
                    return value;
            return BrownFurAbilityScore.None;
        }

        private static bool IsInt32(JToken token)
        {
            if (token == null || token.Type != JTokenType.Integer) return false;
            object value = ((JValue)token).Value;
            return value is long && (long)value >= int.MinValue &&
                (long)value <= int.MaxValue;
        }

        private static bool IsInt64(JToken token)
        {
            return token != null && token.Type == JTokenType.Integer &&
                ((JValue)token).Value is long;
        }

        /// <summary>Compact JSON text of an entry: its captured text when rejected, its released form when valid.</summary>
        internal static string Text(BrownFurPersistedModifierRecord record)
        {
            if (record == null) return "null";
            var raw = record.Raw as JRaw;
            if (raw != null) return Convert.ToString(raw.Value, CultureInfo.InvariantCulture);
            return (record.Raw ?? Write(record)).ToString(Formatting.None);
        }
    }
}
