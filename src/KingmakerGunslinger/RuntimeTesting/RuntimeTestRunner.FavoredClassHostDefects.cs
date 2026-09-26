using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Selection;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.FavoredClass;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // H02 (unsupported and partially initialized hosts), H04 and H05 by
        // owner-authorized simulation. The live host's own observations are
        // cloned, one fact is changed per case, and the production gates
        // (EvaluateBinary, EvaluateHost, EvaluateGunslinger, the availability
        // mapping and the publication planner) run on the clone; a plan is
        // never committed. Nothing live is written: the host's global state,
        // the live status and the published graph are compared before and
        // after. The real disabled host (H02) runs natively in its own staged
        // profile through the host-state lane.
        private RuntimeTestResult RunFavoredClassHostDefects()
        {
            var assertions = new List<RuntimeTestAssertion>();
            FavoredClassIntegrationStatus status = FavoredClassIntegrationStatusRegistry.Current;
            FavoredClassHostHandles host = FavoredClassIntegrationCoordinator.Host;
            FavoredClassBlueprintSet leaves = BlueprintBootstrap.FavoredClassLeaves;
            bool ready = status.Availability == FavoredClassIntegrationAvailability.Published &&
                host != null && host.Binary != null && host.Readiness != null && leaves != null &&
                host.Decision.IsReady && host.GunslingerDecision.IsReady;
            assertions.Add(Assertion("fcb-host-defects-ready",
                "the exact host is published with both gates ready, so each simulated defect starts from the qualified observations",
                status.ToString(), ready, "FavoredClassIntegrationCoordinator.Host observations"));
            if (!ready)
                return CreateResult(RuntimeTestStatuses.Fail, assertions, null);
            var evidence = new JObject();
            var failures = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            foreach (string key in new[] { "binary", "partial", "gunslinger", "untouched" })
                failures[key] = new List<string>();
            string hostBefore = FcbHostGlobalState(host);
            string statusBefore = status.ToString();
            string graphBefore = FcbPublishedGraph(leaves);
            FavoredClassProfileState profile = FavoredClassRuntime.Profile;
            bool gunslingerModule = _context.FeatureModules.Active.Gunslinger;
            FavoredClassPublication live = FavoredClassPublication.Plan(leaves, host, profile, gunslingerModule, null);
            evidence["livePlan"] = new JObject
            {
                ["surfaces"] = live.Surfaces.Count,
                ["skipped"] = new JArray(live.Skipped)
            };

            // H05 and H02 (unsupported): the binary gate on changed clones.
            var binaryCases = new JArray();
            string otherSha = FcbFlipHex(host.Binary.HostFileSha256);
            var binaries = new[]
            {
                Tuple.Create("same-version-other-sha256", (Action<FavoredClassHostObservation>)(value =>
                    value.HostFileSha256 = otherSha), "binary-sha256"),
                Tuple.Create("same-version-other-mvid", (Action<FavoredClassHostObservation>)(value =>
                    value.HostModuleVersionId = FcbFlipHex(value.HostModuleVersionId.Replace("-", ""))), "binary-mvid"),
                Tuple.Create("same-version-changed-core-load", (Action<FavoredClassHostObservation>)(value =>
                    value.MethodIlSha256[FavoredClassHostContract.CoreLoadKey] =
                        FcbFlipHex(value.MethodIlSha256[FavoredClassHostContract.CoreLoadKey])),
                    "unverified-behavior:" + FavoredClassHostContract.CoreLoadKey),
                Tuple.Create("same-version-missing-member", (Action<FavoredClassHostObservation>)(value =>
                    value.MissingMember = FavoredClassHostContract.BonusSelectionMapField), "required-member"),
                Tuple.Create("other-version-other-sha256", (Action<FavoredClassHostObservation>)(value =>
                {
                    value.HostModVersion = "1.3.2";
                    value.HostFileSha256 = otherSha;
                }), "binary-sha256"),
                Tuple.Create("dependency-other-sha256", (Action<FavoredClassHostObservation>)(value =>
                    value.CallOfTheWildFileSha256 = FcbFlipHex(value.CallOfTheWildFileSha256)), "dependency-sha256"),
                Tuple.Create("dependency-not-loaded", (Action<FavoredClassHostObservation>)(value =>
                    value.CallOfTheWildAssemblyLoaded = false), "dependency-not-loaded"),
            };
            foreach (var entry in binaries)
            {
                FavoredClassHostObservation clone = FcbCloneBinary(host.Binary);
                entry.Item2(clone);
                FavoredClassHostDecision decision = FavoredClassHostContract.EvaluateBinary(clone);
                FavoredClassIntegrationAvailability availability =
                    FavoredClassIntegrationStatus.FromHostState(decision.State);
                string planned = FcbPlanOutcome(leaves, FcbSimulatedHandles(host, decision, decision, clone,
                    host.Readiness, null), profile, gunslingerModule);
                binaryCases.Add(new JObject
                {
                    ["case"] = entry.Item1,
                    ["versionLabel"] = clone.HostModVersion,
                    ["decision"] = decision.ToString(),
                    ["availability"] = availability.ToString(),
                    ["plan"] = planned
                });
                if (decision.State != FavoredClassHostState.UnsupportedBinary || decision.Reason != entry.Item3 ||
                    availability != FavoredClassIntegrationAvailability.UnsupportedBinary ||
                    !planned.StartsWith("refused:", StringComparison.Ordinal))
                    failures["binary"].Add(entry.Item1 + ": " + decision + " -> " + availability + ", " + planned);
            }
            // The version label is diagnostic only: it neither admits a
            // changed binary nor rejects the exact one.
            FavoredClassHostObservation label = FcbCloneBinary(host.Binary);
            label.HostModVersion = "9.9.9";
            FavoredClassHostDecision labelDecision = FavoredClassHostContract.EvaluateBinary(label);
            binaryCases.Add(new JObject { ["case"] = "label-only-exact-binary", ["decision"] = labelDecision.ToString() });
            if (!labelDecision.IsReady)
                failures["binary"].Add("the exact binary under another version label was rejected: " + labelDecision);
            evidence["binary"] = binaryCases;

            // H02 (partially initialized): the host-wide gate on changed clones.
            var partialCases = new JArray();
            var partials = new[]
            {
                Tuple.Create("library-unassigned", (Action<FavoredClassHostReadinessObservation>)(value =>
                    value.LibraryAssigned = false)),
                Tuple.Create("core-load-incomplete", (Action<FavoredClassHostReadinessObservation>)(value =>
                    value.CoreLoadCompleted = false)),
                Tuple.Create("favored-class-selection-missing", (Action<FavoredClassHostReadinessObservation>)(value =>
                    value.FavoredClassSelectionPresent = false)),
            };
            foreach (var entry in partials)
            {
                FavoredClassHostReadinessObservation clone = FcbCloneReadiness(host.Readiness);
                entry.Item2(clone);
                FavoredClassHostDecision hostDecision = FavoredClassHostContract.EvaluateHost(host.Decision, clone);
                FavoredClassHostDecision gunslinger = FavoredClassHostContract.EvaluateGunslinger(hostDecision, clone);
                FavoredClassIntegrationAvailability availability =
                    FavoredClassIntegrationStatus.FromHostState(hostDecision.State);
                string planned = FcbPlanOutcome(leaves, FcbSimulatedHandles(host, hostDecision, gunslinger,
                    host.Binary, clone, null), profile, gunslingerModule);
                partialCases.Add(new JObject
                {
                    ["case"] = entry.Item1,
                    ["host"] = hostDecision.ToString(),
                    ["gunslinger"] = gunslinger.ToString(),
                    ["availability"] = availability.ToString(),
                    ["plan"] = planned
                });
                if (hostDecision.State != FavoredClassHostState.IncompleteInitialization ||
                    hostDecision.Reason != entry.Item1 ||
                    availability != FavoredClassIntegrationAvailability.HostIncomplete ||
                    !planned.StartsWith("refused:", StringComparison.Ordinal))
                    failures["partial"].Add(entry.Item1 + ": " + hostDecision + " -> " + availability + ", " + planned);
            }
            evidence["partial"] = partialCases;

            // H04: the host initialized without a Gunslinger entry. The
            // host-wide gate stays ready; only Gunslinger rows are blocked, the
            // rest of the plan is exactly the live one, and nothing touches a
            // Gunslinger selection.
            string gunslingerClass = host.Readiness.GunslingerClassGuid;
            var gunslingerCases = new JArray();
            var gunslingers = new[]
            {
                Tuple.Create("gunslinger-not-scanned", (Action<FavoredClassHostReadinessObservation>)(value =>
                {
                    value.GunslingerProgressionGuid = null;
                    value.GunslingerBonusSelectionGuid = null;
                    value.GunslingerProgressionOffered = false;
                    value.GunslingerProgressionLevels = 0;
                    value.GunslingerLevelsGrantBonusSelection = false;
                    value.GenericHitPointLeafPresent = true;
                    value.GenericSkillLeavesPresent = true;
                }), true),
                Tuple.Create("gunslinger-progression-not-offered", (Action<FavoredClassHostReadinessObservation>)(value =>
                    value.GunslingerProgressionOffered = false), false),
                Tuple.Create("gunslinger-progression-shape", (Action<FavoredClassHostReadinessObservation>)(value =>
                    value.GunslingerProgressionLevels = 10), false),
            };
            HashSet<string> liveNonGunslinger = new HashSet<string>(live.Surfaces.Where(surface =>
                surface.Pair.Effect.ClassFamily != FavoredClassCatalog.Gunslinger).Select(surface => surface.Key),
                StringComparer.Ordinal);
            foreach (var entry in gunslingers)
            {
                FavoredClassHostReadinessObservation clone = FcbCloneReadiness(host.Readiness);
                entry.Item2(clone);
                FavoredClassHostDecision hostDecision = FavoredClassHostContract.EvaluateHost(host.Decision, clone);
                FavoredClassHostDecision gunslinger = FavoredClassHostContract.EvaluateGunslinger(hostDecision, clone);
                // Without a scan the host's map has no Gunslinger entry: a copy
                // of the live map without it (the live map is never changed).
                Dictionary<string, BlueprintFeatureSelection> map = host.BonusSelections
                    .Where(value => !entry.Item3 || !string.Equals(value.Key, gunslingerClass, StringComparison.Ordinal))
                    .ToDictionary(value => value.Key, value => value.Value, StringComparer.Ordinal);
                FavoredClassHostHandles simulated = FcbSimulatedHandles(host, hostDecision, gunslinger, host.Binary,
                    clone, map);
                FavoredClassPublication plan = FavoredClassPublication.Plan(leaves, simulated, profile,
                    gunslingerModule, null);
                string[] gunslingerSkips = plan.Skipped.Where(value => leaves.Pairs.Any(pair =>
                    pair.Effect.ClassFamily == FavoredClassCatalog.Gunslinger &&
                    value.StartsWith(pair.Effect.Id + ":", StringComparison.Ordinal))).ToArray();
                var planned = new HashSet<string>(plan.Surfaces.Select(surface => surface.Key), StringComparer.Ordinal);
                bool touchesGunslinger = plan.Surfaces.Any(surface =>
                    surface.Pair.Effect.ClassFamily == FavoredClassCatalog.Gunslinger ||
                    (host.GunslingerSelection != null && ReferenceEquals(surface.Selection, host.GunslingerSelection)));
                gunslingerCases.Add(new JObject
                {
                    ["case"] = entry.Item1,
                    ["host"] = hostDecision.ToString(),
                    ["gunslinger"] = gunslinger.ToString(),
                    ["surfaces"] = plan.Surfaces.Count,
                    ["gunslingerSkipped"] = gunslingerSkips.Length,
                    ["skipReasons"] = new JArray(gunslingerSkips.Select(value => value.Substring(value.IndexOf(':') + 1))
                        .Distinct()),
                    ["touchesGunslinger"] = touchesGunslinger,
                    ["otherFamiliesAsLive"] = planned.SetEquals(liveNonGunslinger)
                });
                string reason = "gunslinger-not-ready:" + entry.Item1;
                if (!hostDecision.IsReady || gunslinger.State != FavoredClassHostState.GunslingerMissing ||
                    gunslinger.Reason != entry.Item1 || touchesGunslinger || !planned.SetEquals(liveNonGunslinger) ||
                    gunslingerSkips.Length == 0 || gunslingerSkips.Any(value =>
                        !value.EndsWith(":" + reason, StringComparison.Ordinal) &&
                        !value.EndsWith(":profile-disabled", StringComparison.Ordinal)))
                    failures["gunslinger"].Add(entry.Item1 + ": host " + hostDecision + ", gunslinger " + gunslinger +
                        ", surfaces " + plan.Surfaces.Count + ", touches Gunslinger " + touchesGunslinger);
            }
            evidence["gunslinger"] = gunslingerCases;

            string hostAfter = FcbHostGlobalState(host);
            string graphAfter = FcbPublishedGraph(leaves);
            string statusAfter = FavoredClassIntegrationStatusRegistry.Current.ToString();
            evidence["hostGlobalState"] = hostBefore;
            evidence["publishedGraph"] = graphBefore;
            if (hostAfter != hostBefore)
                failures["untouched"].Add("the host's global state changed: " + hostAfter);
            if (graphAfter != graphBefore)
                failures["untouched"].Add("the published graph changed");
            if (statusAfter != statusBefore)
                failures["untouched"].Add("the live status changed: " + statusAfter);
            string evidencePath = WriteFavoredClassEvidence("favored-class-host-defects.json", evidence);
            Action<string, string, string> add = (key, id, expectation) => assertions.Add(Assertion(id, expectation,
                Describe(null, failures[key]), failures[key].Count == 0,
                "FavoredClassHostContract gates and FavoredClassPublication.Plan on cloned live observations"));
            add("binary", "fcb-host-defects-binary",
                "H05 and H02 (unsupported): the same version label with a changed SHA-256, MVID, Core.load body or required member, another version, or a changed or unloaded dependency is UnsupportedBinary with its specific reason and nothing can be planned; the version label alone decides nothing");
            add("partial", "fcb-host-defects-partial",
                "H02 (partially initialized): an unassigned library, an incomplete Core.load or a missing favored class choice is HostIncomplete with its specific reason and nothing can be planned");
            add("gunslinger", "fcb-host-defects-gunslinger-missing",
                "H04: a host without a Gunslinger entry (not scanned, not offered, wrong shape) blocks exactly the Gunslinger rows with their reason, touches no Gunslinger selection, plans every other family exactly as live, and never rebuilds the host");
            add("untouched", "fcb-host-defects-untouched",
                "the host's global state (maps, favored class choice, Core.load's last assignment), the live status and the published graph are unchanged by every simulation");
            assertions.Add(Assertion("loaded-mod-version", _request.ExpectedModVersion,
                _context.ModEntry.Info.Version,
                _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                "Unity Mod Manager ModEntry.Info.Version"));
            RuntimeTestResult result = CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions, null);
            result.EvidenceFiles.Add(evidencePath);
            return result;
        }

        private static FavoredClassHostHandles FcbSimulatedHandles(FavoredClassHostHandles live,
            FavoredClassHostDecision decision, FavoredClassHostDecision gunslinger, FavoredClassHostObservation binary,
            FavoredClassHostReadinessObservation readiness, IDictionary bonusSelections)
        {
            IDictionary map = bonusSelections ?? live.BonusSelections.ToDictionary(value => value.Key,
                value => value.Value, StringComparer.Ordinal);
            return new FavoredClassHostHandles(decision, gunslinger, binary, readiness,
                decision.IsReady ? map : null,
                gunslinger.IsReady ? live.GunslingerSelection : null,
                live.PrerequisiteRaceType, live.PrerequisiteRaceField, live.GenericHitPoint, live.GenericSkillFull,
                live.GenericSkillPartial);
        }

        /// <summary>The planner's outcome, never committed: refused, or its surface count.</summary>
        private static string FcbPlanOutcome(FavoredClassBlueprintSet leaves, FavoredClassHostHandles host,
            FavoredClassProfileState profile, bool gunslingerModule)
        {
            try
            {
                return "planned:" + FavoredClassPublication.Plan(leaves, host, profile, gunslingerModule, null)
                    .Surfaces.Count;
            }
            catch (InvalidOperationException exception)
            {
                return "refused:" + exception.Message;
            }
        }

        private static FavoredClassHostObservation FcbCloneBinary(FavoredClassHostObservation live)
        {
            var clone = new FavoredClassHostObservation
            {
                HostModInstalled = live.HostModInstalled,
                HostModEnabled = live.HostModEnabled,
                HostModVersion = live.HostModVersion,
                HostAssemblyLoaded = live.HostAssemblyLoaded,
                HostAssemblyName = live.HostAssemblyName,
                HostModuleVersionId = live.HostModuleVersionId,
                HostFileSha256 = live.HostFileSha256,
                CallOfTheWildAssemblyLoaded = live.CallOfTheWildAssemblyLoaded,
                CallOfTheWildModuleVersionId = live.CallOfTheWildModuleVersionId,
                CallOfTheWildFileSha256 = live.CallOfTheWildFileSha256,
                MissingMember = live.MissingMember
            };
            foreach (KeyValuePair<string, string> value in live.MethodIlSha256)
                clone.MethodIlSha256[value.Key] = value.Value;
            return clone;
        }

        private static FavoredClassHostReadinessObservation FcbCloneReadiness(FavoredClassHostReadinessObservation live)
        {
            return new FavoredClassHostReadinessObservation
            {
                LibraryAssigned = live.LibraryAssigned,
                CoreLoadCompleted = live.CoreLoadCompleted,
                FavoredClassSelectionPresent = live.FavoredClassSelectionPresent,
                GunslingerClassGuid = live.GunslingerClassGuid,
                GunslingerProgressionGuid = live.GunslingerProgressionGuid,
                GunslingerBonusSelectionGuid = live.GunslingerBonusSelectionGuid,
                GunslingerProgressionOffered = live.GunslingerProgressionOffered,
                GunslingerProgressionLevels = live.GunslingerProgressionLevels,
                GunslingerLevelsGrantBonusSelection = live.GunslingerLevelsGrantBonusSelection,
                GenericHitPointLeafPresent = live.GenericHitPointLeafPresent,
                GenericSkillLeavesPresent = live.GenericSkillLeavesPresent
            };
        }

        /// <summary>A different value of the same length (the first hexadecimal digit changed).</summary>
        private static string FcbFlipHex(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "0";
            char first = value[0] == '0' ? '1' : '0';
            return first + value.Substring(1);
        }

        /// <summary>
        /// The host's global state a second Core.load() or a rebuild would
        /// change: its maps (keys and value identities), the favored class
        /// choice and its items, and Core.load's last assignment.
        /// </summary>
        private static string FcbHostGlobalState(FavoredClassHostHandles host)
        {
            Type core = host.PrerequisiteRaceType == null ? null :
                host.PrerequisiteRaceType.Assembly.GetType(FavoredClassHostContract.CoreTypeName, false, false);
            if (core == null)
                return "<core-unavailable>";
            const BindingFlags statics = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            Func<string, object> read = name =>
            {
                FieldInfo field = core.GetField(name, statics);
                return field == null ? null : field.GetValue(null);
            };
            Func<object, string> identity = value => value == null ? "null" :
                RuntimeHelpers.GetHashCode(value).ToString(System.Globalization.CultureInfo.InvariantCulture);
            Func<string, string> map = name =>
            {
                var dictionary = read(name) as IDictionary;
                if (dictionary == null)
                    return name + "=null";
                var entries = new List<string>();
                foreach (DictionaryEntry entry in dictionary)
                    entries.Add(entry.Key + ">" + identity(entry.Value));
                entries.Sort(StringComparer.Ordinal);
                return name + "=" + dictionary.Count + ":" + string.Join(",", entries.ToArray()).GetHashCode();
            };
            var choice = read(FavoredClassHostContract.FavoredClassSelectionField) as BlueprintFeatureSelection;
            return map(FavoredClassHostContract.ProgressionMapField) + ";" +
                map(FavoredClassHostContract.BonusSelectionMapField) + ";choice=" + identity(choice) + ":" +
                (choice == null ? "-" : string.Join(",", choice.AllFeatures.Select(value => value.AssetGuid).ToArray())
                    .GetHashCode().ToString(System.Globalization.CultureInfo.InvariantCulture)) +
                ";prestigious=" + identity(read(FavoredClassHostContract.PrestigiousSpellcasterField));
        }

        /// <summary>Every owned leaf's placement in the live selections.</summary>
        private static string FcbPublishedGraph(FavoredClassBlueprintSet leaves)
        {
            var owned = new HashSet<string>(leaves.Pairs.SelectMany(pair => pair.Leaves).Select(leaf => leaf.AssetGuid),
                StringComparer.Ordinal);
            var rows = new List<string>();
            foreach (BlueprintFeatureSelection selection in BlueprintBootstrap.Library.GetAllBlueprints()
                .OfType<BlueprintFeatureSelection>())
            {
                if (selection.AllFeatures == null)
                    continue;
                string[] placed = selection.AllFeatures.Where(value => value != null && owned.Contains(value.AssetGuid))
                    .Select(value => value.AssetGuid).ToArray();
                if (placed.Length != 0)
                    rows.Add(selection.AssetGuid + ":" + selection.AllFeatures.Length + ":" + string.Join(",", placed));
            }
            rows.Sort(StringComparer.Ordinal);
            return rows.Count + ":" + string.Join("|", rows.ToArray()).GetHashCode().ToString(
                System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
