using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Visual.LocalMap;
using Kingmaker.Visual.WeatherSystem;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using KingmakerGunslinger.Spells.Teleportation;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private WhiteoutDisposableFixture _whiteoutDisposableFixture;
        private void PollWhiteoutDisposableFixture()
        {
            if (_request.Scenario != RuntimeTestScenarioCatalog.ObserveWhiteoutDisposableWeatherFixture ||
                !_request.ExitAfterCompletion || _workingSaveSmoke == null || !_workingSaveSmoke.Complete || _workingSaveSmoke.WriteObserved)
                throw new InvalidOperationException("Exact no-write working-save readiness and automatic exit required.");
            if (LoadingProcess.Instance.IsLoadingInProcess || LoadingProcess.Instance.IsLoadingScreenActive) return;
            if (_whiteoutDisposableFixture == null) _whiteoutDisposableFixture = new WhiteoutDisposableFixture(this);
            _whiteoutDisposableFixture.Tick();
            if (!_whiteoutDisposableFixture.ReadyForCompletion) return;
            var fixture = _whiteoutDisposableFixture;
            var path = Path.Combine(_request.EvidenceDirectory, "whiteout-disposable-weather-fixture.json");
            WriteTeleportationForensicJson(path, new {
                schemaVersion=1, runId=_request.RunId, gate=fixture.Failure==null ? "WEATHER-FIXTURE-QUALIFIED" : "WEATHER-FIXTURE-FAILED",
                NonPartySceneReferenceRestorationRequired=false, DisposableNoSaveProcessIsolation=true,
                samples=fixture.Samples, assertions=fixture.Assertions, events=fixture.Probe.Events,
                mutationCount=fixture.MutationCount, saveWriteObserved=_workingSaveSmoke.WriteObserved,
                error=fixture.Failure, WhiteoutPublished=false, attackAdapterInstalled=false });
            var result=CreateResult(fixture.Failure==null && fixture.Assertions.All(a=>a.Status=="PASS") ? "PASS" : "FAIL", fixture.Assertions, fixture.Failure);
            result.EvidenceFiles.Add(path);
            result.Diagnostics.Add("Weather gate only. One-way mansion to Oleg; non-party scene replacement accepted inside automatic-exiting no-save process. No reference restoration claim.");
            Complete(result);
        }

        private void StopWhiteoutDisposableFixture(RuntimeTestResult result)
        {
            if (_whiteoutDisposableFixture == null) return;
            try { _whiteoutDisposableFixture.Close(); }
            catch(Exception error) { result.Status="FAIL"; result.Diagnostics.Add("Whiteout fixture cleanup failed: "+error); }
        }

        // Each area's weather and owned state restore synchronously before leaving
        // it. No old scene unit reference is retained or consulted across unload.
        private sealed class WhiteoutDisposableFixture
        {
            private readonly RuntimeTestRunner _runner;
            private readonly Game _game;
            private readonly BlueprintAreaEnterPoint _outdoor;
            private readonly UnitEntityData[] _party;
            private readonly object _mainCharacter;
            private readonly Kingmaker.Items.ItemEntity[] _inventory;
            private readonly int[] _counts;
            private readonly long _money;
            private readonly bool _pause;
            private readonly TimeSpan _time, _next;
            private readonly Player.WeatherData _savedWeather;
            private readonly InclemencyType _current;
            private IEnumerator<int> _load;
            private int _phase;
            private readonly System.Diagnostics.Stopwatch _watch=System.Diagnostics.Stopwatch.StartNew();
            internal readonly List<RuntimeTestAssertion> Assertions=new List<RuntimeTestAssertion>();
            internal readonly List<object> Samples=new List<object>();
            internal readonly WhiteoutFixtureProbe Probe=new WhiteoutFixtureProbe();
            internal string Failure;
            internal bool ReadyForCompletion;
            internal int MutationCount;
            internal WhiteoutDisposableFixture(RuntimeTestRunner runner)
            {
                _runner=runner; _game=Game.Instance;
                _party=_game.Player.Party.ToArray(); _mainCharacter=_game.Player.MainCharacter;
                _inventory=_game.Player.Inventory.Items.ToArray(); _counts=_inventory.Select(i=>i.Count).ToArray();
                _money=_game.Player.Money; _pause=_game.IsPaused; _time=_game.Player.GameTime;
                _savedWeather=_game.Player.Weather; _current=_savedWeather.CurrentWeather; _next=_savedWeather.NextWeatherChange;
                _outdoor=BlueprintLibraryLookup.RequireExact<BlueprintLocation>(BlueprintBootstrap.Library, WordOfRecallDestinationPolicy.OlegId, "native Oleg outdoor location").AreaEntrance;
                if (_game.CurrentlyLoadedArea?.AssetGuid!="2849fdde28fe50f4d935bf2cf3405051" || _outdoor?.Area?.AssetGuid!="ead426a6c23d39548a670ee515d77df4" ||
                    _game.CurrentMode!=GameModeType.Default || _savedWeather.ActualWeather!=InclemencyType.Clear)
                    throw new InvalidOperationException("Only the qualified clear mansion/Oleg fixture is authorized.");
            }
            internal void Tick()
            {
                try
                {
                    if (_watch.Elapsed.TotalSeconds>_runner._request.CompletionTimeoutSeconds) throw new TimeoutException("Disposable weather fixture deadline.");
                    _game.IsPaused=true;
                    if (_load!=null)
                    {
                        if (_load.MoveNext()) return;
                        _load.Dispose(); _load=null;
                        if (_game.CurrentlyLoadedArea!=_outdoor.Area) throw new InvalidOperationException("Native one-way Oleg load identity mismatch.");
                    }
                    if (_phase==0)
                    {
                        RunSite("indoor-mansion",true);
                        PersistentControls("before-one-way-transition");
                        if (Failure!=null) { Finish(); return; }
                        _phase=1;
                        _load=_runner.WhiteoutLoadArea(_outdoor.Area,_outdoor).GetEnumerator();
                        return;
                    }
                    if (_phase==1) { RunSite("outdoor-oleg",false); _phase=2; }
                    Finish();
                }
                catch(Exception error)
                {
                    if(Failure==null) Failure=error.ToString();
                    if(_load!=null) { _load.Dispose(); _load=null; }
                    Finish();
                }
            }
            private void Check(string name,string expected,string observed,bool pass)
            {
                Assertions.Add(Assertion(name,expected,observed,pass,"Exact native state; synchronous per-area weather rollback; disposable no-save process."));
                if(!pass && Failure==null) Failure="Failed invariant: "+name;
            }
            private void PersistentControls(string phase)
            {
                bool pass=_game.Player.Party.SequenceEqual(_party) && Equals(_game.Player.MainCharacter,_mainCharacter) &&
                    _game.Player.Inventory.Items.SequenceEqual(_inventory) && _inventory.Select((v,i)=>v.Count==_counts[i]).All(v=>v) &&
                    _game.Player.Money==_money && _game.Player.GameTime==_time && ReferenceEquals(_game.Player.Weather,_savedWeather) &&
                    _savedWeather.CurrentWeather==_current && _savedWeather.NextWeatherChange==_next && !_runner._workingSaveSmoke.WriteObserved;
                Check("whiteout-"+phase+"-persistent-controls","party/character/inventory identities and counts, money, clock, saved schedule unchanged; zero save writes","pass="+pass,pass);
            }
            internal void Close()
            {
                if(_load!=null) { _load.Dispose(); _load=null; }
                EventBus.Unsubscribe(Probe); _game.IsPaused=_pause;
            }
            private void Finish()
            {
                PersistentControls("before-process-exit");
                EventBus.Unsubscribe(Probe);
                Check("whiteout-process-probe-cleanup","no owned weather listener","subscribed="+EventBus.IsGloballySubscribed(Probe),!EventBus.IsGloballySubscribed(Probe));
                _game.IsPaused=_pause;
                ReadyForCompletion=true;
            }
            private void RunSite(string phase, bool indoor)
            {
                var anchor = _party.First(u => u.IsInGame && u.View != null);
                var map = LocalMapArea.GetClosest(anchor.Position);
                if (map == null || map.AreaPart == null || map.AreaPart.IsIndoor != indoor || LocalMapArea.IsIndoor(anchor.Position) != indoor)
                    throw new InvalidOperationException("Exact loaded-point indoor identity is missing or contradictory.");
                var state = new WhiteoutWeatherState();
                int before = Probe.Changes;
                var listenersBefore = WhiteoutListeners(typeof(IWeatherChangeHandler));
                EventBus.Subscribe(Probe);
                try
                {
                using (var weather = new WhiteoutWeatherMutation(Samples))
                {
                    weather.Set(WeatherType.Rain);
                    MutationCount++;
                    var actual = _game.Player.Weather.ActualWeather;
                    bool active = WhiteoutPolicy.WeatherActive(true, WhiteoutPrecipitation.Rain, (int)actual, LocalMapArea.IsIndoor(anchor.Position));
                    state.Reconcile(true, WhiteoutPrecipitation.Rain, (int)actual, indoor, true, true);
                    Check(phase+"-rain-light", "native Rain/Light; outdoors-only active="+!indoor,
                        "type="+WeatherSystemBehaviour.Instance.WeatherType+";actual="+actual+";indoor="+LocalMapArea.IsIndoor(anchor.Position)+";active="+active,
                        actual == InclemencyType.Light && WeatherSystemBehaviour.Instance.WeatherType == WeatherType.Rain && active == !indoor);
                    weather.Set(WeatherType.Rain); MutationCount++;
                    state.Reconcile(true, WhiteoutPrecipitation.Rain, (int)_game.Player.Weather.ActualWeather, indoor, true, true);
                    Check(phase+"-repeat-idempotent", "no duplicated transition", "transitions="+state.TransitionCount,
                        state.TransitionCount == (indoor ? 0 : 1));
                    weather.Set(WeatherType.Snow); MutationCount++;
                    actual = _game.Player.Weather.ActualWeather;
                    active = WhiteoutPolicy.WeatherActive(true, WhiteoutPrecipitation.Snow, (int)actual, indoor);
                    state.Reconcile(true, WhiteoutPrecipitation.Snow, (int)actual, indoor, true, true);
                    Check(phase+"-snow-light", "native Snow/Light; outdoors-only active="+!indoor,
                        "type="+WeatherSystemBehaviour.Instance.WeatherType+";actual="+actual+";active="+active,
                        actual == InclemencyType.Light && WeatherSystemBehaviour.Instance.WeatherType == WeatherType.Snow && active == !indoor);
                    weather.Set(WeatherType.Normal); MutationCount++;
                    state.Reconcile(true, WhiteoutPrecipitation.Normal, (int)_game.Player.Weather.ActualWeather, indoor, true, true);
                    Check(phase+"-clear", "Clear and inactive", _game.Player.Weather.ActualWeather+";active="+state.Active,
                        _game.Player.Weather.ActualWeather == InclemencyType.Clear && !state.Active);
                }
                }
                finally { EventBus.Unsubscribe(Probe); }
                Check(phase+"-listener-cleanup", "exact pre-site listeners", "restored="+WhiteoutListeners(typeof(IWeatherChangeHandler)).SequenceEqual(listenersBefore), WhiteoutListeners(typeof(IWeatherChangeHandler)).SequenceEqual(listenersBefore));
                Check(phase+"-native-event-count", "exactly five native weather-change deliveries, including restoration", "deliveries="+(Probe.Changes-before), Probe.Changes-before == 5);
                Samples.Add(new { phase, area = _game.CurrentlyLoadedArea.AssetGuid, part = map.AreaPart.AssetGuid, indoor,
                    restoredVisual = WeatherSystemBehaviour.Instance.WeatherType.ToString(), restoredActual = _game.Player.Weather.ActualWeather.ToString() });
            }
        }
        private sealed class WhiteoutFixtureProbe : IWeatherChangeHandler
        {
            internal int Changes;
            internal readonly List<string> Events=new List<string>();
            public void OnWeatherChange() { Changes++; Events.Add("weather:"+WeatherSystemBehaviour.Instance.WeatherType+":"+Game.Instance.Player.Weather.ActualWeather); }
        }

        // Runtime-only weather writer. No yield, Update, Tick, Init, schedule
        // write or coroutine occurs between the first mutation and finally.
        private sealed class WhiteoutWeatherMutation : IDisposable
        {
            private readonly WeatherSystemBehaviour _visual;
            private readonly WeatherController _controller;
            private readonly FieldInfo _season, _overridden;
            private readonly object _seasonBefore, _overriddenBefore;
            private readonly WeatherType _type;
            private readonly float _rain, _snow;
            private readonly Player.WeatherData _weather;
            private readonly InclemencyType _current, _actual;
            private readonly TimeSpan _next;
            private readonly object[] _listeners;
            private readonly UnitEntityData[] _units;
            private readonly Dictionary<UnitEntityData,Buff[]> _buffs;
            private readonly List<object> _samples;
            private bool _disposed;
            internal WhiteoutWeatherMutation(List<object> samples)
            {
                _samples = samples; _visual = WeatherSystemBehaviour.Instance; _weather = Game.Instance.Player.Weather;
                if (_visual == null || typeof(WeatherController).Module.ModuleVersionId != new Guid("07fa1e4d-8618-41b3-9b8d-faa17d3b26f7")) throw new InvalidOperationException("Exact installed native weather contract mismatch.");
                _type = _visual.WeatherType; _rain = _visual.RainIntensity; _snow = _visual.SnowIntensity;
                _current = _weather.CurrentWeather; _next = _weather.NextWeatherChange; _actual = _weather.ActualWeather;
                if (_actual != InclemencyType.Clear) throw new InvalidOperationException("Restoration requires the qualified clear starting fixture.");
                _listeners = WhiteoutListeners(typeof(IWeatherChangeHandler));
                _samples.Add(new { phase="weather-subscriber-preflight", handlers=_listeners.Select(listener => {
                    var conditional = listener as AddBuffInBadWeather;
                    return new { type=listener.GetType().FullName,
                        sourceFact=conditional?.Fact?.Blueprint?.AssetGuid,
                        targetBuff=conditional?.Buff?.AssetGuid,
                        whenCalmer=conditional == null ? (bool?)null : conditional.WhenCalmer,
                        threshold=conditional == null ? null : conditional.Weather.ToString(),
                        wouldGrantClear=conditional == null ? (bool?)null : conditional.WhenCalmer == (InclemencyType.Clear < conditional.Weather),
                        wouldGrantLight=conditional == null ? (bool?)null : conditional.WhenCalmer == (InclemencyType.Light < conditional.Weather),
                        existingTargetBuff=conditional == null ? (bool?)null : conditional.Owner.Buffs.Enumerable.Any(b=>ReferenceEquals(b.Blueprint,conditional.Buff)) };
                }).ToArray() });
                foreach (object listener in _listeners)
                {
                    if (listener is WhiteoutFixtureProbe || listener.GetType().FullName == "Kingmaker.Visual.Sound.SoundState") continue;
                    var party = listener as UnitPartPartyWeatherBuff;
                    if (party != null && typeof(UnitPartPartyWeatherBuff).GetField("m_LastBuff",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(party)==null) continue;
                    var conditional = listener as AddBuffInBadWeather;
                    // Native OnWeatherChange removes its Buff on this exact false
                    // branch. With no such fact present, every fixture delivery
                    // (Clear or Light) is an identity-preserving no-op.
                    if (conditional != null && conditional.Buff != null &&
                        !(conditional.WhenCalmer == (InclemencyType.Clear < conditional.Weather)) &&
                        !(conditional.WhenCalmer == (InclemencyType.Light < conditional.Weather)) &&
                        !conditional.Owner.Buffs.Enumerable.Any(b=>ReferenceEquals(b.Blueprint,conditional.Buff))) continue;
                    throw new InvalidOperationException("Unsafe or unqualified native weather subscriber; no writes performed: "+listener.GetType().FullName);
                }
                // Pausing deactivates the Default-mode controller's subscription.
                // The native accessor includes that same existing inactive instance;
                // it neither constructs a controller nor activates/ticks its mode.
                _controller = Game.Instance.GetController<WeatherController>(true);
                if (_controller == null || _controller.GetType() != typeof(WeatherController))
                    throw new InvalidOperationException("Exact existing native mode-owned weather controller is absent.");
                _season = typeof(WeatherController).GetField("m_SeasonData",BindingFlags.Instance|BindingFlags.NonPublic);
                _overridden = typeof(WeatherController).GetField("m_Overridden",BindingFlags.Instance|BindingFlags.NonPublic);
                if (_season == null || _season.FieldType != typeof(WeatherRoot.SeasonalData) || _overridden == null || _overridden.FieldType != typeof(bool)) throw new InvalidOperationException("Exact controller restoration fields absent.");
                _seasonBefore = _season.GetValue(_controller); _overriddenBefore = _overridden.GetValue(_controller);
                _units = Game.Instance.State.Units.All.ToArray(); _buffs = _units.ToDictionary(u=>u,u=>u.Buffs.Enumerable.ToArray());
                LightIntensity(WeatherType.Rain); LightIntensity(WeatherType.Snow);
                _samples.Add(new { phase="before-mutation", type=_type.ToString(), rain=_rain, snow=_snow, actual=_actual.ToString(),
                    current=_current.ToString(), nextTicks=_next.Ticks, controllerOverridden=_overriddenBefore, handlers=_listeners.Select(l=>l.GetType().FullName).ToArray(),
                    rainThresholds=BlueprintRoot.Instance.WeatherSettings.RainIntensitites.Values, snowThresholds=BlueprintRoot.Instance.WeatherSettings.SnowIntensitites.Values });
            }
            private static float LightIntensity(WeatherType type)
            {
                var values = (type==WeatherType.Rain ? BlueprintRoot.Instance.WeatherSettings.RainIntensitites : BlueprintRoot.Instance.WeatherSettings.SnowIntensitites).Values;
                if (values == null || values.Length!=5 || values.Any(v=>float.IsNaN(v)||float.IsInfinity(v)) || Enumerable.Range(1,4).Any(i=>values[i]<=values[i-1])) throw new InvalidOperationException("Exact native intensity threshold contract mismatch.");
                return values[1]; // Native getter: first i with intensity < Values[i]-0.01 returns i-1.
            }
            internal void Set(WeatherType type)
            {
                if (_disposed || !ReferenceEquals(WeatherSystemBehaviour.Instance,_visual)) throw new InvalidOperationException("Exact weather instance changed.");
                _visual.WeatherType = type;
                _visual.RainIntensity = type==WeatherType.Rain ? LightIntensity(type) : 0;
                _visual.SnowIntensity = type==WeatherType.Snow ? LightIntensity(type) : 0;
                _controller.OnUpdateWeatherSystem(true);
                _samples.Add(new { phase="native-notification", type=type.ToString(), rain=_visual.RainIntensity, snow=_visual.SnowIntensity, actual=_weather.ActualWeather.ToString() });
            }
            public void Dispose()
            {
                if (_disposed) return;
                try
                {
                    _visual.WeatherType = _type; _visual.RainIntensity = _rain; _visual.SnowIntensity = _snow;
                    _controller.OnUpdateWeatherSystem(true);
                }
                finally
                {
                    _season.SetValue(_controller,_seasonBefore); _overridden.SetValue(_controller,_overriddenBefore); _disposed=true;
                }
                bool restored = ReferenceEquals(WeatherSystemBehaviour.Instance,_visual) && _visual.WeatherType==_type && _visual.RainIntensity==_rain && _visual.SnowIntensity==_snow &&
                    _weather.CurrentWeather==_current && _weather.NextWeatherChange==_next && _weather.ActualWeather==_actual &&
                    ReferenceEquals(_season.GetValue(_controller),_seasonBefore) && Equals(_overridden.GetValue(_controller),_overriddenBefore) &&
                    WhiteoutListeners(typeof(IWeatherChangeHandler)).SequenceEqual(_listeners) && _units.All(u=>u.Buffs.Enumerable.SequenceEqual(_buffs[u]));
                _samples.Add(new { phase="exact-weather-restoration", restored });
                if (!restored) throw new InvalidOperationException("Native weather/listener/condition restoration mismatch.");
            }
        }
        private static object[] WhiteoutListeners(Type handler)
        {
            var field = typeof(SubscriptionManager<IGlobalSubscriber>).GetField("m_Listeners",BindingFlags.Instance|BindingFlags.NonPublic);
            var dictionary = field.GetValue(EventBus.GlobalSubscribers) as PooledDictionary<Type,SubscribersList<IGlobalSubscriber>>;
            if (dictionary==null) throw new InvalidOperationException("Exact native listener dictionary unavailable.");
            var list = dictionary.Get(handler);
            if (list==null) return new object[0];
            var entries = typeof(SubscribersList<IGlobalSubscriber>).GetField("List",BindingFlags.Instance|BindingFlags.NonPublic);
            if (entries==null || list.Executing) throw new InvalidOperationException("Exact idle native listener list unavailable.");
            return ((IEnumerable<IGlobalSubscriber>)entries.GetValue(list)).Where(l=>l!=null).Cast<object>().ToArray();
        }
    }
}