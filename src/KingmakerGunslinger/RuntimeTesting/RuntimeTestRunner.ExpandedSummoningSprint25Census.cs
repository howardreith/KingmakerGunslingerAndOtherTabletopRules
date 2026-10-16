using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.View;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        /// <summary>
        /// The one bounded Sprint 25 quadruped donor census. It reuses the
        /// Sprint 24 capture helpers unchanged: those already emit complete
        /// local and world rest transforms, the renderer bind frame with per
        /// bone parent/bindPosition/bindRotation, anatomy chains, animation
        /// actions with their clips, colliders and materials. Nothing new is
        /// invented to read the donor.
        ///
        /// <para>Two questions this answers that a metadata-only survey could
        /// not. First, whether the Mastodon rig's neck, trunk, tail and leg
        /// chains can carry a credible long-necked sauropod and a trunked
        /// elephant, or whether a bounded alternative is needed. Second,
        /// whether the installed library exposes concrete Great Cleave and
        /// Greater Overrun feats at all, which is what separates "this project
        /// never mapped it" from "the engine does not implement it".</para>
        ///
        /// <para>Research evidence only. It proves donor structure and native
        /// feat presence; it qualifies neither Sprint 25 creature, and this
        /// candidate is never published. Detached read-only prefabs: no spawn,
        /// save, actor, asset mutation or gameplay event.</para>
        /// </summary>
        private RuntimeTestResult RunExpandedSummoningSprint25DonorCensus()
        {
            WriteLifecycleStage("expanded-summoning-sprint25-donor-census-start");
            var assertions = new List<RuntimeTestAssertion>();
            var rows = new JArray();
            var facts = new JArray();
            var candidates = new JArray();
            var document = new JObject {
                ["scope"] = "Sprint 25 closed quadruped donor census; detached read-only native prefabs and exact native feat graphs; no spawn, save, asset mutation or gameplay event.",
                ["decision"] = "Research evidence only. It proves donor structure and native feat presence, not creature qualification, and this candidate is never published.",
                ["runId"] = _request.RunId,
                ["donors"] = rows,
                ["factAnchors"] = facts,
                ["featCandidates"] = candidates
            };

            try
            {
                Sprint25DonorCensusPolicy.Validate();
                BlueprintScriptableObject[] all = BlueprintBootstrap.Library
                    .GetAllBlueprints().Where(value => value != null).ToArray();
                BlueprintUnit[] units = all.OfType<BlueprintUnit>().ToArray();
                int captured = 0;
                int discoveredPrefabs = 0;

                foreach (string[] anchor in Sprint25DonorCensusPolicy.Donors)
                {
                    BlueprintUnit unit = units.SingleOrDefault(value =>
                        string.Equals(value.AssetGuid, anchor[1],
                            StringComparison.Ordinal));
                    if (unit == null)
                        throw new InvalidOperationException("creature=" + anchor[0] +
                            ";predicate=exact-donor;guid=" + anchor[1]);
                    string actualPrefab = unit.Prefab == null ? null : unit.Prefab.AssetId;
                    bool discovering = string.Equals(anchor[2],
                        Sprint25DonorCensusPolicy.DiscoverPrefab,
                        StringComparison.Ordinal);
                    if (discovering)
                    {
                        if (string.IsNullOrEmpty(actualPrefab))
                            throw new InvalidOperationException("creature=" + anchor[0] +
                                ";predicate=discoverable-prefab;actual=null");
                        discoveredPrefabs++;
                    }
                    else if (!string.Equals(actualPrefab, anchor[2],
                        StringComparison.Ordinal))
                        throw new InvalidOperationException("creature=" + anchor[0] +
                            ";predicate=exact-prefab;expected=" + anchor[2] +
                            ";actual=" + actualPrefab);
                    UnitEntityView prefab = unit.Prefab.Load(false);
                    if (prefab == null)
                        throw new InvalidOperationException("creature=" + anchor[0] +
                            ";predicate=detached-prefab-load");

                    JObject row = CapturePrimateCensusViewMetadata(prefab,
                        anchor[0], unit.AssetGuid, unit.name, actualPrefab);
                    row["prefabWasDiscovered"] = discovering;
                    row["nativeUnit"] = DescribeNativeUnit(unit);
                    row["size"] = unit.Size.ToString();
                    row["restHierarchy"] = CaptureSprint24RestHierarchy(prefab);
                    row["anatomy"] = CaptureSprint24Anatomy(prefab);
                    row["animationManagers"] =
                        CaptureSprint24AnimationManagers(prefab);
                    row["colliders"] = CaptureSprint24Colliders(prefab);
                    rows.Add(row);
                    captured++;
                }

                foreach (string[] anchor in Sprint25DonorCensusPolicy.FactAnchors)
                {
                    BlueprintScriptableObject fact = all.SingleOrDefault(value =>
                        string.Equals(value.AssetGuid, anchor[1],
                            StringComparison.Ordinal));
                    facts.Add(new JObject {
                        ["key"] = anchor[0], ["guid"] = anchor[1],
                        ["found"] = fact != null,
                        ["name"] = fact == null ? null : fact.name,
                        ["type"] = fact == null ? null : fact.GetType().FullName,
                        ["graph"] = fact == null ? JValue.CreateNull() :
                            DescribeGraph(fact, 0, new HashSet<object>(
                                NativeDonorReferenceComparer.Instance), 8)
                    });
                }

                BlueprintScriptableObject[] discovered = all.Where(value =>
                        Sprint25DonorCensusPolicy.MatchesFeatCandidate(value.name))
                    .OrderBy(value => value.name, StringComparer.Ordinal)
                    .ThenBy(value => value.AssetGuid, StringComparer.Ordinal)
                    .Take(Sprint25DonorCensusPolicy
                        .MaximumDiscoveredFeatBlueprints + 1).ToArray();
                if (discovered.Length > Sprint25DonorCensusPolicy
                    .MaximumDiscoveredFeatBlueprints)
                    throw new InvalidOperationException(
                        "predicate=bounded-feat-discovery;actual=" +
                        discovered.Length);
                foreach (BlueprintScriptableObject value in discovered)
                    candidates.Add(new JObject {
                        ["guid"] = value.AssetGuid, ["name"] = value.name,
                        ["type"] = value.GetType().FullName,
                        ["graph"] = DescribeGraph(value, 0,
                            new HashSet<object>(
                                NativeDonorReferenceComparer.Instance), 8)
                    });

                // ---------------------------------------------------------
                // The released Elephant, as a production creature. Its
                // three Sprint 25 carrier symbols are reserved names with
                // no allocated identity, and the recorded condition for
                // appending any of them is exactly this observation.
                BlueprintUnit elephant = units.SingleOrDefault(value =>
                    string.Equals(value.AssetGuid,
                        Sprint25DonorCensusPolicy.ReleasedElephantUnitGuid,
                        StringComparison.Ordinal));
                if (elephant == null)
                    throw new InvalidOperationException(
                        "creature=released-elephant;predicate=exact-unit;guid=" +
                        Sprint25DonorCensusPolicy.ReleasedElephantUnitGuid);

                var elephantGrants = new JArray();
                bool trampleFound = false;
                foreach (BlueprintUnitFact fact in
                    (elephant.AddFacts ?? Array.Empty<BlueprintUnitFact>())
                        .Where(value => value != null))
                {
                    JToken graph = DescribeGraph(fact, 0,
                        new HashSet<object>(
                            NativeDonorReferenceComparer.Instance), 6);
                    string rendered = graph == null ? string.Empty
                        : graph.ToString(Formatting.None);
                    bool marker = Sprint25DonorCensusPolicy
                        .TrampleComponentMarkers.Any(value =>
                            rendered.IndexOf(value,
                                StringComparison.Ordinal) >= 0);
                    if (marker) trampleFound = true;
                    elephantGrants.Add(new JObject {
                        ["guid"] = fact.AssetGuid,
                        ["name"] = fact.name,
                        ["type"] = fact.GetType().FullName,
                        ["carriesTrampleMarker"] = marker,
                        ["graph"] = graph
                    });
                }

                // The printed routine is two primary attacks. Which weapon
                // sits in which limb slot is what decides whether a line
                // takes the native secondary penalty, so the slots are
                // reported separately rather than flattened.
                var bodyRecord = new JObject();
                if (elephant.Body != null)
                {
                    // The slot is typed as hand equipment, and a natural
                    // weapon is one kind of it; anything else is reported
                    // as absent rather than silently coerced.
                    bodyRecord["primaryHand"] = DescribeWeaponSlot(
                        elephant.Body.PrimaryHand as BlueprintItemWeapon);
                    bodyRecord["primaryHandDeclaredType"] =
                        elephant.Body.PrimaryHand == null ? null
                            : elephant.Body.PrimaryHand.GetType().FullName;
                    var additional = new JArray();
                    foreach (BlueprintItemWeapon weapon in
                        elephant.Body.AdditionalLimbs ??
                        Array.Empty<BlueprintItemWeapon>())
                        additional.Add(DescribeWeaponSlot(weapon));
                    bodyRecord["additionalLimbs"] = additional;
                    var secondary = new JArray();
                    foreach (BlueprintItemWeapon weapon in
                        elephant.Body.AdditionalSecondaryLimbs ??
                        Array.Empty<BlueprintItemWeapon>())
                        secondary.Add(DescribeWeaponSlot(weapon));
                    bodyRecord["additionalSecondaryLimbs"] = secondary;
                }

                var routes = new JArray();
                foreach (string guid in
                    Sprint25DonorCensusPolicy.ReleasedElephantRoutes)
                {
                    BlueprintScriptableObject route = all.SingleOrDefault(
                        value => string.Equals(value.AssetGuid, guid,
                            StringComparison.Ordinal));
                    routes.Add(new JObject {
                        ["guid"] = guid,
                        ["found"] = route != null,
                        ["name"] = route == null ? null : route.name,
                        ["type"] = route == null ? null
                            : route.GetType().FullName
                    });
                }

                document["releasedElephant"] = new JObject {
                    ["scope"] = "Released production creature, observed as metadata only. It records which grants, abilities and attack carriers resolve; it records nothing about any live command outcome.",
                    ["guid"] = elephant.AssetGuid,
                    ["blueprintName"] = elephant.name,
                    ["size"] = elephant.Size.ToString(),
                    ["unitType"] = elephant.Type == null ? null
                        : elephant.Type.name,
                    ["prefab"] = elephant.Prefab == null ? null
                        : elephant.Prefab.AssetId,
                    ["grantCount"] = elephantGrants.Count,
                    ["grants"] = elephantGrants,
                    ["tramplePresent"] = trampleFound,
                    ["body"] = bodyRecord,
                    ["routes"] = routes,
                    ["nativeUnit"] = DescribeNativeUnit(elephant)
                };

                bool routesResolved = routes.OfType<JObject>()
                    .All(value => (bool)value["found"]);
                assertions.Add(Assertion("sprint25-census-released-elephant",
                    "the released Elephant resolves with its eight routes and its own carriers",
                    "grants=" + elephantGrants.Count + ";routes=" +
                        routes.Count + ";routesResolved=" + routesResolved +
                        ";tramplePresent=" + trampleFound,
                    routesResolved && elephantGrants.Count > 0,
                    "metadata only; no spawn, save or command"));
                bool anchorsFound = facts.OfType<JObject>()
                    .All(value => (bool)value["found"]);
                assertions.Add(Assertion("sprint25-census-donors",
                    "both exact detached quadruped-census candidates are captured",
                    "captured=" + captured + ";expected=" +
                        Sprint25DonorCensusPolicy.Donors.Length +
                        ";discoveredPrefabs=" + discoveredPrefabs,
                    captured == Sprint25DonorCensusPolicy.Donors.Length,
                    "read-only prefabs; no actor or save"));
                assertions.Add(Assertion("sprint25-census-positive-controls",
                    "every already-mapped native carrier resolves exactly",
                    "anchors=" + string.Join(",",
                        Sprint25DonorCensusPolicy.FactAnchors.Select(v => v[0])),
                    anchorsFound,
                    "a census that cannot find Endurance or Cleave is measuring nothing"));
                assertions.Add(Assertion("sprint25-census-bounded",
                    "closed inputs and bounded feat-name results",
                    "donors=" + rows.Count + ";facts=" + facts.Count +
                        ";candidates=" + candidates.Count,
                    rows.Count == Sprint25DonorCensusPolicy.Donors.Length &&
                        facts.Count == Sprint25DonorCensusPolicy.FactAnchors.Length &&
                        candidates.Count <= Sprint25DonorCensusPolicy
                            .MaximumDiscoveredFeatBlueprints,
                    "fixed source-owned identities and terms"));
            }
            catch (Exception exception)
            {
                document["exception"] =
                    DescribeExpandedSummoningCorrectionException(exception);
                assertions.Add(Assertion("sprint25-census-capture",
                    "bounded read-only census completes",
                    document["exception"].ToString(), false,
                    "unknown metadata is not qualification"));
            }

            string fileName = "sprint25-donor-census.json";
            RuntimeTestResultWriter.WriteAtomic(Path.Combine(
                _request.EvidenceDirectory, fileName),
                document.ToString(Formatting.Indented));
            WriteLifecycleStage("expanded-summoning-sprint25-donor-census-written");
            assertions.Add(Assertion("loaded-mod-version",
                _request.ExpectedModVersion, _context.ModEntry.Info.Version,
                _context.ModEntry.Info.Version == _request.ExpectedModVersion,
                "Unity Mod Manager ModEntry.Info.Version"));
            return CreateResult(assertions.All(value => value.Status == "PASS")
                ? "PASS" : "FAIL", assertions, null);
        }

        /// <summary>
        /// Exact identity and declared dice for one natural-weapon slot.
        /// Which slot a weapon occupies is what decides whether it takes
        /// the native secondary penalty, so the slot is never flattened.
        /// </summary>
        private static JObject DescribeWeaponSlot(BlueprintItemWeapon weapon)
        {
            if (weapon == null) return null;
            return new JObject {
                ["guid"] = weapon.AssetGuid,
                ["name"] = weapon.name,
                // The printed dice live in the private m_DamageDice
                // field, and this class already has the reader for it.
                ["damageDice"] = DescribeWeaponDice(weapon),
                ["type"] = weapon.Type == null ? null : weapon.Type.name
            };
        }
    }
}
