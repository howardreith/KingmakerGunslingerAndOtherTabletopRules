using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ElementalCharacterTraitReadinessTests
    {
        private sealed class Feature
        {
            internal readonly string Id;
            internal Feature(string id) { Id=id; }
        }
        private sealed class Fixture
        {
            internal Feature[] Features=Array.Empty<Feature>();
            internal Feature[] All={ new Feature("foreign-racial-trait") };
            internal readonly Feature[] Add=ElementalCharacterTraitCatalog.All().Select(d => new Feature(d.Feature.Guid)).ToArray();
            internal int Writes, Checks, Applied, Removed;
            internal Action OnCheck;
            internal Action<Feature[]> OnWrite;
            internal ElementalCharacterTraitPublicationTransaction<Feature> Prepare(
                ElementalCharacterTraitPublicationConditions conditions=null,
                params CharacterTraitPublicationStep[] steps)
            {
                return new ElementalCharacterTraitPublicationTransaction<Feature>(conditions ?? Ready(),
                    ()=>Features,()=>All,a=> { Writes++; All=a; OnWrite?.Invoke(a); },
                    Add,f=>f.Id,()=>Checks++,()=>OnCheck?.Invoke(),steps);
            }
            internal CharacterTraitPublicationStep OwnedStep(bool fail=false)
            { return new CharacterTraitPublicationStep(()=> { Applied++; if(fail) throw new InvalidOperationException("registration failed"); },()=>Removed++); }
        }
        private static string Read(string path) { return File.ReadAllText(Path.Combine(Environment.CurrentDirectory,path)); }
        private static string Factory { get { return Read("src/KingmakerGunslinger/ElementalRaces/DormantElementalCharacterTraitFactory.cs"); } }
        private static string Grant { get { return Read("src/KingmakerGunslinger/ElementalRaces/ElementalCharacterTraitOwnedGrant.cs"); } }
        private static ElementalCharacterTraitDefinition Def(ElementalCharacterTraitId id) { return ElementalCharacterTraitCatalog.Get(id); }
        private static OriginalIconEvidence Receipt(int i, string fault=null, string sourceHash=null)
        {
            var d=ElementalCharacterTraitCatalog.All()[i];
            return new OriginalIconEvidence(d.IconKey,
                fault=="path" ? "assets/native/donor.png" : d.OriginalPath,d.ExportPath,d.RuntimePath,
                sourceHash ?? new string((char)('a'+i),64),new string((char)('1'+i),64),
                fault=="approval" ? new string('f',64) : new string((char)('1'+i),64),
                fault=="source-size" ? 64 : 1024,1024,fault=="export-size" ? 64 : 128,128,
                fault=="family" ? "native-monogram" : ElementalCharacterTraitCatalog.Family,
                fault=="profile" ? "unqualified" : ElementalCharacterTraitCatalog.OriginalProfile,
                fault=="group" ? "foreign" : d.ReviewGroup,
                fault!="donor",fault!="provenance",fault!="png",fault!="rgba",fault!="catalog",fault!="protected");
        }
        private static ElementalCharacterTraitAssetDecision Assets(string fault=null)
        {
            // Hypothetical immutable receipts exercise policy; no test creates,
            // copies or loads an image, and the native factory cannot accept these.
            return ElementalCharacterTraitAssetGate.Evaluate(Enumerable.Range(0,4).Select(i=>Receipt(i,i==0?fault:null)));
        }
        private static ElementalCharacterTraitPublicationConditions Ready(
            bool present=true,bool compatible=true,bool traits=true,bool module=true,bool graph=true,
            string target=ElementalCharacterTraitCatalog.SelectionGuid,ElementalCharacterTraitAssetDecision assets=null)
        { return new ElementalCharacterTraitPublicationConditions(present,compatible,traits,module,graph,target,assets ?? Assets()); }
        private static void Reject(ElementalCharacterTraitPublicationConditions c)
        { var f=new Fixture(); Assertions.Throws<InvalidOperationException>(()=>f.Prepare(c,f.OwnedStep()).Commit(),"Gate before mutation."); Assertions.Equal(0,f.Writes+f.Applied,"No construction or publication."); }
        private static void BadAsset(string fault)
        { var a=Assets(fault); Assertions.False(a.Ready,"Reject unqualified original receipt: "+fault); Reject(Ready(assets:a)); }
        private static void Has(string text,params string[] tokens)
        { foreach(var t in tokens) Assertions.True(text.Contains(t),"Required exact graph/lifecycle contract: "+t); }

        internal static void FourUniqueFeatures()
        { ElementalCharacterTraitCatalog.Validate(); Assertions.Equal(4,ElementalCharacterTraitCatalog.All().Select(d=>d.Feature.Guid).Distinct().Count(),"Four stable feature identities.");
            var plan=JObject.Parse(Read("docs/design/ELEMENTAL-CHARACTER-RACE-TRAIT-PUBLICATION-PLAN-2026-10-06.json"));
            Assertions.False((bool)plan["TraitsPublished"],"Plan is not publication.");
            var definitions=ElementalCharacterTraitCatalog.All();
            for(int i=0;i<definitions.Length;i++)
            {
                var d=definitions[i]; var row=plan["traits"][i];
                Assertions.Equal(d.RaceGuid,(string)row["raceGuid"],"Catalog/dormant plan exact race.");
                Assertions.Equal(d.IconKey,(string)row["iconKey"],"Catalog/dormant plan exact icon.");
                var nodes=row["nodes"].ToArray();
                Assertions.True(d.Nodes.Select(n=>n.Guid).SequenceEqual(nodes.Select(n=>(string)n["guid"])),"Exact frozen save identities, not a parallel manifest.");
                foreach(var n in nodes) Assertions.False((bool)n["registered"] || (bool)n["bootstrapReachable"] || (bool)n["ordinaryAcquisition"],"Dormant plan cannot advertise acquisition.");
            } }
        internal static void HiddenIdentitiesUnique()
        { var n=ElementalCharacterTraitCatalog.Nodes(); Assertions.Equal(6,n.Count(v=>!v.Visible),"Six hidden graph identities."); Assertions.Equal(n.Length,n.Select(v=>v.Guid).Distinct().Count(),"All nodes unique."); }
        internal static void StableRebuildIdentities()
        { Assertions.True(ElementalCharacterTraitCatalog.Nodes().Select(n=>n.Guid).SequenceEqual(ElementalCharacterTraitCatalog.Nodes().Select(n=>n.Guid)),"No runtime regeneration."); Assertions.False(Factory.Contains("Guid.NewGuid"),"Production graph cannot borrow a transient identity."); }
        internal static void ManifestCollisionPreflight()
        { var manifest=JObject.Parse(Read("blueprints/blueprints.json")); foreach(var n in ElementalCharacterTraitCatalog.Nodes()) { var matches=manifest["entries"].Where(e=>(string)e["symbol"]==n.Symbol).ToArray(); Assertions.Equal(1,matches.Length,"One stable manifest identity."); Assertions.Equal(n.Guid,(string)matches[0]["guid"],"Frozen GUID."); Assertions.Equal(n.BlueprintType,(string)matches[0]["plannedType"],"Exact blueprint type."); } Has(Factory,"library.BlueprintsByAssetId.ContainsKey(node.Guid)","before construction"); }
        internal static void RaceMapping()
        { var a=ElementalCharacterTraitCatalog.All(); Assertions.True(a.Select(d=>d.Race).SequenceEqual(new[]{"Ifrit","Oread","Sylph","Undine"}),"Exact assigned races."); foreach(var d in a) Assertions.True(Read("blueprints/blueprints.json").Contains(d.RaceGuid),"Race already canonical, no new race."); }
        internal static void ExactRaceObject()
        { var race=new object(); Assertions.True(ElementalCharacterTraitCatalog.CanSelect(race,race,false),"Exact race."); Assertions.False(ElementalCharacterTraitCatalog.CanSelect(race,new object(),false),"Lookalike race rejected."); Assertions.False(ElementalCharacterTraitCatalog.CanSelect(null,null,false),"Absent race rejected."); }
        internal static void DuplicateFeatureRejected()
        { var r=new object(); Assertions.False(ElementalCharacterTraitCatalog.CanSelect(r,r,true),"Self duplicate blocked."); Has(Factory,"PrerequisiteNoFeature","noFeature.Feature=feature","Prerequisite.GroupType.All"); }
        internal static void ExactHostRacePrerequisite()
        { Has(Factory,"ZFavoredClass.NewMechanics.PrerequisiteRace","raceField?.FieldType != typeof(BlueprintRace)","raceField.SetValue(race,canonicalRaces[i])","ReferenceEquals(raceField.GetValue(race),canonicalRaces[i])"); }
        internal static void ExactRacialTarget()
        { Assertions.Equal("racial_traits",ElementalCharacterTraitCatalog.SelectionField,"Character traits, not alternate replacements."); Reject(Ready(target:"adopted-traits")); Has(Read("src/KingmakerGunslinger/ElementalRaces/ElementalCharacterTraitPublicationTransaction.cs"),"liveFeatures.Length != 0","All four canonical trait features"); }
        internal static void AbsentFavoredClass() { Reject(Ready(present:false)); }
        internal static void IncompatibleFavoredClass() { Reject(Ready(compatible:false)); }
        internal static void TraitsDisabled() { Reject(Ready(traits:false)); }
        internal static void ModuleDisabled() { Reject(Ready(module:false)); }
        internal static void InvalidGraph() { Reject(Ready(graph:false)); }
        internal static void CurrentAssetGateBlocks()
        { var a=ElementalCharacterTraitAssetGate.Evaluate(ElementalCharacterTraitAssetCatalog.Current()); Assertions.True(a.Ready,"Four exact objectively admitted originals."); foreach(var d in ElementalCharacterTraitCatalog.All()) Assertions.Equal(OriginalIconDisposition.Qualified,a.For(d.Id),"All originals ready; owner aesthetic approval remains separate."); }
        internal static void MissingOneBlocksAll()
        { var a=ElementalCharacterTraitAssetGate.Evaluate(Enumerable.Range(0,3).Select(i=>Receipt(i))); Assertions.Equal(OriginalIconDisposition.Missing,a.For(ElementalCharacterTraitId.Whiteout),"One absent prevents all-four construction."); Reject(Ready(assets:a)); }
        internal static void QualifiedReceiptPolicy()
        { Assertions.True(Assets().Ready,"All-four hypothetical reviewed original receipts admit policy, not actual registration."); }
        internal static void DuplicateAssetKeyRejected()
        { var a=ElementalCharacterTraitAssetGate.Evaluate(new[]{Receipt(0),Receipt(0),Receipt(2),Receipt(3)}); Assertions.False(a.Ready,"No ambiguous concept records."); }
        internal static void CopiedArtRejected()
        { var a=ElementalCharacterTraitAssetGate.Evaluate(Enumerable.Range(0,4).Select(i=>Receipt(i,sourceHash:new string('a',64)))); Assertions.False(a.Ready,"Renaming an existing painting does not supply four originals."); }
        internal static void DonorRejected() { BadAsset("donor"); }
        internal static void MissingProvenanceRejected() { BadAsset("provenance"); }
        internal static void WrongPathRejected() { BadAsset("path"); }
        internal static void WrongDimensionsRejected() { BadAsset("export-size"); BadAsset("source-size"); }
        internal static void WrongFormatRejected() { BadAsset("png"); BadAsset("rgba"); }
        internal static void WrongApprovalHashRejected() { BadAsset("approval"); }
        internal static void WrongFamilyRejected() { BadAsset("family"); }
        internal static void WrongProfileRejected() { BadAsset("profile"); }
        internal static void WrongReviewGroupRejected() { BadAsset("group"); }
        internal static void CatalogFailureRejected() { BadAsset("catalog"); }
        internal static void ProtectedAssignmentFailureRejected() { BadAsset("protected"); }
        internal static void AssetGateBeforeNativeAllocation()
        { int gate=Factory.IndexOf("Evaluate(ElementalCharacterTraitAssetCatalog.Current()).RequireReady()",StringComparison.Ordinal); Assertions.True(gate>=0 && gate<Factory.IndexOf("library?.BlueprintsByAssetId",StringComparison.Ordinal) && gate<Factory.IndexOf("ScriptableObject.CreateInstance",StringComparison.Ordinal),"Real asset gate precedes native construction and copy."); }
        internal static void NoNativeGateOverride()
        { string signature=Factory.Split(new[]{"CreateDetached("},StringSplitOptions.None)[1].Split('{')[0]; Assertions.False(signature.Contains("OriginalIconEvidence") || signature.Contains("AssetDecision"),"Tests/callers cannot supply hypothetical art to native factory."); }
        internal static void TransactionPublishesFour()
        { var f=new Fixture(); var foreign=f.All[0]; var t=f.Prepare(null,f.OwnedStep()); t.Commit(); Assertions.Equal(5,f.All.Length,"All four appended."); Assertions.True(ReferenceEquals(foreign,f.All[0]),"Foreign preserved."); Assertions.True(t.IsCommitted,"Committed only after full graph and selection."); }
        internal static void TransactionRepeatedCommit()
        { var f=new Fixture(); var t=f.Prepare(null,f.OwnedStep()); t.Commit(); int writes=f.Writes; t.Commit(); Assertions.Equal(writes,f.Writes,"No duplicate selection writes."); Assertions.Equal(1,f.Applied,"No repeated registration."); }
        internal static void RepeatedPublicationIdempotent()
        { var f=new Fixture(); f.Prepare().Commit(); int writes=f.Writes; f.Prepare().Commit(); Assertions.Equal(writes,f.Writes,"Canonical existing entries remain once."); Assertions.Equal(5,f.All.Length,"Four published entries."); }
        internal static void ForeignOrderPreserved()
        { var f=new Fixture(); f.All=new[]{new Feature("foreign-b"),new Feature("foreign-a")}; var before=f.All; f.Prepare().Commit(); Assertions.True(ReferenceEquals(before[0],f.All[0]) && ReferenceEquals(before[1],f.All[1]),"No foreign sort or replacement."); }
        internal static void RegistryFailureRollsBack()
        { var f=new Fixture(); var before=f.All; var t=f.Prepare(null,f.OwnedStep(),f.OwnedStep(true)); Assertions.Throws<InvalidOperationException>(()=>t.Commit(),"Partial registration failure."); Assertions.Equal(2,f.Removed,"Both attempted steps, reverse cleanup."); Assertions.True(ReferenceEquals(before,f.All),"Selector never mutated."); }
        internal static void SelectorFailureRollsBack()
        { var f=new Fixture(); var before=f.All; bool once=true; f.OnWrite=a=>{if(a.Length==3 && once){once=false;throw new InvalidOperationException("selection rejected");}}; var t=f.Prepare(null,f.OwnedStep()); Assertions.Throws<InvalidOperationException>(()=>t.Commit(),"Rejected selector write."); Assertions.True(ReferenceEquals(before,f.All),"Exact original array restored."); Assertions.Equal(1,f.Removed,"Owned registration undone."); }
        internal static void RollbackRestoresExactArray()
        { var f=new Fixture(); var before=f.All; var t=f.Prepare(null,f.OwnedStep()); t.Commit(); t.Rollback(); Assertions.True(ReferenceEquals(before,f.All),"Native foreign original reference restored."); Assertions.Equal(1,f.Removed,"Owned registration removed once."); t.Rollback(); Assertions.Equal(1,f.Removed,"Idempotent cleanup."); }
        internal static void RollbackPreservesForeignAddition()
        { var f=new Fixture(); var t=f.Prepare(); t.Commit(); var extra=new Feature("later-foreign"); f.All=f.All.Concat(new[]{extra}).ToArray(); t.Rollback(); Assertions.Equal(2,f.All.Length,"Only own four removed."); Assertions.True(ReferenceEquals(extra,f.All[1]),"Later foreign mutation preserved."); }
        internal static void ConflictBeforeRegistration()
        { var f=new Fixture(); f.All=f.All.Concat(new[]{new Feature(f.Add[0].Id)}).ToArray(); Assertions.Throws<InvalidOperationException>(()=>f.Prepare(null,f.OwnedStep()),"Same GUID foreign object."); Assertions.Equal(0,f.Applied+f.Writes,"No partial registration."); }
        internal static void LateConflictBeforeRegistration()
        { var f=new Fixture(); var t=f.Prepare(null,f.OwnedStep()); f.All=f.All.Concat(new[]{new Feature(f.Add[2].Id)}).ToArray(); Assertions.Throws<InvalidOperationException>(()=>t.Commit(),"Race after prepare revalidated before owned writes."); Assertions.Equal(0,f.Applied+f.Writes,"Zero registration on late conflict."); }
        internal static void HostChangeBeforeRegistration()
        { var f=new Fixture(); var t=f.Prepare(null,f.OwnedStep()); f.Features=new[]{new Feature("changed-contract")}; Assertions.Throws<InvalidOperationException>(()=>t.Commit(),"Exact Features-empty contract changed."); Assertions.Equal(0,f.Applied+f.Writes,"Fail before registration."); }
        internal static void InvalidIdentityBatchRejected()
        { var f=new Fixture(); f.All=new[]{new Feature("duplicate"),new Feature("duplicate")}; Assertions.Throws<InvalidOperationException>(()=>f.Prepare(),"Ambiguous foreign IDs cannot be normalized without authorization."); }
        internal static void PartialBatchRejected()
        { var f=new Fixture(); Assertions.Throws<InvalidOperationException>(()=>new ElementalCharacterTraitPublicationTransaction<Feature>(Ready(),()=>f.Features,()=>f.All,a=>f.All=a,f.Add.Take(3).ToArray(),v=>v.Id,()=>{},()=>{}),"No partial-four publication."); }
        internal static void RevalidationFailureNoMutation()
        { var f=new Fixture(); f.OnCheck=()=>throw new InvalidOperationException("host disabled"); var t=f.Prepare(null,f.OwnedStep()); Assertions.Throws<InvalidOperationException>(()=>t.Commit(),"External compatibility recheck."); Assertions.Equal(0,f.Applied+f.Writes,"No mutation."); }
        internal static void RegistrationRollbackReverseOrder()
        { var f=new Fixture(); var order=new List<int>(); var t=f.Prepare(null,new CharacterTraitPublicationStep(()=>{},()=>order.Add(1)),new CharacterTraitPublicationStep(()=>throw new InvalidOperationException(),()=>order.Add(2))); Assertions.Throws<InvalidOperationException>(()=>t.Commit(),"Fail step2."); Assertions.True(order.SequenceEqual(new[]{2,1}),"Reverse exact graph/copy teardown."); }
        internal static void ClosedTransactionCannotRecommit()
        { var f=new Fixture(); var t=f.Prepare(); t.Commit(); t.Rollback(); Assertions.Throws<InvalidOperationException>(()=>t.Commit(),"Closed plan cannot resuscitate removed registrations."); }
        internal static void FieryFeatureGrant()
        { var d=Def(ElementalCharacterTraitId.FieryGlare); Assertions.Equal(CharacterTraitNodeKind.Toggle,d.GrantedNode.Kind,"Feature grants one toggle."); Has(Factory,"grant.GrantedFact=(BlueprintUnitFact)graph.Resolve(d.GrantedNode.Symbol)","UnpublishedRaceTraitFoundationFactory.CreateFieryGlare()"); }
        internal static void FieryExactNativeCarrier()
        { Has(Read("src/KingmakerGunslinger/ElementalRaces/UnpublishedRaceTraitFoundationFactory.cs"),"Take10ForSuccess","StatType.CheckIntimidate","toggle.Buff","UnitCommand.CommandType.Free"); Assertions.Equal(CharacterTraitNodeKind.ActivationBuff,Def(ElementalCharacterTraitId.FieryGlare).Node(CharacterTraitNodeKind.ActivationBuff).Kind,"Stable hidden activation carrier."); }
        internal static void FieryCopyHonest()
        { Has(Def(ElementalCharacterTraitId.FieryGlare).Description,"whenever that would succeed","otherwise, they are rolled normally","during combat"); }
        internal static void StoicCompleteGraph()
        { var d=Def(ElementalCharacterTraitId.StoicDignity); Assertions.Equal(4,d.Nodes.Length,"Feature/provider/area/recipient."); Has(Factory,"stoic.Recipient","stoic.Area","stoic.Provider"); }
        internal static void StoicBonusAndRadiusPreserved()
        { Has(Read("src/KingmakerGunslinger/ElementalRaces/UnpublishedRaceTraitFoundationFactory.cs"),"10.Feet()","StoicDignitySaveBonus","bonus.AllyRecipient = true","$StoicDignitySelfSave","ContextConditionIsAlly"); Has(Read("src/KingmakerGunslinger/ElementalRaces/StoicDignityMechanics.cs"),"ModifierDescriptor.Trait","ModifierDescriptor.Morale"); }
        internal static void StoicCopyHonest()
        { var d=Def(ElementalCharacterTraitId.StoicDignity); Has(d.Description,"While conscious","Other allies","10 feet","same effect","unrelated effect"); Has(d.Supplemental,"does not remove or suppress"); }
        internal static void StoicHolderContext()
        { Has(Grant,"new MechanicsContext(Owner.Unit,Owner,Fact.Blueprint,null,new TargetWrapper(Owner.Unit))","Owner.Buffs.AddBuff(buff,source,null)"); Assertions.False(Grant.Contains("ScriptableObject.CreateInstance<AddFacts>"),"Native null-context AddFacts cannot deliver attached-area holder context."); }
        internal static void AerialExactWings()
        { Assertions.Equal("e116e1e0a17a4aceb001000000000019",ElementalCharacterTraitCatalog.WingsOfAirGuid,"Canonical qualified Wings buff."); Has(Factory,"AerialObserverFlightContract.VerifyCarrier(canonicalWingsOfAir)","UnpublishedAerialObserverFoundationFactory.Create(canonicalWingsOfAir)"); }
        internal static void AerialPerceptionOnly()
        { Has(Read("src/KingmakerGunslinger/ElementalRaces/AerialObserverMechanics.cs"),"SkillPerception","ModifierDescriptor.Trait","FlightCarrier"); }
        internal static void AerialCopyNamesCarrier()
        { string d=Def(ElementalCharacterTraitId.AerialObserver).Description; Has(d,"While Wings of Air is active","+2 trait bonus on Perception"); Assertions.False(d.Contains("while flying"),"No broader flight promise."); }
        internal static void WhiteoutStableProvider()
        { Has(Factory,"CreateStableWhiteoutProvider(graph","graph.Add(node,provider)","component.ProviderIdentity=provider","CreateInstance<WhiteoutWeatherProvider>"); Assertions.False(Factory.Contains("UnpublishedWhiteoutFoundationFactory.Create()"),"Request-random factory remains test-only."); }
        internal static void WhiteoutCopyHonest()
        { var d=Def(ElementalCharacterTraitId.Whiteout); Has(d.Description,"outdoors","rain or snow","light intensity","independent 10%","after other concealment","ignore concealment","Magical fog","waterfall"); Has(d.Supplemental,"28%","without an attack roll"); }
        internal static void WhiteoutQualifiedPolicyRetained()
        { Assertions.False(WhiteoutPolicy.WeatherActive(true,WhiteoutPrecipitation.Rain,1,true),"Indoor blocked."); int count=0; for(int a=1;a<=100;a++) for(int b=1;b<=100;b++) if(a<=20 || WhiteoutPolicy.IsMiss(b)) count++; Assertions.Equal(2800,count,"Sequential independent policy unchanged."); }
        internal static void HiddenNodesNotSelectable()
        { Has(Factory,"graph.Features.Length != 4","graph.Features","feature.HideInUI=false"); var d=ElementalCharacterTraitCatalog.All(); Assertions.Equal(4,d.Select(v=>v.Feature).Count(),"Only feature nodes are selection additions."); }
        internal static void NoBootstrapCaller()
        { string c=Read("src/KingmakerGunslinger/ElementalRaces/ElementalCharacterTraitPublicationCoordinator.cs"); Has(c,"ResolveCanonicalGraphIfFullyRegistered","CreateDetached(","WithdrawAcquisition()"); Has(Read("src/KingmakerGunslinger/Main.cs"),"ElementalCharacterTraitPublicationCoordinator.Attach(context)"); Assertions.False(Read("src/KingmakerGunslinger/Bootstrap/BlueprintBootstrap.cs").Contains("CreateDetached"),"Native detached construction has one guarded late coordinator."); }
        internal static void NoRegistrationOrLocalization()
        { foreach(string t in new[]{"library.AddAsset(","BlueprintRegistry.Register(","LocalizationService.","LocalizationManager.","RegisterFeature(","Publish("}) Assertions.False(Factory.Contains(t),"Dormant construction has no external registration: "+t); Has(Factory,"Copy { get","new LocalizedString()"); }
        internal static void NoLiveSelectorOrGrant()
        { var c=JObject.Parse(Read("assets-source/original-icons/icon-catalog.json")); foreach(var d in ElementalCharacterTraitCatalog.All()) { var consumer=c["consumers"].Where(e=>(string)e["symbol"]==d.Feature.Symbol).ToArray(); Assertions.Equal(1,consumer.Length,"Each visible feature has one exact icon consumer."); Assertions.Equal(d.IconKey,(string)consumer[0]["concept"],"Correct concept."); } foreach(var n in ElementalCharacterTraitCatalog.Nodes().Where(n=>!n.Visible)) Assertions.False(c["consumers"].Any(e=>(string)e["symbol"]==n.Symbol && e["concept"]!=null && e["concept"].Type!=JTokenType.Null && (string)e["concept"]!="internal"),"No hidden icon consumer."); }
        internal static void NoPlaceholderPackaged()
        { foreach(var d in ElementalCharacterTraitCatalog.All()) { var e=ElementalCharacterTraitAssetCatalog.Current().Single(v=>v.Key==d.IconKey); byte[] bytes=File.ReadAllBytes(Path.Combine(Environment.CurrentDirectory,d.RuntimePath)); using(var sha=System.Security.Cryptography.SHA256.Create()) Assertions.Equal(e.ExportHash,BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-","").ToLowerInvariant(),"Only exact original export is packaged."); } }
        internal static void CatalogSnapshotIndependent()
        { var all=ElementalCharacterTraitCatalog.All(); all[0]=all[1]; Assertions.Equal(ElementalCharacterTraitId.FieryGlare,ElementalCharacterTraitCatalog.All()[0].Id,"Caller cannot alter frozen trait order."); var nodes=Def(ElementalCharacterTraitId.FieryGlare).Nodes; nodes[0]=nodes[1]; Assertions.Equal(CharacterTraitNodeKind.Feature,Def(ElementalCharacterTraitId.FieryGlare).Nodes[0].Kind,"Node snapshot immutable."); }
        internal static void PersistentStableLookup()
        { foreach(var n in ElementalCharacterTraitCatalog.Nodes()) Assertions.True(ElementalCharacterTraitPersistenceContract.CanRestoreIdentity(n.Guid),"Planned saved identity resolves consistently."); Assertions.False(ElementalCharacterTraitPersistenceContract.CanRestoreIdentity(Guid.NewGuid().ToString("N")),"Transient identity not production restore API."); }
        internal static void AcquisitionOffSavedIdentityStable()
        { Assertions.False(ElementalCharacterTraitPersistenceContract.MayOffer(true,false,true,true),"Traits off removes acquisition."); Assertions.False(ElementalCharacterTraitPersistenceContract.MayOffer(true,true,false,true),"Module off removes acquisition."); Assertions.True(ElementalCharacterTraitPersistenceContract.CanRestoreIdentity(Def(ElementalCharacterTraitId.FieryGlare).Feature.Guid),"Saved identity never deleted by settings."); }
        internal static void ProviderInactiveWhenFeatureRemoved()
        { Assertions.False(ElementalCharacterTraitPersistenceContract.MayRebuildOwnedProvider(false,true,true),"Respec removal."); Assertions.False(ElementalCharacterTraitPersistenceContract.MayRebuildOwnedProvider(true,false,true),"Suppressed feature."); Assertions.False(ElementalCharacterTraitPersistenceContract.MayRebuildOwnedProvider(true,true,false),"Module disabled."); Assertions.True(ElementalCharacterTraitPersistenceContract.MayRebuildOwnedProvider(true,true,true),"Later restoration can rebuild owned graph."); }
        internal static void ExactOwnedFactReuse()
        { var b=new object(); var f=new object(); Assertions.True(ElementalCharacterTraitPersistenceContract.MayReuseOwnedFact(f,b,b,false,true),"Exact recorded saved fact reused."); Assertions.False(ElementalCharacterTraitPersistenceContract.MayReuseOwnedFact(f,new object(),b,false,true),"Foreign same-name or blueprint clone not reused."); }
        internal static void DisposedFactNotReused()
        { var b=new object(); Assertions.False(ElementalCharacterTraitPersistenceContract.MayReuseOwnedFact(new object(),b,b,true,true),"Disposed reference cannot survive respec."); Assertions.False(ElementalCharacterTraitPersistenceContract.MayReuseOwnedFact(new object(),b,b,false,false),"Fact removed from collection is stale."); }
        internal static void OwnedCleanupNoForeignSweep()
        { Has(Grant,"[JsonProperty] private Fact _ownedFact","Owner.RemoveFact(owned)","ReferenceEquals(f,_ownedFact)"); Assertions.False(Grant.Contains("RemoveFact(GrantedFact)") || Grant.Contains("RemoveBuff("),"Cleanup targets exact recorded fact, no by-blueprint foreign sweep."); }
        internal static void OffByDefaultContract()
        { Has(Read("src/KingmakerGunslinger/ElementalRaces/UnpublishedRaceTraitFoundationFactory.cs"),"IsOnByDefault = false","DeactivateImmediately = true","OnlyInCombat = false"); Has(Factory,"fiery.Toggle.ActionBarAutoFillIgnored=false"); }
        internal static void NoSaveWriter()
        { foreach(var file in new[]{"DormantElementalCharacterTraitFactory.cs","ElementalCharacterTraitOwnedGrant.cs","ElementalCharacterTraitPublicationTransaction.cs"}) foreach(string token in new[]{"SaveManager","SaveGame(","SaveAs(","WriteAllBytes(","File.Copy("}) Assertions.False(Read("src/KingmakerGunslinger/ElementalRaces/"+file).Contains(token),"No persistence mutation: "+token); }
    }
}
