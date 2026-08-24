using System;
using System.Collections.Generic;
using System.IO;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;
using UnityEngine;

namespace KingmakerGunslinger.Audio
{
    internal static class FirearmSoundRuntime
    {
        private static readonly object Sync = new object();
        private static FirearmSoundStateMachine _machine;
        private static ModContext _context;
        private static string _manifestPath, _manifestHash, _manifestEncoding,
            _manifestBom, _rawSchemaToken, _schemaTokenType;
        private static int _manifestByteLength, _manifestSchemaVersion;
        private static string _sourcePath, _destinationPath, _expectedHash,
            _observedHash, _destinationHash;
        private static string _lastEvent, _lastSource, _lastEmitter, _lastFault;
        private static FirearmKind _lastKind;
        private static uint _lastPlayingId;
        private static int _configurationAttempts, _stageAttempts,
            _committedDischargeAttempts;
        private static readonly Dictionary<string,int> CommittedAttemptsBySource =
            new Dictionary<string,int>(StringComparer.Ordinal);
        private static bool _readyLogged, _loadFaultLogged;
        internal static int AcceptedPosts { get { lock(Sync){return _machine==null?0:_machine.AcceptedPosts;} } }
        internal static string LastEventName { get { lock(Sync){return _lastEvent;} } }
        internal static uint LastPlayingId { get { lock(Sync){return _lastPlayingId;} } }
        internal static int GetCommittedDischargeAttempts(string source)
        {
            lock(Sync)
            {
                int attempts;
                return source != null && CommittedAttemptsBySource.TryGetValue(
                    source,out attempts) ? attempts : 0;
            }
        }

        internal static void Configure(ModContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            lock (Sync)
            {
                if (_machine != null) return;
                _context = context;
                _machine = new FirearmSoundStateMachine(
                    new KingmakerWwiseEngine());
                _configurationAttempts++;
                _readyLogged = false;
                _loadFaultLogged = false;
            }
            string failureStage = "manifest.read";
            try
            {
                string manifestPath = Path.Combine(
                    context.ModEntry.Path,
                    "assets",
                    "soundbanks",
                    "firearm-soundbank-manifest.json");
                FirearmSoundBankManifestDocument document =
                    FirearmSoundBankManifestLoader.Read(manifestPath);
                lock (Sync)
                {
                    _manifestPath = document.PathIdentity;
                    _manifestByteLength = document.ByteLength;
                    _manifestHash = document.ManifestSha256;
                    _manifestEncoding = document.Encoding;
                    _manifestBom = document.Bom;
                    _rawSchemaToken = document.RawSchemaToken;
                    _schemaTokenType = document.SchemaTokenType;
                }
                context.Logger.Info(
                    "audio",
                    "manifest.read",
                    "path=" + document.PathIdentity +
                    ";byteLength=" + document.ByteLength +
                    ";manifestSha256=" + document.ManifestSha256 +
                    ";rawSchemaToken=" + document.RawSchemaToken +
                    ";schemaTokenType=" + document.SchemaTokenType +
                    ";encoding=" + document.Encoding +
                    ";bom=" + document.Bom);

                failureStage = "manifest.semantic-validation";
                FirearmSoundBankManifest manifest = document.Manifest;
                FirearmSoundBankManifestLoader.Validate(manifest);
                lock (Sync)
                {
                    _manifestSchemaVersion = manifest.SchemaVersion;
                    _expectedHash = manifest.Sha256;
                }
                context.Logger.Info(
                    "audio",
                    "manifest.validated",
                    "schemaVersion=" + manifest.SchemaVersion +
                    ";bankName=" + manifest.BankName +
                    ";bankSha256=" + manifest.Sha256 +
                    ";eventCount=" + manifest.Events.Count);

                failureStage = "bank.validation";
                lock (Sync) { _stageAttempts++; }
                FirearmSoundBankStageResult staged =
                    new FirearmSoundBankStager().Stage(
                        context.ModEntry.Path,
                        Application.dataPath,
                        manifest);
                lock (Sync)
                {
                    _sourcePath = staged.SourcePath;
                    _destinationPath = staged.DestinationPath;
                    _observedHash = staged.ObservedHash;
                    _destinationHash = staged.DestinationHash;
                    _machine.MarkStaged();
                }
                context.Logger.Info(
                    "audio",
                    "bank.staged",
                    "decision=" + staged.Status +
                    ";source=" + staged.SourcePath +
                    ";destination=" + staged.DestinationPath +
                    ";sourceSha256=" + staged.ObservedHash +
                    ";destinationSha256=" + staged.DestinationHash +
                    ";hashParity=" + staged.HashParity);
                context.ModEntry.OnUpdate += OnUpdate;
                EnsureReady();
            }
            catch (Exception e)
            {
                var manifestException = e as FirearmSoundBankManifestException;
                var stageException = e as FirearmSoundBankStageException;
                if (manifestException != null)
                    failureStage = manifestException.StageCode;
                else if (stageException != null)
                    failureStage = stageException.StageCode;
                lock (Sync)
                {
                    _lastFault = e.GetType().Name + ": " + e.Message;
                    _machine.Fault(failureStage, e);
                }
                context.Logger.Warning(
                    "audio",
                    "configuration.disabled",
                    "stage=" + failureStage +
                    "; Custom firearm audio is disabled without affecting " +
                    "firearm mechanics: " + e.Message);
            }
        }

        internal static FirearmSoundPostResult TryPostCommittedDischarge(FirearmKind kind, UnitEntityData wielder, string source)
        {
            string eventName; if(!FirearmSoundEventCatalog.TryResolve(kind,out eventName)) return Record(false,kind,null,source,null,0,"Unsupported firearm kind.");
            lock(Sync)
            {
                _committedDischargeAttempts++;
                int attempts;
                CommittedAttemptsBySource.TryGetValue(source??"<none>",out attempts);
                CommittedAttemptsBySource[source??"<none>"]=attempts+1;
            }
            GameObject emitter=wielder==null||wielder.View==null?null:wielder.View.gameObject;
            if(emitter==null) return Record(false,kind,eventName,source,null,0,"A live unit emitter was unavailable.");
            FirearmSoundStateMachine machine;
            lock(Sync){machine=_machine;}
            uint id=machine==null?0:machine.TryPost(eventName,emitter);
            string fault=id==0?(machine==null?
                "PostEvent rejected: audio is not configured.":machine.LastFault):null;
            return Record(id!=0,kind,eventName,source,emitter.name,id,fault);
        }

        internal static FirearmSoundPostResult TryPostGlobalPistolPreview()
        { return TryPostGlobalPreview(FirearmKind.Pistol); }

        internal static FirearmSoundPostResult TryPostGlobalPreview(FirearmKind kind)
        {
            GameObject emitter=Game.Instance==null||Game.Instance.UI==null||Game.Instance.UI.Common==null?null:Game.Instance.UI.Common.gameObject;
            string eventName; if(!FirearmSoundEventCatalog.TryResolve(kind,out eventName)) return Record(false,kind,null,"development-global-preview",null,0,"Unsupported firearm kind."); uint id;
            FirearmSoundStateMachine machine;
            lock(Sync){machine=_machine;}
            id=emitter==null||machine==null?0:machine.TryPost(eventName,emitter);
            string fault=id==0?(emitter==null?
                "PostEvent rejected: global preview emitter is unavailable.":
                machine==null?"PostEvent rejected: audio is not configured.":
                machine.LastFault):null;
            return Record(id!=0,kind,eventName,"development-global-preview",emitter==null?null:emitter.name,id,fault);
        }

        internal static string RetryConfigurationForDevelopment()
        {
            ModContext context;
            lock(Sync)
            {
                context=_context;
                if(_machine!=null&&_machine.State!=FirearmSoundState.Faulted)
                    return "Retry rejected: audio is not faulted.";
                _machine=null;
                ClearConfigurationDiagnostics();
            }
            if(context==null)return "Retry rejected: mod context is unavailable.";
            context.ModEntry.OnUpdate-=OnUpdate; Configure(context); return Describe();
        }

        internal static string Describe()
        {
            lock(Sync)
            {
                return "state="+(_machine==null?FirearmSoundState.NotConfigured:_machine.State)+
                    ";configurationAttempts="+_configurationAttempts+
                    ";stageAttempts="+_stageAttempts+
                    ";manifestPath="+(_manifestPath??"<none>")+
                    ";manifestByteLength="+_manifestByteLength+
                    ";manifestSha256="+(_manifestHash??"<none>")+
                    ";rawSchemaToken="+(_rawSchemaToken??"<none>")+
                    ";schemaTokenType="+(_schemaTokenType??"<none>")+
                    ";schemaVersion="+_manifestSchemaVersion+
                    ";encoding="+(_manifestEncoding??"<none>")+
                    ";bom="+(_manifestBom??"<none>")+
                    ";source="+(_sourcePath??"<none>")+
                    ";destination="+(_destinationPath??"<none>")+
                    ";expectedHash="+(_expectedHash??"<none>")+
                    ";observedHash="+(_observedHash??"<none>")+
                    ";destinationHash="+(_destinationHash??"<none>")+
                    ";loadAttempts="+(_machine==null?0:_machine.LoadAttempts)+
                    ";postAttempts="+(_machine==null?0:_machine.PostAttempts)+
                    ";acceptedPosts="+(_machine==null?0:_machine.AcceptedPosts)+
                    ";committedDischargeAttempts="+_committedDischargeAttempts+
                    ";lastKind="+_lastKind+
                    ";lastEvent="+(_lastEvent??"<none>")+
                    ";lastSource="+(_lastSource??"<none>")+
                    ";lastEmitter="+(_lastEmitter??"<none>")+
                    ";lastPlayingId="+_lastPlayingId+
                    ";failureStage="+(_machine==null||
                        string.IsNullOrEmpty(_machine.LastFailureStage)?
                        "<none>":_machine.LastFailureStage)+
                    ";fault="+(_lastFault??(_machine==null?null:
                        _machine.LastFault)??"<none>");
            }
        }

        private static void OnUpdate(UnityModManagerNet.UnityModManager.ModEntry modEntry,float delta){EnsureReady();}
        private static void EnsureReady()
        {
            FirearmSoundStateMachine machine;
            ModContext context;
            lock(Sync){machine=_machine;context=_context;}
            bool ready=machine!=null&&machine.EnsureReady();
            if(ready&&context!=null)
            {
                bool log;
                lock(Sync){log=!_readyLogged;_readyLogged=true;}
                context.ModEntry.OnUpdate-=OnUpdate;
                if(log)
                    context.Logger.Info(
                        "audio",
                        "bank.ready",
                        "wwiseInitialized=True;bankName="+
                        FirearmSoundEventCatalog.BankName+
                        ";loadAttempts="+machine.LoadAttempts+
                        "; successful PostEvent remains required acceptance evidence.");
            }
            else if(machine!=null&&machine.State==FirearmSoundState.Faulted&&
                context!=null)
            {
                bool log;
                lock(Sync){log=!_loadFaultLogged;_loadFaultLogged=true;}
                context.ModEntry.OnUpdate-=OnUpdate;
                if(log)
                    context.Logger.Warning(
                        "audio",
                        "bank.load.failed",
                        "stage="+(machine.LastFailureStage??"bank.loading")+
                        "; "+(machine.LastFault??"Unknown bank load failure."));
            }
        }
        private static FirearmSoundPostResult Record(bool accepted,FirearmKind kind,string eventName,string source,string emitter,uint id,string fault){lock(Sync){_lastKind=kind;_lastEvent=eventName;_lastSource=source;_lastEmitter=emitter;_lastPlayingId=id;_lastFault=fault;}return new FirearmSoundPostResult(accepted,kind,eventName,source,emitter,id,fault);}

        private static void ClearConfigurationDiagnostics()
        {
            _manifestPath=null;
            _manifestHash=null;
            _manifestEncoding=null;
            _manifestBom=null;
            _rawSchemaToken=null;
            _schemaTokenType=null;
            _manifestByteLength=0;
            _manifestSchemaVersion=0;
            _sourcePath=null;
            _destinationPath=null;
            _expectedHash=null;
            _observedHash=null;
            _destinationHash=null;
            _lastFault=null;
            _readyLogged=false;
            _loadFaultLogged=false;
        }

        private sealed class KingmakerWwiseEngine:IFirearmSoundEngine
        { public bool IsInitialized(){return AkSoundEngine.IsInitialized();} public void LoadBank(string bankName){AkBankManager.LoadBank(bankName,false,false);} public uint PostEvent(string eventName,object emitter){return AkSoundEngine.PostEvent(eventName,(GameObject)emitter);} }
    }
}
