using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.View;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// In-game images of any summon for internal review (Phase 1, Sprint 3
    /// onward). The request names creature keys; each is cast, one at a time,
    /// through its real parent chain into the loaded working save, held across
    /// frames while the Phase 0 party-camera review renders it idle, moving
    /// and attacking, then dismissed through the same cleanup the synchronous
    /// fixtures use, before the next creature is cast. Nothing is saved: the
    /// exact-working-load sentinel and the request-local cleanup assertion
    /// prove the save file was only ever read.
    ///
    /// The review images are the deliverable; the assertions only say whether
    /// each image is worth looking at (creature in frame, screen lit,
    /// renderer enabled, dissolve finished). A creature that renders wrongly
    /// but in frame still passes here, and is caught by the person or the
    /// agent who looks at the file.
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        private const int CreatureReviewSpawnSettleUpdates = 2;
        private const int CreatureReviewCleanupSettleUpdates = 5;

        private List<SummonVariantSpec> _creatureReviewQueue;
        private int _creatureReviewIndex;
        private int _creatureReviewPhase;
        private int _creatureReviewSettle;
        private UnitEntityData[] _creatureReviewUnits = Array.Empty<UnitEntityData>();
        private UnitEntityData _creatureReviewCaster;
        private UnitEntityData[] _creatureReviewParty;
        private object _creatureReviewGameState;
        private BlueprintScriptableObject[] _creatureReviewBlueprints;
        private readonly List<RuntimeTestAssertion> _creatureReviewAssertions =
            new List<RuntimeTestAssertion>();

        /// <summary>
        /// The one variant reviewed per creature key: its own-tier single
        /// under Summon Nature's Ally when the creature has an ally placement,
        /// otherwise under Summon Monster. Unknown keys fail the request.
        /// </summary>
        internal static SummonVariantSpec[] ResolveCreatureReviewVariants(
            string creatures)
        {
            if (string.IsNullOrWhiteSpace(creatures))
                throw new InvalidOperationException(
                    "The creature review needs at least one creature key.");
            string[] keys = creatures.Split(new[] { ',' },
                    StringSplitOptions.RemoveEmptyEntries)
                .Select(value => value.Trim()).Where(value => value.Length > 0)
                .Distinct(StringComparer.Ordinal).ToArray();
            var result = new List<SummonVariantSpec>();
            foreach (string key in keys)
            {
                SummonCreatureSpec creature = ExpandedSummoningCatalog.All
                    .SingleOrDefault(value => value.Key == key);
                if (creature == null) throw new InvalidOperationException(
                    "Unknown creature key for review: " + key + ".");
                SummonFamily family = creature.NaturesAllyTier.HasValue ?
                    SummonFamily.NaturesAlly : SummonFamily.Monster;
                int tier = family == SummonFamily.NaturesAlly ?
                    creature.NaturesAllyTier.Value : creature.MonsterTier.Value;
                SummonVariantSpec variant = ExpandedSummoningCatalog
                    .GenerateVariants(family).Single(value =>
                        value.Creature.Key == key && value.ParentTier == tier &&
                        value.Multiplicity == SummonMultiplicity.One);
                if (!SummonVisibilityCatalog.IsPublished(variant))
                    throw new InvalidOperationException(
                        "A suppressed creature cannot be reviewed through a parent: " +
                        key + ".");
                result.Add(variant);
            }
            return result.ToArray();
        }

        private void StepExpandedSummoningCreatureReview()
        {
            if (_creatureReviewQueue == null)
            {
                if (!_context.FeatureModules.Active.ExpandedSummoning)
                    throw new InvalidOperationException(
                        "The creature review requires the Expanded Summoning module to be active.");
                UnitEntityData[] party = Game.Instance.Player.Party.Where(value =>
                    value != null && value.Descriptor != null).ToArray();
                if (party.Length != WorkingSaveSmokeScenario.ExpectedPartyCount)
                    throw new InvalidOperationException(
                        "The loaded working-save party fingerprint changed before the creature review.");
                UnitEntityData caster = party.FirstOrDefault(value =>
                    value.HoldingState != null);
                if (caster == null) throw new InvalidOperationException(
                    "The loaded working save has no party member in an active area state.");
                object gameState = ReadExactMember(Game.Instance, "State");
                int staleLeft = RemoveExpandedSummoningStaleSummons(gameState, party);
                if (staleLeft != 0)
                    throw new InvalidOperationException(
                        "Stale KMG summons could not be removed before the creature review: " +
                        staleLeft + " remain.");
                _creatureReviewParty = party;
                _creatureReviewCaster = caster;
                _creatureReviewGameState = gameState;
                _creatureReviewBlueprints = BlueprintBootstrap.Library
                    .GetAllBlueprints().Where(value => value != null).ToArray();
                _creatureReviewQueue = ResolveCreatureReviewVariants(
                    (string)_request.Parameters["creatures"]).ToList();
                _creatureReviewIndex = 0;
                _creatureReviewPhase = 0;
                WriteLifecycleStage("creature-review-start");
                // Fall through: the first cast happens in the same guarded
                // update as the setup, exactly as the persistence prepare
                // casts its fixture.
            }
            if (_creatureReviewIndex >= _creatureReviewQueue.Count)
            {
                CompleteExpandedSummoningCreatureReview();
                return;
            }
            SummonVariantSpec variant = _creatureReviewQueue[_creatureReviewIndex];
            string key = variant.Creature.Key;
            switch (_creatureReviewPhase)
            {
                case 0:
                    _creatureReviewUnits = SpawnExpandedSummoningVariants(
                        _creatureReviewBlueprints, _creatureReviewCaster,
                        new[] { variant }, "Creature review of " + key);
                    ResetExpandedSummoningMotionReview(
                        ExpandedSummoningIdentityCatalog.UnitSymbol(variant.Creature)
                            .Replace('.', '_').Replace('-', '_'), key + "-review");
                    _creatureReviewSettle = 0;
                    _creatureReviewPhase = 1;
                    WriteLifecycleStage("creature-review-" + key + "-summoned");
                    return;
                case 1:
                    if (_creatureReviewSettle++ < CreatureReviewSpawnSettleUpdates) return;
                    _creatureReviewPhase = 2;
                    return;
                case 2:
                    if (!StepExpandedSummoningMotionReview(_creatureReviewUnits,
                            "summoned")) return;
                    // Sprint 5: a creature with a registered visual variant
                    // must show it applied on the reviewed view.
                    bool variantRegistered = _creatureReviewUnits.Any(unit =>
                        ExpandedSummoningVisualVariantPatch.RegisteredBlueprintNames
                            .Contains(unit.Blueprint.name));
                    string variantOutcome = string.Join("|", _creatureReviewUnits.Select(
                        unit => ExpandedSummoningVisualVariantPatch.DescribeView(unit.View))
                        .ToArray());
                    bool variantValid = !variantRegistered || _creatureReviewUnits.All(
                        unit => ExpandedSummoningVisualVariantPatch.DescribeView(unit.View)
                            .StartsWith("variant:applied", StringComparison.Ordinal));
                    // Round 8: the attach-time outcome alone proved nothing
                    // about the render (the controller had replaced the
                    // clones); the materials on the view at capture time are
                    // recorded and, for a registered variant, must still be
                    // the project clones.
                    bool retainedAll = true;
                    var materialsNow = new List<string>();
                    foreach (UnitEntityData unit in _creatureReviewUnits)
                    {
                        bool retained;
                        materialsNow.Add(DescribeExpandedSummoningViewMaterials(unit,
                            out retained));
                        retainedAll = retainedAll && retained;
                    }
                    bool retainedValid = !variantRegistered || retainedAll;
                    _creatureReviewAssertions.Add(Assertion(
                        "expanded-summoning-creature-review-" + key,
                        "idle, moving-a, moving-b and attack captures in frame, lit, renderer enabled, intact" +
                            (variantRegistered ? "; registered visual variant applied at attach and retained on the view at capture" : ""),
                        MotionReviewSummary + ";visualVariant=" + variantOutcome +
                            ";materialsAtCapture=" + string.Join("|", materialsNow.ToArray()),
                        MotionReviewValid && variantValid && retainedValid,
                        (variant.Family == SummonFamily.Monster ? "Summon Monster " :
                            "Summon Nature's Ally ") + variant.ParentTier +
                        " single cast through the real parent chain; party-camera renders"));
                    foreach (UnitEntityData unit in _creatureReviewUnits)
                        CleanupExpandedSummoningUnit(unit);
                    Game.Instance.EntityDestroyer.Tick();
                    _creatureReviewSettle = 0;
                    _creatureReviewPhase = 3;
                    return;
                default:
                    if (_creatureReviewSettle++ < CreatureReviewCleanupSettleUpdates) return;
                    int live = _creatureReviewUnits.Count(value => !value.Destroyed ||
                        value.View != null || value.HoldingState != null);
                    _creatureReviewAssertions.Add(Assertion(
                        "expanded-summoning-creature-review-cleanup-" + key, "0",
                        live.ToString(), live == 0,
                        "reviewed summon dismissed and destroyed before the next cast"));
                    WriteLifecycleStage("creature-review-" + key + "-complete");
                    _creatureReviewUnits = Array.Empty<UnitEntityData>();
                    _creatureReviewIndex++;
                    _creatureReviewPhase = 0;
                    return;
            }
        }

        /// <summary>
        /// What the reviewed view's renderers carry at capture time: per
        /// material its name, shader, the declared colour, tint, emission,
        /// texture and dissolve slots, the current tint, the dissolve amount
        /// and whether the view's material controller drives it. The variant
        /// is retained when at least one renderer material is the project
        /// clone (or the controller's instance of it).
        /// </summary>
        private static string DescribeExpandedSummoningViewMaterials(UnitEntityData unit,
            out bool variantRetained)
        {
            variantRetained = false;
            if (unit == null || unit.View == null) return "<no-view>";
            Renderer[] renderers = unit.View.GetComponentsInChildren<Renderer>(true)
                .Where(value => value != null && value.sharedMaterials != null &&
                    value.sharedMaterials.Length != 0).ToArray();
            var controller = unit.View.GetComponentInChildren<
                Kingmaker.Visual.MaterialEffects.StandardMaterialController>(true);
            IList<Material> driven = ExpandedSummoningPteranodonViewPatch
                .ControllerMaterials(controller);
            var parts = new List<string>();
            int materials = 0, variantMaterials = 0;
            foreach (Renderer renderer in renderers)
                foreach (Material material in renderer.sharedMaterials)
                {
                    if (material == null) { parts.Add("<null>"); continue; }
                    materials++;
                    if (material.name.StartsWith(ExpandedSummoningVisualVariantPatch
                            .VariantMaterialName, StringComparison.Ordinal))
                        variantMaterials++;
                    var slots = new List<string>();
                    foreach (string slot in new[] { "_Color", "_TintColor", "_BaseColor",
                        "_MainColor", "_EmissionColor", "_MainTex", "_Dissolve" })
                        if (material.HasProperty(slot)) slots.Add(slot);
                    string tint = material.HasProperty("_TintColor")
                        ? DescribeReviewColour(material.GetColor("_TintColor"))
                        : material.HasProperty("_Color")
                            ? DescribeReviewColour(material.GetColor("_Color")) : "<none>";
                    string dissolve = material.HasProperty("_Dissolve")
                        ? material.GetFloat("_Dissolve").ToString("0.###",
                            CultureInfo.InvariantCulture) : "<none>";
                    parts.Add(material.name.Replace(';', ',').Replace('|', '/') + "{" +
                        (material.shader == null ? "<no-shader>" : material.shader.name
                            .Replace(';', ',').Replace('|', '/')) + ":" +
                        string.Join(",", slots.ToArray()) + ":tint=" + tint +
                        ":dissolve=" + dissolve + ":driven=" +
                        (driven != null && driven.Contains(material)) +
                        ":" + DescribeReviewMaterialProbe(material) + "}");
                }
            variantRetained = variantMaterials != 0;
            var rimAnimations = new List<string>();
            var rimController = ExpandedSummoningRimAnimationPatch.RimControllerOf(controller);
            if (rimController != null && rimController.Animations != null)
                foreach (var animation in rimController.Animations)
                    rimAnimations.Add(animation == null ? "<null>" : "loop=" + animation.LoopAnimation +
                        ",lifetime=" + animation.Lifetime.ToString("0.##", CultureInfo.InvariantCulture) +
                        ",scale=" + animation.IntensityScale.ToString("0.##", CultureInfo.InvariantCulture) +
                        ",colour=" + DescribeReviewColour(animation.CurrentColor) +
                        ",intensity=" + animation.CurrentIntensity.ToString("0.##", CultureInfo.InvariantCulture) +
                        ",finished=" + animation.IsFinished);
            return "renderers=" + renderers.Length + ";materials=" + materials +
                ";variantMaterials=" + variantMaterials + ";controllerMaterials=" +
                (driven == null ? -1 : driven.Count) + ";" +
                string.Join(",", parts.ToArray()) + ";character=" +
                DescribeReviewCharacter(unit.View) + ";rimAnimations[" +
                string.Join("|", rimAnimations.ToArray()) + "];rimPatch=" +
                ExpandedSummoningRimAnimationPatch.Describe(unit.View);
        }

        /// <summary>
        /// Round 11: the view's character-system state. A character built at
        /// runtime rebuilds its texture atlases after the view attaches and
        /// assigns them to the renderer's materials, which is where a coat
        /// put on the clone at attach would be overwritten.
        /// </summary>
        private static string DescribeReviewCharacter(UnitEntityView view)
        {
            var character = view.GetComponentInChildren<
                Kingmaker.Visual.CharacterSystem.Character>(true);
            if (character == null) return "<none>";
            Type type = character.GetType();
            System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
            var atlases = new List<string>();
            var atlasList = type.GetField("m_Atlases", flags) == null ? null :
                type.GetField("m_Atlases", flags).GetValue(character) as System.Collections.IEnumerable;
            if (atlasList != null)
                foreach (object atlas in atlasList)
                {
                    if (atlas == null) { atlases.Add("<null>"); continue; }
                    Type atlasType = atlas.GetType();
                    object channel = atlasType.GetProperty("Channel", flags) == null ? null :
                        atlasType.GetProperty("Channel", flags).GetValue(atlas, null);
                    Texture texture = atlasType.GetProperty("AtlasTexture", flags) == null ? null :
                        atlasType.GetProperty("AtlasTexture", flags).GetValue(atlas, null) as Texture;
                    Material material = atlasType.GetProperty("Material", flags) == null ? null :
                        atlasType.GetProperty("Material", flags).GetValue(atlas, null) as Material;
                    atlases.Add((channel == null ? "?" : channel.ToString()) + "=" +
                        (texture == null ? "<null>" : texture.name.Replace(';', ',')
                            .Replace('|', '/').Replace(':', '.') + "@" + texture.width + "x" +
                            texture.height) + "/" + (material == null ? "<null>" :
                            material.name.Replace(';', ',').Replace('|', '/').Replace(':', '.')));
                }
            var entities = new List<string>();
            var entityList = type.GetField("m_EquipmentEntities", flags) == null ? null :
                type.GetField("m_EquipmentEntities", flags).GetValue(character) as System.Collections.IEnumerable;
            if (entityList != null)
                foreach (object entity in entityList)
                {
                    var ee = entity as Kingmaker.Visual.CharacterSystem.EquipmentEntity;
                    if (ee == null) { entities.Add("<null>"); continue; }
                    entities.Add(ee.name.Replace(';', ',').Replace('|', '/').Replace(':', '.') +
                        "(primaryRamps=" + (ee.PrimaryRamps == null ? -1 : ee.PrimaryRamps.Count) +
                        ",secondaryRamps=" + (ee.SecondaryRamps == null ? -1 : ee.SecondaryRamps.Count) +
                        ",profile=" + (ee.ColorsProfile == null ? "<null>" : ee.ColorsProfile.name
                            .Replace(';', ',').Replace('|', '/').Replace(':', '.')) +
                        ",bodyParts=" + (ee.BodyParts == null ? -1 : ee.BodyParts.Count) + ")");
                }
            var ramps = new List<string>();
            var rampList = type.GetField("m_RampIndices", flags) == null ? null :
                type.GetField("m_RampIndices", flags).GetValue(character) as System.Collections.IEnumerable;
            if (rampList != null)
                foreach (object ramp in rampList) ramps.Add(ramp == null ? "<null>" : ramp.ToString());
            var shared = type.GetField("m_SharedMaterials", flags) == null ? null :
                type.GetField("m_SharedMaterials", flags).GetValue(character) as System.Collections.IEnumerable;
            var sharedNames = new List<string>();
            if (shared != null)
                foreach (object material in shared)
                    sharedNames.Add(material == null ? "<null>" : ((Material)material).name
                        .Replace(';', ',').Replace('|', '/').Replace(':', '.'));
            return "present:baked=" + (character.BakedCharacter != null) + ":dirty=" +
                character.IsDirty + ":atlasesDirty=" + character.IsAtlasesDirty +
                ":atlases[" + string.Join(",", atlases.ToArray()) + "]:entities[" +
                string.Join(",", entities.ToArray()) + "]:rampIndices[" +
                string.Join(",", ramps.ToArray()) + "]:sharedMaterials[" +
                string.Join(",", sharedNames.ToArray()) + "]";
        }

        /// <summary>
        /// Round 11: every texture property the shader declares with the
        /// assigned texture's name and size, the shader keywords, the render
        /// queue, and the float and colour slots the game's own code
        /// references - so a variant can target the slot that paints the rig.
        /// </summary>
        private static string DescribeReviewMaterialProbe(Material material)
        {
            var textures = new List<string>();
            string[] names;
            try { names = material.GetTexturePropertyNames(); }
            catch (Exception) { names = new string[0]; }
            foreach (string name in names)
            {
                Texture texture = material.GetTexture(name);
                textures.Add(name + "=" + (texture == null ? "<null>" :
                    texture.name.Replace(';', ',').Replace('|', '/').Replace(':', '.') +
                    "@" + texture.width + "x" + texture.height));
            }
            var floats = new List<string>();
            foreach (string name in new[] { "_Emission", "_RimPower", "_RimLighting",
                "_Metallic", "_Cutout", "_Alpha", "_AlphaScale", "_DissolveEmission",
                "_Glossiness", "_Smoothness", "_BumpScale", "_OcclusionStrength" })
                if (material.HasProperty(name))
                    floats.Add(name + "=" + material.GetFloat(name).ToString("0.###",
                        CultureInfo.InvariantCulture));
            var colours = new List<string>();
            foreach (string name in new[] { "_RimColor", "_ColorMask", "_ChannelMask",
                "_DissolveColor", "_EmissionColor", "_SpecColor", "_Color", "_TintColor" })
                if (material.HasProperty(name))
                    colours.Add(name + "=" + DescribeReviewColour(material.GetColor(name)));
            return "textures[" + string.Join(",", textures.ToArray()) + "]:floats[" +
                string.Join(",", floats.ToArray()) + "]:colours[" +
                string.Join(",", colours.ToArray()) + "]:keywords[" +
                string.Join(",", material.shaderKeywords ?? new string[0]) + "]:queue=" +
                material.renderQueue;
        }

        private static string DescribeReviewColour(Color value)
        {
            return value.r.ToString("0.##", CultureInfo.InvariantCulture) + "/" +
                value.g.ToString("0.##", CultureInfo.InvariantCulture) + "/" +
                value.b.ToString("0.##", CultureInfo.InvariantCulture) + "/" +
                value.a.ToString("0.##", CultureInfo.InvariantCulture);
        }

        private void CompleteExpandedSummoningCreatureReview()
        {
            WorkingSaveSmokeEvidence evidence = _workingSaveSmoke.Stop();
            int remaining = ExpandedSummoningPersistentUnits(
                _creatureReviewGameState, _creatureReviewParty).Length;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("exact-working-load",
                    "one correlated working descriptor; baseline distinct",
                    "working=" + evidence.WorkingMatchCount + ";baseline=" +
                        evidence.BaselineMatchCount + ";correlated=" +
                        evidence.DescriptorReferenceCorrelated,
                    evidence.WorkingMatchCount == 1 &&
                        evidence.BaselineMatchCount == 1 &&
                        evidence.DescriptorReferenceCorrelated,
                    "object-reference-correlated guarded load path")
            };
            assertions.AddRange(_creatureReviewAssertions);
            assertions.Add(Assertion("expanded-summoning-creature-review-count",
                _creatureReviewQueue.Count.ToString(),
                _creatureReviewAssertions.Count(value =>
                    value.Name.StartsWith("expanded-summoning-creature-review-",
                        StringComparison.Ordinal) && !value.Name.StartsWith(
                        "expanded-summoning-creature-review-cleanup-",
                        StringComparison.Ordinal)).ToString(),
                _creatureReviewAssertions.Count(value =>
                    value.Name.StartsWith("expanded-summoning-creature-review-",
                        StringComparison.Ordinal) && !value.Name.StartsWith(
                        "expanded-summoning-creature-review-cleanup-",
                        StringComparison.Ordinal)) == _creatureReviewQueue.Count,
                "every requested creature was reviewed"));
            assertions.Add(Assertion("request-local-cleanup",
                "no KMG summons remain; no save written",
                "remaining=" + remaining + ";saveRoutines=" +
                    evidence.ExpectedWorkingSaveRoutineCount + ";unexpected=" +
                    evidence.SaveWritingApiObserved,
                remaining == 0 && evidence.ExpectedWorkingSaveRoutineCount == 0 &&
                    !evidence.SaveWritingApiObserved,
                "the working save is read, never written, by the review"));
            assertions.Add(Assertion("loaded-mod-version", _request.ExpectedModVersion,
                _context.ModEntry.Info.Version,
                _context.ModEntry.Info.Version == _request.ExpectedModVersion,
                "Unity Mod Manager ModEntry.Info.Version"));
            RuntimeTestResult result = CreateResult(assertions.All(value =>
                value.Status == RuntimeTestStatuses.Pass) ? RuntimeTestStatuses.Pass :
                RuntimeTestStatuses.Fail, assertions, null);
            result.WorkingSaveSmoke = evidence;
            Complete(result);
        }
    }
}
