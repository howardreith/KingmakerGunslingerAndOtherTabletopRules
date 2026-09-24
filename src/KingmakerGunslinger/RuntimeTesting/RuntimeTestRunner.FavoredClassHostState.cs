using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Root;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.FavoredClass;
using Newtonsoft.Json.Linq;
using UnityModManagerNet;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // H01/H02/H04 across compatibility profiles: the integration state
        // follows the actual host presence exactly; owned leaves always stay
        // registered for saved investments; without a ready host no owned
        // leaf reaches any selection; the core mod is unaffected.
        private RuntimeTestResult RunFavoredClassHostState()
        {
            var assertions = new List<RuntimeTestAssertion>();
            var evidence = new JObject();
            UnityModManager.ModEntry host = null;
            UnityModManager.ModEntry cotw = null;
            Type manager = _context.ModEntry.GetType().DeclaringType;
            FieldInfo field = manager == null ? null : manager.GetField("modEntries",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            IEnumerable values = field == null ? null : field.GetValue(null) as IEnumerable;
            UnityModManager.ModEntry[] entries = values == null ? new UnityModManager.ModEntry[0] :
                values.Cast<object>().OfType<UnityModManager.ModEntry>().ToArray();
            host = entries.SingleOrDefault(value => value.Info != null &&
                value.Info.Id == FavoredClassHostContract.HostModId);
            cotw = entries.SingleOrDefault(value => value.Info != null && value.Info.Id == "CallOfTheWild");
            evidence["ummEntries"] = new JArray(entries.Where(value => value.Info != null)
                .Select(value => value.Info.Id + (value.Enabled ? "" : "(disabled)")));
            evidence["hostEntry"] = host == null ? "absent" : host.Enabled ? "enabled" : "disabled";
            evidence["callOfTheWildEntry"] = cotw == null ? "absent" : cotw.Enabled ? "enabled" : "disabled";

            FavoredClassIntegrationStatus status = FavoredClassIntegrationStatusRegistry.Current;
            evidence["status"] = status.ToString();
            FavoredClassIntegrationAvailability expected = host == null
                ? FavoredClassIntegrationAvailability.HostAbsent
                : !host.Enabled ? FavoredClassIntegrationAvailability.HostDisabled
                : FavoredClassIntegrationAvailability.Published;
            assertions.Add(Assertion("fcb-host-state-matches-environment",
                "the integration availability is exactly HostAbsent without a Favored Class UMM entry, HostDisabled for a disabled entry, and Published for the enabled qualified host",
                "expected=" + expected + ";observed=" + status.Availability + ";detail=" + status.Detail,
                status.Availability == expected, "UMM modEntries and FavoredClassIntegrationStatusRegistry"));

            FavoredClassBlueprintSet leaves = BlueprintBootstrap.FavoredClassLeaves;
            var missing = new List<string>();
            int resolved = 0;
            if (leaves == null)
                missing.Add("<no registered favored-class set>");
            else
                foreach (FavoredClassIdentity identity in FavoredClassIdentityCatalog.All)
                {
                    BlueprintScriptableObject blueprint;
                    BlueprintBootstrap.Library.BlueprintsByAssetId.TryGetValue(identity.Guid, out blueprint);
                    if (blueprint is BlueprintFeature) resolved++;
                    else missing.Add(identity.Symbol);
                }
            evidence["registeredIdentities"] = resolved + "/" + FavoredClassIdentityCatalog.IdentityCount;
            assertions.Add(Assertion("fcb-owned-leaves-registered",
                "every committed favored-class identity resolves as a BlueprintFeature whatever the host state, so saved investments load",
                evidence["registeredIdentities"] + ";missing=" + string.Join(",", missing.ToArray()),
                missing.Count == 0, "LibraryScriptableObject.BlueprintsByAssetId"));

            HashSet<string> owned = new HashSet<string>(FavoredClassIdentityCatalog.All.Select(value => value.Guid),
                StringComparer.Ordinal);
            var publishedIn = new List<string>();
            foreach (BlueprintFeatureSelection selection in BlueprintBootstrap.Library.GetAllBlueprints()
                .OfType<BlueprintFeatureSelection>())
            {
                int count = (selection.AllFeatures ?? new BlueprintFeature[0]).Count(feature =>
                    feature != null && owned.Contains(feature.AssetGuid)) +
                    (selection.Features ?? new BlueprintFeature[0]).Count(feature =>
                        feature != null && owned.Contains(feature.AssetGuid));
                if (count > 0)
                    publishedIn.Add(selection.name + "=" + count);
            }
            evidence["selectionsContainingOwnedLeaves"] = new JArray(publishedIn);
            bool publicationConsistent = expected == FavoredClassIntegrationAvailability.Published
                ? publishedIn.Count == 1 && publishedIn[0].StartsWith("FavoredClassKMG_Gunslinger_", StringComparison.Ordinal)
                : publishedIn.Count == 0;
            assertions.Add(Assertion("fcb-publication-follows-host",
                "without a ready host no owned leaf appears in any selection; with the host only the host's Gunslinger bonus selection holds them",
                string.Join(",", publishedIn.ToArray()), publicationConsistent,
                "every BlueprintFeatureSelection AllFeatures/Features in the library"));

            GunslingerClassBlueprintSetProbe(evidence, assertions);
            assertions.Add(Assertion("loaded-mod-version", _request.ExpectedModVersion,
                _context.ModEntry.Info.Version,
                _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                "Unity Mod Manager ModEntry.Info.Version"));
            string evidencePath = WriteFavoredClassEvidence("favored-class-host-state.json", evidence);
            RuntimeTestResult result = CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions, null);
            result.EvidenceFiles.Add(evidencePath);
            return result;
        }

        private void GunslingerClassBlueprintSetProbe(JObject evidence, List<RuntimeTestAssertion> assertions)
        {
            BlueprintCharacterClass gunslinger = BlueprintBootstrap.GunslingerClass == null ? null :
                BlueprintBootstrap.GunslingerClass.CharacterClass;
            bool offered = gunslinger != null &&
                BlueprintRoot.Instance.Progression.CharacterClasses.Contains(gunslinger);
            evidence["gunslingerClassOffered"] = offered;
            evidence["bootstrapInitialized"] = BlueprintBootstrap.IsInitialized;
            assertions.Add(Assertion("fcb-core-unaffected",
                "the KMG bootstrap completed and the Gunslinger class is offered whatever the host state",
                "initialized=" + BlueprintBootstrap.IsInitialized + ";classOffered=" + offered,
                BlueprintBootstrap.IsInitialized && offered, "BlueprintBootstrap; BlueprintRoot progression"));
        }
    }
}
