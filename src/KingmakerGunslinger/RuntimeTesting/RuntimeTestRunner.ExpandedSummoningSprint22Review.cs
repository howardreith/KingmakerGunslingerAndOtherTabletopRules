using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.UI.Group;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using System.Reflection;
using Harmony12;
using KingmakerGunslinger.Assets;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;
using TurnBased.Controllers;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private ExpandedSummoningCorrectionFixture _sprint22Fixture;
        private IEnumerator<int> _sprint22Steps;
        private readonly List<RuntimeTestAssertion> _sprint22Assertions = new List<RuntimeTestAssertion>();
        private readonly JArray _sprint22Rows = new JArray();

        private sealed class Sprint22DurationObserver : IGlobalRulebookHandler<RuleRollDice>, IGlobalRulebookHandler<RuleApplyBuff>
        {
            internal UnitEntityData Owner, Target;
            internal readonly List<RuleRollDice> Rolls=new List<RuleRollDice>();
            internal readonly List<RuleApplyBuff> Applications=new List<RuleApplyBuff>();
            public void OnEventAboutToTrigger(RuleRollDice evt) { }
            public void OnEventDidTrigger(RuleRollDice evt)
            { if (ReferenceEquals(evt.Initiator,Owner) && evt.DiceFormula.Rolls==1 && evt.DiceFormula.Dice==DiceType.D4) Rolls.Add(evt); }
            public void OnEventAboutToTrigger(RuleApplyBuff evt) { }
            public void OnEventDidTrigger(RuleApplyBuff evt)
            { if (ReferenceEquals(evt.Initiator,Target)) Applications.Add(evt); }
            internal void Clear() { Rolls.Clear();Applications.Clear(); }
        }

        private void Sprint22Check(string predicate, bool passed, JObject row, string expected)
        {
            row["predicate"]=predicate; row["passed"]=passed; row["runId"]=_request.RunId;
            _sprint22Rows.Add(row);
            _sprint22Assertions.Add(Assertion("sprint22-"+predicate,expected,row.ToString(Newtonsoft.Json.Formatting.None),passed,
                "Request-owned Ankylosaurus and victim; resolved native rules. No result or global RNG mutation."));
            RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory,"sprint22-review-progress.json"),
                BoundaryObservationPolicy.Serialize(_sprint22Rows));
        }

        private bool PollSprint22NativeReview()
        {
            try
            {
                if (_sprint22Steps == null)
                {
                    _sprint22Fixture=BeginExpandedSummoningCorrectionFixture("KMG_Runtime_Sprint22_Caster");
                    _sprint22Steps=ReviewSprint22(_sprint22Fixture).GetEnumerator();
                }
                if (_sprint22Steps.MoveNext()) return false;
            }
            catch (Exception error)
            {
                Sprint22Check("execution",false,new JObject { ["unit"]="ankylosaurus",["event"]="native-review",
                    ["owner"]=_sprint22Fixture?.Caster?.UniqueId,["exception"]=error.ToString() },"Native review completes without exception.");
            }
            CleanupSprint22NativeReview();
            string report=Path.Combine(_request.EvidenceDirectory,"sprint22-review.json");
            RuntimeTestResultWriter.WriteAtomic(report,BoundaryObservationPolicy.Serialize(_sprint22Rows));
            _summoningProfileResult=CreateResult(_sprint22Assertions.All(value=>value.Status==RuntimeTestStatuses.Pass)
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,_sprint22Assertions,null);
            _summoningProfileResult.EvidenceFiles.Add(report);
            return true;
        }

        private void CleanupSprint22NativeReview()
        {
            if (_sprint22Steps != null) { _sprint22Steps.Dispose(); _sprint22Steps=null; }
            if (_sprint22Fixture == null) return;
            bool cleaned;
            EndExpandedSummoningCorrectionFixture(_sprint22Fixture,out cleaned);
            Sprint22Check("fixture-restored",cleaned,new JObject { ["unit"]="ankylosaurus",
                ["owner"]=_sprint22Fixture.Caster?.UniqueId,["event"]="exact-owned-teardown" },"Exact original unit/party/area references restored.");
            _sprint22Fixture=null;
        }

        private IEnumerable<int> ReviewSprint22(ExpandedSummoningCorrectionFixture fixture)
        {
            bool originalMode=Kingmaker.UI.SettingsUI.SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool originalPause=Game.Instance.IsPaused;
            try
            {
                Game.Instance.IsPaused=true;
                Sprint22PlayerRoutes(fixture);
                foreach (var family in new[] { SummonFamily.Monster,SummonFamily.NaturesAlly })
                    foreach (var route in ExpandedSummoningCatalog.GenerateVariants(family).Where(value=>value.Creature.Key=="ankylosaurus"))
                    {
                        UnitEntityData[] units=CastExpandedSummoningVariant(fixture.Blueprints,fixture.Caster,route,null,fixture.Evidence);
                        fixture.Created.AddRange(units);
                        int low=route.Multiplicity==SummonMultiplicity.One ? 1 : route.Multiplicity==SummonMultiplicity.OneD3 ? 1 : 2;
                        int high=route.Multiplicity==SummonMultiplicity.One ? 1 : route.Multiplicity==SummonMultiplicity.OneD3 ? 3 : 5;
                        Sprint22Check("route-"+route.StableKey,units.Length>=low && units.Length<=high &&
                            units.All(value=>value.Blueprint.AssetGuid=="10c80d5cfa594332bd3e5127799e426f"),new JObject {
                                ["unit"]="ankylosaurus",["event"]="registered-ability-execution",["route"]=route.StableKey,
                                ["execution"]=_expandedSummoningLastAbilityExecution,["quantity"]=units.Length,
                                ["units"]=new JArray(units.Select(Sprint22UnitReadback)) },"Existing route produces only its unchanged unit identity and legal quantity.");
                        DisposeExpandedSummoningUnits(fixture.Created,units);
                        for (int frame=0;frame<8;frame++) yield return 0;
                    }
                CreateExpandedSummoningCorrectionHostile(fixture);
                foreach(int frame in Sprint22VisualLifecycle(fixture)) yield return frame;
                foreach(int frame in Sprint22CrowdNavigation(fixture)) yield return frame;
                foreach (bool turnBased in new[] { false,true })
                {
                    Kingmaker.UI.SettingsUI.SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue=turnBased;
                    Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused=false;
                    var owner=CastExpandedSummoningOwnTier(fixture,"ankylosaurus");
                    SetExpandedSummoningBrainActive(owner,false);
                    for (int frame=0;frame<16;frame++) yield return 0;
                    Sprint22ReadProfile(fixture,owner,turnBased);
                    Sprint22PowerAttack(fixture,owner,turnBased);
                    Sprint22NegativeWeapons(fixture,owner,turnBased);
                    DisposeExpandedSummoningUnits(fixture.Created,new[] {owner});
                    foreach (string name in new[] { "ordinary-failure","ordinary-success","critical-failure","critical-success",
                        "unconfirmed-threat","miss","daze-immunity","stun-immunity-ordinary","stun-immunity-critical",
                        "daze-immunity-critical","critical-immunity","absorbed-hit","strength-increase","strength-decrease","duplicate-callback" })
                    {
                        var trialOwner=CastExpandedSummoningOwnTier(fixture,"ankylosaurus");
                        SetExpandedSummoningBrainActive(trialOwner,false);
                        foreach (int frame in Sprint22RiderTrial(fixture,trialOwner,name,turnBased)) yield return frame;
                    }
                    owner=CastExpandedSummoningOwnTier(fixture,"ankylosaurus");SetExpandedSummoningBrainActive(owner,false);
                    foreach (int frame in Sprint22Lifetime(fixture,owner,turnBased)) yield return frame;
                    DisposeExpandedSummoningUnits(fixture.Created,new[] { owner });
                    for (int frame=0;frame<16;frame++) yield return 0;
                }
            }
            finally
            {
                Kingmaker.UI.SettingsUI.SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue=originalMode;
                Game.Instance.TurnBasedCombatController.Activate();Game.Instance.IsPaused=originalPause;
            }
        }

        private void Sprint22PowerAttack(ExpandedSummoningCorrectionFixture fixture,UnitEntityData unit,bool mode)
        {
            var toggle=unit.Descriptor.ActivatableAbilities.Enumerable.Single(value=>value.Blueprint.AssetGuid=="a7b339e4f6ff93a4697df5d7a87ff619");
            bool was=toggle.IsOn;var weapon=Sprint19Limbs(unit).Single();
            try
            {
                toggle.IsOn=false;toggle.Stop(true);
                int offAttack=ProbeEntityAttackBonus(unit,fixture.Hostile,weapon);
                int offDamage=Rulebook.Trigger(new RuleCalculateWeaponStats(unit,weapon,null)).DamageDescription[0].CreateDamage().Bonus;
                toggle.IsOn=true;
                int onAttack=ProbeEntityAttackBonus(unit,fixture.Hostile,weapon);
                int onDamage=Rulebook.Trigger(new RuleCalculateWeaponStats(unit,weapon,null)).DamageDescription[0].CreateDamage().Bonus;
                Sprint22Check("power-attack-"+(mode?"turn-based":"rtwp"),toggle.IsOn && offAttack==14 && offDamage==12 && onAttack==12 && onDamage==18,
                    new JObject { ["unit"]=unit.UniqueId,["event"]="native-power-attack-toggle-and-weapon-rules",["owner"]=toggle.Blueprint.AssetGuid,
                        ["offAttack"]=offAttack,["onAttack"]=onAttack,["offDamage"]=offDamage,["onDamage"]=onDamage,["running"]=toggle.IsRunning },
                    "BAB7 Power Attack contributes -2 attack/+6 to the single primary 1.5x-Strength tail; printed line is measured with it off.");
            }
            finally { toggle.IsOn=false;toggle.Stop(true);toggle.IsOn=was; }
        }

        private void Sprint22NegativeWeapons(ExpandedSummoningCorrectionFixture fixture,UnitEntityData unit,bool mode)
        {
            var target=fixture.Hostile;var before=target.Descriptor.Buffs.RawFacts.OfType<Buff>().ToArray();
            var bite=fixture.Blueprints.OfType<BlueprintItemWeapon>().First(value=>value.Type!=null && value.Type.Category==WeaponCategory.Bite);
            var foreignFeature=fixture.Blueprints.OfType<BlueprintFeature>().Single(value=>value.AssetGuid=="e8a085864606409ba25dc123f14d27a2");
            var added=fixture.Caster.Descriptor.AddFact(foreignFeature);
            int oldFort=target.Descriptor.Stats.GetStat(StatType.SaveFortitude).BaseValue;
            try
            {
                target.Descriptor.Stats.GetStat(StatType.SaveFortitude).BaseValue=-100;
                foreach(bool wrongOwner in new[] {false,true})
                {
                    var source=wrongOwner ? fixture.Caster : unit;
                    var weapon=wrongOwner ? new ItemEntityWeapon(Sprint19Limbs(unit).Single().Blueprint) : new ItemEntityWeapon(bite);
                    int oldAttack=source.Descriptor.Stats.AdditionalAttackBonus.BaseValue;
                    var saves=new Sprint19SaveObserver {Target=target};EventBus.Subscribe(saves);
                    try
                    {
                        source.Descriptor.Stats.AdditionalAttackBonus.BaseValue=100;
                        RuleAttackWithWeapon strike=null;var trials=new JArray();
                        for(int trial=1;trial<=12;trial++)
                        {
                            saves.Saves.Clear();target.Descriptor.Damage=fixture.HostileDamage;
                            strike=Rulebook.Trigger(new RuleAttackWithWeapon(source,target,weapon,0));
                            trials.Add(new JObject { ["trial"]=trial,["d20"]=strike.AttackRoll.Roll.Value,["hit"]=strike.AttackRoll.IsHit });
                            if(strike.AttackRoll.IsHit) break;
                        }
                        bool same=before.SequenceEqual(target.Descriptor.Buffs.RawFacts.OfType<Buff>());
                        Sprint22Check((wrongOwner?"foreign-owner":"other-weapon")+"-"+(mode?"turn-based":"rtwp"),strike.AttackRoll.IsHit && saves.Saves.Count==0 && same,
                            new JObject { ["unit"]=source.UniqueId,["event"]="negative-native-strike",["owner"]=source.Blueprint.AssetGuid,
                                ["weapon"]=weapon.Blueprint.AssetGuid,["trials"]=trials,["saves"]=saves.Saves.Count,["buffReferencesUnchanged"]=same },
                            "A native hit with a foreign weapon or the signature feature on a foreign owner produces no rider or save.");
                    }
                    finally { source.Descriptor.Stats.AdditionalAttackBonus.BaseValue=oldAttack;EventBus.Unsubscribe(saves); }
                }
            }
            finally { fixture.Caster.Descriptor.RemoveFact(added);target.Descriptor.Stats.GetStat(StatType.SaveFortitude).BaseValue=oldFort; }
        }

        private IEnumerable<int> Sprint22VisualLifecycle(ExpandedSummoningCorrectionFixture fixture)
        {
            var native=fixture.Blueprints.OfType<BlueprintUnit>().Single(value=>value.AssetGuid==AnkylosaurusVisualPolicy.DonorGuid);
            var control=Game.Instance.EntityCreator.SpawnUnit(native,fixture.Caster.Position,Quaternion.identity,fixture.Scene);
            fixture.Created.Add(control);SetExpandedSummoningBrainActive(control,false);
            var donor=control.View.GetComponentsInChildren<SkinnedMeshRenderer>(true).Select(value=>value.sharedMesh).ToArray();
            try
            {
                for(int cycle=0;cycle<3;cycle++)
                {
                    int reached=0;UnitEntityData unit;
                    try
                    {
                        if(cycle==0) ExpandedSummoningPteranodonViewPatch.PostSuppressionFaultForTest=()=>{ reached++;throw new InvalidOperationException("Sprint22 request-owned post-swap fault"); };
                        unit=CastExpandedSummoningOwnTier(fixture,"ankylosaurus");
                    }
                    finally { ExpandedSummoningPteranodonViewPatch.PostSuppressionFaultForTest=null; }
                    SetExpandedSummoningBrainActive(unit,false);
                    for(int frame=0;frame<30;frame++) yield return 0;
                    var view=unit.View;var skins=view.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                    var skin=skins.Single(value=>value.name=="hodag");
                    var attachment=ResourceOwnership(typeof(ExpandedSummoningPteranodonViewPatch),"Applied",view);
                    var owned=ExpandedSummoningPteranodonViewPatch.CaptureOriginalFlightOwnedResources(view);
                    var borrowed=attachment==null ? donor : new[] {ResourceField(attachment,"OriginalMesh") as Mesh};
                    var borrowedMaterials=attachment==null ? skin.sharedMaterials : (Material[])ResourceField(attachment,"OriginalMaterials");
                    var controller=skin.GetComponentInParent<Kingmaker.Visual.MaterialEffects.StandardMaterialController>();
                    var driven=controller==null ? null : ExpandedSummoningPteranodonViewPatch.ControllerMaterials(controller);
                    bool original=skin.sharedMesh!=null && skin.sharedMesh.name=="KMG_ankylosaurus_Original";
                    bool bound=cycle==0 ? reached==1 && !original && skin.enabled && donor.Contains(skin.sharedMesh) :
                        reached==0 && original && owned.OfType<Mesh>().Count()==1 && owned.OfType<Material>().Count()==2 &&
                        driven!=null && driven.Contains(skin.sharedMaterial) && EntityFadedIn(unit);
                    Sprint22Check("view-cycle-"+cycle,bound,new JObject { ["unit"]=unit.UniqueId,["event"]="view-fault-recovery-and-native-fader",
                        ["owner"]=unit.Blueprint.AssetGuid,["faultReached"]=reached,["original"]=original,["fadedIn"]=EntityFadedIn(unit),
                        ["owned"]=new JArray(owned.Select(value=>new JObject { ["id"]=value.GetInstanceID(),["name"]=value.name,["type"]=value.GetType().FullName })),
                        ["status"]=ExpandedSummoningPteranodonViewPatch.DescribeView(view) },"Fault preserves native donor; subsequent instances bind one private mesh/two materials through the native fader and material controller.");
                    DisposeExpandedSummoningUnits(fixture.Created,new[] {unit});
                    int waited=0;
                    for(;waited<60;waited++)
                    { Game.Instance.EntityDestroyer.Tick();yield return 0;if(unit.Destroyed && unit.View==null && unit.HoldingState==null && owned.All(value=>value==null)) break; }
                    var caches=RosterImmutableCaches();
                    bool released=unit.Destroyed && unit.View==null && unit.HoldingState==null && owned.All(value=>value==null);
                    bool safe=borrowed.All(value=>value!=null) && borrowedMaterials.All(value=>value!=null) && caches.Count>0;
                    ExpandedSummoningPteranodonViewPatch.ReleaseOriginalFlightView(view);
                    Sprint22Check("view-teardown-"+cycle,released && safe,new JObject { ["unit"]=unit.UniqueId,["event"]="native-OnDestroy-deferred-resource-release",
                        ["owner"]="exact-per-view-resources",["waitedFrames"]=waited,["released"]=released,["borrowedSurvive"]=safe,
                        ["cacheIds"]=new JArray(caches.Select(value=>new JObject { ["id"]=value.Key,["cache"]=value.Value })) },"Native destruction releases captured private objects within 60 frames and preserves borrowed/cache identities; repeat release is harmless.");
                }
                bool isolated=control.View!=null && ExpandedSummoningPteranodonViewPatch.DescribeView(control.View)=="not-attempted" &&
                    donor.SequenceEqual(control.View.GetComponentsInChildren<SkinnedMeshRenderer>(true).Select(value=>value.sharedMesh)) && donor.All(value=>value!=null);
                Sprint22Check("native-hodag-isolation",isolated,new JObject { ["unit"]=control.UniqueId,["event"]="donor-negative-control",
                    ["owner"]=native.AssetGuid,["unchanged"]=isolated },"The unrelated native Hodag keeps its exact renderer/mesh references across faults and recovered Ankylosaurus instances.");
            }
            finally { ExpandedSummoningPteranodonViewPatch.PostSuppressionFaultForTest=null;DisposeExpandedSummoningUnits(fixture.Created,new[] {control}); }
        }

        private IEnumerable<int> Sprint22CrowdNavigation(ExpandedSummoningCorrectionFixture fixture)
        {
            var route=ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly).Single(value=>value.Creature.Key=="ankylosaurus" && value.ParentTier==7);
            UnitEntityData[] units=null;var quantities=new JArray();
            for(int trial=1;trial<=12;trial++)
            {
                units=CastExpandedSummoningVariant(fixture.Blueprints,fixture.Caster,route,null,fixture.Evidence);fixture.Created.AddRange(units);
                quantities.Add(units.Length);if(units.Length==5) break;
                DisposeExpandedSummoningUnits(fixture.Created,units);for(int frame=0;frame<8;frame++) yield return 0;
            }
            if(units.Length!=5) throw new InvalidOperationException("Ankylosaurus crowd quantity-five setup unsatisfied: "+quantities);
            var selected=Game.Instance.UI.SelectionManagerPC.SelectedUnits.ToArray();
            var awake=Game.Instance.State.AwakeUnits.ToArray();bool paused=Game.Instance.IsPaused;
            var parking=units.Select(value=>value.Position).ToArray();
            var markers=units.Select(value=>value.Descriptor.Buffs.RawFacts.OfType<Buff>().Single(
                buff=>ReferenceEquals(buff.Blueprint,BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff))).ToArray();
            var owned=units.SelectMany(value=>ExpandedSummoningPteranodonViewPatch
                .CaptureOriginalFlightOwnedResources(value.View)).ToArray();
            var ends=markers.Select(value=>value.EndTime.TotalSeconds).ToArray();
            try
            {
                Game.Instance.UI.SelectionManagerPC.MultiSelect(units.Select(value=>value.View).ToArray(),false);
                bool selection=units.All(value=>Game.Instance.UI.SelectionManagerPC.SelectedUnits.Contains(value));
                var anchor=AstarPath.active.GetNearest(fixture.Caster.Position);
                if(anchor.node==null || !anchor.node.Walkable)
                    throw new InvalidOperationException("No walkable party floor for the Ankylosaurus crowd review.");
                var graph=AstarPath.active.graphs[(int)anchor.node.GraphIndex] as Pathfinding.IRaycastableGraph;
                if(graph==null) throw new InvalidOperationException("The Ankylosaurus crowd floor cannot verify direct native routes.");
                var party=Game.Instance.Player.Party.Where(value=>value!=null && value.IsInGame).ToArray();
                Pathfinding.NNInfo chosenOrigin=default(Pathfinding.NNInfo),chosenDestination=default(Pathfinding.NNInfo);
                Vector3 chosenCenter=Vector3.zero,chosenHeading=Vector3.zero;
                float chosenHalfLength=0f;int centers=0,routes=0;bool routeFound=false;
                var centerOffsets=(from dx in Enumerable.Range(-30,61) from dz in Enumerable.Range(-30,61)
                    select new Vector3(dx,0f,dz)).OrderBy(value=>value.sqrMagnitude).ToArray();
                foreach(Vector3 centerOffset in centerOffsets)
                    if(!routeFound)
                    {
                        Vector3 requestedCenter=anchor.clampedPosition+centerOffset;
                        var center=AstarPath.active.GetNearest(requestedCenter);
                        if(center.node==null || !center.node.Walkable || center.node.Area!=anchor.node.Area ||
                            center.node.GraphIndex!=anchor.node.GraphIndex || Vector3.Distance(requestedCenter,center.clampedPosition)>.6f ||
                            party.Any(value=>Vector3.Distance(value.Position,center.clampedPosition)<3.5f)) continue;
                        centers++;
                        foreach(Vector3 heading in CompassOffsets)
                        {
                            foreach(float halfLength in new[] {6f,5f,4f,3f,2.5f})
                            {
                                routes++;
                                Vector3 requestedOrigin=center.clampedPosition-heading*halfLength;
                                Vector3 requestedDestination=center.clampedPosition+heading*halfLength;
                                var origin=AstarPath.active.GetNearest(requestedOrigin);
                                var destination=AstarPath.active.GetNearest(requestedDestination);
                                float required=Math.Max(4f,units.Max(value=>value.Corpulence)*2f+1f);
                                bool usable=origin.node!=null && destination.node!=null && origin.node.Walkable && destination.node.Walkable &&
                                    origin.node.Area==anchor.node.Area && destination.node.Area==anchor.node.Area &&
                                    origin.node.GraphIndex==anchor.node.GraphIndex && destination.node.GraphIndex==anchor.node.GraphIndex &&
                                    Vector3.Distance(requestedOrigin,origin.clampedPosition)<=.6f &&
                                    Vector3.Distance(requestedDestination,destination.clampedPosition)<=.6f &&
                                    Vector3.Distance(origin.clampedPosition,destination.clampedPosition)>=required &&
                                    party.All(value=>Vector3.Distance(value.Position,origin.clampedPosition)>=2.5f &&
                                        Vector3.Distance(value.Position,destination.clampedPosition)>=2.5f) &&
                                    Sprint9FlightLineClear(graph,origin,destination);
                                if(!usable) continue;
                                chosenOrigin=origin;chosenDestination=destination;chosenCenter=center.clampedPosition;
                                chosenHeading=heading;chosenHalfLength=halfLength;routeFound=true;break;
                            }
                            if(routeFound) break;
                        }
                    }
                if(!routeFound) throw new InvalidOperationException("No connected direct floor route for the Ankylosaurus crowd: area="+
                    anchor.node.Area+";graph="+anchor.node.GraphIndex+";centers="+centers+";routes="+routes);
                // The native first-level scene is a real, irregular floor rather
                // than an invented arena. Keep all five summons alive and jointly
                // selected, move each Huge body over the same surveyed route, then
                // use two members head-on for native contact/avoidance evidence.
                Game.Instance.IsPaused=false;int frames=0;bool navigated=true;var rows=new JArray();
                for(int i=0;i<units.Length;i++)
                {
                    var unit=units[i];SetExpandedSummoningBrainActive(unit,false);
                    var origin=i%2==0 ? chosenOrigin : chosenDestination;
                    var destination=i%2==0 ? chosenDestination : chosenOrigin;
                    PlaceExpandedSummoningUnit(unit,origin.clampedPosition);Vector3 movementOrigin=unit.Position;
                    if(!Game.Instance.State.AwakeUnits.Contains(unit)) Game.Instance.State.AwakeUnits.Add(unit);
                    for(int settle=0;settle<2;settle++) yield return 0;
                    var move=new UnitMoveTo(destination.clampedPosition,.5f);move.Init(unit);bool canStart=move.CanStart;
                    float maximumSpeed=0f;int maximumContacts=0;var movement=System.Diagnostics.Stopwatch.StartNew();
                    if(canStart) unit.Commands.Run(move);
                    while(canStart && !move.IsFinished && movement.Elapsed.TotalSeconds<20)
                    {
                        var agent=unit.View.MovementAgent;maximumSpeed=Math.Max(maximumSpeed,agent.Velocity.magnitude);
                        maximumContacts=Math.Max(maximumContacts,agent.UnitContacts==null ? 0 : agent.UnitContacts.Count);
                        frames++;yield return 0;
                    }
                    float travel=Vector3.Distance(movementOrigin,unit.Position);
                    float gap=Vector3.Distance(destination.clampedPosition,unit.Position);
                    bool member=canStart && move.IsFinished && unit.Descriptor.State.Size==Size.Huge && travel>=1f && gap<=1.5f && maximumSpeed>.01f;
                    navigated&=member;
                    rows.Add(new JObject { ["unit"]=unit.UniqueId,["size"]=unit.Descriptor.State.Size.ToString(),
                        ["corpulence"]=unit.Corpulence,["origin"]=SurveyVector(movementOrigin),["destination"]=SurveyVector(destination.clampedPosition),
                        ["travel"]=travel,["gap"]=gap,["maxVelocity"]=maximumSpeed,["maximumNativeContacts"]=maximumContacts,
                        ["agentCorpulence"]=unit.View.MovementAgent.Corpulence,["avoidanceDisabled"]=unit.View.MovementAgent.AvoidanceDisabled,
                        ["canStart"]=canStart,["commandFinished"]=move.IsFinished,["wallSeconds"]=movement.Elapsed.TotalSeconds });
                    unit.Commands.InterruptAll();PlaceExpandedSummoningUnit(unit,parking[i]);
                }
                Sprint22Check("huge-crowd-navigation-selection",selection && navigated,new JObject { ["unit"]="ankylosaurus",["event"]="native-five-member-move-and-selection",
                    ["owner"]="request-owned-quantity-group",["quantityTrials"]=quantities,["selection"]=selection,["members"]=rows,["frames"]=frames,
                    ["floorSearch"]=new JObject { ["anchor"]=SurveyVector(anchor.clampedPosition),
                        ["center"]=SurveyVector(chosenCenter),["heading"]=SurveyVector(chosenHeading),["halfLength"]=chosenHalfLength,
                        ["centers"]=centers,["routes"]=routes } },
                    "Five Huge summons remain jointly selectable and every member completes a surveyed native route with its legal footprint.");

                var pair=new[] {units[0],units[1]};var pairStarts=new[] {chosenOrigin.clampedPosition,chosenDestination.clampedPosition};
                var pairEnds=new[] {chosenDestination.clampedPosition,chosenOrigin.clampedPosition};var pairMoves=new UnitMoveTo[2];
                var pairContacts=new int[2];float minimumCenterDistance=float.MaxValue;
                for(int i=0;i<2;i++) { pair[i].Commands.InterruptAll();PlaceExpandedSummoningUnit(pair[i],pairStarts[i]); }
                for(int settle=0;settle<2;settle++) yield return 0;
                bool pairCanStart=true;
                for(int i=0;i<2;i++) { pairMoves[i]=new UnitMoveTo(pairEnds[i],.5f);pairMoves[i].Init(pair[i]);
                    pairCanStart&=pairMoves[i].CanStart;if(pairMoves[i].CanStart) pair[i].Commands.Run(pairMoves[i]); }
                float initialCenterDistance=Vector3.Distance(pair[0].Position,pair[1].Position);
                var collisionClock=System.Diagnostics.Stopwatch.StartNew();int collisionFrames=0;
                while(pairCanStart && collisionClock.Elapsed.TotalSeconds<20)
                {
                    for(int i=0;i<2;i++) { var contacts=pair[i].View.MovementAgent.UnitContacts;
                        pairContacts[i]=Math.Max(pairContacts[i],contacts==null ? 0 : contacts.Count); }
                    minimumCenterDistance=Math.Min(minimumCenterDistance,Vector3.Distance(pair[0].Position,pair[1].Position));
                    collisionFrames++;if(pairMoves.All(value=>value.IsFinished)) break;yield return 0;
                }
                bool collision=pairCanStart && pairContacts.Any(value=>value>0) &&
                    initialCenterDistance-minimumCenterDistance>=1f && pair.All(value=>!value.View.MovementAgent.AvoidanceDisabled &&
                        value.Corpulence>0 && Math.Abs(value.Corpulence-value.View.MovementAgent.Corpulence)<.001f);
                Sprint22Check("huge-collision-contact",collision,new JObject { ["unit"]="ankylosaurus",["event"]="head-on-native-movement-contacts",
                    ["owner"]="request-owned-quantity-group-members-0-1",["members"]=new JArray(pair.Select((value,i)=>new JObject {
                        ["unit"]=value.UniqueId,["contacts"]=pairContacts[i],["commandFinished"]=pairMoves[i].IsFinished,
                        ["travel"]=Vector3.Distance(pairStarts[i],value.Position),["agentCorpulence"]=value.View.MovementAgent.Corpulence })),
                    ["canStart"]=pairCanStart,["initialCenterDistance"]=initialCenterDistance,["minimumCenterDistance"]=minimumCenterDistance,
                    ["finalCenterDistance"]=Vector3.Distance(pair[0].Position,pair[1].Position),["frames"]=collisionFrames,
                    ["wallSeconds"]=collisionClock.Elapsed.TotalSeconds },
                    "Two Huge members approach head-on on the surveyed route, produce native contacts and retain avoidance/corpulence.");
                for(int i=0;i<units.Length;i++) { units[i].Commands.InterruptAll();PlaceExpandedSummoningUnit(units[i],parking[i]); }
                var expiry=System.Diagnostics.Stopwatch.StartNew();
                while(units.Any(value=>!value.Destroyed || value.View!=null || value.HoldingState!=null) && expiry.Elapsed.TotalSeconds<150)
                { Game.Instance.EntityDestroyer.Tick();yield return 0; }
                bool gone=units.All(value=>value.Destroyed && value.View==null && value.HoldingState==null) && owned.All(value=>value==null);
                Sprint22Check("native-quantity-expiry",gone,new JObject { ["unit"]="ankylosaurus",["event"]="natural-summon-timer-expiry",["owner"]="five-native-summon-timers",
                    ["endSeconds"]=new JArray(ends),["nativeNow"]=Game.Instance.Player.GameTime.TotalSeconds,["gone"]=gone,["wallSeconds"]=expiry.Elapsed.TotalSeconds },
                    "All five native summon timers expire naturally; units, views, holding state and captured private resources disappear without timer mutation.");
            }
            finally
            {
                foreach(var unit in units) { if(!awake.Contains(unit)) Game.Instance.State.AwakeUnits.Remove(unit);unit.Commands.InterruptAll(); }
                Game.Instance.UI.SelectionManagerPC.MultiSelect(selected.Where(value=>value.View!=null).Select(value=>value.View).ToArray(),false);
                Game.Instance.IsPaused=paused;DisposeExpandedSummoningUnits(fixture.Created,units);
            }
        }

        private static JObject Sprint22UnitReadback(UnitEntityData unit)
        {
            return new JObject { ["uid"]=unit.UniqueId,["blueprint"]=unit.Blueprint.AssetGuid,
                ["size"]=unit.Descriptor.State.Size.ToString(),["canAct"]=unit.Descriptor.State.CanAct,
                ["facts"]=new JArray(unit.Descriptor.Progression.Features.Enumerable.Select(value=>value.Blueprint.name)),
                ["buffs"]=new JArray(unit.Descriptor.Buffs.RawFacts.OfType<Buff>().Select(value=>new JObject {
                    ["guid"]=value.Blueprint.AssetGuid,["name"]=value.Blueprint.name,["endSeconds"]=value.EndTime.TotalSeconds,
                    ["leftSeconds"]=value.TimeLeft.TotalSeconds })) };
        }

        private void Sprint22PlayerRoutes(ExpandedSummoningCorrectionFixture fixture)
        {
            object levelController=null;
            var castRule=typeof(RuleCastSpell).GetMethod("OnTrigger");
            var spawnAction=typeof(ContextActionSpawnMonster).GetMethod("RunAction");
            const BindingFlags statics=BindingFlags.NonPublic|BindingFlags.Static;
            var hooks=HarmonyInstance.Create("kmg.sprint22.routes."+_request.RunId);
            try
            {
                fixture.Caster.Descriptor.Stats.Intelligence.BaseValue=30;
                var wizard=BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                    "ba34257984f4c41408ce1dc2004e342e","native Wizard targeted spellbook path");
                AdvanceDisposableSpellcaster(fixture.Caster.Descriptor,wizard,20,ref levelController);
                var book=fixture.Caster.Descriptor.GetSpellbook(wizard);
                while(book.CasterLevel<20) book.AddCasterLevel();book.UpdateAllSlotsSize(false);book.Rest();
                var parents=ExpandedSummoningInventoryObserver.CanonicalParentGuids.Select(guid=>
                    BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,guid,"canonical summon parent")).ToArray();
                hooks.Patch(castRule,null,new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("ExpandedSummoningPlayerPathRuleCastPostfix",statics)),null);
                hooks.Patch(spawnAction,new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("ExpandedSummoningPlayerPathSpawnPrefix",statics)),null,null);
                _expandedSummoningPlayerPathCaptureActive=true;
                var routes=ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster).Concat(ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly))
                    .Where(value=>value.Creature.Key=="ankylosaurus").ToArray();
                if(routes.Length!=10) throw new InvalidOperationException("Ankylosaurus published route set changed.");
                foreach(var route in routes)
                {
                    var before=SnapshotReferences(fixture.AllUnits);
                    var cases=new List<ExpandedSummoningPlayerPathCase>();
                    AddExpandedSummoningPlayerPathRootCase(cases,fixture.Blueprints,fixture.Caster,book,parents,route,
                        ExpandedSummoningPlayerPathAlignmentFor(route),route.Family==SummonFamily.NaturesAlly
                            ? (SummonAlignmentMode?)SummonAlignmentMode.Caster : SummonAlignmentMode.Celestial,
                        fixture.SceneEntities,fixture.AllUnits,route.StableKey);
                    var item=cases.Single();var durations=ExpandedSummoningRuleDurationCapture.ToArray();
                    bool duration=durations.Length==item.LiveCount && durations.All(value=>Math.Abs(value.BaseDuration.TotalSeconds-120d)<=.001 &&
                        value.BonusDuration.TotalSeconds>=0 && value.Unit!=null && value.Unit.Destroyed);
                    bool exact=item.LiveContract && item.SlotContract && item.CommandStarted && item.CommandResult=="Success" &&
                        item.RuleCastCount==1 && item.SpawnActionCount==1 && item.QuantityContract && duration && before.SequenceEqual(SnapshotReferences(fixture.AllUnits));
                    Sprint22Check("player-route-"+route.StableKey,exact,new JObject { ["unit"]="ankylosaurus",["event"]="native-parent-selected-variant-spellbook",
                        ["owner"]=route.StableKey,["detail"]=item.Describe(),["baseDurations"]=new JArray(durations.Select(value=>value.BaseDuration.TotalSeconds)),
                        ["slotContract"]=item.SlotContract,["quantityContract"]=item.QuantityContract,["durationContract"]=duration },
                        "Each existing selected parent consumes one native spellbook slot and one cast/spawn action, preserves template and quantity, and carries CL20 duration with exact cleanup.");
                }
            }
            finally
            {
                _expandedSummoningPlayerPathCaptureActive=false;hooks.UnpatchAll(hooks.Id);ExpandedSummoningPlayerPathEvents.Clear();
                if(levelController!=null) levelController.GetType().GetMethod("Cancel").Invoke(levelController,null);
            }
        }

        private void Sprint22ReadProfile(ExpandedSummoningCorrectionFixture fixture,UnitEntityData owner,bool turnBased)
        {
            var stats=owner.Descriptor.Stats; var limbs=Sprint19Limbs(owner);
            int[] scores=new[] { StatType.Strength,StatType.Dexterity,StatType.Constitution,StatType.Intelligence,StatType.Wisdom,StatType.Charisma }
                .Select(value=>stats.GetStat(value).ModifiedValue).ToArray();
            int difficulty=stats.AC.Modifiers.Where(value=>value.ModDescriptor==ModifierDescriptor.Difficulty).Sum(value=>value.ModValue);
            // Improved Bull Rush also supplies +2 defense against bull rush;
            // use grapple for the generic printed CMD, then name that feat's
            // separate native maneuver terms rather than subtracting them.
            var cmd=ProbeCombatManeuverDefenceParts(fixture.Hostile,owner,CombatManeuver.Grapple);
            var trip=ProbeCombatManeuverDefenceParts(fixture.Hostile,owner,CombatManeuver.Trip);
            int cmb=Rulebook.Trigger(new RuleCalculateCMB(owner,fixture.Hostile,CombatManeuver.Trip)).Result;
            var weaponRows=new JArray(); bool weaponExact=limbs.Length==1;
            foreach (var weapon in limbs)
            {
                var rule=Rulebook.Trigger(new RuleCalculateWeaponStats(owner,weapon,null));
                BaseDamage damage=rule.DamageDescription[0].CreateDamage(); int attack=ProbeEntityAttackBonus(owner,fixture.Hostile,weapon);
                weaponExact &= damage.Dice.Rolls==3 && (int)damage.Dice.Dice==6 && damage.Bonus==12 && attack==14 && !rule.SecondaryWeapon;
                weaponRows.Add(new JObject { ["guid"]=weapon.Blueprint.AssetGuid,["type"]=weapon.Blueprint.Type.AssetGuid,
                    ["attack"]=attack,["damage"]=Sprint16DamageLine(damage),["secondary"]=rule.SecondaryWeapon });
            }
            int deniedDex=cmd.DexterityDenied || cmd.FlatFooted ? stats.Dexterity.Bonus : 0;
            bool profile=scores.SequenceEqual(new[] {27,10,17,2,13,8}) && owner.Descriptor.State.Size==Size.Huge &&
                owner.Descriptor.Progression.CharacterLevel==10 && stats.HitPoints.ModifiedValue==75 &&
                stats.AC.ModifiedValue-difficulty==22 && stats.AC.Touch-difficulty==8 && stats.AC.FlatFooted-difficulty==22 &&
                stats.GetStat(StatType.SaveFortitude).ModifiedValue==12 && stats.GetStat(StatType.SaveReflex).ModifiedValue==7 &&
                stats.GetStat(StatType.SaveWill).ModifiedValue==4 && stats.GetStat(StatType.SkillPerception).ModifiedValue==14 &&
                stats.BaseAttackBonus.ModifiedValue==7 && cmb==17 && cmd.Result+deniedDex-difficulty==27 && trip.Result-cmd.Result==4 && weaponExact;
            profile &= stats.Reach.ModifiedValue==15 && stats.Speed.ModifiedValue==30 &&
                Math.Abs(limbs.Single().AttackRange.Meters-4.572f)<.002f;
            Sprint22Check("profile-"+(turnBased ? "turn-based":"rtwp"),profile,new JObject {
                ["unit"]=owner.UniqueId,["event"]="native-carrier-readback",["owner"]=owner.Blueprint.AssetGuid,
                ["scores"]=new JArray(scores),["hd"]=owner.Descriptor.Progression.CharacterLevel,["hp"]=stats.HitPoints.ModifiedValue,
                ["ac"]=stats.AC.ModifiedValue,["touch"]=stats.AC.Touch,["flat"]=stats.AC.FlatFooted,["difficulty"]=difficulty,
                ["fort"]=stats.GetStat(StatType.SaveFortitude).ModifiedValue,["reflex"]=stats.GetStat(StatType.SaveReflex).ModifiedValue,
                ["will"]=stats.GetStat(StatType.SaveWill).ModifiedValue,["perception"]=DescribeSprint16Skill(stats.GetStat(StatType.SkillPerception)),
                ["bab"]=stats.BaseAttackBonus.ModifiedValue,["cmb"]=cmb,["cmd"]=cmd.Describe("CMD"),["tripCmd"]=trip.Describe("trip"),
                ["reachFeet"]=stats.Reach.ModifiedValue,["speedFeet"]=stats.Speed.ModifiedValue,
                ["weaponRangeMeters"]=limbs.Single().AttackRange.Meters,["nativeCorpulenceMeters"]=owner.Corpulence,
                ["limbs"]=weaponRows,["carrierFacts"]=Sprint22UnitReadback(owner) },"Adult Huge profile and one 3d6+12 tail +14, native feats and exact Perception ranks.");
            Mesh mesh;string[] names;Texture2D albedo;string status;
            bool loaded=PteranodonAssetRuntime.TryGetAnkylosaurusVisual(out mesh,out names,out albedo,out status);
            var skins=owner.View==null ? new SkinnedMeshRenderer[0] : owner.View.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            var original=skins.Where(value=>value.sharedMesh!=null && value.sharedMesh.name=="KMG_ankylosaurus_Original").ToArray();
            string supportingFrame=original.Length==1 ? CaptureSprint17OriginalBodyFrame(owner,original[0],"sprint22-ankylosaurus-"+(turnBased?"turn-based":"rtwp")) : "no-original-renderer";
            Sprint22Check("body-"+(turnBased ? "turn-based":"rtwp"),loaded && original.Length==1 && original[0].enabled &&
                original[0].bones.Length==24 && original[0].sharedMesh!=mesh && original[0].sharedMaterial.mainTexture==albedo,
                new JObject { ["unit"]=owner.UniqueId,["event"]="owned-view-bind",["owner"]=owner.Blueprint.AssetGuid,
                    ["status"]=status,["supportingFrame"]=supportingFrame,["skins"]=new JArray(skins.Select(value=>new JObject { ["name"]=value.name,["enabled"]=value.enabled,
                        ["mesh"]=value.sharedMesh?.name,["meshId"]=value.sharedMesh==null ? 0 : value.sharedMesh.GetInstanceID(),
                        ["materialId"]=value.sharedMaterial==null ? 0 : value.sharedMaterial.GetInstanceID(),
                        ["bones"]=new JArray(value.bones.Select(bone=>bone?.name)) })) },"One private original body on 24 exact actor-owned drivers with borrowed cached albedo.");
        }

        private IEnumerable<int> Sprint22RiderTrial(ExpandedSummoningCorrectionFixture fixture,UnitEntityData owner,string name,bool turnBased)
        {
            var target=fixture.Hostile; var weapon=Sprint19Limbs(owner).Single();
            var observer=new Sprint16RuleObserver { Owner=owner,Target=target };
            var saves=new Sprint19SaveObserver { Target=target }; var strikes=new List<RuleAttackWithWeapon>();
            var duration=new Sprint22DurationObserver { Owner=owner,Target=target };
            var contacts=new JArray();UnitAttack contactCommand=null;
            observer.ObserveWeaponContact=value=>{
                strikes.Add(value);var row=Sprint22TailContact(owner,target,value,contactCommand);
                if(name=="ordinary-failure")
                {
                    var skin=owner.View.GetComponentsInChildren<SkinnedMeshRenderer>(true).SingleOrDefault(v=>v.sharedMesh!=null && v.sharedMesh.name=="KMG_ankylosaurus_Original");
                    if(skin!=null) row["supportingFrame"]=CaptureSprint17OriginalBodyFrame(owner,skin,"sprint22-tail-contact-"+(turnBased?"turn-based":"rtwp"));
                }
                contacts.Add(row);
            };
            int oldAttack=owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue;
            int oldFort=target.Descriptor.Stats.GetStat(StatType.SaveFortitude).BaseValue;
            int oldStrength=owner.Descriptor.Stats.Strength.BaseValue;
            bool critical=name.Contains("critical") && name!="unconfirmed-threat" && name!="critical-immunity";
            bool success=name=="ordinary-success" || name=="critical-success";
            bool dazeImmune=name=="daze-immunity" || name=="daze-immunity-critical";
            bool stunImmune=name=="stun-immunity-ordinary" || name=="stun-immunity-critical";
            var beforeBuffs=target.Descriptor.Buffs.RawFacts.OfType<Buff>().ToArray();
            BlueprintBuff shieldBlueprint=null; Buff shield=null;
            EventBus.Subscribe(observer);EventBus.Subscribe(saves);EventBus.Subscribe(duration);
            try
            {
                if (dazeImmune) target.Descriptor.State.AddConditionImmunity(UnitCondition.Dazed);
                if (stunImmune) target.Descriptor.State.AddConditionImmunity(UnitCondition.Stunned);
                if (name=="absorbed-hit")
                {
                    shieldBlueprint=ScriptableObject.CreateInstance<BlueprintBuff>();shieldBlueprint.name="KMG_Runtime_Sprint22_Absorber";
                    shieldBlueprint.ComponentsArray=new BlueprintComponent[] { new AddDamageResistancePhysical { Value=1000 } };
                    shield=target.Descriptor.AddBuff(shieldBlueprint,target,TimeSpan.FromMinutes(5));
                }
                if (name=="critical-immunity")
                {
                    shieldBlueprint=ScriptableObject.CreateInstance<BlueprintBuff>();shieldBlueprint.name="KMG_Runtime_Sprint22_CriticalImmunity";
                    shieldBlueprint.ComponentsArray=new BlueprintComponent[] { new AddImmunityToCriticalHits() };
                    shield=target.Descriptor.AddBuff(shieldBlueprint,target,TimeSpan.FromMinutes(5));
                }
                if (name=="strength-increase") owner.Descriptor.Stats.Strength.BaseValue=oldStrength+4;
                if (name=="strength-decrease") owner.Descriptor.Stats.Strength.BaseValue=oldStrength-8;
                target.Descriptor.Stats.GetStat(StatType.SaveFortitude).BaseValue=success ? 100 : -100;
                owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue=name=="miss" || name=="unconfirmed-threat" ? -100 : 100;
                var trials=new JArray(); bool satisfied=false; int bound=critical || name=="unconfirmed-threat" || name=="critical-immunity" ? 120 : 12;
                RuleAttackRoll roll=null; UnitAttack command=null; Buff[] effects=new Buff[0];
                for (int trial=1;trial<=bound;trial++)
                {
                    foreach (Buff buff in target.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(value=>!beforeBuffs.Contains(value) && value!=shield).ToArray()) buff.Remove();
                    target.Descriptor.Damage=fixture.HostileDamage;
                    target.Commands.InterruptAll(); owner.Commands.InterruptAll();
                    PlaceExpandedSummoningUnit(owner,fixture.Caster.Position+Vector3.right*4f);
                    PlaceExpandedSummoningUnit(target,owner.Position+Vector3.forward*3f);
                    observer.Clear(); saves.Saves.Clear();strikes.Clear();contacts.Clear();duration.Clear();
                    if (CombatController.IsInTurnBasedCombat())
                        foreach (int frame in Sprint18AdvanceRound(owner)) yield return frame;
                    foreach (int frame in Sprint18RunFullAttack(fixture,owner,result=>{ command=result;contactCommand=result; })) yield return frame;
                    roll=observer.Attacks.SingleOrDefault();
                    effects=target.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(value=>value.Blueprint.name=="KMG_Summoning_Special_Ankylosaurus_Dazed" ||
                        value.Blueprint.name=="KMG_Summoning_Special_Ankylosaurus_Stunned").ToArray();
                    bool expectedHit=name!="miss";
                    satisfied=command!=null && command.IsFinished && roll!=null && roll.IsHit==expectedHit &&
                        roll.IsCriticalConfirmed==critical && (name!="unconfirmed-threat" || roll.IsCriticalRoll) &&
                        (name!="critical-immunity" || roll.Roll.Value==20 && roll.ImmuneToCriticalHit) &&
                        (name=="miss" || saves.Saves.Count!=1 || saves.Saves[0].IsPassed==success);
                    trials.Add(new JObject { ["trial"]=trial,["preconditionsSatisfied"]=satisfied,["commandFinished"]=command?.IsFinished,
                        ["rolls"]=new JArray(observer.Attacks.Select(value=>new JObject { ["d20"]=value.Roll.Value,["hit"]=value.IsHit,
                            ["threat"]=value.IsCriticalRoll,["confirmed"]=value.IsCriticalConfirmed,["criticalImmune"]=value.ImmuneToCriticalHit })),
                        ["saves"]=new JArray(saves.Saves.Select(value=>new JObject { ["d20"]=value.D20.Value,["dc"]=value.DifficultyClass,
                            ["stat"]=value.StatType.ToString(),["passed"]=value.IsPassed })),
                        ["damage"]=new JArray(observer.Damage.Select(value=>value.Damage)) });
                    if (satisfied) break;
                    // Only a dice/setup precondition may retry. A satisfied
                    // trial is evaluated exactly once below and never rerun.
                    for (int frame=0;frame<8;frame++) yield return 0;
                    owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue=oldAttack;
                    owner.Descriptor.Stats.Strength.BaseValue=oldStrength;
                    DisposeExpandedSummoningUnits(fixture.Created,new[] {owner});
                    owner=CastExpandedSummoningOwnTier(fixture,"ankylosaurus");SetExpandedSummoningBrainActive(owner,false);
                    observer.Owner=owner;duration.Owner=owner;
                    owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue=name=="miss" || name=="unconfirmed-threat" ? -100 : 100;
                    if (name=="strength-increase") owner.Descriptor.Stats.Strength.BaseValue=oldStrength+4;
                    if (name=="strength-decrease") owner.Descriptor.Stats.Strength.BaseValue=oldStrength-8;
                }
                int dc=AnkylosaurusRulesPolicy.DifficultyClass(10,owner.Descriptor.Stats.Strength.Bonus);
                bool immune=critical ? stunImmune : dazeImmune;
                bool expectedCondition=name!="miss" && !success && !immune;
                bool condition=target.Descriptor.State.HasCondition(critical ? UnitCondition.Stunned : UnitCondition.Dazed);
                bool other=target.Descriptor.State.HasCondition(critical ? UnitCondition.Dazed : UnitCondition.Stunned);
                int expectedRolls=critical && name!="miss" && !success ? 1 : 0;
                bool exact=satisfied && condition==expectedCondition && !other &&
                    effects.Length==(name=="miss" || success ? 0 : 1) &&
                    (name!="absorbed-hit" || observer.Damage.Count==1 && observer.Damage[0].Damage==0) &&
                    (name=="miss" ? saves.Saves.Count==0 : saves.Saves.Count==1 && saves.Saves[0].StatType==StatType.SaveFortitude && saves.Saves[0].DifficultyClass==dc);
                exact &= duration.Rolls.Count==expectedRolls && duration.Rolls.All(value=>value.Result.Value>=1 && value.Result.Value<=4);
                int expectedApplications=name!="miss" && !success ? 1 : 0;
                var impact=owner.View.GetComponent<AnkylosaurusTailImpactVisual>();
                exact &= impact!=null && impact.Strikes==(name=="miss" ? 0 : 1);
                exact &= duration.Applications.Count==expectedApplications && duration.Applications.All(value=>value.Duration.HasValue &&
                    Math.Abs(value.Duration.Value.TotalSeconds-(critical ? duration.Rolls.Single().Result.Value*6d : 6d))<.001);
                var lifetimes=new JArray(effects.Select(value=>new JObject { ["buff"]=value.Blueprint.AssetGuid,
                    ["source"]=value.Context?.MaybeCaster?.UniqueId,["endSeconds"]=value.EndTime.TotalSeconds,["leftSeconds"]=value.TimeLeft.TotalSeconds }));
                if (name=="duplicate-callback" && satisfied)
                {
                    var handler=owner.Descriptor.Progression.Features.Enumerable.SelectMany(value=>value.SelectComponents<AnkylosaurusTailControl>()).Single();
                    double end=effects.Single().EndTime.TotalSeconds;
                    handler.OnEventDidTrigger(strikes.Single());handler.OnEventDidTrigger(strikes.Single());
                    exact &= saves.Saves.Count==1 && effects.Single().EndTime.TotalSeconds==end &&
                        target.Descriptor.Buffs.RawFacts.OfType<Buff>().Count(value=>effects.Contains(value))==1 && impact.Strikes==1;
                }
                Sprint22Check("rider-"+name+"-"+(turnBased ? "turn-based":"rtwp"),exact,new JObject {
                    ["unit"]=owner.UniqueId,["target"]=target.UniqueId,["owner"]=owner.Blueprint.AssetGuid,["event"]=name,
                    ["mode"]=turnBased ? "turn-based":"rtwp",["trials"]=trials,["precondition"]=satisfied ? "SATISFIED":"UNSATISFIED",
                    ["dc"]=dc,["expectedCondition"]=expectedCondition,["condition"]=condition,["otherCondition"]=other,["effects"]=lifetimes,
                    ["dazeImmune"]=dazeImmune,["stunImmune"]=stunImmune,["contacts"]=contacts,
                    ["impactCues"]=impact?.Strikes,["impactTarget"]=impact?.LastTarget,
                    ["nativeDurationD4"]=new JArray(duration.Rolls.Select(value=>value.Result.Value)),
                    ["buffApplications"]=new JArray(duration.Applications.Select(value=>new JObject { ["durationSeconds"]=value.Duration?.TotalSeconds,
                        ["canApply"]=value.CanApply,["immune"]=value.Immunity,["storyImmune"]=value.StoryModeImmunity })) },"One native Fortitude save per legal tail hit; mutually exclusive finite condition, separate native immunity and no positive-damage prerequisite.");
                if (name=="ordinary-failure")
                    Sprint22Check("tail-contact-"+(turnBased ? "turn-based":"rtwp"),contacts.Count==1 &&
                        (bool?)contacts[0]["nativeTailAction"]==true && (double?)contacts[0]["gapMeters"]<=.5,
                        new JObject { ["unit"]=owner.UniqueId,["target"]=target.UniqueId,["event"]="real-tail-strike-contact",
                            ["owner"]=owner.Blueprint.AssetGuid,["contacts"]=contacts },"Acted native tail animation puts terminal-club surface within 0.5 m of native target bounds at its real rule event.");
                if (!satisfied) throw new InvalidOperationException("unit="+owner.UniqueId+";predicate="+name+";event=dice-preconditions;owner="+owner.Blueprint.AssetGuid);
                if (!exact) throw new InvalidOperationException("unit="+owner.UniqueId+";predicate="+name+";event=postcondition;owner="+owner.Blueprint.AssetGuid);
                if (name=="critical-failure")
                {
                    double nativeStarted=Game.Instance.TimeController.GameTime.TotalSeconds;
                    var clock=System.Diagnostics.Stopwatch.StartNew();
                    while (target.Descriptor.Buffs.RawFacts.OfType<Buff>().Any(value=>effects.Contains(value)) && clock.Elapsed.TotalSeconds<45)
                    {
                        Game.Instance.IsPaused=false;
                        if (CombatController.IsInTurnBasedCombat())
                        { var turn=Game.Instance.TurnBasedCombatController.CurrentTurn;if (turn!=null) Sprint18EndTurn(turn); }
                        yield return 0;
                    }
                    bool expired=effects.All(value=>!target.Descriptor.Buffs.RawFacts.Contains(value)) && !target.Descriptor.State.HasCondition(UnitCondition.Stunned) && target.Descriptor.State.CanAct;
                    UnitAttack returned=null;
                    foreach(int frame in Sprint18RunFullAttack(fixture,target,value=>returned=value,fixture.Caster)) yield return frame;
                    Sprint22Check("critical-natural-expiry-"+(turnBased ? "turn-based":"rtwp"),expired && returned!=null && returned.IsFinished,
                        new JObject { ["unit"]=target.UniqueId,["event"]="native-stun-expiration-and-action",["owner"]="finite-victim-condition",
                            ["expired"]=expired,["actionFinished"]=returned?.IsFinished,["nativeElapsedSeconds"]=Game.Instance.TimeController.GameTime.TotalSeconds-nativeStarted,
                            ["wallSeconds"]=clock.Elapsed.TotalSeconds },"Critical stun expires naturally and native victim actions return.");
                }
            }
            finally
            {
                EventBus.Unsubscribe(observer);EventBus.Unsubscribe(saves);EventBus.Unsubscribe(duration);
                owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue=oldAttack;
                owner.Descriptor.Stats.Strength.BaseValue=oldStrength;
                target.Descriptor.Stats.GetStat(StatType.SaveFortitude).BaseValue=oldFort;
                if (dazeImmune) target.Descriptor.State.RemoveConditionImmunity(UnitCondition.Dazed);
                if (stunImmune) target.Descriptor.State.RemoveConditionImmunity(UnitCondition.Stunned);
                foreach (Buff buff in target.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(value=>!beforeBuffs.Contains(value)).ToArray()) buff.Remove();
                if (shieldBlueprint!=null) UnityEngine.Object.Destroy(shieldBlueprint);
                DisposeExpandedSummoningUnits(fixture.Created,new[] {owner});
            }
        }

        private static JObject Sprint22TailContact(UnitEntityData owner,UnitEntityData target,RuleAttackWithWeapon rule,UnitAttack command)
        {
            var animation=Sprint17IssuedAnimationObservation(command);
            var row=new JObject { ["event"]="RuleAttackWithWeapon.OnEventDidTrigger",["frame"]=Time.frameCount,
                ["unit"]=owner.UniqueId,["target"]=target.UniqueId,["weapon"]=rule.Weapon.Blueprint.AssetGuid,
                ["hit"]=rule.AttackRoll!=null && rule.AttackRoll.IsHit,["issuedAnimation"]=animation,
                ["nativeTailAction"]=(string)animation["actionName"]=="HodagAnimset_Tail" && (string)animation["clip"]?["name"]=="Hodag_attack_tail" && (bool?)animation["handleActed"]==true };
            var skin=owner.View.GetComponentsInChildren<SkinnedMeshRenderer>(true).SingleOrDefault(value=>value.sharedMesh!=null && value.sharedMesh.name=="KMG_ankylosaurus_Original");
            var victim=target.View.GetComponentsInChildren<SkinnedMeshRenderer>(true).Where(value=>value.enabled && value.sharedMesh!=null)
                .OrderByDescending(value=>value.bones.Length).FirstOrDefault();
            if (skin==null || victim==null) { row["failure"]="missing exact original body or native victim skin";return row; }
            int club=Array.FindIndex(skin.bones,value=>value.name=="Tail5_M");
            var world=Sprint17OriginalWorldVertices(skin);var weights=skin.sharedMesh.boneWeights;var original=skin.sharedMesh.vertices;
            var points=world.Where((value,index)=>weights[index].boneIndex0==club && weights[index].weight0>.9f && Math.Abs(original[index].x)>.12).ToArray();
            if (points.Length==0) { row["failure"]="no terminal club surface";return row; }
            Vector3 nearest=points.OrderBy(value=>Vector3.Distance(value,victim.bounds.ClosestPoint(value))).First();
            row["gapMeters"]=Vector3.Distance(nearest,victim.bounds.ClosestPoint(nearest));row["clubPoints"]=points.Length;
            row["clubSurface"]=SurveyVector(nearest);row["targetBounds"]=SurveyVector(victim.bounds.size);
            row["nativeAnimationOnly"]=true;row["originalVertexOwnership"]=skin.sharedMesh.GetInstanceID();
            return row;
        }

        private IEnumerable<int> Sprint22Lifetime(ExpandedSummoningCorrectionFixture fixture,UnitEntityData owner,bool turnBased)
        {
            UnitEntityData target=fixture.Hostile;
            UnitEntityData second=CastExpandedSummoningOwnTier(fixture,"ankylosaurus");
            SetExpandedSummoningBrainActive(second,false);
            int oldFort=target.Descriptor.Stats.GetStat(StatType.SaveFortitude).BaseValue;
            var original=target.Descriptor.Buffs.RawFacts.OfType<Buff>().ToArray();
            Buff unrelated=null;
            try
            {
                target.Descriptor.Stats.GetStat(StatType.SaveFortitude).BaseValue=-100;
                var native=fixture.Blueprints.OfType<BlueprintBuff>().Single(value=>value.AssetGuid=="d2e35b870e4ac574d9873b36402487e5");
                unrelated=target.Descriptor.AddBuff(native,fixture.Caster,TimeSpan.FromMinutes(1));
                var legal=new JArray();
                int attack1=owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue;
                int attack2=second.Descriptor.Stats.AdditionalAttackBonus.BaseValue;
                bool allLegal=false;
                try
                {
                    owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue=100;
                    second.Descriptor.Stats.AdditionalAttackBonus.BaseValue=100;
                    // The command cases above prove animated attack delivery.
                    // These three distinct native strike rules run in one
                    // frame to test overlapping one-round victim lifetimes
                    // independently of initiative order. No synthetic roll,
                    // damage or rider event is inserted into production.
                    for (int trial=1;trial<=12;trial++)
                    {
                        foreach (Buff buff in target.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(value=>
                            !original.Contains(value) && value!=unrelated).ToArray()) buff.Remove();
                        target.Descriptor.Damage=fixture.HostileDamage;
                        allLegal=true;
                        foreach (UnitEntityData source in new[] {owner,owner,second})
                        {
                            var saves=new Sprint19SaveObserver { Target=target };
                            EventBus.Subscribe(saves);
                            try
                            {
                                var strike=Rulebook.Trigger(new RuleAttackWithWeapon(source,target,Sprint19Limbs(source).Single(),0));
                                var hit=strike.AttackRoll;
                                bool setup=hit!=null && hit.IsHit && !hit.IsCriticalConfirmed &&
                                    (saves.Saves.Count!=1 || !saves.Saves[0].IsPassed);
                                legal.Add(new JObject { ["source"]=source.UniqueId,["trial"]=trial,["preconditionsSatisfied"]=setup,
                                    ["event"]="distinct-native-strike-rule",["d20"]=hit==null ? -1 : hit.Roll.Value,
                                    ["hit"]=hit!=null && hit.IsHit,["critical"]=hit!=null && hit.IsCriticalConfirmed,
                                    ["saves"]=saves.Saves.Count,["passed"]=saves.Saves.Count==1 && saves.Saves[0].IsPassed });
                                if (setup && saves.Saves.Count!=1) throw new InvalidOperationException("unit="+source.UniqueId+";predicate=one-lifetime-save;event=native-strike-rule;owner="+source.Blueprint.AssetGuid);
                                allLegal &= setup;
                            }
                            finally { EventBus.Unsubscribe(saves); }
                        }
                        if (allLegal) break;
                    }
                }
                finally
                {
                    owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue=attack1;
                    second.Descriptor.Stats.AdditionalAttackBonus.BaseValue=attack2;
                }
                if (!allLegal) throw new InvalidOperationException("unit="+owner.UniqueId+";predicate=overlapping-lifetime-setup;event=native-strike-rule;owner="+owner.Blueprint.AssetGuid+";trials="+legal);
                var effects=target.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(value=>value.Blueprint.name=="KMG_Summoning_Special_Ankylosaurus_Dazed").ToArray();
                var sources=effects.Select(value=>value.Context?.MaybeCaster?.UniqueId).Distinct().ToArray();
                var ends=effects.ToDictionary(value=>value,value=>value.EndTime);
                bool finite=effects.All(value=>value.TimeLeft.TotalSeconds>0 && value.TimeLeft.TotalSeconds<=6.01);
                // The exact source dies after its strike. Already applied
                // victim buffs retain their absolute native expiry times.
                Rulebook.Trigger(new RuleDealDamage(target,owner,new DamageBundle(Sprint19Limbs(owner).Single(),Size.Huge,
                    new DirectDamage(new DiceFormula(0,DiceType.D6),owner.Descriptor.Stats.HitPoints.ModifiedValue+owner.Descriptor.Stats.Constitution.ModifiedValue+1))));
                Game.Instance.EntityDestroyer.Tick();
                bool preserved=effects.All(value=>target.Descriptor.Buffs.RawFacts.Contains(value) && value.EndTime==ends[value]) &&
                    target.Descriptor.Buffs.RawFacts.Contains(unrelated);
                Sprint22Check("multi-source-lifetime-"+(turnBased ? "turn-based":"rtwp"),owner.Descriptor.State.IsDead && effects.Length==3 && sources.Length==2 && finite && preserved,
                    new JObject { ["unit"]=owner.UniqueId,["secondSource"]=second.UniqueId,["target"]=target.UniqueId,["event"]="distinct-hits-source-death",
                        ["owner"]=owner.Blueprint.AssetGuid,["legalHits"]=legal,["payloadCount"]=effects.Length,["sources"]=new JArray(sources),
                        ["preserved"]=preserved,["finite"]=finite,["sourceDead"]=owner.Descriptor.State.IsDead,["unrelatedPreserved"]=target.Descriptor.Buffs.RawFacts.Contains(unrelated),
                        ["endTimes"]=new JArray(ends.Select(value=>new JObject { ["buff"]=value.Key.Blueprint.AssetGuid,
                            ["referenceId"]=System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value.Key),["endSeconds"]=value.Value.TotalSeconds })) },
                    "Distinct legal hits have independent finite victim buffs; source death leaves both owners and unrelated conditions intact.");
                unrelated.Remove();unrelated=null;
                double started=Game.Instance.TimeController.GameTime.TotalSeconds;
                var clock=System.Diagnostics.Stopwatch.StartNew();
                while (target.Descriptor.Buffs.RawFacts.OfType<Buff>().Any(value=>effects.Contains(value)) && clock.Elapsed.TotalSeconds<45)
                {
                    Game.Instance.IsPaused=false;
                    if (CombatController.IsInTurnBasedCombat())
                    {
                        var turn=Game.Instance.TurnBasedCombatController.CurrentTurn;
                        if (turn!=null) Sprint18EndTurn(turn);
                    }
                    yield return 0;
                }
                bool expired=effects.All(value=>!target.Descriptor.Buffs.RawFacts.Contains(value)) &&
                    !target.Descriptor.State.HasCondition(UnitCondition.Dazed) && target.Descriptor.State.CanAct;
                UnitAttack action=null;
                foreach (int frame in Sprint18RunFullAttack(fixture,target,value=>action=value,fixture.Caster)) yield return frame;
                Sprint22Check("natural-expiry-actions-"+(turnBased ? "turn-based":"rtwp"),expired && action!=null && action.IsFinished,
                    new JObject { ["unit"]=target.UniqueId,["event"]="native-time-expiration-and-command",["owner"]="victim-side-native-buffs",
                        ["expired"]=expired,["canAct"]=target.Descriptor.State.CanAct,["commandFinished"]=action?.IsFinished,
                        ["nativeElapsedSeconds"]=Game.Instance.TimeController.GameTime.TotalSeconds-started,
                        ["wallSeconds"]=clock.Elapsed.TotalSeconds },"Native finite effects expire and victim commands return in this combat mode.");
            }
            finally
            {
                target.Descriptor.Stats.GetStat(StatType.SaveFortitude).BaseValue=oldFort;
                foreach (Buff buff in target.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(value=>!original.Contains(value)).ToArray()) buff.Remove();
                DisposeExpandedSummoningUnits(fixture.Created,new[] {second});
            }
        }
    }
}
