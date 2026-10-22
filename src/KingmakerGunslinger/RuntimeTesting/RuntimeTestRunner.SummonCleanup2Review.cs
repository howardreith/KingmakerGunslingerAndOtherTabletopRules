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
        private void AppendCleanup2EvidenceFiles(ICollection<string> files)
        {
            var names = new HashSet<string>(StringComparer.Ordinal) {
                "summon-cleanup2-observations.json", "summon-cleanup2-narrative-state.json",
                "native-ui-screens.json"
            };
            foreach (JProperty property in _summonCleanup2Observations.Descendants().OfType<JProperty>()
                .Where(value => value.Name == "file" && value.Value.Type == JTokenType.String))
            {
                string name = (string)property.Value;
                if (!string.IsNullOrEmpty(name) && Path.GetFileName(name) == name) names.Add(name);
            }
            // Only this fresh request directory is inspected. No game/save
            // directory or unrelated evidence is enumerated.
            string screenIndex = Path.Combine(_request.EvidenceDirectory, "native-ui-screens.json");
            if (File.Exists(screenIndex))
                foreach (string path in Directory.GetFiles(_request.EvidenceDirectory, "native-ui-*.png"))
                    names.Add(Path.GetFileName(path));
            if (names.Count > 2048) throw new InvalidOperationException("Bounded Cleanup 2 evidence collection exceeded.");
            foreach (string name in names.OrderBy(value => value, StringComparer.Ordinal))
            {
                string path = Path.Combine(_request.EvidenceDirectory, name);
                if (File.Exists(path) && !files.Contains(path)) files.Add(path);
            }
        }
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
                if (phase == "bodies" || phase == "gorgon-presentation" || phase == "recovery-bodies")
                    foreach (int frame in ReviewSummonCleanup2Bodies(fixture, book)) yield return frame;
                else if (phase == "elementals")
                    foreach (int frame in ReviewSummonCleanup2Elementals(fixture)) yield return frame;
                else if (phase == "elementals-presentation")
                    foreach (int frame in ReviewSummonCleanup2ElementalPresentation(fixture, book)) yield return frame;
                else if (phase == "cloth-probe")
                    foreach (int frame in ReviewSummonCleanup2ClothProbe(fixture, book)) yield return frame;
                else if (phase == "ettin-lifecycle")
                    foreach (int frame in ReviewSummonCleanup2EttinLifecycle(fixture, book)) yield return frame;
                else if (phase == "ettin-commands")
                {
                    foreach (int frame in ReviewSprint29Commands(fixture,false)) yield return frame;
                    foreach (int frame in ReviewSprint29Commands(fixture,true)) yield return frame;
                }
                else if (phase == "crab-attack")
                    foreach (int frame in ReviewSummonCleanup2CrabAttack(fixture, book)) yield return frame;
                else if (phase == "production-attacks")
                    foreach (int frame in ReviewSummonCleanup2ProductionAttacks(fixture, book)) yield return frame;
                else if (phase == "shared-controls")
                    foreach (int frame in ReviewSummonCleanup2SharedControls(fixture, book)) yield return frame;
                else if (phase == "tooltips")
                {
                    _sprint25WaitingForUnityAppearance = true;
                    foreach (int frame in ReviewSummonCleanup2Tooltips(fixture, book)) yield return frame;
                }
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
                    // A native Default scope owns progression until its exact
                    // mode/controller restoration succeeds. Never resume the
                    // manual fixture clock over a still-owned native mode.
                    _sprint25WaitingForUnityAppearance = Cleanup2NativeDefaultPending || appearanceWait;
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
            IDisposable heightProbe = (string)_request.Parameters?["reviewPhase"] == "scale-prototype"
                ? GiantSummonPresentationPatch.BeginRenderScaleProbe(_request) : null;
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
                if ((string)_request.Parameters?["reviewPhase"] == "scale-prototype")
                {
                    // Production colors have independent ownership evidence.
                    // Their pictures must survive a later height-measurement refusal.
                    for (int i = 1; i < actors.Count; i++)
                    {
                        var paletteWait = System.Diagnostics.Stopwatch.StartNew();
                        while (GiantSummonPalettePolicy.ProductionProof(keys[i]) != null &&
                            GiantSummonPaletteRuntime.Describe(actors[i].View).StartsWith("palette:await", StringComparison.Ordinal) &&
                            paletteWait.Elapsed.TotalSeconds < 16d) yield return 0;
                        IDisposable untouchedLease = null; JObject palette = null;
                        Cleanup2ApplyPrototypePalette(actors[i], ref untouchedLease, ref palette);
                        Cleanup2Check("production-palette-" + keys[i],
                            untouchedLease == null && palette != null && (bool?)palette["passed"] == true,
                            palette, "The ordinary summon owns its exact palette; this observation never applies or releases it.");
                    }
                }
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
                if ((string)_request.Parameters?["reviewPhase"] == "scale-prototype")
                {
                    var framing = actors.Select(actor => {
                        Renderer[] skins = actor.View.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                            .Where(skin => skin.enabled && skin.gameObject.activeInHierarchy).Cast<Renderer>().ToArray();
                        if (skins.Length == 0) throw new InvalidOperationException("No visible giant presentation surface.");
                        Bounds bounds = skins[0].bounds;
                        foreach (Renderer skin in skins.Skip(1)) bounds.Encapsulate(skin.bounds);
                        return new Cleanup2ActorMetric { Min = bounds.min, Max = bounds.max,
                            Crown = bounds.center + Vector3.up * bounds.extents.y,
                            Sole = bounds.center - Vector3.up * bounds.extents.y };
                    }).ToArray();
                    JObject picture = Cleanup2CaptureLineup(actors, keys, framing, "cleanup2-production-colors.png");
                    picture["framingOnly"] = true; picture["measurementEvidence"] = false;
                    picture["rendering"] = "Ordinary production giant colors on the surveyed common ground; renderer bounds frame the picture only.";
                    Cleanup2Check("production-colors-picture", true, picture,
                        "All six families stand on the same surveyed ground before the fitted native color capture.");
                }
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
                    // Review19 measured the same native 22-29cm visual sole
                    // offset for Human and both giant families. Navmesh Y is a
                    // placement surface, not an engine promise of skin contact.
                    bool stable = samples.All(v => v[i].Valid) && range <= .05f && stableScale;
                    standing &= stable;
                    summary.Add(new JObject { ["key"] = keys[i], ["unit"] = actors[i].UniqueId,
                        ["meanBodyHeightWorldUnits"] = heights.Average(), ["poseHeightRange"] = range,
                        ["maximumSoleGroundGap"] = soleGap, ["stableStandingSamples"] = stable,
                        ["sameViewScaleAcrossSamples"] = stableScale,
                        ["heightOverHuman"] = heights.Average() / samples.Average(v => v[0].Height),
                        ["heightOverHill"] = heights.Average() / samples.Average(v => v[1].Height),
                        ["soleOffsetIsDiagnosticNotAdmission"] = true });
                }
                Cleanup2Check("standing-comparison-eligibility", standing, new JObject {
                    ["actors"] = summary, ["calibrationApplied"] = false,
                    ["ratioLimitation"] = "Finite stable current-body samples support relative family comparison. Native sole/navmesh offsets are recorded, not assumed zero; no absolute Human/feet calibration is claimed." },
                    "Four finite stable native idle body samples on the surveyed plane admit a relative render-only experiment; navmesh-to-skin offset is not an anatomy calibration.");
                bool scalePrototype = (string)_request.Parameters?["reviewPhase"] == "scale-prototype";
                Cleanup2Check("same-camera-lineup", true,
                    Cleanup2CaptureLineup(actors, keys, samples.Last(),
                        scalePrototype ? "cleanup2-scale-prototype-before.png" : null),
                    "One live same-camera capture contains the verified Human and all six canonically summoned giant families.");
                if (scalePrototype)
                    foreach (int frame in ReviewSummonCleanup2ScalePrototype(fixture, actors, keys, groundPoints, samples, standing)) yield return frame;
            }
            finally
            {
                try { if (heightProbe != null) heightProbe.Dispose(); }
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
            // Installed 2.1.7b UnitMovementAgent.PathTo (060018A3,
            // IL0101-011A) selects mask1 for ordinary walking, mask2 in
            // turn-based combat. Seeker.StartPath (060004ED, IL0034-003B)
            // writes that mask into the path's own NNConstraint. The
            // one-argument GetNearest uses NNConstraint.None instead and can
            // select unwalkable nodes or the overlapping combat grid.
            bool turnBased = TurnBased.Controllers.CombatController.IsInTurnBasedCombat();
            int nativeMask = turnBased ? 2 : 1;
            var queryActors = new JArray();
            var nativeTags = new List<int>();
            foreach (UnitEntityData actor in actors)
            {
                var agent = actor.View?.AgentASP;
                if (agent == null || agent.Unit != actor.View)
                    throw new InvalidOperationException("Owned actor has no exact native walking agent.");
                FieldInfo seekerField = agent.GetType().GetField("m_Seeker", Cleanup2Fields);
                MethodInfo constructTags = agent.GetType().GetMethod("ConstructTags", Cleanup2Fields);
                var seeker = seekerField?.GetValue(agent) as Seeker;
                if (seeker == null || constructTags == null || constructTags.ReturnType != typeof(int) || constructTags.GetParameters().Length != 0)
                    throw new MissingMemberException("Installed native walking seeker/tag contract unavailable.");
                // ConstructTags is the actual pure native query used by
                // PathTo; no path, seeker, graph or shared constraint changes.
                int tags = (int)constructTags.Invoke(agent, null);
                nativeTags.Add(tags);
                var currentField = agent.GetType().GetField("m_Path", Cleanup2Fields);
                queryActors.Add(new JObject { ["unit"] = actor.UniqueId,
                    ["agentInstance"] = agent.GetInstanceID(), ["seekerInstance"] = seeker.GetInstanceID(),
                    ["agentPosition"] = SurveyVector(agent.transform.position),
                    ["seekerTraversableTagsAtIntake"] = seeker.traversableTags,
                    ["nativeConstructTags"] = tags,
                    ["agentCurrentPath"] = Cleanup2GroundPath(currentField?.GetValue(agent) as Pathfinding.Path, actor.Position),
                    ["agentRequestedPath"] = Cleanup2GroundPath(agent.GetType().GetField("m_RequestedPath", Cleanup2Fields)?.GetValue(agent) as Pathfinding.Path, actor.Position),
                    ["seekerCurrentPath"] = Cleanup2GroundPath(seeker.GetCurrentPath(), actor.Position) });
            }
            // All actors must be allowed at every selected support point.
            // Ordinary walking returns -1 for every actor; combat erosion tags
            // are intersected rather than borrowing the Human's smaller size.
            int commonTags = nativeTags.Aggregate(-1, (mask, tags) => mask & tags);
            var constraint = new Pathfinding.PathNNConstraint { graphMask = nativeMask,
                constrainWalkability = true, walkable = true, constrainArea = false,
                constrainTags = true, tags = commonTags, constrainDistance = true,
                distanceXZ = false, distanceXZkeepY = false };
            var anchorNode = AstarPath.active.GetNearest(anchor, constraint);
            bool anchorValid = anchorNode.node != null && anchorNode.node.Walkable &&
                constraint.Suitable(anchorNode.node) && Cleanup2Finite(anchorNode.clampedPosition) &&
                constraint.SuitableGraph((int)anchorNode.node.GraphIndex, AstarPath.active.graphs[anchorNode.node.GraphIndex]);
            var query = new JObject { ["turnBasedCombat"] = turnBased, ["nativePathToGraphMask"] = nativeMask,
                ["commonNativeTraversalTags"] = commonTags, ["actors"] = queryActors,
                ["graphs"] = new JArray(AstarPath.active.graphs.Select((graph, index) => new JObject {
                    ["index"] = index, ["type"] = graph?.GetType().FullName, ["name"] = graph?.name,
                    ["allowedByNativePathToMask"] = graph != null && constraint.SuitableGraph(index, graph) })),
                ["anchor"] = SurveyVector(anchor), ["anchorClamped"] = SurveyVector(anchorNode.clampedPosition),
                ["anchorGraph"] = anchorNode.node == null ? (JToken)JValue.CreateNull() : new JValue(anchorNode.node.GraphIndex),
                ["anchorArea"] = anchorNode.node == null ? (JToken)JValue.CreateNull() : new JValue(anchorNode.node.Area),
                ["anchorVerticalClamp"] = anchorNode.clampedPosition.y - anchor.y,
                ["anchor3DClamp"] = Vector3.Distance(anchorNode.clampedPosition, anchor),
                ["anchorVerifiedNativeWalkingGraph"] = anchorValid,
                ["contract"] = "Installed Assembly-CSharp 3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb; PathTo060018A3 mask1 outside combat/mask2 inside; ConstructTags060018C3; Seeker.StartPath060004ED; AstarPath.GetNearest0600054F. Query-owned constraint, no graph/seeker/path writes. Native navmesh Y is recorded, never used as anatomy or independent physical contact proof." };
            Cleanup2Check("lineup-native-ground-query", anchorValid, query,
                "A positive owned actor's exact native walking contract selects the walkable graph before any bounded terrain survey; overlapping unrelated graphs cannot win an unconstrained query.");
            if (!anchorValid)
                throw new InvalidOperationException("Lineup anchor has no finite walkable native graph node.");
            constraint.SetStart(anchorNode.node);
            constraint.constrainArea = true;
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
            // The first constrained survey found 39 full seven-center inputs
            // only at factor1.0, all failing the unchanged native footprint
            // spacing after up to35cm navmesh clamp. Finer positive padding
            // samples resolve that concrete boundary; no gap/plane guard moves.
            foreach (float spacingFactor in new[] { 1.35f, 1.15f, 1.08f, 1.04f, 1.02f, 1f })
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
                        if (attempts.Count >= 3528 || watch.Elapsed.TotalSeconds > 15)
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
                            var nearest = AstarPath.active.GetNearest(requested, constraint);
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
                        row["minimumAdjacentCenterGaps"] = new JArray(minimumGaps);
                        row["actualAdjacentCenterGaps"] = new JArray(Enumerable.Range(1, Math.Max(0, centers.Count - 1)).Select(i => {
                            Vector3 delta = centers[i] - centers[i - 1]; delta.y = 0f; return delta.magnitude; }));
                        JObject plane = null;
                        if (support.Count == actors.Count * 3)
                        {
                            plane = Cleanup2FitGroundPlane(support);
                            row["plane"] = plane;
                            if (refusal == null)
                            {
                                if (!(bool)plane["finiteNondegenerate"]) refusal = "degenerate-or-nonfinite-plane";
                                else if ((float)plane["grade"] > .03f) refusal = "plane-grade-over-3-percent";
                                else if ((float)plane["maximumVerticalResidualMeters"] > .05f) refusal = "plane-residual-over-5cm";
                            }
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
                ["nativeQuery"] = query, ["queryGraphMask"] = constraint.graphMask,
                ["queryConnectedArea"] = constraint.area, ["queryTags"] = constraint.tags,
                ["candidateCount"] = attempts.Count, ["wallSeconds"] = watch.Elapsed.TotalSeconds,
                ["candidateLimit"] = 3528, ["wallLimitSeconds"] = 15,
                ["selected"] = selected?.Document, ["attempts"] = attempts,
                ["refusals"] = new JObject(refusals.Select(pair => new JProperty(pair.Key, pair.Value))),
                ["proof"] = "Every actor center and both lateral terrain samples are finite walkable clamped points in the anchor's same connected native graph; <=35cm horizontal clamp, <=3% fitted plane grade and <=5cm vertical residual. Requested Y is only a graph query hint. World-up anatomy and each actor's own ground contact remain independent checks. No actor moved during survey." };
            Cleanup2Check("lineup-ground-survey", selected != null, diagnostic,
                "A bounded courtyard search finds seven adequately separated native ground positions on a nondegenerate measured common plane.");
            if (selected == null)
                throw new InvalidOperationException("No proven native seven-actor terrain plane within the recorded bounded courtyard survey; no scale calibration is authorized.");
            return selected;
        }

        private static JObject Cleanup2GroundPath(Pathfinding.Path path, Vector3 ownerPosition)
        {
            if (path == null) return new JObject { ["present"] = false };
            // An in-flight worker owns its mutable result lists. Observe only
            // its scalar state until completion; never race an unfinished path.
            bool done = path.IsDone();
            var nodes = !done || path.path == null ? new Pathfinding.GraphNode[0] : path.path.Where(value => value != null).ToArray();
            var closest = nodes.OrderBy(value => Vector3.SqrMagnitude((Vector3)value.position - ownerPosition)).FirstOrDefault();
            return new JObject { ["present"] = true, ["type"] = path.GetType().FullName,
                ["pathId"] = path.pathID, ["isDone"] = done, ["error"] = path.error, ["completeState"] = path.CompleteState.ToString(),
                ["nodeCount"] = nodes.Length, ["graphIndices"] = new JArray(nodes.Select(value => value.GraphIndex).Distinct()),
                ["connectedAreas"] = new JArray(nodes.Select(value => value.Area).Distinct()),
                ["closestRecordedNodeGraph"] = closest == null ? (JToken)JValue.CreateNull() : new JValue(closest.GraphIndex),
                ["closestRecordedNodeArea"] = closest == null ? (JToken)JValue.CreateNull() : new JValue(closest.Area),
                ["closestRecordedNodeDistance"] = closest == null ? (JToken)JValue.CreateNull() : new JValue(Vector3.Distance((Vector3)closest.position, ownerPosition)),
                ["constraintGraphMask"] = path.nnConstraint?.graphMask,
                ["constraintArea"] = path.nnConstraint?.area, ["constraintConstrainArea"] = path.nnConstraint?.constrainArea,
                ["constraintTags"] = path.nnConstraint?.tags,
                ["meaning"] = "Read-only current native path snapshot; historical nodes are evidence, not an invented current physical-contact location." };
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
            foreach (string name in new[] { "_Dissolve", "_DissolveAmount", "_ZWrite", "_CullMode", "_SrcBlend", "_DstBlend", "_Mode", "_Surface", "_Roughness", "_Metallic", "_Emission" })
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
                    ["texture2DFormat"] = value is Texture2D ? ((Texture2D)value).format.ToString() : null,
                    ["texture2DReadable"] = value is Texture2D ? (JToken)new JValue(((Texture2D)value).isReadable) : JValue.CreateNull(),
                    ["scale"] = new JArray(material.GetTextureScale(name).x, material.GetTextureScale(name).y),
                    ["offset"] = new JArray(material.GetTextureOffset(name).x, material.GetTextureOffset(name).y) };
            }
            Texture texture = material.HasProperty("_MainTex") ? material.GetTexture("_MainTex") : null;
            Texture roughnessLookup = Shader.GetGlobalTexture("unity_NHxRoughness");
            return new JObject { ["name"] = material.name, ["instance"] = material.GetInstanceID(),
                ["activeColorSpace"] = QualitySettings.activeColorSpace.ToString(),
                ["shader"] = material.shader?.name, ["renderQueue"] = material.renderQueue,
                ["localKeywords"] = new JArray(material.shaderKeywords.OrderBy(k=>k,StringComparer.Ordinal)),
                ["globalSpecularOn"] = Shader.IsKeywordEnabled("SPECULAR_ON"),
                ["globalMetallnessOn"] = Shader.IsKeywordEnabled("METALLNESS_ON"),
                ["nativeRoughnessLookup"] = roughnessLookup == null ? (JToken)JValue.CreateNull() : new JObject {
                    ["name"] = roughnessLookup.name, ["instance"] = roughnessLookup.GetInstanceID(),
                    ["width"] = roughnessLookup.width, ["height"] = roughnessLookup.height },
                ["roughnessLookupScope"] = "Read-only global lookup identity; null is not evidence of a missing engine-internal binding.",
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
                    bool sourceReadable = skin.sharedMesh.isReadable;
                    // The installed inaccessible getter logs an engine error
                    // and returns an empty array. Preserve the empty-source
                    // guard without deliberately issuing that invalid read.
                    Vector3[] source = sourceReadable ? skin.sharedMesh.vertices ?? new Vector3[0] : new Vector3[0];
                    int rawSourceCount = source.Length;
                    bool exactCapturedSource = false;
                    JObject cpuReference = null;
                    if (source.Length == 0)
                    {
                        Vector3[] captured;
                        if (Cleanup2CharacterCpuReference.RequestAllowed(_request))
                        {
                            exactCapturedSource = Cleanup2CharacterCpuReference.TryGetExactSource(
                                unit.View, skin, out captured, out cpuReference);
                            if (exactCapturedSource) source = captured;
                        }
                        else cpuReference = new JObject { ["available"] = false, ["reason"] = "not-exact-disposable-giant-request" };
                    }
                    BoneWeight[] weights = skin.sharedMesh.boneWeights ?? new BoneWeight[0];
                    Matrix4x4[] binds = skin.sharedMesh.bindposes ?? new Matrix4x4[0];
                    Transform[] bones = skin.bones ?? new Transform[0];
                    var skinData = new JObject { ["sourceVertexCount"] = source.Length,
                        ["rawSharedMeshSourceVertexCount"] = rawSourceCount,
                        ["sharedMeshCpuReadable"] = sourceReadable,
                        ["sourceMode"] = exactCapturedSource ? "exact-current-Character-pre-upload-reference" : "native-sharedMesh.vertices",
                        ["cpuReference"] = cpuReference,
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

        private JObject Cleanup2CaptureLineup(IList<UnitEntityData> actors, IList<string> keys, Cleanup2ActorMetric[] metrics,
            string fileName = null, bool experimentalProxy = false)
        {
            Camera camera = TeleportationCastingCamera()?.Camera ?? Camera.main;
            if (camera == null) throw new InvalidOperationException("No native lineup camera.");
            Vector3 beforePosition = camera.transform.position;
            Quaternion beforeRotation = camera.transform.rotation;
            float beforeSize = camera.orthographicSize, beforeAspect = camera.aspect;
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
                if (rowAxis.sqrMagnitude < .5f) rowAxis = Vector3.right;
                if (forward.sqrMagnitude < .5f) forward = Vector3.forward;
                var corners = metrics.SelectMany(m => Cleanup2BoundsCorners(new Bounds((m.Min+m.Max)*.5f,m.Max-m.Min))).ToArray();
                // Native camera may be orthographic: moving it backwards alone
                // cannot widen that projection. Fit all eight corners in the
                // actual camera basis, with the output aspect set first.
                camera.aspect = 1920f / 1080f;
                var cameraAttempts = new JArray();
                bool clearSight = false;
                // The old ten-degree view photographed native blue occlusion
                // silhouettes through a wall. Use a gameplay-like downward
                // view, test both sides, and retain each obstruction result.
                foreach (float pitch in new[] { 60f, 72f, 80f })
                {
                    foreach (float side in new[] { 1f, -1f })
                    {
                        Vector3 direction = forward * side * Mathf.Cos(pitch * Mathf.Deg2Rad) -
                            Vector3.up * Mathf.Sin(pitch * Mathf.Deg2Rad);
                        Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
                        Quaternion inverse = Quaternion.Inverse(rotation);
                        Vector3[] projected = corners.Select(p => inverse * (p - centre)).ToArray();
                        float halfWidth = projected.Max(p => Mathf.Abs(p.x));
                        float halfHeight = projected.Max(p => Mathf.Abs(p.y));
                        float halfDepth = projected.Max(p => Mathf.Abs(p.z));
                        float halfFrame = Mathf.Max(halfHeight, halfWidth / camera.aspect) * 1.16f;
                        float distance = halfDepth + Mathf.Max(12f, halfFrame / Mathf.Tan(camera.fieldOfView * Mathf.Deg2Rad * .5f));
                        if (camera.orthographic) camera.orthographicSize = halfFrame;
                        camera.transform.SetPositionAndRotation(centre - direction * distance, rotation);
                        JArray blockers = Cleanup2LineupSightBlockers(camera, actors, metrics);
                        cameraAttempts.Add(new JObject { ["pitchDegrees"] = pitch, ["side"] = side,
                            ["cameraPosition"] = SurveyVector(camera.transform.position), ["blockers"] = blockers });
                        if (blockers.Count == 0) { clearSight = true; break; }
                    }
                    if (clearSight) break;
                }
                if (!clearSight)
                    throw new InvalidOperationException("No downward native lineup camera has clear environment sight: " + cameraAttempts.ToString());
                target = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
                camera.targetTexture = target;
                camera.Render();
                var viewport = new JArray(metrics.Select((m, i) => new JObject {
                    ["key"] = keys[i], ["unit"] = actors[i].UniqueId,
                    ["crown"] = SurveyVector(camera.WorldToViewportPoint(m.Crown)),
                    ["sole"] = SurveyVector(camera.WorldToViewportPoint(m.Sole)) }));
                bool visible = corners.All(p => {
                    Vector3 v = camera.WorldToViewportPoint(p); return v.z > 0 && v.x > .01f && v.x < .99f && v.y > .01f && v.y < .99f;
                });
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
                string file = fileName ?? "cleanup2-six-giants-human-lineup.png";
                if (Path.GetFileName(file) != file || Path.GetExtension(file) != ".png" ||
                    File.Exists(Path.Combine(_request.EvidenceDirectory, file)))
                    throw new InvalidOperationException("Exact new lineup PNG name required; existing evidence is never overwritten.");
                File.WriteAllBytes(Path.Combine(_request.EvidenceDirectory, file), bytes);
                return new JObject { ["file"] = file, ["bytes"] = bytes.Length,
                    ["cameraInstance"] = camera.GetInstanceID(),
                    ["cameraPosition"] = SurveyVector(camera.transform.position),
                    ["cameraRotation"] = new JArray(camera.transform.rotation.x, camera.transform.rotation.y, camera.transform.rotation.z, camera.transform.rotation.w),
                    ["fieldOfView"] = camera.fieldOfView, ["orthographic"] = camera.orthographic,
                    ["screenFade"] = screenFade, ["meanLuma"] = luma,
                    ["downwardCameraAttempts"] = cameraAttempts, ["noEnvironmentPhysicsSightBlocker"] = clearSight,
                    ["occlusionProofLimit"] = "Physics rays reject blocking scene colliders; actual framebuffer still requires visual review for collider-free rendered occluders. No scene renderer/culling/highlight state is changed.",
                    ["allCrownSoleInFrame"] = visible, ["viewport"] = viewport,
                    ["rendering"] = experimentalProxy
                        ? "Actual live actors with explicitly owned experimental render-only proxies/palettes in native scene lighting. No synthetic silhouette. Camera auto-framing may differ from before; both exact poses are recorded."
                        : "Actual live actors, native materials and scene lighting; no proxy meshes, recoloring or synthetic silhouettes." };
            }
            finally
            {
                camera.targetTexture = beforeTarget;
                camera.transform.position = beforePosition;
                camera.transform.rotation = beforeRotation;
                camera.orthographicSize = beforeSize; camera.aspect = beforeAspect;
                RenderTexture.active = beforeActive;
                if (target != null) { target.Release(); UnityEngine.Object.Destroy(target); }
                if (image != null) UnityEngine.Object.Destroy(image);
            }
        }

        private static JArray Cleanup2LineupSightBlockers(Camera camera, IList<UnitEntityData> actors, Cleanup2ActorMetric[] metrics)
        {
            var blockers = new JArray();
            for (int i = 0; i < metrics.Length; i++)
            foreach (Vector3 target in new[] { metrics[i].Crown, (metrics[i].Crown + metrics[i].Sole) * .5f,
                Vector3.Lerp(metrics[i].Sole, metrics[i].Crown, .12f) })
            {
                // Probe crown, torso and lower leg. An unqualified physical
                // sole/floor relationship is not a camera obstruction test.
                // Orthographic rays originate at each target's image-plane
                // projection, rather than converging at the camera position.
                Vector3 viewport = camera.WorldToViewportPoint(target);
                Ray ray = camera.ViewportPointToRay(new Vector3(viewport.x, viewport.y, 0f));
                float distance = Vector3.Dot(target - ray.origin, ray.direction);
                if (!Cleanup2Finite(distance) || distance <= 0f) throw new InvalidOperationException("Non-finite/behind-camera lineup sight input.");
                RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Max(0f, distance - .03f), ~0, QueryTriggerInteraction.Ignore);
                RaycastHit? obstruction = hits.Where(hit => hit.collider != null &&
                    !actors.Any(actor => actor.View != null && (hit.collider.transform == actor.View.transform ||
                        hit.collider.transform.IsChildOf(actor.View.transform))))
                    .OrderBy(hit => hit.distance).Select(hit => (RaycastHit?)hit).FirstOrDefault();
                if (obstruction.HasValue)
                    blockers.Add(new JObject { ["actor"] = actors[i].UniqueId,
                        ["collider"] = obstruction.Value.collider.name, ["instance"] = obstruction.Value.collider.GetInstanceID(),
                        ["type"] = obstruction.Value.collider.GetType().FullName, ["distance"] = obstruction.Value.distance });
            }
            return blockers;
        }

        private static IEnumerable<Vector3> Cleanup2BoundsCorners(Bounds bounds)
        {
            for(int x=-1;x<=1;x+=2) for(int y=-1;y<=1;y+=2) for(int z=-1;z<=1;z+=2)
                yield return bounds.center+Vector3.Scale(bounds.extents,new Vector3(x,y,z));
        }

        private string _summonCleanup2NarrativeStateRunId;

        private bool Cleanup2NoNarrativeOverlay()
        {
            // Read-only admission proof. Native Dialog.Show(false) normally
            // fades its CanvasGroup and moves the panel without deactivating
            // the GameObject. Book controllers deactivate after their fade,
            // and Show(false) is a no-op when m_IsActive is already false.
            // Neither activeInHierarchy alone nor Dialog game mode proves a
            // live narrative. Never call Show/Stop/Continue to obtain a pass.
            var row = new JObject {
                ["runId"] = _request?.RunId,
                ["phase"] = _request?.Parameters == null ? null : (string)_request.Parameters["reviewPhase"],
                ["utc"] = DateTime.UtcNow.ToString("o"),
                ["frame"] = Time.frameCount,
                ["observer"] = "Read-only installed 2.1.7b lifecycle fields and actual native UI alpha; no narrative/input mutation.",
                ["contract"] = "No live BlueprintDialog; manager and all child lifecycle flags closed; each child either inactive in hierarchy or settled at exact zero root/inherited rendered alpha." };
            bool absent = false;
            try
            {
                var game = Game.Instance;
                var controller = game?.DialogController;
                var dialog = controller?.Dialog;
                var manager = game?.UI?.Common?.DialogUIManager;
                row["mode"] = game?.CurrentMode.ToString();
                row["dialogControllerPresent"] = controller != null;
                row["liveDialog"] = dialog == null ? null : new JObject {
                    ["guid"] = dialog.AssetGuid,
                    ["name"] = dialog.name,
                    ["blueprintType"] = dialog.GetType().FullName,
                    ["dialogType"] = dialog.Type.ToString(),
                    ["dialogTypeValue"] = Convert.ToInt32(dialog.Type) };
                var managerRow = new JObject();
                row["manager"] = managerRow;
                Cleanup2NarrativeObject(manager, managerRow);
                bool? managerShown = Cleanup2NarrativeFlag(manager,
                    typeof(Kingmaker.UI.Dialog.DialogBookEventUIManager), "m_IsShow", managerRow);
                FieldInfo stateField = typeof(Kingmaker.UI.Dialog.DialogBookEventUIManager)
                    .GetField("m_State", Cleanup2Fields | BindingFlags.DeclaredOnly);
                bool stateContract = stateField != null && stateField.FieldType.FullName ==
                    "Kingmaker.DialogSystem.Blueprints.DialogType";
                managerRow["stateFieldContract"] = stateContract;
                managerRow["state"] = manager != null && stateContract
                    ? stateField.GetValue(manager).ToString() : null;
                var dialogRow = new JObject();
                var bookRow = new JObject();
                var interchapterRow = new JObject();
                row["dialogUi"] = dialogRow;
                row["bookEventUi"] = bookRow;
                row["interchapterUi"] = interchapterRow;
                bool dialogHidden = Cleanup2NarrativeControllerRead(manager?.Dialog,
                    typeof(Kingmaker.UI.Dialog.DialogController), true, dialogRow);
                bool bookHidden = Cleanup2NarrativeControllerRead(manager?.BookEvent,
                    typeof(Kingmaker.UI.BookEvent.BookEventBaseController), false, bookRow);
                bool interchapterHidden = Cleanup2NarrativeControllerRead(manager?.Interchapter,
                    typeof(Kingmaker.UI.BookEvent.BookEventBaseController), false, interchapterRow);
                absent = controller != null && dialog == null && manager != null &&
                    stateContract && managerShown == false && dialogHidden && bookHidden && interchapterHidden;
            }
            catch (Exception error)
            {
                row["readFailure"] = error.ToString();
            }
            row["narrativeOverlayAbsent"] = absent;
            // Save the first observation and every refusal atomically before
            // returning to the caller. A later successful read never overwrites
            // a refusal. Ownership predicates call this during each UI capture;
            // successful repeated reads do not create per-frame disk writes.
            if (_summonCleanup2NarrativeStateRunId != _request?.RunId || !absent)
            {
                row["snapshotReason"] = absent ? "first-closed-observation" : "refusal";
                if (!IsSummonCleanup2Review || string.IsNullOrEmpty(_request.EvidenceDirectory))
                    throw new InvalidOperationException("Owned Cleanup 2 evidence directory required for narrative-state admission.");
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory,
                    "summon-cleanup2-narrative-state.json"), row.ToString(Formatting.Indented));
                _summonCleanup2NarrativeStateRunId = _request.RunId;
            }
            return absent;
        }

        private static void Cleanup2NarrativeObject(MonoBehaviour value, JObject row)
        {
            row["exists"] = value != null;
            if (value == null) return;
            row["type"] = value.GetType().FullName;
            row["instance"] = value.GetInstanceID();
            row["name"] = value.gameObject.name;
            row["activeSelf"] = value.gameObject.activeSelf;
            row["activeInHierarchy"] = value.gameObject.activeInHierarchy;
            row["enabled"] = value.enabled;
            row["isActiveAndEnabled"] = value.isActiveAndEnabled;
            CanvasGroup group = value.GetComponent<CanvasGroup>();
            row["ownCanvasGroup"] = group == null ? null : new JObject {
                ["instance"] = group.GetInstanceID(), ["enabled"] = group.enabled,
                ["alpha"] = group.alpha, ["interactable"] = group.interactable,
                ["blocksRaycasts"] = group.blocksRaycasts,
                ["ignoreParentGroups"] = group.ignoreParentGroups };
        }

        private static bool? Cleanup2NarrativeFlag(object value, Type declaringType, string name, JObject row)
        {
            FieldInfo field = declaringType.GetField(name, Cleanup2Fields | BindingFlags.DeclaredOnly);
            bool contract = field != null && field.FieldType == typeof(bool);
            row[name + "FieldContract"] = contract;
            bool? result = value != null && contract ? (bool?)field.GetValue(value) : null;
            row[name] = result.HasValue ? new JValue(result.Value) : JValue.CreateNull();
            return result;
        }

        private static bool Cleanup2NarrativeControllerRead(MonoBehaviour value, Type lifecycleType,
            bool ordinaryDialog, JObject row)
        {
            try { return Cleanup2NarrativeController(value, lifecycleType, ordinaryDialog, row); }
            catch (Exception error) { row["readFailure"] = error.ToString(); return false; }
        }

        private static bool Cleanup2NarrativeController(MonoBehaviour value, Type lifecycleType,
            bool ordinaryDialog, JObject row)
        {
            Cleanup2NarrativeObject(value, row);
            bool? active = Cleanup2NarrativeFlag(value, lifecycleType, "m_IsActive", row);
            bool? animation = ordinaryDialog
                ? Cleanup2NarrativeFlag(value, lifecycleType, "m_PlayAnim", row) : (bool?)false;
            if (value == null) return false;
            // Do not invoke the private CanvasGroup property: its native getter
            // lazily writes m_CanvasGroup. Reading the component is sufficient.
            CanvasGroup root = value.GetComponent<CanvasGroup>();
            var groups = new JArray();
            row["canvasGroups"] = groups;
            foreach (CanvasGroup group in value.GetComponentsInChildren<CanvasGroup>(true))
                groups.Add(new JObject {
                    ["path"] = Cleanup2NarrativePath(value.transform, group.transform),
                    ["instance"] = group.GetInstanceID(),
                    ["isControllerRoot"] = group == root,
                    ["activeSelf"] = group.gameObject.activeSelf,
                    ["activeInHierarchy"] = group.gameObject.activeInHierarchy,
                    ["enabled"] = group.enabled,
                    ["alpha"] = group.alpha,
                    ["interactable"] = group.interactable,
                    ["blocksRaycasts"] = group.blocksRaycasts,
                    ["ignoreParentGroups"] = group.ignoreParentGroups });
            var renderers = new JArray();
            row["canvasRenderers"] = renderers;
            int nonzeroActiveRenderers = 0;
            foreach (CanvasRenderer renderer in value.GetComponentsInChildren<CanvasRenderer>(true))
            {
                float alpha = renderer.GetAlpha();
                float inherited = renderer.GetInheritedAlpha();
                bool valid = !float.IsNaN(alpha) && !float.IsInfinity(alpha) && alpha >= 0f && alpha <= 1f &&
                    !float.IsNaN(inherited) && !float.IsInfinity(inherited) && inherited >= 0f && inherited <= 1f;
                bool activeInHierarchy = renderer.gameObject.activeInHierarchy;
                // GetInheritedAlpha is the installed native final alpha,
                // including applicable parent CanvasGroups. A descendant with
                // ignoreParentGroups cannot silently bypass this proof. Cull
                // is diagnostic only: transient clipping never earns a pass.
                if (activeInHierarchy && (!valid || inherited != 0f)) nonzeroActiveRenderers++;
                renderers.Add(new JObject {
                    ["path"] = Cleanup2NarrativePath(value.transform, renderer.transform),
                    ["instance"] = renderer.GetInstanceID(),
                    ["activeSelf"] = renderer.gameObject.activeSelf,
                    ["activeInHierarchy"] = activeInHierarchy,
                    ["cull"] = renderer.cull,
                    ["alpha"] = alpha,
                    ["inheritedAlpha"] = inherited,
                    ["alphaFiniteAndInRange"] = valid });
            }
            var otherRenderers = new JArray();
            row["otherRenderers"] = otherRenderers;
            int activeOtherRenderers = 0;
            foreach (Renderer renderer in value.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer.gameObject.activeInHierarchy && renderer.enabled) activeOtherRenderers++;
                otherRenderers.Add(new JObject {
                    ["path"] = Cleanup2NarrativePath(value.transform, renderer.transform),
                    ["type"] = renderer.GetType().FullName,
                    ["instance"] = renderer.GetInstanceID(),
                    ["activeInHierarchy"] = renderer.gameObject.activeInHierarchy,
                    ["enabled"] = renderer.enabled });
            }
            bool visuallyHidden = !value.gameObject.activeInHierarchy ||
                (root != null && root.enabled && root.alpha == 0f && renderers.Count > 0 &&
                    nonzeroActiveRenderers == 0 && activeOtherRenderers == 0);
            bool hidden = active == false && animation == false && visuallyHidden;
            row["nonzeroOrInvalidActiveCanvasRenderers"] = nonzeroActiveRenderers;
            row["activeOtherRenderers"] = activeOtherRenderers;
            row["rootCanvasGroupPresent"] = root != null;
            row["rootCanvasGroupAlpha"] = root == null ? JValue.CreateNull() : new JValue(root.alpha);
            row["visuallyHidden"] = visuallyHidden;
            row["closedLifecycleAndHidden"] = hidden;
            if (ordinaryDialog)
            {
                var dialog = (Kingmaker.UI.Dialog.DialogController)value;
                var rect = value.transform as RectTransform;
                row["hidePosY"] = dialog.HidePosY;
                row["showPosY"] = dialog.ShowPosY;
                row["anchoredPosition"] = rect == null ? null : new JArray(rect.anchoredPosition.x, rect.anchoredPosition.y);
            }
            return hidden;
        }

        private static string Cleanup2NarrativePath(Transform root, Transform value)
        {
            var parts = new List<string>();
            for (Transform current = value; current != null; current = current.parent)
            {
                parts.Add(current.name + "[" + current.GetSiblingIndex() + "]");
                if (current == root) break;
            }
            parts.Reverse();
            return string.Join("/", parts.ToArray());
        }

        private IEnumerable<int> Cleanup2Menus(ExpandedSummoningCorrectionFixture fixture, Spellbook book)
        {
            var game = Game.Instance;
            var player = game.Player;
            var ui = game.UI;
            if (!Cleanup2NoNarrativeOverlay())
                throw new InvalidOperationException("Native narrative overlay is active or unverified; menu observation refuses without choosing a story answer.");
            if (ActionBarManager.Instance?.Group == null || ui?.SelectionManagerPC == null || GroupController.Instance == null)
                throw new InvalidOperationException("Native action-bar/party selection UI is unavailable.");
            // Native Dialog and other non-casting modes intentionally expose
            // no selected-ability handler. Opening the real group/level/variant
            // controls does not select a casting ability or require that
            // handler; do not initialize another mode merely for observation.
            var selectedAbilityHandler = game.SelectedAbilityHandler;
            var pointerController = game.ClickEventsController;
            UnitEntityData[] party = player.Party.ToArray();
            UnitEntityData[] selected = ui.SelectionManagerPC.SelectedUnits.ToArray();
            UnitEntityData groupCharacter = GroupController.Instance.GetCurrentCharacter();
            var groups = ActionBarManager.Instance.Group.GroupElements.ToArray();
            bool[] toggles = groups.Select(g => g.ToggleState).ToArray();
            if (toggles.Any(value => value) || selectedAbilityHandler?.Ability != null ||
                (pointerController != null && pointerController.Mode != Kingmaker.Controllers.Clicks.PointerMode.Default))
                throw new InvalidOperationException("Cleanup 2 requires idle closed native groups, no selected ability and any exposed pointer in Default mode before claiming UI ownership.");
            Cleanup2Check("ui-input-intake", true, new JObject {
                ["mode"] = game.CurrentMode.ToString(),
                ["narrativeOverlayAbsent"] = Cleanup2NoNarrativeOverlay(),
                ["selectedAbilityHandlerPresent"] = selectedAbilityHandler != null,
                ["pointerControllerPresent"] = pointerController != null,
                ["pointerMode"] = pointerController?.Mode.ToString(),
                ["selectedAbility"] = selectedAbilityHandler?.Ability?.Blueprint.AssetGuid,
                ["openGroups"] = toggles.Count(value => value),
                ["ownership"] = "Native group/level/variant controls only; no casting input controller or game mode is initialized. Absent intake controllers are not claimed for cleanup." },
                "Native action groups are closed, and any exposed selected-ability handler is idle; optional casting controllers are recorded exactly.");
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
                    if (!Cleanup2NoNarrativeOverlay())
                        throw new InvalidOperationException("Narrative overlay appeared before owned menu tier " + tier + "; no answer is selected.");
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
                bool inputControllersExact = ReferenceEquals(game.ClickEventsController, pointerController) &&
                    ReferenceEquals(game.SelectedAbilityHandler, selectedAbilityHandler);
                if (pointerController != null && ReferenceEquals(game.ClickEventsController, pointerController))
                    restore(() => pointerController.ClearPointerMode());
                if (selectedAbilityHandler != null && ReferenceEquals(game.SelectedAbilityHandler, selectedAbilityHandler))
                    restore(() => selectedAbilityHandler.DropAbility());
                restore(() => { if (added) player.PartyCharacters.Remove(fixture.Caster); });
                restore(() => { player.InvalidateCharacterLists(); player.UpdateCharacterLists(); });
                restore(() => GroupController.Instance.SelectUnit(groupCharacter));
                restore(() => ui.SelectionManagerPC.MultiSelect(selected.Where(u => u?.View != null).Select(u => u.View).ToArray(), false));
                for (int i = 0; i < groups.Length; i++) { int index = i; restore(() => groups[index].Toggle(toggles[index], true)); }
                bool slotsExact = false;
                restore(() => slotsExact = slotsBefore.All(snapshot => snapshot.Unchanged()));
                Cleanup2Check("ui-party-restored", failures.Count == 0 && inputControllersExact && player.Party.SequenceEqual(party) && slotsExact,
                    new JObject { ["partyExact"] = player.Party.SequenceEqual(party),
                        ["inputControllerReferencesExact"] = inputControllersExact,
                        ["intakeSelectedAbilityHandlerPresent"] = selectedAbilityHandler != null,
                        ["intakePointerControllerPresent"] = pointerController != null,
                        ["currentSelectedAbilityHandlerPresent"] = game.SelectedAbilityHandler != null,
                        ["currentPointerControllerPresent"] = game.ClickEventsController != null,
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
            if (!Cleanup2NoNarrativeOverlay()) throw new InvalidOperationException("Native menu capture refuses an active or unknown narrative overlay.");
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
            if (rows.Length == 0 || rows.Length > 1024 || lineage.Count > 8)
                throw new InvalidOperationException("Native popup row/depth bound exceeded: " + label);
            Canvas.ForceUpdateCanvases();
            var scroll = Cleanup2AcquirePopupSurface(source, popup, parent, rows, label);
            Component scrollComponent = scroll.Component;
            RectTransform content = scroll.Content, viewport = Cleanup2PopupViewport(scroll);
            Vector2 before = scroll.IsStationary ? Vector2.zero : scroll.Position;
            Vector2 beforeVelocity = scroll.IsStationary ? Vector2.zero : scroll.Velocity;
            Vector2 beforeAnchored = content.anchoredPosition;
            bool beforeHorizontal = scroll.Horizontal, beforeVertical = scroll.Vertical;
            var covered = new HashSet<string>(StringComparer.Ordinal);
            var pages = new JArray();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            Func<bool> owns = () => Cleanup2NoNarrativeOverlay() && popup != null && popup.gameObject.activeInHierarchy &&
                scroll.Component == scrollComponent && scroll.Active && scroll.Content == content &&
                Cleanup2PopupViewport(scroll) == viewport && scroll.Horizontal == beforeHorizontal && scroll.Vertical == beforeVertical &&
                rows.All(row => row != null && row.gameObject.activeInHierarchy && row.transform.IsChildOf(content) &&
                    (row.MechanicSlot?.GetContentData() as AbilityData)?.ConvertedFrom?.Blueprint == parent) &&
                ReferenceEquals(GroupController.Instance.GetCurrentCharacter(), _sprint25Fixture.Caster);
            try
            {
                for (int page = 0; page < 64 && covered.Count < expected.Length; page++)
                {
                    if (watch.Elapsed.TotalSeconds > 120) throw new TimeoutException("Native popup page wall bound: " + label);
                    ActionBarSlot target = rows.First(row => !covered.Contains(((AbilityData)row.MechanicSlot.GetContentData()).Blueprint.AssetGuid));
                    foreach (int frame in Cleanup2RevealPopupRow(scroll, target, owns)) yield return frame;
                    var visible = rows.Where(r => Cleanup2FullyVisible(scroll, r)).ToArray();
                    Vector2 capturePosition = content.anchoredPosition;
                    var state = new JObject { ["parent"] = parent.AssetGuid, ["label"] = label,
                        ["page"] = page, ["target"] = ((AbilityData)target.MechanicSlot.GetContentData()).Blueprint.AssetGuid,
                        ["scrollType"] = scrollComponent.GetType().FullName, ["scrollInstance"] = scrollComponent.GetInstanceID(),
                        ["surfaceMode"] = scroll.IsStationary ? "stationary-native-popup" : "owned-overflow-scroll",
                        ["scrollApi"] = scroll.Api, ["scrollMutationRequested"] = !scroll.IsStationary,
                        ["horizontalEnabled"] = beforeHorizontal, ["verticalEnabled"] = beforeVertical,
                        ["normalizedScroll"] = scroll.IsStationary ? null : new JArray(scroll.Position.x, scroll.Position.y),
                        ["anchoredPosition"] = new JArray(capturePosition.x, capturePosition.y),
                        ["viewportRect"] = new JArray(viewport.rect.xMin, viewport.rect.yMin, viewport.rect.xMax, viewport.rect.yMax),
                        ["contentBoundsInViewport"] = Cleanup2PopupBoundsDocument(Cleanup2PopupBounds(viewport, content)),
                        ["targetGeometry"] = Cleanup2PopupBoundsDocument(Cleanup2PopupBounds(viewport, (RectTransform)target.transform)),
                        ["visible"] = new JArray(visible.Select(r => {
                            var data = (AbilityData)r.MechanicSlot.GetContentData();
                            return new JObject { ["guid"] = data.Blueprint.AssetGuid, ["internalName"] = data.Blueprint.name,
                                ["renderedText"] = new JArray(r.GetComponentsInChildren<TextMeshProUGUI>(true).Select(t => t.text)),
                                ["iconMatches"] = ReferenceEquals(r.Icon.sprite, data.Icon),
                                ["iconInstance"] = r.Icon.sprite == null ? 0 : r.Icon.sprite.GetInstanceID(),
                                ["boundsInViewport"] = Cleanup2PopupBoundsDocument(Cleanup2PopupBounds(viewport, (RectTransform)r.transform)),
                                ["convertedFrom"] = data.ConvertedFrom?.Blueprint.AssetGuid,
                                ["spellbook"] = data.Spellbook?.Blueprint.AssetGuid,
                                ["nestedVariants"] = data.Blueprint.Variants == null ? 0 : data.Blueprint.Variants.Length };
                        })) };
                    foreach (int frame in screens.Capture(label + "-page" + page, state, owns)) yield return frame;
                    bool stable = owns() && Vector2.Distance(content.anchoredPosition, capturePosition) <= .01f &&
                        visible.All(row => Cleanup2FullyVisible(scroll, row));
                    int previouslyCovered = covered.Count;
                    if (stable) foreach (var row in visible) covered.Add(((AbilityData)row.MechanicSlot.GetContentData()).Blueprint.AssetGuid);
                    pages.Add(new JObject { ["page"] = page, ["stableAcrossNativeCapture"] = stable,
                        ["targetFullyVisible"] = Cleanup2FullyVisible(scroll, target), ["newlyCovered"] = covered.Count - previouslyCovered,
                        ["totalCovered"] = covered.Count, ["geometry"] = state });
                    if (!stable || !Cleanup2FullyVisible(scroll, target) || covered.Count == previouslyCovered)
                        break; // Preserve this real page, then fail explicit coverage below.
                }
                bool complete = expected.All(v => covered.Contains(v.AssetGuid));
                Cleanup2Check("popup-full-page-coverage-" + label, complete,
                    new JObject { ["parent"] = parent.AssetGuid, ["pages"] = pages.Count, ["pageObservations"] = pages,
                        ["pageLimit"] = 64, ["wallLimitSeconds"] = 120, ["wallSeconds"] = watch.Elapsed.TotalSeconds,
                        ["fullyVisibleChildren"] = new JArray(covered),
                        ["missing"] = new JArray(expected.Where(v => !covered.Contains(v.AssetGuid)).Select(v => v.AssetGuid)),
                        ["missingGeometry"] = new JArray(rows.Where(row => !covered.Contains(((AbilityData)row.MechanicSlot.GetContentData()).Blueprint.AssetGuid))
                            .Select(row => new JObject { ["guid"] = ((AbilityData)row.MechanicSlot.GetContentData()).Blueprint.AssetGuid,
                                ["boundsInViewport"] = Cleanup2PopupBoundsDocument(Cleanup2PopupBounds(viewport, (RectTransform)row.transform)) })) },
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
                        foreach (int frame in Cleanup2RevealPopupRow(scroll, row, owns)) yield return frame;
                        if (!Cleanup2FullyVisible(scroll, row)) throw new InvalidOperationException("Nested native group row cannot be fully revealed: " + child.AssetGuid);
                        if (!Cleanup2NoNarrativeOverlay()) throw new InvalidOperationException("Nested native group refuses an active or unknown narrative overlay.");
                        childGroup.OnToggleGroupClick();
                        for (int frame = 0; frame < 12; frame++) yield return 0;
                        foreach (int frame in Cleanup2CapturePopup(childGroup, child, label + "-" + child.AssetGuid,
                            screens, new HashSet<string>(lineage))) yield return frame;
                    }
                    finally { childGroup.HideSubGroup(); }
                }
            }
            finally
            {
                bool identities = scroll.Component == scrollComponent && scrollComponent != null && scroll.Active && scroll.Content == content &&
                    content != null && Cleanup2PopupViewport(scroll) == viewport;
                bool restored = false;
                try
                {
                    if (identities && scroll.Horizontal == beforeHorizontal && scroll.Vertical == beforeVertical)
                    {
                        if (scroll.IsStationary)
                            restored = scroll.Active && content.anchoredPosition.Equals(beforeAnchored);
                        else
                        {
                            scroll.Velocity = Vector2.zero;
                            Cleanup2SetPopupPosition(scroll, before);
                            // A fitting disabled axis has no meaningful normalized
                            // ratio. Restore the exact owned content position too.
                            content.anchoredPosition = beforeAnchored;
                            scroll.Velocity = beforeVelocity;
                            Bounds bounds = Cleanup2PopupBounds(viewport, content);
                            restored = content.anchoredPosition.Equals(beforeAnchored) && scroll.Velocity.Equals(beforeVelocity) &&
                                (!beforeHorizontal || bounds.size.x <= viewport.rect.width + .5f || Mathf.Abs(scroll.Position.x - before.x) <= .001f) &&
                                (!beforeVertical || bounds.size.y <= viewport.rect.height + .5f || Mathf.Abs(scroll.Position.y - before.y) <= .001f);
                        }
                    }
                }
                finally
                {
                    Cleanup2Check("popup-scroll-restored-" + label, restored,
                        new JObject { ["parent"] = parent.AssetGuid, ["identitiesExact"] = identities,
                            ["horizontalAtIntake"] = beforeHorizontal, ["verticalAtIntake"] = beforeVertical,
                            ["surfaceMode"] = scroll.IsStationary ? "stationary-native-popup" : "owned-overflow-scroll",
                            ["scrollStateExisted"] = !scroll.IsStationary,
                            ["stationaryPopupUnchangedWithoutWrites"] = scroll.IsStationary ? (JToken)restored : JValue.CreateNull(),
                            ["anchoredPositionAndVelocityExact"] = scroll.IsStationary ? JValue.CreateNull() : (JToken)restored,
                            ["geometryOrAxesFabricated"] = false },
                        "A fitting native popup is unchanged without writes; an overflowing popup restores only its exact owned scroll position/velocity and content position, retaining axes and viewport identities.");
                }
                if (!restored) throw new InvalidOperationException("Exact native popup scroll restoration failed: " + label);
            }
        }

        private NativeIconScreenEvidence.NativeRowScroll Cleanup2AcquirePopupSurface(ActionBarGroupSlot source,
            ActionBarSpellsGroup popup, BlueprintAbility parent, ActionBarSlot[] rows, string label)
        {
            ExpandedSummoningVariantMenuSnapshot snapshot;
            bool captured = ExpandedSummoningVariantMenuRuntime.TryGetSnapshot(popup, out snapshot);
            var root = popup == null ? null : popup.transform as RectTransform;
            var canvas = popup == null ? null : popup.GetComponentInParent<Canvas>()?.rootCanvas;
            var canvasRect = canvas == null ? null : canvas.transform as RectTransform;
            var sourceData = source?.MechanicSlot?.GetContentData() as AbilityData;
            var sourceField = typeof(ActionBarSpellsGroup).GetField("m_SourceSpell", Cleanup2Fields);
            var slotsField = typeof(ActionBarSpellsGroup).GetField("m_Slots", Cleanup2Fields);
            var groupField = typeof(ActionBarGroupSlot).GetField("SubGroup", Cleanup2Fields);
            var nativeSlots = popup == null || slotsField == null ? null :
                slotsField.GetValue(popup) as IList<ActionBarSpontaneousConvertedSlot>;
            var capturedSlots = nativeSlots == null ? new ActionBarSpontaneousConvertedSlot[0] : nativeSlots.ToArray();
            var capturedData = rows.Select(row => row.MechanicSlot?.GetContentData() as AbilityData).ToArray();
            var rowBlueprints = capturedData.Select(data => data?.Blueprint).ToArray();
            var rowSources = capturedData.Select(data => data?.ConvertedFrom).ToArray();
            var rowBooks = capturedData.Select(data => data?.Spellbook).ToArray();
            var rowSiblingIndices = rows.Select(row => row.transform.GetSiblingIndex()).ToArray();
            var variants = parent.Variants == null ? new BlueprintAbility[0] : parent.Variants.ToArray();
            string sourceIdentity = source == null ? null : source.GetType().FullName + "#" +
                source.GetInstanceID().ToString(System.Globalization.CultureInfo.InvariantCulture);
            var rootScrolls = popup == null ? new ScrollRect[0] : popup.GetComponents<ScrollRect>();
            var markers = popup == null ? new ExpandedSummoningVariantMenuViewportMarker[0] :
                popup.GetComponentsInChildren<ExpandedSummoningVariantMenuViewportMarker>(true);
            ScrollRect ownedScroll = rootScrolls.Length == 1 ? rootScrolls[0] : null;
            RectTransform ownedViewport = markers.Length == 1 ? markers[0].transform as RectTransform : null;
            RectTransform ownedContent = ownedScroll == null ? null : ownedScroll.content;
            bool slotSetExact = rows.Length > 0 && rows.Length <= 1024 && capturedSlots.Length == rows.Length &&
                capturedSlots.All(value => value != null && rows.Count(row => ReferenceEquals(row, value)) == 1) &&
                variants.Count(value => value != null) == rows.Length && variants.Where(value => value != null)
                    .All(value => capturedData.Count(data => data?.Blueprint == value) == 1);
            Vector3 rootPosition = root == null ? Vector3.zero : root.anchoredPosition3D;
            Vector3 rootScale = root == null ? Vector3.zero : root.localScale;
            Quaternion rootRotation = root == null ? Quaternion.identity : root.localRotation;
            Vector2 rootSize = root == null ? Vector2.zero : root.sizeDelta;
            Vector2 rootAnchorMin = root == null ? Vector2.zero : root.anchorMin;
            Vector2 rootAnchorMax = root == null ? Vector2.zero : root.anchorMax;
            Vector2 rootPivot = root == null ? Vector2.zero : root.pivot;
            Func<bool> proof = () => {
                try
                {
                    ExpandedSummoningVariantMenuSnapshot current;
                    if (!captured || snapshot == null || root == null || canvasRect == null || source == null ||
                        popup == null || !popup.gameObject.activeInHierarchy || !source.gameObject.activeInHierarchy ||
                        sourceField == null || slotsField == null || groupField == null || sourceData == null ||
                        sourceData.Blueprint != parent || !ReferenceEquals(source.MechanicSlot?.GetContentData(), sourceData) ||
                        !ReferenceEquals(sourceField.GetValue(popup), sourceData) || !ReferenceEquals(groupField.GetValue(source), popup) ||
                        !ReferenceEquals(slotsField.GetValue(popup), nativeSlots) || nativeSlots == null ||
                        !nativeSlots.SequenceEqual(capturedSlots) || !slotSetExact ||
                        !ExpandedSummoningVariantMenuRuntime.TryGetSnapshot(popup, out current) || !ReferenceEquals(current, snapshot) ||
                        snapshot.ParentGuid != parent.AssetGuid || snapshot.ParentVariantCount != variants.Length ||
                        parent.Variants == null || !parent.Variants.SequenceEqual(variants) || snapshot.SlotCount != rows.Length ||
                        !snapshot.ExactSourceSlotCaptured || snapshot.SourceSlotIdentity != sourceIdentity ||
                        snapshot.SourceSlotHierarchyPath != Cleanup2PopupHierarchy(source.transform) ||
                        snapshot.ScrollRectCount != rootScrolls.Length || snapshot.ViewportMarkerCount != markers.Length ||
                        !popup.GetComponents<ScrollRect>().SequenceEqual(rootScrolls) ||
                        !popup.GetComponentsInChildren<ExpandedSummoningVariantMenuViewportMarker>(true).SequenceEqual(markers) ||
                        popup.GetComponentsInChildren<Kingmaker.UI.Common.ScrollRectExtended>(true).Length != 0 ||
                        popup.GetComponentsInChildren<ScrollRect>(true).Length != rootScrolls.Length ||
                        rows.Where((row, index) => row == null || !row.gameObject.activeInHierarchy || capturedData[index] == null ||
                            !ReferenceEquals(row.MechanicSlot?.GetContentData(), capturedData[index]) ||
                            capturedData[index].Blueprint != rowBlueprints[index] ||
                            !ReferenceEquals(capturedData[index].ConvertedFrom, rowSources[index]) ||
                            !ReferenceEquals(capturedData[index].Spellbook, rowBooks[index]) ||
                            row.transform.GetSiblingIndex() != rowSiblingIndices[index] ||
                            capturedData[index].ConvertedFrom?.Blueprint != parent).Any()) return false;
                    SummonVariantMenuRect liveRoot = Cleanup2PopupCanvasRect(canvasRect, root);
                    if (!Cleanup2PopupRectMatches(snapshot.RenderedPopupRect, liveRoot) ||
                        !snapshot.SafeRect.Contains(liveRoot, .5f) ||
                        !Cleanup2PopupRectMatches(snapshot.SourceSlotRect,
                            Cleanup2PopupCanvasRect(canvasRect, source.transform as RectTransform))) return false;
                    bool scaffold = ownedScroll != null && ownedViewport != null && ownedContent != null &&
                        ownedViewport.parent == root && ownedContent.parent == ownedViewport &&
                        ownedScroll.viewport == ownedViewport && ownedScroll.content == ownedContent;
                    if (snapshot.ScrollingRequired)
                        return snapshot.ScrollingInstalled && snapshot.HasViewportRect && scaffold &&
                            ownedScroll.isActiveAndEnabled && ownedScroll.movementType == ScrollRect.MovementType.Clamped &&
                            ownedScroll.horizontal == snapshot.HorizontalScrolling && ownedScroll.vertical == snapshot.VerticalScrolling &&
                            (ownedScroll.horizontal || ownedScroll.vertical) &&
                            ownedViewport.GetComponent<RectMask2D>()?.isActiveAndEnabled == true &&
                            Cleanup2PopupRectMatches(snapshot.ViewportRect, Cleanup2PopupCanvasRect(canvasRect, ownedViewport)) &&
                            rows.All(row => row.transform.parent == ownedContent);
                    // A previously used owned scaffold may remain disabled on a
                    // pooled popup. It is not scroll state for this opening.
                    if (snapshot.ScrollingInstalled || snapshot.VerticalScrolling || snapshot.HorizontalScrolling ||
                        rootScrolls.Length > 1 || markers.Length > 1 ||
                        (rootScrolls.Length == 0 ? markers.Length != 0 :
                            !scaffold || ownedScroll.enabled || ownedViewport.gameObject.activeInHierarchy) ||
                        !root.anchoredPosition3D.Equals(rootPosition) || !root.localScale.Equals(rootScale) ||
                        !root.localRotation.Equals(rootRotation) || !root.sizeDelta.Equals(rootSize) ||
                        !root.anchorMin.Equals(rootAnchorMin) || !root.anchorMax.Equals(rootAnchorMax) || !root.pivot.Equals(rootPivot)) return false;
                    // Installed FillSlots parents each exact m_Slots row directly
                    // to this root. All rows must already fit its actual bounds
                    // and the measured safe rectangle; no substitute ancestor.
                    return snapshot.SlotContentRect.Width > 0f && snapshot.SlotContentRect.Height > 0f &&
                        rows.All(row => row.transform.parent == root &&
                            liveRoot.Contains(Cleanup2PopupCanvasRect(canvasRect, row.transform as RectTransform), .5f) &&
                            snapshot.SafeRect.Contains(Cleanup2PopupCanvasRect(canvasRect, row.transform as RectTransform), .5f));
                }
                catch { return false; }
            };
            bool admitted = proof();
            Cleanup2Check("popup-native-scroll-" + label, admitted,
                Cleanup2PopupSurfaceDiagnostic(source, popup, parent, rows, snapshot, admitted ? "admitted" : "admission-refused"),
                "The exact live source/snapshot/native slot identities admit either a fitting stationary popup or its proven owned overflow viewport; no ancestor or scroll object is invented.");
            if (!admitted) throw new InvalidOperationException("Exact native popup surface proof failed: " + label);
            bool failureRecorded = false;
            Func<bool> guardedProof = () => {
                bool intact = proof();
                if (!intact && !failureRecorded)
                {
                    failureRecorded = true;
                    Cleanup2Check("popup-surface-ownership-" + label, false,
                        Cleanup2PopupSurfaceDiagnostic(source, popup, parent, rows, snapshot, "ownership-refused"),
                        "The captured native popup/source/layout/row identities and geometry must remain exact during observation.");
                }
                return intact;
            };
            return snapshot.ScrollingRequired
                ? new NativeIconScreenEvidence.NativeRowScroll(ownedScroll, guardedProof)
                : NativeIconScreenEvidence.NativeRowScroll.StationaryPopup(root, guardedProof);
        }

        private static SummonVariantMenuRect Cleanup2PopupCanvasRect(RectTransform canvas, RectTransform value)
        {
            if (canvas == null || value == null) throw new InvalidOperationException("Exact native RectTransforms required.");
            Bounds bounds = Cleanup2PopupBounds(canvas, value);
            return new SummonVariantMenuRect(bounds.min.x, bounds.min.y, bounds.size.x, bounds.size.y);
        }

        private static bool Cleanup2PopupRectMatches(SummonVariantMenuRect first, SummonVariantMenuRect second)
        { return Mathf.Abs(first.XMin - second.XMin) <= .5f && Mathf.Abs(first.XMax - second.XMax) <= .5f &&
            Mathf.Abs(first.YMin - second.YMin) <= .5f && Mathf.Abs(first.YMax - second.YMax) <= .5f; }

        private static string Cleanup2PopupHierarchy(Transform value)
        {
            var parts = new List<string>();
            for (Transform current = value; current != null; current = current.parent)
                parts.Add(current.name + "#" + current.GetInstanceID().ToString(System.Globalization.CultureInfo.InvariantCulture));
            parts.Reverse(); return string.Join("/", parts.ToArray());
        }

        private static JObject Cleanup2PopupSurfaceDiagnostic(ActionBarGroupSlot source, ActionBarSpellsGroup popup,
            BlueprintAbility parent, ActionBarSlot[] rows, ExpandedSummoningVariantMenuSnapshot snapshot, string disposition)
        {
            var result = new JObject { ["parent"] = parent?.AssetGuid, ["rowCount"] = rows.Length, ["disposition"] = disposition,
                ["sourceInstance"] = source == null ? 0 : source.GetInstanceID(),
                ["popupInstance"] = popup == null ? 0 : popup.GetInstanceID(),
                ["snapshot"] = snapshot?.ToString(), ["scrollingRequired"] = snapshot == null ? JValue.CreateNull() : (JToken)snapshot.ScrollingRequired,
                ["diagnosticRowLimit"] = 64, ["diagnosticHierarchyLimit"] = 12, ["geometryOrAxesFabricated"] = false };
            try
            {
                var canvas = popup == null ? null : popup.GetComponentInParent<Canvas>()?.rootCanvas;
                var canvasRect = canvas == null ? null : canvas.transform as RectTransform;
                var root = popup == null ? null : popup.transform as RectTransform;
                var sourceData = source?.MechanicSlot?.GetContentData() as AbilityData;
                var sourceField = typeof(ActionBarSpellsGroup).GetField("m_SourceSpell", Cleanup2Fields);
                var groupField = typeof(ActionBarGroupSlot).GetField("SubGroup", Cleanup2Fields);
                var slotsField = typeof(ActionBarSpellsGroup).GetField("m_Slots", Cleanup2Fields);
                var nativeSource = popup == null || sourceField == null ? null : sourceField.GetValue(popup) as AbilityData;
                var nativeSlots = popup == null || slotsField == null ? null : slotsField.GetValue(popup) as IList<ActionBarSpontaneousConvertedSlot>;
                ExpandedSummoningVariantMenuSnapshot current;
                result["currentSnapshotIsCaptured"] = ExpandedSummoningVariantMenuRuntime.TryGetSnapshot(popup, out current) && ReferenceEquals(current, snapshot);
                result["sourceSlotIdentityNow"] = source == null ? null : source.GetType().FullName + "#" +
                    source.GetInstanceID().ToString(System.Globalization.CultureInfo.InvariantCulture);
                result["sourceHierarchyNow"] = source == null ? null : Cleanup2PopupHierarchy(source.transform);
                result["sourceBlueprint"] = sourceData?.Blueprint.AssetGuid;
                result["nativeSourceBlueprint"] = nativeSource?.Blueprint.AssetGuid;
                result["nativeSourceIsExactSourceAbility"] = sourceData != null && ReferenceEquals(sourceData, nativeSource);
                result["sourceOwnsExactPopup"] = source != null && groupField != null && ReferenceEquals(groupField.GetValue(source), popup);
                result["nativeSlotCount"] = nativeSlots?.Count;
                result["nativeSlotInstances"] = nativeSlots == null ? null : new JArray(nativeSlots.Take(64).Select(value => value == null ? 0 : value.GetInstanceID()));
                result["rootScrollRectCount"] = popup == null ? 0 : popup.GetComponents<ScrollRect>().Length;
                result["ownedViewportMarkerCount"] = popup == null ? 0 : popup.GetComponentsInChildren<ExpandedSummoningVariantMenuViewportMarker>(true).Length;
                var hierarchy = new JArray(); result["hierarchy"] = hierarchy;
                for (Transform node = root; node != null && hierarchy.Count < 12; node = node.parent)
                    hierarchy.Add(new JObject { ["name"] = node.name, ["instance"] = node.GetInstanceID(),
                        ["active"] = node.gameObject.activeInHierarchy,
                        ["components"] = new JArray(node.GetComponents<Component>().Where(value => value != null).Take(24).Select(value => value.GetType().FullName)) });
                if (canvasRect != null && root != null)
                {
                    result["livePopupInCanvas"] = Cleanup2PopupBoundsDocument(Cleanup2PopupBounds(canvasRect, root));
                    result["sourceInCanvas"] = source == null ? null : Cleanup2PopupBoundsDocument(Cleanup2PopupBounds(canvasRect, (RectTransform)source.transform));
                    result["rows"] = new JArray(rows.Take(64).Select(row => new JObject {
                        ["instance"] = row == null ? 0 : row.GetInstanceID(),
                        ["guid"] = (row?.MechanicSlot?.GetContentData() as AbilityData)?.Blueprint.AssetGuid,
                        ["active"] = row != null && row.gameObject.activeInHierarchy,
                        ["directParent"] = row == null ? null : row.transform.parent?.name,
                        ["directParentInstance"] = row?.transform.parent == null ? 0 : row.transform.parent.GetInstanceID(),
                        ["boundsInPopup"] = row == null ? null : Cleanup2PopupBoundsDocument(Cleanup2PopupBounds(root, (RectTransform)row.transform)),
                        ["boundsInCanvas"] = row == null ? null : Cleanup2PopupBoundsDocument(Cleanup2PopupBounds(canvasRect, (RectTransform)row.transform)),
                        ["screenCorners"] = row == null ? null : Cleanup2PopupScreenCorners((RectTransform)row.transform, canvas),
                        ["masks"] = row == null ? null : new JArray(row.GetComponentsInParent<RectMask2D>(true).Cast<Behaviour>()
                            .Concat(row.GetComponentsInParent<Mask>(true)).Take(12).Select(mask => new JObject {
                            ["name"] = mask.name, ["instance"] = mask.GetInstanceID(), ["type"] = mask.GetType().FullName,
                            ["active"] = mask.isActiveAndEnabled,
                            ["maskRectInCanvas"] = Cleanup2PopupBoundsDocument(Cleanup2PopupBounds(canvasRect, (RectTransform)mask.transform)),
                            ["rowBoundsInMask"] = Cleanup2PopupBoundsDocument(Cleanup2PopupBounds((RectTransform)mask.transform, (RectTransform)row.transform)) })) }));
                }
                result["surfaces"] = popup == null ? null : new JArray(popup.GetComponentsInChildren<ScrollRect>(true).Take(8).Select(value => new JObject {
                    ["type"] = value.GetType().FullName, ["instance"] = value.GetInstanceID(), ["active"] = value.isActiveAndEnabled,
                    ["content"] = value.content?.name, ["viewport"] = value.viewport?.name,
                    ["horizontal"] = value.horizontal, ["vertical"] = value.vertical }));
            }
            catch (Exception error) { result["diagnosticReadFailure"] = error.ToString(); }
            return result;
        }

        private static JArray Cleanup2PopupScreenCorners(RectTransform rect, Canvas canvas)
        {
            Camera camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay && camera == null) return null;
            var corners = new Vector3[4]; rect.GetWorldCorners(corners);
            return new JArray(corners.Select(point => { Vector2 screen = RectTransformUtility.WorldToScreenPoint(camera, point);
                return new JObject { ["x"] = screen.x, ["y"] = screen.y,
                    ["cameraDepth"] = camera == null ? JValue.CreateNull() : (JToken)camera.WorldToScreenPoint(point).z,
                    ["screenWidth"] = Screen.width, ["screenHeight"] = Screen.height }; }));
        }

        private static RectTransform Cleanup2PopupViewport(NativeIconScreenEvidence.NativeRowScroll scroll)
        {
            if (scroll.Viewport != null) return scroll.Viewport;
            // Installed ScrollRectExtended.get_viewRect0600417C uses its own
            // RectTransform when the serialized viewport is absent.
            return scroll.Component is Kingmaker.UI.Common.ScrollRectExtended ? scroll.Component.transform as RectTransform : null;
        }

        private static Bounds Cleanup2PopupBounds(RectTransform viewport, RectTransform target)
        {
            var corners = new Vector3[4]; target.GetWorldCorners(corners);
            var bounds = new Bounds(viewport.InverseTransformPoint(corners[0]), Vector3.zero);
            foreach (Vector3 corner in corners.Skip(1)) bounds.Encapsulate(viewport.InverseTransformPoint(corner));
            return bounds;
        }

        private static JObject Cleanup2PopupBoundsDocument(Bounds bounds)
        { return new JObject { ["minX"] = bounds.min.x, ["maxX"] = bounds.max.x, ["minY"] = bounds.min.y, ["maxY"] = bounds.max.y }; }

        private static void Cleanup2SetPopupPosition(NativeIconScreenEvidence.NativeRowScroll scroll, Vector2 position)
        {
            if (scroll.IsStationary) throw new InvalidOperationException("A fitting native popup has no scroll state to write.");
            var standard = scroll.Component as ScrollRect;
            if (standard != null)
            {
                if (scroll.Vertical) standard.verticalNormalizedPosition = position.y;
                if (scroll.Horizontal) standard.horizontalNormalizedPosition = position.x;
                return;
            }
            var extended = scroll.Component as Kingmaker.UI.Common.ScrollRectExtended;
            if (extended == null) throw new NotSupportedException("Unknown native popup scroll API.");
            if (scroll.Vertical) extended.verticalNormalizedPosition = position.y;
            if (scroll.Horizontal) extended.horizontalNormalizedPosition = position.x;
        }

        private IEnumerable<int> Cleanup2RevealPopupRow(NativeIconScreenEvidence.NativeRowScroll scroll, ActionBarSlot row, Func<bool> owns)
        {
            if (!owns()) throw new InvalidOperationException("Native popup scroll ownership changed before reveal.");
            Canvas.ForceUpdateCanvases();
            RectTransform viewport = Cleanup2PopupViewport(scroll);
            Bounds content = Cleanup2PopupBounds(viewport, scroll.Content), target = Cleanup2PopupBounds(viewport, (RectTransform)row.transform);
            Vector2 position = scroll.IsStationary ? Vector2.zero : scroll.Position;
            float xRange = content.size.x - viewport.rect.width, yRange = content.size.y - viewport.rect.height;
            if (!scroll.IsStationary && !Cleanup2FullyVisible(scroll, row))
            {
                if (scroll.Horizontal && xRange > .5f) position.x = Mathf.Clamp01(position.x + (target.center.x - viewport.rect.center.x) / xRange);
                if (scroll.Vertical && yRange > .5f) position.y = Mathf.Clamp01(position.y + (target.center.y - viewport.rect.center.y) / yRange);
            }
            if (!scroll.IsStationary)
            {
                scroll.Velocity = Vector2.zero;
                Cleanup2SetPopupPosition(scroll, position);
            }
            var watch = System.Diagnostics.Stopwatch.StartNew();
            Vector2 previous = scroll.Content.anchoredPosition;
            int stableFrames = 0;
            for (int frame = 0; frame < 240 && watch.Elapsed.TotalSeconds < 2; frame++)
            {
                yield return 0;
                if (!owns()) throw new InvalidOperationException("Native popup scroll ownership changed during reveal.");
                Vector2 current = scroll.Content.anchoredPosition;
                stableFrames = Vector2.Distance(previous, current) <= .01f ? stableFrames + 1 : 0;
                previous = current;
                if (frame >= 5 && stableFrames >= 3 && watch.Elapsed.TotalSeconds >= .05) yield break;
            }
            throw new TimeoutException("Native popup scroll did not settle within 240 frames/2 seconds.");
        }

        private static bool Cleanup2FullyVisible(NativeIconScreenEvidence.NativeRowScroll scroll, ActionBarSlot row)
        {
            var rect = row.transform as RectTransform;
            if (rect == null || !row.gameObject.activeInHierarchy) return false;
            RectTransform viewport = Cleanup2PopupViewport(scroll);
            if (viewport == null) return false;
            var clips = new List<RectTransform> { viewport };
            clips.AddRange(row.GetComponentsInParent<RectMask2D>(true).Where(value => value.isActiveAndEnabled).Select(value => value.transform as RectTransform));
            clips.AddRange(row.GetComponentsInParent<Mask>(true).Where(value => value.isActiveAndEnabled).Select(value => value.transform as RectTransform));
            foreach (RectTransform clip in clips.Where(value => value != null).Distinct())
            {
                Bounds bounds = Cleanup2PopupBounds(clip, rect); Rect area = clip.rect;
                if (bounds.min.y < area.yMin - .5f || bounds.max.y > area.yMax + .5f ||
                    bounds.min.x < area.xMin - .5f || bounds.max.x > area.xMax + .5f) return false;
            }
            Canvas canvas = row.GetComponentInParent<Canvas>()?.rootCanvas;
            if (canvas == null) return false;
            Camera camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay && camera == null) return false;
            var corners = new Vector3[4]; rect.GetWorldCorners(corners);
            return corners.All(point => {
                Vector2 screen = RectTransformUtility.WorldToScreenPoint(camera, point);
                return screen.x >= -.5f && screen.x <= Screen.width + .5f && screen.y >= -.5f && screen.y <= Screen.height + .5f &&
                    (camera == null || camera.WorldToScreenPoint(point).z > 0f);
            });
        }
    }
}
