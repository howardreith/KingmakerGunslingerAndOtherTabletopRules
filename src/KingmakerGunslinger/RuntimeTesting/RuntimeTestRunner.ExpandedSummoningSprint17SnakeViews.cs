using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // The existing closed working-save body request only. These are the
        // registered hidden snakes, not stat-mutated donor stand-ins. No
        // manual attachment, visibility forcing, private save access or write.
        private IEnumerable<int> ReviewSprint17ProductionSnakeViews(ExpandedSummoningCorrectionFixture fixture)
        {
            var native = fixture.Blueprints.OfType<BlueprintUnit>().Single(value =>
                value.AssetGuid == "bf2216f48b3f4d24c9c502007649340d");
            var control = Game.Instance.EntityCreator.SpawnUnit(native, fixture.Caster.Position,
                Quaternion.identity, fixture.Scene);
            if (control == null) throw new InvalidOperationException("Native Worm control failed.");
            fixture.Created.Add(control);
            SetExpandedSummoningBrainActive(control, false);
            var donor = control.View.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .Select(value => new { Renderer = value, Name = value.name, Mesh = value.sharedMesh }).ToArray();
            var nativeAnimationSet = control.View.AnimationManager.AnimationSet;
            Vector3 nativeScale = control.View.transform.localScale;
            float nativeCorpulence = control.Corpulence;
            var nativeSkills = control.Blueprint.Skills;
            var skillFields = typeof(BlueprintUnit.UnitSkills).GetFields(
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            int[] nativeSkillSeeds = skillFields.Select(value => (int)value.GetValue(nativeSkills)).ToArray();
            try
            {
                foreach (string key in new[] { "viper", "constrictor-snake" })
                foreach (bool fault in new[] { true, false })
                {
                    UnitEntityData unit = null;
                    int reached = 0;
                    UnityEngine.Object[] owned = new UnityEngine.Object[0];
                    string suffix = key + (fault ? "-rollback" : "-normal");
                    var row = new JObject { ["key"] = key, ["scope"] = "production hidden-snake body binding",
                        ["faultInjected"] = fault, ["gameplayQualified"] = false,
                        ["nativeDonorPerceptionSeed"] = nativeSkills.Perception };
                    _serpentineBodyRows.Add(row);
                    try
                    {
                        try
                        {
                            if (fault) SerpentineVisualAttachment.PostSwapFaultForTest = () => {
                                reached++;
                                // Exactly this newly created owned identity;
                                // no unrelated component is touched or swept.
                                var current = Resources.FindObjectsOfTypeAll<SerpentineVisualAttachment>().Single(value => {
                                    var candidateView = value.GetComponent<Kingmaker.View.UnitEntityView>();
                                    string found;
                                    return candidateView != null && candidateView.EntityData != null &&
                                        !fixture.UnitsBefore.Any(prior => ReferenceEquals(prior, candidateView.EntityData)) &&
                                        SerpentineVisualPolicy.TryProductionSnake(true, candidateView.EntityData.Blueprint.AssetGuid,
                                            candidateView.EntityData.Blueprint.name, candidateView.EntityData.Blueprint.Prefab.AssetId, out found) &&
                                        found == key;
                                });
                                owned = current.CaptureOwnedResources();
                                throw new InvalidOperationException("Scoped production-snake post-swap rollback drill");
                            };
                            // The generic mechanical helper removes appearance
                            // buffs. Use the raw private variant cast here so
                            // native fader/appearance settlement is genuine.
                            unit = CastExpandedSummoningVariant(fixture.Blueprints, fixture.Caster,
                                ExpandedSummoningOwnTierVariant(key, SummonMultiplicity.One),
                                null, fixture.Evidence).Single();
                            fixture.Created.Add(unit);
                        }
                        finally { SerpentineVisualAttachment.PostSwapFaultForTest = null; }
                        SetExpandedSummoningBrainActive(unit, false);
                        if (!Game.Instance.State.AwakeUnits.Contains(unit)) Game.Instance.State.AwakeUnits.Add(unit);
                        // One restored donor remained non-intact at its random
                        // native placement in 2dd. Narrow observation to the
                        // qualified art-point fixture, without enabling a skin,
                        // clearing appearance buffs or changing its fader.
                        row["nativeSpawnPosition"] = SurveyVector(unit.Position);
                        string floorEvidence;
                        Vector3 floor = FindExpandedSummoningArtPoint(fixture.Caster, out floorEvidence);
                        PlaceExpandedSummoningUnit(unit, floor);
                        row["floorSurvey"] = floorEvidence;
                        var visibility = new JArray();
                        row["visibilitySamples"] = visibility;
                        visibility.Add(Sprint17SnakeVisibilitySample(unit, key, 0));
                        int frames = 0;
                        while (++frames <= 600)
                        {
                            yield return 0;
                            if (frames == 30 || frames == 60 || frames == 600)
                                visibility.Add(Sprint17SnakeVisibilitySample(unit, key, frames));
                            if (frames >= 30 && Sprint17BodyIntact(unit.View, SerpentineVisualPolicy.BodyRenderer(key))) break;
                        }
                        visibility.Add(Sprint17SnakeVisibilitySample(unit, key, frames));
                        var view = unit.View;
                        var attachment = view.GetComponent<SerpentineVisualAttachment>();
                        string outcome = ExpandedSummoningSerpentineViewPatch.DescribeView(view);
                        row["blueprint"] = unit.Blueprint.AssetGuid;
                        row["outcome"] = outcome; row["frames"] = frames; row["faultReached"] = reached;
                        row["viewScale"] = SurveyVector(view.transform.localScale);
                        row["nativeScale"] = SurveyVector(nativeScale);
                        row["nativeControlCorpulence"] = nativeCorpulence;
                        row["corpulence"] = unit.Corpulence;
                        row["mechanicalSize"] = unit.Descriptor.State.Size.ToString();
                        bool intact = Sprint17BodyIntact(view, SerpentineVisualPolicy.BodyRenderer(key));
                        bool original = attachment != null && attachment.Body != null &&
                            attachment.Body.sharedMesh != null &&
                            attachment.Body.sharedMesh.name.StartsWith("KMG_" + key + "_Original_", StringComparison.Ordinal);
                        bool fallback = view.GetComponentsInChildren<SkinnedMeshRenderer>(true).All(skin =>
                            donor.Any(source => source.Name == skin.name && source.Mesh == skin.sharedMesh));
                        row["originalAttached"] = original; row["donorRestored"] = fallback; row["intact"] = intact;
                        bool bound = fault
                            ? reached == 1 && !original && fallback && owned.Length >= 5 &&
                                owned.All(value => value == null) && outcome.StartsWith("donor-visual:attach-failed:", StringComparison.Ordinal)
                            : reached == 0 && original && !fallback && outcome.StartsWith("visual:attached;", StringComparison.Ordinal);
                        _serpentineBodyAssertions.Add(Assertion("sprint17-production-binding-" + suffix,
                            "automatic exact-identity original binding or fully owned rollback after native appearance settlement",
                            row.ToString(Formatting.None), intact && bound,
                            "No fixture attachment or renderer/appearance-lock override."));
                        _serpentineBodyAssertions.Add(Assertion("sprint17-production-scale-" + suffix,
                            "one .2 body/base-footprint step, native .5m floor, Medium rules size, unchanged native animation set",
                            row.ToString(Formatting.None),
                            (view.transform.localScale - nativeScale * SerpentineVisualPolicy.SnakeViewMultiplier).sqrMagnitude < .00000001f &&
                                Math.Abs(unit.Corpulence - Math.Max(.5f,
                                    SerpentineVisualPolicy.ScaleSnakeBaseCorpulence(nativeCorpulence))) < .00001f &&
                                unit.Descriptor.State.Size == Kingmaker.Enums.Size.Medium &&
                                ReferenceEquals(view.AnimationManager.AnimationSet, nativeAnimationSet),
                            "Exact snake instance only; weapon reach, native size multiplier and clips unchanged."));
                        if (attachment != null) owned = attachment.CaptureOwnedResources();
                        var meshes = view.GetComponentsInChildren<SkinnedMeshRenderer>(true).Select(value => value.sharedMesh).ToArray();
                        Vector3 scale = view.transform.localScale;
                        float corpulence = unit.Corpulence;
                        // Re-enter only our callback, not native OnDataAttached:
                        // the original native initialization must not replay.
                        ExpandedSummoningSerpentineViewPatch.Postfix(view);
                        _serpentineBodyAssertions.Add(Assertion("sprint17-production-once-" + suffix,
                            "repeat callback preserves exact body/footprint scale, component, meshes and prior outcome",
                            ExpandedSummoningSerpentineViewPatch.DescribeView(view),
                            view.transform.localScale == scale && unit.Corpulence == corpulence &&
                                ReferenceEquals(view.GetComponent<SerpentineVisualAttachment>(), attachment) &&
                                meshes.SequenceEqual(view.GetComponentsInChildren<SkinnedMeshRenderer>(true).Select(value => value.sharedMesh)) &&
                                ExpandedSummoningSerpentineViewPatch.DescribeView(view) == outcome,
                            "Failed attachments do not loop; a later fresh unit may recover."));
                    }
                    finally
                    {
                        SerpentineVisualAttachment.PostSwapFaultForTest = null;
                        if (unit != null && !unit.Destroyed)
                        {
                            InterruptExpandedSummoningFixtureCommands(unit);
                            unit.Destroy();
                            Game.Instance.EntityDestroyer.Tick();
                        }
                    }
                    yield return 0; yield return 0;
                    _serpentineBodyAssertions.Add(Assertion("sprint17-production-destroy-" + suffix,
                        "native unit teardown destroys exact owned meshes, materials, controller clones and texture",
                        "captured=" + owned.Length + ";remaining=" + owned.Count(value => value != null),
                        owned.Length >= 5 && owned.All(value => value == null),
                        "No fixture Release call, premature disposal, name-based deletion or global sweep."));
                }
                _serpentineBodyAssertions.Add(Assertion("sprint17-production-native-worm-negative-control",
                    "native Worm receives no hook, scale or mesh mutation",
                    ExpandedSummoningSerpentineViewPatch.DescribeView(control.View),
                    ExpandedSummoningSerpentineViewPatch.DescribeView(control.View) == "not-attempted" &&
                        control.View.GetComponent<SerpentineVisualAttachment>() == null &&
                        control.View.transform.localScale == nativeScale &&
                        control.Corpulence == nativeCorpulence &&
                        donor.All(value => value.Mesh != null && value.Renderer.sharedMesh == value.Mesh) &&
                        ReferenceEquals(control.Blueprint.Skills, nativeSkills) &&
                        nativeSkillSeeds.SequenceEqual(skillFields.Select(value => (int)value.GetValue(nativeSkills))) &&
                        ReferenceEquals(control.View.AnimationManager.AnimationSet, nativeAnimationSet),
                    "Exact native blueprint, not a KMG proxy; borrowed resources remain live."));
            }
            finally
            {
                SerpentineVisualAttachment.PostSwapFaultForTest = null;
                if (!control.Destroyed) { control.Destroy(); Game.Instance.EntityDestroyer.Tick(); }
            }
        }

        private static JObject Sprint17SnakeVisibilitySample(UnitEntityData unit, string key, int frame)
        {
            var body = unit.View.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .SingleOrDefault(value => value.name == SerpentineVisualPolicy.BodyRenderer(key));
            return new JObject { ["frame"] = frame, ["position"] = SurveyVector(unit.Position),
                ["control"] = Sprint16ControlObservation(unit), ["bodyFound"] = body != null,
                ["enabled"] = body != null && body.enabled,
                ["active"] = body != null && body.gameObject.activeInHierarchy,
                ["mesh"] = body == null || body.sharedMesh == null ? null : body.sharedMesh.name,
                ["materials"] = body == null ? new JArray() : new JArray(body.sharedMaterials.Select(material =>
                    material == null ? new JObject { ["missing"] = true } : new JObject {
                        ["name"] = material.name,
                        ["dissolve"] = material.HasProperty("_Dissolve") ? (JToken)material.GetFloat("_Dissolve") : JValue.CreateNull() })) };
        }
    }
}
