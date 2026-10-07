using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using Newtonsoft.Json.Linq;
using KingmakerGunslinger.RuntimeTesting;
using KingmakerGunslinger.ElementalRaces;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ElementalCharacterTraitSaveTests
    {
        private const string Tx="20261006T2100001234567Z_0123456789abcdef0123456789abcdef";
        private static string Owned { get { return ElementalCharacterTraitSaveContract.Name(Tx); } }
        private static Dictionary<string,string> Before()
        { return new Dictionary<string,string>{{"Manual_299_KMG_AUTOMATION_WORKING.zks","seed-hash"},{"Manual_4_campaign.zks","foreign-hash"}}; }
        private static Dictionary<string,string> WithOwned()
        { var a=Before();a.Add("Manual_301_"+Owned+".zks","owned-v1");return a; }
        private static string OwnedFile { get { return "Manual_301_"+Owned+".zks"; } }
        private static void Reject(bool value,string reason) { Assertions.True(!value,reason); }
        internal static void JsonLeaseTimestamp()
        {
            var start=DateTime.Parse("2026-10-07T00:09:04.3164398Z",CultureInfo.InvariantCulture,DateTimeStyles.RoundtripKind);
            var lease=JObject.Parse("{\"start\":\""+start.ToString("o")+"\",\"expiry\":\"2026-10-07T02:09:30.5467382Z\"}");
            Assertions.True(lease["start"].Type==JTokenType.Date &&
                ElementalCharacterTraitSaveContract.MatchesProcessStart(start,(DateTime)lease["start"]),
                "Native JSON date retains exact process creation ticks.");
        }
        internal static void ProcessStartExactTicks()
        {
            var start=new DateTime(638954789043164398,DateTimeKind.Utc);
            Assertions.True(!ElementalCharacterTraitSaveContract.MatchesProcessStart(start,start.AddTicks(1)),
                "A different exact start tick is rejected; no time tolerance.");
        }
        internal static void ParsedLeaseExpiry()
        {
            var lease=JObject.Parse("{\"expiry\":\"2026-10-07T02:09:30.5467382Z\"}");
            var expires=(DateTime)lease["expiry"];
            Assertions.True(ElementalCharacterTraitSaveContract.LeaseUnexpired(expires.AddTicks(-1),expires),
                "Typed JSON expiry is compared as UTC time.");
        }
        internal static void ExactExpiryBoundary()
        {
            var expires=new DateTime(638954789043164398,DateTimeKind.Utc);
            Assertions.True(!ElementalCharacterTraitSaveContract.LeaseUnexpired(expires,expires)&&
                !ElementalCharacterTraitSaveContract.LeaseUnexpired(expires.AddTicks(1),expires),
                "Expired or exactly expired lease is rejected.");
        }
        internal static void WorkingLoadOnly()
        { Assertions.True(ElementalCharacterTraitSaveContract.InputName(Tx,"prepare")==ElementalCharacterTraitSaveContract.Seed,"Exact seed load.");
          Reject(ElementalCharacterTraitSaveContract.MayWrite(Tx,"prepare",ElementalCharacterTraitSaveContract.Seed,true,true),"Never write seed."); }
        internal static void BaselineRejected()
        { Reject(ElementalCharacterTraitSaveContract.MayWrite(Tx,"prepare","KMG_AUTOMATION_BASELINE",true,true),"No baseline writer.");
          Reject(ElementalCharacterTraitSaveContract.ValidTransaction("KMG_AUTOMATION_BASELINE"),"No baseline transaction."); }
        internal static void UniqueDescriptor()
        { Assertions.True(Owned!=ElementalCharacterTraitSaveContract.Name("20261006T2100001234567Z_fedcba9876543210fedcba9876543210"),"Random run identity separates saves."); }
        internal static void NativeFileContract()
        { Assertions.True(ElementalCharacterTraitSaveContract.MatchesFile(Owned,OwnedFile),"Exact native manual name.");
          Reject(ElementalCharacterTraitSaveContract.MatchesFile(Owned,"../"+OwnedFile),"Traversal excluded."); }
        internal static void OnlyOwnedDelta()
        { Assertions.True(ElementalCharacterTraitSaveContract.InventoryPreserved(Before(),WithOwned(),OwnedFile,true),"Only owned new file accepted."); }
        internal static void WorkingHashExact()
        { var a=WithOwned();a["Manual_299_KMG_AUTOMATION_WORKING.zks"]="changed";
          Reject(ElementalCharacterTraitSaveContract.InventoryPreserved(Before(),a,OwnedFile,true),"Seed hash immutable."); }
        internal static void ForeignHashExact()
        { var a=WithOwned();a["Manual_4_campaign.zks"]="changed";
          Reject(ElementalCharacterTraitSaveContract.InventoryPreserved(Before(),a,OwnedFile,true),"Foreign hash immutable."); }
        internal static void ForeignRemovalRejected()
        { var a=WithOwned();a.Remove("Manual_4_campaign.zks");
          Reject(ElementalCharacterTraitSaveContract.InventoryPreserved(Before(),a,OwnedFile,true),"No missing preexisting save."); }
        internal static void OwnedOverwrite()
        { var a=WithOwned();a[OwnedFile]="owned-v2";
          Assertions.True(ElementalCharacterTraitSaveContract.InventoryPreserved(Before(),a,OwnedFile,true) &&
            ElementalCharacterTraitSaveContract.MayWrite(Tx,"verify-remove",Owned,true,true),"Exact lease may overwrite its own save."); }
        internal static void OwnedDeletionExact()
        { Assertions.True(ElementalCharacterTraitSaveContract.InventoryPreserved(Before(),Before(),OwnedFile,false) &&
            ElementalCharacterTraitSaveContract.MayDelete(Tx,Owned,true,true),"Owned deletion restores inventory."); }
        internal static void AutosaveRejected()
        { var a=WithOwned();a["Auto_2.zks"]="auto";
          Reject(ElementalCharacterTraitSaveContract.InventoryPreserved(Before(),a,OwnedFile,true),"Autosave forbidden delta."); }
        internal static void QuicksaveRejected()
        { var a=WithOwned();a["Quick_1.zks"]="quick";
          Reject(ElementalCharacterTraitSaveContract.InventoryPreserved(Before(),a,OwnedFile,true),"Quicksave forbidden delta."); }
        internal static void CollisionRejected()
        { var b=WithOwned();Reject(ElementalCharacterTraitSaveContract.InventoryPreserved(b,b,OwnedFile,true),"Owned path must be initially absent."); }
        internal static void WrongRunRejected()
        { Reject(ElementalCharacterTraitSaveContract.MayWrite("20261006T2100001234567Z_fedcba9876543210fedcba9876543210","prepare",Owned,true,true),"Wrong run cannot write."); }
        internal static void WrongLeaseRejected()
        { Reject(ElementalCharacterTraitSaveContract.MayWrite(Tx,"prepare",Owned,false,true),"Exact lease required."); }
        internal static void StaleLeaseRejected()
        { Reject(ElementalCharacterTraitSaveContract.MayDelete(Tx,Owned,true,false),"Closed lease cannot delete."); }
        internal static void NonownedDeleteRejected()
        { Reject(ElementalCharacterTraitSaveContract.MayDelete(Tx,"Manual_4_campaign.zks",true,true),"Foreign cleanup forbidden.");
          Reject(ElementalCharacterTraitSaveContract.MayDelete(Tx,Owned,false,true),"Preexisting cleanup forbidden."); }
        internal static void StageOrder()
        { Assertions.True(ElementalCharacterTraitSaveContract.Next("prepare","verify-remove",11,12) &&
            ElementalCharacterTraitSaveContract.Next("verify-remove","verify-absent",12,13),"Three closed fresh-process stages.");
          Reject(ElementalCharacterTraitSaveContract.Next("prepare","verify-absent",11,13),"Cannot skip saved removal."); }
        internal static void FreshProcess()
        { Reject(ElementalCharacterTraitSaveContract.Next("prepare","verify-remove",11,11),"No reused process."); }
        internal static void QualifiedArtifactRequired()
        { Reject(ElementalCharacterTraitSaveContract.MayWrite(Tx,"prepare",Owned,true,false),"Exact artifact required."); }
        internal static void VerifyAbsentNoWrite()
        { Reject(ElementalCharacterTraitSaveContract.MayWrite(Tx,"verify-absent",Owned,true,true),"Absent stage is read-only."); }
        internal static void StableWitness()
        { var stable=ElementalCharacterTraitCatalog.Nodes().Select(n=>n.Guid).ToArray();
          Assertions.True(ElementalCharacterTraitSaveContract.ExactSaveWitness(stable,stable),"All eleven stable canonical IDs."); }
        internal static void TransientRejected()
        { Reject(ElementalCharacterTraitSaveContract.ExactSaveWitness(new[]{Guid.NewGuid().ToString("N")},ElementalCharacterTraitCatalog.Nodes().Select(n=>n.Guid)),"Request-local identities cannot be saved."); }
        private static string Source(string path)
        {
            var d=new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while(d!=null && !File.Exists(Path.Combine(d.FullName,"AGENTS.md"))) d=d.Parent;
            if(d==null) throw new InvalidOperationException("Repository source required.");
            return File.ReadAllText(Path.Combine(d.FullName,path));
        }
        internal static void VisibleRemovalLifecycle()
        { var s=Source("src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ElementalCharacterTraitSave.cs");
          Assertions.True(s.Contains("foreach(var feature in features) owner.Descriptor.RemoveFact(feature)") &&
            !s.Contains("RemoveFact(rebuilt"),"Remove visible features; real lifecycle removes providers."); }
        internal static void NoRawSaveWorkflow()
        { var s=Source("scripts/Invoke-ElementalCharacterTraitPersistenceQualification.ps1");
          Assertions.True(!s.Contains("ZipFile")&&!s.Contains("ReadHeader")&&!s.Contains("Copy-Item") &&
            s.Contains("finally")&&s.Contains("Remove-ElementalTraitOwnedSave")&&s.Contains("Assert-KmgProtectedSaveCatalog"),"No raw parsing/copy/replace; exact cleanup."); }
        internal static void NoSummoningDependency()
        { var s=Source("src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ElementalCharacterTraitSave.cs");
          Assertions.True(!s.Contains("RuntimeTestRunner.ExpandedSummoning")&&!s.Contains("KingmakerGunslinger.Summoning")&&
            !s.Contains("ExpandedSummoningPersistence"),"No Summoning persistence dependency."); }
        internal static void SeedCannotBeOwned()
        { Reject(ElementalCharacterTraitSaveContract.MayDelete(Tx,ElementalCharacterTraitSaveContract.Seed,true,true),"Seed never owned.");
          Reject(ElementalCharacterTraitSaveContract.MatchesFile(Owned,"Manual_299_KMG_AUTOMATION_WORKING.zks"),"Native path must match lease descriptor."); }
        internal static void UnknownPhaseRejected()
        { Reject(ElementalCharacterTraitSaveContract.ValidPhase("cleanup-summon"),"Closed phases.");
          Reject(ElementalCharacterTraitSaveContract.MayWrite(Tx,"other",Owned,true,true),"Unknown phase cannot write."); }
    }
}
