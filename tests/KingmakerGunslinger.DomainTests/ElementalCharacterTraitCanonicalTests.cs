using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.ElementalRaces;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ElementalCharacterTraitCanonicalTests
    {
        private sealed class Asset
        {
            internal CharacterTraitNode Node;
            internal bool Valid=true;
        }
        private static Dictionary<string,Asset> Graph()
        { return ElementalCharacterTraitCatalog.Nodes().ToDictionary(n=>n.Guid,n=>new Asset{Node=n}); }
        private static Asset[] Resolve(Dictionary<string,Asset> graph)
        { return CharacterTraitCanonicalAdmission.Resolve<Asset>(n=>graph.ContainsKey(n.Guid)?graph[n.Guid]:null,
            (n,a)=>a.Valid && ReferenceEquals(n,a.Node)); }
        internal static void FirstInitialization()
        { Assertions.True(Resolve(new Dictionary<string,Asset>())==null,"No identity means detached construction."); }
        internal static void CompleteCanonicalGraph()
        { var g=Graph();var r=Resolve(g);Assertions.Equal(11,r.Length,"All eleven admitted.");Assertions.True(r.All(a=>ReferenceEquals(g[a.Node.Guid],a)),"Exact canonical references."); }
        internal static void RepeatedInitialization()
        { var g=Graph();Assertions.True(Resolve(g).SequenceEqual(Resolve(g)),"Canonical references reused, no reconstruction."); }
        internal static void NewProcessCanonicalGraph()
        { var first=Graph();var fresh=Graph();Assertions.Equal(11,Resolve(fresh).Length,"Fresh-process canonical objects valid.");Assertions.False(ReferenceEquals(Resolve(first)[0],Resolve(fresh)[0]),"No static graph ownership across processes."); }
        internal static void PartialOneRejected()
        { var g=Graph();foreach(var key in g.Keys.Skip(1).ToArray())g.Remove(key);Assertions.Throws<InvalidOperationException>(()=>Resolve(g),"One identity cannot publish."); }
        internal static void PartialTenRejected()
        { var g=Graph();g.Remove(g.Keys.First());Assertions.Throws<InvalidOperationException>(()=>Resolve(g),"Ten identities cannot publish."); }
        internal static void ForeignSameGuidRejected()
        { var g=Graph();g[g.Keys.First()]=new Asset{Node=ElementalCharacterTraitCatalog.Nodes()[1]};Assertions.Throws<InvalidOperationException>(()=>Resolve(g),"Matching dictionary GUID is not sufficient."); }
        internal static void WrongTypeRejected()
        { var g=Graph();g.Values.First().Valid=false;Assertions.Throws<InvalidOperationException>(()=>Resolve(g),"Native wrong type/graph rejected by exact validator."); }
        internal static void MissingGraphNeverRunsValidator()
        { int checks=0;Assertions.True(CharacterTraitCanonicalAdmission.Resolve<Asset>(n=>null,(n,a)=>{checks++;return true;})==null,"Create route.");Assertions.Equal(0,checks,"No imaginary existing object is validated."); }
        internal static void PartialNeverRunsValidator()
        { int checks=0;var node=ElementalCharacterTraitCatalog.Nodes()[0];Assertions.Throws<InvalidOperationException>(()=>CharacterTraitCanonicalAdmission.Resolve<Asset>(n=>n.Guid==node.Guid?new Asset{Node=node}:null,(n,a)=>{checks++;return true;}),"Partial route.");Assertions.Equal(0,checks,"Fail before native side effects."); }
        internal static void ValidatorExceptionFailsClosed()
        { var g=Graph();Assertions.Throws<InvalidOperationException>(()=>CharacterTraitCanonicalAdmission.Resolve<Asset>(n=>g[n.Guid],(n,a)=>throw new InvalidOperationException("contract mismatch")),"No fallback/admission.");Assertions.Equal(11,g.Count,"Read-only preflight preserved identities."); }
        internal static void AcquisitionWithdrawalRetainsIdentity()
        {
            object[] empty=Array.Empty<object>(),selection={new object()};
            var features=ElementalCharacterTraitCatalog.All().Select(d=>(object)d).ToArray();
            int registered=0,removed=0;
            var conditions=new ElementalCharacterTraitPublicationConditions(true,true,true,true,true,ElementalCharacterTraitCatalog.SelectionGuid,
                ElementalCharacterTraitAssetGate.Evaluate(ElementalCharacterTraitAssetCatalog.Current()));
            var original=selection;
            var tx=new ElementalCharacterTraitPublicationTransaction<object>(conditions,()=>empty,()=>selection,a=>selection=a,features,
                v=>v is ElementalCharacterTraitDefinition?((ElementalCharacterTraitDefinition)v).Feature.Guid:"foreign",()=>{},()=>{},
                new CharacterTraitPublicationStep(()=>registered++,()=>removed++));
            tx.Commit();tx.WithdrawAcquisition();tx.WithdrawAcquisition();
            Assertions.Equal(1,registered,"Stable registration retained.");
            Assertions.Equal(0,removed,"Acquisition off cannot delete saved identities/localization.");
            Assertions.True(ReferenceEquals(selection,original),"Foreign selection exactly restored.");
            Assertions.False(tx.IsCommitted,"Acquisition withdrawn.");
        }
    }
}
