using System;
using System.IO;
using System.Text;
using Harmony12;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityModManagerNet;

namespace KmgLoaderCompatHarness
{
    /// <summary>
    /// Test-only UMM entry point for loader-compatibility runs in a disposable game copy. Inert unless the game was
    /// launched with -kmgLoaderCompatRequest=&lt;path&gt;. Never packaged with the product.
    /// </summary>
    public static class HarnessMain
    {
        private const string RequestArgument = "-kmgLoaderCompatRequest=";
        private static HarnessRunner runner;
        private static UnityModManager.ModEntry entry;
        private static HarmonyInstance harmony;
        private static Evidence evidence;
        private static bool productHooksInstalled;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            entry = modEntry;
            string requestPath = null;
            foreach (string argument in Environment.GetCommandLineArgs())
            {
                if (argument.StartsWith(RequestArgument, StringComparison.Ordinal)) requestPath = argument.Substring(RequestArgument.Length).Trim('"');
            }

            if (requestPath == null)
            {
                modEntry.Logger.Log("No " + RequestArgument + " argument; harness inactive.");
                return true;
            }

            HarnessRequest request = JsonConvert.DeserializeObject<HarnessRequest>(File.ReadAllText(requestPath));
            Directory.CreateDirectory(request.EvidenceDir);
            evidence = new Evidence(request.EvidenceDir);
            JObject result = new JObject { ["runId"] = request.RunId, ["requestPath"] = requestPath, ["startedUtc"] = DateTime.UtcNow.ToString("o") };
            try
            {
                harmony = HarmonyInstance.Create("KmgLoaderCompatHarness");
                result["isolation"] = ProfileIsolation.Apply(harmony, request.ProfileDir);
                LoggedExceptions.Install(harmony);
                result["loggerExceptionHooks"] = LoggedExceptions.Patched;
            }
            catch (Exception exception)
            {
                result["status"] = "failed";
                result["failure"] = "isolation: " + exception;
                evidence.WriteJson("harness-result.json", result);
                modEntry.Logger.Log("Harness isolation failed: " + exception);
                UnityEngine.Application.Quit();
                return false;
            }

            evidence.WriteJson("harness-stage-loaded.json", result);
            runner = new HarnessRunner(request, evidence, result);
            modEntry.OnUpdate = OnUpdate;
            modEntry.Logger.Log("Harness run " + request.RunId + " armed: " + result["isolation"]);
            return true;
        }

        private static void OnUpdate(UnityModManager.ModEntry modEntry, float deltaTime)
        {
            try
            {
                if (!productHooksInstalled)
                {
                    // After every mod's Load: the product assembly is resolvable through its loader entry.
                    productHooksInstalled = true;
                    try { ToggleClick.Install(harmony); }
                    catch (Exception exception) { evidence.Event("toggleHookUnavailable", exception.Message); }
                }

                runner?.Tick();
            }
            catch (Exception exception)
            {
                entry.Logger.Log("Harness tick failed: " + exception);
                runner?.Finish(false, "harness exception: " + exception);
            }
        }
    }

    /// <summary>Atomic JSON documents and an append-only event log in the run's evidence directory.</summary>
    internal sealed class Evidence
    {
        private readonly string directory;
        private readonly string eventsPath;

        public Evidence(string directory)
        {
            this.directory = directory;
            eventsPath = Path.Combine(directory, "harness-events.jsonl");
        }

        public void WriteJson(string name, JToken value)
        {
            string target = Path.Combine(directory, name);
            string temporary = target + ".tmp";
            File.WriteAllText(temporary, value.ToString(Formatting.Indented), new UTF8Encoding(false));
            if (File.Exists(target)) File.Delete(target);
            File.Move(temporary, target);
        }

        public void Event(string name, object detail)
        {
            JObject line = new JObject { ["utc"] = DateTime.UtcNow.ToString("o"), ["t"] = UnityEngine.Time.realtimeSinceStartup, ["event"] = name };
            if (detail != null) line["detail"] = JToken.FromObject(detail);
            File.AppendAllText(eventsPath, line.ToString(Formatting.None) + "\n", new UTF8Encoding(false));
        }
    }
}
