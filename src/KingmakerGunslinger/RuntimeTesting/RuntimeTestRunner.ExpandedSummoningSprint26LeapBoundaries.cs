using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> Sprint26LeapCancellation(ExpandedSummoningCorrectionFixture fixture,bool loseTarget)
        {
            var owner=CastExpandedSummoningOwnTier(fixture,"bulette");SetExpandedSummoningBrainActive(owner,false);
            BlueprintUnit targetBlueprint;var target=Sprint26BoundaryTarget(fixture,owner,out targetBlueprint);
            var leap=owner.Descriptor.Abilities.Enumerable.Single(v=>v.Blueprint.name=="KMG_Summoning_Special_Bulette_Leap");
            var data=new AbilityData(leap);var random=UnityEngine.Random.state;
            var observer=new Sprint24AttackObserver{Owner=owner,Target=target};
            try{
                foreach(int f in Sprint26Appearance(owner,"bulette"))yield return f;
                Vector3 start;string lane;var selected=Sprint23FindChargeRoute(fixture,owner,target,data,out start,out lane);
                if(selected==null)throw new InvalidOperationException("No legal cancellation lane: "+lane);
                owner.JoinCombat();target.JoinCombat();var command=new UnitUseAbility(data,selected);var costs=new JArray();
                EventBus.Subscribe(observer);
                using(var seed=new Sprint26JumpSeed(owner,20))using(var economy=new FirearmReviewCosts(owner,costs))using(var queued=new Sprint26QueueTrace(owner)){
                    owner.Commands.Run(command);int frames=0;DateTime deadline=DateTime.UtcNow.AddSeconds(15);
                    while(frames++<2000&&DateTime.UtcNow<deadline&&Vector3.Distance(start,owner.Position)<.15f&&!command.IsFinished){
                        if(command.ExecutionProcess!=null&&!command.ExecutionProcess.IsEnded)command.ExecutionProcess.Tick();yield return 0;
                    }
                    bool reached=seed.Count==1&&!command.IsFinished&&Vector3.Distance(start,owner.Position)>.05f&&queued.Commands.Count==0&&observer.WeaponAttacks.Count==0;
                    if(!reached)throw new InvalidOperationException("Cancellation precondition absent; no negative PASS.");
                    if(loseTarget)DisposeExpandedSummoningUnits(fixture.Created,new[]{target});else command.Interrupt(true);
                    deadline=DateTime.UtcNow.AddSeconds(12);
                    while(frames++<3500&&DateTime.UtcNow<deadline&&(!command.IsFinished||BuletteLeapRuntime.IsArmed(owner))){
                        if(!command.IsFinished&&command.ExecutionProcess!=null&&!command.ExecutionProcess.IsEnded)command.ExecutionProcess.Tick();yield return 0;
                    }
                    bool exact=command.IsFinished&&!BuletteLeapRuntime.IsArmed(owner)&&observer.WeaponAttacks.Count==0&&
                        economy.Count(command)==1&&economy.Exact(command,false)&&queued.Commands.All(v=>v.IsFinished)&&(!loseTarget||target.Destroyed);
                    Sprint25Check("leap-"+(loseTarget?"target-loss":"interruption"),exact,new JObject{["owner"]=owner.UniqueId,["target"]=target.UniqueId,["preconditionReached"]=reached,
                        ["travel"]=Vector3.Distance(start,owner.Position),["parentFinished"]=command.IsFinished,["parentResult"]=command.Result.ToString(),["checkCount"]=seed.Count,
                        ["nativeQueuedChildren"]=queued.Commands.Count,["attacks"]=observer.WeaponAttacks.Count,["armedAfter"]=BuletteLeapRuntime.IsArmed(owner),["costs"]=costs},
                        "Interrupting real charge movement or removing its exact disposable target aborts attacks, clears own armed state and preserves the already-spent native action.");
                }
                EventBus.Unsubscribe(observer);
                foreach(int f in Sprint26Ordinary(fixture,owner,"bulette",false))yield return f;
            }finally{EventBus.Unsubscribe(observer);UnityEngine.Random.state=random;owner.Commands.InterruptAll(true);DisposeExpandedSummoningUnits(fixture.Created,new[]{owner,target});UnityEngine.Object.Destroy(targetBlueprint);}
        }

        private IEnumerable<int> Sprint26NativeChargeControl(ExpandedSummoningCorrectionFixture fixture)
        {
            var blueprint=fixture.Blueprints.OfType<BlueprintUnit>().Single(v=>v.AssetGuid==Sprint26MagicalBeastRules.BuletteDonor);
            var owner=Game.Instance.EntityCreator.SpawnUnit(blueprint,fixture.Caster.Position,Quaternion.identity,fixture.Scene);fixture.Created.Add(owner);Game.Instance.EntityCreator.Tick();
            SetExpandedSummoningBrainActive(owner,false);owner.Descriptor.SwitchFactions(fixture.Caster.Faction,false);
            var nativeCharge=fixture.Blueprints.OfType<BlueprintAbility>().Single(v=>v.AssetGuid==Sprint26MagicalBeastRules.ChargeDonor);
            Kingmaker.Blueprints.Facts.Fact grant=null;
            if(owner.Descriptor.Abilities.GetAbility(nativeCharge)==null)grant=owner.Descriptor.AddFact(nativeCharge);
            var target=fixture.Hostile;var observer=new Sprint24AttackObserver{Owner=owner,Target=target};
            try{
                foreach(int f in Sprint26Appearance(owner,"released-native-bulette-control",false))yield return f;
                var data=new AbilityData(owner.Descriptor.Abilities.GetAbility(nativeCharge));Vector3 start;string route;
                var selected=Sprint23FindChargeRoute(fixture,owner,target,data,out start,out route);if(selected==null)throw new InvalidOperationException("Native charge control lane unavailable.");
                owner.JoinCombat();target.JoinCombat();var command=new UnitUseAbility(data,selected);EventBus.Subscribe(observer);
                var costs=new JArray();
                using(var checks=new Sprint26JumpSeed(owner,20))using(var economy=new FirearmReviewCosts(owner,costs))using(var queued=new Sprint26QueueTrace(owner)){
                    owner.Commands.Run(command);int frames=0;DateTime deadline=DateTime.UtcNow.AddSeconds(25);
                    while(frames++<3000&&DateTime.UtcNow<deadline){if(command.ExecutionProcess!=null&&!command.ExecutionProcess.IsEnded)command.ExecutionProcess.Tick();yield return 0;
                        if(command.IsFinished&&queued.Commands.Count>0&&queued.Commands.All(v=>v.IsFinished))break;}
                    bool exact=command.IsFinished&&queued.Commands.Count==1&&queued.Commands[0].IsFinished&&observer.WeaponAttacks.Count==1&&
                        queued.Commands[0].IsCharge&&BuletteLeapRuntime.Observation(queued.Commands[0])==null&&checks.Count==0&&economy.Count(command)==1&&economy.Exact(command,false);
                    Sprint25Check("released-native-charge-control",exact,new JObject{["owner"]=owner.UniqueId,["blueprint"]=owner.Blueprint.AssetGuid,["nativeAbility"]=nativeCharge.AssetGuid,
                        ["queued"]=queued.Commands.Count,["attacks"]=observer.WeaponAttacks.Count,["jumpChecks"]=checks.Count,["ownLeapObservationAbsent"]=queued.Commands.All(v=>BuletteLeapRuntime.Observation(v)==null),
                        ["travel"]=Vector3.Distance(start,owner.Position),["costs"]=costs},"The exact released native adult Bulette still executes ordinary native Charge with one attack, no Leap check or owned substitution, and its normal action debit.");
                }
            }finally{EventBus.Unsubscribe(observer);if(grant!=null)owner.Descriptor.RemoveFact(grant);owner.Commands.InterruptAll(true);DisposeExpandedSummoningUnits(fixture.Created,new[]{owner});ResetExpandedSummoningHostile(fixture);}
        }

        private void Sprint26LeapGeometryRefusals(ExpandedSummoningCorrectionFixture fixture,UnitEntityData owner,UnitEntityData target,AbilityData data)
        {
            Vector3 originalOwner=owner.Position,originalTarget=target.Position;
            try{
                var probe=new UnitAttack(target);probe.Init(owner);Vector3 direction=(owner.Position-target.Position).normalized;
                var near=Kingmaker.View.ObstacleAnalyzer.GetNearestNode(target.Position+direction*(probe.ApproachRadius+1f));
                if(near.node==null||!near.node.Walkable)throw new InvalidOperationException("No near-target native node for minimum-run-up control.");
                PlaceExpandedSummoningUnit(owner,near.clampedPosition);
                bool ownRefused=!data.CanTarget(new Kingmaker.Utility.TargetWrapper(target));
                Sprint25Check("leap-short-runup-refused",ownRefused&&!BuletteLeapChargeGeometry.HasRunup(owner,target),new JObject{["distance"]=owner.DistanceTo(target),["approach"]=probe.ApproachRadius,["ownRefused"]=ownRefused},"A too-short charge cannot acquire the special routine even if native centre-distance validation would accept it.");
                bool found=false;var attempts=new JArray();
                foreach(float distance in new[]{6f,12f,18f}){
                    foreach(Vector3 offset in CompassOffsets){
                        var point=Kingmaker.View.ObstacleAnalyzer.GetNearestNode(originalTarget+offset*distance);
                        if(point.node==null||!point.node.Walkable||Vector3.Distance(point.clampedPosition,originalTarget+offset*distance)>.6f)continue;
                        PlaceExpandedSummoningUnit(owner,point.clampedPosition);PlaceExpandedSummoningUnit(target,originalTarget);
                        if(!BuletteLeapChargeGeometry.HasRunup(owner,target))continue;
                        bool wall=Kingmaker.Visual.FogOfWar.LineOfSightGeometry.Instance.HasObstacle(owner.EyePosition,target.EyePosition,0);
                        var traced=Kingmaker.View.ObstacleAnalyzer.TraceAlongNavmesh(owner.Position,target.Position);float gap=Vector3.Distance(traced,target.Position);
                        if(!wall&&gap<1f)continue;
                        bool refused=!data.CanTarget(new Kingmaker.Utility.TargetWrapper(target));
                        attempts.Add(new JObject{["origin"]=SurveyVector(owner.Position),["target"]=SurveyVector(target.Position),["wall"]=wall,["navmeshTraceGap"]=gap,["refused"]=refused});
                        if(refused){found=true;break;}
                    }
                    if(found)break;
                }
                Sprint25Check("leap-blocked-route-refused",found,new JObject{["preconditionObtained"]=found,["candidates"]=attempts,["commandsIssued"]=0},"One measured wall or interrupted native straight navmesh route refuses Leap, with enough run-up and no attack or teleport command issued.");
            }finally{PlaceExpandedSummoningUnit(owner,originalOwner);PlaceExpandedSummoningUnit(target,originalTarget);}
        }

        private IEnumerable<int> Sprint26LeapOpportunity(ExpandedSummoningCorrectionFixture fixture)
        {
            var owner=CastExpandedSummoningOwnTier(fixture,"bulette");SetExpandedSummoningBrainActive(owner,false);
            var flanker=CastExpandedSummoningOwnTier(fixture,"gorgon");SetExpandedSummoningBrainActive(flanker,false);
            var target=fixture.Hostile;var data=new AbilityData(owner.Descriptor.Abilities.Enumerable.Single(v=>v.Blueprint.name=="KMG_Summoning_Special_Bulette_Leap"));
            var observer=new Sprint24AttackObserver{Owner=flanker,Target=owner};var random=UnityEngine.Random.state;
            var controller=new Kingmaker.Controllers.Combat.UnitCombatEngagementController();
            var tick=controller.GetType().GetMethod("TickUnit",BindingFlags.Instance|BindingFlags.NonPublic);
            try{
                foreach(int f in Sprint26Appearance(owner,"bulette"))yield return f;
                foreach(int f in Sprint26Appearance(flanker,"gorgon"))yield return f;
                foreach(int f in Sprint22SetPowerAttack(flanker,false))yield return f;
                Vector3 start;string route;var selected=Sprint23FindChargeRoute(fixture,owner,target,data,out start,out route);
                if(selected==null||tick==null)throw new InvalidOperationException("No exact native opportunity control prerequisites.");
                Vector3 forward=(target.Position-start).normalized,side=new Vector3(-forward.z,0,forward.x);
                bool parked=false;
                foreach(var sign in new[]{1f,-1f}){
                    Vector3 requested=start+side*sign*(owner.Corpulence+flanker.Corpulence+.3f);
                    var node=Kingmaker.View.ObstacleAnalyzer.GetNearestNode(requested);
                    if(node.node==null||!node.node.Walkable||Vector3.Distance(requested,node.clampedPosition)>.3f)continue;
                    float reach=flanker.Body.PrimaryHand.MaybeWeapon.AttackRange.Meters+flanker.Corpulence;
                    if(fixture.UnitsBefore.OfType<UnitEntityData>().Any(v=>v!=null&&Vector3.Distance(v.Position,node.clampedPosition)<reach+v.Corpulence+.5f))continue;
                    PlaceExpandedSummoningUnit(flanker,node.clampedPosition);parked=true;break;
                }
                if(!parked)throw new InvalidOperationException("No native threat position clear of every protected actor.");
                flanker.Descriptor.SwitchFactions(fixture.Hostile.Faction,false);
                owner.JoinCombat();target.JoinCombat();flanker.JoinCombat();
                owner.Memory.Add(flanker);flanker.Memory.Add(owner);flanker.CombatState.OnNewRound();
                flanker.CombatState.AttackOfOpportunityCount=flanker.CombatState.AttackOfOpportunityPerRound;
                // This exact native environment dependency acquires/disengages
                // through its own geometry; no attack is requested by this fixture.
                for(int i=0;i<25;i++){tick.Invoke(controller,new object[]{flanker});yield return 0;}
                bool engaged=flanker.CombatState.EngagedUnits.Contains(owner);
                bool available=flanker.CombatState.CanAttackOfOpportunity;
                if(!engaged||!available||!data.CanTarget(selected))throw new InvalidOperationException("Legal charge/opportunity setup not obtained; no PASS.");
                EventBus.Subscribe(observer);
                using(var seed=new Sprint26JumpSeed(owner,20))using(var queued=new Sprint26QueueTrace(owner)){
                    var command=new UnitUseAbility(data,selected);owner.Commands.Run(command);
                    int frames=0;DateTime deadline=DateTime.UtcNow.AddSeconds(30);
                    while(frames++<3500&&DateTime.UtcNow<deadline){
                        if(command.ExecutionProcess!=null&&!command.ExecutionProcess.IsEnded)command.ExecutionProcess.Tick();
                        tick.Invoke(controller,new object[]{flanker});yield return 0;
                        if(command.IsFinished&&queued.Commands.Count>0&&queued.Commands.All(v=>v.IsFinished)&&observer.WeaponAttacks.Count>0)break;
                    }
                    var attacks=observer.WeaponAttacks.Where(v=>v.IsAttackOfOpportunity).ToArray();
                    Sprint25Check("leap-native-opportunity",engaged&&available&&attacks.Length==1&&command.IsFinished&&
                        queued.Commands.Count==1&&BuletteLeapRuntime.Observation(queued.Commands[0])!=null,
                        new JObject{["owner"]=owner.UniqueId,["flanker"]=flanker.UniqueId,["route"]=route,["engagedBefore"]=engaged,["opportunityAvailableBefore"]=available,
                            ["start"]=SurveyVector(start),["landing"]=SurveyVector(owner.Position),["flankerPosition"]=SurveyVector(flanker.Position),
                            ["attacks"]=observer.WeaponAttacks.Count,["nativeOpportunityAttacks"]=attacks.Length,["opportunitiesAfter"]=flanker.CombatState.AttackOfOpportunityCount,
                            ["fixtureAttackCommands"]=0,["onlyExactFlankerEngagementTick"]=true,["protectedActorsOutsideThreat"]=parked},
                        "Legal real Leap movement leaves an actual disposable enemy's native threat, provoking its native opportunity command without teleporting, suppressing opportunity rules or fixture attacks.");
                }
            }finally{EventBus.Unsubscribe(observer);UnityEngine.Random.state=random;owner.Commands.InterruptAll(true);flanker.Commands.InterruptAll(true);
                DisposeExpandedSummoningUnits(fixture.Created,new[]{owner,flanker});ResetExpandedSummoningHostile(fixture);}
        }

        private sealed class Sprint26QueueTrace:IDisposable
        {
            private static Sprint26QueueTrace active;private readonly UnitEntityData owner;
            private readonly Harmony12.HarmonyInstance harmony;private readonly string id="KMG.Sprint26.QueueTrace."+Guid.NewGuid().ToString("N");
            private readonly MethodInfo method=typeof(UnitCommands).GetMethod("AddToQueueFirst");
            internal readonly List<UnitAttack> Commands=new List<UnitAttack>();
            internal Sprint26QueueTrace(UnitEntityData owner){if(active!=null)throw new InvalidOperationException("Nested exact queue observation.");this.owner=owner;harmony=Harmony12.HarmonyInstance.Create(id);
                try{harmony.Patch(method,null,new Harmony12.HarmonyMethod(typeof(Sprint26QueueTrace).GetMethod("After",BindingFlags.Static|BindingFlags.NonPublic)));active=this;}catch{Dispose();throw;}}
            private static void After(UnitCommand __0){var value=active;var attack=__0 as UnitAttack;if(value!=null&&attack!=null&&ReferenceEquals(attack.Executor,value.owner)&&attack.IsCharge)value.Commands.Add(attack);}
            public void Dispose(){harmony.Unpatch(method,Harmony12.HarmonyPatchType.All,id);if(ReferenceEquals(active,this))active=null;Commands.Clear();}
        }
    }
}
