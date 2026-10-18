using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private UnitEntityData Sprint26BoundaryTarget(ExpandedSummoningCorrectionFixture fixture,UnitEntityData owner,out BlueprintUnit blueprint)
        {
            blueprint=UnityEngine.Object.Instantiate(fixture.Hostile.Blueprint);
            blueprint.name="KMG_Sprint26_PrivateBoundaryTarget";
            var target=Game.Instance.EntityCreator.SpawnUnit(blueprint,owner.Position+Vector3.forward,Quaternion.identity,fixture.Scene);
            Game.Instance.EntityCreator.Tick();
            if(target==null||!owner.IsEnemy(target)){
                if(target!=null)target.Dispose();UnityEngine.Object.Destroy(blueprint);
                throw new InvalidOperationException("Exact private hostile boundary clone unavailable.");
            }
            fixture.Created.Add(target);target.Stats.HitPoints.BaseValue=100000;
            SetExpandedSummoningBrainActive(target,false);PlaceExpandedSummoningUnit(target,target.Position);
            if(!Game.Instance.State.AwakeUnits.Contains(target))Game.Instance.State.AwakeUnits.Add(target);
            return target;
        }

        private IEnumerable<int> Sprint26GorgonCastBoundaries(ExpandedSummoningCorrectionFixture fixture)
        {
            foreach(bool immune in new[]{false,true}){
                var owner=CastExpandedSummoningOwnTier(fixture,"gorgon");SetExpandedSummoningBrainActive(owner,false);
                BlueprintUnit targetBlueprint;var target=Sprint26BoundaryTarget(fixture,owner,out targetBlueprint);
                var breath=owner.Descriptor.Abilities.Enumerable.Single(v=>v.Blueprint.name=="KMG_Summoning_Special_Gorgon_Breath").Blueprint;
                var immunity=fixture.Blueprints.OfType<BlueprintFeature>().Single(v=>v.AssetGuid=="b625283fc6eb72c47a2fc5e2a3ff9eb4");
                var casterBefore=fixture.Caster.Position;var random=UnityEngine.Random.state;
                Kingmaker.Blueprints.Facts.Fact grant=null;
                try{
                    foreach(int f in Sprint26Appearance(owner,"gorgon"))yield return f;
                    if(immune)grant=target.Descriptor.AddFact(immunity);
                    target.Stats.SaveFortitude.BaseValue=100;
                    Vector3 origin,forward;JObject lane;
                    if(!Sprint26FindBreathLane(fixture,owner,null,target,breath,out origin,out forward,out lane))throw new InvalidOperationException("Boundary cone setup unavailable: "+lane);
                    if(!immune)Sprint26ConeGeometry(owner,target,breath);
                    owner.JoinCombat();target.JoinCombat();
                    using(var saves=new Sprint26Saves(target)){
                        saves.DesiredRoll=20;
                        foreach(int f in Sprint26Cast(owner,breath,new Kingmaker.Utility.TargetWrapper(target.Position),v=>{}))yield return f;
                        bool exact=!target.Descriptor.State.IsDead&&target.Descriptor.Damage==0&&
                            !target.Descriptor.State.HasCondition(UnitCondition.Petrified)&&
                            !target.Buffs.Enumerable.Any(v=>v.Blueprint.AssetGuid==Sprint26MagicalBeastRules.PetrificationGuid)&&
                            GorgonBreathRuntime.Latest(owner)!=null&&saves.Count==(immune?0:1)&&
                            (immune?target.Descriptor.State.HasConditionImmunity(UnitCondition.Petrified):(bool)saves.Rows[0]["passed"]);
                        Sprint25Check("breath-"+(immune?"native-immunity":"initial-save"),exact,new JObject{["owner"]=owner.UniqueId,["victim"]=target.UniqueId,
                            ["immune"]=immune,["saves"]=saves.Rows,["damage"]=target.Descriptor.Damage,["lifeDead"]=target.Descriptor.State.IsDead,["lane"]=lane},
                            "A real cone activation respects native petrification immunity or a genuine successful first Fortitude save, without state or acid damage.");
                    }
                }finally{
                    if(grant!=null)target.Descriptor.RemoveFact(grant);UnityEngine.Random.state=random;PlaceExpandedSummoningUnit(fixture.Caster,casterBefore);
                    DisposeExpandedSummoningUnits(fixture.Created,new[]{owner,target});UnityEngine.Object.Destroy(targetBlueprint);
                }
            }
        }

        private void Sprint26ConeGeometry(UnitEntityData owner,UnitEntityData target,BlueprintAbility breath)
        {
            var original=target.Position;var origin=owner.Position;
            var delivery=breath.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityDeliverProjectile>();
            var data=new AbilityData(owner.Descriptor.Abilities.GetAbility(breath));
            bool range=false,wall=false;var attempts=new JArray();
            try{
                foreach(float distance in new[]{6f,12f,18f,24f}){
                    foreach(var offset in CompassOffsets){
                        Vector3 requested=origin+offset*distance;var node=Kingmaker.View.ObstacleAnalyzer.GetNearestNode(requested);
                        if(node.node==null||!node.node.Walkable||Vector3.Distance(node.clampedPosition,requested)>.6f)continue;
                        PlaceExpandedSummoningUnit(target,node.clampedPosition);
                        float gap=owner.DistanceTo(target);bool blocked=Kingmaker.Visual.FogOfWar.LineOfSightGeometry.Instance.HasObstacle(owner.EyePosition,target.EyePosition,0);
                        bool reached=delivery.WouldTargetUnit(data,target.Position,target);
                        bool rangeCase=gap>60*.3048f+target.Corpulence+1f&&!blocked;
                        bool wallCase=gap>2f&&gap<60*.3048f-target.Corpulence&&blocked;
                        if(!rangeCase&&!wallCase)continue;
                        attempts.Add(new JObject{["point"]=SurveyVector(target.Position),["distance"]=gap,["wall"]=blocked,["nativeDeliveryReaches"]=reached,["rangeCase"]=rangeCase});
                        if(rangeCase&&!reached)range=true;if(wallCase&&!reached)wall=true;
                        if(range&&wall)break;
                    }
                    if(range&&wall)break;
                }
                Sprint25Check("breath-native-range-wall",range&&wall,new JObject{["rangePreconditionAndRefusal"]=range,["wallPreconditionAndRefusal"]=wall,["observations"]=attempts,["commandsIssued"]=0},
                    "The actual native cone delivery refuses a visible beyond-range victim and an in-range victim behind an actual wall; ordinary cone geometry remains authoritative.");
            }finally{PlaceExpandedSummoningUnit(target,original);}
        }

        private IEnumerable<int> Sprint26GorgonLifeBoundaries(ExpandedSummoningCorrectionFixture fixture)
        {
            foreach(string sample in new[]{"unrelated-native-stone","immunity-cycle","lethal-health","force-kill","live-hd-con-trample"}){
                var owner=CastExpandedSummoningOwnTier(fixture,"gorgon");SetExpandedSummoningBrainActive(owner,false);
                BlueprintUnit targetBlueprint;var target=Sprint26BoundaryTarget(fixture,owner,out targetBlueprint);
                var breath=owner.Descriptor.Abilities.Enumerable.Single(v=>v.Blueprint.name=="KMG_Summoning_Special_Gorgon_Breath").Blueprint;
                var activation=breath.GetComponent<GorgonBreathOnCast>();
                var stone=fixture.Blueprints.OfType<BlueprintBuff>().Single(v=>v.AssetGuid==Sprint26MagicalBeastRules.PetrificationGuid);
                var nativeStone=fixture.Blueprints.OfType<BlueprintBuff>().Single(v=>v.AssetGuid==Sprint26MagicalBeastRules.PetrifiedDonor);
                var random=UnityEngine.Random.state;bool immunityAdded=false;
                var previousRules=_rulesFixture;
                try{
                    foreach(int f in Sprint26Appearance(owner,"gorgon"))yield return f;
                    target.Stats.SaveFortitude.BaseValue=-100;
                    if(sample=="live-hd-con-trample"){
                        owner.Descriptor.Progression.AddClassLevel(activation.HitDieClass);owner.Descriptor.Progression.AddClassLevel(activation.HitDieClass);
                        owner.Stats.Constitution.BaseValue=28;owner.Stats.Strength.BaseValue=26;
                    }
                    if(sample=="unrelated-native-stone")target.Descriptor.AddBuff(nativeStone,owner,null);
                    var data=new AbilityData(owner.Descriptor.Abilities.GetAbility(breath));var calculated=data.CalculateParams();int beforeCastDc=calculated.DC;
                    var context=new AbilityExecutionContext(data,calculated,new Kingmaker.Utility.TargetWrapper(target),Rulebook.CurrentContext);
                    activation.OnCast(context);
                    using(var saves=new Sprint26Saves(target)){
                        saves.DesiredRoll=2;
                        string result=GorgonBreathRuntime.Apply(context,target,stone);
                        var buff=target.Buffs.GetBuff(stone);var state=buff?.SelectComponents<GorgonPetrificationState>().Single();
                        _sprint26Life.TickOwned(target);
                        bool exact;var details=new JObject{["owner"]=owner.UniqueId,["victim"]=target.UniqueId,["sample"]=sample,["deliverySetupOnly"]=true,
                            ["outcome"]=result,["saves"]=saves.Rows,["ownPermanent"]=state?.Permanent,["nativeConditionCount"]=GorgonPetrificationLife.ConditionCount(target.Descriptor.State)};
                        if(sample=="unrelated-native-stone"){
                            exact=result=="temporary"&&state!=null&&!state.Permanent&&saves.Count==1&&target.Buffs.GetBuff(nativeStone)!=null&&target.Descriptor.State.IsDead;
                        }else{
                            exact=result=="temporary"&&state!=null&&state.Initialized&&!state.Permanent&&!target.Descriptor.State.IsDead&&target.Descriptor.Damage==0;
                            if(sample=="immunity-cycle"){
                                target.Descriptor.State.AddConditionImmunity(UnitCondition.Petrified);immunityAdded=true;
                                bool immune=!target.Descriptor.State.HasCondition(UnitCondition.Petrified);
                                target.Descriptor.State.RemoveConditionImmunity(UnitCondition.Petrified);immunityAdded=false;_sprint26Life.TickOwned(target);
                                exact&=immune&&target.Descriptor.State.HasCondition(UnitCondition.Petrified)&&GorgonPetrificationLife.ConditionCount(target.Descriptor.State)==1&&
                                    target.Buffs.GetBuff(nativeStone)==null&&!target.Descriptor.State.IsDead&&target.Descriptor.Damage==0;
                                details["immuneSuppressedCondition"]=immune;
                            }else if(sample=="lethal-health"||sample=="force-kill"){
                                if(sample=="lethal-health")target.Descriptor.Damage=target.Stats.HitPoints.ModifiedValue+target.Stats.Constitution.ModifiedValue;
                                else target.Descriptor.State.ForceKill=true;
                                _sprint26Life.TickOwned(target);exact&=target.Descriptor.State.IsDead;
                            }else{
                                var live=GorgonBreathRuntime.Latest(owner);
                                exact&=owner.Descriptor.Progression.GetClassLevel(activation.HitDieClass)==10&&owner.Stats.Constitution.Bonus==9&&beforeCastDc==24&&live.Dc==24&&state.RecoveryDc==24;
                                details["actualHd"]=owner.Descriptor.Progression.GetClassLevel(activation.HitDieClass);details["constitutionBonus"]=owner.Stats.Constitution.Bonus;
                                details["beforeCastCalculatedDc"]=beforeCastDc;details["actualDc"]=live.Dc;
                                // Reset only this direct-control fixture state before testing contact eligibility; this is not cure proof.
                                buff.Remove();_sprint26Life.TickOwned(target);target.Descriptor.State.Size=Size.Medium;
                                _rulesFixture=fixture;target.Stats.SaveReflex.BaseValue=-100;
                                PrepareExpandedSummoningTrampleResponsePair(fixture,owner,target,0,1f);Game.Instance.Player.GameTime+=TimeSpan.FromSeconds(6);
                                var first=RunExpandedSummoningTrampleContact(owner,target,"sprint26-live-hd-first",2,"KMG_Summoning_Special_Gorgon_Trample");
                                var duplicate=RunExpandedSummoningTrampleContact(owner,target,"sprint26-live-hd-duplicate",2,"KMG_Summoning_Special_Gorgon_Trample");
                                Game.Instance.Player.GameTime+=TimeSpan.FromSeconds(6);
                                var later=RunExpandedSummoningTrampleContact(owner,target,"sprint26-live-hd-later",2,"KMG_Summoning_Special_Gorgon_Trample");
                                exact&=first.Damage.Count==1&&first.Saves.Count==1&&first.Saves[0].DifficultyClass==23&&first.Damage[0].DamageBundle.First.Bonus==12&&
                                    duplicate.Damage.Count==0&&duplicate.Saves.Count==0&&later.Damage.Count==1;
                                details["firstTrample"]=Sprint23TrampleResult(first);details["duplicateTrample"]=Sprint23TrampleResult(duplicate);details["nextRoundTrample"]=Sprint23TrampleResult(later);
                            }
                        }
                        details["deadAfter"]=target.Descriptor.State.IsDead;details["damageAfter"]=target.Descriptor.Damage;
                        Sprint25Check("gorgon-native-boundary-"+sample,exact,details,"Exact disposable native status/life/contact boundaries preserve attribution, immunity, independent death, live HD/Con/Str and one trample contact per target/round. Direct delivery arranges this mechanism control and is not command/AI proof.");
                    }
                }finally{
                    if(immunityAdded)target.Descriptor.State.RemoveConditionImmunity(UnitCondition.Petrified);
                    _rulesFixture=previousRules;UnityEngine.Random.state=random;
                    DisposeExpandedSummoningUnits(fixture.Created,new[]{owner,target});UnityEngine.Object.Destroy(targetBlueprint);
                }
            }
        }
    }
}
