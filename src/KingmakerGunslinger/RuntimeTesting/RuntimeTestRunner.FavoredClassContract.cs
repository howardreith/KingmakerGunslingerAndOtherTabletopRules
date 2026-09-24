using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Root;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Compatibility;
using KingmakerGunslinger.FavoredClass;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Main-menu, save-free observation of the live Favored Class contract
        // and of the committed publication. Publication is exercised on the
        // live host graph only at the main menu, where no build session can
        // hold the menus: rollback, a fault-injected transaction and a fresh
        // re-publication must each leave the exact expected foreign arrays.
        private RuntimeTestResult RunFavoredClassContract()
        {
            var assertions = new List<RuntimeTestAssertion>();
            var evidence = new JObject();
            FavoredClassIntegrationStatus status = FavoredClassIntegrationStatusRegistry.Current;
            FavoredClassHostHandles host = FavoredClassIntegrationCoordinator.Host;
            FavoredClassBlueprintSet leaves = BlueprintBootstrap.FavoredClassLeaves;
            evidence["status"] = status.ToString();
            evidence["ummOrder"] = ClassCatalogDiagnostics.DescribeUmmOrder(_context.ModEntry);
            evidence["loadDictionaryPatches"] =
                ClassCatalogDiagnostics.DescribeLoadDictionaryPatches(_context);

            // Binary identity (H05) and readiness (H02/H03).
            var binaryFailures = new List<string>();
            JObject binary = DescribeFcbBinary(host, binaryFailures);
            evidence["binary"] = binary;
            assertions.Add(Assertion("fcb-host-binary-identity",
                "the loaded Favored Class and Call of the Wild assemblies are exactly the qualified files (SHA-256, MVID, member shapes and method fingerprints)",
                Describe(binary, binaryFailures), binaryFailures.Count == 0,
                "reflection over the live UMM entries and loaded assemblies"));
            var readinessFailures = new List<string>();
            JObject readiness = DescribeFcbReadiness(host, readinessFailures);
            evidence["readiness"] = readiness;
            assertions.Add(Assertion("fcb-host-readiness-and-gunslinger-scan",
                "Core.load completed (completion marker), the favored class selection exists, and the host scanned the KMG Gunslinger class into exactly one favored progression and bonus selection with the MergeIds identities, twenty levels and the generic rewards",
                Describe(readiness, readinessFailures), readinessFailures.Count == 0,
                "host private static maps read after Main.library was observed"));
            if (host == null || leaves == null || host.GunslingerSelection == null ||
                status.Availability != FavoredClassIntegrationAvailability.Published)
            {
                assertions.Add(Assertion("fcb-integration-published", "Published",
                    status.ToString(), false, "FavoredClassIntegrationStatusRegistry"));
                return FinishContract(assertions, evidence);
            }

            BlueprintFeatureSelection gunslinger = host.GunslingerSelection;
            BlueprintFeature[] owned = leaves.Pairs.SelectMany(pair => pair.Leaves).ToArray();
            BlueprintFeature[] published = gunslinger.AllFeatures;
            BlueprintFeature[] foreign = published.Where(value =>
                !owned.Contains(value)).ToArray();
            var graphFailures = new List<string>();
            JObject graph = DescribeFcbPublishedGraph(gunslinger, owned, host, graphFailures);
            evidence["graph"] = graph;
            assertions.Add(Assertion("fcb-publication-graph",
                "every owned grit leaf appears exactly once after the host's untouched generic leaves, with identical full/partial ancestry and investment prerequisites, hidden when unavailable",
                Describe(graph, graphFailures), graphFailures.Count == 0,
                "live host Gunslinger bonus selection AllFeatures and leaf components"));

            // H06: repeated publication is idempotent.
            var idempotentFailures = new List<string>();
            FavoredClassIntegrationCoordinator.TryResolveAndPublish("qualification-repeat");
            if (!ReferenceEquals(published, gunslinger.AllFeatures))
                idempotentFailures.Add("repeat publication replaced the array");
            FavoredClassPublication repeat = FavoredClassPublication.Plan(leaves, host,
                FavoredClassRuntime.Profile, _context.FeatureModules.Active.Gunslinger, null);
            repeat.Commit();
            if (!ReferenceEquals(published, gunslinger.AllFeatures) ||
                !repeat.Evidence.Where(value => value.Contains("action=")).All(value =>
                    value.Contains("action=unchanged")))
                idempotentFailures.Add("second transaction was not a no-op: " +
                    string.Join("|", repeat.Evidence.ToArray()));
            assertions.Add(Assertion("fcb-publication-idempotent",
                "repeating readiness and publication leaves the same array reference, identities, order and counts",
                string.Join("|", repeat.Evidence.ToArray()) + ";failures=" +
                    string.Join(",", idempotentFailures.ToArray()),
                idempotentFailures.Count == 0, "coordinator repeat + second FavoredClassPublication"));

            // H08: rollback, fault injection and re-publication on the live graph.
            var faultFailures = new List<string>();
            JObject fault = new JObject();
            BlueprintComponent[][] hostComponentsBefore = SnapshotHostComponents(host);
            try
            {
                FavoredClassIntegrationCoordinator.Publication.Rollback();
                fault["afterRollback"] = gunslinger.AllFeatures.Length;
                if (!SameFeatureReferences(gunslinger.AllFeatures, foreign))
                    faultFailures.Add("rollback did not restore the exact foreign array");
                FavoredClassPublication faulty = FavoredClassPublication.Plan(leaves, host,
                    FavoredClassRuntime.Profile, _context.FeatureModules.Active.Gunslinger, 1);
                try
                {
                    faulty.Commit();
                    faultFailures.Add("the injected fault did not stop publication");
                }
                catch (InvalidOperationException injected)
                {
                    fault["injected"] = injected.Message;
                }
                fault["afterFault"] = gunslinger.AllFeatures.Length;
                if (!SameFeatureReferences(gunslinger.AllFeatures, foreign))
                    faultFailures.Add("the failed transaction left owned entries or changed foreign ones");
                FavoredClassPublication fresh = FavoredClassPublication.Plan(leaves, host,
                    FavoredClassRuntime.Profile, _context.FeatureModules.Active.Gunslinger, null);
                fresh.Commit();
                FavoredClassIntegrationCoordinator.AdoptQualificationRepublication(fresh);
                fault["afterRepublish"] = gunslinger.AllFeatures.Length;
                if (!SameFeatureReferences(gunslinger.AllFeatures.Take(foreign.Length).ToArray(), foreign) ||
                    gunslinger.AllFeatures.Length != foreign.Length + owned.Length)
                    faultFailures.Add("re-publication did not restore the exact graph");
            }
            catch (Exception exception)
            {
                faultFailures.Add("exception=" + exception);
            }
            if (!SameComponentSnapshots(hostComponentsBefore, SnapshotHostComponents(host)))
                faultFailures.Add("host generic leaf prerequisites/components changed");
            evidence["fault"] = fault;
            assertions.Add(Assertion("fcb-publication-fault-rollback",
                "on the live host graph, rollback restores the exact foreign array, a transaction failing midway leaves no owned entry and no changed foreign entry, and a fresh transaction re-publishes the identical graph; host leaf components are untouched",
                Describe(fault, faultFailures), faultFailures.Count == 0,
                "production FavoredClassPublication with an injected surface fault"));

            // H10: host generic rewards unchanged.
            var genericFailures = new List<string>();
            JObject generic = DescribeFcbGenericRewards(host, genericFailures);
            evidence["generic"] = generic;
            assertions.Add(Assertion("fcb-host-generic-rewards-unchanged",
                "the host's hit-point (20 ranks, AddHitPointOnce) and half-rate skill rewards are unchanged and still offered to the Gunslinger and Fighter",
                Describe(generic, genericFailures), genericFailures.Count == 0,
                "live host generic features and Harmony patch owners"));

            // Inventories for later phases (recorded, scored for presence only).
            var inventoryFailures = new List<string>();
            JObject inventory = DescribeFcbInventory(host, inventoryFailures);
            evidence["inventory"] = inventory;
            assertions.Add(Assertion("fcb-host-human-and-race-inventory",
                "the live human-route leaves of the host's twenty reviewed class families and every source-addressable race identity are recorded; native and KMG races resolve",
                Describe(inventory, inventoryFailures), inventoryFailures.Count == 0,
                "host bonus selections and their PrerequisiteRace instances; BlueprintRoot race catalog"));
            return FinishContract(assertions, evidence);
        }

        private RuntimeTestResult FinishContract(List<RuntimeTestAssertion> assertions, JObject evidence)
        {
            assertions.Add(Assertion("loaded-mod-version", _request.ExpectedModVersion,
                _context.ModEntry.Info.Version,
                _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                "Unity Mod Manager ModEntry.Info.Version"));
            string path = WriteFavoredClassEvidence("favored-class-contract.json", evidence);
            RuntimeTestResult result = CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions, null);
            result.EvidenceFiles.Add(path);
            return result;
        }

        private static JObject DescribeFcbBinary(FavoredClassHostHandles host, IList<string> failures)
        {
            if (host == null)
            {
                failures.Add("host handles unresolved");
                return new JObject();
            }
            FavoredClassHostObservation observed = host.Binary;
            var row = new JObject
            {
                ["decision"] = host.Decision.ToString(),
                ["hostVersion"] = observed.HostModVersion,
                ["hostSha256"] = observed.HostFileSha256,
                ["hostMvid"] = observed.HostModuleVersionId,
                ["cotwSha256"] = observed.CallOfTheWildFileSha256,
                ["cotwMvid"] = observed.CallOfTheWildModuleVersionId,
                ["fingerprints"] = JObject.FromObject(observed.MethodIlSha256)
            };
            if (!host.Decision.IsReady)
                failures.Add("host decision " + host.Decision);
            if (observed.HostFileSha256 != FavoredClassHostContract.VerifiedHostFileSha256 ||
                observed.HostModuleVersionId != FavoredClassHostContract.VerifiedHostModuleVersionId)
                failures.Add("host identity differs");
            if (observed.CallOfTheWildFileSha256 != FavoredClassHostContract.VerifiedCallOfTheWildFileSha256 ||
                observed.CallOfTheWildModuleVersionId != FavoredClassHostContract.VerifiedCallOfTheWildModuleVersionId)
                failures.Add("Call of the Wild identity differs");
            foreach (string key in FavoredClassHostContract.FingerprintKeys)
            {
                string value;
                if (!observed.MethodIlSha256.TryGetValue(key, out value) ||
                    value != FavoredClassHostContract.VerifiedIlSha256(key))
                    failures.Add("fingerprint " + key + "=" + value);
            }
            return row;
        }

        private static JObject DescribeFcbReadiness(FavoredClassHostHandles host, IList<string> failures)
        {
            if (host == null || host.Readiness == null)
            {
                failures.Add("readiness unobserved");
                return new JObject();
            }
            FavoredClassHostReadinessObservation value = host.Readiness;
            BlueprintCharacterClass gunslinger = BlueprintBootstrap.GunslingerClass.CharacterClass;
            BlueprintCharacterClass[] classes = BlueprintRoot.Instance.Progression.CharacterClasses;
            var row = new JObject
            {
                ["libraryAssigned"] = value.LibraryAssigned,
                ["coreLoadCompleted"] = value.CoreLoadCompleted,
                ["favoredClassSelectionPresent"] = value.FavoredClassSelectionPresent,
                ["gunslingerClass"] = value.GunslingerClassGuid,
                ["gunslingerProgression"] = value.GunslingerProgressionGuid,
                ["gunslingerBonusSelection"] = value.GunslingerBonusSelectionGuid,
                ["progressionOffered"] = value.GunslingerProgressionOffered,
                ["progressionLevels"] = value.GunslingerProgressionLevels,
                ["levelsGrantSelection"] = value.GunslingerLevelsGrantBonusSelection,
                ["genericHitPoint"] = value.GenericHitPointLeafPresent,
                ["genericSkill"] = value.GenericSkillLeavesPresent,
                ["gunslingerDecision"] = host.GunslingerDecision.ToString(),
                ["gunslingerCatalogIndex"] = Array.IndexOf(classes, gunslinger),
                ["catalogCount"] = classes.Length
            };
            if (!host.GunslingerDecision.IsReady)
                failures.Add("gunslinger " + host.GunslingerDecision);
            if (value.GunslingerProgressionGuid !=
                    FavoredClassHostContract.ExpectedProgressionGuid(gunslinger.AssetGuid) ||
                value.GunslingerBonusSelectionGuid !=
                    FavoredClassHostContract.ExpectedBonusSelectionGuid(gunslinger.AssetGuid))
                failures.Add("gunslinger host identities differ from MergeIds");
            if (Array.IndexOf(classes, gunslinger) < 0)
                failures.Add("the Gunslinger class is not in the live class catalog");
            return row;
        }

        private static JObject DescribeFcbPublishedGraph(BlueprintFeatureSelection selection,
            BlueprintFeature[] owned, FavoredClassHostHandles host, IList<string> failures)
        {
            BlueprintFeature[] all = selection.AllFeatures;
            var row = new JObject
            {
                ["selection"] = selection.name + ":" + selection.AssetGuid,
                ["entries"] = new JArray(all.Select(value => value.name + ":" + value.AssetGuid)),
                ["featuresEmpty"] = selection.Features == null || selection.Features.Length == 0
            };
            foreach (BlueprintFeature leaf in owned)
            {
                int count = all.Count(value => ReferenceEquals(value, leaf));
                int ids = all.Count(value => value.AssetGuid == leaf.AssetGuid);
                if (count != 1 || ids != 1)
                    failures.Add(leaf.name + " appears " + count + "/" + ids);
                if (!leaf.HideNotAvailibleInUI)
                    failures.Add(leaf.name + " is shown when unavailable");
            }
            int firstOwned = Array.FindIndex(all, value => owned.Contains(value));
            if (firstOwned < 0 || all.Skip(firstOwned).Any(value => !owned.Contains(value)))
                failures.Add("owned leaves are not a contiguous suffix after the foreign entries");
            BlueprintFeature[] expectedHost = { host.GenericHitPoint, host.GenericSkillPartial,
                host.GenericSkillFull };
            if (firstOwned < 0 || !all.Take(firstOwned).SequenceEqual(expectedHost))
                failures.Add("foreign prefix is not exactly the host generic rewards in host order");
            foreach (FavoredClassLeafPair pair in BlueprintBootstrap.FavoredClassLeaves.Pairs)
            {
                if (pair.Partial == null)
                    continue;
                PrerequisiteFavoredClassAncestry fullAncestry = pair.Full.ComponentsArray
                    .OfType<PrerequisiteFavoredClassAncestry>().Single();
                PrerequisiteFavoredClassAncestry partialAncestry = pair.Partial.ComponentsArray
                    .OfType<PrerequisiteFavoredClassAncestry>().Single();
                if (fullAncestry.EffectId != partialAncestry.EffectId ||
                    fullAncestry.Group != partialAncestry.Group)
                    failures.Add(pair.Effect.Id + " full and partial ancestry restrictions differ");
            }
            return row;
        }

        private static JObject DescribeFcbGenericRewards(FavoredClassHostHandles host, IList<string> failures)
        {
            var row = new JObject
            {
                ["hitPoint"] = host.GenericHitPoint == null ? "<absent>" :
                    host.GenericHitPoint.name + " ranks=" + host.GenericHitPoint.Ranks,
                ["skillFull"] = host.GenericSkillFull == null ? "<absent>" :
                    host.GenericSkillFull.name + " ranks=" + host.GenericSkillFull.Ranks,
                ["skillPartial"] = host.GenericSkillPartial == null ? "<absent>" :
                    host.GenericSkillPartial.name + " ranks=" + host.GenericSkillPartial.Ranks
            };
            if (host.GenericHitPoint == null || host.GenericHitPoint.Ranks != 20 ||
                !host.GenericHitPoint.ComponentsArray.Any(component =>
                    component.GetType().FullName == "ZFavoredClass.NewMechanics.AddHitPointOnce"))
                failures.Add("host hit-point reward changed");
            if (host.GenericSkillFull == null || host.GenericSkillFull.Ranks != 10)
                failures.Add("host skill reward changed");
            BlueprintCharacterClass fighter = BlueprintBootstrap.Library.GetAllBlueprints()
                .OfType<BlueprintCharacterClass>().SingleOrDefault(value => value.name == "FighterClass");
            BlueprintFeatureSelection fighterSelection = fighter == null ? null :
                host.BonusSelectionFor(fighter.AssetGuid);
            if (fighterSelection == null || !fighterSelection.AllFeatures.Contains(host.GenericHitPoint))
                failures.Add("the Fighter no longer offers the host hit-point reward");
            // Diagnostic only: methods with hit points in their name that any
            // KMG Harmony owner patches (the integration itself installs none;
            // a domain source check pins that).
            row["kmgPatchedHitPointMethods"] = new JArray(Harmony12.HarmonyInstance
                .Create("KingmakerGunslinger.favored-class.probe").GetPatchedMethods()
                .Where(method => method.Name.IndexOf("HitPoint", StringComparison.OrdinalIgnoreCase) >= 0)
                .Select(method => method.DeclaringType.FullName + "." + method.Name));
            return row;
        }

        private static JObject DescribeFcbInventory(FavoredClassHostHandles host, IList<string> failures)
        {
            var human = new JArray();
            foreach (KeyValuePair<string, BlueprintFeatureSelection> entry in host.BonusSelections)
            {
                if (entry.Value == null)
                    continue;
                BlueprintScriptableObject classBlueprint;
                BlueprintBootstrap.Library.BlueprintsByAssetId.TryGetValue(entry.Key, out classBlueprint);
                foreach (BlueprintFeature leaf in entry.Value.AllFeatures)
                {
                    var races = leaf.ComponentsArray.Where(component =>
                            component.GetType().FullName == FavoredClassHostContract.PrerequisiteRaceTypeName)
                        .Select(component => host.PrerequisiteRaceField.GetValue(component) as BlueprintRace)
                        .Where(race => race != null).ToArray();
                    if (races.Any(race => race.AssetGuid == FcbHumanRace))
                        human.Add((classBlueprint == null ? entry.Key : classBlueprint.name) + ":" +
                            leaf.name + ":" + leaf.AssetGuid + ":races=" + races.Length);
                }
            }
            var raceRows = new JArray();
            BlueprintRace[] playable = BlueprintRoot.Instance.Progression.CharacterRaces;
            foreach (FavoredClassRaceIdentity identity in FavoredClassRaceIdentities.All)
            {
                BlueprintScriptableObject race;
                BlueprintBootstrap.Library.BlueprintsByAssetId.TryGetValue(identity.RaceGuid, out race);
                raceRows.Add(identity.Ancestry + ":" + (race == null ? "absent" : race.name) +
                    ":playable=" + playable.Contains(race as BlueprintRace) + ":provider=" + identity.Provider);
                if (race == null && identity.Provider != FavoredClassRaceProvider.Optional)
                    failures.Add(identity.Ancestry + " race missing");
            }
            if (human.Count == 0)
                failures.Add("no human-route host leaves observed");
            return new JObject { ["humanLeaves"] = human, ["humanLeafCount"] = human.Count,
                ["races"] = raceRows };
        }

        // Component arrays (and each component reference) of the host's generic
        // leaves; publication must never touch another mod's prerequisites.
        private static BlueprintComponent[][] SnapshotHostComponents(FavoredClassHostHandles host)
        {
            return new[] { host.GenericHitPoint, host.GenericSkillFull, host.GenericSkillPartial }
                .Select(feature => feature == null || feature.ComponentsArray == null
                    ? new BlueprintComponent[0] : feature.ComponentsArray.ToArray()).ToArray();
        }

        private static bool SameComponentSnapshots(BlueprintComponent[][] before,
            BlueprintComponent[][] after)
        {
            if (before.Length != after.Length)
                return false;
            for (int index = 0; index < before.Length; index++)
            {
                if (before[index].Length != after[index].Length)
                    return false;
                for (int item = 0; item < before[index].Length; item++)
                    if (!ReferenceEquals(before[index][item], after[index][item]))
                        return false;
            }
            return true;
        }

        private static bool SameFeatureReferences(BlueprintFeature[] left, BlueprintFeature[] right)
        {
            if (left == null || right == null || left.Length != right.Length)
                return false;
            for (int index = 0; index < left.Length; index++)
                if (!ReferenceEquals(left[index], right[index]))
                    return false;
            return true;
        }
    }
}
