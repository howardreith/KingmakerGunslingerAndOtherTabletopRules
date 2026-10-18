using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Newtonsoft.Json.Linq;
using UnityEngine;
namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> Sprint27Ai(ExpandedSummoningCorrectionFixture fixture,UnitEntityData owner)
        {
            ResetExpandedSummoningHostile(fixture);var target=fixture.Hostile;
            owner.Commands.InterruptAll(true);target.Commands.InterruptAll(true);
            owner.Commands.RemoveFinishedAndUpdateQueue();target.Commands.RemoveFinishedAndUpdateQueue();
            owner.JoinCombat();target.JoinCombat();Vector3 origin;string lane;
            var nativeCharge=owner.Descriptor.Abilities.GetAbility(fixture.Blueprints.OfType<BlueprintAbility>().Single(v=>v.AssetGuid=="c78506dd0e14f7c45a599990e4e65038"));
            if(nativeCharge==null)throw new InvalidOperationException("Native route-survey carrier missing.");
            var surveyed=Sprint23FindChargeRoute(fixture,owner,target,new AbilityData(nativeCharge),out origin,out lane,startDistances:new[]{8f,10f});
            if(surveyed==null)throw new InvalidOperationException("No straight native Griffon approach lane: "+lane);
            Sprint27Check("ai-native-lane",true,new JObject{["route"]=lane,["origin"]=SurveyVector(origin),["target"]=SurveyVector(target.Position),["nativeTraceGap"]=Vector3.Distance(Kingmaker.View.ObstacleAnalyzer.TraceAlongNavmesh(origin,target.Position),target.Position),["chargeCommandsIssued"]=0},"The native charge predicate only surveys a straight legal route; the unchanged brain independently chooses its own ordinary attack and path.");
            foreach(var unit in new[]{owner,target}){unit.Wake(0f);if(!Game.Instance.State.AwakeUnits.Contains(unit))Game.Instance.State.AwakeUnits.Add(unit);}
            owner.Memory.Add(target);target.Memory.Add(owner);owner.SightCache.ClearIfNeeded();target.SightCache.ClearIfNeeded();
            new Kingmaker.Controllers.Units.UnitMemoryController().Tick();
            SetExpandedSummoningBrainActive(owner,true);owner.Brain.RestoreAvailableActions();
            owner.JoinCombat();target.JoinCombat();Game.Instance.Player.UpdateIsInCombat();
            new Kingmaker.Controllers.Combat.UnitCombatJoinController().Tick();
            new Kingmaker.Controllers.Combat.UnitCombatPrepareController().Tick();
            int runtimeActions=owner.Brain.Actions==null?0:owner.Brain.Actions.Count,availableActions=owner.Brain.AvailableActions==null?0:owner.Brain.AvailableActions.Count;
            bool ready=runtimeActions>0&&availableActions>0&&owner.IsAIEnabled&&owner.IsInCombat&&owner.Descriptor.State.CanAct&&owner.IsEnemy(target)&&owner.Memory.ContainsVisible(target);
            Sprint27Check("ai-native-readiness",ready,new JObject{["owner"]=owner.UniqueId,["brain"]=owner.Blueprint.Brain.name,["runtimeActions"]=runtimeActions,["availableActions"]=availableActions,["brainActive"]=owner.IsBrainActive,["aiEnabled"]=owner.IsAIEnabled,["combat"]=owner.IsInCombat,["canAct"]=owner.Descriptor.State.CanAct,["enemy"]=owner.IsEnemy(target),["visible"]=owner.Memory.ContainsVisible(target),["los"]=owner.HasLOS(target),["nextCommandTime"]=owner.CombatState.AIData.NextCommandTime,["unityTime"]=Time.time},"The fresh native action list is preserved; the fixture does not clear then pretend to restore it. Exact actor, visibility, combat and actual native actions are prerequisites.");
            if(!ready)throw new InvalidOperationException("Native Griffon AI prerequisites missing.");
            var observer=new Sprint24AttackObserver{Owner=owner,Target=target};var commands=new HashSet<UnitCommand>();var ambient=new HashSet<string>();
            int ticks=0;float travel=0;var movement=new JArray();EventBus.Subscribe(observer);
            try{
                DateTime deadline=DateTime.UtcNow.AddSeconds(35);
                for(int frame=0;frame<3000&&DateTime.UtcNow<deadline;frame++){
                    if(frame==0||frame%60==0&&!owner.Commands.Raw.Any(v=>v!=null&&!v.IsFinished)){
                        foreach(string id in Sprint24TickExactAi(owner))if(id!=owner.UniqueId)ambient.Add(id);ticks++;
                    }
                    foreach(var command in owner.Commands.Raw.Where(v=>v!=null))commands.Add(command);
                    travel=Math.Max(travel,Vector3.Distance(origin,owner.Position));
                    if(frame%60==0 || observer.WeaponAttacks.Count>0)movement.Add(new JObject{["frame"]=Time.frameCount,["position"]=SurveyVector(owner.Position),["targetPosition"]=SurveyVector(target.Position),["canMove"]=owner.Descriptor.State.CanMove,["holdState"]=owner.HoldState,["commandsPreventMovement"]=owner.View.IsCommandsPreventMovement,["animationPreventsMovement"]=owner.View.AnimationManager.IsPreventingMovement,["reallyMoving"]=owner.View.MovementAgent.IsReallyMoving,["pathPresent"]=owner.View.AgentASP.Path!=null,["velocity"]=owner.View.MovementAgent.Velocity.magnitude,["commands"]=new JArray(owner.Commands.Raw.Where(v=>v!=null).Select(Sprint27AiCommand))});
                    if(observer.WeaponAttacks.Count>0)break;Game.Instance.IsPaused=false;yield return 0;
                }
                bool selected=commands.OfType<UnitAttack>().Any(v=>v.AiAction!=null&&ReferenceEquals(v.TargetUnit,target));
                Sprint27Check("ai-native-approach-attack",selected&&ticks>0&&travel>.1f&&observer.WeaponAttacks.Count>0,
                    new JObject{["owner"]=owner.UniqueId,["brain"]=owner.Blueprint.Brain.name,["target"]=target.UniqueId,["lane"]=lane,["ticks"]=ticks,["travel"]=travel,
                    ["ambientCandidatesNeverTicked"]=new JArray(ambient),["commands"]=new JArray(commands.Select(Sprint27AiCommand)),["nativeMovement"]=movement,
                    ["weapons"]=new JArray(observer.WeaponAttacks.Select(v=>v.Weapon.Blueprint.AssetGuid)),["fixtureAttackCommands"]=0,["forcedRolls"]=0},
                    "The exact disposable owner's unchanged native brain chooses an attack, approaches and produces a real weapon event; protected brains are never ticked and no attack or roll is supplied.");
                Sprint27Body(owner,"ai");
            }finally{EventBus.Unsubscribe(observer);SetExpandedSummoningBrainActive(owner,false);owner.Commands.InterruptAll(true);ResetExpandedSummoningHostile(fixture);}
        }
        private static JObject Sprint27AiCommand(UnitCommand command)
        {
            return new JObject{["type"]=command.GetType().Name,["aiAction"]=command.AiAction?.name,["target"]=command.TargetUnit?.UniqueId,["started"]=command.IsStarted,["acted"]=command.IsActed,["finished"]=command.IsFinished,["result"]=command.Result.ToString(),["canStart"]=command.CanStart,["shouldApproach"]=command.ShouldUnitApproach,["enoughClose"]=command.IsUnitEnoughClose,["approachRadius"]=command.ApproachRadius,["nextApproachTime"]=ReadExpandedSummoningOptionalMember(command,"NextApproachTime")?.ToString()};
        }
        private IEnumerable<int> Sprint27CancelCharge(ExpandedSummoningCorrectionFixture fixture,UnitEntityData owner,bool loseTarget,string removeOwner=null,bool turnBased=false)
        {
            if(turnBased){Kingmaker.UI.SettingsUI.SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue=false;Game.Instance.TurnBasedCombatController.Activate();}
            BlueprintUnit targetBlueprint;var target=Sprint26BoundaryTarget(fixture,owner,out targetBlueprint);
            var charge=owner.Descriptor.Abilities.GetAbility(fixture.Blueprints.OfType<BlueprintAbility>().Single(v=>v.AssetGuid=="c78506dd0e14f7c45a599990e4e65038"));
            if(charge==null)throw new InvalidOperationException("Canonical native Charge ability absent.");
            var data=new AbilityData(charge);var observer=new Sprint24AttackObserver{Owner=owner,Target=target};
            try{
                Vector3 start;string lane;var selected=Sprint23FindChargeRoute(fixture,owner,target,data,out start,out lane,startDistances:new[]{20f,16f,12f});
                if(selected==null)throw new InvalidOperationException("No legal Griffon cancellation lane: "+lane);
                owner.JoinCombat();target.JoinCombat();
                if(turnBased){
                    var admission=new JObject();foreach(int f in Sprint22EnrollTurnBasedOwners(new[]{owner},target,admission))yield return f;
                    int waited=0;while(!Sprint18OwnerIsActing(owner)&&waited++<500)yield return 0;
                    admission["acting"]=Sprint18OwnerIsActing(owner);admission["nativeTurnWaitFrames"]=waited;
                    bool admitted=(bool)admission["admitted"]&&Sprint18OwnerIsActing(owner);
                    Sprint27Check("boundary-turn-admission-"+loseTarget,admitted,admission,"A real exact-owner active turn precedes a native turn-based abort or target-loss command.");
                    if(!admitted)throw new InvalidOperationException("Exact boundary turn absent.");
                }
                var command=new UnitUseAbility(data,selected);var costs=new JArray();JObject expiry=null;EventBus.Subscribe(observer);
                using(var economy=new FirearmReviewCosts(owner,costs))using(var queued=new Sprint26QueueTrace(owner)){
                    owner.Commands.Run(command);int frames=0;DateTime deadline=DateTime.UtcNow.AddSeconds(15);
                    while(frames++<2000&&DateTime.UtcNow<deadline&&Vector3.Distance(start,owner.Position)<.15f&&!command.IsFinished){
                        if(command.ExecutionProcess!=null&&!command.ExecutionProcess.IsEnded)command.ExecutionProcess.Tick();yield return 0;
                    }
                    bool ready=!command.IsFinished&&Vector3.Distance(start,owner.Position)>.05f&&queued.Commands.Count==0&&observer.WeaponAttacks.Count==0;
                    Sprint27Check("charge-cancel-precondition-"+loseTarget,ready,new JObject{["route"]=lane,["travel"]=Vector3.Distance(start,owner.Position),["queued"]=queued.Commands.Count,["attacks"]=observer.WeaponAttacks.Count},"Real legal charge is moving before any attack; unmet setup is failure, never a negative PASS.");
                    if(!ready)yield break;
                    if(removeOwner=="death")Kingmaker.Designers.GameHelper.KillUnit(owner,target);
                    else if(removeOwner=="expiry"){
                        var marker=owner.Buffs.Enumerable.Single(v=>ReferenceEquals(v.Blueprint,Kingmaker.Blueprints.Root.BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff));
                        expiry=ExpireSprint16OwnedBuff(owner,marker);
                    }else if(loseTarget)DisposeExpandedSummoningUnits(fixture.Created,new[]{target});else command.Interrupt(true);
                    deadline=DateTime.UtcNow.AddSeconds(12);
                    while(frames++<3500&&DateTime.UtcNow<deadline&&(!command.IsFinished || removeOwner=="expiry"&&!owner.Destroyed)){if(command.ExecutionProcess!=null&&!command.ExecutionProcess.IsEnded)command.ExecutionProcess.Tick();yield return 0;}
                    for(int f=0;f<20;f++)yield return 0;
                    bool exact=command.IsFinished&&observer.WeaponAttacks.Count==0&&economy.Count(command)==1&&economy.Exact(command,turnBased)&&queued.Commands.All(v=>v.IsFinished)&&(!loseTarget||target.Destroyed)&&(removeOwner==null || (removeOwner=="death"?owner.Descriptor.State.IsDead:owner.Destroyed)&&!owner.Commands.Raw.Any(v=>v!=null&&!v.IsFinished));
                    Sprint27Check("charge-"+(removeOwner??(loseTarget?"target-loss":"interruption")),exact,new JObject{["owner"]=owner.UniqueId,["target"]=target.UniqueId,["finished"]=command.IsFinished,["result"]=command.Result.ToString(),["queued"]=queued.Commands.Count,["attacks"]=observer.WeaponAttacks.Count,["costs"]=costs,["turnBased"]=turnBased,["ownerRemoval"]=removeOwner,["nativeExpiry"]=expiry,["ownerDead"]=owner.Descriptor.State.IsDead,["ownerDestroyed"]=owner.Destroyed,["activeOwnerCommands"]=owner.Commands.Raw.Count(v=>v!=null&&!v.IsFinished)},"Native interruption or target/owner removal aborts attacks while preserving the native action debit; no custom persistent charge state exists.");
                }
            }finally{EventBus.Unsubscribe(observer);owner.Commands.InterruptAll(true);DisposeExpandedSummoningUnits(fixture.Created,new[]{target});UnityEngine.Object.Destroy(targetBlueprint);}
            if(turnBased){Kingmaker.UI.SettingsUI.SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue=false;Game.Instance.TurnBasedCombatController.Activate();}
            if(removeOwner==null)foreach(int f in Sprint27Ordinary(fixture,owner,true))yield return f;
        }
        private IEnumerable<int> Sprint27Opportunity(ExpandedSummoningCorrectionFixture fixture,UnitEntityData owner)
        {
            var target=fixture.Hostile;ResetExpandedSummoningHostile(fixture);owner.Commands.InterruptAll(true);
            owner.JoinCombat();target.JoinCombat();Vector3 far;string lane,direction;
            Vector3 origin=Sprint24AiCombatLane(fixture.Caster,owner,target,owner.Corpulence+target.Corpulence+.8f,out far,out lane,out direction);
            PlaceExpandedSummoningUnit(owner,origin);PlaceExpandedSummoningUnit(target,far);owner.Memory.Add(target);target.Memory.Add(owner);
            owner.CombatState.OnNewRound();owner.CombatState.PreventAttacksOfOpporunityNextFrame=false;target.CombatState.PreventAttacksOfOpporunityNextFrame=false;
            for(int f=0;f<8;f++)yield return 0;
            var observer=new Sprint24AttackObserver{Owner=owner,Target=target};EventBus.Subscribe(observer);
            try{
                bool eligible=owner.CombatState.AttackOfOpportunity(target,true);
                bool queued=eligible&&owner.CombatState.AttackOfOpportunity(target,false);
                var commands=owner.Commands.Raw.OfType<UnitAttackOfOpportunity>().ToArray();
                DateTime deadline=DateTime.UtcNow.AddSeconds(20);int frames=0;
                while(frames++<2000&&DateTime.UtcNow<deadline&&commands.Any(v=>!v.IsFinished))yield return 0;
                for(int f=0;f<12;f++)yield return 0;
                bool exact=eligible&&queued&&commands.Length==1&&commands[0].IsFinished&&observer.WeaponAttacks.Count==1&&observer.WeaponAttacks[0].IsAttackOfOpportunity&&!observer.WeaponAttacks[0].IsCharge&&ReferenceEquals(observer.WeaponAttacks[0].Weapon,owner.Body.PrimaryHand.MaybeWeapon);
                Sprint27Check("native-opportunity-command",exact,new JObject{["owner"]=owner.UniqueId,["target"]=target.UniqueId,["eligible"]=eligible,["queued"]=queued,["commands"]=commands.Length,["attacks"]=observer.WeaponAttacks.Count,["weapons"]=new JArray(observer.WeaponAttacks.Select(v=>v.Weapon.Blueprint.AssetGuid)),["nativeReactionApi"]=true,["fixtureRuleAttacks"]=0,["forcedRolls"]=0},"The native reaction API queues and executes one legal bite opportunity command without pounce, rake, supplied weapon rules or a substituted roll.");
            }finally{EventBus.Unsubscribe(observer);owner.Commands.InterruptAll(true);ResetExpandedSummoningHostile(fixture);}
        }
        private void Sprint27PlanReentry(ExpandedSummoningCorrectionFixture fixture,UnitEntityData owner)
        {
            var create=typeof(UnitAttack).GetMethod("CreateFullAttack",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic);
            if(create==null||create.GetParameters().Length!=0)throw new InvalidOperationException("Native full attack planning contract differs.");
            var charge=new UnitAttack(fixture.Hostile){ForceFullAttack=true,IsCharge=true};charge.Init(owner);
            var ordinary=new UnitAttack(fixture.Hostile){ForceFullAttack=true};ordinary.Init(owner);
            var rows=new JArray();bool exact=true;
            for(int attempt=0;attempt<3;attempt++)foreach(var command in new[]{charge,ordinary}){
                var list=(System.Collections.IEnumerable)create.Invoke(command,null);var attacks=list.Cast<AttackHandInfo>().ToArray();
                exact&=attacks.Length==(command.IsCharge?5:3)&&attacks.Select(v=>v.Hand).Distinct().Count()==attacks.Length;
                rows.Add(new JObject{["attempt"]=attempt,["charge"]=command.IsCharge,["count"]=attacks.Length,["weapons"]=new JArray(attacks.Select(v=>v.Weapon.Blueprint.AssetGuid))});
            }
            Sprint27Check("native-plan-reentry",exact,new JObject{["rows"]=rows,["scope"]="bookkeeping only; commands initialized but never queued; charge flag supplied for plan probing",["attacksIssued"]=0},"Repeated native plan construction cannot accumulate extra rake limbs or mutate another command. This is supplemental bookkeeping, separate from genuine native charge-command proof.");
        }    }
}
