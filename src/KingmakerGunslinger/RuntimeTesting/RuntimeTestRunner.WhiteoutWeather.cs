using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints.Area;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Visual.LocalMap;
using Kingmaker.Visual.WeatherSystem;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using KingmakerGunslinger.Spells.Teleportation;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerator<int> _whiteoutWeatherSteps;
        private System.Diagnostics.Stopwatch _whiteoutWeatherWatch;
        private readonly List<RuntimeTestAssertion> _whiteoutWeatherAssertions = new List<RuntimeTestAssertion>();
        private readonly List<object> _whiteoutWeatherSamples = new List<object>();
        private WhiteoutWeatherEvents _whiteoutWeatherEvents;
        private string WhiteoutWeatherPath { get { return Path.Combine(_request.EvidenceDirectory, "whiteout-weather-observation.json"); } }

        private void PollWhiteoutWeather()
        {
            if (_request.Scenario != RuntimeTestScenarioCatalog.ObserveWhiteoutWeather || !_request.ExitAfterCompletion ||
                _workingSaveSmoke == null || !_workingSaveSmoke.Complete || _workingSaveSmoke.WriteObserved)
                throw new InvalidOperationException("Whiteout observation requires the exact guarded working save, no writes and automatic exit.");
            if (_whiteoutWeatherWatch == null) _whiteoutWeatherWatch = System.Diagnostics.Stopwatch.StartNew();
            if (_whiteoutWeatherWatch.Elapsed.TotalSeconds > _request.CompletionTimeoutSeconds)
                throw new InvalidOperationException("Whiteout weather observation timed out.");
            if (LoadingProcess.Instance.IsLoadingInProcess || LoadingProcess.Instance.IsLoadingScreenActive) return;
            if (_whiteoutWeatherSteps == null) _whiteoutWeatherSteps = ObserveWhiteoutWeatherScenes().GetEnumerator();
            Exception failure = null;
            try { if (_whiteoutWeatherSteps.MoveNext()) return; }
            catch (Exception error) { failure = error; }
            try { _whiteoutWeatherSteps.Dispose(); }
            catch (Exception error) { failure = failure == null ? error : new AggregateException(failure, error); }
            _whiteoutWeatherSteps = null;
            WriteWhiteoutWeather(failure?.ToString());
            var result = CreateResult(failure != null ? "ERROR" : _whiteoutWeatherAssertions.All(a => a.Status == "PASS") ? "PASS" : "FAIL",
                _whiteoutWeatherAssertions, failure?.ToString());
            result.EvidenceFiles.Add(WhiteoutWeatherPath);
            result.Diagnostics.Add("Read-only weather observation and pure policy only; no Whiteout marker, buff, patch, icon or player acquisition is registered. Indoor/precipitation conclusions require reviewing the actual observations.");
            Complete(result);
        }

        private IEnumerable<int> ObserveWhiteoutWeatherScenes()
        {
            var game = Game.Instance; var origin = game.CurrentlyLoadedArea;
            var party = game.Player.Party.ToArray(); var positions = party.Select(u => u.Position).ToArray();
            var inventory = game.Player.Inventory.Items.ToArray(); var counts = inventory.Select(i => i.Count).ToArray();
            var money = game.Player.Money; var currentWeather = game.Player.Weather.CurrentWeather;
            var nextWeather = game.Player.Weather.NextWeatherChange;
            var camera = TeleportationCastingCamera(); var cameraPosition = camera.transform.position; var cameraTarget = camera.GetPosition();
            var entry = BlueprintLibraryLookup.RequireExact<BlueprintLocation>(BlueprintBootstrap.Library,
                WordOfRecallDestinationPolicy.OlegId, "native Oleg outdoor location").AreaEntrance;
            if (origin == null || entry?.Area == null || entry.Area == origin || game.CurrentMode != GameModeType.Default)
                throw new InvalidOperationException("Whiteout requires the loaded indoor working-save origin and distinct authored outdoor Oleg entry.");
            MethodInfo stage = typeof(RuleAttackRoll).GetMethod("TryOvercomeTargetConcealmentAndMissChance",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            _whiteoutWeatherAssertions.Add(Assertion("whiteout-native-stage-signature", "exact instance bool method, zero parameters",
                stage == null ? "absent" : stage.DeclaringType.FullName + "." + stage.Name + ";return=" + stage.ReturnType.FullName +
                    ";ILBytes=" + stage.GetMethodBody().GetILAsByteArray().Length + ";module=" + stage.Module.ModuleVersionId,
                stage != null && stage.DeclaringType == typeof(RuleAttackRoll) && stage.ReturnType == typeof(bool) && !stage.IsStatic,
                "read-only exact reflection; signature observation is not patch runtime qualification"));
            _whiteoutWeatherEvents = new WhiteoutWeatherEvents(); EventBus.Subscribe(_whiteoutWeatherEvents);
            try
            {
                CaptureWhiteoutWeather("origin-indoor", true);
                foreach (int frame in WhiteoutLoadArea(entry.Area, entry)) yield return frame;
                // Bounded fixture settling only; no production polling or unit scan.
                for (int i = 0; i < 3; i++) { game.IsPaused = true; yield return 0; }
                CaptureWhiteoutWeather("oleg-outdoor", false);
                foreach (int frame in WhiteoutLoadArea(origin, null)) yield return frame;
                for (int i = 0; i < 3; i++) { game.IsPaused = true; yield return 0; }
                for (int i = 0; i < party.Length; i++) party[i].Translocate(positions[i], null);
                camera.ScrollToImmediately(cameraTarget); camera.transform.position = cameraPosition;
                typeof(Kingmaker.View.CameraRig).GetField("m_TargetPosition", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(camera, cameraTarget);
                CaptureWhiteoutWeather("origin-indoor-return", true);
                _whiteoutWeatherAssertions.Add(Assertion("whiteout-native-area-events-observed", "two area loads/unloads; weather notification count is observed, not presumed",
                    "loads=" + _whiteoutWeatherEvents.Loads + ";unloads=" + _whiteoutWeatherEvents.Unloads + ";weather=" + _whiteoutWeatherEvents.Changes,
                    _whiteoutWeatherEvents.Loads == 2 && _whiteoutWeatherEvents.Unloads == 2,
                    "native ISceneHandler and IWeatherChangeHandler subscriptions; unchanged Clear intensity need not send a weather-change event; no event is fabricated"));
                bool restored = game.CurrentlyLoadedArea == origin && game.Player.Party.SequenceEqual(party) &&
                    party.Select((u, i) => u.Position == positions[i]).All(x => x) && game.Player.Inventory.Items.SequenceEqual(inventory) &&
                    inventory.Select((item, i) => item.Count == counts[i]).All(x => x) && game.Player.Money == money &&
                    game.Player.Weather.CurrentWeather == currentWeather && game.Player.Weather.NextWeatherChange == nextWeather &&
                    camera.transform.position == cameraPosition && camera.GetPosition() == cameraTarget && !_workingSaveSmoke.WriteObserved;
                _whiteoutWeatherAssertions.Add(Assertion("whiteout-observation-restored", "origin, party, positions, inventory, money, saved weather schedule and camera unchanged; zero writes",
                    "area=" + game.CurrentlyLoadedArea.AssetGuid + ";savedWeather=" + game.Player.Weather.CurrentWeather + ";writeObserved=" + _workingSaveSmoke.WriteObserved,
                    restored, "native area round trip with AutoSaveMode.None; no raw save access or weather mutation"));
            }
            finally { EventBus.Unsubscribe(_whiteoutWeatherEvents); }
        }

        private void CaptureWhiteoutWeather(string phase, bool expectedIndoor)
        {
            var game = Game.Instance; var anchor = game.Player.Party.First(u => u.IsInGame && u.View != null);
            var map = LocalMapArea.GetClosest(anchor.Position);
            var part = map == null ? null : map.AreaPart;
            bool indoor = LocalMapArea.IsIndoor(anchor.Position);
            var weather = WeatherSystemBehaviour.Instance;
            var visual = weather == null ? WeatherType.Normal : weather.WeatherType;
            var actual = game.Player.Weather.ActualWeather;
            bool proposed = WhiteoutPolicy.WeatherActive(true, (WhiteoutPrecipitation)(int)visual, (int)actual, false);
            _whiteoutWeatherSamples.Add(new { phase, areaGuid = game.CurrentlyLoadedArea.AssetGuid, areaName = game.CurrentlyLoadedArea.name,
                areaPartGuid = part == null ? null : part.AssetGuid, areaPartName = part == null ? null : part.name,
                areaPartIsIndoor = part == null ? (bool?)null : part.IsIndoor, exactPointIndoor = indoor,
                position = new { anchor.Position.x, anchor.Position.y, anchor.Position.z },
                weatherBehaviourPresent = weather != null, visualWeatherType = visual.ToString(),
                currentWeather = game.Player.Weather.CurrentWeather.ToString(), actualWeather = actual.ToString(),
                rainIntensity = weather == null ? (float?)null : weather.RainIntensity,
                snowIntensity = weather == null ? (float?)null : weather.SnowIntensity,
                proposedMarkerWeatherPredicate = proposed, actualWhiteoutApplied = false, indoorDecision = "NOT_CHOSEN_BY_OBSERVER" });
            _whiteoutWeatherAssertions.Add(Assertion("whiteout-observe-" + phase, "exact native indoor=" + expectedIndoor,
                "area=" + game.CurrentlyLoadedArea.name + ":" + game.CurrentlyLoadedArea.AssetGuid + ";indoor=" + indoor +
                    ";visual=" + visual + ";current=" + game.Player.Weather.CurrentWeather + ";actual=" + actual + ";proposed=" + proposed,
                part != null && indoor == expectedIndoor && part.IsIndoor == indoor,
                "LocalMapArea.IsIndoor(anchor.Position) and BlueprintAreaPart.IsIndoor; live weather getters, no mutation"));
        }

        private IEnumerable<int> WhiteoutLoadArea(BlueprintArea area, BlueprintAreaEnterPoint entry)
        {
            var scene = new CircleSceneObservation(); scene.Start();
            try
            {
                var load = typeof(Game).GetMethod("LoadArea", BindingFlags.Instance | BindingFlags.NonPublic, null,
                    new[] { typeof(BlueprintArea), typeof(BlueprintAreaEnterPoint), typeof(AutoSaveMode), typeof(bool), typeof(SaveInfo) }, null);
                if (load == null) throw new InvalidOperationException("Exact no-save native area-load contract absent.");
                load.Invoke(Game.Instance, new object[] { area, entry, AutoSaveMode.None, false, null });
                while (!scene.Ready) { Game.Instance.IsPaused = true; yield return 0; }
                if (Game.Instance.CurrentlyLoadedArea != area || _workingSaveSmoke.WriteObserved)
                    throw new InvalidOperationException("Whiteout area observation destination/write mismatch.");
                var modes = Enum.GetValues(typeof(GameModeType)).Cast<GameModeType>().Where(Game.Instance.IsModeActive).ToArray();
                if (!modes.Contains(GameModeType.Default) || modes.Any(m => m != GameModeType.Default && m != GameModeType.Pause) ||
                    Kingmaker.UI.DialogMessageBox.Instance.IsShown)
                    throw new InvalidOperationException("Unexpected mode/dialog in Whiteout area observation.");
            }
            finally { scene.Stop(); }
        }

        private void WriteWhiteoutWeather(string error)
        {
            WriteTeleportationForensicJson(WhiteoutWeatherPath, new { schemaVersion = 1, runId = _request.RunId,
                samples = _whiteoutWeatherSamples, events = _whiteoutWeatherEvents?.Events, assertions = _whiteoutWeatherAssertions,
                saveWriteObserved = _workingSaveSmoke != null && _workingSaveSmoke.WriteObserved,
                publication = "NONE; pure policy and observation only", weatherMutation = false, error });
        }
        private void StopWhiteoutWeather(RuntimeTestResult result)
        {
            if (_whiteoutWeatherSteps == null) return;
            var steps = _whiteoutWeatherSteps; _whiteoutWeatherSteps = null;
            try { steps.Dispose(); }
            catch (Exception error) { result.Status = "ERROR"; result.Diagnostics.Add(error.ToString()); }
            WriteWhiteoutWeather("Runner completed before weather observation finished.");
            result.EvidenceFiles.Add(WhiteoutWeatherPath);
        }
        private sealed class WhiteoutWeatherEvents : IWeatherChangeHandler, ISceneHandler
        {
            internal int Changes, Loads, Unloads;
            internal readonly List<string> Events = new List<string>();
            public void OnWeatherChange() { Changes++; Record("weather-change"); }
            public void OnAreaDidLoad() { Loads++; Record("area-loaded"); }
            public void OnAreaBeginUnloading() { Unloads++; Record("area-unloading"); }
            private void Record(string name)
            {
                var area = Game.Instance.CurrentlyLoadedArea; var weather = WeatherSystemBehaviour.Instance;
                Events.Add(name + ";area=" + (area == null ? "<none>" : area.AssetGuid) + ";visual=" +
                    (weather == null ? "<absent>" : weather.WeatherType.ToString()) + ";actual=" + Game.Instance.Player.Weather.ActualWeather);
            }
        }
    }
}
