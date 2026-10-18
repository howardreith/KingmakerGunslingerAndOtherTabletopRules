using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using KingmakerGunslinger.Assets;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private bool IsSprint27Review { get { return _request.Scenario==RuntimeTestScenarioCatalog.DisposableExpandedSummoningSprint27NativeReview; } }
        private IEnumerable<int> ReviewSprint27(ExpandedSummoningCorrectionFixture fixture)
        {
            bool mode=SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue,pause=Game.Instance.IsPaused;
            string phase=RuntimeTestRequestParser.Sprint27ReviewPhase(_request);
            try {
                Sprint25Check("phase",phase!=null,new JObject{["phase"]=phase},"One bounded Griffon phase, never silent fallback.");
                Game.Instance.IsPaused=true;CreateExpandedSummoningCorrectionHostile(fixture);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue=false;Game.Instance.TurnBasedCombatController.Activate();Game.Instance.IsPaused=false;
                for(int i=0;i<16;i++)yield return 0;
                var owner=CastExpandedSummoningOwnTier(fixture,"griffon");SetExpandedSummoningBrainActive(owner,false);
                try {
                    foreach(int f in Sprint26Appearance(owner,"griffon",false))yield return f;
                    Sprint27Body(owner,"idle");
                    if(phase=="rtwp-foundation") {
                        Sprint27Profile(fixture,owner);Sprint27Strength(owner);
                        foreach(int f in Sprint27Ordinary(fixture,owner,false))yield return f;
                        foreach(int f in Sprint27Ordinary(fixture,owner,true))yield return f;
                    } else if(phase=="pounce-rtwp" || phase=="pounce-turn-based") {
                        bool tb=phase=="pounce-turn-based";
                        foreach(int f in Sprint27Pounce(fixture,owner,tb))yield return f;
                        owner.Commands.InterruptAll(true);
                        SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue=false;Game.Instance.TurnBasedCombatController.Activate();
                        foreach(int f in Sprint27Ordinary(fixture,owner,true))yield return f;
                    } else throw new InvalidOperationException("Unimplemented Griffon phase: "+phase);
                } finally {
                    SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue=false;Game.Instance.TurnBasedCombatController.Activate();
                    owner.Commands.InterruptAll(true);if(owner.IsInCombat)owner.LeaveCombat();DisposeExpandedSummoningUnits(fixture.Created,new[]{owner});
                }
            } finally {
                var timing=_sprint25Clock.DescribeTickOwnership();
                Sprint25Check("single-native-frame-tick",(bool)timing["frameSynchronous"] && (int)timing["maximumActionTicksPerUnitPerUnityFrame"]<=1 && (int)timing["maximumAbilityExecutionTicksPerProcessPerUnityFrame"]<=1,timing,"Only exact fixture owners tick once per Unity frame; no protected brain is ticked.");
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue=mode;Game.Instance.TurnBasedCombatController.Activate();Game.Instance.IsPaused=pause;
            }
        }
        private void Sprint27Body(UnitEntityData owner,string state)
        {
            Mesh cache;string[] bones;Texture2D paint;string status;
            bool loaded=PteranodonAssetRuntime.TryGetGriffonVisual(out cache,out bones,out paint,out status);
            var bodies=owner.View.GetComponentsInChildren<SkinnedMeshRenderer>(true).Where(v=>v.enabled&&v.sharedMesh!=null&&v.sharedMesh.name=="KMG_griffon_Original").ToArray();
            var pose=bodies.Length==1?Sprint26Pose(bodies[0]):null;
            bool exact=loaded&&bodies.Length==1&&bodies[0].sharedMesh!=cache&&bodies[0].bones.Length==bones.Length&&ReferenceEquals(bodies[0].sharedMaterial.mainTexture,paint)&&pose!=null&&(bool)pose["finite"]&&(bool)Sprint22SelectionSurface(owner)["selectionSurface"];
            Sprint25Check("body-"+state,exact,new JObject{["unit"]=owner.UniqueId,["assetStatus"]=status,["bodies"]=bodies.Length,["pose"]=pose,
                ["partyCamera"]=WriteExpandedSummoningPartyCameraCapture(owner,_request.EvidenceDirectory,"sprint27-griffon-"+state+"-"+Time.frameCount+".png")},"Actual original Griffon body, native renderer/fader, finite pose and selection; donor fallback cannot pass.");
        }
        private void Sprint27Profile(ExpandedSummoningCorrectionFixture fixture,UnitEntityData owner)
        {
            var limbs=Sprint19Limbs(owner);var attacks=new JArray();bool exact=limbs.Length==5;
            for(int i=0;i<limbs.Length;i++) {
                var stats=Rulebook.Trigger(new RuleCalculateWeaponStats(owner,limbs[i],null));var damage=stats.DamageDescription[0].CreateDamage();
                int attack=Rulebook.Trigger(new RuleCalculateAttackBonusWithoutTarget(owner,limbs[i],0)).Result;
                bool row=!limbs[i].IsSecondary&&attack==(i==0?8:7)&&damage.Bonus==3&&damage.Dice.Rolls==1&&damage.Dice.Dice==(i<3?DiceType.D6:DiceType.D4);
                exact&=row;attacks.Add(new JObject{["weapon"]=limbs[i].Blueprint.AssetGuid,["category"]=limbs[i].Blueprint.Category.ToString(),["secondary"]=limbs[i].IsSecondary,["attack"]=attack,["damage"]=Sprint16DamageLine(damage),["rangeFeet"]=limbs[i].AttackRange.Value,["exact"]=row});
            }
            var cmd=ProbeCombatManeuverDefenceParts(fixture.Hostile,owner,CombatManeuver.Grapple);var trip=ProbeCombatManeuverDefenceParts(fixture.Hostile,owner,CombatManeuver.Trip);
            int cmb=Rulebook.Trigger(new RuleCalculateCMB(owner,fixture.Hostile,CombatManeuver.Grapple)).Result;
            int[] scores={owner.Stats.Strength.ModifiedValue,owner.Stats.Dexterity.ModifiedValue,owner.Stats.Constitution.ModifiedValue,owner.Stats.Intelligence.ModifiedValue,owner.Stats.Wisdom.ModifiedValue,owner.Stats.Charisma.ModifiedValue};
            exact&=scores.SequenceEqual(new[]{16,15,16,5,13,8})&&owner.Descriptor.State.Size==Size.Large&&owner.Stats.BaseAttackBonus.ModifiedValue==5&&owner.Stats.HitPoints.ModifiedValue==42&&owner.Stats.AC.ModifiedValue==17&&owner.Stats.AC.Touch==11&&owner.Stats.AC.FlatFooted==15&&owner.Stats.SaveFortitude.ModifiedValue==7&&owner.Stats.SaveReflex.ModifiedValue==6&&owner.Stats.SaveWill.ModifiedValue==4&&owner.Stats.Speed.ModifiedValue==80&&owner.Stats.SkillMobility.ModifiedValue==10&&owner.Stats.SkillPerception.ModifiedValue==12&&cmb==9&&cmd.Result==21&&trip.Result==25;
            exact&=SummonGrabComponent.Find(owner)==null&&SummonRakeComponent.Find(owner)!=null;
            Sprint25Check("composed-profile",exact,new JObject{["unit"]=owner.UniqueId,["attacks"]=attacks,["scores"]=new JArray(scores),["size"]=owner.Descriptor.State.Size.ToString(),["bab"]=owner.Stats.BaseAttackBonus.ModifiedValue,["hp"]=owner.Stats.HitPoints.ModifiedValue,["ac"]=owner.Stats.AC.ModifiedValue,["touch"]=owner.Stats.AC.Touch,["flat"]=owner.Stats.AC.FlatFooted,["fort"]=owner.Stats.SaveFortitude.ModifiedValue,["ref"]=owner.Stats.SaveReflex.ModifiedValue,["will"]=owner.Stats.SaveWill.ModifiedValue,["speed"]=owner.Stats.Speed.ModifiedValue,["mobility"]=DescribeSprint16Skill(owner.Stats.SkillMobility),["perception"]=DescribeSprint16Skill(owner.Stats.SkillPerception),["cmb"]=cmb,["cmd"]=cmd.Describe("CMD"),["trip"]=trip.Describe("trip"),["hasGrab"]=SummonGrabComponent.Find(owner)!=null,["unitType"]=new JObject{["guid"]=owner.Blueprint.Type.AssetGuid,["name"]=owner.Blueprint.Type.name,["knowledgeStat"]=owner.Blueprint.Type.KnowledgeStat.ToString()},["nativeBlueprintGraph"]=DescribeNativeUnit(owner.Blueprint)},"Composed adult profile with real class/rank/racial/feat/size attribution and no automatic Grab.");
        }
        private void Sprint27Strength(UnitEntityData owner)
        {
            int original=owner.Stats.Strength.BaseValue,additional=owner.Stats.AdditionalDamage.BaseValue;var rows=new JArray();bool exact=true;
            try {
                foreach(int score in new[]{16,17,18,19,9})foreach(int extra in new[]{0,3}) {
                    owner.Stats.Strength.BaseValue=score;owner.Stats.AdditionalDamage.BaseValue=additional+extra;
                    foreach(var weapon in Sprint19Limbs(owner)) {
                        var stats=Rulebook.Trigger(new RuleCalculateWeaponStats(owner,weapon,null));var damage=stats.DamageDescription[0].CreateDamage();
                        bool row=damage.Bonus==owner.Stats.Strength.Bonus+additional+extra;exact&=row;
                        rows.Add(new JObject{["strength"]=owner.Stats.Strength.ModifiedValue,["modifier"]=owner.Stats.Strength.Bonus,["additionalDamage"]=additional+extra,["weapon"]=weapon.Blueprint.AssetGuid,["nativeMultiplier"]=stats.DamageBonusStatMultiplier,["damage"]=Sprint16DamageLine(damage),["damageForms"]=stats.DamageDescription.Count,["exact"]=row});
                    }
                }
            } finally {owner.Stats.Strength.BaseValue=original;owner.Stats.AdditionalDamage.BaseValue=additional;}
            Sprint25Check("signed-strength-stat-calculation",exact,new JObject{["rows"]=rows,["forcedRolls"]=0},"Native final weapon-stat calculations add live signed Strength once at even/odd scores and a penalty, preserving independent damage. These are stat calculations, not AI or command proof.");
        }
        private IEnumerable<int> Sprint27Ordinary(ExpandedSummoningCorrectionFixture fixture,UnitEntityData owner,bool full)
        {
            var target=fixture.Hostile;ResetExpandedSummoningHostile(fixture);owner.JoinCombat();target.JoinCombat();
            Vector3 far;string lane,direction;float reach=5*.3048f,radii=owner.Corpulence+target.Corpulence;
            Vector3 origin=Sprint24AiCombatLane(owner,owner,target,reach+radii+.2f,out far,out lane,out direction);Vector3 forward=(far-origin).normalized;
            PlaceExpandedSummoningUnit(owner,origin);var range=new UnitAttack(target);range.Init(owner);
            PlaceExpandedSummoningUnit(target,origin+forward*(reach+radii-.05f));bool inside=range.IsUnitEnoughClose;
            PlaceExpandedSummoningUnit(target,origin+forward*(reach+radii+.05f));bool outside=range.IsUnitEnoughClose;
            Sprint25Check("reach-"+full,inside&&!outside&&Math.Abs(range.ApproachRadius-reach-radii)<.02f,new JObject{["inside"]=inside,["outside"]=outside,["approachRadius"]=range.ApproachRadius,["radii"]=radii,["lane"]=lane},"Actual melee acquisition has5-foot reach, independently of Large footprint and wings.");
            PlaceExpandedSummoningUnit(target,origin+forward*(radii+.8f));owner.Memory.Add(target);target.Memory.Add(owner);
            var observer=new Sprint24AttackObserver{Owner=owner,Target=target};var costs=new JArray();var endings=new JArray();
            var command=new UnitAttack(target){ForceFullAttack=full,IsSingleAttack=!full};EventBus.Subscribe(observer);
            try {using(var economy=new FirearmReviewCosts(owner,costs))using(var trace=new Sprint26CommandTrace(owner,endings)){
                owner.Commands.Run(command);int frames=0;DateTime deadline=DateTime.UtcNow.AddSeconds(30);
                while(!command.IsFinished&&frames++<3000&&DateTime.UtcNow<deadline)yield return 0;
                int expected=full?3:1;bool exact=command.IsFinished&&(command.Result==UnitCommand.ResultType.Success||trace.Exhausted(command))&&observer.WeaponAttacks.Count==expected&&command.AllAttacks.Count==expected&&economy.Count(command)==1&&economy.Exact(command,false)&&!command.Cutscene&&!command.IsIgnoreCooldown;
                Sprint25Check(full?"ordinary-full-command":"ordinary-single-command",exact,new JObject{["finished"]=command.IsFinished,["result"]=command.Result.ToString(),["planned"]=command.AllAttacks.Count,["weapons"]=new JArray(observer.WeaponAttacks.Select(v=>new JObject{["weapon"]=v.Weapon.Blueprint.AssetGuid,["charge"]=v.IsCharge,["hit"]=v.AttackRoll!=null&&v.AttackRoll.IsHit,["damage"]=v.MeleeDamage==null?0:v.MeleeDamage.Damage})),["nativeEnds"]=endings,["costs"]=costs,["forcedAnimationEvents"]=0},"Genuine ordinary native command resolves its printed attack set once, without rake or fixture attacks.");
                Sprint27Body(owner,full?"full-attack":"single-attack");
            }} finally {EventBus.Unsubscribe(observer);owner.Commands.InterruptAll(true);ResetExpandedSummoningHostile(fixture);}
        }
        private IEnumerable<int> Sprint27Pounce(ExpandedSummoningCorrectionFixture fixture,UnitEntityData owner,bool turnBased)
        {
            var target=fixture.Hostile;var nativeCharge=fixture.Blueprints.OfType<BlueprintAbility>().Single(v=>v.AssetGuid=="c78506dd0e14f7c45a599990e4e65038");
            var granted=owner.Descriptor.Abilities.GetAbility(nativeCharge);
            if(granted==null)throw new InvalidOperationException("Product Griffon has no native Charge carrier; no fixture grant allowed.");
            var data=new AbilityData(granted);Vector3 start;string route;
            var selected=Sprint23FindChargeRoute(fixture,owner,target,data,out start,out route);if(selected==null)throw new InvalidOperationException("No legal native charge lane.");
            owner.JoinCombat();target.JoinCombat();owner.Memory.Add(target);target.Memory.Add(owner);
            if(turnBased){var admission=new JObject();foreach(int f in Sprint22EnrollTurnBasedOwners(new[]{owner},target,admission))yield return f;if(!(bool)admission["admitted"])throw new InvalidOperationException("Exact owner turn missing.");}
            var observer=new Sprint24AttackObserver{Owner=owner,Target=target};var costs=new JArray();EventBus.Subscribe(observer);
            try {using(var economy=new FirearmReviewCosts(owner,costs))using(var queued=new Sprint26QueueTrace(owner)){
                var command=new UnitUseAbility(data,selected);owner.Commands.Run(command);int frames=0;DateTime deadline=DateTime.UtcNow.AddSeconds(40);
                while(frames++<4000&&DateTime.UtcNow<deadline){if(command.ExecutionProcess!=null&&!command.ExecutionProcess.IsEnded)command.ExecutionProcess.Tick();yield return 0;if(command.IsFinished&&queued.Commands.Count>0&&queued.Commands.All(v=>v.IsFinished))break;}
                bool exact=command.IsFinished&&queued.Commands.Count==1&&queued.Commands[0].IsCharge&&queued.Commands[0].IsFinished&&queued.Commands[0].AllAttacks.Count==5&&observer.WeaponAttacks.Count==5&&economy.Count(command)==1&&economy.Exact(command,turnBased)&&!target.Descriptor.State.IsDead;
                Sprint25Check("pounce-"+turnBased,exact,new JObject{["owner"]=owner.UniqueId,["target"]=target.UniqueId,["route"]=route,["finished"]=command.IsFinished,["queued"]=queued.Commands.Count,["planned"]=new JArray(queued.Commands.Select(v=>v.AllAttacks.Count)),["travel"]=Vector3.Distance(start,owner.Position),["weapons"]=new JArray(observer.WeaponAttacks.Select(v=>new JObject{["weapon"]=v.Weapon.Blueprint.AssetGuid,["charge"]=v.IsCharge,["hit"]=v.AttackRoll!=null&&v.AttackRoll.IsHit,["damage"]=v.MeleeDamage==null?0:v.MeleeDamage.Damage})),["costs"]=costs,["forcedAnimationEvents"]=0,["fixtureGrantedCharge"]=false},"A legal native charge yields bite/two talons/two rake attacks against the surviving target with one native action cost, no Grab prerequisite, retry or supplied attack.");
                Sprint27Body(owner,"pounce-"+turnBased);
            }}finally{EventBus.Unsubscribe(observer);owner.Commands.InterruptAll(true);ResetExpandedSummoningHostile(fixture);}
        }
    }
}
