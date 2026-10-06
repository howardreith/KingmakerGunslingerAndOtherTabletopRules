using System;
using System.IO;
using System.Linq;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.Visual.WeatherSystem;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private RuntimeTestResult RunWhiteoutWeatherCatalogObservation()
        {
            // One typed read of registered area metadata. This neither loads a
            // candidate area nor treats authored settings as actual scene weather.
            var parts = BlueprintBootstrap.Library.GetAllBlueprints().OfType<BlueprintAreaPart>()
                .Where(p => p != null).OrderBy(p => p.AssetGuid, StringComparer.Ordinal).ToArray();
            var candidates = parts.Where(p => p.OverrideWeather && p.Inclemency >= InclemencyType.Light &&
                (p.Weather == WeatherType.Rain || p.Weather == WeatherType.Snow)).ToArray();
            var root = BlueprintRoot.Instance.WeatherSettings;
            var rain = root.RainIntensitites.Values.ToArray();
            var snow = root.SnowIntensitites.Values.ToArray();
            string path = Path.Combine(_request.EvidenceDirectory, "whiteout-weather-catalog.json");
            WriteTeleportationForensicJson(path, new { schemaVersion = 1, runId = _request.RunId,
                qualification = "configured metadata only; not loaded-area weather or combat qualification",
                typedAreaPartCount = parts.Length, rainThresholds = rain, snowThresholds = snow,
                candidates = candidates.Select(p => new { guid = p.AssetGuid, nativeName = p.name, type = p.GetType().FullName,
                    indoor = p.IsIndoor, overrideWeather = p.OverrideWeather, weather = p.Weather.ToString(),
                    inclemency = p.Inclemency.ToString(), inclemencyCap = p.InclemencyCap.ToString() }).ToArray() });
            var assertions = new[] {
                Assertion("whiteout-weather-catalog-identities", "known observed mansion and Oleg area parts occur exactly once",
                    "parts=" + parts.Length + ";candidates=" + candidates.Length + ";indoorCandidates=" + candidates.Count(p => p.IsIndoor),
                    parts.Count(p => p.AssetGuid == "2849fdde28fe50f4d935bf2cf3405051") == 1 &&
                    parts.Count(p => p.AssetGuid == "ead426a6c23d39548a670ee515d77df4") == 1,
                    "typed BlueprintAreaPart registry read; exact IDs from the preceding native scene observations"),
                Assertion("whiteout-weather-thresholds", "five finite monotone intensity thresholds for both native precipitation types",
                    "rain=" + string.Join(",", rain) + ";snow=" + string.Join(",", snow),
                    ValidWhiteoutThresholds(rain) && ValidWhiteoutThresholds(snow),
                    "native WeatherRoot arrays read without changing any field, time, weather, scene or unit") };
            var result = CreateResult(assertions.All(a => a.Status == "PASS") ? "PASS" : "FAIL", assertions.ToList(), null);
            result.EvidenceFiles.Add(path);
            result.Diagnostics.Add("No save, scene transition, weather mutation, input, marker or patch. Candidate metadata is not proof of mechanical behavior.");
            return result;
        }
        private static bool ValidWhiteoutThresholds(float[] values)
        {
            return values.Length == 5 && values.All(v => !float.IsNaN(v) && !float.IsInfinity(v)) &&
                values.Skip(1).Select((v, i) => v >= values[i]).All(v => v);
        }
    }
}
