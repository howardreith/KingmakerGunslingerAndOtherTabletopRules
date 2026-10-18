using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Facts;
using Harmony12;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Two disposable state victims, never human-party or quest actors. Only
    // primitive provenance is serialized. The source may be absent on load.
    public sealed class UnitPartSprint26StateReceipt : UnitPart
    {
        [JsonProperty] public string Scope, Role, UnitId, FirstSourceId, LastSourceId, StoneGuid, PrepareRunId;
        [JsonProperty] public string CooldownSourceId;
        [JsonProperty] public long CooldownEndTicks, PreparedGameTicks;
        [JsonProperty] public int CooldownRounds, RecoveryDc;
    }

    internal sealed partial class RuntimeTestRunner
    {
        private Sprint26StateLoadTrace _sprint26StateTrace;
        private void StartSprint26StateTrace()
        {
            if (Sprint26PersistenceScope) _sprint26StateTrace=new Sprint26StateLoadTrace(_context.Harmony);
        }
        private void StopSprint26StateTrace(RuntimeTestResult result)
        {
            if(_sprint26StateTrace==null)return;
            try {
                _sprint26StateTrace.Dispose();
                string path=System.IO.Path.Combine(_request.EvidenceDirectory,"sprint26-state-load-trace.json");
                RuntimeTestResultWriter.WriteAtomic(path,_sprint26StateTrace.Rows.ToString(Newtonsoft.Json.Formatting.Indented));
                if(result.EvidenceFiles==null)result.EvidenceFiles=new List<string>();result.EvidenceFiles.Add(path);
                if(_sprint26StateTrace.Errors!=0)throw new InvalidOperationException("Sprint26 state trace errors="+_sprint26StateTrace.Errors);
            }catch(Exception e){result.Status=RuntimeTestStatuses.Fail;result.Diagnostics.Add("Sprint26 state observer: "+e);}
            finally{_sprint26StateTrace=null;}
        }
        // Request-local read-only attribution, armed before the native cold load.
        // No buff, life state, save, condition counter or roll is changed here.
        private sealed class Sprint26StateLoadTrace : IDisposable
        {
            private static Sprint26StateLoadTrace _active;
            private readonly HarmonyInstance _harmony;
            private readonly List<KeyValuePair<MethodInfo,MethodInfo>> _patches=new List<KeyValuePair<MethodInfo,MethodInfo>>();
            internal readonly JArray Rows=new JArray();
            internal int Errors;
            internal Sprint26StateLoadTrace(HarmonyInstance harmony)
            {
                if(_active!=null)throw new InvalidOperationException("Sprint26 state observer already active.");
                _active=this;_harmony=harmony;
                try{
                    Patch(typeof(Buff).GetMethod("PostLoad"),"BeforeBuffLoad","AfterBuffLoad");
                    Patch(typeof(Buff).GetMethod("PreSave"),null,"AfterBuffSave");
                    Patch(typeof(FactCollection).GetMethod("RemoveFact",new[]{typeof(Fact)}),"BeforeRemoval",null);
                    Patch(typeof(UnitDescriptor).GetMethod("PostLoad"),"BeforeUnitLoad","AfterUnitLoad");
                    Patch(typeof(UnitDescriptor).GetMethod("Resurrect",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic,null,new[]{typeof(float),typeof(bool),typeof(bool)},null),"BeforeResurrection",null);
                }catch{Dispose();throw;}
            }
            private void Patch(MethodInfo original,string prefix,string postfix)
            {
                if(original==null)throw new MissingMethodException("Sprint26 exact native observer seam missing.");
                foreach(string name in new[]{prefix,postfix}.Where(v=>v!=null)){
                    var method=typeof(Sprint26StateLoadTrace).GetMethod(name,BindingFlags.Static|BindingFlags.NonPublic);
                    _harmony.Patch(original,name==prefix?new HarmonyMethod(method):null,name==postfix?new HarmonyMethod(method):null,null);
                    _patches.Add(new KeyValuePair<MethodInfo,MethodInfo>(original,method));
                }
            }
            private static bool Own(Buff buff){return buff?.Blueprint.AssetGuid==Sprint26MagicalBeastRules.PetrificationGuid;}
            private static Buff Stone(UnitDescriptor unit){return unit?.Buffs?.Enumerable.FirstOrDefault(Own);}
            private static bool Victim(UnitDescriptor unit){return unit?.Unit?.Get<UnitPartSprint26StateReceipt>()!=null||Stone(unit)!=null;}
            private static void BeforeBuffLoad(Buff __instance){if(Own(__instance))_active?.Capture("buff-load-before",__instance.Owner,__instance,true);}
            private static void AfterBuffLoad(Buff __instance){if(Own(__instance))_active?.Capture("buff-load-after",__instance.Owner,__instance,false);}
            private static void AfterBuffSave(Buff __instance){if(Own(__instance))_active?.Capture("buff-save-after",__instance.Owner,__instance,true);}
            private static void BeforeRemoval(Fact __0){var b=__0 as Buff;if(Own(b))_active?.Capture("buff-remove",b.Owner,b,false);}
            private static void BeforeUnitLoad(UnitDescriptor __instance){if(Victim(__instance))_active?.Capture("unit-load-before",__instance,Stone(__instance),false);}
            private static void AfterUnitLoad(UnitDescriptor __instance){if(Victim(__instance))_active?.Capture("unit-load-after",__instance,Stone(__instance),false);}
            private static void BeforeResurrection(UnitDescriptor __instance){if(Victim(__instance))_active?.Capture("native-resurrect",__instance,Stone(__instance),false);}
            private void Capture(string stage,UnitDescriptor owner,Buff buff,bool raw)
            {
                try{
                    if(Rows.Count>=128){Errors++;return;}
                    var r=new JObject{["stage"]=stage,["frame"]=Time.frameCount,["unit"]=owner?.Unit?.UniqueId,
                        ["dead"]=owner?.State.IsDead,["finallyDead"]=owner?.State.IsFinallyDead,["damage"]=owner?.Damage,
                        ["conditionCount"]=owner==null?(int?)null:GorgonPetrificationLife.ConditionCount(owner.State),
                        ["stayOnDeath"]=buff?.Blueprint.StayOnDeath,["removeOnResurrect"]=buff?.Blueprint.RemoveOnResurrect,
                        ["contextCaster"]=buff?.MaybeContext?.MaybeCaster?.UniqueId,
                        ["stone"]=buff!=null&&(stage=="buff-load-after"||stage=="buff-save-after")?Sprint26SavedStone(owner.Unit,buff.Blueprint):null,
                        ["stack"]=Environment.StackTrace};
                    if(raw&&buff!=null){
                        var field=typeof(Fact).GetField("m_ComponentsData",BindingFlags.Instance|BindingFlags.NonPublic);
                        var data=field.GetValue(buff) as System.Collections.IEnumerable;
                        r["savedComponents"]=data==null?JValue.CreateNull():JToken.FromObject(data);
                    }
                    Rows.Add(r);
                }catch(Exception e){Errors++;Rows.Add(new JObject{["stage"]=stage,["observerError"]=e.ToString()});}
            }
            public void Dispose(){foreach(var p in _patches)_harmony.Unpatch(p.Key,p.Value);_patches.Clear();if(ReferenceEquals(_active,this))_active=null;}
        }

        private const string Sprint26StateScope="KMG_Sprint26_MagicalBeast_State_v1";
        private Sprint26PersistenceRolls _sprint26PersistenceRolls;
        private static UnitEntityData[] Sprint26StateActors(UnitEntityData caster)
        {
            var state=Game.Instance.State.Units.All;
            var area=caster.HoldingState.AllEntityData.OfType<UnitEntityData>();
            return state.Concat(area).Where(v=>v!=null&&v.Descriptor!=null&&v.Get<UnitPartSprint26StateReceipt>()!=null).Distinct().ToArray();
        }
        private static bool Sprint26OwnsStateActor(UnitEntityData actor)
        {
            var r=actor?.Get<UnitPartSprint26StateReceipt>();
            return r!=null&&r.Scope==Sprint26StateScope&&r.UnitId==actor.UniqueId&&
                (r.Role=="temporary-source-gone"||r.Role=="permanent-cross-source")&&
                r.StoneGuid==Sprint26MagicalBeastRules.PetrificationGuid&&!string.IsNullOrWhiteSpace(r.PrepareRunId)&&
                !string.IsNullOrWhiteSpace(r.FirstSourceId)&&!string.IsNullOrWhiteSpace(r.LastSourceId)&&
                ReferenceEquals(actor.Blueprint,BlueprintRoot.Instance.DefaultPlayerCharacter)&&!Game.Instance.Player.Party.Contains(actor);
        }
        private UnitEntityData Sprint26CreateStateActor(UnitEntityData caster, UnitEntityData source,string role,Vector3 offset)
        {
            var actor=Game.Instance.EntityCreator.SpawnUnit(BlueprintRoot.Instance.DefaultPlayerCharacter,source.Position+offset,Quaternion.identity,caster.HoldingState);
            Game.Instance.EntityCreator.Tick();
            if(actor==null||actor.View==null||!ReferenceEquals(actor.HoldingState,caster.HoldingState)||Game.Instance.Player.Party.Contains(actor))
                throw new InvalidOperationException("Exact disposable Sprint26 state actor unavailable.");
            actor.Stats.HitPoints.BaseValue=100000;actor.Stats.SaveFortitude.BaseValue=-100;
            SetExpandedSummoningBrainActive(actor,false);PlaceExpandedSummoningUnit(actor,actor.Position);
            if(!Game.Instance.State.AwakeUnits.Contains(actor))Game.Instance.State.AwakeUnits.Add(actor);
            var r=actor.Ensure<UnitPartSprint26StateReceipt>();r.Scope=Sprint26StateScope;r.Role=role;r.UnitId=actor.UniqueId;
            r.StoneGuid=Sprint26MagicalBeastRules.PetrificationGuid;r.PrepareRunId=_request.RunId;
            return actor;
        }
        private static int Sprint26D4Seed(int expected)
        {
            for(int seed=0;seed<100000;seed++){UnityEngine.Random.InitState(seed);if(UnityEngine.Random.Range(1,5)==expected)return seed;}
            throw new InvalidOperationException("Required native d4 precondition absent.");
        }
        private static JObject Sprint26SavedStone(UnitEntityData actor,BlueprintBuff stone)
        {
            var buff=actor.Buffs.GetBuff(stone);var s=buff?.SelectComponents<GorgonPetrificationState>().Single();
            return new JObject{["unit"]=actor.UniqueId,["buff"]=buff?.Blueprint.AssetGuid,["initialized"]=s?.Initialized,
                ["permanent"]=s?.Permanent,["dc"]=s?.RecoveryDc,["firstSource"]=s?.FirstSourceId,["lastSource"]=s?.LastSourceId,
                ["contextCaster"]=buff?.MaybeContext?.MaybeCaster?.UniqueId,["nativeConditionCount"]=GorgonPetrificationLife.ConditionCount(actor.Descriptor.State),
                ["dead"]=actor.Descriptor.State.IsDead,["damage"]=actor.Descriptor.Damage};
        }
        private IEnumerable<int> Sprint26PersistentStates(UnitEntityData caster,UnitEntityData[] sources,bool prepare,bool verify)
        {
            var actors=Sprint26StateActors(caster);
            if(!prepare&&!verify){
                RosterPersistenceCheck("sprint26-fresh-state-absence",actors.Length==0,new JObject{["receiptActors"]=actors.Length},"Fresh native load contains no Sprint26 disposable state actors.");yield break;
            }
            var owner=sources.Single(v=>v.Get<UnitPartRelease143RosterReceipt>()?.Key=="gorgon");
            var all=BlueprintBootstrap.Library.GetAllBlueprints().Where(v=>v!=null).ToArray();
            var breath=all.OfType<BlueprintAbility>().Single(v=>v.name=="KMG_Summoning_Special_Gorgon_Breath");
            var stone=all.OfType<BlueprintBuff>().Single(v=>v.AssetGuid==Sprint26MagicalBeastRules.PetrificationGuid);
            var onCast=breath.GetComponent<GorgonBreathOnCast>();var clock=onCast.Cooldown;
            if(prepare){
                if(actors.Length!=0||sources.Length!=2)throw new InvalidOperationException("Sprint26 prepare requires exactly two sources and no prior condition actors.");
                var random=UnityEngine.Random.state;UnitEntityData transient=null;
                var created=new List<UnitEntityData>();
                try{
                    var variant=ExpandedSummoningRosterPersistencePolicy.SelectCreatureVariant(ExpandedSummoningRosterPersistencePolicy.MagicalBeastsScope,"gorgon");
                    transient=SpawnExpandedSummoningVariants(all,caster,new[]{variant},"Sprint26 transient persistence source").Single();
                    SetExpandedSummoningBrainActive(transient,false);
                    var temporary=Sprint26CreateStateActor(caster,transient,"temporary-source-gone",Vector3.right*3f);created.Add(temporary);
                    var permanent=Sprint26CreateStateActor(caster,transient,"permanent-cross-source",Vector3.forward*3f);created.Add(permanent);
                    // Persistence preconditions call the real production state
                    // path. They do not qualify a cast, action, cone or AI; those
                    // have separate mandatory command experiments.
                    var data=new AbilityData(transient.Descriptor.Abilities.GetAbility(breath));
                    var first=new AbilityExecutionContext(data,data.CalculateParams(),new Kingmaker.Utility.TargetWrapper(temporary),Rulebook.CurrentContext);
                    UnityEngine.Random.InitState(Sprint26D4Seed(4));onCast.OnCast(first);
                    string a,b,c;
                    using(var tempRolls=new Sprint26Saves(temporary))using(var permRolls=new Sprint26Saves(permanent)){
                        tempRolls.DesiredRoll=permRolls.DesiredRoll=2;
                        a=GorgonBreathRuntime.Apply(first,temporary,stone);b=GorgonBreathRuntime.Apply(first,permanent,stone);
                        var laterData=new AbilityData(owner.Descriptor.Abilities.GetAbility(breath));
                        var later=new AbilityExecutionContext(laterData,laterData.CalculateParams(),new Kingmaker.Utility.TargetWrapper(permanent),Rulebook.CurrentContext);
                        UnityEngine.Random.InitState(Sprint26D4Seed(4));onCast.OnCast(later);
                        c=GorgonBreathRuntime.Apply(later,permanent,stone);
                    }
                    _sprint26Life.TickOwned(temporary);_sprint26Life.TickOwned(permanent);
                    var activeClock=owner.Buffs.GetBuff(clock);var activation=GorgonBreathRuntime.Latest(owner);
                    if(activeClock==null||activation==null)throw new InvalidOperationException("Production source clock absent before save.");
                    foreach(var actor in created){
                        var r=actor.Get<UnitPartSprint26StateReceipt>();var state=actor.Buffs.GetBuff(stone)?.SelectComponents<GorgonPetrificationState>().Single();
                        if(state==null)throw new InvalidOperationException("Production victim stage absent before save.");
                        r.FirstSourceId=state.FirstSourceId;r.LastSourceId=state.LastSourceId;r.RecoveryDc=state.RecoveryDc;
                        r.CooldownSourceId=owner.UniqueId;r.CooldownEndTicks=activeClock.EndTime.Ticks;r.PreparedGameTicks=Game.Instance.Player.GameTime.Ticks;r.CooldownRounds=activation.CooldownRounds;
                    }
                    string departed=transient.UniqueId;transient.Destroy();Game.Instance.EntityDestroyer.Tick();
                    for(int f=0;f<3;f++)yield return 0;
                    bool exact=a=="temporary"&&b=="temporary"&&c=="permanent"&&transient.Destroyed&&transient.HoldingState==null&&
                        created.All(Sprint26OwnsStateActor)&&!temporary.Descriptor.State.IsDead&&permanent.Descriptor.State.IsDead&&
                        GorgonPetrificationLife.ConditionCount(temporary.Descriptor.State)==1&&GorgonPetrificationLife.ConditionCount(permanent.Descriptor.State)==1&&
                        activation.CooldownRounds==5&&activeClock.TimeLeft.TotalSeconds>0&&activeClock.TimeLeft.TotalSeconds<=30;
                    RosterPersistenceCheck("sprint26-state-fixture-prepared",exact,new JObject{["setupOnly"]=true,["firstOutcome"]=a,["secondVictimOutcome"]=b,["laterOutcome"]=c,
                        ["departedSource"]=departed,["temporary"]=Sprint26SavedStone(temporary,stone),["permanent"]=Sprint26SavedStone(permanent,stone),
                        ["cooldownRounds"]=activation.CooldownRounds,["cooldownEndTicks"]=activeClock.EndTime.Ticks},"Before the exact working-save write, two receipt-owned victims carry real temporary/permanent production states, their first source is gone, and a distinct live source owns a genuine five-round cooldown.");
                }finally{UnityEngine.Random.state=random;if(transient!=null&&!transient.Destroyed){transient.Destroy();Game.Instance.EntityDestroyer.Tick();}}
                yield break;
            }
            if(actors.Length!=2||!actors.All(Sprint26OwnsStateActor))throw new InvalidOperationException("Cold Sprint26 state receipts are absent or unowned.");
            var temp=actors.Single(v=>v.Get<UnitPartSprint26StateReceipt>().Role=="temporary-source-gone");
            var perm=actors.Single(v=>v.Get<UnitPartSprint26StateReceipt>().Role=="permanent-cross-source");
            var receipts=actors.Select(v=>v.Get<UnitPartSprint26StateReceipt>()).ToArray();
            var tempBuff=temp.Buffs.GetBuff(stone);var permBuff=perm.Buffs.GetBuff(stone);
            var ts=tempBuff?.SelectComponents<GorgonPetrificationState>().Single();var ps=permBuff?.SelectComponents<GorgonPetrificationState>().Single();
            var savedClock=owner.Buffs.GetBuff(clock);bool boundary=ts!=null&&ps!=null&&ts.Initialized&&ps.Initialized&&!ts.Permanent&&ps.Permanent&&
                !temp.Descriptor.State.IsDead&&perm.Descriptor.State.IsDead&&receipts.All(r=>r.CooldownSourceId==owner.UniqueId)&&
                receipts.All(r=>r.FirstSourceId==ts.FirstSourceId)&&ps.FirstSourceId==ts.FirstSourceId&&ps.LastSourceId==owner.UniqueId&&
                ts.LastSourceId==ts.FirstSourceId&&ts.RecoveryDc==21&&ps.RecoveryDc==21&&
                tempBuff.MaybeContext.MaybeCaster==null&&permBuff.MaybeContext.MaybeCaster==null&&
                savedClock!=null&&receipts.All(r=>r.CooldownEndTicks==savedClock.EndTime.Ticks)&&savedClock.TimeLeft>TimeSpan.Zero&&
                savedClock.TimeLeft.TotalSeconds<=30&&GorgonBreathRuntime.Latest(owner)==null&&
                !new AbilityData(owner.Descriptor.Abilities.GetAbility(breath)).IsAvailable&&
                actors.All(v=>GorgonPetrificationLife.ConditionCount(v.Descriptor.State)==1);
            RosterPersistenceCheck("sprint26-cold-state-boundary",boundary,new JObject{["moduleEnabled"]=_context.FeatureModules.Active.ExpandedSummoning,
                ["temporary"]=Sprint26SavedStone(temp,stone),["permanent"]=Sprint26SavedStone(perm,stone),["cooldownEndTicks"]=savedClock?.EndTime.Ticks,
                ["savedCooldownEndTicks"]=receipts[0].CooldownEndTicks,["clockSecondsLeft"]=savedClock?.TimeLeft.TotalSeconds,["freshActivationAbsent"]=GorgonBreathRuntime.Latest(owner)==null},
                "Native cold load preserves victim stages/DC/source strings with a missing original source, and the exact unrefreshed source cooldown, including module OFF.");
            if(!boundary)throw new InvalidOperationException("Sprint26 saved-state boundary failed; no cleanup save armed.");
            if(_sprint26PersistenceRolls!=null)_sprint26PersistenceRolls.Enabled=false;
            var next=typeof(Buff).GetProperty("NextTickTime",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
            if(next?.GetSetMethod(true)==null)throw new MissingMemberException("Native Buff.NextTickTime setter absent.");
            int tempDamage=temp.Descriptor.Damage;
            using(var temporarySaves=new Sprint26Saves(temp))using(var permanentSaves=new Sprint26Saves(perm)){
                temporarySaves.DesiredRoll=permanentSaves.DesiredRoll=20;temp.Stats.SaveFortitude.BaseValue=100;
                next.SetValue(tempBuff,Game.Instance.TimeController.GameTime,null);temp.Buffs.UpdateNextEvent();temp.Buffs.Tick();
                next.SetValue(permBuff,Game.Instance.TimeController.GameTime,null);perm.Buffs.UpdateNextEvent();perm.Buffs.Tick();
                _sprint26Life.TickOwned(temp);_sprint26Life.TickOwned(perm);
                bool continued=temp.Buffs.GetBuff(stone)==null&&!temp.Descriptor.State.HasCondition(UnitCondition.Petrified)&&
                    !temp.Descriptor.State.IsDead&&temp.Descriptor.Damage==tempDamage&&temporarySaves.Count==1&&
                    perm.Buffs.GetBuff(stone)==permBuff&&ps.Permanent&&permanentSaves.Count==0&&perm.Descriptor.State.IsDead;
                RosterPersistenceCheck("sprint26-cold-state-behavior",continued,new JObject{["temporarySaves"]=temporarySaves.Rows,["permanentSaves"]=permanentSaves.Rows,
                    ["temporaryConditionCount"]=GorgonPetrificationLife.ConditionCount(temp.Descriptor.State),["permanentStillPresent"]=perm.Buffs.GetBuff(stone)==permBuff},
                    "After cold load with source absent, one real native round save recovers temporary stone and permanent stone receives no natural recovery save; fixture removal is not cure evidence.");
            }
            foreach(var actor in actors){actor.Buffs.GetBuff(stone)?.Remove();actor.Destroy();}Game.Instance.EntityDestroyer.Tick();
            for(int f=0;f<30;f++)yield return 0;
            RosterPersistenceCheck("sprint26-state-cleanup",actors.All(v=>v.Destroyed)&&Sprint26StateActors(caster).Length==0,
                new JObject{["destroyed"]=actors.Count(v=>v.Destroyed),["remainingReceiptActors"]=Sprint26StateActors(caster).Length},"Exact receipt-owned victim cleanup leaves no condition fixture before the native cleanup save.");
        }
        // Deterministic failed recovery is only a saved-fixture precondition.
        // It never targets the human party or changes the result of a check.
        private sealed class Sprint26PersistenceRolls : IGlobalRulebookHandler<RuleSavingThrow>,IDisposable
        {
            internal bool Enabled=true;
            internal Sprint26PersistenceRolls(){EventBus.Subscribe(this);}
            public void OnEventAboutToTrigger(RuleSavingThrow evt){if(Enabled&&Sprint26OwnsStateActor(evt.Initiator)&&evt.Initiator.Get<UnitPartSprint26StateReceipt>().Role=="temporary-source-gone")UnityEngine.Random.InitState(FindNativeD20Seed(2));}
            public void OnEventDidTrigger(RuleSavingThrow evt){}
            public void Dispose(){EventBus.Unsubscribe(this);}
        }
    }
}
