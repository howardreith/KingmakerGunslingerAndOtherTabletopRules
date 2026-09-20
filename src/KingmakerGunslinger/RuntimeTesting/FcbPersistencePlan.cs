using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Guarded two-phase plan for the authorized disposable Favored Class
    // persistence acceptance. The prepare phase starts from the positively
    // identified original working save; the verify phase starts in a fresh
    // process from the exact prepared disposable save, proven by the prior
    // successful run's receipt (run id, dll hash, phase and distinct process)
    // and the captured save-file hash. The owner's live campaign and the
    // protected baseline save are never read or written.
    internal sealed class FcbPersistencePlan
    {
        internal readonly string Transaction, Phase, OutputName, InputPath;
        internal readonly JObject Expected, Input;
        internal readonly WorkingSaveSmokeIdentity Identity;
        internal FcbPersistencePlan(RuntimeTestRequest request)
        {
            if (request.Scenario != FcbPersistenceIdentity.Scenario ||
                !request.ExitAfterCompletion || !ValidParameters(request.Parameters))
                throw new InvalidOperationException(
                    "Favored Class persistence request is not guarded.");
            string path = (string)request.Parameters["planPath"];
            RequireEvidencePath(path);
            var plan = JObject.Parse(File.ReadAllText(path));
            string[] fields = { "schemaVersion", "transactionId", "phase",
                "version", "dllSha256", "input", "expected", "outputSaveName",
                "previousResultPath" };
            if (plan.Properties().Count() != fields.Length ||
                plan.Properties().Any(value => !fields.Contains(value.Name)) ||
                plan.Value<int>("schemaVersion") != 1 ||
                plan.Value<string>("version") != request.ExpectedModVersion ||
                plan.Value<string>("dllSha256") != Hash(typeof(FcbPersistencePlan).Assembly.Location))
                throw new InvalidOperationException(
                    "Favored Class persistence plan source/package identity differs.");
            Transaction = plan.Value<string>("transactionId");
            Phase = plan.Value<string>("phase");
            if (!FcbPersistenceIdentity.ValidTransaction(Transaction) ||
                !FcbPersistenceIdentity.ValidPhase(Phase) ||
                (string)request.Parameters["phase"] != Phase ||
                new DirectoryInfo(Path.GetDirectoryName(path)).Name !=
                    "word-of-recall-fcb-persistence-" + Transaction)
                throw new InvalidOperationException(
                    "Favored Class persistence plan transaction/phase identity is ambiguous.");
            Input = plan["input"] as JObject;
            Expected = plan["expected"] as JObject;
            OutputName = Phase == "verify" ? null :
                FcbPersistenceIdentity.Name(Transaction, Phase);
            if ((string)plan["outputSaveName"] != OutputName || Input == null ||
                (string)Input["name"] != (Phase == "prepare" ?
                    FcbPersistenceIdentity.Working :
                    FcbPersistenceIdentity.Name(Transaction, "prepare")) ||
                (string)request.Parameters["saveName"] != (string)Input["name"] ||
                !FcbPersistenceIdentity.MatchesFile((string)Input["name"],
                    (string)Input["file"]))
                throw new InvalidOperationException(
                    "Favored Class persistence plan does not identify the exact allowed input/output names.");
            InputPath = Path.GetFullPath((string)Input["path"]);
            if (!string.Equals(Path.GetDirectoryName(InputPath),
                    Path.GetFullPath(Kingmaker.Game.Instance.SaveManager.SavePath),
                    StringComparison.OrdinalIgnoreCase) ||
                Path.GetFileName(InputPath) != (string)Input["file"] ||
                Hash(InputPath) != (string)Input["sha256"])
                throw new InvalidOperationException(
                    "The exact Favored Class persistence input save no longer matches its captured hash.");
            if (Phase == "prepare")
            {
                if (Expected != null || plan["previousResultPath"].Type != JTokenType.Null ||
                    (string)Input["file"] != WorkingSaveSmokeScenario.ExpectedFile ||
                    (string)Input["gameId"] != WorkingSaveSmokeScenario.ExpectedGameId ||
                    (string)Input["gameName"] != WorkingSaveSmokeScenario.ExpectedGameName ||
                    (string)Input["areaName"] != WorkingSaveSmokeScenario.ExpectedArea ||
                    (int)Input["partyCount"] != WorkingSaveSmokeScenario.ExpectedPartyCount)
                    throw new InvalidOperationException(
                        "The prepare phase must start from the positively identified original working save.");
            }
            else
            {
                string previous = (string)plan["previousResultPath"];
                RequireEvidencePath(previous);
                var result = JObject.Parse(File.ReadAllText(previous));
                var receipt = JObject.Parse(File.ReadAllText(
                    Path.Combine(Path.GetDirectoryName(previous),
                        "word-of-recall-favored-class-persistence.json")));
                if ((string)result["status"] != "PASS" ||
                    (string)result["scenario"] != FcbPersistenceIdentity.Scenario ||
                    (string)result["runId"] != (string)receipt["runId"] ||
                    (string)receipt["transactionId"] != Transaction ||
                    (string)receipt["phase"] != "prepare" ||
                    (string)receipt["dllSha256"] != (string)plan["dllSha256"] ||
                    (int)receipt["processId"] == Process.GetCurrentProcess().Id ||
                    Expected == null ||
                    !JToken.DeepEquals(Expected, receipt["expected"]) ||
                    !JToken.DeepEquals(Input, receipt["savedInfo"]))
                    throw new InvalidOperationException(
                        "The fresh verify process lacks the exact preceding successful prepare receipt.");
            }
            Identity = new WorkingSaveSmokeIdentity((string)Input["name"],
                (string)Input["file"], (string)Input["gameName"],
                (string)Input["gameId"], (string)Input["areaName"],
                (int)Input["partyCount"]);
        }

        internal static bool ValidParameters(JObject values)
        {
            return values != null && values.Count == 3 &&
                values["saveName"]?.Type == JTokenType.String &&
                values["phase"]?.Type == JTokenType.String &&
                values["planPath"]?.Type == JTokenType.String &&
                FcbPersistenceIdentity.ValidPhase((string)values["phase"]) &&
                ((string)values["phase"] == "prepare"
                    ? (string)values["saveName"] == FcbPersistenceIdentity.Working
                    : ((string)values["saveName"]).StartsWith("KMG_FCB_PERSISTENCE_",
                        StringComparison.Ordinal));
        }

        private static void RequireEvidencePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Path.IsPathRooted(path) ||
                !Path.GetFullPath(path).StartsWith(RuntimeTestRequest.EvidenceRoot +
                    Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
                !File.Exists(path))
                throw new InvalidOperationException(
                    "Favored Class persistence provenance must be an existing guarded evidence file.");
            for (var part = new FileInfo(path) as FileSystemInfo; part != null;
                part = part is FileInfo ? ((FileInfo)part).Directory : ((DirectoryInfo)part).Parent)
                if ((part.Attributes & FileAttributes.ReparsePoint) != 0)
                    throw new InvalidOperationException(
                        "Favored Class persistence provenance cannot cross a reparse point.");
        }

        internal static string Hash(string path)
        {
            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(stream))
                    .Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}
