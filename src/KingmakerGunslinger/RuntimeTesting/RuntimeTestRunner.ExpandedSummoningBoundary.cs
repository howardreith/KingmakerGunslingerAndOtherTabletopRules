using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Measures where the Expanded Summoning feature module's boundary actually
    /// falls, by reading the eighteen native summon parents the publisher writes
    /// to.
    ///
    /// The feature-module gate this feeds used to assert only that the active
    /// flag equalled the requested one. That is a tautology: both sides come
    /// from the settings object the launcher had just written, so the assertion
    /// would have passed unchanged if the module had published its entire
    /// roster with the module turned off, or published nothing with it turned
    /// on. Nothing else in the suite covered the disabled direction either -
    /// every other summoning scenario requires the module enabled, because they
    /// are about what it publishes.
    ///
    /// The reading is then judged choice by choice, by identity, in
    /// <see cref="ExpandedSummoningPublicationAudit"/>. It used to sort choices
    /// by the prefix of their internal name and keep only totals, so a choice
    /// it had no prefix for was counted as unclassified with nothing kept of
    /// which choice it was or where: the generated Spider Summoner rows, added
    /// after this reader was written, were counted that way whenever Races
    /// Unleashed was installed. What is expected comes from the catalogs the
    /// publisher reads and from the identity manifest, never from a local copy
    /// and never from what was read.
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        private RuntimeTestResult RunExpandedSummoningModuleBoundaryObservation()
        {
            bool expected = (bool)_request.Parameters["expandedSummoning"];
            bool active = _context.FeatureModules.Active.ExpandedSummoning;
            ExpandedSummoningBoundary observed =
                ObserveExpandedSummoningBoundary(expected);
            string expectedReading = (expected ? "enabled" : "disabled") +
                ";" + ExpandedSummoningBoundaryExpectation(expected,
                    observed.NativeVariants);
            string actualReading = (active ? "enabled" : "disabled") +
                ";" + observed.Describe();
            var evidenceFiles = new List<string>();
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("expanded-summoning-module-boundary",
                    expectedReading, actualReading,
                    expectedReading == actualReading,
                    "live AbilityVariants census across the eighteen canonical native summon parents")
            };
            assertions.AddRange(ExpandedSummoningBoundaryDetail(
                "expanded-summoning-module-boundary", observed, evidenceFiles));
            assertions.Add(Assertion("loaded-mod-version", _request.ExpectedModVersion,
                _context.ModEntry.Info.Version,
                _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                "Unity Mod Manager ModEntry.Info.Version"));
            if (!expected && !active) assertions.AddRange(ObserveSummonCleanup2ModuleOff());
            RuntimeTestResult result = CreateResult(assertions.All(value => value.Status == "PASS") ?
                "PASS" : "FAIL", assertions, null);
            result.EvidenceFiles = evidenceFiles;
            return result;
        }

        /// <summary>One reading of the eighteen parents, judged.</summary>
        private sealed class ExpandedSummoningBoundary
        {
            internal ExpandedSummoningPublicationAudit Audit;
            /// <summary>Why no audit could be made; null when one was.</summary>
            internal string Fault;

            internal int NativeVariants
            { get { return Audit == null ? 0 : Audit.NativeVariants; } }

            /// <summary>The totals. An observation that could not be made
            /// never reads the same as an expectation.</summary>
            internal string Describe()
            {
                return Audit == null ? "observation unavailable: " + Fault :
                    Audit.Describe();
            }
        }

        // The manifest and the catalogs do not change while the process
        // runs, so the contract is built once.
        private static ExpandedSummoningPublicationContract _summonPublicationContract;
        private static string _summonPublicationContractFault;

        private ExpandedSummoningPublicationContract SummonPublicationContract(
            out string fault)
        {
            if (_summonPublicationContract == null &&
                _summonPublicationContractFault == null)
            {
                try
                {
                    string manifest = File.ReadAllText(Path.Combine(
                        _context.ModEntry.Path,
                        BlueprintManifest.RelativeManifestPath));
                    _summonPublicationContract =
                        ExpandedSummoningPublicationContract.Build(
                            ExpandedSummoningPublisher.CanonicalParentGuids.ToList(),
                            SummonManifestIdentities.Parse(manifest));
                }
                catch (Exception exception)
                {
                    _summonPublicationContractFault = exception.GetType().Name +
                        ": " + exception.Message;
                }
            }

            fault = _summonPublicationContractFault;
            return _summonPublicationContract;
        }

        /// <summary>
        /// The state a reading is judged in. Whether Races Unleashed supplies
        /// the Spider Summoner rows is read from the library's own
        /// identities, not from the summon parents and not from what the
        /// integration says it did.
        /// </summary>
        private SummonPublicationState SummonPublicationStateFor(bool requested)
        {
            var state = new SummonPublicationState
            {
                Requested = requested,
                Active = _context.FeatureModules.Active.ExpandedSummoning,
                SpiderIntegrationOutcome = SpiderSummonerIntegration.Outcome
            };
            Dictionary<string, BlueprintScriptableObject> loaded =
                BlueprintBootstrap.Library.BlueprintsByAssetId;
            BlueprintScriptableObject found;
            state.SpiderFeaturePresent = loaded.TryGetValue(
                SummonForeignRowClassifier.SpiderSummonerFeatureGuid, out found) &&
                found is BlueprintFeature;
            foreach (SummonFamily family in SpiderSummonerCatalog.Families)
            foreach (SpiderSpeciesSpec species in SpiderSummonerCatalog.All)
            {
                if (loaded.TryGetValue(SpiderSummonerCatalog.RowIdentity(family,
                    species), out found) && found is BlueprintAbility)
                    state.SpiderProviderRows.Add(SummonPublicationState.RowKey(
                        family, species.Key));
            }
            return state;
        }

        /// <summary>
        /// What the eighteen parents must look like for a given module state.
        ///
        /// Every number is derived from the frozen catalogs the publisher reads,
        /// never written down here, so a deliberate roster change moves the
        /// expectation and the observation together and this check stays honest
        /// about the surface rather than about a remembered total.
        ///
        /// <paramref name="nativeVariants"/> is reported but not constrained:
        /// the count of choices that are not this project's is a property of
        /// the base game and the other mods installed, which no single run can
        /// establish. It is carried in the string so the enabled and disabled
        /// runs of the compatibility matrix can be compared against each other,
        /// which is where that fact actually lives.
        /// </summary>
        private string ExpandedSummoningBoundaryExpectation(
            bool published, int nativeVariants)
        {
            try
            {
                string fault;
                ExpandedSummoningPublicationContract contract =
                    SummonPublicationContract(out fault);
                return contract == null ? "expectation unavailable: " + fault :
                    ExpandedSummoningPublicationAudit.Expectation(contract,
                        SummonPublicationStateFor(published), nativeVariants);
            }
            catch (Exception exception)
            {
                return "expectation unavailable: " + exception.GetType().Name +
                    ": " + exception.Message;
            }
        }

        private ExpandedSummoningBoundary ObserveExpandedSummoningBoundary(
            bool expectedPublished)
        {
            var result = new ExpandedSummoningBoundary();
            try
            {
                string fault;
                ExpandedSummoningPublicationContract contract =
                    SummonPublicationContract(out fault);
                if (contract == null)
                {
                    result.Fault = "publication contract unavailable: " + fault;
                    return result;
                }

                // The publisher's own census of its placements by name is
                // kept beside the census by identity. It stops at a parent
                // it cannot read; the reading below reports which.
                bool? nameCensusExact;
                try
                {
                    int placementReferences;
                    nameCensusExact = ExpandedSummoningPublisher
                        .RequiredBasePublicationIsExact(BlueprintBootstrap.Library,
                            expectedPublished, out placementReferences);
                }
                catch (Exception)
                {
                    nameCensusExact = false;
                }

                result.Audit = ExpandedSummoningPublicationAudit.Evaluate(contract,
                    SummonPublicationStateFor(expectedPublished),
                    contract.Parents.Select(ReadSummonParent).ToArray(),
                    nameCensusExact);
            }
            catch (Exception exception)
            {
                result.Audit = null;
                result.Fault = exception.GetType().Name + ": " + exception.Message;
            }

            return result;
        }

        private static SummonParentReading ReadSummonParent(SummonParentKey key)
        {
            BlueprintAbility parent = null;
            Func<BlueprintAbility> resolve = () => parent ?? (parent =
                BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                    BlueprintBootstrap.Library, key.Guid,
                    "canonical summon parent boundary probe"));
            // Summon Monster I and Nature's Ally I are direct summons with no
            // variant list at all until this module publishes one, so an
            // absent component is the correct disabled reading rather than a
            // fault.
            return SummonParentReading.Read<BlueprintAbility>(key,
                () => resolve().name,
                () => ExpandedSummoningPublisher.IsPublishedExpandedParent(resolve()),
                () => (resolve().ComponentsArray ?? Array.Empty<BlueprintComponent>())
                    .OfType<AbilityVariants>()
                    .Select(value => (IList<BlueprintAbility>)(value.Variants ??
                        Array.Empty<BlueprintAbility>())).ToList(),
                value => value.AssetGuid, value => value.name);
        }

        /// <summary>
        /// The individual results that stand beside the totals: required
        /// choices, the declared boundary, duplicates and readability, each
        /// its own assertion, and the ledger of every choice written beside
        /// the result. A reading that could not be judged is one failing
        /// assertion that says why.
        /// </summary>
        private List<RuntimeTestAssertion> ExpandedSummoningBoundaryDetail(
            string name, ExpandedSummoningBoundary observed,
            ICollection<string> evidenceFiles)
        {
            const string evidence =
                "each choice under the eighteen canonical summon parents, judged by identity against the " +
                "publication catalogs and the identity manifest; published choices only, not registration " +
                "and not casting";
            var assertions = new List<RuntimeTestAssertion>();
            ExpandedSummoningPublicationAudit audit = observed.Audit;
            if (audit == null)
            {
                assertions.Add(Assertion(name + "-ledger",
                    "every choice under the eighteen parents judged against the publication contract",
                    "not judged: " + observed.Fault, false, evidence));
                return assertions;
            }

            Action<string, SummonAuditOutcome> add = (suffix, outcome) =>
                assertions.Add(Assertion(name + suffix, outcome.Expected,
                    outcome.Observed, outcome.Passed, evidence));
            add("-required-choices", audit.RequiredChoices());
            add("-declared-boundary", audit.Boundary());
            add("-duplicates", audit.Duplicates());
            add("-readable", audit.Readable());

            string written;
            try
            {
                JObject ledger = audit.ToJson();
                ledger["runId"] = _request.RunId;
                ledger["scenario"] = _request.Scenario;
                ledger["loadedModVersion"] = _context.ModEntry.Info.Version;
                string path = Path.Combine(_request.EvidenceDirectory,
                    ExpandedSummoningPublicationAudit.LedgerFile);
                RuntimeTestResultWriter.WriteAtomic(path, ledger.ToString());
                if (evidenceFiles != null && !evidenceFiles.Contains(path))
                    evidenceFiles.Add(path);
                written = null;
            }
            catch (Exception exception)
            {
                written = exception.GetType().Name + ": " + exception.Message;
            }

            // The choices that are not this project's are reported here and
            // not constrained; the ledger holds each one.
            assertions.Add(Assertion(name + "-ledger",
                "the ledger of every choice is written beside the result",
                (written == null ? "written=" +
                    ExpandedSummoningPublicationAudit.LedgerFile :
                    "not written: " + written) + ";choices=" +
                    audit.Entries.Count(value => value.Index >= 0) + ";" +
                    audit.State.Describe() + ";" + audit.DescribeOthers(),
                written == null, evidence));
            return assertions;
        }
    }
}
