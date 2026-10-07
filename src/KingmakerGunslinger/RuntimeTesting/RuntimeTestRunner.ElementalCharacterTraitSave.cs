using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Utility;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private ElementalCharacterTraitSavePlan _traitSavePlan;
        private IEnumerator<int> _traitSaveSteps;
        private readonly List<RuntimeTestAssertion> _traitSaveChecks=new List<RuntimeTestAssertion>();
        private JObject _traitSaveWitness, _traitSavedInfo;
        private GuardedDisposableSaveLease _traitWriteLease;
        private readonly List<object> _traitSaveEvents=new List<object>();
        private string TraitSaveEvidence { get { return Path.Combine(_request.EvidenceDirectory,"elemental-trait-save.json"); } }
        private void TraitSaveAssert(string name,bool pass,object observed)
        {
            _traitSaveEvents.Add(new {name,observed});
            _traitSaveChecks.Add(Assertion("trait-save-"+name,"exact canonical leased persistence contract",
                Newtonsoft.Json.JsonConvert.SerializeObject(observed,new Newtonsoft.Json.JsonSerializerSettings {
                    ContractResolver=new Newtonsoft.Json.Serialization.DefaultContractResolver(),
                    PreserveReferencesHandling=Newtonsoft.Json.PreserveReferencesHandling.None,
                    TypeNameHandling=Newtonsoft.Json.TypeNameHandling.None }),pass,TraitSaveEvidence));
            if(!pass) throw new InvalidOperationException("Trait persistence invariant failed: "+name);
        }
        private void PollElementalTraitSave()
        {
            if(!_workingSaveSmoke.Complete || _workingSaveSmoke.WriteObserved || !_request.ExitAfterCompletion)
                throw new InvalidOperationException("Exact no-write seed/owned load and automatic exit required.");
            if(_traitSaveSteps==null) _traitSaveSteps=RunElementalTraitSave().GetEnumerator();
            Exception failure=null;
            try { if(_traitSaveSteps.MoveNext()) return; } catch(Exception e) { failure=e; }
            try { _traitSaveSteps.Dispose(); } catch(Exception e) { failure=failure==null?e:new AggregateException(failure,e); }
            _traitSaveSteps=null;
            WriteTraitSaveReceipt(failure);
            Complete(CreateResult(failure==null?RuntimeTestStatuses.Pass:RuntimeTestStatuses.Error,
                _traitSaveChecks,failure?.ToString()));
        }
        private void StopElementalTraitSave(RuntimeTestResult result)
        {
            if(_traitSaveSteps==null) return;
            var steps=_traitSaveSteps;_traitSaveSteps=null;
            try { steps.Dispose(); } catch(Exception e) { result.Status=RuntimeTestStatuses.Error;result.Diagnostics.Add(e.ToString()); }
            WriteTraitSaveReceipt(new InvalidOperationException("Stopped before persistence completion."));
        }
        private void WriteTraitSaveReceipt(Exception failure)
        {
            WriteTeleportationForensicJson(TraitSaveEvidence,new {schemaVersion=1,runId=_request.RunId,
                transactionId=_traitSavePlan.Transaction,phase=_traitSavePlan.Phase,processId=Process.GetCurrentProcess().Id,
                dllSha256=TeleportPersistencePlan.Hash(typeof(RuntimeTestRunner).Assembly.Location),
                witness=_traitSaveWitness,savedInfo=_traitSavedInfo,events=_traitSaveEvents,
                saveWrites=_traitWriteLease?.RoutineCount??0,unexpectedSaveWritingApiObserved=_workingSaveSmoke.WriteObserved,
                respecUiAutomated=false,savedFeatureRemovalLifecycleQualified=_traitSavePlan.Phase!="prepare"&&failure==null,
                error=failure?.ToString()});
        }
        private Fact[] TraitAllFacts(UnitDescriptor owner)
        {
            var graph=ElementalCharacterTraitPublicationCoordinator.Graph;
            return graph.Features[0].GetTargetCollection(owner).RawFacts
                .Concat(((BlueprintBuff)graph.Resolve(ElementalCharacterTraitCatalog.All()[3].GrantedNode.Symbol)).GetTargetCollection(owner).RawFacts)
                .Concat(((BlueprintActivatableAbility)graph.Resolve(ElementalCharacterTraitCatalog.All()[0].GrantedNode.Symbol)).GetTargetCollection(owner).RawFacts)
                .Distinct().ToArray();
        }
        private JObject TraitForeignWitness(UnitEntityData main)
        {
            var game=Game.Instance;var owned=new HashSet<string>(ElementalCharacterTraitCatalog.Nodes().Select(n=>n.Guid));
            return new JObject {
                ["owner"]=main.UniqueId,["party"]=new JArray(game.Player.Party.Select(u=>u.UniqueId)),
                ["partyCharacters"]=new JArray(game.Player.PartyCharacters.Select(u=>u.UniqueId)),
                ["money"]=game.Player.Money,["gameTime"]=game.Player.GameTime.Ticks,
                ["inventory"]=new JArray(game.Player.Inventory.Items.Select((item,index)=>new JObject {
                    ["index"]=index,["slot"]=item.InventorySlotIndex,["blueprint"]=item.Blueprint.AssetGuid,
                    ["count"]=item.Count,["identified"]=item.IsIdentified})),
                ["foreignFacts"]=new JArray(game.Player.Party.OrderBy(u=>u.UniqueId,StringComparer.Ordinal).Select(u=>
                    new JObject {["unit"]=u.UniqueId,["facts"]=new JArray(TraitAllFacts(u.Descriptor)
                        .Where(f=>!owned.Contains(f.Blueprint.AssetGuid)).Select(f=>f.Blueprint.AssetGuid).OrderBy(id=>id,StringComparer.Ordinal))}))
            };
        }
        private AreaEffectEntityData[] TraitOwnedAreas()
        {
            var graph=ElementalCharacterTraitPublicationCoordinator.Graph;
            var guid=ElementalCharacterTraitCatalog.All()[1].Node(CharacterTraitNodeKind.Area).Guid;
            var provider=graph.Resolve(ElementalCharacterTraitCatalog.All()[1].GrantedNode.Symbol);
            var field=typeof(AddAreaEffect).GetField("m_AreaEffectInstance",BindingFlags.NonPublic|BindingFlags.Instance);
            if(field==null || field.FieldType!=typeof(AreaEffectEntityData)) throw new InvalidOperationException("Exact native attached-area carrier absent.");
            var attached=Game.Instance.State.Units.All.SelectMany(u=>u.Buffs.Enumerable)
                .Where(b=>ReferenceEquals(b.Blueprint,provider))
                .SelectMany(b=>b.SelectComponents<AddAreaEffect>())
                .Select(c=>field.GetValue(c) as AreaEffectEntityData).Where(a=>a!=null && a.Blueprint.AssetGuid==guid);
            // Attached effects belong to their owner and need not occur in the global area list.
            return ElementalCharacterTraitSaveContract.OwnedAreas(
                Game.Instance.State.AreaEffects.All.Where(a=>a.Blueprint.AssetGuid==guid),attached);
        }
        private void TraitCanonicalGraph()
        {
            var graph=ElementalCharacterTraitPublicationCoordinator.Graph;
            var resolved=ElementalCharacterTraitNativeGraph.ResolveCanonicalGraphIfFullyRegistered(
                BlueprintBootstrap.Library,BlueprintBootstrap.ElementalRaces,
                BlueprintBootstrap.ElementalFeats.Require<BlueprintBuff>(ElementalCharacterTraitCatalog.WingsOfAirGuid));
            TraitSaveAssert("all-eleven-canonical",resolved!=null && ElementalCharacterTraitCatalog.Nodes().All(n=>
                ReferenceEquals(graph.Resolve(n.Symbol),resolved.Resolve(n.Symbol)) &&
                ReferenceEquals(graph.Resolve(n.Symbol),BlueprintBootstrap.Library.BlueprintsByAssetId[n.Guid])),
                new {nodes=graph.Nodes.Select(n=>new {n.Key,guid=n.Value.AssetGuid}).ToArray()});
        }
        private Fact[] TraitFeatures(UnitEntityData owner,bool present)
        {
            var graph=ElementalCharacterTraitPublicationCoordinator.Graph;
            var features=graph.Features.Select(bp=>bp.GetTargetCollection(owner.Descriptor).RawFacts
                .Where(f=>ReferenceEquals(f.Blueprint,bp)).ToArray()).ToArray();
            TraitSaveAssert(present?"four-visible-once":"four-visible-absent",features.All(f=>f.Length==(present?1:0)),
                new {counts=features.Select(f=>f.Length).ToArray()});
            return present?features.Select(f=>f.Single()).ToArray():Array.Empty<Fact>();
        }
        private Fact[] TraitProviders(UnitEntityData owner,Fact[] features,bool present,bool requireToggleOn)
        {
            var graph=ElementalCharacterTraitPublicationCoordinator.Graph;var defs=ElementalCharacterTraitCatalog.All();
            if(!present)
            {
                TraitSaveAssert("hidden-state-absent",defs.All(d=>((BlueprintUnitFact)graph.Resolve(d.GrantedNode.Symbol))
                    .GetTargetCollection(owner.Descriptor).RawFacts.All(f=>!ReferenceEquals(f.Blueprint,graph.Resolve(d.GrantedNode.Symbol)))) &&
                    !owner.Buffs.Enumerable.Any(b=>ElementalCharacterTraitCatalog.Nodes().Any(n=>n.Guid==b.Blueprint.AssetGuid)) &&
                    TraitOwnedAreas().Length==0,new {areas=TraitOwnedAreas().Length});
                return Array.Empty<Fact>();
            }
            var grants=features.Select(f=>f.SelectComponents<ElementalCharacterTraitOwnedGrant>().Single()).ToArray();
            var facts=grants.Select(g=>g.OwnedFact).ToArray();
            TraitSaveAssert("owned-grants-once",facts.All(f=>f!=null&&!f.IsDisposed) && defs.Select((d,i)=>
                ReferenceEquals(facts[i].Blueprint,graph.Resolve(d.GrantedNode.Symbol)) &&
                ((BlueprintUnitFact)graph.Resolve(d.GrantedNode.Symbol)).GetTargetCollection(owner.Descriptor).RawFacts
                    .Count(f=>ReferenceEquals(f.Blueprint,facts[i].Blueprint))==1).All(v=>v),
                new {grants=facts.Select(f=>f?.Blueprint.AssetGuid).ToArray()});
            var toggle=(ActivatableAbility)facts[0];
            TraitSaveAssert(requireToggleOn?"fiery-enabled-witness":"fiery-module-rebuilt-off",
                toggle.IsOn==requireToggleOn,new {toggle.IsOn,expectOn=requireToggleOn});
            var fieryBuff=graph.Resolve(defs[0].Node(CharacterTraitNodeKind.ActivationBuff).Symbol);
            TraitSaveAssert("fiery-buff-state",owner.Buffs.Enumerable.Count(b=>ReferenceEquals(b.Blueprint,fieryBuff))==(requireToggleOn?1:0),
                new {count=owner.Buffs.Enumerable.Count(b=>ReferenceEquals(b.Blueprint,fieryBuff))});
            var stoic=(Buff)facts[1];var add=stoic.SelectComponents<AddAreaEffect>().Single();
            var area=(AreaEffectEntityData)typeof(AddAreaEffect).GetField("m_AreaEffectInstance",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(add);
            TraitSaveAssert("stoic-holder-context-area",ReferenceEquals(stoic.Context?.MaybeCaster,owner) &&
                area!=null && !area.Destroyed && !area.IsEnded && ReferenceEquals(area.Context?.MaybeCaster,owner) &&
                ReferenceEquals(area.Blueprint,graph.Resolve(defs[1].Node(CharacterTraitNodeKind.Area).Symbol)) && TraitOwnedAreas().Length==1,
                new {caster=stoic.Context?.MaybeCaster?.UniqueId,areaCaster=area?.Context?.MaybeCaster?.UniqueId,areas=TraitOwnedAreas().Length});
            var aerial=facts[2].SelectComponents<AerialObserverPerceptionBonus>().Single();
            TraitSaveAssert("aerial-canonical-wings",ReferenceEquals(aerial.FlightCarrier,
                ResourcesLibrary.TryGetBlueprint<BlueprintBuff>(AerialObserverFlightContract.CarrierGuid)),
                new {carrier=aerial.FlightCarrier.AssetGuid});
            var white=facts[3].SelectComponents<WhiteoutWeatherProvider>().Single();
            TraitSaveAssert("whiteout-canonical-backlink",ReferenceEquals(white.ProviderIdentity,facts[3].Blueprint)&&white.HasExactIdentity(),
                new {provider=white.ProviderIdentity.AssetGuid});
            return facts;
        }
        private void TraitOwnedCleanup(Fact[] removed)
        {
            var graph=ElementalCharacterTraitPublicationCoordinator.Graph;
            var recipient=graph.Resolve(ElementalCharacterTraitCatalog.All()[1].Node(CharacterTraitNodeKind.RecipientBuff).Symbol);
            int recipients=Game.Instance.State.Units.All.Sum(u=>u.Buffs.Enumerable.Count(b=>ReferenceEquals(b.Blueprint,recipient)));
            TraitSaveAssert("recipient-cleanup",recipients==0,new {recipients});
            var weather=WhiteoutListeners(typeof(IWeatherChangeHandler));
            var scene=WhiteoutListeners(typeof(ISceneHandler));
            var whites=removed.Where(f=>f!=null).SelectMany(f=>f.SelectComponents<WhiteoutWeatherProvider>()).ToArray();
            TraitSaveAssert("owned-listener-state-cleanup",whites.All(c=>!weather.Contains(c)&&!scene.Contains(c)&&!c.State.Active),
                new {ownedWeatherListeners=whites.Count(c=>weather.Contains(c)),ownedSceneListeners=whites.Count(c=>scene.Contains(c))});
            var aerials=removed.Where(f=>f!=null).SelectMany(f=>f.SelectComponents<AerialObserverPerceptionBonus>()).ToArray();
            var modifier=typeof(AerialObserverPerceptionBonus).GetField("_modifier",BindingFlags.NonPublic|BindingFlags.Instance);
            TraitSaveAssert("owned-perception-modifier-cleanup",modifier!=null&&aerials.All(c=>modifier.GetValue(c)==null)&&
                Game.Instance.Player.Party.All(u=>u.Stats.SkillPerception.Modifiers.All(m=>!removed.Any(f=>ReferenceEquals(m.Source,f)))),
                new {removedProviders=removed.Length});
        }
        private void TraitModule(bool enabled)
        {
            var settings=_context.FeatureModules;var p=settings.Pending;
            settings.SetPending(p.Gunslinger,p.AcadamaeGraduate,p.ShieldOther,p.ExpandedSummoning,p.ElvenBranchedSpears,
                p.EasternWeapons,p.BrownFurTransmuter,p.UrbanBarbarian,p.BodyguardFeats,p.ProtectionFromAlignmentControlImmunity,
                enabled,p.TeleportationSpells,p.MagicCircleSpells);
        }
        private IEnumerable<int> RunElementalTraitSave()
        {
            var plan=_traitSavePlan;plan.RequireLease();
            var game=Game.Instance;var owner=game.Player.MainCharacter.Value;
            bool paused=game.IsPaused;bool module=_context.FeatureModules.Pending.ElementalRaces;
            if(!module || !ElementalCharacterTraitPublicationCoordinator.Published || owner==null || !owner.IsPlayerFaction)
                throw new InvalidOperationException("Canonical publication and existing player-owned witness required.");
            game.IsPaused=true;
            try
            {
                TraitCanonicalGraph();
                var initial=TraitForeignWitness(owner);
                if(plan.Phase=="prepare")
                {
                    TraitFeatures(owner,false);
                    foreach(var feature in ElementalCharacterTraitPublicationCoordinator.Graph.Features)
                        owner.Descriptor.AddFact(feature);
                    var features=TraitFeatures(owner,true);
                    var toggle=(ActivatableAbility)features[0].SelectComponents<ElementalCharacterTraitOwnedGrant>().Single().OwnedFact;
                    toggle.IsOn=true;
                    TraitProviders(owner,features,true,true);
                    TraitSaveAssert("seed-foreign-state-intact",JToken.DeepEquals(initial,TraitForeignWitness(owner)),new {foreign=initial});
                    _traitSaveWitness=initial;
                    foreach(var tick in SaveElementalTraitOwned(false)) yield return tick;
                }
                else if(plan.Phase=="verify-remove")
                {
                    TraitSaveAssert("fresh-foreign-witness",JToken.DeepEquals(initial,plan.Expected),new {expected=plan.Expected,actual=initial});
                    var features=TraitFeatures(owner,true);
                    var savedGrants=TraitProviders(owner,features,true,true);
                    var priorAreas=TraitOwnedAreas();
                    TraitModule(false);
                    foreach(var ended in priorAreas) ended.Tick();
                    game.EntityDestroyer.Tick();game.EntityDestroyer.Tick();
                    TraitSaveAssert("module-off-exact-removal",features.All(f=>f.SelectComponents<ElementalCharacterTraitOwnedGrant>().Single().OwnedFact==null) &&
                        savedGrants.All(f=>f.IsDisposed),new {disposed=savedGrants.Select(f=>f.IsDisposed).ToArray()});
                    TraitProviders(owner,features,false,false);TraitOwnedCleanup(savedGrants);TraitCanonicalGraph();
                    TraitModule(true);
                    TraitProviders(owner,features,true,false);
                    var rebuilt=features.Select(f=>f.SelectComponents<ElementalCharacterTraitOwnedGrant>().Single().OwnedFact).ToArray();
                    priorAreas=TraitOwnedAreas();
                    foreach(var feature in features) owner.Descriptor.RemoveFact(feature);
                    foreach(var ended in priorAreas) ended.Tick();
                    game.EntityDestroyer.Tick();game.EntityDestroyer.Tick();
                    TraitFeatures(owner,false);TraitProviders(owner,features,false,false);TraitOwnedCleanup(rebuilt);TraitCanonicalGraph();
                    TraitSaveAssert("visible-removal-owned-cleanup",features.All(f=>f.IsDisposed)&&rebuilt.All(f=>f.IsDisposed),
                        new {featuresDisposed=features.All(f=>f.IsDisposed),ownedDisposed=rebuilt.All(f=>f.IsDisposed),respecUiAutomated=false});
                    TraitSaveAssert("removal-foreign-state-intact",JToken.DeepEquals(initial,TraitForeignWitness(owner)),new {foreign=initial});
                    _traitSaveWitness=initial;
                    foreach(var tick in SaveElementalTraitOwned(true)) yield return tick;
                }
                else
                {
                    TraitSaveAssert("absence-foreign-witness",JToken.DeepEquals(initial,plan.Expected),new {expected=plan.Expected,actual=initial});
                    TraitFeatures(owner,false);TraitProviders(owner,Array.Empty<Fact>(),false,false);
                    TraitOwnedCleanup(Array.Empty<Fact>());
                    TraitSaveAssert("absence-no-owned-weather-listener",!WhiteoutListeners(typeof(IWeatherChangeHandler)).OfType<WhiteoutWeatherProvider>().Any(),new {ownedListeners=0});
                    TraitSaveAssert("selection-remains-valid",ElementalCharacterTraitPublicationCoordinator.Published &&
                        ElementalCharacterTraitPublicationCoordinator.ResolveHost().Contract.RaceTraits.AllFeatures
                            .Count(f=>ElementalCharacterTraitPublicationCoordinator.Graph.Features.Contains(f))==4,new {published=true});
                    _traitSaveWitness=initial;_traitSavedInfo=plan.Input;
                }
                TraitSaveAssert("unrelated-save-api-absent",!_workingSaveSmoke.WriteObserved,new {unexpectedSaveWritingApiObserved=_workingSaveSmoke.WriteObserved});
            }
            finally
            {
                if(_context.FeatureModules.Pending.ElementalRaces!=module) TraitModule(module);
                game.IsPaused=paused;
            }
        }
        private IEnumerable<int> SaveElementalTraitOwned(bool overwrite)
        {
            var game=Game.Instance;var plan=_traitSavePlan;plan.RequireLease();
            if(!ElementalCharacterTraitSaveContract.MayWrite(plan.Transaction,plan.Phase,plan.OutputName,true,true) ||
                !game.SaveManager.IsSaveAllowed() || game.SaveManager.CommitInProgress ||
                Kingmaker.UI.SettingsUI.SettingsRoot.Instance.OnlyOneSave.CurrentValue)
                throw new InvalidOperationException("Exactly one leased manual save required.");
            var beforeOwned=overwrite?Array.Empty<Fact>():TraitFeatures(game.Player.MainCharacter.Value,true)
                .Select(f=>f.SelectComponents<ElementalCharacterTraitOwnedGrant>().Single().OwnedFact).ToArray();
            var requested=overwrite?_workingSaveSmoke.ExactLoadedDescriptor:game.SaveManager.CreateNewSave(plan.OutputName);
            Action<SaveInfo> prepared=save=>
            {
                plan.RequireLease();
                WriteTeleportationForensicJson(Path.Combine(_request.EvidenceDirectory,"elemental-trait-owned-save.json"),
                    new {schemaVersion=1,runId=_request.RunId,transactionId=plan.Transaction,phase=plan.Phase,
                        name=save.Name,file=Path.GetFileName(overwrite?plan.InputPath:save.FolderName),
                        path=overwrite?plan.InputPath:Path.GetFullPath(save.FolderName),gameId=save.GameId,
                        nativePreparedPath=Path.GetFullPath(save.FolderName),nativePreparedInitiallyAbsent=true,
                        existedBeforePreparation=overwrite,lifecycle="native-prepared-before-write"});
            };
            var lease=overwrite?GuardedDisposableSaveLease.ForOwnedOverwrite(requested,plan.OutputName,game.SaveManager.SavePath,
                plan.InputPath,(string)plan.Input["sha256"],prepared):
                new GuardedDisposableSaveLease(requested,plan.OutputName,game.SaveManager.SavePath,prepared);
            _traitWriteLease=lease;
            _workingSaveSmoke.ArmDisposableSave(lease);
            bool completed=false;game.SaveGame(requested,()=>{completed=true;});
            var watch=Stopwatch.StartNew();
            while(!completed || lease.Saved==null || game.SaveManager.CommitInProgress ||
                lease.Saved.OperationState!=SaveInfo.StateType.None || LoadingProcess.Instance.IsLoadingInProcess || !File.Exists(lease.Saved.FolderName))
            { if(watch.Elapsed.TotalSeconds>120) throw new InvalidOperationException("Owned native manual save did not complete.");yield return 0; }
            TraitSaveAssert(overwrite?"owned-removal-save":"owned-prepare-save",lease.RoutineCount==1&&!_workingSaveSmoke.WriteObserved,
                new {lease.RoutineCount,lease.StashedAreaCount,name=lease.Saved.Name,file=lease.Saved.FileName});
            TraitSaveAssert("native-owned-commit-path",string.Equals(Path.GetFullPath(lease.Saved.FolderName),lease.CommitPath,
                StringComparison.OrdinalIgnoreCase) && (!overwrite || (!File.Exists(lease.PreparedPath)&&!Directory.Exists(lease.PreparedPath))),
                new {finalPath=lease.Saved.FolderName,nativePreparedPath=lease.PreparedPath,temporaryAbsent=overwrite});
            if(!overwrite)
            {
                var features=TraitFeatures(game.Player.MainCharacter.Value,true);
                TraitSaveAssert("native-save-retains-exact-owned-facts",features.Select((f,i)=>
                    ReferenceEquals(f.SelectComponents<ElementalCharacterTraitOwnedGrant>().Single().OwnedFact,beforeOwned[i])).All(v=>v),
                    new {sameReferences=features.Select((f,i)=>ReferenceEquals(f.SelectComponents<ElementalCharacterTraitOwnedGrant>().Single().OwnedFact,beforeOwned[i])).ToArray()});
                TraitProviders(game.Player.MainCharacter.Value,features,true,true);
            }
            else TraitFeatures(game.Player.MainCharacter.Value,false);
            var saved=lease.Saved;
            _traitSavedInfo=new JObject {["name"]=saved.Name,["file"]=saved.FileName,["path"]=saved.FolderName,
                ["sha256"]=TeleportPersistencePlan.Hash(saved.FolderName),["gameName"]=saved.GameName,
                ["gameId"]=saved.GameId,["areaName"]=saved.Area.name,["partyCount"]=saved.PartyPortraits.Count};
            TraitSaveAssert("native-commit-identity",saved.Name==plan.OutputName && saved.GameId==game.Player.GameId &&
                saved.Area==game.CurrentlyLoadedArea && saved.PartyPortraits.Count==game.Player.Party.Count,
                new {name=saved.Name,gameId=saved.GameId,area=saved.Area.AssetGuid,party=saved.PartyPortraits.Count});
        }
    }
}
