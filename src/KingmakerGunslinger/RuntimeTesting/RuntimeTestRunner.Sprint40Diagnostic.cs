using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private void ObserveSprint40GeneratedDiagnostic(List<RuntimeTestAssertion> assertions,
            List<string> evidenceFiles)
        {
            if (ExpandedSummoningBlueprints.Registered?.BySymbol.ContainsKey(
                    "KMG.Summoning.Infernal.IceDevil.SpearFrost") != true) return;
            var producer = AppDomain.CurrentDomain.GetAssemblies().Select(assembly =>
                assembly.GetType("ZFavoredClass.Main", false)).FirstOrDefault(type => type != null);
            if (producer == null) return; // Other installed profiles do not own this producer.
            var storage = AppDomain.CurrentDomain.GetAssemblies().Select(assembly =>
                assembly.GetType("CallOfTheWild.Helpers+GuidStorage", false)).Single(type => type != null);
            var entries = storage.GetField("guids_in_use", BindingFlags.Static | BindingFlags.NonPublic)
                ?.GetValue(null) as IDictionary;
            if (entries == null) throw new InvalidOperationException("Native GuidStorage dictionary absent.");
            string mods = Path.GetDirectoryName(_context.ModEntry.Path.TrimEnd(
                Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            string path = Path.Combine(mods, "ZFavoredClass", "loaded_blueprints.txt");
            var expected = new List<string>();
            foreach (DictionaryEntry entry in entries)
            {
                Kingmaker.Blueprints.BlueprintScriptableObject blueprint;
                if (BlueprintBootstrap.Library.BlueprintsByAssetId.TryGetValue((string)entry.Value, out blueprint) &&
                    blueprint != null)
                    expected.Add(entry.Key + "\t" + entry.Value + "\t" + blueprint.GetType().FullName);
            }
            string hash;
            // A closed native writer and an immutable read while comparing the
            // actual producer dictionary with every emitted row, not a name/count.
            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var sha = SHA256.Create())
                hash = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
            string[] actual = File.ReadAllLines(path);
            bool exact = expected.Count > 0 && expected.SequenceEqual(actual, StringComparer.Ordinal);
            var info = new FileInfo(path);
            var receipt = new JObject {
                ["scenario"] = _request.Scenario, ["path"] = path,
                ["producer"] = "ZFavoredClass.Main.LibraryScriptableObject_LoadDictionary_Patch.Postfix -> CallOfTheWild.Helpers.GuidStorage.dump",
                ["producerAssembly"] = producer.Assembly.FullName,
                ["storageAssembly"] = storage.Assembly.FullName,
                ["dictionaryEntries"] = entries.Count, ["expectedNativeRows"] = expected.Count,
                ["actualRows"] = actual.Length, ["allNativeRowsExact"] = exact,
                ["sha256"] = hash, ["length"] = info.Length,
                ["creationUtcTicks"] = info.CreationTimeUtc.Ticks,
                ["writeUtcTicks"] = info.LastWriteTimeUtc.Ticks,
                ["attributes"] = (int)info.Attributes,
                ["mismatchRows"] = new JArray(actual.Except(expected).Take(5)) };
            string evidence = Path.Combine(_request.EvidenceDirectory, "sprint40-generated-diagnostic.json");
            RuntimeTestResultWriter.WriteAtomic(evidence, receipt.ToString(Formatting.Indented));
            evidenceFiles.Add(evidence);
            assertions.Add(Assertion("sprint40-generated-diagnostic-producer",
                "Every native dictionary entry with a live blueprint is emitted exactly once in native order",
                receipt.ToString(Formatting.None), exact,
                "Read-only native producer/library/file comparison; no regeneration or product behavior supplied by observer."));
        }
    }
}
