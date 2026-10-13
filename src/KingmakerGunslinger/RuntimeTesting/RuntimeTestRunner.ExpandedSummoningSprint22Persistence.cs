using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    public sealed class UnitPartSprint22ControlReceipt : UnitPart
    {
        [JsonProperty] public string Scope, UnitId, SourceId, BuffGuid, PrepareRunId;
        [JsonProperty] public long EndTicks;
        [JsonProperty] public int AttackD20, SaveD20, DifficultyClass;
    }

    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> Sprint22PersistentControl(UnitEntityData caster,UnitEntityData[] sources,bool prepare,bool verify)
        {
            const string scope="KMG_Sprint22_Finite_Control_v1";
            var actors=caster.HoldingState.AllEntityData.OfType<UnitEntityData>().Where(value=>value.Get<UnitPartSprint22ControlReceipt>()!=null).ToArray();
            if (actors.Any(value=>value.Get<UnitPartSprint22ControlReceipt>().Scope!=scope || value.Get<UnitPartSprint22ControlReceipt>().UnitId!=value.UniqueId ||
                Game.Instance.Player.Party.Contains(value))) throw new InvalidOperationException("Ambiguous Sprint22 victim ownership; no cleanup/save allowed.");
            if (prepare)
            {
                if (actors.Length!=0 || sources.Length!=1) throw new InvalidOperationException("Sprint22 prepare requires one summon and no previous victim.");
                var source=sources.Single();
                var victim=Game.Instance.EntityCreator.SpawnUnit(BlueprintRoot.Instance.DefaultPlayerCharacter,
                    source.Position+Vector3.forward*3f,Quaternion.identity,caster.HoldingState);
                Game.Instance.EntityCreator.Tick();SetExpandedSummoningBrainActive(victim,false);
                victim.Descriptor.Stats.HitPoints.BaseValue=100000;
                int oldAttack=source.Descriptor.Stats.AdditionalAttackBonus.BaseValue;
                int oldFort=victim.Descriptor.Stats.SaveFortitude.BaseValue;
                var original=victim.Descriptor.Buffs.RawFacts.OfType<Buff>().ToArray();
                var saves=new Sprint19SaveObserver { Target=victim };EventBus.Subscribe(saves);
                var trials=new JArray();Buff control=null;RuleAttackRoll attack=null;
                try
                {
                    source.Descriptor.Stats.AdditionalAttackBonus.BaseValue=100;
                    victim.Descriptor.Stats.SaveFortitude.BaseValue=-100;
                    for(int trial=1;trial<=120;trial++)
                    {
                        foreach(var buff in victim.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(value=>!original.Contains(value)).ToArray()) buff.Remove();
                        victim.Descriptor.Damage=0;saves.Saves.Clear();
                        var strike=Rulebook.Trigger(new RuleAttackWithWeapon(source,victim,Sprint19Limbs(source).Single(),0));attack=strike.AttackRoll;
                        control=victim.Descriptor.Buffs.RawFacts.OfType<Buff>().SingleOrDefault(value=>value.Blueprint.name=="KMG_Summoning_Special_Ankylosaurus_Stunned");
                        bool setup=attack!=null && attack.IsHit && attack.IsCriticalConfirmed &&
                            (saves.Saves.Count!=1 || !saves.Saves[0].IsPassed);
                        trials.Add(new JObject { ["trial"]=trial,["hit"]=attack!=null && attack.IsHit,["critical"]=attack!=null && attack.IsCriticalConfirmed,
                            ["d20"]=attack==null ? -1 : attack.Roll.Value,["saves"]=saves.Saves.Count,
                            ["passed"]=saves.Saves.Count==1 && saves.Saves[0].IsPassed,["preconditionsSatisfied"]=setup });
                        if (setup)
                        {
                            if (saves.Saves.Count!=1 || control==null || !victim.Descriptor.State.HasCondition(UnitCondition.Stunned))
                                throw new InvalidOperationException("unit="+source.UniqueId+";predicate=persistent-critical-control;event=native-strike;owner="+source.Blueprint.AssetGuid+";trials="+trials);
                            break;
                        }
                        control=null;
                    }
                    if (control==null) throw new InvalidOperationException("Persistent critical-control dice prerequisites unsatisfied; no save armed. "+trials);
                    var receipt=victim.Ensure<UnitPartSprint22ControlReceipt>();
                    receipt.Scope=scope;receipt.UnitId=victim.UniqueId;receipt.SourceId=source.UniqueId;receipt.BuffGuid=control.Blueprint.AssetGuid;
                    receipt.EndTicks=control.EndTime.Ticks;receipt.PrepareRunId=_request.RunId;receipt.AttackD20=attack.Roll.Value;
                    receipt.SaveD20=saves.Saves.Single().D20.Value;receipt.DifficultyClass=saves.Saves.Single().DifficultyClass;
                    RosterPersistenceCheck("sprint22-finite-control-prepared",control.TimeLeft.TotalSeconds>0 && control.TimeLeft.TotalSeconds<=24.01,
                        new JObject { ["unit"]=victim.UniqueId,["source"]=source.UniqueId,["event"]="native-critical-before-save",["owner"]=scope,
                            ["trials"]=trials,["buff"]=receipt.BuffGuid,["endTicks"]=receipt.EndTicks,["remainingSeconds"]=control.TimeLeft.TotalSeconds,
                            ["saveD20"]=receipt.SaveD20,["dc"]=receipt.DifficultyClass },"One native critical failed save creates a finite victim condition before the exact working-save write.");
                }
                finally
                {
                    EventBus.Unsubscribe(saves);source.Descriptor.Stats.AdditionalAttackBonus.BaseValue=oldAttack;
                    victim.Descriptor.Stats.SaveFortitude.BaseValue=oldFort;
                }
                yield break;
            }
            if (verify)
            {
                if (actors.Length!=1 || sources.Length!=1) throw new InvalidOperationException("Exact one saved source/victim required for Sprint22 cold load.");
                var victim=actors.Single();var receipt=victim.Get<UnitPartSprint22ControlReceipt>();
                var control=victim.Descriptor.Buffs.RawFacts.OfType<Buff>().SingleOrDefault(value=>value.Blueprint.AssetGuid==receipt.BuffGuid);
                bool same=control!=null && control.EndTime.Ticks==receipt.EndTicks && control.TimeLeft.TotalSeconds>0 && control.TimeLeft.TotalSeconds<=24.01 &&
                    receipt.SourceId==sources.Single().UniqueId && control.Context?.MaybeCaster==sources.Single() && victim.Descriptor.State.HasCondition(UnitCondition.Stunned);
                RosterPersistenceCheck("sprint22-cold-control",same,new JObject { ["unit"]=victim.UniqueId,["source"]=receipt.SourceId,["event"]="fresh-process-load",
                    ["owner"]=scope,["prepareRunId"]=receipt.PrepareRunId,["buff"]=receipt.BuffGuid,["savedEndTicks"]=receipt.EndTicks,
                    ["loadedEndTicks"]=control?.EndTime.Ticks,["remainingSeconds"]=control?.TimeLeft.TotalSeconds,
                    ["moduleEnabled"]=_context.FeatureModules.Active.ExpandedSummoning },"Native cold load preserves the same absolute finite expiry and caster, including module-disabled compatibility.");
                if (!same) yield break;
                double started=Game.Instance.Player.GameTime.TotalSeconds;var clock=System.Diagnostics.Stopwatch.StartNew();
                Game.Instance.IsPaused=false;
                while ((victim.Descriptor.Buffs.RawFacts.Contains(control) ||
                    victim.Descriptor.State.HasCondition(UnitCondition.Stunned) ||
                    !victim.Descriptor.State.CanAct) && clock.Elapsed.TotalSeconds<45) yield return 0;
                Game.Instance.IsPaused=true;
                bool controlPresent=victim.Descriptor.Buffs.RawFacts.Contains(control);
                bool stunned=victim.Descriptor.State.HasCondition(UnitCondition.Stunned);
                bool canAct=victim.Descriptor.State.CanAct;
                bool expired=!controlPresent && !stunned && canAct;
                RosterPersistenceCheck("sprint22-cold-natural-expiry",expired,new JObject { ["unit"]=victim.UniqueId,["event"]="finite-native-expiration-after-load",
                    ["owner"]=scope,["controlPresent"]=controlPresent,["stunned"]=stunned,["canAct"]=canAct,
                    ["nativeElapsedSeconds"]=Game.Instance.Player.GameTime.TotalSeconds-started,["wallSeconds"]=clock.Elapsed.TotalSeconds,
                    ["timedOut"]=clock.Elapsed.TotalSeconds>=45 },"Saved finite control naturally expires; loading or module state creates no fresh full duration.");
                if (!expired) yield break;
                var source=sources.Single();
                var skins=source.View.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                bool original=skins.Any(value=>value.sharedMesh!=null && value.sharedMesh.name=="KMG_ankylosaurus_Original");
                bool visual=original==_context.FeatureModules.Active.ExpandedSummoning &&
                    (source.View.GetComponent<KingmakerGunslinger.Summoning.AnkylosaurusTailImpactVisual>()!=null)==_context.FeatureModules.Active.ExpandedSummoning;
                int priorAttack=source.Descriptor.Stats.AdditionalAttackBonus.BaseValue;
                int priorFort=victim.Descriptor.Stats.SaveFortitude.BaseValue;
                var saves=new Sprint19SaveObserver { Target=victim };EventBus.Subscribe(saves);
                var trials=new JArray();bool setup=false;
                try
                {
                    source.Descriptor.Stats.AdditionalAttackBonus.BaseValue=100;victim.Descriptor.Stats.SaveFortitude.BaseValue=-100;
                    var old=victim.Descriptor.Buffs.RawFacts.OfType<Buff>().ToArray();
                    for(int trial=1;trial<=12;trial++)
                    {
                        foreach(Buff buff in victim.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(value=>!old.Contains(value)).ToArray()) buff.Remove();
                        victim.Descriptor.Damage=0;saves.Saves.Clear();
                        var strike=Rulebook.Trigger(new RuleAttackWithWeapon(source,victim,Sprint19Limbs(source).Single(),0));
                        setup=strike.AttackRoll.IsHit && !strike.AttackRoll.IsCriticalConfirmed && (saves.Saves.Count!=1 || !saves.Saves[0].IsPassed);
                        trials.Add(new JObject { ["trial"]=trial,["d20"]=strike.AttackRoll.Roll.Value,["hit"]=strike.AttackRoll.IsHit,
                            ["confirmed"]=strike.AttackRoll.IsCriticalConfirmed,["saves"]=saves.Saves.Count,["preconditionsSatisfied"]=setup });
                        if(setup) break;
                    }
                    bool rider=setup && saves.Saves.Count==1 && victim.Descriptor.State.HasCondition(UnitCondition.Dazed) && !victim.Descriptor.State.HasCondition(UnitCondition.Stunned);
                    RosterPersistenceCheck("sprint22-loaded-module-carriers",visual && rider,new JObject { ["unit"]=source.UniqueId,["target"]=victim.UniqueId,
                        ["event"]="loaded-source-native-strike-and-module-view",["owner"]=source.Blueprint.AssetGuid,
                        ["moduleEnabled"]=_context.FeatureModules.Active.ExpandedSummoning,["originalBody"]=original,
                        ["visualContract"]=visual,["riderContract"]=rider,["trials"]=trials },
                        "Restart-bound module OFF restores the donor view while the saved unit's registered facts retain native mechanics; ON rebinds the original exactly once.");
                }
                finally { EventBus.Unsubscribe(saves);source.Descriptor.Stats.AdditionalAttackBonus.BaseValue=priorAttack;victim.Descriptor.Stats.SaveFortitude.BaseValue=priorFort; }
                victim.Destroy();Game.Instance.EntityDestroyer.Tick();
                for(int frame=0;frame<30;frame++) yield return 0;
                RosterPersistenceCheck("sprint22-victim-retired",!Game.Instance.State.Units.All.Contains(victim),new JObject { ["unit"]=receipt.UnitId,
                    ["event"]="exact-owned-victim-cleanup",["owner"]=scope },"Only the receipt-bound victim is retired before saving the cleaned fixture.");
                yield break;
            }
            RosterPersistenceCheck("sprint22-fresh-control-absence",actors.Length==0,new JObject { ["event"]="fresh-process-after-cleanup",["owner"]=scope,
                ["ownedVictims"]=actors.Length },"Fresh load finds no test victim or source-owned control fixture.");
        }
    }
}
