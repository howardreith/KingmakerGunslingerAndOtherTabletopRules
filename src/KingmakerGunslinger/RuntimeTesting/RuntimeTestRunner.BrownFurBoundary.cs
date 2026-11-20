using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker.Blueprints.Classes;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.BrownFur;
using KingmakerGunslinger.FeatureModules;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Gathers what the Brown-Fur publication check reads and hands it to
    /// <see cref="BrownFurPublicationAudit"/>, which judges it as separate
    /// named results.
    ///
    /// The check used to require the transaction's evidence to say it had
    /// found as many archetypes as the selector holds now. That holds only
    /// while nothing is added after this module publishes. Since the
    /// module publishes at the package's own blueprint lifecycle boundary,
    /// another mod may add archetypes afterwards, and the transaction's
    /// counts then rightly differ from the final count. The transaction is
    /// now judged against its own record, taken when it ran, and the final
    /// selector for what must still be true of it.
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        private BrownFurPublicationAudit ObserveBrownFurPublication(bool requested)
        {
            CotwArcanistResolution resolution = BrownFurOptionalExtensionCoordinator.Current;
            BrownFurBlueprintSet blueprints = BrownFurOptionalExtensionCoordinator.Blueprints;
            BrownFurFeatureStatus status = BrownFurFeatureStatusRegistry.Current;
            var reading = new BrownFurPublicationReading
            {
                ProviderAbsent = AppDomain.CurrentDomain.GetAssemblies().All(
                    value => value.GetName().Name != "CallOfTheWild"),
                Requested = requested,
                Active = _context.FeatureModules.Active.BrownFurTransmuter,
                Availability = status.Availability,
                StatusPublished = status.Published,
                ContractCompatible = resolution != null &&
                    resolution.Decision.IsCompatible && resolution.Contract != null &&
                    blueprints != null &&
                    blueprints.Count == BrownFurIdentityCatalog.IdentityCount,
                ContractObjectsAbsent = resolution == null && blueprints == null,
                RegisteredIdentities = BrownFurIdentityCatalog.All.Count(spec =>
                    BlueprintBootstrap.Library.BlueprintsByAssetId.ContainsKey(spec.Guid)),
                ArchetypeGuid = BrownFurIdentityCatalog.All.Single(spec =>
                    spec.Symbol == BrownFurIdentityCatalog.Archetype).Guid,
                ReferencesInAnyClass = BlueprintBootstrap.Library.GetAllBlueprints()
                    .OfType<BlueprintCharacterClass>().Sum(value =>
                        (value.Archetypes ?? Array.Empty<BlueprintArchetype>()).Count(archetype =>
                            archetype != null && BrownFurIdentityCatalog.All.Any(spec =>
                                spec.Guid == archetype.AssetGuid))),
                Evidence = BrownFurOptionalExtensionCoordinator.PublicationEvidence.ToList(),
                Observations = BrownFurOptionalExtensionCoordinator.PublicationObservations.ToList()
            };
            IReadOnlyList<string> reconciled =
                BrownFurOptionalExtensionCoordinator.ReconciledSelector;
            reading.ReconciledSelector = reconciled == null ? null : reconciled.ToList();
            BlueprintCharacterClass arcanist = resolution == null ||
                resolution.Contract == null ? null : resolution.Contract.ArcanistClass;
            if (arcanist != null)
            {
                BlueprintArchetype registered = blueprints == null ? null :
                    blueprints.Archetype;
                string fault;
                reading.Selector = BrownFurSelectorEntry.Read<BlueprintArchetype>(
                    () => arcanist.Archetypes, value => value.AssetGuid,
                    value => value.name,
                    value => registered != null && ReferenceEquals(value, registered),
                    out fault);
                reading.SelectorFault = fault;
            }
            else if (!reading.ProviderAbsent)
            {
                reading.SelectorFault = "no Arcanist class was resolved";
            }

            return BrownFurPublicationAudit.Evaluate(reading);
        }

        /// <summary>
        /// The named results that stand beside the aggregate, each its own
        /// assertion, and the ledger written beside the result.
        /// </summary>
        private List<RuntimeTestAssertion> BrownFurPublicationDetail(string name,
            BrownFurPublicationAudit audit, ICollection<string> evidenceFiles)
        {
            const string evidence =
                "the Brown-Fur archetype in the Call of the Wild Arcanist selector: the transaction judged " +
                "against its own record from when it ran, the final selector judged after every provider " +
                "has finished; identities, not counts, where preservation and order are judged";
            var assertions = audit.Results.Select(value => Assertion(
                name + "-" + value.Name, value.Expected, value.Observed, value.Passed,
                evidence)).ToList();
            string written;
            try
            {
                JObject ledger = audit.ToJson();
                ledger["runId"] = _request.RunId;
                ledger["scenario"] = _request.Scenario;
                ledger["loadedModVersion"] = _context.ModEntry.Info.Version;
                string path = Path.Combine(_request.EvidenceDirectory,
                    BrownFurPublicationAudit.LedgerFile);
                RuntimeTestResultWriter.WriteAtomic(path, ledger.ToString());
                if (evidenceFiles != null && !evidenceFiles.Contains(path))
                    evidenceFiles.Add(path);
                written = null;
            }
            catch (Exception exception)
            {
                written = exception.GetType().Name + ": " + exception.Message;
            }

            assertions.Add(Assertion(name + "-ledger",
                "the ledger of the selector and the transaction's record is written beside the result",
                written == null ? "written=" + BrownFurPublicationAudit.LedgerFile +
                    ";results=" + audit.Results.Count + ";laterAdditions=" +
                    audit.LaterAdditions.Count : "not written: " + written,
                written == null, evidence));
            return assertions;
        }
    }
}
