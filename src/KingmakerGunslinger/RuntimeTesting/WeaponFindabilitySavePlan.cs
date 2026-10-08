using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed class WeaponFindabilitySavePlan
    {
        internal readonly string Transaction, Phase, OutputName, InputPath, WeaponKey;
        internal readonly JObject Input, Expected;
        internal readonly WorkingSaveSmokeIdentity Identity;
        internal readonly string LeasePath;
        internal WeaponFindabilitySavePlan(RuntimeTestRequest request)
        {
            if (request.Scenario != WeaponFindabilitySaveContract.Scenario || !request.ExitAfterCompletion ||
                !ValidParameters(request.Parameters)) throw new InvalidOperationException("Closed automatic persistence request required.");
            if(typeof(Kingmaker.EntitySystem.Persistence.SaveManager).Assembly.ManifestModule.ModuleVersionId.ToString("D") !=
                "07fa1e4d-8618-41b3-9b8d-faa17d3b26f7")
                throw new InvalidOperationException("The exact native save contract/MVID is unavailable.");
            string path=(string)request.Parameters["planPath"];
            RequireEvidencePath(path);
            var plan=JObject.Parse(File.ReadAllText(path));
            string[] keys={"schemaVersion","transactionId","phase","version","dllSha256","input","expected","previousResultPath","leasePath","weaponKey"};
            if(plan.Properties().Count()!=keys.Length || plan.Properties().Any(p=>!keys.Contains(p.Name)) ||
                (int?)plan["schemaVersion"]!=1 || (string)plan["version"]!=request.ExpectedModVersion ||
                (string)plan["dllSha256"]!=TeleportPersistencePlan.Hash(typeof(WeaponFindabilitySavePlan).Assembly.Location))
                throw new InvalidOperationException("Exact persistence artifact/plan mismatch.");
            WeaponKey=(string)plan["weaponKey"];
            if(plan["weaponKey"].Type != JTokenType.Null && (plan["weaponKey"].Type != JTokenType.String ||
                !Acquisition.CampaignWeaponRegistry.IsNamedKey(WeaponKey)))
                throw new InvalidOperationException("Only one registered named world-loot weapon may be qualified.");
            Transaction=(string)plan["transactionId"]; Phase=(string)plan["phase"];
            OutputName=WeaponFindabilitySaveContract.Name(Transaction);
            if(Phase!=(string)request.Parameters["phase"] || !WeaponFindabilitySaveContract.ValidPhase(Phase) ||
                new DirectoryInfo(Path.GetDirectoryName(path)).Name!="weapon-findability-save-"+Transaction)
                throw new InvalidOperationException("Transaction/phase mismatch.");
            LeasePath=(string)plan["leasePath"]; RequireEvidencePath(LeasePath); RequireLease();
            Input=plan["input"] as JObject; Expected=plan["expected"] as JObject;
            if(Input==null || (string)Input["name"]!=WeaponFindabilitySaveContract.InputName(Transaction,Phase) ||
                (string)request.Parameters["saveName"]!=(string)Input["name"] ||
                !WeaponFindabilitySaveContract.MatchesFile((string)Input["name"],(string)Input["file"]))
                throw new InvalidOperationException("Only the read-only seed or exact transaction input is permitted.");
            InputPath=Path.GetFullPath((string)Input["path"]);
            RequireLease();
            if(!string.Equals(Path.GetDirectoryName(InputPath),Path.GetFullPath(Kingmaker.Game.Instance.SaveManager.SavePath),StringComparison.OrdinalIgnoreCase) ||
                Path.GetFileName(InputPath)!=(string)Input["file"] || TeleportPersistencePlan.Hash(InputPath)!=(string)Input["sha256"])
                throw new InvalidOperationException("Native save directory/file/hash mismatch.");
            if(Phase=="prepare")
            {
                if(Expected!=null || plan["previousResultPath"].Type!=JTokenType.Null ||
                    (string)Input["file"]!=WorkingSaveSmokeScenario.ExpectedFile ||
                    (string)Input["gameId"]!=WorkingSaveSmokeScenario.ExpectedGameId ||
                    (string)Input["gameName"]!=WorkingSaveSmokeScenario.ExpectedGameName ||
                    (string)Input["areaName"]!=WorkingSaveSmokeScenario.ExpectedArea ||
                    (int)Input["partyCount"]!=WorkingSaveSmokeScenario.ExpectedPartyCount)
                    throw new InvalidOperationException("Prepare requires the exact qualified load-only seed.");
            }
            else
            {
                string previous=(string)plan["previousResultPath"]; RequireEvidencePath(previous);
                var result=JObject.Parse(File.ReadAllText(previous));
                var receipt=JObject.Parse(File.ReadAllText(Path.Combine(Path.GetDirectoryName(previous),"weapon-findability-save.json")));
                if((string)result["status"]!="PASS" || (string)result["scenario"]!=WeaponFindabilitySaveContract.Scenario ||
                    (string)receipt["runId"]!=(string)result["runId"] || (string)receipt["transactionId"]!=Transaction ||
                    (string)receipt["weaponKey"]!=WeaponKey ||
                    !WeaponFindabilitySaveContract.Next((string)receipt["phase"],Phase,(int)receipt["processId"],Process.GetCurrentProcess().Id) ||
                    (string)receipt["dllSha256"]!=(string)plan["dllSha256"] || Expected==null ||
                    !JToken.DeepEquals(Expected,receipt["witness"]) || !JToken.DeepEquals(Input,receipt["savedInfo"]))
                    throw new InvalidOperationException("Fresh process requires its exact preceding successful native receipt.");
            }
            Identity=new WorkingSaveSmokeIdentity((string)Input["name"],(string)Input["file"],
                (string)Input["gameName"],(string)Input["gameId"],(string)Input["areaName"],(int)Input["partyCount"]);
        }
        internal void RequireLease()
        {
            var lease=JObject.Parse(File.ReadAllText(LeasePath));
            int pid=(int)lease["ownerPid"]; var process=Process.GetProcessById(pid);
            if((string)lease["status"]!="Active" || (string)lease["transactionId"]!=Transaction ||
                (string)lease["descriptor"]!=OutputName || (string)lease["phase"]!=Phase ||
                process.HasExited || !WeaponFindabilitySaveContract.MatchesProcessStart(process.StartTime,(DateTime)lease["ownerStartedUtc"]) ||
                !WeaponFindabilitySaveContract.LeaseUnexpired(DateTime.UtcNow,(DateTime)lease["expiresUtc"]))
                throw new InvalidOperationException("Save lease is stale, foreign or closed; state="+(string)lease["status"]+
                    ";phase="+(string)lease["phase"]+";pid="+pid+";expectedStart="+((DateTime)lease["ownerStartedUtc"]).ToUniversalTime().ToString("o")+
                    ";actualStart="+process.StartTime.ToUniversalTime().ToString("o"));
            if(Phase!="prepare" && (string)lease["ownedPath"]!=InputPath && InputPath!=null)
                throw new InvalidOperationException("Save lease does not own the loaded file.");
        }
        internal static bool ValidParameters(JObject p)
        {
            return p!=null && p.Count==3 && p["saveName"]?.Type==JTokenType.String &&
                p["phase"]?.Type==JTokenType.String && p["planPath"]?.Type==JTokenType.String &&
                WeaponFindabilitySaveContract.ValidPhase((string)p["phase"]) &&
                ((string)p["phase"]=="prepare" ? (string)p["saveName"]==WeaponFindabilitySaveContract.Seed :
                    ((string)p["saveName"]).StartsWith("KMG_WEAPONS_0143_",StringComparison.Ordinal));
        }
        internal static void RequireEvidencePath(string path)
        {
            if(string.IsNullOrWhiteSpace(path) || !Path.IsPathRooted(path) || !File.Exists(path) ||
                !Path.GetFullPath(path).StartsWith(RuntimeTestRequest.EvidenceRoot+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Existing guarded evidence required.");
            for(FileSystemInfo p=new FileInfo(path);p!=null;p=p is FileInfo?((FileInfo)p).Directory:((DirectoryInfo)p).Parent)
                if((p.Attributes&FileAttributes.ReparsePoint)!=0) throw new InvalidOperationException("No reparse escape permitted.");
        }
    }
}
