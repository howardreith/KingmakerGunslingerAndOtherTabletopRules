using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Root;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.FavoredClass;
using Newtonsoft.Json.Linq;
using UnityModManagerNet;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // H01/H02/H04 and L04 across compatibility and settings profiles:
        // the integration state follows the actual host presence and the
        // restart-required integration control exactly; owned identities
        // always stay registered for saved investments; without a ready,
        // enabled host no owned leaf reaches any selection; owned mechanics
        // apply only while the integration is enabled; the core mod is
        // unaffected.
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
            FavoredClassProfileState profile = FavoredClassRuntime.Profile;
            evidence["profile"] = profile.ToString();
            evidence["settings"] = FavoredClassIntegrationCoordinator.Settings == null ? null :
                FavoredClassIntegrationCoordinator.Settings.ToString();
            FavoredClassIntegrationAvailability expected = !profile.IntegrationEnabled
                ? FavoredClassIntegrationAvailability.IntegrationDisabled
                : host == null ? FavoredClassIntegrationAvailability.HostAbsent
                : !host.Enabled ? FavoredClassIntegrationAvailability.HostDisabled
                : FavoredClassIntegrationAvailability.Published;
            assertions.Add(Assertion("fcb-host-state-matches-environment",
                "the integration availability is exactly IntegrationDisabled when the settings disable it, otherwise HostAbsent without a Favored Class UMM entry, HostDisabled for a disabled entry, and Published for the enabled qualified host",
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
                    if (blueprint != null && blueprint.GetType().Name == identity.PlannedType) resolved++;
                    else missing.Add(identity.Symbol);
                }
            evidence["registeredIdentities"] = resolved + "/" + FavoredClassIdentityCatalog.IdentityCount;
            assertions.Add(Assertion("fcb-owned-leaves-registered",
                "every committed favored-class identity resolves with its planned type whatever the host or settings state, so saved investments load",
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
            FavoredClassHostHandles handles = FavoredClassIntegrationCoordinator.Host;
            HashSet<string> hostSelections = new HashSet<string>(handles == null ? new string[0] :
                handles.BonusSelections.Where(value => value.Value != null).Select(value => value.Value.name),
                StringComparer.Ordinal);
            bool publicationConsistent = expected == FavoredClassIntegrationAvailability.Published
                ? publishedIn.Count > 0 && publishedIn.Any(value =>
                        value.StartsWith("FavoredClassKMG_Gunslinger_", StringComparison.Ordinal)) &&
                    publishedIn.All(value => hostSelections.Contains(value.Substring(0, value.LastIndexOf('='))))
                : publishedIn.Count == 0;
            assertions.Add(Assertion("fcb-publication-follows-host",
                "without a ready, enabled host no owned leaf appears in any selection; with it, owned leaves appear only in the host's per-class bonus selections, including the Gunslinger's",
                string.Join(",", publishedIn.ToArray()), publicationConsistent,
                "every BlueprintFeatureSelection AllFeatures/Features in the library"));

            MechanicsFollowIntegrationProbe(evidence, assertions, leaves, profile);
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

        // L04: an already-earned choice keeps its identity and rank; its
        // owned numerical effect applies while the integration is enabled
        // and is suppressed (not refunded) while it is disabled.
        private void MechanicsFollowIntegrationProbe(JObject evidence, List<RuntimeTestAssertion> assertions,
            FavoredClassBlueprintSet leaves, FavoredClassProfileState profile)
        {
            var row = new JObject();
            bool pass = false;
            var units = new List<Kingmaker.EntitySystem.Entities.UnitEntityData>();
            try
            {
                var gunslinger = BlueprintBootstrap.GunslingerClass;
                FavoredClassLeafPair grit = leaves == null ? null : leaves.Pair(FavoredClassCatalog.EffectGrit, null);
                if (gunslinger != null && grit != null)
                {
                    Func<Kingmaker.EntitySystem.Entities.UnitEntityData> create = () =>
                    {
                        var unit = new Kingmaker.UI.LevelUp.ChargenUnit(
                            BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
                        units.Add(unit);
                        unit.Descriptor.Stats.Wisdom.BaseValue = 14;
                        unit.Descriptor.AddFact(gunslinger.Grit.Feature);
                        return unit;
                    };
                    var control = create();
                    var invested = create();
                    int rank = GrantFavoredClassRanks(invested, grit.Full, 2);
                    int delta = gunslinger.Grit.Resource.GetMaxAmount(invested.Descriptor) -
                        gunslinger.Grit.Resource.GetMaxAmount(control.Descriptor);
                    int expectedDelta = profile.IntegrationEnabled ? 2 : 0;
                    row["mechanicsEnabled"] = FavoredClassRuntime.MechanicsEnabled;
                    row["savedRank"] = rank;
                    row["gritMaximumDelta"] = delta;
                    row["expectedDelta"] = expectedDelta;
                    pass = rank == 2 && delta == expectedDelta &&
                        FavoredClassRuntime.MechanicsEnabled == profile.IntegrationEnabled;
                }
                else
                    row["unavailable"] = "gunslinger or grit counter not registered";
            }
            catch (Exception exception)
            {
                row["exception"] = exception.GetType().Name + ": " + exception.Message;
            }
            finally
            {
                foreach (var unit in units)
                    try { unit.Dispose(); } catch (Exception) { }
            }
            evidence["mechanicsFollowIntegration"] = row;
            assertions.Add(Assertion("fcb-mechanics-follow-integration",
                "an earned counter keeps its rank; its owned effect (two grit steps) applies while the integration is enabled and is suppressed, not refunded, while it is disabled",
                row.ToString(Newtonsoft.Json.Formatting.None), pass,
                "ChargenUnit with the native grit feature; BlueprintAbilityResource.GetMaxAmount"));
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
