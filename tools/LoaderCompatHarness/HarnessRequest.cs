using System.Collections.Generic;
using Newtonsoft.Json;

namespace KmgLoaderCompatHarness
{
    /// <summary>One run, read from the path given by -kmgLoaderCompatRequest=&lt;path&gt;.</summary>
    public sealed class HarnessRequest
    {
        [JsonProperty("runId")] public string RunId;
        [JsonProperty("evidenceDir")] public string EvidenceDir;

        /// <summary>Replacement for the persistent-data path: holds the disposable "Saved Games" and area cache.</summary>
        [JsonProperty("profileDir")] public string ProfileDir;

        /// <summary>Save (inside profileDir\Saved Games) to load from the main menu; empty runs the steps at the main menu.</summary>
        [JsonProperty("save")] public string Save;

        [JsonProperty("timeoutSeconds")] public double TimeoutSeconds = 900;

        /// <summary>False when another runner (the product's guarded scenario) owns the process exit.</summary>
        [JsonProperty("exitAfterCompletion")] public bool ExitAfterCompletion = true;

        [JsonProperty("steps")] public List<HarnessStep> Steps = new List<HarnessStep>();
    }

    public sealed class HarnessStep
    {
        [JsonProperty("op")] public string Op;
        [JsonProperty("label")] public string Label;
        [JsonProperty("match")] public string Match;
        [JsonProperty("type")] public string Type;
        [JsonProperty("expect")] public string Expect;
        [JsonProperty("seconds")] public double Seconds;
        [JsonProperty("timeoutSeconds")] public double TimeoutSeconds = 120;
        [JsonProperty("save")] public bool Save;

        public override string ToString() => Op + (Label != null ? "[" + Label + "]" : string.Empty) + (Match != null ? "(" + Match + ")" : string.Empty);
    }
}
