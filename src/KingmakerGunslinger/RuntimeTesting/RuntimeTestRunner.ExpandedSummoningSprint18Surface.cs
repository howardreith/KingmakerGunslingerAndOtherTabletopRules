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
    /// Sprint 18 review.
    ///
    /// <para>The surface checks are blueprint-level and read-only: every one
    /// of the twenty-six new roots exists in the live library with the
    /// quantity the charter gives it, every one of them is still withheld
    /// from the player, and no previously published root has been withheld
    /// to make room.</para>
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        /// <summary>
        /// All twenty-six roots, resolved in the live library through the
        /// identities the builder registered, with their quantities.
        /// </summary>
        private void ReviewSprint18Surface()
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
                    PrimateRulesPolicy.IsPrimate(value.Creature.Key)).ToArray();
                var rows = new JArray();
                var missing = new List<string>();
                var published = new List<string>();
                foreach (SummonVariantSpec variant in mine
                    .OrderBy(value => value.StableKey, StringComparer.Ordinal))
                {
                    string symbol = ExpandedSummoningIdentityCatalog.AbilitySymbol(variant);
                    BlueprintAbility ability = ResolveSprint18Ability(manifest, symbol);
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
                Sprint18Check(_primateAssertions, _primateRows, "twenty-six-roots-live",
                    mine.Length == 26 && missing.Count == 0 && published.Count == 0,
                    new JObject { ["roots"] = mine.Length, ["missing"] = new JArray(missing),
                        ["wronglyPublished"] = new JArray(published), ["detail"] = rows },
                    "all twenty-six new roots exist in the live library and all are withheld");

                int publishedElsewhere = all.Count(value =>
                    !PrimateRulesPolicy.IsPrimate(value.Creature.Key) &&
                    !SummonVisibilityCatalog.IsPublished(value));
                Sprint18Check(_primateAssertions, _primateRows, "no-other-root-withheld",
                    publishedElsewhere == 0 && SummonVisibilityCatalog
                        .PublishedLogicalPlacementCount == 1008,
                    new JObject {
                        ["registered"] = SummonVisibilityCatalog.RegisteredLogicalPlacementCount,
                        ["suppressed"] = SummonVisibilityCatalog.SuppressedLogicalPlacementCount,
                        ["published"] = SummonVisibilityCatalog.PublishedLogicalPlacementCount,
                        ["otherWithheld"] = publishedElsewhere },
                    "Sprint 18 withholds exactly its own roots and no previously published one");

                // The module switch. With Expanded Summoning off, no ape may
                // be rebodied at all, whatever its identity.
                string key;
                bool reachableWhenOff = PrimateVisualPolicy.TryProductionPrimate(false,
                        PrimateVisualPolicy.ApeGuid, PrimateVisualPolicy.ApeBlueprintName,
                        PrimateVisualPolicy.TrollPrefab, out key) ||
                    PrimateVisualPolicy.TryProductionPrimate(false,
                        PrimateVisualPolicy.DireApeGuid,
                        PrimateVisualPolicy.DireApeBlueprintName,
                        PrimateVisualPolicy.TrollPrefab, out key);
                bool nativeTrollReachable = PrimateVisualPolicy.TryProductionPrimate(true,
                    PrimateVisualPolicy.TrollBlueprint, "CR10_FerociousTrollGuard",
                    PrimateVisualPolicy.TrollPrefab, out key);
                Sprint18Check(_primateAssertions, _primateRows, "module-off-and-native-donor",
                    !reachableWhenOff && !nativeTrollReachable, new JObject {
                        ["reachableWithModuleOff"] = reachableWhenOff,
                        ["nativeTrollReachable"] = nativeTrollReachable,
                        ["moduleActive"] = _context.FeatureModules.Active.ExpandedSummoning },
                    "a disabled module rebodies nothing and the native troll is never touched");
            }
            catch (Exception error)
            {
                Sprint18Check(_primateAssertions, _primateRows, "surface-exception", false,
                    new JObject { ["exception"] =
                        DescribeExpandedSummoningCorrectionException(error) },
                    "the registered surface review completes without an exception");
            }
        }

        private BlueprintAbility ResolveSprint18Ability(
            KingmakerGunslinger.Blueprints.BlueprintManifest manifest, string symbol)
        {
            try
            {
                KingmakerGunslinger.Blueprints.BlueprintManifestEntry entry =
                    manifest.ResolveActive(symbol, typeof(BlueprintAbility));
                return entry == null ? null :
                    ResourcesLibrary.TryGetBlueprint<BlueprintAbility>(entry.Id.ToString());
            }
            catch (Exception) { return null; }
        }

        private void CleanupSprint18Review()
        {
            IEnumerator<int> steps = _primateSteps;
            _primateSteps = null;
            ExpandedSummoningCorrectionFixture fixture = _primateFixture;
            _primateFixture = null;
            try { if (steps != null) steps.Dispose(); }
            catch (Exception error)
            {
                _primateAssertions.Add(Assertion("sprint18-disposal", "iterator cleanup",
                    error.Message, false, "owned-only"));
            }
            bool cleaned = false;
            try { EndExpandedSummoningCorrectionFixture(fixture, out cleaned); }
            catch (Exception error)
            {
                _primateAssertions.Add(Assertion("sprint18-cleanup-error", "native cleanup",
                    error.Message, false, "owned-only"));
            }
            _primateAssertions.Add(Assertion("sprint18-fixture-cleanup",
                "exact original unit/party/area references", "cleaned=" + cleaned, cleaned,
                "No save write and no unrelated-unit cleanup."));
        }

        private void FinishSprint18Review()
        {
            ReviewSprint18Surface();
            CleanupSprint18Review();
            _primateAssertions.Add(Assertion("loaded-mod-version", _request.ExpectedModVersion,
                _context.ModEntry.Info.Version,
                _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                "Loaded UMM version."));
            File.WriteAllText(Path.Combine(_request.EvidenceDirectory,
                "sprint18-review.json"), _primateRows.ToString(Formatting.Indented));
            _primateComplete = true;
            Complete(CreateResult(_primateAssertions.All(value => value.Status == "PASS")
                ? "PASS" : "FAIL", _primateAssertions, null));
        }

        // Called by Complete on an outer timeout or error too, so the
        // iterator's actor, pause and random-state finally blocks run before
        // the working-save sentinels close.
        private void StopSprint18Review(RuntimeTestResult result)
        {
            if (_primateSteps == null && _primateFixture == null) return;
            CleanupSprint18Review();
        }
    }
}
