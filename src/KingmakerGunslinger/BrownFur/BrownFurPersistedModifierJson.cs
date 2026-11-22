using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.BrownFur
{
    /// <summary>
    /// Member-level converter for the persisted Powerful Change record lists (RISK-03, A2). The engine's
    /// DefaultJsonSettings have no error handler, so a record that the typed reader cannot convert (an unknown
    /// AbilityScore name, a string where a number belongs, a non-object element) throws out of the whole area or party
    /// file before any owner callback runs. This converter reads the member as plain JSON instead and never throws for
    /// well-formed JSON: every element becomes either a strictly valid record or a rejected entry that keeps its JSON
    /// content. Valid records are written in the released member names, order and value forms.
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
            // Unknown content keeps its text: date-like strings stay strings and
            // fractional numbers keep their decimal digits.
            reader.DateParseHandling = DateParseHandling.None;
            reader.FloatParseHandling = FloatParseHandling.Decimal;
            try
            {
                return BrownFurPersistedModifierJson.ReadList(JToken.ReadFrom(reader));
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

    internal static class BrownFurPersistedModifierJson
    {
        // The released record's members, in the released writer's order.
        internal static readonly string[] Members = {
            "SchemaVersion", "BuffGuid", "SpellGuid", "CasterId", "AbilityScore",
            "Increase", "OriginalValue", "OriginalDescriptor", "CarrierFamily",
            "EndTimeTicks" };

        internal static List<BrownFurPersistedModifierRecord> ReadList(
            JToken token)
        {
            if (token == null || token.Type == JTokenType.Null) return null;
            var array = token as JArray;
            if (array == null)
                return new List<BrownFurPersistedModifierRecord> {
                    BrownFurPersistedModifierRecord.Rejected(token.DeepClone(),
                        "records-not-an-array", Unreadable()) };
            return array.Select(Read).ToList();
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
                (record == null ? JValue.CreateNull() : record.Raw ?? Write(record))
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

        /// <summary>
        /// One element. The engine's reference identifier "$id" at the record's top level is dropped (nothing refers
        /// to a record, and the engine renumbers every save); everything else is kept. Only the released form is
        /// accepted: exactly the ten released members with their released token types, the defined ability-score
        /// names, schema 1, and the existing structural policy.
        /// </summary>
        internal static BrownFurPersistedModifierRecord Read(JToken element)
        {
            if (element == null || element.Type == JTokenType.Null)
                return BrownFurPersistedModifierRecord.Rejected(
                    JValue.CreateNull(), "null-entry", Unreadable());
            var source = element as JObject;
            if (source == null)
                return BrownFurPersistedModifierRecord.Rejected(
                    element.DeepClone(), "entry-not-an-object", Unreadable());
            var raw = (JObject)source.DeepClone();
            JToken id = raw["$id"];
            if (id != null && (id.Type == JTokenType.String ||
                id.Type == JTokenType.Integer))
                raw.Remove("$id");
            string[] identity = Identity(raw);
            string reason = Structure(raw);
            if (reason != null)
                return BrownFurPersistedModifierRecord.Rejected(raw, reason,
                    identity);
            var record = new BrownFurPersistedModifierRecord(
                (string)raw["BuffGuid"], (string)raw["SpellGuid"],
                (string)raw["CasterId"],
                (BrownFurAbilityScore)Enum.Parse(typeof(BrownFurAbilityScore),
                    (string)raw["AbilityScore"], false),
                (int)raw["Increase"], (int)raw["OriginalValue"],
                (string)raw["OriginalDescriptor"],
                (string)raw["CarrierFamily"], (long)raw["EndTimeTicks"]);
            record.SchemaVersion = (int)raw["SchemaVersion"];
            try { BrownFurPersistedModifierPolicy.Validate(record); }
            catch (InvalidOperationException)
            {
                return BrownFurPersistedModifierRecord.Rejected(raw,
                    "invalid-record", identity);
            }
            catch (NotSupportedException)
            {
                return BrownFurPersistedModifierRecord.Rejected(raw,
                    "invalid-record", identity);
            }
            record.Raw = raw;
            return record;
        }

        private static string Structure(JObject raw)
        {
            foreach (JProperty property in raw.Properties())
                if (Array.IndexOf(Members, property.Name) < 0)
                    return "unknown-member";
            foreach (string member in Members)
                if (raw[member] == null) return "missing-member";
            if (!IsInt32(raw["SchemaVersion"])) return "invalid-type";
            if ((int)raw["SchemaVersion"] !=
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
        /// that is absent or fails its own check is null, meaning "could be any value".
        /// </summary>
        internal static string[] Identity(JObject raw)
        {
            return new[] {
                Guid(raw["BuffGuid"]), Guid(raw["SpellGuid"]),
                raw["CasterId"] != null && raw["CasterId"].Type ==
                    JTokenType.String && !string.IsNullOrWhiteSpace(
                        (string)raw["CasterId"]) ? (string)raw["CasterId"] : null,
                raw["AbilityScore"] != null && raw["AbilityScore"].Type ==
                    JTokenType.String && AbilityScore((string)raw["AbilityScore"])
                        != BrownFurAbilityScore.None ?
                    (string)raw["AbilityScore"] : null,
                raw["CarrierFamily"] != null && raw["CarrierFamily"].Type ==
                    JTokenType.String && BrownFurModifierAdjustmentPolicy
                        .IsSupportedCarrier((string)raw["CarrierFamily"]) ?
                    (string)raw["CarrierFamily"] : null };
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
            return value is int || (value is long && (long)value >= int.MinValue &&
                (long)value <= int.MaxValue);
        }

        private static bool IsInt64(JToken token)
        {
            if (token == null || token.Type != JTokenType.Integer) return false;
            object value = ((JValue)token).Value;
            return value is int || value is long;
        }

        /// <summary>Compact JSON text of an entry, for diagnostics.</summary>
        internal static string Text(BrownFurPersistedModifierRecord record)
        {
            JToken token = record == null ? JValue.CreateNull() :
                record.Raw ?? Write(record);
            return token.ToString(Formatting.None);
        }
    }
}
