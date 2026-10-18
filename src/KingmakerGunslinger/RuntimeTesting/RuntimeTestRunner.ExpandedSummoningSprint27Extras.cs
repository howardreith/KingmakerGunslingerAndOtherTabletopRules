using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using Kingmaker.Visual.LocalMap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;
namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private void Sprint27Routes(ExpandedSummoningCorrectionFixture fixture)
        {
            var routes=ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly).Where(v=>v.Creature.Key=="griffon").OrderBy(v=>v.ParentTier).ToArray();
            Sprint27Check("six-root-census",routes.Length==6&&routes.Select(v=>v.ParentTier).SequenceEqual(new[]{4,5,6,7,8,9})&&routes.All(v=>!v.Creature.MonsterTemplated)&&!ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster).Any(v=>v.Creature.Key=="griffon"),
                new JObject{["routes"]=new JArray(routes.Select(v=>v.StableKey)),["published"]=routes.Count(SummonVisibilityCatalog.IsPublished)},"Only the six untemplated SNA IV-IX roots, with no Monster entry.");
            foreach(var route in routes.Where(v=>!SummonVisibilityCatalog.IsPublished(v))){
                var units=CastExpandedSummoningVariant(fixture.Blueprints,fixture.Caster,route,null,fixture.Evidence);fixture.Created.AddRange(units);
                try{int low=route.Multiplicity==SummonMultiplicity.OneD4PlusOne?2:1,high=route.Multiplicity==SummonMultiplicity.One?1:route.Multiplicity==SummonMultiplicity.OneD3?3:5;
                    Sprint27Check("hidden-root-"+route.StableKey,units.Length>=low&&units.Length<=high&&units.All(v=>Sprint24TemplateCount(v)==0),new JObject{["count"]=units.Length,["route"]=route.StableKey,["scope"]="registered hidden-root effect only; public slot path deferred until publication"},"Native registered hidden roots produce their exact permitted quantities without templates.");
                }finally{DisposeExpandedSummoningUnits(fixture.Created,units);}
            }
            Sprint24PlayerRoutes(fixture,routes.Where(SummonVisibilityCatalog.IsPublished).ToArray(),Sprint27Check,false);
        }
        private void Sprint27ChargeRefusals(ExpandedSummoningCorrectionFixture fixture,UnitEntityData owner)
        {
            var target=fixture.Hostile;var granted=owner.Descriptor.Abilities.GetAbility(fixture.Blueprints.OfType<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>().Single(v=>v.AssetGuid=="c78506dd0e14f7c45a599990e4e65038"));var data=new AbilityData(granted);
            Vector3 originalOwner=owner.Position,originalTarget=target.Position;
            try{
                var near=Kingmaker.View.ObstacleAnalyzer.GetNearestNode(target.Position+Vector3.forward*.5f);
                if(near.node==null||!near.node.Walkable)throw new InvalidOperationException("Native near-charge fixture absent.");
                PlaceExpandedSummoningUnit(owner,near.clampedPosition);
                bool refused=!data.CanTarget(new TargetWrapper(target));
                Sprint27Check("too-close-native-refusal",refused,new JObject{["distance"]=owner.DistanceTo(target),["canTarget"]=!refused,["commandsIssued"]=0},"The actual native charge predicate refuses a too-close target; no illegal command or weapon rule is supplied.");
                bool found=false;var attempts=new JArray();
                foreach(float distance in new[]{6f,12f,18f}){
                    foreach(Vector3 offset in CompassOffsets){
                        var point=Kingmaker.View.ObstacleAnalyzer.GetNearestNode(originalTarget+offset*distance);
                        if(point.node==null||!point.node.Walkable||Vector3.Distance(point.clampedPosition,originalTarget+offset*distance)>.6f)continue;
                        PlaceExpandedSummoningUnit(owner,point.clampedPosition);PlaceExpandedSummoningUnit(target,originalTarget);
                        if(owner.DistanceTo(target)<6f)continue;
                        bool wall=Kingmaker.Visual.FogOfWar.LineOfSightGeometry.Instance.HasObstacle(owner.EyePosition,target.EyePosition,0);
                        float gap=Vector3.Distance(Kingmaker.View.ObstacleAnalyzer.TraceAlongNavmesh(owner.Position,target.Position),target.Position);
                        if(!wall&&gap<1f)continue;
                        bool blocked=!data.CanTarget(new TargetWrapper(target));
                        attempts.Add(new JObject{["origin"]=SurveyVector(owner.Position),["wall"]=wall,["navmeshGap"]=gap,["refused"]=blocked});
                        if(blocked){found=true;break;}
                    }if(found)break;
                }
                Sprint27Check("blocked-native-refusal",found,new JObject{["preconditionObtained"]=found,["candidates"]=attempts,["commandsIssued"]=0},"A measured native obstacle or interrupted straight navmesh route refuses charge; an unobtained obstruction is failure, not PASS.");
            }finally{PlaceExpandedSummoningUnit(owner,originalOwner);PlaceExpandedSummoningUnit(target,originalTarget);}
        }
        private IEnumerable<int> Sprint27TwoOwners(ExpandedSummoningCorrectionFixture fixture,UnitEntityData owner)
        {
            var second=CastExpandedSummoningOwnTier(fixture,"griffon");SetExpandedSummoningBrainActive(second,false);
            try{
                foreach(int f in Sprint26Appearance(second,"griffon-second-owner",false))yield return f;
                Sprint27Check("two-exact-owners",!ReferenceEquals(owner,second)&&owner.UniqueId!=second.UniqueId,new JObject{["first"]=owner.UniqueId,["second"]=second.UniqueId},"Two live independent Griffons coexist; neither name nor shared blueprint identifies command state.");
                foreach(int f in Sprint27Pounce(fixture,owner,false,true))yield return f;
                foreach(int f in Sprint27Ordinary(fixture,second,true))yield return f;
                foreach(int f in Sprint27Pounce(fixture,second,false))yield return f;
                foreach(int f in Sprint27Ordinary(fixture,owner,true))yield return f;
            }finally{second.Commands.InterruptAll(true);DisposeExpandedSummoningUnits(fixture.Created,new[]{second});}
        }
        private IEnumerable<int> Sprint27RemovalBoundaries(ExpandedSummoningCorrectionFixture fixture,UnitEntityData survivingOwner)
        {
            foreach(string mode in new[]{"death","expiry"}){
                var removed=CastExpandedSummoningOwnTier(fixture,"griffon");SetExpandedSummoningBrainActive(removed,false);
                try{
                    foreach(int f in Sprint26Appearance(removed,"griffon-charge-"+mode,false))yield return f;
                    foreach(int f in Sprint27CancelCharge(fixture,removed,false,mode))yield return f;
                }finally{removed.Commands.InterruptAll(true);DisposeExpandedSummoningUnits(fixture.Created,new[]{removed});}
                foreach(int f in Sprint27Ordinary(fixture,survivingOwner,true))yield return f;
            }
        }
        private IEnumerable<int> Sprint27Outdoor(ExpandedSummoningCorrectionFixture fixture,UnitEntityData owner)
        {
            var map=LocalMapArea.GetClosest(owner.Position);bool outdoor=map?.AreaPart!=null&&!map.AreaPart.IsIndoor&&!LocalMapArea.IsIndoor(owner.Position);
            Sprint27Check("native-outdoor-placement",outdoor&&Game.Instance.CurrentlyLoadedArea.AssetGuid=="ead426a6c23d39548a670ee515d77df4",new JObject{["area"]=Game.Instance.CurrentlyLoadedArea.AssetGuid,["part"]=map?.AreaPart?.AssetGuid,["outdoor"]=outdoor,["selection"]=Sprint22SelectionSurface(owner)},"Actual native Oleg outdoor point and area-part classification, with the original body selected normally.");
            Vector3 destination;string survey,direction;Vector3 origin=Sprint24AiCombatLane(fixture.Caster,owner,fixture.Hostile,6f,out destination,out survey,out direction);PlaceExpandedSummoningUnit(owner,origin);
            var move=new UnitMoveTo(destination,.5f);owner.Commands.Run(move);int frames=0;DateTime deadline=DateTime.UtcNow.AddSeconds(25);
            while(!move.IsFinished&&frames++<2500&&DateTime.UtcNow<deadline)yield return 0;
            Sprint27Check("outdoor-native-movement",move.IsFinished&&Vector3.Distance(origin,owner.Position)>1f&&Vector3.Distance(destination,owner.Position)<1.5f,new JObject{["finished"]=move.IsFinished,["travel"]=Vector3.Distance(origin,owner.Position),["gap"]=Vector3.Distance(destination,owner.Position),["survey"]=survey},"Original-body Griffon navigates a real outdoor route using native movement; no free altitude or collision bypass.");
            Sprint27Body(owner,"outdoor-movement");foreach(int f in Sprint27Ordinary(fixture,owner,true))yield return f;
        }
        private IEnumerable<int> Sprint27ReleasedControls(ExpandedSummoningCorrectionFixture fixture)
        {
            foreach(string key in new[]{"tiger","eagle","bulette"}){
                var control=CastExpandedSummoningOwnTier(fixture,key);SetExpandedSummoningBrainActive(control,false);var targetSize=fixture.Hostile.Descriptor.State.Size;
                if(key=="tiger")fixture.Hostile.Descriptor.State.Size=Kingmaker.Enums.Size.Huge;
                try{
                    foreach(int f in Sprint26Appearance(control,"released-"+key,false))yield return f;
                    var target=fixture.Hostile;ResetExpandedSummoningHostile(fixture);control.JoinCombat();target.JoinCombat();
                    Vector3 far;string lane,direction;Vector3 origin=Sprint24AiCombatLane(fixture.Caster,control,target,control.Corpulence+target.Corpulence+.2f,out far,out lane,out direction);
                    PlaceExpandedSummoningUnit(control,origin);PlaceExpandedSummoningUnit(target,far);control.Memory.Add(target);target.Memory.Add(control);
                    var observer=new Sprint24AttackObserver{Owner=control,Target=target};var command=new UnitAttack(target){ForceFullAttack=true};EventBus.Subscribe(observer);
                    try{
                        control.Commands.Run(command);DateTime deadline=DateTime.UtcNow.AddSeconds(30);int frame=0;
                        while(!command.IsFinished&&frame++<3000&&DateTime.UtcNow<deadline)yield return 0;
                        Sprint27Check("released-ordinary-"+key,command.IsFinished&&command.AllAttacks.Count==3&&observer.WeaponAttacks.Count==3,new JObject{["key"]=key,["targetSize"]=target.Descriptor.State.Size.ToString(),["grabExcludedBySize"]=key=="tiger"&&target.Descriptor.State.Size>control.Descriptor.State.Size,["unit"]=control.Blueprint.AssetGuid,["planned"]=command.AllAttacks.Count,["weapons"]=new JArray(observer.WeaponAttacks.Select(v=>v.Weapon.Blueprint.AssetGuid)),["forcedRolls"]=0,["forcedAnimationEvents"]=0},"Released raker, flyer and shared-plan Bulette control retain the ordinary three-weapon command; their source/assets are preserved.");
                    }finally{EventBus.Unsubscribe(observer);control.Commands.InterruptAll(true);ResetExpandedSummoningHostile(fixture);}
                    if(key=="tiger")foreach(int f in Sprint27Pounce(fixture,control,false,true))yield return f;
                }finally{control.Commands.InterruptAll(true);SummonGrappleLinks.ReleaseAll(control);fixture.Hostile.Descriptor.State.Size=targetSize;DisposeExpandedSummoningUnits(fixture.Created,new[]{control});}
            }
        }
    }
}
