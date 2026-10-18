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
            owner.JoinCombat();target.JoinCombat();Vector3 far;string lane,direction;
            Vector3 origin=Sprint24AiCombatLane(fixture.Caster,owner,target,8f,out far,out lane,out direction);
            PlaceExpandedSummoningUnit(owner,origin);PlaceExpandedSummoningUnit(target,far);
            foreach(var unit in new[]{owner,target}){unit.Wake(0f);if(!Game.Instance.State.AwakeUnits.Contains(unit))Game.Instance.State.AwakeUnits.Add(unit);}
            owner.Memory.Add(target);target.Memory.Add(owner);owner.SightCache.ClearIfNeeded();target.SightCache.ClearIfNeeded();
            new Kingmaker.Controllers.Units.UnitMemoryController().Tick();
            SetExpandedSummoningBrainActive(owner,true);owner.Brain.RestoreAvailableActions();
            owner.JoinCombat();target.JoinCombat();Game.Instance.Player.UpdateIsInCombat();
            new Kingmaker.Controllers.Combat.UnitCombatJoinController().Tick();
            new Kingmaker.Controllers.Combat.UnitCombatPrepareController().Tick();
            var observer=new Sprint24AttackObserver{Owner=owner,Target=target};var commands=new HashSet<UnitCommand>();var ambient=new HashSet<string>();
            int ticks=0;float travel=0;EventBus.Subscribe(observer);
            try{
                DateTime deadline=DateTime.UtcNow.AddSeconds(35);
                for(int frame=0;frame<3000&&DateTime.UtcNow<deadline;frame++){
                    if(frame==0||frame%60==0&&!owner.Commands.Raw.Any(v=>v!=null&&!v.IsFinished)){
                        foreach(string id in Sprint24TickExactAi(owner))ambient.Add(id);ticks++;
                    }
                    foreach(var command in owner.Commands.Raw.Where(v=>v!=null))commands.Add(command);
                    travel=Math.Max(travel,Vector3.Distance(origin,owner.Position));
                    if(observer.WeaponAttacks.Count>0)break;Game.Instance.IsPaused=false;yield return 0;
                }
                bool selected=commands.OfType<UnitAttack>().Any(v=>v.AiAction!=null&&ReferenceEquals(v.TargetUnit,target));
                Sprint25Check("ai-native-approach-attack",selected&&ticks>0&&travel>.1f&&observer.WeaponAttacks.Count>0,
                    new JObject{["owner"]=owner.UniqueId,["brain"]=owner.Blueprint.Brain.name,["target"]=target.UniqueId,["lane"]=lane,["ticks"]=ticks,["travel"]=travel,
                    ["ambientCandidatesNeverTicked"]=new JArray(ambient),["commands"]=new JArray(commands.Select(v=>new JObject{["type"]=v.GetType().Name,["aiAction"]=v.AiAction?.name})),
                    ["weapons"]=new JArray(observer.WeaponAttacks.Select(v=>v.Weapon.Blueprint.AssetGuid)),["fixtureAttackCommands"]=0,["forcedRolls"]=0},
                    "The exact disposable owner's unchanged native brain chooses an attack, approaches and produces a real weapon event; protected brains are never ticked and no attack or roll is supplied.");
                Sprint27Body(owner,"ai");
            }finally{EventBus.Unsubscribe(observer);SetExpandedSummoningBrainActive(owner,false);owner.Commands.InterruptAll(true);ResetExpandedSummoningHostile(fixture);}
        }
        private IEnumerable<int> Sprint27CancelCharge(ExpandedSummoningCorrectionFixture fixture,UnitEntityData owner,bool loseTarget)
        {
            BlueprintUnit targetBlueprint;var target=Sprint26BoundaryTarget(fixture,owner,out targetBlueprint);
            var charge=owner.Descriptor.Abilities.Enumerable.Single(v=>v.Blueprint.AssetGuid=="c78506dd0e14f7c45a599990e4e65038");
            var data=new AbilityData(charge);var observer=new Sprint24AttackObserver{Owner=owner,Target=target};
            try{
                Vector3 start;string lane;var selected=Sprint23FindChargeRoute(fixture,owner,target,data,out start,out lane,startDistances:new[]{20f,16f,12f});
                if(selected==null)throw new InvalidOperationException("No legal Griffon cancellation lane: "+lane);
                owner.JoinCombat();target.JoinCombat();var command=new UnitUseAbility(data,selected);var costs=new JArray();EventBus.Subscribe(observer);
                using(var economy=new FirearmReviewCosts(owner,costs))using(var queued=new Sprint26QueueTrace(owner)){
                    owner.Commands.Run(command);int frames=0;DateTime deadline=DateTime.UtcNow.AddSeconds(15);
                    while(frames++<2000&&DateTime.UtcNow<deadline&&Vector3.Distance(start,owner.Position)<.15f&&!command.IsFinished){
                        if(command.ExecutionProcess!=null&&!command.ExecutionProcess.IsEnded)command.ExecutionProcess.Tick();yield return 0;
                    }
                    bool ready=!command.IsFinished&&Vector3.Distance(start,owner.Position)>.05f&&queued.Commands.Count==0&&observer.WeaponAttacks.Count==0;
                    Sprint25Check("charge-cancel-precondition-"+loseTarget,ready,new JObject{["route"]=lane,["travel"]=Vector3.Distance(start,owner.Position),["queued"]=queued.Commands.Count,["attacks"]=observer.WeaponAttacks.Count},"Real legal charge is moving before any attack; unmet setup is failure, never a negative PASS.");
                    if(!ready)yield break;
                    if(loseTarget)DisposeExpandedSummoningUnits(fixture.Created,new[]{target});else command.Interrupt(true);
                    deadline=DateTime.UtcNow.AddSeconds(12);
                    while(frames++<3500&&DateTime.UtcNow<deadline&&!command.IsFinished){if(command.ExecutionProcess!=null&&!command.ExecutionProcess.IsEnded)command.ExecutionProcess.Tick();yield return 0;}
                    for(int f=0;f<20;f++)yield return 0;
                    bool exact=command.IsFinished&&observer.WeaponAttacks.Count==0&&economy.Count(command)==1&&economy.Exact(command,false)&&queued.Commands.All(v=>v.IsFinished)&&(!loseTarget||target.Destroyed);
                    Sprint25Check("charge-"+(loseTarget?"target-loss":"interruption"),exact,new JObject{["owner"]=owner.UniqueId,["target"]=target.UniqueId,["finished"]=command.IsFinished,["result"]=command.Result.ToString(),["queued"]=queued.Commands.Count,["attacks"]=observer.WeaponAttacks.Count,["costs"]=costs},"Native interruption or target removal aborts attacks while preserving the native action debit; no custom persistent charge state exists.");
                }
            }finally{EventBus.Unsubscribe(observer);owner.Commands.InterruptAll(true);DisposeExpandedSummoningUnits(fixture.Created,new[]{target});UnityEngine.Object.Destroy(targetBlueprint);}
            foreach(int f in Sprint27Ordinary(fixture,owner,true))yield return f;
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
                Sprint25Check("native-opportunity-command",exact,new JObject{["owner"]=owner.UniqueId,["target"]=target.UniqueId,["eligible"]=eligible,["queued"]=queued,["commands"]=commands.Length,["attacks"]=observer.WeaponAttacks.Count,["weapons"]=new JArray(observer.WeaponAttacks.Select(v=>v.Weapon.Blueprint.AssetGuid)),["nativeReactionApi"]=true,["fixtureRuleAttacks"]=0,["forcedRolls"]=0},"The native reaction API queues and executes one legal bite opportunity command without pounce, rake, supplied weapon rules or a substituted roll.");
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
            Sprint25Check("native-plan-reentry",exact,new JObject{["rows"]=rows,["scope"]="bookkeeping only; commands initialized but never queued; charge flag supplied for plan probing",["attacksIssued"]=0},"Repeated native plan construction cannot accumulate extra rake limbs or mutate another command. This is supplemental bookkeeping, separate from genuine native charge-command proof.");
        }    }
}
