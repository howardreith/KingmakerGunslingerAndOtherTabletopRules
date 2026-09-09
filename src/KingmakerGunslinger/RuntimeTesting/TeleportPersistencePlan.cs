using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed class TeleportPersistencePlan
    {
        internal readonly string Transaction, Phase, OutputName, InputPath;
        internal readonly JObject Expected, Input;
        internal readonly WorkingSaveSmokeIdentity Identity;
        internal TeleportPersistencePlan(RuntimeTestRequest request)
        {
            if (request.Scenario != TeleportPersistenceIdentity.Scenario || !request.ExitAfterCompletion ||
                !ValidParameters(request.Parameters)) throw new InvalidOperationException("Persistence request is not guarded.");
            string path = (string)request.Parameters["planPath"];
            RequireEvidencePath(path);
            var plan = JObject.Parse(File.ReadAllText(path));
            string[] fields = { "schemaVersion", "transactionId", "phase", "version", "dllSha256", "input", "expected", "outputSaveName", "previousResultPath" };
            if (plan.Properties().Count() != fields.Length || plan.Properties().Any(value => !fields.Contains(value.Name)) ||
                plan.Value<int>("schemaVersion") != 1 || plan.Value<string>("version") != request.ExpectedModVersion ||
                plan.Value<string>("dllSha256") != Hash(typeof(TeleportPersistencePlan).Assembly.Location))
                throw new InvalidOperationException("Persistence plan source/package identity differs.");
            Transaction = plan.Value<string>("transactionId"); Phase = plan.Value<string>("phase");
            if (!TeleportPersistenceIdentity.ValidTransaction(Transaction) || !TeleportPersistenceIdentity.ValidPhase(Phase) ||
                (string)request.Parameters["phase"] != Phase ||
                new DirectoryInfo(Path.GetDirectoryName(path)).Name != "teleportation-persistence-" + Transaction)
                throw new InvalidOperationException("Persistence plan transaction/phase identity is ambiguous.");
            Input = plan["input"] as JObject; Expected = plan["expected"] as JObject;
            OutputName = Phase == "D" ? null : TeleportPersistenceIdentity.Name(Transaction, Phase);
            if ((string)plan["outputSaveName"] != OutputName || Input == null ||
                (string)Input["name"] != TeleportPersistenceIdentity.InputName(Transaction, Phase) ||
                (string)request.Parameters["saveName"] != (string)Input["name"] ||
                !TeleportPersistenceIdentity.MatchesFile((string)Input["name"], (string)Input["file"]))
                throw new InvalidOperationException("Persistence plan does not identify the exact allowed input/output names.");
            InputPath = Path.GetFullPath((string)Input["path"]);
            if (!string.Equals(Path.GetDirectoryName(InputPath), Path.GetFullPath(Kingmaker.Game.Instance.SaveManager.SavePath), StringComparison.OrdinalIgnoreCase) ||
                Path.GetFileName(InputPath) != (string)Input["file"] || Hash(InputPath) != (string)Input["sha256"])
                throw new InvalidOperationException("The exact persistence input save no longer matches its captured hash.");
            if (Phase == "A")
            {
                if (Expected != null || plan["previousResultPath"].Type != JTokenType.Null ||
                    (string)Input["file"] != WorkingSaveSmokeScenario.ExpectedFile ||
                    (string)Input["gameId"] != WorkingSaveSmokeScenario.ExpectedGameId ||
                    (string)Input["gameName"] != WorkingSaveSmokeScenario.ExpectedGameName ||
                    (string)Input["areaName"] != WorkingSaveSmokeScenario.ExpectedArea ||
                    (int)Input["partyCount"] != WorkingSaveSmokeScenario.ExpectedPartyCount)
                    throw new InvalidOperationException("Phase A must start from the positively identified original working save.");
            }
            else
            {
                string previous = (string)plan["previousResultPath"]; RequireEvidencePath(previous);
                var result = JObject.Parse(File.ReadAllText(previous));
                var receipt = JObject.Parse(File.ReadAllText(Path.Combine(Path.GetDirectoryName(previous), "teleportation-persistence.json")));
                string priorPhase = Phase == "B" ? "A" : Phase == "C" ? "B" : "C";
                if ((string)result["status"] != "PASS" || (string)result["scenario"] != TeleportPersistenceIdentity.Scenario ||
                    (string)result["runId"] != (string)receipt["runId"] || (string)receipt["transactionId"] != Transaction ||
                    (string)receipt["phase"] != priorPhase || (string)receipt["dllSha256"] != (string)plan["dllSha256"] ||
                    (int)receipt["processId"] == Process.GetCurrentProcess().Id || Expected == null ||
                    !JToken.DeepEquals(Expected, receipt["finalSnapshot"]) || !JToken.DeepEquals(Input, receipt["savedInfo"]))
                    throw new InvalidOperationException("Fresh-process input lacks the exact preceding successful native save receipt.");
            }
            Identity = new WorkingSaveSmokeIdentity((string)Input["name"], (string)Input["file"],
                (string)Input["gameName"], (string)Input["gameId"], (string)Input["areaName"], (int)Input["partyCount"]);
        }
        internal static bool ValidParameters(JObject values)
        {
            return values != null && values.Count == 3 && values["saveName"]?.Type == JTokenType.String &&
                values["phase"]?.Type == JTokenType.String && values["planPath"]?.Type == JTokenType.String &&
                TeleportPersistenceIdentity.ValidPhase((string)values["phase"]) &&
                (values.Value<string>("phase") == "A" ? values.Value<string>("saveName") == TeleportPersistenceIdentity.Working :
                    values.Value<string>("saveName").StartsWith("KMG_TELEPORT_PERSISTENCE_", StringComparison.Ordinal));
        }
        private static void RequireEvidencePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Path.IsPathRooted(path) ||
                !Path.GetFullPath(path).StartsWith(RuntimeTestRequest.EvidenceRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
                !File.Exists(path)) throw new InvalidOperationException("Persistence provenance must be an existing guarded evidence file.");
            for (var part = new FileInfo(path) as FileSystemInfo; part != null; part = part is FileInfo ? ((FileInfo)part).Directory : ((DirectoryInfo)part).Parent)
                if ((part.Attributes & FileAttributes.ReparsePoint) != 0) throw new InvalidOperationException("Persistence provenance cannot cross a reparse point.");
        }
        internal static string Hash(string path)
        {
            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
        }
    }
}
