using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityModManagerNet;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Alternate diagnostic entry point used only by a disposable UMM probe manifest.
    // KMG's normal entry point, context publication, PatchAll and blueprint bootstrap never run.
    public sealed class DisabledKmgCharacterCreationProbe
    {
        internal const string ProbeId = "KMGCharacterCreationProbe";
        private readonly ModContext _context;
        private readonly RuntimeTestRequest _request;
        private readonly Stopwatch _elapsed = Stopwatch.StartNew();
        private readonly DateTime _started = DateTime.UtcNow;
        private ElementalCharacterCreationBaselineScenario _session;
        private LibraryScriptableObject _library;
        private bool _complete;

        private DisabledKmgCharacterCreationProbe(ModContext context, RuntimeTestRequest request)
        { _context = context; _request = request; }

        public static bool Load(UnityModManager.ModEntry entry)
        {
            if (entry == null || entry.Info.Id != ProbeId) return false;
            RuntimeTestRequestDecision decision = RuntimeTestRequestParser.TryActivate(
                Environment.GetCommandLineArgs(), entry.Info.Version);
            if (!decision.Accepted || decision.Request.Scenario !=
                RuntimeTestScenarioCatalog.DisposableGlobalTraitsKmgDisabledControl) return false;
            try
            {
                EnsureProductionDisabled();
                using (new FileStream(Path.Combine(RuntimeTestRequest.EvidenceRoot,
                    ".kmg-run-" + decision.Request.RunId), FileMode.CreateNew, FileAccess.Write, FileShare.None)) { }
                Assembly assembly = typeof(DisabledKmgCharacterCreationProbe).Assembly;
                ModContext context = ModContext.Create(entry, assembly, ModLogger.Create(entry, assembly));
                RuntimeTestRunner.RecordEarlyIdentity(context);
                var probe = new DisabledKmgCharacterCreationProbe(context, decision.Request);
                entry.OnUpdate += probe.Update;
                return true;
            }
            catch (Exception error) { entry.Logger.Log("Disabled KMG control refused: " + error); return false; }
        }

        private static UnityModManager.ModEntry[] Entries()
        {
            var values = (IEnumerable)typeof(UnityModManager).GetField("modEntries",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(null);
            return values.Cast<UnityModManager.ModEntry>().ToArray();
        }

        private static void EnsureProductionDisabled()
        {
            UnityModManager.ModEntry entry = Entries().Single(value => value.Info.Id == "KingmakerGunslinger");
            ModContext published;
            string loader = typeof(Main).GetField("_state", BindingFlags.Static | BindingFlags.NonPublic)
                .GetValue(null).ToString();
            if (entry.Loaded || entry.Active || ModContext.TryGet(out published) ||
                BlueprintBootstrap.Library != null || loader != "NotStarted")
                throw new InvalidOperationException("Profile B requires KMG's actual UMM entry to remain unloaded and inactive.");
        }

        private void Update(UnityModManager.ModEntry entry, float delta)
        {
            if (_complete) return;
            try
            {
                EnsureProductionDisabled();
                if (_elapsed.Elapsed.TotalSeconds > _request.TimeoutSeconds + _request.StartupTimeoutSeconds)
                    throw new TimeoutException("Disabled-control observation timed out.");
                if (_session == null)
                {
                    if (ResourcesLibrary.Preloading || Game.Instance == null || Game.Instance.UI == null ||
                        Game.Instance.UI.CharacterBuildController == null) return;
                    _library = (LibraryScriptableObject)typeof(ResourcesLibrary).GetField("s_LibraryObject",
                        BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
                    if (_library == null) return;
                    VerifyUnmodifiedLibrary();
                    ElementalCharacterCreationRoutingObserver.ArmDisabledControl(_request, _library);
                    _session = new ElementalCharacterCreationBaselineScenario(_context, _request);
                }
                _session.Poll();
                if (!_session.Complete) return;
                VerifyUnmodifiedLibrary();
                Complete(_session.Result);
            }
            catch (Exception error)
            {
                if (_session != null && !_session.Complete) _session.Abort("disabled-control host: " + error);
                RuntimeTestResult result = _session == null ? new RuntimeTestResult {
                    SchemaVersion = 1, RunId = _request.RunId, Scenario = _request.Scenario,
                    LoadedModVersion = _context.ModEntry.Info.Version, RuntimeIdentity = _context.Assembly.FullName,
                    GitCommit = RuntimeBuildIdentity.Capture(_context.Assembly, entry.Info.Version).GitCommit,
                    Assertions = new System.Collections.Generic.List<RuntimeTestAssertion>(),
                    Diagnostics = new System.Collections.Generic.List<string>(),
                    Warnings = new System.Collections.Generic.List<string>(),
                    EvidenceFiles = new System.Collections.Generic.List<string>() } : _session.Result;
                result.Status = RuntimeTestStatuses.Fail; result.ExceptionSummary = error.ToString();
                Complete(result);
            }
        }

        private void VerifyUnmodifiedLibrary()
        {
            EnsureProductionDisabled();
            if (_library.GetAllBlueprints().Any(value => value != null && value.name.StartsWith("KMG_", StringComparison.Ordinal)))
                throw new InvalidOperationException("Production KMG blueprints are present in the disabled control.");
            var registry = HarmonyInstance.Create(ProbeId);
            bool kmgPatched = registry.GetPatchedMethods().Any(method => {
                Patches patches = registry.GetPatchInfo(method);
                return patches.Prefixes.Concat(patches.Postfixes).Concat(patches.Transpilers)
                    .Any(patch => patch.owner == "KingmakerGunslinger");
            });
            if (kmgPatched) throw new InvalidOperationException("Production KMG Harmony patches are active in Profile B.");
        }

        private void Complete(RuntimeTestResult result)
        {
            _complete = true;
            _context.ModEntry.OnUpdate -= Update;
            result.GameVersion = Kingmaker.GameVersion.GetVersion();
            result.StartUtc = _started.ToString("o"); result.EndUtc = DateTime.UtcNow.ToString("o");
            result.DurationMilliseconds = _elapsed.ElapsedMilliseconds;
            result.AutomaticExitRequested = _request.ExitAfterCompletion;
            result.AutomaticExitInitiated = _request.ExitAfterCompletion;
            result.EvidenceDirectory = _request.EvidenceDirectory;
            string proofPath = Path.Combine(_request.EvidenceDirectory, "kmg-disabled-control.json");
            RuntimeTestResultWriter.WriteAtomic(proofPath, new JObject {
                ["runId"] = _request.RunId, ["probeEntry"] = ProbeId,
                ["productionEntryNeverInvoked"] = typeof(Main).GetField("_state",
                    BindingFlags.Static | BindingFlags.NonPublic).GetValue(null).ToString() == "NotStarted",
                ["purpose"] = "Two ordinary Trait choices through native final review; no campaign commit or save load",
                ["mods"] = new JArray(Entries().Select(mod => new JObject { ["id"] = mod.Info.Id,
                    ["version"] = mod.Info.Version, ["loaded"] = mod.Loaded, ["active"] = mod.Active }))
            }.ToString(Formatting.Indented));
            result.EvidenceFiles.Add(proofPath);
            RuntimeTestResultWriter.Write(result, _request.EvidenceDirectory);
            RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory, "runtime-stage-final-result-flushed.json"),
                new JObject { ["runId"] = _request.RunId, ["stage"] = "final-result-flushed" }.ToString());
            if (_request.ExitAfterCompletion) UnityEngine.Application.Quit();
        }
    }
}
