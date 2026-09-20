using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Request-owned immutable receipt. No later read of a mutable evidence path
    // can change the preparation that authorizes this loaded-session operation.
    internal sealed class MagicCirclePreparationBinding
    {
        private readonly JObject _save, _artifact, _fixture, _producerArtifact;
        internal readonly string PrepareRunId, RecordSha256, ResultSha256, ProducerIdentitySha256;
        internal JObject ProducerArtifact => (JObject)_producerArtifact.DeepClone();
        internal static bool RequiresBinding(string scenario)
        {
            return scenario == "working-save-magic-circle-verify" ||
                scenario == "working-save-magic-circle-scene" ||
                scenario == "working-save-magic-circle-cleanup";
        }

        internal MagicCirclePreparationBinding(string encoded, string version)
        {
            Require(!string.IsNullOrEmpty(encoded) && encoded.Length <= 1024 * 1024, "binding size");
            var envelope = ParseStrict(encoded);
            ExactFields(envelope, "schemaVersion", "resultBase64", "resultSha256", "recordBase64", "recordSha256",
                "producerIdentityBase64", "producerIdentitySha256", "consumerArtifact");
            Require((int?)envelope["schemaVersion"] == 2, "binding schema");
            ResultSha256 = String(envelope, "resultSha256"); RecordSha256 = String(envelope, "recordSha256");
            ProducerIdentitySha256 = String(envelope, "producerIdentitySha256");
            var result = Decode(envelope, "result", ResultSha256);
            var record = Decode(envelope, "record", RecordSha256);
            var producerIdentity = Decode(envelope, "producerIdentity", ProducerIdentitySha256);
            PrepareRunId = String(record, "runId");
            Require(System.Text.RegularExpressions.Regex.IsMatch(PrepareRunId, "^[A-Za-z0-9._-]{1,100}$"), "prepare run ID");
            Require((int?)record["schemaVersion"] == 2 && (string)record["phase"] == "prepare" &&
                record["exception"]?.Type == JTokenType.Null && (string)result["runId"] == PrepareRunId &&
                (string)result["scenario"] == "working-save-magic-circle-prepare" && (string)result["status"] == "PASS" &&
                (string)result["loadedModVersion"] == version && (bool?)result["automaticExitRequested"] == true &&
                (bool?)result["automaticExitInitiated"] == true, "successful exact prepare result");
            var assertions = result["assertions"] as JArray;
            Require(assertions != null && assertions.Count > 0 && assertions.All(value => (string)value["status"] == "PASS"), "prepare assertions");
            var guard = result["workingSaveSmoke"] as JObject;
            Require(guard != null && (bool?)guard["descriptorReferenceCorrelated"] == true &&
                (bool?)guard["completionCallbackObserved"] == true && (bool?)guard["saveWritingApiObserved"] == false &&
                (bool?)guard["hooksRemoved"] == true && (int?)guard["expectedWorkingSaveRoutineCount"] == 1 &&
                (int?)guard["expectedWorkingStashedAreaCount"] >= 1, "prepare write guard");
            _save = CloneObject(record, "workingSave"); _producerArtifact = CloneObject(record, "artifact");
            _artifact = CloneObject(envelope, "consumerArtifact");
            _fixture = CloneObject(record, "fixtureIdentity");
            ExactFields(_save, "Name", "FileName", "FolderName", "GameName", "GameId", "Area");
            Require(_save.Properties().All(p => p.Value.Type == JTokenType.String && !string.IsNullOrEmpty((string)p.Value)) &&
                (string)_save["Name"] == "KMG_AUTOMATION_WORKING", "working save identity");
            var fields = guard["resolvedDescriptor"]?["safeFields"] as JArray;
            Require(fields != null && _save.Properties().All(p => fields.Count(f => (string)f["Key"] == p.Name &&
                JToken.DeepEquals(f["Value"], p.Value)) == 1), "prepare descriptor correlation");
            foreach (var artifact in new[] { _producerArtifact, _artifact }) {
                ExactFields(artifact, "version", "dllSha256", "mvid", "gitCommit");
                Require((string)artifact["version"] == version && IsHash((string)artifact["dllSha256"]) &&
                    Guid.TryParse((string)artifact["mvid"], out _) &&
                    System.Text.RegularExpressions.Regex.IsMatch((string)artifact["gitCommit"] ?? "", "^[a-f0-9]{40}$"), "artifact identity");
            }
            // The producer is the immutable successful preparation, corroborated
            // by its actual loaded-module record. The consumer is this request's
            // separately pinned build. A corrected runner can therefore clean its
            // exact old fixture without pretending to be the producer binary.
            Require((string)_producerArtifact["gitCommit"] == (string)result["gitCommit"] &&
                (string)producerIdentity["semanticVersion"] == (string)_producerArtifact["version"] &&
                (string)producerIdentity["loadedModuleSha256"] == (string)_producerArtifact["dllSha256"] &&
                (string)producerIdentity["moduleVersionId"] == (string)_producerArtifact["mvid"] &&
                (string)producerIdentity["gitCommit"] == (string)_producerArtifact["gitCommit"], "prepare loaded artifact identity");
            ExactFields(_fixture, "area", "actors", "carriers", "control", "marketReceipt", "inventory", "gold", "favoredOracle");
            Require(IsAssetId((string)_fixture["area"]) && _fixture["favoredOracle"] is JObject &&
                _fixture["actors"] is JArray actors && actors.Count == 4 &&
                actors.All(a => a is JObject && Guid.TryParse((string)a["id"], out var id) && id != Guid.Empty &&
                    !string.IsNullOrEmpty((string)a["role"]) && IsAssetId((string)a["blueprint"]) && a["books"] is JArray) &&
                actors.Select(a => (string)a["id"]).Distinct(StringComparer.Ordinal).Count() == 4 &&
                actors.Select(a => (string)a["role"]).Distinct(StringComparer.Ordinal).Count() == 4 &&
                _fixture["carriers"] is JArray carriers && carriers.Count == 8 && carriers.All(c => c is JObject &&
                    (string)c["bearer"] == (string)actors[2]["id"] &&
                    actors.Take(2).Any(a => (string)a["id"] == (string)c["caster"]) &&
                    IsAssetId((string)c["blueprint"]) && IsAssetId((string)c["sourceSpell"]) &&
                    (int?)c["level"] > 0 && (long?)c["endTimeTicks"] > 0 && c["extend"]?.Type == JTokenType.Boolean) &&
                _fixture["marketReceipt"] is JObject market && IsAssetId((string)market["TableId"]) &&
                _fixture["control"] is JArray control && control.Count == 1 && control.All(c => c is JObject &&
                    (string)c["source"] == (string)actors[0]["id"] && (long?)c["endTimeTicks"] > 0) &&
                _fixture["inventory"] is JArray inventory && inventory.All(i => i.Type == JTokenType.String) &&
                _fixture["gold"]?.Type == JTokenType.Integer, "prepared fixture identity");
        }

        internal void Execute(JObject save, JObject artifact, JObject fixture, Action authorizedAction)
        {
            // Compare in the same synchronous native invocation as the mutation.
            // Names and structural checks alone never grant this authorization.
            Require(JToken.DeepEquals(_save, save), "loaded working save mismatch");
            Require(JToken.DeepEquals(_artifact, artifact), "loaded artifact mismatch");
            // Native Newtonsoft represents (string)null as a String token, while
            // JSON reads it as Null. Compare the complete JSON values recorded by
            // the receipt, preserving every identity/field and integer deadline.
            Require(JToken.DeepEquals(_fixture, ParseStrict(fixture.ToString(Formatting.None))), "loaded prepared fixture mismatch");
            if (authorizedAction == null) throw new ArgumentNullException(nameof(authorizedAction));
            authorizedAction();
        }

        internal static bool Valid(string encoded, string version)
        { try { new MagicCirclePreparationBinding(encoded, version); return true; } catch { return false; } }

        internal static string Hash(byte[] bytes)
        { using (var hash = SHA256.Create()) return BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant(); }
        private static bool IsAssetId(string value)
        { return value != null && value.Length == 32 && System.Text.RegularExpressions.Regex.IsMatch(value, "^[a-f0-9]{32}$"); }
        private static bool IsHash(string value)
        { return value != null && value.Length == 64 && System.Text.RegularExpressions.Regex.IsMatch(value, "^[a-f0-9]{64}$"); }
        private static JObject Decode(JObject envelope, string prefix, string hash)
        {
            Require(IsHash(hash), prefix + " hash format");
            var bytes = Convert.FromBase64String(String(envelope, prefix + "Base64"));
            Require(bytes.Length > 0 && bytes.Length <= 512 * 1024 && Hash(bytes) == hash, prefix + " hash mismatch");
            return ParseStrict(new UTF8Encoding(false, true).GetString(bytes).TrimStart('\uFEFF'));
        }
        private static JObject CloneObject(JObject value, string name)
        { Require(value[name] is JObject, name + " object"); return (JObject)value[name].DeepClone(); }
        private static string String(JObject value, string name)
        { Require(value[name]?.Type == JTokenType.String, name + " string"); return (string)value[name]; }
        private static void ExactFields(JObject value, params string[] names)
        { Require(value.Properties().Count() == names.Length && value.Properties().All(p => names.Contains(p.Name)), "exact binding fields"); }
        private static void Require(bool condition, string message)
        { if (!condition) throw new InvalidOperationException("Magic Circle preparation rejected: " + message); }
        private static JObject ParseStrict(string text)
        {
            using (var reader = new JsonTextReader(new StringReader(text)) { DateParseHandling = DateParseHandling.None, MaxDepth = 32 }) {
                var objects = new Stack<HashSet<string>>();
                while (reader.Read()) {
                    if (reader.TokenType == JsonToken.StartObject) objects.Push(new HashSet<string>(StringComparer.Ordinal));
                    if (reader.TokenType == JsonToken.PropertyName) Require(objects.Peek().Add((string)reader.Value), "duplicate JSON member");
                    if (reader.TokenType == JsonToken.EndObject) objects.Pop();
                    Require(reader.TokenType != JsonToken.Comment && reader.TokenType != JsonToken.Undefined, "nonstandard JSON");
                }
            }
            using (var reader = new JsonTextReader(new StringReader(text)) { DateParseHandling = DateParseHandling.None })
                return JObject.Load(reader);
        }
    }
}
