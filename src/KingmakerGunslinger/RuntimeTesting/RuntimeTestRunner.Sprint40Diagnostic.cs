using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
            if (_request.Parameters?["sprint40DiagnosticRecovery"]?.Type !=
                    JTokenType.Boolean ||
                !(bool)_request.Parameters["sprint40DiagnosticRecovery"]) return;
            if (ExpandedSummoningBlueprints.Registered?.BySymbol.ContainsKey(
                    "KMG.Summoning.Infernal.IceDevil.SpearFrost") != true)
                throw new InvalidOperationException(
                    "Sprint40 private spear enchantment was not registered.");
            var producer = AppDomain.CurrentDomain.GetAssemblies().Select(assembly =>
                assembly.GetType("ZFavoredClass.Main", false)).FirstOrDefault(type => type != null);
            if (producer == null)
                throw new InvalidOperationException(
                    "ZFavoredClass diagnostic producer assembly was not loaded.");
            string mods = Path.GetDirectoryName(_context.ModEntry.Path.TrimEnd(
                Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            string path = Path.Combine(mods, "ZFavoredClass", "loaded_blueprints.txt");
            byte[] bytes = File.ReadAllBytes(path);
            string hash = Sprint40DiagnosticSha256(bytes);
            string copy = Path.Combine(_request.EvidenceDirectory,
                "sprint40-generated-diagnostic.txt");
            File.WriteAllBytes(copy, bytes);
            if (Sprint40DiagnosticSha256(File.ReadAllBytes(copy)) != hash)
                throw new InvalidOperationException(
                    "Sprint40 generated diagnostic evidence copy differs.");

            var rows = new List<string>();
            using (var reader = new StringReader(Encoding.UTF8.GetString(bytes)))
            {
                string row;
                while ((row = reader.ReadLine()) != null) rows.Add(row);
            }

            int resolved = 0;
            bool structural = rows.Count > 0;
            var names = new HashSet<string>(StringComparer.Ordinal);
            var problems = new JArray();
            foreach (string row in rows)
            {
                string[] fields = row.Split('\t');
                Guid parsed;
                bool valid = fields.Length == 3 &&
                    !string.IsNullOrWhiteSpace(fields[0]) &&
                    Guid.TryParseExact(fields[1], "N", out parsed) &&
                    !string.IsNullOrWhiteSpace(fields[2]) &&
                    names.Add(fields[0]);
                structural &= valid;
                Kingmaker.Blueprints.BlueprintScriptableObject blueprint;
                bool live = valid && BlueprintBootstrap.Library.BlueprintsByAssetId
                    .TryGetValue(fields[1], out blueprint) && blueprint != null &&
                    blueprint.GetType().FullName == fields[2];
                if (live) resolved++;
                else if (problems.Count < 5) problems.Add(row);
            }
            var info = new FileInfo(path);
            DateTime evidenceCreatedUtc = Directory.GetCreationTimeUtc(
                _request.EvidenceDirectory);
            bool fresh = info.LastWriteTimeUtc >= evidenceCreatedUtc;
            bool allLive = structural && resolved == rows.Count;
            bool bootstrap = BlueprintBootstrap.IsInitialized;
            bool exactOriginal = hash ==
                "590ca0e2f5400097d215aeb3764072e4f4da56058a5820588d510751d3a64123" &&
                info.Length == 135399;
            bool exact = bootstrap && fresh && allLive && exactOriginal;
            var receipt = new JObject {
                ["scenario"] = _request.Scenario, ["path"] = path,
                ["producer"] = "ZFavoredClass.Main.LibraryScriptableObject_LoadDictionary_Patch.Postfix -> CallOfTheWild.Helpers.GuidStorage.dump",
                ["producerAssembly"] = producer.Assembly.FullName,
                ["actualRows"] = rows.Count, ["resolvedLiveRows"] = resolved,
                ["structuralRowsValid"] = structural,
                ["allRowsResolveLive"] = allLive,
                ["bootstrapInitialized"] = bootstrap,
                ["evidenceDirectoryCreatedUtc"] = evidenceCreatedUtc.ToString("o"),
                ["freshNativeWrite"] = fresh,
                ["exactOriginalRegeneration"] = exact,
                ["sha256"] = hash, ["length"] = info.Length,
                ["creationUtcTicks"] = info.CreationTimeUtc.Ticks,
                ["writeUtcTicks"] = info.LastWriteTimeUtc.Ticks,
                ["attributes"] = (int)info.Attributes,
                ["evidenceCopyPath"] = copy,
                ["evidenceCopySha256"] = hash,
                ["invalidOrUnresolvedRows"] = problems };
            string evidence = Path.Combine(_request.EvidenceDirectory, "sprint40-generated-diagnostic.json");
            RuntimeTestResultWriter.WriteAtomic(evidence, receipt.ToString(Formatting.Indented));
            evidenceFiles.Add(copy);
            evidenceFiles.Add(evidence);
            assertions.Add(Assertion("sprint40-generated-diagnostic-producer",
                "fresh exact original bytes; every emitted row structurally valid and resolved to its live native blueprint type",
                receipt.ToString(Formatting.None), exact,
                "Read-only post-producer file/library validation plus an evidence copy captured before transaction cleanup."));
        }

        private static string Sprint40DiagnosticSha256(byte[] bytes)
        {
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "")
                    .ToLowerInvariant();
        }
    }
}
