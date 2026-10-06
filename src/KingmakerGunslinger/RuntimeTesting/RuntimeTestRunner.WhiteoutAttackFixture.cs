using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.Utility;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using KingmakerGunslinger.Enchantments;
using KingmakerGunslinger.Firearms;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Entire lifetime is synchronous in one loaded scene. Nothing here is
        // retained across its unload, including diagnostic exact-attack keys.
        private sealed class WhiteoutSiteFixture : IDisposable
        {
            private readonly RuntimeTestRunner _runner;
            private readonly Game _game=Game.Instance;
            private readonly Action<string,string,string,bool> _check;
            private readonly List<WhiteoutAttackObservation> _output;
            internal readonly List<UnitEntityData> Actors=new List<UnitEntityData>();
            private readonly List<BlueprintUnit> _prototypes=new List<BlueprintUnit>();
            private readonly List<UnityEngine.Object> _transient=new List<UnityEngine.Object>();
            private readonly List<Buff> _facts=new List<Buff>();
            private readonly List<KeyValuePair<UnitEntityData,Kingmaker.Blueprints.Facts.Fact>> _featureFacts=new List<KeyValuePair<UnitEntityData,Kingmaker.Blueprints.Facts.Fact>>();
            private readonly List<ItemEntityWeapon> _items=new List<ItemEntityWeapon>();
            private readonly List<WhiteoutWeatherProvider> _providers=new List<WhiteoutWeatherProvider>();
            private readonly UnitEntityData[] _beforeUnits;
            private readonly object[] _beforeAreas;
            private readonly Dictionary<UnitEntityData,Buff[]> _beforeBuffs;
            private readonly Dictionary<UnitEntityData,Vector3> _beforePositions;
            private readonly Dictionary<UnitEntityData,bool> _beforeCombat;
            private readonly object[] _beforeWeatherListeners, _beforeSceneListeners;
            private readonly BlueprintScriptableObject[] _beforeBlueprints;
            private readonly KeyValuePair<string,BlueprintScriptableObject>[] _beforeLookup;
            private readonly UnitEntityData[] _party;
            private readonly TimeSpan _time;
            private readonly Kingmaker.Blueprints.Area.BlueprintArea _area;
            private UnitEntityData _attacker,_otherAttacker,_marked,_secondMarked,_unmarked,_foreign;
            private ItemEntityWeapon _melee,_natural,_bow,_firearm,_seeking,_lookalike,_ray;
            private BlueprintBuff _blur;
            private WhiteoutGuardedDiagnostics _diagnostics;
            private bool _disposed;
            private string Prefix { get { return _area.AssetGuid=="2849fdde28fe50f4d935bf2cf3405051" ? "indoor" : "outdoor"; } }
            internal WhiteoutSiteFixture(RuntimeTestRunner runner,UnitEntityData anchor,Action<string,string,string,bool> check,List<WhiteoutAttackObservation> output)
            {
                _runner=runner; _check=check; _output=output; _area=_game.CurrentlyLoadedArea;
                _time=_game.Player.GameTime; _party=_game.Player.Party.ToArray();
                _beforeUnits=_game.State.Units.All.ToArray();
                _beforeAreas=_game.State.AreaEffects.All.Cast<object>().ToArray();
                _beforeBuffs=_beforeUnits.ToDictionary(u=>u,u=>u.Buffs.Enumerable.ToArray());
                _beforePositions=_beforeUnits.ToDictionary(u=>u,u=>u.Position);
                _beforeCombat=_beforeUnits.ToDictionary(u=>u,u=>u.IsInCombat);
                _beforeWeatherListeners=WhiteoutListeners(typeof(IWeatherChangeHandler));
                _beforeSceneListeners=WhiteoutListeners(typeof(ISceneHandler));
                _beforeBlueprints=BlueprintBootstrap.Library.GetAllBlueprints().ToArray();
                _beforeLookup=BlueprintBootstrap.Library.BlueprintsByAssetId.ToArray();
                try
                {
                    if(!WhiteoutAttackStagePatch.ContractValid || !WhiteoutNativeContract.ModuleEnabled())
                        throw new InvalidOperationException("Exact dormant Whiteout patch/module contract not valid.");
                    _diagnostics=new WhiteoutGuardedDiagnostics(runner._request,runner._workingSaveSmoke.Complete,runner._workingSaveSmoke.WriteObserved);
                    var nativeFaction=BlueprintRoot.Instance.SystemMechanics.FactionNeutrals;
                    if(nativeFaction==null) throw new InvalidOperationException("Exact native neutral disposable faction missing.");
                    // This root identity is friendly for native touch AutoHit.
                    // A request-local clone with the actual Neutral flag tests
                    // the attack seam without making any actor hostile to the
                    // preexisting party or mutating a registered faction.
                    var faction=UnityEngine.Object.Instantiate(nativeFaction);
                    faction.name="KMG_Runtime_Whiteout_Neutral"; faction.Neutral=true;
                    faction.AlwaysEnemy=false; faction.Peaceful=true; faction.Dummy=null;
                    faction.IsDirectlyControllable=false; faction.AttackFactions=Array.Empty<BlueprintFaction>();
                    typeof(BlueprintScriptableObject).GetField("m_AssetGuid",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(faction,Guid.NewGuid().ToString("N"));
                    _transient.Add(faction);
                    _attacker=CircleSpawn("WhiteoutAttacker",anchor.Position,anchor,Actors,_prototypes,faction);
                    _otherAttacker=CircleSpawn("WhiteoutOtherAttacker",anchor.Position,anchor,Actors,_prototypes,faction);
                    _marked=CircleSpawn("WhiteoutMarked",anchor.Position,anchor,Actors,_prototypes,faction);
                    _secondMarked=CircleSpawn("WhiteoutSecondMarked",anchor.Position,anchor,Actors,_prototypes,faction);
                    _unmarked=CircleSpawn("WhiteoutUnmarked",anchor.Position,anchor,Actors,_prototypes,faction);
                    _foreign=CircleSpawn("WhiteoutForeignLookalike",anchor.Position,anchor,Actors,_prototypes,faction);
                    Verify("native-touch-stage-eligible",_marked.Faction.Neutral && !ReferenceEquals(_marked.Faction,nativeFaction),
                        "request-owned neutral faction bypasses friendly touch AutoHit; registered faction unchanged");
                    var provider=(BlueprintBuff)ElementalCharacterTraitPublicationCoordinator.Graph.Resolve(ElementalCharacterTraitCatalog.Get(ElementalCharacterTraitId.Whiteout).GrantedNode.Symbol);
                    foreach(var owner in new[]{_marked,_secondMarked})
                    {
                        var feature=TraitFeatureGrant(owner,ElementalCharacterTraitId.Whiteout);
                        _featureFacts.Add(new KeyValuePair<UnitEntityData,Kingmaker.Blueprints.Facts.Fact>(owner,feature));
                        _facts.Add(TraitOwnedBuff(feature));
                    }
                    var foreign=UnityEngine.Object.Instantiate(provider); foreign.name=provider.name;
                    _transient.Add(foreign); _facts.Add(AerialBuff(_foreign,foreign));
                    foreach(var fact in _facts) _providers.AddRange(fact.SelectComponents<WhiteoutWeatherProvider>());
                    Verify("provider-independent-owners",_providers.Count==3 && !ReferenceEquals(_providers[0],_providers[1]) &&
                        !ReferenceEquals(_providers[0].State,_providers[1].State) && !_providers[2].HasExactIdentity(),
                        "two native per-fact instances; equal name/copied random GUID on foreign blueprint does not authorize it");
                    // Exclude every project-owned identity from generic native donor
                    // lookup. No other development line is used as a source fixture.
                    var entries=(JArray)JObject.Parse(File.ReadAllText(Path.Combine(runner._context.ModEntry.Path,BlueprintManifest.RelativeManifestPath)))["entries"];
                    var owned=new HashSet<string>(entries.Select(e=>(string)e["id"]),StringComparer.Ordinal);
                    Func<BlueprintItemWeapon,bool> native=b=>b!=null && !owned.Contains(b.AssetGuid) && ReferenceEquals(b,ResourcesLibrary.TryGetBlueprint<BlueprintItemWeapon>(b.AssetGuid));
                    var catalog=BlueprintBootstrap.Library.GetAllBlueprints().OfType<BlueprintItemWeapon>().Where(native).OrderBy(b=>b.AssetGuid,StringComparer.Ordinal).ToArray();
                    _melee=Item(catalog.First(b=>b.Category==WeaponCategory.Longsword && b.IsMelee && !b.IsNatural));
                    _natural=Item(catalog.First(b=>b.Category==WeaponCategory.Bite && b.IsNatural && b.IsMelee));
                    _bow=Item(catalog.First(b=>b.Category==WeaponCategory.Longbow && b.IsRanged));
                    var ray=BlueprintRoot.Instance.SystemMechanics.RayWeapon;
                    if(!native(ray) || ray.AttackType!=AttackType.RangedTouch) throw new InvalidOperationException("Canonical native ray/RangedTouch carrier mismatch.");
                    _ray=Item(ray);
                    _firearm=Item(BlueprintBootstrap.MagicFirearms.Entries[0].Item);
                    _seeking=Item(BlueprintBootstrap.MagicFirearms.Entries[6].Item);
                    _lookalike=Item(BlueprintBootstrap.MagicFirearms.Entries[0].Item);
                    var canonical=_seeking.Enchantments.Select(e=>e.Blueprint).OfType<BlueprintWeaponEnchantment>()
                        .Single(e=>e.ComponentsArray.OfType<SeekingWeaponEnchantmentComponent>().Any());
                    var fake=ScriptableObject.CreateInstance<BlueprintWeaponEnchantment>(); fake.name=canonical.name;
                    typeof(BlueprintScriptableObject).GetField("m_AssetGuid",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(fake,canonical.AssetGuid);
                    var fakeMarker=ScriptableObject.CreateInstance<SeekingWeaponEnchantmentComponent>(); fakeMarker.name="$WhiteoutForeignSeekingLookalike";
                    fake.ComponentsArray=new BlueprintComponent[]{fakeMarker}; _transient.Add(fake); _transient.Add(fakeMarker);
                    _lookalike.AddEnchantment(fake,null,null);
                    _blur=BlueprintBootstrap.Library.GetAllBlueprints().OfType<BlueprintBuff>().First(b=>b.AssetGuid=="dd3ad347240624d46a11a092b4dd4674");
                    Verify("exact-seeking-identity",SeekingExactItemResolver.IsAuthorized(_seeking) && !SeekingExactItemResolver.IsAuthorized(_lookalike),
                        "canonical Last Word bypass; foreign equal name/copied canonical GUID/marker never bypasses");
                }
                catch(Exception error)
                {
                    try { Dispose(); } catch(Exception cleanup) { throw new AggregateException(error,cleanup); }
                    throw;
                }
            }
            private ItemEntityWeapon Item(BlueprintItemWeapon blueprint)
            { var item=new ItemEntityWeapon(blueprint); _items.Add(item); return item; }
            internal bool OwnsListener(object value)
            { var provider=value as WhiteoutWeatherProvider; return provider!=null && _providers.Contains(provider) && Actors.Contains(provider.Owner.Unit); }
            private void CleanupAttempt(Action action,List<Exception> errors)
            { try { action(); } catch(Exception error) { errors.Add(error); } }
            private void Verify(string id,bool pass,string observed)
            { _check("whiteout-"+Prefix+"-"+id,"exact frozen unpublished Whiteout contract",observed,pass); }
            private WhiteoutAttackObservation Attack(string id,UnitEntityData attacker,UnitEntityData target,ItemEntityWeapon weapon,
                int? forcedWhiteout,bool expectedActive,bool? expectedResult,int expectedRolls,int? nativeConcealment=null,bool ignore=false,int replay=0)
            {
                attacker.Body.PrimaryHand.InsertItem(weapon);
                var rule=new RuleAttackRoll(attacker,target,weapon,-100) { IgnoreConcealment=ignore };
                var record=_diagnostics.Register(Prefix+"-"+id,rule);
                try
                {
                    if(ReferenceEquals(weapon,_firearm) || ReferenceEquals(weapon,_seeking) || ReferenceEquals(weapon,_lookalike))
                        FirearmRuntimeState.Service.Set(weapon,new FirearmState(FirearmState.CurrentSchemaVersion,1,FirearmStateTokenCatalog.DiagnosticLeadBall,FirearmCondition.Normal));
                    if(forcedWhiteout.HasValue) _diagnostics.Queue(rule,forcedWhiteout.Value);
                    if(nativeConcealment.HasValue) SeekingConcealmentRuntime.QueueForcedRoll(weapon,nativeConcealment.Value);
                    Rulebook.Trigger(rule);
                    for(int i=0;i<replay;i++) WhiteoutNativeContract.AttackMethod(typeof(RuleAttackRoll)).Invoke(rule,null);
                    bool pass=record.StageCalls==1+replay && record.Active==expectedActive && (!expectedResult.HasValue || record.Result==expectedResult.Value) &&
                        record.WhiteoutRollCalls==expectedRolls && record.ForcedConsumed==(forcedWhiteout.HasValue ? expectedRolls : 0) &&
                        record.Error==null && !record.DecisionFailedOpen;
                    if(nativeConcealment.HasValue && !ignore) pass &= record.NativeConcealment=="Partial" && record.NativeConcealmentRoll==nativeConcealment.Value;
                    Verify(id,pass,"stageCalls="+record.StageCalls+";native="+record.NativeSucceeded+";stageResult="+record.Result+";active="+record.Active+
                        ";WhiteoutRolls="+record.WhiteoutRollCalls+";forcedConsumed="+record.ForcedConsumed+";d100="+record.WhiteoutRoll+
                        ";nativeConcealment="+record.NativeConcealment+":"+record.NativeConcealmentRoll+";attackResult="+rule.Result+";attackType="+rule.AttackType);
                    return record;
                }
                finally { _diagnostics.CancelPending(); SeekingConcealmentRuntime.CancelForcedRoll(); attacker.Body.PrimaryHand.RemoveItem(false); }
            }
            internal void ClearControls()
            {
                Attack("unmarked-control",_attacker,_unmarked,_melee,10,false,true,0);
                Attack("marked-clear-control",_attacker,_marked,_melee,10,false,true,0);
            }
            internal void RainControls(bool indoor)
            {
                Verify("weather-state",_providers.Take(2).All(p=>p.State.Active==!indoor) && !_providers[2].State.Active,"exact native provider event state; indoors="+indoor);
                if(indoor)
                {
                    Attack("rain-marked-indoor-control",_attacker,_marked,_melee,10,false,true,0);
                    Attack("rain-unmarked-indoor-control",_attacker,_unmarked,_melee,10,false,true,0);
                    return;
                }
                Attack("melee-10-miss",_attacker,_marked,_melee,10,true,false,1);
                Attack("melee-11-continue",_attacker,_marked,_melee,11,true,true,1);
                var nativeRandom=Attack("native-whiteout-d100",_attacker,_marked,_melee,null,true,null,1);
                Verify("native-rng-boundary",nativeRandom.WhiteoutRoll>=1 && nativeRandom.WhiteoutRoll<=100 && nativeRandom.ForcedConsumed==0 &&
                    nativeRandom.Result==(nativeRandom.WhiteoutRoll>10),"real native Dice.D100 path; no override; observed="+nativeRandom.WhiteoutRoll);
                ExactAttackDiagnosticIsolation();
                Attack("natural-10-miss",_attacker,_marked,_natural,10,true,false,1);
                Attack("bow-10-miss",_attacker,_marked,_bow,10,true,false,1);
                Attack("firearm-10-miss",_attacker,_marked,_firearm,10,true,false,1);
                Attack("ray-rangedtouch-10-miss",_attacker,_marked,_ray,10,true,false,1);
                Attack("unmarked-rain-control",_attacker,_unmarked,_melee,10,false,true,0);
                Attack("foreign-whiteout-lookalike",_attacker,_foreign,_melee,10,false,true,0);
                Attack("other-attacker-unmarked-control",_otherAttacker,_unmarked,_melee,10,false,true,0);
                Attack("second-marked-target-independent",_otherAttacker,_secondMarked,_melee,11,true,true,1);
                Attack("replay-one-whiteout-roll",_attacker,_marked,_melee,10,true,false,1,null,false,2);
                var blur=AerialBuff(_marked,_blur);
                try
                {
                    var failure=Attack("native-20-failure-short-circuit",_attacker,_marked,_melee,10,true,false,0,1);
                    Verify("native-failure-preserved",failure.NativeFailures==1 && !failure.NativeSucceeded,"real native Partial 20% fails before Whiteout; zero roll");
                    Attack("native-20-success-whiteout-10",_attacker,_marked,_melee,10,true,false,1,100);
                    Attack("native-20-success-whiteout-11",_attacker,_marked,_melee,11,true,true,1,100);
                    Attack("seeking-bypass",_attacker,_marked,_seeking,10,true,true,0,1);
                    Attack("foreign-seeking-not-bypass",_attacker,_marked,_lookalike,10,true,false,1,100);
                    Attack("ignore-concealment-bypass",_attacker,_marked,_melee,10,true,true,0,null,true);
                    Verify("native-concealment-unchanged",blur.Active && _marked.Buffs.Enumerable.Contains(blur),"no concealment cleanup/suppression/classification mutation by Whiteout");
                }
                finally { blur.Remove(); }
            }
            private void ExactAttackDiagnosticIsolation()
            {
                // Same attacker/target/weapon/thread, different exact rules.
                // A queued roll for A must not be consumed by B.
                try
                {
                    _attacker.Body.PrimaryHand.InsertItem(_melee);
                    var a=new RuleAttackRoll(_attacker,_marked,_melee,-100);
                    var b=new RuleAttackRoll(_attacker,_marked,_melee,-100);
                    var ra=_diagnostics.Register(Prefix+"-diagnostic-exact-attack-A",a);
                    var rb=_diagnostics.Register(Prefix+"-diagnostic-exact-attack-B",b);
                    _diagnostics.Queue(a,10);
                    Rulebook.Trigger(b); Rulebook.Trigger(a);
                    Verify("diagnostic-exact-attack-isolation",ra.StageCalls==1 && rb.StageCalls==1 &&
                        ra.WhiteoutRollCalls==1 && ra.ForcedConsumed==1 && ra.WhiteoutRoll==10 && !ra.Result &&
                        rb.WhiteoutRollCalls==1 && rb.ForcedConsumed==0 && rb.WhiteoutRoll>=1 && rb.WhiteoutRoll<=100 && rb.Result==(rb.WhiteoutRoll>10),
                        "A forced=10; B native="+rb.WhiteoutRoll+"; same tuple/thread cannot borrow A's queued decision");
                }
                finally { _diagnostics.CancelPending(); SeekingConcealmentRuntime.CancelForcedRoll(); _attacker.Body.PrimaryHand.RemoveItem(false); }
            }
            internal void RepeatedRain(bool indoor)
            {
                Verify("event-idempotence",_providers.Take(2).All(p=>p.State.TransitionCount==(indoor ? 0 : 1) && p.WeatherEventCount==2),
                    "two native deliveries; exactly one outdoor activation, zero indoor activation");
            }
            internal void SnowControls(bool indoor)
            {
                Verify("snow-provider-state",_providers.Take(2).All(p=>p.State.Active==!indoor),"real native Snow/Light event reconciles the exact providers");
                Attack("snow-attack",_attacker,_marked,_melee,10,!indoor,indoor,indoor ? 0 : 1);
            }
            internal void ClearedControls()
            {
                Verify("clear-provider-state",_providers.All(p=>!p.State.Active),"native Clear removes every provider's active state");
                Attack("clear-removes-protection",_attacker,_marked,_melee,10,false,true,0);
            }
            internal void RestoredWeatherEvents()
            {
                Verify("exact-weather-handler-delivery",_providers.All(p=>p.WeatherEventCount==5),"all owned native fact components receive exactly five notifications, including per-area rollback");
            }
            public void Dispose()
            {
                if(_disposed) return; _disposed=true;
                var errors=new List<Exception>();
                // Every owned removal is attempted even if an earlier native
                // cleanup throws. Per-area weather has already rolled back in
                // the enclosing finally; failure still auto-exits the process.
                CleanupAttempt(SeekingConcealmentRuntime.CancelForcedRoll,errors);
                if(_diagnostics!=null)
                {
                    CleanupAttempt(_diagnostics.Dispose,errors);
                    _output.AddRange(_diagnostics.Records);
                }
                foreach(var feature in _featureFacts) CleanupAttempt(()=>feature.Key.Descriptor.RemoveFact(feature.Value),errors);
                foreach(var fact in _facts.ToArray()) if(fact!=null && !fact.IsDisposed) CleanupAttempt(fact.Remove,errors);
                foreach(var weapon in _items.Where(w=>ReferenceEquals(w,_firearm) || ReferenceEquals(w,_seeking) || ReferenceEquals(w,_lookalike)))
                    CleanupAttempt(()=>FirearmRuntimeState.Repository.Remove(weapon),errors);
                foreach(var unit in Actors.Where(u=>u!=null && !u.Destroyed)) CleanupAttempt(unit.Destroy,errors);
                CleanupAttempt(_game.EntityDestroyer.Tick,errors); CleanupAttempt(_game.EntityDestroyer.Tick,errors);
                foreach(var prototype in _prototypes) if(prototype!=null) CleanupAttempt(()=>UnityEngine.Object.DestroyImmediate(prototype),errors);
                foreach(var value in _transient.Distinct()) if(value!=null) CleanupAttempt(()=>UnityEngine.Object.DestroyImmediate(value),errors);
                var current=_game.State.Units.All.ToArray();
                bool restored=errors.Count==0 && current.Length==_beforeUnits.Length && _beforeUnits.All(current.Contains) &&
                    _beforeBuffs.All(p=>p.Key.Buffs.Enumerable.SequenceEqual(p.Value)) && _beforePositions.All(p=>p.Key.Position==p.Value) &&
                    _beforeCombat.All(p=>p.Key.IsInCombat==p.Value) && _game.CurrentlyLoadedArea==_area && _game.Player.Party.SequenceEqual(_party) &&
                    _game.State.AreaEffects.All.Cast<object>().SequenceEqual(_beforeAreas) &&
                    _providers.All(p=>!p.State.Active && !p.HasExactIdentity() && !EventBus.IsGloballySubscribed(p)) &&
                    _game.Player.GameTime==_time && _beforeWeatherListeners.SequenceEqual(WhiteoutListeners(typeof(IWeatherChangeHandler))) &&
                    _beforeSceneListeners.SequenceEqual(WhiteoutListeners(typeof(ISceneHandler))) &&
                    _beforeBlueprints.SequenceEqual(BlueprintBootstrap.Library.GetAllBlueprints()) && _beforeLookup.SequenceEqual(BlueprintBootstrap.Library.BlueprintsByAssetId) &&
                    !_runner._workingSaveSmoke.WriteObserved && !WhiteoutGuardedDiagnostics.HasCurrent && (_diagnostics==null || _diagnostics.CleanupVerified);
                Verify("exact-owned-fixture-cleanup",restored,"same-area native units/facts/positions/combat/party/areas/registry/time/listeners unchanged; owned facts/actors/diagnostics/attack cache removed; zero save writes; cleanupErrors="+errors.Count);
                Actors.Clear(); _featureFacts.Clear(); _facts.Clear(); _providers.Clear(); _items.Clear();
                if(errors.Count!=0) throw new AggregateException("Whiteout owned cleanup failed after every removal was attempted.",errors);
                if(!restored) throw new InvalidOperationException("Whiteout same-area fixture cleanup failed.");
            }
        }
    }
}
