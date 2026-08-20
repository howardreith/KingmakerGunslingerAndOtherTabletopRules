using System;
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
        private static string _sourcePath, _destinationPath, _expectedHash, _observedHash;
        private static string _lastEvent, _lastSource, _lastEmitter, _lastFault;
        private static FirearmKind _lastKind; private static uint _lastPlayingId;
        internal static int AcceptedPosts { get { lock(Sync){return _machine==null?0:_machine.AcceptedPosts;} } }
        internal static string LastEventName { get { lock(Sync){return _lastEvent;} } }
        internal static uint LastPlayingId { get { lock(Sync){return _lastPlayingId;} } }

        internal static void Configure(ModContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            lock (Sync) { if (_machine != null) return; _context=context; _machine=new FirearmSoundStateMachine(new KingmakerWwiseEngine()); }
            try
            {
                string manifestPath=Path.Combine(context.ModEntry.Path,"assets","soundbanks","firearm-soundbank-manifest.json");
                FirearmSoundBankManifest manifest=FirearmSoundBankManifestLoader.Load(manifestPath);
                FirearmSoundBankStageResult staged=new FirearmSoundBankStager().Stage(context.ModEntry.Path,Application.dataPath,manifest);
                lock(Sync){_sourcePath=staged.SourcePath;_destinationPath=staged.DestinationPath;_expectedHash=manifest.Sha256;_observedHash=staged.ObservedHash;_machine.MarkStaged();}
                context.Logger.Info("audio","bank.staged","decision="+staged.Status+";source="+staged.SourcePath+";destination="+staged.DestinationPath+";sha256="+staged.ObservedHash);
                context.ModEntry.OnUpdate += OnUpdate;
                EnsureReady();
            }
            catch(Exception e){ lock(Sync){_lastFault=e.GetType().Name+": "+e.Message;_machine.Fault(e);} context.Logger.Warning("audio","configuration.disabled","Custom firearm audio is disabled without affecting firearm mechanics: "+e.Message); }
        }

        internal static FirearmSoundPostResult TryPostCommittedDischarge(FirearmKind kind, UnitEntityData wielder, string source)
        {
            string eventName; if(!FirearmSoundEventCatalog.TryResolve(kind,out eventName)) return Record(false,kind,null,source,null,0,"Unsupported firearm kind.");
            GameObject emitter=wielder==null||wielder.View==null?null:wielder.View.gameObject;
            if(emitter==null) return Record(false,kind,eventName,source,null,0,"A live unit emitter was unavailable.");
            FirearmSoundStateMachine machine; lock(Sync){machine=_machine;} uint id=machine==null?0:machine.TryPost(eventName,emitter); return Record(id!=0,kind,eventName,source,emitter.name,id,id==0?"Wwise rejected or was not ready.":null);
        }

        internal static FirearmSoundPostResult TryPostGlobalPistolPreview()
        { return TryPostGlobalPreview(FirearmKind.Pistol); }

        internal static FirearmSoundPostResult TryPostGlobalPreview(FirearmKind kind)
        {
            GameObject emitter=Game.Instance==null||Game.Instance.UI==null||Game.Instance.UI.Common==null?null:Game.Instance.UI.Common.gameObject;
            string eventName; if(!FirearmSoundEventCatalog.TryResolve(kind,out eventName)) return Record(false,kind,null,"development-global-preview",null,0,"Unsupported firearm kind."); uint id;
            FirearmSoundStateMachine machine;lock(Sync){machine=_machine;}id=emitter==null||machine==null?0:machine.TryPost(eventName,emitter); return Record(id!=0,kind,eventName,"development-global-preview",emitter==null?null:emitter.name,id,id==0?"Global preview emitter or Wwise was unavailable.":null);
        }

        internal static string RetryConfigurationForDevelopment()
        {
            ModContext context; lock(Sync){context=_context;if(_machine!=null&&_machine.State!=FirearmSoundState.Faulted)return "Retry rejected: audio is not faulted.";_machine=null;_lastFault=null;}
            if(context==null)return "Retry rejected: mod context is unavailable.";
            context.ModEntry.OnUpdate-=OnUpdate; Configure(context); return Describe();
        }

        internal static string Describe()
        {
            lock(Sync){return "state="+(_machine==null?FirearmSoundState.NotConfigured:_machine.State)+";source="+(_sourcePath??"<none>")+";destination="+(_destinationPath??"<none>")+";expectedHash="+(_expectedHash??"<none>")+";observedHash="+(_observedHash??"<none>")+";loadAttempts="+(_machine==null?0:_machine.LoadAttempts)+";postAttempts="+(_machine==null?0:_machine.PostAttempts)+";acceptedPosts="+(_machine==null?0:_machine.AcceptedPosts)+";lastKind="+_lastKind+";lastEvent="+(_lastEvent??"<none>")+";lastSource="+(_lastSource??"<none>")+";lastEmitter="+(_lastEmitter??"<none>")+";lastPlayingId="+_lastPlayingId+";fault="+(_lastFault??(_machine==null?null:_machine.LastFault)??"<none>");}
        }

        private static void OnUpdate(UnityModManagerNet.UnityModManager.ModEntry modEntry,float delta){EnsureReady();}
        private static void EnsureReady(){FirearmSoundStateMachine machine;ModContext context;lock(Sync){machine=_machine;context=_context;}bool ready=machine!=null&&machine.EnsureReady();if(ready&&context!=null){context.ModEntry.OnUpdate-=OnUpdate;context.Logger.Info("audio","bank.ready","Bank load requested once after Wwise initialization; successful PostEvent remains required acceptance evidence.");}}
        private static FirearmSoundPostResult Record(bool accepted,FirearmKind kind,string eventName,string source,string emitter,uint id,string fault){lock(Sync){_lastKind=kind;_lastEvent=eventName;_lastSource=source;_lastEmitter=emitter;_lastPlayingId=id;_lastFault=fault;}return new FirearmSoundPostResult(accepted,kind,eventName,source,emitter,id,fault);}
        private sealed class KingmakerWwiseEngine:IFirearmSoundEngine
        { public bool IsInitialized(){return AkSoundEngine.IsInitialized();} public void LoadBank(string bankName){AkBankManager.LoadBank(bankName,false,false);} public uint PostEvent(string eventName,object emitter){return AkSoundEngine.PostEvent(eventName,(GameObject)emitter);} }
    }
}
