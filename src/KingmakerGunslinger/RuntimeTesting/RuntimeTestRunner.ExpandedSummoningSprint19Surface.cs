using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// The registered surface, the module switch, and the close of the
    /// Sprint 19 review.
    ///
    /// <para>The surface checks are blueprint-level and read-only: every one
    /// of the ten new roots exists in the live library with the quantity the
    /// charter gives it, every one of them is still withheld from the player,
    /// and no previously published root - the Sprint 18 apes included - has
    /// been withheld to make room.</para>
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        /// <summary>
        /// All ten roots, resolved in the live library through the identities
        /// the builder registered, with their quantities.
        /// </summary>
        private void ReviewSprint19Surface()
        {
            try
            {
                KingmakerGunslinger.Blueprints.BlueprintManifest manifest =
                    KingmakerGunslinger.Blueprints.BlueprintManifest.Load(
                        _context.ModEntry.Path);
                SummonVariantSpec[] all = ExpandedSummoningCatalog
                    .GenerateVariants(SummonFamily.Monster)
                    .Concat(ExpandedSummoningCatalog.GenerateVariants(
                        SummonFamily.NaturesAlly)).ToArray();
                SummonVariantSpec[] mine = all.Where(value =>
                    Sprint19ReviewPolicy.Keys.Contains(value.Creature.Key,
                        StringComparer.Ordinal)).ToArray();
                var rows = new JArray();
                var missing = new List<string>();
                var published = new List<string>();
                foreach (SummonVariantSpec variant in mine
                    .OrderBy(value => value.StableKey, StringComparer.Ordinal))
                {
                    string symbol = ExpandedSummoningIdentityCatalog
                        .AbilitySymbol(variant);
                    BlueprintAbility ability =
                        ResolveSprint18Ability(manifest, symbol);
                    bool withheld = !SummonVisibilityCatalog.IsPublished(variant);
                    if (ability == null) missing.Add(variant.StableKey);
                    if (!withheld) published.Add(variant.StableKey);
                    rows.Add(new JObject {
                        ["root"] = variant.StableKey,
                        ["family"] = variant.Family.ToString(),
                        ["parentTier"] = variant.ParentTier,
                        ["quantity"] = variant.Multiplicity.ToString(),
                        ["symbol"] = symbol,
                        ["resolved"] = ability != null,
                        ["guid"] = ability == null ? null : ability.AssetGuid,
                        ["withheld"] = withheld });
                }
                Sprint19Check(_sprint19Assertions, _sprint19Rows,
                    "ten-roots-live",
                    mine.Length == 10 && missing.Count == 0 &&
                    published.Count == 0,
                    new JObject { ["roots"] = mine.Length,
                        ["missing"] = new JArray(missing),
                        ["wronglyPublished"] = new JArray(published),
                        ["detail"] = rows },
                    "all ten new roots exist in the live library and all are "
                    + "withheld");

                int withheldElsewhere = all.Count(value =>
                    !Sprint19ReviewPolicy.Keys.Contains(value.Creature.Key,
                        StringComparer.Ordinal) &&
                    !SummonVisibilityCatalog.IsPublished(value));
                Sprint19Check(_sprint19Assertions, _sprint19Rows,
                    "no-other-root-withheld",
                    withheldElsewhere == 0 && SummonVisibilityCatalog
                        .PublishedLogicalPlacementCount == 1034,
                    new JObject {
                        ["registered"] = SummonVisibilityCatalog
                            .RegisteredLogicalPlacementCount,
                        ["suppressed"] = SummonVisibilityCatalog
                            .SuppressedLogicalPlacementCount,
                        ["published"] = SummonVisibilityCatalog
                            .PublishedLogicalPlacementCount,
                        ["otherWithheld"] = withheldElsewhere },
                    "Sprint 19 withholds exactly its own roots and no "
                    + "previously published one");

                // The module switch. With Expanded Summoning off, neither new
                // creature may be rebodied at all, whatever its identity, and
                // the native donor is never touched in either state.
                string key;
                bool reachableWhenOff = PrimateVisualPolicy.TryProductionPrimate(
                        false, PrimateVisualPolicy.GirallonGuid,
                        PrimateVisualPolicy.GirallonBlueprintName,
                        PrimateVisualPolicy.TrollPrefab, out key) ||
                    PrimateVisualPolicy.TryProductionPrimate(false,
                        PrimateVisualPolicy.XillGuid,
                        PrimateVisualPolicy.XillBlueprintName,
                        PrimateVisualPolicy.TrollPrefab, out key);
                bool nativeTrollReachable =
                    PrimateVisualPolicy.TryProductionPrimate(true,
                        PrimateVisualPolicy.TrollBlueprint,
                        "CR10_FerociousTrollGuard",
                        PrimateVisualPolicy.TrollPrefab, out key);
                Sprint19Check(_sprint19Assertions, _sprint19Rows,
                    "module-off-and-native-donor",
                    !reachableWhenOff && !nativeTrollReachable, new JObject {
                        ["reachableWithModuleOff"] = reachableWhenOff,
                        ["nativeTrollReachable"] = nativeTrollReachable,
                        ["moduleActive"] = _context.FeatureModules.Active
                            .ExpandedSummoning },
                    "a disabled module rebodies nothing and the native troll "
                    + "is never touched");
            }
            catch (Exception error)
            {
                Sprint19Check(_sprint19Assertions, _sprint19Rows,
                    "surface-exception", false,
                    new JObject { ["exception"] =
                        DescribeExpandedSummoningCorrectionException(error) },
                    "the registered surface review completes without an exception");
            }
        }

        private void CleanupSprint19Review()
        {
            IEnumerator<int> steps = _sprint19Steps;
            _sprint19Steps = null;
            ExpandedSummoningCorrectionFixture fixture = _sprint19Fixture;
            _sprint19Fixture = null;
            try { if (steps != null) steps.Dispose(); }
            catch (Exception error)
            {
                _sprint19Assertions.Add(Assertion("sprint19-disposal",
                    "iterator cleanup", error.Message, false, "owned-only"));
            }
            bool cleaned = false;
            try { EndExpandedSummoningCorrectionFixture(fixture, out cleaned); }
            catch (Exception error)
            {
                _sprint19Assertions.Add(Assertion("sprint19-cleanup-error",
                    "native cleanup", error.Message, false, "owned-only"));
            }
            _sprint19Assertions.Add(Assertion("sprint19-fixture-cleanup",
                "exact original unit/party/area references", "cleaned=" + cleaned,
                cleaned, "No save write and no unrelated-unit cleanup."));
        }

        private void FinishSprint19Review()
        {
            ReviewSprint19Surface();
            CleanupSprint19Review();
            _sprint19Assertions.Add(Assertion("loaded-mod-version",
                _request.ExpectedModVersion, _context.ModEntry.Info.Version,
                _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                "Loaded UMM version."));
            File.WriteAllText(Path.Combine(_request.EvidenceDirectory,
                "sprint19-review.json"),
                _sprint19Rows.ToString(Formatting.Indented));
            _sprint19Complete = true;
            Complete(CreateResult(
                _sprint19Assertions.All(value => value.Status == "PASS") ?
                    "PASS" : "FAIL", _sprint19Assertions, null));
        }

        // Called by Complete on an outer timeout or error too, so the
        // iterator's actor, pause and random-state finally blocks run before
        // the working-save sentinels close.
        private void StopSprint19Review(RuntimeTestResult result)
        {
            if (_sprint19Steps == null && _sprint19Fixture == null) return;
            CleanupSprint19Review();
        }
    }
}
