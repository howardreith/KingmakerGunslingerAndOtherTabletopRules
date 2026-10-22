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
using Kingmaker.Visual.MaterialEffects;
using Kingmaker.Visual.Particles;
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
            internal Transform NativeJaw, NativeJawEnd;
            internal Transform[] LeftBranch, RightBranch;
            internal SkinnedMeshRenderer NativeRenderer;
            internal Mesh NativeMesh, BakedMesh;
            internal Material[] NativeMaterials;
            internal Texture[] BorrowedSurfaceTextures;
            internal Vector3[] FirstBakedHeadVertices;
            internal int[] PrivateHeadVertexIndices;
            internal float MaximumBakedHeadMotion;
            internal SkinnedMeshRenderer Renderer;
            internal Mesh Mesh;
            internal Material[] AssignedMaterials;
            internal UnityEngine.Object[] Resources;
            internal int[] ResourceIds;
            internal string[] ResourceNames, ResourceTypes;
            internal SummonCleanup2EttinLateObserver Observer;
            internal readonly JArray Samples = new JArray();
            internal readonly JArray Images = new JArray();
            internal readonly JArray MaterialSamples = new JArray(), FadeSamples = new JArray();
            internal bool MaterialControlValid = true;
            internal int MaterialSampleCount, FadeSampleCount, ActiveFadeSampleCount;
            internal float MaximumFadeError, MinimumFade = float.MaxValue, MaximumFade = float.MinValue;
            internal Vector3 FirstOffset;
            internal Vector3 FirstRightOffset;
            internal Quaternion FirstHeadRotation;
            internal bool FirstSample;
            internal float MaximumOffsetDrift, MaximumRotationError, MaximumHeadMotion;
            internal string Pose = "idle";
            internal JObject NativeExpiry;
            internal bool NativeDismemberFactObserved;
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
                    "Two actual simultaneous Ettins have independent full-body skins and two private head chains each, with disjoint private roots/mesh/material allocations.");
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
                            JObject selection = Cleanup2EttinAttackSelection(info);
                            info.CreateAnimationHandleForAttack();
                            animation = info.AnimationHandle;
                            if (animation != null) head.Actor.View.AnimationManager.Execute(animation);
                            Cleanup2Check("ettin-native-pose-" + head.Actor.UniqueId + "-" + pose, animation != null,
                                new JObject { ["unit"] = head.Actor.UniqueId, ["pose"] = pose,
                                    ["weapon"] = info.Weapon.Blueprint.AssetGuid,
                                    ["animationAction"] = animation?.Action?.GetType().FullName,
                                    ["attackWeaponStyle"] = animation?.AttackWeaponStyle.ToString(),
                                    ["nativeSelection"] = selection,
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
                            if ((pose == "idle" && frame == 12) ||
                                (pose != "idle" && (frame == 8 || frame == 24 || frame == 48)))
                                Cleanup2CaptureEttinPictures(head, pose, frame);
                        }
                    }
                    Cleanup2FinishEttinAnimation(ref animation);
                    // Freeze this packet before putting its JArray into the
                    // atomic observation record; later idle/expiry frames must
                    // not mutate a previously reported sample set.
                    head.Observer.Observe = null;
                    head.Observer.enabled = false;
                    bool poseValid = head.Samples.Count >= 12 && head.MaximumHeadMotion > .1f && head.MaximumBakedHeadMotion > .00001f &&
                        head.MaximumOffsetDrift <= .005f && head.MaximumRotationError <= .5f;
                    Cleanup2Check("ettin-neck-frame-no-orbit-" + head.Actor.UniqueId, poseValid,
                        new JObject { ["unit"] = head.Actor.UniqueId, ["sampleCount"] = head.Samples.Count,
                            ["maximumHeadRotationDegrees"] = head.MaximumHeadMotion,
                            ["maximumActualBakedHeadVertexMotion"] = head.MaximumBakedHeadMotion,
                            ["maximumNeckLocalOffsetDrift"] = head.MaximumOffsetDrift,
                            ["maximumRotationFollowErrorDegrees"] = head.MaximumRotationError,
                            ["samples"] = head.Samples,
                            ["nativeImages"] = head.Images,
                            ["sampling"] = "Request-owned LateUpdate observer at execution order 32000, after the default-order EttinHeadPoseDriver. Scalar transform points only; source/proxy head transforms are never assigned.",
                            ["limits"] = "No-orbit proof requires positive original-head rotation relative to its neck; an unchanged pose remains unknown. Surface aesthetics, attack mechanics and saved hydration remain separate." },
                        "The native pose source actually rotates, both private neck origins retain fixed offsets in its UpperTorso frame, all private chain local rotations follow the native chain, and actual baked head vertices deform without an orbit.");
                    Cleanup2Check("ettin-native-material-control-" + head.Actor.UniqueId,
                        head.MaterialSampleCount >= 12 && head.MaterialControlValid,
                        new JObject { ["unit"] = head.Actor.UniqueId, ["samples"] = head.MaterialSamples.DeepClone(),
                            ["sampleCount"] = head.MaterialSampleCount, ["maximumHeadVsSourceDissolveError"] = head.MaximumFadeError,
                            ["forcedFadeOrVisibility"] = false },
                        "Every exact head material stays driven by the source controller with its full cached native baseline and same-frame native dissolve value.");
                }

                var first = heads[0]; var sibling = heads[1];
                Cleanup2EnableEttinFadeObservation(first);
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
                Cleanup2AssertEttinFadeRelease(first);
                Cleanup2Check("ettin-first-exact-resource-release", naturallyGone && firstReleased && Cleanup2EttinStable(sibling) && Cleanup2EttinControllerReleased(first),
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
                    "A third actual canonical cast adds one fresh private full body with two head branches while the expired body remains released and the sibling remains independent.");

                foreach (var head in new[] { sibling, recreated })
                { Cleanup2EnableEttinFadeObservation(head); Cleanup2ExpireEttin(fixture, head); }
                var finalWatch = Stopwatch.StartNew();
                for (int frame = 0; frame < 1800 && finalWatch.Elapsed.TotalSeconds < 18 && watch.Elapsed.TotalSeconds <= 120 &&
                    heads.Any(head => !head.Actor.Destroyed || head.Actor.View != null || head.Actor.HoldingState != null); frame++) yield return 0;
                foreach (int frame in Cleanup2DrainEttinResources(heads, watch)) yield return frame;
                foreach (var head in new[] { sibling, recreated }) Cleanup2AssertEttinFadeRelease(head);
                bool allReleased = heads.All(head => head.Actor.Destroyed && head.Actor.View == null && head.Actor.HoldingState == null &&
                    head.Resources.All(value => value == null) && Cleanup2EttinControllerReleased(head));
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
                    restore("owned-bake-probe-" + head.Actor.UniqueId, () => {
                        if (head.BakedMesh != null) UnityEngine.Object.DestroyImmediate(head.BakedMesh);
                        head.BakedMesh = null;
                    });
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
            Transform[] originals = transforms.Where(value => value.name == "Head").ToArray();
            var renderers = actor.View.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .Where(value => value.name == EttinSecondHeadVisual.ObjectPrefix + "_Body").ToArray();
            var natives = actor.View.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .Where(value => value.name == "hill_giant_1" && value.sharedMesh != null && value.sharedMesh.name == "hill_giant_1").ToArray();
            if (originals.Length != 1 || renderers.Length != 1 || natives.Length != 1)
                throw new InvalidOperationException("Exact native pose source/private full body/source renderer counts changed.");
            UnityEngine.Object[] resources = EttinSecondHeadVisual.CaptureOwnedResources(actor.View);
            GameObject[] roots = resources.OfType<GameObject>().Where(value => value != null &&
                renderers[0].transform.IsChildOf(value.transform)).ToArray();
            if (roots.Length != 1) throw new InvalidOperationException("One exact production-owned full-body root required.");
            Transform[] left = Cleanup2EttinBranch(transforms, "Left"), right = Cleanup2EttinBranch(transforms, "Right");
            var result = new Cleanup2EttinHead { Actor = actor, View = actor.View,
                Head = originals[0], Neck = originals[0].parent, Root = roots[0].transform, Skull = left[1],
                NativeJaw = originals[0].GetComponentsInChildren<Transform>(true).Single(value => value.name == "Jaw"),
                NativeJawEnd = originals[0].GetComponentsInChildren<Transform>(true).Single(value => value.name == "Jaw_end"),
                LeftBranch = left, RightBranch = right, NativeRenderer = natives[0], NativeMesh = natives[0].sharedMesh,
                NativeMaterials = natives[0].sharedMaterials,
                BorrowedSurfaceTextures = new[] { "_MainTex", "_Parameters", "_BumpMap" }
                    .Select(name => natives[0].sharedMaterial.GetTexture(name)).ToArray(),
                Renderer = renderers[0], Mesh = renderers[0].sharedMesh,
                AssignedMaterials = renderers[0].sharedMaterials, Resources = resources,
                ResourceIds = resources.Select(value => value.GetInstanceID()).ToArray(),
                ResourceNames = resources.Select(value => value.name).ToArray(),
                ResourceTypes = resources.Select(value => value.GetType().FullName).ToArray() };
            bool exact = Cleanup2EttinStable(result);
            JObject attachmentMaterial;
            exact &= Cleanup2EttinAttachmentMaterialExact(actor.View, out attachmentMaterial);
            JObject description = Cleanup2EttinDescription(result);
            description["attachmentMaterialContract"] = attachmentMaterial;
            Cleanup2Check("ettin-head-attachment-" + stage, exact, description,
                "One private full-body skin suppresses only this instance's native body; two exact private neck/head/jaw chains and one owned material preserve the native rig, weapon renderers and borrowed native surface resources.");
            if (!exact) throw new InvalidOperationException("Ettin full-body attachment contract failed: " + stage);
            result.PrivateHeadVertexIndices = Cleanup2EttinHeadVertices(result.Renderer, left, right);
            result.BakedMesh = new Mesh { name = "KMG_Cleanup2_Ettin_RequestOwnedBakeProbe" };
            return result;
        }

        private static Transform[] Cleanup2EttinBranch(Transform[] transforms, string side)
        {
            return new[] { "Neck", "Head", "Jaw", "JawEnd" }.Select(suffix => transforms.Single(value =>
                value.name == EttinSecondHeadVisual.ObjectPrefix + "_" + side + suffix)).ToArray();
        }

        private static bool Cleanup2EttinBranchExact(Transform[] branch)
        {
            return branch != null && branch.Length == 4 && branch.All(value => value != null) &&
                branch[1].parent == branch[0] && branch[2].parent == branch[1] && branch[3].parent == branch[2];
        }

        private static bool Cleanup2EttinStable(Cleanup2EttinHead head)
        {
            if (head.Actor.Destroyed || head.Actor.View != head.View || head.View == null || head.Root == null || head.Head == null ||
                head.Neck == null || head.Skull == null || head.Renderer == null || head.Mesh == null ||
                !Cleanup2EttinBranchExact(head.LeftBranch) || !Cleanup2EttinBranchExact(head.RightBranch) ||
                head.LeftBranch[0].parent != head.Neck.parent || head.RightBranch[0].parent != head.Neck.parent ||
                head.Head.parent != head.Neck || head.NativeRenderer == null || head.NativeRenderer.enabled ||
                head.NativeRenderer.sharedMesh != head.NativeMesh || !head.NativeRenderer.sharedMaterials.SequenceEqual(head.NativeMaterials) ||
                head.Renderer.sharedMesh != head.Mesh || !head.Renderer.sharedMaterials.SequenceEqual(head.AssignedMaterials) ||
                head.Root.parent != head.NativeRenderer.transform.parent || head.Renderer.transform.parent != head.Root ||
                !head.Renderer.enabled || !head.Root.gameObject.activeInHierarchy || head.Resources.Any(value => value == null)) return false;
            Material[] materials = head.Renderer.sharedMaterials;
            return materials.Length == 1 && new[] { "_MainTex", "_Parameters", "_BumpMap" }
                .Select((name, index) => ReferenceEquals(materials[0].GetTexture(name), head.BorrowedSurfaceTextures[index])).All(value => value) &&
                head.Mesh != head.NativeMesh && head.Resources.All(value => !ReferenceEquals(value, head.NativeMesh) &&
                    head.NativeMaterials.All(material => !ReferenceEquals(value, material)) &&
                    head.BorrowedSurfaceTextures.All(texture => !ReferenceEquals(value, texture)));
        }

        private static bool Cleanup2EttinIndependent(Cleanup2EttinHead first, Cleanup2EttinHead second)
        {
            return Cleanup2EttinStable(first) && Cleanup2EttinStable(second) && first.Actor != second.Actor && first.View != second.View &&
                first.Resources.All(value => second.Resources.All(other => !ReferenceEquals(value, other) && value.GetInstanceID() != other.GetInstanceID()));
        }

        private static JObject Cleanup2EttinResourceStates(Cleanup2EttinHead head)
        {
            return new JObject { ["unit"] = head.Actor.UniqueId,
                ["materialControl"] = EttinSecondHeadVisual.ObserveMaterialControl(head.View),
                ["resources"] = new JArray(head.Resources.Select((value, i) =>
                new JObject { ["capturedInstance"] = head.ResourceIds[i], ["capturedName"] = head.ResourceNames[i],
                    ["capturedType"] = head.ResourceTypes[i], ["nativeObjectAlive"] = value != null })),
                ["nativeSharedMeshAlive"] = head.NativeMesh != null,
                ["borrowedSurfaceTexturesAlive"] = new JArray(head.BorrowedSurfaceTextures.Select(value => ReferenceEquals(value, null) || value != null)),
                ["nativeSharedObjectsExcludedFromPrivateAllocations"] = head.Resources.All(value => !ReferenceEquals(value, head.NativeMesh) &&
                    head.NativeMaterials.All(material => !ReferenceEquals(value, material)) &&
                    head.BorrowedSurfaceTextures.All(texture => !ReferenceEquals(value, texture))) };
        }

        private static JObject Cleanup2EttinDescription(Cleanup2EttinHead head)
        {
            return new JObject { ["unit"] = head.Actor.UniqueId, ["blueprint"] = head.Actor.Blueprint.AssetGuid,
                ["viewInstance"] = head.View == null ? 0 : head.View.GetInstanceID(), ["attachment"] = EttinSecondHeadVisual.Describe(head.View),
                ["rootPath"] = Cleanup2TransformPath(head.View.transform, head.Root),
                ["originalHeadPath"] = Cleanup2TransformPath(head.View.transform, head.Head),
                ["neckPath"] = Cleanup2TransformPath(head.View.transform, head.Neck),
                ["skullPath"] = Cleanup2TransformPath(head.View.transform, head.Skull),
                ["leftHeadChain"] = Cleanup2EttinBranchReceipt(head.View, head.LeftBranch),
                ["rightHeadChain"] = Cleanup2EttinBranchReceipt(head.View, head.RightBranch),
                ["nativeBodySuppressed"] = !head.NativeRenderer.enabled,
                ["nativeSharedMesh"] = head.NativeMesh.GetInstanceID(),
                ["originalHeadWorld"] = SurveyVector(head.Head.position),
                ["originalHeadNeckLocal"] = SurveyVector(head.Neck.InverseTransformPoint(head.Head.position)),
                ["skullWorld"] = SurveyVector(head.Skull.position),
                ["skullNeckLocal"] = SurveyVector(head.Neck.InverseTransformPoint(head.Skull.position)),
                ["neckWorld"] = SurveyVector(head.Neck.position),
                ["nativeHeadWorldRotation"] = Cleanup2EttinRotation(head.Head.rotation),
                ["nativeHeadLocalRotation"] = Cleanup2EttinRotation(head.Head.localRotation),
                ["nativeHeadLocalScale"] = SurveyVector(head.Head.localScale),
                ["nativeHeadLossyScale"] = SurveyVector(head.Head.lossyScale),
                ["nativeNeckWorldRotation"] = Cleanup2EttinRotation(head.Neck.rotation),
                ["nativeNeckLocalRotation"] = Cleanup2EttinRotation(head.Neck.localRotation),
                ["nativeNeckLocalScale"] = SurveyVector(head.Neck.localScale),
                ["nativeNeckLossyScale"] = SurveyVector(head.Neck.lossyScale),
                ["authoredRendererRootWorld"] = SurveyVector(head.Root.position),
                ["authoredRenderBoundsCenter"] = SurveyVector(head.Renderer.bounds.center),
                ["authoredRenderBoundsSize"] = SurveyVector(head.Renderer.bounds.size),
                ["upperBodyPoints"] = new JArray(head.View.GetComponentsInChildren<Transform>(true)
                    .Where(value => value.name == "UpperTorso" || value.name.IndexOf("Shoulder", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        value.name.IndexOf("UpperArm", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        value.name.IndexOf("Clavicle", StringComparison.OrdinalIgnoreCase) >= 0).Take(16).Select(value =>
                        new JObject { ["path"] = Cleanup2TransformPath(head.View.transform, value), ["world"] = SurveyVector(value.position),
                            ["neckLocal"] = SurveyVector(head.Neck.InverseTransformPoint(value.position)) })),
                ["mesh"] = head.Mesh?.name, ["meshInstance"] = head.Mesh == null ? 0 : head.Mesh.GetInstanceID(),
                ["vertexCount"] = head.Mesh == null ? 0 : head.Mesh.vertexCount, ["subMeshCount"] = head.Mesh == null ? 0 : head.Mesh.subMeshCount,
                ["materials"] = new JArray(head.Renderer.sharedMaterials.Select(Cleanup2Material)),
                ["materialControl"] = Cleanup2EttinMaterialControl(head.View),
                ["ownedResources"] = Cleanup2EttinResourceStates(head) };
        }

        private static JArray Cleanup2EttinBranchReceipt(UnitEntityView view, Transform[] branch)
        {
            return new JArray(branch.Select(value => new JObject {
                ["name"] = value.name, ["instance"] = value.GetInstanceID(),
                ["path"] = Cleanup2TransformPath(view.transform, value),
                ["parent"] = value.parent == null ? 0 : value.parent.GetInstanceID(),
                ["localPosition"] = SurveyVector(value.localPosition),
                ["localRotation"] = Cleanup2EttinRotation(value.localRotation),
                ["localScale"] = SurveyVector(value.localScale) }));
        }

        private static int[] Cleanup2EttinHeadVertices(SkinnedMeshRenderer renderer, Transform[] left, Transform[] right)
        {
            var indices = new HashSet<int>(left.Concat(right).Select(value => Array.IndexOf(renderer.bones, value)));
            return renderer.sharedMesh.boneWeights.Select((weight, index) => new { weight, index }).Where(value =>
                (value.weight.weight0 > 0f && indices.Contains(value.weight.boneIndex0)) ||
                (value.weight.weight1 > 0f && indices.Contains(value.weight.boneIndex1)) ||
                (value.weight.weight2 > 0f && indices.Contains(value.weight.boneIndex2)) ||
                (value.weight.weight3 > 0f && indices.Contains(value.weight.boneIndex3))).Select(value => value.index).ToArray();
        }

        private static bool Cleanup2EttinSkinningExact(SkinnedMeshRenderer renderer, SkinnedMeshRenderer source, Transform[] left, Transform[] right,
            Transform nativeHead, out JObject receipt)
        {
            Mesh mesh = renderer == null ? null : renderer.sharedMesh;
            receipt = new JObject { ["privateMeshCpuReadable"] = mesh != null && mesh.isReadable, ["verticesExported"] = false };
            if (mesh == null || !mesh.isReadable || mesh.vertexCount <= 0 || mesh.vertexCount > 100000 ||
                renderer.bones == null || renderer.bones.Length < 8 || renderer.bones.Length > 512) return false;
            Vector3[] vertices = mesh.vertices, normals = mesh.normals;
            Vector2[] uv = mesh.uv;
            BoneWeight[] weights = mesh.boneWeights;
            Matrix4x4[] binds = mesh.bindposes;
            Transform[] bones = renderer.bones;
            bool nativePalette = source != null && bones.SequenceEqual(source.bones.Concat(left).Concat(right)) && renderer.rootBone == source.rootBone;
            Matrix4x4[] sourceBinds = source == null || source.sharedMesh == null ? new Matrix4x4[0] : source.sharedMesh.bindposes;
            bool sourceBindPrefix = sourceBinds.Length >= 7 && binds.Length == sourceBinds.Length + 8;
            bool privateBinds = sourceBindPrefix;
            if (sourceBindPrefix)
            {
                for (int index = 0; index < sourceBinds.Length; index++) for (int row = 0; row < 4; row++) for (int col = 0; col < 4; col++)
                    sourceBindPrefix &= binds[index][row, col] == sourceBinds[index][row, col];
                for (int branch = 0; branch < 2; branch++) for (int index = 0; index < 4; index++)
                {
                    Matrix4x4 expected = sourceBinds[3 + index] * Matrix4x4.Translate(-EttinBodyMesh.BranchOffset(branch == 0 ? -1 : 1));
                    for (int row = 0; row < 4; row++) for (int col = 0; col < 4; col++)
                        privateBinds &= binds[sourceBinds.Length + branch * 4 + index][row, col] == expected[row, col];
                }
            }
            bool arrays = normals.Length == vertices.Length && weights.Length == vertices.Length && uv.Length == vertices.Length && binds.Length == bones.Length;
            bool finite = vertices.All(Cleanup2Finite) && normals.All(value => Cleanup2Finite(value) && Mathf.Abs(value.sqrMagnitude - 1f) <= .02f) &&
                uv.All(value => Cleanup2Finite(value.x) && Cleanup2Finite(value.y)) && bones.All(value => value != null);
            foreach (Matrix4x4 bind in binds) for (int row = 0; row < 4; row++) for (int col = 0; col < 4; col++)
                finite &= Cleanup2Finite(bind[row, col]);
            int invalidIndices = 0, unnormalizedWeights = 0, nativeHeadInfluences = 0;
            var nativeHeadBones = new HashSet<int>(nativeHead.GetComponentsInChildren<Transform>(true).Select(value => Array.IndexOf(bones, value)));
            foreach (BoneWeight weight in weights)
            {
                float[] values = { weight.weight0, weight.weight1, weight.weight2, weight.weight3 };
                int[] indices = { weight.boneIndex0, weight.boneIndex1, weight.boneIndex2, weight.boneIndex3 };
                if (values.Any(value => !Cleanup2Finite(value) || value < 0f) || Mathf.Abs(values.Sum() - 1f) > .0001f) unnormalizedWeights++;
                for (int index = 0; index < 4; index++) if (values[index] > 0f)
                {
                    if (indices[index] < 0 || indices[index] >= bones.Length) invalidIndices++;
                    if (nativeHeadBones.Contains(indices[index])) nativeHeadInfluences++;
                }
            }
            bool triangles = mesh.subMeshCount > 0;
            int triangleCount = 0;
            for (int sub = 0; sub < mesh.subMeshCount; sub++)
            {
                int[] indices = mesh.GetTriangles(sub);
                triangles &= indices.Length % 3 == 0 && indices.All(value => value >= 0 && value < vertices.Length);
                triangleCount += indices.Length / 3;
            }
            bool branches = left.Concat(right).All(value => Array.IndexOf(bones, value) >= 0);
            int leftVertices = Cleanup2EttinHeadVertices(renderer, left, new Transform[0]).Length;
            int rightVertices = Cleanup2EttinHeadVertices(renderer, right, new Transform[0]).Length;
            receipt["vertexCount"] = vertices.Length; receipt["triangleCount"] = triangleCount;
            receipt["boneCount"] = bones.Length; receipt["bindPoseCount"] = binds.Length;
            receipt["arraysMatch"] = arrays; receipt["finitePositionsNormalsUvsBinds"] = finite;
            receipt["invalidPositiveWeightIndices"] = invalidIndices; receipt["unnormalizedWeights"] = unnormalizedWeights;
            receipt["nativeOriginalHeadPositiveInfluences"] = nativeHeadInfluences;
            receipt["validTriangleIndices"] = triangles; receipt["bothFullPrivateChainsInPalette"] = branches;
            receipt["nativePaletteOrderAndRootBoneRetained"] = nativePalette;
            receipt["nativeBindPosePrefixExact"] = sourceBindPrefix; receipt["privateBranchBindFramesExact"] = privateBinds;
            receipt["leftHeadBranchWeightedVertices"] = leftVertices; receipt["rightHeadBranchWeightedVertices"] = rightVertices;
            return arrays && finite && triangles && triangleCount > 0 && branches && nativePalette && sourceBindPrefix && privateBinds && invalidIndices == 0 &&
                unnormalizedWeights == 0 && leftVertices > 0 && rightVertices > 0;
        }

        private void Cleanup2CaptureEttinPictures(Cleanup2EttinHead head, string pose, int frame)
        {
            if (!Cleanup2EttinStable(head)) throw new InvalidOperationException("Exact live Ettin head required for native art capture.");
            var rig = TeleportationCastingCamera();
            Camera camera = rig == null ? Camera.main : rig.Camera;
            if (camera == null) throw new InvalidOperationException("No native camera for Ettin art observation.");
            Bounds prior = _refinementBounds;
            try
            {
                // Culling bounds frame pictures only. No anatomy height or
                // mechanical result is inferred from these screenshots.
                float headFrameSize = Mathf.Max(.5f, head.Head.lossyScale.magnitude * .45f);
                Bounds close = new Bounds(head.LeftBranch[1].position, Vector3.one * headFrameSize);
                close.Encapsulate(new Bounds(head.RightBranch[1].position, Vector3.one * headFrameSize));
                close.Encapsulate(head.LeftBranch[2].position); close.Encapsulate(head.RightBranch[2].position);
                var bounds = new List<Bounds> { close };
                var labels = new List<string> { "heads" };
                if (pose == "idle")
                {
                    Bounds body = close;
                    foreach (var skin in head.View.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                        .Where(value => value.enabled && value.gameObject.activeInHierarchy)) body.Encapsulate(skin.bounds);
                    bounds.Add(body); labels.Add("body");
                }
                for (int framing = 0; framing < bounds.Count; framing++)
                {
                    _refinementBounds = bounds[framing];
                    float radius = Mathf.Max(.5f, _refinementBounds.extents.magnitude);
                    float distance = radius / Mathf.Tan(camera.fieldOfView * .5f * Mathf.Deg2Rad) * 1.2f;
                    foreach (float yaw in new[] { -40f, 75f, -105f })
                    {
                        Vector3 toward = Quaternion.Euler(0, yaw, 0) * RefinementTurn(head.Actor, 0f);
                        Vector3 eye = _refinementBounds.center +
                            (toward * Mathf.Cos(18f * Mathf.Deg2Rad) + Vector3.up * Mathf.Sin(18f * Mathf.Deg2Rad)) * distance;
                        string file = "cleanup2-ettin-" + head.Actor.UniqueId + "-" + pose + "-" + frame +
                            "-" + labels[framing] + "-" + yaw + ".png";
                        if (System.IO.File.Exists(System.IO.Path.Combine(_request.EvidenceDirectory, file)))
                            throw new InvalidOperationException("Native Ettin art evidence cannot be overwritten.");
                        string result = WriteRefinementImage(eye,
                            Quaternion.LookRotation(_refinementBounds.center - eye, Vector3.up), file);
                        var row = new JObject { ["file"] = file, ["pose"] = pose, ["poseFrame"] = frame,
                            ["framing"] = labels[framing], ["yaw"] = yaw, ["result"] = result,
                            ["requestedCameraWorld"] = SurveyVector(eye), ["targetWorld"] = SurveyVector(_refinementBounds.center),
                            ["faderVisible"] = EntityFadedIn(head.Actor), ["dissolve"] = DissolveAmount(head.Actor),
                            ["mechanicalAttackProof"] = false, ["ownerVisualApproval"] = false };
                        head.Images.Add(row);
                        bool captured = result.StartsWith("png=" + file + ";", StringComparison.Ordinal) &&
                            EntityFadedIn(head.Actor) && DissolveAmount(head.Actor) <= MotionReviewIntactDissolve;
                        Cleanup2Check("ettin-native-art-" + head.Actor.UniqueId + "-" + pose + "-" + frame + "-" + labels[framing] + "-" + yaw,
                            captured, row, "An exact live native camera frame records the authored heads/neck and body with settled normal visibility; pixels need review.");
                        if (!captured) throw new InvalidOperationException("Native Ettin art capture is unqualified: " + result);
                    }
                }
            }
            finally { _refinementBounds = prior; }
        }

        private static JArray Cleanup2EttinRotation(Quaternion value)
        { return new JArray(value.x, value.y, value.z, value.w); }

        private static void Cleanup2SampleEttinHead(Cleanup2EttinHead head)
        {
            if (!Cleanup2EttinStable(head)) throw new InvalidOperationException("Attachment identity changed before late pose sample.");
            Cleanup2SampleEttinMaterial(head, false);
            Transform torso = head.Neck.parent;
            Vector3 offset = torso.InverseTransformPoint(head.LeftBranch[0].position) - head.Neck.localPosition;
            Vector3 rightOffset = torso.InverseTransformPoint(head.RightBranch[0].position) - head.Neck.localPosition;
            Quaternion originalRotation = Quaternion.Inverse(head.Neck.rotation) * head.Head.rotation;
            Transform[] native = { head.Neck, head.Head, head.NativeJaw, head.NativeJawEnd };
            float rotationError = 0f, childPositionError = 0f, scaleError = 0f;
            foreach (Transform[] branch in new[] { head.LeftBranch, head.RightBranch }) for (int index = 0; index < 4; index++)
            {
                rotationError = Mathf.Max(rotationError, Quaternion.Angle(native[index].localRotation, branch[index].localRotation));
                scaleError = Mathf.Max(scaleError, Vector3.Distance(native[index].localScale, branch[index].localScale));
                if (index > 0) childPositionError = Mathf.Max(childPositionError, Vector3.Distance(native[index].localPosition, branch[index].localPosition));
            }
            if (!Cleanup2Finite(offset) || !Cleanup2Finite(rightOffset) || !Cleanup2Finite(rotationError) ||
                !Cleanup2Finite(scaleError) || !Cleanup2Finite(childPositionError) || scaleError > .0001f || childPositionError > .0001f ||
                !Cleanup2Finite(new Vector3(originalRotation.x, originalRotation.y, originalRotation.z)) || !Cleanup2Finite(originalRotation.w))
                throw new InvalidOperationException("Non-finite neck-relative head sample.");
            if (!head.FirstSample) { head.FirstOffset = offset; head.FirstRightOffset = rightOffset; head.FirstHeadRotation = originalRotation; head.FirstSample = true; }
            head.MaximumOffsetDrift = Mathf.Max(head.MaximumOffsetDrift,
                Mathf.Max(Vector3.Distance(offset, head.FirstOffset), Vector3.Distance(rightOffset, head.FirstRightOffset)));
            head.MaximumRotationError = Mathf.Max(head.MaximumRotationError, rotationError);
            head.MaximumHeadMotion = Mathf.Max(head.MaximumHeadMotion, Quaternion.Angle(originalRotation, head.FirstHeadRotation));
            if (head.Samples.Count < 512 && Time.frameCount % 3 == 0)
            {
                head.Renderer.BakeMesh(head.BakedMesh);
                Vector3[] baked = head.BakedMesh.vertices;
                if (baked.Length != head.Mesh.vertexCount || !baked.All(Cleanup2Finite) || head.PrivateHeadVertexIndices.Length == 0)
                    throw new InvalidOperationException("Actual private full-body BakeMesh/head influence audit failed.");
                Vector3[] selected = head.PrivateHeadVertexIndices.Select(index => baked[index]).ToArray();
                if (head.FirstBakedHeadVertices == null) head.FirstBakedHeadVertices = selected;
                for (int index = 0; index < selected.Length; index++)
                    head.MaximumBakedHeadMotion = Mathf.Max(head.MaximumBakedHeadMotion, Vector3.Distance(selected[index], head.FirstBakedHeadVertices[index]));
                head.Samples.Add(new JObject { ["frame"] = Time.frameCount, ["pose"] = head.Pose,
                    ["nativeHeadWorld"] = SurveyVector(head.Head.position),
                    ["leftHeadWorld"] = SurveyVector(head.LeftBranch[1].position), ["rightHeadWorld"] = SurveyVector(head.RightBranch[1].position),
                    ["leftNeckOffsetInUpperTorso"] = SurveyVector(offset), ["rightNeckOffsetInUpperTorso"] = SurveyVector(rightOffset),
                    ["nativeHeadWorldRotation"] = Cleanup2EttinRotation(head.Head.rotation),
                    ["nativeHeadLocalRotation"] = Cleanup2EttinRotation(head.Head.localRotation),
                    ["nativeNeckWorldRotation"] = Cleanup2EttinRotation(head.Neck.rotation),
                    ["nativeNeckLocalRotation"] = Cleanup2EttinRotation(head.Neck.localRotation),
                    ["chainRotationFollowErrorDegrees"] = rotationError, ["chainChildPositionError"] = childPositionError,
                    ["chainScaleError"] = scaleError, ["actualBakedVertexCount"] = baked.Length,
                    ["actualHeadWeightedVertexCount"] = selected.Length, ["maximumActualBakedHeadVertexMotion"] = head.MaximumBakedHeadMotion,
                    ["bakedVerticesExported"] = false });
            }
        }

        private static JObject Cleanup2EttinMaterialControl(UnitEntityView view)
        {
            JObject result = EttinSecondHeadVisual.ObserveMaterialControl(view);
            var bodies = view == null ? new SkinnedMeshRenderer[0] : view.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .Where(skin => skin.name == "hill_giant_1" && skin.sharedMesh != null && skin.sharedMesh.name == "hill_giant_1").ToArray();
            Material[] sourceMaterials = bodies.Length == 1 ? bodies[0].sharedMaterials : new Material[0];
            Material source = sourceMaterials.Length == 1 ? sourceMaterials[0] : null;
            StandardMaterialController controller = bodies.Length == 1 ? bodies[0].GetComponentInParent<StandardMaterialController>() : null;
            float sourceFade = source != null && source.HasProperty("_Dissolve") ? source.GetFloat("_Dissolve") : float.NaN;
            var materials = result["headMaterials"] as JArray;
            var protectedRenderers = result["protectedRenderers"] as JArray;
            bool scope = (bool?)result["nativeBodySuppressed"] == true && (bool?)result["sourceMeshReferencePreserved"] == true &&
                protectedRenderers != null && protectedRenderers.Count > 0 &&
                protectedRenderers.OfType<JObject>().Count() == protectedRenderers.Count &&
                protectedRenderers.OfType<JObject>().All(row => (int?)row["renderer"] != null && (int?)row["renderer"] != 0 &&
                    (bool?)row["alive"] == true && (bool?)row["enabledMatchesAttach"] == true &&
                    (bool?)row["meshReferencePreserved"] == true && (bool?)row["materialReferencesPreserved"] == true) &&
                protectedRenderers.OfType<JObject>().Select(row => (int)row["renderer"]).Distinct().Count() == protectedRenderers.Count;
            bool exact = (bool?)result["attachmentPresent"] == true && (int?)result["assignedCount"] == 1 &&
                materials != null && materials.Count == 1 && materials.OfType<JObject>().Count() == 1 && controller != null &&
                (int?)result["controller"] == controller.GetInstanceID() && Cleanup2Finite(sourceFade) && scope;
            float maxError = 0f;
            if (materials != null) foreach (JObject row in materials.OfType<JObject>())
            {
                float fade = row["liveDissolve"] == null || row["liveDissolve"].Type == JTokenType.Null ? float.NaN : (float)row["liveDissolve"];
                exact &= (bool?)row["controllerMember"] == true && (bool?)row["nativeDefaultCached"] == true &&
                    (bool?)row["fullCachedFadeBaselineMatchesSource"] == true && Cleanup2Finite(fade);
                if (Cleanup2Finite(fade) && Cleanup2Finite(sourceFade)) maxError = Mathf.Max(maxError, Mathf.Abs(fade - sourceFade));
            }
            MethodInfo count = typeof(StandardMaterialController).GetMethod("GetDissolveAnimationsCount",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            int active = controller == null || count == null || count.ReturnType != typeof(int) ? -1 : (int)count.Invoke(controller, null);
            exact &= maxError <= .0001f && active >= 0;
            result["sourceRendererCount"] = bodies.Length; result["sourceMaterialCount"] = sourceMaterials.Length;
            result["sourceLiveDissolve"] = Cleanup2Finite(sourceFade) ? (JToken)new JValue(sourceFade) : JValue.CreateNull();
            result["nativeRendererScopePreserved"] = scope;
            result["maximumHeadVsSourceDissolveError"] = maxError; result["nativeDissolveAnimationCount"] = active;
            result["exactDrivenBaselineAndSameNativeFade"] = exact;
            return result;
        }

        // Shared with the existing exact giant-humanoids-ii persistence scope.
        // Reads the actual hydrated view; no visual attach/initialization call.
        private static bool Cleanup2EttinAttachmentMaterialExact(UnitEntityView view, out JObject receipt)
        {
            receipt = Cleanup2EttinMaterialControl(view);
            Transform[] transforms = view == null ? new Transform[0] : view.GetComponentsInChildren<Transform>(true);
            Transform[] roots = transforms.Where(value => value.name == EttinSecondHeadVisual.ObjectPrefix + "_Root").ToArray();
            Transform[] heads = transforms.Where(value => value.name == "Head").ToArray();
            var skins = view == null ? new SkinnedMeshRenderer[0] : view.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .Where(value => value.name == EttinSecondHeadVisual.ObjectPrefix + "_Body").ToArray();
            var native = view == null ? new SkinnedMeshRenderer[0] : view.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .Where(value => value.name == "hill_giant_1" && value.sharedMesh != null && value.sharedMesh.name == "hill_giant_1").ToArray();
            UnityEngine.Object[] resources = EttinSecondHeadVisual.CaptureOwnedResources(view);
            Transform[] left = null, right = null;
            string branchError = null;
            try { left = Cleanup2EttinBranch(transforms, "Left"); right = Cleanup2EttinBranch(transforms, "Right"); }
            catch (InvalidOperationException error) { branchError = error.Message; }
            Material source = native.Length == 1 && native[0].sharedMaterials.Length == 1 ? native[0].sharedMaterial : null;
            Material assigned = skins.Length == 1 && skins[0].sharedMaterials.Length == 1 ? skins[0].sharedMaterial : null;
            bool ownedSlots = assigned != null && source != null && !ReferenceEquals(assigned, source) &&
                assigned.shader == source.shader && assigned.shader != null && assigned.shader.name == "PF/StandardDynamic" &&
                resources.Any(value => ReferenceEquals(value, assigned)) && resources.OfType<Texture>().Count() == 0 &&
                new[] { "_MainTex", "_Parameters", "_BumpMap" }.All(name => assigned.HasProperty(name) && source.HasProperty(name) &&
                    ReferenceEquals(assigned.GetTexture(name), source.GetTexture(name)) &&
                    assigned.GetTextureScale(name).Equals(source.GetTextureScale(name)) && assigned.GetTextureOffset(name).Equals(source.GetTextureOffset(name)));
            JObject skinning = new JObject();
            bool skinningExact = skins.Length == 1 && heads.Length == 1 && left != null && right != null &&
                Cleanup2EttinSkinningExact(skins[0], native.Length == 1 ? native[0] : null, left, right, heads[0], out skinning);
            bool shape = view != null && view.EntityData != null && view.EntityData.Blueprint.AssetGuid == EttinSecondHeadVisual.EttinUnitGuid &&
                roots.Length == 1 && heads.Length == 1 && skins.Length == 1 && native.Length == 1 && !native[0].enabled &&
                Cleanup2EttinBranchExact(left) && Cleanup2EttinBranchExact(right) &&
                left[0].parent == heads[0].parent.parent && right[0].parent == heads[0].parent.parent &&
                Vector3.Distance(left[0].localPosition, right[0].localPosition) > .0001f &&
                roots[0].parent == native[0].transform.parent && skins[0].transform.parent == roots[0] &&
                roots[0].localPosition == Vector3.zero && roots[0].localRotation == Quaternion.identity && roots[0].localScale == Vector3.one &&
                skins[0].transform.localPosition == native[0].transform.localPosition &&
                Quaternion.Angle(skins[0].transform.localRotation, native[0].transform.localRotation) <= .0001f &&
                skins[0].transform.localScale == native[0].transform.localScale &&
                skins[0].sharedMesh != null && skins[0].sharedMesh.name == "KMG_Ettin_AuthoredBody" &&
                !ReferenceEquals(skins[0].sharedMesh, native[0].sharedMesh) &&
                skins[0].sharedMesh.subMeshCount == native[0].sharedMesh.subMeshCount && skins[0].sharedMaterials.Length == 1 &&
                resources.OfType<Mesh>().Count() == 1 && resources.OfType<Material>().Any() && resources.OfType<Texture>().Count() == 0 &&
                resources.OfType<Material>().All(value => value.name.StartsWith(EttinSecondHeadVisual.ObjectPrefix, StringComparison.Ordinal)) &&
                resources.Any(value => ReferenceEquals(value, skins[0].sharedMesh)) && ownedSlots && skinningExact;
            receipt["rootCount"] = roots.Length; receipt["nativePoseSourceHeadCount"] = heads.Length;
            receipt["privateHeadBranchCount"] = left == null || right == null ? 0 : 2;
            receipt["branchObservationError"] = branchError; receipt["privateSkinning"] = skinning;
            receipt["nativeSourceBodyRendererCount"] = native.Length;
            receipt["nativeSourceBodyDisabled"] = native.Length == 1 && !native[0].enabled;
            receipt["authoredRendererCount"] = skins.Length; receipt["exactTwoHeadFullBodyAttachment"] = shape;
            receipt["ownedMaterialAndBorrowedNativeTextureSlots"] = ownedSlots;
            receipt["ownedTextureCount"] = resources.OfType<Texture>().Count();
            receipt["attachmentInitializationInvoked"] = false;
            return shape && (bool?)receipt["exactDrivenBaselineAndSameNativeFade"] == true;
        }

        private static void Cleanup2SampleEttinMaterial(Cleanup2EttinHead head, bool expiry)
        {
            if (head.View == null || head.Renderer == null) return;
            JObject row = Cleanup2EttinMaterialControl(head.View);
            head.MaterialControlValid &= (bool?)row["exactDrivenBaselineAndSameNativeFade"] == true;
            head.MaximumFadeError = Mathf.Max(head.MaximumFadeError, (float)row["maximumHeadVsSourceDissolveError"]);
            row["frame"] = Time.frameCount; row["stage"] = expiry ? "native-summon-expiry" : head.Pose;
            if (expiry)
            {
                head.FadeSampleCount++;
                if ((int)row["nativeDissolveAnimationCount"] > 0) head.ActiveFadeSampleCount++;
                if (row["sourceLiveDissolve"].Type != JTokenType.Null)
                { float value = (float)row["sourceLiveDissolve"]; head.MinimumFade = Mathf.Min(head.MinimumFade, value); head.MaximumFade = Mathf.Max(head.MaximumFade, value); }
                if (head.FadeSamples.Count < 180) head.FadeSamples.Add(row);
            }
            else { head.MaterialSampleCount++; if (head.MaterialSamples.Count < 192 && Time.frameCount % 3 == 0) head.MaterialSamples.Add(row); }
        }

        private static void Cleanup2EnableEttinFadeObservation(Cleanup2EttinHead head)
        {
            if (head.Observer == null) head.Observer = head.Root.gameObject.AddComponent<SummonCleanup2EttinLateObserver>();
            head.Observer.Error = null;
            head.Observer.Observe = () => Cleanup2SampleEttinMaterial(head, true);
            head.Observer.enabled = true;
        }

        private static bool Cleanup2EttinControllerReleased(Cleanup2EttinHead head)
        {
            JObject receipt = EttinSecondHeadVisual.ObserveMaterialControl(head.View);
            return (bool?)receipt["attachmentPresent"] == true && (string)receipt["outcome"] == "released" &&
                (int?)receipt["assignedCount"] == 0 && (string)receipt["controllerCleanup"] == "reinitialized";
        }

        private void Cleanup2AssertEttinFadeRelease(Cleanup2EttinHead head)
        {
            // SummonedUnitBuff.OnRemoved (06002A64) delegates disappearance to
            // BloodPuddleFx/DismemberUnitFX and DismemberedUnitBuff. It does not
            // promise an SMC dissolve; positive view disposal and exact native
            // buff progression are required independently of sampled amount.
            bool disappeared = head.Actor.Destroyed && head.Actor.View == null && head.Actor.HoldingState == null;
            Cleanup2Check("ettin-native-expiry-and-controller-release-" + head.Actor.UniqueId,
                head.FadeSampleCount >= 2 && head.NativeDismemberFactObserved && disappeared &&
                    head.NativeMesh != null && head.BorrowedSurfaceTextures.All(value => ReferenceEquals(value, null) || value != null) &&
                    head.Resources.All(value => value == null) && head.MaterialControlValid && Cleanup2EttinControllerReleased(head) &&
                    (ReferenceEquals(head.Observer, null) || head.Observer.Error == null),
                new JObject { ["unit"] = head.Actor.UniqueId, ["sampleCount"] = head.FadeSampleCount,
                    ["activeNativeFadeSamples"] = head.ActiveFadeSampleCount, ["sourceDissolveRange"] = new JArray(head.MinimumFade, head.MaximumFade),
                    ["maximumHeadVsSourceDissolveError"] = head.MaximumFadeError, ["samples"] = head.FadeSamples.DeepClone(),
                    ["releaseControl"] = EttinSecondHeadVisual.ObserveMaterialControl(head.View),
                    ["nativeExpiry"] = head.NativeExpiry, ["nativeDismemberFactObserved"] = head.NativeDismemberFactObserved,
                    ["destroyed"] = head.Actor.Destroyed, ["viewGone"] = head.Actor.View == null,
                    ["holdingGone"] = head.Actor.HoldingState == null,
                    ["positiveSmcDissolveClaim"] = false,
                    ["forcedFadeOrVisibility"] = false, ["coldHydrationClaim"] = false },
                "Exact native marker expiry adds the native dismember fact and positively disposes the entity/view/holding state, with material/controller agreement through disappearance and all exact private allocations released. Native SMC dissolve is reported without substituting summon-in samples for expiry FX.");
        }

        private static JObject Cleanup2ExpireEttin(ExpandedSummoningCorrectionFixture fixture, Cleanup2EttinHead head)
        {
            if (!fixture.Created.Contains(head.Actor) || !ExpandedSummoningPlayerPathUnitExact(head.Actor, fixture.Caster))
                throw new InvalidOperationException("Exact positive Ettin ownership is required for native expiry.");
            Buff[] markers = head.Actor.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(value =>
                value.Blueprint == BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff).ToArray();
            if (markers.Length != 1) throw new InvalidOperationException("One exact native Ettin summon marker required.");
            var dismember = BlueprintRoot.Instance.SystemMechanics.DismemberedUnitBuff;
            int before = head.Actor.Descriptor.Buffs.RawFacts.OfType<Buff>().Count(value => ReferenceEquals(value.Blueprint, dismember));
            var effect = head.Actor.Blueprint.Visual.BloodPuddleFx;
            GameObject prefab = effect == null ? null : effect.Load(false);
            DismemberUnitFX carrier = prefab == null ? null : prefab.GetComponent<DismemberUnitFX>();
            JObject expiry = ExpireSprint16OwnedBuff(head.Actor, markers[0]);
            Buff[] after = head.Actor.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(value => ReferenceEquals(value.Blueprint, dismember)).ToArray();
            head.NativeDismemberFactObserved = before == 0 && after.Length == 1 &&
                !head.Actor.Descriptor.Buffs.RawFacts.OfType<Buff>().Any(value => ReferenceEquals(value, markers[0]));
            head.NativeExpiry = new JObject { ["scheduler"] = expiry,
                ["nativeOnRemovedToken"] = "06002A64", ["nativeFxInitToken"] = "06001074",
                ["effectLink"] = effect?.AssetId, ["prefabAvailable"] = prefab != null,
                ["prefabInstance"] = prefab == null ? 0 : prefab.GetInstanceID(), ["dismemberCarrierAvailable"] = carrier != null,
                ["nativeCarrierDelaySeconds"] = carrier == null ? 6f : carrier.Delay,
                ["dismemberBuff"] = dismember.AssetGuid,
                ["dismemberComponentTypes"] = new JArray(dismember.ComponentsArray.Take(32).Select(value => value?.GetType().FullName)),
                ["dismemberComponentCount"] = dismember.ComponentsArray.Length,
                ["dismemberComponentTypesTruncated"] = dismember.ComponentsArray.Length > 32,
                ["nativeFactAbsentBeforeExpiry"] = before == 0, ["nativeFactCountAfterTick"] = after.Length,
                ["nativeFactEndTimeTicks"] = after.Length == 1 ? (JToken)new JValue(after[0].EndTime.Ticks) : JValue.CreateNull(),
                ["spawnFxInvokedByObserver"] = false, ["smcDissolveRequiredByNativeMarker"] = false };
            return head.NativeExpiry;
        }

        private static JObject Cleanup2EttinAttackSelection(AttackHandInfo info)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            MethodInfo select = typeof(AttackHandInfo).GetMethod("GetAnimationType", flags);
            if (select == null) throw new MissingMethodException("AttackHandInfo.GetAnimationType");
            object[] args = { UnitAnimationType.None,
                Enum.ToObject(select.GetParameters()[1].ParameterType.GetElementType(), 0), 0, null };
            select.Invoke(info, args);
            var type = (UnitAnimationType)args[0];
            UnitAnimationManager manager = info.Hand.Owner.Unit.View.AnimationManager;
            MethodInfo get = typeof(UnitAnimationManager).GetMethod("GetAction", flags, null, new[] { typeof(UnitAnimationType) }, null);
            if (get == null) throw new MissingMethodException("UnitAnimationManager.GetAction");
            var action = get.Invoke(manager, new object[] { type }) as UnityEngine.Object;
            MethodInfo style = info.Weapon.GetType().GetMethod("GetAnimationStyle", flags, null, new[] { typeof(bool) }, null);
            if (style == null) throw new MissingMethodException("ItemEntityWeapon.GetAnimationStyle");
            return new JObject { ["selectedAnimationType"] = type.ToString(), ["selectedAnimationTypeValue"] = (int)type,
                ["selectedSpecialAttackType"] = Convert.ToString(args[1]), ["selectedSpecialAttackCount"] = Convert.ToInt32(args[2]),
                ["weaponAnimationStyleBeforeHandle"] = Convert.ToString(style.Invoke(info.Weapon, new object[] { false })),
                ["weapon"] = info.Weapon.Blueprint.AssetGuid, ["weaponName"] = info.Weapon.Blueprint.name,
                ["nativeActionAvailable"] = action != null, ["nativeActionInstance"] = action == null ? 0 : action.GetInstanceID(),
                ["nativeActionType"] = action == null ? null : action.GetType().FullName,
                ["nativeAnimationSet"] = manager.AnimationSet == null ? 0 : manager.AnimationSet.GetInstanceID(),
                ["getAnimationTypeToken"] = "06002659", ["createHandleToken"] = "0600160C",
                ["nativeActionOrClipSupplied"] = false };
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
