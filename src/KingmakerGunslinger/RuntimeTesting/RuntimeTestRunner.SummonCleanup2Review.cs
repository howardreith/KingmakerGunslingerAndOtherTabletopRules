using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UI.ActionBar;
using Kingmaker.UI.Group;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Request-owned instrumentation only. The integrator registers this exact
    // scenario with the existing save-free first-area fixture/clock/cleanup.
    // No ordinary game callback or human-only save observer invokes it.
    internal sealed partial class RuntimeTestRunner
    {
        private bool IsSummonCleanup2Review { get { return _request != null &&
            _request.Scenario == "disposable-expanded-summoning-cleanup2-native-review"; } }

        private readonly JArray _summonCleanup2Observations = new JArray();
        private const BindingFlags Cleanup2Fields = BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic;

        private void Cleanup2Check(string id, bool pass, JObject row, string expected)
        {
            row["runId"] = _request.RunId;
            row["frame"] = Time.frameCount;
            row["utc"] = DateTime.UtcNow.ToString("o");
            row["gameSeconds"] = Game.Instance.Player.GameTime.TotalSeconds;
            row["predicate"] = id;
            row["passed"] = pass;
            _summonCleanup2Observations.Add(row);
            RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory,
                "summon-cleanup2-observations.json"), _summonCleanup2Observations.ToString(Formatting.Indented));
            Sprint25Check("cleanup2-" + id, pass, row, expected);
        }

        private IEnumerable<int> ReviewSummonCleanup2(ExpandedSummoningCorrectionFixture fixture)
        {
            string phase = _request.Parameters == null ? null : (string)_request.Parameters["reviewPhase"];
            if (!IsSummonCleanup2Review || !_request.ExitAfterCompletion ||
                _request.Parameters == null || _request.Parameters.Count != 1 || !ExpandedSummoningNativeProfilePolicy.Cleanup2Phase(phase))
                throw new InvalidOperationException("Exact automatic-exit Cleanup 2 request/phase required.");
            bool pause = Game.Instance.IsPaused;
            bool turnBased = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool appearanceWait = _sprint25WaitingForUnityAppearance;
            object levelController = null;
            try
            {
                Game.Instance.IsPaused = true;
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate();
                var human = BlueprintRoot.Instance.Progression.CharacterRaces.Single(value =>
                    value.AssetGuid == "0a5d473ead98b0646b94495af250fdc4" && value.name == "HumanRace");
                if (fixture.Caster.Blueprint != fixture.CasterBlueprint ||
                    fixture.CasterBlueprint == BlueprintRoot.Instance.DefaultPlayerCharacter)
                    throw new InvalidOperationException("Cleanup 2 caster is not the request-owned DefaultPlayerCharacter clone.");
                if (fixture.Caster.Descriptor.Progression.Race != null &&
                    fixture.Caster.Descriptor.Progression.Race != human)
                    throw new InvalidOperationException("Cleanup 2 refuses a non-Human caster as an art anchor.");
                // DefaultPlayerCharacter intentionally has no pre-chargen race.
                // Both mutations belong to this exact disposable clone only.
                fixture.CasterBlueprint.Race = human;
                fixture.Caster.Descriptor.Progression.SetRace(human);
                bool verifiedHuman = fixture.Caster.Blueprint.Race == human &&
                    fixture.Caster.Descriptor.Progression.Race == human &&
                    fixture.Caster.Descriptor.HasFact(human);
                Cleanup2Check("verified-human", verifiedHuman, new JObject {
                    ["unit"] = fixture.Caster.UniqueId,
                    ["blueprint"] = fixture.Caster.Blueprint.AssetGuid,
                    ["sourceBlueprint"] = BlueprintRoot.Instance.DefaultPlayerCharacter.AssetGuid,
                    ["race"] = fixture.Caster.Descriptor.Progression.Race?.AssetGuid,
                    ["raceFact"] = fixture.Caster.Descriptor.HasFact(human),
                    ["verifiedHuman"] = verifiedHuman,
                    ["artReferenceFeet"] = 6,
                    ["artAnchorPolicy"] = "This verified Human is an explicit six-foot visual comparison anchor, not a native mechanical height claim; no transform calibration is applied."
                }, "The request-owned DefaultPlayerCharacter clone has the exact native Human race and fact.");
                if (!verifiedHuman) throw new InvalidOperationException("Human anchor verification failed.");
                fixture.Caster.Descriptor.Stats.Intelligence.BaseValue = 30;
                BlueprintCharacterClass wizard = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(
                    BlueprintBootstrap.Library, "ba34257984f4c41408ce1dc2004e342e", "native Wizard fixture spellbook");
                AdvanceDisposableSpellcaster(fixture.Caster.Descriptor, wizard, 20, ref levelController);
                Spellbook book = fixture.Caster.Descriptor.GetSpellbook(wizard);
                while (book.CasterLevel < 20) book.AddCasterLevel();
                book.UpdateAllSlotsSize(false);
                book.Rest();
                Cleanup2Check("spellbook-fixture", book != null && book.CasterLevel == 20,
                    new JObject { ["book"] = book.Blueprint.AssetGuid, ["casterLevel"] = book.CasterLevel,
                        ["class"] = wizard.AssetGuid,
                        ["accessLimitation"] = "The disposable Wizard is explicitly granted/memorizes canonical SNA and SM parents for targeted presentation/cast-path observation. This does not prove natural class learning or Druid conversion access." },
                    "A real native CL20 spellbook supplies exact parent/child/slot links; fixture-granted access is explicit.");
                Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 12; frame++) yield return 0;
                if (phase == "bodies")
                    foreach (int frame in ReviewSummonCleanup2Bodies(fixture, book)) yield return frame;
                else if (phase == "elementals")
                    foreach (int frame in ReviewSummonCleanup2Elementals(fixture)) yield return frame;
                else if (phase == "menus")
                {
                    _sprint25WaitingForUnityAppearance = true;
                    foreach (int frame in Cleanup2Menus(fixture, book)) yield return frame;
                }
                else
                    foreach (int frame in Cleanup2GiantLineup(fixture, book)) yield return frame;
            }
            finally
            {
                try { if (levelController != null) levelController.GetType().GetMethod("Cancel").Invoke(levelController, null); }
                finally
                {
                    _sprint25WaitingForUnityAppearance = appearanceWait;
                    try
                    {
                        SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = turnBased;
                        Game.Instance.TurnBasedCombatController.Activate();
                    }
                    finally { Game.Instance.IsPaused = pause; }
                }
            }
        }

        private sealed class Cleanup2Route
        {
            internal string Key;
            internal SummonFamily Family;
            internal int Tier;
            internal BlueprintAbility Parent, Selected;
            internal BlueprintUnit Unit;
            internal Alignment Alignment;
        }

        private Cleanup2Route Cleanup2GiantRoute(ExpandedSummoningCorrectionFixture fixture,
            string key, SummonFamily family)
        {
            var route = new Cleanup2Route { Key = key, Family = family,
                Alignment = family == SummonFamily.Monster ? Alignment.LawfulEvil : Alignment.ChaoticNeutral };
            if (key == "frost-giant")
            {
                var native = SummonNativeExpansionCatalog.All.Single(value =>
                    value.Family == family && value.CreatureKey == "FrostGiant" && value.Multiplicity == SummonMultiplicity.One);
                route.Tier = native.Tier;
                route.Selected = fixture.Blueprints.OfType<BlueprintAbility>().Single(value =>
                    value.name == ExpandedSummoningInternalName(native.Symbol));
                route.Unit = BlueprintLibraryLookup.RequireExact<BlueprintUnit>(BlueprintBootstrap.Library,
                    native.UnitGuid, "retained Frost Giant canonical wrapper");
            }
            else
            {
                var spec = ExpandedSummoningCatalog.GenerateVariants(family).Single(value =>
                    value.Creature.Key == key && value.Multiplicity == SummonMultiplicity.One);
                route.Tier = spec.ParentTier;
                route.Selected = ExpandedSummoningRoot(fixture.Blueprints, spec);
                route.Unit = ExpandedSummoningUnit(fixture.Blueprints, spec);
            }
            string parentGuid = ExpandedSummoningInventoryObserver.CanonicalParentGuids[
                (family == SummonFamily.Monster ? 0 : 9) + route.Tier - 1];
            route.Parent = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                parentGuid, "canonical giant parent");
            if (!route.Parent.Variants.Contains(route.Selected))
                throw new InvalidOperationException("Exact giant child is absent from its live canonical parent: " + key);
            return route;
        }

        private UnitEntityData Cleanup2CastGiant(ExpandedSummoningCorrectionFixture fixture,
            Spellbook book, Cleanup2Route route)
        {
            // Same qualified native parent/slot/UnitUseAbility seam as
            // Sprint24PlayerRoutes, retaining the resulting actor for review.
            fixture.Caster.Descriptor.Alignment.Set(route.Alignment);
            book.Rest();
            AbilityData data = PrepareExpandedSummoningPlayerPathSpell(book,
                route.Parent, route.Selected, route.Tier);
            int slotsBefore = CountAvailableExpandedSummoningSlots(book, route.Tier);
            ExpandedSummoningRuleCapture.Clear();
            ExpandedSummoningPlayerPathEvents.Clear();
            _expandedSummoningPlayerPathRuleCastCount = 0;
            _expandedSummoningPlayerPathSpawnActionCount = 0;
            UnitUseAbility command;
            var parameters = new Kingmaker.AreaLogic.Cutscenes.CutsceneParametersContext();
            using (parameters.Data) command = new UnitUseAbility(data, new TargetWrapper(fixture.Caster.Position));
            bool canCast = data.IsAvailable && command.CanStart;
            try
            {
                if (canCast)
                {
                    command.IgnoreCooldown(TimeSpan.Zero);
                    fixture.Caster.Commands.Run(command);
                    command.Start();
                    if (command.IsRunning)
                    {
                        if (command.Animation != null) command.Animation.IsActed = true;
                        command.Tick();
                        if (command.ExecutionProcess != null)
                            for (int tick = 0; tick < 5000 && !command.ExecutionProcess.IsEnded; tick++)
                                command.ExecutionProcess.Tick();
                        if (command.Animation != null) FinishExpandedSummoningAnimation(command.Animation);
                        if (!command.IsFinished) command.Tick();
                    }
                }
                Game.Instance.EntityCreator.Tick();
            }
            finally
            {
                // Even a native command exception can follow a successful
                // spawn. Register only positive exact caster provenance so
                // shared teardown never loses that owned actor or claims an
                // unrelated native unit from a before/after difference.
                foreach (var unit in ExpandedSummoningRuleCapture.Where(u => u != null &&
                    u.Blueprint == route.Unit && ExpandedSummoningPlayerPathUnitExact(u, fixture.Caster)))
                    if (!fixture.Created.Contains(unit)) fixture.Created.Add(unit);
            }
            UnitEntityData[] units = ExpandedSummoningRuleCapture.Where(value => value != null).Distinct().ToArray();
            int slotsAfter = CountAvailableExpandedSummoningSlots(book, route.Tier);
            bool exact = canCast && command.Result.ToString() == "Success" && units.Length == 1 &&
                units[0].Blueprint == route.Unit && ExpandedSummoningPlayerPathUnitExact(units[0], fixture.Caster) &&
                _expandedSummoningPlayerPathRuleCastCount == 1 && _expandedSummoningPlayerPathSpawnActionCount == 1 &&
                slotsAfter == slotsBefore - 1 && data.ConvertedFrom?.Blueprint == route.Parent &&
                ReferenceEquals(data.Spellbook, book);
            Cleanup2Check("canonical-cast-" + route.Family + "-" + route.Key, exact,
                new JObject { ["key"] = route.Key, ["family"] = route.Family.ToString(), ["tier"] = route.Tier,
                    ["parent"] = route.Parent.AssetGuid, ["selected"] = route.Selected.AssetGuid,
                    ["expectedUnit"] = route.Unit.AssetGuid, ["caster"] = fixture.Caster.UniqueId,
                    ["spellbook"] = book.Blueprint.AssetGuid, ["dataLinks"] = DescribeExpandedSummoningAbilityData(data),
                    ["slotBefore"] = slotsBefore, ["slotAfter"] = slotsAfter,
                    ["paramSlotSpell"] = data.ParamSpellSlot?.Spell?.Blueprint.AssetGuid,
                    ["commandResult"] = command.Result.ToString(), ["canCast"] = canCast,
                    ["ruleCastCount"] = _expandedSummoningPlayerPathRuleCastCount,
                    ["spawnActionCount"] = _expandedSummoningPlayerPathSpawnActionCount,
                    ["units"] = new JArray(units.Select(u => new JObject {
                        ["id"] = u.UniqueId, ["blueprint"] = u.Blueprint.AssetGuid,
                        ["summonProvenance"] = ExpandedSummoningPlayerPathUnitExact(u, fixture.Caster) })),
                    ["executionScope"] = "Real canonical parent, prepared slot, selected child, native UnitUseAbility/RuleCastSpell/spawn action; qualified fixture synchronously advances command/animation. This does not certify natural input timing." },
                "One exact selected canonical giant cast consumes one real slot and creates one provenance-owned actor through one cast/spawn action.");
            if (!exact) throw new InvalidOperationException("Canonical giant cast failed: " + route.Key);
            SetExpandedSummoningBrainActive(units[0], false);
            return units[0];
        }

        private IEnumerable<int> Cleanup2GiantLineup(ExpandedSummoningCorrectionFixture fixture, Spellbook book)
        {
            var hooks = HarmonyInstance.Create("kmg.cleanup2.lineup." + _request.RunId);
            var actors = new List<UnitEntityData> { fixture.Caster };
            var keys = new List<string> { "human-six-foot-art-anchor" };
            const BindingFlags statics = BindingFlags.Static | BindingFlags.NonPublic;
            try
            {
                string[] generatedKeys = { "hill-giant", "stone-giant", "fire-giant", "cloud-giant", "storm-giant" };
                int generatedMonsterRoutes = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                    .Count(v => generatedKeys.Contains(v.Creature.Key));
                Cleanup2Check("giant-route-scope", generatedMonsterRoutes == 0, new JObject {
                    ["generatedMonsterRoutes"] = generatedMonsterRoutes,
                    ["generatedGiantsUse"] = "Their actual canonical SNA single routes; no SM giant route is invented.",
                    ["retainedFrostUses"] = "Canonical SNA VII and SM VIII wrappers." },
                    "Current generated giant families publish SNA routes; retained Frost has both real canonical families.");
                hooks.Patch(typeof(RuleCastSpell).GetMethod("OnTrigger"), null,
                    new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("ExpandedSummoningPlayerPathRuleCastPostfix", statics)), null);
                hooks.Patch(typeof(ContextActionSpawnMonster).GetMethod("RunAction"),
                    new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("ExpandedSummoningPlayerPathSpawnPrefix", statics)), null, null);
                _expandedSummoningPlayerPathCaptureActive = true;
                // Generated giants have only SNA roots. Frost has actual SNA
                // and SM native wrappers; observe both, not a debug donor spawn.
                foreach (string key in new[] { "hill-giant", "stone-giant", "frost-giant", "fire-giant", "cloud-giant", "storm-giant" })
                {
                    actors.Add(Cleanup2CastGiant(fixture, book, Cleanup2GiantRoute(fixture, key, SummonFamily.NaturesAlly)));
                    keys.Add(key);
                    yield return 0;
                }
                UnitEntityData monsterFrost = Cleanup2CastGiant(fixture, book,
                    Cleanup2GiantRoute(fixture, "frost-giant", SummonFamily.Monster));
                foreach (int frame in Cleanup2WaitAppearance(monsterFrost, "sm-frost-giant")) yield return frame;
                var monsterFrostMetric = Cleanup2MeasureActor(monsterFrost, "sm-frost-giant", monsterFrost.Position, Vector3.up);
                Cleanup2Check("sm-frost-presentation", monsterFrostMetric.Valid,
                    monsterFrostMetric.Document,
                    "The retained native Frost Giant also appears through the actual SM VIII wrapper.");
                DisposeExpandedSummoningUnits(fixture.Created, new[] { monsterFrost });

                // Retain every actor's identity/material/hierarchy even if a
                // later terrain survey or a different actor's skin proof fails.
                for (int i = 0; i < actors.Count; i++)
                    foreach (int frame in Cleanup2WaitAppearance(actors[i], keys[i])) yield return frame;
                _sprint25WaitingForUnityAppearance = true;
                for (int i = 0; i < actors.Count; i++)
                {
                    UnitEntityData actor = actors[i];
                    Cleanup2Check("pre-survey-actor-" + keys[i], actor.View != null, new JObject {
                        ["key"] = keys[i], ["unit"] = actor.UniqueId,
                        ["blueprint"] = actor.Blueprint.AssetGuid, ["prefab"] = actor.Blueprint.Prefab?.AssetId,
                        ["nativePosition"] = SurveyVector(actor.Position), ["viewInstance"] = actor.View.GetInstanceID(),
                        ["viewLocalScale"] = SurveyVector(actor.View.transform.localScale),
                        ["mechanicalSize"] = actor.Descriptor.State.Size.ToString(),
                        ["exactSummonProvenance"] = i == 0 ? (JToken)JValue.CreateNull() : new JValue(ExpandedSummoningPlayerPathUnitExact(actor, fixture.Caster)),
                        ["shapeHierarchy"] = Cleanup2ShapeHierarchy(actor),
                        ["renderers"] = new JArray(actor.View.GetComponentsInChildren<SkinnedMeshRenderer>(true).Select(skin => new JObject {
                            ["name"] = skin.name, ["path"] = Cleanup2TransformPath(actor.View.transform, skin.transform),
                            ["mesh"] = skin.sharedMesh?.name, ["rootBonePath"] = Cleanup2TransformPath(actor.View.transform, skin.rootBone),
                            ["enabled"] = skin.enabled, ["active"] = skin.gameObject.activeInHierarchy,
                            ["materials"] = new JArray(skin.sharedMaterials.Select(Cleanup2Material)) })),
                        ["scope"] = "Settled pre-placement diagnostics only; no comparable standing height or calibration claimed." },
                        "Every canonically created lineup actor retains safe identity, hierarchy and material observations before the ground survey.");
                }
                Vector3 origin = fixture.Caster.Position;
                Cleanup2GroundSurvey ground = Cleanup2LineupGround(origin, actors);
                List<Vector3> groundPoints = ground.Points;
                Vector3 rowAxis = groundPoints.Last() - groundPoints.First();
                rowAxis.y = 0f;
                rowAxis.Normalize();
                Vector3 cameraForward = Vector3.Cross(rowAxis, Vector3.up).normalized;
                for (int i = 0; i < actors.Count; i++)
                {
                    PlaceExpandedSummoningUnit(actors[i], groundPoints[i]);
                    actors[i].View.transform.rotation = Quaternion.LookRotation(-cameraForward, Vector3.up);
                }
                float groundSpread = groundPoints.Max(p => p.y) - groundPoints.Min(p => p.y);
                Cleanup2Check("common-ground", ground.Valid, new JObject {
                    ["origin"] = SurveyVector(groundPoints[0]), ["up"] = SurveyVector(Vector3.up),
                    ["right"] = SurveyVector(rowAxis), ["cameraForward"] = SurveyVector(cameraForward),
                    ["groundYSpreadMeters"] = groundSpread,
                    ["points"] = new JArray(groundPoints.Select(SurveyVector)),
                    ["provenTerrainPlane"] = ground.Document,
                    ["placement"] = "Native walkable graph, request-owned actors only; no model scale or calibration change." },
                    "All seven actors use one geometrically proven gently graded terrain plane with <=5cm residual; vertical anatomy remains world-up and contact is checked at each actor's own ground point.");
                for (int i = 0; i < actors.Count; i++)
                    foreach (int frame in Cleanup2WaitAppearance(actors[i], keys[i])) yield return frame;
                // Retain Unity idle samples while suspending synthetic game-time
                // pulses. Never force a renderer, fader, dissolve or animation.
                _sprint25WaitingForUnityAppearance = true;
                var samples = new List<Cleanup2ActorMetric[]>();
                for (int sample = 0; sample < 4; sample++)
                {
                    for (int frame = 0; frame < 12; frame++) yield return 0;
                    var measured = actors.Select((actor, i) => Cleanup2MeasureActor(actor,
                        keys[i], groundPoints[0], Vector3.up)).ToArray();
                    samples.Add(measured);
                    Cleanup2Check("lineup-sample-" + sample, measured.All(v => v.Valid),
                        new JObject { ["sample"] = sample, ["basisOrigin"] = SurveyVector(groundPoints[0]),
                            ["basisUp"] = SurveyVector(Vector3.up),
                            ["actors"] = new JArray(measured.Select(v => v.Document)) },
                        "Transient baked current-pose body vertices yield finite crown/sole metrics with a verified world transformation; no native vertices are exported.");
                }
                var summary = new JArray();
                bool standing = ground.Valid;
                for (int i = 0; i < actors.Count; i++)
                {
                    float[] heights = samples.Select(v => v[i].Height).ToArray();
                    float range = heights.Max() - heights.Min();
                    float soleGap = samples.Max(v => Mathf.Abs(v[i].Sole.y - groundPoints[i].y));
                    bool stableScale = samples.All(v => v[i].ViewId == samples[0][i].ViewId &&
                        Vector3.Distance(v[i].ViewScale, samples[0][i].ViewScale) <= .00001f);
                    bool stable = samples.All(v => v[i].Valid) && range <= .05f && soleGap <= .15f && stableScale;
                    standing &= stable;
                    summary.Add(new JObject { ["key"] = keys[i], ["unit"] = actors[i].UniqueId,
                        ["meanBodyHeightWorldUnits"] = heights.Average(), ["poseHeightRange"] = range,
                        ["maximumSoleGroundGap"] = soleGap, ["stableStandingSamples"] = stable,
                        ["sameViewScaleAcrossSamples"] = stableScale,
                        ["heightOverHuman"] = heights.Average() / samples.Average(v => v[0].Height),
                        ["comparisonFeetUsingExplicitHumanArtAnchor"] = 6f * heights.Average() / samples.Average(v => v[0].Height) });
                }
                Cleanup2Check("standing-comparison-eligibility", standing, new JObject {
                    ["actors"] = summary, ["calibrationApplied"] = false,
                    ["ratioLimitation"] = "Ratios/feet are descriptive only. An ineligible standing/ground sample cannot authorize scale calibration." },
                    "Four native idle samples have comparable standing contact/height before any later art calibration.");
                Cleanup2Check("same-camera-lineup", true,
                    Cleanup2CaptureLineup(actors, keys, samples.Last()),
                    "One live same-camera capture contains the verified Human and all six canonically summoned giant families.");
            }
            finally
            {
                _expandedSummoningPlayerPathCaptureActive = false;
                try { hooks.UnpatchAll(hooks.Id); }
                finally
                {
                    ExpandedSummoningPlayerPathEvents.Clear();
                    _sprint25WaitingForUnityAppearance = false;
                }
            }
        }

        private sealed class Cleanup2GroundSurvey
        {
            internal List<Vector3> Points;
            internal JObject Document;
            internal bool Valid;
        }

        private Cleanup2GroundSurvey Cleanup2LineupGround(Vector3 anchor, IList<UnitEntityData> actors)
        {
            if (AstarPath.active == null || !Cleanup2Finite(anchor))
                throw new InvalidOperationException("No finite native lineup ground anchor/graph.");
            var anchorNode = AstarPath.active.GetNearest(anchor);
            if (anchorNode.node == null || !anchorNode.node.Walkable || !Cleanup2Finite(anchorNode.clampedPosition))
                throw new InvalidOperationException("Lineup anchor has no finite walkable native graph node.");
            // Fit a genuinely two-dimensional surface, not an underdetermined
            // plane through seven collinear points. Each actor center plus
            // terrain one metre to both sides must lie on the same gentle
            // plane. Y variation from grade is distinct from plane residual.
            float[] minimumGaps = Enumerable.Range(1, actors.Count - 1).Select(i =>
                Mathf.Max(2.5f, actors[i - 1].Corpulence + actors[i].Corpulence + .35f)).ToArray();
            if (minimumGaps.Any(value => !Cleanup2Finite(value)))
                throw new InvalidOperationException("Non-finite native footprint spacing in lineup.");
            var attempts = new JArray();
            var refusals = new Dictionary<string, int>(StringComparer.Ordinal);
            var watch = System.Diagnostics.Stopwatch.StartNew();
            Cleanup2GroundSurvey selected = null;
            float[] offsets = { 0f, 6f, -6f, 12f, -12f, 18f, -18f };
            foreach (float spacingFactor in new[] { 1.35f, 1.15f, 1f })
            {
                var along = new float[actors.Count];
                for (int i = 1; i < along.Length; i++) along[i] = along[i - 1] + minimumGaps[i - 1] * spacingFactor;
                float span = along.Last();
                for (int direction = 0; direction < 12 && selected == null; direction++)
                {
                    Vector3 axis = Quaternion.Euler(0f, direction * 15f, 0f) * Vector3.right;
                    Vector3 across = Vector3.Cross(axis, Vector3.up).normalized;
                    foreach (float longitudinal in offsets)
                    foreach (float lateral in offsets)
                    {
                        if (selected != null) break;
                        if (attempts.Count >= 1764 || watch.Elapsed.TotalSeconds > 15)
                            goto SurveyFinished;
                        Vector3 center = anchor + axis * longitudinal + across * lateral;
                        var centers = new List<Vector3>();
                        var support = new List<Vector3>();
                        string refusal = null;
                        float maximumHorizontalClamp = 0f;
                        for (int actor = 0; actor < actors.Count && refusal == null; actor++)
                        foreach (float side in new[] { 0f, -1f, 1f })
                        {
                            Vector3 requested = center + axis * (along[actor] - span * .5f) + across * side;
                            var nearest = AstarPath.active.GetNearest(requested);
                            Vector3 point = nearest.clampedPosition;
                            if (nearest.node == null || !nearest.node.Walkable)
                                refusal = "missing-or-unwalkable-node";
                            else if (nearest.node.Area != anchorNode.node.Area || nearest.node.GraphIndex != anchorNode.node.GraphIndex)
                                refusal = "different-native-connected-graph";
                            else if (!Cleanup2Finite(point)) refusal = "non-finite-ground";
                            else
                            {
                                Vector3 clamp = point - requested;
                                float horizontal = new Vector2(clamp.x, clamp.z).magnitude;
                                maximumHorizontalClamp = Mathf.Max(maximumHorizontalClamp, horizontal);
                                if (horizontal > .35f) refusal = "horizontal-clamp-over-35cm";
                                else if (Mathf.Abs(point.y - anchor.y) > 2f) refusal = "terrain-over-2m-from-anchor-level";
                                else if (Vector3.Distance(point, anchor) > 40f) refusal = "terrain-outside-40m-search-radius";
                            }
                            if (refusal != null) break;
                            support.Add(point);
                            if (side == 0f) centers.Add(point);
                        }
                        if (refusal == null)
                            for (int i = 1; i < centers.Count; i++)
                            {
                                Vector3 separation = centers[i] - centers[i - 1]; separation.y = 0f;
                                if (separation.magnitude < minimumGaps[i - 1] - .05f)
                                { refusal = "insufficient-native-footprint-separation"; break; }
                            }
                        var row = new JObject { ["candidate"] = attempts.Count + 1,
                            ["center"] = SurveyVector(center), ["yawDegrees"] = direction * 15,
                            ["longitudinalOffset"] = longitudinal, ["lateralOffset"] = lateral,
                            ["spacingFactor"] = spacingFactor, ["requestedSpanMeters"] = span,
                            ["centerCount"] = centers.Count, ["terrainSupportCount"] = support.Count,
                            ["maximumHorizontalClamp"] = maximumHorizontalClamp };
                        JObject plane = null;
                        if (refusal == null)
                        {
                            plane = Cleanup2FitGroundPlane(support);
                            row["plane"] = plane;
                            if (!(bool)plane["finiteNondegenerate"]) refusal = "degenerate-or-nonfinite-plane";
                            else if ((float)plane["grade"] > .03f) refusal = "plane-grade-over-3-percent";
                            else if ((float)plane["maximumVerticalResidualMeters"] > .05f) refusal = "plane-residual-over-5cm";
                        }
                        row["refusal"] = refusal;
                        attempts.Add(row);
                        if (refusal == null)
                        {
                            selected = new Cleanup2GroundSurvey { Points = centers, Valid = true,
                                Document = new JObject { ["candidate"] = row, ["plane"] = plane,
                                    ["terrainSupportPoints"] = new JArray(support.Select(SurveyVector)),
                                    ["minimumAdjacentCenterGaps"] = new JArray(minimumGaps),
                                    ["actualAdjacentCenterGaps"] = new JArray(Enumerable.Range(1, centers.Count - 1).Select(i => {
                                        Vector3 delta = centers[i] - centers[i - 1]; delta.y = 0f; return delta.magnitude; })) } };
                        }
                        else { int count; refusals.TryGetValue(refusal, out count); refusals[refusal] = count + 1; }
                    }
                }
                if (selected != null) break;
            }
        SurveyFinished:
            var diagnostic = new JObject { ["anchor"] = SurveyVector(anchor),
                ["nativeAnchorGround"] = SurveyVector(anchorNode.clampedPosition),
                ["nativeGraphIndex"] = anchorNode.node.GraphIndex, ["nativeConnectedArea"] = anchorNode.node.Area,
                ["candidateCount"] = attempts.Count, ["wallSeconds"] = watch.Elapsed.TotalSeconds,
                ["candidateLimit"] = 1764, ["wallLimitSeconds"] = 15,
                ["selected"] = selected?.Document, ["attempts"] = attempts,
                ["refusals"] = JObject.FromObject(refusals),
                ["proof"] = "Every actor center and both lateral terrain samples are finite walkable clamped points in the anchor's same connected native graph; <=35cm horizontal clamp, <=3% fitted plane grade and <=5cm vertical residual. Requested Y is only a graph query hint. World-up anatomy and each actor's own ground contact remain independent checks. No actor moved during survey." };
            Cleanup2Check("lineup-ground-survey", selected != null, diagnostic,
                "A bounded courtyard search finds seven adequately separated native ground positions on a nondegenerate measured common plane.");
            if (selected == null)
                throw new InvalidOperationException("No proven native seven-actor terrain plane within the recorded bounded courtyard survey; no scale calibration is authorized.");
            return selected;
        }

        private static JObject Cleanup2FitGroundPlane(IList<Vector3> points)
        {
            Vector3 mean = points.Aggregate(Vector3.zero, (sum, point) => sum + point) / points.Count;
            double xx = 0, xz = 0, zz = 0, xy = 0, zy = 0;
            foreach (Vector3 point in points)
            {
                Vector3 delta = point - mean;
                xx += delta.x * (double)delta.x; xz += delta.x * (double)delta.z;
                zz += delta.z * (double)delta.z; xy += delta.x * (double)delta.y; zy += delta.z * (double)delta.y;
            }
            double determinant = xx * zz - xz * xz;
            if (determinant <= .0001 || double.IsNaN(determinant) || double.IsInfinity(determinant))
                return new JObject { ["finiteNondegenerate"] = false, ["determinant"] = determinant };
            float gradeX = (float)((xy * zz - zy * xz) / determinant);
            float gradeZ = (float)((zy * xx - xy * xz) / determinant);
            float grade = new Vector2(gradeX, gradeZ).magnitude;
            float residual = points.Max(point => Mathf.Abs(point.y - mean.y -
                gradeX * (point.x - mean.x) - gradeZ * (point.z - mean.z)));
            return new JObject { ["finiteNondegenerate"] = Cleanup2Finite(mean) && Cleanup2Finite(grade) && Cleanup2Finite(residual),
                ["origin"] = SurveyVector(mean), ["worldXGrade"] = gradeX, ["worldZGrade"] = gradeZ,
                ["grade"] = grade, ["slopeDegrees"] = Mathf.Atan(grade) * Mathf.Rad2Deg,
                ["normal"] = SurveyVector(new Vector3(-gradeX, 1f, -gradeZ).normalized),
                ["maximumVerticalResidualMeters"] = residual, ["supportCount"] = points.Count,
                ["determinant"] = determinant };
        }

        private IEnumerable<int> Cleanup2WaitAppearance(UnitEntityData unit, string key)
        {
            DateTime deadline = DateTime.UtcNow.AddSeconds(12);
            bool before = _sprint25WaitingForUnityAppearance, ready = false;
            float dissolve = 1f;
            int frames = 0;
            try
            {
                while (DateTime.UtcNow < deadline && frames++ < 1800)
                {
                    bool appearAbsent = unit.Descriptor.Buffs.GetBuff(BlueprintRoot.Instance.SystemMechanics.SummonedUnitAppearBuff) == null;
                    dissolve = DissolveAmount(unit);
                    ready = unit.View != null && EntityFadedIn(unit) && appearAbsent &&
                        unit.Descriptor.State.CanAct && unit.Descriptor.State.CanMove &&
                        dissolve >= 0f && dissolve <= .02f &&
                        unit.View.GetComponentsInChildren<SkinnedMeshRenderer>(true).Any(Cleanup2BodyRenderer);
                    if (ready) break;
                    _sprint25WaitingForUnityAppearance = appearAbsent && EntityFadedIn(unit);
                    Game.Instance.IsPaused = false;
                    yield return 0;
                }
            }
            finally { _sprint25WaitingForUnityAppearance = before; }
            Cleanup2Check("settled-" + key, ready, new JObject { ["key"] = key,
                ["unit"] = unit.UniqueId, ["frames"] = frames, ["dissolve"] = dissolve,
                ["fadedIn"] = EntityFadedIn(unit), ["modelStateForced"] = false },
                "Native spawn buff/fade/dissolve settles on a real enabled body without shader/opacity overrides.");
            if (!ready) throw new InvalidOperationException("Cleanup 2 native appearance not settled: " + key);
        }

        private sealed class Cleanup2ActorMetric
        {
            internal JObject Document;
            internal Vector3 Crown, Sole, Min, Max;
            internal Vector3 ViewScale;
            internal int ViewId;
            internal float Height;
            internal bool Valid;
        }

        private static bool Cleanup2AnatomyBone(string name)
        {
            string n = (name ?? "").ToLowerInvariant();
            return n.Contains("head") || n.Contains("pelvis") || n.Contains("spine") ||
                n.Contains("neck") || n.Contains("foot") || n.Contains("toe") || n.Contains("torso");
        }

        private static bool Cleanup2BodyRenderer(SkinnedMeshRenderer skin)
        {
            if (skin == null || !skin.enabled || !skin.gameObject.activeInHierarchy ||
                skin.sharedMesh == null || skin.bones == null) return false;
            Transform[] bones = skin.bones;
            Func<int, float, bool> anatomy = (index, weight) => weight > .001f &&
                index >= 0 && index < bones.Length && bones[index] != null && Cleanup2AnatomyBone(bones[index].name);
            // Equipment can carry a full donor bone palette while using only
            // Hand weights. Inspect positive skin weights, not palette names.
            return skin.sharedMesh.boneWeights.Any(w => anatomy(w.boneIndex0, w.weight0) ||
                anatomy(w.boneIndex1, w.weight1) || anatomy(w.boneIndex2, w.weight2) || anatomy(w.boneIndex3, w.weight3));
        }

        private static JArray Cleanup2Matrix(Matrix4x4 matrix)
        {
            var result = new JArray();
            for (int row = 0; row < 4; row++) for (int col = 0; col < 4; col++) result.Add(matrix[row, col]);
            return result;
        }

        private static bool Cleanup2Finite(Vector3 p)
        { return !(float.IsNaN(p.x) || float.IsNaN(p.y) || float.IsNaN(p.z) || float.IsInfinity(p.x) || float.IsInfinity(p.y) || float.IsInfinity(p.z)); }

        private static bool Cleanup2Finite(float value)
        { return !float.IsNaN(value) && !float.IsInfinity(value); }

        private static bool Cleanup2Finite(Matrix4x4 matrix)
        {
            for (int row = 0; row < 4; row++) for (int col = 0; col < 4; col++)
                if (!Cleanup2Finite(matrix[row, col])) return false;
            return true;
        }

        private static string Cleanup2TransformPath(Transform root, Transform current)
        {
            if (current == null) return null;
            var parts = new List<string>();
            for (Transform t = current; t != null && t != root; t = t.parent)
            {
                parts.Add(t.name + "[" + t.GetSiblingIndex() + "]");
                if (t.parent == null) { parts.Add("<outside-view>"); break; }
            }
            parts.Reverse();
            return current == root ? "." : string.Join("/", parts.ToArray());
        }

        private static JObject Cleanup2ShapeHierarchy(UnitEntityData unit)
        {
            Transform root = unit.View.transform;
            Transform[] transforms = unit.View.GetComponentsInChildren<Transform>(true);
            var nodes = new JArray(transforms.Take(4096).Select(t => new JObject {
                ["path"] = Cleanup2TransformPath(root, t),
                ["parent"] = t == root ? null : Cleanup2TransformPath(root, t.parent),
                ["instance"] = t.GetInstanceID(), ["active"] = t.gameObject.activeInHierarchy,
                ["localPosition"] = SurveyVector(t.localPosition),
                ["localRotation"] = new JArray(t.localRotation.x, t.localRotation.y, t.localRotation.z, t.localRotation.w),
                ["localScale"] = SurveyVector(t.localScale), ["lossyScale"] = SurveyVector(t.lossyScale),
                ["components"] = new JArray(t.GetComponents<Component>().Where(c => c != null && !(c is Transform))
                    .Select(c => c.GetType().FullName)) }));
            var colliders = new JArray();
            foreach (Collider collider in unit.View.GetComponentsInChildren<Collider>(true))
            {
                var row = new JObject { ["path"] = Cleanup2TransformPath(root, collider.transform),
                    ["instance"] = collider.GetInstanceID(), ["type"] = collider.GetType().FullName,
                    ["enabled"] = collider.enabled, ["active"] = collider.gameObject.activeInHierarchy,
                    ["isTrigger"] = collider.isTrigger, ["layer"] = collider.gameObject.layer,
                    ["localScale"] = SurveyVector(collider.transform.localScale),
                    ["lossyScale"] = SurveyVector(collider.transform.lossyScale),
                    ["worldPosition"] = SurveyVector(collider.transform.position),
                    ["boundsCenter"] = SurveyVector(collider.bounds.center),
                    ["boundsSize"] = SurveyVector(collider.bounds.size) };
                var capsule = collider as CapsuleCollider;
                if (capsule != null) { row["center"] = SurveyVector(capsule.center); row["radius"] = capsule.radius;
                    row["height"] = capsule.height; row["direction"] = capsule.direction; }
                var box = collider as BoxCollider;
                if (box != null) { row["center"] = SurveyVector(box.center); row["size"] = SurveyVector(box.size); }
                var sphere = collider as SphereCollider;
                if (sphere != null) { row["center"] = SurveyVector(sphere.center); row["radius"] = sphere.radius; }
                var mesh = collider as MeshCollider;
                if (mesh != null) { row["mesh"] = mesh.sharedMesh?.name; row["convex"] = mesh.convex; }
                colliders.Add(row);
            }
            return new JObject { ["viewCorpulence"] = unit.View.Corpulence,
                ["nativeSizeScale"] = unit.View.GetSizeScale(),
                ["movementAgentCorpulence"] = unit.View.MovementAgent == null ? (JToken)JValue.CreateNull() : new JValue(unit.View.MovementAgent.Corpulence),
                ["transformCount"] = transforms.Length, ["complete"] = transforms.Length <= 4096,
                ["nodes"] = nodes, ["colliders"] = colliders,
                ["scope"] = "Read-only transform/component and collider scalar diagnostics. Collider bounds describe selection/physics shapes only and are never used as anatomy height. No visual subtree or collider is mutated." };
        }

        private static JToken Cleanup2Material(Material material)
        {
            if (material == null) return JValue.CreateNull();
            var properties = new JObject();
            foreach (string name in new[] { "_Dissolve", "_DissolveAmount", "_ZWrite", "_CullMode", "_SrcBlend", "_DstBlend", "_Mode", "_Surface" })
                if (material.HasProperty(name)) properties[name] = material.GetFloat(name);
            var colors = new JObject();
            foreach (string name in new[] { "_Color", "_TintColor", "_BaseColor", "_MainColor" })
            {
                if (!material.HasProperty(name)) { colors[name] = JValue.CreateNull(); continue; }
                Color color = material.GetColor(name);
                colors[name] = new JArray(color.r, color.g, color.b, color.a);
            }
            var textures = new JObject();
            foreach (string name in new[] { "_MainTex", "_BumpMap", "_Parameters" })
            {
                if (!material.HasProperty(name)) { textures[name] = JValue.CreateNull(); continue; }
                Texture value = material.GetTexture(name);
                textures[name] = value == null ? (JToken)JValue.CreateNull() : new JObject {
                    ["name"] = value.name, ["instance"] = value.GetInstanceID(),
                    ["width"] = value.width, ["height"] = value.height,
                    ["type"] = value.GetType().FullName,
                    ["scale"] = new JArray(material.GetTextureScale(name).x, material.GetTextureScale(name).y),
                    ["offset"] = new JArray(material.GetTextureOffset(name).x, material.GetTextureOffset(name).y) };
            }
            Texture texture = material.HasProperty("_MainTex") ? material.GetTexture("_MainTex") : null;
            return new JObject { ["name"] = material.name, ["instance"] = material.GetInstanceID(),
                ["shader"] = material.shader?.name, ["renderQueue"] = material.renderQueue,
                ["properties"] = properties, ["colors"] = colors, ["textures"] = textures,
                ["color"] = material.HasProperty("_Color") ? new JArray(material.color.r, material.color.g, material.color.b, material.color.a) : null,
                ["texture"] = texture?.name, ["textureInstance"] = texture == null ? 0 : texture.GetInstanceID() };
        }

        private static void Cleanup2ValidateSkinInfluence(int vertex, int slot, int index, float weight,
            Transform root, Transform[] bones, Matrix4x4[] binds, Dictionary<int, Matrix4x4> matrices, JObject diagnostics)
        {
            string reason = null;
            if (!Cleanup2Finite(weight) || weight < 0f) reason = "Non-finite or negative influence weight.";
            else if (weight == 0f) return; // Unused palette indices/null bones have no skinning contribution.
            else if (index < 0 || index >= bones.Length) reason = "Positive influence has no bone palette entry.";
            else if (index >= binds.Length) reason = "Positive influence has no bind pose.";
            else if (bones[index] == null) reason = "Positive influence references a null bone.";
            else if (!matrices.ContainsKey(index))
            {
                Matrix4x4 matrix = bones[index].localToWorldMatrix * binds[index];
                if (!Cleanup2Finite(binds[index]) || !Cleanup2Finite(matrix))
                    reason = "Positive influence has a non-finite bind or world skin matrix.";
                else
                {
                    matrices.Add(index, matrix);
                    ((JArray)diagnostics["positiveUsedBones"]).Add(new JObject {
                        ["paletteIndex"] = index, ["name"] = bones[index].name,
                        ["path"] = Cleanup2TransformPath(root, bones[index]) });
                }
            }
            if (reason == null) return;
            // Ordinals identify the rejected input; no vertex coordinates,
            // triangle indices or individual influence weights are exported.
            diagnostics["rejectedVertexOrdinal"] = vertex;
            diagnostics["rejectedInfluenceSlot"] = slot;
            diagnostics["rejectedPaletteIndex"] = index;
            diagnostics["rejection"] = reason;
            throw new InvalidOperationException(reason + " Vertex " + vertex + ", slot " + slot + ", palette index " + index + ".");
        }

        private static Vector3 Cleanup2SkinWorldPoint(Vector3 point, BoneWeight weight, Dictionary<int, Matrix4x4> matrices)
        {
            Vector3 result = Vector3.zero;
            if (weight.weight0 > 0f) result += matrices[weight.boneIndex0].MultiplyPoint3x4(point) * weight.weight0;
            if (weight.weight1 > 0f) result += matrices[weight.boneIndex1].MultiplyPoint3x4(point) * weight.weight1;
            if (weight.weight2 > 0f) result += matrices[weight.boneIndex2].MultiplyPoint3x4(point) * weight.weight2;
            if (weight.weight3 > 0f) result += matrices[weight.boneIndex3].MultiplyPoint3x4(point) * weight.weight3;
            return result;
        }

        private Cleanup2ActorMetric Cleanup2MeasureActor(UnitEntityData unit, string key, Vector3 origin, Vector3 up)
        {
            var result = new Cleanup2ActorMetric { Valid = true, Min = Vector3.one * float.MaxValue,
                Max = Vector3.one * float.MinValue, ViewId = unit.View.GetInstanceID(),
                ViewScale = unit.View.transform.localScale };
            float min = float.MaxValue, max = float.MinValue;
            var skins = unit.View.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            var renderers = new JArray();
            int measuredCount = 0;
            foreach (var skin in skins)
            {
                bool body = Cleanup2BodyRenderer(skin);
                var row = new JObject { ["name"] = skin.name, ["mesh"] = skin.sharedMesh?.name,
                    ["path"] = Cleanup2TransformPath(unit.View.transform, skin.transform),
                    ["rootBonePath"] = Cleanup2TransformPath(unit.View.transform, skin.rootBone),
                    ["enabled"] = skin.enabled, ["active"] = skin.gameObject.activeInHierarchy,
                    ["includedAsBody"] = body,
                    ["classification"] = "Active skinned surface with positive head/neck/torso/spine/pelvis/foot/toe weights; merely carrying those bones in a palette is insufficient. Hand-only weapons and unskinned effects excluded; all choices recorded.",
                    ["rendererLocalToWorld"] = Cleanup2Matrix(skin.transform.localToWorldMatrix),
                    ["rendererToView"] = Cleanup2Matrix(unit.View.transform.worldToLocalMatrix * skin.transform.localToWorldMatrix),
                    ["rendererToViewPosition"] = SurveyVector(unit.View.transform.InverseTransformPoint(skin.transform.position)),
                    ["rendererLossyScale"] = SurveyVector(skin.transform.lossyScale),
                    ["materials"] = new JArray(skin.sharedMaterials.Select(Cleanup2Material)) };
                renderers.Add(row);
                if (!body) continue;
                Mesh baked = null;
                try
                {
                    Vector3[] source = skin.sharedMesh.vertices ?? new Vector3[0];
                    BoneWeight[] weights = skin.sharedMesh.boneWeights ?? new BoneWeight[0];
                    Matrix4x4[] binds = skin.sharedMesh.bindposes ?? new Matrix4x4[0];
                    Transform[] bones = skin.bones ?? new Transform[0];
                    var skinData = new JObject { ["sourceVertexCount"] = source.Length,
                        ["bakedVertexCount"] = JValue.CreateNull(), ["weightRecordCount"] = weights.Length,
                        ["bonePaletteCount"] = bones.Length, ["bindPoseCount"] = binds.Length,
                        ["nullPaletteEntryCount"] = bones.Count(b => b == null),
                        ["positiveUsedBones"] = new JArray(),
                        ["validation"] = "Every finite nonnegative influence is checked; strictly positive influences require an existing nonnull bone, bind pose and finite world matrix. Zero-weight palette entries are never indexed. Vertex influence sums must be within 0.005 of one." };
                    row["skinData"] = skinData;
                    baked = new Mesh();
                    skin.BakeMesh(baked);
                    Vector3[] bakedPoints = baked.vertices ?? new Vector3[0];
                    skinData["bakedVertexCount"] = bakedPoints.Length;
                    if (bakedPoints.Length == 0 || source.Length != bakedPoints.Length || weights.Length != source.Length)
                    {
                        skinData["rejection"] = "Source/baked vertex or weight-record count mismatch/empty body.";
                        throw new InvalidOperationException("Incomplete native body skin records: " + skin.name);
                    }
                    var matrices = new Dictionary<int, Matrix4x4>();
                    for (int i = 0; i < source.Length; i++)
                    {
                        BoneWeight w = weights[i];
                        Cleanup2ValidateSkinInfluence(i, 0, w.boneIndex0, w.weight0, unit.View.transform, bones, binds, matrices, skinData);
                        Cleanup2ValidateSkinInfluence(i, 1, w.boneIndex1, w.weight1, unit.View.transform, bones, binds, matrices, skinData);
                        Cleanup2ValidateSkinInfluence(i, 2, w.boneIndex2, w.weight2, unit.View.transform, bones, binds, matrices, skinData);
                        Cleanup2ValidateSkinInfluence(i, 3, w.boneIndex3, w.weight3, unit.View.transform, bones, binds, matrices, skinData);
                        float sum = w.weight0 + w.weight1 + w.weight2 + w.weight3;
                        if (!Cleanup2Finite(source[i]) || !Cleanup2Finite(bakedPoints[i]) ||
                            !Cleanup2Finite(sum) || Mathf.Abs(sum - 1f) > .005f)
                        {
                            skinData["rejectedVertexOrdinal"] = i;
                            skinData["rejection"] = "Non-finite source/baked point or vertex influence sum outside the recorded normalization tolerance.";
                            throw new InvalidOperationException("Invalid native body vertex/influence normalization: " + skin.name + ", vertex " + i);
                        }
                    }
                    skinData["positiveUsedBoneCount"] = matrices.Count;
                    skinData["unusedNullPaletteEntryCount"] = bones.Count(b => b == null);
                    skinData["validated"] = true;
                    float transformedError = 0f, rotationOnlyError = 0f;
                    // Determine this engine's BakeMesh scale convention from
                    // weighted bone/bindpose skinning, never from culling bounds.
                    for (int i = 0; i < source.Length; i++)
                    {
                        Vector3 exact = Cleanup2SkinWorldPoint(source[i], weights[i], matrices);
                        Vector3 transformed = skin.transform.TransformPoint(bakedPoints[i]);
                        Vector3 rotated = skin.transform.position + skin.transform.rotation * bakedPoints[i];
                        if (!Cleanup2Finite(exact) || !Cleanup2Finite(transformed) || !Cleanup2Finite(rotated))
                        {
                            skinData["rejectedVertexOrdinal"] = i;
                            skinData["rejection"] = "Non-finite evaluated world skin/bake point.";
                            throw new InvalidOperationException("Non-finite evaluated native skin: " + skin.name + ", vertex " + i);
                        }
                        transformedError = Mathf.Max(transformedError, Vector3.Distance(exact, transformed));
                        rotationOnlyError = Mathf.Max(rotationOnlyError, Vector3.Distance(exact, rotated));
                    }
                    bool transform = transformedError <= rotationOnlyError;
                    float error = Mathf.Min(transformedError, rotationOnlyError);
                    row["bakeScaleConvention"] = transform ? "renderer.TransformPoint(bakedVertex)" : "renderer.position + renderer.rotation * bakedVertex (BakeMesh already includes scale)";
                    row["maxErrorVsWeightedBoneSkinWorld"] = error;
                    row["transformPointError"] = transformedError;
                    row["rotationOnlyError"] = rotationOnlyError;
                    row["vertexCount"] = source.Length;
                    result.Valid &= error <= .005f;
                    foreach (Vector3 p in bakedPoints)
                    {
                        Vector3 world = transform ? skin.transform.TransformPoint(p) : skin.transform.position + skin.transform.rotation * p;
                        result.Valid &= Cleanup2Finite(world);
                        float h = Vector3.Dot(world - origin, up);
                        if (h < min) { min = h; result.Sole = world; }
                        if (h > max) { max = h; result.Crown = world; }
                        result.Min = Vector3.Min(result.Min, world);
                        result.Max = Vector3.Max(result.Max, world);
                    }
                    measuredCount++;
                }
                catch (Exception ex)
                {
                    row["measurementException"] = ex.GetType().FullName + ": " + ex.Message;
                    Cleanup2Check("skin-measurement-rejected-" + key, false, new JObject {
                        ["key"] = key, ["unit"] = unit.UniqueId, ["blueprint"] = unit.Blueprint.AssetGuid,
                        ["viewInstance"] = result.ViewId, ["renderersReadBeforeRejection"] = renderers,
                        ["shapeHierarchy"] = Cleanup2ShapeHierarchy(unit), ["calibrationApplied"] = false },
                        "All positively weighted body influences have complete finite native skin data; rejected input counts and cause are retained before fixture teardown.");
                    throw;
                }
                finally { if (baked != null) UnityEngine.Object.Destroy(baked); }
            }
            result.Height = max - min;
            result.Valid &= measuredCount > 0 && result.Height > .1f && Cleanup2Finite(result.Crown) && Cleanup2Finite(result.Sole);
            var anchorTransforms = unit.View.GetComponentsInChildren<Transform>(true).Where(t => {
                string n = t.name.ToLowerInvariant(); return n.Contains("head") || n.Contains("foot") || n.Contains("toe");
            }).ToArray();
            result.Valid &= anchorTransforms.Any(t => t.name.ToLowerInvariant().Contains("head")) &&
                anchorTransforms.Any(t => t.name.ToLowerInvariant().Contains("foot") || t.name.ToLowerInvariant().Contains("toe"));
            var anchors = anchorTransforms.Select(t => new JObject { ["name"] = t.name, ["world"] = SurveyVector(t.position),
                ["view"] = SurveyVector(unit.View.transform.InverseTransformPoint(t.position)) });
            var lifecycle = unit.Descriptor.Buffs.RawFacts.OfType<Buff>().FirstOrDefault(b =>
                b.Blueprint == BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff);
            result.Document = new JObject { ["key"] = key, ["unit"] = unit.UniqueId,
                ["viewInstance"] = result.ViewId,
                ["blueprint"] = unit.Blueprint.AssetGuid, ["prefab"] = unit.Blueprint.Prefab?.AssetId,
                ["mechanicalSizeUnchanged"] = unit.Descriptor.State.Size.ToString(),
                ["nativePosition"] = SurveyVector(unit.Position), ["viewPosition"] = SurveyVector(unit.View.transform.position),
                ["viewLocalScale"] = SurveyVector(unit.View.transform.localScale), ["viewLossyScale"] = SurveyVector(unit.View.transform.lossyScale),
                ["viewLocalToWorld"] = Cleanup2Matrix(unit.View.transform.localToWorldMatrix),
                ["bodyCrownWorld"] = SurveyVector(result.Crown), ["bodySoleWorld"] = SurveyVector(result.Sole),
                ["bodyHeightWorldUnits"] = result.Height, ["valid"] = result.Valid,
                ["headFeetBonePoints"] = new JArray(anchors), ["renderers"] = renderers,
                ["shapeHierarchy"] = Cleanup2ShapeHierarchy(unit),
                ["summoner"] = lifecycle?.MaybeContext?.MaybeCaster?.UniqueId,
                ["summonRemainingSeconds"] = lifecycle == null ? (JToken)JValue.CreateNull() : new JValue(lifecycle.TimeLeft.TotalSeconds),
                ["dissolve"] = DissolveAmount(unit), ["fadedIn"] = EntityFadedIn(unit),
                ["measurement"] = "All included current-pose SkinnedMeshRenderer.BakeMesh vertices in a recorded common world-up basis. Temporary meshes destroyed; no proprietary vertex/index/weight data exported. Culling bounds are not anatomy height." };
            return result;
        }

        private JObject Cleanup2CaptureLineup(IList<UnitEntityData> actors, IList<string> keys, Cleanup2ActorMetric[] metrics)
        {
            Camera camera = TeleportationCastingCamera()?.Camera ?? Camera.main;
            if (camera == null) throw new InvalidOperationException("No native lineup camera.");
            Vector3 beforePosition = camera.transform.position;
            Quaternion beforeRotation = camera.transform.rotation;
            RenderTexture beforeTarget = camera.targetTexture, beforeActive = RenderTexture.active;
            RenderTexture target = null;
            Texture2D image = null;
            try
            {
                Vector3 min = metrics.Select(v => v.Min).Aggregate(Vector3.Min);
                Vector3 max = metrics.Select(v => v.Max).Aggregate(Vector3.Max);
                Vector3 centre = (min + max) * .5f;
                Vector3 rowAxis = (actors.Last().Position - actors.First().Position).normalized;
                Vector3 forward = Vector3.Cross(rowAxis, Vector3.up).normalized;
                var corners = metrics.SelectMany(m => new[] {
                    new Vector3(m.Min.x, m.Min.y, m.Min.z), new Vector3(m.Max.x, m.Min.y, m.Min.z),
                    new Vector3(m.Min.x, m.Min.y, m.Max.z), new Vector3(m.Max.x, m.Min.y, m.Max.z) }).ToArray();
                float span = corners.Max(p => Vector3.Dot(p, rowAxis)) - corners.Min(p => Vector3.Dot(p, rowAxis));
                float distance = Mathf.Max(12f, span / (2f * Mathf.Tan(camera.fieldOfView * Mathf.Deg2Rad * .5f) * (1920f / 1080f))) * 1.3f;
                camera.transform.position = centre - forward * distance + Vector3.up * Mathf.Max(2f, (max.y - min.y) * .22f);
                camera.transform.rotation = Quaternion.LookRotation(centre - camera.transform.position, Vector3.up);
                target = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
                camera.targetTexture = target;
                camera.Render();
                var viewport = new JArray(metrics.Select((m, i) => new JObject {
                    ["key"] = keys[i], ["unit"] = actors[i].UniqueId,
                    ["crown"] = SurveyVector(camera.WorldToViewportPoint(m.Crown)),
                    ["sole"] = SurveyVector(camera.WorldToViewportPoint(m.Sole)) }));
                bool visible = metrics.All(m => new[] { m.Crown, m.Sole }.All(p => {
                    Vector3 v = camera.WorldToViewportPoint(p); return v.z > 0 && v.x > .01f && v.x < .99f && v.y > .01f && v.y < .99f;
                }));
                RenderTexture.active = target;
                image = new Texture2D(1920, 1080, TextureFormat.RGBA32, false, false);
                image.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
                image.Apply(false, false);
                byte[] bytes = EncodeExpandedSummoningPng(image);
                Color32[] pixels = image.GetPixels32();
                double luma = pixels.Where((p, i) => i % 997 == 0).Average(p => .2126 * p.r + .7152 * p.g + .0722 * p.b);
                float screenFade = ScreenFadeAlpha();
                if (bytes == null || bytes.Length < 4096 || !visible || luma <= 4d || screenFade > .02f)
                    throw new InvalidOperationException("Native lineup frame is empty or clips an actor's crown/sole.");
                string file = "cleanup2-six-giants-human-lineup.png";
                File.WriteAllBytes(Path.Combine(_request.EvidenceDirectory, file), bytes);
                return new JObject { ["file"] = file, ["bytes"] = bytes.Length,
                    ["cameraPosition"] = SurveyVector(camera.transform.position),
                    ["cameraRotation"] = new JArray(camera.transform.rotation.x, camera.transform.rotation.y, camera.transform.rotation.z, camera.transform.rotation.w),
                    ["fieldOfView"] = camera.fieldOfView, ["orthographic"] = camera.orthographic,
                    ["screenFade"] = screenFade, ["meanLuma"] = luma,
                    ["allCrownSoleInFrame"] = visible, ["viewport"] = viewport,
                    ["rendering"] = "Actual live actors, native materials and scene lighting; no proxy meshes, recoloring or synthetic silhouettes." };
            }
            finally
            {
                camera.targetTexture = beforeTarget;
                camera.transform.position = beforePosition;
                camera.transform.rotation = beforeRotation;
                RenderTexture.active = beforeActive;
                if (target != null) { target.Release(); UnityEngine.Object.Destroy(target); }
                if (image != null) UnityEngine.Object.Destroy(image);
            }
        }

        private IEnumerable<int> Cleanup2Menus(ExpandedSummoningCorrectionFixture fixture, Spellbook book)
        {
            var player = Game.Instance.Player;
            var ui = Game.Instance.UI;
            if (ActionBarManager.Instance == null || ui?.SelectionManagerPC == null || GroupController.Instance == null)
                throw new InvalidOperationException("Native action-bar/party selection UI is unavailable.");
            UnitEntityData[] party = player.Party.ToArray();
            UnitEntityData[] selected = ui.SelectionManagerPC.SelectedUnits.ToArray();
            UnitEntityData groupCharacter = GroupController.Instance.GetCurrentCharacter();
            var groups = ActionBarManager.Instance.Group.GroupElements.ToArray();
            bool[] toggles = groups.Select(g => g.ToggleState).ToArray();
            if (toggles.Any(value => value) || Game.Instance.SelectedAbilityHandler.Ability != null)
                throw new InvalidOperationException("Cleanup 2 requires idle closed native groups and no selected ability before claiming UI ownership.");
            var slotsBefore = party.SelectMany(u => u.Descriptor.Spellbooks).SelectMany(b =>
                Enumerable.Range(0, 10).Select(level => new Cleanup2SlotSnapshot(b, level))).ToArray();
            bool added = false;
            using (var screens = new NativeIconScreenEvidence(_request))
            try
            {
                if (!player.Party.Contains(fixture.Caster))
                {
                    player.PartyCharacters.Add(fixture.Caster);
                    added = true;
                    player.InvalidateCharacterLists(); player.UpdateCharacterLists();
                }
                ui.SelectionManagerPC.SelectUnit(fixture.Caster.View, true, true, false);
                GroupController.Instance.SelectUnit(fixture.Caster);
                var parents = new List<AbilityData>();
                for (int tier = 1; tier <= 9; tier++)
                {
                    string guid = ExpandedSummoningInventoryObserver.CanonicalParentGuids[9 + tier - 1];
                    BlueprintAbility parent = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library, guid, "canonical SNA menu parent");
                    parents.Add(PrepareExpandedSummoningPlayerPathSpell(book, parent, parent, tier));
                }
                for (int frame = 0; frame < 20; frame++) yield return 0;
                var spellGroup = groups.Single(g => (ActionBarSlotType)typeof(ActionBarGroupElement)
                    .GetField("SlotType", Cleanup2Fields).GetValue(g) == ActionBarSlotType.Spell);
                for (int tier = 1; tier <= 9; tier++)
                {
                    AbilityData parent = parents[tier - 1];
                    if (!spellGroup.ToggleState) spellGroup.OnClick();
                    for (int frame = 0; frame < 20; frame++) yield return 0;
                    var levels = (ActionBarSubGroupLevels)typeof(ActionBarGroupElement).GetField("m_Levels", Cleanup2Fields).GetValue(spellGroup);
                    if (levels == null) throw new InvalidOperationException("Native spell-level tabs missing.");
                    levels.OnClick(tier);
                    for (int frame = 0; frame < 20; frame++) yield return 0;
                    var sources = spellGroup.GetComponentsInChildren<ActionBarGroupSlot>(true).Where(row => {
                        var data = row.MechanicSlot?.GetContentData() as AbilityData;
                        return row.gameObject.activeInHierarchy && data?.Blueprint == parent.Blueprint && ReferenceEquals(data.Spellbook, book);
                    }).ToArray();
                    if (sources.Length != 1) throw new InvalidOperationException("Canonical SNA " + tier + " native spell slot count=" + sources.Length);
                    ActionBarGroupSlot source = sources[0];
                    try
                    {
                        source.OnToggleGroupClick();
                        for (int frame = 0; frame < 20; frame++) yield return 0;
                        foreach (int frame in Cleanup2CapturePopup(source, parent.Blueprint,
                            "sna" + tier, screens, new HashSet<string>())) yield return frame;
                    }
                    finally { source.HideSubGroup(); }
                }
                Cleanup2Check("nine-sna-ui-tiers", true, new JObject { ["tiers"] = 9,
                    ["surface"] = "Actual native spell group, level tabs, canonical parent variant popups, all scroll regions and reachable nested variant groups.",
                    ["screens"] = "native-ui-screens.json",
                    ["remainingCoverage"] = "Druid spontaneous conversion, alternate adapters, old learned/prepared/hotbar Elemental policy and module-off startup are distinct scenarios; this phase does not claim them." },
                    "Every canonical SNA I-IX native variant popup and reachable page has complete row coverage and framebuffer evidence.");
            }
            finally
            {
                var failures = new List<Exception>();
                Action<Action> restore = action => { try { action(); } catch (Exception error) { failures.Add(error); } };
                restore(() => Game.Instance.ClickEventsController.ClearPointerMode());
                restore(() => Game.Instance.SelectedAbilityHandler.DropAbility());
                restore(() => { if (added) player.PartyCharacters.Remove(fixture.Caster); });
                restore(() => { player.InvalidateCharacterLists(); player.UpdateCharacterLists(); });
                restore(() => GroupController.Instance.SelectUnit(groupCharacter));
                restore(() => ui.SelectionManagerPC.MultiSelect(selected.Where(u => u?.View != null).Select(u => u.View).ToArray(), false));
                for (int i = 0; i < groups.Length; i++) { int index = i; restore(() => groups[index].Toggle(toggles[index], true)); }
                bool slotsExact = false;
                restore(() => slotsExact = slotsBefore.All(snapshot => snapshot.Unchanged()));
                Cleanup2Check("ui-party-restored", failures.Count == 0 && player.Party.SequenceEqual(party) && slotsExact,
                    new JObject { ["partyExact"] = player.Party.SequenceEqual(party),
                        ["originalMemorizedSlotReferencesAndAvailabilityExact"] = slotsExact,
                        ["originalMemorizedSlotCount"] = slotsBefore.Sum(snapshot => snapshot.Slots.Length),
                        ["restorationErrors"] = new JArray(failures.Select(error => error.ToString())),
                        ["fixtureSlotsPolicy"] = "Only the disposable caster's new native spellbook/slots are prepared; original party spellbooks and saved hotbar references are not edited." },
                    "Temporary party/selection/group ownership is restored and only request-owned spell slots were used.");
                if (failures.Count > 0) throw new AggregateException("Cleanup 2 native UI restoration failed.", failures);
            }
        }

        private sealed class Cleanup2SlotSnapshot
        {
            internal readonly Spellbook Book;
            internal readonly int Level;
            internal readonly SpellSlot[] Slots;
            private readonly bool[] _available;
            private readonly AbilityData[] _spells;
            internal Cleanup2SlotSnapshot(Spellbook book, int level)
            {
                Book = book; Level = level;
                Slots = book.GetMemorizedSpellSlots(level).ToArray();
                _available = Slots.Select(slot => slot != null && slot.Available).ToArray();
                _spells = Slots.Select(slot => slot?.Spell).ToArray();
            }
            internal bool Unchanged()
            {
                var current = Book.GetMemorizedSpellSlots(Level).ToArray();
                return current.Length == Slots.Length && current.Select((slot, i) =>
                    ReferenceEquals(slot, Slots[i]) && (slot == null ||
                    (slot.Available == _available[i] && ReferenceEquals(slot.Spell, _spells[i])))).All(value => value);
            }
        }

        private IEnumerable<int> Cleanup2CapturePopup(ActionBarGroupSlot source, BlueprintAbility parent,
            string label, NativeIconScreenEvidence screens, HashSet<string> lineage)
        {
            if (!lineage.Add(parent.AssetGuid)) throw new InvalidOperationException("Cyclic native variant menu: " + parent.AssetGuid);
            var popup = (ActionBarSpellsGroup)typeof(ActionBarGroupSlot).GetField("SubGroup", Cleanup2Fields).GetValue(source);
            if (popup == null || !popup.gameObject.activeInHierarchy) throw new InvalidOperationException("Native variant popup missing: " + label);
            ActionBarSlot[] rows = popup.GetComponentsInChildren<ActionBarSlot>(true).Where(row =>
                row.gameObject.activeInHierarchy && (row.MechanicSlot?.GetContentData() as AbilityData)?.ConvertedFrom?.Blueprint == parent).ToArray();
            BlueprintAbility[] expected = parent.Variants.Where(v => v != null).ToArray();
            bool exact = rows.Length == expected.Length && expected.All(v => rows.Count(r =>
                ((AbilityData)r.MechanicSlot.GetContentData()).Blueprint == v) == 1);
            Cleanup2Check("popup-identities-" + label, exact, new JObject {
                ["parent"] = parent.AssetGuid, ["expected"] = new JArray(expected.Select(v => v.AssetGuid)),
                ["actual"] = new JArray(rows.Select(r => ((AbilityData)r.MechanicSlot.GetContentData()).Blueprint.AssetGuid)) },
                "The real native popup contains every live parent's child exactly once, preserving unknown third-party entries.");
            if (!exact) throw new InvalidOperationException("Native popup child identities differ: " + label);
            ScrollRect scroll = popup.GetComponentInChildren<ScrollRect>(true);
            Vector2 before = scroll == null ? Vector2.zero : scroll.normalizedPosition;
            var covered = new HashSet<string>(StringComparer.Ordinal);
            try
            {
                Canvas.ForceUpdateCanvases();
                float viewport = scroll?.viewport == null ? 0f : scroll.viewport.rect.height;
                float range = scroll?.content == null ? 0f : Mathf.Max(0f, scroll.content.rect.height - viewport);
                int pages = viewport <= 0f || range <= .5f ? 1 : Mathf.CeilToInt(range / (viewport * .70f)) + 1;
                if (pages > 64) throw new InvalidOperationException("Unexpected unbounded native popup page count: " + pages);
                for (int page = 0; page < pages; page++)
                {
                    if (scroll != null) { scroll.StopMovement(); scroll.verticalNormalizedPosition = pages == 1 ? 1f : 1f - (float)page / (pages - 1); }
                    for (int frame = 0; frame < 6; frame++) yield return 0;
                    var visible = rows.Where(r => Cleanup2FullyVisible(scroll, r)).ToArray();
                    foreach (var row in visible) covered.Add(((AbilityData)row.MechanicSlot.GetContentData()).Blueprint.AssetGuid);
                    var state = new JObject { ["parent"] = parent.AssetGuid, ["label"] = label,
                        ["page"] = page, ["pages"] = pages, ["normalizedScroll"] = scroll == null ? 1f : scroll.verticalNormalizedPosition,
                        ["visible"] = new JArray(visible.Select(r => {
                            var data = (AbilityData)r.MechanicSlot.GetContentData();
                            return new JObject { ["guid"] = data.Blueprint.AssetGuid, ["internalName"] = data.Blueprint.name,
                                ["renderedText"] = new JArray(r.GetComponentsInChildren<TextMeshProUGUI>(true).Select(t => t.text)),
                                ["iconMatches"] = ReferenceEquals(r.Icon.sprite, data.Icon),
                                ["iconInstance"] = r.Icon.sprite == null ? 0 : r.Icon.sprite.GetInstanceID(),
                                ["convertedFrom"] = data.ConvertedFrom?.Blueprint.AssetGuid,
                                ["spellbook"] = data.Spellbook?.Blueprint.AssetGuid,
                                ["nestedVariants"] = data.Blueprint.Variants == null ? 0 : data.Blueprint.Variants.Length };
                        })) };
                    foreach (int frame in screens.Capture(label + "-page" + page, state,
                        () => popup != null && popup.gameObject.activeInHierarchy &&
                            ReferenceEquals(GroupController.Instance.GetCurrentCharacter(), _sprint25Fixture.Caster))) yield return frame;
                }
                bool complete = expected.All(v => covered.Contains(v.AssetGuid));
                Cleanup2Check("popup-full-page-coverage-" + label, complete,
                    new JObject { ["parent"] = parent.AssetGuid, ["pages"] = pages,
                        ["fullyVisibleChildren"] = new JArray(covered),
                        ["missing"] = new JArray(expected.Where(v => !covered.Contains(v.AssetGuid)).Select(v => v.AssetGuid)) },
                    "Each actual selectable child was fully visible on at least one captured native scroll page; no partial-only row counts as covered.");
                if (!complete) throw new InvalidOperationException("Native scroll coverage incomplete: " + label);
                foreach (var row in rows)
                {
                    var child = ((AbilityData)row.MechanicSlot.GetContentData()).Blueprint;
                    if (child.Variants == null || child.Variants.Length == 0) continue;
                    var childGroup = row as ActionBarGroupSlot;
                    if (childGroup == null) throw new InvalidOperationException("Reachable nested variant row lacks native group control: " + child.AssetGuid);
                    try
                    {
                        childGroup.OnToggleGroupClick();
                        for (int frame = 0; frame < 12; frame++) yield return 0;
                        foreach (int frame in Cleanup2CapturePopup(childGroup, child, label + "-" + child.AssetGuid,
                            screens, new HashSet<string>(lineage))) yield return frame;
                    }
                    finally { childGroup.HideSubGroup(); }
                }
            }
            finally { if (scroll != null) { scroll.StopMovement(); scroll.normalizedPosition = before; } }
        }

        private static bool Cleanup2FullyVisible(ScrollRect scroll, ActionBarSlot row)
        {
            var rect = row.transform as RectTransform;
            if (rect == null || !row.gameObject.activeInHierarchy) return false;
            if (scroll?.viewport == null) return true;
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            Vector3[] local = corners.Select(p => scroll.viewport.InverseTransformPoint(p)).ToArray();
            Rect viewport = scroll.viewport.rect;
            return local.Min(p => p.y) >= viewport.yMin - .5f && local.Max(p => p.y) <= viewport.yMax + .5f &&
                local.Min(p => p.x) >= viewport.xMin - .5f && local.Max(p => p.x) <= viewport.xMax + .5f;
        }
    }
}
