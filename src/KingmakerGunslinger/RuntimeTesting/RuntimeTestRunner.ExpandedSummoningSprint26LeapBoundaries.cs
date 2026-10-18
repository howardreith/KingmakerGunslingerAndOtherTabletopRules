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
                Vector3 start;string lane;var selected=Sprint23FindChargeRoute(fixture,owner,target,data,out start,out lane,startDistances:new[]{20f,16f,12f});
                if(selected==null)throw new InvalidOperationException("No legal cancellation lane: "+lane);
                owner.JoinCombat();target.JoinCombat();var command=new UnitUseAbility(data,selected);var costs=new JArray();
                EventBus.Subscribe(observer);
                using(var seed=new Sprint26JumpSeed(owner,20))using(var economy=new FirearmReviewCosts(owner,costs))using(var queued=new Sprint26QueueTrace(owner)){
                    owner.Commands.Run(command);int frames=0;DateTime deadline=DateTime.UtcNow.AddSeconds(15);
                    while(frames++<2000&&DateTime.UtcNow<deadline&&Vector3.Distance(start,owner.Position)<.15f&&!command.IsFinished){
                        if(command.ExecutionProcess!=null&&!command.ExecutionProcess.IsEnded)command.ExecutionProcess.Tick();yield return 0;
                    }
                    bool reached=seed.Count==1&&!command.IsFinished&&Vector3.Distance(start,owner.Position)>.05f&&queued.Commands.Count==0&&observer.WeaponAttacks.Count==0;
                    Sprint25Check("leap-"+(loseTarget?"target-loss":"interruption")+"-precondition",reached,
                        new JObject{["owner"]=owner.UniqueId,["target"]=target.UniqueId,["route"]=lane,["frames"]=frames,
                            ["checkCount"]=seed.Count,["parentFinished"]=command.IsFinished,["parentResult"]=command.Result.ToString(),
                            ["travel"]=Vector3.Distance(start,owner.Position),["queuedChildren"]=queued.Commands.Count,
                            ["weaponAttacks"]=observer.WeaponAttacks.Count,["available"]=data.IsAvailable,["canAct"]=owner.Descriptor.State.CanAct,
                            ["targetDestroyed"]=target.Destroyed,["enemy"]=owner.IsEnemy(target),["costs"]=costs},
                        "A real checked charge is moving before any attack and remains interruptible; missing setup is a failed observation, never negative PASS.");
                    if(!reached)yield break;
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
            var leapAttacks=new Sprint24AttackObserver{Owner=owner,Target=target};
            var costs=new JArray();var endings=new JArray();
            var controller=new Kingmaker.Controllers.Combat.UnitCombatEngagementController();
            var tick=controller.GetType().GetMethod("TickUnit",BindingFlags.Instance|BindingFlags.NonPublic);
            try{
                foreach(int f in Sprint26Appearance(owner,"bulette"))yield return f;
                foreach(int f in Sprint26Appearance(flanker,"gorgon"))yield return f;
                foreach(int f in Sprint22SetPowerAttack(flanker,false))yield return f;
                PlaceExpandedSummoningUnit(target,Sprint25IsolatedAnchor(fixture));
                bool parked=false;Vector3 start;string route;
                var selected=Sprint23FindChargeRoute(fixture,owner,target,data,out start,out route,(actor,victim)=>{
                    Vector3 direction=(victim.Position-actor.Position).normalized,side=new Vector3(-direction.z,0,direction.x);
                    foreach(var sign in new[]{1f,-1f}){
                        Vector3 requested=actor.Position+side*sign*(actor.Corpulence+flanker.Corpulence+.3f);
                        var node=Kingmaker.View.ObstacleAnalyzer.GetNearestNode(requested);
                        if(node.node==null||!node.node.Walkable||Vector3.Distance(requested,node.clampedPosition)>.3f)continue;
                        float reach=flanker.Body.PrimaryHand.MaybeWeapon.AttackRange.Meters+flanker.Corpulence;
                        if(fixture.UnitsBefore.OfType<UnitEntityData>().Any(v=>v!=null&&Vector3.Distance(v.Position,node.clampedPosition)<reach+v.Corpulence+.5f))continue;
                        PlaceExpandedSummoningUnit(flanker,node.clampedPosition);parked=true;return true;
                    }
                    return false;
                });
                if(selected==null||tick==null||!parked)throw new InvalidOperationException("No legal isolated charge with protected actors outside native opportunity threat.");
                // Summons initially share the player-controlled group. Faction
                // switching alone does not replace native group membership.
                flanker.GroupId = "KMG_S26_Opportunity_" + flanker.UniqueId;
                flanker.Descriptor.SwitchFactions(fixture.Hostile.Faction,true);
                flanker.UpdateGroup();
                owner.JoinCombat();target.JoinCombat();flanker.JoinCombat();
                owner.Memory.Add(flanker);flanker.Memory.Add(owner);flanker.CombatState.OnNewRound();
                flanker.CombatState.AttackOfOpportunityCount=flanker.CombatState.AttackOfOpportunityPerRound;
                // This exact native environment dependency acquires/disengages
                // through its own geometry; no attack is requested by this fixture.
                Game.Instance.Player.UpdateIsInCombat();
                new Kingmaker.Controllers.Combat.UnitCombatJoinController().Tick();
                new Kingmaker.Controllers.Combat.UnitCombatPrepareController().Tick();
                int readyFrames=0;DateTime readyDeadline=DateTime.UtcNow.AddSeconds(10);
                while(!flanker.CombatState.CanActInCombat&&readyFrames++<1500&&DateTime.UtcNow<readyDeadline){
                    tick.Invoke(controller,new object[]{flanker});yield return 0;
                }
                // These flags belong to fixture placement, not the subsequent
                // real movement. Reuse the released disposable-responder setup.
                flanker.CombatState.PreventAttacksOfOpporunityNextFrame=false;
                owner.CombatState.PreventAttacksOfOpporunityNextFrame=false;
                for(int i=0;i<5;i++){tick.Invoke(controller,new object[]{flanker});yield return 0;}
                bool engaged=flanker.CombatState.EngagedUnits.Contains(owner);
                bool available=flanker.CombatState.CanAttackOfOpportunity;
                bool chargeAvailable=data.CanTarget(selected);
                int beforeSimulation=flanker.CombatState.AttackOfOpportunityCount;
                // Native simulate=true issues no command or budget debit.
                bool reactionEligible=flanker.CombatState.AttackOfOpportunity(owner,true);
                var lastTarget=flanker.CombatState.LastTarget;
                int previousTargets=flanker.CombatState.DisengageAttackTargets.Count;
                // simulate=true skips the native single-engaged-enemy branch.
                // The actual RTWP query only reads state; scoped LastTarget is
                // restored by native finally. Observe that exact production policy.
                bool disengageEligible=flanker.CombatState.ShouldAttackOnDisengage(owner,false);
                bool simulatedDisengage=flanker.CombatState.ShouldAttackOnDisengage(owner,true);
                bool simulationReadOnly=beforeSimulation==flanker.CombatState.AttackOfOpportunityCount&&ReferenceEquals(lastTarget,flanker.CombatState.LastTarget)&&previousTargets==flanker.CombatState.DisengageAttackTargets.Count;
                Func<string,JObject> state=stage=>new JObject{["stage"]=stage,["frame"]=Time.frameCount,
                    ["ownerPosition"]=SurveyVector(owner.Position),["flankerPosition"]=SurveyVector(flanker.Position),
                    ["engaged"]=flanker.CombatState.EngagedUnits.Contains(owner),["canActInCombat"]=flanker.CombatState.CanActInCombat,
                    ["waitingInitiative"]=flanker.CombatState.IsWaitingInitiative,["initiativeCooldown"]=flanker.CombatState.Cooldown.Initiative,
                    ["flankerHasMotion"]=flanker.HasMotionThisTick,["ownerPreventsReaction"]=owner.CombatState.PreventAttacksOfOpporunityNextFrame,
                    ["flankerPreventsReaction"]=flanker.CombatState.PreventAttacksOfOpporunityNextFrame,
                    ["ownerForceMoveProvoke"]=owner.Get<Kingmaker.UnitLogic.Parts.UnitPartForceMove>()?.ProvokeAttackOfOpportunity,
                    ["opportunityCount"]=flanker.CombatState.AttackOfOpportunityCount};
                var movementStates=new JArray{state("before-charge")};
                Sprint25Check("leap-opportunity-preconditions",engaged&&available&&chargeAvailable&&reactionEligible&&disengageEligible&&simulationReadOnly,
                    new JObject{["owner"]=owner.UniqueId,["flanker"]=flanker.UniqueId,["route"]=route,
                        ["engaged"]=engaged,["opportunityAvailable"]=available,["chargeAvailable"]=chargeAvailable,
                        ["nativeReactionEligible"]=reactionEligible,["nativeDisengageEligible"]=disengageEligible,["nativeSimulatedDisengage"]=simulatedDisengage,["simulationReadOnly"]=simulationReadOnly,
                        ["readinessFrames"]=readyFrames,["nativeState"]=state("ready"),
                        ["count"]=flanker.CombatState.AttackOfOpportunityCount,["canAct"]=flanker.Descriptor.State.CanAct,
                        ["sleeping"]=flanker.IsSleeping,["awakePool"]=Game.Instance.State.AwakeUnits.Contains(flanker),
                        ["enemy"]=flanker.IsEnemy(owner),["group"]=flanker.GroupId,["ownerGroup"]=owner.GroupId,["primaryWeapon"]=flanker.Body.PrimaryHand.MaybeWeapon?.Blueprint.AssetGuid,
                        ["engagedIds"]=new JArray(flanker.CombatState.EngagedUnits.Select(v=>v.UniqueId)),["protectedActorsOutsideThreat"]=parked},
                    "Real native engagement, opportunity budget and legal charge are obtained outside every protected actor before the production command.");
                if(!engaged||!available||!chargeAvailable||!reactionEligible||!disengageEligible||!simulationReadOnly)yield break;
                EventBus.Subscribe(observer);EventBus.Subscribe(leapAttacks);
                using(var seed=new Sprint26JumpSeed(owner,20))using(var queued=new Sprint26QueueTrace(owner))
                using(var economy=new FirearmReviewCosts(owner,costs))using(var trace=new Sprint26CommandTrace(owner,endings)){
                    var command=new UnitUseAbility(data,selected);owner.Commands.Run(command);
                    int minimumBudget=flanker.CombatState.AttackOfOpportunityCount;
                    int frames=0;bool movingSample=false,disengagedSample=false;DateTime deadline=DateTime.UtcNow.AddSeconds(30);
                    while(frames++<3500&&DateTime.UtcNow<deadline){
                        if(command.ExecutionProcess!=null&&!command.ExecutionProcess.IsEnded)command.ExecutionProcess.Tick();
                        tick.Invoke(controller,new object[]{flanker});minimumBudget=Math.Min(minimumBudget,flanker.CombatState.AttackOfOpportunityCount);yield return 0;
                        minimumBudget=Math.Min(minimumBudget,flanker.CombatState.AttackOfOpportunityCount);
                        if(!movingSample&&Vector3.Distance(start,owner.Position)>.1f){movementStates.Add(state("moving"));movingSample=true;}
                        if(!disengagedSample&&!flanker.CombatState.EngagedUnits.Contains(owner)){movementStates.Add(state("disengaged"));disengagedSample=true;}
                        if(command.IsFinished&&queued.Commands.Count>0&&queued.Commands.All(v=>v.IsFinished)&&observer.WeaponAttacks.Count>0)break;
                    }
                    var attacks=observer.WeaponAttacks.Where(v=>v.IsAttackOfOpportunity).ToArray();
                    var bound=queued.Commands.Count==1?BuletteLeapRuntime.Observation(queued.Commands[0]):null;
                    var completedLeapAttacks=leapAttacks.OwnerWeaponAttacks.Where(v=>!v.IsAttackOfOpportunity&&ReferenceEquals(v.Target,target)).ToArray();
                    Sprint25Check("leap-native-opportunity",engaged&&available&&attacks.Length==1&&command.IsFinished&&
                        bound!=null&&bound.Check!=null&&bound.Check.IsPassed&&queued.Commands[0].IsFinished&&
                        completedLeapAttacks.Length==4&&completedLeapAttacks.All(v=>ReferenceEquals(v.Weapon?.Blueprint,bound.Claw)),
                        new JObject{["owner"]=owner.UniqueId,["flanker"]=flanker.UniqueId,["route"]=route,["engagedBefore"]=engaged,["opportunityAvailableBefore"]=available,
                            ["start"]=SurveyVector(start),["landing"]=SurveyVector(owner.Position),["flankerPosition"]=SurveyVector(flanker.Position),
                            ["movementStates"]=movementStates,["finalNativeState"]=state("after-charge"),
                            ["parentFinished"]=command.IsFinished,["parentResult"]=command.Result.ToString(),["executionEnded"]=command.ExecutionProcess?.IsEnded,
                            ["frames"]=frames,["ownerDamage"]=owner.Descriptor.Damage,["ownerDead"]=owner.Descriptor.State.IsDead,
                            ["ownerCanAct"]=owner.Descriptor.State.CanAct,["ownerCanMove"]=owner.Descriptor.State.CanMove,
                            ["targetDead"]=target.Descriptor.State.IsDead,["targetDestroyed"]=target.Destroyed,
                            ["allAttackQueueVisits"]=queued.QueueVisits.DeepClone(),["queuedVisits"]=queued.Commands.Count,["distinctCommands"]=queued.Commands.Distinct().Count(),["minimumOpportunityBudget"]=minimumBudget,
                            ["queuedCommands"]=new JArray(queued.Commands.Select(q=>{var a=BuletteLeapRuntime.Observation(q);return new JObject{
                                ["referenceId"]=System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(q),["finished"]=q.IsFinished,["result"]=q.Result.ToString(),
                                ["target"]=q.Target?.UniqueId,["planned"]=q.AllAttacks.Count,["plannedIndex"]=(int)ReadExpandedSummoningOptionalMember(q,"m_AttackIndex"),["plannedAbsent"]=q.PlannedAttack==null,
                                ["leapBound"]=a!=null,["checkPassed"]=a?.Check?.IsPassed,["plannedClaws"]=a?.PlannedClaws,["eligible"]=a?.EligibleAtStart,["travelAtQueue"]=a?.TravelAtQueue};})),
                            ["leapArmed"]=BuletteLeapRuntime.IsArmed(owner),["nativeEnds"]=endings,["costs"]=costs,
                            ["ownerAttacks"]=new JArray(leapAttacks.OwnerWeaponAttacks.Select(v=>new JObject{["target"]=v.Target?.UniqueId,["weapon"]=v.Weapon?.Blueprint.AssetGuid,["opportunity"]=v.IsAttackOfOpportunity})),
                            ["attacks"]=observer.WeaponAttacks.Count,["nativeOpportunityAttacks"]=attacks.Length,["opportunitiesAfter"]=flanker.CombatState.AttackOfOpportunityCount,
                            ["fixtureAttackCommands"]=0,["onlyExactFlankerEngagementTick"]=true,["protectedActorsOutsideThreat"]=parked},
                        "Legal real Leap movement leaves an actual disposable enemy's native threat, provoking its native opportunity command without teleporting, suppressing opportunity rules or fixture attacks.");
                }
            }finally{EventBus.Unsubscribe(observer);EventBus.Unsubscribe(leapAttacks);UnityEngine.Random.state=random;owner.Commands.InterruptAll(true);flanker.Commands.InterruptAll(true);
                DisposeExpandedSummoningUnits(fixture.Created,new[]{owner,flanker});ResetExpandedSummoningHostile(fixture);}
        }

        private sealed class Sprint26QueueTrace:IDisposable
        {
            private static Sprint26QueueTrace active;private readonly UnitEntityData owner;
            private readonly Harmony12.HarmonyInstance harmony;private readonly string id="KMG.Sprint26.QueueTrace."+Guid.NewGuid().ToString("N");
            private readonly MethodInfo method=typeof(UnitCommands).GetMethod("AddToQueueFirst");
            internal readonly List<UnitAttack> Commands=new List<UnitAttack>();
            internal readonly JArray QueueVisits=new JArray();
            internal Sprint26QueueTrace(UnitEntityData owner){if(active!=null)throw new InvalidOperationException("Nested exact queue observation.");this.owner=owner;harmony=Harmony12.HarmonyInstance.Create(id);
                try{harmony.Patch(method,null,new Harmony12.HarmonyMethod(typeof(Sprint26QueueTrace).GetMethod("After",BindingFlags.Static|BindingFlags.NonPublic)));active=this;}catch{Dispose();throw;}}
            private static void After(UnitCommand __0){
                var value=active;var attack=__0 as UnitAttack;
                if(value==null||attack==null||!ReferenceEquals(attack.Executor,value.owner))return;
                if(attack.IsCharge)value.Commands.Add(attack);
                var owner=value.owner;var agent=owner.View.AgentASP;
                value.QueueVisits.Add(new JObject{["frame"]=Time.frameCount,["isCharge"]=attack.IsCharge,["ignoresCooldown"]=attack.IsIgnoreCooldown,
                    ["referenceId"]=System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(attack),["target"]=attack.Target?.UniqueId,
                    ["shouldApproach"]=attack.ShouldUnitApproach,["enoughClose"]=attack.IsUnitEnoughClose,["approachRadius"]=attack.ApproachRadius,
                    ["distance"]=attack.Target==null?-1f:owner.DistanceTo(attack.Target),["position"]=SurveyVector(owner.Position),["previousPosition"]=SurveyVector(owner.PreviousPosition),
                    ["pathPresent"]=agent.Path!=null,["reallyMoving"]=agent.IsReallyMoving,["nextPointIndex"]=ReadExpandedSummoningOptionalMember(agent,"m_NextPointIndex")?.ToString(),
                    ["canAct"]=owner.Descriptor.State.CanAct,["canMove"]=owner.Descriptor.State.CanMove,["leapArmed"]=BuletteLeapRuntime.IsArmed(owner),
                    ["bound"]=BuletteLeapRuntime.Observation(attack)!=null,["callers"]=new JArray(new System.Diagnostics.StackTrace().GetFrames().Skip(1).Take(8).Select(f=>f.GetMethod().DeclaringType?.FullName+"."+f.GetMethod().Name))});
            }
            public void Dispose(){harmony.Unpatch(method,Harmony12.HarmonyPatchType.All,id);if(ReferenceEquals(active,this))active=null;Commands.Clear();QueueVisits.Clear();}
        }
    }
}
