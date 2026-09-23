using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.View;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Observes the Pteranodon view on a really summoned unit.
    ///
    /// The detached-prefab scenario can prove the mesh, the rig and the Avatar,
    /// but not the animation controller: Kingmaker assigns the
    /// runtimeAnimatorController when the view attaches to a unit, so a prefab
    /// instantiated outside that path legitimately reports none. Asserting a
    /// controller there would be an attach-time fact claimed from a detached
    /// object - the same error twice. This scenario summons the creature through
    /// the real rule path and reads the controller, clips, attack and impact
    /// events, and effect anchors that Sprint 2 must preserve.
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        private RuntimeTestResult RunDisposablePteranodonAttachedView()
        {
            BlueprintScriptableObject[] blueprints = BlueprintBootstrap.Library
                .GetAllBlueprints().Where(value => value != null).ToArray();

            // Exactly one Pteranodon from its own tier, so the observation is
            // about one unmistakable unit rather than a crowd.
            SummonVariantSpec variant = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster)
                .Single(value => value.Creature.Key == "pteranodon" &&
                    value.ParentTier == 4 &&
                    value.Multiplicity == SummonMultiplicity.One);

            object state = ReadExactMember(Game.Instance, "State");
            object allUnits = ReadExactMember(state, "AllUnits");
            object player = ReadExactMember(Game.Instance, "Player");
            object party = ReadExactMember(player, "Party");
            object[] unitsBefore = SnapshotReferences(allUnits);
            object[] partyBefore = SnapshotReferences(party);

            UnitEntityData caster = null;
            BlueprintUnit casterBlueprint = null;
            Kingmaker.EntitySystem.SceneEntitiesState scene = null;
            object sceneEntities = null;
            object[] entitiesBefore = null;
            UnitEntityData summoned = null;
            MethodInfo summonRuleMethod = null;
            string attachedReport = "<unobserved>";
            string anchorReport = "<unobserved>";
            bool controllerIdentified = false;
            bool clipsIdentified = false;
            bool eventsIdentified = false;
            bool viewIsPteranodon = false;
            bool cleaned = false;
            int clipCount = 0;
            int eventCount = 0;

            try
            {
                summonRuleMethod = typeof(RuleSummonUnit).GetMethod("OnTrigger",
                    BindingFlags.Public | BindingFlags.Instance);
                if (summonRuleMethod == null) throw new MissingMethodException(
                    typeof(RuleSummonUnit).FullName, "OnTrigger");
                MethodInfo capturePostfix = typeof(RuntimeTestRunner).GetMethod(
                    "ExpandedSummoningRuleCapturePostfix",
                    BindingFlags.NonPublic | BindingFlags.Static);

                UnityEngine.SceneManagement.Scene activeScene =
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                if (!activeScene.IsValid() || !activeScene.isLoaded)
                    throw new InvalidOperationException(
                        "No exact loaded Unity scene was available for the disposable caster.");

                // The caster must be spawned INTO the loaded area's persistent
                // state. Passing null spawns a unit with no AreaPersistentState,
                // which native destruction then dereferences: the first version
                // of this scenario died in EntityDestructionController with a
                // NullReferenceException during its own cleanup. Native spawn
                // placement also needs the loaded area's pathfinding graph.
                UnitEntityData areaAnchor = partyBefore.OfType<UnitEntityData>()
                    .FirstOrDefault(value => value.HoldingState != null);
                if (areaAnchor == null)
                    throw new InvalidOperationException(
                        "The guarded working save provided no party unit with an area state.");
                scene = areaAnchor.HoldingState;

                casterBlueprint = UnityEngine.Object.Instantiate(
                    BlueprintRoot.Instance.DefaultPlayerCharacter);
                casterBlueprint.name = "KMG_Runtime_PteranodonAttachedView_Caster";
                casterBlueprint.IsCheater = true;
                caster = Game.Instance.EntityCreator.SpawnUnit(
                    casterBlueprint, Vector3.zero, Quaternion.identity, scene);
                Game.Instance.EntityCreator.Tick();
                if (caster == null || caster.View == null)
                    throw new InvalidOperationException(
                        "Native entity creation did not produce a live caster view.");
                if (!caster.IsInState)
                    throw new InvalidOperationException(
                        "The disposable caster did not enter the exact loaded-area state.");
                sceneEntities = scene.AllEntityData;
                entitiesBefore = SnapshotReferences(sceneEntities);

                _context.Harmony.Patch(summonRuleMethod, null,
                    new HarmonyMethod(capturePostfix), null);
                _expandedSummoningRuleCaptureActive = true;
                ExpandedSummoningRuleCapture.Clear();

                BlueprintAbility ability = ResolveExpandedSummoningExecution(
                    blueprints, variant);
                caster.Descriptor.AddFact(ability);
                try
                {
                    ExecuteExpandedSummoningRuntimeAbility(caster, ability,
                        variant.ParentTier);
                }
                finally
                {
                    if (caster.Descriptor.HasFact(ability))
                        caster.Descriptor.RemoveFact(ability);
                }

                Game.Instance.EntityCreator.Tick();
                if (ExpandedSummoningRuleCapture.Count != 1)
                    throw new InvalidOperationException(
                        "The Pteranodon cast did not create exactly one native summon.");
                summoned = ExpandedSummoningRuleCapture.Single();

                UnitEntityView view = summoned.View;
                if (view == null)
                    throw new InvalidOperationException(
                        "The summoned Pteranodon has no live view.");

                viewIsPteranodon = summoned.Blueprint != null &&
                    summoned.Blueprint.name.IndexOf("Pteranodon",
                        StringComparison.OrdinalIgnoreCase) >= 0;

                AttachedObservation observed = DescribeAttachedView(view);
                attachedReport = observed.Text;
                anchorReport = observed.Anchors;
                clipCount = observed.ClipCount;
                eventCount = observed.EventCount;
                controllerIdentified = !string.IsNullOrEmpty(observed.ControllerName);
                clipsIdentified = observed.ClipCount > 0;
                eventsIdentified = observed.EventCount > 0;
            }
            finally
            {
                _expandedSummoningRuleCaptureActive = false;
                ExpandedSummoningRuleCapture.Clear();
                if (summonRuleMethod != null)
                    _context.Harmony.Unpatch(summonRuleMethod,
                        HarmonyPatchType.All, _context.ModId);
                IEnumerable<UnitEntityData> localUnits = sceneEntities == null
                    ? Enumerable.Empty<UnitEntityData>()
                    : SnapshotReferences(sceneEntities).OfType<UnitEntityData>();
                foreach (UnitEntityData unit in localUnits.Concat(
                    SnapshotReferences(allUnits).OfType<UnitEntityData>())
                    .Where(value => !unitsBefore.Any(prior =>
                        ReferenceEquals(prior, value))).Distinct().ToArray())
                {
                    if (unit.IsInState) unit.Destroy();
                    else unit.Dispose();
                }

                // Destroying a unit removes its facts first, and native aura
                // components can enqueue dependent entities only while that
                // first item is processed. A single Tick leaves those behind,
                // so drain across several passes exactly as the shipped
                // expanded-summoning scenario does.
                if (sceneEntities != null)
                    DrainExpandedSummoningDestroyQueue(sceneEntities, entitiesBefore);
                else
                    Game.Instance.EntityDestroyer.Tick();
                if (casterBlueprint != null)
                    UnityEngine.Object.Destroy(casterBlueprint);
                cleaned = SameReferences(unitsBefore, SnapshotReferences(allUnits)) &&
                    SameReferences(partyBefore, SnapshotReferences(party)) &&
                    (caster == null || !ContainsReference(allUnits, caster));
            }

            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("pteranodon-attached-unit-summoned",
                    "exactly one Pteranodon summoned through the real rule path",
                    "viewIsPteranodon=" + viewIsPteranodon, viewIsPteranodon,
                    "RuleSummonUnit capture on the disposable caster"),
                Assertion("pteranodon-attached-controller-identified",
                    "the attached view carries a named runtimeAnimatorController",
                    attachedReport, controllerIdentified,
                    "UnitEntityView.Animator on a live summoned unit"),
                Assertion("pteranodon-attached-clips-identified",
                    "the controller exposes the animation clips the creature uses",
                    "clips=" + clipCount, clipsIdentified,
                    "RuntimeAnimatorController.animationClips on the attached view"),
                Assertion("pteranodon-attached-events-identified",
                    "attack and impact timing is observable so a replacement bite lands on the native frames",
                    "events=" + eventCount, eventsIdentified,
                    "AnimationClip.events across the attached controller"),
                Assertion("pteranodon-attached-effect-anchors-recorded",
                    "particle and hit-effect anchors are recorded so the replacement preserves them",
                    anchorReport, anchorReport != "<unobserved>",
                    "ParticlesSnapMap and UnitHitFxManager on the attached view"),
                Assertion("pteranodon-attached-view-cleanup",
                    "the disposable caster and summon are removed and the unit tables are restored",
                    "cleaned=" + cleaned, cleaned,
                    "AllUnits and Party reference snapshots before and after"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };

            return CreateResult(assertions.TrueForAll(value =>
                value.Status == "PASS") ? RuntimeTestStatuses.Pass :
                RuntimeTestStatuses.Fail, assertions, null);
        }

        private sealed class AttachedObservation
        {
            internal string Text;
            internal string Anchors;
            internal string ControllerName;
            internal int ClipCount;
            internal int EventCount;
        }

        private static AttachedObservation DescribeAttachedView(UnitEntityView view)
        {
            var result = new AttachedObservation();
            var text = new StringBuilder();
            text.Append("view=").Append(view.name);

            Animator animator = view.Animator ??
                view.GetComponentsInChildren<Animator>(true)
                    .FirstOrDefault(value => value != null);
            text.Append(";animator=")
                .Append(animator == null ? "<null>" : animator.name);

            if (animator != null)
            {
                RuntimeAnimatorController controller = animator.runtimeAnimatorController;
                result.ControllerName = controller == null ? null : controller.name;
                text.Append(";controller=")
                    .Append(controller == null ? "<null>" : controller.name);

                AnimationClip[] clips = controller == null ||
                        controller.animationClips == null
                    ? new AnimationClip[0]
                    : controller.animationClips.Where(value => value != null).ToArray();
                result.ClipCount = clips.Length;
                text.Append(";clips=").Append(clips.Length.ToString(
                    CultureInfo.InvariantCulture));

                string[] names = clips.Select(value => value.name)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray();
                text.Append(";clipNames=").Append(string.Join(",", names.Take(60).ToArray()));

                // The frames a replacement bite has to hit.
                var events = new List<string>();
                foreach (AnimationClip clip in clips)
                {
                    AnimationEvent[] clipEvents = clip.events;
                    if (clipEvents == null) continue;
                    foreach (AnimationEvent clipEvent in clipEvents)
                    {
                        events.Add(clip.name + "@" + clipEvent.time.ToString(
                            "0.###", CultureInfo.InvariantCulture) + ":" +
                            clipEvent.functionName);
                    }
                }

                result.EventCount = events.Count;
                text.Append(";eventCount=").Append(events.Count.ToString(
                    CultureInfo.InvariantCulture));
                text.Append(";events=").Append(string.Join(",",
                    events.OrderBy(value => value, StringComparer.Ordinal)
                        .Take(60).ToArray()));
            }

            result.Text = text.ToString();
            result.Anchors = DescribeAnchors(view);
            return result;
        }

        /// <summary>
        /// Effect anchors the replacement must keep: particle snap points and
        /// the hit-effect manager's transforms.
        /// </summary>
        private static string DescribeAnchors(UnitEntityView view)
        {
            var text = new StringBuilder();
            text.Append("anchors=[");
            object snapMap = ReadField(view, "m_ParticleSnapMap");
            text.Append("particlesSnapMap=")
                .Append(snapMap == null || snapMap.Equals(null)
                    ? "<null>" : snapMap.GetType().Name);

            object hitFx = null;
            try { hitFx = view.HitFxManager; }
            catch (Exception) { hitFx = null; }
            text.Append(";hitFxManager=")
                .Append(hitFx == null || hitFx.Equals(null)
                    ? "<null>" : hitFx.GetType().Name);

            Transform centerTorso = view.CenterTorso;
            text.Append(";centerTorso=")
                .Append(centerTorso == null ? "<null>" : centerTorso.name);
            text.Append(";corpulence=").Append(view.Corpulence.ToString(
                "0.###", CultureInfo.InvariantCulture));
            text.Append(";scale=").Append(view.transform.localScale.x.ToString(
                "0.###", CultureInfo.InvariantCulture));
            text.Append(']');
            return text.ToString();
        }
    }
}
