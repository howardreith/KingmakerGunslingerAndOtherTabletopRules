using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using KingmakerGunslinger.Audio;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.DomainTests
{
    internal static class FirearmAudioTests
    {
        internal static void CatalogExact()
        {
            Assertions.Equal("KMG_Firearm_Pistol_Shot", FirearmSoundEventCatalog.Resolve(FirearmKind.Pistol), "Pistol event mismatch.");
            Assertions.Equal("KMG_Firearm_Musket_Shot", FirearmSoundEventCatalog.Resolve(FirearmKind.Musket), "Musket event mismatch.");
            Assertions.Equal("KMG_Firearm_Blunderbuss_Shot", FirearmSoundEventCatalog.Resolve(FirearmKind.Blunderbuss), "Blunderbuss event mismatch.");
            Assertions.Equal("KMG_Firearm_Revolver_Shot", FirearmSoundEventCatalog.Resolve(FirearmKind.Revolver), "Revolver event mismatch.");
            Assertions.Equal("KMG_Firearm_Rifle_Shot", FirearmSoundEventCatalog.Resolve(FirearmKind.Rifle), "Rifle event mismatch.");
            Assertions.Throws<ArgumentOutOfRangeException>(() => FirearmSoundEventCatalog.Resolve(FirearmKind.Unknown), "Unknown firearm audio must fail closed.");
            Assertions.Throws<ArgumentOutOfRangeException>(() => FirearmSoundEventCatalog.Resolve((FirearmKind)99), "Undefined firearm audio must fail closed.");
        }
        internal static void ManifestValidation()
        {
            FirearmSoundBankManifest valid=Manifest(HashBytes(new byte[]{1,2,3})); FirearmSoundBankManifestValidator.Validate(valid);
            Invalid(valid,m=>m.SchemaVersion=2); Invalid(valid,m=>m.BankName="Init"); Invalid(valid,m=>m.BankFileName="Init.bnk");
            Invalid(valid,m=>m.BankFileName="..\\KMG_Firearms.bnk"); Invalid(valid,m=>m.BankFileName="C:\\KMG_Firearms.bnk");
            Invalid(valid,m=>m.Platform="Mac"); Invalid(valid,m=>m.WwiseVersion="2024.1.0"); Invalid(valid,m=>m.Sha256="00");
            Invalid(valid,m=>m.Sha256=m.Sha256.ToLowerInvariant()); Invalid(valid,m=>m.MediaEmbedded=false);
            Invalid(valid,m=>m.Events.Remove("Pistol")); Invalid(valid,m=>m.Events["Pistol"]="wrong");
            Invalid(valid,m=>m.Events["Pistol"]=m.Events["Musket"]);
        }
        internal static void StagingLifecycle()
        {
            string root=Path.Combine(Path.GetTempPath(),"kmg-audio-"+Guid.NewGuid().ToString("N"));
            try
            {
                string mod=Path.Combine(root,"mod"), data=Path.Combine(root,"Kingmaker_Data"), banks=Path.Combine(mod,"assets","soundbanks"); Directory.CreateDirectory(banks);
                byte[] bytes=Bank(false); string source=Path.Combine(banks,FirearmSoundEventCatalog.BankFileName); File.WriteAllBytes(source,bytes);
                FirearmSoundBankManifest manifest=Manifest(FirearmSoundBankStager.Hash(source)); var stager=new FirearmSoundBankStager();
                FirearmSoundBankStageResult first=stager.Stage(mod,data,manifest); Assertions.Equal(FirearmSoundBankStageStatus.Copied,first.Status,"First stage must copy.");
                string destination=first.DestinationPath; Assertions.Equal(manifest.Sha256,FirearmSoundBankStager.Hash(destination),"Destination hash mismatch.");
                string sibling=Path.Combine(Path.GetDirectoryName(destination),"Init.bnk"); File.WriteAllBytes(sibling,new byte[]{4,4});
                FirearmSoundBankStageResult second=stager.Stage(mod,data,manifest); Assertions.Equal(FirearmSoundBankStageStatus.Skipped,second.Status,"Matching destination must skip.");
                File.WriteAllBytes(destination,new byte[]{0}); FirearmSoundBankStageResult third=stager.Stage(mod,data,manifest); Assertions.Equal(FirearmSoundBankStageStatus.Copied,third.Status,"Mismatched exact destination must be replaced.");
                Assertions.Equal(2,File.ReadAllBytes(sibling).Length,"Sibling Init bank was modified.");
                File.Delete(source); Assertions.Throws<FileNotFoundException>(()=>stager.Stage(mod,data,manifest),"Missing source must reject.");
            }
            finally { if(Directory.Exists(root)) Directory.Delete(root,true); }
        }
        internal static void BankBinaryValidation()
        {
            FirearmSoundBankBinaryValidator.Validate(Bank(false));
            Assertions.Throws<InvalidDataException>(()=>FirearmSoundBankBinaryValidator.Validate(Bank(true)),"Zero-length embedded media was accepted.");
            byte[] truncated=Bank(false); Array.Resize(ref truncated,truncated.Length-1);
            Assertions.Throws<InvalidDataException>(()=>FirearmSoundBankBinaryValidator.Validate(truncated),"Truncated SoundBank was accepted.");
            Assertions.Throws<ArgumentNullException>(()=>FirearmSoundBankBinaryValidator.Validate((byte[])null),"Null SoundBank was accepted.");
        }
        internal static void StateMachineLifecycle()
        {
            var engine=new FakeEngine(); var machine=new FirearmSoundStateMachine(engine); machine.MarkStaged();
            Assertions.False(machine.EnsureReady(),"Uninitialized Wwise must wait."); Assertions.Equal(0,machine.LoadAttempts,"Waiting must not load.");
            engine.Initialized=true; Assertions.True(machine.EnsureReady(),"Initialized Wwise must load."); Assertions.True(machine.EnsureReady(),"Ready state must be idempotent."); Assertions.Equal(1,engine.Loads,"Bank loaded more than once.");
            Assertions.Equal((uint)41,machine.TryPost("event",new object()),"Valid playing ID rejected."); Assertions.Equal(1,machine.AcceptedPosts,"Accepted count mismatch.");
            engine.PlayingId=0; Assertions.Equal((uint)0,machine.TryPost("event",new object()),"Invalid playing ID accepted."); Assertions.Equal(1,machine.AcceptedPosts,"Rejected post incremented acceptance.");
        }
        private static FirearmSoundBankManifest Manifest(string hash) { return new FirearmSoundBankManifest { SchemaVersion=1,BankName=FirearmSoundEventCatalog.BankName,BankFileName=FirearmSoundEventCatalog.BankFileName,Platform="Windows",WwiseVersion="2016.2.6.6153",Sha256=hash,MediaEmbedded=true,Events=new Dictionary<string,string>{{"Pistol",FirearmSoundEventCatalog.Resolve(FirearmKind.Pistol)},{"Musket",FirearmSoundEventCatalog.Resolve(FirearmKind.Musket)},{"Blunderbuss",FirearmSoundEventCatalog.Resolve(FirearmKind.Blunderbuss)},{"Revolver",FirearmSoundEventCatalog.Resolve(FirearmKind.Revolver)},{"Rifle",FirearmSoundEventCatalog.Resolve(FirearmKind.Rifle)}}}; }
        private static void Invalid(FirearmSoundBankManifest original,Action<FirearmSoundBankManifest> mutate){FirearmSoundBankManifest copy=Manifest(original.Sha256);mutate(copy);Assertions.Throws<InvalidDataException>(()=>FirearmSoundBankManifestValidator.Validate(copy),"Invalid manifest accepted.");}
        private static string HashBytes(byte[] bytes){string path=Path.GetTempFileName();try{File.WriteAllBytes(path,bytes);return FirearmSoundBankStager.Hash(path);}finally{File.Delete(path);}}
        private static byte[] Bank(bool zeroMedia)
        {
            using(var stream=new MemoryStream()) using(var writer=new BinaryWriter(stream,Encoding.ASCII,true))
            {
                Chunk(writer,"BKHD",new byte[28]);
                using(var index=new MemoryStream()) using(var entries=new BinaryWriter(index,Encoding.ASCII,true))
                {
                    for(uint i=0;i<5;i++){entries.Write(i+1);entries.Write(i*10);entries.Write(zeroMedia&&i==2?0u:10u);}
                    Chunk(writer,"DIDX",index.ToArray());
                }
                Chunk(writer,"DATA",new byte[50]); Chunk(writer,"HIRC",new byte[]{1,0,0,0}); return stream.ToArray();
            }
        }
        private static void Chunk(BinaryWriter writer,string id,byte[] payload){writer.Write(Encoding.ASCII.GetBytes(id));writer.Write((uint)payload.Length);writer.Write(payload);}
        private sealed class FakeEngine:IFirearmSoundEngine { internal bool Initialized; internal int Loads; internal uint PlayingId=41; public bool IsInitialized(){return Initialized;} public void LoadBank(string name){Loads++;} public uint PostEvent(string name,object emitter){return PlayingId;} }
    }
}
