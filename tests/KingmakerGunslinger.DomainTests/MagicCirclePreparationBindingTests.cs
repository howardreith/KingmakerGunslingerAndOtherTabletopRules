using System;
using System.Linq;
using System.Text;
using KingmakerGunslinger.RuntimeTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class MagicCirclePreparationBindingTests
    {
        private const string Version = "fixture-version";
        private static JObject Save() { return new JObject { ["Name"] = "KMG_AUTOMATION_WORKING", ["FileName"] = "Working.zks",
            ["FolderName"] = "Working.zks", ["GameName"] = "fixture", ["GameId"] = "fixture-game", ["Area"] = "fixture-area" }; }
        private static JObject Artifact() { return new JObject { ["version"] = Version, ["dllSha256"] = new string('a', 64),
            ["mvid"] = "ea9ac240-4984-421b-b2e1-4336fa5770c6", ["gitCommit"] = new string('b', 40) }; }
        private static JObject Fixture()
        {
            var actors = new JArray(Enumerable.Range(0, 4).Select(i => new JObject { ["role"] = "role" + i,
                ["id"] = Guid.NewGuid().ToString("D"), ["blueprint"] = new string('a', 32), ["books"] = new JArray(),
                // Native projection uses implicit string tokens for absent held
                // touches. JSON writes null, then parses it as JTokenType.Null.
                ["heldDelivery"] = (string)null, ["heldRoot"] = (string)null }));
            return new JObject { ["actors"] = actors, ["area"] = new string('b', 32), ["favoredOracle"] = new JObject(),
                ["carriers"] = new JArray(Enumerable.Range(0, 8).Select(i => new JObject { ["bearer"] = actors[2]["id"],
                    ["caster"] = actors[i % 2]["id"], ["blueprint"] = new string('c', 32), ["sourceSpell"] = new string('d', 32),
                    ["level"] = 8, ["endTimeTicks"] = 1234567L, ["extend"] = true })),
                ["marketReceipt"] = new JObject { ["TableId"] = new string('e', 32) },
                ["control"] = new JArray(new JObject { ["source"] = actors[0]["id"], ["endTimeTicks"] = 1234567L }),
                ["inventory"] = new JArray(), ["gold"] = 123 };
        }
        private static JObject Record(JObject fixture) { return new JObject { ["schemaVersion"] = 2, ["phase"] = "prepare",
            ["runId"] = Guid.NewGuid().ToString("N"), ["exception"] = JValue.CreateNull(), ["workingSave"] = Save(),
            ["artifact"] = Artifact(), ["fixtureIdentity"] = fixture.DeepClone() }; }
        private static JObject Result(JObject record) { return new JObject { ["runId"] = record["runId"],
            ["scenario"] = "working-save-magic-circle-prepare", ["status"] = "PASS", ["loadedModVersion"] = Version,
            ["gitCommit"] = Artifact()["gitCommit"], ["automaticExitRequested"] = true, ["automaticExitInitiated"] = true,
            ["assertions"] = new JArray(new JObject { ["status"] = "PASS" }), ["workingSaveSmoke"] = new JObject {
                ["descriptorReferenceCorrelated"] = true, ["completionCallbackObserved"] = true,
                ["saveWritingApiObserved"] = false, ["hooksRemoved"] = true, ["expectedWorkingSaveRoutineCount"] = 1,
                ["expectedWorkingStashedAreaCount"] = 1, ["resolvedDescriptor"] = new JObject { ["safeFields"] =
                    new JArray(Save().Properties().Select(p => new JObject { ["Key"] = p.Name, ["Value"] = p.Value })) } } }; }
        private static JObject Envelope(JObject record, JObject result = null, JObject consumer = null, JObject producerIdentity = null)
        {
            var a = Encoding.UTF8.GetBytes(record.ToString(Formatting.None));
            var b = Encoding.UTF8.GetBytes((result ?? Result(record)).ToString(Formatting.None));
            var producer = (JObject)record["artifact"];
            var c = Encoding.UTF8.GetBytes((producerIdentity ?? new JObject {
                ["semanticVersion"] = producer["version"], ["loadedModuleSha256"] = producer["dllSha256"],
                ["moduleVersionId"] = producer["mvid"], ["gitCommit"] = producer["gitCommit"] }).ToString(Formatting.None));
            return new JObject { ["schemaVersion"] = 2, ["recordBase64"] = Convert.ToBase64String(a),
                ["recordSha256"] = MagicCirclePreparationBinding.Hash(a), ["resultBase64"] = Convert.ToBase64String(b),
                ["resultSha256"] = MagicCirclePreparationBinding.Hash(b), ["producerIdentityBase64"] = Convert.ToBase64String(c),
                ["producerIdentitySha256"] = MagicCirclePreparationBinding.Hash(c), ["consumerArtifact"] = (consumer ?? Artifact()).DeepClone() };
        }
        private static void Reject(Action action, string message)
        { bool rejected = false; try { action(); } catch (InvalidOperationException) { rejected = true; } Assertions.True(rejected, message); }

        internal static void DifferentValidPreparationCannotMutate()
        {
            var a = Fixture(); var b = Fixture();
            var consumer = Artifact(); consumer["dllSha256"] = new string('c', 64);
            var receiptA = new MagicCirclePreparationBinding(Envelope(Record(a), consumer: consumer).ToString(), Version);
            var receiptB = new MagicCirclePreparationBinding(Envelope(Record(b), consumer: consumer).ToString(), Version);
            int mutations = 0, writes = 0;
            Action cleanup = () => { mutations++; writes++; };
            Reject(() => receiptA.Execute(Save(), consumer, b, cleanup), "Valid A must not authorize loaded valid B.");
            Reject(() => receiptB.Execute(Save(), Artifact(), b, cleanup), "Even the producer binary cannot substitute for the expected consumer.");
            Assertions.True(mutations == 0 && writes == 0, "Mismatch must reject before mutation and save authorization.");
            receiptB.Execute(Save(), consumer, b, cleanup);
            Assertions.True(mutations == 1 && writes == 1, "The legitimate matching fixture executes once.");
        }

        internal static void MalformedAndMismatchedBindingsFailClosed()
        {
            var record = Record(Fixture()); var envelope = Envelope(record);
            foreach (var text in new[] { "", "null", "{}", "{", envelope.ToString().Replace("\"schemaVersion\": 2", "\"schemaVersion\": 2, \"schemaVersion\": 2") })
                Assertions.True(!MagicCirclePreparationBinding.Valid(text, Version), "Missing/malformed/duplicate binding rejected.");
            var malformed = Record(Fixture()); malformed["fixtureIdentity"]["carriers"] = new JArray(Enumerable.Range(0, 8));
            Assertions.True(!MagicCirclePreparationBinding.Valid(Envelope(malformed).ToString(), Version), "Carrier count alone is not a fixture identity.");
            foreach (var key in new[] { "recordSha256", "resultSha256", "recordBase64", "resultBase64", "producerIdentityBase64", "producerIdentitySha256" }) {
                var changed = (JObject)envelope.DeepClone(); changed[key] = "wrong";
                Assertions.True(!MagicCirclePreparationBinding.Valid(changed.ToString(), Version), "Corrupted receipt rejected: " + key);
            }
            var wrongResult = Result(record); wrongResult["runId"] = "another-valid-run";
            Assertions.True(!MagicCirclePreparationBinding.Valid(Envelope(record, wrongResult).ToString(), Version), "Different preparation result cannot bind record.");
            wrongResult = Result(record); wrongResult["workingSaveSmoke"]["expectedWorkingSaveRoutineCount"] = 0;
            Assertions.True(!MagicCirclePreparationBinding.Valid(Envelope(record, wrongResult).ToString(), Version), "Unwritten preparation is invalid.");
            Assertions.True(!MagicCirclePreparationBinding.Valid(envelope.ToString(), "different-version"), "Wrong version rejected.");
            Assertions.True(!MagicCirclePreparationBinding.Valid(Envelope(record, producerIdentity: new JObject {
                ["semanticVersion"] = Version, ["loadedModuleSha256"] = new string('c', 64),
                ["moduleVersionId"] = Artifact()["mvid"], ["gitCommit"] = Artifact()["gitCommit"] }).ToString(), Version),
                "Producer record must match its immutable actual loaded-module identity.");
            var binding = new MagicCirclePreparationBinding(envelope.ToString(), Version);
            int mutations = 0;
            var wrongSave = Save(); wrongSave["GameId"] = "other-game";
            Reject(() => binding.Execute(wrongSave, Artifact(), (JObject)record["fixtureIdentity"], () => mutations++), "Different save rejected.");
            var wrongArtifact = Artifact(); wrongArtifact["dllSha256"] = new string('c', 64);
            Reject(() => binding.Execute(Save(), wrongArtifact, (JObject)record["fixtureIdentity"], () => mutations++), "Different binary rejected.");
            Assertions.True(mutations == 0, "Every mismatch remains mutation-free.");
        }

        internal static void ReceiptOwnsItsImmutableSnapshot()
        {
            var fixture = Fixture(); var record = Record(fixture);
            var binding = new MagicCirclePreparationBinding(Envelope(record).ToString(), Version);
            record["fixtureIdentity"]["actors"][0]["id"] = Guid.NewGuid().ToString("D");
            int calls = 0; binding.Execute(Save(), Artifact(), fixture, () => calls++);
            Reject(() => binding.Execute(Save(), Artifact(), (JObject)record["fixtureIdentity"], () => calls++), "Later receipt mutations cannot change authority.");
            Assertions.True(calls == 1, "Original immutable snapshot remains authoritative.");
        }
    }
}
