using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Commands;
using KingmakerGunslinger.Assets;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;
namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private static SkinnedMeshRenderer Sprint27Skin(UnitEntityData unit)
        {
            return unit?.View==null?null:unit.View.GetComponentsInChildren<SkinnedMeshRenderer>(true).SingleOrDefault(v=>v.enabled&&v.sharedMesh!=null&&v.sharedMesh.name=="KMG_griffon_Original");
        }
        private IEnumerable<int> Sprint27VisualLifecycle(ExpandedSummoningCorrectionFixture fixture,UnitEntityData owner)
        {
            Mesh cache;string[] bones;Texture2D paint;string status;
            if(!PteranodonAssetRuntime.TryGetGriffonVisual(out cache,out bones,out paint,out status))throw new InvalidOperationException("Griffon cache absent before lifecycle.");
            var native=fixture.Blueprints.OfType<BlueprintUnit>().Single(v=>v.AssetGuid==GriffonVisualPolicy.DonorGuid);
            var control=Game.Instance.EntityCreator.SpawnUnit(native,fixture.Caster.Position,Quaternion.identity,fixture.Scene);fixture.Created.Add(control);SetExpandedSummoningBrainActive(control,false);
            for(int f=0;f<60;f++)yield return 0;
            var donorSkins=control.View.GetComponentsInChildren<SkinnedMeshRenderer>(true);var donorMeshes=donorSkins.Select(v=>v.sharedMesh).ToArray();var donorEnabled=donorSkins.Select(v=>v.enabled).ToArray();
            try{
                int beforeMeshes=Resources.FindObjectsOfTypeAll<Mesh>().Count(v=>v.name.StartsWith("KMG_griffon_Original",StringComparison.Ordinal));
                int beforeMaterials=Resources.FindObjectsOfTypeAll<Material>().Count(v=>v.name.StartsWith("KMG_griffon_Original",StringComparison.Ordinal));
                UnitEntityData failed=null;int reached=0;
                try{
                    if(ExpandedSummoningPteranodonViewPatch.PostSuppressionFaultForTest!=null)throw new InvalidOperationException("Foreign attachment fault scope.");
                    try{
                        ExpandedSummoningPteranodonViewPatch.PostSuppressionFaultForTest=()=>{reached++;throw new InvalidOperationException("Sprint27 exact-view rollback probe");};
                        failed=CastExpandedSummoningOwnTier(fixture,"griffon");
                    }finally{ExpandedSummoningPteranodonViewPatch.PostSuppressionFaultForTest=null;}
                    SetExpandedSummoningBrainActive(failed,false);foreach(int f in Sprint26Appearance(failed,"griffon-rollback",false))yield return f;
                    string outcome=ExpandedSummoningPteranodonViewPatch.DescribeView(failed.View);
                    bool nativeBody=failed.View.GetComponentsInChildren<SkinnedMeshRenderer>(true).Any(v=>v.enabled&&donorMeshes.Contains(v.sharedMesh));
                    bool exact=reached==1&&nativeBody&&outcome.StartsWith("donor-visual:attach-failed:",StringComparison.Ordinal)&&ExpandedSummoningPteranodonViewPatch.CaptureGriffonOwnedResources(failed.View).Length==0;
                    Sprint25Check("post-suppression-rollback",exact,new JObject{["owner"]=failed.UniqueId,["faultReached"]=reached,["outcome"]=outcome,["nativeDonorVisible"]=nativeBody},"An injected attachment failure after the swap rolls back to the exact usable donor and releases all private original-body objects. Fallback is accepted only for this explicit failure control.");
                }finally{if(failed!=null)DisposeExpandedSummoningUnits(fixture.Created,new[]{failed});}
                for(int f=0;f<60;f++){Game.Instance.EntityDestroyer.Tick();yield return 0;}
                int afterMeshes=Resources.FindObjectsOfTypeAll<Mesh>().Count(v=>v.name.StartsWith("KMG_griffon_Original",StringComparison.Ordinal));
                int afterMaterials=Resources.FindObjectsOfTypeAll<Material>().Count(v=>v.name.StartsWith("KMG_griffon_Original",StringComparison.Ordinal));
                Mesh after;string[] names;Texture2D albedo;string afterStatus;
                bool sameCache=PteranodonAssetRuntime.TryGetGriffonVisual(out after,out names,out albedo,out afterStatus)&&ReferenceEquals(cache,after)&&ReferenceEquals(paint,albedo)&&bones.SequenceEqual(names)&&status==afterStatus;
                Sprint25Check("rollback-bounded-resources",sameCache&&beforeMeshes==afterMeshes&&beforeMaterials==afterMaterials,new JObject{["meshesBefore"]=beforeMeshes,["meshesAfter"]=afterMeshes,["materialsBefore"]=beforeMaterials,["materialsAfter"]=afterMaterials,["cacheExact"]=sameCache},"Rollback leaves exact baseline private-object counts and immutable shared cache references.");
                var route=ExpandedSummoningOwnTierVariant("griffon",SummonMultiplicity.OneD4PlusOne);
                var group=CastExpandedSummoningVariant(fixture.Blueprints,fixture.Caster,route,null,fixture.Evidence);fixture.Created.AddRange(group);var resources=new List<UnityEngine.Object>();
                try{
                    foreach(var member in group){SetExpandedSummoningBrainActive(member,false);foreach(int f in Sprint26Appearance(member,"griffon-quantity",false))yield return f;resources.AddRange(ExpandedSummoningPteranodonViewPatch.CaptureGriffonOwnedResources(member.View));}
                    bool quantity=group.Length>=2&&group.Length<=5&&group.All(v=>Sprint27Skin(v)!=null&&(bool)Sprint22SelectionSurface(v)["selectionSurface"]&&(bool)Sprint26Pose(Sprint27Skin(v))["finite"]);
                    quantity&=resources.OfType<Mesh>().Count()==group.Length&&resources.Distinct().Count()==resources.Count&&group.Where(v=>Sprint27Skin(v)!=null).Select(v=>Sprint27Skin(v).sharedMesh).Distinct().Count()==group.Length;
                    Sprint25Check("quantity-instance-isolation",quantity,new JObject{["count"]=group.Length,["owners"]=new JArray(group.Select(v=>v.UniqueId)),["privateObjects"]=resources.Count,["privateMeshes"]=resources.OfType<Mesh>().Count(),["scope"]="native internal root multiplicity; public spell-slot path is a separate observation"},"Every member of the native1d4+1 root has its own original mesh/materials and selection surface, sharing only immutable cache data.");
                    var moving=group[0];Vector3 destination;string survey,direction;
                    Vector3 origin=Sprint24AiCombatLane(fixture.Caster,moving,fixture.Hostile,6f,out destination,out survey,out direction);PlaceExpandedSummoningUnit(moving,origin);
                    var move=new UnitMoveTo(destination,.5f);move.Init(moving);if(!move.CanStart)throw new InvalidOperationException("Griffon native movement refused.");moving.Commands.Run(move);
                    int frames=0;float velocity=0;DateTime deadline=DateTime.UtcNow.AddSeconds(25);
                    while(!move.IsFinished&&frames++<2500&&DateTime.UtcNow<deadline){velocity=Math.Max(velocity,moving.View.MovementAgent.Velocity.magnitude);yield return 0;}
                    float travel=Vector3.Distance(origin,moving.Position),gap=Vector3.Distance(destination,moving.Position);
                    Sprint25Check("ordinary-map-airborne-movement",move.IsFinished&&travel>1f&&gap<=1.5f&&velocity>.01f,new JObject{["owner"]=moving.UniqueId,["survey"]=survey,["travel"]=travel,["gap"]=gap,["maximumVelocity"]=velocity,["corpulence"]=moving.Corpulence,["speedFeet"]=moving.Stats.Speed.ModifiedValue,["finished"]=move.IsFinished},"The selected80-foot ordinary-map representation navigates a real connected route with native movement and command completion; no Z-axis or separate land mode is claimed.");
                    Sprint27Body(moving,"movement");moving.Commands.InterruptAll(true);
                    var dead=group[0];var dyingSkin=Sprint27Skin(dead);bool observedDead=false,finite=true;
                    Kingmaker.Designers.GameHelper.KillUnit(dead,fixture.Hostile);deadline=DateTime.UtcNow.AddSeconds(8);
                    for(int f=0;f<900&&DateTime.UtcNow<deadline&&dead.View!=null;f++){yield return 0;observedDead|=dead.Descriptor.State.IsDead;if(dyingSkin!=null)finite&=(bool)Sprint26Pose(dyingSkin)["finite"];if(observedDead&&f>=30)break;}
                    Sprint25Check("native-death",observedDead&&finite,new JObject{["owner"]=dead.UniqueId,["dead"]=observedDead,["finite"]=finite},"Native death on an exact disposable original-body Griffon has finite geometry and retains native lifecycle ownership.");
                    var fading=Sprint27Skin(group[1]);var expiries=new JArray();
                    foreach(var member in group){var marker=member.Buffs.Enumerable.SingleOrDefault(v=>ReferenceEquals(v.Blueprint,BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff));if(marker!=null)expiries.Add(ExpireSprint16OwnedBuff(member,marker));}
                    float dissolve=0;deadline=DateTime.UtcNow.AddSeconds(18);
                    for(int f=0;f<2500&&DateTime.UtcNow<deadline;f++){yield return 0;ObserveSprint17NativeExpiryMaterial(fading,ref dissolve);if(group.All(v=>v.Destroyed&&v.View==null&&v.HoldingState==null))break;}
                    Sprint25Check("native-expiry-fading",group.All(v=>v.Destroyed&&v.View==null&&v.HoldingState==null)&&dissolve>.05f,new JObject{["count"]=group.Length,["nativeExpiries"]=expiries,["maxDissolve"]=dissolve,["destroyed"]=group.Count(v=>v.Destroyed&&v.View==null&&v.HoldingState==null)},"Native summon expiry fades the actual original body and removes all logical/view/holding state; fixture destruction does not supply this assertion.");
                }finally{DisposeExpandedSummoningUnits(fixture.Created,group);}
                for(int f=0;f<90&&resources.Any(v=>v!=null);f++){Game.Instance.EntityDestroyer.Tick();yield return 0;}
                Sprint25Check("private-cleanup-cache-survives",resources.All(v=>v==null)&&cache!=null&&paint!=null,new JObject{["captured"]=resources.Count,["remaining"]=resources.Count(v=>v!=null),["cacheSurvives"]=cache!=null&&paint!=null},"Native teardown releases every captured private view object and preserves borrowed native assets and the one immutable process cache.");
                bool isolated=donorSkins.Select(v=>v.sharedMesh).SequenceEqual(donorMeshes)&&donorSkins.Select(v=>v.enabled).SequenceEqual(donorEnabled)&&donorSkins.All(v=>v.sharedMesh==null||!v.sharedMesh.name.StartsWith("KMG_",StringComparison.Ordinal));
                Sprint25Check("native-manticore-donor-isolation",isolated,new JObject{["donor"]=native.AssetGuid,["owner"]=control.UniqueId},"The exact native Manticore control retains its original native meshes and renderer visibility through attachment, rollback, quantity and cleanup.");
            }finally{DisposeExpandedSummoningUnits(fixture.Created,new[]{control});}
        }
    }
}
