using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.View;
using Kingmaker.Visual.Animation.Kingmaker;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private sealed class Cleanup2EttinHead
        {
            internal UnitEntityData Actor;
            internal UnitEntityView View;
            internal Transform Head, Neck, Root, Skull;
            internal SkinnedMeshRenderer Renderer;
            internal Mesh Mesh;
            internal Material[] AssignedMaterials;
            internal UnityEngine.Object[] Resources;
            internal int[] ResourceIds;
            internal string[] ResourceNames, ResourceTypes;
            internal SummonCleanup2EttinLateObserver Observer;
            internal readonly JArray Samples = new JArray();
            internal Vector3 FirstOffset;
            internal Quaternion FirstHeadRotation;
            internal bool FirstSample;
            internal float MaximumOffsetDrift, MaximumRotationError, MaximumHeadMotion;
            internal string Pose = "idle";
        }

        private IEnumerable<int> ReviewSummonCleanup2EttinLifecycle(
            ExpandedSummoningCorrectionFixture fixture, Spellbook book)
        {
            if (!IsSummonCleanup2Review || !_request.ExitAfterCompletion ||
                (string)_request.Parameters?["reviewPhase"] != "ettin-lifecycle" ||
                fixture == null || book == null || _sprint25Clock == null || _expandedSummoningPlayerPathCaptureActive)
                throw new InvalidOperationException("Exact idle Cleanup 2 Ettin lifecycle fixture/book/clock required.");
            bool pause = Game.Instance.IsPaused, waiting = _sprint25WaitingForUnityAppearance;
            var previousCapture = ExpandedSummoningRuleCapture.ToArray();
            var previousEvents = ExpandedSummoningPlayerPathEvents.ToArray();
            int previousCastCount = _expandedSummoningPlayerPathRuleCastCount, previousSpawnCount = _expandedSummoningPlayerPathSpawnActionCount;
            var hooks = HarmonyInstance.Create("kmg.cleanup2.ettin-lifecycle." + _request.RunId);
            var heads = new List<Cleanup2EttinHead>();
            UnitAnimationActionHandle animation = null;
            var watch = Stopwatch.StartNew();
            const BindingFlags statics = BindingFlags.Static | BindingFlags.NonPublic;
            try
            {
                Cleanup2Route route = Cleanup2GiantRoute(fixture, "ettin", SummonFamily.NaturesAlly);
                var creature = ExpandedSummoningCatalog.All.Single(value => value.Key == "ettin");
                bool routeExact = creature.NaturesAllyTier.HasValue && route.Tier == creature.NaturesAllyTier.Value &&
                    route.Unit.AssetGuid == EttinSecondHeadVisual.EttinUnitGuid;
                Cleanup2Check("ettin-lifecycle-route", routeExact, new JObject {
                    ["family"] = route.Family.ToString(), ["tierDerivedFromCatalog"] = route.Tier,
                    ["parent"] = route.Parent.AssetGuid, ["selected"] = route.Selected.AssetGuid,
                    ["unit"] = route.Unit.AssetGuid, ["multiplicity"] = "One",
                    ["maximumWallSeconds"] = 120,
                    ["scope"] = "Two independent canonical single casts, exact native summon-marker expiry, sibling isolation and one real cast-again cycle. Native weapon-specific pose animations are art evidence, not attack resolution. No save, cold-load or hydration claim." },
                    "The exact catalog-derived Ettin SNA own-tier single root supplies all lifecycle actors.");
                if (!routeExact) throw new InvalidOperationException("Exact canonical Ettin route changed.");
                hooks.Patch(typeof(RuleCastSpell).GetMethod("OnTrigger"), null,
                    new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("ExpandedSummoningPlayerPathRuleCastPostfix", statics)), null);
                hooks.Patch(typeof(ContextActionSpawnMonster).GetMethod("RunAction"),
                    new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("ExpandedSummoningPlayerPathSpawnPrefix", statics)), null, null);
                _expandedSummoningPlayerPathCaptureActive = true;
                Game.Instance.IsPaused = false;
                for (int actorIndex = 0; actorIndex < 2; actorIndex++)
                {
                    UnitEntityData actor = Cleanup2CastGiant(fixture, book, route);
                    foreach (int frame in Cleanup2WaitAppearance(actor, "ettin-lifecycle-" + actorIndex)) yield return frame;
                    Cleanup2EttinHead head = Cleanup2CaptureEttinHead(actor, "initial-" + actorIndex);
                    heads.Add(head);
                    head.Observer = head.Root.gameObject.AddComponent<SummonCleanup2EttinLateObserver>();
                    head.Observer.Observe = () => Cleanup2SampleEttinHead(head);
                }
                bool independent = Cleanup2EttinIndependent(heads[0], heads[1]);
                Cleanup2Check("ettin-two-independent-heads", independent, new JObject {
                    ["first"] = Cleanup2EttinDescription(heads[0]), ["second"] = Cleanup2EttinDescription(heads[1]),
                    ["ownedResourceReferencesDisjoint"] = independent },
                    "Two actual simultaneous Ettins have one authored second head each and disjoint live private root/mesh/material objects.");
                _sprint25WaitingForUnityAppearance = false;
                foreach (Cleanup2EttinHead head in heads)
                {
                    foreach (string pose in new[] { "idle", "main-hand", "off-hand" })
                    {
                        Cleanup2FinishEttinAnimation(ref animation);
                        head.Pose = pose;
                        if (pose != "idle")
                        {
                            var hand = pose == "main-hand" ? head.Actor.Body.PrimaryHand : head.Actor.Body.SecondaryHand;
                            if (hand == null || !hand.HasWeapon)
                                throw new InvalidOperationException("Actual Ettin weapon hand is missing: " + pose);
                            var info = new AttackHandInfo(hand, 0, 0);
                            info.CreateAnimationHandleForAttack();
                            animation = info.AnimationHandle;
                            if (animation != null) head.Actor.View.AnimationManager.Execute(animation);
                            Cleanup2Check("ettin-native-pose-" + head.Actor.UniqueId + "-" + pose, animation != null,
                                new JObject { ["unit"] = head.Actor.UniqueId, ["pose"] = pose,
                                    ["weapon"] = info.Weapon.Blueprint.AssetGuid,
                                    ["animationAction"] = animation?.Action?.GetType().FullName,
                                    ["attackWeaponStyle"] = animation?.AttackWeaponStyle.ToString(),
                                    ["nativeAttackCommandIssued"] = false, ["attackRuleTriggered"] = false },
                                "Native AttackHandInfo selects the actual hand's weapon-specific animation without a mechanical attack command or supplied hit.");
                        }
                        int frameCount = pose == "idle" ? 18 : 66;
                        for (int frame = 0; frame < frameCount; frame++)
                        {
                            if (watch.Elapsed.TotalSeconds > 120) throw new TimeoutException("Bounded Ettin lifecycle wall limit.");
                            if (!Cleanup2EttinStable(head) || head.Observer.Error != null)
                                throw new InvalidOperationException("Ettin attachment changed during pose observation: " + head.Observer.Error);
                            Game.Instance.IsPaused = false;
                            yield return 0;
                        }
                    }
                    Cleanup2FinishEttinAnimation(ref animation);
                    // Freeze this packet before putting its JArray into the
                    // atomic observation record; later idle/expiry frames must
                    // not mutate a previously reported sample set.
                    head.Observer.Observe = null;
                    head.Observer.enabled = false;
                    bool poseValid = head.Samples.Count >= 12 && head.MaximumHeadMotion > .1f &&
                        head.MaximumOffsetDrift <= .005f && head.MaximumRotationError <= .5f;
                    Cleanup2Check("ettin-neck-frame-no-orbit-" + head.Actor.UniqueId, poseValid,
                        new JObject { ["unit"] = head.Actor.UniqueId, ["sampleCount"] = head.Samples.Count,
                            ["maximumHeadRotationDegrees"] = head.MaximumHeadMotion,
                            ["maximumNeckLocalOffsetDrift"] = head.MaximumOffsetDrift,
                            ["maximumRotationFollowErrorDegrees"] = head.MaximumRotationError,
                            ["samples"] = head.Samples,
                            ["sampling"] = "Request-owned LateUpdate observer at execution order 32000, after the default-order EttinHeadPoseDriver. Scalar transform points only; source/proxy head transforms are never assigned.",
                            ["limits"] = "No-orbit proof requires positive original-head rotation relative to its neck; an unchanged pose remains unknown. Surface aesthetics, attack mechanics and saved hydration remain separate." },
                        "The original head actually rotates while the authored head maintains one neck-relative offset and follows its rotation without orbiting.");
                }

                var first = heads[0]; var sibling = heads[1];
                JObject expiry = Cleanup2ExpireEttin(fixture, first);
                int expiryFrames = 0;
                var expiryWatch = Stopwatch.StartNew();
                bool siblingStable = true;
                while ((!first.Actor.Destroyed || first.Actor.View != null || first.Actor.HoldingState != null) &&
                    expiryFrames++ < 1800 && expiryWatch.Elapsed.TotalSeconds < 18 && watch.Elapsed.TotalSeconds <= 120)
                {
                    siblingStable &= Cleanup2EttinStable(sibling);
                    yield return 0;
                }
                bool naturallyGone = first.Actor.Destroyed && first.Actor.View == null && first.Actor.HoldingState == null;
                Cleanup2Check("ettin-first-native-expiry", naturallyGone && siblingStable,
                    new JObject { ["unit"] = first.Actor.UniqueId, ["expiry"] = expiry,
                        ["destroyed"] = first.Actor.Destroyed, ["viewGone"] = first.Actor.View == null,
                        ["holdingGone"] = first.Actor.HoldingState == null, ["frames"] = expiryFrames,
                        ["wallSeconds"] = expiryWatch.Elapsed.TotalSeconds, ["siblingAttachmentRetained"] = siblingStable,
                        ["fixtureDestructionUsedForAssertion"] = false },
                    "The exact first native summon marker expires its own logical entity/view while the second Ettin retains the same head/resources.");
                foreach (int frame in Cleanup2DrainEttinResources(new[] { first }, watch)) yield return frame;
                bool firstReleased = first.Resources.All(value => value == null);
                Cleanup2Check("ettin-first-exact-resource-release", naturallyGone && firstReleased && Cleanup2EttinStable(sibling),
                    new JObject { ["firstResources"] = Cleanup2EttinResourceStates(first),
                        ["siblingResources"] = Cleanup2EttinResourceStates(sibling),
                        ["boundedDrain"] = "Native entity destroyer and up to180 Unity frames/6 seconds; surviving exact references are unresolved release, not a claim of an unbounded leak." },
                    "Captured original/controller-created head materials, private mesh and root are destroyed, with the sibling's exact resources still alive.");
                if (!naturallyGone) throw new InvalidOperationException("Ettin expiry did not complete; fixture teardown will retain ownership.");

                UnitEntityData recreatedActor = Cleanup2CastGiant(fixture, book, route);
                foreach (int frame in Cleanup2WaitAppearance(recreatedActor, "ettin-lifecycle-recast")) yield return frame;
                var recreated = Cleanup2CaptureEttinHead(recreatedActor, "recast");
                heads.Add(recreated);
                bool recreatedExact = recreated.Actor.UniqueId != first.Actor.UniqueId &&
                    Cleanup2EttinIndependent(sibling, recreated) && firstReleased && Cleanup2EttinStable(sibling);
                Cleanup2Check("ettin-recast-no-accumulation", recreatedExact,
                    new JObject { ["expiredResources"] = Cleanup2EttinResourceStates(first),
                        ["survivor"] = Cleanup2EttinDescription(sibling), ["recast"] = Cleanup2EttinDescription(recreated),
                        ["activeExactEttinCount"] = heads.Count(head => !head.Actor.Destroyed && head.Actor.View != null),
                        ["recastIsSaveHydration"] = false },
                    "A third actual canonical cast adds exactly one fresh authored head while the expired head remains released and the sibling remains independent.");

                foreach (var head in new[] { sibling, recreated }) Cleanup2ExpireEttin(fixture, head);
                var finalWatch = Stopwatch.StartNew();
                for (int frame = 0; frame < 1800 && finalWatch.Elapsed.TotalSeconds < 18 && watch.Elapsed.TotalSeconds <= 120 &&
                    heads.Any(head => !head.Actor.Destroyed || head.Actor.View != null || head.Actor.HoldingState != null); frame++) yield return 0;
                foreach (int frame in Cleanup2DrainEttinResources(heads, watch)) yield return frame;
                bool allReleased = heads.All(head => head.Actor.Destroyed && head.Actor.View == null && head.Actor.HoldingState == null && head.Resources.All(value => value == null));
                Cleanup2Check("ettin-all-exact-private-resources-released", allReleased,
                    new JObject { ["actors"] = new JArray(heads.Select(Cleanup2EttinResourceStates)),
                        ["wallSeconds"] = watch.Elapsed.TotalSeconds, ["globalSceneScanUsed"] = false,
                        ["viewReconstruction"] = "NOT EXERCISED: native AttachToViewOnLoad is a load lifecycle entry, not an established isolated ordinary-unit reattach seam.",
                        ["coldLoadProof"] = "Separate qualified giant-humanoids-ii Working persistence run remains required; no save writes or fake hydration here." },
                    "All three positively owned lifetime allocations are released through native summon expiry and a bounded destruction drain.");
            }
            finally
            {
                var failures = new List<string>();
                Action<string, Action> restore = (label, action) => { try { action(); } catch (Exception error) { failures.Add(label + ":" + error.GetType().Name + ":" + error.Message); } };
                restore("animation", () => Cleanup2FinishEttinAnimation(ref animation));
                foreach (var head in heads)
                {
                    restore("late-observer-" + head.Actor.UniqueId, () => {
                        if (!ReferenceEquals(head.Observer, null))
                        {
                            head.Observer.Observe = null;
                            if (head.Observer != null) { head.Observer.enabled = false; UnityEngine.Object.Destroy(head.Observer); }
                        }
                    });
                    restore("owned-commands-" + head.Actor.UniqueId, () => { if (!head.Actor.Destroyed) head.Actor.Commands.InterruptAll(true); });
                }
                _expandedSummoningPlayerPathCaptureActive = false;
                restore("capture-hooks", () => hooks.UnpatchAll(hooks.Id));
                restore("capture-state", () => {
                    ExpandedSummoningRuleCapture.Clear(); ExpandedSummoningRuleCapture.AddRange(previousCapture);
                    ExpandedSummoningPlayerPathEvents.Clear(); ExpandedSummoningPlayerPathEvents.AddRange(previousEvents);
                    _expandedSummoningPlayerPathRuleCastCount = previousCastCount;
                    _expandedSummoningPlayerPathSpawnActionCount = previousSpawnCount;
                });
                _sprint25WaitingForUnityAppearance = waiting;
                restore("pause", () => Game.Instance.IsPaused = pause);
                Cleanup2Check("ettin-lifecycle-owned-state-restored", failures.Count == 0,
                    new JObject { ["failures"] = new JArray(failures),
                        ["positiveActorsStillRegistered"] = new JArray(heads.Where(head => fixture.Created.Contains(head.Actor)).Select(head => head.Actor.UniqueId)),
                        ["humanSaveOrWorldMutated"] = false, ["sourceHeadTransformWritten"] = false },
                    "Only owned pose handles/observers/commands and capture/pause fields are restored; shared teardown retains exact positive actors on failure.");
                if (failures.Count > 0) throw new InvalidOperationException("Ettin lifecycle cleanup failed: " + string.Join("|", failures.ToArray()));
            }
        }

        private Cleanup2EttinHead Cleanup2CaptureEttinHead(UnitEntityData actor, string stage)
        {
            Transform[] transforms = actor.View.GetComponentsInChildren<Transform>(true);
            Transform[] roots = transforms.Where(value => value.name == EttinSecondHeadVisual.ObjectPrefix + "_Root").ToArray();
            Transform[] originals = transforms.Where(value => value.name == "Head").ToArray();
            Transform[] skulls = transforms.Where(value => value.name == EttinSecondHeadVisual.ObjectPrefix + "_SkullControl").ToArray();
            if (roots.Length != 1 || originals.Length != 1 || skulls.Length != 1)
                throw new InvalidOperationException("Exact Ettin root/original head/skull counts changed: " + roots.Length + "/" + originals.Length + "/" + skulls.Length);
            var renderers = roots[0].GetComponentsInChildren<SkinnedMeshRenderer>(true);
            if (renderers.Length != 1) throw new InvalidOperationException("Expected exactly one authored Ettin head renderer.");
            UnityEngine.Object[] resources = EttinSecondHeadVisual.CaptureOwnedResources(actor.View);
            var result = new Cleanup2EttinHead { Actor = actor, View = actor.View,
                Head = originals[0], Neck = originals[0].parent, Root = roots[0], Skull = skulls[0],
                Renderer = renderers[0], Mesh = renderers[0].sharedMesh,
                AssignedMaterials = renderers[0].sharedMaterials, Resources = resources,
                ResourceIds = resources.Select(value => value.GetInstanceID()).ToArray(),
                ResourceNames = resources.Select(value => value.name).ToArray(),
                ResourceTypes = resources.Select(value => value.GetType().FullName).ToArray() };
            bool exact = Cleanup2EttinStable(result) && result.Mesh.name == "KMG_Ettin_AuthoredHead" &&
                result.Renderer.bones.Length == 2 && result.Renderer.bones[0] == result.Neck && result.Renderer.bones[1] == result.Skull &&
                result.Renderer.rootBone == result.Neck && result.AssignedMaterials.Length == 4 &&
                resources.OfType<Mesh>().Count() == 1 && resources.OfType<Material>().Count() >= 4 &&
                resources.OfType<Material>().All(value => value.name.StartsWith(EttinSecondHeadVisual.ObjectPrefix, StringComparison.Ordinal));
            Cleanup2Check("ettin-head-attachment-" + stage, exact, Cleanup2EttinDescription(result),
                "One exact authored two-driver head and four live assigned materials are owned by this canonical Ettin; actual original and controller-instantiated resources are retained.");
            if (!exact) throw new InvalidOperationException("Authored Ettin head attachment contract failed: " + stage);
            return result;
        }

        private static bool Cleanup2EttinStable(Cleanup2EttinHead head)
        {
            if (head.Actor.Destroyed || head.Actor.View != head.View || head.View == null || head.Root == null || head.Head == null ||
                head.Neck == null || head.Skull == null || head.Renderer == null || head.Mesh == null ||
                head.Root.parent != head.Neck || head.Skull.parent != head.Root || head.Head.parent != head.Neck ||
                head.Renderer.sharedMesh != head.Mesh || !head.Renderer.sharedMaterials.SequenceEqual(head.AssignedMaterials) ||
                !head.Renderer.enabled || !head.Root.gameObject.activeInHierarchy || head.Resources.Any(value => value == null)) return false;
            return head.View.GetComponentsInChildren<Transform>(true).Count(value => value.name == EttinSecondHeadVisual.ObjectPrefix + "_Root") == 1 &&
                EttinSecondHeadVisual.Describe(head.View).StartsWith("attached;heads=2;", StringComparison.Ordinal);
        }

        private static bool Cleanup2EttinIndependent(Cleanup2EttinHead first, Cleanup2EttinHead second)
        {
            return Cleanup2EttinStable(first) && Cleanup2EttinStable(second) && first.Actor != second.Actor && first.View != second.View &&
                first.Resources.All(value => second.Resources.All(other => !ReferenceEquals(value, other) && value.GetInstanceID() != other.GetInstanceID()));
        }

        private static JObject Cleanup2EttinResourceStates(Cleanup2EttinHead head)
        {
            return new JObject { ["unit"] = head.Actor.UniqueId, ["resources"] = new JArray(head.Resources.Select((value, i) =>
                new JObject { ["capturedInstance"] = head.ResourceIds[i], ["capturedName"] = head.ResourceNames[i],
                    ["capturedType"] = head.ResourceTypes[i], ["nativeObjectAlive"] = value != null })) };
        }

        private static JObject Cleanup2EttinDescription(Cleanup2EttinHead head)
        {
            return new JObject { ["unit"] = head.Actor.UniqueId, ["blueprint"] = head.Actor.Blueprint.AssetGuid,
                ["viewInstance"] = head.View == null ? 0 : head.View.GetInstanceID(), ["attachment"] = EttinSecondHeadVisual.Describe(head.View),
                ["rootPath"] = Cleanup2TransformPath(head.View.transform, head.Root),
                ["originalHeadPath"] = Cleanup2TransformPath(head.View.transform, head.Head),
                ["neckPath"] = Cleanup2TransformPath(head.View.transform, head.Neck),
                ["skullPath"] = Cleanup2TransformPath(head.View.transform, head.Skull),
                ["mesh"] = head.Mesh?.name, ["meshInstance"] = head.Mesh == null ? 0 : head.Mesh.GetInstanceID(),
                ["vertexCount"] = head.Mesh == null ? 0 : head.Mesh.vertexCount, ["subMeshCount"] = head.Mesh == null ? 0 : head.Mesh.subMeshCount,
                ["materials"] = new JArray(head.Renderer.sharedMaterials.Select(Cleanup2Material)),
                ["ownedResources"] = Cleanup2EttinResourceStates(head) };
        }

        private static void Cleanup2SampleEttinHead(Cleanup2EttinHead head)
        {
            if (!Cleanup2EttinStable(head)) throw new InvalidOperationException("Attachment identity changed before late pose sample.");
            Vector3 original = head.Neck.InverseTransformPoint(head.Head.position);
            Vector3 authored = head.Neck.InverseTransformPoint(head.Skull.position);
            Vector3 offset = authored - original;
            Quaternion originalRotation = Quaternion.Inverse(head.Neck.rotation) * head.Head.rotation;
            Quaternion authoredRotation = Quaternion.Inverse(head.Neck.rotation) * head.Skull.rotation;
            if (!Cleanup2Finite(original) || !Cleanup2Finite(authored) || !Cleanup2Finite(offset) ||
                !Cleanup2Finite(new Vector3(originalRotation.x, originalRotation.y, originalRotation.z)) || !Cleanup2Finite(originalRotation.w) ||
                !Cleanup2Finite(new Vector3(authoredRotation.x, authoredRotation.y, authoredRotation.z)) || !Cleanup2Finite(authoredRotation.w))
                throw new InvalidOperationException("Non-finite neck-relative head sample.");
            if (!head.FirstSample) { head.FirstOffset = offset; head.FirstHeadRotation = originalRotation; head.FirstSample = true; }
            head.MaximumOffsetDrift = Mathf.Max(head.MaximumOffsetDrift, Vector3.Distance(offset, head.FirstOffset));
            head.MaximumRotationError = Mathf.Max(head.MaximumRotationError, Quaternion.Angle(originalRotation, authoredRotation));
            head.MaximumHeadMotion = Mathf.Max(head.MaximumHeadMotion, Quaternion.Angle(originalRotation, head.FirstHeadRotation));
            if (head.Samples.Count < 512 && Time.frameCount % 3 == 0)
                head.Samples.Add(new JObject { ["frame"] = Time.frameCount, ["pose"] = head.Pose,
                    ["originalHeadWorld"] = SurveyVector(head.Head.position), ["authoredHeadWorld"] = SurveyVector(head.Skull.position),
                    ["neckWorld"] = SurveyVector(head.Neck.position), ["originalHeadNeckLocal"] = SurveyVector(original),
                    ["authoredHeadNeckLocal"] = SurveyVector(authored), ["neckLocalOffset"] = SurveyVector(offset),
                    ["rotationFollowErrorDegrees"] = Quaternion.Angle(originalRotation, authoredRotation) });
        }

        private static JObject Cleanup2ExpireEttin(ExpandedSummoningCorrectionFixture fixture, Cleanup2EttinHead head)
        {
            if (!fixture.Created.Contains(head.Actor) || !ExpandedSummoningPlayerPathUnitExact(head.Actor, fixture.Caster))
                throw new InvalidOperationException("Exact positive Ettin ownership is required for native expiry.");
            Buff[] markers = head.Actor.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(value =>
                value.Blueprint == BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff).ToArray();
            if (markers.Length != 1) throw new InvalidOperationException("One exact native Ettin summon marker required.");
            return ExpireSprint16OwnedBuff(head.Actor, markers[0]);
        }

        private static IEnumerable<int> Cleanup2DrainEttinResources(IEnumerable<Cleanup2EttinHead> heads, Stopwatch phaseWatch)
        {
            var watch = Stopwatch.StartNew();
            for (int frame = 0; frame < 180 && watch.Elapsed.TotalSeconds < 6 && phaseWatch.Elapsed.TotalSeconds <= 120 &&
                heads.Any(head => head.Resources.Any(value => value != null)); frame++)
            { Game.Instance.EntityDestroyer.Tick(); yield return 0; }
        }

        private static void Cleanup2FinishEttinAnimation(ref UnitAnimationActionHandle handle)
        {
            if (handle == null) return;
            try { handle.IsActed = true; FinishExpandedSummoningAnimation(handle); }
            finally { handle = null; }
        }
    }

    [DefaultExecutionOrder(32000)]
    internal sealed class SummonCleanup2EttinLateObserver : MonoBehaviour
    {
        internal Action Observe;
        internal string Error;
        private void LateUpdate()
        {
            if (Observe == null || Error != null) return;
            try { Observe(); }
            catch (Exception error) { Error = error.GetType().Name + ":" + error.Message; }
        }
    }
}
