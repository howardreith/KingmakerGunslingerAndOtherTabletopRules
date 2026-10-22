using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.View;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Kingmaker;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private sealed class Cleanup2SharedViewReceipt
        {
            internal UnitEntityView View;
            internal SkinnedMeshRenderer[] Skins;
            internal Mesh[] Meshes;
            internal Material[][] Materials;
            internal Transform[][] Bones;
            internal Transform[] RootBones;
            internal bool[] Enabled;
            internal UnitAnimationManager Manager;
            internal bool PrefabAsset;
            internal UnityEngine.Object AnimationSet;
            internal object ActionList;
            internal UnityEngine.Object[] Actions;
            internal Vector3 Scale;
            internal JObject Scalars;
        }

        // This observer has no save/load, rule/stat writes, movement command or
        // animation stimulus. The native control is an exact new scene spawn;
        // only the released Triceratops is a canonical parent/slot summon.
        private IEnumerable<int> ReviewSummonCleanup2SharedControls(
            ExpandedSummoningCorrectionFixture fixture, Spellbook book)
        {
            if (!IsSummonCleanup2Review || !_request.ExitAfterCompletion ||
                (string)_request.Parameters?["reviewPhase"] != "shared-controls" ||
                fixture == null || book == null || _sprint25Clock == null || fixture.Scene == null)
                throw new InvalidOperationException("Exact save-free shared-controls fixture/book/clock required.");
            if (_expandedSummoningPlayerPathCaptureActive || _refinementUnit != null || _refinementAction != null)
                throw new InvalidOperationException("Another review owns cast/refinement state.");

            BlueprintUnit native = fixture.Blueprints.OfType<BlueprintUnit>().SingleOrDefault(value =>
                value.AssetGuid == GiantBodyVisualPolicy.DonorGuid);
            bool exactSource = native != null &&
                ReferenceEquals(native, BlueprintLibraryLookup.RequireExact<BlueprintUnit>(BlueprintBootstrap.Library,
                    GiantBodyVisualPolicy.DonorGuid, "shared-controls native Mastodon")) &&
                native.Prefab != null && native.Prefab.AssetId == GiantBodyVisualPolicy.DonorPrefabGuid &&
                ArmoredDinosaurVisualPolicy.TriceratopsDonorGuid == GiantBodyVisualPolicy.DonorGuid;
            Cleanup2Check("shared-controls-native-source", exactSource, new JObject {
                ["nativeUnit"] = native?.AssetGuid, ["nativeName"] = native?.name,
                ["prefab"] = native?.Prefab?.AssetId, ["expectedUnit"] = GiantBodyVisualPolicy.DonorGuid,
                ["expectedPrefab"] = GiantBodyVisualPolicy.DonorPrefabGuid,
                ["scope"] = "Exact existing installed native donor; no cloned/renamed project blueprint, debug substitute, save or trample setup." },
                "The existing Mastodon donor and the Triceratops donor contract resolve to the same exact native blueprint/prefab.");
            if (!exactSource) yield break;

            UnitEntityView sourceView = null;
            string sourceFailure = null;
            try { sourceView = native.Prefab.Load(false); }
            catch (Exception error) { sourceFailure = error.GetType().Name + ":" + error.Message; }
            UnitAnimationManager sourceManager;
            AnimationSet sourceSet;
            JObject sourceAnimation;
            bool loaded = Cleanup2SharedAnimation(sourceView, true, out sourceManager, out sourceSet, out sourceAnimation);
            Cleanup2Check("shared-controls-native-prefab", loaded, new JObject {
                ["prefab"] = native.Prefab.AssetId, ["loadedView"] = sourceView == null ? 0 : sourceView.GetInstanceID(),
                ["failure"] = sourceFailure, ["animationCensus"] = sourceAnimation,
                ["instantiated"] = false, ["modified"] = false },
                "The exact native prefab loads read-only before spawning either control.");
            if (!loaded) yield break;

            object sourcePrefabLink = native.Prefab;
            Cleanup2SharedViewReceipt source = Cleanup2SharedTakeView(sourceView, true);
            bool supported = source.Skins.Length == 1 && source.Skins[0].name == GiantBodyVisualPolicy.BodyRendererName &&
                source.Meshes[0] != null && source.Meshes.All(mesh => !mesh.name.StartsWith("KMG_", StringComparison.Ordinal));
            Cleanup2Check("shared-controls-source-assets-before", supported, (JObject)source.Scalars.DeepClone(),
                "The measured native Mastodon prefab has its original single native skin, material/action references and no project body.");
            if (!supported) yield break;

            Cleanup2Route route = Cleanup2GiantRoute(fixture, ArmoredDinosaurRulesPolicy.TriceratopsKey, SummonFamily.NaturesAlly);
            bool routeExact = route.Unit.name == ArmoredDinosaurVisualPolicy.TriceratopsBlueprintName &&
                route.Unit.Prefab.AssetId == native.Prefab.AssetId;
            Cleanup2Check("shared-controls-triceratops-route", routeExact, new JObject {
                ["key"] = route.Key, ["family"] = route.Family.ToString(), ["tier"] = route.Tier,
                ["parent"] = route.Parent.AssetGuid, ["selected"] = route.Selected.AssetGuid,
                ["unit"] = route.Unit.AssetGuid, ["prefab"] = route.Unit.Prefab.AssetId,
                ["spellbook"] = book.Blueprint.AssetGuid, ["caster"] = fixture.Caster.UniqueId,
                ["accessLimit"] = "The disposable Human caster's fixture-granted prepared spellbook access is not a class availability or natural input timing test." },
                "The unchanged released Triceratops uses its actual SNA own-tier single canonical parent/prepared-slot child and shared native prefab.");
            if (!routeExact) yield break;
            object projectPrefabLink = route.Unit.Prefab;

            bool paused = Game.Instance.IsPaused, waiting = _sprint25WaitingForUnityAppearance;
            var oldCapture = ExpandedSummoningRuleCapture.ToArray();
            var oldEvents = ExpandedSummoningPlayerPathEvents.ToArray();
            int oldCasts = _expandedSummoningPlayerPathRuleCastCount, oldSpawns = _expandedSummoningPlayerPathSpawnActionCount;
            var hooks = HarmonyInstance.Create("kmg.cleanup2.shared-controls." + _request.RunId);
            var owned = new List<UnitEntityData>();
            var brains = new Dictionary<UnitEntityData, bool>();
            var privateResources = new List<UnityEngine.Object>();
            var watch = Stopwatch.StartNew();
            UnitEntityData mastodon = null, triceratops = null;
            Cleanup2SharedViewReceipt nativeBefore = null, projectBefore = null;
            JObject nativeStats = null, projectStats = null;
            int pulsesBefore = _sprint25Clock.Pulses;
            double advancedBefore = _sprint25Clock.AdvancedSeconds;
            const BindingFlags statics = BindingFlags.Static | BindingFlags.NonPublic;
            try
            {
                hooks.Patch(typeof(RuleCastSpell).GetMethod("OnTrigger"), null,
                    new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("ExpandedSummoningPlayerPathRuleCastPostfix", statics)), null);
                hooks.Patch(typeof(ContextActionSpawnMonster).GetMethod("RunAction"),
                    new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("ExpandedSummoningPlayerPathSpawnPrefix", statics)), null, null);
                _expandedSummoningPlayerPathCaptureActive = true;
                Game.Instance.IsPaused = false;

                // This exact EntityCreator return is the creation receipt, not
                // an arbitrary before/after scene difference or name match.
                mastodon = Game.Instance.EntityCreator.SpawnUnit(native, fixture.Caster.Position,
                    Quaternion.identity, fixture.Scene);
                bool fresh = mastodon != null && ReferenceEquals(mastodon.Blueprint, native) &&
                    !fixture.UnitsBefore.Any(value => ReferenceEquals(value, mastodon)) &&
                    !fixture.ExactStart.Any(value => ReferenceEquals(value, mastodon)) &&
                    !fixture.PartyBefore.Any(value => ReferenceEquals(value, mastodon)) &&
                    !fixture.Created.Contains(mastodon) && !ReferenceEquals(mastodon, fixture.Caster) &&
                    !ReferenceEquals(mastodon, fixture.Hostile);
                if (fresh)
                {
                    fixture.Created.Add(mastodon); owned.Add(mastodon);
                    brains.Add(mastodon, mastodon.IsBrainActive);
                    SetExpandedSummoningBrainActive(mastodon, false);
                }
                Game.Instance.EntityCreator.Tick();
                bool nativeOwned = fresh && mastodon.IsInState && ReferenceEquals(mastodon.HoldingState, fixture.Scene) &&
                    mastodon.View != null && ReferenceEquals(mastodon.View.Data, mastodon);
                Cleanup2Check("shared-controls-native-creation-receipt", nativeOwned, new JObject {
                    ["unit"] = mastodon?.UniqueId, ["newExactReturn"] = fresh,
                    ["blueprint"] = mastodon?.Blueprint?.AssetGuid,
                    ["sameRequestedScene"] = mastodon != null && ReferenceEquals(mastodon.HoldingState, fixture.Scene),
                    ["isInState"] = mastodon != null && mastodon.IsInState,
                    ["registeredExactPositiveActor"] = fresh && fixture.Created.Contains(mastodon),
                    ["summonRouteClaim"] = false, ["nativeControlCreation"] = "Qualified Sprint27 direct native EntityCreator.SpawnUnit into the existing disposable fixture.Scene; native blueprint/faction/stats untouched." },
                    "Only the exact fresh native SpawnUnit return in the requested scene is adopted for cleanup.");
                if (!nativeOwned) throw new InvalidOperationException("Native Mastodon safe creation receipt refused.");
                foreach (int frame in Cleanup2WaitAppearance(mastodon, "shared-native-mastodon"))
                { Cleanup2SharedDeadline(watch); yield return frame; }
                nativeBefore = Cleanup2SharedTakeView(mastodon.View);
                nativeStats = Cleanup2SharedStats(mastodon);
                bool nativeBody = nativeBefore.Meshes.SequenceEqual(source.Meshes) &&
                    ReferenceEquals(nativeBefore.AnimationSet, source.AnimationSet) &&
                    ExpandedSummoningPteranodonViewPatch.CaptureArmoredDinosaurOwnedResources(mastodon.View).Length == 0 &&
                    !ExpandedSummoningPteranodonViewPatch.DescribeView(mastodon.View).StartsWith("visual:attached;", StringComparison.Ordinal);
                Cleanup2Check("shared-controls-native-body", nativeBody, new JObject {
                    ["unit"] = mastodon.UniqueId, ["view"] = nativeBefore.Scalars,
                    ["statsAndResources"] = nativeStats, ["attachment"] = ExpandedSummoningPteranodonViewPatch.DescribeView(mastodon.View),
                    ["sameNativeMeshReferences"] = nativeBefore.Meshes.SequenceEqual(source.Meshes),
                    ["sameNativeAnimationSet"] = ReferenceEquals(nativeBefore.AnimationSet, source.AnimationSet),
                    ["selection"] = Sprint22SelectionSurface(mastodon) },
                    "The native Mastodon retains the untouched native body and animation set, with no authored-body receipt.");
                if (!nativeBody) throw new InvalidOperationException("Native Mastodon acquired a project or unexpected body.");
                _sprint25WaitingForUnityAppearance = true;
                for (int frame = 0; frame < 12; frame++) { Cleanup2SharedDeadline(watch); yield return 0; }
                Cleanup2SharedPictures(mastodon, "native-mastodon", watch);

                triceratops = Cleanup2CastGiant(fixture, book, route);
                owned.Add(triceratops); brains.Add(triceratops, triceratops.IsBrainActive);
                foreach (int frame in Cleanup2WaitAppearance(triceratops, "shared-triceratops"))
                { Cleanup2SharedDeadline(watch); yield return frame; }
                Transform[] borrowedBones; Matrix4x4[] borrowedBinds; Mesh borrowedMesh;
                UnityEngine.Object[] resources = ExpandedSummoningPteranodonViewPatch.CaptureArmoredDinosaurOwnedResources(triceratops.View);
                privateResources.AddRange(resources);
                bool attached = ExpandedSummoningPteranodonViewPatch.DescribeView(triceratops.View).StartsWith("visual:attached;", StringComparison.Ordinal) &&
                    ExpandedSummoningPteranodonViewPatch.TryGetDonorRig(triceratops.View, out borrowedBones, out borrowedBinds, out borrowedMesh) &&
                    source.Meshes.Contains(borrowedMesh) && resources.OfType<Mesh>().Count() == 1 && resources.OfType<Material>().Any();
                projectBefore = Cleanup2SharedTakeView(triceratops.View);
                projectStats = Cleanup2SharedStats(triceratops);
                attached &= projectBefore.Meshes.Any(mesh => resources.Contains(mesh)) &&
                    ReferenceEquals(projectBefore.AnimationSet, source.AnimationSet);
                Cleanup2Check("shared-controls-triceratops-body", attached, new JObject {
                    ["unit"] = triceratops.UniqueId, ["view"] = projectBefore.Scalars,
                    ["statsAndResources"] = projectStats, ["attachment"] = ExpandedSummoningPteranodonViewPatch.DescribeView(triceratops.View),
                    ["privateResources"] = new JArray(resources.Select(Cleanup2SharedObject)),
                    ["selection"] = Sprint22SelectionSurface(triceratops),
                    ["scope"] = "Released Triceratops authored-body attachment and native borrowed source receipt; no new Triceratops art is supplied by this observer." },
                    "The actual canonical Triceratops owns its approved body while retaining the exact native Mastodon source/action set.");
                if (!attached) throw new InvalidOperationException("Released Triceratops attachment/source receipt refused.");

                // Retain the untouched native actor through project attachment,
                // then destroy only that positive control before the project
                // images so two large co-located bodies cannot mask each other.
                bool isolation = Cleanup2SharedSourceExact(native, sourceView, sourcePrefabLink) &&
                    Cleanup2SharedSameView(source) && Cleanup2SharedSameView(nativeBefore) &&
                    JToken.DeepEquals(nativeStats, Cleanup2SharedStats(mastodon));
                Cleanup2Check("shared-controls-native-retained-through-attachment", isolation, new JObject {
                    ["nativeUnit"] = mastodon.UniqueId, ["projectUnit"] = triceratops.UniqueId,
                    ["sourceAssetsUnchanged"] = Cleanup2SharedSameView(source),
                    ["sourceBlueprintPrefabAndLoadedViewExact"] = Cleanup2SharedSourceExact(native, sourceView, sourcePrefabLink),
                    ["nativeActorAssetsUnchanged"] = Cleanup2SharedSameView(nativeBefore),
                    ["nativeStatsAndResourcesAfter"] = Cleanup2SharedStats(mastodon),
                    ["coexistence"] = "Both exact scene-owned actors are alive while this assertion is measured; no movement/combat or collision-response claim." },
                    "Attaching the released Triceratops leaves the live native Mastodon's original references, selected stats/resources and root scale unchanged.");
                if (!isolation) throw new InvalidOperationException("Shared native donor/control changed during Triceratops attachment.");
                mastodon.Commands.InterruptAll(true);
                DisposeExpandedSummoningUnits(fixture.Created, new[] { mastodon });
                _sprint25WaitingForUnityAppearance = true;
                for (int frame = 0; frame < 12; frame++) { Cleanup2SharedDeadline(watch); yield return 0; }
                Cleanup2SharedPictures(triceratops, "released-triceratops", watch);
                bool final = Cleanup2SharedSourceExact(native, sourceView, sourcePrefabLink) &&
                    ReferenceEquals(route.Unit.Prefab, projectPrefabLink) && Cleanup2SharedSameView(source) && Cleanup2SharedSameView(projectBefore) &&
                    JToken.DeepEquals(projectStats, Cleanup2SharedStats(triceratops));
                Cleanup2Check("shared-controls-protected-references-after", final, new JObject {
                    ["sourceAssetsAfter"] = Cleanup2SharedViewScalars(sourceView, true),
                    ["projectActorAfter"] = Cleanup2SharedViewScalars(triceratops.View),
                    ["projectStatsAndResourcesAfter"] = Cleanup2SharedStats(triceratops),
                    ["sourceBlueprintPrefabAndLoadedViewExact"] = Cleanup2SharedSourceExact(native, sourceView, sourcePrefabLink),
                    ["projectPrefabLinkExact"] = ReferenceEquals(route.Unit.Prefab, projectPrefabLink),
                    ["wallSeconds"] = watch.Elapsed.TotalSeconds,
                    ["qualifiedNativeClockPulses"] = _sprint25Clock.Pulses - pulsesBefore,
                    ["syntheticGameSecondsAdvanced"] = _sprint25Clock.AdvancedSeconds - advancedBefore,
                    ["limits"] = "Six real native normal-visibility stills support pixel review only. No travel, attack/hit, timing, trample, save persistence, full art packet, selection click or owner visual acceptance is claimed. Appearance may use the existing qualified synthetic fixture clock; idle images suspend its pulses." },
                    "Source assets and the released Triceratops retain their exact mesh/material/bone/action references, selected state and root scale after observation.");
                if (!final) throw new InvalidOperationException("Protected shared-control references/state changed.");
            }
            finally
            {
                var failures = new List<string>();
                Action<string, Action> restore = (label, action) => { try { action(); } catch (Exception error) { failures.Add(label + ":" + error.GetType().Name + ":" + error.Message); } };
                _expandedSummoningPlayerPathCaptureActive = false;
                restore("capture-hooks", () => hooks.UnpatchAll(hooks.Id));
                restore("capture-state", () => {
                    ExpandedSummoningRuleCapture.Clear(); ExpandedSummoningRuleCapture.AddRange(oldCapture);
                    ExpandedSummoningPlayerPathEvents.Clear(); ExpandedSummoningPlayerPathEvents.AddRange(oldEvents);
                    _expandedSummoningPlayerPathRuleCastCount = oldCasts; _expandedSummoningPlayerPathSpawnActionCount = oldSpawns;
                });
                foreach (UnitEntityData actor in owned)
                    restore("positive-actor-" + actor.UniqueId, () => {
                        if (!actor.Destroyed) { actor.Commands.InterruptAll(true); SetExpandedSummoningBrainActive(actor, brains[actor]); }
                        if (fixture.Created.Contains(actor)) DisposeExpandedSummoningUnits(fixture.Created, new[] { actor });
                    });
                _sprint25WaitingForUnityAppearance = waiting;
                restore("pause", () => Game.Instance.IsPaused = paused);
                Cleanup2Check("shared-controls-owned-state-restored", failures.Count == 0, new JObject {
                    ["failures"] = new JArray(failures), ["positiveActors"] = new JArray(owned.Select(actor => actor.UniqueId)),
                    ["stillRegisteredForSharedTeardown"] = new JArray(owned.Where(fixture.Created.Contains).Select(actor => actor.UniqueId)),
                    ["foreignActorsAdopted"] = false, ["saveAccess"] = false },
                    "Only the two positive scene-owned controls, this request's hooks/capture state, brain flags and pause/wait fields are cleaned or restored.");
            }

            // Bounded exact-reference destruction drain, never a global object
            // count or a claim that one delayed Unity Destroy is a leak.
            var drain = Stopwatch.StartNew();
            for (int frame = 0; frame < 120 && drain.Elapsed.TotalSeconds < 3 && watch.Elapsed.TotalSeconds < 120 &&
                (owned.Any(actor => !actor.Destroyed || actor.View != null || actor.HoldingState != null) || privateResources.Any(value => value != null)); frame++)
            { Game.Instance.EntityDestroyer.Tick(); yield return 0; }
            bool released = owned.All(actor => actor.Destroyed && actor.View == null && actor.HoldingState == null && !fixture.Created.Contains(actor)) &&
                privateResources.All(value => value == null) && Cleanup2SharedSameView(source) &&
                Cleanup2SharedSourceExact(native, sourceView, sourcePrefabLink) && ReferenceEquals(route.Unit.Prefab, projectPrefabLink);
            Cleanup2Check("shared-controls-exact-cleanup", released, new JObject {
                ["actors"] = new JArray(owned.Select(actor => new JObject { ["unit"] = actor.UniqueId, ["destroyed"] = actor.Destroyed,
                    ["viewGone"] = actor.View == null, ["holdingGone"] = actor.HoldingState == null,
                    ["stillRegistered"] = fixture.Created.Contains(actor) })),
                ["privateResourceReferences"] = privateResources.Count, ["privateReferencesStillAlive"] = privateResources.Count(value => value != null),
                ["sourceAssetsStillExact"] = Cleanup2SharedSameView(source), ["drainSeconds"] = drain.Elapsed.TotalSeconds,
                ["wallSeconds"] = watch.Elapsed.TotalSeconds, ["maximumWallSeconds"] = 120,
                ["remainingReferenceMeaning"] = "Unresolved bounded release if any exact owned reference survives; no unbounded global leak claim." },
                "Both exact controls and captured private Triceratops resources are released; borrowed native prefab assets remain unchanged.");
        }

        private static void Cleanup2SharedDeadline(Stopwatch watch)
        { if (watch.Elapsed.TotalSeconds >= 120) throw new TimeoutException("Shared-controls 120-second wall bound."); }

        private static bool Cleanup2SharedSourceExact(BlueprintUnit native, UnitEntityView loaded, object prefabLink)
        {
            return native != null && ReferenceEquals(native.Prefab, prefabLink) &&
                native.Prefab.AssetId == GiantBodyVisualPolicy.DonorPrefabGuid &&
                ReferenceEquals(native, BlueprintLibraryLookup.RequireExact<BlueprintUnit>(BlueprintBootstrap.Library,
                    GiantBodyVisualPolicy.DonorGuid, "shared-controls protected native Mastodon")) &&
                ReferenceEquals(native.Prefab.Load(false), loaded);
        }

        // Native UnitEntityView caches its manager during live initialization.
        // A loaded prefab has serialized child components but no such cache.
        // AnimationManager.AnimationSet also falls back to the Human set: read
        // the exact serialized field instead, never initialize a prefab.
        private static readonly FieldInfo Cleanup2SerializedAnimationSet = typeof(AnimationManager).GetField(
            "m_AnimationSet", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo Cleanup2CachedAnimationManager = typeof(UnitEntityView).GetField(
            "m_AnimatorManager", BindingFlags.Instance | BindingFlags.NonPublic);

        private static bool Cleanup2SharedAnimation(UnitEntityView view, bool prefab,
            out UnitAnimationManager manager, out AnimationSet set, out JObject census)
        {
            manager = null; set = null;
            var candidates = view == null ? new UnitAnimationManager[0] :
                view.GetComponentsInChildren<UnitAnimationManager>(true);
            var cached = view == null || Cleanup2CachedAnimationManager == null ? null :
                Cleanup2CachedAnimationManager.GetValue(view) as UnitAnimationManager;
            bool contract = Cleanup2SerializedAnimationSet != null &&
                Cleanup2SerializedAnimationSet.FieldType == typeof(AnimationSet) &&
                Cleanup2CachedAnimationManager != null &&
                Cleanup2CachedAnimationManager.FieldType == typeof(UnitAnimationManager);
            census = new JObject { ["prefabAsset"] = prefab, ["candidateCount"] = candidates.Length,
                ["cachedViewManager"] = Cleanup2SharedObject(cached), ["fieldContract"] = contract,
                ["candidates"] = new JArray(candidates.Take(8).Select(candidate => new JObject {
                    ["manager"] = Cleanup2SharedObject(candidate),
                    ["path"] = Cleanup2TransformPath(view.transform, candidate.transform),
                    ["serializedSet"] = contract ? Cleanup2SharedObject(
                        Cleanup2SerializedAnimationSet.GetValue(candidate) as AnimationSet) : JValue.CreateNull() })),
                ["initializationInvoked"] = false, ["humanSetFallbackUsed"] = false };
            if (!contract || candidates.Length != 1 || candidates[0] == null) return false;
            manager = candidates[0]; set = Cleanup2SerializedAnimationSet.GetValue(manager) as AnimationSet;
            // The live getter is allowed only on the already initialized owned
            // actor. It must agree with the exact serialized child component.
            bool liveCache = prefab || ReferenceEquals(view.AnimationManager, manager);
            census["liveCacheAgrees"] = prefab ? JValue.CreateNull() : new JValue(liveCache);
            census["serializedSetPresent"] = set != null;
            census["actionCount"] = set == null || set.Actions == null ? -1 : set.Actions.Count;
            return liveCache && set != null && set.Actions != null && set.Actions.Count > 0 && set.Actions.Count <= 128;
        }

        private static Cleanup2SharedViewReceipt Cleanup2SharedTakeView(UnitEntityView view, bool prefab = false)
        {
            UnitAnimationManager manager; AnimationSet set; JObject census;
            if (!Cleanup2SharedAnimation(view, prefab, out manager, out set, out census))
                throw new NotSupportedException("Shared control has no exact serialized child manager/action set: " + census.ToString(Newtonsoft.Json.Formatting.None));
            var skins = view.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            if (skins.Length == 0 || skins.Length > 16 ||
                set.Actions.OfType<UnitAnimationAction>().Any(action => action != null && action.Clips != null && action.Clips.Take(65).Count() > 64))
                throw new NotSupportedException("Shared control skin/action census outside bounded native contract.");
            return new Cleanup2SharedViewReceipt { View = view, Skins = skins,
                Meshes = skins.Select(value => value.sharedMesh).ToArray(),
                Materials = skins.Select(value => value.sharedMaterials).ToArray(),
                Bones = skins.Select(value => value.bones).ToArray(), RootBones = skins.Select(value => value.rootBone).ToArray(),
                Enabled = skins.Select(value => value.enabled).ToArray(), Scale = view.transform.localScale,
                Manager = manager, PrefabAsset = prefab,
                AnimationSet = set, ActionList = set.Actions,
                Actions = set.Actions.Cast<UnityEngine.Object>().ToArray(),
                Scalars = Cleanup2SharedViewScalars(view, prefab) };
        }

        private static bool Cleanup2SharedSameView(Cleanup2SharedViewReceipt before)
        {
            UnitAnimationManager manager; AnimationSet set; JObject census;
            if (before == null || before.View == null ||
                !Cleanup2SharedAnimation(before.View, before.PrefabAsset, out manager, out set, out census) ||
                !ReferenceEquals(manager, before.Manager) || !ReferenceEquals(set, before.AnimationSet) ||
                !ReferenceEquals(set.Actions, before.ActionList) ||
                !set.Actions.Cast<UnityEngine.Object>().SequenceEqual(before.Actions) ||
                before.View.transform.localScale != before.Scale ||
                !before.View.GetComponentsInChildren<SkinnedMeshRenderer>(true).SequenceEqual(before.Skins)) return false;
            for (int index = 0; index < before.Skins.Length; index++)
            {
                SkinnedMeshRenderer skin = before.Skins[index];
                if (skin == null || !ReferenceEquals(skin.sharedMesh, before.Meshes[index]) ||
                    !skin.sharedMaterials.SequenceEqual(before.Materials[index]) || !skin.bones.SequenceEqual(before.Bones[index]) ||
                    !ReferenceEquals(skin.rootBone, before.RootBones[index]) || skin.enabled != before.Enabled[index]) return false;
            }
            return JToken.DeepEquals(before.Scalars, Cleanup2SharedViewScalars(before.View, before.PrefabAsset));
        }

        private static JObject Cleanup2SharedViewScalars(UnitEntityView view, bool prefab = false)
        {
            UnitAnimationManager manager; AnimationSet set; JObject census;
            if (!Cleanup2SharedAnimation(view, prefab, out manager, out set, out census))
                throw new NotSupportedException("Shared control animation census refused: " + census.ToString(Newtonsoft.Json.Formatting.None));
            return new JObject { ["view"] = view.GetInstanceID(), ["rootLocalScale"] = SurveyVector(view.transform.localScale),
                ["animationCensus"] = census, ["animationSet"] = Cleanup2SharedObject(set),
                ["actions"] = new JArray(set.Actions.OfType<UnitAnimationAction>().Take(128).Select(action => new JObject {
                    ["action"] = Cleanup2SharedObject(action), ["type"] = action.Type.ToString(),
                    ["clips"] = new JArray((action.Clips ?? Enumerable.Empty<AnimationClip>()).Take(64).Select(clip =>
                        clip == null ? (JToken)JValue.CreateNull() : new JObject { ["clip"] = Cleanup2SharedObject(clip), ["length"] = clip.length })) })),
                ["skins"] = new JArray(view.GetComponentsInChildren<SkinnedMeshRenderer>(true).Select(skin => new JObject {
                    ["renderer"] = Cleanup2SharedObject(skin), ["path"] = Cleanup2TransformPath(view.transform, skin.transform),
                    ["enabled"] = skin.enabled, ["active"] = skin.gameObject.activeInHierarchy,
                    ["mesh"] = Cleanup2SharedObject(skin.sharedMesh), ["vertexCount"] = skin.sharedMesh == null ? 0 : skin.sharedMesh.vertexCount,
                    ["submeshes"] = skin.sharedMesh == null ? 0 : skin.sharedMesh.subMeshCount,
                    ["bones"] = new JArray(skin.bones.Select(Cleanup2SharedObject)), ["rootBone"] = Cleanup2SharedObject(skin.rootBone),
                    ["materials"] = new JArray(skin.sharedMaterials.Select(material => {
                        JToken value = Cleanup2Material(material);
                        // Dissolve is native presentation progress, not immutable
                        // asset state. Settled visibility is checked separately.
                        var properties = material == null ? null : value["properties"] as JObject;
                        properties?.Remove("_Dissolve"); properties?.Remove("_DissolveAmount");
                        return value;
                    })) })),
                ["dataLimit"] = "Object identities and selected scalar properties only; no mesh vertices/indices/weights, bind matrices, texture pixels or native asset export." };
        }

        private static JToken Cleanup2SharedObject(UnityEngine.Object value)
        { return value == null ? (JToken)JValue.CreateNull() : new JObject { ["instance"] = value.GetInstanceID(), ["name"] = value.name, ["class"] = value.GetType().FullName }; }

        private static JObject Cleanup2SharedStats(UnitEntityData actor)
        {
            var resources = new List<BlueprintScriptableObject>();
            foreach (BlueprintScriptableObject resource in actor.Descriptor.Resources) resources.Add(resource);
            return new JObject { ["unit"] = actor.UniqueId, ["blueprint"] = actor.Blueprint.AssetGuid,
                ["size"] = actor.Descriptor.State.Size.ToString(), ["corpulence"] = actor.Corpulence,
                ["viewCorpulence"] = actor.View.Corpulence, ["agentCorpulence"] = actor.View.MovementAgent?.Corpulence,
                ["strength"] = actor.Stats.Strength.ModifiedValue, ["dexterity"] = actor.Stats.Dexterity.ModifiedValue,
                ["constitution"] = actor.Stats.Constitution.ModifiedValue, ["intelligence"] = actor.Stats.Intelligence.ModifiedValue,
                ["wisdom"] = actor.Stats.Wisdom.ModifiedValue, ["charisma"] = actor.Stats.Charisma.ModifiedValue,
                ["maximumHP"] = actor.Stats.HitPoints.ModifiedValue, ["damage"] = actor.Descriptor.Damage,
                ["AC"] = actor.Stats.AC.ModifiedValue, ["BAB"] = actor.Stats.BaseAttackBonus.ModifiedValue,
                ["speed"] = actor.Stats.Speed.ModifiedValue, ["fortitude"] = actor.Stats.SaveFortitude.ModifiedValue,
                ["reflex"] = actor.Stats.SaveReflex.ModifiedValue, ["will"] = actor.Stats.SaveWill.ModifiedValue,
                ["resources"] = new JArray(resources.OrderBy(value => value.AssetGuid, StringComparer.Ordinal)
                    .Select(value => new JObject { ["blueprint"] = value.AssetGuid, ["amount"] = actor.Descriptor.Resources.GetResourceAmount(value) })),
                ["primaryWeapon"] = actor.Body.PrimaryHand?.MaybeWeapon?.Blueprint.AssetGuid,
                ["secondaryWeapon"] = actor.Body.SecondaryHand?.MaybeWeapon?.Blueprint.AssetGuid };
        }

        private void Cleanup2SharedPictures(UnitEntityData actor, string label, Stopwatch watch)
        {
            Camera camera = TeleportationCastingCamera()?.Camera ?? Camera.main;
            if (camera == null) throw new InvalidOperationException("Shared-controls native camera unavailable.");
            SkinnedMeshRenderer[] visible = actor.View.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .Where(value => value.enabled && value.gameObject.activeInHierarchy && value.sharedMesh != null).ToArray();
            if (visible.Length == 0) throw new InvalidOperationException("Shared-controls has no normally visible body.");
            Bounds prior = _refinementBounds;
            try
            {
                _refinementBounds = visible[0].bounds;
                foreach (var skin in visible.Skip(1)) _refinementBounds.Encapsulate(skin.bounds);
                float radius = Mathf.Max(.5f, _refinementBounds.extents.magnitude);
                float distance = radius / Mathf.Tan(camera.fieldOfView * .5f * Mathf.Deg2Rad) * 1.2f;
                if (!Cleanup2Finite(_refinementBounds.center) || !Cleanup2Finite(distance) || distance <= 0)
                    throw new InvalidOperationException("Shared-controls framing is non-finite.");
                string[] views = { "front", "side", "rear" }; float[] yaws = { 0f, -90f, 180f };
                for (int index = 0; index < views.Length; index++)
                {
                    Cleanup2SharedDeadline(watch);
                    Vector3 toward = Quaternion.Euler(0, yaws[index], 0) * RefinementTurn(actor, 0);
                    Vector3 eye = _refinementBounds.center + (toward * Mathf.Cos(18f * Mathf.Deg2Rad) + Vector3.up * Mathf.Sin(18f * Mathf.Deg2Rad)) * distance;
                    string file = "cleanup2-shared-" + label + "-" + views[index] + ".png";
                    if (File.Exists(Path.Combine(_request.EvidenceDirectory, file))) throw new InvalidOperationException("Shared-controls PNG cannot be overwritten.");
                    bool normal = EntityFadedIn(actor) && DissolveAmount(actor) >= 0 && DissolveAmount(actor) <= .02f;
                    string result = normal ? WriteRefinementImage(eye, Quaternion.LookRotation(_refinementBounds.center - eye, Vector3.up), file) : "refused:not-normally-visible";
                    bool saved = normal && result.StartsWith("png=" + file + ";", StringComparison.Ordinal);
                    Cleanup2Check("shared-controls-picture-" + label + "-" + views[index], saved, new JObject {
                        ["unit"] = actor.UniqueId, ["file"] = file, ["result"] = result, ["view"] = views[index],
                        ["cameraWorld"] = SurveyVector(eye), ["targetWorld"] = SurveyVector(_refinementBounds.center),
                        ["fadedIn"] = EntityFadedIn(actor), ["dissolve"] = DissolveAmount(actor),
                        ["framing"] = "Renderer bounds frame the native image only; never anatomy or a mechanical measurement.",
                        ["visibilityForced"] = false, ["ownerVisualApproval"] = false },
                        "One actual front/side/rear native image is captured with normal settled materials; image pixels require review.");
                    if (!saved) throw new InvalidOperationException("Shared-controls native picture refused: " + result);
                }
            }
            finally { _refinementBounds = prior; }
        }
    }
}
