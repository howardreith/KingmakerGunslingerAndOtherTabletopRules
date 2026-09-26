using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.FavoredClass;
using KingmakerGunslinger.FavoredClass.Hooks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private static readonly Regex FcbMissingDependencyFixture = new Regex(
            @"^Manual_[0-9]+_KMG_FCB_PERSISTENCE_[0-9]{8}T[0-9]{13}Z_[0-9a-f]{32}_prepare\.zks$",
            RegexOptions.CultureInvariant);

        private static readonly Regex FcbBlueprintReference = new Regex("\"([0-9a-f]{32})\"",
            RegexOptions.CultureInvariant);

        // L06 natively (owner-authorized): in the disabled-host profile, the
        // transaction-owned fixture save made with the host (its builds hold
        // the host's favored class choice and per-class favored progressions
        // and bonus selections) is opened read-only through the native ZIP
        // saver. Every blueprint reference of its party and player state is
        // resolved against the live library, and each unresolvable one goes
        // through the game's own blueprint converter exactly as a load reads
        // it: the converter must throw (nothing is substituted), KMG's
        // observation must record it, and KMG's warning must name the host
        // and its state precisely before the unchanged native message. KMG's
        // own favored-class identities in the save all resolve. The fixture's
        // bytes and the saves folder are unchanged.
        private RuntimeTestResult RunFavoredClassMissingDependency()
        {
            var assertions = new List<RuntimeTestAssertion>();
            FavoredClassIntegrationStatus status = FavoredClassIntegrationStatusRegistry.Current;
            FavoredClassIntegrationAvailability availability = status.Availability;
            bool ready = (availability == FavoredClassIntegrationAvailability.HostDisabled ||
                    availability == FavoredClassIntegrationAvailability.HostAbsent) &&
                FavoredClassDependencyWarningPatches.ConverterInstalled && BlueprintBootstrap.Library != null &&
                Game.Instance.CurrentlyLoadedArea == null;
            assertions.Add(Assertion("fcb-missing-dependency-ready",
                "the Favored Class host is disabled or absent, the converter observation is installed and no area is loaded",
                status + ";converterObserved=" + FavoredClassDependencyWarningPatches.ConverterInstalled, ready,
                "FavoredClassIntegrationStatusRegistry; FavoredClassDependencyWarningPatches"));
            if (!ready)
                return CreateResult(RuntimeTestStatuses.Fail, assertions, null);
            var evidence = new JObject { ["availability"] = availability.ToString() };
            var failures = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            foreach (string key in new[] { "fixture", "native", "owned", "recorded", "warning", "hooks" })
                failures[key] = new List<string>();
            try
            {
                string folder = Path.Combine(Application.persistentDataPath, "Saved Games");
                Func<string> folderState = () => string.Join("|", Directory.GetFiles(folder)
                    .Select(path => new FileInfo(path))
                    .OrderBy(info => info.Name, StringComparer.Ordinal)
                    .Select(info => info.Name + ":" + info.Length + ":" + info.LastWriteTimeUtc.Ticks).ToArray());
                string[] fixtures = Directory.GetFiles(folder)
                    .Where(path => FcbMissingDependencyFixture.IsMatch(Path.GetFileName(path))).ToArray();
                evidence["fixtures"] = new JArray(fixtures.Select(Path.GetFileName));
                if (fixtures.Length != 1)
                    throw new InvalidOperationException("Expected exactly one transaction-owned fixture save; found " +
                        fixtures.Length + ".");
                string fixture = fixtures[0];
                string folderBefore = folderState();
                string hashBefore = FcbSha256(fixture);
                evidence["fixture"] = Path.GetFileName(fixture);
                evidence["fixtureSha256"] = hashBefore;

                // Read-only: the native ZIP saver's reads, disposed unchanged.
                string party, player;
                Type zip = typeof(Game).Assembly.GetType("Kingmaker.EntitySystem.Persistence.ZipSaver", true);
                var saver = (ISaver)Activator.CreateInstance(zip, BindingFlags.Instance | BindingFlags.Public |
                    BindingFlags.NonPublic, null, new object[] { fixture }, null);
                try
                {
                    party = saver.ReadJson("party");
                    player = saver.ReadJson("player");
                }
                finally
                {
                    saver.Dispose();
                }
                if (party == null || player == null)
                    throw new InvalidOperationException("The fixture save has no party or player state.");
                var references = new HashSet<string>(StringComparer.Ordinal);
                foreach (string text in new[] { party, player })
                    foreach (Match match in FcbBlueprintReference.Matches(text))
                        references.Add(match.Groups[1].Value);
                Dictionary<string, BlueprintScriptableObject> library = BlueprintBootstrap.Library.BlueprintsByAssetId;
                string[] missing = references.Where(guid => !library.ContainsKey(guid))
                    .OrderBy(guid => guid, StringComparer.Ordinal).ToArray();
                var owned = new HashSet<string>(FavoredClassIdentityCatalog.All.Select(value => value.Guid),
                    StringComparer.Ordinal);
                string[] ownedInSave = references.Where(owned.Contains).ToArray();
                evidence["references"] = references.Count;
                evidence["missing"] = new JArray(missing);
                evidence["ownedInSave"] = ownedInSave.Length;
                if (missing.Length == 0)
                    failures["native"].Add("no reference of the fixture is missing, so the profile proves nothing");
                if (ownedInSave.Length == 0)
                    failures["owned"].Add("the fixture holds no KMG favored-class identity");
                string[] ownedMissing = ownedInSave.Where(guid => !library.ContainsKey(guid)).ToArray();
                if (ownedMissing.Length != 0)
                    failures["owned"].Add("KMG identities are missing: " + string.Join(",", ownedMissing));

                // The native converter on every missing reference, as a load reads it.
                FavoredClassDependencyWarning.BeginLoad();
                JsonSerializer serializer = JsonSerializer.Create(DefaultJsonSettings.DefaultSettings);
                var thrown = new JArray();
                foreach (string guid in missing)
                {
                    try
                    {
                        object value;
                        using (var reader = new JsonTextReader(new StringReader("\"" + guid + "\"")))
                            // The converter reads any blueprint subclass field alike.
                            value = serializer.Deserialize<BlueprintFeature>(reader);
                        failures["native"].Add(guid + " converted to " + (value == null ? "null" : value.ToString()));
                    }
                    catch (JsonSerializationException exception)
                    {
                        if (exception.Message.IndexOf("Failed to load blueprint by guid " + guid,
                                StringComparison.Ordinal) < 0)
                            failures["native"].Add(guid + ": " + exception.Message);
                        else
                            thrown.Add(guid);
                    }
                }
                evidence["nativeThrew"] = thrown.Count;
                string[] recorded = FavoredClassDependencyWarning.Missing;
                evidence["recorded"] = recorded.Length;
                if (!new HashSet<string>(recorded, StringComparer.Ordinal).SetEquals(missing))
                    failures["recorded"].Add("recorded " + recorded.Length + " of " + missing.Length +
                        " missing references");

                // The warning the return to the main menu would carry.
                FavoredClassDependencyReport report = FavoredClassDependencyPolicy.Classify(recorded, availability,
                    FavoredClassDependencyWarning.KnownClassGuids());
                evidence["report"] = report.ToString();
                string native = "Cannot load game. Are you trying to load an older save?\nFailed to load blueprint by guid " +
                    (missing.Length == 0 ? "" : missing[0]);
                string shown = FavoredClassDependencyWarning.Augment(native);
                evidence["message"] = shown;
                if (report.Host < 2)
                    failures["warning"].Add("fewer than two host identities were attributed: " + report);
                foreach (string token in new[]
                {
                    "Kingmaker Gunslinger: this save uses favored-class content that is not loaded.",
                    "from Favored Class (ZFavoredClass 1.3.1, which is " +
                        FavoredClassDependencyPolicy.HostState(availability) + ")",
                    "Nothing was loaded and the save file was not changed.",
                    "do not save over it"
                })
                    if (shown == null || shown.IndexOf(token, StringComparison.Ordinal) < 0)
                        failures["warning"].Add("the warning lacks: " + token);
                if (shown == null || !shown.EndsWith("\n\n" + native, StringComparison.Ordinal))
                    failures["warning"].Add("the native message is not kept unchanged after the warning");
                if (FavoredClassDependencyWarning.Missing.Length != 0)
                    failures["warning"].Add("the records were not consumed by the warning");
                if (FavoredClassDependencyWarning.Augment("Error: unrelated") != "Error: unrelated")
                    failures["warning"].Add("an unrelated message was changed");

                string hashAfter = FcbSha256(fixture);
                string folderAfter = folderState();
                evidence["fixtureUnchanged"] = hashAfter == hashBefore;
                evidence["folderUnchanged"] = folderAfter == folderBefore;
                if (hashAfter != hashBefore)
                    failures["fixture"].Add("the fixture save changed");
                if (folderAfter != folderBefore)
                    failures["fixture"].Add("the saves folder changed");

                HarmonyInstance harmony = _context.Harmony;
                Func<MethodBase, Type, bool> prefixed = (method, owner) =>
                {
                    Patches patches = method == null ? null : harmony.GetPatchInfo(method);
                    return patches != null && patches.Prefixes.Any(value => value.patch != null &&
                        value.patch.DeclaringType == owner);
                };
                bool load = prefixed(typeof(Game).GetMethod("LoadGame", BindingFlags.Instance | BindingFlags.Public),
                    typeof(FavoredClassDependencyLoadPatch));
                bool menu = prefixed(typeof(Game).GetMethod("ResetToMainMenu", BindingFlags.Instance |
                    BindingFlags.Public), typeof(FavoredClassDependencyMenuPatch));
                Type converter = typeof(Game).Assembly.GetType(FavoredClassDependencyWarningPatches.ConverterTypeName,
                    false);
                bool read = prefixed(converter == null ? null : converter.GetMethod("ReadJson"),
                    typeof(FavoredClassDependencyWarningPatches));
                evidence["hooks"] = "load=" + load + ";menu=" + menu + ";converter=" + read;
                if (!load || !menu || !read)
                    failures["hooks"].Add("a warning hook is not installed: " + evidence["hooks"]);
            }
            catch (Exception exception)
            {
                failures["fixture"].Add(exception.GetType().Name + ": " + exception.Message);
                evidence["exception"] = exception.ToString();
            }
            string evidencePath = WriteFavoredClassEvidence("favored-class-missing-dependency.json", evidence);
            Action<string, string, string> add = (key, id, expectation) => assertions.Add(Assertion(id, expectation,
                Describe(null, failures[key]), failures[key].Count == 0,
                "native ZipSaver reads of the transaction-owned fixture; DefaultJsonSettings BlueprintConverter"));
            add("fixture", "fcb-missing-dependency-fixture-untouched",
                "exactly one transaction-owned fixture is read through the native ZIP saver and neither it nor the saves folder changes");
            add("native", "fcb-missing-dependency-no-substitution",
                "the fixture has missing references, and the game's own blueprint converter throws on every one exactly as a load does (nothing is substituted)");
            add("owned", "fcb-missing-dependency-kmg-identities",
                "every KMG favored-class identity the fixture holds still resolves, so its investments stay recorded");
            add("recorded", "fcb-missing-dependency-recorded",
                "KMG's converter observation records exactly the references the converter could not resolve");
            add("warning", "fcb-missing-dependency-warning",
                "the return to the main menu carries a precise warning (the host by name, version and state, the counts, that nothing was loaded or changed, how to recover, not to save over it) before the unchanged native message; unrelated messages are unchanged");
            add("hooks", "fcb-missing-dependency-hooks",
                "the load, main-menu and converter observations are installed as KMG prefixes");
            assertions.Add(Assertion("loaded-mod-version", _request.ExpectedModVersion,
                _context.ModEntry.Info.Version,
                _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                "Unity Mod Manager ModEntry.Info.Version"));
            RuntimeTestResult result = CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions, null);
            result.EvidenceFiles.Add(evidencePath);
            return result;
        }

        private static string FcbSha256(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
        }
    }
}
