using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Assets;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private static SkinnedMeshRenderer Sprint26Skin(UnitEntityData unit, string key)
        {
            return unit?.View == null ? null : unit.View.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .SingleOrDefault(v => v.enabled && v.sharedMesh != null && (key == "gorgon"
                    ? v.sharedMesh.name == "KMG_gorgon_Original" : v.name == "Bulette"));
        }

        private IEnumerable<int> Sprint26VisualLifecycle(ExpandedSummoningCorrectionFixture fixture)
        {
            Sprint26IconConsumers(fixture);
            var controls = new List<UnitEntityData>();
            var meshes = new Dictionary<UnitEntityData, Mesh[]>();
            var enabled = new Dictionary<UnitEntityData, bool[]>();
            foreach (string id in new[] { "5bb9579fdb2b26b48bb10d61c81cfdfb", "18fa241bd1afd77438564ec92614f7f1" }) {
                var blueprint = fixture.Blueprints.OfType<BlueprintUnit>().Single(v => v.AssetGuid == id);
                var unit = Game.Instance.EntityCreator.SpawnUnit(blueprint, fixture.Caster.Position, Quaternion.identity, fixture.Scene);
                controls.Add(unit); fixture.Created.Add(unit); SetExpandedSummoningBrainActive(unit, false);
            }
            for (int i=0;i<60;i++) yield return 0;
            foreach (var unit in controls) {
                var skins = unit.View.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                meshes.Add(unit, skins.Select(v=>v.sharedMesh).ToArray()); enabled.Add(unit, skins.Select(v=>v.enabled).ToArray());
            }
            try {
                Mesh cache; string[] bones; Texture2D paint; string status;
                bool cached = PteranodonAssetRuntime.TryGetUngulateVisual("gorgon",out cache,out bones,out paint,out status);
                if (!cached) throw new InvalidOperationException("Gorgon cache absent before lifecycle.");
                for (int fault=0;fault<2;fault++) {
                    UnitEntityData failed=null; int reached=0;
                    int meshCount=Resources.FindObjectsOfTypeAll<Mesh>().Count(v=>v.name.StartsWith("KMG_gorgon_Original",StringComparison.Ordinal));
                    int materialCount=Resources.FindObjectsOfTypeAll<Material>().Count(v=>v.name.StartsWith("KMG_gorgon_Original",StringComparison.Ordinal));
                    try {
                        if (fault==0) using(PteranodonAssetRuntime.WithdrawGorgonForTest()) failed=CastExpandedSummoningOwnTier(fixture,"gorgon");
                        else {
                            if(ExpandedSummoningPteranodonViewPatch.PostSuppressionFaultForTest!=null) throw new InvalidOperationException("Foreign attachment fault scope.");
                            try { ExpandedSummoningPteranodonViewPatch.PostSuppressionFaultForTest=()=>{reached++;throw new InvalidOperationException("Sprint26 exact-view rollback probe");};
                                failed=CastExpandedSummoningOwnTier(fixture,"gorgon"); }
                            finally {ExpandedSummoningPteranodonViewPatch.PostSuppressionFaultForTest=null;}
                        }
                        SetExpandedSummoningBrainActive(failed,false);
                        foreach(int f in Sprint26Appearance(failed,"gorgon-fallback-"+fault,false))yield return f;
                        string outcome=ExpandedSummoningPteranodonViewPatch.DescribeView(failed.View);
                        var skins=failed.View.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                        bool exact=skins.Any(v=>v.enabled&&meshes[controls[0]].Contains(v.sharedMesh))&&
                            ExpandedSummoningPteranodonViewPatch.CaptureGorgonOwnedResources(failed.View).Length==0&&
                            (fault==0?outcome.StartsWith("donor-visual:withdrawn-for-test:",StringComparison.Ordinal):reached==1&&outcome.StartsWith("donor-visual:attach-failed:",StringComparison.Ordinal));
                        Sprint25Check("gorgon-fault-"+fault,exact,new JObject{["unit"]=failed.UniqueId,["outcome"]=outcome,["faultReached"]=reached},"Unavailable payload and post-swap exception each leave the exact native donor usable with no owned attachment.");
                    } finally {if(failed!=null)DisposeExpandedSummoningUnits(fixture.Created,new[]{failed});}
                    for(int f=0;f<60;f++){Game.Instance.EntityDestroyer.Tick();yield return 0;}
                    Mesh after;string[] names;Texture2D albedo;string afterStatus;
                    bool same=PteranodonAssetRuntime.TryGetUngulateVisual("gorgon",out after,out names,out albedo,out afterStatus)&&
                        ReferenceEquals(cache,after)&&ReferenceEquals(paint,albedo)&&bones.SequenceEqual(names)&&status==afterStatus;
                    int meshAfter=Resources.FindObjectsOfTypeAll<Mesh>().Count(v=>v.name.StartsWith("KMG_gorgon_Original",StringComparison.Ordinal));
                    int materialAfter=Resources.FindObjectsOfTypeAll<Material>().Count(v=>v.name.StartsWith("KMG_gorgon_Original",StringComparison.Ordinal));
                    Sprint25Check("gorgon-fault-cleanup-"+fault,same&&meshAfter==meshCount&&materialAfter==materialCount,
                        new JObject{["cacheExact"]=same,["meshesBefore"]=meshCount,["meshesAfter"]=meshAfter,["materialsBefore"]=materialCount,["materialsAfter"]=materialAfter},"Fault rollback restores exact process-cache references and leaves no additional project body mesh or material.");
                }
                foreach(string key in new[]{"gorgon","bulette"}) {
                    var route=ExpandedSummoningOwnTierVariant(key,key=="gorgon"?SummonMultiplicity.OneD3:SummonMultiplicity.OneD4PlusOne);
                    var group=CastExpandedSummoningVariant(fixture.Blueprints,fixture.Caster,route,null,fixture.Evidence);fixture.Created.AddRange(group);
                    var resources=new List<UnityEngine.Object>();
                    try {
                        foreach(var member in group) {
                            SetExpandedSummoningBrainActive(member,false);
                            foreach(int f in Sprint26Appearance(member,key))yield return f;
                            if(key=="gorgon")resources.AddRange(ExpandedSummoningPteranodonViewPatch.CaptureGorgonOwnedResources(member.View));
                        }
                        bool quantity=group.Length>=(key=="gorgon"?1:2)&&group.Length<=(key=="gorgon"?3:5)&&group.All(v=>Sprint26Skin(v,key)!=null);
                        if(key=="gorgon")quantity&=resources.OfType<Mesh>().Count()==group.Length&&resources.Distinct().Count()==resources.Count;
                        Sprint25Check("quantity-body-"+key,quantity,new JObject{["route"]=route.StableKey,["units"]=new JArray(group.Select(v=>v.UniqueId)),["privateCaptured"]=resources.Count},"A real quantity cast creates the legal count of visible selectable bodies, with independent per-view Gorgon mesh ownership.");
                        var owner=group[0];Vector3 destination;string survey,direction;
                        Vector3 origin=Sprint24AiCombatLane(fixture.Caster,owner,fixture.Hostile,6f,out destination,out survey,out direction);
                        PlaceExpandedSummoningUnit(owner,origin);var move=new UnitMoveTo(destination,.5f);move.Init(owner);
                        if(!move.CanStart)throw new InvalidOperationException("No native movement start: "+key);
                        owner.Commands.Run(move);DateTime deadline=DateTime.UtcNow.AddSeconds(20);int frames=0;float velocity=0;
                        while(!move.IsFinished&&frames++<2500&&DateTime.UtcNow<deadline){velocity=Math.Max(velocity,owner.View.MovementAgent.Velocity.magnitude);if(frames==12)Sprint26Body(owner,key,"movement");yield return 0;}
                        Sprint25Check("movement-"+key,move.IsFinished&&Vector3.Distance(origin,owner.Position)>1&&Vector3.Distance(destination,owner.Position)<=1.5f&&velocity>.01f,
                            new JObject{["unit"]=owner.UniqueId,["survey"]=survey,["travel"]=Vector3.Distance(origin,owner.Position),["gap"]=Vector3.Distance(destination,owner.Position),["maximumVelocity"]=velocity,["finished"]=move.IsFinished},"Native traversable movement completes with a visible body and real movement-agent velocity.");
                        owner.Commands.InterruptAll(true);owner.Commands.RemoveFinishedAndUpdateQueue();
                        foreach(int f in Sprint25ConfinedMove(fixture,owner,key))yield return f;
                        var body=Sprint26Skin(owner,key);bool finite=true;bool dead=false;
                        Kingmaker.Designers.GameHelper.KillUnit(owner,fixture.Hostile);deadline=DateTime.UtcNow.AddSeconds(8);
                        for(int f=0;f<900&&DateTime.UtcNow<deadline&&owner.View!=null;f++){yield return 0;dead|=owner.Descriptor.State.IsDead;if(body!=null)finite&=(bool)Sprint26Pose(body)["finite"];if(dead&&f>=30)break;}
                        if(body!=null)Sprint26Body(owner,key,"death");
                        Sprint25Check("native-death-"+key,dead&&finite,new JObject{["unit"]=owner.UniqueId,["deadObserved"]=dead,["finite"]=finite},"Native KillUnit on an exact disposable summon preserves finite visible geometry and its native death path.");
                        var marker=owner.Buffs.Enumerable.Single(v=>ReferenceEquals(v.Blueprint,BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff));
                        var expiry=ExpireSprint16OwnedBuff(owner,marker);float dissolve=0;deadline=DateTime.UtcNow.AddSeconds(18);
                        for(int f=0;f<2500&&DateTime.UtcNow<deadline;f++){yield return 0;ObserveSprint17NativeExpiryMaterial(body,ref dissolve);if(owner.Destroyed&&owner.View==null&&owner.HoldingState==null)break;}
                        Sprint25Check("native-expiry-"+key,owner.Destroyed&&owner.View==null&&owner.HoldingState==null&&dissolve>.05f,
                            new JObject{["unit"]=owner.UniqueId,["expiry"]=expiry,["maxDissolve"]=dissolve,["destroyed"]=owner.Destroyed,["viewGone"]=owner.View==null,["holdingGone"]=owner.HoldingState==null},"The real summon expiry fades and removes its logical entity and view without fixture destruction supplying the assertion.");
                    } finally {DisposeExpandedSummoningUnits(fixture.Created,group);}
                    for(int f=0;f<90&&resources.Any(v=>v!=null);f++){Game.Instance.EntityDestroyer.Tick();yield return 0;}
                    Sprint25Check("private-cleanup-"+key,resources.All(v=>v==null)&&cache!=null&&paint!=null,new JObject{["captured"]=resources.Count,["remaining"]=resources.Count(v=>v!=null),["cacheSurvives"]=cache!=null&&paint!=null},"Exact per-view private objects are released while borrowed native and immutable process-cache assets survive.");
                }
                bool isolated=controls.All(unit=>{var skins=unit.View.GetComponentsInChildren<SkinnedMeshRenderer>(true);return skins.Select(v=>v.sharedMesh).SequenceEqual(meshes[unit])&&skins.Select(v=>v.enabled).SequenceEqual(enabled[unit])&&skins.All(v=>v.sharedMesh==null||!v.sharedMesh.name.StartsWith("KMG_",StringComparison.Ordinal));});
                Sprint25Check("native-donor-isolation",isolated,new JObject{["controls"]=new JArray(controls.Select(v=>v.Blueprint.AssetGuid))},"Untouched exact Horse and adult Bulette controls retain their original native meshes and renderer visibility.");
            } finally {DisposeExpandedSummoningUnits(fixture.Created,controls);}
        }

        private void Sprint26IconConsumers(ExpandedSummoningCorrectionFixture fixture)
        {
            var routes = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                .Concat(ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly))
                .Where(v=>v.Creature.Key=="gorgon"||v.Creature.Key=="bulette").ToArray();
            var rows=new JArray(); bool exact=routes.Length==6 && ExpandedSummoningProjectIcons.FallbackCount==0;
            foreach(var route in routes) {
                var icon=ExpandedSummoningProjectIcons.Require(route.Creature.Key);
                var root=ExpandedSummoningRoot(fixture.Blueprints,route);
                exact&=ReferenceEquals(root.Icon,icon)&&icon.rect.width==128&&icon.rect.height==128;
                rows.Add(new JObject{["route"]=route.StableKey,["ability"]=root.AssetGuid,["sprite"]=root.Icon?.GetInstanceID(),["expectedSprite"]=icon.GetInstanceID()});
            }
            var leap=fixture.Blueprints.OfType<BlueprintAbility>().Single(v=>v.name=="KMG_Summoning_Special_Bulette_Leap");
            var breath=fixture.Blueprints.OfType<BlueprintAbility>().Single(v=>v.name=="KMG_Summoning_Special_Gorgon_Breath");
            var trample=fixture.Blueprints.OfType<BlueprintAbility>().Single(v=>v.name=="KMG_Summoning_Special_Gorgon_Trample");
            var nativeStone=fixture.Blueprints.OfType<BlueprintBuff>().Single(v=>v.AssetGuid==Sprint26MagicalBeastRules.PetrifiedDonor);
            var nativeOverrun=fixture.Blueprints.OfType<BlueprintAbility>().Single(v=>v.AssetGuid=="1a3b471ecea51f7439a946b23577fd70");
            exact&=ReferenceEquals(leap.Icon,ProjectAssetIcons.RequireIcon("bulette-leap"))&&leap.Icon.rect.width==64&&leap.Icon.rect.height==64&&
                ReferenceEquals(breath.Icon,nativeStone.Icon)&&breath.Icon!=null&&ReferenceEquals(trample.Icon,nativeOverrun.Icon)&&trample.Icon!=null;
            var icons=new[]{ExpandedSummoningProjectIcons.Require("gorgon"),ExpandedSummoningProjectIcons.Require("bulette"),leap.Icon};
            string preview="sprint26-loaded-project-sprites-32px.png";
            RenderTexture previous=RenderTexture.active;var target=new RenderTexture(96,32,0);Texture2D pixels=null;bool matrix=false;
            try {
                RenderTexture.active=target;GL.Clear(true,true,new Color(.10f,.08f,.05f,1f));GL.PushMatrix();matrix=true;GL.LoadPixelMatrix(0,96,32,0);
                for(int i=0;i<icons.Length;i++)Graphics.DrawTexture(new Rect(i*32,0,32,32),icons[i].texture);
                pixels=new Texture2D(96,32,TextureFormat.RGBA32,false);pixels.ReadPixels(new Rect(0,0,96,32),0,0);pixels.Apply(false,false);
                File.WriteAllBytes(Path.Combine(_request.EvidenceDirectory,preview),EncodeExpandedSummoningPng(pixels));
            }finally{if(matrix)GL.PopMatrix();RenderTexture.active=previous;if(pixels!=null)UnityEngine.Object.DestroyImmediate(pixels);target.Release();UnityEngine.Object.DestroyImmediate(target);}
            Sprint25Check("icon-consumers",exact,new JObject{["roots"]=rows,["leapSprite"]=leap.Icon?.GetInstanceID(),["nativeStoneSprite"]=nativeStone.Icon?.GetInstanceID(),
                ["nativeTrampleSprite"]=nativeOverrun.Icon?.GetInstanceID(),["preview"]=preview,["previewMeaning"]="Actual loaded project sprites rendered at 32 pixels; supporting readability, not native menu navigation."},
                "All six roots carry their exact painted medallions, Leap its own emblem, and breath/trample their semantically correct native sprites.");
        }
    }
}
